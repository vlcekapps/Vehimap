// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class MaintenanceWorkspaceView : WorkspaceViewBase<MaintenanceWorkspaceViewModel>
{
    private MaintenanceWorkspaceViewModel? _subscribedViewModel;

    public MaintenanceWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("MaintenanceListBox");
        DataContextChanged += OnMaintenanceDataContextChanged;
        ApplyHostMode();
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        DesktopFocusTarget.MaintenanceList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.MaintenanceSearch
            or DesktopFocusTarget.MaintenanceList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.MaintenanceSearch => this.FindControl<TextBox>("MaintenanceSearchBox"),
            DesktopFocusTarget.MaintenanceList => this.FindControl<ListBox>("MaintenanceListBox"),
            _ => null
        };

    protected override void OnAllowEditingChanged()
    {
        ApplyHostMode();
    }

    private void ApplyHostMode()
    {
        if (this.FindControl<Control>("MaintenanceActionPanel") is { } actionPanel)
        {
            actionPanel.IsVisible = AllowEditing;
        }
    }

    private void OnMaintenanceDataContextChanged(object? sender, EventArgs e)
    {
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.MaintenanceTemplatesRequested -= OnMaintenanceTemplatesRequested;
            _subscribedViewModel.MaintenanceCompletionRequested -= OnMaintenanceCompletionRequested;
        }

        _subscribedViewModel = DataContext as MaintenanceWorkspaceViewModel;
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.MaintenanceTemplatesRequested += OnMaintenanceTemplatesRequested;
            _subscribedViewModel.MaintenanceCompletionRequested += OnMaintenanceCompletionRequested;
        }
    }

    private async void OnMaintenanceTemplatesRequested(object? sender, EventArgs e)
    {
        await OpenMaintenanceTemplatesDialogAsync();
    }

    private async Task OpenMaintenanceTemplatesDialogAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        var preview = ViewModel.BuildMaintenanceTemplatePreview();
        if (preview.TotalMissingCount == 0)
        {
            ViewModel.SetMaintenanceTemplateStatus(
                DesktopLocalization.Localizer.GetString("MaintenanceWorkspace.Status.NoMissingTemplates"));
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            return;
        }

        var dialog = new VehicleStarterBundleWindow
        {
            DataContext = VehicleStarterBundleDialogViewModel.CreateMaintenanceTemplates(preview)
        };

        var result = await dialog.ShowDialog<VehicleStarterBundleDialogResult?>(owner);
        if (result is null)
        {
            return;
        }

        var message = await ViewModel.ApplyMaintenanceTemplatesAsync(result.SelectedItems);
        ViewModel.SetMaintenanceTemplateStatus(message);
    }

    private async void OnMaintenanceCompletionRequested(object? sender, EventArgs e)
    {
        await OpenMaintenanceCompletionDialogAsync();
    }

    private async Task OpenMaintenanceCompletionDialogAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialogViewModel = ViewModel.BuildMaintenanceCompletionDialogViewModel();
        if (dialogViewModel is null)
        {
            ViewModel.SetMaintenanceStatus(
                DesktopLocalization.Localizer.GetString("MaintenanceWorkspace.Status.SelectMaintenancePlan"));
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            return;
        }

        var dialog = new MaintenanceCompletionWindow
        {
            DataContext = dialogViewModel
        };

        var result = await dialog.ShowDialog<MaintenanceCompletionDialogResult?>(owner);
        if (result is null)
        {
            return;
        }

        var message = await ViewModel.ApplyMaintenanceCompletionAsync(result);
        ViewModel.SetMaintenanceStatus(message);
    }
}
