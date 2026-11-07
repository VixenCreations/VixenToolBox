#if UNITY_EDITOR && !UDON
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.IO;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Dynamics.PhysBone.Components;
#endif

namespace VixenTools.Editor
{
    public class PhysBoneTopologyMapper : EditorWindow
    {
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/PhysBoneTopologyMapperStyles.uss";

#if VRC_SDK_VRCSDK3
        private GameObject _sourceAvatar;
        private GameObject _targetAvatar;
        private PhysBoneBlueprint _loadedBlueprint;
        private string _blueprintName = "Avatar_PhysBones";
#endif

        [MenuItem("VixenTools/Avatars/PhysBone Blueprints")]
        public static void ShowWindow()
        {
            var window = GetWindow<PhysBoneTopologyMapper>("PhysBone Blueprints");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "topology-root";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);
            else Debug.LogWarning($"[VixForge] Could not load Stylesheet at {UssPath}");

            var headerRect = new VisualElement { name = "tool-header" };
            var titleLabel = new Label("<color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> PHYSBONE BLUEPRINTS") { enableRichText = true };
            headerRect.Add(titleLabel);
            root.Add(headerRect);

            var scrollContainer = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            var scrollContent = new VisualElement();
            scrollContainer.Add(scrollContent);
            root.Add(scrollContainer);

#if VRC_SDK_VRCSDK3
            BuildExtractionUI(scrollContent);
            BuildInjectionUI(scrollContent);
#else
            BuildMissingSdkUI(scrollContent);
#endif
        }

#if VRC_SDK_VRCSDK3
        private void BuildExtractionUI(VisualElement container)
        {
            var panel = CreateCyberPanel("Phase 1: Save", "#00e5ff");

            var infoLabel = new Label("Select the root of your tuned avatar. This saves a Blueprint, plus one Preset per bone.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            var sourceField = new ObjectField("Source Avatar (Root)") { objectType = typeof(GameObject), allowSceneObjects = true, value = _sourceAvatar };
            sourceField.RegisterValueChangedCallback(e => _sourceAvatar = e.newValue as GameObject);
            panel.Add(sourceField);

            var nameField = new TextField("Blueprint Name") { value = _blueprintName };
            nameField.RegisterValueChangedCallback(e => _blueprintName = e.newValue);
            panel.Add(nameField);

            var execBtn = new Button(ExtractTopology) { text = "Save Blueprint" };
            execBtn.AddToClassList("cyber-action-btn");
            execBtn.AddToClassList("cyan-btn");
            panel.Add(execBtn);

            container.Add(panel);
        }

        private void BuildInjectionUI(VisualElement container)
        {
            var panel = CreateCyberPanel("Phase 2: Apply", "#ff00aa");

            var infoLabel = new Label("Select a blank avatar and a Blueprint. This rebuilds every PhysBone setup the Blueprint holds.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            var targetField = new ObjectField("Target Avatar (Root)") { objectType = typeof(GameObject), allowSceneObjects = true, value = _targetAvatar };
            targetField.RegisterValueChangedCallback(e => _targetAvatar = e.newValue as GameObject);
            panel.Add(targetField);

            var blueprintField = new ObjectField("Master Blueprint") { objectType = typeof(PhysBoneBlueprint), allowSceneObjects = false, value = _loadedBlueprint };
            blueprintField.RegisterValueChangedCallback(e => _loadedBlueprint = e.newValue as PhysBoneBlueprint);
            panel.Add(blueprintField);

            var execBtn = new Button(InjectTopology) { text = "Apply Blueprint" };
            execBtn.AddToClassList("cyber-action-btn");
            execBtn.AddToClassList("pink-btn");
            panel.Add(execBtn);

            container.Add(panel);
        }

        private void ExtractTopology()
        {
            if (_sourceAvatar == null)
            {
                Debug.LogError("[VixForge] Source Avatar missing.");
                return;
            }

            string baseDir = $"Assets/VixenTools/Blueprints/{_blueprintName}";
            EnsureDirectoryExists(baseDir);
            EnsureDirectoryExists($"{baseDir}/Presets");

            PhysBoneBlueprint blueprint = ScriptableObject.CreateInstance<PhysBoneBlueprint>();

            VRCPhysBone[] physBones = _sourceAvatar.GetComponentsInChildren<VRCPhysBone>(true);
            int count = 0;
            int outside = 0;
            Transform root = _sourceAvatar.transform;

            foreach (var pb in physBones)
            {
                string relativePath = AnimationUtility.CalculateTransformPath(pb.transform, root);

                Preset pbPreset = new Preset(pb);
                string cleanPathName = relativePath.Replace("/", "_");
                if (string.IsNullOrEmpty(cleanPathName)) cleanPathName = "Root";

                string presetPath = AssetDatabase.GenerateUniqueAssetPath($"{baseDir}/Presets/{cleanPathName}.preset");
                AssetDatabase.CreateAsset(pbPreset, presetPath);

                var node = new PhysBoneBlueprint.Node
                {
                    bonePath = relativePath,
                    preset = pbPreset,
                    componentIndex = System.Array.IndexOf(pb.GetComponents<VRCPhysBone>(), pb),
                    hasReferences = true,
                    hasRootTransform = pb.rootTransform != null
                };
                if (pb.rootTransform != null)
                {
                    if (pb.rootTransform.IsChildOf(root)) node.rootTransformPath = AnimationUtility.CalculateTransformPath(pb.rootTransform, root);
                    else { node.hasRootTransform = false; outside++; }
                }
                if (pb.ignoreTransforms != null)
                {
                    foreach (var t in pb.ignoreTransforms)
                    {
                        if (t == null) continue;
                        if (t.IsChildOf(root)) node.ignoreTransformPaths.Add(AnimationUtility.CalculateTransformPath(t, root));
                        else outside++;
                    }
                }
                if (pb.colliders != null)
                {
                    foreach (var c in pb.colliders)
                    {
                        if (c == null) continue;
                        if (!c.transform.IsChildOf(root)) { outside++; continue; }
                        node.colliderPaths.Add(AnimationUtility.CalculateTransformPath(c.transform, root));
                        node.colliderIndices.Add(System.Array.IndexOf(c.GetComponents<VRC.Dynamics.VRCPhysBoneColliderBase>(), c));
                    }
                }
                blueprint.nodes.Add(node);

                count++;
            }
            if (outside > 0) Debug.LogWarning($"[VixForge] {outside} PhysBone links point outside {_sourceAvatar.name}, so the blueprint leaves them out.");

            string blueprintPath = AssetDatabase.GenerateUniqueAssetPath($"{baseDir}/{_blueprintName}.asset");
            AssetDatabase.CreateAsset(blueprint, blueprintPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VixForge] Extraction Complete! Mapped {count} PhysBones to {blueprintPath}.");
        }

        private void InjectTopology()
        {
            if (_targetAvatar == null || _loadedBlueprint == null)
            {
                Debug.LogError("[VixForge] Target Avatar or Blueprint missing.");
                return;
            }

            int successCount = 0;
            int failCount = 0;
            int missingLinks = 0;
            bool oldBlueprint = false;
            Transform root = _targetAvatar.transform;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Apply PhysBone Blueprint");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (var node in _loadedBlueprint.nodes)
            {
                Transform targetBone = string.IsNullOrEmpty(node.bonePath) ? root : root.Find(node.bonePath);
                if (targetBone == null || node.preset == null)
                {
                    Debug.LogWarning($"[VixForge] Bone not found on target: {node.bonePath}. Skipping.");
                    failCount++;
                    continue;
                }

                int index = Mathf.Max(0, node.componentIndex);
                var existing = targetBone.GetComponents<VRCPhysBone>();
                while (existing.Length <= index)
                {
                    Undo.AddComponent<VRCPhysBone>(targetBone.gameObject);
                    existing = targetBone.GetComponents<VRCPhysBone>();
                }
                VRCPhysBone pb = existing[index];
                Undo.RecordObject(pb, "Apply PhysBone Blueprint");
                node.preset.ApplyTo(pb);

                if (node.hasReferences)
                {
                    pb.rootTransform = null;
                    if (node.hasRootTransform)
                    {
                        pb.rootTransform = string.IsNullOrEmpty(node.rootTransformPath) ? root : root.Find(node.rootTransformPath);
                        if (pb.rootTransform == null) missingLinks++;
                    }
                    pb.ignoreTransforms = new List<Transform>();
                    foreach (var path in node.ignoreTransformPaths)
                    {
                        var t = string.IsNullOrEmpty(path) ? root : root.Find(path);
                        if (t != null) pb.ignoreTransforms.Add(t);
                        else missingLinks++;
                    }
                    pb.colliders = new List<VRC.Dynamics.VRCPhysBoneColliderBase>();
                    for (int i = 0; i < node.colliderPaths.Count; i++)
                    {
                        var t = string.IsNullOrEmpty(node.colliderPaths[i]) ? root : root.Find(node.colliderPaths[i]);
                        var cols = t != null ? t.GetComponents<VRC.Dynamics.VRCPhysBoneColliderBase>() : new VRC.Dynamics.VRCPhysBoneColliderBase[0];
                        int ci = i < node.colliderIndices.Count ? node.colliderIndices[i] : 0;
                        if (ci >= 0 && ci < cols.Length) pb.colliders.Add(cols[ci]);
                        else missingLinks++;
                    }
                }
                else
                {
                    oldBlueprint = true;
                    if (pb.ignoreTransforms != null) pb.ignoreTransforms.RemoveAll(t => t == null);
                    if (pb.colliders != null) pb.colliders.RemoveAll(c => c == null);
                }

                if (PrefabUtility.IsPartOfPrefabInstance(pb)) PrefabUtility.RecordPrefabInstancePropertyModifications(pb);
                EditorUtility.SetDirty(pb);
                successCount++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log($"[VixForge] Injection Complete! Applied {successCount} presets. Failed/Skipped: {failCount}.");
            if (missingLinks > 0) Debug.LogWarning($"[VixForge] {missingLinks} collider, ignore or root bone links could not be found on {_targetAvatar.name}. Check those PhysBones.");
            if (oldBlueprint) Debug.LogWarning("[VixForge] This blueprint was saved by an older version, so it has no collider, ignore or root bone links. Save it again from the source avatar to keep them.");
        }
#else
        private void BuildMissingSdkUI(VisualElement container)
        {
            var panel = CreateCyberPanel("Heads Up", "#ffaa00");

            var warningLabel = new Label("VRChat SDK3 is not detected in this project. PhysBone Blueprints requires the VRChat Avatar SDK to work.");
            warningLabel.AddToClassList("warning-box-styled");
            panel.Add(warningLabel);

            var extractBtn = new Button() { text = "Save Blueprint (SDK Required)" };
            extractBtn.AddToClassList("cyber-action-btn");
            extractBtn.AddToClassList("disabled-btn");
            extractBtn.SetEnabled(false);
            panel.Add(extractBtn);

            var injectBtn = new Button() { text = "Apply Blueprint (SDK Required)" };
            injectBtn.AddToClassList("cyber-action-btn");
            injectBtn.AddToClassList("disabled-btn");
            injectBtn.SetEnabled(false);
            panel.Add(injectBtn);

            container.Add(panel);
        }
#endif

        private VisualElement CreateCyberPanel(string title, string accentColorHex)
        {
            var panel = new VisualElement();
            panel.AddToClassList("cyber-panel");

            if (!string.IsNullOrEmpty(title))
            {
                var lbl = new Label($"<color={accentColorHex}>{title}</color>") { enableRichText = true };
                lbl.AddToClassList("panel-header");
                panel.Add(lbl);

                var sep = new VisualElement();
                sep.AddToClassList("panel-separator");
                ColorUtility.TryParseHtmlString(accentColorHex, out Color c);
                c.a = 0.3f;
                sep.style.backgroundColor = c;
                panel.Add(sep);
            }
            return panel;
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif