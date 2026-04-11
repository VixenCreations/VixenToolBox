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
        private string authoringFilter = ""; 

        [MenuItem("VixenTools/Pipeline Preset Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BulkPresetGenerator>("Preset Manager");
            window.minSize = new Vector2(450, 500);
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
            EditorGUI.LabelField(headerRect, "<color=#00e5ff>VIXEN</color><color=#ff00aa>TOOLS</color> PRESET MANAGER", headerStyle);

            GUILayout.Space(10);

            // --- MODE SWITCHER ---
            currentMode = (ToolMode)GUILayout.Toolbar((int)currentMode, new string[] { "Extraction Pipeline", "Authoring Engine" }, GUILayout.Height(30));
            GUILayout.Space(15);

            GUIStyle sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true, fontSize = 14 };

            if (currentMode == ToolMode.Extraction)
            {
                DrawExtractionUI(sectionHeaderStyle);
            }
            else
            {
                DrawAuthoringUI(sectionHeaderStyle);
            }
        }

        private void DrawExtractionUI(GUIStyle headerStyle)
        {
            EditorGUILayout.LabelField("<color=#00e5ff>Bulk Preset Extraction</color>", headerStyle);
            EditorGUILayout.HelpBox("Select objects in your hierarchy or project. This tool will rip their component configurations into reusable Unity Presets.", MessageType.Info);
            GUILayout.Space(10);

            outputDirectory = EditorGUILayout.TextField("Output Directory", outputDirectory);
            extractionFilter = EditorGUILayout.TextField(new GUIContent("Preset Filter (Optional)", "Filter string applied in the Preset Manager. Leave blank to apply to all."), extractionFilter);
            
            GUILayout.Space(5);
            ignoreTransforms = EditorGUILayout.Toggle("Ignore Transforms", ignoreTransforms);
            includeChildren = EditorGUILayout.Toggle("Include Children", includeChildren);
            registerExtractionToManager = EditorGUILayout.Toggle("Auto-Register to Manager", registerExtractionToManager);

            GUILayout.Space(20);
            DrawSeparator(new Color(0f, 0.9f, 1f, 0.3f)); // Cyan separator
            GUILayout.Space(20);

            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.8f);
            if (GUILayout.Button("Extract Presets from Selection", GUILayout.Height(40)))
            {
                ExecuteExtraction();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawAuthoringUI(GUIStyle headerStyle)
        {
            EditorGUILayout.LabelField("<color=#ff00aa>Programmatic Asset Authoring</color>", headerStyle);
            EditorGUILayout.HelpBox("Defines strict import standards (e.g., 4K texture caps, mip-map rules) and generates a master preset without needing a source asset.", MessageType.Info);
            GUILayout.Space(10);

            outputDirectory = EditorGUILayout.TextField("Output Directory", outputDirectory);
            authoringPresetName = EditorGUILayout.TextField("Preset Name", authoringPresetName);
            authoringFilter = EditorGUILayout.TextField(new GUIContent("Preset Filter (Glob)", "Example: glob:\"**/*_BaseColor.png\""), authoringFilter);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Texture Import Rules", EditorStyles.boldLabel);
            textureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", textureType);
            maxTextureSize = EditorGUILayout.IntPopup("Max Texture Size", maxTextureSize, new[] { "1024", "2048", "4096", "8192" }, new[] { 1024, 2048, 4096, 8192 });
            enableMipMaps = EditorGUILayout.Toggle("Generate Mip Maps", enableMipMaps);
            isReadable = EditorGUILayout.Toggle("Read/Write Enabled", isReadable);
            registerAuthoringToManager = EditorGUILayout.Toggle("Auto-Register to Manager", registerAuthoringToManager);

            GUILayout.Space(20);
            DrawSeparator(new Color(1f, 0f, 0.66f, 0.3f)); // Pink separator
            GUILayout.Space(20);

            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.5f);
            if (GUILayout.Button("Author Texture Standard Preset", GUILayout.Height(40)))
            {
                ExecuteTextureAuthoring();
            }
            GUI.backgroundColor = Color.white;
        }

        #region Execution Logic
        private void ExecuteExtraction()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("[VixenTools] No objects selected for extraction.");
                return;
            }

            EnsureDirectoryExists(outputDirectory);
            int count = 0;

            foreach (var obj in selectedObjects)
            {
                Component[] components = includeChildren ? obj.GetComponentsInChildren<Component>(true) : obj.GetComponents<Component>();
                
                foreach (var comp in components)
                {
                    if (comp == null || (ignoreTransforms && comp is Transform)) continue;
                    
                    Preset preset = new Preset(comp);
                    string typeName = comp.GetType().Name;
                    string path = AssetDatabase.GenerateUniqueAssetPath($"{outputDirectory}/{obj.name}_{typeName}.preset");
                    
                    AssetDatabase.CreateAsset(preset, path);
                    count++;

                    if (registerExtractionToManager)
                    {
                        InjectIntoPresetManager(preset, extractionFilter);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[VixenTools] Extracted {count} presets to {outputDirectory}.");
        }

        private void ExecuteTextureAuthoring()
        {
            EnsureDirectoryExists(outputDirectory);

            // 1. Create a "Phantom Asset" (Temporary file to base the importer on)
            string phantomPath = "Assets/VixenTools_PhantomTexture.png";
            File.WriteAllBytes(phantomPath, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // Minimal valid PNG header
            AssetDatabase.ImportAsset(phantomPath, ImportAssetOptions.ForceUpdate);

            // 2. Grab the importer and inject our standardized rules
            TextureImporter importer = AssetImporter.GetAtPath(phantomPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = textureType;
                importer.maxTextureSize = maxTextureSize;
                importer.mipmapEnabled = enableMipMaps;
                importer.isReadable = isReadable;
                importer.SaveAndReimport();

                // 3. Rip the configuration into a permanent Preset
                Preset newPreset = new Preset(importer);
                string presetPath = AssetDatabase.GenerateUniqueAssetPath($"{outputDirectory}/{authoringPresetName}.preset");
                AssetDatabase.CreateAsset(newPreset, presetPath);

                if (registerAuthoringToManager)
                {
                    InjectIntoPresetManager(newPreset, authoringFilter);
                }

                Debug.Log($"[VixenTools] Authored Master Texture Preset: {presetPath}");
            }

            // 4. Clean up the Phantom Asset
            AssetDatabase.DeleteAsset(phantomPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void InjectIntoPresetManager(Preset newPreset, string filter)
        {
            PresetType targetType = newPreset.GetPresetType();
            DefaultPreset[] currentDefaults = Preset.GetDefaultPresetsForType(targetType);
            
            if (currentDefaults.Any(dp => dp.preset == newPreset && dp.filter == filter))
                return;

            List<DefaultPreset> updatedDefaults = new List<DefaultPreset>(currentDefaults);
            
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

        private void DrawSeparator(Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, color);
        }
        #endregion
    }
}
#endif