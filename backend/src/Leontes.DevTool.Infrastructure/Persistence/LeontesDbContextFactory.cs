using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Leontes.DevTool.Infrastructure.Persistence;

/// <summary>Lets `dotnet ef` build the context at design time without the desktop host.</summary>
public sealed class LeontesDbContextFactory : IDesignTimeDbContextFactory<LeontesDbContext>
{
    public LeontesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LeontesDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabaseFile}")
            .Options;
        return new LeontesDbContext(options);
    }
}
