***

# VixForge - Changelog

All notable changes to the VixForge project will be documented in this file.

***
## [2.17.0] - 2026-09-05
*Quest conversion for worlds, and the World Engine learns collision layers, baked lighting and four more packages.*

### Added
* **Quest World Converter**, a new tool at `VixenTools/Scene/Quest World Converter`. Worlds never had a Quest conversion path, only avatars did. Press **Scan Open Scenes** and it collects every material on your renderers, terrains and skybox, then **Convert Selected Materials** writes Quest-ready copies into `Assets/VixenTools/Quest World/<Scene>/`. Your originals are never touched. Materials already using a Quest shader are detected and left alone. **Point the scene at the new materials** is off by default, and asks before it swaps anything.
* **Collision layers, not just collider shapes.** The World Engine now reports what your colliders actually are, and reads your project's real collision matrix to find geometry players will walk straight through. It also catches a concave mesh collider under a physics Rigidbody, which Unity cannot simulate at all.
* **Baked lighting checks.** New findings for objects set to Contribute GI that sit under a Rigidbody, so their baked lighting stays behind when they move; for moving objects with Light Probes switched off in a baked scene; for a baked scene with no probes and no Light Volumes; and for a probe group that was never baked. If Light Volumes is in your scene, the probe findings stay quiet, because Light Volumes already covers that job.
* **Post processing checks.** Missing Reference Camera, a reference camera with no Post Process Layer, a volume layer set to Nothing, volumes sitting on a layer the camera is not watching, volumes with no profile, and Temporal Anti-Aliasing, which VRChat does not accept.
* **GPU Particle Volumes** support, covering a manager with no mesh assigned, volumes recalculating every frame, and empty include or exclude slots.
* **TXL Player Audio, Portal and Misc** support, covering duplicate audio managers, zones with no trigger or no settings, portals missing their station or zone, chairs with no station, and material swappers with nothing to swap.
* **Light Volumes 3.0** support for the runtime shadow baker, catching one with no target assigned and one left on Realtime.

### Fixed
* **Two World Engine checks had quietly stopped working.** The VRSL check looked for a class that no longer exists in VRSL, and the ProTV check looked for a `UrlInput` component that ProTV 3 removed. Both had been finding nothing at all. They now look for what those packages actually ship.
* **The Udon sync fix wrote a value that does not exist.** Aligning an Udon behaviour's sync mode set the field to an undefined number, and VRChat's own setter then copied that number onto every other Udon behaviour on the same object. It now sets None properly.
* **Udon instruction counts were inflated.** The heavy script warning counted data declarations as executed instructions. Measured across 251 real Udon programs, counts were about 22 percent high and up to 43 percent on individual scripts.
* **Colliders and contacts could be deleted from an avatar.** The Optimization Suite only protected a PhysBone collider or contact when its Root Transform was filled in, but leaving that empty is the default and means the component uses its own transform. Those bones were left eligible for removal. Every collider and contact now protects the object it sits on.
* **The Quest converter offered shaders VRChat rejects.** `Mobile/Standard` was a target and `Standard` was the fallback, and neither is on VRChat's mobile shader list, so uploads replaced them. Both converters now read that list from the SDK itself and offer all ten shaders on it.

### Changed
* **The shader replacer works with our shaders.** `VixenWear/` and `VixForge/` shaders are the replacement targets and are never flagged for conversion. Retired `VixenWorld/` and `Vixen/` shaders from earlier releases are flagged so you can move off them.
* **Poiyomi materials are treated as broken when Poiyomi is missing.** A locked Poiyomi material still renders but its inspector will not load and it cannot be unlocked. These used to be protected from replacement by the rules covering hidden and optimized shaders. When Poiyomi is not in your project they are now offered for conversion instead.
* **More third-party shaders left alone.** Towel, GPU Grass, GPU Infinite Grass, QvPen, Silent and Mochie shaders are no longer flagged for conversion.

## [2.16.0] - 2026-08-30
*A new tool for when your materials quietly disagree with each other, and our X handle is now @VixForge.*

### Added
* **Material Conflict Finder**, a new tool at `VixenTools/Avatars/Material Conflict Finder` and on the Hub's Avatar Tools grid. Drop an avatar in, press **Analyze Materials & Toggles**, and it reports what it found in four sections, each with the buttons to settle it.
* **Cross-Material Property Mismatches.** Toggle properties holding different values across your materials, grouped by value so you can see which materials sit on which side. **Sync All to 0.0 (OFF)**, **Sync All to 1.0 (ON)** and **Align Majority** settle a whole group in one press, and **Select All** and **Ping** take you to the materials involved.
* **Orphaned Shader Keywords.** A toggle switched off while its shader keyword is still enabled. That is the state where a feature reads as disabled in the inspector but still renders. **Fix** clears one, **Fix All Keywords** clears them all.
* **Animation & VRCFury Driven Toggle Conflicts.** A clip drives a toggle up to 1.0, but the keyword it needs is switched off on the material, so the animation changes nothing in game. It reads your VRC base and special playable layers, plus any clips VRCFury carries.
* **Scanned Materials Inventory.** Everything the scan looked at, in one list.
* **Locked materials read correctly.** A locked material carries no keywords, because the locker bakes them into the generated shader, so a plain keyword check calls every animated toggle broken. The finder reads the pre-lock keyword list stashed on the material instead, and leaves alone any property the lock deliberately kept animated.

### Changed
* **Our X handle is now @VixForge.** The Hub's **Twitter** button, the package and VPM listing author links, and every page on the website point at `x.com/VixForge`.

## [2.15.1] - 2026-08-16
*Every button, panel and dialog in plain English, two Hub fixes, and the PhysBone Topology Mapper is now PhysBone Blueprints.*

### Changed
* **Fix Scene Data Tool:** Moved the Fix Scene Data tool to `VixenTools/Unity Engine/Fix Scene Data` to condense the menu sizes a little more
* **Plainer names on everything you click.** Buttons and panel headers across the toolbox lost the jargon. **EXECUTE DEEP SYSTEM SCAN** is now **SCAN AVATAR**, **EXECUTE DESTRUCTIVE TOPOLOGY FIXES** is **APPLY SELECTED FIXES**, **EXECUTE TARGETED DOWNSCALE** is **Downscale Selected Textures**, **Optimization Target (px)** is **Max Texture Size (px)**, and the Optimization Suite's panels read **Settings**, **Physics Components**, **Textures** and **Memory & Limits**. The Quest engine converts with **Convert To Quest**, the Preset Manager's tabs are **Extract From Assets** and **Create New Preset**, and the Hub's **Architecture** tab is now **Overview**. Tool names, menu paths and every setting behave exactly as before: only the wording changed. The docs and the website match.
* **The PhysBone Topology Mapper is now PhysBone Blueprints**, under `VixenTools/Avatars/PhysBone Blueprints`. Its two buttons are **Save Blueprint** and **Apply Blueprint** instead of Extract Master Copy and Inject Blueprint, and new blueprints are named `Avatar_PhysBones` rather than `Avatar_MasterTopology`. Blueprints you already saved keep working: only the default name for new ones changed.
* **The Quest converter's component list dropped its "System:" prefixes.** The sections are just **Animators**, **PhysBones**, **Colliders** and so on, and the auto-culled ones say **removed for you** instead of **(Auto-Culled)**.

### Fixed
* **The update badge lands on the changelog.** Clicking the update notice in the Scene View opened the Hub on News the first time, because it asked for the Changelogs tab before the Hub had finished building it. It now waits and takes you where it says it will.
* **Old releases stopped halfway down.** Reading release 1.4.3 in the Hub's Changelogs tab cut off partway, because the Hub used to stop reading a document the moment it saw the words "Network Links", and that release happens to mention them. Every release reads to the end now.
 
## [2.15.0] - 2026-08-15
*Format before resolution, a safer purge, and a truthful version badge.*

### Added
* **Texture compression comes before texture shrinking.** The Optimization Suite now finds textures left on Uncompressed and offers to move them to high-quality block compression, and it puts that at the top of the task list, above the resize panel. An uncompressed RGBA texture costs 32 bits per pixel and a compressed one costs 8, so this is the same VRAM saving you would get by halving the resolution, without the blur. Unity picks the right block format per texture, so colour maps get BC7 and normal maps stay normal maps. Shrink the resolution afterwards if you still need to, rather than first.
* **Free quality upgrade for transparent textures.** A second task spots textures already compressed on the older DXT5 format and offers BC7 instead. Both are 8 bits per pixel, so memory does not move, but banding and block artifacts largely go away. It deliberately skips opaque textures: those sit on 4-bit DXT1 today, and moving them to BC7 would double their memory rather than cost nothing.

### Changed
* **Magick.NET updated to 14.16.0** (ImageMagick 7.1.2-29, up from 14.14.0 and 7.1.2-25). Upstream bug fixes only. Nothing about how your textures are resized or composited has changed.
* **The imaging library is Editor-only now.** It moved out of `Runtime` and ships with import settings that pin it to the Editor on Windows x64, so 25 MB of native imaging code can no longer be dragged into an Android or Quest build.
* **Animator Forge writes to `Assets/VixenTools/AnimatorForge/`.** Everything the toolbox generates now lives under one folder instead of a second `Assets/VixenForge/` one. Clips you already have keep working exactly where they are.
* **New PhysBone blueprints are named `Avatar_MasterTopology`** by default instead of carrying a test avatar's name.

### Fixed
* **Nothing in the Optimization Suite is ticked for you any more.** Every task in the Destructive Optimization Engine now starts unchecked, so the execute button does nothing until you have actually chosen something. They used to arrive pre-selected, which put a one-click purge of your hierarchy behind a button you might press while still reading the list.
* **Purging orphans and collapsing leaf bones now check whether anything is using them first.** Both passes previously looked only at components and children, so an empty GameObject that a clip toggled, or a bone a VRCFury component pointed at, read as dead weight and got destroyed. Both now build a reference set from every animation clip on the avatar (your playable layers plus anything VRCFury carries), every object reference held by a VRCFury component, contact senders and receivers, and constraint sources, then skip anything that appears in it.
* **The Hub showed the wrong VRChat SDK version.** It read the SDK version out of the toolbox's own manifest, so it always printed the minimum version we build against rather than the SDK you actually have. It now reads your installed SDK.
* **PhysBone Topology Mapper in projects without the VRChat SDK.** It was meant to open with a friendly "SDK required" screen, but the whole tool was compiled out, so it never appeared at all. The fallback works as described now.
* **Animation Workbench property categories.** Shader properties starting with `_AL` never landed in the AudioLink group. They do now, without sweeping up names like `_Alpha` by mistake.

### Removed
* Dead mesh-welding code left behind when QEM decimation replaced it, plus a duplicated property-category helper.

***
## [2.14.0] - 2026-08-10
*Animation Workbench learns to see the rest of your clip.*

### Added
* **Animation Workbench - animate components:** A **Component Property** picker now sits beside the material one. Anything Unity can key on your avatar is reachable from it: a component's enabled toggle, Transform values, fields on a script. Set your Preview Target, hit Choose, pick the property, then Add Binding.

### Fixed
* **Animation Workbench - material swaps were invisible.** A clip that swapped materials looked empty. Swap tracks are a different kind of track from the rest and were never being read, so a clip that opened on one showed nothing at all. They now list alongside everything else, marked as swaps, with each key shown as its time and the material it swaps to. Single-key swaps pinned at the start show up too, which is how most of them are written. They are there to be read rather than edited: there is nothing to ease between two materials.
* **Animation Workbench - the window opened unstyled.** It was looking for its stylesheet somewhere it was never kept, so it fell back to bare layout and logged a warning every time you opened it.

***
## [2.13.1] - 2026-08-07
*Hub Maint Removed Partners links and code*

### Removed
* **Hub:** Removed the Partnership tab, due to conflicting values and morals we are no longer partnered with Engima Industries and will no longer be supporting their needs.
* **Hub:** Removed the ShaderDoc Tab, This is now packaged seperately with the Gumroad products.
* **VixForge Ecosystem:** Removed TechnicallySane's Integrations completely due to licensing compliance.

## [2.12.0] - 2026-07-11

*Adds the Animator Forge, a new avatar tool that both diagnoses broken animators and builds fully-rigged toggles for you. New avatar bases routinely ship with FX controllers whose Sit/Action layers reference VRChat driver parameters (like `Seated`) that were never declared, spamming the SDK panel with "uses parameter which does not exist" errors. Animator Forge finds those and a whole family of related problems, and its Forge side generates a complete toggle (parameter, menu control, FX layer and states, animation clips) in one click, matched to your avatar's own Write Defaults convention. Editor tooling only; Built-in Render Pipeline / VRChat, as always.*

### Added

* **Animator Forge - Doctor (Avatar Pipeline Tools):** A new diagnostics engine that scans an avatar's base and special playable-layer controllers plus its Expression Parameters and Menu, then reports a ranked list of findings with one-click fixes. It catches: transitions and blend trees that reference parameters missing from the controller (recognizing the full set of VRChat driver-provided parameters such as `Seated`, `AFK`, `VRMode`, `GestureLeft/Right` and adding them back with the correct type); a broken mix of Write Defaults across states; menu controls and toggles bound to parameters missing from the Expression Parameters; synced-parameter cost overflow and duplicate or type-mismatched parameters; zero-weight and default-less layers; and instant or type-invalid transitions. Each finding has a Locate button that pings the offending asset and, where safe, a Fix button, plus a "Fix all safe issues" batch that resolves the unambiguous ones (missing built-in and menu parameters) in a single pass.
* **Animator Forge - Write Defaults Normalizer (Avatar Pipeline Tools):** The Write Defaults check and fix follow the widely-used VRCFury detection model: states are grouped as normal, Direct-blend-tree, or additive; the target is taken from the FX layer's majority (or the avatar-wide majority on smaller rigs); and normalization forces Direct blend trees and additive layers to always write defaults on, since those break otherwise. The result is reported with the exact offending states and applied only on request, so it never silently changes animation behavior.
* **Animator Forge - Empty Write-Defaults-Off Clip Check (Avatar Pipeline Tools):** Detects the SDK's "one or more animator states with Write Defaults disabled where the animation clip is either missing or empty" warning, using the same logic as VRChat's own scan (Write-Defaults-off states whose motion is missing, an empty clip, or a blend tree with a missing/empty child). The one-click fix assigns a shared inert clip that carries a single harmless property, which is what actually clears the warning: a genuinely empty clip still reads as empty to the SDK and would leave it flagged.
* **Animator Forge - Rig Builder (Avatar Pipeline Tools):** The Forge tab builds complete, ready-to-use rigs without hand-wiring an Animator. Five rig types are supported: **GameObject toggle** (show/hide one or many objects), **blendshape toggle** and **blendshape slider** (a radial-puppet 0 to 100 dial), **material swap**, and **exclusive group** (a radio set where enabling one option disables the rest). Every build generates the Expression parameter (synced and saved by default), the menu control or submenu, the FX layer with its states and instant transitions, and the animation clips, with new states created to match the avatar's own Write Defaults convention. It is non-destructive: it reuses an existing FX controller and expression assets or creates them if absent, clones read-only sample controllers before touching them, and never deletes existing layers or parameters.

***
## [2.11.1] - 2026-07-10

*A maintenance patch focused on restoring project compilation stability within Udon-managed world environments and refreshing our public web content.*

### Changed

* **Web Infrastructure and News Tab Updates:** Updated the official website and the existing news tab to roll out coverage for the 1,500 download milestone and keep documentation aligned with recent studio updates.

### Removed

* **AreaLitBroadcaster Component (World Diagnostics / Udon Infrastructure):** Temporarily removed the AreaLitBroadcaster component from the core distribution package. A critical dependency conflict between its automated editor scripting hooks and the UdonSharp assembly compilation pipeline was triggering a total compilation failure across the host project when imported into worlds. The component has been completely isolated and extracted from the main assembly loop to unblock world creators immediately. A rewritten, strictly decoupled version will be re-added in a future release once the Udon compilation lifecycle hooks are fully stabilized.

## [2.10.1] - 2026-07-05

*A same-cycle hotfix that finishes the per-mesh bounds work introduced in 2.10.0. The 2.10.0 auto-fit could still mis-place the culling box on meshes that carry a Blender import scale (they would vanish as you got close), and its PhysBone swing inflation over-bloated body meshes that are genuinely weighted to a far-swinging bone. Both are resolved by fitting to Unity's own posed bounds and dropping the swing-reach heuristic entirely. Built-in Render Pipeline / VRChat only.*

### Added

* **Automated Animated Material Swap Re-Mapping (VixForge Quest Engine):** The Quest conversion pipeline now natively supports animated material swaps. It scans both standard Animators and `VRCAvatarDescriptor` custom layers (Base and Special) for animation clips that swap materials at runtime. When detected, it clones the source `AnimatorController` (saving it as `[Name]_Quest.controller`) and deep-copies the affected `.anim` clips. It then rewires the object reference curves within the cloned clips to point to the new Quest-optimized materials, and recursively rebuilds the state machines and blend trees to reference the remapped clips. This completely eliminates the need for manual controller duplication and clip editing when porting avatars with material-based toggles or effects.

### Fixed

* **Auto-Fit Bounds No Longer Mis-Places the Culling Box (Avatar Optimization Suite):** The 2.10.0 fit snapshotted each mesh through `BakeMesh` and re-projected it with a hand-built matrix. On FBX meshes carrying a Blender import scale (e.g. 0.0254 or 2.54), that applied the scale a second time - collapsing the box and shoving its centre far off (a body read a centre near Y = -38 in root-bone space), so the mesh frustum-culled and disappeared the moment the camera drew close. It now reads Unity's own posed `localBounds` directly (via a momentary Update-When-Offscreen toggle), which is already in the root bone's space, so the fitted box always matches what Unity actually culls against. Verified against the Unity editor source, which draws `localBounds` inside the root bone's transform.
* **PhysBone Swing Inflation Removed (Avatar Optimization Suite):** The swing-reach inflation added in 2.10.0 still over-bloated compact meshes: a body legitimately weighted to a bone that a chain can swing about a metre received a guaranteed-worst-case box roughly double its true size, and its centre collapsed toward the hips. Guaranteed swing coverage is too conservative for static culling bounds, and swinging appendages (tail, hair, ears) are separate meshes with their own bounds anyway, so the swing-reach pass is removed. Each renderer now fits to its posed geometry plus a small (about 10%) margin for animation range, matching Unity's default tight bounds.

***
## [2.10.0] - 2026-07-04

