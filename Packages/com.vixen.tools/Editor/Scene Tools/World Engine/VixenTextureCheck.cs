#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using UnityEditor;
using UnityEngine;

namespace VixenTools.Editor
{
    public static class VixenTextureCheck
    {
        public const string SRGBAttribute = "VixForgesRGBWarning";
        public const string BentNormalProp = "_BentNormalMap";
        public const int BentNormalCap = 1024;
        public const int SmallTextureSide = 256;
        public const long LooseSourceBytes = 15L * 1024 * 1024;
        public const string ConvertedRoot = "Assets/VixenTools/Converted";

        private const string DesktopPlatform = "Standalone";
        private static readonly string[] OverridePlatforms = { "Standalone", "Android", "iPhone" };
        private static readonly string[] LooseSourceExtensions = { ".tif", ".tiff", ".tga", ".bmp" };
        private static readonly string[] PngExcludedChunks = { "bKGD", "cHRM", "EXIF", "gAMA", "iCCP", "iTXt", "sRGB", "tEXt", "zCCP", "zTXt", "date" };

        public enum Issue { ReadWrite, Uncompressed, Oversize, LowFrequency, NormalFormat, NormalType, NoMipmaps, ColorSpace, Conflict, SourceFormat }

        public enum Effect { Saves, Neutral, Costs, None }

        public class Slot
        {
            public Material Material;
            public string Property;
            public string Label;
            public bool IsNormal;
            public string SRGB;
        }

        public class Finding
        {
            public Texture Texture;
            public string Path;
            public Issue Kind;
            public Effect MemoryEffect;
            public string Title;
            public string Description;
            public int TargetSize;
            public bool WantSRGB;
            public Material Material;
            public string Property;
            public List<Slot> Slots;

            public bool CanApply => Kind != Issue.Uncompressed && Kind != Issue.Conflict;
            public bool KeepOutOfBatch => MemoryEffect == Effect.Costs;
        }

        public static Slot SlotFor(Material material, string property)
        {
            if (material == null || material.shader == null) return null;
            Shader shader = material.shader;
            int index = shader.FindPropertyIndex(property);
            if (index < 0) return new Slot { Material = material, Property = property, Label = property };
            string[] attributes = shader.GetPropertyAttributes(index);
            return new Slot
            {
                Material = material,
                Property = property,
                Label = shader.GetPropertyDescription(index),
                IsNormal = attributes.Any(a => a == "Normal"),
                SRGB = attributes.FirstOrDefault(a => a.StartsWith(SRGBAttribute, StringComparison.Ordinal)),
            };
        }

        public static string Megabytes(long bytes)
        {
            return (bytes / (1024f * 1024f)).ToString("0.0") + " MB";
        }

        public static bool IsDesktopTarget()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            return target != BuildTarget.Android && target != BuildTarget.iOS;
        }

        public static List<Finding> CheckImport(Texture tex, TextureImporter ti, string path, IList<Slot> slots, int target)
        {
            var found = new List<Finding>();
            if (tex == null || ti == null) return found;

            string name = tex.name;
            long bytes = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);
            int side = Mathf.Max(tex.width, tex.height);
            bool normal = ti.textureType == TextureImporterType.NormalMap;
            bool ui = ti.textureType == TextureImporterType.Sprite || ti.textureType == TextureImporterType.GUI || ti.textureType == TextureImporterType.Cursor;

            Finding New(Issue kind, Effect effect, string title, string description)
            {
                var f = new Finding { Texture = tex, Path = path, Kind = kind, MemoryEffect = effect, Title = title, Description = description };
                found.Add(f);
                return f;
            }

            if (ti.isReadable)
            {
                New(Issue.ReadWrite, Effect.Saves, "Read/Write Enabled",
                    $"'{name}' has Read/Write on, so a second copy stays in memory. Turn it off unless a script reads its pixels.");
            }

