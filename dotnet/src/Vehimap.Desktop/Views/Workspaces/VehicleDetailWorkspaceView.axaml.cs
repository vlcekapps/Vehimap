using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class VehicleDetailWorkspaceView : WorkspaceViewBase<VehicleDetailWorkspaceViewModel>
{
    public VehicleDetailWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        ApplyHostMode();
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget()
    {
        return AllowEditing ? DesktopFocusTarget.VehicleDetailPrimaryAction : null;
    }

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.VehicleDetailPrimaryAction;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.VehicleDetailPrimaryAction => ResolvePrimaryAction(),
            _ => null
        };

    protected override void OnAllowEditingChanged()
    {
        ApplyHostMode();
    }

    private async void OnOpenVehicleStarterBundleClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await OpenVehicleStarterBundleDialogAsync(postCreateOffer: false);
    }

    private async Task OpenVehicleStarterBundleDialogAsync(bool postCreateOffer)
    {
        if (ViewModel is null)
        {
            return;
        }

        var preview = ViewModel.BuildVehicleStarterBundlePreview();
        if (preview.TotalMissingCount == 0)
        {
            ViewModel.SetVehicleStarterBundleStatus(
                DesktopLocalization.Localizer.GetString(postCreateOffer
                    ? "VehicleDetail.Status.NewVehicleBundleNoItems"
                    : "VehicleDetail.Status.BundleNoMissingItems"));
            FocusPrimaryActionAfterLayout();
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            FocusPrimaryActionAfterLayout();
            return;
        }

        var dialog = new VehicleStarterBundleWindow
        {
            DataContext = new VehicleStarterBundleDialogViewModel(preview)
        };

        var result = await dialog.ShowDialog<VehicleStarterBundleDialogResult?>(owner);
        if (result is null)
        {
            FocusPrimaryActionAfterLayout();
            return;
        }

        var message = await ViewModel.ApplyVehicleStarterBundleAsync(result.SelectedItems);
        ViewModel.SetVehicleStarterBundleStatus(message);
        FocusPrimaryActionAfterLayout();
    }

    private void OnOpenVehicleHistoryWorkspaceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseVehicleDetailWindowIfNeeded(ViewModel?.OpenVehicleHistoryWorkspace() == true);
    }

    private void OnOpenVehicleFuelWorkspaceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseVehicleDetailWindowIfNeeded(ViewModel?.OpenVehicleFuelWorkspace() == true);
    }

    private void OnOpenVehicleReminderWorkspaceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseVehicleDetailWindowIfNeeded(ViewModel?.OpenVehicleReminderWorkspace() == true);
    }

    private void OnOpenVehicleMaintenanceWorkspaceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseVehicleDetailWindowIfNeeded(ViewModel?.OpenVehicleMaintenanceWorkspace() == true);
    }

    private void OnOpenVehicleRecordWorkspaceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseVehicleDetailWindowIfNeeded(ViewModel?.OpenVehicleRecordWorkspace() == true);
    }

    private void OnOpenVehicleTimelineWorkspaceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseVehicleDetailWindowIfNeeded(ViewModel?.OpenVehicleTimelineWorkspace() == true);
    }

    private async void OnOpenVehicleServiceBookClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel is null || TopLevel.GetTopLevel(this) is not Window owner)
        {
            return;
        }

        var model = ViewModel.BuildVehicleServiceBookModel();
        if (model is null)
        {
            return;
        }

        var dialog = new ServiceBookWindow
        {
            DataContext = model
        };

        await dialog.ShowDialog(owner);
        CloseVehicleDetailWindowIfNeeded(model.DidOpenSelectedItem);
    }

    private async void OnOpenVehicleCostsWorkspaceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        CloseVehicleDetailWindowIfNeeded(await ViewModel.OpenVehicleCostsWorkspaceAsync());
    }

    private Control? ResolvePrimaryAction()
    {
        if (ViewModel?.IsVehicleDetailVisible == true
            && this.FindControl<Button>("EditVehicleButton") is { } editButton
            && editButton.IsVisible)
        {
            return editButton;
        }

        return this.FindControl<Button>("CreateVehicleButton");
    }

    private void FocusPrimaryActionAfterLayout()
    {
        Dispatcher.UIThread.Post(
            () => DispatcherTimer.RunOnce(
                () => ResolvePrimaryAction()?.Focus(NavigationMethod.Unspecified, KeyModifiers.None),
                TimeSpan.FromMilliseconds(80)),
            DispatcherPriority.Loaded);
    }

    private void CloseVehicleDetailWindowIfNeeded(bool navigationSucceeded)
    {
        if (!navigationSucceeded)
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is VehicleDetailWindow detailWindow)
        {
            detailWindow.Close();
        }
    }

    private void ApplyHostMode()
    {
        if (this.FindControl<Control>("VehicleActionPanel") is { } actionPanel)
        {
            actionPanel.IsVisible = AllowEditing;
        }
    }
}
