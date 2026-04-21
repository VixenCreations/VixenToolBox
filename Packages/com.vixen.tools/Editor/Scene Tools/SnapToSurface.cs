#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Utility: Enterprise-grade surface snapping. Resolves the Event.current null-ref 
    /// from legacy Update loops by utilizing Transform.hasChanged, and calculates true mesh/collider 
    /// bounds to snap objects by their "feet" rather than their pivot.
    /// </summary>
    [InitializeOnLoad]
    public static class SnapToSurface
    {
        private const string TOGGLE_MENU_PATH = "VixenTools/Scene/Live Surface Snapping";
        private const string DROP_MENU_PATH = "VixenTools/Scene/Drop to Surface %&s"; // Ctrl+Alt+S

        private static bool _liveSnappingEnabled;

        static SnapToSurface()
        {
            _liveSnappingEnabled = EditorPrefs.GetBool(TOGGLE_MENU_PATH, false);
            
            // Track transformations gracefully without fighting the Event system
            EditorApplication.update += OnEditorUpdate;
        }

        [MenuItem(TOGGLE_MENU_PATH, priority = 100)]
        private static void ToggleLiveSnap()
        {
            _liveSnappingEnabled = !_liveSnappingEnabled;
            EditorPrefs.SetBool(TOGGLE_MENU_PATH, _liveSnappingEnabled);
            Menu.SetChecked(TOGGLE_MENU_PATH, _liveSnappingEnabled);
            
            if (_liveSnappingEnabled)
            {
                ForceSnapSelection();
            }
        }

        [MenuItem(TOGGLE_MENU_PATH, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(TOGGLE_MENU_PATH, _liveSnappingEnabled);
            return true;
        }

        [MenuItem(DROP_MENU_PATH, priority = 101)]
        public static void ForceSnapSelection()
        {
            if (Selection.transforms.Length == 0) return;
            
            Undo.RecordObjects(Selection.transforms, "VixenTools: Drop to Surface");
            foreach (var t in Selection.transforms)
            {
                ExecuteSnap(t);
            }
        }

        private static void OnEditorUpdate()
        {
            if (!_liveSnappingEnabled || Selection.transforms.Length == 0) return;

            // Limit live snapping to when the Move Tool is active to prevent fighting Rotation/Scale
            if (Tools.current != Tool.Move) return;

            foreach (var t in Selection.transforms)
            {
                if (t == null) continue;

                // hasChanged is a native, low-overhead way to detect if the matrix was modified
                if (t.hasChanged)
                {
                    ExecuteSnap(t);
                    t.hasChanged = false; // Clear the flag to prevent recursive loops
                }
            }
        }

        private static void ExecuteSnap(Transform t)
        {
            float bottomOffset = CalculateFeetOffset(t);
            
            // Cast from slightly above to allow dragging over uneven terrain/slopes
            Vector3 rayOrigin = t.position + (Vector3.up * 2.0f);
            
            // Prevent the object from raycasting against its own colliders
            int originalLayer = t.gameObject.layer;
            t.gameObject.layer = 2; // 2 is the built-in Ignore Raycast layer

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1000f))
            {
                Vector3 targetPos = hit.point + (Vector3.up * bottomOffset);
                
                // Only apply if the distance is meaningful (stops micro-jitters in the Editor loop)
                if (Vector3.Distance(t.position, targetPos) > 0.005f)
                {
                    t.position = targetPos;
                }
            }

            // Restore original layer
            t.gameObject.layer = originalLayer;
        }

        /// <summary>
        /// Calculates the distance from the Transform's pivot to the absolute bottom of its colliders/renderers.
        /// Ensures objects sit flush on the ground regardless of pivot placement.
        /// </summary>
        private static float CalculateFeetOffset(Transform t)
        {
            float lowestPoint = t.position.y;
            bool foundBounds = false;

            // 1. Prioritize Colliders (Most accurate for physical placement)
            var colliders = t.GetComponentsInChildren<Collider>(false);
            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    if (col.isTrigger) continue;
                    if (col.bounds.min.y < lowestPoint)
                    {
                        lowestPoint = col.bounds.min.y;
                        foundBounds = true;
                    }
                }
            }

            // 2. Fallback to Renderers if no physical colliders exist
            if (!foundBounds)
            {
                var renderers = t.GetComponentsInChildren<Renderer>(false);
                foreach (var rend in renderers)
                {
                    if (rend.bounds.min.y < lowestPoint)
                    {
                        lowestPoint = rend.bounds.min.y;
                        foundBounds = true;
                    }
                }
            }

            return foundBounds ? (t.position.y - lowestPoint) : 0f;
        }
    }
}
#endif