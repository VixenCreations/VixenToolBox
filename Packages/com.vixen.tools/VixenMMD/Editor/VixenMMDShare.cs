using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using static VixenTools.MMD.Editor.VixenMMDText;

namespace VixenTools.MMD.Editor
{
    [Serializable]
    internal class MimJson
    {
        public int dataVersion = 1;
        public bool ignoreFaceMeshName;
        public bool ignoreFaceMeshNotAtRoot;
        public bool ignoreWriteDefaultsOff;
        public List<MimMapping> _blendShapeMappings = new List<MimMapping>();
    }

    [Serializable]
    internal class MimMapping
    {
        public string mmdKey = "";
        public string[] avatarKeys = new string[0];
        public float[] avatarKeyScaleOverrides = new float[0];
        public string legacyAvatarKey = "";
    }

    internal static class VixenMMDShare
    {
        internal const string MimMappings = "enitimeago.NonDestructiveMMD.BlendShapeMappings";
        internal const string MimRename = "enitimeago.NonDestructiveMMD.RenameFaceForMmdComponent";
        internal const string MimRemoveLayers = "enitimeago.NonDestructiveMMD.RemoveAnimatorLayersComponent";
        internal const string MimWriteDefaults = "enitimeago.NonDestructiveMMD.WriteDefaultsComponent";

        internal static List<VixenMMDMorph> ReadJson(string json, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                error = L("The file is empty.");
                return null;
            }
            MimJson data;
            try
            {
                data = JsonUtility.FromJson<MimJson>(json);
            }
            catch (Exception e)
            {
                error = L("The file is not a Make It MMD or VixenMMD .json file.") + " " + e.Message;
                return null;
            }
            if (data == null || data._blendShapeMappings == null)
            {
                error = L("The file has no MMD shapes in it.");
                return null;
            }
            if (data.dataVersion > 1)
            {
                error = L("The file comes from a newer version of Make It MMD than VixenMMD can read.");
                return null;
            }
            return FromMappings(data._blendShapeMappings);
        }

        internal static List<VixenMMDMorph> FromMappings(IEnumerable<MimMapping> mappings)
        {
            var list = new List<VixenMMDMorph>();
            foreach (var m in mappings)
            {
                if (m == null || string.IsNullOrEmpty(m.mmdKey)) continue;
                var keys = m.avatarKeys != null && m.avatarKeys.Length > 0 ? m.avatarKeys
                    : string.IsNullOrEmpty(m.legacyAvatarKey) ? new string[0] : new[] { m.legacyAvatarKey };
                var morph = new VixenMMDMorph(m.mmdKey);
                for (int i = 0; i < keys.Length; i++)
                {
                    if (string.IsNullOrEmpty(keys[i])) continue;
                    float s = m.avatarKeyScaleOverrides != null && i < m.avatarKeyScaleOverrides.Length ? m.avatarKeyScaleOverrides[i] : 1f;
                    morph.sources.Add(new VixenMMDSource(keys[i], s));
                }
                list.RemoveAll(x => x.name == morph.name);
                list.Add(morph);
            }
            return list;
        }

        internal static string WriteJson(VixenMMD mmd)
        {
            var data = new MimJson();
            foreach (var m in mmd.morphs)
            {
                if (m == null || !m.enabled || string.IsNullOrEmpty(m.name)) continue;
                var sources = m.sources.Where(s => s != null && !string.IsNullOrEmpty(s.shape)).ToList();
                var mapping = new MimMapping
                {
                    mmdKey = m.name,
                    avatarKeys = sources.Select(s => s.shape).ToArray(),
                    avatarKeyScaleOverrides = sources.Any(s => s.scale != 1f) ? sources.Select(s => s.scale).ToArray() : new float[0]
                };
                data._blendShapeMappings.Add(mapping);
            }
            return JsonUtility.ToJson(data, true);
        }

        internal static void Merge(VixenMMD mmd, List<VixenMMDMorph> incoming, bool replaceAll)
        {
            Undo.RecordObject(mmd, "VixenMMD import");
            if (replaceAll) mmd.morphs.Clear();
            foreach (var m in incoming)
            {
                int i = mmd.morphs.FindIndex(x => x != null && x.name == m.name);
                if (i >= 0) mmd.morphs[i] = m;
                else mmd.morphs.Add(m);
            }
            EditorUtility.SetDirty(mmd);
        }

        internal static void ImportDialog(VixenMMD mmd)
        {
            string path = EditorUtility.OpenFilePanel(L("Import MMD shapes (.json)"), "", "json");
            if (string.IsNullOrEmpty(path)) return;
            string json;
            try
            {
                json = File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("VixenMMD", L("Could not read the file.") + " " + e.Message, L("OK"));
                return;
            }
            var morphs = ReadJson(json, out string error);
            if (morphs == null)
            {
                EditorUtility.DisplayDialog("VixenMMD", error, L("OK"));
                return;
            }
            int choice = mmd.morphs.Count == 0 ? 0 : EditorUtility.DisplayDialogComplex("VixenMMD",
                L("This file has {0} MMD shapes. This component already has {1}.", morphs.Count, mmd.morphs.Count),
                L("Replace all"), L("Cancel"), L("Add and update"));
            if (choice == 1) return;
            Merge(mmd, morphs, choice == 0);
            EditorUtility.DisplayDialog("VixenMMD", L("Imported {0} MMD shapes.", morphs.Count), L("OK"));
        }

