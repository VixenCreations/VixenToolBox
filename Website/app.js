/**
 * Optimized app.js for VPM Listing
 * Handles DOM filtering, Data-Bridge Modal Injection, VCC URI protocols, and Clipboard API.
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
    if (!textToCopy) return;
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

// --- Helper: Safely grab value from Fluent UI Web Components ---
const getFieldValue = (fieldId) => {
    const field = document.getElementById(fieldId);
    if (!field) return '';
    return field.value || field.getAttribute('value') || '';
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

    // 5. PACKAGE INFO MODAL INJECTION (The Data-Bridge Fix)
    document.querySelectorAll('.rowPackageInfoButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            // Find the parent row and grab our hidden data
            const row = e.currentTarget.closest('.grid-row');
            const meta = row.querySelector('.pkg-meta').dataset;
            const depsHtml = row.querySelector('.pkg-deps').innerHTML;

            // Inject the data into the modal UI
            document.getElementById('packageInfoName').textContent = meta.name;
            document.getElementById('packageInfoId').textContent = meta.id;
            document.getElementById('packageInfoVersion').textContent = 'v' + meta.version;
            document.getElementById('packageInfoDescription').textContent = meta.desc;

            const authorEl = document.getElementById('packageInfoAuthor');
            authorEl.textContent = meta.author;
            authorEl.href = meta.authorUrl !== '#' ? meta.authorUrl : 'javascript:void(0)';

            document.getElementById('packageInfoLicense').textContent = meta.license;
            document.getElementById('packageInfoDependencies').innerHTML = depsHtml;

            // Show the perfectly populated modal
            showDialog('packageInfoModal');
        });
    });

    // 6. Help Menu Trigger
    const helpBtn = document.getElementById('urlBarHelp');
    if (helpBtn) {
        helpBtn.addEventListener('click', () => showDialog('addListingToVccHelp'));
    }

    // 7. Context Menu Triggers
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