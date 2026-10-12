#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VixenTools.Editor
{
    public partial class VixenHub
    {
        private const string SidebarPref = "VixenTools.Hub.Sidebar";
        private const float DrawerWidth = 600f;
        private const int MinQueryLength = 2;
        private const int MaxHitsPerPage = 6;
        private const int MaxHits = 60;
        private const int MaxOutline = 14;

        private const string SidebarTip = "Shows or hides the list of pages. Ctrl+F jumps to the search box.";
        private const string SearchHint = "Search pages and tools";
        private const string ShaderHubType = "VixenWearHub";
        private const string ShaderHubMenu = "VixenTools/Shader Hub";

        private class SearchLine
        {
            public HubPage Page;
            public string Anchor;
            public int Release = -1;
            public string Context;
            public string Text;
        }

        private VisualElement _sidebar;
        private VisualElement _nav;
        private VisualElement _outline;
        private ToolbarSearchField _search;
        private Label _searchHint;
        private readonly Dictionary<string, Label> _shelfLabels = new Dictionary<string, Label>();

        private bool _sidebarOn = true;
        private bool _drawerMode;
        private bool _drawerOpen;
        private bool _searching;
        private bool _muteSearch;
        private string _query = "";
        private IVisualElementScheduledItem _searchJob;
        private List<SearchLine> _searchIndex;

        private static readonly Regex HeadingLine = new Regex(@"^(#{1,6})\s+(.*)$");
        private static readonly Regex ListMarker = new Regex(@"^(?:[-*+]|\d+[.)]|>)\s*");
        private static readonly Regex RuleLine = new Regex(@"^(?:\*{3,}|-{3,}|_{3,})$");
        private static readonly Regex TableRule = new Regex(@"^\|?[\s:\-|]+\|?$");

        private VisualElement BuildSidebar()
        {
            _sidebarOn = EditorPrefs.GetBool(SidebarPref, true);
            _shelfLabels.Clear();

            _sidebar = new VisualElement { name = "hub-sidebar" };

            _search = new ToolbarSearchField { name = "hub-search" };
            _searchHint = new Label(SearchHint) { pickingMode = PickingMode.Ignore };
            _searchHint.AddToClassList("hub-search-hint");
            _search.Add(_searchHint);
            _search.RegisterValueChangedCallback(e => OnSearchChanged(e.newValue));
            _sidebar.Add(_search);

            var scroll = new ScrollView(ScrollViewMode.Vertical) { name = "hub-nav-scroll" };
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _nav = scroll.contentContainer;
            _sidebar.Add(scroll);

            _outline = new VisualElement();
            _outline.AddToClassList("hub-nav-outline");

            string shelf = "";
            foreach (HubPage page in _pages)
            {
                if (page.Category != null && page.Category != shelf)
                {
                    shelf = page.Category;
                    var label = new Label(shelf.ToUpperInvariant());
                    label.AddToClassList("hub-shelf");
                    _shelfLabels[shelf] = label;
                    _nav.Add(label);
                }
                _nav.Add(NavItem(page));
            }

            return _sidebar;
        }

        private VisualElement NavItem(HubPage page)
        {
            var item = new VisualElement { tooltip = page.Description };
            item.AddToClassList("hub-nav-item");

            var label = new Label(page.Title);
            label.AddToClassList("hub-nav-label");
            item.Add(label);

            item.AddManipulator(new Clickable(() =>
            {
                ClearSearch();
                ShowPage(page);
            }));

            page.NavItem = item;
            return item;
        }

        private void RefreshNav()
        {
            if (_nav == null) return;

            foreach (HubPage page in _pages)
                page.NavItem?.EnableInClassList("hub-nav-item--active", page == _activePage && !_searching);

            _outline.RemoveFromHierarchy();
            _outline.Clear();
            if (_searching || _activePage?.NavItem == null) return;

            List<(int level, string text, VisualElement element)> sections = OutlineSections();
            if (sections.Count < 2) return;

            foreach (var section in sections.Take(MaxOutline))
            {
                string anchor = section.text;
                var sub = new Label(anchor) { tooltip = anchor };
                sub.AddToClassList("hub-nav-sub");
                sub.AddManipulator(new Clickable(() =>
                {
                    ScrollToHeading(anchor);
                    CloseDrawer();
                }));
                _outline.Add(sub);
            }

            VisualElement item = _activePage.NavItem;
            item.parent.Insert(item.parent.IndexOf(item) + 1, _outline);
        }

        private List<(int level, string text, VisualElement element)> OutlineSections()
        {
            var result = new List<(int, string, VisualElement)>();
            if (_activePage == null || _activePage.Id == TabHome) return result;
            if (_activePage.FileName != null && _activePage.FileName.EndsWith(ChangelogSuffix, System.StringComparison.OrdinalIgnoreCase)) return result;

            int top = _headings.Count > 0 ? _headings.Min(h => h.level) : 0;
            int depth = _headings.Count(h => h.level == top) >= 3 ? top : top + 1;
            foreach (var h in _headings)
            {
                if (h.level <= depth && !string.IsNullOrEmpty(h.text)) result.Add(h);
            }
            return result;
        }

        private void ToggleSidebar()
        {
            if (_drawerMode)
            {
                _drawerOpen = !_drawerOpen;
            }
            else
            {
                _sidebarOn = !_sidebarOn;
                EditorPrefs.SetBool(SidebarPref, _sidebarOn);
            }
            ApplySidebar();
        }

        private void OpenSidebarForSearch()
        {
            if (_drawerMode) _drawerOpen = true;
            else if (!_sidebarOn)
            {
                _sidebarOn = true;
                EditorPrefs.SetBool(SidebarPref, true);
            }
            ApplySidebar();
        }

        private void CloseDrawer()
        {
            if (!_drawerMode || !_drawerOpen) return;
            _drawerOpen = false;
            ApplySidebar();
        }

        private void SetDrawerMode(bool on)
        {
            if (_drawerMode == on) return;
            _drawerMode = on;
            _drawerOpen = false;
            ApplySidebar();
        }

        private void ApplySidebar()
        {
            VisualElement root = rootVisualElement;
            root.EnableInClassList("hub-drawer", _drawerMode);
            root.EnableInClassList("hub-no-sidebar", _drawerMode ? !_drawerOpen : !_sidebarOn);
        }

        private void OnSearchChanged(string value)
        {
            _searchHint.EnableInClassList("hub-hidden", !string.IsNullOrEmpty(value));
            if (_muteSearch) return;

            _query = (value ?? "").Trim();
            _searchJob?.Pause();
            _searchJob = rootVisualElement.schedule.Execute(RunSearch).StartingIn(150);
        }

        private void ClearSearch()
        {
            if (_search == null) return;

            _searchJob?.Pause();
            _query = "";
            _muteSearch = true;
            _search.value = "";
            _muteSearch = false;
            _searchHint.EnableInClassList("hub-hidden", false);
            FilterNav(null);
        }

        private void RunSearch()
        {
            if (_query.Length < MinQueryLength)
            {
                if (!_searching) return;
                FilterNav(null);
                ShowPage(_activePage ?? _pages.FirstOrDefault());
                return;
            }

            _searching = true;
            string[] terms = _query.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            List<HubAction> tools = SearchActions().Where(a => Matches(a.Title + " " + a.Description + " " + a.Where, terms)).ToList();
            List<HubPage> pages = _pages.Where(p => p.Id != TabHome && Matches(p.Title + " " + p.Description, terms)).ToList();
            List<SearchLine> lines = FindLines(terms);

            var hitPages = new HashSet<HubPage>(pages);
            foreach (SearchLine line in lines) hitPages.Add(line.Page);
            if (tools.Count > 0) hitPages.Add(FindPage(TabCoreModules));
            FilterNav(hitPages);

            RenderResults(terms, tools, pages, lines);
        }

        private static bool Matches(string text, string[] terms)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (string term in terms)
                if (text.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) < 0) return false;
            return true;
        }

        private List<HubAction> SearchActions()
        {
            var actions = new List<HubAction>(ToolActions());
            if (WorldProject) actions.AddRange(PackageActions());
            actions.AddRange(LinkActions());
            return actions;
        }

        private List<SearchLine> FindLines(string[] terms)
        {
            var hits = new List<SearchLine>();
            var perPage = new Dictionary<HubPage, int>();

            foreach (SearchLine line in SearchIndex())
            {
                if (!Matches(line.Text, terms)) continue;
                perPage.TryGetValue(line.Page, out int count);
                if (count >= MaxHitsPerPage) continue;
                perPage[line.Page] = count + 1;
                hits.Add(line);
                if (hits.Count >= MaxHits) break;
            }
            return hits;
        }

        private List<SearchLine> SearchIndex()
        {
            if (_searchIndex != null) return _searchIndex;
            _searchIndex = new List<SearchLine>();

            foreach (HubPage page in _pages)
            {
                if (page.FileName != null && page.FileName.EndsWith(ChangelogSuffix, System.StringComparison.OrdinalIgnoreCase))
                {
                    List<ChangelogEntry> entries = ChangelogEntries(page.FileName);
                    for (int r = 0; r < entries.Count; r++)
                    {
                        if (!string.IsNullOrEmpty(entries[r].Summary)) IndexLine(page, null, r, entries[r].Label, entries[r].Summary);
                        IndexText(page, entries[r].Content, r, entries[r].Label);
                    }
                }
                else if (page.Source != null)
                {
                    IndexText(page, page.Source(), -1, null);
                }
            }
            return _searchIndex;
        }

        private void IndexText(HubPage page, string text, int release, string context)
        {
            if (string.IsNullOrEmpty(text)) return;

            string anchor = null;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || RuleLine.IsMatch(line) || TableRule.IsMatch(line)) continue;

                Match heading = HeadingLine.Match(line);
                if (heading.Success)
                {
                    anchor = PlainText(heading.Groups[2].Value);
                    IndexLine(page, anchor, release, context, anchor);
                    continue;
                }

                IndexLine(page, anchor, release, context, PlainText(ListMarker.Replace(line, "")));
            }
        }

        private void IndexLine(HubPage page, string anchor, int release, string context, string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 3) return;
            _searchIndex.Add(new SearchLine { Page = page, Anchor = anchor, Release = release, Context = context ?? anchor, Text = text });
        }

        private void FilterNav(HashSet<HubPage> visible)
        {
            if (_nav == null) return;

            var shelvesInUse = new HashSet<string>();
            foreach (HubPage page in _pages)
            {
                if (page.NavItem == null) continue;
                bool show = visible == null || page.Id == TabHome || visible.Contains(page);
                page.NavItem.EnableInClassList("hub-hidden", !show);
                if (show && page.Category != null) shelvesInUse.Add(page.Category);
            }

            foreach (KeyValuePair<string, Label> shelf in _shelfLabels)
                shelf.Value.EnableInClassList("hub-hidden", !shelvesInUse.Contains(shelf.Key));

            if (visible != null) RefreshNav();
        }

        private void RenderResults(string[] terms, List<HubAction> tools, List<HubPage> pages, List<SearchLine> lines)
        {
            int total = tools.Count + pages.Count + lines.Count;
            string shown = _query.Replace("<", "").Replace(">", "");
            SetPageHead("Search", "Results for \"" + shown + "\"",
                total == 0 ? null : total == 1 ? "1 match." : total + " matches.");

            _headings.Clear();
            _contentContainer.Clear();
            _contentScroll.scrollOffset = Vector2.zero;

            if (total == 0)
            {
                var empty = new Label("Nothing matches that. Try a tool name, a menu path or a word from one of the guides.");
                empty.AddToClassList("hub-empty");
                _contentContainer.Add(empty);
            }

            if (tools.Count > 0) RenderActionGrid(_contentContainer, "Tools and Links", tools);

            if (pages.Count > 0)
            {
                var title = new Label("Pages");
                title.AddToClassList("hub-section-title");
                _contentContainer.Add(title);

                var grid = new VisualElement();
                grid.AddToClassList("hub-grid");
                foreach (HubPage page in pages) grid.Add(PageCard(page));
                _contentContainer.Add(grid);
            }

            if (lines.Count > 0)
            {
                var title = new Label("In The Guides");
                title.AddToClassList("hub-section-title");
                _contentContainer.Add(title);

                foreach (SearchLine line in lines) _contentContainer.Add(ResultRow(line, terms));
            }

            RefreshNav();
            UpdateCompactLocation();
        }

        private VisualElement ResultRow(SearchLine line, string[] terms)
        {
            var row = new VisualElement();
            row.AddToClassList("hub-result");

            string where = line.Page.Title + (string.IsNullOrEmpty(line.Context) ? "" : "  ›  " + line.Context);
            var whereLabel = new Label(where.ToUpperInvariant());
            whereLabel.AddToClassList("hub-result-where");
            row.Add(whereLabel);

            var text = new Label(Snippet(line.Text, terms)) { enableRichText = true };
            text.AddToClassList("hub-result-text");
            row.Add(text);

            row.AddManipulator(new Clickable(() => OpenResult(line)));
            return row;
        }

        private void OpenResult(SearchLine line)
        {
            if (line.Release >= 0 && line.Page.FileName != null) _selectedChangelogIndex[line.Page.FileName] = line.Release;
            ClearSearch();
            _activePage = null;
            ShowPage(line.Page, line.Anchor);
        }

        private static string Snippet(string text, string[] terms)
        {
            int at = -1;
            foreach (string term in terms)
            {
                at = text.IndexOf(term, System.StringComparison.OrdinalIgnoreCase);
                if (at >= 0) break;
            }

            const int before = 60, after = 140;
            int start = Mathf.Max(0, at - before);
            int end = Mathf.Min(text.Length, Mathf.Max(at, 0) + after);
            string cut = text.Substring(start, end - start).Replace("<", "‹").Replace(">", "›");
            if (start > 0) cut = "…" + cut;
            if (end < text.Length) cut += "…";

            string pattern = string.Join("|", terms.Select(Regex.Escape));
            return Regex.Replace(cut, pattern, m => "<color=#00e5ff><b>" + m.Value + "</b></color>", RegexOptions.IgnoreCase);
        }

        private void RenderHome(VisualElement container)
        {
            container.Add(BuildReleaseHero());
            if (ShaderHubInstalled()) container.Add(BuildThanksCard());

            List<HubAction> quick = ToolActions().Where(a => a.IsOn == null).ToList();
            if (quick.Count > 0)
            {
                var title = new Label("Quick Launch");
                title.AddToClassList("hub-section-title");
                container.Add(title);

                var row = new VisualElement();
                row.AddToClassList("hub-quick");
                foreach (HubAction action in quick)
                {
                    HubAction captured = action;
                    var button = new Button(() => captured.Run()) { text = captured.Title, tooltip = captured.Description };
                    button.AddToClassList("hub-quick-btn");
                    row.Add(button);
                }
                container.Add(row);
            }

            foreach (string category in CategoryOrder)
            {
                List<HubPage> shelf = _pages.Where(p => p.Category == category).ToList();
                if (shelf.Count == 0) continue;

                var title = new Label(category);
                title.AddToClassList("hub-section-title");
                container.Add(title);

                var grid = new VisualElement();
                grid.AddToClassList("hub-grid");
                foreach (HubPage page in shelf) grid.Add(PageCard(page));
                container.Add(grid);
            }
        }

        private VisualElement BuildReleaseHero()
        {
            var hero = new VisualElement();
            hero.AddToClassList("hub-hero");

            HubPage changelogPage = FindPage(TabChangelogs);
            List<ChangelogEntry> entries = changelogPage != null ? ChangelogEntries(changelogPage.FileName) : new List<ChangelogEntry>();
            int index = entries.FindIndex(e => e.Version == _packageVersion);
            if (index < 0) index = 0;
            ChangelogEntry entry = entries.Count > 0 ? entries[index] : null;

            var top = new VisualElement();
            top.AddToClassList("hub-hero-top");
            var title = new Label(entry != null ? "Vixens Toolbox " + entry.Version : "Vixens Toolbox " + _packageVersion);
            title.AddToClassList("hub-hero-title");
            top.Add(title);
            if (entry != null && entry.Version == _packageVersion) top.Add(Chip("Installed", "hub-chip--green"));
            if (entry != null && !string.IsNullOrEmpty(entry.Date)) top.Add(Chip(entry.Date, entry.Unreleased ? "hub-chip--pink" : null));
            hero.Add(top);

            var text = new Label(entry != null && !string.IsNullOrEmpty(entry.Summary)
                ? FormatInline(entry.Summary)
                : "Every tool and guide in the toolbox, in one place.") { enableRichText = true };
            text.AddToClassList("hub-hero-text");
            hero.Add(text);

            var actions = new VisualElement();
            actions.AddToClassList("hub-hero-actions");
            if (entry != null)
            {
                int captured = index;
                var read = new Button(() =>
                {
                    _selectedChangelogIndex[changelogPage.FileName] = captured;
                    OpenTab(TabChangelogs);
                }) { text = "What Changed" };
                read.AddToClassList("hub-btn");
                actions.Add(read);
            }
            if (FindPage(TabNews) != null)
            {
                var news = new Button(() => OpenTab(TabNews)) { text = "News" };
                news.AddToClassList("hub-btn");
                news.AddToClassList("hub-btn--quiet");
                actions.Add(news);
            }
            var tools = new Button(() => OpenTab(TabCoreModules)) { text = "All Tools" };
            tools.AddToClassList("hub-btn");
            tools.AddToClassList("hub-btn--quiet");
            actions.Add(tools);
            hero.Add(actions);

            return hero;
        }

        private static bool? s_shaderHub;

        private static bool ShaderHubInstalled()
        {
            if (s_shaderHub == null)
                s_shaderHub = TypeCache.GetTypesDerivedFrom<EditorWindow>().Any(t => t.FullName == ShaderHubType);
            return s_shaderHub.Value;
        }

        private VisualElement BuildThanksCard()
        {
            var card = new VisualElement();
            card.AddToClassList("hub-thanks");

            var text = new VisualElement();
            text.AddToClassList("hub-thanks-text");
            var title = new Label("Thank you for supporting our work");
            title.AddToClassList("hub-thanks-title");
            var body = new Label("You run our shaders as well as the Vixens Toolbox. That support is what keeps the forge lit.");
            body.AddToClassList("hub-thanks-body");
            text.Add(title);
            text.Add(body);
            card.Add(text);

            var open = new Button(() => EditorApplication.ExecuteMenuItem(ShaderHubMenu)) { text = "Open the Shader Hub" };
            open.AddToClassList("hub-btn");
            card.Add(open);
            return card;
        }

        private VisualElement PageCard(HubPage page)
        {
            var card = new VisualElement();
            card.AddToClassList("hub-card");
            if (page.Category == CatStart) card.AddToClassList("hub-card--start");
            else if (page.Category == CatDocs) card.AddToClassList("hub-card--docs");
            else if (page.Category == CatAbout) card.AddToClassList("hub-card--about");

            var title = new Label(page.Title);
            title.AddToClassList("hub-card-title");
            card.Add(title);

            var desc = new Label(page.Description);
            desc.AddToClassList("hub-card-desc");
            card.Add(desc);

            card.AddManipulator(new Clickable(() =>
            {
                ClearSearch();
                ShowPage(page);
            }));
            return card;
        }
    }
}
#endif
