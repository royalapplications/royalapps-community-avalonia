using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace RoyalApps.Community.Avalonia.Common.Controls;

// Value-only message: the render thread never retains or reads a Control, binding, or mutable UI brush.
internal readonly record struct AmbientGlowVisualState(
    Size Size, CornerRadius CornerRadius, Color Color, bool IsDark,
    double HighlightThickness, double GlowOpacity, TimeSpan CycleDuration, bool IsAnimating,
    bool InvertBorderGradient = false, double HighlightOpacity = 1,
    IReadOnlyList<ImmutableGradientStop>? HighlightGradientStops = null,
    IReadOnlyList<ImmutableGradientStop>? GlowGradientStops = null);
