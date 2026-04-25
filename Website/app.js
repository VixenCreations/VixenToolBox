document.addEventListener('DOMContentLoaded', () => {
    // --- Utility: Copy to Clipboard ---
    const handleCopyToClipboard = async (textToCopy, buttonElement) => {
        if (!textToCopy) return;
        try {
            await navigator.clipboard.writeText(textToCopy);
            const originalHtml = buttonElement.innerHTML;
            buttonElement.innerHTML = `<span style="color: var(--neon-cyan); text-shadow: 0 0 8px var(--neon-cyan-glow); font-weight: bold;">Copied!</span>`;
            setTimeout(() => {
                buttonElement.innerHTML = originalHtml;
            }, 2000);
        } catch (err) {
            console.error('Failed to copy text: ', err);
        }
    };

    // --- Utility: Add to VCC ---
    const handleAddToVCC = (repoUrl) => {
        if (!repoUrl) return;
        window.location.href = `vcc://vpm/addRepo?url=${encodeURIComponent(repoUrl)}`;
    };

    // --- Modal Management ---
    const showDialog = (id) => {
        const dialog = document.getElementById(id);
        if (dialog) dialog.hidden = false;
    };

    const hideDialog = (id) => {
        const dialog = document.getElementById(id);
        if (dialog) dialog.hidden = true;
    };

    // 1. Search Functionality
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            const term = e.target.value.toLowerCase();
            document.querySelectorAll('.grid-row').forEach(row => {
                const name = row.getAttribute('data-package-name')?.toLowerCase() || '';
                const id = row.getAttribute('data-package-id')?.toLowerCase() || '';
                if (name.includes(term) || id.includes(term)) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    }

    // 2. Global Actions (Header)
    const vccUrlField = document.getElementById('vccUrlField');

    document.getElementById('vccUrlFieldCopy')?.addEventListener('click', function () {
        handleCopyToClipboard(vccUrlField?.value, this);
    });

    document.getElementById('vccAddRepoButton')?.addEventListener('click', () => {
        handleAddToVCC(vccUrlField?.value);
    });

    document.getElementById('urlBarHelp')?.addEventListener('click', () => showDialog('addListingToVccHelp'));
    document.getElementById('addListingToVccHelpClose')?.addEventListener('click', () => hideDialog('addListingToVccHelp'));

    // 3. Middle Section Modals
    document.getElementById('openToolsBreakdownBtn')?.addEventListener('click', () => showDialog('toolsBreakdownModal'));
    document.getElementById('toolsBreakdownModalClose')?.addEventListener('click', () => hideDialog('toolsBreakdownModal'));

    document.getElementById('openChangelogBtn')?.addEventListener('click', () => showDialog('changelogModal'));
    document.getElementById('changelogModalClose')?.addEventListener('click', () => hideDialog('changelogModal'));

    // 4. Package Grid Interactions
    // Add Buttons
    document.querySelectorAll('.rowAddToVccButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const pkgId = e.currentTarget.getAttribute('data-package-id');
            // Usually, adding a specific package still just adds the repo, or uses a specific VCC protocol if supported.
            // Defaulting to adding the main repo url for safety.
            handleAddToVCC(vccUrlField?.value);
        });
    });

    // Info Buttons
    document.querySelectorAll('.rowPackageInfoButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const pkgId = e.currentTarget.getAttribute('data-package-id');
            const pkgData = window.VPM_PACKAGES?.[pkgId];
            if (!pkgData) return;

            document.getElementById('packageInfoName').textContent = pkgData.displayName;
            document.getElementById('packageInfoId').textContent = pkgData.name;
            document.getElementById('packageInfoVersion').textContent = `v${pkgData.version}`;
            document.getElementById('packageInfoDescription').textContent = pkgData.description;

            const authorEl = document.getElementById('packageInfoAuthor');
            authorEl.textContent = pkgData.author.name;
            authorEl.href = pkgData.author.url !== '#' ? pkgData.author.url : 'javascript:void(0)';

            document.getElementById('packageInfoLicense').textContent = pkgData.license;

            const depsList = document.getElementById('packageInfoDependencies');
            depsList.innerHTML = '';
            if (pkgData.dependencies && Object.keys(pkgData.dependencies).length > 0) {
                Object.entries(pkgData.dependencies).forEach(([depName, depVer]) => {
                    const li = document.createElement('li');
                    li.className = 'mb-1';
                    li.innerHTML = `<code>${depName}</code> @ ${depVer}`;
                    depsList.appendChild(li);
                });
            } else {
                depsList.innerHTML = '<li class="mb-1" style="color: #8c73a6;">None</li>';
            }

            showDialog('packageInfoModal');
        });
    });

    document.getElementById('packageInfoModalClose')?.addEventListener('click', () => hideDialog('packageInfoModal'));
    document.getElementById('packageInfoListingHelp')?.addEventListener('click', () => {
        // Gracefully dismiss the active matrix before spawning the secondary help dialog
        hideDialog('packageInfoModal');
        showDialog('addListingToVccHelp');
    });

    document.getElementById('packageInfoVccUrlFieldCopy')?.addEventListener('click', function () {
        const url = document.getElementById('packageInfoVccUrlField')?.value;
        handleCopyToClipboard(url, this);
    });

    // Modal Copy Buttons inside Add to VCC Help
    document.getElementById('vccListingInfoUrlFieldCopy')?.addEventListener('click', function () {
        const url = document.getElementById('vccListingInfoUrlField')?.value;
        handleCopyToClipboard(url, this);
    });

    // 5. Row More Menu (Download Zip)
    const rowMenu = document.getElementById('rowMoreMenu');
    let currentZipUrl = '';

    document.querySelectorAll('.rowMenuButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            currentZipUrl = e.currentTarget.getAttribute('data-package-url');

            const rect = e.currentTarget.getBoundingClientRect();
            rowMenu.style.top = `${rect.bottom + window.scrollY + 5}px`;
            rowMenu.style.left = `${rect.left + window.scrollX - 140}px`;
            rowMenu.hidden = false;
        });
    });

    document.getElementById('rowMoreMenuDownload')?.addEventListener('click', () => {
        if (currentZipUrl) window.location.href = currentZipUrl;
        rowMenu.hidden = true;
    });

    // Close menu when clicking outside
    document.addEventListener('click', (e) => {
        if (!e.target.closest('#rowMoreMenu') && !e.target.closest('.rowMenuButton')) {
            if (rowMenu) rowMenu.hidden = true;
        }
    });
});