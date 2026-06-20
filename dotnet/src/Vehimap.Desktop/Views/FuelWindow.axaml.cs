using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class FuelWindow : Window
{
    public FuelWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "FuelWorkspaceHost", "zavřít editor tankování");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
