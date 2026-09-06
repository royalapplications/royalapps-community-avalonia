using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.Composition;

namespace RoyalApps.Community.Avalonia.Common.Controls;

internal sealed class AmbientGlowVisualHandler(AmbientGlowAnimation? animation = null) : CompositionCustomVisualHandler
{
    private readonly AmbientGlowAnimation _animation = animation ?? new();
    private readonly AmbientGlowDrawing _drawing = new();
    private AmbientGlowVisualState _state;

    public override void OnMessage(object message)
    {
        if (message is not AmbientGlowVisualState state) return;
        _state = state;
        _drawing.Update(state);
        if (_animation.Configure(state.IsAnimating, state.CycleDuration))
            RegisterForNextAnimationFrameUpdate();
        _drawing.SetAngle(_animation.Angle);
        Invalidate();
    }

    public override void OnAnimationFrameUpdate()
    {
        if (!_animation.Tick(CompositionNow)) return;
        _drawing.SetAngle(_animation.Angle);
        Invalidate();
        RegisterForNextAnimationFrameUpdate();
    }

    public override void OnRender(ImmediateDrawingContext drawingContext) => _drawing.Draw(drawingContext);
    public override Rect GetRenderBounds() => new(_state.Size);
}
