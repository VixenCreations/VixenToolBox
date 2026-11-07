using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using nadena.dev.ndmf.localization;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using Object = UnityEngine.Object;

[assembly: ExportsPlugin(typeof(VixenTools.MMD.Editor.VixenMMDPlugin))]

namespace VixenTools.MMD.Editor
{
    public sealed class VixenMMDPlugin : Plugin<VixenMMDPlugin>
    {
        public override string QualifiedName => "com.vixencreations.vixenmmd";
        public override string DisplayName => "VixenMMD";

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar");
            seq.WithRequiredExtension(typeof(AnimatorServicesContext), s =>
            {
                s.Run("VixenMMD: prepare the face mesh", VixenMMDPasses.PrepareFace);
                s.Run("VixenMMD: build MMD blend shapes", VixenMMDPasses.BuildMorphs);
                s.Run("VixenMMD: FX layers", VixenMMDPasses.FxLayers);
            });
            seq.Run("VixenMMD: keep MMD shapes safe from VRCFury", VixenMMDPasses.KeepShapesForVrcFury);

            InPhase(BuildPhase.Optimizing).BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run("VixenMMD: remove the VRCFury helper", VixenMMDPasses.RemoveKeepHelper);
        }
    }

    internal static class VixenMMDReport
    {
        private static Localizer Localizer => VixenMMDText.Localizer;

        internal static void Info(string key, params object[] args) => ErrorReport.ReportError(Localizer, ErrorSeverity.Information, key, args);
        internal static void Warn(string key, params object[] args) => ErrorReport.ReportError(Localizer, ErrorSeverity.NonFatal, key, args);
        internal static void Error(string key, params object[] args) => ErrorReport.ReportError(Localizer, ErrorSeverity.Error, key, args);
    }

    internal static class VixenMMDPasses
    {
        internal const string GuardParameter = "VixenMMD/MMDWorld";

        private static VixenMMD Find(BuildContext ctx)
        {
            var all = ctx.AvatarRootObject.GetComponentsInChildren<VixenMMD>(true);
            if (all.Length == 0) return null;
            return all[0];
        }

        internal static SkinnedMeshRenderer FaceOf(VixenMMD mmd, VRCAvatarDescriptor descriptor)
        {
            if (mmd != null && mmd.faceMesh != null) return mmd.faceMesh;
            return descriptor != null ? descriptor.VisemeSkinnedMesh : null;
        }

        internal static void PrepareFace(BuildContext ctx)
        {
            var all = ctx.AvatarRootObject.GetComponentsInChildren<VixenMMD>(true);
            if (all.Length == 0) return;
            var mmd = all[0];
            if (all.Length > 1) VixenMMDReport.Warn("vixenmmd.several", all.Length.ToString(), mmd.gameObject.name, mmd);
            if (VixenMMDShare.FindMakeItMmd(ctx.AvatarRootObject).Count > 0) VixenMMDReport.Warn("vixenmmd.mim", mmd);

            var descriptor = ctx.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            var face = FaceOf(mmd, descriptor);
            if (face == null)
            {
                VixenMMDReport.Error("vixenmmd.noface", mmd);
                return;
            }
            var root = ctx.AvatarRootTransform;
            if (mmd.moveFaceToRoot && face.transform.parent != root && face.transform != root)
            {
                face.transform.SetParent(root, true);
            }
            if (mmd.renameFaceToBody && face.name != "Body")
            {
                var taken = new HashSet<string>();
                foreach (Transform child in root) taken.Add(child.name);
                foreach (Transform child in root)
                {
                    if (child == face.transform || child.name != "Body") continue;
                    child.name = UniqueName("Body (Original)", taken);
                    taken.Add(child.name);
                }
                face.name = "Body";
            }
        }

        internal static string UniqueName(string wanted, HashSet<string> taken)
        {
            if (!taken.Contains(wanted)) return wanted;
            for (int i = 2; ; i++)
            {
                string n = wanted + " " + i;
                if (!taken.Contains(n)) return n;
            }
        }

        internal static void BuildMorphs(BuildContext ctx)
        {
            var mmd = Find(ctx);
            if (mmd == null) return;
            var descriptor = ctx.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            var face = FaceOf(mmd, descriptor);
            if (face == null || face.sharedMesh == null) return;
            var result = VixenMMDMorphBuilder.Build(face, mmd.morphs);
            foreach (var p in result.Skipped) VixenMMDReport.Warn("vixenmmd.skipped", p.Morph, p.Detail, mmd);
            if (result.Mesh != null) ctx.AssetSaver.SaveAsset(result.Mesh);

            var keep = ctx.GetState<KeepState>();
            keep.Face = face;
            keep.Host = face.gameObject;
            keep.Names.Clear();
            var ours = new HashSet<string>(mmd.morphs.Where(m => m != null && m.enabled).Select(m => m.name));
            var mesh = face.sharedMesh;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string n = mesh.GetBlendShapeName(i);
                if (VixenMMDNames.IsKnown(n) || ours.Contains(n)) keep.Names.Add(n);
            }
        }

        internal const string KeepHelperName = "VixenMMD Keep MMD Shapes";

        internal sealed class KeepState
        {
            public SkinnedMeshRenderer Face;
            public GameObject Host;
            public readonly List<string> Names = new List<string>();
            public AnimatorController Controller;
            public AnimationClip Clip;
        }

        internal static void KeepShapesForVrcFury(BuildContext ctx)
        {
            var keep = ctx.GetState<KeepState>();
            if (keep.Face == null || keep.Face.sharedMesh == null || keep.Names.Count == 0) return;
            if (!VixenMMDChecks.HasVrcFuryFeature(ctx.AvatarRootObject, "BlendshapeOptimizer")) return;
            var blocker = VixenMMDChecks.VrcFuryKeepBlocker(keep.Host);
            if (blocker != null)
            {
                VixenMMDReport.Warn("vixenmmd.vrcfnokeep", blocker);
                return;
            }

            var mesh = keep.Face.sharedMesh;
            var clip = new AnimationClip { name = KeepHelperName };
            foreach (var n in keep.Names)
            {
                int index = mesh.GetBlendShapeIndex(n);
                float now = index >= 0 ? keep.Face.GetBlendShapeWeight(index) : 0f;
                float value = Mathf.Approximately(now, 100f) ? 0f : 100f;
                clip.SetCurve("", typeof(SkinnedMeshRenderer), "blendShape." + n, AnimationCurve.Constant(0f, 1f / 60f, value));
            }

            var controller = new AnimatorController { name = KeepHelperName };
            controller.AddLayer("Keep");
            var state = controller.layers[0].stateMachine.AddState("Keep");
            state.motion = clip;
            state.writeDefaultValues = false;

            var animator = keep.Host.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.enabled = false;
            keep.Controller = controller;
            keep.Clip = clip;
        }

        internal static void RemoveKeepHelper(BuildContext ctx)
        {
            var keep = ctx.GetState<KeepState>();
            if (keep.Controller == null || keep.Host == null) return;
            foreach (var a in keep.Host.GetComponents<Animator>())
            {
                var c = a.runtimeAnimatorController;
                if (c == keep.Controller || (c != null && c.name.Contains(KeepHelperName))) Object.DestroyImmediate(a);
            }
            keep.Controller = null;
            keep.Clip = null;
        }

        internal static void FxLayers(BuildContext ctx)
        {
            var mmd = Find(ctx);
            if (mmd == null) return;
            try
            {
                bool wantsFx = (mmd.removeLayers != null && mmd.removeLayers.Count > 0) || mmd.worldGuard || mmd.forceWriteDefaults;
                if (!wantsFx) return;
                var asc = ctx.Extension<AnimatorServicesContext>();
                if (!asc.ControllerContext.Controllers.TryGetValue(VRCAvatarDescriptor.AnimLayerType.FX, out var fx) || fx == null)
                {
                    VixenMMDReport.Warn("vixenmmd.nofx", mmd);
                    return;
                }
                RemoveLayers(fx, mmd);
                if (mmd.worldGuard) AddGuard(asc.ControllerContext.CloneContext, fx, mmd);
                if (mmd.forceWriteDefaults)
                {
                    foreach (var layer in fx.Layers)
                    {
                        foreach (var state in AllStates(layer.StateMachine)) state.WriteDefaultValues = true;
                    }
                }
            }
            finally
            {
                foreach (var c in ctx.AvatarRootObject.GetComponentsInChildren<VixenMMD>(true)) Object.DestroyImmediate(c);
            }
        }

        private static void RemoveLayers(VirtualAnimatorController fx, VixenMMD mmd)
        {
            if (mmd.removeLayers == null || mmd.removeLayers.Count == 0) return;
            var wanted = new HashSet<string>(mmd.removeLayers.Where(n => !string.IsNullOrEmpty(n)));
            var layers = fx.Layers.ToList();
            var missing = wanted.Where(n => layers.All(l => l.Name != n)).ToList();
            if (missing.Count > 0) VixenMMDReport.Info("vixenmmd.layersmissing", string.Join(", ", missing), mmd);
            fx.Layers = layers.Where(l => !wanted.Contains(l.Name)).ToList();
        }

        private static void AddGuard(CloneContext cc, VirtualAnimatorController fx, VixenMMD mmd)
        {
            var names = new HashSet<string>((mmd.guardLayers ?? new List<string>()).Where(n => !string.IsNullOrEmpty(n)));
            var layers = fx.Layers.ToList();
            var targets = layers.Where(l => names.Contains(l.Name)).ToList();
            var missing = names.Where(n => targets.All(l => l.Name != n)).ToList();
            if (targets.Count == 0)
            {
                VixenMMDReport.Warn("vixenmmd.guardnothing", mmd);
                return;
            }
            if (missing.Count > 0) VixenMMDReport.Info("vixenmmd.layersmissing", string.Join(", ", missing), mmd);

            string param = GuardParameter;
            for (int i = 2; fx.Parameters.ContainsKey(param); i++) param = GuardParameter + i;
            fx.Parameters = fx.Parameters.SetItem(param, new AnimatorControllerParameter
            {
                name = param,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f
            });

            var detector = VirtualLayer.Create(cc, "VixenMMD Detector");
            detector.DefaultWeight = 1f;
            detector.StateMachine = VirtualStateMachine.Create(cc, "VixenMMD Detector");
            var clip = VirtualClip.Create("VixenMMD Detector");
            clip.SetFloatCurve("", typeof(Animator), param, AnimationCurve.Constant(0f, 1f / 60f, 0f));
            var probe = AddState(detector.StateMachine, "Normal world", clip, new Vector3(300f, 0f, 0f));
            probe.WriteDefaultValues = true;
            detector.StateMachine.DefaultState = probe;

            var control = VirtualLayer.Create(cc, "VixenMMD Layer Control");
            control.DefaultWeight = 0f;
            control.StateMachine = VirtualStateMachine.Create(cc, "VixenMMD Layer Control");
            var normal = AddState(control.StateMachine, "Normal world", null, new Vector3(300f, 0f, 0f));
            var inMmd = AddState(control.StateMachine, "MMD world", null, new Vector3(300f, 100f, 0f));
            normal.WriteDefaultValues = true;
            inMmd.WriteDefaultValues = true;
            control.StateMachine.DefaultState = normal;
            normal.Transitions = normal.Transitions.Add(Transition(inMmd, param, AnimatorConditionMode.Greater));
            inMmd.Transitions = inMmd.Transitions.Add(Transition(normal, param, AnimatorConditionMode.Less));

            foreach (var t in targets)
            {
                normal.Behaviours = normal.Behaviours.Add(LayerControl(t.VirtualLayerIndex, t.DefaultWeight));
                inMmd.Behaviours = inMmd.Behaviours.Add(LayerControl(t.VirtualLayerIndex, 0f));
            }

            int at = Math.Min(1, layers.Count);
            layers.Insert(at, detector);
            layers.Insert(at + 1, control);
            fx.Layers = layers;
        }

        private static VirtualState AddState(VirtualStateMachine sm, string name, VirtualMotion motion, Vector3 position)
        {
            var state = VirtualState.Create(name);
            state.Motion = motion;
            sm.States = sm.States.Add(new VirtualStateMachine.VirtualChildState { State = state, Position = position });
            return state;
        }

        private static VirtualStateTransition Transition(VirtualState to, string param, AnimatorConditionMode mode)
        {
            var t = VirtualStateTransition.Create();
            t.SetDestination(to);
            t.ExitTime = null;
            t.Duration = 0f;
            t.HasFixedDuration = true;
            t.Conditions = ImmutableList.Create(new AnimatorCondition { mode = mode, parameter = param, threshold = 0.5f });
            return t;
        }

        private static StateMachineBehaviour LayerControl(int virtualLayer, float weight)
        {
            var alc = ScriptableObject.CreateInstance<VRCAnimatorLayerControl>();
            alc.playable = VRC_AnimatorLayerControl.BlendableLayer.FX;
            alc.layer = virtualLayer;
            alc.goalWeight = weight;
            alc.blendDuration = 0f;
            return alc;
        }

        internal static IEnumerable<VirtualState> AllStates(VirtualStateMachine sm)
        {
            if (sm == null) yield break;
            foreach (var child in sm.States)
            {
                if (child.State != null) yield return child.State;
            }
            foreach (var sub in sm.StateMachines)
            {
                foreach (var s in AllStates(sub.StateMachine)) yield return s;
            }
        }
    }
}
