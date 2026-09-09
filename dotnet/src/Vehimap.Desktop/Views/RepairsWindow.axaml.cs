// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class RepairsWindow : Window
{
    private bool _editorOpen;
    private readonly RepairsWindowViewModel _model = null!;
    public RepairsWindow() { AvaloniaXamlLoader.Load(this); }
    public RepairsWindow(RepairsWindowViewModel model) : this()
    {
        _model = model;
        DataContext = model;
        KeyboardAccessibilityHelper.RegisterWindow(this);
        model.CloseRequested += OnCloseRequested;
        Closed += (_, _) => model.CloseRequested -= OnCloseRequested;
        Closing += (_, e) => e.Cancel = _editorOpen;
        Opened += (_, _) => Dispatcher.UIThread.Post(FocusListOrNew, DispatcherPriority.Loaded);
    }
    private void OnCloseRequested() { if (!_editorOpen) Close(); }
    private void FocusListOrNew()
    {
        if (_model.Items.Count > 0 && this.FindControl<ListBox>("RepairsListBox") is { } list) WorkspaceFocusHelpers.FocusListBox(list);
        else this.FindControl<Button>("NewRepairButton")?.Focus();
    }
    private async void OnActionClick(object? sender, RoutedEventArgs e)
    {
        if (_editorOpen || sender is not Button button || !Enum.TryParse<RepairAction>(button.Tag?.ToString(), out var action)) return;
        var editor = _model.CreateEditor(action);
        if (editor is null) return;
        _editorOpen = true;
        try
        {
            var saved = await new RepairEditorWindow(editor).ShowDialog<bool>(this);
            _model.Refresh(editor.SavedRepairId);
            if (saved) _model.Status = DesktopLocalization.LiveLocalizer.GetString("Repairs.Saved");
        }
        finally
        {
            _editorOpen = false;
            Dispatcher.UIThread.Post(() => { if (button.IsEffectivelyEnabled) button.Focus(); else FocusListOrNew(); }, DispatcherPriority.Background);
        }
    }
    internal static async Task ShowAsync(Window owner, MainWindowViewModel root, string? repairId = null)
    {
        var model = root.BuildRepairsModel();
        if (model is null) return;
        model.Refresh(repairId);
        var previous = owner.FocusManager?.GetFocusedElement() as Control;
        if (previous is MenuItem or Menu) previous = null;
        root.IsRepairsWindowOpen = true;
        try { await new RepairsWindow(model).ShowDialog(owner); }
        finally
        {
            root.IsRepairsWindowOpen = false;
            Dispatcher.UIThread.Post(() =>
            {
                owner.Activate();
                if (previous is { IsEffectivelyVisible: true, IsEffectivelyEnabled: true } && previous.Focus()) return;
                root.RequestWorkspaceFocus(root.SelectedVehicleTabIndex switch
                {
                    DesktopTabIndexes.Audit => DesktopFocusTarget.AuditList,
                    DesktopTabIndexes.Dashboard => DesktopFocusTarget.DashboardAuditList,
                    _ => DesktopFocusTarget.VehicleList
                });
            }, DispatcherPriority.Background);
        }
    }
}
