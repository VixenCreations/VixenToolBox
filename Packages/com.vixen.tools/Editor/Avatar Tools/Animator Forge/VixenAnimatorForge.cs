#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using VRC.SDK3.Avatars.Components;

namespace VixenTools.Editor
{
    public class VixenAnimatorForge : EditorWindow
    {
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenAnimatorForgeStyles.uss";
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";

        private enum Mode { Doctor, Forge }
        private Mode _mode = Mode.Doctor;

        private Font _cyberFont;
        private ObjectField _targetField;
        private Button _btnDoctor;
        private Button _btnForge;
        private VisualElement _content;

        private List<AnimatorDoctor.AnimatorFinding> _findings = new List<AnimatorDoctor.AnimatorFinding>();

        private RigForge.RigType _rigType = RigForge.RigType.GameObjectToggle;
        private string _rigName = "New Toggle";
        private string _paramName = "";
        private bool _saved = true;
        private bool _synced = true;
        private bool _startOn = false;
        private readonly List<GameObject> _targets = new List<GameObject> { null };
        private SkinnedMeshRenderer _blendRenderer;
        private string _blendShape = "";
        private float _blendOn = 100f;
        private Renderer _swapRenderer;
        private int _matSlot = 0;
        private Material _swapMaterial;
        private readonly List<GameObject> _options = new List<GameObject> { null, null };
        private readonly List<string> _forgeLog = new List<string>();

        [MenuItem("VixenTools/Avatars/Animator Forge", priority = 43)]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenAnimatorForge>("Animator Forge");
            window.minSize = new Vector2(480, 680);
            window.Show();
        }

        private void OnEnable() => _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

        private void OnSelectionChange()
        {
            var go = Selection.activeGameObject;
            if (go != null && go.GetComponent<VRCAvatarDescriptor>() != null && _targetField != null && _targetField.value != go)
            {
                _targetField.value = go;
                _findings.Clear();
                RenderContent();
            }
        }

