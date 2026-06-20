using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class HistoryWindow : Window
{
    public HistoryWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "HistoryWorkspaceHost", "zavřít editor historie");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
