using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class RemindersWindow : Window
{
    public RemindersWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "ReminderWorkspaceHost", "zavřít editor připomínek");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
