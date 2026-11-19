#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;

namespace VixenTools.Editor
{
    [FilePath("ProjectSettings/VixenToolsUploadOptimizer.asset", FilePathAttribute.Location.ProjectFolder)]
    public class VixenUploadSettings : ScriptableSingleton<VixenUploadSettings>
    {
        [System.Serializable]
        public class Entry
        {
            public string BlueprintId = "";
            public string AvatarName = "";
            public bool RemoveUnusedBlendShapes;
            public bool MergeRepeatedSlots;
            public bool RemoveUnusedColliders;
            public bool MotionVectorsOff;
            public bool RingFingerOnFeet;
            public bool DisablePhysBonesWithMesh;
            public bool RemoveUnusedParameters;
            public bool MergeSameRatioBlendShapes;

            public bool Any => RemoveUnusedBlendShapes || MergeRepeatedSlots || RemoveUnusedColliders || MotionVectorsOff || RingFingerOnFeet
                || DisablePhysBonesWithMesh || RemoveUnusedParameters || MergeSameRatioBlendShapes;
        }

        public List<Entry> Entries = new List<Entry>();

        public static string BlueprintOf(GameObject avatar)
        {
            var pm = avatar != null ? avatar.GetComponent<PipelineManager>() : null;
            return pm != null && pm.blueprintId != null ? pm.blueprintId : "";
        }

        public static string NameOf(GameObject avatar)
        {
            string n = avatar != null ? avatar.name : "";
            return n.EndsWith("(Clone)") ? n.Substring(0, n.Length - 7).TrimEnd() : n;
        }

        public Entry Find(GameObject avatar, bool create)
        {
            string id = BlueprintOf(avatar);
            string name = NameOf(avatar);
            Entry entry = null;
            if (!string.IsNullOrEmpty(id)) entry = Entries.FirstOrDefault(e => e.BlueprintId == id);
            if (entry == null) entry = Entries.FirstOrDefault(e => string.IsNullOrEmpty(e.BlueprintId) && e.AvatarName == name);
            if (entry != null)
            {
                if (create && string.IsNullOrEmpty(entry.BlueprintId) && !string.IsNullOrEmpty(id))
                {
                    entry.BlueprintId = id;
                    SaveNow();
                }
                return entry;
            }
            if (!create) return null;
            entry = new Entry { BlueprintId = id, AvatarName = name };
            Entries.Add(entry);
            SaveNow();
            return entry;
        }

        public void SaveNow() => Save(true);
    }

