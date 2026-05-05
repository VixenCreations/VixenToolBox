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

namespace VixenTools.Editor
{
    public class VixenWorldEngine : EditorWindow
    {
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenWorldSpider.uss";
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        
        private const string TargetDictPath = "Packages/com.vixencreations.vixens-toolbox/Editor/Scene Tools/World Engine/VixenReplacementTargets.asset";
        private const string WhitelistDictPath = "Packages/com.vixencreations.vixens-toolbox/Editor/Scene Tools/World Engine/VixenShaderWhitelist.asset";

        private ScrollView _mainScroll;
        private VisualElement _dashboardContainer;
        private VisualElement _matrixContainer;
        private Font _cyberFont;

        private HashSet<Texture> _detectedTextures = new HashSet<Texture>();
        private HashSet<AudioClip> _detectedAudio = new HashSet<AudioClip>();
        private HashSet<Mesh> _detectedMeshes = new HashSet<Mesh>();
        private HashSet<Texture> _detectedUITextures = new HashSet<Texture>();

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

        private class EngineDiagnostic
        {
            public string Category;
            public string IssueType;
            public string Description;
            public string HexColor;
            public UnityEngine.Object Context;
            public bool IsSelected = false; // FIX 1: Default to unchecked
            public Action FixPayload; 
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

            // Texture Resolution
            var resRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
            var resLabel = new Label("TARGET MAX TEXTURE SIZE:");
            resLabel.AddToClassList("control-label"); 
            var resDropdown = new DropdownField(_resolutionOptions, 2);
            resDropdown.AddToClassList("vixen-dropdown");
            resDropdown.RegisterValueChangedCallback(evt => {
                if (int.TryParse(evt.newValue, out int res)) _targetTextureResolution = res;
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

            _dashboardContainer = new VisualElement();
            _mainScroll.Add(_dashboardContainer);

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

        private void InitiateFullMatrixScan()
        {
            EnsureDictionariesExist();
            RefreshCustomDropdown();

            _diagnosticsDb.Clear();
            _detectedTextures.Clear(); 
            _detectedAudio.Clear(); 
            _detectedMeshes.Clear();
            _detectedUITextures.Clear();

            AuditUdonAndNetwork();
            AuditLightingAndCameras();
            AuditPhysics();
            AuditGeometryAndMaterials(); 
            AnalyzeTextures();
            AuditExplicitTextComponents(); 
            AuditCanvasesAndUIMemory();
            
            // --- NEW: Video Pipeline Catch ---
            AuditNativeVideoPipelines(); 
            
            AuditProTVEcosystem(); 
            AuditTxlEcosystem(); 
            AuditIwaSyncEcosystem();
            AuditUdonPersistence(); 

            RenderHeuristicsDashboard();
            RenderDiagnosticMatrix();
        }

        private void RenderHeuristicsDashboard()
        {
            _dashboardContainer.Clear();
            
            long texBytes = _detectedTextures.Sum(t => t != null ? Profiler.GetRuntimeMemorySizeLong(t) : 0);
            long meshBytes = _detectedMeshes.Sum(m => m != null ? Profiler.GetRuntimeMemorySizeLong(m) : 0);
            long audioBytes = _detectedAudio.Sum(a => a != null ? Profiler.GetRuntimeMemorySizeLong(a) : 0);
            long uiBytes = _detectedUITextures.Sum(t => t != null ? Profiler.GetRuntimeMemorySizeLong(t) : 0);
            
            float texMB = texBytes / 1048576f;
            float meshMB = meshBytes / 1048576f;
            float audioMB = audioBytes / 1048576f;
            float uiMB = uiBytes / 1048576f;
            float totalVramMB = texMB + meshMB + uiMB; 

            var renderers = FindObjectsOfType<Renderer>(true);
            int estDrawCalls = renderers.Sum(r => r.sharedMaterials.Length);
            int rigidbodies = FindObjectsOfType<Rigidbody>(true).Length;
            
            int realtimeShadowCasters = FindObjectsOfType<Light>(true).Count(l => 
                l.lightmapBakeType != LightmapBakeType.Baked && 
                l.shadows != LightShadows.None);
            
            float computeScore = (estDrawCalls * 0.5f) + (rigidbodies * 2.0f) + (realtimeShadowCasters * 80.0f);
            
            string threatLevel = computeScore < 100 ? "<color=#00ff88>OPTIMAL</color>" : 
                                 computeScore < 250 ? "<color=#ffaa00>MODERATE</color>" : 
                                 "<color=#ff00aa>SEVERE</color>";

            var dash = new VisualElement();
            dash.AddToClassList("dashboard-panel");

            dash.Add(new Label("WORLD PROFILER : MEMORY & COMPUTE") { name = "dash-header" });
            
            dash.Add(CreateDashStat("TOTAL ESTIMATED VRAM", $"{totalVramMB:F2} MB", "#00e5ff"));
            dash.Add(CreateDashStat("  ■ TEXTURE MEMORY", $"{texMB:F2} MB", "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ UI/TMP MEMORY", $"{uiMB:F2} MB", "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ MESH GEOMETRY", $"{meshMB:F2} MB", "#e0e0e0"));
            dash.Add(CreateDashStat("AUDIO RAM FOOTPRINT", $"{audioMB:F2} MB", "#ffaa00"));
            
            dash.Add(new VisualElement { style = { height = 1, backgroundColor = new StyleColor(new Color(1,1,1,0.1f)), marginTop = 8, marginBottom = 8 } });
            
            dash.Add(CreateDashStat("ESTIMATED DRAW CALLS", $"{estDrawCalls}", "#e0e0e0"));
            dash.Add(CreateDashStat("RT SHADOW LIGHTS", $"{realtimeShadowCasters}", "#ffaa00"));
            dash.Add(CreateDashStat("COMPUTE THREAT LEVEL", $"{threatLevel}", "#ffffff", true));

            _dashboardContainer.Add(dash);
        }

        private VisualElement CreateDashStat(string title, string value, string hexCol, bool richText = false)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, paddingTop = 2, paddingBottom = 2 } };
            row.Add(new Label(title) { style = { color = new StyleColor(Color.gray), fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } });
            
