#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace VixenTools.Editor
{
    public static class BlendShapeSetForge
    {
        public enum Output { VRCFuryOnAccessory, AvatarFxLayers }

        public class Entry
        {
            public string Shape;
            public bool Include = true;
            public bool DefaultOn;
            public string Group;
            public string Label;
        }

        public class Section
        {
            public string Name;
            public List<string> Shapes = new List<string>();
        }

        public class Settings
        {
            public Output Output = Output.VRCFuryOnAccessory;
            public string MenuPrefix = "Clothing/Accessories";
            public string Verb = "Hide";
            public float Value = 100f;
            public bool Saved = true;
            public bool Synced = true;
        }

        private static readonly Regex Divider = new Regex(@"^\s*[-=_*#~]{3,}");
        private static readonly Regex Numbered = new Regex(@"^(?<base>[A-Za-z]+?)(?<side>L|R)?(?<num>\d+)$");

        private static readonly Dictionary<string, string[]> KnownRows = new Dictionary<string, string[]>
        {
            { "Ear", new[] { "Top Bar", "Mid Bar", "Bottom Bar", "Top Ring", "Mid Ring", "Bottom Ring" } },
        };

        private static readonly Dictionary<string, string> KnownSingles = new Dictionary<string, string>
        {
            { "SBL", "Snake Bite Left" },
            { "SBR", "Snake Bite Right" },
            { "NoseRing", "Nose Ring" },
            { "LipRing", "Lip Ring" },
            { "Tongue", "Tongue" },
        };

        public static List<Section> FindSections(Mesh mesh)
        {
            var sections = new List<Section>();
            if (mesh == null) return sections;
            Section current = null;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string name = mesh.GetBlendShapeName(i);
                if (Divider.IsMatch(name))
                {
                    current = new Section { Name = name.Trim('-', '=', '_', '*', '#', '~', ' ') };
                    sections.Add(current);
                    continue;
                }
                if (current == null)
                {
                    current = new Section { Name = "(before any divider)" };
                    sections.Add(current);
                }
                current.Shapes.Add(name);
            }
            return sections.Where(s => s.Shapes.Count > 0).ToList();
        }

        public static int GuessSection(List<Section> sections)
        {
            for (int i = 0; i < sections.Count; i++)
                if (sections[i].Name.IndexOf("Hide", StringComparison.OrdinalIgnoreCase) >= 0
                    || sections[i].Name.IndexOf("Piercing", StringComparison.OrdinalIgnoreCase) >= 0
                    || sections[i].Name.IndexOf("Toggle", StringComparison.OrdinalIgnoreCase) >= 0)
                    return i;
            return 0;
        }

        public static List<Entry> BuildEntries(IEnumerable<string> shapes)
        {
            var list = new List<Entry>();
            foreach (string shape in shapes)
            {
                var e = new Entry { Shape = shape };
                var m = Numbered.Match(shape);
                if (m.Success)
                {
                    string baseName = Nicify(m.Groups["base"].Value);
                    string side = m.Groups["side"].Value;
                    int num = int.Parse(m.Groups["num"].Value);
                    e.Group = side == "L" ? "Left " + baseName : side == "R" ? "Right " + baseName : baseName;
                    if (KnownRows.TryGetValue(m.Groups["base"].Value, out var row) && num >= 1 && num <= row.Length)
                        e.Label = row[num - 1];
                    else
                        e.Label = "Piercing " + num;
                }
                else if (KnownSingles.TryGetValue(shape, out var single))
                {
                    e.Group = "Face";
                    e.Label = single;
                }
                else
                {
                    e.Group = "Other";
                    e.Label = Nicify(shape);
                }
                list.Add(e);
            }
            return list;
        }

        public static string MenuPathFor(Settings s, Entry e)
        {
            string verb = string.IsNullOrWhiteSpace(s.Verb) ? "" : s.Verb.Trim() + " ";
            string prefix = string.IsNullOrWhiteSpace(s.MenuPrefix) ? "" : s.MenuPrefix.Trim().TrimEnd('/') + "/";
            return prefix + e.Group.Trim() + "/" + verb + e.Label.Trim();
        }

        public static bool VRCFuryAvailable => FindType("com.vrcfury.api.FuryComponents") != null;

        public static List<string> Generate(VRCAvatarDescriptor descriptor, SkinnedMeshRenderer renderer, GameObject accessoryRoot,
                                            List<Entry> entries, Settings s)
        {
            var log = new List<string>();
            var chosen = entries.Where(e => e.Include && !string.IsNullOrWhiteSpace(e.Label) && !string.IsNullOrWhiteSpace(e.Group)).ToList();
            if (renderer == null) { log.Add("ERROR: pick a Skinned Mesh Renderer."); return log; }
            if (chosen.Count == 0) { log.Add("ERROR: nothing is ticked."); return log; }

            if (s.Output == Output.VRCFuryOnAccessory)
            {
                if (!VRCFuryAvailable) { log.Add("ERROR: VRCFury is not installed in this project. Pick Avatar FX Layers or install VRCFury."); return log; }
                if (accessoryRoot == null) accessoryRoot = renderer.transform.parent != null ? renderer.transform.parent.gameObject : renderer.gameObject;
                GenerateVRCFury(renderer, accessoryRoot, chosen, s, log);
            }
            else
            {
                if (descriptor == null) { log.Add("ERROR: Avatar FX Layers needs the Avatar Root set at the top of the window."); return log; }
                GenerateFx(descriptor, renderer, chosen, s, log);
            }
            return log;
        }

        private static void GenerateVRCFury(SkinnedMeshRenderer renderer, GameObject accessoryRoot, List<Entry> chosen, Settings s, List<string> log)
        {
            var api = FindType("com.vrcfury.api.FuryComponents");
            var create = api.GetMethod("CreateToggle", BindingFlags.Public | BindingFlags.Static);
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Generate VRCFury toggles");
            int made = 0, kept = 0;

            foreach (var byGroup in chosen.GroupBy(e => e.Group.Trim()))
            {
                string holderName = "Toggles " + byGroup.Key;
                var holderT = accessoryRoot.transform.Find(holderName);
                GameObject holder;
                if (holderT != null) holder = holderT.gameObject;
                else
                {
                    holder = new GameObject(holderName);
                    Undo.RegisterCreatedObjectUndo(holder, "Generate VRCFury toggles");
                    holder.transform.SetParent(accessoryRoot.transform, false);
                }

                var existingPaths = ExistingToggleNames(holder);
                foreach (var e in byGroup)
                {
                    string path = MenuPathFor(s, e);
                    if (existingPaths.Contains(path)) { kept++; continue; }

                    var before = new HashSet<Component>(holder.GetComponents<Component>());
                    object toggle = create.Invoke(null, new object[] { holder });
                    var t = toggle.GetType();
                    t.GetMethod("SetMenuPath").Invoke(toggle, new object[] { path });
                    if (s.Saved) t.GetMethod("SetSaved").Invoke(toggle, null);
                    if (e.DefaultOn) t.GetMethod("SetDefaultOn").Invoke(toggle, null);
                    object actions = t.GetMethod("GetActions").Invoke(toggle, null);
                    actions.GetType().GetMethod("AddBlendshape").Invoke(actions, new object[] { e.Shape, s.Value, renderer });
                    foreach (var c in holder.GetComponents<Component>())
                        if (!before.Contains(c)) Undo.RegisterCreatedObjectUndo(c, "Generate VRCFury toggles");
                    existingPaths.Add(path);
                    made++;
                }
                EditorUtility.SetDirty(holder);
            }

            Undo.CollapseUndoOperations(group);
            if (accessoryRoot.scene.IsValid()) EditorSceneManager.MarkSceneDirty(accessoryRoot.scene);
            log.Add($"VRCFury: {made} toggle(s) added and {kept} already there left as they were, under '{accessoryRoot.name}'.");
            log.Add("Each VRCFury toggle uses one bool parameter unless VRCFury's Parameter Compressor is on the avatar.");
        }

        private static HashSet<string> ExistingToggleNames(GameObject holder)
        {
            var names = new HashSet<string>();
            foreach (var c in holder.GetComponents<Component>())
            {
                if (c == null || c.GetType().Name != "VRCFury") continue;
                var so = new SerializedObject(c);
                var p = so.FindProperty("content.name");
                if (p != null && p.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(p.stringValue))
                    names.Add(p.stringValue);
            }
            return names;
        }

        private static void GenerateFx(VRCAvatarDescriptor descriptor, SkinnedMeshRenderer renderer, List<Entry> chosen, Settings s, List<string> log)
        {
            int ok = 0, failed = 0;
            foreach (var e in chosen)
            {
                string path = MenuPathFor(s, e);
                int slash = path.LastIndexOf('/');
                var req = new RigForge.RigRequest
                {
                    Type = RigForge.RigType.BlendshapeToggle,
                    RigName = slash >= 0 ? path.Substring(slash + 1) : path,
                    ParameterName = path,
                    Saved = s.Saved,
                    Synced = s.Synced,
                    StartOn = e.DefaultOn,
                    BlendRenderer = renderer,
                    BlendShape = e.Shape,
                    BlendOnValue = s.Value,
                    MenuPath = slash >= 0 ? path.Substring(0, slash) : ""
                };
                var result = RigForge.Build(descriptor, req);
                if (result.Success) ok++;
                else
                {
                    failed++;
                    log.AddRange(result.Log.Where(l => l.StartsWith("ERROR")));
                }
            }
            log.Add($"FX layers: {ok} toggle(s) forged, {failed} failed.");
            var expr = descriptor.expressionParameters;
            if (expr != null)
            {
                int cost = expr.CalcTotalCost();
                log.Add($"Synced parameter memory now {cost} of {VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.MAX_PARAMETER_COST} bits.");
            }
        }

        private static string Nicify(string s)
        {
            return ObjectNames.NicifyVariableName(s);
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, false);
                if (t != null) return t;
            }
            return null;
        }
    }
}
#endif
