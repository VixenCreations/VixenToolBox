/* ============================================================================
   VIXFORGE DOCS SHELL
   Sticky section rail, live filter, shader chips and collapsible sub-groups
   for the long reference pages. Pairs with docs-shell.css.

   A page opts in with:
       <body class="docs-shell">
   and a layout of
       <div class="docs-layout">
           <div class="docs-main"> ...sections... </div>
       </div>
   Sections are elements carrying .shader-section with an id and an <h2>.
   Sub-groups are <h3 class="opt-group"> inside a section.

   Everything below degrades to a plain scrolling page if JS never runs, so the
   markup must remain readable on its own.
   ============================================================================ */

(function () {
    'use strict';

    // Which element carries a section's title. Set per page from
    // data-shell-heading; the rail and the spy both read it.
    var HEADING_SEL = 'h2';

    function initDocsShell() {
        var layout = document.querySelector('.docs-layout');
        var main = document.querySelector('.docs-main');
        if (!layout || !main) return;

        // Reference pages use .shader-section panels. The changelogs and news
        // are a flat run of entries instead, so a page can name its own
        // section element and the heading to read a title from.
        var sel = main.getAttribute('data-shell-sections') || '.shader-section';
        var headSel = main.getAttribute('data-shell-heading') || 'h2';

        var sections = Array.prototype.slice.call(main.querySelectorAll(sel));
        if (!sections.length) return;

        // Entries carry data-version rather than an id; the rail needs one to
        // link to, so mint it. data-version is unique per release, but a news
        // article's data-chips is a category two articles can share, so the
        // heading text is preferred and a counter guarantees uniqueness. A
        // duplicate id here silently breaks both anchors and the rail lookup.
        var used = {};
        sections.forEach(function (section, i) {
            if (section.id) {
                used[section.id] = true;
                return;
            }
            var head = section.querySelector(headSel);
            var key = section.getAttribute('data-version') || (head ? text(head) : '') ||
                      section.getAttribute('data-chips') || ('entry-' + i);
            var base = slug(key) || ('entry-' + i);
            var id = base;
            var n = 2;
            while (used[id] || document.getElementById(id)) {
                id = base + '-' + n++;
            }
            used[id] = true;
            section.id = id;
        });

        HEADING_SEL = headSel;

        var groups = wrapGroups(sections);

        // A page can be one section per thing with no sub-headings at all (the
        // products catalogue). Those sections carry their own tags, so chips
        // have to come from the sections as well as from the groups.
        // Chip keys come from one of two places, never both: the .shader-tag
        // pills already in the copy, or an attribute the entries carry. Two
        // filter bars fighting over the same sections is what left the news
        // page with a topic filter the search box would silently undo.
        var chipAttr = main.getAttribute('data-chip-attr');
        var chipMode = main.getAttribute('data-chip-mode') || 'prefix';

        sections.forEach(function (section) {
            if (chipAttr) {
                var raw = section.getAttribute(chipAttr) || '';
                var key = chipMode === 'major' ? raw.split('.')[0] : raw.split('-')[0];
                section.shaders = key ? [key.toLowerCase()] : [];
            } else {
                section.shaders = Array.prototype.slice
                    .call(section.querySelectorAll('.shader-tag'))
                    .map(function (t) { return text(t).toLowerCase(); });
            }
        });

        var rail = buildRail(layout, sections, groups);
        var toolbar = buildToolbar(main, sections, groups, rail);

        // A changelog shows one release at a time (app.js sets entry.hidden),
        // so there is nothing to scroll and nothing to spy on. There the rail
        // becomes the picker itself, driving the same <select> app.js already
        // listens to, which keeps the prev/next buttons in step.
        if (main.getAttribute('data-shell-mode') === 'picker') {
            wirePicker(sections, rail);
        } else {
            wireSpy(sections, groups, rail);
        }

        wireCollapse(groups);
        openFromHash(groups);

        // A deep link that lands on a collapsed group has to open it.
        window.addEventListener('hashchange', function () { openFromHash(groups); });

        if (toolbar) toolbar.update();
    }

    /* ---------------------------------------------------------- structure -- */

    // Each <h3 class="opt-group"> owns everything up to the next h3 or the end
    // of its section. Wrapping that run gives us one node to hide or collapse.
    function wrapGroups(sections) {
        var groups = [];

        sections.forEach(function (section) {
            var heads = Array.prototype.slice.call(section.querySelectorAll('h3.opt-group'));

            heads.forEach(function (head, i) {
                var body = document.createElement('div');
                body.className = 'docs-group-body';

                var node = head.nextSibling;
                var stop = heads[i + 1] || null;
                var moved = [];
                while (node && node !== stop) {
                    var next = node.nextSibling;
                    moved.push(node);
                    node = next;
                }
                moved.forEach(function (n) { body.appendChild(n); });
                head.parentNode.insertBefore(body, head.nextSibling);

                if (!head.id) head.id = slug(section.id + '-' + text(head));
                head.setAttribute('role', 'button');
                head.setAttribute('tabindex', '0');
                head.setAttribute('aria-expanded', 'true');
                head.setAttribute('aria-controls', head.id + '-body');
                body.id = head.id + '-body';

                var g = {
                    head: head,
                    body: body,
                    section: section,
                    shaders: readShaderTags(body, head, section)
                };
                // Deep-dive material can ship folded so the page opens short.
                if (head.hasAttribute('data-collapsed')) setCollapsed(g, true);
                groups.push(g);
            });
        });

        return groups;
    }

    // Availability comes from the .shader-tag pills already in the copy, so no
    // page needs a second source of truth for what a feature applies to.
    // Tags on the group win; otherwise the section's own tags apply to all of
    // its groups, which is how a whole-page-per-thing layout (one section per
    // tool) expresses "this entire thing is Avatar only".
    function readShaderTags(body, head, section) {
        var tags = [].concat(
            Array.prototype.slice.call(body.querySelectorAll('.shader-tag')),
            Array.prototype.slice.call(head.querySelectorAll('.shader-tag'))
        );
        if (!tags.length && section) {
            tags = Array.prototype.slice.call(section.querySelectorAll('.shader-tag'));
        }
        return tags.map(function (t) { return text(t).toLowerCase(); });
    }

    /* --------------------------------------------------------------- rail -- */

    function buildRail(layout, sections, groups) {
        var rail = document.createElement('nav');
        rail.className = 'docs-rail';
        rail.setAttribute('aria-label', 'On this page');

        var title = document.createElement('div');
        title.className = 'docs-rail-title';
        title.textContent = 'On this page';
        rail.appendChild(title);

        var list = document.createElement('div');
        list.className = 'docs-rail-list';
        rail.appendChild(list);

        var links = [];

        sections.forEach(function (section) {
            var h2 = section.querySelector(HEADING_SEL);
            if (!h2 || !section.id) return;

            links.push(addLink(list, section.id, text(h2), 'is-section', section));

            groups.filter(function (g) { return g.section === section; })
                  .forEach(function (g) {
                      links.push(addLink(list, g.head.id, text(g.head), 'is-sub', g.head, g));
                  });
        });

        layout.appendChild(rail);
        rail.links = links;
        rail.list = list;
        return rail;
    }

    function addLink(list, id, label, cls, target, group) {
        var a = document.createElement('a');
        a.className = 'docs-rail-link ' + cls;
        a.href = '#' + id;
        a.textContent = label;
        a.target_el = target;
        a.group = group || null;
        list.appendChild(a);
        return a;
    }

    /* ------------------------------------------------------------ toolbar -- */

    function buildToolbar(main, sections, groups, rail) {
        var bar = document.createElement('div');
        bar.className = 'docs-toolbar';

        var row = document.createElement('div');
        row.className = 'docs-toolbar-row';

        var search = document.createElement('input');
        search.type = 'search';
        search.className = 'docs-search';
        search.placeholder = main.dataset.searchLabel || 'Filter controls…';
        search.setAttribute('aria-label', 'Filter this page');

        var count = document.createElement('div');
        count.className = 'docs-count';

        var collapseBtn = document.createElement('button');
        collapseBtn.type = 'button';
        collapseBtn.className = 'docs-tool-btn';
        collapseBtn.textContent = 'Collapse all';

        row.appendChild(search);
        row.appendChild(count);
        // Offering "Collapse all" where nothing is collapsible is a dead
        // control; the news page has no sub-groups at all.
        if (groups.length) row.appendChild(collapseBtn);
        bar.appendChild(row);

        // Shader chips, built from whatever tags the page actually uses.
        var names = shaderNames(groups, sections);
        var active = {};
        if (names.length > 1) {
            var chips = document.createElement('div');
            chips.className = 'docs-chips';

            var label = document.createElement('span');
            label.className = 'docs-chips-label';
            // "Shader" is wrong on a news or toolbox page; let the page say.
            label.textContent = main.getAttribute('data-chip-label') || 'Shader';
            chips.appendChild(label);

            names.forEach(function (name) {
                var chip = document.createElement('button');
                chip.type = 'button';
                chip.className = 'filter-chip';
                chip.textContent = CHIP_LABELS[name] ||
                    name.replace(/\b\w/g, function (c) { return c.toUpperCase(); });
                chip.addEventListener('click', function () {
                    if (active[name]) delete active[name]; else active[name] = true;
                    chip.classList.toggle('is-on');
                    apply();
                });
                chips.appendChild(chip);
            });

            var showAll = document.createElement('button');
            showAll.type = 'button';
            showAll.className = 'docs-tool-btn';
            showAll.textContent = 'Show all';
            showAll.addEventListener('click', function () {
                active = {};
                Array.prototype.forEach.call(chips.querySelectorAll('.filter-chip'), function (c) {
                    c.classList.remove('is-on');
                });
                apply();
            });
            chips.appendChild(showAll);

            bar.appendChild(chips);
        }

        main.insertBefore(bar, main.firstChild);

        var empty = document.createElement('div');
        empty.className = 'docs-empty docs-hidden';
        empty.textContent = 'Nothing matches that. Try a shorter word.';
        main.appendChild(empty);

        var timer = null;
        search.addEventListener('input', function () {
            clearTimeout(timer);
            timer = setTimeout(apply, 120);
        });

        collapseBtn.addEventListener('click', function () {
            var collapsing = collapseBtn.textContent === 'Collapse all';
            groups.forEach(function (g) { setCollapsed(g, collapsing); });
            collapseBtn.textContent = collapsing ? 'Expand all' : 'Collapse all';
        });

        var picker = main.getAttribute('data-shell-mode') === 'picker';

        function applyPicker() {
            // One release is on screen at a time, so the chips and the search
            // filter the rail: they decide which releases are reachable, and
            // wirePicker moves the selection to one that still is.
            var q = search.value.trim().toLowerCase();
            var wanted = Object.keys(active);
            var hits = 0;

            sections.forEach(function (section) {
                var tags = section.shaders || [];
                var okChip = !wanted.length || !tags.length ||
                    wanted.some(function (w) {
                        return tags.some(function (t) { return t.indexOf(w) !== -1; });
                    });
                section.classList.toggle('docs-hidden', !okChip);

                var hit = okChip && (!q || section.textContent.toLowerCase().indexOf(q) !== -1);
                section.matchesSearch = hit;
                if (hit) hits++;
            });
            if (rail && rail.links) {
                rail.links.forEach(function (a) {
                    var t = a.target_el;
                    var hide = t && (!t.matchesSearch || t.classList.contains('docs-hidden'));
                    a.classList.toggle('is-hidden', !!hide);
                });
            }
            count.innerHTML = q
                ? '<strong>' + hits + '</strong> ' + plural(hits, 'release', 'releases') + ' mention that'
                : '<strong>' + hits + '</strong> ' + plural(hits, 'release', 'releases');

            // Narrowing the list can strand the release on screen; move to one
            // that is still in it.
            if (rail && rail.repick) rail.repick();
        }

        function apply() {
            if (picker) return applyPicker();
            var q = search.value.trim().toLowerCase();
            var wanted = Object.keys(active);
            var shown = 0;

            groups.forEach(function (g) {
                var okShader = !wanted.length || !g.shaders.length ||
                    wanted.some(function (w) {
                        return g.shaders.some(function (s) { return s.indexOf(w) !== -1; });
                    });

                var items = Array.prototype.slice.call(g.body.querySelectorAll('li'));
                var hits = 0;

                if (!q) {
                    items.forEach(function (li) {
                        li.classList.remove('docs-hidden', 'docs-hit');
                    });
                    hits = items.length;
                } else {
                    items.forEach(function (li) {
                        // Only test leaf items so a parent never hides its match.
                        if (li.querySelector('li')) {
                            li.classList.remove('docs-hidden', 'docs-hit');
                            return;
                        }
                        var hit = li.textContent.toLowerCase().indexOf(q) !== -1;
                        li.classList.toggle('docs-hidden', !hit);
                        li.classList.toggle('docs-hit', hit);
                        if (hit) hits++;
                    });
                }

                var visible = okShader && (!q || hits > 0);
                g.head.classList.toggle('docs-hidden', !visible);
                g.body.classList.toggle('docs-hidden', !visible);
                if (visible) shown += hits;

                // A filter that leaves a group collapsed hides its own results.
                if (q && visible) setCollapsed(g, false);
            });

            sections.forEach(function (section) {
                var mine = groups.filter(function (g) { return g.section === section; });
                var anyGroup = mine.some(function (g) { return !g.head.classList.contains('docs-hidden'); });

                // Loose items that sit outside any sub-group. Product cards
                // count as items too, so a catalogue page filters card by card.
                var loose = Array.prototype.slice
                    .call(section.querySelectorAll('li, .ecosystem-card'))
                    .filter(function (li) { return !li.closest('.docs-group-body'); });
                var looseHits = 0;
                loose.forEach(function (li) {
                    if (li.querySelector('li')) return;
                    if (!q) {
                        li.classList.remove('docs-hidden', 'docs-hit');
                        looseHits++;
                        return;
                    }
                    var hit = li.textContent.toLowerCase().indexOf(q) !== -1;
                    li.classList.toggle('docs-hidden', !hit);
                    li.classList.toggle('docs-hit', hit);
                    if (hit) looseHits++;
                });
                if (!q) looseHits = loose.length;
                else shown += looseHits;

                // Sections without sub-groups are filtered on their own tags.
                var ownTags = section.shaders || [];
                var okSection = !wanted.length || !ownTags.length ||
                    wanted.some(function (w) {
                        return ownTags.some(function (t) { return t.indexOf(w) !== -1; });
                    });

                // With no search term a section stands on its tags alone; a
                // section that has neither groups nor items would otherwise be
                // hidden by a chip it actually matches.
                //
                // Sections whose content is prose rather than a list (a news
                // article) have nothing for an item-level search to match, so
                // fall back to the section's own text.
                var hasContent = mine.length > 0 || loose.length > 0;
                var proseHit = q && section.textContent.toLowerCase().indexOf(q) !== -1;
                var matches = !q ? true
                    : (anyGroup || looseHits > 0 || proseHit || !hasContent);

                section.classList.toggle('docs-hidden', !(okSection && matches));
            });

            if (rail && rail.links) {
                rail.links.forEach(function (a) {
                    var hidden = a.target_el && a.target_el.classList.contains('docs-hidden');
                    a.classList.toggle('is-hidden', !!hidden);
                });
            }

            var visibleSections = sections.filter(function (s) { return !s.classList.contains('docs-hidden'); }).length;
            empty.classList.toggle('docs-hidden', visibleSections > 0);

            count.innerHTML = q
                ? '<strong>' + shown + '</strong> ' + plural(shown, 'match', 'matches') +
                  ' in <strong>' + visibleSections + '</strong> ' + plural(visibleSections, 'section', 'sections')
                : '<strong>' + visibleSections + '</strong> ' + plural(visibleSections, 'section', 'sections');
        }

        return { update: apply, search: search };
    }

    function shaderNames(groups, sections) {
        var seen = {};
        groups.forEach(function (g) {
            g.shaders.forEach(function (s) { seen[s] = true; });
        });
        (sections || []).forEach(function (sec) {
            (sec.shaders || []).forEach(function (s) { seen[s] = true; });
        });
        return Object.keys(seen).sort();
    }

    /* ----------------------------------------------------------- collapse -- */

    function wireCollapse(groups) {
        groups.forEach(function (g) {
            g.head.addEventListener('click', function () { setCollapsed(g, !isCollapsed(g)); });
            g.head.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    setCollapsed(g, !isCollapsed(g));
                }
            });
        });
    }

    function isCollapsed(g) { return g.head.classList.contains('is-collapsed'); }

    function setCollapsed(g, on) {
        g.head.classList.toggle('is-collapsed', on);
        g.body.classList.toggle('is-collapsed', on);
        g.head.setAttribute('aria-expanded', on ? 'false' : 'true');
    }

    function openFromHash(groups) {
        var id = location.hash.slice(1);
        if (!id) return;
        var el = document.getElementById(id);
        if (!el) return;
        groups.forEach(function (g) {
            if (g.head === el || g.body.contains(el)) setCollapsed(g, false);
        });
    }

    /* --------------------------------------------------------------- spy -- */

    // Measured straight off positions rather than tracked through
    // IntersectionObserver state. A section here can be 50,000px tall and a fast
    // jump skips whole runs of entries, which left the old observer version
    // holding a stale winner; reading the DOM each frame cannot go stale.
    function wireSpy(sections, groups, rail) {
        if (!rail) return;

        var LINE = 130; // the reading line, in px from the top of the viewport
        var byId = {};
        rail.links.forEach(function (a) { byId[a.getAttribute('href').slice(1)] = a; });

        var targets = sections.concat(groups.map(function (g) { return g.head; }))
            .filter(function (t) { return t.id && byId[t.id]; });

        var last = null;
        var queued = false;

        function pick() {
            queued = false;

            var best = null;
            var bestTop = -Infinity;
            var firstVisible = null;

            for (var i = 0; i < targets.length; i++) {
                var t = targets[i];
                if (t.classList.contains('docs-hidden')) continue;
                var top = t.getBoundingClientRect().top;
                if (!firstVisible && top > 0) firstVisible = t;
                // The lowest heading that is still above the reading line.
                if (top <= LINE && top > bestTop) {
                    bestTop = top;
                    best = t;
                }
            }

            // Above the first heading, light the first one rather than nothing.
            if (!best) best = firstVisible || targets[0];
            if (!best || best === last) return;
            last = best;

            rail.links.forEach(function (a) { a.classList.remove('is-active'); });
            var link = byId[best.id];
            if (!link) return;
            link.classList.add('is-active');
            keepInView(rail.list, link);
        }

        function onScroll() {
            if (queued) return;
            queued = true;
            window.requestAnimationFrame(pick);
        }

        // styles.css gives body `overflow: hidden auto`, so the page scrolls a
        // nested container and scroll never reaches window. Scroll does not
        // bubble, so listen on document in the capture phase to catch it from
        // whichever element actually moves.
        document.addEventListener('scroll', onScroll, { capture: true, passive: true });
        window.addEventListener('scroll', onScroll, { passive: true });
        window.addEventListener('resize', onScroll, { passive: true });
        rail.repick = onScroll;
        pick();
    }

    // Rail-as-picker. One release shows at a time and the rail chooses it.
    //
    // This owns the whole behaviour rather than driving the old "Jump to"
    // <select>, because that select was the bug: filtering to one shader left
    // it pointing at a release the filter had just hidden, so the panel went
    // blank. The rail already lists exactly what the filter allows, so the
    // selection follows the filter instead of fighting it.
    function wirePicker(sections, rail) {
        if (!rail) return;

        var prevBtn = document.getElementById('prevVersionBtn');   // newer
        var nextBtn = document.getElementById('nextVersionBtn');   // older
        var current = null;

        function available() {
            return sections.filter(function (s) {
                return !s.classList.contains('docs-hidden') && s.matchesSearch !== false;
            });
        }

        function show(section) {
            if (!section) return;
            current = section;
            sections.forEach(function (s) { s.hidden = (s !== section); });

            rail.links.forEach(function (a) {
                a.classList.toggle('is-active', a.target_el === section);
            });
            var active = rail.list.querySelector('.is-active');
            if (active) keepInView(rail.list, active);

            var list = available();
            var i = list.indexOf(section);
            setDisabled(prevBtn, i <= 0);
            setDisabled(nextBtn, i < 0 || i >= list.length - 1);
        }

        function setDisabled(btn, off) {
            if (!btn) return;
            if (off) btn.setAttribute('disabled', 'true');
            else btn.removeAttribute('disabled');
        }

        function step(by) {
            var list = available();
            var i = list.indexOf(current);
            if (i < 0) return show(list[0]);
            var next = list[i + by];
            if (next) show(next);
        }

        rail.links.forEach(function (a) {
            if (!a.target_el) return;
            a.addEventListener('click', function (e) {
                e.preventDefault();
                show(a.target_el);
            });
        });

        if (prevBtn) prevBtn.addEventListener('click', function () { step(-1); });
        if (nextBtn) nextBtn.addEventListener('click', function () { step(1); });

        // Called after a chip or search changes what is available. If the
        // release on screen is no longer in the list, move to the first that is.
        rail.repick = function () {
            var list = available();
            if (!list.length) return;
            if (!current || list.indexOf(current) === -1) show(list[0]);
            else show(current);
        };

        show(available()[0] || sections[0]);
    }

    function keepInView(list, link) {
        var lr = link.getBoundingClientRect();
        var cr = list.getBoundingClientRect();
        if (lr.top < cr.top || lr.bottom > cr.bottom) {
            list.scrollTop += (lr.top - cr.top) - cr.height / 2 + lr.height / 2;
        }
    }

    /* ------------------------------------------------------------- utils -- */

    function text(el) { return (el.textContent || '').replace(/\s+/g, ' ').trim(); }

    function plural(n, one, many) { return n === 1 ? one : many; }

    function slug(s) {
        return s.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '').slice(0, 60);
    }

    /* =======================================================================
       CHIP BAR
       A lighter mode for pages that are a flat list rather than sections: the
       changelogs and the news. Declare it with

         <div class="docs-chipbar"
              data-chip-items=".changelog-entry"
              data-chip-attr="data-version"></div>

       Keys come from the item attribute, cut at the first "-", so an entry
       already marked data-version="latex-4.8.0" needs no new markup.
       ======================================================================= */

    var CHIP_LABELS = {
        latex: 'Latex Ultra', toon: 'Toon', clothingpro: 'Clothing Pro',
        furpro: 'Fur Pro', editor: 'Editor', worldsurface: 'World Surface',
        worldfur: 'World Fur', toolbox: 'Toolbox', shaders: 'Shaders',
        v2: 'Toolbox 2.x', v1: 'Toolbox 1.x'
    };

    function initChipBar() {
        var bars = document.querySelectorAll('.docs-chipbar');
        Array.prototype.forEach.call(bars, function (bar) {
            var sel = bar.getAttribute('data-chip-items');
            var attr = bar.getAttribute('data-chip-attr') || 'data-chips';
            if (!sel) return;

            var items = Array.prototype.slice.call(document.querySelectorAll(sel));
            if (!items.length) return;

            items.forEach(function (el) {
                var raw = el.getAttribute(attr) || '';
                el.chipKey = raw.split('-')[0].toLowerCase();
            });

            var keys = [];
            items.forEach(function (el) {
                if (el.chipKey && keys.indexOf(el.chipKey) === -1) keys.push(el.chipKey);
            });
            if (keys.length < 2) return;

            keys.sort(function (a, b) {
                return (CHIP_LABELS[a] || a).localeCompare(CHIP_LABELS[b] || b);
            });

            var label = document.createElement('span');
            label.className = 'docs-chips-label';
            label.textContent = bar.getAttribute('data-chip-label') || 'Show';
            bar.appendChild(label);

            var active = {};
            var count = document.createElement('span');
            count.className = 'docs-count';

            var rail = document.querySelector('.docs-rail');

            function apply() {
                var wanted = Object.keys(active);
                var shown = 0;
                items.forEach(function (el) {
                    var keep = !wanted.length || wanted.indexOf(el.chipKey) !== -1;
                    el.classList.toggle('docs-hidden', !keep);
                    if (keep) shown++;
                });
                count.innerHTML = '<strong>' + shown + '</strong> of ' + items.length;

                // The rail lists the same entries, so it has to agree with the
                // chips; otherwise filtering to one shader still shows all 56
                // releases in the sidebar.
                if (rail && rail.links) {
                    rail.links.forEach(function (a) {
                        var t = a.target_el;
                        a.classList.toggle('is-hidden', !!(t && t.classList.contains('docs-hidden')));
                    });
                    if (rail.repick) rail.repick();
                }
            }

            keys.forEach(function (k) {
                var chip = document.createElement('button');
                chip.type = 'button';
                chip.className = 'filter-chip';
                chip.textContent = CHIP_LABELS[k] || k;
                chip.addEventListener('click', function () {
                    if (active[k]) delete active[k]; else active[k] = true;
                    chip.classList.toggle('is-on');
                    apply();
                });
                bar.appendChild(chip);
            });

            var clear = document.createElement('button');
            clear.type = 'button';
            clear.className = 'docs-tool-btn';
            clear.textContent = 'Show all';
            clear.addEventListener('click', function () {
                active = {};
                Array.prototype.forEach.call(bar.querySelectorAll('.filter-chip'), function (c) {
                    c.classList.remove('is-on');
                });
                apply();
            });
            bar.appendChild(clear);
            bar.appendChild(count);

            apply();
        });
    }

    function boot() {
        initDocsShell();
        initChipBar();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }
})();
