using CommunityToolkit.Mvvm.ComponentModel;

namespace BoimetriaBeta.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    bool isBusy;

    [ObservableProperty]
    string? busyMessage;

    protected async Task RunBusyAsync(string message, Func<Task> work)
    {
        if (IsBusy) return;
        try
        {
            BusyMessage = message;
            IsBusy = true;
            await work();
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }
}
