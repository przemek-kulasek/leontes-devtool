using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Domain.Entities;

/// <summary>
/// A single unit of work, usually mapped to a Jira ticket. Owns a folder on disk and carries the
/// per-step workflow state, linked materials, and versioned-document index.
/// </summary>
public sealed class WorkTask : Entity
{
    public Guid FeatureId { get; set; }

    public Feature? Feature { get; set; }

    /// <summary>Jira issue key, e.g. "MA-777". Doubles as the task folder name.</summary>
    public required string JiraKey { get; set; }

    public required string Title { get; set; }

    /// <summary>Absolute folder for this task (under the feature folder).</summary>
    public required string FolderPath { get; set; }

    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Draft;

    public ICollection<StepState> Steps { get; set; } = [];

    public ICollection<MaterialLink> Materials { get; set; } = [];

    public ICollection<DocumentVersion> Versions { get; set; } = [];
}
