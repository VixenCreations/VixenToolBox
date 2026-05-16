# VIXENWEAR : SHADER USAGE

The **VixenWear Latex Ultra** is a high-fidelity, dual-lobe PBR surface shader engineered for complex synthetic materials. It bypasses standard Unity lighting models to provide true tangent-space micro-shadowing, hardware tessellation, dynamic thin-film interference, and fully integrated SM5 hardware processing for AudioLink and VRSL DMX.

To achieve flawless high-gloss black materials, deep atmospheric reflections, and reactive neon emissives, you must adhere to this pipeline architecture.

---

## 1. TEXTURE PACKING (THE PBR MAP)

To maximize VRAM efficiency and guarantee precise pixel registration, this shader requires a rigorously packed PBR map. Do not use standard Unity channel packing.

Assign your channels in Substance Painter or the Vixen Pipeline Manager as follows:

* **Red (R): Metallic** (White = Full Metal, Black = Dielectric)
* **Green (G): Ambient Occlusion** (White = Flat, Black = Deep Shadow)
* **Blue (B): Displacement / Height** (White = Extruded, Black = Recessed)
* **Alpha (A): Smoothness** (White = High-Gloss, Black = Rough)

---

### SUBSTANCE PAINTER EXPORT WORKFLOW

By default, Substance Painter exports standard Unity HDRP or URP maps, which do not match our custom PBR requirement. You must build a custom output template.

**Step 1: Setting up the Output Template**

* Go to `File` > `Export Textures` (or press `Ctrl+Shift+E`).
* Click the `Output Templates` tab at the top.
* Click the `+` button to create a new template and name it `VixenWear PBR`.
* Under the Output Maps section, click the **`R+G+B+A`** button. This creates a new blank texture file that supports an Alpha channel.
* Rename the new file structure to something like `$mesh_$textureSet_PackedPBR`.

**Step 2: Routing the Data Channels**
Drag the correct data maps from the right-side panels into the RGB+A boxes of your new texture. When prompted, always select **Grayscale Channel**.

* **R Box (Metallic):** From `Input Maps`, drag in **`Metallic`**.
* **G Box (Ambient Occlusion):** From `Input Maps`, drag in **`Ambient occlusion`** or `Mixed AO`.
* **B Box (Displacement):** From `Input Maps`, drag in **`Height`**.
* **A Box (Smoothness):** *Crucial Step:* Substance natively works in "Roughness", but Unity needs "Smoothness". Go to **`Converted Maps`** and drag **`Glossiness`** into the A box.

**Step 3: Export Settings**
Return to the `Settings` tab in the Export window.

* **Output Template:** Select your `VixenWear PBR` template.
* **File Format:** You **MUST** select a format that supports an Alpha channel (**PNG, Targa/TGA, or TIFF**). JPEGs will delete the Alpha channel and destroy your smoothness data.
* **Bit Depth:** 8-bit is standard. If your displacement looks "stepped" or stair-cased in Unity, export this specific map as **16-bit**.

**Step 4: The Unity Import Rule (Critical)**
Because this map contains purely mathematical data and not color, Unity's default settings will warp the displacement and metalness.

* Drag your exported packed map into your Unity project.
* Click the image in your Project window.
* In the Inspector, **uncheck `sRGB (Color Texture)**`.
* Hit **Apply**.

---

## 2. PARALLAX & MICRO-SHADOWING

The shader utilizes a 50-step High-Fidelity Gradient Raymarcher mapped to the **Blue (Height)** channel of your Packed PBR map.

* **Parallax Depth:** Controls the structural extrusion of the fabric.
* **Micro-Shadow Hardness:** Calculates physical light intersection against the raymarched heightmap. This creates true tangent-space shadows inside the recessed seams of your geometry.
* **Hardware Tessellation:** If the GPU supports DX11/Tessellation, the geometry is physically sub-divided up to 50 times based on the `Edge Length` and physically extruded using the `Displacement Strength`.

---

## 3. CLEARCOAT & THIN-FILM INTERFERENCE

The "Latex" visual relies on a dual-lobe specular BRDF. The base fabric utilizes standard roughness, while a secondary **Clearcoat** layer floats above it, calculating independent environmental reflections and fresnel curves.

* **Clearcoat Smoothness:** Pushing this to `0.95+` creates the iconic, liquid-like specular finish.
* **Specular AA Variance:** Suppresses specular aliasing (glinting/flickering) on high-gloss meshes when the camera moves. Keep between `0.10` and `0.20`.
* **Base Polish Layer Thickness (nm):** The physical thickness of the clearcoat, calculated in nanometers. Altering this value shifts the refractive iridescence (oil-slick/holographic effect) across the light spectrum.
* **Iridescence Strength:** Blends the Thin-Film light separation into the clearcoat reflection.

