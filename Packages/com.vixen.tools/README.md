***

# VixenTools Ecosystem

VixenTools is a comprehensive suite of Unity Editor utilities and automation pipelines focused specifically on **Avatar Pipeline & Topology Architecture**. Designed to eliminate human error and enforce strict consistency, this toolset streamlines complex VRChat avatar development and asset optimization.

### **1. Distribution & Infrastructure**
* **VPM-Native Architecture:** Distributed exclusively via a custom VRChat Creator Companion (VCC) repository.
* **Strict Compiler Safeguards:** Assembly Definitions (`.asmdef`) and `#if UNITY_EDITOR` directives deeply isolate all editor-only scripts, ensuring zero compiler bleed-over when users build their avatars or worlds for runtime.

### **2. Core Infrastructure & Utilities**
* **VixenTools Hub:** A centralized developer dashboard providing quick access to documentation, updates, and community routing.
  * [GitHub Repository](https://github.com/VixenCreations)
  * [YouTube Channel](https://www.youtube.com/@vixenlicous)
  * [X (Twitter)](https://x.com/VixenVRC)
* **Animation Workbench Pro:** An advanced visual workspace for staging, easing, and sampling complex animation curves and material property bindings. Features interactive timeline ribbons, a real-time preview engine, and a heavily optimized, runtime-safe math library (`EasingFunctions.cs`) for flawless curve generation.
* **Pipeline Preset Manager:** Handles bulk extraction of configuration presets from existing assets and the programmatic authoring of standardized importer settings using a "Phantom Asset" architecture.
* **Fix Scene Data:** A dedicated utility for repairing, standardizing, and maintaining active scene integrity.
* **In-Editor Changelog:** A custom Markdown parser that renders the package's `CHANGELOG.md` directly inside Unity with native rich-text and ecosystem branding.

### **3. Avatar Physics & Topology**
* **PhysBone Topology Mapper:** The flagship utility for physics management. Automates PhysBone architecture through a two-phase **Extraction** and **Injection** process.
* **Master Blueprints:** Utilizes `AnimationUtility.CalculateTransformPath` and Unity `.preset` files to bypass native prefab constraints, allowing developers to map and reconstruct complex physics matrices seamlessly across different avatar versions or base models.