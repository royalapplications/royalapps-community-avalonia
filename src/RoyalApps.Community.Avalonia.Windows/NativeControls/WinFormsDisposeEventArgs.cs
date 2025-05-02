using System;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

/// <summary>
/// EventArgs with a reference to the view model.
/// </summary>
/// <inheritdoc cref="EventArgs"/>
public class WinFormsDisposeEventArgs : EventArgs
{
    /// <summary>
    /// The view model which implements the IDisposeWinFormsControl interface.
    /// </summary>
    public IDisposeWinFormsControl ViewModel { get; }

    /// <summary>
    /// Creates a new WinFormsDisposeEventArgs instance.
    /// </summary>
    /// <param name="viewModel">The view model which implements the IDisposeWinFormsControl interface.</param>
    public WinFormsDisposeEventArgs(IDisposeWinFormsControl viewModel)
    {
        ViewModel = viewModel;
    }
}
