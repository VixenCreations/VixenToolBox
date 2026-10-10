#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using System.IO;
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
using ImageMagick;
using ImageMagick.Configuration;
#endif

namespace VixenTools.Editor
{
    [InitializeOnLoad]
    public static class VixenMagickKit
    {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
        public const string UnavailableMessage = "ImageMagick did not start in this editor, so this is turned off. The Console says why.";
#else
        public const string UnavailableMessage = "This needs ImageMagick, which the toolbox includes for Windows and Linux.";
#endif

        public static bool IsReady { get; private set; }

#if UNITY_EDITOR_LINUX
        const string LinuxNativeLibrary = "Magick.Native-Q16-x64.dll.so";
        const int RtldLazy = 0x1;
        const int RtldDeepBind = 0x8;

        [System.Runtime.InteropServices.DllImport("libdl.so.2")]
        static extern System.IntPtr dlopen(string file, int mode);

        [System.Runtime.InteropServices.DllImport("libdl.so.2")]
        static extern System.IntPtr dlerror();

        static bool LoadLinuxNativeLibrary()
        {
            var folders = new System.Collections.Generic.List<string>();
            try { folders.Add(Path.GetDirectoryName(typeof(MagickNET).Assembly.Location)); } catch { }
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(VixenMagickKit).Assembly);
            if (package != null) folders.Add(Path.Combine(package.resolvedPath, "Editor", "ImageMagik"));

            foreach (string folder in folders)
            {
                if (string.IsNullOrEmpty(folder)) continue;
                string file = Path.Combine(folder, LinuxNativeLibrary);
                if (!File.Exists(file)) continue;
                if (dlopen(file, RtldLazy | RtldDeepBind) != System.IntPtr.Zero) return true;
                Debug.LogWarning($"[VixForge] ImageMagick could not load, so the tools that need it are turned off. {System.Runtime.InteropServices.Marshal.PtrToStringAnsi(dlerror())}");
                return false;
            }

            Debug.LogWarning($"[VixForge] ImageMagick could not load, so the tools that need it are turned off. {LinuxNativeLibrary} is missing from the toolbox's Editor/ImageMagik folder.");
            return false;
        }
#endif

#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
        const string SecurityPolicy =
            "<policymap>\n" +
            "  <policy domain=\"delegate\" rights=\"none\" pattern=\"*\"/>\n" +
            "  <policy domain=\"path\" rights=\"none\" pattern=\"[Ff][Dd]:*\"/>\n" +
            "  <policy domain=\"coder\" rights=\"none\" pattern=\"{MSL,MVG,SVG,MSVG,RSVG,TEXT,URL,HTTP,HTTPS,FTP,EPHEMERAL,SHOW,WIN,X,CLIPBOARD,SCREENSHOT,VID,PANGO}\"/>\n" +
            "  <policy domain=\"coder\" rights=\"none\" pattern=\"{PS,PS2,PS3,EPS,EPI,EPSF,EPSI,EPT,PDF,PDFA,AI,XPS,PCL}\"/>\n" +
            "  <policy domain=\"coder\" rights=\"none\" pattern=\"{CIP,ASE,MAT,CUT,CALS,PCD,FLIF,HALD,CUBE,UHDR,YAML,JSON,INFO,WBINFO,YUV}\"/>\n" +
            "  <policy domain=\"resource\" name=\"width\" value=\"32KP\"/>\n" +
            "  <policy domain=\"resource\" name=\"height\" value=\"32KP\"/>\n" +
            "</policymap>\n";

        static VixenMagickKit()
        {
            // Every tool that uses ImageMagick runs from a window in the main editor, so import workers leave it off.
            // Unity starts several workers at once, and they would all write the same policy file.
            if (AssetDatabase.IsAssetImportWorkerProcess()) return;
#if UNITY_EDITOR_LINUX
            string marker = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", "VixenTools", "ImageMagick", "linux-load.pending"));
            if (File.Exists(marker))
            {
                Debug.LogWarning($"[VixForge] ImageMagick closed the editor the last time it loaded, so the tools that need it are turned off. Delete '{marker}' to try again.");
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(marker));
                File.WriteAllText(marker, string.Empty);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[VixForge] ImageMagick stays off, because its load marker could not be written: " + e.Message);
                return;
            }

