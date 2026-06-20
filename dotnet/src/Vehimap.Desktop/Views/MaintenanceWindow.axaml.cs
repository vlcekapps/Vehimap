using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class MaintenanceWindow : Window
{
    public MaintenanceWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "MaintenanceWorkspaceHost", "zavřít editor údržby");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
