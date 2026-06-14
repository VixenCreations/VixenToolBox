#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Animations;
using ImageMagick;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Avatars.Components;
#endif

namespace VixenTools.Editor
{
    public class QuestConversionEngine : EditorWindow
    {
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/QuestConversionEngineStyles.uss";

        private Font _cyberFont;
        private GameObject _sourceAvatar;

        private int _totalTriangles = 0;
        private int _skinnedMeshCount = 0;
        private int _basicMeshCount = 0;
        private int _materialSlotCount = 0;
        private int _uniqueMaterialCount = 0;
        private int _uniqueTextureCount = 0;
        private bool _hasScanned = false;

        private enum TargetQuestShader { VRCMobileToonStandard, VRCMobileToonLit, VRCMobileStandard, VRCMobileMatcap, UnityMobileStandard }
        private TargetQuestShader _selectedTargetShader = TargetQuestShader.VRCMobileToonStandard;

        private List<string> _textureSizeLabels = new List<string> { "256 (Aggressive - 10MB)", "512 (Balanced - 18MB)", "1024 (Standard - 40MB)", "2048 (Heavy - Very Poor)" };
        private int[] _textureSizeOptions = { 256, 512, 1024, 2048 };
        private int _selectedTextureSizeIndex = 2;

        private enum MobilePerformanceRank { Excellent, Good, Medium, Poor }
        private MobilePerformanceRank _targetPerformanceRank = MobilePerformanceRank.Poor;

        private const string BASE_OUTPUT_DIR = "Assets/VixenTools/QuestConversion";
        private Dictionary<Texture, Texture> _textureCache = new Dictionary<Texture, Texture>();
        private HashSet<Material> _scannedMaterials = new HashSet<Material>();
        private string _activeTexturesDir;

        private class TopologyNode
        {
            public Component component;
            public string relativePath;
            public int depth;
            public bool keep;
            public bool isLocked;
        }

        private class TextureNode
        {
            public Texture texture;
            public bool processTexture;
            public string name;
            public int width;
            public int height;
        }

        private List<TopologyNode> _scannedAnimators = new List<TopologyNode>();

        private List<TopologyNode> _scannedPhysBones = new List<TopologyNode>();
        private List<TopologyNode> _scannedColliders = new List<TopologyNode>();
        private List<TopologyNode> _scannedContacts = new List<TopologyNode>();
        private List<TopologyNode> _scannedConstraints = new List<TopologyNode>();
        private List<TopologyNode> _scannedRaycasts = new List<TopologyNode>();

        private List<TopologyNode> _scannedParticles = new List<TopologyNode>();
        private List<TopologyNode> _scannedTrails = new List<TopologyNode>();
        private List<TopologyNode> _scannedLines = new List<TopologyNode>();
        private List<TopologyNode> _scannedJoints = new List<TopologyNode>();
        private List<TopologyNode> _scannedIncompatible = new List<TopologyNode>();
        private List<TopologyNode> _scannedFaceTracking = new List<TopologyNode>();

        private List<TextureNode> _scannedTextures = new List<TextureNode>();

        private VisualElement _dynamicContainer;
        private ScrollView _pbScroll, _colScroll, _contactScroll, _constraintScroll, _raycastScroll, _animatorScroll, _particleScroll, _trailScroll, _lineScroll, _jointScroll, _incompatibleScroll, _textureScroll, _ftScroll;
        private Label _pbLabel, _colLabel, _contactLabel, _constraintLabel, _raycastLabel, _animatorLabel, _particleLabel, _trailLabel, _lineLabel, _jointLabel, _incompatibleLabel, _textureLabel, _ftLabel;

        [MenuItem("VixenTools/Avatars/Quest Conversion Engine", priority = 41)]
        public static void ShowWindow()
        {
            var window = GetWindow<QuestConversionEngine>("Quest Engine");
            window.minSize = new Vector2(500, 750);
            window.Show();
        }

        private void OnEnable() => _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "quest-engine-root";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            var headerRect = new VisualElement { name = "tool-header" };
            var titleLabel = new Label("<color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> QUEST ENGINE") { enableRichText = true };
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            headerRect.Add(titleLabel);
            root.Add(headerRect);

            var mainScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            var scrollContent = new VisualElement();
            mainScroll.Add(scrollContent);
            root.Add(mainScroll);

            BuildPhase1UI(scrollContent);

            _dynamicContainer = new VisualElement();
            scrollContent.Add(_dynamicContainer);
        }

