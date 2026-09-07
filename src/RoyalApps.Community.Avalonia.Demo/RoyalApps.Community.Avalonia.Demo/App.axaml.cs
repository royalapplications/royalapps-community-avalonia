using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RoyalApps.Community.Avalonia.Demo.ViewModels;
namespace RoyalApps.Community.Avalonia.Demo;
public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var model = new MainViewModel();
            desktop.MainWindow = new MainWindow { DataContext = model };
            desktop.MainWindow.Closed += (_, _) => model.Dispose();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
