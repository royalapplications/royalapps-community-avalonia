using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using RoyalApps.Community.Avalonia.Common.Behaviors;
using Xunit;

namespace RoyalApps.Community.Avalonia.Common.Tests.Behaviors;

public sealed class GridSplitterBehaviorTests
{
    [AvaloniaFact]
    public void DoubleTapped_EqualizesStarColumns()
    {
        var (grid, splitter) = CreateColumnGrid(3, 1);
        GridSplitterBehavior.SetEqualizeOnDoubleTapped(splitter, true);
        var window = Show(grid, 406, 200);

        RaiseDoubleTapped(window, splitter);
        Dispatcher.UIThread.RunJobs();

        AssertClose(grid.ColumnDefinitions[0].ActualWidth, grid.ColumnDefinitions[2].ActualWidth);
        AssertClose(200, grid.ColumnDefinitions[0].ActualWidth);
        window.Close();
    }

    [AvaloniaFact]
    public void DoubleTapped_ResolvesAutomaticRowDirectionAndBehavior()
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(CreateStarRow(3));
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6) });
        grid.RowDefinitions.Add(CreateStarRow(1));

        var splitter = new GridSplitter
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            ResizeDirection = GridResizeDirection.Auto,
            ResizeBehavior = GridResizeBehavior.BasedOnAlignment,
        };
        Grid.SetRow(splitter, 1);
        grid.Children.Add(splitter);
        GridSplitterBehavior.SetEqualizeOnDoubleTapped(splitter, true);
        var window = Show(grid, 200, 406);

        RaiseDoubleTapped(window, splitter);
        Dispatcher.UIThread.RunJobs();

        AssertClose(grid.RowDefinitions[0].ActualHeight, grid.RowDefinitions[2].ActualHeight);
        AssertClose(200, grid.RowDefinitions[0].ActualHeight);
        window.Close();
    }

    [AvaloniaFact]
    public void DoubleTapped_UsesNearestEqualSplitAllowedByConstraints()
    {
        var (grid, splitter) = CreateColumnGrid(3, 1);
        grid.ColumnDefinitions[0].MinWidth = 260;
        GridSplitterBehavior.SetEqualizeOnDoubleTapped(splitter, true);
        var window = Show(grid, 406, 200);

        RaiseDoubleTapped(window, splitter);
        Dispatcher.UIThread.RunJobs();

        AssertClose(260, grid.ColumnDefinitions[0].ActualWidth);
        AssertClose(140, grid.ColumnDefinitions[2].ActualWidth);
        window.Close();
    }

    [AvaloniaFact]
    public void DoubleTapped_DoesNothingAfterBehaviorIsDisabled()
    {
        var (grid, splitter) = CreateColumnGrid(3, 1);
        GridSplitterBehavior.SetEqualizeOnDoubleTapped(splitter, true);
        GridSplitterBehavior.SetEqualizeOnDoubleTapped(splitter, false);
        var window = Show(grid, 406, 200);

        var eventArgs = RaiseDoubleTapped(window, splitter);
        Dispatcher.UIThread.RunJobs();

        Assert.False(eventArgs.Handled);
        AssertClose(300, grid.ColumnDefinitions[0].ActualWidth);
        AssertClose(100, grid.ColumnDefinitions[2].ActualWidth);
        window.Close();
    }

    [AvaloniaFact]
    public void DoubleTapped_DoesNothingForNonStarPair()
    {
        var (grid, splitter) = CreateColumnGrid(3, 1);
        grid.ColumnDefinitions[0].Width = new GridLength(100);
        GridSplitterBehavior.SetEqualizeOnDoubleTapped(splitter, true);
        var window = Show(grid, 406, 200);

        var eventArgs = RaiseDoubleTapped(window, splitter);
        Dispatcher.UIThread.RunJobs();

        Assert.False(eventArgs.Handled);
        Assert.Equal(GridUnitType.Pixel, grid.ColumnDefinitions[0].Width.GridUnitType);
        Assert.Equal(GridUnitType.Star, grid.ColumnDefinitions[2].Width.GridUnitType);
        window.Close();
    }

    [AvaloniaFact]
    public void DoubleTapped_DoesNothingWithoutGridParent()
    {
        var splitter = new GridSplitter();
        GridSplitterBehavior.SetEqualizeOnDoubleTapped(splitter, true);
        var window = Show(splitter, 200, 50);

        var exception = Record.Exception(() => RaiseDoubleTapped(window, splitter));

        Assert.Null(exception);
        window.Close();
    }

    private static void AssertClose(double expected, double actual) =>
        Assert.InRange(Math.Abs(expected - actual), 0, 0.01);

    private static (Grid Grid, GridSplitter Splitter) CreateColumnGrid(
        double firstStar,
        double secondStar)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(CreateStarColumn(firstStar));
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
        grid.ColumnDefinitions.Add(CreateStarColumn(secondStar));

        var splitter = new GridSplitter
        {
            ResizeDirection = GridResizeDirection.Columns,
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };
        Grid.SetColumn(splitter, 1);
        grid.Children.Add(splitter);
        return (grid, splitter);
    }

    private static ColumnDefinition CreateStarColumn(double value) =>
        new() { Width = new GridLength(value, GridUnitType.Star) };

    private static RowDefinition CreateStarRow(double value) =>
        new() { Height = new GridLength(value, GridUnitType.Star) };

    private static TappedEventArgs RaiseDoubleTapped(Window window, GridSplitter splitter)
    {
        var point = new Point(
            splitter.Bounds.X + (splitter.Bounds.Width / 2),
            splitter.Bounds.Y + (splitter.Bounds.Height / 2));
        using var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
#pragma warning disable CS0618 // The public constructor is required to provide a pointer payload to TappedEventArgs.
        var pointerEventArgs = new PointerEventArgs(
            InputElement.PointerPressedEvent,
            splitter,
            pointer,
            window,
            point,
            0,
            new PointerPointProperties(),
            KeyModifiers.None);
#pragma warning restore CS0618
        var eventArgs = new TappedEventArgs(InputElement.DoubleTappedEvent, pointerEventArgs);
        splitter.RaiseEvent(eventArgs);
        return eventArgs;
    }

    private static Window Show(Control content, double width, double height)
    {
        var window = new Window
        {
            Content = content,
            Width = width,
            Height = height,
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
