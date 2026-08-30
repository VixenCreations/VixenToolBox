#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace VixenTools.Editor
{
    public class VixenHub : EditorWindow
    {
        private const string PackageRoot = "Packages/com.vixencreations.vixens-toolbox/";
        private const string FontPath = PackageRoot + "Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string UssPath = PackageRoot + "Editor/UiStyles/VixenHubStyles.uss";
        private const string ChangelogFile = "CHANGELOG.md";

        private const string TabNews = "news";
        private const string TabOverview = "overview";
        private const string TabCoreModules = "core-modules";
        private const string TabSupportedModules = "supported-modules";
        private const string TabMetrics = "metrics-engine";
        private const string TabNetwork = "network";
        private const string TabSupport = "support";
        private const string TabChangelogs = "changelogs";

        private const string CatStart = "Start Here";
        private const string CatTools = "Tools";
        private const string CatDocs = "Docs";
        private const string CatAbout = "About";

        private static readonly string[] CategoryOrder = { CatStart, CatTools, CatDocs, CatAbout };

        private const int DropdownThreshold = 9;

        private class HubTab
        {
            public string Id;
            public string Title;
            public string Description;
            public string Category;
            public int Order;
            public System.Action<VisualElement> Render;
            public Button Button;
        }

        private class ChangelogEntry
        {
            public string VersionTitle;
            public string Content;
        }

        private readonly List<HubTab> _tabs = new List<HubTab>();
        private List<HubTab> _dropdownOrder;
        private DropdownField _tabDropdown;
        private string _pendingTabId;

        private List<ChangelogEntry> _changelogEntries = new List<ChangelogEntry>();
        private int _selectedChangelogIndex = 0;

        private Font _cyberFont;
        private string _packageVersion = "Unknown";
        private string _sdkVersion = "Unknown";

        private Label _tabDescription;
        private ScrollView _contentScroll;
        private VisualElement _contentContainer;

        [MenuItem("VixenTools/Hub Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenHub>("VixForge Hub");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        public static void ShowChangelogWindow()
        {
            var window = GetWindow<VixenHub>("VixForge Hub");
            window.minSize = new Vector2(450, 600);
            window.Show();

            window.OpenTab(TabChangelogs);
        }

        public void OpenChangelogTab()
        {
            OpenTab(TabChangelogs);
        }

        public void OpenTab(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (_contentContainer == null)
            {
                _pendingTabId = id;
                return;
            }

            SelectTab(id);
            Repaint();
        }

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            LoadVersionData();
            BuildTabs();
        }

        private void LoadVersionData()
        {
            string path = Path.GetFullPath(PackageRoot + "package.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

                Match vMatch = Regex.Match(json, @"""version""\s*:\s*""([^""]+)""");
                if (vMatch.Success) _packageVersion = vMatch.Groups[1].Value;
            }

            _sdkVersion = ResolveInstalledSdkVersion();
        }

        private static string ResolveInstalledSdkVersion()
        {
            string[] sdkPackageIds = { "com.vrchat.avatars", "com.vrchat.worlds", "com.vrchat.base" };

            foreach (string packageId in sdkPackageIds)
            {
                try
                {
                    string sdkPath = Path.GetFullPath($"Packages/{packageId}/package.json");
                    if (!File.Exists(sdkPath)) continue;

                    Match match = Regex.Match(File.ReadAllText(sdkPath), @"""version""\s*:\s*""([^""]+)""");
                    if (match.Success) return match.Groups[1].Value;
                }
                catch { }
            }

            return "Unknown";
        }

        private void BuildTabs()
        {
            _tabs.Clear();

            AddTab(TabNews, "News", CatStart,
                "Latest ecosystem news, partnerships, and announcements from <b>VixForge Interactive</b>.",
                c => ParseMarkdownAndInject(LoadMarkdownFile("NEWS.md"), c));

            AddTab(TabOverview, "Overview", CatStart,
                "What is in the toolbox and what each part does, straight from the package README.",
                c => ParseMarkdownAndInject(LoadMarkdownFile("README.md"), c));

            AddTab(TabCoreModules, "Core Modules", CatTools,
                "Every tool in the box, opened from here.",
                RenderCoreModules);

            AddTab(TabSupportedModules, "Supported Modules", CatTools,
                "The third-party systems the toolbox knows about and checks for you.",
                RenderSupportedModules);

#if UDON
            AddTab(TabMetrics, "Metrics Engine", CatDocs,
                "How the World Profiler works out the score it gives your scene.",
                c => ParseMarkdownAndInject(LoadMarkdownFile("HOWITWORKS.md"), c));
#endif

            AddTab(TabNetwork, "Network", CatAbout,
                "The repo, the Discord, issue tracker and YouTube channel.",
                RenderNetwork);

            AddTab(TabSupport, "Support", CatAbout,
                "These tools take a lot of R&D. If they save your sanity, consider fueling the engine.",
                RenderSupport);

            AddTab(TabChangelogs, "Changelogs", CatDocs,
                "Review version history, upgrades and fixes.",
                RenderChangelog);
        }

        private void AddTab(string id, string title, string category, string description, System.Action<VisualElement> render)
        {
            _tabs.Add(new HubTab
            {
                Id = id,
                Title = title,
                Category = category,
                Description = description,
                Order = _tabs.Count,
                Render = render
            });
        }

        private static int CategoryRank(string category)
        {
            int i = System.Array.IndexOf(CategoryOrder, category);
            return i < 0 ? CategoryOrder.Length : i;
        }

        private static string DropdownLabel(HubTab tab)
        {
            return $"{tab.Category}  /  {tab.Title}";
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "hub-root";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            var headerRect = new VisualElement { name = "hub-header" };
            var textContainer = new VisualElement();
            textContainer.AddToClassList("hub-header-text-container");
            textContainer.style.height = 120;

            var titleLabel = new Label("<color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> HUB") { enableRichText = true };
            titleLabel.AddToClassList("hub-header-title");
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);

            string sdkText = _sdkVersion != "Unknown" ? $" • VRCSDK {_sdkVersion}" : "";
            var versionLabel = new Label($"v{_packageVersion}{sdkText} • System Online") { style = { color = new Color(0.6f, 0.6f, 0.6f) } };

            textContainer.Add(titleLabel);
            textContainer.Add(versionLabel);
            headerRect.Add(textContainer);
            root.Add(headerRect);

            if (_tabs.Count == 0) BuildTabs();

            var tabContainer = new VisualElement { name = "tab-container" };
            if (_tabs.Count >= DropdownThreshold) BuildTabDropdown(tabContainer);
            else BuildTabButtons(tabContainer);
            root.Add(tabContainer);

            var descContainer = new VisualElement { name = "desc-container" };
            _tabDescription = new Label() { enableRichText = true };
            _tabDescription.AddToClassList("tab-desc-label");
            descContainer.Add(_tabDescription);
            root.Add(descContainer);

            _contentScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            _contentScroll.style.flexGrow = 1;
            _contentScroll.style.flexShrink = 1;

            _contentContainer = new VisualElement();
            _contentContainer.style.flexShrink = 1;
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.paddingBottom = 40;

            _contentScroll.Add(_contentContainer);
            root.Add(_contentScroll);

            HubTab initial = _tabs.FirstOrDefault(t => t.Id == _pendingTabId) ?? _tabs.FirstOrDefault();
            _pendingTabId = null;
            if (initial != null) SwitchMode(initial);
        }

        private void BuildTabButtons(VisualElement container)
        {
            container.style.flexWrap = Wrap.Wrap;
            container.style.flexDirection = FlexDirection.Row;

            foreach (HubTab tab in _tabs)
            {
                HubTab captured = tab;
                captured.Button = new Button(() => SwitchMode(captured)) { text = captured.Title };
                captured.Button.AddToClassList("tab-btn");
                container.Add(captured.Button);
            }
        }

        private void BuildTabDropdown(VisualElement container)
        {
            _dropdownOrder = new List<HubTab>(_tabs);
            _dropdownOrder.Sort((a, b) =>
            {
                int ca = CategoryRank(a.Category), cb = CategoryRank(b.Category);
                if (ca != cb) return ca.CompareTo(cb);
                return a.Order.CompareTo(b.Order);
            });

            container.style.flexDirection = FlexDirection.Row;

            _tabDropdown = new DropdownField();
            _tabDropdown.choices = _dropdownOrder.Select(DropdownLabel).ToList();
            _tabDropdown.AddToClassList("doc-dropdown");
            _tabDropdown.style.flexGrow = 1;
            _tabDropdown.RegisterValueChangedCallback(evt =>
            {
                int i = _tabDropdown.choices.IndexOf(evt.newValue);
                if (i >= 0 && i < _dropdownOrder.Count) SwitchMode(_dropdownOrder[i]);
            });
            container.Add(_tabDropdown);
        }

        private void SyncDropdown(HubTab tab)
        {
            if (_tabDropdown == null || _dropdownOrder == null) return;
            int i = _dropdownOrder.IndexOf(tab);
            if (i >= 0 && _tabDropdown.index != i) _tabDropdown.SetValueWithoutNotify(_tabDropdown.choices[i]);
        }

        private void SelectTab(string id)
        {
            HubTab tab = _tabs.FirstOrDefault(t => t.Id == id);
            if (tab != null) SwitchMode(tab);
        }

        private void SwitchMode(HubTab tab)
        {
            if (tab == null || _contentContainer == null) return;

            SyncDropdown(tab);

            foreach (HubTab t in _tabs)
            {
                if (t.Button == null) continue;
                t.Button.RemoveFromClassList("tab-btn-active");
                t.Button.AddToClassList("tab-btn-inactive");
            }

            if (tab.Button != null)
            {
                tab.Button.RemoveFromClassList("tab-btn-inactive");
                tab.Button.AddToClassList("tab-btn-active");
            }

            _tabDescription.text = tab.Description;
            _contentContainer.Clear();
            tab.Render(_contentContainer);
        }

        private string LoadMarkdownFile(string fileName)
        {
            string path = PackageRoot + fileName;
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset != null) return asset.text;

            string fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath)) return File.ReadAllText(fullPath);

            return $"### Error: File Not Found\nCould not locate `{fileName}` at `{PackageRoot}`. Ensure the VPM package is installed correctly.";
        }

        private void ParseChangelogData()
        {
            _changelogEntries.Clear();
            string rawText = LoadMarkdownFile(ChangelogFile);
            string[] lines = rawText.Split('\n');

            ChangelogEntry currentEntry = null;

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();

                if (line.StartsWith("## ["))
                {
                    currentEntry = new ChangelogEntry
                    {
                        VersionTitle = line.Replace("## ", "").Trim(),
                        Content = ""
                    };
                    _changelogEntries.Add(currentEntry);
                }
                else if (currentEntry != null)
                {
                    currentEntry.Content += line + "\n";
                }
            }
        }

        private void RenderChangelog(VisualElement container)
        {
            if (_changelogEntries.Count == 0) ParseChangelogData();

            if (_changelogEntries.Count == 0)
            {
                ParseMarkdownAndInject($"### Error\nCould not parse changelog versions from `{ChangelogFile}`.", container);
                return;
            }

            var controlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 15, marginTop = 10, paddingLeft = 5, paddingRight = 5 } };

            var dropLabel = new Label("Target Release:") { style = { color = new Color(0.67f, 0.67f, 0.67f), marginRight = 10, fontSize = 14 } };
            if (_cyberFont != null) dropLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            controlRow.Add(dropLabel);

            List<string> versionNames = new List<string>();
            foreach (var log in _changelogEntries) versionNames.Add(log.VersionTitle);

            if (_selectedChangelogIndex < 0 || _selectedChangelogIndex >= versionNames.Count) _selectedChangelogIndex = 0;

            var dropdown = new DropdownField(versionNames, _selectedChangelogIndex);
            dropdown.style.flexGrow = 1;
            dropdown.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            dropdown.style.borderTopColor = new Color(0f, 0.898f, 1f, 0.4f);
            dropdown.style.borderBottomColor = new Color(0f, 0.898f, 1f, 0.4f);
            dropdown.style.borderLeftColor = new Color(0f, 0.898f, 1f, 0.4f);
            dropdown.style.borderRightColor = new Color(0f, 0.898f, 1f, 0.4f);
            dropdown.style.color = new Color(0.9f, 0.9f, 0.9f);

            var logContentContainer = new VisualElement();

            dropdown.RegisterValueChangedCallback(e =>
            {
                int index = versionNames.IndexOf(e.newValue);
                if (index < 0) return;
                _selectedChangelogIndex = index;
                logContentContainer.Clear();
                ParseMarkdownAndInject(_changelogEntries[index].Content, logContentContainer);
            });

            controlRow.Add(dropdown);
            container.Add(controlRow);

            var sep = new VisualElement();
            sep.AddToClassList("md-separator");
            sep.style.backgroundColor = new Color(0f, 0.9f, 1f, 0.3f);
            container.Add(sep);

            container.Add(logContentContainer);

            ParseMarkdownAndInject(_changelogEntries[_selectedChangelogIndex].Content, logContentContainer);
        }

        private void RenderCoreModules(VisualElement container)
        {
            var universalList = new List<(System.Action action, string title, string desc)>
            {
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Unity Engine/Animation Workbench Pro"), "Animation Workbench Pro", "A visual workspace for building and easing animation curves."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Unity Engine/Pipeline Preset Manager"), "Pipeline Preset Manager", "Pulls import settings out of your assets, or creates a preset from scratch.")
            };

            RenderActionGrid(container, "Universal Utilities", "#00e5ff", universalList);
