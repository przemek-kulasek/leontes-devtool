using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Domain.Entities;

/// <summary>
/// Persisted state for one workflow step of a task. Each task is seeded with one row per
/// <see cref="StepKind"/>; the user advances them independently (the workflow is non-linear).
/// </summary>
public sealed class StepState : Entity
{
    public Guid WorkTaskId { get; set; }

    public WorkTask? WorkTask { get; set; }

    public StepKind Kind { get; set; }

    public StepStatus Status { get; set; } = StepStatus.NotStarted;

    /// <summary>The most recently generated prompt for this step (kickoff, review, etc.).</summary>
    public string? GeneratedPrompt { get; set; }

    /// <summary>Path (relative to the task folder) of the artifact this step produces, if any.</summary>
    public string? OutputRelativePath { get; set; }

    /// <summary>Model suggested for this step (resolved from the recommendation map).</summary>
    public string? SuggestedModel { get; set; }

    public string? Notes { get; set; }

    public DateTime? CompletedUtc { get; set; }
}
