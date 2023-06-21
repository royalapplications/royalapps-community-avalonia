using System;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

public interface IDisposeNativeControl
{
    event EventHandler<NativeControlEventArgs> DisposeNativeControl;
}