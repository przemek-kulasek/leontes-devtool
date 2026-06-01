using System.Text;
using Leontes.DevTool.Application.Abstractions;
using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leontes.DevTool.Application.Services.Implementation;

/// <summary>
/// Layers inherited knowledge for a task: project general context, feature context, a short index
/// of sibling tasks, and any extra text supplied on the Context step.
/// </summary>
public sealed class KnowledgeAggregator(IAppDbContext db) : IKnowledgeAggregator
{
    public async Task<string> ComposeContextAsync(Guid taskId, string? extraContext, CancellationToken ct = default)
    {
        var task = await db.WorkTasks
            .Include(t => t.Feature)!
            .ThenInclude(f => f!.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId, ct)
            ?? throw new NotFoundException(nameof(WorkTask), taskId);

        var feature = task.Feature!;
        var project = feature.Project!;

        var siblings = await db.WorkTasks
            .Where(t => t.FeatureId == feature.Id && t.Id != taskId)
            .OrderBy(t => t.JiraKey)
            .Select(t => new { t.JiraKey, t.Title, t.Status })
            .AsNoTracking()
            .ToListAsync(ct);

        var sb = new StringBuilder();

        AppendSection(sb, "Project context", project.GeneralContext);
        AppendSection(sb, "Feature context", feature.FeatureContext);

        if (siblings.Count > 0)
        {
            sb.AppendLine("## Related tasks in this feature");
            foreach (var s in siblings)
                sb.AppendLine($"- {s.JiraKey}: {s.Title} ({s.Status})");
            sb.AppendLine();
        }

        AppendSection(sb, "Additional context", extraContext);

        return sb.ToString().TrimEnd();
    }

    private static void AppendSection(StringBuilder sb, string heading, string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return;

        sb.AppendLine($"## {heading}");
        sb.AppendLine(body.Trim());
        sb.AppendLine();
    }
}
