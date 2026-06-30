using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.Views;

public partial class RecordsWindow : Window
{
    public RecordsWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "RecordWorkspaceHost", DesktopLocalization.Localizer.GetString("WorkspaceWindow.CloseAction.Record"));
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
