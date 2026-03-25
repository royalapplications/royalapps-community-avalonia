using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Windows.Win32.Foundation;
using Control = System.Windows.Forms.Control;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

/// <summary>
/// A NativeControlHost descendent which creates a WinForms control and keeps it alive and in memory,
/// even when the hosting view is detached from the visual tree. Destruction/disposal of the control
/// has to be invoked manually by implementing the the IDisposeWinFormsControl interface on your
/// view model and raising the DisposeWinFormsControl event.
/// </summary>
/// <inheritdoc cref="NativeControlHost"/>
/// <typeparam name="T">The type of the WinForms control you want to host.</typeparam>
public class WinFormsControlHost<T> : NativeControlHost where T : Control
{
    private HWND _hostParentHandle = HWND.Null;
    private Control? _attachedHost;

    /// <summary>
    /// The WinForms control.
    /// </summary>
    public T? Control => WinFormsLifetimeManager.Instance.GetControl(DataContext as IDisposeWinFormsControl) as T;

    /// <inheritdoc cref="CreateNativeControlCore"/>
    protected sealed override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _hostParentHandle = new HWND(parent.Handle);

        // data context object is used as key in dictionary to locate existing control instances
        if (DataContext is not IDisposeWinFormsControl viewModel)
            throw new InvalidOperationException($"{nameof(DataContext)} must implement {nameof(IDisposeWinFormsControl)}.");

        if (WinFormsLifetimeManager.Instance.GetHost(viewModel) is null)
        {
            var newInstance = OnCreateWinFormsControl() ?? Activator.CreateInstance<T>();
            WinFormsLifetimeManager.Instance.AddControl(viewModel, newInstance);
        }

        _attachedHost = WinFormsLifetimeManager.Instance.AttachHost(viewModel, _hostParentHandle)
            ?? throw new InvalidOperationException("Failed to attach a hosted WinForms site.");
        UpdateAttachedHostBounds(GetHostedLogicalSize(Bounds.Size));

        return new PlatformHandle(_attachedHost.Handle, "Hndl");
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var measured = base.MeasureOverride(availableSize);

        var width = ResolveExplicitLength(Width, MaxWidth, availableSize.Width);
        var height = ResolveExplicitLength(Height, MaxHeight, availableSize.Height);

        if (width is null && height is null)
            return measured;

        return new Size(
            width ?? measured.Width,
            height ?? measured.Height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        UpdateAttachedHostBounds(GetHostedLogicalSize(finalSize));
        return arranged;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WidthProperty ||
            change.Property == HeightProperty ||
            change.Property == MaxWidthProperty ||
            change.Property == MaxHeightProperty)
        {
            UpdateAttachedHostBounds(GetHostedLogicalSize(Bounds.Size));
        }
    }

    /// <summary>
    /// Override this method to create the WinForms control instance yourself.
    /// If not implemented, the instance will be created automatically.
    /// </summary>
    /// <returns>An instance of the WinForms control to host.</returns>
    protected virtual T? OnCreateWinFormsControl() => null;

    /// <inheritdoc cref="DestroyNativeControlCore"/>
    protected sealed override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (DataContext is IDisposeWinFormsControl viewModel && _attachedHost is not null)
        {
            WinFormsLifetimeManager.Instance.ParkHost(viewModel, _attachedHost, _hostParentHandle);
        }

        _attachedHost = null;
        _hostParentHandle = HWND.Null;
    }

    private void UpdateAttachedHostBounds(Size logicalSize)
    {
        if (_attachedHost is null)
            return;

        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? VisualRoot?.RenderScaling ?? 1D;
        var pixelWidth = Math.Max(1, (int)Math.Ceiling(logicalSize.Width * scaling));
        var pixelHeight = Math.Max(1, (int)Math.Ceiling(logicalSize.Height * scaling));

        if (_attachedHost.Bounds.Width == pixelWidth && _attachedHost.Bounds.Height == pixelHeight)
            return;

        _attachedHost.Bounds = new System.Drawing.Rectangle(
            System.Drawing.Point.Empty,
            new System.Drawing.Size(pixelWidth, pixelHeight));
    }

    private Size GetHostedLogicalSize(Size fallback)
    {
        var width = ResolveExplicitLength(Width, MaxWidth, fallback.Width) ?? fallback.Width;
        var height = ResolveExplicitLength(Height, MaxHeight, fallback.Height) ?? fallback.Height;
        return new Size(width, height);
    }

    private static double? ResolveExplicitLength(double value, double maxValue, double available)
    {
        var hasValue = !double.IsNaN(value);
        var candidate = hasValue ? value : double.NaN;

        if (!double.IsNaN(maxValue))
            candidate = hasValue ? Math.Min(candidate, maxValue) : maxValue;

        if (double.IsNaN(candidate))
            return null;

        if (!double.IsInfinity(available))
            candidate = Math.Min(candidate, available);

        return Math.Max(0D, candidate);
    }
}
