using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class UpdateInstallProgressWindow : Window
{
    private CancellationTokenSource? _cancellation;
    private bool _started;
    private bool _operationFinished;

    public UpdateInstallProgressWindow()
    {
        AvaloniaXamlLoader.Load(this);
        AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        Opened += OnOpened;
    }

    public Func<IProgress<UpdateInstallProgress>, CancellationToken, Task<UpdateInstallResult>>? PrepareInstallAsync { get; set; }

    public UpdateInstallResult Result { get; private set; } =
        new(false, "Stahování aktualizace nebylo spuštěno.", null);

    private void OnOpened(object? sender, EventArgs e)
    {
        this.FindControl<Button>("CancelButton")?.Focus();
        if (_started)
        {
            return;
        }

        _started = true;
        Dispatcher.UIThread.Post(async () => await RunPrepareInstallAsync().ConfigureAwait(true), DispatcherPriority.Background);
    }

    private async Task RunPrepareInstallAsync()
    {
        if (DataContext is not UpdateInstallProgressDialogViewModel model || PrepareInstallAsync is null)
        {
            Close(Result);
            return;
        }

        _cancellation = new CancellationTokenSource();
        var progress = new Progress<UpdateInstallProgress>(model.ApplyProgress);
        try
        {
            Result = await PrepareInstallAsync(progress, _cancellation.Token).ConfigureAwait(true);
            _operationFinished = true;
            model.MarkCompleted(Result.Message);
            await Task.Delay(350).ConfigureAwait(true);
            Close(Result);
        }
        catch (OperationCanceledException)
        {
            Result = new UpdateInstallResult(false, "Stahování aktualizace bylo zrušeno.", null);
            _operationFinished = true;
            model.MarkCancelled();
            await Task.Delay(250).ConfigureAwait(true);
            Close(Result);
        }
        catch (Exception ex)
        {
            Result = new UpdateInstallResult(false, $"Aktualizaci se nepodařilo připravit: {ex.Message}", null);
            _operationFinished = true;
            model.MarkCompleted(Result.Message);
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (!_operationFinished && _cancellation is { IsCancellationRequested: false })
        {
            _cancellation.Cancel();
            if (DataContext is UpdateInstallProgressDialogViewModel model)
            {
                model.StatusMessage = "Ruším stahování aktualizace.";
                model.CanCancel = false;
            }

            return;
        }

        Close(Result);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
        {
            return;
        }

        if (e.Key != Key.Escape || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        e.Handled = true;
        OnCancelClick(sender, e);
    }
}
