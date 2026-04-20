#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ImageMagick;
using UnityEditor;
using UnityEngine;

namespace VixenTools.Editor
{
    public class VixenBadgeMaker : EditorWindow
    {
        private enum ToolMode { BadgeGenerator, TemplateBuilder }
        private enum AuthoringType { ProceduralBase, IngestFromSource }
        private enum Ecosystem { VixenTools, FuralitySDK }
        
        // The Master Shader Enum
        private enum TargetShader 
        { 
            AutoDetect, 
            Standard, 
            PoiyomiToon, 
            LilToon, 
            FuralityAqua, 
            FuralitySylva, 
            FuralitySomna, 
            FuralityUmbra, 
            VRCToonStandard, 
            VRCMobileToonLit 
        }

        private ToolMode _currentMode = ToolMode.BadgeGenerator;

        // Scrollable Body GUI
        private Vector2 _scrollPosition;

        // GDI Font Loading
        [DllImport("Gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int AddFontResourceEx(string lpFileName, uint fl, IntPtr pdv);

        [DllImport("Gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RemoveFontResourceEx(string lpFileName, uint fl, IntPtr pdv);

        // --- Shared State ---
        private const string VixenRootPath = "Assets/VixenTools/Badges";
        private const string FuralityRootPath = "Assets/Furality";
        private const string FontFileName = "Cyberpunk-Regular.ttf"; 
        private static string SystemFontPath => Path.Combine(Application.persistentDataPath, "VixenFonts");

        // --- Generator State ---
        private string _badgeName = "";
        private string _title = "";
        private Color _neonColor = new Color(1f, 1f, 1f); 
        private bool _applyToMaterial = true;
        
        // Dynamic Shader Validation State
        private TargetShader _targetShader = TargetShader.AutoDetect; 
        private List<TargetShader> _validShaders = new List<TargetShader>();
        private string[] _validShaderNames = new string[0];
        private int _selectedShaderIndex = 0;

        // Ecosystem Routing
        private Ecosystem _activeEcosystem = Ecosystem.VixenTools;
        
        // VixenTools Ecosystem
        private string[] _vixenTemplates = new string[0];
        private int _selectedVixenTemplate = 0;

        // Furality Ecosystem
        private List<string> _furalityConventions = new List<string>();
        private int _selectedFuralityConv = 0;
        private List<string> _furalityTiers = new List<string>();
        private int _selectedFuralityTier = 0;

        // Dynamic Layout Bounds 
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

        [MenuItem("VixenTools/Badge Studio")]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenBadgeMaker>("Badge Studio");
            window.minSize = new Vector2(480, 650);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPrefs();
            RefreshEcosystems();
            ValidateInstalledShaders(); // Initial checksum pass
        }

        private void OnDisable()
        {
            SavePrefs();
            UnloadFonts();
        }

        private void RefreshEcosystems()
        {
            if (Directory.Exists(VixenRootPath))
            {
                _vixenTemplates = AssetDatabase.GetSubFolders(VixenRootPath).Select(Path.GetFileName).ToArray();
            }
            else _vixenTemplates = new string[0];

            _furalityConventions.Clear();
            if (Directory.Exists(FuralityRootPath))
            {
                var convFolders = AssetDatabase.GetSubFolders(FuralityRootPath);
                foreach (var folder in convFolders)
                {
                    string badgePath = Path.Combine(folder, "Avatar Assets", "Badges");
                    if (Directory.Exists(badgePath))
                    {
                        _furalityConventions.Add(Path.GetFileName(folder));
                    }
                }
            }

            UpdateFuralityTiers();
            ValidateInstalledShaders(); // Re-verify shaders just in case a package was imported/deleted
        }

        private void UpdateFuralityTiers()
        {
            _furalityTiers.Clear();
            if (_furalityConventions.Count > 0 && _selectedFuralityConv < _furalityConventions.Count)
            {
                string badgePath = Path.Combine(FuralityRootPath, _furalityConventions[_selectedFuralityConv], "Avatar Assets", "Badges");
                if (Directory.Exists(badgePath))
                {
                    _furalityTiers.AddRange(AssetDatabase.GetSubFolders(badgePath).Select(Path.GetFileName));
                }
            }
            _selectedFuralityTier = Mathf.Clamp(_selectedFuralityTier, 0, Mathf.Max(0, _furalityTiers.Count - 1));
        }

        private void OnGUI()
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, 50);
            EditorGUI.DrawRect(headerRect, new Color(0.08f, 0.04f, 0.12f));

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true, alignment = TextAnchor.MiddleCenter, fontSize = 20 };
            EditorGUI.LabelField(headerRect, "<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> BADGE STUDIO", headerStyle);

            GUILayout.Space(10);

            _currentMode = (ToolMode)GUILayout.Toolbar((int)_currentMode, new string[] { "High-Fidelity Generator", "Template Authoring Engine" }, GUILayout.Height(30));
            GUILayout.Space(10);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);
            GUILayout.Space(5); 

