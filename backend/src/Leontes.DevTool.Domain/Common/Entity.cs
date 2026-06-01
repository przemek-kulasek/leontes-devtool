namespace Leontes.DevTool.Domain.Common;

/// <summary>Base type for all persisted aggregates. Identity is a Guid; audit timestamps are UTC.</summary>
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedUtc { get; set; }
}
