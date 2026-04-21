#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ImageMagick;

// Safely integrate VRChat SDK namespace
#if VRC_SDK_VRCSDK3
using VRC.SDK3.Dynamics.PhysBone.Components;
#endif

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: Non-destructive Quest material and hierarchy conversion engine.
    /// Features ImageMagick Lanczos downsampling for high-fidelity mobile textures,
    /// and an Interactive Topology Matrix allowing creators to manually override 
    /// heuristic PhysBone culling prior to execution.
    /// </summary>
    public class QuestConversionEngine : EditorWindow
    {
        private GameObject sourceAvatar;
        
        // Scan Data
        private int rendererCount = 0;
        private int uniqueMaterialCount = 0;
        private int uniqueTextureCount = 0;
        private bool hasScanned = false;
        private Vector2 scrollPos;
        private Vector2 topologyScrollPos;

        private enum TargetQuestShader
        {
            VRCMobileToonStandard, // New default for high-fidelity Quest materials
            VRCMobileToonLit,
            VRCMobileStandard,
            VRCMobileMatcap,
            UnityMobileStandard
        }
        private TargetQuestShader selectedTargetShader = TargetQuestShader.VRCMobileToonStandard;

        // Texture Memory Caps for Mobile
        private int[] textureSizeOptions = { 256, 512, 1024, 2048 };
        private string[] textureSizeLabels = { 
            "256 (Aggressive - 10MB Excellent Limit)", 
            "512 (Balanced - 18MB Good Limit)", 
            "1024 (Standard - 40MB Poor Limit)", 
            "2048 (Heavy - Very Poor/Unoptimized)" 
        };
        private int selectedTextureSizeIndex = 2;

        // VRChat Mobile Performance Ranks
        private enum MobilePerformanceRank
        {
            Excellent, // 0 PB, 0 Col
            Good,      // 4 PB, 4 Col
            Medium,    // 6 PB, 8 Col
            Poor       // 8 PB, 16 Col (Hard Limits)
        }
        private MobilePerformanceRank targetPerformanceRank = MobilePerformanceRank.Poor;

        private const string BASE_OUTPUT_DIR = "Assets/VixenTools/QuestConversion";
        
        // Runtime execution caches
        private Dictionary<Texture, Texture> textureCache = new Dictionary<Texture, Texture>();
        private string activeTexturesDir;

#if VRC_SDK_VRCSDK3
        // Interactive Topology Matrix State
        private class TopologyNode
        {
            public Component component;
            public string relativePath;
            public int depth;
            public bool keep;
        }
        private List<TopologyNode> scannedPhysBones = new List<TopologyNode>();
        private List<TopologyNode> scannedColliders = new List<TopologyNode>();
        private bool showTopologyMatrix = true;
#endif

        [MenuItem("VixenTools/Avatars/Quest Conversion Engine")]
        public static void ShowWindow()
        {
            var window = GetWindow<QuestConversionEngine>("Quest Engine");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnGUI()
        {
            // --- HEADER NAVIGATION BAR ---
            Rect headerRect = EditorGUILayout.GetControlRect(false, 50);
            EditorGUI.DrawRect(headerRect, new Color(0.08f, 0.04f, 0.12f)); 
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20
            };
            EditorGUI.LabelField(headerRect, "<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> QUEST ENGINE", headerStyle);

            GUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUIStyle sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true, fontSize = 14 };

            // --- PHASE 1: TARGETING & ANALYSIS ---
            EditorGUILayout.LabelField("<color=#00e5ff>Phase 1: Deep Matrix Scanning</color>", sectionHeaderStyle);
            EditorGUILayout.HelpBox("Select the root of your PC Avatar. This process is 100% non-destructive. The original will be disabled, and a Quest-isolated clone will be generated.", MessageType.Info);
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            sourceAvatar = (GameObject)EditorGUILayout.ObjectField("Source Avatar (Root)", sourceAvatar, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                hasScanned = false; 
            }

            selectedTargetShader = (TargetQuestShader)EditorGUILayout.EnumPopup("Target Mobile Shader", selectedTargetShader);
            selectedTextureSizeIndex = EditorGUILayout.Popup("Max Texture Resolution", selectedTextureSizeIndex, textureSizeLabels);
            
            EditorGUI.BeginChangeCheck();
            targetPerformanceRank = (MobilePerformanceRank)EditorGUILayout.EnumPopup("Target Performance Rank", targetPerformanceRank);
            if (EditorGUI.EndChangeCheck() && hasScanned)
            {
                // Re-calculate heuristic defaults if the user changes the rank after scanning
#if VRC_SDK_VRCSDK3
                ApplyHeuristicCullingRules();
#endif
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Scan Matrix Architecture", GUILayout.Height(30)))
            {
                AnalyzeHierarchy();
            }

            if (hasScanned)
            {
                GUILayout.Space(10);
                DrawResultsBox();

#if VRC_SDK_VRCSDK3
                DrawInteractiveTopologyMatrix(sectionHeaderStyle);
#endif
            }

            GUILayout.Space(20);
            DrawSeparator(new Color(0.5f, 0.5f, 0.5f, 0.3f));
            GUILayout.Space(20);

            // --- PHASE 2: EXECUTION ---
            EditorGUILayout.LabelField("<color=#ff00aa>Phase 2: Deep Conversion Pipeline</color>", sectionHeaderStyle);
            
            GUI.enabled = hasScanned && sourceAvatar != null && uniqueMaterialCount > 0;
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.5f);
            
            if (GUILayout.Button("Execute Full Quest Conversion", GUILayout.Height(40)))
            {
                ExecuteConversion();
            }
            
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            GUILayout.Space(20);
            EditorGUILayout.EndScrollView();
        }

        private void DrawResultsBox()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle resultsStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true, fontSize = 12 };

            string results = $"<b><color=#00e5ff>■</color> MATRIX SCAN RESULTS:</b>\n\n" +
                             $"  • Renderers Detected: <b><color=#ff00aa>{rendererCount}</color></b>\n" +
                             $"  • Unique Materials: <b><color=#ff00aa>{uniqueMaterialCount}</color></b>\n" +
                             $"  • Unique Textures: <b><color=#ff00aa>{uniqueTextureCount}</color></b>";

