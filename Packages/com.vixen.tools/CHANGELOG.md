***

# VixenTools - Changelog

All notable changes to the VixenToolBox project will be documented in this file.

***

## [1.4.0] - 2026-04-24
### Added
- **Enterprise UI Toolkit Architecture:** Completely eradicated legacy IMGUI constraints across the entire ecosystem. Vixen Hub, Badge Studio, Animation Workbench Pro, Quest Conversion Engine, PhysBone Topology Mapper, and Preset Manager now operate natively on Unity's high-performance UI Toolkit DOM tree.
- **Centralized Styling Matrix:** Established a master `UiStyles` repository (`Packages/.../Editor/UiStyles/`) to house the `Cyberpunk-Regular.ttf` font and all `.uss` CSS design tokens. This guarantees absolute visual consistency across all windows and provides a single-point maintenance hub for future UI scaling.
- **Dynamic Flex Layouts:** Engineered highly responsive, flex-wrapping grid architectures for data-heavy tools (Animation Workbench, Preset Manager). The interfaces now mathematically scale and stack panels gracefully upon window resize, completely eliminating clipped text and overlapping bounds.
- **Dynamic DOM Bridge (Quest Engine & Badge Studio):** Replaced the volatile `OnGUI` redraw loops with persistent UI Toolkit state injectors. Interfaces now seamlessly sync variable states and spawn interactive matrices instantly without triggering Editor-wide lag spikes.
- **Dynamic JSON Layout Engine (Badge Studio):** Replaced hardcoded badge layout coordinates with a persistent `layout.json` system. The tool now automatically saves and loads advanced UV bounds, text rotations, and neon hex overrides directly into custom template directories.
- **Furality Layout Generator:** Built a dedicated developer utility natively integrated into Badge Studio to autonomously scaffold directories and format perfect `layout.json` boundary configurations for all Furality convention templates.
- **Autonomous Scene View HUD Notifier:** Eradicated the workflow-breaking update popup window. The engine now silently detects version discrepancies and injects a sleek, non-blocking, neon-accented `>> VIXENTOOLS UPDATE` button directly into the corner of the active Unity Scene View.
- **Live Scene UV Mapper (Badge Studio):** Integrated a 3D coordinate mapper that temporarily suspends Unity's default gizmos (`Tools.current = Tool.None`) to hijack Scene View clicks. It raycasts against the badge mesh, mathematically inverts UVs to ImageMagick pixel space, and draws a neon pink debug indicator at the exact intersection point.

### Changed
- **Magick.NET Font Engine Upgrade (Badge Studio):** Ripped out unstable, Windows-only `Gdi32.dll` OS-level font installation hacks. Migrated to modern Magick.NET direct file pathing (`@font.ttf`), ensuring cross-platform stability and significantly faster text-plate rendering.
- **Cyber-Noir Aesthetic Standardization:** Unified the visual language across the entire VixenTools suite. Deployed tinted glass `.cyber-panel` wrappers, high-contrast neon headers, and massive, deeply saturated action buttons (`.cyan-btn`, `.pink-btn`) to clearly delineate execution phases.
- **Markdown Engine Sanitization:** Upgraded the Hub's internal Markdown parser to natively generate `VisualElements`. Actively stripped all volatile unicode emojis—which inherently conflict with Unity's TextCore rendering engine—and replaced them with stable, terminal-compliant chevron syntax (`>>`, `::`, `>`).
- **Graceful Degradation States:** Overhauled error-handling UIs (such as missing VRChat SDK warnings) to utilize the new enterprise styling, deploying amber `.warning-box-styled` elements to keep the interface looking premium even in failure states.
- **Skinned Mesh Raycasting (UV Mapper):** The mapping engine now automatically detects and bakes `SkinnedMeshRenderer` data into a static `MeshCollider`, allowing accurate raycasting on dynamically weighted Furality attendee badges.

