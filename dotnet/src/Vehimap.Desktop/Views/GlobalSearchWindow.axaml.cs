using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class GlobalSearchWindow : Window
{
    public GlobalSearchWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "GlobalSearchWorkspaceHost");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
