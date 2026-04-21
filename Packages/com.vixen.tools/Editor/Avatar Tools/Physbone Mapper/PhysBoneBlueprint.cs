#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Presets;
using System.Collections.Generic;

namespace VixenTools.Editor
{
    /// <summary>
    /// VixenTools Core: A blueprint asset to store the exact skeletal paths and presets of an avatar's physics.
    /// </summary>
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