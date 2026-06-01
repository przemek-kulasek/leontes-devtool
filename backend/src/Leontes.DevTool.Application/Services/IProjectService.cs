using Leontes.DevTool.Domain.Entities;

namespace Leontes.DevTool.Application.Services;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);

    Task<Project> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>Creates the project and its owned root folder.</summary>
    Task<Project> CreateAsync(string name, string workspaceRootPath, string? description, CancellationToken ct = default);

    Task UpdateAsync(Project project, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