#if VRC_SDK_VRCSDK3
            results += $"\n  • PhysBones Found: <b><color=#ff00aa>{scannedPhysBones.Count}</color></b>\n" +
                       $"  • PhysBone Colliders: <b><color=#ff00aa>{scannedColliders.Count}</color></b>";
#endif
            
            EditorGUILayout.LabelField(results, resultsStyle);
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

#if VRC_SDK_VRCSDK3
        private void DrawInteractiveTopologyMatrix(GUIStyle headerStyle)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("<color=#00e5ff>Phase 1.5: Interactive Topology Matrix</color>", headerStyle);
            EditorGUILayout.HelpBox("Heuristic limits have been applied based on your Target Performance Rank. You may manually override which physics components survive the Quest conversion.", MessageType.Info);
            
            showTopologyMatrix = EditorGUILayout.Foldout(showTopologyMatrix, "Topology Culling Matrix (PhysBones)");
            if (showTopologyMatrix)
            {
                topologyScrollPos = EditorGUILayout.BeginScrollView(topologyScrollPos, GUILayout.Height(400));
                
                // Keep count logic
                int pbKept = scannedPhysBones.Count(n => n.keep);
                int limit = GetMaxPhysBones();
                Color countColor = pbKept > limit ? Color.red : new Color(0.2f, 0.8f, 0.2f);
                
                EditorGUILayout.LabelField($"Selected: <color=#{ColorUtility.ToHtmlStringRGB(countColor)}><b>{pbKept} / {limit}</b></color> Allowed", new GUIStyle(EditorStyles.label) { richText = true, fontSize = 14 });
                GUILayout.Space(10);

                // Upgraded text styling for maximum readability
                GUIStyle rowStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 13,
                    richText = true,
                    alignment = TextAnchor.MiddleLeft
                };

                foreach (var node in scannedPhysBones)
                {
                    // Wrap each entry in a subtle box for padding and distinct row separation
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    
                    // Center the toggle vertically with the text
                    node.keep = EditorGUILayout.Toggle(node.keep, GUILayout.Width(20));
                    
                    // UI/UX Upgrade: Parse the path to dim the parent folders and highlight the actual bone in high-contrast neon green
                    string displayPath = node.relativePath;
                    int lastSlash = displayPath.LastIndexOf('/');
                    
                    if (lastSlash >= 0 && lastSlash < displayPath.Length - 1)
                    {
                        string baseDir = displayPath.Substring(0, lastSlash + 1);
                        string boneName = displayPath.Substring(lastSlash + 1);
                        // High-contrast green (#00ff66) for the target bone
                        displayPath = $"<color=#00ff66>{baseDir}</color><b><color=#00ff66>{boneName}</color></b>";
                    }
                    else if (string.IsNullOrEmpty(displayPath))
                    {
                        displayPath = "<b><color=#00e5ff>[Avatar Root]</color></b>";
                    }
                    else
                    {
                        // Fallback for top-level bones
                        displayPath = $"<b><color=#00ff66>{displayPath}</color></b>";
                    }

                    // Apply the new text style and force a minimum height for breathing room
                    EditorGUILayout.LabelField(displayPath, rowStyle, GUILayout.Height(20));
                    
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
        }
