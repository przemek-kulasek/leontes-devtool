namespace Leontes.DevTool.Application.Services;

/// <summary>
/// Owns the on-disk folder layout. All operations are idempotent and safe to call against
/// existing folders. Names are sanitized into filesystem-safe segments.
/// </summary>
public interface IFileSystemService
{
    /// <summary>Standard subfolders created inside every task folder.</summary>
    IReadOnlyList<string> TaskSubfolders { get; }

    /// <summary>Ensures &lt;workspaceRoot&gt;/&lt;project&gt; exists; returns its absolute path.</summary>
    string EnsureProjectFolder(string workspaceRootPath, string projectName);

    /// <summary>Ensures &lt;projectFolder&gt;/&lt;feature&gt; exists; returns its absolute path.</summary>
    string EnsureFeatureFolder(string projectFolder, string featureName);

    /// <summary>Ensures &lt;featureFolder&gt;/&lt;taskKey&gt; and all task subfolders exist; returns its absolute path.</summary>
    string EnsureTaskFolder(string featureFolder, string taskKey);

    string Combine(string baseFolder, string relativePath);

    bool FileExists(string absolutePath);

    /// <summary>Reads a UTF-8 text file, returning empty string if it does not exist.</summary>
    string ReadText(string absolutePath);

    /// <summary>Writes UTF-8 text, creating parent folders as needed.</summary>
    void WriteText(string absolutePath, string content);

    void CopyFile(string sourcePath, string destinationPath, bool overwrite);

    void DeleteFolder(string absolutePath);
}
