#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: Centralized hub for developer links and pipeline documentation.
    /// </summary>
    public class VixenToolsHub : EditorWindow
    {
        // Replace these placeholder strings with your actual routing URLs
        private const string GITHUB_URL = "https://github.com/VixenCreations";
        private const string YOUTUBE_URL = "https://www.youtube.com/@vixenlicous";
        private const string X_URL = "https://x.com/VixenVRC";

        [MenuItem("VixenTools/About & Links")]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenToolsHub>("VixenTools Hub");
            // Increased height slightly to cleanly accommodate the third button
            window.minSize = new Vector2(300, 265);
            window.maxSize = new Vector2(300, 265);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(15);
            
            // Header Typography
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            EditorGUILayout.LabelField("VixenTools", headerStyle);
            
            GUIStyle subHeaderStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                wordWrap = true
            };
            EditorGUILayout.LabelField("Avatar Pipeline & Topology Architecture", subHeaderStyle);
            
            GUILayout.Space(15);
            DrawSeparator();
            GUILayout.Space(15);

            EditorGUILayout.HelpBox("Access documentation, updates, and video guides below.", MessageType.None);
            GUILayout.Space(10);

            // GitHub Routing
            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.8f); // Cyan aesthetic
            if (GUILayout.Button("GitHub Repository", GUILayout.Height(35)))
            {
                Application.OpenURL(GITHUB_URL);
            }

            GUILayout.Space(5);

            // YouTube Routing
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.5f); // Pink/Magenta aesthetic
            if (GUILayout.Button("YouTube Channel", GUILayout.Height(35)))
            {
                Application.OpenURL(YOUTUBE_URL);
            }

            GUILayout.Space(5);

            // X Routing
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f); // Dark Charcoal aesthetic
            if (GUILayout.Button("X (Twitter)", GUILayout.Height(35)))
            {
                Application.OpenURL(X_URL);
            }

            // Reset color state to prevent bleeding into other editor UI
            GUI.backgroundColor = Color.white;
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Version 1.0.0", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(10);
        }

        private void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 1));
        }
    }
}
#endif