using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Desktop.ViewModels.Items;

public sealed partial class RuleCheckViewModel : ObservableObject
{
    public required string Name { get; init; }
    public required string Text { get; init; }

    [ObservableProperty]
    private bool _isChecked;
}

public sealed partial class MaterialItemViewModel : ObservableObject
{
    public required Guid Id { get; init; }
    public required MaterialType Type { get; init; }
    public required string Label { get; init; }
    public required string PathOrUrl { get; init; }
    public string? Description { get; init; }

    [ObservableProperty]
    private bool _linkedInSpec;

    public string Display => $"{Type}: {Label}";
}

public sealed class VersionItemViewModel
{
    public required Guid Id { get; init; }
    public required string RelativePath { get; init; }
    public required string SnapshotRelativePath { get; init; }
    public string? Label { get; init; }
    public DateTime CreatedUtc { get; init; }

    public string Display =>
        $"{CreatedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}" + (string.IsNullOrWhiteSpace(Label) ? string.Empty : $"  ·  {Label}");
}

public sealed class PullRequestItemViewModel
{
    public required int Number { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }
    public bool IsBot { get; init; }

    public string Display => $"#{Number}  {Title}  —  {Author}{(IsBot ? " (bot)" : string.Empty)}";
}

public sealed class CommentItemViewModel
{
    public required string Author { get; init; }
    public bool IsBot { get; init; }
    public required string Body { get; init; }
    public string? FilePath { get; init; }

    public string Header => $"{Author}{(IsBot ? " (bot)" : string.Empty)}" +
        (string.IsNullOrWhiteSpace(FilePath) ? string.Empty : $"  ·  {FilePath}");
}

public sealed class JiraCommentItemViewModel
{
    public required string Author { get; init; }
    public required string Body { get; init; }
    public required DateTimeOffset Created { get; init; }

    public string Header => $"{Author}  ·  {Created.LocalDateTime:yyyy-MM-dd HH:mm}";
}