        internal static void ExportDialog(VixenMMD mmd)
        {
            var root = mmd.GetComponentInParent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            string name = (root != null ? root.gameObject.name : mmd.gameObject.name) + " MMD";
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            string path = EditorUtility.SaveFilePanel(L("Export MMD shapes (.json)"), "", name, "json");
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllText(path, WriteJson(mmd), new UTF8Encoding(false));
        }

        internal static List<Component> FindMakeItMmd(GameObject root)
        {
            var list = new List<Component>();
            if (root == null) return list;
            foreach (var c in root.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                string n = c.GetType().FullName;
                if (n == MimMappings || n == MimRename || n == MimRemoveLayers || n == MimWriteDefaults) list.Add(c);
            }
            return list;
        }

        internal static VixenMMD ConvertMakeItMmd(GameObject avatarRoot, VixenMMD existing)
        {
            var found = FindMakeItMmd(avatarRoot);
            if (found.Count == 0) return existing;
            var mappings = found.FirstOrDefault(c => c.GetType().FullName == MimMappings);
            var host = existing != null ? existing.gameObject : mappings != null ? mappings.gameObject : avatarRoot;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Convert Make It MMD to VixenMMD");
            int group = Undo.GetCurrentGroup();
            var mmd = existing != null ? existing : Undo.AddComponent<VixenMMD>(host);
            Undo.RecordObject(mmd, "Convert Make It MMD");

            var incoming = new List<VixenMMDMorph>();
            bool rename = false;
            bool forceWd = false;
            var removeLayers = new List<string>();
            foreach (var c in found)
            {
                var so = new SerializedObject(c);
                switch (c.GetType().FullName)
                {
                    case MimMappings:
                        incoming.AddRange(ReadMappings(so));
                        break;
                    case MimRename:
                        rename = true;
                        break;
                    case MimRemoveLayers:
                        var layers = so.FindProperty("layersToRemove");
                        for (int i = 0; layers != null && i < layers.arraySize; i++) removeLayers.Add(layers.GetArrayElementAtIndex(i).stringValue);
                        break;
                    case MimWriteDefaults:
                        var wd = so.FindProperty("forceAvatarWriteDefaults");
                        forceWd = wd != null && wd.boolValue;
                        break;
                }
            }
            if (existing == null)
            {
                mmd.renameFaceToBody = rename;
                mmd.moveFaceToRoot = false;
            }
            else if (rename) mmd.renameFaceToBody = true;
            foreach (var l in removeLayers) if (!string.IsNullOrEmpty(l) && !mmd.removeLayers.Contains(l)) mmd.removeLayers.Add(l);
            mmd.forceWriteDefaults |= forceWd;
            foreach (var m in incoming)
            {
                int i = mmd.morphs.FindIndex(x => x != null && x.name == m.name);
                if (i >= 0) mmd.morphs[i] = m;
                else mmd.morphs.Add(m);
            }
            EditorUtility.SetDirty(mmd);
            foreach (var c in found) Undo.DestroyObjectImmediate(c);
            if (host != avatarRoot && host.name == "Make It MMD")
            {
                Undo.RecordObject(host, "Rename");
                host.name = "VixenMMD";
            }
            Undo.CollapseUndoOperations(group);
            return mmd;
        }

        private static List<VixenMMDMorph> ReadMappings(SerializedObject so)
        {
            var list = new List<MimMapping>();
            var arr = so.FindProperty("_blendShapeMappings");
            for (int i = 0; arr != null && i < arr.arraySize; i++)
            {
                var e = arr.GetArrayElementAtIndex(i);
                var m = new MimMapping { mmdKey = e.FindPropertyRelative("mmdKey")?.stringValue ?? "" };
                var keys = e.FindPropertyRelative("avatarKeys");
                var scales = e.FindPropertyRelative("avatarKeyScaleOverrides");
                m.avatarKeys = Enumerable.Range(0, keys != null ? keys.arraySize : 0).Select(k => keys.GetArrayElementAtIndex(k).stringValue).ToArray();
                m.avatarKeyScaleOverrides = Enumerable.Range(0, scales != null ? scales.arraySize : 0).Select(k => scales.GetArrayElementAtIndex(k).floatValue).ToArray();
                m.legacyAvatarKey = e.FindPropertyRelative("legacyAvatarKey")?.stringValue ?? "";
                list.Add(m);
            }
            return FromMappings(list);
        }
    }
}
