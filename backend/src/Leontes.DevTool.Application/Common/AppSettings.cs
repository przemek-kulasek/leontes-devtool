namespace Leontes.DevTool.Application.Common;

/// <summary>
/// Non-secret user preferences persisted as JSON. Tokens (Jira API token, GitHub PAT) are never
/// stored here — they live in <see cref="Leontes.DevTool.Application.Services.ISecretStore"/>.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Parent folder the app lays projects out under.</summary>
    public string? WorkspaceRootPath { get; set; }

    public string? JiraBaseUrl { get; set; }

    public string? JiraEmail { get; set; }

    public string OllamaEndpoint { get; set; } = "http://localhost:11434";

    public string OllamaChatModel { get; set; } = "qwen2.5:7b-instruct";

    public string? GitHubDefaultOwner { get; set; }

    public string? GitHubDefaultRepo { get; set; }

    /// <summary>
    /// Jira custom field ID that holds acceptance criteria. Defaults to <c>customfield_10016</c>
    /// which is the most common field ID used in Jira Cloud templates for acceptance criteria.
    /// Override via Settings if your instance uses a different field.
    /// </summary>
    public string JiraAcceptanceCriteriaFieldId { get; set; } = "customfield_10016";
}
