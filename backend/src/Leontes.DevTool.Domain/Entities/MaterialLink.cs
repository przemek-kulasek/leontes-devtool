using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Domain.Entities;

/// <summary>A piece of supporting material attached to a task (folder, image, snippet, link).</summary>
public sealed class MaterialLink : Entity
{
    public Guid WorkTaskId { get; set; }

    public WorkTask? WorkTask { get; set; }

    public MaterialType Type { get; set; }

    /// <summary>Filesystem path or URL, depending on <see cref="Type"/>.</summary>
    public required string PathOrUrl { get; set; }

    public required string Label { get; set; }

    public string? Description { get; set; }

    /// <summary>When true, this material is referenced in the generated spec.md.</summary>
    public bool LinkedInSpec { get; set; } = true;
}