### Fixed
- **Ecosystem Scanner Bleed-Over:** Fixed a directory routing bug where the VixenTools source network was blindly indexing nested Furality template assets. Injected strict exclusion filters so Furality assets correctly populate via the dedicated SDK parser instead of crowding the custom template dropdown.
- **UI Toolkit Constraint Clipping:** Addressed a layout violation where nested flex-containers would clip their content. Mathematically adjusted root `min-width` parameters across the matrix and injected `white-space: normal;` into info-box CSS to ensure text descriptions properly wrap and stack horizontally.
- **CSS Prefix Validation:** Sanitized the newly migrated `.uss` files to strictly adhere to Unity's TextCore engine, replacing standard CSS tags with required `-unity-` prefixes to eliminate console UI warnings.
- **Raycast Piercing (UV Mapper):** Upgraded the mapper to use `Physics.RaycastAll()` with `MouseDrag` support. The tool now successfully pierces through invisible avatar capsule colliders that were previously eating `MouseDown` events and blocking the raycast.

## [1.3.1] - 2026-04-23
### Added
- **Autonomous Update Prompter:** The Vixen Hub now features an `[InitializeOnLoadMethod]` that passively reads the VPM `package.json` in the background during Unity compilation. If it detects a version discrepancy against the local `VersionIs.asset`, it automatically opens the Hub to immediately display the latest release notes.
- **Extended Hub Architecture:** Expanded the Vixen Hub into a 5-tab control matrix (Documentation, Changelog, Donation, Social Media, Get Support). Added an internal Markdown parser for the new `SUPPORT.md` file and integrated stylized action buttons for external ecosystem routing.
- **VixenGitListener (CI/CD Telemetry):** Engineered a fully isolated, asynchronous Discord webhook bot (`aiohttp`). It utilizes an outbound Server-Sent Events (SSE) stream via Smee to intercept GitHub payloads natively, bypassing the need for port-forwarding and keeping the host IP 100% invisible.
- **Deep Payload Parsing:** The Git listener now calculates exact file architecture changes (added, modified, removed) per commit and renders them into the Discord channel using cyber-terminal branch styling. It also intercepts and blockquotes commit and issue comments directly into the matrix.
- **Heuristic AutoMod Matrix:** Deployed a battle-tested Rust regex engine to the community Discord. It is aggressively tuned to intercept and nuke VRChat developer-targeted RATs (disguised as `.rar`/`.zip` game tests), typosquatted domains, and fake Steam login phishing attempts.

### Changed
- **Triage Routing:** The Hub's "Get Support" tab has been overhauled to inject a direct invite to the VixenTools community Discord, establishing a streamlined funnel for live pipeline troubleshooting and architectural upgrades.

### Fixed
- **Gateway Race Condition (Git Listener):** Injected an `await self.wait_until_ready()` execution lock into the webhook background task. This prevents the Smee SSE stream from prematurely intercepting GitHub payloads before the bot has successfully cached the Discord server's channel architecture.

## [1.3.0] - 2026-04-21
### Added
- **Quest Conversion Engine:** Engineered a fully non-destructive, enterprise-grade pipeline for converting PC avatars to Android/Quest. The engine generates an isolated prefab sandbox, mathematically clones materials into a unique ecosystem, and maps dependencies without ever mutating the master PC hierarchy.
- **ImageMagick Lanczos Pipeline:** Intercepted Unity's native texture importer. The engine now routes PC textures through a high-fidelity Magick.NET downsampling pass before forcefully applying Android ASTC compression. This guarantees "Glossy Girl" neon aesthetics and high-contrast maps remain crisp even when crushed to meet VRChat's strict VRAM limits (10MB/18MB/40MB).
- **Heuristic PhysBone Culling:** Integrated a topology scanner that mathematically calculates skeletal depth. When an avatar exceeds the targeted Mobile Performance Rank (e.g., 8 PhysBones for Poor), the engine heuristically culls deep leaf bones while preserving vital root-level physics.
- **Interactive Topology Matrix:** Added a massive, scrollable pre-execution control panel. The matrix parses the entire skeletal hierarchy, automatically applies heuristic culling limits, and allows the creator to manually override which physics components survive the Quest conversion.
- **High-Fidelity Material Translation:** The conversion engine now defaults to `VRChat/Mobile/Toon Standard` and actively hunts for PC-side metallic, gloss, normal, and emission properties across third-party shaders (Poiyomi, lilToon), natively mapping them into the mobile shader to preserve maximum depth.

