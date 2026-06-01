using System;
using System.IO;
using Avalonia;
using Leontes.DevTool.Application;
using Leontes.DevTool.Desktop.Services;
using Leontes.DevTool.Desktop.ViewModels;
using Leontes.DevTool.Infrastructure;
using Leontes.DevTool.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            App.Services = BuildServiceProvider();
            InitializeDatabase(App.Services);
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Leontes");
            Directory.CreateDirectory(logDir);
            File.WriteAllText(Path.Combine(logDir, "crash.log"), ex.ToString());
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void InitializeDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<DbInitializer>().InitializeAsync().GetAwaiter().GetResult();
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        services.AddInfrastructure();

        services.AddSingleton<DialogService>();
        services.AddSingleton<IDialogService>(sp => sp.GetRequiredService<DialogService>());

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<TaskViewModel>();
        services.AddTransient<ProjectEditorViewModel>();
        services.AddTransient<FeatureEditorViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider();
    }
}