        private void BuildPhase1UI(VisualElement container)
        {
            var panel = CreateCyberPanel("Phase 1: Deep System Scanning", "#00e5ff");

            var infoLabel = new Label("Select the root of your PC Avatar. The engine maps 100% of VRChat Mobile Performance caps and non-destructively isolates a Quest clone.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            var sourceField = new ObjectField("Source Avatar (Root)") { objectType = typeof(GameObject), allowSceneObjects = true, value = _sourceAvatar };
            sourceField.RegisterValueChangedCallback(e =>
            {
                _sourceAvatar = e.newValue as GameObject;
                _hasScanned = false;
                _dynamicContainer.Clear();
            });
            panel.Add(sourceField);

            var shaderEnum = new EnumField("Target Mobile Shader", _selectedTargetShader);
            shaderEnum.RegisterValueChangedCallback(e => _selectedTargetShader = (TargetQuestShader)e.newValue);
            panel.Add(shaderEnum);

            var texDropdown = new DropdownField("Max Texture Resolution", _textureSizeLabels, _selectedTextureSizeIndex);
            texDropdown.RegisterValueChangedCallback(e => _selectedTextureSizeIndex = _textureSizeLabels.IndexOf(e.newValue));
            panel.Add(texDropdown);

            var rankEnum = new EnumField("Target Performance Rank", _targetPerformanceRank);
            rankEnum.RegisterValueChangedCallback(e =>
            {
                _targetPerformanceRank = (MobilePerformanceRank)e.newValue;
                if (_hasScanned)
                {
                    ApplyHeuristicCullingRules();
                    RefreshTopologyUI();
                }
            });
            panel.Add(rankEnum);

            var scanBtn = new Button(AnalyzeHierarchy) { text = "Scan System Architecture" };
            scanBtn.AddToClassList("cyber-action-btn");
            scanBtn.AddToClassList("cyan-btn");
            panel.Add(scanBtn);

            container.Add(panel);
        }

        private void BuildDynamicResultsUI()
        {
            _dynamicContainer.Clear();

            var resultsBox = new VisualElement();
            resultsBox.AddToClassList("results-box");

            string triColor = _totalTriangles > 20000 ? "#ff0044" : "#00ff66";
            string smrColor = _skinnedMeshCount > 2 ? "#ff0044" : "#00ff66";
            string matColor = _materialSlotCount > 4 ? "#ff0044" : "#00ff66";

            string resultsText = $"<b><color=#00e5ff>■</color> HARD CAP ANALYSIS:</b>\n" +
                             $"  • Total Triangles: <color={triColor}><b>{_totalTriangles:N0}</b></color> {(_totalTriangles > 20000 ? "(Exceeds Mobile 'Poor' Limit)" : "")}\n" +
                             $"  • Skinned Meshes: <color={smrColor}><b>{_skinnedMeshCount}</b></color>\n" +
                             $"  • Basic Meshes: <b><color=#00ff66>{_basicMeshCount}</color></b>\n" +
                             $"  • Material Slots: <color={matColor}><b>{_materialSlotCount}</b></color>\n\n" +
                             $"<b><color=#ff00aa>■</color> COMPONENT SCANNERS:</b>\n" +
                             $"  • Unique PC Materials: <b><color=#ff00aa>{_uniqueMaterialCount}</color></b>\n" +
                             $"  • Unique PC Textures: <b><color=#ff00aa>{_uniqueTextureCount}</color></b>\n" +
                             $"  • Animators: <b><color=#ff00aa>{_scannedAnimators.Count}</color></b>\n";

#if VRC_SDK_VRCSDK3
            resultsText += $"  • PhysBones: <b><color=#ff00aa>{_scannedPhysBones.Count}</color></b>  |  Colliders: <b><color=#ff00aa>{_scannedColliders.Count}</color></b>\n" +
                           $"  • Contacts: <b><color=#ff00aa>{_scannedContacts.Count}</color></b>  |  Constraints: <b><color=#ff00aa>{_scannedConstraints.Count}</color></b>\n" +
                           $"  • Raycasts: <b><color=#ff00aa>{_scannedRaycasts.Count}</color></b>\n";
#endif

            resultsText += $"  • Particles/Trails: <b><color=#ff00aa>{_scannedParticles.Count + _scannedTrails.Count + _scannedLines.Count}</color></b>\n" +
                           $"  • Physics Joints (Auto-Culled): <b><color=#ff0044>{_scannedJoints.Count}</color></b>\n" +
                           $"  • Incompatible Mobile Objects: <b><color=#ff0044>{_scannedIncompatible.Count}</color></b>\n" +
                           $"  • Face Tracking Nodes (Auto-Culled): <b><color=#ff0044>{_scannedFaceTracking.Count}</color></b>";

            var resultsLabel = new Label(resultsText) { enableRichText = true };
            resultsBox.Add(resultsLabel);
            _dynamicContainer.Add(resultsBox);

            BuildTopologyUI(_dynamicContainer);

            var execPanel = CreateCyberPanel("Phase 2: Deep Conversion Pipeline", "#ff00aa");
            var execBtn = new Button(ExecuteConversion) { text = "Execute Full Quest Conversion" };
            execBtn.AddToClassList("cyber-action-btn");
            execBtn.AddToClassList("pink-btn");

            if (_sourceAvatar == null || _uniqueMaterialCount == 0)
            {
                execBtn.SetEnabled(false);
                execBtn.AddToClassList("disabled-btn");
                execBtn.text = "Execution Locked (No Data)";
            }

            execPanel.Add(execBtn);
            _dynamicContainer.Add(execPanel);
        }

        private void BuildTopologyUI(VisualElement container)
        {
            var panel = CreateCyberPanel("Phase 1.5: Interactive Topology System", "#00e5ff");

            var infoLabel = new Label("VRChat limits are mathematically applied based on Target Rank. You may override kept nodes. ImageMagick conversions can be individually disabled to save time.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            BuildTextureSection(panel, "System: Texture Map Selection", true, out _textureScroll, out _textureLabel);

            BuildTopologySection(panel, "System: Animators", false, out _animatorScroll, out _animatorLabel);

#if VRC_SDK_VRCSDK3
            BuildTopologySection(panel, "System: PhysBones", false, out _pbScroll, out _pbLabel);
            BuildTopologySection(panel, "System: Colliders", false, out _colScroll, out _colLabel);
            BuildTopologySection(panel, "System: Contacts", false, out _contactScroll, out _contactLabel);
            BuildTopologySection(panel, "System: Constraints", false, out _constraintScroll, out _constraintLabel);
            BuildTopologySection(panel, "System: Raycasts", false, out _raycastScroll, out _raycastLabel);
#endif
            BuildTopologySection(panel, "System: Particle Systems", false, out _particleScroll, out _particleLabel);
            BuildTopologySection(panel, "System: Trail Renderers", false, out _trailScroll, out _trailLabel);
            BuildTopologySection(panel, "System: Line Renderers", false, out _lineScroll, out _lineLabel);
            BuildTopologySection(panel, "System: Face Tracking & VRCFT (Auto-Culled)", false, out _ftScroll, out _ftLabel);
            BuildTopologySection(panel, "System: Physics Joints (Auto-Culled)", false, out _jointScroll, out _jointLabel);
            BuildTopologySection(panel, "Incompatible Mobile Components (Auto-Culled)", false, out _incompatibleScroll, out _incompatibleLabel);

            container.Add(panel);
            RefreshTopologyUI();
            RefreshTextureUI();
        }

        private void BuildTopologySection(VisualElement parent, string title, bool startOpen, out ScrollView scrollView, out Label countLabel)
        {
            var foldout = new Foldout { text = title, value = startOpen };
            foldout.AddToClassList("bold-foldout");

            countLabel = new Label() { enableRichText = true };
            countLabel.style.fontSize = 14;
            countLabel.style.marginBottom = 10;
            foldout.Add(countLabel);

            scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.maxHeight = 200;
            scrollView.AddToClassList("topology-scroll-view");
            foldout.Add(scrollView);

            parent.Add(foldout);
        }

        private void BuildTextureSection(VisualElement parent, string title, bool startOpen, out ScrollView scrollView, out Label countLabel)
        {
            var foldout = new Foldout { text = title, value = startOpen };
            foldout.AddToClassList("bold-foldout");

            countLabel = new Label() { enableRichText = true };
            countLabel.style.fontSize = 14;
            countLabel.style.marginBottom = 10;
            foldout.Add(countLabel);

            var controlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 5 } };
            var btnSelectAll = new Button(() => { _scannedTextures.ForEach(t => t.processTexture = true); RefreshTextureUI(); }) { text = "Select All", style = { flexGrow = 1 } };
            var btnDeselectAll = new Button(() => { _scannedTextures.ForEach(t => t.processTexture = false); RefreshTextureUI(); }) { text = "Deselect All", style = { flexGrow = 1 } };
            controlRow.Add(btnSelectAll); controlRow.Add(btnDeselectAll);
            foldout.Add(controlRow);

            scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.maxHeight = 250;
            scrollView.AddToClassList("topology-scroll-view");
            foldout.Add(scrollView);

            parent.Add(foldout);
        }

