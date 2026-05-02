***

# VixenTools - Changelog

All notable changes to the VixenToolBox project will be documented in this file.

***

## [1.5.1] - 2026-05-01

### Added
- **Autonomous Domain Polling (Vixen Hub):** Engineered a static `[InitializeOnLoad]` routine that silently parses the `package.json` manifest during Unity domain reloads. It now dynamically cross-references the active ecosystem against local `EditorPrefs` to autonomously trigger the Scene View update badge without requiring manual intervention.
- **Direct Tab Routing (UI Ecosystem):** Injected synchronous tab-routing execution into the `VixenHub` class. Clicking the Scene View update notification now bypasses the default dashboard and instantly maps the UI matrix directly to the Release Changelogs.

### Changed
- **Nuclear Decimation Downgrade (Topology Engine):** Re-engineered the Multi-Pass Welder from an aggressive 3D volumetric crusher into an *Extreme Precision 5D Microwelder*. Enforced a strict 5mm (`0.005f`) structural hard cap to safely seal sub-millimeter import seams without collapsing limb volumes or flattening continuous meshes into 2D singularities.
- **Kinematic Shield Bypassing (Avatar Validator):** Strategically punctured the `Neck`/`Head` protective bone matrix. The heuristic scanner now explicitly isolates and exposes dense, non-structural children (`Hair`, `Ears`, `Fluff`) to the optimization grid, allowing the decimator to successfully hit Quest poly limits without being bottlenecked by shielded geometry.

### Fixed
- **UV Seam Tearing (Topology Engine):** Eradicated catastrophic texture stretching during decimation sweeps. The upgraded 5D Spatial Hash `(X, Y, Z, U, V)` now strictly locks UV coordinates, guaranteeing that vertices sharing atomic space but possessing divergent texture mappings are never violently fused.
- **RenderTexture Magick Crashes (VRAM Optimizer):** Patched a critical failure loop where the `ProcessTexturesWithMagick` pipeline attempted to intercept and decode volatile, active `RenderTexture` assets, fully resolving the recurring `error/constitute.c/ReadImage/753` console spam during a Deep Matrix Scan.
- **SDK Version Regex Blindspot (Vixen Hub):** Expanded the ecosystem's JSON parser to actively detect modern `com.vrchat.base` and `com.vrchat.worlds` dependencies, fixing a logic gap where the dashboard header falsely reported the active SDK version as "Unknown".

## [1.5.0] - 2026-04-30

### Architectural Overhaul & Dual-SDK Ecosystem Integration
- **Context-Aware Dual-SDK Architecture (Vixen Hub):** The core VixenToolBox ecosystem has been completely re-engineered to natively bridge and map both the VRChat Avatar SDK (`VRC_SDK_VRCSDK3`) and the VRChat World SDK (`UDON`). The central Hub now operates as a dynamic gateway, autonomously reading the active compiler directives upon project initialization and seamlessly hot-swapping the "Flagship Tools" UI matrix to match the current development environment.
- **Airtight Compiler Sandboxing:** Implemented strict, cryptographic-level compiler isolation across the entire asset suite to guarantee zero assembly bleed-over. World-exclusive utilities like `SnapToSurface.cs` are now mathematically locked behind `#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON` directives, while avatar-centric suites (`VixenBadgeMaker`, `QuestConversionEngine`, `AvatarSDKValidator`) operate strictly within `#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON` boundaries. This ensures Unity's compiler strips incompatible scripts entirely, preventing catastrophic `VRCAvatarDescriptor` or `UdonBehaviour` reference bricks when migrating the toolbox across radically different VRChat project infrastructures.
- **Dynamic Assembly Resolution:** The internal reflection engine now safely queries for third-party and cross-SDK assemblies without establishing hard dependencies. By utilizing `comp.GetType().Name.Contains("VRCFury")` during serialization sweeps, the package exists seamlessly in vanilla or heavily modded Unity environments without throwing missing assembly references.

