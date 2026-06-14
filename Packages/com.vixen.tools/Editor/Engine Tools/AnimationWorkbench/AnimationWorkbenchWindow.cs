#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Vixenlicious.AnimationWorkbench
{
    public class AnimationWorkbenchWindow : EditorWindow
    {
        private VisualElement root;
        private Font _cyberFont;
        private const string PackageFontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";

        private AnimationClip currentClip;
        private GameObject previewTarget;
        private readonly List<EditorCurveBinding> allBindings = new List<EditorCurveBinding>();
        private readonly List<BindingProfile> bindingProfiles = new List<BindingProfile>();
        private readonly Dictionary<EditorCurveBinding, AnimationCurve> stagedCurves =
            new Dictionary<EditorCurveBinding, AnimationCurve>();

        private ObjectField clipField;
        private ObjectField previewTargetField;
        private Button newClipBtn;
        private Button refreshBindingsBtn;
        private Button applyBtn;
        private Button revertBtn;
        private Button commitBtn;
        private ScrollView bindingsListContainer;
        private IntegerField intermediateDefaultField;
        private EnumField easingDefaultField;
        private CurveGraphView graphView;
        private TimelineRibbon timelineRibbon;
        private PreviewEngine previewEngine;
        private Label statusLabel;

        private SliderInt zoomSlider;
        private int zoomPercent = 100;

        private Button materialPickerButton;
        private Label materialSelectedLabel;
        private Button addMaterialBindingBtn;
        private readonly List<MaterialPropertySearchPopup.Entry> materialEntries =
            new List<MaterialPropertySearchPopup.Entry>();
        private MaterialPropertySearchPopup.Entry currentMaterialEntry;

        private float startTime = 0f;
        private float endTime = 1f;
        private bool sampleStart = false;
        private bool sampleEnd = false;
        private float overrideStartValue = 0f;
        private float overrideEndValue = 1f;

        [MenuItem("VixenTools/Unity Engine/Animation Workbench Pro")]
        public static void ShowWindow()
        {
            var w = GetWindow<AnimationWorkbenchWindow>("Workbench Pro");
            w.minSize = new Vector2(500, 600);
            w.Show();
        }

        private void OnEnable()
        {
            root = rootVisualElement;
            root.name = "workbench-root";
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(PackageFontPath);

            LoadStyles();
            ConstructUI();

            previewEngine = new PreviewEngine();
        }

        private void OnDisable()
        {
            previewEngine?.StopPreview();
            AnimationMode.StopAnimationMode();
        }

        private void LoadStyles()
        {
            var sheet = Resources.Load<StyleSheet>("AnimationWorkbenchStyles");
            if (sheet != null)
            {
                root.styleSheets.Add(sheet);
            }
            else
            {
                Debug.LogWarning(
                    "[VixForge] Stylesheet not found. Expected at: " +
                    "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/AnimationWorkbenchStyles.uss");
            }
        }

        private void ConstructUI()
        {
            root.Clear();

            var headerRect = new VisualElement();
            headerRect.style.height = 60;
            headerRect.style.backgroundColor = new Color(0.08f, 0.04f, 0.12f);
            headerRect.style.justifyContent = Justify.Center;
            headerRect.style.alignItems = Align.Center;
            headerRect.style.borderBottomWidth = 2;
            headerRect.style.borderBottomColor = new Color(1f, 0f, 0.66f, 0.8f);

            var headerLabel = new Label("<color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> ANIMATION WORKBENCH");
            headerLabel.enableRichText = true;
            headerLabel.style.fontSize = 24;
            if (_cyberFont != null)
            {
                headerLabel.style.unityFont = _cyberFont;
                headerLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            }
            headerRect.Add(headerLabel);
            root.Add(headerRect);

            var topToolbar = new VisualElement { name = "top-toolbar" };
            topToolbar.style.flexDirection = FlexDirection.Row;
            topToolbar.style.flexWrap = Wrap.Wrap;
            topToolbar.style.alignItems = Align.Center;
            topToolbar.style.paddingLeft = 6;
            topToolbar.style.paddingRight = 6;
            topToolbar.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f);

            clipField = new ObjectField("Animation Clip")
            {
                objectType = typeof(AnimationClip),
                allowSceneObjects = false
            };
            clipField.tooltip = "The animation clip currently being edited.";
            clipField.style.minWidth = 200;
            clipField.style.flexGrow = 1;
            clipField.RegisterValueChangedCallback(evt =>
            {
                currentClip = evt.newValue as AnimationClip;
                if (timelineRibbon != null)
                    timelineRibbon.SetClip(currentClip);
                RefreshBindings();
            });

            newClipBtn = new Button(CreateNewClip) { text = "New Clip" };
            newClipBtn.tooltip = "Create a new AnimationClip asset and load it into the workbench.";

            previewTargetField = new ObjectField("Preview Target")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true
            };
            previewTargetField.tooltip = "Scene GameObject used for material discovery and animated preview.";
            previewTargetField.style.minWidth = 200;
            previewTargetField.style.flexGrow = 1;
            previewTargetField.RegisterValueChangedCallback(evt =>
            {
                previewTarget = evt.newValue as GameObject;
                previewEngine?.SetTarget(previewTarget);

                if (previewTarget != null)
                    BuildMaterialPropertyList();
            });

            topToolbar.Add(clipField);
            topToolbar.Add(newClipBtn);
            topToolbar.Add(previewTargetField);
            root.Add(topToolbar);

            var mainScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            mainScroll.style.flexGrow = 1f;

            var scrollContent = new VisualElement();
            scrollContent.style.flexDirection = FlexDirection.Column;
            scrollContent.style.flexGrow = 1f;
            mainScroll.Add(scrollContent);

            var controlRow = new VisualElement();
            controlRow.style.flexDirection = FlexDirection.Row;
            controlRow.style.flexWrap = Wrap.Wrap;
            controlRow.style.marginTop = 6;
            controlRow.style.paddingLeft = 6;
            controlRow.style.paddingRight = 6;

            var selectionBox = new VisualElement { name = "selection-panel" };
            selectionBox.AddToClassList("cyber-panel");
            selectionBox.style.minWidth = 280;
            selectionBox.style.flexGrow = 1;
            selectionBox.style.flexDirection = FlexDirection.Column;

            var selectionHeader = new Label("Selection / Range") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14 } };
            selectionHeader.enableRichText = true;
            selectionHeader.text = "<color=#00e5ff>Selection / Range</color>";
            selectionBox.Add(selectionHeader);

            var sRow = new VisualElement();
            sRow.style.flexDirection = FlexDirection.Row;
            sRow.style.marginTop = 10;

            var startField = new FloatField("Start Time");
            startField.tooltip = "Start of the selected time range.";
            startField.SetValueWithoutNotify(startTime);
            startField.RegisterValueChangedCallback(e =>
            {
                startTime = Mathf.Max(0f, e.newValue);
                timelineRibbon?.SetRange(startTime, endTime);
                graphView?.SetRange(startTime, endTime);
                graphView?.SetZoomFactor(zoomPercent / 100f);
            });

            var endField = new FloatField("End Time");
            endField.tooltip = "End of the selected time range.";
            endField.SetValueWithoutNotify(endTime);
            endField.RegisterValueChangedCallback(e =>
            {
                endTime = Mathf.Max(0f, e.newValue);
                timelineRibbon?.SetRange(startTime, endTime);
                graphView?.SetRange(startTime, endTime);
                graphView?.SetZoomFactor(zoomPercent / 100f);
            });

            sRow.Add(startField);
            sRow.Add(endField);
            selectionBox.Add(sRow);

            var sampRow = new VisualElement();
            sampRow.style.flexDirection = FlexDirection.Row;
            sampRow.style.marginTop = 5;

            var sampleStartToggle = new Toggle("Sample Start Value") { value = sampleStart };
            sampleStartToggle.RegisterValueChangedCallback(e => sampleStart = e.newValue);

            var sampleEndToggle = new Toggle("Sample End Value") { value = sampleEnd };
            sampleEndToggle.RegisterValueChangedCallback(e => sampleEnd = e.newValue);

            sampRow.Add(sampleStartToggle);
            sampRow.Add(sampleEndToggle);
            selectionBox.Add(sampRow);

            var overrideRow = new VisualElement();
            overrideRow.style.flexDirection = FlexDirection.Row;
            overrideRow.style.marginTop = 5;

            var startOverrideField = new FloatField("Start Value (override)") { value = overrideStartValue };
            startOverrideField.RegisterValueChangedCallback(e => overrideStartValue = e.newValue);

            var endOverrideField = new FloatField("End Value (override)") { value = overrideEndValue };
            endOverrideField.RegisterValueChangedCallback(e => overrideEndValue = e.newValue);

            overrideRow.Add(startOverrideField);
            overrideRow.Add(endOverrideField);
            selectionBox.Add(overrideRow);

            controlRow.Add(selectionBox);

            var bindingBox = new VisualElement { name = "bindings-panel" };
            bindingBox.AddToClassList("cyber-panel");
            bindingBox.style.minWidth = 350;
            bindingBox.style.flexGrow = 2;
            bindingBox.style.flexDirection = FlexDirection.Column;

            var bindingsHeader = new Label("Bindings System") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14 } };
            bindingsHeader.enableRichText = true;
            bindingsHeader.text = "<color=#ff00aa>Bindings System</color>";
            bindingBox.Add(bindingsHeader);

            var bindingToolbar = new VisualElement();
            bindingToolbar.style.flexDirection = FlexDirection.Row;
            bindingToolbar.style.marginTop = 10;

            refreshBindingsBtn = new Button(RefreshBindings) { text = "Refresh System" };
            bindingToolbar.Add(refreshBindingsBtn);

            var selectAllBtn = new Button(() =>
            {
                foreach (var p in bindingProfiles) p.selected = true;
                RebuildBindingsUI();
            })
            { text = "Select All" };
            bindingToolbar.Add(selectAllBtn);

            var deselectAllBtn = new Button(() =>
            {
                foreach (var p in bindingProfiles) p.selected = false;
                RebuildBindingsUI();
            })
            { text = "None" };
            bindingToolbar.Add(deselectAllBtn);

            var deleteSelectedBtn = new Button(DeleteSelectedBindings) { text = "Delete Selected" };
            deleteSelectedBtn.style.backgroundColor = new Color(0.6f, 0.1f, 0.1f);
            deleteSelectedBtn.style.color = Color.white;
            deleteSelectedBtn.style.marginLeft = 10;
            bindingToolbar.Add(deleteSelectedBtn);

            bindingBox.Add(bindingToolbar);

            var materialRow = new VisualElement();
            materialRow.style.flexDirection = FlexDirection.Row;
            materialRow.style.marginTop = 10;
            materialRow.style.alignItems = Align.Center;

            var materialLabel = new Label("Material Property");
            materialLabel.style.minWidth = 100;

            materialSelectedLabel = new Label("<None Selected>");
            materialSelectedLabel.style.flexGrow = 1;

            materialPickerButton = new Button(() =>
            {
                if (materialEntries.Count == 0)
                {
                    statusLabel.text = "[VixForge] No material float properties found.";
                    return;
                }

                MaterialPropertySearchPopup.Show(materialEntries, currentMaterialEntry, this, entry =>
                {
                    currentMaterialEntry = entry;
                    materialSelectedLabel.text = entry.displayName;
                    addMaterialBindingBtn.SetEnabled(true);
                });
            })
            { text = "Choose…" };

            addMaterialBindingBtn = new Button(AddBindingFromMaterialProperty) { text = "Add Binding" };
            addMaterialBindingBtn.style.backgroundColor = new Color(0.2f, 0.7f, 0.8f);
            addMaterialBindingBtn.style.color = Color.black;
            addMaterialBindingBtn.SetEnabled(false);

            materialRow.Add(materialLabel);
            materialRow.Add(materialSelectedLabel);
            materialRow.Add(materialPickerButton);
            materialRow.Add(addMaterialBindingBtn);
            bindingBox.Add(materialRow);

            bindingsListContainer = new ScrollView();
            bindingsListContainer.AddToClassList("scroll-section");
            bindingsListContainer.style.maxHeight = 180;
            bindingBox.Add(bindingsListContainer);

            var defaultsRow = new VisualElement();
            defaultsRow.style.flexDirection = FlexDirection.Row;
            defaultsRow.style.flexWrap = Wrap.Wrap;
            defaultsRow.style.marginTop = 10;

            intermediateDefaultField = new IntegerField("Default Intermediate Keys") { value = 4 };
            intermediateDefaultField.style.width = 210;

            var easingDropdown = new EasingDropdown(EasingFunctions.EaseType.SmoothStep);
            easingDropdown.tooltip = "Default easing used when generating intermediate keys.";
            easingDropdown.style.width = 180;
            easingDropdown.style.maxWidth = 190;
            easingDropdown.OnValueChanged += val =>
            {
                foreach (var p in bindingProfiles) p.easing = val;
            };

            defaultsRow.Add(intermediateDefaultField);
            defaultsRow.Add(easingDropdown);
            bindingBox.Add(defaultsRow);

            var generateButton = new Button(BuildStagedForSelection) { text = "Generate Keys (Selection)" };
            generateButton.style.marginTop = 10;
            bindingBox.Add(generateButton);

            controlRow.Add(bindingBox);

            var actionBox = new VisualElement();
            actionBox.AddToClassList("cyber-panel");
            actionBox.style.minWidth = 220;
            actionBox.style.flexGrow = 1;
            actionBox.style.flexDirection = FlexDirection.Column;

            applyBtn = new Button(ApplyStagedToClip) { text = "Apply (Stage → Clip)" };
            applyBtn.AddToClassList("cyber-action-btn");
            applyBtn.AddToClassList("cyan-btn");

            commitBtn = new Button(CommitChanges) { text = "Commit + Save" };
            commitBtn.AddToClassList("cyber-action-btn");
            commitBtn.AddToClassList("pink-btn");

            revertBtn = new Button(RevertStaged) { text = "Revert Staged" };
            revertBtn.style.marginTop = 10;

            var previewLabel = new Label("Engine Preview") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 20, fontSize = 14 } };

            var previewBtn = new Button(() =>
            {
                if (currentClip == null || previewTarget == null)
                {
                    statusLabel.text = "[VixForge] Cannot preview: missing clip or preview target.";
                    return;
                }

                foreach (var kv in stagedCurves)
                    AnimationUtility.SetEditorCurve(currentClip, kv.Key, kv.Value);

                previewEngine?.StartPreview(currentClip, startTime);
                statusLabel.text = $"[VixForge] Preview running.";
            })
            { text = "Start Preview" };

            var stopPreviewBtn = new Button(() =>
            {
                previewEngine?.StopPreview();
                statusLabel.text = "[VixForge] Preview halted.";
            })
            { text = "Stop Preview" };

            actionBox.Add(applyBtn);
            actionBox.Add(commitBtn);
            actionBox.Add(revertBtn);
            actionBox.Add(previewLabel);
            actionBox.Add(previewBtn);
            actionBox.Add(stopPreviewBtn);

            controlRow.Add(actionBox);
            scrollContent.Add(controlRow);

            var zoomRow = new VisualElement();
            zoomRow.style.flexDirection = FlexDirection.Row;
            zoomRow.style.marginTop = 6;
            zoomRow.style.marginLeft = 6;
            zoomRow.style.marginRight = 6;

            zoomSlider = new SliderInt("Zoom %", 25, 400) { value = zoomPercent };
            zoomSlider.style.flexGrow = 1;
            zoomSlider.RegisterValueChangedCallback(e =>
            {
                zoomPercent = e.newValue;
                graphView?.SetZoomFactor(zoomPercent / 100f);
            });

            zoomRow.Add(zoomSlider);
            scrollContent.Add(zoomRow);

            var graphContainer = new VisualElement { name = "curve-graph-container" };
            graphContainer.style.flexGrow = 1;
            graphContainer.style.minHeight = 240;

            graphView = new CurveGraphView(OnGraphKeyChanged);
            graphView.style.flexGrow = 1;
            graphView.SetRange(startTime, endTime);
            graphView.SetZoomFactor(zoomPercent / 100f);

            graphContainer.Add(graphView);
            scrollContent.Add(graphContainer);

            timelineRibbon = new TimelineRibbon();
            timelineRibbon.name = "timeline-ribbon";
            timelineRibbon.OnRangeChanged = (s, e) =>
            {
                startTime = s;
                endTime = e;
                graphView.SetRange(s, e);
                graphView.SetZoomFactor(zoomPercent / 100f);
            };
            timelineRibbon.SetClip(currentClip);
            scrollContent.Add(timelineRibbon);

            var bottomRow = new VisualElement { name = "status-bar" };
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.marginTop = 6;
            bottomRow.style.paddingLeft = 6;
            bottomRow.style.paddingRight = 6;
            bottomRow.style.backgroundColor = new Color(0.08f, 0.08f, 0.10f);

            statusLabel = new Label("[VixForge] Systems Online.");
            statusLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
            bottomRow.Add(statusLabel);

            root.Add(mainScroll);
            root.Add(bottomRow);

            RefreshBindings();
        }

        #region Execution Logic

        private void CreateNewClip()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Animation Clip", "NewAnimation", "anim", "Select where to create the new animation clip.");
            if (string.IsNullOrEmpty(path)) return;

            var clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();

            currentClip = clip;
            clipField.SetValueWithoutNotify(clip);

            timelineRibbon.SetClip(currentClip);
            RefreshBindings();

            statusLabel.text = $"[VixForge] Authored new clip: {path}";
        }

        private void DeleteSelectedBindings()
        {
            if (currentClip == null) return;

            var toRemove = bindingProfiles.Where(p => p.selected).Select(p => p.binding).ToList();
            if (toRemove.Count == 0)
            {
                statusLabel.text = "[VixForge] Warning: No bindings selected for deletion.";
                return;
            }

            Undo.RecordObject(currentClip, "Delete Animation Bindings");

            foreach (var b in toRemove)
            {
                AnimationUtility.SetEditorCurve(currentClip, b, null);
            }

            EditorUtility.SetDirty(currentClip);
            AssetDatabase.SaveAssets();

            statusLabel.text = $"[VixForge] Successfully purged {toRemove.Count} bindings from clip system.";
            RefreshBindings();
        }

        private void BuildMaterialPropertyList()
        {
            materialEntries.Clear();
            currentMaterialEntry = null;
            materialSelectedLabel.text = "<None Selected>";
            addMaterialBindingBtn?.SetEnabled(false);

            if (previewTarget == null) return;

            var renderers = previewTarget.GetComponentsInChildren<Renderer>(true);

            foreach (var rend in renderers)
            {
                if (rend == null) continue;

                var mats = rend.sharedMaterials;
                if (mats == null) continue;

                string path = AnimationUtility.CalculateTransformPath(rend.transform, previewTarget.transform);

                foreach (var mat in mats)
                {
                    if (mat == null || mat.shader == null) continue;

                    string mName = mat.name;
                    int count = mat.shader.GetPropertyCount();

                    for (int i = 0; i < count; i++)
                    {
                        ShaderPropertyType propType = mat.shader.GetPropertyType(i);

                        bool supported = propType == ShaderPropertyType.Float ||
                                         propType == ShaderPropertyType.Range ||
                                         propType == ShaderPropertyType.Color ||
                                         propType == ShaderPropertyType.Vector;

                        if (!supported) continue;

                        string shaderProp = mat.shader.GetPropertyName(i);
                        string category = MaterialPropertySearchPopup_DetectCategory(shaderProp);

                        if (propType == ShaderPropertyType.Float || propType == ShaderPropertyType.Range)
                        {
                            AddMaterialEntry(mName, category, shaderProp, shaderProp, path);
                        }
                        else if (propType == ShaderPropertyType.Color)
                        {
                            AddMaterialEntry(mName, category, shaderProp + ".r", $"{shaderProp} (R)", path);
                            AddMaterialEntry(mName, category, shaderProp + ".g", $"{shaderProp} (G)", path);
                            AddMaterialEntry(mName, category, shaderProp + ".b", $"{shaderProp} (B)", path);
                            AddMaterialEntry(mName, category, shaderProp + ".a", $"{shaderProp} (A)", path);
                        }
                        else if (propType == ShaderPropertyType.Vector)
                        {
                            AddMaterialEntry(mName, category, shaderProp + ".x", $"{shaderProp} (X)", path);
                            AddMaterialEntry(mName, category, shaderProp + ".y", $"{shaderProp} (Y)", path);
                            AddMaterialEntry(mName, category, shaderProp + ".z", $"{shaderProp} (Z)", path);
                            AddMaterialEntry(mName, category, shaderProp + ".w", $"{shaderProp} (W)", path);
                        }
                    }
                }
            }

            var sorted = materialEntries.OrderBy(e => e.materialName).ThenBy(e => e.category).ThenBy(e => e.shaderProperty).ToList();
            materialEntries.Clear();
            materialEntries.AddRange(sorted);
        }

        private void AddMaterialEntry(string mName, string category, string shaderProp, string displayLabel, string path)
        {
            string display = $"{mName}  ▸  {category}  ▸  {displayLabel}";
            materialEntries.Add(new MaterialPropertySearchPopup.Entry
            {
                displayName = display,
                materialName = mName,
                category = category,
                shaderProperty = shaderProp,
                path = path,
                type = typeof(Renderer)
            });
        }

        private string MaterialPropertySearchPopup_DetectCategory(string prop)
        {
            string p = prop.ToLowerInvariant();
            if (p.Contains("emis")) return "Emission";
            if (p.Contains("dissolv")) return "Dissolve";
            if (p.Contains("rim")) return "Rim";
            if (p.Contains("hue") || p.Contains("sat") || p.Contains("color")) return "Color";
            if (p.Contains("outline")) return "Outline";
            if (p.StartsWith("al") || p.Contains("audio")) return "AudioLink";
            if (p.Contains("sdf")) return "SDF";
            if (p.Contains("mask")) return "Masking";
            if (p.Contains("smooth") || p.Contains("brdf") || p.Contains("light")) return "Shading";
            return "General";
        }

        private void AddBindingFromMaterialProperty()
        {
            if (currentClip == null)
            {
                statusLabel.text = "[VixForge] Cannot append binding: clip asset missing.";
                return;
            }

            if (currentMaterialEntry == null) return;

            var opt = currentMaterialEntry;
            var binding = new EditorCurveBinding
            {
                path = opt.path,
                type = opt.type,
                propertyName = $"material.{opt.shaderProperty}"
            };

            if (AnimationUtility.GetEditorCurve(currentClip, binding) != null)
            {
                statusLabel.text = "[VixForge] Binding logic halted: Track already exists on clip.";
                return;
            }

            Undo.RecordObject(currentClip, "Add Material Property Binding");

            var curve = CreateDefaultTwoKeyCurve(binding);
            AnimationUtility.SetEditorCurve(currentClip, binding, curve);
            EditorUtility.SetDirty(currentClip);
            AssetDatabase.SaveAssets();

            RefreshBindings();
            statusLabel.text = $"[VixForge] Injected target path: {binding.path} → {binding.propertyName}";
        }

        private AnimationCurve CreateDefaultTwoKeyCurve(EditorCurveBinding binding)
        {
            float clipLen = (currentClip != null && currentClip.length > 0f) ? currentClip.length : 1f;
            float sTime = Mathf.Clamp(startTime, 0f, clipLen);
            float eTime = Mathf.Clamp(endTime, 0f, clipLen);

            float sampledStart = overrideStartValue;
            float sampledEnd = overrideEndValue;

            if (previewTarget != null && binding.type == typeof(Renderer))
            {
                Transform t = string.IsNullOrEmpty(binding.path) ? previewTarget.transform : previewTarget.transform.Find(binding.path);
                if (t != null)
                {
                    var r = t.GetComponent<Renderer>();

                    if (TryGetMaterialFloat(r, binding.propertyName, out float matValue))
                    {
                        if (sampleStart) sampledStart = matValue;
                        if (sampleEnd) sampledEnd = matValue;
                    }
                }
            }

            float sVal = sampleStart ? sampledStart : overrideStartValue;
            float eVal = sampleEnd ? sampledEnd : overrideEndValue;

            var c = new AnimationCurve(new Keyframe(sTime, sVal), new Keyframe(eTime, eVal));

            for (int i = 0; i < c.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(c, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(c, i, AnimationUtility.TangentMode.Auto);
            }
            return c;
        }

        private void RefreshBindings()
        {
            if (bindingsListContainer == null || graphView == null || timelineRibbon == null || statusLabel == null) return;

            allBindings.Clear();
            bindingProfiles.Clear();
            stagedCurves.Clear();

            if (currentClip == null)
            {
                statusLabel.text = "[VixForge] Standby. No clip assigned.";
                bindingsListContainer.Clear();
                graphView.SetCurveSet(new Dictionary<EditorCurveBinding, AnimationCurve>());
                timelineRibbon.SetClip(null);
                return;
            }

            var bindings = AnimationUtility.GetCurveBindings(currentClip);
            allBindings.AddRange(bindings);

            int defaultIntermediate = 4;
            if (intermediateDefaultField != null) defaultIntermediate = Mathf.Max(0, intermediateDefaultField.value);

            EasingFunctions.EaseType defaultEase = EasingFunctions.EaseType.SmoothStep;
            if (easingDefaultField != null && easingDefaultField.value is Enum ev) defaultEase = (EasingFunctions.EaseType)ev;

            foreach (var b in allBindings)
            {
                var curve = AnimationUtility.GetEditorCurve(currentClip, b);
                AnimationCurve originalCurve = curve != null ? new AnimationCurve(curve.keys) : new AnimationCurve();

                var profile = new BindingProfile
                {
                    binding = b,
                    selected = true,
                    easing = defaultEase,
                    intermediateKeys = defaultIntermediate,
                    originalCurve = originalCurve,
                    currentCurve = originalCurve != null ? new AnimationCurve(originalCurve.keys) : new AnimationCurve()
                };

                bindingProfiles.Add(profile);
                stagedCurves[b] = originalCurve != null ? new AnimationCurve(originalCurve.keys) : new AnimationCurve();
            }

            RebuildBindingsUI();
            timelineRibbon.SetClip(currentClip);
            statusLabel.text = $"[VixForge] Indexed {bindingProfiles.Count} binding nodes.";

            graphView.SetCurveSet(stagedCurves);

            float clipLen = Mathf.Max(currentClip.length, 1f);
            if (endTime <= startTime)
            {
                startTime = 0f;
                endTime = clipLen;
            }

            graphView.SetRange(startTime, endTime);
            graphView.SetZoomFactor(Mathf.Max(0.01f, zoomPercent) / 100f);

            TryAutoFitGraph();
        }

        private void RebuildBindingsUI()
        {
            bindingsListContainer.Clear();

            foreach (var p in bindingProfiles)
            {
                var row = new VisualElement();
                row.AddToClassList("binding-row");
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;

                var toggle = new Toggle { value = p.selected };
                toggle.style.width = 18;
                toggle.tooltip = "Toggle whether this binding participates in key generation.";
                toggle.RegisterValueChangedCallback(evt => { p.selected = evt.newValue; });

                var label = new Label($"{p.binding.path} → {p.binding.propertyName}");
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.flexGrow = 1;
                label.tooltip = $"{p.binding.type?.Name ?? "Unknown"}";

                var inter = new IntegerField { value = p.intermediateKeys };
                inter.style.width = 80;
                inter.tooltip = "Number of intermediate keys generated inside the selected range.";
                inter.RegisterValueChangedCallback(evt => p.intermediateKeys = Mathf.Max(0, evt.newValue));

                var easingChoices = new List<EasingFunctions.EaseType>((EasingFunctions.EaseType[])Enum.GetValues(typeof(EasingFunctions.EaseType)));

                var ease = new PopupField<EasingFunctions.EaseType>(easingChoices, p.easing);
                ease.style.width = 110;
                ease.style.maxWidth = 120;
                ease.label = "";
                ease.tooltip = "Easing profile applied when generating intermediate keys.";
                ease.RegisterValueChangedCallback(evt => { p.easing = evt.newValue; });

                var sampleBtn = new Button(() =>
                {
                    if (stagedCurves.TryGetValue(p.binding, out var c))
                    {
                        overrideStartValue = c.Evaluate(startTime);
                        overrideEndValue = c.Evaluate(endTime);
                        statusLabel.text = $"[VixForge] Vector Sampled: {p.binding.propertyName} start={overrideStartValue:0.000} end={overrideEndValue:0.000}";
                    }
                })
                { text = "Sample" };
                sampleBtn.tooltip = "Sample this curve at Start / End Time and push the values into the override fields.";

                row.Add(toggle);
                row.Add(label);
                row.Add(inter);
                row.Add(ease);
                row.Add(sampleBtn);
                bindingsListContainer.Add(row);
            }
        }

        private void OnGraphKeyChanged(Dictionary<EditorCurveBinding, AnimationCurve> curves)
        {
            if (curves == null) return;
            foreach (var kv in curves) stagedCurves[kv.Key] = kv.Value;
            statusLabel.text = "[VixForge] Staged system updated via graph node edit.";
        }

        private AnimationCurve EnsureCurveExistsForBinding(AnimationClip clip, EditorCurveBinding binding)
        {
            var existing = AnimationUtility.GetEditorCurve(clip, binding);
            if (existing != null) return existing;

            var c = CreateDefaultTwoKeyCurve(binding);
            AnimationUtility.SetEditorCurve(clip, binding, c);
            return c;
        }

        private void BuildStagedForSelection()
        {
            if (currentClip == null) return;

            float sTime = startTime;
            float eTime = endTime;

            if (eTime < sTime)
            {
                float tmp = sTime;
                sTime = eTime;
                eTime = tmp;
            }

            foreach (var p in bindingProfiles)
            {
                if (!p.selected) continue;

                var orig = EnsureCurveExistsForBinding(currentClip, p.binding);
                AnimationCurve baseCurve = new AnimationCurve(orig.keys);

                float sVal = sampleStart ? baseCurve.Evaluate(sTime) : overrideStartValue;
                float eVal = sampleEnd ? baseCurve.Evaluate(eTime) : overrideEndValue;

                AnimationCurve newCurve = CurveOperations.BuildStretchedCurve(baseCurve, sTime, eTime, sVal, eVal, p.intermediateKeys, p.easing);
                stagedCurves[p.binding] = newCurve;
            }

            graphView.SetCurveSet(stagedCurves);
            statusLabel.text = "[VixForge] Mathematical easing applied to staged curves.";
            timelineRibbon.SetRange(sTime, eTime);
            graphView.SetRange(sTime, eTime);
            graphView.SetZoomFactor(zoomPercent / 100f);
        }

        private void ApplyStagedToClip()
        {
            if (currentClip == null) return;

            Undo.RecordObject(currentClip, "Apply Staged Curves");

            foreach (var kv in stagedCurves)
                AnimationUtility.SetEditorCurve(currentClip, kv.Key, kv.Value);

            EditorUtility.SetDirty(currentClip);
            AssetDatabase.SaveAssets();

            statusLabel.text = $"[VixForge] Push successful. Applied {stagedCurves.Count} curve datasets to active clip.";

            foreach (var p in bindingProfiles)
                p.currentCurve = AnimationUtility.GetEditorCurve(currentClip, p.binding);

            graphView.SetCurveSet(stagedCurves);
        }

        private void RevertStaged()
        {
            if (currentClip == null) return;

            Undo.RecordObject(currentClip, "Revert Staged Curves");

            foreach (var p in bindingProfiles)
            {
                stagedCurves[p.binding] = new AnimationCurve(p.originalCurve.keys);
                AnimationUtility.SetEditorCurve(currentClip, p.binding, new AnimationCurve(p.originalCurve.keys));
            }

            graphView.SetCurveSet(stagedCurves);
            AssetDatabase.SaveAssets();
            statusLabel.text = "[VixForge] Changes reverted. Restored original timeline states.";
            TryAutoFitGraph();
        }

        private void CommitChanges()
        {
            if (currentClip == null) return;

            Undo.RecordObject(currentClip, "Commit Animation Workbench Changes");

            foreach (var kv in stagedCurves)
                AnimationUtility.SetEditorCurve(currentClip, kv.Key, kv.Value);

            EditorUtility.SetDirty(currentClip);
            AssetDatabase.SaveAssets();
            statusLabel.text = "[VixForge] Core data saved successfully.";
        }

        [ContextMenu("Generate Staged")]
        public void GenerateStaged() => BuildStagedForSelection();

        private void TryAutoFitGraph()
        {
            if (stagedCurves == null || stagedCurves.Count == 0) return;

            float minT = float.MaxValue;
            float maxT = float.MinValue;

            foreach (var c in stagedCurves.Values)
            {
                if (c == null || c.keys == null || c.keys.Length == 0) continue;
                foreach (var k in c.keys)
                {
                    minT = Mathf.Min(minT, k.time);
                    maxT = Mathf.Max(maxT, k.time);
                }
            }

            if (minT == float.MaxValue || maxT == float.MinValue) return;

            float span = Mathf.Max(1f, maxT - minT);
            float padding = span * 0.08f;

            startTime = Mathf.Max(0f, minT - padding);
            endTime = maxT + padding;

            graphView.SetRange(startTime, endTime);
            graphView.SetZoomFactor(zoomPercent / 100f);
            timelineRibbon.SetRange(startTime, endTime);
        }

        private void OnInspectorUpdate()
        {
            if (previewEngine != null && previewEngine.IsPreviewing)
            {
                Repaint();
            }
        }

        #endregion

        private bool TryGetMaterialFloat(Renderer r, string propertyName, out float value)
        {
            value = 0f;
            if (r == null || r.sharedMaterials == null) return false;

            string raw = propertyName.Replace("material.", string.Empty);

            string baseProp = raw;
            string channel = "";
            int dotIdx = raw.LastIndexOf('.');
            if (dotIdx != -1)
            {
                baseProp = raw.Substring(0, dotIdx);
                channel = raw.Substring(dotIdx + 1).ToLower();
            }

            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null) continue;

                if (mat.HasProperty(baseProp))
                {
                    if (!string.IsNullOrEmpty(channel))
                    {
                        if (channel == "r" || channel == "g" || channel == "b" || channel == "a")
                        {
                            Color c = mat.GetColor(baseProp);
                            if (channel == "r") value = c.r;
                            else if (channel == "g") value = c.g;
                            else if (channel == "b") value = c.b;
                            else if (channel == "a") value = c.a;
                            return true;
                        }
                        else if (channel == "x" || channel == "y" || channel == "z" || channel == "w")
                        {
                            Vector4 v = mat.GetVector(baseProp);
                            if (channel == "x") value = v.x;
                            else if (channel == "y") value = v.y;
                            else if (channel == "z") value = v.z;
                            else if (channel == "w") value = v.w;
                            return true;
                        }
                    }
                    else
                    {
                        try
                        {
                            value = mat.GetFloat(baseProp);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        private class BindingProfile
        {
            public EditorCurveBinding binding;
            public bool selected;
            public EasingFunctions.EaseType easing;
            public int intermediateKeys;
            public AnimationCurve originalCurve;
            public AnimationCurve currentCurve;
        }
    }
}
#endif