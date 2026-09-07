using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RoyalApps.Community.Avalonia.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
