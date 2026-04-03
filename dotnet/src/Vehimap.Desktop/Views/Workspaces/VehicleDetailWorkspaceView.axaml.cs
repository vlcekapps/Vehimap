using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class VehicleDetailWorkspaceView : WorkspaceViewBase<VehicleDetailWorkspaceViewModel>
{
    public VehicleDetailWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation(DesktopFocusTarget.VehicleEditorCancel, "VehicleEditorNameBox");
        ApplyHostMode();
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget()
    {
        if (ViewModel?.IsEditingVehicle == true)
        {
            return DesktopFocusTarget.VehicleEditorName;
        }

        return AllowEditing ? DesktopFocusTarget.VehicleDetailPrimaryAction : null;
    }

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.VehicleDetailPrimaryAction or DesktopFocusTarget.VehicleEditorName or DesktopFocusTarget.VehicleEditorCancel;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.VehicleDetailPrimaryAction => ResolvePrimaryAction(),
            DesktopFocusTarget.VehicleEditorName => this.FindControl<TextBox>("VehicleEditorNameBox"),
            DesktopFocusTarget.VehicleEditorCancel => this.FindControl<Button>("CancelVehicleButton"),
            _ => null
        };

    protected override void OnAllowEditingChanged()
    {
        ApplyHostMode();
    }

    private async void OnSaveVehicleClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        await ViewModel.SaveVehicleCommand.ExecuteAsync(null);
        if (ViewModel.TryConsumePendingVehicleStarterBundleOffer())
        {
            await OpenVehicleStarterBundleDialogAsync(postCreateOffer: true);
        }
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
            ViewModel.SetVehicleStarterBundleStatus(postCreateOffer
                ? "Nové vozidlo bylo uloženo. Balíček pro vozidlo už neměl žádné nové položky."
                : "Balíček pro vozidlo už nemá žádné chybějící položky.");
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            return;
        }

        var dialog = new VehicleStarterBundleWindow
        {
            DataContext = new VehicleStarterBundleDialogViewModel(preview)
        };

        var result = await dialog.ShowDialog<VehicleStarterBundleDialogResult?>(owner);
        if (result is null)
        {
            return;
        }

        var message = await ViewModel.ApplyVehicleStarterBundleAsync(result.SelectedItems);
        ViewModel.SetVehicleStarterBundleStatus(message);
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

    private void ApplyHostMode()
    {
        if (this.FindControl<Control>("VehicleActionPanel") is { } actionPanel)
        {
            actionPanel.IsVisible = AllowEditing;
        }

        if (this.FindControl<Control>("VehicleEditorHost") is { } editorHost)
        {
            editorHost.IsVisible = AllowEditing;
        }
    }
}
