using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Application.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Application;

public static class DependencyInjection
{
    /// <summary>Registers the use-case services that depend only on abstractions (no external resources).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<IProjectService, ProjectService>();
        services.AddTransient<IFeatureService, FeatureService>();
        services.AddTransient<ITaskService, TaskService>();
        services.AddTransient<IKnowledgeAggregator, KnowledgeAggregator>();
        services.AddSingleton<IPromptGenerator, PromptGenerator>();
        services.AddSingleton<IModelRecommendation, ModelRecommendationService>();
        return services;
    }
}
