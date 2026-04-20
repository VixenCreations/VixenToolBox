/**
 * Optimized app.js for VPM Listing
 * Handles DOM filtering, VCC URI protocols, and Clipboard API.
 * 100% Pure JavaScript (No Scriban tags in this file).
 */

const handleSearch = (event) => {
    const searchTerm = event.target.value.trim().toLowerCase();
    const rows = document.querySelectorAll('fluent-data-grid-row.grid-row');

    rows.forEach(row => {
        const rowText = row.textContent.toLowerCase();
        row.hidden = !rowText.includes(searchTerm);
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

// --- Modal & Menu Management ---
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
    // Position slightly below and to the left of the button
    menu.style.top = `${rect.bottom + window.scrollY + 5}px`;
    menu.style.left = `${rect.left + window.scrollX - 120}px`;
    menu.hidden = false;
};

const hideContextMenu = () => {
    document.getElementById('rowMoreMenu').hidden = true;
    currentMenuPkg = null;
};

// --- Initialization ---
document.addEventListener('DOMContentLoaded', () => {

    // 1. Search Bar
    const searchInput = document.getElementById('searchInput');
    if (searchInput) searchInput.addEventListener('input', handleSearch);

    // 2. Context Menu Actions
    document.getElementById('menuCopyId')?.addEventListener('click', () => {
        if (currentMenuPkg) {
            navigator.clipboard.writeText(currentMenuPkg);
            hideContextMenu();
        }
    });

    document.getElementById('menuDownloadZip')?.addEventListener('click', () => {
        if (currentMenuPkg && window.VPM_PACKAGES[currentMenuPkg]) {
            const url = window.VPM_PACKAGES[currentMenuPkg].url;
            if (url) window.location.href = url;
            hideContextMenu();
        }
    });

    // 3. Modal Close Buttons
    document.querySelectorAll('.close-dialog').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const dialog = e.target.closest('fluent-dialog');
            if (dialog) dialog.hidden = true;
        });
    });

    // 4. Details Buttons (Package Info)
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

    // 5. Help Menu Trigger
    const helpBtn = document.getElementById('urlBarHelp');
    if (helpBtn) helpBtn.addEventListener('click', () => showDialog('addListingToVccHelp'));

    // 6. Context Menu Triggers
    document.querySelectorAll('.rowMenuButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            showContextMenu(e.currentTarget);
        });
    });

    document.addEventListener('click', (e) => {
        if (!e.target.closest('#rowMoreMenu') && !e.target.closest('.rowMenuButton')) hideContextMenu();
    });

    // 7. Architecture Modal Trigger
    const toolsBreakdownBtn = document.getElementById('openToolsBreakdownBtn');
    if (toolsBreakdownBtn) toolsBreakdownBtn.addEventListener('click', () => showDialog('toolsBreakdownModal'));

    // 8. Changelog Modal Trigger (NEW)
    const changelogBtn = document.getElementById('openChangelogBtn');
    if (changelogBtn) changelogBtn.addEventListener('click', () => showDialog('changelogModal'));
});