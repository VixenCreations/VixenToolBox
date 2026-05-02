#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace VixenTools.Editor
{
    // --- AUTONOMOUS SCENE HUD NOTIFIER ---
    [InitializeOnLoad]
    public static class VixenUpdateNotifier
    {
        private const string BADGE_NAME = "vixen-update-badge";
        private const string PREF_STORED_VER = "VixenTools_StoredVersion";
        private const string PREF_UPDATE_PENDING = "VixenTools_UpdatePending";
        private const string PKG_PATH = "Packages/com.vixencreations.vixens-toolbox/package.json";

        static VixenUpdateNotifier()
        {
            // 1. Autonomous Version Detection on Domain Reload
            CheckForPackageChanges();
            
            // 2. Bind the UI injector
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void CheckForPackageChanges()
        {
            try
            {
                string path = Path.GetFullPath(PKG_PATH);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Match vMatch = Regex.Match(json, @"""version""\s*:\s*""([^""]+)""");
                    if (vMatch.Success)
                    {
                        string currentVersion = vMatch.Groups[1].Value;
                        string storedVersion = EditorPrefs.GetString(PREF_STORED_VER, "");

                        // If the version changed (or it's a fresh install), trigger the HUD
                        if (string.IsNullOrEmpty(storedVersion) || storedVersion != currentVersion)
                        {
                            EditorPrefs.SetBool(PREF_UPDATE_PENDING, true);
                            EditorPrefs.SetString(PREF_STORED_VER, currentVersion);
                        }
                    }
                }
            }
            catch { /* Fail silently to prevent disrupting editor loads if package is migrating */ }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var root = sceneView.rootVisualElement;
            if (root == null) return;

            bool updatePending = EditorPrefs.GetBool(PREF_UPDATE_PENDING, false);
            var existingBadge = root.Q<Button>(BADGE_NAME);

            if (!updatePending)
            {
                if (existingBadge != null) existingBadge.style.display = DisplayStyle.None;
                return;
            }

            if (existingBadge == null)
            {
                existingBadge = BuildCyberBadge();
                root.Add(existingBadge);
            }

            existingBadge.style.display = DisplayStyle.Flex;
        }

        private static Button BuildCyberBadge()
        {
            var badge = new Button(() => 
            {
                EditorPrefs.SetBool(PREF_UPDATE_PENDING, false);
                VixenHub.ShowChangelogWindow();
            }) 
            { 
                name = BADGE_NAME 
            };

            // --- LAYOUT & POSITIONING ---
            badge.style.position = Position.Absolute;
            badge.style.bottom = 20;
            badge.style.right = 20;
            badge.style.width = 240;
            badge.style.height = 36;

            // --- CYBERPUNK STYLING ---
            badge.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);
            
            badge.style.borderTopWidth = 0;
            badge.style.borderRightWidth = 0;
            badge.style.borderBottomWidth = 1;
            badge.style.borderLeftWidth = 4;
            
            badge.style.borderLeftColor = new Color(1f, 0f, 0.66f); // Hot Pink
            badge.style.borderBottomColor = new Color(0f, 0.9f, 1f, 0.3f); // Cyan Glow
            
            badge.style.marginLeft = 0;
            badge.style.marginRight = 0;
            badge.style.marginTop = 0;
            badge.style.marginBottom = 0;
            badge.style.paddingLeft = 0;
            badge.style.paddingRight = 0;
            badge.style.paddingTop = 0;
            badge.style.paddingBottom = 0;

            badge.style.alignItems = Align.Center;
            badge.style.justifyContent = Justify.Center;

            // --- INTERACTIVITY ---
            badge.style.transitionDuration = new List<TimeValue> { new TimeValue(0.15f) };
            badge.RegisterCallback<PointerEnterEvent>(e => badge.style.backgroundColor = new Color(0.12f, 0.12f, 0.18f, 0.95f));
            badge.RegisterCallback<PointerLeaveEvent>(e => badge.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f));

            // --- TYPOGRAPHY ---
            var label = new Label(">> <color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> UPDATE") { enableRichText = true };
            label.style.fontSize = 14;
            
            Font cyberFont = AssetDatabase.LoadAssetAtPath<Font>("Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf");
            if (cyberFont != null) label.style.unityFontDefinition = new StyleFontDefinition(cyberFont);

            badge.Add(label);

            return badge;
        }
    }

    public class VixenHub : EditorWindow
    {
        private enum TabMode { Dashboard, CoreModules, Network, Support, Changelog }
        private TabMode _currentMode = TabMode.Dashboard;

        private const string PackageRoot = "Packages/com.vixencreations.vixens-toolbox/";
        private const string FontPath = PackageRoot + "Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string UssPath = PackageRoot + "Editor/UiStyles/VixenHubStyles.uss";
        
        private Font _cyberFont;
        
        // --- Dynamic Version States ---
        private string _packageVersion = "Unknown"; 
        private string _sdkVersion = "Unknown";
        
        // --- Changelog Pagination State ---
        private class ChangelogEntry
        {
            public string VersionTitle;
            public string Content;
        }
        private List<ChangelogEntry> _changelogEntries = new List<ChangelogEntry>();
        private int _selectedChangelogIndex = 0;

        private Button _btnDashboard;
        private Button _btnCoreModules;
        private Button _btnNetwork;
        private Button _btnSupport;
        private Button _btnChangelog;
        
        private Label _tabDescription;
        private ScrollView _contentScroll;
        private VisualElement _contentContainer;

        [MenuItem("VixenTools/Hub Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenHub>("Vixen Hub");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        // --- NEW: Dedicated Changelog Launcher ---
        public static void ShowChangelogWindow()
        {
            var window = GetWindow<VixenHub>("Vixen Hub");
            window.minSize = new Vector2(450, 600);
            window.Show();
            
            // Force the UI to route to the changelog tab after initialization
            window.OpenChangelogTab(); 
        }

        // --- NEW: Public accessor to bypass private enumerators ---
        public void OpenChangelogTab()
        {
            if (_contentContainer != null) // Safety check to ensure GUI is initialized
            {
                SwitchMode(TabMode.Changelog);
            }
        }

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            LoadVersionData();
        }

        private void LoadVersionData()
        {
            string path = Path.GetFullPath(PackageRoot + "package.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                
                Match vMatch = Regex.Match(json, @"""version""\s*:\s*""([^""]+)""");
                if (vMatch.Success) _packageVersion = vMatch.Groups[1].Value;

                // THE FIX: Expanding the regex to catch com.vrchat.base, avatars, or worlds.
                Match sdkMatch = Regex.Match(json, @"""com\.vrchat\.(?:base|avatars|worlds)""\s*:\s*""([^""]+)""");
                if (sdkMatch.Success) _sdkVersion = sdkMatch.Groups[1].Value.Replace("^", "").Replace("~", "");
            }
        }

        private void ParseChangelogData()
        {
            _changelogEntries.Clear();
            string rawText = LoadMarkdownFile("CHANGELOG.md");
            string[] lines = rawText.Split('\n');

            ChangelogEntry currentEntry = null;

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();
                
                if (line.StartsWith("## ["))
                {
                    currentEntry = new ChangelogEntry { 
                        VersionTitle = line.Replace("## ", "").Trim(), 
                        Content = "" 
                    };
                    _changelogEntries.Add(currentEntry);
                }
                else if (currentEntry != null)
                {
                    currentEntry.Content += line + "\n";
                }
            }
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "hub-root";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            // --- HEADER BANNER ---
            var headerRect = new VisualElement { name = "hub-header" };
            var textContainer = new VisualElement();
            textContainer.AddToClassList("hub-header-text-container");
            textContainer.style.height = 120;

            var titleLabel = new Label("<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> HUB") { enableRichText = true };
            titleLabel.AddToClassList("hub-header-title");
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            
            string sdkText = _sdkVersion != "Unknown" ? $" • VRCSDK {_sdkVersion}" : "";
            var versionLabel = new Label($"v{_packageVersion}{sdkText} • System Online") { style = { color = new Color(0.6f, 0.6f, 0.6f) } };
            
            textContainer.Add(titleLabel);
            textContainer.Add(versionLabel);
            headerRect.Add(textContainer);
            root.Add(headerRect);

            // --- TABS NAVIGATION ---
            var tabContainer = new VisualElement { name = "tab-container" };
            
            _btnDashboard = new Button(() => SwitchMode(TabMode.Dashboard)) { text = "Ecosystem Architecture" };
            _btnCoreModules = new Button(() => SwitchMode(TabMode.CoreModules)) { text = "Core Modules" };
            _btnNetwork = new Button(() => SwitchMode(TabMode.Network)) { text = "Network Routing" };
            _btnSupport = new Button(() => SwitchMode(TabMode.Support)) { text = "Support the Developer" };
            _btnChangelog = new Button(() => SwitchMode(TabMode.Changelog)) { text = "Release Changelogs" };
            
            _btnDashboard.AddToClassList("tab-btn");
            _btnCoreModules.AddToClassList("tab-btn");
            _btnNetwork.AddToClassList("tab-btn");
            _btnSupport.AddToClassList("tab-btn");
            _btnChangelog.AddToClassList("tab-btn");

            tabContainer.Add(_btnDashboard);
            tabContainer.Add(_btnCoreModules);
            tabContainer.Add(_btnNetwork);
            tabContainer.Add(_btnSupport);
            tabContainer.Add(_btnChangelog);
            root.Add(tabContainer);

            // --- TAB DESCRIPTION BOX ---
            var descContainer = new VisualElement { name = "desc-container" };
            _tabDescription = new Label() { enableRichText = true };
            _tabDescription.AddToClassList("tab-desc-label");
            descContainer.Add(_tabDescription);
            root.Add(descContainer);

            // --- CONTENT AREA ---
            _contentScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            
            _contentScroll.style.flexGrow = 1;
            _contentScroll.style.flexShrink = 1;
            
            _contentContainer = new VisualElement();
            _contentContainer.style.flexShrink = 1;
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.paddingBottom = 40; 
            
            _contentScroll.Add(_contentContainer);
            root.Add(_contentScroll);

            SwitchMode(TabMode.Dashboard);
        }

        private void SwitchMode(TabMode mode)
        {
            _currentMode = mode;
            
            _btnDashboard.RemoveFromClassList("tab-btn-active"); _btnDashboard.AddToClassList("tab-btn-inactive");
            _btnCoreModules.RemoveFromClassList("tab-btn-active"); _btnCoreModules.AddToClassList("tab-btn-inactive");
            _btnNetwork.RemoveFromClassList("tab-btn-active"); _btnNetwork.AddToClassList("tab-btn-inactive");
            _btnSupport.RemoveFromClassList("tab-btn-active"); _btnSupport.AddToClassList("tab-btn-inactive");
            _btnChangelog.RemoveFromClassList("tab-btn-active"); _btnChangelog.AddToClassList("tab-btn-inactive");

            _contentContainer.Clear();

            switch (mode)
            {
                case TabMode.Dashboard:
                    _btnDashboard.RemoveFromClassList("tab-btn-inactive"); _btnDashboard.AddToClassList("tab-btn-active");
                    _tabDescription.text = "A comprehensive suite of custom Unity Editor utilities and automation scripts focused specifically on <b>High-Fidelity Avatar Pipeline & Topology Architecture</b>.";
                    ParseMarkdownAndInject(LoadMarkdownFile("README.md"), _contentContainer);
                    break;
                case TabMode.CoreModules:
                    _btnCoreModules.RemoveFromClassList("tab-btn-inactive"); _btnCoreModules.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Direct access to the flagship utilities that power the VixenTools ecosystem.";
                    RenderCoreModules();
                    break;
                case TabMode.Network:
                    _btnNetwork.RemoveFromClassList("tab-btn-inactive"); _btnNetwork.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Connect with the ecosystem, access open-source repositories, and monitor pipeline updates.";
                    RenderNetwork(); 
                    break;
                case TabMode.Support:
                    _btnSupport.RemoveFromClassList("tab-btn-inactive"); _btnSupport.AddToClassList("tab-btn-active");
                    _tabDescription.text = "This architecture requires immense R&D. If these tools save your sanity, consider fueling the engine.";
                    RenderSupport();
                    break;
                case TabMode.Changelog:
                    _btnChangelog.RemoveFromClassList("tab-btn-inactive"); _btnChangelog.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Review version history, architecture upgrades, and pipeline fixes.";
                    
                    if (_changelogEntries.Count == 0) ParseChangelogData();

                    if (_changelogEntries.Count > 0)
                    {
                        var controlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 15, marginTop = 10, paddingLeft = 5, paddingRight = 5 } };
                        
                        var dropLabel = new Label("Target Release:") { style = { color = new Color(0.67f, 0.67f, 0.67f), marginRight = 10, fontSize = 14 } };
                        if (_cyberFont != null) dropLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
                        controlRow.Add(dropLabel);

                        List<string> versionNames = new List<string>();
                        foreach (var log in _changelogEntries) versionNames.Add(log.VersionTitle);

                        if (_selectedChangelogIndex >= versionNames.Count) _selectedChangelogIndex = 0;

                        var dropdown = new DropdownField(versionNames, _selectedChangelogIndex);
                        dropdown.style.flexGrow = 1;
                        
                        dropdown.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
                        dropdown.style.borderTopColor = new Color(0f, 0.898f, 1f, 0.4f);
                        dropdown.style.borderBottomColor = new Color(0f, 0.898f, 1f, 0.4f);
                        dropdown.style.borderLeftColor = new Color(0f, 0.898f, 1f, 0.4f);
                        dropdown.style.borderRightColor = new Color(0f, 0.898f, 1f, 0.4f);
                        dropdown.style.color = new Color(0.9f, 0.9f, 0.9f);

                        var logContentContainer = new VisualElement();
                        
                        dropdown.RegisterValueChangedCallback(e => {
                            _selectedChangelogIndex = versionNames.IndexOf(e.newValue);
                            logContentContainer.Clear();
                            ParseMarkdownAndInject(_changelogEntries[_selectedChangelogIndex].Content, logContentContainer);
                        });

                        controlRow.Add(dropdown);
                        _contentContainer.Add(controlRow);

                        var sep = new VisualElement();
                        sep.AddToClassList("md-separator");
                        sep.style.backgroundColor = new Color(0f, 0.9f, 1f, 0.3f);
                        _contentContainer.Add(sep);

                        _contentContainer.Add(logContentContainer);

                        ParseMarkdownAndInject(_changelogEntries[_selectedChangelogIndex].Content, logContentContainer);
                    }
                    else
                    {
                        ParseMarkdownAndInject("### Error\nCould not parse changelog versions from `CHANGELOG.md`.", _contentContainer);
                    }
                    break;
            }
        }

        private string LoadMarkdownFile(string fileName)
        {
            string path = PackageRoot + fileName;
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset != null) return asset.text;
            
            string fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath)) return File.ReadAllText(fullPath);

            return $"### Error: File Not Found\nCould not locate `{fileName}` at `{PackageRoot}`. Ensure the VPM package is installed correctly.";
        }

        // =========================================================================
        // BUTTON GRIDS (Core Modules, Network, Support)
        // =========================================================================

        private void RenderCoreModules()
        {
            var universalList = new List<(System.Action action, string title, string desc)>
            {
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Unity Engine/Animation Workbench Pro"), "Animation Workbench Pro", "Visual workspace for staging, easing, and sampling complex animation curves."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Unity Engine/Pipeline Preset Manager"), "Pipeline Preset Manager", "Bulk extraction and programmatic authoring of standardized importer settings.")
            };

            RenderActionGrid("Universal Utilities", "#00e5ff", universalList);

