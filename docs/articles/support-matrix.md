# Support matrix

This table describes the current source tree, not every previously published package.

| Capability | Common | Windows |
| --- | --- | --- |
| Target framework | `net10.0` | `net10.0-windows` |
| Avalonia version | 12.1.2 | 12.1.2 |
| Ambient glow decorator | Yes | Use Common |
| Composition ring spinner | Yes | Use Common |
| GridSplitter equalization | Yes | Use Common |
| WinForms hosting | No | Yes |
| Windows Forms dependency | No | Yes |
| RoyalConnect or Actipro dependency | No | No |

The Common library uses Avalonia's cross-platform APIs. Native rendering behavior should still be verified on each platform your application supports. The Windows library requires Windows and is not suitable for browser or mobile applications.

## Motion and rendering

Ambient glow uses a composition visual when available. Without composition, it renders a stationary fallback. Hosts control reduced-motion behavior through `IsMotionAllowed`; the library does not read operating-system motion preferences itself.

## Source and package versions

Common and Windows share release version `1.3.0`, defined in `src/Directory.Build.props`. Successful package workflow runs on `main` publish new versions of both packages to NuGet.org; already published versions are skipped. The spinner uses public Avalonia drawing APIs and introduces no direct Skia or Reactive dependency to Common.

Align Avalonia dependencies across consuming projects. A library compiled against a newer Avalonia assembly cannot necessarily be consumed by an application referencing an older version. Verify package metadata before upgrading an existing application.
