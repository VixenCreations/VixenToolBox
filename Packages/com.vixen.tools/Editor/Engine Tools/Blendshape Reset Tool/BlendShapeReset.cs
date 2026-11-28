#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VixenTools.Editor
{
    public static class BlendShapeReset
    {
        private const string MenuPath = "VixenTools/Unity Engine/Reset Selected Blend Shapes to Zero";
        private const string UndoName = "Reset Blend Shapes to Zero";

        [MenuItem(MenuPath)]
        public static void ResetSelected()
        {
            List<SkinnedMeshRenderer> renderers = CollectRenderers();
            if (renderers.Count == 0)
            {
                Debug.LogWarning("[VixForge] Select a mesh with blend shapes, or an avatar or object that has one under it, then try again.");
                return;
            }

            List<SkinnedMeshRenderer> changed = new List<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smr in renderers)
                if (HasNonZeroWeight(smr)) changed.Add(smr);

            if (changed.Count == 0)
            {
                Debug.Log($"[VixForge] Blend shapes are already at zero on the {renderers.Count} selected mesh(es).");
                return;
            }

            Undo.SetCurrentGroupName(UndoName);
            int group = Undo.GetCurrentGroup();
            Undo.RecordObjects(changed.ToArray(), UndoName);

            int shapes = 0;
            foreach (SkinnedMeshRenderer smr in changed)
            {
                int count = smr.sharedMesh.blendShapeCount;
                for (int i = 0; i < count; i++)
                {
                    if (smr.GetBlendShapeWeight(i) == 0f) continue;
                    smr.SetBlendShapeWeight(i, 0f);
                    shapes++;
                }
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log($"[VixForge] Reset {shapes} blend shape(s) to zero on {changed.Count} mesh(es). Undo with Ctrl+Z.");
        }

        [MenuItem(MenuPath, true)]
        private static bool ResetSelectedValidate()
        {
            return CollectRenderers().Count > 0;
        }

        private static List<SkinnedMeshRenderer> CollectRenderers()
        {
            HashSet<SkinnedMeshRenderer> found = new HashSet<SkinnedMeshRenderer>();
            foreach (GameObject go in Selection.gameObjects)
            {
                if (go == null || EditorUtility.IsPersistent(go)) continue;
                foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    if (smr.sharedMesh != null && smr.sharedMesh.blendShapeCount > 0) found.Add(smr);
            }
            return new List<SkinnedMeshRenderer>(found);
        }

        private static bool HasNonZeroWeight(SkinnedMeshRenderer smr)
        {
            int count = smr.sharedMesh.blendShapeCount;
            for (int i = 0; i < count; i++)
                if (smr.GetBlendShapeWeight(i) != 0f) return true;
            return false;
        }
    }
}
#endif
