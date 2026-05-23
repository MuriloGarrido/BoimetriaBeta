using BoimetriaBeta.Models;
using SkiaSharp;

namespace BoimetriaBeta.Services;

public sealed class ImageProcessingService : IImageProcessingService
{
    static readonly SKColor AccentColor = new(0x1B, 0x5E, 0x20);

    public Task<byte[]> DrawBoundingBoxAsync(byte[] imageBytes, BoundingBox box, float confidence, CancellationToken cancellationToken = default)
        => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var bitmap = SKBitmap.Decode(imageBytes)
                ?? throw new InvalidOperationException("Não foi possível decodificar a imagem.");

            using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
            var canvas = surface.Canvas;
            canvas.DrawBitmap(bitmap, 0, 0);

            var strokeWidth = Math.Max(4f, bitmap.Width / 220f);
            using var stroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = AccentColor,
                StrokeWidth = strokeWidth,
                IsAntialias = true,
            };

            var rect = new SKRect(box.X1, box.Y1, box.X2, box.Y2);
            var radius = Math.Max(12f, bitmap.Width / 80f);
            canvas.DrawRoundRect(rect, radius, radius, stroke);

            var label = $"MUZZLE  {confidence * 100f:F1}%";
            var fontSize = Math.Max(22f, bitmap.Width / 32f);
            using var font = new SKFont(SKTypeface.Default, fontSize) { Edging = SKFontEdging.Antialias };
            var textWidth = font.MeasureText(label);
            var padX = fontSize * 0.6f;
            var padY = fontSize * 0.35f;
            var pillHeight = fontSize + padY * 2;
            var pillWidth = textWidth + padX * 2;
            var pillX = Math.Max(0f, box.X1);
            var pillY = Math.Max(0f, box.Y1 - pillHeight - 6f);

            using var pillPaint = new SKPaint { Color = AccentColor, Style = SKPaintStyle.Fill, IsAntialias = true };
            canvas.DrawRoundRect(new SKRect(pillX, pillY, pillX + pillWidth, pillY + pillHeight), pillHeight / 2f, pillHeight / 2f, pillPaint);

            using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            var metrics = font.Metrics;
            var textY = pillY + (pillHeight - metrics.Descent - metrics.Ascent) / 2f;
            canvas.DrawText(label, pillX + padX, textY, SKTextAlign.Left, font, textPaint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 92);
            return data.ToArray();
        }, cancellationToken);

    public Task<byte[]> CropAsync(byte[] imageBytes, BoundingBox box, CancellationToken cancellationToken = default)
        => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var bitmap = SKBitmap.Decode(imageBytes)
                ?? throw new InvalidOperationException("Não foi possível decodificar a imagem.");

            // Pequena folga ao redor do focinho pra dar respiro visual
            var padding = Math.Max(6f, Math.Min(bitmap.Width, bitmap.Height) * 0.025f);

            var x1 = (int)Math.Clamp(MathF.Floor(box.X1 - padding), 0, bitmap.Width - 1);
            var y1 = (int)Math.Clamp(MathF.Floor(box.Y1 - padding), 0, bitmap.Height - 1);
            var x2 = (int)Math.Clamp(MathF.Ceiling(box.X2 + padding), x1 + 1, bitmap.Width);
            var y2 = (int)Math.Clamp(MathF.Ceiling(box.Y2 + padding), y1 + 1, bitmap.Height);

            var width = x2 - x1;
            var height = y2 - y1;

            // Garante uma região mínima utilizável (evita crop degenerado em bordas)
            if (width < 16 || height < 16)
                throw new InvalidOperationException("A região do focinho ficou pequena demais pra recortar. Tira a foto de mais perto.");

            // Usa canvas em vez de ExtractSubset: mais confiável e produz um bitmap independente
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var cropped = new SKBitmap(info);
            using (var canvas = new SKCanvas(cropped))
            {
                canvas.Clear(SKColors.Transparent);
                var source = new SKRect(x1, y1, x2, y2);
                var dest = new SKRect(0, 0, width, height);
                canvas.DrawBitmap(bitmap, source, dest);
            }

            using var image = SKImage.FromBitmap(cropped);
            using var data = image.Encode(SKEncodedImageFormat.Png, 92)
                ?? throw new InvalidOperationException("Falha ao gerar a imagem recortada.");
            return data.ToArray();
        }, cancellationToken);
}
