using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class RemindersWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public RemindersWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += OnOpened;
        Closed += OnClosed;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_viewModel?.IsEditingReminder == true)
            {
                RequestFocus(DesktopFocusTarget.ReminderEditorTitle);
                return;
            }

            RequestFocus(DesktopFocusTarget.ReminderList);
        }, DispatcherPriority.Loaded);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested -= OnFocusRequested;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested -= OnFocusRequested;
        }

        _viewModel = DataContext as MainWindowViewModel;
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested += OnFocusRequested;
        }
    }

    private void OnFocusRequested(DesktopFocusTarget target)
    {
        switch (target)
        {
            case DesktopFocusTarget.ReminderList:
            case DesktopFocusTarget.ReminderEditorTitle:
                RequestFocus(target);
                break;
        }
    }

    private void RequestFocus(DesktopFocusTarget target)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ResolveFocusTarget(target) is not { } control)
            {
                return;
            }

            if (control is ListBox listBox)
            {
                FocusListBox(listBox);
                return;
            }

            control.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }, DispatcherPriority.Background);
    }

    private static bool FocusListBox(ListBox listBox)
    {
        if (listBox.ItemCount <= 0)
        {
            return listBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }

        if (listBox.SelectedItem is null && listBox.ItemCount > 0)
        {
            listBox.SelectedIndex = 0;
        }

        if (listBox.SelectedItem is not null)
        {
            listBox.ScrollIntoView(listBox.SelectedItem);
            if (listBox.ContainerFromItem(listBox.SelectedItem) is Control itemContainer)
            {
                return itemContainer.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            }
        }

        return listBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
    }

    private Control? ResolveFocusTarget(DesktopFocusTarget target)
    {
        return target switch
        {
            DesktopFocusTarget.ReminderList => this.FindControl<ListBox>("ReminderWindowListBox"),
            DesktopFocusTarget.ReminderEditorTitle => this.FindControl<TextBox>("ReminderWindowTitleBox"),
            _ => null
        };
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
