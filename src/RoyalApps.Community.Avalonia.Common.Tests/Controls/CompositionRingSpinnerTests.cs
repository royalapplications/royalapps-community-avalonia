using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Threading;
using RoyalApps.Community.Avalonia.Common.Controls;
using Xunit;

namespace RoyalApps.Community.Avalonia.Common.Tests.Controls;

public sealed class CompositionRingSpinnerTests
{
    [AvaloniaFact]
    public void StandaloneStyleLoadsFromDocumentedResourceUri()
    {
        var spinner = new CompositionRingSpinner { Width = 24, Height = 24 };
        var window = new Window { Content = spinner, Width = 100, Height = 100 };
        window.Styles.Add(new StyleInclude(new Uri("avares://RoyalApps.Community.Avalonia.Common/"))
        {
            Source = new Uri("avares://RoyalApps.Community.Avalonia.Common/Controls/CompositionRingSpinner.Styles.axaml")
        });
        try
        {
            window.Show(); window.UpdateLayout();
            Assert.Equal(Colors.DodgerBlue, spinner.ForegroundColor);
            spinner.ForegroundColor = Colors.Red;
            Assert.Equal(Colors.Red, spinner.ForegroundColor);
        }
        finally { window.Close(); Drain(); }
    }

    [AvaloniaFact]
    public void RepeatedMessagesDoNotRegisterDuplicateFrames()
    {
        var spinner = new CompositionRingSpinner { Width = 24, Height = 24 };
        var window = AmbientGlowDecoratorTests.Show(spinner);
        try
        {
            Drain();
            var handler = spinner.VisualHandler!;
            for (var i = 0; i < 20; i++) spinner.StrokeThickness = 1 + i % 3;
            Dispatcher.UIThread.RunJobs();
            var before = handler.FrameCount;
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Assert.Equal(before + 1, handler.FrameCount);
            Assert.True(handler.HasPendingFrame);
        }
        finally { window.Close(); Drain(); }
    }

    [AvaloniaFact]
    public void DefaultsAndValidation()
    {
        var spinner = new CompositionRingSpinner();
        Assert.True(spinner.IsActive);
        Assert.Equal(Colors.DodgerBlue, spinner.ForegroundColor);
        Assert.Equal(Colors.Transparent, spinner.TrackColor);
        Assert.Equal(2, spinner.StrokeThickness);
        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Assert.Throws<ArgumentException>(() => spinner.StrokeThickness = invalid);
        foreach (var finite in new[] { -10d, 0, 1, double.MaxValue })
        {
            spinner.StrokeThickness = finite;
            Assert.Equal(finite, spinner.StrokeThickness);
        }
    }

    [AvaloniaTheory]
    [InlineData(20, 30, 20)]
    [InlineData(double.PositiveInfinity, 16, 16)]
    [InlineData(24, double.PositiveInfinity, 24)]
    [InlineData(double.PositiveInfinity, double.PositiveInfinity, 0)]
    [InlineData(0, 10, 0)]
    public void MeasurementIsSquare(double width, double height, double expected)
    {
        var spinner = new CompositionRingSpinner();
        spinner.Measure(new Size(width, height));
        Assert.Equal(new Size(expected, expected), spinner.DesiredSize);
    }

    [AvaloniaFact]
    public void PropertiesVisibilityAndReattachmentUpdateCompositionState()
    {
        var spinner = new CompositionRingSpinner { Width = 24, Height = 24 };
        var parent = new Border { Child = spinner };
        var window = AmbientGlowDecoratorTests.Show(parent);
        try
        {
            Drain();
            Assert.True(spinner.VisualState!.Value.IsActive);
            var original = spinner.VisualHandler;
            spinner.ForegroundColor = Colors.Red;
            spinner.TrackColor = Colors.Gray;
            spinner.StrokeThickness = 3;
            Assert.Equal(Colors.Red, spinner.VisualState.Value.ForegroundColor);
            Assert.Equal(Colors.Gray, spinner.VisualState.Value.TrackColor);
            Assert.Equal(3, spinner.VisualState.Value.StrokeThickness);
            parent.IsVisible = false;
            Assert.False(spinner.VisualState.Value.IsActive);
            Drain();
            Assert.False(original!.IsActive);
            parent.IsVisible = true;
            spinner.IsActive = false;
            Assert.False(spinner.VisualState.Value.IsActive);
            spinner.IsActive = true;
            parent.Child = null;
            Assert.Null(spinner.VisualHandler);
            Assert.Equal(0, spinner.VisibilitySubscriptionCount);
            Drain();
            Assert.False(original.IsActive);
            parent.Child = spinner;
            window.UpdateLayout();
            Drain();
            Assert.NotSame(original, spinner.VisualHandler);
            Assert.True(spinner.VisualHandler!.IsActive);
            spinner.Width = 0;
            window.UpdateLayout();
            Assert.False(spinner.VisualState!.Value.IsActive);
        }
        finally { window.Close(); Drain(); }
    }

