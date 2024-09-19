using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InteropDemo.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private int _counter = 1;
    [ObservableProperty] private TabViewModel? _selectedTab;
    [ObservableProperty] private IReadOnlyList<WindowTransparencyLevel> _transparency = [WindowTransparencyLevel.Mica, WindowTransparencyLevel.Transparent
    ];
    public ObservableCollection<TabViewModel> Tabs { get; set; } = new();

    [RelayCommand]
    private void AddTab(TabViewModel? tab)
    {
        tab ??= new TestViewModel();
        tab.Caption += $" {_counter++}";
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private void RemoveTab(TabViewModel? tab)
    {
        if (tab is null)
            return;

        tab.RaiseTabClosing();
        Tabs.Remove(tab);
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        if (Application.Current is null)
            return;
        
        Application.Current.RequestedThemeVariant = Application.Current.ActualThemeVariant == ThemeVariant.Light
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
    }

    [RelayCommand]
    private void ToggleMica()
    {
        Transparency = Transparency.Contains(WindowTransparencyLevel.Mica)
            ? []
            : new[] { WindowTransparencyLevel.Mica };
    }
}