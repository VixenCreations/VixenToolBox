# VixenWear / Latex Ultra - How To Use

The end-game PBR shader for VRChat avatars. One material, six tabs, full integration with AudioLink, VRSL (DMX hijack + GI wash), LTCGI, AreaLit, and VRC Light Volumes. Built-in Render Pipeline only (VRChat) - HDRP/URP are not supported (this is a surface shader).

Ships in two flavors: **VixenWear / Latex Ultra** (the full tessellated shader) and **VixenWear / Latex Ultra SPS** (a tessellation-free clone for VRCFury SPS / penetrator setups). Both share the exact same inspector.

The Shader Pipeline deep-dive and the Changelog are also available in-editor: **VixenTools > Hub Dashboard** opens the VixForge Hub.

---

## 1. Installation

VixenWear ships as part of the **Vixens Toolbox** VPM package. Add the toolbox to your project (VCC / ALCOM, or the package listing) and the shaders, includes, and editor are imported automatically - there is no manual drop-in step.

The shader files live inside the package:

```
Packages/com.vixencreations.vixens-toolbox/
├── SHADERSETUP.md                          <- deep technical reference (also in the Hub)
├── HOW_TO_USE.md                           <- this guide
├── CHANGELOG.md
├── Editor/Avatar Tools/Shaders/
│   ├── VixenWear Latex.shader
│   ├── VixenWear Latex SPS.shader
│   ├── VixenWearEditor.cs
│   └── cginc/                              <- shader includes (LTCGI, AudioLink, AreaLit, LightVolumes)
└── Runtime/AreaLitBroadcaster/
    └── AreaLitGlobalBroadcaster.cs         <- world-side AreaLit helper (see §12)
```

The shaders' `#include` directives are package-absolute references to `Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/`, so they resolve automatically wherever VPM installs the package. You never need to edit them.

### Which shader do I use?

- **VixenWear / Latex Ultra** - the default. Full hardware tessellation, all features, the cybernetic HUD, and the flying-shard kinetic engine.
- **VixenWear / Latex Ultra SPS** - assign this on meshes that VRCFury's SPS / penetrator patcher rewrites. It drops the tessellation stage (which VRCFury can't patch) and the PC-only geometry effects (3D drips, HUD, flying shards) but keeps the full surface/lighting feature set, including the wet soak and melting goo.

### Optional Integrations

All integrations are auto-detected. If a package isn't installed, the matching feature is silently inert - no compile errors.

| Package | Source |
|---------|--------|
| AudioLink | github.com/llealloo/audiolink |
| LTCGI | github.com/PiMaker/ltcgi |
| AreaLit | (PiMaker AreaLit - auto via the included Global Broadcaster, or per-material slots; see §12) |
| VRChat Light Volumes | github.com/REDSIM/VRCLightVolumes |
| VRSL-GI | github.com/AcChosen/VR-Stage-Lighting |
| VRCFury (wardrobe toggles) | vrcfury.com |

---

## 2. Quick Start

1. Create a new material.
2. Assign shader **VixenWear / Latex Ultra**.
3. **BASE** tab - drop in your albedo, pick a base color.
4. **SURFACE** tab - drop in your packed PBR mask and normal map. If the mask was authored for Poiyomi / Substance / Marmoset, set the channel selectors directly below the mask slot to match that packing.
5. Push sliders to taste.

---

## 3. The Inspector

Six tabs across the top. Active tab has a cyan underline. Click to switch. Your active tab persists across material selections.

| Tab | What It Controls |
|-----|------------------|
| **BASE** | Render mode (Opaque/Cutout/Fade/Transparent), base color, albedo, minimum brightness, animated UV (rotation + scroll speed) |
| **SURFACE** | Packed PBR maps with per-channel selectors, Poiyomi/Mochie reflection + specular masks (with one-click setup), normals, parallax shadows, tessellated displacement, micro detail layer |
| **POLISH** | Polish layer master gate (clearcoat, thin film, rim, SSS, anisotropic latex stretch, transmission, multi-scatter), wet + run-off, melting goo, backface outline |
| **INTEGRATION** | Dual emission layers, RGB region masks, dual MatCap stacks, Light Volumes, LTCGI area lights |
| **AUDIOLINK** | Cybernetic HUD overlays, kinetic vertex engine, UV vortex/pump/fracture, audio scanlines, chronotensity FX, DFT note targeting, lobe-thump modulations |
| **STAGE** | VRSL-GI DMX routing, stage hijack power, geo-warping, color override |

Each tab has a one-line description under the tab bar reminding you what's in it.

---

## 4. Multi-Material Editing

VRChat outfits usually have many materials per avatar. To edit them all at once:

