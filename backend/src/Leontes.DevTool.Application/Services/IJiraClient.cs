using Leontes.DevTool.Application.Models;

namespace Leontes.DevTool.Application.Services;

public interface IJiraClient
{
    /// <summary>Fetches a single issue from Jira Cloud by its key (e.g. "MA-777").</summary>
    Task<JiraTicket> GetIssueAsync(string issueKey, CancellationToken ct = default);

    /// <summary>
    /// Downloads image attachments from <paramref name="attachments"/> into
    /// <paramref name="targetFolder"/>, skipping non-image files.
    /// Files that already exist are silently overwritten.
    /// Returns the number of images downloaded.
    /// </summary>
    Task<int> DownloadAttachmentsAsync(
        IReadOnlyList<JiraAttachment> attachments,
        string targetFolder,
        CancellationToken ct = default);
}
