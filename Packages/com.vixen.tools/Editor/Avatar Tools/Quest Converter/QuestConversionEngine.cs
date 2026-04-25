#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ImageMagick;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Dynamics.PhysBone.Components;
#endif

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: Non-destructive Quest material and hierarchy conversion engine.
    /// Features ImageMagick Lanczos downsampling for high-fidelity mobile textures,
    /// and an Interactive Topology Matrix allowing creators to manually override 
    /// heuristic PhysBone culling prior to execution.
    /// </summary>
    public class QuestConversionEngine : EditorWindow
    {
        // Centralized styling paths
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/QuestConversionEngineStyles.uss";

        private Font _cyberFont;

        private GameObject _sourceAvatar;
        
        // Scan Data
        private int _rendererCount = 0;
        private int _uniqueMaterialCount = 0;
        private int _uniqueTextureCount = 0;
        private bool _hasScanned = false;

        private enum TargetQuestShader
        {
            VRCMobileToonStandard, 
            VRCMobileToonLit,
            VRCMobileStandard,
            VRCMobileMatcap,
            UnityMobileStandard
        }
        private TargetQuestShader _selectedTargetShader = TargetQuestShader.VRCMobileToonStandard;

        // Texture Memory Caps for Mobile
        private List<string> _textureSizeLabels = new List<string> { 
            "256 (Aggressive - 10MB Excellent Limit)", 
            "512 (Balanced - 18MB Good Limit)", 
            "1024 (Standard - 40MB Poor Limit)", 
            "2048 (Heavy - Very Poor/Unoptimized)" 
        };
        private int[] _textureSizeOptions = { 256, 512, 1024, 2048 };
        private int _selectedTextureSizeIndex = 2;

        // VRChat Mobile Performance Ranks
        private enum MobilePerformanceRank { Excellent, Good, Medium, Poor }
        private MobilePerformanceRank _targetPerformanceRank = MobilePerformanceRank.Poor;

        private const string BASE_OUTPUT_DIR = "Assets/VixenTools/QuestConversion";
        
        // Runtime execution caches
        private Dictionary<Texture, Texture> _textureCache = new Dictionary<Texture, Texture>();
        private string _activeTexturesDir;

#if VRC_SDK_VRCSDK3
        // Interactive Topology Matrix State
        private class TopologyNode
        {
            public Component component;
            public string relativePath;
            public int depth;
            public bool keep;
        }
        private List<TopologyNode> _scannedPhysBones = new List<TopologyNode>();
        private List<TopologyNode> _scannedColliders = new List<TopologyNode>();
#endif

        // UI Elements
        private VisualElement _dynamicContainer;
        private ScrollView _topologyScrollView;
        private Label _topologyCountLabel;

        [MenuItem("VixenTools/Avatars/Quest Conversion Engine")]
        public static void ShowWindow()
        {
            var window = GetWindow<QuestConversionEngine>("Quest Engine");
            window.minSize = new Vector2(500, 650);
            window.Show();
        }

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "quest-engine-root";

            // Load USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);
            else Debug.LogWarning($"[VixenTools] Could not load Stylesheet at {UssPath}");

            // --- HEADER ---
            var headerRect = new VisualElement { name = "tool-header" };
            var titleLabel = new Label("<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> QUEST ENGINE") { enableRichText = true };
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            headerRect.Add(titleLabel);
            root.Add(headerRect);

            // --- SCROLL CONTENT ---
            var mainScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            var scrollContent = new VisualElement();
            mainScroll.Add(scrollContent);
            root.Add(mainScroll);

            BuildPhase1UI(scrollContent);

            // Container for everything that spawns AFTER clicking "Scan Matrix"
            _dynamicContainer = new VisualElement();
            scrollContent.Add(_dynamicContainer);
        }

        private void BuildPhase1UI(VisualElement container)
        {
            var panel = CreateCyberPanel("Phase 1: Deep Matrix Scanning", "#00e5ff");

            var infoLabel = new Label("Select the root of your PC Avatar. This process is 100% non-destructive. The original will be disabled, and a Quest-isolated clone will be generated.");
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
#if VRC_SDK_VRCSDK3
                    ApplyHeuristicCullingRules();
                    RefreshTopologyUI();
#endif
                }
            });
            panel.Add(rankEnum);

            var scanBtn = new Button(AnalyzeHierarchy) { text = "Scan Matrix Architecture" };
            scanBtn.AddToClassList("cyber-action-btn");
            scanBtn.AddToClassList("cyan-btn");
            panel.Add(scanBtn);

            container.Add(panel);
        }

        private void BuildDynamicResultsUI()
        {
            _dynamicContainer.Clear();

            // --- RESULTS BOX ---
            var resultsBox = new VisualElement();
            resultsBox.AddToClassList("results-box");

            string resultsText = $"<b><color=#00e5ff>■</color> MATRIX SCAN RESULTS:</b>\n\n" +
                             $"  • Renderers Detected: <b><color=#ff00aa>{_rendererCount}</color></b>\n" +
                             $"  • Unique Materials: <b><color=#ff00aa>{_uniqueMaterialCount}</color></b>\n" +
                             $"  • Unique Textures: <b><color=#ff00aa>{_uniqueTextureCount}</color></b>";

#if VRC_SDK_VRCSDK3
            resultsText += $"\n  • PhysBones Found: <b><color=#ff00aa>{_scannedPhysBones.Count}</color></b>\n" +
                           $"  • PhysBone Colliders: <b><color=#ff00aa>{_scannedColliders.Count}</color></b>";
#endif
            
            var resultsLabel = new Label(resultsText) { enableRichText = true };
            resultsBox.Add(resultsLabel);
            _dynamicContainer.Add(resultsBox);

#if VRC_SDK_VRCSDK3
            BuildTopologyUI(_dynamicContainer);
#endif

            // --- PHASE 2 EXECUTION ---
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

#if VRC_SDK_VRCSDK3
        private void BuildTopologyUI(VisualElement container)
        {
            var panel = CreateCyberPanel("Phase 1.5: Interactive Topology Matrix", "#00e5ff");

            var infoLabel = new Label("Heuristic limits have been applied based on your Target Performance Rank. You may manually override which physics components survive the Quest conversion.");
            infoLabel.AddToClassList("info-box-styled");
            panel.Add(infoLabel);

            var foldout = new Foldout { text = "Topology Culling Matrix (PhysBones)", value = true };
            foldout.AddToClassList("bold-foldout");

            _topologyCountLabel = new Label() { enableRichText = true };
            _topologyCountLabel.style.fontSize = 14;
            _topologyCountLabel.style.marginBottom = 10;
            foldout.Add(_topologyCountLabel);

            _topologyScrollView = new ScrollView(ScrollViewMode.Vertical);
            _topologyScrollView.style.height = 300;
            _topologyScrollView.AddToClassList("topology-scroll-view");
            foldout.Add(_topologyScrollView);

            panel.Add(foldout);
            container.Add(panel);

            RefreshTopologyUI();
        }

        private void RefreshTopologyUI()
        {
            if (_topologyScrollView == null || _topologyCountLabel == null) return;

            int pbKept = _scannedPhysBones.Count(n => n.keep);
            int limit = GetMaxPhysBones();
            Color countColor = pbKept > limit ? Color.red : new Color(0.2f, 0.8f, 0.2f);
            
            _topologyCountLabel.text = $"Selected: <color=#{ColorUtility.ToHtmlStringRGB(countColor)}><b>{pbKept} / {limit}</b></color> Allowed";

            _topologyScrollView.Clear();

            foreach (var node in _scannedPhysBones)
            {
                var row = new VisualElement();
                row.AddToClassList("topology-row");

                var toggle = new Toggle { value = node.keep };
                toggle.RegisterValueChangedCallback(e => 
                {
                    node.keep = e.newValue;
                    // Dynamically update the ratio string without full rebuild
                    int currentKept = _scannedPhysBones.Count(n => n.keep);
                    Color c = currentKept > limit ? Color.red : new Color(0.2f, 0.8f, 0.2f);
                    _topologyCountLabel.text = $"Selected: <color=#{ColorUtility.ToHtmlStringRGB(c)}><b>{currentKept} / {limit}</b></color> Allowed";
                });
                row.Add(toggle);

                string displayPath = node.relativePath;
                int lastSlash = displayPath.LastIndexOf('/');
                
                if (lastSlash >= 0 && lastSlash < displayPath.Length - 1)
                {
                    string baseDir = displayPath.Substring(0, lastSlash + 1);
                    string boneName = displayPath.Substring(lastSlash + 1);
                    displayPath = $"<color=#00ff66>{baseDir}</color><b><color=#00ff66>{boneName}</color></b>";
                }
                else if (string.IsNullOrEmpty(displayPath))
                {
                    displayPath = "<b><color=#00e5ff>[Avatar Root]</color></b>";
                }
                else
                {
                    displayPath = $"<b><color=#00ff66>{displayPath}</color></b>";
                }

                var label = new Label(displayPath) { enableRichText = true };
                label.AddToClassList("topology-label");
                row.Add(label);

                _topologyScrollView.Add(row);
            }
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

        private void AnalyzeHierarchy()
        {
            if (_sourceAvatar == null)
            {
                Debug.LogWarning("[VixenTools] No Source Avatar selected for scanning.");
                return;
            }

            Renderer[] renderers = _sourceAvatar.GetComponentsInChildren<Renderer>(true);
            _rendererCount = renderers.Length;
            
            HashSet<Material> uniqueMats = new HashSet<Material>();
            HashSet<Texture> uniqueTexs = new HashSet<Texture>();

            foreach (var rend in renderers)
            {
                if (rend == null) continue;
                foreach (var mat in rend.sharedMaterials)
                {
                    if (mat != null) 
                    {
                        uniqueMats.Add(mat);
                        if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null) uniqueTexs.Add(mat.GetTexture("_MainTex"));
                        if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") != null) uniqueTexs.Add(mat.GetTexture("_BaseMap"));
                        if (mat.HasProperty("_EmissionMap") && mat.GetTexture("_EmissionMap") != null) uniqueTexs.Add(mat.GetTexture("_EmissionMap"));
                    }
                }
            }

            _uniqueMaterialCount = uniqueMats.Count;
            _uniqueTextureCount = uniqueTexs.Count;

#if VRC_SDK_VRCSDK3
            _scannedPhysBones.Clear();
            _scannedColliders.Clear();

            VRCPhysBone[] pbs = _sourceAvatar.GetComponentsInChildren<VRCPhysBone>(true);
            foreach (var pb in pbs)
            {
                _scannedPhysBones.Add(new TopologyNode {
                    component = pb,
                    relativePath = AnimationUtility.CalculateTransformPath(pb.transform, _sourceAvatar.transform),
                    depth = GetHierarchyDepth(pb.transform),
                    keep = true
                });
            }

            VRCPhysBoneCollider[] cols = _sourceAvatar.GetComponentsInChildren<VRCPhysBoneCollider>(true);
            foreach (var col in cols)
            {
                _scannedColliders.Add(new TopologyNode {
                    component = col,
                    relativePath = AnimationUtility.CalculateTransformPath(col.transform, _sourceAvatar.transform),
                    depth = GetHierarchyDepth(col.transform),
                    keep = true
                });
            }

            ApplyHeuristicCullingRules();
#endif

            _hasScanned = true;
            BuildDynamicResultsUI();
            Debug.Log($"[VixenTools] Matrix Scan Complete.");
        }

#if VRC_SDK_VRCSDK3
        private void ApplyHeuristicCullingRules()
        {
            int maxPB = GetMaxPhysBones();
            int maxCol = GetMaxColliders();

            _scannedPhysBones = _scannedPhysBones.OrderBy(n => n.depth).ToList();
            for (int i = 0; i < _scannedPhysBones.Count; i++)
            {
                _scannedPhysBones[i].keep = (i < maxPB);
            }

            _scannedColliders = _scannedColliders.OrderBy(n => n.depth).ToList();
            for (int i = 0; i < _scannedColliders.Count; i++)
            {
                _scannedColliders[i].keep = (i < maxCol);
            }
        }

        private int GetMaxPhysBones()
        {
            switch (_targetPerformanceRank)
            {
                case MobilePerformanceRank.Excellent: return 0;
                case MobilePerformanceRank.Good: return 4;
                case MobilePerformanceRank.Medium: return 6;
                default: return 8;
            }
        }

        private int GetMaxColliders()
        {
            switch (_targetPerformanceRank)
            {
                case MobilePerformanceRank.Excellent: return 0;
                case MobilePerformanceRank.Good: return 4;
                case MobilePerformanceRank.Medium: return 8;
                default: return 16;
            }
        }

        private int GetHierarchyDepth(Transform t)
        {
            int depth = 0;
            while (t.parent != null)
            {
                depth++;
                t = t.parent;
            }
            return depth;
        }
#endif

        private void ExecuteConversion()
        {
            if (_sourceAvatar == null) return;

            try
            {
                EditorUtility.DisplayProgressBar("VixenTools Quest Engine", "Initializing Directory Structures...", 0.1f);

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

                EditorUtility.DisplayProgressBar("VixenTools Quest Engine", "Generating Prefab Sandbox...", 0.2f);
                string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{avatarDir}/{avatarName}_Base.prefab");
                GameObject tempPrefab = PrefabUtility.SaveAsPrefabAsset(_sourceAvatar, prefabPath);

                Undo.RecordObject(_sourceAvatar, "Disable PC Avatar");
                _sourceAvatar.SetActive(false);

                GameObject questClone = (GameObject)PrefabUtility.InstantiatePrefab(tempPrefab);
                questClone.name = questName;
                questClone.transform.position = _sourceAvatar.transform.position;
                questClone.transform.rotation = _sourceAvatar.transform.rotation;
                questClone.transform.parent = _sourceAvatar.transform.parent;

                EditorUtility.DisplayProgressBar("VixenTools Quest Engine", "Cloning and Converting Materials...", 0.4f);
                Renderer[] cloneRenderers = questClone.GetComponentsInChildren<Renderer>(true);
                Dictionary<Material, Material> materialCache = new Dictionary<Material, Material>();
                Shader targetShader = GetShaderForEnum(_selectedTargetShader);

                for (int r = 0; r < cloneRenderers.Length; r++)
                {
                    Renderer rend = cloneRenderers[r];
                    if (rend == null) continue;

                    EditorUtility.DisplayProgressBar("VixenTools Quest Engine", $"Processing Materials ({r}/{cloneRenderers.Length})...", 0.4f + (0.3f * ((float)r / cloneRenderers.Length)));

                    Material[] currentMats = rend.sharedMaterials;
                    Material[] newMats = new Material[currentMats.Length];

                    for (int i = 0; i < currentMats.Length; i++)
                    {
                        Material originalMat = currentMats[i];
                        if (originalMat == null) continue;

                        if (materialCache.TryGetValue(originalMat, out Material cachedNewMat))
                        {
                            newMats[i] = cachedNewMat;
                            continue;
                        }

                        Material questMat = new Material(targetShader);
                        questMat.name = $"{originalMat.name}_Quest";

                        TransferProperties(originalMat, questMat);

                        string matPath = AssetDatabase.GenerateUniqueAssetPath($"{materialsDir}/{questMat.name}.mat");
                        AssetDatabase.CreateAsset(questMat, matPath);
                        
                        materialCache.Add(originalMat, questMat);
                        newMats[i] = questMat;
                    }

                    rend.sharedMaterials = newMats;
                    EditorUtility.SetDirty(rend);
                }

#if VRC_SDK_VRCSDK3
                EditorUtility.DisplayProgressBar("VixenTools Quest Engine", "Applying Matrix Topology Overrides...", 0.8f);
                ApplyTopologyOverrides(questClone);
#endif

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                PrefabUtility.UnpackPrefabInstance(questClone, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                Selection.activeGameObject = questClone;
                
                Debug.Log($"[VixenTools] Quest Conversion Complete! {materialCache.Count} materials and {_textureCache.Count} high-fidelity textures processed.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _textureCache.Clear(); 
            }
        }

#if VRC_SDK_VRCSDK3
        private void ApplyTopologyOverrides(GameObject clone)
        {
            foreach (var node in _scannedPhysBones)
            {
                if (!node.keep)
                {
                    Transform targetTransform = string.IsNullOrEmpty(node.relativePath) ? clone.transform : clone.transform.Find(node.relativePath);
                    if (targetTransform != null)
                    {
                        VRCPhysBone pb = targetTransform.GetComponent<VRCPhysBone>();
                        if (pb != null) DestroyImmediate(pb, true);
                    }
                }
            }

            foreach (var node in _scannedColliders)
            {
                if (!node.keep)
                {
                    Transform targetTransform = string.IsNullOrEmpty(node.relativePath) ? clone.transform : clone.transform.Find(node.relativePath);
                    if (targetTransform != null)
                    {
                        VRCPhysBoneCollider col = targetTransform.GetComponent<VRCPhysBoneCollider>();
                        if (col != null) DestroyImmediate(col, true);
                    }
                }
            }
        }
#endif

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
                target.SetTexture("_MainTex", ProcessAndCloneTexture(source.GetTexture("_MainTex")));
            else if (source.HasProperty("_BaseMap") && target.HasProperty("_MainTex")) 
                target.SetTexture("_MainTex", ProcessAndCloneTexture(source.GetTexture("_BaseMap")));

            if (source.HasProperty("_Color") && target.HasProperty("_Color"))
                target.SetColor("_Color", source.GetColor("_Color"));
            else if (source.HasProperty("_BaseColor") && target.HasProperty("_Color"))
                target.SetColor("_Color", source.GetColor("_BaseColor"));

            if (source.HasProperty("_EmissionMap") && target.HasProperty("_EmissionMap"))
                target.SetTexture("_EmissionMap", ProcessAndCloneTexture(source.GetTexture("_EmissionMap")));
            
            if (source.HasProperty("_EmissionColor") && target.HasProperty("_EmissionColor"))
                target.SetColor("_EmissionColor", source.GetColor("_EmissionColor"));

            if (source.HasProperty("_BumpMap") && target.HasProperty("_BumpMap"))
            {
                target.SetTexture("_BumpMap", ProcessAndCloneTexture(source.GetTexture("_BumpMap")));
                
                if (source.HasProperty("_BumpScale") && target.HasProperty("_BumpScale"))
                    target.SetFloat("_BumpScale", source.GetFloat("_BumpScale"));
            }

            if (target.HasProperty("_MetallicMap"))
            {
                if (source.HasProperty("_MetallicGlossMap"))
                    target.SetTexture("_MetallicMap", ProcessAndCloneTexture(source.GetTexture("_MetallicGlossMap")));
                
                if (source.HasProperty("_Metallic") && target.HasProperty("_MetallicStrength"))
                    target.SetFloat("_MetallicStrength", source.GetFloat("_Metallic"));
                
                if (source.HasProperty("_Glossiness") && target.HasProperty("_GlossStrength"))
                    target.SetFloat("_GlossStrength", source.GetFloat("_Glossiness"));
            }
        }

        private Texture ProcessAndCloneTexture(Texture sourceTex)
        {
            if (sourceTex == null) return null;
            if (_textureCache.TryGetValue(sourceTex, out Texture cachedTex)) return cachedTex;

            string sourcePath = AssetDatabase.GetAssetPath(sourceTex);
            if (string.IsNullOrEmpty(sourcePath)) return sourceTex; 

            string texName = sourceTex.name;
            string extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(extension)) extension = ".png";

            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{_activeTexturesDir}/{texName}_Quest{extension}");
            int targetSize = _textureSizeOptions[_selectedTextureSizeIndex];

            try
            {
                using (MagickImage img = new MagickImage(sourcePath))
                {
                    if (img.Width > targetSize || img.Height > targetSize)
                    {
                        MagickGeometry size = new MagickGeometry((uint)targetSize, (uint)targetSize);
                        size.IgnoreAspectRatio = false;
                        img.FilterType = FilterType.Lanczos; 
                        img.Resize(size);
                    }
                    img.Write(newPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VixenTools] ImageMagick processing failed for {texName}, falling back to Unity Copy. Error: {ex.Message}");
                AssetDatabase.CopyAsset(sourcePath, newPath);
            }

            AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(newPath) as TextureImporter;
            
            if (importer != null)
            {
                importer.maxTextureSize = targetSize;
                
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