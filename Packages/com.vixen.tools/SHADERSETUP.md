# VIXENWEAR : SHADER USAGE

The **VixenWear Latex Ultra** is a high-fidelity, dual-lobe PBR surface shader engineered for complex synthetic materials. It bypasses standard Unity lighting models to provide a full GGX BRDF stack (Burley diffuse, Smith-Joint visibility, Schlick Fresnel, Karis split-sum environment, optional Filament multi-scatter compensation), true tangent-space micro-shadowing, hardware tessellation, dynamic thin-film interference, anisotropic specular for stretched latex, thin-part transmission, wet/run-off and melting-goo surface FX, and fully integrated SM5 hardware processing for AudioLink, VRSL DMX (stage hijack + GI wash), LTCGI and AreaLit area lights, and VRC Light Volumes. Built-in Render Pipeline only (VRChat); as a surface shader it does not compile under HDRP/URP.

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

### Reflection & Specular Masks (New in 2.4.0)

`Enable Reflection / Specular Masks` (`_UsePackedMasks`) reads two extra channels off the same packed PBR map to dim specular response without a second texture:

* **Reflection Mask** (`_ReflMask_Ch` / `_ReflMask_Inv` / `_ReflMask_Str`): attenuates environment / reflection-probe specular - including the clearcoat environment lobe, Light Volume specular, and LTCGI reflections.
* **Specular Mask** (`_SpecMask_Ch` / `_SpecMask_Inv` / `_SpecMask_Str`): attenuates direct-light highlights only.

Both default to `1.0` (no effect) until the gate is on. Channel defaults match Mochie packing (`B` = reflection, `A` = specular). Matcaps keep their own masks and are unaffected.

### One-Click Mochie / Poiyomi Metallic Map (New in 2.4.0)

If you already have a Mochie or Poiyomi **Metallic Maps** texture (`R:Metallic G:Smoothness B:Reflection Mask A:Specular Mask`), drop it into `Packed PBR Map` and hit **Set Up for Poiyomi / Mochie Metallic Map** on the SURFACE tab. The button sets the four channel selectors, disables AO (None), and enables the reflection + specular masks in one shot - no manual channel-routing required.

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
* **Hardware Tessellation:** If the GPU supports DX11/Tessellation, the geometry is physically sub-divided and extruded using the `Displacement Strength`. The `Tessellation Detail` slider (0–1) sets how dense that subdivision is — **higher = more detail and GPU cost, lower = cheaper** — and the amount is distance/screen-adaptive (far or small-on-screen surfaces are automatically cheaper) and capped so it can't run away. Leave it at 0 if you aren't using Displacement Strength. (Base shader only; the SPS variant has no tessellation stage.)

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

### Polish Layer Master Gate (New in 2.4.0)

