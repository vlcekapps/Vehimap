using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string QuickActionNoAlertLegacyCzech = "Bez upozorn\u011Bn\u00ED";
    private const string QuickActionNoAlertLegacyEnglish = "No alert";

    [RelayCommand]
    private async Task OpenNearestTechnicalAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.OpenNearestTechnical")).ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("technical");
        if (items.Count == 0)
        {
            ShellStatus = LO("QuickActions.Status.NoTechnical");
            return;
        }

        ShellStatus = LFO("QuickActions.Status.NearestTechnical", items[0].VehicleName, items[0].Date);
        OpenTimelineItem(items[0]);
    }

    [RelayCommand(CanExecute = nameof(CanOpenBackgroundNotificationQuickAction))]
    private async Task OpenBackgroundNotificationQuickActionAsync()
    {
        await OpenBackgroundNotificationAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task ReviewTechnicalAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.ReviewTechnical")).ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "technical",
            LO("Overview.Filter.Technical"),
            LO("QuickActions.Status.NoTechnical"),
            "QuickActions.Status.ReviewTechnicalOpened");
    }

    [RelayCommand]
    private async Task OpenNearestGreenCardAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.OpenNearestGreenCard")).ConfigureAwait(true))
        {
            return;
        }

        if (!HasAnyGreenCardConfigured())
        {
            ShellStatus = LO("QuickActions.Status.NoGreenCardsConfigured");
            return;
        }

        var items = BuildQuickActionItems("green");
        if (items.Count == 0)
        {
            ShellStatus = HasAnyMissingGreenCard()
                ? LO("QuickActions.Status.NoGreenCardsDueWithMissing")
                : LO("QuickActions.Status.NoGreenCardsDue");
            return;
        }

        ShellStatus = LFO("QuickActions.Status.NearestGreenCard", items[0].VehicleName, items[0].Date);
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewGreenCardsAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.ReviewGreenCards")).ConfigureAwait(true))
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
                ? LO("QuickActions.Status.NoGreenCardsDue")
                : LO("QuickActions.Status.NoGreenCardsConfigured");
            return;
        }

        OpenQuickActionOverview(
            "green",
            LO("Overview.Filter.GreenCards"),
            LO("QuickActions.Status.NoGreenCardsDue"),
            "QuickActions.Status.ReviewGreenOpened",
            includeMissingGreenCards: hasMissingGreenCard);
    }

    [RelayCommand]
    private async Task OpenNearestReminderAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.OpenNearestReminder")).ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("custom");
        if (items.Count == 0)
        {
            ShellStatus = LO("QuickActions.Status.NoReminder");
            return;
        }

        ShellStatus = LFO("QuickActions.Status.NearestReminder", items[0].VehicleName, items[0].Title, items[0].Date);
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewRemindersAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.ReviewReminders")).ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "custom",
            LO("Overview.Filter.Reminders"),
            LO("QuickActions.Status.NoReminder"),
            "QuickActions.Status.ReviewReminderOpened");
    }

    [RelayCommand]
    private async Task OpenNearestMaintenanceAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.OpenNearestMaintenance")).ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("maintenance");
        if (items.Count == 0)
        {
            ShellStatus = LO("QuickActions.Status.NoMaintenance");
            return;
        }

        ShellStatus = LFO("QuickActions.Status.NearestMaintenance", items[0].VehicleName, items[0].Title, items[0].Date);
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewMaintenanceAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.ReviewMaintenance")).ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "maintenance",
            LO("Overview.Filter.Maintenance"),
            LO("QuickActions.Status.NoMaintenance"),
            "QuickActions.Status.ReviewMaintenanceOpened");
    }

    [RelayCommand]
    private async Task OpenNearestRecordAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.OpenNearestRecord")).ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("record");
        if (items.Count == 0)
        {
            ShellStatus = LO("QuickActions.Status.NoRecord");
            return;
        }

        ShellStatus = LFO("QuickActions.Status.NearestRecord", items[0].VehicleName, items[0].Title, items[0].Date);
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewRecordsAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.ReviewRecords")).ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "record",
            LO("Overview.Filter.Records"),
            LO("QuickActions.Status.NoRecord"),
            "QuickActions.Status.ReviewRecordOpened");
    }

    private List<VehicleTimelineItemViewModel> BuildQuickActionItems(string kind)
    {
        return _dataSet.Vehicles
            .SelectMany(vehicle => _timelineService.BuildVehicleTimeline(_dataSet, vehicle.Id, DateOnly.FromDateTime(DateTime.Today)))
            .Where(item => string.Equals(item.Kind, kind, StringComparison.Ordinal))
            .Where(item => IsTimelineStatusAttention(item.Status))
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

    internal async Task<bool> OpenBackgroundNotificationAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("QuickActions.Action.OpenBackgroundNotification")).ConfigureAwait(true))
        {
            return false;
        }

        var attentionItems = BuildBackgroundAttentionItems();
        if (attentionItems.FirstOrDefault() is { } timelineItem)
        {
            ShellStatus = LFO("QuickActions.Status.OpenedBackgroundTimeline", timelineItem.VehicleName, timelineItem.Title, timelineItem.Date);
            OpenTimelineItem(timelineItem);
            return true;
        }

        if (AuditItems.FirstOrDefault() is { } auditItem)
        {
            ShellStatus = LFO("QuickActions.Status.OpenedBackgroundAudit", auditItem.VehicleName, auditItem.Title);
            SelectVehicleAndOpenEntity(auditItem.VehicleId, auditItem.EntityKind, auditItem.EntityId);
            return true;
        }

        ShellStatus = LO("QuickActions.Status.NoBackgroundNotification");
        return false;
    }

    private void OpenQuickActionOverview(
        string kind,
        string overviewFilterLabel,
        string emptyMessage,
        string openedStatusResourceKey,
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

        ShellStatus = LFO(openedStatusResourceKey, items.Count);
    }

    private void OpenMissingGreenCardsOverview()
    {
        var missingCount = _dataSet.Vehicles.Count(vehicle => string.IsNullOrWhiteSpace(vehicle.GreenCardTo));
        UpcomingOverviewWorkspace.UpcomingOverviewSearchText = string.Empty;
        UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview = true;
        UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter = LO("Overview.Filter.GreenCards");
        RefreshUpcomingOverview();
        SelectedVehicleTabIndex = UpcomingOverviewTabIndex;
        UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem =
            UpcomingOverviewItems.FirstOrDefault(item => string.Equals(item.Title, LO("Overview.MissingGreen.Title"), StringComparison.CurrentCultureIgnoreCase))
            ?? UpcomingOverviewItems.FirstOrDefault();
        RequestFocus(DesktopFocusTarget.UpcomingOverviewList);
        ShellStatus = LFO("QuickActions.Status.MissingGreenCardsOpened", missingCount);
    }

    private static bool IsTimelineStatusAttention(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var normalizedStatus = status.Trim();
        return !string.Equals(normalizedStatus, LO("Timeline.Status.NoAlert"), StringComparison.CurrentCultureIgnoreCase)
            && !string.Equals(normalizedStatus, QuickActionNoAlertLegacyCzech, StringComparison.CurrentCultureIgnoreCase)
            && !string.Equals(normalizedStatus, QuickActionNoAlertLegacyEnglish, StringComparison.OrdinalIgnoreCase);
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

    public bool CanOpenBackgroundNotificationQuickAction => CanOpenBackgroundNotification(BuildBackgroundSnapshot());

    internal void NotifyQuickActionAvailabilityChanged()
    {
        OnPropertyChanged(nameof(CanOpenBackgroundNotificationQuickAction));
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
        OpenBackgroundNotificationQuickActionCommand.NotifyCanExecuteChanged();
    }

    private bool HasQuickActionTarget(string kind) =>
        BuildQuickActionItems(kind).Count > 0;

    internal TrayActionsDialogViewModel BuildTrayActionsDialogModel()
    {
        var canUseDataActions = CanUseDataActions;
        var background = BuildBackgroundSnapshot();

        return TrayActionsDialogViewModel.CreateDefault(DesktopLocalization.Localizer) with
        {
            BackgroundStatus = BuildTrayBackgroundStatus(background),
            CanOpenBackgroundStatus = CanOpenBackgroundNotification(background),
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
            ? DesktopLocalization.Localizer.GetString("TrayActions.BackgroundStatus.NoNotification")
            : DesktopLocalization.Localizer.Format("TrayActions.BackgroundStatus.ActiveWithDetail", toolTip);
    }

    private bool CanOpenBackgroundNotification(DesktopBackgroundSnapshot snapshot) =>
        CanUseWorkspaceNavigation && snapshot.HasNotification;

    private static string NormalizeTrayBackgroundText(string value)
    {
        var parts = value
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => !string.Equals(part, "Vehimap Desktop", StringComparison.CurrentCultureIgnoreCase));

        return string.Join(" ", parts);
    }
}
