using System.IO;

namespace Leontes.DevTool.Infrastructure.Persistence;

/// <summary>Resolves the per-user application data locations and ensures the base folder exists.</summary>
public static class AppPaths
{
    public static string DataDirectory
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Leontes");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string DatabaseFile => Path.Combine(DataDirectory, "leontes.db");

    public static string SecretsFile => Path.Combine(DataDirectory, "secrets.dat");

    public static string SettingsFile => Path.Combine(DataDirectory, "settings.json");
}
