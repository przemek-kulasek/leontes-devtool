using Leontes.DevTool.Application.Abstractions;
using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leontes.DevTool.Application.Services.Implementation;

public sealed class FeatureService(IAppDbContext db, IFileSystemService fs) : IFeatureService
{
    public async Task<Feature> GetAsync(Guid id, CancellationToken ct = default) =>
        await db.Features.FirstOrDefaultAsync(f => f.Id == id, ct)
        ?? throw new NotFoundException(nameof(Feature), id);

    public async Task<Feature> CreateAsync(Guid projectId, string name, string? description, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Feature name is required.");

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == projectId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        var folder = fs.EnsureFeatureFolder(project.RootFolderPath, name);
        var feature = new Feature { ProjectId = projectId, Name = name.Trim(), Description = description, FolderPath = folder };

        db.Features.Add(feature);
        await db.SaveChangesAsync(ct);
        return feature;
    }

    public async Task UpdateAsync(Feature feature, CancellationToken ct = default)
    {
        feature.ModifiedUtc = DateTime.UtcNow;
        db.Features.Update(feature);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var feature = await db.Features.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new NotFoundException(nameof(Feature), id);
        db.Features.Remove(feature);
        await db.SaveChangesAsync(ct);
    }
}
