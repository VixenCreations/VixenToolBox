[![VPM-Ready](https://img.shields.io/badge/VPM-Compatible-00e5ff?style=for-the-badge&logo=vrchat)](https://vixencreations.github.io/VixenToolBox/)
[![Unity 2022.3](https://img.shields.io/badge/Unity-2022.3.22f1-lightgrey?style=for-the-badge&logo=unity)](https://unity.com/)
[![VRChat SDK](https://img.shields.io/badge/VRChat%20SDK-3.10.3-ff0055?style=for-the-badge&logo=vrchat)](https://vrchat.com/)
[![Version](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FVixenCreations%2FVixenToolBox%2Fbadge-data%2Fversion.json&style=for-the-badge)](https://github.com/VixenCreations/VixenToolBox/releases/latest)
[![Downloads](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FVixenCreations%2FVixenToolBox%2Fbadge-data%2Fdownloads.json&style=for-the-badge)](https://github.com/VixenCreations/VixenToolBox/releases)
[![CodeQL](https://github.com/VixenCreations/VixenToolBox/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/VixenCreations/VixenToolBox/actions/workflows/github-code-scanning/codeql)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)

---

# Vixens Toolbox

A free set of Unity editor tools for VRChat avatars and worlds, by VixForge Interactive. It checks your work against VRChat's limits, fixes what it can in one click, and takes the repetitive setup off your hands.

* **Needs:** Unity 2022.3.22f1 and VRChat SDK 3.10.3 or newer
* **Package:** `com.vixencreations.vixens-toolbox`
* **Website:** [vixencreations.github.io/VixenToolBox](https://vixencreations.github.io/VixenToolBox/)

---

## Install

1. Open the **[Vixens Toolbox website](https://vixencreations.github.io/VixenToolBox/)** and click **Add to VCC**.
2. In the VRChat Creator Companion, open your project and add the **Vixens Toolbox** package.
3. In Unity, open **VixenTools > Hub Dashboard**.

The Hub shows the tools your project can use: avatar projects get the avatar tools, world projects get the world tools, and both get the Unity tools.

---

## What's inside

### Worlds and scenes

* **Vixen World Engine:** runs more than 160 checks on your world and fixes the ones you tick.
* **Quest World Converter:** makes Quest copies of your world's materials and leaves the originals alone.
* **Live Surface Snapping** and **Precision Click-to-Place:** drop objects onto the surface below them, or click where you want them.
* **Omni-Chaos Generator:** builds a test scene full of deliberate problems for the World Engine to catch.

### Avatars

* **Optimization Suite:** checks your avatar against VRChat's limits, then shrinks meshes and textures to fit.
* **Quest Conversion Engine:** makes a Quest copy of your avatar and leaves the original alone.
* **Animator Forge:** finds problems in your animator, and builds toggles, sliders and swaps for you.
* **Material Conflict Finder:** finds materials whose toggles disagree and settles them.
* **PhysBone Blueprints:** saves an avatar's PhysBones and puts them back on another one.
* **Accessory Mounting Engine:** mounts accessories onto a clean copy of your armature.
* **Badge Studio:** builds VRChat convention badges, Furality included.

### Unity editor

* **Scene View Enhancer:** lowers the editor's frame rate while it's idle, and shows how often the Scene view redraws.
* **Memory Diagnostics:** shows which textures, materials and meshes a tool made and never freed.
* **Animation Workbench Pro:** build and ease animation curves with a live preview.
* **Pipeline Preset Manager:** turns import settings into presets and makes them the default.
* **Fix Scene Data:** reattaches a scene's lighting data when it goes missing.

Want the full picture? **[Details.md](Details.md)** covers every tool in depth, plus how the toolbox is built and released.

---

## Links

* **[Details](Details.md):** every tool in depth, and how the toolbox is built
* **[Tools](https://vixencreations.github.io/VixenToolBox/tools.html)**
* **[Changelog](https://vixencreations.github.io/VixenToolBox/changelog.html)**
* **[Support](https://vixencreations.github.io/VixenToolBox/support.html)**
* **[AI Transparency](https://vixencreations.github.io/VixenToolBox/ai-transparency.html)**

Found a bug or want a feature? [Open an issue](https://github.com/VixenCreations/VixenToolBox/issues).

---

## License

The Vixens Toolbox is open source under the [MIT License](LICENSE), and the same text ships inside the package as `LICENSE.md`. Magick.NET and ImageMagick come with it under their own licenses, listed in [Third Party Notices](Packages/com.vixen.tools/Third%20Party%20Notices.md).

---

## Thanks

* **[Map1en](https://github.com/Map1en) and the [VRCX-0](https://github.com/Map1en/VRCX-0) project:** our downloads and version badges are adapted from their `badge-downloads` workflow. We would not have known we had crossed 1,500 downloads without reading their source. Thank you.
* **The VRChat community:** for building with our tools and pushing them to their limits.

---

*Maintained by VixForge Interactive*
