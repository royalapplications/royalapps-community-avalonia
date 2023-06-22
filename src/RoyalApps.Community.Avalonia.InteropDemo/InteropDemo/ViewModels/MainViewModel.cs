using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InteropDemo.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private int _counter = 1;
    [ObservableProperty] private TabViewModel? _selectedTab;
    public ObservableCollection<TabViewModel> Tabs { get; set; } = new();
    [RelayCommand] public void Add() => AddTab(new TestViewModel());
    [RelayCommand] public void Remove() => RemoveTab(SelectedTab);

    private void AddTab(TabViewModel tab)
    {
        tab.Caption += $" {_counter++}";
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void RemoveTab(TabViewModel? tab)
    {
        if (tab is null)
            return;

        tab.RaiseTabClosing();
        Tabs.Remove(tab);
    }
}