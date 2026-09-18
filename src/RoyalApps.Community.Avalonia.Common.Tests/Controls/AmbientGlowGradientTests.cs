using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using RoyalApps.Community.Avalonia.Common.Controls;
using SkiaSharp;
using Xunit;

namespace RoyalApps.Community.Avalonia.Common.Tests.Controls;

public sealed class AmbientGlowGradientTests
{
    [AvaloniaFact]
    public void CustomStopsDoNotAddPerFrameDrawingAllocations()
    {
        var state = new AmbientGlowVisualState(new Size(180, 36), new CornerRadius(18), Colors.Blue,
            false, 0.5, 1, TimeSpan.FromSeconds(9), true);
        var stops = AmbientGlowGradientObserver.CreateSnapshot(new GradientStops
        {
            new(Colors.Transparent, 0), new(Colors.Cyan, 0.5), new(Colors.Transparent, 1)
        });
        var normal = new AmbientGlowDrawing();
        normal.Update(state);
        var custom = new AmbientGlowDrawing();
        custom.Update(state with { HighlightGradientStops = stops, GlowGradientStops = stops });
        static long Measure(AmbientGlowDrawing drawing)
        {
            for (var i = 0; i < 50; i++) drawing.SetAngle(i);
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 120; i++) drawing.SetAngle(i * 3);
            return GC.GetAllocatedBytesForCurrentThread() - before;
        }
        Assert.Equal(Measure(normal), Measure(custom));
    }

    [AvaloniaFact]
    public void DefaultsAndOpacityValidationPreserveExistingAppearance()
    {
        var glow = new AmbientGlowDecorator();
        Assert.False(glow.InvertBorderGradient);
        Assert.Null(glow.HighlightGradientStops);
        Assert.Null(glow.GlowGradientStops);
        Assert.Equal(1, glow.HighlightOpacity);
        foreach (var value in new[] { -1d, 1.1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Assert.Throws<ArgumentException>(() => glow.HighlightOpacity = value);
        glow.HighlightOpacity = 0;
        glow.HighlightOpacity = 1;
    }

    [AvaloniaFact]
    public void SnapshotsNormalizeOffsetsWithoutChangingSharedSource()
    {
        var stops = new GradientStops
        {
            new(Colors.Red, 2), new(Colors.Blue, -1), new(Colors.Green, 0.5),
            new(Colors.Yellow, 0.5), new(Colors.Black, double.NaN)
        };
        var snapshot = AmbientGlowGradientObserver.CreateSnapshot(stops)!;
        Assert.Equal(new[] { 0d, 0.5, 0.5, 1 }, snapshot.Select(stop => stop.Offset));
        Assert.Equal(new[] { Colors.Blue, Colors.Green, Colors.Yellow, Colors.Red }, snapshot.Select(stop => stop.Color));
        stops[0].Color = Colors.Purple;
        Assert.Equal(Colors.Red, snapshot[3].Color);
        Assert.Equal(2, stops[0].Offset);
        Assert.Null(AmbientGlowGradientObserver.CreateSnapshot(null));
        Assert.Null(AmbientGlowGradientObserver.CreateSnapshot(new GradientStops()));
        Assert.Null(AmbientGlowGradientObserver.CreateSnapshot(new GradientStops { new(Colors.Red, double.PositiveInfinity) }));
        Assert.Single(AmbientGlowGradientObserver.CreateSnapshot(new GradientStops { new(Colors.Red, 0.5) })!);
    }

    [AvaloniaFact]
    public void ObserverTracksEditsReplacementResetAndDuplicateStopsWithoutRetention()
    {
        var changes = 0;
        using var observer = new AmbientGlowGradientObserver(() => changes++);
        var stop = new GradientStop(Colors.Red, 0);
        var source = new GradientStops { stop, stop };
        observer.SetSource(source);
        var snapshot = observer.Snapshot;
        observer.SetSource(source);
        Assert.Same(snapshot, observer.Snapshot);
        stop.Color = Colors.Blue;
        Assert.Equal(1, changes);
        Assert.Equal(Colors.Red, snapshot![0].Color);
        Assert.Equal(Colors.Blue, observer.Snapshot![0].Color);
        stop.Offset = 0.4;
        Assert.Equal(2, changes);
        source.RemoveAt(0);
        stop.Color = Colors.Green;
        Assert.Equal(4, changes);
        source.Clear();
        Assert.Null(observer.Snapshot);
        stop.Color = Colors.Yellow;
        Assert.Equal(5, changes);
        var replacement = new GradientStops { stop };
        observer.SetSource(replacement);
        source.Add(new GradientStop(Colors.Black, 1));
        Assert.Equal(5, changes);
        observer.Dispose();
        replacement.Add(new GradientStop(Colors.White, 1));
        stop.Color = Colors.Black;
        Assert.Equal(5, changes);
    }

    [AvaloniaFact]
    public void SharedStopsUpdateEachOwnerAndRefreshAfterReattachment()
    {
        var stops = new GradientStops { new(Colors.Red, 0) };
        var first = new AmbientGlowDecorator { Width = 120, Height = 40, HighlightGradientStops = stops };
        var second = new AmbientGlowDecorator { Width = 120, Height = 40, GlowGradientStops = stops };
        var panel = new StackPanel();
        panel.Children.Add(first);
        panel.Children.Add(second);
        var window = AmbientGlowDecoratorTests.Show(panel);
        try
        {
            var oldSnapshot = AmbientGlowDecoratorTests.Surface(first).VisualState!.Value.HighlightGradientStops;
            stops[0].Color = Colors.Blue;
            Assert.Equal(Colors.Blue, AmbientGlowDecoratorTests.Surface(first).VisualState!.Value.HighlightGradientStops![0].Color);
            Assert.Equal(Colors.Blue, AmbientGlowDecoratorTests.Surface(second).VisualState!.Value.GlowGradientStops![0].Color);
            Assert.Equal(Colors.Red, oldSnapshot![0].Color);
            panel.Children.Remove(first);
            stops[0].Color = Colors.Green;
            panel.Children.Add(first);
            window.UpdateLayout();
            Assert.Equal(Colors.Green, AmbientGlowDecoratorTests.Surface(first).VisualState!.Value.HighlightGradientStops![0].Color);
            first.HighlightGradientStops = null;
            Assert.Null(AmbientGlowDecoratorTests.Surface(first).VisualState!.Value.HighlightGradientStops);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ThemeResourcesCanReplaceGradientsAndInversionAtRuntime()
    {
        var lightStops = new GradientStops { new(Colors.Red, 0) };
        var darkStops = new GradientStops { new(Colors.Blue, 0) };
        var glow = new AmbientGlowDecorator { Width = 120, Height = 40 };
        glow.Resources.ThemeDictionaries[ThemeVariant.Light] = new ResourceDictionary { ["Stops"] = lightStops, ["Invert"] = true };
        glow.Resources.ThemeDictionaries[ThemeVariant.Dark] = new ResourceDictionary { ["Stops"] = darkStops, ["Invert"] = false };
        using var stopsBinding = glow.Bind(AmbientGlowDecorator.HighlightGradientStopsProperty, glow.GetResourceObservable("Stops"));
        using var invertBinding = glow.Bind(AmbientGlowDecorator.InvertBorderGradientProperty, glow.GetResourceObservable("Invert"));
        var window = AmbientGlowDecoratorTests.Show(glow);
        try
        {
            foreach (var theme in new[] { ThemeVariant.Light, new ThemeVariant("CustomDark", ThemeVariant.Dark), ThemeVariant.Light })
            {
                window.RequestedThemeVariant = theme;
                Dispatcher.UIThread.RunJobs();
                var state = AmbientGlowDecoratorTests.Surface(glow).VisualState!.Value;
                Assert.Equal(theme == ThemeVariant.Light, state.InvertBorderGradient);
                Assert.Equal(theme == ThemeVariant.Light ? Colors.Red : Colors.Blue, state.HighlightGradientStops![0].Color);
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void SharedResourceDoesNotRetainDetachedControl()
    {
        var stops = new GradientStops { new(Colors.Red, 0) };
        var window = AmbientGlowDecoratorTests.Show(new Border());
        try
        {
            var weak = AttachAndRemove(window, stops);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Assert.False(weak.IsAlive);
            GC.KeepAlive(stops);
        }
        finally { window.Close(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachAndRemove(Window window, GradientStops stops)
    {
        var glow = new AmbientGlowDecorator { Width = 120, Height = 40, HighlightGradientStops = stops, GlowGradientStops = stops };
        window.Content = glow;
        window.UpdateLayout();
        var weak = new WeakReference(glow);
        window.Content = null;
        window.UpdateLayout();
        return weak;
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void RenderingHonorsInversionCustomPrecedenceAndIndependentOpacity(bool dark)
    {
        var glow = new AmbientGlowDecorator { Width = 180, Height = 36, CornerRadius = new CornerRadius(18), IsAnimationEnabled = false };
        var window = AmbientGlowDecoratorTests.Show(glow, 220, 76);
        window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        window.Background = dark ? Brushes.Black : Brushes.White;
        try
        {
            var probe = new GlowRenderProbe(window);
            using var bitmap = new SKBitmap(220, 76);
            using var canvas = new SKCanvas(bitmap);
            SKColor[] Render()
            {
                canvas.Clear();
                probe.RenderTo(canvas);
                return bitmap.Pixels;
            }
            var original = Render();
            glow.InvertBorderGradient = true;
            Assert.False(original.SequenceEqual(Render()));
            glow.InvertBorderGradient = false;
            Assert.Equal(original, Render());
            glow.HighlightGradientStops = new GradientStops();
            glow.GlowGradientStops = new GradientStops();
            Assert.Equal(original, Render());
            glow.HighlightGradientStops = new GradientStops { new(Colors.Red, 0), new(Colors.Blue, 0.5), new(Colors.Red, 1) };
            glow.GlowGradientStops = new GradientStops { new(Colors.Lime, 0) };
            var custom = Render();
            glow.InvertBorderGradient = true;
            Assert.Equal(custom, Render());
            probe.UseImmediateDrawing = false;
            var fallback = Render();
            var difference = 0L;
            for (var i = 0; i < custom.Length; i++)
                difference += Math.Abs(custom[i].Red - fallback[i].Red) + Math.Abs(custom[i].Green - fallback[i].Green)
                    + Math.Abs(custom[i].Blue - fallback[i].Blue);
            // Same sub-channel average tolerance as the existing immediate/fallback rendering test.
            Assert.True(difference / (double)(custom.Length * 3) < 1);
            probe.UseImmediateDrawing = true;
            glow.HighlightOpacity = 0;
            Assert.False(custom.SequenceEqual(Render()));
            glow.GlowGradientStops = null;
            var generatedGlow = Render();
            glow.InvertBorderGradient = false;
            Assert.False(generatedGlow.SequenceEqual(Render()));
            glow.GlowOpacity = 0;
            var noHighlight = Render();
            glow.HighlightOpacity = 1;
            Assert.False(noHighlight.SequenceEqual(Render()));
        }
        finally { window.Close(); }
    }
}
