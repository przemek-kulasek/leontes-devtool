using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Leontes.DevTool.Desktop.Services;

namespace Leontes.DevTool.Desktop.ViewModels.Dialogs;

public sealed partial class InputFieldViewModel(DialogField field) : ObservableObject
{
    public string Label { get; } = field.Label;
    public bool Multiline { get; } = field.Multiline;

    [ObservableProperty]
    private string _value = field.Initial;
}

public sealed partial class InputDialogViewModel : ObservableObject
{
    public string Title { get; }
    public string? Message { get; }
    public bool ShowFields => Fields.Count > 0;
    public bool ShowCancel { get; }
    public string PrimaryText { get; }

    public ObservableCollection<InputFieldViewModel> Fields { get; }

    public InputDialogViewModel(string title, string? message, IReadOnlyList<DialogField> fields, bool showCancel, string primaryText)
    {
        Title = title;
        Message = message;
        ShowCancel = showCancel;
        PrimaryText = primaryText;
        Fields = [.. CreateFields(fields)];
    }

    public IReadOnlyDictionary<string, string> Result()
    {
        var result = new Dictionary<string, string>();
        foreach (var field in Fields)
            result[field.Label] = field.Value;
        return result;
    }

    private static IEnumerable<InputFieldViewModel> CreateFields(IReadOnlyList<DialogField> fields)
    {
        foreach (var field in fields)
            yield return new InputFieldViewModel(field);
    }
}
