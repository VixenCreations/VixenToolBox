# Vixens Toolbox

A free set of Unity editor tools for VRChat avatars and worlds. It checks your work against VRChat's limits, fixes what it can in one click, and takes the repetitive setup off your hands.

* **Package:** `com.vixencreations.vixens-toolbox` (v2.18.1)
* **Needs:** Unity 2022.3.22f1 and VRChat SDK 3.10.3 or newer
* **Docs and install:** [vixencreations.github.io/VixenToolBox](https://vixencreations.github.io/VixenToolBox/)

Everything lives under the **VixenTools** menu. The **Hub** (`VixenTools/Hub Dashboard`) opens every tool your project can use: avatar projects get the avatar tools, world projects get the world tools, and both get the Unity tools.

### 1. Vixen World Engine

`VixenTools/Scene/Vixen World Engine`. Press **SCAN SCENE** to run more than 160 checks on your world, tick the problems you want fixed, then press **FIX SELECTED ISSUES**.

* **Supported packages:** ProTV, AudioLink, LTCGI, VizVid, Video TXL (with TXL Player Audio, Portal and Misc), iwaSync3, YouTube Search (Rinvo), VRC Light Volumes, VR Stage Lighting and GPU Particle Volumes.
* **VRChat's own video players:** unlimited resolution, low latency mode and speakers that are not assigned.
* **Udon:** sync settings, scripts that do a lot every frame, and player data saved every frame, read from your compiled scripts.
* **Textures:** checked the way the VixForge Texture Check does it: Max Size over your target, normal maps not on BC5, mipmaps off, the wrong colour space and Read/Write left on. Fixes change import settings only, never your image files. **Convert To PNG** saves PNG copies of PSD, JPG and big TIF, TGA or BMP files to `Assets/VixenTools/Converted/` and points your materials at them.
* **Physics, lighting and post processing:** colliders players walk straight through, baked lighting that will not hold, Light Probe and Light Volume coverage, and post processing VRChat does not accept.
* **Performance Map:** a second tab in the same window that scores how heavy your world is and estimates its memory. The Hub's **Metrics Engine** tab explains the score.

### 2. Optimization Suite

`VixenTools/Avatars/Optimization Suite`. Checks your avatar against VRChat's limits, then shrinks meshes and textures to fit.

* **Mesh decimation** with QEM, the same kind of algorithm as Blender's Decimate. It keeps UV seams, material edges and open borders intact, and carries UVs, colours, bone weights and blendshapes through.
* **Protected parts:** materials with eye, visor, lens, blush, face, mouth, teeth, pupil or iris in their name are never decimated, and on a Humanoid rig neither are the hands.
* **Textures** are resized on every CPU core, and the materials your VRCFury toggles and animations swap in are included.
* **VRAM estimate** for everything the avatar uses.

### 3. Quest Conversion Engine

`VixenTools/Avatars/Quest Conversion Engine`. Makes a Quest copy of your avatar and leaves the original alone.

* Removes PC face tracking (adjerry91's VRCFT templates and VRCFury's face tracking prefabs), which Quest cannot use.
* Lists PhysBones, colliders, contacts, constraints, particles and other components against the Quest limits for the rank you are aiming for.
* Shrinks textures with a Lanczos resize and a light sharpen, then sets them to ASTC 6x6.

### 4. Avatar Tools

* **PhysBone Blueprints** (`VixenTools/Avatars/PhysBone Blueprints`): **Save Blueprint** records every PhysBone setup on an avatar, and **Apply Blueprint** puts them back on another one. Handy after a Quest conversion or an optimization pass.
* **Material Conflict Finder** (`VixenTools/Avatars/Material Conflict Finder`): finds materials whose toggles disagree, shader keywords left on after their toggle was switched off, and toggles your animations or VRCFury fight over. **Sync All to 0.0 (OFF)**, **Sync All to 1.0 (ON)** and **Fix All Keywords** settle them.
* **Animator Forge** (`VixenTools/Avatars/Animator Forge`): **RUN DIAGNOSTICS** finds undefined and missing parameters, mixed Write Defaults, empty layers, overfull menus and more. **FORGE RIG** builds toggles, sliders, swaps and exclusive groups for you.
* **Accessory Mounting Engine** (`VixenTools/Avatars/Accessory Engine`): clones a clean armature and mounts accessories onto it, either by re-rigging them or with parent constraints. Press **MOUNT ACCESSORIES**.
* **Badge Studio** (`VixenTools/Avatars/Badge Studio`): builds VRChat convention badges, Furality included, for Poiyomi, lilToon and VRChat Mobile shaders. **Author Master Template** sets up a template, **Compile High-Fidelity Badge** builds the badge, and **ACTIVATE SCENE MAPPING** lets you place text on a curved badge right in the Scene view.

### 5. Scene Tools

* **Quest World Converter** (`VixenTools/Scene/Quest World Converter`): **Scan Open Scenes**, then **Convert Selected Materials** writes Quest copies of your world's materials and leaves the originals alone. **Point the scene at the new materials** swaps them in when you are ready.
* **Live Surface Snapping** (`VixenTools/Scene/Live Surface Snapping`): drops selected objects onto the floor or shelf below them as you move them.
* **Precision Click-to-Place** (`VixenTools/Scene/Precision Click-to-Place`): click in the Scene view to move the selection onto that surface. It ignores VRChat's player, pickup, UI and water layers.
* **Omni-Chaos Generator** (`VixenTools/QA/Generate Omni-Chaos Environment`): builds a stress-test scene full of deliberate problems, so you can watch the World Engine catch them.

### 6. Unity Tools

* **Scene View Enhancer** (`VixenTools/Unity Engine/Scene View Enhancer`): caps the editor's frame rate while it is idle, through Unity's own Interaction Mode setting, and puts your previous setting back when you turn it off. Its **Vixen Scene Stats** panel in the Scene view shows how often the view redraws and how much memory scripts, Unity and the graphics driver use. Works in world and avatar projects.
* **Memory Diagnostics** (`VixenTools/Unity Engine/Memory Diagnostics`): take a snapshot, repeat the task you want to test, then compare, and see which textures, materials and meshes a tool made and never freed. Only things that are not saved in your project are counted, so loaded assets never show up as leaks. **Largest Textures** lists what uses the most memory.
* **Animation Workbench Pro** (`VixenTools/Unity Engine/Animation Workbench Pro`): a visual workspace for building and easing animation curves, with a live preview on your scene object.
* **Pipeline Preset Manager** (`VixenTools/Unity Engine/Pipeline Preset Manager`): pulls import settings out of your assets, or creates a preset from scratch, and can make it the default for everything you import next.
* **Fix Scene Data** (`VixenTools/Unity Engine/Fix Scene Data`): reattaches a scene's lighting data when it has gone missing.

### 7. The Hub

`VixenTools/Hub Dashboard` has News, Overview, Core Modules, Supported Modules, Network, Support and Changelogs tabs, plus Metrics Engine in world projects. When a new version is out, a badge in the Scene view takes you straight to its changelog.

### 8. VixenWear Latex Ultra

Our dual-lobe PBR shader for synthetic materials is a **standalone product** and does not ship inside this package. The Optimization Suite still recognises `VixenWear/Latex Ultra` materials, so avatars wearing it keep their packed-map checks.

> Every option in every inspector tab is documented control by control, with screenshots, in the [Shader Docs](https://vixencreations.github.io/VixenToolBox/shaderdocs.html).

### 9. Third-Party Components

* **Magick.NET `14.17.1`** (ImageMagick `7.1.2-31`), by Dirk Lemstra, under the Apache-2.0 licence. It does all of the toolbox's image work: texture resizing, the World Engine's PNG copies, badge compositing and the VRAM passes. It ships as an Editor-only Windows x64 plugin, so nothing of it reaches your avatar or world.

### 10. Thanks

Thank you to everyone whose ideas and bug reports shaped the toolbox:

* **Lt_Shadow:** suggested the font replacer.
* **TheCastle:** told me about the LTCGI issues behind our LTCGI fixes.
* **ValenVRC:** caught the Udon network checks being too vague, which led to only counting components that are switched on.
* **KittehKun:** suggested simpler naming and wording.
* **DJ Red_Panda:** suggested LOD fixes and plenty of smaller improvements.
* **RBN's World Creators:** suggested how the VRAM estimates should work.
* **flickfluff:** caught the Quest conversion leaving face tracking behind, which led to it being removed automatically.
