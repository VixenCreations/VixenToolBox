# VIXENWEAR : SHADER USAGE

The **VixenWear Latex Ultra** is a high-fidelity, dual-lobe PBR surface shader engineered for complex synthetic materials. It bypasses standard Unity lighting models to provide a full GGX BRDF stack (Burley diffuse, Smith-Joint visibility, Schlick Fresnel, Karis split-sum environment, optional Filament multi-scatter compensation), true tangent-space micro-shadowing, hardware tessellation, dynamic thin-film interference, anisotropic specular for stretched latex, thin-part transmission, and fully integrated SM5 hardware processing for AudioLink and VRSL DMX.

To achieve flawless high-gloss black materials, deep atmospheric reflections, and reactive neon emissives, you must adhere to this pipeline architecture.

---

## 1. RENDERING MODE SELECTOR

The inspector now exposes a Standard-shader-style `Rendering Mode` dropdown at the top of the BASE tab. Switching modes drives the underlying blend state, ZWrite, render queue, RenderType / VRCFallback tags, and alpha workflow keywords - no manual queue edits required.

* **Opaque (`0`):** Full PBR, no clip, no blend. Render queue `Geometry`, fallback `ToonDoubleSided`.
* **Cutout (`1`):** Hard clip on `_CutOff` (default). Shadows match silhouette via `addshadow`. Render queue `AlphaTest`, fallback `ToonCutoutDoubleSided`. This is the historical 2.1.x behavior.
* **Fade (`2`):** Straight alpha blend (`SrcAlpha / OneMinusSrcAlpha`). Everything - including specular - fades together. Render queue `Transparent`, fallback `ToonTransparentDoubleSided`.
* **Transparent (`3`):** Premultiplied alpha (`One / OneMinusSrcAlpha`). Specular highlights survive at low opacity - the correct mode for glass and wet-look latex membranes. Same queue/fallback as Fade.

The mode is wired all the way through the build pipeline: the editor's `SetupMaterialWithBlendMode` runs on `_Mode` change and shader assignment, and `SyncKeywords` re-applies the right alpha keyword on every build/play-mode transition so upgraded materials pick up the correct state without an inspector visit.

---

## 2. TEXTURE PACKING (THE PBR MAP)

To maximize VRAM efficiency, this shader uses a single RGBA-packed PBR mask. **The native packing has not changed**, but the engine now also supports drop-in compatibility with Poiyomi / Substance / Marmoset packings via per-channel selectors.

### Native Vixen Packing (Default)

* **Red (R): Metallic** (White = Full Metal, Black = Dielectric)
* **Green (G): Ambient Occlusion** (White = Flat, Black = Deep Shadow)
* **Blue (B): Displacement / Height** (White = Extruded, Black = Recessed)
* **Alpha (A): Smoothness** (White = High-Gloss, Black = Rough)

### Poiyomi / Substance Compatibility Layer

The SURFACE tab now exposes per-channel selectors under the Packed PBR Mask:

* `Metallic Channel` (R/G/B/A) + `Invert Metallic`
* `Smoothness Channel` (R/G/B/A) + `Channel Stores Roughness (Invert)` - flip if your packing follows Substance's roughness convention
* `AO Channel` (R/G/B/A)
* `Height Channel` (R/G/B/A)

The selector is honored everywhere the height/metallic/smoothness/AO values are consumed: parallax raymarch, BRDF parallax shadow trace, surface stage, and vertex displacement. Drop in a Poiyomi-packed mask, flip the four selectors, and you're done.

### Substance Painter Export Workflow (Native Pack)

If you're authoring fresh in Substance Painter, the native pack still gives you the cleanest pipeline.

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
* **A Box (Smoothness):** *Crucial Step:* Substance natively works in "Roughness", but the native pack uses "Smoothness". Go to **`Converted Maps`** and drag **`Glossiness`** into the A box. *(Alternatively, drag in `Roughness` and flip the `Channel Stores Roughness (Invert)` toggle in the inspector.)*

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

## 3. PARALLAX & MICRO-SHADOWING

The shader utilizes a 50-step adaptive Gradient Raymarcher mapped to the height channel of your Packed PBR map (channel chosen by `Height Channel`).

* **Parallax Depth:** Controls the structural extrusion of the fabric.
* **Shadow Hardness:** Calculates physical light intersection against the raymarched heightmap. This creates true tangent-space shadows inside the recessed seams of your geometry, coupled to the parallax displacement.
* **Hardware Tessellation:** If the GPU supports DX11/Tessellation, the geometry is physically sub-divided up to 50 times based on the `Tessellation Edge Length` and physically extruded using the `Displacement Strength`.