*A combined release folding in this cycle's shader micro-updates alongside a ground-up overhaul of the Avatar Optimization Suite, and the Enigma Industries x VixForge Interactive partnership. On the shaders: an in-scene four-layer decal system, two realistic-lighting refinements, a re-sync to the current upstream VRC Light Volumes revision, and fixes for two Thry-locking regressions that dropped AudioLink and the procedural effect passes. On the tooling: a genuine QEM mesh decimator replaces the old duplicate-vertex welder, per-mesh bounds now fit in the correct space, the VRChat performance "Locate" buttons jump to the real offender instead of the avatar root, textures are downscaled selectively instead of in bulk, and a per-avatar state flag stops applied fixes from re-listing. Built-in Render Pipeline / VRChat only, as always.*

### Added

* **News Tab & Enigma Industries Partnership (VixForge Hub):** VixForge Interactive and Enigma Industries (the next chapter of Club Enigma) have partnered - two studios pooling tools, community, and immersive-world experience for VRChat. The Hub now opens on a new front-and-centre **News** tab that renders the announcement from a package-local `NEWS.md` and surfaces dynamic buttons that jump straight to the full write-up, Enigma's Discord, and Enigma's VRChat group.
* **Four-Layer Decals (Both Shaders):** A new Decals section places up to four independent decals on the material, each positioned right in the Scene view by dragging the gizmo handles. Per layer you get tint, position / scale / rotation (with optional spin), side offsets, a UV source, a packed RGBA mask channel, eight colour blend modes (Normal, Darken, Multiply, Lighten, Screen, Add, Subtract, Overlay), an emission glow, a smoothness offset so a sticker can read glossier or more matte than the latex, mirror and symmetry, and a clamp-or-tile toggle. All four decals share one texture sampler, so they add no sampler-budget cost. Off by default, so existing materials are unchanged.
* **Horizon Specular Occlusion (Both Shaders):** A new control that fades reflections a normal map would otherwise leak through the surface at grazing angles, across reflection probes, Light Volumes, LTCGI and VRSL. Full by default; set to 0 to disable.
* **Specular Highlight Clamp (Both Shaders):** A new control that caps direct point-light highlight intensity to tame sparkle on very glossy latex. Effectively off by default; lower it to clamp harder.
* **QEM Mesh Decimation (Avatar Optimization Suite):** Real Quadric Error Metric edge-collapse decimation (Garland-Heckbert), the same class of algorithm as Blender's Decimate modifier, replacing the previous "smoothweld" that could only merge coincident duplicate/seam vertices (and so removed almost nothing on a clean mesh). Each heavy mesh is driven toward a triangle target while preventing face flips (triangle normals are recomputed on every collapse, not read stale from the first pass), rejecting sliver triangles, and preserving UV/normal seams, material (submesh) boundaries, and open borders. It locks the eye / mouth / teeth submeshes and the humanoid hand bones, interpolates UV0, vertex colours and bone weights across each collapse, and remaps every blendshape. The optimal collapse position is solved and guarded the way Blender's `BLI_quadric_optimize` does it - an epsilon-regularised determinant plus an edge-distance clamp - so near-flat regions can't fling a vertex off the surface. Applied non-destructively: the decimator writes a new patched mesh asset and reassigns it through `Undo.RecordObject`, so the source mesh is untouched and Ctrl+Z reverts.
* **Decimation Target Slider (Avatar Optimization Suite):** A new *Decimation Target (tris / mesh)* slider (2,000 - 70,000, with a typed input box) on the config panel controls how aggressive the decimation is. Higher keeps more detail (softer silhouette); lower is smaller and more aggressive. Only meshes above the target are decimated, each down to the target. Default 24,000. The per-avatar state flag keys on the slider value, so moving it re-surfaces the decimation task rather than treating it as already applied.
* **Selective Texture Targeting (Avatar Optimization Suite):** The blanket "downscale every texture" behaviour is replaced by a *Texture Optimization Targeting* panel. Every processable texture gets its own checkbox, its dimensions, a `[linear/data]` tag, and a Locate button, plus Select All / Deselect All and a live queued-count. Only the checked textures are resized (down or up); nothing is touched in bulk. Smart defaults pre-select only textures that exceed the target (or, in upscale mode, fall under it), sorted largest-first, and you override freely.
* **Per-Avatar Optimization State Flag (Avatar Optimization Suite):** A versioned JSON cache under `Assets/VixenTools/Asset Database/Optimization Suite/`, mirroring the World Engine's lookup-cache pattern (load on demand, save + reimport on write, hash/version guarded). It records which Destructive Topology tasks have been applied to each avatar, keyed by a stable `GlobalObjectId` plus a per-task state signature, so a task clears from the list once a pass is made instead of re-listing forever. It only re-surfaces when the avatar's relevant state actually changes (new leaf bones, a re-imported mesh, a moved slider).

### Changed

* **Realistic Lighting, Refined (Both Shaders):** The Horizon Specular Occlusion and Specular Highlight Clamp controls above are surgical, opt-in additions to the existing realistic lighting model. Their defaults reproduce the previous look exactly, so nothing changes until you reach for them.
* **VRC Light Volumes Re-Sync (Both Shaders):** Re-synced the vendored Light Volumes include across this cycle's upstream revisions - a structural performance pass that also reworked speculars to be more physically correct and light-source-size aware (larger point, spot, and area sources fall off more softly), followed by the current early-July revision that compiles faster, folds in upstream compile-error fixes, and swaps in a cheaper fast-exp path. The one entry point whose signature changed (the point-light sampler, whose parameters were reordered) was updated at both call sites to preserve the previous look exactly; every other entry point is unchanged, so the improvements come in transparently. No new properties, samplers, textures, keywords, or shader variants, and existing materials and worlds render unchanged.

### Fixed

* **AudioLink No Longer Fails to Compile on Locked Materials (Both Shaders):** Locking a material with the Poiyomi / Thry optimizer (required before an avatar upload) could drop AudioLink from every shader pass except the first, so affected materials failed to compile with an "undeclared identifier" error. The optimizer inlines each shared include only once per shader, and the material had previously pulled the AudioLink helper in separately inside each pass, so only the first pass kept it. The AudioLink include is now hoisted into a single shared block that the engine applies to every pass, so all passes get it after locking. Affects both shaders; re-lock any affected material to pick it up. No new controls, no cost, and unlocked materials are unchanged.
* **Locking No Longer Breaks the Procedural Effect Passes (Both Shaders):** Locking a material with the Thry optimizer (required before an avatar upload, so unlocked shaders are not stripped from the build) was silently disabling the geometry stage of the procedural effect passes - the water drips, the melting goo, the AudioLink HUD ring, and the fracture shards and trails - leaving them to render incorrectly or fail to compile. The shader now tells the optimizer to keep its geometry passes when locking, so every effect survives the lock intact. No new controls, no cost, and unlocked materials are unchanged; re-lock any affected material to pick up the fix.
* **"Locate" Now Jumps to the Real Offender (VRChat Official Performance):** Every Locate button on a VRChat official performance warning used to just ping the avatar root. Each category now resolves to the actual worst offender - the heaviest renderer for the triangle count, the widest-reaching renderer for the bounding box (AABB), the renderer with the most material slots, the largest chain for PhysBone component / transform counts, the first matching component for collider / contact / light / particle / trail / line / cloth / rigidbody / audio counts, and the heaviest texture for VRAM - so Locate takes you straight to the geometry or component to fix.
* **Per-Mesh Bounds Fit Was Computed in the Wrong Space (Auto-Fit Avatar Bounds):** On avatars whose body is skinned across a scaled armature, the auto-fit produced a culling box roughly twice the correct size and mis-centred, because it computed the fit against the renderer's own transform while Unity stores and culls a Skinned Mesh Renderer's `localBounds` in the root bone's space. It now resolves the true `actualRootBone` (the one Unity draws and culls against) and snapshots the mesh through Unity's own `BakeMesh` skinner, so the fitted box matches Unity's default tight bounds. Verified against the Unity editor source, which draws `localBounds` inside `actualRootBone.localToWorldMatrix`.
* **PhysBone Swing Inflation Over-Bloated Static Meshes (Auto-Fit Avatar Bounds):** The swing-reach inflation was applied whenever a mesh touched *any* transform in a PhysBone chain - including the anchored root that never moves - and it tested against the renderer's entire bone list, which on a body is the whole skeleton. So a body would get inflated by an unrelated mane or tail chain it carries in its bone array but has no vertex weight on. Inflation is now filtered to bones the mesh actually has vertex weight on, and scales with how far down the swinging chain that weight sits; a mesh weighted only to an anchored chain root gets no inflation at all.

***
## [2.5.0] - 2026-06-13

*Full world-lighting integration pass: the latex now reacts to every supported stage/area-light system, and the legacy TV-GI tint is gone. Every integration is fail-safe - it strips or no-ops cleanly in worlds that don't have the system, so the suit never blacks out or picks up stray colour. Built-in Render Pipeline only (VRChat); HDRP is out of scope by design (a `#pragma surface` shader can't compile under HDRP/URP).*

### Added

* **AreaLit Area-Light Support (Both Shaders, `AREALIT_ENABLE`):** The latex now receives PiMaker **AreaLit** area lights, same role as LTCGI. AreaLit is analytic Linearly-Transformed-Cosines (no LUT texture), vendored and trimmed into `Editor/Avatar Tools/Shaders/cginc/AreaLit/AreaLit_Latex.cginc` (`AL_`-prefixed, no namespace, single light texture, 16-quad cap, no MSBuffer/checker). AreaLit itself ships no scene broadcast, so the suit **intercepts it at the GI level like LTCGI** via a small world-side helper: `Runtime/AreaLitBroadcaster/AreaLitGlobalBroadcaster.cs` (UdonSharp) publishes the LightCam's data as scene globals `_Udon_AreaLit_LightMesh` / `_Udon_AreaLit_Tex0` / `_Udon_AreaLit_Enable`, and the shader reads those automatically (no per-material assignment, every avatar in the world lit). A per-material pair (`_AreaLit_LightMesh` / `_AreaLit_LightTex0`) remains as a manual fallback when no broadcaster is present; both sources funnel through one `AL_ShadeCore(...)` selected by a runtime branch on `_Udon_AreaLit_Enable`. LightMesh is read with `.Load` (no sampler) and the light texture is `UNITY_DECLARE_TEX2D_NOSAMPLER` sampled through `_MainTex`'s sampler - **zero new sampler registers**. The whole include is `#ifndef SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER`-guarded (with a no-op stub) like the LTCGI uniforms, since the surface-analysis MojoShader pass can't parse object textures. Keyword-gated by the editor on `_AreaLit_Int > 0`; the broadcaster ships in its own asmdef gated on the UdonSharp package so it's excluded from avatar projects. On the INTEGRATION tab.
* **VRSL GI Wash (Both Shaders):** A new additive coloured **stage wash** on top of the existing VRSL stage-hijack. Instead of overriding emission, it spills the DMX fixtures' colour onto the suit as real light (the stage lighting you up), decoded from the same DMX grid + channel offsets (base+3/4/5) the Color Hijack reads. Controls `_VRSL_GI_Int` / `_VRSL_GI_Spec` / `_VRSL_GI_Sat` on the STAGE tab. Rides the existing `VRSL_ENABLE` keyword + `_UseVRSL` runtime gate; a TexelSize liveness probe skips it in worlds with no DMX node.
* **Fail-Safe Fallback Model (All World-Lighting Integrations):** Formalised the "no system in this world = no cost, no artifact" guarantee. Heavy paths (VRSL, LTCGI, AreaLit, Light Volumes) are keyword-stripped when off; cheap global reads (VRSL GI) are runtime float-gated and probe their data source for liveness (DMX TexelSize, `LightMesh.Load == 0`). All integration includes are vendored under `Editor/Avatar Tools/Shaders/cginc/`, so the shaders compile with every package absent.
* **MatCap Tiling & 3-Axis Scrolling (Both Shaders + Inspector):** Both matcap layers gained a `Tiling (XY)` vector (`_MatCap_Tiling` / `_MatCap2_Tiling`) and a `Scroll (X pan, Y pan, Z spin)` vector (`_MatCap_Scroll` / `_MatCap2_Scroll`). Tiling repeats the matcap around its centre so the sphere highlight stays put as the pattern repeats; Scroll drives smooth continuous motion off real time, where `X` / `Y` pan the UV and `Z` spins the matcap in degrees per second (a matcap is a 2D sphere projection, so rotation is the only third axis that reads as a true scroll). The spin angle is wrapped with `fmod(..., 360)` so sin/cos stay precise and jitter-free over long sessions. Defaults (Tiling `(1,1)`, Scroll `(0,0,0)`) are identity, so existing materials render unchanged, with no new sampler registers. Visible repeat at tile > 1 needs the matcap texture's Wrap Mode = Repeat. New rows under each matcap layer's Intensity on the INTEGRATION tab.

### Removed

* **Screen Glow / "Any TV GI" Feature:** Removed `_UseScreenGlow` and the whole `_ScreenGlow_*` stack (plus the `_Udon_VideoTex` and standalone `_Udon_LTCGI_Texture_LOD0` declarations and the `BRDF_Latex_GGX` glow block) from both shaders and the inspector. The refined lighting model is unchanged; the dedicated area-light integrations (LTCGI + AreaLit) cover screen lighting properly with real screen geometry, so the Tier-B raw-texture tint is no longer needed.
* **VixenWear Latex Ultra Shader (now a standalone release):** Both shader variants (`VixenWear/Latex Ultra` and `VixenWear/Latex Ultra SPS`), the custom material inspector, and the vendored `cginc/` includes (AudioLink, LTCGI, AreaLit, Light Volumes) have been pulled out of the toolbox; the shader now ships as its own standalone product. The world-side **AreaLit GI broadcaster** (`Runtime/AreaLitBroadcaster/`) stays so worlds can still feed avatar GI, and the **Shader Pipeline** documentation tab (`SHADERSETUP.md`) stays in VixenHub. The Avatar Validator still recognises `VixenWear/Latex Ultra` materials, so avatars wearing the standalone shader keep their packed-map checks.

### Changed

