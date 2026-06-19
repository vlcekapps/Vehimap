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
        RegisterShiftTabBackNavigation("MaintenanceListBox", "MaintenanceEditorTitleBox");
        DataContextChanged += OnMaintenanceDataContextChanged;
        ApplyHostMode();
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingMaintenance == true ? DesktopFocusTarget.MaintenanceEditorTitle : DesktopFocusTarget.MaintenanceList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.MaintenanceList or DesktopFocusTarget.MaintenanceEditorTitle;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.MaintenanceList => this.FindControl<ListBox>("MaintenanceListBox"),
            DesktopFocusTarget.MaintenanceEditorTitle => this.FindControl<TextBox>("MaintenanceEditorTitleBox"),
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
        }

        _subscribedViewModel = DataContext as MaintenanceWorkspaceViewModel;
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.MaintenanceTemplatesRequested += OnMaintenanceTemplatesRequested;
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
}
