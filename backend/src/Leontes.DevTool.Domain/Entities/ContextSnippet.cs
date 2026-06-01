using Leontes.DevTool.Domain.Common;

namespace Leontes.DevTool.Domain.Entities;

/// <summary>
/// Reusable context text the user types often (e.g. tech-stack notes, conventions). Scoped to a
/// project so it can be offered as a selectable preset when composing a task's context.
/// </summary>
public sealed class ContextSnippet : Entity
{
    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }

    public required string Title { get; set; }

    public required string Text { get; set; }
}
