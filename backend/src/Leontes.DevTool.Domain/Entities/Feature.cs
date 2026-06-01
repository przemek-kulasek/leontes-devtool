using Leontes.DevTool.Domain.Common;

namespace Leontes.DevTool.Domain.Entities;

/// <summary>
/// A feature groups related tasks. Its context is inherited by the tasks beneath it and layered
/// on top of the parent project's general context.
/// </summary>
public sealed class Feature : Entity
{
    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>Absolute folder for this feature (under the project root).</summary>
    public required string FolderPath { get; set; }

    /// <summary>Feature-level knowledge layered on top of the project's general context.</summary>
    public string? FeatureContext { get; set; }

    public ICollection<WorkTask> Tasks { get; set; } = [];
}
