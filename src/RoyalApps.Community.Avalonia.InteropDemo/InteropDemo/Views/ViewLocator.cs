using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using InteropDemo.ViewModels;

namespace InteropDemo.Views;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data!.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type == null) 
            return new TextBlock {Text = "Not Found: " + name};
        
        var view = (Control)Activator.CreateInstance(type)!; // Create View
        return view;
    }

    public bool Match(object? data) => data is ViewModelBase;
}