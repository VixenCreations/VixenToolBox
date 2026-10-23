# Credits

Everyone whose work this package uses, builds on or works alongside. Code that ships with its own license
has its notice in a `LICENSE-*` file in this folder. Everything else here is a thank-you: an idea, a method
or a paper we learned from.

## Code that ships with its license

- **AudioLink** by llealloo, cnlohr, lox9973, pema99 and float3. The AudioLink section and the Director.
  github.com/llealloo/audiolink. Notice: `LICENSE-AudioLink.md`.
- **LTCGI** by _pi_ (pimaker), built on the LTC method by Eric Heitz, Jonathan Dupuy, Stephen Hill and
  David Neubelt. The LTCGI section. github.com/pimaker/ltcgi. Notice: `LICENSE-LTCGI.md`.
- **VRC Light Volumes** by RED_SIM. The Light Volumes section. github.com/REDSIM/VRCLightVolumes.
  Notice: `LICENSE-VRCLightVolumes.md`.
- **VR Stage Lighting (VRSL)** by AcChosen. The VRSL section. github.com/AcChosen/VR-Stage-Lighting.
  Notice: `LICENSE-VRSL.md`.
- **ThryEditor** by Thryrallo. VixForge Editor, the material inspector and locking, is a fork of it.
  github.com/Thryrallo/ThryEditor. Notice: `LICENSE-ThryEditor.md`.
- **Hyperspace, Tunnel style**, adapted from a shader by **Yohei Nishitsuji** (@YoheiNishitsuji).
  Notice: `LICENSE-Nishitsuji.md`.
- **Filamented** by **Silent**. Smooth Filter on World Surface and World Fur is adapted from its bicubic
  filter. gitlab.com/s-ilent/filamented. Notice: `LICENSE-Filamented.md`.
- **Poiyomi Toon** by **Poiyomi** (Poiyomi Labs). Dissolve, Color Adjust and Alpha Options (its Global Mask
  too) are adapted from its sections of the same names, the dither pattern included, and so are each
  decal's Alpha, Alpha Blend Mode and Hue Shift. All of them keep the same property names.
  github.com/poiyomi/PoiyomiToonShader. Notice: `LICENSE-PoiyomiToon.md`.
- **Björn Ottosson**. Hue Shift's gamut clipping and its Okhsv colour space follow his OKLab work; the
  clipping came by way of Filament's colour code (github.com/google/filament). bottosson.github.io.
  Notice: `LICENSE-Ottosson.md`.
- **alwan** by **Soufiane Khiat**. The Okhsv conversion in Hue Shift is adapted from this colour science
  library. Notice: `LICENSE-alwan.md`.
- **Unity's built-in shaders** by Unity Technologies. The shaders started as the output of Unity's surface
  shader compiler, and the lightmapped light setup follows `UnityGI_Base`. Notice:
  `LICENSE-UnityBuiltinShaders.md`.

## Inside VixForge Editor, carried over from ThryEditor

ThryEditor brought in work by other people, and it keeps their credit here:

- The shader optimizer that locks materials: **Kaj** (DarthShader), github.com/DarthShader/Kaj-Unity-Shaders.
  Its MIT notice is at the top of `ShaderOptimizer.cs`.
- The sRGB warning: **z3y**. Auto anchor: **Pumkin** (rurre). Property renaming on lock: **RequiDev**.
  Shared locked shaders: **Float3**.
- The material cleaner: **lukis101**, github.com/lukis101/VRCUnityStuffs. Its MIT notice is in
  `MaterialCleaner.cs`.
- The static batching sort fix: **apkd**, github.com/apkd/UnityStaticBatchingSortingPatch.
- The TGA loader: **aaro4130**, from the Unity forum. Inspector capture: **markeahogan**, from a GitHub gist.
- The enum and mask drawers follow Unity's own `MaterialEnumDrawer`.
- Tile renaming in UV Tile Discard: the idea came from an anonymous user, and **BluWizard LABS** built it into
  Poiyomi's ThryEditor, github.com/poiyomi/ThryEditor.

## Works with, not bundled

