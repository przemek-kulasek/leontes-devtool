using Leontes.DevTool.Domain.Entities;

namespace Leontes.DevTool.Application.Services;

public interface IFeatureService
{
    Task<Feature> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>Creates the feature and its owned folder under the project root.</summary>
    Task<Feature> CreateAsync(Guid projectId, string name, string? description, CancellationToken ct = default);

    Task UpdateAsync(Feature feature, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
