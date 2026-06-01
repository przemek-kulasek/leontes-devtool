namespace Leontes.DevTool.Application.Models;

/// <summary>A Jira issue reduced to the fields the workflow needs.</summary>
public sealed record JiraTicket(
    string Key,
    string Summary,
    string DescriptionMarkdown,
    string? IssueType,
    string? Status,
    IReadOnlyList<string> Labels,
    string? AcceptanceCriteria,
    IReadOnlyList<JiraComment> Comments,
    IReadOnlyList<JiraAttachment> Attachments);
