using System.Collections.Generic;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

internal class WinFormsLifetimeManager
{
    private static WinFormsLifetimeManager? _instance;
    public static WinFormsLifetimeManager Instance => _instance ??= new WinFormsLifetimeManager();

    private readonly Dictionary<IDisposeWinFormsControl, Control> _controls = new();

    private WinFormsLifetimeManager() { }

    public void AddControl(IDisposeWinFormsControl viewModel, Control control)
    {
        _controls[viewModel] = control;
        viewModel.DisposeWinFormsControl += DisposeWinFormsControl;
    }

    public Control? GetControl(IDisposeWinFormsControl? viewModel)
    {
        if (viewModel == null)
            return null;
        
        return _controls.TryGetValue(viewModel, out var existingInstance) 
            ? existingInstance 
            : null;
    }
    
    private void DisposeWinFormsControl(object? sender, WinFormsDisposeEventArgs e)
    {
        var viewModel = e.ViewModel;
        viewModel.DisposeWinFormsControl -= DisposeWinFormsControl;

        var existingInstance = GetControl(viewModel);
        if (existingInstance is null)
            return;
        
        _controls.Remove(viewModel);
        
        PInvoke.DestroyWindow(new HWND(existingInstance.Handle));
        existingInstance.Dispose();
    }
}