            GUIStyle sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true, fontSize = 14 };

            if (_currentMode == ToolMode.BadgeGenerator) DrawGeneratorUI(sectionHeaderStyle);
            else DrawTemplateBuilderUI(sectionHeaderStyle);

            GUILayout.EndScrollView();
        }

        private void DrawGeneratorUI(GUIStyle headerStyle)
        {
            EditorGUILayout.LabelField("<color=#00e5ff>Ecosystem Routing</color>", headerStyle);
            GUILayout.Space(5);
            
            _activeEcosystem = (Ecosystem)EditorGUILayout.EnumPopup("Source Network", _activeEcosystem);

            if (_activeEcosystem == Ecosystem.VixenTools)
            {
                if (_vixenTemplates.Length == 0)
                {
                    EditorGUILayout.HelpBox($"No VixenTools templates found at {VixenRootPath}. Switch to Authoring Engine or Furality SDK.", MessageType.Warning);
                    if (GUILayout.Button("Initialize VixenTools Pipeline Root")) { Directory.CreateDirectory(VixenRootPath); AssetDatabase.Refresh(); RefreshEcosystems(); }
                    return;
                }
                _selectedVixenTemplate = EditorGUILayout.Popup("Template Base", _selectedVixenTemplate, _vixenTemplates);
            }
            else // Furality
            {
                if (_furalityConventions.Count == 0)
                {
                    EditorGUILayout.HelpBox("No Furality installations detected in Assets/Furality. Import a convention SDK to continue.", MessageType.Warning);
                    return;
                }

                EditorGUI.BeginChangeCheck();
                _selectedFuralityConv = EditorGUILayout.Popup("Convention", _selectedFuralityConv, _furalityConventions.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateFuralityTiers();
                    AutoAssignLayoutBounds(_furalityConventions[_selectedFuralityConv]);
                }

                if (_furalityTiers.Count > 0)
                {
                    _selectedFuralityTier = EditorGUILayout.Popup("Badge Tier", _selectedFuralityTier, _furalityTiers.ToArray());
                }
                else EditorGUILayout.HelpBox("No Badge Tiers found in this convention.", MessageType.Error);
            }

            GUILayout.Space(15);
            DrawSeparator(new Color(0f, 0.9f, 1f, 0.3f)); 
            GUILayout.Space(15);

            EditorGUILayout.LabelField("<color=#00e5ff>Identity Generation</color>", headerStyle);
            _badgeName = EditorGUILayout.TextField("Display Name", _badgeName);
            _title = EditorGUILayout.TextField("Title / Pronouns", _title);

            GUILayout.Space(15);
            EditorGUILayout.LabelField("<color=#ff00aa>Aesthetics & Materials</color>", headerStyle);
            _neonColor = EditorGUILayout.ColorField("Neon Text Color", _neonColor);
            
            // Dynamic Validated Shader Dropdown
            _selectedShaderIndex = EditorGUILayout.Popup("Target Shader", _selectedShaderIndex, _validShaderNames);
            _targetShader = _validShaders[_selectedShaderIndex];
            
            _applyToMaterial = EditorGUILayout.Toggle("Auto-Apply to Material", _applyToMaterial);

            GUILayout.Space(15);
            DrawSeparator(new Color(1f, 0f, 0.66f, 0.3f)); 
            GUILayout.Space(15);

            _showAdvancedLayout = EditorGUILayout.Foldout(_showAdvancedLayout, "Advanced UV Layout Bounds");
            if (_showAdvancedLayout)
            {
                EditorGUI.indentLevel++;
                _nameX = EditorGUILayout.IntField("Name X", _nameX); _nameY = EditorGUILayout.IntField("Name Y", _nameY);
                _nameW = EditorGUILayout.IntField("Name W", _nameW); _nameH = EditorGUILayout.IntField("Name H", _nameH);
                _nameRotation = EditorGUILayout.FloatField("Name Rotation", _nameRotation);
                EditorGUILayout.Space(5);
                _titleX = EditorGUILayout.IntField("Title X", _titleX); _titleY = EditorGUILayout.IntField("Title Y", _titleY);
                _titleW = EditorGUILayout.IntField("Title W", _titleW); _titleH = EditorGUILayout.IntField("Title H", _titleH);
                _titleRotation = EditorGUILayout.FloatField("Title Rotation", _titleRotation);
                EditorGUI.indentLevel--;
                if (GUILayout.Button("Save Layout As Default")) SavePrefs();
            }

            GUILayout.Space(20);

            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.5f);
            if (GUILayout.Button("Compile High-Fidelity Badge", GUILayout.Height(40)))
            {
                GUI.backgroundColor = Color.white;
                try { GenerateBadgeEndToEnd(); } finally { EditorUtility.ClearProgressBar(); }
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawTemplateBuilderUI(GUIStyle headerStyle)
        {
            EditorGUILayout.LabelField("<color=#ff00aa>Programmatic Asset Authoring (VixenTools)</color>", headerStyle);
            EditorGUILayout.HelpBox("Scaffolds a complete directory structure inside Assets/VixenTools/Badges.", MessageType.Info);
            GUILayout.Space(10);

            _newTemplateName = EditorGUILayout.TextField("Template Name", _newTemplateName);
            
            GUILayout.Space(10);
            _authoringType = (AuthoringType)EditorGUILayout.EnumPopup("Authoring Mode", _authoringType);
            GUILayout.Space(10);

            if (_authoringType == AuthoringType.ProceduralBase)
            {
                int[] resOptions = { 512, 1024, 2048, 4096 };
                string[] resLabels = { "512x", "1K", "2K", "4K" };
                int resIndex = Array.IndexOf(resOptions, _templateResolution);
                resIndex = EditorGUILayout.Popup("Base Resolution", resIndex == -1 ? 3 : resIndex, resLabels);
                _templateResolution = resOptions[resIndex];

                _baseTemplateColor = EditorGUILayout.ColorField("Base Diffuse Color", _baseTemplateColor);
            }
            else
            {
                _sourceDiffuse = (Texture2D)EditorGUILayout.ObjectField("Empty Diffuse Map", _sourceDiffuse, typeof(Texture2D), false);
                _sourceEmission = (Texture2D)EditorGUILayout.ObjectField("Empty Emission Map", _sourceEmission, typeof(Texture2D), false);
            }

            GUILayout.Space(10);
            
            // Dynamic Validated Shader Dropdown
            _selectedShaderIndex = EditorGUILayout.Popup("Master Material Shader", _selectedShaderIndex, _validShaderNames);
            _targetShader = _validShaders[_selectedShaderIndex];

            GUILayout.Space(20);
            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.8f);
            if (GUILayout.Button("Author Master Template", GUILayout.Height(40)))
            {
                GUI.backgroundColor = Color.white;
                try { ExecuteTemplateAuthoring(); } finally { EditorUtility.ClearProgressBar(); }
            }
            GUI.backgroundColor = Color.white;
        }

        #region Validation & Checksum Engine

        private void ValidateInstalledShaders()
        {
            _validShaders.Clear();
            _validShaders.Add(TargetShader.AutoDetect); // Always available

            // Ping the Unity Shader Database for every target in our ecosystem
            foreach (TargetShader shader in Enum.GetValues(typeof(TargetShader)))
            {
                if (shader == TargetShader.AutoDetect) continue;
                
                if (FindShaderSafely(shader) != null)
                {
                    _validShaders.Add(shader);
                }
            }

            // Map valid enums to clean, readable UI display names
            _validShaderNames = _validShaders.Select(GetShaderDisplayName).ToArray();

            // Safety catch to prevent out-of-bounds indexing if a shader gets deleted
            if (_selectedShaderIndex >= _validShaders.Count) _selectedShaderIndex = 0;
            _targetShader = _validShaders[_selectedShaderIndex];
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
                case TargetShader.VRCToonStandard: return "VRChat Mobile Toon Standard";
                case TargetShader.VRCMobileToonLit: return "VRChat Mobile Toon Lit";
                default: return shader.ToString();
            }
        }

        #endregion

        #region Routing & Logic Engine

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

            // Poiyomi Version Fallback Heuristic
            if (foundShader == null && target == TargetShader.PoiyomiToon)
            {
                foundShader = Shader.Find(".poiyomi/Old Versions/9.3/Poiyomi Toon");
                if (foundShader == null) foundShader = Shader.Find("Hidden/Locked/poiyomi/Toon"); // Check locked state
            }
            return foundShader;
        }

        private void AutoAssignLayoutBounds(string conventionName)
        {
            if (conventionName.Contains("Luma"))
            {
                _nameX = 2258; _nameY = 1224; _nameW = 2538; _nameH = 855;
                _titleX = 2701; _titleY = 1677; _titleW = 1554; _titleH = 257;
                _neonColor = Color.white;
            }
            else if (conventionName.Contains("Somna"))
            {
                _nameX = 375; _nameY = 700; _nameW = 610; _nameH = 150;
                _titleX = 450; _titleY = 810; _titleW = 449; _titleH = 75;
                _neonColor = ColorUtility.TryParseHtmlString("#ffeead", out Color c) ? c : Color.white;
            }
            else if (conventionName.Contains("Sylva"))
            {
                _nameX = 1024; _nameY = 800; _nameW = 1400; _nameH = 300;
                _titleX = 1024; _titleY = 1150; _titleW = 1000; _titleH = 150;
                _neonColor = ColorUtility.TryParseHtmlString("#66ff00", out Color c) ? c : Color.green; 
            }
            else if (conventionName.Contains("Umbra"))
            {
                _nameX = 2258; _nameY = 1224; _nameW = 2538; _nameH = 855;
                _titleX = 2701; _titleY = 1677; _titleW = 1554; _titleH = 257;
                _neonColor = Color.white;
            }
            Debug.Log($"[VixenTools] Auto-configured layout bounds for {conventionName}");
        }

        private void GenerateBadgeEndToEnd()
        {
            string tierFolder = "";
            string outDir = "";
            string outPrefix = "";
            string tierName = "";
            string conventionName = "";

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

            LoadFontSafely();
            MagickColor mColor = UnityColorToMagick(_neonColor);
            MagickColor mWhite = new MagickColor("#ffffff");
            string fontPath = Path.Combine(SystemFontPath, FontFileName);

            EditorUtility.DisplayProgressBar("Badge Studio", "Rendering Text Plates...", 0.3f);
            using MagickImage nameImg = GenerateTextPlate(fontPath, _badgeName, _nameW, _nameH, mColor, _nameRotation);
            using MagickImage titleImg = GenerateTextPlate(fontPath, _title, _titleW, _titleH, mWhite, _titleRotation);

            EditorUtility.DisplayProgressBar("Badge Studio", "Compositing Maps...", 0.6f);

            CompositeTexture(difIn, nameImg, titleImg, difOut, applyGrayscale: false);
            CompositeTexture(emiIn, nameImg, titleImg, emiOut, applyGrayscale: isUmbra);

            AssetDatabase.Refresh();
            SetupTextureImporter(difOut, false);
            SetupTextureImporter(emiOut, false);

            if (_applyToMaterial) ApplyToMaterial(conventionName, tierName, difOut, emiOut);

            UnloadFonts();
            Debug.Log($"[VixenTools] Successfully compiled badge to {outDir}");
        }

        private string ResolveFuralityTexture(string folder, string conventionName, string tierName, string mapType)
        {
            if (!Directory.Exists(folder)) return null;

            var allFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || 
                            s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                            s.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                .ToList();

            string noSpaceTier = Regex.Replace(tierName, @"\s+", "");
            string[] exactNames = new string[0];

            if (conventionName.Contains("Luma") || conventionName.Contains("Sylva"))
            {
                exactNames = mapType == "DIF" ? 
                    new[] { $"{tierName}_Empty.png", $"{tierName}_Empty.jpg" } : 
                    new[] { $"{tierName}_Empty_EMI.png", $"{tierName}_Empty_EMI.jpg" };
            }
            else if (conventionName.Contains("Somna"))
            {
                exactNames = mapType == "DIF" ? 
                    new[] { $"Badge{noSpaceTier}_DIF.png", $"Badge{noSpaceTier}_DIF.jpg" } : 
                    new[] { $"Badge{noSpaceTier}_EMI.png", $"Badge{noSpaceTier}_EMI.jpg" };
            }
            else if (conventionName.Contains("Umbra"))
            {
                exactNames = new[] { $"Badge {tierName}_EMI_BLANK.png", $"Badge {tierName}_EMI_BLANK.jpg" };
            }

            foreach (var name in exactNames)
            {
                var match = allFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(name, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }

            return FindTextureMatch(allFiles, tierName, 
                mapType == "DIF" ? new[] { "_Empty", "_DIF", "BLANK" } : new[] { "_EMI", "_Empty_EMI" }, 
                mapType == "DIF" ? "MASK" : "MASK", 
                mapType == "DIF" ? "_EMI" : "_DIF");
        }

        private string FindTextureMatch(List<string> files, string tierName, string[] keywords, string exclude1, string exclude2)
        {
            string noSpaceTier = Regex.Replace(tierName, @"\s+", "");
            
            foreach (var kw in keywords)
            {
                var match = files.FirstOrDefault(f => 
                    (f.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 || f.IndexOf($"Badge{noSpaceTier}{kw}", StringComparison.OrdinalIgnoreCase) >= 0) && 
                    f.IndexOf(exclude1, StringComparison.OrdinalIgnoreCase) < 0 && 
                    f.IndexOf(exclude2, StringComparison.OrdinalIgnoreCase) < 0);
                
                if (match != null) return match;
            }
            return null;
        }

        private void CompositeTexture(string baseTexPath, MagickImage namePlate, MagickImage titlePlate, string outPath, bool applyGrayscale)
        {
            if (string.IsNullOrEmpty(baseTexPath) || !File.Exists(baseTexPath))
            {
                Debug.LogWarning($"[VixenTools] Missing base texture at: {baseTexPath}. Skipping composite.");
                return;
            }

            using MagickImage img = new MagickImage(baseTexPath);
            
            if (applyGrayscale) img.Grayscale(); 

            if (namePlate != null) img.Composite(namePlate, _nameX - (int)(namePlate.Width / 2), _nameY - (int)(namePlate.Height / 2), CompositeOperator.Over);
            if (titlePlate != null) img.Composite(titlePlate, _titleX - (int)(titlePlate.Width / 2), _titleY - (int)(titlePlate.Height / 2), CompositeOperator.Over);
            img.Write(outPath);
        }

        private MagickImage GenerateTextPlate(string font, string text, int w, int h, MagickColor color, float rotation)
        {
            if (string.IsNullOrEmpty(text)) text = " ";
            var image = new MagickImage($"label:{text}", new MagickReadSettings { BackgroundColor = MagickColors.Transparent, FillColor = color, Font = File.Exists(font) ? font : "Arial", Width = (uint)w, Height = (uint)h });
            image.Trim();
            if (rotation != 0f) { image.BackgroundColor = MagickColors.Transparent; image.Rotate(rotation); }
            return image;
        }

        private void ApplyToMaterial(string conventionName, string tierName, string difPath, string emiPath)
        {
            string tierFolder = _activeEcosystem == Ecosystem.VixenTools ?
                Path.Combine(VixenRootPath, tierName) :
                Path.Combine(FuralityRootPath, conventionName, "Avatar Assets", "Badges", tierName);

            string matFolder = Path.Combine(tierFolder, "Materials");
            if (!Directory.Exists(matFolder) && Directory.Exists(Path.Combine(tierFolder, "Material"))) matFolder = Path.Combine(tierFolder, "Material");

            if (!Directory.Exists(matFolder))
            {
                Debug.LogWarning($"[VixenTools] Could not find Material(s) folder for {tierName}. Skipping auto-apply.");
                return;
            }

            string noSpaceTier = Regex.Replace(tierName, @"\s+", "");
            var matFiles = Directory.GetFiles(matFolder, "*.mat");
            string targetMatPath = matFiles.FirstOrDefault(f => f.Contains("Attendee") || f.Contains($"Badge{noSpaceTier}") || f.Contains(tierName));
            if (targetMatPath == null && matFiles.Length > 0) targetMatPath = matFiles[0];

            if (targetMatPath != null)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(targetMatPath);
                if (material)
                {
                    // 1. Explicit Shader Assignment
                    if (_targetShader != TargetShader.AutoDetect)
                    {
                        Shader newShader = FindShaderSafely(_targetShader);
                        if (newShader != null) material.shader = newShader;
                        else Debug.LogWarning($"[VixenTools] Could not load {_targetShader}. Kept current shader.");
                    }

                    Texture2D difTex = AssetDatabase.LoadAssetAtPath<Texture2D>(difPath);
                    Texture2D emiTex = AssetDatabase.LoadAssetAtPath<Texture2D>(emiPath);

                    // 2. Universal Property Targeting
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", difTex);
                    else if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", difTex); 

                    if (material.HasProperty("_EmissionMap")) material.SetTexture("_EmissionMap", emiTex);
                    if (material.HasProperty("_EmissionStrength")) material.SetFloat("_EmissionStrength", 1f); // Poiyomi
                    if (material.HasProperty("_EnableEmission")) material.SetFloat("_EnableEmission", 1f);     // Aqua
                    if (material.HasProperty("_UseEmission")) material.SetFloat("_UseEmission", 1f);           // lilToon

                    if (material.HasProperty("_EmissionColor"))
                    {
                        Color currentEmi = material.GetColor("_EmissionColor");
                        if (currentEmi == Color.black || currentEmi.a == 0f) material.SetColor("_EmissionColor", Color.white);
                    }

                    AssetDatabase.SaveAssets();
                    Debug.Log($"[VixenTools] Auto-applied textures to {material.shader.name} material: {targetMatPath}");
                }
            }
        }

        // --- Template Authoring (VixenTools Only) ---
        private void ExecuteTemplateAuthoring()
        {
            string safeName = Regex.Replace(_newTemplateName, @"[<>:""/\\|?* ]", "_");
            string templateDir = Path.Combine(VixenRootPath, safeName);
            if (Directory.Exists(templateDir)) { Debug.LogError($"[VixenTools] Template {safeName} already exists!"); return; }

            Directory.CreateDirectory(templateDir);
            string texDir = Path.Combine(templateDir, "Textures"); Directory.CreateDirectory(texDir);
            Directory.CreateDirectory(Path.Combine(texDir, "Output"));
            string matDir = Path.Combine(templateDir, "Materials"); Directory.CreateDirectory(matDir);

            string difPath = Path.Combine(texDir, $"{safeName}_DIF.png");
            string emiPath = Path.Combine(texDir, $"{safeName}_EMI.png");

            if (_authoringType == AuthoringType.ProceduralBase)
            {
                using (MagickImage dif = new MagickImage(UnityColorToMagick(_baseTemplateColor), (uint)_templateResolution, (uint)_templateResolution)) dif.Write(difPath);
                using (MagickImage emi = new MagickImage(MagickColors.Black, (uint)_templateResolution, (uint)_templateResolution)) emi.Write(emiPath);
            }
            else
            {
                if (_sourceDiffuse != null) { using (MagickImage dif = new MagickImage(AssetDatabase.GetAssetPath(_sourceDiffuse))) { _templateResolution = (int)dif.Width; dif.Write(difPath); } } 
                if (_sourceEmission != null) { using (MagickImage emi = new MagickImage(AssetDatabase.GetAssetPath(_sourceEmission))) emi.Write(emiPath); }
                else { using (MagickImage emi = new MagickImage(MagickColors.Black, (uint)_templateResolution, (uint)_templateResolution)) emi.Write(emiPath); }
            }

            AssetDatabase.Refresh();
            SetupTextureImporter(difPath, false); SetupTextureImporter(emiPath, false); 

            // Initialize Master Material
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
            _currentMode = ToolMode.BadgeGenerator; 
        }

        private MagickColor UnityColorToMagick(Color c) { return new MagickColor((ushort)(c.r * 65535), (ushort)(c.g * 65535), (ushort)(c.b * 65535), (ushort)(c.a * 65535)); }

        #endregion

        #region Standard Utility Handlers
        private void SetupTextureImporter(string path, bool isLinear)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) return; importer.streamingMipmaps = true; importer.sRGBTexture = !isLinear; importer.SaveAndReimport();
        }
        private void LoadFontSafely()
        {
            if (!Directory.Exists(SystemFontPath)) Directory.CreateDirectory(SystemFontPath);
            string src = Path.Combine(Application.dataPath, "Vixenlicious/Editor/Fonts", FontFileName), dst = Path.Combine(SystemFontPath, FontFileName);
            if (File.Exists(src)) { File.Copy(src, dst, true); AddFontResourceEx(dst, 0, IntPtr.Zero); }
        }
        private void UnloadFonts()
        {
            string dst = Path.Combine(SystemFontPath, FontFileName);
            if (File.Exists(dst)) { RemoveFontResourceEx(dst, 0, IntPtr.Zero); try { File.Delete(dst); } catch { } }
        }
        private void SavePrefs() { /* Omitted for brevity: standard EditorPrefs calls */ }
        private void LoadPrefs() { /* Omitted for brevity: standard EditorPrefs calls */ }
        private void DrawSeparator(Color color) { Rect rect = EditorGUILayout.GetControlRect(false, 1); rect.height = 1; EditorGUI.DrawRect(rect, color); }
        #endregion
    }
}
#endif