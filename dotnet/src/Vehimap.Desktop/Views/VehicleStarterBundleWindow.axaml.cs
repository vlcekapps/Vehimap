using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class VehicleStarterBundleWindow : Window
{
    public VehicleStarterBundleWindow()
    {
        AvaloniaXamlLoader.Load(this);
        AddHandler(InputElement.KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
        this.FindControl<ListBox>("BundleItemsListBox")?.AddHandler(InputElement.KeyDownEvent, OnBundleItemsListKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<ListBox>("BundleItemsListBox")?.Focus());
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            Close(null);
            return;
        }

        if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            ApplySelectedItems();
            return;
        }

        if (e.Key == Key.A && e.KeyModifiers is KeyModifiers.Control or (KeyModifiers.Control | KeyModifiers.Shift))
        {
            if (e.Source is TextBox)
            {
                return;
            }

            if (DataContext is not VehicleStarterBundleDialogViewModel viewModel)
            {
                return;
            }

            e.Handled = true;
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                viewModel.ClearSelectionCommand.Execute(null);
            }
            else
            {
                viewModel.SelectAllCommand.Execute(null);
            }
        }
    }

    private static void OnBundleItemsListKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space
            || e.KeyModifiers != KeyModifiers.None
            || e.Source is CheckBox
            || sender is not ListBox { SelectedItem: VehicleStarterBundleItemEditorViewModel item })
        {
            return;
        }

        item.IsSelected = !item.IsSelected;
        e.Handled = true;
    }

    private void OnApplyClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ApplySelectedItems();
    }

    private void ApplySelectedItems()
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
