#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using System.IO;
using ImageMagick;

namespace VixenTools.Editor
{
    [InitializeOnLoad]
    public static class VixenMagickKit
    {
        static VixenMagickKit()
        {
            try
            {
                ResourceLimits.Thread = (ulong)System.Math.Max(1, System.Environment.ProcessorCount);
            }
            catch { }
        }

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
            return false;
        }

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

        public static bool IsLinearOrNormalData(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;
            if (importer.textureType == TextureImporterType.NormalMap) return true;
            return !importer.sRGBTexture;
        }

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

        public static bool ProcessTextureFile(string path, uint targetSize, bool linearData, bool downscale)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            bool resized = false;
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
            return resized;
        }
    }
}
#endif
