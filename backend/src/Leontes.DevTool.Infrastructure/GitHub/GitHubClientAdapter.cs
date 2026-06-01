using Gh = Octokit;
using Leontes.DevTool.Application.Common;
using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Domain.Common;

namespace Leontes.DevTool.Infrastructure.GitHub;

/// <summary>Octokit-backed GitHub client. Reads the PAT from the secret store at call time.</summary>
public sealed class GitHubClientAdapter(ISecretStore secrets) : IGitHubClient
{
    private const string UserAgent = "Leontes-DevTool";

    public async Task<IReadOnlyList<PullRequestRef>> GetClosedPullRequestsAsync(string owner, string repo, int max, CancellationToken ct = default)
    {
        var client = CreateClient();
        var request = new Gh.PullRequestRequest { State = Gh.ItemStateFilter.Closed };
        var options = new Gh.ApiOptions { PageSize = Math.Clamp(max, 1, 100), PageCount = 1 };

        var pulls = await client.PullRequest.GetAllForRepository(owner, repo, request, options);
        return [.. pulls.Select(p => new PullRequestRef(
            p.Number,
            p.Title,
            p.User.Login,
            IsBot(p.User),
            p.ClosedAt?.UtcDateTime,
            p.HtmlUrl))];
    }

    public async Task<IReadOnlyList<PullRequestComment>> GetPullRequestCommentsAsync(string owner, string repo, int number, CancellationToken ct = default)
    {
        var client = CreateClient();

        var reviewComments = await client.PullRequest.ReviewComment.GetAll(owner, repo, number);
        var issueComments = await client.Issue.Comment.GetAllForIssue(owner, repo, number);

        var result = new List<PullRequestComment>();
        result.AddRange(reviewComments.Select(c => new PullRequestComment(
            number, c.User.Login, IsBot(c.User), c.Body, c.Path, c.CreatedAt.UtcDateTime, c.HtmlUrl)));
        result.AddRange(issueComments.Select(c => new PullRequestComment(
            number, c.User.Login, IsBot(c.User), c.Body, null, c.CreatedAt.UtcDateTime, c.HtmlUrl)));

        return [.. result.OrderBy(c => c.CreatedUtc)];
    }

    private Gh.GitHubClient CreateClient()
    {
        var pat = secrets.Get(SecretKeys.GitHubPat);
        if (string.IsNullOrWhiteSpace(pat))
            throw new ValidationException("GitHub is not configured. Add a personal access token in Settings.");

        return new Gh.GitHubClient(new Gh.ProductHeaderValue(UserAgent)) { Credentials = new Gh.Credentials(pat) };
    }

    private static bool IsBot(Gh.User user) =>
        user.Type == Gh.AccountType.Bot
        || user.Login.EndsWith("[bot]", StringComparison.OrdinalIgnoreCase)
        || user.Login.Equals("copilot", StringComparison.OrdinalIgnoreCase);
}
