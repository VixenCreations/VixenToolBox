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
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/PhysBoneTopologyMapperStyles.uss";

        private Font _cyberFont;

#if VRC_SDK_VRCSDK3
        private GameObject _sourceAvatar;
        private GameObject _targetAvatar;
        private PhysBoneBlueprint _loadedBlueprint;
        private string _blueprintName = "Avatar_MasterTopology";
#endif

        [MenuItem("VixenTools/Avatars/PhysBone Topology Mapper")]
        public static void ShowWindow()
        {
            var window = GetWindow<PhysBoneTopologyMapper>("Topology Mapper");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "topology-root";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);
            else Debug.LogWarning($"[VixForge] Could not load Stylesheet at {UssPath}");

            var headerRect = new VisualElement { name = "tool-header" };
            var titleLabel = new Label("<color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> TOPOLOGY MAPPER") { enableRichText = true };
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
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
            var panel = CreateCyberPanel("Phase 1: Architecture Extraction", "#00e5ff");

            var infoLabel = new Label("Select the root of your tuned avatar. This generates a Master Blueprint and all associated Presets.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            var sourceField = new ObjectField("Source Avatar (Root)") { objectType = typeof(GameObject), allowSceneObjects = true, value = _sourceAvatar };
            sourceField.RegisterValueChangedCallback(e => _sourceAvatar = e.newValue as GameObject);
            panel.Add(sourceField);

            var nameField = new TextField("Blueprint Name") { value = _blueprintName };
            nameField.RegisterValueChangedCallback(e => _blueprintName = e.newValue);
            panel.Add(nameField);

            var execBtn = new Button(ExtractTopology) { text = "Extract Master Copy" };
            execBtn.AddToClassList("cyber-action-btn");
            execBtn.AddToClassList("cyan-btn");
            panel.Add(execBtn);

            container.Add(panel);
        }

        private void BuildInjectionUI(VisualElement container)
        {
            var panel = CreateCyberPanel("Phase 2: Architecture Injection", "#ff00aa");

            var infoLabel = new Label("Select a blank avatar and a Blueprint. This will reconstruct your master physics system.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            var targetField = new ObjectField("Target Avatar (Root)") { objectType = typeof(GameObject), allowSceneObjects = true, value = _targetAvatar };
            targetField.RegisterValueChangedCallback(e => _targetAvatar = e.newValue as GameObject);
            panel.Add(targetField);

            var blueprintField = new ObjectField("Master Blueprint") { objectType = typeof(PhysBoneBlueprint), allowSceneObjects = false, value = _loadedBlueprint };
            blueprintField.RegisterValueChangedCallback(e => _loadedBlueprint = e.newValue as PhysBoneBlueprint);
            panel.Add(blueprintField);

            var execBtn = new Button(InjectTopology) { text = "Inject Blueprint" };
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

            foreach (var pb in physBones)
            {
                string relativePath = AnimationUtility.CalculateTransformPath(pb.transform, _sourceAvatar.transform);

                Preset pbPreset = new Preset(pb);
                string cleanPathName = relativePath.Replace("/", "_");
                if (string.IsNullOrEmpty(cleanPathName)) cleanPathName = "Root";

                string presetPath = AssetDatabase.GenerateUniqueAssetPath($"{baseDir}/Presets/{cleanPathName}.preset");
                AssetDatabase.CreateAsset(pbPreset, presetPath);

                blueprint.nodes.Add(new PhysBoneBlueprint.Node {
                    bonePath = relativePath,
                    preset = pbPreset
                });

                count++;
            }

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

            foreach (var node in _loadedBlueprint.nodes)
            {
                Transform targetBone = _targetAvatar.transform.Find(node.bonePath);

                if (string.IsNullOrEmpty(node.bonePath))
                    targetBone = _targetAvatar.transform;

                if (targetBone != null)
                {
                    VRCPhysBone pb = targetBone.GetComponent<VRCPhysBone>();
                    if (pb == null)
                    {
                        pb = targetBone.gameObject.AddComponent<VRCPhysBone>();
                    }

                    node.preset.ApplyTo(pb);
                    successCount++;
                }
                else
                {
                    Debug.LogWarning($"[VixForge] Bone not found on target: {node.bonePath}. Skipping.");
                    failCount++;
                }
            }

            EditorUtility.SetDirty(_targetAvatar);
            Debug.Log($"[VixForge] Injection Complete! Applied {successCount} presets. Failed/Skipped: {failCount}.");
        }
#else
        private void BuildMissingSdkUI(VisualElement container)
        {
            var panel = CreateCyberPanel("System Alert", "#ffaa00");

            var warningLabel = new Label("VRChat SDK3 is not detected in this project. The PhysBone Topology Mapper requires the VRChat Avatar SDK to function.");
            warningLabel.AddToClassList("warning-box-styled");
            panel.Add(warningLabel);

            var extractBtn = new Button() { text = "Extract Master Copy (SDK Required)" };
            extractBtn.AddToClassList("cyber-action-btn");
            extractBtn.AddToClassList("disabled-btn");
            extractBtn.SetEnabled(false);
            panel.Add(extractBtn);

            var injectBtn = new Button() { text = "Inject Blueprint (SDK Required)" };
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