        private VRCAvatarDescriptor Descriptor
        {
            get
            {
                var go = _targetField != null ? _targetField.value as GameObject : null;
                return go != null ? go.GetComponent<VRCAvatarDescriptor>() : null;
            }
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.name = "hub-root";
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (uss != null) root.styleSheets.Add(uss);

            var header = new VisualElement { name = "hub-header", style = { minHeight = 80, justifyContent = Justify.Center, alignItems = Align.Center } };
            var title = new Label("<color=#00e5ff>ANIMATOR</color> <color=#ff00aa>FORGE</color>") { enableRichText = true };
            title.AddToClassList("hub-header-title");
            if (_cyberFont != null) title.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            header.Add(title);
            root.Add(header);

            var targetPanel = new VisualElement { style = { paddingLeft = 15, paddingRight = 15, paddingTop = 12 } };
            _targetField = new ObjectField("Avatar Root") { objectType = typeof(GameObject), allowSceneObjects = true };
            var selected = Selection.activeGameObject;
            if (selected != null && selected.GetComponent<VRCAvatarDescriptor>() != null) _targetField.value = selected;
            _targetField.RegisterValueChangedCallback(_ => { _findings.Clear(); _forgeLog.Clear(); RenderContent(); });
            targetPanel.Add(_targetField);
            root.Add(targetPanel);

            var tabs = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingLeft = 15, paddingRight = 15, paddingTop = 8, paddingBottom = 4 } };
            _btnDoctor = MakeTab("Doctor", Mode.Doctor);
            _btnForge = MakeTab("Forge", Mode.Forge);
            tabs.Add(_btnDoctor);
            tabs.Add(_btnForge);
            root.Add(tabs);

            var scroll = new ScrollView { style = { flexGrow = 1, paddingLeft = 15, paddingRight = 15, paddingTop = 8 } };
            _content = new VisualElement();
            scroll.Add(_content);
            root.Add(scroll);

            RenderContent();
        }

        private Button MakeTab(string text, Mode mode)
        {
            var btn = new Button(() => { _mode = mode; RenderContent(); }) { text = text };
            btn.AddToClassList("forge-tab");
            btn.style.flexGrow = 1;
            return btn;
        }

        private void RenderContent()
        {
            if (_content == null) return;
            _btnDoctor.EnableInClassList("forge-tab-active", _mode == Mode.Doctor);
            _btnForge.EnableInClassList("forge-tab-active", _mode == Mode.Forge);

            _content.Clear();
            if (_mode == Mode.Doctor) RenderDoctor();
            else RenderForge();
        }

        private void RenderDoctor()
        {
            var panel = Panel("Animator Diagnostics", "#00e5ff");

            var scanBtn = new Button(() =>
            {
                var d = Descriptor;
                _findings = d != null ? AnimatorDoctor.RunDiagnostics(d.gameObject) : new List<AnimatorDoctor.AnimatorFinding>();
                RenderContent();
            })
            { text = "RUN DIAGNOSTICS" };
            scanBtn.AddToClassList("cyber-action-btn");
            scanBtn.AddToClassList("cyan-btn");
            panel.Add(scanBtn);

            if (Descriptor == null)
            {
                panel.Add(Para("Select an avatar (with a VRCAvatarDescriptor) to scan its FX / gesture / action controllers, expression parameters, and menu."));
                _content.Add(panel);
                return;
            }

            if (_findings.Count == 0)
            {
                panel.Add(Para("No results yet. Press <b>RUN DIAGNOSTICS</b> to scan this avatar's animators."));
                _content.Add(panel);
                return;
            }

            int errors = _findings.Count(f => f.Severity == AnimatorDoctor.Severity.Error);
            int warns = _findings.Count(f => f.Severity == AnimatorDoctor.Severity.Warning);
            int infos = _findings.Count(f => f.Severity == AnimatorDoctor.Severity.Info);
            panel.Add(Para($"<color=#ff0033>{errors} errors</color>   <color=#ffaa00>{warns} warnings</color>   <color=#00e5ff>{infos} info</color>"));

            var safe = _findings.Where(f => f.IsSafe && f.Fix != null).ToList();
            if (safe.Count > 0)
            {
                var fixAll = new Button(() =>
                {
                    foreach (var f in safe) f.Fix();
                    AssetDatabase.SaveAssets();
                    var d = Descriptor;
                    _findings = d != null ? AnimatorDoctor.RunDiagnostics(d.gameObject) : new List<AnimatorDoctor.AnimatorFinding>();
                    RenderContent();
                })
                { text = $"FIX ALL SAFE ISSUES ({safe.Count})" };
                fixAll.AddToClassList("cyber-action-btn");
                fixAll.AddToClassList("pink-btn");
                panel.Add(fixAll);
            }

            _content.Add(panel);

            foreach (var finding in _findings)
                _content.Add(FindingCard(finding));
        }

        private VisualElement FindingCard(AnimatorDoctor.AnimatorFinding f)
        {
            string hex = f.Severity == AnimatorDoctor.Severity.Error ? "#ff0033"
                       : f.Severity == AnimatorDoctor.Severity.Warning ? "#ffaa00" : "#00e5ff";

            var card = new VisualElement();
            card.AddToClassList("cyber-panel");
            card.style.borderLeftWidth = 3;
            ColorUtility.TryParseHtmlString(hex, out var c);
            card.style.borderLeftColor = c;

            var titleRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween } };
            var titleLabel = new Label($"<color={hex}><b>[{f.Severity.ToString().ToUpper()}]</b></color> {f.Title}") { enableRichText = true };
            titleLabel.AddToClassList("md-p");
            titleLabel.style.flexShrink = 1;
            titleLabel.style.flexGrow = 1;
            titleRow.Add(titleLabel);
            card.Add(titleRow);

            if (!string.IsNullOrEmpty(f.Detail))
            {
                var detail = new Label(f.Detail) { enableRichText = true };
                detail.AddToClassList("md-p");
                detail.style.color = new Color(0.7f, 0.7f, 0.7f);
                detail.style.marginTop = 4;
                card.Add(detail);
            }

            var actions = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 8, justifyContent = Justify.FlexEnd } };
            if (f.Context != null)
            {
                var locate = new Button(() => { EditorGUIUtility.PingObject(f.Context); Selection.activeObject = f.Context; }) { text = "LOCATE" };
                locate.AddToClassList("data-tag-btn");
                locate.AddToClassList("data-tag-locate");
                actions.Add(locate);
            }
            if (f.Fix != null)
            {
                var fix = new Button(() =>
                {
                    f.Fix();
                    AssetDatabase.SaveAssets();
                    var d = Descriptor;
                    _findings = d != null ? AnimatorDoctor.RunDiagnostics(d.gameObject) : new List<AnimatorDoctor.AnimatorFinding>();
                    RenderContent();
                })
                { text = f.FixLabel };
                fix.AddToClassList("data-tag-btn");
                fix.AddToClassList(f.IsSafe ? "data-tag-optimize" : "data-tag-warning");
                actions.Add(fix);
            }
            card.Add(actions);
            return card;
        }

        private void RenderForge()
        {
            var panel = Panel("Rig Parameters", "#ff00aa");

            var typeField = new EnumField("Rig Type", _rigType);
            typeField.RegisterValueChangedCallback(e => { _rigType = (RigForge.RigType)e.newValue; RenderContent(); });
            panel.Add(typeField);

            var nameField = new TextField("Rig Name") { value = _rigName };
            nameField.RegisterValueChangedCallback(e => _rigName = e.newValue);
            panel.Add(nameField);

            var paramField = new TextField("Parameter Name") { value = _paramName };
            paramField.RegisterValueChangedCallback(e => _paramName = e.newValue);
            panel.Add(paramField);
            panel.Add(Hint("Leave parameter blank to reuse the rig name."));

            if (_rigType != RigForge.RigType.ExclusiveGroup && _rigType != RigForge.RigType.BlendshapeSlider)
            {
                var startOn = new Toggle("Start Enabled") { value = _startOn };
                startOn.RegisterValueChangedCallback(e => _startOn = e.newValue);
                panel.Add(startOn);
            }

            var savedToggle = new Toggle("Saved") { value = _saved };
            savedToggle.RegisterValueChangedCallback(e => _saved = e.newValue);
            panel.Add(savedToggle);

            var syncedToggle = new Toggle("Synced") { value = _synced };
            syncedToggle.RegisterValueChangedCallback(e => _synced = e.newValue);
            panel.Add(syncedToggle);

            _content.Add(panel);

            switch (_rigType)
            {
                case RigForge.RigType.GameObjectToggle: _content.Add(BuildTargetsPanel()); break;
                case RigForge.RigType.BlendshapeToggle:
                case RigForge.RigType.BlendshapeSlider: _content.Add(BuildBlendshapePanel()); break;
                case RigForge.RigType.MaterialSwap: _content.Add(BuildMaterialPanel()); break;
                case RigForge.RigType.ExclusiveGroup: _content.Add(BuildExclusivePanel()); break;
            }

            var forgeBtn = new Button(ExecuteForge) { text = "FORGE RIG" };
            forgeBtn.AddToClassList("cyber-action-btn");
            forgeBtn.AddToClassList("pink-btn");
            _content.Add(forgeBtn);

            if (_forgeLog.Count > 0)
            {
                var logPanel = Panel("Result", "#00e5ff");
                foreach (var line in _forgeLog) logPanel.Add(Para(line));
                _content.Add(logPanel);
            }
        }

        private VisualElement BuildTargetsPanel()
        {
            var panel = Panel("Target Objects", "#00e5ff");
            panel.Add(Hint("Objects to show/hide. All are toggled together by one parameter."));
            BuildObjectList(panel, _targets);
            return panel;
        }

        private VisualElement BuildExclusivePanel()
        {
            var panel = Panel("Exclusive Options", "#00e5ff");
            panel.Add(Hint("One object per option. Only one is ever active at a time (auto radio-wired)."));
            BuildObjectList(panel, _options);
            return panel;
        }

        private void BuildObjectList(VisualElement panel, List<GameObject> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int index = i;
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 3 } };
                var field = new ObjectField { objectType = typeof(GameObject), allowSceneObjects = true, value = list[index] };
                field.style.flexGrow = 1;
                field.RegisterValueChangedCallback(e => list[index] = e.newValue as GameObject);
                row.Add(field);

                var remove = new Button(() =>
                {
                    list.RemoveAt(index);
                    if (list.Count == 0) list.Add(null);
                    RenderContent();
                })
                { text = "X" };
                remove.AddToClassList("data-tag-btn");
                remove.AddToClassList("data-tag-destructive");
                row.Add(remove);
                panel.Add(row);
            }

            var add = new Button(() => { list.Add(null); RenderContent(); }) { text = "+ ADD SLOT" };
            add.AddToClassList("data-tag-btn");
            add.AddToClassList("data-tag-locate");
            add.style.marginTop = 6;
            panel.Add(add);
        }

        private VisualElement BuildBlendshapePanel()
        {
            var panel = Panel("Blendshape", "#00e5ff");
            var rendererField = new ObjectField("Skinned Mesh") { objectType = typeof(SkinnedMeshRenderer), allowSceneObjects = true, value = _blendRenderer };
            rendererField.RegisterValueChangedCallback(e => { _blendRenderer = e.newValue as SkinnedMeshRenderer; _blendShape = ""; RenderContent(); });
            panel.Add(rendererField);

            var shapes = GetBlendShapes(_blendRenderer);
            if (shapes.Count > 0)
            {
                if (string.IsNullOrEmpty(_blendShape) || !shapes.Contains(_blendShape)) _blendShape = shapes[0];
                var popup = new PopupField<string>("Blendshape", shapes, shapes.IndexOf(_blendShape));
                popup.RegisterValueChangedCallback(e => _blendShape = e.newValue);
                panel.Add(popup);
            }
            else
            {
                panel.Add(Hint("Assign a Skinned Mesh Renderer that has blendshapes."));
            }

            var onValue = new FloatField(_rigType == RigForge.RigType.BlendshapeSlider ? "Max Value" : "On Value") { value = _blendOn };
            onValue.RegisterValueChangedCallback(e => _blendOn = Mathf.Clamp(e.newValue, 0f, 100f));
            panel.Add(onValue);
            return panel;
        }

        private VisualElement BuildMaterialPanel()
        {
            var panel = Panel("Material Swap", "#00e5ff");
            var rendererField = new ObjectField("Renderer") { objectType = typeof(Renderer), allowSceneObjects = true, value = _swapRenderer };
            rendererField.RegisterValueChangedCallback(e => { _swapRenderer = e.newValue as Renderer; _matSlot = 0; RenderContent(); });
            panel.Add(rendererField);

            if (_swapRenderer != null)
            {
                int count = Mathf.Max(1, _swapRenderer.sharedMaterials.Length);
                var slots = Enumerable.Range(0, count).Select(i => $"Slot {i}").ToList();
                if (_matSlot >= slots.Count) _matSlot = 0;
                var slotPopup = new DropdownField("Material Slot", slots, _matSlot);
                slotPopup.RegisterValueChangedCallback(e => _matSlot = Mathf.Max(0, slots.IndexOf(e.newValue)));
                panel.Add(slotPopup);
            }

            var matField = new ObjectField("Material (On)") { objectType = typeof(Material), allowSceneObjects = false, value = _swapMaterial };
            matField.RegisterValueChangedCallback(e => _swapMaterial = e.newValue as Material);
            panel.Add(matField);
            panel.Add(Hint("Off state restores the renderer's current material on that slot."));
            return panel;
        }

        private void ExecuteForge()
        {
            _forgeLog.Clear();
            var d = Descriptor;
            if (d == null) { _forgeLog.Add("ERROR: Select an avatar with a VRCAvatarDescriptor."); RenderContent(); return; }

            if (!EditorUtility.DisplayDialog("Animator Forge",
                $"Forge a '{ObjectNames.NicifyVariableName(_rigType.ToString())}' rig named '{_rigName}' onto '{d.name}'?\n\n" +
                "This will create/assign FX controller, expression parameters, menu, and animation clips as needed. Existing layers and parameters are never deleted.",
                "Forge", "Cancel"))
                return;

            var req = new RigForge.RigRequest
            {
                Type = _rigType,
                RigName = _rigName,
                ParameterName = _paramName,
                Saved = _saved,
                Synced = _synced,
                StartOn = _startOn,
                Targets = _targets.Where(t => t != null).ToList(),
                BlendRenderer = _blendRenderer,
                BlendShape = _blendShape,
                BlendOnValue = _blendOn,
                SwapRenderer = _swapRenderer,
                MaterialSlot = _matSlot,
                SwapMaterial = _swapMaterial,
                Options = _options.Where(o => o != null).ToList()
            };

            var result = RigForge.Build(d, req);
            _forgeLog.AddRange(result.Log);
            _forgeLog.Add(result.Success ? "SUCCESS." : "Aborted (see errors above).");
            RenderContent();
        }

        private static List<string> GetBlendShapes(SkinnedMeshRenderer smr)
        {
            var list = new List<string>();
            if (smr == null || smr.sharedMesh == null) return list;
            var mesh = smr.sharedMesh;
            for (int i = 0; i < mesh.blendShapeCount; i++) list.Add(mesh.GetBlendShapeName(i));
            return list;
        }

        private VisualElement Panel(string title, string hex)
        {
            var panel = new VisualElement();
            panel.AddToClassList("cyber-panel");
            var header = new Label($"<color={hex}>{title}</color>") { enableRichText = true };
            header.AddToClassList("panel-header");
            if (_cyberFont != null) header.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            panel.Add(header);
            var sep = new VisualElement();
            sep.AddToClassList("md-separator");
            ColorUtility.TryParseHtmlString(hex, out var c); c.a = 0.3f;
            sep.style.backgroundColor = c;
            panel.Add(sep);
            return panel;
        }

        private static Label Para(string text)
        {
            var l = new Label(text) { enableRichText = true };
            l.AddToClassList("md-p");
            return l;
        }

        private static Label Hint(string text)
        {
            var l = new Label(text) { enableRichText = true };
            l.AddToClassList("md-p");
            l.style.color = new Color(0.6f, 0.6f, 0.6f);
            l.style.marginBottom = 4;
            return l;
        }
    }
}
#endif