#if !UDON
            var avatarList = new List<(System.Action action, string title, string desc)>
            {
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Badge Studio"), "Vixen Badge Studio", "High-fidelity procedural generation engine for VRChat convention badges."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Quest Conversion Engine"), "Quest Conversion Engine", "Non-destructive, high-fidelity pipeline mapping 100% of Android performance limits."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/PhysBone Topology Mapper"), "PhysBone Topology Mapper", "Automates PhysBone extraction and injection bypassing native prefab constraints."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Optimization Suite"), "Optimization Suite", "Advanced dual-pipeline validation and automated optimization engine with ImageMagick VRAM resolution.")
            };

            RenderActionGrid("Avatar Pipeline Tools", "#ff00aa", avatarList);
#elif UDON
            bool isSnapActive = EditorPrefs.GetBool("VixenTools/Scene/Live Surface Snapping", false);
            string snapTitle = isSnapActive ? "Snap To Surface [ ACTIVE ]" : "Snap To Surface [ OFF ]";

            var worldList = new List<(System.Action action, string title, string desc)>
            {
                (() => 
                {
                    EditorApplication.ExecuteMenuItem("VixenTools/Scene/Live Surface Snapping");
                    SwitchMode(TabMode.CoreModules);
                }, snapTitle, "Enterprise-grade surface snapping tool. Calculates true mesh/collider bounds and safely dirties Udon components.")
            };

            RenderActionGrid("World Pipeline Tools", "#ff00aa", worldList);
