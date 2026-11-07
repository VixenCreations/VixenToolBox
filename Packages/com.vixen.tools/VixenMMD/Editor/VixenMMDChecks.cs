using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static VixenTools.MMD.Editor.VixenMMDText;
using VRC.SDK3.Avatars.Components;

namespace VixenTools.MMD.Editor
{
    internal enum CheckLevel { Info, Warning, Error }

    internal sealed class Check
    {
        public CheckLevel Level;
        public string Message;
        public string FixLabel;
        public Action Fix;
    }

    internal static class VixenMMDChecks
    {
        internal static VRCAvatarDescriptor AvatarOf(Component c)
        {
            return c != null ? c.GetComponentInParent<VRCAvatarDescriptor>(true) : null;
        }

        internal static AnimatorController FxController(VRCAvatarDescriptor d)
        {
            if (d == null || d.baseAnimationLayers == null) return null;
            foreach (var l in d.baseAnimationLayers)
            {
                if (l.type == VRCAvatarDescriptor.AnimLayerType.FX && !l.isDefault && l.animatorController != null) return l.animatorController as AnimatorController;
            }
            return null;
        }

        internal static List<string> FxLayerNames(VRCAvatarDescriptor d)
        {
            var fx = FxController(d);
            return fx != null ? fx.layers.Select(l => l.name).ToList() : new List<string>();
        }

        internal static bool HasWriteDefaultsOff(AnimatorController fx)
        {
            if (fx == null) return false;
            foreach (var layer in fx.layers)
            {
                foreach (var s in States(layer.stateMachine))
                {
                    if (!s.writeDefaultValues) return true;
                }
            }
            return false;
        }

        private static IEnumerable<AnimatorState> States(AnimatorStateMachine sm)
        {
            if (sm == null) yield break;
            foreach (var c in sm.states) if (c.state != null) yield return c.state;
            foreach (var sub in sm.stateMachines)
            {
                foreach (var s in States(sub.stateMachine)) yield return s;
            }
        }

