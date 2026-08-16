#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VixenTools.Editor
{
    public static class AnimatorDoctor
    {
        public enum Severity { Error, Warning, Info }

        public class AnimatorFinding
        {
            public Severity Severity = Severity.Warning;
            public string Category = "general";
            public string Title = "";
            public string Detail = "";
            public UnityEngine.Object Context;
            public Action Fix;
            public string FixLabel = "FIX";
            public bool IsSafe;
        }

        public static readonly Dictionary<string, AnimatorControllerParameterType> BuiltInParameters =
            new Dictionary<string, AnimatorControllerParameterType>
        {
            { "IsLocal", AnimatorControllerParameterType.Bool },
            { "PreviewMode", AnimatorControllerParameterType.Int },
            { "Viseme", AnimatorControllerParameterType.Int },
            { "Voice", AnimatorControllerParameterType.Float },
            { "GestureLeft", AnimatorControllerParameterType.Int },
            { "GestureRight", AnimatorControllerParameterType.Int },
            { "GestureLeftWeight", AnimatorControllerParameterType.Float },
            { "GestureRightWeight", AnimatorControllerParameterType.Float },
            { "AngularY", AnimatorControllerParameterType.Float },
            { "VelocityX", AnimatorControllerParameterType.Float },
            { "VelocityY", AnimatorControllerParameterType.Float },
            { "VelocityZ", AnimatorControllerParameterType.Float },
            { "VelocityMagnitude", AnimatorControllerParameterType.Float },
            { "Upright", AnimatorControllerParameterType.Float },
            { "Grounded", AnimatorControllerParameterType.Bool },
            { "Seated", AnimatorControllerParameterType.Bool },
            { "AFK", AnimatorControllerParameterType.Bool },
            { "TrackingType", AnimatorControllerParameterType.Int },
            { "VRMode", AnimatorControllerParameterType.Int },
            { "MuteSelf", AnimatorControllerParameterType.Bool },
            { "InStation", AnimatorControllerParameterType.Bool },
            { "Earmuffs", AnimatorControllerParameterType.Bool },
            { "IsOnFriendsList", AnimatorControllerParameterType.Bool },
            { "AvatarVersion", AnimatorControllerParameterType.Int },
            { "ScaleModified", AnimatorControllerParameterType.Bool },
            { "ScaleFactor", AnimatorControllerParameterType.Float },
            { "ScaleFactorInverse", AnimatorControllerParameterType.Float },
            { "EyeHeightAsMeters", AnimatorControllerParameterType.Float },
            { "EyeHeightAsPercent", AnimatorControllerParameterType.Float },
        };

        public struct ControllerRef
        {
            public AnimatorController Controller;
            public VRCAvatarDescriptor.AnimLayerType Type;
        }

        public static List<ControllerRef> CollectControllers(VRCAvatarDescriptor descriptor)
        {
            var list = new List<ControllerRef>();
            if (descriptor == null) return list;

            void Consume(VRCAvatarDescriptor.CustomAnimLayer[] layers)
            {
                if (layers == null) return;
                foreach (var layer in layers)
                {
                    var ac = layer.animatorController as AnimatorController;
                    if (ac == null) continue;
                    if (list.Any(r => r.Controller == ac)) continue;
                    list.Add(new ControllerRef { Controller = ac, Type = layer.type });
                }
            }

            Consume(descriptor.baseAnimationLayers);
            Consume(descriptor.specialAnimationLayers);
            return list;
        }

        public static List<AnimatorFinding> RunDiagnostics(GameObject avatarRoot)
        {
            var findings = new List<AnimatorFinding>();
            if (avatarRoot == null) return findings;

            var descriptor = avatarRoot.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null)
            {
                findings.Add(new AnimatorFinding
                {
                    Severity = Severity.Error,
                    Category = "descriptor",
                    Title = "No VRCAvatarDescriptor",
                    Detail = "Select an avatar with a VRCAvatarDescriptor to diagnose its animators.",
                    Context = avatarRoot
                });
                return findings;
            }

            var controllers = CollectControllers(descriptor);

            CheckMissingParameters(controllers, findings);
            CheckWriteDefaults(controllers, findings);
            CheckEmptyWriteDefaultsOffClips(controllers, findings);
            CheckExpressionSync(descriptor, controllers, findings);
            CheckMenu(descriptor, findings);
            CheckLayers(controllers, findings);
            CheckTransitions(controllers, findings);

            return findings
                .OrderBy(f => (int)f.Severity)
                .ToList();
        }

        private class ParamUsage
        {
            public HashSet<AnimatorConditionMode> ConditionModes = new HashSet<AnimatorConditionMode>();
            public bool UsedAsFloat;
        }

        private static void CheckMissingParameters(List<ControllerRef> controllers, List<AnimatorFinding> findings)
        {
            foreach (var cref in controllers)
            {
                var controller = cref.Controller;
                var declared = new HashSet<string>(controller.parameters.Select(p => p.name));

                var referenced = new Dictionary<string, ParamUsage>();
                void Note(string name, AnimatorConditionMode? mode, bool asFloat)
                {
                    if (string.IsNullOrEmpty(name)) return;
                    if (!referenced.TryGetValue(name, out var usage))
                    {
                        usage = new ParamUsage();
                        referenced[name] = usage;
                    }
                    if (mode.HasValue) usage.ConditionModes.Add(mode.Value);
                    if (asFloat) usage.UsedAsFloat = true;
                }

                foreach (var layer in controller.layers)
                {
                    if (layer.stateMachine == null) continue;
                    foreach (var transition in AllTransitions(layer.stateMachine))
                        foreach (var condition in transition.conditions)
                            Note(condition.parameter, condition.mode, false);

                    foreach (var state in AllStates(layer.stateMachine))
                    {
                        if (state.speedParameterActive) Note(state.speedParameter, null, true);
                        if (state.mirrorParameterActive) Note(state.mirrorParameter, null, false);
                        if (state.timeParameterActive) Note(state.timeParameter, null, true);
                        if (state.cycleOffsetParameterActive) Note(state.cycleOffsetParameter, null, true);

                        foreach (var tree in AllTrees(state.motion as BlendTree))
                        {
                            if (tree.blendType == BlendTreeType.Direct)
                            {
                                foreach (var child in tree.children) Note(child.directBlendParameter, null, true);
                            }
                            else
                            {
                                Note(tree.blendParameter, null, true);
                                if (tree.blendType != BlendTreeType.Simple1D) Note(tree.blendParameterY, null, true);
                            }
                        }
                    }
                }

                foreach (var pair in referenced)
                {
                    if (declared.Contains(pair.Key)) continue;

                    string name = pair.Key;
                    bool isBuiltIn = BuiltInParameters.TryGetValue(name, out var builtInType);
                    var resolvedType = isBuiltIn ? builtInType : InferParameterType(pair.Value);

                    var finding = new AnimatorFinding
                    {
                        Severity = Severity.Error,
                        Category = "missing-parameter",
                        Context = controller,
                        IsSafe = isBuiltIn,
                        FixLabel = "ADD PARAM"
                    };

                    if (isBuiltIn)
                    {
                        finding.Title = $"'{controller.name}' is missing built-in parameter '{name}'";
                        finding.Detail = $"Transitions reference the VRChat built-in parameter '{name}' but it is not declared in the controller. " +
                                         $"Add it as {resolvedType} to clear the SDK error.";
                    }
                    else
                    {
                        finding.Title = $"'{controller.name}' references undefined parameter '{name}'";
                        finding.Detail = $"'{name}' is used by a transition or blend tree but not declared, and is not a VRChat built-in. " +
                                         $"It may be a typo or a renamed parameter. Inferred type: {resolvedType}. Review before adding.";
                    }

                    var capturedController = controller;
                    var capturedName = name;
                    var capturedType = resolvedType;
                    finding.Fix = () =>
                    {
                        if (capturedController.parameters.Any(p => p.name == capturedName)) return;
                        capturedController.AddParameter(capturedName, capturedType);
                        EditorUtility.SetDirty(capturedController);
                    };

                    findings.Add(finding);
                }
            }
        }

        private static AnimatorControllerParameterType InferParameterType(ParamUsage usage)
        {
            if (usage.UsedAsFloat) return AnimatorControllerParameterType.Float;

            bool hasBoolMode = usage.ConditionModes.Contains(AnimatorConditionMode.If) ||
                               usage.ConditionModes.Contains(AnimatorConditionMode.IfNot);
            bool hasRangeMode = usage.ConditionModes.Contains(AnimatorConditionMode.Greater) ||
                                usage.ConditionModes.Contains(AnimatorConditionMode.Less);
            bool hasEqualityMode = usage.ConditionModes.Contains(AnimatorConditionMode.Equals) ||
                                   usage.ConditionModes.Contains(AnimatorConditionMode.NotEqual);

            if (hasBoolMode && !hasRangeMode && !hasEqualityMode) return AnimatorControllerParameterType.Bool;
            if (hasEqualityMode && !hasRangeMode) return AnimatorControllerParameterType.Int;
            if (hasRangeMode) return AnimatorControllerParameterType.Float;
            return AnimatorControllerParameterType.Bool;
        }

        public class WriteDefaultsResult
        {
            public bool IsBroken;
            public bool ShouldBeOn;
            public List<string> WeirdStates = new List<string>();
            public string DebugInfo = "";
        }

        private class WdBucket
        {
            public VRCAvatarDescriptor.AnimLayerType Type;
            public List<string> NormalOn = new List<string>();
            public List<string> NormalOff = new List<string>();
            public List<string> DirectOn = new List<string>();
            public List<string> DirectOff = new List<string>();
            public List<string> AdditiveOn = new List<string>();
            public List<string> AdditiveOff = new List<string>();
        }

        public static WriteDefaultsResult DetectWriteDefaults(List<ControllerRef> controllers)
        {
            var buckets = new List<WdBucket>();

            foreach (var cref in controllers)
            {
                var bucket = new WdBucket { Type = cref.Type };
                foreach (var layer in cref.Controller.layers)
                {
                    if (layer.stateMachine == null) continue;
                    bool additive = layer.blendingMode == AnimatorLayerBlendingMode.Additive ||
                                    cref.Type == VRCAvatarDescriptor.AnimLayerType.Additive;

                    foreach (var state in AllStates(layer.stateMachine))
                    {
                        bool hasDirect = AllTrees(state.motion as BlendTree).Any(t => t.blendType == BlendTreeType.Direct);
                        string label = $"{cref.Type} | {layer.name} | {state.name}";
                        List<string> target;
                        if (additive) target = state.writeDefaultValues ? bucket.AdditiveOn : bucket.AdditiveOff;
                        else if (hasDirect) target = state.writeDefaultValues ? bucket.DirectOn : bucket.DirectOff;
                        else target = state.writeDefaultValues ? bucket.NormalOn : bucket.NormalOff;
                        target.Add(label);
                    }
                }
                buckets.Add(bucket);
            }

            var fx = buckets.FirstOrDefault(b => b.Type == VRCAvatarDescriptor.AnimLayerType.FX);
            int totalNormalOn = buckets.Sum(b => b.NormalOn.Count);
            int totalNormalOff = buckets.Sum(b => b.NormalOff.Count);

            bool shouldBeOn;
            if (fx != null && fx.NormalOn.Count + fx.NormalOff.Count > 10)
                shouldBeOn = fx.NormalOn.Count > fx.NormalOff.Count;
            else
                shouldBeOn = totalNormalOn > totalNormalOff;

            var weird = new List<string>();
            weird.AddRange(shouldBeOn ? buckets.SelectMany(b => b.NormalOff) : buckets.SelectMany(b => b.NormalOn));
            weird.AddRange(buckets.SelectMany(b => b.DirectOff));
            weird.AddRange(buckets.SelectMany(b => b.AdditiveOff));

            var debug = new List<string>();
            foreach (var b in buckets)
            {
                var entries = new List<string>();
                if (b.NormalOn.Count > 0) entries.Add($"{b.NormalOn.Count} on");
                if (b.NormalOff.Count > 0) entries.Add($"{b.NormalOff.Count} off");
                if (b.DirectOn.Count + b.DirectOff.Count > 0) entries.Add($"{b.DirectOn.Count + b.DirectOff.Count} direct");
                if (b.AdditiveOn.Count + b.AdditiveOff.Count > 0) entries.Add($"{b.AdditiveOn.Count + b.AdditiveOff.Count} additive");
                if (entries.Count > 0) debug.Add($"{b.Type}: {string.Join(", ", entries)}");
            }

            return new WriteDefaultsResult
            {
                IsBroken = weird.Count > 0,
                ShouldBeOn = shouldBeOn,
                WeirdStates = weird,
                DebugInfo = string.Join(" | ", debug)
            };
        }

        public static void NormalizeWriteDefaults(List<ControllerRef> controllers, bool shouldBeOn)
        {
            foreach (var cref in controllers)
            {
                foreach (var layer in cref.Controller.layers)
                {
                    if (layer.stateMachine == null) continue;
                    bool additive = layer.blendingMode == AnimatorLayerBlendingMode.Additive ||
                                    cref.Type == VRCAvatarDescriptor.AnimLayerType.Additive;
                    bool hasDirect = AllStates(layer.stateMachine)
                        .Any(s => AllTrees(s.motion as BlendTree).Any(t => t.blendType == BlendTreeType.Direct));
                    bool target = shouldBeOn || additive || hasDirect;

                    foreach (var state in AllStates(layer.stateMachine))
                    {
                        if (state.writeDefaultValues != target)
                            state.writeDefaultValues = target;
                    }
                }
                EditorUtility.SetDirty(cref.Controller);
            }
        }

        private static void CheckWriteDefaults(List<ControllerRef> controllers, List<AnimatorFinding> findings)
        {
            if (controllers.Count == 0) return;
            var result = DetectWriteDefaults(controllers);
            if (!result.IsBroken) return;

            string preview = string.Join("\n", result.WeirdStates.Take(12));
            if (result.WeirdStates.Count > 12) preview += $"\n... and {result.WeirdStates.Count - 12} more";

            var captured = controllers;
            bool target = result.ShouldBeOn;

            findings.Add(new AnimatorFinding
            {
                Severity = Severity.Warning,
                Category = "write-defaults",
                Context = controllers[0].Controller,
                IsSafe = false,
                FixLabel = target ? "NORMALIZE -> ON" : "NORMALIZE -> OFF",
                Title = $"Mixed Write Defaults across {result.WeirdStates.Count} state(s)",
                Detail = $"A broken mix of Write Defaults was detected. VRChat behaves best when every state in a layer agrees. " +
                         $"Target (majority): {(target ? "ON" : "OFF")}. Direct blend trees and additive layers are always forced ON.\n" +
                         (target ? "" : "Note: converting to WD OFF only flips the flags here; states that relied on WD to reset props may need default clips added.\n") +
                         $"Counts: {result.DebugInfo}\nOffending states:\n{preview}",
                Fix = () => NormalizeWriteDefaults(captured, target)
            });
        }

        private const string EmptyClipFolder = "Assets/VixenTools/AnimatorForge";
        private const string EmptyClipPath = EmptyClipFolder + "/_Empty.anim";
        private const string LegacyEmptyClipPath = "Assets/VixenForge/_Empty.anim";

        public static AnimationClip GetOrCreateEmptyClip()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(EmptyClipPath);
            if (existing != null) return existing;

            var legacy = AssetDatabase.LoadAssetAtPath<AnimationClip>(LegacyEmptyClipPath);
            if (legacy != null) return legacy;

            if (!AssetDatabase.IsValidFolder("Assets/VixenTools"))
                AssetDatabase.CreateFolder("Assets", "VixenTools");
            if (!AssetDatabase.IsValidFolder(EmptyClipFolder))
                AssetDatabase.CreateFolder("Assets/VixenTools", "AnimatorForge");
            var clip = new AnimationClip { name = "_Empty" };
            clip.SetCurve("", typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0f, 1f / 60f, 1f));
            AssetDatabase.CreateAsset(clip, EmptyClipPath);
            return clip;
        }

        public static bool MotionHasEmptyIssue(Motion motion)
        {
            if (motion == null) return true;
            if (motion is AnimationClip clip) return clip.empty;
            if (motion is BlendTree tree)
            {
                foreach (var child in tree.children)
                    if (MotionHasEmptyIssue(child.motion)) return true;
            }
            return false;
        }

        private static void RepairStateMotion(AnimatorState state, AnimationClip empty)
        {
            if (state.motion == null || (state.motion is AnimationClip c && c.empty))
            {
                state.motion = empty;
                return;
            }
            if (state.motion is BlendTree tree) RepairTreeChildren(tree, empty);
        }

        private static void RepairTreeChildren(BlendTree tree, AnimationClip empty)
        {
            var children = tree.children;
            bool changed = false;
            for (int i = 0; i < children.Length; i++)
            {
                var m = children[i].motion;
                if (m == null || (m is AnimationClip c && c.empty))
                {
                    children[i].motion = empty;
                    changed = true;
                }
                else if (m is BlendTree nested)
                {
                    RepairTreeChildren(nested, empty);
                }
            }
            if (changed) tree.children = children;
        }

        private static void CheckEmptyWriteDefaultsOffClips(List<ControllerRef> controllers, List<AnimatorFinding> findings)
        {
            foreach (var cref in controllers)
            {
                var controller = cref.Controller;
                foreach (var layer in controller.layers)
                {
                    if (layer.stateMachine == null) continue;
                    foreach (var state in AllStates(layer.stateMachine))
                    {
                        if (state.writeDefaultValues) continue;
                        if (!MotionHasEmptyIssue(state.motion)) continue;

                        var capturedState = state;
                        var capturedController = controller;
                        string kind = state.motion == null
                            ? "no clip assigned"
                            : (state.motion is BlendTree ? "a blend tree with a missing/empty child clip" : "an empty clip");

                        findings.Add(new AnimatorFinding
                        {
                            Severity = Severity.Warning,
                            Category = "empty-clip",
                            Context = state,
                            IsSafe = true,
                            FixLabel = "ASSIGN _Empty",
                            Title = $"WD-off state '{state.name}' in '{layer.name}' has {kind}",
                            Detail = $"State '{state.name}' (layer '{layer.name}', controller '{controller.name}') has Write Defaults OFF with a missing or empty animation clip. VRChat flags this and the animation can behave unpredictably. Fix assigns a shared inert '_Empty' clip (one harmless property, so the SDK's empty-clip check passes). An empty clip with zero curves would NOT clear the warning.",
                            Fix = () =>
                            {
                                var empty = GetOrCreateEmptyClip();
                                RepairStateMotion(capturedState, empty);
                                EditorUtility.SetDirty(capturedController);
                            }
                        });
                    }
                }
            }
        }

        private static void CheckExpressionSync(VRCAvatarDescriptor descriptor, List<ControllerRef> controllers, List<AnimatorFinding> findings)
        {
            var expr = descriptor.expressionParameters;
            if (expr == null || expr.parameters == null) return;

            var duplicates = expr.parameters
                .Where(p => !string.IsNullOrEmpty(p.name))
                .GroupBy(p => p.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                findings.Add(new AnimatorFinding
                {
                    Severity = Severity.Warning,
                    Category = "expression-sync",
                    Context = expr,
                    IsSafe = true,
                    FixLabel = "DEDUPE",
                    Title = $"Duplicate expression parameter(s): {string.Join(", ", duplicates)}",
                    Detail = "The same parameter name appears more than once in the Expression Parameters. Only the first is used; the rest waste sync bits.",
                    Fix = () =>
                    {
                        var seen = new HashSet<string>();
                        var kept = new List<VRCExpressionParameters.Parameter>();
                        foreach (var p in expr.parameters)
                        {
                            if (!string.IsNullOrEmpty(p.name) && !seen.Add(p.name)) continue;
                            kept.Add(p);
                        }
                        expr.parameters = kept.ToArray();
                        EditorUtility.SetDirty(expr);
                    }
                });
            }

            int cost = expr.CalcTotalCost();
            if (cost > VRCExpressionParameters.MAX_PARAMETER_COST)
            {
                findings.Add(new AnimatorFinding
                {
                    Severity = Severity.Error,
                    Category = "expression-sync",
                    Context = expr,
                    Title = $"Synced parameter cost {cost}/{VRCExpressionParameters.MAX_PARAMETER_COST} bits exceeded",
                    Detail = "Too many synced parameters. Un-sync (uncheck Synced) or convert Int/Float to Bool where possible. Bool costs 1 bit, Int/Float cost 8."
                });
            }

            var declaredByName = new Dictionary<string, AnimatorControllerParameterType>();
            foreach (var cref in controllers)
                foreach (var p in cref.Controller.parameters)
                    if (!declaredByName.ContainsKey(p.name)) declaredByName[p.name] = p.type;

            foreach (var p in expr.parameters)
            {
                if (string.IsNullOrEmpty(p.name)) continue;
                if (!declaredByName.TryGetValue(p.name, out var animType)) continue;
                var exprAsAnim = ToAnimatorType(p.valueType);
                if (exprAsAnim != animType && animType != AnimatorControllerParameterType.Trigger)
                {
                    findings.Add(new AnimatorFinding
                    {
                        Severity = Severity.Warning,
                        Category = "expression-sync",
                        Context = expr,
                        Title = $"Type mismatch on '{p.name}'",
                        Detail = $"Expression Parameters declares '{p.name}' as {p.valueType}, but the animator declares it as {animType}. These must match or toggles will misbehave."
                    });
                }
            }
        }

        private static void CheckMenu(VRCAvatarDescriptor descriptor, List<AnimatorFinding> findings)
        {
            var menu = descriptor.expressionsMenu;
            var expr = descriptor.expressionParameters;
            if (menu == null) return;

            var declared = new HashSet<string>();
            if (expr != null && expr.parameters != null)
                foreach (var p in expr.parameters)
                    if (!string.IsNullOrEmpty(p.name)) declared.Add(p.name);

            var visited = new HashSet<VRCExpressionsMenu>();
            void Walk(VRCExpressionsMenu m)
            {
                if (m == null || !visited.Add(m)) return;
                if (m.controls == null) return;

                if (m.controls.Count > VRCExpressionsMenu.MAX_CONTROLS)
                {
                    findings.Add(new AnimatorFinding
                    {
                        Severity = Severity.Warning,
                        Category = "menu",
                        Context = m,
                        Title = $"Menu '{m.name}' has {m.controls.Count} controls (max {VRCExpressionsMenu.MAX_CONTROLS})",
                        Detail = "VRChat only shows the first 8 controls per menu page. Split extras into a submenu."
                    });
                }

                foreach (var control in m.controls)
                {
                    if (control.type == VRCExpressionsMenu.Control.ControlType.SubMenu)
                    {
                        if (control.subMenu == null)
                            findings.Add(new AnimatorFinding
                            {
                                Severity = Severity.Warning,
                                Category = "menu",
                                Context = m,
                                Title = $"Submenu control '{control.name}' has no target menu",
                                Detail = "This SubMenu control points at nothing and will open an empty page."
                            });
                        Walk(control.subMenu);
                        continue;
                    }

                    string paramName = control.parameter != null ? control.parameter.name : null;
                    if (string.IsNullOrEmpty(paramName))
                    {
                        findings.Add(new AnimatorFinding
                        {
                            Severity = Severity.Warning,
                            Category = "menu",
                            Context = m,
                            Title = $"Control '{control.name}' has no parameter",
                            Detail = $"A {control.type} control with an empty parameter does nothing."
                        });
                        continue;
                    }

                    if (expr != null && !declared.Contains(paramName))
                    {
                        var capturedExpr = expr;
                        var capturedName = paramName;
                        var valueType = ControlToValueType(control.type);
                        findings.Add(new AnimatorFinding
                        {
                            Severity = Severity.Error,
                            Category = "menu",
                            Context = expr,
                            IsSafe = true,
                            FixLabel = "ADD PARAM",
                            Title = $"Menu control '{control.name}' uses undeclared parameter '{paramName}'",
                            Detail = $"'{paramName}' is driven by a menu control but missing from Expression Parameters. Add it as {valueType} (synced, saved).",
                            Fix = () =>
                            {
                                if (capturedExpr.parameters.Any(p => p.name == capturedName)) return;
                                var list = capturedExpr.parameters.ToList();
                                list.Add(new VRCExpressionParameters.Parameter
                                {
                                    name = capturedName,
                                    valueType = valueType,
                                    defaultValue = 0f,
                                    saved = true,
                                    networkSynced = true
                                });
                                capturedExpr.parameters = list.ToArray();
                                EditorUtility.SetDirty(capturedExpr);
                            }
                        });
                        declared.Add(paramName);
                    }
                }
            }

            Walk(menu);
        }

        private static void CheckLayers(List<ControllerRef> controllers, List<AnimatorFinding> findings)
        {
            foreach (var cref in controllers)
            {
                var controller = cref.Controller;
                var layers = controller.layers;

                var nameCounts = layers.GroupBy(l => l.name).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                foreach (var dup in nameCounts)
                    findings.Add(new AnimatorFinding
                    {
                        Severity = Severity.Info,
                        Category = "layer",
                        Context = controller,
                        Title = $"'{controller.name}' has duplicate layer name '{dup}'",
                        Detail = "Multiple layers share a name. This is legal but makes debugging and parameter drivers ambiguous."
                    });

                for (int i = 0; i < layers.Length; i++)
                {
                    var layer = layers[i];
                    var sm = layer.stateMachine;
                    if (sm == null) continue;

                    var states = AllStates(sm).ToList();
                    if (states.Count == 0)
                    {
                        findings.Add(new AnimatorFinding
                        {
                            Severity = Severity.Info,
                            Category = "layer",
                            Context = controller,
                            Title = $"Empty layer '{layer.name}' in '{controller.name}'",
                            Detail = "This layer has no states and does nothing."
                        });
                        continue;
                    }

                    if (sm.defaultState == null)
                        findings.Add(new AnimatorFinding
                        {
                            Severity = Severity.Warning,
                            Category = "layer",
                            Context = controller,
                            Title = $"Layer '{layer.name}' has no default state",
                            Detail = "A state machine with states but no default (orange) state has undefined entry behavior."
                        });

                    if (i > 0 && Mathf.Approximately(layer.defaultWeight, 0f))
                    {
                        int capturedIndex = i;
                        findings.Add(new AnimatorFinding
                        {
                            Severity = Severity.Info,
                            Category = "layer",
                            Context = controller,
                            FixLabel = "WEIGHT = 1",
                            Title = $"Layer '{layer.name}' has weight 0",
                            Detail = "This layer will not affect the avatar unless something raises its weight. If it is a toggle/animation layer, its weight should be 1.",
                            Fix = () =>
                            {
                                var arr = controller.layers;
                                if (capturedIndex < arr.Length)
                                {
                                    arr[capturedIndex].defaultWeight = 1f;
                                    controller.layers = arr;
                                    EditorUtility.SetDirty(controller);
                                }
                            }
                        });
                    }
                }
            }
        }

        private static void CheckTransitions(List<ControllerRef> controllers, List<AnimatorFinding> findings)
        {
            foreach (var cref in controllers)
            {
                var controller = cref.Controller;
                var declaredTypes = new Dictionary<string, AnimatorControllerParameterType>();
                foreach (var p in controller.parameters)
                    if (!string.IsNullOrEmpty(p.name)) declaredTypes[p.name] = p.type;
                int instantCount = 0;

                foreach (var layer in controller.layers)
                {
                    if (layer.stateMachine == null) continue;
                    foreach (var state in AllStates(layer.stateMachine))
                    {
                        foreach (var t in state.transitions)
                        {
                            if (t.conditions.Length == 0 && !t.hasExitTime) instantCount++;

                            foreach (var c in t.conditions)
                            {
                                if (!declaredTypes.TryGetValue(c.parameter, out var type)) continue;
                                if (!ModeMatchesType(c.mode, type))
                                    findings.Add(new AnimatorFinding
                                    {
                                        Severity = Severity.Warning,
                                        Category = "transition",
                                        Context = state,
                                        Title = $"Condition on '{c.parameter}' uses {c.mode} which is invalid for a {type}",
                                        Detail = $"State '{state.name}' in layer '{layer.name}' has a transition whose condition mode does not match the parameter type."
                                    });
                            }
                        }
                    }
                }

                if (instantCount > 0)
                    findings.Add(new AnimatorFinding
                    {
                        Severity = Severity.Warning,
                        Category = "transition",
                        Context = controller,
                        Title = $"'{controller.name}' has {instantCount} instant transition(s)",
                        Detail = "One or more state transitions have no conditions and no exit time, so they fire immediately. This is usually a mistake unless intentional pass-through."
                    });
            }
        }

        private static bool ModeMatchesType(AnimatorConditionMode mode, AnimatorControllerParameterType type)
        {
            switch (type)
            {
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return mode == AnimatorConditionMode.If || mode == AnimatorConditionMode.IfNot;
                case AnimatorControllerParameterType.Int:
                    return mode == AnimatorConditionMode.Greater || mode == AnimatorConditionMode.Less ||
                           mode == AnimatorConditionMode.Equals || mode == AnimatorConditionMode.NotEqual;
                case AnimatorControllerParameterType.Float:
                    return mode == AnimatorConditionMode.Greater || mode == AnimatorConditionMode.Less;
            }
            return true;
        }

        public static AnimatorControllerParameterType ToAnimatorType(VRCExpressionParameters.ValueType valueType)
        {
            switch (valueType)
            {
                case VRCExpressionParameters.ValueType.Int: return AnimatorControllerParameterType.Int;
                case VRCExpressionParameters.ValueType.Float: return AnimatorControllerParameterType.Float;
                default: return AnimatorControllerParameterType.Bool;
            }
        }

        private static VRCExpressionParameters.ValueType ControlToValueType(VRCExpressionsMenu.Control.ControlType type)
        {
            switch (type)
            {
                case VRCExpressionsMenu.Control.ControlType.RadialPuppet:
                case VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet:
                case VRCExpressionsMenu.Control.ControlType.FourAxisPuppet:
                    return VRCExpressionParameters.ValueType.Float;
                default:
                    return VRCExpressionParameters.ValueType.Bool;
            }
        }

        public static IEnumerable<AnimatorState> AllStates(AnimatorStateMachine sm)
        {
            if (sm == null) yield break;
            foreach (var cs in sm.states)
                if (cs.state != null) yield return cs.state;
            foreach (var child in sm.stateMachines)
                foreach (var s in AllStates(child.stateMachine))
                    yield return s;
        }

        public static IEnumerable<AnimatorTransitionBase> AllTransitions(AnimatorStateMachine sm)
        {
            if (sm == null) yield break;
            foreach (var t in sm.anyStateTransitions) yield return t;
            foreach (var t in sm.entryTransitions) yield return t;
            foreach (var cs in sm.states)
            {
                if (cs.state == null) continue;
                foreach (var t in cs.state.transitions) yield return t;
            }
            foreach (var child in sm.stateMachines)
            {
                foreach (var t in sm.GetStateMachineTransitions(child.stateMachine)) yield return t;
                foreach (var t in AllTransitions(child.stateMachine)) yield return t;
            }
        }

        public static IEnumerable<BlendTree> AllTrees(BlendTree tree)
        {
            if (tree == null) yield break;
            yield return tree;
            foreach (var child in tree.children)
                foreach (var t in AllTrees(child.motion as BlendTree))
                    yield return t;
        }
    }
}
#endif
