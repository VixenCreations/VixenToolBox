[![VPM-Ready](https://img.shields.io/badge/VPM-Compatible-00e5ff?style=for-the-badge&logo=vrchat)](https://vixencreations.github.io/VixenToolBox/)
[![Unity 2022.3](https://img.shields.io/badge/Unity-2022.3.22f1-lightgrey?style=for-the-badge&logo=unity)](https://unity.com/)
[![VRChat SDK](https://img.shields.io/badge/VRChat%20SDK-3.10.3-ff0055?style=for-the-badge&logo=vrchat)](https://vrchat.com/)
[![Version](https://img.shields.io/badge/Release-v2.4.0-ffaa00?style=for-the-badge)](https://github.com/VixenCreations/VixenToolBox/releases)

---

# Vixens Toolbox

**The central distribution hub and automated CI/CD pipeline for the Vixens Toolbox ecosystem, by VixForge Interactive.**

The Vixens Toolbox is a VPM-native, cyber-noir suite of Unity Editor utilities and a flagship avatar shader, engineered to eliminate human error, enforce strict project consistency, and push Unity to its technical limits.

This repository acts as the **first-class VPM package source**, powering the entire ecosystem with automated updates, documentation, and infrastructure.

* **Package:** `com.vixencreations.vixens-toolbox` (v2.4.0)
* **Target:** Unity 2022.3.22f1 / VRChat SDK 3.10.3
* **Storefront & Docs:** [vixencreations.github.io/VixenToolBox](https://vixencreations.github.io/VixenToolBox/)

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

* **Avatar Optimization Suite:** A destructive, multi-threaded execution system. A Vertice Mesh Welder fuses vertices using a 5D XYZ + UV key to crush polycounts while preserving seams; a Dual-Shielding system locks facial topology out of the decimation grid via material-keyword and humanoid-bone analysis; a Deep Material Inspector Spider hunts hidden materials inside Animator controllers and VRCFury toggles; and hardware-level VRAM profiling reads GPU memory directly.
* **Quest Conversion Engine:** A fully non-destructive Android pipeline that clones the avatar into an isolated prefab sandbox. A "Hunter-Killer" Biometric Purge strips PC-VR face tracking (adjerry91, VF_UE_VRCFT, VRCFury branches); a Magick.NET Lanczos pipeline downsamples in linear space with UnsharpMask recovery before ASTC; and an interactive topology view caps PhysBones, colliders, contacts, constraints, particles, and more per target rank.
* **PhysBone Topology Mapper:** Snapshots a PhysBone architecture into reusable blueprints. "Extract Master Copy" walks the source avatar via `AnimationUtility.CalculateTransformPath`, serializes each `VRCPhysBone` into a `Preset`, and writes a master `ScriptableObject` mapping `bonePath` to `Preset`. "Inject Blueprint" traverses a target by relative path, auto-adds missing components, and re-applies the presets - perfect for restoring physics stripped during Quest conversion or optimization. Degrades gracefully when the VRChat SDK is absent.
* **Accessory Mounting Engine:** Clones sterile armatures from a source rig and surgically mounts accessories. `FullGeneration` clones a fresh sterile armature (recursive `CloneHierarchy` preserving local TRS); `AppendToExisting` reuses an existing rig by relative path. A destructive auto-rig bakes meshes with a per-child bone offset and fully preserves blendshapes, while rigid props use a locked `ParentConstraint`. PhysBone-safe root locking, culling-resistant bounds, GUID-suffixed asset persistence, and single-undo-group rollback round it out.
* **Animation Workbench Pro:** A custom curve-graph animation environment. A Material Property Discovery engine scans renderers and exposes categorized, Poiyomi-aware shader properties (with color/vector channel splitting) as animatable bindings; an easing library and `CurveGraphView` drive staged, non-destructive curve editing; and a real-time `PreviewEngine` plays staged clips directly on a scene object via `AnimationMode.SampleAnimationClip`.
* **Pipeline Preset Manager:** A dual-mode import-automation engine. Authoring Mode creates a phantom asset to rip importer rules into reusable Presets; Extraction Mode rips Presets from existing component hierarchies. Generated presets can be registered globally via `Preset.SetDefaultPresetsForType()` with glob filters, enforcing project-wide texture, audio, renderer, and PhysBone consistency.

### 4. VixenWear Latex Ultra - Native Shader Architecture

A proprietary, high-fidelity dual-lobe PBR surface shader engineered for synthetic materials, shipping alongside a tessellation-free **Latex Ultra SPS** variant for VRCFury SPS patching. Its inspector is organized into six tabs (BASE, SURFACE, POLISH, INTEGRATION, AUDIOLINK, STAGE).

* **Physical Lighting Model:** A full industry-standard GGX BRDF stack (`D_GGX`, `V_SmithJointGGX`, `F_Schlick`, `Burley` diffuse, Karis split-sum) with optional multi-scatter energy compensation, anisotropic latex-stretch specular, thin-part transmission, tinted dielectric clearcoat, geometric specular AA, and a VRChat mirror-camera fix for per-eye-correct specular.
* **Texture Packing & Compatibility:** A single packed RGBA PBR map with Poiyomi / Substance / Marmoset channel selectors, Mochie reflection & specular masks with a one-click setup button, and a Standard-style render-mode selector (Opaque / Cutout / Fade / Transparent).
* **Liquid Surface System (2.4.0):** A master Polish gate, a wet / run-off layer (soaked look, animated rivulets, PC-only geometry water droplets), and gravity-aligned melting goo bring physical liquid behaviour to the latex.
* **World Integration & Kinetics:** Deep Light Volumes and LTCGI mix controls, the VRSL stage-hijack protocol with DMX geo-warping, runtime-gated AudioLink, the God Tier Cybernetics HUD, and a flying-shard kinetic vertex engine.
* **Hybrid Inspector:** A custom `VectorLabelDrawer` (`MaterialPropertyDrawer`) hooked into ShaderLab vector arrays, wrapped in a UITK + IMGUI hybrid inspector with per-tab copy/paste/reset, and a build-time variant stripper that drops dead keyword variants and unused passes.

> Every option in every tab is documented control-by-control, with full editor screenshots, in the [Shader Docs](https://vixencreations.github.io/VixenToolBox/shaderdocs.html).

### 5. Scene, Convention & QA Tools

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

*Maintained by VixForge Interactive*
