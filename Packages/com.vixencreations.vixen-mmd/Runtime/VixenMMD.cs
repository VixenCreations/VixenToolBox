using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace VixenTools.MMD
{
    [Serializable]
    public class VixenMMDSource
    {
        public string shape = "";
        public float scale = 1f;

        public VixenMMDSource() { }

        public VixenMMDSource(string shape, float scale)
        {
            this.shape = shape;
            this.scale = scale;
        }
    }

    [Serializable]
    public class VixenMMDMorph
    {
        public string name = "";
        public bool enabled = true;
        public List<VixenMMDSource> sources = new List<VixenMMDSource>();

        public VixenMMDMorph() { }

        public VixenMMDMorph(string name)
        {
            this.name = name;
        }
    }

    [Serializable]
    public class VixenMMDShapeSet
    {
        public string key = "";
        public List<VixenMMDSource> sources = new List<VixenMMDSource>();

        public VixenMMDShapeSet() { }

        public VixenMMDShapeSet(string key)
        {
            this.key = key;
        }
    }

    [AddComponentMenu("VixenTools/VixenMMD")]
    [DisallowMultipleComponent]
    [HelpURL("https://vixencreations.github.io/VixenToolBox/tools.html")]
    public class VixenMMD : MonoBehaviour, IEditorOnly
    {
        public const int CurrentDataVersion = 1;

        public int dataVersion = CurrentDataVersion;
        public SkinnedMeshRenderer faceMesh;
        public List<VixenMMDMorph> morphs = new List<VixenMMDMorph>();

        public bool renameFaceToBody = true;
        public bool moveFaceToRoot = true;

        public bool worldGuard = false;
        public List<string> guardLayers = new List<string>();

        public List<string> removeLayers = new List<string>();
        public bool forceWriteDefaults = false;

        public List<VixenMMDShapeSet> expressionShapes = new List<VixenMMDShapeSet>();
        public List<VixenMMDShapeSet> visemeShapes = new List<VixenMMDShapeSet>();

        public VixenMMDMorph FindMorph(string morphName)
        {
            if (morphs == null) return null;
            foreach (var m in morphs)
            {
                if (m != null && m.name == morphName) return m;
            }
            return null;
        }
    }
}
