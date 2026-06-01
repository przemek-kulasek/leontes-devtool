namespace Leontes.DevTool.Application.Models;

/// <summary>A single comment from a Jira issue, converted to plain text.</summary>
public sealed record JiraComment(string Author, string BodyMarkdown, DateTimeOffset Created);
