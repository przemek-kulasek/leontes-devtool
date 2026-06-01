using Leontes.DevTool.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leontes.DevTool.Application.Abstractions;

/// <summary>Persistence seam over the EF Core context, kept in Application so inner layers never depend on Infrastructure.</summary>
public interface IAppDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<Feature> Features { get; }
    DbSet<WorkTask> WorkTasks { get; }
    DbSet<StepState> StepStates { get; }
    DbSet<MaterialLink> MaterialLinks { get; }
    DbSet<DocumentVersion> DocumentVersions { get; }
    DbSet<RulePreset> RulePresets { get; }
    DbSet<ContextSnippet> ContextSnippets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
