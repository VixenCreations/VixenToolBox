# VixenTools Executive Summary

VixenTools is a comprehensive, centralized suite of Unity Editor utilities designed to streamline avatar development, world creation, and asset optimization pipelines for VRChat creators.

### **1. Core Infrastructure & Hub**

  * **VixenTools Hub**: A centralized developer dashboard providing quick access to documentation, video guides, and community repositories.
  * **Pipeline Preset Manager**: Handles bulk extraction of configuration presets from existing assets and the programmatic authoring of standardized importer settings (e.g., 4K texture standards) using a "Phantom Asset" architecture.
  * **Animation Workbench Pro**: An advanced visual workspace for staging, easing, and sampling complex animation curves and material property bindings.

### **2. Avatar Physics & Topology**

  * **PhysBone Topology Mapper**: Automates the extraction and injection of complete PhysBone architectures.
  * **PhysBone Blueprints**: Scriptable assets that store exact skeletal paths and presets, allowing developers to reconstruct master physics matrices across different avatar versions or models.

### **3. Map & Environment Generation**

  * **Procedural Architecture**:
      * **Modular Room Builder**: A procedural generator for world-space interiors featuring customizable dimensions, door/window configurations, and style profile blending.
      * **Victorian Prefab Factory & Mansion Architect**: Specialized tools for generating complete Victorian-style estate kits and generating entire mansions on a modular grid.
  * **Natural Terrain**:
      * **Realistic Mesh Terrain**: A voxel-based generator using 3D Perlin noise to create organic landscapes with caves, boulders, and scatter-growth trees.
      * **Roof Builder**: Calculates and generates sloped roof panels with adjustable pitch, overhang, and thickness for existing house bases.
  * **Advanced Shape Generators**: Includes a Filler Shape Generator for creating randomized structural variants like pillars, slabs, arches, and "chaos blends".

### **4. Rendering & Material Suite**

  * **Vixen Realistic Suite**: High-performance PBR and Glass shaders integrated with the Redsim LightVolumes API.
  * **Volumetric Systems**:
      * **Raycaster Generator**: Automates the creation of raymarched volumetric light shafts linked to directional lights.
      * **Light Matrix Binder**: A script-driven utility that syncs light-space projection matrices for real-time volumetric rendering.
  * **Keyword Optimization**: Includes build-time postprocessors that automatically strip unused shader keywords to optimize material performance and texture memory.

### **5. VRChat Optimization & Porting**

  * **World Texture Optimizer**: Applies platform-specific resolution and compression policies (PC vs. Quest) based on texture roles (e.g., Albedo, Normal, Mask).
  * **Quest Shader Replacer**: Automatically swaps non-compatible materials for optimized mobile variants while attempting to preserve common properties.
  * **GI & Scene Prep**: Tools for forcing lightmap-only GI rendering and preparing active scenes for Light Volume usage on PC and Quest.
