# Security Policy

## Supported versions

We fix security problems in the latest release only. To get a fix, update the Vixens Toolbox in the VRChat Creator Companion.

| Version | Gets security fixes |
| ------- | ------------------- |
| [Latest release](https://github.com/VixenCreations/VixenToolBox/releases/latest) | Yes |
| Any older release | No |

## What to report

The toolbox runs inside the Unity editor, with the same access to your files that Unity has. It makes no network requests of its own: links in the Hub just open in your browser.

Please report anything that could let someone harm a person using the toolbox, for example:

* a file, image or scene that makes a tool run code, or write or delete files outside the project
* a security problem in the bundled Magick.NET or ImageMagick that means we should update them
* a way to tamper with our releases, the VCC listing or the GitHub workflows that build them
* a problem on the [website](https://vixencreations.github.io/VixenToolBox/), such as injected scripts

Problems in Unity, the VRChat SDK or VRChat itself belong with Unity or VRChat. Ordinary bugs go in a normal [issue](https://github.com/VixenCreations/VixenToolBox/issues).

## How to report

**Please don't open a public issue for a security problem.** Report it privately instead:

* **GitHub (preferred):** [report a vulnerability](https://github.com/VixenCreations/VixenToolBox/security/advisories/new). Only you and the maintainers can see it.
* **Email:** [Vixenlicious@outlook.com](mailto:Vixenlicious@outlook.com), with "Security" in the subject.

Tell us which version you're on, what an attacker could do, and the steps or files we need to see it happen.

## What happens next

* We'll reply within a week to confirm we got your report.
* We'll keep you updated while we look into it, and tell you whether we accept it. If we don't, we'll explain why.
* Accepted problems get fixed in a new release. Once people have had time to update, we publish a GitHub security advisory and credit you in it, unless you'd rather stay anonymous.
* Please keep the details private until the fix is out. We'll agree a disclosure date with you.