#if !UDON
            var avatarList = new List<(System.Action action, string title, string desc)>
            {
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Badge Studio"), "Badge Studio", "Builds VRChat convention badges for you, ready to wear."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Quest Conversion Engine"), "Quest Conversion Engine", "Makes a Quest copy of your avatar without touching the original, checked against every Android limit."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/PhysBone Blueprints"), "PhysBone Blueprints", "Saves every PhysBone setup on an avatar as a blueprint, then puts it back on another one."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Animator Forge"), "Animator Forge", "Diagnoses broken animators (missing params, mixed Write Defaults, menu desync) and forges fully-rigged toggles, sliders, swaps, and exclusive groups."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Material Conflict Finder"), "Material Conflict Finder", "Finds materials that disagree on a shader setting, leftover keywords, and toggles your animations or VRCFury fight over, then syncs them for you."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Accessory Engine"), "Accessory Mounting Engine", "Clones a clean armature and mounts accessories onto it, either by re-rigging them or with parent constraints."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Optimization Suite"), "Optimization Suite", "Checks your avatar against VRChat limits, then shrinks meshes and textures to fit.")
            };

            RenderActionGrid(container, "Avatar Tools", "#ff00aa", avatarList);
#elif UDON
            bool isSnapActive = EditorPrefs.GetBool("VixenTools/Scene/Live Surface Snapping", false);
            string snapTitle = isSnapActive
                ? "<color=#00e5ff>Live Surface Snapping [ ACTIVE ]</color>"
                : "Live Surface Snapping [ OFF ]";

            bool isPrecisionActive = EditorPrefs.GetBool("VixenTools/Scene/Precision Click-to-Place", false);
            string precisionTitle = isPrecisionActive
                ? "<color=#00e5ff>Precision Click-to-Place [ ACTIVE ]</color>"
                : "Precision Click-to-Place [ OFF ]";

            var worldList = new List<(System.Action action, string title, string desc)>
            {
                (() =>
                {
                    EditorApplication.ExecuteMenuItem("VixenTools/Scene/Live Surface Snapping");
                    SelectTab(TabCoreModules);
                },
                snapTitle,
                "Enterprise-grade gravity snapper. Automatically drops selected objects to the nearest floor or shelf when moved."),

                (() =>
                {
                    EditorApplication.ExecuteMenuItem("VixenTools/Scene/Precision Click-to-Place");
                    SelectTab(TabCoreModules);
                },
                precisionTitle,
                "Sniper-rifle camera raycaster. Click anywhere in the Scene View to instantly teleport objects to complex shelf polygons."),

                (() =>
                {
                    EditorApplication.ExecuteMenuItem("VixenTools/Scene/Vixen World Engine");
                    SelectTab(TabCoreModules);
                },
                "<color=#00e5ff>Vixen World Engine</color>",
                "Audits your whole world: ProTV, TXL, IwaSync3, video players, Udon persistence and shader swaps.")
            };

            RenderActionGrid(container, "WORLD TOOLS", "#ff00aa", worldList);
