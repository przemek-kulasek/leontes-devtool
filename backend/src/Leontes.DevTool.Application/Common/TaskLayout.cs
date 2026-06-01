namespace Leontes.DevTool.Application.Common;

/// <summary>Canonical file/folder names inside a task folder, shared by the file layout and prompt generation.</summary>
public static class TaskLayout
{
    public const string SpecFile = "spec.md";
    public const string ImagesFolder = "images";
    public const string ImplementationFolder = "implementation";
    public const string SelfReviewFolder = "review-self";
    public const string PastCommentsFolder = "review-against-past-comments";
    public const string PrCommentsFolder = "review-pr-comments";

    /// <summary>Your own free-text notes inside <see cref="ImplementationFolder"/>.</summary>
    public const string NotesFile = "notes.md";

    /// <summary>The handoff file the coding agent is asked to keep updated; surfaced read-only in the Implement step.</summary>
    public const string ProgressFile = "progress.md";

    public const string HistoryFolder = ".leontes/history";

    /// <summary>Folders created inside every task folder.</summary>
    public static readonly IReadOnlyList<string> Subfolders =
    [
        ImagesFolder,
        ImplementationFolder,
        SelfReviewFolder,
        PastCommentsFolder,
        PrCommentsFolder,
        HistoryFolder,
    ];
}