1. Select your avatar (or any GameObject) in the Hierarchy.
2. Press **Ctrl + Shift + M** - or use **VixenTools > VixenWear > Edit Materials From Selection**.
3. The Selection swaps to the unique set of VixenWear materials under that GameObject - including ones on disabled wardrobe toggles.
4. Edits in the Inspector now apply to every material at once.

Mixed values across selected materials show as `–` (dash) per component. Typing a new value overwrites that one component on all selected materials, preserving the other components.

---

## 5. Copy, Paste, and Reset Tabs

**Right-click any tab header** for:

- **Copy [TAB] Settings** - copies every property in that tab (values, colors, textures, tiling/offset) to the clipboard.
- **Paste [TAB] Settings (Values Only)** - paste without overwriting texture slots.
- **Paste [TAB] Settings (With Textures)** - paste everything including textures and their tiling.
- **Reset [TAB] to Defaults** - restore the tab to the shader's declared defaults.

The clipboard persists for the Unity session. Every paste and reset is Undo-able (Ctrl+Z).

Each tab also has a pink **↺ Reset [TAB] to Defaults** button at the bottom, identical to the right-click reset.

---

## 6. Render Modes (BASE tab)

| Mode | Use For |
|------|---------|
| **Opaque** | Solid materials. No alpha. |
| **Cutout** | Hard-edged transparency (lace, leaves). The Alpha Cutoff slider appears only in this mode. |
| **Fade** | Straight alpha - specular fades out with the surface. |
| **Transparent** | Premultiplied alpha - specular highlights survive at low opacity. Use for glass, wet latex highlights, etc. |

Switching modes automatically updates blend state, ZWrite, render queue, RenderType tag, VRCFallback tag, and alpha keywords. You never need to touch render queue manually.

---

## 7. Packed PBR Compatibility (SURFACE tab)

The shader's native packing is **R: Metallic, G: AO, B: Displacement, A: Smoothness**, but you can re-route every channel:

- **Metallic Channel** + **Invert Metallic**
- **Smoothness Channel** + **Channel Stores Roughness (Invert)**
- **AO Channel**
- **Height Channel**

This means Poiyomi / Substance / Marmoset / Standard packed masks drop in without re-authoring - just pick which channel drives which property.

### Reflection & Specular Masks

Tick **Enable Reflection / Specular Masks** to read two more channels off the same packed map:

- **Reflection Mask** dims environment / reflection-probe specular (including clearcoat env, Light Volume, and LTCGI reflections).
- **Specular Mask** dims direct-light highlights.

Each has its own channel selector, invert, and strength. Channel defaults match Mochie packing (B = reflection, A = specular). Matcaps keep their own masks and are not affected.

### One-Click Mochie / Poiyomi Setup

Already have a Mochie or Poiyomi **Metallic Maps** texture (R: Metallic, G: Smoothness, B: Reflection, A: Specular)? Drop it into **Packed PBR Map** and press **Set Up for Poiyomi / Mochie Metallic Map**. It sets the four channel selectors, disables AO, and enables the reflection + specular masks in one click.

---

## 8. Polish Layer, Wet & Goo (POLISH tab)

### Polish Layer Gate

**Enable Polish Layer** (on by default) is the master switch for the whole finish stack - clearcoat, thin film, rim, SSS, anisotropy, transmission, and multi-scatter. Turn it off for a flat matte base or a lighter material. A **Polish Mask** scopes the layer to painted regions.

### Wet & Run-Off

**Enable Wet** soaks the masked area like it just came out of the pool:

- **Soaked Look** - Wetness, Darkening, Wet Smoothness, Film Sheen, and Normal Flatten drive water absorption, a near-mirror film, a dielectric sheen (the sheen rides on the clearcoat, so keep Polish enabled for the strongest highlight), and detail flattening.
- **Run-Off Rivulets** - animated vertical water streaks layered on the soak. Set Streak Strength to 0 for a still, evenly-soaked look.
- **Clear 3D Drips** (base shader only) - real water droplets emitted by a geometry stage that swell, pinch off, and fall away, with sway, body-slide, and floor-splat physics. Not present on the SPS shader or Quest.

A **Wet Mask** scopes the whole effect.

### Goo (Melting Sag)

**Enable Goo** is a gravity-aligned vertex melt that mimics runny latex. Melt Amount sets intensity, Stretch Distance extends the sag (world units), and Strand Variation adds uneven tendrils. **Melt To Ground** + **Ground / Floor Height** pull strands toward the world floor regardless of avatar height. Sway, Surface Follow, Floor Collision, and Floor Pooling add procedural physics. Runs on both shaders and benefits from tessellation on the base shader. A **Goo Mask** scopes it.

