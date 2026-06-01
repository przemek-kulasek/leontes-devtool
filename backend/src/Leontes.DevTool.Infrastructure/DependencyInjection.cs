using Leontes.DevTool.Application.Abstractions;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Infrastructure.FileSystem;
using Leontes.DevTool.Infrastructure.GitHub;
using Leontes.DevTool.Infrastructure.Jira;
using Leontes.DevTool.Infrastructure.Llm;
using Leontes.DevTool.Infrastructure.Persistence;
using Leontes.DevTool.Infrastructure.Security;
using Leontes.DevTool.Infrastructure.Settings;
using Leontes.DevTool.Infrastructure.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<LeontesDbContext>(
            options => options.UseSqlite($"Data Source={AppPaths.DatabaseFile}"),
            ServiceLifetime.Transient);
        services.AddTransient<IAppDbContext>(sp => sp.GetRequiredService<LeontesDbContext>());
        services.AddTransient<DbInitializer>();

        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<ISecretStore>(_ => new AesFileSecretStore(AppPaths.SecretsFile));
        services.AddSingleton<ISettingsStore>(_ => new JsonSettingsStore(AppPaths.SettingsFile));
        services.AddSingleton<ILlmService, OllamaLlmService>();

        services.AddTransient<IVersioningService, SnapshotVersioningService>();
        services.AddTransient<IGitHubClient, GitHubClientAdapter>();

        services.AddHttpClient<IJiraClient, JiraCloudClient>()
            .AddStandardResilienceHandler();

        return services;
    }
}
