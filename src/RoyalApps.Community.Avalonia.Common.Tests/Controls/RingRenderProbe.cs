using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia.Helpers;
using RoyalApps.Community.Avalonia.Common.Controls;
using SkiaSharp;

namespace RoyalApps.Community.Avalonia.Common.Tests.Controls;

internal sealed class RingRenderProbe : Control, ICustomDrawOperation
{
    private readonly CompositionRingSpinnerDrawing[] _drawings;
    private readonly LegacyRingSpinnerVisualHandler[] _legacy;
    internal CompositionRingSpinnerVisualState State { get; private set; }
    internal TimeSpan Phase { get; set; }
    internal bool UseLegacy { get; set; }

    internal RingRenderProbe(double size, int count = 1, bool active = true)
    {
        State = new(active, new Size(size, size), Color.FromArgb(210, 30, 144, 255),
            Color.FromArgb(60, 100, 100, 100), 2);
        _drawings = new CompositionRingSpinnerDrawing[count];
        _legacy = new LegacyRingSpinnerVisualHandler[count];
        for (var i = 0; i < count; i++)
        {
            _drawings[i] = new CompositionRingSpinnerDrawing();
            _drawings[i].Update(State);
            _legacy[i] = new LegacyRingSpinnerVisualHandler();
        }
        Width = (size + 8) * count;
        Height = size + 8;
        Measure(new Size(Width, Height));
        Arrange(new Rect(0, 0, Width, Height));
    }

    internal void RenderTo(SKCanvas canvas, double scale = 1)
    {
        RenderTransformOrigin = RelativePoint.TopLeft;
        RenderTransform = new ScaleTransform(scale, scale);
        DrawingContextHelper.RenderAsync(canvas, this, new Rect(0, 0, Width * scale, Height * scale),
            new Vector(96 * scale, 96 * scale)).GetAwaiter().GetResult();
    }

    internal void SetState(CompositionRingSpinnerVisualState state)
    {
        State = state;
        foreach (var drawing in _drawings) drawing.Update(state);
    }

    public override void Render(DrawingContext context) => context.Custom(this);
    public void Render(ImmediateDrawingContext context)
    {
        for (var i = 0; i < _drawings.Length; i++)
        {
            using var transform = context.PushPreTransform(Matrix.CreateTranslation(4 + i * (State.Size.Width + 8), 4));
            if (UseLegacy) _legacy[i].DrawAt(context, State, Phase);
            else _drawings[i].Draw(context, Phase);
        }
    }
    public bool HitTest(Point point) => false;
    public bool Equals(ICustomDrawOperation? other) => ReferenceEquals(this, other);
    public void Dispose() { }
}
