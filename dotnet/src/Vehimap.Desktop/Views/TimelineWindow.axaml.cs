using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Views;

public partial class TimelineWindow : Window
{
    public TimelineWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ModalWorkspaceWindowHelpers.RegisterWorkspaceLifecycle(this, "TimelineWorkspaceHost");
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
