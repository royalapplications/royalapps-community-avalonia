using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace RoyalApps.Community.Avalonia.Common.Controls;

// UI-thread adapter only: publishes configuration changes, never schedules animation frames.
internal sealed class AmbientGlowSurface : Control
{
    private readonly List<IDisposable> _subscriptions = [];
    private AmbientGlowDecorator? _owner;
    private CompositionCustomVisual? _customVisual;
    private AmbientGlowVisualState? _lastState;
    private AmbientGlowDrawing? _fallbackDrawing;
    private bool _attached;
    internal bool IsAnimationRunning => _customVisual is not null && _lastState is { IsAnimating: true };
    internal int VisibilitySubscriptionCount => _subscriptions.Count;
    internal AmbientGlowVisualState? VisualState => _lastState;

    public AmbientGlowSurface()
    {
        IsHitTestVisible = false;
        Focusable = false;
    }

    internal void Connect(AmbientGlowDecorator owner)
    {
        _owner = owner;
        Update();
    }

    internal void Disconnect()
    {
        DetachVisual();
        _owner = null;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        foreach (var ancestor in this.GetVisualAncestors())
            _subscriptions.Add(ancestor.GetObservable(IsVisibleProperty).Subscribe(new AnonymousObserver<bool>(_ => Update())));
        Update();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        DetachVisual();
        foreach (var subscription in _subscriptions) subscription.Dispose();
        _subscriptions.Clear();
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachVisual()
    {
        if (_customVisual is not null && _lastState is { } state)
            _customVisual.SendHandlerMessage(state with { IsAnimating = false, Size = default });
        ElementComposition.SetElementChildVisual(this, null);
        _customVisual = null;
        _lastState = null;
        _fallbackDrawing = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty || change.Property == IsVisibleProperty) Update();
    }

    internal void Update()
    {
        if (_owner is not { } owner) return;
        if (_attached && _customVisual is null && ElementComposition.GetElementVisual(this) is { } visual)
        {
            // A fresh handler on reattachment isolates messages/ticks queued for the old visual.
            _customVisual = visual.Compositor.CreateCustomVisual(new AmbientGlowVisualHandler());
            ElementComposition.SetElementChildVisual(this, _customVisual);
            _lastState = null;
            _fallbackDrawing = null;
        }
        var size = Bounds.Size;
        var active = _attached && IsEffectivelyVisible && owner.IsEffectivelyEnabled && owner.IsAnimationEnabled
            && owner.IsMotionAllowed && size.Width > 0 && size.Height > 0;
        var state = new AmbientGlowVisualState(size, owner.CornerRadius, owner.EffectiveAmbientColor,
            owner.IsDarkTheme, owner.HighlightThickness, owner.GlowOpacity,
            owner.CycleDuration, active);
        if (_lastState == state) return;
        _lastState = state;
        if (_customVisual is not null)
        {
            _customVisual.Size = new Vector(size.Width, size.Height);
            _customVisual.SendHandlerMessage(state);
        }
        else
        {
            // Static fallback for hosts without composition; never installs a UI-thread timer.
            _fallbackDrawing ??= new AmbientGlowDrawing();
            _fallbackDrawing.Update(state);
            _fallbackDrawing.SetAngle(0);
        }
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_customVisual is null) _fallbackDrawing?.Draw(context);
    }
}
