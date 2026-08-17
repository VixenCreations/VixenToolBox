#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace VixenTools.Editor
{
    public class FixSceneData
    {
        [MenuItem("VixenTools/Unity Engine/Fix Scene Data")]
        public static void FixLightingDataAssignment()
        {
            Scene currentScene = SceneManager.GetActiveScene();

            if (string.IsNullOrEmpty(currentScene.path))
            {
                Debug.LogError("[VixForge] Scene must be saved to a file before fixing lighting data.");
                return;
            }

            var lightingData = Lightmapping.lightingDataAsset;

            if (lightingData == null)
            {
                Debug.LogWarning($"[VixForge] No Lighting Data Asset found for {currentScene.name}. You may need to Generate Lighting once first.");
                return;
            }

            Lightmapping.lightingDataAsset = lightingData;

            EditorSceneManager.MarkSceneDirty(currentScene);

            bool saveSuccess = EditorSceneManager.SaveScene(currentScene);
            AssetDatabase.SaveAssets();

            if (saveSuccess)
            {
                Debug.Log($"[VixForge] Successfully re-assigned and serialized lighting data for: {currentScene.name}");
            }
            else
            {
                Debug.LogError("[VixForge] Failed to save the scene during the fix process.");
            }
        }
    }
}
#endif