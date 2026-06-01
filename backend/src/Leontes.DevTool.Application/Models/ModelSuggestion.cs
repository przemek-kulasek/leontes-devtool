namespace Leontes.DevTool.Application.Models;

/// <summary>A recommended model for a workflow step, plus why and whether it runs locally.</summary>
public sealed record ModelSuggestion(string Model, string Rationale, bool IsLocal);
