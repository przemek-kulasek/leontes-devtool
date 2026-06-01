using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leontes.DevTool.Application.Abstractions;
using Leontes.DevTool.Application.Common;
using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Desktop.Services;
using Leontes.DevTool.Desktop.ViewModels.Items;
using Leontes.DevTool.Domain.Common;
using Leontes.DevTool.Domain.Entities;
using Leontes.DevTool.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Desktop.ViewModels;

public sealed partial class TaskViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogs;
    private readonly IPromptGenerator _prompts;
    private readonly IFileSystemService _fs;
    private readonly ISettingsStore _settings;
    private readonly ILlmService _llm;

    private WorkTask _task = null!;
    private JiraTicket? _ticket;

    public Guid TaskId { get; private set; }

    [ObservableProperty] private string _header = string.Empty;
    [ObservableProperty] private string _status = "Ready";
    [ObservableProperty] private bool _isBusy;

    public ObservableCollection<StepViewModel> Steps { get; } = [];
    [ObservableProperty] private StepViewModel? _selectedStep;

    // Intake
    [ObservableProperty] private string _jiraKey = string.Empty;
    [ObservableProperty] private string? _ticketSummary;
    [ObservableProperty] private string? _ticketType;
    [ObservableProperty] private string? _ticketStatus;
    [ObservableProperty] private string? _ticketDescription;
    [ObservableProperty] private string? _ticketAcceptanceCriteria;
    [ObservableProperty] private string? _ticketAttachmentsInfo;
    [ObservableProperty] private bool _hasTicketComments;

    public ObservableCollection<JiraCommentItemViewModel> TicketComments { get; } = [];

    // Materials
    public ObservableCollection<MaterialItemViewModel> Materials { get; } = [];

    // Rules
    public ObservableCollection<RuleCheckViewModel> Rules { get; } = [];

    // Context
    [ObservableProperty] private string _inheritedContext = string.Empty;
    [ObservableProperty] private string _extraContext = string.Empty;

    // Spec & kickoff
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerateSpec))]
    private string _specText = string.Empty;

    /// <summary>Generation is offered only until a spec exists; after that the user edits in place.</summary>
    public bool CanGenerateSpec => string.IsNullOrWhiteSpace(SpecText);

    [ObservableProperty] private bool _showPreview;
    [ObservableProperty] private string? _kickoffPrompt;
    public ObservableCollection<VersionItemViewModel> Versions { get; } = [];
    [ObservableProperty] private VersionItemViewModel? _selectedVersion;
    [ObservableProperty] private string? _diffText;

    /// <summary>Optimized spec awaiting the user's Apply/Discard decision; null when no preview is pending.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOptimizedPreview))]
    private string? _optimizedSpec;

    public bool HasOptimizedPreview => OptimizedSpec is not null;

    // Implement
    [ObservableProperty] private string _implementationNotes = string.Empty;

    /// <summary>Read-only view of the handoff file the coding agent keeps updated; reloaded on demand.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImplementationProgress))]
    private string _implementationProgress = string.Empty;

    public bool HasImplementationProgress => !string.IsNullOrWhiteSpace(ImplementationProgress);

    // Self review
    [ObservableProperty] private string? _selfReviewPrompt;

    // Past PR comments
    [ObservableProperty] private string? _gitHubOwner;
    [ObservableProperty] private string? _gitHubRepo;
    public ObservableCollection<PullRequestItemViewModel> ClosedPullRequests { get; } = [];
    [ObservableProperty] private PullRequestItemViewModel? _selectedPullRequest;
    [ObservableProperty] private string? _pastCommentsPrompt;

    // PR comments
    [ObservableProperty] private string _prNumberText = string.Empty;
    public ObservableCollection<CommentItemViewModel> PrComments { get; } = [];
    [ObservableProperty] private string? _prCommentsPrompt;

    public TaskViewModel(
        IServiceProvider services, IDialogService dialogs, IPromptGenerator prompts,
        IFileSystemService fs, ISettingsStore settings, ILlmService llm)
    {
        _services = services;
        _dialogs = dialogs;
        _prompts = prompts;
        _fs = fs;
        _settings = settings;
        _llm = llm;
    }

    public async Task LoadAsync(Guid taskId)
    {
        TaskId = taskId;

        using (var scope = _services.CreateScope())
        {
            var sp = scope.ServiceProvider;
            _task = await sp.GetRequiredService<ITaskService>().GetAsync(taskId);
            InheritedContext = await sp.GetRequiredService<IKnowledgeAggregator>().ComposeContextAsync(taskId, null);

            var db = sp.GetRequiredService<IAppDbContext>();

            var presets = await db.RulePresets
                .OrderBy(r => r.SortOrder).AsNoTracking().ToListAsync();
            Rules.Clear();
            foreach (var r in presets)
                Rules.Add(new RuleCheckViewModel { Name = r.Name, Text = r.Text, IsChecked = r.DefaultSelected });
        }

        Header = $"{_task.JiraKey} — {_task.Title}";
        JiraKey = _task.JiraKey;

        Steps.Clear();
        foreach (var s in _task.Steps.OrderBy(s => s.Kind))
            Steps.Add(new StepViewModel { Id = s.Id, Kind = s.Kind, Status = s.Status, SuggestedModel = s.SuggestedModel });
        SelectedStep = Steps.FirstOrDefault();

        Materials.Clear();
        foreach (var m in _task.Materials)
            Materials.Add(new MaterialItemViewModel
            {
                Id = m.Id, Type = m.Type, Label = m.Label, PathOrUrl = m.PathOrUrl,
                Description = m.Description, LinkedInSpec = m.LinkedInSpec,
            });

        ExtraContext = _task.Steps.FirstOrDefault(s => s.Kind == StepKind.Context)?.Notes ?? string.Empty;

        SpecText = _fs.ReadText(_fs.Combine(_task.FolderPath, TaskLayout.SpecFile));
        ImplementationNotes = _fs.ReadText(NotesPath());
        ImplementationProgress = _fs.ReadText(ProgressPath());

        var config = _settings.Load();
        GitHubOwner = config.GitHubDefaultOwner;
        GitHubRepo = config.GitHubDefaultRepo;

        await ReloadVersionsAsync();
    }

    partial void OnSelectedStepChanged(StepViewModel? value)
    {
        if (value is { Status: StepStatus.NotStarted })
            _ = SetStepStatusAsync(value, StepStatus.InProgress);

        if (value is { Kind: StepKind.Context })
            _ = RefreshInheritedContextAsync();
    }

    // ---- Intake ----------------------------------------------------------

    [RelayCommand]
    private Task FetchTicketAsync() => RunAsync("Fetching Jira ticket…", async () =>
    {
        using var scope = _services.CreateScope();
        var jira = scope.ServiceProvider.GetRequiredService<IJiraClient>();
        _ticket = await jira.GetIssueAsync(JiraKey);

        TicketSummary = _ticket.Summary;
        TicketType = _ticket.IssueType;
        TicketStatus = _ticket.Status;
        TicketDescription = _ticket.DescriptionMarkdown;
        TicketAcceptanceCriteria = _ticket.AcceptanceCriteria;

        TicketComments.Clear();
        foreach (var c in _ticket.Comments)
            TicketComments.Add(new JiraCommentItemViewModel { Author = c.Author, Body = c.BodyMarkdown, Created = c.Created });
        HasTicketComments = TicketComments.Count > 0;

        TicketAttachmentsInfo = null;
        if (_ticket.Attachments.Count > 0)
        {
            var imagesFolder = _fs.Combine(_task.FolderPath, TaskLayout.ImagesFolder);
            var downloaded = await jira.DownloadAttachmentsAsync(_ticket.Attachments, imagesFolder);
            var imageTotal = _ticket.Attachments.Count(a => a.IsImage);
            var nonImage = _ticket.Attachments.Count - imageTotal;
            TicketAttachmentsInfo = downloaded > 0
                ? $"Downloaded {downloaded} image{(downloaded == 1 ? string.Empty : "s")} to images/" +
                  (nonImage > 0 ? $"  ·  {nonImage} non-image attachment{(nonImage == 1 ? string.Empty : "s")} skipped" : string.Empty)
                : nonImage > 0
                    ? $"{nonImage} attachment{(nonImage == 1 ? string.Empty : "s")} — no images to download"
                    : null;
        }

        await MarkDoneAsync(StepKind.Intake);
    });

    // ---- Materials -------------------------------------------------------

    [RelayCommand]
    private async Task AddFolderMaterialAsync()
    {
        var path = await _dialogs.PickFolderAsync("Choose a folder to reference");
        if (string.IsNullOrWhiteSpace(path))
            return;

        var defaultLabel = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
        var values = await _dialogs.PromptFormAsync("Add folder material",
            [new DialogField("Label", defaultLabel)]);
        if (values is null)
            return;

        await AddMaterialAsync(MaterialType.Folder, path, values["Label"]);
    }

    [RelayCommand]
    private async Task AddLinkMaterialAsync()
    {
        var values = await _dialogs.PromptFormAsync("Add a link",
            [new DialogField("Label"), new DialogField("URL")]);
        if (values is null)
            return;
        await AddMaterialAsync(MaterialType.Url, values["URL"], values["Label"]);
    }

    [RelayCommand]
    private async Task RemoveMaterialAsync(MaterialItemViewModel? material)
    {
        if (material is null)
            return;
        await RunAsync("Removing material…", async () =>
        {
            using var scope = _services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ITaskService>().RemoveMaterialAsync(material.Id);
            Materials.Remove(material);
        });
    }

    private async Task AddMaterialAsync(MaterialType type, string pathOrUrl, string label)
    {
        using var scope = _services.CreateScope();
        var created = await scope.ServiceProvider.GetRequiredService<ITaskService>()
            .AddMaterialAsync(TaskId, type, pathOrUrl, label, null);
        Materials.Add(new MaterialItemViewModel
        {
            Id = created.Id, Type = created.Type, Label = created.Label,
            PathOrUrl = created.PathOrUrl, Description = created.Description, LinkedInSpec = created.LinkedInSpec,
        });
        await MarkDoneAsync(StepKind.Materials);
    }

    // ---- Context ---------------------------------------------------------

    [RelayCommand]
    private Task SaveContextAsync() => RunAsync("Saving context…", async () =>
    {
        var step = Steps.FirstOrDefault(s => s.Kind == StepKind.Context);
        if (step is null)
            return;

        using var scope = _services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ITaskService>()
            .SaveStepAsync(step.Id, null, ExtraContext);
        await SetStepStatusAsync(step, StepStatus.Done);
        Status = "Context saved";
    });

    [RelayCommand]
    private Task RefreshInheritedContextAsync() => RunAsync("Refreshing inherited context…", async () =>
    {
        using var scope = _services.CreateScope();
        InheritedContext = await scope.ServiceProvider.GetRequiredService<IKnowledgeAggregator>()
            .ComposeContextAsync(TaskId, null);
        if (string.IsNullOrWhiteSpace(InheritedContext))
            Status = "No project/feature context yet — select the Project or Feature in the workspace and fill in its context.";
    });

    // ---- Spec & kickoff --------------------------------------------------

    [RelayCommand]
    private Task GenerateSpecAsync() => RunAsync("Composing spec…", async () =>
    {
        if (!CanGenerateSpec)
            return;

        string composed;
        using (var scope = _services.CreateScope())
            composed = await scope.ServiceProvider.GetRequiredService<IKnowledgeAggregator>()
                .ComposeContextAsync(TaskId, ExtraContext);

        var materials = Materials.Select(m => new MaterialLink
        {
            WorkTaskId = TaskId, Type = m.Type, Label = m.Label, PathOrUrl = m.PathOrUrl,
            Description = m.Description, LinkedInSpec = m.LinkedInSpec,
        }).ToList();

        SpecText = _prompts.ComposeSpecMarkdown(_task, EffectiveTicket(), materials, SelectedRules(), composed);

        // First generation writes spec.md and captures the initial snapshot automatically.
        await PersistSpecAsync(snapshot: true);
        Status = "Spec generated and saved as the first version — edit it, fill in “What to do”, then Save to snapshot changes.";
    });

    [RelayCommand]
    private Task SaveSpecAsync() => RunAsync("Saving spec…", async () =>
    {
        await PersistSpecAsync(snapshot: true);
        await MarkDoneAsync(StepKind.SpecAndKickoff);
    });

    [RelayCommand]
    private Task CopyKickoffAsync() => RunAsync("Copying kickoff prompt…", async () =>
    {
        // Persist the current editor text so the kickoff prompt (and spec.md it points to) match the screen.
        await PersistSpecAsync(snapshot: false);

        KickoffPrompt = _prompts.BuildKickoffPrompt(_task, SelectedRules());
        await PersistStepPromptAsync(StepKind.SpecAndKickoff, KickoffPrompt);
        await _dialogs.SetClipboardTextAsync(KickoffPrompt);
        Status = "Spec saved and kickoff prompt copied to clipboard";
    });

    /// <summary>Writes the current spec text to disk; optionally snapshots it into the version history.</summary>
    private async Task PersistSpecAsync(bool snapshot)
    {
        _fs.WriteText(_fs.Combine(_task.FolderPath, TaskLayout.SpecFile), SpecText);
        if (!snapshot)
            return;

        using (var scope = _services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<IVersioningService>()
                .SaveSnapshotAsync(_task, TaskLayout.SpecFile, label: null);
        await ReloadVersionsAsync();
    }

    [RelayCommand]
    private Task OptimizeSpecAsync() => RunAsync("Optimizing with local LLM…", async () =>
    {
        if (string.IsNullOrWhiteSpace(SpecText))
        {
            Status = "Nothing to optimize — generate or write the spec first.";
            return;
        }

        if (!await _llm.IsAvailableAsync())
        {
            await _dialogs.ShowMessageAsync("Local LLM unavailable",
                "Ollama isn't reachable. Start it and check the endpoint/model in Settings, then try again.");
            return;
        }

        var optimized = (await _llm.OptimizeAsync(SpecText)).Trim();
        if (string.IsNullOrWhiteSpace(optimized) || optimized == SpecText.Trim())
        {
            Status = "The local LLM suggested no changes.";
            return;
        }

        using var scope = _services.CreateScope();
        var diff = scope.ServiceProvider.GetRequiredService<IVersioningService>().Diff(SpecText, optimized);
        DiffText = string.Join(Environment.NewLine, diff.Lines.Select(FormatDiffLine));
        OptimizedSpec = optimized;
        Status = "Review the changes on the right, then Apply or Discard.";
    });

    [RelayCommand]
    private void ApplyOptimizedSpec()
    {
        if (OptimizedSpec is null)
            return;
        SpecText = OptimizedSpec;
        OptimizedSpec = null;
        DiffText = null;
        Status = "Optimized spec applied — hit Save to snapshot it.";
    }

    [RelayCommand]
    private void DiscardOptimizedSpec()
    {
        OptimizedSpec = null;
        DiffText = null;
        Status = "Discarded the optimization.";
    }

    [RelayCommand]
    private void ShowVersionDiff()
    {
        if (SelectedVersion is null)
        {
            DiffText = null;
            return;
        }

        using var scope = _services.CreateScope();
        var versioning = scope.ServiceProvider.GetRequiredService<IVersioningService>();
        var snapshot = versioning.ReadSnapshot(_task, ToDomainVersion(SelectedVersion));
        var diff = versioning.Diff(snapshot, SpecText);
        DiffText = string.Join(Environment.NewLine, diff.Lines.Select(FormatDiffLine));
    }

    [RelayCommand]
    private Task RestoreVersionAsync() => RunAsync("Restoring version…", async () =>
    {
        if (SelectedVersion is null)
            return;
        using var scope = _services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IVersioningService>()
            .RestoreAsync(_task, ToDomainVersion(SelectedVersion));
        SpecText = _fs.ReadText(_fs.Combine(_task.FolderPath, TaskLayout.SpecFile));
        await ReloadVersionsAsync();
    });

    // ---- Implement -------------------------------------------------------

    [RelayCommand]
    private Task SaveImplementationAsync() => RunAsync("Saving notes…", async () =>
    {
        _fs.WriteText(NotesPath(), ImplementationNotes);
        await MarkDoneAsync(StepKind.Implement);
    });

    [RelayCommand]
    private void ReloadImplementationProgress()
    {
        ImplementationProgress = _fs.ReadText(ProgressPath());
        Status = HasImplementationProgress
            ? "Reloaded the agent's progress file."
            : $"No {TaskLayout.ProgressFile} yet — the coding agent writes it as it works.";
    }

    // ---- Self review -----------------------------------------------------

    [RelayCommand]
    private Task GenerateSelfReviewAsync() => RunAsync("Building review prompt…", async () =>
    {
        SelfReviewPrompt = _prompts.BuildSelfReviewPrompt(_task);
        await PersistStepPromptAsync(StepKind.SelfReview, SelfReviewPrompt);
        await _dialogs.SetClipboardTextAsync(SelfReviewPrompt);
        Status = "Self-review prompt copied to clipboard";
    });

    // ---- Past PR comments ------------------------------------------------

    [RelayCommand]
    private Task FetchClosedPullRequestsAsync() => RunAsync("Fetching closed PRs…", async () =>
    {
        using var scope = _services.CreateScope();
        var prs = await scope.ServiceProvider.GetRequiredService<IGitHubClient>()
            .GetClosedPullRequestsAsync(RequireOwner(), RequireRepo(), max: 30);
        ClosedPullRequests.Clear();
        foreach (var p in prs)
            ClosedPullRequests.Add(new PullRequestItemViewModel
            {
                Number = p.Number, Title = p.Title, Author = p.Author, IsBot = p.IsBot,
            });
    });

    [RelayCommand]
    private Task GeneratePastCommentsAsync() => RunAsync("Fetching PR comments…", async () =>
    {
        if (SelectedPullRequest is null)
            return;
        using var scope = _services.CreateScope();
        var all = await scope.ServiceProvider.GetRequiredService<IGitHubClient>()
            .GetPullRequestCommentsAsync(RequireOwner(), RequireRepo(), SelectedPullRequest.Number);
        var human = all.Where(c => !c.IsBot).ToList();
        PastCommentsPrompt = _prompts.BuildPastCommentsPrompt(_task, human);
        await PersistStepPromptAsync(StepKind.PastPrComments, PastCommentsPrompt);
        await _dialogs.SetClipboardTextAsync(PastCommentsPrompt);
    });

    // ---- PR comments -----------------------------------------------------

    [RelayCommand]
    private Task FetchPrCommentsAsync() => RunAsync("Fetching PR comments…", async () =>
    {
        if (!int.TryParse(PrNumberText, out var number))
        {
            await _dialogs.ShowMessageAsync("PR number", "Enter a valid pull request number.");
            return;
        }

        using var scope = _services.CreateScope();
        var comments = await scope.ServiceProvider.GetRequiredService<IGitHubClient>()
            .GetPullRequestCommentsAsync(RequireOwner(), RequireRepo(), number);
        PrComments.Clear();
        foreach (var c in comments)
            PrComments.Add(new CommentItemViewModel { Author = c.Author, IsBot = c.IsBot, Body = c.Body, FilePath = c.FilePath });

        PrCommentsPrompt = _prompts.BuildPrCommentsPrompt(_task, comments);
        await PersistStepPromptAsync(StepKind.PrComments, PrCommentsPrompt);
        await _dialogs.SetClipboardTextAsync(PrCommentsPrompt);
    });

    // ---- Step status & version preview -----------------------------------

    [RelayCommand]
    private Task MarkSelectedDoneAsync() =>
        SelectedStep is null ? Task.CompletedTask : SetStepStatusAsync(SelectedStep, StepStatus.Done);

    [RelayCommand]
    private Task SkipSelectedAsync() =>
        SelectedStep is null ? Task.CompletedTask : SetStepStatusAsync(SelectedStep, StepStatus.Skipped);

    [RelayCommand]
    private Task ResetSelectedAsync() =>
        SelectedStep is null ? Task.CompletedTask : SetStepStatusAsync(SelectedStep, StepStatus.InProgress);

    [RelayCommand]
    private void ViewVersion()
    {
        if (SelectedVersion is null)
            return;
        using var scope = _services.CreateScope();
        var snapshot = scope.ServiceProvider.GetRequiredService<IVersioningService>()
            .ReadSnapshot(_task, ToDomainVersion(SelectedVersion));
        DiffText = $"# Saved version — {SelectedVersion.Display}{Environment.NewLine}{Environment.NewLine}{snapshot}";
    }

    // ---- Helpers ---------------------------------------------------------

    private JiraTicket? EffectiveTicket()
    {
        var hasContent = !string.IsNullOrWhiteSpace(TicketSummary) || !string.IsNullOrWhiteSpace(TicketDescription);
        return hasContent
            ? new JiraTicket(JiraKey, TicketSummary ?? string.Empty, TicketDescription ?? string.Empty,
                TicketType, TicketStatus, _ticket?.Labels ?? [],
                TicketAcceptanceCriteria, _ticket?.Comments ?? [], _ticket?.Attachments ?? [])
            : null;
    }

    private List<RulePreset> SelectedRules() =>
        [.. Rules.Where(r => r.IsChecked).Select(r => new RulePreset { Name = r.Name, Text = r.Text, DefaultSelected = true })];

    private string NotesPath() =>
        _fs.Combine(_task.FolderPath, Path.Combine(TaskLayout.ImplementationFolder, TaskLayout.NotesFile));

    private string ProgressPath() =>
        _fs.Combine(_task.FolderPath, Path.Combine(TaskLayout.ImplementationFolder, TaskLayout.ProgressFile));

    private string RequireOwner() =>
        !string.IsNullOrWhiteSpace(GitHubOwner) ? GitHubOwner! : throw new ValidationException("Set the GitHub owner.");

    private string RequireRepo() =>
        !string.IsNullOrWhiteSpace(GitHubRepo) ? GitHubRepo! : throw new ValidationException("Set the GitHub repo.");

    private async Task ReloadVersionsAsync()
    {
        using var scope = _services.CreateScope();
        var versions = await scope.ServiceProvider.GetRequiredService<IVersioningService>()
            .ListVersionsAsync(TaskId, TaskLayout.SpecFile);
        Versions.Clear();
        foreach (var v in versions)
            Versions.Add(new VersionItemViewModel
            {
                Id = v.Id, RelativePath = v.RelativePath, SnapshotRelativePath = v.SnapshotRelativePath,
                Label = v.Label, CreatedUtc = v.CreatedUtc,
            });
    }

    private static DocumentVersion ToDomainVersion(VersionItemViewModel v) => new()
    {
        Id = v.Id, WorkTaskId = default, RelativePath = v.RelativePath, SnapshotRelativePath = v.SnapshotRelativePath,
    };

    private static string FormatDiffLine(DiffLine line) => line.Kind switch
    {
        DiffChangeKind.Inserted => $"+ {line.Text}",
        DiffChangeKind.Deleted => $"- {line.Text}",
        DiffChangeKind.Modified => $"~ {line.Text}",
        _ => $"  {line.Text}",
    };

    private async Task MarkDoneAsync(StepKind kind)
    {
        var step = Steps.FirstOrDefault(s => s.Kind == kind);
        if (step is not null)
            await SetStepStatusAsync(step, StepStatus.Done);
    }

    private async Task SetStepStatusAsync(StepViewModel step, StepStatus status)
    {
        step.Status = status;
        using var scope = _services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ITaskService>().SetStepStatusAsync(step.Id, status);
    }

    private async Task PersistStepPromptAsync(StepKind kind, string prompt)
    {
        var step = Steps.FirstOrDefault(s => s.Kind == kind);
        if (step is null)
            return;
        using var scope = _services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ITaskService>().SaveStepAsync(step.Id, prompt, null);
        await SetStepStatusAsync(step, StepStatus.Done);
    }

    private async Task RunAsync(string busyMessage, Func<Task> action)
    {
        try
        {
            IsBusy = true;
            Status = busyMessage;
            await action();
            if (Status == busyMessage)
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
