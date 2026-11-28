#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VixenTools.Editor
{
    public static class TextureCompressionTools
    {
        private const string MenuRoot = "VixenTools/Unity Engine/Textures/";
        private const string ResetMenu = MenuRoot + "Reset Compression to Default";
        private const string DisableMenu = MenuRoot + "Turn Off Compression (Testing)";

        private static readonly string[] Platforms =
        {
            "Standalone", "Android", "iPhone", "WebGL", "WSA", "tvOS", "PS4", "PS5",
            "XboxOne", "GameCoreXboxOne", "GameCoreXboxSeries", "Switch"
        };

        [MenuItem(ResetMenu)]
        public static void ResetCompression()
        {
            List<string> paths = CollectTexturePaths();
            if (paths.Count == 0)
            {
                WarnNothingSelected();
                return;
            }

            if (!EditorUtility.DisplayDialog("Reset Texture Compression",
                    $"Set compression back to Unity's default on {paths.Count} texture(s)?\n\nPlatform overrides stay on and keep their max size, but their format goes back to Automatic. Every scene and avatar that uses these textures changes too.",
                    "Reset", "Cancel"))
                return;

            int changed = Apply(paths, "Resetting Texture Compression", imp => SetCompression(imp, TextureImporterCompression.Compressed));
            Debug.Log($"[VixForge] Reset compression to default on {changed} of {paths.Count} texture(s).");
        }

        [MenuItem(DisableMenu)]
        public static void DisableCompression()
        {
            List<string> paths = CollectTexturePaths();
            if (paths.Count == 0)
            {
                WarnNothingSelected();
                return;
            }

            if (!EditorUtility.DisplayDialog("Turn Off Texture Compression",
                    $"Turn off compression on {paths.Count} texture(s)?\n\nThis is for testing, such as checking that the Optimization Suite finds uncompressed textures. Uncompressed textures use up to 8 times more video memory. Reset Compression to Default puts them back.",
                    "Turn Off", "Cancel"))
                return;

            int changed = Apply(paths, "Turning Off Texture Compression", imp => SetCompression(imp, TextureImporterCompression.Uncompressed));
            Debug.Log($"[VixForge] Turned off compression on {changed} of {paths.Count} texture(s).");
        }

        [MenuItem(ResetMenu, true)]
        [MenuItem(DisableMenu, true)]
        private static bool HasSelection()
        {
            return Selection.objects.Length > 0;
        }

        private static bool SetCompression(TextureImporter imp, TextureImporterCompression compression)
        {
            bool modified = false;
            if (imp.textureCompression != compression)
            {
                imp.textureCompression = compression;
                modified = true;
            }
            if (imp.crunchedCompression)
            {
                imp.crunchedCompression = false;
                modified = true;
            }

            foreach (string platform in Platforms)
            {
                TextureImporterPlatformSettings pc = imp.GetPlatformTextureSettings(platform);
                if (!pc.overridden) continue;
                if (pc.format == TextureImporterFormat.Automatic && pc.textureCompression == compression && !pc.crunchedCompression) continue;

                pc.format = TextureImporterFormat.Automatic;
                pc.textureCompression = compression;
                pc.crunchedCompression = false;
                imp.SetPlatformTextureSettings(pc);
                modified = true;
            }
            return modified;
        }

        private static int Apply(List<string> paths, string title, System.Func<TextureImporter, bool> change)
        {
            int changed = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < paths.Count; i++)
                {
                    if (i % 10 == 0 && EditorUtility.DisplayCancelableProgressBar(title, $"{paths[i]} ({i + 1}/{paths.Count})", (float)i / paths.Count))
                    {
                        Debug.LogWarning("[VixForge] Canceled. Textures already changed keep their new settings.");
                        break;
                    }

                    if (!(AssetImporter.GetAtPath(paths[i]) is TextureImporter imp)) continue;
                    if (!change(imp)) continue;
                    imp.SaveAndReimport();
                    changed++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }
            return changed;
        }

        private static List<string> CollectTexturePaths()
        {
            List<string> paths = new List<string>();
            HashSet<string> seen = new HashSet<string>();

            void Add(string path)
            {
                if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/")) return;
                if (!(AssetImporter.GetAtPath(path) is TextureImporter)) return;
                if (seen.Add(path)) paths.Add(path);
            }

            void AddFromMaterials(IEnumerable<Material> materials)
            {
                Object[] roots = materials.Where(m => m != null).Cast<Object>().ToArray();
                if (roots.Length == 0) return;
                foreach (Object dep in EditorUtility.CollectDependencies(roots))
                    if (dep is Texture) Add(AssetDatabase.GetAssetPath(dep));
            }

            foreach (Object obj in Selection.objects)
            {
                if (obj == null) continue;
                string path = AssetDatabase.GetAssetPath(obj);

                if (obj is GameObject go)
                {
                    foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
                        AddFromMaterials(r.sharedMaterials);
                }
                else if (obj is Material mat)
                {
                    AddFromMaterials(new[] { mat });
                }
                else if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (string guid in AssetDatabase.FindAssets("t:Texture", new[] { path }))
                        Add(AssetDatabase.GUIDToAssetPath(guid));
                }
                else
                {
                    Add(path);
                }
            }
            return paths;
        }

        private static void WarnNothingSelected()
        {
            Debug.LogWarning("[VixForge] Select textures, a folder, materials, or an avatar or object in the scene, then try again. Only textures inside Assets can be changed.");
        }
    }
}
#endif