    [AvaloniaFact]
    public void CompositorProgressesWithoutDispatcherAndPendingCallbacksStop()
    {
        var spinner = new CompositionRingSpinner { Width = 24, Height = 24 };
        var window = AmbientGlowDecoratorTests.Show(spinner);
        try
        {
            Drain();
            var handler = spinner.VisualHandler!;
            var before = handler.Elapsed;
            Task.Run(() =>
            {
                for (var i = 0; i < 5; i++)
                {
                    Thread.Sleep(25);
                    AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                }
            }).GetAwaiter().GetResult();
            Assert.True(handler.Elapsed > before);
            spinner.IsVisible = false;
            Drain();
            var stopped = handler.Elapsed;
            var stoppedFrames = handler.FrameCount;
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(5);
            Assert.False(handler.IsActive);
            Assert.False(handler.HasPendingFrame);
            Assert.Equal(stoppedFrames, handler.FrameCount);
            Assert.Equal(stopped, handler.Elapsed);
        }
        finally { window.Close(); Drain(); }
    }

    [AvaloniaFact]
    public void DetachedControlsAndHandlersAreCollectible()
    {
        var parent = new Border();
        var window = AmbientGlowDecoratorTests.Show(parent);
        try
        {
            var references = new WeakReference[40];
            for (var i = 0; i < references.Length; i += 2)
                (references[i], references[i + 1]) = AttachAndDetach(parent, window);
            Drain();
            for (var i = 0; i < 3; i++)
            {
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); Drain();
            }
            foreach (var reference in references) Assert.False(reference.IsAlive);
        }
        finally { window.Close(); Drain(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference, WeakReference) AttachAndDetach(Border parent, Window window)
    {
        var spinner = new CompositionRingSpinner { Width = 24, Height = 24 };
        parent.Child = spinner;
        window.UpdateLayout(); Drain();
        var result = (new WeakReference(spinner), new WeakReference(spinner.VisualHandler!));
        parent.Child = null; Drain();
        return result;
    }

    [AvaloniaFact]
    public void ReportRetentionAfterRepeatedAttachmentCycles()
    {
        var parent = new Border();
        var window = AmbientGlowDecoratorTests.Show(parent);
        var report = new StringBuilder("instances,batch,managed_heap_bytes,private_process_bytes,retained_controls_or_handlers\n");
        try
        {
            foreach (var count in new[] { 1, 20, 100 })
            {
                for (var warmup = 0; warmup < 20; warmup++) AttachBatch(parent, window, count);
                for (var batch = 0; batch < 10; batch++)
                {
                    var references = new WeakReference[count * 2 * 10];
                    for (var cycle = 0; cycle < 10; cycle++)
                        AttachBatch(parent, window, count).CopyTo(references, cycle * count * 2);
                    Drain(); GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); Drain();
                    var retained = 0;
                    foreach (var reference in references) if (reference.IsAlive) retained++;
                    using var process = Process.GetCurrentProcess();
                    report.AppendLine($"{count},{batch},{GC.GetTotalMemory(true)},{process.PrivateMemorySize64},{retained}");
                    Assert.Equal(0, retained);
                }
            }
        }
        finally
        {
            window.Close(); Drain();
            var directory = Path.Combine(AppContext.BaseDirectory, "spinner-comparison");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "retention.csv"), report.ToString());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] AttachBatch(Border parent, Window window, int count)
    {
        var panel = new StackPanel();
        var references = new WeakReference[count * 2];
        for (var i = 0; i < count; i++) panel.Children.Add(new CompositionRingSpinner { Width = 24, Height = 24 });
        parent.Child = panel; window.UpdateLayout(); Drain();
        for (var i = 0; i < count; i++)
        {
            var spinner = (CompositionRingSpinner)panel.Children[i];
            references[i * 2] = new WeakReference(spinner);
            references[i * 2 + 1] = new WeakReference(spinner.VisualHandler!);
        }
        parent.Child = null; Drain();
        return references;
    }

    private static void Drain()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
    }
}
