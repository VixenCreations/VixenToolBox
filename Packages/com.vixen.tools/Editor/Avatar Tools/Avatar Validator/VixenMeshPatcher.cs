#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: Advanced Mesh Topology Engine.
    /// Clones FBX mesh data into memory, applies vertex/bone transformations, 
    /// and serializes the optimized mesh to disk for SDK compilation.
    /// </summary>
    public static class VixenMeshPatcher
    {
        private const string GENERATED_ASSET_PATH = "Assets/VixenTools/Meshes/Patched/";

        /// <summary>
        /// Executes a destructive patch safely by cloning the mesh, applying an action, and saving it.
        /// </summary>
        public static void PatchSkinnedMesh(SkinnedMeshRenderer smr, string patchLabel, System.Action<Mesh, SkinnedMeshRenderer> patchingLogic)
        {
            if (smr == null || smr.sharedMesh == null) return;

            // 1. Memory Clone: Instantiate bypasses the FBX read-only lock.
            Mesh clonedMesh = UnityEngine.Object.Instantiate(smr.sharedMesh);
            clonedMesh.name = $"{smr.sharedMesh.name}_VixenPatched_{patchLabel}";

            // 2. Execute Custom Topology Matrix (Vertices, UVs, Bones)
            patchingLogic?.Invoke(clonedMesh, smr);

            // 3. Recalculate structural integrity
            clonedMesh.RecalculateBounds();
            clonedMesh.RecalculateNormals(); 
            clonedMesh.RecalculateTangents();

            // 4. Persistence: Step-through folder validation to ensure the path exists
            if (!AssetDatabase.IsValidFolder("Assets/VixenTools"))
                AssetDatabase.CreateFolder("Assets", "VixenTools");
                
            if (!AssetDatabase.IsValidFolder("Assets/VixenTools/Meshes"))
                AssetDatabase.CreateFolder("Assets/VixenTools", "Meshes");
                
            if (!AssetDatabase.IsValidFolder("Assets/VixenTools/Meshes/Patched"))
                AssetDatabase.CreateFolder("Assets/VixenTools/Meshes", "Patched");

            // 5. Serialize to Disk
            string assetPath = $"{GENERATED_ASSET_PATH}{clonedMesh.name}_{System.Guid.NewGuid().ToString().Substring(0, 5)}.asset";
            AssetDatabase.CreateAsset(clonedMesh, assetPath);
            AssetDatabase.SaveAssets();

            // 6. Apply the swap with Undo support
            Undo.RecordObject(smr, "Apply Patched Mesh");
            smr.sharedMesh = clonedMesh;

            Debug.Log($"[VixenTools] Topology Patched: {clonedMesh.name} serialized to {assetPath}");
        }

        // ====================================================================
        // DESTRUCTIVE TOPOLOGY PIPELINES (VERTEX WELDING + BLENDSHAPE RECOVERY)
        // ====================================================================

        /// <summary>
        /// Precision Microwelder: Seals sub-millimeter splits while strictly preserving UV texture seams.
        /// Utilizes a 5D Hash Matrix (X, Y, Z, U, V) to ensure rendering integrity is never compromised.
        /// </summary>
        public static void WeldVertices(
            SkinnedMeshRenderer smr, 
            float threshold = 0.0001f, 
            HashSet<int> protectedSubmeshes = null, 
            HashSet<int> protectedBones = null)
        {
            if (protectedSubmeshes == null) protectedSubmeshes = new HashSet<int>();
            if (protectedBones == null) protectedBones = new HashSet<int>();

            PatchSkinnedMesh(smr, $"MicroWeld_{threshold:F4}", (mesh, renderer) =>
            {
                Vector3[] oldVerts = mesh.vertices;
                Vector3[] oldNormals = mesh.normals;
                Vector2[] oldUvs = mesh.uv;
                BoneWeight[] oldWeights = mesh.boneWeights;
                
                // 1. SURGICAL EXCLUSION SCAN (MATERIALS)
                HashSet<int> protectedVertIndices = new HashSet<int>();
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    if (protectedSubmeshes.Contains(s))
                    {
                        int[] tris = mesh.GetTriangles(s);
                        foreach (int t in tris) protectedVertIndices.Add(t);
                    }
                }

                // 2. BLENDSHAPE MEMORY EXTRACTION
                int blendShapeCount = mesh.blendShapeCount;
                var extractedBlendShapes = new List<BlendShapeExtract>();
                
                for (int b = 0; b < blendShapeCount; b++)
                {
                    string shapeName = mesh.GetBlendShapeName(b);
                    int frameCount = mesh.GetBlendShapeFrameCount(b);
                    var frames = new List<BlendShapeFrame>();

                    for (int f = 0; f < frameCount; f++)
                    {
                        float weight = mesh.GetBlendShapeFrameWeight(b, f);
                        Vector3[] deltaVerts = new Vector3[oldVerts.Length];
                        Vector3[] deltaNormals = new Vector3[oldVerts.Length];
                        Vector3[] deltaTangents = new Vector3[oldVerts.Length];
                        
                        mesh.GetBlendShapeFrameVertices(b, f, deltaVerts, deltaNormals, deltaTangents);
                        frames.Add(new BlendShapeFrame { Weight = weight, DeltaVerts = deltaVerts, DeltaNormals = deltaNormals, DeltaTangents = deltaTangents });
                    }
                    extractedBlendShapes.Add(new BlendShapeExtract { Name = shapeName, Frames = frames });
                }

                // 3. THE 5D PRECISION HASH MATRIX (Zero-GC, UV-Safe)
                List<Vector3> newVerts = new List<Vector3>();
                List<Vector3> newNormals = new List<Vector3>();
                List<Vector2> newUvs = new List<Vector2>();
                List<BoneWeight> newWeights = new List<BoneWeight>();
                
                int[] map = new int[oldVerts.Length];
                
                // UPGRADED: 5D Tuple (X, Y, Z, U, V) protects UV texture seams from collapsing.
                var spatialHash = new Dictionary<(long, long, long, long, long), int>();
                
                float multiplier = 1f / Mathf.Max(threshold, 0.00001f);
                float uvMultiplier = 10000f; // Fixed high-precision scale for UV maps (0.0 to 1.0 space)

                for (int i = 0; i < oldVerts.Length; i++)
                {
                    bool isProtected = protectedVertIndices.Contains(i);

                    if (!isProtected && protectedBones.Count > 0 && oldWeights != null && oldWeights.Length > i)
                    {
                        BoneWeight w = oldWeights[i];
                        if ((w.weight0 > 0 && protectedBones.Contains(w.boneIndex0)) ||
                            (w.weight1 > 0 && protectedBones.Contains(w.boneIndex1)) ||
                            (w.weight2 > 0 && protectedBones.Contains(w.boneIndex2)) ||
                            (w.weight3 > 0 && protectedBones.Contains(w.boneIndex3)))
                        {
                            isProtected = true;
                        }
                    }

                    (long, long, long, long, long) key;

                    if (isProtected)
                    {
                        // Absolute unique key. i guarantees it never merges.
                        key = (long.MaxValue, long.MaxValue, long.MaxValue, 0, i);
                    }
                    else
                    {
                        long hashX = (long)(Mathf.Round(oldVerts[i].x * multiplier));
                        long hashY = (long)(Mathf.Round(oldVerts[i].y * multiplier));
                        long hashZ = (long)(Mathf.Round(oldVerts[i].z * multiplier));
                        
                        long uvHashX = 0; 
                        long uvHashY = 0;
                        if (oldUvs != null && oldUvs.Length > i)
                        {
                            uvHashX = (long)(Mathf.Round(oldUvs[i].x * uvMultiplier));
                            uvHashY = (long)(Mathf.Round(oldUvs[i].y * uvMultiplier));
                        }

                        key = (hashX, hashY, hashZ, uvHashX, uvHashY);
                    }

                    if (spatialHash.TryGetValue(key, out int existingIndex))
                    {
                        map[i] = existingIndex; 
                    }
                    else
                    {
                        int newIndex = newVerts.Count;
                        spatialHash.Add(key, newIndex);
                        map[i] = newIndex;

                        newVerts.Add(oldVerts[i]);
                        if (oldNormals != null && oldNormals.Length > i) newNormals.Add(oldNormals[i]);
                        if (oldUvs != null && oldUvs.Length > i) newUvs.Add(oldUvs[i]);
                        if (oldWeights != null && oldWeights.Length > i) newWeights.Add(oldWeights[i]);
                    }
                }

                // 4. SUBMESH TRIANGLE REBUILD
                int subMeshCount = mesh.subMeshCount;
                List<int[]> newSubMeshes = new List<int[]>();

                for (int s = 0; s < subMeshCount; s++)
                {
                    int[] oldSubTris = mesh.GetTriangles(s);
                    List<int> newSubTris = new List<int>();

                    for (int i = 0; i < oldSubTris.Length; i += 3)
                    {
                        int v1 = map[oldSubTris[i]];
                        int v2 = map[oldSubTris[i + 1]];
                        int v3 = map[oldSubTris[i + 2]];

                        if (v1 != v2 && v2 != v3 && v3 != v1)
                        {
                            newSubTris.Add(v1);
                            newSubTris.Add(v2);
                            newSubTris.Add(v3);
                        }
                    }
                    newSubMeshes.Add(newSubTris.ToArray());
                }

                // 5. TOPOLOGY APPLICATION
                mesh.Clear(); 
                
                mesh.vertices = newVerts.ToArray();
                if (newNormals.Count > 0) mesh.normals = newNormals.ToArray();
                if (newUvs.Count > 0) mesh.uv = newUvs.ToArray();
                if (newWeights.Count > 0) mesh.boneWeights = newWeights.ToArray();
                
                mesh.subMeshCount = subMeshCount;
                for (int s = 0; s < subMeshCount; s++)
                {
                    mesh.SetTriangles(newSubMeshes[s], s);
                }

                mesh.RecalculateNormals(); 

                // 6. BLENDSHAPE RE-MAPPING MATRIX
                foreach (var shape in extractedBlendShapes)
                {
                    foreach (var frame in shape.Frames)
                    {
                        Vector3[] newDV = new Vector3[newVerts.Count];
                        Vector3[] newDN = new Vector3[newVerts.Count];
                        Vector3[] newDT = new Vector3[newVerts.Count];
                        int[] mergeCounts = new int[newVerts.Count];

                        for (int i = 0; i < oldVerts.Length; i++)
                        {
                            int targetIdx = map[i];
                            newDV[targetIdx] += frame.DeltaVerts[i];
                            newDN[targetIdx] += frame.DeltaNormals[i];
                            newDT[targetIdx] += frame.DeltaTangents[i];
                            mergeCounts[targetIdx]++;
                        }

                        for (int j = 0; j < newVerts.Count; j++)
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
            });
        }

        /// <summary>
        /// Vixen Core: Multi-Pass Precision Microwelder.
        /// Iteratively seals spatial seams while STRICTLY locking UV coordinates.
        /// Prioritizes absolute visual integrity over reaching polygon targets.
        /// </summary>
        public static void MultipassTargetedWeld(
            SkinnedMeshRenderer smr, 
            int targetTriangles = 15000, 
            float startThreshold = 0.0001f, 
            float maxThreshold = 0.005f, // Hard cap at 5mm. Extreme precision only.
            float step = 0.0005f, 
            HashSet<int> protectedSubmeshes = null, 
            HashSet<int> protectedBones = null)
        {
            if (protectedSubmeshes == null) protectedSubmeshes = new HashSet<int>();
            if (protectedBones == null) protectedBones = new HashSet<int>();

            PatchSkinnedMesh(smr, $"MicroWeld_{targetTriangles}Tri", (mesh, renderer) =>
            {
                // 1. INITIAL STATE EXTRACTION
                Vector3[] originalVerts = mesh.vertices;
                Vector3[] currentVerts = mesh.vertices;
                Vector3[] currentNormals = mesh.normals;
                Vector2[] currentUvs = mesh.uv;
                BoneWeight[] currentWeights = mesh.boneWeights;
                
                List<int[]> currentSubmeshes = new List<int[]>();
                for (int s = 0; s < mesh.subMeshCount; s++) currentSubmeshes.Add(mesh.GetTriangles(s));

                // 2. THE MASTER TRANSLATION MAP
                int[] masterMap = new int[originalVerts.Length];
                for (int i = 0; i < masterMap.Length; i++) masterMap[i] = i;

                // 3. BLENDSHAPE MEMORY ISOLATION
                int blendShapeCount = mesh.blendShapeCount;
                var extractedBlendShapes = new List<BlendShapeExtract>();
                
                for (int b = 0; b < blendShapeCount; b++)
                {
                    string shapeName = mesh.GetBlendShapeName(b);
                    int frameCount = mesh.GetBlendShapeFrameCount(b);
                    var frames = new List<BlendShapeFrame>();

                    for (int f = 0; f < frameCount; f++)
                    {
                        float weight = mesh.GetBlendShapeFrameWeight(b, f);
                        Vector3[] deltaVerts = new Vector3[originalVerts.Length];
                        Vector3[] deltaNormals = new Vector3[originalVerts.Length];
                        Vector3[] deltaTangents = new Vector3[originalVerts.Length];
                        
                        mesh.GetBlendShapeFrameVertices(b, f, deltaVerts, deltaNormals, deltaTangents);
                        frames.Add(new BlendShapeFrame { Weight = weight, DeltaVerts = deltaVerts, DeltaNormals = deltaNormals, DeltaTangents = deltaTangents });
                    }
                    extractedBlendShapes.Add(new BlendShapeExtract { Name = shapeName, Frames = frames });
                }
                mesh.ClearBlendShapes(); 

                // 4. THE IN-MEMORY ITERATION MATRIX (UV-Locked 5D Hash)
                int currentTriCount = mesh.triangles.Length / 3;
                float currentThreshold = startThreshold;
                int pass = 1;

                while (currentTriCount > targetTriangles && currentThreshold <= maxThreshold)
                {
                    HashSet<int> currentProtectedIndices = new HashSet<int>();
                    for (int s = 0; s < currentSubmeshes.Count; s++)
                    {
                        if (protectedSubmeshes.Contains(s))
                        {
                            foreach (int t in currentSubmeshes[s]) currentProtectedIndices.Add(t);
                        }
                    }

                    List<Vector3> newVerts = new List<Vector3>();
                    List<Vector3> newNormals = new List<Vector3>();
                    List<Vector2> newUvs = new List<Vector2>();
                    List<BoneWeight> newWeights = new List<BoneWeight>();
                    
                    int[] passMap = new int[currentVerts.Length];
                    
                    // STRICT UV LOCK: Re-introduced U and V to the matrix. Textures physically cannot tear.
                    var spatialHash = new Dictionary<(long, long, long, long, long), int>();
                    
                    float multiplier = 1f / Mathf.Max(currentThreshold, 0.00001f);
                    float uvMultiplier = 10000f; // High-precision UV quantization

                    for (int i = 0; i < currentVerts.Length; i++)
                    {
                        bool isProtected = currentProtectedIndices.Contains(i);

                        if (!isProtected && protectedBones.Count > 0 && currentWeights != null && currentWeights.Length > i)
                        {
                            BoneWeight w = currentWeights[i];
                            if ((w.weight0 > 0 && protectedBones.Contains(w.boneIndex0)) ||
                                (w.weight1 > 0 && protectedBones.Contains(w.boneIndex1)) ||
                                (w.weight2 > 0 && protectedBones.Contains(w.boneIndex2)) ||
                                (w.weight3 > 0 && protectedBones.Contains(w.boneIndex3)))
                            {
                                isProtected = true;
                            }
                        }

                        (long, long, long, long, long) key;

                        if (isProtected)
                        {
                            key = (long.MaxValue, long.MaxValue, long.MaxValue, 0, i);
                        }
                        else
                        {
                            long hashX = (long)(Mathf.Round(currentVerts[i].x * multiplier));
                            long hashY = (long)(Mathf.Round(currentVerts[i].y * multiplier));
                            long hashZ = (long)(Mathf.Round(currentVerts[i].z * multiplier));
                            
                            long uvHashX = 0; long uvHashY = 0;
                            if (currentUvs != null && currentUvs.Length > i)
                            {
                                uvHashX = (long)(Mathf.Round(currentUvs[i].x * uvMultiplier));
                                uvHashY = (long)(Mathf.Round(currentUvs[i].y * uvMultiplier));
                            }

                            key = (hashX, hashY, hashZ, uvHashX, uvHashY);
                        }

                        if (spatialHash.TryGetValue(key, out int existingIndex))
                        {
                            passMap[i] = existingIndex; 
                        }
                        else
                        {
                            int newIndex = newVerts.Count;
                            spatialHash.Add(key, newIndex);
                            passMap[i] = newIndex;

                            newVerts.Add(currentVerts[i]);
                            if (currentNormals != null && currentNormals.Length > i) newNormals.Add(currentNormals[i]);
                            if (currentUvs != null && currentUvs.Length > i) newUvs.Add(currentUvs[i]);
                            if (currentWeights != null && currentWeights.Length > i) newWeights.Add(currentWeights[i]);
                        }
                    }

                    int newTriCount = 0;
                    List<int[]> newSubmeshes = new List<int[]>();
                    for (int s = 0; s < currentSubmeshes.Count; s++)
                    {
                        int[] oldSubTris = currentSubmeshes[s];
                        List<int> newSubTris = new List<int>();

                        for (int i = 0; i < oldSubTris.Length; i += 3)
                        {
                            int v1 = passMap[oldSubTris[i]];
                            int v2 = passMap[oldSubTris[i + 1]];
                            int v3 = passMap[oldSubTris[i + 2]];

                            if (v1 != v2 && v2 != v3 && v3 != v1)
                            {
                                newSubTris.Add(v1); newSubTris.Add(v2); newSubTris.Add(v3);
                                newTriCount++;
                            }
                        }
                        newSubmeshes.Add(newSubTris.ToArray());
                    }

                    for (int i = 0; i < masterMap.Length; i++) masterMap[i] = passMap[masterMap[i]];

                    currentVerts = newVerts.ToArray();
                    currentNormals = newNormals.ToArray();
                    currentUvs = newUvs.ToArray();
                    currentWeights = newWeights.ToArray();
                    currentSubmeshes = newSubmeshes;
                    currentTriCount = newTriCount;
                    
                    currentThreshold += step;
                    pass++;
                }

                // 5. TOPOLOGY APPLICATION
                mesh.Clear(); 
                mesh.vertices = currentVerts;
                if (currentNormals.Length > 0) mesh.normals = currentNormals;
                if (currentUvs.Length > 0) mesh.uv = currentUvs;
                if (currentWeights.Length > 0) mesh.boneWeights = currentWeights;
                
                mesh.subMeshCount = currentSubmeshes.Count;
                for (int s = 0; s < currentSubmeshes.Count; s++) mesh.SetTriangles(currentSubmeshes[s], s);
                mesh.RecalculateNormals(); 

                // 6. MASTER BLENDSHAPE RE-MAPPING
                foreach (var shape in extractedBlendShapes)
                {
                    foreach (var frame in shape.Frames)
                    {
                        Vector3[] newDV = new Vector3[currentVerts.Length];
                        Vector3[] newDN = new Vector3[currentVerts.Length];
                        Vector3[] newDT = new Vector3[currentVerts.Length];
                        int[] mergeCounts = new int[currentVerts.Length];

                        for (int i = 0; i < originalVerts.Length; i++)
                        {
                            int targetIdx = masterMap[i];
                            newDV[targetIdx] += frame.DeltaVerts[i];
                            newDN[targetIdx] += frame.DeltaNormals[i];
                            newDT[targetIdx] += frame.DeltaTangents[i];
                            mergeCounts[targetIdx]++;
                        }

                        for (int j = 0; j < currentVerts.Length; j++)
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
                
                Debug.Log($"[VixenTools] Precision Microweld Halted. Passes: {pass}. Final Tris: {currentTriCount}");
            });
        }

        /// <summary>
        /// Evaluates a BoneWeight struct to determine which bone has the highest 
        /// structural influence over the vertex. Used for kinematic isolation.
        /// </summary>
        private static int GetDominantBone(BoneWeight bw)
        {
            float maxWeight = bw.weight0;
            int maxIndex = bw.boneIndex0;

            if (bw.weight1 > maxWeight) { maxWeight = bw.weight1; maxIndex = bw.boneIndex1; }
            if (bw.weight2 > maxWeight) { maxWeight = bw.weight2; maxIndex = bw.boneIndex2; }
            if (bw.weight3 > maxWeight) { maxWeight = bw.weight3; maxIndex = bw.boneIndex3; }

            return maxIndex;
        }

        // --- Data structures for the BlendShape memory cache ---
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

        // ====================================================================
        // KINEMATIC OPTIMIZATION PIPELINES
        // ====================================================================

        /// <summary>
        /// Heuristic to collapse specific bones and transfer their vertex weights to a parent bone.
        /// </summary>
        public static void CollapseBonesToParent(SkinnedMeshRenderer smr, List<Transform> bonesToCull)
        {
            PatchSkinnedMesh(smr, "BoneCollapse", (mesh, renderer) => 
            {
                Transform[] currentBones = renderer.bones;
                BoneWeight[] weights = mesh.boneWeights;
                
                Dictionary<Transform, int> boneIndexMap = new Dictionary<Transform, int>();
                for (int i = 0; i < currentBones.Length; i++) boneIndexMap[currentBones[i]] = i;

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
                if (cullList.Contains(currentBone) && currentBone.parent != null)
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
    }
}
#endif