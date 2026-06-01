using Leontes.DevTool.Application.Abstractions;
using Leontes.DevTool.Domain.Entities;
using Leontes.DevTool.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Leontes.DevTool.Infrastructure.Persistence;

public sealed class LeontesDbContext(DbContextOptions<LeontesDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<StepState> StepStates => Set<StepState>();
    public DbSet<MaterialLink> MaterialLinks => Set<MaterialLink>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<RulePreset> RulePresets => Set<RulePreset>();
    public DbSet<ContextSnippet> ContextSnippets => Set<ContextSnippet>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<StepKind>().HaveConversion<string>();
        builder.Properties<StepStatus>().HaveConversion<string>();
        builder.Properties<WorkTaskStatus>().HaveConversion<string>();
        builder.Properties<MaterialType>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(b =>
        {
            b.HasMany(p => p.Features).WithOne(f => f.Project!).HasForeignKey(f => f.ProjectId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(p => p.ContextSnippets).WithOne(c => c.Project!).HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Feature>()
            .HasMany(f => f.Tasks).WithOne(t => t.Feature!).HasForeignKey(t => t.FeatureId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkTask>(b =>
        {
            b.HasMany(t => t.Steps).WithOne(s => s.WorkTask!).HasForeignKey(s => s.WorkTaskId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(t => t.Materials).WithOne(m => m.WorkTask!).HasForeignKey(m => m.WorkTaskId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(t => t.Versions).WithOne(v => v.WorkTask!).HasForeignKey(v => v.WorkTaskId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(t => new { t.FeatureId, t.JiraKey }).IsUnique();
        });
    }
}
