#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ImageMagick;

namespace VixenTools.Editor
{
    [InitializeOnLoad]
    public static class VixenUpdateNotifier
    {
        private const string BADGE_NAME = "vixen-update-badge";
        private const string PREF_STORED_VER = "VixenTools_StoredVersion";
        private const string PREF_UPDATE_PENDING = "VixenTools_UpdatePending";
        private const string PKG_PATH = "Packages/com.vixencreations.vixens-toolbox/package.json";

        static VixenUpdateNotifier()
        {
            CheckForPackageChanges();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void CheckForPackageChanges()
        {
            try
            {
                string path = Path.GetFullPath(PKG_PATH);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Match vMatch = Regex.Match(json, @"""version""\s*:\s*""([^""]+)""");
                    if (vMatch.Success)
                    {
                        string currentVersion = vMatch.Groups[1].Value;
                        string storedVersion = EditorPrefs.GetString(PREF_STORED_VER, "");

                        if (string.IsNullOrEmpty(storedVersion) || storedVersion != currentVersion)
                        {
                            EditorPrefs.SetBool(PREF_UPDATE_PENDING, true);
                            EditorPrefs.SetString(PREF_STORED_VER, currentVersion);
                        }
                    }
                }
            }
            catch { }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var root = sceneView.rootVisualElement;
            if (root == null) return;

            bool updatePending = EditorPrefs.GetBool(PREF_UPDATE_PENDING, false);
            var existingBadge = root.Q<Button>(BADGE_NAME);

            if (!updatePending)
            {
                if (existingBadge != null) existingBadge.style.display = DisplayStyle.None;
                return;
            }

            if (existingBadge == null)
            {
                existingBadge = BuildCyberBadge();
                root.Add(existingBadge);
            }

            existingBadge.style.display = DisplayStyle.Flex;
        }

        private static Button BuildCyberBadge()
        {
            var badge = new Button(() =>
            {
                EditorPrefs.SetBool(PREF_UPDATE_PENDING, false);
                VixenHub.ShowChangelogWindow();
            })
            {
                name = BADGE_NAME
            };

            badge.style.position = Position.Absolute;
            badge.style.bottom = 20;
            badge.style.right = 20;
            badge.style.width = 240;
            badge.style.height = 36;

            badge.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);

            badge.style.borderTopWidth = 0;
            badge.style.borderRightWidth = 0;
            badge.style.borderBottomWidth = 1;
            badge.style.borderLeftWidth = 4;

            badge.style.borderLeftColor = new Color(1f, 0f, 0.66f);
            badge.style.borderBottomColor = new Color(0f, 0.9f, 1f, 0.3f);

            badge.style.marginLeft = 0;
            badge.style.marginRight = 0;
            badge.style.marginTop = 0;
            badge.style.marginBottom = 0;
            badge.style.paddingLeft = 0;
            badge.style.paddingRight = 0;
            badge.style.paddingTop = 0;
            badge.style.paddingBottom = 0;

            badge.style.alignItems = Align.Center;
            badge.style.justifyContent = Justify.Center;

            badge.style.transitionDuration = new List<TimeValue> { new TimeValue(0.15f) };
            badge.RegisterCallback<PointerEnterEvent>(e => badge.style.backgroundColor = new Color(0.12f, 0.12f, 0.18f, 0.95f));
            badge.RegisterCallback<PointerLeaveEvent>(e => badge.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f));

            var label = new Label(">> <color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> UPDATE") { enableRichText = true };
            label.style.fontSize = 14;

            Font cyberFont = AssetDatabase.LoadAssetAtPath<Font>("Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf");
            if (cyberFont != null) label.style.unityFontDefinition = new StyleFontDefinition(cyberFont);

            badge.Add(label);

