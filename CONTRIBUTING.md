# Contributing to the Vixens Toolbox

Thanks for wanting to help! Bug reports, ideas, fixes and docs changes are all welcome. Everyone taking part follows our [Code of Conduct](CODE_OF_CONDUCT.md).

## Reporting a bug

[Open an issue](https://github.com/VixenCreations/VixenToolBox/issues/new/choose) with the bug report template. The most useful things to include:

* the toolbox version (the VRChat Creator Companion shows it), your Unity version and your VRChat SDK version
* which tool you used, and the steps that cause the problem
* the full error from the Unity Console, if there is one

Found a security problem? Please don't open a public issue. Follow [SECURITY.md](SECURITY.md) instead. For anything that makes the toolbox hard to use with a disability, see [ACCESSIBILITY.md](ACCESSIBILITY.md).

## Suggesting a feature

Open an issue with the feature request template. Start with the problem you're trying to solve: it tells us more than a description of the solution.

## Making changes

### Setting up

1. Fork the repository and clone your fork.
2. Add the folder to the VRChat Creator Companion as an existing project.
3. Add the VRChat Avatars SDK or Worlds SDK, depending on which tools you're working on, and open the project in Unity 2022.3.22f1.

The toolbox itself lives in `Packages/com.vixen.tools`. The website is plain HTML in `Website/`, published with GitHub Pages.

### Guidelines

* **Keep each pull request to one fix or feature.** Small changes get reviewed faster.
* **Test it in Unity** on a real avatar or world, and make sure the Console shows no new errors or warnings.
* **Load package files by the package name,** `Packages/com.vixencreations.vixens-toolbox/...`. Unity looks packages up by name, so the folder name `com.vixen.tools` won't resolve.
* **Wrap Magick.NET code** in `#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || VIXEN_MAGICK_NET`. Its native libraries only ship for Windows and Linux, and the guard keeps every other tool working on macOS.
* **Don't rename saved names.** Serialized field names, menu paths and `EditorPrefs` keys are read by people's saved projects. Change the label people see and keep the name underneath.
* **Use plain words in the UI.** Name the action and what it acts on, like **Scan Avatar**.
* **Keep comments short.** Use single `//` lines, and put the longer explanation in your pull request.
* **Leave versions and releases to us.** Please don't change the version in `package.json`, add `CHANGELOG.md` entries, or commit `.unitypackage` or `.zip` builds. We write the changelog and build each release with GitHub Actions.
* **Update the docs** when you change what a tool does: [Details.md](Details.md) and the tool's page under `Website/`.
* **Follow the [accessibility expectations](ACCESSIBILITY.md#contributor-expectations)** for anything people see.

### Opening a pull request

Open your pull request against `main`. Tell us what changed, why, and how you tested it, and add a screenshot for anything that changes how a window looks. We'll review it as soon as we can and may ask for a few changes.

## License

By contributing, you agree that your work is released under the [MIT License](LICENSE), the same as the rest of the toolbox. The one exception is `.github/workflows/badge-downloads.yml`, which is under the GNU GPL v3, so changes to that file are too.

## Questions

Ask on our [Discord](https://discord.com/invite/3vbJCKcPtJ) or [open an issue](https://github.com/VixenCreations/VixenToolBox/issues).
