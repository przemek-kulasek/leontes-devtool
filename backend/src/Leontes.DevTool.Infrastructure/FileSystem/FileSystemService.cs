using System.IO;
using System.Text;
using Leontes.DevTool.Application.Common;
using Leontes.DevTool.Application.Services;

namespace Leontes.DevTool.Infrastructure.FileSystem;

/// <summary>Owns the on-disk folder layout. Idempotent and safe against existing folders.</summary>
public sealed class FileSystemService : IFileSystemService
{
    private static readonly char[] InvalidChars =
        [.. Path.GetInvalidFileNameChars(), .. new[] { ' ' }];

    public IReadOnlyList<string> TaskSubfolders => TaskLayout.Subfolders;

    public string EnsureProjectFolder(string workspaceRootPath, string projectName)
    {
        var folder = Path.Combine(workspaceRootPath, Sanitize(projectName));
        Directory.CreateDirectory(folder);
        return folder;
    }

    public string EnsureFeatureFolder(string projectFolder, string featureName)
    {
        var folder = Path.Combine(projectFolder, Sanitize(featureName));
        Directory.CreateDirectory(folder);
        return folder;
    }

    public string EnsureTaskFolder(string featureFolder, string taskKey)
    {
        var folder = Path.Combine(featureFolder, Sanitize(taskKey));
        Directory.CreateDirectory(folder);
        foreach (var sub in TaskLayout.Subfolders)
            Directory.CreateDirectory(Path.Combine(folder, sub));
        return folder;
    }

    public string Combine(string baseFolder, string relativePath) => Path.Combine(baseFolder, relativePath);

    public bool FileExists(string absolutePath) => File.Exists(absolutePath);

    public string ReadText(string absolutePath) =>
        File.Exists(absolutePath) ? File.ReadAllText(absolutePath, Encoding.UTF8) : string.Empty;

    public void WriteText(string absolutePath, string content)
    {
        var dir = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(absolutePath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
    {
        var dir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.Copy(sourcePath, destinationPath, overwrite);
    }

    public void DeleteFolder(string absolutePath)
    {
        if (Directory.Exists(absolutePath))
            Directory.Delete(absolutePath, recursive: true);
    }

    private static string Sanitize(string name)
    {
        var trimmed = name.Trim();
        var builder = new StringBuilder(trimmed.Length);
        foreach (var c in trimmed)
            builder.Append(Array.IndexOf(InvalidChars, c) >= 0 ? '-' : c);

        var result = builder.ToString().Trim('-', '.');
        return string.IsNullOrEmpty(result) ? "untitled" : result;
    }
}
