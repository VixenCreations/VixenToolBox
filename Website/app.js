/**
 * Optimized app.js for VPM Listing
 * Uses DOM-based filtering to preserve server-side HTML templates.
 */

import { baseLayerLuminance, StandardLuminance } from "https://unpkg.com/@microsoft/fast-components";

// State management
const state = {
    selectedPackage: null,
    contextMenuRowId: null,
};

// Enforce Dark Mode for the Cyberpunk Aesthetic
const setTheme = () => {
    try {
        baseLayerLuminance.setValueFor(document.documentElement, StandardLuminance.DarkMode);
    } catch (e) {
        console.warn("FAST components not loaded.", e);
    }
};

/**
 * High-performance DOM filter for the search bar
 */
const handleSearch = (event) => {
    const searchTerm = event.target.value.trim().toLowerCase();

    // Select all rows that are NOT the header
    const rows = document.querySelectorAll('fluent-data-grid-row.grid-row');

    rows.forEach(row => {
        // Grab the text content of the entire row (Name, Description, Author, etc.)
        const rowText = row.textContent.toLowerCase();

        if (rowText.includes(searchTerm)) {
            row.hidden = false;
        } else {
            row.hidden = true;
        }
    });
};

// Dialog Helpers
const showDialog = (dialogId, pkg = null) => {
    const dialog = document.getElementById(dialogId);
    if (!dialog) return;
    dialog.hidden = false;
};

const hideDialog = (dialogId) => {
    const dialog = document.getElementById(dialogId);
    if (!dialog) return;
    dialog.hidden = true;
};

// Context Menu Helpers
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

// Lifecycle Hooks
(() => {
    setTheme();
})();

document.addEventListener("DOMContentLoaded", () => {
    const searchInput = document.getElementById("searchInput");

    if (searchInput) {
        searchInput.addEventListener("input", handleSearch);
    }

    // Modal Close Buttons
    document.querySelectorAll('.close-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            hideDialog(e.target.closest('fluent-dialog').id);
        });
    });

    // Info Buttons
    document.querySelectorAll('.rowPackageInfoButton').forEach(btn => {
        btn.addEventListener('click', () => {
            showDialog('packageInfoModal');
        });
    });

    // Help Buttons
    const helpBtn = document.getElementById('urlBarHelp');
    if (helpBtn) {
        helpBtn.addEventListener('click', () => {
            showDialog('addListingToVccHelp');
        });
    }

    // Row Menu Buttons (...)
    document.querySelectorAll('.rowMenuButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            showContextMenu(e.currentTarget);
        });
    });

    // Hide context menu if clicking outside
    document.addEventListener('click', (e) => {
        if (!e.target.closest('#rowMoreMenu') && !e.target.closest('.rowMenuButton')) {
            hideContextMenu();
        }
    });
});