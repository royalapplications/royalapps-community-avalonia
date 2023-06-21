using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaInterop.Test.ViewModels;

namespace AvaloniaInterop.Test;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}