---

## 4. GGX BRDF & PHYSICAL CONTROLS

VixenWear no longer uses Unity's legacy `BRDF3` lighting model. Direct and indirect specular run through a full GGX stack:

* `D_GGX` (Trowbridge-Reitz) for the normal distribution.
* `V_SmithJointGGX` for masking/shadowing visibility.
* `F_Schlick` for Fresnel.
* `Burley_Diffuse` for direct diffuse (proper retroreflection at grazing angles).
* `EnvBRDFApprox_AB` (Karis split-sum) for environment specular - no per-mip BRDF integral required.
* `EnergyCompensation` (Filament/Frostbite) optionally adds back the energy lost to multi-scatter at high roughness. Toggle via `Multi-Scatter Energy Compensation` in POLISH.

### Geometric Specular AA

`Specular Anti-Aliasing` (`_CC_Spec_AA`) now drives a Toksvig-style filter (`GeometricSpecAA`) that roughens the base and clearcoat normals based on screen-space normal derivative variance. Eliminates the glinting/flickering on high-gloss meshes that hammered mid-distance silhouettes in older versions. Keep between `0.10` and `0.50` for noticeable suppression without losing micro-detail.

---

## 5. CLEARCOAT & THIN-FILM INTERFERENCE

The "Latex" visual relies on a dual-lobe specular BRDF. The base fabric utilizes its own GGX lobe, while a secondary **Clearcoat** layer floats above it, calculating independent environmental reflections and Fresnel curves.

* **Clearcoat Strength:** Master energy term for the entire coat. Drops to dielectric-base behavior at `0`.
* **Clearcoat Smoothness:** Pushing this to `0.95+` creates the iconic, liquid-like specular finish.
* **Clearcoat Flattening:** Lerps the shaded normal toward the smooth geometric normal so the coat reflection remains liquid even if the underlying fabric normal is extremely rough.
* **Clearcoat Tint:** New in 2.3.0. Coloured tints give the under-layer a complementary cast via per-channel `baseEnergy` attenuation - useful for oil-on-water/iridescent acrylic looks. White tint at `F0=0.04` reproduces the standard dielectric exactly.
* **Clearcoat F0:** New in 2.3.0. Custom Fresnel base reflectance for the coat. Bump above `0.04` for stronger surface reflections; useful for liquid glass or wet-resin finishes.
* **Base Polish Layer Thickness (nm):** The physical thickness of the clearcoat, calculated in nanometers. Altering this value shifts the refractive iridescence (oil-slick/holographic effect) across the light spectrum.
* **Thin Film Strength:** Blends the Thin-Film light separation into the clearcoat reflection.

---

## 6. ANISOTROPIC SPECULAR (LATEX STRETCH)

New in 2.3.0. The POLISH tab exposes:

* **Anisotropy (`_Aniso`):** -1 to 1. Negative values stretch the highlight along the bitangent; positive values along the tangent. `0` falls back to isotropic GGX.
* **Anisotropy Rotation (`_AnisoRot`):** 0-360 degrees. Rotates the world tangent around N before splitting roughness alpha into `ax` / `ay`, letting you align the stretch direction with the visible latex pull instead of the UV unwrap.

When `|aniso| > 0.005` the BRDF switches to `D_GGX_Aniso` + `V_SmithJointGGX_Aniso` (Burley 2012). Use sparingly - anisotropy is computationally heavier than the isotropic path because of the extra TBN dot products.

---

## 7. TRANSMISSION (THIN-PART BACK-LIGHT)

New in 2.3.0. For ears, fingers, latex membranes, and any thin geometry that should bleed back-light:

* **Transmission Strength (`_Trans_Str`):** Master amount.
* **Transmission Distance (`_Trans_Dist`):** Controls Beer-Lambert absorption (`exp(-(1-diffColor) / _Trans_Dist)`). Lower values produce a deeper, more saturated bleed; higher values let more of the light through.
* **Transmission Falloff (`_Trans_Power`):** Sharpness of the view-aligned back-light response. Higher values tighten the bleed to direct silhouettes.

Transmission is modulated by the live bio pulse so audio-reactive regions bleed brighter, and obeys clearcoat energy conservation via `baseEnergy`.

---

## 8. NORMALS & MICRO-DETAIL

For high-fidelity synthetics, macro-geometry is handled by displacement, while physical texturing (leather grain, latex pores) is handled by the normal maps.

