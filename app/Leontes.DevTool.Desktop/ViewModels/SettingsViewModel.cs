using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leontes.DevTool.Application.Common;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Desktop.Services;

namespace Leontes.DevTool.Desktop.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsStore _settings;
    private readonly ISecretStore _secrets;
    private readonly ILlmService _llm;
    private readonly IDialogService _dialogs;

    [ObservableProperty] private string? _workspaceRootPath;
    [ObservableProperty] private string? _jiraBaseUrl;
    [ObservableProperty] private string? _jiraEmail;
    [ObservableProperty] private string? _jiraApiToken;
    [ObservableProperty] private string _jiraAcceptanceCriteriaFieldId = "customfield_10016";
    [ObservableProperty] private string _ollamaEndpoint = "http://localhost:11434";
    [ObservableProperty] private string _ollamaChatModel = "qwen2.5:7b-instruct";
    [ObservableProperty] private string? _gitHubDefaultOwner;
    [ObservableProperty] private string? _gitHubDefaultRepo;
    [ObservableProperty] private string? _gitHubPat;
    [ObservableProperty] private string? _ollamaStatus;

    public SettingsViewModel(ISettingsStore settings, ISecretStore secrets, ILlmService llm, IDialogService dialogs)
    {
        _settings = settings;
        _secrets = secrets;
        _llm = llm;
        _dialogs = dialogs;
        Load();
    }

    private void Load()
    {
        var s = _settings.Load();
        WorkspaceRootPath = s.WorkspaceRootPath;
        JiraBaseUrl = s.JiraBaseUrl;
        JiraEmail = s.JiraEmail;
        OllamaEndpoint = s.OllamaEndpoint;
        JiraAcceptanceCriteriaFieldId = s.JiraAcceptanceCriteriaFieldId;
        OllamaChatModel = s.OllamaChatModel;
        GitHubDefaultOwner = s.GitHubDefaultOwner;
        GitHubDefaultRepo = s.GitHubDefaultRepo;
        JiraApiToken = _secrets.Get(SecretKeys.JiraApiToken);
        GitHubPat = _secrets.Get(SecretKeys.GitHubPat);
    }

    public void Save()
    {
        _settings.Save(new AppSettings
        {
            WorkspaceRootPath = WorkspaceRootPath,
            JiraBaseUrl = JiraBaseUrl,
            JiraEmail = JiraEmail,
            OllamaEndpoint = OllamaEndpoint,
            OllamaChatModel = OllamaChatModel,
            GitHubDefaultOwner = GitHubDefaultOwner,
            GitHubDefaultRepo = GitHubDefaultRepo,
            JiraAcceptanceCriteriaFieldId = JiraAcceptanceCriteriaFieldId,
        });

        StoreSecret(SecretKeys.JiraApiToken, JiraApiToken);
        StoreSecret(SecretKeys.GitHubPat, GitHubPat);
    }

    [RelayCommand]
    private async Task BrowseRootAsync()
    {
        var folder = await _dialogs.PickFolderAsync("Choose the workspace root folder");
        if (folder is not null)
            WorkspaceRootPath = folder;
    }

    [RelayCommand]
    private async Task TestOllamaAsync()
    {
        Save();
        OllamaStatus = "Checking…";
        var ok = await _llm.IsAvailableAsync();
        OllamaStatus = ok ? "Connected ✓" : "Not reachable ✗";
    }

    private void StoreSecret(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            _secrets.Delete(key);
        else
            _secrets.Set(key, value);
    }
}
