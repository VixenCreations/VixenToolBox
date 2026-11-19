#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Validation;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using UnityEngine.Profiling;
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
using ImageMagick;
#endif
using VRC.SDK3.Avatars;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VixenTools.Editor
{
    public static class AvatarSDKValidator
    {
        public enum PCPerformanceRank { Excellent, Good, Medium, Poor }
        public enum ResizeMode { Downscale, Upscale }

        public class Anomaly
        {
            public string Description;
            public UnityEngine.Object ContextObject;
            public System.Action AutoFix;
            public string FixLabel = "OPTIMIZE";
        }

        public class OptimizationTask
        {
            public string ID;
            public string Label;
            public string Description;
            public bool IsSelected = false;
            public System.Action Execute;
            public System.Func<string> ComputeSignature;
        }

        public class PhysicsNode
        {
            public Component Component;
            public string Name;
            public string TypeName;
            public bool Cull = false;
        }

        public class TextureNode
        {
            public Texture Texture;
            public string Name;
            public string AssetPath;
            public int Width;
            public int Height;
            public bool Linear;
            public bool Process;
        }

        public class ValidationReport
        {
            public bool IsPCUploadReady = true;
            public bool IsQuestUploadReady = true;
            public GameObject ArmatureRoot;
            public int BoneCount = 0;
            public float TotalVRAM_MB = 0f;
            public float MeshVRAM_MB = 0f;
            public HashSet<Texture> UniqueTextures = new HashSet<Texture>();

            public int PolyCount = 0;
            public int SkinnedMeshCount = 0;
            public int MaterialSlotCount = 0;
            public int PBComponents = 0;
            public int PBTransforms = 0;
            public int Contacts = 0;
            public int AnimatorsCount = 0;

            public List<PhysicsNode> PhysicsNodes = new List<PhysicsNode>();
            public List<TextureNode> TextureNodes = new List<TextureNode>();
            public string AvatarKey;

            public List<Anomaly> PCErrors = new List<Anomaly>();
            public List<Anomaly> PCPerformanceWarnings = new List<Anomaly>();
            public List<Anomaly> QuestErrors = new List<Anomaly>();
            public List<Anomaly> Warnings = new List<Anomaly>();
            public List<OptimizationTask> OptimizationSuite = new List<OptimizationTask>();

            public string OfficialOverallRating = null;
            public List<Anomaly> OfficialPerfWarnings = new List<Anomaly>();
            public AvatarPerformanceStats OfficialStats;
        }

        public static ValidationReport RunFullSweep(GameObject avatarRoot, int targetTexSize = 1024, PCPerformanceRank targetRank = PCPerformanceRank.Poor, ResizeMode resizeMode = ResizeMode.Downscale, int decimateTarget = 24000)
        {
            var report = new ValidationReport();
            if (avatarRoot == null) return report;

            report.AvatarKey = OptimizationStateCache.GetAvatarKey(avatarRoot);

            void AddTask(OptimizationTask task, bool prepend = false)
            {
                if (task == null) return;
                string sig = task.ComputeSignature != null ? task.ComputeSignature() : task.ID;
                if (OptimizationStateCache.IsHandled(report.AvatarKey, task.ID, sig)) return;
                if (prepend) report.OptimizationSuite.Insert(0, task);
                else report.OptimizationSuite.Add(task);
            }

            var animator = avatarRoot.GetComponent<Animator>();
            HashSet<Transform> protectedTransforms = new HashSet<Transform>();
            HashSet<Transform> humanoidBones = new HashSet<Transform>();

            if (animator != null && animator.isHuman)
            {
                Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (hips != null && hips.parent != null)
                {
                    report.ArmatureRoot = hips.parent.gameObject;
                    report.BoneCount = hips.parent.GetComponentsInChildren<Transform>(true).Length;
                }

                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    Transform b = animator.GetBoneTransform((HumanBodyBones)i);
                    if (b != null)
                    {
                        protectedTransforms.Add(b);
                        humanoidBones.Add(b);
                    }
                }
            }

            var descriptor = avatarRoot.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null)
            {
                report.PCErrors.Add(new Anomaly { Description = "Missing VRCAvatarDescriptor. The SDK will block your upload.", ContextObject = avatarRoot });
                report.IsPCUploadReady = false;
                report.IsQuestUploadReady = false;
            }

            var allTransforms = avatarRoot.GetComponentsInChildren<Transform>(true);
            var renderers = avatarRoot.GetComponentsInChildren<Renderer>(true);
            var skinnedRenderers = avatarRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach (var smr in skinnedRenderers)
            {
                if (smr.bones == null) continue;
                foreach (var bone in smr.bones) if (bone != null) protectedTransforms.Add(bone);
            }

            foreach (var pb in avatarRoot.GetComponentsInChildren<VRCPhysBoneBase>(true))
            {
                Transform root = pb.GetRootTransform();
                if (root != null)
                {
                    protectedTransforms.Add(root);
                    foreach(var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        bool ignored = false;
                        if (pb.ignoreTransforms != null)
                        {
                            foreach (var ig in pb.ignoreTransforms)
                            {
                                if (ig != null && (t == ig || t.IsChildOf(ig))) { ignored = true; break; }
                            }
                        }
                        if (!ignored) protectedTransforms.Add(t);
                    }
                }
            }
            foreach (var col in avatarRoot.GetComponentsInChildren<VRCPhysBoneColliderBase>(true))
            {
                if (col == null) continue;
                protectedTransforms.Add(col.transform);
                if (col.rootTransform != null) protectedTransforms.Add(col.rootTransform);
            }
            foreach (var contact in avatarRoot.GetComponentsInChildren<VRCContactReceiver>(true))
            {
                if (contact == null) continue;
                protectedTransforms.Add(contact.transform);
                if (contact.rootTransform != null) protectedTransforms.Add(contact.rootTransform);
            }
            foreach (var sender in avatarRoot.GetComponentsInChildren<VRCContactSender>(true))
            {
                if (sender == null) continue;
                protectedTransforms.Add(sender.transform);
                if (sender.rootTransform != null) protectedTransforms.Add(sender.rootTransform);
            }
            foreach (var constraint in avatarRoot.GetComponentsInChildren<UnityEngine.Animations.IConstraint>(true))
            {
                Component comp = constraint as Component;
                if (comp != null) protectedTransforms.Add(comp.transform);
            }

            var animatedEnabled = new Dictionary<Transform, List<System.Type>>();
            var clipRoots = new Dictionary<AnimationClip, HashSet<Transform>>();
            HashSet<Transform> referencedTransforms = CollectReferencedTransforms(avatarRoot, descriptor, animatedEnabled, clipRoots);
            protectedTransforms.UnionWith(referencedTransforms);

            var toolRefs = CollectToolReferences(avatarRoot);

            var protectedShapes = CollectProtectedBlendShapeNames(avatarRoot, descriptor, skinnedRenderers, clipRoots);
            var shapeEdits = new List<(SkinnedMeshRenderer smr, HashSet<string> remove, Dictionary<string, float> bake)>();
            int shapeRemoveCount = 0, shapeBakeCount = 0;
            foreach (var smr in skinnedRenderers)
            {
                var mesh = smr != null ? smr.sharedMesh : null;
                if (mesh == null || mesh.blendShapeCount == 0) continue;
                var remove = new HashSet<string>();
                var bake = new Dictionary<string, float>();
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    string shapeName = mesh.GetBlendShapeName(i);
                    if (protectedShapes.Contains(shapeName) || BlendShapeDivider.IsMatch(shapeName)) continue;
                    float weight = smr.GetBlendShapeWeight(i);
                    if (Mathf.Approximately(weight, 0f)) remove.Add(shapeName);
                    else if (mesh.GetBlendShapeFrameCount(i) == 1) bake[shapeName] = weight;
                }
                if (remove.Count + bake.Count == 0) continue;
                shapeEdits.Add((smr, remove, bake));
                shapeRemoveCount += remove.Count;
                shapeBakeCount += bake.Count;
            }
            if (shapeEdits.Count > 0)
            {
                string bakeNote = shapeBakeCount > 0 ? $" {shapeBakeCount} of them are set above zero, so they're baked into the mesh first and the avatar looks the same." : "";
                AddTask(new OptimizationTask
                {
                    ID = "REMOVE_UNUSED_BLENDSHAPES",
                    Label = $"Remove {shapeRemoveCount + shapeBakeCount} Unused Blend Shapes",
                    Description = $"<color=#ff0033>Destructive.</color> Removes blend shapes that nothing uses: no animation, viseme, eyelid, VRCFury, Modular Avatar or VixenMMD setting names them, and no other mesh has one with the same name.{bakeNote} Every blend shape costs memory and skinning time. Your meshes are replaced with patched copies, so keep an eye on the result before you save.",
                    ComputeSignature = () => "blendshapes:" + string.Join("|", shapeEdits
                        .Where(e => e.smr != null && e.smr.sharedMesh != null)
                        .Select(e => AnimationUtility.CalculateTransformPath(e.smr.transform, avatarRoot.transform) + ":" + e.smr.sharedMesh.blendShapeCount)),
                    Execute = () =>
                    {
                        foreach (var e in shapeEdits) if (e.smr != null) RemoveBlendShapes(e.smr, e.remove, e.bake, descriptor);
                    }
                });
            }

            var swapTargets = CollectMaterialSwapTargets(avatarRoot, clipRoots);
            var slotMerges = new List<SkinnedMeshRenderer>();
            int slotsSaved = 0;
            foreach (var smr in skinnedRenderers)
            {
                if (smr == null || smr.sharedMesh == null || swapTargets.Contains(smr)) continue;
                int saved = CountRepeatedSlots(smr);
                if (saved == 0) continue;
                slotMerges.Add(smr);
                slotsSaved += saved;
            }
            if (slotMerges.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "MERGE_REPEATED_SLOTS",
                    Label = $"Merge {slotsSaved} Repeated Material Slots",
                    Description = "Where one mesh uses the same material in more than one slot, joins those parts into one slot. Every slot is a separate draw call and counts toward the material slot limit. Meshes with material swaps are left alone. Your meshes are replaced with patched copies.",
                    ComputeSignature = () => "slots:" + string.Join("|", slotMerges
                        .Where(s => s != null)
                        .Select(s => AnimationUtility.CalculateTransformPath(s.transform, avatarRoot.transform) + ":" + s.sharedMaterials.Length)),
                    Execute = () =>
                    {
                        foreach (var smr in slotMerges) if (smr != null) MergeRepeatedSlots(smr);
                    }
                });
            }

            var listedColliders = new HashSet<VRCPhysBoneColliderBase>();
            foreach (var pb in avatarRoot.GetComponentsInChildren<VRCPhysBoneBase>(true))
            {
                if (pb == null || pb.colliders == null) continue;
                foreach (var c in pb.colliders) if (c != null) listedColliders.Add(c);
            }
            var unusedColliders = avatarRoot.GetComponentsInChildren<VRCPhysBoneColliderBase>(true)
                .Where(c => c != null && !listedColliders.Contains(c) && !toolRefs.Contains(c) && !toolRefs.Contains(c.gameObject))
                .ToList();
            if (unusedColliders.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "REMOVE_UNUSED_COLLIDERS",
                    Label = $"Remove {unusedColliders.Count} Unused PhysBone Colliders",
                    Description = "Deletes PhysBone colliders that no PhysBone on the avatar lists and that no VRCFury or Modular Avatar component points at. They do nothing, but VRChat still counts them.",
                    ComputeSignature = () => "colliders:" + string.Join("|", unusedColliders
                        .Where(c => c != null)
                        .Select(c => AnimationUtility.CalculateTransformPath(c.transform, avatarRoot.transform))),
                    Execute = () =>
                    {
                        foreach (var c in unusedColliders) if (c != null) Undo.DestroyObjectImmediate(c);
                    }
                });
            }

            var unityConstraints = avatarRoot.GetComponentsInChildren<UnityEngine.Animations.IConstraint>(true)
                .OfType<Component>()
                .Where(c => c != null && c.GetType().Namespace == "UnityEngine.Animations")
                .ToList();
            var constraintConverter = FindConstraintConverter();
            if (unityConstraints.Count > 0 && constraintConverter != null)
            {
                AddTask(new OptimizationTask
                {
                    ID = "CONVERT_UNITY_CONSTRAINTS",
                    Label = $"Convert {unityConstraints.Count} Unity Constraints to VRChat Constraints",
                    Description = "Runs the VRChat SDK's own converter on this avatar. VRChat constraints are much cheaper than Unity's, and the game converts Unity ones at load anyway, so converting here makes Play mode match what other players see. The SDK asks you to confirm, and the animations that drive these constraints are updated too.",
                    ComputeSignature = () => "constraints:" + string.Join("|", unityConstraints
                        .Where(c => c != null)
                        .Select(c => AnimationUtility.CalculateTransformPath(c.transform, avatarRoot.transform) + ":" + c.GetType().Name)),
                    Execute = () => constraintConverter.Invoke(null, new object[] { new List<GameObject> { avatarRoot } })
                });
            }

            var motionVectorMeshes = skinnedRenderers.Where(s => s != null && s.skinnedMotionVectors).ToList();
            if (motionVectorMeshes.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "SKINNED_MOTION_VECTORS_OFF",
                    Label = $"Turn Off Motion Vectors on {motionVectorMeshes.Count} Skinned Meshes",
                    Description = "Each skinned mesh with motion vectors on keeps a second copy of its skinned vertices every frame. VRChat doesn't use motion vectors on avatars, so turning them off saves memory and skinning time with no change to how the avatar looks.",
                    ComputeSignature = () => "motionvectors:" + string.Join("|", motionVectorMeshes
                        .Where(s => s != null)
                        .Select(s => AnimationUtility.CalculateTransformPath(s.transform, avatarRoot.transform))),
                    Execute = () =>
                    {
                        foreach (var s in motionVectorMeshes)
                        {
                            if (s == null) continue;
                            Undo.RecordObject(s, "Turn Off Skinned Motion Vectors");
                            s.skinnedMotionVectors = false;
                        }
                    }
                });
            }

            List<Transform> orphanedTransforms = new List<Transform>();
            foreach (var t in allTransforms)
            {
                if (t == avatarRoot.transform || protectedTransforms.Contains(t)) continue;
                if (t.GetComponents<Component>().Length == 1 && t.childCount == 0) orphanedTransforms.Add(t);
            }

            if (orphanedTransforms.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "FLATTEN_HIERARCHY",
                    Label = $"Delete {orphanedTransforms.Count} Unused Empty Objects",
                    Description = "Destroys GameObjects that hold nothing: no components beyond their Transform, no children, no vertex weights. Before anything is listed here it is checked against every animation clip on the avatar (your playable layers, any other Animator, and any clip VRCFury or Modular Avatar brings with it), every object a VRCFury or Modular Avatar component points at, mesh probe anchors and root bones, PhysBone and collider roots, contact senders and receivers, and anything a Unity or VRChat constraint moves or follows. If a path is animated or referenced anywhere, it stays.",
                    ComputeSignature = () => "orphans:" + orphanedTransforms.Count(t => t != null),
                    Execute = () => {
                        int culled = 0;
                        foreach (var t in orphanedTransforms) { if (t != null) { Undo.DestroyObjectImmediate(t.gameObject); culled++; } }
                        Debug.Log($"[VixForge] Removed {culled} unused empty objects.");
                    }
                });
            }

            List<Behaviour> disabledComponents = new List<Behaviour>();
            foreach (var b in avatarRoot.GetComponentsInChildren<Behaviour>(true))
            {
                if (b == null || b.enabled || b is Animator) continue;
                if (b is VRC.SDKBase.IEditorOnly || IsBuildTimeTool(b.GetType())) continue;
                if (IsTurnedOnByAnimation(b, animatedEnabled)) continue;
                disabledComponents.Add(b);
            }

            if (disabledComponents.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "STRIP_DISABLED_COMPS",
                    Label = $"Remove {disabledComponents.Count} Disabled Components",
                    Description = "Deletes components you have left disabled. Makes the avatar a little lighter to load. Components an animation turns on stay, and so do VRCFury, Modular Avatar and other components that never reach the upload.",
                    ComputeSignature = () => "disabled:" + disabledComponents.Count(b => b != null),
                    Execute = () => {
                        int culled = 0;
                        foreach (var b in disabledComponents) { if (b != null) { Undo.DestroyObjectImmediate(b); culled++; } }
                        Debug.Log($"[VixForge] Removed {culled} disabled components.");
                    }
                });
            }

            AddTask(new OptimizationTask
            {
                ID = "OPTIMIZE_BOUNDS",
                Label = $"<color=#00e5ff>Auto-Fit Mesh Bounds</color>",
                Description = "Fits each mesh's bounds to the mesh as it's posed, even on a scaled armature, with a little room for animation. VRChat uses these bounds to decide when to draw a mesh, so bounds that are too big cost frames and bounds that are too small make parts vanish. Also turns off Update When Offscreen.",
                ComputeSignature = () => {
                    var sb = new System.Text.StringBuilder("bounds:");
                    foreach (var s in skinnedRenderers)
                    {
                        if (s == null) continue;
                        Bounds b = s.localBounds;
                        sb.Append(AnimationUtility.CalculateTransformPath(s.transform, avatarRoot.transform))
                          .Append(s.updateWhenOffscreen ? '+' : '-')
                          .Append(Vector3Sig(b.center)).Append('/').Append(Vector3Sig(b.size)).Append(';');
                    }
                    return sb.ToString();
                },
                Execute = () => {
                    int meshesProcessed = 0;

                    const float staticMargin = 1.1f;
                    const float minWorldFloor = 0.01f;

                    foreach (var smr in skinnedRenderers)
                    {
                        if (smr == null) continue;

                        Undo.RecordObject(smr, "Auto-Fit Bounds");
                        smr.updateWhenOffscreen = false;

                        Transform rootBone = GetActualRootBone(smr);
                        Vector3 ls = rootBone.lossyScale;
                        Vector3 localFloor = new Vector3(
                            minWorldFloor / Mathf.Max(1e-5f, Mathf.Abs(ls.x)),
                            minWorldFloor / Mathf.Max(1e-5f, Mathf.Abs(ls.y)),
                            minWorldFloor / Mathf.Max(1e-5f, Mathf.Abs(ls.z)));

                        if (smr.sharedMesh == null)
                        {
                            smr.localBounds = new Bounds(Vector3.zero, localFloor);
                            meshesProcessed++;
                            continue;
                        }

                        if (!TryComputeSkinnedLocalBounds(smr, rootBone, out Bounds fitted))
                        {
                            bool hasBones = false;
                            foreach (var b in smr.bones)
                            {
                                if (b == null) continue;
                                Vector3 localPos = rootBone.InverseTransformPoint(b.position);
                                if (!hasBones) { fitted = new Bounds(localPos, Vector3.zero); hasBones = true; }
                                else fitted.Encapsulate(localPos);
                            }
            
                            if (hasBones) 
                            {
                                fitted.Expand(smr.sharedMesh.bounds.extents.magnitude);
                            } 
                            else 
                            {
                                fitted = TransformBoundsCorners(smr.sharedMesh.bounds, p => rootBone.InverseTransformPoint(smr.transform.TransformPoint(p)));
                            }
                        }
                        fitted.Expand(fitted.size * (staticMargin - 1f));

                        Vector3 size = fitted.size;
                        size.x = Mathf.Max(size.x, localFloor.x);
                        size.y = Mathf.Max(size.y, localFloor.y);
                        size.z = Mathf.Max(size.z, localFloor.z);
                        fitted.size = size;

                        smr.localBounds = fitted;
                        meshesProcessed++;
                    }
                    Debug.Log($"[VixForge] Bounds updated: {meshesProcessed} renderers fitted (true skinned AABB in root-bone space with a small static margin).");
                }
            });

            List<Transform> deepLeafBones = new List<Transform>();
            if (report.ArmatureRoot != null)
            {
                foreach (var t in report.ArmatureRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (t.childCount == 0 && !humanoidBones.Contains(t) && !HasPhysBoneProtection(t, avatarRoot)
                        && !referencedTransforms.Contains(t))
                    {
                        deepLeafBones.Add(t);
                    }
                }
            }

            if (deepLeafBones.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "COLLAPSE_LEAF_BONES",
                    Label = $"<color=#ff0033>Collapse {deepLeafBones.Count} Unused End Bones</color>",
                    Description = "<color=#ff0033>Destructive.</color> Clones each affected mesh, then folds the vertex weights of dead-end bones into their parents. Humanoid bones, anything a PhysBone touches, and any bone that is animated or referenced by a VRCFury or Modular Avatar component, a mesh's root bone or probe anchor, a constraint or a contact are all left alone. Your meshes are replaced with patched copies, so keep an eye on the result before you save.",
                    ComputeSignature = () => "collapse:" + string.Join("|", deepLeafBones
                        .Where(b => b != null)
                        .Select(b => AnimationUtility.CalculateTransformPath(b, avatarRoot.transform))
                        .OrderBy(p => p, System.StringComparer.Ordinal)),
                    Execute = () => {
                        foreach (var smr in skinnedRenderers)
                        {
                            if (smr.bones.Intersect(deepLeafBones).Any())
                            {
                                VixenMeshPatcher.CollapseBonesToParent(smr, deepLeafBones);
                            }
                        }
                    }
                });
            }

            int decimateTargetTris = Mathf.Max(1000, decimateTarget);
            List<SkinnedMeshRenderer> heavyMeshes = skinnedRenderers.Where(s =>
                s.sharedMesh != null &&
                !s.sharedMesh.name.Contains("VixenPatched") &&
                CountTriangles(s.sharedMesh) > decimateTargetTris).ToList();
            if (heavyMeshes.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "WELD_VERTICES",
                    Label = $"<color=#00e5ff>Shrink {heavyMeshes.Count} Heavy Meshes</color>",
                    Description = $"Reduces each heavy mesh toward the slider target of ~{decimateTargetTris:N0} triangles, the way Blender's Decimate does, without flipping faces. <color=#00ff66><b>Keeps UV and normal seams, material borders and open edges, and leaves the eyes, face and hands alone.</b></color> UVs, colours, bone weights and blend shapes carry over. It stops early rather than damage the parts it protects.",
                    ComputeSignature = () => "decimate:" + string.Join("|", heavyMeshes
                        .Where(s => s != null && s.sharedMesh != null)
                        .Select(s => AnimationUtility.CalculateTransformPath(s.transform, avatarRoot.transform) + ":" + CountTriangles(s.sharedMesh))),
                    Execute = () => {
                        int originalTotal = 0;
                        int newTotal = 0;
                        string[] shieldKeywords = { "eye", "visor", "lens", "blush", "face", "mouth", "teeth", "pupil", "iris" };

                        foreach (var smr in heavyMeshes)
                        {
                            originalTotal += smr.sharedMesh.vertexCount;

                            HashSet<int> protectedSlots = new HashSet<int>();
                            var slotMaterials = smr.sharedMaterials;
                            for (int m = 0; m < slotMaterials.Length; m++)
                            {
                                var mat = slotMaterials[m];
                                if (mat != null && shieldKeywords.Any(k => mat.name.ToLower().Contains(k))) protectedSlots.Add(m);
                            }

                            HashSet<int> protectedBoneIndices = new HashSet<int>();
                            if (animator != null && animator.isHuman)
                            {
                                protectedBoneIndices = VixenMeshPatcher.GenerateProtectedBoneIndices(
                                    animator,
                                    smr,
                                    HumanBodyBones.LeftHand,
                                    HumanBodyBones.RightHand
                                );
                            }

                            VixenMeshPatcher.DecimateToTarget(
                                smr,
                                targetTriangles: decimateTargetTris,
                                protectedSubmeshes: protectedSlots,
                                protectedBones: protectedBoneIndices,
                                aggressiveness: 7.0,
                                preserveBorders: true,
                                smoothIterations: 0
                            );

                            newTotal += smr.sharedMesh.vertexCount;
                        }
                        Debug.Log($"[VixForge] QEM Decimation pass: Erased {originalTotal - newTotal} vertices across {heavyMeshes.Count} meshes. Kinematic shielding active.");
                    }
                });
            }

            HashSet<Material> allMaterials = new HashSet<Material>();

            foreach (var r in renderers)
            {
                if (r == null) continue;
                foreach (var m in r.sharedMaterials) if (m != null) allMaterials.Add(m);
            }

            foreach (var anim in avatarRoot.GetComponentsInChildren<Animator>(true))
            {
                if (anim.runtimeAnimatorController != null)
                {
                    var deps = EditorUtility.CollectDependencies(new UnityEngine.Object[] { anim.runtimeAnimatorController });
                    foreach (var dep in deps) if (dep is Material mat) allMaterials.Add(mat);
                }
            }

            foreach (var mono in avatarRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono != null && mono.GetType().Name.Contains("VRCFury"))
                {
                    var deps = EditorUtility.CollectDependencies(new UnityEngine.Object[] { mono });
                    foreach (var dep in deps) if (dep is Material mat) allMaterials.Add(mat);
                }
            }

            long totalBytes = 0;
            foreach (var mat in allMaterials)
            {
                var deps = EditorUtility.CollectDependencies(new UnityEngine.Object[] { mat });
                foreach (var d in deps)
                {
                    if (d is Texture tex && report.UniqueTextures.Add(tex)) totalBytes += TextureBytes(tex);
                }
            }
            report.TotalVRAM_MB = totalBytes / (1024f * 1024f);

            RunOfficialPerformanceScan(avatarRoot, report);

            bool isUpscale = resizeMode == ResizeMode.Upscale;
            foreach (var t in report.UniqueTextures)
            {
                if (!IsProcessableTexture(t, out string texPath)) continue;

                bool defaultProcess = isUpscale
                    ? (t.width < targetTexSize || t.height < targetTexSize)
                    : (t.width > targetTexSize || t.height > targetTexSize);

                report.TextureNodes.Add(new TextureNode
                {
                    Texture = t,
                    Name = t.name,
                    AssetPath = texPath,
                    Width = t.width,
                    Height = t.height,
                    Linear = VixenMagickKit.IsLinearOrNormalData(texPath),
                    Process = defaultProcess
                });
            }
            report.TextureNodes.Sort((a, b) => ((long)b.Width * b.Height).CompareTo((long)a.Width * a.Height));

            List<string> lowQualityAlphaTexPaths = new List<string>();
            foreach (var tex in report.UniqueTextures)
            {
                if (!IsProcessableTexture(tex, out string alphaPath)) continue;
                var alphaImporter = AssetImporter.GetAtPath(alphaPath) as TextureImporter;
                if (alphaImporter == null) continue;
                if (alphaImporter.textureCompression != TextureImporterCompression.Compressed) continue;
                if (alphaImporter.textureType == TextureImporterType.NormalMap) continue;
                if (alphaImporter.GetPlatformTextureSettings("Standalone").overridden) continue;
                if (alphaImporter.alphaSource == TextureImporterAlphaSource.None) continue;

                bool hasAlpha;
                try { hasAlpha = alphaImporter.DoesSourceTextureHaveAlpha(); }
                catch { continue; }

                if (hasAlpha) lowQualityAlphaTexPaths.Add(alphaPath);
            }

            if (lowQualityAlphaTexPaths.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "UPGRADE_TEXTURE_QUALITY",
                    Label = $"<color=#00e5ff>Upgrade {lowQualityAlphaTexPaths.Count} Transparent Texture(s) to Higher Quality</color>",
                    Description = "Better looking, same memory. These already use compression, but the older DXT5 block format. High quality compression gives them BC7 instead, which is also 8 bits per pixel, so VRAM does not move while gradient banding and block artifacts largely go away. Worth it on skin, eyes, and anything with a smooth colour ramp. It's set as a PC override, so Quest builds stay as they are. This only covers textures that actually carry alpha, because an opaque texture sits on 4-bit DXT1 today and moving it to BC7 would double its memory instead of costing nothing.",
                    ComputeSignature = () => "texquality:" + string.Join("|", lowQualityAlphaTexPaths.OrderBy(p => p, System.StringComparer.Ordinal)),
                    Execute = () => {
                        int upgraded = 0;
                        try
                        {
                            AssetDatabase.StartAssetEditing();
                            foreach (string path in lowQualityAlphaTexPaths)
                            {
                                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                                if (imp == null) continue;
                                var pc = imp.GetPlatformTextureSettings("Standalone");
                                pc.overridden = true;
                                pc.format = TextureImporterFormat.BC7;
                                pc.maxTextureSize = imp.maxTextureSize;
                                imp.SetPlatformTextureSettings(pc);
                                imp.SaveAndReimport();
                                upgraded++;
                            }
                        }
                        finally
                        {
                            AssetDatabase.StopAssetEditing();
                        }
                        Debug.Log($"[VixForge] Texture Quality Pass: {upgraded} transparent texture(s) moved from DXT5 to BC7 at the same memory cost.");
                    }
                }, prepend: true);
            }

            List<string> dxtNormalPaths = new List<string>();
            foreach (var tex in report.UniqueTextures)
            {
                if (!IsProcessableTexture(tex, out string normalPath)) continue;
                var normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if (normalImporter == null || normalImporter.textureType != TextureImporterType.NormalMap) continue;
                if (normalImporter.textureCompression != TextureImporterCompression.Compressed) continue;
                if (normalImporter.GetPlatformTextureSettings("Standalone").overridden) continue;
                dxtNormalPaths.Add(normalPath);
            }

            if (dxtNormalPaths.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "NORMALS_BC5",
                    Label = $"<color=#00e5ff>Upgrade {dxtNormalPaths.Count} Normal Map(s) to BC5</color>",
                    Description = "Better looking, same memory. These normal maps use the older DXT5 format. BC5 keeps the two channels a normal map needs at full quality, so bumps get smoother and lose their blocky steps, at the same 8 bits per pixel. It's set as a PC override, so Quest builds stay as they are. Shaders that use Unity's normal map functions read BC5 without any change.",
                    ComputeSignature = () => "bc5:" + string.Join("|", dxtNormalPaths.OrderBy(p => p, System.StringComparer.Ordinal)),
                    Execute = () =>
                    {
                        int upgraded = 0;
                        try
                        {
                            AssetDatabase.StartAssetEditing();
                            foreach (string path in dxtNormalPaths)
                            {
                                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                                if (imp == null) continue;
                                var pc = imp.GetPlatformTextureSettings("Standalone");
                                pc.overridden = true;
                                pc.format = TextureImporterFormat.BC5;
                                pc.maxTextureSize = imp.maxTextureSize;
                                imp.SetPlatformTextureSettings(pc);
                                imp.SaveAndReimport();
                                upgraded++;
                            }
                        }
                        finally
                        {
                            AssetDatabase.StopAssetEditing();
                        }
                        Debug.Log($"[VixForge] Normal Map Pass: {upgraded} normal map(s) moved from DXT5 to BC5 on PC at the same memory cost.");
                    }
                }, prepend: true);
            }

            List<string> uncompressedTexPaths = new List<string>();
            foreach (var tex in report.UniqueTextures)
            {
                if (!IsProcessableTexture(tex, out string uncompressedPath)) continue;
                var texImporter = AssetImporter.GetAtPath(uncompressedPath) as TextureImporter;
                if (texImporter != null && texImporter.textureCompression == TextureImporterCompression.Uncompressed)
                    uncompressedTexPaths.Add(uncompressedPath);
            }

            if (uncompressedTexPaths.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "COMPRESS_TEXTURES",
                    Label = $"<color=#00ff66>Compress {uncompressedTexPaths.Count} Uncompressed Texture(s) to High Quality</color>",
                    Description = "Do this before you shrink anything. An uncompressed RGBA texture costs 32 bits per pixel; high-quality block compression costs 8, so this drops those textures to a quarter of their VRAM. That is the same saving you get from halving the resolution, without the blur and the mushy edges. Unity picks the right block format per texture, so colour maps get BC7 and normal maps stay normal maps. Resolution is the blunt instrument: reach for it after the format is already right.",
                    ComputeSignature = () => "compress:" + string.Join("|", uncompressedTexPaths.OrderBy(p => p, System.StringComparer.Ordinal)),
                    Execute = () => {
                        int converted = 0;
                        try
                        {
                            AssetDatabase.StartAssetEditing();
                            foreach (string path in uncompressedTexPaths)
                            {
                                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                                if (imp == null) continue;
                                imp.textureCompression = TextureImporterCompression.CompressedHQ;
                                imp.SaveAndReimport();
                                converted++;
                            }
                        }
                        finally
                        {
                            AssetDatabase.StopAssetEditing();
                        }
                        Debug.Log($"[VixForge] Texture Format Pass: {converted} texture(s) moved to high-quality block compression. Re-scan to see the new VRAM figure.");
                    }
                }, prepend: true);
            }

            foreach (var tex in report.UniqueTextures)
            {
                if (tex is RenderTexture) continue;

                string texPath = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(texPath) || !texPath.StartsWith("Assets/")) continue;

                TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
                if (importer != null && !importer.streamingMipmaps)
                {
                    report.PCPerformanceWarnings.Add(new Anomaly {
                        Description = $"<b>VRAM Bottleneck:</b> Texture [{tex.name}] lacks Mip Streaming. Causes aggressive VRAM overhead.",
                        ContextObject = tex,
                        FixLabel = "ENABLE STREAMING",
                        AutoFix = () => {
                            var imp = AssetImporter.GetAtPath(texPath) as TextureImporter;
                            if (imp != null) {
                                imp.streamingMipmaps = true;
                                imp.SaveAndReimport();
                            }
                        }
                    });
                }
            }

            foreach (var mat in allMaterials)
            {
                if (mat == null || mat.shader == null) continue;

                if (mat.shader.name == "VixenWear/Latex Ultra")
                {
                    Texture packedMap = mat.GetTexture("_MetallicGlossMap");
                    if (packedMap != null)
                    {
                        string texPath = AssetDatabase.GetAssetPath(packedMap);
                        if (!string.IsNullOrEmpty(texPath))
                        {
                            TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;

                            if (importer != null && importer.sRGBTexture)
                            {
                                report.PCPerformanceWarnings.Add(new Anomaly {
                                    Description = $"<b>Data Corruption:</b> [{mat.name}]'s Packed PBR map ({packedMap.name}) has sRGB enabled. This breaks physical material data.",
                                    ContextObject = packedMap,
                                    FixLabel = "FORCE LINEAR",
                                    AutoFix = () => {
                                        var imp = AssetImporter.GetAtPath(texPath) as TextureImporter;
                                        if (imp != null) {
                                            imp.sRGBTexture = false;
                                            imp.SaveAndReimport();
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }

            var illegalPC = AvatarValidation.FindIllegalComponents(avatarRoot).ToList();
            foreach (var comp in illegalPC)
            {
                report.IsPCUploadReady = false;
                report.PCErrors.Add(new Anomaly {
                    Description = $"Illegal Component [{comp.GetType().Name}] detected on <b>{comp.gameObject.name}</b>",
                    ContextObject = comp.gameObject,
                    AutoFix = () => Undo.DestroyObjectImmediate(comp),
                    FixLabel = "REMOVE"
                });
            }

            CheckMaterialContactTargets(avatarRoot, report);

            foreach (var smr in skinnedRenderers)
            {
                if (smr.sharedMesh != null) report.PolyCount += CountTriangles(smr.sharedMesh);
                report.MaterialSlotCount += smr.sharedMaterials.Length;
            }

            foreach (var mr in avatarRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                var filter = mr.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null) report.PolyCount += CountTriangles(filter.sharedMesh);
                report.MaterialSlotCount += mr.sharedMaterials.Length;
            }

            report.SkinnedMeshCount = skinnedRenderers.Length;
            report.AnimatorsCount = avatarRoot.GetComponentsInChildren<Animator>(true).Length;

            HashSet<Transform> uniquePbTransforms = new HashSet<Transform>();

            foreach (var pb in avatarRoot.GetComponentsInChildren<VRCPhysBoneBase>(true))
            {
                report.PBComponents++;
                report.PhysicsNodes.Add(new PhysicsNode { Component = pb, Name = pb.gameObject.name, TypeName = "PhysBone" });

                Transform root = pb.GetRootTransform();
                if (root != null)
                {
                    foreach(var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        bool ignored = false;
                        if (pb.ignoreTransforms != null)
                        {
                            foreach (var ig in pb.ignoreTransforms)
                            {
                                if (ig != null && (t == ig || t.IsChildOf(ig))) { ignored = true; break; }
                            }
                        }
                        if (!ignored) uniquePbTransforms.Add(t);
                    }
                }
            }

            foreach (var col in avatarRoot.GetComponentsInChildren<VRCPhysBoneColliderBase>(true))
                report.PhysicsNodes.Add(new PhysicsNode { Component = col, Name = col.gameObject.name, TypeName = "Collider" });

            foreach (var con in avatarRoot.GetComponentsInChildren<VRCContactSender>(true))
            {
                report.Contacts++;
                report.PhysicsNodes.Add(new PhysicsNode { Component = con, Name = con.gameObject.name, TypeName = "Contact Sender" });
            }

            foreach (var con in avatarRoot.GetComponentsInChildren<VRCContactReceiver>(true))
            {
                report.Contacts++;
                report.PhysicsNodes.Add(new PhysicsNode { Component = con, Name = con.gameObject.name, TypeName = "Contact Receiver" });
            }

            foreach (var constraint in avatarRoot.GetComponentsInChildren<UnityEngine.Animations.IConstraint>(true))
            {
                var comp = constraint as Component;
                if (comp != null) report.PhysicsNodes.Add(new PhysicsNode { Component = comp, Name = comp.gameObject.name, TypeName = "Constraint" });
            }

            report.PBTransforms = uniquePbTransforms.Count;
            ApplyOfficialCounts(report);

            report.PhysicsNodes.Sort((a, b) => GetDepth(b.Component.transform).CompareTo(GetDepth(a.Component.transform)));

            int maxPb = 32;
            int maxContacts = 32;
            int maxAnimators = 2;

            switch (targetRank)
            {
                case PCPerformanceRank.Excellent: maxPb = 4; maxContacts = 4; maxAnimators = 1; break;
                case PCPerformanceRank.Good: maxPb = 8; maxContacts = 8; maxAnimators = 1; break;
                case PCPerformanceRank.Medium: maxPb = 16; maxContacts = 16; maxAnimators = 2; break;
                case PCPerformanceRank.Poor:
                default: maxPb = 32; maxContacts = 32; maxAnimators = 2; break;
            }

            if (report.PolyCount > 70000) report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Very Poor:</b> Polygons ({report.PolyCount:N0} / 70,000)", ContextObject = avatarRoot });
            if (report.SkinnedMeshCount > 16) report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Very Poor:</b> Skinned Meshes ({report.SkinnedMeshCount} / 16)", ContextObject = avatarRoot });
            if (report.MaterialSlotCount > 32) report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Very Poor:</b> Material Slots ({report.MaterialSlotCount} / 32)", ContextObject = avatarRoot });
            if (report.PBTransforms > 128) report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Very Poor:</b> PB Transforms ({report.PBTransforms} / 128)", ContextObject = avatarRoot });
            if (report.BoneCount > 400) report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Very Poor:</b> Bones ({report.BoneCount} / 400)", ContextObject = avatarRoot });

            if (report.AnimatorsCount > maxAnimators) report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Exceeds Target:</b> Animators ({report.AnimatorsCount} / {maxAnimators})", ContextObject = avatarRoot });
            if (report.PBComponents > maxPb) report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Exceeds Target:</b> PhysBones ({report.PBComponents} / {maxPb})", ContextObject = avatarRoot });
            if (report.Contacts > maxContacts) report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Exceeds Target:</b> Contacts ({report.Contacts} / {maxContacts})", ContextObject = avatarRoot });

            var mobileShaderWhitelist = new HashSet<string>(VRC.SDKBase.Validation.AvatarValidation.ShaderWhiteList);
            var questMeshes = new Dictionary<Shader, List<string>>();
            var questMaterial = new Dictionary<Shader, Material>();
            foreach (var r in renderers)
            {
                foreach (var m in r.sharedMaterials)
                {
                    if (m != null && m.shader != null && !mobileShaderWhitelist.Contains(m.shader.name) && m.shader.name != "Hidden/InternalErrorShader")
                    {
                        if (!questMeshes.TryGetValue(m.shader, out var meshes))
                        {
                            meshes = new List<string>();
                            questMeshes[m.shader] = meshes;
                            questMaterial[m.shader] = m;
                        }
                        if (!meshes.Contains(r.gameObject.name)) meshes.Add(r.gameObject.name);
                    }
                }
            }
            foreach (var kv in questMeshes)
            {
                report.IsQuestUploadReady = false;
                var names = kv.Value;
                string list = names.Count <= 4 ? string.Join(", ", names) : $"{string.Join(", ", names.Take(3))} and {names.Count - 3} more";
                report.QuestErrors.Add(new Anomaly { Description = $"Unsupported Quest Shader: <b>{kv.Key.name}</b> ({(names.Count == 1 ? "Mesh" : "Meshes")}: {list})", ContextObject = questMaterial[kv.Key] });
            }

            foreach (var joint in avatarRoot.GetComponentsInChildren<Joint>(true))
            {
                report.IsQuestUploadReady = false;
                report.QuestErrors.Add(new Anomaly { Description = $"Forbidden Physics Joint [{joint.GetType().Name}] on <b>{joint.gameObject.name}</b>.", ContextObject = joint.gameObject, AutoFix = () => Undo.DestroyObjectImmediate(joint), FixLabel = "REMOVE JOINT" });
            }

            foreach (var cam in avatarRoot.GetComponentsInChildren<Camera>(true))
            {
                report.IsQuestUploadReady = false;
                report.QuestErrors.Add(new Anomaly { Description = $"Camera found on <b>{cam.gameObject.name}</b>. Prohibited on Quest.", ContextObject = cam.gameObject, AutoFix = () => Undo.DestroyObjectImmediate(cam.gameObject), FixLabel = "REMOVE CAMERA" });
            }

            var meshSet = new HashSet<Mesh>();
            foreach (var smr in skinnedRenderers) if (smr != null && smr.sharedMesh != null) meshSet.Add(smr.sharedMesh);
            foreach (var mf in avatarRoot.GetComponentsInChildren<MeshFilter>(true)) if (mf != null && mf.sharedMesh != null) meshSet.Add(mf.sharedMesh);
            long meshBytes = 0;
            foreach (var m in meshSet) meshBytes += MeshBytes(m);
            report.MeshVRAM_MB = meshBytes / (1024f * 1024f);
            if (report.MeshVRAM_MB > 80f)
                report.PCPerformanceWarnings.Add(new Anomaly { Description = $"<b>Very Poor:</b> Mesh memory ({report.MeshVRAM_MB:F1} MB / 80 MB). VRChat's rank doesn't count it, but it uses video memory just like textures. Removing unused blend shapes and shrinking heavy meshes both help.", ContextObject = avatarRoot });

            CheckAnyStateTransitions(descriptor, report);
            CheckDeadAnimationBindings(avatarRoot, descriptor, report);
            CheckGrabPasses(allMaterials, report);
            AddBuildTimeNotice(avatarRoot, report);

            return report;
        }

        private static readonly Dictionary<UnityEngine.Rendering.VertexAttributeFormat, int> VertexAttributeBytes = new Dictionary<UnityEngine.Rendering.VertexAttributeFormat, int>
        {
            { UnityEngine.Rendering.VertexAttributeFormat.UNorm8, 1 }, { UnityEngine.Rendering.VertexAttributeFormat.SNorm8, 1 },
            { UnityEngine.Rendering.VertexAttributeFormat.UInt8, 1 }, { UnityEngine.Rendering.VertexAttributeFormat.SInt8, 1 },
            { UnityEngine.Rendering.VertexAttributeFormat.UNorm16, 2 }, { UnityEngine.Rendering.VertexAttributeFormat.SNorm16, 2 },
            { UnityEngine.Rendering.VertexAttributeFormat.UInt16, 2 }, { UnityEngine.Rendering.VertexAttributeFormat.SInt16, 2 },
            { UnityEngine.Rendering.VertexAttributeFormat.Float16, 2 },
            { UnityEngine.Rendering.VertexAttributeFormat.Float32, 4 }, { UnityEngine.Rendering.VertexAttributeFormat.UInt32, 4 },
            { UnityEngine.Rendering.VertexAttributeFormat.SInt32, 4 },
        };

        private static long MeshBytes(Mesh mesh)
        {
            if (mesh == null) return 0;
            bool skinned = mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.BlendIndices) && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.BlendWeight);
            long perVertex = 0;
            foreach (var a in mesh.GetVertexAttributes())
            {
                int doubled = skinned && (a.attribute == UnityEngine.Rendering.VertexAttribute.Position || a.attribute == UnityEngine.Rendering.VertexAttribute.Normal || a.attribute == UnityEngine.Rendering.VertexAttribute.Tangent) ? 2 : 1;
                perVertex += (VertexAttributeBytes.TryGetValue(a.format, out int size) ? size : 4) * a.dimension * doubled;
            }
            long shapeBytes = 0;
            if (mesh.blendShapeCount > 0)
            {
                var dv = new Vector3[mesh.vertexCount];
                var dn = new Vector3[mesh.vertexCount];
                var dt = new Vector3[mesh.vertexCount];
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    for (int f = 0; f < mesh.GetBlendShapeFrameCount(i); f++)
                    {
                        mesh.GetBlendShapeFrameVertices(i, f, dv, dn, dt);
                        for (int k = 0; k < dv.Length; k++)
                            if (dv[k] != Vector3.zero || dn[k] != Vector3.zero || dt[k] != Vector3.zero) shapeBytes += 40;
                    }
                }
            }
            return perVertex * mesh.vertexCount + shapeBytes;
        }

        private static void CheckAnyStateTransitions(VRCAvatarDescriptor descriptor, ValidationReport report)
        {
            if (descriptor == null) return;
            int count = 0;
            var seen = new HashSet<RuntimeAnimatorController>();
            int Count(UnityEditor.Animations.AnimatorStateMachine sm)
            {
                if (sm == null) return 0;
                int n = sm.anyStateTransitions.Length;
                foreach (var child in sm.stateMachines) n += Count(child.stateMachine);
                return n;
            }
            foreach (var layer in descriptor.baseAnimationLayers.Concat(descriptor.specialAnimationLayers))
            {
                if (!(layer.animatorController is UnityEditor.Animations.AnimatorController controller) || !seen.Add(controller)) continue;
                foreach (var l in controller.layers) count += Count(l.stateMachine);
            }
            if (count <= 100) return;
            report.PCPerformanceWarnings.Add(new Anomaly
            {
                Description = $"<b>{(count > 150 ? "Very Poor" : "Poor")}:</b> Any State transitions ({count} / 100). The animator checks every one of them on every frame. A toggle with two states and normal transitions costs far less.",
                ContextObject = descriptor
            });
        }

        private static void CheckDeadAnimationBindings(GameObject avatarRoot, VRCAvatarDescriptor descriptor, ValidationReport report)
        {
            if (descriptor == null) return;
            Transform root = avatarRoot.transform;
            var seen = new HashSet<RuntimeAnimatorController>();
            foreach (var layer in descriptor.baseAnimationLayers.Concat(descriptor.specialAnimationLayers))
            {
                var controller = layer.animatorController;
                if (controller == null || layer.isDefault || !seen.Add(controller)) continue;
                var deadBindings = new HashSet<string>();
                var examples = new List<string>();
                foreach (var clip in controller.animationClips.Where(c => c != null).Distinct())
                {
                    var bindings = AnimationUtility.GetCurveBindings(clip).Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip));
                    foreach (var b in bindings)
                    {
                        if (string.IsNullOrEmpty(b.path)) continue;
                        var t = root.Find(b.path);
                        bool dead;
                        if (t == null) dead = true;
                        else if (b.type == typeof(GameObject) || b.type == typeof(Transform) || !typeof(Component).IsAssignableFrom(b.type)) dead = false;
                        else if (t.GetComponent(b.type) == null) dead = true;
                        else if (b.type == typeof(SkinnedMeshRenderer) && b.propertyName.StartsWith("blendShape."))
                        {
                            var smr = t.GetComponent<SkinnedMeshRenderer>();
                            dead = smr.sharedMesh == null || smr.sharedMesh.GetBlendShapeIndex(b.propertyName.Substring(11)) < 0;
                        }
                        else dead = false;
                        if (!dead) continue;
                        if (deadBindings.Add(b.path + "|" + b.type.Name + "|" + b.propertyName) && examples.Count < 3 && !examples.Contains(b.path))
                            examples.Add(b.path);
                    }
                }
                if (deadBindings.Count == 0) continue;
                report.PCPerformanceWarnings.Add(new Anomaly
                {
                    Description = $"{deadBindings.Count} animated properties in <b>{controller.name}</b> ({layer.type}) point at objects, components or blend shapes this avatar doesn't have, for example <b>{string.Join("</b>, <b>", examples)}</b>. They do nothing. This usually happens when an avatar is copied and parts are deleted from the copy.",
                    ContextObject = controller
                });
            }
        }

        private static readonly Dictionary<Shader, bool> GrabPassCache = new Dictionary<Shader, bool>();
        private static readonly System.Text.RegularExpressions.Regex GrabPassPattern =
            new System.Text.RegularExpressions.Regex(@"^(?![ \t]*//).*\bGrabPass\s*\{", System.Text.RegularExpressions.RegexOptions.Multiline);

        private static bool ShaderHasGrabPass(Shader shader)
        {
            if (shader == null) return false;
            if (GrabPassCache.TryGetValue(shader, out bool cached)) return cached;
            bool found = false;
            string path = AssetDatabase.GetAssetPath(shader);
            if (!string.IsNullOrEmpty(path))
            {
                string full = path;
                if (path.StartsWith("Packages/"))
                {
                    var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
                    string prefix = info != null ? "Packages/" + info.name + "/" : null;
                    if (prefix != null && path.StartsWith(prefix)) full = Path.Combine(info.resolvedPath, path.Substring(prefix.Length));
                }
                try { if (File.Exists(full)) found = GrabPassPattern.IsMatch(File.ReadAllText(full)); }
                catch (System.Exception) { found = false; }
            }
            GrabPassCache[shader] = found;
            return found;
        }

        private static void CheckGrabPasses(IEnumerable<Material> materials, ValidationReport report)
        {
            var hits = new List<string>();
            UnityEngine.Object context = null;
            foreach (var group in materials.Where(m => m != null && m.shader != null).GroupBy(m => m.shader))
            {
                if (!ShaderHasGrabPass(group.Key)) continue;
                bool unlocked = group.Any(m => m.HasProperty("_ShaderOptimizerEnabled") && m.GetFloat("_ShaderOptimizerEnabled") < 0.5f);
                hits.Add(group.Key.name + (unlocked ? " (unlocked, locking may remove it)" : ""));
                if (context == null) context = group.First();
            }
            if (hits.Count == 0) return;
            string list = hits.Count <= 4 ? string.Join(", ", hits) : $"{string.Join(", ", hits.Take(3))} and {hits.Count - 3} more";
            report.PCPerformanceWarnings.Add(new Anomaly
            {
                Description = $"{hits.Count} shader(s) copy the whole screen with a grab pass: {list}. Every player who can see you pays for each copy, so keep them to effects that need one.",
                ContextObject = context
            });
        }

        private static System.Reflection.MethodInfo FindConstraintConverter()
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType("VRC.SDK3.Avatars.AvatarDynamicsSetup", false);
                var method = type?.GetMethod("ConvertUnityConstraintsAcrossGameObjects",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new[] { typeof(List<GameObject>) }, null);
                if (method != null) return method;
            }
            return null;
        }

        private static HashSet<UnityEngine.Object> CollectToolReferences(GameObject avatarRoot)
        {
            var refs = new HashSet<UnityEngine.Object>();
            foreach (var mono in avatarRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null || IsVRChatType(mono.GetType())) continue;
                var it = new SerializedObject(mono).GetIterator();
                while (it.Next(true))
                    if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null) refs.Add(it.objectReferenceValue);
            }
            return refs;
        }

        private static bool IsVRChatType(System.Type type)
        {
            string ns = type.Namespace ?? "";
            return ns.StartsWith("VRC.") || ns == "VRC";
        }

        private static readonly HashSet<string> MmdStandardNames = new HashSet<string>
        {
            "まばたき", "笑い", "ウィンク", "ウィンク右", "ウィンク２", "ｳｨﾝｸ２右", "なごみ", "はぅ", "びっくり", "じと目", "ｷﾘｯ",
            "はちゅ目", "星目", "はぁと", "瞳小", "瞳縦潰れ", "光下", "恐ろしい子！", "ハイライト消", "映り込み消", "喜び", "わぉ?!",
            "なごみω", "悲しむ", "敵意", "あ", "い", "う", "え", "お", "あ２", "ん", "▲", "∧", "□", "ワ", "ω", "ω□", "にやり",
            "にやり２", "にっこり", "ぺろっ", "てへぺろ", "てへぺろ２", "口角上げ", "口角下げ", "口横広げ", "歯無し上", "歯無し下",
            "真面目", "困る", "にこり", "怒り", "上", "下", "前", "眉頭左", "眉頭右", "照れ", "涙", "青ざめ", "白目", "瞳大",
        };

        private static HashSet<string> _vixenMmdNames;
        private static readonly System.Text.RegularExpressions.Regex BlendShapeDivider = new System.Text.RegularExpressions.Regex(@"^\s*[-=_*#~]{3,}");

        internal static HashSet<string> CollectProtectedBlendShapeNames(GameObject avatarRoot, VRCAvatarDescriptor descriptor, SkinnedMeshRenderer[] skinnedRenderers, Dictionary<AnimationClip, HashSet<Transform>> clipRoots)
        {
            var names = new HashSet<string>();

            void Harvest(UnityEngine.Object obj)
            {
                var it = new SerializedObject(obj).GetIterator();
                while (it.Next(true))
                    if (it.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(it.stringValue)) names.Add(it.stringValue);
            }

            if (descriptor != null)
            {
                Harvest(descriptor);
                var eye = descriptor.customEyeLookSettings;
                var lidMesh = eye.eyelidsSkinnedMesh != null ? eye.eyelidsSkinnedMesh.sharedMesh : null;
                if (lidMesh != null && eye.eyelidsBlendshapes != null)
                    foreach (int i in eye.eyelidsBlendshapes)
                        if (i >= 0 && i < lidMesh.blendShapeCount) names.Add(lidMesh.GetBlendShapeName(i));
            }

            foreach (var mono in avatarRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null || IsVRChatType(mono.GetType())) continue;
                Harvest(mono);
            }
            names.UnionWith(MmdStandardNames);
            names.UnionWith(VixenMmdNames());

            foreach (var pair in clipRoots)
                foreach (var b in AnimationUtility.GetCurveBindings(pair.Key))
                    if (b.type == typeof(SkinnedMeshRenderer) && b.propertyName.StartsWith("blendShape."))
                        names.Add(b.propertyName.Substring(11));

            var seenOnce = new HashSet<string>();
            foreach (var smr in skinnedRenderers)
            {
                var mesh = smr != null ? smr.sharedMesh : null;
                if (mesh == null) continue;
                var local = new HashSet<string>();
                for (int i = 0; i < mesh.blendShapeCount; i++) local.Add(mesh.GetBlendShapeName(i));
                foreach (var n in local) if (!seenOnce.Add(n)) names.Add(n);
            }
            return names;
        }

        private static HashSet<string> VixenMmdNames()
        {
            if (_vixenMmdNames != null) return _vixenMmdNames;
            var names = new HashSet<string>();
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name.IndexOf("VixenMMD", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                System.Type[] types;
                try { types = asm.GetTypes(); }
                catch (System.Reflection.ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                foreach (var type in types)
                {
                    if (type.Name != "VixenMMDRecipes" && type.Name != "VixenMMDNames") continue;
                    var field = type.GetField("All", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (field != null && field.GetValue(null) is System.Collections.IEnumerable all)
                        foreach (var item in all) HarvestStrings(item, names, 0);
                }
            }
            _vixenMmdNames = names;
            return names;
        }

        private static void HarvestStrings(object value, HashSet<string> names, int depth)
        {
            if (value == null || depth > 5) return;
            if (value is string s) { if (s.Length > 0) names.Add(s); return; }
            var type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || value is UnityEngine.Object) return;
            if (value is System.Collections.IEnumerable list)
            {
                foreach (var item in list) HarvestStrings(item, names, depth + 1);
                return;
            }
            foreach (var f in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                HarvestStrings(f.GetValue(value), names, depth + 1);
        }

        private static void RemoveBlendShapes(SkinnedMeshRenderer smr, HashSet<string> remove, Dictionary<string, float> bake, VRCAvatarDescriptor descriptor)
        {
            var source = smr.sharedMesh;
            if (source == null) return;
            var weights = new Dictionary<string, float>();
            for (int i = 0; i < source.blendShapeCount; i++) weights[source.GetBlendShapeName(i)] = smr.GetBlendShapeWeight(i);

            string[] lidNames = null;
            bool lids = descriptor != null && descriptor.customEyeLookSettings.eyelidsSkinnedMesh == smr && descriptor.customEyeLookSettings.eyelidsBlendshapes != null;
            if (lids)
                lidNames = descriptor.customEyeLookSettings.eyelidsBlendshapes
                    .Select(i => i >= 0 && i < source.blendShapeCount ? source.GetBlendShapeName(i) : null).ToArray();

            Undo.RecordObject(smr, "Remove Unused Blend Shapes");
            VixenMeshPatcher.PatchSkinnedMesh(smr, "BlendShapes", (m, r) =>
            {
                int vc = m.vertexCount;
                var verts = m.vertices;
                var norms = m.normals;
                var tans = m.tangents;
                bool hasNormals = norms != null && norms.Length == vc;
                bool hasTangents = tans != null && tans.Length == vc;
                var dv = new Vector3[vc];
                var dn = new Vector3[vc];
                var dt = new Vector3[vc];
                var kept = new List<(string name, float weight, Vector3[] v, Vector3[] n, Vector3[] t)>();

                for (int i = 0; i < m.blendShapeCount; i++)
                {
                    string shapeName = m.GetBlendShapeName(i);
                    if (remove.Contains(shapeName)) continue;
                    if (bake.TryGetValue(shapeName, out float w))
                    {
                        float frameWeight = m.GetBlendShapeFrameWeight(i, 0);
                        if (Mathf.Approximately(frameWeight, 0f)) continue;
                        float f = w / frameWeight;
                        m.GetBlendShapeFrameVertices(i, 0, dv, dn, dt);
                        for (int k = 0; k < vc; k++)
                        {
                            verts[k] += dv[k] * f;
                            if (hasNormals) norms[k] += dn[k] * f;
                            if (hasTangents)
                            {
                                var tk = tans[k];
                                tans[k] = new Vector4(tk.x + dt[k].x * f, tk.y + dt[k].y * f, tk.z + dt[k].z * f, tk.w);
                            }
                        }
                        continue;
                    }
                    for (int fr = 0; fr < m.GetBlendShapeFrameCount(i); fr++)
                    {
                        var fv = new Vector3[vc];
                        var fn = new Vector3[vc];
                        var ft = new Vector3[vc];
                        m.GetBlendShapeFrameVertices(i, fr, fv, fn, ft);
                        kept.Add((shapeName, m.GetBlendShapeFrameWeight(i, fr), fv, fn, ft));
                    }
                }

                if (hasNormals) for (int k = 0; k < vc; k++) norms[k] = norms[k].normalized;
                m.ClearBlendShapes();
                m.vertices = verts;
                if (hasNormals) m.normals = norms;
                if (hasTangents) m.tangents = tans;
                foreach (var frame in kept) m.AddBlendShapeFrame(frame.name, frame.weight, frame.v, frame.n, frame.t);
            }, false);

            var patched = smr.sharedMesh;
            for (int i = 0; i < patched.blendShapeCount; i++)
                smr.SetBlendShapeWeight(i, weights.TryGetValue(patched.GetBlendShapeName(i), out float keep) ? keep : 0f);

            if (lids)
            {
                Undo.RecordObject(descriptor, "Remove Unused Blend Shapes");
                var eye = descriptor.customEyeLookSettings;
                eye.eyelidsBlendshapes = lidNames.Select(n => n == null ? -1 : patched.GetBlendShapeIndex(n)).ToArray();
                descriptor.customEyeLookSettings = eye;
                EditorUtility.SetDirty(descriptor);
            }
        }

        private static HashSet<Renderer> CollectMaterialSwapTargets(GameObject avatarRoot, Dictionary<AnimationClip, HashSet<Transform>> clipRoots)
        {
            var targets = new HashSet<Renderer>();
            foreach (var pair in clipRoots)
            {
                foreach (var b in AnimationUtility.GetObjectReferenceCurveBindings(pair.Key))
                {
                    if (!b.propertyName.StartsWith("m_Materials")) continue;
                    foreach (var root in pair.Value)
                    {
                        var t = string.IsNullOrEmpty(b.path) ? root : root.Find(b.path);
                        if (t != null && t.GetComponent(b.type) is Renderer r) targets.Add(r);
                    }
                }
            }
            foreach (var mono in avatarRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null || IsVRChatType(mono.GetType())) continue;
                bool swaps = mono.GetType().Name.IndexOf("Material", System.StringComparison.OrdinalIgnoreCase) >= 0;
                var found = new List<Renderer>();
                var it = new SerializedObject(mono).GetIterator();
                while (it.Next(true))
                {
                    if (it.name == "materialIndex") swaps = true;
                    if (it.propertyType != SerializedPropertyType.ObjectReference || it.objectReferenceValue == null) continue;
                    if (it.objectReferenceValue is Renderer r) found.Add(r);
                    else if (it.objectReferenceValue is GameObject go) found.AddRange(go.GetComponents<Renderer>());
                }
                if (swaps) targets.UnionWith(found);
            }
            return targets;
        }

        private static int CountRepeatedSlots(SkinnedMeshRenderer smr)
        {
            var mesh = smr.sharedMesh;
            var mats = smr.sharedMaterials;
            if (mesh == null || mats.Length != mesh.subMeshCount || mats.Length < 2) return 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
                if (mesh.GetTopology(i) != MeshTopology.Triangles) return 0;
            var seen = new HashSet<Material>();
            int repeats = 0;
            foreach (var m in mats) if (m != null && !seen.Add(m)) repeats++;
            return repeats;
        }

        private static void MergeRepeatedSlots(SkinnedMeshRenderer smr)
        {
            if (CountRepeatedSlots(smr) == 0) return;
            var mats = smr.sharedMaterials;
            var groups = new List<List<int>>();
            var firstOf = new Dictionary<Material, int>();
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && firstOf.TryGetValue(mats[i], out int g)) { groups[g].Add(i); continue; }
                if (mats[i] != null) firstOf[mats[i]] = groups.Count;
                groups.Add(new List<int> { i });
            }

            Undo.RecordObject(smr, "Merge Repeated Material Slots");
            VixenMeshPatcher.PatchSkinnedMesh(smr, "Slots", (m, r) =>
            {
                var triangles = groups.Select(g => g.SelectMany(i => m.GetTriangles(i)).ToArray()).ToList();
                m.subMeshCount = triangles.Count;
                for (int s = 0; s < triangles.Count; s++) m.SetTriangles(triangles[s], s, false);
            }, false);
            smr.sharedMaterials = groups.Select(g => mats[g[0]]).ToArray();
        }

        private static System.Reflection.MethodInfo _textureStorageSize;
        private static bool _textureStorageLooked;

        private static long TextureBytes(Texture tex)
        {
            if (tex == null) return 0;
            if (!_textureStorageLooked)
            {
                _textureStorageLooked = true;
                var util = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TextureUtil");
                _textureStorageSize = util?.GetMethod("GetStorageMemorySizeLong",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                    null, new[] { typeof(Texture) }, null);
            }
            if (_textureStorageSize != null)
            {
                try { return (long)_textureStorageSize.Invoke(null, new object[] { tex }); }
                catch (System.Exception) { _textureStorageSize = null; }
            }
            return Profiler.GetRuntimeMemorySizeLong(tex);
        }

        private static void ApplyOfficialCounts(ValidationReport report)
        {
            var s = report.OfficialStats;
            if (s == null) return;
            if (s.polyCount.HasValue) report.PolyCount = s.polyCount.Value;
            if (s.skinnedMeshCount.HasValue) report.SkinnedMeshCount = s.skinnedMeshCount.Value;
            if (s.materialCount.HasValue) report.MaterialSlotCount = s.materialCount.Value;
            if (s.boneCount.HasValue) report.BoneCount = s.boneCount.Value;
            if (s.textureMegabytes.HasValue) report.TotalVRAM_MB = (float)s.textureMegabytes.Value;
            if (s.physBone.HasValue)
            {
                report.PBComponents = s.physBone.Value.componentCount;
                report.PBTransforms = s.physBone.Value.transformCount;
            }
        }

        private static void AddBuildTimeNotice(GameObject avatarRoot, ValidationReport report)
        {
            bool fury = false, ma = false;
            foreach (var mono in avatarRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null) continue;
                var type = mono.GetType();
                if (type.Name == "VRCFury") fury = true;
                else if (type.Namespace != null && type.Namespace.StartsWith("nadena.dev.modular_avatar")) ma = true;
                if (fury && ma) break;
            }
            if (!fury && !ma) return;

            string tools = fury && ma ? "VRCFury and Modular Avatar add" : fury ? "VRCFury adds" : "Modular Avatar adds";
            string example = fury ? "SPS sockets and their lights" : "merged armatures and menus";
            var notice = new Anomaly
            {
                Description = $"{tools} parts when the avatar builds, such as {example}, so VRChat will count more than this check sees. Build a test copy and check that for the final numbers.",
                ContextObject = avatarRoot
            };
            if (fury)
            {
                var root = avatarRoot;
                notice.AutoFix = () =>
                {
                    Selection.activeGameObject = root;
                    EditorApplication.ExecuteMenuItem("Tools/VRCFury/Build an Editor Test Copy");
                };
                notice.FixLabel = "BUILD TEST COPY";
            }
            report.PCPerformanceWarnings.Add(notice);
        }

        private static void CheckMaterialContactTargets(GameObject avatarRoot, ValidationReport report)
        {
            Transform root = avatarRoot.transform;
            foreach (var receiver in avatarRoot.GetComponentsInChildren<VRCContactReceiver>(true))
            {
                var so = new SerializedObject(receiver);
                var write = so.FindProperty("writeToMaterials");
                var targets = so.FindProperty("shaderTargetRenderers");
                if (write == null || targets == null || !write.boolValue || !targets.isArray) continue;

                var bad = new List<string>();
                int good = 0, empty = 0;
                for (int i = 0; i < targets.arraySize; i++)
                {
                    var r = targets.GetArrayElementAtIndex(i).objectReferenceValue as Renderer;
                    if (r == null) empty++;
                    else if (!r.transform.IsChildOf(root)) bad.Add($"<b>{r.name}</b> on {r.transform.root.name}");
                    else good++;
                }
                if (empty == 1) bad.Add("an empty slot");
                else if (empty > 1) bad.Add($"{empty} empty slots");
                if (bad.Count == 0) continue;

                var rc = receiver;
                report.IsPCUploadReady = false;
                report.PCErrors.Add(new Anomaly {
                    Description = $"Contact Receiver <b>{receiver.gameObject.name}</b> writes to materials on {string.Join(", ", bad)}. VRChat's website can fail the avatar's security check for this. Point it at this avatar's meshes or remove it.",
                    ContextObject = receiver.gameObject,
                    AutoFix = good > 0 ? (System.Action)(() => RemoveForeignShaderTargets(rc, root)) : () => RemoveReceiver(rc),
                    FixLabel = good > 0 ? "CLEAN UP" : "REMOVE"
                });
            }
        }

        private static void RemoveReceiver(VRCContactReceiver receiver)
        {
            if (receiver == null) return;
            var go = receiver.gameObject;
            Undo.DestroyObjectImmediate(receiver);
            if (go != null && go.transform.childCount == 0 && go.GetComponents<Component>().Length == 1)
                Undo.DestroyObjectImmediate(go);
        }

        private static void RemoveForeignShaderTargets(VRCContactReceiver receiver, Transform root)
        {
            if (receiver == null) return;
            var so = new SerializedObject(receiver);
            var targets = so.FindProperty("shaderTargetRenderers");
            if (targets == null || !targets.isArray) return;
            for (int i = targets.arraySize - 1; i >= 0; i--)
            {
                var r = targets.GetArrayElementAtIndex(i).objectReferenceValue as Renderer;
                if (r != null && r.transform.IsChildOf(root)) continue;
                targets.GetArrayElementAtIndex(i).objectReferenceValue = null;
                targets.DeleteArrayElementAtIndex(i);
            }
            so.ApplyModifiedProperties();
        }

        private const string ModularAvatarAvatarRoot = "$$$AVATAR_ROOT$$$";

        private static bool IsBuildTimeTool(System.Type type)
        {
            if (type == null) return false;
            if (type.Name.Contains("VRCFury")) return true;
            return type.Namespace != null && type.Namespace.StartsWith("nadena.dev.modular_avatar");
        }

        private static bool IsTurnedOnByAnimation(Behaviour behaviour, Dictionary<Transform, List<System.Type>> animatedEnabled)
        {
            if (animatedEnabled == null || !animatedEnabled.TryGetValue(behaviour.transform, out var types)) return false;
            var type = behaviour.GetType();
            foreach (var animatedType in types)
                if (animatedType != null && animatedType.IsAssignableFrom(type)) return true;
            return false;
        }

        private static HashSet<Transform> CollectReferencedTransforms(GameObject avatarRoot, VRCAvatarDescriptor descriptor, Dictionary<Transform, List<System.Type>> animatedEnabled, Dictionary<AnimationClip, HashSet<Transform>> clipRootsOut = null)
        {
            var referenced = new HashSet<Transform>();
            var clipRoots = clipRootsOut ?? new Dictionary<AnimationClip, HashSet<Transform>>();
            Transform avatar = avatarRoot.transform;
            var avatarOnly = new[] { avatar };

            void AddClip(AnimationClip clip, IEnumerable<Transform> roots)
            {
                if (clip == null) return;
                if (!clipRoots.TryGetValue(clip, out var set)) clipRoots[clip] = set = new HashSet<Transform>();
                foreach (var r in roots) if (r != null) set.Add(r);
            }

            void AddMotion(Motion motion, IEnumerable<Transform> roots)
            {
                if (motion is AnimationClip clip) AddClip(clip, roots);
                else if (motion is UnityEditor.Animations.BlendTree tree)
                    foreach (var child in tree.children) AddMotion(child.motion, roots);
            }

            void CollectFrom(RuntimeAnimatorController controller, IEnumerable<Transform> roots)
            {
                if (controller == null) return;
                foreach (var clip in controller.animationClips) AddClip(clip, roots);
            }

            Transform Resolve(string path)
            {
                if (string.IsNullOrEmpty(path)) return null;
                return path == ModularAvatarAvatarRoot ? avatar : avatar.Find(path);
            }

            foreach (var anim in avatarRoot.GetComponentsInChildren<Animator>(true))
                CollectFrom(anim.runtimeAnimatorController, new[] { anim.transform, avatar });

            if (descriptor != null)
            {
                if (descriptor.baseAnimationLayers != null)
                    foreach (var layer in descriptor.baseAnimationLayers) CollectFrom(layer.animatorController, avatarOnly);
                if (descriptor.specialAnimationLayers != null)
                    foreach (var layer in descriptor.specialAnimationLayers) CollectFrom(layer.animatorController, avatarOnly);
            }

            foreach (var mono in avatarRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null || !IsBuildTimeTool(mono.GetType())) continue;

                if (mono.GetType().Name.Contains("VRCFury"))
                {
                    foreach (var dep in EditorUtility.CollectDependencies(new UnityEngine.Object[] { mono }))
                    {
                        if (dep is AnimationClip depClip) AddClip(depClip, avatarOnly);
                        else if (dep is RuntimeAnimatorController depController) CollectFrom(depController, avatarOnly);
                    }
                }

                var roots = new HashSet<Transform> { avatar, mono.transform };
                var motions = new List<UnityEngine.Object>();
                var serialized = new SerializedObject(mono);
                var iterator = serialized.GetIterator();
                while (iterator.Next(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.String)
                    {
                        if (iterator.name != "referencePath") continue;
                        var target = Resolve(iterator.stringValue);
                        if (target == null) continue;
                        referenced.Add(target);
                        if (iterator.propertyPath.IndexOf("relativePathRoot", System.StringComparison.OrdinalIgnoreCase) >= 0) roots.Add(target);
                        continue;
                    }
                    if (iterator.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var obj = iterator.objectReferenceValue;
                    if (obj == null) continue;
                    if (obj is GameObject go)
                    {
                        if (!go.transform.IsChildOf(avatar)) continue;
                        referenced.Add(go.transform);
                        if (iterator.name == "rootObjOverride") roots.Add(go.transform);
                    }
                    else if (obj is Component comp)
                    {
                        if (comp.transform.IsChildOf(avatar)) referenced.Add(comp.transform);
                    }
                    else if (obj is Motion || obj is RuntimeAnimatorController)
                    {
                        motions.Add(obj);
                    }
                }

                foreach (var motion in motions)
                {
                    if (motion is RuntimeAnimatorController controller) CollectFrom(controller, roots);
                    else AddMotion((Motion)motion, roots);
                }
            }

            foreach (var pair in clipRoots)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(pair.Key))
                {
                    bool isEnabled = animatedEnabled != null && binding.propertyName == "m_Enabled";
                    foreach (var root in pair.Value)
                    {
                        Transform bound = string.IsNullOrEmpty(binding.path) ? root : root.Find(binding.path);
                        if (bound == null) continue;
                        if (!string.IsNullOrEmpty(binding.path)) referenced.Add(bound);
                        if (!isEnabled) continue;
                        if (!animatedEnabled.TryGetValue(bound, out var types)) animatedEnabled[bound] = types = new List<System.Type>();
                        if (!types.Contains(binding.type)) types.Add(binding.type);
                    }
                }
                foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(pair.Key))
                {
                    if (string.IsNullOrEmpty(binding.path)) continue;
                    foreach (var root in pair.Value)
                    {
                        Transform bound = root.Find(binding.path);
                        if (bound != null) referenced.Add(bound);
                    }
                }
            }

            foreach (var renderer in avatarRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                if (renderer.probeAnchor != null && renderer.probeAnchor.IsChildOf(avatar)) referenced.Add(renderer.probeAnchor);
                if (renderer is SkinnedMeshRenderer skinned && skinned.rootBone != null && skinned.rootBone.IsChildOf(avatar)) referenced.Add(skinned.rootBone);
            }

            foreach (var sender in avatarRoot.GetComponentsInChildren<VRCContactSender>(true))
            {
                if (sender == null) continue;
                referenced.Add(sender.transform);
                if (sender.rootTransform != null) referenced.Add(sender.rootTransform);
            }

            foreach (var col in avatarRoot.GetComponentsInChildren<VRCPhysBoneColliderBase>(true))
            {
                if (col == null) continue;
                referenced.Add(col.transform);
                if (col.rootTransform != null) referenced.Add(col.rootTransform);
            }

            foreach (var receiver in avatarRoot.GetComponentsInChildren<VRCContactReceiver>(true))
            {
                if (receiver == null) continue;
                referenced.Add(receiver.transform);
                if (receiver.rootTransform != null) referenced.Add(receiver.rootTransform);
            }

            foreach (var constraint in avatarRoot.GetComponentsInChildren<UnityEngine.Animations.IConstraint>(true))
            {
                if (constraint is Component constrained) referenced.Add(constrained.transform);
                for (int i = 0; i < constraint.sourceCount; i++)
                {
                    var source = constraint.GetSource(i);
                    if (source.sourceTransform != null) referenced.Add(source.sourceTransform);
                }
            }

            foreach (var constraint in avatarRoot.GetComponentsInChildren<VRCConstraintBase>(true))
            {
                if (constraint == null) continue;
                referenced.Add(constraint.transform);
                if (constraint.TargetTransform != null) referenced.Add(constraint.TargetTransform);
                for (int i = 0; i < constraint.Sources.Count; i++)
                {
                    var source = constraint.Sources[i];
                    if (source.SourceTransform != null) referenced.Add(source.SourceTransform);
                }
            }

            return referenced;
        }

        private static int GetDepth(Transform t)
        {
            int depth = 0;
            while (t.parent != null) { depth++; t = t.parent; }
            return depth;
        }

        private static bool HasPhysBoneProtection(Transform target, GameObject root)
        {
            var pbs = root.GetComponentsInChildren<VRCPhysBoneBase>(true);
            foreach (var pb in pbs)
            {
                Transform pbRoot = pb.GetRootTransform();
                if (pbRoot != null && target.IsChildOf(pbRoot))
                {
                    bool isIgnored = false;
                    if (pb.ignoreTransforms != null)
                    {
                        foreach (var ignored in pb.ignoreTransforms)
                        {
                            if (ignored != null && (target == ignored || target.IsChildOf(ignored)))
                            {
                                isIgnored = true;
                                break;
                            }
                        }
                    }
                    if (!isIgnored) return true;
                }
            }
            return false;
        }

        private static Bounds TransformBoundsCorners(Bounds source, System.Func<Vector3, Vector3> map)
        {
            Vector3 c = source.center;
            Vector3 ext = source.extents;
            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = c + new Vector3(
                    (i & 1) == 0 ? -ext.x : ext.x,
                    (i & 2) == 0 ? -ext.y : ext.y,
                    (i & 4) == 0 ? -ext.z : ext.z);
                Vector3 p = map(corner);
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }

            return new Bounds((min + max) * 0.5f, max - min);
        }

        private static string Vector3Sig(Vector3 v) => $"{v.x:F3},{v.y:F3},{v.z:F3}";

        private static System.Reflection.PropertyInfo _actualRootBoneProp;

        private static Transform GetActualRootBone(SkinnedMeshRenderer smr)
        {
            if (smr == null) return null;
            if (smr.rootBone != null) return smr.rootBone;

            try
            {
                if (_actualRootBoneProp == null)
                    _actualRootBoneProp = typeof(SkinnedMeshRenderer).GetProperty(
                        "actualRootBone",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                Transform t = _actualRootBoneProp != null ? _actualRootBoneProp.GetValue(smr) as Transform : null;
                if (t != null) return t;
            }
            catch { }

            return smr.transform;
        }

        private static bool TryComputeSkinnedLocalBounds(SkinnedMeshRenderer smr, Transform rootBone, out Bounds bounds)
        {
            bounds = default;
            if (smr == null || smr.sharedMesh == null || rootBone == null) return false;

            bool prevUpdateOffscreen = smr.updateWhenOffscreen;
            smr.updateWhenOffscreen = true;
            bounds = smr.localBounds;
            smr.updateWhenOffscreen = prevUpdateOffscreen;

            return bounds.size.sqrMagnitude > 0f;
        }

        private static int CountTriangles(Mesh mesh)
        {
            if (mesh == null) return 0;
            uint indices = 0;
            for (int s = 0; s < mesh.subMeshCount; s++)
                indices += mesh.GetIndexCount(s);
            return (int)(indices / 3);
        }

        private static void RunOfficialPerformanceScan(GameObject avatarRoot, ValidationReport report)
        {
            if (avatarRoot == null) return;
            try
            {
                bool isMobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
                var perfStats = new AvatarPerformanceStats(isMobile);
                AvatarPerformance.CalculatePerformanceStats(avatarRoot.name, avatarRoot, perfStats, isMobile);
                report.OfficialStats = perfStats;

                var overall = perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall);
                report.OfficialOverallRating = AvatarPerformanceStats.GetPerformanceRatingDisplayName(overall);

                var offenders = BuildOfficialOffenderMap(avatarRoot, report);

                foreach (AvatarPerformanceCategory category in System.Enum.GetValues(typeof(AvatarPerformanceCategory)))
                {
                    if (category == AvatarPerformanceCategory.Overall ||
                        category == AvatarPerformanceCategory.AvatarPerformanceCategoryCount)
                        continue;

                    SDKPerformanceDisplay.GetSDKPerformanceInfoText(
                        perfStats, category, out string statText, out string errorText,
                        out PerformanceInfoDisplayLevel level);

                    if (level != PerformanceInfoDisplayLevel.None && level != PerformanceInfoDisplayLevel.Info)
                    {
                        string msg = string.IsNullOrEmpty(errorText) ? statText : $"{statText}: {errorText}";
                        if (!string.IsNullOrEmpty(msg))
                        {
                            UnityEngine.Object ctx = (offenders.TryGetValue(category, out var offender) && offender != null)
                                ? offender : avatarRoot;
                            if (category == AvatarPerformanceCategory.AABB && ctx is GameObject widest && widest != avatarRoot)
                                msg += $" Largest: <b>{AnimationUtility.CalculateTransformPath(widest.transform, avatarRoot.transform)}</b>.";
                            report.OfficialPerfWarnings.Add(new Anomaly { Description = msg, ContextObject = ctx });
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VixForge] Official VRChat performance scan unavailable: {e.Message}");
            }
        }

        private static Dictionary<AvatarPerformanceCategory, UnityEngine.Object> BuildOfficialOffenderMap(GameObject avatarRoot, ValidationReport report)
        {
            var map = new Dictionary<AvatarPerformanceCategory, UnityEngine.Object>();
            if (avatarRoot == null) return map;

            var skinned = avatarRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var meshRenderers = avatarRoot.GetComponentsInChildren<MeshRenderer>(true);

            Renderer heaviestPoly = null; int heaviestTris = -1;
            Renderer widestBounds = null; float widestExtent = -1f;
            Renderer mostSlots = null; int mostSlotCount = -1;
            SkinnedMeshRenderer smallestSkinned = null; int smallestTris = int.MaxValue;

            void Consider(Renderer r, Mesh mesh)
            {
                if (r == null) return;
                int tris = CountTriangles(mesh);
                if (tris > heaviestTris) { heaviestTris = tris; heaviestPoly = r; }

                float ext = r.bounds.size.magnitude;
                if (ext > widestExtent) { widestExtent = ext; widestBounds = r; }

                int slots = r.sharedMaterials != null ? r.sharedMaterials.Length : 0;
                if (slots > mostSlotCount) { mostSlotCount = slots; mostSlots = r; }
            }

            foreach (var smr in skinned)
            {
                Consider(smr, smr.sharedMesh);
                if (smr.sharedMesh != null)
                {
                    int tris = CountTriangles(smr.sharedMesh);
                    if (tris < smallestTris) { smallestTris = tris; smallestSkinned = smr; }
                }
            }
            foreach (var mr in meshRenderers)
            {
                var mf = mr.GetComponent<MeshFilter>();
                Consider(mr, mf != null ? mf.sharedMesh : null);
            }

            if (heaviestPoly != null) map[AvatarPerformanceCategory.PolyCount] = heaviestPoly.gameObject;
            if (widestBounds != null) map[AvatarPerformanceCategory.AABB] = widestBounds.gameObject;
            if (mostSlots != null) map[AvatarPerformanceCategory.MaterialCount] = mostSlots.gameObject;
            if (smallestSkinned != null) map[AvatarPerformanceCategory.SkinnedMeshCount] = smallestSkinned.gameObject;
            if (meshRenderers.Length > 0) map[AvatarPerformanceCategory.MeshCount] = meshRenderers[0].gameObject;
            if (report.ArmatureRoot != null) map[AvatarPerformanceCategory.BoneCount] = report.ArmatureRoot;

            foreach (var an in avatarRoot.GetComponentsInChildren<Animator>(true))
            {
                if (an != null && an.gameObject != avatarRoot) { map[AvatarPerformanceCategory.AnimatorCount] = an.gameObject; break; }
            }

            VRCPhysBoneBase biggestPb = null; int biggestChain = -1;
            foreach (var pb in avatarRoot.GetComponentsInChildren<VRCPhysBoneBase>(true))
            {
                Transform root = pb.GetRootTransform();
                int chain = root != null ? root.GetComponentsInChildren<Transform>(true).Length : 0;
                if (chain > biggestChain) { biggestChain = chain; biggestPb = pb; }
            }
            if (biggestPb != null)
            {
                map[AvatarPerformanceCategory.PhysBoneComponentCount] = biggestPb.gameObject;
                map[AvatarPerformanceCategory.PhysBoneTransformCount] = biggestPb.gameObject;
            }

            AddFirstComponent<VRCPhysBoneColliderBase>(map, avatarRoot, AvatarPerformanceCategory.PhysBoneColliderCount);

            var contact = (Component)avatarRoot.GetComponentInChildren<VRCContactReceiver>(true)
                          ?? avatarRoot.GetComponentInChildren<VRCContactSender>(true);
            if (contact != null) map[AvatarPerformanceCategory.ContactCount] = contact.gameObject;

            AddFirstComponent<ParticleSystem>(map, avatarRoot, AvatarPerformanceCategory.ParticleSystemCount);
            AddFirstComponent<TrailRenderer>(map, avatarRoot, AvatarPerformanceCategory.TrailRendererCount);
            AddFirstComponent<LineRenderer>(map, avatarRoot, AvatarPerformanceCategory.LineRendererCount);
            AddFirstComponent<Light>(map, avatarRoot, AvatarPerformanceCategory.LightCount);
            AddFirstComponent<Cloth>(map, avatarRoot, AvatarPerformanceCategory.ClothCount);
            AddFirstComponent<AudioSource>(map, avatarRoot, AvatarPerformanceCategory.AudioSourceCount);
            AddFirstComponent<Rigidbody>(map, avatarRoot, AvatarPerformanceCategory.PhysicsRigidbodyCount);
            AddFirstComponent<Collider>(map, avatarRoot, AvatarPerformanceCategory.PhysicsColliderCount);

            Texture heaviestTex = null; long heaviestBytes = -1;
            foreach (var tex in report.UniqueTextures)
            {
                if (tex == null) continue;
                long bytes = TextureBytes(tex);
                if (bytes > heaviestBytes) { heaviestBytes = bytes; heaviestTex = tex; }
            }
            if (heaviestTex != null) map[AvatarPerformanceCategory.TextureMegabytes] = heaviestTex;

            return map;
        }

        private static void AddFirstComponent<T>(Dictionary<AvatarPerformanceCategory, UnityEngine.Object> map, GameObject root, AvatarPerformanceCategory cat) where T : Component
        {
            var c = root.GetComponentInChildren<T>(true);
            if (c != null) map[cat] = c.gameObject;
        }

        private static bool IsProcessableTexture(Texture tex, out string assetPath)
        {
            assetPath = null;
            if (tex == null || tex is RenderTexture) return false;
            assetPath = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets/")) return false;
            if (VixenMagickKit.IsProtectedAsset(assetPath)) return false;
            return true;
        }

        public static void ProcessTexturesWithMagick(IEnumerable<Texture> textures, int targetSize, ResizeMode mode)
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET
            bool downscale = mode == ResizeMode.Downscale;
            string activeVerb = downscale ? "Downscaling" : "Upscaling";

            var jobPaths = new List<string>();
            var jobLinear = new List<bool>();
            foreach (var tex in textures)
            {
                if (tex == null) continue;
                if (!IsProcessableTexture(tex, out string path)) continue;
                jobPaths.Add(path);
                jobLinear.Add(VixenMagickKit.IsLinearOrNormalData(path));
            }

            int total = jobPaths.Count;
            int count = 0;
            int done = 0;
            bool canceled = false;

            int workers = Mathf.Clamp(total, 1, Mathf.Min(8, Mathf.Max(1, System.Environment.ProcessorCount)));
            ulong prevThreads = 0;
            bool threadsChanged = false;
            try
            {
                prevThreads = ResourceLimits.Thread;
                ResourceLimits.Thread = (ulong)Mathf.Max(1, System.Environment.ProcessorCount / workers);
                threadsChanged = true;
            }
            catch { }

            try
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = workers };
                for (int start = 0; start < total; start += workers)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            $"VixForge: {activeVerb} Textures (x{workers})",
                            $"({done}/{total})",
                            total == 0 ? 1f : (float)done / total))
                    {
                        canceled = true;
                        break;
                    }

                    int end = Mathf.Min(start + workers, total);
                    Parallel.For(start, end, options, i =>
                    {
                        if (VixenMagickKit.ProcessTextureFile(jobPaths[i], (uint)targetSize, jobLinear[i], downscale))
                            Interlocked.Increment(ref count);
                        Interlocked.Increment(ref done);
                    });
                }
            }
            finally
            {
                if (threadsChanged) { try { ResourceLimits.Thread = prevThreads; } catch { } }
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            string verb = downscale ? "compressed" : "upscaled";
            string tail = canceled ? " (canceled)" : "";
            Debug.Log($"[VixForge] Optimization Engine: {count} textures {verb}{tail} (parallel x{workers}).");
#else
            Debug.LogWarning("[VixForge] " + VixenMagickKit.UnavailableMessage);
#endif
        }
    }

    internal static class OptimizationStateCache
    {
        private const int VERSION = 1;
        private const string Dir = "Assets/VixenTools/Asset Database/Optimization Suite";
        private const string CachePath = Dir + "/OptimizationState.json";

        [System.Serializable]
        private class Record
        {
            public string key;
            public string signature;
            public int version;
        }

        [System.Serializable]
        private class Cache
        {
            public List<Record> records = new List<Record>();
        }

        private static Cache _cache;
        private static Dictionary<string, Record> _map;

        public static string GetAvatarKey(GameObject avatarRoot)
        {
            if (avatarRoot == null) return "null";
            try { return GlobalObjectId.GetGlobalObjectIdSlow(avatarRoot).ToString(); }
            catch { return avatarRoot.name; }
        }

        private static void EnsureLoaded()
        {
            if (_map != null) return;
            _map = new Dictionary<string, Record>();
            _cache = new Cache();

            string abs = System.IO.Path.GetFullPath(CachePath);
            if (System.IO.File.Exists(abs))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(abs);
                    _cache = JsonUtility.FromJson<Cache>(json) ?? new Cache();
                }
                catch { _cache = new Cache(); }
            }

            if (_cache.records == null) _cache.records = new List<Record>();
            foreach (var r in _cache.records)
                if (r != null && !string.IsNullOrEmpty(r.key)) _map[r.key] = r;
        }

        private static string MakeKey(string avatarKey, string taskId) => avatarKey + "|" + taskId;

        public static bool IsHandled(string avatarKey, string taskId, string signature)
        {
            EnsureLoaded();
            if (!_map.TryGetValue(MakeKey(avatarKey, taskId), out var rec)) return false;
            return rec.version == VERSION && string.Equals(rec.signature, signature, System.StringComparison.Ordinal);
        }

        public static void RecordHandled(string avatarKey, string taskId, string signature)
        {
            EnsureLoaded();
            string key = MakeKey(avatarKey, taskId);
            if (!_map.TryGetValue(key, out var rec))
            {
                rec = new Record { key = key };
                _map[key] = rec;
            }
            rec.signature = signature;
            rec.version = VERSION;
            Save();
        }

        private static void Save()
        {
            _cache.records = _map.Values.ToList();

            string abs = System.IO.Path.GetFullPath(CachePath);
            string dir = System.IO.Path.GetDirectoryName(abs);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

            System.IO.File.WriteAllText(abs, JsonUtility.ToJson(_cache, true));
            AssetDatabase.ImportAsset(CachePath, ImportAssetOptions.ForceUpdate);
        }
    }

    public class VixenAvatarValidator : EditorWindow
    {
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenAvatarValidatorStyles.uss";

        private VisualElement _resultsContainer;
        private ObjectField _targetField;
        private PopupField<int> _sizePopup;
        private static readonly List<int> SizePresets = new List<int> { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };
        private EnumField _rankEnum;
        private AvatarSDKValidator.PCPerformanceRank _targetRank = AvatarSDKValidator.PCPerformanceRank.Poor;
        private EnumField _modeEnum;
        private AvatarSDKValidator.ResizeMode _resizeMode = AvatarSDKValidator.ResizeMode.Downscale;
        private SliderInt _decimateSlider;
        private int _decimateTarget = 24000;
        private AvatarSDKValidator.ValidationReport _lastReport;

        [MenuItem("VixenTools/Avatars/Optimization Suite", priority = 40)]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenAvatarValidator>("Optimization Suite");
            window.minSize = new Vector2(480, 650);
            window.Show();
        }

        private double _nextScanTime = 0;
        private bool _scanQueued = false;

        private void Update()
        {
            if (_scanQueued && EditorApplication.timeSinceStartup > _nextScanTime)
            {
                _scanQueued = false;
                if (_targetField != null && _targetField.value != null)
                {
                    ExecuteDeepScan();
                }
            }
        }

        private void QueueDeepScan()
        {
            if (_lastReport != null && _targetField != null && _targetField.value != null)
            {
                _scanQueued = true;
                _nextScanTime = EditorApplication.timeSinceStartup + 0.5;
            }
        }

        private void OnHierarchyChange() => QueueDeepScan();

        private void OnProjectChange() => QueueDeepScan();

        private void OnSelectionChange()
        {
            var selected = Selection.activeGameObject;
            if (selected != null && selected.GetComponent<VRCAvatarDescriptor>() != null)
            {
                if (_targetField.value != selected)
                {
                    _targetField.value = selected;
                    ExecuteDeepScan();
                }
            }
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.name = "hub-root";
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath));

            var header = new VisualElement { name = "hub-header", style = { minHeight = 80, justifyContent = Justify.Center, alignItems = Align.Center } };
            var titleLabel = new Label("<color=#00e5ff>AVATAR</color> <color=#ff00aa>OPTIMIZATION</color> SUITE") { enableRichText = true };
            titleLabel.AddToClassList("hub-header-title");
            header.Add(titleLabel);
            root.Add(header);

            var scroll = new ScrollView() { style = { flexGrow = 1, paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };

            var configPanel = CreateCyberPanel("Settings", "#00e5ff");
            _targetField = new ObjectField("Avatar Root") { objectType = typeof(GameObject), allowSceneObjects = true };
            configPanel.Add(_targetField);

            _sizePopup = new PopupField<int>("Max Texture Size (px)", SizePresets, Mathf.Max(0, SizePresets.IndexOf(1024)));
            configPanel.Add(_sizePopup);

            _modeEnum = new EnumField("Resize Mode", _resizeMode);
            _modeEnum.RegisterValueChangedCallback(e => _resizeMode = (AvatarSDKValidator.ResizeMode)e.newValue);
            configPanel.Add(_modeEnum);

            _rankEnum = new EnumField("Target PC Performance Rank", _targetRank);
            _rankEnum.RegisterValueChangedCallback(e => _targetRank = (AvatarSDKValidator.PCPerformanceRank)e.newValue);
            configPanel.Add(_rankEnum);

            _decimateSlider = new SliderInt("Triangle Target (per mesh)", 2000, 70000) { value = _decimateTarget, showInputField = true };
            _decimateSlider.RegisterValueChangedCallback(e => _decimateTarget = e.newValue);
            configPanel.Add(_decimateSlider);

            var decimateHint = new Label("Higher = softer / more detail. Lower = smaller / more aggressive. Only meshes above this triangle count are decimated down to it.");
            decimateHint.AddToClassList("md-p");
            configPanel.Add(decimateHint);

            var scanBtn = new Button(ExecuteDeepScan) { text = "SCAN AVATAR" };
            scanBtn.AddToClassList("cyber-action-btn");
            scanBtn.AddToClassList("cyan-btn");
            configPanel.Add(scanBtn);
            scroll.Add(configPanel);

            _resultsContainer = new VisualElement();
            scroll.Add(_resultsContainer);
            root.Add(scroll);
        }

        private VisualElement BuildUploadPanel(GameObject avatar)
        {
            var panel = CreateCyberPanel("On Upload", "#00ff66");
            var intro = new Label("These run on the copy that VRChat uploads, after VRCFury and Modular Avatar have built it. The avatar in your scene and your assets stay as they are. Each one starts off and is saved for this avatar in this project.") { enableRichText = true };
            intro.AddToClassList("opt-task-desc");
            panel.Add(intro);

            var settings = VixenUploadSettings.instance;
            var entry = settings.Find(avatar, false);

            void Row(string label, string description, System.Func<VixenUploadSettings.Entry, bool> get, System.Action<VixenUploadSettings.Entry, bool> set)
            {
                var row = new VisualElement();
                row.AddToClassList("opt-task-row");
                var toggle = new Toggle { value = entry != null && get(entry) };
                toggle.RegisterValueChangedCallback(e =>
                {
                    if (entry == null) entry = settings.Find(avatar, true);
                    set(entry, e.newValue);
                    settings.SaveNow();
                });
                row.Add(toggle);
                var text = new Label($"<b>{label}</b>\n<color=#aaaaaa>{description}</color>") { enableRichText = true };
                text.AddToClassList("opt-task-desc");
                row.Add(text);
                panel.Add(row);
            }

            Row("Remove Unused Blend Shapes", "The same check as the fix above, on the upload copy. It also catches shapes that only VRCFury or Modular Avatar needed before they built.",
                e => e.RemoveUnusedBlendShapes, (e, v) => e.RemoveUnusedBlendShapes = v);
            Row("Merge Repeated Material Slots", "Joins slots that use the same material, including ones that VRCFury or Modular Avatar created.",
                e => e.MergeRepeatedSlots, (e, v) => e.MergeRepeatedSlots = v);
            Row("Remove Unused PhysBone Colliders", "Deletes colliders that no PhysBone lists once everything is built.",
                e => e.RemoveUnusedColliders, (e, v) => e.RemoveUnusedColliders = v);
            Row("Turn Off Skinned Motion Vectors", "Saves memory and skinning time. VRChat doesn't use them on avatars.",
                e => e.MotionVectorsOff, (e, v) => e.MotionVectorsOff = v);
            Row("Ring Finger Colliders on Feet", "Moves your ring finger contacts to your feet, so your feet can push other players' PhysBones. Your ring fingers stop touching PhysBones.",
                e => e.RingFingerOnFeet, (e, v) => e.RingFingerOnFeet = v);
            Row("Turn Off PhysBones With Their Mesh", "When only one mesh moves with a PhysBone, the PhysBone turns off whenever a toggle hides that mesh, so hidden hair or tails cost nothing. PhysBones with a parameter, or with anything else on their bones, are left alone.",
                e => e.DisablePhysBonesWithMesh, (e, v) => e.DisablePhysBonesWithMesh = v);
            Row("Merge Same-Ratio Blend Shapes", "Blend shapes that every animation moves together in the same ratio become one shape, with the same look. Visemes, eyelids, MMD shapes and shapes other tools name are kept.",
                e => e.MergeSameRatioBlendShapes, (e, v) => e.MergeSameRatioBlendShapes = v);
            Row("Remove Unused Animator Parameters", "Removes parameters that nothing in a controller uses. Anything in your expression parameters stays.",
                e => e.RemoveUnusedParameters, (e, v) => e.RemoveUnusedParameters = v);
            return panel;
        }

        private void ExecuteDeepScan()
        {
            _resultsContainer.Clear();
            var target = _targetField.value as GameObject;

            _lastReport = AvatarSDKValidator.RunFullSweep(target, _sizePopup.value, _targetRank, _resizeMode, _decimateTarget);

            var archPanel = CreateCyberPanel("What Is On This Avatar", "#00e5ff");
            if (_lastReport.ArmatureRoot != null)
            {
                archPanel.Add(CreateRow($"<b>Armature Root:</b> {_lastReport.ArmatureRoot.name}", _lastReport.ArmatureRoot, "#00e5ff"));
                archPanel.Add(CreateRow($"<b>Bone Density:</b> {_lastReport.BoneCount} Transforms", null, "#00e5ff"));

                string vramHex = _lastReport.TotalVRAM_MB > 150f ? "#ff0033" : (_lastReport.TotalVRAM_MB > 40f ? "#ffaa00" : "#00e5ff");
                archPanel.Add(CreateRow($"<b>Hardware VRAM Footprint:</b> {_lastReport.TotalVRAM_MB:F2} MB ({_lastReport.UniqueTextures.Count} Textures)", null, vramHex));
                string meshHex = _lastReport.MeshVRAM_MB > 80f ? "#ff0033" : (_lastReport.MeshVRAM_MB > 35f ? "#ffaa00" : "#00e5ff");
                archPanel.Add(CreateRow($"<b>Mesh Memory:</b> {_lastReport.MeshVRAM_MB:F2} MB (not part of VRChat's rank)", null, meshHex));
            }
            _resultsContainer.Add(archPanel);

            if (_lastReport.OfficialOverallRating != null)
            {
                var perfPanel = CreateCyberPanel("VRChat Official Performance", "#00ff66");
                perfPanel.Add(CreateRow($"<b>Overall Rating:</b> {_lastReport.OfficialOverallRating}", null, "#00ff66"));
                if (_lastReport.OfficialPerfWarnings.Count == 0)
                {
                    perfPanel.Add(CreateRow("No category warnings from the VRChat SDK.", null, "#00e5ff"));
                }
                else
                {
                    foreach (var w in _lastReport.OfficialPerfWarnings)
                        perfPanel.Add(CreateRow(w.Description, w.ContextObject, "#ffaa00"));
                }
                _resultsContainer.Add(perfPanel);
            }

            int maxPb = 32; int maxContacts = 32; int maxAnimators = 2;
            switch (_targetRank) {
                case AvatarSDKValidator.PCPerformanceRank.Excellent: maxPb=4; maxContacts=4; maxAnimators=1; break;
                case AvatarSDKValidator.PCPerformanceRank.Good: maxPb=8; maxContacts=8; maxAnimators=1; break;
                case AvatarSDKValidator.PCPerformanceRank.Medium: maxPb=16; maxContacts=16; maxAnimators=2; break;
                case AvatarSDKValidator.PCPerformanceRank.Poor: maxPb=32; maxContacts=32; maxAnimators=2; break;
            }

            var statsPanel = CreateCyberPanel("Memory & Limits", "#00ff66");

            string triColor = _lastReport.PolyCount > 70000 ? "#ff0033" : "#00ff66";
            string smrColor = _lastReport.SkinnedMeshCount > 16 ? "#ff0033" : "#00ff66";
            string matColor = _lastReport.MaterialSlotCount > 32 ? "#ff0033" : "#00ff66";
            string pbTColor = _lastReport.PBTransforms > 128 ? "#ff0033" : "#00ff66";
            string pbCColor = _lastReport.PBComponents > maxPb ? "#ff0033" : "#00ff66";
            string conColor = _lastReport.Contacts > maxContacts ? "#ff0033" : "#00ff66";
            string aniColor = _lastReport.AnimatorsCount > maxAnimators ? "#ff0033" : "#00ff66";

            string statsText =
                $"<b><color=#00e5ff>■</color> STATIC VRC LIMITS (VERY POOR):</b>\n" +
                $"  • Total Triangles: <color={triColor}><b>{_lastReport.PolyCount:N0}</b></color> / 70,000\n" +
                $"  • Skinned Meshes: <color={smrColor}><b>{_lastReport.SkinnedMeshCount}</b></color> / 16\n" +
                $"  • Material Slots: <color={matColor}><b>{_lastReport.MaterialSlotCount}</b></color> / 32\n" +
                $"  • PhysBone Transforms: <color={pbTColor}><b>{_lastReport.PBTransforms}</b></color> / 128\n\n" +
                $"<b><color=#ffaa00>■</color> DYNAMIC LIMITS ({_targetRank.ToString().ToUpper()} RANK):</b>\n" +
                $"  • PhysBone Components: <color={pbCColor}><b>{_lastReport.PBComponents}</b></color> / {maxPb}\n" +
                $"  • Contacts: <color={conColor}><b>{_lastReport.Contacts}</b></color> / {maxContacts}\n" +
                $"  • Animators: <color={aniColor}><b>{_lastReport.AnimatorsCount}</b></color> / {maxAnimators}";

            var statsLabel = new Label(statsText) { enableRichText = true };
            statsLabel.AddToClassList("md-p");
            statsPanel.Add(statsLabel);
            _resultsContainer.Add(statsPanel);

            if (_lastReport.PhysicsNodes.Count > 0)
            {
                var physPanel = CreateCyberPanel("Physics Components", "#ffaa00");

                var info = new Label("Pick the physics components to remove so the avatar fits your target rank. Sorted by depth (Leaf nodes first).");
                info.AddToClassList("md-p");
                physPanel.Add(info);

                var controlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10, marginBottom = 10 } };

                var physCountLabel = new Label($"Queued for removal: <color=#ff0033><b>0</b></color> / {_lastReport.PhysicsNodes.Count}") { enableRichText = true, style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
                controlRow.Add(physCountLabel);

                List<Toggle> nodeToggles = new List<Toggle>();

                var btnSelectAll = new Button(() => {
                    _lastReport.PhysicsNodes.ForEach(n => n.Cull = true);
                    foreach (var t in nodeToggles) t.SetValueWithoutNotify(true);
                    physCountLabel.text = $"Queued for removal: <color=#ff0033><b>{_lastReport.PhysicsNodes.Count}</b></color> / {_lastReport.PhysicsNodes.Count}";
                }) { text = "Select All" };
                btnSelectAll.AddToClassList("data-tag-btn"); btnSelectAll.AddToClassList("data-tag-destructive");

                var btnDeselectAll = new Button(() => {
                    _lastReport.PhysicsNodes.ForEach(n => n.Cull = false);
                    foreach (var t in nodeToggles) t.SetValueWithoutNotify(false);
                    physCountLabel.text = $"Queued for removal: <color=#ff0033><b>0</b></color> / {_lastReport.PhysicsNodes.Count}";
                }) { text = "Deselect All" };
                btnDeselectAll.AddToClassList("data-tag-btn"); btnDeselectAll.AddToClassList("data-tag-optimize");

                controlRow.Add(btnSelectAll);
                controlRow.Add(btnDeselectAll);
                physPanel.Add(controlRow);

                var physScroll = new ScrollView(ScrollViewMode.Vertical) { style = { maxHeight = 250, backgroundColor = new Color(0,0,0,0.2f), paddingBottom = 5, paddingTop = 5, borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4 } };

                foreach(var node in _lastReport.PhysicsNodes)
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, paddingLeft = 5, paddingRight = 5, paddingTop = 2, paddingBottom = 2 } };
                    row.AddToClassList("md-row");

                    var toggle = new Toggle { value = node.Cull };
                    nodeToggles.Add(toggle);

                    toggle.RegisterValueChangedCallback(e => {
                        node.Cull = e.newValue;
                        int culledCount = _lastReport.PhysicsNodes.Count(n => n.Cull);
                        physCountLabel.text = $"Queued for removal: <color=#ff0033><b>{culledCount}</b></color> / {_lastReport.PhysicsNodes.Count}";
                    });
                    row.Add(toggle);

                    var lbl = new Label($"<b>{node.Name}</b> <i><color=#aaaaaa>({node.TypeName})</color></i>") { enableRichText = true, style = { flexGrow = 1 } };
                    row.Add(lbl);

                    var locateBtn = new Button(() => { EditorGUIUtility.PingObject(node.Component); Selection.activeObject = node.Component; }) { text = "LOCATE" };
                    locateBtn.AddToClassList("data-tag-btn"); locateBtn.AddToClassList("data-tag-locate");
                    row.Add(locateBtn);

                    physScroll.Add(row);
                }
                physPanel.Add(physScroll);

                var executePhysBtn = new Button(() => {
                    int culled = 0;
                    foreach(var node in _lastReport.PhysicsNodes) {
                        if (node.Cull && node.Component != null) {
                            Undo.DestroyObjectImmediate(node.Component);
                            culled++;
                        }
                    }
                    Debug.Log($"[VixForge] Removed {culled} physics components.");
                    ExecuteDeepScan();
                }) { text = "REMOVE SELECTED" };
                executePhysBtn.AddToClassList("cyber-action-btn");
                executePhysBtn.AddToClassList("danger-btn");
                physPanel.Add(executePhysBtn);

                _resultsContainer.Add(physPanel);
            }

            if (_lastReport.OptimizationSuite.Count > 0)
            {
                var suitePanel = CreateCyberPanel("Destructive Fixes", "#ff00aa");

                foreach (var task in _lastReport.OptimizationSuite)
                {
                    var taskRow = new VisualElement();
                    taskRow.AddToClassList("opt-task-row");

                    var toggle = new Toggle { value = task.IsSelected };
                    toggle.RegisterValueChangedCallback(e => task.IsSelected = e.newValue);
                    taskRow.Add(toggle);

                    var descLabel = new Label($"<b>{task.Label}</b>\n<color=#aaaaaa>{task.Description}</color>") { enableRichText = true };
                    descLabel.AddToClassList("opt-task-desc");
                    taskRow.Add(descLabel);

                    suitePanel.Add(taskRow);
                }

                var applyBtn = new Button(ApplySelected) { text = "APPLY SELECTED FIXES" };
                applyBtn.AddToClassList("cyber-action-btn");
                applyBtn.AddToClassList("danger-btn");
                suitePanel.Add(applyBtn);

                _resultsContainer.Add(suitePanel);
            }

            if (target != null && target.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() != null)
                _resultsContainer.Add(BuildUploadPanel(target));

            if (_lastReport.TextureNodes.Count > 0)
            {
                bool isUp = _resizeMode == AvatarSDKValidator.ResizeMode.Upscale;
                var texPanel = CreateCyberPanel("Textures", "#00e5ff");

                var info = new Label($"Select which textures to {(isUp ? "upscale" : "downscale")} to {_sizePopup.value}px. Defaults pre-select only textures that need it, but you have full manual control. Sorted largest first. Only the ticked textures are changed, and the change is permanent.");
                info.AddToClassList("md-p");
                texPanel.Add(info);

                var controlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10, marginBottom = 10 } };
                int initialSelected = _lastReport.TextureNodes.Count(n => n.Process);
                var texCountLabel = new Label($"Queued for resizing: <color=#00ff66><b>{initialSelected}</b></color> / {_lastReport.TextureNodes.Count}") { enableRichText = true, style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
                controlRow.Add(texCountLabel);

                List<Toggle> texToggles = new List<Toggle>();

                void UpdateTexCount()
                {
                    int c = _lastReport.TextureNodes.Count(n => n.Process);
                    texCountLabel.text = $"Queued for resizing: <color=#00ff66><b>{c}</b></color> / {_lastReport.TextureNodes.Count}";
                }

                var btnSelectAll = new Button(() => {
                    _lastReport.TextureNodes.ForEach(n => n.Process = true);
                    foreach (var t in texToggles) t.SetValueWithoutNotify(true);
                    UpdateTexCount();
                }) { text = "Select All" };
                btnSelectAll.AddToClassList("data-tag-btn"); btnSelectAll.AddToClassList("data-tag-optimize");

                var btnDeselectAll = new Button(() => {
                    _lastReport.TextureNodes.ForEach(n => n.Process = false);
                    foreach (var t in texToggles) t.SetValueWithoutNotify(false);
                    UpdateTexCount();
                }) { text = "Deselect All" };
                btnDeselectAll.AddToClassList("data-tag-btn"); btnDeselectAll.AddToClassList("data-tag-warning");

                controlRow.Add(btnSelectAll);
                controlRow.Add(btnDeselectAll);
                texPanel.Add(controlRow);

                var texScroll = new ScrollView(ScrollViewMode.Vertical) { style = { maxHeight = 250, backgroundColor = new Color(0, 0, 0, 0.2f), paddingBottom = 5, paddingTop = 5, borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4 } };

                foreach (var node in _lastReport.TextureNodes)
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, paddingLeft = 5, paddingRight = 5, paddingTop = 2, paddingBottom = 2 } };
                    row.AddToClassList("md-row");

                    var toggle = new Toggle { value = node.Process };
                    texToggles.Add(toggle);
                    toggle.RegisterValueChangedCallback(e => { node.Process = e.newValue; UpdateTexCount(); });
                    row.Add(toggle);

                    string linTag = node.Linear ? " <color=#ffaa00><i>[linear/data]</i></color>" : "";
                    var lbl = new Label($"<b>{node.Name}</b> <color=#aaaaaa>({node.Width}x{node.Height})</color>{linTag}") { enableRichText = true, style = { flexGrow = 1 } };
                    row.Add(lbl);

                    var locateBtn = new Button(() => { EditorGUIUtility.PingObject(node.Texture); Selection.activeObject = node.Texture; }) { text = "LOCATE" };
                    locateBtn.AddToClassList("data-tag-btn"); locateBtn.AddToClassList("data-tag-locate");
                    row.Add(locateBtn);

                    texScroll.Add(row);
                }
                texPanel.Add(texScroll);

                var executeTexBtn = new Button(() => {
                    var selected = _lastReport.TextureNodes.Where(n => n.Process && n.Texture != null).Select(n => n.Texture).ToList();
                    if (selected.Count == 0)
                    {
                        Debug.LogWarning("[VixForge] No textures selected. Check at least one texture to resize.");
                        return;
                    }
                    AvatarSDKValidator.ProcessTexturesWithMagick(selected, _sizePopup.value, _resizeMode);
                    ExecuteDeepScan();
                }) { text = isUp ? "Upscale Selected Textures" : "Downscale Selected Textures" };
                executeTexBtn.AddToClassList("cyber-action-btn");
                executeTexBtn.AddToClassList("cyan-btn");
                texPanel.Add(executeTexBtn);
                if (!VixenMagickKit.IsReady)
                {
                    executeTexBtn.SetEnabled(false);
                    var magickNote = new Label(VixenMagickKit.UnavailableMessage);
                    magickNote.AddToClassList("md-p");
                    texPanel.Add(magickNote);
                }

                _resultsContainer.Add(texPanel);
            }

            BuildPlatformResult("PC Windows", _lastReport.IsPCUploadReady, _lastReport.PCErrors, _lastReport.PCPerformanceWarnings);
            BuildPlatformResult("Quest Android", _lastReport.IsQuestUploadReady, _lastReport.QuestErrors, null);
        }

        private void ApplySelected()
        {
            foreach (var task in _lastReport.OptimizationSuite.Where(t => t.IsSelected))
            {
                task.Execute?.Invoke();
                string sig = task.ComputeSignature != null ? task.ComputeSignature() : task.ID;
                OptimizationStateCache.RecordHandled(_lastReport.AvatarKey, task.ID, sig);
            }
            ExecuteDeepScan();
        }

        private void BuildPlatformResult(string title, bool ready, List<AvatarSDKValidator.Anomaly> errors, List<AvatarSDKValidator.Anomaly> warnings)
        {
            var p = CreateCyberPanel(title, ready ? "#00e5ff" : "#ff00aa");
            string status = ready ? "READY" : "BLOCKED";

            var headLabel = new Label($"<color={(ready ? "#00e5ff" : "#ff00aa")}>{title}</color> - {status}") { enableRichText = true };
            headLabel.AddToClassList("md-h1");
            p.Insert(0, headLabel);

            bool hasErrors = !ready && errors.Count > 0;
            bool hasWarns = warnings != null && warnings.Count > 0;

            if (hasErrors) foreach (var err in errors) p.Add(CreateRow(err.Description, err.ContextObject, "#ff0033", err.AutoFix, err.FixLabel));
            if (hasWarns) foreach (var warn in warnings) p.Add(CreateRow(warn.Description, warn.ContextObject, "#ffaa00", warn.AutoFix, warn.FixLabel));
            if (!hasErrors && !hasWarns) p.Add(CreateRow("Nothing wrong found. This avatar fits every limit for the platform.", null, "#00e5ff"));

            _resultsContainer.Add(p);
        }

        private VisualElement CreateRow(string text, UnityEngine.Object context, string hexColor, System.Action fix = null, string fixLabel = "OPTIMIZE")
        {
            var row = new VisualElement { style = { alignItems = Align.Center, flexDirection = FlexDirection.Row } };
            row.AddToClassList("md-row");

            ColorUtility.TryParseHtmlString(hexColor, out Color col);
            var bullet = new Label(">>") { style = { color = col } };
            bullet.AddToClassList("md-bullet");
            row.Add(bullet);

            var contentLabel = new Label(text) { enableRichText = true, style = { flexGrow = 1 } };
            contentLabel.AddToClassList("md-p");
            row.Add(contentLabel);

            if (context != null)
            {
                var locate = new Button(() => { EditorGUIUtility.PingObject(context); Selection.activeObject = context; }) { text = "LOCATE" };
                locate.AddToClassList("data-tag-btn");
                locate.AddToClassList("data-tag-locate");
                row.Add(locate);
            }
            if (fix != null)
            {
                var optimize = new Button(() => { fix.Invoke(); ExecuteDeepScan(); }) { text = fixLabel };
                optimize.AddToClassList("data-tag-btn");

                if (hexColor == "#ff0033" || hexColor == "#ff00aa" || fixLabel.Contains("REMOVE") || fixLabel.Contains("DELETE"))
                {
                    optimize.AddToClassList("data-tag-destructive");
                }
                else if (hexColor == "#ffaa00")
                {
                    optimize.AddToClassList("data-tag-warning");
                }
                else
                {
                    optimize.AddToClassList("data-tag-optimize");
                }
                row.Add(optimize);
            }
            return row;
        }

        private VisualElement CreateCyberPanel(string title, string hex)
        {
            var panel = new VisualElement();
            panel.AddToClassList("cyber-panel");
            if (!string.IsNullOrEmpty(title))
            {
                var header = new Label($"<color={hex}>{title}</color>") { enableRichText = true };
                header.AddToClassList("panel-header");
                panel.Add(header);

                var sep = new VisualElement();
                sep.AddToClassList("md-separator");
                ColorUtility.TryParseHtmlString(hex, out Color c); c.a = 0.3f;
                sep.style.backgroundColor = c;
                panel.Add(sep);
            }
            return panel;
        }
    }
}
#endif