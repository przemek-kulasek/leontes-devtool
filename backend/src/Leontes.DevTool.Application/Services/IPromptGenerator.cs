using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Domain.Entities;

namespace Leontes.DevTool.Application.Services;

/// <summary>Builds the spec document and the per-step LLM prompts, all referencing absolute task paths.</summary>
public interface IPromptGenerator
{
    /// <summary>Composes the full spec.md from the ticket, materials, selected rules, and aggregated context.</summary>
    string ComposeSpecMarkdown(
        WorkTask task,
        JiraTicket? ticket,
        IReadOnlyList<MaterialLink> materials,
        IReadOnlyList<RulePreset> rules,
        string composedContext);

    /// <summary>The kickoff prompt handed to Copilot/Claude to start implementation.</summary>
    string BuildKickoffPrompt(WorkTask task, IReadOnlyList<RulePreset> selectedRules);

    string BuildSelfReviewPrompt(WorkTask task);

    string BuildPastCommentsPrompt(WorkTask task, IReadOnlyList<PullRequestComment> humanComments);

    string BuildPrCommentsPrompt(WorkTask task, IReadOnlyList<PullRequestComment> comments);
}
