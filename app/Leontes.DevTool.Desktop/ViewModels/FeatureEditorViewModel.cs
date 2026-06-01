using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leontes.DevTool.Application.Services;
using Leontes.DevTool.Desktop.Services;
using Leontes.DevTool.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Desktop.ViewModels;

public sealed partial class FeatureEditorViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogs;
    private Feature _feature = null!;

    public Guid FeatureId { get; private set; }

    /// <summary>Set by the shell so the tree refreshes after a save.</summary>
    public Func<Task>? Saved { get; set; }

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private string? _featureContext;
    [ObservableProperty] private string _status = string.Empty;

    public FeatureEditorViewModel(IServiceProvider services, IDialogService dialogs)
    {
        _services = services;
        _dialogs = dialogs;
    }

    public async Task LoadAsync(Guid featureId)
    {
        FeatureId = featureId;
        using var scope = _services.CreateScope();
        _feature = await scope.ServiceProvider.GetRequiredService<IFeatureService>().GetAsync(featureId);
        Name = _feature.Name;
        Description = _feature.Description;
        FeatureContext = _feature.FeatureContext;
        Status = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            _feature.Name = Name;
            _feature.Description = Description;
            _feature.FeatureContext = FeatureContext;

            using var scope = _services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IFeatureService>().UpdateAsync(_feature);

            Status = "Saved";
            if (Saved is not null)
                await Saved();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowMessageAsync("Could not save feature", ex.Message);
        }
    }
}
