using Leontes.DevTool.Domain.Entities;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Application.Services;

public interface ITaskService
{
    /// <summary>Loads a task with its steps, materials, and version index.</summary>
    Task<WorkTask> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates the task, lays out its folder tree (images/, implementation/, review-*/, .leontes/history),
    /// and seeds one <see cref="StepState"/> per <see cref="StepKind"/> with suggested models.
    /// </summary>
    Task<WorkTask> CreateAsync(Guid featureId, string jiraKey, string title, CancellationToken ct = default);

    Task UpdateAsync(WorkTask task, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task SetStepStatusAsync(Guid stepId, StepStatus status, CancellationToken ct = default);

    Task SaveStepAsync(Guid stepId, string? prompt, string? notes, CancellationToken ct = default);

    Task<MaterialLink> AddMaterialAsync(Guid taskId, MaterialType type, string pathOrUrl, string label, string? description, CancellationToken ct = default);

    Task RemoveMaterialAsync(Guid materialId, CancellationToken ct = default);
}