* **Normal Strength:** Lerps between flat (`(0,0,1)`) and the unpacked normal so authors can dial back overly aggressive maps without rebaking.
* **Clearcoat Flattening:** Allows the clearcoat reflection to remain smooth like liquid, even if the underlying fabric normal map is extremely rough.
* **Detail Normal:** A secondary tiled normal map used for micro-pores or hex-grid patterns. Use the `Detail UV Tiling` to scale it independently of your main texture.

---

## 9. EMISSION, REGION MASKS & MULTI-LAYER MATCAPS

The INTEGRATION tab in 2.3.0 absorbs a major Poiyomi-style overhaul.

### Primary Emission

Drive the `Emission Color` via HDR values; the emission map RGB tints and its alpha is the master mask. Manual emission is locked to the masked regions (circuitry lines, panels) - never bleeds onto unmasked albedo.

### Secondary Emission Layer

* **Enable Secondary Emission Layer:** Activates an entire second emission stack.
* **Emission Map 2 / Color 2:** Independent texture and HDR tint.
* **Emission 2 Mask Channel:** Selects which channel of `Emission Map 2` to read as the mask (R/G/B/A).
* **Emission 2 AL Band / Amplitude:** Routes a *different* AudioLink band to this layer than the primary emission. Bass on one circuitry layer, treble on another, no animator wiring required.

### Multi-Region Color Mask

* **Enable Multi-Region Color Mask** + **Region Mask** texture (paint R / G / B zones).
* **Red / Green / Blue Zone Tint:** Multiplies into albedo inside each zone independently (overlapping zones stack).
* **Red / Green / Blue Zone Emission Boost:** Multiplies emission inside each zone - lets you brighten specific feature areas (panels, claws, paw-print decals) without a second emission map.

### Multi-Layer MatCap System

* **MatCap 1:** Now exposes `Mask Channel` (R/G/B/A) and per-layer `Tint` so a single RGB region mask can drive different matcaps in different zones.
* **MatCap 2 Layer:** New in 2.3.0. Full second matcap with own `Texture`, `Mask`, `Mask Channel`, `Tint`, `Intensity`, `Rotation`, and a `Blend Mode` (`Add` / `Replace` / `Multiply`).
* **Matcap Lighting Mix:** Governs how much the MatCap respects realtime shadows; same for both layers.

A common workflow is to drop the same red/blue/black region mask into both layers and pick `R` for layer 1, `B` for layer 2 - each zone shows a different matcap.

---

## 10. LIGHT VOLUMES & LTCGI (DEEP INTEGRATION)

VRC Light Volumes now expose a full integration stack instead of a single intensity slider:

* **Light Volumes Intensity:** Master mix.
* **Light Volumes Specular Mix:** Independent base-layer specular contribution from L1 directionality.
* **Light Volumes Specular (Dominant Mode):** Switches between full L1 specular and dominant-light specular (cheaper, better for stylized worlds).
* **Light Volumes Clearcoat Specular:** Clearcoat-specific specular contribution from L1.
* **Light Volumes Normal Bias:** Pushes the sample position along the world normal as `worldPosOffset` to fix light bleed at sharp edges (matches the official LV PBR reference shader).
* **Light Volumes Position Offset (Vector):** Manual world-space offset for thin or sleeve geometry where the bias alone isn't enough.
* **Additive-Only Mode:** Samples only additive volumes and layers them on top of Unity's probe diffuse. Use this when you want LV to enhance probe lighting instead of replacing it.
* **Use Deringed Probes (Bakery L1, opt-in):** Falls back to Bakery-style deringed L0+L1 reconstruction on non-LV worlds. Off by default so Unity's full SH9 probe path is preserved (keeps L2 detail and avoids the negative-L1 black-out artifact).

LTCGI gains independent diffuse and specular mix:

* **LTCGI Intensity:** Master mix.
* **LTCGI Specular Mix:** Area-light specular only.
* **LTCGI Diffuse Mix:** Area-light diffuse only.

---

## 11. VRSL STAGE HIJACK PROTOCOL

This shader intercepts world-space DMX buffers to mimic a **Standard 13-Channel Moving Head Fixture**. By assigning a `DMX Base Channel` that matches a venue's spotlights (e.g., Ch 1, 14, or 27), the material will:

* Override native emission with the stage's RGB and Strobe data via `_VRSL_Intensity`.
* **Kinetic Geo-Warping:** Utilize DMX Pan (Base+1) and Tilt (Base+2) to physically warp and bend the tessellated vertices of the avatar toward the active light beam, scaled by `_VRSL_Geo_Warp`.
* **VRSL Color Hijack:** New in 2.3.0. Independent slider that lerps the AudioLink color toward live DMX RGB (sector channels +3/+4/+5) without disabling AL color reactivity entirely. Let the stage wash the palette without losing the FFT-driven movement.

