#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vixenlicious.AnimationWorkbench
{
    public class MaterialPropertySearchPopup : EditorWindow
    {
        public class Entry
        {
            public string displayName;
            public string materialName;
            public string category;
            public string shaderProperty;
            public string path;
            public Type type;
        }

        private Action<Entry> _onSelect;
        private List<Entry> _all;
        private List<Entry> _filtered;
        private Entry _current;

        private TextField _search;
        private ScrollView _scroll;

        public static void Show(
            List<Entry> entries,
            Entry selected,
            EditorWindow owner,
            Action<Entry> onPick)
        {
            if (entries == null || entries.Count == 0)
                return;

            var wnd = CreateInstance<MaterialPropertySearchPopup>();

            wnd._all = entries;
            wnd._filtered = new List<Entry>(entries);
            wnd._current = selected;
            wnd._onSelect = onPick;

            wnd.titleContent = new GUIContent("Select Material Property");

            Rect ownerPos = owner != null ? owner.position : new Rect(200, 200, 800, 600);
            Vector2 initialSize = new Vector2(420, 480);

            wnd.minSize = new Vector2(300, 240);
            wnd.position = new Rect(
                ownerPos.x + (ownerPos.width - initialSize.x) * 0.5f,
                ownerPos.y + (ownerPos.height - initialSize.y) * 0.5f,
                initialSize.x,
                initialSize.y);

            wnd.ShowAuxWindow();
            wnd.Focus();
        }

        private void OnEnable()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.AddToClassList("popup-root");

            _search = new TextField("Search");
            _search.AddToClassList("popup-search");
            _search.RegisterValueChangedCallback(evt =>
            {
                Filter(evt.newValue);
                EditorApplication.delayCall += RebuildList;
            });
            root.Add(_search);

            _scroll = new ScrollView();
            _scroll.AddToClassList("popup-scroll");
            _scroll.style.flexGrow = 1f;
            root.Add(_scroll);

            var closeRow = new VisualElement();
            closeRow.AddToClassList("popup-footer");
            closeRow.style.flexDirection = FlexDirection.Row;
            closeRow.style.justifyContent = Justify.FlexEnd;

            var closeBtn = new Button(Close) { text = "Close" };
            closeBtn.AddToClassList("popup-close-btn");
            closeRow.Add(closeBtn);
            root.Add(closeRow);

            EditorApplication.delayCall += RebuildList;
        }

        private void Filter(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
            {
                _filtered = new List<Entry>(_all);
                return;
            }

            txt = txt.ToLowerInvariant().Replace("_", "").Replace(" ", "");

            _filtered = _all.Where(e =>
                 e.shaderProperty.ToLowerInvariant().Contains(txt) ||
                 e.displayName.ToLowerInvariant().Replace(" ", "").Replace("▸", "").Replace("_", "").Contains(txt) ||
                 e.category.ToLowerInvariant().Contains(txt) ||
                 e.materialName.ToLowerInvariant().Contains(txt)
            ).ToList();
        }

        private void RebuildList()
        {
            if (_scroll == null) return;
            _scroll.Clear();

            if (_filtered == null || _filtered.Count == 0)
            {
                _scroll.Add(new Label("No matching properties."));
                return;
            }

            var mats = _filtered
                .GroupBy(e => e.materialName)
                .OrderBy(g => g.Key);

            foreach (var matGroup in mats)
            {
                var matFold = new Foldout
                {
                    text = matGroup.Key,
                    value = false
                };
                matFold.AddToClassList("mat-foldout");

                var matContainer = matFold.contentContainer;

                var categories = matGroup
                    .GroupBy(e => e.category)
                    .OrderBy(g => g.Key);

                foreach (var catGroup in categories)
                {
                    var catFold = new Foldout
                    {
                        text = catGroup.Key,
                        value = false
                    };
                    catFold.AddToClassList("cat-foldout");

                    var catContainer = catFold.contentContainer;

                    foreach (var e in catGroup.OrderBy(x => x.shaderProperty))
                    {
                        var row = new Button(() =>
                        {
                            _onSelect?.Invoke(e);
                            Close();
                        })
                        {
                            text = e.shaderProperty
                        };

                        row.AddToClassList("prop-row");

                        if (_current != null &&
                            _current.shaderProperty == e.shaderProperty &&
                            _current.materialName == e.materialName)
                        {
                            row.AddToClassList("prop-row-selected");
                        }

                        catContainer.Add(row);
                    }

                    matContainer.Add(catFold);
                }

                _scroll.Add(matFold);
            }
        }
    }
}
#endif
