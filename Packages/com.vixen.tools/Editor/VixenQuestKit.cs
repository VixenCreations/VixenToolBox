#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ImageMagick;

namespace VixenTools.Editor
{
    public enum MobileShaderTarget
    {
        ToonStandard,
        ToonStandardOutline,
        ToonLit,
        StandardLite,
        MatCapLit,
        Diffuse,
        BumpedDiffuse,
        BumpedMappedSpecular,
        ParticlesAdditive,
        ParticlesMultiply,
    }

    public static class VixenQuestKit
    {
        public delegate Texture TextureProcessor(Texture source, bool isNormalMap, bool isLinear);

        private static readonly Dictionary<MobileShaderTarget, string> ShaderNames =
            new Dictionary<MobileShaderTarget, string>
        {
            { MobileShaderTarget.ToonStandard, "VRChat/Mobile/Toon Standard" },
            { MobileShaderTarget.ToonStandardOutline, "VRChat/Mobile/Toon Standard (Outline)" },
            { MobileShaderTarget.ToonLit, "VRChat/Mobile/Toon Lit" },
            { MobileShaderTarget.StandardLite, "VRChat/Mobile/Standard Lite" },
            { MobileShaderTarget.MatCapLit, "VRChat/Mobile/MatCap Lit" },
            { MobileShaderTarget.Diffuse, "VRChat/Mobile/Diffuse" },
            { MobileShaderTarget.BumpedDiffuse, "VRChat/Mobile/Bumped Diffuse" },
            { MobileShaderTarget.BumpedMappedSpecular, "VRChat/Mobile/Bumped Mapped Specular" },
            { MobileShaderTarget.ParticlesAdditive, "VRChat/Mobile/Particles/Additive" },
            { MobileShaderTarget.ParticlesMultiply, "VRChat/Mobile/Particles/Multiply" },
        };

        private static readonly Dictionary<MobileShaderTarget, string> ShaderLabels =
            new Dictionary<MobileShaderTarget, string>
        {
            { MobileShaderTarget.ToonStandard, "Toon Standard" },
            { MobileShaderTarget.ToonStandardOutline, "Toon Standard (Outline)" },
            { MobileShaderTarget.ToonLit, "Toon Lit" },
            { MobileShaderTarget.StandardLite, "Standard Lite" },
            { MobileShaderTarget.MatCapLit, "MatCap Lit" },
            { MobileShaderTarget.Diffuse, "Diffuse" },
            { MobileShaderTarget.BumpedDiffuse, "Bumped Diffuse" },
            { MobileShaderTarget.BumpedMappedSpecular, "Bumped Mapped Specular" },
            { MobileShaderTarget.ParticlesAdditive, "Particles Additive" },
            { MobileShaderTarget.ParticlesMultiply, "Particles Multiply" },
        };

        private static readonly MobileShaderTarget[] FallbackOrder =
        {
            MobileShaderTarget.ToonStandard,
            MobileShaderTarget.StandardLite,
            MobileShaderTarget.ToonLit,
            MobileShaderTarget.Diffuse,
        };

        public static string GetShaderName(MobileShaderTarget target)
        {
            return ShaderNames.TryGetValue(target, out string name) ? name : ShaderNames[MobileShaderTarget.ToonStandard];
        }

        public static string GetShaderLabel(MobileShaderTarget target)
        {
            return ShaderLabels.TryGetValue(target, out string label) ? label : target.ToString();
        }

        public static List<string> AllShaderLabels()
        {
            return Enum.GetValues(typeof(MobileShaderTarget))
                .Cast<MobileShaderTarget>()
                .Select(GetShaderLabel)
                .ToList();
        }

        public static MobileShaderTarget TargetFromLabel(string label)
        {
            foreach (var pair in ShaderLabels)
            {
                if (pair.Value == label) return pair.Key;
            }
            return MobileShaderTarget.ToonStandard;
        }

        public static string[] VRChatMobileWhitelist()
        {
            return VRC.SDKBase.Validation.AvatarValidation.ShaderWhiteList;
        }

        public static bool IsWhitelisted(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            foreach (var allowed in VRChatMobileWhitelist())
            {
                if (string.Equals(allowed, shaderName, StringComparison.Ordinal)) return true;
            }
            return false;
        }

        public static bool IsWhitelisted(Shader shader)
        {
            return shader != null && IsWhitelisted(shader.name);
        }

        public static Shader ResolveShader(MobileShaderTarget target)
        {
            Shader found = Shader.Find(GetShaderName(target));
            if (found != null) return found;

            foreach (var candidate in FallbackOrder)
            {
                found = Shader.Find(GetShaderName(candidate));
                if (found != null) return found;
            }

            foreach (var allowed in VRChatMobileWhitelist())
            {
                found = Shader.Find(allowed);
                if (found != null) return found;
            }

            return null;
        }