* **Built-in-Only Banner:** Both shaders now carry a header note that they target the Built-in Render Pipeline / VRChat and that HDRP/URP are unsupported by design (surface shaders can't compile there). No functional change.
* **Faster Shader Imports (Both Shaders, `only_renderers d3d11` + `SHADOWS_SOFT` skip):** Every program now carries `#pragma only_renderers d3d11`, so Unity compiles a single graphics API instead of the whole desktop set (gles3 / metal / vulkan / glcore). VixenWear is PC / Built-in-RP only and PC VRChat runs DX11, so this cuts both the in-editor reimport and the VRCFury SPS patch + import (which force-synchronously compiles every pass twice) by a large factor. `SHADOWS_SOFT` was also added to the skip_variants list to roughly halve the ForwardBase shadow-receiving variant set, at the cost of slightly harder shadow edges. Heads up: because the shaders compile DX11 only, a player who force-launches VRChat in Vulkan or DX12 (experimental, uncommon) would see the material break; standard PC clients are unaffected. `VERTEXLIGHT_ON` is deliberately left in, because VRCFury SPS socket detection reads the per-vertex light arrays that only populate under it.
* **TechnicallySane GlobalRGB Support - Held Back:** Pulled from the shipping shaders and inspector on 2026-06-11 at the request of the TechnicallySane integration's creator (holding off for now). The reader (`_Udon_TS_GlobalRGB` -> `_UseTS` / `_TS_Str` / `_TS_Sat` / `_TS_Albedo`) is removed from both Latex shaders and the INTEGRATION tab; the exact code is preserved internally for a clean re-add once cleared. No other integration is affected.

### Fixed

* **Tessellation control inverted & uncapped (base shader):** The old `Tessellation Edge Length` slider (`_Tess_Edge`, Range 1–50) was backwards and unbounded - its value is the *target edge length* fed to `UnityEdgeLengthBasedTess`, which sits in the **denominator** of the tessellation factor, so a **lower** number meant denser subdivision and the GPU's 64× per-edge cap was hit on dense displaced meshes → severe lag. Replaced with `Tessellation Detail` (`_Tess_Detail`, Range 0–1) where **higher = more detail/cost, lower = cheaper**, mapped via `edgeLen = lerp(40, 2, detail)`. The distance/screen-adaptive LOD is preserved (far/small-on-screen surfaces stay cheap) and the per-edge factor is now clamped to `VW_TESS_MAX` (32) so the close-up worst case can't melt the GPU. The SPS variant has no tessellation, so it's unaffected. **Migration:** the property was renamed, so existing materials reset to the new `0.5` default and the tessellation amount should be re-set once (the previous default of edge-length 10 ≈ detail 0.79).
* **Shader compiler warning (POM, both shaders):** Eliminated the "gradient instruction used in a loop with varying iteration, attempting to unroll the loop" warning on the ForwardBase pass. The parallax-occlusion height march now samples `_MetallicGlossMap` with `tex2Dlod` (explicit LOD 0) instead of `tex2Dgrad`, so the dynamic `[loop]` no longer carries a gradient instruction - the runtime early-out `break` (and its performance) is preserved. The heightfield is now marched at mip 0 (standard for POM).

***
## [2.4.0] - 2026-06-08

### Added

* **Wet & Run-Off Layer (`_UseDrip`, Both Shaders):** New POLISH-tab effect that soaks the latex like it just came out of the pool. The "Soaked Look" stack (`_Wet_Amount`, `_Wet_Darken`, `_Wet_Smoothness`, `_Wet_Sheen`, `_Wet_Flatten`) darkens the masked area for water absorption, drives reflections toward a near-mirror water film, layers a dielectric Fresnel sheen on the clearcoat, and flattens micro-detail. Layered on top, animated "Run-Off Rivulets" (`_Drip_Density`, `_Drip_Width`, `_Drip_Coverage`, `_Drip_Speed`, `_Drip_Strength`, `_Drip_Normal`) stream vertical water trails down the UV. A B&W `_DripMask` + `_DripMaskCh` channel picker scopes the whole effect. The soak and rivulets run on both the base and SPS shaders.
* **Clear 3D Drips (Geometry, PC Only):** On the base `VixenWear/Latex Ultra` shader, `_Drip3D_Strength` adds real water droplets emitted by a dedicated geometry stage (`dripGeom`): they swell on downward-facing wet areas, form a neck, pinch off, then fall away as free geometry and dry out. Tuned by `_Drip3D_Scale` (droplet size, roughly millimetres), `_Drip3D_Sheen` (glassiness), and `_Drip3D_Fall` (fall distance), tinted to the Clearcoat Tint. Physics: `_Drip_Sway` adds surface-tension wobble and a breeze that grows with fall distance, `_Drip_BodyFollow` makes attached drops run down along the body before detaching (faked body collision), and `_Drip_FloorCollide` pins drops to the shared world floor and spreads them into a fading puddle. Not present on the SPS shader or Quest (the geometry stage is stripped there).
* **Goo Melting Sag (`_UseGoo`, Both Shaders):** Gravity-aligned vertex melt that mimics runny/melting latex, running in the displacement stage so it benefits from tessellation on the base shader (more verts = smoother strands). `_Goo_Strength` is the master intensity, `_Goo_Reach` extends how far it sags in world units, `_Goo_Variation` drives procedural FBM noise from uniform to wildly uneven strands, with `_Goo_Noise`, `_Goo_Speed`, and `_Goo_Droop` shaping the tendrils. `_Goo_ToGround` + `_Goo_GroundY` pull the melt toward the world floor regardless of avatar height. Physics: `_Goo_Sway` / `_Goo_SwaySpeed` give a per-strand pendulum swing, `_Goo_BodyFollow` flows goo along the body instead of through it, `_Goo_FloorCollide` clamps to the floor height, and `_Goo_Pool` spreads landed strands sideways into a puddle. Scoped by `_GooMask` + `_GooMaskCh`.
* **Reflection / Specular Masks (Poiyomi / Mochie Compatibility):** New `_UsePackedMasks` gate on the SURFACE tab with `_ReflMask_Ch/_Inv/_Str` and `_SpecMask_Ch/_Inv/_Str`. The Reflection Mask dims environment / reflection-probe specular (including clearcoat env, Light Volume, and LTCGI reflections); the Specular Mask dims direct-light highlights. Channel defaults match Mochie packing (B = reflection, A = specular); matcaps keep their own masks and are untouched. Both default to 1.0 (no effect) until enabled.
* **One-Click Poiyomi / Mochie Metallic Map Setup:** New "Set Up for Poiyomi / Mochie Metallic Map" button on the SURFACE tab translates a Mochie/Poiyomi "Metallic Maps" texture (R:Metallic G:Smoothness B:Reflection Mask A:Specular Mask) onto the packed PBR in one shot - sets the four channel selectors, disables AO, and enables the reflection + specular masks. Backed by a null-safe `SetF()` editor helper so it no-ops cleanly on the SPS variant where a property is missing.
* **Polish Layer Master Gate (`_UsePolish`):** New top-of-POLISH toggle (default ON) that wraps the entire polish lighting layer - clearcoat, thin film, SSS, transmission, anisotropy, rim, and multi-scatter compensation - behind a single switch. Off collapses the material to a flat GGX base for a clean matte look or a cheaper variant. A per-pixel `_PolishMask` + `_PolishMaskCh` scopes the layer to painted regions.
* **Flying-Shard Fracture Engine (PC Only):** The 2.3.0 single-value `_Vtx_Fracture_Str` scatter is replaced by a full shard system driven by new geometry passes (`shardGeom` + `trailGeom`). `_Vtx_Fracture_Amount` is a manual hold/animate dissolve, `_Vtx_Fracture_Dist` sets shard hover distance, `_Vtx_Fracture_Spin` tumbles them, `_Vtx_Fracture_Str` is now an AudioLink jitter, and `_Vtx_Fracture_Spiral`, `_Vtx_Fracture_Lift`, `_Vtx_Fracture_Float`, and `_Vtx_Fracture_Trail` shape the dispersal and motion trails. Shard coloring adds `_Shard_ColorMod` (hue shift) + `_Shard_ColorMod_Speed`, and `_UseShardCC` + `_Shard_CC_Str` blend the shards toward the live AudioLink ColorChord. The body opens up as the fracture progresses; the SPS shader has no geometry pass, so it dissolves via the main-pass clip only (no flying shards).
* **Autocorrelator Ring Effects (PC HUD):** The cyber HUD autocorrelator ring gains four independent, per-band effects - `_Cyber_Auto_Shimmer`, `_Cyber_Auto_Pop`, `_Cyber_Auto_Sizzle`, and `_Cyber_Auto_Electrify` - each with its own band selector (`_..._Band`) so a different AudioLink band can drive each effect, mirroring the VU-meter per-band pattern. When AudioLink isn't running, each effect falls back to a `_Time`-driven idle level so it stays visible while authoring. The SPS shader declares the props inert for inspector/copy-paste parity.
* **Self-Playing AudioLink Console (VU "Console" Style):** New `_Cyber_VU_Style` selector (Console / Bar) replaces the old `_Cyber_VU_Band`. The Console style renders a full self-playing AudioLink control panel (gain/threshold/hit-fade/falloff sliders, 4-band readout, theme/ColorChord swatches, and an autocorrelator scope) ported from the upstream `AudioLinkUI-Functions.cginc`, now vendored into the package's `cginc/` folder. The sliders are display-only readouts of the live AudioLink state.
* **Per-Widget HUD Reaction Bands:** The Waveform, DMX Grid, and Autocorrelator HUD segments each gain their own band selector (`_Cyber_Wave_Band`, `_Cyber_DMX_Band`, `_Cyber_Auto_Band`) so every widget can react to a different AudioLink band instead of sharing one global selection.

### Changed

* **LTCGI Avatar Mode On By Default:** `cginc/LTCGI_config.cginc` now enables `LTCGI_AVATAR_MODE` and `LTCGI_BLENDED_DIFFUSE_SAMPLING` out of the box, tuning the vendored LTCGI path for avatar use and smoothing diffuse area-light contribution with a slight extra screen-color sample. The LTCGI AudioLink no-op fallback `#include` was also re-pointed from the upstream `at.pimaker.ltcgi` package path to the package-local `cginc/LTCGI_AudioLinkNoOp.cginc`, so the LTCGI AudioLink branch resolves even when the LTCGI package isn't installed.
* **POLISH Tab Reorganization:** The POLISH tab now opens with the Polish Layer gate (clearcoat / thin film / SSS / transmission / anisotropy / rim / multi-scatter collapse under it), followed by the new "Wet & Run-Off" and "Goo (Melting Sag)" sections, then the existing Outline section. Each new effect carries inline help boxes explaining the soak-vs-rivulet split, the PC-only geometry drips, and the shader limits on true inertial physics (drive a PhysBone for real swing).

## [2.3.4] - 2026-06-04

### Changed

* **Changelog Backfill (Maintenance):** Version-bump release that backfilled the `2.3.3` changelog entries that shipped late. No functional, shader, or editor changes - documentation only.

## [2.3.3] - 2026-06-04

### Added

* **SPS-Compatible Shader Variant (`VixenWear/Latex Ultra SPS`):** Shipped a dedicated, tessellation-free clone of the Latex Ultra shader so VRCFury's SPS (penetrator) patcher can rewrite the surface pragma's vertex function without a compile error. VRCFury rewrites `vertex:disp` to consume `SpsInputs` but leaves the `tessellate:tessEdge` entry untouched, which fails with a struct type mismatch on the base shader; the SPS variant drops the hull/domain tessellation stage entirely while preserving displacement at vertex resolution (`disp()`) and per-pixel via parallax raymarching. Identical property layout and inspector to the base shader. The build preprocessor, variant stripper, media-state fixer, and keyword sync now resolve both shaders through a shared `VixenWearBuildPreprocessor.IsVixenWearShader()` check.
* **Backface-Extrusion Outline Pass (Both Shaders):** New keyword-gated (`_OUTLINE_ON`) Cull Front outline rendered as PASS 0 ahead of the main forward pass. The toggle's *off* state is the no-keyword default, so a material without outlines compiles and runs with zero added cost. Exposes `_OutlineColor`, an HDR `_OutlineEmis` glow, `_OutlineWidth`, `_MaxOutlineWidth` (distance clamp), `_OutlineViewFudge` (pushes the shell toward the camera to mitigate z-fighting), an `_OutlineMask` with a None/R/G/B/A channel selector, and AudioLink reactivity via `_AL_Band_Outline` + `_AL_Outline_Mod`. Width auto-scales with eye depth so the outline reads as a constant visual thickness at distance instead of vanishing. Surfaced under a new "Outline (Backface Extrusion)" section on the POLISH tab.
* **Official VRChat Performance Scan (Optimization Suite):** The Avatar Validator now runs VRChat's own `AvatarPerformance.CalculatePerformanceStats`, so its rating matches the upload screen exactly and surfaces the ~19 categories the hand-rolled hardware-cap panel never measured - particles, lights, cloth, audio sources, constraints, contacts, PhysBone collision checks, and more. Renders a new "VRChat Official Performance" panel with the authoritative overall rating plus any above-`Info` category warnings. Fully additive and exception-guarded; a missing SDK calculator simply hides the panel.
* **Texture Upscale Mode (Optimization Suite):** Added a `Resize Mode` selector (Downscale / Upscale). Upscale grows undersized textures with a Mitchell filter plus mild adaptive sharpening (skipping anything already at or above the target), complementing the existing Lanczos downscale path.
* **Shared Magick.NET Kit (`VixenMagickKit`):** New process-wide `[InitializeOnLoad]` helper that centralizes all ImageMagick usage. Forces OpenMP to use every CPU core (some Windows configs default to a single thread), exposes one `IsProtectedAsset()` policy gate, and provides `TryLosslessOptimize()` - a lossless re-encode that reads bytes through managed I/O first (never holding the source handle open) and runs `ImageOptimizer` with `OptimalCompression`. Files of 10 MB or less get the full 4-pass quality search; larger files drop to single-pass to avoid the multi-minute-per-file lockup that OptimalCompression caused on 30 MB upscale targets.

### Changed

* **Per-Mesh Auto-Fit Bounds (Replaces Universal 2.5m³ Box):** The `OPTIMIZE_BOUNDS` task now sizes each renderer's culling bounds from its bind-pose AABB transformed into root-bone local space (per Unity's SkinnedMeshRenderer docs), instead of stamping a blunt 2.5m³ box onto every mesh. Static meshes get a 1.5x margin (+25% per side) for animation drift; meshes skinned to any `VRCPhysBone` subtree get 3.0x (+100% per side) to cover runtime swing that import-time bounds can't predict; a 0.3m floor guards degenerate stub meshes. `updateWhenOffscreen` stays false to preserve the VRChat performance win.
* **Lossless Image Optimization Across Every Image Tool:** Badge Maker, the Quest Conversion Engine, and the World Engine texture passes now fire `VixenMagickKit.TryLosslessOptimize()` after every write, shrinking PNG/JPEG/GIF/ICO output with no quality loss. Unsupported formats (TGA, DDS, EXR) are skipped silently so callers can fire it blindly.
* **AdaptiveSharpen Replaces UnsharpMask (Quest Conversion):** Texture downscales now finish with an edge-targeted `AdaptiveSharpen` instead of the previous mild `UnsharpMask`, producing visibly crisper detail after a Lanczos shrink without amplifying noise in flat skin, hair, and background regions.
* **Realistic Ambient Occlusion (Both Shaders):** Removed the `GTAOMultiBounce` approximation from the BRDF. Indirect diffuse and LTCGI diffuse now use raw scalar occlusion (Poiyomi-style), dropping the colored multi-bounce tint that pushed AO away from a neutral darkening.
* **Non-Allocating Triangle Counting (Validator):** Poly-count tallying and heavy-mesh detection moved from `Mesh.triangles.Length` - which allocates a full `int[]` copy on every access - to a zero-allocation `Mesh.GetIndexCount` sum across submeshes, cutting GC pressure on dense avatars.
* **GPU Instancing on the Main Latex Pass:** Added `#pragma multi_compile_instancing` to the forward pass so the shader participates in VRChat single-pass-stereo GPU instancing / avatar batching in VR.
* **Resize UI Rework (Validator):** The optimization-target control moved from a 256-4096 slider to Unity's standard Max-Size preset ladder (32 to 16384) via a `PopupField`, and the whole resize run is now wrapped in a cancelable progress bar so the editor is no longer silently frozen during multi-minute Magick passes.
* **Website URL & Authorship Correction (9 Pages):** Reverted every page's canonical, Open Graph, Twitter, and JSON-LD URLs from the not-yet-live `vixforge-interactive.github.io/Toolkit` pattern back to the actually-deployed `vixencreations.github.io/VixenToolBox`, and flipped `meta author` / JSON-LD founder from `Trae` back to `Vixenlicous`. Keeps social unfurls and search canonicalization pointed at the real GitHub Pages site.

### Fixed

* **Magick.NET File-Handle Leaks (Badge Maker, Quest Converter, World Engine):** Switched every path-based `new MagickImage(path)` constructor to `new MagickImage(File.ReadAllBytes(path))`. The path constructors leaked OS file handles inside Unity, locking source textures until an editor restart; reading the bytes through managed I/O releases the handle before Magick ever touches the file.
* **Protected Asset Corruption (Quest Converter, World Engine):** Texture resize/optimize passes now skip shader-internal and HDR data textures - Poiyomi / lilToon / Sunao internals, `Editor Default Resources`, and any `.exr` / `.hdr` / `.cubemap` / `.rendertexture` - through the shared `IsProtectedAsset()` gate. Resampling these corrupts the data the shader reads and triggers a Unity reimport storm; they now pass through untouched.
* **Emission Stripped From Cloned Materials (`EmissiveIsBlack`):** Freshly cloned materials (e.g. VRCFury swap targets) carried Unity's default `EmissiveIsBlack` GI flag, letting the build pipeline strip `_EmissionColor`, `_EmissionMap`, and `_EmissionColor2` even though the shader emits. `VixenWearEditor` now clears the flag on keyword sync and on first shader assignment, and the build preprocessor persists that flag change to disk even when the material's keyword set was already in sync.

## [2.3.2] - 2026-05-29

### Added

* **Accessory Mounting Engine:** New avatar pipeline tool (`VixenTools/Avatars/Accessory Engine`) that clones sterile armatures from a source rig and surgically mounts accessories onto the resulting hierarchy. Ships with two pipeline modes — `FullGeneration` clones a fresh sterile armature while `AppendToExisting` reuses an existing one — and two mount strategies. `DestructiveAutoRig` recursively bakes child meshes onto the target bone with full blendshape, normal, and tangent preservation, then locks the accessory root via a ParentConstraint to keep PhysBones intact. `KinematicConstraint` skips baking entirely and applies a parent-constraint mount, intended for rigid prefabs, particles, and audio sources. Baked meshes serialize to `Assets/VixenTools/Meshes/BakedAccessories/` with a step-through folder bootstrap so the path resolves cleanly on fresh installs.
* **Vixforge Interactive Umbrella Branding:** Reframed the entire public portal under the new Vixforge Interactive studio name. Every page's sidebar caption flipped from `Ecosystem Matrix` (and the truncated `Ecosystem` variant carried by three pages) to `by Vixforge Interactive`, surfacing the parent-studio relationship at first glance without retiring the `VIXENTOOLS` wordmark or its underlying VPM identifier.
* **JSON-LD Organization Schema Promotion:** Every public page's structured-data block now declares `Vixforge Interactive` as an `Organization` with explicit `author` and `publisher` roles, replacing the legacy `Person` schema (`Vixenlicious`) so Google Rich Results, LinkedIn org cards, and Slack/Discord/Twitter embeds align with the new studio identity rather than the original creator handle.
* **Product / Studio Naming Convention:** Locked in the two-tier naming split - `Vixforge Toolkit` for the package/product line, `Vixforge Interactive` for the parent studio. Product pages (`index`, `tools`, `docs`, `changelog`, `support`, `why-choose-us`) now lead with the Toolkit name; architecture/network pages (`social`, `extended-projects`, `ai-transparency`) lead with the Interactive name.

### Changed

* **Console Prefix Normalization (`[VixForge]`):** Flipped 60 stray `[VixenTools]` Debug.Log prefixes across 8 editor files (`AccessoryArmatureCloner`, `BulkPresetGenerator`, `VixenBadgeMaker`, `QuestConversionEngine`, `PhysBoneTopologyMapper`, `FixSceneData`, `AnimationWorkbenchWindow`, `VixenAvatarValidator`) to `[VixForge]`, completing the partial rebrand started in 2.3.1. Window titles, panel headers, docstrings, and progress-bar captions also normalized. `namespace VixenTools.Editor`, `VixenTools/...` menu paths, `EditorPrefs` keys, and the `com.vixencreations.vixens-toolbox` package id are intentionally untouched to preserve user state and muscle memory.
* **"Matrix" Vocabulary Sweep:** Replaced "Matrix" with plain-language "System" (or "Engine" where the term refers to an active process) across user-facing labels, button text, foldout headers, comments, dialog strings, and internal CSS/identifier names. Renames in `VixenWorldSpider` include `InitiateFullMatrixScan` → `InitiateFullSystemScan`, `RenderDiagnosticMatrix` → `RenderDiagnosticSystem`, the `_matrixContainer` field, and the `.matrix-foldout` USS class. `QuestConversionEngine` foldout labels shift from `Matrix: PhysBones` / `Matrix: Colliders` / etc. to the `System: ...` prefix. `Matrix4x4` Unity API calls, third-party shader includes (LightVolumes, LTCGI, AudioLink), and historical changelog entries were left untouched.
* **"Forged in Digital Fire" Visual Redesign:** Comprehensive industrial-aesthetic overhaul of `styles.css`. Palette: `--dark-void` deepened from `#06020a` to `#04060a`, `--glass-bg` darkened to a near-opaque `rgba(10, 14, 20, 0.85)` and `--glass-border` flipped from pink to a 25%-alpha cyan, neutral text recolored to `#d1d5db`, `--neon-pink-glow` and `--neon-cyan-glow` opacity dialed back from `0.6` to `0.4`, and `--base-layer-luminance` lowered from `0.15` to `0.12`. Panel corners sharpened (16px -> 8px -> 4px in places); webkit scrollbars halved to 6px with rectangular thumbs and a 1px tracked border for a machined-edge feel; sidebar nav padding tightened from `1.25rem` to `1rem` with a 90deg-cyan-gradient active state replacing the solid fill. Section dividers, button hover lifts, and the support/comp/tool card families all rebuilt around the new colder, low-saturation industrial palette.
* **Background Art Z-Stack Fix:** Corrected a layered painting bug where `.background-art` (the blurred hero JPEG) was rendering correctly into the DOM but never reached the screen - `body`'s opaque `background-color: var(--dark-void)` was sitting on top of the `position: fixed; z-index: -1` element. Body fill flipped to `transparent`; `<html>` still paints `--dark-void` as the FOUC fallback while the cloak/reveal layer holds the page invisible until `app.js` flips `.app-ready`. Blur tightened from `blur(8px) brightness(0.4)` to `blur(12px) brightness(0.25)` to match the deeper void palette.
* **Cross-Page SEO Refresh (9 Pages):** Rewrote every public page's full SEO block - `title`, `meta description`, `meta keywords`, `meta author`, canonical URL, full Open Graph block, full Twitter card block, and JSON-LD structured-data payload. Canonical pattern moved to `https://vixforge-interactive.github.io/Toolkit/{page}.html`; `og:site_name` now reads `Vixforge Interactive` site-wide; `twitter:site` swapped from `@VixenVRC` to `@Vixforge`; `meta author` flipped from `Vixenlicious` to `Trae | Vixforge Interactive`. Page-specific copy refreshed to reference Unity 2022.3, UdonSharp, and the Vixforge Toolkit product line where appropriate.
* **Typography & Density Tightening:** Sidebar nav font weight reduced from 600 to 500 (600 reserved for the active state), `h1` letter-spacing cut from 2px to 1px and font-size from 2.5rem to 2.2rem, panel padding and border-radii tuned for a denser, software-tool body. `caption1` foreground recolored from lavender (`#bfa8d2`) to cool grey (`#9ca3af`) to match the new metal palette; `.id-badge` and `.badge` rebuilt as `monospace` chips with 4px corners.

