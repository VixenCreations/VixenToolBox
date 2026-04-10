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

const getFieldValue = (fieldId) => {
    const field = document.getElementById(fieldId);
    if (!field) return '';
    return field.value || field.getAttribute('value') || '';
};

const showDialog = (dialogId) => {
    const dialog = document.getElementById(dialogId);
    if (dialog) dialog.hidden = false;
};

const hideDialog = (dialogId) => {
    const dialog = document.getElementById(dialogId);
    if (dialog) dialog.hidden = true;
};

const showContextMenu = (anchorElement) => {
    const menu = document.getElementById('rowMoreMenu');
    if (!menu || !anchorElement) return;

    const rect = anchorElement.getBoundingClientRect();
    menu.style.top = `${rect.bottom + window.scrollY}px`;
    menu.style.left = `${rect.left + window.scrollX}px`;
    menu.hidden = false;
};

const hideContextMenu = () => {
    const menu = document.getElementById('rowMoreMenu');
    if (menu) menu.hidden = true;
};

document.addEventListener("DOMContentLoaded", () => {

    // 1. Search Bar
    const searchInput = document.getElementById("searchInput");
    if (searchInput) searchInput.addEventListener("input", handleSearch);

    // 2. Add to VCC Action Buttons
    document.getElementById('vccAddRepoButton')?.addEventListener('click', () => {
        handleAddToVCC(getFieldValue('vccUrlField'));
    });

    document.querySelectorAll('.rowAddToVccButton').forEach(btn => {
        btn.addEventListener('click', () => {
            handleAddToVCC(getFieldValue('vccUrlField'));
        });
    });

    // 3. Copy to Clipboard Buttons
    document.getElementById('vccUrlFieldCopy')?.addEventListener('click', (e) => {
        handleCopyToClipboard(getFieldValue('vccUrlField'), e.currentTarget);
    });

    document.getElementById('vccListingInfoUrlFieldCopy')?.addEventListener('click', (e) => {
        handleCopyToClipboard(getFieldValue('vccListingInfoUrlField'), e.currentTarget);
    });

    document.getElementById('packageInfoVccUrlFieldCopy')?.addEventListener('click', (e) => {
        handleCopyToClipboard(getFieldValue('packageInfoVccUrlField'), e.currentTarget);
    });

    // 4. Close Modal Triggers
    document.querySelectorAll('.close-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            hideDialog(e.target.closest('fluent-dialog').id);
        });
    });

    // 5. PACKAGE INFO MODAL (Reads from the clean window object in index.html)
    document.querySelectorAll('.rowPackageInfoButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const pkgId = e.currentTarget.dataset.packageId;

            // Pull the data from the global object we generated in index.html
            const meta = window.VPM_PACKAGES[pkgId];
            if (!meta) return;

            document.getElementById('packageInfoName').textContent = meta.displayName;
            document.getElementById('packageInfoId').textContent = meta.name;
            document.getElementById('packageInfoVersion').textContent = 'v' + meta.version;
            document.getElementById('packageInfoDescription').textContent = meta.description;

            const authorEl = document.getElementById('packageInfoAuthor');
            authorEl.textContent = meta.author.name;
            authorEl.href = meta.author.url !== '#' ? meta.author.url : 'javascript:void(0)';

            document.getElementById('packageInfoLicense').textContent = meta.license;

            const depsContainer = document.getElementById('packageInfoDependencies');
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

    // 6. Help Menu Trigger
    const helpBtn = document.getElementById('urlBarHelp');
    if (helpBtn) helpBtn.addEventListener('click', () => showDialog('addListingToVccHelp'));

    // 7. Context Menu Triggers
    document.querySelectorAll('.rowMenuButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            showContextMenu(e.currentTarget);
        });
    });

    document.addEventListener('click', (e) => {
        if (!e.target.closest('#rowMoreMenu') && !e.target.closest('.rowMenuButton')) hideContextMenu();
    });
});