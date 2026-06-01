using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Application.Services;

/// <summary>Suggests which model to use for a given workflow step (local for light edits, cloud for heavy reasoning).</summary>
public interface IModelRecommendation
{
    ModelSuggestion Recommend(StepKind step);
}
