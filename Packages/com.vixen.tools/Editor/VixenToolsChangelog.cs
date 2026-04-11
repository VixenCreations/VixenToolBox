#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Editor: Dynamically parses and renders the package CHANGELOG.md
    /// with custom rich-text styling matching the ecosystem's visual identity.
    /// </summary>
    public class VixenToolsChangelog : EditorWindow
    {
        private Vector2 scrollPosition;
        private string[] changelogLines;
        private GUIStyle richTextStyle;

        // Path to the changelog relative to the package root
        private const string CHANGELOG_PATH = "Packages/com.vixencreations.vixens-toolbox/CHANGELOG.md";

        [MenuItem("VixenTools/View Changelog")]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenToolsChangelog>("VixenTools Changelog");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnEnable()
        {
            LoadChangelog();
        }

        private void LoadChangelog()
        {
            // Attempt to load the Markdown file directly through the AssetDatabase
            TextAsset changelogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CHANGELOG_PATH);
            
            if (changelogAsset != null)
            {
                changelogLines = changelogAsset.text.Split('\n');
            }
            else
            {
                changelogLines = new string[] 
                { 
                    "<color=#ff00aa><b>[ERROR]</b></color> Could not locate CHANGELOG.md.", 
                    $"Expected path: {CHANGELOG_PATH}",
                    "Ensure the package is properly installed via VCC." 
                };
            }
        }

        private void OnGUI()
        {
            // Initialize rich text style if it doesn't exist
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

            // Header Banner
            Rect headerRect = EditorGUILayout.GetControlRect(false, 60);
            EditorGUI.DrawRect(headerRect, new Color(0.08f, 0.04f, 0.12f)); // Deep dark purple bg
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22
            };
            EditorGUI.LabelField(headerRect, "<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> ARCHIVE", headerStyle);

            GUILayout.Space(10);

            // Scrollable Markdown Content
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (changelogLines != null)
            {
                foreach (string line in changelogLines)
                {
                    string trimmedLine = line.TrimEnd('\r', '\n');
                    
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        GUILayout.Space(10);
                        continue;
                    }

                    RenderMarkdownLine(trimmedLine);
                }
            }

            EditorGUILayout.EndScrollView();
            
            // Footer
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Stay efficient. Stay sharp.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(10);
        }

        private void RenderMarkdownLine(string line)
        {
            // H1 (Main Title)
            if (line.StartsWith("# "))
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField($"<color=#ff00aa><size=18><b>{line.Substring(2)}</b></size></color>", richTextStyle);
                DrawSeparator(new Color(1f, 0f, 0.66f, 0.3f)); // Pink line
            }
            // H2 (Version & Date)
            else if (line.StartsWith("## "))
            {
                GUILayout.Space(15);
                EditorGUILayout.LabelField($"<color=#00e5ff><size=15><b>{line.Substring(3)}</b></size></color>", richTextStyle);
                DrawSeparator(new Color(0f, 0.9f, 1f, 0.2f)); // Cyan line
            }
            // H3 (Category: Added, Fixed, etc.)
            else if (line.StartsWith("### "))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField($"<color=#bfa8d2><b>{line.Substring(4)}</b></color>", richTextStyle);
            }
            // Bullet Points
            else if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                // Convert markdown bold (**text**) to Unity rich text (<b>text</b>)
                string parsedLine = ParseMarkdownFormatting(line.Substring(2));
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20); // Indent
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
            // A quick and dirty regex-free parser for standard markdown bold and inline code
            
            // Bold
            while (text.Contains("**"))
            {
                int first = text.IndexOf("**");
                int second = text.IndexOf("**", first + 2);
                if (second == -1) break;
                
                text = text.Substring(0, first) + "<b>" + text.Substring(first + 2, second - first - 2) + "</b>" + text.Substring(second + 2);
            }

            // Inline Code (Tinted cyan)
            while (text.Contains("`"))
            {
                int first = text.IndexOf("`");
                int second = text.IndexOf("`", first + 1);
                if (second == -1) break;
                
                text = text.Substring(0, first) + "<color=#00e5ff>" + text.Substring(first + 1, second - first - 1) + "</color>" + text.Substring(second + 1);
            }

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