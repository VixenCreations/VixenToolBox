using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static VixenTools.MMD.Editor.VixenMMDText;

namespace VixenTools.MMD.Editor
{
    internal sealed class VixenMMDWindow : EditorWindow
    {
        internal const string MenuPath = "VixenTools/Avatars/VixenMMD";
        private static string[] Tabs => new[] { L("Morphs"), L("Auto-fill"), L("Dance Check") };
        private static string[] Filters => new[] { L("All"), L("Set up here"), L("On the face already"), L("Missing"), L("Used by most dances") };

        [SerializeField] private VixenMMD _target;
        [SerializeField] private int _tab;
        [SerializeField] private string _search = "";
        [SerializeField] private int _filter;
        [SerializeField] private string _selected;
        [SerializeField] private string _shapeSearch = "";
        [SerializeField] private string _customName = "";
        [SerializeField] private bool _preview;
        [SerializeField] private bool _showVisemes;
        [SerializeField] private List<string> _motionPaths = new List<string>();

        private Vector2 _listScroll, _detailScroll, _shapeScroll, _planScroll, _danceScroll;
        private readonly AutoFillOptions _options = new AutoFillOptions();
        private List<PlannedMorph> _plan;
        private string _planMessage;
        private List<string> _recipeNotes;
        private FaceShapeResolver _resolver;
        private VixenMMD _resolverFor;
        private Mesh _resolverMesh;
        private List<(string mmd, string copyOf)> _copies;
        private List<VmdMotion> _motions;
        private bool _ownsAnimationMode;
        private string _previewKey;

        [MenuItem(MenuPath, false, 40)]
        private static void OpenFromMenu() => Open(null);

        internal static void Open(VixenMMD target)
        {
            var w = GetWindow<VixenMMDWindow>();
            w.titleContent = new GUIContent("VixenMMD");
            w.minSize = new Vector2(760f, 480f);
            if (target != null) w._target = target;
            else w.PickFromSelection();
            w.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += StopPreview;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= StopPreview;
            StopPreview();
        }

        private void OnSelectionChanged()
        {
            if (PickFromSelection()) Repaint();
        }

        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            StopPreview();
            _preview = false;
        }

        private bool PickFromSelection()
        {
            var go = Selection.activeGameObject;
            if (go == null) return false;
            var found = go.GetComponentInParent<VixenMMD>(true);
            if (found == null)
            {
                var avatar = go.GetComponentInParent<VRCAvatarDescriptor>(true);
                if (avatar != null) found = avatar.GetComponentInChildren<VixenMMD>(true);
            }
            if (found == null || found == _target) return false;
            StopPreview();
            _target = found;
            _plan = null;
            _copies = null;
            return true;
        }

        private VRCAvatarDescriptor Avatar => _target != null ? VixenMMDChecks.AvatarOf(_target) : null;

        private SkinnedMeshRenderer Face
        {
            get
            {
                if (_target == null) return null;
                if (_target.faceMesh != null) return _target.faceMesh;
                var a = Avatar;
                return a != null ? a.VisemeSkinnedMesh : null;
            }
        }

        private FaceShapeResolver Resolver
        {
            get
            {
                var face = Face;
                var mesh = face != null ? face.sharedMesh : null;
                if (_resolver == null || _resolverFor != _target || _resolverMesh != mesh)
                {
                    _resolver = new FaceShapeResolver(Avatar, face);
                    _resolverFor = _target;
                    _resolverMesh = mesh;
                }
                return _resolver;
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            if (_target == null) return;
            if (Face == null || Face.sharedMesh == null)
            {
                EditorGUILayout.HelpBox(L("This avatar has no face mesh. Set one on the VixenMMD component, or set the viseme mesh on the avatar descriptor."), MessageType.Warning);
                return;
            }
            int tab = GUILayout.Toolbar(_tab, Tabs, GUILayout.Height(24));
            if (tab != _tab)
            {
                _tab = tab;
                GUI.FocusControl(null);
            }
            EditorGUILayout.Space(4);
            switch (_tab)
            {
                case 0: DrawMorphs(); break;
                case 1: DrawAutoFill(); break;
                default: DrawDances(); break;
            }
            UpdatePreview();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var picked = (VixenMMD)EditorGUILayout.ObjectField(_target, typeof(VixenMMD), true, GUILayout.Width(260));
                if (picked != _target)
                {
                    StopPreview();
                    _target = picked;
                    _plan = null;
                    _copies = null;
                }
                GUILayout.FlexibleSpace();
                if (_target != null)
                {
                    var a = Avatar;
                    var f = Face;
                    GUILayout.Label((a != null ? a.gameObject.name : L("No avatar")) + "   " + (f != null ? L("Face: {0} ({1} shapes)", f.name, f.sharedMesh != null ? f.sharedMesh.blendShapeCount : 0) : L("Face: none")), EditorStyles.miniLabel);
                }
            }
            DrawLanguagePicker();
            if (_target != null) return;

            var go = Selection.activeGameObject;
            var avatar = go != null ? go.GetComponentInParent<VRCAvatarDescriptor>(true) : null;
            EditorGUILayout.Space(8);
            if (avatar == null)
            {
                EditorGUILayout.HelpBox(L("Select an avatar in the Hierarchy to set up MMD dancing for it."), MessageType.Info);
                return;
            }
            var mim = VixenMMDShare.FindMakeItMmd(avatar.gameObject);
            if (mim.Count > 0)
            {
                EditorGUILayout.HelpBox(L("{0} is set up with Make It MMD. VixenMMD can take over its MMD shapes and settings.", avatar.gameObject.name), MessageType.Info);
                if (GUILayout.Button(L("Convert Make It MMD Setup"), GUILayout.Height(28)))
                {
                    _target = VixenMMDShare.ConvertMakeItMmd(avatar.gameObject, null);
                    if (_target != null) Selection.activeObject = _target.gameObject;
                }
                EditorGUILayout.Space(4);
            }
            if (GUILayout.Button(L("Add VixenMMD to {0}", avatar.gameObject.name), GUILayout.Height(28)))
            {
                var child = new GameObject("VixenMMD");
                Undo.RegisterCreatedObjectUndo(child, "Add VixenMMD");
                child.transform.SetParent(avatar.transform, false);
                _target = Undo.AddComponent<VixenMMD>(child);
                Selection.activeObject = child;
                _tab = 1;
            }
        }

