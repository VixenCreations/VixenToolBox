# Vixens Toolbox: Details

The [README](README.md) is the short version. This page goes through each tool in depth, then covers how the toolbox is built and released.

Everything lives under the **VixenTools** menu in Unity.

* [The Hub](#the-hub)
* [Worlds and scenes](#worlds-and-scenes)
* [Avatars](#avatars)
* [Unity editor](#unity-editor)
* [VixenWear Latex Ultra](#vixenwear-latex-ultra)
* [How it's built](#how-its-built)
* [Development](#development)

---

## The Hub

`VixenTools/Hub Dashboard`

* **Tabs:** News, Overview, Core Modules, Supported Modules, Network, Support and Changelogs, plus Metrics Engine in world projects.
* **Avatar or world:** it checks which VRChat SDK your project has (`VRC_SDK_VRCSDK3` for avatars, `UDON` for worlds) and shows the matching tools.
* **Version and changelog:** it reads the version from the package's own `package.json`, and splits `CHANGELOG.md` into one page per version with a picker.
* **Docs inside Unity:** the guides, the changelog and `NEWS.md` are rendered right in the window.
* **Update badge:** when a new version is out, a badge in the Scene view takes you straight to its changelog.

---

## Worlds and scenes

### Vixen World Engine

`VixenTools/Scene/Vixen World Engine`. Press **SCAN SCENE** to run more than 160 checks on your world, tick the problems you want fixed, then press **FIX SELECTED ISSUES**.

* **Supported packages:** ProTV, AudioLink, LTCGI, VizVid, Video TXL (with TXL Player Audio, Portal and Misc), iwaSync3, YouTube Search (Rinvo), VRC Light Volumes, VR Stage Lighting and GPU Particle Volumes.
* **VRChat's own video players:** unlimited resolution, low latency mode and speakers that are not assigned.
* **Udon:** sync settings, scripts that do a lot every frame, and player data saved every frame. It reads your compiled UdonSharp scripts, so it sees what actually runs.
* **Textures:** Max Size over your target, normal maps not on BC5, mipmaps off, the wrong colour space and Read/Write left on. Fixes change import settings only, never your image files. **Convert To PNG** saves PNG copies of PSD, JPG and big TIF, TGA or BMP files to `Assets/VixenTools/Converted/` and points your materials at them.
* **Physics, lighting and post processing:** colliders players walk straight through, baked lighting that will not hold, Light Probe and Light Volume coverage, and post processing VRChat does not accept.
* **Performance Map:** a second tab that rates your world as OPTIMAL, MODERATE or SEVERE and estimates its memory, using Unity's own `Profiler.GetRuntimeMemorySizeLong`. The Hub's **Metrics Engine** tab explains the score.

### Quest World Converter

`VixenTools/Scene/Quest World Converter`. Press **Scan Open Scenes**, then **Convert Selected Materials** to write Quest copies of your world's materials. The originals are left alone. **Point the scene at the new materials** swaps them in when you're ready.

### Live Surface Snapping

`VixenTools/Scene/Live Surface Snapping`. Drops the selected objects onto the floor or shelf below them as you move them.

### Precision Click-to-Place

`VixenTools/Scene/Precision Click-to-Place`. Click in the Scene view to move the selection onto that surface. It ignores VRChat's player, pickup, UI and water layers.

### Omni-Chaos Generator

`VixenTools/QA/Generate Omni-Chaos Environment`. Builds a `Stress Test.unity` scene full of deliberately broken objects: physics, UI, VRAM, persistence, and the third-party packages the World Engine checks. Scan it to watch the World Engine catch them.

---

## Avatars

### Optimization Suite

`VixenTools/Avatars/Optimization Suite`. Checks your avatar against VRChat's limits, then shrinks meshes and textures to fit. It changes the avatar in place.

* **Mesh decimation** with QEM (Garland-Heckbert edge collapse, the same kind of algorithm as Blender's Decimate). The **Triangle Target (per mesh)** slider, from 2,000 to 70,000, sets how far each heavy mesh goes. It avoids flipped faces and sliver triangles, keeps UV seams, material edges and open borders intact, and carries UVs, colours, bone weights and blendshapes through.
* **Protected parts:** materials with eye, visor, lens, blush, face, mouth, teeth, pupil or iris in their name are never decimated, and on a Humanoid rig neither are the hands.
* **Textures** are resized on every CPU core, including the materials your animator controllers and VRCFury toggles swap in.
* **VRAM estimate** for every texture the avatar uses.
* **Auto-Fit Bounds** fits each mesh's bounds to its posed shape, so it no longer vanishes when you get close.

### Quest Conversion Engine

`VixenTools/Avatars/Quest Conversion Engine`. Makes a Quest copy of your avatar as its own prefab and leaves the original alone.

* **Face tracking:** removes PC face tracking (adjerry91's VRCFT templates, VF_UE_VRCFT and VRCFury's face tracking prefabs), which Quest cannot use.
* **Components:** lists PhysBones, colliders, contacts, constraints, particles and other components against the Quest limits for the rank you're aiming for.
* **Textures:** shrinks them with a Lanczos resize and a light sharpen, then sets them to ASTC 6x6.
* **Material swaps in animations:** it copies the controllers and clips that swap materials, including your avatar's custom layers, and points them at the Quest materials, so your toggles keep working.

### Animator Forge

`VixenTools/Avatars/Animator Forge`. **RUN DIAGNOSTICS** finds undefined and missing parameters, mixed Write Defaults, empty layers, overfull menus and more. **FORGE RIG** builds toggles, sliders, swaps and exclusive groups for you.

### Material Conflict Finder

`VixenTools/Avatars/Material Conflict Finder`. Finds materials that have drifted out of sync with each other.

* Toggles that hold different values across materials. **Sync All to 0.0 (OFF)**, **Sync All to 1.0 (ON)** and **Align Majority** settle them.
* Shader keywords left on after their toggle was switched off. **Fix All Keywords** clears them.
* Toggles your animations or VRCFury fight over, read from the animator layers and VRCFury clips.
* Locked materials are read through their original keywords, so a locked avatar doesn't report false conflicts.

### PhysBone Blueprints

`VixenTools/Avatars/PhysBone Blueprints`. **Save Blueprint** records every PhysBone on an avatar by its bone path, as Unity Presets in one asset. **Apply Blueprint** finds the same bones on another avatar, adds any missing PhysBones and applies the saved settings. Handy after a Quest conversion or an optimization pass.

### Accessory Mounting Engine

`VixenTools/Avatars/Accessory Engine`. Mounts accessories onto an armature. Press **MOUNT ACCESSORIES**.

* **Full Generation** clones a fresh, empty armature from the source and mounts the accessories onto it. **Append To Existing** adds them to the armature already on your target.
* Skinned accessories are re-rigged onto the new bones with their blendshapes kept. Rigid props are held in place with a Parent Constraint.
* The whole mount is one undo step.

### Badge Studio

`VixenTools/Avatars/Badge Studio`. Builds VRChat convention badges, Furality included, for Poiyomi, lilToon and VRChat Mobile shaders.

* **Author Master Template** sets up a template, and **Compile High-Fidelity Badge** builds the badge.
* **ACTIVATE SCENE MAPPING** lets you click a curved badge in the Scene view to find the spot under your cursor on the badge texture, and copies it to the clipboard.

---

## Unity editor

### Scene View Enhancer

`VixenTools/Unity Engine/Scene View Enhancer`. Caps the editor's frame rate while it's idle, through Unity's own Interaction Mode setting, and puts your previous setting back when you turn it off. Its **Vixen Scene Stats** panel in the Scene view shows how often the view redraws and how much memory scripts, Unity and the graphics driver use. Works in world and avatar projects.

### Memory Diagnostics

`VixenTools/Unity Engine/Memory Diagnostics`. Take a snapshot, repeat the task you want to test, then compare, and see which textures, materials and meshes a tool made and never freed. Only things that are not saved in your project are counted, so loaded assets never show up as leaks. **Largest Textures** lists what uses the most memory.

### Animation Workbench Pro

`VixenTools/Unity Engine/Animation Workbench Pro`. A curve graph for building animations. It finds the shader properties on your renderers so you can animate them, has a library of easing curves, and plays the clip live on your scene object while you edit.

### Pipeline Preset Manager

`VixenTools/Unity Engine/Pipeline Preset Manager`. Two ways to make a preset: **Authoring** builds one from scratch, and **Extraction** pulls the settings out of assets you already have. Either kind can be made the default for everything you import next, with a filter to limit which files it applies to.

### Fix Scene Data

`VixenTools/Unity Engine/Fix Scene Data`. Reattaches a scene's lighting data when it has gone missing.

---

## VixenWear Latex Ultra

Our dual-lobe PBR shader for synthetic materials is a **standalone product** and does not ship inside this package. The Optimization Suite still recognises `VixenWear/Latex Ultra` materials, so avatars wearing it keep their packed-map checks. Every option is documented in the [Shader Docs](https://vixencreations.github.io/VixenToolBox/shaderdocs.html).

---

## How it's built

* **Editor only.** All of the toolbox's code is in one Editor assembly, `com.vixen.tools.Editor.asmdef`, so none of it ends up in an avatar or world upload.
* **Image work** is done by Magick.NET 14.17.2 (ImageMagick 7.1.2-32), an Editor-only plugin for Windows and Linux x64. Its licences are in [Third Party Notices](Packages/com.vixen.tools/Third%20Party%20Notices.md).
* **Releases** (`.github/workflows/release.yml`, run by hand): zips the package folder, builds a `.unitypackage` from it, and publishes both, with `package.json`, as a GitHub Release.
* **VPM listing and website** (`.github/workflows/build-listing.yml`): runs after every release, builds the VPM listing that VCC reads, and publishes it with the `Website/` folder to GitHub Pages.
* **Download badges** (`.github/workflows/badge-downloads.yml`): once a day, adds up the `.zip` and `.unitypackage` downloads across every release, and writes that count and the latest version to the `badge-data` branch. The README badges and the website's counters read from there.

---

## Development

* **Language:** C#, Unity editor scripting
* **Target:** Unity 2022.3.22f1 and VRChat SDK 3.10.3
* **Package source:** `Packages/com.vixen.tools/`
* **Website source:** `Website/`

Open an [issue](https://github.com/VixenCreations/VixenToolBox/issues) for mesh edge cases, world check problems or feature requests.
