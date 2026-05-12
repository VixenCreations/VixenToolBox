#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Profiling;
using UnityEditor.IMGUI.Controls; 
using VRC.Udon;
using VRC.SDK3.Components;
using UdonSharp;
using TMPro;
using ImageMagick;

namespace VixenTools.Editor
{
    public class VixenWorldEngine : EditorWindow
    {
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenWorldSpider.uss";
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        
        private const string TargetDictPath = "Assets/VixenTools/Asset Database/World Engine/VixenReplacementTargets.asset";
        private const string WhitelistDictPath = "Assets/VixenTools/Asset Database/World Engine/VixenShaderWhitelist.asset";

        private ScrollView _mainScroll;
        private VisualElement _matrixContainer;
        private Font _cyberFont;

        private int _targetTextureResolution = 2048;
        private readonly List<string> _resolutionOptions = new List<string> { "512", "1024", "2048", "4096" };
        
        // Font Swap Targets
        private TMP_FontAsset _targetTMPFont; 
        private Font _targetLegacyFont; 
        
        private Shader _targetReplacementShader;
        private ShaderDictionaryAsset _targetShaderAsset;
        private ShaderDictionaryAsset _shaderWhitelistAsset;
        private Button _shaderSelectButton; 
        private List<string> _validShaderList = new List<string>(); 

        private HashSet<Texture> _detectedTextures = new HashSet<Texture>();
        private readonly HashSet<string> _processedTexturePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<AudioClip> _detectedAudio = new HashSet<AudioClip>();
        private HashSet<Mesh> _detectedMeshes = new HashSet<Mesh>();
        private HashSet<Texture> _detectedUITextures = new HashSet<Texture>();
        private HashSet<string> _expandedCategories = new HashSet<string>(); // <-- NEW: Tracks open UI folders

        private class EngineDiagnostic
        {
            public string Category;
            public string IssueType;
            public string Description;
            public string HexColor;
            public UnityEngine.Object Context;
            public bool IsSelected = false; // FIX 1: Default to unchecked
            public Action FixPayload;
            public Action OnFixedUIUpdate;
        }
        private List<EngineDiagnostic> _diagnosticsDb = new List<EngineDiagnostic>();

        [MenuItem("VixenTools/Scene/Vixen World Engine")]
        public static void ShowWindow() => GetWindow<VixenWorldEngine>("Vixen World Engine");

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            _targetTMPFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            _targetLegacyFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            
            if (_targetLegacyFont == null)
            {
                _targetLegacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            
            minSize = new Vector2(650, 850);
        }

        private void CreateGUI()
        {
            EnsureDictionariesExist(); 

            var root = rootVisualElement;
            root.name = "world-spider-root";
            
            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styles != null) root.styleSheets.Add(styles);

            // HEADER
            var header = new VisualElement { name = "tool-header", style = { justifyContent = Justify.Center, alignItems = Align.Center, paddingLeft = 0 } };
            var title = new Label();
            title.AddToClassList("panel-header");
            title.style.color = ColorUtility.TryParseHtmlString("#ffffff", out Color w) ? w : Color.white;
            title.text = "<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> WORLD ENGINE";
            title.enableRichText = true;
            if (_cyberFont != null) title.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            header.Add(title);
            root.Add(header);

            _mainScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            root.Add(_mainScroll);

            var infoBox = new Label("SYSTEM ACTIVE. ENGINE SCANNING HIERARCHY ACROSS ALL SDK VECTORS.") { name = "info-box" };
            infoBox.AddToClassList("info-box-styled");
            _mainScroll.Add(infoBox);

            // CONTROL PANEL
            var controlPanel = new VisualElement();
            controlPanel.AddToClassList("spider-panel");

            // Texture Resolution (replace existing block)
            var resRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
            var resLabel = new Label("TARGET MAX TEXTURE SIZE:");
            resLabel.AddToClassList("control-label");

            // Determine initial index from current _targetTextureResolution
            int initialIndex = _resolutionOptions.IndexOf(_targetTextureResolution.ToString());
            if (initialIndex < 0) initialIndex = Mathf.Clamp(_resolutionOptions.Count - 1, 0, _resolutionOptions.Count - 1); // fallback to last option

            var resDropdown = new DropdownField(_resolutionOptions, initialIndex);
            resDropdown.AddToClassList("vixen-dropdown");

            // Ensure the dropdown text matches the runtime value
            resDropdown.value = _targetTextureResolution.ToString();

            resDropdown.RegisterValueChangedCallback(evt =>
            {
                if (int.TryParse(evt.newValue, out int res))
                {
                    _targetTextureResolution = res;
                    Debug.Log($"[Vixen] Target texture resolution set to {_targetTextureResolution}");
                }
            });

            resRow.Add(resLabel);
            resRow.Add(resDropdown);
            controlPanel.Add(resRow);

            // TMP Font Swap
            var fontRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
            var fontLabel = new Label("GLOBAL TMP FONT SWAP:");
            fontLabel.AddToClassList("control-label");
            var fontField = new ObjectField { objectType = typeof(TMP_FontAsset), allowSceneObjects = false, value = _targetTMPFont };
            fontField.AddToClassList("vixen-object-field");
            fontField.RegisterValueChangedCallback(evt => _targetTMPFont = evt.newValue as TMP_FontAsset);
            fontRow.Add(fontLabel);
            fontRow.Add(fontField);
            controlPanel.Add(fontRow);

