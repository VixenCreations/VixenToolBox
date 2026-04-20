#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace VixenTools.Editor
{
    public class VixenHub : EditorWindow
    {
        private enum HubMode { Documentation, Changelog }
        private HubMode _currentMode = HubMode.Documentation;

        // VPM Package Paths
        private const string README_PATH = "Packages/com.vixencreations.vixens-toolbox/README.md";
        private const string CHANGELOG_PATH = "Packages/com.vixencreations.vixens-toolbox/CHANGELOG.md";
        private const string HEADER_IMAGE_PATH = "Packages/com.vixencreations.vixens-toolbox/Editor/Assets/New Tool Art.png";

        // --- ARTWORK FRAMING ---
        // 0.0f = Pinned to Top, 0.5f = Center, 1.0f = Pinned to Bottom. 
        // Lower values slide the artwork DOWN inside the banner frame.
        private const float HeaderImagePanY = 0.65f; 
        private const float HeaderHeight = 120f;

        // GUI State
        private Vector2 _scrollPosition;
        private string[] _readmeLines;
        private string[] _changelogLines;
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

        private void OnEnable()
        {
            LoadFiles();
        }

        private void LoadFiles()
        {
            TextAsset readmeAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(README_PATH);
            _readmeLines = readmeAsset != null ? readmeAsset.text.Split('\n') : new string[] { "<color=#ff00aa>Error: README.md not found at package root.</color>" };

            TextAsset changelogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CHANGELOG_PATH);
            _changelogLines = changelogAsset != null ? changelogAsset.text.Split('\n') : new string[] { "<color=#ff00aa>Error: CHANGELOG.md not found at package root.</color>" };

            _headerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(HEADER_IMAGE_PATH);
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
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    padding = new RectOffset(10, 10, 8, 8),
                    normal = { textColor = new Color(0f, 0.9f, 1f) }, 
                    hover = { textColor = new Color(1f, 0f, 0.66f) }  
                };
            }
        }

        private void OnGUI()
        {
            InitializeStyles();

            // --- HEADER ART ---
            DrawHeaderUI();

            GUILayout.Space(10);

            // --- MODE SWITCHER ---
            _currentMode = (HubMode)GUILayout.Toolbar((int)_currentMode, new string[] { "About & Documentation", "Ecosystem Changelog" }, GUILayout.Height(30));
            GUILayout.Space(10);

            // --- CONTENT SCROLL AREA ---
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            string[] linesToDraw = _currentMode == HubMode.Documentation ? _readmeLines : _changelogLines;
            DrawMarkdownEngine(linesToDraw);

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            // --- SOCIAL FOOTER ---
            DrawFooterUI();
        }

        private void DrawHeaderUI()
        {
            if (_headerTexture != null)
            {
                Rect headerRect = GUILayoutUtility.GetRect(0, HeaderHeight, GUILayout.ExpandWidth(true));
                
                // Calculate the framing ratio
                float screenAspect = headerRect.width / headerRect.height;
                float texAspect = (float)_headerTexture.width / _headerTexture.height;

                Rect texCoords = new Rect(0, 0, 1, 1);

                if (texAspect > screenAspect)
                {
                    // Image is wider than the banner - crop left/right evenly
                    float cropWidth = screenAspect / texAspect;
                    texCoords.width = cropWidth;
                    texCoords.x = (1f - cropWidth) * 0.5f; 
                }
                else
                {
                    // Image is taller than the banner - crop top/bottom
                    float cropHeight = texAspect / screenAspect;
                    texCoords.height = cropHeight;
                    
                    // Apply custom pan to slide the image up/down inside the frame
                    texCoords.y = (1f - cropHeight) * HeaderImagePanY; 
                }

                // Draw with custom UV coordinates
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

        private void DrawFooterUI()
        {
            GUIStyle footerStyle = new GUIStyle();
            footerStyle.normal.background = MakeTex(1, 1, new Color(0.08f, 0.04f, 0.12f));
            footerStyle.padding = new RectOffset(0, 0, 5, 5);

            EditorGUILayout.BeginHorizontal(footerStyle, GUILayout.Height(40));
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("GitHub", _socialButtonStyle, GUILayout.Height(30), GUILayout.Width(100))) 
                Application.OpenURL("https://github.com/VixenCreations");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("YouTube", _socialButtonStyle, GUILayout.Height(30), GUILayout.Width(100))) 
                Application.OpenURL("https://www.youtube.com/@vixenlicous");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("X (Twitter)", _socialButtonStyle, GUILayout.Height(30), GUILayout.Width(100))) 
                Application.OpenURL("https://x.com/VixenVRC");
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
#endif