            if (!LoadLinuxNativeLibrary())
            {
                try { File.Delete(marker); } catch { }
                return;
            }
#endif
            IsReady = ApplySecurityPolicy() && PolicyIsEnforced();

            if (IsReady)
            {
                try
                {
                    ResourceLimits.Thread = (ulong)System.Math.Max(1, System.Environment.ProcessorCount);
                }
                catch { }
            }
#if UNITY_EDITOR_LINUX
            try { File.Delete(marker); } catch { }
#endif
        }

        // MagickNET.Initialize only writes the files that are missing, and fails if another process
        // creates one first, so the policy is written here and only when it differs.
        static bool ApplySecurityPolicy()
        {
            IConfigurationFiles config = ConfigurationFiles.Default;
            config.Policy.Data = SecurityPolicy;
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", "VixenTools", "ImageMagick"));
            string policyFile = Path.Combine(path, config.Policy.FileName);

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    Directory.CreateDirectory(path);
                    if (!File.Exists(policyFile) || File.ReadAllText(policyFile) != SecurityPolicy)
                        File.WriteAllText(policyFile, SecurityPolicy);
                    MagickNET.Initialize(config, path);
                    return true;
                }
                catch (IOException) when (attempt < 5)
                {
                    System.Threading.Thread.Sleep(50 * attempt);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[VixForge] Could not apply the ImageMagick security policy, so the tools that need ImageMagick are turned off. " + e.Message);
                    return false;
                }
            }
        }

        // ImageMagick reads its policy once per editor session, the first time it runs, so a policy written
        // after that never takes effect. The policy blocks MVG, so ImageMagick must refuse to read one.
        static bool PolicyIsEnforced()
        {
            string detail = "";
            try
            {
                using (new MagickImage(System.Text.Encoding.ASCII.GetBytes("viewbox 0 0 4 4\nrectangle 0 0 3 3\n"), new MagickReadSettings { Format = MagickFormat.Mvg })) { }
            }
            catch (MagickPolicyErrorException) { return true; }
            catch (System.Exception e) { detail = " " + e.Message; }
            Debug.LogWarning("[VixForge] ImageMagick is running without the toolbox's security policy, so the tools that need it are turned off. Restart the editor to turn them back on." + detail);
            return false;
        }
