using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace RoyalApps.Community.Avalonia.Windows.NativeControls;

internal class WinFormsLifetimeManager
{
    private static readonly Lazy<WinFormsLifetimeManager> SingletonInstance = new (() => new WinFormsLifetimeManager(), LazyThreadSafetyMode.ExecutionAndPublication);
    public static WinFormsLifetimeManager Instance => SingletonInstance.Value;
    private readonly Dictionary<IDisposeWinFormsControl, Control> _controls = new();
    private readonly Dictionary<IDisposeWinFormsControl, HostedControlSite> _hosts = new();
    private Form? _parkingWindow;
    private Panel? _parkingHost;

    private WinFormsLifetimeManager() { }

    public void AddControl(IDisposeWinFormsControl viewModel, Control control)
    {
        _controls[viewModel] = control;
        _hosts[viewModel] = new HostedControlSite(control);
        viewModel.DisposeWinFormsControl += DisposeWinFormsControl;
    }

    public Control? GetControl(IDisposeWinFormsControl? viewModel) => viewModel == null
        ? null
        : _controls.GetValueOrDefault(viewModel);

    public Control? GetHost(IDisposeWinFormsControl? viewModel) => viewModel == null
        ? null
        : _hosts.GetValueOrDefault(viewModel);

    public Control? AttachHost(IDisposeWinFormsControl viewModel, HWND hostHandle)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        if (!TryGetPersistentHost(viewModel, out var persistentHost))
            return null;

        if (persistentHost.Parent is { } currentParent)
        {
            currentParent.Controls.Remove(persistentHost);
            persistentHost.Parent = null;
        }

        var attachedHost = new AttachedHostSite();
        attachedHost.SuspendLayout();
        attachedHost.Controls.Add(persistentHost);
        persistentHost.Dock = DockStyle.Fill;
        persistentHost.Visible = true;
        attachedHost.ResumeLayout(true);

        if (hostHandle == HWND.Null)
        {
            DetachPersistentHost(attachedHost, persistentHost);
            attachedHost.Dispose();
            return null;
        }

        _ = attachedHost.Handle;

        var hostControlHandle = new HWND(attachedHost.Handle);
        if (hostControlHandle == HWND.Null)
        {
            DetachPersistentHost(attachedHost, persistentHost);
            attachedHost.Dispose();
            return null;
        }

        if (PInvoke.GetParent(hostControlHandle) != hostHandle)
            PInvoke.SetParent(hostControlHandle, hostHandle);

        attachedHost.Bounds = new Rectangle(Point.Empty, GetParentClientSize(hostHandle, attachedHost.Size));
        attachedHost.Visible = true;
        attachedHost.Show();
        attachedHost.BringToFront();
        attachedHost.PerformLayout();
        attachedHost.Invalidate(true);
        attachedHost.Update();
        return attachedHost;
    }

    public void ParkHost(IDisposeWinFormsControl viewModel, Control attachedHost, HWND expectedCurrentParent)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(attachedHost);

        var parkingWindow = EnsureParkingWindow();
        var hostControlHandle = new HWND(attachedHost.Handle);
        if (hostControlHandle == HWND.Null)
            return;

        _hosts.TryGetValue(viewModel, out var persistentHost);

        if (expectedCurrentParent != HWND.Null && PInvoke.GetParent(hostControlHandle) != expectedCurrentParent)
        {
            DetachPersistentHost(attachedHost, persistentHost);
            attachedHost.Dispose();
            return;
        }

        if (persistentHost is null)
        {
            attachedHost.Dispose();
            return;
        }

        if (!ReferenceEquals(persistentHost.Parent, attachedHost))
        {
            DetachPersistentHost(attachedHost, persistentHost);
            attachedHost.Dispose();
            return;
        }

        _parkingHost ??= CreateParkingHost();

        parkingWindow.SuspendLayout();
        _parkingHost.SuspendLayout();

        if (_parkingHost.Parent is null)
            parkingWindow.Controls.Add(_parkingHost);

        attachedHost.Controls.Remove(persistentHost);
        persistentHost.Parent = _parkingHost;
        persistentHost.Dock = DockStyle.Fill;
        persistentHost.Visible = true;
        persistentHost.Show();
        persistentHost.BringToFront();

        _parkingHost.ResumeLayout(true);
        parkingWindow.ResumeLayout(true);
        attachedHost.Dispose();
    }

    private bool TryGetPersistentHost(IDisposeWinFormsControl viewModel, out HostedControlSite persistentHost)
    {
        if (_hosts.TryGetValue(viewModel, out persistentHost!))
        {
            if (!persistentHost.IsDisposed)
                return true;

            _hosts.Remove(viewModel);
        }

        if (!_controls.TryGetValue(viewModel, out var control) || control.IsDisposed)
        {
            persistentHost = null!;
            return false;
        }

        persistentHost = new HostedControlSite(control);
        _hosts[viewModel] = persistentHost;
        return true;
    }

    private static void DetachPersistentHost(Control attachedHost, HostedControlSite? persistentHost)
    {
        if (persistentHost is null || persistentHost.IsDisposed)
            return;

        if (!ReferenceEquals(persistentHost.Parent, attachedHost))
            return;

        attachedHost.Controls.Remove(persistentHost);
        persistentHost.Parent = null;
    }

    private void DisposeWinFormsControl(object? sender, WinFormsDisposeEventArgs e)
    {
        var viewModel = e.ViewModel;
        viewModel.DisposeWinFormsControl -= DisposeWinFormsControl;

        var existingHost = GetHost(viewModel);
        if (existingHost is null)
            return;

        _controls.Remove(viewModel);
        _hosts.Remove(viewModel);

        existingHost.Dispose();

        if (_controls.Count == 0)
        {
            _parkingHost?.Dispose();
            _parkingHost = null;
            _parkingWindow?.Dispose();
            _parkingWindow = null;
        }
    }

    private Form EnsureParkingWindow()
    {
        if (_parkingWindow is not null && !_parkingWindow.IsDisposed)
            return _parkingWindow;

        _parkingWindow = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000),
            Size = new Size(1, 1),
            Opacity = 0,
            Text = "Hidden Host"
        };

        _ = _parkingWindow.Handle;
        return _parkingWindow;
    }

    private static Panel CreateParkingHost() =>
        new()
        {
            Dock = DockStyle.Fill
        };

    private static Size GetParentClientSize(HWND parentHandle, Size fallback)
    {
        if (parentHandle == HWND.Null || !PInvoke.GetClientRect(parentHandle, out var clientRect))
            return fallback;

        var width = Math.Max(1, clientRect.right - clientRect.left);
        var height = Math.Max(1, clientRect.bottom - clientRect.top);
        return new Size(width, height);
    }

    private sealed class HostedControlSite : Panel
    {
        public HostedControlSite(Control payload)
        {
            ArgumentNullException.ThrowIfNull(payload);

            SuspendLayout();
            Margin = default;
            Padding = default;
            payload.Parent = this;
            payload.Dock = DockStyle.Fill;
            ResumeLayout(true);
        }
    }

    private sealed class AttachedHostSite : Panel
    {
        public AttachedHostSite()
        {
            Margin = default;
            Padding = default;
        }
    }
}
