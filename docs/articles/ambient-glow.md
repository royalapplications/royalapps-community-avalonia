# Ambient glow decoration

`AmbientGlowDecorator` decorates content with a contained accent bloom and a traveling border highlight. It requires only Avalonia.

<DemoMedia name="ambient-glow" alt="The ambient glow sample with live color, motion, and intensity controls" />

::: details Watch the glow animation

<DemoMedia name="ambient-glow" alt="Card, button, and pill-shaped badge animations continue while the UI thread is blocked; the standard Avalonia progress bar pauses." animated />

Card, button, and pill-shaped badge animations continue while the UI thread is blocked; the standard Avalonia progress bar pauses. The dark recording has a reduced capture rate during the blocked interval; use the live sample to inspect continuous motion.

:::

## Try the sample

The gallery includes a content card, a glow-backed button, and a pill-shaped badge. All three share the color and motion controls under **Properties**.

Choose **Block UI thread for 5 seconds** to compare them with a standard indeterminate Avalonia `ProgressBar`. After a short preparation delay, the sample intentionally blocks the dispatcher. The progress bar and input pause, while the compositor-driven glow continues. The UI recovers automatically after five seconds. This deliberate blocking is demonstration code, not an application pattern.

## Standalone setup

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

## Example

```xml
<common:AmbientGlowDecorator AmbientColorLight="#16863C"
                            AmbientColorDark="#57C879"
                            Padding="16" CornerRadius="12"
                            IsAnimationEnabled="False">
    <ContentControl Content="{Binding Content}" />
</common:AmbientGlowDecorator>
```

Omit `AmbientColorDark` to derive it from the light color. Set `IsAnimationEnabled="False"` for a stationary decoration. A host can bind `IsMotionAllowed` independently to its global motion preference.

See the [API reference](../api/reference/royalapps-community-avalonia-common-controls-ambientglowdecorator) for defaults and validation rules.
