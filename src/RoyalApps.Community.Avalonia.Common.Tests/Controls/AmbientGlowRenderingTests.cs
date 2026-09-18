using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Skia.Helpers;
using RoyalApps.Community.Avalonia.Common.Controls;
using Xunit;
using SkiaSharp;

namespace RoyalApps.Community.Avalonia.Common.Tests.Controls;

// Render directly into a real Skia canvas without changing the headless test configuration.
public sealed class AmbientGlowRenderingTests
{
    [AvaloniaFact]
    public void CompositorDrawingPreservesAsymmetricCornersAndMatchesStaticFallback()
    {
        var decoration = new AmbientGlowDecorator
        {
            Width = 240, Height = 48, CornerRadius = new CornerRadius(2, 12, 20, 6),
            IsAnimationEnabled = false, AmbientColor = Colors.ForestGreen
        };
        var window = AmbientGlowDecoratorTests.Show(decoration, 280, 88);
        window.Background = Brushes.White;
        window.RequestedThemeVariant = ThemeVariant.Light;
        try
        {
            var probe = new GlowRenderProbe(window);
            using var actual = new SKBitmap(280, 88);
            using var expected = new SKBitmap(280, 88);
            using var actualCanvas = new SKCanvas(actual);
            using var expectedCanvas = new SKCanvas(expected);
            foreach (var angle in new[] { 0d, 90, 180, 270 })
            {
                probe.SetAngle(0, angle);
                probe.UseImmediateDrawing = true;
                probe.RenderTo(actualCanvas);
                probe.UseImmediateDrawing = false;
                probe.RenderTo(expectedCanvas);
                var totalDifference = 0L;
                for (var y = 0; y < actual.Height; y++)
                for (var x = 0; x < actual.Width; x++)
                {
                    var a = actual.GetPixel(x, y);
                    var b = expected.GetPixel(x, y);
                    totalDifference += Math.Abs(a.Red - b.Red) + Math.Abs(a.Green - b.Green) + Math.Abs(a.Blue - b.Blue);
                }
                // Permit small rasterizer differences at the independently clipped corners.
                Assert.True((totalDifference / (double)(actual.Width * actual.Height * 3)) < 1);
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void GlowColorAndThemeChangesRepaintWithoutSpillingOutsideBounds()
    {
        var decoration = new AmbientGlowDecorator
        {
            Width = 240, Height = 48, CornerRadius = new CornerRadius(10),
            IsAnimationEnabled = false, AmbientColor = Colors.DodgerBlue
        };
        var window = AmbientGlowDecoratorTests.Show(decoration, 280, 88);
        window.Background = new SolidColorBrush(Color.Parse("#141C22"));
        window.RequestedThemeVariant = ThemeVariant.Dark;
        try
        {
            using var bitmap = new SKBitmap(280, 88);
            using var canvas = new SKCanvas(bitmap);
            var probe = new GlowRenderProbe(window);
            void Render() => probe.RenderTo(canvas);
            var origin = decoration.TranslatePoint(default, window)!.Value;
            var x = (int)origin.X + 120;
            var y = (int)origin.Y;
            Render();
            var bloom = bitmap.GetPixel(x, y + 4);
            var highlight = bitmap.GetPixel(x, y);
            Assert.Equal(new SKColor(20, 28, 34), bitmap.GetPixel((int)origin.X - 1, y + 10));
            Assert.Equal(new SKColor(20, 28, 34), bitmap.GetPixel((int)origin.X, y));
            decoration.GlowOpacity = 0;
            Render();
            Assert.NotEqual(bloom, bitmap.GetPixel(x, y + 4));
            decoration.GlowOpacity = 1;
            window.RequestedThemeVariant = ThemeVariant.Light;
            Render();
            Assert.NotEqual(highlight, bitmap.GetPixel(x, y));
            decoration.AmbientColor = Colors.Red;
            Render();
            Assert.True(bitmap.GetPixel(x, y + 4).Red > bitmap.GetPixel(x, y + 4).Blue);
            decoration.AmbientColorLight = Colors.Blue;
            decoration.AmbientColorDark = Colors.Lime;
            Render();
            Assert.True(bitmap.GetPixel(x, y + 4).Blue > bitmap.GetPixel(x, y + 4).Red);
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Render();
            Assert.True(bitmap.GetPixel(x, y + 4).Green > bitmap.GetPixel(x, y + 4).Blue);
            decoration.AmbientColorDark = null;
            Render();
            Assert.True(bitmap.GetPixel(x, y + 4).Blue > bitmap.GetPixel(x, y + 4).Green);
            decoration.CornerRadius = new CornerRadius(1000);
            decoration.HighlightThickness = 1000;
            decoration.Width = 1;
            decoration.Height = 1;
            window.UpdateLayout();
            Render();
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void RenderThemeDpiAndPhaseGalleryAndProfile()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "ambient-glow-verification");
        Directory.CreateDirectory(directory);
        var panel = new StackPanel { Margin = new Thickness(20), Spacing = 20 };
        var decorations = new List<AmbientGlowDecorator>();
        for (var row = 0; row < 4; row++)
        {
            var strip = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 24 };
            foreach (var size in new[] { new Size(240, 48), new Size(160, 100), new Size(64, 64) })
            {
                var decoration = new AmbientGlowDecorator
                {
                    Width = size.Width, Height = size.Height, CornerRadius = new CornerRadius(12),
                    AmbientColorLight = Color.Parse("#16863C"),
                    AmbientColorDark = size.Width == 160 ? Color.Parse("#57C879") : null,
                    IsAnimationEnabled = false
                };
                strip.Children.Add(decoration);
                decorations.Add(decoration);
            }
            panel.Children.Add(strip);
        }
        var window = AmbientGlowDecoratorTests.Show(panel, 560, 520);
        var probe = new GlowRenderProbe(window);
        try
        {
            foreach (var dark in new[] { true, false })
            {
                window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
                window.Background = new SolidColorBrush(Color.Parse(dark ? "#141C22" : "#F4F6F8"));
                window.UpdateLayout();
                for (var i = 0; i < decorations.Count; i++)
                    probe.SetAngle(i, i / 3 * 90);
                foreach (var scale in new[] { 1d, 1.5, 2d })
                {
                    using var bitmap = new SKBitmap((int)(560 * scale), (int)(520 * scale));
                    using var canvas = new SKCanvas(bitmap);
                    probe.RenderTo(canvas, scale);
                    var path = Path.Combine(directory, $"{(dark ? "dark" : "light")}-{scale:0.0}.png");
                    using (var image = SKImage.FromBitmap(bitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    using (var stream = File.Create(path)) data.SaveTo(stream);
                    using var decoded = SKBitmap.Decode(path);
                    Assert.Equal((int)(560 * scale), decoded.Width);
                    // The top highlight must differ from the static wash at the card center.
                    var origin = decorations[0].TranslatePoint(default, window)!.Value;
                    Assert.NotEqual(decoded.GetPixel((int)((origin.X + 120) * scale), (int)((origin.Y + 24) * scale)), decoded.GetPixel((int)((origin.X + 120) * scale), (int)(origin.Y * scale)));
                }
            }
        }
        finally { window.Close(); }

        var report = new List<string>();
        foreach (var count in new[] { 1, 20 })
        {
            Profile(count, report);
            Profile(count, report, custom: true);
        }
        File.WriteAllLines(Path.Combine(directory, "profile.txt"), report);
    }

    [AvaloniaFact]
    public void RenderBorderGradientGallery()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "ambient-glow-verification");
        Directory.CreateDirectory(directory);
        var panel = new StackPanel { Margin = new Thickness(20), Spacing = 20 };
        for (var mode = 0; mode < 3; mode++)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 24 };
            foreach (var size in new[] { new Size(240, 36), new Size(160, 72), new Size(64, 64) })
            {
                var glow = new AmbientGlowDecorator
                {
                    Width = size.Width, Height = size.Height,
                    CornerRadius = new CornerRadius(size.Width == 240 ? 18 : 12),
                    InvertBorderGradient = mode == 1, IsAnimationEnabled = false
                };
                if (mode == 2)
                {
                    glow.HighlightGradientStops = new GradientStops
                    {
                        new(Colors.Transparent, 0), new(Colors.Cyan, 0.4), new(Colors.Magenta, 0.5),
                        new(Colors.Transparent, 0.6), new(Colors.Transparent, 1)
                    };
                    glow.GlowGradientStops = new GradientStops
                    {
                        new(Colors.Transparent, 0), new(Colors.MediumPurple, 0.5), new(Colors.Transparent, 1)
                    };
                }
                row.Children.Add(glow);
            }
            panel.Children.Add(row);
        }
        var window = AmbientGlowDecoratorTests.Show(panel, 560, 296);
        try
        {
            var probe = new GlowRenderProbe(window);
            foreach (var dark in new[] { false, true })
            {
                window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
                window.Background = new SolidColorBrush(Color.Parse(dark ? "#141C22" : "#F4F6F8"));
                foreach (var scale in new[] { 1d, 1.5, 2d })
                {
                    using var bitmap = new SKBitmap((int)(560 * scale), (int)(296 * scale));
                    using var canvas = new SKCanvas(bitmap);
                    probe.RenderTo(canvas, scale);
                    using var image = SKImage.FromBitmap(bitmap);
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    using var stream = File.Create(Path.Combine(directory, $"borders-{(dark ? "dark" : "light")}-{scale:0.0}.png"));
                    data.SaveTo(stream);
                }
            }
        }
        finally { window.Close(); }
    }

