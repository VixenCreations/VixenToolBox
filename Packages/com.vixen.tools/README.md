# Vixens Toolbox

I built the Vixens Toolbox because the VRChat pipeline is full of repetitive, mind-numbing bottlenecks.

This is a comprehensive suite of custom Unity Editor utilities and automation scripts focused on getting avatars and worlds ready to ship. The four flagship tools are the **Quest Conversion Engine**, the **Avatar Optimization Suite**, the **Vixen World Engine**, and **Animation Workbench Pro**. Engineered entirely on Unity's modern UI Toolkit, the goal is simple: eliminate human error, enforce strict project consistency, and push the Unity engine to its absolute limits. Whether you're violently crushing a PC avatar's polycount or auditing a complex world for Udon network starvations, this toolset is designed to save you hundreds of hours of grinding so you can actually focus on creating.

* **Package:** `com.vixencreations.vixens-toolbox` (v2.17.0)
* **Target:** Unity 2022.3.22f1 / VRChat SDK 3.10.3
* **Docs & Storefront:** [vixencreations.github.io/VixenToolBox](https://vixencreations.github.io/VixenToolBox/)

### 1. Distribution & Infrastructure

* **VPM-Native Distribution:** Distributed via a custom VRChat Creator Companion (VCC) repository so your packages stay updated inside your existing workflow.
* **Context-Aware Dual-SDK Ecosystem:** The toolbox natively bridges both the VRChat Avatar SDK (`VRC_SDK_VRCSDK3`) and the VRChat World SDK (`UDON`). The central Hub reads active compiler directives and hot-swaps the UI system to match your current development environment.
* **Reactive Scene State Routing (Hub):** Bypasses manual UI refreshes. The Hub's `RenderActionGrid` uses synchronous layout flushing and rich-text injection to read `EditorPrefs` and instantly update button labels and badge states (e.g. `<color=#00e5ff>[ ACTIVE ]</color>`) as you toggle tools.
* **Autonomous Scene View HUD Notifier:** No more modal popups. A native UI Toolkit badge is injected directly into `SceneView.rootVisualElement` whenever a new Vixens Toolbox version is detected, routing you straight into the Hub's changelog view.
* **Modern UI Toolkit Core:** All major tools (Hub, World Engine, Quest Conversion Engine, etc.) are built on UI Toolkit with custom USS themes, cyberpunk typography, and responsive layouts tuned for 2022+ editor workflows.

### 2. The Vixen World Engine

A lethal, enterprise-level heuristic auditing system running around 137 distinct checks across nine supported ecosystems, most of them carrying a one-click fix.

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

### 3. Avatar Optimization & QEM Decimation

An execution system designed to violently crush PC avatar polycounts while protecting the detail in the face, with the texture pass spread across every core your machine has.

* **Precision QEM Decimation:** Quadric Error Metric edge collapse (Garland-Heckbert), the same class of algorithm as Blender's Decimate. Drives each heavy mesh toward your triangle target while preventing face flips and preserving UV/normal seams, material boundaries and open borders. UVs, colours and bone weights are interpolated across every collapse, blendshapes are remapped, and the run halts early rather than shredding protected geometry.
* **Dual-Shielding Exclusion System:** Material slots whose names read as delicate (eye, visor, lens, blush, face, mouth, teeth, pupil, iris) are locked out of the decimation grid, and on a Humanoid rig the vertices weighted to the left and right Hand bones are locked too.
* **Deep Material Inspector Spider:** Pierces through obfuscated components, including Animator controllers and **VRCFury** toggles, to hunt down hidden materials and textures, ensuring the downsampling pipeline captures 100% of the avatar's VRAM footprint.
* **Hardware-Level VRAM Profiling:** Captures the base texture class directly from GPU registers via `Profiler.GetRuntimeMemorySizeLong()`, unmasking procedural, unmanaged, and 64MB 4K `RenderTexture` assets that hide from standard project scanners.

### 4. The Quest Conversion Engine

A fully non-destructive pipeline for converting PC avatars to Android. It generates an isolated prefab sandbox so your base PC avatar is never irreversibly altered.

* **Biometric Purge ("Hunter-Killer"):** Eradicates compilation blockages by aggressively stripping out PC-VR face tracking parameters. Specifically targets `adjerry91` templates and internal VRCFury branches like `VF_UE_VRCFT` and `VRCFury - Face Tracking Prefabs`, locking them out of the Android build pipeline.
* **High-Fidelity Linear Downsampling:** Intercepts Unity's importer to route textures through a Magick.NET Lanczos pipeline in linear color space, followed by an `AdaptiveSharpen` pass to recover micro-contrast (fur, fabric) before ASTC compression.
* **ASTC Pipeline Sync:** Fixes sRGB/Linear mismatches by forcing correct format serialization before applying `ASTC_6x6` crunching, preventing corrupted normal maps and inverted color outputs.
* **Interactive Component List:** Presents PhysBones, colliders, contacts, constraints, raycasts, particles, trails, lines, joints, and incompatible components in a UI Toolkit system. VRChat mobile limits are applied per target rank, with auto-culled categories (e.g. face tracking, joints) locked for safety but still visible for forensic review.
* **Texture Processing System:** Lists all detected textures with resolution metadata and per-texture toggles, allowing you to selectively opt out of Magick.NET processing for assets you want to preserve at full fidelity.

### 5. Avatar Pipeline & Iteration Tools

The connective tissue that makes re-rigging, mounting, and animating avatars repeatable instead of error-prone.

* **PhysBone Blueprints:** Snapshots an entire PhysBone setup into a reusable blueprint. "Extract Master Copy" walks the source avatar via `AnimationUtility.CalculateTransformPath`, serializes each `VRCPhysBone` into a `Preset`, and writes a master `ScriptableObject` mapping `bonePath` to `Preset`. "Inject Blueprint" traverses a target avatar by relative path, auto-adds any missing components, and re-applies the stored presets, restoring dozens of distinct physics setups in a single click after a destructive optimization or Quest conversion. Degrades gracefully (buttons disabled, clearly labeled) when the VRChat SDK is not present.
* **Material Conflict Finder:** Diagnoses avatars whose materials have drifted out of sync with each other. `AnalyzeTarget` collects materials from every renderer, then walks the animator side through `baseAnimationLayers` and `specialAnimationLayers` on the `VRCAvatarDescriptor`, every `RuntimeAnimatorController` it finds, and any clips carried by VRCFury components. It reports four sections. **Cross-Material Property Mismatches** groups toggle-style float properties (`[Toggle]` attribute, or names containing `toggle`/`enable`, or prefixed `_Use`/`_Is`) by the values they hold across materials, and settles a group with **Sync All to 0.0 (OFF)**, **Sync All to 1.0 (ON)** or **Align Majority**, with **Select All** and **Ping** to reach the materials. **Orphaned Shader Keywords** catches a toggle sitting at 0 while its `Toggle(KEYWORD)` keyword is still enabled, fixed per row with **Fix** or in bulk with **Fix All Keywords**. **Animation & VRCFury Driven Toggle Conflicts** catches a clip driving a toggle above 0 on a material whose keyword is statically disabled, so the animation renders nothing. **Scanned Materials Inventory** lists everything the pass touched. Locked materials are read through their `OriginalKeywords` tag rather than `IsKeywordEnabled`, and properties the locker marked animated are skipped, so a locked avatar does not report one false conflict per animated toggle.
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

The central command console and documentation layer for the whole toolbox.

* **Vixen Hub Dashboard:** A UI Toolkit-powered control center with tabs for News, Overview, Core Modules, Supported Modules, Network, Support and Changelogs. World projects gain an eighth tab, Metrics Engine. All major tools are launched from here with consistent styling and layout.
* **Dynamic Changelog Viewer:** Parses `CHANGELOG.md` into version-indexed entries with a dropdown selector. The Hub's update badge routes directly into this tab so you can see exactly what changed in each release.
* **Core Modules Grid:** Launchers for the tools your project can actually run. Animation Workbench Pro and the Pipeline Preset Manager always appear; avatar projects add Badge Studio, the Quest Conversion Engine, the Optimization Suite, PhysBone Blueprints, Animator Forge, the Material Conflict Finder and the Accessory Mounting Engine; world projects add the Vixen World Engine plus Live Surface Snapping and Precision Click-to-Place, which show their on/off state right on the card.
* **Network & Routing:** Centralized routing to our active channels: **GitHub** (source and issue tracking), **Discord**, **X/Twitter** and **YouTube**. Storefront and donation links live on the Support tab.
* **In-Engine Documentation:** Bypassing external websites entirely, the Hub features a dynamic Markdown-to-UIElements parser. The News, Overview and Changelog tabs render straight from the package's own markdown, and world projects get the full Heuristics Engine breakdown on the Metrics Engine tab.

### 9. VixenWear Latex Ultra

Our dual-lobe PBR shader for synthetic materials is a **standalone product** and does not ship inside this package. The Avatar Validator still recognises `VixenWear/Latex Ultra` materials, so avatars wearing it keep their packed-map checks.

> Every option in every inspector tab is documented control-by-control, with full editor screenshots, in the [Shader Docs](https://vixencreations.github.io/VixenToolBox/shaderdocs.html).

### 10. Third-Party Components

* **Magick.NET `14.16.0`** (ImageMagick `7.1.2-29`), by Dirk Lemstra, under the Apache-2.0 licence. This is the imaging engine behind every texture resize, badge composite, and VRAM pass in the toolbox. It ships inside the package as an Editor-only Windows x64 plugin, so nothing about it reaches your avatar or world build.

### 11. Special Thanks & Acknowledgements

This project requires immense R&D and community synergy. Massive thanks to the following creators for their crucial insights and forensic pipeline debugging for keeping the engine running:

* **Lt_Shadow:** Suggested the UI Toolkit font replacer infrastructure.
* **TheCastle:** Informed me about the underlying issues with LTCGI, driving our deadlock resolution.
* **ValenVRC:** Caught the Udon network heuristics being too vague, leading directly to the invention of our serialization ghost-component detector.
* **KittehKun:** Suggested the early naming and wording schemes being more streamlined.
* **DJ Red_Panda:** Suggested advanced LOD fixes and other critical miscellaneous pipeline items.
* **RBN's World Creators:** Suggested specific mechanics for the VRAM estimation matrices.
* **flickfluff:** Caught the Avatar Validator failing to strip Quest face-tracking components, directly inspiring the "Hunter-Killer" Biometric Purge.
