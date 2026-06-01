using Leontes.DevTool.Application.Services;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace Leontes.DevTool.Infrastructure.Llm;

/// <summary>Local-LLM helpers over Ollama. A fresh client is built per call so endpoint/model edits in Settings apply immediately.</summary>
public sealed class OllamaLlmService(ISettingsStore settings) : ILlmService
{
    private const string TypoSystemPrompt =
        "You fix typos and grammar. Return only the corrected text, preserving meaning and any Markdown formatting. Do not add commentary.";

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.IsRunningAsync(ct);
        }
        catch
        {
            return false;
        }
    }

    public Task<string> FixTyposAsync(string text, CancellationToken ct = default) =>
        CompleteAsync(TypoSystemPrompt, text, ct);

    public Task<string> OptimizeAsync(string text, CancellationToken ct = default)
    {
        const string system =
            "Rewrite the following document so it is efficient for an LLM to consume. " +
            "Remove duplicated or redundant statements, merge overlapping points, and tighten wording to be precise. " +
            "Preserve every fact, name, path, requirement and constraint. " +
            "Return clean, well-formed Markdown: keep the existing headings and structure, use proper heading levels, lists and code spans, and fix any malformed Markdown. " +
            "Do not add new information. Return only the rewritten document, no commentary.";
        return CompleteAsync(system, text, ct);
    }

    private async Task<string> CompleteAsync(string system, string user, CancellationToken ct)
    {
        using var client = CreateClient();
        IChatClient chat = client;

        List<ChatMessage> messages =
        [
            new(ChatRole.System, system),
            new(ChatRole.User, user),
        ];

        var response = await chat.GetResponseAsync(messages, cancellationToken: ct);
        return response.Text?.Trim() ?? string.Empty;
    }

    private OllamaApiClient CreateClient()
    {
        var config = settings.Load();
        return new OllamaApiClient(new Uri(config.OllamaEndpoint), config.OllamaChatModel);
    }
}
