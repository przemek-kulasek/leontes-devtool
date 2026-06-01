using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Leontes.DevTool.Desktop.Services;
using Leontes.DevTool.Desktop.ViewModels;
using Leontes.DevTool.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Desktop;

public partial class App : Avalonia.Application
{
    /// <summary>Set by <see cref="Program"/> before the Avalonia lifetime starts.</summary>
    public static IServiceProvider Services { get; set; } = default!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            var mainWindow = new MainWindow { DataContext = mainViewModel };

            (Services.GetRequiredService<IDialogService>() as DialogService)!.Owner = mainWindow;

            desktop.MainWindow = mainWindow;
            _ = mainViewModel.LoadAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
