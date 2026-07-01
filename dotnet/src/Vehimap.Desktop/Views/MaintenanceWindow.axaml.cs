// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.Views;

public partial class MaintenanceWindow : Window
{
    public MaintenanceWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "MaintenanceWorkspaceHost", DesktopLocalization.Localizer.GetString("WorkspaceWindow.CloseAction.Maintenance"));
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
