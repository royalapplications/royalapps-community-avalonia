using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaInterop.Test.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private TabViewModel? _selectedTab;
    public ObservableCollection<TabViewModel> Tabs { get; set; } = new();
    [RelayCommand] public void Add() => AddAndSelectTab(new TestViewModel());
    [RelayCommand] public void Close() => CloseAndSelectPreviousTab(SelectedTab);

    private void AddAndSelectTab(TabViewModel tab)
    {
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void CloseAndSelectPreviousTab(TabViewModel? tab)
    {
        if (tab is null)
            return;
        
        tab.RaiseTabClosing();
        Tabs.Remove(tab);
    }
}