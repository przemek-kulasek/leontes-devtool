using Avalonia.Controls;
using Avalonia.Interactivity;
using Leontes.DevTool.Desktop.ViewModels;

namespace Leontes.DevTool.Desktop.Views.Dialogs;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        (DataContext as SettingsViewModel)?.Save();
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
