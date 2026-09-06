using System;
using Avalonia;
using Avalonia.Media;

namespace RoyalApps.Community.Avalonia.Common.Controls;

// Value-only message: the render thread never retains or reads a Control, binding, or mutable UI brush.
internal readonly record struct AmbientGlowVisualState(
    Size Size, CornerRadius CornerRadius, Color Color, bool IsDark,
    double HighlightThickness, double GlowOpacity, TimeSpan CycleDuration, bool IsAnimating);
