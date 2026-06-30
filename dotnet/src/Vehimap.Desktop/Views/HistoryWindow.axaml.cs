using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.Views;

public partial class HistoryWindow : Window
{
    public HistoryWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "HistoryWorkspaceHost", DesktopLocalization.Localizer.GetString("WorkspaceWindow.CloseAction.History"));
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
