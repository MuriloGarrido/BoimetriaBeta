using System.Globalization;
using BoimetriaBeta.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoimetriaBeta.ViewModels;

public sealed partial class MainViewModel : BaseViewModel
{
    static readonly Brush ReadyBrush = new SolidColorBrush(Color.FromArgb("#1B5E20"));
    static readonly Brush PendingBrush = new SolidColorBrush(Color.FromArgb("#BDBDBD"));
    static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    readonly ISettingsService _settings;
    readonly IDialogService _dialog;

    public MainViewModel(ISettingsService settings, IDialogService dialog)
    {
        _settings = settings;
        _dialog = dialog;
        Refresh();
    }

    [ObservableProperty]
    string greeting = string.Empty;

    [ObservableProperty]
    string todayLabel = string.Empty;

    [ObservableProperty]
    string systemStatus = string.Empty;

    [ObservableProperty]
    string systemDetail = string.Empty;

    [ObservableProperty]
    bool isReady;

    [ObservableProperty]
    Brush statusBrush = PendingBrush;

    [ObservableProperty]
    string statusBadgeText = "OFFLINE";

    public string AppVersion =>
        $"Versão {AppInfo.Current.VersionString} · beta";

    public void Refresh()
    {
        var now = DateTime.Now;
        Greeting = GreetingFor(now.Hour);
        TodayLabel = Capitalize(now.ToString("dddd, dd 'de' MMMM", PtBr));

        IsReady = _settings.HasModel;
        if (IsReady)
        {
            SystemStatus = "Sistema pronto pra usar";
            SystemDetail = "Pode começar a identificar e fazer o manejo";
            StatusBrush = ReadyBrush;
            StatusBadgeText = "ONLINE";
        }
        else
        {
            SystemStatus = "Sistema precisa ser preparado";
            SystemDetail = "Toque aqui e carregue o arquivo do técnico";
            StatusBrush = PendingBrush;
            StatusBadgeText = "OFFLINE";
        }

        OnPropertyChanged(nameof(AppVersion));
    }

    [RelayCommand]
    Task OpenIdentificationAsync() => Shell.Current.GoToAsync("//identificar");

    [RelayCommand]
    Task OpenReportsAsync() => Shell.Current.GoToAsync("//relatorios");

    [RelayCommand]
    Task OpenSettingsAsync() => Shell.Current.GoToAsync("//ajustes");

    [RelayCommand]
    Task ManejoSoonAsync(string? feature) => _dialog.AlertAsync(
        "Em breve",
        $"\"{feature ?? "Esta função"}\" entra numa próxima versão. Por enquanto você já pode identificar os boi pelo focinho — aí, quando o manejo estiver pronto, é só apertar.",
        "Beleza");

    static string GreetingFor(int hour) => hour switch
    {
        < 12 => "Bom dia, parceiro",
        < 18 => "Boa tarde, parceiro",
        _ => "Boa noite, parceiro",
    };

    static string Capitalize(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpper(value[0], PtBr) + value[1..];
}
