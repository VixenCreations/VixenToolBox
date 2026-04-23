#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: A lightweight ScriptableObject used to track the currently installed version
    /// and trigger the Hub popup when an update is detected.
    /// </summary>
    public class VixenVersionAsset : ScriptableObject
    {
        public string currentVersion = "0.0.0";
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
        private const string HEADER_IMAGE_PATH = "Packages/com.vixencreations.vixens-toolbox/Editor/Assets/New Tool Art.png";

        // --- ARTWORK FRAMING ---
        private const float HeaderImagePanY = 0.65f; 
        private const float HeaderHeight = 120f;

        // GUI State
        private Vector2 _scrollPosition;
        private string[] _readmeLines;
        private string[] _changelogLines;
        private string[] _supportLines;
        private GUIStyle _richTextStyle;
        private GUIStyle _socialButtonStyle;
        private Texture2D _headerTexture;

        [MenuItem("VixenTools/Vixen Hub", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenHub>("Vixen Hub");
            window.minSize = new Vector2(550, 650);
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
                
                string dir = "Assets/VixenTools/VersionCheck";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    AssetDatabase.Refresh();
                }
                
                string assetPath = $"{dir}/VersionIs.asset";
                VixenVersionAsset versionAsset = AssetDatabase.LoadAssetAtPath<VixenVersionAsset>(assetPath);
                
                if (versionAsset == null)
                {
                    versionAsset = ScriptableObject.CreateInstance<VixenVersionAsset>();
                    versionAsset.currentVersion = newVersion;
                    AssetDatabase.CreateAsset(versionAsset, assetPath);
                    AssetDatabase.SaveAssets();
                    ShowWindow(); // Prompt on fresh install
                }
                else if (versionAsset.currentVersion != newVersion)
                {
                    versionAsset.currentVersion = newVersion;
                    EditorUtility.SetDirty(versionAsset);
                    AssetDatabase.SaveAssets();
                    ShowWindow(); // Prompt on package update
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
        }

        private string[] LoadMarkdownFile(string path, string name)
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            return asset != null ? asset.text.Split('\n') : new string[] { $"<color=#ff00aa>Error: {name} not found at package root.</color>" };
        }

        private void InitializeStyles()
        {
            if (_richTextStyle == null)
            {
                _richTextStyle = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    wordWrap = true,
                    fontSize = 13,
                    margin = new RectOffset(10, 10, 2, 2)
                };
            }

            if (_socialButtonStyle == null)
            {
                _socialButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    padding = new RectOffset(10, 10, 10, 10),
                    normal = { textColor = new Color(0f, 0.9f, 1f) }, 
                    hover = { textColor = new Color(1f, 0f, 0.66f) }  
                };
            }
        }

        private void OnGUI()
        {
            InitializeStyles();
            DrawHeaderUI();
            GUILayout.Space(10);

            // --- TAB MATRIX ---
            string[] tabs = { "Documentation", "Changelog", "Donation", "Social Media", "Get Support" };
            _currentMode = (HubMode)GUILayout.SelectionGrid((int)_currentMode, tabs, 3, GUILayout.Height(65));
            
            DrawTabDescription();

            // --- CONTENT SCROLL AREA ---
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            switch (_currentMode)
            {
                case HubMode.Documentation:
                    DrawMarkdownEngine(_readmeLines);
                    break;
                case HubMode.Changelog:
                    DrawMarkdownEngine(_changelogLines);
                    break;
                case HubMode.Donation:
                    DrawMarkdownEngine(_supportLines);
                    DrawDonationLinks();
                    break;
                case HubMode.SocialMedia:
                    DrawSocialMediaLinks();
                    break;
                case HubMode.GetSupport:
                    DrawGetSupportLinks();
                    break;
            }

            GUILayout.EndScrollView();
        }

        private void DrawTabDescription()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            string desc = "";
            switch (_currentMode)
            {
                case HubMode.Documentation: desc = "Official ecosystem documentation and pipeline manuals."; break;
                case HubMode.Changelog: desc = "Recent architectural upgrades, bug fixes, and patch notes."; break;
                case HubMode.Donation: desc = "Fuel the VixenTools engine. Your support keeps the pipelines running."; break;
                case HubMode.SocialMedia: desc = "Connect with the developer and stay updated on new releases."; break;
                case HubMode.GetSupport: desc = "Report anomalies, request features, or consult the Wiki."; break;
            }
            EditorGUILayout.LabelField($"<color=#00e5ff>■</color> <i>{desc}</i>", new GUIStyle(EditorStyles.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        #region Link Renderers
        private void DrawDonationLinks()
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("<size=18><color=#ff00aa><b>Direct Support Links</b></color></size>", _richTextStyle);
            DrawSeparator(new Color(1f, 0f, 0.66f, 0.3f));
            GUILayout.Space(10);

            DrawLinkAction("Ko-Fi", "https://ko-fi.com/vixenlicous", "Support the developer with a one-time coffee tip.");
            DrawLinkAction("Gumroad Store", "https://vixencreations.gumroad.com/", "Explore all available tools, assets, and support tiers.");
            DrawLinkAction("Gumroad Donation", "https://vixencreations.gumroad.com/coffee", "Fuel the engine directly via the Gumroad ecosystem.");
        }

        private void DrawSocialMediaLinks()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("<size=18><color=#00e5ff><b>VixenTools Network</b></color></size>", _richTextStyle);
            DrawSeparator(new Color(0f, 0.9f, 1f, 0.3f));
            GUILayout.Space(10);

            DrawLinkAction("Twitter (X)", "https://x.com/VixenVRC", "Follow for the latest pipeline updates, architectural teases, and VRChat development.");
            DrawLinkAction("YouTube", "https://www.youtube.com/@vixenlicous", "In-depth video documentation, pipeline tutorials, and visual guides.");
            DrawLinkAction("GitHub", "https://github.com/VixenCreations/VixenToolBox", "The core code repository and automated VPM distribution hub.");
        }

        private void DrawGetSupportLinks()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("<size=18><color=#00e5ff><b>Diagnostic & Expansion Triage</b></color></size>", _richTextStyle);
            DrawSeparator(new Color(0f, 0.9f, 1f, 0.3f));
            GUILayout.Space(10);

            DrawLinkAction("VixenTools Discord", "https://discord.gg/3vbJCKcPtJ", "Join the central matrix for live pipeline support, architectural upgrades, and community troubleshooting.");
            DrawLinkAction("Report an Issue", "https://github.com/VixenCreations/VixenToolBox/issues", "Encountered a bug or an anomaly in the matrix? File a diagnostic report here.");
            DrawLinkAction("Request a Feature", "https://github.com/VixenCreations/VixenToolBox/issues", "Have an idea for a new architectural upgrade? Submit a feature request.");
            DrawLinkAction("Ecosystem Wiki", "https://github.com/VixenCreations/VixenToolBox/wiki", "Deep-dive into the advanced mechanics and documentation of the VixenTools pipelines.");
        }

        private void DrawLinkAction(string label, string url, string subtext)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            if (GUILayout.Button(label, _socialButtonStyle, GUILayout.Height(35)))
            {
                Application.OpenURL(url);
            }
            GUILayout.Space(2);
            EditorGUILayout.LabelField(subtext, new GUIStyle(EditorStyles.miniLabel) { wordWrap = true, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
        #endregion

        private void DrawHeaderUI()
        {
            if (_headerTexture != null)
            {
                Rect headerRect = GUILayoutUtility.GetRect(0, HeaderHeight, GUILayout.ExpandWidth(true));
                float screenAspect = headerRect.width / headerRect.height;
                float texAspect = (float)_headerTexture.width / _headerTexture.height;
                Rect texCoords = new Rect(0, 0, 1, 1);

                if (texAspect > screenAspect)
                {
                    float cropWidth = screenAspect / texAspect;
                    texCoords.width = cropWidth;
                    texCoords.x = (1f - cropWidth) * 0.5f; 
                }
                else
                {
                    float cropHeight = texAspect / screenAspect;
                    texCoords.height = cropHeight;
                    texCoords.y = (1f - cropHeight) * HeaderImagePanY; 
                }
                GUI.DrawTextureWithTexCoords(headerRect, _headerTexture, texCoords, true);
            }
            else
            {
                Rect headerRect = EditorGUILayout.GetControlRect(false, 50);
                EditorGUI.DrawRect(headerRect, new Color(0.08f, 0.04f, 0.12f));
                GUIStyle hubTitleStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true, alignment = TextAnchor.MiddleCenter, fontSize = 20 };
                EditorGUI.LabelField(headerRect, "<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> HUB", hubTitleStyle);
            }
        }

        private void DrawMarkdownEngine(string[] lines)
        {
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    GUILayout.Space(10);
                    continue;
                }

                if (line.StartsWith("---") || line.StartsWith("***"))
                {
                    DrawSeparator(new Color(1f, 0f, 0.66f, 0.3f)); 
                    continue;
                }

                if (line.StartsWith("# "))
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField($"<size=18><color=#00e5ff><b>{ParseMarkdownFormatting(line.Substring(2))}</b></color></size>", _richTextStyle);
                    DrawSeparator(new Color(0f, 0.9f, 1f, 0.3f)); 
                    GUILayout.Space(5);
                }
                else if (line.StartsWith("## "))
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField($"<size=16><color=#ff00aa><b>{ParseMarkdownFormatting(line.Substring(3))}</b></color></size>", _richTextStyle);
                }
                else if (line.StartsWith("### "))
                {
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField($"<size=14><color=#ffffff><b>{ParseMarkdownFormatting(line.Substring(4))}</b></color></size>", _richTextStyle);
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    string parsedLine = ParseMarkdownFormatting(line.Substring(2));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField($"<color=#00e5ff>■</color>  {parsedLine}", _richTextStyle);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField(ParseMarkdownFormatting(line), _richTextStyle);
                }
            }
        }

        private string ParseMarkdownFormatting(string text)
        {
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");
            text = Regex.Replace(text, @"\*(.*?)\*", "<i>$1</i>");
            text = Regex.Replace(text, @"\`(.*?)\`", "<color=#00e5ff>$1</color>");
            text = Regex.Replace(text, @"\[(.*?)\]\(.*?\)", "<color=#00e5ff>$1</color>");
            return text;
        }

        private void DrawSeparator(Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, color);
        }
    }
}
#endif