            return badge;
        }
    }

    [InitializeOnLoad]
    public static class VixenMagickKit
    {
        static VixenMagickKit()
        {
            try
            {
                ResourceLimits.Thread = (ulong)System.Math.Max(1, System.Environment.ProcessorCount);
            }
            catch { }
        }

        private static readonly string[] ProtectedPathFragments =
        {
            "/_PoiyomiShaders/",
            "/_PoiyomiToonShaders/",
            "/Poiyomi/",
            "/lilToon/",
            "/Sunao Shader/",
            "/Editor Default Resources/",
        };

        private static readonly string[] ProtectedExtensions =
        {
            ".exr", ".hdr", ".cubemap", ".rendertexture",
        };

        public static bool IsProtectedAsset(string path)
        {
            if (string.IsNullOrEmpty(path)) return true;

            string normalized = path.Replace('\\', '/');

            foreach (var ext in ProtectedExtensions)
                if (normalized.EndsWith(ext, System.StringComparison.OrdinalIgnoreCase))
                    return true;

            foreach (var fragment in ProtectedPathFragments)
                if (normalized.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

            return false;
        }

        private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;

        public static bool TryLosslessOptimize(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            if (IsProtectedAsset(path)) return false;
            try
            {
                long fileBytes = new FileInfo(path).Length;
                bool useOptimal = fileBytes <= OptimalCompressionMaxBytes;

                byte[] original = File.ReadAllBytes(path);
                using var ms = new MemoryStream(original.Length);
                ms.Write(original, 0, original.Length);
                ms.Position = 0;

                var optimizer = new ImageOptimizer
                {
                    OptimalCompression = useOptimal,
                    IgnoreUnsupportedFormats = true
                };

                if (optimizer.LosslessCompress(ms))
                {
                    byte[] optimized = ms.ToArray();
                    if (optimized.Length > 0 && optimized.Length < original.Length)
                    {
                        File.WriteAllBytes(path, optimized);
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[VixForge] LosslessCompress skipped for '{path}': {ex.Message}");
            }
            return false;
        }

        public static bool TryGetDimensions(byte[] bytes, out uint width, out uint height)
        {
            width = 0;
            height = 0;
            if (bytes == null || bytes.Length == 0) return false;
            try
            {
                var info = new MagickImageInfo(bytes);
                width = info.Width;
                height = info.Height;
                return width > 0 && height > 0;
            }
            catch { return false; }
        }

        public static MagickReadSettings DownscaleReadSettings(uint targetMaxDim)
        {
            var settings = new MagickReadSettings();
            if (targetMaxDim > 0)
            {
                uint hint = targetMaxDim <= (uint.MaxValue / 2u) ? targetMaxDim * 2u : targetMaxDim;
                settings.SetDefine(MagickFormat.Jpeg, "size", $"{hint}x{hint}");
            }
            return settings;
        }

        public static bool IsLinearOrNormalData(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;
            if (importer.textureType == TextureImporterType.NormalMap) return true;
            return !importer.sRGBTexture;
        }

        public static void HighQualityResize(MagickImage img, uint targetW, uint targetH, bool linearData, FilterType filter, bool onlyShrink, double sharpenSigma)
        {
            if (img == null) return;
            img.FilterType = filter;

            bool gammaCorrect = !linearData && img.ColorSpace == ImageMagick.ColorSpace.sRGB;
            if (gammaCorrect) img.ColorSpace = ImageMagick.ColorSpace.RGB;

            img.Resize(new MagickGeometry(targetW, targetH) { IgnoreAspectRatio = false, Greater = onlyShrink });

            if (gammaCorrect) img.ColorSpace = ImageMagick.ColorSpace.sRGB;

            if (sharpenSigma > 0.0) img.AdaptiveSharpen(0.0, sharpenSigma);
        }

        public static void ApplyOptimalEncoding(MagickImage img, int jpegQuality = 90)
        {
            if (img == null) return;
            img.Strip();

            var fmt = img.Format;
            if (fmt == MagickFormat.Png)
            {
                img.Settings.SetDefine(MagickFormat.Png, "compression-level", 9);
            }
            else if (fmt == MagickFormat.Jpeg || fmt == MagickFormat.Jpg)
            {
                img.Quality = (uint)System.Math.Max(1, System.Math.Min(100, jpegQuality));
            }
        }

        public static bool ProcessTextureFile(string path, uint targetSize, bool linearData, bool downscale)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            bool resized = false;
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (TryGetDimensions(bytes, out uint w, out uint h))
                {
                    bool needsWork = downscale ? (w > targetSize || h > targetSize) : (w < targetSize && h < targetSize);
                    if (needsWork)
                    {
                        var readSettings = downscale ? DownscaleReadSettings(targetSize) : new MagickReadSettings();
                        using (var img = new MagickImage(bytes, readSettings))
                        {
                            if (downscale)
                                HighQualityResize(img, targetSize, targetSize, linearData, FilterType.Lanczos, true, 0.5);
                            else
                                HighQualityResize(img, targetSize, targetSize, linearData, FilterType.Mitchell, false, 0.6);

                            ApplyOptimalEncoding(img);
                            img.Write(path);
                            resized = true;
                        }
                    }
                }
                TryLosslessOptimize(path);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VixForge] Magick failed for '{path}': {e.Message}");
            }
            return resized;
        }
    }

    public class VixenHub : EditorWindow
    {
        private enum TabMode { News, Dashboard, CoreModules, SupportedModules, ShaderDocs, MetricsDocs, Network, Support, Changelog }
        private TabMode _currentMode = TabMode.News;

        private const string PackageRoot = "Packages/com.vixencreations.vixens-toolbox/";
        private const string FontPath = PackageRoot + "Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string UssPath = PackageRoot + "Editor/UiStyles/VixenHubStyles.uss";

        private Font _cyberFont;

        private string _packageVersion = "Unknown";
        private string _sdkVersion = "Unknown";

        private class ChangelogEntry
        {
            public string VersionTitle;
            public string Content;
        }
        private List<ChangelogEntry> _changelogEntries = new List<ChangelogEntry>();
        private int _selectedChangelogIndex = 0;

        private Button _btnNews;
        private Button _btnDashboard;
        private Button _btnCoreModules;
        private Button _btnSupportedModules;
        private Button _btnShaderDocs;
#if UDON
        private Button _btnMetricsDocs;
#endif
        private Button _btnNetwork;
        private Button _btnSupport;
        private Button _btnChangelog;

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

            window.OpenChangelogTab();
        }

        public void OpenChangelogTab()
        {
            if (_contentContainer != null)
            {
                SwitchMode(TabMode.Changelog);
            }
        }

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            LoadVersionData();
        }

        private void LoadVersionData()
        {
            string path = Path.GetFullPath(PackageRoot + "package.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

                Match vMatch = Regex.Match(json, @"""version""\s*:\s*""([^""]+)""");
                if (vMatch.Success) _packageVersion = vMatch.Groups[1].Value;

                Match sdkMatch = Regex.Match(json, @"""com\.vrchat\.(?:base|avatars|worlds)""\s*:\s*""([^""]+)""");
                if (sdkMatch.Success) _sdkVersion = sdkMatch.Groups[1].Value.Replace("^", "").Replace("~", "");
            }
        }

        private void ParseChangelogData()
        {
            _changelogEntries.Clear();
            string rawText = LoadMarkdownFile("CHANGELOG.md");
            string[] lines = rawText.Split('\n');

            ChangelogEntry currentEntry = null;

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();

                if (line.StartsWith("## ["))
                {
                    currentEntry = new ChangelogEntry {
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

            var tabContainer = new VisualElement { name = "tab-container" };
            tabContainer.style.flexWrap = Wrap.Wrap;
            tabContainer.style.flexDirection = FlexDirection.Row;

            _btnNews = new Button(() => SwitchMode(TabMode.News)) { text = "News" };
            _btnDashboard = new Button(() => SwitchMode(TabMode.Dashboard)) { text = "Architecture" };
            _btnCoreModules = new Button(() => SwitchMode(TabMode.CoreModules)) { text = "Core Modules" };
            _btnSupportedModules = new Button(() => SwitchMode(TabMode.SupportedModules)) { text = "Supported Modules" };
            _btnShaderDocs = new Button(() => SwitchMode(TabMode.ShaderDocs)) { text = "Shader Pipeline" };
#if UDON
            _btnMetricsDocs = new Button(() => SwitchMode(TabMode.MetricsDocs)) { text = "Metrics Engine" };
#endif
            _btnNetwork = new Button(() => SwitchMode(TabMode.Network)) { text = "Network" };
            _btnSupport = new Button(() => SwitchMode(TabMode.Support)) { text = "Support" };
            _btnChangelog = new Button(() => SwitchMode(TabMode.Changelog)) { text = "Changelogs" };

            _btnNews.AddToClassList("tab-btn");
            _btnDashboard.AddToClassList("tab-btn");
            _btnCoreModules.AddToClassList("tab-btn");
            _btnSupportedModules.AddToClassList("tab-btn");
            _btnShaderDocs.AddToClassList("tab-btn");
#if UDON
            _btnMetricsDocs.AddToClassList("tab-btn");
#endif
            _btnNetwork.AddToClassList("tab-btn");
            _btnSupport.AddToClassList("tab-btn");
            _btnChangelog.AddToClassList("tab-btn");

            tabContainer.Add(_btnNews);
            tabContainer.Add(_btnDashboard);
            tabContainer.Add(_btnCoreModules);
            tabContainer.Add(_btnSupportedModules);
            tabContainer.Add(_btnShaderDocs);
#if UDON
            tabContainer.Add(_btnMetricsDocs);
#endif
            tabContainer.Add(_btnNetwork);
            tabContainer.Add(_btnSupport);
            tabContainer.Add(_btnChangelog);
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

            SwitchMode(TabMode.News);
        }

        private void SwitchMode(TabMode mode)
        {
            _currentMode = mode;

            _btnNews.RemoveFromClassList("tab-btn-active"); _btnNews.AddToClassList("tab-btn-inactive");
            _btnDashboard.RemoveFromClassList("tab-btn-active"); _btnDashboard.AddToClassList("tab-btn-inactive");
            _btnCoreModules.RemoveFromClassList("tab-btn-active"); _btnCoreModules.AddToClassList("tab-btn-inactive");
            _btnSupportedModules.RemoveFromClassList("tab-btn-active"); _btnSupportedModules.AddToClassList("tab-btn-inactive");
            _btnShaderDocs.RemoveFromClassList("tab-btn-active"); _btnShaderDocs.AddToClassList("tab-btn-inactive");
#if UDON
            _btnMetricsDocs.RemoveFromClassList("tab-btn-active"); _btnMetricsDocs.AddToClassList("tab-btn-inactive");
#endif
            _btnNetwork.RemoveFromClassList("tab-btn-active"); _btnNetwork.AddToClassList("tab-btn-inactive");
            _btnSupport.RemoveFromClassList("tab-btn-active"); _btnSupport.AddToClassList("tab-btn-inactive");
            _btnChangelog.RemoveFromClassList("tab-btn-active"); _btnChangelog.AddToClassList("tab-btn-inactive");

            _contentContainer.Clear();

            switch (mode)
            {
                case TabMode.News:
                    _btnNews.RemoveFromClassList("tab-btn-inactive"); _btnNews.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Latest ecosystem news, partnerships, and announcements from <b>VixForge Interactive</b>.";
                    RenderNews();
                    break;
                case TabMode.Dashboard:
                    _btnDashboard.RemoveFromClassList("tab-btn-inactive"); _btnDashboard.AddToClassList("tab-btn-active");
                    _tabDescription.text = "A comprehensive suite of custom Unity Editor utilities and automation scripts focused specifically on <b>High-Fidelity Avatar Pipeline & Topology Architecture</b>.";
                    ParseMarkdownAndInject(LoadMarkdownFile("README.md"), _contentContainer);
                    break;
                case TabMode.CoreModules:
                    _btnCoreModules.RemoveFromClassList("tab-btn-inactive"); _btnCoreModules.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Direct access to the flagship utilities that power the VixForge ecosystem.";
                    RenderCoreModules();
                    break;
                case TabMode.SupportedModules:
                    _btnSupportedModules.RemoveFromClassList("tab-btn-inactive"); _btnSupportedModules.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Explore third-party integrations and assets natively supported and audited by the VixForge ecosystem.";
                    RenderSupportedModules();
                    break;
                case TabMode.ShaderDocs:
                    _btnShaderDocs.RemoveFromClassList("tab-btn-inactive"); _btnShaderDocs.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Comprehensive architecture breakdown of the VixenWear Latex Ultra shader pipeline, texture packing, and ecosystem integrations.";
                    RenderShaderDocs();
                    break;
#if UDON
                case TabMode.MetricsDocs:
                    _btnMetricsDocs.RemoveFromClassList("tab-btn-inactive"); _btnMetricsDocs.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Transparent breakdown of the 4D-Chess heuristic multipliers powering the World Profiler.";
                    RenderMetricsDocs();
                    break;
#endif
                case TabMode.Network:
                    _btnNetwork.RemoveFromClassList("tab-btn-inactive"); _btnNetwork.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Connect with the ecosystem, access open-source repositories, and monitor pipeline updates.";
                    RenderNetwork();
                    break;
                case TabMode.Support:
                    _btnSupport.RemoveFromClassList("tab-btn-inactive"); _btnSupport.AddToClassList("tab-btn-active");
                    _tabDescription.text = "This architecture requires immense R&D. If these tools save your sanity, consider fueling the engine.";
                    RenderSupport();
                    break;
                case TabMode.Changelog:
                    _btnChangelog.RemoveFromClassList("tab-btn-inactive"); _btnChangelog.AddToClassList("tab-btn-active");
                    _tabDescription.text = "Review version history, architecture upgrades, and pipeline fixes.";

                    if (_changelogEntries.Count == 0) ParseChangelogData();

                    if (_changelogEntries.Count > 0)
                    {
                        var controlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 15, marginTop = 10, paddingLeft = 5, paddingRight = 5 } };

                        var dropLabel = new Label("Target Release:") { style = { color = new Color(0.67f, 0.67f, 0.67f), marginRight = 10, fontSize = 14 } };
                        if (_cyberFont != null) dropLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
                        controlRow.Add(dropLabel);

                        List<string> versionNames = new List<string>();
                        foreach (var log in _changelogEntries) versionNames.Add(log.VersionTitle);

                        if (_selectedChangelogIndex >= versionNames.Count) _selectedChangelogIndex = 0;

                        var dropdown = new DropdownField(versionNames, _selectedChangelogIndex);
                        dropdown.style.flexGrow = 1;

                        dropdown.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
                        dropdown.style.borderTopColor = new Color(0f, 0.898f, 1f, 0.4f);
                        dropdown.style.borderBottomColor = new Color(0f, 0.898f, 1f, 0.4f);
                        dropdown.style.borderLeftColor = new Color(0f, 0.898f, 1f, 0.4f);
                        dropdown.style.borderRightColor = new Color(0f, 0.898f, 1f, 0.4f);
                        dropdown.style.color = new Color(0.9f, 0.9f, 0.9f);

                        var logContentContainer = new VisualElement();

                        dropdown.RegisterValueChangedCallback(e => {
                            _selectedChangelogIndex = versionNames.IndexOf(e.newValue);
                            logContentContainer.Clear();
                            ParseMarkdownAndInject(_changelogEntries[_selectedChangelogIndex].Content, logContentContainer);
                        });

                        controlRow.Add(dropdown);
                        _contentContainer.Add(controlRow);

                        var sep = new VisualElement();
                        sep.AddToClassList("md-separator");
                        sep.style.backgroundColor = new Color(0f, 0.9f, 1f, 0.3f);
                        _contentContainer.Add(sep);

                        _contentContainer.Add(logContentContainer);

                        ParseMarkdownAndInject(_changelogEntries[_selectedChangelogIndex].Content, logContentContainer);
                    }
                    else
                    {
                        ParseMarkdownAndInject("### Error\nCould not parse changelog versions from `CHANGELOG.md`.", _contentContainer);
                    }
                    break;
            }
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

#if UDON
        private void RenderMetricsDocs()
        {
            string markdown = LoadMarkdownFile("HOWITWORKS.md");
            ParseMarkdownAndInject(markdown, _contentContainer);
        }
#endif

        private void RenderShaderDocs()
        {
            string markdown = LoadMarkdownFile("SHADERSETUP.md");
            ParseMarkdownAndInject(markdown, _contentContainer);
        }

        private void RenderCoreModules()
        {
            var universalList = new List<(System.Action action, string title, string desc)>
            {
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Unity Engine/Animation Workbench Pro"), "Animation Workbench Pro", "Visual workspace for staging, easing, and sampling complex animation curves."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Unity Engine/Pipeline Preset Manager"), "Pipeline Preset Manager", "Bulk extraction and programmatic authoring of standardized importer settings.")
            };

            RenderActionGrid("Universal Utilities", "#00e5ff", universalList);
