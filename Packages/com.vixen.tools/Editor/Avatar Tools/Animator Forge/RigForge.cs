#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VixenTools.Editor
{
    public static class RigForge
    {
        public enum RigType { GameObjectToggle, BlendshapeToggle, BlendshapeSlider, MaterialSwap, ExclusiveGroup }

        public class RigRequest
        {
            public RigType Type = RigType.GameObjectToggle;
            public string RigName = "New Toggle";
            public string ParameterName = "";
            public bool Saved = true;
            public bool Synced = true;
            public bool StartOn = false;

            public List<GameObject> Targets = new List<GameObject>();

            public SkinnedMeshRenderer BlendRenderer;
            public string BlendShape = "";
            public float BlendOnValue = 100f;

            public Renderer SwapRenderer;
            public int MaterialSlot = 0;
            public Material SwapMaterial;

            public List<GameObject> Options = new List<GameObject>();
        }

        public class RigResult
        {
            public bool Success;
            public List<string> Log = new List<string>();
            public void Info(string m) => Log.Add(m);
            public void Fail(string m) { Success = false; Log.Add("ERROR: " + m); }
        }

        public static RigResult Build(VRCAvatarDescriptor descriptor, RigRequest req)
        {
            var result = new RigResult { Success = true };
            if (descriptor == null) { result.Fail("No avatar descriptor."); return result; }
            if (string.IsNullOrWhiteSpace(req.RigName)) { result.Fail("Rig name is empty."); return result; }

            string param = string.IsNullOrWhiteSpace(req.ParameterName) ? req.RigName : req.ParameterName;
            param = param.Trim();

            var controller = EnsureFxController(descriptor, result);
            if (controller == null) { result.Success = false; return result; }

            EnsureExpressionAssets(descriptor, out var expr, out var menu, result);
            if (expr == null || menu == null) { result.Success = false; return result; }

            var allRefs = AnimatorDoctor.CollectControllers(descriptor);
            bool wdTarget = AnimatorDoctor.DetectWriteDefaults(allRefs).ShouldBeOn;
            int totalStates = allRefs.Sum(r => r.Controller.layers
                .Sum(l => l.stateMachine != null ? AnimatorDoctor.AllStates(l.stateMachine).Count() : 0));
            if (totalStates == 0) wdTarget = true;
            result.Info($"Write Defaults for new states: {(wdTarget ? "ON" : "OFF")} " +
                        (totalStates == 0 ? "(fresh controller, defaulting ON)." : "(matched to avatar)."));

            string folder = EnsureAssetFolder(descriptor);
            var root = descriptor.transform;

            try
            {
                switch (req.Type)
                {
                    case RigType.GameObjectToggle: BuildGameObjectToggle(descriptor, controller, expr, menu, req, param, wdTarget, folder, root, result); break;
                    case RigType.BlendshapeToggle: BuildBlendshapeToggle(controller, expr, menu, req, param, wdTarget, folder, root, result); break;
                    case RigType.BlendshapeSlider: BuildBlendshapeSlider(controller, expr, menu, req, param, wdTarget, folder, root, result); break;
                    case RigType.MaterialSwap: BuildMaterialSwap(controller, expr, menu, req, param, wdTarget, folder, root, result); break;
                    case RigType.ExclusiveGroup: BuildExclusiveGroup(controller, expr, menu, req, param, wdTarget, folder, root, result); break;
                }
            }
            catch (Exception e)
            {
                result.Fail(e.Message);
                Debug.LogException(e);
            }

            if (result.Success)
            {
                EditorUtility.SetDirty(controller);
                EditorUtility.SetDirty(expr);
                EditorUtility.SetDirty(menu);
                EditorUtility.SetDirty(descriptor);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                result.Info("Saved. Rig is fully wired: parameter + menu control + FX layer + clips.");
            }

            return result;
        }

        private static void BuildGameObjectToggle(VRCAvatarDescriptor descriptor, AnimatorController controller,
            VRCExpressionParameters expr, VRCExpressionsMenu menu, RigRequest req, string param, bool wd,
            string folder, Transform root, RigResult result)
        {
            if (req.Targets == null || req.Targets.Count == 0) { result.Fail("No target GameObjects selected."); return; }

            var offClip = NewClip(folder, req.RigName + "_Off");
            var onClip = NewClip(folder, req.RigName + "_On");
            foreach (var go in req.Targets)
            {
                if (go == null) continue;
                string path = PathTo(go.transform, root);
                if (path == null) { result.Fail($"'{go.name}' is not under the avatar."); return; }
                SetActive(offClip, path, false);
                SetActive(onClip, path, true);
            }

            AddBoolParam(controller, param, req.StartOn);
            AddExpressionParam(expr, param, VRCExpressionParameters.ValueType.Bool, req.StartOn ? 1f : 0f, req.Saved, req.Synced, result);
            BuildToggleLayer(controller, "Toggle " + req.RigName, param, offClip, onClip, req.StartOn, wd);
            AddMenuControl(menu, new VRCExpressionsMenu.Control
            {
                name = req.RigName,
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = param },
                value = 1f
            }, result);
            result.Info($"GameObject toggle '{req.RigName}' over {req.Targets.Count(t => t != null)} object(s).");
        }

        private static void BuildBlendshapeToggle(AnimatorController controller, VRCExpressionParameters expr,
            VRCExpressionsMenu menu, RigRequest req, string param, bool wd, string folder, Transform root, RigResult result)
        {
            if (req.BlendRenderer == null || string.IsNullOrEmpty(req.BlendShape)) { result.Fail("Blendshape renderer or shape not set."); return; }
            string path = PathTo(req.BlendRenderer.transform, root);
            if (path == null) { result.Fail("Blendshape renderer is not under the avatar."); return; }

            var offClip = NewClip(folder, req.RigName + "_Off");
            var onClip = NewClip(folder, req.RigName + "_On");
            SetBlendshape(offClip, path, req.BlendShape, 0f);
            SetBlendshape(onClip, path, req.BlendShape, req.BlendOnValue);

            AddBoolParam(controller, param, req.StartOn);
            AddExpressionParam(expr, param, VRCExpressionParameters.ValueType.Bool, req.StartOn ? 1f : 0f, req.Saved, req.Synced, result);
            BuildToggleLayer(controller, "Toggle " + req.RigName, param, offClip, onClip, req.StartOn, wd);
            AddMenuControl(menu, new VRCExpressionsMenu.Control
            {
                name = req.RigName,
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = param },
                value = 1f
            }, result);
            result.Info($"Blendshape toggle '{req.RigName}' on '{req.BlendShape}'.");
        }

        private static void BuildBlendshapeSlider(AnimatorController controller, VRCExpressionParameters expr,
            VRCExpressionsMenu menu, RigRequest req, string param, bool wd, string folder, Transform root, RigResult result)
        {
            if (req.BlendRenderer == null || string.IsNullOrEmpty(req.BlendShape)) { result.Fail("Blendshape renderer or shape not set."); return; }
            string path = PathTo(req.BlendRenderer.transform, root);
            if (path == null) { result.Fail("Blendshape renderer is not under the avatar."); return; }

            var minClip = NewClip(folder, req.RigName + "_0");
            var maxClip = NewClip(folder, req.RigName + "_100");
            SetBlendshape(minClip, path, req.BlendShape, 0f);
            SetBlendshape(maxClip, path, req.BlendShape, req.BlendOnValue);

            AddFloatParam(controller, param, req.StartOn ? 1f : 0f);
            AddExpressionParam(expr, param, VRCExpressionParameters.ValueType.Float, req.StartOn ? 1f : 0f, req.Saved, req.Synced, result);

            var sm = NewLayer(controller, "Slider " + req.RigName);
            var tree = new BlendTree
            {
                name = req.RigName,
                blendType = BlendTreeType.Simple1D,
                blendParameter = param,
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(minClip, 0f);
            tree.AddChild(maxClip, 1f);
            var state = sm.AddState(req.RigName);
            state.writeDefaultValues = wd;
            state.motion = tree;
            sm.defaultState = state;

            AddMenuControl(menu, new VRCExpressionsMenu.Control
            {
                name = req.RigName,
                type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                subParameters = new[] { new VRCExpressionsMenu.Control.Parameter { name = param } }
            }, result);
            result.Info($"Blendshape slider '{req.RigName}' (radial puppet 0..{req.BlendOnValue}).");
        }

        private static void BuildMaterialSwap(AnimatorController controller, VRCExpressionParameters expr,
            VRCExpressionsMenu menu, RigRequest req, string param, bool wd, string folder, Transform root, RigResult result)
        {
            if (req.SwapRenderer == null || req.SwapMaterial == null) { result.Fail("Material swap renderer or material not set."); return; }
            string path = PathTo(req.SwapRenderer.transform, root);
            if (path == null) { result.Fail("Renderer is not under the avatar."); return; }

            var mats = req.SwapRenderer.sharedMaterials;
            if (req.MaterialSlot < 0 || req.MaterialSlot >= mats.Length) { result.Fail($"Material slot {req.MaterialSlot} out of range."); return; }
            var original = mats[req.MaterialSlot];

            var offClip = NewClip(folder, req.RigName + "_Off");
            var onClip = NewClip(folder, req.RigName + "_On");
            SetMaterial(offClip, path, req.SwapRenderer.GetType(), req.MaterialSlot, original);
            SetMaterial(onClip, path, req.SwapRenderer.GetType(), req.MaterialSlot, req.SwapMaterial);

            AddBoolParam(controller, param, req.StartOn);
            AddExpressionParam(expr, param, VRCExpressionParameters.ValueType.Bool, req.StartOn ? 1f : 0f, req.Saved, req.Synced, result);
            BuildToggleLayer(controller, "Swap " + req.RigName, param, offClip, onClip, req.StartOn, wd);
            AddMenuControl(menu, new VRCExpressionsMenu.Control
            {
                name = req.RigName,
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = param },
                value = 1f
            }, result);
            result.Info($"Material swap '{req.RigName}' on slot {req.MaterialSlot}.");
        }

        private static void BuildExclusiveGroup(AnimatorController controller, VRCExpressionParameters expr,
            VRCExpressionsMenu menu, RigRequest req, string param, bool wd, string folder, Transform root, RigResult result)
        {
            var options = req.Options.Where(o => o != null).ToList();
            if (options.Count < 2) { result.Fail("Exclusive group needs at least 2 option objects."); return; }

            AddIntParam(controller, param, 0);
            AddExpressionParam(expr, param, VRCExpressionParameters.ValueType.Int, 0f, req.Saved, req.Synced, result);

            var paths = options.Select(o => PathTo(o.transform, root)).ToList();
            if (paths.Any(p => p == null)) { result.Fail("One or more option objects are not under the avatar."); return; }

            var sm = NewLayer(controller, "Exclusive " + req.RigName);

            AnimationClip MakeStateClip(int activeIndex)
            {
                string clipName = activeIndex < 0 ? req.RigName + "_Off" : req.RigName + "_" + options[activeIndex].name;
                var clip = NewClip(folder, clipName);
                for (int j = 0; j < options.Count; j++)
                    SetActive(clip, paths[j], j == activeIndex);
                return clip;
            }

            var offState = sm.AddState(req.RigName + " Off");
            offState.writeDefaultValues = wd;
            offState.motion = MakeStateClip(-1);
            sm.defaultState = offState;

            var offAny = sm.AddAnyStateTransition(offState);
            ConfigureInstant(offAny);
            offAny.AddCondition(AnimatorConditionMode.Equals, 0, param);

            var subMenu = CreateMenuAsset(folder, req.RigName + " Group");
            for (int i = 0; i < options.Count; i++)
            {
                var st = sm.AddState(options[i].name);
                st.writeDefaultValues = wd;
                st.motion = MakeStateClip(i);
                var tr = sm.AddAnyStateTransition(st);
                ConfigureInstant(tr);
                tr.AddCondition(AnimatorConditionMode.Equals, i + 1, param);

                subMenu.controls.Add(new VRCExpressionsMenu.Control
                {
                    name = options[i].name,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter { name = param },
                    value = i + 1
                });
            }
            EditorUtility.SetDirty(subMenu);

            AddMenuControl(menu, new VRCExpressionsMenu.Control
            {
                name = req.RigName,
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = "" },
                subMenu = subMenu
            }, result);
            result.Info($"Exclusive group '{req.RigName}' with {options.Count} mutually-exclusive options.");
        }

        private static AnimatorStateMachine NewLayer(AnimatorController controller, string name)
        {
            controller.AddLayer(name);
            var layers = controller.layers;
            var layer = layers[layers.Length - 1];
            layer.defaultWeight = 1f;
            controller.layers = layers;
            return controller.layers[controller.layers.Length - 1].stateMachine;
        }

        private static void BuildToggleLayer(AnimatorController controller, string layerName, string param,
            AnimationClip offClip, AnimationClip onClip, bool startOn, bool wd)
        {
            var sm = NewLayer(controller, layerName);

            var offState = sm.AddState("Off", new Vector3(300, 120, 0));
            offState.writeDefaultValues = wd;
            offState.motion = offClip;

            var onState = sm.AddState("On", new Vector3(300, 220, 0));
            onState.writeDefaultValues = wd;
            onState.motion = onClip;

            sm.defaultState = startOn ? onState : offState;

            var toOn = offState.AddTransition(onState);
            ConfigureInstant(toOn);
            toOn.AddCondition(AnimatorConditionMode.If, 0, param);

            var toOff = onState.AddTransition(offState);
            ConfigureInstant(toOff);
            toOff.AddCondition(AnimatorConditionMode.IfNot, 0, param);
        }

        private static void ConfigureInstant(AnimatorStateTransition t)
        {
            t.hasExitTime = false;
            t.hasFixedDuration = true;
            t.duration = 0f;
            t.exitTime = 0f;
            t.canTransitionToSelf = false;
        }

        private static void AddBoolParam(AnimatorController controller, string name, bool defaultValue)
        {
            if (controller.parameters.Any(p => p.name == name)) return;
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = name,
                type = AnimatorControllerParameterType.Bool,
                defaultBool = defaultValue
            });
        }

        private static void AddIntParam(AnimatorController controller, string name, int defaultValue)
        {
            if (controller.parameters.Any(p => p.name == name)) return;
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = name,
                type = AnimatorControllerParameterType.Int,
                defaultInt = defaultValue
            });
        }

        private static void AddFloatParam(AnimatorController controller, string name, float defaultValue)
        {
            if (controller.parameters.Any(p => p.name == name)) return;
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = name,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = defaultValue
            });
        }

        private static void AddExpressionParam(VRCExpressionParameters expr, string name,
            VRCExpressionParameters.ValueType type, float defaultValue, bool saved, bool synced, RigResult result)
        {
            if (expr.parameters != null && expr.parameters.Any(p => p.name == name))
            {
                result.Info($"Expression parameter '{name}' already existed, reused.");
                return;
            }
            var list = expr.parameters != null ? expr.parameters.ToList() : new List<VRCExpressionParameters.Parameter>();
            list.Add(new VRCExpressionParameters.Parameter
            {
                name = name,
                valueType = type,
                defaultValue = defaultValue,
                saved = saved,
                networkSynced = synced
            });
            expr.parameters = list.ToArray();
        }

        private static void AddMenuControl(VRCExpressionsMenu menu, VRCExpressionsMenu.Control control, RigResult result)
        {
            if (menu.controls == null) menu.controls = new List<VRCExpressionsMenu.Control>();
            if (menu.controls.Count >= VRCExpressionsMenu.MAX_CONTROLS)
                result.Info($"Warning: root menu already has {menu.controls.Count} controls (max {VRCExpressionsMenu.MAX_CONTROLS}); the new control may not be visible. Consider a submenu.");
            menu.controls.Add(control);
        }

        private static AnimationClip NewClip(string folder, string name)
        {
            var clip = new AnimationClip { name = name };
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{Sanitize(name)}.anim");
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void SetActive(AnimationClip clip, string path, bool active)
        {
            clip.SetCurve(path, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0f, 1f / 60f, active ? 1f : 0f));
        }

        private static void SetBlendshape(AnimationClip clip, string path, string shape, float value)
        {
            clip.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + shape, AnimationCurve.Constant(0f, 1f / 60f, value));
        }

        private static void SetMaterial(AnimationClip clip, string path, Type rendererType, int slot, Material mat)
        {
            var binding = EditorCurveBinding.PPtrCurve(path, rendererType, $"m_Materials.Array.data[{slot}]");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, new[]
            {
                new ObjectReferenceKeyframe { time = 0f, value = mat }
            });
        }

        private static string PathTo(Transform target, Transform root)
        {
            if (target == root) return "";
            if (!target.IsChildOf(root)) return null;
            return AnimationUtility.CalculateTransformPath(target, root);
        }

        private static AnimatorController EnsureFxController(VRCAvatarDescriptor descriptor, RigResult result)
        {
            var layers = descriptor.baseAnimationLayers;
            int idx = layers != null ? Array.FindIndex(layers, l => l.type == VRCAvatarDescriptor.AnimLayerType.FX) : -1;

            if (idx < 0)
            {
                result.Fail("Avatar has no FX playable layer slot. Open the descriptor and enable custom playable layers first.");
                return null;
            }

            var fx = layers[idx];
            var existing = fx.animatorController as AnimatorController;

            if (existing != null && !fx.isDefault)
            {
                string assetPath = AssetDatabase.GetAssetPath(existing);
                if (!string.IsNullOrEmpty(assetPath) && (assetPath.StartsWith("Packages/") || assetPath.Contains("VRCSDK/Sample")))
                {
                    string folder = EnsureAssetFolder(descriptor);
                    string clonePath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{Sanitize(descriptor.name)}_FX.controller");
                    if (AssetDatabase.CopyAsset(assetPath, clonePath))
                    {
                        var clone = AssetDatabase.LoadAssetAtPath<AnimatorController>(clonePath);
                        layers[idx].animatorController = clone;
                        layers[idx].isDefault = false;
                        descriptor.baseAnimationLayers = layers;
                        descriptor.customizeAnimationLayers = true;
                        EditorUtility.SetDirty(descriptor);
                        result.Info($"FX controller was a read-only sample; cloned to '{clonePath}'.");
                        return clone;
                    }
                }
                return existing;
            }

            string newFolder = EnsureAssetFolder(descriptor);
            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{newFolder}/{Sanitize(descriptor.name)}_FX.controller");
            var created = AnimatorController.CreateAnimatorControllerAtPath(newPath);
            layers[idx].animatorController = created;
            layers[idx].isDefault = false;
            layers[idx].isEnabled = true;
            descriptor.baseAnimationLayers = layers;
            descriptor.customizeAnimationLayers = true;
            EditorUtility.SetDirty(descriptor);
            result.Info($"No custom FX controller; created '{newPath}' and assigned it.");
            return created;
        }

        private static void EnsureExpressionAssets(VRCAvatarDescriptor descriptor, out VRCExpressionParameters expr,
            out VRCExpressionsMenu menu, RigResult result)
        {
            string folder = EnsureAssetFolder(descriptor);
            descriptor.customExpressions = true;

            expr = descriptor.expressionParameters;
            if (expr == null)
            {
                expr = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                expr.parameters = new[]
                {
                    new VRCExpressionParameters.Parameter { name = "VRCEmote", valueType = VRCExpressionParameters.ValueType.Int, saved = false, networkSynced = true },
                    new VRCExpressionParameters.Parameter { name = "VRCFaceBlendH", valueType = VRCExpressionParameters.ValueType.Float, saved = false, networkSynced = true },
                    new VRCExpressionParameters.Parameter { name = "VRCFaceBlendV", valueType = VRCExpressionParameters.ValueType.Float, saved = false, networkSynced = true },
                };
                string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{Sanitize(descriptor.name)}_Parameters.asset");
                AssetDatabase.CreateAsset(expr, path);
                descriptor.expressionParameters = expr;
                EditorUtility.SetDirty(descriptor);
                result.Info($"Created Expression Parameters at '{path}'.");
            }

            menu = descriptor.expressionsMenu;
            if (menu == null)
            {
                menu = CreateMenuAsset(folder, Sanitize(descriptor.name) + "_Menu");
                descriptor.expressionsMenu = menu;
                EditorUtility.SetDirty(descriptor);
                result.Info($"Created Expressions Menu.");
            }
        }

        private static VRCExpressionsMenu CreateMenuAsset(string folder, string name)
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            menu.controls = new List<VRCExpressionsMenu.Control>();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{Sanitize(name)}.asset");
            AssetDatabase.CreateAsset(menu, path);
            return menu;
        }

        private static string EnsureAssetFolder(VRCAvatarDescriptor descriptor)
        {
            if (!AssetDatabase.IsValidFolder("Assets/VixenForge"))
                AssetDatabase.CreateFolder("Assets", "VixenForge");
            string sub = Sanitize(descriptor.name);
            string full = "Assets/VixenForge/" + sub;
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder("Assets/VixenForge", sub);
            return full;
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "Unnamed";
            return Regex.Replace(s, "[^a-zA-Z0-9_ \\-]", "_").Trim();
        }
    }
}
#endif