#endif

        private static readonly string[] ProtectedPathFragments =
        {
            "/_PoiyomiShaders/",
            "/_PoiyomiToonShaders/",
            "/Poiyomi/",
            "/lilToon/",
            "/Sunao Shader/",
            "/Editor Default Resources/",
        };

        private static readonly string[] ProtectedExtensions =
        {
            ".exr", ".hdr", ".cubemap", ".rendertexture",
        };

        public static bool IsProtectedAsset(string path)
        {
            if (string.IsNullOrEmpty(path)) return true;

            string normalized = path.Replace('\\', '/');

            foreach (var ext in ProtectedExtensions)
                if (normalized.EndsWith(ext, System.StringComparison.OrdinalIgnoreCase))
                    return true;

            foreach (var fragment in ProtectedPathFragments)
                if (normalized.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

            return false;
        }

        private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;

        public static bool TryLosslessOptimize(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            if (IsProtectedAsset(path)) return false;
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
            if (!IsReady) return false;
            try
            {
                long fileBytes = new FileInfo(path).Length;
                bool useOptimal = fileBytes <= OptimalCompressionMaxBytes;

                byte[] original = File.ReadAllBytes(path);
                using var ms = new MemoryStream(original.Length);
                ms.Write(original, 0, original.Length);
                ms.Position = 0;

                var optimizer = new ImageOptimizer
                {
                    OptimalCompression = useOptimal,
                    IgnoreUnsupportedFormats = true
                };

                if (optimizer.LosslessCompress(ms))
                {
                    byte[] optimized = ms.ToArray();
                    if (optimized.Length > 0 && optimized.Length < original.Length)
                    {
                        File.WriteAllBytes(path, optimized);
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[VixForge] LosslessCompress skipped for '{path}': {ex.Message}");
            }
#endif
            return false;
        }

#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
        public static bool TryGetDimensions(byte[] bytes, out uint width, out uint height)
        {
            width = 0;
            height = 0;
            if (bytes == null || bytes.Length == 0) return false;
            try
            {
                var info = new MagickImageInfo(bytes);
                width = info.Width;
                height = info.Height;
                return width > 0 && height > 0;
            }
            catch { return false; }
        }

        public static MagickReadSettings DownscaleReadSettings(uint targetMaxDim)
        {
            var settings = new MagickReadSettings();
            if (targetMaxDim > 0)
            {
                uint hint = targetMaxDim <= (uint.MaxValue / 2u) ? targetMaxDim * 2u : targetMaxDim;
                settings.SetDefine(MagickFormat.Jpeg, "size", $"{hint}x{hint}");
            }
            return settings;
        }
#endif

        public static bool IsLinearOrNormalData(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;
            if (importer.textureType == TextureImporterType.NormalMap) return true;
            return !importer.sRGBTexture;
        }

#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
        public static void HighQualityResize(MagickImage img, uint targetW, uint targetH, bool linearData, FilterType filter, bool onlyShrink, double sharpenSigma)
        {
            if (img == null) return;
            img.FilterType = filter;

            bool gammaCorrect = !linearData && img.ColorSpace == ImageMagick.ColorSpace.sRGB;
            if (gammaCorrect) img.ColorSpace = ImageMagick.ColorSpace.RGB;

            img.Resize(new MagickGeometry(targetW, targetH) { IgnoreAspectRatio = false, Greater = onlyShrink });

            if (gammaCorrect) img.ColorSpace = ImageMagick.ColorSpace.sRGB;

            if (sharpenSigma > 0.0) img.AdaptiveSharpen(0.0, sharpenSigma);
        }

        public static void ApplyOptimalEncoding(MagickImage img, int jpegQuality = 90)
        {
            if (img == null) return;
            img.Strip();

            var fmt = img.Format;
            if (fmt == MagickFormat.Png)
            {
                img.Settings.SetDefine(MagickFormat.Png, "compression-level", 9);
            }
            else if (fmt == MagickFormat.Jpeg || fmt == MagickFormat.Jpg)
            {
                img.Quality = (uint)System.Math.Max(1, System.Math.Min(100, jpegQuality));
            }
        }
#endif

        public static bool ProcessTextureFile(string path, uint targetSize, bool linearData, bool downscale)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            bool resized = false;
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
            if (!IsReady) return false;
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (TryGetDimensions(bytes, out uint w, out uint h))
                {
                    bool needsWork = downscale ? (w > targetSize || h > targetSize) : (w < targetSize && h < targetSize);
                    if (needsWork)
                    {
                        var readSettings = downscale ? DownscaleReadSettings(targetSize) : new MagickReadSettings();
                        using (var img = new MagickImage(bytes, readSettings))
                        {
                            if (downscale)
                                HighQualityResize(img, targetSize, targetSize, linearData, FilterType.Lanczos, true, 0.5);
                            else
                                HighQualityResize(img, targetSize, targetSize, linearData, FilterType.Mitchell, false, 0.6);

                            ApplyOptimalEncoding(img);
                            img.Write(path);
                            resized = true;
                        }
                    }
                }
                TryLosslessOptimize(path);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VixForge] Magick failed for '{path}': {e.Message}");
            }
#endif
            return resized;
        }
    }
}
#endif
