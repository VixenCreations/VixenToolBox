# VixenTools Ecosystem

VixenTools is a comprehensive suite of Unity Editor utilities and automation pipelines focused specifically on **Avatar Pipeline & Topology Architecture**. Designed to eliminate human error and enforce strict consistency, this toolset streamlines complex VRChat avatar development and asset optimization.

### **1. Distribution & Infrastructure**
* **VPM-Native Architecture:** Distributed exclusively via a custom VRChat Creator Companion (VCC) repository.
* **Strict Compiler Safeguards:** Assembly Definitions (`.asmdef`) and `#if UNITY_EDITOR` directives deeply isolate all editor-only scripts, ensuring zero compiler bleed-over when users build their avatars or worlds for runtime.

### **2. Core Infrastructure & Utilities**
* **Vixen Hub:** A centralized, tabbed developer dashboard unifying ecosystem documentation and the native `CHANGELOG.md` parser into a single interface. Features a custom UV-framing engine for dynamic header scaling and quick community routing.
  * [GitHub Repository](https://github.com/VixenCreations)
  * [YouTube Channel](https://www.youtube.com/@vixenlicous)
  * [X (Twitter)](https://x.com/VixenVRC)
* **Animation Workbench Pro:** An advanced visual workspace for staging, easing, and sampling complex animation curves and material property bindings. Features interactive timeline ribbons, a real-time preview engine, and a heavily optimized, runtime-safe math library (`EasingFunctions.cs`) for flawless curve generation.
* **Pipeline Preset Manager:** Handles bulk extraction of configuration presets from existing assets and the programmatic authoring of standardized importer settings using a "Phantom Asset" architecture.
* **Fix Scene Data:** A dedicated utility for repairing, standardizing, and maintaining active scene integrity.

### **3. Avatar Physics & Topology**
* **PhysBone Topology Mapper:** The flagship utility for physics management. Automates PhysBone architecture through a two-phase **Extraction** and **Injection** process.
* **Master Blueprints:** Utilizes `AnimationUtility.CalculateTransformPath` and Unity `.preset` files to bypass native prefab constraints, allowing developers to map and reconstruct complex physics matrices seamlessly across different avatar versions or base models.

### **4. Convention & Identity Pipelines**
* **Vixen Badge Studio:** A high-fidelity, procedural generation engine for authoring and compositing VRChat convention badges (natively supporting Furality Luma, Somna, Sylva, and Umbra).
  * **Ecosystem Discovery Engine:** Utilizes recursive deep-scanning to dynamically locate Furality SDK assets, bypassing inconsistent year-to-year folder restructuring or missing directories.
  * **Universal Shader Targeting:** A heuristic material engine that automatically detects and configures targets across Poiyomi Toon, lilToon, VRChat Mobile, and legacy convention shaders. It seamlessly manages emission map injection and intercepts blackout override multipliers.
  * **Programmatic Template Authoring:** Allows users to ingest raw convention `.jpg` textures or generate procedural bases, automatically transcoding assets and scaffolding the required directory and material architecture.
  * **Dynamic UV Auto-Layout:** Intercepts convention selections and automatically snaps internal text bounds, rotational math, and signature neon hex colors to perfectly map onto the specific year's 3D mesh.