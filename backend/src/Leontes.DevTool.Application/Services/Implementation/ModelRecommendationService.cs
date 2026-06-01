using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Application.Services.Implementation;

/// <summary>
/// Static recommendation map: heavy reasoning steps (spec authoring, implementation, review) suggest
/// a frontier cloud model; light text steps suggest the local Ollama model to keep work offline and free.
/// </summary>
public sealed class ModelRecommendationService : IModelRecommendation
{
    private const string CloudModel = "claude-opus-4-8";
    private const string LocalModel = "qwen2.5:7b-instruct";

    public ModelSuggestion Recommend(StepKind step) => step switch
    {
        StepKind.Intake => new(LocalModel, "Summarizing/cleaning ticket text is light work — keep it local.", true),
        StepKind.Materials => new(LocalModel, "Organizing materials needs no frontier model.", true),
        StepKind.Rules => new(LocalModel, "Rule selection is local-only.", true),
        StepKind.Context => new(LocalModel, "Context optimization runs well on a local model.", true),
        StepKind.SpecAndKickoff => new(CloudModel, "Authoring a precise spec benefits from a frontier model.", false),
        StepKind.Implement => new(CloudModel, "Implementation is the highest-leverage step — use the strongest model.", false),
        StepKind.SelfReview => new(CloudModel, "Code review quality scales with model strength.", false),
        StepKind.PastPrComments => new(LocalModel, "Cross-referencing past comments is mostly retrieval.", true),
        StepKind.PrComments => new(CloudModel, "Judging which review comments to act on benefits from strong reasoning.", false),
        _ => new(LocalModel, "Default to local.", true),
    };
}
