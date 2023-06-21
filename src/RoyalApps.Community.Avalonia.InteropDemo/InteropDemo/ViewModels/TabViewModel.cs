using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoyalApps.Community.Avalonia.Windows.NativeControls;

namespace AvaloniaInterop.Test.ViewModels;

public partial class TabViewModel : ViewModelBase, IDisposeNativeControl
{
    [ObservableProperty] private string _caption = "n/a";

    public event EventHandler<NativeControlEventArgs>? DisposeNativeControl;

    [RelayCommand] public void Close() => App.MainViewModel.CloseAndSelectPreviousTab(this);

    public void RaiseTabClosing()
    {
        DisposeNativeControl?.Invoke(this, new NativeControlEventArgs(this));
    }
}