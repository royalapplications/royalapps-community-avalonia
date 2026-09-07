using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using RoyalApps.Community.Avalonia.Demo.ViewModels;
namespace RoyalApps.Community.Avalonia.Demo.Views;
public sealed class SampleViewLocator : IDataTemplate
{
    public bool Match(object? data) => data is SampleViewModel;
    public Control Build(object? data) => data switch
    {
        GlowViewModel => new GlowView(),
        SpinnerViewModel => new SpinnerView(),
        SplitterViewModel => new SplitterView(),
#if WINDOWS
        WinFormsViewModel { IsAvailable: true } => new WinFormsView(),
#endif
        WinFormsViewModel => new UnavailableView(),
        _ => throw new ArgumentException("Unknown sample", nameof(data))
    };
}
