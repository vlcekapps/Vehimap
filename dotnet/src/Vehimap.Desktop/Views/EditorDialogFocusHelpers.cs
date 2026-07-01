// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

internal static class EditorDialogFocusHelpers
{
    public static EditorDialogLifecycle<TViewModel> CreateLifecycle<TViewModel>(
        Window window,
        string firstControlName,
        string cancelButtonName,
        Func<TViewModel, ICommand> saveCommand,
        Func<TViewModel, ICommand> cancelCommand,
        Func<TViewModel, bool> isEditing,
        Action<TViewModel, Action<DesktopFocusTarget>> subscribeFocus,
        Action<TViewModel, Action<DesktopFocusTarget>> unsubscribeFocus,
        Func<DesktopFocusTarget, string?> getFocusControlName)
        where TViewModel : class =>
        new(
            window,
            firstControlName,
            cancelButtonName,
            saveCommand,
            cancelCommand,
            isEditing,
            subscribeFocus,
            unsubscribeFocus,
            getFocusControlName);

    public static void RegisterEditorDialog(
        Window window,
        string firstControlName,
        string cancelButtonName,
        Action cancelAction,
        Func<Task>? saveAction = null)
    {
        KeyboardAccessibilityHelper.RegisterWindow(window);
        window.Opened += (_, _) => FocusControl(window, firstControlName);

        window.AddHandler(
            InputElement.KeyDownEvent,
            (_, e) =>
            {
                if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
                {
                    return;
                }

                if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
                {
                    e.Handled = true;
                    cancelAction();
                    return;
                }

                if (saveAction is not null && e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
                {
                    e.Handled = true;
                    _ = saveAction();
                }
            },
            RoutingStrategies.Tunnel);

        if (window.FindControl<Control>(firstControlName) is { } firstControl)
        {
            firstControl.AddHandler(
                InputElement.KeyDownEvent,
                (_, e) =>
                {
                    if (e.Key != Key.Tab || !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        return;
                    }

                    FocusControl(window, cancelButtonName);
                    e.Handled = true;
                },
                RoutingStrategies.Tunnel);
        }
    }

    public static void FocusControl(Window window, string controlName)
    {
        Dispatcher.UIThread.Post(
            () => window.FindControl<Control>(controlName)?.Focus(NavigationMethod.Unspecified, KeyModifiers.None),
            DispatcherPriority.Background);
    }

    public static async Task ExecuteCommandAsync(ICommand command)
    {
        if (command is IAsyncRelayCommand asyncCommand)
        {
            await asyncCommand.ExecuteAsync(null).ConfigureAwait(true);
            return;
        }

        command.Execute(null);
    }
}

internal sealed class EditorDialogLifecycle<TViewModel>
    where TViewModel : class
{
    private readonly Window _window;
    private readonly Func<TViewModel, ICommand> _saveCommand;
    private readonly Func<TViewModel, ICommand> _cancelCommand;
    private readonly Func<TViewModel, bool> _isEditing;
    private readonly Action<TViewModel, Action<DesktopFocusTarget>> _subscribeFocus;
    private readonly Action<TViewModel, Action<DesktopFocusTarget>> _unsubscribeFocus;
    private readonly Func<DesktopFocusTarget, string?> _getFocusControlName;
    private TViewModel? _viewModel;
    private bool _closeRequested;
    private bool _saveInProgress;

    public EditorDialogLifecycle(
        Window window,
        string firstControlName,
        string cancelButtonName,
        Func<TViewModel, ICommand> saveCommand,
        Func<TViewModel, ICommand> cancelCommand,
        Func<TViewModel, bool> isEditing,
        Action<TViewModel, Action<DesktopFocusTarget>> subscribeFocus,
        Action<TViewModel, Action<DesktopFocusTarget>> unsubscribeFocus,
        Func<DesktopFocusTarget, string?> getFocusControlName)
    {
        _window = window;
        _saveCommand = saveCommand;
        _cancelCommand = cancelCommand;
        _isEditing = isEditing;
        _subscribeFocus = subscribeFocus;
        _unsubscribeFocus = unsubscribeFocus;
        _getFocusControlName = getFocusControlName;

        _window.DataContextChanged += OnDataContextChanged;
        _window.Closing += OnClosing;
        _window.Closed += OnClosed;

        EditorDialogFocusHelpers.RegisterEditorDialog(
            _window,
            firstControlName,
            cancelButtonName,
            CancelAndClose,
            SaveAndCloseIfValidAsync);
    }

    public async Task SaveAndCloseIfValidAsync()
    {
        if (_saveInProgress || _closeRequested)
        {
            return;
        }

        if (_viewModel is not { } viewModel)
        {
            Close(false);
            return;
        }

        var command = _saveCommand(viewModel);
        if (!command.CanExecute(null))
        {
            return;
        }

        _saveInProgress = true;
        try
        {
            await EditorDialogFocusHelpers.ExecuteCommandAsync(command).ConfigureAwait(true);
        }
        finally
        {
            _saveInProgress = false;
        }

        if (_isEditing(viewModel))
        {
            return;
        }

        Close(true);
    }

    public void CancelAndClose()
    {
        if (_closeRequested)
        {
            return;
        }

        if (_viewModel is { } viewModel)
        {
            ExecuteCancel(viewModel);
        }

        Close(false);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        UnsubscribeCurrentViewModel();

        _viewModel = _window.DataContext as TViewModel;
        if (_viewModel is not null)
        {
            _subscribeFocus(_viewModel, OnFocusRequested);
        }
    }

    private void OnFocusRequested(DesktopFocusTarget target)
    {
        var controlName = _getFocusControlName(target);
        if (!string.IsNullOrWhiteSpace(controlName))
        {
            EditorDialogFocusHelpers.FocusControl(_window, controlName);
        }
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!_closeRequested && _viewModel is { } viewModel && _isEditing(viewModel))
        {
            ExecuteCancel(viewModel);
        }

        _closeRequested = true;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        UnsubscribeCurrentViewModel();
    }

    private void ExecuteCancel(TViewModel viewModel)
    {
        var command = _cancelCommand(viewModel);
        if (command.CanExecute(null))
        {
            command.Execute(null);
        }
    }

    private void Close(bool result)
    {
        _closeRequested = true;
        _window.Close(result);
    }

    private void UnsubscribeCurrentViewModel()
    {
        if (_viewModel is not null)
        {
            _unsubscribeFocus(_viewModel, OnFocusRequested);
            _viewModel = null;
        }
    }
}
