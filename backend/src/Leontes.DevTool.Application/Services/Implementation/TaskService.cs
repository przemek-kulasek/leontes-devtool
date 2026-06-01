using Leontes.DevTool.Application.Abstractions;
using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Entities;
using Leontes.DevTool.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Leontes.DevTool.Application.Services.Implementation;

public sealed class TaskService(IAppDbContext db, IFileSystemService fs, IModelRecommendation models) : ITaskService
{
    public async Task<WorkTask> GetAsync(Guid id, CancellationToken ct = default) =>
        await db.WorkTasks
            .Include(t => t.Steps)
            .Include(t => t.Materials)
            .Include(t => t.Versions)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
        ?? throw new NotFoundException(nameof(WorkTask), id);

    public async Task<WorkTask> CreateAsync(Guid featureId, string jiraKey, string title, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jiraKey))
            throw new ValidationException("A task key is required.");

        var feature = await db.Features.FirstOrDefaultAsync(f => f.Id == featureId, ct)
            ?? throw new NotFoundException(nameof(Feature), featureId);

        var folder = fs.EnsureTaskFolder(feature.FolderPath, jiraKey);
        var task = new WorkTask
        {
            FeatureId = featureId,
            JiraKey = jiraKey.Trim(),
            Title = string.IsNullOrWhiteSpace(title) ? jiraKey.Trim() : title.Trim(),
            FolderPath = folder,
            Steps = SeedSteps(),
        };

        db.WorkTasks.Add(task);
        await db.SaveChangesAsync(ct);
        return task;
    }

    public async Task UpdateAsync(WorkTask task, CancellationToken ct = default)
    {
        task.ModifiedUtc = DateTime.UtcNow;
        db.WorkTasks.Update(task);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await db.WorkTasks.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException(nameof(WorkTask), id);
        db.WorkTasks.Remove(task);
        await db.SaveChangesAsync(ct);
    }

    public async Task SetStepStatusAsync(Guid stepId, StepStatus status, CancellationToken ct = default)
    {
        var step = await db.StepStates.FirstOrDefaultAsync(s => s.Id == stepId, ct)
            ?? throw new NotFoundException(nameof(StepState), stepId);

        step.Status = status;
        step.CompletedUtc = status == StepStatus.Done ? DateTime.UtcNow : null;
        step.ModifiedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveStepAsync(Guid stepId, string? prompt, string? notes, CancellationToken ct = default)
    {
        var step = await db.StepStates.FirstOrDefaultAsync(s => s.Id == stepId, ct)
            ?? throw new NotFoundException(nameof(StepState), stepId);

        step.GeneratedPrompt = prompt;
        step.Notes = notes;
        step.ModifiedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<MaterialLink> AddMaterialAsync(Guid taskId, MaterialType type, string pathOrUrl, string label, string? description, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
            throw new ValidationException("A path or URL is required for a material.");

        _ = await db.WorkTasks.FirstOrDefaultAsync(t => t.Id == taskId, ct)
            ?? throw new NotFoundException(nameof(WorkTask), taskId);

        var material = new MaterialLink
        {
            WorkTaskId = taskId,
            Type = type,
            PathOrUrl = pathOrUrl.Trim(),
            Label = string.IsNullOrWhiteSpace(label) ? pathOrUrl.Trim() : label.Trim(),
            Description = description,
        };

        db.MaterialLinks.Add(material);
        await db.SaveChangesAsync(ct);
        return material;
    }

    public async Task RemoveMaterialAsync(Guid materialId, CancellationToken ct = default)
    {
        var material = await db.MaterialLinks.FirstOrDefaultAsync(m => m.Id == materialId, ct)
            ?? throw new NotFoundException(nameof(MaterialLink), materialId);
        db.MaterialLinks.Remove(material);
        await db.SaveChangesAsync(ct);
    }

    private List<StepState> SeedSteps() =>
        [.. Enum.GetValues<StepKind>().Select(kind => new StepState
        {
            Kind = kind,
            SuggestedModel = models.Recommend(kind).Model,
        })];
}