        private void RefreshTopologyUI()
        {
            UpdateTopologyList(_scannedAnimators, GetMaxAnimators(), _animatorScroll, _animatorLabel);
#if VRC_SDK_VRCSDK3
            UpdateTopologyList(_scannedPhysBones, GetMaxPhysBones(), _pbScroll, _pbLabel);
            UpdateTopologyList(_scannedColliders, GetMaxColliders(), _colScroll, _colLabel);
            UpdateTopologyList(_scannedContacts, GetMaxContacts(), _contactScroll, _contactLabel);
            UpdateTopologyList(_scannedConstraints, GetMaxConstraints(), _constraintScroll, _constraintLabel);
            UpdateTopologyList(_scannedRaycasts, GetMaxRaycasts(), _raycastScroll, _raycastLabel);
#endif
            UpdateTopologyList(_scannedParticles, GetMaxParticles(), _particleScroll, _particleLabel);
            UpdateTopologyList(_scannedTrails, GetMaxTrails(), _trailScroll, _trailLabel);
            UpdateTopologyList(_scannedLines, GetMaxLines(), _lineScroll, _lineLabel);
            UpdateTopologyList(_scannedFaceTracking, 0, _ftScroll, _ftLabel);
            UpdateTopologyList(_scannedJoints, 0, _jointScroll, _jointLabel);
            UpdateTopologyList(_scannedIncompatible, 0, _incompatibleScroll, _incompatibleLabel);
        }

        private void RefreshTextureUI()
        {
            if (_textureScroll == null || _textureLabel == null) return;

            int activeCount = _scannedTextures.Count(t => t.processTexture);
            _textureLabel.text = $"Queued for ImageMagick Processing: <b><color=#00ff66>{activeCount} / {_scannedTextures.Count}</color></b>";

            _textureScroll.Clear();

            if (_scannedTextures.Count == 0)
            {
                _textureScroll.Add(new Label("  <i>No textures detected.</i>") { style = { color = new Color(0.5f, 0.5f, 0.5f) } });
                return;
            }

            foreach (var node in _scannedTextures)
            {
                var row = new VisualElement();
                row.AddToClassList("topology-row");

                var toggle = new Toggle { value = node.processTexture };
                toggle.RegisterValueChangedCallback(e =>
                {
                    node.processTexture = e.newValue;
                    int newCount = _scannedTextures.Count(t => t.processTexture);
                    _textureLabel.text = $"Queued for ImageMagick Processing: <b><color=#00ff66>{newCount} / {_scannedTextures.Count}</color></b>";
                });
                row.Add(toggle);

                string displayInfo = $"<b><color=#00e5ff>{node.name}</color></b> <color=#aaaaaa><i>({node.width}x{node.height})</i></color>";

                var label = new Label(displayInfo) { enableRichText = true };
                label.AddToClassList("topology-label");
                row.Add(label);

                _textureScroll.Add(row);
            }
        }

