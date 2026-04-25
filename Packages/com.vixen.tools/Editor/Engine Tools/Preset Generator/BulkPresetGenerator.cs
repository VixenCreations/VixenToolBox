#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: A unified pipeline tool that handles both bulk extraction of presets 
    /// from existing assets, and the programmatic authoring of standardized Importer presets 
    /// from scratch using a Phantom Asset architecture.
    /// </summary>
    public class BulkPresetGenerator : EditorWindow
    {
        private enum ToolMode { Extraction, Authoring }
        private ToolMode _currentMode = ToolMode.Extraction;

        // Centralized styling paths
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/BulkPresetGeneratorStyles.uss";

        private Font _cyberFont;

        // --- Shared Configuration ---
        private string _outputDirectory = "Assets/VixenTools/GeneratedPresets";

        // --- Extraction Variables ---
        private bool _ignoreTransforms = true;
        private bool _includeChildren = false;
        private bool _registerExtractionToManager = true;
        private string _extractionFilter = "";

        // --- Authoring Variables (Texture Standards) ---
        private string _authoringPresetName = "Global_4K_Texture_Standard";
        private int _maxTextureSize = 4096;
        private TextureImporterType _textureType = TextureImporterType.Default;
        private bool _enableMipMaps = true;
        private bool _isReadable = false;
        private bool _registerAuthoringToManager = true;
        private string _authoringFilter = ""; 

        // --- UI Elements ---
        private Button _btnExtractionTab;
        private Button _btnAuthoringTab;
        private VisualElement _extractionContainer;
        private VisualElement _authoringContainer;

        private List<string> _texSizeLabels = new List<string> { "1024", "2048", "4096", "8192" };
        private int[] _texSizeValues = { 1024, 2048, 4096, 8192 };

        [MenuItem("VixenTools/Unity Engine/Pipeline Preset Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BulkPresetGenerator>("Preset Manager");
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
            root.name = "preset-manager-root";

            // Load USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);
            else Debug.LogWarning($"[VixenTools] Could not load Stylesheet at {UssPath}");

            // --- HEADER ---
            var headerRect = new VisualElement { name = "tool-header" };
            var titleLabel = new Label("<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> PRESET MANAGER") { enableRichText = true };
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            headerRect.Add(titleLabel);
            root.Add(headerRect);

            // --- TABS ---
            var tabContainer = new VisualElement { name = "tab-toolbar" };
            
            _btnExtractionTab = new Button(() => SwitchMode(ToolMode.Extraction)) { text = "EXTRACTION PIPELINE" };
            _btnAuthoringTab = new Button(() => SwitchMode(ToolMode.Authoring)) { text = "AUTHORING ENGINE" };
            
            tabContainer.Add(_btnExtractionTab);
            tabContainer.Add(_btnAuthoringTab);
            root.Add(tabContainer);

            // --- SCROLL CONTENT ---
            var scrollContainer = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            var scrollContent = new VisualElement();
            scrollContainer.Add(scrollContent);
            root.Add(scrollContainer);

            // --- BUILD UI CONTAINERS ---
            _extractionContainer = new VisualElement();
            BuildExtractionUI(_extractionContainer);
            scrollContent.Add(_extractionContainer);

            _authoringContainer = new VisualElement();
            BuildAuthoringUI(_authoringContainer);
            scrollContent.Add(_authoringContainer);

            SwitchMode(_currentMode);
        }

        private void SwitchMode(ToolMode mode)
        {
            _currentMode = mode;
            
            if (mode == ToolMode.Extraction)
            {
                _btnExtractionTab.AddToClassList("tab-btn-active");
                _btnExtractionTab.RemoveFromClassList("tab-btn-inactive");
                _btnAuthoringTab.AddToClassList("tab-btn-inactive");
                _btnAuthoringTab.RemoveFromClassList("tab-btn-active");

                _extractionContainer.style.display = DisplayStyle.Flex;
                _authoringContainer.style.display = DisplayStyle.None;
            }
            else
            {
                _btnAuthoringTab.AddToClassList("tab-btn-active");
                _btnAuthoringTab.RemoveFromClassList("tab-btn-inactive");
                _btnExtractionTab.AddToClassList("tab-btn-inactive");
                _btnExtractionTab.RemoveFromClassList("tab-btn-active");

                _extractionContainer.style.display = DisplayStyle.None;
                _authoringContainer.style.display = DisplayStyle.Flex;
            }
        }

        private void BuildExtractionUI(VisualElement container)
        {
            var panel = CreateCyberPanel("Bulk Preset Extraction", "#00e5ff");

            var infoLabel = new Label("Select objects in your hierarchy or project. This tool will rip their component configurations into reusable Unity Presets.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            var outDirField = new TextField("Output Directory") { value = _outputDirectory };
            outDirField.RegisterValueChangedCallback(e => _outputDirectory = e.newValue);
            panel.Add(outDirField);

            var filterField = new TextField("Preset Filter (Optional)") { value = _extractionFilter, tooltip = "Filter string applied in the Preset Manager. Leave blank to apply to all." };
            filterField.RegisterValueChangedCallback(e => _extractionFilter = e.newValue);
            panel.Add(filterField);

            var paramHeader = new Label("Pipeline Parameters") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } };
            panel.Add(paramHeader);

            var ignoreToggle = new Toggle("Ignore Transforms") { value = _ignoreTransforms };
            ignoreToggle.RegisterValueChangedCallback(e => _ignoreTransforms = e.newValue);
            panel.Add(ignoreToggle);

            var childToggle = new Toggle("Include Children") { value = _includeChildren };
            childToggle.RegisterValueChangedCallback(e => _includeChildren = e.newValue);
            panel.Add(childToggle);

            var regToggle = new Toggle("Auto-Register to Manager") { value = _registerExtractionToManager };
            regToggle.RegisterValueChangedCallback(e => _registerExtractionToManager = e.newValue);
            panel.Add(regToggle);

            container.Add(panel);

            var execBtn = new Button(ExecuteExtraction) { text = "Extract Presets from Selection" };
            execBtn.AddToClassList("cyber-action-btn");
            execBtn.AddToClassList("cyan-btn");
            container.Add(execBtn);
        }

        private void BuildAuthoringUI(VisualElement container)
        {
            var panel = CreateCyberPanel("Programmatic Asset Authoring", "#ff00aa");

            var infoLabel = new Label("Defines strict import standards (e.g., 4K texture caps, mip-map rules) and generates a master preset without needing a source asset.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            var outDirField = new TextField("Output Directory") { value = _outputDirectory };
            outDirField.RegisterValueChangedCallback(e => _outputDirectory = e.newValue);
            panel.Add(outDirField);

            var nameField = new TextField("Preset Name") { value = _authoringPresetName };
            nameField.RegisterValueChangedCallback(e => _authoringPresetName = e.newValue);
            panel.Add(nameField);

            var filterField = new TextField("Preset Filter (Glob)") { value = _authoringFilter, tooltip = "Example: glob:\"**/*_BaseColor.png\"" };
            filterField.RegisterValueChangedCallback(e => _authoringFilter = e.newValue);
            panel.Add(filterField);

            var rulesHeader = new Label("Texture Import Rules") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } };
            panel.Add(rulesHeader);

            var typeEnum = new EnumField("Texture Type", _textureType);
            typeEnum.RegisterValueChangedCallback(e => _textureType = (TextureImporterType)e.newValue);
            panel.Add(typeEnum);

            int initialSizeIndex = System.Array.IndexOf(_texSizeValues, _maxTextureSize);
            if (initialSizeIndex == -1) initialSizeIndex = 2; // Default to 4096
            var sizeDropdown = new DropdownField("Max Texture Size", _texSizeLabels, initialSizeIndex);
            sizeDropdown.RegisterValueChangedCallback(e => _maxTextureSize = _texSizeValues[_texSizeLabels.IndexOf(e.newValue)]);
            panel.Add(sizeDropdown);

            var mipToggle = new Toggle("Generate Mip Maps") { value = _enableMipMaps };
            mipToggle.RegisterValueChangedCallback(e => _enableMipMaps = e.newValue);
            panel.Add(mipToggle);

            var readToggle = new Toggle("Read/Write Enabled") { value = _isReadable };
            readToggle.RegisterValueChangedCallback(e => _isReadable = e.newValue);
            panel.Add(readToggle);

            var regToggle = new Toggle("Auto-Register to Manager") { value = _registerAuthoringToManager };
            regToggle.RegisterValueChangedCallback(e => _registerAuthoringToManager = e.newValue);
            panel.Add(regToggle);

            container.Add(panel);

            var execBtn = new Button(ExecuteTextureAuthoring) { text = "Author Texture Standard Preset" };
            execBtn.AddToClassList("cyber-action-btn");
            execBtn.AddToClassList("pink-btn");
            container.Add(execBtn);
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

        #region Execution Logic
        private void ExecuteExtraction()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("[VixenTools] No objects selected for extraction.");
                return;
            }

            EnsureDirectoryExists(_outputDirectory);
            int count = 0;

            foreach (var obj in selectedObjects)
            {
                Component[] components = _includeChildren ? obj.GetComponentsInChildren<Component>(true) : obj.GetComponents<Component>();
                
                foreach (var comp in components)
                {
                    if (comp == null || (_ignoreTransforms && comp is Transform)) continue;
                    
                    Preset preset = new Preset(comp);
                    string typeName = comp.GetType().Name;
                    string path = AssetDatabase.GenerateUniqueAssetPath($"{_outputDirectory}/{obj.name}_{typeName}.preset");
                    
                    AssetDatabase.CreateAsset(preset, path);
                    count++;

                    if (_registerExtractionToManager)
                    {
                        InjectIntoPresetManager(preset, _extractionFilter);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[VixenTools] Extracted {count} presets to {_outputDirectory}.");
        }

        private void ExecuteTextureAuthoring()
        {
            EnsureDirectoryExists(_outputDirectory);

            // 1. Create a "Phantom Asset" (Temporary file to base the importer on)
            string phantomPath = "Assets/VixenTools_PhantomTexture.png";
            File.WriteAllBytes(phantomPath, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // Minimal valid PNG header
            AssetDatabase.ImportAsset(phantomPath, ImportAssetOptions.ForceUpdate);

            // 2. Grab the importer and inject our standardized rules
            TextureImporter importer = AssetImporter.GetAtPath(phantomPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = _textureType;
                importer.maxTextureSize = _maxTextureSize;
                importer.mipmapEnabled = _enableMipMaps;
                importer.isReadable = _isReadable;
                importer.SaveAndReimport();

                // 3. Rip the configuration into a permanent Preset
                Preset newPreset = new Preset(importer);
                string presetPath = AssetDatabase.GenerateUniqueAssetPath($"{_outputDirectory}/{_authoringPresetName}.preset");
                AssetDatabase.CreateAsset(newPreset, presetPath);

                if (_registerAuthoringToManager)
                {
                    InjectIntoPresetManager(newPreset, _authoringFilter);
                }

                Debug.Log($"[VixenTools] Authored Master Texture Preset: {presetPath}");
            }

            // 4. Clean up the Phantom Asset
            AssetDatabase.DeleteAsset(phantomPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void InjectIntoPresetManager(Preset newPreset, string filter)
        {
            PresetType targetType = newPreset.GetPresetType();
            DefaultPreset[] currentDefaults = Preset.GetDefaultPresetsForType(targetType);
            
            if (currentDefaults.Any(dp => dp.preset == newPreset && dp.filter == filter))
                return;

            List<DefaultPreset> updatedDefaults = new List<DefaultPreset>(currentDefaults);
            
            updatedDefaults.Insert(0, new DefaultPreset
            {
                preset = newPreset,
                filter = filter
            });

            Preset.SetDefaultPresetsForType(targetType, updatedDefaults.ToArray());
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }
        #endregion
    }
}
#endif