#endif
        }

        private void RenderNetwork()
        {
            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://github.com/VixenCreations/VixenToolBox"), "GitHub Repository", "Core ecosystem source code and release tracking."),
                (() => Application.OpenURL("https://x.com/VixenVRC"), "Twitter", "Where I post All Kinds of things and Interact with the community."),
                (() => Application.OpenURL("https://discord.com/invite/3vbJCKcPtJ"), "Discord", "My Official Community to get help with things."),
                (() => Application.OpenURL("https://github.com/VixenCreations/VixenToolBox/issues"), "Report An Issue", "Report an Issue or Request a new feature."),
                (() => Application.OpenURL("https://www.youtube.com/@vixenlicous"), "YouTube Channel", "Technical breakdowns, pipeline tutorials, and development logs.")
            };

            RenderActionGrid("Ecosystem Routing", "#00e5ff", list);
        }

        private void RenderSupport()
        {
            string markdown = @"
If my code has ever saved your scene from completely bricking, optimized your Quest fallback in under 10 seconds, or just made your workflow suck a little bit less... consider throwing a coffee my way:
";
            ParseMarkdownAndInject(markdown, _contentContainer);

            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://ko-fi.com/vixenlicous"), "Ko-Fi Donation", "One-Time Support"),
                (() => Application.OpenURL("https://vixenlicous.gumroad.com/coffee"), "Gumroad Donation", "One-Time Donation"),
                (() => Application.OpenURL("https://cash.app/$VixenVRC"), "CashApp Donation", "Direct Transfer"),
                (() => Application.OpenURL("https://www.patreon.com/cw/Vixenlicious"), "Patreon Donation", "Monthly Pipeline Fuel"),
                (() => Application.OpenURL("https://jinxxy.com/Vixenlicious"), "Jinxxy", "Full Asset Storefront"),
                (() => Application.OpenURL("https://vixenlicous.gumroad.com/"), "Gumroad", "Full Asset Storefront")
            };

            RenderActionGrid("Support The Architecture", "#ff00aa", list);
        }

        private void RenderActionGrid(string headerText, string accentHex, List<(System.Action action, string title, string desc)> items)
        {
            var header = new Label($"<color={accentHex}>{headerText}</color>") { enableRichText = true };
            header.AddToClassList("md-h1");
            if (_cyberFont != null) header.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            _contentContainer.Add(header);

            var sep = new VisualElement();
            sep.AddToClassList("md-separator");
            ColorUtility.TryParseHtmlString(accentHex, out Color c);
            c.a = 0.3f;
            sep.style.backgroundColor = c;
            _contentContainer.Add(sep);

            var grid = new VisualElement();
            grid.AddToClassList("link-grid");

            foreach (var item in items)
            {
                var card = new VisualElement();
                card.AddToClassList("link-card");

                var btn = new Button(item.action) { text = item.title };
                btn.AddToClassList("link-card-btn");

                var desc = new Label(item.desc) { enableRichText = true };
                desc.AddToClassList("link-card-desc");

                card.Add(btn);
                card.Add(desc);
                grid.Add(card);
            }

            _contentContainer.Add(grid);
        }

        // =========================================================================
        // MARKDOWN PARSER (Dashboard & Changelog)
        // =========================================================================

        private void ParseMarkdownAndInject(string text, VisualElement container)
        {
            string[] lines = text.Split('\n');

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.Contains("[ Network Links ]") || line.Contains("Network Links")) break;

                if (line.StartsWith("---") || line.StartsWith("***"))
                {
                    var sep = new VisualElement();
                    sep.AddToClassList("md-separator");
                    sep.style.backgroundColor = new Color(1f, 0f, 0.66f, 0.3f);
                    container.Add(sep);
                }
                else if (line.StartsWith("# ") || line.StartsWith("## ") || line.StartsWith("### "))
                {
                    int hashes = 0;
                    while (hashes < line.Length && line[hashes] == '#') hashes++;
                    
                    string hText = line.Substring(hashes).Trim();
                    hText = ParseMarkdownFormatting(hText);
                    var lbl = new Label(hText) { enableRichText = true };
                    
                    if (hashes == 1) lbl.AddToClassList("md-h1");
                    else if (hashes == 2) lbl.AddToClassList("md-h2");
                    else lbl.AddToClassList("md-h3");
                    
                    if (_cyberFont != null) lbl.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
                    container.Add(lbl);
                    
                    if (hashes <= 2)
                    {
                        var sep = new VisualElement();
                        sep.AddToClassList("md-separator");
                        sep.style.backgroundColor = new Color(0f, 0.9f, 1f, 0.3f);
                        container.Add(sep);
                    }
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    string pText = line.Substring(2).Trim();
                    pText = ParseMarkdownFormatting(pText);
                    
                    var row = new VisualElement(); 
                    row.AddToClassList("md-row"); 
                    
                    row.style.width = new StyleLength(Length.Percent(100)); 
                    row.style.maxWidth = new StyleLength(Length.Percent(100)); 
                    row.style.flexShrink = 1;
                    
                    var bullet = new Label(">>") { style = { color = new Color(1f, 0f, 0.66f) } };
                    bullet.AddToClassList("md-bullet");
                    bullet.style.flexShrink = 0; 
                    
                    var lbl = new Label(pText) { enableRichText = true };
                    lbl.AddToClassList("md-p");
                    
                    lbl.style.whiteSpace = WhiteSpace.Normal; 
                    lbl.style.flexShrink = 1; 
                    lbl.style.flexGrow = 1;   
                    lbl.style.width = new StyleLength(Length.Percent(100)); 
                    lbl.style.maxWidth = new StyleLength(Length.Percent(100)); 
                    
                    row.Add(bullet);
                    row.Add(lbl);
                    container.Add(row);
                }
                else if (line.StartsWith("> "))
                {
                    string pText = line.Substring(2).Trim();
                    pText = ParseMarkdownFormatting(pText);
                    var lbl = new Label(pText) { enableRichText = true };
                    lbl.AddToClassList("md-p");
                    
                    lbl.style.whiteSpace = WhiteSpace.Normal;
                    lbl.style.flexShrink = 1;
                    lbl.style.width = new StyleLength(Length.Percent(100));
                    lbl.style.maxWidth = new StyleLength(Length.Percent(100));
                    
                    lbl.style.color = new Color(0.7f, 0.7f, 0.7f);
                    lbl.style.borderLeftWidth = 3;
                    lbl.style.borderLeftColor = new Color(0f, 0.9f, 1f);
                    lbl.style.paddingLeft = 10;
                    lbl.style.marginLeft = 5;
                    lbl.style.marginTop = 5;
                    lbl.style.marginBottom = 5;
                    container.Add(lbl);
                }
                else
                {
                    string pText = ParseMarkdownFormatting(line);
                    var lbl = new Label(pText) { enableRichText = true };
                    
                    if (pText.StartsWith("<b>")) lbl.AddToClassList("md-h2");
                    else lbl.AddToClassList("md-p");
                    
                    container.Add(lbl);
                }
            }
        }

        private string ParseMarkdownFormatting(string text)
        {
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b><color=#00e5ff>$1</color></b>");
            text = Regex.Replace(text, @"\*(.*?)\*", "<i>$1</i>");
            text = Regex.Replace(text, @"\`(.*?)\`", "<color=#ffaa00><i>$1</i></color>");
            text = Regex.Replace(text, @"\[(.*?)\]\(.*?\)", "<b><color=#ff00aa>$1</color></b>");
            return text;
        }
    }
}
#endif