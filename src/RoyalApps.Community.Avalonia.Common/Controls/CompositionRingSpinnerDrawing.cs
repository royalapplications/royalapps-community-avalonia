using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace RoyalApps.Community.Avalonia.Common.Controls;

// Owned exclusively by the compositor thread. No UI-thread AvaloniaObjects are retained.
internal sealed class CompositionRingSpinnerDrawing
{
    private CompositionRingSpinnerVisualState _state;
    private ImmutablePen? _arcPen;
    private ImmutablePen? _trackPen;
    private ImmutablePen? _dashedPen;
    private readonly double[] _dashLengths = new double[2];
    private float _radius;
    private float _sweep = float.NaN;

    internal void Update(CompositionRingSpinnerVisualState state)
    {
        var side = (float)Math.Min(state.Size.Width, state.Size.Height);
        if (side <= 0 || !float.IsFinite(side))
        {
            _state = state;
            _arcPen = null;
            _trackPen = null;
            _dashedPen = null;
            _radius = 0;
            return;
        }
        var stroke = Math.Clamp((float)state.StrokeThickness, Math.Min(1F, side / 2F), side / 2F);
        var radius = (side - stroke) / 2F;
        if (_radius != radius)
        {
            _radius = radius;
            _dashedPen = null;
        }
        if (_arcPen is null || _arcPen.Thickness != stroke || _state.ForegroundColor != state.ForegroundColor)
            _arcPen = new ImmutablePen(new ImmutableSolidColorBrush(state.ForegroundColor), stroke, lineCap: PenLineCap.Round);
        if (_trackPen is null || _trackPen.Thickness != stroke || _state.TrackColor != state.TrackColor)
            _trackPen = new ImmutablePen(new ImmutableSolidColorBrush(state.TrackColor), stroke, lineCap: PenLineCap.Round);
        if (_state.ForegroundColor != state.ForegroundColor || _state.StrokeThickness != state.StrokeThickness) _dashedPen = null;
        _state = state;
    }

    internal void Draw(ImmediateDrawingContext context, TimeSpan elapsed)
    {
        if (_radius <= 0) return;
        var center = new Point(_state.Size.Width / 2, _state.Size.Height / 2);
        if (_state.TrackColor.A > 0)
            context.DrawEllipse(null, _trackPen, center, _radius, _radius);
        if (!_state.IsActive) return;

        var (start, sweep) = GetAngles(elapsed);
        // ImmediateDrawingContext exposes ellipses but not arbitrary geometry in the
        // published Avalonia 12.1.2 reference assembly. One dash describes the arc;
        // rotating the circle positions its start without rebuilding the pen.
        // The pen stays cached throughout the constant-sweep part of the cycle.
        if (_dashedPen is null || _sweep != sweep)
        {
            var circumference = 2 * Math.PI * _radius / _arcPen!.Thickness;
            _dashLengths[0] = circumference * sweep / 360;
            _dashLengths[1] = circumference - _dashLengths[0];
            _dashedPen = new ImmutablePen((IImmutableBrush)_arcPen.Brush!, _arcPen.Thickness,
                new ImmutableDashStyle(_dashLengths, 0), PenLineCap.Round);
            _sweep = sweep;
        }
        using var transform = context.PushPreTransform(
            Matrix.CreateRotation(start * Math.PI / 180) * Matrix.CreateTranslation(center.X, center.Y));
        context.DrawEllipse(null, _dashedPen, default, _radius, _radius);
    }

    internal static (float Start, float Sweep) GetAngles(TimeSpan elapsed)
    {
        var rootRotation = Progress(elapsed, TimeSpan.FromSeconds(3)) * 359.99F;
        var sliceProgress = Progress(elapsed, TimeSpan.FromMilliseconds(1200));
        var sliceRotation = 0F;
        var sweep = 280F;
        if (sliceProgress < 0.33F)
            sweep = Lerp(10F, 280F, EaseOutQuadratic(sliceProgress / 0.33F));
        else if (sliceProgress > 0.66F)
        {
            var progress = EaseOutQuadratic((sliceProgress - 0.66F) / 0.34F);
            sliceRotation = Lerp(0F, 359.99F, progress);
            sweep = Lerp(280F, 10F, progress);
        }
        return (rootRotation + sliceRotation - 90F, sweep);
    }

    private static float Progress(TimeSpan elapsed, TimeSpan duration) =>
        (float)((double)(elapsed.Ticks % duration.Ticks) / duration.Ticks);
    private static float EaseOutQuadratic(float value)
    {
        value = Math.Clamp(value, 0F, 1F);
        return 1F - (1F - value) * (1F - value);
    }
    private static float Lerp(float start, float end, float progress) => start + (end - start) * progress;
}
