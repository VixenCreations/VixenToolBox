# VixForge Interactive

## Overview

VixForge Interactive is an independent creator-tools studio building professional-grade software for the VRChat platform. We develop shaders, avatar and world tooling, and Unity editor automation that help creators produce high-fidelity content faster and with less friction.

Our work targets Unity 2022.3 on the Built-In Render Pipeline, the active standard for VRChat projects, and ships through the VRChat Package Manager (VPM).

## Flagship Product: Vixens Toolbox

The Vixens Toolbox is our professional-grade VRChat package suite, delivering avatar optimization, mobile conversion, world auditing, and animation authoring in one integrated package.

The four flagship tools:

- **Quest Conversion Engine** - a non-destructive PC to Android conversion pipeline that clones the avatar into an isolated workspace, maps every VRChat mobile limit per performance rank, and routes textures through Magick.NET before ASTC compression.
- **Avatar Optimization Suite** - QEM (Garland-Heckbert) mesh decimation with face and hand shielding, leaf-bone collapsing, tight-fit skinned bounds, and ImageMagick-driven VRAM control.
- **Vixen World Engine** - the omni-system diagnostic spider, running roughly 137 checks across nine third-party ecosystems (ProTV, TXL, VizVid, IwaSync3, AudioLink, LTCGI, Rinvo, VRC Light Volumes, VRSL), most carrying a one-click fix.
- **Animation Workbench Pro** - a curve authoring environment with material and component property binding, an easing library, staged non-destructive edits, and real-time preview.

Supporting tools: **Vixen Hub** (the unified editor hub that ties the suite together), **Animator Forge**, **PhysBone Blueprints**, the **Accessory Mounting Engine**, **Vixen Badge Studio**, and the Pipeline Preset Manager.

Note: **VixenWear Latex Ultra** is a separate standalone product and does not ship inside the Vixens Toolbox.

## Focus and Research

Beyond the shipping toolbox, VixForge Interactive invests in R&D for next-generation creator workflows: UdonSharp and C# frameworks, spatial engine experiments, automation pipelines, and prototype editor systems that feed back into our released tools.

## Extended Projects and R&D

### Experimental and Standalone Tools

- **VixenLens** - A standalone, high-performance metadata indexing and retrieval engine. Built on a Rust backend with a Tauri frontend, it autonomously traverses and indexes thousands of VRChat snapshots to bring order to massive historical archives. Stack: Rust, Tauri, metadata parsing.
- **Fish Validator** - A community-focused validation engine that appraises and validates structured data within specialized environments, backed by a fully documented ecosystem wiki. Stack: data validation, community tooling, wiki documented.
- **Stream Connector** - An ultra-low-latency, multi-threaded orchestration engine that bridges live broadcast events straight into physical hardware states. Local-first and fail-closed, with a tri-state concurrency core and a haptic abstraction layer spanning PiShock, Intiface, OWO, and OSC. Stack: Python / asyncio, local-first, haptics and OSC.

### In Active Development

- **Latex Ultra (VixenWear)** - _Shipping and evolving, sold separately from the Vixens Toolbox._ Our GGX latex avatar shader: clearcoat, thin-film, wet / drip and melting-goo effects, triple matcaps, and live reactions to every major world-lighting system (AudioLink, LTCGI, VRC Light Volumes, and VRSL). Built-In Render Pipeline, with both a base and an SPS twin.
- **Surface Ultra (VixenWorld)** - _In development._ The world-side counterpart to Latex Ultra, sharing the same BRDF and world-lighting stack so VRChat worlds and props can react to the same systems your avatar does.
- **ClothingPro (VixenWear)** - _In development._ A modular clothing and wardrobe-layer system built on top of the VixenWear pipeline, designed to drop layered outfits in cleanly without manual rig surgery.
- **VixForge Director** - _In development._ An over-the-top automatic Virtual Jockey system that puts on full lighting shows when a DJ does not have a VJ on hand to run the visuals. Stack: pipeline, worlds, UdonSharp.

## Links

- Website: https://vixencreations.github.io/VixenToolBox/
- Repository: https://github.com/VixenCreations/VixenToolBox
- X (Twitter): @VixForge (https://x.com/VixForge)
- YouTube: @vixenlicous
- Discord: https://discord.gg/3vbJCKcPtJ

## Brand Reference

- Studio name: **VixForge Interactive** (capital F).
- Product suite: **Vixens Toolbox**.
- Combined lockup: **VixForge Interactive | Vixens Toolbox**.
- Individual tools keep their `Vixen X` names (Vixen Hub, Vixen World Engine, Vixen Badge Studio).
- The flagship toolbox tools are the **Quest Conversion Engine**, the **Avatar Optimization Suite**, the **Vixen World Engine**, and **Animation Workbench Pro**.
- **VixenWear Latex Ultra** (with the Latex Ultra SPS variant) is our shader line. It is a standalone product and is not part of the Vixens Toolbox package.
- Repository, URL, and package identifiers remain under `VixenToolBox` / `vixencreations` (VPM package id `com.vixencreations.vixens-toolbox`).