        private void UpdateTopologyList(List<TopologyNode> nodes, int limit, ScrollView scrollView, Label countLabel)
        {
            if (scrollView == null || countLabel == null) return;

            int kept = nodes.Count(n => n.keep);
            Color countColor = kept > limit ? Color.red : new Color(0.2f, 0.8f, 0.2f);

            if (limit == 0 && nodes.Count > 0 && nodes[0].isLocked && !nodes[0].keep)
                countLabel.text = $"Selected: <color=#ff0044><b>0 / 0</b></color> (Hard Mobile Limitation)";
            else
                countLabel.text = $"Selected: <color=#{ColorUtility.ToHtmlStringRGB(countColor)}><b>{kept} / {limit}</b></color> Allowed";

            scrollView.Clear();
            if (nodes.Count == 0)
            {
                scrollView.Add(new Label("  <i>No components detected in this category.</i>") { style = { color = new Color(0.5f, 0.5f, 0.5f) } });
                return;
            }

            foreach (var node in nodes)
            {
                var row = new VisualElement();
                row.AddToClassList("topology-row");

                var toggle = new Toggle { value = node.keep };
                if (node.isLocked)
                {
                    toggle.SetEnabled(false);
                    if (!node.keep) toggle.AddToClassList("locked-culled-toggle");
                    else toggle.AddToClassList("locked-kept-toggle");
                }
                else
                {
                    toggle.RegisterValueChangedCallback(e =>
                    {
                        node.keep = e.newValue;
                        int currentKept = nodes.Count(n => n.keep);
                        Color c = currentKept > limit ? Color.red : new Color(0.2f, 0.8f, 0.2f);
                        countLabel.text = $"Selected: <color=#{ColorUtility.ToHtmlStringRGB(c)}><b>{currentKept} / {limit}</b></color> Allowed";
                    });
                }
                row.Add(toggle);

                string displayPath = node.relativePath;
                int lastSlash = displayPath.LastIndexOf('/');

                string hexColor = (node.isLocked && !node.keep) ? "#ff0044" : "#00ff66";

                if (lastSlash >= 0 && lastSlash < displayPath.Length - 1)
                {
                    string baseDir = displayPath.Substring(0, lastSlash + 1);
                    string boneName = displayPath.Substring(lastSlash + 1);
                    displayPath = $"<color={hexColor}>{baseDir}</color><b><color={hexColor}>{boneName}</color></b>";
                }
                else if (string.IsNullOrEmpty(displayPath)) displayPath = $"<b><color={hexColor}>[Avatar Root]</color></b>";
                else displayPath = $"<b><color={hexColor}>{displayPath}</color></b>";

                string typeName = node.component != null ? node.component.GetType().Name : "Unknown";
                displayPath += $" <color=#aaaaaa><i>({typeName})</i></color>";

                var label = new Label(displayPath) { enableRichText = true };
                label.AddToClassList("topology-label");
                row.Add(label);

                scrollView.Add(row);
            }
        }

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

        private void AnalyzeHierarchy()
        {
            if (_sourceAvatar == null) return;

            _totalTriangles = 0;
            _skinnedMeshCount = 0;
            _basicMeshCount = 0;
            _materialSlotCount = 0;
            _scannedMaterials.Clear();

            foreach (var smr in _sourceAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                _skinnedMeshCount++;
                _materialSlotCount += smr.sharedMaterials.Length;
                if (smr.sharedMesh != null) _totalTriangles += smr.sharedMesh.triangles.Length / 3;
                foreach (var mat in smr.sharedMaterials) if (mat != null) _scannedMaterials.Add(mat);
            }

            foreach (var mf in _sourceAvatar.GetComponentsInChildren<MeshFilter>(true))
            {
                _basicMeshCount++;
                Renderer rend = mf.GetComponent<Renderer>();
                if (rend != null)
                {
                    _materialSlotCount += rend.sharedMaterials.Length;
                    foreach (var mat in rend.sharedMaterials) if (mat != null) _scannedMaterials.Add(mat);
                }
                if (mf.sharedMesh != null) _totalTriangles += mf.sharedMesh.triangles.Length / 3;
            }

            foreach (var animator in _sourceAvatar.GetComponentsInChildren<Animator>(true))
            {
                if (animator.runtimeAnimatorController != null)
                {
                    var deps = EditorUtility.CollectDependencies(new UnityEngine.Object[] { animator.runtimeAnimatorController });
                    foreach (var dep in deps)
                    {
                        if (dep is Material mat) _scannedMaterials.Add(mat);
                    }
                }
            }

            foreach (var mono in _sourceAvatar.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono != null && mono.GetType().Name.Contains("VRCFury"))
                {
                    var deps = EditorUtility.CollectDependencies(new UnityEngine.Object[] { mono });
                    foreach (var dep in deps)
                    {
                        if (dep is Material mat) _scannedMaterials.Add(mat);
                    }
                }
            }

            _uniqueMaterialCount = _scannedMaterials.Count;

