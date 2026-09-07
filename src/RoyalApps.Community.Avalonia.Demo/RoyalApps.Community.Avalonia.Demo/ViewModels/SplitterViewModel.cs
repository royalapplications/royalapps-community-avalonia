using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
namespace RoyalApps.Community.Avalonia.Demo.ViewModels;
public partial class SplitterViewModel : SampleViewModel
{
    public override string Title => "GridSplitterBehavior";
    public override string Category => "Behaviors";
    public override string Description => "Drag either splitter to resize its neighbors, then double-tap the divider to equalize the adjacent panes.";
    [ObservableProperty] private bool _equalizeEnabled = true;
    [ObservableProperty] private GridLength _leftWidth = new(2, GridUnitType.Star);
    [ObservableProperty] private GridLength _rightWidth = new(1, GridUnitType.Star);
    [ObservableProperty] private GridLength _topHeight = new(2, GridUnitType.Star);
    [ObservableProperty] private GridLength _bottomHeight = new(1, GridUnitType.Star);
    public override void Reset()
    {
        EqualizeEnabled = true;
        LeftWidth = TopHeight = new(2, GridUnitType.Star);
        RightWidth = BottomHeight = new(1, GridUnitType.Star);
    }
    public override string Code => """
        <!-- xmlns:behaviors="using:RoyalApps.Community.Avalonia.Common.Behaviors" -->
        <Grid ColumnDefinitions="2*,Auto,*">
            <TextBlock Text="Left pane" />
            <GridSplitter Grid.Column="1" Width="8" ResizeDirection="Columns"
                behaviors:GridSplitterBehavior.EqualizeOnDoubleTapped="True" />
            <TextBlock Grid.Column="2" Text="Right pane" />
        </Grid>
        <!-- For stacked panes use RowDefinitions and ResizeDirection="Rows". -->
        """;
}
