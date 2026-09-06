using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using RoyalApps.Community.Avalonia.Common.Controls;
using Xunit;

namespace RoyalApps.Community.Avalonia.Common.Tests.Controls;

public sealed class AmbientGlowDecoratorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(2.25, 90)]
    [InlineData(4.5, 180)]
    [InlineData(6.75, 270)]
    [InlineData(9, 0)]
    [InlineData(11.25, 90)]
    public void PhaseUsesElapsedTime(double seconds, double expected) =>
        Assert.Equal(expected, AmbientGlowAnimation.AngleAt(TimeSpan.FromSeconds(seconds), TimeSpan.FromSeconds(9)));

    [Fact]
    public void StopsDrainQueuedTicksAndRestartWithoutDuplicateLoops()
    {
        var animation = new AmbientGlowAnimation();
        var duration = TimeSpan.FromSeconds(9);
        Assert.True(animation.Configure(true, duration));
        Assert.False(animation.Configure(true, duration));
        Assert.True(animation.Tick(TimeSpan.Zero));
        Assert.True(animation.Tick(TimeSpan.FromSeconds(2.25)));
        Assert.Equal(90, animation.Angle);
        Assert.False(animation.Configure(false, duration));
        Assert.Equal(0, animation.Angle);
        // Reuse the still-pending tick rather than scheduling a second chain.
        Assert.False(animation.Configure(true, duration));
        Assert.True(animation.Tick(TimeSpan.FromSeconds(30)));
        Assert.Equal(0, animation.Angle);
        Assert.False(animation.Configure(false, duration));
        Assert.False(animation.Tick(TimeSpan.FromSeconds(31)));
        Assert.True(animation.Configure(true, duration));
        Assert.True(animation.Tick(TimeSpan.FromSeconds(40)));
        Assert.False(animation.Configure(true, TimeSpan.FromSeconds(6)));
        Assert.True(animation.Tick(TimeSpan.FromSeconds(50)));
        Assert.Equal(0, animation.Angle);
        Assert.True(animation.Tick(TimeSpan.FromSeconds(51.5)));
        Assert.Equal(90, animation.Angle);
    }
    [AvaloniaFact]
    public void PropertiesRejectInvalidValues()
    {
        var decoration = new AmbientGlowDecorator();
        Assert.Equal(TimeSpan.FromSeconds(9), decoration.CycleDuration);
        Assert.Equal(0.5, decoration.HighlightThickness);
        Assert.Equal(1, decoration.GlowOpacity);
        Assert.True(decoration.IsAnimationEnabled);
        Assert.True(decoration.IsMotionAllowed);
        Assert.Throws<ArgumentException>(() => decoration.CycleDuration = TimeSpan.Zero);
        Assert.Throws<ArgumentException>(() => decoration.CycleDuration = TimeSpan.FromSeconds(-1));
        foreach (var value in new[] { 0d, -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Assert.Throws<ArgumentException>(() => decoration.HighlightThickness = value);
        foreach (var value in new[] { -1d, 2, double.NaN, double.PositiveInfinity })
            Assert.Throws<ArgumentException>(() => decoration.GlowOpacity = value);
        decoration.GlowOpacity = 0;
    }

    [AvaloniaFact]
    public void VisibilitySettingsSizeAndAttachmentGateMotion()
    {
        var decoration = new AmbientGlowDecorator { Width = 240, Height = 60 };
        var parent = new Border { Child = decoration };
        var window = Show(parent);
        try
        {
            var surface = Surface(decoration);
            Assert.True(surface.IsAnimationRunning);
            parent.IsVisible = false;
            Assert.False(surface.IsAnimationRunning);
            parent.IsVisible = true;
            Assert.True(surface.IsAnimationRunning);
            decoration.IsMotionAllowed = false;
            Assert.False(surface.IsAnimationRunning);
            decoration.IsAnimationEnabled = false;
            decoration.IsAnimationEnabled = true;
            Assert.False(surface.IsAnimationRunning);
            decoration.IsMotionAllowed = true;
            Assert.True(surface.IsAnimationRunning);
            decoration.IsAnimationEnabled = false;
            Assert.False(surface.IsAnimationRunning);
            decoration.IsAnimationEnabled = true;
            parent.IsEnabled = false;
            Assert.False(surface.IsAnimationRunning);
            parent.IsEnabled = true;
            Assert.True(surface.IsAnimationRunning);
            decoration.Width = 0;
            window.UpdateLayout();
            Assert.False(surface.IsAnimationRunning);
            decoration.Width = 240;
            window.UpdateLayout();
            Assert.True(surface.IsAnimationRunning);
            var subscriptionCount = surface.VisibilitySubscriptionCount;
            Assert.True(subscriptionCount > 0);
            parent.Child = null;
            Assert.False(surface.IsAnimationRunning);
            Assert.Equal(0, surface.VisibilitySubscriptionCount);
            parent.Child = decoration;
            window.UpdateLayout();
            Assert.True(surface.IsAnimationRunning);
            Assert.Equal(subscriptionCount, surface.VisibilitySubscriptionCount);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void StandaloneThemeSuppliesDefaultsWithoutApplicationResources()
    {
        var decoration = new AmbientGlowDecorator { Width = 200, Height = 60, IsAnimationEnabled = false };
        var window = Show(decoration);
        try
        {
            Assert.Equal(Colors.DodgerBlue, decoration.AmbientColor);
            Assert.Equal(new CornerRadius(8), decoration.CornerRadius);
            Assert.False(decoration.Focusable);
            Assert.False(Surface(decoration).IsHitTestVisible);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ThemeOverridesUpdateResolvedColorAndBoundTint()
    {
        var decoration = new AmbientGlowDecorator
        {
            AmbientColor = Colors.Purple, AmbientColorLight = Colors.ForestGreen,
            AmbientColorDark = Colors.LightGreen, IsAnimationEnabled = false
        };
        var tint = new SolidColorBrush();
        using var binding = tint.Bind(SolidColorBrush.ColorProperty,
            decoration.GetObservable(AmbientGlowDecorator.EffectiveAmbientColorProperty));
        var window = Show(decoration);
        try
        {
            window.RequestedThemeVariant = ThemeVariant.Light;
            Assert.Equal(Colors.ForestGreen, decoration.EffectiveAmbientColor);
            Assert.Equal(Colors.ForestGreen, tint.Color);
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Assert.Equal(Colors.LightGreen, decoration.EffectiveAmbientColor);
            Assert.Equal(Colors.LightGreen, tint.Color);
            decoration.AmbientColorDark = Colors.Lime;
            Assert.Equal(Colors.Lime, tint.Color);
            decoration.AmbientColorLight = null;
            window.RequestedThemeVariant = ThemeVariant.Light;
            Assert.Equal(Colors.Purple, tint.Color);
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Assert.Equal(Colors.Lime, tint.Color);
            decoration.ClearValue(AmbientGlowDecorator.AmbientColorDarkProperty);
            Assert.Equal(Colors.Purple, tint.Color);
            decoration.AmbientColor = Colors.Orange;
            Assert.Equal(Colors.Orange, tint.Color);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(1)]
    [InlineData(2)]
    public void InheritedThemeVariantsUpdateColorsAndRendering(int inheritanceDepth)
    {
        var dark = ThemeVariant.Dark;
        var light = ThemeVariant.Light;
        for (var i = 0; i < inheritanceDepth; i++)
        {
            dark = new ThemeVariant($"CustomDark{i}", dark);
            light = new ThemeVariant($"CustomLight{i}", light);
        }

        var decoration = new AmbientGlowDecorator
        {
            Width = 200, Height = 60, IsAnimationEnabled = false,
            AmbientColorLight = Color.Parse("#800080FF"), AmbientColorDark = Colors.Lime
        };
        var window = Show(decoration);
        try
        {
            var surface = Surface(decoration);
            window.RequestedThemeVariant = dark;
            Assert.Equal(Colors.Lime, decoration.EffectiveAmbientColor);
            Assert.Equal(Colors.Lime, surface.VisualState!.Value.Color);
            Assert.True(surface.VisualState.Value.IsDark);

            decoration.AmbientColorDark = null;
            Assert.Equal(Color.Parse("#803F9FFF"), decoration.EffectiveAmbientColor);
            Assert.Equal(decoration.EffectiveAmbientColor, surface.VisualState.Value.Color);
            Assert.True(surface.VisualState.Value.IsDark);

            window.RequestedThemeVariant = light;
            Assert.Equal(Color.Parse("#800080FF"), decoration.EffectiveAmbientColor);
            Assert.Equal(decoration.EffectiveAmbientColor, surface.VisualState.Value.Color);
            Assert.False(surface.VisualState.Value.IsDark);

            window.RequestedThemeVariant = dark;
            Assert.Equal(Color.Parse("#803F9FFF"), decoration.EffectiveAmbientColor);
            Assert.Equal(decoration.EffectiveAmbientColor, surface.VisualState.Value.Color);
            Assert.True(surface.VisualState.Value.IsDark);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData("#800080FF", "#803F9FFF")]
    [InlineData("#FF000000", "#FF3F3F3F")]
    [InlineData("#FFFFFFFF", "#FFFFFFFF")]
    [InlineData("#000080FF", "#003F9FFF")]
    public void OmittedDarkColorIsDerivedFromLightAndPreservesAlpha(string light, string dark)
    {
        var decoration = new AmbientGlowDecorator { AmbientColorLight = Color.Parse(light), IsAnimationEnabled = false };
        decoration.Classes.Add("success");
        var window = Show(decoration);
        try
        {
            Assert.Null(decoration.AmbientColorDark);
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Assert.Equal(Color.Parse(dark), decoration.EffectiveAmbientColor);
            window.RequestedThemeVariant = ThemeVariant.Light;
            Assert.Equal(Color.Parse(light), decoration.EffectiveAmbientColor);
            decoration.AmbientColorLight = Colors.Black;
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Assert.Equal(Color.Parse("#FF3F3F3F"), decoration.EffectiveAmbientColor);
            decoration.ClearValue(AmbientGlowDecorator.AmbientColorLightProperty);
            Assert.Equal(decoration.AmbientColor, decoration.EffectiveAmbientColor);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void DetachedDecorationIsCollectibleAfterPendingFramesDrain()
    {
        var window = Show(new Border());
        try
        {
            var weak = AttachAndRemove(window);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.False(weak.IsAlive);
        }
        finally { window.Close(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachAndRemove(Window window)
    {
        var decoration = new AmbientGlowDecorator { Width = 200, Height = 60 };
        window.Content = decoration;
        window.UpdateLayout();
        var weak = new WeakReference(decoration);
        window.Content = null;
        window.UpdateLayout();
        return weak;
    }

    internal static AmbientGlowSurface Surface(AmbientGlowDecorator decoration)
    {
        var surface = decoration.GetVisualDescendants().OfType<AmbientGlowSurface>().SingleOrDefault();
        Assert.NotNull(surface);
        return surface!;
    }

    internal static Window Show(Control content, double width = 500, double height = 400)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        const string source = "avares://RoyalApps.Community.Avalonia.Common/Controls/AmbientGlowDecorator.Styles.axaml";
        var application = Application.Current!;
        if (!application.Styles.OfType<StyleInclude>().Any(style => style.Source?.ToString() == source))
            application.Styles.Add(new StyleInclude(new Uri("avares://RoyalApps.Community.Avalonia.Common.Tests/")) { Source = new Uri(source) });
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
