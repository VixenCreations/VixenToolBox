#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ImageMagick;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VixenTools.Editor
{
    // JSON Wrapper for dynamic template layouts
    [Serializable]
    public class BadgeLayout
    {
        public int nameX = 2258, nameY = 1224, nameW = 2538, nameH = 855;
        public float nameRotation = 0f;
        public int titleX = 2701, titleY = 1677, titleW = 1554, titleH = 257;
        public float titleRotation = 0f;
        
        // Legacy variable preserved for backwards compatibility with old layout.json files
        public Color neonColor = Color.white; 
        
        public Color matBaseColor = Color.white;
        public Color emiMaskColor = Color.white;
        
        public bool emitName = false;
        public bool emitTitle = true;
        public bool hasUpgradedBools = true; // Safety flag for older JSON files
    }

    public class VixenBadgeMaker : EditorWindow
    {
        private enum ToolMode { BadgeGenerator, TemplateBuilder, UVMapper }
        private enum AuthoringType { ProceduralBase, IngestFromSource }
        private enum Ecosystem { VixenTools, FuralitySDK }
        
        private enum TargetShader 
        { 
            AutoDetect, Standard, PoiyomiToon, LilToon, FuralityAqua, 
            FuralitySylva, FuralitySomna, FuralityUmbra, VRCToonStandard, VRCMobileToonLit,
            FuralityModular // <-- Added new shader
        }

        private ToolMode _currentMode = ToolMode.BadgeGenerator;

        // --- Shared State ---
        private const string VixenRootPath = "Assets/VixenTools/Badges/Template Files";
        private const string FuralityRootPath = "Assets/Furality";
        private const string PackageFontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string USS_PATH = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenBadgeMakerStyles.uss";

        private Font _cyberFont;

        // --- Generator State ---
        private string _badgeName = "";
        private string _title = "";
        
        private Color _matBaseColor = new Color(1f, 1f, 1f, 1f); 
        private Color _mainTextColor = new Color(1f, 1f, 1f, 1f); 
        private Color _emiMaskColor = new Color(1f, 1f, 1f, 1f); 
        
        private bool _emitName = false;
        private bool _emitTitle = true;
        private bool _applyToMaterial = true;
        
        private TargetShader _targetShader = TargetShader.AutoDetect; 
        private List<TargetShader> _validShaders = new List<TargetShader>();
        private List<string> _validShaderNames = new List<string>();
        private int _selectedShaderIndex = 0;

        private Ecosystem _activeEcosystem = Ecosystem.VixenTools;
        private List<string> _vixenTemplates = new List<string>();
        private int _selectedVixenTemplate = 0;
        private List<string> _furalityConventions = new List<string>();
        private int _selectedFuralityConv = 0;
        private List<string> _furalityTiers = new List<string>();
        private int _selectedFuralityTier = 0;

        private int _nameX = 2258, _nameY = 1224, _nameW = 2538, _nameH = 855;
        private float _nameRotation = 0f;
        private int _titleX = 2701, _titleY = 1677, _titleW = 1554, _titleH = 257;
        private float _titleRotation = 0f;
        private bool _showAdvancedLayout = false;

        // --- Template Builder State ---
        private AuthoringType _authoringType = AuthoringType.IngestFromSource;
        private string _newTemplateName = "Custom_Base";
        private int _templateResolution = 4096;
        private Color _baseTemplateColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        private Texture2D _sourceDiffuse;
        private Texture2D _sourceEmission;

        // --- UV Mapper State ---
        public bool IsMappingActive { get; private set; } = false;
        public Vector3 LastHitPoint { get; private set; } = Vector3.zero;
        public Vector3 LastHitNormal { get; private set; } = Vector3.back;
        private Tool _previousTool = Tool.None;
        
        private GameObject _mapperTargetMesh;
        private MeshCollider _tempCollider; 
        private int _mapperTexWidth = 4096;
        private int _mapperTexHeight = 4096;
        private int _lastPixelX = 0;
        private int _lastPixelY = 0;

        // --- UI Elements ---
        private VisualElement _generatorContainer;
        private VisualElement _templateContainer;
        private VisualElement _uvMapperContainer;
        
        private VisualElement _vixenRoutingUI;
        private VisualElement _furalityRoutingUI;
        private DropdownField _vixenTemplateDropdown;
        private DropdownField _furalityConvDropdown;
        private DropdownField _furalityTierDropdown;
        private DropdownField _generatorShaderDropdown;
        private DropdownField _templateShaderDropdown;
        private VisualElement _proceduralBaseUI;
        private VisualElement _ingestSourceUI;

        private ColorField _matBaseColorField;
        private ColorField _mainTextColorField;
        private ColorField _emiMaskColorField;
        
        private IntegerField _nxField, _nyField, _nwField, _nhField;
        private FloatField _nrField;
        private IntegerField _txField, _tyField, _twField, _thField;
        private FloatField _trField;

        private Button _btnGeneratorTab;
        private Button _btnTemplateTab;
        private Button _btnUVMapperTab;
        private Label _coordLabelX;
        private Label _coordLabelY;
        private Button _toggleMappingBtn;
        private Label _statusLabel;

        [MenuItem("VixenTools/Avatars/Badge Studio")]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenBadgeMaker>("Badge Studio");
            window.minSize = new Vector2(500, 800);
            window.Show();
        }

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(PackageFontPath);
            ValidateInstalledShaders(); 
            RefreshEcosystems();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            if (IsMappingActive)
            {
                IsMappingActive = false;
                Tools.current = _previousTool == Tool.None ? Tool.Move : _previousTool;
            }
            CleanupTempCollider(); 
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "badge-root";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            var headerRect = new VisualElement { name = "tool-header" };
            var titleLabel = new Label("<color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> BADGE STUDIO") { enableRichText = true };
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            headerRect.Add(titleLabel);
            root.Add(headerRect);

            var tabContainer = new VisualElement { name = "tab-toolbar" };
            
            _btnGeneratorTab = new Button(() => SwitchMode(ToolMode.BadgeGenerator)) { text = "High-Fidelity Generator" };
            _btnTemplateTab = new Button(() => SwitchMode(ToolMode.TemplateBuilder)) { text = "Template Authoring" };
            _btnUVMapperTab = new Button(() => SwitchMode(ToolMode.UVMapper)) { text = "Scene UV Mapper" };
            
            tabContainer.Add(_btnGeneratorTab);
            tabContainer.Add(_btnTemplateTab);
            tabContainer.Add(_btnUVMapperTab);
            root.Add(tabContainer);

            var scrollContainer = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            var scrollContent = new VisualElement();
            scrollContainer.Add(scrollContent);
            root.Add(scrollContainer);

            _generatorContainer = new VisualElement();
            BuildGeneratorUI(_generatorContainer);
            scrollContent.Add(_generatorContainer);

            _templateContainer = new VisualElement();
            BuildTemplateUI(_templateContainer);
            scrollContent.Add(_templateContainer);

            _uvMapperContainer = new VisualElement();
            BuildUVMapperUI(_uvMapperContainer);
            scrollContent.Add(_uvMapperContainer);

            SwitchMode(_currentMode);
            SyncUIToEcosystem();
        }

        private void SwitchMode(ToolMode mode)
        {
            _currentMode = mode;
            
            _btnGeneratorTab.AddToClassList("tab-btn-inactive");
            _btnGeneratorTab.RemoveFromClassList("tab-btn-active");
            _btnTemplateTab.AddToClassList("tab-btn-inactive");
            _btnTemplateTab.RemoveFromClassList("tab-btn-active");
            _btnUVMapperTab.AddToClassList("tab-btn-inactive");
            _btnUVMapperTab.RemoveFromClassList("tab-btn-active");

            _generatorContainer.style.display = DisplayStyle.None;
            _templateContainer.style.display = DisplayStyle.None;
            _uvMapperContainer.style.display = DisplayStyle.None;

            if (mode == ToolMode.BadgeGenerator)
            {
                _btnGeneratorTab.AddToClassList("tab-btn-active");
                _btnGeneratorTab.RemoveFromClassList("tab-btn-inactive");
                _generatorContainer.style.display = DisplayStyle.Flex;
            }
            else if (mode == ToolMode.TemplateBuilder)
            {
                _btnTemplateTab.AddToClassList("tab-btn-active");
                _btnTemplateTab.RemoveFromClassList("tab-btn-inactive");
                _templateContainer.style.display = DisplayStyle.Flex;
            }
            else if (mode == ToolMode.UVMapper)
            {
                _btnUVMapperTab.AddToClassList("tab-btn-active");
                _btnUVMapperTab.RemoveFromClassList("tab-btn-inactive");
                _uvMapperContainer.style.display = DisplayStyle.Flex;
            }
        }

        private void BuildGeneratorUI(VisualElement container)
        {
            var routingPanel = CreateCyberPanel("Ecosystem Routing", "#00e5ff");
            var ecoEnum = new EnumField("Source Network", _activeEcosystem);
            ecoEnum.RegisterValueChangedCallback(e => 
            {
                _activeEcosystem = (Ecosystem)e.newValue;
                SyncUIToEcosystem();
            });
            routingPanel.Add(ecoEnum);

            _vixenRoutingUI = new VisualElement();
            _vixenTemplateDropdown = new DropdownField("Template Base", _vixenTemplates, _selectedVixenTemplate);
            _vixenTemplateDropdown.RegisterValueChangedCallback(e => 
            {
                _selectedVixenTemplate = _vixenTemplates.IndexOf(e.newValue);
                if (_selectedVixenTemplate >= 0) LoadLayoutConfig(Path.Combine(VixenRootPath, _vixenTemplates[_selectedVixenTemplate]));
            });
            _vixenRoutingUI.Add(_vixenTemplateDropdown);

            _furalityRoutingUI = new VisualElement();
            _furalityConvDropdown = new DropdownField("Convention", _furalityConventions, _selectedFuralityConv);
            _furalityConvDropdown.RegisterValueChangedCallback(e => 
            {
                _selectedFuralityConv = _furalityConventions.IndexOf(e.newValue);
                UpdateFuralityTiers();
                AutoAssignLayoutBounds(_furalityConventions[_selectedFuralityConv]);
            });
            
            _furalityTierDropdown = new DropdownField("Badge Tier", _furalityTiers, _selectedFuralityTier);
            _furalityTierDropdown.RegisterValueChangedCallback(e => _selectedFuralityTier = _furalityTiers.IndexOf(e.newValue));

            _furalityRoutingUI.Add(_furalityConvDropdown);
            _furalityRoutingUI.Add(_furalityTierDropdown);

            routingPanel.Add(_vixenRoutingUI);
            routingPanel.Add(_furalityRoutingUI);
            container.Add(routingPanel);

            var idPanel = CreateCyberPanel("Identity & Aesthetics", "#ff00aa");
            
            var nameField = new TextField("Display Name") { value = _badgeName };
            nameField.RegisterValueChangedCallback(e => _badgeName = e.newValue);
            idPanel.Add(nameField);

            var titleField = new TextField("Title / Pronouns") { value = _title };
            titleField.RegisterValueChangedCallback(e => _title = e.newValue);
            idPanel.Add(titleField);

            // COLOR CONTROLS
            var colorLabel = new Label("Material & Map Colors") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } };
            idPanel.Add(colorLabel);

            _matBaseColorField = new ColorField("Material Base Color") { value = _matBaseColor };
            _matBaseColorField.RegisterValueChangedCallback(e => _matBaseColor = e.newValue);
            idPanel.Add(_matBaseColorField);

            _mainTextColorField = new ColorField("Main Text Color (Diffuse)") { value = _mainTextColor };
            _mainTextColorField.RegisterValueChangedCallback(e => _mainTextColor = e.newValue);
            idPanel.Add(_mainTextColorField);

            _emiMaskColorField = new ColorField("Emissive Mask Color") { value = _emiMaskColor };
            _emiMaskColorField.RegisterValueChangedCallback(e => _emiMaskColor = e.newValue);
            idPanel.Add(_emiMaskColorField);

            // PIPELINE PROCESS CONTROLS
            var processLabel = new Label("Pipeline Processing") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } };
            idPanel.Add(processLabel);

            var emitNameToggle = new Toggle("Apply Glow to Display Name") { value = _emitName };
            emitNameToggle.RegisterValueChangedCallback(e => _emitName = e.newValue);
            idPanel.Add(emitNameToggle);

            var emitTitleToggle = new Toggle("Apply Glow to Pronouns") { value = _emitTitle };
            emitTitleToggle.RegisterValueChangedCallback(e => _emitTitle = e.newValue);
            idPanel.Add(emitTitleToggle);

            _generatorShaderDropdown = new DropdownField("Target Shader", _validShaderNames, _selectedShaderIndex);
            _generatorShaderDropdown.RegisterValueChangedCallback(e => 
            {
                _selectedShaderIndex = _validShaderNames.IndexOf(e.newValue);
                _targetShader = _validShaders[_selectedShaderIndex];
                if (_templateShaderDropdown != null) _templateShaderDropdown.index = _selectedShaderIndex;
            });
            idPanel.Add(_generatorShaderDropdown);

            var autoApplyToggle = new Toggle("Auto-Apply to Material") { value = _applyToMaterial };
            autoApplyToggle.RegisterValueChangedCallback(e => _applyToMaterial = e.newValue);
            idPanel.Add(autoApplyToggle);
            
            container.Add(idPanel);

            var layoutPanel = CreateCyberPanel("", "#ffffff"); 
            var foldout = new Foldout { text = "Advanced UV Layout Bounds", value = _showAdvancedLayout };
            foldout.RegisterValueChangedCallback(e => _showAdvancedLayout = e.newValue);
            foldout.AddToClassList("bold-foldout");

            var layoutGrid = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10 } };
            var nameBox = new VisualElement { style = { flexGrow = 1, marginRight = 5 } };
            nameBox.AddToClassList("help-box-styled");
            nameBox.Add(new Label("Display Name") { style = { unityTextAlign = TextAnchor.MiddleCenter, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 }});
            
            _nxField = new IntegerField("Position X") { value = _nameX }; _nxField.RegisterValueChangedCallback(e => _nameX = e.newValue);
            _nyField = new IntegerField("Position Y") { value = _nameY }; _nyField.RegisterValueChangedCallback(e => _nameY = e.newValue);
            _nwField = new IntegerField("Width") { value = _nameW }; _nwField.RegisterValueChangedCallback(e => _nameW = e.newValue);
            _nhField = new IntegerField("Height") { value = _nameH }; _nhField.RegisterValueChangedCallback(e => _nameH = e.newValue);
            _nrField = new FloatField("Rotation") { value = _nameRotation }; _nrField.RegisterValueChangedCallback(e => _nameRotation = e.newValue);
            
            nameBox.Add(_nxField); nameBox.Add(_nyField); nameBox.Add(_nwField); nameBox.Add(_nhField); nameBox.Add(_nrField);
            layoutGrid.Add(nameBox);

            var titleBox = new VisualElement { style = { flexGrow = 1, marginLeft = 5 } };
            titleBox.AddToClassList("help-box-styled");
            titleBox.Add(new Label("Title/Pronouns") { style = { unityTextAlign = TextAnchor.MiddleCenter, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 }});
            
            _txField = new IntegerField("Position X") { value = _titleX }; _txField.RegisterValueChangedCallback(e => _titleX = e.newValue);
            _tyField = new IntegerField("Position Y") { value = _titleY }; _tyField.RegisterValueChangedCallback(e => _titleY = e.newValue);
            _twField = new IntegerField("Width") { value = _titleW }; _twField.RegisterValueChangedCallback(e => _titleW = e.newValue);
            _thField = new IntegerField("Height") { value = _titleH }; _thField.RegisterValueChangedCallback(e => _titleH = e.newValue);
            _trField = new FloatField("Rotation") { value = _titleRotation }; _trField.RegisterValueChangedCallback(e => _titleRotation = e.newValue);

            titleBox.Add(_txField); titleBox.Add(_tyField); titleBox.Add(_twField); titleBox.Add(_thField); titleBox.Add(_trField);
            layoutGrid.Add(titleBox);

            foldout.Add(layoutGrid);

            var saveBtn = new Button(() => 
            {
                if (_activeEcosystem == Ecosystem.VixenTools && _vixenTemplates.Count > 0) 
                    SaveLayoutConfig(Path.Combine(VixenRootPath, _vixenTemplates[_selectedVixenTemplate]));
                else if (_activeEcosystem == Ecosystem.FuralitySDK && _furalityConventions.Count > 0)
                {
                    string furalityFolder = Path.Combine(VixenRootPath, "Furality", _furalityConventions[_selectedFuralityConv]);
                    if (!Directory.Exists(furalityFolder)) Directory.CreateDirectory(furalityFolder);
                    SaveLayoutConfig(furalityFolder);
                }
            }) { text = "Save Layout As Default Override" };
            saveBtn.style.marginTop = 10;
            foldout.Add(saveBtn);

            layoutPanel.Add(foldout);
            container.Add(layoutPanel);

            var execBtn = new Button(() => { try { GenerateBadgeEndToEnd(); } finally { EditorUtility.ClearProgressBar(); } }) { text = "Compile High-Fidelity Badge" };
            execBtn.AddToClassList("cyber-action-btn");
            execBtn.AddToClassList("cyan-btn");
            container.Add(execBtn);
        }

        private void BuildTemplateUI(VisualElement container)
        {
            var authPanel = CreateCyberPanel("Programmatic Asset Authoring", "#ff00aa");

            var infoLabel = new Label($"Scaffolds a complete directory structure inside {VixenRootPath}.");
            infoLabel.AddToClassList("info-box-styled");
            authPanel.Add(infoLabel);

            var nameField = new TextField("Template Name") { value = _newTemplateName };
            nameField.RegisterValueChangedCallback(e => _newTemplateName = e.newValue);
            authPanel.Add(nameField);

            var modeEnum = new EnumField("Authoring Mode", _authoringType);
            modeEnum.RegisterValueChangedCallback(e => 
            {
                _authoringType = (AuthoringType)e.newValue;
                SyncTemplateUIToMode();
            });
            authPanel.Add(modeEnum);

            _proceduralBaseUI = new VisualElement();
            var resChoices = new List<string> { "512x", "1K", "2K", "4K" };
            var resDropdown = new DropdownField("Base Resolution", resChoices, 3);
            resDropdown.RegisterValueChangedCallback(e => 
            {
                int[] vals = { 512, 1024, 2048, 4096 };
                _templateResolution = vals[resChoices.IndexOf(e.newValue)];
            });
            _proceduralBaseUI.Add(resDropdown);

            var colorField = new ColorField("Base Diffuse Color") { value = _baseTemplateColor };
            colorField.RegisterValueChangedCallback(e => _baseTemplateColor = e.newValue);
            _proceduralBaseUI.Add(colorField);
            authPanel.Add(_proceduralBaseUI);

            _ingestSourceUI = new VisualElement();
            var difField = new ObjectField("Empty Diffuse Map") { objectType = typeof(Texture2D), value = _sourceDiffuse };
            difField.RegisterValueChangedCallback(e => _sourceDiffuse = e.newValue as Texture2D);
            _ingestSourceUI.Add(difField);

            var emiField = new ObjectField("Empty Emission Map") { objectType = typeof(Texture2D), value = _sourceEmission };
            emiField.RegisterValueChangedCallback(e => _sourceEmission = e.newValue as Texture2D);
            _ingestSourceUI.Add(emiField);
            authPanel.Add(_ingestSourceUI);

            _templateShaderDropdown = new DropdownField("Master Shader Default", _validShaderNames, _selectedShaderIndex);
            _templateShaderDropdown.style.marginTop = 10;
            _templateShaderDropdown.RegisterValueChangedCallback(e => 
            {
                _selectedShaderIndex = _validShaderNames.IndexOf(e.newValue);
                _targetShader = _validShaders[_selectedShaderIndex];
                if (_generatorShaderDropdown != null) _generatorShaderDropdown.index = _selectedShaderIndex;
            });
            authPanel.Add(_templateShaderDropdown);

            container.Add(authPanel);

            var execBtn = new Button(() => { try { ExecuteTemplateAuthoring(); } finally { EditorUtility.ClearProgressBar(); } }) { text = "Author Master Template" };
            execBtn.AddToClassList("cyber-action-btn");
            execBtn.AddToClassList("pink-btn");
            container.Add(execBtn);

            var devPanel = CreateCyberPanel("Furality Master Layouts", "#00e5ff");
            
            var furalityLabel = new Label("Autonomously scaffold directories and format perfect layout.json boundary configurations for all Furality convention templates.");
            furalityLabel.AddToClassList("info-box-styled");
            devPanel.Add(furalityLabel);

            var genFuralityBtn = new Button(() => { 
                GenerateFuralityLayouts(); 
                RefreshEcosystems(); 
            }) { text = "Generate Furality JSON Layouts" };
            genFuralityBtn.AddToClassList("cyber-action-btn");
            genFuralityBtn.AddToClassList("cyan-btn");
            devPanel.Add(genFuralityBtn);
            
            container.Add(devPanel);

            SyncTemplateUIToMode();
        }

        private void BuildUVMapperUI(VisualElement container)
        {
            var targetPanel = CreateCyberPanel("Target Parameters", "#00e5ff");
            
            var infoLabel = new Label("Select the badge GameObject. Unity requires a MeshCollider to calculate UV intersections. Enable mapping, then click the Scene View.");
            infoLabel.AddToClassList("info-box-styled");
            targetPanel.Add(infoLabel);

            var targetField = new ObjectField("Target Badge Mesh") { objectType = typeof(GameObject), allowSceneObjects = true, value = _mapperTargetMesh };
            targetField.RegisterValueChangedCallback(e => 
            {
                CleanupTempCollider(); 
                _mapperTargetMesh = e.newValue as GameObject;
                ValidateCollider();
            });
            targetPanel.Add(targetField);

            _statusLabel = new Label("Status: Standby") { enableRichText = true };
            _statusLabel.style.marginTop = 5;
            targetPanel.Add(_statusLabel);

            var resRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10 } };
            var wField = new IntegerField("Texture Width") { value = _mapperTexWidth, style = { flexGrow = 1 } };
            wField.RegisterValueChangedCallback(e => _mapperTexWidth = e.newValue);
            var hField = new IntegerField("Height") { value = _mapperTexHeight, style = { flexGrow = 1 } };
            hField.RegisterValueChangedCallback(e => _mapperTexHeight = e.newValue);
            resRow.Add(wField);
            resRow.Add(hField);
            targetPanel.Add(resRow);

            container.Add(targetPanel);

            var coordPanel = CreateCyberPanel("Live Scene Coordinates", "#ff00aa");

            var flexRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginTop = 10 } };
            
            var boxX = new VisualElement(); boxX.AddToClassList("coord-box");
            boxX.Add(new Label("PIXEL X") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0f, 0.9f, 1f) } });
            _coordLabelX = new Label("0") { style = { fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } };
            boxX.Add(_coordLabelX);
            
            var boxY = new VisualElement(); boxY.AddToClassList("coord-box");
            boxY.Add(new Label("PIXEL Y") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = new Color(1f, 0f, 0.66f) } });
            _coordLabelY = new Label("0") { style = { fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } };
            boxY.Add(_coordLabelY);

            flexRow.Add(boxX);
            flexRow.Add(boxY);
            coordPanel.Add(flexRow);

            var copyRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10 } };
            var copyXBtn = new Button(() => EditorGUIUtility.systemCopyBuffer = _lastPixelX.ToString()) { text = "Copy X", style = { flexGrow = 1 } };
            var copyYBtn = new Button(() => EditorGUIUtility.systemCopyBuffer = _lastPixelY.ToString()) { text = "Copy Y", style = { flexGrow = 1 } };
            copyRow.Add(copyXBtn); copyRow.Add(copyYBtn);
            coordPanel.Add(copyRow);

            container.Add(coordPanel);

            _toggleMappingBtn = new Button(ToggleMapping) { text = "ACTIVATE SCENE MAPPING" };
            _toggleMappingBtn.AddToClassList("cyber-action-btn");
            _toggleMappingBtn.AddToClassList("cyan-btn");
            container.Add(_toggleMappingBtn);

            ValidateCollider();
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

        private void SyncUIToEcosystem()
        {
            if (_vixenRoutingUI == null || _furalityRoutingUI == null) return;

            if (_activeEcosystem == Ecosystem.VixenTools)
            {
                _vixenRoutingUI.style.display = DisplayStyle.Flex;
                _furalityRoutingUI.style.display = DisplayStyle.None;
                if (_vixenTemplates.Count > 0) LoadLayoutConfig(Path.Combine(VixenRootPath, _vixenTemplates[_selectedVixenTemplate]));
            }
            else
            {
                _vixenRoutingUI.style.display = DisplayStyle.None;
                _furalityRoutingUI.style.display = DisplayStyle.Flex;
                if (_furalityConventions.Count > 0) AutoAssignLayoutBounds(_furalityConventions[_selectedFuralityConv]);
            }
        }

        private void SyncTemplateUIToMode()
        {
            if (_proceduralBaseUI == null || _ingestSourceUI == null) return;

            if (_authoringType == AuthoringType.ProceduralBase)
            {
                _proceduralBaseUI.style.display = DisplayStyle.Flex;
                _ingestSourceUI.style.display = DisplayStyle.None;
            }
            else
            {
                _proceduralBaseUI.style.display = DisplayStyle.None;
                _ingestSourceUI.style.display = DisplayStyle.Flex;
            }
        }

        private void SyncLayoutUIToState()
        {
            _nxField?.SetValueWithoutNotify(_nameX);
            _nyField?.SetValueWithoutNotify(_nameY);
            _nwField?.SetValueWithoutNotify(_nameW);
            _nhField?.SetValueWithoutNotify(_nameH);
            _nrField?.SetValueWithoutNotify(_nameRotation);

            _txField?.SetValueWithoutNotify(_titleX);
            _tyField?.SetValueWithoutNotify(_titleY);
            _twField?.SetValueWithoutNotify(_titleW);
            _thField?.SetValueWithoutNotify(_titleH);
            _trField?.SetValueWithoutNotify(_titleRotation);

            _matBaseColorField?.SetValueWithoutNotify(_matBaseColor);
            _mainTextColorField?.SetValueWithoutNotify(_mainTextColor);
            _emiMaskColorField?.SetValueWithoutNotify(_emiMaskColor);
        }

        #region File & Core Logic

        private void RefreshEcosystems()
        {
            if (!Directory.Exists(VixenRootPath)) Directory.CreateDirectory(VixenRootPath);
            
            _vixenTemplates = AssetDatabase.GetSubFolders(VixenRootPath)
                .Select(Path.GetFileName)
                .Where(folder => !folder.Equals("Furality", StringComparison.OrdinalIgnoreCase))
                .ToList();

            _furalityConventions.Clear();
            if (Directory.Exists(FuralityRootPath))
            {
                var convFolders = AssetDatabase.GetSubFolders(FuralityRootPath);
                foreach (var folder in convFolders)
                {
                    if (Directory.Exists(Path.Combine(folder, "Avatar Assets", "Badges")))
                        _furalityConventions.Add(Path.GetFileName(folder));
                }
            }

            UpdateFuralityTiers();

            if (_vixenTemplateDropdown != null) 
            {
                _vixenTemplateDropdown.choices = _vixenTemplates;
                if (_vixenTemplates.Count > 0 && _selectedVixenTemplate >= _vixenTemplates.Count) _selectedVixenTemplate = 0;
                _vixenTemplateDropdown.SetValueWithoutNotify(_vixenTemplates.Count > 0 ? _vixenTemplates[_selectedVixenTemplate] : "");
            }
            if (_furalityConvDropdown != null)
            {
                _furalityConvDropdown.choices = _furalityConventions;
                if (_furalityConventions.Count > 0 && _selectedFuralityConv >= _furalityConventions.Count) _selectedFuralityConv = 0;
                _furalityConvDropdown.SetValueWithoutNotify(_furalityConventions.Count > 0 ? _furalityConventions[_selectedFuralityConv] : "");
            }

            if (_activeEcosystem == Ecosystem.VixenTools && _vixenTemplates.Count > 0)
                LoadLayoutConfig(Path.Combine(VixenRootPath, _vixenTemplates[_selectedVixenTemplate]));
        }

        private void UpdateFuralityTiers()
        {
            _furalityTiers.Clear();
            if (_furalityConventions.Count > 0 && _selectedFuralityConv < _furalityConventions.Count)
            {
                string badgePath = Path.Combine(FuralityRootPath, _furalityConventions[_selectedFuralityConv], "Avatar Assets", "Badges");
                if (Directory.Exists(badgePath))
                    _furalityTiers.AddRange(AssetDatabase.GetSubFolders(badgePath).Select(Path.GetFileName));
            }
            _selectedFuralityTier = Mathf.Clamp(_selectedFuralityTier, 0, Mathf.Max(0, _furalityTiers.Count - 1));

            if (_furalityTierDropdown != null)
            {
                _furalityTierDropdown.choices = _furalityTiers;
                _furalityTierDropdown.SetValueWithoutNotify(_furalityTiers.Count > 0 ? _furalityTiers[_selectedFuralityTier] : "");
            }
        }

        private void LoadLayoutConfig(string folderPath)
        {
            string jsonPath = Path.Combine(folderPath, "layout.json");
            if (File.Exists(jsonPath))
            {
                try
                {
                    var layout = JsonUtility.FromJson<BadgeLayout>(File.ReadAllText(jsonPath));
                    _nameX = layout.nameX; _nameY = layout.nameY; _nameW = layout.nameW; _nameH = layout.nameH; _nameRotation = layout.nameRotation;
                    _titleX = layout.titleX; _titleY = layout.titleY; _titleW = layout.titleW; _titleH = layout.titleH; _titleRotation = layout.titleRotation;
                    
                    _mainTextColor = layout.neonColor; 
                    _emiMaskColor = (layout.emiMaskColor.a == 0f) ? Color.white : layout.emiMaskColor;
                    _matBaseColor = (layout.matBaseColor.a == 0f) ? Color.white : layout.matBaseColor;
                    
                    // Fallback safety to prevent old templates from disabling glows
                    if (!layout.hasUpgradedBools)
                    {
                        _emitName = false;
                        _emitTitle = true;
                    }
                    else
                    {
                        _emitName = layout.emitName;
                        _emitTitle = layout.emitTitle;
                    }
                }
                catch { Debug.LogWarning("[VixForge] Failed to parse layout.json."); }
            }
            SyncLayoutUIToState();
        }

        private void SaveLayoutConfig(string folderPath)
        {
            var layout = new BadgeLayout {
                nameX = _nameX, nameY = _nameY, nameW = _nameW, nameH = _nameH, nameRotation = _nameRotation,
                titleX = _titleX, titleY = _titleY, titleW = _titleW, titleH = _titleH, titleRotation = _titleRotation,
                neonColor = _mainTextColor,
                emiMaskColor = _emiMaskColor,
                matBaseColor = _matBaseColor,
                emitName = _emitName,
                emitTitle = _emitTitle,
                hasUpgradedBools = true
            };
            string jsonPath = Path.Combine(folderPath, "layout.json");
            File.WriteAllText(jsonPath, JsonUtility.ToJson(layout, true));
            AssetDatabase.Refresh();
            Debug.Log($"[VixForge] Persisted layout to {jsonPath}");
        }

        private void AutoAssignLayoutBounds(string conventionName)
        {
            string jsonFolder = Path.Combine(VixenRootPath, "Furality", conventionName);
            string jsonPath = Path.Combine(jsonFolder, "layout.json");

            if (File.Exists(jsonPath))
            {
                LoadLayoutConfig(jsonFolder);
            }
            else
            {
                if (conventionName.Contains("Luma") || conventionName.Contains("Umbra"))
                {
                    _nameX = 2258; _nameY = 1224; _nameW = 2538; _nameH = 855;
                    _titleX = 2701; _titleY = 1677; _titleW = 1554; _titleH = 257;
                    _mainTextColor = Color.white;
                    _emiMaskColor = Color.white;
                    _matBaseColor = Color.white;
                }
                else if (conventionName.Contains("Somna"))
                {
                    _nameX = 375; _nameY = 700; _nameW = 610; _nameH = 150;
                    _titleX = 450; _titleY = 810; _titleW = 449; _titleH = 75;
                    _mainTextColor = ColorUtility.TryParseHtmlString("#ffeead", out Color c) ? c : Color.white;
                    _emiMaskColor = Color.white;
                    _matBaseColor = Color.white;
                }
                else if (conventionName.Contains("Sylva"))
                {
                    _nameX = 1968; _nameY = 1273; _nameW = 1650; _nameH = 450;
                    _titleX = 2005; _titleY = 1707; _titleW = 1250; _titleH = 300;
                    _mainTextColor = ColorUtility.TryParseHtmlString("#66ff00", out Color c) ? c : Color.green; 
                    _emiMaskColor = Color.white;
                    _matBaseColor = Color.white;
                }
                else if (conventionName.Contains("Ultra"))
                {
                    _nameX = 500; _nameY = 300; _nameW = 750; _nameH = 750;
                    _titleX = 550; _titleY = 600; _titleW = 650; _titleH = 750;
                    _mainTextColor = ColorUtility.TryParseHtmlString("#ff00aa", out Color c) ? c : Color.magenta;
                    _emiMaskColor = Color.white;
                    _matBaseColor = Color.white;
                }
                _emitName = false;
                _emitTitle = true;
                SyncLayoutUIToState();
            }
        }

        private void ValidateInstalledShaders()
        {
            _validShaders.Clear();
            _validShaders.Add(TargetShader.AutoDetect);

            foreach (TargetShader shader in Enum.GetValues(typeof(TargetShader)))
            {
                if (shader == TargetShader.AutoDetect) continue;
                if (FindShaderSafely(shader) != null) _validShaders.Add(shader);
            }

            _validShaderNames = _validShaders.Select(GetShaderDisplayName).ToList();
            if (_selectedShaderIndex >= _validShaders.Count) _selectedShaderIndex = 0;
            _targetShader = _validShaders[_selectedShaderIndex];

            if (_generatorShaderDropdown != null) _generatorShaderDropdown.choices = _validShaderNames;
            if (_templateShaderDropdown != null) _templateShaderDropdown.choices = _validShaderNames;
        }

        private string GetShaderDisplayName(TargetShader shader)
        {
            switch (shader)
            {
                case TargetShader.AutoDetect: return "Auto-Detect (Current Material)";
                case TargetShader.Standard: return "Unity Standard";
                case TargetShader.PoiyomiToon: return "Poiyomi Toon";
                case TargetShader.LilToon: return "lilToon";
                case TargetShader.FuralityAqua: return "Furality Aqua";
                case TargetShader.FuralitySylva: return "Furality Sylva";
                case TargetShader.FuralitySomna: return "Furality Somna";
                case TargetShader.FuralityUmbra: return "Furality Umbra";
                case TargetShader.FuralityModular: return "Furality Modular (Ultra)"; // <-- Added
                case TargetShader.VRCToonStandard: return "VRChat Mobile Toon Standard";
                case TargetShader.VRCMobileToonLit: return "VRChat Mobile Toon Lit";
                default: return shader.ToString();
            }
        }

        private string GetShaderString(TargetShader shader)
        {
            switch (shader)
            {
                case TargetShader.Standard: return "Standard";
                case TargetShader.LilToon: return "lilToon";
                case TargetShader.FuralityAqua: return "Furality/Aqua Shader/Aqua Shader";
                case TargetShader.FuralitySylva: return "Furality/Sylva Shader/Sylva Opaque";
                case TargetShader.FuralitySomna: return "Furality/Somna Shader";
                case TargetShader.FuralityUmbra: return "Furality/Umbra Shader/Umbra Opaque";
                case TargetShader.FuralityModular: return "Furality/Modular/Standard"; // <-- Added
                case TargetShader.VRCToonStandard: return "VRChat/Mobile/Toon Standard";
                case TargetShader.VRCMobileToonLit: return "VRChat/Mobile/Toon Lit";
                case TargetShader.PoiyomiToon: return ".poiyomi/Poiyomi Toon"; 
                default: return "Standard";
            }
        }

        private Shader FindShaderSafely(TargetShader target)
        {
            string sName = GetShaderString(target);
            Shader foundShader = Shader.Find(sName);
            if (foundShader == null && target == TargetShader.PoiyomiToon)
            {
                foundShader = Shader.Find(".poiyomi/Old Versions/9.3/Poiyomi Toon");
                if (foundShader == null) foundShader = Shader.Find("Hidden/Locked/poiyomi/Toon"); 
            }
            return foundShader;
        }

        private void GenerateBadgeEndToEnd()
        {
            string tierFolder = "", outDir = "", outPrefix = "", tierName = "", conventionName = "";

            if (_activeEcosystem == Ecosystem.VixenTools)
            {
                tierName = _vixenTemplates[_selectedVixenTemplate];
                tierFolder = Path.Combine(VixenRootPath, tierName);

                string texDir = Path.Combine(tierFolder, "Textures");
                if (!Directory.Exists(texDir) && Directory.Exists(Path.Combine(tierFolder, "Texture"))) texDir = Path.Combine(tierFolder, "Texture");

                outDir = Path.Combine(texDir, "Output");
                outPrefix = $"VIXEN_{Regex.Replace(_badgeName, @"[<>:""/\\|?* ]", "")}";
            }
            else 
            {
                conventionName = _furalityConventions[_selectedFuralityConv];
                tierName = _furalityTiers[_selectedFuralityTier];
                tierFolder = Path.Combine(FuralityRootPath, conventionName, "Avatar Assets", "Badges", tierName);

                string texDir = Path.Combine(tierFolder, "Textures");
                if (!Directory.Exists(texDir) && Directory.Exists(Path.Combine(tierFolder, "Texture"))) texDir = Path.Combine(tierFolder, "Texture");

                outDir = Path.Combine(texDir, "Custom");
                outPrefix = $"CUSTOM_{Regex.Replace(_badgeName, @"[<>:""/\\|?* ]", "_")}_{tierName}";
            }

            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            string difOut = Path.Combine(outDir, $"{outPrefix}_DIF.png");
            string emiOut = Path.Combine(outDir, $"{outPrefix}_EMI.png");

            string difIn, emiIn;
            bool isUmbra = false;

            if (_activeEcosystem == Ecosystem.FuralitySDK)
            {
                difIn = ResolveFuralityTexture(tierFolder, conventionName, tierName, "DIF");
                emiIn = ResolveFuralityTexture(tierFolder, conventionName, tierName, "EMI");
                isUmbra = conventionName.Contains("Umbra");
            }
            else
            {
                string texDir = Path.Combine(tierFolder, "Textures");
                if (!Directory.Exists(texDir) && Directory.Exists(Path.Combine(tierFolder, "Texture"))) texDir = Path.Combine(tierFolder, "Texture");

                var files = Directory.Exists(texDir) ? Directory.GetFiles(texDir, "*.*").ToList() : new List<string>();
                difIn = FindTextureMatch(files, tierName, new[] { "_Empty.jpg", "_DIF.png", "Empty", "DIF" }, "MASK", "EMI");
                emiIn = FindTextureMatch(files, tierName, new[] { "_EMI", "_Empty_EMI" }, "MASK", "DIF");
            }

            string fontAbsolutePath = Path.GetFullPath(PackageFontPath).Replace("\\", "/");

            // Hardcode Alpha channel to 65535 (fully opaque) to prevent invisible text renders
            MagickColor mMainText = new MagickColor((ushort)(_mainTextColor.r * 65535), (ushort)(_mainTextColor.g * 65535), (ushort)(_mainTextColor.b * 65535), 65535);
            MagickColor mEmiText = new MagickColor((ushort)(_emiMaskColor.r * 65535), (ushort)(_emiMaskColor.g * 65535), (ushort)(_emiMaskColor.b * 65535), 65535);
            MagickColor mWhite = new MagickColor(65535, 65535, 65535, 65535);

            EditorUtility.DisplayProgressBar("Badge Studio", "Rendering Text Plates...", 0.3f);
            
            // Generate plates for the DIFFUSE map
            using MagickImage nameImg = GenerateTextPlate(fontAbsolutePath, _badgeName, _nameW, _nameH, mMainText, _nameRotation);
            using MagickImage titleImg = GenerateTextPlate(fontAbsolutePath, _title, _titleW, _titleH, mWhite, _titleRotation);
            
            // Generate targeted plates for the EMISSION map based on UI Toggles
            using MagickImage nameImgEmi = _emitName ? GenerateTextPlate(fontAbsolutePath, _badgeName, _nameW, _nameH, mEmiText, _nameRotation) : null;
            using MagickImage titleImgEmi = _emitTitle ? GenerateTextPlate(fontAbsolutePath, _title, _titleW, _titleH, mEmiText, _titleRotation) : null;

            EditorUtility.DisplayProgressBar("Badge Studio", "Compositing Maps...", 0.6f);

            CompositeTexture(difIn, nameImg, titleImg, difOut, applyGrayscale: false, isEmission: false);
            CompositeTexture(emiIn, nameImgEmi, titleImgEmi, emiOut, applyGrayscale: isUmbra, isEmission: true);

            AssetDatabase.Refresh();
            SetupTextureImporter(difOut, false);
            SetupTextureImporter(emiOut, false);

            if (_applyToMaterial) ApplyToMaterial(conventionName, tierName, difOut, emiOut);

            Debug.Log($"[VixForge] Successfully compiled badge to {outDir}");
        }

        private MagickImage GenerateTextPlate(string fontPath, string text, int w, int h, MagickColor color, float rotation)
        {
            if (string.IsNullOrEmpty(text)) text = " ";
            var settings = new MagickReadSettings { BackgroundColor = MagickColors.Transparent, FillColor = color, Font = "@" + fontPath, Width = (uint)w, Height = (uint)h };

            // @filename indirection avoids the label: parser choking on ' " ` @ : in user text.
            string tempFile = Path.Combine(Path.GetTempPath(), $"vixen_label_{Guid.NewGuid():N}.txt").Replace("\\", "/");
            MagickImage image;
            try
            {
                File.WriteAllText(tempFile, text, new System.Text.UTF8Encoding(false));
                image = new MagickImage($"label:@{tempFile}", settings);
            }
            finally
            {
                try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
            }

            image.Trim();
            if (rotation != 0f) { image.BackgroundColor = MagickColors.Transparent; image.Rotate(rotation); }
            return image;
        }

        private void CompositeTexture(string baseTexPath, MagickImage namePlate, MagickImage titlePlate, string outPath, bool applyGrayscale, bool isEmission)
        {
            if (string.IsNullOrEmpty(baseTexPath) || !File.Exists(baseTexPath)) { Debug.LogWarning($"[VixForge] Missing base texture at: {baseTexPath}"); return; }
            
            using MagickImage img = new MagickImage(File.ReadAllBytes(baseTexPath));
            img.HasAlpha = true; // Force alpha channel support for the incoming plate blending
            
            if (applyGrayscale) img.Grayscale(); 
            
            if (namePlate != null) img.Composite(namePlate, _nameX - (int)(namePlate.Width / 2), _nameY - (int)(namePlate.Height / 2), CompositeOperator.Over);
            if (titlePlate != null) img.Composite(titlePlate, _titleX - (int)(titlePlate.Width / 2), _titleY - (int)(titlePlate.Height / 2), CompositeOperator.Over);
            
            if (isEmission)
            {
                // CRITICAL ARCHITECTURAL FIX: Flatten the mask to pure black.
                // Unity completely ignores Alpha on emission maps and reads raw RGB.
                // If Magick saves a cleared background as Transparent White (255,255,255,0), the whole badge blows out.
                using MagickImage blackBg = new MagickImage(MagickColors.Black, img.Width, img.Height);
                blackBg.Composite(img, CompositeOperator.Over);
                blackBg.HasAlpha = false;
                blackBg.Write(outPath);
            }
            else
            {
                img.Write(outPath);
            }
            VixenMagickKit.TryLosslessOptimize(outPath);
        }

        private string ResolveFuralityTexture(string folder, string conventionName, string tierName, string mapType)
        {
            if (!Directory.Exists(folder)) return null;
            var allFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".tga", StringComparison.OrdinalIgnoreCase)).ToList();

            string noSpaceTier = Regex.Replace(tierName, @"\s+", "");
            string[] exactNames = new string[0];

            if (conventionName.Contains("Luma") || conventionName.Contains("Sylva"))
                exactNames = mapType == "DIF" ? new[] { $"{tierName}_Empty.png", $"{tierName}_Empty.jpg" } : new[] { $"{tierName}_Empty_EMI.png", $"{tierName}_Empty_EMI.jpg" };
            else if (conventionName.Contains("Somna") || conventionName.Contains("Ultra"))
                exactNames = mapType == "DIF" ? new[] { $"Badge{noSpaceTier}_DIF.png", $"Badge{noSpaceTier}_DIF.jpg" } : new[] { $"Badge{noSpaceTier}_EMI.png", $"Badge{noSpaceTier}_EMI.jpg" };
            else if (conventionName.Contains("Umbra"))
                exactNames = new[] { $"Badge {tierName}_EMI_BLANK.png", $"Badge {tierName}_EMI_BLANK.jpg" };

            foreach (var name in exactNames)
            {
                var match = allFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(name, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }
            return FindTextureMatch(allFiles, tierName, mapType == "DIF" ? new[] { "_Empty", "_DIF", "BLANK" } : new[] { "_EMI", "_Empty_EMI" }, "MASK", mapType == "DIF" ? "_EMI" : "_DIF");
        }

        private string FindTextureMatch(List<string> files, string tierName, string[] keywords, string exclude1, string exclude2)
        {
            string noSpaceTier = Regex.Replace(tierName, @"\s+", "");
            foreach (var kw in keywords)
            {
                var match = files.FirstOrDefault(f => (f.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 || f.IndexOf($"Badge{noSpaceTier}{kw}", StringComparison.OrdinalIgnoreCase) >= 0) && f.IndexOf(exclude1, StringComparison.OrdinalIgnoreCase) < 0 && f.IndexOf(exclude2, StringComparison.OrdinalIgnoreCase) < 0);
                if (match != null) return match;
            }
            return null;
        }

        private void ApplyToMaterial(string conventionName, string tierName, string difPath, string emiPath)
        {
            string tierFolder = _activeEcosystem == Ecosystem.VixenTools ? Path.Combine(VixenRootPath, tierName) : Path.Combine(FuralityRootPath, conventionName, "Avatar Assets", "Badges", tierName);
            string matFolder = Path.Combine(tierFolder, "Materials");
            if (!Directory.Exists(matFolder) && Directory.Exists(Path.Combine(tierFolder, "Material"))) matFolder = Path.Combine(tierFolder, "Material");
            if (!Directory.Exists(matFolder)) return;

            string noSpaceTier = Regex.Replace(tierName, @"\s+", "");
            var matFiles = Directory.GetFiles(matFolder, "*.mat");
            string targetMatPath = matFiles.FirstOrDefault(f => f.Contains("Attendee") || f.Contains($"Badge{noSpaceTier}") || f.Contains(tierName)) ?? (matFiles.Length > 0 ? matFiles[0] : null);

            if (targetMatPath != null)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(targetMatPath);
                if (material)
                {
                    if (_targetShader != TargetShader.AutoDetect)
                    {
                        Shader newShader = FindShaderSafely(_targetShader);
                        if (newShader != null) material.shader = newShader;
                    }
                    
                    Texture2D difTex = AssetDatabase.LoadAssetAtPath<Texture2D>(difPath);
                    Texture2D emiTex = AssetDatabase.LoadAssetAtPath<Texture2D>(emiPath);
                    
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", difTex); 
                    else if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", difTex); 
                    
                    if (material.HasProperty("_EmissionMap")) material.SetTexture("_EmissionMap", emiTex);
                    if (material.HasProperty("_EmissionStrength")) material.SetFloat("_EmissionStrength", 1f); 
                    if (material.HasProperty("_EnableEmission")) material.SetFloat("_EnableEmission", 1f);     
                    if (material.HasProperty("_UseEmission")) material.SetFloat("_UseEmission", 1f);           
                    
                    // CRITICAL FIX: Push explicit user colors to the material properties
                    if (material.HasProperty("_Color")) material.SetColor("_Color", _matBaseColor);
                    else if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", _matBaseColor);
                    
                    if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", _emiMaskColor);

                    AssetDatabase.SaveAssets();
                }
            }
        }

        private void ExecuteTemplateAuthoring()
        {
            string safeName = Regex.Replace(_newTemplateName, @"[<>:""/\\|?* ]", "_");
            string templateDir = Path.Combine(VixenRootPath, safeName);
            if (Directory.Exists(templateDir)) { Debug.LogError($"[VixForge] Template {safeName} already exists!"); return; }

            Directory.CreateDirectory(templateDir);
            string texDir = Path.Combine(templateDir, "Textures"); Directory.CreateDirectory(texDir);
            Directory.CreateDirectory(Path.Combine(texDir, "Output"));
            string matDir = Path.Combine(templateDir, "Materials"); Directory.CreateDirectory(matDir);

            string difPath = Path.Combine(texDir, $"{safeName}_DIF.png");
            string emiPath = Path.Combine(texDir, $"{safeName}_EMI.png");
            MagickColor mColor = new MagickColor((ushort)(_baseTemplateColor.r * 65535), (ushort)(_baseTemplateColor.g * 65535), (ushort)(_baseTemplateColor.b * 65535), (ushort)(_baseTemplateColor.a * 65535));

            if (_authoringType == AuthoringType.ProceduralBase)
            {
                using (MagickImage dif = new MagickImage(mColor, (uint)_templateResolution, (uint)_templateResolution)) dif.Write(difPath);
                using (MagickImage emi = new MagickImage(MagickColors.Black, (uint)_templateResolution, (uint)_templateResolution)) emi.Write(emiPath);
            }
            else
            {
                if (_sourceDiffuse != null) { using (MagickImage dif = new MagickImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(_sourceDiffuse)))) { _templateResolution = (int)dif.Width; dif.Write(difPath); } }
                if (_sourceEmission != null) { using (MagickImage emi = new MagickImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(_sourceEmission)))) emi.Write(emiPath); }
                else { using (MagickImage emi = new MagickImage(MagickColors.Black, (uint)_templateResolution, (uint)_templateResolution)) emi.Write(emiPath); }
            }
            VixenMagickKit.TryLosslessOptimize(difPath);
            VixenMagickKit.TryLosslessOptimize(emiPath);

            SaveLayoutConfig(templateDir);
            AssetDatabase.Refresh();
            SetupTextureImporter(difPath, false); SetupTextureImporter(emiPath, false); 

            Shader matShader = Shader.Find("Standard");
            if (_targetShader != TargetShader.AutoDetect)
            {
                Shader foundShader = FindShaderSafely(_targetShader);
                if (foundShader != null) matShader = foundShader;
            }

            Material mat = new Material(matShader);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(difPath));
            if (mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(emiPath));
            if (mat.HasProperty("_EmissionStrength")) mat.SetFloat("_EmissionStrength", 1f);
            
            AssetDatabase.CreateAsset(mat, Path.Combine(matDir, $"{safeName}.mat"));
            AssetDatabase.SaveAssets(); RefreshEcosystems();
            SwitchMode(ToolMode.BadgeGenerator); 
        }

        private void SetupTextureImporter(string path, bool isLinear)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) return; importer.streamingMipmaps = true; importer.sRGBTexture = !isLinear; importer.SaveAndReimport();
        }

        #endregion

        #region UV Mapper & Raycast Logic

        private void CleanupTempCollider()
        {
            if (_tempCollider != null)
            {
                DestroyImmediate(_tempCollider);
                _tempCollider = null;
            }
        }

        private void ToggleMapping()
        {
            if (_mapperTargetMesh == null)
            {
                EditorUtility.DisplayDialog("Missing Target", "Please assign a Target Badge Mesh first.", "OK");
                return;
            }

            IsMappingActive = !IsMappingActive;

            if (IsMappingActive)
            {
                _previousTool = Tools.current;
                Tools.current = Tool.None; 
            }
            else
            {
                Tools.current = _previousTool == Tool.None ? Tool.Move : _previousTool;
                LastHitPoint = Vector3.zero;
                SceneView.RepaintAll();
                CleanupTempCollider(); 
            }
            
            UpdateMappingUI();
            ValidateCollider(); 
        }

        private void UpdateMappingUI()
        {
            if (_toggleMappingBtn == null) return;

            if (IsMappingActive)
            {
                _toggleMappingBtn.text = "MAPPING ACTIVE (CLICK TO STOP)";
                _toggleMappingBtn.RemoveFromClassList("cyan-btn");
                _toggleMappingBtn.AddToClassList("pink-btn");
            }
            else
            {
                _toggleMappingBtn.text = "ACTIVATE SCENE MAPPING";
                _toggleMappingBtn.RemoveFromClassList("pink-btn");
                _toggleMappingBtn.AddToClassList("cyan-btn");
            }
        }

        private void ValidateCollider()
        {
            if (_statusLabel == null) return;

            if (_mapperTargetMesh == null)
            {
                _statusLabel.text = "<color=#aaaaaa>Status: Awaiting Target...</color>";
                return;
            }

            var col = _mapperTargetMesh.GetComponent<MeshCollider>();
            if (col == null)
            {
                _statusLabel.text = "<color=#ffaa00>Warning: No MeshCollider detected. Raycast will fail.</color>";
                
                for (int i = _statusLabel.parent.childCount - 1; i >= 0; i--)
                {
                    if (_statusLabel.parent[i] is Button b && b.text.Contains("Attach Temporary"))
                        _statusLabel.parent.RemoveAt(i);
                }

                var fixBtn = new Button(() => 
                {
                    _tempCollider = _mapperTargetMesh.AddComponent<MeshCollider>();
                    
                    SkinnedMeshRenderer smr = _mapperTargetMesh.GetComponent<SkinnedMeshRenderer>();
                    if (smr != null)
                    {
                        Mesh bakedMesh = new Mesh();
                        smr.BakeMesh(bakedMesh);
                        _tempCollider.sharedMesh = bakedMesh;
                    }

                    ValidateCollider();
                }) { text = "Attach Temporary MeshCollider", style = { marginTop = 5 } };
                
                _statusLabel.parent.Add(fixBtn);
            }
            else
            {
                _statusLabel.text = "<color=#00ff66>Status: MeshCollider OK. Ready for mapping.</color>";
                
                for (int i = _statusLabel.parent.childCount - 1; i >= 0; i--)
                {
                    if (_statusLabel.parent[i] is Button b && b.text.Contains("Attach Temporary"))
                        _statusLabel.parent.RemoveAt(i);
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!IsMappingActive || _mapperTargetMesh == null) return;

            if (LastHitPoint != Vector3.zero)
            {
                Handles.color = new Color(1f, 0f, 0.66f, 0.8f);
                Handles.DrawSolidDisc(LastHitPoint, LastHitNormal, 0.02f);
                sceneView.Repaint();
            }

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            Event e = Event.current;
            
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && e.modifiers == EventModifiers.None)
            {
                ProcessRaycast(e.mousePosition);
                e.Use(); 
            }
        }

        public void ProcessRaycast(Vector2 mousePosition)
        {
            if (_mapperTargetMesh == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == _mapperTargetMesh)
                {
                    Vector2 uv = hit.textureCoord;

                    _lastPixelX = Mathf.RoundToInt(uv.x * _mapperTexWidth);
                    _lastPixelY = Mathf.RoundToInt(_mapperTexHeight - (uv.y * _mapperTexHeight));

                    if (_coordLabelX != null) _coordLabelX.text = _lastPixelX.ToString();
                    if (_coordLabelY != null) _coordLabelY.text = _lastPixelY.ToString();

                    LastHitPoint = hit.point;
                    LastHitNormal = hit.normal;

                    Repaint(); 
                    break;
                }
            }
        }

        #endregion

        #region Embedded Furality Layout Generator

        private void GenerateFuralityLayouts()
        {
            string basePath = "Assets/VixenTools/Badges/Template Files/Furality";

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            // --- Furality Luma ---
            GenerateLayout(basePath, "Furality Luma", new BadgeLayout {
                nameX = 2258, nameY = 1224, nameW = 2538, nameH = 855,
                titleX = 2701, titleY = 1677, titleW = 1554, titleH = 257,
                neonColor = Color.white, emiMaskColor = Color.white, matBaseColor = Color.white
            });

            // --- Furality Umbra ---
            GenerateLayout(basePath, "Furality Umbra", new BadgeLayout {
                nameX = 2258, nameY = 1224, nameW = 2538, nameH = 855,
                titleX = 2701, titleY = 1677, titleW = 1554, titleH = 257,
                neonColor = Color.white, emiMaskColor = Color.white, matBaseColor = Color.white
            });

            // --- Furality Somna ---
            ColorUtility.TryParseHtmlString("#ffeead", out Color somnaColor);
            GenerateLayout(basePath, "Furality Somna", new BadgeLayout {
                nameX = 375, nameY = 700, nameW = 610, nameH = 150,
                titleX = 450, titleY = 810, titleW = 449, titleH = 75,
                neonColor = somnaColor, emiMaskColor = Color.white, matBaseColor = Color.white
            });

            // --- Furality Sylva ---
            ColorUtility.TryParseHtmlString("#66ff00", out Color sylvaColor);
            GenerateLayout(basePath, "Furality Sylva", new BadgeLayout {
                nameX = 1968, nameY = 1273, nameW = 1650, nameH = 450,
                titleX = 2005, titleY = 1707, titleW = 1250, titleH = 300,
                neonColor = sylvaColor, emiMaskColor = Color.white, matBaseColor = Color.white
            });

            // --- Furality Ultra ---
            ColorUtility.TryParseHtmlString("#ff00aa", out Color ultraColor);
            GenerateLayout(basePath, "Furality Ultra", new BadgeLayout {
                nameX = 500, nameY = 300, nameW = 750, nameH = 750,
                titleX = 550, titleY = 600, titleW = 650, titleH = 750,
                neonColor = ultraColor, emiMaskColor = Color.white, matBaseColor = Color.white
            });

            AssetDatabase.Refresh();
            Debug.Log("[VixForge] Successfully generated all Furality layout.json templates!");
        }

        private void GenerateLayout(string basePath, string conventionName, BadgeLayout layout)
        {
            string conventionPath = Path.Combine(basePath, conventionName);
            if (!Directory.Exists(conventionPath))
            {
                Directory.CreateDirectory(conventionPath);
            }

            string jsonPath = Path.Combine(conventionPath, "layout.json");
            string jsonContent = JsonUtility.ToJson(layout, true);
            File.WriteAllText(jsonPath, jsonContent);
        }

        #endregion
    }
}
#endif