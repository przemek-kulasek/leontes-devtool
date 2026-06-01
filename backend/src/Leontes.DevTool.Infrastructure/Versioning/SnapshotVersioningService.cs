using System.Globalization;
using System.IO;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Leontes.DevTool.Application.Abstractions;
using Leontes.DevTool.Application.Common;
using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leontes.DevTool.Infrastructure.Versioning;

/// <summary>Keeps timestamped snapshots of task documents under the task's .leontes/history folder.</summary>
public sealed class SnapshotVersioningService(IAppDbContext db, IFileSystemService fs) : IVersioningService
{
    public async Task<DocumentVersion> SaveSnapshotAsync(WorkTask task, string relativePath, string? label, CancellationToken ct = default)
    {
        var source = fs.Combine(task.FolderPath, relativePath);
        var content = fs.ReadText(source);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmssfff", CultureInfo.InvariantCulture);
        var snapshotName = $"{Path.GetFileName(relativePath)}.{timestamp}.snap";
        var snapshotRelative = Path.Combine(TaskLayout.HistoryFolder, snapshotName);

        fs.WriteText(fs.Combine(task.FolderPath, snapshotRelative), content);

        var version = new DocumentVersion
        {
            WorkTaskId = task.Id,
            RelativePath = relativePath,
            SnapshotRelativePath = snapshotRelative,
            Label = label,
        };

        db.DocumentVersions.Add(version);
        await db.SaveChangesAsync(ct);
        return version;
    }

    public async Task<IReadOnlyList<DocumentVersion>> ListVersionsAsync(Guid taskId, string relativePath, CancellationToken ct = default) =>
        await db.DocumentVersions
            .Where(v => v.WorkTaskId == taskId && v.RelativePath == relativePath)
            .OrderByDescending(v => v.CreatedUtc)
            .AsNoTracking()
            .ToListAsync(ct);

    public string ReadSnapshot(WorkTask task, DocumentVersion version) =>
        fs.ReadText(fs.Combine(task.FolderPath, version.SnapshotRelativePath));

    public async Task RestoreAsync(WorkTask task, DocumentVersion version, CancellationToken ct = default)
    {
        await SaveSnapshotAsync(task, version.RelativePath, "Auto-snapshot before restore", ct);
        var content = ReadSnapshot(task, version);
        fs.WriteText(fs.Combine(task.FolderPath, version.RelativePath), content);
    }

    public DocumentDiff Diff(string oldText, string newText)
    {
        var model = InlineDiffBuilder.Diff(oldText, newText);
        var lines = model.Lines.Select(l => new DiffLine(Map(l.Type), l.Text)).ToList();
        return new DocumentDiff(lines);
    }

    private static DiffChangeKind Map(ChangeType type) => type switch
    {
        ChangeType.Inserted => DiffChangeKind.Inserted,
        ChangeType.Deleted => DiffChangeKind.Deleted,
        ChangeType.Modified => DiffChangeKind.Modified,
        _ => DiffChangeKind.Unchanged,
    };
}