#endif

        private void AnalyzeHierarchy()
        {
            if (sourceAvatar == null)
            {
                Debug.LogWarning("[VixenTools] No Source Avatar selected for scanning.");
                return;
            }

            Renderer[] renderers = sourceAvatar.GetComponentsInChildren<Renderer>(true);
            rendererCount = renderers.Length;
            
            HashSet<Material> uniqueMats = new HashSet<Material>();
            HashSet<Texture> uniqueTexs = new HashSet<Texture>();

            foreach (var rend in renderers)
            {
                if (rend == null) continue;
                foreach (var mat in rend.sharedMaterials)
                {
                    if (mat != null) 
                    {
                        uniqueMats.Add(mat);
                        if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null) uniqueTexs.Add(mat.GetTexture("_MainTex"));
                        if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") != null) uniqueTexs.Add(mat.GetTexture("_BaseMap"));
                        if (mat.HasProperty("_EmissionMap") && mat.GetTexture("_EmissionMap") != null) uniqueTexs.Add(mat.GetTexture("_EmissionMap"));
                    }
                }
            }

            uniqueMaterialCount = uniqueMats.Count;
            uniqueTextureCount = uniqueTexs.Count;

#if VRC_SDK_VRCSDK3
            scannedPhysBones.Clear();
            scannedColliders.Clear();

            VRCPhysBone[] pbs = sourceAvatar.GetComponentsInChildren<VRCPhysBone>(true);
            foreach (var pb in pbs)
            {
                scannedPhysBones.Add(new TopologyNode {
                    component = pb,
                    relativePath = AnimationUtility.CalculateTransformPath(pb.transform, sourceAvatar.transform),
                    depth = GetHierarchyDepth(pb.transform),
                    keep = true
                });
            }

            VRCPhysBoneCollider[] cols = sourceAvatar.GetComponentsInChildren<VRCPhysBoneCollider>(true);
            foreach (var col in cols)
            {
                scannedColliders.Add(new TopologyNode {
                    component = col,
                    relativePath = AnimationUtility.CalculateTransformPath(col.transform, sourceAvatar.transform),
                    depth = GetHierarchyDepth(col.transform),
                    keep = true
                });
            }

            ApplyHeuristicCullingRules();
#endif

            hasScanned = true;
            Debug.Log($"[VixenTools] Matrix Scan Complete.");
        }

