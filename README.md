[![VPM-Ready](https://img.shields.io/badge/VPM-Compatible-00e5ff?style=for-the-badge&logo=vrchat)](https://vixencreations.github.io/VixenToolBox/)
[![Unity 2022.3](https://img.shields.io/badge/Unity-2022.3.22f1-lightgrey?style=for-the-badge&logo=unity)](https://unity.com/)
[![Build Status](https://img.shields.io/badge/Build-Automated-ff00aa?style=for-the-badge&logo=githubactions)](https://github.com/VixenCreations/VixenToolBox/actions)

-----

# VixenToolBox 🦊⚡

**The central distribution hub and automated CI/CD pipeline for the VixenTools ecosystem.**

VixenToolBox is a comprehensive, VPM-native suite of Unity Editor utilities and automation pipelines designed to eliminate human error, enforce strict consistency, and push Unity to its technical limits. This repository serves as a first-class, auto-updating VPM package source for VRChat creators.

-----

## 🛠️ The VixenTools Ecosystem

### 1\. Core Infrastructure & Utilities

  * **Vixen Hub:** A centralized, tabbed developer dashboard unifying ecosystem documentation, network routing, and release history. 
      * **Dynamic Version Extraction:** Engineered with a lightweight Regex parser that autonomously reads the ecosystem's `package.json` to extract and display real-time versioning without hardcoded strings.
      * **Changelog Pagination Engine:** Replaces infinite scrolling with a memory-efficient data dictionary and a native UI Toolkit dropdown to dynamically isolate and render individual release histories.
      * **Native Style Hijacking:** Enforces the signature "cyber-noir" aesthetic across the entire VixenTools ecosystem by actively hijacking and overriding base Unity UI Toolkit classes.
  * **Animation Workbench Pro:** An advanced visual workspace for staging, easing, and sampling complex animation curves and material property bindings. Features interactive timeline ribbons, a real-time preview engine, and a heavily optimized, runtime-safe math library (`EasingFunctions.cs`).
  * **Pipeline Preset Manager:** Handles bulk extraction of configuration presets from existing assets and the programmatic authoring of standardized importer settings using a "Phantom Asset" architecture.
  * **Live Surface Snapping:** An enterprise-grade scene utility utilizing Unity's native `Transform.hasChanged` matrix tracking for low-overhead drag detection. Features a recursive bounding algorithm to snap objects perfectly by their geometric "feet," self-collision immunity, and a discrete `Ctrl+Alt+S` hotkey.
  * **Fix Scene Data:** A dedicated utility for repairing, standardizing, and maintaining active scene integrity by forcefully serializing unlinked lightmap references.

### 2\. Avatar Physics & Cross-Platform Topology

  * **Quest Conversion Engine:** A fully non-destructive, high-fidelity pipeline for converting PC avatars to Android/Quest.
      * **ImageMagick Lanczos Pipeline:** Routes PC textures through a high-fidelity Magick.NET downsampling pass before forcefully applying Android ASTC compression, preserving crisp details within strict VRAM limits.
      * **Heuristic PhysBone Culling:** Mathematically calculates skeletal depth to aggressively cull deep leaf bones while preserving vital root-level physics based on targeted Mobile Performance Ranks.
      * **Physics Joint Heuristics:** Actively hunts down and isolates unsupported Unity physics constraints (`SpringJoint`, `FixedJoint`, `HingeJoint`) into a dedicated interactive matrix, mathematically locking them for removal to guarantee 0 SDK validation errors.
      * **Interactive Topology Matrix:** A dynamic UI matrix that automatically applies heuristic culling limits and allows the creator to manually override which physics components survive the Quest conversion.
      * **High-Fidelity Material Translation:** Defaults to `VRChat/Mobile/Toon Standard`, hunting for PC-side metallic, gloss, normal, and emission properties across third-party shaders (Poiyomi, lilToon) and injecting them into the mobile shader to preserve maximum visual depth.
  * **PhysBone Topology Mapper:** The flagship utility for physics management. Automates PhysBone architecture through a two-phase **Extraction** and **Injection** process.
  * **Master Blueprints:** Utilizes `AnimationUtility.CalculateTransformPath` and Unity `.preset` files to bypass native prefab constraints, mapping complex physics matrices seamlessly across different avatar versions or base models (e.g., Novabeast Master Topology).

### 3\. Convention & Identity Pipelines

  * **Vixen Badge Studio:** A high-fidelity, procedural generation engine for authoring and compositing VRChat convention badges (natively supporting Furality Luma, Somna, Sylva, and Umbra).
      * **Ecosystem Discovery Engine:** Utilizes recursive deep-scanning to dynamically locate Furality SDK assets, bypassing inconsistent year-to-year folder restructuring.
      * **Universal Shader Targeting:** A heuristic material engine that automatically detects and configures targets across Poiyomi Toon, lilToon, VRChat Mobile, and legacy convention shaders.
      * **Programmatic Template Authoring:** Allows users to ingest raw convention `.jpg` textures or generate procedural bases, automatically transcoding assets and scaffolding the required directory and material architecture.
      * **Dynamic UV Auto-Layout:** Intercepts convention selections and automatically snaps internal text bounds, rotational math, and signature neon hex colors to perfectly map onto the specific year's 3D mesh.

-----

## 🚀 Architecture & Strategic Value

This repository goes beyond hosting code; it acts as a bulletproof, automated infrastructure backbone:

### 1\. Strict Compiler Safeguards

Assembly Definitions (`.asmdef`) and `#if UNITY_EDITOR` directives deeply isolate all editor-only scripts. This guarantees zero compiler bleed-over, preventing catastrophic errors when users build their avatars or worlds for runtime.

### 2\. Automated Build & Release Pipeline

Powered by heavily customized GitHub Actions and an enterprise-grade CI/CD telemetry matrix:

  * **Build Release:** Automatically compiles the package, generates both `.zip` and `.unitypackage` formats, constructs the `package.json`, and publishes to GitHub Releases.
  * **Build Repo Listing:** Scans all releases, reconstructs the VPM repository listing, applies custom Scriban templating, and deploys the glassmorphic, cyberpunk-styled storefront to GitHub Pages.
  * **VixenGitWatch Telemetry:** A fully autonomous, self-healing Python bot running via Server-Sent Events (SSE) that natively intercepts over 15 GitHub lifecycle events (releases, PRs, workflow runs, issue tracking) and routes formatted telemetry directly to the community Discord.

### 3\. Unified Distribution Platform

Built on the official VRChat VPM Package Template, this repo allows users to seamlessly add the VixenTools ecosystem directly to their VRChat Creator Companion (VCC). It guarantees seamless versioning, dependency management, and instant updates.

-----

## 📦 Installation (VRChat Creator Companion)

To integrate the VixenTools suite into your Unity environment:

1.  Navigate to our [VixenToolBox Storefront](https://vixencreations.github.io/VixenToolBox/).
2.  Click **Add to VCC**.
3.  Open your VRChat Creator Companion, select your project, and add the VixenTools packages from your newly linked repository.

-----

## 💻 Development & Contribution

  * **Primary Language:** C# (Unity Editor Scripting)
  * **Target Environment:** Unity 2022.3.22f1 / VRChat SDK

If you encounter topology edge-cases or wish to request specific pipeline automations, please open an Issue. Ensure you provide relevant console outputs and hierarchy structures.

-----

*Maintained by Vixenlicious*
