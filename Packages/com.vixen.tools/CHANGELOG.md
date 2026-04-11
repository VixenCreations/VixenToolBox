# VixenTools Ecosystem Changelog

All notable changes to the VixenToolBox project will be documented in this file.

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

### Changed
- **Compiler Safeguards:** Wrapped all Animation Workbench Editor scripts (`AnimationWorkbenchWindow`, `CurveGraphView`, `PreviewEngine`, etc.) in strict `#if UNITY_EDITOR` directives to prevent runtime build crashes.
- **UI Toolkit Pathing:** Updated stylesheet loading paths to be strictly VPM-compliant, ensuring `.uss` files resolve correctly from the `Packages/` directory instead of the local `Assets/` folder.

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