#if !UDON
            var avatarList = new List<(System.Action action, string title, string desc)>
            {
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Badge Studio"), "Badge Studio", "High-fidelity procedural generation engine for VRChat convention badges."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Quest Conversion Engine"), "Quest Conversion Engine", "Non-destructive, high-fidelity pipeline mapping 100% of Android performance limits."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/PhysBone Topology Mapper"), "PhysBone Topology Mapper", "Automates PhysBone extraction and injection bypassing native prefab constraints."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Accessory Engine"), "Accessory Mounting Engine", "Generates sterile armature clones and surgically mounts accessories via destructive auto-rigging or kinematic parent constraints."),
                (() => EditorApplication.ExecuteMenuItem("VixenTools/Avatars/Optimization Suite"), "Optimization Suite", "Advanced dual-pipeline validation and automated optimization engine with ImageMagick VRAM resolution.")
            };

            RenderActionGrid("Avatar Pipeline Tools", "#ff00aa", avatarList);
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
                    SwitchMode(TabMode.CoreModules);
                },
                snapTitle,
                "Enterprise-grade gravity snapper. Automatically drops selected objects to the nearest floor or shelf when moved."),

                (() =>
                {
                    EditorApplication.ExecuteMenuItem("VixenTools/Scene/Precision Click-to-Place");
                    SwitchMode(TabMode.CoreModules);
                },
                precisionTitle,
                "Sniper-rifle camera raycaster. Click anywhere in the Scene View to instantly teleport objects to complex shelf polygons."),

                (() =>
                {
                    EditorApplication.ExecuteMenuItem("VixenTools/Scene/Vixen World Engine");
                    SwitchMode(TabMode.CoreModules);
                },
                "<color=#00e5ff>Vixen World Engine</color>",
                "Omni-System diagnostic spider. Full ecosystem audits for ProTV, TXL, IwaSync3, video pipelines, Udon persistence, and shader replacement systems.")
            };

            RenderActionGrid("WORLD PIPELINE TOOLS", "#ff00aa", worldList);
