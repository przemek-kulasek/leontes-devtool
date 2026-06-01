using Leontes.DevTool.Application.Models;

namespace Leontes.DevTool.Application.Services;

public interface IGitHubClient
{
    Task<IReadOnlyList<PullRequestRef>> GetClosedPullRequestsAsync(string owner, string repo, int max, CancellationToken ct = default);

    /// <summary>Returns review + issue comments for a PR, with each author classified as human or bot.</summary>
    Task<IReadOnlyList<PullRequestComment>> GetPullRequestCommentsAsync(string owner, string repo, int number, CancellationToken ct = default);
}
