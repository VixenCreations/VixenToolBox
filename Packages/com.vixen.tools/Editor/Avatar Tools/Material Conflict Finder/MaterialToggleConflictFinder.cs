#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vixforge.Toolkit.Diagnostics
{
    public class MaterialToggleConflictFinder : EditorWindow
    {
        private const string USS_PATH = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/MaterialConflictFinderStyles.uss";

        private ScrollView _mainScrollView;
        private VisualElement _contentContainer;
        private GameObject _targetObject;

        // Diagnostic Data
        private Dictionary<string, List<MaterialPropertyValue>> propertyMap = new Dictionary<string, List<MaterialPropertyValue>>();
        private List<MaterialKeywordMismatch> keywordMismatches = new List<MaterialKeywordMismatch>();
        private List<AnimationMaterialConflict> animationConflicts = new List<AnimationMaterialConflict>();
        private List<DiscoveredMaterialInfo> materialInventory = new List<DiscoveredMaterialInfo>();

        [MenuItem("VixenTools/Avatars/Material Conflict Finder", priority = 44)]
        public static void ShowWindow()
        {
            var window = GetWindow<MaterialToggleConflictFinder>("Conflict Finder");
            window.minSize = new Vector2(540, 680);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.AddToClassList("mcf-root");

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning($"[Vixforge Toolkit] Could not locate USS at {USS_PATH}. UI may not render correctly.");
            }

            _mainScrollView = new ScrollView();
            _mainScrollView.AddToClassList("mcf-scroll-view");
            root.Add(_mainScrollView);

            _contentContainer = new VisualElement();
            _mainScrollView.Add(_contentContainer);

            BuildUI();
        }

        private void BuildUI()
        {
            _contentContainer.Clear();

            _contentContainer.Add(GenerateHeader());

            var targetField = new ObjectField("Target Avatar Root")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true,
                value = _targetObject
            };
            targetField.RegisterValueChangedCallback(evt =>
            {
                _targetObject = evt.newValue as GameObject;
            });
            targetField.style.marginBottom = 10;
            _contentContainer.Add(targetField);

            Button scanButton = new Button(() =>
            {
                if (_targetObject != null)
                {
                    AnalyzeTarget(_targetObject);
                }
                else
                {
                    Debug.LogWarning("[Vixforge Toolkit] Please assign a Target Avatar Root GameObject.");
                }
            })
            {
                text = "Analyze Materials & Toggles"
            };
            scanButton.AddToClassList("mcf-action-btn-main");
            _contentContainer.Add(scanButton);

            if (propertyMap.Count == 0 && keywordMismatches.Count == 0 && animationConflicts.Count == 0 && materialInventory.Count == 0)
            {
                var helpLabel = new Label("Assign an avatar root and click 'Analyze Materials & Toggles' to run the diagnostic sweep.");
                helpLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                helpLabel.style.marginTop = 10;
                helpLabel.style.whiteSpace = WhiteSpace.Normal;
                _contentContainer.Add(helpLabel);
                return;
            }

            DrawCrossMaterialConflicts_UI();
            DrawKeywordMismatches_UI();
            DrawAnimationDrivenConflicts_UI();
            DrawMaterialInventory_UI();
        }

        #region UI Toolkit Builders

        private VisualElement GenerateHeader()
        {
            var headerBox = new VisualElement();
            headerBox.AddToClassList("mcf-header");

            var title = new Label("MATERIAL TOGGLE CONFLICT FINDER");
            title.AddToClassList("mcf-header-title");

            var subtitle = new Label("Vixforge Toolkit Avatar Pipeline • VRCFury & Animation Layer Diagnostic");
            subtitle.AddToClassList("mcf-header-subtitle");

            headerBox.Add(title);
            headerBox.Add(subtitle);

            return headerBox;
        }

        private void DrawCrossMaterialConflicts_UI()
        {
            var conflictingProperties = propertyMap
                .Where(kvp => kvp.Value.Select(v => v.value).Distinct().Count() > 1)
                .ToList();

            var foldout = new Foldout
            {
                text = $"Cross-Material Property Mismatches ({conflictingProperties.Count})",
                value = true
            };
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;

            if (conflictingProperties.Count == 0)
            {
                foldout.Add(new Label("No cross-material float toggle conflicts found.") { style = { color = Color.gray } });
            }
            else
            {
                foreach (var kvp in conflictingProperties)
                {
                    string propName = kvp.Key;
                    List<MaterialPropertyValue> values = kvp.Value;
                    var distinctValues = values.Select(v => v.value).Distinct().OrderBy(v => v).ToList();

                    var panel = new VisualElement();
                    panel.AddToClassList("mcf-panel");
                    panel.AddToClassList("mcf-panel-warning");

                    var title = new Label($"State Mismatch: {propName} [{values.Count} Materials, {distinctValues.Count} States]");
                    title.style.unityFontStyleAndWeight = FontStyle.Bold;
                    title.style.color = new Color(1.0f, 0.82f, 0.28f);
                    panel.Add(title);

                    panel.Add(GenerateRecommendationsBox_UI(propName, values, distinctValues));

                    var breakdownLabel = new Label("Affected Materials Breakdown:");
                    breakdownLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    breakdownLabel.style.marginTop = 6;
                    breakdownLabel.style.marginBottom = 4;
                    panel.Add(breakdownLabel);

                    foreach (var distinctVal in distinctValues)
                    {
                        var matsWithVal = values.Where(v => v.value == distinctVal).Select(v => v.material).ToList();
                        string valLabel = distinctVal == 1f ? "1.0 (Enabled)" : distinctVal == 0f ? "0.0 (Disabled)" : $"{distinctVal}";

                        var valFoldout = new Foldout
                        {
                            text = $"Value [{valLabel}] — ({matsWithVal.Count} {(matsWithVal.Count == 1 ? "Material" : "Materials")})",
                            value = false
                        };

                        var selectAllBtn = new Button(() => Selection.objects = matsWithVal.ToArray()) { text = "Select All" };
                        selectAllBtn.style.height = 20;
                        selectAllBtn.style.width = 70;

                        var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
                        headerRow.Add(valFoldout);
                        headerRow.Add(selectAllBtn);
                        panel.Add(headerRow);

                        var foldoutContent = new VisualElement();
                        valFoldout.Add(foldoutContent);

                        foreach (var mat in matsWithVal)
                        {
                            var row = new VisualElement();
                            row.AddToClassList("mcf-data-row");

                            var matField = new ObjectField { objectType = typeof(Material), value = mat };
                            matField.SetEnabled(false);
                            matField.style.flexGrow = 1;

                            var pingBtn = new Button(() => EditorGUIUtility.PingObject(mat)) { text = "Ping" };
                            pingBtn.style.width = 42;

                            row.Add(matField);
                            row.Add(pingBtn);
                            foldoutContent.Add(row);
                        }
                    }
                    foldout.Add(panel);
                }
            }
            _contentContainer.Add(foldout);
        }

        private VisualElement GenerateRecommendationsBox_UI(string propName, List<MaterialPropertyValue> values, List<float> distinctValues)
        {
            var recBox = new VisualElement();
            recBox.AddToClassList("mcf-recommendation-box");

            var title = new Label("Recommended Actions & Quick Fixes");
            title.AddToClassList("mcf-recommendation-title");
            recBox.Add(title);

            float majorityVal = values.GroupBy(v => v.value)
                                      .OrderByDescending(g => g.Count())
                                      .First().Key;
            int majorityCount = values.Count(v => v.value == majorityVal);

            var info = new Label($"Mismatched shader toggle properties across materials sharing a shader family break GPU Instancing and SRP Batching. Standardize '{propName}' unless intentionally toggled per renderer.");
            info.style.whiteSpace = WhiteSpace.Normal;
            info.style.marginBottom = 6;
            recBox.Add(info);

            var btnGroup = new VisualElement();
            btnGroup.AddToClassList("mcf-sync-btn-group");

            var syncZeroBtn = new Button(() => BatchSetPropertyValue(values, propName, 0f)) { text = "Sync All to 0.0 (OFF)" };
            syncZeroBtn.AddToClassList("mcf-btn-sync");

            var syncOneBtn = new Button(() => BatchSetPropertyValue(values, propName, 1f)) { text = "Sync All to 1.0 (ON)" };
            syncOneBtn.AddToClassList("mcf-btn-sync");

            var alignMajorityBtn = new Button(() => BatchSetPropertyValue(values, propName, majorityVal)) { text = $"Align Majority [{majorityVal}]" };
            alignMajorityBtn.AddToClassList("mcf-btn-sync");

            btnGroup.Add(syncZeroBtn);
            btnGroup.Add(syncOneBtn);
            btnGroup.Add(alignMajorityBtn);
            recBox.Add(btnGroup);

            return recBox;
        }

        private void DrawKeywordMismatches_UI()
        {
            var foldout = new Foldout
            {
                text = $"Orphaned Shader Keywords ({keywordMismatches.Count})",
                value = true
            };
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            foldout.style.marginTop = 10;

            if (keywordMismatches.Count == 0)
            {
                foldout.Add(new Label("No orphaned shader keywords detected.") { style = { color = Color.gray } });
            }
            else
            {
                var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 8 } };
                var info = new Label("Orphaned keywords occur when a float toggle is set to 0, but Unity keeps the keyword active. This breaks batching.");
                info.style.whiteSpace = WhiteSpace.Normal;
                info.style.flexShrink = 1;

                var fixAllBtn = new Button(() => FixAllKeywordMismatches()) { text = "Fix All Keywords" };
                fixAllBtn.AddToClassList("mcf-btn-fix");
                fixAllBtn.style.width = 110;
                fixAllBtn.style.height = 32;

                headerRow.Add(info);
                headerRow.Add(fixAllBtn);
                foldout.Add(headerRow);

                foreach (var mismatch in keywordMismatches)
                {
                    var row = new VisualElement();
                    row.AddToClassList("mcf-data-row");

                    var matField = new ObjectField { objectType = typeof(Material), value = mismatch.material };
                    matField.SetEnabled(false);
                    matField.style.width = 150;

                    var desc = new Label($"Prop: {mismatch.propertyName} = 0 | Active Keyword: {mismatch.keyword}");
                    desc.AddToClassList("mcf-label-mini");
                    desc.style.flexGrow = 1;

                    var fixBtn = new Button(() =>
                    {
                        mismatch.material.DisableKeyword(mismatch.keyword);
                        EditorUtility.SetDirty(mismatch.material);
                        AssetDatabase.SaveAssets();
                        AnalyzeTarget(_targetObject);
                    })
                    { text = "Fix" };
                    fixBtn.AddToClassList("mcf-btn-fix");

                    row.Add(matField);
                    row.Add(desc);
                    row.Add(fixBtn);
                    foldout.Add(row);
                }
            }
            _contentContainer.Add(foldout);
        }

        private void DrawAnimationDrivenConflicts_UI()
        {
            var foldout = new Foldout
            {
                text = $"Animation & VRCFury Driven Toggle Conflicts ({animationConflicts.Count})",
                value = true
            };
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            foldout.style.marginTop = 10;

            if (animationConflicts.Count == 0)
            {
                foldout.Add(new Label("No animation curve vs. shader keyword desyncs detected.") { style = { color = Color.gray } });
            }
            else
            {
                foreach (var conflict in animationConflicts)
                {
                    var panel = new VisualElement();
                    panel.AddToClassList("mcf-panel");
                    panel.AddToClassList("mcf-panel-error");

                    var title = new Label($"Desync: {conflict.material.name} ← {conflict.clipName}");
                    title.style.unityFontStyleAndWeight = FontStyle.Bold;
                    title.style.color = new Color(1f, 0.45f, 0.45f);

                    var context = new Label($"Source Context: {conflict.sourceContext}");
                    context.AddToClassList("mcf-label-mini");

                    var issue = new Label(conflict.issue);
                    issue.style.whiteSpace = WhiteSpace.Normal;
                    issue.style.color = new Color(0.8f, 0.8f, 0.8f);

                    panel.Add(title);
                    panel.Add(context);
                    panel.Add(issue);
                    foldout.Add(panel);
                }
            }
            _contentContainer.Add(foldout);
        }

        private void DrawMaterialInventory_UI()
        {
            var foldout = new Foldout
            {
                text = $"Scanned Materials Inventory ({materialInventory.Count})",
                value = false
            };
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            foldout.style.marginTop = 10;

            foreach (var info in materialInventory)
            {
                var row = new VisualElement();
                row.AddToClassList("mcf-data-row");

                var matField = new ObjectField { objectType = typeof(Material), value = info.material };
                matField.SetEnabled(false);
                matField.style.width = 180;

                var desc = new Label($"[{info.sourceType}] {info.sourceName}");
                desc.AddToClassList("mcf-label-mini");

                row.Add(matField);
                row.Add(desc);
                foldout.Add(row);
            }
            _contentContainer.Add(foldout);
        }

        #endregion

        #region Core Diagnostic Engine

        private void AnalyzeTarget(GameObject root)
        {
            propertyMap.Clear();
            _effectiveKeywords.Clear();
            keywordMismatches.Clear();
            animationConflicts.Clear();
            materialInventory.Clear();

            HashSet<Material> uniqueMaterials = new HashSet<Material>();
            List<AnimatedMaterialProperty> animatedProps = new List<AnimatedMaterialProperty>();

            // Intercept and suppress Unity's native material drawer instantiation error spam from malformed third-party shaders
            Application.LogCallback logInterceptor = (condition, stackTrace, type) =>
            {
                if (type == LogType.Error && condition.Contains("Failed to create material drawer Helpbox"))
                {
                    return; // Suppress harmless native drawer reflection errors from external shaders
                }
                if (type == LogType.Warning && condition.Contains("Failed to create material drawer Helpbox"))
                {
                    return; // Suppress harmless native drawer reflection errors from external shaders
                }
            };
            Application.logMessageReceived += logInterceptor;

            try
            {
                ScanRenderers(root, uniqueMaterials);
                ScanAnimatorsAndLayers(root, uniqueMaterials, animatedProps);
                ScanVRCFuryComponents(root, uniqueMaterials, animatedProps);

                foreach (Material mat in uniqueMaterials)
                {
                    AnalyzeMaterial(mat);
                }

                EvaluateAnimationConflicts(animatedProps, uniqueMaterials);
            }
            finally
            {
                Application.logMessageReceived -= logInterceptor;
            }

            BuildUI(); // Rebuild DOM after scan
        }

        private void ScanRenderers(GameObject root, HashSet<Material> uniqueMaterials)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var rend in renderers)
            {
                string path = GetGameObjectPath(rend.gameObject, root);
                foreach (var mat in rend.sharedMaterials)
                {
                    if (mat == null) continue;
                    uniqueMaterials.Add(mat);

                    materialInventory.Add(new DiscoveredMaterialInfo
                    {
                        material = mat,
                        sourceType = SourceType.Renderer,
                        sourceName = $"{rend.gameObject.name} ({rend.GetType().Name})",
                        contextPath = path
                    });
                }
            }
        }

        private void ScanAnimatorsAndLayers(GameObject root, HashSet<Material> uniqueMaterials, List<AnimatedMaterialProperty> animatedProps)
        {
            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            foreach (var anim in animators)
            {
                if (anim.runtimeAnimatorController == null) continue;
                ProcessRuntimeAnimatorController(anim.runtimeAnimatorController, $"{anim.gameObject.name} (Animator)", uniqueMaterials, animatedProps, root);
            }

            Component descriptor = root.GetComponents<Component>()
                .FirstOrDefault(c => c != null && c.GetType().Name == "VRCAvatarDescriptor");

            if (descriptor != null)
            {
                SerializedObject so = new SerializedObject(descriptor);
                ScanPlayableLayerArray(so.FindProperty("baseAnimationLayers"), "VRC Base Layer", descriptor.gameObject, uniqueMaterials, animatedProps, root);
                ScanPlayableLayerArray(so.FindProperty("specialAnimationLayers"), "VRC Special Layer", descriptor.gameObject, uniqueMaterials, animatedProps, root);
            }
        }

        private void ScanPlayableLayerArray(SerializedProperty layerArrayProp, string layerCategory, GameObject avatarObj, HashSet<Material> uniqueMaterials, List<AnimatedMaterialProperty> animatedProps, GameObject root)
        {
            if (layerArrayProp == null || !layerArrayProp.isArray) return;

            for (int i = 0; i < layerArrayProp.arraySize; i++)
            {
                SerializedProperty layerProp = layerArrayProp.GetArrayElementAtIndex(i);
                SerializedProperty isDefaultProp = layerProp.FindPropertyRelative("isDefault");
                SerializedProperty controllerProp = layerProp.FindPropertyRelative("animatorController");

                if (isDefaultProp != null && isDefaultProp.boolValue) continue;
                if (controllerProp != null && controllerProp.objectReferenceValue is RuntimeAnimatorController rac)
                {
                    ProcessRuntimeAnimatorController(rac, $"{layerCategory} [{rac.name}]", uniqueMaterials, animatedProps, root);
                }
            }
        }

        private void ProcessRuntimeAnimatorController(RuntimeAnimatorController rac, string sourceName, HashSet<Material> uniqueMaterials, List<AnimatedMaterialProperty> animatedProps, GameObject root)
        {
            foreach (AnimationClip clip in rac.animationClips)
            {
                if (clip == null) continue;
                AnalyzeAnimationClip(clip, sourceName, uniqueMaterials, animatedProps, root);
            }
        }

        private void ScanVRCFuryComponents(GameObject root, HashSet<Material> uniqueMaterials, List<AnimatedMaterialProperty> animatedProps)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            foreach (var comp in components)
            {
                if (comp == null) continue;
                Type compType = comp.GetType();
                string typeName = compType.FullName ?? compType.Name;

                if (!typeName.Contains("VRCFury") && !typeName.StartsWith("VF.")) continue;

                SerializedObject so = new SerializedObject(comp);
                SerializedProperty prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (prop.objectReferenceValue is Material mat && mat != null)
                        {
                            uniqueMaterials.Add(mat);
                            materialInventory.Add(new DiscoveredMaterialInfo
                            {
                                material = mat,
                                sourceType = SourceType.VRCFury,
                                sourceName = $"{comp.gameObject.name} ({compType.Name})",
                                contextPath = prop.displayName
                            });
                        }
                        else if (prop.objectReferenceValue is AnimationClip clip && clip != null)
                        {
                            AnalyzeAnimationClip(clip, $"VRCFury ({comp.gameObject.name})", uniqueMaterials, animatedProps, root);
                        }
                    }
                }
            }
        }

        private void AnalyzeAnimationClip(AnimationClip clip, string sourceContext, HashSet<Material> uniqueMaterials, List<AnimatedMaterialProperty> animatedProps, GameObject root)
        {
            EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in objectBindings)
            {
                if (binding.type != null && typeof(Renderer).IsAssignableFrom(binding.type))
                {
                    ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    if (keyframes == null) continue;

                    foreach (var frame in keyframes)
                    {
                        if (frame.value is Material mat && mat != null)
                        {
                            uniqueMaterials.Add(mat);
                            materialInventory.Add(new DiscoveredMaterialInfo
                            {
                                material = mat,
                                sourceType = SourceType.AnimationClip,
                                sourceName = $"{clip.name} (Swap)",
                                contextPath = $"{sourceContext} -> {binding.path}"
                            });
                        }
                    }
                }
            }

            EditorCurveBinding[] floatBindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in floatBindings)
            {
                if (binding.propertyName.StartsWith("material.") || binding.propertyName.StartsWith("m_SavedProperties.Float"))
                {
                    string propName = ExtractPropertyNameFromBinding(binding.propertyName);
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve == null || curve.keys.Length == 0) continue;

                    float minVal = curve.keys.Min(k => k.value);
                    float maxVal = curve.keys.Max(k => k.value);

                    animatedProps.Add(new AnimatedMaterialProperty
                    {
                        clip = clip,
                        propertyName = propName,
                        targetPath = binding.path,
                        sourceContext = sourceContext,
                        minValue = minVal,
                        maxValue = maxVal
                    });
                }
            }
        }

        private void AnalyzeMaterial(Material mat)
        {
            Shader shader = mat.shader;
            if (shader == null) return;

            int propCount = shader.GetPropertyCount();

            for (int i = 0; i < propCount; i++)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Float) continue;

                string propName = shader.GetPropertyName(i);
                
                string[] attributes = null;
                try
                {
                    attributes = shader.GetPropertyAttributes(i);
                }
                catch (Exception)
                {
                    attributes = new string[0];
                }

                bool isToggle = attributes.Any(a => a.Contains("Toggle")) ||
                                propName.ToLower().Contains("toggle") ||
                                propName.ToLower().Contains("enable") ||
                                propName.StartsWith("_Use") ||
                                propName.StartsWith("_Is");

                if (isToggle)
                {
                    int propID = Shader.PropertyToID(propName);
                    float value = mat.HasProperty(propID) ? mat.GetFloat(propID) : 0f;

                    if (!propertyMap.ContainsKey(propName))
                        propertyMap[propName] = new List<MaterialPropertyValue>();

                    propertyMap[propName].Add(new MaterialPropertyValue { material = mat, value = value });

                    string toggleAttr = attributes.FirstOrDefault(a => a.StartsWith("Toggle("));
                    string keyword = null;

                    if (!string.IsNullOrEmpty(toggleAttr))
                    {
                        keyword = toggleAttr.Replace("Toggle(", "").Replace(")", "").Trim();
                    }
                    else if (attributes.Any(a => a.Equals("Toggle", StringComparison.OrdinalIgnoreCase)))
                    {
                        keyword = propName.ToUpper() + "_ON";
                    }

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        bool hasKeyword = HasEffectiveKeyword(mat, keyword);

                        if (value == 0f && hasKeyword)
                        {
                            keywordMismatches.Add(new MaterialKeywordMismatch
                            {
                                material = mat,
                                propertyName = propName,
                                keyword = keyword,
                                currentFloatValue = value,
                                keywordState = true
                            });
                        }
                    }
                }
            }
        }

        // A locked material carries NO keywords. The locker bakes their state into the generated
        // shader and stashes the original list in an "OriginalKeywords" override tag, so
        // IsKeywordEnabled reports every keyword as disabled on one. Testing that directly flagged
        // one desync per animated toggle per material on any locked avatar, none of them real.
        private const string LockedShaderPrefix = "Hidden/Locked/";
        private const string OriginalKeywordsTag = "OriginalKeywords";
        private const string AnimatedTagSuffix = "Animated";

        private readonly Dictionary<Material, HashSet<string>> _effectiveKeywords =
            new Dictionary<Material, HashSet<string>>();

        private static bool IsMaterialLocked(Material mat)
        {
            return mat != null && mat.shader != null
                && mat.shader.name.StartsWith(LockedShaderPrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// The material's keyword state, read from the pre-lock stash when the material is locked.
        /// </summary>
        private bool HasEffectiveKeyword(Material mat, string keyword)
        {
            if (mat == null || string.IsNullOrEmpty(keyword)) return false;

            if (!_effectiveKeywords.TryGetValue(mat, out HashSet<string> keywords))
            {
                if (IsMaterialLocked(mat))
                {
                    string stashed = mat.GetTag(OriginalKeywordsTag, false, "");
                    keywords = new HashSet<string>(stashed.Split(' ').Where(k => !string.IsNullOrEmpty(k)));
                }
                else
                {
                    keywords = new HashSet<string>(mat.shaderKeywords);
                }

                _effectiveKeywords[mat] = keywords;
            }

            return keywords.Contains(keyword);
        }

        /// <summary>
        /// True when the locker was told to keep this property live instead of baking it to a literal,
        /// which is what makes a clip able to drive it after the lock. Nothing to report in that case.
        /// </summary>
        private static bool IsPropertyMarkedAnimated(Material mat, string propertyName)
        {
            if (mat == null || string.IsNullOrEmpty(propertyName)) return false;

            string tag = mat.GetTag(propertyName + AnimatedTagSuffix, false, "");
            return tag == "1" || tag == "2";
        }

        private void EvaluateAnimationConflicts(List<AnimatedMaterialProperty> animatedProps, HashSet<Material> uniqueMaterials)
        {
            foreach (var animProp in animatedProps)
            {
                foreach (var mat in uniqueMaterials)
                {
                    if (!mat.HasProperty(animProp.propertyName)) continue;

                    Shader shader = mat.shader;
                    if (shader == null) continue;

                    int propIdx = shader.FindPropertyIndex(animProp.propertyName);
                    if (propIdx == -1) continue;

                    string[] attributes = null;
                    try
                    {
                        attributes = shader.GetPropertyAttributes(propIdx);
                    }
                    catch (Exception)
                    {
                        attributes = new string[0];
                    }

                    string toggleAttr = attributes.FirstOrDefault(a => a.StartsWith("Toggle("));

                    if (!string.IsNullOrEmpty(toggleAttr))
                    {
                        string keyword = toggleAttr.Replace("Toggle(", "").Replace(")", "").Trim();

                        // The lock kept this property live on purpose, so the clip still drives it.
                        if (IsPropertyMarkedAnimated(mat, animProp.propertyName)) continue;

                        bool hasKeyword = HasEffectiveKeyword(mat, keyword);

                        if (animProp.maxValue > 0f && !hasKeyword)
                        {
                            animationConflicts.Add(new AnimationMaterialConflict
                            {
                                material = mat,
                                propertyName = animProp.propertyName,
                                keyword = keyword,
                                clipName = animProp.clip.name,
                                sourceContext = animProp.sourceContext,
                                issue = IsMaterialLocked(mat)
                                    ? $"Clip '{animProp.clip.name}' drives '{animProp.propertyName}' to 1.0, but '{mat.name}' was locked while '{keyword}' was off, so the lock baked that state in. Unlock, switch it on or mark the property animated, then lock again."
                                    : $"Clip '{animProp.clip.name}' drives '{animProp.propertyName}' to 1.0, but keyword '{keyword}' is statically disabled on material '{mat.name}'."
                            });
                        }
                    }
                }
            }
        }

        #endregion

        #region Core Fix Actions

        private void BatchSetPropertyValue(List<MaterialPropertyValue> values, string propName, float newValue)
        {
            foreach (var item in values)
            {
                if (item.material != null && item.material.HasProperty(propName))
                {
                    Undo.RecordObject(item.material, $"Set {propName} on {item.material.name}");
                    item.material.SetFloat(propName, newValue);

                    Shader shader = item.material.shader;
                    int idx = shader.FindPropertyIndex(propName);
                    if (idx != -1)
                    {
                        string[] attrs = null;
                        try
                        {
                            attrs = shader.GetPropertyAttributes(idx);
                        }
                        catch (Exception)
                        {
                            attrs = new string[0];
                        }

                        string toggleAttr = attrs.FirstOrDefault(a => a.StartsWith("Toggle("));
                        string keyword = !string.IsNullOrEmpty(toggleAttr)
                            ? toggleAttr.Replace("Toggle(", "").Replace(")", "").Trim()
                            : (attrs.Any(a => a.Equals("Toggle", StringComparison.OrdinalIgnoreCase)) ? propName.ToUpper() + "_ON" : null);

                        if (!string.IsNullOrEmpty(keyword))
                        {
                            if (newValue > 0f) item.material.EnableKeyword(keyword);
                            else item.material.DisableKeyword(keyword);
                        }
                    }

                    EditorUtility.SetDirty(item.material);
                }
            }

            AssetDatabase.SaveAssets();
            AnalyzeTarget(_targetObject);
        }

        private void FixAllKeywordMismatches()
        {
            foreach (var mismatch in keywordMismatches)
            {
                mismatch.material.DisableKeyword(mismatch.keyword);
                EditorUtility.SetDirty(mismatch.material);
            }
            AssetDatabase.SaveAssets();
            AnalyzeTarget(_targetObject);
        }

        #endregion

        #region Helpers & Data Structures

        private static string GetGameObjectPath(GameObject obj, GameObject root)
        {
            if (obj == root || obj.transform.parent == null) return obj.name;
            return GetGameObjectPath(obj.transform.parent.gameObject, root) + "/" + obj.name;
        }

        private static string ExtractPropertyNameFromBinding(string bindingProp)
        {
            if (bindingProp.StartsWith("material.")) return bindingProp.Replace("material.", "");
            int lastIndex = bindingProp.LastIndexOf('.');
            if (lastIndex != -1) return bindingProp.Substring(lastIndex + 1);
            return bindingProp;
        }

        private enum SourceType
        {
            Renderer,
            AnimatorClip,
            AnimationClip,
            VRCFury
        }

        private struct MaterialPropertyValue
        {
            public Material material;
            public float value;
        }

        private struct MaterialKeywordMismatch
        {
            public Material material;
            public string propertyName;
            public string keyword;
            public float currentFloatValue;
            public bool keywordState;
        }

        private struct AnimatedMaterialProperty
        {
            public AnimationClip clip;
            public string propertyName;
            public string targetPath;
            public string sourceContext;
            public float minValue;
            public float maxValue;
        }

        private struct AnimationMaterialConflict
        {
            public Material material;
            public string propertyName;
            public string keyword;
            public string clipName;
            public string sourceContext;
            public string issue;
        }

        private struct DiscoveredMaterialInfo
        {
            public Material material;
            public SourceType sourceType;
            public string sourceName;
            public string contextPath;
        }

        #endregion
    }
}
#endif