#if VRC_SDK_VRCSDK3
        private void ApplyHeuristicCullingRules()
        {
            int maxPB = GetMaxPhysBones();
            int maxCol = GetMaxColliders();

            // Sort by depth (root closest first) and toggle heuristically
            scannedPhysBones = scannedPhysBones.OrderBy(n => n.depth).ToList();
            for (int i = 0; i < scannedPhysBones.Count; i++)
            {
                scannedPhysBones[i].keep = (i < maxPB);
            }

            scannedColliders = scannedColliders.OrderBy(n => n.depth).ToList();
            for (int i = 0; i < scannedColliders.Count; i++)
            {
                scannedColliders[i].keep = (i < maxCol);
            }
        }

        private int GetMaxPhysBones()
        {
            switch (targetPerformanceRank)
            {
                case MobilePerformanceRank.Excellent: return 0;
                case MobilePerformanceRank.Good: return 4;
                case MobilePerformanceRank.Medium: return 6;
                default: return 8;
            }
        }

        private int GetMaxColliders()
        {
            switch (targetPerformanceRank)
            {
                case MobilePerformanceRank.Excellent: return 0;
                case MobilePerformanceRank.Good: return 4;
                case MobilePerformanceRank.Medium: return 8;
                default: return 16;
            }
        }

        private int GetHierarchyDepth(Transform t)
        {
            int depth = 0;
            while (t.parent != null)
            {
                depth++;
                t = t.parent;
            }
            return depth;
        }
