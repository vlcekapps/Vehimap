using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.Views;

public partial class FuelWindow : Window
{
    public FuelWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "FuelWorkspaceHost", DesktopLocalization.Localizer.GetString("WorkspaceWindow.CloseAction.Fuel"));
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
