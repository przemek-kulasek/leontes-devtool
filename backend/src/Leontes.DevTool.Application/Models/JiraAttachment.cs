namespace Leontes.DevTool.Application.Models;

/// <summary>Metadata for a single Jira issue attachment. Use <c>MimeType</c> to detect images.</summary>
public sealed record JiraAttachment(string FileName, string MimeType, string ContentUrl, long Size)
{
    public bool IsImage => MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}
