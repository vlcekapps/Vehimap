using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.Views;

public partial class RemindersWindow : Window
{
    public RemindersWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "ReminderWorkspaceHost", DesktopLocalization.Localizer.GetString("WorkspaceWindow.CloseAction.Reminder"));
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
