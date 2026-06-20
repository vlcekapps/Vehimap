using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
        RegisterShiftTabBackNavigation("MaintenanceListBox", "MaintenanceTemplateComboBox");
        DataContextChanged += OnMaintenanceDataContextChanged;
        ApplyHostMode();
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingMaintenance == true ? DesktopFocusTarget.MaintenanceEditorTemplate : DesktopFocusTarget.MaintenanceList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.MaintenanceSearch
            or DesktopFocusTarget.MaintenanceList
            or DesktopFocusTarget.MaintenanceEditorTemplate
            or DesktopFocusTarget.MaintenanceEditorTitle
            or DesktopFocusTarget.MaintenanceEditorIntervalKm
            or DesktopFocusTarget.MaintenanceEditorIntervalMonths
            or DesktopFocusTarget.MaintenanceEditorLastServiceDate
            or DesktopFocusTarget.MaintenanceEditorLastServiceOdometer;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.MaintenanceSearch => this.FindControl<TextBox>("MaintenanceSearchBox"),
            DesktopFocusTarget.MaintenanceList => this.FindControl<ListBox>("MaintenanceListBox"),
            DesktopFocusTarget.MaintenanceEditorTemplate => this.FindControl<ComboBox>("MaintenanceTemplateComboBox"),
            DesktopFocusTarget.MaintenanceEditorTitle => this.FindControl<TextBox>("MaintenanceEditorTitleBox"),
            DesktopFocusTarget.MaintenanceEditorIntervalKm => this.FindControl<TextBox>("MaintenanceEditorIntervalKmBox"),
            DesktopFocusTarget.MaintenanceEditorIntervalMonths => this.FindControl<TextBox>("MaintenanceEditorIntervalMonthsBox"),
            DesktopFocusTarget.MaintenanceEditorLastServiceDate => this.FindControl<TextBox>("MaintenanceEditorLastServiceDateBox"),
            DesktopFocusTarget.MaintenanceEditorLastServiceOdometer => this.FindControl<TextBox>("MaintenanceEditorLastServiceOdometerBox"),
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

        if (this.FindControl<Control>("MaintenanceEditorHost") is { } editorHost)
        {
            editorHost.IsVisible = AllowEditing;
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
            ViewModel.SetMaintenanceTemplateStatus("Doporučené šablony už nemají žádné chybějící servisní plány.");
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
            ViewModel.SetMaintenanceStatus("Nejprve vyberte servisní plán.");
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