- **VRCFury** by Senky. Locking runs before VRCFury, and the shaders keep working with its SPS. Get VRCFury
  from vrcfury.com.
- **ProTV** by ArchiTechVR. The Director can take its music from ProTV.
- **UdonSharp** by MerlinVR. The Director's world scripts are written in it.
- **Translate Materials** reads materials made for Unity Standard, REDSIM's Light Volume PBR, Poiyomi, lilToon
  and VRChat's Toon Standard, and the packed map setup button reads the Poiyomi and Mochie Metallic Maps layout.
  The lilToon decal conversion follows lilToon's own decal placement, github.com/lilxyzw/lilToon.

## Ideas and thank-yous

- **XorDev** (@XorDev). The Weave style of Hyperspace was written from scratch after seeing a lattice effect
  XorDev published on X. None of their code is used.
- **Ghostpaw**. Infinity Mirror started as their idea.
- **BeatHologram** on Booth. Ghost's beat-driven edge glow and its scanlines with a glitch, and Hologram's beat
  pulse, colour ramps, scanlines and glitch that shifts the body, started from a look at it. It ships no
  license, so Ghost and Hologram are written from scratch and none of its code is used.
  booth.pm/en/items/8426272.
- **IridescentAuroraPearl** by yossy222 on Booth. Pearl's aurora colours, rainbow shimmer, high shine, back face
  tint and the root to tip gradient that keeps brightness started from a look at it. It ships no license, so
  Pearl is written from scratch and none of its code is used. yossy222.booth.pm/items/8615318.
- **Warren** (Warren's Fast Fur Shader). Real Fur's density, its speckle-free strands and its screen-size
  detail, and Touch reading hands from the depth texture, follow his ideas. They are written our own way, and
  no Fast Fur code is used. warrenwolfy.gumroad.com/l/atntv.
- **Poiyomi** and **Razgriz**, who built UV Tile Discard for Poiyomi Toon. Ours keeps the same property names
  so toggles made for theirs work on ours, and its tile test is written from scratch.
  github.com/poiyomi/PoiyomiToonShader.
- **Poiyomi** and **lilToon**. The rim light rebuild started from a look at how they draw a rim, and GPU
  instancing through tessellation follows the way lilToon carries the instance ID. github.com/lilxyzw/lilToon.
- **Sam Hocevar**, for the RGB and HSV conversion the colour controls use.

## Methods and papers

- GGX highlights and Smith visibility: Walter et al. 2007, Heitz 2014.
- Diffuse: Burley 2012 (Disney).
- Reflections: the split-sum approximation, Karis 2013 and 2014.
- Light Size: widening the highlight for a light with a size, Karis 2013.
- Specular occlusion from ambient occlusion: Lagarde and de Rousiers 2014 (Moving Frostbite to PBR).
- Multi-scatter energy compensation: Kulla and Conty 2017, as Filament writes it.
- Brushed reflections: Filament's bent reflection vector.
- Specular anti-aliasing: Kaplanyan et al. 2016, Tokuyoshi and Kaplanyan 2019.
- Fabric sheen: Estevez and Kulla 2017, with the Ashikhmin visibility term as Neubelt and Pettineo use it.
- Fur highlight: Scheuermann 2004, after Marschner et al. 2003.
- Sharper Probes: the Geomerics non-linear reconstruction of light probes.
- Stochastic tiling: the triangle grid from Heitz and Neyret 2018.
- Noise: PCG3D from Jarzynski and Olano 2020 (Hash Functions for GPU Rendering).
- Dissolve's OKLab hue shift: the OKLab colour space by Björn Ottosson, bottosson.github.io/posts/oklab.
- Sheer & Lace edge thickening: the Beer-Lambert path through a thin layer, which grows as one over the
  cosine of the viewing angle.
- Sheer & Lace Tulle: the nearest-centre hexagon tiling shared on Shadertoy by Shane, with the hexagon
  distance Inigo Quilez describes at iquilezles.org.
- Unified Light: the research page at vixencreations.github.io/VixenToolBox/research.html lists every
  reference behind it.
