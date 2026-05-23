namespace BoimetriaBeta.Services;

public sealed class SettingsService : ISettingsService
{
    const string ModelPathKey = "boimetria.model.path";

    public string? ModelPath
    {
        get
        {
            var raw = Preferences.Default.Get(ModelPathKey, string.Empty);
            return string.IsNullOrWhiteSpace(raw) ? null : raw;
        }
    }

    public bool HasModel => !string.IsNullOrWhiteSpace(ModelPath) && File.Exists(ModelPath);

    public void SetModelPath(string path) => Preferences.Default.Set(ModelPathKey, path);

    public void ClearModelPath() => Preferences.Default.Remove(ModelPathKey);
}