            var valLabel = new Label(value) { enableRichText = richText, style = { fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } };
            if (!richText) valLabel.style.color = new StyleColor(ColorUtility.TryParseHtmlString(hexCol, out Color c) ? c : Color.white);
            
            row.Add(valLabel);
            return row;
        }

        private void RenderDiagnosticMatrix()
        {
            _matrixContainer.Clear();
            var categories = _diagnosticsDb.Select(d => d.Category).Distinct().OrderBy(c => c);

            foreach (var category in categories)
            {
                var foldout = new Foldout { text = category, value = false };
                foldout.AddToClassList("matrix-foldout");
                
                var contentContainer = new VisualElement();
                contentContainer.AddToClassList("topology-container");

                var catIssues = _diagnosticsDb.Where(d => d.Category == category && d.FixPayload != null).ToList();
                if (catIssues.Count > 0)
                {
                    var toggleAllRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginBottom = 4 } };
                    var toggleAllBtn = new Button(() => {
                        bool anyUnchecked = catIssues.Any(i => !i.IsSelected);
                        foreach (var issue in catIssues) issue.IsSelected = anyUnchecked;
                        RenderDiagnosticMatrix(); 
                    }) { text = "Toggle All Fixes", style = { fontSize = 10, paddingLeft = 10, paddingRight = 10 } };
                    toggleAllRow.Add(toggleAllBtn);
                    contentContainer.Add(toggleAllRow);
                }

                var issuesInCategory = _diagnosticsDb.Where(d => d.Category == category);
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

                        if (issue.FixPayload != null)
                        {
                            var toggle = new Toggle { value = issue.IsSelected };
                            toggle.AddToClassList("vixen-toggle");
                            var currentIssue = issue; 
                            toggle.RegisterValueChangedCallback(e => currentIssue.IsSelected = e.newValue);
                            row.Add(toggle);
                        }
                        else
                        {
                            var spacer = new VisualElement { style = { width = 16, marginRight = 8, marginLeft = 2 } };
                            row.Add(spacer);
                        }

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

                foldout.Add(contentContainer);
                _matrixContainer.Add(foldout);
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

