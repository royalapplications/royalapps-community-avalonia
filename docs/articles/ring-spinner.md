# Ring spinner

`CompositionRingSpinner` is an indeterminate progress indicator in
`RoyalApps.Community.Avalonia.Common.Controls`. Its animation runs in a composition
visual, independently of UI dispatcher work. Avoid blocking the dispatcher in
application code: input and layout still need it.

## Standalone setup

Reference Common and include its optional standalone style in your application:

```xml
<StyleInclude Source="avares://RoyalApps.Community.Avalonia.Common/Controls/CompositionRingSpinner.Styles.axaml" />
```

Add the namespace and set a size in unbounded layouts:

```xml
<community:CompositionRingSpinner
    xmlns:community="clr-namespace:RoyalApps.Community.Avalonia.Common.Controls;assembly=RoyalApps.Community.Avalonia.Common"
    Width="24" Height="24"
    ForegroundColor="DodgerBlue" TrackColor="#30606060"
    StrokeThickness="2" IsActive="True" />
```

The control measures to the smaller available dimension. When both dimensions
are unbounded its desired size is zero. The ring remains centered in rectangular
bounds and uses their smaller dimension as its diameter.

## Styling and animation

| Property | Default | Behavior |
| --- | --- | --- |
| `IsActive` | `true` | Shows and animates the foreground arc |
| `ForegroundColor` | `DodgerBlue` | Color of the animated arc |
| `TrackColor` | `Transparent` | Color of the stationary full ring |
| `StrokeThickness` | `2` | Finite DIP width, clamped to fit the ring when drawing |

When inactive, only the track is drawn. Animation is also suspended when the
control or an ancestor is hidden, its bounds are empty, or it is detached.
Reattachment creates a fresh composition visual. The root rotation takes three
seconds; the arc grows and shrinks between 10 and 280 degrees over 1.2 seconds.

The spinner does not read application-wide or operating-system motion settings.
Bind `IsActive` to the state your application intends to display. RoyalConnect's
UI.Core style supplies its dynamic application accent while standalone consumers
retain DodgerBlue or their own explicit color.

Nonfinite stroke widths are rejected. Finite widths below one DIP are normally
drawn at one DIP; bounds smaller than two DIPs reduce that minimum to half the
available diameter. Large widths are clamped to half the diameter.

See the generated [API reference](/api/) for the complete public contract.
