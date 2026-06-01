using Leontes.DevTool.Domain.Common;

namespace Leontes.DevTool.Domain.Entities;

/// <summary>
/// A reusable agent-instruction rule shown as a checklist on the Rules step (e.g. "never assume —
/// always ask if something is unclear"). Seeded with the user's standard set; editable in settings.
/// </summary>
public sealed class RulePreset : Entity
{
    public required string Name { get; set; }

    public required string Text { get; set; }

    /// <summary>Whether this rule is checked by default on new tasks.</summary>
    public bool DefaultSelected { get; set; } = true;

    /// <summary>Display order in the rules checklist.</summary>
    public int SortOrder { get; set; }
}
