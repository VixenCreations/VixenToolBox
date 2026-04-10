#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using System.Collections.Generic;
using System.IO;

// Safely include VRChat namespaces only if the SDK is present
#if VRC_SDK_VRCSDK3
using VRC.SDK3.Dynamics.PhysBone.Components;
#endif

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Editor: Extracts and Injects complete PhysBone architectures across avatars.
    /// Safely degrades in non-VRChat Unity environments.
    /// </summary>
    public class PhysBoneTopologyMapper : EditorWindow
    {
        private Vector2 scrollPos;

#if VRC_SDK_VRCSDK3
        private GameObject sourceAvatar;
        private GameObject targetAvatar;
        private PhysBoneBlueprint loadedBlueprint;
        private string blueprintName = "Novabeast_1.2_MasterTopology";
#endif

        [MenuItem("VixenTools/PhysBone Topology Mapper")]
        public static void ShowWindow()
        {
            var window = GetWindow<PhysBoneTopologyMapper>("Topology Mapper");
            window.minSize = new Vector2(400, 450);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("VixenTools | Topology Blueprint Engine", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

#if VRC_SDK_VRCSDK3
            // --- PHASE 1: EXTRACTION ---
            EditorGUILayout.LabelField("Phase 1: Architecture Extraction", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select the root of your tuned avatar. This generates a Master Blueprint and all associated Presets.", MessageType.Info);
            
            sourceAvatar = (GameObject)EditorGUILayout.ObjectField("Source Avatar (Root)", sourceAvatar, typeof(GameObject), true);
            blueprintName = EditorGUILayout.TextField("Blueprint Name", blueprintName);

            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.8f);
            if (GUILayout.Button("Extract Master Copy", GUILayout.Height(35)))
            {
                ExtractTopology();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(20);
            DrawSeparator();
            GUILayout.Space(20);

            // --- PHASE 2: INJECTION ---
            EditorGUILayout.LabelField("Phase 2: Architecture Injection", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select a blank avatar and a Blueprint. This will reconstruct your master physics matrix.", MessageType.Info);

            targetAvatar = (GameObject)EditorGUILayout.ObjectField("Target Avatar (Root)", targetAvatar, typeof(GameObject), true);
            loadedBlueprint = (PhysBoneBlueprint)EditorGUILayout.ObjectField("Master Blueprint", loadedBlueprint, typeof(PhysBoneBlueprint), false);

            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.5f);
            if (GUILayout.Button("Inject Blueprint", GUILayout.Height(35)))
            {
                InjectTopology();
            }
            GUI.backgroundColor = Color.white;
#else
            // --- GRACEFUL DEGRADATION FOR STANDALONE UNITY ---
            EditorGUILayout.HelpBox("VRChat SDK3 is not detected in this project. The PhysBone Topology Mapper requires the VRChat Avatar SDK to function.", MessageType.Warning);
            
            GUI.enabled = false;
            GUILayout.Button("Extract Master Copy (VRChat SDK Required)", GUILayout.Height(35));
            GUILayout.Space(10);
            GUILayout.Button("Inject Blueprint (VRChat SDK Required)", GUILayout.Height(35));
            GUI.enabled = true;
#endif

            EditorGUILayout.EndScrollView();
        }

#if VRC_SDK_VRCSDK3
        private void ExtractTopology()
        {
            if (sourceAvatar == null)
            {
                Debug.LogError("[VixenTools] Source Avatar missing.");
                return;
            }

            string baseDir = $"Assets/VixenTools/Blueprints/{blueprintName}";
            EnsureDirectoryExists(baseDir);
            EnsureDirectoryExists($"{baseDir}/Presets");

            // Create the Blueprint Asset
            PhysBoneBlueprint blueprint = ScriptableObject.CreateInstance<PhysBoneBlueprint>();

            VRCPhysBone[] physBones = sourceAvatar.GetComponentsInChildren<VRCPhysBone>(true);
            int count = 0;

            foreach (var pb in physBones)
            {
                string relativePath = AnimationUtility.CalculateTransformPath(pb.transform, sourceAvatar.transform);
                
                Preset pbPreset = new Preset(pb);
                string cleanPathName = relativePath.Replace("/", "_");
                if (string.IsNullOrEmpty(cleanPathName)) cleanPathName = "Root"; 
                
                string presetPath = AssetDatabase.GenerateUniqueAssetPath($"{baseDir}/Presets/{cleanPathName}.preset");
                AssetDatabase.CreateAsset(pbPreset, presetPath);

                blueprint.nodes.Add(new PhysBoneBlueprint.Node { 
                    bonePath = relativePath, 
                    preset = pbPreset 
                });
                
                count++;
            }

            string blueprintPath = AssetDatabase.GenerateUniqueAssetPath($"{baseDir}/{blueprintName}.asset");
            AssetDatabase.CreateAsset(blueprint, blueprintPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VixenTools] Extraction Complete! Mapped {count} PhysBones to {blueprintPath}.");
        }

        private void InjectTopology()
        {
            if (targetAvatar == null || loadedBlueprint == null)
            {
                Debug.LogError("[VixenTools] Target Avatar or Blueprint missing.");
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var node in loadedBlueprint.nodes)
            {
                Transform targetBone = targetAvatar.transform.Find(node.bonePath);
                
                if (string.IsNullOrEmpty(node.bonePath)) 
                    targetBone = targetAvatar.transform;

                if (targetBone != null)
                {
                    VRCPhysBone pb = targetBone.GetComponent<VRCPhysBone>();
                    if (pb == null)
                    {
                        pb = targetBone.gameObject.AddComponent<VRCPhysBone>();
                    }

                    node.preset.ApplyTo(pb);
                    successCount++;
                }
                else
                {
                    Debug.LogWarning($"[VixenTools] Bone not found on target: {node.bonePath}. Skipping.");
                    failCount++;
                }
            }

            EditorUtility.SetDirty(targetAvatar);
            Debug.Log($"[VixenTools] Injection Complete! Applied {successCount} presets. Failed/Skipped: {failCount}.");
        }
#endif

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        private void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }
}
#endif