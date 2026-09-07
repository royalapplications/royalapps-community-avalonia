using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
namespace RoyalApps.Community.Avalonia.Demo.ViewModels;
public partial class SpinnerViewModel : SampleViewModel
{
    public UiThreadDemoViewModel ThreadDemo { get; } = new();
    public override string Title => "CompositionRingSpinner";
    public override bool StartsCategory => false;
    public override string Category => "Controls";
    public override string Description => "A compositor-driven indeterminate progress indicator. Adjust its size, stroke, and colors while it runs.";
    [ObservableProperty] private bool _isActive = true;
    [ObservableProperty] private double _size = 64;
    [ObservableProperty] private double _strokeThickness = 3;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(ForegroundColor))] private string _colorPreset = "DodgerBlue";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(TrackColor))] private string _trackPreset = "MediumPurple";
    public Color ForegroundColor => ParseColor(ColorPreset);
    public Color TrackColor { get { var color = ParseColor(TrackPreset); return Color.FromArgb(45, color.R, color.G, color.B); } }
    public override void Reset() { IsActive = true; Size = 64; StrokeThickness = 3; ColorPreset = "DodgerBlue"; TrackPreset = "MediumPurple"; }
    public override string Code => """
        <!-- In Application.Styles: -->
        <StyleInclude Source="avares://RoyalApps.Community.Avalonia.Common/Controls/CompositionRingSpinner.Styles.axaml" />

        <!-- xmlns:common="using:RoyalApps.Community.Avalonia.Common.Controls" -->
        <common:CompositionRingSpinner Width="64" Height="64" IsActive="True"
            StrokeThickness="3" ForegroundColor="DodgerBlue" TrackColor="#2D9370DB" />
        """;
}