            HashSet<Texture> uniqueTexs = new HashSet<Texture>();
            string[] texPropsToScan = {
                "_MainTex", "_BaseMap", "_EmissionMap",
                "_BumpMap", "_DetailNormalMap",
                "_MetallicGlossMap", "_MetallicMap", "_SpecGlossMap",
                "_MochieMetallicMap", "_MochieMetallicMaps"
            };

            foreach (var mat in _scannedMaterials)
            {
                foreach (var prop in texPropsToScan)
                {
                    if (mat.HasProperty(prop) && mat.GetTexture(prop) != null)
                        uniqueTexs.Add(mat.GetTexture(prop));
                }
            }

            _uniqueTextureCount = uniqueTexs.Count;

            _scannedTextures.Clear();
            foreach(var tex in uniqueTexs)
            {
                _scannedTextures.Add(new TextureNode {
                    texture = tex,
                    name = tex.name,
                    width = tex.width,
                    height = tex.height,
                    processTexture = true
                });
            }

            _scannedAnimators.Clear(); _scannedParticles.Clear(); _scannedTrails.Clear(); _scannedLines.Clear();
            _scannedJoints.Clear(); _scannedIncompatible.Clear(); _scannedFaceTracking.Clear();

            foreach (var t in _sourceAvatar.GetComponentsInChildren<Transform>(true))
            {
                bool isFaceTracking = false;

#if UNITY_EDITOR
                string prefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject));
                if (!string.IsNullOrEmpty(prefabPath) && prefabPath.Contains("adjerry91.vrcft.templates"))
                {
                    isFaceTracking = true;
                }
#endif

                if (!isFaceTracking)
                {
                    string goName = t.gameObject.name;
                    if (goName.Contains("VRCFury - Face Tracking") || goName.Contains("VF_UE_VRCFT"))
                    {
                        isFaceTracking = true;
                    }
                }

                if (!isFaceTracking)
                {
                    foreach (var comp in t.GetComponents<MonoBehaviour>())
                    {
                        if (comp == null) continue;
                        string typeName = comp.GetType().Name.ToLower();
                        string nameSpace = comp.GetType().Namespace?.ToLower() ?? "";

                        if (typeName.Contains("vrcft") ||
                            typeName.Contains("facetracking") ||
                            typeName.Contains("vf_ue_vrcft") ||
                            nameSpace.Contains("adjerry91"))
                        {
                            isFaceTracking = true;
                            break;
                        }
                    }
                }

                if (isFaceTracking)
                {
                    _scannedFaceTracking.Add(new TopologyNode {
                        component = t,
                        relativePath = AnimationUtility.CalculateTransformPath(t, _sourceAvatar.transform),
                        depth = GetHierarchyDepth(t),
                        keep = false,
                        isLocked = true
                    });
                }
            }

            foreach (var anim in _sourceAvatar.GetComponentsInChildren<Animator>(true))
            {
                bool isRoot = anim.transform == _sourceAvatar.transform;
                _scannedAnimators.Add(new TopologyNode {
                    component = anim,
                    relativePath = AnimationUtility.CalculateTransformPath(anim.transform, _sourceAvatar.transform),
                    depth = GetHierarchyDepth(anim.transform),
                    keep = true,
                    isLocked = isRoot
                });
            }

            foreach (var ps in _sourceAvatar.GetComponentsInChildren<ParticleSystem>(true)) _scannedParticles.Add(CreateNode(ps, false));
            foreach (var tr in _sourceAvatar.GetComponentsInChildren<TrailRenderer>(true)) _scannedTrails.Add(CreateNode(tr, false));
            foreach (var lr in _sourceAvatar.GetComponentsInChildren<LineRenderer>(true)) _scannedLines.Add(CreateNode(lr, false));
            foreach (var l in _sourceAvatar.GetComponentsInChildren<Light>(true)) _scannedIncompatible.Add(CreateNode(l, true));
            foreach (var c in _sourceAvatar.GetComponentsInChildren<Cloth>(true)) _scannedIncompatible.Add(CreateNode(c, true));
            foreach (var r in _sourceAvatar.GetComponentsInChildren<Rigidbody>(true)) _scannedIncompatible.Add(CreateNode(r, true));
            foreach (var j in _sourceAvatar.GetComponentsInChildren<Joint>(true)) _scannedJoints.Add(CreateNode(j, true));
            foreach (var a in _sourceAvatar.GetComponentsInChildren<AudioSource>(true)) _scannedIncompatible.Add(CreateNode(a, true));
            foreach (var cam in _sourceAvatar.GetComponentsInChildren<Camera>(true)) _scannedIncompatible.Add(CreateNode(cam, true));
            foreach (var col in _sourceAvatar.GetComponentsInChildren<Collider>(true)) _scannedIncompatible.Add(CreateNode(col, true));

#if VRC_SDK_VRCSDK3
            _scannedPhysBones.Clear(); _scannedColliders.Clear(); _scannedContacts.Clear(); _scannedConstraints.Clear(); _scannedRaycasts.Clear();

            foreach (var pb in _sourceAvatar.GetComponentsInChildren<VRCPhysBone>(true)) _scannedPhysBones.Add(CreateNode(pb, false));
            foreach (var col in _sourceAvatar.GetComponentsInChildren<VRCPhysBoneCollider>(true)) _scannedColliders.Add(CreateNode(col, false));
            foreach (var sender in _sourceAvatar.GetComponentsInChildren<VRCContactSender>(true)) _scannedContacts.Add(CreateNode(sender, false));
            foreach (var receiver in _sourceAvatar.GetComponentsInChildren<VRCContactReceiver>(true)) _scannedContacts.Add(CreateNode(receiver, false));
            foreach (var raycast in _sourceAvatar.GetComponentsInChildren<VRCRaycast>(true)) _scannedRaycasts.Add(CreateNode(raycast, false));
            foreach (var constraint in _sourceAvatar.GetComponentsInChildren<IConstraint>(true))
            {
                if (constraint as Component != null) _scannedConstraints.Add(CreateNode(constraint as Component, false));
            }
