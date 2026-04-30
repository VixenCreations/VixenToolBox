***

# VixenTools

I built VixenTools because the VRChat avatar pipeline is full of repetitive, mind-numbing bottlenecks. 

This is a comprehensive suite of custom Unity Editor utilities and automation scripts focused specifically on High-Fidelity Avatar Pipeline & Topology Architecture. Engineered entirely from the ground up on Unity's modern UI Toolkit, the goal is simple: eliminate human error, enforce strict project consistency, and push the Unity engine to its absolute limits. Whether you are violently crushing a PC avatar's polycount or seamlessly converting a heavily-weighted hierarchy for Quest, this toolset is designed to save you hundreds of hours of grinding so you can actually focus on creating.

### 1. Architectural Distribution & Infrastructure
* **VPM-Native Architecture:** Distributed exclusively via a custom VRChat Creator Companion (VCC) repository so your packages stay seamlessly updated within your existing workflow.
* **Context-Aware Dual-SDK Ecosystem:** The core architecture natively bridges both the VRChat Avatar SDK (`VRC_SDK_VRCSDK3`) and the VRChat World SDK (`UDON`). The central Hub autonomously reads active compiler directives upon project initialization and seamlessly hot-swaps the "Flagship Tools" UI matrix to match your current development environment.
* **Airtight Compiler Sandboxing:** Features strict, cryptographic-level compiler isolation across the entire asset suite. World-exclusive utilities are mathematically locked behind `#if UDON` directives, while avatar-centric suites operate strictly within `#if !UDON` boundaries. This guarantees zero assembly bleed-over and prevents catastrophic `VRCAvatarDescriptor` reference bricks when migrating the toolbox across radically different project infrastructures.
* **Enterprise UI Toolkit Matrix:** The entire suite operates natively on a high-performance, persistent DOM tree. By utilizing a centralized `UiStyles` repository for CSS design tokens (`.uss`) and native class hijacking, VixenTools delivers a unified, cyber-noir desktop application experience completely free of legacy IMGUI redraw lag.
* **Autonomous Scene View HUD Notifier:** Workflow-breaking popup windows are dead. The engine passively detects version discrepancies via a custom CI/CD backend and injects a sleek, native UI Toolkit `Button` directly into the `sceneView.rootVisualElement`, ensuring perfect DPI scaling and absolute coordinate anchoring.

### 2. Avatar Optimization & 5D Spatial Welding
A multi-threaded, highly destructive execution matrix designed to violently crush PC avatar polycounts and physics limits while mathematically protecting high-fidelity facial topology.
* **5D Spatial Hash Welder:** Annihilates excessively heavy meshes (>15,000 polygons) using a proprietary 5-dimensional coordinate key (mapping XYZ position natively alongside U and V coordinates). This drastically crushes raw polycounts while mathematically guaranteeing the survival of critical texture seams.
* **Dual-Shielding Exclusion Matrix:** The Welder features an autonomous Surgical Exclusion Matrix that deploys two layers of defense during decimation. It automatically scans submesh material slots for high-detail keywords (eyes, visor, face) AND actively queries the Humanoid Armature for Kinematic Head/Neck bone weights, mathematically locking delicate facial topology entirely out of the decimation grid.
* **BlendShape Memory Recovery:** Executing a destructive mesh wipe inherently annihilates Unity's blendshape arrays. The Welder caches all 175+ blendshape frames (visemes, facial expressions) prior to mesh destruction, then mathematically averages the delta offsets across the newly fused vertices so animations perfectly survive the meltdown.
* **Interactive Physics Executioner:** Replaces blunt auto-cull algorithms with a surgical UI Toolkit panel. The PC Validator spawns an interactive matrix mapping every single physics component on the avatar. Sorted mathematically by hierarchy depth, it empowers creators to manually select and eradicate expendable leaf-node physics while visually protecting structural physics roots.
* **Deep Material Inspector Spider (VRAM Profiling):** Weaponizes `SerializedObject` data streams to pierce deeply through the Unity Inspector. It spiders through highly obfuscated components, including Animator controllers and VRCFury outfit toggles, to hunt down nested textures, ensuring the ImageMagick downsampling pipeline captures 100% of the avatar's actual memory footprint.
* **Dynamic PC Target Rank Matrix:** Eradicates static culling limits. Creators can explicitly target specific VRChat Performance Ranks (Excellent, Good, Medium, Poor), which instantly recalibrates the deep-scanning heuristic thresholds for Animators, Contacts, and PhysBones.

