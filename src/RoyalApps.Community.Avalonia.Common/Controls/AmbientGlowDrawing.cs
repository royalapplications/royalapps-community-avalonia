using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace RoyalApps.Community.Avalonia.Common.Controls;

// Owned by one render handler (or one static fallback). No AvaloniaObjects or native resources.
internal sealed class AmbientGlowDrawing
{
    private AmbientGlowVisualState? _state;
    private ImmutableGradientStop[] _bloomStops = [];
    private ImmutableGradientStop[] _glowStops = [];
    private ImmutableGradientStop[] _lightStops = [];
    private ImmutableSolidColorBrush? _wash;
    private ImmutableRadialGradientBrush? _bloom;
    private ImmutablePen? _glowPen;
    private ImmutablePen? _lightPen;
    private RoundedRect _card;
    private RoundedRect _stroke;
    private double _thickness;
    private double _angle = double.NaN;

    internal void Update(AmbientGlowVisualState state)
    {
        state = state with { IsAnimating = false, CycleDuration = default };
        if (_state == state) return;
        _state = state;
        var accent = state.Color;
        var highlight = accent.ChangeBrightness(0.55F);
        _wash = new ImmutableSolidColorBrush(accent, 0.055);
        var bloomColor = Color.FromArgb((byte)(accent.A * (state.IsDark ? 40 : 100) / 255), accent.R, accent.G, accent.B);
        var transparent = Color.FromArgb(0, accent.R, accent.G, accent.B);
        _bloomStops = [new(0, state.IsDark ? bloomColor : transparent), new(1, state.IsDark ? transparent : bloomColor)];
        var borderBase = state.IsDark ? Color.FromArgb(0, highlight.R, highlight.G, highlight.B) : accent.ChangeBrightness(-0.35F);
        _glowStops = CreateSweep(0.2, borderBase, highlight);
        _lightStops = CreateSweep(0.05, borderBase, highlight);
        var side = Math.Max(0, Math.Min(state.Size.Width, state.Size.Height));
        _thickness = Math.Min(state.HighlightThickness, side);
        var radius = state.CornerRadius;
        _card = new RoundedRect(new Rect(state.Size), new CornerRadius(
            Math.Clamp(radius.TopLeft, 0, side / 2), Math.Clamp(radius.TopRight, 0, side / 2),
            Math.Clamp(radius.BottomRight, 0, side / 2), Math.Clamp(radius.BottomLeft, 0, side / 2)));
        _stroke = _card.Deflate(_thickness / 2, _thickness / 2);
        var angle = double.IsNaN(_angle) ? 0 : _angle;
        _angle = double.NaN;
        SetAngle(angle);
    }

    internal void SetAngle(double angle)
    {
        if (_state is not { } state || _angle == angle) return;
        _angle = angle;
        var radians = angle * Math.PI / 180;
        var center = new RelativePoint(0.5 + 0.5 * Math.Sin(radians), 0.5 - 0.5 * Math.Cos(radians), RelativeUnit.Relative);
        // Immutable wrappers change per frame; immutable stop arrays are shared, never rebuilt here.
        _bloom = new ImmutableRadialGradientBrush(_bloomStops, opacity: state.GlowOpacity,
            center: center, gradientOrigin: center, radius: 0.85);
        _glowPen = new ImmutablePen(new ImmutableConicGradientBrush(_glowStops,
            opacity: (state.IsDark ? 0.5 : 0.8) * state.GlowOpacity, angle: angle + 180), _thickness * 6);
        _lightPen = new ImmutablePen(new ImmutableConicGradientBrush(_lightStops, angle: angle + 180), _thickness);
    }

    internal void Draw(ImmediateDrawingContext context)
    {
        if (_card.Rect.Width <= 0 || _card.Rect.Height <= 0) return;
        using var clip = context.PushClip(_card);
        context.DrawRectangle(_wash, null, _card.Rect);
        context.DrawRectangle(_bloom, null, _card.Rect);
        DrawBorder(context, _glowPen!);
        DrawBorder(context, _lightPen!);
    }

    private void DrawBorder(ImmediateDrawingContext context, ImmutablePen pen)
    {
        if (_stroke.IsUniform)
        {
            context.DrawRectangle(null, pen, _stroke.Rect, _stroke.RadiiTopLeft.X, _stroke.RadiiTopLeft.Y);
            return;
        }
        // ImmediateDrawingContext has no per-corner rectangle overload. Clip four uniform
        // rectangles to quadrants while keeping the full brush bounds for identical gradients.
        var rect = _card.Rect;
        var half = new Size(rect.Width / 2, rect.Height / 2);
        DrawQuarter(context, pen, new Rect(rect.TopLeft, half), _stroke.RadiiTopLeft);
        DrawQuarter(context, pen, new Rect(new Point(half.Width, 0), half), _stroke.RadiiTopRight);
        DrawQuarter(context, pen, new Rect(new Point(half.Width, half.Height), half), _stroke.RadiiBottomRight);
        DrawQuarter(context, pen, new Rect(new Point(0, half.Height), half), _stroke.RadiiBottomLeft);
    }

    private void DrawQuarter(ImmediateDrawingContext context, ImmutablePen pen, Rect clip, Vector radius)
    {
        using var quadrant = context.PushClip(clip);
        context.DrawRectangle(null, pen, _stroke.Rect, radius.X, radius.Y);
    }

    internal void Draw(DrawingContext context)
    {
        if (_card.Rect.Width <= 0 || _card.Rect.Height <= 0) return;
        using var clip = context.PushClip(_card);
        context.DrawRectangle(_wash, null, _card);
        context.DrawRectangle(_bloom, null, _card);
        context.DrawRectangle(null, _glowPen, _stroke);
        context.DrawRectangle(null, _lightPen, _stroke);
    }

    private static ImmutableGradientStop[] CreateSweep(double arc, Color background, Color highlight)
    {
        var shoulder = Color.FromArgb(
            (byte)(background.A + (highlight.A - background.A) * (110d / 255)),
            (byte)(background.R + (highlight.R - background.R) * (110d / 255)),
            (byte)(background.G + (highlight.G - background.G) * (110d / 255)),
            (byte)(background.B + (highlight.B - background.B) * (110d / 255)));
        return [new(0, background), new(0.5 - arc, background), new(0.5 - arc * 0.4, shoulder),
            new(0.5, highlight), new(0.5 + arc * 0.4, shoulder), new(0.5 + arc, background), new(1, background)];
    }
}
