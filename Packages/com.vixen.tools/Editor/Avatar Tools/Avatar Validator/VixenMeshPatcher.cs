#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

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
            Mesh clonedMesh = Object.Instantiate(smr.sharedMesh);
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
        /// Highly destructive 5D Spatial Hash Welder. Features a Dual-Shielding Matrix:
        /// Protects vertices based on Material Submeshes AND Kinematic Bone Weights.
        /// </summary>
        public static void WeldVertices(SkinnedMeshRenderer smr, float threshold = 0.01f, HashSet<int> protectedSubmeshes = null, HashSet<int> protectedBones = null)
        {
            if (protectedSubmeshes == null) protectedSubmeshes = new HashSet<int>();
            if (protectedBones == null) protectedBones = new HashSet<int>();

            PatchSkinnedMesh(smr, $"VertWeld_{threshold:F3}", (mesh, renderer) =>
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

                // 3. THE 5D SPATIAL HASH MATRIX
                List<Vector3> newVerts = new List<Vector3>();
                List<Vector3> newNormals = new List<Vector3>();
                List<Vector2> newUvs = new List<Vector2>();
                List<BoneWeight> newWeights = new List<BoneWeight>();
                
                int[] map = new int[oldVerts.Length];
                Dictionary<string, int> spatialHash = new Dictionary<string, int>();
                float uvThreshold = 0.005f; 

                for (int i = 0; i < oldVerts.Length; i++)
                {
                    bool isProtected = protectedVertIndices.Contains(i);

                    // KINEMATIC BONE SHIELDING: If the vertex isn't protected by material, check its bones.
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

                    string key;
                    if (isProtected)
                    {
                        // Absolute unique key. This vertex is locked out of the welding grid.
                        key = $"PROTECTED_{i}";
                    }
                    else
                    {
                        long hashX = (long)(Mathf.Round(oldVerts[i].x / threshold));
                        long hashY = (long)(Mathf.Round(oldVerts[i].y / threshold));
                        long hashZ = (long)(Mathf.Round(oldVerts[i].z / threshold));
                        
                        long uvHashX = 0; long uvHashY = 0;
                        if (oldUvs != null && oldUvs.Length > i)
                        {
                            uvHashX = (long)(Mathf.Round(oldUvs[i].x / uvThreshold));
                            uvHashY = (long)(Mathf.Round(oldUvs[i].y / uvThreshold));
                        }

                        key = $"{hashX}:{hashY}:{hashZ}:{uvHashX}:{uvHashY}";
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