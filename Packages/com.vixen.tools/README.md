***

# VixenTools

I built VixenTools because the VRChat avatar pipeline is full of repetitive, mind-numbing bottlenecks. 

This is a comprehensive suite of custom Unity Editor utilities and automation scripts focused specifically on High-Fidelity Avatar Pipeline & Topology Architecture. Engineered entirely from the ground up on Unity's modern UI Toolkit, the goal is simple: eliminate human error, enforce strict project consistency, and push the Unity engine to its absolute limits. Whether you are optimizing a complex hierarchy or seamlessly converting a heavily-weighted PC avatar for Quest, this toolset is designed to save you hundreds of hours of grinding so you can actually focus on creating.

### 1. Distribution & Enterprise Infrastructure
* **VPM-Native Architecture:** Distributed exclusively via a custom VRChat Creator Companion (VCC) repository so your packages stay seamlessly updated within your existing workflow.
* **Enterprise UI Toolkit Matrix:** The entire suite operates natively on a high-performance, persistent DOM tree. By utilizing a centralized `UiStyles` repository for CSS design tokens (`.uss`) and the `Cyberpunk-Regular` font core, VixenTools delivers a unified, responsive, cyber-noir desktop application experience completely free of legacy IMGUI redraw lag and layout clipping.
* **Dynamic DOM Bridges:** Interfaces dynamically sync variable states and spawn interactive configuration matrices instantly upon window resize, executing complex data loops without triggering Editor-wide hang-ups.
* **Autonomous Scene View HUD Notifier:** Workflow-breaking popup windows are dead. The engine passively detects version discrepancies via a custom CI/CD backend and injects a sleek, non-blocking `>> VIXENTOOLS UPDATE` button directly into the corner of your active Unity Scene View.

### 2. Core Toolsets & Utilities
* **Vixen Hub:** Your central control matrix. A developer dashboard featuring a custom, native Markdown-to-UIToolkit parser that unifies ecosystem documentation, changelogs, and triage routing directly inside the Unity Editor (safely sanitizing unicode into terminal-compliant chevron syntax).
* **Animation Workbench Pro:** An advanced visual workspace for staging, easing, and sampling complex animation curves and material bindings. Features interactive timeline ribbons, flex-wrapping grid layouts, and a heavily optimized math library (`EasingFunctions.cs`) for flawless curve generation.
* **Live Surface Snapping:** Stop manually dragging objects to the floor. This enterprise-grade scene utility uses Unity's native `Transform.hasChanged` tracking for low-overhead drag detection. Hit `Ctrl+Alt+S` and objects perfectly snap to the surface by their geometric "feet," completely immune to self-collision.
* **Pipeline Preset Manager:** Handles bulk extraction of configuration presets from existing assets and the programmatic authoring of standardized importer settings using a non-destructive "Phantom Asset" architecture.

### 3. The Quest Conversion Engine
A fully non-destructive, high-fidelity pipeline for converting PC avatars to Android/Quest. It maps 100% of VRChat's Android Performance Limits natively.
* **High-Fidelity Linear Downsampling:** Intercepts Unity's native importer to route PC textures through a custom Magick.NET Lanczos pipeline. Textures are temporarily shifted into `ColorSpace.RGB` (Linear) during downsampling to prevent the mathematical darkening/crushing of highlights, followed by an `UnsharpMask(0.0, 0.5, 1.0, 0.05)` pass to recover micro-contrast details (fur, fabric weaves) before forcefully applying Android ASTC compression.
* **Interactive Texture Culling:** A dedicated DOM matrix displays all parsed textures prior to conversion, allowing creators to selectively bypass ImageMagick processing on specific textures to massively reduce compile times on redundant avatars.
* **Incompatible Mobile Component Purge:** Actively hunts down objects globally banned on VRChat Mobile (Cameras, Lights, AudioSources, Cloth, Rigidbodies, Unity Colliders). These populate in a locked "Auto-Culled" matrix, enforcing guaranteed removal.
* **Root Animator Protection:** A recursive depth scanner detects the avatar's core root Animator, hard-locking it into a "Kept" state to ensure heuristic topology culling never accidentally shatters the avatar's base locomotion.
* **Deep Heuristic Culling:** Mathematically calculates skeletal depth to aggressively cull deep leaf bones, colliders, contacts, and raycasts while preserving vital root-level physics based strictly on your targeted Mobile Performance Rank (Excellent to Poor). Let the engine handle the math, or manually override the Interactive Topology Matrix before execution.
* **Advanced Material Translation:** Natively hunts down PC-side metallic, gloss, normal, and emission properties across standard shaders and third-party variants (seamlessly mapping non-standard properties like `_MochieMetallicMaps` directly to Mobile Standard `_MetallicMap`).

### 4. Identity Pipelines & Vixen Badge Studio
A CSS-driven procedural generation engine for authoring and compositing VRChat convention badges (natively supporting Furality Luma, Somna, Sylva, and Umbra).
* **Live Scene UV Mapper:** Hijack the Scene View to map custom coordinates in 3D space. It temporarily suspends Unity gizmos (`Tools.current = Tool.None`), raycasts against the badge `MeshCollider`, mathematically inverts the UV hits into ImageMagick pixel space, and draws a neon debug indicator at the exact click intersection.
* **Programmatic Template Authoring:** Ingest raw convention textures or generate procedural bases. The studio automatically transcodes assets, scaffolds the required directory/material architecture, and drops a persistent `layout.json` to handle future UV bounds, text rotations, and target shaders.
* **Furality Layout Generator:** A dedicated developer utility natively integrated into Badge Studio to autonomously scaffold directories and format mathematically perfect `layout.json` boundary configurations for all Furality convention templates instantly.
* **Multi-Channel Emissive Compositing:** The ImageMagick rendering engine flattens and strips alpha channels during `_EMI` mask creation to prevent blowout loops. Features decoupled UI toggles to target glow specifically to Pronouns or Display Names, and a custom `ColorField` to precisely control the emissive mask color without washing out VRChat's post-processing bloom.
* **Universal Shader Targeting:** A heuristic material engine that automatically detects and injects generated parameters into Poiyomi Toon, lilToon, VRChat Mobile, and legacy convention shaders.

***