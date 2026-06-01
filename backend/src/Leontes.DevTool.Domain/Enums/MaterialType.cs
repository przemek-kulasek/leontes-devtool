namespace Leontes.DevTool.Domain.Enums;

/// <summary>Kinds of supporting material a task can reference.</summary>
public enum MaterialType
{
    /// <summary>A folder on disk (e.g. legacy DB scripts, the new API project).</summary>
    Folder = 0,

    /// <summary>An image copied into the task's images/ folder (design, screenshot).</summary>
    Image = 1,

    /// <summary>A free-text snippet linked into the spec.</summary>
    TextSnippet = 2,

    /// <summary>A specific code location (file or path) relevant to the task.</summary>
    CodeLocation = 3,

    /// <summary>An external URL (Confluence page, design tool, etc.).</summary>
    Url = 4,
}
