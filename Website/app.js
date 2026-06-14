document.addEventListener('DOMContentLoaded', () => {
    // --- FOUC Guard: reveal the page once Fluent UI is hydrated ---
    // styles.css cloaks <body> until .app-ready is added. We wait for the
    // critical fluent custom elements to register, then flip the class.
    // A 2s safety timer guarantees the page never stays hidden if the
    // unpkg module fails (offline, CDN down, etc).
    let revealed = false;
    const revealPage = () => {
        if (revealed) return;
        revealed = true;
        document.body.classList.add('app-ready');
    };
    const fluentTags = ['fluent-anchor', 'fluent-button', 'fluent-select', 'fluent-option'];
    Promise.all(fluentTags.map(tag => customElements.whenDefined(tag)))
        .then(revealPage)
        .catch(revealPage);
    setTimeout(revealPage, 2000);

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

    // 4. Package Grid Interactions
    // Add Buttons
    document.querySelectorAll('.rowAddToVccButton').forEach(btn => {
        btn.addEventListener('click', (e) => {
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
        hideDialog('packageInfoModal');
        showDialog('addListingToVccHelp');
    });

    document.getElementById('packageInfoVccUrlFieldCopy')?.addEventListener('click', function () {
        const url = document.getElementById('packageInfoVccUrlField')?.value;
        handleCopyToClipboard(url, this);
    });

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

    // 6. Lightbox System
    const lightbox = document.getElementById('lightboxOverlay');
    const lightboxImg = document.getElementById('lightboxImg');
    const lightboxClose = document.getElementById('lightboxClose');

    if (lightbox && lightboxImg && lightboxClose) {
        // Pending src-clear timer. Tracked so a rapid reopen can cancel it
        // before it nukes the freshly-set image src.
        let lightboxClearTimer = null;

        // Open Lightbox when an image with .preview-trigger is clicked
        document.querySelectorAll('.preview-trigger').forEach(img => {
            img.addEventListener('click', (e) => {
                if (lightboxClearTimer) {
                    clearTimeout(lightboxClearTimer);
                    lightboxClearTimer = null;
                }
                lightboxImg.src = e.target.src;
                lightbox.classList.add('active');
            });
        });

        // Function to close Lightbox gracefully
        const closeLightbox = () => {
            lightbox.classList.remove('active');
            // Clear the src after the CSS fade transition completes (300ms)
            if (lightboxClearTimer) clearTimeout(lightboxClearTimer);
            lightboxClearTimer = setTimeout(() => {
                lightboxImg.src = '';
                lightboxClearTimer = null;
            }, 300);
        };

        // Close via X button
        lightboxClose.addEventListener('click', closeLightbox);

        // Close via clicking the blurred background
        lightbox.addEventListener('click', (e) => {
            if (e.target === lightbox) {
                closeLightbox();
            }
        });

        // Close via Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && lightbox.classList.contains('active')) {
                closeLightbox();
            }
        });
    }

    // 7. Changelog Pagination System
    const changelogEntries = Array.from(document.querySelectorAll('.changelog-entry'));
    const versionSelect = document.getElementById('versionSelect');
    const prevVersionBtn = document.getElementById('prevVersionBtn');
    const nextVersionBtn = document.getElementById('nextVersionBtn');

    if (changelogEntries.length > 0 && versionSelect && prevVersionBtn && nextVersionBtn) {
        let currentVersionIndex = 0; // 0 = Newest Version

        const updateChangelogView = (index) => {
            // Hide all, show targeted
            changelogEntries.forEach((entry, i) => {
                if (i === index) {
                    entry.hidden = false;
                    // Trigger CSS reflow to replay the fade-in animation
                    entry.classList.remove('changelog-fade');
                    void entry.offsetWidth;
                    entry.classList.add('changelog-fade');
                } else {
                    entry.hidden = true;
                }
            });

            // Sync the dropdown menu value
            versionSelect.value = changelogEntries[index].getAttribute('data-version');

            // Manage Button States (Disabled if at the edge of the array)
            if (index === 0) {
                prevVersionBtn.setAttribute('disabled', 'true');
            } else {
                prevVersionBtn.removeAttribute('disabled');
            }

            if (index === changelogEntries.length - 1) {
                nextVersionBtn.setAttribute('disabled', 'true');
            } else {
                nextVersionBtn.removeAttribute('disabled');
            }
        };

        // Event: Dropdown Selection Changed
        versionSelect.addEventListener('change', (e) => {
            const targetVersion = e.target.value;
            const targetIndex = changelogEntries.findIndex(entry => entry.getAttribute('data-version') === targetVersion);

            if (targetIndex !== -1) {
                currentVersionIndex = targetIndex;
                updateChangelogView(currentVersionIndex);
            }
        });

        // Event: Clicked 'Newer Updates'
        prevVersionBtn.addEventListener('click', () => {
            if (currentVersionIndex > 0) {
                currentVersionIndex--;
                updateChangelogView(currentVersionIndex);
            }
        });

        // Event: Clicked 'Older Updates'
        nextVersionBtn.addEventListener('click', () => {
            if (currentVersionIndex < changelogEntries.length - 1) {
                currentVersionIndex++;
                updateChangelogView(currentVersionIndex);
            }
        });

        // Initialize view on load
        updateChangelogView(0);
    }

    // 8. Jump to Top Button Logic
    const jumpToTopBtn = document.getElementById('jumpToTopBtn');

    if (jumpToTopBtn) {
        // Monitor scroll depth
        window.addEventListener('scroll', () => {
            if (window.scrollY > 200) {
                jumpToTopBtn.classList.add('visible');
            } else {
                jumpToTopBtn.classList.remove('visible');
            }
        });

        // Smooth scroll to top on click
        jumpToTopBtn.addEventListener('click', () => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
    }
});