    public class VixenUploadOptimizerHook : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => 5000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return true;
            var entry = VixenUploadSettings.instance.Find(avatarGameObject, false);
            if (entry == null || !entry.Any) return true;
            try
            {
                VixenUploadOptimizer.Run(avatarGameObject, entry);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VixForge] On Upload stopped part way and the upload carries on without the rest: {e}");
            }
            return true;
        }
    }

    public static class VixenUploadOptimizer
    {
        public const string GeneratedFolder = "Assets/VixenTools/Generated/Upload";

        public static List<string> Run(GameObject avatar, VixenUploadSettings.Entry entry)
        {
            var log = new List<string>();
            if (AssetDatabase.IsValidFolder(GeneratedFolder)) AssetDatabase.DeleteAsset(GeneratedFolder);
            VixenMeshPatcher.FolderOverride = GeneratedFolder;
            try
            {
                var report = AvatarSDKValidator.RunFullSweep(avatar);
                foreach (var task in report.OptimizationSuite)
                {
                    bool wanted =
                        (task.ID == "REMOVE_UNUSED_BLENDSHAPES" && entry.RemoveUnusedBlendShapes) ||
                        (task.ID == "MERGE_REPEATED_SLOTS" && entry.MergeRepeatedSlots) ||
                        (task.ID == "REMOVE_UNUSED_COLLIDERS" && entry.RemoveUnusedColliders) ||
                        (task.ID == "SKINNED_MOTION_VECTORS_OFF" && entry.MotionVectorsOff);
                    if (!wanted || task.Execute == null) continue;
                    task.Execute();
                    log.Add(System.Text.RegularExpressions.Regex.Replace(task.Label, "<.*?>", ""));
                }
                var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
                if (descriptor != null && (entry.DisablePhysBonesWithMesh || entry.RemoveUnusedParameters || entry.MergeSameRatioBlendShapes))
                {
                    var animators = new UploadAnimators(descriptor);
                    if (entry.DisablePhysBonesWithMesh)
                    {
                        int n = UploadAnimationPasses.DisablePhysBonesWithMesh(avatar, animators);
                        if (n > 0) log.Add($"{n} PhysBone(s) now turn off with their mesh");
                    }
                    if (entry.MergeSameRatioBlendShapes)
                    {
                        int n = UploadAnimationPasses.MergeSameRatioBlendShapes(avatar, descriptor, animators);
                        if (n > 0) log.Add($"{n} blend shape(s) merged into the shape they always move with");
                    }
                    if (entry.RemoveUnusedParameters)
                    {
                        int n = UploadAnimationPasses.RemoveUnusedParameters(descriptor, animators);
                        if (n > 0) log.Add($"{n} unused animator parameter(s) removed");
                    }
                    animators.SaveEdited();
                }
                if (entry.RingFingerOnFeet && MoveRingFingerCollidersToFeet(avatar)) log.Add("Ring finger colliders moved to the feet");
            }
            finally
            {
                VixenMeshPatcher.FolderOverride = null;
            }
            Debug.Log($"[VixForge] On Upload for '{avatar.name}': " + (log.Count == 0 ? "nothing needed changing." : string.Join("; ", log) + "."));
            return log;
        }

        public static bool MoveRingFingerCollidersToFeet(GameObject avatar)
        {
            var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null) return false;
            if (descriptor.collider_footL.transform == null || descriptor.collider_footR.transform == null) return false;

            VRCAvatarDescriptor.ColliderConfig OnFoot(VRCAvatarDescriptor.ColliderConfig foot, string side)
            {
                foot.state = VRCAvatarDescriptor.ColliderConfig.State.Custom;
                foot.height -= foot.radius * 2f;
                var start = new GameObject($"{side} Foot Collider Start");
                start.transform.SetParent(foot.transform, false);
                start.transform.SetLocalPositionAndRotation(foot.position + foot.rotation * (-(foot.height * 0.5f) * Vector3.up), foot.rotation);
                var end = new GameObject($"{side} Foot Collider End");
                end.transform.SetParent(start.transform, false);
                end.transform.SetLocalPositionAndRotation(new Vector3(0f, foot.height, 0f), Quaternion.identity);
                foot.transform = end.transform;
                return foot;
            }

            descriptor.collider_fingerRingL = OnFoot(descriptor.collider_footL, "Left");
            descriptor.collider_fingerRingR = OnFoot(descriptor.collider_footR, "Right");
            return true;
        }
    }

    public sealed class UploadAnimators
    {
        private readonly VRCAvatarDescriptor _descriptor;
        private readonly Dictionary<AnimationClip, AnimationClip> _clipCopies = new Dictionary<AnimationClip, AnimationClip>();
        private readonly Dictionary<UnityEditor.Animations.AnimatorController, UnityEditor.Animations.AnimatorController> _controllerCopies =
            new Dictionary<UnityEditor.Animations.AnimatorController, UnityEditor.Animations.AnimatorController>();
        private bool _prepared;

        public readonly List<UnityEditor.Animations.AnimatorController> Controllers = new List<UnityEditor.Animations.AnimatorController>();

        public UploadAnimators(VRCAvatarDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public static bool IsGenerated(Object asset)
        {
            if (asset == null) return false;
            string path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrEmpty(path)
                || path.StartsWith("Packages/com.vrcfury.temp/")
                || path.StartsWith("Packages/nadena.dev.ndmf/__Generated/")
                || path.StartsWith(VixenUploadOptimizer.GeneratedFolder + "/");
        }

        public void Prepare()
        {
            if (_prepared) return;
            _prepared = true;
            VixenMeshPatcher.EnsureFolder(VixenUploadOptimizer.GeneratedFolder);
            var baseLayers = _descriptor.baseAnimationLayers;
            for (int i = 0; i < baseLayers.Length; i++) baseLayers[i].animatorController = Editable(baseLayers[i].animatorController);
            _descriptor.baseAnimationLayers = baseLayers;
            var specialLayers = _descriptor.specialAnimationLayers;
            for (int i = 0; i < specialLayers.Length; i++) specialLayers[i].animatorController = Editable(specialLayers[i].animatorController);
            _descriptor.specialAnimationLayers = specialLayers;
        }

        private RuntimeAnimatorController Editable(RuntimeAnimatorController controller)
        {
            if (!(controller is UnityEditor.Animations.AnimatorController source)) return controller;
            if (_controllerCopies.TryGetValue(source, out var existing)) return existing;
            var editable = source;
            if (!IsGenerated(source))
            {
                string path = $"{VixenUploadOptimizer.GeneratedFolder}/{source.name}_{System.Guid.NewGuid().ToString("N").Substring(0, 6)}.controller";
                if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source), path)) return controller;
                editable = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
            }
            string ownPath = AssetDatabase.GetAssetPath(editable);
            foreach (var state in States(editable))
                if (state.motion is UnityEditor.Animations.BlendTree tree) state.motion = EditableTree(tree, ownPath);
            Controllers.Add(editable);
            _controllerCopies[source] = editable;
            return editable;
        }

        private static UnityEditor.Animations.BlendTree EditableTree(UnityEditor.Animations.BlendTree tree, string controllerPath)
        {
            var result = tree;
            bool changed = false;
            if (!IsGenerated(tree) && AssetDatabase.GetAssetPath(tree) != controllerPath)
            {
                result = Object.Instantiate(tree);
                result.name = tree.name;
                result.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(result, controllerPath);
                changed = true;
            }
            var children = result.children;
            for (int i = 0; i < children.Length; i++)
            {
                if (!(children[i].motion is UnityEditor.Animations.BlendTree sub)) continue;
                var editableSub = EditableTree(sub, controllerPath);
                if (editableSub == sub) continue;
                children[i].motion = editableSub;
                changed = true;
            }
            if (changed) result.children = children;
            return result;
        }

        public static IEnumerable<UnityEditor.Animations.AnimatorStateMachine> Machines(UnityEditor.Animations.AnimatorStateMachine machine)
        {
            if (machine == null) yield break;
            yield return machine;
            foreach (var child in machine.stateMachines)
                foreach (var nested in Machines(child.stateMachine)) yield return nested;
        }

        public static IEnumerable<UnityEditor.Animations.AnimatorState> States(UnityEditor.Animations.AnimatorController controller)
        {
            return controller.layers.SelectMany(l => Machines(l.stateMachine)).SelectMany(m => m.states.Select(s => s.state));
        }

        public List<AnimationClip> AllClips()
        {
            Prepare();
            return Controllers.SelectMany(c => c.animationClips).Where(c => c != null).Distinct().ToList();
        }

        private readonly HashSet<Object> _edited = new HashSet<Object>();

        public void SaveEdited()
        {
            foreach (var c in Controllers) if (c != null) AssetDatabase.SaveAssetIfDirty(c);
            foreach (var o in _edited) if (o != null) AssetDatabase.SaveAssetIfDirty(o);
        }

        public AnimationClip EditableClip(AnimationClip clip)
        {
            var editable = EditableClipInner(clip);
            if (editable != null) _edited.Add(editable);
            return editable;
        }

        private AnimationClip EditableClipInner(AnimationClip clip)
        {
            Prepare();
            if (clip == null || IsGenerated(clip)) return clip;
            if (_clipCopies.TryGetValue(clip, out var copy)) return copy;
            copy = Object.Instantiate(clip);
            copy.name = clip.name;
            AssetDatabase.CreateAsset(copy, $"{VixenUploadOptimizer.GeneratedFolder}/{clip.name}_{System.Guid.NewGuid().ToString("N").Substring(0, 6)}.anim");
            _clipCopies[clip] = copy;
            foreach (var controller in Controllers)
            {
                foreach (var state in States(controller))
                {
                    if (state.motion == clip) state.motion = copy;
                    else if (state.motion is UnityEditor.Animations.BlendTree tree) Repoint(tree, clip, copy);
                }
            }
            return copy;
        }

        private static void Repoint(UnityEditor.Animations.BlendTree tree, Motion from, Motion to)
        {
            var children = tree.children;
            bool changed = false;
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].motion == from) { children[i].motion = to; changed = true; }
                else if (children[i].motion is UnityEditor.Animations.BlendTree sub) Repoint(sub, from, to);
            }
            if (changed) tree.children = children;
        }
    }

    public static class UploadAnimationPasses
    {
        public static int DisablePhysBonesWithMesh(GameObject avatar, UploadAnimators animators)
        {
            var root = avatar.transform;
            var clips = animators.AllClips();
            var toggleClips = new Dictionary<string, List<(AnimationClip clip, EditorCurveBinding binding)>>();
            var animatedPhysBones = new HashSet<string>();
            foreach (var clip in clips)
            {
                foreach (var b in AnimationUtility.GetCurveBindings(clip))
                {
                    bool toggle = (b.type == typeof(GameObject) && b.propertyName == "m_IsActive")
                               || (b.type == typeof(SkinnedMeshRenderer) && b.propertyName == "m_Enabled");
                    if (toggle)
                    {
                        string key = b.path + "|" + b.type.Name + "|" + b.propertyName;
                        if (!toggleClips.TryGetValue(key, out var list)) toggleClips[key] = list = new List<(AnimationClip, EditorCurveBinding)>();
                        list.Add((clip, b));
                    }
                    if (typeof(VRC.Dynamics.VRCPhysBoneBase).IsAssignableFrom(b.type) && b.propertyName == "m_Enabled") animatedPhysBones.Add(b.path);
                }
            }

            var renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var weighted = renderers.ToDictionary(r => r, WeightedBones);
            int done = 0;
            foreach (var pb in avatar.GetComponentsInChildren<VRC.Dynamics.VRCPhysBoneBase>(true))
            {
                if (pb == null || !string.IsNullOrEmpty(pb.parameter)) continue;
                string pbPath = AnimationUtility.CalculateTransformPath(pb.transform, root);
                if (animatedPhysBones.Contains(pbPath)) continue;
                var affected = AffectedTransforms(pb);
                if (affected.Count == 0) continue;
                bool sharedBones = affected.Any(t => t.GetComponents<Component>().Any(c =>
                    c != null && !(c is Transform) && !(c is VRC.Dynamics.VRCPhysBoneBase) && !(c is VRC.Dynamics.VRCPhysBoneColliderBase)
                    && !(c is VRC.SDKBase.IEditorOnly)));
                if (sharedBones) continue;
                var users = renderers.Where(r => weighted[r].Overlaps(affected)).ToList();
                if (users.Count != 1) continue;
                var mesh = users[0];

                var candidates = new List<string>
                {
                    AnimationUtility.CalculateTransformPath(mesh.transform, root) + "|" + nameof(SkinnedMeshRenderer) + "|m_Enabled"
                };
                for (var t = mesh.transform; t != null && t != root; t = t.parent)
                {
                    if (pb.transform == t || pb.transform.IsChildOf(t)) break;
                    candidates.Add(AnimationUtility.CalculateTransformPath(t, root) + "|" + nameof(GameObject) + "|m_IsActive");
                }
                var hits = candidates.Where(c => toggleClips.ContainsKey(c)).ToList();
                if (hits.Count != 1) continue;

                var target = EditorCurveBinding.FloatCurve(pbPath, pb.GetType(), "m_Enabled");
                foreach (var use in toggleClips[hits[0]])
                {
                    var curve = AnimationUtility.GetEditorCurve(use.clip, use.binding);
                    if (curve == null) continue;
                    AnimationUtility.SetEditorCurve(animators.EditableClip(use.clip), target, curve);
                }
                done++;
            }
            return done;
        }

        private static HashSet<Transform> WeightedBones(SkinnedMeshRenderer renderer)
        {
            var set = new HashSet<Transform>();
            var mesh = renderer.sharedMesh;
            var bones = renderer.bones;
            if (mesh == null || bones == null) return set;
            foreach (var w in mesh.GetAllBoneWeights())
                if (w.weight > 0f && w.boneIndex >= 0 && w.boneIndex < bones.Length && bones[w.boneIndex] != null) set.Add(bones[w.boneIndex]);
            return set;
        }

        private static HashSet<Transform> AffectedTransforms(VRC.Dynamics.VRCPhysBoneBase pb)
        {
            var set = new HashSet<Transform>();
            var start = pb.GetRootTransform();
            if (start == null) return set;
            foreach (var t in start.GetComponentsInChildren<Transform>(true))
            {
                bool ignored = pb.ignoreTransforms != null && pb.ignoreTransforms.Any(ig => ig != null && (t == ig || t.IsChildOf(ig)));
                if (!ignored) set.Add(t);
            }
            return set;
        }

        public static int RemoveUnusedParameters(VRCAvatarDescriptor descriptor, UploadAnimators animators)
        {
            animators.Prepare();
            var keep = new HashSet<string>();
            if (descriptor.expressionParameters != null && descriptor.expressionParameters.parameters != null)
                foreach (var p in descriptor.expressionParameters.parameters) if (p != null && !string.IsNullOrEmpty(p.name)) keep.Add(p.name);

            int removed = 0;
            foreach (var controller in animators.Controllers)
            {
                var used = new HashSet<string>(keep);
                void Conditions(IEnumerable<UnityEditor.Animations.AnimatorTransitionBase> transitions)
                {
                    foreach (var t in transitions) foreach (var c in t.conditions) used.Add(c.parameter);
                }
                void Strings(Object o)
                {
                    if (o == null) return;
                    var it = new SerializedObject(o).GetIterator();
                    while (it.Next(true)) if (it.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(it.stringValue)) used.Add(it.stringValue);
                }
                void Tree(Motion motion)
                {
                    if (!(motion is UnityEditor.Animations.BlendTree tree)) return;
                    used.Add(tree.blendParameter);
                    used.Add(tree.blendParameterY);
                    foreach (var child in tree.children) { used.Add(child.directBlendParameter); Tree(child.motion); }
                }
                foreach (var layer in controller.layers)
                {
                    foreach (var machine in UploadAnimators.Machines(layer.stateMachine))
                    {
                        Conditions(machine.anyStateTransitions);
                        Conditions(machine.entryTransitions);
                        foreach (var child in machine.stateMachines) Conditions(machine.GetStateMachineTransitions(child.stateMachine));
                        foreach (var b in machine.behaviours) Strings(b);
                        foreach (var child in machine.states)
                        {
                            var s = child.state;
                            Conditions(s.transitions);
                            if (s.speedParameterActive) used.Add(s.speedParameter);
                            if (s.cycleOffsetParameterActive) used.Add(s.cycleOffsetParameter);
                            if (s.mirrorParameterActive) used.Add(s.mirrorParameter);
                            if (s.timeParameterActive) used.Add(s.timeParameter);
                            Tree(s.motion);
                            foreach (var b in s.behaviours) Strings(b);
                        }
                    }
                }
                var unused = controller.parameters.Where(p => !used.Contains(p.name)).Select(p => p.name).ToList();
                foreach (var name in unused)
                {
                    int index = System.Array.FindIndex(controller.parameters, p => p.name == name);
                    if (index < 0) continue;
                    controller.RemoveParameter(index);
                    removed++;
                }
            }
            return removed;
        }

        public static int MergeSameRatioBlendShapes(GameObject avatar, VRCAvatarDescriptor descriptor, UploadAnimators animators)
        {
            var root = avatar.transform;
            var renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var protectedNames = AvatarSDKValidator.CollectProtectedBlendShapeNames(avatar, descriptor, renderers, new Dictionary<AnimationClip, HashSet<Transform>>());
            var clips = animators.AllClips();
            int merged = 0;

            foreach (var smr in renderers)
            {
                var mesh = smr.sharedMesh;
                if (mesh == null || mesh.blendShapeCount < 2) continue;
                bool otherAnimator = false;
                for (var t = smr.transform; t != null && t != root; t = t.parent)
                {
                    var anim = t.GetComponent<Animator>();
                    if (anim != null && anim.runtimeAnimatorController != null) { otherAnimator = true; break; }
                }
                if (otherAnimator) continue;

                string path = AnimationUtility.CalculateTransformPath(smr.transform, root);
                var curves = new Dictionary<string, Dictionary<AnimationClip, AnimationCurve>>();
                foreach (var clip in clips)
                {
                    foreach (var b in AnimationUtility.GetCurveBindings(clip))
                    {
                        if (b.path != path || b.type != typeof(SkinnedMeshRenderer) || !b.propertyName.StartsWith("blendShape.")) continue;
                        string shape = b.propertyName.Substring(11);
                        if (!curves.TryGetValue(shape, out var perClip)) curves[shape] = perClip = new Dictionary<AnimationClip, AnimationCurve>();
                        perClip[clip] = AnimationUtility.GetEditorCurve(clip, b);
                    }
                }

                var candidates = curves.Keys
                    .Where(n => mesh.GetBlendShapeIndex(n) >= 0 && !protectedNames.Contains(n) && mesh.GetBlendShapeFrameCount(mesh.GetBlendShapeIndex(n)) == 1)
                    .OrderBy(n => mesh.GetBlendShapeIndex(n)).ToList();
                var into = new Dictionary<string, (string target, float ratio)>();
                for (int a = 0; a < candidates.Count; a++)
                {
                    if (into.ContainsKey(candidates[a])) continue;
                    float restA = smr.GetBlendShapeWeight(mesh.GetBlendShapeIndex(candidates[a]));
                    for (int b = a + 1; b < candidates.Count; b++)
                    {
                        if (into.ContainsKey(candidates[b])) continue;
                        float restB = smr.GetBlendShapeWeight(mesh.GetBlendShapeIndex(candidates[b]));
                        if (TryRatio(curves[candidates[a]], curves[candidates[b]], restA, restB, out float k))
                            into[candidates[b]] = (candidates[a], k);
                    }
                }
                if (into.Count == 0) continue;

                MergeShapes(smr, into, descriptor);
                foreach (var pair in into)
                {
                    var binding = EditorCurveBinding.FloatCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + pair.Key);
                    foreach (var clip in curves[pair.Key].Keys) AnimationUtility.SetEditorCurve(animators.EditableClip(clip), binding, null);
                }
                merged += into.Count;
            }
            return merged;
        }

        private static bool TryRatio(Dictionary<AnimationClip, AnimationCurve> a, Dictionary<AnimationClip, AnimationCurve> b, float restA, float restB, out float k)
        {
            k = 0f;
            if (a.Count != b.Count || a.Keys.Any(c => !b.ContainsKey(c))) return false;
            bool found = false;
            if (Mathf.Abs(restA) > 1e-4f) { k = restB / restA; found = true; }
            else if (Mathf.Abs(restB) > 1e-4f) return false;
            if (!found)
            {
                foreach (var clip in a.Keys)
                {
                    foreach (var key in a[clip].keys)
                    {
                        if (Mathf.Abs(key.value) <= 1e-4f) continue;
                        var other = b[clip].keys.FirstOrDefault(x => Mathf.Abs(x.time - key.time) < 1e-5f);
                        k = other.value / key.value;
                        found = true;
                        break;
                    }
                    if (found) break;
                }
            }
            if (!found || Mathf.Abs(k) < 1e-4f) return false;
            float tolerance = 1e-3f * Mathf.Max(1f, Mathf.Abs(k));
            foreach (var clip in a.Keys)
            {
                var ka = a[clip].keys;
                var kb = b[clip].keys;
                if (ka.Length != kb.Length) return false;
                for (int i = 0; i < ka.Length; i++)
                {
                    if (Mathf.Abs(ka[i].time - kb[i].time) > 1e-5f) return false;
                    if (Mathf.Abs(kb[i].value - k * ka[i].value) > tolerance) return false;
                    if (!float.IsInfinity(ka[i].inTangent) && Mathf.Abs(kb[i].inTangent - k * ka[i].inTangent) > tolerance) return false;
                    if (!float.IsInfinity(ka[i].outTangent) && Mathf.Abs(kb[i].outTangent - k * ka[i].outTangent) > tolerance) return false;
                    if (float.IsInfinity(ka[i].inTangent) != float.IsInfinity(kb[i].inTangent)) return false;
                    if (float.IsInfinity(ka[i].outTangent) != float.IsInfinity(kb[i].outTangent)) return false;
                }
            }
            return Mathf.Abs(restB - k * restA) <= tolerance;
        }

        private static void MergeShapes(SkinnedMeshRenderer smr, Dictionary<string, (string target, float ratio)> into, VRCAvatarDescriptor descriptor)
        {
            var source = smr.sharedMesh;
            var weights = new Dictionary<string, float>();
            for (int i = 0; i < source.blendShapeCount; i++) weights[source.GetBlendShapeName(i)] = smr.GetBlendShapeWeight(i);
            bool lids = descriptor != null && descriptor.customEyeLookSettings.eyelidsSkinnedMesh == smr && descriptor.customEyeLookSettings.eyelidsBlendshapes != null;
            string[] lidNames = lids
                ? descriptor.customEyeLookSettings.eyelidsBlendshapes.Select(i => i >= 0 && i < source.blendShapeCount ? source.GetBlendShapeName(i) : null).ToArray()
                : null;

            VixenMeshPatcher.PatchSkinnedMesh(smr, "SameRatio", (m, r) =>
            {
                int vc = m.vertexCount;
                var frames = new List<(string name, float weight, Vector3[] v, Vector3[] n, Vector3[] t)>();
                var byName = new Dictionary<string, int>();
                for (int i = 0; i < m.blendShapeCount; i++)
                {
                    for (int f = 0; f < m.GetBlendShapeFrameCount(i); f++)
                    {
                        var dv = new Vector3[vc]; var dn = new Vector3[vc]; var dt = new Vector3[vc];
                        m.GetBlendShapeFrameVertices(i, f, dv, dn, dt);
                        if (f == 0) byName[m.GetBlendShapeName(i)] = frames.Count;
                        frames.Add((m.GetBlendShapeName(i), m.GetBlendShapeFrameWeight(i, f), dv, dn, dt));
                    }
                }
                foreach (var pair in into)
                {
                    var target = frames[byName[pair.Value.target]];
                    var merge = frames[byName[pair.Key]];
                    float scale = pair.Value.ratio * target.weight / merge.weight;
                    for (int k = 0; k < vc; k++)
                    {
                        target.v[k] += merge.v[k] * scale;
                        target.n[k] += merge.n[k] * scale;
                        target.t[k] += merge.t[k] * scale;
                    }
                }
                m.ClearBlendShapes();
                foreach (var frame in frames)
                    if (!into.ContainsKey(frame.name)) m.AddBlendShapeFrame(frame.name, frame.weight, frame.v, frame.n, frame.t);
            }, false);

            var patched = smr.sharedMesh;
            for (int i = 0; i < patched.blendShapeCount; i++)
                smr.SetBlendShapeWeight(i, weights.TryGetValue(patched.GetBlendShapeName(i), out float w) ? w : 0f);
            if (lids)
            {
                var eye = descriptor.customEyeLookSettings;
                eye.eyelidsBlendshapes = lidNames.Select(n => n == null ? -1 : patched.GetBlendShapeIndex(n)).ToArray();
                descriptor.customEyeLookSettings = eye;
            }
        }
    }
}
#endif
