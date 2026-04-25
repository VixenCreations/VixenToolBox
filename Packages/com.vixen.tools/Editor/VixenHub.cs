#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Text.RegularExpressions;

namespace VixenTools.Editor
{
    // --- AUTONOMOUS SCENE HUD NOTIFIER ---
    [InitializeOnLoad]
    public static class VixenUpdateNotifier
    {
        static VixenUpdateNotifier()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!EditorPrefs.GetBool("VixenTools_UpdatePending", false)) return;

            Handles.BeginGUI();
            
            float width = 240f;
            float height = 36f;
            Rect r = new Rect(sceneView.position.width - width - 20, sceneView.position.height - height - 20, width, height);
            
            EditorGUI.DrawRect(r, new Color(0.05f, 0.05f, 0.08f, 0.95f)); 
            EditorGUI.DrawRect(new Rect(r.x, r.y, 4, r.height), new Color(1f, 0f, 0.66f)); 
            EditorGUI.DrawRect(new Rect(r.x, r.y + r.height - 1, r.width, 1), new Color(0f, 0.9f, 1f, 0.3f)); 

            Font cyberFont = AssetDatabase.LoadAssetAtPath<Font>("Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf");

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = new Color(0f, 0.9f, 1f) },
                hover = { textColor = new Color(1f, 0f, 0.66f) },
                fontStyle = cyberFont != null ? FontStyle.Normal : FontStyle.Bold,
                fontSize = cyberFont != null ? 14 : 12,
                alignment = TextAnchor.MiddleCenter,
                font = cyberFont
            };

            // Replaced the emoji with terminal syntax
            if (GUI.Button(r, ">> VIXENTOOLS UPDATE", btnStyle))
            {
                EditorPrefs.SetBool("VixenTools_UpdatePending", false);
                VixenHub.ShowWindow();
            }

            Handles.EndGUI();
        }
    }

    public class VixenHub : EditorWindow
    {
        private enum HubMode { Documentation, Changelog, Donation, SocialMedia, GetSupport }
        private HubMode _currentMode = HubMode.Documentation;

        // VPM Package Paths
        private const string PACKAGE_JSON_PATH = "Packages/com.vixencreations.vixens-toolbox/package.json";
        private const string README_PATH = "Packages/com.vixencreations.vixens-toolbox/README.md";
        private const string CHANGELOG_PATH = "Packages/com.vixencreations.vixens-toolbox/CHANGELOG.md";
        private const string SUPPORT_PATH = "Packages/com.vixencreations.vixens-toolbox/SUPPORT.md";
        private const string HEADER_IMAGE_PATH = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/New Tool Art.png";
        private const string CYBER_FONT_PATH = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string USS_PATH = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenHubStyles.uss"; 

        // Data Models
        private string[] _readmeLines;
        private string[] _changelogLines;
        private string[] _supportLines;
        private Texture2D _headerTexture;
        private Font _cyberFont;

        // UI Elements
        private VisualElement _contentContainer;
        private Label _tabDescriptionLabel;
        private VisualElement _tabContainer;

        [MenuItem("VixenTools/Vixen Hub", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenHub>("Vixen Hub");
            window.minSize = new Vector2(550, 650);
            EditorPrefs.SetBool("VixenTools_UpdatePending", false);
            window.Show();
        }

        #region Auto-Update Prompter
        [InitializeOnLoadMethod]
        private static void RunVersionCheck()
        {
            EditorApplication.delayCall += () => 
            {
                if (!File.Exists(PACKAGE_JSON_PATH)) return;
                
                string pkgJson = File.ReadAllText(PACKAGE_JSON_PATH);
                var match = Regex.Match(pkgJson, @"\""version\""\s*:\s*\""(.*?)\""");
                if (!match.Success) return;
                
                string newVersion = match.Groups[1].Value;
                string projectPath = Application.dataPath.Replace('/', '_').Replace(':', '_');
                string prefsKey = "VixenTools_InstalledVersion_" + projectPath;
                string savedVersion = EditorPrefs.GetString(prefsKey, "0.0.0");
                
                if (savedVersion == "0.0.0" || savedVersion != newVersion)
                {
                    EditorPrefs.SetString(prefsKey, newVersion);
                    EditorPrefs.SetBool("VixenTools_UpdatePending", true);
                }
            };
        }
        #endregion

        private void OnEnable()
        {
            LoadFiles();
        }

        private void LoadFiles()
        {
            _readmeLines = LoadMarkdownFile(README_PATH, "README.md");
            _changelogLines = LoadMarkdownFile(CHANGELOG_PATH, "CHANGELOG.md");
            _supportLines = LoadMarkdownFile(SUPPORT_PATH, "SUPPORT.md");
            _headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(HEADER_IMAGE_PATH);
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(CYBER_FONT_PATH);
        }

        private string[] LoadMarkdownFile(string path, string name)
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            return asset != null ? asset.text.Split('\n') : new string[] { $"<color=#ff00aa>Error: {name} not found at package root.</color>" };
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "hub-root";

            // Load USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);
            else Debug.LogWarning($"[VixenTools] Could not load VixenHubStyles.uss at {USS_PATH}");

            // --- HEADER ---
            var headerRect = new VisualElement { name = "hub-header" };
            if (_headerTexture != null)
            {
                headerRect.style.backgroundImage = new StyleBackground(_headerTexture);
                headerRect.AddToClassList("hub-header-image");
            }
            else
            {
                headerRect.AddToClassList("hub-header-text-container");
                var titleLabel = new Label("<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> HUB");
                titleLabel.enableRichText = true;
                titleLabel.AddToClassList("hub-header-title");
                if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
                headerRect.Add(titleLabel);
            }
            root.Add(headerRect);

            // --- TABS ---
            _tabContainer = new VisualElement { name = "tab-container" };
            root.Add(_tabContainer);

            string[] tabNames = { "Documentation", "Changelog", "Donation", "Social Media", "Get Support" };
            for (int i = 0; i < tabNames.Length; i++)
            {
                int index = i;
                var btn = new Button(() => SwitchTab((HubMode)index)) { text = tabNames[i] };
                btn.AddToClassList("tab-btn");
                _tabContainer.Add(btn);
            }

            // --- TAB DESCRIPTION ---
            var descContainer = new VisualElement { name = "desc-container" };
            _tabDescriptionLabel = new Label();
            _tabDescriptionLabel.enableRichText = true;
            _tabDescriptionLabel.AddToClassList("tab-desc-label");
            descContainer.Add(_tabDescriptionLabel);
            root.Add(descContainer);

            // --- SCROLL CONTENT ---
            var scrollContainer = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            _contentContainer = new VisualElement();
            scrollContainer.Add(_contentContainer);
            root.Add(scrollContainer);

            SwitchTab(_currentMode);
        }

        private void SwitchTab(HubMode newMode)
        {
            _currentMode = newMode;

            // Update tab button styles
            for (int i = 0; i < _tabContainer.childCount; i++)
            {
                var btn = _tabContainer[i] as Button;
                if (btn != null)
                {
                    if (i == (int)_currentMode)
                    {
                        btn.AddToClassList("tab-btn-active");
                        btn.RemoveFromClassList("tab-btn-inactive");
                    }
                    else
                    {
                        btn.AddToClassList("tab-btn-inactive");
                        btn.RemoveFromClassList("tab-btn-active");
                    }
                }
            }

            // Update description
            string desc = "";
            switch (_currentMode)
            {
                case HubMode.Documentation: desc = "Official ecosystem documentation and pipeline manuals."; break;
                case HubMode.Changelog: desc = "Recent architectural upgrades, bug fixes, and patch notes."; break;
                case HubMode.Donation: desc = "Fuel the VixenTools engine. Your support keeps the pipelines running."; break;
                case HubMode.SocialMedia: desc = "Connect with the developer and stay updated on new releases."; break;
                case HubMode.GetSupport: desc = "Report anomalies, request features, or consult the Wiki."; break;
            }
            _tabDescriptionLabel.text = $"<color=#00e5ff>::</color> <i>{desc}</i>";

            // Rebuild Content
            _contentContainer.Clear();
            switch (_currentMode)
            {
                case HubMode.Documentation: RenderMarkdown(_readmeLines); break;
                case HubMode.Changelog: RenderMarkdown(_changelogLines); break;
                case HubMode.Donation: 
                    RenderMarkdown(_supportLines); 
                    RenderLinkCards("Direct Support Links", "#ff00aa", new[] {
                        ("Ko-Fi", "https://ko-fi.com/vixenlicous", "Support the developer with a one-time coffee tip."),
                        ("Gumroad Store", "https://vixencreations.gumroad.com/", "Explore all available tools, assets, and support tiers."),
                        ("Gumroad Donation", "https://vixencreations.gumroad.com/coffee", "Fuel the engine directly via the Gumroad ecosystem.")
                    });
                    break;
                case HubMode.SocialMedia: 
                    RenderLinkCards("VixenTools Network", "#00e5ff", new[] {
                        ("Twitter (X)", "https://x.com/VixenVRC", "Follow for the latest pipeline updates, architectural teases, and VRChat development."),
                        ("YouTube", "https://www.youtube.com/@vixenlicous", "In-depth video documentation, pipeline tutorials, and visual guides."),
                        ("GitHub", "https://github.com/VixenCreations/VixenToolBox", "The core code repository and automated VPM distribution hub.")
                    });
                    break;
                case HubMode.GetSupport: 
                    RenderLinkCards("Diagnostic & Expansion Triage", "#00e5ff", new[] {
                        ("VixenTools Discord", "https://discord.gg/3vbJCKcPtJ", "Join the central matrix for live pipeline support, architectural upgrades, and community troubleshooting."),
                        ("Report an Issue", "https://github.com/VixenCreations/VixenToolBox/issues", "Encountered a bug or an anomaly in the matrix? File a diagnostic report here."),
                        ("Request a Feature", "https://github.com/VixenCreations/VixenToolBox/issues", "Have an idea for a new architectural upgrade? Submit a feature request."),
                        ("Ecosystem Wiki", "https://github.com/VixenCreations/VixenToolBox/wiki", "Deep-dive into the advanced mechanics and documentation of the VixenTools pipelines.")
                    });
                    break;
            }
        }

        private void RenderMarkdown(string[] lines)
        {
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    var spacer = new VisualElement();
                    spacer.style.height = 10;
                    _contentContainer.Add(spacer);
                    continue;
                }

                if (line.StartsWith("---") || line.StartsWith("***"))
                {
                    var sep = new VisualElement();
                    sep.AddToClassList("md-separator");
                    sep.style.backgroundColor = new Color(1f, 0f, 0.66f, 0.3f);
                    _contentContainer.Add(sep);
                    continue;
                }

                if (line.StartsWith("# "))
                {
                    var lbl = new Label(ParseMarkdownFormatting(line.Substring(2))) { enableRichText = true };
                    lbl.AddToClassList("md-h1");
                    if (_cyberFont != null) lbl.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
                    _contentContainer.Add(lbl);

                    var sep = new VisualElement();
                    sep.AddToClassList("md-separator");
                    sep.style.backgroundColor = new Color(0f, 0.9f, 1f, 0.3f);
                    _contentContainer.Add(sep);
                }
                else if (line.StartsWith("## "))
                {
                    var lbl = new Label(ParseMarkdownFormatting(line.Substring(3))) { enableRichText = true };
                    lbl.AddToClassList("md-h2");
                    if (_cyberFont != null) lbl.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
                    _contentContainer.Add(lbl);
                }
                else if (line.StartsWith("### "))
                {
                    var lbl = new Label(ParseMarkdownFormatting(line.Substring(4))) { enableRichText = true };
                    lbl.AddToClassList("md-h3");
                    _contentContainer.Add(lbl);
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    
                    var bullet = new Label("<color=#00e5ff>></color>") { enableRichText = true };
                    bullet.AddToClassList("md-bullet");
                    
                    var lbl = new Label(ParseMarkdownFormatting(line.Substring(2))) { enableRichText = true };
                    lbl.AddToClassList("md-p");
                    
                    row.Add(bullet);
                    row.Add(lbl);
                    _contentContainer.Add(row);
                }
                else
                {
                    var lbl = new Label(ParseMarkdownFormatting(line)) { enableRichText = true };
                    lbl.AddToClassList("md-p");
                    _contentContainer.Add(lbl);
                }
            }
        }

        private void RenderLinkCards(string headerTitle, string accentHex, (string title, string url, string desc)[] links)
        {
            var spacer = new VisualElement();
            spacer.style.height = 20;
            _contentContainer.Add(spacer);

            var header = new Label($"<color={accentHex}>{headerTitle}</color>") { enableRichText = true };
            header.AddToClassList("md-h2");
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

            foreach (var link in links)
            {
                var card = new VisualElement();
                card.AddToClassList("link-card");

                var btn = new Button(() => Application.OpenURL(link.url)) { text = link.title };
                btn.AddToClassList("link-card-btn");

                var desc = new Label(link.desc) { enableRichText = true };
                desc.AddToClassList("link-card-desc");

                card.Add(btn);
                card.Add(desc);
                grid.Add(card);
            }

            _contentContainer.Add(grid);
        }

        private string ParseMarkdownFormatting(string text)
        {
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");
            text = Regex.Replace(text, @"\*(.*?)\*", "<i>$1</i>");
            text = Regex.Replace(text, @"\`(.*?)\`", "<color=#00e5ff>$1</color>");
            text = Regex.Replace(text, @"\[(.*?)\]\(.*?\)", "<b><color=#00e5ff>$1</color></b>");
            return text;
        }
    }
}
#endif