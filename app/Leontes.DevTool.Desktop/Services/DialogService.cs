using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Leontes.DevTool.Desktop.ViewModels;
using Leontes.DevTool.Desktop.ViewModels.Dialogs;
using Leontes.DevTool.Desktop.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Leontes.DevTool.Desktop.Services;

public sealed class DialogService(IServiceProvider services) : IDialogService
{
    /// <summary>Set once the main window exists; all dialogs are owned by it.</summary>
    public Window? Owner { get; set; }

    public async Task<IReadOnlyDictionary<string, string>?> PromptFormAsync(string title, IReadOnlyList<DialogField> fields)
    {
        var vm = new InputDialogViewModel(title, null, fields, showCancel: true, primaryText: "OK");
        var dialog = new InputDialog { DataContext = vm };
        var ok = await ShowAsync(dialog);
        return ok ? vm.Result() : null;
    }

    public async Task<string?> PickFolderAsync(string title)
    {
        if (Owner is null)
            return null;

        var folders = await Owner.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = title, AllowMultiple = false });

        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        var vm = new InputDialogViewModel(title, message, [], showCancel: true, primaryText: "Yes");
        return ShowAsync(new InputDialog { DataContext = vm });
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        var vm = new InputDialogViewModel(title, message, [], showCancel: false, primaryText: "OK");
        await ShowAsync(new InputDialog { DataContext = vm });
    }

    public async Task ShowSettingsAsync()
    {
        if (Owner is null)
            return;

        var vm = services.GetRequiredService<SettingsViewModel>();
        var window = new SettingsWindow { DataContext = vm };
        await window.ShowDialog(Owner);
    }

    public async Task SetClipboardTextAsync(string text)
    {
        if (Owner?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(text);
    }

    private Task<bool> ShowAsync(Window dialog) =>
        Owner is null ? Task.FromResult(false) : dialog.ShowDialog<bool>(Owner);
}