        internal static List<Check> Run(VixenMMD mmd)
        {
            var list = new List<Check>();
            var avatar = AvatarOf(mmd);
            if (avatar == null)
            {
                list.Add(new Check { Level = CheckLevel.Error, Message = L("Put VixenMMD inside an avatar. It needs a VRC Avatar Descriptor above it.") });
                return list;
            }
            var all = avatar.GetComponentsInChildren<VixenMMD>(true);
            if (all.Length > 1)
            {
                list.Add(new Check { Level = CheckLevel.Error, Message = L("This avatar has {0} VixenMMD components. Keep one; only the first one is used.", all.Length) });
            }
            var mim = VixenMMDShare.FindMakeItMmd(avatar.gameObject);
            if (mim.Count > 0)
            {
                list.Add(new Check
                {
                    Level = CheckLevel.Warning,
                    Message = L("Make It MMD is also set up on this avatar. Both tools would edit the face. Convert its settings into VixenMMD, then you can remove Make It MMD."),
                    FixLabel = L("Convert"),
                    Fix = () => VixenMMDShare.ConvertMakeItMmd(avatar.gameObject, mmd)
                });
            }

            var face = mmd.faceMesh != null ? mmd.faceMesh : avatar.VisemeSkinnedMesh;
            if (face == null)
            {
                list.Add(new Check { Level = CheckLevel.Error, Message = L("No face mesh. Set one here, or set the viseme mesh on the avatar descriptor.") });
                return list;
            }
            if (face.sharedMesh == null || face.sharedMesh.blendShapeCount == 0)
            {
                list.Add(new Check { Level = CheckLevel.Error, Message = L("The face mesh has no blend shapes.") });
                return list;
            }
            if (!face.transform.IsChildOf(avatar.transform))
            {
                list.Add(new Check { Level = CheckLevel.Error, Message = L("The face mesh is not part of this avatar.") });
                return list;
            }
            if (face.name != "Body" && !mmd.renameFaceToBody)
            {
                list.Add(new Check
                {
                    Level = CheckLevel.Warning,
                    Message = L("MMD worlds only animate a mesh called \"Body\". Your face mesh is \"{0}\".", face.name),
                    FixLabel = L("Rename on upload"),
                    Fix = () => { Undo.RecordObject(mmd, "VixenMMD"); mmd.renameFaceToBody = true; }
                });
            }
            if (face.transform.parent != avatar.transform && !mmd.moveFaceToRoot)
            {
                list.Add(new Check
                {
                    Level = CheckLevel.Warning,
                    Message = L("MMD worlds only find the face mesh when it sits right under the avatar root."),
                    FixLabel = L("Move on upload"),
                    Fix = () => { Undo.RecordObject(mmd, "VixenMMD"); mmd.moveFaceToRoot = true; }
                });
            }
            if (mmd.renameFaceToBody && face.name != "Body")
            {
                foreach (Transform child in avatar.transform)
                {
                    if (child.name == "Body" && child != face.transform)
                    {
                        list.Add(new Check { Level = CheckLevel.Info, Message = L("\"Body\" is already taken by another object. On upload it becomes \"Body (Original)\".") });
                        break;
                    }
                }
            }

            var problems = new List<VixenMMDMorphBuilder.Problem>();
            var valid = VixenMMDMorphBuilder.ValidMorphs(face.sharedMesh, mmd.morphs, problems);
            foreach (var p in problems) list.Add(new Check { Level = CheckLevel.Warning, Message = p.Morph + ": " + p.Detail });
            var widths = mmd.morphs.Where(m => m != null && VixenMMDNames.FindNearName(m.name) != null).Select(m => L("{0} (use {1})", m.name, VixenMMDNames.FindNearName(m.name))).ToList();
            if (widths.Count > 0)
            {
                list.Add(new Check { Level = CheckLevel.Warning, Message = L("Dances will not drive these names, because a digit or mark is off: {0}.", string.Join(", ", widths)) });
            }

            var fx = FxController(avatar);
            if (!mmd.forceWriteDefaults && HasWriteDefaultsOff(fx))
            {
                list.Add(new Check
                {
                    Level = CheckLevel.Warning,
                    Message = L("Some FX states have Write Defaults off. MMD worlds such as Nero3D ask for Write Defaults on. Force it on for the upload, then test your toggles."),
                    FixLabel = L("Force Write Defaults"),
                    Fix = () => { Undo.RecordObject(mmd, "VixenMMD"); mmd.forceWriteDefaults = true; }
                });
            }
            var animator = avatar.GetComponent<Animator>();
            var hips = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Hips) : null;
            if (hips != null)
            {
                var loose = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(s => s != null && s.sharedMesh != null && s.bones != null && s.bones.Length > 0 && !FollowsHips(s, hips, avatar.transform))
                    .Select(s => s.name).ToList();
                if (loose.Count > 0)
                {
                    list.Add(new Check
                    {
                        Level = CheckLevel.Warning,
                        Message = L("MMD dances move your hips around the stage. These meshes have a Root Bone that doesn't follow them, so they can disappear mid-dance: {0}. Set each Root Bone to Hips or Chest, then run Auto-Fit Mesh Bounds in the Optimization Suite.", string.Join(", ", loose))
                    });
                }
            }
            if (mmd.worldGuard)
            {
                var names = fx != null ? fx.layers.Select(l => l.name).ToList() : new List<string>();
                var missing = mmd.guardLayers.Where(n => !string.IsNullOrEmpty(n) && !names.Contains(n)).ToList();
                if (mmd.guardLayers.Count == 0)
                    list.Add(new Check { Level = CheckLevel.Warning, Message = L("The MMD world guard is on, but no layers are picked. Pick your hand gesture layers.") });
                else if (missing.Count > 0)
                    list.Add(new Check { Level = CheckLevel.Info, Message = L("Not in your FX controller right now: {0}. That's fine if another tool adds them on upload.", string.Join(", ", missing)) });
                if (HasVrcFuryFeature(avatar.gameObject, "MmdCompatibility"))
                    list.Add(new Check { Level = CheckLevel.Warning, Message = L("VRCFury MMD Compatibility is on this avatar too. Use one of the two, not both.") });
            }

