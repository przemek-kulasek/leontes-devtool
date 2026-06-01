using Leontes.DevTool.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leontes.DevTool.Infrastructure.Persistence;

/// <summary>Applies migrations on startup and seeds the default rule presets once.</summary>
public sealed class DbInitializer(LeontesDbContext db)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);
        await SeedRulePresetsAsync(ct);
    }

    private async Task SeedRulePresetsAsync(CancellationToken ct)
    {
        var existing = await db.RulePresets.Select(r => r.Name).ToListAsync(ct);
        var missing = DefaultRules.Where(r => !existing.Contains(r.Name)).ToList();
        if (missing.Count == 0)
            return;

        db.RulePresets.AddRange(missing);
        await db.SaveChangesAsync(ct);
    }

    private static IEnumerable<RulePreset> DefaultRules =>
    [
        new() { Name = "No trivial questions", SortOrder = 0,
            Text = "Do not ask about trivial things you can find in the given materials and locations." },
        new() { Name = "Ask when unclear", SortOrder = 1,
            Text = "Always ask if something is unclear and you cannot find it in the materials. Never make assumptions or hallucinate." },
        new() { Name = "Clean code & conventions", SortOrder = 2,
            Text = "Unless specified otherwise, use clean code, DRY, YAGNI and SOLID. Match the existing codebase style and reuse its patterns." },
        new() { Name = "Ask before new packages", SortOrder = 3,
            Text = "Always ask before using an external package and present its license information." },
        new() { Name = "No tests yet", SortOrder = 4,
            Text = "Do not add tests at this stage." },
        new() { Name = "Minimal, focused changes", SortOrder = 5,
            Text = "Keep changes minimal and scoped to this ticket. Do not refactor unrelated code or add speculative features." },
        new() { Name = "Prefer editing existing code", SortOrder = 6,
            Text = "Prefer extending existing code and components over rewriting; reuse what is already there." },
        new() { Name = "Match error handling & logging", SortOrder = 7,
            Text = "Follow the project's existing error-handling, validation and logging patterns." },
        new() { Name = "No secrets in code", SortOrder = 8,
            Text = "Never put secrets, API keys or credentials in source, config or commits." },
        new() { Name = "Confirm before large refactors", SortOrder = 9,
            Text = "Stop and confirm before any large refactor, dependency change, or schema/migration change." },
        new() { Name = "Update docs when behavior changes", SortOrder = 10,
            Text = "Update README/relevant docs when you add, remove or change behavior, dependencies or setup." },
        new() { Name = "Behavior parity with legacy", SortOrder = 11, DefaultSelected = false,
            Text = "Behavior must match the existing/legacy app unless the ticket explicitly says otherwise." },
    ];
}
