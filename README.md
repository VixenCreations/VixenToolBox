[![VPM-Ready](https://img.shields.io/badge/VPM-Compatible-00e5ff?style=for-the-badge&logo=vrchat)](https://vixencreations.github.io/VixenToolBox/)
[![Unity 2022.3](https://img.shields.io/badge/Unity-2022.3.22f1-lightgrey?style=for-the-badge&logo=unity)](https://unity.com/)
[![Build Status](https://img.shields.io/badge/Build-Automated-ff00aa?style=for-the-badge&logo=githubactions)](https://github.com/VixenCreations/VixenToolBox/actions)

-----

# VixenToolBox 🦊⚡  
**The central distribution hub and automated CI/CD pipeline for the VixenTools ecosystem.**

VixenToolBox is a VPM-native, cyber‑noir suite of Unity Editor utilities engineered to eliminate human error, enforce strict project consistency, and push Unity to its technical limits.  
This repository acts as the **first‑class VPM package source**, powering the entire VixenTools ecosystem with automated updates, documentation, and infrastructure.

-----

## 🛠️ The VixenTools Ecosystem

### 1. Core Infrastructure & Hub Architecture

**Vixen Hub** — the command center of the ecosystem.  
A fully UI Toolkit–driven dashboard with tabs for:

- **Ecosystem Architecture**  
- **Core Modules**  
- **Network Routing**  
- **Support**  
- **Release Changelogs** (with version selector)

Key Hub features:

- **Dynamic Version Extraction:**  
  Reads `package.json` directly to display live package + SDK versions without hardcoded strings.

- **Changelog Pagination Engine:**  
  Parses `CHANGELOG.md` into isolated entries with a native dropdown selector.

- **Autonomous Scene HUD Notifier:**  
  Injects a neon cyber badge into the Scene View when updates are detected, routing directly to the Hub’s changelog tab.

- **Native Style Hijacking:**  
  Overrides Unity’s base UI Toolkit classes to enforce the signature neon‑cyber aesthetic across all tools.

- **Reactive Scene State Routing:**  
  Tools launched from the Hub instantly update their UI state using synchronous layout flushing — no domain reloads required.

### 2. World Engine — Omni-Matrix Diagnostic Spider

A full-spectrum, multi‑ecosystem world auditor built for VRChat creators who need **real‑time diagnostics, topology forensics, and automated repair protocols**.

Capabilities:

- **ProTV Auditor:**  
  Detects GSV conflicts, emissive GI blowouts, HDR VRAM nukes, RTGI sinks, playlist recursion, and queue UI rebuild storms.

- **TXL Auditor:**  
  Flags starvation-level polling, collapsed translation tables, forced collider checks, orphaned Udon behaviours, and cryptographic sinks.

- **IwaSync3 Auditor:**  
  Detects 1080p+ video blowouts, unbounded playlists, global non‑spatialized audio, emissive screen overdrive, and GC-heavy event invokers.

- **Native Video Pipeline Catchers:**  
  Auto-detects and fixes “Unlimited (0)” resolution bandwidth nukes and low-latency instability in AVPro/Unity video players.

- **Raw UASM Bytecode Scraper:**  
  Intercepts the `UdonSharpEditorCache` to detect `PlayerData.Set` calls inside Update loops — catching rate‑limit bombs before they detonate.

- **World Profiler Dashboard:**  
  Aggregates VRAM, mesh, audio, and UI memory; estimates draw calls; and computes a threat score with OPTIMAL/MODERATE/SEVERE classification.

- **Shader Replacement Matrix:**  
  Uses dictionary assets + whitelist to bulk-replace unsafe desktop shaders with mobile-safe targets.

### 3. Avatar Optimization & Cross-Platform Topology

**Optimization Suite** — a destructive, multi-threaded execution matrix for PC avatars.

- **5D Spatial Hash Welder:**  
  Fuses vertices using XYZ + UV coordinates to preserve seams while crushing polycounts.

- **Dual-Shielding Exclusion Matrix:**  
  Protects facial topology using material keyword scanning + humanoid bone weight analysis.

- **Deep Material Inspector Spider:**  
  Hunts down hidden materials inside Animator controllers and VRCFury toggles.

- **Hardware-Level VRAM Profiling:**  
  Reads GPU memory directly via `Profiler.GetRuntimeMemorySizeLong()`.

### 4. Quest Conversion Engine — Non-Destructive Android Pipeline

A fully isolated, high-fidelity Quest conversion system.

- **Biometric Matrix Purge (Hunter-Killer):**  
  Removes all PC-VR face tracking templates (adjerry91, VRCFury, VF_UE_VRCFT).

- **Magick.NET Lanczos Pipeline:**  
  Linear downsampling + UnsharpMask recovery before ASTC compression.

- **ASTC Pipeline Sync:**  
  Fixes sRGB/Linear mismatches to prevent corrupted normals.

- **Interactive Topology Matrix:**  
  PhysBones, colliders, contacts, constraints, raycasts, particles, trails, lines, joints, incompatible components — all categorized with per-node toggles.

- **Texture Processing Matrix:**  
  Per-texture toggles for ImageMagick processing with resolution metadata.

### 5. Scene Tools & QA Protocol

- **Precision Click-to-Place Raycaster:**  
  A sniper-grade Scene View raycaster with cyan/magenta UV projection disc and VRChat-aware layer masking.

- **Live Surface Snapping:**  
  Uses `Transform.hasChanged` to drop objects flush to geometry with surgical collider shielding.

- **Disjointed Pivot Alignment:**  
  Computes true lowest bounds across colliders/renderers for perfect floor alignment.

- **Omni-Chaos Generator:**  
  Builds a dedicated `Stress Test.unity` scene with dynamic “Nightmare Pods” for ProTV, TXL, IwaSync3, VRAM nukes, UI voids, and more.

### 6. Convention & Identity Pipelines

**Vixen Badge Studio** — procedural badge generation for VRChat conventions.

- **Ecosystem Discovery Engine:**  
  Auto-detects Furality SDK assets across inconsistent folder structures.

- **Universal Shader Targeting:**  
  Supports Poiyomi, lilToon, VRChat Mobile, and legacy badge shaders.

- **Programmatic Template Authoring:**  
  Generates directory structures, materials, and emissive maps automatically.

- **Dynamic UV Auto-Layout:**  
  Snaps text bounds and neon hex accents to each year’s badge mesh.

-----

## 🚀 Architecture & Strategic Value

### 1. Strict Compiler Safeguards
All editor-only scripts are isolated via `.asmdef` and `#if UNITY_EDITOR` directives, preventing runtime bleed-over and catastrophic build failures.

### 2. Automated Build & Release Pipeline
Powered by GitHub Actions:

- **Build Release:**  
  Compiles the package, generates `.zip` + `.unitypackage`, constructs `package.json`, and publishes to Releases.

- **Build Repo Listing:**  
  Reconstructs the VPM repository listing and deploys the cyberpunk storefront to GitHub Pages.

- **VixenGitWatch Telemetry:**  
  A Python SSE bot that monitors releases, PRs, issues, and workflow runs, routing formatted telemetry to Discord.

### 3. Unified Distribution Platform
Built on the official VRChat VPM template — add the repo to VCC and receive instant updates.

-----

## 📦 Installation (VRChat Creator Companion)

1. Visit the **[VixenToolBox Storefront](https://vixencreations.github.io/VixenToolBox/)**  
2. Click **Add to VCC**  
3. Open VCC → your project → add VixenTools packages

-----

## 💻 Development & Contribution

- **Language:** C# (Unity Editor Scripting)  
- **Target Environment:** Unity 2022.3.22f1 / VRChat SDK  

Open an Issue for topology edge cases, world diagnostic anomalies, or feature requests.

-----

*Maintained by Vixenlicious*
