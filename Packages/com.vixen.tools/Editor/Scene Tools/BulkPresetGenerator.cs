#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: A unified pipeline tool that handles both bulk extraction of presets 
    /// from existing assets, and the programmatic authoring of standardized Importer presets 
    /// from scratch using a Phantom Asset architecture.
    /// </summary>
    public class BulkPresetGenerator : EditorWindow
    {
        private enum ToolMode { Extraction, Authoring }
        private ToolMode currentMode = ToolMode.Extraction;

        // --- Shared Configuration ---
        private string outputDirectory = "Assets/VixenTools/GeneratedPresets";

        // --- Extraction Variables ---
        private bool ignoreTransforms = true;
        private bool includeChildren = false;
        private bool registerExtractionToManager = true;
        private string extractionFilter = "";

        // --- Authoring Variables (Texture Standards) ---
        private string authoringPresetName = "Global_4K_Texture_Standard";
        private int maxTextureSize = 4096;
        private TextureImporterType textureType = TextureImporterType.Default;
        private bool enableMipMaps = true;
        private bool isReadable = false;
        private bool registerAuthoringToManager = true;
        private string authoringFilter = ""; // Empty = applies to ALL textures

        [MenuItem("VixenTools/Pipeline Preset Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BulkPresetGenerator>("VixenTools Pipeline");
            window.minSize = new Vector2(480, 450);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("VixenTools | Preset Architecture Core", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Tab Navigation
            string[] tabs = { "Bulk Extraction", "Standard Authoring" };
            currentMode = (ToolMode)GUILayout.Toolbar((int)currentMode, tabs, GUILayout.Height(30));
            GUILayout.Space(15);

            outputDirectory = EditorGUILayout.TextField("Global Output Directory", outputDirectory);
            GUILayout.Space(10);

            switch (currentMode)
            {
                case ToolMode.Extraction:
                    DrawExtractionUI();
                    break;
                case ToolMode.Authoring:
                    DrawAuthoringUI();
                    break;
            }
        }

        #region UI Rendering

        private void DrawExtractionUI()
        {
            EditorGUILayout.HelpBox(
                "Extract configuration from selected GameObjects or Assets to generate .preset files.", 
                MessageType.Info);
            
            GUILayout.Space(10);
            ignoreTransforms = EditorGUILayout.Toggle("Ignore Transforms", ignoreTransforms);
            includeChildren = EditorGUILayout.Toggle("Include Children (Hierarchy)", includeChildren);

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Global Routing (Preset Manager)", EditorStyles.boldLabel);
            
            registerExtractionToManager = EditorGUILayout.Toggle("Register to Manager", registerExtractionToManager);
            
            EditorGUI.BeginDisabledGroup(!registerExtractionToManager);
            extractionFilter = EditorGUILayout.TextField(
                new GUIContent("Target Filter", "e.g., 'glob:\"*_diffuse\"' or folder path"), 
                extractionFilter);
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.8f); 
            if (GUILayout.Button("Execute Extraction Pipeline", GUILayout.Height(40)))
            {
                ExecuteExtractionPipeline();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);
        }

        private void DrawAuthoringUI()
        {
            EditorGUILayout.HelpBox(
                "Author standardized Importer Presets from scratch using a Phantom Asset pattern. " +
                "Currently supports Texture Importers.", 
                MessageType.Info);
            
            GUILayout.Space(10);
            authoringPresetName = EditorGUILayout.TextField("Preset Name", authoringPresetName);

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Texture Standards", EditorStyles.boldLabel);
            
            int[] sizes = { 512, 1024, 2048, 4096, 8192 };
            string[] sizeLabels = { "512", "1K (1024)", "2K (2048)", "4K (4096)", "8K (8192)" };
            int sizeIndex = System.Array.IndexOf(sizes, maxTextureSize);
            sizeIndex = EditorGUILayout.Popup("Max Resolution", sizeIndex >= 0 ? sizeIndex : 3, sizeLabels);
            maxTextureSize = sizes[sizeIndex];

            textureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", textureType);
            enableMipMaps = EditorGUILayout.Toggle("Generate MipMaps", enableMipMaps);
            isReadable = EditorGUILayout.Toggle("Read/Write Enabled", isReadable);

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Preset Manager Routing", EditorStyles.boldLabel);
            registerAuthoringToManager = EditorGUILayout.Toggle("Set as Global Default", registerAuthoringToManager);
            
            EditorGUI.BeginDisabledGroup(!registerAuthoringToManager);
            authoringFilter = EditorGUILayout.TextField(
                new GUIContent("Target Filter", "Leave blank to apply to ALL textures, or use e.g., 'glob:\"*_Albedo\"'"), 
                authoringFilter);
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.5f); // Vixen pink/magenta accent
            if (GUILayout.Button("Generate & Inject Standard", GUILayout.Height(40)))
            {
                ExecuteAuthoringPipeline();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);
        }

        #endregion

        #region Logic: Authoring (Phantom Asset)

        private void ExecuteAuthoringPipeline()
        {
            EnsureDirectoryExists(outputDirectory);

            // Step 1: The Phantom Asset Pattern
            string dummyPath = "Assets/VixenTools_PhantomTexture.png";
            
            Texture2D dummyTex = new Texture2D(1, 1);
            dummyTex.SetPixel(0, 0, Color.black);
            dummyTex.Apply();
            
            File.WriteAllBytes(dummyPath, dummyTex.EncodeToPNG());
            AssetDatabase.Refresh();

            // Step 2: Extract and Configure the Importer
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(dummyPath);
            if (importer == null)
            {
                Debug.LogError("[VixenTools] Critical failure: Could not hook into phantom asset importer.");
                return;
            }

            importer.textureType = textureType;
            importer.maxTextureSize = maxTextureSize;
            importer.mipmapEnabled = enableMipMaps;
            importer.isReadable = isReadable;
            
            importer.SaveAndReimport();

            // Step 3: Snapshot into a Preset
            Preset newPreset = new Preset(importer);
            
            string cleanName = string.Join("_", authoringPresetName.Split(Path.GetInvalidFileNameChars()));
            string finalPresetPath = AssetDatabase.GenerateUniqueAssetPath($"{outputDirectory}/{cleanName}.preset");
            
            AssetDatabase.CreateAsset(newPreset, finalPresetPath);

            // Step 4: Clean up
            AssetDatabase.DeleteAsset(dummyPath);

            // Step 5: Global Injection
            if (registerAuthoringToManager)
            {
                InjectIntoPresetManager(newPreset, authoringFilter);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VixenTools] Importer Standard applied. Preset generated at {finalPresetPath}.");
        }

        #endregion

        #region Logic: Extraction

        private void ExecuteExtractionPipeline()
        {
            EnsureDirectoryExists(outputDirectory);

            Object[] selectedObjects = Selection.objects;
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("[VixenTools] Execution aborted: No source objects selected.");
                return;
            }

            int generatedCount = 0;
            List<string> generatedPaths = new List<string>();

            foreach (Object obj in selectedObjects)
            {
                if (obj is GameObject go)
                {
                    generatedCount += ProcessGameObject(go, generatedPaths);
                }
                else
                {
                    generatedCount += ProcessProjectAsset(obj, generatedPaths);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VixenTools] Extraction operations complete. Generated & routed {generatedCount} preset(s).");
        }

        private int ProcessGameObject(GameObject go, List<string> paths)
        {
            int count = 0;
            Component[] components = includeChildren 
                ? go.GetComponentsInChildren<Component>(true) 
                : go.GetComponents<Component>();

            foreach (Component comp in components)
            {
                if (comp == null) continue;
                if (ignoreTransforms && comp is Transform) continue;

                Preset preset = new Preset(comp);
                if (preset != null)
                {
                    string baseName = $"{go.name}_{comp.GetType().Name}";
                    count += SaveAndRegisterPreset(preset, baseName, paths);
                }
            }
            return count;
        }

        private int ProcessProjectAsset(Object obj, List<string> paths)
        {
            int count = 0;
            string assetPath = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(assetPath)) return count;

            if (obj is ScriptableObject || obj is Material)
            {
                Preset dataPreset = new Preset(obj);
                count += SaveAndRegisterPreset(dataPreset, $"{obj.name}_{obj.GetType().Name}", paths);
            }
            else
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    Preset importerPreset = new Preset(importer);
                    count += SaveAndRegisterPreset(importerPreset, $"{obj.name}_{importer.GetType().Name}", paths);
                }
                else
                {
                    Preset fallbackPreset = new Preset(obj);
                    count += SaveAndRegisterPreset(fallbackPreset, $"{obj.name}_{obj.GetType().Name}", paths);
                }
            }

            return count;
        }

        private int SaveAndRegisterPreset(Preset preset, string baseName, List<string> paths)
        {
            if (preset == null) return 0;
            
            string cleanName = string.Join("_", baseName.Split(Path.GetInvalidFileNameChars()));
            string path = AssetDatabase.GenerateUniqueAssetPath($"{outputDirectory}/{cleanName}.preset");
            
            AssetDatabase.CreateAsset(preset, path);
            paths.Add(path);

            if (registerExtractionToManager)
            {
                InjectIntoPresetManager(preset, extractionFilter);
            }

            return 1;
        }

        #endregion

        #region Shared Core Infrastructure

        private void InjectIntoPresetManager(Preset newPreset, string filter)
        {
            PresetType targetType = newPreset.GetPresetType();
            DefaultPreset[] currentDefaults = Preset.GetDefaultPresetsForType(targetType);
            
            // Prevent duplicate bindings to maintain clean topology
            if (currentDefaults.Any(dp => dp.preset == newPreset && dp.filter == filter))
                return;

            List<DefaultPreset> updatedDefaults = new List<DefaultPreset>(currentDefaults);
            
            // Insert at the top of the hierarchy to prioritize custom configurations
            updatedDefaults.Insert(0, new DefaultPreset
            {
                preset = newPreset,
                filter = filter
            });

            Preset.SetDefaultPresetsForType(targetType, updatedDefaults.ToArray());
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        #endregion
    }
}
#endif