            // Legacy Font Swap
            var legacyFontRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 15 } };
            var legacyFontLabel = new Label("GLOBAL LEGACY FONT SWAP:");
            legacyFontLabel.AddToClassList("control-label");
            var legacyFontField = new ObjectField { objectType = typeof(Font), allowSceneObjects = false, value = _targetLegacyFont };
            legacyFontField.AddToClassList("vixen-object-field");
            legacyFontField.RegisterValueChangedCallback(evt => _targetLegacyFont = evt.newValue as Font);
            legacyFontRow.Add(legacyFontLabel);
            legacyFontRow.Add(legacyFontField);
            controlPanel.Add(legacyFontRow);

            // Global Shader Replacer Target
            var shaderTargetRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
            var shaderTargetLabel = new Label("REPLACEMENT TARGET:");
            shaderTargetLabel.AddToClassList("control-label");
            shaderTargetLabel.style.color = ColorUtility.TryParseHtmlString("#ff00aa", out Color p) ? p : Color.magenta; 
            
            _shaderSelectButton = new Button();
            _shaderSelectButton.AddToClassList("vixen-dropdown");
            _shaderSelectButton.style.backgroundColor = ColorUtility.TryParseHtmlString("#0a0a0f", out Color bg) ? bg : Color.black;
            _shaderSelectButton.style.borderTopColor = ColorUtility.TryParseHtmlString("#00e5ff", out Color bc) ? bc : Color.cyan;
            _shaderSelectButton.style.borderBottomColor = bc;
            _shaderSelectButton.style.borderLeftColor = bc;
            _shaderSelectButton.style.borderRightColor = bc;
            _shaderSelectButton.style.borderTopWidth = 1;
            _shaderSelectButton.style.borderBottomWidth = 1;
            _shaderSelectButton.style.borderLeftWidth = 1;
            _shaderSelectButton.style.borderRightWidth = 1;
            _shaderSelectButton.style.borderTopLeftRadius = 3;
            _shaderSelectButton.style.borderTopRightRadius = 3;
            _shaderSelectButton.style.borderBottomLeftRadius = 3;
            _shaderSelectButton.style.borderBottomRightRadius = 3;
            _shaderSelectButton.style.color = ColorUtility.TryParseHtmlString("#e0e0e0", out Color tc) ? tc : Color.white;
            _shaderSelectButton.style.fontSize = 13;
            _shaderSelectButton.style.paddingTop = 4;
            _shaderSelectButton.style.paddingBottom = 4;
            _shaderSelectButton.style.unityTextAlign = TextAnchor.MiddleLeft;

            _shaderSelectButton.clicked += () => {
                if (_validShaderList.Count == 0) return;
                var dropdown = new ShaderSelectionDropdown(new AdvancedDropdownState(), _validShaderList, (selectedName) => {
                    _targetReplacementShader = Shader.Find(selectedName);
                    _shaderSelectButton.text = selectedName;
                });
                dropdown.Show(_shaderSelectButton.worldBound); 
            };

            shaderTargetRow.Add(shaderTargetLabel);
            shaderTargetRow.Add(_shaderSelectButton);
            controlPanel.Add(shaderTargetRow);

            // Target Dictionary (.asset)
            var targetDictRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
            var targetDictLabel = new Label("TARGET DICTIONARY (.ASSET):");
            targetDictLabel.AddToClassList("control-label");
            var targetDictField = new ObjectField { objectType = typeof(ShaderDictionaryAsset), allowSceneObjects = false, value = _targetShaderAsset };
            targetDictField.AddToClassList("vixen-object-field");
            targetDictField.RegisterValueChangedCallback(evt => {
                _targetShaderAsset = evt.newValue as ShaderDictionaryAsset;
                RefreshCustomDropdown();
            });
            targetDictRow.Add(targetDictLabel);
            targetDictRow.Add(targetDictField);
            controlPanel.Add(targetDictRow);

            // Whitelist Dictionary (.asset)
            var whitelistRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 10 } };
            var whitelistLabel = new Label("WHITELIST DICTIONARY (.ASSET):");
            whitelistLabel.AddToClassList("control-label");
            var whitelistField = new ObjectField { objectType = typeof(ShaderDictionaryAsset), allowSceneObjects = false, value = _shaderWhitelistAsset };
            whitelistField.AddToClassList("vixen-object-field");
            whitelistField.RegisterValueChangedCallback(evt => {
                _shaderWhitelistAsset = evt.newValue as ShaderDictionaryAsset;
            });
            whitelistRow.Add(whitelistLabel);
            whitelistRow.Add(whitelistField);
            controlPanel.Add(whitelistRow);

            RefreshCustomDropdown();

            // Action Buttons
            var btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var scanBtn = new Button(InitiateFullMatrixScan) { text = "SCAN SCENE" };
            scanBtn.AddToClassList("spider-action-btn");
            scanBtn.AddToClassList("btn-cyan");

            var optiBtn = new Button(ExecuteSelectedProtocols) { text = "FIX SELECTED ISSUES" };
            optiBtn.AddToClassList("spider-action-btn");
            optiBtn.AddToClassList("btn-pink");

            btnRow.Add(scanBtn);
            btnRow.Add(optiBtn);
            controlPanel.Add(btnRow);
            _mainScroll.Add(controlPanel);

            _matrixContainer = new VisualElement();
            _mainScroll.Add(_matrixContainer);
        }

        private void EnsureDictionariesExist(bool forceRebuild = false)
        {
            string targetPath = Path.GetFullPath(TargetDictPath); 
            string targetDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            string whitelistPath = Path.GetFullPath(WhitelistDictPath); 
            string whitelistDir = Path.GetDirectoryName(whitelistPath);
            if (!Directory.Exists(whitelistDir)) Directory.CreateDirectory(whitelistDir);

            // THE NUKE: Delete existing files if a rebuild is triggered
            if (forceRebuild)
            {
                AssetDatabase.DeleteAsset(TargetDictPath);
                AssetDatabase.DeleteAsset(WhitelistDictPath);
                _targetShaderAsset = null;
                _shaderWhitelistAsset = null;
                Debug.Log("[Vixen System] Previous dictionaries purged. Rebuilding from fresh schema...");
            }

            _targetShaderAsset = AssetDatabase.LoadAssetAtPath<ShaderDictionaryAsset>(TargetDictPath);
            if (_targetShaderAsset == null)
            {
                _targetShaderAsset = ScriptableObject.CreateInstance<ShaderDictionaryAsset>();
                AssetDatabase.CreateAsset(_targetShaderAsset, TargetDictPath);
                ShaderDictionaryAsset.AutoPopulateTargets(_targetShaderAsset); 
            }

            _shaderWhitelistAsset = AssetDatabase.LoadAssetAtPath<ShaderDictionaryAsset>(WhitelistDictPath);
            if (_shaderWhitelistAsset == null)
            {
                _shaderWhitelistAsset = ScriptableObject.CreateInstance<ShaderDictionaryAsset>();
                AssetDatabase.CreateAsset(_shaderWhitelistAsset, WhitelistDictPath);
                ShaderDictionaryAsset.AutoPopulateWhitelist(_shaderWhitelistAsset); 
            }

            // Force Unity to acknowledge the new files immediately so the UI doesn't hitch
            if (forceRebuild) AssetDatabase.Refresh(); 
        }

        private void RefreshCustomDropdown()
        {
            if (_shaderSelectButton == null) return;

            if (_targetShaderAsset != null && _targetShaderAsset.shaders.Count > 0)
            {
                _validShaderList = _targetShaderAsset.shaders
                    .Where(s => s != null)
                    .Select(s => s.name)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                if (_validShaderList.Count > 0)
                {
                    if (_targetReplacementShader != null && _validShaderList.Contains(_targetReplacementShader.name))
                    {
                        _shaderSelectButton.text = _targetReplacementShader.name;
                    }
                    else
                    {
                        string fallback = _validShaderList.Contains("VRChat/Mobile/Toon Standard") 
                            ? "VRChat/Mobile/Toon Standard" 
                            : _validShaderList[0];
                        
                        _shaderSelectButton.text = fallback;
                        _targetReplacementShader = Shader.Find(fallback);
                    }
                    return;
                }
            }
            
            _validShaderList.Clear();
            _shaderSelectButton.text = "No Valid Targets Found";
            _targetReplacementShader = null;
        }

        // === 4D-CHESS CACHING: Prevents brutal O(N) Scene Sweeps ===
        private Dictionary<Type, UnityEngine.Object[]> _sceneObjectCache = new Dictionary<Type, UnityEngine.Object[]>();
        private Dictionary<string, Texture2D> _textureRecoveryCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        // Accepts the 'includeInactive' boolean
        private T[] GetCachedObjects<T>(bool includeInactive = true) where T : UnityEngine.Object
        {
            Type t = typeof(T);
            if (_sceneObjectCache.TryGetValue(t, out var cached)) return cached as T[];
            var objs = FindObjectsOfType<T>(includeInactive);
            _sceneObjectCache[t] = objs;
            return objs;
        }

        // Accepts the 'includeInactive' boolean for the Type-based lookups
        private UnityEngine.Object[] GetCachedObjects(Type t, bool includeInactive = true)
        {
            if (t == null) return new UnityEngine.Object[0];
            if (_sceneObjectCache.TryGetValue(t, out var cached)) return cached;
            var objs = FindObjectsOfType(t, includeInactive);
            _sceneObjectCache[t] = objs;
            return objs;
        }

        // === 4D-CHESS CACHING: Persistent JSON Scene Checksum (Failure-Aware) ===
        [Serializable]
        public class AssetRecord
        {
            public string guid;
            public string hash;
            public int lastResolution;
            public int version;
            public bool failed;
        }

        [Serializable]
        public class WorldEngineCache
        {
            public List<AssetRecord> textures = new List<AssetRecord>();
            public List<AssetRecord> meshes = new List<AssetRecord>();
        }

        private const int CURRENT_ENGINE_CACHE_VERSION = 1;

        private WorldEngineCache _worldCache = new WorldEngineCache();

        // Fast lookup maps (RAM only)
        private Dictionary<string, AssetRecord> _textureRecordMap = new Dictionary<string, AssetRecord>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, AssetRecord> _meshRecordMap = new Dictionary<string, AssetRecord>(StringComparer.OrdinalIgnoreCase);

        private string GetLookupCachePath()
        {
            string sceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "Untitled";
            return $"Assets/VixenTools/Asset Database/Scene Lookup/{sceneName}_WorldEngineLookup.json";
        }

        private void LoadLookupCache()
        {
            string path = GetLookupCachePath();
            string absolutePath = System.IO.Path.GetFullPath(path);

            if (System.IO.File.Exists(absolutePath))
            {
                string json = System.IO.File.ReadAllText(absolutePath);
                _worldCache = JsonUtility.FromJson<WorldEngineCache>(json) ?? new WorldEngineCache();
            }
            else
            {
                _worldCache = new WorldEngineCache();
            }

            _textureRecordMap.Clear();
            _meshRecordMap.Clear();

            if (_worldCache.textures != null)
            {
                foreach (var rec in _worldCache.textures)
                {
                    if (rec != null && !string.IsNullOrEmpty(rec.guid))
                        _textureRecordMap[rec.guid] = rec;
                }
            }

            if (_worldCache.meshes != null)
            {
                foreach (var rec in _worldCache.meshes)
                {
                    if (rec != null && !string.IsNullOrEmpty(rec.guid))
                        _meshRecordMap[rec.guid] = rec;
                }
            }
        }

        private void SaveLookupCache()
        {
            string path = GetLookupCachePath();
            string absolutePath = System.IO.Path.GetFullPath(path);
            string dir = System.IO.Path.GetDirectoryName(absolutePath);

            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            _worldCache.textures = _textureRecordMap.Values.ToList();
            _worldCache.meshes = _meshRecordMap.Values.ToList();

            string json = JsonUtility.ToJson(_worldCache, true);
            System.IO.File.WriteAllText(absolutePath, json);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private bool ShouldProcessTextureAsset(string guid, string assetPath)
        {
            if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(assetPath))
                return true;

            string hash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString();

            if (!_textureRecordMap.TryGetValue(guid, out var rec))
                return true;

            bool mustReprocess =
                rec.failed ||
                rec.version != CURRENT_ENGINE_CACHE_VERSION ||
                rec.lastResolution != _targetTextureResolution ||
                !string.Equals(rec.hash, hash, StringComparison.OrdinalIgnoreCase);

            return mustReprocess;
        }

        private void RecordTextureResult(string guid, string assetPath, bool success)
        {
            if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(assetPath))
                return;

            string hash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString();

            if (!_textureRecordMap.TryGetValue(guid, out var rec))
            {
                rec = new AssetRecord { guid = guid };
                _textureRecordMap[guid] = rec;
            }

            rec.hash = hash;
            rec.lastResolution = _targetTextureResolution;
            rec.version = CURRENT_ENGINE_CACHE_VERSION;
            rec.failed = !success;
        }

        private bool ShouldProcessMeshAsset(string guid, string assetPath)
        {
            if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(assetPath))
                return true;

            string hash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString();

            if (!_meshRecordMap.TryGetValue(guid, out var rec))
                return true;

            bool mustReprocess =
                rec.failed ||
                rec.version != CURRENT_ENGINE_CACHE_VERSION ||
                !string.Equals(rec.hash, hash, StringComparison.OrdinalIgnoreCase);

            return mustReprocess;
        }

        private void RecordMeshResult(string guid, string assetPath, bool success)
        {
            if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(assetPath))
                return;

            string hash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString();

            if (!_meshRecordMap.TryGetValue(guid, out var rec))
            {
                rec = new AssetRecord { guid = guid };
                _meshRecordMap[guid] = rec;
            }

            rec.hash = hash;
            rec.lastResolution = _targetTextureResolution; // Keeps schema consistent
            rec.version = CURRENT_ENGINE_CACHE_VERSION;
            rec.failed = !success;
        }

        // === 4D-CHESS CACHING: Reflection & Background Queue ===
        private static Dictionary<(Type, string), System.Reflection.FieldInfo> _fieldCache = new Dictionary<(Type, string), System.Reflection.FieldInfo>();

        private System.Reflection.FieldInfo GetFieldCached(Type t, string name, System.Reflection.BindingFlags flags)
        {
            var key = (t, name);
            if (_fieldCache.TryGetValue(key, out var fi)) return fi;
            fi = t.GetField(name, flags);
            _fieldCache[key] = fi;
            return fi;
        }

        private Queue<Action> _workQueue = new Queue<Action>();
        private bool _isProcessingQueue = false;

        private void EnqueueWork(Action a) => _workQueue.Enqueue(a);

        private void StartProcessingQueue()
        {
            if (_isProcessingQueue) return;
            if (_workQueue.Count == 0) return;

            _isProcessingQueue = true;
            AssetDatabase.StartAssetEditing(); // Suspends Unity's file watcher (CRITICAL for I/O speed)
            EditorApplication.update += ProcessQueueTick;
        }

        private void ProcessQueueTick()
        {
            int perTick = 2; // Process 2 heavy ImageMagick files per frame to keep the Editor responsive
            for (int i = 0; i < perTick && _workQueue.Count > 0; i++)
            {
                try 
                {
                    _workQueue.Dequeue().Invoke();
                } 
                catch (Exception e) 
                {
                    Debug.LogError($"[VixenWorldSpider] Queue Execution Failed on an asset: {e.Message}");
                }
            }

            if (_workQueue.Count == 0)
            {
                EditorApplication.update -= ProcessQueueTick;
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh(); // One single refresh for all modified textures
                
                // Save the persistent JSON database
                SaveLookupCache(); 
                
                _isProcessingQueue = false;
                EditorUtility.ClearProgressBar();
                
                // Re-render the UI matrix to clear the ghosts
                InitiateFullMatrixScan(); 
                Debug.Log("[Vixen System] Background Asset Queue Completed. Lookup Checksum Saved.");
            }
            else
            {
                EditorUtility.DisplayProgressBar("VIXEN SYSTEM: I/O THREAD", $"Processing Asset Queue... ({_workQueue.Count} remaining)", 0.5f);
            }
        }

        private void InitiateFullMatrixScan()
        {
            // === INTERNAL ENGINE CALLS ===
            LoadLookupCache(); // <-- Added 05/10/26
            EnsureDictionariesExist();
            RefreshCustomDropdown();

            _diagnosticsDb.Clear();
            _detectedTextures.Clear(); 
            _detectedAudio.Clear(); 
            _detectedMeshes.Clear();
            _detectedUITextures.Clear();

            // === ENGINE ARCHITECTURE AUDITS ===
            AuditUdonAndNetwork();
            AuditLightingAndCameras();
            AuditPhysics();
            AuditTerrainAndEnvironment(); // <-- Added 05/06/26
            AuditGeometryAndMaterials();
            AnalyzeTextures();
            AuditExplicitTextComponents(); 
            AuditCanvasesAndUIMemory();
            AuditUdonPersistence(); 

            // === THIRD-PARTY ECOSYSTEM AUDITS ===
            AuditNativeVideoPipelines();
            AuditLightVolumesEcosystem(); // <-- Added 05/07/26
            AuditProTVEcosystem(); 
            AuditTxlEcosystem(); 
            AuditIwaSyncEcosystem();
            AuditVizVidEcosystem(); // <-- Added 05/07/26
            AuditRinvoSearchEcosystem(); // <-- Added 05/07/26
            AuditAudioLinkEcosystem(); // <-- Added 05/07/26
            AuditLTCGIPipeline(); // <-- Added 05/09/26

            RenderDiagnosticMatrix();

            // POP OUT THE NEW HEURISTICS WINDOW
            VixenHeuristicsDashboard.Open(_detectedTextures, _detectedMeshes, _detectedAudio, _detectedUITextures);
        }

        private void RenderDiagnosticMatrix()
        {
            _matrixContainer.Clear();
            var categories = _diagnosticsDb.Select(d => d.Category).Distinct().OrderBy(c => c).ToList();

            foreach (var category in categories)
            {
                // Check if the user had this specific category open before the live refresh
                bool isExpanded = _expandedCategories.Contains(category);
                
                var foldout = new Foldout { text = category, value = isExpanded };
                foldout.AddToClassList("matrix-foldout");
                
                var contentContainer = new VisualElement();
                contentContainer.AddToClassList("topology-container");
                foldout.Add(contentContainer);

                bool isLoaded = false;
                
                // If it was already expanded from a previous scan, populate it immediately
                if (isExpanded)
                {
                    PopulateCategoryRows(category, contentContainer);
                    isLoaded = true;
                }

                // LAZY LOAD DOM: Only build the UI nodes when the user clicks the category open
                foldout.RegisterValueChangedCallback(evt => {
                    if (evt.newValue)
                    {
                        _expandedCategories.Add(category); // Memorize state
                        if (!isLoaded)
                        {
                            PopulateCategoryRows(category, contentContainer);
                            isLoaded = true;
                        }
                    }
                    else
                    {
                        _expandedCategories.Remove(category); // Forget state
                    }
                });

                _matrixContainer.Add(foldout);
            }
        }

        private void PopulateCategoryRows(string category, VisualElement contentContainer)
        {
            var catIssues = _diagnosticsDb.Where(d => d.Category == category && d.FixPayload != null).ToList();
            List<Toggle> categoryToggles = new List<Toggle>();

            if (catIssues.Count > 0)
            {
                var toggleAllRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginBottom = 4 } };
                
                var toggleAllBtn = new Button(() => {
                    bool anyUnchecked = catIssues.Any(i => !i.IsSelected);
                    for (int i = 0; i < catIssues.Count; i++)
                    {
                        catIssues[i].IsSelected = anyUnchecked;
                        if (i < categoryToggles.Count && categoryToggles[i] != null)
                        {
                            categoryToggles[i].SetValueWithoutNotify(anyUnchecked);
                        }
                    }
                }) { text = "Toggle All Fixes", style = { fontSize = 10, paddingLeft = 10, paddingRight = 10 } };
                
                toggleAllRow.Add(toggleAllBtn);
                contentContainer.Add(toggleAllRow);
            }

            var issuesInCategory = _diagnosticsDb.Where(d => d.Category == category).ToList();
            var issueTypes = issuesInCategory.Select(d => d.IssueType).Distinct().OrderBy(t => t);

            foreach (var issueType in issueTypes)
            {
                var subHeader = new Label($"■ {issueType}");
                subHeader.AddToClassList("issue-subheader");
                contentContainer.Add(subHeader);

                var specificIssues = issuesInCategory.Where(d => d.IssueType == issueType);
                foreach (var issue in specificIssues)
                {
                    var row = new VisualElement();
                    row.AddToClassList("spider-row");

                    Toggle toggle = null;
                    if (issue.FixPayload != null)
                    {
                        toggle = new Toggle { value = issue.IsSelected };
                        toggle.AddToClassList("vixen-toggle");
                        var currentIssue = issue; 
                        toggle.RegisterValueChangedCallback(e => currentIssue.IsSelected = e.newValue);
                        row.Add(toggle);
                        categoryToggles.Add(toggle);
                    }
                    else
                    {
                        var spacer = new VisualElement { style = { width = 16, marginRight = 8, marginLeft = 2 } };
                        row.Add(spacer);
                    }

                    var issueForGhost = issue;
                    issueForGhost.OnFixedUIUpdate = () => {
                        row.SetEnabled(false);
                        row.style.opacity = 0.35f; 
                        if (toggle != null) toggle.SetValueWithoutNotify(false);
                    };

                    var bar = new VisualElement { name = "indicator-bar" };
                    bar.AddToClassList("indicator-bar");
                    bar.style.backgroundColor = ColorUtility.TryParseHtmlString(issue.HexColor, out Color c) ? c : Color.white;
                    row.Add(bar);

                    var label = new Label(issue.Description) { enableRichText = true };
                    label.AddToClassList("spider-label");
                    row.Add(label);

                    if (issue.Context != null)
                    {
                        var ctx = issue.Context; 
                        row.RegisterCallback<MouseDownEvent>(e => { EditorGUIUtility.PingObject(ctx); Selection.activeObject = ctx; });
                    }

                    contentContainer.Add(row);
                }
            }
        }

        private void LogDiagnostic(string category, string type, string desc, string hex, UnityEngine.Object context, Action fixPayload = null)
        {
            _diagnosticsDb.Add(new EngineDiagnostic { 
                Category = category, 
                IssueType = type, 
                Description = desc, 
                HexColor = hex, 
                Context = context,
                FixPayload = fixPayload
            });
        }

        private Type GetTypeSafe(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = assembly.GetType(typeName);
                if (t != null) return t;
            }
            return null;
        }

        private string GetUdonTypeNameSafe(UdonBehaviour udon)
        {
            if (udon == null) return string.Empty;

            // Attempt 1: Safe reflection into UdonSharpEditorUtility (Scalable, no hard compile dependency)
            try
            {
                var editorAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "UdonSharp.Editor");
                if (editorAsm != null)
                {
                    Type utilityType = editorAsm.GetType("UdonSharp.Editor.UdonSharpEditorUtility");
                    if (utilityType != null)
                    {
                        var getTypeMethod = utilityType.GetMethod("GetUdonSharpBehaviourType", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (getTypeMethod != null)
                        {
                            Type backingType = getTypeMethod.Invoke(null, new object[] { udon }) as Type;
                            if (backingType != null) return backingType.FullName;
                        }
                    }
                }
            }
            catch (Exception) { /* Fail silently and let the heuristic fallback take over */ }

            // Attempt 2: Fallback to the physical program asset name (Heuristic)
            if (udon.programSource != null)
            {
                return udon.programSource.name;
            }

            return string.Empty;
        }

        // Struct to hold the validation results
        public struct LTCGIValidationReport
        {
            public int TotalScreens;
            public int StaleScreenCount;
            public int OrphanedRenderers;
            public bool RequiresRebuild;
        }

        // The Validator: Safely extracts and checks data without hard-linking to the LTCGI assembly
        public LTCGIValidationReport CheckForStaleLTCGIData(Component adapter)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            Type adapterType = adapter.GetType();

            var screenCountField = adapterType.GetField("_LTCGI_ScreenCount", flags);
            var dynamicCountField = adapterType.GetField("_LTCGI_ScreenCountDynamic", flags);
            
            int totalScreens = screenCountField != null ? Convert.ToInt32(screenCountField.GetValue(adapter)) : 0;
            int dynamicScreens = dynamicCountField != null ? Convert.ToInt32(dynamicCountField.GetValue(adapter)) : 0;

            var report = new LTCGIValidationReport { TotalScreens = totalScreens };
            bool needsRebuild = false;

            // Extract Arrays
            var screensField = adapterType.GetField("_Screens", flags);
            var extraDataField = adapterType.GetField("_LTCGI_ExtraData", flags);
            var transformsField = adapterType.GetField("_LTCGI_ScreenTransforms", flags);
            var renderersField = adapterType.GetField("_Renderers", flags);

            GameObject[] screens = screensField?.GetValue(adapter) as GameObject[];
            Vector4[] extraData = extraDataField?.GetValue(adapter) as Vector4[];
            Transform[] transforms = transformsField?.GetValue(adapter) as Transform[];
            Renderer[] renderers = renderersField?.GetValue(adapter) as Renderer[];

            // 1. Validate Screens (The Emitters)
            if (screens != null && extraData != null)
            {
                for (int i = 0; i < totalScreens; i++)
                {
                    if (i >= screens.Length || i >= extraData.Length) break;

                    GameObject screenObj = screens[i];
                    
                    // Check for nulls (destroyed objects) or objects that have been moved out of active scenes
                    if (screenObj == null || !screenObj.activeInHierarchy)
                    {
                        // Read the ExtraData. If w-component (flags) or color isn't zeroed out, we have a ghost light.
                        Vector4 data = extraData[i];
                        if (data.sqrMagnitude > 0.01f) // It's disabled in hierarchy but active in shader memory
                        {
                            report.StaleScreenCount++;
                            needsRebuild = true;
                            
                            // Immediate Mitigation: Zero out the data to kill the light in the shader immediately in editor
                            extraData[i] = Vector4.zero; 
                        }
                    }
                    else if (transforms != null && i < transforms.Length && transforms[i] != null)
                    {
                        // 4D Chess: Verify Transform bounds on STATIC screens.
                        // Dynamic screens update at runtime, but static screens bake their position. 
                        // If a static screen moved in the editor, its emission bounds are permanently desynced until a rebuild.
                        Transform t = transforms[i];
                        if (t.hasChanged)
                        {
                            if (i >= dynamicScreens) 
                            {
                                report.StaleScreenCount++;
                                needsRebuild = true;
                            }
                            t.hasChanged = false; 
                        }
                    }
                }
            }

            // 2. Validate Renderers (The Receivers)
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer r = renderers[i];
                    
                    // We only strictly flag NULL renderers (deleted objects). 
                    // Disabled renderers (!r.enabled) are fine as Udon logic might toggle them on during gameplay.
                    if (r == null) 
                    {
                        report.OrphanedRenderers++;
                        needsRebuild = true;
                    }
                }
            }

            report.RequiresRebuild = needsRebuild;
            return report;
        }

        // The Execution Block: Hooks the validation report into the UI/Auto-Fixer
        private void AuditLTCGIPipeline()
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
            
            Type adapterType = GetTypeSafe("LTCGI_UdonAdapter");
            Type controllerType = GetTypeSafe("pi.LTCGI.LTCGI_Controller");
            Type screenType = GetTypeSafe("pi.LTCGI.LTCGI_Screen");

            if (controllerType != null)
            {
                var controllers = GetCachedObjects(controllerType);
                foreach (var ctrl in controllers)
                {
                    var comp = (Component)ctrl;

                    // === 1. NRE DEADLOCK FIX (BAKE CACHE PURGE) ===
                    var bakeKeyField = controllerType.GetField("bakeMaterialReset_key", flags);
                    var bakeProgField = controllerType.GetField("bakeInProgress", flags);

                    if (bakeKeyField != null && bakeProgField != null)
                    {
                        bool isBaking = Convert.ToBoolean(bakeProgField.GetValue(ctrl));
                        object keys = bakeKeyField.GetValue(ctrl);

                        // If Unity serialization lost the list reference while bakeInProgress is stuck true
                        if (isBaking && (keys == null || keys.Equals(null)))
                        {
                            LogDiagnostic("LTCGI PIPELINE: FATAL DESYNC", "Bake Cache Deadlock (NRE)",
                                $"The LTCGI Controller '{comp.gameObject.name}' is stuck in a 'Bake In Progress' state, but its material cache is corrupted. Clicking 'Reset Settings' in the inspector will throw a MissingReferenceException. Click Fix to force-clear the deadlock.",
                                "#ff00aa", comp, () => {
                                    Undo.RecordObject(comp, "Nuke LTCGI Bake Cache");
                                    
                                    // Atomically reconstruct the lists in memory to satisfy the SerializedObject
                                    Type matListType = typeof(System.Collections.Generic.List<Material>);
                                    bakeKeyField.SetValue(ctrl, Activator.CreateInstance(matListType));
                                    
                                    var bakeValField = controllerType.GetField("bakeMaterialReset_val", flags);
                                    if (bakeValField != null)
                                    {
                                        Type enumListType = typeof(System.Collections.Generic.List<UnityEngine.MaterialGlobalIlluminationFlags>);
                                        bakeValField.SetValue(ctrl, Activator.CreateInstance(enumListType));
                                    }

                                    bakeProgField.SetValue(ctrl, false);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                                    UnityEngine.Debug.Log("[VixenWorldSpider] LTCGI Bake Deadlock Cleared. Controller reset successful.");
                                });
                        }
                    }

                    // === 2. VIDEO PLAYER AUTO-LINKING ===
                    var videoTexField = controllerType.GetField("VideoTexture", flags);
                    if (videoTexField != null && screenType != null)
                    {
                        Texture currentVideoTex = videoTexField.GetValue(ctrl) as Texture;
                        
                        // Check if dynamic screens actually exist
                        var screens = GetCachedObjects(screenType);
                        bool hasDynamicScreens = false;
                        foreach(var s in screens) 
                        {
                            var dynField = screenType.GetField("Dynamic", flags);
                            if (dynField != null && Convert.ToBoolean(dynField.GetValue(s))) {
                                hasDynamicScreens = true;
                                break;
                            }
                        }

                        if (hasDynamicScreens && currentVideoTex == null)
                        {
                            // Hunt for a valid Video Player CRT
                            Texture detectedVideoTex = null;
                            string detectedPlayer = "";

                            // Target A: ProTV
                            Type protvType = GetTypeSafe("ArchiTech.ProTV.TVManager");
                            if (protvType != null) {
                                var tvs = GetCachedObjects(protvType);
                                if (tvs.Length > 0) {
                                    var customTexField = protvType.GetField("customTexture", flags);
                                    detectedVideoTex = customTexField?.GetValue(tvs[0]) as Texture;
                                    detectedPlayer = "ProTV Custom Texture";
                                }
                            }

                            // Target B: TXL
                            if (detectedVideoTex == null) {
                                Type txlScreenMgrType = GetTypeSafe("Texel.ScreenManager");
                                if (txlScreenMgrType != null) {
                                    var sms = GetCachedObjects(txlScreenMgrType);
                                    if (sms.Length > 0) {
                                        var crtProp = new SerializedObject((Component)sms[0]).FindProperty("outputCRT");
                                        detectedVideoTex = crtProp?.objectReferenceValue as Texture;
                                        detectedPlayer = "TXL Output CRT";
                                    }
                                }
                            }

                            if (detectedVideoTex != null)
                            {
                                LogDiagnostic("LTCGI PIPELINE: INTEGRATION", "Missing Video Texture",
                                    $"'{comp.gameObject.name}' has dynamic screens but no VideoTexture assigned. Detected {detectedPlayer} ('{detectedVideoTex.name}'). Ready to auto-link.",
                                    "#00e5ff", comp, () => {
                                        Undo.RecordObject(comp, "Link Video Texture to LTCGI");
                                        videoTexField.SetValue(ctrl, detectedVideoTex);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                                    });
                            }
                            else
                            {
                                LogDiagnostic("LTCGI PIPELINE: INTEGRATION", "Missing Video Texture",
                                    $"'{comp.gameObject.name}' has dynamic screens but no VideoTexture assigned. You must manually assign your video player's output Render Texture for lighting to react.",
                                    "#ffaa00", comp);
                            }
                        }
                    }
                }

                // === 3. ARRAY FRAGMENTATION / GHOST SCREEN VALIDATION ===
                if (adapterType != null)
                {
                    foreach (var adapter in GetCachedObjects(adapterType))
                    {
                        var component = (Component)adapter;
                        LTCGIValidationReport report = CheckForStaleLTCGIData(component);

                        if (report.RequiresRebuild)
                        {
                            string issueDesc = "";
                            if (report.StaleScreenCount > 0) issueDesc += $"\n• {report.StaleScreenCount} Ghost/Desynced Screens";
                            if (report.OrphanedRenderers > 0) issueDesc += $"\n• {report.OrphanedRenderers} Destroyed/Orphaned Renderers";

                            LogDiagnostic("LTCGI PIPELINE: FRAGMENTATION", "Stale Data Rebuild Required",
                                $"Adapter '{component.gameObject.name}' has accumulated fragmented memory arrays.{issueDesc}\nThis causes ghost lighting and wastes GPU cycles.",
                                "#ff4444", component, () => {
                                    
                                    var singletonField = controllerType.GetField("Singleton", flags);
                                    var singleton = singletonField?.GetValue(null);
                                    
                                    if (singleton != null)
                                    {
                                        // 4D Chess: Try to invoke the parameterless UpdateMaterials(), if pi changed the signature, fallback to the bool override.
                                        var updateMethodParamless = controllerType.GetMethod("UpdateMaterials", new Type[0]);
                                        if (updateMethodParamless != null) {
                                            updateMethodParamless.Invoke(singleton, null);
                                        } else {
                                            var updateMethod = controllerType.GetMethod("UpdateMaterials", new Type[] { typeof(bool), screenType });
                                            updateMethod?.Invoke(singleton, new object[] { false, null });
                                        }
                                        UnityEngine.Debug.Log("[VixenWorldSpider] Forced native LTCGI Controller Rebuild.");
                                    }
                                });
                        }

                        // --- VIDEO TEXTURE BINDING GUARD ---
                        var blurCrtField = adapterType.GetField("BlurCRTInput", flags);
                        if (blurCrtField != null)
                        {
                            CustomRenderTexture blurCrt = blurCrtField.GetValue(adapter) as CustomRenderTexture;
                            if (blurCrt != null && blurCrt.material != null)
                            {
                                Texture mainTex = blurCrt.material.GetTexture("_MainTex");
                                if (mainTex == null)
                                {
                                    LogDiagnostic("LTCGI PIPELINE: TOPOLOGY", "Unbound Video Texture",
                                        $"Adapter '{component.gameObject.name}' has no VideoTexture bound to its Blur Chain. Dynamic video lighting will fail.",
                                        "#00e5ff", component, null); 
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AuditNativeVideoPipelines()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // PRE-AUDIT: Locate AudioLink Core for connectivity handshake
            Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");
            var alInstances = audioLinkType != null ? GetCachedObjects(audioLinkType, true) : null;
            Component alCore = (alInstances != null && alInstances.Length > 0) ? (Component)alInstances[0] : null;

            // === 1. AVPRO NATIVE PIPELINE ===
            Type avProType = GetTypeSafe("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer");
            if (avProType != null)
            {
                foreach (var player in GetCachedObjects(avProType))
                {
                    var component = (Component)player;
                    
                    // --- Resolution & Latency Guard ---
                    var maxResField = avProType.GetField("maximumResolution", flags);
                    if (maxResField != null)
                    {
                        int res = Convert.ToInt32(maxResField.GetValue(player));
                        if (res == 0 || res > 1080) 
                        {
                            string resStr = res == 0 ? "UNLIMITED (0)" : $"{res}p";
                            LogDiagnostic("VIDEO PIPELINE: BANDWIDTH NUKE", "Extreme AVPro Resolution",
                                $"'{component.gameObject.name}' has maximumResolution set to {resStr}. Forcing unconstrained or 4K streams will absolutely cripple instance bandwidth and crash Quest users. Throttle to 1080 or 720.",
                                "#ff00aa", component, () => {
                                    Undo.RecordObject(component, "Throttle AVPro Resolution");
                                    maxResField.SetValue(player, 1080);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }

                    var lowLatencyField = avProType.GetField("useLowLatency", flags);
                    if (lowLatencyField != null && Convert.ToBoolean(lowLatencyField.GetValue(player)))
                    {
                        LogDiagnostic("VIDEO PIPELINE: STABILITY", "Low Latency Enabled",
                            $"'{component.gameObject.name}' has 'Use Low Latency' enabled. This strips the video buffer and will cause severe stuttering for any player without a perfect internet connection. Disable for general media.",
                            "#ffaa00", component, () => {
                                Undo.RecordObject(component, "Disable Low Latency");
                                lowLatencyField.SetValue(player, false);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                    }

                    // --- AUDIOLINK TOPOLOGY HANDSHAKE ---
                    if (alCore != null)
                    {
                        var audioSourcesField = avProType.GetField("targetAudioSources", flags);
                        var sources = audioSourcesField?.GetValue(player) as AudioSource[];
                        var alSourceField = audioLinkType.GetField("audioSource", flags);
                        var currentAlSource = alSourceField?.GetValue(alCore) as AudioSource;

                        if (sources != null && sources.Length > 0)
                        {
                            bool linked = sources.Any(s => s != null && s == currentAlSource);
                            if (!linked)
                            {
                                LogDiagnostic("AUDIOLINK: TOPOLOGY", "AVPro Not Linked to AudioLink", 
                                    $"AVPro Player '{component.name}' outputs audio to '{sources[0].name}', but AudioLink is not listening to it. Reactive materials will not pulse.", 
                                    "#00e5ff", component, () => {
                                        Undo.RecordObject(alCore, "Link AVPro to AudioLink");
                                        alSourceField.SetValue(alCore, sources[0]);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(alCore);
                                    });
                            }
                        }
                    }
                }
            }

            // === 2. UNITY NATIVE PIPELINE ===
            Type unityVideoType = GetTypeSafe("VRC.SDK3.Video.Components.VRCUnityVideoPlayer");
            if (unityVideoType != null)
            {
                foreach (var player in GetCachedObjects(unityVideoType))
                {
                    var component = (Component)player;
                    
                    // --- Resolution Guard ---
                    var maxResField = unityVideoType.GetField("maximumResolution", flags);
                    if (maxResField != null)
                    {
                        int res = Convert.ToInt32(maxResField.GetValue(player));
                        if (res == 0 || res > 1080)
                        {
                            string resStr = res == 0 ? "UNLIMITED (0)" : $"{res}p";
                            LogDiagnostic("VIDEO PIPELINE: BANDWIDTH NUKE", "Extreme Unity Video Resolution",
                                $"'{component.gameObject.name}' has maximumResolution set to {resStr}. Forcing unconstrained or 4K streams will cripple instance bandwidth.",
                                "#ff00aa", component, () => {
                                    Undo.RecordObject(component, "Throttle Unity Video Resolution");
                                    maxResField.SetValue(player, 1080);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }

                    // --- AUDIOLINK TOPOLOGY HANDSHAKE ---
                    if (alCore != null)
                    {
                        var audioSourcesField = unityVideoType.GetField("targetAudioSources", flags);
                        var sources = audioSourcesField?.GetValue(player) as AudioSource[];
                        var alSourceField = audioLinkType.GetField("audioSource", flags);
                        var currentAlSource = alSourceField?.GetValue(alCore) as AudioSource;

                        if (sources != null && sources.Length > 0)
                        {
                            bool linked = sources.Any(s => s != null && s == currentAlSource);
                            if (!linked)
                            {
                                LogDiagnostic("AUDIOLINK: TOPOLOGY", "Unity Video Not Linked to AudioLink", 
                                    $"Unity Video Player '{component.name}' outputs audio to '{sources[0].name}', but AudioLink is not listening to it.", 
                                    "#00e5ff", component, () => {
                                        Undo.RecordObject(alCore, "Link Unity Video to AudioLink");
                                        alSourceField.SetValue(alCore, sources[0]);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(alCore);
                                    });
                            }
                        }
                    }
                }
            }
        }

        private void AuditTxlEcosystem()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // PRE-AUDIT: Locate AudioLink Core for connectivity handshake
            Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");
            var alInstances = audioLinkType != null ? GetCachedObjects(audioLinkType, true) : null;
            Component alCore = (alInstances != null && alInstances.Length > 0) ? (Component)alInstances[0] : null;

            // === 1. BASIC UDON HYGIENE ===
            var udonBehaviours = GetCachedObjects<UdonBehaviour>(true);
            foreach (var udon in udonBehaviours)
            {
                if (udon.programSource == null)
                {
                    LogDiagnostic("TXL ECOSYSTEM & UDON", "Orphaned UdonBehaviour", $"'{udon.gameObject.name}' has a dead Udon component with no program source. It will bloat serialization.", "#ff00aa", udon.gameObject, () => {
                        Undo.DestroyObjectImmediate(udon);
                    });
                }
            }

            // === 2. TEXEL UTILITY AUDITS ===
            Type debugUserListType = GetTypeSafe("Texel.DebugUserList");
            if (debugUserListType != null)
            {
                foreach (var dul in GetCachedObjects(debugUserListType))
                {
                    LogDiagnostic("TXL ECOSYSTEM & UDON", "Debug GC Sink Active", $"'{((Component)dul).gameObject.name}' contains a Texel DebugUserList. This allocates massive amounts of string garbage per frame on player updates. Disable before publishing.", "#ffaa00", (Component)dul);
                }
            }

            Type accessControlType = GetTypeSafe("Texel.AccessControl");
            if (accessControlType != null)
            {
                foreach (var acl in GetCachedObjects(accessControlType))
                {
                    var component = (Component)acl;
                    var whitelistField = accessControlType.GetField("userWhitelist", flags);
                    if (whitelistField != null)
                    {
                        var whitelist = whitelistField.GetValue(acl) as string[];
                        if (whitelist != null && whitelist.Length > 50)
                        {
                            LogDiagnostic("TXL ECOSYSTEM & UDON", "Inefficient Inline Whitelist", $"'{component.gameObject.name}' has an inline array of {whitelist.Length} users. Use a remote list or hashed whitelist instead.", "#00e5ff", component);
                        }
                    }
                }
            }

            // === 3. TXL PLAYER -> AUDIOLINK HANDSHAKE ===
            if (alCore != null)
            {
                Type syncPlayerType = GetTypeSafe("Texel.SyncPlayer") ?? GetTypeSafe("Texel.Video.SyncPlayer");
                Type txlPlayerType = GetTypeSafe("Texel.TXLVideoPlayer");
                
                var players = new List<Component>();
                if (syncPlayerType != null) players.AddRange(GetCachedObjects(syncPlayerType, true).Cast<Component>());
                if (txlPlayerType != null) players.AddRange(GetCachedObjects(txlPlayerType, true).Cast<Component>());

                foreach (var player in players)
                {
                    var sourcesField = player.GetType().GetField("audioSources", flags);
                    var sources = sourcesField?.GetValue(player) as AudioSource[];
                    var alSourceField = audioLinkType.GetField("audioSource", flags);
                    var currentAlSource = alSourceField?.GetValue(alCore) as AudioSource;

                    if (sources != null && sources.Length > 0)
                    {
                        if (!sources.Any(s => s != null && s == currentAlSource))
                        {
                            LogDiagnostic("AUDIOLINK: TOPOLOGY", "TXL Player Not Linked to AudioLink", 
                                $"TXL Video Player '{player.gameObject.name}' outputs audio to '{sources[0].name}', but AudioLink is not listening. Reactive materials will not pulse during video playback.", 
                                "#00e5ff", player.gameObject, () => {
                                    Undo.RecordObject(alCore, "Link TXL to AudioLink");
                                    alSourceField.SetValue(alCore, sources[0]);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(alCore);
                                });
                        }
                    }
                }
            }

            // === 4. TXL SCREEN MANAGER & CRT ECOSYSTEM ===
            Type screenManagerType = GetTypeSafe("Texel.ScreenManager");
            if (screenManagerType != null)
            {
                foreach (var sm in GetCachedObjects(screenManagerType, true))
                {
                    var smComp = (Component)sm;
                    SerializedObject smSO = new SerializedObject(smComp);
                    List<CustomRenderTexture> crtsToCheck = new List<CustomRenderTexture>();
                    
                    var legacyCrtProp = smSO.FindProperty("outputCRT");
                    if (legacyCrtProp != null && legacyCrtProp.objectReferenceValue != null)
                        crtsToCheck.Add(legacyCrtProp.objectReferenceValue as CustomRenderTexture);
                    
                    var crtArrayProp = smSO.FindProperty("renderOutCrt");
                    if (crtArrayProp != null && crtArrayProp.isArray)
                    {
                        for (int i = 0; i < crtArrayProp.arraySize; i++)
                        {
                            var crtElement = crtArrayProp.GetArrayElementAtIndex(i);
                            if (crtElement != null && crtElement.objectReferenceValue != null)
                            {
                                var crt = crtElement.objectReferenceValue as CustomRenderTexture;
                                if (!crtsToCheck.Contains(crt)) crtsToCheck.Add(crt);
                            }
                        }
                    }

                    foreach (var crt in crtsToCheck)
                    {
                        if (crt == null) continue;

                        if (crt.updateMode != CustomRenderTextureUpdateMode.Realtime)
                        {
                            LogDiagnostic("TXL RENDER ECOSYSTEM", "CRT Update Mode Not Realtime", 
                                $"CRT '{crt.name}' on '{smComp.gameObject.name}' is OnDemand. Force to Realtime to prevent frozen video frames in VRChat.", 
                                "#ffaa00", crt, () => {
                                    Undo.RecordObject(crt, "Fix CRT Update Mode");
                                    crt.updateMode = CustomRenderTextureUpdateMode.Realtime;
                                });
                        }
                        
                        if (!crt.doubleBuffered)
                        {
                            LogDiagnostic("TXL RENDER ECOSYSTEM", "CRT Screen Tearing Risk", 
                                $"CRT '{crt.name}' is not double buffered. This will cause visible flickering/tearing on screens.", 
                                "#ffaa00", crt, () => {
                                    Undo.RecordObject(crt, "Enable CRT Double Buffering");
                                    crt.doubleBuffered = true;
                                });
                        }
                    }
                }
            }

            // === 5. TXL QUEUE + ACCESS CONTROL ECOSYSTEM ===
            Type rinvoType = GetTypeSafe("Rinvo.YoutubeSearchManager");
            bool hasRinvo = rinvoType != null && GetCachedObjects(rinvoType, true).Length > 0;

            foreach (var comp in GetCachedObjects<UdonSharpBehaviour>(true))
            {
                if (comp != null && comp.GetType().Name.Contains("PlaylistQueue"))
                {
                    SerializedObject queueSO = new SerializedObject(comp);
                    SerializedProperty allowProxyProp = queueSO.FindProperty("allowAddFromProxy");
                    SerializedProperty interruptProp = queueSO.FindProperty("canInterruptSources");

                    if (interruptProp != null && !interruptProp.boolValue && hasRinvo)
                    {
                        LogDiagnostic("TXL QUEUE ECOSYSTEM", "Queue Auto-Play Deadlock", 
                            $"The Playlist Queue '{comp.gameObject.name}' has 'Can Interrupt Sources' disabled. This will block Rinvo instant-play requests.", 
                            "#00e5ff", comp, () => {
                                Undo.RecordObject(comp, "Enable Queue Source Interruption");
                                queueSO.Update();
                                interruptProp.boolValue = true;
                                queueSO.ApplyModifiedProperties();
                            });
                    }

                    if (allowProxyProp != null && hasRinvo && !allowProxyProp.boolValue)
                    {
                        LogDiagnostic("TXL QUEUE & ACCESS", "Proxy Queue Rejection", 
                            $"'{comp.gameObject.name}' has 'Allow Add From Proxy' disabled. This breaks the link with Rinvo search.", 
                            "#ff00aa", comp, () => {
                                Undo.RecordObject(comp, "Enable Proxy Adding");
                                queueSO.Update();
                                allowProxyProp.boolValue = true;
                                queueSO.ApplyModifiedProperties();
                            });
                    }
                }
            }
        }

        private void AuditProTVEcosystem()
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            Type proTvType = GetTypeSafe("ArchiTech.ProTV.TVManager");
            Type vpManagerType = GetTypeSafe("ArchiTech.ProTV.VPManager");
            Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");
            Type proTvAlAdapterType = GetTypeSafe("ArchiTech.ProTV.AudioLinkAdapter");

            // Locate AudioLink Core for connectivity handshake
            var alInstances = audioLinkType != null ? GetCachedObjects(audioLinkType, true) : null;
            Component alCore = (alInstances != null && alInstances.Length > 0) ? (Component)alInstances[0] : null;

            if (proTvType != null)
            {
                // === 1. PROTV TOPOLOGY & AUDIOLINK HANDSHAKE ===
                var tvs = GetCachedObjects(proTvType, true);
                
                if (tvs.Length > 0 && alCore != null)
                {
                    Component mainTv = (Component)tvs[0];
                    var adapters = proTvAlAdapterType != null ? GetCachedObjects(proTvAlAdapterType, true) : new UnityEngine.Object[0];

                    if (adapters.Length == 0)
                    {
                        // Fallback check: If no adapter, is AudioLink directly listening to ANY of the TV's speakers?
                        var alSourceField = audioLinkType.GetField("audioSource", flags);
                        var currentAlSource = alSourceField?.GetValue(alCore) as AudioSource;
                        bool isLinked = false;
                        AudioSource firstAvailableSpeaker = null;

                        if (vpManagerType != null)
                        {
                            foreach (var vpm in mainTv.GetComponentsInChildren(vpManagerType, true))
                            {
                                var speakersField = vpManagerType.GetField("speakers", flags);
                                var speakers = speakersField?.GetValue(vpm) as AudioSource[];
                                if (speakers != null && speakers.Length > 0)
                                {
                                    if (firstAvailableSpeaker == null) firstAvailableSpeaker = speakers[0];
                                    if (speakers.Contains(currentAlSource)) isLinked = true;
                                }
                            }
                        }

                        if (!isLinked && firstAvailableSpeaker != null)
                        {
                            LogDiagnostic("PROTV TOPOLOGY", "AudioLink Disconnected from TV", 
                                $"AudioLink is not listening to any of '{mainTv.gameObject.name}'s speakers. Reactive materials will not pulse. (Note: Using the official ProTV AudioLinkAdapter prefab is recommended for multi-player switching).", 
                                "#00e5ff", alCore, () => {
                                    Undo.RecordObject(alCore, "Link TV to AudioLink");
                                    alSourceField.SetValue(alCore, firstAvailableSpeaker);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(alCore);
                                });
                        }
                    }
                    else
                    {
                        // Verify ProTV AudioLinkAdapter bindings
                        foreach (var adapter in adapters)
                        {
                            var comp = (Component)adapter;
                            var tvField = proTvAlAdapterType.GetField("tv", flags);
                            var alField = proTvAlAdapterType.GetField("audioLink", flags);

                            var linkedTv = tvField?.GetValue(adapter) as Component;
                            var linkedAl = alField?.GetValue(adapter) as Component;

                            if (linkedTv == null)
                            {
                                LogDiagnostic("PROTV TOPOLOGY", "Adapter Missing TV", 
                                    $"ProTV AudioLink Adapter '{comp.gameObject.name}' is not linked to a TVManager. It will not receive hot-swap events.", 
                                    "#00e5ff", comp, () => {
                                        Undo.RecordObject(comp, "Link Adapter to TV");
                                        tvField.SetValue(adapter, mainTv);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                                    });
                            }

                            if (linkedAl == null)
                            {
                                LogDiagnostic("PROTV TOPOLOGY", "Adapter Missing AudioLink", 
                                    $"ProTV AudioLink Adapter '{comp.gameObject.name}' is not linked to the AudioLink Core.", 
                                    "#00e5ff", comp, () => {
                                        Undo.RecordObject(comp, "Link Adapter to AudioLink");
                                        alField.SetValue(adapter, alCore);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                                    });
                            }
                        }
                    }
                }

                // === 2. TV MANAGER CONFIGURATION ===
                int globalTextureCount = 0;
                foreach (var tv in tvs)
                {
                    var component = (Component)tv;
                    SerializedObject tvSO = new SerializedObject(component);

                    var enableHDRField = proTvType.GetField("enableHDR", flags);
                    if (enableHDRField != null && Convert.ToBoolean(enableHDRField.GetValue(tv)))
                    {
                        LogDiagnostic("PROTV VRAM: HDR BLOAT", "HDR Video Enabled", $"'{component.gameObject.name}' has HDR enabled. This forces ARGB64, doubling video texture VRAM footprint.", "#ff00aa", component, () => {
                            Undo.RecordObject(component, "Disable HDR on ProTV");
                            enableHDRField.SetValue(tv, false);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                    }

                    var bakeGlobalField = proTvType.GetField("bakeGlobalVideoTexture", flags);
                    if (bakeGlobalField != null && Convert.ToBoolean(bakeGlobalField.GetValue(tv)))
                    {
                        LogDiagnostic("PROTV VRAM: BAKED GSV", "Baked Global Texture", $"'{component.gameObject.name}' bakes the global texture. This adds an extra internal Blit pass and wastes GPU memory if not explicitly needed.", "#ffaa00", component, () => {
                            Undo.RecordObject(component, "Disable Baked Global Texture");
                            bakeGlobalField.SetValue(tv, false);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                    }

                    var preferAltField = proTvType.GetField("preferAlternateUrlForQuest", flags);
                    if (preferAltField != null && !Convert.ToBoolean(preferAltField.GetValue(tv)))
                    {
                        LogDiagnostic("PROTV COMPATIBILITY: QUEST FALLBACK", "Missing Quest Fallback", $"'{component.gameObject.name}' has 'Prefer Alternate URL for Quest' disabled. Android clients will try to resolve high-bitrate PC endpoints, often resulting in silent fail.", "#ffaa00", component, () => {
                            Undo.RecordObject(component, "Enable Quest Fallback");
                            preferAltField.SetValue(tv, true);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                    }

                    // Ensure aspect ratio is strictly managed via SerializedObject to survive recompiles
                    SerializedProperty aspectProp = tvSO.FindProperty("defaultAspectRatio");
                    if (aspectProp == null) aspectProp = tvSO.FindProperty("aspectRatio"); // Fallback for older versions

                    if (aspectProp != null)
                    {
                        float tvAspect = aspectProp.floatValue;
                        // 1.777777f is ProTV's exact internal default for 16:9
                        if (tvAspect <= 0f || Math.Abs(tvAspect - 1.777777f) > 0.05f && Math.Abs(tvAspect - 1.333333f) > 0.05f && Math.Abs(tvAspect - 2.333333f) > 0.05f)
                        {
                            LogDiagnostic("PROTV CONFIG: INVALID ASPECT", "Non-Standard Aspect Ratio", $"'{component.gameObject.name}' has its default aspect ratio set to {tvAspect:F3}. This breaks shader matrix bounds and UV calculations. Click Fix to force standard 16:9.", "#ff00aa", component, () => {
                                tvSO.Update();
                                aspectProp.floatValue = 1.777777f;
                                tvSO.ApplyModifiedProperties();
                            });
                        }
                    }

                    var enableGSVField = proTvType.GetField("enableGSV", flags);
                    if (enableGSVField != null && Convert.ToBoolean(enableGSVField.GetValue(tv))) globalTextureCount++;
                    
                    var videoManagersField = proTvType.GetField("videoManagers", flags);
                    if (videoManagersField != null)
                    {
                        var videoManagers = videoManagersField.GetValue(tv) as Array;
                        if (videoManagers == null || videoManagers.Length == 0)
                        {
                            LogDiagnostic("PROTV CRITICAL: MISSING MANAGERS", "Missing Video Managers", $"'{component.gameObject.name}' has no VPManagers assigned. The TV will crash on initialization.", "#ff00aa", component);
                        }
                    }

                    // Safely query the custom texture and correct sizing to 1920x1080
                    SerializedProperty customTexProp = tvSO.FindProperty("customTexture");
                    if (customTexProp != null && customTexProp.objectReferenceValue != null)
                    {
                        RenderTexture customTex = customTexProp.objectReferenceValue as RenderTexture;
                        if (customTex != null)
                        {
                            if (customTex.width != 1920 || customTex.height != 1080)
                            {
                                float mb = (customTex.width * customTex.height * 4) / 1048576f;
                                LogDiagnostic("PROTV VRAM: OPTIMIZATION", "Non-Standard Render Texture", 
                                    $"'{component.gameObject.name}' has a custom RenderTexture assigned of {customTex.width}x{customTex.height} (~{mb:F2} MB). ProTV operates optimally at exactly 1920x1080. Click fix to resize the asset.", 
                                    "#ffaa00", customTex, () => {
                                        Undo.RecordObject(customTex, "Resize Custom RenderTexture");
                                        customTex.Release(); // Flush GPU memory
                                        customTex.width = 1920;
                                        customTex.height = 1080;
                                        customTex.Create(); // Reallocate
                                        EditorUtility.SetDirty(customTex);
                                        AssetDatabase.SaveAssets();
                                    });
                            }
                        }
                    }
                }

                if (globalTextureCount > 1)
                {
                    LogDiagnostic("PROTV RENDER: GSV CONFLICT", "GSV Conflict", $"Found {globalTextureCount} TVs with Global Video Texture (GSV) enabled. Only one should be active to prevent global shader variable tearing.", "#ff00aa", null);
                }
            }

            // === 3. SUB-COMPONENTS & UI ===
            Type mediaControlsType = GetTypeSafe("ArchiTech.ProTV.MediaControls");
            if (mediaControlsType != null)
            {
                foreach (var controls in GetCachedObjects(mediaControlsType))
                {
                    var component = (Component)controls;
                    var realtimeSeekField = mediaControlsType.GetField("realtimeSeek", flags);
                    
                    if (realtimeSeekField != null && Convert.ToBoolean(realtimeSeekField.GetValue(controls)))
                    {
                        LogDiagnostic("PROTV COMPUTE: GC ALLOCATION SINK", "GC Allocation Sink", $"'{component.gameObject.name}' has Realtime Seek enabled. Updating TMP clock strings per-frame causes severe Garbage Collection spikes.", "#ffaa00", component, () => {
                            Undo.RecordObject(component, "Disable Realtime Seek");
                            realtimeSeekField.SetValue(controls, false);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                    }
                }
            }

            Type playlistDataType = GetTypeSafe("ArchiTech.ProTV.PlaylistData");
            if (playlistDataType != null)
            {
                foreach (var playlistData in GetCachedObjects(playlistDataType))
                {
                    var component = (Component)playlistData;
                    var imagesField = playlistDataType.GetField("images", flags);
                    
                    if (imagesField != null)
                    {
                        Sprite[] images = imagesField.GetValue(playlistData) as Sprite[];
                        if (images != null && images.Length > 20)
                        {
                            LogDiagnostic("PROTV VRAM: THUMBNAIL BLOAT", "Playlist Thumbnail Bloat", $"'{component.gameObject.name}' contains {images.Length} sprites serialized in the playlist. Ensure these are aggressively crunched, or it will bloat world VRAM.", "#ffaa00", component);
                        }
                    }
                }
            }

            Type playlistSearchType = GetTypeSafe("ArchiTech.ProTV.PlaylistSearch");
            if (playlistSearchType != null)
            {
                foreach (var search in GetCachedObjects(playlistSearchType))
                {
                    var component = (Component)search;
                    var aggroField = playlistSearchType.GetField("searchAggressionLevel", flags);
                    if (aggroField != null)
                    {
                        int aggro = Convert.ToInt32(aggroField.GetValue(search));
                        if (aggro > 10)
                        {
                            LogDiagnostic("PROTV COMPUTE: SEARCH BOTTLENECK", "Compute Bottleneck", $"'{component.gameObject.name}' has Search Aggression set to {aggro}. This high value will spike frame times on Quest/Low-End PCVR during playlist searches.", "#00e5ff", component, () => {
                                Undo.RecordObject(component, "Throttle Search Aggression");
                                aggroField.SetValue(search, 5);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                        }
                    }
                }
            }

            Type rtgiType = GetTypeSafe("ArchiTech.ProTV.RTGIUpdater");
            if (rtgiType != null)
            {
                foreach (var rtgi in GetCachedObjects(rtgiType))
                {
                    var component = (Component)rtgi;
                    var runOnMobileField = rtgiType.GetField("runOnMobile", flags);
                    if (runOnMobileField != null && Convert.ToBoolean(runOnMobileField.GetValue(rtgi)))
                    {
                        LogDiagnostic("PROTV COMPUTE: MOBILE RTGI SINK", "Mobile Compute Sink", $"'{component.gameObject.name}' is running RTGI updates on Mobile. Real-time GI updates in LateUpdate will absolutely nuke Quest frame rates.", "#ff00aa", component, () => {
                            Undo.RecordObject(component, "Disable Mobile RTGI");
                            runOnMobileField.SetValue(rtgi, false);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                    }
                }
            }

            Type queueType = GetTypeSafe("ArchiTech.ProTV.Queue");
            if (queueType != null)
            {
                foreach (var queue in GetCachedObjects(queueType))
                {
                    var component = (Component)queue;
                    var maxEntriesField = queueType.GetField("maxEntriesPerPlayer", flags);
                    var maxBurstField = queueType.GetField("maxBurstEntriesPerPlayer", flags);
                    
                    if (maxEntriesField != null && maxBurstField != null)
                    {
                        int maxEntries = Convert.ToInt32(maxEntriesField.GetValue(queue));
                        int maxBurst = Convert.ToInt32(maxBurstField.GetValue(queue));

                        if (maxEntries > 10 || maxBurst > 3)
                        {
                            LogDiagnostic("PROTV NETWORK: QUEUE SPAM RISK", "High Queue Burst/Limits", $"'{component.gameObject.name}' allows players to add {maxBurst} burst entries or {maxEntries} total entries. This enables bad actors to instantly fill the queue and trigger VRChat's Udon network rate limits.", "#ffaa00", component, () => {
                                Undo.RecordObject(component, "Throttle Queue Limits");
                                if (maxEntries > 10) maxEntriesField.SetValue(queue, 10);
                                if (maxBurst > 3) maxBurstField.SetValue(queue, 3);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                        }
                    }
                }
            }

            Type tvTogglesType = GetTypeSafe("ArchiTech.ProTV.TVToggles");
            if (tvTogglesType != null)
            {
                foreach (var toggles in GetCachedObjects(tvTogglesType))
                {
                    var component = (Component)toggles;
                    var superField = tvTogglesType.GetField("superGameObjects", flags);
                    var authField = tvTogglesType.GetField("authorizedGameObjects", flags);
                    var unauthField = tvTogglesType.GetField("unauthorizedGameObjects", flags);

                    int totalCount = 0;
                    if (superField != null && superField.GetValue(toggles) is GameObject[] s) totalCount += s.Length;
                    if (authField != null && authField.GetValue(toggles) is GameObject[] a) totalCount += a.Length;
                    if (unauthField != null && unauthField.GetValue(toggles) is GameObject[] u) totalCount += u.Length;

                    if (totalCount > 20)
                    {
                        LogDiagnostic("PROTV COMPUTE: MASSIVE TOGGLE ARRAY", "Massive Toggle Event", $"'{component.gameObject.name}' iterates over {totalCount} GameObjects on state changes. Toggling this many objects at once can cause a noticeable frame hitch. Consider grouping them under a single parent.", "#ffaa00", component);
                    }
                }
            }

            if (vpManagerType != null)
            {
                foreach (var vpm in GetCachedObjects(vpManagerType))
                {
                    var component = (Component)vpm;
                    
                    Component parentTv = null;
                    bool parentHasGSV = false;
                    if (proTvType != null)
                    {
                        parentTv = component.GetComponentInParent(proTvType);
                        if (parentTv != null)
                        {
                            var enableGSVField = proTvType.GetField("enableGSV", flags);
                            parentHasGSV = enableGSVField != null && Convert.ToBoolean(enableGSVField.GetValue(parentTv));
                        }
                    }

                    var speakersField = vpManagerType.GetField("speakers", flags);
                    if (speakersField != null)
                    {
                        var speakers = speakersField.GetValue(vpm) as AudioSource[];
                        if (speakers != null)
                        {
                            foreach (var speaker in speakers)
                            {
                                if (speaker == null) continue;
                                
                                if (speaker.spatialBlend > 0.8f && speaker.maxDistance > 100f)
                                {
                                    LogDiagnostic("PROTV AUDIO: SPATIALIZATION BLEED", "Excessive 3D Max Distance", 
                                        $"The speaker '{speaker.name}' on '{component.gameObject.name}' is set to 3D, but has a maxDistance of {speaker.maxDistance}m. This essentially forces it to behave as 2D audio that bleeds through walls, destroying occlusion logic.", 
                                        "#00e5ff", speaker);
                                }
                            }
                        }
                    }

                    var screensField = vpManagerType.GetField("screens", flags);
                    if (screensField != null)
                    {
                        var screens = screensField.GetValue(vpm) as GameObject[];
                        if (screens != null)
                        {
                            foreach (var scr in screens)
                            {
                                if (scr == null) continue;

                                float aspect = 0f;
                                string dimensions = "";
                                
                                RectTransform rect = scr.GetComponent<RectTransform>();
                                if (rect != null)
                                {
                                    if (rect.rect.height != 0) aspect = rect.rect.width / rect.rect.height;
                                    dimensions = $"{rect.rect.width}x{rect.rect.height} (UI)";
                                }
                                else
                                {
                                    Vector3 scale = scr.transform.lossyScale; 
                                    if (scale.y != 0) aspect = Math.Abs(scale.x / scale.y);
                                    dimensions = $"Scale: {scale.x:F2}x{scale.y:F2} (Mesh)";
                                }

                                if (aspect > 0)
                                {
                                    if (Math.Abs(aspect - 1.0f) < 0.05f)
                                    {
                                        LogDiagnostic("PROTV DESIGN: UNCALIBRATED SCREEN MESH", "1:1 Screen Mesh Scale", 
                                            $"The screen '{scr.name}' assigned to '{component.gameObject.name}' has a 1:1 physical aspect ratio [{dimensions}]. Unless you are explicitly relying on the shader's aspect-correction (which wastes fragment operations), scale it to a standard ratio like 16:9 (e.g., X: 1.6, Y: 0.9).", 
                                            "#00e5ff", scr);
                                    }
                                }

                                var rend = scr.GetComponent<Renderer>();
                                if (rend != null)
                                {
                                    foreach (var mat in rend.sharedMaterials)
                                    {
                                        if (mat == null) continue;

                                        if (parentHasGSV && mat.HasProperty("_UseGlobalTexture") && !mat.IsKeywordEnabled("_USEGLOBALTEXTURE"))
                                        {
                                            LogDiagnostic("PROTV SHADER: GSV DESYNC", "Missing Global Texture Keyword", 
                                                $"The TV '{parentTv.name}' has Global Video Texture (GSV) enabled, but the screen material on '{scr.name}' is missing the _USEGLOBALTEXTURE keyword. The TV is running an expensive blit pass that this material ignores.", 
                                                "#ff00aa", scr, () => {
                                                    Undo.RecordObject(mat, "Enable GSV Keyword");
                                                    mat.SetFloat("_UseGlobalTexture", 1f);
                                                    mat.EnableKeyword("_USEGLOBALTEXTURE");
                                                    EditorUtility.SetDirty(mat);
                                                });
                                        }

                                        if (mat.globalIlluminationFlags == MaterialGlobalIlluminationFlags.RealtimeEmissive)
                                        {
                                            LogDiagnostic("PROTV RENDER: REALTIME GI SINK", "Realtime GI on Screen", 
                                                $"The screen '{scr.name}' assigned to '{component.gameObject.name}' has Realtime Emissive enabled. This forces Unity to recalculate the entire room's Global Illumination every frame the video plays.", 
                                                "#ff00aa", scr, () => {
                                                    Undo.RecordObject(mat, "Disable Realtime GI");
                                                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                                                    EditorUtility.SetDirty(mat);
                                                });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Type queueUiType = GetTypeSafe("ArchiTech.ProTV.QueueUI");
            Type historyUiType = GetTypeSafe("ArchiTech.ProTV.HistoryUI");
            Type playlistUiType = GetTypeSafe("ArchiTech.ProTV.PlaylistUI"); 

            var complexUis = new List<Component>();
            if (queueUiType != null) complexUis.AddRange(GetCachedObjects(queueUiType).Cast<Component>());
            if (historyUiType != null) complexUis.AddRange(GetCachedObjects(historyUiType).Cast<Component>());
            if (playlistUiType != null) complexUis.AddRange(GetCachedObjects(playlistUiType).Cast<Component>());

            foreach (var uiComp in complexUis)
            {
                Canvas parentCanvas = uiComp.GetComponentInParent<Canvas>();
                if (parentCanvas != null && parentCanvas.isRootCanvas)
                {
                    LogDiagnostic("PROTV UI: CANVAS REBUILD CASCADE", "Canvas Rebuild Cascade", $"'{uiComp.gameObject.name}' modifies layout elements on a Root Canvas. This forces a full rebuild of the entire Canvas every time an item changes. Nest it inside a sub-canvas.", "#ffaa00", parentCanvas.gameObject);
                }

                var casters = uiComp.GetComponentsInChildren<UnityEngine.UI.GraphicRaycaster>(true);
                if (casters.Length > 1) 
                {
                    LogDiagnostic("PROTV UI: RAYCASTER BLOAT", "Nested GraphicRaycasters", 
                        $"'{uiComp.gameObject.name}' contains {casters.Length} GraphicRaycaster components. VRChat evaluates every raycaster in the hierarchy against the VRCUiShape per-frame. Remove redundant raycasters from nested elements to recover CPU overhead.", 
                        "#ffaa00", uiComp.gameObject);
                }
            }

            // === 4. UMBRELLA & EXTRAS ===
            Type atToggleType = GetTypeSafe("ArchiTech.Umbrella.ATToggle");
            if (atToggleType != null)
            {
                foreach (var toggle in GetCachedObjects(atToggleType))
                {
                    var component = (Component)toggle;
                    var actionsField = atToggleType.BaseType.GetField("actions", flags); 
                    if (actionsField != null && actionsField.GetValue(toggle) is int[] actions && actions.Length > 15)
                    {
                        LogDiagnostic("UMBRELLA COMPUTE: MASSIVE TOGGLE EVENT", "Massive Toggle Event", $"'{component.gameObject.name}' iterates over {actions.Length} actions on state change. Toggling this many objects simultaneously will cause a noticeable frame hitch.", "#ffaa00", component);
                    }
                }
            }

            Type zoneTriggerType = GetTypeSafe("ArchiTech.Umbrella.ZoneTrigger");
            if (zoneTriggerType != null)
            {
                foreach (var trigger in GetCachedObjects(zoneTriggerType))
                {
                    var component = (Component)trigger;
                    var typeField = zoneTriggerType.GetField("triggerType", flags);
                    if (typeField != null && Convert.ToInt32(typeField.GetValue(trigger)) == 2) 
                    {
                        if (component.GetComponents<Collider>().Length == 0)
                        {
                            LogDiagnostic("UMBRELLA LOGIC: MISSING COLLIDER", "ZoneTrigger Missing Collider", $"'{component.gameObject.name}' is set to use a Collider for its trigger area, but no Collider component is attached to the GameObject.", "#ff00aa", component);
                        }
                    }
                }
            }

            Type actionProxyType = GetTypeSafe("ArchiTech.Umbrella.ColliderActionProxy");
            if (actionProxyType != null)
            {
                foreach (var proxy in GetCachedObjects(actionProxyType))
                {
                    var component = (Component)proxy;
                    var targetField = actionProxyType.GetField("eventTarget", flags);
                    if (targetField != null && targetField.GetValue(proxy) == null)
                    {
                        LogDiagnostic("UMBRELLA LOGIC: DEAD PROXY", "Dead ColliderActionProxy", $"'{component.gameObject.name}' is missing an Event Target (UdonBehaviour). Interactions and Collisions will fail silently.", "#ff00aa", component);
                    }
                }
            }

            Type proxyType = GetTypeSafe("ArchiTech.ProTV.Extras.UIToAnimatorProxy");
            if (proxyType != null)
            {
                foreach (var proxy in GetCachedObjects(proxyType))
                {
                    var component = (Component)proxy;
                    var animatorsField = proxyType.GetField("animators", flags);
                    var parametersField = proxyType.GetField("parameters", flags);
                    
                    if (animatorsField != null && parametersField != null)
                    {
                        var animators = animatorsField.GetValue(proxy) as Animator[];
                        var parameters = parametersField.GetValue(proxy) as string[];

                        if (animators != null && parameters != null)
                        {
                            for (int i = 0; i < animators.Length; i++)
                            {
                                if (animators[i] != null && (i >= parameters.Length || string.IsNullOrWhiteSpace(parameters[i])))
                                {
                                    LogDiagnostic("EXTRAS LOGIC: UNMAPPED PROXY", "Unmapped Animator Proxy", $"'{component.gameObject.name}' has Animator '{animators[i].name}' assigned but is missing the target Parameter string at index {i}.", "#ffaa00", component);
                                }
                            }
                        }
                    }
                }
            }

            int forcedError = SessionState.GetInt("FORCE-VIDEO-ERROR", -1); 
            if (forcedError != -1)
            {
                LogDiagnostic("SHIM CONFIG: FORCED ERROR ACTIVE", "Forced Video Error Active", 
                    $"The PlayMode URL Resolver is currently configured to simulate a VideoError ({(VRC.SDK3.Components.Video.VideoError)forcedError}). Video playback in Editor will artificially fail until cleared.", 
                    "#ffaa00", null, () => {
                        SessionState.SetInt("FORCE-VIDEO-ERROR", -1);
                    });
            }

            Type avproType = GetTypeSafe("RenderHeads.Media.AVProVideo.MediaPlayer");
            if (avproType != null)
            {
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                
                if (!defines.Contains("AVPRO_IMPORTED"))
                {
                    LogDiagnostic("SHIM CONFIG: AVPRO DEFINE MISSING", "AVPro Define Missing", 
                        "AVPro is installed in the project, but the 'AVPRO_IMPORTED' scripting define is missing from Player Settings. Editor playmode AVPro simulation will not function correctly.", 
                        "#ff00aa", null, () => {
                            string newDefines = string.IsNullOrWhiteSpace(defines) ? "AVPRO_IMPORTED" : defines + ";AVPRO_IMPORTED";
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
                        });
                }
            }
        }

        private void AuditIwaSyncEcosystem()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // PRE-AUDIT: Locate AudioLink Core for connectivity handshake
            Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");
            var alInstances = audioLinkType != null ? GetCachedObjects(audioLinkType, true) : null;
            Component alCore = (alInstances != null && alInstances.Length > 0) ? (Component)alInstances[0] : null;

            // === 1. CORE & RESOLUTION ===
            Type iwaType = GetTypeSafe("HoshinoLabs.IwaSync3.IwaSync3");
            if (iwaType != null)
            {
                foreach (var iwa in GetCachedObjects(iwaType))
                {
                    var component = (Component)iwa;
                    var maxResField = iwaType.GetField("maximumResolution", flags);
                    if (maxResField != null)
                    {
                        int res = Convert.ToInt32(maxResField.GetValue(iwa));
                        if (res > 720)
                        {
                            LogDiagnostic("IWASYNC3 ECOSYSTEM", "High Default Resolution", 
                                $"'{component.gameObject.name}' defaults to {res}p. Forcing high resolutions can cripple instance bandwidth and Quest frame rates.", 
                                "#ffaa00", component, () => {
                                    Undo.RecordObject(component, "Throttle IwaSync3 Resolution");
                                    maxResField.SetValue(iwa, 720);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }
                }
            }

            // === 2. NETWORK QUEUE (PLAYLISTS) ===
            Type playlistType = GetTypeSafe("HoshinoLabs.IwaSync3.Playlist");
            if (playlistType != null)
            {
                foreach (var pl in GetCachedObjects(playlistType))
                {
                    var component = (Component)pl;
                    var limitField = playlistType.GetField("playlistLimitCount", flags);
                    if (limitField != null)
                    {
                        int limit = Convert.ToInt32(limitField.GetValue(pl));
                        if (limit <= 0 || limit > 50)
                        {
                            LogDiagnostic("IWASYNC3 ECOSYSTEM", "Unbounded Playlist Fetch", 
                                $"'{component.gameObject.name}' has no strict playlist fetch limit ({limit}). Massive YouTube playlists will choke Udon's network queue on load.", 
                                "#ff00aa", component, () => {
                                    Undo.RecordObject(component, "Set Safe Playlist Limit");
                                    limitField.SetValue(pl, 50);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }
                }
            }

            // === 3. AUDIO TOPOLOGY & AUDIOLINK HANDSHAKE ===
            Type speakerType = GetTypeSafe("HoshinoLabs.IwaSync3.Speaker");
            if (speakerType != null)
            {
                var speakers = GetCachedObjects(speakerType);
                bool isAudioLinkConnected = false;
                AudioSource firstValidSpeakerSource = null;

                foreach (var spk in speakers)
                {
                    var component = (Component)spk;
                    
                    // Spatialization Check
                    var spatializeField = speakerType.GetField("spatialize", flags);
                    if (spatializeField != null)
                    {
                        bool spatialize = Convert.ToBoolean(spatializeField.GetValue(spk));
                        if (!spatialize)
                        {
                            LogDiagnostic("IWASYNC3 ECOSYSTEM", "Global 2D Speaker", 
                                $"'{component.gameObject.name}' has spatialization disabled. This forces 2D global audio, which can cause voice starvation if not strictly intended for BGM.", 
                                "#ffaa00", component, () => {
                                    Undo.RecordObject(component, "Enable Speaker Spatialization");
                                    spatializeField.SetValue(spk, true);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }

                    // Extract AudioSource for Handshake
                    AudioSource speakerSource = component.GetComponent<AudioSource>();
                    if (speakerSource != null)
                    {
                        if (firstValidSpeakerSource == null) firstValidSpeakerSource = speakerSource;

                        if (alCore != null)
                        {
                            var alSourceField = audioLinkType.GetField("audioSource", flags);
                            var currentAlSource = alSourceField?.GetValue(alCore) as AudioSource;
                            if (currentAlSource == speakerSource) isAudioLinkConnected = true;
                        }
                    }
                }

                // Execute AudioLink Handshake
                if (alCore != null && speakers.Length > 0 && !isAudioLinkConnected && firstValidSpeakerSource != null)
                {
                    LogDiagnostic("AUDIOLINK: TOPOLOGY", "IwaSync3 Not Linked to AudioLink", 
                        $"AudioLink is not listening to any of IwaSync3's Speakers. Reactive materials will not pulse during video playback.", 
                        "#00e5ff", alCore, () => {
                            var alSourceField = audioLinkType.GetField("audioSource", flags);
                            Undo.RecordObject(alCore, "Link IwaSync3 to AudioLink");
                            alSourceField.SetValue(alCore, firstValidSpeakerSource);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(alCore);
                        });
                }
            }

            // === 4. SCREEN SHADERS & RENDER TARGETS ===
            Type screenType = GetTypeSafe("HoshinoLabs.IwaSync3.Screen");
            if (screenType != null)
            {
                foreach (var scr in GetCachedObjects(screenType))
                {
                    var component = (Component)scr;
                    var matIndexField = screenType.GetField("materialIndex", flags);
                    var screenRendererField = screenType.GetField("screen", flags); 
                    
                    if (screenRendererField != null && matIndexField != null)
                    {
                        var renderer = screenRendererField.GetValue(scr) as Renderer; 
                        if (renderer != null)
                        {
                            int idx = Convert.ToInt32(matIndexField.GetValue(scr));
                            if (idx >= 0 && idx < renderer.sharedMaterials.Length)
                            {
                                Material targetMat = renderer.sharedMaterials[idx];
                                if (targetMat != null && targetMat.globalIlluminationFlags == MaterialGlobalIlluminationFlags.RealtimeEmissive)
                                {
                                    LogDiagnostic("IWASYNC3 ECOSYSTEM", "Realtime GI Compute Sink", 
                                        $"'{component.gameObject.name}' drives a screen material set to Realtime Emissive. This forces Unity to recalculate Global Illumination every frame the video plays.", 
                                        "#ff00aa", component, () => {
                                            Undo.RecordObject(targetMat, "Disable Realtime GI on Screen");
                                            targetMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                                            EditorUtility.SetDirty(targetMat);
                                        });
                                }
                            }
                        }
                    }
                }
            }

            // === 5. CORE UDON LOGIC & INSTANTIATION TIMING ===
            Type videoCoreType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoCore");
            if (videoCoreType != null)
            {
                foreach (var core in GetCachedObjects(videoCoreType))
                {
                    var component = (Component)core;
                    var syncFreqField = videoCoreType.GetField("syncFrequency", flags);
                    if (syncFreqField != null)
                    {
                        float freq = Convert.ToSingle(syncFreqField.GetValue(core));
                        if (freq < 5.0f)
                        {
                            LogDiagnostic("IWASYNC3 ECOSYSTEM", "Aggressive Video Sync",
                                $"'{component.gameObject.name}' has a sync frequency of {freq}s. Syncing video state this rapidly consumes severe network bandwidth and causes player IK to lag.",
                                "#ffaa00", component, () => {
                                    Undo.RecordObject(component, "Throttle Sync Frequency");
                                    syncFreqField.SetValue(core, 9.2f); 
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }
                }
            }

            Type udonScreenType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoScreen");
            if (udonScreenType != null)
            {
                foreach (var scr in GetCachedObjects(udonScreenType))
                {
                    var component = (Component)scr;
                    var emissiveBoostField = udonScreenType.GetField("defaultEmissiveBoost", flags);
                    if (emissiveBoostField != null)
                    {
                        float boost = Convert.ToSingle(emissiveBoostField.GetValue(scr));
                        if (boost > 1.5f)
                        {
                            LogDiagnostic("IWASYNC3 ECOSYSTEM", "Blinding Emissive Boost",
                                $"'{component.gameObject.name}' has an emissive boost of {boost}. Values above 1.5 usually cause severe post-processing blowout, blinding VR users in dark worlds.",
                                "#ff00aa", component, () => {
                                    Undo.RecordObject(component, "Normalize Emissive Boost");
                                    emissiveBoostField.SetValue(scr, 1.0f);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }
                }
            }

            Type eventInvokerType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.CustomEventInvoker");
            if (eventInvokerType != null)
            {
                foreach (var invoker in GetCachedObjects(eventInvokerType))
                {
                    var component = (Component)invoker;
                    LogDiagnostic("IWASYNC3 ECOSYSTEM", "Runtime Instantiation Risk",
                        $"'{component.gameObject.name}' contains a CustomEventInvoker. This script uses Instantiate() at runtime to process delayed events. Rapidly triggering UI elements connected to this will cause severe Garbage Collection spikes and frame stutters.",
                        "#ffaa00", component);
                }
            }
        }

        private void AuditVizVidEcosystem()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    
            // Soft-dependency resolution
            Type vvmwCoreType = GetTypeSafe("JLChnToZ.VRC.VVMW.Core");
            Type vvmwRateLimitType = GetTypeSafe("JLChnToZ.VRC.VVMW.RateLimitResolver");
            Type vvmwFrontendType = GetTypeSafe("JLChnToZ.VRC.VVMW.FrontendHandler");
            Type vvmwUiHandlerType = GetTypeSafe("JLChnToZ.VRC.VVMW.UIHandler");
            Type vvmwVideoPlayerHandlerType = GetTypeSafe("JLChnToZ.VRC.VVMW.VideoPlayerHandler");
            Type vvmwGlobalSettingsType = GetTypeSafe("JLChnToZ.VRC.VVMW.Designer.GlobalSettings");
            Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");

            var alInstances = audioLinkType != null ? GetCachedObjects(audioLinkType, true) : null;
            Component alCore = (alInstances != null && alInstances.Length > 0) ? (Component)alInstances[0] : null;

            if (vvmwCoreType != null)
            {
                // 1. Singleton Enforcement: Global Settings
                if (vvmwGlobalSettingsType != null)
                {
                    var globalSettings = GetCachedObjects(vvmwGlobalSettingsType, true);
                    if (globalSettings.Length > 1)
                    {
                        LogDiagnostic("VIZVID ECOSYSTEM", "Singleton Violation: Global Settings", 
                            $"Matrix detected {globalSettings.Length} GlobalSettings instances. VVMW architecture strictly dictates a single global settings module. Multiple instances will trigger race conditions and initialization failures.", 
                            "#ff00aa", (Component)globalSettings[1]);
                    }
                }

                var cores = GetCachedObjects(vvmwCoreType, true);
                foreach (var core in cores)
                {
                    var component = (Component)core;

                    // 2. Audit Player Handlers & Cross-Platform Fallbacks
                    var handlersField = vvmwCoreType.GetField("playerHandlers", flags);
                    if (handlersField != null)
                    {
                        var handlers = handlersField.GetValue(core) as Component[];
                        if (handlers == null || handlers.Length == 0)
                        {
                            LogDiagnostic("VIZVID ECOSYSTEM", "Disconnected Player Handlers", 
                                $"'{component.gameObject.name}' has no registered Player Handlers. The video backend is orphaned and will silently fail to load media.", 
                                "#ff00aa", component);
                        }
                        else if (vvmwVideoPlayerHandlerType != null)
                        {
                            // Scan for Android/Quest compatibility gaps
                            foreach(var handler in handlers)
                            {
                                if (handler != null && handler.GetType() == vvmwVideoPlayerHandlerType)
                                {
                                    var isAvProField = vvmwVideoPlayerHandlerType.GetField("isAvPro", flags);
                                    var fallbackField = vvmwVideoPlayerHandlerType.GetField("fallbackHandler", flags);
                            
                                    bool isAvPro = isAvProField != null && (bool)isAvProField.GetValue(handler);
                                    var fallback = fallbackField != null ? fallbackField.GetValue(handler) as Component : null;

                                    if (isAvPro && fallback == null)
                                    {
                                        LogDiagnostic("VIZVID TOPOLOGY: CROSS-PLATFORM", "Missing Quest Fallback (AVPro)", 
                                            $"Video Handler '{handler.gameObject.name}' is an AVPro player but lacks a Unity Video Fallback Handler. Android/Quest clients cannot natively process AVPro and will be locked out of the stream.", 
                                            "#00e5ff", handler);
                                    }
                                }
                            }
                        }
                    }

                    // 3. Audio Spatialization
                    var audioSourcesField = vvmwCoreType.GetField("audioSources", flags);
                    if (audioSourcesField != null)
                    {
                        var audioSources = audioSourcesField.GetValue(core) as AudioSource[];
                        if (audioSources != null)
                        {
                            foreach (var src in audioSources)
                            {
                                if (src != null && src.spatialBlend < 1f)
                                {
                                    LogDiagnostic("VIZVID AUDIO: SPATIALIZATION", "2D Audio Bleed Risk", 
                                        $"AudioSource '{src.gameObject.name}' linked to VVMW is not fully 3D spatialized (Blend: {src.spatialBlend}). This will broadcast instance-wide unless specifically intended.", 
                                        "#00e5ff", src, () => {
                                            Undo.RecordObject(src, "Force 3D Spatialization");
                                            src.spatialBlend = 1f;
                                            PrefabUtility.RecordPrefabInstancePropertyModifications(src);
                                        });
                                }
                            }
                        }
                    }

                    // 4. AUDIOLINK TOPOLOGY HANDSHAKE
                    var alRefField = vvmwCoreType.GetField("audioLink", flags);
                    if (alRefField != null)
                    {
                        var linkedAl = alRefField.GetValue(core);
                        if (linkedAl == null && alCore != null)
                        {
                            LogDiagnostic("VIZVID: TOPOLOGY", "AudioLink Not Linked", 
                                $"VizVid Core '{component.gameObject.name}' is not linked to the AudioLink prefab. VizVid cannot automatically sync media states (Play/Pause) or pipe audio into the shaders.", 
                                "#00e5ff", component, () => {
                                    Undo.RecordObject(component, "Link VizVid to AudioLink");
                                    alRefField.SetValue(core, alCore);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }

                    // 5. Material Color Space & Shader Compatibility
                    var screenTargetsField = vvmwCoreType.GetField("screenTargets", flags);
                    if (screenTargetsField != null)
                    {
                        var screenTargets = screenTargetsField.GetValue(core) as UnityEngine.Object[];
                        if (screenTargets != null)
                        {
                            foreach (var target in screenTargets)
                            {
                                if (target is Renderer rend)
                                {
                                    foreach (var mat in rend.sharedMaterials)
                                    {
                                        if (mat != null && !mat.shader.name.StartsWith("JLChnToZ/Video") && !_validShaderList.Contains(mat.shader.name))
                                        {
                                            LogDiagnostic("VIZVID RENDER PIPELINE", "Non-Whitelisted Target Shader", 
                                                $"VVMW Screen '{rend.name}' uses '{mat.shader.name}'. Video color spaces (Gamma to Linear) or inverted UVs from AVPro may render incorrectly unless the shader actively supports '_IsAVProVideo'.", 
                                                "#ffaa00", rend);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 6. Rate Limiter Validation
            if (vvmwRateLimitType != null)
            {
                var resolvers = GetCachedObjects(vvmwRateLimitType, true);
                if (resolvers.Length == 0 && vvmwCoreType != null && GetCachedObjects(vvmwCoreType, true).Length > 0)
                {
                    LogDiagnostic("VIZVID NETWORK TOPOLOGY", "Missing Rate Limit Resolver", 
                        $"The scene utilizes VizVid but lacks a RateLimitResolver. Rapid video switching requests from late-joiners may trigger VRChat API rate limits, causing instance desyncs.", 
                        "#ff00aa", null);
                }
            }

            // 7. Interface Decoupling Checks (Orphaned UI/Frontends)
            if (vvmwFrontendType != null)
            {
                var frontends = GetCachedObjects(vvmwFrontendType, true);
                foreach(var frontend in frontends)
                {
                    var coreField = vvmwFrontendType.GetField("core", flags);
                    if (coreField != null)
                    {
                        var linkedCore = coreField.GetValue(frontend) as Component;
                        if (linkedCore == null)
                        {
                            LogDiagnostic("VIZVID INTERFACE", "Orphaned Frontend Handler", 
                                $"FrontendHandler '{((Component)frontend).gameObject.name}' is decoupled. It has no linked VizVid Core and will fail to execute logic.", 
                                "#ff00aa", (Component)frontend);
                        }
                    }
                }
            }
    
            if (vvmwUiHandlerType != null)
            {
                var uiHandlers = GetCachedObjects(vvmwUiHandlerType, true);
                foreach(var ui in uiHandlers)
                {
                    var coreField = vvmwUiHandlerType.GetField("core", flags);
                    if (coreField != null)
                    {
                        var linkedCore = coreField.GetValue(ui) as Component;
                        if (linkedCore == null)
                        {
                            LogDiagnostic("VIZVID INTERFACE", "Orphaned UI Handler", 
                                $"UIHandler '{((Component)ui).gameObject.name}' is decoupled. It has no linked VizVid Core, meaning all local UI inputs will hit a dead end.", 
                                "#ff00aa", (Component)ui);
                        }
                    }
                }
            }
        }

        private void AuditAudioLinkEcosystem()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            
            // Core Type Definitions
            Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");
            Type reactiveType = GetTypeSafe("AudioLink.AudioReactive");
            Type vvmwCoreType = GetTypeSafe("JLChnToZ.VRC.VVMW.Core");
            Type proTvType = GetTypeSafe("ArchiTech.ProTV.TVManager");

            // 1. DATA VORTEX: FIND THE CORE
            var alInstances = audioLinkType != null ? GetCachedObjects(audioLinkType, true) : null;
            Component alCore = (alInstances != null && alInstances.Length > 0) ? (Component)alInstances[0] : null;
            bool coreExists = alCore != null;

            if (coreExists)
            {
                if (alInstances.Length > 1)
                {
                    LogDiagnostic("AUDIOLINK: TOPOLOGY", "Multiple Cores Detected", 
                        "Found more than one AudioLink core. This causes global shader keyword collisions and doubles DFT compute cost.", 
                        "#ff00aa", (Component)alInstances[1]);
                }

                // --- PIPELINE SYNC: VIDEO PLAYER -> AUDIOLINK ---
                var sourceField = audioLinkType.GetField("audioSource", flags);
                AudioSource currentAlSource = sourceField?.GetValue(alCore) as AudioSource;
                AudioSource detectedMasterSource = null;
                string sourceSystem = "None";

                // Scan for VizVid (VVMW) Master Source
                if (vvmwCoreType != null)
                {
                    var vvmw = FindObjectOfType(vvmwCoreType, true);
                    if (vvmw != null)
                    {
                        var vvmwSources = vvmwCoreType.GetField("audioSources", flags)?.GetValue(vvmw) as AudioSource[];
                        if (vvmwSources != null && vvmwSources.Length > 0) { detectedMasterSource = vvmwSources[0]; sourceSystem = "VizVid (VVMW)"; }
                    }
                }

                // Scan for ProTV Master Source (Fallback)
                if (detectedMasterSource == null && proTvType != null)
                {
                    // FIX: Explicitly cast the returned UnityEngine.Object to a Component
                    var tv = FindObjectOfType(proTvType, true) as Component;
                    if (tv != null)
                    {
                        // ProTV stores speakers in VPManagers, but often has a main audio source
                        detectedMasterSource = tv.GetComponentInChildren<AudioSource>();
                        sourceSystem = "ProTV";
                    }
                }

                if (detectedMasterSource != null && currentAlSource != detectedMasterSource)
                {
                    LogDiagnostic("AUDIOLINK: PIPELINE", "Desynced Audio Input", 
                        $"AudioLink is listening to '{(currentAlSource != null ? currentAlSource.name : "Nothing")}', but your {sourceSystem} master audio is '{detectedMasterSource.name}'. Click Fix to pipe the audio correctly.", 
                        "#00e5ff", alCore, () => {
                            Undo.RecordObject(alCore, "Link Audio Source");
                            sourceField.SetValue(alCore, detectedMasterSource);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(alCore);
                        });
                }

                // --- PERFORMANCE: QUEST READBACK CHECK ---
                var readbackField = audioLinkType.GetField("audioDataToggle", flags);
                if (readbackField != null && (bool)readbackField.GetValue(alCore))
                {
                    LogDiagnostic("AUDIOLINK: PERFORMANCE", "Quest GPU Stall (Readback Enabled)", 
                        "GPU Data Readback is ENABLED. This causes a sync-point stall on Android/Quest. Disable this if you only use AudioLink for shaders to gain ~5-10 FPS on mobile.", 
                        "#ffaa00", alCore);
                }
            }
            else
            {
                LogDiagnostic("AUDIOLINK: TOPOLOGY", "System Missing", 
                    "No AudioLink Core found. All sound-reactive materials and stage lighting will remain static.", 
                    "#ffaa00", null);
            }

            // 2. SHADER PROBE: POIYOMI / LILTOON
            // We use the scene-scraped materials from AuditGeometryAndMaterials for efficiency
            var sceneMaterials = GetCachedObjects<Renderer>(true)
                .SelectMany(r => r.sharedMaterials)
                .Distinct()
                .Where(m => m != null);

            foreach (var mat in sceneMaterials)
            {
                if (mat.shader == null) continue;
                string sName = mat.shader.name;

                // Poiyomi Detection
                if (sName.Contains("Poiyomi") && mat.HasProperty("_AudioLinkEnable"))
                {
                    if (mat.GetFloat("_AudioLinkEnable") > 0 && !coreExists)
                    {
                        LogDiagnostic("3RD PARTY: SHADER", "Orphaned Poiyomi AudioLink", 
                            $"Material '{mat.name}' is trying to use AudioLink, but no controller exists in the scene.", 
                            "#ffaa00", mat);
                    }
                }

                // lilToon Detection
                if (sName.Contains("lilToon") && mat.HasProperty("_AudioLink"))
                {
                    if (mat.GetFloat("_AudioLink") > 0 && !coreExists)
                    {
                        LogDiagnostic("3RD PARTY: SHADER", "Orphaned lilToon AudioLink", 
                            $"lilToon material '{mat.name}' is listening for a missing AudioLink Core.", 
                            "#ffaa00", mat);
                    }
                }
            }

            // 3. SCRIPT PROBE: VRSL / LTCGI / VVMW
            
            // VRSL Check
            Type vrslAdapterType = GetTypeSafe("VRSL.AudioLinkAdapter.VRSL_AudioLinkAdapter");
            if (vrslAdapterType != null)
            {
                foreach (var adapter in GetCachedObjects(vrslAdapterType, true))
                {
                    if (!coreExists) LogDiagnostic("3RD PARTY: VRSL", "Dead VRSL Adapter", "VRSL is waiting for AudioLink but the Core is missing.", "#ff00aa", (Component)adapter);
                }
            }

            // LTCGI Check
            Type ltcgiControllerType = GetTypeSafe("LTCGI.LTCGI_Controller");
            if (ltcgiControllerType != null)
            {
                foreach (var ltcgi in GetCachedObjects(ltcgiControllerType, true))
                {
                    var alInput = ltcgiControllerType.GetField("audioLinkInput", flags);
                    if (alInput != null && (int)alInput.GetValue(ltcgi) == 1 && !coreExists) // 1 = AL Mode
                    {
                        LogDiagnostic("3RD PARTY: LTCGI", "LTCGI Disconnect", "LTCGI set to AudioLink mode but no core found.", "#ff00aa", (Component)ltcgi);
                    }
                }
            }

            // VizVid (VVMW) Internal Reference Check
            if (vvmwCoreType != null && coreExists)
            {
                foreach (var vvmw in GetCachedObjects(vvmwCoreType, true))
                {
                    var alRefField = vvmwCoreType.GetField("audioLink", flags);
                    if (alRefField != null && alRefField.GetValue(vvmw) == null)
                    {
                        LogDiagnostic("VIZVID: TOPOLOGY", "AudioLink Not Linked", 
                            "VizVid Core is not linked to AudioLink. VizVid cannot automatically sync track time and media states (Play/Pause) to your shaders.", 
                            "#00e5ff", (Component)vvmw, () => {
                                Undo.RecordObject((Component)vvmw, "Link VVMW to AudioLink");
                                alRefField.SetValue(vvmw, alCore);
                                PrefabUtility.RecordPrefabInstancePropertyModifications((Component)vvmw);
                            });
                    }
                }
            }

            // 4. NATIVE REACTIVE ORPHANS
            if (reactiveType != null)
            {
                foreach (var r in GetCachedObjects(reactiveType, true))
                {
                    var component = (Component)r;
                    var alField = component.GetType().GetField("audioLink", flags);
                    if (alField != null && alField.GetValue(r) == null && coreExists)
                    {
                        LogDiagnostic("AUDIOLINK: ORPHAN", "Unlinked Reactive Object", 
                            $"'{component.gameObject.name}' has no core assigned. It will never move or glow.", 
                            "#ff00aa", component, () => {
                                Undo.RecordObject(component, "Auto-Link Reactive Object");
                                alField.SetValue(r, alCore);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                    }
                }
            }
        }

        private void AuditRinvoSearchEcosystem()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type rinvoType = GetTypeSafe("Rinvo.YoutubeSearchManager");
            if (rinvoType == null) return;

            foreach (var searchManager in GetCachedObjects(rinvoType, true))
            {
                var component = (Component)searchManager;
                
                // Fetch Core Fields
                var uiControllerField = rinvoType.GetField("VideoPlayerUIController", flags);
                var urlField = rinvoType.GetField("UrlInputField", flags);
                var playerTypeField = rinvoType.GetField("videoPlayerType", flags);

                var currentUiController = uiControllerField?.GetValue(searchManager) as UdonBehaviour;
                var currentUrlInput = urlField?.GetValue(searchManager) as VRC.SDK3.Components.VRCUrlInputField;
                int currentPlayerType = playerTypeField != null ? Convert.ToInt32(playerTypeField.GetValue(searchManager)) : 0;

                // === 1. MISSING REFERENCES & AUTO-LINKING ===
                if (currentUiController == null || currentUrlInput == null)
                {
                    UdonBehaviour detectedUi = null;
                    VRC.SDK3.Components.VRCUrlInputField detectedInput = null;
                    int detectedEnum = 0;
                    string detectedName = "";

                    // Attempt A: ProTV 3
                    Type protvUrlInputType = GetTypeSafe("ArchiTech.ProTV.UrlInput");
                    if (protvUrlInputType != null)
                    {
                        var protvInput = FindObjectOfType(protvUrlInputType);
                        if (protvInput != null)
                        {
                            detectedUi = protvInput as UdonBehaviour;
                            detectedInput = ((Component)protvInput).GetComponentInChildren<VRC.SDK3.Components.VRCUrlInputField>(true);
                            detectedEnum = 2; // VideoPlayerType.ProTV3
                            detectedName = "ProTV 3";
                        }
                    }
                    
                    // Attempt B: IwaSync3
                    if (detectedUi == null)
                    {
                        Type iwaControllerType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoController");
                        if (iwaControllerType != null)
                        {
                            var iwaController = FindObjectOfType(iwaControllerType);
                            if (iwaController != null)
                            {
                                detectedUi = iwaController as UdonBehaviour;
                                detectedInput = ((Component)iwaController).GetComponentInChildren<VRC.SDK3.Components.VRCUrlInputField>(true);
                                detectedEnum = 3; // VideoPlayerType.IwaSync3
                                detectedName = "IwaSync3";
                            }
                        }
                    }

                    // Attempt C: TXL (Texel) Input Proxy
                    if (detectedUi == null)
                    {
                        Type txlProxyType = GetTypeSafe("Texel.InputProxy") ?? GetTypeSafe("Texel.Video.UI.InputProxy");
                        if (txlProxyType != null)
                        {
                            var txlProxy = FindObjectOfType(txlProxyType);
                            if (txlProxy != null)
                            {
                                detectedUi = txlProxy as UdonBehaviour;
                                SerializedObject so = new SerializedObject(txlProxy);
                                SerializedProperty urlProp = so.FindProperty("urlInputField");
                                if (urlProp != null && urlProp.objectReferenceValue != null)
                                {
                                    detectedInput = urlProp.objectReferenceValue as VRC.SDK3.Components.VRCUrlInputField;
                                }
                                detectedEnum = 6; // VideoPlayerType.Other (TXL hooks natively)
                                detectedName = "TXL Input Proxy";
                            }
                        }
                    }

                    // Attempt D: USharpVideo
                    if (detectedUi == null)
                    {
                        Type usharpType = GetTypeSafe("UdonSharpVideo.USharpVideoPlayer");
                        if (usharpType != null)
                        {
                            var usharpPlayer = FindObjectOfType(usharpType);
                            if (usharpPlayer != null)
                            {
                                detectedUi = usharpPlayer as UdonBehaviour;
                                detectedInput = ((Component)usharpPlayer).GetComponentInChildren<VRC.SDK3.Components.VRCUrlInputField>(true);
                                if (detectedInput == null && ((Component)usharpPlayer).transform.parent != null)
                                {
                                     detectedInput = ((Component)usharpPlayer).transform.parent.GetComponentInChildren<VRC.SDK3.Components.VRCUrlInputField>(true);
                                }
                                detectedEnum = 0; // VideoPlayerType.USharpVideo
                                detectedName = "USharpVideo";
                            }
                        }
                    }

                    if (detectedUi != null && detectedInput != null)
                    {
                        LogDiagnostic("YOUTUBE SEARCH ECOSYSTEM", "Missing Video Player Link", 
                            $"'{component.gameObject.name}' is missing references to a Video Player. Auto-detected {detectedName} in the scene. Ready to link and configure UI components.", 
                            "#00e5ff", component, () => {
                                Undo.RecordObject(component, "Auto-Link YouTube Search");
                                uiControllerField?.SetValue(searchManager, detectedUi);
                                urlField?.SetValue(searchManager, detectedInput);
                                playerTypeField?.SetValue(searchManager, detectedEnum);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                        
                        currentUiController = detectedUi;
                        currentUrlInput = detectedInput;
                        currentPlayerType = detectedEnum;
                    }
                    else
                    {
                        LogDiagnostic("YOUTUBE SEARCH ECOSYSTEM", "Orphaned Search Manager", 
                            $"'{component.gameObject.name}' is missing Video Player UI references and no compatible video player could be automatically detected in the scene. Manual setup required.", 
                            "#ff00aa", component);
                    }
                }

                if (currentUiController != null)
                {
                    string uiName = GetUdonTypeNameSafe(currentUiController);
                    int expectedEnum = currentPlayerType;
                    string expectedName = "";

                    // === 2. ARCHITECTURAL DECOUPLING (CORE VS UI LAYER) ===
                    Type protvTvType = GetTypeSafe("ArchiTech.ProTV.TVManager");
                    Type protvInputType = GetTypeSafe("ArchiTech.ProTV.UrlInput");
                    if (protvTvType != null && currentUiController.GetComponent(protvTvType) != null)
                    {
                        expectedEnum = 2; expectedName = "ProTV 3";
                        if (protvInputType != null)
                        {
                            var actualInput = currentUiController.GetComponentInChildren(protvInputType) ?? FindObjectOfType(protvInputType);
                            if (actualInput != null)
                            {
                                LogDiagnostic("PROTV + RINVO ECOSYSTEM", "Invalid ProTV UI Target", 
                                    $"'{component.gameObject.name}' is pointing directly to the TVManager instead of the UrlInput component. Rinvo's custom event ('EndEditUrlInput') exclusively targets the UrlInput script.", 
                                    "#ff00aa", component, () => {
                                        Undo.RecordObject(component, "Fix ProTV Target");
                                        uiControllerField?.SetValue(searchManager, actualInput as UdonBehaviour);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                    });
                                
                                currentUiController = actualInput as UdonBehaviour;
                                uiName = GetUdonTypeNameSafe(currentUiController);
                            }
                        }
                    }
                    
                    Type iwaCoreType = GetTypeSafe("HoshinoLabs.IwaSync3.IwaSync3");
                    Type iwaControllerType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoController");
                    if (iwaCoreType != null && currentUiController.GetComponent(iwaCoreType) != null)
                    {
                        expectedEnum = 3; expectedName = "IwaSync3";
                        if (iwaControllerType != null)
                        {
                            var actualController = currentUiController.GetComponentInChildren(iwaControllerType) ?? FindObjectOfType(iwaControllerType);
                            if (actualController != null)
                            {
                                LogDiagnostic("IWASYNC3 + RINVO ECOSYSTEM", "Invalid IwaSync3 UI Target", 
                                    $"'{component.gameObject.name}' is pointing directly to the core IwaSync3 manager instead of its UI VideoController. Rinvo's custom events ('OnURLChanged') only exist on the UI component.", 
                                    "#ff00aa", component, () => {
                                        Undo.RecordObject(component, "Fix IwaSync Target");
                                        uiControllerField?.SetValue(searchManager, actualController as UdonBehaviour);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                    });
                                
                                currentUiController = actualController as UdonBehaviour;
                                uiName = GetUdonTypeNameSafe(currentUiController);
                            }
                        }
                    }

                    Type txlPlayerType = GetTypeSafe("Texel.TXLVideoPlayer") ?? GetTypeSafe("Texel.SyncPlayer");
                    Type txlProxyType = GetTypeSafe("Texel.InputProxy") ?? GetTypeSafe("Texel.Video.UI.InputProxy");
                    if (txlPlayerType != null && currentUiController.GetComponent(txlPlayerType) != null)
                    {
                        expectedEnum = 6; expectedName = "TXL Input Proxy (Other)";
                        if (txlProxyType != null)
                        {
                            var actualInput = currentUiController.GetComponentInChildren(txlProxyType) ?? FindObjectOfType(txlProxyType);
                            if (actualInput != null)
                            {
                                LogDiagnostic("TXL + RINVO ECOSYSTEM", "Invalid TXL UI Target", 
                                    $"'{component.gameObject.name}' is pointing directly to the Core TXL Video Player instead of the InputProxy. Rinvo must be linked to the InputProxy component for events and queues to execute properly.", 
                                    "#ff00aa", component, () => {
                                        Undo.RecordObject(component, "Fix TXL Target");
                                        uiControllerField?.SetValue(searchManager, actualInput as UdonBehaviour);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                    });
                                
                                currentUiController = actualInput as UdonBehaviour;
                                uiName = GetUdonTypeNameSafe(currentUiController);
                            }
                        }
                    }

                    // === 3. ENUM / TARGET MISMATCH LOGIC ===
                    if (uiName.Contains("UrlInput") && currentPlayerType != 2) { expectedEnum = 2; expectedName = "ProTV 3"; }
                    else if ((uiName.Contains("VideoController") || uiName.Contains("IwaSync3")) && currentPlayerType != 3) { expectedEnum = 3; expectedName = "IwaSync3"; }
                    else if (uiName.Contains("USharpVideo") && currentPlayerType != 0) { expectedEnum = 0; expectedName = "USharpVideo"; }
                    else if ((uiName.IndexOf("InputProxy", StringComparison.OrdinalIgnoreCase) >= 0 || uiName.IndexOf("Texel", StringComparison.OrdinalIgnoreCase) >= 0 || uiName.IndexOf("TXL", StringComparison.OrdinalIgnoreCase) >= 0) && currentPlayerType != 6) { expectedEnum = 6; expectedName = "TXL Input Proxy (Other)"; }

                    if (expectedEnum != currentPlayerType && !string.IsNullOrEmpty(expectedName))
                    {
                        LogDiagnostic("YOUTUBE SEARCH ECOSYSTEM", "Mismatched Player Target Enum", 
                            $"'{component.gameObject.name}' is linked to {expectedName}, but its VideoPlayerType enum is incorrectly targeting Enum ID {currentPlayerType}. This will cause Rinvo to send the wrong playback events.", 
                            "#ffaa00", component, () => {
                                Undo.RecordObject(component, "Fix Search Player Target Enum");
                                playerTypeField?.SetValue(searchManager, expectedEnum);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                    }

                    // === 4. TEXEL (TXL) CONFLICT RESOLUTION ===
                    if (uiName.IndexOf("InputProxy", StringComparison.OrdinalIgnoreCase) >= 0 || 
                        uiName.IndexOf("Texel", StringComparison.OrdinalIgnoreCase) >= 0 || 
                        uiName.IndexOf("TXL", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Component proxyComp = null;

                        foreach (var comp in currentUiController.GetComponents<UdonSharpBehaviour>())
                        {
                            if (comp != null && comp.GetType().Name.Contains("InputProxy"))
                            {
                                proxyComp = comp;
                                break;
                            }
                        }

                        if (proxyComp != null)
                        {
                            SerializedObject proxySO = new SerializedObject(proxyComp);
                            SerializedProperty alwaysQueueProp = proxySO.FindProperty("alwaysUseQueue");

                            var usingQueueField = rinvoType.GetField("UsingQueue", flags);
                            var usingOnlyQueueField = rinvoType.GetField("UsingOnlyQueue", flags);

                            bool alwaysQ = alwaysQueueProp != null && alwaysQueueProp.boolValue;
                            bool usingQ = usingQueueField != null && Convert.ToBoolean(usingQueueField.GetValue(searchManager));
                            bool onlyQ = usingOnlyQueueField != null && Convert.ToBoolean(usingOnlyQueueField.GetValue(searchManager));

                            // "ALWAYS USE QUEUE" UX MISMATCH (Align Rinvo to TXL)
                            if (alwaysQ && usingQ && !onlyQ)
                            {
                                LogDiagnostic("TXL + RINVO ECOSYSTEM", "Play Button Hijacked (Queue Mismatch)", 
                                    $"Rinvo Search displays BOTH Play and Queue buttons, but the TXL Input Proxy '{proxyComp.gameObject.name}' has 'Always Use Queue' enabled. The Play button will secretly act as a Queue button. Click fix to align Rinvo to TXL by enabling 'Only Queue'.", 
                                    "#ffaa00", component, () => {
                                        Undo.RecordObject(component, "Align Rinvo to TXL Queue Mode");
                                        if (usingOnlyQueueField != null) usingOnlyQueueField.SetValue(searchManager, true);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                    });
                            }
                        }
                    }

                    // === 5. UNITY BASE / USHARPVIDEO CONFLICT RESOLUTION ===
                    if (expectedEnum == 0 || uiName.Contains("USharpVideo"))
                    {
                        var swapAvproField = rinvoType.GetField("swapToAvproForLivestreams", flags);
                        if (swapAvproField != null && !Convert.ToBoolean(swapAvproField.GetValue(searchManager)))
                        {
                            LogDiagnostic("USHARPVIDEO + RINVO ECOSYSTEM", "Live Stream Auto-Swap Disabled", 
                                $"'{component.gameObject.name}' is linked to USharpVideo but 'Swap To Avpro For Livestreams' is disabled. The Unity Base VideoPlayer cannot parse YouTube Live Streams, causing silent sync failures for all users when a stream is clicked.", 
                                "#ffaa00", component, () => {
                                    Undo.RecordObject(component, "Enable Livestream Auto-Swap");
                                    swapAvproField.SetValue(searchManager, true);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                });
                        }
                    }
                }

                // === 6. FALLBACK QUEUE AUTO-LINKING (For ProTV / Non-TXL) ===
                var usingQueueFieldCheck = rinvoType.GetField("UsingQueue", flags);
                if (usingQueueFieldCheck != null && Convert.ToBoolean(usingQueueFieldCheck.GetValue(searchManager)))
                {
                    var queueUiField = rinvoType.GetField("QueueUIController", flags);
                    var queueUrlField = rinvoType.GetField("UrlInputFieldQueue", flags);
                    
                    if ((queueUiField?.GetValue(searchManager) as UdonBehaviour) == null || (queueUrlField?.GetValue(searchManager) as VRC.SDK3.Components.VRCUrlInputField) == null)
                    {
                        // Note: TXL Queue linking is already handled comprehensively in Step 4A. This is a fallback for ProTV
                        Type protvQueueType = GetTypeSafe("ArchiTech.ProTV.Queue");
                        if (protvQueueType != null)
                        {
                            var protvQueue = FindObjectOfType(protvQueueType);
                            if (protvQueue != null)
                            {
                                var queueInput = ((Component)protvQueue).GetComponentInChildren<VRC.SDK3.Components.VRCUrlInputField>(true);
                                if (queueInput != null)
                                {
                                    LogDiagnostic("YOUTUBE SEARCH ECOSYSTEM", "Missing Queue Link", 
                                        $"'{component.gameObject.name}' has UsingQueue enabled but no targets. Auto-detected ProTV Queue in the scene. Ready to link.", 
                                        "#00e5ff", component, () => {
                                            Undo.RecordObject(component, "Auto-Link Search Queue");
                                            queueUiField?.SetValue(searchManager, protvQueue as UdonBehaviour);
                                            queueUrlField?.SetValue(searchManager, queueInput);
                                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                                        });
                                }
                            }
                        }
                    }
                }

                // === 7. POOL SIZE BOUNDS CHECKS ===
                var poolSizeFieldCheck = rinvoType.GetField("poolSize", flags);
                if (poolSizeFieldCheck != null)
                {
                    int poolSize = Convert.ToInt32(poolSizeFieldCheck.GetValue(searchManager));
                    if (poolSize < 100)
                    {
                        LogDiagnostic("YOUTUBE SEARCH ECOSYSTEM", "Critically Low API Pool Size", 
                            $"'{component.gameObject.name}' has a pool size of {poolSize}. The creator recommends a minimum of 100 to avoid severe Udon API rate limits/errors.", 
                            "#ffaa00", component, () => {
                                Undo.RecordObject(component, "Normalize Pool Size");
                                poolSizeFieldCheck.SetValue(searchManager, 100);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                    }
                    else if (poolSize > 100000)
                    {
                        LogDiagnostic("YOUTUBE SEARCH ECOSYSTEM", "Massive API Pool Size", 
                            $"'{component.gameObject.name}' has a pool size of {poolSize}. This exceeds the maximum safe limit (100,000) and will permanently bloat network serialization.", 
                            "#ff00aa", component, () => {
                                Undo.RecordObject(component, "Normalize Pool Size");
                                poolSizeFieldCheck.SetValue(searchManager, 100000);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                    }
                }
            }
        }

        private void AuditLightVolumesEcosystem()
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            // 1. Manager Integrity
            Type managerType = GetTypeSafe("VRCLightVolumes.LightVolumeManager");
            if (managerType != null)
            {
                var managers = GetCachedObjects(managerType, true);
                if (managers.Length > 1)
                {
                    LogDiagnostic("LIGHT VOLUMES ECOSYSTEM", "Multiple Managers Detected", 
                        "Found more than one LightVolumeManager in the scene. There should strictly be only one to avoid global shader variable tearing.", 
                        "#ff00aa", (Component)managers[1]);
                }
            }

            // 2. Setup Thresholds & Bounding Spheres
            Type setupType = GetTypeSafe("VRCLightVolumes.LightVolumeSetup");
            if (setupType != null)
            {
                foreach (var setup in GetCachedObjects(setupType, true))
                {
                    var comp = (Component)setup;
                    var cutoffField = setupType.GetField("LightsBrightnessCutoff", flags);
                    if (cutoffField != null)
                    {
                        float cutoff = Convert.ToSingle(cutoffField.GetValue(setup));
                        if (cutoff < 0.15f)
                        {
                            LogDiagnostic("LIGHT VOLUMES ECOSYSTEM", "Aggressive Brightness Cutoff", 
                                $"'{comp.gameObject.name}' has a LightsBrightnessCutoff of {cutoff}. Extremely low values cause point lights to generate massive bounding spheres, drastically increasing GPU overlap calculations.", 
                                "#ffaa00", comp, () => {
                                    Undo.RecordObject(comp, "Optimize Brightness Cutoff");
                                    cutoffField.SetValue(setup, 0.35f);
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                                });
                        }
                    }
                }
            }

            // 3. Point Light Compute Loads
            Type plvType = GetTypeSafe("VRCLightVolumes.PointLightVolume");
            if (plvType != null)
            {
                foreach (var plv in GetCachedObjects(plvType, true))
                {
                    var comp = (Component)plv;
                    var typeField = plvType.GetField("Type", flags);
                    if (typeField != null)
                    {
                        int typeVal = Convert.ToInt32(typeField.GetValue(plv));
                        if (typeVal == 2) // 2 corresponds to AreaLight in the Enum
                        {
                            LogDiagnostic("LIGHT VOLUMES COMPUTE", "Area Light Detected", 
                                $"'{comp.gameObject.name}' is set to Area Light. This is the heaviest mathematical light shape. Unless it is a dynamic moving panel, consider baking a standard Light Volume instead.", 
                                "#ffaa00", comp);
                        }
                    }
                }
            }

            // 4. TVGI Integration & Strobe Safety
            Type tvgiType = GetTypeSafe("VRCLightVolumes.LightVolumeTVGI");
            if (tvgiType != null)
            {
                foreach (var tvgi in GetCachedObjects(tvgiType, true))
                {
                    var comp = (Component)tvgi;
                    var rtField = tvgiType.GetField("TargetRenderTexture", flags);
                    if (rtField != null && rtField.GetValue(tvgi) == null)
                    {
                        LogDiagnostic("LIGHT VOLUMES TVGI", "Missing Render Target", 
                            $"'{comp.gameObject.name}' has no Target RenderTexture assigned. It is eating CPU cycles calculating nothing.", 
                            "#ff00aa", comp);
                    }

                    var flickerField = tvgiType.GetField("AntiFlickering", flags);
                    if (flickerField != null && !Convert.ToBoolean(flickerField.GetValue(tvgi)))
                    {
                        LogDiagnostic("LIGHT VOLUMES TVGI", "Anti-Flicker Disabled", 
                            $"'{comp.gameObject.name}' has Anti-Flickering disabled. Rapidly changing video pixels will cause seizure-inducing strobe lighting across the room.", 
                            "#ff00aa", comp, () => {
                                Undo.RecordObject(comp, "Enable Anti-Flicker");
                                flickerField.SetValue(tvgi, true);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                            });
                    }
                }
            }

            // 5. AudioLink Strobe Safety
            Type alType = GetTypeSafe("VRCLightVolumes.LightVolumeAudioLink");
            if (alType != null)
            {
                foreach (var al in GetCachedObjects(alType, true))
                {
                    var comp = (Component)al;
                    var smoothField = alType.GetField("SmoothingEnabled", flags);
                    if (smoothField != null && !Convert.ToBoolean(smoothField.GetValue(al)))
                    {
                        LogDiagnostic("LIGHT VOLUMES AUDIOLINK", "Smoothing Disabled", 
                            $"'{comp.gameObject.name}' has smoothing disabled. Unfiltered AudioLink raw data can cause rapid visual flickering.", 
                            "#ffaa00", comp, () => {
                                Undo.RecordObject(comp, "Enable AudioLink Smoothing");
                                smoothField.SetValue(al, true);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                            });
                    }
                }
            }
        }

        private void AuditUdonAndNetwork()
        {
            Assembly editorAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "UdonSharp.Editor");
            Type cacheType = editorAsm?.GetType("UdonSharp.UdonSharpEditorCache");
            var cache = cacheType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
            var getUasm = cacheType?.GetMethod("GetUASMStr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var udon in GetCachedObjects<UdonBehaviour>(true))
            {
                if (udon.SyncMethod == VRC.SDKBase.Networking.SyncType.Continuous)
                    LogDiagnostic("UDON BANDWIDTH: CONTINUOUS SYNC", "Continuous Sync Active", $"'{udon.gameObject.name}' consumes high bandwidth. Verify if manual sync is possible.", "#ff00aa", udon.gameObject);

                if (udon.programSource is UdonSharpProgramAsset uAsset && getUasm != null && cache != null)
                {
                    string uasm = (string)getUasm.Invoke(cache, new object[] { uAsset });
                    if (!string.IsNullOrEmpty(uasm))
                    {
                        int count = uasm.Split('\n').Count(l => l.Contains(",") || l.Trim().EndsWith("EXTERN"));
                        if (count > 4000) LogDiagnostic("UDON COMPUTE: HEAVY INSTRUCTIONS", "Heavy Instruction Count", $"'{uAsset.name}' executes {count} UASM lines.", "#ffaa00", udon.gameObject);
                    }
                }
            }

            foreach (var objSync in GetCachedObjects<VRCObjectSync>(true))
            {
                LogDiagnostic("UDON PHYSICS: OBJECT SYNC", "VRC Object Sync", $"'{objSync.gameObject.name}' transmits physics state over network.", "#00ff88", objSync.gameObject);
            }
        }

        private void AuditLightingAndCameras()
        {
            // === LIGHTING ===
            foreach (var light in GetCachedObjects<Light>(true))
            {
                if (light == null) continue; 
                
                var component = (Component)light;
                
                if (light.type != LightType.Directional && light.lightmapBakeType == LightmapBakeType.Realtime)
                {
                    LogDiagnostic("LIGHTING & SHADOWS", "Realtime Point/Spot Light", 
                        $"'{light.name}' is fully dynamic. Overlapping realtime point/spot lights cause severe draw call multiplication. Bake this light or use Mixed mode.", 
                        "#ffaa00", component, () => {
                            Undo.RecordObject(component, "Change Light to Baked");
                            light.lightmapBakeType = LightmapBakeType.Baked;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                }

                if (light.lightmapBakeType == LightmapBakeType.Realtime && light.shadows != LightShadows.None)
                {
                    LogDiagnostic("LIGHTING & SHADOWS", "Realtime Shadow Caster", 
                        $"'{light.name}' is casting realtime shadows. This essentially renders the entire scene geometry an additional time per light. Limit to one directional light.", 
                        "#ff00aa", component, () => {
                            Undo.RecordObject(component, "Disable Realtime Shadows");
                            light.shadows = LightShadows.None;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                }
                
                if (light.range > 50f && light.type != LightType.Directional)
                {
                    LogDiagnostic("LIGHTING & SHADOWS", "Massive Light Range", 
                        $"'{light.name}' has a range of {light.range}m. Large overlapping light volumes cripple pixel fillrate. Clamp the range to the immediate affected area.", 
                        "#00e5ff", component);
                }
            }

            // === REFLECTION PROBES ===
            foreach (var probe in GetCachedObjects<ReflectionProbe>(true))
            {
                if (probe == null) continue;

                var component = (Component)probe;

                if (probe.mode == UnityEngine.Rendering.ReflectionProbeMode.Realtime)
                {
                    LogDiagnostic("LIGHTING & SHADOWS", "Realtime Reflection Probe", 
                        $"'{probe.name}' is rendering the scene dynamically. This acts as 6 extra cameras rendering every frame or time-slice. Bake it unless absolutely necessary for mirrors.", 
                        "#ff00aa", component, () => {
                            Undo.RecordObject(component, "Bake Reflection Probe");
                            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                }

                if (probe.resolution > 512)
                {
                    LogDiagnostic("LIGHTING & SHADOWS", "VRAM Nuke: 4K/2K Probe", 
                        $"'{probe.name}' has a resolution of {probe.resolution}. Reflection probes are cubemaps (6 textures). A 1024+ probe will nuke VRAM. Drop this to 256 or 512.", 
                        "#ff00aa", component, () => {
                            Undo.RecordObject(component, "Throttle Probe Resolution");
                            probe.resolution = 256;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                }
            }

            // === CAMERAS ===
            foreach (var cam in GetCachedObjects<Camera>(true))
            {
                if (cam == null) continue;

                var component = (Component)cam;
                
                // 1. Skip VRChat's safe reference cameras
                if (cam.name == "VRCCam" || cam.gameObject.tag == "MainCamera") continue;

                // 2. Skip cameras that are physically disabled
                if (!cam.gameObject.activeInHierarchy || !cam.enabled) continue;

                // 3. UI Event Camera Protection
                // If the Culling Mask is 0 ("Nothing"), it renders no geometry. 
                // It is functionally safe and operates purely for UI Raycasts.
                bool isEventCamera = cam.cullingMask == 0;

                // If it renders to the screen (no target texture) and actually draws geometry...
                if (cam.targetTexture == null && !isEventCamera)
                {
                    LogDiagnostic("RENDER PIPELINE", "Rogue Active Camera", 
                        $"'{cam.name}' is active, rendering the world (Culling Mask != Nothing), and outputs directly to the screen. This forces the engine to double-render the world geometry. Disable it, assign a RenderTexture, or set Culling Mask to 'Nothing' if it's an Event Camera.", 
                        "#ff00aa", component, () => {
                            // Safely disable the component rather than the GameObject
                            Undo.RecordObject(cam, "Disable Rogue Camera");
                            cam.enabled = false;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(cam);
                        });
                }

                if (cam.targetTexture != null && cam.targetTexture.width > 2048)
                {
                    LogDiagnostic("RENDER PIPELINE", "Massive Render Target", 
                        $"'{cam.name}' renders to a {cam.targetTexture.width}x{cam.targetTexture.height} texture. This causes massive VRAM allocation and pixel fillrate lag.", 
                        "#ffaa00", component);
                }
            }
        }

        private void AuditPhysics()
        {
            // === COLLIDERS ===
            foreach (var collider in GetCachedObjects<MeshCollider>(true))
            {
                var component = (Component)collider;

                if (!collider.convex)
                {
                    LogDiagnostic("PHYSICS & COLLIDERS", "Non-Convex Mesh Collider", 
                        $"'{collider.gameObject.name}' uses a non-convex mesh collider. The physics engine must calculate collision against every single polygon. Switch to Convex, or better, use primitive box/sphere colliders.", 
                        "#ff00aa", component, () => {
                            Undo.RecordObject(component, "Make Convex");
                            collider.convex = true;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                }
                else if (collider.sharedMesh != null && collider.sharedMesh.vertexCount > 255)
                {
                    LogDiagnostic("PHYSICS & COLLIDERS", "High-Poly Convex Collider", 
                        $"'{collider.gameObject.name}' uses a complex mesh ({collider.sharedMesh.vertexCount} verts) for physics. Unity restricts convex hulls to 255 polygons, forcing an expensive internal bake. Use a low-poly proxy mesh.", 
                        "#ffaa00", component);
                }
            }

            // === RIGIDBODIES ===
            foreach (var rb in GetCachedObjects<Rigidbody>(true))
            {
                var component = (Component)rb;

                if (rb.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic)
                {
                    LogDiagnostic("PHYSICS & COLLIDERS", "Continuous Dynamic Physics", 
                        $"'{rb.gameObject.name}' is using Continuous Dynamic collision. This uses continuous swept collision detection which causes severe CPU spikes. Only use this for extreme high-speed projectiles.", 
                        "#ffaa00", component, () => {
                            Undo.RecordObject(component, "Throttle Collision Detection");
                            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                }
            }
        }

        private void AuditTerrainAndEnvironment()
        {
            // === TERRAINS ===
            foreach (var terrain in GetCachedObjects<Terrain>(true))
            {
                // Null-safety check for objects in transition
                if (terrain == null) continue;

                var component = (Component)terrain;

                if (terrain.terrainData != null)
                {
                    if (terrain.terrainData.heightmapResolution > 1025)
                    {
                        LogDiagnostic("ENVIRONMENT", "Insane Heightmap Resolution", 
                            $"'{terrain.name}' has a heightmap resolution of {terrain.terrainData.heightmapResolution}. This allocates massive memory overhead. VRChat terrain should rarely exceed 1025 (1024x1024).", 
                            "#ff00aa", component);
                    }
                }

                if (!terrain.drawInstanced)
                {
                    LogDiagnostic("ENVIRONMENT", "Draw Instanced Disabled", 
                        $"'{terrain.name}' has Draw Instanced disabled. Instancing significantly reduces CPU draw call overhead for terrain trees and geometry. Enable this in the terrain settings.", 
                        "#00e5ff", component, () => {
                            Undo.RecordObject(component, "Enable Terrain Instancing");
                            terrain.drawInstanced = true;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                }

                if (terrain.heightmapPixelError < 5f)
                {
                    LogDiagnostic("ENVIRONMENT", "Low Pixel Error", 
                        $"'{terrain.name}' has a Pixel Error of {terrain.heightmapPixelError}. Low values force high-poly LODs even at a distance. Increase this to 5 or higher for better framerates.", 
                        "#ffaa00", component, () => {
                            Undo.RecordObject(component, "Optimize Pixel Error");
                            terrain.heightmapPixelError = 5f;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                }
            }

            // === GLOBAL ILLUMINATION & LIGHTMAPS ===
            if (Lightmapping.realtimeGI)
            {
                LogDiagnostic("ENVIRONMENT & LIGHTING", "Global Realtime GI Enabled", 
                    "The scene Lighting Settings have 'Realtime Global Illumination' enabled. This forces the CPU to calculate bouncing light rays at runtime. Disable this and use Baked GI for VRChat.", 
                    "#ff00aa", null, () => {
                        Lightmapping.realtimeGI = false;
                    });
            }

            if (LightmapSettings.lightmaps != null && LightmapSettings.lightmaps.Length > 10)
            {
                LogDiagnostic("ENVIRONMENT & LIGHTING", "Excessive Lightmap Count", 
                    $"The scene contains {LightmapSettings.lightmaps.Length} lightmaps. This causes heavy VRAM bloat and memory bandwidth limits. Reduce lightmap resolution or bake less objects.", 
                    "#ffaa00", null);
            }

            // 4D-CHESS FIX: Unity 2022+ throws an exception here if no asset is assigned.
            LightingSettings lightingSettings = null;
            try 
            {
                lightingSettings = Lightmapping.lightingSettings;
            }
            catch 
            {
                // Explicitly swallow API exception to prevent scan interruption
            }

            if (lightingSettings != null)
            {
                if (lightingSettings.lightmapMaxSize > 2048)
                {
                    LogDiagnostic("ENVIRONMENT & LIGHTING", "Massive Lightmap Atlas Size", 
                        $"The max lightmap atlas size is set to {lightingSettings.lightmapMaxSize}. Values above 2048 exponentially increase VRAM usage and world download size. Consider capping it to 2048 or 1024.", 
                        "#ffaa00", null, () => {
                            Undo.RecordObject(lightingSettings, "Optimize Max Atlas Size");
                            lightingSettings.lightmapMaxSize = 2048;
                            EditorUtility.SetDirty(lightingSettings);
                        });
                }

                if (lightingSettings.directionalityMode == LightmapsMode.CombinedDirectional)
                {
                    LogDiagnostic("ENVIRONMENT & LIGHTING", "Directional Lightmaps Enabled", 
                        "Directional Lightmaps are enabled. This effectively doubles the VRAM and file size of your baked lightmaps to store specular direction data. If you are struggling with world size, change this to Non-Directional.", 
                        "#00e5ff", null, () => {
                            Undo.RecordObject(lightingSettings, "Change to Non-Directional Lightmaps");
                            lightingSettings.directionalityMode = LightmapsMode.NonDirectional;
                            EditorUtility.SetDirty(lightingSettings);
                        });
                }
                
                if (lightingSettings.lightmapResolution > 40f)
                {
                    LogDiagnostic("ENVIRONMENT & LIGHTING", "High Lightmap Bake Resolution", 
                        $"The bake resolution is set to {lightingSettings.lightmapResolution} texels per unit. High values cause exponentially longer bake times and massive lightmap memory. Ensure this is necessary, or drop it to 40 or lower.", 
                        "#ffaa00", null, () => {
                            Undo.RecordObject(lightingSettings, "Lower Bake Resolution");
                            lightingSettings.lightmapResolution = 40f;
                            EditorUtility.SetDirty(lightingSettings);
                        });
                }
            }
        }

        // === 4D-CHESS CACHING: Prevents AssetDatabase spam during material loops ===
        private HashSet<string> _failedTextureSearches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private void AttemptTextureRecovery(Material mat)
        {
            if (mat == null) return;

            // Strip common Unity suffixes to find the true root name (e.g., "Floor01 (Instance)" -> "Floor01")
            string baseName = mat.name.Replace(" (Instance)", "").Replace("_Mat", "").Replace("_Material", "").Trim();

            // The Omni-Schema: Maps common Shader Property Names -> Suffixes to search for on disk
            var recoverySchema = new Dictionary<string[], string[]>
            {
                // Core PBR / Diffuse
                { new[] { "_MainTex", "_BaseMap", "_BaseColorMap", "_ColorMap" }, new[] { "_BaseMap", "_Albedo", "_Color", "_Diffuse", "_Main", "_Base" } },
                // Normal / Bump
                { new[] { "_BumpMap", "_NormalMap", "_Normal" }, new[] { "_Normal", "_NormalMap", "_Bump", "_Nrm", "_NRM" } },
                // Metallic / Smoothness / Roughness / Masks
                { new[] { "_MetallicGlossMap", "_MaskMap", "_SpecGlossMap", "_MetallicMap", "_RoughnessMap" }, new[] { "_MaskMap", "_Metallic", "_Smoothness", "_Specular", "_Roughness", "_Mask", "_Rgh", "_Met" } },
                // Emission / Glow
                { new[] { "_EmissionMap", "_Emissive", "_Emission" }, new[] { "_Emission", "_Emissive", "_Glow", "_Illum" } },
                // Ambient Occlusion
                { new[] { "_OcclusionMap", "_AmbientOcclusionMap", "_AO" }, new[] { "_AO", "_Occlusion", "_AmbientOcclusion" } },
                // Height / Parallax
                { new[] { "_ParallaxMap", "_HeightMap" }, new[] { "_Height", "_HeightMap", "_Parallax", "_Displacement" } },
                // --- POIYOMI / TOON SPECIFIC ---
                // Matcaps
                { new[] { "_MatcapTex", "_Matcap", "_MatcapTexture", "_Matcap1", "_Matcap2" }, new[] { "_Matcap", "_MC", "_MatcapTex" } },
                { new[] { "_MatcapMask", "_Matcap1Mask", "_Matcap2Mask" }, new[] { "_MatcapMask", "_MCMask" } },
                // Shadows / Ramps
                { new[] { "_ShadowTex", "_ShadowMap", "_ShadowRamp" }, new[] { "_Shadow", "_ShadowMap", "_Ramp" } },
                { new[] { "_ShadowMask" }, new[] { "_ShadowMask" } },
                // Outlines
                { new[] { "_OutlineTexture", "_OutlineTex" }, new[] { "_Outline", "_OutlineTex" } },
                { new[] { "_OutlineMask" }, new[] { "_OutlineMask" } },
                // Fur (Poiyomi Fur)
                { new[] { "_FurNormalMap", "_FurNormal" }, new[] { "_FurNormal", "_FurNrm" } },
                { new[] { "_FurMask", "_FurAlphaMask" }, new[] { "_FurMask", "_FurAlpha" } },
                { new[] { "_FurLengthMask", "_FurLengthMap" }, new[] { "_FurLength", "_FurHeight" } },
                // Details (Filamented & Poiyomi)
                { new[] { "_DetailTex", "_DetailAlbedoMap" }, new[] { "_Detail", "_DetailAlbedo" } },
                { new[] { "_DetailNormalMap", "_DetailNormal" }, new[] { "_DetailNormal", "_DetailNrm" } },
                { new[] { "_DetailMask" }, new[] { "_DetailMask" } },
                // Decals
                { new[] { "_DecalTexture", "_DecalTex", "_DecalColorMap", "_Decal0", "_Decal1" }, new[] { "_Decal", "_DecalTex", "_Logo" } },
                { new[] { "_DecalMask" }, new[] { "_DecalMask" } }
            };

            var recoveryPlan = new Dictionary<string, Texture2D>();

            foreach (var target in recoverySchema)
            {
                string[] shaderProperties = target.Key;
                string[] searchSuffixes = target.Value;

                string activeProp = null;
                bool slotNeedsRepair = false;

                foreach (var prop in shaderProperties)
                {
                    if (mat.HasProperty(prop))
                    {
                        activeProp = prop;
                        if (mat.GetTexture(prop) == null) slotNeedsRepair = true;
                        break; 
                    }
                }

                if (activeProp != null && slotNeedsRepair)
                {
                    Texture2D foundTex = null;

                    foreach (var suffix in searchSuffixes)
                    {
                        string expectedName = baseName + suffix;

                        if (_textureRecoveryCache.TryGetValue(expectedName, out var cachedTex))
                        {
                            foundTex = cachedTex;
                            break;
                        }
                        if (_failedTextureSearches.Contains(expectedName))
                        {
                            continue;
                        }

                        string searchQuery = $"{expectedName} t:Texture2D";
                        string[] guids = AssetDatabase.FindAssets(searchQuery);

                        foreach (var guid in guids)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guid);
                            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

                            if (string.Equals(fileName, expectedName, StringComparison.OrdinalIgnoreCase))
                            {
                                foundTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                                break;
                            }
                        }

                        if (foundTex != null)
                        {
                            _textureRecoveryCache[expectedName] = foundTex;
                            break; 
                        }
                        else
                        {
                            _failedTextureSearches.Add(expectedName);
                        }
                    }

                    if (foundTex != null)
                    {
                        recoveryPlan[activeProp] = foundTex;
                    }
                }
            }

            if (recoveryPlan.Count > 0)
            {
                string recoveredList = string.Join(", ", recoveryPlan.Values.Select(t => t.name));
                
                LogDiagnostic("MATERIAL PIPELINE: AUTORECOVERY", "Orphaned Textures Found", 
                    $"'{mat.name}' is missing maps. Discovered {recoveryPlan.Count} matching textures in the project via schema: {recoveredList}. Ready to re-bind.", 
                    "#00e5ff", mat, () => {
                        Undo.RecordObject(mat, "Auto-Recover Textures");
                        foreach (var kvp in recoveryPlan)
                        {
                            mat.SetTexture(kvp.Key, kvp.Value);
                        }
                        EditorUtility.SetDirty(mat);
                        AssetDatabase.SaveAssets();
                    });
            }
        }

        private void AuditGeometryAndMaterials()
        {
            var renderers = GetCachedObjects<Renderer>(true);
            HashSet<Material> sceneMaterials = new HashSet<Material>();

            Type vvmwCoreType = GetTypeSafe("JLChnToZ.VRC.VVMW.Core");
            Type proTvType = GetTypeSafe("ArchiTech.ProTV.TVManager");
            Type txlPlayerType = GetTypeSafe("Texel.TXLVideoPlayer");
            Type iwaSyncType = GetTypeSafe("HoshinoLabs.IwaSync3.IwaSync3");
            Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");

            foreach (var renderer in renderers)
            {
                int matCount = renderer.sharedMaterials.Length;
                int submeshCount = 0;

                bool hasMissingMats = false;
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null) 
                    { 
                        hasMissingMats = true; 
                        continue; 
                    }
                    sceneMaterials.Add(mat);
                    ScrapeTexturesFromMaterial(mat); 
                }

                // === 0. NULL MATERIAL RECOVERY PROTOCOL ===
                if (hasMissingMats)
                {
                    LogDiagnostic("MESHES & GEOMETRY", "Missing Material Reference", 
                        $"'{renderer.name}' has null material slots. This results in the infamous Unity pink/invisible mesh bug. Ready to auto-generate and bind replacement materials.", 
                        "#ff00aa", renderer.gameObject, () => {
                            Undo.RecordObject(renderer, "Auto-Generate Missing Materials");
                            var mats = renderer.sharedMaterials;
                            bool changed = false;

                            string saveDir = "Assets/VixenTools/RecoveredMaterials";
                            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(renderer.gameObject);
                            
                            if (prefab != null)
                            {
                                string prefabPath = AssetDatabase.GetAssetPath(prefab);
                                if (!string.IsNullOrEmpty(prefabPath)) saveDir = System.IO.Path.GetDirectoryName(prefabPath).Replace("\\", "/");
                            }
                            else if (renderer is MeshRenderer mr && mr.GetComponent<MeshFilter>()?.sharedMesh != null)
                            {
                                string meshPath = AssetDatabase.GetAssetPath(mr.GetComponent<MeshFilter>().sharedMesh);
                                if (!string.IsNullOrEmpty(meshPath)) saveDir = System.IO.Path.GetDirectoryName(meshPath).Replace("\\", "/");
                            }
                            else if (renderer is SkinnedMeshRenderer smr && smr.sharedMesh != null)
                            {
                                string meshPath = AssetDatabase.GetAssetPath(smr.sharedMesh);
                                if (!string.IsNullOrEmpty(meshPath)) saveDir = System.IO.Path.GetDirectoryName(meshPath).Replace("\\", "/");
                            }

                            if (!saveDir.StartsWith("Assets"))
                            {
                                saveDir = "Assets/VixenTools/RecoveredMaterials";
                            }

                            if (!System.IO.Directory.Exists(saveDir))
                            {
                                System.IO.Directory.CreateDirectory(saveDir);
                                AssetDatabase.Refresh();
                            }

                            for (int i = 0; i < mats.Length; i++)
                            {
                                if (mats[i] == null)
                                {
                                    Shader targetShader = _targetReplacementShader != null ? _targetReplacementShader : Shader.Find("Standard");
                                    Material newMat = new Material(targetShader);
                                    
                                    string cleanName = string.Join("_", renderer.gameObject.name.Split(System.IO.Path.GetInvalidFileNameChars()));
                                    string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{saveDir}/{cleanName}_Recovered_{i}.mat");
                                    
                                    AssetDatabase.CreateAsset(newMat, fullPath);
                                    mats[i] = newMat;
                                    changed = true;
                                }
                            }

                            if (changed)
                            {
                                renderer.sharedMaterials = mats;
                                EditorUtility.SetDirty(renderer);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                                AssetDatabase.SaveAssets();
                            }
                        });
                }

                // === 1. STATIC GEOMETRY PROTECTION ===
                bool isProtectedVideoComponent = false;
                if (vvmwCoreType != null && renderer.GetComponentInParent(vvmwCoreType, true) != null) isProtectedVideoComponent = true;
                else if (proTvType != null && renderer.GetComponentInParent(proTvType, true) != null) isProtectedVideoComponent = true;
                else if (txlPlayerType != null && renderer.GetComponentInParent(txlPlayerType, true) != null) isProtectedVideoComponent = true;
                else if (iwaSyncType != null && renderer.GetComponentInParent(iwaSyncType, true) != null) isProtectedVideoComponent = true;
                else if (audioLinkType != null && renderer.GetComponentInParent(audioLinkType, true) != null) isProtectedVideoComponent = true;

                if (!isProtectedVideoComponent && renderer is MeshRenderer && !renderer.gameObject.isStatic)
                {
                    if (renderer.GetComponentInParent<Rigidbody>() == null && 
                        renderer.GetComponentInParent<VRC.SDK3.Components.VRCPickup>() == null &&
                        renderer.GetComponentInParent<Animator>() == null &&
                        renderer.GetComponentInParent<VRC.Udon.UdonBehaviour>() == null)
                    {
                        LogDiagnostic("MESHES & GEOMETRY", "Unprotected Dynamic Mesh", 
                            $"'{renderer.name}' is not marked as Static, meaning Unity assumes its transform will be modified at runtime. If this is a structural or environmental prop, mark it Static to lock it and enable heavy draw-call batching.", 
                            "#ffaa00", renderer.gameObject, () => {
                                Undo.RecordObject(renderer.gameObject, "Mark Static");
                                renderer.gameObject.isStatic = true;
                                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer.gameObject);
                            });
                    }
                }

                if (renderer is SkinnedMeshRenderer smr && smr.sharedMesh != null) 
                {
                    _detectedMeshes.Add(smr.sharedMesh);
                    submeshCount = smr.sharedMesh.subMeshCount;

                    if (!isProtectedVideoComponent && smr.updateWhenOffscreen)
                    {
                        LogDiagnostic("MESHES & GEOMETRY", "Always Updating Bounds", 
                            $"'{renderer.name}' calculates bone bounds even when completely offscreen. This eats CPU.", 
                            "#ffaa00", renderer.gameObject, () => {
                                Undo.RecordObject(smr, "Disable Update When Offscreen");
                                smr.updateWhenOffscreen = false;
                                PrefabUtility.RecordPrefabInstancePropertyModifications(smr);
                            });
                    }
                }
                else if (renderer is MeshRenderer mr)
                {
                    var filter = mr.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null)
                    {
                        _detectedMeshes.Add(filter.sharedMesh);
                        submeshCount = filter.sharedMesh.subMeshCount;

                        if (!isProtectedVideoComponent && filter.sharedMesh.vertexCount > 5000 && renderer.GetComponentInParent<LODGroup>() == null)
                        {
                            LogDiagnostic("MESHES & GEOMETRY", "Missing LOD Group", 
                                $"'{renderer.name}' has {filter.sharedMesh.vertexCount} verts but no LODs. Will generate a Culling LODGroup.", 
                                "#00e5ff", renderer.gameObject, () => {
                                    Undo.RecordObject(renderer.gameObject, "Generate Culling LODGroup");
                                    LODGroup lodGroup = renderer.gameObject.AddComponent<LODGroup>();
                                    LOD[] lods = new LOD[1];
                                    lods[0] = new LOD(0.05f, new Renderer[] { renderer }); 
                                    lodGroup.SetLODs(lods);
                                    lodGroup.RecalculateBounds();
                                    PrefabUtility.RecordPrefabInstancePropertyModifications(renderer.gameObject);
                                });
                        }
                    }
                }

                if (!isProtectedVideoComponent && submeshCount > 0 && matCount > submeshCount)
                {
                    LogDiagnostic("MESHES & GEOMETRY", "Material Slot Bloat", 
                        $"'{renderer.name}' has {matCount} materials but its mesh only has {submeshCount} submeshes. This wastes draw calls.", 
                        "#ff00aa", renderer.gameObject, () => {
                            Undo.RecordObject(renderer, "Clean Material Slots");
                            var newMats = new Material[submeshCount];
                            System.Array.Copy(renderer.sharedMaterials, newMats, submeshCount);
                            renderer.sharedMaterials = newMats;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                        });
                }
            }

            // === 1.5 OMNI-HARVESTER ===
            if (RenderSettings.skybox != null) { sceneMaterials.Add(RenderSettings.skybox); ScrapeTexturesFromMaterial(RenderSettings.skybox); }
            foreach (var sky in GetCachedObjects<Skybox>(true)) if (sky.material != null) { sceneMaterials.Add(sky.material); ScrapeTexturesFromMaterial(sky.material); }

            foreach (var graphic in GetCachedObjects<UnityEngine.UI.Graphic>(true))
                if (graphic.material != null && graphic.material != graphic.defaultMaterial) { sceneMaterials.Add(graphic.material); ScrapeTexturesFromMaterial(graphic.material); }
            
            foreach (var tmp in GetCachedObjects<TMPro.TMP_Text>(true))
            {
                if (tmp == null) continue;
                
                try 
                {
                    // 4D-Chess: Only attempt to read materials if the font asset physically exists.
                    // This prevents internal TMPro NullReferenceExceptions on corrupted UI elements.
                    if (tmp.font != null)
                    {
                        if (tmp.fontSharedMaterial != null) 
                        { 
                            sceneMaterials.Add(tmp.fontSharedMaterial); 
                            ScrapeTexturesFromMaterial(tmp.fontSharedMaterial); 
                        }
                        
                        if (tmp.fontSharedMaterials != null) 
                        {
                            foreach (var m in tmp.fontSharedMaterials) 
                            {
                                if (m != null) 
                                { 
                                    sceneMaterials.Add(m); 
                                    ScrapeTexturesFromMaterial(m); 
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Explicitly swallow internal TMP crashes on ghost objects
                    // to prevent the matrix scan from halting.
                }
            }

            foreach (var proj in GetCachedObjects<Projector>(true)) if (proj.material != null) { sceneMaterials.Add(proj.material); ScrapeTexturesFromMaterial(proj.material); }
            foreach (var psr in GetCachedObjects<ParticleSystemRenderer>(true)) if (psr.trailMaterial != null) { sceneMaterials.Add(psr.trailMaterial); ScrapeTexturesFromMaterial(psr.trailMaterial); }
            foreach (var light in GetCachedObjects<Light>(true)) if (light.cookie != null) _detectedTextures.Add(light.cookie);
            foreach (var probe in GetCachedObjects<ReflectionProbe>(true)) if (probe.customBakedTexture != null) _detectedTextures.Add(probe.customBakedTexture);

            foreach (var terrain in GetCachedObjects<Terrain>(true))
            {
                if (terrain.materialTemplate != null) { sceneMaterials.Add(terrain.materialTemplate); ScrapeTexturesFromMaterial(terrain.materialTemplate); }
                if (terrain.terrainData != null)
                {
                    foreach (var layer in terrain.terrainData.terrainLayers)
                    {
                        if (layer != null)
                        {
                            if (layer.diffuseTexture != null) _detectedTextures.Add(layer.diffuseTexture);
                            if (layer.normalMapTexture != null) _detectedTextures.Add(layer.normalMapTexture);
                            if (layer.maskMapTexture != null) _detectedTextures.Add(layer.maskMapTexture);
                        }
                    }
                }
            }

            foreach (var udon in GetCachedObjects<VRC.Udon.UdonBehaviour>(true))
            {
                if (udon.publicVariables != null)
                {
                    foreach (var symbol in udon.publicVariables.VariableSymbols)
                    {
                        if (udon.publicVariables.TryGetVariableValue(symbol, out object val))
                        {
                            if (val is Material m && m != null) { sceneMaterials.Add(m); ScrapeTexturesFromMaterial(m); }
                            else if (val is Material[] mats && mats != null) foreach (var mat in mats) if (mat != null) { sceneMaterials.Add(mat); ScrapeTexturesFromMaterial(mat); }
                            else if (val is Texture tex && tex != null) _detectedTextures.Add(tex);
                            else if (val is Texture[] texs && texs != null) foreach (var t in texs) if (t != null) _detectedTextures.Add(t);
                        }
                    }
                }
            }

            var allBehaviours = GetCachedObjects<MonoBehaviour>(true);
            var monoFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            Dictionary<Type, System.Reflection.FieldInfo[]> fieldCache = new Dictionary<Type, System.Reflection.FieldInfo[]>();
            
            foreach (var b in allBehaviours)
            {
                if (b == null) continue;
                Type t = b.GetType();
                
                if (t.Namespace != null && (t.Namespace.StartsWith("UnityEngine") || t.Namespace.StartsWith("UnityEditor") || t.Namespace.StartsWith("VRC.SDKBase"))) continue;

                if (!fieldCache.TryGetValue(t, out var fields))
                {
                    fields = t.GetFields(monoFlags);
                    fieldCache[t] = fields;
                }

                foreach (var f in fields)
                {
                    if (f.FieldType == typeof(Material))
                    {
                        var m = f.GetValue(b) as Material;
                        if (m != null) { sceneMaterials.Add(m); ScrapeTexturesFromMaterial(m); }
                    }
                    else if (f.FieldType == typeof(Material[]))
                    {
                        var mats = f.GetValue(b) as Material[];
                        if (mats != null) foreach(var m in mats) if (m != null) { sceneMaterials.Add(m); ScrapeTexturesFromMaterial(m); }
                    }
                    else if (typeof(Texture).IsAssignableFrom(f.FieldType)) 
                    {
                        var tex = f.GetValue(b) as Texture;
                        if (tex != null) _detectedTextures.Add(tex);
                    }
                    else if (f.FieldType.IsArray && typeof(Texture).IsAssignableFrom(f.FieldType.GetElementType()))
                    {
                        var texs = f.GetValue(b) as Texture[];
                        if (texs != null) foreach(var tex in texs) if (tex != null) _detectedTextures.Add(tex);
                    }
                    else if (f.FieldType == typeof(Sprite))
                    {
                        var spr = f.GetValue(b) as Sprite;
                        if (spr != null && spr.texture != null) _detectedTextures.Add(spr.texture);
                    }
                    else if (f.FieldType.IsArray && f.FieldType.GetElementType() == typeof(Sprite))
                    {
                        var sprs = f.GetValue(b) as Sprite[];
                        if (sprs != null) foreach(var s in sprs) if (s != null && s.texture != null) _detectedTextures.Add(s.texture);
                    }
                }
            }

            // === 2. SCRIPT ASSET SCRAPER ===
            Type txlScreenMgrType = GetTypeSafe("Texel.ScreenManager");
            if (txlScreenMgrType != null)
            {
                foreach (var sm in GetCachedObjects(txlScreenMgrType, true))
                {
                    string[] matFields = { "playbackMaterial", "logoMaterial", "loadingMaterial", "syncMaterial", "audioMaterial", "errorMaterial", "errorInvalidMaterial", "errorRateLimitedMaterial", "errorBlockedMaterial", "vrslBlitMat" };
                    foreach (var field in matFields)
                    {
                        var mInfo = txlScreenMgrType.GetField(field, monoFlags);
                        if (mInfo != null && mInfo.GetValue(sm) is Material m && m != null) { sceneMaterials.Add(m); ScrapeTexturesFromMaterial(m); }
                    }

                    string[] texFields = { "logoTexture", "loadingTexture", "syncTexture", "audioTexture", "errorTexture", "errorInvalidTexture", "errorRateLimitedTexture", "errorBlockedTexture", "editorTexture" };
                    foreach (var field in texFields)
                    {
                        var tInfo = txlScreenMgrType.GetField(field, monoFlags);
                        if (tInfo != null && tInfo.GetValue(sm) is Texture t && t != null) _detectedTextures.Add(t);
                    }
                }
            }

            if (proTvType != null)
            {
                foreach (var tv in GetCachedObjects(proTvType, true))
                {
                    var customTexField = proTvType.GetField("customTexture", monoFlags);
                    if (customTexField != null && customTexField.GetValue(tv) is Texture t && t != null) _detectedTextures.Add(t);
                }
            }
            
            Type proTvPlaylistType = GetTypeSafe("ArchiTech.ProTV.PlaylistData");
            if (proTvPlaylistType != null)
            {
                foreach (var pl in GetCachedObjects(proTvPlaylistType, true))
                {
                    var imagesField = proTvPlaylistType.GetField("images", monoFlags);
                    if (imagesField != null && imagesField.GetValue(pl) is Sprite[] sprites)
                    {
                        foreach (var s in sprites)
                        {
                            if (s != null && s.texture != null) _detectedTextures.Add(s.texture);
                        }
                    }
                }
            }

            if (audioLinkType != null)
            {
                foreach (var al in GetCachedObjects(audioLinkType, true))
                {
                    var matField = audioLinkType.GetField("audioMaterial", monoFlags);
                    if (matField != null && matField.GetValue(al) is Material m && m != null) { sceneMaterials.Add(m); ScrapeTexturesFromMaterial(m); }

                    var rtField = audioLinkType.GetField("audioData", monoFlags);
                    if (rtField != null && rtField.GetValue(al) is Texture t && t != null) _detectedTextures.Add(t);
                    
                    var tex2DField = audioLinkType.GetField("audioData2D", monoFlags);
                    if (tex2DField != null && tex2DField.GetValue(al) is Texture t2 && t2 != null) _detectedTextures.Add(t2);
                }
            }

            // === 3. IMPORTER LEAKS & COMPRESSION (CACHE GUARDED) ===
            bool cacheUpdatedDuringScan = false;

            foreach (var mesh in _detectedMeshes)
            {
                if (mesh == null) continue;

                string meshPath = AssetDatabase.GetAssetPath(mesh);
                if (string.IsNullOrEmpty(meshPath) || (!meshPath.StartsWith("Assets") && !meshPath.StartsWith("Packages"))) continue;

                string guid = AssetDatabase.AssetPathToGUID(meshPath);
                
                if (!ShouldProcessMeshAsset(guid, meshPath)) continue;

                bool isCompliant = true;

                if (mesh.vertexCount > 10000) 
                {
                    LogDiagnostic("MESHES & GEOMETRY", "High Poly Counts", $"'{mesh.name}' has {mesh.vertexCount} vertices.", "#ff00aa", mesh);
                    isCompliant = false; 
                }

                string ext = System.IO.Path.GetExtension(meshPath).ToLowerInvariant();
                
                if (ext == ".fbx" || ext == ".obj" || ext == ".dae" || ext == ".blend")
                {
                    ModelImporter imp = AssetImporter.GetAtPath(meshPath) as ModelImporter;
                    if (imp != null) 
                    {
                        if (mesh.isReadable)
                        {
                            isCompliant = false;
                            LogDiagnostic("MESHES & GEOMETRY", "Read/Write Enabled", 
                                $"'{mesh.name}' keeps a duplicate copy in CPU RAM. Disable this unless accessed by C#/Udon scripts.", 
                                "#ffaa00", mesh, () => {
                                    imp.isReadable = false; 
                                    imp.SaveAndReimport(); 
                                    RecordMeshResult(guid, meshPath, true);
                            });
                        }

                        if (imp.meshCompression == ModelImporterMeshCompression.Off && mesh.vertexCount > 1000)
                        {
                            isCompliant = false;
                            LogDiagnostic("MESHES & GEOMETRY", "Uncompressed Mesh", 
                                $"'{mesh.name}' has no mesh compression. Applying 'Low' compression significantly reduces VRAM.", 
                                "#00e5ff", mesh, () => {
                                    imp.meshCompression = ModelImporterMeshCompression.Low;
                                    imp.SaveAndReimport();
                                    RecordMeshResult(guid, meshPath, true);
                            });
                        }
                    }
                }

                if (isCompliant)
                {
                    RecordMeshResult(guid, meshPath, true);
                    cacheUpdatedDuringScan = true;
                }
            }
            if (cacheUpdatedDuringScan) SaveLookupCache();

            // === 4. MATERIAL PROTECTION & SHADER COMPLIANCE ===
            foreach (var mat in sceneMaterials)
            {
                string shaderName = mat.shader != null ? mat.shader.name : "Missing Shader";
                string materialName = mat.name;
                bool isMissingOrInvalid = mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader";
                
                if (isMissingOrInvalid)
                {
                    LogDiagnostic("SHADER PIPELINE & REPLACER", "Invalid/Missing Shader (Magenta)", $"'{mat.name}' is broken. Ready to swap to target.", "#ff00aa", mat, () => {
                        if (_targetReplacementShader != null)
                        {
                            Undo.RecordObject(mat, "Replace Invalid Shader");
                            mat.shader = _targetReplacementShader;
                            EditorUtility.SetDirty(mat);
                        }
                    });
                }
                else
                {
                    // === A. SHADER ENFORCER (WHITELIST & REPLACER) ===
                    bool isInternalPluginShader = shaderName.IndexOf("AudioLink", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                                  shaderName.IndexOf("VideoTXL", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                                  shaderName.IndexOf("ProTV", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                                  shaderName.IndexOf("AVPro", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                  shaderName.IndexOf("JLChnToZ", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                  shaderName.IndexOf("VVMW", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                  shaderName.IndexOf("UI/Default", StringComparison.OrdinalIgnoreCase) >= 0;

                    bool isProtectedMaterialName = materialName.IndexOf("VideoTXL", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                   materialName.IndexOf("ProTV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                   materialName.IndexOf("VideoSurface", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                                   materialName.IndexOf("IwaSync", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                   (materialName.IndexOf("Screen", StringComparison.OrdinalIgnoreCase) >= 0 && iwaSyncType != null); 

                    if (!isInternalPluginShader && !isProtectedMaterialName)
                    {
                        if (_targetReplacementShader != null && mat.shader != _targetReplacementShader && !ShaderDictionaryAsset.IsGloballyProtected(mat.shader)) 
                        {
                            bool isWhitelisted = false;
                            if (_shaderWhitelistAsset != null && _shaderWhitelistAsset.shaders != null)
                            {
                                if (_shaderWhitelistAsset.shaders.Contains(mat.shader)) isWhitelisted = true;
                            }

                            if (!isWhitelisted)
                            {
                                LogDiagnostic("SHADER PIPELINE & REPLACER", "Non-Whitelisted Shader", $"'{mat.name}' uses '{shaderName}'. Ready to convert.", "#ffaa00", mat, () => {
                                    if (_targetReplacementShader != null)
                                    {
                                        Undo.RecordObject(mat, "Replace Shader");
                                        mat.shader = _targetReplacementShader;
                                        EditorUtility.SetDirty(mat);
                                    }
                                });
                            }
                        }
                    }

                    // === B. GLOBAL MATERIAL OPTIMIZATION ===
                    if (shaderName.IndexOf("Poiyomi", StringComparison.OrdinalIgnoreCase) >= 0 || 
                        shaderName.IndexOf("lilToon", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (shaderName.IndexOf("Locked", StringComparison.OrdinalIgnoreCase) < 0 && 
                            shaderName.IndexOf("Optimized", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            LogDiagnostic("MATERIAL SECURITY & COMPUTE", "Unlocked Toon Shader", 
                                $"'{mat.name}' is using an unlocked {shaderName}. Unlocked materials leave hundreds of properties exposed to the CPU because Unity assumes they might be modified at runtime. Lock this material in its inspector to bake it.", 
                                "#ff00aa", mat);
                        }
                    }

                    if (!mat.enableInstancing && !isProtectedMaterialName)
                    {
                        LogDiagnostic("MATERIAL OPTIMIZATION", "GPU Instancing Disabled", 
                            $"'{mat.name}' has GPU Instancing disabled. Instancing protects CPU threads by allowing Unity to render multiple identical meshes in a single draw call. Enable this for environmental materials.", 
                            "#00e5ff", mat, () => {
                                Undo.RecordObject(mat, "Enable GPU Instancing");
                                mat.enableInstancing = true;
                                EditorUtility.SetDirty(mat);
                            });
                    }
                }
                
                AttemptTextureRecovery(mat);
            }
        }

        private void ScrapeTexturesFromMaterial(Material mat)
        {
            if (mat == null) return;

            string[] texNames = mat.GetTexturePropertyNames();
            foreach (string propName in texNames)
            {
                Texture tex = mat.GetTexture(propName);
                if (tex != null)
                {
                    _detectedTextures.Add(tex);
                }
            }
        }

        private void AnalyzeTextures()
        {
            _processedTexturePaths.Clear();
            bool cacheUpdatedDuringScan = false;

            foreach (var tex in _detectedTextures)
            {
                if (tex == null || tex is RenderTexture) continue;

                string path = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(path) || (!path.StartsWith("Assets", StringComparison.OrdinalIgnoreCase) && !path.StartsWith("Packages", StringComparison.OrdinalIgnoreCase))) continue;

                string guid = AssetDatabase.AssetPathToGUID(path);

                if (!ShouldProcessTextureAsset(guid, path)) continue;

                if (!_processedTexturePaths.Add(path)) continue;

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool isCompliant = true;

                if (importer.isReadable)
                {
                    isCompliant = false;
                    LogDiagnostic("TEXTURES & VRAM", "Read/Write Enabled",
                        $"'{tex.name}' has Read/Write enabled, keeping a duplicate copy of the texture in CPU RAM.",
                        "#ffaa00", tex, () =>
                        {
                            Undo.RecordObject(importer, "Disable Read/Write");
                            importer.isReadable = false;
                            importer.SaveAndReimport();
                            RecordTextureResult(guid, path, true);
                        });
                }

                if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    isCompliant = false;
                    LogDiagnostic("TEXTURES & VRAM", "Uncompressed Texture",
                        $"'{tex.name}' is fully uncompressed.",
                        "#00e5ff", tex, () =>
                        {
                            Undo.RecordObject(importer, "Compress Texture");
                            importer.textureCompression = TextureImporterCompression.Compressed;
                            importer.crunchedCompression = true;
                            importer.compressionQuality = 75;
                            importer.SaveAndReimport();
                            RecordTextureResult(guid, path, true);
                        });
                }

                bool isUI = importer.textureType == TextureImporterType.Sprite || importer.textureType == TextureImporterType.GUI || importer.textureType == TextureImporterType.Cursor;

                if (!isUI && (!Mathf.IsPowerOfTwo(tex.width) || !Mathf.IsPowerOfTwo(tex.height)))
                {
                    if (importer.npotScale == TextureImporterNPOTScale.None)
                    {
                        isCompliant = false;
                        LogDiagnostic("TEXTURES & VRAM", "Non-Power of 2 Source",
                            $"'{tex.name}' is {tex.width}x{tex.height}. Unity can scale this safely.",
                            "#ff00aa", tex, () =>
                            {
                                Undo.RecordObject(importer, "Scale to Nearest Power of 2");
                                importer.npotScale = TextureImporterNPOTScale.ToNearest;
                                importer.SaveAndReimport();
                                RecordTextureResult(guid, path, true);
                            });
                    }
                }
                
                importer.GetSourceTextureWidthAndHeight(out int srcWidth, out int srcHeight);
                int targetMax = Mathf.Clamp(_targetTextureResolution, 32, 16384);

                if (srcWidth > targetMax || srcHeight > targetMax)
                {
                    isCompliant = false;
                    TextureImporter importerLocal = importer;
                    string fullPathForResize = System.IO.Path.GetFullPath(path);

                    LogDiagnostic("TEXTURES & VRAM", $"{targetMax}+ Texture Nuke",
                        $"'{tex.name}' is {srcWidth}x{srcHeight}. Physically crushing to {targetMax}.",
                        "#ff00aa", tex, () =>
                        {
                            EnqueueWork(() => {
                                bool success = ResizeTextureWithMagick(fullPathForResize, path, targetMax, targetMax);
                                if (success) {
                                    Undo.RecordObject(importerLocal, "Clamp Texture Max Size");
                                    importerLocal.maxTextureSize = targetMax;
                                    importerLocal.SaveAndReimport();
                                }
                                RecordTextureResult(guid, path, success);
                            });
                        });
                    continue;
                }

                string fullPath = System.IO.Path.GetFullPath(path);
                if (System.IO.File.Exists(fullPath))
                {
                    string ext = System.IO.Path.GetExtension(fullPath).ToLowerInvariant();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".tif" || ext == ".tiff")
                    {
                        long fileBytes = new System.IO.FileInfo(fullPath).Length;
                        if (fileBytes > 15 * 1024 * 1024)
                        {
                            isCompliant = false;
                            LogDiagnostic("TEXTURES & VRAM", "Massive Raw File Bloat",
                                $"'{tex.name}' is {fileBytes / 1048576f:F1} MB on disk. Stripping metadata.",
                                "#ff00aa", tex, () =>
                                {
                                    EnqueueWork(() => {
                                        bool success = OptimizeTextureWithMagick(fullPath, path);
                                        RecordTextureResult(guid, path, success);
                                    });
                                });
                        }
                    }
                }

                if (isCompliant)
                {
                    RecordTextureResult(guid, path, true);
                    cacheUpdatedDuringScan = true;
                }
            }
            
            if (cacheUpdatedDuringScan) SaveLookupCache();
        }

        // ==========================================
        // IMAGEMAGICK DEPLOYMENT PROTOCOLS
        // ==========================================

        private bool ResizeTextureWithMagick(string fullPath, string assetPath, int maxWidth, int maxHeight)
        {
            try
            {
                using (var img = new MagickImage(fullPath))
                {
                    img.FilterType = FilterType.Lanczos;
                    img.Resize(new MagickGeometry((uint)maxWidth, (uint)maxHeight)
                    {
                        IgnoreAspectRatio = false,
                        Greater = true
                    });

                    img.Strip();
                    img.Write(fullPath);
                }

                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Vixen World Engine] Magick resize failed for '{assetPath}': {ex.Message}");
                return false;
            }
        }

        private bool OptimizeTextureWithMagick(string fullPath, string assetPath)
        {
            try
            {
                using (var img = new MagickImage(fullPath))
                {
                    img.Strip();

                    if (img.Format == MagickFormat.Png) img.Quality = 90;
                    else img.Quality = 85;

                    img.Write(fullPath);
                }

                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Vixen World Engine] Magick optimize failed for '{assetPath}': {ex.Message}");
                return false;
            }
        }

        private void AuditCanvasesAndUIMemory()
        {
            var canvases = GetCachedObjects<Canvas>(true);
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    var channels = canvas.additionalShaderChannels;
                    bool hasTexCoord1 = (channels & AdditionalCanvasShaderChannels.TexCoord1) != 0;
                    bool hasNormal = (channels & AdditionalCanvasShaderChannels.Normal) != 0;
                    bool hasTangent = (channels & AdditionalCanvasShaderChannels.Tangent) != 0;

                    if (!hasTexCoord1 || !hasNormal || !hasTangent)
                    {
                        LogDiagnostic("UI & CANVAS OPTIMIZATION", "Missing TMP Shader Channels", $"'{canvas.name}' is missing VRChat TMP channels.", "#ffaa00", canvas, () => {
                            Undo.RecordObject(canvas, "Fix Canvas Shader Channels");
                            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
                        });
                    }

                    if (canvas.GetComponent<VRC.SDKBase.VRC_UiShape>() == null && canvas.GetComponent<VRC.SDK3.Components.VRCUiShape>() == null)
                    {
                         LogDiagnostic("UI & CANVAS OPTIMIZATION", "Missing VRC UI Shape", $"'{canvas.name}' is World Space but lacks a VRCUiShape. VRChat laser pointers will ignore it.", "#ff00aa", canvas, () => {
                             Undo.AddComponent<VRC.SDK3.Components.VRCUiShape>(canvas.gameObject);
                         });
                    }
                }

                if ((canvas.renderMode == RenderMode.WorldSpace || canvas.renderMode == RenderMode.ScreenSpaceCamera) && canvas.worldCamera == null)
                {
                    var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    if (raycaster != null || canvas.renderMode == RenderMode.ScreenSpaceCamera) 
                    {
                        string issueLevel = canvas.renderMode == RenderMode.ScreenSpaceCamera ? "#ff00aa" : "#ffaa00";
                        string issueDesc = canvas.renderMode == RenderMode.ScreenSpaceCamera 
                            ? $"'{canvas.name}' is Screen Space - Camera but has no Camera assigned. It will fail to render."
                            : $"'{canvas.name}' is World Space but lacks an Event Camera. Unity will throw continuous warnings and UI events may fail.";

                        LogDiagnostic("UI & CANVAS OPTIMIZATION", "Missing Event Camera", issueDesc, issueLevel, canvas, () => {
                            Undo.RecordObject(canvas, "Assign Event Camera");
                            Camera eventCam = GameObject.Find("Vixen UI Event Camera")?.GetComponent<Camera>();
                            if (eventCam == null) 
                            {
                                GameObject camObj = new GameObject("Vixen UI Event Camera");
                                eventCam = camObj.AddComponent<Camera>();
                                eventCam.clearFlags = CameraClearFlags.Nothing;
                                eventCam.cullingMask = 0; 
                                eventCam.useOcclusionCulling = false;
                                eventCam.stereoTargetEye = StereoTargetEyeMask.None; 
                                eventCam.enabled = false; 
                                Undo.RegisterCreatedObjectUndo(camObj, "Create UI Event Camera");
                            }
                            canvas.worldCamera = eventCam;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
                        });
                    }
                }
            }

            foreach (var img in GetCachedObjects<UnityEngine.UI.Image>(true))
                if (img.sprite != null && img.sprite.texture != null) _detectedUITextures.Add(img.sprite.texture);
                
            foreach (var raw in GetCachedObjects<UnityEngine.UI.RawImage>(true))
                if (raw.texture != null) _detectedUITextures.Add(raw.texture);
                
            foreach (var txt in GetCachedObjects<TMP_Text>(true))
                if (txt.font != null && txt.font.material != null && txt.font.material.mainTexture != null) 
                    _detectedUITextures.Add(txt.font.material.mainTexture);
                    
            foreach (var legacyTxt in GetCachedObjects<UnityEngine.UI.Text>(true))
                if (legacyTxt.font != null && legacyTxt.font.material != null && legacyTxt.font.material.mainTexture != null) 
                    _detectedUITextures.Add(legacyTxt.font.material.mainTexture);
        }

        private void AuditExplicitTextComponents()
        {
            if (_targetTMPFont != null)
            {
                var text3DComps = GetCachedObjects<TextMeshPro>(true); 
                foreach (var txt3D in text3DComps)
                {
                    if (txt3D.font != _targetTMPFont)
                    {
                        LogDiagnostic("3D WORLD TEXT (TMP)", "Legacy/Mismatched Font Asset", $"'{txt3D.name}' uses '{txt3D.font?.name}'.", "#ffaa00", txt3D, () => {
                            Undo.RecordObject(txt3D, "Update 3D TMP Font");
                            txt3D.font = _targetTMPFont;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(txt3D);
                        });
                    }
                }

                var textUIComps = GetCachedObjects<TextMeshProUGUI>(true);
                foreach (var txtUI in textUIComps)
                {
                    if (txtUI.font != _targetTMPFont)
                    {
                        LogDiagnostic("UI CANVAS TEXT (TMP)", "Legacy/Mismatched Font Asset", $"'{txtUI.name}' uses '{txtUI.font?.name}'.", "#00e5ff", txtUI, () => {
                            Undo.RecordObject(txtUI, "Update Canvas TMP Font");
                            txtUI.font = _targetTMPFont;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(txtUI);
                        });
                    }
                }
            }

            if (_targetLegacyFont != null)
            {
                var legacyTextComps = GetCachedObjects<UnityEngine.UI.Text>(true);
                foreach (var txtLegacy in legacyTextComps)
                {
                    if (txtLegacy.font != _targetLegacyFont)
                    {
                        LogDiagnostic("UI CANVAS TEXT (LEGACY)", "Mismatched Legacy Font", $"'{txtLegacy.name}' uses '{txtLegacy.font?.name}'.", "#00e5ff", txtLegacy, () => {
                            Undo.RecordObject(txtLegacy, "Update Legacy Font");
                            txtLegacy.font = _targetLegacyFont;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(txtLegacy);
                        });
                    }
                }
            }
        }

        private void AuditAudio()
        {
            foreach (var source in GetCachedObjects<AudioSource>(true))
                if (source.clip != null)
                {
                    _detectedAudio.Add(source.clip);
                    if (source.spatialBlend < 1f) LogDiagnostic("AUDIO OPTIMIZATION", "Global 2D Audio", $"'{source.name}' is not spatialized.", "#ffaa00", source.gameObject);
                }

            foreach (var clip in _detectedAudio)
            {
                string path = AssetDatabase.GetAssetPath(clip);
                if (string.IsNullOrEmpty(path)) continue;
                AudioImporter imp = AssetImporter.GetAtPath(path) as AudioImporter;
                if (imp == null) continue;

                if (!imp.forceToMono)
                    LogDiagnostic("AUDIO OPTIMIZATION", "Stereo Files", $"'{clip.name}' taking double RAM. Force Mono?", "#00e5ff", clip, () => {
                        imp.forceToMono = true;
                        imp.SaveAndReimport();
                    });
                
                if (imp.defaultSampleSettings.loadType == AudioClipLoadType.DecompressOnLoad && clip.length > 5f)
                    LogDiagnostic("AUDIO OPTIMIZATION", "Decompress On Load", $"'{clip.name}' causes loading lag.", "#ff00aa", clip, () => {
                        var settings = imp.defaultSampleSettings;
                        settings.loadType = AudioClipLoadType.CompressedInMemory;
                        imp.defaultSampleSettings = settings;
                        imp.SaveAndReimport();
                    });
            }
        }

        private void AuditUdonPersistence()
        {
            Type playerObjectType = GetTypeSafe("VRC.SDK3.Persistence.VRCPlayerObject");
            if (playerObjectType != null)
            {
                var playerObjects = GetCachedObjects(playerObjectType);
                foreach (var po in playerObjects)
                {
                    var component = (Component)po;
                    var objSync = component.GetComponent<VRC.SDK3.Components.VRCObjectSync>();
                    var udon = component.GetComponent<UdonBehaviour>();

                    if (objSync == null && udon == null)
                    {
                        LogDiagnostic("UDON PERSISTENCE", "Empty Player Object", 
                            $"'{component.gameObject.name}' has a VRCPlayerObject component but no VRCObjectSync or UdonBehaviour. It will not persist any data and serves no purpose.", 
                            "#ff00aa", component.gameObject);
                    }

                    if (udon != null && udon.SyncMethod == VRC.SDKBase.Networking.SyncType.Continuous)
                    {
                        LogDiagnostic("UDON PERSISTENCE", "Continuous Player Object Sync", 
                            $"'{component.gameObject.name}' is a Player Object set to Continuous sync. This will cause extreme bandwidth bloat when instances scale. Set to Manual unless physical transform syncing is strictly required.", 
                            "#ffaa00", component.gameObject, () => {
                                Undo.RecordObject(udon, "Set PlayerObject to Manual Sync");
                                udon.SyncMethod = VRC.SDKBase.Networking.SyncType.Manual;
                                PrefabUtility.RecordPrefabInstancePropertyModifications(udon);
                            });
                    }
                }
            }

            Assembly editorAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "UdonSharp.Editor");
            Type cacheType = editorAsm?.GetType("UdonSharp.UdonSharpEditorCache");
            var cache = cacheType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
            var getUasm = cacheType?.GetMethod("GetUASMStr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (getUasm != null && cache != null)
            {
                HashSet<UdonSharpProgramAsset> checkedAssets = new HashSet<UdonSharpProgramAsset>();
                var udonBehaviours = GetCachedObjects<UdonBehaviour>(true);

                foreach (var udon in udonBehaviours)
                {
                    if (udon.programSource is UdonSharpProgramAsset uAsset && !checkedAssets.Contains(uAsset))
                    {
                        checkedAssets.Add(uAsset);
                        string uasm = (string)getUasm.Invoke(cache, new object[] { uAsset });
                        
                        if (!string.IsNullOrEmpty(uasm))
                        {
                            bool usesPlayerData = uasm.Contains("Persistence") && uasm.Contains("PlayerData");
                            bool hasUpdate = uasm.Contains("_update") || uasm.Contains("_lateUpdate") || uasm.Contains("_fixedUpdate");
                            bool usesOnPlayerDataUpdated = uasm.Contains("_onPlayerDataUpdated");
                            bool usesSet = uasm.Contains("PlayerData.__Set");
                            bool usesGet = uasm.Contains("PlayerData.__Get");

                            if (usesPlayerData)
                            {
                                if (usesSet && hasUpdate)
                                {
                                    LogDiagnostic("UDON PERSISTENCE", "PlayerData in Update Loop", 
                                        $"'{uAsset.name}' executes PlayerData.Set() alongside an Update loop. Writing to persistence every frame will instantly trigger VRChat's rate limits and cause total data loss for the player. Refactor to only save on discrete state changes.", 
                                        "#ff00aa", udon.gameObject);
                                }
                                
                                if (usesGet && !usesOnPlayerDataUpdated) 
                                {
                                    LogDiagnostic("UDON PERSISTENCE", "Unmonitored PlayerData", 
                                        $"'{uAsset.name}' reads PlayerData but doesn't implement OnPlayerDataUpdated. VRChat cloud data can take several seconds to load after joining. This script will likely read null/default data and permanently desync.", 
                                        "#00e5ff", udon.gameObject);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ExecuteSelectedProtocols()
        {
            var actionableDiagnostics = _diagnosticsDb.Where(d => d.IsSelected && d.FixPayload != null).ToList();

            if (actionableDiagnostics.Count == 0)
            {
                EditorUtility.DisplayDialog("VIXEN SYSTEM", "No fixes selected or available.", "OK");
                return;
            }

            if (actionableDiagnostics.Any(d => d.Category == "SHADER PIPELINE & REPLACER") && _targetReplacementShader == null)
            {
                 if (!EditorUtility.DisplayDialog("VIXEN SYSTEM WARNING", "You have selected shaders to replace, but have not set a REPLACEMENT SHADER TARGET. \n\nContinue anyway (skipping shader swaps)?", "CONTINUE", "ABORT")) return;
            }
            else if (!EditorUtility.DisplayDialog("VIXEN ENFORCEMENT", $"Applying {actionableDiagnostics.Count} specific fixes. This may take a moment to reimport assets.\n\nExecute Protocol?", "EXECUTE", "ABORT")) return;

            try
            {
                foreach (var diag in actionableDiagnostics)
                {
                    try 
                    {
                        diag.FixPayload.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Vixen System] Fix execution failed for '{diag.IssueType}': {ex.Message}");
                    }
                    
                    diag.FixPayload = null; // Prevent double execution
                    diag.IsSelected = false;
                    diag.OnFixedUIUpdate?.Invoke(); // Keeps visual continuity during the split-second before the rescan hits
                }
            }
            finally
            {
                // Persist all standard Unity structural changes (Materials, Prefabs, Shaders, etc.)
                AssetDatabase.SaveAssets(); 
                
                // 4D-Chess: Check if the fixes generated heavy I/O operations (ImageMagick)
                if (_workQueue.Count > 0)
                {
                    // Offload to the background thread to prevent Editor freezing.
                    // ProcessQueueTick() will handle the Refresh(), SaveLookupCache(), and InitiateFullMatrixScan() when finished.
                    StartProcessingQueue();
                }
                else
                {
                    // If there was no heavy IO, execute the Live Refresh instantly
                    AssetDatabase.Refresh();
                    
                    // Force a save to the JSON database just in case any instant-fixes modified the cache
                    SaveLookupCache();
                    
                    InitiateFullMatrixScan();
                    
                    EditorUtility.DisplayDialog("VIXEN SYSTEM", "Targeted purges complete. Matrix updated.", "ACKNOWLEDGE");
                }
            }
        }

        private class ShaderDropdownItem : AdvancedDropdownItem
        {
            public string fullShaderName;
            public ShaderDropdownItem(string name, string fullName) : base(name)
            {
                fullShaderName = fullName;
            }
        }

        private class ShaderSelectionDropdown : AdvancedDropdown
        {
            private List<string> _validShaders;
            private Action<string> _onItemSelected;

            public ShaderSelectionDropdown(AdvancedDropdownState state, List<string> validShaders, Action<string> onItemSelected) : base(state)
            {
                _validShaders = validShaders;
                _onItemSelected = onItemSelected;
                this.minimumSize = new Vector2(300, 400);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Valid PBR / Toon Shaders");

                foreach (var shaderName in _validShaders)
                {
                    var splits = shaderName.Split('/');
                    var currentParent = root;

                    for (int i = 0; i < splits.Length; i++)
                    {
                        bool isLeaf = (i == splits.Length - 1);
                        string part = splits[i];

                        var existingChild = currentParent.children.FirstOrDefault(c => c.name == part);
                        
                        if (existingChild == null)
                        {
                            var newItem = isLeaf ? new ShaderDropdownItem(part, shaderName) : new AdvancedDropdownItem(part);
                            currentParent.AddChild(newItem);
                            currentParent = newItem;
                        }
                        else
                        {
                            currentParent = existingChild;
                        }
                    }
                }
                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item is ShaderDropdownItem shaderItem)
                {
                    _onItemSelected?.Invoke(shaderItem.fullShaderName);
                }
            }
        }
    }
}
#endif