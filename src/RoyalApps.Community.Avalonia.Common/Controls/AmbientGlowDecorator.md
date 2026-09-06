# Ambient glow decoration

`AmbientGlowDecorator` decorates content with a contained accent bloom and a traveling border highlight. It requires only Avalonia.

Include the theme in your application's styles:

```xml
<StyleInclude Source="avares://RoyalApps.Community.Avalonia.Common/Controls/AmbientGlowDecorator.Styles.axaml" />
```

Declare `xmlns:common="clr-namespace:RoyalApps.Community.Avalonia.Common.Controls;assembly=RoyalApps.Community.Avalonia.Common"` in the consuming view, then wrap content:

```xml
<common:AmbientGlowDecorator AmbientColor="DodgerBlue" Padding="12" CornerRadius="8">
    <ContentControl Content="{Binding Content}" />
</common:AmbientGlowDecorator>
```

Give binding scopes their normal explicit `x:DataType`. Transparent child backgrounds reveal the interior glow. Content is not clipped to the rounded corners, but is clipped to the control's rectangular bounds by default. Set `ClipToBounds="False"` on the decorator to allow content overflow; ancestor clipping still applies. Standalone defaults are DodgerBlue and an 8-DIP corner radius. The control is not focusable and leaves content input and focus intact.

## Color and animation

`AmbientColorLight` and `AmbientColorDark` are optional overrides. Light mode falls back to `AmbientColor`. Dark mode uses the explicit dark override, or blends the light override 25% toward white while preserving alpha, or falls back to `AmbientColor`. `EffectiveAmbientColor` is the observable, read-only resolved color and can drive accompanying tints.

`CycleDuration` defaults to nine seconds and must be positive. `HighlightThickness` defaults to 0.5 DIP and must be positive and finite. `GlowOpacity` ranges from zero to one, defaults to one, and does not dim the sharp highlight.

`IsAnimationEnabled` and `IsMotionAllowed` both default to true. Motion requires both permissions, effective visibility, effective enabled state, attachment, and nonempty bounds. Hosts can bind `IsMotionAllowed` to their global motion preference independently of local `IsAnimationEnabled`. Without composition, the control draws a stationary fallback without a UI-thread timer. Detachment stops the compositor and disposes ancestor visibility subscriptions.

## RoyalConnect integration

Desktop's integration style includes this theme, supplies Actipro's button corner radius and dynamic application accent, and supports the `accent`, `success`, `warning`, and `danger` classes. Explicit local color values take precedence over those styles. The integration binds `IsMotionAllowed` to inherited `UIGlobal.AnimationsEnabled`.

## Verification

Run the focused suite from the Community.Avalonia submodule root:

```powershell
dotnet test --project src/RoyalApps.Community.Avalonia.Common.Tests/RoyalApps.Community.Avalonia.Common.Tests.csproj --no-restore -- --filter-class '*AmbientGlow*'
```

The rendering test invokes the same immediate drawing routine as the compositor handler on a real Skia canvas, without changing the suite's headless configuration. A test adapter is necessary because normal offscreen control traversal does not include child composition visuals. It writes light/dark galleries at 1x, 1.5x, and 2x scale under `ambient-glow-verification` beside the test executable. Rows show top, right, bottom, and left highlight positions; columns show wide, tall, and square bounds. Wide and square examples derive their dark color automatically; tall examples use an explicit dark color. These decoration-only images avoid the headless host's placeholder font renderer; content input and layout are checked separately.

The generated `profile.txt` records a warmed-up, 120-frame CPU drawing probe for one and twenty instances, including Avalonia/Skia drawing allocations, and checks that hiding their parent stops every animation controller. This is an offscreen rendering measurement, not a desktop GPU frame-rate guarantee. Immutable gradient stops and geometry inputs are cached per handler. Each changing frame creates three immutable brush wrappers and two pens, reusing the stop arrays; no application-owned collections or callbacks are created per frame. This avoids accessing thread-affine Avalonia brushes from the render thread. Avalonia/Skia drawing also allocates internally.

The compositor integration test commits a real custom visual, drives the headless compositor from a worker while the UI dispatcher is blocked, and checks phase progress and stop/detach behavior. This establishes independence from UI dispatch; it does not benchmark a native desktop GPU. The Community GC and Desktop input/layout tests cover detached control collection and unchanged content behavior.
