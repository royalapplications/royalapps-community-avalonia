using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia.Helpers;
using Avalonia.VisualTree;
using RoyalApps.Community.Avalonia.Common.Controls;
using SkiaSharp;

namespace RoyalApps.Community.Avalonia.Common.Tests.Controls;

// Offscreen rendering invokes exactly the immediate-mode drawing used by the compositor handler.
// DrawingContextHelper's normal control traversal does not include child composition visuals.
internal sealed class GlowRenderProbe : Control, ICustomDrawOperation
{
    private readonly Control _root;
    private readonly List<(AmbientGlowSurface Surface, AmbientGlowDrawing Drawing)> _entries = [];
    internal bool UseImmediateDrawing { get; set; } = true;

    internal GlowRenderProbe(Control root)
    {
        _root = root;
        foreach (var decoration in root.GetVisualDescendants().OfType<AmbientGlowDecorator>())
            _entries.Add((AmbientGlowDecoratorTests.Surface(decoration), new AmbientGlowDrawing()));
        Width = root.Bounds.Width;
        Height = root.Bounds.Height;
        Measure(new Size(Width, Height));
        Arrange(new Rect(0, 0, Width, Height));
    }

    internal void SetAngle(int index, double angle)
    {
        var entry = _entries[index];
        entry.Drawing.Update(entry.Surface.VisualState!.Value);
        entry.Drawing.SetAngle(angle);
    }

    internal void RenderTo(SKCanvas canvas, double scale = 1)
    {
        RenderTransformOrigin = RelativePoint.TopLeft;
        RenderTransform = new ScaleTransform(scale, scale);
        DrawingContextHelper.RenderAsync(canvas, this, new Rect(0, 0, Width * scale, Height * scale),
            new Vector(96 * scale, 96 * scale)).GetAwaiter().GetResult();
    }

    public override void Render(DrawingContext context)
    {
        if (UseImmediateDrawing)
        {
            context.Custom(this);
            return;
        }
        if (_root is Window window) context.DrawRectangle(window.Background, null, Bounds);
        foreach (var entry in _entries)
        {
            if (entry.Surface.VisualState is not { } state) continue;
            entry.Drawing.Update(state);
            var origin = entry.Surface.TranslatePoint(default, _root)!.Value;
            using var transform = context.PushTransform(Matrix.CreateTranslation(origin.X, origin.Y));
            entry.Drawing.Draw(context);
        }
    }

    public void Render(ImmediateDrawingContext context)
    {
        if (_root is Window window && window.Background is ISolidColorBrush background)
            context.DrawRectangle(new ImmutableSolidColorBrush(background.Color, background.Opacity), null, Bounds);
        foreach (var entry in _entries)
        {
            if (entry.Surface.VisualState is not { } state) continue;
            entry.Drawing.Update(state);
            var origin = entry.Surface.TranslatePoint(default, _root)!.Value;
            using var transform = context.PushPreTransform(Matrix.CreateTranslation(origin.X, origin.Y));
            entry.Drawing.Draw(context);
        }
    }

    public bool HitTest(Point point) => false;
    public bool Equals(ICustomDrawOperation? other) => ReferenceEquals(this, other);
    public void Dispose() { }
}
