using BoimetriaBeta.Models;

namespace BoimetriaBeta.Services;

public interface IModelService
{
    bool IsReady { get; }
    string? LoadedModelPath { get; }
    Task EnsureLoadedAsync(CancellationToken cancellationToken = default);
    Task ReloadAsync(CancellationToken cancellationToken = default);
    Task<DetectionResult?> DetectAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}