### Changed
- **Ecosystem Architecture Re-route:** Completely refactored the global `MenuItem` domain structure. Scripts are now strictly isolated into `VixenTools/Avatars`, `VixenTools/Unity Engine`, and `VixenTools/Scene` to establish a highly scalable foundation for future toolsets.
- **Live Surface Snapping (Engine Upgrade):** Overhauled the legacy update loop. Replaced brittle GUI `Event.current` dragging with Unity's native, low-overhead `Transform.hasChanged` flag. Also integrated a dedicated `Ctrl+Alt+S` hotkey for discrete, one-shot surface dropping.
- **Matrix UI/UX:** Transformed the Interactive Topology Matrix from a cramped text dump into a highly readable control panel. Parent hierarchy paths are now dimmed, while target leaf bones are highlighted in high-contrast neon green (`#00ff66`). 

### Fixed
- **Pivot-Clipping (Surface Snap):** Objects no longer incorrectly snap by their center pivot. A new recursive algorithm dynamically calculates true geometric bounds across all child colliders and renderers, ensuring meshes always sit perfectly flush on their "feet."
- **Self-Collision Loops (Surface Snap):** Injected an execution micro-state that temporarily pushes the target object to the `IgnoreRaycast` layer during raycasting, completely preventing meshes from snapping to their own internal colliders.
- **HelpBox Rich Text Stripping:** Bypassed Unity's default EditorGUI stripping by constructing a custom HelpBox layout wrapper, restoring full HTML/Rich Text formatting to the Quest Engine's results dashboard.

## [1.2.0] - 2026-04-20
### Added
- **Vixen Hub Integration:** Merged the standalone documentation and changelog parsers into a unified, tabbed Editor dashboard. Features a custom UV-framing engine for dynamic banner scaling and persistent ecosystem routing.
- **Ecosystem Discovery Engine:** The Badge Studio now passively scans the project hierarchy to detect installed Furality SDKs (Luma, Somna, Sylva, Umbra). Automatically indexes available convention tiers and dynamically routes output paths.
- **Universal Shader Targeting:** Built a heuristic material engine that natively detects and configures target shaders. Automatically routes diffuse/emission maps and activates required toggle parameters across Poiyomi, lilToon, VRChat Mobile, and all legacy Furality shaders.
- **Dynamic UV Auto-Layout:** The generator now intercepts convention selections and automatically adjusts localized text bounds, rotations, and signature neon hex colors to perfectly match the specific year's mesh layout.

### Changed
- **Template Authoring Pipeline:** Upgraded the procedural base generator to support raw Asset Ingestion. The tool can now swallow empty convention `.jpg` files, transcode them, and perfectly adapt its internal compilation resolution to match the source file.

### Fixed
- **Furality SDK Plurality & Obfuscation:** Engineered a recursive deep-scan discovery protocol to bypass Furality's erratic year-to-year folder restructuring. The engine now aggressively hunts down target assets regardless of missing subfolders or `Texture`/`Textures` plurality variations.
- **Magick.NET Dimension Casting:** Resolved implicit `uint` to `int` conversion deadlocks during dynamic resolution matching.
- **Emission Blackout Safeguard:** Added a failsafe to detect and override black or transparent `_EmissionColor` multipliers on legacy convention materials, preventing generated emission masks from multiplying to zero and rendering invisibly.

## [1.1.1] - 2026-04-18
### Added
- **Destructive Pipeline Operations:** Integrated a "Delete Selected" function within the Animation Workbench bindings matrix. This allows for the safe pruning of dead or unused property tracks, preventing asset bloat and VRChat SDK validation errors.

