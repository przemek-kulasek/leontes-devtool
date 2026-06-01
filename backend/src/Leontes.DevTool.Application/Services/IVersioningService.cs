using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Domain.Entities;

namespace Leontes.DevTool.Application.Services;

/// <summary>Snapshot-based versioning for task documents (spec.md and friends).</summary>
public interface IVersioningService
{
    /// <summary>Snapshots the current contents of a document into the task's history and records the version.</summary>
    Task<DocumentVersion> SaveSnapshotAsync(WorkTask task, string relativePath, string? label, CancellationToken ct = default);

    Task<IReadOnlyList<DocumentVersion>> ListVersionsAsync(Guid taskId, string relativePath, CancellationToken ct = default);

    /// <summary>Reads the stored snapshot text for a version.</summary>
    string ReadSnapshot(WorkTask task, DocumentVersion version);

    /// <summary>Snapshots the current file (so the restore is itself reversible), then overwrites it with the version's contents.</summary>
    Task RestoreAsync(WorkTask task, DocumentVersion version, CancellationToken ct = default);

    DocumentDiff Diff(string oldText, string newText);
}
