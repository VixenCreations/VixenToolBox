#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace VixenTools.Editor
{
    [InitializeOnLoad]
    public static class SnapToSurface
    {
        private const string LIVE_SNAP_MENU = "VixenTools/Scene/Live Surface Snapping";
        private const string PRECISION_SNAP_MENU = "VixenTools/Scene/Precision Click-to-Place";
        private const string DROP_MENU_PATH = "VixenTools/Scene/Drop to Surface %&s";

        private static bool _liveSnappingEnabled;
        private static bool _precisionPlacementEnabled;

        private const int VRC_SNAP_LAYER_MASK = ~((1 << 2) | (1 << 4) | (1 << 5) | (1 << 9) | (1 << 10) | (1 << 12) | (1 << 13));

        static SnapToSurface()
        {
            _liveSnappingEnabled = EditorPrefs.GetBool(LIVE_SNAP_MENU, false);
            _precisionPlacementEnabled = EditorPrefs.GetBool(PRECISION_SNAP_MENU, false);

            EditorApplication.update += OnEditorUpdate;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        [MenuItem(LIVE_SNAP_MENU, priority = 100)]
        private static void ToggleLiveSnap()
        {
            _liveSnappingEnabled = !_liveSnappingEnabled;
            EditorPrefs.SetBool(LIVE_SNAP_MENU, _liveSnappingEnabled);
            Menu.SetChecked(LIVE_SNAP_MENU, _liveSnappingEnabled);
            if (_liveSnappingEnabled) ForceSnapSelection();
        }

        [MenuItem(LIVE_SNAP_MENU, true)]
        private static bool ValidateLiveSnap() { Menu.SetChecked(LIVE_SNAP_MENU, _liveSnappingEnabled); return true; }

        [MenuItem(PRECISION_SNAP_MENU, priority = 101)]
        private static void TogglePrecisionSnap()
        {
            _precisionPlacementEnabled = !_precisionPlacementEnabled;
            EditorPrefs.SetBool(PRECISION_SNAP_MENU, _precisionPlacementEnabled);
            Menu.SetChecked(PRECISION_SNAP_MENU, _precisionPlacementEnabled);
        }

        [MenuItem(PRECISION_SNAP_MENU, true)]
        private static bool ValidatePrecisionSnap() { Menu.SetChecked(PRECISION_SNAP_MENU, _precisionPlacementEnabled); return true; }

        [MenuItem(DROP_MENU_PATH, priority = 102)]
        public static void ForceSnapSelection()
        {
            if (Selection.transforms.Length == 0) return;
            Undo.RecordObjects(Selection.transforms, "VixenTools: Drop to Surface");
            foreach (var t in Selection.transforms) ExecuteGravitySnap(t);
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!_precisionPlacementEnabled || Selection.transforms.Length == 0) return;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.type == EventType.Layout) HandleUtility.AddDefaultControl(controlID);

            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            List<Collider> disabledColliders = new List<Collider>();
            foreach (var t in Selection.transforms)
            {
                foreach (var c in t.GetComponentsInChildren<Collider>())
                {
                    if (c.enabled) { c.enabled = false; disabledColliders.Add(c); }
                }
            }

            bool foundSurface = false;
            RaycastHit bestHit = new RaycastHit();

            RaycastHit[] hits = Physics.RaycastAll(ray, 1000f, VRC_SNAP_LAYER_MASK);
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit.collider.isTrigger) continue;
                if (hit.distance < closestDist)
                {
                    closestDist = hit.distance;
                    bestHit = hit;
                    foundSurface = true;
                }
            }

            foreach (var c in disabledColliders) if (c != null) c.enabled = true;

            if (foundSurface)
            {
                Handles.color = new Color(0f, 0.898f, 1f, 0.6f);
                Handles.DrawSolidDisc(bestHit.point, bestHit.normal, 0.04f);
                Handles.color = new Color(1f, 0f, 0.66f, 1f);
                Handles.DrawWireDisc(bestHit.point, bestHit.normal, 0.04f);
                sceneView.Repaint();

                if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 &&
                    (e.modifiers == EventModifiers.None || e.modifiers == EventModifiers.Shift))
                {
                    bool alignRotation = (e.modifiers != EventModifiers.Shift);

                    Undo.RecordObjects(Selection.transforms, "VixenTools: Precision Place");
                    foreach (var t in Selection.transforms)
                    {
                        float bottomOffset = 0f;

                        if (alignRotation)
                        {
                            Vector3 originalPos = t.position;
                            Quaternion originalRot = t.rotation;

                            t.position = Vector3.zero;
                            t.rotation = Quaternion.identity;
                            bottomOffset = CalculateFeetOffset(t);

                            t.position = originalPos;
                            t.rotation = originalRot;

                            t.rotation = Quaternion.FromToRotation(t.up, bestHit.normal) * t.rotation;

                            t.position = bestHit.point + (bestHit.normal * bottomOffset);
                        }
                        else
                        {
                            bottomOffset = CalculateFeetOffset(t);
                            t.position = new Vector3(bestHit.point.x, bestHit.point.y + bottomOffset, bestHit.point.z);
                        }

                        t.hasChanged = false;

                        var udonBehaviour = t.GetComponent("VRC.Udon.UdonBehaviour");
                        if (udonBehaviour != null) EditorUtility.SetDirty(udonBehaviour);
                    }
                    e.Use();
                }
            }
        }

        private static void OnEditorUpdate()
        {
            if (!_liveSnappingEnabled || Selection.transforms.Length == 0) return;
            if (Tools.current != Tool.Move) return;

            foreach (var t in Selection.transforms)
            {
                if (t == null) continue;
                if (t.hasChanged)
                {
                    ExecuteGravitySnap(t);
                    t.hasChanged = false;
                }
            }
        }

        private static void ExecuteGravitySnap(Transform t)
        {
            float bottomOffset = CalculateFeetOffset(t);
            int originalLayer = t.gameObject.layer;
            t.gameObject.layer = 2;

            bool foundSurface = false;
            float targetY = float.MinValue;
            float closestDist = float.MaxValue;

            Vector3 rayOrigin = new Vector3(t.position.x, t.position.y + 500f, t.position.z);
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 1000f, VRC_SNAP_LAYER_MASK);

            foreach (var hit in hits)
            {
                if (hit.collider.isTrigger) continue;
                if (hit.normal.y < 0.5f) continue;
                if (hit.collider.transform.IsChildOf(t)) continue;

                float dist = Mathf.Abs(hit.point.y - t.position.y);
                float penalty = hit.point.y > t.position.y ? 0.3f : 0f;
                dist += penalty;

                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetY = hit.point.y;
                    foundSurface = true;
                }
            }

            if (Terrain.activeTerrains != null && Terrain.activeTerrains.Length > 0)
            {
                foreach (Terrain terrain in Terrain.activeTerrains)
                {
                    Vector3 localPos = t.position - terrain.transform.position;
                    if (localPos.x >= 0 && localPos.x <= terrain.terrainData.size.x &&
                        localPos.z >= 0 && localPos.z <= terrain.terrainData.size.z)
                    {
                        float tHeight = terrain.SampleHeight(t.position) + terrain.transform.position.y;
                        float dist = Mathf.Abs(tHeight - t.position.y);
                        float penalty = tHeight > t.position.y ? 0.3f : 0f;
                        dist += penalty;

                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targetY = tHeight;
                            foundSurface = true;
                        }
                    }
                }
            }

            if (foundSurface)
            {
                Vector3 targetPos = new Vector3(t.position.x, targetY + bottomOffset, t.position.z);
                if (Vector3.Distance(t.position, targetPos) > 0.005f)
                {
                    t.position = targetPos;
                    var udonBehaviour = t.GetComponent("VRC.Udon.UdonBehaviour");
                    if (udonBehaviour != null) EditorUtility.SetDirty(udonBehaviour);
                }
            }

            t.gameObject.layer = originalLayer;
        }

        private static float CalculateFeetOffset(Transform t)
        {
            float lowestPoint = t.position.y;
            bool foundBounds = false;

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