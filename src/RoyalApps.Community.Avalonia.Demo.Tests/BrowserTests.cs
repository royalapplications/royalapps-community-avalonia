using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using RoyalApps.Community.Avalonia.Demo;
using RoyalApps.Community.Avalonia.Demo.ViewModels;
using RoyalApps.Community.Avalonia.Demo.Views;
using RoyalApps.Community.Avalonia.Common.Controls;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(RoyalApps.Community.Avalonia.Demo.Tests.TestApplicationBuilder))]
[assembly: AvaloniaTestFramework]
namespace RoyalApps.Community.Avalonia.Demo.Tests;
public static class TestApplicationBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
public sealed class BrowserTests
{
    [AvaloniaFact]
    public void NavigationPreservesSettingsAndDetachesPreviousPreview()
    {
        using var model = new MainViewModel();
        var window = new MainWindow { DataContext = model };
        window.Show();
        try
        {
            Assert.Equal(4, model.Samples.Count);
            var glow = Assert.IsType<GlowViewModel>(model.SelectedSample);
            glow.GlowOpacity = 0.35;
            window.UpdateLayout();
            var oldPreview = window.GetVisualDescendants().OfType<AmbientGlowDecorator>().First();
            Assert.Equal(0.35, oldPreview.GlowOpacity);
            model.SelectedSample = model.Samples[1];
            window.UpdateLayout();
            Assert.Null(TopLevel.GetTopLevel(oldPreview));
            var spinner = window.GetVisualDescendants().OfType<CompositionRingSpinner>().Single();
            Assert.Equal(64, spinner.Width);
            model.SelectedSample = model.Samples[0];
            window.UpdateLayout();
            Assert.Same(glow, model.SelectedSample);
            Assert.Equal(0.35, window.GetVisualDescendants().OfType<AmbientGlowDecorator>().First().GlowOpacity);
            model.SelectedSample.ResetCommand.Execute(null);
            Assert.Equal(1, glow.GlowOpacity);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void EditorsAndResetUpdateLiveSamples()
    {
        using var model = new MainViewModel();
        var window = new MainWindow { DataContext = model };
        window.Show();
        try
        {
            window.UpdateLayout();
            window.GetVisualDescendants().OfType<Expander>().Single().IsExpanded = true;
            window.UpdateLayout();
            Assert.Equal(3, window.GetVisualDescendants().OfType<AmbientGlowDecorator>().Count());
            Assert.True(window.GetVisualDescendants().OfType<ProgressBar>().Single().IsIndeterminate);
            var slider = window.GetVisualDescendants().OfType<Slider>().First();
            slider.Value = 15;
            Assert.Equal(15, Assert.IsType<GlowViewModel>(model.SelectedSample).CycleSeconds);
            var glow = (GlowViewModel)model.SelectedSample;
            glow.ThemeColors = true;
            model.SelectedTheme = "Dark";
            Assert.Equal(Colors.MediumPurple, window.GetVisualDescendants().OfType<AmbientGlowDecorator>().First().EffectiveAmbientColor);
            model.SelectedTheme = "Light";
            Assert.Equal(Colors.SeaGreen, window.GetVisualDescendants().OfType<AmbientGlowDecorator>().First().EffectiveAmbientColor);
            model.SelectedSample = model.Samples[1];
            window.UpdateLayout();
            var spinner = (SpinnerViewModel)model.SelectedSample;
            spinner.Size = 100; spinner.IsActive = false; spinner.StrokeThickness = 7;
            var preview = window.GetVisualDescendants().OfType<CompositionRingSpinner>().Single();
            Assert.Equal(100, preview.Width); Assert.False(preview.IsActive);
            spinner.ResetCommand.Execute(null);
            Assert.Equal(64, preview.Width); Assert.True(preview.IsActive); Assert.Equal(3, preview.StrokeThickness);
            model.SelectedSample = model.Samples[2];
            window.UpdateLayout();
            var splitter = (SplitterViewModel)model.SelectedSample;
            splitter.LeftWidth = new GridLength(5, GridUnitType.Star);
            model.SelectedSample = model.Samples[0];
            model.SelectedSample = splitter;
            window.UpdateLayout();
            Assert.Equal(5, splitter.LeftWidth.Value);
            splitter.ResetCommand.Execute(null);
            Assert.Equal(2, splitter.LeftWidth.Value);
            Assert.Equal(2, splitter.TopHeight.Value);
        }
        finally { model.SelectedTheme = "System"; window.Close(); }
    }

    [AvaloniaFact]
    public void AllPortablePagesRenderInBothThemesAndWindowSizes()
    {
        using var model = new MainViewModel();
        var window = new MainWindow { DataContext = model };
        window.Show();
        try
        {
            foreach (var theme in new[] { "Light", "Dark" })
            foreach (var width in new[] { 850d, 1180d })
            foreach (var page in model.Samples.Where(s => s is not WinFormsViewModel w || !w.IsAvailable))
            {
                model.SelectedTheme = theme;
                window.Width = width;
                model.SelectedSample = page;
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                using var frame = window.CaptureRenderedFrame();
                Assert.NotNull(frame);
                Assert.Equal((int)width, frame.PixelSize.Width);
                var directory = Environment.GetEnvironmentVariable("DEMO_SCREENSHOTS");
                if (!string.IsNullOrEmpty(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                    using var output = System.IO.File.Create(System.IO.Path.Combine(directory, $"{page.Title}-{theme}-{width}.png"));
                    frame.Save(output, new global::Avalonia.Media.Imaging.PngBitmapEncoderOptions());
                }
            }
        }
        finally { model.SelectedTheme = "System"; window.Close(); }
    }

    [AvaloniaFact]
    public void WinFormsAvailabilityMatchesBuildAndDisposalIsExplicit()
    {
        using var model = new MainViewModel();
        var sample = Assert.IsType<WinFormsViewModel>(model.Samples[3]);
#if WINDOWS
        Assert.True(sample.IsAvailable);
        var nativeTemplate = new NativeTabViewLocator();
        Assert.IsNotAssignableFrom<IRecyclingDataTemplate>(nativeTemplate);
        Assert.NotSame(nativeTemplate.Build(sample.Tabs[0]), nativeTemplate.Build(sample.Tabs[1]));
        var first = sample.Tabs[0];
        var second = sample.Tabs[1];
        var disposed = 0;
        first.DisposeWinFormsControl += (_, _) => disposed++;
        sample.SelectedTab = second;
        model.SelectedSample = sample;
        model.SelectedSample = model.Samples[0];
        Assert.Equal(0, disposed);
        first.CloseCommand.Execute(null);
        Assert.Equal(1, disposed);
        Assert.DoesNotContain(first, sample.Tabs);
        first.Dispose();
        Assert.Equal(1, disposed);
        var shutdownDisposals = 0;
        second.DisposeWinFormsControl += (_, _) => shutdownDisposals++;
        model.Dispose();
        Assert.Equal(1, shutdownDisposals);
        Assert.Empty(sample.Tabs);
#else
        Assert.False(sample.IsAvailable);
        Assert.IsType<UnavailableView>(new SampleViewLocator().Build(sample));
        var references = typeof(App).Assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(references, a => a.Name == "RoyalApps.Community.Avalonia.Windows" || a.Name == "System.Windows.Forms");
#endif
    }
}
