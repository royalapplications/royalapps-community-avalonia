using System;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

/// <summary>
/// Implement this interface in view models using views containing a WinFormsControlHost.
/// </summary>
public interface IDisposeWinFormsControl
{
    /// <summary>
    /// Raise this event whenever you want to finally destroy/dispose your WinForms control.
    /// </summary>
    /// <inheritdoc cref="EventHandler{TEventArgs}"/>
    event EventHandler<WinFormsDisposeEventArgs> DisposeWinFormsControl;
}