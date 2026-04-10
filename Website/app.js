/**
 * Optimized app.js for VPM Listing
 * Handles DOM filtering, VCC URI protocols, and Clipboard API.
 */

// State management
const state = {
    selectedPackage: null,
    contextMenuRowId: null,
};

/**
 * High-performance DOM filter for the search bar
 */
const handleSearch = (event) => {
    const searchTerm = event.target.value.trim().toLowerCase();
    const rows = document.querySelectorAll('fluent-data-grid-row.grid-row');

    rows.forEach(row => {
        const rowText = row.textContent.toLowerCase();
        row.hidden = !rowText.includes(searchTerm);
    });
};

/**
 * Copies target text to the system clipboard and provides visual feedback
 */
const handleCopyToClipboard = async (textToCopy, buttonElement) => {
    try {
        await navigator.clipboard.writeText(textToCopy);

        // Brief visual feedback on the button
        const originalHtml = buttonElement.innerHTML;
        buttonElement.innerHTML = `<span style="color: #fff; text-shadow: 0 0 8px #00e5ff;">Copied!</span>`;
        setTimeout(() => {
            buttonElement.innerHTML = originalHtml;
        }, 2000);
    } catch (err) {
        console.error('Failed to copy text: ', err);
    }
};

/**
 * Executes the VCC Protocol to add the repository
 */
const handleAddToVCC = (repoUrl) => {
    if (!repoUrl) return;
    const vccUri = `vcc://vpm/addRepo?url=${encodeURIComponent(repoUrl)}`;
    window.location.href = vccUri;
};

// --- Dialog & Menu Helpers ---
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

// --- Initialization & Event Listeners ---
document.addEventListener("DOMContentLoaded", () => {

    // 1. Search Bar
    const searchInput = document.getElementById("searchInput");
    if (searchInput) {
        searchInput.addEventListener("input", handleSearch);
    }

    // 2. Fetch the Master VPM Repository URL from the DOM
    const mainRepoUrl = document.getElementById('vccUrlField')?.value;

    // 3. Add to VCC Action Buttons
    document.getElementById('vccAddRepoButton')?.addEventListener('click', () => {
        handleAddToVCC(mainRepoUrl);
    });

    // Add to VCC buttons on individual package rows
    // Note: VPM architecture links the entire *listing/repo*, not an isolated package.
    document.querySelectorAll('.rowAddToVccButton').forEach(btn => {
        btn.addEventListener('click', () => {
            handleAddToVCC(mainRepoUrl);
        });
    });

    // 4. Copy to Clipboard Buttons
    document.getElementById('vccUrlFieldCopy')?.addEventListener('click', (e) => {
        handleCopyToClipboard(mainRepoUrl, e.currentTarget);
    });

    document.getElementById('vccListingInfoUrlFieldCopy')?.addEventListener('click', (e) => {
        const url = document.getElementById('vccListingInfoUrlField')?.value;
        handleCopyToClipboard(url, e.currentTarget);
    });

    document.getElementById('packageInfoVccUrlFieldCopy')?.addEventListener('click', (e) => {
        const url = document.getElementById('packageInfoVccUrlField')?.value;
        handleCopyToClipboard(url, e.currentTarget);
    });

    // 5. Modal & Menu Triggers
    document.querySelectorAll('.close-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            hideDialog(e.target.closest('fluent-dialog').id);
        });
    });

    document.querySelectorAll('.rowPackageInfoButton').forEach(btn => {
        btn.addEventListener('click', () => {
            showDialog('packageInfoModal');
        });
    });

    const helpBtn = document.getElementById('urlBarHelp');
    if (helpBtn) {
        helpBtn.addEventListener('click', () => showDialog('addListingToVccHelp'));
    }

    document.querySelectorAll('.rowMenuButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            showContextMenu(e.currentTarget);
        });
    });

    // Hide context menu on outside click
    document.addEventListener('click', (e) => {
        if (!e.target.closest('#rowMoreMenu') && !e.target.closest('.rowMenuButton')) {
            hideContextMenu();
        }
    });
});