        private void AuditNativeVideoPipelines()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // 1. AVPro Check
            Type avProType = GetTypeSafe("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer");
            if (avProType != null)
            {
                foreach (var player in FindObjectsOfType(avProType))
                {
                    var component = (Component)player;
                    var maxResField = avProType.GetField("maximumResolution", flags);
                    if (maxResField != null)
                    {
                        int res = (int)maxResField.GetValue(player);
                        // 0 means unlimited. Anything above 1080p is lethal in VRChat.
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
                    if (lowLatencyField != null && (bool)lowLatencyField.GetValue(player))
                    {
                        LogDiagnostic("VIDEO PIPELINE: STABILITY", "Low Latency Enabled",
                            $"'{component.gameObject.name}' has 'Use Low Latency' enabled. This strips the video buffer and will cause severe stuttering for any player without a perfect internet connection. Disable for general media.",
                            "#ffaa00", component, () => {
                                Undo.RecordObject(component, "Disable Low Latency");
                                lowLatencyField.SetValue(player, false);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                    }
                }
            }

            // 2. Unity Video Check
            Type unityVideoType = GetTypeSafe("VRC.SDK3.Video.Components.VRCUnityVideoPlayer");
            if (unityVideoType != null)
            {
                foreach (var player in FindObjectsOfType(unityVideoType))
                {
                    var component = (Component)player;
                    var maxResField = unityVideoType.GetField("maximumResolution", flags);
                    if (maxResField != null)
                    {
                        int res = (int)maxResField.GetValue(player);
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
                }
            }
        }

        private void AuditTxlEcosystem()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var udonBehaviours = FindObjectsOfType<UdonBehaviour>(true);
            foreach (var udon in udonBehaviours)
            {
                if (udon.programSource == null)
                {
                    LogDiagnostic("TXL ECOSYSTEM & UDON", "Orphaned UdonBehaviour", $"'{udon.gameObject.name}' has a dead Udon component with no program source. It will bloat serialization.", "#ff00aa", udon.gameObject, () => {
                        Undo.DestroyObjectImmediate(udon);
                    });
                }
            }

            Type debugUserListType = GetTypeSafe("Texel.DebugUserList");
            if (debugUserListType != null)
            {
                foreach (var dul in FindObjectsOfType(debugUserListType))
                {
                    LogDiagnostic("TXL ECOSYSTEM & UDON", "Debug GC Sink Active", $"'{((Component)dul).gameObject.name}' contains a Texel DebugUserList. This allocates massive amounts of string garbage per frame on player updates. Disable before publishing.", "#ffaa00", (Component)dul);
                }
            }

            Type accessControlType = GetTypeSafe("Texel.AccessControl");
            if (accessControlType != null)
            {
                foreach (var acl in FindObjectsOfType(accessControlType))
                {
                    var component = (Component)acl;
                    var whitelistField = accessControlType.GetField("userWhitelist", flags);
                    if (whitelistField != null)
                    {
                        var whitelist = whitelistField.GetValue(acl) as string[];
                        if (whitelist != null && whitelist.Length > 50)
                        {
                            LogDiagnostic("TXL ECOSYSTEM & UDON", "Inefficient Inline Whitelist", $"'{component.gameObject.name}' has an inline array of {whitelist.Length} users. This forces heavy string iteration on Start/Join. Use a remote list or hashed whitelist instead.", "#00e5ff", component);
                        }
                    }
                }
            }

            Type trackedZoneTriggerType = GetTypeSafe("Texel.TrackedZoneTrigger");
            if (trackedZoneTriggerType != null)
            {
                foreach (var tzt in FindObjectsOfType(trackedZoneTriggerType))
                {
                    var component = (Component)tzt;
                    var intervalField = trackedZoneTriggerType.GetField("monitorTriggerInterval", flags);
                    if (intervalField != null)
                    {
                        float interval = (float)intervalField.GetValue(tzt);
                        if (interval < 0.5f)
                        {
                            LogDiagnostic("TXL ECOSYSTEM & UDON", "Trigger CPU Starvation", $"'{component.gameObject.name}' polls players every {interval}s. In an 80-player instance, this will throttle the Udon VM. Increase interval to 0.5s+.", "#ff00aa", component, () => {
                                Undo.RecordObject(component, "Throttle Trigger Polling");
                                intervalField.SetValue(tzt, 0.5f);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                            });
                        }
                    }
                }
            }

            Type compoundZoneTriggerType = GetTypeSafe("Texel.CompoundZoneTrigger");
            if (compoundZoneTriggerType != null)
            {
                foreach (var czt in FindObjectsOfType(compoundZoneTriggerType))
                {
                    var component = (Component)czt;
                    var forceCheckField = compoundZoneTriggerType.GetField("forceColliderCheck", flags);
                    bool forceCheck = forceCheckField != null && (bool)forceCheckField.GetValue(czt);
                    
                    var meshColliders = component.GetComponents<MeshCollider>();
                    if (meshColliders.Length > 0 && forceCheck)
                    {
                         LogDiagnostic("TXL ECOSYSTEM & UDON", "Compound Physics Drag", $"'{component.gameObject.name}' uses forceColliderCheck alongside MeshColliders. ClosestPoint checks on meshes are incredibly expensive in Udon.", "#ffaa00", component);
                    }
                }
            }

            Type translationTableType = GetTypeSafe("Texel.TranslationTable");
            if (translationTableType != null)
            {
                foreach (var table in FindObjectsOfType(translationTableType))
                {
                    var component = (Component)table;
                    var langs = translationTableType.GetField("languages", flags)?.GetValue(table) as string[];
                    var keys = translationTableType.GetField("keys", flags)?.GetValue(table) as string[];
                    var values = translationTableType.GetField("values", flags)?.GetValue(table) as string[];
                    
                    if (langs != null && keys != null && values != null)
                    {
                        int expected = langs.Length * keys.Length;
                        if (values.Length != expected)
                        {
                            LogDiagnostic("TXL ECOSYSTEM & UDON", "Translation Matrix Collapsed", $"'{component.gameObject.name}' has a desynced 1D translation array ({values.Length} values, expected {expected}). This will cause IndexOutOfRange errors at runtime.", "#ff00aa", component);
                        }
                    }
                }
            }

            Type serializedKeyType = GetTypeSafe("Texel.SerializedKey");
            if (serializedKeyType != null)
            {
                foreach (var sKey in FindObjectsOfType(serializedKeyType))
                {
                    var component = (Component)sKey;
                    var keyStr = serializedKeyType.GetField("key", flags)?.GetValue(sKey) as string;
                    if (!string.IsNullOrEmpty(keyStr))
                    {
                        LogDiagnostic("TXL DATA & SECURITY", "Plaintext Validation Key", $"'{component.gameObject.name}' is exposing a data validation key in plaintext via Inspector serialization. Vulnerable to client memory dumps.", "#ff00aa", component);
                    }
                }
            }

            Type digestValidatorType = GetTypeSafe("Texel.DigestValidator");
            if (digestValidatorType != null)
            {
                foreach (var validator in FindObjectsOfType(digestValidatorType))
                {
                    var component = (Component)validator;
                    LogDiagnostic("TXL DATA & SECURITY", "Heavy Cryptography Load", $"'{component.gameObject.name}' uses UdonHashLib. Executing SHA/HMAC functions natively in Udon UASM is extremely slow. Ensure this validator is not linked to high-frequency network events.", "#ffaa00", component);
                }
            }
        }

        private void AuditProTVEcosystem()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Type proTvType = GetTypeSafe("ArchiTech.ProTV.TVManager");
            if (proTvType != null)
            {
                int globalTextureCount = 0;
                foreach (var tv in FindObjectsOfType(proTvType))
                {
                    var component = (Component)tv;

                    var enableHDRField = proTvType.GetField("enableHDR", flags);
                    if (enableHDRField != null && (bool)enableHDRField.GetValue(tv))
                    {
                        LogDiagnostic("PROTV VRAM: HDR BLOAT", "HDR Video Enabled", $"'{component.gameObject.name}' has HDR enabled. This forces ARGB64, doubling video texture VRAM footprint.", "#ff00aa", component, () => {
                            Undo.RecordObject(component, "Disable HDR on ProTV");
                            enableHDRField.SetValue(tv, false);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                    }

                    var bakeGlobalField = proTvType.GetField("bakeGlobalVideoTexture", flags);
                    if (bakeGlobalField != null && (bool)bakeGlobalField.GetValue(tv))
                    {
                        LogDiagnostic("PROTV VRAM: BAKED GSV", "Baked Global Texture", $"'{component.gameObject.name}' bakes the global texture. This adds an extra internal Blit pass and wastes GPU memory if not explicitly needed.", "#ffaa00", component, () => {
                            Undo.RecordObject(component, "Disable Baked Global Texture");
                            bakeGlobalField.SetValue(tv, false);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                    }

                    var preferAltField = proTvType.GetField("preferAlternateUrlForQuest", flags);
                    if (preferAltField != null && !(bool)preferAltField.GetValue(tv))
                    {
                        LogDiagnostic("PROTV COMPATIBILITY: QUEST FALLBACK", "Missing Quest Fallback", $"'{component.gameObject.name}' has 'Prefer Alternate URL for Quest' disabled. Android clients will try to resolve high-bitrate PC endpoints, often resulting in silent fail.", "#ffaa00", component, () => {
                            Undo.RecordObject(component, "Enable Quest Fallback");
                            preferAltField.SetValue(tv, true);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                        });
                    }

                    var enableGSVField = proTvType.GetField("enableGSV", flags);
                    if (enableGSVField != null && (bool)enableGSVField.GetValue(tv)) globalTextureCount++;
                    
                    var videoManagersField = proTvType.GetField("videoManagers", flags);
                    if (videoManagersField != null)
                    {
                        var videoManagers = videoManagersField.GetValue(tv) as Array;
                        if (videoManagers == null || videoManagers.Length == 0)
                        {
                            LogDiagnostic("PROTV CRITICAL: MISSING MANAGERS", "Missing Video Managers", $"'{component.gameObject.name}' has no VPManagers assigned. The TV will crash on initialization.", "#ff00aa", component);
                        }
                    }

                    var customTextureField = proTvType.GetField("customTexture", flags);
                    if (customTextureField != null)
                    {
                        var customTex = customTextureField.GetValue(tv) as RenderTexture;
                        if (customTex != null && (customTex.width > 2048 || customTex.height > 2048))
                        {
                            float mb = (customTex.width * customTex.height * 4) / 1048576f;
                            LogDiagnostic("PROTV VRAM: MASSIVE RENDER TEXTURE", "Oversized Custom Texture", 
                                $"'{component.gameObject.name}' has a custom RenderTexture assigned of {customTex.width}x{customTex.height} (~{mb:F2} MB). ProTV duplicates this memory buffer natively. This will nuke your world's VRAM.", 
                                "#ff00aa", component);
                        }
                    }
                }

                if (globalTextureCount > 1)
                {
                    LogDiagnostic("PROTV RENDER: GSV CONFLICT", "GSV Conflict", $"Found {globalTextureCount} TVs with Global Video Texture (GSV) enabled. Only one should be active to prevent global shader variable tearing.", "#ff00aa", null);
                }
            }

            Type mediaControlsType = GetTypeSafe("ArchiTech.ProTV.MediaControls");
            if (mediaControlsType != null)
            {
                foreach (var controls in FindObjectsOfType(mediaControlsType))
                {
                    var component = (Component)controls;
                    var realtimeSeekField = mediaControlsType.GetField("realtimeSeek", flags);
                    
                    if (realtimeSeekField != null && (bool)realtimeSeekField.GetValue(controls))
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
                foreach (var playlistData in FindObjectsOfType(playlistDataType))
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
                foreach (var search in FindObjectsOfType(playlistSearchType))
                {
                    var component = (Component)search;
                    var aggroField = playlistSearchType.GetField("searchAggressionLevel", flags);
                    if (aggroField != null)
                    {
                        int aggro = (int)aggroField.GetValue(search);
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
                foreach (var rtgi in FindObjectsOfType(rtgiType))
                {
                    var component = (Component)rtgi;
                    var runOnMobileField = rtgiType.GetField("runOnMobile", flags);
                    if (runOnMobileField != null && (bool)runOnMobileField.GetValue(rtgi))
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
                foreach (var queue in FindObjectsOfType(queueType))
                {
                    var component = (Component)queue;
                    var maxEntriesField = queueType.GetField("maxEntriesPerPlayer", flags);
                    var maxBurstField = queueType.GetField("maxBurstEntriesPerPlayer", flags);
                    
                    if (maxEntriesField != null && maxBurstField != null)
                    {
                        int maxEntries = (int)maxEntriesField.GetValue(queue);
                        int maxBurst = (int)maxBurstField.GetValue(queue);

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
                foreach (var toggles in FindObjectsOfType(tvTogglesType))
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

            Type vpManagerType = GetTypeSafe("ArchiTech.ProTV.VPManager");
            if (vpManagerType != null)
            {
                foreach (var vpm in FindObjectsOfType(vpManagerType))
                {
                    var component = (Component)vpm;
                    var screensField = vpManagerType.GetField("screens", flags);
                    if (screensField != null)
                    {
                        var screens = screensField.GetValue(vpm) as GameObject[];
                        if (screens != null)
                        {
                            foreach (var scr in screens)
                            {
                                if (scr == null) continue;
                                var rend = scr.GetComponent<Renderer>();
                                if (rend != null)
                                {
                                    foreach (var mat in rend.sharedMaterials)
                                    {
                                        if (mat != null && mat.globalIlluminationFlags == MaterialGlobalIlluminationFlags.RealtimeEmissive)
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
            if (queueUiType != null) complexUis.AddRange(FindObjectsOfType(queueUiType).Cast<Component>());
            if (historyUiType != null) complexUis.AddRange(FindObjectsOfType(historyUiType).Cast<Component>());
            if (playlistUiType != null) complexUis.AddRange(FindObjectsOfType(playlistUiType).Cast<Component>());

            foreach (var uiComp in complexUis)
            {
                Canvas parentCanvas = uiComp.GetComponentInParent<Canvas>();
                if (parentCanvas != null && parentCanvas.isRootCanvas)
                {
                    LogDiagnostic("PROTV UI: CANVAS REBUILD CASCADE", "Canvas Rebuild Cascade", $"'{uiComp.gameObject.name}' modifies layout elements on a Root Canvas. This forces a full rebuild of the entire Canvas every time an item changes. Nest it inside a sub-canvas.", "#ffaa00", parentCanvas.gameObject);
                }
            }
        }

        private void AuditIwaSyncEcosystem()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Type iwaType = GetTypeSafe("HoshinoLabs.IwaSync3.IwaSync3");
            if (iwaType != null)
            {
                foreach (var iwa in FindObjectsOfType(iwaType))
                {
                    var component = (Component)iwa;
                    var maxResField = iwaType.GetField("maximumResolution", flags);
                    if (maxResField != null)
                    {
                        int res = (int)maxResField.GetValue(iwa);
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

            Type playlistType = GetTypeSafe("HoshinoLabs.IwaSync3.Playlist");
            if (playlistType != null)
            {
                foreach (var pl in FindObjectsOfType(playlistType))
                {
                    var component = (Component)pl;
                    var limitField = playlistType.GetField("playlistLimitCount", flags);
                    if (limitField != null)
                    {
                        int limit = (int)limitField.GetValue(pl);
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

            Type speakerType = GetTypeSafe("HoshinoLabs.IwaSync3.Speaker");
            if (speakerType != null)
            {
                foreach (var spk in FindObjectsOfType(speakerType))
                {
                    var component = (Component)spk;
                    var spatializeField = speakerType.GetField("spatialize", flags);
                    if (spatializeField != null)
                    {
                        bool spatialize = (bool)spatializeField.GetValue(spk);
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
                }
            }

            Type screenType = GetTypeSafe("HoshinoLabs.IwaSync3.Screen");
            if (screenType != null)
            {
                foreach (var scr in FindObjectsOfType(screenType))
                {
                    var component = (Component)scr;
                    var matIndexField = screenType.GetField("materialIndex", flags);
                    var screenRendererField = screenType.GetField("screen", flags); 
                    
                    if (screenRendererField != null && matIndexField != null)
                    {
                        var renderer = screenRendererField.GetValue(scr) as Renderer; 
                        if (renderer != null)
                        {
                            int idx = (int)matIndexField.GetValue(scr);
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

            Type videoCoreType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoCore");
            if (videoCoreType != null)
            {
                foreach (var core in FindObjectsOfType(videoCoreType))
                {
                    var component = (Component)core;
                    var syncFreqField = videoCoreType.GetField("syncFrequency", flags);
                    if (syncFreqField != null)
                    {
                        float freq = (float)syncFreqField.GetValue(core);
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
                foreach (var scr in FindObjectsOfType(udonScreenType))
                {
                    var component = (Component)scr;
                    var emissiveBoostField = udonScreenType.GetField("defaultEmissiveBoost", flags);
                    if (emissiveBoostField != null)
                    {
                        float boost = (float)emissiveBoostField.GetValue(scr);
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
                foreach (var invoker in FindObjectsOfType(eventInvokerType))
                {
                    var component = (Component)invoker;
                    LogDiagnostic("IWASYNC3 ECOSYSTEM", "Runtime Instantiation Risk",
                        $"'{component.gameObject.name}' contains a CustomEventInvoker. This script uses Instantiate() at runtime to process delayed events. Rapidly triggering UI elements connected to this will cause severe Garbage Collection spikes and frame stutters.",
                        "#ffaa00", component);
                }
            }
        }

        private void AuditUdonAndNetwork()
        {
            Assembly editorAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "UdonSharp.Editor");
            Type cacheType = editorAsm?.GetType("UdonSharp.UdonSharpEditorCache");
            var cache = cacheType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
            var getUasm = cacheType?.GetMethod("GetUASMStr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var udon in FindObjectsOfType<UdonBehaviour>(true))
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

            foreach (var objSync in FindObjectsOfType<VRCObjectSync>(true))
            {
                LogDiagnostic("UDON PHYSICS: OBJECT SYNC", "VRC Object Sync", $"'{objSync.gameObject.name}' transmits physics state over network.", "#00ff88", objSync.gameObject);
            }
        }

        private void AuditLightingAndCameras()
        {
            foreach (var light in FindObjectsOfType<Light>(true))
            {
                if (light.type != LightType.Directional && light.lightmapBakeType == LightmapBakeType.Realtime)
                    LogDiagnostic("LIGHTING & SHADOWS", "Realtime Lights", $"'{light.name}' is fully dynamic (Expensive).", "#ffaa00", light.gameObject);

                if (light.lightmapBakeType == LightmapBakeType.Realtime && light.shadows != LightShadows.None)
                    LogDiagnostic("LIGHTING & SHADOWS", "Shadow Casters", $"'{light.name}' casting realtime shadows.", "#ff00aa", light.gameObject);
            }

            foreach (var probe in FindObjectsOfType<ReflectionProbe>(true))
                if (probe.mode == UnityEngine.Rendering.ReflectionProbeMode.Realtime)
                    LogDiagnostic("LIGHTING & SHADOWS", "Realtime Probes", $"'{probe.name}' rendering scene per-frame.", "#ff00aa", probe.gameObject);
        }

        private void AuditPhysics()
        {
            foreach (var collider in FindObjectsOfType<MeshCollider>(true))
                if (!collider.convex)
                    LogDiagnostic("PHYSICS & COLLIDERS", "Non-Convex Meshes", $"'{collider.gameObject.name}' uses complex collision geometry.", "#ff00aa", collider.gameObject);
        }

        private void AuditGeometryAndMaterials()
        {
            var renderers = FindObjectsOfType<Renderer>(true);
            HashSet<Material> sceneMaterials = new HashSet<Material>();

            foreach (var renderer in renderers)
            {
                // DEEP TEXTURE SCRAPE: Capture textures from every material slot in the scene
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null) continue;
                    sceneMaterials.Add(mat);
                    ScrapeTexturesFromMaterial(mat); 
                }

                // Mesh Extraction & LOD Check
                if (renderer is SkinnedMeshRenderer smr && smr.sharedMesh != null) 
                {
                    _detectedMeshes.Add(smr.sharedMesh);
                }
                else if (renderer is MeshRenderer mr)
                {
                    var filter = mr.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null)
                    {
                        _detectedMeshes.Add(filter.sharedMesh);
                        // Flag heavy meshes that will cause excessive draw calls due to missing LODs
                        if (filter.sharedMesh.vertexCount > 5000 && renderer.GetComponentInParent<LODGroup>() == null)
                        {
                            LogDiagnostic("MESHES & GEOMETRY", "Missing LOD Group", $"'{renderer.name}' has {filter.sharedMesh.vertexCount} verts but no LODs.", "#00e5ff", renderer.gameObject);
                        }
                    }
                }
            }

            // Audit discovered Meshes for Read/Write leaks
            foreach (var mesh in _detectedMeshes)
            {
                if (mesh.vertexCount > 65000) LogDiagnostic("MESHES & GEOMETRY", "High Poly Counts", $"'{mesh.name}' has {mesh.vertexCount} vertices.", "#ff00aa", mesh);
                if (mesh.isReadable)
                {
                    LogDiagnostic("MESHES & GEOMETRY", "Read/Write Enabled", $"'{mesh.name}' leaks CPU Memory.", "#ffaa00", mesh, () => {
                        string meshPath = AssetDatabase.GetAssetPath(mesh);
                        if (!string.IsNullOrEmpty(meshPath))
                        {
                            ModelImporter imp = AssetImporter.GetAtPath(meshPath) as ModelImporter;
                            if (imp != null) { imp.isReadable = false; imp.SaveAndReimport(); }
                        }
                    });
                }
            }

            // Audit Materials for Shader integrity and Whitelist compliance
            foreach (var mat in sceneMaterials)
            {
                string shaderName = mat.shader != null ? mat.shader.name : "Missing Shader";
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
                    // Circuit Breaker: If the material is ALREADY using the selected replacement shader, 
                    // it is inherently compliant. Skip further diagnostic checks.
                    if (_targetReplacementShader != null && mat.shader == _targetReplacementShader) continue;
                    // ---------------

                    // Protection Check: Skip Video Players, AudioLink, etc.
                    if (ShaderDictionaryAsset.IsGloballyProtected(mat.shader)) continue; 
                    
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
                
                // Final pass: Capture any standard asset dependencies
                string path = AssetDatabase.GetAssetPath(mat);
                if (string.IsNullOrEmpty(path)) continue;
                
                var deps = AssetDatabase.GetDependencies(path, true)
                    .Select(AssetDatabase.LoadAssetAtPath<Texture>) 
                    .Where(t => t != null);
                
                foreach (var tex in deps) _detectedTextures.Add(tex);
            }
        }

        private void ScrapeTexturesFromMaterial(Material mat)
        {
            Shader shader = mat.shader;
            if (shader == null) return;

            int propCount = ShaderUtil.GetPropertyCount(shader);
            for (int i = 0; i < propCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    Texture tex = mat.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                    if (tex != null) _detectedTextures.Add(tex);
                }
            }
        }

        private void AnalyzeTextures()
        {
            foreach (var tex in _detectedTextures)
            {
                if (tex == null) continue;

                string path = AssetDatabase.GetAssetPath(tex);
                long bytes = Profiler.GetRuntimeMemorySizeLong(tex);
                float mb = bytes / 1048576f;

                if (string.IsNullOrEmpty(path))
                {
                    if (tex.width > _targetTextureResolution || mb > 5f)
                    {
                        LogDiagnostic("TEXTURE MEMORY (VRAM)", "Unmanaged Instanced Texture", 
                            $"'{tex.name}' ({tex.width}x{tex.height}) is a runtime instance taking {mb:F2} MB. This cannot be auto-fixed; check your scripts.", 
                            "#ff00aa", tex);
                    }
                    continue;
                }

                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;

                if (tex.width > _targetTextureResolution || tex.height > _targetTextureResolution)
                {
                    LogDiagnostic("TEXTURE MEMORY (VRAM)", "Oversized Textures", 
                        $"'{tex.name}' ({tex.width}x{tex.height}) is {mb:F2} MB. Ready to crunch to {_targetTextureResolution}.", 
                        "#ff00aa", tex, () => {
                        imp.maxTextureSize = _targetTextureResolution;
                        imp.textureCompression = TextureImporterCompression.CompressedHQ;
                        imp.crunchedCompression = true;
                        imp.SaveAndReimport();
                    });
                }
                
                if (imp.isReadable)
                {
                    LogDiagnostic("TEXTURE MEMORY (VRAM)", "Read/Write Enabled", 
                        $"'{tex.name}' leaks CPU Memory (Read/Write is ON).", "#ffaa00", tex, () => {
                        imp.isReadable = false;
                        imp.SaveAndReimport();
                    });
                }

                if (imp.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    LogDiagnostic("TEXTURE MEMORY (VRAM)", "Uncompressed Asset", 
                        $"'{tex.name}' is uncompressed, taking {mb:F2} MB in VRAM.", "#ffaa00", tex, () => {
                        imp.textureCompression = TextureImporterCompression.CompressedHQ;
                        imp.crunchedCompression = true;
                        imp.SaveAndReimport();
                    });
                }
            }
        }

        private void AuditCanvasesAndUIMemory()
        {
            var canvases = FindObjectsOfType<Canvas>(true);
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

            foreach (var img in FindObjectsOfType<UnityEngine.UI.Image>(true))
                if (img.sprite != null && img.sprite.texture != null) _detectedUITextures.Add(img.sprite.texture);
                
            foreach (var raw in FindObjectsOfType<UnityEngine.UI.RawImage>(true))
                if (raw.texture != null) _detectedUITextures.Add(raw.texture);
                
            foreach (var txt in FindObjectsOfType<TMP_Text>(true))
                if (txt.font != null && txt.font.material != null && txt.font.material.mainTexture != null) 
                    _detectedUITextures.Add(txt.font.material.mainTexture);
                    
            foreach (var legacyTxt in FindObjectsOfType<UnityEngine.UI.Text>(true))
                if (legacyTxt.font != null && legacyTxt.font.material != null && legacyTxt.font.material.mainTexture != null) 
                    _detectedUITextures.Add(legacyTxt.font.material.mainTexture);
        }

        private void AuditExplicitTextComponents()
        {
            if (_targetTMPFont != null)
            {
                var text3DComps = FindObjectsOfType<TextMeshPro>(true); 
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

                var textUIComps = FindObjectsOfType<TextMeshProUGUI>(true);
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
                var legacyTextComps = FindObjectsOfType<UnityEngine.UI.Text>(true);
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
            foreach (var source in FindObjectsOfType<AudioSource>(true))
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
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Type playerObjectType = GetTypeSafe("VRC.SDK3.Persistence.VRCPlayerObject");
            if (playerObjectType != null)
            {
                var playerObjects = FindObjectsOfType(playerObjectType);
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
                var udonBehaviours = FindObjectsOfType<UdonBehaviour>(true);

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
                AssetDatabase.StartAssetEditing();
                foreach (var diag in actionableDiagnostics) diag.FixPayload.Invoke();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                InitiateFullMatrixScan(); 
                EditorUtility.DisplayDialog("VIXEN SYSTEM", "Targeted purges complete.", "ACKNOWLEDGE");
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