        private void DrawShapeSet(List<VixenMMDShapeSet> list, string key, string label, string[] options, string emptyText)
        {
            var set = list.FirstOrDefault(s => s != null && s.key == key);
            int count = set != null && set.sources != null ? set.sources.Count : 0;
            for (int i = 0; i <= count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(i == 0 ? label : "", GUILayout.Width(170));
                    if (i < count)
                    {
                        var s = set.sources[i];
                        bool exists = Array.IndexOf(options, s.shape) >= 0;
                        var content = new GUIContent(exists ? s.shape : L("{0} (not on this face)", s.shape), L("Click to pick a different shape."));
                        var rect = GUILayoutUtility.GetRect(content, EditorStyles.popup, GUILayout.ExpandWidth(true));
                        if (GUI.Button(rect, content, EditorStyles.popup))
                        {
                            var target = s;
                            VixenMMDShapeSearch.Show(rect, options, null, shape => EditSet(() => target.shape = shape));
                        }
                        EditorGUI.BeginChangeCheck();
                        float scale = EditorGUILayout.FloatField(s.scale, GUILayout.Width(48));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(_target, "VixenMMD");
                            s.scale = scale;
                            EditorUtility.SetDirty(_target);
                            _plan = null;
                        }
                        if (GUILayout.Button(L("Remove"), GUILayout.Width(64)))
                        {
                            Undo.RecordObject(_target, "VixenMMD");
                            set.sources.RemoveAt(i);
                            if (set.sources.Count == 0) list.Remove(set);
                            EditorUtility.SetDirty(_target);
                            _plan = null;
                            GUIUtility.ExitGUI();
                        }
                    }
                    else
                    {
                        var content = new GUIContent(count == 0 ? emptyText + "   " + L("(click to add a shape)") : L("Add another shape..."));
                        var rect = GUILayoutUtility.GetRect(content, EditorStyles.popup, GUILayout.ExpandWidth(true));
                        GUILayout.Space(118);
                        if (GUI.Button(rect, content, EditorStyles.popup))
                        {
                            var used = set != null ? set.sources.Select(x => x.shape).ToList() : new List<string>();
                            VixenMMDShapeSearch.Show(rect, options, used, shape => EditSet(() =>
                            {
                                var existing = list.FirstOrDefault(x => x != null && x.key == key);
                                if (existing == null)
                                {
                                    existing = new VixenMMDShapeSet(key);
                                    list.Add(existing);
                                }
                                existing.sources.Add(new VixenMMDSource(shape, 1f));
                            }));
                        }
                    }
                }
            }
        }

        private void EditSet(Action change)
        {
            if (_target == null) return;
            Undo.RecordObject(_target, "VixenMMD");
            change();
            EditorUtility.SetDirty(_target);
            _plan = null;
            Repaint();
        }

        private void GuessExpressions(FaceShapeResolver r, bool overwrite)
        {
            var mmdNames = new HashSet<string>(VixenMMDNames.All.Select(n => n.Name));
            Undo.RecordObject(_target, "VixenMMD guess");
            foreach (var slot in VixenMMDRecipes.SlotNames())
            {
                var set = _target.expressionShapes.FirstOrDefault(s => s != null && s.key == slot);
                if (set != null && set.sources != null && set.sources.Count > 0 && !overwrite) continue;
                var guess = VixenMMDAutoFill.GuessExpression(slot, r, mmdNames);
                if (guess.Count == 0) continue;
                if (set == null)
                {
                    set = new VixenMMDShapeSet(slot);
                    _target.expressionShapes.Add(set);
                }
                set.sources = guess;
            }
            EditorUtility.SetDirty(_target);
            _plan = null;
        }

        private string StatusOf(string name, out int sources, out bool native)
        {
            var m = _target.FindMorph(name);
            sources = m != null && m.sources != null ? m.sources.Count(s => s != null && !string.IsNullOrEmpty(s.shape)) : 0;
            native = Resolver.Has(name);
            if (m != null && m.enabled && sources > 0) return native ? L("replaces face") : sources == 1 ? L("1 shape") : L("{0} shapes", sources);
            if (native) return L("on the face");
            return "";
        }

        private void DrawMorphs()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(340)))
                {
                    DrawMorphList();
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawMorphDetail();
                }
            }
        }

        private void DrawMorphList()
        {
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            _filter = EditorGUILayout.Popup(_filter, Filters);
            var custom = _target.morphs.Where(m => m != null && !string.IsNullOrEmpty(m.name) && !VixenMMDNames.IsKnown(m.name)).Select(m => m.name).Distinct().ToList();
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            foreach (MmdCategory cat in Enum.GetValues(typeof(MmdCategory)))
            {
                var rows = VixenMMDNames.All.Where(n => n.Category == cat && Visible(n.Name, n.English, n.DanceCore)).ToList();
                if (rows.Count == 0) continue;
                EditorGUILayout.LabelField(VixenMMDNames.CategoryLabel(cat), EditorStyles.boldLabel);
                foreach (var n in rows) DrawRow(n.Name, n.English);
            }
            var customRows = custom.Where(c => Visible(c, "", false)).ToList();
            if (customRows.Count > 0)
            {
                EditorGUILayout.LabelField(L("Custom"), EditorStyles.boldLabel);
                foreach (var c in customRows) DrawRow(c, "");
            }
            EditorGUILayout.EndScrollView();
            using (new EditorGUILayout.HorizontalScope())
            {
                _customName = EditorGUILayout.TextField(_customName);
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_customName)))
                {
                    if (GUILayout.Button(L("Add Name"), GUILayout.Width(80)))
                    {
                        string n = _customName.Trim();
                        if (_target.FindMorph(n) == null)
                        {
                            Undo.RecordObject(_target, "VixenMMD");
                            _target.morphs.Add(new VixenMMDMorph(n));
                            EditorUtility.SetDirty(_target);
                        }
                        _selected = n;
                        _customName = "";
                        GUI.FocusControl(null);
                    }
                }
            }
        }

        private bool Visible(string name, string english, bool danceCore)
        {
            if (!string.IsNullOrEmpty(_search))
            {
                bool hit = name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0
                    || (english ?? "").IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0
                    || Meaning(name, english).IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0
                    || VixenMMDNames.Fold(name).IndexOf(VixenMMDNames.Fold(_search), StringComparison.OrdinalIgnoreCase) >= 0;
                if (!hit) return false;
            }
            StatusOf(name, out int sources, out bool native);
            switch (_filter)
            {
                case 1: return sources > 0;
                case 2: return native;
                case 3: return sources == 0 && !native;
                case 4: return danceCore;
                default: return true;
            }
        }

        private void DrawRow(string name, string english)
        {
            string status = StatusOf(name, out _, out _);
            var rect = GUILayoutUtility.GetRect(10f, 18f, GUILayout.ExpandWidth(true));
            bool selected = name == _selected;
            if (selected) EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.90f, 0.35f));
            var left = new Rect(rect.x + 4f, rect.y, rect.width * 0.72f - 4f, rect.height);
            var right = new Rect(rect.x + rect.width * 0.72f, rect.y, rect.width * 0.28f - 4f, rect.height);
            string meaning = Meaning(name, english);
            GUI.Label(left, string.IsNullOrEmpty(meaning) ? name : name + "  " + meaning, EditorStyles.label);
            GUI.Label(right, status, EditorStyles.miniLabel);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                _selected = name;
                _recipeNotes = null;
                GUI.FocusControl(null);
                Event.current.Use();
            }
        }

        private void DrawMorphDetail()
        {
            if (string.IsNullOrEmpty(_selected))
            {
                EditorGUILayout.HelpBox(L("Pick an MMD morph on the left. Use Auto-fill to set most of them up from your face tracking shapes in one go."), MessageType.Info);
                return;
            }
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            var title = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 };
            EditorGUILayout.LabelField(_selected, title, GUILayout.Height(24));
            if (VixenMMDNames.TryGet(_selected, out var entry))
            {
                string fam = entry.Families.HasFlag(MmdFamily.Standard) ? L("Standard MMD name") : entry.Families.HasFlag(MmdFamily.InochiPM) ? L("From the InochiPM chart") : L("Extended name");
                EditorGUILayout.LabelField(Meaning(entry.Name, entry.English) + "   " + fam + (entry.DanceCore ? "   " + L("Used by most dances") : ""), EditorStyles.wordWrappedMiniLabel);
            }
            string variant = VixenMMDNames.FindNearName(_selected);
            if (variant != null) EditorGUILayout.HelpBox(L("Dances use \"{0}\". A digit or mark in this name is off, so dances will not drive it.", variant), MessageType.Warning);

            bool native = Resolver.Has(_selected);
            var morph = _target.FindMorph(_selected);
            _preview = EditorGUILayout.ToggleLeft(L("Preview on the avatar"), _preview);

            if (morph == null)
            {
                EditorGUILayout.HelpBox(native ? L("Your face already has this shape. Dances will use it as it is.") : L("Not set up yet."), MessageType.None);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (!native && GUILayout.Button(L("Auto-fill This Morph"))) AutoFillSelected(null);
                    if (GUILayout.Button(native ? L("Replace It With My Own") : L("Set Up By Hand")))
                    {
                        Undo.RecordObject(_target, "VixenMMD");
                        morph = new VixenMMDMorph(_selected);
                        _target.morphs.Add(morph);
                        EditorUtility.SetDirty(_target);
                    }
                }
                if (morph == null) DrawFillNotes();
            }
            if (morph != null)
            {
                if (native) EditorGUILayout.HelpBox(L("Your face already has this shape. On upload, VixenMMD's version takes its place."), MessageType.None);
                EditorGUI.BeginChangeCheck();
                bool enabled = EditorGUILayout.ToggleLeft(L("Build this morph"), morph.enabled);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_target, "VixenMMD");
                    morph.enabled = enabled;
                    EditorUtility.SetDirty(_target);
                }
                DrawSources(morph);
                DrawFillNotes();
                DrawRecipe(morph);
                DrawShapePicker(morph);
                EditorGUILayout.Space();
                if (GUILayout.Button(L("Remove From List"), GUILayout.Width(140)))
                {
                    Undo.RecordObject(_target, "VixenMMD");
                    _target.morphs.Remove(morph);
                    EditorUtility.SetDirty(_target);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSources(VixenMMDMorph morph)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L("Made From"), EditorStyles.boldLabel);
            if (morph.sources.Count == 0)
            {
                EditorGUILayout.LabelField(L("No shapes yet. Add some below."), EditorStyles.miniLabel);
                if (GUILayout.Button(L("Auto-fill This Morph"), GUILayout.ExpandWidth(false))) AutoFillSelected(morph);
            }
            for (int i = 0; i < morph.sources.Count; i++)
            {
                var s = morph.sources[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool exists = Resolver.Has(s.shape);
                    var style = exists ? EditorStyles.label : new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(1f, 0.45f, 0.4f) } };
                    EditorGUILayout.LabelField(new GUIContent(s.shape, exists ? s.shape : L("The face has no shape with this name.")), style, GUILayout.Width(200));
                    EditorGUI.BeginChangeCheck();
                    float scale = EditorGUILayout.Slider(s.scale, -1f, 2f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_target, "VixenMMD");
                        s.scale = scale;
                        EditorUtility.SetDirty(_target);
                    }
                    if (GUILayout.Button(L("Remove"), GUILayout.Width(64)))
                    {
                        Undo.RecordObject(_target, "VixenMMD");
                        morph.sources.RemoveAt(i);
                        EditorUtility.SetDirty(_target);
                        GUIUtility.ExitGUI();
                    }
                }
            }
        }

        private void DrawRecipe(VixenMMDMorph morph)
        {
            var recipe = VixenMMDRecipes.Find(morph.name);
            if (recipe == null) return;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L("Face Tracking Recipe"), EditorStyles.boldLabel);
            var resolved = VixenMMDRecipes.Resolve(recipe);
            string summary = resolved == null ? "" : string.Join("  +  ", resolved.Terms.Select(TermText));
            if (recipe.Kind == RecipeKind.Alias) summary = L("Same as {0}:", recipe.AliasOf) + "  " + summary;
            EditorGUILayout.LabelField(summary, EditorStyles.wordWrappedMiniLabel);
            if (recipe.Kind == RecipeKind.Proposal) EditorGUILayout.LabelField(L("Untested starting point. Check it with Preview."), EditorStyles.miniLabel);
            if (GUILayout.Button(L("Fill From Face Tracking"), GUILayout.Width(180)))
            {
                var plan = VixenMMDAutoFill.PlanOne(recipe, morph.name, Resolver, AutoFillInputs.From(_target));
                _recipeNotes = plan.Notes.ToList();
                if (plan.WillApply)
                {
                    Undo.RecordObject(_target, "VixenMMD");
                    morph.sources = plan.Sources.Select(s => new VixenMMDSource(s.shape, s.scale)).ToList();
                    morph.enabled = true;
                    EditorUtility.SetDirty(_target);
                }
                else _recipeNotes.Insert(0, plan.Skipped);
            }
        }

        private void DrawFillNotes()
        {
            if (_recipeNotes != null && _recipeNotes.Count > 0) EditorGUILayout.HelpBox(string.Join("\n", _recipeNotes), MessageType.Info);
        }

        private void AutoFillSelected(VixenMMDMorph morph)
        {
            var plan = VixenMMDAutoFill.PlanBest(_selected, Resolver, AutoFillInputs.From(_target));
            _recipeNotes = plan.Notes.ToList();
            if (plan.WillApply)
            {
                Undo.RecordObject(_target, "VixenMMD auto-fill");
                if (morph == null)
                {
                    morph = new VixenMMDMorph(_selected);
                    _target.morphs.Add(morph);
                }
                morph.sources = plan.Sources.Select(s => new VixenMMDSource(s.shape, s.scale)).ToList();
                morph.enabled = true;
                EditorUtility.SetDirty(_target);
                _plan = null;
            }
            else _recipeNotes.Insert(0, plan.Skipped);
            GUIUtility.ExitGUI();
        }

        private static string TermText(RecipeTerm t)
        {
            string scale = Mathf.Approximately(t.Scale, 1f) ? "" : " x" + t.Scale.ToString("0.##");
            switch (t.Kind)
            {
                case RecipeTermKind.Viseme: return L("viseme {0}", t.Name) + scale;
                case RecipeTermKind.Slot: return VixenMMDRecipes.SlotLabel(t.Name) + scale;
                default:
                    var bases = t.FaceKeys.Select(k => FaceShapeResolver.HasSide(k, out string b, out _) ? b : k).Distinct().ToList();
                    return string.Join("/", bases) + (t.FaceKeys.All(k => k.EndsWith("Left")) ? " " + L("(left)") : t.FaceKeys.All(k => k.EndsWith("Right")) ? " " + L("(right)") : "") + scale;
            }
        }

        private void DrawShapePicker(VixenMMDMorph morph)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L("Add A Shape"), EditorStyles.boldLabel);
            _shapeSearch = EditorGUILayout.TextField(_shapeSearch, EditorStyles.toolbarSearchField);
            var used = new HashSet<string>(morph.sources.Select(s => s.shape));
            var shapes = Resolver.UsableShapeNames.Where(n => !used.Contains(n) && (string.IsNullOrEmpty(_shapeSearch) || n.IndexOf(_shapeSearch, StringComparison.OrdinalIgnoreCase) >= 0)).Take(300).ToList();
            _shapeScroll = EditorGUILayout.BeginScrollView(_shapeScroll, GUILayout.Height(180));
            foreach (var n in shapes)
            {
                if (GUILayout.Button(n, EditorStyles.miniButton))
                {
                    Undo.RecordObject(_target, "VixenMMD");
                    morph.sources.Add(new VixenMMDSource(n, 1f));
                    morph.enabled = true;
                    EditorUtility.SetDirty(_target);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawAutoFill()
        {
            var r = Resolver;
            _planScroll = EditorGUILayout.BeginScrollView(_planScroll);
            int keys = CountKeys(r);
            string readsFrom = r.UsesNativeFaceTracking
                ? L("Reads face tracking from your avatar's face tracking settings. Found {0} face tracking shapes and {1} of 15 visemes.", keys, r.VisemeCount)
                : L("Reads face tracking from blend shape names (Unified Expressions, ARKit and similar). Found {0} face tracking shapes and {1} of 15 visemes.", keys, r.VisemeCount);
            EditorGUILayout.HelpBox(readsFrom, keys == 0 ? MessageType.Warning : MessageType.Info);
            if (keys == 0) EditorGUILayout.HelpBox(L("VixenMMD found no face tracking shapes on this face. Visemes still fill the mouth shapes, and you can set up the rest by hand."), MessageType.None);

            EditorGUILayout.LabelField(L("Options"), EditorStyles.boldLabel);
            _options.SkipMeshShapes = EditorGUILayout.ToggleLeft(L("Keep MMD shapes the face already has"), _options.SkipMeshShapes);
            _options.ReplaceSetUp = EditorGUILayout.ToggleLeft(L("Redo morphs that are already set up"), _options.ReplaceSetUp);
            _options.IncludeAliases = EditorGUILayout.ToggleLeft(L("Add the InochiPM chart names too"), _options.IncludeAliases);
            _options.IncludeProposals = EditorGUILayout.ToggleLeft(L("Add untested chart morphs"), _options.IncludeProposals);

            if (_target.expressionShapes == null) _target.expressionShapes = new List<VixenMMDShapeSet>();
            if (_target.visemeShapes == null) _target.visemeShapes = new List<VixenMMDShapeSet>();
            var options = r.UsableShapeNames.ToArray();

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(L("Expression Shapes"), EditorStyles.boldLabel);
                if (GUILayout.Button(L("Guess Empty Ones"), GUILayout.Width(130))) GuessExpressions(r, false);
                if (GUILayout.Button(L("Guess All"), GUILayout.Width(90)) && EditorUtility.DisplayDialog("VixenMMD", L("Replace the shapes of every expression with VixenMMD's guess?"), L("Replace"), L("Cancel"))) GuessExpressions(r, true);
            }
            EditorGUILayout.LabelField(L("Some MMD morphs need an expression that face tracking can't make. Build each one from one or more of your shapes, with an amount for each, or leave it empty."), EditorStyles.wordWrappedMiniLabel);
            foreach (var slot in VixenMMDRecipes.SlotNames())
            {
                DrawShapeSet(_target.expressionShapes, slot, VixenMMDRecipes.SlotLabel(slot), options, L("(none)"));
            }

            EditorGUILayout.Space();
            _showVisemes = EditorGUILayout.Foldout(_showVisemes, L("Viseme Shapes"), true);
            if (_showVisemes)
            {
                EditorGUILayout.LabelField(L("The mouth morphs start from your visemes. Add shapes to a viseme to build it your own way, for example a jaw shape and a lip shape together."), EditorStyles.wordWrappedMiniLabel);
                foreach (var v in VixenMMDRecipes.VisemeNames())
                {
                    string current = r.TryViseme(v, out string vs) ? L("Using {0}", vs) : L("No shape found");
                    DrawShapeSet(_target.visemeShapes, v, L("Viseme {0}", v), options, current);
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(L("Preview Changes"), GUILayout.Height(26)))
                {
                    _plan = VixenMMDAutoFill.Plan(_target, r, AutoFillInputs.From(_target), _options);
                    _planMessage = null;
                }
                int n = _plan != null ? _plan.Count(p => p.WillApply) : 0;
                using (new EditorGUI.DisabledScope(n == 0))
                {
                    string label = _plan == null ? L("Apply (Preview First)") : n == 0 ? L("Nothing To Apply") : n == 1 ? L("Apply 1 Morph") : L("Apply {0} Morphs", n);
                    if (GUILayout.Button(label, GUILayout.Height(26)))
                    {
                        int done = VixenMMDAutoFill.Apply(_target, _plan);
                        _planMessage = L("Set up {0} morphs. Undo reverses this.", done);
                        _plan = null;
                    }
                }
            }
            if (_planMessage != null) EditorGUILayout.HelpBox(_planMessage, MessageType.Info);
            if (_plan != null) DrawPlan();

            EditorGUILayout.Space(10);
            DrawNativeShapes(r);
            EditorGUILayout.EndScrollView();
        }

        private static int CountKeys(FaceShapeResolver r)
        {
            var keys = new HashSet<string>();
            foreach (var recipe in VixenMMDRecipes.All)
            {
                foreach (var t in recipe.Terms)
                {
                    if (t.Kind == RecipeTermKind.Keys) foreach (var k in t.FaceKeys) keys.Add(k);
                }
            }
            var shapes = new HashSet<string>();
            foreach (var k in keys)
            {
                if (r.TryKey(k, out string s, out _, out _)) shapes.Add(s);
            }
            return shapes.Count;
        }

        private void DrawPlan()
        {
            int add = _plan.Count(p => p.WillApply);
            EditorGUILayout.LabelField(L("{0} morphs will be set up, {1} left as they are.", add, _plan.Count - add), EditorStyles.boldLabel);
            foreach (var p in _plan.OrderBy(p => p.WillApply ? 0 : 1))
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    string head = p.Name + (p.Kind == RecipeKind.Proposal ? "   " + L("untested") : p.Kind == RecipeKind.Alias ? "   " + L("chart name") : "");
                    EditorGUILayout.LabelField(head, EditorStyles.boldLabel);
                    if (p.WillApply)
                    {
                        string src = string.Join("  +  ", p.Sources.Select(s => s.shape + (Mathf.Approximately(s.scale, 1f) ? "" : " x" + s.scale.ToString("0.###"))));
                        EditorGUILayout.LabelField(src, EditorStyles.wordWrappedMiniLabel);
                    }
                    else EditorGUILayout.LabelField(p.Skipped, EditorStyles.wordWrappedMiniLabel);
                    foreach (var note in p.Notes) EditorGUILayout.LabelField(note, EditorStyles.wordWrappedMiniLabel);
                }
            }
        }

        private void DrawNativeShapes(FaceShapeResolver r)
        {
            EditorGUILayout.LabelField(L("MMD Shapes On The Face"), EditorStyles.boldLabel);
            var native = r.ShapeNames.Where(VixenMMDNames.IsKnown).ToList();
            var wrongWidth = r.ShapeNames.Select(n => (n, v: VixenMMDNames.FindNearName(n))).Where(x => x.v != null).ToList();
            EditorGUILayout.LabelField(native.Count == 0 ? L("None. That's normal for most VRChat avatars.") : string.Join("  ", native), EditorStyles.wordWrappedLabel);
            foreach (var (n, v) in wrongWidth)
            {
                var copy = _target.FindMorph(v);
                if (copy != null && copy.enabled && copy.sources != null && copy.sources.Any(s => s != null && s.shape == n)) continue;
                EditorGUILayout.HelpBox(L("\"{0}\" looks like the MMD shape \"{1}\", but a digit or mark is off, so dances will not drive it.", n, v), MessageType.Warning);
                if (r.Has(v)) continue;
                if (GUILayout.Button(copy == null ? L("Add \"{0}\" As A Copy", v) : L("Make \"{0}\" A Copy Of \"{1}\"", v, n), GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(_target, "VixenMMD");
                    if (copy == null)
                    {
                        copy = new VixenMMDMorph(v);
                        _target.morphs.Add(copy);
                    }
                    copy.sources = new List<VixenMMDSource> { new VixenMMDSource(n, 1f) };
                    copy.enabled = true;
                    EditorUtility.SetDirty(_target);
                    GUIUtility.ExitGUI();
                }
            }
            if (native.Count == 0) return;
            if (GUILayout.Button(L("Find Which Ones Copy Another Shape"), GUILayout.Width(260))) _copies = FindCopies(Face.sharedMesh, native);
            if (_copies == null) return;
            if (_copies.Count == 0)
            {
                EditorGUILayout.LabelField(L("None of them is an exact copy of another shape."), EditorStyles.miniLabel);
                return;
            }
            foreach (var (mmd, copyOf) in _copies) EditorGUILayout.LabelField(L("{0}  is a copy of  {1}", mmd, copyOf), EditorStyles.miniLabel);
            if (GUILayout.Button(L("Add Them As Editable Morphs"), GUILayout.Width(260)))
            {
                Undo.RecordObject(_target, "VixenMMD");
                foreach (var (mmd, copyOf) in _copies)
                {
                    if (_target.FindMorph(mmd) != null) continue;
                    var m = new VixenMMDMorph(mmd);
                    m.sources.Add(new VixenMMDSource(copyOf, 1f));
                    _target.morphs.Add(m);
                }
                EditorUtility.SetDirty(_target);
            }
        }

        private static List<(string, string)> FindCopies(Mesh mesh, List<string> mmdNames)
        {
            var result = new List<(string, string)>();
            int vc = mesh.vertexCount;
            var mmdSet = new HashSet<string>(mmdNames);
            var others = new List<int>();
            for (int i = 0; i < mesh.blendShapeCount; i++) if (!mmdSet.Contains(mesh.GetBlendShapeName(i))) others.Add(i);
            var a = new Vector3[vc];
            var b = new Vector3[vc];
            var an = new Vector3[vc];
            var bn = new Vector3[vc];
            var at = new Vector3[vc];
            var bt = new Vector3[vc];
            var sums = new Dictionary<int, Vector3>();
            try
            {
                for (int x = 0; x < others.Count; x++)
                {
                    int i = others[x];
                    if (EditorUtility.DisplayCancelableProgressBar("VixenMMD", L("Reading shapes"), x / (float)others.Count)) return result;
                    mesh.GetBlendShapeFrameVertices(i, mesh.GetBlendShapeFrameCount(i) - 1, b, bn, bt);
                    sums[i] = Sum(b);
                }
                foreach (var name in mmdNames)
                {
                    int mi = mesh.GetBlendShapeIndex(name);
                    int frames = mesh.GetBlendShapeFrameCount(mi);
                    mesh.GetBlendShapeFrameVertices(mi, frames - 1, a, an, at);
                    var sa = Sum(a);
                    foreach (int i in others)
                    {
                        if (mesh.GetBlendShapeFrameCount(i) != frames || sums[i] != sa) continue;
                        bool same = true;
                        for (int f = 0; f < frames && same; f++)
                        {
                            mesh.GetBlendShapeFrameVertices(mi, f, a, an, at);
                            mesh.GetBlendShapeFrameVertices(i, f, b, bn, bt);
                            for (int v = 0; v < vc && same; v++) same = a[v] == b[v] && an[v] == bn[v] && at[v] == bt[v];
                        }
                        if (same)
                        {
                            result.Add((name, mesh.GetBlendShapeName(i)));
                            break;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return result;
        }

        private static Vector3 Sum(Vector3[] v)
        {
            var s = Vector3.zero;
            for (int i = 0; i < v.Length; i++) s += v[i];
            return s;
        }

        private void DrawDances()
        {
            if (_motions == null) _motions = _motionPaths.Select(VixenMMDVmd.Read).ToList();
            EditorGUILayout.LabelField(L("Check which MMD shapes a dance needs, and whether this avatar has them."), EditorStyles.wordWrappedLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(L("Add Motion (.vmd)"), GUILayout.Width(160)))
                {
                    string path = EditorUtility.OpenFilePanel(L("Add a VMD motion"), "", "vmd");
                    if (!string.IsNullOrEmpty(path) && !_motionPaths.Contains(path))
                    {
                        _motionPaths.Add(path);
                        _motions.Add(VixenMMDVmd.Read(path));
                    }
                }
                using (new EditorGUI.DisabledScope(_motionPaths.Count == 0))
                {
                    if (GUILayout.Button(L("Clear"), GUILayout.Width(80)))
                    {
                        _motionPaths.Clear();
                        _motions.Clear();
                    }
                }
            }
            _danceScroll = EditorGUILayout.BeginScrollView(_danceScroll);
            for (int i = 0; i < _motions.Count; i++)
            {
                var m = _motions[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    string info = m.Error ?? (m.Tracks == 0 ? L("no face keys (camera or body only)") : L("{0} morphs used, made for {1}", m.Peaks.Count(p => p.Value > 0f), m.Model));
                    EditorGUILayout.LabelField(m.FileName, info, EditorStyles.miniLabel);
                    if (GUILayout.Button(L("Remove"), GUILayout.Width(64)))
                    {
                        _motionPaths.RemoveAt(i);
                        _motions.RemoveAt(i);
                        GUIUtility.ExitGUI();
                    }
                }
            }
            var used = new Dictionary<string, float>();
            foreach (var m in _motions)
            {
                foreach (var kv in m.Peaks)
                {
                    if (kv.Value <= 0f) continue;
                    used[kv.Key] = Math.Max(used.TryGetValue(kv.Key, out float p) ? p : 0f, kv.Value);
                }
            }
            if (used.Count > 0)
            {
                var face = Face.sharedMesh;
                var valid = new HashSet<string>(VixenMMDMorphBuilder.ValidMorphs(face, _target.morphs, null).Select(x => x.name));
                var rows = used.Keys.Select(n => (n, status: DanceStatus(n, valid))).OrderBy(x => x.status.ok ? 1 : 0).ThenBy(x => x.n).ToList();
                int ok = rows.Count(x => x.status.ok);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(L("This avatar covers {0} of the {1} MMD shapes these dances use.", ok, rows.Count), ok == rows.Count ? MessageType.Info : MessageType.Warning);
                foreach (var (n, status) in rows)
                {
                    EditorGUILayout.LabelField(n + "   " + L("(up to {0}%)", Mathf.RoundToInt(used[n] * 100f)), status.text, EditorStyles.miniLabel);
                }
                var missing = rows.Where(x => !x.status.ok).Select(x => x.n).ToList();
                if (missing.Count > 0 && GUILayout.Button(L("Set Up The Missing Ones From Face Tracking"), GUILayout.Width(300)))
                {
                    var plan = VixenMMDAutoFill.Plan(_target, Resolver, AutoFillInputs.From(_target), new AutoFillOptions { SkipMeshShapes = false, ReplaceSetUp = true, IncludeAliases = true, IncludeProposals = true })
                        .Where(p => missing.Contains(p.Name)).ToList();
                    int done = VixenMMDAutoFill.Apply(_target, plan);
                    int left = missing.Count - done;
                    EditorUtility.DisplayDialog("VixenMMD", L("Set up {0} morphs.", done) + (left > 0 ? " " + L("{0} have no face tracking recipe or matching shapes. Set them up by hand on the Morphs tab.", left) : ""), L("OK"));
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private (bool ok, string text) DanceStatus(string name, HashSet<string> valid)
        {
            if (valid.Contains(name)) return (true, L("VixenMMD builds it"));
            if (Resolver.Has(name)) return (true, L("on the face"));
            string near = Resolver.ShapeNames.FirstOrDefault(n => VixenMMDNames.FindNearName(n) == name) ?? valid.FirstOrDefault(n => VixenMMDNames.FindNearName(n) == name);
            if (near != null) return (false, L("missing: you have \"{0}\", which is off by a digit or mark", near));
            return (false, L("missing"));
        }

        private void UpdatePreview()
        {
            if (Event.current.type != EventType.Repaint) return;
            if (!_preview || _tab != 0 || string.IsNullOrEmpty(_selected) || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                StopPreview();
                return;
            }
            var weights = new Dictionary<string, float>();
            var morph = _target.FindMorph(_selected);
            if (morph != null && morph.sources.Count > 0)
            {
                foreach (var s in morph.sources)
                {
                    if (s == null || string.IsNullOrEmpty(s.shape)) continue;
                    weights[s.shape] = (weights.TryGetValue(s.shape, out float w) ? w : 0f) + s.scale * 100f;
                }
            }
            else if (Resolver.Has(_selected)) weights[_selected] = 100f;
            string key = _selected + "|" + string.Join(",", weights.Select(kv => kv.Key + "=" + kv.Value.ToString("0.###")));
            if (key == _previewKey && _ownsAnimationMode) return;
            ApplyPreview(weights);
            _previewKey = key;
        }

        private void ApplyPreview(Dictionary<string, float> weights)
        {
            var face = Face;
            var avatar = Avatar;
            if (face == null || avatar == null || face.sharedMesh == null) return;
            if (!_ownsAnimationMode)
            {
                if (AnimationMode.InAnimationMode()) return;
                AnimationMode.StartAnimationMode();
                _ownsAnimationMode = true;
            }
            var clip = new AnimationClip { hideFlags = HideFlags.HideAndDontSave };
            string path = AnimationUtility.CalculateTransformPath(face.transform, avatar.transform);
            var mesh = face.sharedMesh;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string n = mesh.GetBlendShapeName(i);
                float v = weights.TryGetValue(n, out float w) ? w : 0f;
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + n), AnimationCurve.Constant(0f, 1f / 60f, v));
            }
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(avatar.gameObject, clip, 0f);
            AnimationMode.EndSampling();
            DestroyImmediate(clip);
            SceneView.RepaintAll();
        }

        private void StopPreview()
        {
            _previewKey = null;
            if (!_ownsAnimationMode) return;
            _ownsAnimationMode = false;
            if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
            SceneView.RepaintAll();
        }
    }
}
