using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Leontes.DevTool.Desktop.Views.Dialogs;

public partial class InputDialog : Window
{
    public InputDialog()
    {
        InitializeComponent();
    }

    private void OnPrimary(object? sender, RoutedEventArgs e) => Close(true);

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
