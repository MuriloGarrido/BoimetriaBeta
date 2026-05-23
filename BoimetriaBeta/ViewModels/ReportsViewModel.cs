using BoimetriaBeta.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoimetriaBeta.ViewModels;

public sealed partial class ReportsViewModel : BaseViewModel
{
    readonly IDialogService _dialog;

    public ReportsViewModel(IDialogService dialog) => _dialog = dialog;

    [ObservableProperty]
    string selectedRange = "Esta semana";

    [ObservableProperty]
    string managementsCount = "0";

    [ObservableProperty]
    string animalsCount = "0";

    [ObservableProperty]
    string pendingCount = "0";

    [RelayCommand]
    void SelectRange(string range)
    {
        if (!string.IsNullOrWhiteSpace(range))
            SelectedRange = range;
    }

    [RelayCommand]
    Task SoonAsync(string? feature)
        => _dialog.AlertAsync(
            "Em breve",
            $"\"{feature ?? "Esse relatório"}\" entra numa próxima versão. Por enquanto você já pode identificar os boi pelo focinho.",
            "Beleza");
}