### Destructive Topology & Spatial Welding Engines
- **5D Spatial Hash Welder (Optimization Suite):** Engineered a highly destructive, multi-threaded vertex decimation engine designed specifically for massive PC avatars (`VixenMeshPatcher.WeldVertices`). The welder violently fuses co-located vertices on excessively heavy meshes (>15,000 polygons) by generating a proprietary 5-dimensional coordinate key (`"{hashX}:{hashY}:{hashZ}:{uvHashX}:{uvHashY}"`). By running both `x / threshold` and `u / uvThreshold` calculations, it drastically crushes raw polycounts while mathematically guaranteeing the survival of critical texture seams.
- **Dual-Shielding Exclusion Matrix:** The Welder is no longer blind to high-fidelity topology. It now features an autonomous Surgical Exclusion Matrix that deploys two layers of defense during decimation:
  - **Material Keyword Mapping:** Automatically scans all submesh material slots for high-detail keywords (`"eye"`, `"visor"`, `"lens"`, `"blush"`, `"face"`, `"mouth"`, `"teeth"`, `"pupil"`, `"iris"`) and assigns them a locked `"PROTECTED_{i}"` spatial key.
  - **Kinematic Bone-Weight Shielding:** Actively queries the active Humanoid Armature for `HumanBodyBones.Neck` and `HumanBodyBones.Head` transforms, recursively mapping their children to mathematically lock any vertex influenced by these bones entirely out of the decimation grid.
- **BlendShape Memory Recovery:** Executing a destructive `mesh.Clear()` inherently annihilates Unity's blendshape delta arrays. The Spatial Welder now performs a pre-execution memory extraction via a `BlendShapeExtract` struct, caching all 175+ frames (`DeltaVerts`, `DeltaNormals`, `DeltaTangents`). Post-decimation, it mathematically averages the delta offsets across the newly fused vertices via `mergeCounts`, allowing highly complex facial animations to perfectly survive aggressive topology meltdowns.
- **Submesh Array Preservation:** The decimation pipeline now strictly extracts and reconstructs triangle arrays on a per-submesh basis using `mesh.GetTriangles(s)`. This prevents the "Atlas Trap" where decimated avatars would violently collapse into a single material slot, guaranteeing your multi-material setups remain perfectly intact.

### Dynamic Matrix & Performance Heuristics
- **Dynamic PC Target Rank Matrix:** Eradicated static, hardcoded PC culling limits. The PC Optimization Suite now features a dynamic `<fluent-select>` Enum dropdown mapping to `AvatarSDKValidator.PCPerformanceRank`. Creators can explicitly target specific VRChat Performance Ranks (Excellent, Good, Medium, Poor). The deep-scanning heuristics instantly recalibrate their execution thresholds (e.g., Excellent strictly enforces a maximum of 4 PhysBones, 4 Contacts, and 1 Animator, while Poor expands the cap to 32).
- **Interactive Physics Executioner:** Replaced blunt, invisible auto-cull algorithms with a surgical UI Toolkit panel. The PC Validator now spawns a dedicated interactive matrix populating a `List<PhysicsNode>`. Sorted mathematically by hierarchy depth (`GetDepth`), it empowers creators to manually select and eradicate expendable leaf-node physics via `Undo.DestroyObjectImmediate` while visually protecting structural physics roots.
- **Deep Material Inspector Spider (VRAM Profiling):** Upgraded the VRAM calculation engine to pierce deeply through the Unity Inspector. It now weaponizes `EditorUtility.CollectDependencies` data streams to spider through highly obfuscated components, specifically targeting `anim.runtimeAnimatorController` and VRCFury `MonoBehaviour` outfit toggles. It aggressively hunts down and caches nested `Texture` dependencies, ensuring the ImageMagick downsampling pipeline captures 100% of the avatar's actual memory footprint.
- **Base-Class Physics Targeting:** Upgraded the deep matrix scanners from strict `<VRCPhysBone>` wrappers to their foundational `<VRCPhysBoneBase>` and `<VRCPhysBoneColliderBase>` classes. The engine now successfully captures 100% of internal physics variations, including VRCFury auto-conversions, script-generated endpoints, and legacy dynamic bone migrations.

