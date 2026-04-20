/**
 * Optimized app.js for VPM Listing
 * Handles DOM filtering, VCC URI protocols, Clipboard API, and Listing Hydration.
 */

// 1. Core DOM Hydration (Restored)
const hydratePackages = () => {
    const grid = document.getElementById('packageGrid');
    if (!grid || !window.VPM_PACKAGES) return;

    Object.values(window.VPM_PACKAGES).forEach(pkg => {
        const row = document.createElement('fluent-data-grid-row');
        row.className = 'grid-row';
        row.innerHTML = `
            <fluent-data-grid-cell grid-column="1" class="pkg-info-cell">
                <div class="pkg-header">
                    <h3 class="pkg-title">${pkg.displayName}</h3>
                    <span class="pkg-version">v${pkg.version}</span>
                </div>
                <p class="pkg-desc">${pkg.description}</p>
                <div class="pkg-meta">
                    <code>${pkg.name}</code>
                    <span style="color: #6c5b7b;">|</span>
                    <span>${pkg.author.name}</span>
                </div>
            </fluent-data-grid-cell>
            
            <fluent-data-grid-cell grid-column="2" class="pkg-actions-cell">
                <div class="row-actions">
                    <fluent-button class="info-btn btn-neon-cyan" data-pkg="${pkg.name}" appearance="outline">
                        <svg slot="start" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>
                        Details
                    </fluent-button>
                    <fluent-button class="rowMenuButton btn-neon-pink" data-pkg="${pkg.name}" appearance="stealth">
                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="1"></circle><circle cx="12" cy="5" r="1"></circle><circle cx="12" cy="19" r="1"></circle></svg>
                    </fluent-button>
                </div>
            </fluent-data-grid-cell>
        `;
        grid.appendChild(row);
    });
};

// 2. Standard Utilities
const handleSearch = (event) => {
    const searchTerm = event.target.value.trim().toLowerCase();
    const rows = document.querySelectorAll('fluent-data-grid-row.grid-row');

    rows.forEach(row => {
        const rowText = row.textContent.toLowerCase();
        if (rowText.includes(searchTerm)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
};

const handleCopyToClipboard = async (textToCopy, buttonElement) => {
    if (!textToCopy) return;
    try {
        await navigator.clipboard.writeText(textToCopy);
        const originalHtml = buttonElement.innerHTML;
        buttonElement.innerHTML = `<span style="color: #fff; text-shadow: 0 0 8px #00e5ff;">Copied!</span>`;
        setTimeout(() => {
            buttonElement.innerHTML = originalHtml;
        }, 2000);
    } catch (err) {
        console.error('Failed to copy text: ', err);
    }
};

const handleAddToVCC = (repoUrl) => {
    if (!repoUrl) return;
    const vccUri = `vcc://vpm/addRepo?url=${encodeURIComponent(repoUrl)}`;
    window.location.href = vccUri;
};

// 3. Modal & Menu Management
const showDialog = (id) => {
    const dialog = document.getElementById(id);
    if (dialog) dialog.hidden = false;
};

const hideDialog = (id) => {
    const dialog = document.getElementById(id);
    if (dialog) dialog.hidden = true;
};

let currentMenuPkg = null;
const showContextMenu = (btnEl) => {
    const menu = document.getElementById('rowMoreMenu');
    currentMenuPkg = btnEl.dataset.pkg;

    const rect = btnEl.getBoundingClientRect();
    menu.style.top = `${rect.bottom + window.scrollY + 5}px`;
    menu.style.left = `${rect.left + window.scrollX - 120}px`;
    menu.hidden = false;
};

const hideContextMenu = () => {
    const menu = document.getElementById('rowMoreMenu');
    if (menu) menu.hidden = true;
    currentMenuPkg = null;
};

// 4. Initialization & Event Binding
document.addEventListener('DOMContentLoaded', () => {

    // Crucial step: Build the DOM first!
    hydratePackages();

    // Search Bar
    const searchInput = document.getElementById('searchInput');
    if (searchInput) searchInput.addEventListener('input', handleSearch);

    // Context Menu Actions
    const copyIdBtn = document.getElementById('menuCopyId');
    if (copyIdBtn) {
        copyIdBtn.addEventListener('click', () => {
            if (currentMenuPkg) {
                navigator.clipboard.writeText(currentMenuPkg);
                hideContextMenu();
            }
        });
    }

    const downloadZipBtn = document.getElementById('menuDownloadZip');
    if (downloadZipBtn) {
        downloadZipBtn.addEventListener('click', () => {
            if (currentMenuPkg && window.VPM_PACKAGES[currentMenuPkg]) {
                const url = window.VPM_PACKAGES[currentMenuPkg].url;
                if (url) window.location.href = url;
                hideContextMenu();
            }
        });
    }

    // Modal Close Buttons
    document.querySelectorAll('.close-dialog').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const dialog = e.target.closest('fluent-dialog');
            if (dialog) dialog.hidden = true;
        });
    });

    // Details Buttons (Package Info)
    document.querySelectorAll('.info-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const pkgId = e.currentTarget.dataset.pkg;
            const meta = window.VPM_PACKAGES[pkgId];
            if (!meta) return;

            document.getElementById('modalPkgTitle').textContent = meta.displayName;
            document.getElementById('modalPkgId').textContent = meta.name;

            const authorLink = meta.author.url !== '#'
                ? `<a href="${meta.author.url}" target="_blank" style="color: var(--neon-cyan);">${meta.author.name}</a>`
                : meta.author.name;
            document.getElementById('modalPkgAuthor').innerHTML = authorLink;

            const depsContainer = document.getElementById('modalPkgDeps');
            depsContainer.innerHTML = '';

            if (Object.keys(meta.dependencies).length > 0) {
                Object.entries(meta.dependencies).forEach(([name, version]) => {
                    const li = document.createElement('li');
                    li.className = 'mb-1';
                    li.innerHTML = `<code>${name}</code> @ v${version}`;
                    depsContainer.appendChild(li);
                });
            } else {
                depsContainer.innerHTML = '<li class="mb-1" style="color: #8c73a6;">No external dependencies</li>';
            }

            showDialog('packageInfoModal');
        });
    });

    // Help Menu Trigger
    const helpBtn = document.getElementById('urlBarHelp');
    if (helpBtn) helpBtn.addEventListener('click', () => showDialog('addListingToVccHelp'));

    // Context Menu Triggers
    document.querySelectorAll('.rowMenuButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            showContextMenu(e.currentTarget);
        });
    });

    document.addEventListener('click', (e) => {
        if (!e.target.closest('#rowMoreMenu') && !e.target.closest('.rowMenuButton')) hideContextMenu();
    });

    // Architecture Modal Trigger
    const toolsBreakdownBtn = document.getElementById('openToolsBreakdownBtn');
    if (toolsBreakdownBtn) toolsBreakdownBtn.addEventListener('click', () => showDialog('toolsBreakdownModal'));

    // Changelog Modal Trigger
    const changelogBtn = document.getElementById('openChangelogBtn');
    if (changelogBtn) changelogBtn.addEventListener('click', () => showDialog('changelogModal'));
});