### Changed
- **UI/UX Ecosystem Integration:** Overhauled the Animation Workbench's UI Toolkit construction to natively inject the signature VixenTools "cyber-noir" aesthetic. The tool now features the persistent branded header and color-coded action buttons without requiring core stylesheet rewrites.
- **Explicit Intent Paradigm:** Flipped the default states for "Sample Start Value" and "Sample End Value" to `false`. The pipeline now safely defaults to manual override values (`1` and `0`) when generating curves, preventing the tool from destructively hijacking live scene data unless explicitly commanded.

### Fixed
- **Multidimensional Property Extraction:** Resolved a critical native API crash when attempting to bind Vector/Color properties (e.g., `_AudioLinkEmission0CenterOut`) on locked Poiyomi shaders. The discovery engine now safely decomposes these properties into their individually animatable float channels (`.r`, `.x`, etc.) and routes them through a robust `TryGetMaterialFloat` fail-safe protocol.

## [1.1.0] - 2026-04-10
### Added
- **Animation Workbench Pro:** Officially migrated into the VixenTools package ecosystem. Features advanced curve operations, easing dropdowns, timeline ribbons, and a real-time preview engine.
- **Runtime Math Library:** Established the `Runtime` assembly by migrating `EasingFunctions.cs` to handle pure mathematical logic (Linear, SmoothStep, Cubic, etc.) safely in-game.
- **Dynamic Hub Engine:** Upgraded the VixenTools Hub to autonomously parse and render the package `README.md` using a custom Regex-driven Markdown-to-IMGUI pipeline.
- **Ecosystem Routing:** Integrated a persistent, stylized navigation bar into the Hub for direct access to the GitHub repository, YouTube video guides, and X (Twitter) announcements.

### Changed
- **UI/UX Standardization:** Overhauled the Editor interfaces for `PhysBoneTopologyMapper` and the `Pipeline Preset Manager` to perfectly mirror the VixenTools ecosystem aesthetic. Implemented custom IMGUI headers, neon-tinted rich text, and standardized action buttons.
- **Compiler Safeguards:** Wrapped all Animation Workbench Editor scripts (`AnimationWorkbenchWindow`, `CurveGraphView`, `PreviewEngine`, etc.) in strict `#if UNITY_EDITOR` directives to prevent runtime build crashes.
- **UI Toolkit Pathing:** Updated stylesheet loading paths to be strictly VPM-compliant, ensuring `.uss` files resolve correctly from the `Packages/` directory instead of the local `Assets/` folder.
- **Markdown Sanitization:** The in-editor Markdown parser now actively strips VPM/GitHub web badges to prevent IMGUI rendering errors while converting standard hyperlinks into clean, stylized rich text.

## [1.0.3] - 2026-04-10
### Fixed
- **VPM Dependency Resolver:** Replaced strict greater-than inequality (`>=`) with caret operators (`^`) in `package.json` to prevent the VRChat Creator Companion from aggressively fetching unstable beta SDKs.
- **Listing Hydration:** Repaired the VCC listing frontend by migrating Scriban JSON data generation out of the client-side JavaScript and into a protected DOM bridge, preventing IDE auto-formatters from shattering the build pipeline.

## [1.0.2] - 2026-04-10
### Fixed
- **Compiler Bleed-Over:** Injected a strictly configured Assembly Definition (`.asmdef`) into the `Editor` directory. This hard-locks compilation boundaries, preventing the `PhysBoneTopologyMapper` from crashing Unity when users attempt to build their avatars for runtime.

## [1.0.1] - 2026-04-10
### Added
- **CI/CD Pipeline:** Established fully automated GitHub Actions workflows for generating `.zip` releases, `.unitypackage` fallbacks, and VPM-compliant registry manifests.
- **VCC Storefront:** Deployed a glassmorphic, cyberpunk-styled VPM package listing using Fluent UI web components and FAST Design tokens.

## [1.0.0] - 2026-04-09
### Added
- **PhysBoneTopologyMapper:** Initial release of the flagship extraction and injection engine.
- **Master Blueprints:** Added functionality to utilize `AnimationUtility.CalculateTransformPath` and Unity `.preset` files to bypass native prefab constraints and map complex topologies (e.g., Novabeast Master Topology) seamlessly across compatible avatar roots.