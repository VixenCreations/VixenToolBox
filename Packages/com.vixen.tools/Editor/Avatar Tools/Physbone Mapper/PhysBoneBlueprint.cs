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
            public int componentIndex;
            public bool hasReferences;
            public bool hasRootTransform;
            public string rootTransformPath = "";
            public List<string> ignoreTransformPaths = new List<string>();
            public List<string> colliderPaths = new List<string>();
            public List<int> colliderIndices = new List<int>();
        }
        public List<Node> nodes = new List<Node>();
    }
}
#endif