#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace VixenTools.Editor
{
    public partial class VixenHub : EditorWindow
    {
        private const string PackageName = "com.vixencreations.vixens-toolbox";
        private const string PackageRoot = "Packages/" + PackageName + "/";
        private const string UssPath = PackageRoot + "Editor/UiStyles/VixenHubStyles.uss";
        private const string ChangelogSuffix = "CHANGELOG.md";
        private const string SiteRoot = "https://vixencreations.github.io/VixenToolBox/";
        private const string WindowTitle = "VixForge Hub";
        private const string BrandTitle = "<color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> HUB";
        private const string FindCommand = "Find";

        private const string TabHome = "home";
        private const string TabNews = "news";
        private const string TabOverview = "overview";
        private const string TabCoreModules = "core-modules";
        private const string TabSupportedModules = "supported-modules";
        private const string TabMetrics = "metrics-engine";
        private const string TabChangelogs = "changelogs";
        private const string TabSupporters = "supporters";
        private const string TabSupport = "support";
        private const string TabNetwork = "network";
        private const string TabPrivacy = "privacy";
        private const string TabLicense = "license";
        private const string TabThirdParty = "third-party";

        private const string CatStart = "Start Here";
        private const string CatTools = "Tools";
        private const string CatDocs = "Docs";
        private const string CatAbout = "About";
        private const string CatMore = "More";

        private static readonly string[] CategoryOrder = { CatStart, CatTools, CatDocs, CatAbout, CatMore };

        private const string GroupUnity = "Unity Tools";
        private const string GroupAvatar = "Avatar Tools";
        private const string GroupWorld = "World Tools";

#if UDON
        private static readonly bool WorldProject = true;
#else
        private static readonly bool WorldProject = false;
#endif

        private class DocMeta
        {
            public string Id;
            public string Title;
            public string Description;
            public string Category;
            public int Order;
            public bool WorldOnly;
        }

        private static readonly Dictionary<string, DocMeta> KnownDocs = new Dictionary<string, DocMeta>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "NEWS.md", new DocMeta { Id = TabNews, Title = "News", Category = CatStart, Order = 0, Description = "What's new in this version, and news from VixForge Interactive." } },
            { "README.md", new DocMeta { Id = TabOverview, Title = "Overview", Category = CatStart, Order = 1, Description = "What is in the toolbox and what each part does." } },
            { "CHANGELOG.md", new DocMeta { Id = TabChangelogs, Title = "Changelog", Category = CatDocs, Order = 20, Description = "Every release, and what it added, changed and fixed." } },
            { "HOWITWORKS.md", new DocMeta { Id = TabMetrics, Title = "How The Score Works", Category = CatDocs, Order = 21, WorldOnly = true, Description = "How the World Engine's Performance Map works out its score." } },
            { "SUPPORTERS.md", new DocMeta { Id = TabSupporters, Title = "Supporters", Category = CatAbout, Order = 30, Description = "Everyone backing the toolbox on Patreon and Ko-fi." } },
            { "SUPPORT.md", new DocMeta { Id = TabSupport, Title = "Support", Category = CatAbout, Order = 31, Description = "The toolbox is free. If it saves you time, here is how to help keep it going." } },
            { "LICENSE.md", new DocMeta { Id = TabLicense, Title = "MIT License", Category = CatAbout, Order = 34, Description = "The toolbox's own licence: free to use, change and share, as long as the copyright notice stays." } },
            { "Third Party Notices.md", new DocMeta { Id = TabThirdParty, Title = "Third Party Notices", Category = CatAbout, Order = 35, Description = "Magick.NET, ImageMagick and the libraries built into them, with their licences." } },
        };

        private const string SupportedPackagesIntro =
            "The **World Engine** checks each of these packages in your scene and fixes what it can. Click one to open its own page.";

        private const string PrivacyMarkdown =
            "## Your Privacy\n" +
            "The toolbox runs inside Unity on your computer. It never connects to the internet, and it sends nothing to us or to anyone else.\n\n" +
            "* **Settings** stay in Unity's editor preferences on this computer.\n" +
            "* **Your Assets folder** only changes when you use a tool. ImageMagick keeps its settings file in your project's Library folder.\n" +
            "* **Buttons and links** in the Hub that point to our website, Discord or stores open them in your web browser.\n\n" +
            "## The Full Details\n" +
            "Our Privacy Policy covers the toolbox, our shaders, VixForge Camera Pro, our Discord server and its bot, and our memberships. Our Terms of Service are the rules for using all of them.\n";

        private class HubPage
        {
            public string Id;
            public string Title;
            public string Description;
            public string Category;
            public int Order;
            public string FileName;
            public System.Action<VisualElement> Render;
            public System.Func<string> Source;
            public VisualElement NavItem;
        }

        private class HubAction
        {
            public string Group;
            public string Title;
            public string Description;
            public string Where;
            public System.Action Run;
            public System.Func<bool> IsOn;
        }

        private readonly List<HubPage> _pages = new List<HubPage>();
        private HubPage _activePage;
        [SerializeField] private string _activePageId;
        private string _pendingPageId;

        private string _packageVersion = "Unknown";
        private string _sdkVersion = "Unknown";

        private Label _pageCrumb;
        private Label _pageTitle;
        private Label _pageDescription;
        private ScrollView _contentScroll;
        private VisualElement _contentContainer;

        private static readonly Regex CommentLine = new Regex(@"^[ \t]*<!--.*?-->[ \t]*\r?\n?", RegexOptions.Multiline);
        private static readonly Regex VersionField = new Regex(@"""version""\s*:\s*""([^""]+)""");

        [MenuItem("VixenTools/Hub Dashboard")]
        public static void ShowWindow()
        {
            OpenHub();
        }

        public static void ShowChangelogWindow()
        {
            OpenHub().OpenTab(TabChangelogs);
        }

        public void OpenChangelogTab()
        {
            OpenTab(TabChangelogs);
        }

        private static VixenHub OpenHub()
        {
            var window = GetWindow<VixenHub>(WindowTitle);
            if (window._minimized) window.SetMinimized(false);
            else if (window.minSize.x < NormalMinSize.x) window.minSize = NormalMinSize;
            window.Show();
            return window;
        }

        public void OpenTab(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (_contentContainer == null)
            {
                _pendingPageId = id;
                return;
            }

            if (_minimized) SetMinimized(false);
            ClearSearch();
            ShowPage(FindPage(id));
            Repaint();
        }

        private void OnEnable()
        {
            LoadVersionData();
            BuildPages();
        }

        private static string PackageDirectory()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackageRoot + "package.json");
            if (info != null && !string.IsNullOrEmpty(info.resolvedPath)) return info.resolvedPath;
            return Path.GetFullPath(PackageRoot);
        }

        private void LoadVersionData()
        {
            string path = Path.Combine(PackageDirectory(), "package.json");
            if (File.Exists(path))
            {
                Match vMatch = VersionField.Match(File.ReadAllText(path));
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

                    Match match = VersionField.Match(File.ReadAllText(sdkPath));
                    if (match.Success) return match.Groups[1].Value;
                }
                catch { }
            }

            return "Unknown";
        }

        private static List<string> DiscoverDocs()
        {
            var files = new List<string>();
            try
            {
                string dir = PackageDirectory();
                if (Directory.Exists(dir))
                {
                    foreach (string full in Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly))
                        files.Add(Path.GetFileName(full));
                }
            }
            catch { }

            if (files.Count == 0) files.AddRange(KnownDocs.Keys);
            return files;
        }

        private void BuildPages()
        {
            _pages.Clear();
            _searchIndex = null;
            _changelogEntries.Clear();

            AddPage(TabHome, "Home", null, 0, "Every page and every tool in the toolbox, in one place.", RenderHome, null);

            foreach (string file in DiscoverDocs())
            {
                KnownDocs.TryGetValue(file, out DocMeta meta);
                if (meta != null && meta.WorldOnly && !WorldProject) continue;

                string captured = file;
                bool changelog = file.EndsWith(ChangelogSuffix, System.StringComparison.OrdinalIgnoreCase);
                string pretty = PrettifyDocName(file);

                var page = new HubPage
                {
                    Id = meta != null ? meta.Id : file.ToLowerInvariant(),
                    Title = meta != null ? meta.Title : pretty,
                    Description = meta != null ? meta.Description : $"{pretty}, from the toolbox's own files.",
                    Category = meta != null ? meta.Category : (changelog ? CatDocs : CatMore),
                    Order = meta != null ? meta.Order : 100,
                    FileName = file,
                    Source = () => LoadMarkdownFile(captured)
                };

                if (changelog) page.Render = c => RenderChangelog(captured, c);
                else if (page.Id == TabSupporters) page.Render = c => RenderSupporters(captured, c);
                else if (page.Id == TabSupport) page.Render = c => RenderSupport(captured, c);
                else page.Render = c => RenderDocument(captured, c);

                _pages.Add(page);
            }

            AddPage(TabCoreModules, "All Tools", CatTools, 10,
                "Every tool in the toolbox, one click away.",
                RenderTools, null);

            if (WorldProject)
            {
                AddPage(TabSupportedModules, "Supported Packages", CatTools, 11,
                    "The third-party packages the World Engine knows and checks for you.",
                    RenderSupportedPackages, () => SupportedPackagesIntro);
            }

            AddPage(TabNetwork, "Links", CatAbout, 32,
                "Our website, the source code, the Discord, and where to report a problem.",
                RenderLinks, null);

            AddPage(TabPrivacy, "Privacy & Terms", CatAbout, 33,
                "What the toolbox does with your information, and the rules for using it.",
                RenderPrivacy, () => PrivacyMarkdown);

            _pages.Sort((a, b) =>
            {
                int ca = CategoryRank(a.Category), cb = CategoryRank(b.Category);
                if (ca != cb) return ca.CompareTo(cb);
                if (a.Order != b.Order) return a.Order.CompareTo(b.Order);
                return string.Compare(a.Title, b.Title, System.StringComparison.OrdinalIgnoreCase);
            });
        }

        private void AddPage(string id, string title, string category, int order, string description, System.Action<VisualElement> render, System.Func<string> source)
        {
            _pages.Add(new HubPage
            {
                Id = id,
                Title = title,
                Category = category,
                Description = description,
                Order = order,
                Render = render,
                Source = source
            });
        }

        private static int CategoryRank(string category)
        {
            if (category == null) return -1;
            int i = System.Array.IndexOf(CategoryOrder, category);
            return i < 0 ? CategoryOrder.Length : i;
        }

        private HubPage FindPage(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _pages.FirstOrDefault(p => p.Id == id);
        }

        private static string PrettifyDocName(string fileName)
        {
            string raw = Path.GetFileNameWithoutExtension(fileName).Replace('_', ' ').Replace('-', ' ');
            string[] words = raw.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i];
                words[i] = char.ToUpperInvariant(w[0]) + (w.Length > 1 ? w.Substring(1).ToLowerInvariant() : "");
            }
            return words.Length > 0 ? string.Join(" ", words) : fileName;
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "hub-root";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            if (_pages.Count == 0) BuildPages();

            VisualElement header = BuildHeader();
            root.Add(header);
            SetUpFrame(root, header);

            var body = new VisualElement { name = "hub-body" };
            body.Add(BuildReader());
            body.Add(BuildSidebar());
            root.Add(body);

            HookShortcuts(root);

            HubPage initial = FindPage(_pendingPageId) ?? FindPage(_activePageId) ?? _pages.FirstOrDefault();
            _pendingPageId = null;
            ShowPage(initial);

            ApplyFrame();
        }

        private VisualElement BuildHeader()
        {
            var header = new VisualElement { name = "hub-header" };

            var library = new Button(ToggleSidebar) { text = "Library", tooltip = SidebarTip };
            library.AddToClassList("hub-frame-btn");
            library.AddToClassList("hub-library-btn");
            header.Add(library);

            var brand = new VisualElement();
            brand.AddToClassList("hub-brand");

            var title = new Label(BrandTitle) { enableRichText = true };
            title.AddToClassList("hub-header-title");
            brand.Add(title);
            brand.Add(Chip("v" + _packageVersion));
            if (_sdkVersion != "Unknown") brand.Add(Chip("VRChat SDK " + _sdkVersion, "hub-chip--sdk"));

            header.Add(brand);
            return header;
        }

        private VisualElement BuildReader()
        {
            var reader = new VisualElement { name = "hub-reader" };
            reader.RegisterCallback<PointerDownEvent>(_ => CloseDrawer());

            _contentScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };
            _contentScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _contentScroll.RegisterCallback<PointerUpLinkTagEvent>(OnLinkClicked);

            var head = new VisualElement { name = "hub-page-head" };
            _pageCrumb = new Label();
            _pageCrumb.AddToClassList("hub-page-crumb");
            _pageTitle = new Label { enableRichText = true };
            _pageTitle.AddToClassList("hub-page-title");
            _pageDescription = new Label();
            _pageDescription.AddToClassList("hub-page-desc");
            head.Add(_pageCrumb);
            head.Add(_pageTitle);
            head.Add(_pageDescription);

            _contentContainer = new VisualElement { name = "hub-page-body" };

            _contentScroll.Add(head);
            _contentScroll.Add(_contentContainer);
            reader.Add(_contentScroll);
            return reader;
        }

        private static Label Chip(string text, string extraClass = null)
        {
            var chip = new Label(text);
            chip.AddToClassList("hub-chip");
            if (extraClass != null) chip.AddToClassList(extraClass);
            return chip;
        }

        private void SetPageHead(string crumb, string title, string description)
        {
            _pageCrumb.text = (crumb ?? "").ToUpperInvariant();
            _pageTitle.text = title ?? "";
            _pageDescription.text = description ?? "";
            _pageDescription.EnableInClassList("hub-hidden", string.IsNullOrEmpty(description));
        }

        private void ShowPage(HubPage page, string anchor = null)
        {
            if (page == null || _contentContainer == null) return;

            bool samePage = _activePage == page && !_searching;
            _activePage = page;
            _activePageId = page.Id;
            _searching = false;

            SetPageHead(page.Category ?? "Vixens Toolbox", page.Title, page.Description);

            _headings.Clear();
            _contentContainer.Clear();
            page.Render(_contentContainer);

            RefreshNav();
            if (!samePage) _contentScroll.scrollOffset = Vector2.zero;
            if (!string.IsNullOrEmpty(anchor)) ScrollToHeading(anchor);

            UpdateCompactLocation();
            CloseDrawer();
        }

        private void RenderDocument(string fileName, VisualElement container)
        {
            string text = LoadMarkdownFile(fileName);
            string title = TakeLeadingTitle(ref text);
            if (title != null) _pageTitle.text = FormatInline(title);
            RenderMarkdown(text, container);
        }

        private void RenderSupport(string fileName, VisualElement container)
        {
            RenderDocument(fileName, container);

            RenderActionGrid(container, "Memberships", new List<HubAction>
            {
                WebLink(null, "Every Tier", SiteRoot + "support.html", "What each membership includes, on our website."),
                PageLink(null, "Supporters", TabSupporters, "The people already on the wall.")
            });
        }

        private void RenderPrivacy(VisualElement container)
        {
            RenderMarkdown(PrivacyMarkdown, container);

            RenderActionGrid(container, "Read More", new List<HubAction>
            {
                WebLink(null, "Privacy Policy", SiteRoot + "privacy.html", "What our website, tools, shaders, Discord server and bot do with your information."),
                WebLink(null, "Terms of Service", SiteRoot + "terms.html", "The rules for using our website, products, Discord server and memberships."),
                PageLink(null, "MIT License", TabLicense, "The toolbox's own licence, right here in the Hub."),
                PageLink(null, "Third Party Notices", TabThirdParty, "The licences of the parts of the toolbox we didn't write.")
            });
        }

        private void RenderTools(VisualElement container)
        {
            foreach (IGrouping<string, HubAction> group in ToolActions().GroupBy(a => a.Group))
                RenderActionGrid(container, group.Key, group.ToList());
        }

        private void RenderSupportedPackages(VisualElement container)
        {
            RenderMarkdown(SupportedPackagesIntro, container);
            RenderActionGrid(container, "Supported Packages", PackageActions());
        }

        private void RenderLinks(VisualElement container)
        {
            RenderActionGrid(container, null, LinkActions());
        }

        private List<HubAction> ToolActions()
        {
            var list = new List<HubAction>
            {
                MenuTool(GroupUnity, "Animation Workbench Pro", "VixenTools/Unity Engine/Animation Workbench Pro", "Build and ease animation curves, with a live preview on your scene object."),
                MenuTool(GroupUnity, "Pipeline Preset Manager", "VixenTools/Unity Engine/Pipeline Preset Manager", "Turns import settings into presets, and can make one the default for everything you import next."),
                new HubAction
                {
                    Group = GroupUnity,
                    Title = "Scene View Enhancer",
                    Where = VixenSceneViewEnhancer.MenuPath,
                    Description = "Caps the editor's frame rate while it's idle, and adds a Scene view panel showing redraws and memory.",
                    Run = () => VixenSceneViewEnhancer.SetEnabled(!VixenSceneViewEnhancer.IsEnabled),
                    IsOn = () => VixenSceneViewEnhancer.IsEnabled
                },
                new HubAction
                {
                    Group = GroupUnity,
                    Title = "Memory Diagnostics",
                    Where = VixenMemoryDiagnostics.MenuPath,
                    Description = "Shows which textures, materials and meshes a tool made and never freed.",
                    Run = VixenMemoryDiagnostics.Open
                },
                MenuTool(GroupUnity, "Fix Scene Data", "VixenTools/Unity Engine/Fix Scene Data", "Reattaches a scene's lighting data when it has gone missing.")
            };
#if !UDON
            list.Add(MenuTool(GroupAvatar, "Optimization Suite", "VixenTools/Avatars/Optimization Suite", "Checks your avatar against VRChat's limits, then shrinks meshes and textures to fit."));
            list.Add(MenuTool(GroupAvatar, "Quest Conversion Engine", "VixenTools/Avatars/Quest Conversion Engine", "Makes a Quest copy of your avatar and leaves the original alone."));
            list.Add(MenuTool(GroupAvatar, "Animator Forge", "VixenTools/Avatars/Animator Forge", "Finds problems in your animator, and builds toggles, sliders, swaps and exclusive groups for you."));
            list.Add(MenuTool(GroupAvatar, "Material Conflict Finder", "VixenTools/Avatars/Material Conflict Finder", "Finds materials whose toggles disagree and keywords left switched on, then settles them."));
            list.Add(MenuTool(GroupAvatar, "PhysBone Blueprints", "VixenTools/Avatars/PhysBone Blueprints", "Saves an avatar's PhysBones and puts them back on another one."));
            list.Add(MenuTool(GroupAvatar, "Accessory Mounting Engine", "VixenTools/Avatars/Accessory Engine", "Mounts accessories onto a clean copy of your armature."));
            list.Add(MenuTool(GroupAvatar, "Badge Studio", "VixenTools/Avatars/Badge Studio", "Builds VRChat convention badges, Furality included."));
#else
            list.Add(MenuTool(GroupWorld, "Vixen World Engine", "VixenTools/Scene/Vixen World Engine", "Runs more than 160 checks on your world and fixes the ones you tick."));
            list.Add(MenuTool(GroupWorld, "Quest World Converter", "VixenTools/Scene/Quest World Converter", "Makes Quest copies of your world's materials and leaves the originals alone."));
            list.Add(MenuToggle(GroupWorld, "Live Surface Snapping", "VixenTools/Scene/Live Surface Snapping", "Drops selected objects onto the floor or shelf below them as you move them."));
            list.Add(MenuToggle(GroupWorld, "Precision Click-to-Place", "VixenTools/Scene/Precision Click-to-Place", "Click in the Scene view to move the selection onto that surface."));
            list.Add(MenuTool(GroupWorld, "Drop to Surface", "VixenTools/Scene/Drop to Surface", "Drops the selected objects onto the surface below them, once. Shortcut: Ctrl+Alt+S."));
            list.Add(MenuTool(GroupWorld, "Omni-Chaos Generator", "VixenTools/QA/Generate Omni-Chaos Environment", "Builds a test scene full of deliberate problems for the World Engine to catch."));
#endif
            return list;
        }

        private List<HubAction> PackageActions()
        {
            const string g = "Supported Packages";
            return new List<HubAction>
            {
                WebLink(g, "ProTV (Techanon)", "https://protv.dev/", "Checks screens, speakers and the AudioLink link, Quest fallbacks, global video textures, and heavy UI or queue settings."),
                WebLink(g, "AudioLink", "https://github.com/llealloo/audiolink", "Finds a missing or doubled AudioLink, video players and materials that are not linked to it, and a setting that stalls Quest."),
                WebLink(g, "LTCGI", "https://ltcgi.dev/", "Finds screens with no video texture, old screen data that needs a rebuild, a stuck bake, and AudioLink screens with no AudioLink."),
                WebLink(g, "VizVid (VVMW)", "https://xtlcdn.github.io/VizVid/", "Checks player and UI handlers, the rate limit resolver, Quest fallbacks, 2D audio bleed and the AudioLink link."),
                WebLink(g, "Video TXL", "https://github.com/vrctxl/VideoTXL", "Checks screen render textures, the queue and access settings, the AudioLink link, audio zones, portals, chairs and material swappers."),
                WebLink(g, "iwaSync3", "https://booth.pm/en/items/2666275", "Flags very frequent sync, overbright screens, global 2D speakers, high default resolution and playlists that fetch without limit."),
                WebLink(g, "YouTube Search (Rinvo)", "https://rinvo.booth.pm/items/5757644", "Checks that search results reach the right player UI for ProTV, TXL, iwaSync3 and USharpVideo, and the size of its request pool."),
                WebLink(g, "VRC Light Volumes", "https://github.com/REDSIM/VRCLightVolumes", "Flags more than one manager, too many volumes or point lights, volumes with no bake, shadow bakers, area lights, and AudioLink and TVGI flicker settings."),
                WebLink(g, "VR Stage Lighting", "https://github.com/AcChosen/VR-Stage-Lighting", "Flags AudioLink fixtures in a scene with no AudioLink."),
                WebLink(g, "GPU Particle Volumes", "https://github.com/REDSIM/GPUParticleVolumes", "Flags a manager with no mesh, volumes that update every frame, and empty volume slots.")
            };
        }

        private List<HubAction> LinkActions()
        {
            const string g = "Links";
            return new List<HubAction>
            {
                WebLink(g, "Website", SiteRoot, "Docs, changelogs, and adding the toolbox to the VRChat Creator Companion."),
                WebLink(g, "GitHub", "https://github.com/VixenCreations/VixenToolBox", "The toolbox's source code and every release."),
                WebLink(g, "Report a Problem", "https://github.com/VixenCreations/VixenToolBox/issues", "Report a bug or ask for a feature."),
                WebLink(g, "Discord", "https://discord.com/invite/3vbJCKcPtJ", "Our community, and the quickest place to get help."),
                WebLink(g, "X", "https://x.com/VixForge", "News and previews of what's coming."),
                WebLink(g, "YouTube", "https://www.youtube.com/@vixenlicous", "Tutorials, breakdowns and development logs.")
            };
        }

        private static HubAction MenuTool(string group, string title, string menuPath, string description)
        {
            return new HubAction
            {
                Group = group,
                Title = title,
                Where = menuPath,
                Description = description,
                Run = () => EditorApplication.ExecuteMenuItem(menuPath)
            };
        }

        private static HubAction MenuToggle(string group, string title, string menuPath, string description)
        {
            HubAction action = MenuTool(group, title, menuPath, description);
            action.IsOn = () => EditorPrefs.GetBool(menuPath, false);
            return action;
        }

        private static HubAction WebLink(string group, string title, string url, string description)
        {
            return new HubAction
            {
                Group = group,
                Title = title,
                Where = ShortUrl(url),
                Description = description,
                Run = () => Application.OpenURL(url)
            };
        }

        private HubAction PageLink(string group, string title, string pageId, string description)
        {
            return new HubAction
            {
                Group = group,
                Title = title,
                Description = description,
                Run = () => OpenTab(pageId)
            };
        }

        private static string ShortUrl(string url)
        {
            try
            {
                var uri = new System.Uri(url);
                string host = uri.Host.StartsWith("www.") ? uri.Host.Substring(4) : uri.Host;
                return host + uri.AbsolutePath.TrimEnd('/');
            }
            catch
            {
                return url;
            }
        }

        private void RenderActionGrid(VisualElement container, string heading, List<HubAction> actions)
        {
            if (!string.IsNullOrEmpty(heading))
            {
                var title = new Label(heading);
                title.AddToClassList("hub-section-title");
                container.Add(title);
                _headings.Add((2, heading, title));
            }

            var grid = new VisualElement();
            grid.AddToClassList("hub-grid");
            foreach (HubAction action in actions) grid.Add(ActionCard(action));
            container.Add(grid);
        }

        private static VisualElement ActionCard(HubAction action)
        {
            var card = new VisualElement();
            card.AddToClassList("hub-card");

            var top = new VisualElement();
            top.AddToClassList("hub-card-top");
            var title = new Label(action.Title);
            title.AddToClassList("hub-card-title");
            top.Add(title);

            Label pill = null;
            if (action.IsOn != null)
            {
                pill = new Label();
                pill.AddToClassList("hub-pill");
                top.Add(pill);
            }
            card.Add(top);

            var desc = new Label(action.Description);
            desc.AddToClassList("hub-card-desc");
            card.Add(desc);

            if (!string.IsNullOrEmpty(action.Where))
            {
                var where = new Label(action.Where);
                where.AddToClassList("hub-card-where");
                card.Add(where);
            }

            System.Action sync = () =>
            {
                if (pill == null) return;
                bool on = action.IsOn();
                pill.text = on ? "ON" : "OFF";
                pill.EnableInClassList("hub-pill--on", on);
                card.EnableInClassList("hub-card--on", on);
            };
            sync();

            card.AddManipulator(new Clickable(() =>
            {
                action.Run();
                sync();
            }));
            return card;
        }

        private void OnLinkClicked(PointerUpLinkTagEvent evt)
        {
            string target = evt.linkID;
            if (string.IsNullOrEmpty(target)) return;

            if (target.StartsWith(HubLinkPrefix))
            {
                string file = target.Substring(HubLinkPrefix.Length);
                HubPage page = _pages.FirstOrDefault(p => string.Equals(p.FileName, file, System.StringComparison.OrdinalIgnoreCase));
                if (page != null) EditorApplication.delayCall += () => { ClearSearch(); ShowPage(page); };
                return;
            }

            if (IsWebLink(target)) Application.OpenURL(target);
        }

        private static bool IsWebLink(string url)
        {
            return !string.IsNullOrEmpty(url)
                && (url.StartsWith("https://") || url.StartsWith("http://") || url.StartsWith("mailto:"));
        }

        private void HookShortcuts(VisualElement root)
        {
            root.focusable = true;
            root.RegisterCallback<PointerDownEvent>(_ =>
            {
                if (root.focusController != null && root.focusController.focusedElement == null) root.Focus();
            }, TrickleDown.TrickleDown);

            root.RegisterCallback<ValidateCommandEvent>(e =>
            {
                if (e.commandName == FindCommand) e.StopPropagation();
            });

            root.RegisterCallback<ExecuteCommandEvent>(e =>
            {
                if (e.commandName != FindCommand) return;
                FocusSearch();
                e.StopPropagation();
            });

            root.RegisterCallback<KeyDownEvent>(e =>
            {
                if (!e.actionKey || e.keyCode != KeyCode.F) return;
                FocusSearch();
                e.StopPropagation();
            }, TrickleDown.TrickleDown);
        }

        private void FocusSearch()
        {
            if (_search == null || _minimized) return;
            if (_collapsed) ToggleCollapsed();
            OpenSidebarForSearch();
            _search.Focus();
        }
    }
}
#endif