### Fixed

* **Hidden Background Image (All Pages):** The blurred hero JPEG (`background.jpg`, 1434x672) loaded on every page but never appeared because `body` painted a solid `--dark-void` fill on top of the `z-index: -1 / position: fixed` `.background-art` element. Now transparent at the body layer; the image is visible on every page once `app.js` flips `.app-ready`.
* **Stale `Vixens Toolbox` in Open Graph Site Name:** `og:site_name` previously read `Vixens Toolbox` across every page, leaking the legacy product brand into every Discord/Slack/Twitter unfurl regardless of which subpage was shared. Now consistently `Vixforge Interactive` site-wide.
* **Inconsistent Sidebar Taglines:** Three of nine pages (`docs`, `ai-transparency`, `why-choose-us`) carried a truncated `Ecosystem` caption while the other six carried `Ecosystem Matrix`. Both variants normalized to `by Vixforge Interactive` for uniform navigation copy across the portal.

## [2.3.1] - 2026-05-28

### Added

* **Furality Ultra Automated Integration:** Engineered full pipeline support for the Furality Ultra convention layout. The `VixenBadgeMaker` now autonomously targets the mathematically precise top-left UV quadrant (`[500, 300]` base bounds) to perfectly isolate the "First Class" name and title plates directly from the standard 4K texture sheets without pixel bleed.
* **Modular Shader Subsystem Support:** Intercepted and integrated Furality's new `.fmodular` architecture. The generator tool now natively registers, targets, and assigns the `Furality/Modular/Standard` shader directly to generated badge materials, ensuring perfect 1:1 parity with their latest SDK infrastructure.

### Changed

* **Latex BRDF Optimization (Performance):** Gutted and re-engineered the core specular lighting loop in the Latex Shader. Shifted from a computationally heavy, multi-pass GGX evaluation to a highly optimized split-sum approximation. This drastically reduces the overall GPU instruction count while maintaining the hyper-glossy, anisotropic surface tension.
* **AudioLink Data Fetching (Performance):** Streamlined the AudioLink sampling architecture within the latex material. Stripped out unconditional dynamic branching in favor of static compile-time toggles for FFT queries, significantly smoothing out frame delivery in highly populated, light-heavy VRChat instances.

### Fixed

* **Ultra Layout Conditional Bleed:** Corrected a structural logic flaw in the `VixenBadgeMaker` where legacy "Somna" UI checks were preemptively consuming the new "Ultra" array paths. Structurally isolated the string evaluations so the `Badge{Tier}_DIF` and `_EMI` conventions natively route to the correct layout JSON without cross-contamination.
* **Latex Interpolator Overflow:** Resolved a critical SM5 register bottleneck that was causing localized frame-drops during intense directional lighting calculations. Packed the roughness, ambient occlusion, and subsurface scattering masks into a singular, tightly bound `float4` interpolator, freeing up crucial `TEXCOORD` slots for real-time vertex operations.

## [2.3.0] - 2026-05-23

### Added

