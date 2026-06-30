using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class MainWindowViewModelVehicleListAndQuickActionsTests
{
    [Fact]
    public void Projection_service_filters_vehicle_list_by_category_search_and_inactive_state()
    {
        var projectionService = new DesktopProjectionService();
        var timelineService = new LegacyTimelineService();
        var dataSet = new VehimapDataSet
        {
            Settings = BuildSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "05/2099", "01/2099", "03/2099"),
                new Vehicle("veh_2", "Drobeček", "Autobusy", "Meziměsto", "Karosa C734.20", "4C3 3359", "1992", "154", "", "10/2099", "03/2099", "04/2099"),
                new Vehicle("veh_3", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "", "1973", "30", "", "09/2099", "", "")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Běžný provoz", "rodina", "Benzín", "Má klimatizaci", "Řemen", "Manuální"),
                new VehicleMeta("veh_2", "Odstaveno", "autobus", "Nafta", "", "", ""),
                new VehicleMeta("veh_3", "Veterán", "veterán", "Benzín", "", "", "")
            ]
        };

        var projection = projectionService.BuildVehicleList(
            dataSet,
            dataSet.VehicleMetaEntries.ToDictionary(item => item.VehicleId, StringComparer.Ordinal),
            [],
            timelineService,
            new DesktopVehicleListFilters(
                "Milena",
                "Osobní vozidla",
                MainWindowViewModel.AllVehicleStatusFilterLabel,
                true),
            new DateOnly(2026, 4, 3));

        var item = Assert.Single(projection.Items);
        Assert.Equal("veh_1", item.Id);
        Assert.Contains("zobrazeno 1 z 3", projection.Summary, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public void Vehicle_detail_workspace_shows_recent_history_and_evidence_summary()
    {
        var dataSet = BuildQuickActionDataSet();
        dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_1", "veh_2", "01.05.2026", "Starší servis", "12000", "1500", "Olej"));
        dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_2", "veh_2", "10.05.2026", "Novější servis", "12500", "2200", "Brzdy"));
        dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_3", "veh_1", "15.05.2026", "Cizí servis", "8000", "500", "Jiné vozidlo"));
        dataSet.FuelEntries.Add(new FuelEntry("fuel_1", "veh_2", "12.05.2026", "12650", "35", "1200", true, "Benzín", "Plná"));

        var viewModel = CreateViewModel(dataSet);
        viewModel.SelectedVehicle = Assert.Single(viewModel.Vehicles, item => item.Id == "veh_2");

        Assert.Contains("Poslední tachometr: 12650 km", viewModel.VehicleDetailWorkspace.SelectedVehicleOverview, StringComparison.CurrentCulture);
        Assert.Contains("Štítky: veterán, srazy", viewModel.VehicleDetailWorkspace.SelectedVehicleOverview, StringComparison.CurrentCulture);
        Assert.Contains("historie 2", viewModel.VehicleDetailWorkspace.SelectedVehicleEvidenceSummary, StringComparison.CurrentCulture);
        Assert.Contains("tankování 1", viewModel.VehicleDetailWorkspace.SelectedVehicleEvidenceSummary, StringComparison.CurrentCulture);
        Assert.Contains("doklady 1", viewModel.VehicleDetailWorkspace.SelectedVehicleEvidenceSummary, StringComparison.CurrentCulture);
        Assert.Contains("připomínky 1", viewModel.VehicleDetailWorkspace.SelectedVehicleEvidenceSummary, StringComparison.CurrentCulture);
        Assert.Contains("servisní plány 1", viewModel.VehicleDetailWorkspace.SelectedVehicleEvidenceSummary, StringComparison.CurrentCulture);
        Assert.Contains("zobrazeno 2", viewModel.VehicleDetailWorkspace.SelectedVehicleRecentHistorySummary, StringComparison.CurrentCulture);
        Assert.Equal(2, viewModel.VehicleDetailWorkspace.RecentHistoryItems.Count);
        Assert.Equal("Novější servis", viewModel.VehicleDetailWorkspace.RecentHistoryItems[0].EventType);
        Assert.DoesNotContain(viewModel.VehicleDetailWorkspace.RecentHistoryItems, item => item.Id == "hist_3");

        Assert.Equal(5, viewModel.VehicleDetailWorkspace.EvidenceSummaryItems.Count);
        Assert.Contains(
            viewModel.VehicleDetailWorkspace.EvidenceSummaryItems,
            item => item.Title == "Historie" && item.Summary.Contains("Poslední událost: Novější servis", StringComparison.CurrentCulture));
        Assert.Contains(
            viewModel.VehicleDetailWorkspace.EvidenceSummaryItems,
            item => item.Title == "Tankování" && item.Summary.Contains("Poslední tankování: 35 l za 1200", StringComparison.CurrentCulture));
        Assert.Contains(
            viewModel.VehicleDetailWorkspace.EvidenceSummaryItems,
            item => item.Title == "Připomínky" && item.Summary.Contains("Nejbližší: Objednat servis", StringComparison.CurrentCulture));
        Assert.Contains(
            viewModel.VehicleDetailWorkspace.EvidenceSummaryItems,
            item => item.Title == "Doklady" && item.Summary.Contains("Bez vyplněné cesty: 1", StringComparison.CurrentCulture));
        Assert.Contains(
            viewModel.VehicleDetailWorkspace.EvidenceSummaryItems,
            item => item.Title == "Údržba" && item.Summary.Contains("Nejbližší: Motorový olej", StringComparison.CurrentCulture));
    }

    [Fact]
    public async Task Open_nearest_technical_quick_action_selects_matching_vehicle()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.OpenNearestTechnicalCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsDetailTabSelected);
        Assert.Equal("Milena", viewModel.SelectedVehicle?.Name);
        Assert.Equal(DesktopFocusTarget.VehicleList, requestedFocus);
        Assert.Contains("Nejbližší technická kontrola", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public void Clear_vehicle_filters_restores_full_list_and_focuses_search()
    {
        var dataSet = BuildQuickActionDataSet();
        var viewModel = CreateViewModel(dataSet);
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.VehicleSearchText = "Božena";
        viewModel.SelectedVehicleCategoryFilter = "Osobní vozidla";
        viewModel.SelectedVehicleStatusFilter = MainWindowViewModel.MissingGreenVehicleStatusFilterLabel;
        viewModel.HideInactiveVehicles = true;

        Assert.True(viewModel.ClearVehicleFiltersCommand.CanExecute(null));
        Assert.True(viewModel.Vehicles.Count < dataSet.Vehicles.Count);

        viewModel.ClearVehicleFiltersCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.VehicleSearchText);
        Assert.Equal(MainWindowViewModel.AllVehicleCategoriesLabel, viewModel.SelectedVehicleCategoryFilter);
        Assert.Equal(MainWindowViewModel.AllVehicleStatusFilterLabel, viewModel.SelectedVehicleStatusFilter);
        Assert.False(viewModel.HideInactiveVehicles);
        Assert.Equal(dataSet.Vehicles.Count, viewModel.Vehicles.Count);
        Assert.False(viewModel.ClearVehicleFiltersCommand.CanExecute(null));
        Assert.Equal("0", dataSet.Settings.GetValue("app", "hide_inactive_vehicles", "0"));
        Assert.Equal(MainWindowViewModel.AllVehicleCategoriesLabel, dataSet.Settings.GetValue("app", "vehicle_category_filter", string.Empty));
        Assert.Equal(MainWindowViewModel.AllVehicleStatusFilterLabel, dataSet.Settings.GetValue("app", "vehicle_status_filter", string.Empty));
        Assert.Equal(DesktopFocusTarget.VehicleSearch, requestedFocus);
        Assert.Contains("Filtry seznamu vozidel byly vymazány", viewModel.ShellStatus, StringComparison.CurrentCulture);
    }

    [Fact]
    public void Stable_vehicle_filter_preferences_are_restored_from_settings()
    {
        var dataSet = BuildQuickActionDataSet();
        dataSet.Settings.SetValue("app", "vehicle_category_filter", "Osobní vozidla");
        dataSet.Settings.SetValue("app", "vehicle_status_filter", MainWindowViewModel.AttentionVehicleStatusFilterLabel);
        dataSet.Settings.SetValue("app", "hide_inactive_vehicles", "1");

        var viewModel = CreateViewModel(dataSet);

        Assert.Equal("Osobní vozidla", viewModel.SelectedVehicleCategoryFilter);
        Assert.Equal(MainWindowViewModel.AttentionVehicleStatusFilterLabel, viewModel.SelectedVehicleStatusFilter);
        Assert.True(viewModel.HideInactiveVehicles);
        Assert.True(viewModel.ClearVehicleFiltersCommand.CanExecute(null));
    }

    [Fact]
    public void Stable_vehicle_filter_changes_are_saved_to_settings()
    {
        var dataSet = BuildQuickActionDataSet();
        var viewModel = CreateViewModel(dataSet);

        viewModel.SelectedVehicleCategoryFilter = "Osobní vozidla";
        viewModel.SelectedVehicleStatusFilter = MainWindowViewModel.OverdueVehicleStatusFilterLabel;
        viewModel.HideInactiveVehicles = true;

        Assert.Equal("Osobní vozidla", dataSet.Settings.GetValue("app", "vehicle_category_filter", string.Empty));
        Assert.Equal(MainWindowViewModel.OverdueVehicleStatusFilterLabel, dataSet.Settings.GetValue("app", "vehicle_status_filter", string.Empty));
        Assert.Equal("1", dataSet.Settings.GetValue("app", "hide_inactive_vehicles", "0"));
    }

    [Fact]
    public void Unknown_vehicle_filter_preferences_fall_back_to_safe_defaults()
    {
        var dataSet = BuildQuickActionDataSet();
        dataSet.Settings.SetValue("app", "vehicle_category_filter", "Neznámá kategorie");
        dataSet.Settings.SetValue("app", "vehicle_status_filter", "Neznámý stav");

        var viewModel = CreateViewModel(dataSet);

        Assert.Equal(MainWindowViewModel.AllVehicleCategoriesLabel, viewModel.SelectedVehicleCategoryFilter);
        Assert.Equal(MainWindowViewModel.AllVehicleStatusFilterLabel, viewModel.SelectedVehicleStatusFilter);
        Assert.Equal(dataSet.Vehicles.Count, viewModel.Vehicles.Count);
        Assert.False(viewModel.ClearVehicleFiltersCommand.CanExecute(null));
    }

    [Fact]
    public void Vehicle_list_controls_are_locked_while_an_editor_is_open()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        viewModel.VehicleSearchText = "Milena";

        Assert.True(viewModel.CanUseVehicleList);
        Assert.False(viewModel.IsVehicleListLocked);
        Assert.True(viewModel.ClearVehicleFiltersCommand.CanExecute(null));

        viewModel.EditSelectedVehicleCommand.Execute(null);

        Assert.True(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.False(viewModel.CanUseVehicleList);
        Assert.True(viewModel.IsVehicleListLocked);
        Assert.False(viewModel.CanUseWorkspaceNavigation);
        Assert.True(viewModel.IsWorkspaceNavigationLocked);
        Assert.Contains("detail vozidla", viewModel.VehicleListLockStatus, StringComparison.CurrentCultureIgnoreCase);
        Assert.Contains("Uložte nebo zrušte", viewModel.VehicleListLockStatus, StringComparison.CurrentCulture);
        Assert.False(viewModel.ClearVehicleFiltersCommand.CanExecute(null));

        viewModel.CancelVehicleEditCommand.Execute(null);

        Assert.False(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.True(viewModel.CanUseVehicleList);
        Assert.False(viewModel.IsVehicleListLocked);
        Assert.True(viewModel.CanUseWorkspaceNavigation);
        Assert.False(viewModel.IsWorkspaceNavigationLocked);
        Assert.True(viewModel.ClearVehicleFiltersCommand.CanExecute(null));
    }

    [Fact]
    public async Task Review_green_cards_quick_action_opens_matching_overview_filter()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.ReviewGreenCardsCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsUpcomingOverviewTabSelected);
        Assert.Equal("Zelené karty", viewModel.UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter);
        Assert.Equal(DesktopFocusTarget.UpcomingOverviewList, requestedFocus);
        Assert.NotEmpty(viewModel.UpcomingOverviewWorkspace.UpcomingOverviewItems);
    }

    [Fact]
    public async Task Review_green_cards_quick_action_opens_missing_green_cards_when_no_due_green_cards()
    {
        var dataSet = BuildQuickActionDataSet();
        dataSet.Vehicles.Clear();
        dataSet.Reminders.Clear();
        dataSet.MaintenancePlans.Clear();
        dataSet.Records.Clear();
        dataSet.VehicleMetaEntries.Clear();
        dataSet.Vehicles.Add(new Vehicle("veh_ok", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "12/2099", "01/2099", "12/2099"));
        dataSet.Vehicles.Add(new Vehicle("veh_missing", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "", "1973", "30", "", "12/2099", "", ""));
        var viewModel = CreateViewModel(dataSet);
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.ReviewGreenCardsCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsUpcomingOverviewTabSelected);
        Assert.True(viewModel.UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview);
        Assert.Equal("Zelené karty", viewModel.UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter);
        Assert.Equal(DesktopFocusTarget.UpcomingOverviewList, requestedFocus);
        Assert.Contains(
            viewModel.UpcomingOverviewWorkspace.UpcomingOverviewItems,
            item => item.VehicleId == "veh_missing" && item.Title == "Chybí zelená karta");
        Assert.Equal("veh_missing", viewModel.UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem?.VehicleId);
        Assert.Contains("bez zelené karty", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Open_nearest_reminder_quick_action_selects_matching_reminder()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.OpenNearestReminderCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsReminderTabSelected);
        Assert.Equal("Božena", viewModel.SelectedVehicle?.Name);
        Assert.Equal("rem_1", viewModel.ReminderWorkspace.SelectedReminder?.Id);
        Assert.Equal(DesktopFocusTarget.ReminderList, requestedFocus);
        Assert.Contains("Nejbližší připomínka", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Review_reminders_quick_action_opens_matching_overview_filter()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.ReviewRemindersCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsOverdueOverviewTabSelected);
        Assert.Equal("Připomínky", viewModel.OverdueOverviewWorkspace.SelectedOverdueOverviewFilter);
        Assert.Equal(DesktopFocusTarget.OverdueOverviewList, requestedFocus);
        Assert.NotEmpty(viewModel.OverdueOverviewWorkspace.OverdueOverviewItems);
        Assert.Contains("Připomínky k prověření", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Open_nearest_maintenance_quick_action_selects_matching_plan()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.OpenNearestMaintenanceCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsMaintenanceTabSelected);
        Assert.Equal("Božena", viewModel.SelectedVehicle?.Name);
        Assert.Equal("mnt_1", viewModel.MaintenanceWorkspace.SelectedMaintenance?.Id);
        Assert.Equal(DesktopFocusTarget.MaintenanceList, requestedFocus);
        Assert.Contains("Nejbližší servis", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Review_maintenance_quick_action_opens_matching_overview_filter()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.ReviewMaintenanceCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsOverdueOverviewTabSelected);
        Assert.Equal("Údržba", viewModel.OverdueOverviewWorkspace.SelectedOverdueOverviewFilter);
        Assert.Equal(DesktopFocusTarget.OverdueOverviewList, requestedFocus);
        Assert.NotEmpty(viewModel.OverdueOverviewWorkspace.OverdueOverviewItems);
        Assert.Contains("Údržba k prověření", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Open_nearest_record_quick_action_selects_matching_record()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.OpenNearestRecordCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsRecordTabSelected);
        Assert.Equal("Božena", viewModel.SelectedVehicle?.Name);
        Assert.Equal("rec_1", viewModel.RecordWorkspace.SelectedRecord?.Id);
        Assert.Equal(DesktopFocusTarget.RecordList, requestedFocus);
        Assert.Contains("Nejbližší doklad", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Review_records_quick_action_opens_matching_overview_filter()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.ReviewRecordsCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsOverdueOverviewTabSelected);
        Assert.Equal("Doklady", viewModel.OverdueOverviewWorkspace.SelectedOverdueOverviewFilter);
        Assert.Equal(DesktopFocusTarget.OverdueOverviewList, requestedFocus);
        Assert.NotEmpty(viewModel.OverdueOverviewWorkspace.OverdueOverviewItems);
        Assert.Contains("Doklady k prověření", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Open_background_notification_prefers_current_due_timeline_item()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        Assert.True(viewModel.CanOpenBackgroundNotificationQuickAction);
        Assert.True(viewModel.OpenBackgroundNotificationQuickActionCommand.CanExecute(null));

        await viewModel.OpenBackgroundNotificationQuickActionCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsDetailTabSelected);
        Assert.Equal("Milena", viewModel.SelectedVehicle?.Name);
        Assert.Equal(DesktopFocusTarget.VehicleList, requestedFocus);
        Assert.Contains("Otevřeno aktuální upozornění", viewModel.ShellStatus, StringComparison.CurrentCulture);
        Assert.Contains("Milena", viewModel.ShellStatus, StringComparison.CurrentCulture);
    }

    [Fact]
    public async Task Open_background_notification_falls_back_to_first_audit_item()
    {
        var dataSet = BuildQuietQuickActionDataSet();
        dataSet.Vehicles.Clear();
        dataSet.Vehicles.Add(new Vehicle("veh_audit", "Bez spz", "Osobní vozidla", "", "Škoda 120", "", "1988", "43", "", "12/2099", "01/2099", "12/2099"));
        var viewModel = CreateViewModel(dataSet);
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        var opened = await viewModel.OpenBackgroundNotificationAsync();

        Assert.True(opened);
        Assert.True(viewModel.IsDetailTabSelected);
        Assert.Equal("Bez spz", viewModel.SelectedVehicle?.Name);
        Assert.Equal(DesktopFocusTarget.VehicleList, requestedFocus);
        Assert.Contains("Otevřena auditní položka", viewModel.ShellStatus, StringComparison.CurrentCulture);
    }

    [Fact]
    public void Tray_actions_availability_reflects_current_quick_action_targets()
    {
        var activeViewModel = CreateViewModel(BuildQuickActionDataSet());
        var activeModel = activeViewModel.BuildTrayActionsDialogModel();

        Assert.True(activeViewModel.CanUseDataActions);
        Assert.True(activeViewModel.CanOpenNearestTechnicalQuickAction);
        Assert.True(activeViewModel.CanOpenNearestGreenCardQuickAction);
        Assert.True(activeViewModel.CanOpenNearestReminderQuickAction);
        Assert.True(activeViewModel.CanOpenNearestMaintenanceQuickAction);
        Assert.True(activeViewModel.CanOpenNearestRecordQuickAction);
        Assert.True(activeViewModel.CanReviewTechnicalQuickAction);
        Assert.True(activeViewModel.CanReviewGreenCardsQuickAction);
        Assert.True(activeViewModel.CanReviewRemindersQuickAction);
        Assert.True(activeViewModel.CanReviewMaintenanceQuickAction);
        Assert.True(activeViewModel.CanReviewRecordsQuickAction);
        Assert.True(activeViewModel.CanOpenBackgroundNotificationQuickAction);
        Assert.True(activeViewModel.OpenBackgroundNotificationQuickActionCommand.CanExecute(null));
        Assert.True(activeModel.CanOpenNearestTechnical);
        Assert.True(activeModel.CanOpenNearestGreenCard);
        Assert.True(activeModel.CanOpenNearestReminder);
        Assert.True(activeModel.CanOpenNearestMaintenance);
        Assert.True(activeModel.CanOpenNearestRecord);
        Assert.True(activeModel.CanReviewTechnical);
        Assert.True(activeModel.CanReviewGreenCards);
        Assert.True(activeModel.CanReviewReminders);
        Assert.True(activeModel.CanReviewMaintenance);
        Assert.True(activeModel.CanReviewRecords);
        Assert.True(activeModel.CanOpenBackgroundStatus);
        Assert.True(activeModel.CanCreateAutomaticBackupNow);
        Assert.True(activeModel.CanOpenAutomaticBackupFolder);
        Assert.Contains("Vehimap:", activeModel.BackgroundStatus, StringComparison.Ordinal);
        Assert.Contains("termínů k řešení", activeModel.BackgroundStatus, StringComparison.Ordinal);

        var quietViewModel = CreateViewModel(BuildQuietQuickActionDataSet());
        var quietModel = quietViewModel.BuildTrayActionsDialogModel();

        Assert.False(quietViewModel.CanOpenNearestTechnicalQuickAction);
        Assert.False(quietViewModel.CanOpenNearestGreenCardQuickAction);
        Assert.False(quietViewModel.CanOpenNearestReminderQuickAction);
        Assert.False(quietViewModel.CanOpenNearestMaintenanceQuickAction);
        Assert.False(quietViewModel.CanOpenNearestRecordQuickAction);
        Assert.False(quietViewModel.CanReviewTechnicalQuickAction);
        Assert.False(quietViewModel.CanReviewGreenCardsQuickAction);
        Assert.False(quietViewModel.CanReviewRemindersQuickAction);
        Assert.False(quietViewModel.CanReviewMaintenanceQuickAction);
        Assert.False(quietViewModel.CanReviewRecordsQuickAction);
        Assert.False(quietViewModel.CanOpenBackgroundNotificationQuickAction);
        Assert.False(quietViewModel.OpenBackgroundNotificationQuickActionCommand.CanExecute(null));
        Assert.True(quietModel.CanShowMainWindow);
        Assert.True(quietModel.CanCreateAutomaticBackupNow);
        Assert.True(quietModel.CanOpenAutomaticBackupFolder);
        Assert.True(quietModel.CanOpenDataFolder);
        Assert.True(quietModel.CanOpenSettings);
        Assert.False(quietModel.CanOpenNearestTechnical);
        Assert.False(quietModel.CanOpenNearestGreenCard);
        Assert.False(quietModel.CanOpenNearestReminder);
        Assert.False(quietModel.CanOpenNearestMaintenance);
        Assert.False(quietModel.CanOpenNearestRecord);
        Assert.False(quietModel.CanReviewTechnical);
        Assert.False(quietModel.CanReviewGreenCards);
        Assert.False(quietModel.CanReviewReminders);
        Assert.False(quietModel.CanReviewMaintenance);
        Assert.False(quietModel.CanReviewRecords);
        Assert.False(quietModel.CanOpenBackgroundStatus);
        Assert.Contains("Pozadí je aktivní", quietModel.BackgroundStatus, StringComparison.Ordinal);
        Assert.Contains("Vozidla:", quietModel.BackgroundStatus, StringComparison.Ordinal);
    }

    [Fact]
    public void Quick_actions_treat_english_no_alert_status_as_quiet()
    {
        var viewModel = CreateViewModel(
            BuildQuietQuickActionDataSet(),
            new LegacyTimelineService(new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"))));
        var model = viewModel.BuildTrayActionsDialogModel();

        Assert.False(viewModel.CanOpenNearestTechnicalQuickAction);
        Assert.False(viewModel.CanOpenNearestGreenCardQuickAction);
        Assert.False(viewModel.CanOpenNearestReminderQuickAction);
        Assert.False(viewModel.CanOpenNearestMaintenanceQuickAction);
        Assert.False(viewModel.CanOpenNearestRecordQuickAction);
        Assert.False(viewModel.CanOpenBackgroundNotificationQuickAction);
        Assert.False(model.CanOpenBackgroundStatus);
    }

    [Fact]
    public void Tray_actions_disable_navigation_and_data_actions_while_editing()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());

        viewModel.EditSelectedVehicleCommand.Execute(null);
        var model = viewModel.BuildTrayActionsDialogModel();

        Assert.False(viewModel.CanOpenNearestTechnicalQuickAction);
        Assert.False(viewModel.CanOpenBackgroundNotificationQuickAction);
        Assert.False(viewModel.OpenBackgroundNotificationQuickActionCommand.CanExecute(null));
        Assert.False(viewModel.CanReviewTechnicalQuickAction);
        Assert.False(viewModel.CanUseDataActions);
        Assert.True(model.CanShowMainWindow);
        Assert.True(model.CanOpenAbout);
        Assert.True(model.CanThankAuthor);
        Assert.True(model.CanCheckForUpdates);
        Assert.True(model.CanExit);
        Assert.False(model.CanShowDashboard);
        Assert.False(model.CanShowUpcomingOverview);
        Assert.False(model.CanShowOverdueOverview);
        Assert.False(model.CanOpenBackgroundStatus);
        Assert.False(model.CanOpenNearestTechnical);
        Assert.False(model.CanReviewTechnical);
        Assert.False(model.CanOpenPrintableReport);
        Assert.False(model.CanExportBackup);
        Assert.False(model.CanImportBackup);
        Assert.False(model.CanCreateAutomaticBackupNow);
        Assert.False(model.CanOpenAutomaticBackupFolder);
        Assert.False(model.CanOpenSettings);
        Assert.False(model.CanExportCalendar);
        Assert.False(model.CanReloadData);
        Assert.False(model.CanOpenDataFolder);
    }

    private static MainWindowViewModel CreateViewModel(VehimapDataSet dataSet, ITimelineService? timelineService = null)
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataStore = new VehicleListQuickActionStubLegacyDataStore(dataSet);
        var bootstrapper = new LegacyVehimapBootstrapper(new VehicleListQuickActionStubDataRootLocator(dataRoot), dataStore);

        return new MainWindowViewModel(
            dataStore,
            bootstrapper,
            new ManagedAttachmentPathService(),
            new VehicleListQuickActionStubFileLauncher(),
            new VehicleListQuickActionStubFilePickerService(),
            new LegacyGlobalSearchService(new ManagedAttachmentPathService()),
            timelineService ?? new LegacyTimelineService(),
            new LegacyCalendarExportService(),
            new VehicleListQuickActionStubTextFileSaveService(),
            new VehicleListQuickActionStubBackupService(),
            new VehicleListQuickActionStubFileDialogService(),
            new DesktopSupportedSettingsService(),
            new VehicleListQuickActionStubBuildInfoProvider());
    }

    private static VehimapDataSet BuildQuickActionDataSet()
    {
        return new VehimapDataSet
        {
            Settings = BuildSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "05/2026", "01/2026", "06/2027"),
                new Vehicle("veh_2", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "", "1973", "30", "", "09/2099", "01/2099", "10/2099")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_2", "Objednat servis", "01.01.2000", "30", "Neopakovat", "Zavolat servisu")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_2", "Motorový olej", "", "12", "01.01.1999", "", true, "Roční servis")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_2", "Doklad", "Asistence", "Pomoc na cestách", "", "01/2000", "1200", VehicleRecordAttachmentMode.External, "", "Prověřit smlouvu")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Běžný provoz", "", "Benzín", "Má klimatizaci", "Řemen", "Manuální"),
                new VehicleMeta("veh_2", "Veterán", "veterán, srazy", "Benzín", "", "", "")
            ]
        };
    }

    private static VehimapDataSet BuildQuietQuickActionDataSet()
    {
        return new VehimapDataSet
        {
            Settings = BuildSettings(),
            Vehicles =
            [
                new Vehicle("veh_quiet", "Klidné auto", "Osobní vozidla", "", "Škoda Octavia", "1AB2345", "2024", "110", "", "12/2099", "01/2099", "12/2099")
            ]
        };
    }

    private static VehimapSettings BuildSettings()
    {
        var settings = new VehimapSettings();
        settings.SetValue("notifications", "technical_reminder_days", "365");
        settings.SetValue("notifications", "green_card_reminder_days", "365");
        settings.SetValue("notifications", "maintenance_reminder_days", "31");
        settings.SetValue("notifications", "maintenance_reminder_km", "1000");
        return settings;
    }

    private sealed class VehicleListQuickActionStubDataRootLocator : IDataRootLocator
    {
        private readonly VehimapDataRoot _dataRoot;

        public VehicleListQuickActionStubDataRootLocator(VehimapDataRoot dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public VehimapDataRoot Resolve(string appBasePath) => _dataRoot;
    }

    private sealed class VehicleListQuickActionStubLegacyDataStore : ILegacyDataStore
    {
        public VehicleListQuickActionStubLegacyDataStore(VehimapDataSet dataSet)
        {
            CurrentDataSet = dataSet;
        }

        public VehimapDataSet CurrentDataSet { get; set; }

        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
            => Task.FromResult(CurrentDataSet);

        public Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            CurrentDataSet = dataSet;
            return Task.CompletedTask;
        }
    }

    private sealed class VehicleListQuickActionStubFileLauncher : IFileLauncher
    {
        public Task OpenAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class VehicleListQuickActionStubTextFileSaveService : ITextFileSaveService
    {
        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class VehicleListQuickActionStubFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class VehicleListQuickActionStubFileDialogService : IFileDialogService
    {
        public Task<string?> PickOpenFileAsync(string title, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class VehicleListQuickActionStubBuildInfoProvider : IAppBuildInfoProvider
    {
        public AppBuildInfo GetCurrent() => new(
            "Vehimap",
            "1.0.2",
            "1.0.2.0",
            "vývojový Avalonia shell",
            @"C:\vehimap\Vehimap.Desktop.exe",
            "Windows",
            ".NET 10",
            "https://example.com/latest.ini",
            "https://example.com/release",
            @"C:\vehimap\Vehimap.Updater.exe",
            false);
    }

    private sealed class VehicleListQuickActionStubBackupService : IBackupService
    {
        public Task<BackupExportResult> ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default) =>
            Task.FromResult(new BackupExportResult(backupPath, 0, 0));

        public Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default) =>
            Task.FromResult(new VehimapBackupBundle(new VehimapDataSet(), []));

        public Task<BackupRestoreResult> RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default) =>
            Task.FromResult(new BackupRestoreResult(null, 0));
    }
}