### UI/UX & Cyber-Noir Standardization
- **Universal Cyber-Noir Topology (UI Ecosystem):** Extended the native UI Toolkit `.uss` architecture universally across the entire VixenTools ecosystem. Every utility—from the `AnimationWorkbenchWindow` to the `QuestConversionEngine`—now perfectly inherits the signature dark-panel aesthetics, neon-cyan borders, and hyper-responsive interactive hover states (`.cyber-panel`, `.cyan-btn`, `.danger-btn`, `.data-tag-destructive`) via centralized stylesheet injections.
- **Native Class Hijacking:** Overhauled the core `.uss` stylesheets to natively target and hijack Unity's internal UI classes, specifically `.unity-enum-field`, `.unity-popup-field`, and `.unity-slider-int`. Optimization sliders now render as sleek, flat cyberpunk tracks with glowing cyan draggers (`.unity-slider-int .unity-base-slider__dragger`), while dropdowns feature tinted glass backgrounds and neon borders.
- **Macro Topology Overrides:** Injected global "Keep All", "Cull All", "Select All", and "Deselect All" macro execution buttons into the interactive UI panels of both the Quest Engine and PC Validator via UI Toolkit button callbacks. This establishes a highly streamlined triage workflow when parsing massive arrays of `TopologyNode` physics components or `TextureNode` assets.
- **Instant UI State Refresh (Vixen Hub):** Re-engineered the action delegate for the "Snap To Surface" world tool. It now asynchronously reads live `EditorPrefs.GetBool` states and forces an immediate UIElements layout refresh, allowing the button text to instantly transition between active and inactive states without requiring a manual window reload.

### Critical Bug Fixes & Matrix Stabilizations
- **Kinematic Bounding Box Detonation:** Repaired a massive mathematical flaw where extracting standard bounds from meshes with offset origins threw the bounding box entirely out of the camera's frustum. The engine now initializes a `new Bounds(rootCenter, Vector3.zero)`, extracts all 8 `corners` of the `sharedMesh.bounds` into absolute World Space via `TransformPoint`, and projects them backward into root-bone Local Space via `InverseTransformPoint` for a mathematically perfect, tight-fit `Encapsulate` pass.
- **The Atlas Trap (Decimation Bleed):** Resolved a critical oversight where detailed facial geometry utilizing the same universal `bodyMat` as the torso was being incorrectly targeted and melted down by the Welder. Successfully mitigated via the new Kinematic Bone-Weight Shielding protocol scanning `HumanBodyBones.Neck`.
- **Texture Shattering (UV Seam Rips):** Fixed the destructive spatial hash blindly gluing UV seams together based solely on 3D proximity, which previously turned decimated textures into shattered, stretched geometry.
- **IgnoreTransform Shield Piercing:** The Topology Erasure matrix now actively parses every `ignoreTransforms` array inside `VRCPhysBoneBase` components. Utilizing `.IsChildOf(ig)`, if a bone is explicitly listed on that array, the `HasPhysBoneProtection` heuristic shield is forcefully dropped, allowing the Culler to properly identify and melt dead leaf bones that were previously evading execution.
- **Autonomous Scene HUD Notifier:** Completely rebuilt the update notification badge from the ground up to support Unity 2022.3. Stripped out legacy `Handles.BeginGUI()` IMGUI math and injected a native UI Toolkit `Button` directly into the `sceneView.rootVisualElement` to perfectly respect high-DPI scaling, absolute coordinate anchoring, and scene depth-sorting.
- **CSS BorderRadius Traps:** Replaced the incompatible `borderRadius` UI Toolkit C# shorthand with explicit, pedantic corner definitions (`borderTopLeftRadius`, `borderBottomRightRadius`, etc.) to permanently resolve `CS0117` compilation errors while maintaining the sleek, rounded aesthetic on the interactive scroll views.
- **Compiler Scope Breakage:** Resolved a critical `CS1022` namespace definition error caused by a rogue bracket escaping the `#endif` directive in the Hub's master script, restoring flawless domain reloads and script compilation.

