using System.Collections.Generic;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

public class NativeControlManager
{
    private static NativeControlManager? _instance;
    public static NativeControlManager Instance => _instance ??= new NativeControlManager();

    private readonly Dictionary<IDisposeNativeControl, Control> _controls = new();

    private NativeControlManager() { }

    public void AddControl(IDisposeNativeControl viewModel, Control control)
    {
        _controls[viewModel] = control;
        viewModel.DisposeNativeControl += ViewModel_DisposeNativeControl;
    }

    public Control? GetControl(IDisposeNativeControl viewModel)
    {
        return _controls.TryGetValue(viewModel, out var existingInstance) 
            ? existingInstance 
            : null;
    }
    
    private void ViewModel_DisposeNativeControl(object? sender, NativeControlEventArgs e)
    {
        var viewModel = e.ViewModel;
        viewModel.DisposeNativeControl -= ViewModel_DisposeNativeControl;

        var existingInstance = GetControl(viewModel);
        if (existingInstance is null)
            return;
        
        _controls.Remove(viewModel);
        PInvoke.DestroyWindow(new HWND(existingInstance.Handle));
        existingInstance.Dispose();
    }
}