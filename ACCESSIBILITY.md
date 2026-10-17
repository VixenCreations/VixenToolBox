# Accessibility

We want everyone who builds for VRChat to be able to use the Vixens Toolbox, read its website and help make it better. This page covers what we focus on, what we know doesn't work well yet, and how to tell us when something gets in your way.

Hit a barrier and just want it fixed? Skip to [Reporting accessibility issues](#reporting-accessibility-issues).

## Priorities

* **Plain words.** Buttons name the action, like **Scan Avatar**, and tooltips and docs say what a control does in everyday language.
* **More than color.** A problem a tool finds should always be described in words, never shown only as a red or yellow highlight.
* **Unity's own settings keep working.** Tool windows should stay usable with the editor's UI Scaling turned up, and with the keyboard wherever Unity allows it.
* **A readable website.** Text for every image, a layout that works on phones, visible keyboard focus, and respect for your reduced-motion setting. We use WCAG 2.2 level AA as a guide for the site. That's a goal we work toward, not a tested conformance claim.

## Contributor expectations

If your change touches something people see, please:

* Give every control a text label. Icon-only buttons need a tooltip.
* Pair color with text or an icon, so the meaning still comes across for people who can't tell the colors apart.
* Build tool windows from Unity's standard controls (IMGUI or UI Toolkit) rather than custom-drawn ones, so keyboard focus and UI Scaling keep working.
* On the website, use real `<button>` and `<a>` elements, give every image `alt` text, keep text contrast readable, and put motion behind `prefers-reduced-motion`.
* Say in your pull request what you checked, for example "tabbed through the page", "200% browser zoom" or "Unity UI Scaling at 150%".

There's no automated accessibility check in our builds yet, so this review is done by hand.

## Reporting accessibility issues

[Open an issue](https://github.com/VixenCreations/VixenToolBox/issues/new) and put **Accessibility** in the title. If GitHub is hard to use for you, email [Vixenlicious@outlook.com](mailto:Vixenlicious@outlook.com) or ask on our [Discord](https://discord.com/invite/3vbJCKcPtJ) instead.

It helps to know:

* what you were trying to do, and which tool or web page you were on
* what happened instead
* your Unity version or browser, and your operating system
* any assistive technology or settings you use, such as a screen magnifier, high contrast or UI Scaling

Screenshots and recordings are welcome but never required, and you don't need to tell us anything about a disability.

### Severity

You don't need to pick one. We'll set it when we look at your report.

* **Blocking:** you can't finish the task at all. For example, a window hides its Apply button at your UI Scaling, or a web page can't be used with a keyboard.
* **Serious:** you can finish, but only with a lot of extra effort or someone's help.
* **Minor:** it gets in the way, but there's an easy workaround.

### How we respond

* We'll reply within a week to say we've seen it.
* Blocking issues go ahead of new features.
* If a fix will take a while, we'll suggest a workaround in the meantime.
* When a fix is ready, we'll ask if you'd like to try it before we close the issue. It ships in the next release, which you can get through the VRChat Creator Companion.

## Ownership and maintenance

The toolbox is maintained by VixForge Interactive. The maintainer, [@VixenCreations](https://github.com/VixenCreations), looks after accessibility along with everything else: sorting reports, reviewing changes against the expectations above, and keeping this page accurate. We review this page whenever we add a tool or change the website's layout, and at least once a year. If the maintainer changes, this page will name the new one.

## Supported environments

* **Tools:** the Unity 2022.3.22f1 editor with the VRChat SDK. They're built and tested on Windows, with a mouse and keyboard.
* **Website:** current versions of the major desktop and mobile browsers.
* **Assistive technology:** we haven't tested the tools or the site with screen readers, screen magnifiers or voice control yet. If you use one, we'd love to hear how it goes.

## Known limitations

* **Screen readers can't read the tools.** The Unity editor itself doesn't work with screen readers, so windows built inside it don't either. Every tool is described in text in [Details.md](Details.md) and on the [website](https://vixencreations.github.io/VixenToolBox/tools.html).
* **Some tools need a mouse.** Precision Click-to-Place works by clicking in the Scene view.
* **The website hasn't had a full audit.** What we've checked so far: every image in its pages has alt text, every page declares its language, and the home page plus the docs, changelog and news pages respect your reduced-motion setting.

## Feedback and improvements

Have an idea for how we could do better, or spotted something wrong on this page? Open an issue, or a pull request that edits this file. If something is stopping you from using the toolbox right now, use the [reporting steps above](#reporting-accessibility-issues) so it gets looked at first.