    private static void Profile(int count, List<string> report, bool custom = false)
    {
        var panel = new StackPanel { Spacing = 4 };
        var decorations = new List<AmbientGlowDecorator>();
        var surfaces = new List<AmbientGlowSurface>();
        for (var i = 0; i < count; i++)
        {
            var decoration = new AmbientGlowDecorator
            {
                Width = 240, Height = 48, CornerRadius = new CornerRadius(10),
                AmbientColor = Colors.DodgerBlue, IsAnimationEnabled = false
            };
            if (custom)
            {
                decoration.HighlightGradientStops = new GradientStops
                {
                    new(Colors.Transparent, 0), new(Colors.Cyan, 0.5), new(Colors.Transparent, 1)
                };
                decoration.GlowGradientStops = decoration.HighlightGradientStops;
            }
            decorations.Add(decoration);
            panel.Children.Add(decoration);
        }
        var window = AmbientGlowDecoratorTests.Show(panel, 240, count * 52);
        var probe = new GlowRenderProbe(panel);
        window.UpdateLayout();
        try
        {
            foreach (var decoration in decorations) surfaces.Add(AmbientGlowDecoratorTests.Surface(decoration));
            using var bitmap = new SKBitmap(240, count * 52);
            using var canvas = new SKCanvas(bitmap);
            for (var frame = 0; frame < 20; frame++)
            {
                for (var i = 0; i < count; i++) probe.SetAngle(i, frame);
                canvas.Clear();
                probe.RenderTo(canvas);
            }
            var stopwatch = Stopwatch.StartNew();
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var frame = 0; frame < 120; frame++)
            {
                for (var i = 0; i < count; i++) probe.SetAngle(i, frame * 3);
                canvas.Clear();
                probe.RenderTo(canvas);
            }
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            stopwatch.Stop();
            report.Add($"{count} {(custom ? "custom" : "generated")} visible: deterministic angle update + real Skia DrawingContextHelper.RenderAsync, " +
                $"{stopwatch.Elapsed.TotalMilliseconds / 120:F3} ms/frame, {allocated / 120} managed bytes/frame (120 frames, warmed up; includes Avalonia rendering).");
            foreach (var decoration in decorations) decoration.IsAnimationEnabled = true;
            foreach (var surface in surfaces) Assert.True(surface.IsAnimationRunning);
            panel.IsVisible = false;
            foreach (var surface in surfaces) Assert.False(surface.IsAnimationRunning);
            report.Add($"{count} {(custom ? "custom" : "generated")} hidden: all frame controllers stopped.");
        }
        finally { window.Close(); }
    }
}
