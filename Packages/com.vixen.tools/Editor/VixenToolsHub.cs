#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: Centralized hub that dynamically reads the package README.md 
    /// and provides persistent routing links.
    /// </summary>
    public class VixenToolsHub : EditorWindow
    {
        private const string GITHUB_URL = "https://github.com/VixenCreations";
        private const string YOUTUBE_URL = "https://www.youtube.com/@vixenlicous";
        private const string X_URL = "https://x.com/VixenVRC";

        // Path to the README relative to the package root
        private const string README_PATH = "Packages/com.vixencreations.vixens-toolbox/README.md";

        private Vector2 scrollPosition;
        private string[] readmeLines;
        private GUIStyle richTextStyle;

        [MenuItem("VixenTools/About & Links")]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenToolsHub>("VixenTools Hub");
            // Expanded size to comfortably read the documentation
            window.minSize = new Vector2(550, 600);
            window.Show();
        }

        private void OnEnable()
        {
            LoadReadme();
        }

        private void LoadReadme()
        {
            TextAsset readmeAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(README_PATH);
            
            if (readmeAsset != null)
            {
                readmeLines = readmeAsset.text.Split('\n');
            }
            else
            {
                readmeLines = new string[] 
                { 
                    "<color=#ff00aa><b>[ERROR]</b></color> Could not locate README.md.", 
                    $"Expected path: {README_PATH}",
                    "Ensure the package is properly installed via VCC." 
                };
            }
        }

        private void OnGUI()
        {
            if (richTextStyle == null)
            {
                richTextStyle = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    wordWrap = true,
                    fontSize = 13,
                    padding = new RectOffset(10, 10, 2, 2)
                };
            }

            // --- HEADER NAVIGATION BAR ---
            Rect headerRect = EditorGUILayout.GetControlRect(false, 50);
            EditorGUI.DrawRect(headerRect, new Color(0.08f, 0.04f, 0.12f)); 
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20
            };
            EditorGUI.LabelField(headerRect, "<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> HUB", headerStyle);

            GUILayout.Space(5);

            // Horizontal Routing Buttons
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.8f); // Cyan
            if (GUILayout.Button("GitHub Repository", GUILayout.Height(30))) Application.OpenURL(GITHUB_URL);
            
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.5f); // Pink
            if (GUILayout.Button("YouTube Channel", GUILayout.Height(30))) Application.OpenURL(YOUTUBE_URL);
            
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f); // Dark Charcoal
            if (GUILayout.Button("X (Twitter)", GUILayout.Height(30))) Application.OpenURL(X_URL);
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.white; // Reset colors
            GUILayout.Space(10);
            DrawSeparator(new Color(0.5f, 0.5f, 0.5f, 0.5f));
            GUILayout.Space(5);

            // --- SCROLLABLE README CONTENT ---
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (readmeLines != null)
            {
                foreach (string line in readmeLines)
                {
                    string trimmedLine = line.TrimEnd('\r', '\n');
                    
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        GUILayout.Space(8);
                        continue;
                    }

                    RenderMarkdownLine(trimmedLine);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void RenderMarkdownLine(string line)
        {
            // Skip rendering markdown images and web badges (e.g., [![VPM-Ready](...)])
            if (line.Contains("![") || line.Contains("[![")) return;
            // Skip the raw horizontal rules (we draw our own)
            if (line == "---" || line == "***") 
            {
                GUILayout.Space(10);
                DrawSeparator(new Color(0.5f, 0.5f, 0.5f, 0.3f));
                GUILayout.Space(10);
                return;
            }

            // H1
            if (line.StartsWith("# "))
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField($"<color=#ff00aa><size=18><b>{line.Substring(2)}</b></size></color>", richTextStyle);
                DrawSeparator(new Color(1f, 0f, 0.66f, 0.3f));
            }
            // H2
            else if (line.StartsWith("## "))
            {
                GUILayout.Space(15);
                EditorGUILayout.LabelField($"<color=#00e5ff><size=15><b>{line.Substring(3)}</b></size></color>", richTextStyle);
                DrawSeparator(new Color(0f, 0.9f, 1f, 0.2f));
            }
            // H3
            else if (line.StartsWith("### "))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField($"<color=#bfa8d2><b>{line.Substring(4)}</b></color>", richTextStyle);
            }
            // Bullet Points
            else if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                string parsedLine = ParseMarkdownFormatting(line.Substring(2));
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.LabelField($"<color=#00e5ff>■</color>  {parsedLine}", richTextStyle);
                EditorGUILayout.EndHorizontal();
            }
            // Standard Text
            else
            {
                string parsedLine = ParseMarkdownFormatting(line);
                EditorGUILayout.LabelField(parsedLine, richTextStyle);
            }
        }

        private string ParseMarkdownFormatting(string text)
        {
            // Bold (**text**)
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");
            
            // Italic (*text*)
            text = Regex.Replace(text, @"\*(.*?)\*", "<i>$1</i>");
            
            // Inline Code (`text`) -> Tinted Cyan
            text = Regex.Replace(text, @"\`(.*?)\`", "<color=#00e5ff>$1</color>");
            
            // Hyperlinks ([text](url)) -> Strip the URL, leave the text tinted cyan
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