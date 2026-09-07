using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RoyalApps.Community.Avalonia.Demo.ViewModels;

public partial class UiThreadDemoViewModel : ViewModelBase
{
    [ObservableProperty] private string _status = "Ready. Compare the animations while the UI thread is blocked.";

    [RelayCommand]
    private async Task BlockUiThreadAsync()
    {
        Dispatcher.UIThread.VerifyAccess();
        Status = "Starting in one second...";
        await Task.Delay(1000);
        Status = "UI thread blocked for 5 seconds. Watch the animations above.";
        // Let the status and disabled button render before deliberately freezing the dispatcher.
        await Task.Delay(200);
        try
        {
            // Deliberately on the dispatcher: Task.Run would not demonstrate UI-thread blocking.
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
        finally
        {
            Status = "UI thread resumed. The standard progress bar can animate again.";
        }
    }
}
