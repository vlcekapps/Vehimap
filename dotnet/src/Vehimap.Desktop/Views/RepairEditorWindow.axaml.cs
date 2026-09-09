// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class RepairEditorWindow : Window
{
    private readonly EditorDialogLifecycle<RepairEditorViewModel> _lifecycle;
    public RepairEditorWindow() : this(null) { }
    public RepairEditorWindow(RepairEditorViewModel? model)
    {
        AvaloniaXamlLoader.Load(this);
        _lifecycle = EditorDialogFocusHelpers.CreateLifecycle<RepairEditorViewModel>(this, model?.FirstControlName ?? "RepairTitleBox", "CancelRepairButton",
            vm => vm.SaveCommand, vm => vm.CancelCommand, vm => vm.IsEditing,
            (vm, handler) => vm.FocusRequested += handler, (vm, handler) => vm.FocusRequested -= handler,
            target => target switch
            {
                DesktopFocusTarget.RepairTitle => "RepairTitleBox",
                DesktopFocusTarget.RepairReportedDate => "RepairReportedDateBox",
                DesktopFocusTarget.RepairPlannedDate => "RepairPlannedDateBox",
                DesktopFocusTarget.RepairReminderDays => "RepairReminderDaysBox",
                DesktopFocusTarget.RepairCompletedDate => "RepairCompletedDateBox",
                DesktopFocusTarget.RepairOdometer => "RepairOdometerBox",
                DesktopFocusTarget.RepairCost => "RepairCostBox",
                DesktopFocusTarget.RepairReason => "RepairReasonBox",
                _ => null
            });
        DataContext = model;
    }
    private async void OnSaveClick(object? sender, RoutedEventArgs e) => await _lifecycle.SaveAndCloseIfValidAsync();
    private void OnCancelClick(object? sender, RoutedEventArgs e) => _lifecycle.CancelAndClose();
}
