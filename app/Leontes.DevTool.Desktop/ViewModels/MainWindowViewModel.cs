using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Desktop.Services;
using Leontes.DevTool.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogs;
    private readonly ISettingsStore _settings;

    public ObservableCollection<NavNode> Roots { get; } = [];

    [ObservableProperty] private NavNode? _selectedNode;
    [ObservableProperty] private object? _currentContent;
    [ObservableProperty] private string _status = "Ready";
    [ObservableProperty] private bool _isBusy;

    public MainWindowViewModel(IServiceProvider services, IDialogService dialogs, ISettingsStore settings)
    {
        _services = services;
        _dialogs = dialogs;
        _settings = settings;
    }

    public Task LoadAsync() => RefreshAsync();

    partial void OnSelectedNodeChanged(NavNode? value)
    {
        if (value is not null)
            _ = OpenAsync(value);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await RunAsync("Loading…", async () =>
        {
            using var scope = _services.CreateScope();
            var projects = await scope.ServiceProvider.GetRequiredService<IProjectService>().GetAllAsync();

            Roots.Clear();
            foreach (var p in projects)
            {
                var projectNode = new NavNode { Kind = NavKind.Project, Id = p.Id, ProjectId = p.Id, Name = p.Name };
                foreach (var f in p.Features)
                {
                    var featureNode = new NavNode
                    {
                        Kind = NavKind.Feature, Id = f.Id, ProjectId = p.Id, FeatureId = f.Id, Name = f.Name,
                    };
                    foreach (var t in f.Tasks)
                        featureNode.Children.Add(new NavNode
                        {
                            Kind = NavKind.WorkTask, Id = t.Id, ProjectId = p.Id, FeatureId = f.Id,
                            Name = $"{t.JiraKey} — {t.Title}",
                        });
                    projectNode.Children.Add(featureNode);
                }
                Roots.Add(projectNode);
            }
        });
    }

    [RelayCommand]
    private async Task AddProjectAsync()
    {
        var root = _settings.Load().WorkspaceRootPath;
        if (string.IsNullOrWhiteSpace(root))
        {
            root = await _dialogs.PickFolderAsync("Choose the workspace root folder for your projects");
            if (string.IsNullOrWhiteSpace(root))
                return;

            var s = _settings.Load();
            s.WorkspaceRootPath = root;
            _settings.Save(s);
        }

        var values = await _dialogs.PromptFormAsync("New project",
        [
            new DialogField("Name"),
            new DialogField("Description", Multiline: true),
        ]);
        if (values is null)
            return;

        await RunAsync("Creating project…", async () =>
        {
            using var scope = _services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IProjectService>()
                .CreateAsync(values["Name"], root!, values["Description"]);
        });
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task AddFeatureAsync()
    {
        if (SelectedNode is not { } node)
        {
            await _dialogs.ShowMessageAsync("Select a project", "Select a project (or one of its items) first.");
            return;
        }

        var values = await _dialogs.PromptFormAsync("New feature",
        [
            new DialogField("Name"),
            new DialogField("Description", Multiline: true),
        ]);
        if (values is null)
            return;

        await RunAsync("Creating feature…", async () =>
        {
            using var scope = _services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IFeatureService>()
                .CreateAsync(node.ProjectId, values["Name"], values["Description"]);
        });
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task AddTaskAsync()
    {
        if (SelectedNode is not { Kind: NavKind.Feature or NavKind.WorkTask } node)
        {
            await _dialogs.ShowMessageAsync("Select a feature", "Select the feature (or a task within it) to add a task to.");
            return;
        }

        var values = await _dialogs.PromptFormAsync("New task",
        [
            new DialogField("Key", "e.g. MA-777"),
            new DialogField("Title"),
        ]);
        if (values is null)
            return;

        await RunAsync("Creating task…", async () =>
        {
            using var scope = _services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ITaskService>()
                .CreateAsync(node.FeatureId, values["Key"], values["Title"]);
        });
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedNode is not { } node)
            return;

        var confirmed = await _dialogs.ConfirmAsync("Delete",
            $"Remove '{node.Name}' from Leontes? Files on disk are kept.");
        if (!confirmed)
            return;

        await RunAsync("Deleting…", async () =>
        {
            using var scope = _services.CreateScope();
            var sp = scope.ServiceProvider;
            switch (node.Kind)
            {
                case NavKind.Project: await sp.GetRequiredService<IProjectService>().DeleteAsync(node.Id); break;
                case NavKind.Feature: await sp.GetRequiredService<IFeatureService>().DeleteAsync(node.Id); break;
                case NavKind.WorkTask: await sp.GetRequiredService<ITaskService>().DeleteAsync(node.Id); break;
            }
        });

        if (CurrentContent is TaskViewModel task && node.Id == task.TaskId)
            CurrentContent = null;
        await RefreshAsync();
    }

    [RelayCommand]
    private Task OpenSettingsAsync() => _dialogs.ShowSettingsAsync();

    private Task OpenAsync(NavNode node) => node.Kind switch
    {
        NavKind.WorkTask => OpenContentAsync("Opening task…", async () =>
        {
            var vm = _services.GetRequiredService<TaskViewModel>();
            await vm.LoadAsync(node.Id);
            return vm;
        }),
        NavKind.Project => OpenContentAsync("Opening project…", async () =>
        {
            var vm = _services.GetRequiredService<ProjectEditorViewModel>();
            await vm.LoadAsync(node.Id);
            vm.Saved = RefreshAsync;
            return vm;
        }),
        NavKind.Feature => OpenContentAsync("Opening feature…", async () =>
        {
            var vm = _services.GetRequiredService<FeatureEditorViewModel>();
            await vm.LoadAsync(node.Id);
            vm.Saved = RefreshAsync;
            return vm;
        }),
        _ => Task.CompletedTask,
    };

    private Task OpenContentAsync(string busyMessage, Func<Task<object>> factory) =>
        RunAsync(busyMessage, async () => CurrentContent = await factory());

    private async Task RunAsync(string busyMessage, Func<Task> action)
    {
        try
        {
            IsBusy = true;
            Status = busyMessage;
            await action();
            Status = "Ready";
        }
        catch (ValidationException ex)
        {
            Status = "Ready";
            await _dialogs.ShowMessageAsync("Validation", string.Join(Environment.NewLine, ex.Errors));
        }
        catch (Exception ex)
        {
            Status = "Ready";
            await _dialogs.ShowMessageAsync("Something went wrong", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
