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

        public static readonly string[] VixenWorldShaders = new string[]
        {
            "VixForge/World Surface",
            "VixForge/World Fur",
        };

        public static readonly string[] VixenAvatarShaders = new string[]
        {
            "VixenWear/Latex Ultra",
            "VixenWear/Clothing Pro",
            "VixForge/Fur Pro",
            "VixForge/Toon",
        };

        public static readonly string[] VixenShaderPrefixes = new string[]
        {
            "VixenWear/",
            "VixForge/",
        };

        public static readonly string[] VixenRetiredShaderPrefixes = new string[]
        {
            "VixenWorld/",
            "Vixen/",
        };

        public static readonly string[] ProtectedShaderPrefixes = new string[]
        {
            "Towel/",
            "GPU Grass/",
            "GPU Infinite Grass/",
            "QvPen/",
            "Silent/",
            "Mochie/",
        };

        public static IEnumerable<string> AllVixenShaders()
        {
            foreach (var name in VixenWorldShaders) yield return name;
            foreach (var name in VixenAvatarShaders) yield return name;
        }

        public static bool IsVixenShader(Shader s)
        {
            if (s == null) return false;
            foreach (var prefix in VixenShaderPrefixes)
            {
                if (s.name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool IsRetiredVixenShader(Shader s)
        {
            if (s == null) return false;
            foreach (var prefix in VixenRetiredShaderPrefixes)
            {
                if (s.name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static void AutoPopulateTargets(ShaderDictionaryAsset dict)
        {
            if (dict == null) return;
            int addedCount = 0;
            int missingCount = 0;

            foreach (var name in AllVixenShaders())
            {
                Shader s = Shader.Find(name);

                if (s == null)
                {
                    missingCount++;
                    continue;
                }

                if (!dict.shaders.Contains(s))
                {
                    dict.shaders.Add(s);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                EditorUtility.SetDirty(dict);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Vixen System] Added {addedCount} VixForge shaders to the replacement list.");
            }

            if (missingCount > 0)
            {
                Debug.Log($"[Vixen System] {missingCount} VixForge shaders are not in this project. Install VixenWear to use them as replacements.");
            }
        }

        public const string PoiyomiLockedPrefix = "Hidden/Locked/.poiyomi/";
        public const string PoiyomiUnlockedPrefix = ".poiyomi/";

        private static bool? _poiyomiInstalled;

        public static void ResetPoiyomiCache()
        {
            _poiyomiInstalled = null;
        }

        public static bool IsPoiyomiInstalled()
        {
            if (_poiyomiInstalled.HasValue) return _poiyomiInstalled.Value;

            bool found = false;
            foreach (var info in ShaderUtil.GetAllShaderInfo())
            {
                if (info.name.StartsWith(PoiyomiUnlockedPrefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }

            _poiyomiInstalled = found;
            return found;
        }

        public static bool IsLockedPoiyomiShader(Shader s)
        {
            return s != null && s.name.StartsWith(PoiyomiLockedPrefix, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPoiyomiShader(Shader s)
        {
            if (s == null) return false;

            return s.name.StartsWith(PoiyomiLockedPrefix, System.StringComparison.OrdinalIgnoreCase)
                || s.name.StartsWith(PoiyomiUnlockedPrefix, System.StringComparison.OrdinalIgnoreCase)
                || s.name.IndexOf("Poiyomi", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsOrphanedPoiyomiShader(Shader s)
        {
            return IsPoiyomiShader(s) && !IsPoiyomiInstalled();
        }

        public static bool IsGloballyProtected(Shader s)
        {
            if (s == null) return false;

            string name = s.name;

            if (IsRetiredVixenShader(s)) return false;
            if (IsVixenShader(s)) return true;
            if (IsOrphanedPoiyomiShader(s)) return false;

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

            foreach (var prefix in ProtectedShaderPrefixes)
            {
                if (name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) return true;
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