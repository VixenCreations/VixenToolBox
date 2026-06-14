#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace VixenTools.Editor
{
    [CreateAssetMenu(fileName = "VixenShaderDictionary", menuName = "VixenTools/Shader Dictionary")]
    public class ShaderDictionaryAsset : ScriptableObject
    {
        [Header("Contained Shaders")]
        public List<Shader> shaders = new List<Shader>();

        public static void AutoPopulateTargets(ShaderDictionaryAsset dict)
        {
            if (dict == null) return;
            int addedCount = 0;
            string[] targetPaths = new string[] {
                "Packages/com.vrchat.base/Runtime/VRCSDK/Sample Assets/Shaders/Mobile/ToonStandard/ToonStandard.shader",
                "Packages/com.vrchat.base/Runtime/VRCSDK/Sample Assets/Shaders/Mobile/ToonStandard/ToonStandardOutline.shader",
                "Packages/com.vrchat.base/Runtime/VRCSDK/Sample Assets/Shaders/Mobile/VRChat-Mobile-StandardLite.shader",
                "Packages/com.vrchat.base/Runtime/VRCSDK/Sample Assets/Shaders/Mobile/VRChat-Mobile-ToonLit.shader",
                "Packages/s-ilent.filamented/Filamented/Standard.shader",
                "Packages/s-ilent.filamented/Filamented/StandardCloth.shader",
                "Packages/s-ilent.filamented/Filamented/StandardRoughness.shader",
                "Packages/s-ilent.filamented/Filamented/StandardSpecular.shader",
                "Assets/Mochie/Standard Shader/Standard.shader",
                "Assets/Mochie/Standard Shader/Standard Lite.shader",
                "Assets/Mochie/Standard Shader/Standard Mobile.shader"
            };

            foreach (var path in targetPaths)
            {
                Shader s = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (s == null) s = Shader.Find(path);

                if (s != null && !dict.shaders.Contains(s))
                {
                    dict.shaders.Add(s);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                EditorUtility.SetDirty(dict);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Vixen System] Auto-Populated Target Dictionary with {addedCount} PBR/Toon shaders.");
            }
        }

        public static bool IsGloballyProtected(Shader s)
        {
            if (s == null) return false;

            string name = s.name;

            if (name == "Particles/Standard Unlit" || name == "Unlit/Color") return true;

            if (name.StartsWith("Skybox/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Nature/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Terrain", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Hidden/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("UI/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("GUI/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Particles/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Sprites/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("FX/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("VR/", System.StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("VRChat/UI/", System.StringComparison.OrdinalIgnoreCase) ||
                name.Contains("InternalErrorShader"))
            {
                return true;
            }

            string path = AssetDatabase.GetAssetPath(s);
            if (string.IsNullOrEmpty(path)) return false;

            if (path == "Resources/unity_builtin_extra" || path == "Library/unity default resources")
            {
                if (name != "Standard" && name != "Standard (Specular setup)" && !name.StartsWith("Legacy Shaders/"))
                {
                    return true;
                }
            }

            path = path.Replace("\\", "/");

            string[] protectedPaths = new string[]
            {
                "Packages/com.llealloo.audiolink/",
                "Assets/AudioLink/",
                "Packages/idv.jlchntoz.vvmw/",
                "Packages/com.texelsaur.video/",
                "Packages/red.sim.lightvolumes/",
                "Packages/red.sim.particlevolumes/",
                "Packages/jp.lilxyzw.editortoolbox/",
                "Packages/dev.architech.protv/",
                "Packages/com.vrcbilliards.vrcbce2/Shaders",
                "Packages/com.vrchat.base/Runtime/VRCSDK/Sample Assets/Shaders",
                "Packages/at.pimaker.ltcgi/",
                "Assets/TsunaMoo/",
                "Assets/_TechnicallySane/",
                "Assets/[● Radiance]/",
                "Assets/MS-VRCSA-Billiards/",
                "Assets/Beer Tap Machine Matic 954/",
                "Assets/HoshinoLabs/iwaSync3/",
                "Assets/AVProVideo/",
                "Assets/TextMesh Pro/",
                "Assets/Mochie/Glass Shader/",
                "Assets/Mochie/LED Shader/",
                "Assets/Mochie/Particle Shader/",
                "Assets/Mochie/ScreenFX Shader/",
                "Assets/Mochie/Uber Shader/",
                "Assets/Mochie/Unity/",
                "Assets/Mochie/Water Shader/",
                "Assets/House/Shader",
                "/OptimizedShaders/"
            };

            foreach (var p in protectedPaths)
            {
                if (path.Contains(p)) return true;
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(path, @"Packages/com\.acchosen\.vr-stage-lighting/Runtime/Shaders/.*"))
            {
                return true;
            }

            return false;
        }

        public static void AutoPopulateWhitelist(ShaderDictionaryAsset dict)
        {
            if (dict == null) return;

            int addedCount = 0;
            var allShaders = ShaderUtil.GetAllShaderInfo();

            foreach (var info in allShaders)
            {
                Shader s = Shader.Find(info.name);

                if (s != null && IsGloballyProtected(s) && !dict.shaders.Contains(s))
                {
                    dict.shaders.Add(s);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                EditorUtility.SetDirty(dict);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Vixen System] Discovered and populated Whitelist Dictionary with {addedCount} globally protected shaders.");
            }
        }
    }
}
#endif