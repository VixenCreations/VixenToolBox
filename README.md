[![VPM-Ready](https://img.shields.io/badge/VPM-Compatible-00e5ff?style=for-the-badge&logo=vrchat)](https://vixencreations.github.io/VixenToolBox/)
[![Unity 2022.3](https://img.shields.io/badge/Unity-2022.3.22f1-lightgrey?style=for-the-badge&logo=unity)](https://unity.com/)
[![VRChat SDK](https://img.shields.io/badge/VRChat%20SDK-3.10.3-ff0055?style=for-the-badge&logo=vrchat)](https://vrchat.com/)
[![Version](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FVixenCreations%2FVixenToolBox%2Fbadge-data%2Fversion.json&style=for-the-badge)](https://github.com/VixenCreations/VixenToolBox/releases/latest)
[![Downloads](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FVixenCreations%2FVixenToolBox%2Fbadge-data%2Fdownloads.json&style=for-the-badge)](https://github.com/VixenCreations/VixenToolBox/releases)
[![CodeQL](https://github.com/VixenCreations/VixenToolBox/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/VixenCreations/VixenToolBox/actions/workflows/github-code-scanning/codeql)

---

# Vixens Toolbox

**The central distribution hub and automated CI/CD pipeline for the Vixens Toolbox ecosystem, by VixForge Interactive.**

The Vixens Toolbox is a VPM-native, cyber-noir suite of Unity Editor utilities, engineered to eliminate human error, enforce strict project consistency, and push Unity to its technical limits. Its flagship tools are the **Quest Conversion Engine**, the **Avatar Optimization Suite**, the **Vixen World Engine**, and **Animation Workbench Pro**.

This repository acts as the **first-class VPM package source**, powering the entire ecosystem with automated updates, documentation, and infrastructure.

* **Package:** `com.vixencreations.vixens-toolbox` (v2.10.1)
* **Target:** Unity 2022.3.22f1 / VRChat SDK 3.10.3
* **Storefront & Docs:** [vixencreations.github.io/VixenToolBox](https://vixencreations.github.io/VixenToolBox/)
* **Community:** Trusted by 1,500+ creators and counting.

---

## The Toolset

### 1. Vixen Hub - Core Infrastructure

The command center of the ecosystem: a fully UI Toolkit-driven dashboard that launches every tool with consistent styling.

* **Dynamic Version Extraction:** Reads `package.json` directly to display live package + SDK versions without hardcoded strings.
* **Changelog Pagination Engine:** Parses `CHANGELOG.md` into isolated, version-indexed entries with a native dropdown selector.
* **Autonomous Scene HUD Notifier:** Injects a neon cyber badge into the Scene View when a new version is detected, routing directly to the Hub's changelog tab.
* **Context-Aware Dual-SDK Bridge:** Reads active compiler directives and hot-swaps the UI to match the Avatar SDK (`VRC_SDK_VRCSDK3`) or World SDK (`UDON`) environment.
* **Reactive Scene State Routing:** Tools launched from the Hub instantly reflect their UI state via synchronous layout flushing - no domain reloads required.
* **In-Engine Documentation:** A Markdown-to-UIElements parser renders deep-dive architecture and heuristics guides natively inside Unity.
* **News Tab & Partnerships:** The Hub now opens on a front-and-centre News tab that renders a package-local `NEWS.md`, currently headlining the **Enigma Industries x VixForge Interactive** partnership (the next chapter of Club Enigma), with dynamic buttons that jump to the full write-up, Enigma's Discord, and Enigma's VRChat group.

### 2. Vixen World Engine

A full-spectrum, multi-ecosystem world auditor built for VRChat creators who need **real-time diagnostics, topology forensics, and automated repair protocols**.

* **Omni-Ecosystem Audits:** Explicitly mapped to **ProTV**, **TXL**, **VizVid**, **IwaSync3**, **AudioLink**, **LTCGI**, **Rinvo**, and **Light Volumes**.
* **ProTV Auditor:** Detects GSV conflicts, emissive GI blowouts, HDR VRAM nukes, RTGI sinks, playlist recursion, and queue UI rebuild storms.
* **VizVid / IwaSync3 / TXL Auditors:** Catch AVPro fallback failures, RateLimitResolver absences, 1080p+ video blowouts, unbounded playlists, starvation-level polling, orphaned Udon behaviours, and GC-heavy event invokers.
* **LTCGI & AudioLink Integrity:** Resolves `BakeInProgress` NRE deadlocks, detects fragmented memory arrays (ghost screens), flags Quest GPU readback stalls, and auto-links missing video output textures.
* **Raw UASM Code Validation:** Intercepts the `UdonSharpEditorCache` to detect `PlayerData.Set` calls inside `Update()` loops, catching network rate-limit bombs before they detonate.
* **World Profiler Dashboard:** Aggregates VRAM, mesh, audio, and UI memory via `Profiler.GetRuntimeMemorySizeLong()` and computes an `OPTIMAL / MODERATE / SEVERE` threat score.
* **Shader Replacement System:** Uses dictionary assets + a protective whitelist to bulk-replace unsafe desktop shaders with mobile-safe targets.

### 3. Avatar Pipeline Tools

A connected set of utilities for building, optimizing, and porting high-fidelity avatars.

* **Avatar Optimization Suite:** A destructive, multi-threaded execution system built around a real **QEM mesh decimator** (Garland-Heckbert edge-collapse, the same class of algorithm as Blender's Decimate modifier) that drives each heavy mesh toward a triangle target while preventing face flips, rejecting sliver triangles, and preserving UV/normal seams, submesh boundaries, blendshapes, and bone weights. A *Decimation Target* slider (2k-70k tris) controls silhouette aggression; a Dual-Shielding system locks facial topology and humanoid hand bones out of the collapse; a Deep Material Inspector Spider hunts hidden materials inside Animator controllers and VRCFury toggles; a **Selective Texture Targeting** panel replaces blanket downscaling with per-texture checkboxes; a per-avatar JSON state flag clears applied tasks so they stop re-listing forever; and hardware-level VRAM profiling reads GPU memory directly. Auto-Fit Bounds fits each renderer's culling box to Unity's own posed `localBounds`, so meshes no longer frustum-cull and vanish up close.
* **Quest Conversion Engine:** A fully non-destructive Android pipeline that clones the avatar into an isolated prefab sandbox. A "Hunter-Killer" Biometric Purge strips PC-VR face tracking (adjerry91, VF_UE_VRCFT, VRCFury branches); a Magick.NET Lanczos pipeline downsamples in linear space with UnsharpMask recovery before ASTC; and an interactive topology view caps PhysBones, colliders, contacts, constraints, particles, and more per target rank. It also auto-remaps **animated material swaps** - scanning standard Animators and `VRCAvatarDescriptor` custom layers, cloning affected controllers and `.anim` clips, and rewiring object-reference curves to the Quest-optimized materials, so material toggles and effects keep working with zero manual controller editing.
* **PhysBone Topology Mapper:** Snapshots a PhysBone architecture into reusable blueprints. "Extract Master Copy" walks the source avatar via `AnimationUtility.CalculateTransformPath`, serializes each `VRCPhysBone` into a `Preset`, and writes a master `ScriptableObject` mapping `bonePath` to `Preset`. "Inject Blueprint" traverses a target by relative path, auto-adds missing components, and re-applies the presets - perfect for restoring physics stripped during Quest conversion or optimization. Degrades gracefully when the VRChat SDK is absent.
* **Accessory Mounting Engine:** Clones sterile armatures from a source rig and surgically mounts accessories. `FullGeneration` clones a fresh sterile armature (recursive `CloneHierarchy` preserving local TRS); `AppendToExisting` reuses an existing rig by relative path. A destructive auto-rig bakes meshes with a per-child bone offset and fully preserves blendshapes, while rigid props use a locked `ParentConstraint`. PhysBone-safe root locking, culling-resistant bounds, GUID-suffixed asset persistence, and single-undo-group rollback round it out.
* **Animation Workbench Pro:** A custom curve-graph animation environment. A Material Property Discovery engine scans renderers and exposes categorized, Poiyomi-aware shader properties (with color/vector channel splitting) as animatable bindings; an easing library and `CurveGraphView` drive staged, non-destructive curve editing; and a real-time `PreviewEngine` plays staged clips directly on a scene object via `AnimationMode.SampleAnimationClip`.
* **Pipeline Preset Manager:** A dual-mode import-automation engine. Authoring Mode creates a phantom asset to rip importer rules into reusable Presets; Extraction Mode rips Presets from existing component hierarchies. Generated presets can be registered globally via `Preset.SetDefaultPresetsForType()` with glob filters, enforcing project-wide texture, audio, renderer, and PhysBone consistency.

### 4. Scene, Convention & QA Tools

* **Vixen Badge Studio:** Procedural badge generation for VRChat conventions. Auto-detects Furality SDK assets, targets Poiyomi / lilToon / VRChat Mobile / legacy badge shaders, generates directory structures, materials, and emissive maps, and snaps text bounds and neon accents to each year's badge mesh.
* **Live Scene UV Mapper:** Hijacks the Scene View camera to raycast badge coordinates directly onto curved 3D meshes, inverting barycentric UVs into ImageMagick pixel space with one-click clipboard copy. Non-destructive (temporary MeshCollider) and ideal for curved or 3D-printed badge surfaces where 2D layout tools fail.
* **Precision Click-to-Place Raycaster:** A sniper-grade Scene View raycaster with a cyan/magenta UV projection disc and VRChat-aware layer masking, plus Live Surface Snapping (`Transform.hasChanged`) and disjointed-pivot floor alignment.
* **Omni-Chaos Generator (QA):** Builds a dedicated `Stress Test.unity` scene with engineered "Nightmare Pods" that simulate catastrophic performance issues across physics, UI voids, VRAM nukes, network persistence sinks, and the third-party ecosystems the World Engine audits.

---

## Architecture & Strategic Value

### 1. Strict Compiler Safeguards

All editor-only scripts are isolated via `.asmdef` and `#if UNITY_EDITOR` directives, preventing runtime bleed-over and catastrophic build failures.

### 2. Automated Build & Release Pipeline

Powered by GitHub Actions:

* **Build Release:** Compiles the package, generates `.zip` + `.unitypackage`, constructs `package.json`, and publishes to Releases.
* **Build Repo Listing:** Reconstructs the VPM repository listing and deploys the cyberpunk storefront to GitHub Pages.
* **Downloads Badge:** A daily job that tallies real `.zip` + `.unitypackage` installs across every release and publishes live download / version endpoints to an orphan `badge-data` branch, powering the badges above and the storefront's live counters.
* **VixenGitWatch Telemetry:** A Python SSE bot that monitors releases, PRs, issues, and workflow runs, routing formatted telemetry to Discord.

### 3. Unified Distribution Platform

Built on the official VRChat VPM template - add the repo to VCC and receive instant updates.

---

## Installation (VRChat Creator Companion)

1. Visit the **[Vixens Toolbox Storefront](https://vixencreations.github.io/VixenToolBox/)**
2. Click **Add to VCC**
3. Open VCC, select your project, and add the Vixens Toolbox package

---

## Documentation

* **Storefront & VPM Listing:** [vixencreations.github.io/VixenToolBox](https://vixencreations.github.io/VixenToolBox/)
* **List of Tools:** [tools.html](https://vixencreations.github.io/VixenToolBox/tools.html)
* **Deep-Dive Docs:** [docs.html](https://vixencreations.github.io/VixenToolBox/docs.html)
* **Shader Docs (VixenWear Latex Ultra inspector reference):** [shaderdocs.html](https://vixencreations.github.io/VixenToolBox/shaderdocs.html)
* **Changelog:** [changelog.html](https://vixencreations.github.io/VixenToolBox/changelog.html)
* **AI Transparency & Development Ethics:** [ai-transparency.html](https://vixencreations.github.io/VixenToolBox/ai-transparency.html)

---

## Development & Contribution

* **Language:** C# (Unity Editor Scripting) + HLSL (ShaderLab)
* **Target Environment:** Unity 2022.3.22f1 / VRChat SDK 3.10.3

Open an Issue for topology edge cases, world diagnostic anomalies, or feature requests.

---

## Acknowledgements

* **[Map1en](https://github.com/Map1en) and the [VRCX-0](https://github.com/Map1en/VRCX-0) project** - our live downloads / version badge pipeline is adapted from their `badge-downloads` workflow. We would not have known we had crossed 1,500+ downloads without reading their source. Thank you.
* **The VRChat community** - for building with our tools and pushing them to their limits.

---

*Maintained by VixForge Interactive*
