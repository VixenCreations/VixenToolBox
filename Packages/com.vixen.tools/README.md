# Vixens Toolbox

I built the Vixens Toolbox because the VRChat pipeline is full of repetitive, mind-numbing bottlenecks.

This is a comprehensive suite of custom Unity Editor utilities, automation scripts, and a flagship avatar shader focused specifically on **High-Fidelity Avatar Pipeline & Topology Architecture**. Engineered entirely on Unity's modern UI Toolkit, the goal is simple: eliminate human error, enforce strict project consistency, and push the Unity engine to its absolute limits. Whether you're violently crushing a PC avatar's polycount or auditing a complex world for Udon network starvations, this toolset is designed to save you hundreds of hours of grinding so you can actually focus on creating.

* **Package:** `com.vixencreations.vixens-toolbox` (v2.4.0)
* **Target:** Unity 2022.3.22f1 / VRChat SDK 3.10.3
* **Docs & Storefront:** [vixencreations.github.io/VixenToolBox](https://vixencreations.github.io/VixenToolBox/)

### 1. Architectural Distribution & Infrastructure

* **VPM-Native Architecture:** Distributed via a custom VRChat Creator Companion (VCC) repository so your packages stay updated inside your existing workflow.
* **Context-Aware Dual-SDK Ecosystem:** The core architecture natively bridges both the VRChat Avatar SDK (`VRC_SDK_VRCSDK3`) and the VRChat World SDK (`UDON`). The central Hub reads active compiler directives and hot-swaps the UI system to match your current development environment.
* **Reactive Scene State Routing (Hub):** Bypasses manual UI refreshes. The Hub's `RenderActionGrid` uses synchronous layout flushing and rich-text injection to read `EditorPrefs` and instantly update button labels and badge states (e.g. `<color=#00e5ff>[ ACTIVE ]</color>`) as you toggle tools.
* **Autonomous Scene View HUD Notifier:** No more modal popups. A native UI Toolkit badge is injected directly into `SceneView.rootVisualElement` whenever a new Vixens Toolbox version is detected, routing you straight into the Hub's changelog view.
* **Modern UI Toolkit Core:** All major tools (Hub, World Engine, Quest Conversion Engine, etc.) are built on UI Toolkit with custom USS themes, cyberpunk typography, and responsive layouts tuned for 2022+ editor workflows.

### 2. The Vixen World Engine

A lethal, enterprise-level heuristic auditing system designed to hunt down 25+ specific architectural anti-patterns across the VRChat ecosystem.

* **Omni-Ecosystem Audits:** Explicitly mapped to **ProTV**, **TXL**, **VizVid**, **IwaSync3**, **AudioLink**, **LTCGI**, **Rinvo**, and **Light Volumes**.
* **ProTV:** Detects GSV texture conflicts, Realtime GI emission blowouts on video screens, oversized 4K `RenderTexture` VRAM nukes, and misconfigured RTGI sinks.
* **VizVid:** Identifies cross-platform AVPro fallback failures, RateLimitResolver absences, decoupled UI/Frontend handlers, and spatialized audio bleed.
* **IwaSync3:** Flags aggressive VideoCore sync frequencies, global 2D audio wash, blinding emissive screens, and GC-heavy custom event invokers.
* **TXL:** Identifies starvation-level tracked zone polling, collapsed translation tables, orphaned Udon behaviours, and heavy cryptography sinks.
* **LTCGI:** Resolves `BakeInProgress` NRE deadlocks, detects fragmented memory arrays (ghost screens), and auto-links missing video output textures.
* **AudioLink & Light Volumes:** Flags multiple core collisions, Quest GPU readback stalls, unlinked reactive objects, and unsafe TVGI strobe/flicker thresholds.
* **Rinvo:** Re-aligns mismatched VideoPlayer UI target enums, enforces proxy addition rules, and clamps API pool sizes to prevent Udon network rate limits.
* **Raw UASM Code Validation:** Bypasses standard component checks by intercepting the `UdonSharpEditorCache`. Parses raw Udon Assembly (UASM) to detect `PlayerData.Set` calls trapped inside `Update()` loops, catching network rate-limit nukes before they silently corrupt or throttle cloud data.
* **Native Video Pipeline Catchers:** Dedicated heuristics for `VRCAVProVideoPlayer` and `VRCUnityVideoPlayer` that detect and offer auto-fixes for "Unlimited (0)" resolution bandwidth nukes and low-latency configurations that destabilize mobile instances.
* **World Profiler Dashboard:** Aggregates texture, mesh, audio, and UI memory via `Profiler.GetRuntimeMemorySizeLong()` and computes a threat score that heavily penalizes realtime shadow casters, physics, and dense Light Volume setups, surfacing an at-a-glance `OPTIMAL / MODERATE / SEVERE` compute threat level.

### 3. Avatar Optimization & Vertice Mesh Welder

A multi-threaded execution system designed to violently crush PC avatar polycounts while mathematically protecting high-fidelity facial topology.

* **Vertice Mesh Welder:** Annihilates heavy meshes using a proprietary 5D coordinate key (XYZ position plus UV coordinates). Vertices sharing atomic space but divergent UVs are never fused, preventing seam tearing and texture warping.
* **Dual-Shielding Exclusion System:** Scans submesh material slots for high-detail keywords (eyes, visor, face) and queries the Humanoid armature for head/neck bone weights to lock delicate facial topology out of the decimation grid.
* **Deep Material Inspector Spider:** Pierces through obfuscated components, including Animator controllers and **VRCFury** toggles, to hunt down hidden materials and textures, ensuring the downsampling pipeline captures 100% of the avatar's VRAM footprint.
* **Hardware-Level VRAM Profiling:** Captures the base texture class directly from GPU registers via `Profiler.GetRuntimeMemorySizeLong()`, unmasking procedural, unmanaged, and 64MB 4K `RenderTexture` assets that hide from standard project scanners.

### 4. The Quest Conversion Engine

A fully non-destructive pipeline for converting PC avatars to Android. It generates an isolated prefab sandbox so your base PC avatar is never irreversibly altered.

* **Biometric Purge ("Hunter-Killer"):** Eradicates compilation blockages by aggressively stripping out PC-VR face tracking parameters. Specifically targets `adjerry91` templates and internal VRCFury branches like `VF_UE_VRCFT` and `VRCFury - Face Tracking Prefabs`, locking them out of the Android build pipeline.
* **High-Fidelity Linear Downsampling:** Intercepts Unity's importer to route textures through a Magick.NET Lanczos pipeline in linear color space, followed by an `UnsharpMask` pass to recover micro-contrast (fur, fabric) before ASTC compression.
* **ASTC Pipeline Sync:** Fixes sRGB/Linear mismatches by forcing correct format serialization before applying `ASTC_6x6` crunching, preventing corrupted normal maps and inverted color outputs.
* **Interactive Topology System:** Presents PhysBones, colliders, contacts, constraints, raycasts, particles, trails, lines, joints, and incompatible components in a UI Toolkit system. VRChat mobile limits are applied per target rank, with auto-culled categories (e.g. face tracking, joints) locked for safety but still visible for forensic review.
* **Texture Processing System:** Lists all detected textures with resolution metadata and per-texture toggles, allowing you to selectively opt out of Magick.NET processing for assets you want to preserve at full fidelity.

### 5. Avatar Pipeline & Iteration Tools

The connective tissue that makes re-rigging, mounting, and animating avatars repeatable instead of error-prone.

* **PhysBone Topology Mapper:** Snapshots an entire PhysBone architecture into a reusable blueprint. "Extract Master Copy" walks the source avatar via `AnimationUtility.CalculateTransformPath`, serializes each `VRCPhysBone` into a `Preset`, and writes a master `ScriptableObject` mapping `bonePath` to `Preset`. "Inject Blueprint" traverses a target avatar by relative path, auto-adds any missing components, and re-applies the stored presets, restoring dozens of distinct physics setups in a single click after a destructive optimization or Quest conversion. Degrades gracefully (buttons disabled, clearly labeled) when the VRChat SDK is not present.
* **Accessory Mounting Engine:** Clones sterile armatures from a source rig and surgically mounts accessories onto the result. `FullGeneration` clones a fresh sterile armature with a recursive `CloneHierarchy` pass that preserves local TRS at every node; `AppendToExisting` reuses an already-generated rig by resolving relative bone paths. A destructive auto-rig bakes each `SkinnedMeshRenderer` with a per-child `localToBoneOffset` matrix and fully preserves blendshape deltas, while rigid props, particles, and audio sources use a locked `ParentConstraint`. PhysBone-safe root locking, culling-resistant 2.5m bounds, GUID-suffixed asset persistence, and a single collapsed undo group keep the whole pipeline clean and reversible.
* **Animation Workbench Pro:** A precision animation authoring environment that outperforms Unity's native curve tools. A `MaterialPropertySearchPopup` scans every renderer on your preview target and exposes categorized, Poiyomi-aware shader properties (with R/G/B/A color and X/Y/Z/W vector channel splitting) as animatable bindings. A custom `CurveGraphView` (zoom, pan, double-click insert, right-click delete) plus an easing library drive staged, non-destructive curve edits via `CurveOperations.BuildStretchedCurve()`, and a real-time `PreviewEngine` plays staged clips directly on a scene object through `AnimationMode.SampleAnimationClip`. Every destructive action is gated behind explicit user intent.
* **Pipeline Preset Manager:** A dual-mode import-automation engine that enforces project-wide consistency. Authoring Mode creates a temporary phantom PNG, injects your import rules, and rips the importer state into a permanent `Preset`; Extraction Mode rips Presets from existing component hierarchies (optionally including children, ignoring Transforms). Generated presets can be globally registered via `Preset.SetDefaultPresetsForType()` with glob filters, so every new texture, audio clip, renderer, or PhysBone entering the project inherits your standards automatically.

### 6. Scene Tools & QA Protocol

Tools that weaponize the Scene View itself as a precision editing surface and stress lab.

* **Precision Click-to-Place Raycaster:** A "sniper-rifle" camera raycaster that lets you visually paint or teleport objects onto complex polygons. Guided by a cyan/magenta UV projection disc, it uses a custom bitwise layer mask to ignore VRChat utility layers (Ignore Raycast, Water, UI, Player, PlayerLocal, UiMenu, Pickup) and snap exclusively to physical geometry.
* **Live Surface Snapping:** Uses low-overhead `Transform.hasChanged` tracking to continuously drop selected objects flush to the floor by their geometric "feet." Includes "Surgical Shielding" that temporarily disables child colliders to prevent self-occlusion during raycasts, then restores them cleanly afterward.
* **Disjointed Pivot Alignment:** Re-engineered `CalculateFeetOffset` logic iterates through all child colliders and renderers to compute true lowest bounds, ensuring complex prefabs with offset pivots land perfectly on surfaces instead of floating or clipping.
* **Live Scene UV Mapper:** Hijacks the Scene View camera to raycast badge coordinates directly onto curved 3D meshes, mathematically inverting barycentric UVs into ImageMagick pixel space with one-click clipboard copy. Non-destructive (a temporary MeshCollider is injected and removed on exit) and built for curved or 3D-printed badge surfaces where flat 2D layout tools fail.
* **Autonomous Omni-Chaos Generator (QA):** An environment constructor that quarantines execution into a dedicated `Stress Test.unity` scene. It builds a minimal VRChat world (lighting, floor, `VRCSceneDescriptor`, spawn) and spawns heavily engineered "Nightmare Pods" to simulate catastrophic performance issues across physics, UI voids, VRAM nukes, network persistence sinks, and third-party ecosystems (ProTV, TXL, IwaSync3, VizVid, LTCGI, Rinvo), letting you validate heuristic catch-rates in a controlled sandbox.

### 7. Vixen Badge Studio

Procedural badge generation for VRChat conventions and identity work.

* **Ecosystem Discovery Engine:** Auto-detects Furality SDK assets across inconsistent folder structures.
* **Universal Shader Targeting:** Supports Poiyomi, lilToon, VRChat Mobile, and legacy badge shaders, auto-mapping diffuse/emissive slots and matcap channels.
* **Programmatic Template Authoring:** Generates directory structures, materials, and emissive maps automatically, including 4K convention-grade templates.
* **Dynamic UV Auto-Layout:** Snaps text bounds and neon hex accents to each year's badge mesh, and pairs directly with the Live Scene UV Mapper for pixel-perfect placement on curved surfaces.

### 8. Vixen Hub & In-Engine Documentation

The central command console and documentation layer for the entire architecture.

* **Vixen Hub Dashboard:** A UI Toolkit-powered control center with tabs for Ecosystem Architecture, Core Modules, Network Routing, Support, and Release Changelogs. All major tools are launched from here with consistent styling and layout.
* **Dynamic Changelog Viewer:** Parses `CHANGELOG.md` into version-indexed entries with a dropdown selector. The Hub's update badge routes directly into this tab so you can see exactly what changed in each release.
* **Core Modules Grid:** Curated launchers for flagship tools like the Quest Conversion Engine, Badge Studio, Animation Workbench Pro, PhysBone Topology Mapper, Accessory Mounting Engine, Pipeline Preset Manager, and the World Pipeline tools (Live Surface Snapping, Precision Click-to-Place, Vixen World Engine), with live state indicators for scene utilities.
* **AI Transparency & Development Ethics:** Fully integrated documentation detailing our enterprise-scale AI deployment, our 100% hand-reviewed code standards, agentic research integration, and workflow accelerations, guaranteeing total transparency across the ecosystem.
* **Network & Routing:** Centralized, distraction-free routing restricted strictly to our active channels: **GitHub** (source repositories, issue tracking, and version history) and **YouTube** (technical breakdowns and pipeline tutorials).
* **In-Engine Documentation:** Bypassing external websites entirely, the Hub features a dynamic Markdown-to-UIElements parser. Deep-dive architecture guides (like the Heuristics Engine and Shader Setup) are rendered directly inside the Unity Editor, keeping your workflow unbroken.

### 9. VixenWear Latex Ultra - Native Shader Architecture

A proprietary, high-fidelity dual-lobe PBR surface shader engineered for synthetic materials, shipping alongside a tessellation-free `Latex Ultra SPS` variant for VRCFury SPS patching. The inspector is split into six tabs: BASE, SURFACE, POLISH, INTEGRATION, AUDIOLINK, and STAGE.

* **Physical Lighting Model:** A full industry-standard GGX BRDF stack (`D_GGX`, `V_SmithJointGGX`, `F_Schlick`, `Burley` diffuse, Karis split-sum) with optional Filament/Frostbite multi-scatter energy compensation, anisotropic latex-stretch specular, thin-part transmission, tinted dielectric clearcoat, geometric specular anti-aliasing, and a VRChat mirror-camera fix that reads the true rendering camera for per-eye-correct specular and parallax.
* **Texture Packing & Compatibility:** A single packed RGBA PBR map with Poiyomi / Substance / Marmoset channel selectors, Mochie reflection & specular masks with a one-click setup button, and a Standard-style render-mode selector (Opaque / Cutout / Fade / Transparent).
* **Liquid Surface System (2.4.0):** A master Polish gate collapses the entire clearcoat/SSS/transmission stack to a flat base on demand; a wet/run-off layer adds a soaked look, animated rivulets, and PC-only geometry water droplets; and a gravity-aligned melting goo system sags the mesh in the displacement stage with floor pooling and per-strand sway.
* **World Integration & Kinetics:** Deep Light Volumes, LTCGI, and PiMaker AreaLit area-light mix controls (AreaLit intercepted at the GI level through an included world-side Global Broadcaster, so every avatar in the world is lit automatically), the VRSL stage-hijack protocol with DMX geo-warping plus an additive GI stage wash, runtime-gated AudioLink, the God Tier Cybernetics HUD (VU meters, spectrum, waveform, DMX grid, autocorrelator ring), and a flying-shard kinetic vertex engine. Every world-lighting integration is fail-safe: it strips or no-ops cleanly in worlds that lack the system.
* **VectorLabelDrawer & Hybrid UI Architecture:** A custom `MaterialPropertyDrawer` natively hooked into ShaderLab's Vector arrays eliminates 4-float UI clutter while preserving exact field alignments, wrapped in a UITK + IMGUI hybrid inspector with secure `EditorPrefs` caching that survives domain reloads. Per-tab copy/paste/reset and a build-time variant stripper keep materials clean.
* **Aggressive Baseline Optimization & Packing:** Shader variables are consolidated into packed `Vector` attributes (`_PBRParams`, `_GeoEmisParams`, `_ClearcoatParams`, etc.) to slash uniform overhead, and expensive calculations (Parallax, Displacement, Iridescence, Rim, AudioLink) are zeroed out by default for maximum out-of-the-box performance.

> Every option in every inspector tab is documented control-by-control, with full editor screenshots, in the [Shader Docs](https://vixencreations.github.io/VixenToolBox/shaderdocs.html).

### 10. Special Thanks & Acknowledgements

This architecture requires immense R&D and community synergy. Massive thanks to the following creators for their crucial insights and forensic pipeline debugging for keeping the engine running:

* **Lt_Shadow:** Suggested the UI Toolkit font replacer infrastructure.
* **TheCastle:** Informed me about the underlying issues with LTCGI, driving our deadlock resolution.
* **ValenVRC:** Caught the Udon network heuristics being too vague, leading directly to the invention of our serialization ghost-component detector.
* **KittehKun:** Suggested the early naming and wording schemes being more streamlined.
* **DJ Red_Panda:** Suggested advanced LOD fixes and other critical miscellaneous pipeline items.
* **RBN's World Creators:** Suggested specific mechanics for the VRAM estimation matrices.
* **flickfluff:** Caught the Avatar Validator failing to strip Quest face-tracking components, directly inspiring the "Hunter-Killer" Biometric Purge.