#endif

            ApplyHeuristicCullingRules();

            _hasScanned = true;
            BuildDynamicResultsUI();
            Debug.Log($"[VixForge] Deep System Scan Complete. {_scannedMaterials.Count} unique materials extracted including hidden nodes.");
        }

        private TopologyNode CreateNode(Component comp, bool lockedPurge)
        {
            return new TopologyNode {
                component = comp,
                relativePath = AnimationUtility.CalculateTransformPath(comp.transform, _sourceAvatar.transform),
                depth = GetHierarchyDepth(comp.transform),
                keep = !lockedPurge,
                isLocked = lockedPurge
            };
        }

        private void ApplyHeuristicCullingRules()
        {
            ApplyDepthCulling(_scannedAnimators, GetMaxAnimators());
#if VRC_SDK_VRCSDK3
            ApplyDepthCulling(_scannedPhysBones, GetMaxPhysBones());
            ApplyDepthCulling(_scannedColliders, GetMaxColliders());
            ApplyDepthCulling(_scannedContacts, GetMaxContacts());
            ApplyDepthCulling(_scannedConstraints, GetMaxConstraints());
            ApplyDepthCulling(_scannedRaycasts, GetMaxRaycasts());
#endif
            ApplyDepthCulling(_scannedParticles, GetMaxParticles());
            ApplyDepthCulling(_scannedTrails, GetMaxTrails());
            ApplyDepthCulling(_scannedLines, GetMaxLines());
            ApplyDepthCulling(_scannedFaceTracking, 0);
            ApplyDepthCulling(_scannedJoints, 0);
            ApplyDepthCulling(_scannedIncompatible, 0);
        }

        private void ApplyDepthCulling(List<TopologyNode> nodes, int maxAllowed)
        {
            nodes.Sort((a, b) => a.depth.CompareTo(b.depth));
            int currentKept = 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].isLocked)
                {
                    if (nodes[i].keep) currentKept++;
                }
                else
                {
                    if (currentKept < maxAllowed)
                    {
                        nodes[i].keep = true;
                        currentKept++;
                    }
                    else
                    {
                        nodes[i].keep = false;
                    }
                }
            }
        }

        private int GetMaxAnimators() => _targetPerformanceRank == MobilePerformanceRank.Poor ? 2 : 1;
        private int GetMaxParticles() => _targetPerformanceRank == MobilePerformanceRank.Poor ? 2 : 0;
        private int GetMaxTrails() => _targetPerformanceRank == MobilePerformanceRank.Poor ? 1 : 0;
        private int GetMaxLines() => _targetPerformanceRank == MobilePerformanceRank.Poor ? 1 : 0;

#if VRC_SDK_VRCSDK3
        private int GetMaxPhysBones() { switch (_targetPerformanceRank) { case MobilePerformanceRank.Excellent: return 0; case MobilePerformanceRank.Good: return 4; case MobilePerformanceRank.Medium: return 6; default: return 8; } }
        private int GetMaxColliders() { switch (_targetPerformanceRank) { case MobilePerformanceRank.Excellent: return 0; case MobilePerformanceRank.Good: return 4; case MobilePerformanceRank.Medium: return 8; default: return 16; } }
        private int GetMaxContacts() { switch (_targetPerformanceRank) { case MobilePerformanceRank.Excellent: return 2; case MobilePerformanceRank.Good: return 4; case MobilePerformanceRank.Medium: return 8; default: return 16; } }
        private int GetMaxConstraints() { switch (_targetPerformanceRank) { case MobilePerformanceRank.Excellent: return 30; case MobilePerformanceRank.Good: return 60; case MobilePerformanceRank.Medium: return 120; default: return 150; } }
        private int GetMaxRaycasts() { switch (_targetPerformanceRank) { case MobilePerformanceRank.Excellent: return 1; case MobilePerformanceRank.Good: return 2; case MobilePerformanceRank.Medium: return 4; default: return 8; } }
