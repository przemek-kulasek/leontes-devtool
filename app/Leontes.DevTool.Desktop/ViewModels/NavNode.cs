using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Leontes.DevTool.Desktop.ViewModels;

public enum NavKind
{
    Project,
    Feature,
    WorkTask,
}

/// <summary>A node in the Projects → Features → Tasks navigation tree.</summary>
public sealed partial class NavNode : ObservableObject
{
    public required NavKind Kind { get; init; }
    public required Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid FeatureId { get; init; }

    [ObservableProperty]
    private string _name = string.Empty;

    public ObservableCollection<NavNode> Children { get; } = [];

    public string Icon => Kind switch
    {
        NavKind.Project => "📁",
        NavKind.Feature => "📦",
        NavKind.WorkTask => "📝",
        _ => "•",
    };
}
