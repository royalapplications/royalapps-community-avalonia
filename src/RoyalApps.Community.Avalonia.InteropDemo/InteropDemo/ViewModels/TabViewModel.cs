using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoyalApps.Community.Avalonia.Windows.NativeControls;

namespace InteropDemo.ViewModels;

public partial class TabViewModel : ViewModelBase, IDisposeWinFormsControl
{
    [ObservableProperty] private string _caption = "n/a";

    public event EventHandler<WinFormsDisposeEventArgs>? DisposeWinFormsControl;

    [RelayCommand] public void Close() => App.MainViewModel.RemoveTabCommand.Execute(this);

    public void RaiseTabClosing()
    {
        DisposeWinFormsControl?.Invoke(this, new WinFormsDisposeEventArgs(this));
    }
}