            bool bentOnly = slots != null && slots.Count > 0 && slots.All(s => s.Property == BentNormalProp);
            if (bentOnly && side > BentNormalCap)
            {
                long after = Shrunk(bytes, side, BentNormalCap);
                New(Issue.LowFrequency, Effect.Saves, "Bent Normal Bigger Than Needed",
                    $"'{name}' is a bent normal map at {side}px. It only shades broad lighting, so {BentNormalCap}px looks the same and uses about {Megabytes(after)} instead of {Megabytes(bytes)}. Your original file is not changed.")
                    .TargetSize = BentNormalCap;
            }
            else if (side > target)
            {
                long after = Shrunk(bytes, side, target);
                New(Issue.Oversize, Effect.Saves, $"Over {target}px",
                    $"'{name}' imports at {side}px, over your {target}px target. Lowering its Max Size to {target} brings it to about {Megabytes(after)} from {Megabytes(bytes)}. Your original file is not changed.")
                    .TargetSize = target;
            }

            if (normal && IsDesktopTarget())
            {
                ReadDesktopFormat(ti, out bool good, out bool crunched, out bool fourBit, out bool uncompressed);
                if (uncompressed)
                {
                    New(Issue.NormalFormat, Effect.Saves, "Normal Map Format",
                        $"'{name}' is an uncompressed normal map using {Megabytes(bytes)}. BC5 keeps the detail at a quarter of the memory.");
                }
                else if (!good || crunched)
                {
                    if (fourBit)
                        New(Issue.NormalFormat, Effect.Costs, "Normal Map Format",
                            $"'{name}' is a normal map on a low quality 4-bit format. BC5 looks far better but doubles its memory, to about {Megabytes(bytes * 2)}. Tick it yourself if the look is worth it.");
                    else if (crunched)
                        New(Issue.NormalFormat, Effect.Neutral, "Normal Map Format",
                            $"'{name}' is a crunched normal map. Crunch only shrinks the download and blurs normal maps most of all. BC5 uses the same memory and looks far better, and the download grows a little.");
                    else
                        New(Issue.NormalFormat, Effect.Neutral, "Normal Map Format",
                            $"'{name}' is a normal map that is not on BC5. BC5 uses the same memory and keeps more detail.");
                }
            }

            if (!normal && !ui && side >= SmallTextureSide && IsUncompressed(ti))
            {
                New(Issue.Uncompressed, Effect.None, "Uncompressed",
                    $"'{name}' is uncompressed and uses {Megabytes(bytes)}. Set Compression in its import settings: Normal Quality if it has no alpha, High Quality if it does. This choice is left to you.");
            }

            bool lookup = Mathf.Min(tex.width, tex.height) < SmallTextureSide || ti.wrapMode == TextureWrapMode.Clamp;
            if (!ti.mipmapEnabled && !lookup && !ui && ti.textureType != TextureImporterType.Cookie)
            {
                New(Issue.NoMipmaps, Effect.Costs, "Mipmaps Off",
                    $"'{name}' has mipmaps off, so it shimmers and draws slower at a distance. Turning them on adds about {Megabytes(bytes / 3)}, so tick it yourself.");
            }

