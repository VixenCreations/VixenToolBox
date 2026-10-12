#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VixenTools.Editor
{
    public partial class VixenHub
    {
        private const string HubLinkPrefix = "hub:";
        private const string LinkColor = "#ff4fc3";
        private const string CodeColor = "#ffb547";

        private class ChangelogEntry
        {
            public string Version;
            public string Date;
            public string Summary;
            public string Content;

            public bool Unreleased => Date != null && Date.IndexOf("Unreleased", System.StringComparison.OrdinalIgnoreCase) >= 0;
            public string Label => string.IsNullOrEmpty(Date) ? Version : Version + " - " + Date;
        }

        private readonly Dictionary<string, List<ChangelogEntry>> _changelogEntries = new Dictionary<string, List<ChangelogEntry>>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _selectedChangelogIndex = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        private readonly List<(int level, string text, VisualElement element)> _headings = new List<(int level, string text, VisualElement element)>();

        private static readonly string CodeMark = ((char)1).ToString();
        private static readonly string CodeEnd = ((char)2).ToString();
        private static readonly Regex CodeToken = new Regex(CodeMark + @"(\d+)" + CodeEnd);

        private static readonly Regex ReleaseHeading = new Regex(@"^##\s*\[([^\]]+)\]\s*(?:[-:]\s*(.*))?$");
        private static readonly Regex ItalicLine = new Regex(@"^\*([^*].*?)\*$");
        private static readonly Regex BulletLine = new Regex(@"^([-*+])\s+(.*)$");
        private static readonly Regex OrderedLine = new Regex(@"^(\d+)[.)]\s+(.*)$");
        private static readonly Regex CodeSpan = new Regex(@"`([^`]+)`");
        private static readonly Regex ImageSpan = new Regex(@"!\[([^\]]*)\]\([^)]*\)");
        private static readonly Regex LinkSpan = new Regex(@"\[([^\]]+)\]\(([^)\s]+)\)");
        private static readonly Regex BareUrl = new Regex(@"(?<!\]\()(?<![\[""=>])https?://[^\s<>()\[\]]*[^\s<>()\[\].,;:!?'""]");
        private static readonly Regex BoldSpan = new Regex(@"\*\*(.+?)\*\*");
        private static readonly Regex ItalicSpan = new Regex(@"(?<![*\w])\*(?![\s*])(.+?)(?<![\s*])\*(?![*\w])");
        private static readonly Regex RichTag = new Regex(@"</?(?:color|size|b|i|u|s|mark|alpha|font|sup|sub|link|noparse|nobr|cspace|mspace|voffset|indent|align|lowercase|uppercase|smallcaps|style|width|margin|line-height|rotate)\b[^>]*>", RegexOptions.IgnoreCase);
        private static readonly Regex Whitespace = new Regex(@"\s+");

        private static readonly string[] SectionTags = { "Added", "Changed", "Fixed", "Removed", "Heads up", "Notes", "Security", "Deprecated" };

        private static string LoadMarkdownFile(string fileName)
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(PackageRoot + fileName);
            if (asset != null) return CommentLine.Replace(asset.text, "");

            string fullPath = Path.Combine(PackageDirectory(), fileName);
            if (File.Exists(fullPath)) return CommentLine.Replace(File.ReadAllText(fullPath), "");

            return $"### File Not Found\nCould not find `{fileName}` in the toolbox package. Reinstall the Vixens Toolbox from the VRChat Creator Companion.";
        }

        private static string TakeLeadingTitle(ref string text)
        {
            string[] lines = text.Replace("\r", "").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || RuleLine.IsMatch(line)) continue;
                if (!line.StartsWith("# ")) return null;

                text = string.Join("\n", lines.Skip(i + 1));
                return line.Substring(2).Trim();
            }
            return null;
        }

        private void RenderMarkdown(string text, VisualElement container)
        {
            if (string.IsNullOrEmpty(text)) return;

            string[] lines = text.Replace("\r", "").Split('\n');
            var paragraph = new StringBuilder();

            System.Action flush = () =>
            {
                if (paragraph.Length == 0) return;
                string p = paragraph.ToString();
                paragraph.Clear();

                bool lead = p.Length > 4 && p.StartsWith("**") && p.IndexOf("**", 2, System.StringComparison.Ordinal) == p.Length - 2;
                container.Add(TextLabel(lead ? p.Substring(2, p.Length - 4) : p, lead ? "md-lead" : "md-p"));
            };

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();
                string trimmed = line.TrimStart();
                int indent = line.Length - trimmed.Length;

                if (trimmed.Length == 0)
                {
                    flush();
                    continue;
                }

                if (trimmed.StartsWith("```"))
                {
                    flush();
                    var code = new StringBuilder();
                    for (i++; i < lines.Length && !lines[i].TrimStart().StartsWith("```"); i++)
                        code.Append(lines[i].TrimEnd()).Append('\n');
                    container.Add(CodeBlock(code.ToString().TrimEnd('\n')));
                    continue;
                }

                if (trimmed.StartsWith("|"))
                {
                    flush();
                    var rows = new List<string>();
                    for (; i < lines.Length && lines[i].TrimStart().StartsWith("|"); i++) rows.Add(lines[i].Trim());
                    i--;
                    container.Add(Table(rows));
                    continue;
                }

                if (RuleLine.IsMatch(trimmed))
                {
                    flush();
                    var rule = new VisualElement();
                    rule.AddToClassList("md-rule");
                    container.Add(rule);
                    continue;
                }

                Match heading = HeadingLine.Match(trimmed);
                if (heading.Success)
                {
                    flush();
                    container.Add(Heading(heading.Groups[1].Value.Length, heading.Groups[2].Value.Trim().TrimEnd('#').Trim()));
                    continue;
                }

                if (trimmed.StartsWith(">"))
                {
                    flush();
                    var quote = new StringBuilder();
                    for (; i < lines.Length && lines[i].TrimStart().StartsWith(">"); i++)
                    {
                        string part = lines[i].TrimStart().Substring(1).Trim();
                        if (part.Length == 0) continue;
                        if (quote.Length > 0) quote.Append(' ');
                        quote.Append(part);
                    }
                    i--;
                    container.Add(Callout(quote.ToString()));
                    continue;
                }

                Match bullet = BulletLine.Match(trimmed);
                if (bullet.Success)
                {
                    flush();
                    container.Add(ListItem("•", bullet.Groups[2].Value, indent));
                    continue;
                }

                Match ordered = OrderedLine.Match(trimmed);
                if (ordered.Success)
                {
                    flush();
                    container.Add(ListItem(ordered.Groups[1].Value + ".", ordered.Groups[2].Value, indent));
                    continue;
                }

                if (paragraph.Length > 0) paragraph.Append(' ');
                paragraph.Append(trimmed);
            }

            flush();
        }

        private static Label TextLabel(string markdown, string className)
        {
            var label = new Label(FormatInline(markdown)) { enableRichText = true };
            label.AddToClassList(className);
            return label;
        }

        private VisualElement Heading(int level, string raw)
        {
            string plain = PlainText(raw);
            string tag = level >= 3 ? SectionTags.FirstOrDefault(t => string.Equals(t, plain, System.StringComparison.OrdinalIgnoreCase)) : null;

            Label label;
            if (tag != null)
            {
                label = new Label(plain.ToUpperInvariant());
                label.AddToClassList("md-tag");
                label.AddToClassList("md-tag--" + tag.ToLowerInvariant().Replace(' ', '-'));
            }
            else
            {
                label = new Label(FormatInline(raw.Replace("**", ""))) { enableRichText = true };
                label.AddToClassList(level == 1 ? "md-h1" : level == 2 ? "md-h2" : "md-h3");
            }

            _headings.Add((level, plain, label));
            return label;
        }

        private static VisualElement ListItem(string marker, string markdown, int indent)
        {
            var row = new VisualElement();
            row.AddToClassList("md-li");
            if (indent >= 4) row.AddToClassList("md-li--l2");
            else if (indent >= 2) row.AddToClassList("md-li--l1");

            var bullet = new Label(marker);
            bullet.AddToClassList("md-bullet");
            row.Add(bullet);
            row.Add(TextLabel(markdown, "md-li-text"));
            return row;
        }

        private static VisualElement Callout(string markdown)
        {
            var box = new VisualElement();
            box.AddToClassList("md-callout");
            box.Add(TextLabel(markdown, "md-callout-text"));
            return box;
        }

        private static VisualElement CodeBlock(string code)
        {
            var box = new VisualElement();
            box.AddToClassList("md-code");
            var label = new Label(code) { enableRichText = false };
            label.AddToClassList("md-code-text");
            box.Add(label);
            return box;
        }

        private static VisualElement Table(List<string> rows)
        {
            var table = new VisualElement();
            table.AddToClassList("md-table");

            bool first = true;
            for (int r = 0; r < rows.Count; r++)
            {
                if (TableRule.IsMatch(rows[r])) continue;

                bool header = r + 1 < rows.Count && TableRule.IsMatch(rows[r + 1]) && rows[r + 1].Contains("-");
                var row = new VisualElement();
                row.AddToClassList("md-tr");
                if (header) row.AddToClassList("md-tr--head");
                else if (first) row.AddToClassList("md-tr--first");
                first = false;

                foreach (string cell in rows[r].Trim('|').Split('|'))
                {
                    var td = new Label(FormatInline(cell.Trim())) { enableRichText = true };
                    td.AddToClassList("md-td");
                    row.Add(td);
                }
                table.Add(row);
            }
            return table;
        }

        private static string FormatInline(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            var codes = new List<string>();
            text = CodeSpan.Replace(text, m =>
            {
                codes.Add(m.Groups[1].Value);
                return CodeMark + (codes.Count - 1) + CodeEnd;
            });

            text = ImageSpan.Replace(text, "$1");
            text = BareUrl.Replace(text, m => LinkTag(m.Value, m.Value));
            text = LinkSpan.Replace(text, m => LinkFor(m.Groups[1].Value, m.Groups[2].Value));
            text = BoldSpan.Replace(text, "<b><color=#ffffff>$1</color></b>");
            text = ItalicSpan.Replace(text, "<i>$1</i>");
            text = CodeToken.Replace(text, m => "<color=" + CodeColor + ">" + codes[int.Parse(m.Groups[1].Value)] + "</color>");
            return text;
        }

        private static string LinkFor(string label, string target)
        {
            if (IsWebLink(target)) return LinkTag(target, label);

            string file = System.Uri.UnescapeDataString(target.Split('#')[0]);
            if (file.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase)) return LinkTag(HubLinkPrefix + Path.GetFileName(file), label);

            return "<b>" + label + "</b>";
        }

        private static string LinkTag(string target, string label)
        {
            return "<link=\"" + target + "\"><u><color=" + LinkColor + ">" + label + "</color></u></link>";
        }

        private static string PlainText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            text = RichTag.Replace(text, "");
            text = ImageSpan.Replace(text, "$1");
            text = LinkSpan.Replace(text, "$1");
            text = text.Replace("**", "").Replace("`", "");
            text = ItalicSpan.Replace(text, "$1");
            return Whitespace.Replace(text, " ").Trim();
        }

        private void ScrollToHeading(string anchor)
        {
            VisualElement target = _headings.FirstOrDefault(h => string.Equals(h.text, anchor, System.StringComparison.OrdinalIgnoreCase)).element;
            if (target == null) return;

            if (!float.IsNaN(target.layout.height) && target.layout.height > 0f)
            {
                JumpTo(target);
                return;
            }

            EventCallback<GeometryChangedEvent> once = null;
            once = e =>
            {
                target.UnregisterCallback(once);
                target.schedule.Execute(() => JumpTo(target));
            };
            target.RegisterCallback(once);
        }

        private void JumpTo(VisualElement target)
        {
            if (target.panel == null || _contentScroll == null) return;

            float y = target.ChangeCoordinatesTo(_contentScroll.contentContainer, Vector2.zero).y;
            _contentScroll.scrollOffset = new Vector2(0f, Mathf.Max(0f, y - 8f));

            target.AddToClassList("md-flash");
            target.schedule.Execute(() => target.RemoveFromClassList("md-flash")).StartingIn(900);
        }

        private List<ChangelogEntry> ChangelogEntries(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return new List<ChangelogEntry>();
            if (_changelogEntries.TryGetValue(fileName, out List<ChangelogEntry> cached)) return cached;

            var entries = new List<ChangelogEntry>();
            ChangelogEntry current = null;
            var content = new StringBuilder();

            System.Action close = () =>
            {
                if (current == null) return;

                var body = content.ToString().Split('\n').ToList();
                while (body.Count > 0 && (body[body.Count - 1].Trim().Length == 0 || RuleLine.IsMatch(body[body.Count - 1].Trim()))) body.RemoveAt(body.Count - 1);
                while (body.Count > 0 && body[0].Trim().Length == 0) body.RemoveAt(0);

                if (body.Count > 0)
                {
                    Match summary = ItalicLine.Match(body[0].Trim());
                    if (summary.Success)
                    {
                        current.Summary = summary.Groups[1].Value.Trim();
                        body.RemoveAt(0);
                    }
                }

                current.Content = string.Join("\n", body);
                entries.Add(current);
                content.Clear();
            };

            foreach (string raw in LoadMarkdownFile(fileName).Replace("\r", "").Split('\n'))
            {
                string line = raw.TrimEnd();
                Match release = ReleaseHeading.Match(line);
                if (release.Success)
                {
                    close();
                    current = new ChangelogEntry
                    {
                        Version = release.Groups[1].Value.Trim(),
                        Date = release.Groups[2].Success && release.Groups[2].Value.Trim().Length > 0 ? release.Groups[2].Value.Trim() : null
                    };
                    continue;
                }
                if (current != null) content.Append(line).Append('\n');
            }
            close();

            _changelogEntries[fileName] = entries;
            return entries;
        }

        private void RenderChangelog(string fileName, VisualElement container)
        {
            List<ChangelogEntry> entries = ChangelogEntries(fileName);
            if (entries.Count == 0)
            {
                RenderMarkdown($"Could not read the versions in `{fileName}`.", container);
                return;
            }

            if (!_selectedChangelogIndex.TryGetValue(fileName, out int selected) || selected < 0 || selected >= entries.Count) selected = 0;
            int latest = Mathf.Max(0, entries.FindIndex(e => !e.Unreleased));

            var bar = new VisualElement();
            bar.AddToClassList("cl-bar");

            var newer = new Button { text = "‹  Newer", tooltip = "The release after this one." };
            newer.AddToClassList("hub-btn");
            newer.AddToClassList("hub-btn--quiet");

            var picker = new DropdownField(entries.Select(e => e.Label).ToList(), selected);
            picker.AddToClassList("cl-picker");

            var older = new Button { text = "Older  ›", tooltip = "The release before this one." };
            older.AddToClassList("hub-btn");
            older.AddToClassList("hub-btn--quiet");

            bar.Add(newer);
            bar.Add(picker);
            bar.Add(older);
            container.Add(bar);

            var release = new VisualElement();
            container.Add(release);

            System.Action<int> show = i =>
            {
                i = Mathf.Clamp(i, 0, entries.Count - 1);
                _selectedChangelogIndex[fileName] = i;
                picker.SetValueWithoutNotify(entries[i].Label);
                newer.SetEnabled(i > 0);
                older.SetEnabled(i < entries.Count - 1);

                _headings.Clear();
                release.Clear();

                ChangelogEntry entry = entries[i];
                var head = new VisualElement();
                head.AddToClassList("cl-head");
                var version = new Label(entry.Version);
                version.AddToClassList("cl-version");
                head.Add(version);
                if (entry.Version == _packageVersion) head.Add(Chip("Installed", "hub-chip--green"));
                if (!string.IsNullOrEmpty(entry.Date)) head.Add(Chip(entry.Date, entry.Unreleased ? "hub-chip--pink" : null));
                if (i == latest && !entry.Unreleased) head.Add(Chip("Latest", "hub-chip--cyan"));
                release.Add(head);

                if (!string.IsNullOrEmpty(entry.Summary))
                {
                    var summary = new Label(FormatInline(entry.Summary)) { enableRichText = true };
                    summary.AddToClassList("cl-summary");
                    release.Add(summary);
                }

                RenderMarkdown(entry.Content, release);
            };

            System.Action<int> step = delta =>
            {
                _selectedChangelogIndex.TryGetValue(fileName, out int current);
                show(current + delta);
                _contentScroll.scrollOffset = Vector2.zero;
            };

            newer.clicked += () => step(-1);
            older.clicked += () => step(1);
            picker.RegisterValueChangedCallback(e =>
            {
                int i = picker.index;
                if (i < 0) return;
                show(i);
                _contentScroll.scrollOffset = Vector2.zero;
            });

            show(selected);
        }

        private void RenderSupporters(string fileName, VisualElement container)
        {
            string text = LoadMarkdownFile(fileName);
            string title = TakeLeadingTitle(ref text);
            if (title != null) _pageTitle.text = FormatInline(title);

            var intro = new StringBuilder();
            var outro = new StringBuilder();
            var tiers = new List<(string name, string tagline, List<string> people)>();
            bool inTier = false;

            foreach (string raw in text.Replace("\r", "").Split('\n'))
            {
                string line = raw.Trim();

                if (line.StartsWith("## "))
                {
                    tiers.Add((RichTag.Replace(line.Substring(3), "").Trim(), null, new List<string>()));
                    inTier = true;
                    continue;
                }

                if (!inTier)
                {
                    (tiers.Count == 0 ? intro : outro).Append(raw).Append('\n');
                    continue;
                }

                if (RuleLine.IsMatch(line))
                {
                    inTier = false;
                    continue;
                }
                if (line.Length == 0) continue;

                var tier = tiers[tiers.Count - 1];
                if (line.StartsWith(">"))
                {
                    tiers[tiers.Count - 1] = (tier.name, RichTag.Replace(line.Substring(1), "").Trim(), tier.people);
                    continue;
                }

                Match bullet = BulletLine.Match(line);
                if (bullet.Success) tier.people.Add(RichTag.Replace(bullet.Groups[2].Value, "").Trim());
            }

            RenderMarkdown(intro.ToString(), container);

            var grid = new VisualElement();
            grid.AddToClassList("sup-grid");
            foreach (var tier in tiers) grid.Add(SupporterCard(tier.name, tier.tagline, tier.people));
            container.Add(grid);

            RenderMarkdown(outro.ToString(), container);
        }

        private VisualElement SupporterCard(string name, string tagline, List<string> people)
        {
            var card = new VisualElement();
            card.AddToClassList("sup-card");
            card.AddToClassList(TierAccent(name));

            var title = new Label(name) { enableRichText = false };
            title.AddToClassList("sup-name");
            card.Add(title);
            _headings.Add((2, name, title));

            if (!string.IsNullOrEmpty(tagline))
            {
                var line = new Label(tagline) { enableRichText = false };
                line.AddToClassList("sup-tagline");
                card.Add(line);
            }

            var names = new VisualElement();
            names.AddToClassList("sup-names");
            foreach (string person in people)
            {
                if (person.StartsWith("Nobody here yet", System.StringComparison.OrdinalIgnoreCase))
                {
                    var empty = new Label(person) { enableRichText = false };
                    empty.AddToClassList("sup-empty");
                    card.Add(empty);
                    continue;
                }

                var chip = new Label(person) { enableRichText = false };
                chip.AddToClassList("sup-person");
                names.Add(chip);
            }
            if (names.childCount > 0) card.Add(names);

            return card;
        }

        private static string TierAccent(string name)
        {
            string upper = name.ToUpperInvariant();
            if (upper.Contains("SCRIPT")) return "sup--green";
            if (upper.Contains("PIPELINE")) return "sup--orange";
            if (upper.Contains("VANGUARD")) return "sup--gold";
            return "sup--cyan";
        }
    }
}
#endif
