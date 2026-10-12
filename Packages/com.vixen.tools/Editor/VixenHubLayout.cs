#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VixenTools.Editor
{
    public partial class VixenHub
    {
        private const string CollapsedPref = "VixenTools.Hub.Collapsed";
        private const string FloatRectPref = "VixenTools.Hub.FloatRect";
        private const float BarHeight = 40f;
        private const float NarrowWidth = 760f;
        private const float TinyWidth = 520f;
        private static readonly Vector2 NormalMinSize = new Vector2(420f, 480f);

        private enum Placement { DockBottom, FloatBottom, Separate }

        private static readonly string[] BottomWindowTypes = { "UnityEditor.ConsoleWindow", "UnityEditor.ProjectBrowser" };

        private const string CollapseTip = "Folds the header and the page list away, so the page gets the room. Click again to bring them back.";
        private const string MoveTip = "Docks the Hub at the bottom of the editor, floats it along the bottom edge, or opens it in a window of its own.";
        private const string MinimizeTip = "Shrinks the Hub to this one bar. Restore brings it back as it was.";

        [SerializeField] private bool _minimized;
        [SerializeField] private Rect _restoreRect;
        [SerializeField] private Vector2 _restoreMinSize;

        private bool _collapsed;
        private Label _compactLocation;
        private readonly List<Button> _collapseButtons = new List<Button>();
        private readonly List<Button> _minimizeButtons = new List<Button>();

        private void SetUpFrame(VisualElement root, VisualElement header)
        {
            _collapseButtons.Clear();
            _minimizeButtons.Clear();
            _collapsed = EditorPrefs.GetBool(CollapsedPref, false);

            var compactBar = new VisualElement { name = "hub-compact-bar" };
            var title = new Label("<color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> HUB") { enableRichText = true };
            title.AddToClassList("hub-compact-title");
            _compactLocation = new Label();
            _compactLocation.AddToClassList("hub-compact-location");
            compactBar.Add(title);
            compactBar.Add(_compactLocation);
            compactBar.Add(BuildFrameButtons("hub-bar-buttons"));
            root.Insert(0, compactBar);

            header.Add(BuildFrameButtons("hub-window-buttons"));
            root.RegisterCallback<GeometryChangedEvent>(e => FitToSize(e.newRect));
        }

        private void FitToSize(Rect r)
        {
            VisualElement root = rootVisualElement;
            root.EnableInClassList("hub-narrow", r.width < NarrowWidth || r.height < 520f);
            root.EnableInClassList("hub-tiny", r.width < TinyWidth || r.height < 360f);
            SetDrawerMode(r.width < DrawerWidth);
        }

        private VisualElement BuildFrameButtons(string className)
        {
            var row = new VisualElement();
            row.AddToClassList(className);

            var collapse = new Button(ToggleCollapsed) { tooltip = CollapseTip };
            collapse.AddToClassList("hub-frame-btn");
            collapse.AddToClassList("hub-collapse-btn");
            _collapseButtons.Add(collapse);

            Button move = null;
            move = new Button(() => ShowMoveMenu(move)) { text = "Move", tooltip = MoveTip };
            move.AddToClassList("hub-frame-btn");

            var minimize = new Button(() => SetMinimized(!_minimized)) { tooltip = MinimizeTip };
            minimize.AddToClassList("hub-frame-btn");
            _minimizeButtons.Add(minimize);

            row.Add(collapse);
            row.Add(move);
            row.Add(minimize);
            return row;
        }

        private void ApplyFrame()
        {
            VisualElement root = rootVisualElement;
            root.EnableInClassList("hub-collapsed", _collapsed && !_minimized);
            root.EnableInClassList("hub-minimized", _minimized);
            foreach (Button b in _collapseButtons) b.text = _collapsed ? "Show Bars" : "Hide Bars";
            foreach (Button b in _minimizeButtons) b.text = _minimized ? "Restore" : "Minimize";
            ApplySidebar();
            UpdateCompactLocation();
        }

        private void UpdateCompactLocation()
        {
            if (_compactLocation == null) return;
            if (_searching) _compactLocation.text = "Search";
            else if (_activePage == null) _compactLocation.text = "";
            else if (_activePage.Category == null) _compactLocation.text = _activePage.Title;
            else _compactLocation.text = _activePage.Category + "  /  " + _activePage.Title;
        }

        private void ToggleCollapsed()
        {
            _collapsed = !_collapsed;
            EditorPrefs.SetBool(CollapsedPref, _collapsed);
            ApplyFrame();
        }

        private void SetMinimized(bool on)
        {
            if (_minimized == on) return;
            _minimized = on;
            if (!docked)
            {
                if (on)
                {
                    _restoreRect = position;
                    _restoreMinSize = minSize;
                    minSize = new Vector2(240f, BarHeight);
                    Rect r = position;
                    Rect main = EditorGUIUtility.GetMainWindowPosition();
                    bool onBottom = Mathf.Abs(r.yMax - main.yMax) < 48f;
                    position = new Rect(r.x, onBottom ? r.yMax - BarHeight : r.y, r.width, BarHeight);
                }
                else
                {
                    if (_restoreRect.width > 0f) position = _restoreRect;
                    minSize = _restoreMinSize.x > 0f ? _restoreMinSize : NormalMinSize;
                }
            }
            ApplyFrame();
        }

        private void ShowMoveMenu(VisualElement anchor)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Dock at the Bottom of the Editor"), false, () => MoveTo(Placement.DockBottom));
            menu.AddItem(new GUIContent("Float Along the Bottom of the Editor"), false, () => MoveTo(Placement.FloatBottom));
            menu.AddItem(new GUIContent("Open in a Separate Window"), false, () => MoveTo(Placement.Separate));
            menu.DropDown(anchor != null ? anchor.worldBound : new Rect(0f, 0f, 1f, 1f));
        }

        private void MoveTo(Placement placement)
        {
            if (!docked && placement != Placement.DockBottom)
            {
                bool wasMinimized = _minimized;
                if (wasMinimized) SetMinimized(false);
                PlaceFloating(this, placement);
                if (wasMinimized) SetMinimized(true);
                return;
            }

            if (!docked && !_minimized) SaveFloatRect(position);
            string tabId = _activePage != null ? _activePage.Id : null;
            bool minimized = _minimized;
            EditorApplication.delayCall += () => Reopen(placement, tabId, minimized);
            Close();
        }

        private static void Reopen(Placement placement, string tabId, bool minimized)
        {
            VixenHub hub;
            if (placement == Placement.DockBottom)
            {
                Type[] near = BottomWindowTypes.Select(n => typeof(EditorWindow).Assembly.GetType(n)).Where(t => t != null).ToArray();
                hub = GetWindow<VixenHub>(WindowTitle, true, near);
                if (hub.minSize.x < NormalMinSize.x) hub.minSize = NormalMinSize;
            }
            else
            {
                hub = CreateInstance<VixenHub>();
                hub.titleContent = new GUIContent(WindowTitle);
                hub.minSize = NormalMinSize;
                hub.Show();
                PlaceFloating(hub, placement);
            }
            hub.OpenTab(tabId);
            if (minimized) hub.SetMinimized(true);
        }

        private static void PlaceFloating(VixenHub hub, Placement placement)
        {
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            if (placement == Placement.FloatBottom)
            {
                float height = Mathf.Max(hub.minSize.y, 320f);
                hub.position = new Rect(main.x + 4f, main.yMax - height - 4f, main.width - 8f, height);
                return;
            }
            Rect saved = ReadFloatRect();
            hub.position = saved.width >= 240f && saved.height >= BarHeight
                ? saved
                : new Rect(main.center.x - 520f, main.center.y - 380f, 1040f, 760f);
        }

        private static void SaveFloatRect(Rect r)
        {
            EditorPrefs.SetString(FloatRectPref, string.Join(",", new[] { r.x, r.y, r.width, r.height }
                .Select(f => f.ToString(CultureInfo.InvariantCulture)).ToArray()));
        }

        private static Rect ReadFloatRect()
        {
            string[] parts = EditorPrefs.GetString(FloatRectPref, "").Split(',');
            if (parts.Length != 4) return new Rect();
            var v = new float[4];
            for (int i = 0; i < 4; i++)
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v[i])) return new Rect();
            return new Rect(v[0], v[1], v[2], v[3]);
        }
    }
}
#endif
