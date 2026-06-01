namespace Leontes.DevTool.Application.Services;

/// <summary>Small local-LLM helpers (typo fixing, context optimization) backed by Ollama.</summary>
public interface ILlmService
{
    /// <summary>Returns true if the configured Ollama endpoint is reachable.</summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>Returns the text with typos and grammar fixed, preserving meaning and markdown.</summary>
    Task<string> FixTyposAsync(string text, CancellationToken ct = default);

    /// <summary>
    /// Rewrites the text to be efficient for an LLM to consume: removes duplicated/redundant
    /// information and tightens wording while preserving every fact, name, path and constraint.
    /// </summary>
    Task<string> OptimizeAsync(string text, CancellationToken ct = default);
}