            var built = valid.Select(m => m.name).ToList();
            if (HasVrcFuryFeature(avatar.gameObject, "BlendshapeOptimizer"))
            {
                var blocker = VrcFuryKeepBlocker(face.gameObject);
                if (blocker != null)
                    list.Add(new Check { Level = CheckLevel.Warning, Message = L("VRCFury Blendshape Optimizer will remove your MMD shapes on upload, and VixenMMD can't stop it: {0} Turn the optimizer off for this avatar.", blocker) });
                else
                    list.Add(new Check { Level = CheckLevel.Info, Message = L("VRCFury Blendshape Optimizer is on. VixenMMD keeps every MMD shape on your face safe from it when you upload.") });
            }
            var d4rk = avatar.GetComponents<Component>().FirstOrDefault(c => c != null && c.GetType().Name == "d4rkAvatarOptimizer");
            if (d4rk != null && built.Count > 0)
            {
                var prop = d4rk.GetType().GetProperty("MMDCompatibility", BindingFlags.Public | BindingFlags.Instance);
                bool on = prop == null || (bool)prop.GetValue(d4rk);
                if (!on)
                    list.Add(new Check { Level = CheckLevel.Warning, Message = L("d4rk Avatar Optimizer has MMD Compatibility off, so it can remove your MMD shapes.") });
                var keep = d4rk.GetType().GetField("MMDBlendShapes", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as HashSet<string>;
                if (on && keep != null)
                {
                    var risky = built.Where(n => !keep.Contains(n)).ToList();
                    if (risky.Count > 0)
                        list.Add(new Check { Level = CheckLevel.Info, Message = L("d4rk Avatar Optimizer may remove these, because they are not on its MMD list: {0}.", string.Join(", ", risky)) });
                }
            }
            return list;
        }

        private static bool FollowsHips(SkinnedMeshRenderer s, Transform hips, Transform avatarRoot)
        {
            var anchor = s.rootBone != null ? s.rootBone : s.transform;
            if (anchor.IsChildOf(hips)) return true;
            for (var t = anchor; t != null && t != avatarRoot; t = t.parent)
            {
                if (HasConstraint(t.gameObject, hips)) return true;
                if (MergesIntoArmature(t.gameObject, s.rootBone != null)) return true;
            }
            return false;
        }

        private static bool HasConstraint(GameObject go, Transform hips)
        {
            foreach (var c in go.GetComponents<Component>())
            {
                if (c == null) continue;
                if (c is UnityEngine.Animations.IConstraint ic)
                {
                    for (int i = 0; i < ic.sourceCount; i++)
                    {
                        var src = ic.GetSource(i).sourceTransform;
                        if (src != null && src.IsChildOf(hips)) return true;
                    }
                    continue;
                }
                string n = c.GetType().Name;
                if (n.StartsWith("VRC") && n.EndsWith("Constraint")) return true;
            }
            return false;
        }

        private static bool MergesIntoArmature(GameObject go, bool bonesMerge)
        {
            foreach (var c in go.GetComponents<Component>())
            {
                if (c == null) continue;
                var type = c.GetType();
                if (type.Name == "ModularAvatarBoneProxy") return true;
                if (!bonesMerge) continue;
                if (type.Name == "ModularAvatarMergeArmature") return true;
                if (type.FullName != "VF.Model.VRCFury") continue;
                var p = new SerializedObject(c).FindProperty("content");
                if (p != null && p.propertyType == SerializedPropertyType.ManagedReference && (p.managedReferenceFullTypename ?? "").Contains("ArmatureLink")) return true;
            }
            return false;
        }

        internal static bool HasVrcFuryFeature(GameObject root, string feature)
        {
            foreach (var c in root.GetComponentsInChildren<Component>(true))
            {
                if (c == null || c.GetType().FullName != "VF.Model.VRCFury") continue;
                var p = new SerializedObject(c).FindProperty("content");
                if (p != null && p.propertyType == SerializedPropertyType.ManagedReference && (p.managedReferenceFullTypename ?? "").Contains(feature)) return true;
            }
            return false;
        }

        internal static string VrcFuryKeepBlocker(GameObject faceObject)
        {
            if (faceObject == null) return L("The face mesh is missing.");
            if (faceObject.GetComponent<Animator>() != null) return L("\"{0}\" already has an Animator.", faceObject.name);
            foreach (var c in faceObject.GetComponents<Component>())
            {
                if (c != null && c.GetType().FullName == "VF.Model.VRCFury") return L("\"{0}\" has a VRCFury component, and VRCFury removes Animators from objects that have one.", faceObject.name);
            }
            return null;
        }
    }
}