### 3. The Quest Conversion Engine
A fully non-destructive, enterprise-grade pipeline for converting PC avatars to Android/Quest. It generates an isolated prefab sandbox to ensure your base PC avatar is never irreversibly altered.
* **High-Fidelity Linear Downsampling:** Intercepts Unity's native importer to route PC textures through a custom Magick.NET Lanczos pipeline. Textures are temporarily shifted into `ColorSpace.RGB` (Linear) to prevent the crushing of highlights, followed by an `UnsharpMask(0.0, 0.5, 1.0, 0.05)` pass to recover micro-contrast details (fur, fabric weaves) before forcefully applying Android ASTC compression.
* **Base-Class Physics Targeting:** Scanners utilize foundational `<VRCPhysBoneBase>` and `<VRCPhysBoneColliderBase>` classes to capture 100% of internal physics variations, including VRCFury auto-conversions and legacy dynamic bone migrations.
* **IgnoreTransform Shield Piercing:** The topology erasure matrix actively parses every `ignoreTransforms` array inside PhysBones. If a bone is explicitly listed, the heuristic shield drops, allowing the Culler to properly identify and melt dead leaf bones.
* **Deep Material VRCFury Cloning:** Hunts down hidden materials inside VRCFury toggles and state behaviors via C# serialization loops. It non-destructively re-maps all hidden references to the newly generated Android material variants to prevent pink shader errors at runtime.
* **Macro Topology Overrides:** Features global "Keep All" and "Cull All" macro execution buttons within the interactive UI panels, establishing a highly streamlined triage workflow when parsing massive arrays of physics or textures.

### 4. Core Toolsets & Scene Utilities
* **PhysBone Topology Mapper:** The flagship utility for physics management. Eliminates the human error of manually dragging and dropping colliders across avatar iterations. Utilizes Master Blueprints to automate PhysBone architecture through an Extraction/Injection process mapping `AnimationUtility.CalculateTransformPath` directly to Unity `.preset` files.
* **Animation Workbench Pro:** An advanced visual workspace for staging, easing, and sampling complex animation curves and material bindings. Features interactive timeline ribbons, flex-wrapping grid layouts, and a heavily optimized math library (`EasingFunctions.cs`). Built with an "Explicit Intent Paradigm" to prevent destructive live-scene hijacking.
* **Live Surface Snapping (World SDK):** Stop manually dragging objects to the floor. This enterprise-grade scene utility uses Unity's native `Transform.hasChanged` tracking for low-overhead drag detection. Hit `Ctrl+Alt+S` and objects perfectly snap to the surface by their geometric "feet," completely immune to self-collision.
* **Pipeline Preset Manager:** Enforce strict project consistency by bulk-extracting configuration presets from existing assets to programmatically author standardized importer settings via a non-destructive "Phantom Asset" architecture.

### 5. Identity Pipelines & Vixen Badge Studio
A CSS-driven procedural generation engine for authoring and compositing VRChat convention badges (natively supporting Furality Luma, Somna, Sylva, and Umbra).
* **Live Scene UV Mapper:** Hijacks the Scene View to map custom coordinates in 3D space interactively. Temporarily suspends Unity gizmos, raycasts against the badge `MeshCollider`, and mathematically inverts the UV hits into ImageMagick pixel space.
* **Programmatic Template Authoring:** Ingest raw convention textures or generate procedural bases. The studio automatically transcodes assets, scaffolds the required directory architecture, and drops a persistent `layout.json` to handle future UV bounds and target shaders.
* **Multi-Channel Emissive Compositing:** The ImageMagick rendering engine flattens and strips alpha channels during `_EMI` mask creation to prevent blowout loops. Features decoupled UI toggles to target glow specifically to Pronouns or Display Names, without washing out VRChat's post-processing bloom.
* **Universal Shader Targeting:** A heuristic material engine that automatically detects and injects generated parameters into Poiyomi Toon, lilToon, VRChat Mobile, and legacy convention shaders.

***