---

## 4. NORMALS & MICRO-DETAIL

For high-fidelity synthetics, macro-geometry is handled by displacement, while physical texturing (leather grain, latex pores) is handled by the normal maps.

* **Clearcoat Normal Flattening:** Allows the clearcoat reflection to remain smooth like liquid, even if the underlying fabric normal map is extremely rough.
* **Detail Normal:** A secondary tiled normal map used for micro-pores or hex-grid patterns. Use the `Detail UV Tiling` to scale it independently of your main texture.

---

## 5. ECOSYSTEM INTEGRATIONS

**EMISSION & MATCAP ENVIRONMENT**
For glowing neon cyan or hot pink aesthetic accents, drive the `Emission Color` via HDR values. The `MatCap` system provides fallback low-light studio lighting, allowing you to force specific rim-lights or reflections even in completely unlit VRChat worlds. Use `Matcap Lighting Mix` to govern how much the MatCap respects realtime shadows.

**VRC LIGHT VOLUMES & LTCGI**
Native implementations for both realtime polygonal area lighting (LTCGI) and baked Spherical Harmonic volumes (Light Volumes). The custom BRDF integrates LTCGI directly into the Clearcoat specular lobe, ensuring area lights reflect flawlessly off high-gloss surfaces.

**VRSL STAGE HIJACK PROTOCOL**
This shader intercepts world-space DMX buffers to mimic a **Standard 13-Channel Moving Head Fixture**. By assigning a `DMX Base Channel` that matches a venue's spotlights (e.g., Ch 1, 14, or 27), the material will:

* Override native emission with the stage's RGB and Strobe data.
* **Kinetic Geo-Warping:** Utilize DMX Pan (Base+1) and Tilt (Base+2) to physically warp and bend the tessellated vertices of the avatar toward the active light beam.

---

## 6. GOD TIER CYBERNETICS (HUD OVERLAYS)

A built-in diagnostic and cybernetic UI system. Provide a black-and-white mask to `B&W Window Mask` to define where the HUD displays on your avatar. Each overlay can be independently scaled, rotated, and positioned inside this window.

* **VU Meter:** A segmented volume unit readout mapped to a specific AudioLink band.
* **Spectrum:** A raw 64-pixel EQ strip displaying the full frequency spectrum.
* **Waveform:** An oscilloscope line reading the raw AudioLink waveform buffer.
* **DMX Grid:** A real-time grid visualizing the VRSL DMX universe activity.
* **AutoCorrelator Mask Warp:** Physically distorts the HUD window's UVs based on audio pitch.

---

## 7. KINETIC UV ENGINE (SURFACE MAPS)

Transforms the mathematical surface of the material without altering the mesh.

* **Vortex Twist:** Swirls the local UVs into a spiral based on audio intensity.
* **UV Bass Pump:** Scales the texture inward/outward to simulate speaker cone thumping.
* **UV Fracture Shard:** Uses spatial hashing to brutally tear and misalign the texture coordinates along a grid structure.

---

## 8. KINETIC VERTEX ENGINE (DUAL-PASS GEOMETRY SHATTER)

Utilizes a raw Shader Model 5.0 Geometry pipeline to mathematically destroy the mesh topology in real-time.

* **Normal Inflate Distance:** Pushes the welded vertices outward along the normals to physically inflate the avatar to the bass.
* **Geometry Shard Scatter:** When triggered, Pass 1 clips the affected polygons from the base suit. Pass 2 intercepts them, calculates their true centroids, un-welds the topology, and physically blasts the literal polygons outward into 3D space.
* *Note:* To maximize the visual effect, fractured geometry sheds its physical PBR properties and converts to raw kinetic energy (Unlit Emissive) during flight.



---

## 9. GLOBAL MATERIAL MODULATIONS

Secondary atmospheric effects driven by AudioLink FFT arrays.

* **Audio Scanlines:** Scrolling CRT-style horizontal bands driven by track speed and amplitude.
* **Parallax Thump:** Dynamically deepens the raymarched parallax mapping to the beat.
* **Clearcoat Shatter:** Destroys the smoothness value of the clearcoat on high frequencies.
* **Thin Film Expansion:** Pulses the nanometer thickness of the iridescence to shift refractive colors dynamically.
* **Digital Tear:** Micro-stutters and slices the UV map laterally to simulate codec failure.