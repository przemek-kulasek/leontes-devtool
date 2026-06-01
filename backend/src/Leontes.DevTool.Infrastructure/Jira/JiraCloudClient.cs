using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Leontes.DevTool.Application.Common;
using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Domain.Common;

namespace Leontes.DevTool.Infrastructure.Jira;

/// <summary>
/// Jira Cloud REST v3 client. Reads base URL/email from settings and the API token from the secret
/// store at call time, so credentials updated in the UI take effect without restarting.
/// </summary>
public sealed class JiraCloudClient(HttpClient http, ISettingsStore settings, ISecretStore secrets) : IJiraClient
{
    public async Task<JiraTicket> GetIssueAsync(string issueKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(issueKey))
            throw new ValidationException("A Jira issue key is required.");

        var config = settings.Load();
        var token = secrets.Get(SecretKeys.JiraApiToken);
        if (string.IsNullOrWhiteSpace(config.JiraBaseUrl) || string.IsNullOrWhiteSpace(config.JiraEmail) || string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Jira is not configured. Set the base URL, email and API token in Settings.");

        var acField = config.JiraAcceptanceCriteriaFieldId;
        var url = $"{config.JiraBaseUrl.TrimEnd('/')}/rest/api/3/issue/{Uri.EscapeDataString(issueKey)}" +
                  $"?fields=summary,description,issuetype,status,labels,comment,attachment,{acField}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.JiraEmail}:{token}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new ValidationException("Jira rejected the credentials. Check the email and API token in Settings.");
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException("Jira issue", issueKey);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return Parse(issueKey, doc.RootElement, acField);
    }

    public async Task<int> DownloadAttachmentsAsync(
        IReadOnlyList<JiraAttachment> attachments,
        string targetFolder,
        CancellationToken ct = default)
    {
        var config = settings.Load();
        var token = secrets.Get(SecretKeys.JiraApiToken);
        if (string.IsNullOrWhiteSpace(config.JiraBaseUrl) || string.IsNullOrWhiteSpace(config.JiraEmail) || string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Jira is not configured. Set the base URL, email and API token in Settings.");

        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.JiraEmail}:{token}"));
        Directory.CreateDirectory(targetFolder);

        var downloaded = 0;
        foreach (var att in attachments.Where(a => a.IsImage))
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, att.ContentUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            using var resp = await http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            // Path.GetFileName strips any directory components from the API-supplied name,
            // preventing a crafted filename (e.g. "../../evil.sh") from escaping targetFolder.
            var safeFileName = Path.GetFileName(att.FileName);
            if (string.IsNullOrWhiteSpace(safeFileName))
                continue;

            var dest = Path.Combine(targetFolder, safeFileName);
            await using var fs = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None);
            await resp.Content.CopyToAsync(fs, ct);
            downloaded++;
        }

        return downloaded;
    }

    private static JiraTicket Parse(string key, JsonElement root, string acFieldId)
    {
        var fields = root.GetProperty("fields");

        var summary = fields.TryGetProperty("summary", out var s) ? s.GetString() ?? string.Empty : string.Empty;
        var description = fields.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.Object
            ? AdfToMarkdown.Convert(d)
            : string.Empty;
        var issueType = fields.TryGetProperty("issuetype", out var it) && it.ValueKind == JsonValueKind.Object
            ? it.GetProperty("name").GetString()
            : null;
        var status = fields.TryGetProperty("status", out var st) && st.ValueKind == JsonValueKind.Object
            ? st.GetProperty("name").GetString()
            : null;

        var labels = fields.TryGetProperty("labels", out var ls) && ls.ValueKind == JsonValueKind.Array
            ? ls.EnumerateArray().Select(x => x.GetString() ?? string.Empty).Where(x => x.Length > 0).ToList()
            : [];

        var acceptanceCriteria = ParseAcceptanceCriteria(fields, acFieldId);
        var comments = ParseComments(fields);
        var attachments = ParseAttachments(fields);

        return new JiraTicket(key, summary, description, issueType, status, labels,
            acceptanceCriteria, comments, attachments);
    }

    private static string? ParseAcceptanceCriteria(JsonElement fields, string acFieldId)
    {
        if (!fields.TryGetProperty(acFieldId, out var ac) || ac.ValueKind == JsonValueKind.Null)
            return null;

        // Rich-text (ADF) field
        if (ac.ValueKind == JsonValueKind.Object)
        {
            var md = AdfToMarkdown.Convert(ac);
            return string.IsNullOrWhiteSpace(md) ? null : md;
        }

        // Plain string field
        if (ac.ValueKind == JsonValueKind.String)
        {
            var text = ac.GetString();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        return null;
    }

    private static IReadOnlyList<JiraComment> ParseComments(JsonElement fields)
    {
        if (!fields.TryGetProperty("comment", out var commentNode) ||
            commentNode.ValueKind != JsonValueKind.Object ||
            !commentNode.TryGetProperty("comments", out var arr) ||
            arr.ValueKind != JsonValueKind.Array)
            return [];

        var result = new List<JiraComment>();
        foreach (var c in arr.EnumerateArray())
        {
            var author = c.TryGetProperty("author", out var a) && a.ValueKind == JsonValueKind.Object
                ? a.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? "Unknown" : "Unknown"
                : "Unknown";

            var body = c.TryGetProperty("body", out var b) && b.ValueKind == JsonValueKind.Object
                ? AdfToMarkdown.Convert(b)
                : string.Empty;

            var created = c.TryGetProperty("created", out var cr) && cr.ValueKind == JsonValueKind.String
                ? DateTimeOffset.TryParse(cr.GetString(), out var dt) ? dt : DateTimeOffset.UtcNow
                : DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(body))
                result.Add(new JiraComment(author, body, created));
        }

        return result;
    }

    private static IReadOnlyList<JiraAttachment> ParseAttachments(JsonElement fields)
    {
        if (!fields.TryGetProperty("attachment", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return [];

        var result = new List<JiraAttachment>();
        foreach (var a in arr.EnumerateArray())
        {
            var filename = a.TryGetProperty("filename", out var fn) ? fn.GetString() ?? string.Empty : string.Empty;
            var mimeType = a.TryGetProperty("mimeType", out var mt) ? mt.GetString() ?? string.Empty : string.Empty;
            var content = a.TryGetProperty("content", out var cu) ? cu.GetString() ?? string.Empty : string.Empty;
            var size = a.TryGetProperty("size", out var sz) ? sz.GetInt64() : 0L;

            if (!string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(content))
                result.Add(new JiraAttachment(filename, mimeType, content, size));
        }

        return result;
    }
}
