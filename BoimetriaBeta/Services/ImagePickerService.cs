namespace BoimetriaBeta.Services;

public sealed class ImagePickerService : IImagePickerService
{
    public async Task<byte[]?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
            throw new InvalidOperationException("Esse celular não tem câmera disponível.");

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            throw new UnauthorizedAccessException("Você precisa liberar o acesso à câmera nas configurações do celular.");

        var photo = await MediaPicker.Default.CapturePhotoAsync();
        return photo is null ? null : await ReadAsync(photo, cancellationToken);
    }

    public async Task<byte[]?> PickAsync(CancellationToken cancellationToken = default)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync();
        return photo is null ? null : await ReadAsync(photo, cancellationToken);
    }

    static async Task<byte[]> ReadAsync(FileResult file, CancellationToken cancellationToken)
    {
        using var stream = await file.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        return memory.ToArray();
    }
}