> True inertial lag and per-bone body collision are not possible in a shader. For real swing, drive a PhysBone chain over the goo/drip region.

## 9. Multi-Region Color Masks (INTEGRATION tab)

Drop a 3-channel mask where each color marks a zone:

- **Red channel** → Zone A
- **Green channel** → Zone B
- **Blue channel** → Zone C

Each zone gets its own:
- **Tint** - multiplies into albedo (white = no change)
- **Emission Boost** - adds zone-colored glow on top

Use this to drive three outfit colorways from one material without splitting the mesh.

---

## 10. Dual MatCap Layers (INTEGRATION tab)

Layer 1 is always on. Enable **MatCap 2 Layer** for a second stack with its own mask channel.

Combine with a region mask - set Layer 1's mask channel to R and Layer 2's to B - and you get a chrome matcap in red zones, a pearl matcap in blue zones, all on the same material.

---

## 11. Light Volumes (INTEGRATION tab)

- **Intensity** > 0 to activate.
- **Base Specular Mix** / **Clearcoat Specular Mix** - route LV into specular.
- **Dominant Mode** - faster specular path using only the dominant light direction.
- **Normal Bias** / **Position Offset** - sampling adjustments for problem geometry.
- **Additive-Only Mode** - preserve native probe lighting and add LV on top.
- **Deringed Probes Fallback** - for Bakery L1 baked scenes.

---

## 12. LTCGI & AreaLit (INTEGRATION tab)

### LTCGI

- **Intensity** > 0 to activate.
- **Specular Mix** / **Diffuse Mix** - balance the area-light contribution.

Auto-disabled in builds where LTCGI isn't installed.

### AreaLit

AreaLit lights the latex like LTCGI. AreaLit itself ships no scene-wide broadcast (its LightCam just renders into a `LightMesh` RenderTexture that each AreaLit/Standard material points at), so VixenWear bridges that gap two ways:

- **Automatic (GI-level, recommended)** - drop the **AreaLit Global Broadcaster** (`Runtime/AreaLitBroadcaster/AreaLitGlobalBroadcaster.cs`) on a GameObject next to the world's AreaLit `LightCam`, and assign that LightCam's `LightMesh` RenderTexture + the light/video RenderTexture. It publishes them scene-wide as `_Udon_AreaLit_LightMesh` / `_Udon_AreaLit_Tex0` / `_Udon_AreaLit_Enable`, and **every VixenWear avatar in the world intercepts the area lights automatically** - exactly like reading LTCGI's globals, no per-material assignment. (The broadcaster is world-side tooling and only compiles in a VRChat Worlds / UdonSharp project; it's excluded from avatar projects.)
- **Manual fallback** - if no broadcaster is present, assign the world's AreaLit `LightMesh.renderTexture` and its light/video RenderTexture into the two **(manual fallback)** slots on the material yourself (the same RTs an AreaLit/Standard material uses).

Controls:

- **AreaLit Intensity** > 0 to activate (this also compiles in the `AREALIT_ENABLE` keyword).
- **LightMesh RT / Light Texture (manual fallback)** - the per-material slots used only when no broadcaster is live.
- **Specular Mix** / **Diffuse Mix** - balance the contribution.

When `_Udon_AreaLit_Enable > 0` the broadcast globals take priority; otherwise the manual slots are used; if neither is present it contributes nothing. (Trimmed to a single light texture and 16 quads for avatar cost.)

---

## 13. AudioLink (AUDIOLINK tab)

Tick **Enable AudioLink** to reveal the full reactivity tree:

