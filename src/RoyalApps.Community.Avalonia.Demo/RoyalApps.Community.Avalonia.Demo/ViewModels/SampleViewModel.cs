using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
namespace RoyalApps.Community.Avalonia.Demo.ViewModels;
public abstract partial class SampleViewModel : ViewModelBase
{
    public abstract string Title { get; }
    public abstract string Category { get; }
    public virtual bool StartsCategory => true;
    public abstract string Description { get; }
    public abstract string Code { get; }
    public IReadOnlyList<string> ColorPresets { get; } = ["DodgerBlue", "MediumPurple", "SeaGreen", "OrangeRed"];
    protected static Color ParseColor(string value) => Color.Parse(value);
    [RelayCommand] public abstract void Reset();
}
