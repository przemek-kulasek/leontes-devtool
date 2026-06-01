using Leontes.DevTool.Domain.Common;

namespace Leontes.DevTool.Domain.Entities;

/// <summary>
/// Top of the organization hierarchy. Holds the disk root the app owns and the project-level
/// "general context" that every feature and task below it inherits.
/// </summary>
public sealed class Project : Entity
{
    public required string Name { get; set; }

    /// <summary>Root folder the app owns and lays features/tasks out under.</summary>
    public required string RootFolderPath { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Project-wide knowledge inherited downward, e.g. "we are rewriting a legacy app; behavior
    /// must match the old app unless the ticket says otherwise".
    /// </summary>
    public string? GeneralContext { get; set; }

    /// <summary>Default Jira project key (e.g. "MA") used to prefill task intake.</summary>
    public string? JiraProjectKey { get; set; }

    public string? GitHubOwner { get; set; }

    public string? GitHubRepo { get; set; }

    public ICollection<Feature> Features { get; set; } = [];

    public ICollection<ContextSnippet> ContextSnippets { get; set; } = [];
}
