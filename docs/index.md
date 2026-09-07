---
layout: home
hero:
  name: RoyalApps Avalonia
  text: Small controls. Lasting state.
  tagline: Reusable controls and behaviors for Avalonia, with Windows Forms hosting that keeps control state across view changes.
  image:
    src: /assets/RoyalApps_1024.png
    alt: Royal Apps
  actions:
    - theme: brand
      text: Getting started
      link: /articles/getting-started
    - theme: alt
      text: API reference
      link: /api/
    - theme: alt
      text: GitHub
      link: https://github.com/royalapplications/royalapps-community-avalonia
features:
  - title: Composition ring spinner
    details: Display an indeterminate ring with compositor-driven animation and an optional stationary track.
    link: /articles/ring-spinner
  - title: Ambient glow
    details: Add a contained bloom and traveling border highlight, with theme-aware colors and independent motion permissions.
    link: /articles/ambient-glow
  - title: Balanced layouts
    details: Let users double-tap a GridSplitter to equalize adjacent star-sized rows or columns while respecting size constraints.
    link: /articles/grid-splitter
  - title: Persistent WinForms controls
    details: Preserve native control instances across Avalonia view detachment and dispose them when their owning view model closes.
    link: /articles/winforms-hosting
---

## Explore the component browser

<DemoMedia name="sample-browser" alt="The sample browser with sidebar navigation and an interactive ring spinner preview" />

See the [ambient glow](./articles/ambient-glow), [ring spinner](./articles/ring-spinner), [GridSplitter](./articles/grid-splitter), and [WinForms hosting](./articles/winforms-hosting) pages for screenshots, animations, and usage examples.

## Two libraries, focused responsibilities

| Library | Purpose | Platform |
| --- | --- | --- |
| `RoyalApps.Community.Avalonia.Common` | Controls and behaviors | Cross-platform Avalonia |
| `RoyalApps.Community.Avalonia.Windows` | Windows Forms hosting | Windows |

These pages describe the current source tree. Check the [support matrix](./articles/support-matrix) before selecting a package version.
