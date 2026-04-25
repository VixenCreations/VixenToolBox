***

# VixenTools

I built VixenTools because the VRChat avatar pipeline is full of repetitive, mind-numbing bottlenecks. 

This is a comprehensive suite of custom Unity Editor utilities and automation scripts focused specifically on **Avatar Pipeline & Topology Architecture**. Engineered entirely on Unity's modern UI Toolkit, the goal is simple: eliminate human error, enforce strict project consistency, and push the Unity engine to its absolute limits. Whether you are optimizing a high-fidelity model or converting an entire hierarchy for Quest, this toolset is designed to save you hundreds of hours of grinding so you can actually focus on creating.

### **1. Distribution & Enterprise Infrastructure**
* **VPM-Native Architecture:** Distributed exclusively via a custom VRChat Creator Companion (VCC) repository so your packages stay seamlessly updated.
* **Enterprise UI Toolkit Matrix:** The entire suite operates on a high-performance, persistent DOM tree. By utilizing centralized CSS design tokens (`.uss`), VixenTools delivers a unified, responsive, cyber-noir desktop application experience completely free of legacy IMGUI lag spikes and clipping.
* **Strict Compiler Safeguards:** All editor-only scripts are aggressively wrapped in Assembly Definitions (`.asmdef`) and `#if UNITY_EDITOR` directives. This guarantees zero compiler bleed-over when you build your avatars or worlds for runtime.

### **2. Core Toolsets & Utilities**
* **Vixen Hub:** Your central control matrix. A 5-tab developer dashboard featuring an autonomous `[InitializeOnLoadMethod]` update prompter that passively detects new VPM releases. It features a custom native Markdown-to-UIToolkit parser that unifies ecosystem documentation, changelogs, and direct triage routing right inside the editor.
  * [GitHub Repository](https://github.com/VixenCreations)
  * [YouTube Channel](https://www.youtube.com/@vixenlicous)
  * [Gumroad Storefront](https://vixencreations.gumroad.com/)
* **Animation Workbench Pro:** An advanced visual workspace for staging, easing, and sampling complex animation curves and material bindings. Features interactive timeline ribbons, flex-wrapping grid layouts, and a heavily optimized, runtime-safe math library (`EasingFunctions.cs`) for flawless curve generation.
* **Pipeline Preset Manager:** Handles bulk extraction of configuration presets from existing assets and the programmatic authoring of standardized importer settings using a "Phantom Asset" architecture.
* **Live Surface Snapping:** Stop manually dragging objects to the floor. This enterprise-grade scene utility uses Unity's native `Transform.hasChanged` tracking for low-overhead drag detection. Hit `Ctrl+Alt+S` and objects perfectly snap to the surface by their geometric "feet," completely immune to self-collision.
* **Fix Scene Data:** A dedicated utility for repairing and maintaining active scene integrity by forcefully serializing unlinked lightmap references.

### **3. Avatar Physics & Cross-Platform Topology**
* **Quest Conversion Engine:** A fully non-destructive, high-fidelity pipeline for converting PC avatars to Android/Quest. 
  * **ImageMagick Lanczos Pipeline:** Intercepts Unity's native texture importer to route PC textures through a high-fidelity Magick.NET downsampling pass before forcefully applying Android ASTC compression. It preserves crisp, glossy details within strict VRAM limits.
  * **Heuristic PhysBone Culling:** Mathematically calculates skeletal depth to aggressively cull deep leaf bones while preserving vital root-level physics based on your targeted Mobile Performance Rank.
  * **Interactive Topology Matrix:** A dynamic UI control panel driven by a custom DOM bridge that instantly spawns hierarchy results post-scan. It automatically applies heuristic culling limits and lets you manually override exactly which physics components survive the Quest conversion without Editor lag.
  * **High-Fidelity Material Translation:** Natively hunts down PC-side metallic, gloss, normal, and emission properties across third-party shaders (Poiyomi, lilToon) and natively maps them into `VRChat/Mobile/Toon Standard` to preserve maximum visual depth.
* **PhysBone Topology Mapper:** The flagship utility for physics management. It completely automates PhysBone architecture through a two-phase **Extraction** and **Injection** process, featuring graceful UI degradation if the VRChat SDK is missing.
* **Master Blueprints:** Bypasses native prefab constraints using `AnimationUtility.CalculateTransformPath` and Unity `.preset` files, allowing you to map and reconstruct complex physics matrices seamlessly across different avatar versions.

### **4. Convention & Identity Pipelines**
* **Vixen Badge Studio:** A high-fidelity, CSS-driven procedural generation engine for authoring and compositing VRChat convention badges (natively supporting Furality Luma, Somna, Sylva, and Umbra).
  * **Ecosystem Discovery Engine:** Utilizes recursive deep-scanning to dynamically locate Furality SDK assets, bypassing their inconsistent year-to-year folder restructuring.
  * **Universal Shader Targeting:** A heuristic material engine that automatically detects and configures targets across Poiyomi Toon, lilToon, VRChat Mobile, and legacy convention shaders.
  * **Programmatic Template Authoring:** Ingest raw convention `.jpg` textures or generate procedural bases. The studio automatically transcodes assets, scaffolds the required directory/material architecture, and drops a persistent `layout.json` for future modifications.
  * **Dynamic UV Auto-Layout:** Automatically snaps internal text bounds, rotational math, and signature neon hex colors to perfectly map onto the specific year's 3D mesh.

***