* **GGX BRDF Rewrite (VixenWear Latex Ultra):** Tore out the legacy BRDF3 lobe and replaced it with an industry-standard GGX pipeline: `D_GGX` (Trowbridge-Reitz), `V_SmithJointGGX`, `F_Schlick`, `Burley_Diffuse`, and a Karis split-sum (`EnvBRDFApprox_AB`) for indirect specular. Optional Filament/Frostbite multi-scatter energy compensation (`EnergyCompensation`) preserves bright metal highlights at high roughness without breaking energy conservation.
* **Anisotropic Specular Engine:** Added `_Aniso` and `_AnisoRot` for stretched highlights along a per-material tangent. Uses Burley's anisotropic alpha split with full `D_GGX_Aniso` + `V_SmithJointGGX_Aniso` evaluation; rotates the world tangent around N at the requested degree before splitting alpha into `ax`/`ay`, producing true latex-stretch highlights instead of a fake brushed-metal hack.
* **Thin-Part Transmission:** New `_Trans_Str`, `_Trans_Dist`, and `_Trans_Power` drive a Burley/Filament back-light term with Beer-Lambert absorption (`exp(-(1-diffColor)/_Trans_Dist)`), giving ears, fingers, and latex membranes physically correct light bleed without polluting the SSS lobe.
* **Tinted Dielectric Clearcoat:** Added `_CC_Tint` and `_CC_F0` so the clearcoat reflects with a colored cast and a custom Fresnel base reflectance. White tint at `F0=0.04` reproduces the standard dielectric exactly; coloured tints give the under-layer a complementary cast via per-channel `baseEnergy` attenuation.
* **Geometric Specular Anti-Aliasing:** Added `GeometricSpecAA` - a Toksvig-style filter that roughens both base and clearcoat normals based on screen-space normal derivative variance. Eliminates the spec aliasing/sparkle that hammered high-gloss surfaces at mid-distance.
* **Poiyomi-Pack PBR Compatibility Layer:** Added `_PBR_Met_Ch`, `_PBR_Smooth_Ch`, `_PBR_AO_Ch`, `_PBR_Height_Ch` channel selectors plus `_PBR_Met_Inv` and `_PBR_Smooth_Inv` (Channel Stores Roughness). Drops Poiyomi/Substance/Marmoset-packed masks straight in without re-authoring; the `ChannelPick` helper is wired through every consumer (parallax raymarch, BRDF shadow trace, surface metallic/smoothness, AO, height) so the channel choice is honored everywhere.
* **Multi-Region Color Mask (Poiyomi-Style):** New `_UseRegionMask` + `_RegionMask` with independent R/G/B zone tints (`_Region_R/G/B_Tint`) and per-zone emission boosts (`_Region_R/G/B_Emis`). Channels stack independently, letting authors paint hard-edged feature zones (panels, claws, decals) without a second material.
* **Secondary Emission Layer:** New `_UseEmission2` with its own `_EmissionMap2`, `_EmissionColor2`, mask channel picker (`_Emis2_MaskCh`), and dedicated AudioLink band reactor (`_AL_Band_Emis2`, `_AL_Emis2_Mod`). Authors can route bass to one circuitry layer and treble to another without juggling animator parameters.
* **Multi-Layer MatCap Engine:** Added a full second matcap layer (`_UseMatCap2`, `_MatCap2`, `_MatCap2_Mask`, `_MatCap2_MaskCh`, `_MatCap2_Tint`, `_MatCap2_Int`, `_MatCap2_Rot`) with three blend modes (`Add`, `Replace`, `Multiply`). Layer 1 also gains mask channel selection (`_MatCap_MaskCh`) and per-layer tint so a single RGB region mask can drive different matcaps in different zones.
* **VRChat Mirror Camera Rendering Fix:** Added `vw_CameraPos()` and `vw_WorldViewDir()` helpers that read the actual rendering camera from `UNITY_MATRIX_I_V._m03_m13_m23` (per-eye correct under single-pass instanced) instead of `_WorldSpaceCameraPos` (which stays glued to the player's head in mirrors). Routed through surface viewDir, GI viewDir, parallax raymarch, and clearcoat reflection so specular/parallax/cubemap math is finally correct in mirrors.
* **Light Volumes Deep Integration:** Replaced the single intensity slider with a full LV stack: `_LV_Spec_Mix`, `_LV_Spec_Dominant` (dominant-light vs full L1 specular), `_LV_CC_Spec_Mix` for clearcoat-specific specular, `_LV_Bias` for normal-bias offset (fixes light bleed at sharp edges), `_LV_PosOffset` for manual world-space offset on thin/sleeve geometry, `_LV_AdditiveOnly` for additive-only mode that preserves Unity's probe baseline, and `_LV_ProbeDering` - an opt-in Bakery L1 deringed-probe fallback for non-LV worlds.
* **LTCGI Surgical Mix Controls:** Split LTCGI into independent `_LTCGI_Diff_Mix` and `_LTCGI_Spec_Mix` (alongside the master `_LTCGI_Int`), letting creators dial area-light diffuse and specular contributions separately without disabling LTCGI on the whole material.
* **VRSL Color Hijack:** Added `_VRSL_Color_Hijack` (independent from `_VRSL_Intensity`) that lerps the AudioLink color toward live DMX RGB (sector channels +3/+4/+5). Lets stage colour washes override neon emission palettes without disabling AL color reactivity entirely.
* **Chronotensity Index Selector:** New `_AL_Chrono_Idx` (0-7) lets authors pick which chronotensity row they're tapping, and `_UseChronoFX` makes the entire chrono read opt-in - avoiding 4 extra texture samples per pixel for amplitude-only setups. All chrono-driven effects (vortex breath, fracture re-roll, glitch re-seed, scanline reaction, flicker time) now gate behind this toggle.
* **DFT Note Emission Reactor:** Added `_AL_DFT_Note` (0-11) and `_AL_DFT_Mod` driving `AudioLinkGetAmplitudesAtNote` - the emission layer can now pull on a specific musical note across all octaves (e.g., always glow on a C# or pull a snare hit cleanly out of the mix) instead of using band averages.
* **ColorChord Strip Position:** New `ColorChord Strip` source for `_AL_ColorMode` lets the artist pick any position along the 128-pixel `ALPASS_CCSTRIP` via `_AL_Strip_Pos`, exposing the full continuous ColorChord gradient.
* **Audio Color Blend (Rainbow):** `_AL_Col_Blend` now drives a `HUEtoRGB` rainbow cycle over `_Time.y + bio + worldPos.y` before VRSL hijack, giving a worldspace-anchored chromatic shift that reacts to the live bio pulse.
* **Spherical Autocorrelator Vertex Engine:** New `_Vtx_AutoCorr_Str` reads `AudioLinkGetSphericalMappedAutoCorrelatorValue(normalize(v.vertex.xyz))` per-vertex - drives a smooth volumetric ripple that doesn't require a band selection because every vertex samples its own object-space direction.
* **Spectrum Bar Density Control:** Added `_Cyber_CC_Density` (4-64 bars) - the HUD spectrum is now a true bar chart with adjustable resolution and proper inter-bar gaps, not a continuous strip.
* **Autocorrelator HUD Ring:** New `_UseCyberAuto` with full `Transform` placement and `_Cyber_AutoCorr_Str` intensity. Renders a radial ring driven by `ALPASS_AUTOCORRELATOR` with animated angular spokes (`sin(angle * 12.0 + animTime)`) and a gaussian falloff envelope for the readout band.
* **HUD Hover-Off-Body Engine:** Added `_Cyber_Hover` (parallax-out height) and `_Cyber_Hover_Bob` (subtle vertical drift). The HUD UV is shifted along the tangent-space view direction so the panel reads as a holographic plane floating *above* the avatar surface, with a subtle sine-driven bob for "alive" feel.
* **Standard-Style Render Mode Selector:** New `_Mode` property (Opaque/Cutout/Fade/Transparent) drives blend state, ZWrite, render queue, RenderType tag, VRCFallback tag, and alpha keywords via `SetupMaterialWithBlendMode`. `Fade` uses straight alpha; `Transparent` uses premultiplied alpha so specular survives at low opacity (glass/latex). Cutout retains the historical `clip(c.a - _CutOff)` behavior.
* **Build-Time Variant Stripper (`VixenWearVariantStripper`):** New `IPreprocessShaders` that hammers shader variants in three layers: (1) managed feature keywords still enabled on any live material, (2) Deferred/Meta/MotionVectors passes entirely dropped (avatar clothing never uses them), and (3) dead built-in keywords leaking past the surface pragma (`LIGHTMAP_ON`, `DIRLIGHTMAP_COMBINED`, `DYNAMICLIGHTMAP_ON`, `LIGHTMAP_SHADOW_MIXING`, `SHADOWS_SHADOWMASK`, `LIGHTPROBE_SH`, `LOD_FADE_CROSSFADE`). Accompanied by `VixenWearVariantStripReporter` that logs `kept/stripped/total` post-build.
* **Build & Play-Mode Keyword Sync:** `VixenWearBuildPreprocessor` (`IPreprocessBuildWithReport`) auto-runs `CleanAllMaterials` on every build, and `VixenWearPlayModeSync` (`InitializeOnLoad`) syncs every VixenWear material's keywords on `ExitingEditMode` so stale toggles never no-op on the first play frame.
* **`Edit Materials From Selection` Menu (`Ctrl+Shift+M`):** Promotes Hierarchy GameObject selection to the underlying VixenWear `.mat` assets by walking `GetComponentsInChildren<Renderer>(includeInactive: true)`, deduplicating via `HashSet<Material>`, and swapping `Selection.objects`. Picks up wardrobe layers that are toggled off - critical for VRC clothing where the inspector otherwise collapses to `-` across renderers.
* **`Disable Media-State Gate On All Materials` Menu:** Project-wide bulk fix that clears `_UseMediaState` on every VixenWear material at once, restoring AudioLink reactivity for worlds without a `_MediaPlaying` driver.
* **`Clean Latex Material Keywords` Menu:** Manual trigger for the build-time keyword sync - rebuilds every VixenWear material's keyword state from current property values, useful after bulk-editing via animation clips or VRCFury.
* **Per-Tab Reset to Shader Defaults:** Right-click any inspector tab and pick "Reset {TAB} to Defaults" - spawns a hidden defaults material from the same shader and writes its values back through `MaterialProperty`. Honors `_Mode` by re-running `SetupMaterialWithBlendMode` after the reset on the BASE tab.

### Changed

* **AudioLink Now Always-Compiled (No `AL_ENABLE` Variant):** Ripped out the `AL_ENABLE` shader feature keyword entirely. AudioLink.cginc is now always included and gated at runtime via `_UseAudioLink`, so VRCFury material-toggle animations can flip AudioLink on/off without a build-time variant explosion (VRC materials can't change keywords at runtime). The stripper actively `DisableKeyword("AL_ENABLE")` on every material to clear stale state.
* **Dead `CYBER_ENABLE` Keyword Purged:** Removed `CYBER_ENABLE` from the pragma list and force-disabled on every material - the shader never `#if`-gated on it so it was generating a useless 2x variant set.
* **`VectorLabelDrawer` Per-Component Multi-Edit Fix:** The drawer now writes individual `X`/`Y`/`Z`/`W` components per selected material via `m.SetVector(name, cur)` where `cur[i] = newVal`. Previously `prop.vectorValue = v` broadcast the first material's *entire* vector to every selected material on every edit - silently overwriting independent settings whenever a single component changed.
* **`VectorLabelDrawer` Per-Component Mixed-Value Display:** Each X/Y/Z/W slot independently shows Unity's `-` mixed-value indicator instead of the all-or-nothing `prop.hasMixedValue` behavior. Matches the native `Vector4Field` workflow users expect.
* **`VectorLabelDrawer` Adaptive Label Sizing:** Labels now shrink from font size 11 down to 8 to fit the available slot, fall back to short forms (`X Offset` -> `X`, `Scale` -> `Scl`, `Rotation` -> `Rot`), and ellipsis-truncate as a last resort. Full label is preserved in the tooltip.
* **`VixenALBandDrawer` Change-Gated Writes:** Wrapped the `IntPopup` write in `EditorGUI.BeginChangeCheck` so it no longer unconditionally overwrites every selected material's value on every repaint. `EditorGUI.showMixedValue = p.hasMixedValue` shows the proper `-` indicator for multi-edit.
* **`UseMediaState` Default Flipped to OFF:** New materials now default to AudioLink running regardless of media player state. The opt-in gate is still available for worlds that genuinely want video-driven activation, but the previous opt-out default was silently killing AudioLink for users without a VRC video player in scene.
* **Inspector Tab Reorganization:** `BASE` gains `_Mode`; `SURFACE` gains all `_PBR_*_Ch` selectors; `POLISH` gains `_CC_Tint`, `_CC_F0`, anisotropy, transmission, and `_UseMultiScatter`; `INTEGRATION` absorbs MatCap (with channel/tint), gains layer 2, all region mask + secondary emission props, and the deep Light Volumes + LTCGI stacks; `AUDIOLINK` gains chronotensity, DFT, autocorrelator HUD, hover, strip-position, spectrum density, and vertex autocorrelator; `STAGE` gains `_VRSL_Color_Hijack`.
* **Surface Pragma Hardened:** Added `keepalpha exclude_path:deferred exclude_path:prepass nolightmap nodynlightmap nodirlightmap noshadowmask nometa nolppv` plus a defensive `#pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK`. Massive baseline variant reduction even before the build-time stripper runs.
* **Vertex Fracture Engine Rewritten:** Replaced the old per-primitive geometry-shader scatter with a `disp()`-stage shatter that snaps verts to a 3D grid (`floor(vertex.xyz * 25.0)`), hashes each cell for a per-shard random axis (Rodrigues rotation), pivot offset along the normal, scatter direction, and subtle scale - all gated by `_UseVtxKinetic` and strictly driven by AudioLink band amplitude so silent worlds never shatter the avatar.
* **Per-Pixel Fracture Clip in Surface Stage:** The fragment now `clip(fractureNoise - fractureCut)` against a per-pixel hash, with `fracturePop` driving a tiny parallax offset on shard edges - reads as physical shard separation and survives tessellation cleanly.
* **`PerformPaste` Honors `_Mode`:** Pasting BASE tab settings now re-runs `SetupMaterialWithBlendMode` on the destination materials so the blend state actually matches the pasted `_Mode` instead of inheriting the previous mode's leftover state.
* **Codebase-Wide Multi-Line Comment Purge:** Stripped `/* */` block-comment syntax across the C# editor scripts (`VixenHub`, `VixenWorldSpider`, `VixenEngineStressTest`), every UI Toolkit stylesheet under `Editor/UiStyles/`, the `AnimationWorkbench` resource styles, and the public-site CSS. C# blocks rewritten as `//` line comments; USS/CSS section banners flattened to single-line `/* ... */` (USS has no `//` syntax). Resolves editor-side lag on larger stylesheet files in VS Code / Rider - cosmetic only, zero runtime impact. Third-party vendored shader libraries (LTCGI, AudioLink, LightVolumes) intentionally skipped to preserve upstream sync.

### Fixed

* **Mirror Camera Specular/Parallax Math:** Resolved a long-standing issue where view-dependent math (specular highlights, parallax raymarching, clearcoat cubemap reflection) rendered wrong in VRChat mirrors. Unity's surface-shader plumbing computed viewDir from `_WorldSpaceCameraPos` (the player's head, not the mirror camera) - now reprojected through `vw_WorldViewDir()` from the actual rendering camera matrix.
* **Light Volumes Black-Out on Default Worlds:** Probe SH (especially Bakery's dering path) can produce negative diffuse values when L1 magnitude exceeds L0, causing the avatar to render pitch-black on probes-only worlds. Now clamps `LightVolumeEvaluate` output to `max(..., 0)` to suppress the negative reconstruction artifact.
* **Light Volumes Edge Bleed:** Sharp geometry edges sampled the volume at the wrong location, causing light bleed into shadowed cavities. Added `_LV_Bias` normal-bias offset (matches the official LV PBR reference shader) plus manual `_LV_PosOffset` for thin sleeve/membrane geometry.
* **Multi-Edit Vector Property Corruption:** The `VectorLabelDrawer` was silently broadcasting the *first* selected material's entire vector to every other selected material on every component edit. Per-component, per-material writes via `Undo.RecordObjects` + `m.SetVector` preserve each target's independent state.
* **Multi-Edit Enum Property Spam:** `VixenALBandDrawer` unconditionally overwrote `p.floatValue` on every OnGUI tick, silently resetting other selected materials' band selections back to the first material's value. Now change-gated.
* **Geometry Shader Pass Instability Under Tessellation:** The dual-pass SM5 `GEOMETRY_FRACTURE` Pass 2 from 2.1.5 collapsed under hardware tessellation - the `primID` hashing desynced from the tessellated triangle indices, producing flickering and orphaned shards. Replaced entirely with the per-pixel surface-shader noise clip (which also tessellates correctly).
* **Static-Slider Fracture in Silent Worlds:** Fracture/pump/autocorrelator vertex effects could fire from manual slider values even when AudioLink wasn't running, shattering avatars in silent worlds. All vertex AL effects are now strictly band-amplitude-driven with `_UseAudioLink > 0.5 && _UseVtxKinetic > 0.5` as the master gate.
* **Stale Keyword Build Variant Bloat:** Materials retained `AL_ENABLE`, `CYBER_ENABLE`, and obsolete alpha keywords from prior shader versions, forcing the build system to compile thousands of dead variants. Build preprocessor now scrubs every VixenWear material's keyword set against the current pragma list before compilation; post-build report logs the strip count.
* **Stale `_UseMediaState` Gates Blocking AudioLink:** Materials authored under 2.1.5 had `_UseMediaState` ON by default and silently muted AudioLink in worlds without a VRC video player. New default is OFF; `Disable Media-State Gate On All Materials` provides a project-wide bulk fix for upgraded materials.
* **MatCap UV Collapse on Flat Geometry:** Previous tangent-space matcap math collapsed to `(0,0,1)` on flat polygons, producing a stuck reflection point. Switched to view-space normal via `mul((float3x3)UNITY_MATRIX_V, nWorld)` which is mirror-correct and never collapses.
* **Tab Inspector State Loss on Reset:** `PerformReset` on the BASE tab now re-runs `SetupMaterialWithBlendMode` after writing defaults, so the reset value of `_Mode` actually takes visual effect instead of lagging behind the property.
* **Clipboard Texture Offsets/Scales Lost on Paste:** `PerformPaste` now writes `SetTextureOffset` and `SetTextureScale` per-target inside the paste loop instead of relying on `prop.textureValue` to round-trip the tiling state.
* **Destructive Topology Engine Crash on Dirty Rigs:** `VixenMeshPatcher.CollapseBonesToParent` threw an `ArgumentNullException` when mapping `SkinnedMeshRenderer.bones` into a dictionary if the array contained `null` slots (common from dirty FBX vertex groups or upstream hierarchy flatteners). Dictionary insertions and hierarchy lookups are now strictly null-gated to prevent serialization halts.
* **Aggressive Frustum Culling on Modular Clothing:** The `OPTIMIZE_BOUNDS` heuristic previously calculated tight-fit spatial bounds per-mesh, causing modular clothing to vanish instantly when the tight bounds rotated slightly off-camera. Replaced with a **Universal Avatar-Scale Bounds** matrix (a universally safe 2.5m³ volume anchored to the root bone) to completely neutralize clipping while safely enforcing `updateWhenOffscreen = false` for VRAM optimization.
* **Validator UI Desync & Stale Data:** The Validator window previously went stale if hierarchy or component edits occurred outside the tool. Integrated a debounced event trigger hooking into Unity's `OnHierarchyChange`, `OnProjectChange`, and `OnSelectionChange` to silently queue a deep scan and refresh the UI after a 500ms delay, preventing Editor lockups during rapid manual edits.

### Removed

* **`Pass 2 GEOMETRY_FRACTURE` (SM5 Geometry Shader):** Removed entirely. Hardware tessellation desyncs were unfixable without sacrificing tessellation, and the per-pixel clip+parallax pop in Pass 1 produces an equivalent (and more stable) visual.
* **`AL_ENABLE` Shader Feature:** Now runtime-gated. AudioLink.cginc is always included; `_UseAudioLink` flips it without a keyword variant.
* **`CYBER_ENABLE` Shader Feature:** Dead keyword that the shader never `#if`-gated against. Force-disabled on every material via the keyword stripper.

## [2.1.5] - 2026-05-16

### Added

* **Dual-Pass Geometry Shatter Engine:** Implemented a raw Shader Model 5.0 Geometry Shader (`#pragma geometry`) pass. Bypassing standard Surface Shader limitations, this intercepts clipped polygons, calculates their true centers using `SV_PrimitiveID`, un-welds the topology, and physically blasts literal mesh polygons into 3D space to the beat of AudioLink.
* **God Tier Cybernetics HUD:** Engineered a realtime diagnostic UI system utilizing a B&W window mask. Renders active AudioLink VU meters, 64-pixel spectrum arrays, oscilloscopes, and VRSL DMX grids directly onto the material with independent UV transform and scaling controls.
* **Granular Clipboard Architecture:** Upgraded the native IMGUI inspector with a highly advanced right-click Context Menu system. Features cross-material Copy/Paste mechanics with distinct execution paths for "Values Only" (preserving local maps) or "With Textures" across all defined UI tabs.
* **VRSL Stage Hijack Protocol:** Integrated a direct DMX world-buffer intercept. The material now natively mimics a 13-Channel Moving Head fixture, seamlessly overriding local emission with stage RGB/Strobe data and physically geo-warping the mesh using live DMX Pan and Tilt arrays.
* **Kinetic UV Engine:** Deployed hardware-level UV manipulations including Vortex Twists, Bass Pumping, and spatial-hashed UV Fracture Shards, all mathematically tied to specific AudioLink FFT bands.

### Changed

* **Documentation Overhaul:** Stripped out ambiguous or overly stylistic terminology to ensure clear, universally understood technical instructions for the VixenWear deployment guide.
* **Inspector & ShaderLab Synchronization:** Purged all redundant `[Header]` attributes from the raw `.shader` properties block, completely eliminating the "double-labeling" UI conflict between Unity's native layout passes and the custom C# Editor script.

### Fixed

* **Vertex "Move Tool" Sliding Bug:** Corrected a critical math flaw in the kinetic vertex engine where global linear translation caused the entire avatar mesh to slide across the screen. The pump matrix now correctly displaces strictly along `v.normal` for true volumetric inflation.
* **SM5 Compiler & Keyword Conflicts:** Resolved severe d3d11 compile errors in the Domain/Vertex stages by ripping out legacy `tex2Dlod` macros in favor of native `AudioLinkData` fetches. Additionally, renamed variables in the geometry pipeline to prevent syntax tree failures caused by reserved HLSL keywords (`centroid`).
* **Missing Variable Initialization:** Registered the `_Cyber_AutoCorr_Str` and `_UseCyber` floats into the shader's internal memory allocation, fixing a variable pass failure that previously crashed the HUD Transform Engine.

## [2.1.0] - 2026-05-15

### Added

* **VectorLabelDrawer System:** Engineered a custom MaterialPropertyDrawer natively hooked into ShaderLab's Vector arrays. This handles packed vector data inside the inspector seamlessly, preventing standard 4-float structural UI clutter while preserving exact field alignments.
* **UITK & IMGUI Hybrid Editor:** Deployed a completely overhauled inspector architecture (`VixenWearUITKInspector`). Features a sleek, tab-driven workflow dividing material configuration into Base, Surface & PBR, Polish & Translucency, Integration, and AudioLink.
* **AI Transparency & Development Ethics Documentation:** Published our formal documentation on enterprise-scale AI deployment. This structural breakdown details our 100% hand-reviewed code standards, agentic research integration, and measurable workflow accelerations to ensure transparency across the VixenTools ecosystem.

### Changed

* **Massive Shader Property Packing:** Consolidated loose floats and variables into highly optimized `Vector` attributes (such as `_PBRParams`, `_GeoEmisParams`, and `_ClearcoatParams`). This drastically reduces uniform overhead and streamlines memory bandwidth across the material pipeline.
* **Aggressive Baseline Optimization:** Expensive calculations have been zeroed out by default to guarantee maximum out-of-the-box performance. Parallax and Displacement are now disabled at the baseline, and Iridescence and Rim lighting have also been zeroed out.
* **AudioLink Modulation Standardization:** All AudioLink reactive modulations (`_ALParamsA`, `_ALParamsB`) are entirely zeroed out by default to prevent unintended emission or vertex manipulation unless explicitly configured by the user.

### Fixed

* **Inspector State Loss (Domain Reloads):** Solved an aggressive UX friction point where the material inspector would reset its tab focus during Unity script compilation. The editor now securely caches active tabs and foldout states to `EditorPrefs`, surviving domain reloads without breaking the workflow.

## [2.0.4] - 2026-05-15

### Added

* **Natural MatCap Reflection Model:** Introduced a physically‑inspired matcap blending system using Fresnel weighting, roughness‑driven clarity falloff, and energy‑normalized contribution curves. Matcaps now behave like a stylized reflection lobe rather than an additive light source, producing smoother, more realistic latex highlights.
* **MatCap–Reflection Harmony Layer (Patch 4):** Added a new cross‑fade stage that blends matcaps into the clearcoat reflection based on surface smoothness. High‑polish surfaces now transition seamlessly into probe reflections, eliminating the “double highlight” artifact common in older matcap pipelines.

### Changed

* **MatCap Sampling Pipeline:** Moved matcap intensity and lighting mix out of the BRDF and into the surface stage, reducing redundant per‑pixel math and preventing variant duplication. The BRDF now receives a clean, pre‑normalized matcap signal for consistent lighting behavior.
* **Clearcoat‑Aware MatCap Integration:** Updated the BRDF to evaluate matcaps against the clearcoat normal rather than the base normal, ensuring matcaps follow the same polish layer as reflections and thin‑film interference.
* **Energy‑Conserving Highlight Behavior:** Replaced the legacy additive matcap term with a physically‑bounded contribution that respects clearcoat Fresnel, occlusion, and energy conservation. This prevents matcaps from overpowering direct light or blowing out bright latex surfaces.

### Fixed

* **Harsh MatCap Brightness Spikes:** Resolved the long‑standing issue where matcaps appeared unnaturally bright at grazing angles or under high‑intensity lighting. The new Fresnel‑weighted model eliminates edge blowout and restores smooth highlight rolloff.
* **MatCap vs Reflection Fighting:** Fixed the visual conflict where matcaps and reflection probes produced competing highlight shapes. The new cross‑fade system ensures both layers reinforce each other instead of stacking destructively.
* **Double‑Smoothness Multiplication:** Removed an accidental double‑application of smoothness to matcap intensity, which previously caused inconsistent gloss levels across different materials and lighting conditions.

## [2.0.3] - 2026-05-14

### Added

* **Semantic Ecosystem Layout (Ecosystem Architecture Page):** Wrapped the entire suite overview in a semantic `<main class="main-wrapper">` container with a dedicated `.page-header`, `.comparison-section`, and `.tools-grid` structure. This creates a clean document outline for crawlers while preserving the cyberpunk glass-panel aesthetic.
* **World Engine Omni‑Matrix Card (Expanded Matrix Variant):** Introduced the `expanded-matrix-card` variant of `tool-card` with a dedicated `.matrix-grid` and `.matrix-column` flex layout. The Spider’s dual-column feature matrix is now fully responsive, readable, and structurally isolated for both users and search engines.

### Changed

* **SEO‑Optimized Content Hierarchy:** Normalized heading levels (`h1` page title, `h3` tool names, `h4` matrix section headers) and grouped related copy into clearly scoped sections. This improves snippet extraction, rich result eligibility, and ensures search engines can correctly infer feature groupings (Hub, Optimization Suite, Quest Conversion, PhysBone Mapper, Badge Studio, World Engine).
* **Crawl‑Friendly Tool Cards:** Refactored each tool into a consistent `tool-card` pattern with a single image, a descriptive `h3`, and a tightly scoped feature list. Alt text and card titles now read as human‑legible summaries instead of internal labels, boosting relevance for avatar, Quest, and VRChat‑tooling queries.
* **Preview & Media SEO Pass:** Converted the Quest vs PC comparison into a `.comparison-banner` with a single, descriptive `<img>` and caption block. The preview trigger is now attached to the container instead of inline styles, reducing DOM noise and making the hero comparison image a clear, high‑value media target for search indexing.
* **Matrix Grid Flexbox Refactor:** Replaced the ad‑hoc grid styles with a dedicated `.matrix-grid` flexbox system and responsive `.matrix-column` rules. On narrow viewports, the Spider’s ecosystem and UI/security columns collapse into a vertical stack without breaking readability or keyword density.

### Fixed

* **Broken World Engine Card Markup:** Removed the stray closing `<div>` and normalized the World Engine: Omni‑Matrix Diagnostic Spider card structure. The `tool-card → tool-image-container → tool-content → matrix-grid → matrix-column` hierarchy now validates cleanly and no longer corrupts the surrounding tools grid.
* **Inline Style & Redundant Wrapper Noise:** Stripped unnecessary inline styles and redundant wrapper divs from the Ecosystem Architecture section, reducing DOM bloat that previously diluted keyword signals and harmed Lighthouse/SEO scores.
* **List & Typography Consistency:** Aligned all feature bullets under a single `ul.custom-list` pattern with consistent font sizes and spacing. This prevents fragmented list structures that confused both assistive tech and search parsers when scanning long capability matrices.

## [2.0.2] - 2026-05-13

### Fixed

* **Vixenwear GI Integration (Baked Lighting):** Resolved a critical precision collapse in the baked lighting evaluation matrix across Vixenwear assets. Re-engineered the indirect illumination and spherical harmonic sampling pathways to correctly decode lightmap data. This eliminates localized light bleeding, stabilizes micro-shadow gradients, and guarantees physically accurate spatial integration when Vixenwear is deployed in heavily baked environments.

## [2.0.1] - 2026-05-12

### Added

* **Dynamic Markdown Architecture (Vixen Hub):** Integrated `HOWITWORKS.md` (Metrics Engine) and `SHADERSETUP.md` (Shader Pipeline) directly into the Vixen Hub. Offloads heavy UI styling by utilizing a custom regex Markdown-to-UIElements parser, allowing real-time documentation updates without triggering C# recompiles.
* **Serialization Ghost Detection (World Engine):** Engineered a reflection bridge that cross-references physical `UdonBehaviour` components against their underlying C# `[UdonBehaviourSyncMode]` attributes. Automatically detects and offers 1-click fixes for inspector-level "Continuous Sync" ghosts that waste network IDs on explicitly declared `NoVariableSync` scripts.
* **Context-Aware Network Heuristics:** Upgraded the Udon auditor to cross-reference physics topologies. Unjustified Continuous Sync warnings are now intelligently suppressed if the target object contains physical movement drivers like `Rigidbody` or `VRC_Pickup`.

### Changed

* **Zero-Allocation UASM Parsing:** Refactored Udon compute instruction counting to utilize a memory-safe `StringReader` stream, eliminating massive string array allocations and preventing Garbage Collection (GC) spikes on heavy scripts.
* **O(1) Program Source Memoization:** Replaced brute-force scene traversal in `GetUdonTypeNameSafe` with a static reflection cache and `AbstractUdonProgramSource` dictionary. Instanced prefabs (e.g., hundreds of toggle buttons sharing the same script) now resolve instantly after the first script analysis, drastically cutting O(N) execution time.
* **Strict Active-State Compute Filtering (Dashboard):** Hardened the Heuristics Dashboard to strictly enforce `&& object.enabled && object.gameObject.activeInHierarchy` across all compute matrices (Renderers, Lights, LTCGI, AudioLink, Video Players, Probes). Guarantees the Threat Level exactly mirrors the actively rendering pipeline without being skewed by disabled object pooling or hidden rooms.
* **Isolated Lighting Metrics:** Extracted "Active Scene Lights" into an independent, pre-filtered metric on the Dashboard to provide granular visibility into scene-wide light limits and draw-call multiplication.

### Fixed

* **Cache Poisoning Desyncs (Heuristics Dashboard):** Fixed a critical state-poisoning bug where `_sceneObjectCache` queries for inactive objects would overwrite active queries. Upgraded the dictionary key to a `(Type, bool)` Tuple, guaranteeing perfect mathematical isolation.
* **AppDomain Execution Freezes:** Completely eliminated Editor thread freezing caused by recursive `AppDomain.CurrentDomain.GetAssemblies()` calls during massive Udon script resolutions.
* **Disabled Object False Positives (Spider):** Patched the Matrix Spider to properly evaluate `.enabled` states on components. It now silently ignores explicitly disabled Lights and Reflection Probes, perfectly aligning Matrix warnings with the Dashboard's compute math.

## [2.0.0] - 2026-05-12

### Added

* **4D-Chess Scene Caching & Heuristics Profiler (World Engine):** Engineered a persistent O(1) lookup architecture utilizing `_sceneObjectCache`, `_textureRecoveryCache`, and a dedicated reflection `_fieldCache`. Includes a standalone World Profiler window that calculates real-time granular VRAM footprints and CPU overhead across Textures & Materials, Meshes & Geometry, Terrain & Environment, UI & Canvases, Lightmaps & Global Illumination, Volumetrics (Light Volumes), AudioLink, LTCGI, Video Players, Audio Objects, Udon & Network Persistence, Physics, and the Rinvo Search Ecosystem. Outputs a compute threat score with color-coded severity.
* **Persistent JSON Lookup Cache & I/O Pipeline:** Deployed the new `WorldEngineCache` and `AssetRecord` versioning system. Tracks texture/mesh hashes, last-known resolutions, and failure states to automatically skip previously processed assets. Handled by a background `_workQueue` utilizing per-frame processing and `AssetDatabase.StartAssetEditing()` batching.
* **Omni-Chaos Environment Generator (QA):** Introduced `VixenEngineStressTest.cs`, establishing an extreme edge-case stress test scene designed to push VRAM, polygon limits, and hierarchy depth to their absolute breaking points to rigorously validate pipeline resilience.
* **Proprietary Shader Integration:** Deployed native ecosystem support, validation, and global matrix protection for `Shader "VixenTools/Latex Suit Ultra"`.
* **Proprietary Shader Integration:** Deployed new HLSL Latex `Shader "VixenTools/Latex Suit Ultra"`.
* **Ecosystem Integration (Hub):** Added a dedicated "Supported Modules" tab to the VixenHub. The architecture now actively audits, scans, and intrinsically protects the following third-party infrastructure:
* **ProTV (Techanon):** Comprehensive pipeline auditing, GSV conflict resolution, and AudioLink topology handshakes.
* **AudioLink:** Reflective extraction of internal FFT textures, orphan detection, and global whitelist protection.
* **LTCGI:** Real-time polygonal area lighting matrices, ghost screen eradication, and bake cache deadlock resolution.
* **VizVid (VVMW):** Comprehensive video pipeline analysis, interface decoupling checks, and Quest fallback validation.
* **Video TXL:** CRT render ecosystem validation, GC sink detection, and Playlist Queue access control integration.
* **iwaSync3:** Network sync frequency tuning, blinding emissive bounds detection, and global 2D audio isolation.
* **YouTube Search (Rinvo):** Autonomous video player target linking, UI architectural decoupling, and API pool size validation.
* **VRC Light Volumes:** Compute load detection, sphere threshold optimization, and TVGI/AudioLink strobe safety enforcement.
* **VR Stage Lighting:** Regex-based heuristic protection and DMX audit support.
* **Global Protection Rules & Auto-Population (Shader Dictionaries):** `AutoPopulateTargets()` and `AutoPopulateWhitelist()` now automatically inject VRChat Mobile Toon, Filamented, and Mochie shaders while securing protected assets project-wide.
* **Three-Button Utility Panel & Smart Reset (UI/UX):** Added rapid-access controls for *Populate Targets*, *Populate Whitelist*, and *Factory Reset (Nuke & Rebuild)* to the Shader Dictionary Editor, alongside smart autonomous detection of active dictionary types based on filenames.
* **Advanced Ecosystem Audits & Scanners:** Actively scans Light Volumes (reads atlas/3D textures and custom arrays) and Rinvo Search. Maps ProTV, TXL, IwaSync3, and standard Unity/AVPro players while isolating unique screens across all video systems.
* **Precision Surface Snapping (Scene Tools):** Added an enterprise-grade SceneView raycaster featuring a UV-style neon placement disc. Users can Shift-click to bypass standard rotation alignment, utilize live gravity snapping that respects terrain topology, and leverage automated collider shielding.

### Changed

* **Neon Cyberpunk UI Overhaul (UI/UX):** Rolled out new dropdowns, object fields, shader selectors, and deep magenta, pink, and cyan styling. The resolution dropdown now perfectly syncs with the live runtime value.
* **Lazy-Loaded Diagnostic (UI/UX):** `_expandedCategories` now stores open UI states. Heavy diagnostic foldouts only execute and populate their respective matrices when actively expanded, drastically cutting down on wasted Editor overhead.
* **Shader Dictionary Auto-Repair:** `EnsureDictionariesExist()` upgraded to support force-rebuilding, dynamically auto-populating both target and whitelist dictionaries if missing or corrupted.
* **Bone Collapse Engine:** Intelligently folds terminal vertex weights directly into parent bone structures during topology optimization.

### Fixed

* **TMPro Harvester Crashes:** Wrapped the Omni-Harvester's TextMeshPro material scraper in a fail-safe execution block, resolving thread-killing `NullReferenceExceptions` caused by missing font assets on corrupted UI elements.
* **ImageMagick Crash Loops (VRAM Downscaler):** Eliminated a queue crash condition triggered by ImageMagick execution failures. Added a bypass for active `RenderTexture` objects to prevent decoding halts on unreadable formats.
* **State Desyncs (World Engine):** Fixed shader dropdown state desyncs occurring when dictionary values change. Resolved an issue where category foldouts collapsed unexpectedly after a system refresh.
* **Font Fallback Failures:** Patched TMP/Legacy font fallback logic across the ecosystem.
* **Mesh Structural Corruption (Topology):** Resolved severe triangle collapse and bone index mapping errors that occurred during high-aggression vertex welding. Fixed mathematical miscalculations in the blendshape frame averaging logic, and engineered a dynamic recursive Kinematic Protection Matrix into the microwelder to flawlessly shield delicate avatar hierarchies (e.g., Novabeast facial fluff, ears, and phalanges) from destructive vertex merging.
* **AppDomain Sweep Bottlenecks (Reflection):** The static reflection cache completely eliminates the massive AppDomain assembly sweeps that previously caused multi-second stutters during UI construction and FFT extraction.
* **Quest Shader Whitelist Anomalies (Validator):** Fixed an edge-case logic failure in the Quest conversion pipeline where mobile shader detection failed to accurately parse the `VRC.SDKBase.Validation` whitelist.

## [1.6.1] - 2026-05-05

### Added
- **Factory Reset Protocol (Shader Dictionaries):** Engineered a safe "Soft Nuke" execution button into the `ShaderDictionaryAssetEditor`. Bypassing standard `AssetDatabase.DeleteAsset` calls to prevent GUI null-reference collapses, the protocol clears the internal array and uses smart file-path parsing to autonomously rebuild either the PBR Target schema or the Protected Whitelist schema from scratch.

### Changed
- **Non-Destructive Asset Relocation (Data Safety):** Migrated the default initialization paths for `ShaderDictionaryAsset` files out of the volatile `Packages/` directory. Dictionaries now securely generate within `Assets/VixenTools/Asset Database/World Engine/`. This permanently isolates custom user shader entries from VPM, ensuring they are shielded from being overwritten or wiped during future package version bumps.
- **Diagnostic Execution Safety (World Engine):** Restructured the Engine Diagnostic matrix to default all heuristic fixes to an opt-in state (`IsSelected = false`). This prevents the automated Action protocols from accidentally mass-purging or modifying volatile hierarchy data without explicit user confirmation.

### Fixed
- **Serialization Deadlock & CS0246 (Architecture):** Radically decoupled the `ShaderDictionaryAsset` and its `[CustomEditor]` class from the primary `VixenWorldEngine` window space into distinct scripts. This forces Unity's AssetDatabase to index the custom inspector immediately upon compilation, obliterating the deferred-loading bug and resolving the `CS0246` namespace missing errors.
- **Infinite Replacement Loop (Shader Pipeline):** Injected an $O(1)$ circuit breaker into the Geometry & Materials auditor. The heuristic scanner now explicitly recognizes the active `_targetReplacementShader` as inherently compliant, preventing the engine from falsely flagging—and attempting to recursively convert—materials that have already achieved their optimized target state.

## [1.6.0] - 2026-05-04

### Added
- **Full Website & Documentation Expansion:** Rebuilt the entire documentation portal with a unified neon‑cyber aesthetic, expanded technical breakdowns, and a fully rewritten Core Documentation page covering every subsystem in the VixenTools ecosystem.
- **Tool‑Specific Deep‑Dive Pages:** Authored complete documentation for Quest Conversion Engine, Badge Studio, Animation Workbench Pro, PhysBone Topology Mapper, Preset Manager, Scene Utilities, and the World Engine auditors.
- **Interactive DOM‑Driven Layout System:** Implemented a modular HTML layout grid with glass‑panel containers, badge headers, multi‑column matrices, and stable UIToolkit‑inspired spacing rules.
- **Expanded Workflow Guides:** Added step‑by‑step “In Practice” workflow sections for every major tool, including real‑world usage patterns, optimization strategies, and cross‑tool integration notes.
- **New Visual Identity System:** Introduced standardized glow headers, badge markers, preview cards, and consistent typography across all documentation pages.
- **Changelog Archive Overhaul:** Rebuilt the changelog index with improved readability, version‑indexed navigation, and expanded historical entries.
- **Developer‑Facing Architecture Notes:** Added internal logic explanations for heuristics, math libraries, reflection systems, VRAM analyzers, and topology forensics used across the ecosystem.
- **Mobile‑Optimized Documentation Layout:** Rewrote responsive CSS rules to ensure all panels, matrices, and code blocks render cleanly on mobile and tablet devices.
- **New “Under the Hood” Section:** Added a full technical deep‑dive into DOM bridging, shader injection, VRAM heuristics, and the internal architecture powering VixenTools.
- **Omni-Matrix Diagnostic Spider (World Engine):** Deployed a lethal, 4D-chess-level heuristic auditing matrix explicitly mapped to VRChat's most dominant third-party ecosystems (ProTV, TXL, and IwaSync3). The engine now autonomously hunts down 25+ specific architectural anti-patterns, including GSV texture conflicts, Realtime GI emission blowouts, aggressive polling starvations, and unbounded UI Canvas rebuild cascades.
- **Native Video Pipeline Catchers (World Engine):** Integrated dedicated heuristics for base `VRCAVProVideoPlayer` and `VRCUnityVideoPlayer` components. The engine now detects and offers auto-fixes for "Unlimited (0)" resolution bandwidth nukes and low-latency configurations that destabilize mobile instances.
- **Raw UASM Code Scraper (Udon Persistence):** Bypassed standard Unity component checks by directly intercepting the `UdonSharpEditorCache`. The engine now parses raw Udon Assembly (UASM) instructions to detect `PlayerData.Set` calls trapped inside Update loops—autonomously catching network rate-limit nukes before they can cause cloud data loss.
- **Autonomous Omni-Chaos Generator (QA Protocol):** Upgraded the engine stress tester from a basic script into an isolated environment constructor. It now autonomously quarantines execution into a dedicated `Stress Test.unity` scene, builds the foundational VRChat world architecture (VRCSceneDescriptor, Spawn points, Floor), and uses Reflection to dynamically spawn 8 discrete "Nightmare Pods" (only if target SDK addons are detected) to validate heuristic catch-rates.
- **Precision Click-to-Place Raycaster (Topology Tools):** Engineered a sniper-rifle camera raycaster for the Scene View. Enables creators to visually paint/teleport objects onto complex shelf polygons using click-or-drag mechanics, guided by a real-time, cyber-aesthetic (Cyan/Magenta) UV projection disc that adheres to surface normals.
- **Dynamic Action Grid Polling (Vixen Hub):** Engineered real-time state polling for Scene View tools. The dashboard now autonomously reads `EditorPrefs` to inject rich-text hex formatting (`<color=#00e5ff>[ ACTIVE ]</color>`) directly into Hub buttons, providing instant visual feedback on tool toggles.

### Changed
- **Hardware-Level VRAM Footprint Extraction:** Decoupled the texture memory auditor from the Unity `AssetDatabase`. The Spider now captures the `Base Texture` class directly from the GPU registers via `Profiler.GetRuntimeMemorySizeLong()`, successfully unmasking procedural, unmanaged, and 64MB 4K RenderTexture "VRAM Nukes" that hide from standard project scanners.
- **Aggressive Compute Threat Matrix:** Re-calibrated the engine's compute scoring algorithm to strictly penalize mobile frame-killers. Real-time Shadow Casters are now assigned a massive 80.0x threat weight, ensuring that dynamic lighting in unbaked scenes instantly flags as a `SEVERE` compute hazard.
- **Reactive Scene State Routing (Vixen Hub):** Re-engineered the World Pipeline Tools dashboard integration. Executing a tool from the Hub now triggers a synchronous UI layout flush (`SwitchMode`), forcing the UI Toolkit visual tree to rebuild and reflect badge states instantly without requiring a domain reload.
- **VRChat-Optimized Snapping Matrix (Topology Tools):** Hardcoded a custom bitwise layer mask `(~((1 << 2) | (1 << 4) | (1 << 5) | (1 << 9) | (1 << 10) | (1 << 12) | (1 << 13)))` into the gravity snapping engine. The raycaster now explicitly ignores VRChat-specific utility layers (Ignore Raycast, Water, UI, Player, PlayerLocal, UiMenu, Pickup), ensuring props snap exclusively to physical environmental geometry.
- **Asset Protection Whitelist Expansion:** Injected critical community dependencies into the global protection matrix. The engine now rigidly ignores shaders from `VRC Billiards`, `VRChat SDK Samples`, and `IwaSync3 Internal` to prevent the autonomous PBR replacer from breaking logic-driven materials.

### Fixed
- **Full Website Comb Over:** Fixed multiple outstanding issues with missing documentation, buttons, etc.
- **Missing “Jump to Top” Button (Documentation):** Restored the global scroll-to-top control across all documentation pages after a regression removed it from several layouts.
- **Discord Bot Embed Truncation:** Patched the VixenGitWatch telemetry bot to prevent aggressive truncation of release notes and commit bodies. Updated the embed engine to use safer limits and improved formatting for long-form content.
- **Biometric Matrix Purge (Quest Conversion):** Eradicated compilation blockages caused by unsupported PC-VR Face Tracking parameters. The Conversion Engine now deploys a targeted heuristic sweep to aggressively strip out localized OSC/Blendshape-driven eye and jaw tracking components before initiating the Android build pipeline.
- **VRCFury Face-Tracking "Hunter-Killer" Expansion:** Hardened the hierarchy purge logic to completely eradicate specialized face-tracking branches including `VRCFury - Face Tracking Prefabs`, `VRCFury - Face Tracking - Ears`, and `VF_UE_VRCFT` internal nodes.
- **Prefab API Deadlock Correction:** Patched a critical compilation error where `PrefabUtility.GetPrefabAssetPathOfNearestInstance` was unrecognized in Unity 2022.3. The logic now utilizes `AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource())` to safely identify external template dependencies.
- **ASTC Texture Pipeline Desync (Quest Conversion):** Patched a catastrophic compression loop where Unity's internal texture importer mismatched sRGB/Linear color spaces on certain custom textures. The engine now rigidly forces correct format serialization before applying `ASTC_6x6` crunching, preventing corrupted normal maps and inverted color outputs.
- **Component Dependency Deadlocks (QA Protocol):** Resolved an architectural flaw where the Omni-Chaos generator triggered `NullReferenceExceptions` when attempting to spawn ProTV or IwaSync3 screens. The deployment pipeline now strictly honors `[RequireComponent(typeof(Renderer))]` tags, establishing mesh foundations prior to script injection.
- **Legacy Font Thread Death (World Engine):** Removed all hardcoded references to the deprecated `Arial.ttf` system asset. The UI Void diagnostic and the stress tester now safely interface with the Unity 2022+ compliant `LegacyRuntime.ttf`, preventing hard crashes during font-swap operations.
- **Self-Occlusion Snapping Deadlocks (Topology Tools):** Patched a severe raycasting failure in the Precision Snapping tool where the target object would collide with its own geometry, freezing it mid-air. Implemented a "Surgical Shielding" loop that caches and disables all child colliders on the selected transform *before* the raycast, and cleanly restores them post-execution.
- **Disjointed Pivot Alignment (Topology Tools):** Re-engineered the `CalculateFeetOffset` logic to dynamically iterate through all child colliders or mesh bounds. This ensures objects with offset pivots or complex prefab hierarchies snap flush to the floor rather than floating or sinking into the floor polygons.

## [1.5.1] - 2026-05-01

### Added
- **Autonomous Domain Polling (Vixen Hub):** Engineered a static `[InitializeOnLoad]` routine that silently parses the `package.json` manifest during Unity domain reloads. It now dynamically cross-references the active ecosystem against local `EditorPrefs` to autonomously trigger the Scene View update badge without requiring manual intervention.
- **Direct Tab Routing (UI Ecosystem):** Injected synchronous tab-routing execution into the `VixenHub` class. Clicking the Scene View update notification now bypasses the default dashboard and instantly maps the UI matrix directly to the Release Changelogs.

### Changed
- **Nuclear Decimation Downgrade (Topology Engine):** Re-engineered the Multi-Pass Welder from an aggressive 3D volumetric crusher into an *Extreme Precision 5D Microwelder*. Enforced a strict 5mm (`0.005f`) structural hard cap to safely seal sub-millimeter import seams without collapsing limb volumes or flattening continuous meshes into 2D singularities.
- **Kinematic Shield Bypassing (Avatar Validator):** Strategically punctured the `Neck`/`Head` protective bone matrix. The heuristic scanner now explicitly isolates and exposes dense, non-structural children (`Hair`, `Ears`, `Fluff`) to the optimization grid, allowing the decimator to successfully hit Quest poly limits without being bottlenecked by shielded geometry.

### Fixed
- **UV Seam Tearing (Topology Engine):** Eradicated catastrophic texture stretching during decimation sweeps. The upgraded 5D Spatial Hash `(X, Y, Z, U, V)` now strictly locks UV coordinates, guaranteeing that vertices sharing atomic space but possessing divergent texture mappings are never violently fused.
- **RenderTexture Magick Crashes (VRAM Optimizer):** Patched a critical failure loop where the `ProcessTexturesWithMagick` pipeline attempted to intercept and decode volatile, active `RenderTexture` assets, fully resolving the recurring `error/constitute.c/ReadImage/753` console spam during a Deep Matrix Scan.
- **SDK Version Regex Blindspot (Vixen Hub):** Expanded the ecosystem's JSON parser to actively detect modern `com.vrchat.base` and `com.vrchat.worlds` dependencies, fixing a logic gap where the dashboard header falsely reported the active SDK version as "Unknown".

## [1.5.0] - 2026-04-30

### Architectural Overhaul & Dual-SDK Ecosystem Integration
- **Context-Aware Dual-SDK Architecture (Vixen Hub):** The core VixenToolBox ecosystem has been completely re-engineered to natively bridge and map both the VRChat Avatar SDK (`VRC_SDK_VRCSDK3`) and the VRChat World SDK (`UDON`). The central Hub now operates as a dynamic gateway, autonomously reading the active compiler directives upon project initialization and seamlessly hot-swapping the "Flagship Tools" UI matrix to match the current development environment.
- **Airtight Compiler Sandboxing:** Implemented strict, cryptographic-level compiler isolation across the entire asset suite to guarantee zero assembly bleed-over. World-exclusive utilities like `SnapToSurface.cs` are now mathematically locked behind `#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON` directives, while avatar-centric suites (`VixenBadgeMaker`, `QuestConversionEngine`, `AvatarSDKValidator`) operate strictly within `#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON` boundaries. This ensures Unity's compiler strips incompatible scripts entirely, preventing catastrophic `VRCAvatarDescriptor` or `UdonBehaviour` reference bricks when migrating the toolbox across radically different VRChat project infrastructures.
- **Dynamic Assembly Resolution:** The internal reflection engine now safely queries for third-party and cross-SDK assemblies without establishing hard dependencies. By utilizing `comp.GetType().Name.Contains("VRCFury")` during serialization sweeps, the package exists seamlessly in vanilla or heavily modded Unity environments without throwing missing assembly references.

### Destructive Topology & Spatial Welding Engines
- **5D Spatial Hash Welder (Optimization Suite):** Engineered a highly destructive, multi-threaded vertex decimation engine designed specifically for massive PC avatars (`VixenMeshPatcher.WeldVertices`). The welder violently fuses co-located vertices on excessively heavy meshes (>15,000 polygons) by generating a proprietary 5-dimensional coordinate key (`"{hashX}:{hashY}:{hashZ}:{uvHashX}:{uvHashY}"`). By running both `x / threshold` and `u / uvThreshold` calculations, it drastically crushes raw polycounts while mathematically guaranteeing the survival of critical texture seams.
- **Dual-Shielding Exclusion Matrix:** The Welder is no longer blind to high-fidelity topology. It now features an autonomous Surgical Exclusion Matrix that deploys two layers of defense during decimation:
  - **Material Keyword Mapping:** Automatically scans all submesh material slots for high-detail keywords (`"eye"`, `"visor"`, `"lens"`, `"blush"`, `"face"`, `"mouth"`, `"teeth"`, `"pupil"`, `"iris"`) and assigns them a locked `"PROTECTED_{i}"` spatial key.
  - **Kinematic Bone-Weight Shielding:** Actively queries the active Humanoid Armature for `HumanBodyBones.Neck` and `HumanBodyBones.Head` transforms, recursively mapping their children to mathematically lock any vertex influenced by these bones entirely out of the decimation grid.
- **BlendShape Memory Recovery:** Executing a destructive `mesh.Clear()` inherently annihilates Unity's blendshape delta arrays. The Spatial Welder now performs a pre-execution memory extraction via a `BlendShapeExtract` struct, caching all 175+ frames (`DeltaVerts`, `DeltaNormals`, `DeltaTangents`). Post-decimation, it mathematically averages the delta offsets across the newly fused vertices via `mergeCounts`, allowing highly complex facial animations to perfectly survive aggressive topology meltdowns.
- **Submesh Array Preservation:** The decimation pipeline now strictly extracts and reconstructs triangle arrays on a per-submesh basis using `mesh.GetTriangles(s)`. This prevents the "Atlas Trap" where decimated avatars would violently collapse into a single material slot, guaranteeing your multi-material setups remain perfectly intact.

### Dynamic Matrix & Performance Heuristics
- **Dynamic PC Target Rank Matrix:** Eradicated static, hardcoded PC culling limits. The PC Optimization Suite now features a dynamic `<fluent-select>` Enum dropdown mapping to `AvatarSDKValidator.PCPerformanceRank`. Creators can explicitly target specific VRChat Performance Ranks (Excellent, Good, Medium, Poor). The deep-scanning heuristics instantly recalibrate their execution thresholds (e.g., Excellent strictly enforces a maximum of 4 PhysBones, 4 Contacts, and 1 Animator, while Poor expands the cap to 32).
- **Interactive Physics Executioner:** Replaced blunt, invisible auto-cull algorithms with a surgical UI Toolkit panel. The PC Validator now spawns a dedicated interactive matrix populating a `List<PhysicsNode>`. Sorted mathematically by hierarchy depth (`GetDepth`), it empowers creators to manually select and eradicate expendable leaf-node physics via `Undo.DestroyObjectImmediate` while visually protecting structural physics roots.
- **Deep Material Inspector Spider (VRAM Profiling):** Upgraded the VRAM calculation engine to pierce deeply through the Unity Inspector. It now weaponizes `EditorUtility.CollectDependencies` data streams to spider through highly obfuscated components, specifically targeting `anim.runtimeAnimatorController` and VRCFury `MonoBehaviour` outfit toggles. It aggressively hunts down and caches nested `Texture` dependencies, ensuring the ImageMagick downsampling pipeline captures 100% of the avatar's actual memory footprint.
- **Base-Class Physics Targeting:** Upgraded the deep matrix scanners from strict `<VRCPhysBone>` wrappers to their foundational `<VRCPhysBoneBase>` and `<VRCPhysBoneColliderBase>` classes. The engine now successfully captures 100% of internal physics variations, including VRCFury auto-conversions, script-generated endpoints, and legacy dynamic bone migrations.

### UI/UX & Cyber-Noir Standardization
- **Universal Cyber-Noir Topology (UI Ecosystem):** Extended the native UI Toolkit `.uss` architecture universally across the entire VixenTools ecosystem. Every utility—from the `AnimationWorkbenchWindow` to the `QuestConversionEngine`—now perfectly inherits the signature dark-panel aesthetics, neon-cyan borders, and hyper-responsive interactive hover states (`.cyber-panel`, `.cyan-btn`, `.danger-btn`, `.data-tag-destructive`) via centralized stylesheet injections.
- **Native Class Hijacking:** Overhauled the core `.uss` stylesheets to natively target and hijack Unity's internal UI classes, specifically `.unity-enum-field`, `.unity-popup-field`, and `.unity-slider-int`. Optimization sliders now render as sleek, flat cyberpunk tracks with glowing cyan draggers (`.unity-slider-int .unity-base-slider__dragger`), while dropdowns feature tinted glass backgrounds and neon borders.
- **Macro Topology Overrides:** Injected global "Keep All", "Cull All", "Select All", and "Deselect All" macro execution buttons into the interactive UI panels of both the Quest Engine and PC Validator via UI Toolkit button callbacks. This establishes a highly streamlined triage workflow when parsing massive arrays of `TopologyNode` physics components or `TextureNode` assets.
- **Instant UI State Refresh (Vixen Hub):** Re-engineered the action delegate for the "Snap To Surface" world tool. It now asynchronously reads live `EditorPrefs.GetBool` states and forces an immediate UIElements layout refresh, allowing the button text to instantly transition between active and inactive states without requiring a manual window reload.

### Critical Bug Fixes & Matrix Stabilizations
- **Kinematic Bounding Box Detonation:** Repaired a massive mathematical flaw where extracting standard bounds from meshes with offset origins threw the bounding box entirely out of the camera's frustum. The engine now initializes a `new Bounds(rootCenter, Vector3.zero)`, extracts all 8 `corners` of the `sharedMesh.bounds` into absolute World Space via `TransformPoint`, and projects them backward into root-bone Local Space via `InverseTransformPoint` for a mathematically perfect, tight-fit `Encapsulate` pass.
- **The Atlas Trap (Decimation Bleed):** Resolved a critical oversight where detailed facial geometry utilizing the same universal `bodyMat` as the torso was being incorrectly targeted and melted down by the Welder. Successfully mitigated via the new Kinematic Bone-Weight Shielding protocol scanning `HumanBodyBones.Neck`.
- **Texture Shattering (UV Seam Rips):** Fixed the destructive spatial hash blindly gluing UV seams together based solely on 3D proximity, which previously turned decimated textures into shattered, stretched geometry.
- **IgnoreTransform Shield Piercing:** The Topology Erasure matrix now actively parses every `ignoreTransforms` array inside `VRCPhysBoneBase` components. Utilizing `.IsChildOf(ig)`, if a bone is explicitly listed on that array, the `HasPhysBoneProtection` heuristic shield is forcefully dropped, allowing the Culler to properly identify and melt dead leaf bones that were previously evading execution.
- **Autonomous Scene HUD Notifier:** Completely rebuilt the update notification badge from the ground up to support Unity 2022.3. Stripped out legacy `Handles.BeginGUI()` IMGUI math and injected a native UI Toolkit `Button` directly into the `sceneView.rootVisualElement` to perfectly respect high-DPI scaling, absolute coordinate anchoring, and scene depth-sorting.
- **CSS BorderRadius Traps:** Replaced the incompatible `borderRadius` UI Toolkit C# shorthand with explicit, pedantic corner definitions (`borderTopLeftRadius`, `borderBottomRightRadius`, etc.) to permanently resolve `CS0117` compilation errors while maintaining the sleek, rounded aesthetic on the interactive scroll views.
- **Compiler Scope Breakage:** Resolved a critical `CS1022` namespace definition error caused by a rogue bracket escaping the `#endif` directive in the Hub's master script, restoring flawless domain reloads and script compilation.

## [1.4.5] - 2026-04-28

### Added
- **Dynamic Version Extraction (Vixen Hub):** Engineered a lightweight Regex parser that autonomously reads the ecosystem's `package.json` during initialization to extract and display the active VixenTools and VRCSDK versions in the header, completely eliminating hardcoded strings and race conditions.
- **Changelog Pagination Engine (Vixen Hub):** Completely overhauled the changelog markdown parser. Replaced infinite vertical scrolling with a structured, memory-efficient data dictionary and a native `DropdownField` to dynamically isolate and render individual release histories.
- **Physics Joint Heuristics (Quest Engine):** Expanded the deep matrix scan to actively hunt and isolate unsupported Unity physics constraints (`SpringJoint`, `FixedJoint`, `HingeJoint`).
- **Dedicated Joint UI Topology (Quest Engine):** Physics joints now populate in a dedicated phase 1.5 Interactive Topology matrix section, equipped with mathematically locked (limit 0), neon-red danger toggles to guarantee they are auto-culled during the conversion pipeline.
- **Native Style Hijacking (UI Ecosystem):** Injected base Unity class overrides (e.g., `.unity-base-popup-field__input`) into the core `.uss` stylesheets. Standard Unity fields now automatically inherit the VixenTools cyber-noir dark panels and cyan-tinted borders without requiring clunky inline C# modifications.

### Changed
- **Destruction Pipeline Routing (Quest Engine):** Re-routed the `ProcessDestruction` sequence so that physics joints are securely stripped from the prefab sandbox before unpack, preventing VRChat SDK mobile validation errors.

### Fixed
- **UI Toolkit Flex Boundaries (Vixen Hub):** Repaired a critical layout quirk where the markdown `ScrollView` lacked strict flex limits, causing the viewport to bleed off the bottom of the Editor window. Enforced `flex-shrink: 1` and applied aerodynamic padding buffers to restore full scroll depth.
- **Regex Verbatim Escaping (Vixen Hub):** Resolved `CS1026` and `CS8997` compile errors caused by double-escaped quotes during the manifest parsing sequence.

## [1.4.4] - 2026-04-27

### Added
- **[Website] Support Ecosystem Hub:** Created a dedicated support page featuring interactive, glass-morphic tip-jar cards to transparently fund development without paywalling features.
- **[Website] Network & Media Matrix:** Deployed a new social routing hub featuring a mathematically enforced 16:9 responsive YouTube iframe for the Master Class video, accompanied by a 6-card creator network grid.
- **[Website] Changelog Pagination Engine:** Built a JavaScript-driven pagination system for the changelog. Replaced infinite scrolling with isolated version views, seamless CSS fade animations, and "Newer/Older" step routing.
- **[Website] Version Jump Dropdown:** Integrated a native `<fluent-select>` dropdown to instantly route users to specific historical releases.
- **[Website] Scroll Tracking UX:** Engineered a dynamic "Jump to Top" button that monitors viewport depth and gracefully fades into the UI to prevent endless scrolling on lengthy matrices.
- **[Website] Interactive Lightbox Engine:** Engineered a native, zero-dependency lightbox system allowing users to dynamically expand UI Toolkit previews into high-resolution cinematic viewports on the Architecture matrix.
- **[Website] SaaS-Grade Competitive Matrix:** Completely eradicated the legacy HTML data table on the comparison page. Replaced it with a responsive, flex-wrapping feature grid using high-contrast neon styling to directly pit VixenTools against fragmented industry standards.
- **[Website] Visual Architecture Showcase:** Overhauled the Suite Architecture page from a text-heavy bulleted list into a high-fidelity image grid showcasing the actual custom Editor inspectors and utilities.

### Changed
- **[Website] Shadow DOM Styling:** Pierced the Fluent UI `<fluent-select>` Shadow DOM using `::part(listbox)` to force native dropdowns to respect the dark cyber-noir aesthetic and enforce scrollbar constraints.
- **[Website] Registry Template Sanitization:** Stripped deprecated modal structures from the core `index.html` VPM registry and secured the Scriban templating engine (e.g., `{{~ for package in packages ~}}`) for safe backend hydration during CI/CD deployment.
- **[Website] Enterprise Navigation Matrix:** Scrapped the horizontal top-row navigation. Implemented a persistent, fixed-position vertical sidebar to create a true desktop application feel, utilizing full-width block interaction states and cyan active-border tracking.
- **[Website] Cinematic Image Cropping:** Deployed `object-fit: cover` logic to the Quest vs PC Comparison banner to safely lock its height and prevent it from infinitely scaling and hogging the viewport.

### Fixed
- **[Tool] SDK Dependency Resolution:** Bumped ecosystem manifest to `v1.4.4` and strictly enforced VRChat SDK requirements (`com.vrchat.base` and `com.vrchat.avatars` mapped to `^3.10.3`). This resolves critical missing assembly references required by the newly integrated Live Scene UV Mapper's raycasting components.

## [1.4.3] - 2026-04-26

### Added
- **Core Modules Matrix (Vixen Hub):** Added a dedicated "Core Modules" tab serving as a centralized launcher for all flagship utilities within the VixenTools ecosystem.
- **Action Delegate Routing (Vixen Hub):** Upgraded the Hub's grid rendering engine (`RenderActionGrid`) to accept raw C# `System.Action` delegates. This allows the exact same CSS `.link-card` architecture to seamlessly route both web URLs and native `EditorApplication.ExecuteMenuItem` commands without duplicating code.
- **Dynamic Markdown Link Injection (Vixen Hub):** Upgraded the internal Markdown parser to intercept `[text](url)` syntax mid-paragraph. It dynamically spawns transparent UI Toolkit `Button` elements, stripping their native borders and backgrounds to blend perfectly into the standard `Label` text flow while executing `Application.OpenURL`.
- **Full Mobile Performance Heuristics (Quest Engine):** The scanner now mathematically calculates 100% of VRChat's Android Performance limits. Added native topology parsing for Triangles, Skinned Meshes, Material Slots, `VRCRaycast`, `ParticleSystem`, `TrailRenderer`, and `LineRenderer`.
- **Incompatible Mobile Component Purge:** The Quest Engine now actively hunts down objects globally banned on VRChat mobile (Cameras, Lights, AudioSources, Cloth, Rigidbodies, and standard Unity Colliders). These populate in a new "Auto-Culled" UI matrix featuring locked, neon-red danger toggles to guarantee their removal.
- **Root Animator Protection:** Developed a recursive depth scanner that detects the avatar's core root Animator. This component is now hard-locked into a "Kept" state (visualized with a protected neon-cyan toggle) to ensure heuristic culling never accidentally shatters the avatar's base functionality.
- **High-Fidelity Linear Downsampling Pipeline:** Upgraded the ImageMagick Lanczos resize method to perform gamma-correct scaling. Textures are temporarily moved into `ColorSpace.RGB` (Linear) during the downsample, preventing the mathematical darkening/crushing of highlights typical in standard sRGB scaling.
- **Micro-Contrast Texture Recovery:** Injected a mathematically calculated `UnsharpMask(0.0, 0.5, 1.0, 0.05)` into the ImageMagick pipeline post-resize to recover high-frequency material details (fur, fabric weaves) naturally destroyed by downsampling.
- **Interactive Texture Culling Matrix:** Added a dedicated DOM matrix displaying all parsed textures and their resolutions prior to conversion. Creators can now selectively disable ImageMagick processing on a per-texture basis to massively reduce compile times on highly redundant avatars.
- **Mochie Shader Support:** The Quest Engine scanner now inherently recognizes custom third-party property routing, specifically mapping `_MochieMetallicMaps` and `_MochieMetallicMap` down to the VRChat Mobile Standard `_MetallicMap` slot.
- **Emissive Mask Color Control (Badge Studio):** Injected a new `ColorField` to give creators absolute control over the generation of the `_EMI` emission mask. This allows for multi-channel emission blending without blowing out VRChat's bloom. Saved natively to the `layout.json` matrix.
- **Material Parameter Injection (Badge Studio):** The engine now physically reaches into the compiled material to forcefully write user-selected colors (Material Base Color, Emissive Mask Color) directly into the `_Color` / `_BaseColor` and `_EmissionColor` shader properties, overriding legacy states.
- **Targeted Emission Routing (Badge Studio):** Decoupled the text rendering pipelines, adding explicit UI toggles to independently choose whether the Display Name or the Pronouns get injected into the emissive mask.

### Changed
- **Hub Parser Execution (Vixen Hub):** The Hub's dashboard now intelligently terminates its Markdown parsing loop upon detecting external routing headers (e.g., "[ Network Links ]") to prevent rendering redundant data that is already handled by dedicated tabs.
- **Topology Matrix CSS Partitioning:** Separated the massive Quest Engine topology readout into distinct, collapsible DOM foldouts (PhysBones, Colliders, Contacts, Constraints, etc.) to prevent UI bloat on highly complex avatars. 
- **Texture Importer Asset Enforcement:** The Quest Engine no longer blindly imports all processed files as standard sRGB maps. The pipeline now detects if a texture is a Normal Map or a Metallic/Gloss data map, directly injecting the `TextureImporterType.NormalMap` and `sRGBTexture = false` flags to prevent washed-out shading on the Quest clone.
- **ColorField USS Integration:** Updated the core `.uss` stylesheets to natively target `.unity-color-field`, ensuring newly spawned color pickers inherit the custom cyber-noir text scaling, panel backgrounds, and border alignments.

### Fixed
- **UI Toolkit Flex-Box Clipping (Vixen Hub):** Repaired a critical math error where nested `100%` width assignments inside `FlexDirection.Row` containers forced text to blow past the window bounds and trigger a horizontal scrollbar. Enforced strict `flex-shrink: 1`, `flex-grow: 1`, and `whiteSpace = WhiteSpace.Normal` states directly via C# to guarantee clean line wrapping around `>>` chevron bullets.
- **Alpha-Channel Emission Blowout (Badge Studio):** Fixed a critical bug where ImageMagick's transparent text backgrounds were saving as `(255,255,255,0)` (Transparent White). Because Unity's Standard shader emission strictly reads RGB and ignores Alpha, this caused the entire badge to glow blindingly white and override the diffuse map. The engine now mathematically flattens the final `_EMI` composite onto a pure `MagickColors.Black` background and permanently strips the Alpha channel to guarantee zero-bleed emission.
- **Temporary MeshCollider Memory Leak (Badge Studio):** Built a garbage-collection tracker (`_tempCollider`) for the Live Scene UV Mapper. The tool now safely executes `DestroyImmediate` on the auto-generated MeshCollider if the user disables mapping, swaps targets, or closes the window.
- **Emissive Mask Darkening (Badge Studio):** Fixed a compositing math error where the text plate on the generated `_EMI` map was adopting the user's Neon Color, which artificially darkened the shader's target `_EmissionStrength`. Text on the mask now strictly utilizes the new Emissive Mask Color (defaulting to pure white).
- **Quest Engine Compiler Warnings:** Purged a dangling `_rendererCount` variable assignment and eliminated a structurally impossible type-check against `VRCPhysBoneCollider` inside the incompatible components loop. 
- **Legacy JSON Fallback:** Fortified the Badge Studio `layout.json` parser to gracefully handle older template configs. If an older JSON file lacks the newly added `emiMaskColor` parameter, the system safely falls back to `Color.white` instead of defaulting to a transparent/black mask.

## [1.4.0] - 2026-04-24
### Added
- **Enterprise UI Toolkit Architecture:** Completely eradicated legacy IMGUI constraints across the entire ecosystem. Vixen Hub, Badge Studio, Animation Workbench Pro, Quest Conversion Engine, PhysBone Topology Mapper, and Preset Manager now operate natively on Unity's high-performance UI Toolkit DOM tree.
- **Centralized Styling Matrix:** Established a master `UiStyles` repository (`Packages/.../Editor/UiStyles/`) to house the `Cyberpunk-Regular.ttf` font and all `.uss` CSS design tokens. This guarantees absolute visual consistency across all windows and provides a single-point maintenance hub for future UI scaling.
- **Dynamic Flex Layouts:** Engineered highly responsive, flex-wrapping grid architectures for data-heavy tools (Animation Workbench, Preset Manager). The interfaces now mathematically scale and stack panels gracefully upon window resize, completely eliminating clipped text and overlapping bounds.
- **Dynamic DOM Bridge (Quest Engine & Badge Studio):** Replaced the volatile `OnGUI` redraw loops with persistent UI Toolkit state injectors. Interfaces now seamlessly sync variable states and spawn interactive matrices instantly without triggering Editor-wide lag spikes.
- **Dynamic JSON Layout Engine (Badge Studio):** Replaced hardcoded badge layout coordinates with a persistent `layout.json` system. The tool now automatically saves and loads advanced UV bounds, text rotations, and neon hex overrides directly into custom template directories.
- **Furality Layout Generator:** Built a dedicated developer utility natively integrated into Badge Studio to autonomously scaffold directories and format perfect `layout.json` boundary configurations for all Furality convention templates.
- **Autonomous Scene View HUD Notifier:** Eradicated the workflow-breaking update popup window. The engine now silently detects version discrepancies and injects a sleek, non-blocking, neon-accented `>> VIXENTOOLS UPDATE` button directly into the corner of the active Unity Scene View.
- **Live Scene UV Mapper (Badge Studio):** Integrated a 3D coordinate mapper that temporarily suspends Unity's default gizmos (`Tools.current = Tool.None`) to hijack Scene View clicks. It raycasts against the badge mesh, mathematically inverts UVs to ImageMagick pixel space, and draws a neon pink debug indicator at the exact intersection point.

### Changed
- **Magick.NET Font Engine Upgrade (Badge Studio):** Ripped out unstable, Windows-only `Gdi32.dll` OS-level font installation hacks. Migrated to modern Magick.NET direct file pathing (`@font.ttf`), ensuring cross-platform stability and significantly faster text-plate rendering.
- **Cyber-Noir Aesthetic Standardization:** Unified the visual language across the entire VixenTools suite. Deployed tinted glass `.cyber-panel` wrappers, high-contrast neon headers, and massive, deeply saturated action buttons (`.cyan-btn`, `.pink-btn`) to clearly delineate execution phases.
- **Markdown Engine Sanitization:** Upgraded the Hub's internal Markdown parser to natively generate `VisualElements`. Actively stripped all volatile unicode emojis—which inherently conflict with Unity's TextCore rendering engine—and replaced them with stable, terminal-compliant chevron syntax (`>>`, `::`, `>`).
- **Graceful Degradation States:** Overhauled error-handling UIs (such as missing VRChat SDK warnings) to utilize the new enterprise styling, deploying amber `.warning-box-styled` elements to keep the interface looking premium even in failure states.
- **Skinned Mesh Raycasting (UV Mapper):** The mapping engine now automatically detects and bakes `SkinnedMeshRenderer` data into a static `MeshCollider`, allowing accurate raycasting on dynamically weighted Furality attendee badges.

### Fixed
- **Ecosystem Scanner Bleed-Over:** Fixed a directory routing bug where the VixenTools source network was blindly indexing nested Furality template assets. Injected strict exclusion filters so Furality assets correctly populate via the dedicated SDK parser instead of crowding the custom template dropdown.
- **UI Toolkit Constraint Clipping:** Addressed a layout violation where nested flex-containers would clip their content. Mathematically adjusted root `min-width` parameters across the matrix and injected `white-space: normal;` into info-box CSS to ensure text descriptions properly wrap and stack horizontally.
- **CSS Prefix Validation:** Sanitized the newly migrated `.uss` files to strictly adhere to Unity's TextCore engine, replacing standard CSS tags with required `-unity-` prefixes to eliminate console UI warnings.
- **Raycast Piercing (UV Mapper):** Upgraded the mapper to use `Physics.RaycastAll()` with `MouseDrag` support. The tool now successfully pierces through invisible avatar capsule colliders that were previously eating `MouseDown` events and blocking the raycast.

## [1.3.1] - 2026-04-23
### Added
- **Autonomous Update Prompter:** The Vixen Hub now features an `[InitializeOnLoadMethod]` that passively reads the VPM `package.json` in the background during Unity compilation. If it detects a version discrepancy against the local `VersionIs.asset`, it automatically opens the Hub to immediately display the latest release notes.
- **Extended Hub Architecture:** Expanded the Vixen Hub into a 5-tab control matrix (Documentation, Changelog, Donation, Social Media, Get Support). Added an internal Markdown parser for the new `SUPPORT.md` file and integrated stylized action buttons for external ecosystem routing.
- **VixenGitListener (CI/CD Telemetry):** Engineered a fully isolated, asynchronous Discord webhook bot (`aiohttp`). It utilizes an outbound Server-Sent Events (SSE) stream via Smee to intercept GitHub payloads natively, bypassing the need for port-forwarding and keeping the host IP 100% invisible.
- **Deep Payload Parsing:** The Git listener now calculates exact file architecture changes (added, modified, removed) per commit and renders them into the Discord channel using cyber-terminal branch styling. It also intercepts and blockquotes commit and issue comments directly into the matrix.
- **Heuristic AutoMod Matrix:** Deployed a battle-tested Rust regex engine to the community Discord. It is aggressively tuned to intercept and nuke VRChat developer-targeted RATs (disguised as `.rar`/`.zip` game tests), typosquatted domains, and fake Steam login phishing attempts.

### Changed
- **Triage Routing:** The Hub's "Get Support" tab has been overhauled to inject a direct invite to the VixenTools community Discord, establishing a streamlined funnel for live pipeline troubleshooting and architectural upgrades.

### Fixed
- **Gateway Race Condition (Git Listener):** Injected an `await self.wait_until_ready()` execution lock into the webhook background task. This prevents the Smee SSE stream from prematurely intercepting GitHub payloads before the bot has successfully cached the Discord server's channel architecture.

## [1.3.0] - 2026-04-21
### Added
- **Quest Conversion Engine:** Engineered a fully non-destructive, enterprise-grade pipeline for converting PC avatars to Android/Quest. The engine generates an isolated prefab sandbox, mathematically clones materials into a unique ecosystem, and maps dependencies without ever mutating the master PC hierarchy.
- **ImageMagick Lanczos Pipeline:** Intercepted Unity's native texture importer. The engine now routes PC textures through a high-fidelity Magick.NET downsampling pass before forcefully applying Android ASTC compression. This guarantees "Glossy Girl" neon aesthetics and high-contrast maps remain crisp even when crushed to meet VRChat's strict VRAM limits (10MB/18MB/40MB).
- **Heuristic PhysBone Culling:** Integrated a topology scanner that mathematically calculates skeletal depth. When an avatar exceeds the targeted Mobile Performance Rank (e.g., 8 PhysBones for Poor), the engine heuristically culls deep leaf bones while preserving vital root-level physics.
- **Interactive Topology Matrix:** Added a massive, scrollable pre-execution control panel. The matrix parses the entire skeletal hierarchy, automatically applies heuristic culling limits, and allows the creator to manually override which physics components survive the Quest conversion.
- **High-Fidelity Material Translation:** The conversion engine now defaults to `VRChat/Mobile/Toon Standard` and actively hunts for PC-side metallic, gloss, normal, and emission properties across third-party shaders (Poiyomi, lilToon), natively mapping them into the mobile shader to preserve maximum depth.

### Changed
- **Ecosystem Architecture Re-route:** Completely refactored the global `MenuItem` domain structure. Scripts are now strictly isolated into `VixenTools/Avatars`, `VixenTools/Unity Engine`, and `VixenTools/Scene` to establish a highly scalable foundation for future toolsets.
- **Live Surface Snapping (Engine Upgrade):** Overhauled the legacy update loop. Replaced brittle GUI `Event.current` dragging with Unity's native, low-overhead `Transform.hasChanged` flag. Also integrated a dedicated `Ctrl+Alt+S` hotkey for discrete, one-shot surface dropping.
- **Matrix UI/UX:** Transformed the Interactive Topology Matrix from a cramped text dump into a highly readable control panel. Parent hierarchy paths are now dimmed, while target leaf bones are highlighted in high-contrast neon green (`#00ff66`). 

### Fixed
- **Pivot-Clipping (Surface Snap):** Objects no longer incorrectly snap by their center pivot. A new recursive algorithm dynamically calculates true geometric bounds across all child colliders and renderers, ensuring meshes always sit perfectly flush on their "feet."
- **Self-Collision Loops (Surface Snap):** Injected an execution micro-state that temporarily pushes the target object to the `IgnoreRaycast` layer during raycasting, completely preventing meshes from snapping to their own internal colliders.
- **HelpBox Rich Text Stripping:** Bypassed Unity's default EditorGUI stripping by constructing a custom HelpBox layout wrapper, restoring full HTML/Rich Text formatting to the Quest Engine's results dashboard.

## [1.2.0] - 2026-04-20
### Added
- **Vixen Hub Integration:** Merged the standalone documentation and changelog parsers into a unified, tabbed Editor dashboard. Features a custom UV-framing engine for dynamic banner scaling and persistent ecosystem routing.
- **Ecosystem Discovery Engine:** The Badge Studio now passively scans the project hierarchy to detect installed Furality SDKs (Luma, Somna, Sylva, Umbra). Automatically indexes available convention tiers and dynamically routes output paths.
- **Universal Shader Targeting:** Built a heuristic material engine that natively detects and configures target shaders. Automatically routes diffuse/emission maps and activates required toggle parameters across Poiyomi, lilToon, VRChat Mobile, and all legacy Furality shaders.
- **Dynamic UV Auto-Layout:** The generator now intercepts convention selections and automatically adjusts localized text bounds, rotations, and signature neon hex colors to perfectly match the specific year's mesh layout.

### Changed
- **Template Authoring Pipeline:** Upgraded the procedural base generator to support raw Asset Ingestion. The tool can now swallow empty convention `.jpg` files, transcode them, and perfectly adapt its internal compilation resolution to match the source file.

### Fixed
- **Furality SDK Plurality & Obfuscation:** Engineered a recursive deep-scan discovery protocol to bypass Furality's erratic year-to-year folder restructuring. The engine now aggressively hunts down target assets regardless of missing subfolders or `Texture`/`Textures` plurality variations.
- **Magick.NET Dimension Casting:** Resolved implicit `uint` to `int` conversion deadlocks during dynamic resolution matching.
- **Emission Blackout Safeguard:** Added a failsafe to detect and override black or transparent `_EmissionColor` multipliers on legacy convention materials, preventing generated emission masks from multiplying to zero and rendering invisibly.

## [1.1.1] - 2026-04-18
### Added
- **Destructive Pipeline Operations:** Integrated a "Delete Selected" function within the Animation Workbench bindings matrix. This allows for the safe pruning of dead or unused property tracks, preventing asset bloat and VRChat SDK validation errors.

### Changed
- **UI/UX Ecosystem Integration:** Overhauled the Animation Workbench's UI Toolkit construction to natively inject the signature VixenTools "cyber-noir" aesthetic. The tool now features the persistent branded header and color-coded action buttons without requiring core stylesheet rewrites.
- **Explicit Intent Paradigm:** Flipped the default states for "Sample Start Value" and "Sample End Value" to `false`. The pipeline now safely defaults to manual override values (`1` and `0`) when generating curves, preventing the tool from destructively hijacking live scene data unless explicitly commanded.

### Fixed
- **Multidimensional Property Extraction:** Resolved a critical native API crash when attempting to bind Vector/Color properties (e.g., `_AudioLinkEmission0CenterOut`) on locked Poiyomi shaders. The discovery engine now safely decomposes these properties into their individually animatable float channels (`.r`, `.x`, etc.) and routes them through a robust `TryGetMaterialFloat` fail-safe protocol.

## [1.1.0] - 2026-04-10
### Added
- **Animation Workbench Pro:** Officially migrated into the VixenTools package ecosystem. Features advanced curve operations, easing dropdowns, timeline ribbons, and a real-time preview engine.
- **Runtime Math Library:** Established the `Runtime` assembly by migrating `EasingFunctions.cs` to handle pure mathematical logic (Linear, SmoothStep, Cubic, etc.) safely in-game.
- **Dynamic Hub Engine:** Upgraded the VixenTools Hub to autonomously parse and render the package `README.md` using a custom Regex-driven Markdown-to-IMGUI pipeline.
- **Ecosystem Routing:** Integrated a persistent, stylized navigation bar into the Hub for direct access to the GitHub repository, YouTube video guides, and X (Twitter) announcements.

### Changed
- **UI/UX Standardization:** Overhauled the Editor interfaces for `PhysBoneTopologyMapper` and the `Pipeline Preset Manager` to perfectly mirror the VixenTools ecosystem aesthetic. Implemented custom IMGUI headers, neon-tinted rich text, and standardized action buttons.
- **Compiler Safeguards:** Wrapped all Animation Workbench Editor scripts (`AnimationWorkbenchWindow`, `CurveGraphView`, `PreviewEngine`, etc.) in strict `#if UNITY_EDITOR` directives to prevent runtime build crashes.
- **UI Toolkit Pathing:** Updated stylesheet loading paths to be strictly VPM-compliant, ensuring `.uss` files resolve correctly from the `Packages/` directory instead of the local `Assets/` folder.
- **Markdown Sanitization:** The in-editor Markdown parser now actively strips VPM/GitHub web badges to prevent IMGUI rendering errors while converting standard hyperlinks into clean, stylized rich text.

## [1.0.3] - 2026-04-10
### Fixed
- **VPM Dependency Resolver:** Replaced strict greater-than inequality (`>=`) with caret operators (`^`) in `package.json` to prevent the VRChat Creator Companion from aggressively fetching unstable beta SDKs.
- **Listing Hydration:** Repaired the VCC listing frontend by migrating Scriban JSON data generation out of the client-side JavaScript and into a protected DOM bridge, preventing IDE auto-formatters from shattering the build pipeline.

## [1.0.2] - 2026-04-10
### Fixed
- **Compiler Bleed-Over:** Injected a strictly configured Assembly Definition (`.asmdef`) into the `Editor` directory. This hard-locks compilation boundaries, preventing the `PhysBoneTopologyMapper` from crashing Unity when users attempt to build their avatars for runtime.

## [1.0.1] - 2026-04-10
### Added
- **CI/CD Pipeline:** Established fully automated GitHub Actions workflows for generating `.zip` releases, `.unitypackage` fallbacks, and VPM-compliant registry manifests.
- **VCC Storefront:** Deployed a glassmorphic, cyberpunk-styled VPM package listing using Fluent UI web components and FAST Design tokens.

## [1.0.0] - 2026-04-09
### Added
- **PhysBoneTopologyMapper:** Initial release of the flagship extraction and injection engine.
- **Master Blueprints:** Added functionality to utilize `AnimationUtility.CalculateTransformPath` and Unity `.preset` files to bypass native prefab constraints and map complex topologies (e.g., Novabeast Master Topology) seamlessly across compatible avatar roots.