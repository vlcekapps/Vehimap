using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class RecordsWindow : Window
{
    public RecordsWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "RecordWorkspaceHost", "zavřít editor dokladů");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
