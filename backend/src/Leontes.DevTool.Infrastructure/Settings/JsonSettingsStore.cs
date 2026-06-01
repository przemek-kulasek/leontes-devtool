using System.IO;
using System.Text.Json;
using Leontes.DevTool.Application.Common;
using Leontes.DevTool.Application.Services;

namespace Leontes.DevTool.Infrastructure.Settings;

/// <summary>Persists non-secret <see cref="AppSettings"/> as indented JSON in the app data dir.</summary>
public sealed class JsonSettingsStore(string filePath) : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public AppSettings Load()
    {
        if (!File.Exists(filePath))
            return new AppSettings();

        try
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(filePath)) ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings) =>
        File.WriteAllText(filePath, JsonSerializer.Serialize(settings, Options));
}
