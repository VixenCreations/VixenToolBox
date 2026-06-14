#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
        }

        public class PhysicsNode
        {
            public Component Component;
            public string Name;
            public string TypeName;
            public bool Cull = false;
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

            public List<Anomaly> PCErrors = new List<Anomaly>();
            public List<Anomaly> PCPerformanceWarnings = new List<Anomaly>();
            public List<Anomaly> QuestErrors = new List<Anomaly>();
            public List<Anomaly> Warnings = new List<Anomaly>();
            public List<OptimizationTask> OptimizationSuite = new List<OptimizationTask>();

            public string OfficialOverallRating = null;
            public List<Anomaly> OfficialPerfWarnings = new List<Anomaly>();
        }

        public static ValidationReport RunFullSweep(GameObject avatarRoot, int targetTexSize = 1024, PCPerformanceRank targetRank = PCPerformanceRank.Poor, ResizeMode resizeMode = ResizeMode.Downscale)
        {
            var report = new ValidationReport();
            if (avatarRoot == null) return report;

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
                report.OptimizationSuite.Add(new OptimizationTask
                {
                    ID = "FLATTEN_HIERARCHY",
                    Label = $"Purge {orphanedTransforms.Count} Orphaned Transforms",
                    Description = "Vixen Core Heuristic: Flattens the hierarchy by destroying empty GameObjects carrying zero vertex weights.",
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
                report.OptimizationSuite.Add(new OptimizationTask
                {
                    ID = "STRIP_DISABLED_COMPS",
                    Label = $"Strip {disabledComponents.Count} Disabled Components",
                    Description = "Vixen Core Heuristic: Destroys hard-disabled Behaviours to permanently reduce serialization overhead.",
                    Execute = () => {
                        int culled = 0;
                        foreach (var b in disabledComponents) { if (b != null) { Undo.DestroyObjectImmediate(b); culled++; } }
                        Debug.Log($"[VixForge] System Cleaned: {culled} dead components stripped.");
                    }
                });
            }

            report.OptimizationSuite.Add(new OptimizationTask
            {
                ID = "OPTIMIZE_BOUNDS",
                Label = $"<color=#00e5ff>Auto-Fit Per-Mesh Avatar Bounds</color>",
                Description = "Vixen Core Fix: Sizes each renderer's culling bounds from its bind pose, transformed into root-bone local space (per Unity SMR docs). Static meshes get a 50% safety margin; meshes driven by VRCPhysBones get 200% to cover runtime swing. Floor at 0.3m guards against degenerate bounds on stub meshes.",
                Execute = () => {
                    int meshesProcessed = 0;

                    const float staticMargin = 1.5f;
                    const float physBoneMargin = 3.0f;
                    const float minBoundsSize = 0.3f;

                    var physBoneAffected = new HashSet<Transform>();
                    foreach (var pb in avatarRoot.GetComponentsInChildren<VRCPhysBoneBase>(true))
                    {
                        Transform pbRoot = pb.GetRootTransform();
                        if (pbRoot == null) continue;
                        foreach (var t in pbRoot.GetComponentsInChildren<Transform>(true))
                            physBoneAffected.Add(t);
                    }

                    foreach (var smr in skinnedRenderers)
                    {
                        if (smr == null) continue;

                        Undo.RecordObject(smr, "Auto-Fit Bounds");

                        smr.updateWhenOffscreen = false;

                        bool hasPhysBone = false;
                        if (smr.bones != null)
                        {
                            foreach (var b in smr.bones)
                            {
                                if (b != null && physBoneAffected.Contains(b)) { hasPhysBone = true; break; }
                            }
                        }
                        float margin = hasPhysBone ? physBoneMargin : staticMargin;

                        Bounds fitted;
                        if (smr.sharedMesh != null)
                        {
                            Bounds bind = smr.sharedMesh.bounds;
                            Transform rootBone = smr.rootBone != null ? smr.rootBone : smr.transform;
                            Bounds rootSpace = TransformBoundsToSpace(bind, smr.transform, rootBone);

                            Vector3 size = rootSpace.size * margin;
                            size.x = Mathf.Max(size.x, minBoundsSize);
                            size.y = Mathf.Max(size.y, minBoundsSize);
                            size.z = Mathf.Max(size.z, minBoundsSize);
                            fitted = new Bounds(rootSpace.center, size);
                        }
                        else
                        {
                            fitted = new Bounds(Vector3.zero, Vector3.one * minBoundsSize);
                        }

                        smr.localBounds = fitted;
                        meshesProcessed++;
                    }
                    Debug.Log($"[VixForge] Geometry Culling System updated: Per-mesh bounds fitted on {meshesProcessed} renderers (PhysBone-aware margins).");
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
                report.OptimizationSuite.Add(new OptimizationTask
                {
                    ID = "COLLAPSE_LEAF_BONES",
                    Label = $"<color=#ff0033>Collapse {deepLeafBones.Count} Dead-End Leaf Bones</color>",
                    Description = "Destructive Topology: Clones meshes, folds terminal vertex weights into parent bones. Ignores elements shielded by Physics.",
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

            List<SkinnedMeshRenderer> heavyMeshes = skinnedRenderers.Where(s => s.sharedMesh != null && CountTriangles(s.sharedMesh) > 15000).ToList();
            if (heavyMeshes.Count > 0)
            {
                report.OptimizationSuite.Add(new OptimizationTask
                {
                    ID = "WELD_VERTICES",
                    Label = $"<color=#00e5ff>Precision Multi-Pass Microweld ({heavyMeshes.Count} Meshes)</color>",
                    Description = "Safe Topology Optimization: Iteratively seals sub-millimeter seams. <color=#00ff66><b>STRICTLY LOCKS UVs.</b></color> Visually preserves the avatar, but will intentionally halt before reaching extreme Quest limits to protect geometry.",
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
                                    HumanBodyBones.Head,
                                    HumanBodyBones.Neck,
                                    HumanBodyBones.LeftHand,
                                    HumanBodyBones.RightHand
                                );
                            }

                            VixenMeshPatcher.MultipassTargetedWeld(
                                smr,
                                targetTriangles: 14500,
                                startThreshold: 0.0001f,
                                maxThreshold: 0.005f,
                                step: 0.0005f,
                                protectedSubmeshes: protectedSlots,
                                protectedBones: protectedBoneIndices
                            );

                            newTotal += smr.sharedMesh.vertexCount;
                        }
                        Debug.Log($"[VixForge] Topology Welded: Erased {originalTotal - newTotal} vertices. Kinematic shielding active.");
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

            int processableTextures = 0;
            foreach (var t in report.UniqueTextures)
                if (IsProcessableTexture(t, out _)) processableTextures++;

            bool showResizeTask = processableTextures > 0 &&
                (resizeMode == ResizeMode.Upscale || report.TotalVRAM_MB > 40f);

            if (showResizeTask)
            {
                bool isUp = resizeMode == ResizeMode.Upscale;
                report.OptimizationSuite.Add(new OptimizationTask
                {
                    ID = isUp ? "VRAM_UPSCALE" : "VRAM_REDUCE",
                    Label = $"{(isUp ? "Upscale" : "Downscale")} {processableTextures} Textures to {targetTexSize}px",
                    Description = isUp
                        ? "Uses ImageMagick with Mitchell filter + adaptive sharpening to upscale undersized textures. Skips textures already at or above target."
                        : "Uses ImageMagick for destructive zero-cloud VRAM control. Hits all textures, including VRCFury variants.",
                    Execute = () => ProcessTexturesWithMagick(report.UniqueTextures, targetTexSize, resizeMode)
                });
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

        private static Bounds TransformBoundsToSpace(Bounds source, Transform sourceSpace, Transform targetSpace)
        {
            if (sourceSpace == targetSpace || targetSpace == null) return source;

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
                Vector3 world = sourceSpace.TransformPoint(corner);
                Vector3 target = targetSpace.InverseTransformPoint(world);
                min = Vector3.Min(min, target);
                max = Vector3.Max(max, target);
            }

            return new Bounds((min + max) * 0.5f, max - min);
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
                        string msg = string.IsNullOrEmpty(errorText) ? statText : $"{statText} — {errorText}";
                        if (!string.IsNullOrEmpty(msg))
                            report.OfficialPerfWarnings.Add(new Anomaly { Description = msg, ContextObject = avatarRoot });
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VixForge] Official VRChat performance scan unavailable: {e.Message}");
            }
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

        private static void ProcessTexturesWithMagick(HashSet<Texture> textures, int targetSize, ResizeMode mode)
        {
            int count = 0;
            int processed = 0;
            int total = textures.Count;
            string activeVerb = mode == ResizeMode.Downscale ? "Downscaling" : "Upscaling";
            bool canceled = false;

            try
            {
                foreach (var tex in textures)
                {
                    processed++;
                    if (!IsProcessableTexture(tex, out string path)) continue;

                    if (EditorUtility.DisplayCancelableProgressBar(
                            $"VixForge: {activeVerb} Textures",
                            $"({processed}/{total}) {System.IO.Path.GetFileName(path)}",
                            (float)processed / Mathf.Max(1, total)))
                    {
                        canceled = true;
                        Debug.LogWarning($"[VixForge] {activeVerb} canceled at {processed}/{total}.");
                        break;
                    }

                    try
                    {
                        using (MagickImage img = new MagickImage(File.ReadAllBytes(path)))
                        {
                            bool needsWork = mode == ResizeMode.Downscale
                                ? (img.Width > targetSize || img.Height > targetSize)
                                : (img.Width < targetSize && img.Height < targetSize);

                            if (needsWork)
                            {
                                img.FilterType = mode == ResizeMode.Downscale ? FilterType.Lanczos : FilterType.Mitchell;
                                img.Resize(new MagickGeometry((uint)targetSize, (uint)targetSize));
                                if (mode == ResizeMode.Upscale)
                                {
                                    img.AdaptiveSharpen(0, 0.6);
                                }
                                img.Strip();
                                img.Write(path);
                                count++;
                            }
                        }
                        VixenMagickKit.TryLosslessOptimize(path);
                    }
                    catch (System.Exception e) { Debug.LogWarning($"[VixForge] Magick failed for {tex.name}: {e.Message}"); }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            string verb = mode == ResizeMode.Downscale ? "compressed" : "upscaled";
            string tail = canceled ? " (canceled)" : "";
            Debug.Log($"[VixForge] Optimization Engine: {count} textures {verb}{tail}.");
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

            _sizePopup = new PopupField<int>("Optimization Target (px)", SizePresets, 1024);
            configPanel.Add(_sizePopup);

            _modeEnum = new EnumField("Resize Mode", _resizeMode);
            _modeEnum.RegisterValueChangedCallback(e => _resizeMode = (AvatarSDKValidator.ResizeMode)e.newValue);
            configPanel.Add(_modeEnum);

            _rankEnum = new EnumField("Target PC Performance Rank", _targetRank);
            _rankEnum.RegisterValueChangedCallback(e => _targetRank = (AvatarSDKValidator.PCPerformanceRank)e.newValue);
            configPanel.Add(_rankEnum);

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

            _lastReport = AvatarSDKValidator.RunFullSweep(target, _sizePopup.value, _targetRank, _resizeMode);

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

            BuildPlatformResult("PC Windows Pipeline", _lastReport.IsPCUploadReady, _lastReport.PCErrors, _lastReport.PCPerformanceWarnings);
            BuildPlatformResult("Quest Android Pipeline", _lastReport.IsQuestUploadReady, _lastReport.QuestErrors, null);
        }

        private void ApplySelected()
        {
            foreach (var task in _lastReport.OptimizationSuite.Where(t => t.IsSelected)) task.Execute?.Invoke();
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