using System;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

public class NativeControlEventArgs : EventArgs
{
    public IDisposeNativeControl ViewModel { get; }

    public NativeControlEventArgs(IDisposeNativeControl viewModel)
    {
        ViewModel = viewModel;
    }
}