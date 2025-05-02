using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace InteropDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
