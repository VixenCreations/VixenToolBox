#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON
using UnityEditor;
using UnityEngine;

namespace VixenTools.Editor
{
    [CustomEditor(typeof(ShaderDictionaryAsset))]
    public class ShaderDictionaryAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ShaderDictionaryAsset dict = (ShaderDictionaryAsset)target;

            GUILayout.Space(15);

            GUIStyle targetBtnStyle = new GUIStyle(GUI.skin.button);
            targetBtnStyle.fontStyle = FontStyle.Bold;
            targetBtnStyle.normal.textColor = ColorUtility.TryParseHtmlString("#00e5ff", out Color c1) ? c1 : Color.cyan;

            if (GUILayout.Button("Populate PBR Replacement Targets", targetBtnStyle, GUILayout.Height(30)))
            {
                Undo.RecordObject(dict, "Populate Targets");
                ShaderDictionaryAsset.AutoPopulateTargets(dict);
            }

            GUILayout.Space(5);

            GUIStyle whitelistBtnStyle = new GUIStyle(GUI.skin.button);
            whitelistBtnStyle.fontStyle = FontStyle.Bold;
            whitelistBtnStyle.normal.textColor = ColorUtility.TryParseHtmlString("#ff00aa", out Color c2) ? c2 : Color.magenta;

            if (GUILayout.Button("Discover & Populate Protected Whitelist", whitelistBtnStyle, GUILayout.Height(30)))
            {
                Undo.RecordObject(dict, "Populate Whitelist");
                ShaderDictionaryAsset.AutoPopulateWhitelist(dict);
            }

            GUILayout.Space(15);

            GUIStyle resetBtnStyle = new GUIStyle(GUI.skin.button);
            resetBtnStyle.fontStyle = FontStyle.Bold;
            resetBtnStyle.normal.textColor = ColorUtility.TryParseHtmlString("#ff3333", out Color c3) ? c3 : Color.red;

            if (GUILayout.Button("FACTORY RESET (Clear & Rebuild)", resetBtnStyle, GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("VIXEN SYSTEM WARNING", "This will completely wipe this dictionary and rebuild it from the default schema. Any custom shaders you added will be lost.\n\nExecute Protocol?", "NUKE & REBUILD", "ABORT"))
                {
                    Undo.RecordObject(dict, "Factory Reset Dictionary");

                    dict.shaders.Clear();

                    string path = AssetDatabase.GetAssetPath(dict);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (path.Contains("Target"))
                        {
                            ShaderDictionaryAsset.AutoPopulateTargets(dict);
                        }
                        else if (path.Contains("Whitelist"))
                        {
                            ShaderDictionaryAsset.AutoPopulateWhitelist(dict);
                        }
                    }

                    EditorUtility.SetDirty(dict);
                    AssetDatabase.SaveAssets();

                    Debug.Log($"[Vixen System] Dictionary '{dict.name}' has been factory reset.");
                }
            }

            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Use the Cyan button for your Target Dictionary (PBR Shaders).\nUse the Pink button for your Whitelist Dictionary (Video/AudioLink).\nUse the Red button to nuke and pave the current list.", MessageType.Info);
        }
    }
}
#endif