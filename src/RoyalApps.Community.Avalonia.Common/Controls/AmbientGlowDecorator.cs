using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Platform;

namespace RoyalApps.Community.Avalonia.Common.Controls;

/// <summary>Decorates content with a contained accent bloom and a traveling border highlight.</summary>
/// <remarks>
/// Use transparent child backgrounds to reveal the interior glow. Content is not clipped to the rounded corners,
/// but is clipped to the control's rectangular bounds by default. Set <see cref="Visual.ClipToBounds"/> to false to allow content overflow.
/// </remarks>
public sealed class AmbientGlowDecorator : ContentControl
{
    private AmbientGlowSurface? _surface;
    private Color _effectiveAmbientColor = Colors.DodgerBlue;

    // Avalonia's conversion follows custom variants' inheritance chains.
    internal bool IsDarkTheme => (PlatformThemeVariant?)ActualThemeVariant == PlatformThemeVariant.Dark;

    /// <summary>Creates a decoration whose theme supplies the default visual composition.</summary>
    public AmbientGlowDecorator()
    {
        ActualThemeVariantChanged += (_, _) =>
        {
            UpdateEffectiveAmbientColor();
            _surface?.Update();
        };
        UpdateEffectiveAmbientColor();
    }

    /// <summary>Identifies <see cref="AmbientColor"/>.</summary>
    public static readonly StyledProperty<Color> AmbientColorProperty =
        AvaloniaProperty.Register<AmbientGlowDecorator, Color>(nameof(AmbientColor), Colors.DodgerBlue);
    /// <summary>Identifies <see cref="AmbientColorLight"/>.</summary>
    public static readonly StyledProperty<Color?> AmbientColorLightProperty =
        AvaloniaProperty.Register<AmbientGlowDecorator, Color?>(nameof(AmbientColorLight));
    /// <summary>Identifies <see cref="AmbientColorDark"/>.</summary>
    public static readonly StyledProperty<Color?> AmbientColorDarkProperty =
        AvaloniaProperty.Register<AmbientGlowDecorator, Color?>(nameof(AmbientColorDark));
    /// <summary>Identifies the read-only <see cref="EffectiveAmbientColor"/> property.</summary>
    public static readonly DirectProperty<AmbientGlowDecorator, Color> EffectiveAmbientColorProperty =
        AvaloniaProperty.RegisterDirect<AmbientGlowDecorator, Color>(nameof(EffectiveAmbientColor), owner => owner.EffectiveAmbientColor);
    /// <summary>Identifies <see cref="IsAnimationEnabled"/>.</summary>
    public static readonly StyledProperty<bool> IsAnimationEnabledProperty =
        AvaloniaProperty.Register<AmbientGlowDecorator, bool>(nameof(IsAnimationEnabled), true);
    /// <summary>Identifies <see cref="IsMotionAllowed"/>.</summary>
    public static readonly StyledProperty<bool> IsMotionAllowedProperty =
        AvaloniaProperty.Register<AmbientGlowDecorator, bool>(nameof(IsMotionAllowed), true);

    /// <summary>Identifies <see cref="CycleDuration"/>.</summary>
    public static readonly StyledProperty<TimeSpan> CycleDurationProperty =
        AvaloniaProperty.Register<AmbientGlowDecorator, TimeSpan>(nameof(CycleDuration), TimeSpan.FromSeconds(9),
            validate: value => value > TimeSpan.Zero);
    /// <summary>Identifies <see cref="HighlightThickness"/>.</summary>
    public static readonly StyledProperty<double> HighlightThicknessProperty =
        AvaloniaProperty.Register<AmbientGlowDecorator, double>(nameof(HighlightThickness), 0.5,
            validate: value => double.IsFinite(value) && value > 0);
    /// <summary>Identifies <see cref="GlowOpacity"/>.</summary>
    public static readonly StyledProperty<double> GlowOpacityProperty =
        AvaloniaProperty.Register<AmbientGlowDecorator, double>(nameof(GlowOpacity), 1,
            validate: value => double.IsFinite(value) && value >= 0 && value <= 1);

    /// <summary>Gets or sets the fallback color for both themes. Semantic classes or the default theme supply it unless explicitly set.</summary>
    public Color AmbientColor { get => GetValue(AmbientColorProperty); set => SetValue(AmbientColorProperty, value); }
    /// <summary>Gets or sets the light-theme override. When the dark override is absent, its dark variant blends 25% toward white, preserving alpha.</summary>
    public Color? AmbientColorLight { get => GetValue(AmbientColorLightProperty); set => SetValue(AmbientColorLightProperty, value); }
    /// <summary>Gets or sets the dark-theme override. Null derives from <see cref="AmbientColorLight"/>, or falls back to <see cref="AmbientColor"/>.</summary>
    public Color? AmbientColorDark { get => GetValue(AmbientColorDarkProperty); set => SetValue(AmbientColorDarkProperty, value); }
    /// <summary>Gets the color used by the effect in the current theme. Bind accompanying tints to this property.</summary>
    public Color EffectiveAmbientColor
    {
        get => _effectiveAmbientColor;
        private set => SetAndRaise(EffectiveAmbientColorProperty, ref _effectiveAmbientColor, value);
    }
    /// <summary>Gets or sets local motion permission, also gated by <see cref="IsMotionAllowed"/>.</summary>
    public bool IsAnimationEnabled { get => GetValue(IsAnimationEnabledProperty); set => SetValue(IsAnimationEnabledProperty, value); }
    /// <summary>Gets or sets host motion permission, independently of local animation permission. Defaults to true.</summary>
    public bool IsMotionAllowed { get => GetValue(IsMotionAllowedProperty); set => SetValue(IsMotionAllowedProperty, value); }

    /// <summary>Gets or sets the positive duration of one clockwise revolution. Defaults to nine seconds.</summary>
    public TimeSpan CycleDuration { get => GetValue(CycleDurationProperty); set => SetValue(CycleDurationProperty, value); }
    /// <summary>Gets or sets the positive, finite highlight width in device-independent pixels. Defaults to 0.5.</summary>
    public double HighlightThickness { get => GetValue(HighlightThicknessProperty); set => SetValue(HighlightThicknessProperty, value); }
    /// <summary>Gets or sets the bloom and broad glow intensity from zero to one, without dimming the sharp highlight.</summary>
    public double GlowOpacity { get => GetValue(GlowOpacityProperty); set => SetValue(GlowOpacityProperty, value); }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _surface?.Disconnect();
        base.OnApplyTemplate(e);
        _surface = e.NameScope.Find<AmbientGlowSurface>("PART_GlowSurface");
        _surface?.Connect(this);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == AmbientColorProperty || change.Property == AmbientColorLightProperty || change.Property == AmbientColorDarkProperty)
        {
            UpdateEffectiveAmbientColor();
            _surface?.Update();
        }
        else if (change.Property == IsAnimationEnabledProperty
            || change.Property == CycleDurationProperty || change.Property == HighlightThicknessProperty
            || change.Property == GlowOpacityProperty || change.Property == CornerRadiusProperty
            || change.Property == IsEffectivelyEnabledProperty
            || change.Property == IsMotionAllowedProperty)
            _surface?.Update();
    }

    private void UpdateEffectiveAmbientColor()
    {
        EffectiveAmbientColor = IsDarkTheme
            ? AmbientColorDark ?? (AmbientColorLight is { } light ? light.ChangeBrightness(0.25F) : AmbientColor)
            : AmbientColorLight ?? AmbientColor;
    }
}
