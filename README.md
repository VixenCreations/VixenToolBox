-----

# VixenToolBox 🦊⚡

**The central distribution hub and automated CI/CD pipeline for the VixenTools ecosystem.**

[![VPM-Ready](https://img.shields.io/badge/VPM-Compatible-00e5ff?style=for-the-badge&logo=vrchat)](https://vixencreations.github.io/VixenToolBox/)
[![Unity 2022.3](https://img.shields.io/badge/Unity-2022.3.22f1-lightgrey?style=for-the-badge&logo=unity)](https://unity.com/)
[![Build Status](https://img.shields.io/badge/Build-Automated-ff00aa?style=for-the-badge&logo=githubactions)](https://github.com/VixenCreations/VixenToolBox/actions)

VixenToolBox is a comprehensive, VPM-native suite of Unity Editor utilities and automation pipelines designed to eliminate human error, enforce strict consistency, and push Unity to its technical limits. This repository serves as a first-class, auto-updating VPM package source for VRChat creators.

-----

## 🛠️ The VixenTools Ecosystem

### 1\. Core Infrastructure & Utilities

  * **Animation Workbench Pro:** An advanced visual workspace for staging, easing, and sampling complex animation curves and material property bindings. Features interactive timeline ribbons, a real-time preview engine, and a heavily optimized, runtime-safe math library (`EasingFunctions.cs`).
  * **Pipeline Preset Manager:** Handles bulk extraction of configuration presets from existing assets and the programmatic authoring of standardized importer settings using a "Phantom Asset" architecture.
  * **Fix Scene Data:** A dedicated utility for repairing, standardizing, and maintaining active scene integrity.
  * **VixenTools Hub:** A centralized developer dashboard (via the *About Links* menu) providing quick access to documentation, video guides, and community repositories.
  * **In-Editor Changelog:** A custom Markdown parser that renders the package's `CHANGELOG.md` directly inside Unity with native rich-text and ecosystem branding.

### 2\. Avatar Physics & Topology

  * **PhysBone Topology Mapper:** The flagship utility for physics management. Automates PhysBone architecture through a two-phase **Extraction** and **Injection** process.
  * **Master Blueprints:** Utilizes `AnimationUtility.CalculateTransformPath` and Unity `.preset` files to bypass native prefab constraints, mapping complex physics matrices seamlessly across different avatar versions or base models (e.g., Novabeast Master Topology).

-----

## 🚀 Architecture & Strategic Value

This repository goes beyond hosting code; it acts as a bulletproof, automated infrastructure backbone:

### 1\. Strict Compiler Safeguards

Assembly Definitions (`.asmdef`) and `#if UNITY_EDITOR` directives deeply isolate all editor-only scripts. This guarantees zero compiler bleed-over, preventing catastrophic errors when users build their avatars or worlds for runtime.

### 2\. Automated Build & Release Pipeline

Powered by heavily customized GitHub Actions, our `.github/workflows` eliminate manual packaging:

  * **Build Release:** Automatically compiles the package, generates both `.zip` and `.unitypackage` formats, constructs the `package.json`, and publishes to GitHub Releases.
  * **Build Repo Listing:** Scans all releases, reconstructs the VPM repository listing, applies custom Scriban templating, and deploys the glassmorphic, cyberpunk-styled storefront to GitHub Pages.

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

  * **Primary Language:** C\# (Unity Editor Scripting)
  * **Target Environment:** Unity 2022.3.22f1 / VRChat SDK

If you encounter topology edge-cases or wish to request specific pipeline automations, please open an Issue. Ensure you provide relevant console outputs and hierarchy structures.

-----

*Maintained by VixenCreations*
