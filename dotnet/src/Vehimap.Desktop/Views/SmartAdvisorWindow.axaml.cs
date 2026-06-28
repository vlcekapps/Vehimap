using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class SmartAdvisorWindow : Window
{
    public SmartAdvisorWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "SmartAdvisorWorkspaceHost");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
