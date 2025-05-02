# Utilities and Helpers for AvaloniaUI

[![NuGet Version](https://img.shields.io/nuget/v/RoyalApps.Community.Avalonia.Windows.svg?style=flat)](https://www.nuget.org/packages/RoyalApps.Community.Avalonia.Windows)
[![NuGet Downloads](https://img.shields.io/nuget/dt/RoyalApps.Community.Avalonia.Windows.svg?color=green)](https://www.nuget.org/packages/RoyalApps.Community.Avalonia.Windows)
[![.NET](https://img.shields.io/badge/.NET-%3E%3D%20%207.0-blueviolet)](https://dotnet.microsoft.com/download)

# RoyalApps.Community.Avalonia
RoyalApps.Community.Avalonia contains projects/packages for AvaloniaUI.

## RoyalApps.Community.Avalonia.Windows
This package contains a WinFormsControlHost with a custom lifecycle management. XAML based controls like the TabControl in Avalonia, detach and attach views dynamically when switching between tabs. In general, this is a good approach to make rendering and resource utilization efficient.

Putting a NativeControlHost in such a view can cause issues because every time the view gets detached and another one attached, the native control is destroyed and recreated. If you want to host legacy WinForms controls, for example, you might want to have more control over the lifetime of your user control.

![InteropDemo](https://raw.githubusercontent.com/royalapplications/royalapps-community-avalonia/main/docs/assets/InteropDemo.gif)

The demo application as shown above, creates a view with a WinFormsControlHost for each tab. The WinForms control (also part of the demo app), simply contains a text box, prints an "instance id" and has a random background color to demonstrate that the instances "survive" and keep the state until manually disposed.

### Installation
Install the RoyalApps.Community.Avalonia.Windows with NuGet:
```
Install-Package RoyalApps.Community.Avalonia.Windows
```
or via the command line interface:
```
dotnet add package RoyalApps.Community.Avalonia.Windows
```

### Using the WinFormsNativeHost Control

#### Add the Control

You can find the `WinFormsControlHost` in the namespace `RoyalApps.Community.Avalonia.Windows.NativeControls`:
```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:winForms="clr-namespace:InteropDemo.WinForms;assembly=InteropDemo.WinForms"
             xmlns:nativeControls="clr-namespace:RoyalApps.Community.Avalonia.Windows.NativeControls;assembly=RoyalApps.Community.Avalonia.Windows"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="InteropDemo.Views.TestView"
             Padding="10">
    <nativeControls:WinFormsControlHost x:Name="WinFormsControlHost" x:TypeArguments="winForms:TestControl" />
</UserControl>
```

> **Note**
> Since the control is a generic type `WinFormsControlHost<T> where T : System.Windows.Forms.Control`, you need to specify the type of your WinForms control you want to host in the `x:TypeArguments` attribute.

By default, the `WinFormsControlHost<T>` creates a new instance of your type automatically and keeps track of the instance. You can subclass the control and override the `OnCreateWinFormsControl()` method to create and return the instance of the WinForms control yourself.

To configure your control (e.g. set properties), use the `OnLoaded()` override:
```csharp
protected override void OnLoaded()
{
    base.OnLoaded();

    if (WinFormsControlHost.Control is not { } testControl)
        return;
    testControl.BackColor = Color.White;
}
```

#### Manage the Lifetime of the Control
In the view model you use as data context where you placed the `WinFormsControlHost`, simply implement the `IDisposeWinFormsControl` interface:
```csharp
public partial class TabViewModel : ViewModelBase, IDisposeWinFormsControl
{
    [ObservableProperty] private string _caption = "n/a";

    public event EventHandler<WinFormsDisposeEventArgs>? DisposeWinFormsControl;

    [RelayCommand] public void Close() => App.MainViewModel.RemoveTab(this);

    public void RaiseTabClosing()
    {
        DisposeWinFormsControl?.Invoke(this, new WinFormsDisposeEventArgs(this));
    }
}
```
Simply invoke the `DisposeWinFormsControl` event and pass the instance of the view model to the `WinFormsDisposeEventArgs` constructor.

#### How does it work?
The singleton class `WinFormsLifetimeManager` keeps the instances of all WinForms controls in a dictionary as long as they are required (not manually disposed).

> **Note**
> The view model instance (which implements `IDisposeWinFormsControl`) will be used as `key` for the dictionary. Depending on your view model implementation, you might see issues with this approach when `GetHashcode` or `Equals` is overridden.

The `WinFormsControlHost` inherits from `NativeControlHost` and prevents the control from being destroyed. When an instance of the WinForms control is requested for a specific view model, you either get an existing instance if available, or an instance is created for you.

When you are done (e.g. the view model is getting removed for good), you invoke the `DisposeWinFormsControl` event on your view model to signal the `WinFormsLifetimeManager` the WinForms control can be destroyed/disposed.
