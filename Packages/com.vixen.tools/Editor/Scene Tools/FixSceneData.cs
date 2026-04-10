using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Utility: Forces serialization of lighting data to resolve 
    /// missing or unlinked lightmap references in the active scene.
    /// </summary>
    public class FixSceneData
    {
        // Placed at the root of the VixenTools menu for immediate access
        [MenuItem("VixenTools/Fix Scene Data")]
        public static void FixLightingDataAssignment()
        {
            Scene currentScene = SceneManager.GetActiveScene();

            if (string.IsNullOrEmpty(currentScene.path))
            {
                Debug.LogError("[VixenTools] Scene must be saved to a file before fixing lighting data.");
                return;
            }

            // Reference the existing lighting data asset for the active scene
            var lightingData = Lightmapping.lightingDataAsset;

            if (lightingData == null)
            {
                Debug.LogWarning($"[VixenTools] No Lighting Data Asset found for {currentScene.name}. You may need to Generate Lighting once first.");
                return;
            }

            // Re-assigning the asset forces Unity to refresh the serialized reference in the scene file
            Lightmapping.lightingDataAsset = lightingData;

            // Mark the scene as 'dirty' so the Editor knows it has unsaved changes
            EditorSceneManager.MarkSceneDirty(currentScene);

            // Save the scene and flush all asset changes to disk (Serialization)
            bool saveSuccess = EditorSceneManager.SaveScene(currentScene);
            AssetDatabase.SaveAssets();

            if (saveSuccess)
            {
                Debug.Log($"[VixenTools] Successfully re-assigned and serialized lighting data for: {currentScene.name}");
            }
            else
            {
                Debug.LogError("[VixenTools] Failed to save the scene during the fix process.");
            }
        }
    }
}