using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using RoyalApps.Community.Avalonia.Common.Controls;
using Xunit;

namespace RoyalApps.Community.Avalonia.Common.Tests.Controls;

public sealed class AmbientGlowCompositionTests
{
    [AvaloniaFact]
    public void CompositorClockAdvancesWhileUiThreadWaitsAndStopsAfterStopMessage()
    {
        var host = new Border { Width = 240, Height = 48 };
        var window = AmbientGlowDecoratorTests.Show(host);
        var clock = new AmbientGlowAnimation();
        var compositor = ElementComposition.GetElementVisual(host)!.Compositor;
        var visual = compositor.CreateCustomVisual(new AmbientGlowVisualHandler(clock));
        visual.Size = new Vector(240, 48);
        ElementComposition.SetElementChildVisual(host, visual);
        var state = new AmbientGlowVisualState(new Size(240, 48), new CornerRadius(8), Colors.Green,
            false, 0.5, 1, TimeSpan.FromSeconds(9), true);
        try
        {
            visual.SendHandlerMessage(state);
            Drain();
            Assert.True(clock.IsRunning);
            var before = clock.Angle;
            // The headless timer normally runs on the dispatcher. Drive its real compositor
            // from a worker here, while deliberately blocking that dispatcher without pumping.
            Task.Run(() =>
            {
                for (var i = 0; i < 5; i++)
                {
                    Thread.Sleep(25);
                    AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                }
            }).GetAwaiter().GetResult();
            Assert.True(clock.Angle > before);

            visual.SendHandlerMessage(state with { IsAnimating = false });
            Drain();
            Assert.False(clock.IsRunning);
            Assert.Equal(0, clock.Angle);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(3);
            Assert.Equal(0, clock.Angle);

            visual.SendHandlerMessage(state);
            Drain();
            Assert.True(clock.IsRunning);
            ElementComposition.SetElementChildVisual(host, null);
            Drain();
            var detachedAngle = clock.Angle;
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(3);
            Assert.Equal(detachedAngle, clock.Angle);
        }
        finally
        {
            ElementComposition.SetElementChildVisual(host, null);
            window.Close();
            Drain();
        }
    }

    [AvaloniaFact]
    public void SurfaceReattachmentUsesANewVisualAndDoesNotRetainOldMessages()
    {
        var decoration = new AmbientGlowDecorator { Width = 240, Height = 48 };
        var parent = new Border { Child = decoration };
        var window = AmbientGlowDecoratorTests.Show(parent);
        try
        {
            var surface = AmbientGlowDecoratorTests.Surface(decoration);
            var original = ElementComposition.GetElementChildVisual(surface);
            Assert.NotNull(original);
            parent.Child = null;
            Assert.Null(ElementComposition.GetElementChildVisual(surface));
            Assert.Equal(0, surface.VisibilitySubscriptionCount);
            parent.Child = decoration;
            window.UpdateLayout();
            Assert.NotSame(original, ElementComposition.GetElementChildVisual(surface));
            Drain();
            Assert.True(surface.IsAnimationRunning);
            parent.IsVisible = false;
            Assert.False(surface.VisualState!.Value.IsAnimating);
            Drain();
        }
        finally { window.Close(); Drain(); }
    }

    private static void Drain()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
    }
}
