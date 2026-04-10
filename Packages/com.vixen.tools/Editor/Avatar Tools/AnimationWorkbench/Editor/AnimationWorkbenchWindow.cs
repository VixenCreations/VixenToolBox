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
        // UI root
        private VisualElement root;

        // Models
        private AnimationClip currentClip;
        private GameObject previewTarget;
        private readonly List<EditorCurveBinding> allBindings = new List<EditorCurveBinding>();
        private readonly List<BindingProfile> bindingProfiles = new List<BindingProfile>();
        private readonly Dictionary<EditorCurveBinding, AnimationCurve> stagedCurves =
            new Dictionary<EditorCurveBinding, AnimationCurve>();

        // UI
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

        // Zoom
        private SliderInt zoomSlider;
        private int zoomPercent = 100;

        // Material property binding helpers
        private Button materialPickerButton;
        private Label materialSelectedLabel;
        private Button addMaterialBindingBtn;
        private readonly List<MaterialPropertySearchPopup.Entry> materialEntries =
            new List<MaterialPropertySearchPopup.Entry>();
        private MaterialPropertySearchPopup.Entry currentMaterialEntry;

        // time / sampling
        private float startTime = 0f;
        private float endTime = 1f;
        private bool sampleStart = true;
        private bool sampleEnd = true;
        private float overrideStartValue = 1f;
        private float overrideEndValue = 0f;

        [MenuItem("VixenTools/Animation Workbench Pro")]
        public static void ShowWindow()
        {
            var w = GetWindow<AnimationWorkbenchWindow>();
            w.titleContent = new GUIContent("Workbench Pro");
            w.minSize = new Vector2(900, 600);
            w.Show();
        }

        private void OnEnable()
        {
            root = rootVisualElement;
            root.name = "workbench-root";

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
                    "[AnimationWorkbench] Stylesheet not found. Expected at: " +
                    "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/AnimationWorkbench/Editor/Resources/AnimationWorkbenchStyles.uss");
            }
        }

        private void ConstructUI()
        {
            root.Clear();

            // --------------------------------------------------------------------
            // TOP TOOLBAR
            // --------------------------------------------------------------------
            var topToolbar = new VisualElement { name = "top-toolbar" };
            topToolbar.style.flexDirection = FlexDirection.Row;
            topToolbar.style.alignItems = Align.Center;
            topToolbar.style.paddingLeft = 4;
            topToolbar.style.paddingRight = 4;

            clipField = new ObjectField("Animation Clip")
            {
                objectType = typeof(AnimationClip),
                allowSceneObjects = false
            };
            clipField.tooltip = "The animation clip currently being edited.";
            clipField.style.flexGrow = 1;
            clipField.RegisterValueChangedCallback(evt =>
            {
                currentClip = evt.newValue as AnimationClip;
                if (timelineRibbon != null)
                    timelineRibbon.SetClip(currentClip);
                RefreshBindings();
            });

            newClipBtn = new Button(CreateNewClip)
            {
                text = "New Clip"
            };
            newClipBtn.tooltip = "Create a new AnimationClip asset and load it into the workbench.";

            previewTargetField = new ObjectField("Preview Target")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true
            };
            previewTargetField.tooltip = "Scene GameObject used for material discovery and animated preview.";
            previewTargetField.style.width = 280;
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

            // --------------------------------------------------------------------
            // MAIN SCROLL AREA
            // --------------------------------------------------------------------
            var mainScroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "main-scroll"
            };
            mainScroll.style.flexGrow = 1f;

            var scrollContent = new VisualElement();
            scrollContent.style.flexDirection = FlexDirection.Column;
            scrollContent.style.flexGrow = 1f;
            mainScroll.Add(scrollContent);

            // --------------------------------------------------------------------
            // CONTROLS ROW (3-column layout)
            // --------------------------------------------------------------------
            var controlRow = new VisualElement();
            controlRow.style.flexDirection = FlexDirection.Row;
            controlRow.style.marginTop = 6;
            controlRow.style.paddingLeft = 6;
            controlRow.style.paddingRight = 6;

            // ---------------------------
            // Selection Panel
            // ---------------------------
            var selectionBox = new VisualElement { name = "selection-panel" };
            selectionBox.style.width = 300;
            selectionBox.style.flexDirection = FlexDirection.Column;

            selectionBox.Add(new Label("Selection / Range")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            });

            var sRow = new VisualElement();
            sRow.style.flexDirection = FlexDirection.Row;

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

            var sampleStartToggle = new Toggle("Sample Start Value") { value = sampleStart };
            sampleStartToggle.RegisterValueChangedCallback(e => sampleStart = e.newValue);

            var sampleEndToggle = new Toggle("Sample End Value") { value = sampleEnd };
            sampleEndToggle.RegisterValueChangedCallback(e => sampleEnd = e.newValue);

            sampRow.Add(sampleStartToggle);
            sampRow.Add(sampleEndToggle);
            selectionBox.Add(sampRow);

            var overrideRow = new VisualElement();
            overrideRow.style.flexDirection = FlexDirection.Row;

            var startOverrideField = new FloatField("Start Value (override)") { value = overrideStartValue };
            startOverrideField.RegisterValueChangedCallback(e => overrideStartValue = e.newValue);

            var endOverrideField = new FloatField("End Value (override)") { value = overrideEndValue };
            endOverrideField.RegisterValueChangedCallback(e => overrideEndValue = e.newValue);

            overrideRow.Add(startOverrideField);
            overrideRow.Add(endOverrideField);
            selectionBox.Add(overrideRow);

            controlRow.Add(selectionBox);

            // ---------------------------
            // BINDINGS PANEL
            // ---------------------------
            var bindingBox = new VisualElement { name = "bindings-panel" };
            bindingBox.style.flexGrow = 1;
            bindingBox.style.marginLeft = 6;
            bindingBox.style.flexDirection = FlexDirection.Column;

            bindingBox.Add(new Label("Bindings")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            });

            var bindingToolbar = new VisualElement();
            bindingToolbar.style.flexDirection = FlexDirection.Row;

            refreshBindingsBtn = new Button(RefreshBindings) { text = "Refresh" };
            bindingToolbar.Add(refreshBindingsBtn);

            var selectAllBtn = new Button(() =>
            {
                foreach (var p in bindingProfiles)
                    p.selected = true;
                RebuildBindingsUI();
            })
            { text = "Select All" };
            bindingToolbar.Add(selectAllBtn);

            var deselectAllBtn = new Button(() =>
            {
                foreach (var p in bindingProfiles)
                    p.selected = false;
                RebuildBindingsUI();
            })
            { text = "None" };
            bindingToolbar.Add(deselectAllBtn);

            bindingBox.Add(bindingToolbar);

            // Material property row
            var materialRow = new VisualElement();
            materialRow.style.flexDirection = FlexDirection.Row;
            materialRow.style.marginTop = 4;
            materialRow.style.alignItems = Align.Center;

            var materialLabel = new Label("Material Property");
            materialLabel.style.minWidth = 100;

            materialSelectedLabel = new Label("<None Selected>");
            materialSelectedLabel.style.flexGrow = 1;

            materialPickerButton = new Button(() =>
            {
                if (materialEntries.Count == 0)
                {
                    statusLabel.text = "No material float properties found.";
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

            addMaterialBindingBtn = new Button(AddBindingFromMaterialProperty)
            { text = "Add Binding" };
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

            // Defaults row (Intermediate keys + Easing dropdown)
            var defaultsRow = new VisualElement();
            defaultsRow.style.flexDirection = FlexDirection.Row;
            defaultsRow.style.marginTop = 4;

            intermediateDefaultField = new IntegerField("Default Intermediate Keys") { value = 4 };
            intermediateDefaultField.style.width = 210;

            // --- Custom Easing Dropdown ---
            var easingDropdown = new EasingDropdown(EasingFunctions.EaseType.SmoothStep);
            easingDropdown.tooltip = "Default easing used when generating intermediate keys.";
            easingDropdown.style.width = 180;
            easingDropdown.style.maxWidth = 190;

            easingDropdown.OnValueChanged += val =>
            {
                foreach (var p in bindingProfiles)
                    p.easing = val;
            };

            defaultsRow.Add(intermediateDefaultField);
            defaultsRow.Add(easingDropdown);
            bindingBox.Add(defaultsRow);

            var generateButton = new Button(BuildStagedForSelection)
            {
                text = "Generate Keys (Selection)"
            };
            bindingBox.Add(generateButton);

            controlRow.Add(bindingBox);

            // ---------------------------
            // ACTION PANEL
            // ---------------------------
            var actionBox = new VisualElement();
            actionBox.style.width = 260;
            actionBox.style.marginLeft = 6;
            actionBox.style.flexDirection = FlexDirection.Column;

            applyBtn = new Button(ApplyStagedToClip)
            {
                text = "Apply (Stage → Clip)"
            };
            revertBtn = new Button(RevertStaged)
            {
                text = "Revert Staged"
            };
            commitBtn = new Button(CommitChanges)
            {
                text = "Commit + Save"
            };

            var previewLabel = new Label("Preview");

            var previewBtn = new Button(() =>
            {
                if (currentClip == null || previewTarget == null)
                {
                    statusLabel.text = "Cannot preview: missing clip or preview target.";
                    return;
                }

                foreach (var kv in stagedCurves)
                    AnimationUtility.SetEditorCurve(currentClip, kv.Key, kv.Value);

                previewEngine?.StartPreview(currentClip, startTime);
                statusLabel.text = $"Preview started.";
            })
            { text = "Start Preview" };

            var stopPreviewBtn = new Button(() =>
            {
                previewEngine?.StopPreview();
                statusLabel.text = "Preview stopped.";
            })
            { text = "Stop Preview" };

            actionBox.Add(applyBtn);
            actionBox.Add(revertBtn);
            actionBox.Add(commitBtn);
            actionBox.Add(previewLabel);
            actionBox.Add(previewBtn);
            actionBox.Add(stopPreviewBtn);

            controlRow.Add(actionBox);

            scrollContent.Add(controlRow);

            // --------------------------------------------------------------------
            // ZOOM ROW
            // --------------------------------------------------------------------
            var zoomRow = new VisualElement();
            zoomRow.style.flexDirection = FlexDirection.Row;
            zoomRow.style.marginTop = 6;
            zoomRow.style.marginLeft = 6;
            zoomRow.style.marginRight = 6;

            zoomSlider = new SliderInt("Zoom %", 25, 400)
            {
                value = zoomPercent
            };
            zoomSlider.style.flexGrow = 1;
            zoomSlider.RegisterValueChangedCallback(e =>
            {
                zoomPercent = e.newValue;
                graphView?.SetZoomFactor(zoomPercent / 100f);
            });

            zoomRow.Add(zoomSlider);
            scrollContent.Add(zoomRow);

            // --------------------------------------------------------------------
            // GRAPH
            // --------------------------------------------------------------------
            var graphContainer = new VisualElement { name = "curve-graph-container" };
            graphContainer.style.flexGrow = 1;
            graphContainer.style.minHeight = 240;

            graphView = new CurveGraphView(OnGraphKeyChanged);
            graphView.style.flexGrow = 1;
            graphView.SetRange(startTime, endTime);
            graphView.SetZoomFactor(zoomPercent / 100f);

            graphContainer.Add(graphView);
            scrollContent.Add(graphContainer);

            // --------------------------------------------------------------------
            // TIMELINE
            // --------------------------------------------------------------------
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

            // --------------------------------------------------------------------
            // STATUS BAR
            // --------------------------------------------------------------------
            var bottomRow = new VisualElement { name = "status-bar" };
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.marginTop = 6;
            bottomRow.style.paddingLeft = 6;
            bottomRow.style.paddingRight = 6;

            statusLabel = new Label("Ready");
            bottomRow.Add(statusLabel);

            // Final assembly
            root.Add(mainScroll);
            root.Add(bottomRow);

            // --------------------------------------------------------------------
            // CRITICAL: Refresh AFTER UI is completely constructed
            // --------------------------------------------------------------------
            RefreshBindings();
        }

        // ------------------------------------------------------------------------
        // New Clip creation
        // ------------------------------------------------------------------------
        private void CreateNewClip()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Animation Clip",
                "NewAnimation",
                "anim",
                "Select where to create the new animation clip.");

            if (string.IsNullOrEmpty(path))
                return;

            var clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();

            currentClip = clip;
            clipField.SetValueWithoutNotify(clip);

            timelineRibbon.SetClip(currentClip);
            RefreshBindings();

            statusLabel.text = $"Created new clip at: {path}";
        }

        // ------------------------------------------------------------------------
        // Material property discovery + binding creation
        // ------------------------------------------------------------------------
        private void BuildMaterialPropertyList()
        {
            materialEntries.Clear();
            currentMaterialEntry = null;
            materialSelectedLabel.text = "<None Selected>";
            addMaterialBindingBtn?.SetEnabled(false);

            if (previewTarget == null)
                return;

            var renderers = previewTarget.GetComponentsInChildren<Renderer>(true);

            foreach (var rend in renderers)
            {
                if (rend == null) continue;

                var mats = rend.sharedMaterials;
                if (mats == null) continue;

                string path = AnimationUtility.CalculateTransformPath(
                    rend.transform,
                    previewTarget.transform
                );

                foreach (var mat in mats)
                {
                    if (mat == null || mat.shader == null) continue;

                    string mName = mat.name;
                    int count = mat.shader.GetPropertyCount();

                    for (int i = 0; i < count; i++)
                    {
                        ShaderPropertyType propType = mat.shader.GetPropertyType(i);

                        bool supported =
                               propType == ShaderPropertyType.Float
                            || propType == ShaderPropertyType.Range
                            || propType == ShaderPropertyType.Color
                            || propType == ShaderPropertyType.Vector;

                        if (!supported)
                            continue;

                        string shaderProp = mat.shader.GetPropertyName(i);

                        string category = MaterialPropertySearchPopup_DetectCategory(shaderProp);

                        string display = $"{mName}  ▸  {category}  ▸  {shaderProp}";

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
                }
            }

            var sorted = materialEntries
                .OrderBy(e => e.materialName)
                .ThenBy(e => e.category)
                .ThenBy(e => e.shaderProperty)
                .ToList();

            materialEntries.Clear();
            materialEntries.AddRange(sorted);
        }

        private string MaterialPropertySearchPopup_DetectCategory(string prop)
        {
            string p = prop.ToLowerInvariant();

            if (p.Contains("emis")) return "Emission";
            if (p.Contains("dissolv")) return "Dissolve";
            if (p.Contains("rim")) return "Rim";
            if (p.Contains("hue") || p.Contains("sat") || p.Contains("color"))
                return "Color";
            if (p.Contains("outline")) return "Outline";
            if (p.StartsWith("al") || p.Contains("audio")) return "AudioLink";
            if (p.Contains("sdf")) return "SDF";
            if (p.Contains("mask")) return "Masking";
            if (p.Contains("smooth") || p.Contains("brdf") || p.Contains("light"))
                return "Shading";

            return "General";
        }

        private void AddBindingFromMaterialProperty()
        {
            if (currentClip == null)
            {
                statusLabel.text = "Cannot add binding: no clip assigned.";
                return;
            }

            if (currentMaterialEntry == null)
            {
                statusLabel.text = "No material property selected.";
                return;
            }

            var opt = currentMaterialEntry;

            var binding = new EditorCurveBinding
            {
                path = opt.path,
                type = opt.type,
                propertyName = $"material.{opt.shaderProperty}"
            };

            if (AnimationUtility.GetEditorCurve(currentClip, binding) != null)
            {
                statusLabel.text = "Binding already exists on this clip.";
                return;
            }

            Undo.RecordObject(currentClip, "Add Material Property Binding");

            var curve = CreateDefaultTwoKeyCurve(binding);
            AnimationUtility.SetEditorCurve(currentClip, binding, curve);
            EditorUtility.SetDirty(currentClip);
            AssetDatabase.SaveAssets();

            RefreshBindings();
            statusLabel.text = $"Added binding: {binding.path} → {binding.propertyName}";
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
                Transform t = string.IsNullOrEmpty(binding.path)
                    ? previewTarget.transform
                    : previewTarget.transform.Find(binding.path);

                if (t != null)
                {
                    var r = t.GetComponent<Renderer>();
                    if (r != null && r.sharedMaterial != null)
                    {
                        string raw = binding.propertyName.Replace("material.", string.Empty);
                        if (r.sharedMaterial.HasProperty(raw))
                        {
                            float matValue = r.sharedMaterial.GetFloat(raw);
                            if (sampleStart) sampledStart = matValue;
                            if (sampleEnd) sampledEnd = matValue;
                        }
                    }
                }
            }

            float sVal = sampleStart ? sampledStart : overrideStartValue;
            float eVal = sampleEnd ? sampledEnd : overrideEndValue;

            var c = new AnimationCurve(
                new Keyframe(sTime, sVal),
                new Keyframe(eTime, eVal)
            );

            for (int i = 0; i < c.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(c, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(c, i, AnimationUtility.TangentMode.Auto);
            }

            return c;
        }

        // ------------------------------------------------------------------------
        // Binding / curve management
        // ------------------------------------------------------------------------
        private void RefreshBindings()
        {
            // If the UI isn't fully constructed yet, bail out safely.
            if (bindingsListContainer == null || graphView == null || timelineRibbon == null || statusLabel == null)
                return;

            allBindings.Clear();
            bindingProfiles.Clear();
            stagedCurves.Clear();

            if (currentClip == null)
            {
                statusLabel.text = "No clip assigned.";
                bindingsListContainer.Clear();
                graphView.SetCurveSet(new Dictionary<EditorCurveBinding, AnimationCurve>());
                timelineRibbon.SetClip(null);
                return;
            }

            // Pull all curve bindings from the clip
            var bindings = AnimationUtility.GetCurveBindings(currentClip);
            allBindings.AddRange(bindings);

            // Resolve default intermediate key count
            int defaultIntermediate = 4;
            if (intermediateDefaultField != null)
                defaultIntermediate = Mathf.Max(0, intermediateDefaultField.value);

            // Resolve default easing from the EnumField if present
            EasingFunctions.EaseType defaultEase = EasingFunctions.EaseType.SmoothStep;
            if (easingDefaultField != null && easingDefaultField.value is Enum ev)
                defaultEase = (EasingFunctions.EaseType)ev;

            // Build profiles + staged curves
            foreach (var b in allBindings)
            {
                var curve = AnimationUtility.GetEditorCurve(currentClip, b);
                AnimationCurve originalCurve =
                    curve != null ? new AnimationCurve(curve.keys) : new AnimationCurve();

                var profile = new BindingProfile
                {
                    binding = b,
                    selected = true,
                    easing = defaultEase,
                    intermediateKeys = defaultIntermediate,
                    originalCurve = originalCurve,
                    currentCurve = originalCurve != null
                        ? new AnimationCurve(originalCurve.keys)
                        : new AnimationCurve()
                };

                bindingProfiles.Add(profile);

                // Staged curve starts as a deep copy of original
                stagedCurves[b] = originalCurve != null
                    ? new AnimationCurve(originalCurve.keys)
                    : new AnimationCurve();
            }

            // Rebuild the bindings list UI
            RebuildBindingsUI();

            // Wire timeline + graph to this clip
            timelineRibbon.SetClip(currentClip);
            statusLabel.text = $"Loaded {bindingProfiles.Count} bindings.";

            graphView.SetCurveSet(stagedCurves);

            // Ensure we have a sensible visible range
            float clipLen = Mathf.Max(currentClip.length, 1f);
            if (endTime <= startTime)
            {
                startTime = 0f;
                endTime = clipLen;
            }

            graphView.SetRange(startTime, endTime);
            graphView.SetZoomFactor(Mathf.Max(0.01f, zoomPercent) / 100f);

            // Let the helper tighten the view around actual keys
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
                inter.RegisterValueChangedCallback(evt =>
                    p.intermediateKeys = Mathf.Max(0, evt.newValue));

                var easingChoices = new List<EasingFunctions.EaseType>(
                    (EasingFunctions.EaseType[])Enum.GetValues(typeof(EasingFunctions.EaseType)));

                var ease = new PopupField<EasingFunctions.EaseType>(easingChoices, p.easing);
                ease.style.width = 110;
                ease.style.maxWidth = 120;
                ease.label = "";
                ease.tooltip = "Easing profile applied when generating intermediate keys.";

                ease.RegisterValueChangedCallback(evt =>
                {
                    p.easing = evt.newValue;
                });

                var sampleBtn = new Button(() =>
                {
                    if (stagedCurves.TryGetValue(p.binding, out var c))
                    {
                        overrideStartValue = c.Evaluate(startTime);
                        overrideEndValue = c.Evaluate(endTime);
                        statusLabel.text =
                            $"Sampled {p.binding.propertyName} start={overrideStartValue:0.000} end={overrideEndValue:0.000}";
                    }
                })
                { text = "Sample" };
                sampleBtn.tooltip =
                    "Sample this curve at Start / End Time and push the values into the override fields.";

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

            foreach (var kv in curves)
                stagedCurves[kv.Key] = kv.Value;

            statusLabel.text = "Staged curves updated from graph edits.";
        }

        private AnimationCurve EnsureCurveExistsForBinding(AnimationClip clip, EditorCurveBinding binding)
        {
            var existing = AnimationUtility.GetEditorCurve(clip, binding);
            if (existing != null)
                return existing;

            var c = CreateDefaultTwoKeyCurve(binding);

            AnimationUtility.SetEditorCurve(clip, binding, c);
            return c;
        }

        private void BuildStagedForSelection()
        {
            if (currentClip == null) return;

            // Fix: Respect user input entirely. Do not clamp to clip.length.
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

                AnimationCurve newCurve = CurveOperations.BuildStretchedCurve(
                    baseCurve,
                    sTime,
                    eTime,
                    sVal,
                    eVal,
                    p.intermediateKeys,
                    p.easing
                );

                stagedCurves[p.binding] = newCurve;
            }

            graphView.SetCurveSet(stagedCurves);
            statusLabel.text = "Generated staged curves for selected bindings.";
            timelineRibbon.SetRange(sTime, eTime);
            graphView.SetRange(sTime, eTime);
            graphView.SetZoomFactor(zoomPercent / 100f);
        }

        private void ApplyStagedToClip()
        {
            if (currentClip == null)
            {
                statusLabel.text = "No clip assigned.";
                return;
            }

            Undo.RecordObject(currentClip, "Apply Staged Curves");

            foreach (var kv in stagedCurves)
                AnimationUtility.SetEditorCurve(currentClip, kv.Key, kv.Value);

            EditorUtility.SetDirty(currentClip);
            AssetDatabase.SaveAssets();

            statusLabel.text = $"Applied {stagedCurves.Count} staged curves to clip.";

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
                AnimationUtility.SetEditorCurve(currentClip, p.binding,
                    new AnimationCurve(p.originalCurve.keys));
            }

            graphView.SetCurveSet(stagedCurves);
            AssetDatabase.SaveAssets();
            statusLabel.text = "Reverted staged curves to original.";
            TryAutoFitGraph();
        }

        private void CommitChanges()
        {
            if (currentClip == null)
            {
                statusLabel.text = "No clip to commit.";
                return;
            }

            Undo.RecordObject(currentClip, "Commit Animation Workbench Changes");

            foreach (var kv in stagedCurves)
                AnimationUtility.SetEditorCurve(currentClip, kv.Key, kv.Value);

            EditorUtility.SetDirty(currentClip);
            AssetDatabase.SaveAssets();
            statusLabel.text = "Committed changes. Use Undo to roll back.";
        }

        [ContextMenu("Generate Staged")]
        public void GenerateStaged()
        {
            BuildStagedForSelection();
        }

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

            if (minT == float.MaxValue || maxT == float.MinValue)
                return;

            //the span calculation to enforce a minimum readable width:
            float span = Mathf.Max(1f, maxT - minT);    // Changed from 0.001f to 1f
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