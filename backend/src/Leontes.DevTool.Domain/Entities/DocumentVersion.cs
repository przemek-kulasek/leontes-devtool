using Leontes.DevTool.Domain.Common;

namespace Leontes.DevTool.Domain.Entities;

/// <summary>
/// Index row for a saved snapshot of a task document (spec.md and friends). The snapshot bytes live
/// on disk under the task's .leontes/history folder; this row records where and when.
/// </summary>
public sealed class DocumentVersion : Entity
{
    public Guid WorkTaskId { get; set; }

    public WorkTask? WorkTask { get; set; }

    /// <summary>Document this snapshot belongs to, relative to the task folder (e.g. "spec.md").</summary>
    public required string RelativePath { get; set; }

    /// <summary>Snapshot file location, relative to the task folder (under .leontes/history).</summary>
    public required string SnapshotRelativePath { get; set; }

    /// <summary>Optional user label for the version (e.g. "after review fixes").</summary>
    public string? Label { get; set; }
}
