using System;
using Avalonia.Controls;
using Avalonia.Platform;
using Control = System.Windows.Forms.Control;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

public class ManagedNativeControlHost<T> : NativeControlHost where T : Control
{
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        // data context object is used as key in dictionary to locate existing control instances
        if (DataContext is not IDisposeNativeControl viewModel)
            throw new InvalidOperationException($"{nameof(DataContext)} must implement {nameof(IDisposeNativeControl)}.");

        // get existing instance based on the data context object
        var existingInstance = NativeControlManager.Instance.GetControl(viewModel);
        if (existingInstance is not null)
            return new PlatformHandle(existingInstance.Handle, "Hndl");

        // create a new instance and store it in the dictionary
        var newInstance = Activator.CreateInstance<T>();
        NativeControlManager.Instance.AddControl(viewModel, newInstance);
        return new PlatformHandle(newInstance.Handle, "Hndl");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        // DO NOT dispose/cleanup the instance here!
    }
}