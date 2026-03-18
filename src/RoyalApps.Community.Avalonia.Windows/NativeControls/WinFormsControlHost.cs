using System;
using Avalonia.Controls;
using Avalonia.Platform;
using Windows.Win32.Foundation;
using Control = System.Windows.Forms.Control;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

/// <summary>
/// A NativeControlHost descendent which creates a WinForms control and keeps it alive and in memory,
/// even when the hosting view is detached from the visual tree. Destruction/disposal of the control
/// has to be invoked manually by implementing the the IDisposeWinFormsControl interface on your
/// view model and raising the DisposeWinFormsControl event.
/// </summary>
/// <inheritdoc cref="NativeControlHost"/>
/// <typeparam name="T">The type of the WinForms control you want to host.</typeparam>
public class WinFormsControlHost<T> : NativeControlHost where T : Control
{
    private HWND _hostParentHandle = HWND.Null;
    private Control? _attachedHost;

    /// <summary>
    /// The WinForms control.
    /// </summary>
    public T? Control => WinFormsLifetimeManager.Instance.GetControl(DataContext as IDisposeWinFormsControl) as T;

    /// <inheritdoc cref="CreateNativeControlCore"/>
    protected sealed override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _hostParentHandle = new HWND(parent.Handle);

        // data context object is used as key in dictionary to locate existing control instances
        if (DataContext is not IDisposeWinFormsControl viewModel)
            throw new InvalidOperationException($"{nameof(DataContext)} must implement {nameof(IDisposeWinFormsControl)}.");

        if (WinFormsLifetimeManager.Instance.GetHost(viewModel) is null)
        {
            var newInstance = OnCreateWinFormsControl() ?? Activator.CreateInstance<T>();
            WinFormsLifetimeManager.Instance.AddControl(viewModel, newInstance);
        }

        _attachedHost = WinFormsLifetimeManager.Instance.AttachHost(viewModel, _hostParentHandle)
            ?? throw new InvalidOperationException("Failed to attach a hosted WinForms site.");

        return new PlatformHandle(_attachedHost.Handle, "Hndl");
    }

    /// <summary>
    /// Override this method to create the WinForms control instance yourself.
    /// If not implemented, the instance will be created automatically.
    /// </summary>
    /// <returns>An instance of the WinForms control to host.</returns>
    protected virtual T? OnCreateWinFormsControl() => null;

    /// <inheritdoc cref="DestroyNativeControlCore"/>
    protected sealed override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (DataContext is IDisposeWinFormsControl viewModel && _attachedHost is not null)
        {
            WinFormsLifetimeManager.Instance.ParkHost(viewModel, _attachedHost, _hostParentHandle);
        }

        _attachedHost = null;
        _hostParentHandle = HWND.Null;
    }
}
