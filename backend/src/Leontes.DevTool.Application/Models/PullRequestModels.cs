namespace Leontes.DevTool.Application.Models;

/// <summary>Summary of a closed pull request.</summary>
public sealed record PullRequestRef(
    int Number,
    string Title,
    string Author,
    bool IsBot,
    DateTime? ClosedUtc,
    string Url);

/// <summary>A single review or issue comment on a pull request, with author classification.</summary>
public sealed record PullRequestComment(
    int PullRequestNumber,
    string Author,
    bool IsBot,
    string Body,
    string? FilePath,
    DateTime CreatedUtc,
    string Url);
