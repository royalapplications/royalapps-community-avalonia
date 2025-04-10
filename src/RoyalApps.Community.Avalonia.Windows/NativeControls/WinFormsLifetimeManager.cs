using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

internal class WinFormsLifetimeManager
{
    private static readonly Lazy<WinFormsLifetimeManager> _instance = new (() => new WinFormsLifetimeManager(), LazyThreadSafetyMode.ExecutionAndPublication);
    public static WinFormsLifetimeManager Instance => _instance.Value;
    private readonly Dictionary<IDisposeWinFormsControl, Control> _controls = new();

    private WinFormsLifetimeManager() { }

    public void AddControl(IDisposeWinFormsControl viewModel, Control control)
    {
        _controls[viewModel] = control;
        viewModel.DisposeWinFormsControl += DisposeWinFormsControl;
    }

    public Control? GetControl(IDisposeWinFormsControl? viewModel) => viewModel == null 
        ? null 
        : _controls.GetValueOrDefault(viewModel);

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