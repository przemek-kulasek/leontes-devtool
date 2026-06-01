using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Desktop.Services;
using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Desktop.ViewModels;

public sealed partial class ProjectEditorViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogs;
    private Project _project = null!;

    public Guid ProjectId { get; private set; }

    /// <summary>Set by the shell so the tree refreshes after a save.</summary>
    public Func<Task>? Saved { get; set; }

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private string? _generalContext;
    [ObservableProperty] private string? _jiraProjectKey;
    [ObservableProperty] private string? _gitHubOwner;
    [ObservableProperty] private string? _gitHubRepo;
    [ObservableProperty] private string _rootFolderPath = string.Empty;
    [ObservableProperty] private string _status = string.Empty;

    public ProjectEditorViewModel(IServiceProvider services, IDialogService dialogs)
    {
        _services = services;
        _dialogs = dialogs;
    }

    public async Task LoadAsync(Guid projectId)
    {
        ProjectId = projectId;
        using var scope = _services.CreateScope();
        _project = await scope.ServiceProvider.GetRequiredService<IProjectService>().GetAsync(projectId);
        Name = _project.Name;
        Description = _project.Description;
        GeneralContext = _project.GeneralContext;
        JiraProjectKey = _project.JiraProjectKey;
        GitHubOwner = _project.GitHubOwner;
        GitHubRepo = _project.GitHubRepo;
        RootFolderPath = _project.RootFolderPath;
        Status = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            _project.Name = Name;
            _project.Description = Description;
            _project.GeneralContext = GeneralContext;
            _project.JiraProjectKey = JiraProjectKey;
            _project.GitHubOwner = GitHubOwner;
            _project.GitHubRepo = GitHubRepo;

            using var scope = _services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IProjectService>().UpdateAsync(_project);

            Status = "Saved";
            if (Saved is not null)
                await Saved();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowMessageAsync("Could not save project", ex.Message);
        }
    }
}
