using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaInterop.Test.ViewModels;

namespace AvaloniaInterop.Test.Views;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data!.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type == null) 
            return new TextBlock {Text = "Not Found: " + name};
        
        var view = (Control)Activator.CreateInstance(type)!; // Create View
        view.DataContext = data; // Set ViewModel as DataContext
        return view;
    }

    public bool Match(object? data) => data is ViewModelBase;
}