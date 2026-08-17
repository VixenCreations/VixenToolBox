#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace VixenTools.Editor
{
    [InitializeOnLoad]
    public static class VixenUpdateNotifier
    {
        private const string BADGE_NAME = "vixen-update-badge";
        private const string PREF_STORED_VER = "VixenTools_StoredVersion";
        private const string PREF_UPDATE_PENDING = "VixenTools_UpdatePending";
        private const string PKG_PATH = "Packages/com.vixencreations.vixens-toolbox/package.json";
        private const string FONT_PATH = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";

        static VixenUpdateNotifier()
        {
            CheckForPackageChanges();

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

                        if (string.IsNullOrEmpty(storedVersion) || storedVersion != currentVersion)
                        {
                            EditorPrefs.SetBool(PREF_UPDATE_PENDING, true);
                            EditorPrefs.SetString(PREF_STORED_VER, currentVersion);
                        }
                    }
                }
            }
            catch { }
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

            badge.style.position = Position.Absolute;
            badge.style.bottom = 20;
            badge.style.right = 20;
            badge.style.width = 240;
            badge.style.height = 36;

            badge.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);

            badge.style.borderTopWidth = 0;
            badge.style.borderRightWidth = 0;
            badge.style.borderBottomWidth = 1;
            badge.style.borderLeftWidth = 4;

            badge.style.borderLeftColor = new Color(1f, 0f, 0.66f);
            badge.style.borderBottomColor = new Color(0f, 0.9f, 1f, 0.3f);

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

            badge.style.transitionDuration = new List<TimeValue> { new TimeValue(0.15f) };
            badge.RegisterCallback<PointerEnterEvent>(e => badge.style.backgroundColor = new Color(0.12f, 0.12f, 0.18f, 0.95f));
            badge.RegisterCallback<PointerLeaveEvent>(e => badge.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f));

            var label = new Label(">> <color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> UPDATE") { enableRichText = true };
            label.style.fontSize = 14;

            Font cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH);
            if (cyberFont != null) label.style.unityFontDefinition = new StyleFontDefinition(cyberFont);

            badge.Add(label);

            return badge;
        }
    }
}
#endif
