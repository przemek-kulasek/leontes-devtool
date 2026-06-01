using Leontes.DevTool.Application.Abstractions;
using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leontes.DevTool.Application.Services.Implementation;

public sealed class ProjectService(IAppDbContext db, IFileSystemService fs) : IProjectService
{
    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default) =>
        await db.Projects
            .Include(p => p.Features)
            .ThenInclude(f => f.Tasks)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<Project> GetAsync(Guid id, CancellationToken ct = default) =>
        await db.Projects
            .Include(p => p.ContextSnippets)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
        ?? throw new NotFoundException(nameof(Project), id);

    public async Task<Project> CreateAsync(string name, string workspaceRootPath, string? description, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Project name is required.");
        if (string.IsNullOrWhiteSpace(workspaceRootPath))
            throw new ValidationException("A workspace root folder is required.");

        var folder = fs.EnsureProjectFolder(workspaceRootPath, name);
        var project = new Project { Name = name.Trim(), RootFolderPath = folder, Description = description };

        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        return project;
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        project.ModifiedUtc = DateTime.UtcNow;
        db.Projects.Update(project);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(Project), id);
        db.Projects.Remove(project);
        await db.SaveChangesAsync(ct);
    }
}
