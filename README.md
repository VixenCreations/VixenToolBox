# VixenToolBox 🦊⚡

**The central distribution hub and automated CI/CD pipeline for the VixenTools ecosystem.**

[![VPM-Ready](https://img.shields.io/badge/VPM-Compatible-00e5ff?style=for-the-badge&logo=vrchat)](https://vixencreations.github.io/VixenToolBox/)
[![Unity 2022.3](https://img.shields.io/badge/Unity-2022.3.22f1-lightgrey?style=for-the-badge&logo=unity)](https://unity.com/)
[![Build Status](https://img.shields.io/badge/Build-Automated-ff00aa?style=for-the-badge&logo=githubactions)](https://github.com/VixenCreations/VixenToolBox/actions)

VixenToolBox is a suite of Unity editor utilities, automation pipelines, and avatar architecture frameworks designed to enforce consistency, eliminate human error, and push Unity to its technical limits. This repository serves as a first-class, auto-updating VPM package source for VRChat creators.

---

## 🚀 Architecture & Strategic Value

This repository goes beyond hosting code; it acts as an automated infrastructure backbone:

### 1. Unified Distribution Platform
Built on the official VRChat VPM Package Template, this repo allows users to seamlessly add the VixenTools ecosystem directly to their VRChat Creator Companion (VCC). It guarantees seamless versioning, dependency management, and instant updates.

### 2. Automated Build & Release Pipeline
Powered by heavily customized GitHub Actions, our `.github/workflows` eliminate manual packaging:
* **Build Release:** Automatically compiles the package, generates both `.zip` and `.unitypackage` formats, constructs the `package.json`, and publishes to GitHub Releases.
* **Build Repo Listing:** Scans all releases, reconstructs the VPM repository listing, applies custom Scriban templating, and deploys the sleek storefront to GitHub Pages.

### 3. Modular Ecosystem
Designed for massive scalability. Whether handling complex `PhysBoneTopologyMapper` extractions or future material automation, the underlying C# architecture is strictly modular.

---

## 📦 Installation (VRChat Creator Companion)

To integrate the VixenTools suite into your Unity environment:

1. Navigate to our [VixenToolBox Storefront](https://vixencreations.github.io/VixenToolBox/).
2. Click **Add to VCC**.
3. Open your VRChat Creator Companion, select your project, and add the VixenTools packages from your newly linked repository.

---

## 💻 Development & Contribution

* **Primary Language:** C# (Unity Editor Scripting)
* **Target Environment:** Unity 2022.3.22f1 / VRChat SDK

If you encounter topology edge-cases or wish to request specific pipeline automations, please open an Issue. Ensure you provide relevant console outputs and hierarchy structures.

---
*Maintained by VixenCreations*
