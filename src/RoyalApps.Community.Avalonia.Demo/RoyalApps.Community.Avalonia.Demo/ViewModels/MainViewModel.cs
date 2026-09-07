using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
namespace RoyalApps.Community.Avalonia.Demo.ViewModels;
public partial class MainViewModel : ViewModelBase, IDisposable
{
    public IReadOnlyList<SampleViewModel> Samples { get; } = [new GlowViewModel(), new SpinnerViewModel(), new SplitterViewModel(), new WinFormsViewModel()];
    [ObservableProperty] private SampleViewModel _selectedSample;
    [ObservableProperty] private string _selectedTheme = "System";
    [ObservableProperty] private bool _micaEnabled;
    public IReadOnlyList<string> Themes { get; } = ["System", "Light", "Dark"];
    public bool IsWindows => OperatingSystem.IsWindows();
    public IReadOnlyList<WindowTransparencyLevel> Transparency => MicaEnabled && IsWindows ? [WindowTransparencyLevel.Mica] : [];
    public MainViewModel() => _selectedSample = Samples[0];
    partial void OnSelectedThemeChanged(string value)
    {
        if (Application.Current is { } app)
            app.RequestedThemeVariant = value switch { "Light" => ThemeVariant.Light, "Dark" => ThemeVariant.Dark, _ => ThemeVariant.Default };
    }
    partial void OnMicaEnabledChanged(bool value) => OnPropertyChanged(nameof(Transparency));
    public void Dispose()
    {
        foreach (var sample in Samples)
            if (sample is IDisposable disposable) disposable.Dispose();
    }
}
