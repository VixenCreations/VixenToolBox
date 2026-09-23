using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace VixenTools.Editor
{
    public class VixenMemoryDiagnostics : EditorWindow
    {
        public const string MenuPath = "VixenTools/Unity Engine/Memory Diagnostics";

        const int LeakTab = 0;
        const int TextureTab = 1;
        const int TextureRows = 50;

        const string LeakHelp =
            "A leak is something a tool makes and never frees. Press Take Snapshot, repeat the task you want to test a few times " +
            "(a bake, a texture pack, a material lock), press Free Unused Memory, then press Compare With Snapshot. The rows that grew are what stayed behind.\n\n" +
            "Only textures, materials and meshes that are not saved in your project are counted. A saved asset that is loaded is not a leak.";

        const string TextureHelp =
            "The textures using the most memory in the editor right now. The editor can keep an extra readable copy, so a texture can be " +
            "bigger here than it is on the GPU in VRChat. \"Not saved\" means it was made at runtime, by a tool or by Unity itself. A big one from a tool is worth a look.";

        struct Row
        {
            public string key;
            public string type;
            public string name;
            public int count;
            public int growth;
            public long bytes;
            public string detail;
            public Object sample;
        }

        static Dictionary<string, int> s_snapshot;
        static double s_snapshotTime;

        readonly List<Row> m_rows = new List<Row>();
        int m_tab;
        Button m_leakTabButton;
        Button m_textureTabButton;
        Button m_snapshotButton;
        Button m_compareButton;
        Button m_scanButton;
        HelpBox m_help;
        Label m_summary;
        ListView m_list;

        [MenuItem(MenuPath, false, 31)]
        public static void Open()
        {
            VixenMemoryDiagnostics window = GetWindow<VixenMemoryDiagnostics>("Memory Diagnostics");
            window.minSize = new Vector2(560f, 420f);
            window.Show();
        }

        public static void FreeUnusedMemory()
        {
            System.GC.Collect();
            EditorUtility.UnloadUnusedAssetsImmediate();
            System.GC.Collect();
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
                "[Vixen Tools] Freed unused memory. Engine memory in use: {0}.",
                VixenSceneViewEnhancer.FormatBytes(Profiler.GetTotalAllocatedMemoryLong()));
        }

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 6;
            root.style.paddingBottom = 6;

            Label title = new Label("Memory Diagnostics");
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 4;
            root.Add(title);

            VisualElement tabs = HorizontalGroup();
            m_leakTabButton = new Button(() => SetTab(LeakTab)) { text = "Leak Check" };
            m_textureTabButton = new Button(() => SetTab(TextureTab)) { text = "Largest Textures" };
            tabs.Add(m_leakTabButton);
            tabs.Add(m_textureTabButton);
            root.Add(tabs);

            m_help = new HelpBox("", HelpBoxMessageType.Info);
            m_help.style.marginTop = 4;
            m_help.style.marginBottom = 4;
            root.Add(m_help);

            VisualElement actions = HorizontalGroup();
            m_snapshotButton = new Button(TakeSnapshot) { text = "Take Snapshot" };
            m_compareButton = new Button(Compare) { text = "Compare With Snapshot" };
            m_scanButton = new Button(Scan) { text = "Scan Now" };
            Button free = new Button(() =>
            {
                FreeUnusedMemory();
                m_summary.text = "Freed unused memory. Press Compare With Snapshot or Scan Now to see what is left.";
            }) { text = "Free Unused Memory" };
            actions.Add(m_snapshotButton);
            actions.Add(m_compareButton);
            actions.Add(m_scanButton);
            actions.Add(free);
            root.Add(actions);

            m_summary = new Label();
            m_summary.style.whiteSpace = WhiteSpace.Normal;
            m_summary.style.marginTop = 4;
            m_summary.style.marginBottom = 4;
            root.Add(m_summary);

            m_list = new ListView(m_rows, 22, MakeRow, BindRow);
            m_list.style.flexGrow = 1;
            m_list.selectionType = SelectionType.None;
            root.Add(m_list);

            SetTab(LeakTab);
        }

        static VisualElement HorizontalGroup()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            return row;
        }

        void SetTab(int tab)
        {
            m_tab = tab;
            bool leaks = tab == LeakTab;
            m_leakTabButton.style.unityFontStyleAndWeight = leaks ? FontStyle.Bold : FontStyle.Normal;
            m_textureTabButton.style.unityFontStyleAndWeight = leaks ? FontStyle.Normal : FontStyle.Bold;
            m_help.text = leaks ? LeakHelp : TextureHelp;
            m_snapshotButton.style.display = leaks ? DisplayStyle.Flex : DisplayStyle.None;
            m_compareButton.style.display = leaks ? DisplayStyle.Flex : DisplayStyle.None;
            m_compareButton.SetEnabled(s_snapshot != null);
            m_scanButton.text = leaks ? "Scan Now" : "Scan Textures";
            m_rows.Clear();
            m_summary.text = leaks
                ? (s_snapshot == null ? "No snapshot yet." : "A snapshot is ready from " + Ago(s_snapshotTime) + ".")
                : "Press Scan Textures.";
            m_list.Rebuild();
        }

        VisualElement MakeRow()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            Label name = new Label { name = "name" };
            name.style.flexGrow = 1;
            name.style.flexShrink = 1;
            name.style.overflow = Overflow.Hidden;
            name.style.textOverflow = TextOverflow.Ellipsis;

            Label amount = new Label { name = "amount" };
            amount.style.width = 110;
            amount.style.unityTextAlign = TextAnchor.MiddleRight;
            amount.style.unityFontStyleAndWeight = FontStyle.Bold;

            Label detail = new Label { name = "detail" };
            detail.style.width = 170;
            detail.style.unityTextAlign = TextAnchor.MiddleRight;

            Button select = new Button { name = "select", text = "Select" };
            select.style.width = 60;

            row.Add(name);
            row.Add(amount);
            row.Add(detail);
            row.Add(select);
            return row;
        }

        void BindRow(VisualElement element, int index)
        {
            if (index < 0 || index >= m_rows.Count) return;
            Row row = m_rows[index];
            element.Q<Label>("name").text = row.type + "   " + row.name;
            element.Q<Label>("name").tooltip = row.name;
            element.Q<Label>("amount").text = VixenSceneViewEnhancer.FormatBytes(row.bytes);
            element.Q<Label>("detail").text = row.detail;

            Button select = element.Q<Button>("select");
            Object target = row.sample;
            select.clickable = new Clickable(() => SelectObject(target));
            select.SetEnabled(target != null);
        }

        static void SelectObject(Object target)
        {
            if (target == null) return;
            Selection.activeObject = target;
            if (EditorUtility.IsPersistent(target)) EditorGUIUtility.PingObject(target);
        }

        static IEnumerable<Object> LeakCandidates()
        {
            foreach (Texture t in Resources.FindObjectsOfTypeAll<Texture>()) yield return t;
            foreach (Material m in Resources.FindObjectsOfTypeAll<Material>()) yield return m;
            foreach (Mesh m in Resources.FindObjectsOfTypeAll<Mesh>()) yield return m;
        }

        static Dictionary<string, Row> CollectUnsaved()
        {
            Dictionary<string, Row> groups = new Dictionary<string, Row>();
            foreach (Object obj in LeakCandidates())
            {
                if (obj == null || EditorUtility.IsPersistent(obj)) continue;

                string type = obj.GetType().Name;
                string name = string.IsNullOrEmpty(obj.name) ? "(no name)" : obj.name;
                string key = type + "|" + name;

                Row row;
                if (!groups.TryGetValue(key, out row))
                {
                    row.key = key;
                    row.type = type;
                    row.name = name;
                    row.sample = obj;
                }
                row.count++;
                row.bytes += Profiler.GetRuntimeMemorySizeLong(obj);
                groups[key] = row;
            }
            return groups;
        }

        void TakeSnapshot()
        {
            Dictionary<string, Row> groups = CollectUnsaved();
            s_snapshot = groups.ToDictionary(g => g.Key, g => g.Value.count);
            s_snapshotTime = EditorApplication.timeSinceStartup;
            m_compareButton.SetEnabled(true);

            m_rows.Clear();
            m_list.Rebuild();
            m_summary.text = "Snapshot taken: " + groups.Values.Sum(g => g.count) + " unsaved objects in " + groups.Count +
                             " groups. Repeat the task you want to test, press Free Unused Memory, then press Compare With Snapshot.";
        }

        void Compare()
        {
            if (s_snapshot == null) return;

            Dictionary<string, Row> groups = CollectUnsaved();
            m_rows.Clear();
            int grown = 0;
            long grownBytes = 0;
            foreach (Row current in groups.Values)
            {
                int before;
                s_snapshot.TryGetValue(current.key, out before);
                int growth = current.count - before;
                if (growth <= 0) continue;

                Row row = current;
                row.growth = growth;
                row.detail = "+" + growth + " (" + current.count + " now)";
                m_rows.Add(row);
                grown += growth;
                grownBytes += current.count > 0 ? current.bytes * growth / current.count : 0;
            }

            m_rows.Sort((a, b) => b.growth != a.growth ? b.growth.CompareTo(a.growth) : b.bytes.CompareTo(a.bytes));
            m_list.Rebuild();
            m_summary.text = m_rows.Count == 0
                ? "Nothing grew since the snapshot from " + Ago(s_snapshotTime) + "."
                : m_rows.Count + " group(s) grew since the snapshot from " + Ago(s_snapshotTime) + ": " + grown + " more objects, about " +
                  VixenSceneViewEnhancer.FormatBytes(grownBytes) + ". A group that grows every time you repeat the same task is a leak.";
        }

        void Scan()
        {
            if (m_tab == TextureTab)
            {
                ScanTextures();
                return;
            }

            Dictionary<string, Row> groups = CollectUnsaved();
            m_rows.Clear();
            foreach (Row current in groups.Values)
            {
                if (current.count < 2 && current.name.IndexOf("(Clone)", System.StringComparison.Ordinal) < 0) continue;
                Row row = current;
                row.detail = current.count + " in memory";
                m_rows.Add(row);
            }

            m_rows.Sort((a, b) => b.bytes.CompareTo(a.bytes));
            m_list.Rebuild();
            m_summary.text = groups.Values.Sum(g => g.count) + " unsaved textures, materials and meshes in memory, using " +
                             VixenSceneViewEnhancer.FormatBytes(groups.Values.Sum(g => g.bytes)) + ". Shown: groups with more than one copy or a (Clone) name. Most of these are Unity's own, because it keeps a copy for each editor window, " +
                             "UI panel and realtime reflection probe. To find a leak, press Take Snapshot, repeat the task, then press Compare With Snapshot.";
        }

        void ScanTextures()
        {
            m_rows.Clear();
            long total = 0;
            List<Row> all = new List<Row>();
            foreach (Texture texture in Resources.FindObjectsOfTypeAll<Texture>())
            {
                if (texture == null) continue;
                long bytes = Profiler.GetRuntimeMemorySizeLong(texture);
                total += bytes;

                Row row = new Row();
                row.type = texture.GetType().Name;
                row.name = string.IsNullOrEmpty(texture.name) ? "(no name)" : texture.name;
                row.bytes = bytes;
                row.sample = texture;
                string size = texture.width > 0 && texture.height > 0 ? texture.width + "x" + texture.height : "internal";
                row.detail = EditorUtility.IsPersistent(texture) ? size : size + ", not saved";
                all.Add(row);
            }

            all.Sort((a, b) => b.bytes.CompareTo(a.bytes));
            m_rows.AddRange(all.Take(TextureRows));
            m_list.Rebuild();
            m_summary.text = all.Count + " textures in memory, using " + VixenSceneViewEnhancer.FormatBytes(total) + ". Showing the " +
                             m_rows.Count + " largest.";
        }

        static string Ago(double time)
        {
            int seconds = Mathf.Max(0, (int)(EditorApplication.timeSinceStartup - time));
            return seconds < 60 ? seconds + " s ago" : (seconds / 60) + " min ago";
        }
    }
}
