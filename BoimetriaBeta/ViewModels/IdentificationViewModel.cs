using BoimetriaBeta.Models;
using BoimetriaBeta.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoimetriaBeta.ViewModels;

public sealed partial class IdentificationViewModel : BaseViewModel
{
    const string CachePrefix = "boimetria_";

    readonly IImagePickerService _picker;
    readonly IModelService _model;
    readonly IImageProcessingService _imaging;
    readonly IDialogService _dialog;

    public IdentificationViewModel(
        IImagePickerService picker,
        IModelService model,
        IImageProcessingService imaging,
        IDialogService dialog)
    {
        _picker = picker;
        _model = model;
        _imaging = imaging;
        _dialog = dialog;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    ImageSource? annotatedImage;

    [ObservableProperty]
    ImageSource? muzzleImage;

    [ObservableProperty]
    string confidenceText = string.Empty;

    [ObservableProperty]
    string cropConfidenceText = string.Empty;

    [ObservableProperty]
    double confidence;

    [ObservableProperty]
    string emptyStateMessage = "Tira uma foto ou escolhe uma do celular pra começar.";

    public bool HasResult => AnnotatedImage is not null;

    [RelayCommand]
    Task CaptureAsync() => ProcessAsync(_picker.CaptureAsync, "Não deu pra usar a câmera");

    [RelayCommand]
    Task PickAsync() => ProcessAsync(_picker.PickAsync, "Não deu pra abrir as fotos");

    [RelayCommand]
    void Reset()
    {
        AnnotatedImage = null;
        MuzzleImage = null;
        ConfidenceText = string.Empty;
        CropConfidenceText = string.Empty;
        Confidence = 0;
        EmptyStateMessage = "Tira uma foto ou escolhe uma do celular pra começar.";
    }

    [RelayCommand]
    Task ManejoSoonAsync(string? feature) => _dialog.AlertAsync(
        "Em breve",
        $"\"{feature ?? "Esse manejo"}\" entra numa próxima versão. Por enquanto o app só reconhece o boi pelo focinho — quando o manejo estiver pronto, é só apertar aqui pra registrar.",
        "Beleza");

    Task ProcessAsync(Func<CancellationToken, Task<byte[]?>> source, string errorTitle)
        => RunBusyAsync("Reconhecendo o boi...", async () =>
        {
            try
            {
                var bytes = await source(CancellationToken.None);
                if (bytes is null) return;

                Reset();

                var detection = await _model.DetectAsync(bytes);
                if (detection is null)
                {
                    EmptyStateMessage = "Não achei o focinho dessa vez. Tenta de novo.";
                    await _dialog.AlertAsync(
                        "Não achei o boi",
                        "Não consegui reconhecer o focinho nessa foto. Tenta tirar de mais perto, com boa luz e o boi paradinho.",
                        "Tentar de novo");
                    return;
                }

                await PresentAsync(bytes, detection);
            }
            catch (Exception ex)
            {
                await _dialog.AlertAsync(errorTitle, ex.Message);
            }
        });

    async Task PresentAsync(byte[] originalBytes, DetectionResult detection)
    {
        var annotated = await _imaging.DrawBoundingBoxAsync(originalBytes, detection.Box, detection.Confidence);
        var cropped = await _imaging.CropAsync(originalBytes, detection.Box);

        // Salva em cache com nome único pra evitar que o MAUI Image cacheie a versão antiga
        CleanupCache();
        var token = DateTime.UtcNow.Ticks;
        var annotatedPath = Path.Combine(FileSystem.CacheDirectory, $"{CachePrefix}annotated_{token}.png");
        var muzzlePath = Path.Combine(FileSystem.CacheDirectory, $"{CachePrefix}muzzle_{token}.png");

        await File.WriteAllBytesAsync(annotatedPath, annotated);
        await File.WriteAllBytesAsync(muzzlePath, cropped);

        Confidence = detection.Confidence;
        ConfidenceText = $"{detection.Confidence * 100f:F1}%";
        CropConfidenceText = $"{detection.Confidence * 100f:F1}%";

        // Atribui a imagem anotada primeiro — isso dispara HasResult=true e o painel de resultado
        // aparece. Os controles de Image do painel só são medidos/laid out depois desse momento.
        AnnotatedImage = ImageSource.FromFile(annotatedPath);

        // Espera o layout completar antes de carregar a imagem do recorte.
        // Sem isso, o image handler do Android às vezes perde a primeira carga (controle ainda sendo
        // medido enquanto recebe Source), e o card do focinho fica em branco até o usuário sair e
        // voltar pra página, o que força um re-measure.
        await Task.Yield();
        await Task.Delay(80);

        MuzzleImage = ImageSource.FromFile(muzzlePath);
    }

    static void CleanupCache()
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(FileSystem.CacheDirectory, $"{CachePrefix}*.png"))
            {
                try { File.Delete(file); } catch { /* ignora arquivos travados */ }
            }
        }
        catch
        {
            // ignora qualquer erro de cleanup, não é crítico
        }
    }
}
