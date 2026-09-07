using Avalonia.Controls;
using Avalonia.Controls.Templates;
using RoyalApps.Community.Avalonia.Demo.ViewModels;
namespace RoyalApps.Community.Avalonia.Demo.Views;
// A native host must detach before its owner changes. Do not recycle the view across tabs.
public sealed class NativeTabViewLocator : IDataTemplate
{
    public bool Match(object? data) => data is NativeTabViewModel;
    public Control Build(object? data) => new TestView();
}
