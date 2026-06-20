using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class OverdueOverviewWindow : Window
{
    public OverdueOverviewWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "OverdueOverviewWorkspaceHost");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
