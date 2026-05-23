using BoimetriaBeta.Models;

namespace BoimetriaBeta.Services;

public interface IImageProcessingService
{
    Task<byte[]> DrawBoundingBoxAsync(byte[] imageBytes, BoundingBox box, float confidence, CancellationToken cancellationToken = default);
    Task<byte[]> CropAsync(byte[] imageBytes, BoundingBox box, CancellationToken cancellationToken = default);
}
