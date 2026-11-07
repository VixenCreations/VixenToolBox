using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static VixenTools.MMD.Editor.VixenMMDText;

namespace VixenTools.MMD.Editor
{
    [CustomEditor(typeof(VixenMMD))]
    internal sealed class VixenMMDEditor : UnityEditor.Editor
    {
        private bool _advanced;
        private List<Check> _checks;
        private double _checkedAt;

        public override void OnInspectorGUI()
        {
            var mmd = (VixenMMD)target;
            serializedObject.Update();
            var avatar = VixenMMDChecks.AvatarOf(mmd);
            var face = mmd.faceMesh != null ? mmd.faceMesh : avatar != null ? avatar.VisemeSkinnedMesh : null;

            int count = mmd.morphs.Count(m => m != null && m.enabled && m.sources != null && m.sources.Count > 0);
            EditorGUILayout.HelpBox(count == 0
                ? L("Open VixenMMD to set up the MMD blend shapes for this avatar.")
                : L("Builds {0} MMD blend shapes on the face when you upload or enter Play Mode. Your avatar files are not changed.", count), MessageType.None);
            if (GUILayout.Button(L("Open VixenMMD"), GUILayout.Height(28))) VixenMMDWindow.Open(mmd);
            DrawLanguagePicker();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L("Face"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("faceMesh"), new GUIContent(L("Face Mesh"), L("Leave empty to use the viseme mesh from the avatar descriptor.")));
            if (mmd.faceMesh == null && face != null) EditorGUILayout.LabelField(" ", L("Using {0}", face.name), EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("renameFaceToBody"), new GUIContent(L("Rename To Body"), L("MMD worlds only animate a mesh called Body.")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveFaceToRoot"), new GUIContent(L("Move To Avatar Root"), L("MMD worlds only find Body right under the avatar root.")));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L("MMD Worlds"), EditorStyles.boldLabel);
            var guard = serializedObject.FindProperty("worldGuard");
            EditorGUILayout.PropertyField(guard, new GUIContent(L("Layer Guard"), L("In MMD worlds, switches off the FX layers you pick, so hand gestures don't fight the dance. They switch back on everywhere else.")));
            var fxNames = VixenMMDChecks.FxLayerNames(avatar);
            if (guard.boolValue)
            {
                DrawLayerList(serializedObject.FindProperty("guardLayers"), fxNames, L("Switch Off In MMD Worlds"));
            }

            _advanced = EditorGUILayout.Foldout(_advanced, L("Advanced"), true);
            if (_advanced)
            {
                EditorGUI.indentLevel++;
                DrawLayerList(serializedObject.FindProperty("removeLayers"), fxNames, L("Remove From Upload"));
                var wd = serializedObject.FindProperty("forceWriteDefaults");
                EditorGUILayout.PropertyField(wd, new GUIContent(L("Force Write Defaults"), L("Turns Write Defaults on for every FX state on upload.")));
                if (wd.boolValue) EditorGUILayout.HelpBox(L("Gimmicks built for Write Defaults off can break with this on. Test your toggles after upload."), MessageType.Warning);
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (_checks == null || EditorApplication.timeSinceStartup - _checkedAt > 1.0)
            {
                _checks = VixenMMDChecks.Run(mmd);
                _checkedAt = EditorApplication.timeSinceStartup;
            }
            foreach (var c in _checks)
            {
                var type = c.Level == CheckLevel.Error ? MessageType.Error : c.Level == CheckLevel.Warning ? MessageType.Warning : MessageType.Info;
                EditorGUILayout.HelpBox(c.Message, type);
                if (c.Fix != null && GUILayout.Button(c.FixLabel))
                {
                    c.Fix();
                    _checks = null;
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(L("Import .json"))) VixenMMDShare.ImportDialog(mmd);
                if (GUILayout.Button(L("Export .json"))) VixenMMDShare.ExportDialog(mmd);
            }
            EditorGUILayout.LabelField(L("Reads and writes Make It MMD presets too."), EditorStyles.centeredGreyMiniLabel);
        }

        internal static void DrawLayerList(SerializedProperty list, List<string> fxNames, string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            for (int i = 0; i < list.arraySize; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var e = list.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(e, GUIContent.none);
                    if (GUILayout.Button(L("Remove"), GUILayout.Width(64)))
                    {
                        list.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }
            var current = new HashSet<string>(Enumerable.Range(0, list.arraySize).Select(i => list.GetArrayElementAtIndex(i).stringValue));
            var options = fxNames.Where(n => !current.Contains(n)).ToList();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (options.Count > 0)
                {
                    var labels = new[] { L("Add an FX layer...") }.Concat(options).ToArray();
                    int pick = EditorGUILayout.Popup(0, labels);
                    if (pick > 0) Append(list, options[pick - 1]);
                }
                else
                {
                    EditorGUILayout.LabelField(fxNames.Count == 0 ? L("No FX controller found.") : L("Every FX layer is listed."), EditorStyles.miniLabel);
                }
                if (GUILayout.Button(L("Add Name"), GUILayout.Width(80))) Append(list, "");
            }
        }

        private static void Append(SerializedProperty list, string value)
        {
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).stringValue = value;
        }
    }
}
