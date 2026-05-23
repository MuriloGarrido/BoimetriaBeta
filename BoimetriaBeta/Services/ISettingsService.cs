namespace BoimetriaBeta.Services;

public interface ISettingsService
{
    string? ModelPath { get; }
    bool HasModel { get; }
    void SetModelPath(string path);
    void ClearModelPath();
}
