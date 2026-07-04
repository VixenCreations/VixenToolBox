#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace VixenTools.Editor
{
    public static class VixenMeshPatcher
    {
        private const string GENERATED_ASSET_PATH = "Assets/VixenTools/Meshes/Patched/";

        public static void PatchSkinnedMesh(SkinnedMeshRenderer smr, string patchLabel, System.Action<Mesh, SkinnedMeshRenderer> patchingLogic)
        {
            if (smr == null || smr.sharedMesh == null) return;

            Mesh clonedMesh = UnityEngine.Object.Instantiate(smr.sharedMesh);
            clonedMesh.name = $"{smr.sharedMesh.name}_VixenPatched_{patchLabel}";

            patchingLogic?.Invoke(clonedMesh, smr);

            clonedMesh.RecalculateBounds();
            clonedMesh.RecalculateNormals();
            clonedMesh.RecalculateTangents();

            if (!AssetDatabase.IsValidFolder("Assets/VixenTools"))
                AssetDatabase.CreateFolder("Assets", "VixenTools");

            if (!AssetDatabase.IsValidFolder("Assets/VixenTools/Meshes"))
                AssetDatabase.CreateFolder("Assets/VixenTools", "Meshes");

            if (!AssetDatabase.IsValidFolder("Assets/VixenTools/Meshes/Patched"))
                AssetDatabase.CreateFolder("Assets/VixenTools/Meshes", "Patched");

            string assetPath = $"{GENERATED_ASSET_PATH}{clonedMesh.name}_{System.Guid.NewGuid().ToString().Substring(0, 5)}.asset";
            AssetDatabase.CreateAsset(clonedMesh, assetPath);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(smr, "Apply Patched Mesh");
            smr.sharedMesh = clonedMesh;

            Debug.Log($"[VixForge] Topology Patched: {clonedMesh.name} serialized to {assetPath}");
        }

        public static void WeldVertices(
            SkinnedMeshRenderer smr,
            float threshold = 0.0001f,
            HashSet<int> protectedSubmeshes = null,
            HashSet<int> protectedBones = null,
            float smoothing = 1f,
            int seamRelaxIterations = 0,
            float seamRelaxFactor = 0.5f)
        {
            if (protectedSubmeshes == null) protectedSubmeshes = new HashSet<int>();
            if (protectedBones == null) protectedBones = new HashSet<int>();

            PatchSkinnedMesh(smr, $"MicroWeld_{threshold:F4}", (mesh, renderer) =>
            {
                var shapes = ExtractBlendShapes(mesh, out int originalCount);

                var geo = new WeldGeometry
                {
                    Verts = mesh.vertices,
                    Normals = mesh.normals,
                    Uvs = mesh.uv,
                    Weights = mesh.boneWeights,
                    Submeshes = GetSubmeshes(mesh)
                };

                WeldPass pass = RunWeldPass(geo, threshold, smoothing, protectedSubmeshes, protectedBones);

                if (seamRelaxIterations > 0)
                    LaplacianRelax(pass, seamRelaxIterations, seamRelaxFactor);

                ApplyGeometry(mesh, pass.Geometry);
                RemapBlendShapes(mesh, shapes, pass.Map, originalCount, pass.Geometry.Verts.Length);
            });
        }

        public static void MultipassTargetedWeld(
            SkinnedMeshRenderer smr,
            int targetTriangles = 15000,
            float startThreshold = 0.0001f,
            float maxThreshold = 0.005f,
            float step = 0.0005f,
            HashSet<int> protectedSubmeshes = null,
            HashSet<int> protectedBones = null,
            float smoothing = 1f,
            int seamRelaxIterations = 0,
            float seamRelaxFactor = 0.5f)
        {
            if (protectedSubmeshes == null) protectedSubmeshes = new HashSet<int>();
            if (protectedBones == null) protectedBones = new HashSet<int>();

            PatchSkinnedMesh(smr, $"MicroWeld_{targetTriangles}Tri", (mesh, renderer) =>
            {
                var shapes = ExtractBlendShapes(mesh, out int originalCount);

                var geo = new WeldGeometry
                {
                    Verts = mesh.vertices,
                    Normals = mesh.normals,
                    Uvs = mesh.uv,
                    Weights = mesh.boneWeights,
                    Submeshes = GetSubmeshes(mesh)
                };

                int[] masterMap = new int[originalCount];
                for (int i = 0; i < masterMap.Length; i++) masterMap[i] = i;

                int triCount = CountTriangles(geo.Submeshes);
                float threshold = startThreshold;
                int pass = 1;
                WeldPass last = null;

                while (triCount > targetTriangles && threshold <= maxThreshold)
                {
                    last = RunWeldPass(geo, threshold, smoothing, protectedSubmeshes, protectedBones);

                    for (int i = 0; i < masterMap.Length; i++) masterMap[i] = last.Map[masterMap[i]];

                    geo = last.Geometry;
                    triCount = last.TriangleCount;
                    threshold += step;
                    pass++;
                }

                if (last != null && seamRelaxIterations > 0)
                    LaplacianRelax(last, seamRelaxIterations, seamRelaxFactor);

                ApplyGeometry(mesh, geo);
                RemapBlendShapes(mesh, shapes, masterMap, originalCount, geo.Verts.Length);

                Debug.Log($"[VixForge] Precision Microweld Halted. Passes: {pass}. Final Tris: {triCount}");
            });
        }

        public static void DecimateToTarget(
            SkinnedMeshRenderer smr,
            int targetTriangles,
            HashSet<int> protectedSubmeshes = null,
            HashSet<int> protectedBones = null,
            double aggressiveness = 7.0,
            bool preserveBorders = true,
            int smoothIterations = 0)
        {
            if (protectedSubmeshes == null) protectedSubmeshes = new HashSet<int>();
            if (protectedBones == null) protectedBones = new HashSet<int>();

            PatchSkinnedMesh(smr, $"Decimate_{targetTriangles}Tri", (mesh, renderer) =>
            {
                var shapes = ExtractBlendShapes(mesh, out int originalCount);
                Matrix4x4[] bindposes = mesh.bindposes;

                QuadricSimplifier.Simplify(
                    mesh,
                    targetTriangles,
                    aggressiveness,
                    preserveBorders,
                    protectedSubmeshes,
                    protectedBones,
                    smoothIterations,
                    out int[] finalMap,
                    out int newCount,
                    out int finalTriangles);

                if (bindposes != null && bindposes.Length > 0) mesh.bindposes = bindposes;

                RemapBlendShapes(mesh, shapes, finalMap, originalCount, newCount);

                Debug.Log($"[VixForge] QEM Decimation complete. Verts {originalCount} -> {newCount}. Final Tris: {finalTriangles} (target {targetTriangles}).");
            });
        }

        private static BoneWeight BlendBoneWeights(BoneWeight a, BoneWeight b, float t)
        {
            var influences = new Dictionary<int, float>();
            void Add(int idx, float w)
            {
                if (w <= 0f) return;
                influences.TryGetValue(idx, out float existing);
                influences[idx] = existing + w;
            }

            float ta = 1f - t;
            Add(a.boneIndex0, a.weight0 * ta); Add(a.boneIndex1, a.weight1 * ta);
            Add(a.boneIndex2, a.weight2 * ta); Add(a.boneIndex3, a.weight3 * ta);
            Add(b.boneIndex0, b.weight0 * t); Add(b.boneIndex1, b.weight1 * t);
            Add(b.boneIndex2, b.weight2 * t); Add(b.boneIndex3, b.weight3 * t);

            if (influences.Count == 0) return a;

            var top = influences.OrderByDescending(kv => kv.Value).Take(4).ToList();
            float sum = 0f;
            foreach (var kv in top) sum += kv.Value;
            if (sum <= 0f) return a;

            var res = new BoneWeight();
            for (int i = 0; i < top.Count; i++)
            {
                float w = top[i].Value / sum;
                int idx = top[i].Key;
                switch (i)
                {
                    case 0: res.boneIndex0 = idx; res.weight0 = w; break;
                    case 1: res.boneIndex1 = idx; res.weight1 = w; break;
                    case 2: res.boneIndex2 = idx; res.weight2 = w; break;
                    default: res.boneIndex3 = idx; res.weight3 = w; break;
                }
            }
            return res;
        }

        private static WeldPass RunWeldPass(
            WeldGeometry input,
            float threshold,
            float smoothing,
            HashSet<int> protectedSubmeshes,
            HashSet<int> protectedBones,
            float uvMultiplier = 10000f)
        {
            Vector3[] verts = input.Verts;
            Vector3[] normals = input.Normals;
            Vector2[] uvs = input.Uvs;
            BoneWeight[] weights = input.Weights;
            int n = verts.Length;

            bool hasNormals = normals != null && normals.Length == n;
            bool hasUvs = uvs != null && uvs.Length == n;
            bool hasWeights = weights != null && weights.Length == n;

            HashSet<int> protectedVerts = new HashSet<int>();
            for (int s = 0; s < input.Submeshes.Count; s++)
            {
                if (protectedSubmeshes.Contains(s))
                    foreach (int t in input.Submeshes[s]) protectedVerts.Add(t);
            }

            bool IsLocked(int i)
            {
                if (protectedVerts.Contains(i)) return true;
                if (protectedBones.Count > 0 && hasWeights)
                {
                    BoneWeight w = weights[i];
                    if ((w.weight0 > 0 && protectedBones.Contains(w.boneIndex0)) ||
                        (w.weight1 > 0 && protectedBones.Contains(w.boneIndex1)) ||
                        (w.weight2 > 0 && protectedBones.Contains(w.boneIndex2)) ||
                        (w.weight3 > 0 && protectedBones.Contains(w.boneIndex3)))
                        return true;
                }
                return false;
            }

            float invCell = 1f / Mathf.Max(threshold, 1e-5f);
            float sqrThreshold = threshold * threshold;

            long UvCell(float v) => (long)Mathf.Round(v * uvMultiplier);
            (long, long, long) PosCell(Vector3 p) => (
                (long)Mathf.Floor(p.x * invCell),
                (long)Mathf.Floor(p.y * invCell),
                (long)Mathf.Floor(p.z * invCell));

            var buckets = new Dictionary<(long, long, long, long, long), List<int>>();
            for (int i = 0; i < n; i++)
            {
                if (IsLocked(i)) continue;
                long ux = hasUvs ? UvCell(uvs[i].x) : 0;
                long uy = hasUvs ? UvCell(uvs[i].y) : 0;
                var (cx, cy, cz) = PosCell(verts[i]);
                var key = (ux, uy, cx, cy, cz);
                if (!buckets.TryGetValue(key, out var list)) { list = new List<int>(); buckets[key] = list; }
                list.Add(i);
            }

            int[] map = new int[n];
            for (int i = 0; i < n; i++) map[i] = -1;

            List<Vector3> newVerts = new List<Vector3>();
            List<Vector3> newNormals = hasNormals ? new List<Vector3>() : null;
            List<Vector2> newUvs = hasUvs ? new List<Vector2>() : null;
            List<BoneWeight> newWeights = hasWeights ? new List<BoneWeight>() : null;
            List<bool> seam = new List<bool>();
            List<bool> locked = new List<bool>();

            float smooth = Mathf.Clamp01(smoothing);

            for (int i = 0; i < n; i++)
            {
                if (map[i] != -1) continue;

                int outIndex = newVerts.Count;
                map[i] = outIndex;

                if (IsLocked(i))
                {
                    newVerts.Add(verts[i]);
                    if (hasNormals) newNormals.Add(normals[i]);
                    if (hasUvs) newUvs.Add(uvs[i]);
                    if (hasWeights) newWeights.Add(weights[i]);
                    seam.Add(false);
                    locked.Add(true);
                    continue;
                }

                long ux = hasUvs ? UvCell(uvs[i].x) : 0;
                long uy = hasUvs ? UvCell(uvs[i].y) : 0;
                var (cx, cy, cz) = PosCell(verts[i]);

                Vector3 centroid = verts[i];
                int clusterCount = 1;

                for (long dz = -1; dz <= 1; dz++)
                for (long dy = -1; dy <= 1; dy++)
                for (long dx = -1; dx <= 1; dx++)
                {
                    var key = (ux, uy, cx + dx, cy + dy, cz + dz);
                    if (!buckets.TryGetValue(key, out var list)) continue;

                    foreach (int j in list)
                    {
                        if (j == i || map[j] != -1) continue;
                        if ((verts[j] - verts[i]).sqrMagnitude <= sqrThreshold)
                        {
                            map[j] = outIndex;
                            centroid += verts[j];
                            clusterCount++;
                        }
                    }
                }

                Vector3 outPos = verts[i];
                if (clusterCount > 1 && smooth > 0f)
                {
                    centroid /= clusterCount;
                    outPos = Vector3.Lerp(verts[i], centroid, smooth);
                }

                newVerts.Add(outPos);
                if (hasNormals) newNormals.Add(normals[i]);
                if (hasUvs) newUvs.Add(uvs[i]);
                if (hasWeights) newWeights.Add(weights[i]);
                seam.Add(clusterCount > 1);
                locked.Add(false);
            }

            var newSubmeshes = new List<int[]>(input.Submeshes.Count);
            int triCount = 0;
            for (int s = 0; s < input.Submeshes.Count; s++)
            {
                int[] old = input.Submeshes[s];
                List<int> tris = new List<int>(old.Length);
                for (int t = 0; t < old.Length; t += 3)
                {
                    int a = map[old[t]];
                    int b = map[old[t + 1]];
                    int c = map[old[t + 2]];
                    if (a != b && b != c && c != a)
                    {
                        tris.Add(a); tris.Add(b); tris.Add(c);
                        triCount++;
                    }
                }
                newSubmeshes.Add(tris.ToArray());
            }

            return new WeldPass
            {
                Geometry = new WeldGeometry
                {
                    Verts = newVerts.ToArray(),
                    Normals = hasNormals ? newNormals.ToArray() : null,
                    Uvs = hasUvs ? newUvs.ToArray() : null,
                    Weights = hasWeights ? newWeights.ToArray() : null,
                    Submeshes = newSubmeshes
                },
                Map = map,
                TriangleCount = triCount,
                SeamVertex = seam.ToArray(),
                LockedVertex = locked.ToArray()
            };
        }

        private static void LaplacianRelax(WeldPass pass, int iterations, float factor)
        {
            WeldGeometry geo = pass.Geometry;
            Vector3[] verts = geo.Verts;
            int n = verts.Length;
            float f = Mathf.Clamp01(factor);
            if (n == 0 || iterations <= 0 || f <= 0f) return;

            var neighbors = new HashSet<int>[n];
            for (int i = 0; i < n; i++) neighbors[i] = new HashSet<int>();

            foreach (int[] sub in geo.Submeshes)
            {
                for (int t = 0; t < sub.Length; t += 3)
                {
                    int a = sub[t], b = sub[t + 1], c = sub[t + 2];
                    neighbors[a].Add(b); neighbors[a].Add(c);
                    neighbors[b].Add(a); neighbors[b].Add(c);
                    neighbors[c].Add(a); neighbors[c].Add(b);
                }
            }

            bool[] move = new bool[n];
            for (int i = 0; i < n; i++)
                move[i] = pass.SeamVertex[i] && !pass.LockedVertex[i];

            for (int it = 0; it < iterations; it++)
            {
                Vector3[] updated = new Vector3[n];
                for (int i = 0; i < n; i++)
                {
                    if (!move[i] || neighbors[i].Count == 0) { updated[i] = verts[i]; continue; }

                    Vector3 avg = Vector3.zero;
                    foreach (int nb in neighbors[i]) avg += verts[nb];
                    avg /= neighbors[i].Count;
                    updated[i] = Vector3.Lerp(verts[i], avg, f);
                }
                verts = updated;
            }

            geo.Verts = verts;
        }

        private static void ApplyGeometry(Mesh mesh, WeldGeometry geo)
        {
            mesh.Clear();
            mesh.vertices = geo.Verts;
            if (geo.Normals != null && geo.Normals.Length > 0) mesh.normals = geo.Normals;
            if (geo.Uvs != null && geo.Uvs.Length > 0) mesh.uv = geo.Uvs;
            if (geo.Weights != null && geo.Weights.Length > 0) mesh.boneWeights = geo.Weights;

            mesh.subMeshCount = geo.Submeshes.Count;
            for (int s = 0; s < geo.Submeshes.Count; s++) mesh.SetTriangles(geo.Submeshes[s], s);
            mesh.RecalculateNormals();
        }

        private static void RemapBlendShapes(Mesh mesh, List<BlendShapeExtract> shapes, int[] finalMap, int originalCount, int newCount)
        {
            foreach (var shape in shapes)
            {
                foreach (var frame in shape.Frames)
                {
                    Vector3[] newDV = new Vector3[newCount];
                    Vector3[] newDN = new Vector3[newCount];
                    Vector3[] newDT = new Vector3[newCount];
                    int[] mergeCounts = new int[newCount];

                    for (int i = 0; i < originalCount; i++)
                    {
                        int targetIdx = finalMap[i];
                        newDV[targetIdx] += frame.DeltaVerts[i];
                        newDN[targetIdx] += frame.DeltaNormals[i];
                        newDT[targetIdx] += frame.DeltaTangents[i];
                        mergeCounts[targetIdx]++;
                    }

                    for (int j = 0; j < newCount; j++)
                    {
                        if (mergeCounts[j] > 0)
                        {
                            newDV[j] /= mergeCounts[j];
                            newDN[j] /= mergeCounts[j];
                            newDT[j] /= mergeCounts[j];
                        }
                    }

                    mesh.AddBlendShapeFrame(shape.Name, frame.Weight, newDV, newDN, newDT);
                }
            }
        }

        private static List<BlendShapeExtract> ExtractBlendShapes(Mesh mesh, out int vertexCount)
        {
            vertexCount = mesh.vertexCount;
            int n = vertexCount;
            int count = mesh.blendShapeCount;
            var list = new List<BlendShapeExtract>(count);

            for (int b = 0; b < count; b++)
            {
                string shapeName = mesh.GetBlendShapeName(b);
                int frameCount = mesh.GetBlendShapeFrameCount(b);
                var frames = new List<BlendShapeFrame>(frameCount);

                for (int f = 0; f < frameCount; f++)
                {
                    float weight = mesh.GetBlendShapeFrameWeight(b, f);
                    Vector3[] deltaVerts = new Vector3[n];
                    Vector3[] deltaNormals = new Vector3[n];
                    Vector3[] deltaTangents = new Vector3[n];

                    mesh.GetBlendShapeFrameVertices(b, f, deltaVerts, deltaNormals, deltaTangents);
                    frames.Add(new BlendShapeFrame { Weight = weight, DeltaVerts = deltaVerts, DeltaNormals = deltaNormals, DeltaTangents = deltaTangents });
                }
                list.Add(new BlendShapeExtract { Name = shapeName, Frames = frames });
            }
            return list;
        }

        private static List<int[]> GetSubmeshes(Mesh mesh)
        {
            var list = new List<int[]>(mesh.subMeshCount);
            for (int s = 0; s < mesh.subMeshCount; s++) list.Add(mesh.GetTriangles(s));
            return list;
        }

        private static int CountTriangles(List<int[]> submeshes)
        {
            int c = 0;
            foreach (int[] sub in submeshes) c += sub.Length / 3;
            return c;
        }

        private class WeldGeometry
        {
            public Vector3[] Verts;
            public Vector3[] Normals;
            public Vector2[] Uvs;
            public BoneWeight[] Weights;
            public List<int[]> Submeshes;
        }

        private class WeldPass
        {
            public WeldGeometry Geometry;
            public int[] Map;
            public int TriangleCount;
            public bool[] SeamVertex;
            public bool[] LockedVertex;
        }

        public static HashSet<int> GenerateProtectedBoneIndices(Animator animator, SkinnedMeshRenderer smr, params HumanBodyBones[] protectedHumanBones)
        {
            HashSet<int> protectedIndices = new HashSet<int>();

            if (animator == null || !animator.isHuman || smr == null)
            {
                Debug.LogWarning("[VixForge] Warning: Missing Animator, Non-Humanoid Rig, or missing SMR. Returning empty protection system.");
                return protectedIndices;
            }

            HashSet<Transform> protectedTransforms = new HashSet<Transform>();

            foreach (var humanBone in protectedHumanBones)
            {
                Transform boneTransform = animator.GetBoneTransform(humanBone);
                if (boneTransform != null)
                {
                    CollectTransformsRecursive(boneTransform, protectedTransforms);
                }
                else
                {
                    Debug.LogWarning($"[VixForge] HumanBodyBone {humanBone} not mapped in Animator.");
                }
            }

            Transform[] smrBones = smr.bones;
            for (int i = 0; i < smrBones.Length; i++)
            {
                if (smrBones[i] != null && protectedTransforms.Contains(smrBones[i]))
                {
                    protectedIndices.Add(i);
                }
            }

            Debug.Log($"[VixForge] Kinematic Protection System generated: {protectedIndices.Count} structural bones locked.");
            return protectedIndices;
        }

        private static void CollectTransformsRecursive(Transform current, HashSet<Transform> collection)
        {
            collection.Add(current);
            foreach (Transform child in current)
            {
                CollectTransformsRecursive(child, collection);
            }
        }

        private static int GetDominantBone(BoneWeight bw)
        {
            float maxWeight = bw.weight0;
            int maxIndex = bw.boneIndex0;

            if (bw.weight1 > maxWeight) { maxWeight = bw.weight1; maxIndex = bw.boneIndex1; }
            if (bw.weight2 > maxWeight) { maxWeight = bw.weight2; maxIndex = bw.boneIndex2; }
            if (bw.weight3 > maxWeight) { maxWeight = bw.weight3; maxIndex = bw.boneIndex3; }

            return maxIndex;
        }

        private struct BlendShapeExtract
        {
            public string Name;
            public List<BlendShapeFrame> Frames;
        }

        private struct BlendShapeFrame
        {
            public float Weight;
            public Vector3[] DeltaVerts;
            public Vector3[] DeltaNormals;
            public Vector3[] DeltaTangents;
        }

        public static void CollapseBonesToParent(SkinnedMeshRenderer smr, List<Transform> bonesToCull)
        {
            PatchSkinnedMesh(smr, "BoneCollapse", (mesh, renderer) =>
            {
                Transform[] currentBones = renderer.bones;
                BoneWeight[] weights = mesh.boneWeights;

                Dictionary<Transform, int> boneIndexMap = new Dictionary<Transform, int>();
                for (int i = 0; i < currentBones.Length; i++)
                {
                    if (currentBones[i] != null)
                    {
                        boneIndexMap[currentBones[i]] = i;
                    }
                }

                for (int i = 0; i < weights.Length; i++)
                {
                    BoneWeight w = weights[i];
                    w = ProcessWeightChannel(w, 0, currentBones, bonesToCull, boneIndexMap);
                    w = ProcessWeightChannel(w, 1, currentBones, bonesToCull, boneIndexMap);
                    w = ProcessWeightChannel(w, 2, currentBones, bonesToCull, boneIndexMap);
                    w = ProcessWeightChannel(w, 3, currentBones, bonesToCull, boneIndexMap);
                    weights[i] = w;
                }
                mesh.boneWeights = weights;
            });
        }

        private static BoneWeight ProcessWeightChannel(BoneWeight w, int channel, Transform[] allBones, List<Transform> cullList, Dictionary<Transform, int> indexMap)
        {
            int targetIndex = channel == 0 ? w.boneIndex0 : channel == 1 ? w.boneIndex1 : channel == 2 ? w.boneIndex2 : w.boneIndex3;
            float weight = channel == 0 ? w.weight0 : channel == 1 ? w.weight1 : channel == 2 ? w.weight2 : w.weight3;

            if (weight > 0 && targetIndex >= 0 && targetIndex < allBones.Length)
            {
                Transform currentBone = allBones[targetIndex];

                if (currentBone != null && cullList.Contains(currentBone) && currentBone.parent != null)
                {
                    if (indexMap.TryGetValue(currentBone.parent, out int parentIndex))
                    {
                        if (channel == 0) w.boneIndex0 = parentIndex;
                        else if (channel == 1) w.boneIndex1 = parentIndex;
                        else if (channel == 2) w.boneIndex2 = parentIndex;
                        else w.boneIndex3 = parentIndex;
                    }
                }
            }
            return w;
        }

        private static class QuadricSimplifier
        {
            private struct SymmetricMatrix
            {
                public double m0, m1, m2, m3, m4, m5, m6, m7, m8, m9;

                public SymmetricMatrix(double c) { m0 = m1 = m2 = m3 = m4 = m5 = m6 = m7 = m8 = m9 = c; }

                public SymmetricMatrix(double a, double b, double c, double d)
                {
                    m0 = a * a; m1 = a * b; m2 = a * c; m3 = a * d;
                    m4 = b * b; m5 = b * c; m6 = b * d;
                    m7 = c * c; m8 = c * d;
                    m9 = d * d;
                }

                public SymmetricMatrix(double n0, double n1, double n2, double n3, double n4, double n5, double n6, double n7, double n8, double n9)
                {
                    m0 = n0; m1 = n1; m2 = n2; m3 = n3; m4 = n4; m5 = n5; m6 = n6; m7 = n7; m8 = n8; m9 = n9;
                }

                public double this[int i]
                {
                    get
                    {
                        switch (i)
                        {
                            case 0: return m0; case 1: return m1; case 2: return m2; case 3: return m3;
                            case 4: return m4; case 5: return m5; case 6: return m6; case 7: return m7;
                            case 8: return m8; default: return m9;
                        }
                    }
                }

                public double Det(int a11, int a12, int a13, int a21, int a22, int a23, int a31, int a32, int a33)
                {
                    return this[a11] * this[a22] * this[a33] + this[a13] * this[a21] * this[a32] + this[a12] * this[a23] * this[a31]
                         - this[a13] * this[a22] * this[a31] - this[a11] * this[a23] * this[a32] - this[a12] * this[a21] * this[a33];
                }

                public static SymmetricMatrix operator +(SymmetricMatrix a, SymmetricMatrix b)
                {
                    return new SymmetricMatrix(
                        a.m0 + b.m0, a.m1 + b.m1, a.m2 + b.m2, a.m3 + b.m3,
                        a.m4 + b.m4, a.m5 + b.m5, a.m6 + b.m6,
                        a.m7 + b.m7, a.m8 + b.m8, a.m9 + b.m9);
                }
            }

            private class SVert
            {
                public Vector3 p;
                public int tstart;
                public int tcount;
                public SymmetricMatrix q;
                public bool border;
                public bool locked;
            }

            private class STri
            {
                public int v0, v1, v2;
                public double err0, err1, err2, err3;
                public bool deleted;
                public bool dirty;
                public Vector3 n;
                public int submesh;
            }

            private struct SRef { public int tid; public int tvertex; }

            private static List<SVert> verts;
            private static List<STri> tris;
            private static List<SRef> refs;
            private static Vector2[] uv;
            private static BoneWeight[] bw;
            private static Color[] colors;
            private static bool hasUv;
            private static bool hasBw;
            private static bool hasColor;
            private static int[] parent;

            private const double OptimizeEpsilon = 1e-12;

            private static int Find(int x)
            {
                while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; }
                return x;
            }

            private static void MarkVertSubmesh(int[] vertSub, bool[] lockedVerts, int v, int submesh)
            {
                if (vertSub[v] == -1) vertSub[v] = submesh;
                else if (vertSub[v] != submesh) lockedVerts[v] = true;
            }

            public static void Simplify(Mesh mesh, int targetTriangles, double aggressiveness, bool preserveBorders,
                HashSet<int> protectedSubmeshes, HashSet<int> protectedBones, int smoothIterations,
                out int[] finalMap, out int newCount, out int finalTriangles)
            {
                Vector3[] positions = mesh.vertices;
                int vcountOrig = positions.Length;

                uv = mesh.uv; hasUv = uv != null && uv.Length == vcountOrig;
                bw = mesh.boneWeights; hasBw = bw != null && bw.Length == vcountOrig;
                colors = mesh.colors; hasColor = colors != null && colors.Length == vcountOrig;

                verts = new List<SVert>(vcountOrig);
                for (int i = 0; i < vcountOrig; i++) verts.Add(new SVert { p = positions[i], q = new SymmetricMatrix(0.0) });

                parent = new int[vcountOrig];
                for (int i = 0; i < vcountOrig; i++) parent[i] = i;

                int subCount = mesh.subMeshCount;
                tris = new List<STri>();
                var lockedVerts = new bool[vcountOrig];

                for (int s = 0; s < subCount; s++)
                {
                    int[] idx = mesh.GetTriangles(s);
                    bool subLocked = protectedSubmeshes.Contains(s);
                    for (int t = 0; t + 2 < idx.Length; t += 3)
                    {
                        tris.Add(new STri { v0 = idx[t], v1 = idx[t + 1], v2 = idx[t + 2], submesh = s });
                        if (subLocked) { lockedVerts[idx[t]] = true; lockedVerts[idx[t + 1]] = true; lockedVerts[idx[t + 2]] = true; }
                    }
                }

                if (subCount > 1)
                {
                    int[] vertSub = new int[vcountOrig];
                    for (int i = 0; i < vcountOrig; i++) vertSub[i] = -1;
                    foreach (var t in tris)
                    {
                        MarkVertSubmesh(vertSub, lockedVerts, t.v0, t.submesh);
                        MarkVertSubmesh(vertSub, lockedVerts, t.v1, t.submesh);
                        MarkVertSubmesh(vertSub, lockedVerts, t.v2, t.submesh);
                    }
                }

                if (hasBw && protectedBones.Count > 0)
                {
                    for (int i = 0; i < vcountOrig; i++)
                    {
                        BoneWeight w = bw[i];
                        if ((w.weight0 > 0 && protectedBones.Contains(w.boneIndex0)) ||
                            (w.weight1 > 0 && protectedBones.Contains(w.boneIndex1)) ||
                            (w.weight2 > 0 && protectedBones.Contains(w.boneIndex2)) ||
                            (w.weight3 > 0 && protectedBones.Contains(w.boneIndex3)))
                            lockedVerts[i] = true;
                    }
                }
                for (int i = 0; i < vcountOrig; i++) verts[i].locked = lockedVerts[i];

                refs = new List<SRef>();

                int deletedTriangles = 0;
                int triangleCount = tris.Count;
                const int maxIterations = 100;

                var deleted0 = new List<bool>();
                var deleted1 = new List<bool>();

                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    if (triangleCount - deletedTriangles <= targetTriangles) break;

                    if (iteration % 5 == 0) UpdateMesh(iteration, preserveBorders);

                    for (int i = 0; i < tris.Count; i++) tris[i].dirty = false;

                    double threshold = 0.000000001 * Math.Pow(iteration + 3, aggressiveness);

                    for (int i = 0; i < tris.Count; i++)
                    {
                        STri t = tris[i];
                        if (t.err3 > threshold || t.deleted || t.dirty) continue;

                        for (int j = 0; j < 3; j++)
                        {
                            double ej = j == 0 ? t.err0 : (j == 1 ? t.err1 : t.err2);
                            if (ej > threshold) continue;

                            int i0 = j == 0 ? t.v0 : (j == 1 ? t.v1 : t.v2);
                            int i1 = j == 0 ? t.v1 : (j == 1 ? t.v2 : t.v0);

                            SVert v0 = verts[i0];
                            SVert v1 = verts[i1];

                            if (v0.locked || v1.locked) continue;
                            if (v0.border || v1.border) continue;

                            CalcError(i0, i1, out Vector3 p);

                            Resize(deleted0, v0.tcount);
                            Resize(deleted1, v1.tcount);

                            if (Flipped(p, i0, i1, v0, deleted0)) continue;
                            if (Flipped(p, i1, i0, v1, deleted1)) continue;

                            Vector3 e = v1.p - v0.p;
                            float denom = (float)e.sqrMagnitude;
                            float tt = denom > 1e-12f ? Mathf.Clamp01(Vector3.Dot(p - v0.p, e) / denom) : 0.5f;
                            if (hasUv) uv[i0] = Vector2.Lerp(uv[i0], uv[i1], tt);
                            if (hasBw) bw[i0] = BlendBoneWeights(bw[i0], bw[i1], tt);
                            if (hasColor) colors[i0] = Color.Lerp(colors[i0], colors[i1], tt);

                            v0.p = p;
                            v0.q = v1.q + v0.q;

                            int tstart = refs.Count;
                            UpdateTriangles(i0, v0, deleted0, ref deletedTriangles);
                            UpdateTriangles(i0, v1, deleted1, ref deletedTriangles);

                            v0.tstart = tstart;
                            v0.tcount = refs.Count - tstart;

                            parent[i1] = i0;
                            break;
                        }

                        if (triangleCount - deletedTriangles <= targetTriangles) break;
                    }
                }

                if (smoothIterations > 0) SmoothTaubin(smoothIterations, 0.5f, -0.53f);

                CompactMesh(mesh, out finalMap, out newCount, out finalTriangles, vcountOrig, subCount);
            }

            private static void SmoothTaubin(int iterations, float lambda, float mu)
            {
                int n = verts.Count;
                var nb = new HashSet<int>[n];

                foreach (var t in tris)
                {
                    if (t.deleted) continue;
                    AddNeighbor(nb, t.v0, t.v1); AddNeighbor(nb, t.v0, t.v2);
                    AddNeighbor(nb, t.v1, t.v0); AddNeighbor(nb, t.v1, t.v2);
                    AddNeighbor(nb, t.v2, t.v0); AddNeighbor(nb, t.v2, t.v1);
                }

                Vector3[] pos = new Vector3[n];
                for (int i = 0; i < n; i++) pos[i] = verts[i].p;

                for (int it = 0; it < iterations; it++)
                {
                    float factor = (it % 2 == 0) ? lambda : mu;
                    Vector3[] updated = new Vector3[n];
                    for (int i = 0; i < n; i++)
                    {
                        SVert v = verts[i];
                        if (v.locked || v.border || nb[i] == null || nb[i].Count == 0)
                        {
                            updated[i] = pos[i];
                            continue;
                        }

                        Vector3 avg = Vector3.zero;
                        foreach (int j in nb[i]) avg += pos[j];
                        avg /= nb[i].Count;
                        updated[i] = pos[i] + (avg - pos[i]) * factor;
                    }
                    pos = updated;
                }

                for (int i = 0; i < n; i++) verts[i].p = pos[i];
            }

            private static void AddNeighbor(HashSet<int>[] nb, int a, int b)
            {
                if (nb[a] == null) nb[a] = new HashSet<int>();
                nb[a].Add(b);
            }

            private static void Resize(List<bool> list, int size)
            {
                if (list.Count > size) list.RemoveRange(size, list.Count - size);
                else while (list.Count < size) list.Add(false);
            }

            private static double VertexError(SymmetricMatrix q, double x, double y, double z)
            {
                return q.m0 * x * x + 2 * q.m1 * x * y + 2 * q.m2 * x * z + 2 * q.m3 * x
                     + q.m4 * y * y + 2 * q.m5 * y * z + 2 * q.m6 * y
                     + q.m7 * z * z + 2 * q.m8 * z + q.m9;
            }

            private static double CalcError(int idV1, int idV2, out Vector3 result)
            {
                SymmetricMatrix q = verts[idV1].q + verts[idV2].q;
                bool border = verts[idV1].border && verts[idV2].border;
                double det = q.Det(0, 1, 2, 1, 4, 5, 2, 5, 7);

                Vector3 p1 = verts[idV1].p;
                Vector3 p2 = verts[idV2].p;
                Vector3 mid = (p1 + p2) * 0.5f;

                if (Math.Abs(det) > OptimizeEpsilon && !border)
                {
                    double x = -1 / det * q.Det(1, 2, 3, 4, 5, 6, 5, 7, 8);
                    double y = 1 / det * q.Det(0, 2, 3, 1, 5, 6, 2, 7, 8);
                    double z = -1 / det * q.Det(0, 1, 3, 1, 4, 6, 2, 5, 8);

                    bool finite = !double.IsNaN(x) && !double.IsInfinity(x) &&
                                  !double.IsNaN(y) && !double.IsInfinity(y) &&
                                  !double.IsNaN(z) && !double.IsInfinity(z);

                    if (finite)
                    {
                        Vector3 opt = new Vector3((float)x, (float)y, (float)z);
                        float maxOff = Mathf.Max(Vector3.Distance(p1, p2), 1e-6f);
                        if (Vector3.Distance(opt, mid) <= maxOff)
                        {
                            result = opt;
                            return VertexError(q, x, y, z);
                        }
                    }
                }

                Vector3 p3 = mid;
                double e1 = VertexError(q, p1.x, p1.y, p1.z);
                double e2 = VertexError(q, p2.x, p2.y, p2.z);
                double e3 = VertexError(q, p3.x, p3.y, p3.z);
                double error = Math.Min(e1, Math.Min(e2, e3));
                if (error == e1) result = p1;
                else if (error == e2) result = p2;
                else result = p3;
                return error;
            }

            private static bool Flipped(Vector3 p, int i0, int i1, SVert v0, List<bool> deleted)
            {
                for (int k = 0; k < v0.tcount; k++)
                {
                    SRef r = refs[v0.tstart + k];
                    STri t = tris[r.tid];
                    if (t.deleted) continue;

                    int s = r.tvertex;
                    int id1 = s == 0 ? t.v1 : (s == 1 ? t.v2 : t.v0);
                    int id2 = s == 0 ? t.v2 : (s == 1 ? t.v0 : t.v1);

                    if (id1 == i1 || id2 == i1) { deleted[k] = true; continue; }

                    Vector3 d1 = (verts[id1].p - p).normalized;
                    Vector3 d2 = (verts[id2].p - p).normalized;
                    if (Mathf.Abs(Vector3.Dot(d1, d2)) > 0.99f) return true;

                    Vector3 n = Vector3.Cross(d1, d2).normalized;
                    deleted[k] = false;
                    if (Vector3.Dot(n, t.n) < 0.2f) return true;
                }
                return false;
            }

            private static void UpdateTriangles(int i0, SVert v, List<bool> deleted, ref int deletedTriangles)
            {
                for (int k = 0; k < v.tcount; k++)
                {
                    SRef r = refs[v.tstart + k];
                    STri t = tris[r.tid];
                    if (t.deleted) continue;
                    if (deleted[k]) { t.deleted = true; deletedTriangles++; continue; }

                    if (r.tvertex == 0) t.v0 = i0;
                    else if (r.tvertex == 1) t.v1 = i0;
                    else t.v2 = i0;

                    Vector3 np0 = verts[t.v0].p, np1 = verts[t.v1].p, np2 = verts[t.v2].p;
                    t.n = Vector3.Cross(np1 - np0, np2 - np0).normalized;

                    t.dirty = true;
                    t.err0 = CalcError(t.v0, t.v1, out _);
                    t.err1 = CalcError(t.v1, t.v2, out _);
                    t.err2 = CalcError(t.v2, t.v0, out _);
                    t.err3 = Math.Min(t.err0, Math.Min(t.err1, t.err2));
                    refs.Add(r);
                }
            }

            private static void UpdateMesh(int iteration, bool preserveBorders)
            {
                if (iteration > 0)
                {
                    int dst = 0;
                    for (int i = 0; i < tris.Count; i++) if (!tris[i].deleted) tris[dst++] = tris[i];
                    if (dst < tris.Count) tris.RemoveRange(dst, tris.Count - dst);
                }

                if (iteration == 0)
                {
                    foreach (var v in verts) v.q = new SymmetricMatrix(0.0);

                    foreach (var t in tris)
                    {
                        Vector3 p0 = verts[t.v0].p, p1 = verts[t.v1].p, p2 = verts[t.v2].p;
                        Vector3 n = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                        t.n = n;
                        double d = -Vector3.Dot(n, p0);
                        var kp = new SymmetricMatrix((double)n.x, (double)n.y, (double)n.z, d);
                        verts[t.v0].q += kp; verts[t.v1].q += kp; verts[t.v2].q += kp;
                    }

                    foreach (var t in tris)
                    {
                        t.err0 = CalcError(t.v0, t.v1, out _);
                        t.err1 = CalcError(t.v1, t.v2, out _);
                        t.err2 = CalcError(t.v2, t.v0, out _);
                        t.err3 = Math.Min(t.err0, Math.Min(t.err1, t.err2));
                    }
                }

                for (int i = 0; i < verts.Count; i++) { verts[i].tstart = 0; verts[i].tcount = 0; }
                foreach (var t in tris) { verts[t.v0].tcount++; verts[t.v1].tcount++; verts[t.v2].tcount++; }

                int start = 0;
                for (int i = 0; i < verts.Count; i++)
                {
                    SVert v = verts[i];
                    v.tstart = start;
                    start += v.tcount;
                    v.tcount = 0;
                }

                var refArr = new SRef[start];
                for (int i = 0; i < tris.Count; i++)
                {
                    STri t = tris[i];
                    SVert a = verts[t.v0]; refArr[a.tstart + a.tcount] = new SRef { tid = i, tvertex = 0 }; a.tcount++;
                    SVert b = verts[t.v1]; refArr[b.tstart + b.tcount] = new SRef { tid = i, tvertex = 1 }; b.tcount++;
                    SVert c = verts[t.v2]; refArr[c.tstart + c.tcount] = new SRef { tid = i, tvertex = 2 }; c.tcount++;
                }
                refs = new List<SRef>(refArr);

                if (iteration == 0 && preserveBorders)
                {
                    var vcount = new List<int>();
                    var vids = new List<int>();
                    foreach (var v in verts) v.border = false;

                    for (int i = 0; i < verts.Count; i++)
                    {
                        SVert v = verts[i];
                        vcount.Clear();
                        vids.Clear();
                        for (int k = 0; k < v.tcount; k++)
                        {
                            STri t = tris[refs[v.tstart + k].tid];
                            for (int j = 0; j < 3; j++)
                            {
                                int id = j == 0 ? t.v0 : (j == 1 ? t.v1 : t.v2);
                                if (id == i) continue;
                                int ofs = 0;
                                bool found = false;
                                for (; ofs < vids.Count; ofs++) if (vids[ofs] == id) { found = true; break; }
                                if (!found) { vids.Add(id); vcount.Add(1); }
                                else vcount[ofs]++;
                            }
                        }
                        for (int k = 0; k < vids.Count; k++)
                            if (vcount[k] == 1) { verts[vids[k]].border = true; verts[i].border = true; }
                    }
                }
            }

            private static void CompactMesh(Mesh mesh, out int[] finalMap, out int newCount, out int finalTriangles, int originalVertCount, int subCount)
            {
                bool[] used = new bool[verts.Count];
                foreach (var t in tris)
                {
                    if (t.deleted) continue;
                    used[t.v0] = used[t.v1] = used[t.v2] = true;
                }

                int[] newIndex = new int[verts.Count];
                for (int i = 0; i < newIndex.Length; i++) newIndex[i] = -1;

                var newVerts = new List<Vector3>();
                var newUv = hasUv ? new List<Vector2>() : null;
                var newBw = hasBw ? new List<BoneWeight>() : null;
                var newColors = hasColor ? new List<Color>() : null;

                int nc = 0;
                for (int i = 0; i < verts.Count; i++)
                {
                    if (!used[i]) continue;
                    newIndex[i] = nc++;
                    newVerts.Add(verts[i].p);
                    if (hasUv) newUv.Add(uv[i]);
                    if (hasBw) newBw.Add(bw[i]);
                    if (hasColor) newColors.Add(colors[i]);
                }

                var subTris = new List<int>[subCount];
                for (int s = 0; s < subCount; s++) subTris[s] = new List<int>();

                foreach (var t in tris)
                {
                    if (t.deleted) continue;
                    int a = newIndex[t.v0], b = newIndex[t.v1], c = newIndex[t.v2];
                    if (a < 0 || b < 0 || c < 0) continue;
                    if (a == b || b == c || c == a) continue;
                    var list = subTris[t.submesh];
                    list.Add(a); list.Add(b); list.Add(c);
                }

                finalMap = new int[originalVertCount];
                for (int i = 0; i < originalVertCount; i++)
                {
                    int root = Find(i);
                    int ni = (root >= 0 && root < newIndex.Length) ? newIndex[root] : -1;
                    finalMap[i] = ni >= 0 ? ni : 0;
                }

                mesh.Clear();
                if (nc > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices(newVerts);
                if (hasUv) mesh.SetUVs(0, newUv);
                if (hasColor) mesh.SetColors(newColors);
                if (hasBw) mesh.boneWeights = newBw.ToArray();

                mesh.subMeshCount = subCount;
                finalTriangles = 0;
                for (int s = 0; s < subCount; s++)
                {
                    mesh.SetTriangles(subTris[s], s, false);
                    finalTriangles += subTris[s].Count / 3;
                }

                newCount = nc;

                verts = null; tris = null; refs = null; uv = null; bw = null; colors = null; parent = null;
            }
        }
    }
}
#endif
