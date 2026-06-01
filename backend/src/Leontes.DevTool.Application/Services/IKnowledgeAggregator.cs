namespace Leontes.DevTool.Application.Services;

/// <summary>
/// Composes the context for a task by layering project general context, feature context, brief
/// summaries of sibling tasks, and any extra text the user adds on the Context step.
/// </summary>
public interface IKnowledgeAggregator
{
    Task<string> ComposeContextAsync(Guid taskId, string? extraContext, CancellationToken ct = default);
}