The POLISH tab now opens with `Enable Polish Layer` (`_UsePolish`, default ON). This is the master switch for the entire polish lighting layer - clearcoat, thin film, SSS, transmission, anisotropy, rim, and multi-scatter compensation all collapse under it. Turn it off and the material drops to a flat GGX base for a clean matte look or a lighter variant. A per-pixel `Polish Mask` (`_PolishMask` + `_PolishMaskCh`) scopes the layer to painted regions, so you can keep panels glossy while the rest reads matte. The Wet, Goo, and Outline effects below the gate have their own independent toggles.

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
* **Tiling / Scroll (per layer):** Each layer has a `Tiling (XY)` vector and a `Scroll (X pan, Y pan, Z spin)` vector. Tiling repeats the matcap around its centre (set the matcap texture's Wrap Mode to **Repeat** to see it actually repeat; Clamp just stretches the border). Scroll animates it smoothly off real time: `X`/`Y` pan and `Z` spins the matcap in degrees per second. Leave Tiling at `(1,1)` and Scroll at `(0,0,0)` for the classic static matcap.

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

### AreaLit (New in 2.5.0)

AreaLit area lights are vendored in as analytic Linearly-Transformed-Cosines (`Editor/cginc/AreaLit/AreaLit_Latex.cginc`, `AL_`-prefixed, no LUT texture). AreaLit itself ships **no scene-wide broadcast** (its LightCam renders into a per-material `LightMesh` RenderTexture), so VixenWear reads it two ways, preferring the global path so the suit intercepts AreaLit **at the GI level like LTCGI**:

* **Scene globals (primary):** `_Udon_AreaLit_LightMesh` / `_Udon_AreaLit_Tex0` / `_Udon_AreaLit_Enable`, published by the world-side `AreaLitGlobalBroadcaster` Udon helper (`Runtime/AreaLitGlobalBroadcaster.cs`, `VRCShader.SetGlobalTexture` in `Start`). When `_Udon_AreaLit_Enable > 0.5` the dispatcher reads these - no per-material assignment, every avatar in the world lit automatically.
* **Per-material fallback:** `_AreaLit_LightMesh` + `_AreaLit_LightTex0` slots, used when no broadcaster is live.
* Both sources funnel through one `AL_ShadeCore(Texture2D<float4> lmesh, Texture2D ltex0, ...)` (a runtime branch on `_Udon_AreaLit_Enable` selects the pair). LightMesh is read with `.Load` (no sampler); the light texture is `UNITY_DECLARE_TEX2D_NOSAMPLER` sampled through `_MainTex`'s sampler - so AreaLit adds **zero** sampler registers.
* **AreaLit Intensity / Specular Mix / Diffuse Mix:** mirror the LTCGI mixes.

Gated by the `AREALIT_ENABLE` keyword (compiled in when Intensity > 0, stripped otherwise). The include is wrapped in `#ifndef SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER` (with a no-op stub) because Unity's surface-analysis pass uses MojoShader, which can't parse object textures - the same guard the vendored LTCGI uniforms use. Trimmed for avatar cost: single light texture, 16-quad cap, no MSBuffer/checkerboard. An unbound global, empty slots, or a non-AreaLit world all contribute nothing.

---

## 11. VRSL STAGE HIJACK PROTOCOL

This shader intercepts world-space DMX buffers to mimic a **Standard 13-Channel Moving Head Fixture**. By assigning a `DMX Base Channel` that matches a venue's spotlights (e.g., Ch 1, 14, or 27), the material will:

* Override native emission with the stage's RGB and Strobe data via `_VRSL_Intensity`.
* **Kinetic Geo-Warping:** Utilize DMX Pan (Base+1) and Tilt (Base+2) to physically warp and bend the tessellated vertices of the avatar toward the active light beam, scaled by `_VRSL_Geo_Warp`.
* **VRSL Color Hijack:** New in 2.3.0. Independent slider that lerps the AudioLink color toward live DMX RGB (sector channels +3/+4/+5) without disabling AL color reactivity entirely. Let the stage wash the palette without losing the FFT-driven movement.
* **VRSL GI Wash:** New in 2.5.0. A *lighting* contribution, distinct from the emission hijack above. Instead of overriding the suit's emission, it spills the DMX fixtures' colour onto the surface as real additive light (the stage illuminating you). `_VRSL_GI_Int` is the master, `_VRSL_GI_Spec` adds a fresnel-weighted highlight, and `_VRSL_GI_Sat` desaturates toward luma so the wash tints rather than repaints. Decoded from the same DMX grid + channel offsets the Color Hijack reads (so the two agree), it rides the `VRSL_ENABLE` keyword + `_UseVRSL` gate and skips itself via a DMX-grid TexelSize liveness probe in worlds with no node.

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

> **Per-widget reaction bands (New in 2.4.0):** the Waveform, DMX Grid, and Autocorrelator segments each carry their own band selector (`_Cyber_Wave_Band`, `_Cyber_DMX_Band`, `_Cyber_Auto_Band`) so every widget can react to a different AudioLink band instead of sharing one global selection.

### Overlays

* **VU Meter:** A volume unit readout. New in 2.4.0: `VU Meter Style` (`_Cyber_VU_Style`) switches between **Console** and **Bar**. The Console style renders a full self-playing AudioLink control panel - gain / threshold / hit-fade / falloff sliders, a 4-band readout, theme + ColorChord swatches, and an autocorrelator scope - ported from the upstream `AudioLinkUI-Functions.cginc` (now vendored into the package's `cginc/` folder). The sliders are live display-only readouts of the AudioLink state, not interactive controls.
* **Spectrum:** Sample N bars from an AudioLink band row. New in 2.3.0: `Spectrum Bar Count` (4-64) makes this a true bar chart with proper inter-bar gaps.
* **Waveform:** An oscilloscope line reading the raw AudioLink waveform buffer with smoothstep edge glow.
* **DMX Grid Readout:** Real-time grid visualizing VRSL DMX universe activity (requires VRSL enabled).
* **Autocorrelator Ring:** New in 2.3.0. Renders a radial ring from `ALPASS_AUTOCORRELATOR` with animated angular spokes (12-spoke sine wave + `_Time.y * 1.5` rotation) and a gaussian falloff envelope. Reads like a holographic compass spinning to the beat.
  * **Ring Effects (New in 2.4.0):** four independent per-band reactors layer onto the ring - `Shimmer` (`_Cyber_Auto_Shimmer`), `Pop` (`_Cyber_Auto_Pop`), `Sizzle` (`_Cyber_Auto_Sizzle`), and `Electrify` (`_Cyber_Auto_Electrify`). Each has its own `..._Band` enum so one band can drive the shimmer while another drives the pop. With AudioLink off, each effect falls back to a `_Time`-driven idle level so the ring still animates while you author. PC HUD only; the SPS shader declares the props inert for inspector and copy/paste parity.

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
* **Flying-Shard Fracture (overhauled in 2.4.0, PC only):** The old single-value scatter is replaced by a full shard system driven by dedicated geometry passes. `_Vtx_Fracture_Amount` is a manual hold/animate dissolve (the body opens up as it rises), `_Vtx_Fracture_Dist` sets how far shards hover, `_Vtx_Fracture_Spin` tumbles them, `_Vtx_Fracture_Str` is now an AudioLink jitter, and `_Vtx_Fracture_Spiral`, `_Vtx_Fracture_Lift`, `_Vtx_Fracture_Float`, and `_Vtx_Fracture_Trail` shape the dispersal and motion trails. Shard coloring adds `_Shard_ColorMod` (hue shift) + `_Shard_ColorMod_Speed`, and `_UseShardCC` + `_Shard_CC_Str` blend the shards toward the live AudioLink ColorChord. On the **SPS shader** there is no geometry pass, so the suit dissolves via the main-pass clip only - no flying shards.
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

---

## 18. WET, RUN-OFF & GOO (NEW IN 2.4.0)

Three new POLISH-tab effects bring physical liquid behaviour to the latex. Each has its own enable toggle and B&W mask, so they layer independently over the polish lighting.

### Wet & Run-Off (`_UseDrip`)

Soaks the masked area like the avatar just stepped out of the pool.

* **Soaked Look:** `Wetness` (`_Wet_Amount`) is the master amount. `Darkening` (`_Wet_Darken`) deepens the latex for water absorption, `Wet Smoothness` (`_Wet_Smoothness`) drives reflections toward a near-mirror water film, `Film Sheen` (`_Wet_Sheen`) adds a dielectric Fresnel sheen that rides on the clearcoat (keep the Polish layer enabled for the strongest highlight), and `Normal Flatten` (`_Wet_Flatten`) smooths out micro-detail.
* **Run-Off Rivulets:** Animated vertical water streaks layered on top of the soak. `Density` (`_Drip_Density`) sets the column count, `Rivulet Thinness` (`_Drip_Width`) narrows each streak, and `Coverage`, `Flow Speed`, `Streak Strength`, and `Streak Normal Bump` (`_Drip_Coverage` / `_Drip_Speed` / `_Drip_Strength` / `_Drip_Normal`) shape the flow. Set Streak Strength to `0` for a still, evenly-soaked look.
* **Mask:** `_DripMask` + `_DripMaskCh` scope the entire effect.

The soak and rivulets run on **both** the base and SPS shaders.

### Clear 3D Drips (Geometry, PC Only)

`Clear Drip Amount` (`_Drip3D_Strength`) emits real water droplets from a geometry stage on the base `VixenWear/Latex Ultra` shader: they swell on downward-facing wet areas, form a neck, pinch off, then fall away as free geometry and dry out (fade).

* `Droplet Size` (`_Drip3D_Scale`, roughly millimetres), `Glassiness` (`_Drip3D_Sheen`), and `Fall Distance` (`_Drip3D_Fall`) tune the look; drops are tinted to the Clearcoat Tint and share Coverage / Flow Speed with the rivulets.
* **Physics:** `Sway / Wobble` (`_Drip_Sway`) adds surface-tension wobble and a breeze that grows the further a drop falls. `Surface Slide (Body)` (`_Drip_BodyFollow`) makes an attached drop run down along the body before it detaches (a faked body collision). `Floor Splat` (`_Drip_FloorCollide`) pins drops to the shared world floor and spreads them into a fading puddle - the floor height is the Goo `Ground / Floor Height` below.

Not present on the SPS shader or Quest - the droplet emitter is a geometry stage that gets stripped on those targets. Drops always fall under world gravity, so they already track movement; true inertial trailing would need a PhysBone, not a shader.

### Goo (Melting Sag) (`_UseGoo`)

Gravity-aligned vertex melt that mimics runny / melting latex. Runs in the displacement stage, so on the base shader it benefits from tessellation (more verts = smoother strands).

* **Shape:** `Melt Amount` (`_Goo_Strength`) is the master intensity; `Stretch Distance` (`_Goo_Reach`) dramatically extends how far it sags in world units; `Strand Variation` (`_Goo_Variation`) adds procedural FBM noise so tendrils range from uniform (`0`) to wildly uneven (`1`). `Tendril Scale`, `Flow Speed`, and `Underside Bias` (`_Goo_Noise` / `_Goo_Speed` / `_Goo_Droop`) refine the strands.
* **Reach the floor:** `Melt To Ground` (`_Goo_ToGround`) pulls the goo toward the world ground plane; set `Ground / Floor Height` (`_Goo_GroundY`) to your world floor's Y (usually `0`) so strands reach the floor regardless of avatar height.
* **Physics:** `Sway Amount` / `Sway Speed` (`_Goo_Sway` / `_Goo_SwaySpeed`) give a per-strand pendulum swing; `Surface Follow (Body Collide)` (`_Goo_BodyFollow`) flows goo along the body instead of through it; `Floor Collision` (`_Goo_FloorCollide`) clamps the melt to the floor height; `Floor Pooling` (`_Goo_Pool`) spreads landed strands into a puddle.
* **Mask:** `_GooMask` + `_GooMaskCh`.

Goo runs on **both** shaders (it is a vertex/displacement effect). Note: extreme stretch can be frustum-culled when the body is off-screen unless the mesh bounds (or an Anchor Override) are expanded. The goo re-aligns to gravity and the body surface every frame, so it tracks posing and locomotion; true inertial lag and per-bone body collision are not possible in a shader - drive a PhysBone chain over the goo region for that.
