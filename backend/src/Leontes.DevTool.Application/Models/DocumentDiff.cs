namespace Leontes.DevTool.Application.Models;

public enum DiffChangeKind
{
    Unchanged = 0,
    Inserted = 1,
    Deleted = 2,
    Modified = 3,
}

public sealed record DiffLine(DiffChangeKind Kind, string Text);

/// <summary>Line-oriented diff between two document versions, provider-agnostic for the UI.</summary>
public sealed record DocumentDiff(IReadOnlyList<DiffLine> Lines);
