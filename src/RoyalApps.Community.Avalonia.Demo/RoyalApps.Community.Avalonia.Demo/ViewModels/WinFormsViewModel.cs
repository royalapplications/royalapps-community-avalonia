using System;
using CommunityToolkit.Mvvm.Input;
#if WINDOWS
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RoyalApps.Community.Avalonia.Windows.NativeControls;
#endif
namespace RoyalApps.Community.Avalonia.Demo.ViewModels;
public partial class WinFormsViewModel : SampleViewModel, IDisposable
{
    public override string Title => "WinFormsControlHost";
    public override string Category => "Windows interop";
    public override string Description => "Native Windows Forms controls retain their identity and text across tab and page changes. Closing a tab explicitly disposes its control.";
#if WINDOWS
    public bool IsAvailable => OperatingSystem.IsWindows();
    private int _counter;
    public ObservableCollection<NativeTabViewModel> Tabs { get; } = [];
    [ObservableProperty] private NativeTabViewModel? _selectedTab;
    public WinFormsViewModel() { AddTab(); AddTab(); SelectedTab = Tabs[0]; }
    [RelayCommand] private void AddTab()
    {
        var tab = new NativeTabViewModel($"Sample {++_counter}", this);
        Tabs.Add(tab);
        SelectedTab = tab;
    }
    public void CloseTab(NativeTabViewModel tab)
    {
        var index = Tabs.IndexOf(tab);
        if (index < 0) return;
        Tabs.Remove(tab);
        if (SelectedTab == tab || SelectedTab is null)
            SelectedTab = Tabs.Count == 0 ? null : Tabs[Math.Min(index, Tabs.Count - 1)];
        tab.Dispose();
    }
    public void Dispose() { SelectedTab = null; foreach (var tab in Tabs) tab.Dispose(); Tabs.Clear(); }
    public override void Reset() { Dispose(); _counter = 0; AddTab(); AddTab(); SelectedTab = Tabs[0]; }
#else
    public bool IsAvailable => false;
    public void Dispose() { }
    public override void Reset() { }
#endif
    public override string Code => """
        <!-- Windows target; xmlns:native="using:RoyalApps.Community.Avalonia.Windows.NativeControls"
             xmlns:forms="using:RoyalApps.Community.Avalonia.Demo.WinForms" -->
        <native:WinFormsControlHost x:TypeArguments="forms:TestControl" />

        // Each host's DataContext owns the native control lifetime:
        public sealed class TabOwner : IDisposeWinFormsControl
        {
            public event EventHandler<WinFormsDisposeEventArgs>? DisposeWinFormsControl;
            public void Close() => DisposeWinFormsControl?.Invoke(
                this, new WinFormsDisposeEventArgs(this));
        }
        // Keep the owner alive when navigating; call Close when permanently finished.
        // IDisposeWinFormsControl and WinFormsDisposeEventArgs are in
        // RoyalApps.Community.Avalonia.Windows.NativeControls.
        """;
}
#if WINDOWS
public sealed partial class NativeTabViewModel(string caption, WinFormsViewModel owner) : ViewModelBase, IDisposeWinFormsControl, IDisposable
{
    private bool _disposed;
    public string Caption { get; } = caption;
    public event EventHandler<WinFormsDisposeEventArgs>? DisposeWinFormsControl;
    [RelayCommand] private void Close() => owner.CloseTab(this);
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeWinFormsControl?.Invoke(this, new WinFormsDisposeEventArgs(this));
    }
}
#endif