        public static void TransferProperties(Material source, Material target, TextureProcessor process)
        {
            if (source == null || target == null) return;
            if (process == null) process = (t, n, l) => t;

            if (source.HasProperty("_MainTex") && target.HasProperty("_MainTex"))
                target.SetTexture("_MainTex", process(source.GetTexture("_MainTex"), false, false));
            else if (source.HasProperty("_BaseMap") && target.HasProperty("_MainTex"))
                target.SetTexture("_MainTex", process(source.GetTexture("_BaseMap"), false, false));

            if (source.HasProperty("_Color") && target.HasProperty("_Color"))
                target.SetColor("_Color", source.GetColor("_Color"));
            else if (source.HasProperty("_BaseColor") && target.HasProperty("_Color"))
                target.SetColor("_Color", source.GetColor("_BaseColor"));

            if (source.HasProperty("_EmissionMap") && target.HasProperty("_EmissionMap"))
                target.SetTexture("_EmissionMap", process(source.GetTexture("_EmissionMap"), false, false));

            if (source.HasProperty("_EmissionColor") && target.HasProperty("_EmissionColor"))
                target.SetColor("_EmissionColor", source.GetColor("_EmissionColor"));

            if (source.HasProperty("_BumpMap") && target.HasProperty("_BumpMap"))
            {
                target.SetTexture("_BumpMap", process(source.GetTexture("_BumpMap"), true, true));
                if (source.HasProperty("_BumpScale") && target.HasProperty("_BumpScale"))
                    target.SetFloat("_BumpScale", source.GetFloat("_BumpScale"));
            }

            if (target.HasProperty("_MetallicGlossMap") || target.HasProperty("_MetallicMap"))
            {
                string sourceMetProp = null;

                if (source.HasProperty("_MetallicGlossMap")) sourceMetProp = "_MetallicGlossMap";
                else if (source.HasProperty("_MetallicMap")) sourceMetProp = "_MetallicMap";
                else if (source.HasProperty("_MochieMetallicMaps")) sourceMetProp = "_MochieMetallicMaps";
                else if (source.HasProperty("_MochieMetallicMap")) sourceMetProp = "_MochieMetallicMap";

                string targetMetProp = target.HasProperty("_MetallicGlossMap") ? "_MetallicGlossMap" : "_MetallicMap";

                if (sourceMetProp != null && source.GetTexture(sourceMetProp) != null)
                    target.SetTexture(targetMetProp, process(source.GetTexture(sourceMetProp), false, true));

                if (source.HasProperty("_Metallic") && target.HasProperty("_Metallic"))
                    target.SetFloat("_Metallic", source.GetFloat("_Metallic"));

                if (source.HasProperty("_Glossiness") && target.HasProperty("_Glossiness"))
                    target.SetFloat("_Glossiness", source.GetFloat("_Glossiness"));
            }

            CopyMainTextureScaleAndOffset(source, target);
        }

        private static void CopyMainTextureScaleAndOffset(Material source, Material target)
        {
            if (!source.HasProperty("_MainTex") || !target.HasProperty("_MainTex")) return;

            target.SetTextureScale("_MainTex", source.GetTextureScale("_MainTex"));
            target.SetTextureOffset("_MainTex", source.GetTextureOffset("_MainTex"));
        }

        public static Texture ProcessAndCloneTexture(
            Texture sourceTex,
            bool isNormalMap,
            bool isLinear,
            string outputDir,
            int targetSize,
            Dictionary<Texture, Texture> cache,
            Func<Texture, bool> shouldProcess)
        {
            if (sourceTex == null) return null;

            if (shouldProcess != null && !shouldProcess(sourceTex)) return sourceTex;

            if (cache != null && cache.TryGetValue(sourceTex, out Texture cachedTex)) return cachedTex;

            string sourcePath = AssetDatabase.GetAssetPath(sourceTex);

            if (string.IsNullOrEmpty(sourcePath) || sourcePath.StartsWith("Resources/") || sourcePath.StartsWith("Library/"))
            {
                return sourceTex;
            }

            if (VixenMagickKit.IsProtectedAsset(sourcePath))
            {
                return sourceTex;
            }

            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning($"[VixForge] Skipping a texture that is not on disk: {sourceTex.name} at {sourcePath}.");
                return sourceTex;
            }

            EnsureDirectoryExists(outputDir);

            string texName = sourceTex.name;
            string extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(extension)) extension = ".png";

            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{outputDir}/{texName}_Quest{extension}");

            try
            {
                using (MagickImage img = new MagickImage(File.ReadAllBytes(sourcePath), VixenMagickKit.DownscaleReadSettings((uint)targetSize)))
                {
                    if (img.Width > targetSize || img.Height > targetSize)
                    {
                        bool linear = isNormalMap || isLinear;
                        VixenMagickKit.HighQualityResize(img, (uint)targetSize, (uint)targetSize, linear, FilterType.Lanczos, true, 1.0);
                    }
                    VixenMagickKit.ApplyOptimalEncoding(img);
                    img.Write(newPath);
                }
                VixenMagickKit.TryLosslessOptimize(newPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VixForge] Image processing failed for {texName}, copying the original instead. {ex.Message}");

                if (File.Exists(sourcePath))
                {
                    AssetDatabase.CopyAsset(sourcePath, newPath);
                }
                else
                {
                    Debug.LogError($"[VixForge] Copy failed. The source file is missing: {sourcePath}.");
                    return sourceTex;
                }
            }

            AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(newPath) as TextureImporter;

            if (importer != null)
            {
                importer.maxTextureSize = targetSize;

                if (isNormalMap) importer.textureType = TextureImporterType.NormalMap;
                else if (isLinear) importer.sRGBTexture = false;
                else importer.sRGBTexture = true;

                TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings
                {
                    name = "Android",
                    overridden = true,
                    maxTextureSize = targetSize,
                    format = TextureImporterFormat.ASTC_6x6,
                    textureCompression = TextureImporterCompression.Compressed
                };

                importer.SetPlatformTextureSettings(androidSettings);
                importer.SaveAndReimport();
            }

            Texture newTex = AssetDatabase.LoadAssetAtPath<Texture>(newPath);
            if (cache != null) cache[sourceTex] = newTex;
            return newTex;
        }

        public static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (Directory.Exists(path)) return;

            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
#endif
