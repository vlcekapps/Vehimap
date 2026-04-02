using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class VehicleStarterBundleWindow : Window
{
    public VehicleStarterBundleWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<ListBox>("BundleItemsListBox")?.Focus());
    }

    private void OnApplyClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not VehicleStarterBundleDialogViewModel viewModel)
        {
            Close(null);
            return;
        }

        var selectedItems = viewModel.BuildSelectedTemplates();
        if (selectedItems.Count == 0)
        {
            return;
        }

        Close(new VehicleStarterBundleDialogResult(selectedItems));
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(null);
}
