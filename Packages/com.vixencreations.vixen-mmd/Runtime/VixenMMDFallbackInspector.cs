#if UNITY_EDITOR && !VIXEN_NDMF
using UnityEditor;

namespace VixenTools.MMD
{
    [CustomEditor(typeof(VixenMMD))]
    internal class VixenMMDFallbackInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("VixenMMD needs NDMF 1.14.7 or newer to build your MMD blend shapes. Add or update Modular Avatar or NDMF in the VRChat Creator Companion, then come back. Your settings on this component are kept.", MessageType.Warning);
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(true))
            {
                DrawDefaultInspector();
            }
        }
    }
}
#endif
