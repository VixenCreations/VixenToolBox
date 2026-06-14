#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using UnityEngine;
using UnityEditor.Presets;
using System.Collections.Generic;

namespace VixenTools.Editor
{
    public class PhysBoneBlueprint : ScriptableObject
    {
        [System.Serializable]
        public class Node
        {
            public string bonePath;
            public Preset preset;
        }
        public List<Node> nodes = new List<Node>();
    }
}
#endif