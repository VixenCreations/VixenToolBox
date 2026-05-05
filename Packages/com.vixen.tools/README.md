***

# VixenTools

I built VixenTools because the VRChat pipeline is full of repetitive, mind-numbing bottlenecks.

This is a comprehensive suite of custom Unity Editor utilities and automation scripts focused specifically on **High-Fidelity Avatar Pipeline & Topology Architecture**. Engineered entirely on Unity’s modern UI Toolkit, the goal is simple: eliminate human error, enforce strict project consistency, and push the Unity engine to its absolute limits. Whether you’re violently crushing a PC avatar’s polycount or auditing a complex world for Udon network starvations, this toolset is designed to save you hundreds of hours of grinding so you can actually focus on creating.

### 1. Architectural Distribution & Infrastructure
* **VPM-Native Architecture:** Distributed via a custom VRChat Creator Companion (VCC) repository so your packages stay updated inside your existing workflow.
* **Context-Aware Dual-SDK Ecosystem:** The core architecture natively bridges both the VRChat Avatar SDK (`VRC_SDK_VRCSDK3`) and the VRChat World SDK (`UDON`). The central Hub reads active compiler directives and hot-swaps the UI matrix to match your current development environment.
* **Reactive Scene State Routing (Hub):** Bypasses manual UI refreshes. The Hub’s `RenderActionGrid` uses synchronous layout flushing and rich-text injection to read `EditorPrefs` and instantly update button labels and badge states (e.g. `<color=#00e5ff>[ ACTIVE ]</color>`) as you toggle tools.
* **Autonomous Scene View HUD Notifier:** No more modal popups. A native UI Toolkit badge is injected directly into `SceneView.rootVisualElement` whenever a new VixenTools version is detected, routing you straight into the Hub’s changelog view.
* **Modern UI Toolkit Core:** All major tools (Hub, World Engine, Quest Conversion Engine, etc.) are built on UI Toolkit with custom USS themes, cyberpunk typography, and responsive layouts tuned for 2022+ editor workflows.

### 2. The World Engine: Omni-Matrix Diagnostic Spider
A lethal, 4D-chess-level heuristic auditing matrix designed to hunt down 25+ specific architectural anti-patterns across the VRChat ecosystem.
* **Omni-Matrix Ecosystem Audits:** Explicitly mapped to **ProTV**, **TXL**, and **IwaSync3**.
  * **ProTV:** Detects GSV texture conflicts, Realtime GI emission blowouts on video screens, oversized 4K `RenderTexture` VRAM nukes, and misconfigured RTGI sinks.
  * **IwaSync3:** Flags aggressive VideoCore sync frequencies, global 2D audio wash, blinding emissive screens, and GC-heavy custom event invokers.
  * **TXL:** Identifies starvation-level tracked zone polling, collapsed translation tables, orphaned Udon behaviours, and heavy cryptography sinks.
* **Raw UASM Code Scraper (Udon Persistence):** Bypasses standard component checks by intercepting the `UdonSharpEditorCache`. Parses raw Udon Assembly (UASM) to detect `PlayerData.Set` calls trapped inside `Update()` loops—catching network rate-limit nukes before they silently corrupt or throttle cloud data.
* **Native Video Pipeline Catchers:** Dedicated heuristics for `VRCAVProVideoPlayer` and `VRCUnityVideoPlayer` that detect and offer auto-fixes for “Unlimited (0)” resolution bandwidth nukes and low-latency configurations that destabilize mobile instances.
* **World Profiler Dashboard:** Aggregates texture, mesh, audio, and UI memory via `Profiler.GetRuntimeMemorySizeLong()` and computes a threat score that heavily penalizes realtime shadow casters and physics, surfacing an at-a-glance `OPTIMAL / MODERATE / SEVERE` compute threat level.

### 3. Avatar Optimization & 5D Spatial Welding
A multi-threaded execution matrix designed to violently crush PC avatar polycounts while mathematically protecting high-fidelity facial topology.
* **5D Spatial Hash Welder:** Annihilates heavy meshes using a proprietary 5D coordinate key (XYZ position plus UV coordinates). Vertices sharing atomic space but divergent UVs are never fused, preventing seam tearing and texture warping.
* **Dual-Shielding Exclusion Matrix:** Scans submesh material slots for high-detail keywords (eyes, visor, face) and queries the Humanoid armature for head/neck bone weights to lock delicate facial topology out of the decimation grid.
* **Deep Material Inspector Spider:** Pierces through obfuscated components—including Animator controllers and **VRCFury** toggles—to hunt down hidden materials and textures, ensuring the downsampling pipeline captures 100% of the avatar’s VRAM footprint.
* **Hardware-Level VRAM Profiling:** Captures the base texture class directly from GPU registers via `Profiler.GetRuntimeMemorySizeLong()`, unmasking procedural, unmanaged, and 64MB 4K `RenderTexture` assets that hide from standard project scanners.