## [1.4.5] - 2026-04-28

### Added
- **Dynamic Version Extraction (Vixen Hub):** Engineered a lightweight Regex parser that autonomously reads the ecosystem's `package.json` during initialization to extract and display the active VixenTools and VRCSDK versions in the header, completely eliminating hardcoded strings and race conditions.
- **Changelog Pagination Engine (Vixen Hub):** Completely overhauled the changelog markdown parser. Replaced infinite vertical scrolling with a structured, memory-efficient data dictionary and a native `DropdownField` to dynamically isolate and render individual release histories.
- **Physics Joint Heuristics (Quest Engine):** Expanded the deep matrix scan to actively hunt and isolate unsupported Unity physics constraints (`SpringJoint`, `FixedJoint`, `HingeJoint`).
- **Dedicated Joint UI Topology (Quest Engine):** Physics joints now populate in a dedicated phase 1.5 Interactive Topology matrix section, equipped with mathematically locked (limit 0), neon-red danger toggles to guarantee they are auto-culled during the conversion pipeline.
- **Native Style Hijacking (UI Ecosystem):** Injected base Unity class overrides (e.g., `.unity-base-popup-field__input`) into the core `.uss` stylesheets. Standard Unity fields now automatically inherit the VixenTools cyber-noir dark panels and cyan-tinted borders without requiring clunky inline C# modifications.

### Changed
- **Destruction Pipeline Routing (Quest Engine):** Re-routed the `ProcessDestruction` sequence so that physics joints are securely stripped from the prefab sandbox before unpack, preventing VRChat SDK mobile validation errors.

### Fixed
- **UI Toolkit Flex Boundaries (Vixen Hub):** Repaired a critical layout quirk where the markdown `ScrollView` lacked strict flex limits, causing the viewport to bleed off the bottom of the Editor window. Enforced `flex-shrink: 1` and applied aerodynamic padding buffers to restore full scroll depth.
- **Regex Verbatim Escaping (Vixen Hub):** Resolved `CS1026` and `CS8997` compile errors caused by double-escaped quotes during the manifest parsing sequence.

## [1.4.4] - 2026-04-27

### Added
- **[Website] Support Ecosystem Hub:** Created a dedicated support page featuring interactive, glass-morphic tip-jar cards to transparently fund development without paywalling features.
- **[Website] Network & Media Matrix:** Deployed a new social routing hub featuring a mathematically enforced 16:9 responsive YouTube iframe for the Master Class video, accompanied by a 6-card creator network grid.
- **[Website] Changelog Pagination Engine:** Built a JavaScript-driven pagination system for the changelog. Replaced infinite scrolling with isolated version views, seamless CSS fade animations, and "Newer/Older" step routing.
- **[Website] Version Jump Dropdown:** Integrated a native `<fluent-select>` dropdown to instantly route users to specific historical releases.
- **[Website] Scroll Tracking UX:** Engineered a dynamic "Jump to Top" button that monitors viewport depth and gracefully fades into the UI to prevent endless scrolling on lengthy matrices.
- **[Website] Interactive Lightbox Engine:** Engineered a native, zero-dependency lightbox system allowing users to dynamically expand UI Toolkit previews into high-resolution cinematic viewports on the Architecture matrix.
- **[Website] SaaS-Grade Competitive Matrix:** Completely eradicated the legacy HTML data table on the comparison page. Replaced it with a responsive, flex-wrapping feature grid using high-contrast neon styling to directly pit VixenTools against fragmented industry standards.
- **[Website] Visual Architecture Showcase:** Overhauled the Suite Architecture page from a text-heavy bulleted list into a high-fidelity image grid showcasing the actual custom Editor inspectors and utilities.

