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
using ImageMagick;
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
            public bool IsSelected = true;
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
        }

        public static ValidationReport RunFullSweep(GameObject avatarRoot, int targetTexSize = 1024, PCPerformanceRank targetRank = PCPerformanceRank.Poor, ResizeMode resizeMode = ResizeMode.Downscale, int decimateTarget = 24000)
        {
            var report = new ValidationReport();
            if (avatarRoot == null) return report;

            report.AvatarKey = OptimizationStateCache.GetAvatarKey(avatarRoot);

            void AddTask(OptimizationTask task)
            {
                if (task == null) return;
                string sig = task.ComputeSignature != null ? task.ComputeSignature() : task.ID;
                if (OptimizationStateCache.IsHandled(report.AvatarKey, task.ID, sig)) return;
                report.OptimizationSuite.Add(task);
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
                report.PCErrors.Add(new Anomaly { Description = "Missing VRCAvatarDescriptor. The SDK pipeline will block this.", ContextObject = avatarRoot });
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
            foreach (var col in avatarRoot.GetComponentsInChildren<VRCPhysBoneColliderBase>(true)) if (col.rootTransform != null) protectedTransforms.Add(col.rootTransform);
            foreach (var contact in avatarRoot.GetComponentsInChildren<VRCContactReceiver>(true)) if (contact.rootTransform != null) protectedTransforms.Add(contact.rootTransform);
            foreach (var constraint in avatarRoot.GetComponentsInChildren<UnityEngine.Animations.IConstraint>(true))
            {
                Component comp = constraint as Component;
                if (comp != null) protectedTransforms.Add(comp.transform);
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
                    Label = $"Purge {orphanedTransforms.Count} Orphaned Transforms",
                    Description = "Vixen Core Heuristic: Flattens the hierarchy by destroying empty GameObjects carrying zero vertex weights.",
                    ComputeSignature = () => "orphans:" + orphanedTransforms.Count(t => t != null),
                    Execute = () => {
                        int culled = 0;
                        foreach (var t in orphanedTransforms) { if (t != null) { Undo.DestroyObjectImmediate(t.gameObject); culled++; } }
                        Debug.Log($"[VixForge] Topology Flattened: {culled} orphans purged.");
                    }
                });
            }

            List<Behaviour> disabledComponents = new List<Behaviour>();
            foreach (var b in avatarRoot.GetComponentsInChildren<Behaviour>(true))
            {
                if (!b.enabled && !(b is Animator)) disabledComponents.Add(b);
            }

            if (disabledComponents.Count > 0)
            {
                AddTask(new OptimizationTask
                {
                    ID = "STRIP_DISABLED_COMPS",
                    Label = $"Strip {disabledComponents.Count} Disabled Components",
                    Description = "Vixen Core Heuristic: Destroys hard-disabled Behaviours to permanently reduce serialization overhead.",
                    ComputeSignature = () => "disabled:" + disabledComponents.Count(b => b != null),
                    Execute = () => {
                        int culled = 0;
                        foreach (var b in disabledComponents) { if (b != null) { Undo.DestroyObjectImmediate(b); culled++; } }
                        Debug.Log($"[VixForge] System Cleaned: {culled} dead components stripped.");
                    }
                });
            }

            AddTask(new OptimizationTask
            {
                ID = "OPTIMIZE_BOUNDS",
                Label = $"<color=#00e5ff>Auto-Fit Per-Mesh Avatar Bounds</color>",
                Description = "Vixen Core Fix: Fits each renderer's culling bounds to its real skinned geometry, sampled across every bound bone and bind pose so meshes driven by many bones or a scaled armature measure their true size (not the authored mesh AABB). Adds a small skinning margin and, for VRCPhysBone-driven meshes, the real swing reach of the affecting chains (no blunt multipliers). Uses a scale-aware world-space floor so meshes authored at odd scales aren't over-inflated, and sets Update When Offscreen off since VRChat culls on the static bounds.",
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
                    const float physBoneReachSafety = 1.15f;
                    const float minWorldFloor = 0.01f;

                    var physBones = new List<PhysBoneReach>();
                    foreach (var pb in avatarRoot.GetComponentsInChildren<VRCPhysBoneBase>(true))
                    {
                        Transform pbRoot = pb.GetRootTransform();
                        if (pbRoot == null) continue;

                        var boneDist = new Dictionary<Transform, float>();
                        foreach (var t in pbRoot.GetComponentsInChildren<Transform>(true))
                            boneDist[t] = Vector3.Distance(pbRoot.position, t.position);
                        physBones.Add(new PhysBoneReach { RootWorld = pbRoot.position, BoneDist = boneDist });
                    }

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
                            fitted = TransformBoundsCorners(smr.sharedMesh.bounds, p => rootBone.InverseTransformPoint(smr.transform.TransformPoint(p)));
                        fitted.Expand(fitted.size * (staticMargin - 1f));

                        if (smr.bones != null && smr.bones.Length > 0 && physBones.Count > 0)
                        {
                            HashSet<Transform> weightedBones = new HashSet<Transform>();
                            BoneWeight[] meshWeights = smr.sharedMesh.boneWeights;
                            if (meshWeights != null && meshWeights.Length > 0)
                            {
                                HashSet<int> wIdx = new HashSet<int>();
                                foreach (var w in meshWeights)
                                {
                                    if (w.weight0 > 0f) wIdx.Add(w.boneIndex0);
                                    if (w.weight1 > 0f) wIdx.Add(w.boneIndex1);
                                    if (w.weight2 > 0f) wIdx.Add(w.boneIndex2);
                                    if (w.weight3 > 0f) wIdx.Add(w.boneIndex3);
                                }
                                for (int bi = 0; bi < smr.bones.Length; bi++)
                                    if (wIdx.Contains(bi) && smr.bones[bi] != null) weightedBones.Add(smr.bones[bi]);
                            }
                            else
                            {
                                foreach (var b in smr.bones) if (b != null) weightedBones.Add(b);
                            }

                            foreach (var pb in physBones)
                            {
                                float relevantReach = 0f;
                                foreach (var b in weightedBones)
                                {
                                    if (pb.BoneDist.TryGetValue(b, out float d) && d > relevantReach)
                                        relevantReach = d;
                                }
                                if (relevantReach > 0f)
                                {
                                    Bounds pbWorld = new Bounds(pb.RootWorld, Vector3.one * (relevantReach * physBoneReachSafety * 2f));
                                    fitted.Encapsulate(TransformBoundsCorners(pbWorld, rootBone.InverseTransformPoint));
                                }
                            }
                        }

                        Vector3 size = fitted.size;
                        size.x = Mathf.Max(size.x, localFloor.x);
                        size.y = Mathf.Max(size.y, localFloor.y);
                        size.z = Mathf.Max(size.z, localFloor.z);
                        fitted.size = size;

                        smr.localBounds = fitted;
                        meshesProcessed++;
                    }
                    Debug.Log($"[VixForge] Geometry Culling System updated: {meshesProcessed} renderers fitted (true skinned AABB across all bound bones + real PhysBone swing reach).");
                }
            });

            List<Transform> deepLeafBones = new List<Transform>();
            if (report.ArmatureRoot != null)
            {
                foreach (var t in report.ArmatureRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (t.childCount == 0 && !humanoidBones.Contains(t) && !HasPhysBoneProtection(t, avatarRoot))
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
                    Label = $"<color=#ff0033>Collapse {deepLeafBones.Count} Dead-End Leaf Bones</color>",
                    Description = "Destructive Topology: Clones meshes, folds terminal vertex weights into parent bones. Ignores elements shielded by Physics.",
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
                    Label = $"<color=#00e5ff>Precision QEM Decimation ({heavyMeshes.Count} Meshes)</color>",
                    Description = $"Quadric Error Metric edge-collapse decimation (Garland-Heckbert), the same class of algorithm as Blender's Decimate. Drives each heavy mesh toward the slider target of ~{decimateTargetTris:N0} triangles while preventing face flips. <color=#00ff66><b>Preserves UV/normal seams, material (submesh) boundaries, open borders, and locks eye/face/hand submeshes plus humanoid Hand bones.</b></color> Interpolates UVs, colors and bone weights across each collapse, and remaps blendshapes. Halts early rather than shredding protected geometry.",
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
                            for (int m = 0; m < smr.sharedMaterials.Length; m++)
                            {
                                var mat = smr.sharedMaterials[m];
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
                    if (d is Texture tex && report.UniqueTextures.Add(tex)) totalBytes += Profiler.GetRuntimeMemorySizeLong(tex);
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
                    FixLabel = "CULL COMPONENT"
                });
            }

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
            foreach (var r in renderers)
            {
                foreach (var m in r.sharedMaterials)
                {
                    if (m != null && m.shader != null && !mobileShaderWhitelist.Contains(m.shader.name) && m.shader.name != "Hidden/InternalErrorShader")
                    {
                        report.IsQuestUploadReady = false;
                        report.QuestErrors.Add(new Anomaly { Description = $"Unsupported Quest Shader: <b>{m.shader.name}</b> (Mesh: {r.gameObject.name})", ContextObject = m });
                    }
                }
            }

            foreach (var joint in avatarRoot.GetComponentsInChildren<Joint>(true))
            {
                report.IsQuestUploadReady = false;
                report.QuestErrors.Add(new Anomaly { Description = $"Forbidden Physics Joint [{joint.GetType().Name}] on <b>{joint.gameObject.name}</b>.", ContextObject = joint.gameObject, AutoFix = () => Undo.DestroyObjectImmediate(joint), FixLabel = "STRIP JOINT" });
            }

            foreach (var cam in avatarRoot.GetComponentsInChildren<Camera>(true))
            {
                report.IsQuestUploadReady = false;
                report.QuestErrors.Add(new Anomaly { Description = $"Camera found on <b>{cam.gameObject.name}</b>. Prohibited on Quest.", ContextObject = cam.gameObject, AutoFix = () => Undo.DestroyObjectImmediate(cam.gameObject), FixLabel = "CULL CAMERA" });
            }

            return report;
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

        private struct PhysBoneReach
        {
            public Vector3 RootWorld;
            public Dictionary<Transform, float> BoneDist;
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

            Mesh baked = new Mesh();
            try
            {
                smr.BakeMesh(baked, false);
                Vector3[] verts = baked.vertices;
                if (verts == null || verts.Length == 0) return false;

                Matrix4x4 m = rootBone.worldToLocalMatrix * smr.transform.localToWorldMatrix;
                return AccumulateBounds(verts, i => m.MultiplyPoint3x4(verts[i]), out bounds);
            }
            catch
            {
                return false;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(baked);
            }
        }

        private static bool AccumulateBounds(Vector3[] verts, System.Func<int, Vector3> map, out Bounds bounds)
        {
            bounds = default;
            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 p = map(i);
                if (float.IsNaN(p.x) || float.IsInfinity(p.x)) continue;
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }

            if (min.x > max.x) return false;
            bounds = new Bounds((min + max) * 0.5f, max - min);
            return true;
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
                long bytes = Profiler.GetRuntimeMemorySizeLong(tex);
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
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";

        private Font _cyberFont;
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

        private void OnEnable() => _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

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
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            header.Add(titleLabel);
            root.Add(header);

            var scroll = new ScrollView() { style = { flexGrow = 1, paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };

            var configPanel = CreateCyberPanel("Target Parameters", "#00e5ff");
            _targetField = new ObjectField("Avatar Root") { objectType = typeof(GameObject), allowSceneObjects = true };
            configPanel.Add(_targetField);

            _sizePopup = new PopupField<int>("Optimization Target (px)", SizePresets, Mathf.Max(0, SizePresets.IndexOf(1024)));
            configPanel.Add(_sizePopup);

            _modeEnum = new EnumField("Resize Mode", _resizeMode);
            _modeEnum.RegisterValueChangedCallback(e => _resizeMode = (AvatarSDKValidator.ResizeMode)e.newValue);
            configPanel.Add(_modeEnum);

            _rankEnum = new EnumField("Target PC Performance Rank", _targetRank);
            _rankEnum.RegisterValueChangedCallback(e => _targetRank = (AvatarSDKValidator.PCPerformanceRank)e.newValue);
            configPanel.Add(_rankEnum);

            _decimateSlider = new SliderInt("Decimation Target (tris / mesh)", 2000, 70000) { value = _decimateTarget, showInputField = true };
            _decimateSlider.RegisterValueChangedCallback(e => _decimateTarget = e.newValue);
            configPanel.Add(_decimateSlider);

            var decimateHint = new Label("Higher = softer / more detail. Lower = smaller / more aggressive. Only meshes above this triangle count are decimated down to it.");
            decimateHint.AddToClassList("md-p");
            configPanel.Add(decimateHint);

            var scanBtn = new Button(ExecuteDeepScan) { text = "EXECUTE DEEP SYSTEM SCAN" };
            scanBtn.AddToClassList("cyber-action-btn");
            scanBtn.AddToClassList("cyan-btn");
            configPanel.Add(scanBtn);
            scroll.Add(configPanel);

            _resultsContainer = new VisualElement();
            scroll.Add(_resultsContainer);
            root.Add(scroll);
        }

        private void ExecuteDeepScan()
        {
            _resultsContainer.Clear();
            var target = _targetField.value as GameObject;

            _lastReport = AvatarSDKValidator.RunFullSweep(target, _sizePopup.value, _targetRank, _resizeMode, _decimateTarget);

            var archPanel = CreateCyberPanel("Hierarchy Topology", "#00e5ff");
            if (_lastReport.ArmatureRoot != null)
            {
                archPanel.Add(CreateRow($"<b>Armature Root:</b> {_lastReport.ArmatureRoot.name}", _lastReport.ArmatureRoot, "#00e5ff"));
                archPanel.Add(CreateRow($"<b>Bone Density:</b> {_lastReport.BoneCount} Transforms", null, "#00e5ff"));

                string vramHex = _lastReport.TotalVRAM_MB > 150f ? "#ff0033" : (_lastReport.TotalVRAM_MB > 40f ? "#ffaa00" : "#00e5ff");
                archPanel.Add(CreateRow($"<b>Hardware VRAM Footprint:</b> {_lastReport.TotalVRAM_MB:F2} MB ({_lastReport.UniqueTextures.Count} Textures)", null, vramHex));
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

            var statsPanel = CreateCyberPanel("Hardware Cap Analysis", "#00ff66");

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
                var physPanel = CreateCyberPanel("Interactive Physics System", "#ffaa00");

                var info = new Label("Select specific physics components to violently purge from the hierarchy to meet Target Rank constraints. Sorted by depth (Leaf nodes first).");
                info.AddToClassList("md-p");
                physPanel.Add(info);

                var controlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10, marginBottom = 10 } };

                var physCountLabel = new Label($"Queued for Eradication: <color=#ff0033><b>0</b></color> / {_lastReport.PhysicsNodes.Count}") { enableRichText = true, style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
                controlRow.Add(physCountLabel);

                List<Toggle> nodeToggles = new List<Toggle>();

                var btnSelectAll = new Button(() => {
                    _lastReport.PhysicsNodes.ForEach(n => n.Cull = true);
                    foreach (var t in nodeToggles) t.SetValueWithoutNotify(true);
                    physCountLabel.text = $"Queued for Eradication: <color=#ff0033><b>{_lastReport.PhysicsNodes.Count}</b></color> / {_lastReport.PhysicsNodes.Count}";
                }) { text = "Select All" };
                btnSelectAll.AddToClassList("data-tag-btn"); btnSelectAll.AddToClassList("data-tag-destructive");

                var btnDeselectAll = new Button(() => {
                    _lastReport.PhysicsNodes.ForEach(n => n.Cull = false);
                    foreach (var t in nodeToggles) t.SetValueWithoutNotify(false);
                    physCountLabel.text = $"Queued for Eradication: <color=#ff0033><b>0</b></color> / {_lastReport.PhysicsNodes.Count}";
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
                        physCountLabel.text = $"Queued for Eradication: <color=#ff0033><b>{culledCount}</b></color> / {_lastReport.PhysicsNodes.Count}";
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
                    Debug.Log($"[VixForge] System Culler: Eradicated {culled} physics nodes.");
                    ExecuteDeepScan();
                }) { text = "EXECUTE PHYSICS ERADICATION" };
                executePhysBtn.AddToClassList("cyber-action-btn");
                executePhysBtn.AddToClassList("danger-btn");
                physPanel.Add(executePhysBtn);

                _resultsContainer.Add(physPanel);
            }

            if (_lastReport.OptimizationSuite.Count > 0)
            {
                var suitePanel = CreateCyberPanel("Destructive Topology Engine", "#ff00aa");

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

                var applyBtn = new Button(ApplySelected) { text = "EXECUTE DESTRUCTIVE TOPOLOGY FIXES" };
                applyBtn.AddToClassList("cyber-action-btn");
                applyBtn.AddToClassList("danger-btn");
                suitePanel.Add(applyBtn);

                _resultsContainer.Add(suitePanel);
            }

            if (_lastReport.TextureNodes.Count > 0)
            {
                bool isUp = _resizeMode == AvatarSDKValidator.ResizeMode.Upscale;
                var texPanel = CreateCyberPanel("Texture Optimization Targeting", "#00e5ff");

                var info = new Label($"Select which textures to {(isUp ? "upscale" : "downscale")} to {_sizePopup.value}px. Defaults pre-select only textures that need it, but you have full manual control. Sorted largest first; runs ImageMagick destructively on the checked set only.");
                info.AddToClassList("md-p");
                texPanel.Add(info);

                var controlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10, marginBottom = 10 } };
                int initialSelected = _lastReport.TextureNodes.Count(n => n.Process);
                var texCountLabel = new Label($"Queued for ImageMagick: <color=#00ff66><b>{initialSelected}</b></color> / {_lastReport.TextureNodes.Count}") { enableRichText = true, style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
                controlRow.Add(texCountLabel);

                List<Toggle> texToggles = new List<Toggle>();

                void UpdateTexCount()
                {
                    int c = _lastReport.TextureNodes.Count(n => n.Process);
                    texCountLabel.text = $"Queued for ImageMagick: <color=#00ff66><b>{c}</b></color> / {_lastReport.TextureNodes.Count}";
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
                }) { text = isUp ? "EXECUTE TARGETED UPSCALE" : "EXECUTE TARGETED DOWNSCALE" };
                executeTexBtn.AddToClassList("cyber-action-btn");
                executeTexBtn.AddToClassList("cyan-btn");
                texPanel.Add(executeTexBtn);

                _resultsContainer.Add(texPanel);
            }

            BuildPlatformResult("PC Windows Pipeline", _lastReport.IsPCUploadReady, _lastReport.PCErrors, _lastReport.PCPerformanceWarnings);
            BuildPlatformResult("Quest Android Pipeline", _lastReport.IsQuestUploadReady, _lastReport.QuestErrors, null);
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
            string status = ready ? "SYSTEM GREEN: VALIDATED" : "SYSTEM RED: BLOCKED";

            var headLabel = new Label($"<color={(ready ? "#00e5ff" : "#ff00aa")}>{title}</color> - {status}") { enableRichText = true };
            headLabel.AddToClassList("md-h1");
            if (_cyberFont != null) headLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            p.Insert(0, headLabel);

            bool hasErrors = !ready && errors.Count > 0;
            bool hasWarns = warnings != null && warnings.Count > 0;

            if (hasErrors) foreach (var err in errors) p.Add(CreateRow(err.Description, err.ContextObject, "#ff0033", err.AutoFix, err.FixLabel));
            if (hasWarns) foreach (var warn in warnings) p.Add(CreateRow(warn.Description, warn.ContextObject, "#ffaa00", warn.AutoFix, warn.FixLabel));
            if (!hasErrors && !hasWarns) p.Add(CreateRow("Zero anomalies detected. Platform constraints perfectly mapped.", null, "#00e5ff"));

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

                if (hexColor == "#ff0033" || hexColor == "#ff00aa" || fixLabel.Contains("CULL") || fixLabel.Contains("PURGE") || fixLabel.Contains("STRIP"))
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
                if (_cyberFont != null) header.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
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