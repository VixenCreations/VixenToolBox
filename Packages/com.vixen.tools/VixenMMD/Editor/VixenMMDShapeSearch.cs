using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static VixenTools.MMD.Editor.VixenMMDText;

namespace VixenTools.MMD.Editor
{
    internal sealed class VixenMMDShapeSearch : PopupWindowContent
    {
        private const string FieldName = "VixenMMDShapeSearchField";
        private const int MaxRows = 400;

        private readonly string[] _options;
        private readonly HashSet<string> _exclude;
        private readonly Action<string> _onPick;
        private string _search = "";
        private Vector2 _scroll;
        private bool _focused;

        internal VixenMMDShapeSearch(IEnumerable<string> options, IEnumerable<string> exclude, Action<string> onPick)
        {
            _options = options.ToArray();
            _exclude = new HashSet<string>(exclude ?? Enumerable.Empty<string>());
            _onPick = onPick;
        }

        internal static void Show(Rect anchor, IEnumerable<string> options, IEnumerable<string> exclude, Action<string> onPick)
        {
            PopupWindow.Show(anchor, new VixenMMDShapeSearch(options, exclude, onPick));
        }

        public override Vector2 GetWindowSize() => new Vector2(340f, 380f);

        private List<string> Matches()
        {
            var list = new List<string>();
            string needle = _search.Trim();
            string folded = VixenMMDNames.Fold(needle);
            foreach (var n in _options)
            {
                if (_exclude.Contains(n)) continue;
                if (needle.Length > 0
                    && n.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0
                    && VixenMMDNames.Fold(n).IndexOf(folded, StringComparison.OrdinalIgnoreCase) < 0) continue;
                list.Add(n);
            }
            return list;
        }

        public override void OnGUI(Rect rect)
        {
            var e = Event.current;
            var hits = Matches();
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) && hits.Count > 0)
            {
                Pick(hits[0]);
                e.Use();
                return;
            }
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                editorWindow.Close();
                e.Use();
                return;
            }
            GUI.SetNextControlName(FieldName);
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            if (!_focused)
            {
                EditorGUI.FocusTextInControl(FieldName);
                _focused = true;
            }
            string count = hits.Count > MaxRows ? L("Showing {0} of {1}. Type more to narrow it down.", MaxRows, hits.Count) : hits.Count == 0 ? L("0 shapes") : hits.Count == 1 ? L("1 shape. Enter picks the first one.") : L("{0} shapes. Enter picks the first one.", hits.Count);
            EditorGUILayout.LabelField(count, EditorStyles.miniLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < hits.Count && i < MaxRows; i++)
            {
                if (GUILayout.Button(hits[i], EditorStyles.label))
                {
                    Pick(hits[i]);
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void Pick(string shape)
        {
            _onPick?.Invoke(shape);
            editorWindow.Close();
        }
    }
}
