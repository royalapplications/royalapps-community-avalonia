using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace RoyalApps.Community.Avalonia.Common.Behaviors;

/// <summary>
/// Provides opt-in behaviors for <see cref="GridSplitter"/> controls.
/// </summary>
public sealed class GridSplitterBehavior : AvaloniaObject
{
    /// <summary>
    /// Defines the <c>EqualizeOnDoubleTapped</c> attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> EqualizeOnDoubleTappedProperty =
        AvaloniaProperty.RegisterAttached<GridSplitterBehavior, GridSplitter, bool>(
            "EqualizeOnDoubleTapped");

    static GridSplitterBehavior()
    {
        EqualizeOnDoubleTappedProperty.Changed.AddClassHandler<GridSplitter>(OnEqualizeOnDoubleTappedChanged);
    }

    private GridSplitterBehavior()
    {
    }

    /// <summary>
    /// Gets whether the two star-sized definitions controlled by the splitter are equalized when it is double-tapped.
    /// </summary>
    /// <param name="splitter">The splitter from which to read the value.</param>
    /// <returns>
    /// <see langword="true"/> when double-tapping equalizes the definitions; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool GetEqualizeOnDoubleTapped(GridSplitter splitter)
    {
        ArgumentNullException.ThrowIfNull(splitter);
        return splitter.GetValue(EqualizeOnDoubleTappedProperty);
    }

    /// <summary>
    /// Sets whether the two star-sized definitions controlled by the splitter are equalized when it is double-tapped.
    /// </summary>
    /// <param name="splitter">The splitter on which to set the value.</param>
    /// <param name="value">
    /// <see langword="true"/> to equalize the definitions on double-tap; otherwise, <see langword="false"/>.
    /// </param>
    public static void SetEqualizeOnDoubleTapped(GridSplitter splitter, bool value)
    {
        ArgumentNullException.ThrowIfNull(splitter);
        splitter.SetValue(EqualizeOnDoubleTappedProperty, value);
    }

