using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leontes.DevTool.Desktop.Services;

/// <summary>Owner-bound dialogs (prompts, folder picking, settings, messages) for view-models to call.</summary>
public interface IDialogService
{
    /// <summary>Shows a form of labeled text fields; returns the filled values keyed by label, or null if cancelled.</summary>
    Task<IReadOnlyDictionary<string, string>?> PromptFormAsync(string title, IReadOnlyList<DialogField> fields);

    /// <summary>Opens a folder picker; returns the chosen absolute path or null.</summary>
    Task<string?> PickFolderAsync(string title);

    Task<bool> ConfirmAsync(string title, string message);

    Task ShowMessageAsync(string title, string message);

    Task ShowSettingsAsync();

    Task SetClipboardTextAsync(string text);
}

/// <summary>A single editable field in a prompt form.</summary>
public sealed record DialogField(string Label, string Initial = "", bool Multiline = false);