---

## 12. AUDIOLINK (RUNTIME-GATED)

AudioLink is now **always compiled** into the shader and gated at runtime by `_UseAudioLink`. This means VRCFury material-toggle animations can flip AudioLink on and off without a build-time variant explosion - VRC materials can't change keywords at runtime, but they can flip floats freely.

### Global Setup

* **Enable AudioLink:** Master runtime toggle.
* **Color Source:** Manual / ColorChord / Theme 0-3 / **ColorChord Strip** (new in 2.3.0).
* **ColorChord Strip Position:** New in 2.3.0. When `Color Source = ColorChord Strip`, picks any 0-1 position along the 128-pixel `ALPASS_CCSTRIP` - the full continuous ColorChord gradient is now exposed.
* **Power Down on Pause/Stop:** OFF by default in 2.3.0 (was ON in 2.1.5, which silently muted AudioLink for users without a VRC video player in scene). The bulk fix `VixenTools > VixenWear > Disable Media-State Gate On All Materials` clears this on every existing material project-wide.

### Chronotensity (Opt-In)

* **Enable Chronotensity FX:** Master opt-in. Required for any chrono-driven motion (vortex breath, fracture re-roll, glitch re-seed, scanline reaction, flicker time). OFF by default to avoid 4 extra texture samples per pixel for amplitude-only setups.
* **Chronotensity Index:** Selects which chronotensity row (0-7) the shader reads from.

### DFT Note Reactor

* **DFT Note Mod (0-11):** Pulls amplitudes at a specific musical note across all octaves via `AudioLinkGetAmplitudesAtNote`. Pull a snare hit cleanly, lock to a key signature, or strobe on every C# in the mix.
* **DFT Note Emission Amount:** Mix into the emission lobe.

---

## 13. GOD TIER CYBERNETICS (HUD OVERLAYS)

A built-in diagnostic and cybernetic UI system. Provide a black-and-white mask to `Cyber Mask (B&W Window)` to define where the HUD displays on your avatar. Each overlay can be independently scaled, rotated, positioned, and audio-reacted inside this window.

### Hover Engine (New in 2.3.0)

The HUD now floats *off* the body surface as a holographic plane:

* **HUD Hover Height:** Parallax-out distance. The HUD UV is shifted along the tangent-space view direction so the panel reads as a true overlay above the geometry.
* **HUD Hover Bob:** Subtle vertical drift (`sin(_Time.y * 1.6) * _Cyber_Hover * 0.25 * _Cyber_Hover_Bob`) - keeps the HUD feeling "alive" without rotating the panel.

### Overlays

* **VU Meter:** A segmented volume unit readout mapped to a specific AudioLink band.
* **Spectrum:** Sample N bars from an AudioLink band row. New in 2.3.0: `Spectrum Bar Count` (4-64) makes this a true bar chart with proper inter-bar gaps.
* **Waveform:** An oscilloscope line reading the raw AudioLink waveform buffer with smoothstep edge glow.
* **DMX Grid Readout:** Real-time grid visualizing VRSL DMX universe activity (requires VRSL enabled).
* **Autocorrelator Ring:** New in 2.3.0. Renders a radial ring from `ALPASS_AUTOCORRELATOR` with animated angular spokes (12-spoke sine wave + `_Time.y * 1.5` rotation) and a gaussian falloff envelope. Reads like a holographic compass spinning to the beat.

---

## 14. KINETIC UV ENGINE (SURFACE MAPS)

Transforms the mathematical surface of the material without altering the mesh.

* **Vortex Twist:** Swirls the local UVs into a spiral based on audio intensity. Optional chronotensity breath modulates the twist envelope when `Enable Chronotensity FX` is on.
* **UV Bass Pump:** Radially scales the texture inward to simulate speaker cone thumping.
* **UV Fracture Shard:** Uses spatial hashing to brutally tear and misalign the texture coordinates along a grid structure. The fracture mask now feeds a tiny parallax pop on shard edges - reads as physical shard separation.

---

## 15. KINETIC VERTEX ENGINE

In 2.3.0 the dual-pass geometry shader from 2.1.5 has been **removed** - it desynced under hardware tessellation. The replacement runs entirely inside the `disp()` vertex stage and the surface fragment, and is more stable while producing an equivalent (or better) shatter.

