using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
namespace RoyalApps.Community.Avalonia.Demo.ViewModels;
public partial class GlowViewModel : SampleViewModel
{
    public UiThreadDemoViewModel ThreadDemo { get; } = new();
    public override string Title => "AmbientGlowDecorator";
    public override string Category => "Controls";
    public override string Description => "A contained ambient bloom with a traveling border highlight. Keep child backgrounds transparent to reveal the glow.";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(AmbientColor))] private string _colorPreset = "DodgerBlue";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(LightColor)), NotifyPropertyChangedFor(nameof(DarkColor))] private bool _themeColors;
    [ObservableProperty] private bool _animationEnabled = true;
    [ObservableProperty] private bool _motionAllowed = true;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CycleDuration))] private double _cycleSeconds = 9;
    [ObservableProperty] private double _highlightThickness = 0.5;
    [ObservableProperty] private double _glowOpacity = 1;
    public Color AmbientColor => ParseColor(ColorPreset);
    public Color? LightColor => ThemeColors ? Colors.SeaGreen : null;
    public Color? DarkColor => ThemeColors ? Colors.MediumPurple : null;
    public TimeSpan CycleDuration => TimeSpan.FromSeconds(CycleSeconds);
    public override void Reset() { ColorPreset = "DodgerBlue"; ThemeColors = false; AnimationEnabled = MotionAllowed = true; CycleSeconds = 9; HighlightThickness = 0.5; GlowOpacity = 1; }
    public override string Code => """
        <!-- In Application.Styles: -->
        <StyleInclude Source="avares://RoyalApps.Community.Avalonia.Common/Controls/AmbientGlowDecorator.Styles.axaml" />

        <!-- xmlns:common="using:RoyalApps.Community.Avalonia.Common.Controls" -->
        <common:AmbientGlowDecorator AmbientColor="DodgerBlue"
            IsAnimationEnabled="True" IsMotionAllowed="True" CycleDuration="0:0:9"
            HighlightThickness="0.5" GlowOpacity="1" CornerRadius="16" Padding="32">
            <TextBlock Text="A little atmosphere" />
        </common:AmbientGlowDecorator>
        <!-- A glow-backed button -->
        <Button Padding="0" Background="Transparent" BorderThickness="0">
            <common:AmbientGlowDecorator CornerRadius="12" Padding="24,12">
                <TextBlock Text="Connect" />
            </common:AmbientGlowDecorator>
        </Button>

        <!-- A pill-shaped badge -->
        <common:AmbientGlowDecorator CornerRadius="100" Padding="20,12">
            <TextBlock Text="Online" />
        </common:AmbientGlowDecorator>
        <!-- Optional: AmbientColorLight="SeaGreen" AmbientColorDark="MediumPurple" -->
        """;
}
