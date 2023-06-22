using System;
using Avalonia.Controls;
using Avalonia.Platform;
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
    /// <summary>
    /// The WinForms control.
    /// </summary>
    public T? Control => WinFormsLifetimeManager.Instance.GetControl(DataContext as IDisposeWinFormsControl) as T;
    
    /// <inheritdoc cref="CreateNativeControlCore"/>
    protected sealed override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        // data context object is used as key in dictionary to locate existing control instances
        if (DataContext is not IDisposeWinFormsControl viewModel)
            throw new InvalidOperationException($"{nameof(DataContext)} must implement {nameof(IDisposeWinFormsControl)}.");

        // get existing instance based on the data context object
        var existingInstance = WinFormsLifetimeManager.Instance.GetControl(viewModel);
        if (existingInstance is not null)
            return new PlatformHandle(existingInstance.Handle, "Hndl");

        // check if a WinForms control is provided
        var newInstance = OnCreateWinFormsControl();
        if (newInstance != null)
            return new PlatformHandle(newInstance.Handle, "Hndl");
        
        // create a new instance and store it in the dictionary
        newInstance = Activator.CreateInstance<T>();
        WinFormsLifetimeManager.Instance.AddControl(viewModel, newInstance);
        return new PlatformHandle(newInstance.Handle, "Hndl");
    }

    /// <summary>
    /// Override this method to create the WinForms control instance yourself.
    /// If not implemented, the instance will be created automatically.
    /// </summary>
    /// <returns>An instance of the WinForms control to host.</returns>
    protected virtual T? OnCreateWinFormsControl()
    {
        return null;
    }
    
    /// <inheritdoc cref="DestroyNativeControlCore"/>
    protected sealed override void DestroyNativeControlCore(IPlatformHandle control)
    {
        // do not destroy/dispose the WinForms control here as it is managed in the WinFormsLifetimeManager
    }
}