#endif

        private int GetHierarchyDepth(Transform t)
        {
            int depth = 0;
            while (t.parent != null) { depth++; t = t.parent; }
            return depth;
        }

        private void ExecuteConversion()
        {
            if (_sourceAvatar == null) return;

            try
            {
                EditorUtility.DisplayProgressBar("VixForge Quest Engine", "Initializing Directory Structures...", 0.1f);

                string avatarName = _sourceAvatar.name;
                string questName = $"Quest_{avatarName}";

                EnsureDirectoryExists(BASE_OUTPUT_DIR);
                string avatarDir = $"{BASE_OUTPUT_DIR}/{questName}";
                EnsureDirectoryExists(avatarDir);

                string materialsDir = $"{avatarDir}/Materials";
                EnsureDirectoryExists(materialsDir);

                _activeTexturesDir = $"{avatarDir}/Textures";
                EnsureDirectoryExists(_activeTexturesDir);

                _textureCache.Clear();

                EditorUtility.DisplayProgressBar("VixForge Quest Engine", "Generating Prefab Sandbox...", 0.2f);
                string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{avatarDir}/{avatarName}_Base.prefab");
                GameObject tempPrefab = PrefabUtility.SaveAsPrefabAsset(_sourceAvatar, prefabPath);

                Undo.RecordObject(_sourceAvatar, "Disable PC Avatar");
                _sourceAvatar.SetActive(false);

                GameObject questClone = (GameObject)PrefabUtility.InstantiatePrefab(tempPrefab);
                questClone.name = questName;
                questClone.transform.position = _sourceAvatar.transform.position;
                questClone.transform.rotation = _sourceAvatar.transform.rotation;
                questClone.transform.parent = _sourceAvatar.transform.parent;

                EditorUtility.DisplayProgressBar("VixForge Quest Engine", "Cloning and Converting ALL System Materials...", 0.4f);
                Dictionary<Material, Material> materialCache = new Dictionary<Material, Material>();
                Shader targetShader = GetShaderForEnum(_selectedTargetShader);

                int matIndex = 0;
                foreach (Material originalMat in _scannedMaterials)
                {
                    if (originalMat == null) continue;

                    EditorUtility.DisplayProgressBar("VixForge Quest Engine", $"Processing Material Cache ({matIndex}/{_scannedMaterials.Count})...", 0.4f + (0.2f * ((float)matIndex / _scannedMaterials.Count)));

                    Material questMat = new Material(targetShader);
                    questMat.name = $"{originalMat.name}_Quest";

                    TransferProperties(originalMat, questMat);

                    string matPath = AssetDatabase.GenerateUniqueAssetPath($"{materialsDir}/{questMat.name}.mat");
                    AssetDatabase.CreateAsset(questMat, matPath);

                    materialCache.Add(originalMat, questMat);
                    matIndex++;
                }

                EditorUtility.DisplayProgressBar("VixForge Quest Engine", "Deep Re-Mapping Component References...", 0.65f);

                Renderer[] cloneRenderers = questClone.GetComponentsInChildren<Renderer>(true);
                foreach (var rend in cloneRenderers)
                {
                    if (rend == null) continue;
                    Material[] currentMats = rend.sharedMaterials;
                    Material[] newMats = new Material[currentMats.Length];
                    for (int i = 0; i < currentMats.Length; i++)
                    {
                        if (currentMats[i] != null && materialCache.TryGetValue(currentMats[i], out Material qMat))
                            newMats[i] = qMat;
                        else
                            newMats[i] = currentMats[i];
                    }
                    rend.sharedMaterials = newMats;
                    EditorUtility.SetDirty(rend);
                }

                Component[] allComponents = questClone.GetComponentsInChildren<Component>(true);
                foreach (Component comp in allComponents)
                {
                    if (comp == null || comp is Transform || comp is Renderer) continue;

                    if (comp is MonoBehaviour && comp.GetType().Name.Contains("VRCFury"))
                    {
                        SerializedObject so = new SerializedObject(comp);
                        SerializedProperty prop = so.GetIterator();
                        bool modified = false;

                        while (prop.Next(true))
                        {
                            if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue is Material)
                            {
                                Material oldMat = prop.objectReferenceValue as Material;
                                if (oldMat != null && materialCache.TryGetValue(oldMat, out Material qMat))
                                {
                                    prop.objectReferenceValue = qMat;
                                    modified = true;
                                }
                            }
                        }
                        if (modified) so.ApplyModifiedProperties();
                    }
                }

                EditorUtility.DisplayProgressBar("VixForge Quest Engine", "Applying System Topology Overrides...", 0.8f);

                ProcessDestruction(_scannedAnimators, questClone);
#if VRC_SDK_VRCSDK3
                ProcessDestruction(_scannedPhysBones, questClone);
                ProcessDestruction(_scannedColliders, questClone);
                ProcessDestruction(_scannedContacts, questClone);
                ProcessDestruction(_scannedConstraints, questClone);
                ProcessDestruction(_scannedRaycasts, questClone);
#endif
                ProcessDestruction(_scannedParticles, questClone);
                ProcessDestruction(_scannedTrails, questClone);
                ProcessDestruction(_scannedLines, questClone);
                ProcessDestruction(_scannedJoints, questClone);
                ProcessDestruction(_scannedIncompatible, questClone);

                ProcessGameObjectPurge(_scannedFaceTracking, questClone);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                PrefabUtility.UnpackPrefabInstance(questClone, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                Selection.activeGameObject = questClone;

                Debug.Log($"[VixForge] Quest Conversion Complete! {materialCache.Count} total materials and {_textureCache.Count} high-fidelity textures processed.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _textureCache.Clear();
            }
        }

        private void ProcessDestruction(List<TopologyNode> nodes, GameObject clone)
        {
            foreach (var node in nodes)
            {
                if (!node.keep)
                {
                    Transform targetTransform = string.IsNullOrEmpty(node.relativePath) ? clone.transform : clone.transform.Find(node.relativePath);
                    if (targetTransform != null)
                    {
                        Type compType = node.component.GetType();
                        Component comp = targetTransform.GetComponent(compType);
                        if (comp != null) DestroyImmediate(comp, true);
                    }
                }
            }
        }

        private void ProcessGameObjectPurge(List<TopologyNode> nodes, GameObject clone)
        {
            var sortedNodes = nodes.Where(n => !n.keep).OrderByDescending(n => n.depth).ToList();

            foreach (var node in sortedNodes)
            {
                Transform targetTransform = string.IsNullOrEmpty(node.relativePath) ? clone.transform : clone.transform.Find(node.relativePath);

                if (targetTransform != null && targetTransform.gameObject != clone)
                {
                    DestroyImmediate(targetTransform.gameObject, true);
                }
            }
        }

        private Shader GetShaderForEnum(TargetQuestShader target)
        {
            string shaderName = "VRChat/Mobile/Toon Standard";
            switch (target)
            {
                case TargetQuestShader.VRCMobileToonStandard: shaderName = "VRChat/Mobile/Toon Standard"; break;
                case TargetQuestShader.VRCMobileToonLit: shaderName = "VRChat/Mobile/Toon Lit"; break;
                case TargetQuestShader.VRCMobileStandard: shaderName = "VRChat/Mobile/Standard Lite"; break;
                case TargetQuestShader.VRCMobileMatcap: shaderName = "VRChat/Mobile/MatCap Lit"; break;
                case TargetQuestShader.UnityMobileStandard: shaderName = "Mobile/Standard"; break;
            }

            Shader found = Shader.Find(shaderName);
            if (found == null) return Shader.Find("Standard");
            return found;
        }

        private void TransferProperties(Material source, Material target)
        {
            if (source.HasProperty("_MainTex") && target.HasProperty("_MainTex"))
                target.SetTexture("_MainTex", ProcessAndCloneTexture(source.GetTexture("_MainTex"), false, false));
            else if (source.HasProperty("_BaseMap") && target.HasProperty("_MainTex"))
                target.SetTexture("_MainTex", ProcessAndCloneTexture(source.GetTexture("_BaseMap"), false, false));

            if (source.HasProperty("_Color") && target.HasProperty("_Color"))
                target.SetColor("_Color", source.GetColor("_Color"));
            else if (source.HasProperty("_BaseColor") && target.HasProperty("_Color"))
                target.SetColor("_Color", source.GetColor("_BaseColor"));

            if (source.HasProperty("_EmissionMap") && target.HasProperty("_EmissionMap"))
                target.SetTexture("_EmissionMap", ProcessAndCloneTexture(source.GetTexture("_EmissionMap"), false, false));

            if (source.HasProperty("_EmissionColor") && target.HasProperty("_EmissionColor"))
                target.SetColor("_EmissionColor", source.GetColor("_EmissionColor"));

            if (source.HasProperty("_BumpMap") && target.HasProperty("_BumpMap"))
            {
                target.SetTexture("_BumpMap", ProcessAndCloneTexture(source.GetTexture("_BumpMap"), true, true));
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

                if (sourceMetProp != null && targetMetProp != null && source.GetTexture(sourceMetProp) != null)
                    target.SetTexture(targetMetProp, ProcessAndCloneTexture(source.GetTexture(sourceMetProp), false, true));

                if (source.HasProperty("_Metallic") && target.HasProperty("_Metallic"))
                    target.SetFloat("_Metallic", source.GetFloat("_Metallic"));

                if (source.HasProperty("_Glossiness") && target.HasProperty("_Glossiness"))
                    target.SetFloat("_Glossiness", source.GetFloat("_Glossiness"));
            }
        }

        private Texture ProcessAndCloneTexture(Texture sourceTex, bool isNormalMap = false, bool isLinear = false)
        {
            if (sourceTex == null) return null;

            var node = _scannedTextures.FirstOrDefault(t => t.texture == sourceTex);
            if (node != null && !node.processTexture) return sourceTex;

            if (_textureCache.TryGetValue(sourceTex, out Texture cachedTex)) return cachedTex;

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
                Debug.LogWarning($"[VixForge] Bypassing virtual or missing texture: {sourceTex.name} at path {sourcePath}.");
                return sourceTex;
            }

            string texName = sourceTex.name;
            string extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(extension)) extension = ".png";

            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{_activeTexturesDir}/{texName}_Quest{extension}");
            int targetSize = _textureSizeOptions[_selectedTextureSizeIndex];

            try
            {
                using (MagickImage img = new MagickImage(File.ReadAllBytes(sourcePath)))
                {
                    if (img.Width > targetSize || img.Height > targetSize)
                    {
                        MagickGeometry size = new MagickGeometry((uint)targetSize, (uint)targetSize);
                        size.IgnoreAspectRatio = false;

                        if (!isNormalMap && !isLinear) img.ColorSpace = ImageMagick.ColorSpace.RGB;
                        img.FilterType = FilterType.Lanczos;
                        img.Resize(size);
                        if (!isNormalMap && !isLinear) img.ColorSpace = ImageMagick.ColorSpace.sRGB;

                        img.AdaptiveSharpen(0, 1.0);
                    }
                    img.Quality = 100;
                    img.Write(newPath);
                }
                VixenMagickKit.TryLosslessOptimize(newPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VixForge] ImageMagick processing failed for {texName}, falling back to Unity Copy. Error: {ex.Message}");

                if (File.Exists(sourcePath))
                {
                    AssetDatabase.CopyAsset(sourcePath, newPath);
                }
                else
                {
                    Debug.LogError($"[VixForge] Fallback copy failed. Source path does not exist on disk: {sourcePath}. Passing original reference.");
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
            _textureCache[sourceTex] = newTex;
            return newTex;
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