**Environment & Media**
- Global color source (audio → color mapping)
- ColorChord strip position
- Power Down on Pause/Stop (kills reactivity when the world's video player is paused)

**Chronotensity FX** - Time-warp effect, choose slot 0–7.

**HUD Overlays (God Tier Cybernetics)** - Hovering display panels rendered above the surface (base shader only - the SPS variant has no geometry HUD pass):
- VU Meter - choose **Console** style (a full self-playing AudioLink control panel) or **Bar**.
- Spectrum bars
- Waveform line
- DMX grid
- Autocorrelator ring - with four optional per-band effects: Shimmer, Pop, Sizzle, Electrify.

Each segment toggles independently with its own band, intensity, and screen transform (X, Y, Scale, Rotation).

**Kinetic Vertex Engine** - Audio-driven mesh inflation and shard fracture (needs SM5 / tessellation-capable target).

**Kinetic UV Engine** - Vortex twist, bass pump, fracture displacement, all in UV space with per-effect transforms.

**Global Modulations** - Emission band, color blend, surface waveform ripple, autocorrelator ripple, DFT note targeting (pick a single chromatic note to react to).

**Audio Scanlines** - Reactive overlay with adjustable density, speed, and chronotensity reaction.

**Physical Lobe Thump** - Audio modulates real lighting properties: thin film thickness, parallax depth, clearcoat shatter, digital glitch tear.

All AudioLink toggles are **runtime-branched, not keyword-gated** - so VRCFury material-toggle animations flip them without spawning new shader variants.

---

## 14. VRSL Stage Hijack (STAGE tab)

Tick **Enable VRSL Stage Hijack Protocol** to bind the material to world DMX buffers:

- **DMX Base Channel (Sector ID)** - which DMX channel the material listens to.
- **Stage Hijack Override Power** - how strongly the stage takes over.
- **Pan/Tilt Displacement Scale** - tessellated vertex positions warp with stage moves.
- **DMX RGB Color Override** - DMX color overrides AudioLink color when active.

**Stage GI Wash** (separate from the hijack): instead of overriding emission, this spills the DMX fixtures' colour onto the suit as real additive light - the stage actually lighting you up.

- **GI Wash Intensity** > 0 to activate.
- **GI Specular Mix** - fresnel-weighted highlight from the wash.
- **GI Color Saturation** - desaturate toward luma so the wash tints without repainting your design.

Requires VRSL in the world. If it's absent (no DMX grid), both the hijack and the wash are silently inert.

---

## 15. Build & Upload Optimization

The package ships with an aggressive variant stripper that drops:

1. Feature-keyword variants no material in your project has on.
2. Deferred / Meta / MotionVectors passes (avatars don't use them).
3. Lightmap / DynamicLightmap / LPPV / LOD-fade variants.

After a build you'll see a log line like:
```
[Vixen Wear] Variant strip: kept 124, stripped 4012 (total 4136).
```

Keywords are auto-synced on:
- Build start
- Play-mode entry
- Any property change in the inspector

Force a re-sync via **VixenTools > VixenWear > Clean Latex Material Keywords**.

---

## 16. VixenTools Menu Reference

| Menu | What It Does |
|------|--------------|
| `VixenTools > VixenWear > Clean Latex Material Keywords` | Forces keyword sync on every VixenWear material in the project and writes to disk. |
| `VixenTools > VixenWear > Edit Materials From Selection` (`Ctrl + Shift + M`) | Replaces the current selection with the unique set of VixenWear materials under the selected GameObjects (includes inactive children). |
| `VixenTools > VixenWear > Disable Media-State Gate On All Materials` | Bulk-disables `_UseMediaState` so AudioLink reacts even without a world video player. |

---

## 17. Troubleshooting

**Shader error: cannot open include file**
The shader `#include` paths are package-absolute (`Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/`) and resolve automatically under VPM. If you've copied the shaders out of the package into `Assets/`, the includes no longer resolve - keep them inside the package, or edit the include lines near the top of each pass in `Editor/Avatar Tools/Shaders/VixenWear Latex.shader` to point at the new location.

**AudioLink isn't reacting**
- Confirm AudioLink is in the world (the AudioLink prefab must be in the scene).
- Confirm **AUDIOLINK > Enable AudioLink** is ticked on the material.
- If the world has no video player, run **VixenTools > VixenWear > Disable Media-State Gate On All Materials** (or untick **Power Down on Pause/Stop** per material).

**Specular looks wrong in VRChat mirrors**
The shader already uses mirror-corrected camera-position math. If something still looks off, confirm you don't have a custom mirror script overriding camera matrices.

**VRCFury toggles don't change shader keywords**
This is intentional. AudioLink and HUD layers are runtime-branched so VRCFury doesn't multiply shader variants. The visual effect still toggles - via a uniform branch instead of a keyword.

**Transparent outfit casts hard shadows**
Disable shadow casting on the renderer. Fully-transparent fragments are discarded, but partially-transparent ones still cast.

**Inspector shows `–` (dash) on a field**
Selected materials disagree on that property. Type a value to set it on all of them; that one component is overwritten while the others are preserved.

**Material looks wrong after switching render mode**
The blend state should update automatically. If it doesn't, re-select the material - the inspector reapplies the full blend/queue/tag setup on every BASE-tab _Mode change.

---

## 18. Tips

- **Right-click any property label** in the inspector to copy its internal shader property name (useful for VRCFury / animation clips).
- **Tab clipboard survives across material switches** - copy POLISH from one material, switch to another, paste, repeat.
- **Reset a single tab** if you've over-tweaked. The shader defaults are sane starting points.
- **For wardrobe toggles**: animate `_UseAudioLink` directly. No keyword permutations are emitted, so toggle animations are zero-cost at variant level.

---

That's it. Open the inspector, drop in some maps, push some sliders.

Welcome to Latex Ultra.
