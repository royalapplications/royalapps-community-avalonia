using System;

namespace RoyalApps.Community.Avalonia.Common.Controls;

// Render-thread clock state. One pending compositor tick is shared across rapid stop/start changes.
internal sealed class AmbientGlowAnimation
{
    private TimeSpan? _started;
    private TimeSpan _duration;
    private bool _framePending;
    internal bool IsRunning { get; private set; }
    internal double Angle { get; private set; }

    internal bool Configure(bool running, TimeSpan duration)
    {
        if (running != IsRunning || duration != _duration)
        {
            _started = null;
            Angle = 0;
        }
        IsRunning = running;
        _duration = duration;
        return RequestFrame();
    }

    internal bool Tick(TimeSpan timestamp)
    {
        _framePending = false;
        if (!IsRunning) return false;
        _started ??= timestamp;
        Angle = AngleAt(timestamp - _started.Value, _duration);
        return RequestFrame();
    }

    private bool RequestFrame()
    {
        if (!IsRunning || _framePending) return false;
        _framePending = true;
        return true;
    }

    internal static double AngleAt(TimeSpan elapsed, TimeSpan duration) =>
        (Math.Max(0, elapsed.Ticks) % duration.Ticks) / (double)duration.Ticks * 360;
}
