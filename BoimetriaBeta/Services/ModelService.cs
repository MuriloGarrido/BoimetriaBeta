using BoimetriaBeta.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace BoimetriaBeta.Services;

public sealed class ModelService : IModelService, IDisposable
{
    const int InputSize = 640;
    const float ConfidenceThreshold = 0.5f;

    readonly ISettingsService _settings;
    readonly SemaphoreSlim _gate = new(1, 1);

    InferenceSession? _session;
    string? _inputName;

    public ModelService(ISettingsService settings) => _settings = settings;

    public bool IsReady => _session is not null;
    public string? LoadedModelPath { get; private set; }

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_session is not null) return;

        var path = _settings.ModelPath
            ?? throw new InvalidOperationException("O app precisa ser preparado primeiro. Vai em Ajustes e carrega o arquivo do técnico.");

        if (!File.Exists(path))
            throw new FileNotFoundException("Não encontrei o arquivo do sistema. Carrega ele de novo em Ajustes.", path);

        await LoadInternalAsync(path, cancellationToken);
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        DisposeSession();
        await EnsureLoadedAsync(cancellationToken);
    }

    public async Task<DetectionResult?> DetectAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);

        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var bitmap = SKBitmap.Decode(imageBytes)
                ?? throw new InvalidOperationException("Não foi possível decodificar a imagem para inferência.");

            var (tensor, originalWidth, originalHeight) = BuildInputTensor(bitmap);

            var session = _session ?? throw new InvalidOperationException("Sessão ONNX não inicializada.");
            var inputName = _inputName ?? session.InputMetadata.Keys.First();
            var input = NamedOnnxValue.CreateFromTensor(inputName, tensor);

            using var results = session.Run(new[] { input });
            var output = results.First().AsTensor<float>();

            return ParseBestDetection(output, originalWidth, originalHeight);
        }, cancellationToken);
    }

    static (DenseTensor<float> Tensor, int OriginalWidth, int OriginalHeight) BuildInputTensor(SKBitmap source)
    {
        using var resized = source.Resize(new SKImageInfo(InputSize, InputSize, SKColorType.Rgba8888), SKSamplingOptions.Default)
            ?? throw new InvalidOperationException("Falha ao redimensionar a imagem para 640x640.");

        var tensor = new DenseTensor<float>(new[] { 1, 3, InputSize, InputSize });
        var pixels = resized.Pixels;

        for (var y = 0; y < InputSize; y++)
        {
            var rowOffset = y * InputSize;
            for (var x = 0; x < InputSize; x++)
            {
                var pixel = pixels[rowOffset + x];
                tensor[0, 0, y, x] = pixel.Red / 255f;
                tensor[0, 1, y, x] = pixel.Green / 255f;
                tensor[0, 2, y, x] = pixel.Blue / 255f;
            }
        }

        return (tensor, source.Width, source.Height);
    }

    static DetectionResult? ParseBestDetection(Tensor<float> output, int originalWidth, int originalHeight)
    {
        var dimensions = output.Dimensions;
        if (dimensions.Length != 3 || dimensions[2] < 6)
            return null;

        var detections = dimensions[1];
        var scaleX = originalWidth / (float)InputSize;
        var scaleY = originalHeight / (float)InputSize;

        var bestConfidence = 0f;
        DetectionResult? best = null;

        for (var i = 0; i < detections; i++)
        {
            var confidence = output[0, i, 4];
            if (confidence <= ConfidenceThreshold || confidence <= bestConfidence)
                continue;

            var x1 = Math.Clamp(output[0, i, 0] * scaleX, 0f, originalWidth);
            var y1 = Math.Clamp(output[0, i, 1] * scaleY, 0f, originalHeight);
            var x2 = Math.Clamp(output[0, i, 2] * scaleX, 0f, originalWidth);
            var y2 = Math.Clamp(output[0, i, 3] * scaleY, 0f, originalHeight);

            if (x2 <= x1 || y2 <= y1)
                continue;

            bestConfidence = confidence;
            best = new DetectionResult(new BoundingBox(x1, y1, x2, y2), confidence, (int)output[0, i, 5]);
        }

        return best;
    }

    async Task LoadInternalAsync(string path, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_session is not null) return;

            var options = new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
                InterOpNumThreads = 1,
                IntraOpNumThreads = Math.Max(1, Environment.ProcessorCount / 2),
            };

            var session = await Task.Run(() => new InferenceSession(path, options), cancellationToken);
            _session = session;
            _inputName = session.InputMetadata.Keys.First();
            LoadedModelPath = path;
        }
        finally
        {
            _gate.Release();
        }
    }

    void DisposeSession()
    {
        _session?.Dispose();
        _session = null;
        _inputName = null;
        LoadedModelPath = null;
    }

    public void Dispose()
    {
        DisposeSession();
        _gate.Dispose();
    }
}