### Changed
- **[Website] Shadow DOM Styling:** Pierced the Fluent UI `<fluent-select>` Shadow DOM using `::part(listbox)` to force native dropdowns to respect the dark cyber-noir aesthetic and enforce scrollbar constraints.
- **[Website] Registry Template Sanitization:** Stripped deprecated modal structures from the core `index.html` VPM registry and secured the Scriban templating engine (e.g., `{{~ for package in packages ~}}`) for safe backend hydration during CI/CD deployment.
- **[Website] Enterprise Navigation Matrix:** Scrapped the horizontal top-row navigation. Implemented a persistent, fixed-position vertical sidebar to create a true desktop application feel, utilizing full-width block interaction states and cyan active-border tracking.
- **[Website] Cinematic Image Cropping:** Deployed `object-fit: cover` logic to the Quest vs PC Comparison banner to safely lock its height and prevent it from infinitely scaling and hogging the viewport.

### Fixed
- **[Tool] SDK Dependency Resolution:** Bumped ecosystem manifest to `v1.4.4` and strictly enforced VRChat SDK requirements (`com.vrchat.base` and `com.vrchat.avatars` mapped to `^3.10.3`). This resolves critical missing assembly references required by the newly integrated Live Scene UV Mapper's raycasting components.

## [1.4.3] - 2026-04-26

### Added
- **Core Modules Matrix (Vixen Hub):** Added a dedicated "Core Modules" tab serving as a centralized launcher for all flagship utilities within the VixenTools ecosystem.
- **Action Delegate Routing (Vixen Hub):** Upgraded the Hub's grid rendering engine (`RenderActionGrid`) to accept raw C# `System.Action` delegates. This allows the exact same CSS `.link-card` architecture to seamlessly route both web URLs and native `EditorApplication.ExecuteMenuItem` commands without duplicating code.
- **Dynamic Markdown Link Injection (Vixen Hub):** Upgraded the internal Markdown parser to intercept `[text](url)` syntax mid-paragraph. It dynamically spawns transparent UI Toolkit `Button` elements, stripping their native borders and backgrounds to blend perfectly into the standard `Label` text flow while executing `Application.OpenURL`.
- **Full Mobile Performance Heuristics (Quest Engine):** The scanner now mathematically calculates 100% of VRChat's Android Performance limits. Added native topology parsing for Triangles, Skinned Meshes, Material Slots, `VRCRaycast`, `ParticleSystem`, `TrailRenderer`, and `LineRenderer`.
- **Incompatible Mobile Component Purge:** The Quest Engine now actively hunts down objects globally banned on VRChat mobile (Cameras, Lights, AudioSources, Cloth, Rigidbodies, and standard Unity Colliders). These populate in a new "Auto-Culled" UI matrix featuring locked, neon-red danger toggles to guarantee their removal.
- **Root Animator Protection:** Developed a recursive depth scanner that detects the avatar's core root Animator. This component is now hard-locked into a "Kept" state (visualized with a protected neon-cyan toggle) to ensure heuristic culling never accidentally shatters the avatar's base functionality.
- **High-Fidelity Linear Downsampling Pipeline:** Upgraded the ImageMagick Lanczos resize method to perform gamma-correct scaling. Textures are temporarily moved into `ColorSpace.RGB` (Linear) during the downsample, preventing the mathematical darkening/crushing of highlights typical in standard sRGB scaling.
- **Micro-Contrast Texture Recovery:** Injected a mathematically calculated `UnsharpMask(0.0, 0.5, 1.0, 0.05)` into the ImageMagick pipeline post-resize to recover high-frequency material details (fur, fabric weaves) naturally destroyed by downsampling.
- **Interactive Texture Culling Matrix:** Added a dedicated DOM matrix displaying all parsed textures and their resolutions prior to conversion. Creators can now selectively disable ImageMagick processing on a per-texture basis to massively reduce compile times on highly redundant avatars.
- **Mochie Shader Support:** The Quest Engine scanner now inherently recognizes custom third-party property routing, specifically mapping `_MochieMetallicMaps` and `_MochieMetallicMap` down to the VRChat Mobile Standard `_MetallicMap` slot.
- **Emissive Mask Color Control (Badge Studio):** Injected a new `ColorField` to give creators absolute control over the generation of the `_EMI` emission mask. This allows for multi-channel emission blending without blowing out VRChat's bloom. Saved natively to the `layout.json` matrix.
- **Material Parameter Injection (Badge Studio):** The engine now physically reaches into the compiled material to forcefully write user-selected colors (Material Base Color, Emissive Mask Color) directly into the `_Color` / `_BaseColor` and `_EmissionColor` shader properties, overriding legacy states.
- **Targeted Emission Routing (Badge Studio):** Decoupled the text rendering pipelines, adding explicit UI toggles to independently choose whether the Display Name or the Pronouns get injected into the emissive mask.

