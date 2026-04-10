/**
 * Full app.js - Optimized for VPM Listing with Enforced Dark Mode.
 * This script handles data binding, search, sorting, filtering, 
 * and UI interactions for the VRChat Package Manager interface.
 *
 * NOTE: Ensure you are importing the following from 
 * @fluentui/web-components and @microsoft/fast-components
 * in your build pipeline or HTML header before this script runs.
 */

// Example necessary imports (adjust based on your build setup):
// import { provideFluentDesignSystem, fluentDataGrid, fluentDataGridCell, fluentDataGridRow, fluentDialog, fluentSearch, fluentButton, fluentTextField } from "@fluentui/web-components";
// import { baseLayerLuminance, StandardLuminance } from "@microsoft/fast-components";

let packageGrid = null;
let allPackages = [];
let searchInput = null;
let currentSearchTerm = "";

// State management for UI elements
const state = {
    selectedPackage: null,
    contextMenuRowId: null,
};

/**
 * 1. ENFORCE DARK MODE: This is crucial for the signature aesthetic.
 * Locks the environment to preserve the high-gloss/neon cyberpunk look.
 */
const setTheme = () => {
    try {
        // Enforce Dark Mode: Disregard user OS preference.
        // This makes 'baseLayerLuminance' low (dark), forcing light text
        // and vibrant, glowing accents, matching the scene lighting.
        baseLayerLuminance.setValueFor(document.documentElement, StandardLuminance.DarkMode);
        console.log("Visual style: Signature Dark Mode locked.");
    } catch (e) {
        console.warn("Theme management not available. Ensure FAST/Fluent components are loaded.", e);
    }
};

/**
 * Loads and binds the data to the main package list.
 */
const bindDataGrid = (data) => {
    if (!packageGrid) return;

    // Simple sort example (by Name ascending)
    const sortedData = [...data].sort((a, b) => a.name.localeCompare(b.name));

    // Bind data to the fluentDataGrid rowSource
    packageGrid.rowsData = sortedData;
};

/**
 * Handles search/filtering logic.
 */
const handleSearch = (event) => {
    currentSearchTerm = event.target.value.trim().toLowerCase();
    filterAndBindData();
};

/**
 * Filters the master package list based on current state (search, etc.)
 * and then updates the UI.
 */
const filterAndBindData = () => {
    if (currentSearchTerm === "") {
        bindDataGrid(allPackages);
        return;
    }

    const filteredData = allPackages.filter(pkg => {
        return (
            pkg.name.toLowerCase().includes(currentSearchTerm) ||
            pkg.description.toLowerCase().includes(currentSearchTerm) ||
            (pkg.author && pkg.author.toLowerCase().includes(currentSearchTerm))
        );
    });

    bindDataGrid(filteredData);
};

// --- Dialog / UI Helpers ---

const showDialog = (dialogId, pkg = null) => {
    const dialog = document.getElementById(dialogId);
    if (!dialog) return;

    state.selectedPackage = pkg;

    // Optional: Populate dynamic content if needed, e.g., Package Info
    if (pkg && dialogId === 'packageInfoModal') {
        dialog.querySelector('h3[slot="title"]').innerText = pkg.name;
        // ... (populate descriptions, versions, links, etc.)
    }

    dialog.hidden = false;
};

const hideDialog = (dialogId) => {
    const dialog = document.getElementById(dialogId);
    if (!dialog) return;
    dialog.hidden = true;
    state.selectedPackage = null;
};

// --- Context Menu / More Menu ---

const showContextMenu = (rowId, anchorElement) => {
    const menu = document.getElementById('rowMoreMenu');
    if (!menu || !anchorElement) return;

    state.contextMenuRowId = rowId;

    // Simple absolute positioning (implement better alignment logic as needed)
    const rect = anchorElement.getBoundingClientRect();
    menu.style.top = `${rect.bottom + window.scrollY}px`;
    menu.style.left = `${rect.left + window.scrollX}px`;

    menu.hidden = false;
};

const hideContextMenu = () => {
    const menu = document.getElementById('rowMoreMenu');
    if (menu) menu.hidden = true;
    state.contextMenuRowId = null;
};


// --- Core Lifecycle Hooks ---

// 2. Execute theme enforcement as early as possible.
(() => {
    setTheme();
})();

// 3. Initialize components and event listeners on DOM Ready.
document.addEventListener("DOMContentLoaded", async () => {
    // A. Query core UI elements
    packageGrid = document.getElementById("packageGrid");
    searchInput = document.getElementById("searchInput");

    // B. Register dynamic cell template (Fluent DataGrid standard)
    // In this listing structure, we are overriding simple text display for customization
    try {
        if (typeof fluentDataGridCell !== 'undefined' && fluentDataGridCell.definition) {
            fluentDataGridCell.definition.register(packageGrid);
        }
    } catch (e) {
        console.warn("Grid customization not initialized. Ensure required FAST components are loaded.", e);
    }

    // C. Initialize Event Listeners
    if (searchInput) {
        searchInput.addEventListener("input", handleSearch);
    }

    // Modal Close Buttons (assuming buttons exist with `data-dismiss="modal"` or similar)
    document.querySelectorAll('[data-dismiss="dialog"]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            hideDialog(e.target.closest('fluent-dialog').id);
        });
    });

    // Handle "Help" button action (show instruction dialog)
    const helpBtn = document.getElementById('addListingHelpBtn');
    if (helpBtn) {
        helpBtn.addEventListener('click', () => {
            showDialog('addListingToVccHelp');
        });
    }

    // Hide context menu if clicking outside
    document.addEventListener('click', (e) => {
        if (!e.target.closest('#rowMoreMenu') && !e.target.closest('.row-action-btn')) {
            hideContextMenu();
        }
    });

    // D. Fetch and Bind Data
    // Replace with your actual data source (e.g., fetch('./packages.json'))
    try {
        console.log("Fetching packages list...");
        // const response = await fetch('./packages.json');
        // allPackages = await response.json();

        // SAMPLE DATA for demonstration:
        allPackages = [
            { id: "pkg.cyan.suite.core", name: "Cyan Suite Core", version: "1.0.2", author: "VPM Dev Team", description: "Base components and shader libraries optimized for the atmospheric listing aesthetic." },
            { id: "pkg.magenta.avatars.basic", name: "Magenta Avatar Prefabs", version: "0.9.1", author: "Novabeast Labs", description: "Starter prefabs for the Novabeast avatar, including pre-configured lighting setups." },
            { id: "pkg.glow.vinyl.textures", name: "Glowing Vinyl Texture Pack", version: "2.1.0", author: "VisualFX Works", description: "High-gloss PBR textures featuring integrated emissive channels (cyan and pink edge highlights)." },
        ];

        bindDataGrid(allPackages);
        console.log("VPM listing initialized.");

    } catch (error) {
        console.error("Error initializing package list:", error);
        // Implement user-facing error state in UI
    }
});