#endif
        }

        private void RenderSupportedModules()
        {
            string markdown = @"
The **VixForge Architecture** is engineered to interoperate flawlessly with industry-standard third-party modules. The World Engine and Avatar Validators actively audit, scan, and protect these ecosystems natively.
";
            ParseMarkdownAndInject(markdown, _contentContainer);

            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://protv.dev/"),
                    "ProTV (Techanon)",
                    "Comprehensive pipeline auditing, GSV conflict resolution, and AudioLink topology handshakes."),

                (() => Application.OpenURL("https://github.com/llealloo/audiolink"),
                    "AudioLink",
                    "Reflective extraction of internal FFT textures, orphan detection, and global whitelist protection."),

                (() => Application.OpenURL("https://ltcgi.dev/"),
                    "LTCGI",
                    "Real-time polygonal area lighting matrices, ghost screen eradication, and bake cache deadlock resolution."),

                (() => Application.OpenURL("https://xtlcdn.github.io/VizVid/"),
                    "VizVid (VVMW)",
                    "Comprehensive video pipeline analysis, interface decoupling checks, and Quest fallback validation."),

                (() => Application.OpenURL("https://github.com/vrctxl/VideoTXL"),
                    "Video TXL",
                    "CRT render ecosystem validation, GC sink detection, and Playlist Queue access control integration."),

                (() => Application.OpenURL("https://booth.pm/en/items/2666275"),
                    "iwaSync3",
                    "Network sync frequency tuning, blinding emissive bounds detection, and global 2D audio isolation."),

                (() => Application.OpenURL("https://rinvo.booth.pm/items/5757644"),
                    "YouTube Search (Rinvo)",
                    "Autonomous video player target linking, UI architectural decoupling, and API pool size validation."),

                (() => Application.OpenURL("https://github.com/REDSIM/VRCLightVolumes"),
                    "VRC Light Volumes",
                    "Compute load detection, sphere threshold optimization, and TVGI/AudioLink strobe safety enforcement."),

                (() => Application.OpenURL("https://github.com/AcChosen/VR-Stage-Lighting"),
                    "VR Stage Lighting",
                    "Regex-based heuristic protection and DMX audit support.")
            };

            RenderActionGrid("Ecosystem Integrations", "#00e5ff", list);
        }

        private void RenderNews()
        {
            ParseMarkdownAndInject(LoadMarkdownFile("NEWS.md"), _contentContainer);

            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://vixencreations.github.io/VixenToolBox/news.html"), "Read The Full Announcement", "The complete Enigma Industries x VixForge Interactive partnership write-up."),
                (() => Application.OpenURL("https://discord.gg/clubenigmavr"), "Join Enigma's Discord", "Club Enigma / Enigma Industries community hub."),
                (() => Application.OpenURL("https://vrc.group/ENIGMA.8607"), "Join Enigma's VRC Group", "Follow Enigma Industries in VRChat.")
            };

            RenderActionGrid("Partnership Links", "#ff00aa", list);
        }

        private void RenderNetwork()
        {
            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://github.com/VixenCreations/VixenToolBox"), "GitHub Repository", "Core ecosystem source code and release tracking."),
                (() => Application.OpenURL("https://x.com/VixenVRC"), "Twitter", "Where I post All Kinds of things and Interact with the community."),
                (() => Application.OpenURL("https://discord.com/invite/3vbJCKcPtJ"), "Discord", "My Official Community to get help with things."),
                (() => Application.OpenURL("https://github.com/VixenCreations/VixenToolBox/issues"), "Report An Issue", "Report an Issue or Request a new feature."),
                (() => Application.OpenURL("https://www.youtube.com/@vixenlicous"), "YouTube Channel", "Technical breakdowns, pipeline tutorials, and development logs.")
            };

            RenderActionGrid("Ecosystem Routing", "#00e5ff", list);
        }

        private void RenderSupport()
        {
            string markdown = @"
If my code has ever saved your scene from completely bricking, optimized your Quest fallback in under 10 seconds, or just made your workflow suck a little bit less... consider throwing a coffee my way:
";
            ParseMarkdownAndInject(markdown, _contentContainer);

            var list = new List<(System.Action action, string title, string desc)>
            {
                (() => Application.OpenURL("https://ko-fi.com/vixenlicous"), "Ko-Fi Donation", "One-Time Support"),
                (() => Application.OpenURL("https://vixenlicous.gumroad.com/coffee"), "Gumroad Donation", "One-Time Donation"),
                (() => Application.OpenURL("https://cash.app/$VixenVRC"), "CashApp Donation", "Direct Transfer"),
                (() => Application.OpenURL("https://www.patreon.com/cw/Vixenlicious"), "Patreon Donation", "Monthly Pipeline Fuel"),
                (() => Application.OpenURL("https://jinxxy.com/Vixenlicious"), "Jinxxy", "Full Asset Storefront"),
                (() => Application.OpenURL("https://vixenlicous.gumroad.com/"), "Gumroad", "Full Asset Storefront")
            };

            RenderActionGrid("Support The Architecture", "#ff00aa", list);
        }

        private void RenderActionGrid(string headerText, string accentHex, List<(System.Action action, string title, string desc)> items)
        {
            var header = new Label($"<color={accentHex}>{headerText}</color>") { enableRichText = true };
            header.AddToClassList("md-h1");
            if (_cyberFont != null) header.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            _contentContainer.Add(header);

            var sep = new VisualElement();
            sep.AddToClassList("md-separator");
            ColorUtility.TryParseHtmlString(accentHex, out Color c);
            c.a = 0.3f;
            sep.style.backgroundColor = c;
            _contentContainer.Add(sep);

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

            _contentContainer.Add(grid);
        }

        private void ParseMarkdownAndInject(string text, VisualElement container)
        {
            string[] lines = text.Split('\n');

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.Contains("[ Network Links ]") || line.Contains("Network Links")) break;

                if (line.StartsWith("---") || line.StartsWith("***"))
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