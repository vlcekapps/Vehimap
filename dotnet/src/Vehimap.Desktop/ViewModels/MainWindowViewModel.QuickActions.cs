using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    [RelayCommand]
    private async Task OpenNearestTechnicalAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít nejbližší technickou kontrolu").ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("technical");
        if (items.Count == 0)
        {
            ShellStatus = "Momentálně není žádné vozidlo s blížící se nebo propadlou technickou kontrolou.";
            return;
        }

        ShellStatus = $"Nejbližší technická kontrola: {items[0].VehicleName} - {items[0].Date}.";
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewTechnicalAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("zkontrolovat technické kontroly").ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "technical",
            AttentionVehicleStatusFilterLabel,
            "Technické kontroly",
            "Žádná vozidla teď nevyžadují upozornění na technickou kontrolu.");
    }

    [RelayCommand]
    private async Task OpenNearestGreenCardAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít nejbližší zelenou kartu").ConfigureAwait(true))
        {
            return;
        }

        if (!HasAnyGreenCardConfigured())
        {
            ShellStatus = "U žádného vozidla není vyplněná zelená karta. Můžete ji doplnit v detailu vozidla.";
            return;
        }

        var items = BuildQuickActionItems("green");
        if (items.Count == 0)
        {
            ShellStatus = HasAnyMissingGreenCard()
                ? "Žádná vyplněná zelená karta teď nevyžaduje upozornění. U některých vozidel zelená karta vyplněná není."
                : "Žádná vyplněná zelená karta teď nevyžaduje upozornění.";
            return;
        }

        ShellStatus = $"Nejbližší zelená karta: {items[0].VehicleName} - {items[0].Date}.";
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewGreenCardsAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("zkontrolovat zelené karty").ConfigureAwait(true))
        {
            return;
        }

        var hasMissingGreenCard = HasAnyMissingGreenCard();
        var greenCardItems = BuildQuickActionItems("green");
        if (greenCardItems.Count == 0 && hasMissingGreenCard)
        {
            OpenMissingGreenCardsOverview();
            return;
        }

        if (greenCardItems.Count == 0)
        {
            ShellStatus = HasAnyGreenCardConfigured()
                ? "Žádná vyplněná zelená karta teď nevyžaduje upozornění."
                : "U žádného vozidla není vyplněná zelená karta. Můžete ji doplnit v detailu vozidla.";
            return;
        }

        OpenQuickActionOverview(
            "green",
            MissingGreenVehicleStatusFilterLabel,
            "Zelené karty",
            "Žádná vyplněná zelená karta teď nevyžaduje upozornění.",
            includeMissingGreenCards: hasMissingGreenCard);
    }

    [RelayCommand]
    private async Task OpenNearestReminderAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít nejbližší připomínku").ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("custom");
        if (items.Count == 0)
        {
            ShellStatus = "Momentálně není žádná vlastní připomínka s blížícím se nebo propadlým termínem.";
            return;
        }

        ShellStatus = $"Nejbližší připomínka: {items[0].VehicleName} - {items[0].Title} ({items[0].Date}).";
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewRemindersAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("zkontrolovat připomínky").ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "custom",
            AttentionVehicleStatusFilterLabel,
            "Připomínky",
            "Žádné vlastní připomínky teď nevyžadují upozornění.");
    }

    [RelayCommand]
    private async Task OpenNearestMaintenanceAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít nejbližší servisní úkon").ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("maintenance");
        if (items.Count == 0)
        {
            ShellStatus = "Momentálně není žádný servisní úkon s blížícím se nebo propadlým termínem.";
            return;
        }

        ShellStatus = $"Nejbližší servis: {items[0].VehicleName} - {items[0].Title} ({items[0].Date}).";
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewMaintenanceAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("zkontrolovat plán údržby").ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "maintenance",
            AttentionVehicleStatusFilterLabel,
            "Údržba",
            "Žádné servisní úkony teď nevyžadují upozornění.");
    }

    [RelayCommand]
    private async Task OpenNearestRecordAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít nejbližší doklad").ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("record");
        if (items.Count == 0)
        {
            ShellStatus = "Momentálně není žádný doklad s blížící se nebo propadlou platností.";
            return;
        }

        ShellStatus = $"Nejbližší doklad: {items[0].VehicleName} - {items[0].Title} ({items[0].Date}).";
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewRecordsAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("zkontrolovat doklady").ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "record",
            AttentionVehicleStatusFilterLabel,
            "Doklady",
            "Žádné doklady teď nevyžadují upozornění.");
    }

    private List<VehicleTimelineItemViewModel> BuildQuickActionItems(string kind)
    {
        return _dataSet.Vehicles
            .SelectMany(vehicle => _timelineService.BuildVehicleTimeline(_dataSet, vehicle.Id, DateOnly.FromDateTime(DateTime.Today)))
            .Where(item => string.Equals(item.Kind, kind, StringComparison.Ordinal))
            .Where(item => !string.IsNullOrWhiteSpace(item.Status) && !string.Equals(item.Status, "Bez upozornění", StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(item => item.IsFuture ? 1 : 0)
            .ThenBy(item => item.IsFuture ? item.Date.DayNumber : -item.Date.DayNumber)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleTimelineItemViewModel(
                item.Kind,
                item.KindLabel,
                item.DateText,
                item.Title,
                item.Detail,
                item.Status,
                item.VehicleName,
                item.VehicleId,
                item.EntryId,
                item.IsFuture,
                item.Note))
            .ToList();
    }

    private void OpenQuickActionOverview(
        string kind,
        string emptyFilterStatusMessage,
        string overviewFilterLabel,
        string emptyMessage,
        bool includeMissingGreenCards = false)
    {
        var items = BuildQuickActionItems(kind);
        if (items.Count == 0)
        {
            ShellStatus = emptyMessage;
            return;
        }

        var targetTabIndex = items.Any(item => !item.IsFuture)
            ? OverdueOverviewTabIndex
            : UpcomingOverviewTabIndex;

        if (targetTabIndex == UpcomingOverviewTabIndex)
        {
            UpcomingOverviewWorkspace.UpcomingOverviewSearchText = string.Empty;
            UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview = includeMissingGreenCards;
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter = overviewFilterLabel;
            RefreshUpcomingOverview();
            SelectedVehicleTabIndex = UpcomingOverviewTabIndex;
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem = FindById(
                UpcomingOverviewItems,
                BuildOverviewSelectionKey,
                BuildOverviewSelectionKey(items[0]));
            RequestFocus(DesktopFocusTarget.UpcomingOverviewList);
        }
        else
        {
            OverdueOverviewWorkspace.OverdueOverviewSearchText = string.Empty;
            OverdueOverviewWorkspace.SelectedOverdueOverviewFilter = overviewFilterLabel;
            RefreshOverdueOverview();
            SelectedVehicleTabIndex = OverdueOverviewTabIndex;
            OverdueOverviewWorkspace.SelectedOverdueOverviewItem = FindById(
                OverdueOverviewItems,
                BuildOverviewSelectionKey,
                BuildOverviewSelectionKey(items[0]));
            RequestFocus(DesktopFocusTarget.OverdueOverviewList);
        }

        ShellStatus = kind switch
        {
            "technical" => $"Technické kontroly k prověření: {items.Count}. Otevřen je příslušný přehled.",
            "green" => $"Zelené karty k prověření: {items.Count}. Otevřen je příslušný přehled.",
            "custom" => $"Připomínky k prověření: {items.Count}. Otevřen je příslušný přehled.",
            "maintenance" => $"Údržba k prověření: {items.Count}. Otevřen je příslušný přehled.",
            "record" => $"Doklady k prověření: {items.Count}. Otevřen je příslušný přehled.",
            _ => emptyFilterStatusMessage
        };
    }

    private void OpenMissingGreenCardsOverview()
    {
        var missingCount = _dataSet.Vehicles.Count(vehicle => string.IsNullOrWhiteSpace(vehicle.GreenCardTo));
        UpcomingOverviewWorkspace.UpcomingOverviewSearchText = string.Empty;
        UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview = true;
        UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter = "Zelené karty";
        RefreshUpcomingOverview();
        SelectedVehicleTabIndex = UpcomingOverviewTabIndex;
        UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem =
            UpcomingOverviewItems.FirstOrDefault(item => string.Equals(item.Title, "Chybí zelená karta", StringComparison.CurrentCultureIgnoreCase))
            ?? UpcomingOverviewItems.FirstOrDefault();
        RequestFocus(DesktopFocusTarget.UpcomingOverviewList);
        ShellStatus = $"Vozidla bez zelené karty k doplnění: {missingCount}. Otevřen je přehled blížících se termínů.";
    }

    private bool HasAnyGreenCardConfigured() =>
        _dataSet.Vehicles.Any(vehicle => !string.IsNullOrWhiteSpace(vehicle.GreenCardTo));

    private bool HasAnyMissingGreenCard() =>
        _dataSet.Vehicles.Any(vehicle => string.IsNullOrWhiteSpace(vehicle.GreenCardTo));

    public bool CanOpenNearestTechnicalQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("technical");

    public bool CanReviewTechnicalQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("technical");

    public bool CanOpenNearestGreenCardQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("green");

    public bool CanReviewGreenCardsQuickAction => CanUseWorkspaceNavigation && (HasQuickActionTarget("green") || HasAnyMissingGreenCard());

    public bool CanOpenNearestReminderQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("custom");

    public bool CanReviewRemindersQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("custom");

    public bool CanOpenNearestMaintenanceQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("maintenance");

    public bool CanReviewMaintenanceQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("maintenance");

    public bool CanOpenNearestRecordQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("record");

    public bool CanReviewRecordsQuickAction => CanUseWorkspaceNavigation && HasQuickActionTarget("record");

    internal void NotifyQuickActionAvailabilityChanged()
    {
        OnPropertyChanged(nameof(CanOpenNearestTechnicalQuickAction));
        OnPropertyChanged(nameof(CanReviewTechnicalQuickAction));
        OnPropertyChanged(nameof(CanOpenNearestGreenCardQuickAction));
        OnPropertyChanged(nameof(CanReviewGreenCardsQuickAction));
        OnPropertyChanged(nameof(CanOpenNearestReminderQuickAction));
        OnPropertyChanged(nameof(CanReviewRemindersQuickAction));
        OnPropertyChanged(nameof(CanOpenNearestMaintenanceQuickAction));
        OnPropertyChanged(nameof(CanReviewMaintenanceQuickAction));
        OnPropertyChanged(nameof(CanOpenNearestRecordQuickAction));
        OnPropertyChanged(nameof(CanReviewRecordsQuickAction));
    }

    private bool HasQuickActionTarget(string kind) =>
        BuildQuickActionItems(kind).Count > 0;

    internal TrayActionsDialogViewModel BuildTrayActionsDialogModel()
    {
        var canUseDataActions = CanUseDataActions;
        var background = BuildBackgroundSnapshot();

        return TrayActionsDialogViewModel.CreateDefault() with
        {
            BackgroundStatus = BuildTrayBackgroundStatus(background),
            CanShowDashboard = CanUseWorkspaceNavigation,
            CanShowUpcomingOverview = CanUseWorkspaceNavigation,
            CanShowOverdueOverview = CanUseWorkspaceNavigation,
            CanOpenNearestTechnical = CanOpenNearestTechnicalQuickAction,
            CanOpenNearestGreenCard = CanOpenNearestGreenCardQuickAction,
            CanOpenNearestReminder = CanOpenNearestReminderQuickAction,
            CanOpenNearestMaintenance = CanOpenNearestMaintenanceQuickAction,
            CanOpenNearestRecord = CanOpenNearestRecordQuickAction,
            CanReviewTechnical = CanReviewTechnicalQuickAction,
            CanReviewGreenCards = CanReviewGreenCardsQuickAction,
            CanReviewReminders = CanReviewRemindersQuickAction,
            CanReviewMaintenance = CanReviewMaintenanceQuickAction,
            CanReviewRecords = CanReviewRecordsQuickAction,
            CanOpenPrintableReport = canUseDataActions,
            CanExportBackup = canUseDataActions,
            CanImportBackup = canUseDataActions,
            CanCreateAutomaticBackupNow = this.CanCreateAutomaticBackupNow,
            CanOpenAutomaticBackupFolder = this.CanOpenAutomaticBackupFolder,
            CanOpenSettings = canUseDataActions,
            CanExportCalendar = canUseDataActions,
            CanReloadData = canUseDataActions,
            CanOpenDataFolder = this.CanOpenDataFolder
        };
    }

    private static string BuildTrayBackgroundStatus(DesktopBackgroundSnapshot snapshot)
    {
        if (snapshot.HasNotification)
        {
            return $"{snapshot.NotificationTitle}. {snapshot.NotificationMessage}";
        }

        var toolTip = NormalizeTrayBackgroundText(snapshot.ToolTipText);
        return string.IsNullOrWhiteSpace(toolTip)
            ? "Pozadí je aktivní. Aktuálně není nic k oznámení."
            : $"Pozadí je aktivní. {toolTip}";
    }

    private static string NormalizeTrayBackgroundText(string value)
    {
        var parts = value
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => !string.Equals(part, "Vehimap Desktop", StringComparison.CurrentCultureIgnoreCase));

        return string.Join(" ", parts);
    }
}