### Changed
- **Hub Parser Execution (Vixen Hub):** The Hub's dashboard now intelligently terminates its Markdown parsing loop upon detecting external routing headers (e.g., "[ Network Links ]") to prevent rendering redundant data that is already handled by dedicated tabs.
- **Topology Matrix CSS Partitioning:** Separated the massive Quest Engine topology readout into distinct, collapsible DOM foldouts (PhysBones, Colliders, Contacts, Constraints, etc.) to prevent UI bloat on highly complex avatars. 
- **Texture Importer Asset Enforcement:** The Quest Engine no longer blindly imports all processed files as standard sRGB maps. The pipeline now detects if a texture is a Normal Map or a Metallic/Gloss data map, directly injecting the `TextureImporterType.NormalMap` and `sRGBTexture = false` flags to prevent washed-out shading on the Quest clone.
- **ColorField USS Integration:** Updated the core `.uss` stylesheets to natively target `.unity-color-field`, ensuring newly spawned color pickers inherit the custom cyber-noir text scaling, panel backgrounds, and border alignments.

### Fixed
- **UI Toolkit Flex-Box Clipping (Vixen Hub):** Repaired a critical math error where nested `100%` width assignments inside `FlexDirection.Row` containers forced text to blow past the window bounds and trigger a horizontal scrollbar. Enforced strict `flex-shrink: 1`, `flex-grow: 1`, and `whiteSpace = WhiteSpace.Normal` states directly via C# to guarantee clean line wrapping around `>>` chevron bullets.
- **Alpha-Channel Emission Blowout (Badge Studio):** Fixed a critical bug where ImageMagick's transparent text backgrounds were saving as `(255,255,255,0)` (Transparent White). Because Unity's Standard shader emission strictly reads RGB and ignores Alpha, this caused the entire badge to glow blindingly white and override the diffuse map. The engine now mathematically flattens the final `_EMI` composite onto a pure `MagickColors.Black` background and permanently strips the Alpha channel to guarantee zero-bleed emission.
- **Temporary MeshCollider Memory Leak (Badge Studio):** Built a garbage-collection tracker (`_tempCollider`) for the Live Scene UV Mapper. The tool now safely executes `DestroyImmediate` on the auto-generated MeshCollider if the user disables mapping, swaps targets, or closes the window.
- **Emissive Mask Darkening (Badge Studio):** Fixed a compositing math error where the text plate on the generated `_EMI` map was adopting the user's Neon Color, which artificially darkened the shader's target `_EmissionStrength`. Text on the mask now strictly utilizes the new Emissive Mask Color (defaulting to pure white).
- **Quest Engine Compiler Warnings:** Purged a dangling `_rendererCount` variable assignment and eliminated a structurally impossible type-check against `VRCPhysBoneCollider` inside the incompatible components loop. 
- **Legacy JSON Fallback:** Fortified the Badge Studio `layout.json` parser to gracefully handle older template configs. If an older JSON file lacks the newly added `emiMaskColor` parameter, the system safely falls back to `Color.white` instead of defaulting to a transparent/black mask.

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