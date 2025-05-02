using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using InteropDemo.ViewModels;

namespace InteropDemo;

public class App : Application
{
    public static MainViewModel MainViewModel;

    static App()
    {
        MainViewModel = new MainViewModel();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = MainViewModel
            };
        }

        if (ApplicationLifetime is ISingleViewApplicationLifetime singleLifetime)
        {
            singleLifetime.MainView = new MainView
            {
                DataContext = MainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