### 4. The Quest Conversion Engine
A fully non-destructive pipeline for converting PC avatars to Android. It generates an isolated prefab sandbox so your base PC avatar is never irreversibly altered.
* **Biometric Matrix Purge (“Hunter-Killer”):** Eradicates compilation blockages by aggressively stripping out PC-VR face tracking parameters. Specifically targets `adjerry91` templates and internal VRCFury branches like `VF_UE_VRCFT` and `VRCFury - Face Tracking Prefabs`, locking them out of the Android build pipeline.
* **High-Fidelity Linear Downsampling:** Intercepts Unity’s importer to route textures through a Magick.NET Lanczos pipeline in linear color space, followed by an `UnsharpMask` pass to recover micro-contrast (fur, fabric) before ASTC compression.
* **ASTC Pipeline Sync:** Fixes sRGB/Linear mismatches by forcing correct format serialization before applying `ASTC_6x6` crunching, preventing corrupted normal maps and inverted color outputs.
* **Interactive Topology Matrix:** Presents PhysBones, colliders, contacts, constraints, raycasts, particles, trails, lines, joints, and incompatible components in a UI Toolkit matrix. VRChat mobile limits are applied per target rank, with auto-culled categories (e.g. face tracking, joints) locked for safety but still visible for forensic review.
* **Texture Processing Matrix:** Lists all detected textures with resolution metadata and per-texture toggles, allowing you to selectively opt out of Magick.NET processing for assets you want to preserve at full fidelity.

### 5. Topology Tools & QA Protocol
Tools that weaponize the Scene View itself as a precision editing surface and stress lab.
* **Precision Click-to-Place Raycaster:** A “sniper-rifle” camera raycaster that lets you visually paint or teleport objects onto complex polygons. Guided by a cyan/magenta UV projection disc, it uses a custom bitwise layer mask to ignore VRChat utility layers (Ignore Raycast, Water, UI, Player, PlayerLocal, UiMenu, Pickup) and snap exclusively to physical geometry.
* **Live Surface Snapping:** Uses low-overhead `Transform.hasChanged` tracking to continuously drop selected objects flush to the floor by their geometric “feet.” Includes “Surgical Shielding” that temporarily disables child colliders to prevent self-occlusion during raycasts, then restores them cleanly afterward.
* **Disjointed Pivot Alignment:** Re-engineered `CalculateFeetOffset` logic iterates through all child colliders and renderers to compute true lowest bounds, ensuring complex prefabs with offset pivots land perfectly on surfaces instead of floating or clipping.
* **Autonomous Omni-Chaos Generator (QA):** An environment constructor that quarantines execution into a dedicated `Stress Test.unity` scene. It builds a minimal VRChat world (lighting, floor, `VRCSceneDescriptor`, spawn) and spawns “Nightmare Pods” for standard performance issues, UI voids, VRAM nukes, persistence/network sinks, and third-party ecosystems (ProTV, TXL, IwaSync3) when detected—letting you validate heuristic catch-rates in a controlled sandbox.

### 6. Vixen Hub, Documentation, and Ecosystem
The central command console and documentation layer for the entire architecture.
* **Vixen Hub Dashboard:** A UI Toolkit-powered control center with tabs for Ecosystem Architecture, Core Modules, Network Routing, Support, and Release Changelogs. All major tools are launched from here with consistent styling and layout.
* **Dynamic Changelog Viewer:** Parses `CHANGELOG.md` into version-indexed entries with a dropdown selector. The Hub’s update badge routes directly into this tab so you can see exactly what changed in each release.
* **Core Modules Grid:** Curated launchers for flagship tools like Quest Conversion Engine, Badge Studio, Animation Workbench Pro, PhysBone Topology Mapper, Pipeline Preset Manager, and World Pipeline tools (Live Surface Snapping, Precision Click-to-Place), with live state indicators for scene utilities.
* **Network & Support Routing:** Centralized links to source repositories, issue tracking, community spaces, and support channels, framed as part of the same neon-cyber ecosystem rather than scattered bookmarks.
* **Full Website & Documentation Expansion:** A dedicated documentation portal mirrors the Hub’s structure with neon-cyber layouts, deep-dive pages for each tool, “In Practice” workflow guides, and “Under the Hood” sections that explain heuristics, math libraries, reflection systems, VRAM analyzers, and topology forensics powering VixenTools.

***