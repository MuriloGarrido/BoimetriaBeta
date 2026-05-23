using BoimetriaBeta.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoimetriaBeta.ViewModels;

public sealed partial class SettingsViewModel : BaseViewModel
{
    static readonly Brush ReadyChipBrush = new SolidColorBrush(Color.FromArgb("#1B5E20"));
    static readonly Brush PendingChipBrush = new SolidColorBrush(Color.FromArgb("#BDBDBD"));

    readonly ISettingsService _settings;
    readonly IModelService _model;
    readonly IDialogService _dialog;

    public SettingsViewModel(ISettingsService settings, IModelService model, IDialogService dialog)
    {
        _settings = settings;
        _model = model;
        _dialog = dialog;
        Refresh();
    }

    [ObservableProperty]
    string modelFileName = "Sistema ainda não preparado";

    [ObservableProperty]
    string modelStatus = "Toque em \"Carregar arquivo\" abaixo";

    [ObservableProperty]
    bool hasModel;

    [ObservableProperty]
    string statusChipText = "PENDENTE";

    [ObservableProperty]
    Brush statusChipBrush = PendingChipBrush;

    public void Refresh()
    {
        HasModel = _settings.HasModel;
        if (HasModel && _settings.ModelPath is { } path)
        {
            ModelFileName = Path.GetFileName(path);
            ModelStatus = "Tá tudo certo, pode usar o app";
            StatusChipText = "PRONTO";
            StatusChipBrush = ReadyChipBrush;
        }
        else
        {
            ModelFileName = "Sistema ainda não preparado";
            ModelStatus = "Toque em \"Carregar arquivo\" abaixo";
            StatusChipText = "PENDENTE";
            StatusChipBrush = PendingChipBrush;
        }
    }

    [RelayCommand]
    Task PickModelAsync() => RunBusyAsync("Carregando arquivo...", async () =>
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Escolha o arquivo enviado pelo técnico",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/octet-stream", "*/*" } },
                    { DevicePlatform.iOS, new[] { "public.data", "public.item" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.data" } },
                    { DevicePlatform.WinUI, new[] { ".onnx" } },
                }),
            };

            var file = await FilePicker.Default.PickAsync(options);
            if (file is null) return;

            if (!file.FileName.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase))
            {
                await _dialog.AlertAsync(
                    "Arquivo diferente",
                    "Esse arquivo não parece ser o certo. O arquivo do sistema termina com .onnx. Peça o arquivo correto pro responsável técnico.");
                return;
            }

            var destination = Path.Combine(FileSystem.AppDataDirectory, "model.onnx");
            await using (var source = await file.OpenReadAsync())
            await using (var target = File.Create(destination))
            {
                await source.CopyToAsync(target);
            }

            _settings.SetModelPath(destination);
            await _model.ReloadAsync();
            Refresh();

            await _dialog.AlertAsync("Pronto!", "Sistema preparado. Agora pode ir em Identificar.");
        }
        catch (Exception ex)
        {
            await _dialog.AlertAsync("Não deu certo", ex.Message);
        }
    });

    [RelayCommand]
    async Task ClearModelAsync()
    {
        if (!await _dialog.ConfirmAsync(
                "Tirar o arquivo?",
                "Se você tirar, o app não vai mais identificar boi até carregar de novo."))
            return;

        _settings.ClearModelPath();
        Refresh();
    }
}
