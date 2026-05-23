namespace BoimetriaBeta.Services;

public interface IImagePickerService
{
    Task<byte[]?> CaptureAsync(CancellationToken cancellationToken = default);
    Task<byte[]?> PickAsync(CancellationToken cancellationToken = default);
}
