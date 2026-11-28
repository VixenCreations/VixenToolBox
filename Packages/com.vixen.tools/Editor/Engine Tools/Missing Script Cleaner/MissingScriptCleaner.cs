#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VixenTools.Editor
{
    public static class MissingScriptCleaner
    {
        private const string MenuRoot = "VixenTools/Unity Engine/Clean Missing Scripts/";
        private const string UndoName = "Remove Missing Scripts";

        [MenuItem(MenuRoot + "From Selection")]
        public static void CleanSelection()
        {
            List<GameObject> sceneRoots = new List<GameObject>();
            List<string> prefabPaths = new List<string>();
            foreach (GameObject go in Selection.gameObjects)
            {
                if (go == null) continue;
                if (EditorUtility.IsPersistent(go))
                {
                    string path = AssetDatabase.GetAssetPath(go);
                    if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab") && !prefabPaths.Contains(path)) prefabPaths.Add(path);
                }
                else
                {
                    sceneRoots.Add(go);
                }
            }

            if (sceneRoots.Count == 0 && prefabPaths.Count == 0)
            {
                Debug.LogWarning("[VixForge] Select objects in the scene, or prefabs in the Project window, then try again.");
                return;
            }

            if (prefabPaths.Count > 0 && !EditorUtility.DisplayDialog("Clean Missing Scripts",
                    $"Remove missing scripts from {prefabPaths.Count} selected prefab(s) and save them? Changes to prefab files can't be undone with Ctrl+Z.",
                    "Remove", "Cancel"))
                prefabPaths.Clear();

            HashSet<string> blockedBy = new HashSet<string>();
            int removed = CleanSceneObjects(sceneRoots, blockedBy, out int left);
            removed += CleanPrefabAssets(prefabPaths, blockedBy, ref left, out int prefabsSaved);
            Report(removed, prefabsSaved, left, blockedBy, "your selection");
        }

        [MenuItem(MenuRoot + "From Selection", true)]
        private static bool CleanSelectionValidate()
        {
            return Selection.gameObjects.Length > 0;
        }

        [MenuItem(MenuRoot + "From Open Scenes")]
        public static void CleanOpenScenes()
        {
            List<GameObject> roots = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded) roots.AddRange(scene.GetRootGameObjects());
            }

            HashSet<string> blockedBy = new HashSet<string>();
            int removed = CleanSceneObjects(roots, blockedBy, out int left);
            Report(removed, 0, left, blockedBy, "the open scenes");
        }

        private static int CleanSceneObjects(List<GameObject> roots, HashSet<string> blockedBy, out int left)
        {
            left = 0;
            int removed = 0;
            if (roots.Count == 0) return 0;

            Undo.SetCurrentGroupName(UndoName);
            int group = Undo.GetCurrentGroup();
            HashSet<GameObject> seen = new HashSet<GameObject>();

            foreach (GameObject root in roots)
            {
                if (root == null) continue;
                Transform[] hierarchy = root.GetComponentsInChildren<Transform>(true);
                bool recorded = false;
                foreach (Transform t in hierarchy)
                {
                    GameObject go = t.gameObject;
                    if (!seen.Add(go)) continue;
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) == 0) continue;

                    if (!recorded)
                    {
                        Undo.RegisterFullObjectHierarchyUndo(root, UndoName);
                        recorded = true;
                    }
                    int here = RemoveFrom(go);
                    removed += here;
                    if (here > 0) EditorSceneManager.MarkSceneDirty(go.scene);

                    int stuck = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    if (stuck > 0)
                    {
                        left += stuck;
                        string source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                        if (!string.IsNullOrEmpty(source)) blockedBy.Add(source);
                    }
                }
            }

            Undo.CollapseUndoOperations(group);
            return removed;
        }

        private static int CleanPrefabAssets(List<string> paths, HashSet<string> blockedBy, ref int left, out int saved)
        {
            saved = 0;
            int removed = 0;
            foreach (string path in paths)
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int here = 0;
                    foreach (Transform t in contents.GetComponentsInChildren<Transform>(true))
                    {
                        GameObject go = t.gameObject;
                        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) == 0) continue;
                        here += RemoveFrom(go);

                        int stuck = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                        if (stuck > 0)
                        {
                            left += stuck;
                            string source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                            blockedBy.Add(string.IsNullOrEmpty(source) ? path : source);
                        }
                    }

                    if (here > 0)
                    {
                        PrefabUtility.SaveAsPrefabAsset(contents, path);
                        removed += here;
                        saved++;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
            return removed;
        }

        private static int RemoveFrom(GameObject go)
        {
            try
            {
                return GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        private static void Report(int removed, int prefabsSaved, int left, HashSet<string> blockedBy, string where)
        {
            string saved = prefabsSaved > 0 ? $" Saved {prefabsSaved} prefab(s)." : "";
            if (removed == 0 && left == 0)
                Debug.Log($"[VixForge] No missing scripts found in {where}.");
            else
                Debug.Log($"[VixForge] Removed {removed} missing script(s) from {where}.{saved}");

            if (left > 0)
            {
                string list = blockedBy.Count > 0 ? "\n" + string.Join("\n", blockedBy) : "";
                Debug.LogWarning($"[VixForge] {left} missing script(s) belong to a prefab, so they can only be removed in the prefab itself. Select the prefab in the Project window and run Clean Missing Scripts > From Selection.{list}");
            }
        }
    }
}
#endif