#endif

        private void ExecuteConversion()
        {
            if (sourceAvatar == null) return;

            try
            {
                EditorUtility.DisplayProgressBar("VixenTools Quest Engine", "Initializing Directory Structures...", 0.1f);

                string avatarName = sourceAvatar.name;
                string questName = $"Quest_{avatarName}";
                
                EnsureDirectoryExists(BASE_OUTPUT_DIR);
                string avatarDir = $"{BASE_OUTPUT_DIR}/{questName}";
                EnsureDirectoryExists(avatarDir);
                
                string materialsDir = $"{avatarDir}/Materials";
                EnsureDirectoryExists(materialsDir);

                activeTexturesDir = $"{avatarDir}/Textures";
                EnsureDirectoryExists(activeTexturesDir);

                textureCache.Clear();

                EditorUtility.DisplayProgressBar("VixenTools Quest Engine", "Generating Prefab Sandbox...", 0.2f);
                string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{avatarDir}/{avatarName}_Base.prefab");
                GameObject tempPrefab = PrefabUtility.SaveAsPrefabAsset(sourceAvatar, prefabPath);

                Undo.RecordObject(sourceAvatar, "Disable PC Avatar");
                sourceAvatar.SetActive(false);

                GameObject questClone = (GameObject)PrefabUtility.InstantiatePrefab(tempPrefab);
                questClone.name = questName;
                questClone.transform.position = sourceAvatar.transform.position;
                questClone.transform.rotation = sourceAvatar.transform.rotation;
                questClone.transform.parent = sourceAvatar.transform.parent;

                EditorUtility.DisplayProgressBar("VixenTools Quest Engine", "Cloning and Converting Materials...", 0.4f);
                Renderer[] cloneRenderers = questClone.GetComponentsInChildren<Renderer>(true);
                Dictionary<Material, Material> materialCache = new Dictionary<Material, Material>();
                Shader targetShader = GetShaderForEnum(selectedTargetShader);

                for (int r = 0; r < cloneRenderers.Length; r++)
                {
                    Renderer rend = cloneRenderers[r];
                    if (rend == null) continue;

                    EditorUtility.DisplayProgressBar("VixenTools Quest Engine", $"Processing Materials ({r}/{cloneRenderers.Length})...", 0.4f + (0.3f * ((float)r / cloneRenderers.Length)));

                    Material[] currentMats = rend.sharedMaterials;
                    Material[] newMats = new Material[currentMats.Length];

                    for (int i = 0; i < currentMats.Length; i++)
                    {
                        Material originalMat = currentMats[i];
                        if (originalMat == null) continue;

                        if (materialCache.TryGetValue(originalMat, out Material cachedNewMat))
                        {
                            newMats[i] = cachedNewMat;
                            continue;
                        }

                        Material questMat = new Material(targetShader);
                        questMat.name = $"{originalMat.name}_Quest";

                        TransferProperties(originalMat, questMat);

                        string matPath = AssetDatabase.GenerateUniqueAssetPath($"{materialsDir}/{questMat.name}.mat");
                        AssetDatabase.CreateAsset(questMat, matPath);
                        
                        materialCache.Add(originalMat, questMat);
                        newMats[i] = questMat;
                    }

                    rend.sharedMaterials = newMats;
                    EditorUtility.SetDirty(rend);
                }

#if VRC_SDK_VRCSDK3
                EditorUtility.DisplayProgressBar("VixenTools Quest Engine", "Applying Matrix Topology Overrides...", 0.8f);
                ApplyTopologyOverrides(questClone);
#endif

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                PrefabUtility.UnpackPrefabInstance(questClone, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                Selection.activeGameObject = questClone;
                
                Debug.Log($"[VixenTools] Quest Conversion Complete! {materialCache.Count} materials and {textureCache.Count} high-fidelity textures processed.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                textureCache.Clear(); 
            }
        }

#if VRC_SDK_VRCSDK3
        private void ApplyTopologyOverrides(GameObject clone)
        {
            // Execute PhysBone Culling based on user's manual UI selection
            foreach (var node in scannedPhysBones)
            {
                if (!node.keep)
                {
                    Transform targetTransform = string.IsNullOrEmpty(node.relativePath) ? clone.transform : clone.transform.Find(node.relativePath);
                    if (targetTransform != null)
                    {
                        VRCPhysBone pb = targetTransform.GetComponent<VRCPhysBone>();
                        if (pb != null) DestroyImmediate(pb, true);
                    }
                }
            }

            // Execute Collider Culling
            foreach (var node in scannedColliders)
            {
                if (!node.keep)
                {
                    Transform targetTransform = string.IsNullOrEmpty(node.relativePath) ? clone.transform : clone.transform.Find(node.relativePath);
                    if (targetTransform != null)
                    {
                        VRCPhysBoneCollider col = targetTransform.GetComponent<VRCPhysBoneCollider>();
                        if (col != null) DestroyImmediate(col, true);
                    }
                }
            }
        }
#endif

        private Shader GetShaderForEnum(TargetQuestShader target)
        {
            string shaderName = "VRChat/Mobile/Toon Standard"; // Default fallback
            switch (target)
            {
                case TargetQuestShader.VRCMobileToonStandard: shaderName = "VRChat/Mobile/Toon Standard"; break;
                case TargetQuestShader.VRCMobileToonLit: shaderName = "VRChat/Mobile/Toon Lit"; break;
                case TargetQuestShader.VRCMobileStandard: shaderName = "VRChat/Mobile/Standard Lite"; break;
                case TargetQuestShader.VRCMobileMatcap: shaderName = "VRChat/Mobile/MatCap Lit"; break;
                case TargetQuestShader.UnityMobileStandard: shaderName = "Mobile/Standard"; break;
            }

            Shader found = Shader.Find(shaderName);
            if (found == null) return Shader.Find("Standard");
            return found;
        }

        private void TransferProperties(Material source, Material target)
        {
            // --- Primary Albedo ---
            if (source.HasProperty("_MainTex") && target.HasProperty("_MainTex"))
                target.SetTexture("_MainTex", ProcessAndCloneTexture(source.GetTexture("_MainTex")));
            else if (source.HasProperty("_BaseMap") && target.HasProperty("_MainTex")) 
                target.SetTexture("_MainTex", ProcessAndCloneTexture(source.GetTexture("_BaseMap")));

            if (source.HasProperty("_Color") && target.HasProperty("_Color"))
                target.SetColor("_Color", source.GetColor("_Color"));
            else if (source.HasProperty("_BaseColor") && target.HasProperty("_Color"))
                target.SetColor("_Color", source.GetColor("_BaseColor"));

            // --- Emission Integrity ---
            if (source.HasProperty("_EmissionMap") && target.HasProperty("_EmissionMap"))
                target.SetTexture("_EmissionMap", ProcessAndCloneTexture(source.GetTexture("_EmissionMap")));
            
            if (source.HasProperty("_EmissionColor") && target.HasProperty("_EmissionColor"))
                target.SetColor("_EmissionColor", source.GetColor("_EmissionColor"));

            // --- High-Fidelity Normal Maps ---
            if (source.HasProperty("_BumpMap") && target.HasProperty("_BumpMap"))
            {
                target.SetTexture("_BumpMap", ProcessAndCloneTexture(source.GetTexture("_BumpMap")));
                
                if (source.HasProperty("_BumpScale") && target.HasProperty("_BumpScale"))
                    target.SetFloat("_BumpScale", source.GetFloat("_BumpScale"));
            }

            // --- Glossy Girl Preservation (Metallic/Gloss) ---
            // Maps standard PBR/Poiyomi smoothness into VRC Mobile Toon Standard
            if (target.HasProperty("_MetallicMap"))
            {
                if (source.HasProperty("_MetallicGlossMap"))
                    target.SetTexture("_MetallicMap", ProcessAndCloneTexture(source.GetTexture("_MetallicGlossMap")));
                
                if (source.HasProperty("_Metallic") && target.HasProperty("_MetallicStrength"))
                    target.SetFloat("_MetallicStrength", source.GetFloat("_Metallic"));
                
                if (source.HasProperty("_Glossiness") && target.HasProperty("_GlossStrength"))
                    target.SetFloat("_GlossStrength", source.GetFloat("_Glossiness"));
            }
        }

        private Texture ProcessAndCloneTexture(Texture sourceTex)
        {
            if (sourceTex == null) return null;
            if (textureCache.TryGetValue(sourceTex, out Texture cachedTex)) return cachedTex;

            string sourcePath = AssetDatabase.GetAssetPath(sourceTex);
            if (string.IsNullOrEmpty(sourcePath)) return sourceTex; 

            string texName = sourceTex.name;
            string extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(extension)) extension = ".png";

            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{activeTexturesDir}/{texName}_Quest{extension}");
            int targetSize = textureSizeOptions[selectedTextureSizeIndex];

            try
            {
                // Utilize ImageMagick for high-fidelity Lanczos downsampling before Unity imports it
                using (MagickImage img = new MagickImage(sourcePath))
                {
                    if (img.Width > targetSize || img.Height > targetSize)
                    {
                        MagickGeometry size = new MagickGeometry((uint)targetSize, (uint)targetSize);
                        size.IgnoreAspectRatio = false;
                        img.FilterType = FilterType.Lanczos; 
                        img.Resize(size);
                    }
                    img.Write(newPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VixenTools] ImageMagick processing failed for {texName}, falling back to Unity Copy. Error: {ex.Message}");
                AssetDatabase.CopyAsset(sourcePath, newPath);
            }

            AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(newPath) as TextureImporter;
            
            if (importer != null)
            {
                importer.maxTextureSize = targetSize;
                
                // Force ASTC compression to respect Android VRAM limitations
                TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings
                {
                    name = "Android",
                    overridden = true,
                    maxTextureSize = targetSize,
                    format = TextureImporterFormat.ASTC_6x6, 
                    textureCompression = TextureImporterCompression.Compressed
                };
                
                importer.SetPlatformTextureSettings(androidSettings);
                importer.SaveAndReimport();
            }

            Texture newTex = AssetDatabase.LoadAssetAtPath<Texture>(newPath);
            textureCache[sourceTex] = newTex;
            return newTex;
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        private void DrawSeparator(Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, color);
        }
    }
}
#endif