            return found;
        }

        public static List<Finding> CheckSlots(Texture tex, TextureImporter ti, string path, IList<Slot> slots)
        {
            var found = new List<Finding>();
            if (tex == null || ti == null || slots == null) return found;

            var wanted = new HashSet<bool>();
            foreach (Slot slot in slots)
            {
                if (slot.IsNormal || slot.SRGB == null) continue;
                wanted.Add(slot.SRGB.Contains("gamma") || slot.SRGB.Contains("true"));
            }

            if (wanted.Count > 1)
            {
                found.Add(new Finding
                {
                    Texture = tex, Path = path, Kind = Issue.Conflict, MemoryEffect = Effect.None, Title = "Colour Space Conflict",
                    Description = $"'{tex.name}' is used as a colour in one slot and as data in another, so no one setting suits both. Use a separate copy of it for one of them.",
                });
            }

            var reported = new HashSet<Material>();
            foreach (Slot slot in slots)
            {
                string where = slot.Material != null ? $" on '{slot.Material.name}'" : "";
                if (slot.IsNormal)
                {
                    if (ti.textureType != TextureImporterType.NormalMap && reported.Add(slot.Material))
                    {
                        found.Add(new Finding
                        {
                            Texture = tex, Path = path, Kind = Issue.NormalType, MemoryEffect = Effect.Costs, Title = "Normal Map Imported As Colour",
                            Material = slot.Material, Property = slot.Property,
                            Description = $"'{tex.name}' fills {slot.Label}{where}, which expects a normal map, but it imports as a colour texture, so its lighting comes out wrong. Setting its type to Normal map fixes that and can make it bigger in memory, so tick it yourself.",
                        });
                    }
                    continue;
                }

                if (slot.SRGB == null || wanted.Count > 1) continue;
                bool want = slot.SRGB.Contains("gamma") || slot.SRGB.Contains("true");
                if (ti.sRGBTexture != want && reported.Add(slot.Material))
                {
                    found.Add(new Finding
                    {
                        Texture = tex, Path = path, Kind = Issue.ColorSpace, MemoryEffect = Effect.Neutral, Title = "Wrong Colour Space",
                        Material = slot.Material, Property = slot.Property, WantSRGB = want,
                        Description = want
                            ? $"'{tex.name}' fills {slot.Label}{where} as a colour, but sRGB is off, so it comes out too dark and flat."
                            : $"'{tex.name}' fills {slot.Label}{where} as data, but sRGB is on, so its values read wrong.",
                    });
                }
            }

            return found;
        }

        public static Finding CheckSource(Texture tex, string path, IList<Slot> slots, bool looseUse)
        {
            if (tex == null || string.IsNullOrEmpty(path) || looseUse || slots == null || slots.Count == 0) return null;
            if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) return null;
            if (path.StartsWith(ConvertedRoot + "/", StringComparison.OrdinalIgnoreCase)) return null;
            if (slots.Any(s => s.Material == null || !IsEditableMaterial(s.Material))) return null;

            string ext = Path.GetExtension(path).ToLowerInvariant();
            string full = Path.GetFullPath(path);
            if (!File.Exists(full)) return null;
            long fileBytes = new FileInfo(full).Length;

            string folder = FolderFor(slots);
            string description;
            if (ext == ".psd")
                description = $"'{tex.name}' is a Photoshop file. Convert saves a flattened PNG copy to {folder}/ and points your materials at it. Your PSD and its layers stay where they are.";
            else if (ext == ".jpg" || ext == ".jpeg")
                description = $"'{tex.name}' is a JPG, which loses detail every time it is saved. Convert saves a PNG copy to {folder}/ and points your materials at it, so no more is lost if you edit it. It cannot bring back what the JPG already lost. The original stays where it is.";
            else if (LooseSourceExtensions.Contains(ext) && fileBytes > LooseSourceBytes)
                description = $"'{tex.name}' is a {Megabytes(fileBytes)} {ext.TrimStart('.').ToUpperInvariant()} file. Convert saves a PNG copy with the same pixels to {folder}/ and points your materials at it. The original stays where it is.";
            else
                return null;

            return new Finding
            {
                Texture = tex, Path = path, Kind = Issue.SourceFormat, MemoryEffect = Effect.Neutral, Title = "Convert To PNG",
                Description = description, Slots = slots.ToList(),
            };
        }

        public static bool IsEditableMaterial(Material material)
        {
            string path = AssetDatabase.GetAssetPath(material);
            return !string.IsNullOrEmpty(path)
                && path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                && path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase);
        }

        public static bool Apply(Finding f)
        {
            if (f == null || !f.CanApply) return false;
            if (f.Kind == Issue.SourceFormat) return ConvertToPng(f);

            var ti = AssetImporter.GetAtPath(f.Path) as TextureImporter;
            if (ti == null) return false;

            switch (f.Kind)
            {
                case Issue.ReadWrite:
                    ti.isReadable = false;
                    break;

                case Issue.Oversize:
                case Issue.LowFrequency:
                    CapMaxSize(ti, f.TargetSize);
                    break;

                case Issue.NormalFormat:
                {
                    TextureImporterPlatformSettings ps = ti.GetPlatformTextureSettings(DesktopPlatform);
                    if (!ps.overridden)
                    {
                        TextureImporterPlatformSettings def = ti.GetDefaultPlatformTextureSettings();
                        ps.maxTextureSize = def.maxTextureSize;
                        ps.resizeAlgorithm = def.resizeAlgorithm;
                    }
                    ps.overridden = true;
                    ps.format = TextureImporterFormat.BC5;
                    ps.crunchedCompression = false;
                    ti.SetPlatformTextureSettings(ps);
                    ti.crunchedCompression = false;
                    break;
                }

                case Issue.NormalType:
                    ti.textureType = TextureImporterType.NormalMap;
                    break;

                case Issue.NoMipmaps:
                    ti.mipmapEnabled = true;
                    break;

                case Issue.ColorSpace:
                    ti.sRGBTexture = f.WantSRGB;
                    break;

                default:
                    return false;
            }

            ti.SaveAndReimport();
            Debug.Log($"[Vixen World Engine] {f.Title}: changed the import settings of '{f.Path}'.");
            return true;
        }

        private static long Shrunk(long bytes, int side, int cap)
        {
            if (side <= cap || side <= 0) return bytes;
            double ratio = (double)cap / side;
            return (long)(bytes * ratio * ratio);
        }

        private static void CapMaxSize(TextureImporter ti, int cap)
        {
            if (cap <= 0) return;
            if (ti.maxTextureSize > cap) ti.maxTextureSize = cap;
            foreach (string platform in OverridePlatforms)
            {
                TextureImporterPlatformSettings ps = ti.GetPlatformTextureSettings(platform);
                if (ps != null && ps.overridden && ps.maxTextureSize > cap)
                {
                    ps.maxTextureSize = cap;
                    ti.SetPlatformTextureSettings(ps);
                }
            }
        }

        private static void ReadDesktopFormat(TextureImporter ti, out bool good, out bool crunched, out bool fourBit, out bool uncompressed)
        {
            TextureImporterPlatformSettings ps = ti.GetPlatformTextureSettings(DesktopPlatform);
            if (ps != null && ps.overridden)
            {
                TextureImporterFormat fmt = ps.format;
                good = fmt == TextureImporterFormat.BC5 || fmt == TextureImporterFormat.BC7;
                crunched = fmt == TextureImporterFormat.DXT1Crunched || fmt == TextureImporterFormat.DXT5Crunched;
                fourBit = fmt == TextureImporterFormat.DXT1 || fmt == TextureImporterFormat.DXT1Crunched;
                uncompressed = fmt != TextureImporterFormat.Automatic && !IsCompressedFormat(fmt);
                if (fmt == TextureImporterFormat.Automatic) ReadDefault(ti, out good, out crunched, out fourBit, out uncompressed);
                return;
            }
            ReadDefault(ti, out good, out crunched, out fourBit, out uncompressed);
        }

        private static void ReadDefault(TextureImporter ti, out bool good, out bool crunched, out bool fourBit, out bool uncompressed)
        {
            TextureImporterCompression c = ti.textureCompression;
            uncompressed = c == TextureImporterCompression.Uncompressed;
            good = c == TextureImporterCompression.CompressedHQ;
            crunched = !good && !uncompressed && ti.crunchedCompression;
            fourBit = c == TextureImporterCompression.CompressedLQ;
        }

        private static bool IsUncompressed(TextureImporter ti)
        {
            string platform = IsDesktopTarget() ? DesktopPlatform : (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? "iPhone" : "Android");
            TextureImporterPlatformSettings ps = ti.GetPlatformTextureSettings(platform);
            if (ps != null && ps.overridden && ps.format != TextureImporterFormat.Automatic) return !IsCompressedFormat(ps.format);
            return ti.textureCompression == TextureImporterCompression.Uncompressed;
        }

        private static bool IsCompressedFormat(TextureImporterFormat f)
        {
            switch (f)
            {
                case TextureImporterFormat.RGBA32:
                case TextureImporterFormat.RGB24:
                case TextureImporterFormat.ARGB32:
                case TextureImporterFormat.RGBAHalf:
                case TextureImporterFormat.RGBAFloat:
                    return false;
                default:
                    return true;
            }
        }

        private static string FolderFor(IList<Slot> slots)
        {
            string material = slots.Select(s => s.Material != null ? s.Material.name : "").Where(n => n.Length > 0).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).FirstOrDefault() ?? "Unnamed";
            return ConvertedRoot + "/" + SafeName(material);
        }

        private static string SafeName(string name)
        {
            char[] bad = Path.GetInvalidFileNameChars();
            string clean = new string(name.Select(c => bad.Contains(c) ? '_' : c).ToArray()).Trim().TrimEnd('.');
            return clean.Length == 0 ? "Unnamed" : clean;
        }

        private static void EnsureFolder(string assetFolder)
        {
            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static bool ConvertToPng(Finding f)
        {
            var source = AssetImporter.GetAtPath(f.Path) as TextureImporter;
            if (source == null || f.Slots == null || f.Slots.Count == 0) return false;

            string folder = FolderFor(f.Slots);
            EnsureFolder(folder);
            string target = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + SafeName(Path.GetFileNameWithoutExtension(f.Path)) + ".png");

            try
            {
                using (var img = new MagickImage(File.ReadAllBytes(Path.GetFullPath(f.Path))))
                {
                    if (img.Depth > 16)
                    {
                        Debug.LogWarning($"[Vixen World Engine] Skipped '{f.Path}': it holds more than 16 bits per channel, which PNG cannot keep.");
                        return false;
                    }
                    img.Format = MagickFormat.Png;
                    img.Settings.SetDefine(MagickFormat.Png, "exclude-chunks", string.Join(",", PngExcludedChunks));
                    img.Settings.SetDefine(MagickFormat.Png, "compression-level", 9);
                    img.Write(Path.GetFullPath(target));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Vixen World Engine] Could not convert '{f.Path}' to PNG: {ex.Message}");
                return false;
            }

            AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
            var copy = AssetImporter.GetAtPath(target) as TextureImporter;
            if (copy == null)
            {
                Debug.LogWarning($"[Vixen World Engine] '{target}' was written but Unity did not import it as a texture. Your materials were not changed.");
                return false;
            }

            var settings = new TextureImporterSettings();
            source.ReadTextureSettings(settings);
            copy.SetTextureSettings(settings);
            copy.ignorePngGamma = true;
            copy.SetPlatformTextureSettings(source.GetDefaultPlatformTextureSettings());
            foreach (string platform in OverridePlatforms)
            {
                TextureImporterPlatformSettings ps = source.GetPlatformTextureSettings(platform);
                if (ps != null && ps.overridden) copy.SetPlatformTextureSettings(ps);
            }
            copy.SaveAndReimport();

            Texture converted = AssetDatabase.LoadAssetAtPath<Texture>(target);
            if (converted == null) return false;

            var changed = new List<string>();
            foreach (Slot slot in f.Slots)
            {
                if (slot.Material == null || slot.Material.GetTexture(slot.Property) != f.Texture) continue;
                Undo.RecordObject(slot.Material, "Use Converted Texture");
                slot.Material.SetTexture(slot.Property, converted);
                EditorUtility.SetDirty(slot.Material);
                changed.Add($"{slot.Material.name} ({slot.Label})");
            }

            Debug.Log($"[Vixen World Engine] Converted '{f.Path}' to '{target}'. The original was not changed. Materials now using the copy:\n" + string.Join("\n", changed));
            return true;
        }
    }
}
#endif
