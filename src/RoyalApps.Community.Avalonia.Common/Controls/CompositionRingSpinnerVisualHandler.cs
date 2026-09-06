using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.Composition;

namespace RoyalApps.Community.Avalonia.Common.Controls;

internal sealed class CompositionRingSpinnerVisualHandler : CompositionCustomVisualHandler
{
    private CompositionRingSpinnerDrawing? _drawing;
    private CompositionRingSpinnerVisualState _state;
    private TimeSpan? _firstFrame;
    private TimeSpan _currentFrame;
    private bool _frameRegistered;

    internal TimeSpan Elapsed => _firstFrame.HasValue ? _currentFrame - _firstFrame.Value : TimeSpan.Zero;
    internal bool IsActive => _state.IsActive;
    internal int FrameCount { get; private set; }
    internal bool HasPendingFrame => _frameRegistered;

    public override void OnMessage(object message)
    {
        if (message is not CompositionRingSpinnerVisualState state) return;
        _state = state;
        _drawing ??= new CompositionRingSpinnerDrawing();
        _drawing.Update(state);
        if (state.IsActive) RegisterFrame();
        else _firstFrame = null;
        Invalidate();
    }

    public override void OnAnimationFrameUpdate()
    {
        FrameCount++;
        _frameRegistered = false;
        if (!_state.IsActive) return;
        _currentFrame = CompositionNow;
        _firstFrame ??= _currentFrame;
        Invalidate();
        RegisterFrame();
    }

    public override void OnRender(ImmediateDrawingContext context) => _drawing?.Draw(context, Elapsed);
    public override Rect GetRenderBounds() => new(_state.Size);

    private void RegisterFrame()
    {
        if (_frameRegistered) return;
        _frameRegistered = true;
        RegisterForNextAnimationFrameUpdate();
    }
}
