using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using SkiaSharp;
using Xunit;

namespace RoyalApps.Community.Avalonia.Common.Tests.Controls;

public sealed class RingRenderingComparisonTests
{
    private static string ArtifactDirectory => Path.Combine(AppContext.BaseDirectory, "spinner-comparison");

    [AvaloniaFact]
    public void ColorsChangedDuringEmptyBoundsAreUsedWhenRestored()
    {
        var probe = new RingRenderProbe(24) { Phase = TimeSpan.FromMilliseconds(500) };
        probe.SetState(probe.State with { Size = default, ForegroundColor = Colors.Lime, TrackColor = Colors.Transparent });
        probe.SetState(probe.State with { Size = new Size(24, 24) });
        using var bitmap = new SKBitmap(32, 32);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        probe.RenderTo(canvas);
        var painted = 0;
        for (var y = 0; y < bitmap.Height; y++)
        for (var x = 0; x < bitmap.Width; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            if (pixel.Alpha == 0) continue;
            painted++;
            Assert.True(pixel.Green > pixel.Blue && pixel.Green > pixel.Red);
        }
        Assert.True(painted > 0);
    }

    [AvaloniaFact]
    public void RenderingMatchesLegacyAtMultipleSizesScalesAndPhases()
    {
        Directory.CreateDirectory(ArtifactDirectory);
        var report = new StringBuilder("size,scale,phase_ms,mean_alpha_error,max_alpha_error\n");
        var worstMean = 0d;
        foreach (var size in new[] { 12, 16, 24, 32 })
        foreach (var scale in new[] { 1d, 1.5, 2 })
        {
            var probe = new RingRenderProbe(size);
            var side = (int)((size + 8) * scale);
            using var expected = new SKBitmap(side, side);
            using var actual = new SKBitmap(side, side);
            using var expectedCanvas = new SKCanvas(expected);
            using var actualCanvas = new SKCanvas(actual);
            using var gallery = new SKBitmap(side * 2, side * 8);
            using var galleryCanvas = new SKCanvas(gallery);
            galleryCanvas.Clear(SKColors.Transparent);
            var row = 0;
            foreach (var phase in new[] { 0, 100, 300, 500, 792, 900, 1100, 1500 })
            {
                probe.Phase = TimeSpan.FromMilliseconds(phase);
                expectedCanvas.Clear(SKColors.Transparent);
                actualCanvas.Clear(SKColors.Transparent);
                probe.UseLegacy = true; probe.RenderTo(expectedCanvas, scale);
                probe.UseLegacy = false; probe.RenderTo(actualCanvas, scale);
                var difference = 0L;
                var max = 0;
                for (var y = 0; y < side; y++)
                for (var x = 0; x < side; x++)
                {
                    var delta = Math.Abs(actual.GetPixel(x, y).Alpha - expected.GetPixel(x, y).Alpha);
                    difference += delta; max = Math.Max(max, delta);
                    if (x < 2 * scale || y < 2 * scale || x >= side - 2 * scale || y >= side - 2 * scale)
                        Assert.Equal(0, actual.GetPixel(x, y).Alpha);
                }
                var mean = difference / (double)(side * side);
                worstMean = Math.Max(worstMean, mean);
                report.AppendLine(FormattableString.Invariant($"{size},{scale},{phase},{mean:F4},{max}"));
                galleryCanvas.DrawBitmap(expected, 0, row * side, new SKSamplingOptions());
                galleryCanvas.DrawBitmap(actual, side, row * side, new SKSamplingOptions());
                row++;
            }
            using var data = gallery.Encode(SKEncodedImageFormat.Png, 100);
            using var file = File.Create(Path.Combine(ArtifactDirectory, $"size-{size}-scale-{scale.ToString(CultureInfo.InvariantCulture)}.png"));
            data.SaveTo(file);
        }
        File.WriteAllText(Path.Combine(ArtifactDirectory, "visuals.csv"), report.ToString());
        // A small average rasterization tolerance; galleries are also reviewed for cap/endpoint shape.
        Assert.True(worstMean < 2, $"Mean alpha error {worstMean:F4}; inspect spinner-comparison galleries.");
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(0.25)]
    [InlineData(1)]
    [InlineData(1.99)]
    [InlineData(24)]
    public void InactiveAndTinyRingsRenderSafely(double size)
    {
        if (size < 2)
        {
            var activeProbe = new RingRenderProbe(size);
            using var tinyBitmap = new SKBitmap(10, 10);
            using var tinyCanvas = new SKCanvas(tinyBitmap);
            activeProbe.RenderTo(tinyCanvas);
        }
        var probe = new RingRenderProbe(size, active: false);
        using var bitmap = new SKBitmap((int)Math.Ceiling(size + 8), (int)Math.Ceiling(size + 8));
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        probe.RenderTo(canvas);
        if (size < 2) return; // Tiny cases primarily guard against an invalid clamp range.
        using var expected = new SKBitmap(bitmap.Width, bitmap.Height);
        using var expectedCanvas = new SKCanvas(expected);
        expectedCanvas.Clear(SKColors.Transparent);
        probe.UseLegacy = true; probe.RenderTo(expectedCanvas);
        for (var y = 0; y < bitmap.Height; y++)
        for (var x = 0; x < bitmap.Width; x++)
            Assert.Equal(expected.GetPixel(x, y), bitmap.GetPixel(x, y));
    }

    [AvaloniaFact]
    public void ReportWarmedFrameCpuAndAllocations()
    {
        Directory.CreateDirectory(ArtifactDirectory);
        var report = new StringBuilder("instances,run,renderer,cpu_ms_per_frame,allocated_bytes_per_frame\n");
        foreach (var count in new[] { 1, 20, 100 })
        {
            var probe = new RingRenderProbe(24, count);
            using var bitmap = new SKBitmap((int)probe.Width, (int)probe.Height);
            using var canvas = new SKCanvas(bitmap);
            foreach (var legacy in new[] { true, false })
            {
                probe.UseLegacy = legacy;
                for (var frame = 0; frame < 600; frame++) Render(frame);
            }
            for (var run = 0; run < 5; run++)
            for (var order = 0; order < 2; order++)
            {
                probe.UseLegacy = (run + order) % 2 == 0;
                for (var frame = 0; frame < 60; frame++) Render(frame);
                var allocated = GC.GetTotalAllocatedBytes(true);
                var started = Stopwatch.GetTimestamp();
                for (var frame = 0; frame < 600; frame++) Render(frame);
                var ms = Stopwatch.GetElapsedTime(started).TotalMilliseconds / 600;
                var bytes = (GC.GetTotalAllocatedBytes(true) - allocated) / 600d;
                report.AppendLine(FormattableString.Invariant($"{count},{run},{(probe.UseLegacy ? "Skia" : "Avalonia")},{ms:F6},{bytes:F2}"));
            }
            void Render(int frame)
            {
                probe.Phase = TimeSpan.FromMilliseconds(frame * (1000d / 60));
                canvas.Clear(SKColors.Transparent);
                probe.RenderTo(canvas);
            }
        }
        File.WriteAllText(Path.Combine(ArtifactDirectory, "performance.csv"), report.ToString());
        // The documented acceptance decision uses repeated paired runs, not a noisy CI assertion.
    }
}