* **Normal Inflate Distance (`_Vtx_Pump_Str`):** Pushes vertices outward along their world normal to physically inflate the avatar to the bass.
* **Geometry Shard Scatter (`_Vtx_Fracture_Str`):** Snaps vertices to a 3D grid (`floor(vertex.xyz * 25.0)`), hashes each cell for a deterministic per-shard random axis, applies Rodrigues rotation around that axis, scales the shard, and offsets it along a pivot. All gated by `_UseVtxKinetic` and strictly driven by AudioLink band amplitude so silent worlds never shatter the avatar.
* **Vertex Autocorrelator Ripple (`_Vtx_AutoCorr_Str`):** New in 2.3.0. Reads `AudioLinkGetSphericalMappedAutoCorrelatorValue(normalize(v.vertex.xyz))` per-vertex and drives a smooth volumetric ripple. No band selection needed - every vertex samples its own object-space direction.

The surface fragment also performs a per-pixel fracture clip (`clip(fractureNoise - fractureCut)`) so the visible mesh punches holes in sync with the vertex scatter. This survives tessellation cleanly.

---

## 16. GLOBAL MATERIAL MODULATIONS

Secondary atmospheric effects driven by AudioLink FFT arrays.

* **Audio Color Blend (Rainbow):** When `_AL_Col_Blend > 0`, the AL color cycles through a worldspace-anchored rainbow (`_Time.y + bio + worldPos.y`). Applied before VRSL hijack.
* **Surface Waveform Ripple:** Scrolling micro-stutters across the UV driven by the raw waveform buffer.
* **Surface Autocorrelator Ripple:** New in 2.3.0. Vertically warps the emission UV with `(autoCorr - 0.5)` so circuitry breathes without recolouring.
* **Audio Scanlines:** Scrolling CRT-style horizontal bands driven by track speed and amplitude. Chronotensity reaction is opt-in.
* **Parallax Thump:** Dynamically deepens the raymarched parallax mapping to the beat.
* **Clearcoat Shatter:** Destroys the smoothness value of the clearcoat on high frequencies.
* **Thin Film Expansion:** Pulses the nanometer thickness of the iridescence to shift refractive colors dynamically.
* **Digital Tear:** Micro-stutters and slices the UV map laterally to simulate codec failure. The hash re-seeds against `_Time.y * 9 + chr_glit * 4` so the tear evolves over time.

---

## 17. EDITOR WORKFLOW (NEW IN 2.3.0)

Beyond the inspector tabs, the `VixenTools > VixenWear` menu adds three project-wide utilities:

* **`Edit Materials From Selection` (`Ctrl+Shift+M`):** Promotes the current Hierarchy GameObject selection to its underlying VixenWear `.mat` assets. Walks `GetComponentsInChildren<Renderer>(includeInactive: true)` - critical for VRC wardrobe layers that are toggled off - deduplicates the materials, and swaps `Selection.objects`. The inspector then multi-edits cleanly instead of collapsing to `-` across renderers.
* **`Clean Latex Material Keywords`:** Manually rebuilds every VixenWear material's keyword set against the current pragma list. Useful after bulk-editing via animation clips or VRCFury.
* **`Disable Media-State Gate On All Materials`:** Project-wide bulk fix that clears `_UseMediaState` on every VixenWear material at once, restoring AudioLink reactivity for worlds without a `_MediaPlaying` driver. Most useful when upgrading from 2.1.x where this property defaulted to ON.

### Tab Context Menu

Right-click any inspector tab (`BASE`, `SURFACE`, `POLISH`, `INTEGRATION`, `AUDIOLINK`, `STAGE`) for:

* **Copy {TAB} Settings** - clipboard the tab's float/color/vector/texture state.
* **Paste {TAB} Settings (Values Only)** - paste without overwriting textures.
* **Paste {TAB} Settings (With Textures)** - paste including texture references + offsets + scales.
* **Reset {TAB} to Defaults** - new in 2.3.0. Spawns a hidden defaults material from the same shader and writes its values back through `MaterialProperty`. Honors `_Mode` on the BASE tab by re-running `SetupMaterialWithBlendMode` after the reset.

### Build-Time Variant Stripping

The package now ships a `VixenWearVariantStripper` (`IPreprocessShaders`) that hammers shader variants in three layers during every build:

1. Managed feature keywords - drop variants where no material has the keyword on.
2. Deferred / Meta / MotionVectors passes - dropped entirely (avatar clothing never uses them).
3. Dead built-in keywords leaking past the surface pragma (`LIGHTMAP_ON`, `DIRLIGHTMAP_COMBINED`, `DYNAMICLIGHTMAP_ON`, `LIGHTMAP_SHADOW_MIXING`, `SHADOWS_SHADOWMASK`, `LIGHTPROBE_SH`, `LOD_FADE_CROSSFADE`) - dropped any variant that has one set.

`VixenWearVariantStripReporter` posts a `kept / stripped / total` log line after every build so you can verify the speedup.