    private static void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is GridSplitter splitter && TryEqualize(splitter))
            e.Handled = true;
    }

    private static void OnEqualizeOnDoubleTappedChanged(
        GridSplitter splitter,
        AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.NewValue is true)
            splitter.DoubleTapped += OnDoubleTapped;
        else
            splitter.DoubleTapped -= OnDoubleTapped;
    }

    private static GridResizeBehavior ResolveResizeBehavior(
        GridSplitter splitter,
        GridResizeDirection direction)
    {
        if (splitter.ResizeBehavior != GridResizeBehavior.BasedOnAlignment)
            return splitter.ResizeBehavior;

        return direction switch
        {
            GridResizeDirection.Columns when splitter.HorizontalAlignment == HorizontalAlignment.Left =>
                GridResizeBehavior.PreviousAndCurrent,
            GridResizeDirection.Columns when splitter.HorizontalAlignment == HorizontalAlignment.Right =>
                GridResizeBehavior.CurrentAndNext,
            GridResizeDirection.Rows when splitter.VerticalAlignment == VerticalAlignment.Top =>
                GridResizeBehavior.PreviousAndCurrent,
            GridResizeDirection.Rows when splitter.VerticalAlignment == VerticalAlignment.Bottom =>
                GridResizeBehavior.CurrentAndNext,
            _ => GridResizeBehavior.PreviousAndNext,
        };
    }

    private static GridResizeDirection ResolveResizeDirection(GridSplitter splitter)
    {
        if (splitter.ResizeDirection != GridResizeDirection.Auto)
            return splitter.ResizeDirection;

        if (splitter.HorizontalAlignment != HorizontalAlignment.Stretch)
            return GridResizeDirection.Columns;

        if (splitter.VerticalAlignment != VerticalAlignment.Stretch)
            return GridResizeDirection.Rows;

        return splitter.Bounds.Width <= splitter.Bounds.Height
            ? GridResizeDirection.Columns
            : GridResizeDirection.Rows;
    }

    private static bool TryCalculateDefinitionIndices(
        int splitterIndex,
        GridResizeBehavior behavior,
        int definitionCount,
        out int firstIndex,
        out int secondIndex)
    {
        switch (behavior)
        {
            case GridResizeBehavior.PreviousAndCurrent:
                firstIndex = splitterIndex - 1;
                secondIndex = splitterIndex;
                break;
            case GridResizeBehavior.CurrentAndNext:
                firstIndex = splitterIndex;
                secondIndex = splitterIndex + 1;
                break;
            default:
                firstIndex = splitterIndex - 1;
                secondIndex = splitterIndex + 1;
                break;
        }

        return firstIndex >= 0 && secondIndex < definitionCount;
    }

    private static bool TryCalculateEqualLengths(
        double totalLength,
        double firstMinimum,
        double firstMaximum,
        double secondMinimum,
        double secondMaximum,
        out double firstLength,
        out double secondLength)
    {
        firstLength = 0;
        secondLength = 0;

        if (!double.IsFinite(totalLength) || totalLength <= 0)
            return false;

        var minimumFirstLength = Math.Max(firstMinimum, totalLength - secondMaximum);
        var maximumFirstLength = Math.Min(firstMaximum, totalLength - secondMinimum);
        if (minimumFirstLength > maximumFirstLength)
            return false;

        firstLength = Math.Clamp(totalLength / 2, minimumFirstLength, maximumFirstLength);
        secondLength = totalLength - firstLength;
        return true;
    }

    private static bool TryEqualize(GridSplitter splitter)
    {
        if (splitter.Parent is not Grid grid)
            return false;

        var direction = ResolveResizeDirection(splitter);
        var span = direction == GridResizeDirection.Columns
            ? Grid.GetColumnSpan(splitter)
            : Grid.GetRowSpan(splitter);
        if (span != 1)
            return false;

        var splitterIndex = direction == GridResizeDirection.Columns
            ? Grid.GetColumn(splitter)
            : Grid.GetRow(splitter);
        var behavior = ResolveResizeBehavior(splitter, direction);

        return direction == GridResizeDirection.Columns
            ? TryEqualizeColumns(grid, splitterIndex, behavior)
            : TryEqualizeRows(grid, splitterIndex, behavior);
    }

    private static bool TryEqualizeColumns(
        Grid grid,
        int splitterIndex,
        GridResizeBehavior behavior)
    {
        if (!TryCalculateDefinitionIndices(
                splitterIndex,
                behavior,
                grid.ColumnDefinitions.Count,
                out var firstIndex,
                out var secondIndex))
        {
            return false;
        }

        var firstDefinition = grid.ColumnDefinitions[firstIndex];
        var secondDefinition = grid.ColumnDefinitions[secondIndex];
        if (!firstDefinition.Width.IsStar || !secondDefinition.Width.IsStar)
            return false;

        if (!TryCalculateEqualLengths(
                firstDefinition.ActualWidth + secondDefinition.ActualWidth,
                firstDefinition.MinWidth,
                firstDefinition.MaxWidth,
                secondDefinition.MinWidth,
                secondDefinition.MaxWidth,
                out var firstLength,
                out var secondLength))
        {
            return false;
        }

        for (var index = 0; index < grid.ColumnDefinitions.Count; index++)
        {
            var definition = grid.ColumnDefinitions[index];
            if (!definition.Width.IsStar)
                continue;

            var length = index switch
            {
                var current when current == firstIndex => firstLength,
                var current when current == secondIndex => secondLength,
                _ => definition.ActualWidth,
            };
            definition.SetCurrentValue(
                ColumnDefinition.WidthProperty,
                new GridLength(length, GridUnitType.Star));
        }

        return true;
    }

    private static bool TryEqualizeRows(
        Grid grid,
        int splitterIndex,
        GridResizeBehavior behavior)
    {
        if (!TryCalculateDefinitionIndices(
                splitterIndex,
                behavior,
                grid.RowDefinitions.Count,
                out var firstIndex,
                out var secondIndex))
        {
            return false;
        }

        var firstDefinition = grid.RowDefinitions[firstIndex];
        var secondDefinition = grid.RowDefinitions[secondIndex];
        if (!firstDefinition.Height.IsStar || !secondDefinition.Height.IsStar)
            return false;

        if (!TryCalculateEqualLengths(
                firstDefinition.ActualHeight + secondDefinition.ActualHeight,
                firstDefinition.MinHeight,
                firstDefinition.MaxHeight,
                secondDefinition.MinHeight,
                secondDefinition.MaxHeight,
                out var firstLength,
                out var secondLength))
        {
            return false;
        }

        for (var index = 0; index < grid.RowDefinitions.Count; index++)
        {
            var definition = grid.RowDefinitions[index];
            if (!definition.Height.IsStar)
                continue;

            var length = index switch
            {
                var current when current == firstIndex => firstLength,
                var current when current == secondIndex => secondLength,
                _ => definition.ActualHeight,
            };
            definition.SetCurrentValue(
                RowDefinition.HeightProperty,
                new GridLength(length, GridUnitType.Star));
        }

        return true;
    }
}