#endif
        }

        private void RenderSupportedModules(VisualElement container)
        {
            string markdown = @"
**VixForge tools** are built to work alongside the big third-party systems. The World Engine and Avatar Validators actively audit, scan, and protect these ecosystems natively.
";
            ParseMarkdownAndInject(markdown, container);

            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://protv.dev/"),
                    "ProTV (Techanon)",
                    "Audits your ProTV setup, resolves GSV conflicts and checks the AudioLink handshake."),

                (() => Application.OpenURL("https://github.com/llealloo/audiolink"),
                    "AudioLink",
                    "Reflective extraction of internal FFT textures, orphan detection, and global whitelist protection."),

                (() => Application.OpenURL("https://ltcgi.dev/"),
                    "LTCGI",
                    "Area lighting checks, ghost screen cleanup and unsticking a jammed bake cache."),

                (() => Application.OpenURL("https://xtlcdn.github.io/VizVid/"),
                    "VizVid (VVMW)",
                    "Video player checks, unlinked interface detection and Quest fallback validation."),

                (() => Application.OpenURL("https://github.com/vrctxl/VideoTXL"),
                    "Video TXL",
                    "CRT render ecosystem validation, GC sink detection, and Playlist Queue access control integration."),

                (() => Application.OpenURL("https://booth.pm/en/items/2666275"),
                    "iwaSync3",
                    "Network sync frequency tuning, blinding emissive bounds detection, and global 2D audio isolation."),

                (() => Application.OpenURL("https://rinvo.booth.pm/items/5757644"),
                    "YouTube Search (Rinvo)",
                    "Autonomous video player target linking, UI decoupling, and API pool size validation."),

                (() => Application.OpenURL("https://github.com/REDSIM/VRCLightVolumes"),
                    "VRC Light Volumes",
                    "Compute load detection, sphere threshold optimization, and TVGI/AudioLink strobe safety enforcement."),

                (() => Application.OpenURL("https://github.com/AcChosen/VR-Stage-Lighting"),
                    "VR Stage Lighting",
                    "Regex-based heuristic protection and DMX audit support.")
            };

            RenderActionGrid(container, "Ecosystem Integrations", "#00e5ff", list);
        }

        private void RenderNetwork(VisualElement container)
        {
            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://github.com/VixenCreations/VixenToolBox"), "GitHub Repository", "Core ecosystem source code and release tracking."),
                (() => Application.OpenURL("https://x.com/VixForge"), "Twitter", "Where I post All Kinds of things and Interact with the community."),
                (() => Application.OpenURL("https://discord.com/invite/3vbJCKcPtJ"), "Discord", "My Official Community to get help with things."),
                (() => Application.OpenURL("https://github.com/VixenCreations/VixenToolBox/issues"), "Report An Issue", "Report an Issue or Request a new feature."),
                (() => Application.OpenURL("https://www.youtube.com/@vixenlicous"), "YouTube Channel", "Technical breakdowns, tutorials and development logs.")
            };

            RenderActionGrid(container, "Ecosystem Routing", "#00e5ff", list);
        }

        private void RenderSupport(VisualElement container)
        {
            string markdown = @"
If my code has ever saved your scene from completely bricking, optimized your Quest fallback in under 10 seconds, or just made your workflow suck a little bit less... consider throwing a coffee my way:
";
            ParseMarkdownAndInject(markdown, container);

            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://ko-fi.com/vixenlicous"), "Ko-Fi Donation", "One-Time Support"),
                (() => Application.OpenURL("https://vixenlicous.gumroad.com/coffee"), "Gumroad Donation", "One-Time Donation"),
                (() => Application.OpenURL("https://cash.app/$VixenVRC"), "CashApp Donation", "Direct Transfer"),
                (() => Application.OpenURL("https://www.patreon.com/cw/Vixenlicious"), "Patreon Donation", "Monthly Support"),
                (() => Application.OpenURL("https://jinxxy.com/Vixenlicious"), "Jinxxy", "Full Asset Storefront"),
                (() => Application.OpenURL("https://vixenlicous.gumroad.com/"), "Gumroad", "Full Asset Storefront")
            };

            RenderActionGrid(container, "Support The Projects", "#ff00aa", list);
        }

        private void RenderActionGrid(VisualElement container, string headerText, string accentHex, List<(System.Action action, string title, string desc)> items)
        {
            var header = new Label($"<color={accentHex}>{headerText}</color>") { enableRichText = true };
            header.AddToClassList("md-h1");
            if (_cyberFont != null) header.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            container.Add(header);

            var sep = new VisualElement();
            sep.AddToClassList("md-separator");
            ColorUtility.TryParseHtmlString(accentHex, out Color c);
            c.a = 0.3f;
            sep.style.backgroundColor = c;
            container.Add(sep);

            var grid = new VisualElement();
            grid.AddToClassList("link-grid");

            foreach (var item in items)
            {
                var card = new VisualElement();
                card.AddToClassList("link-card");

                var btn = new Button(item.action) { text = item.title };
                btn.AddToClassList("link-card-btn");

                var desc = new Label(item.desc) { enableRichText = true };
                desc.AddToClassList("link-card-desc");

                card.Add(btn);
                card.Add(desc);
                grid.Add(card);
            }

            container.Add(grid);
        }

        private void ParseMarkdownAndInject(string text, VisualElement container)
        {
            string[] lines = text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("|"))
                {
                    string[] cells = line.Trim().Trim('|').Split('|');

                    bool isSeparator = true;
                    foreach (string c in cells)
                    {
                        if (!Regex.IsMatch(c.Trim(), @"^:?-{2,}:?$")) { isSeparator = false; break; }
                    }
                    if (isSeparator) continue;

                    string next = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";
                    bool isHeader = next.StartsWith("|") && Regex.IsMatch(next.Replace("|", "").Trim(), @"^[:\-\s]+$") && next.Contains("-");

                    var tableRow = new VisualElement();
                    tableRow.style.flexDirection = FlexDirection.Row;
                    tableRow.style.width = new StyleLength(Length.Percent(100));
                    tableRow.style.borderBottomWidth = 1;
                    tableRow.style.borderBottomColor = new Color(0f, 0.9f, 1f, 0.15f);
                    tableRow.style.paddingTop = 2;
                    tableRow.style.paddingBottom = 2;

                    foreach (string c in cells)
                    {
                        var cellLbl = new Label(ParseMarkdownFormatting(c.Trim())) { enableRichText = true };
                        cellLbl.AddToClassList("md-p");
                        cellLbl.style.flexGrow = 1;
                        cellLbl.style.flexBasis = 0;
                        cellLbl.style.flexShrink = 1;
                        cellLbl.style.whiteSpace = WhiteSpace.Normal;
                        cellLbl.style.paddingRight = 8;
                        if (isHeader) cellLbl.style.color = new Color(0f, 0.898f, 1f);
                        tableRow.Add(cellLbl);
                    }
                    container.Add(tableRow);
                }
                else if (line.StartsWith("---") || line.StartsWith("***"))
                {
                    var sep = new VisualElement();
                    sep.AddToClassList("md-separator");
                    sep.style.backgroundColor = new Color(1f, 0f, 0.66f, 0.3f);
                    container.Add(sep);
                }
                else if (line.StartsWith("# ") || line.StartsWith("## ") || line.StartsWith("### "))
                {
                    int hashes = 0;
                    while (hashes < line.Length && line[hashes] == '#') hashes++;

                    string hText = line.Substring(hashes).Trim();
                    hText = ParseMarkdownFormatting(hText);
                    var lbl = new Label(hText) { enableRichText = true };

                    if (hashes == 1) lbl.AddToClassList("md-h1");
                    else if (hashes == 2) lbl.AddToClassList("md-h2");
                    else lbl.AddToClassList("md-h3");

                    if (_cyberFont != null) lbl.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
                    container.Add(lbl);

                    if (hashes <= 2)
                    {
                        var sep = new VisualElement();
                        sep.AddToClassList("md-separator");
                        sep.style.backgroundColor = new Color(0f, 0.9f, 1f, 0.3f);
                        container.Add(sep);
                    }
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    string pText = line.Substring(2).Trim();
                    pText = ParseMarkdownFormatting(pText);

                    var row = new VisualElement();
                    row.AddToClassList("md-row");
                    row.style.width = new StyleLength(Length.Percent(100));
                    row.style.maxWidth = new StyleLength(Length.Percent(100));
                    row.style.flexShrink = 1;

                    var bullet = new Label(">>") { style = { color = new Color(1f, 0f, 0.66f) } };
                    bullet.AddToClassList("md-bullet");
                    bullet.style.flexShrink = 0;

                    var lbl = new Label(pText) { enableRichText = true };
                    lbl.AddToClassList("md-p");
                    lbl.style.whiteSpace = WhiteSpace.Normal;
                    lbl.style.flexShrink = 1;
                    lbl.style.flexGrow = 1;
                    lbl.style.width = new StyleLength(Length.Percent(100));
                    lbl.style.maxWidth = new StyleLength(Length.Percent(100));

                    row.Add(bullet);
                    row.Add(lbl);
                    container.Add(row);
                }
                else if (line.StartsWith("> "))
                {
                    string pText = line.Substring(2).Trim();
                    pText = ParseMarkdownFormatting(pText);
                    var lbl = new Label(pText) { enableRichText = true };
                    lbl.AddToClassList("md-p");
                    lbl.style.whiteSpace = WhiteSpace.Normal;
                    lbl.style.flexShrink = 1;
                    lbl.style.width = new StyleLength(Length.Percent(100));
                    lbl.style.maxWidth = new StyleLength(Length.Percent(100));
                    lbl.style.color = new Color(0.7f, 0.7f, 0.7f);
                    lbl.style.borderLeftWidth = 3;
                    lbl.style.borderLeftColor = new Color(0f, 0.9f, 1f);
                    lbl.style.paddingLeft = 10;
                    lbl.style.marginLeft = 5;
                    lbl.style.marginTop = 5;
                    lbl.style.marginBottom = 5;
                    container.Add(lbl);
                }
                else
                {
                    string pText = ParseMarkdownFormatting(line);
                    var lbl = new Label(pText) { enableRichText = true };

                    if (pText.StartsWith("<b>")) lbl.AddToClassList("md-h2");
                    else lbl.AddToClassList("md-p");

                    container.Add(lbl);
                }
            }
        }

        private string ParseMarkdownFormatting(string text)
        {
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b><color=#00e5ff>$1</color></b>");
            text = Regex.Replace(text, @"\*(.*?)\*", "<i>$1</i>");
            text = Regex.Replace(text, @"\`(.*?)\`", "<color=#ffaa00><i>$1</i></color>");
            text = Regex.Replace(text, @"\[(.*?)\]\(.*?\)", "<b><color=#ff00aa>$1</color></b>");
            return text;
        }
    }
}
#endif
