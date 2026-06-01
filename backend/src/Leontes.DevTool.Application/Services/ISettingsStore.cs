using Leontes.DevTool.Application.Common;

namespace Leontes.DevTool.Application.Services;

/// <summary>Loads and persists non-secret <see cref="AppSettings"/> (JSON file in the app data dir).</summary>
public interface ISettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}
