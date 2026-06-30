using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed record TrayActionsDialogViewModel(
    string Title,
    string Description,
    string BackgroundStatus,
    string OpenBackgroundStatusLabel,
    string ShowMainWindowLabel,
    string ShowDashboardLabel,
    string ShowUpcomingOverviewLabel,
    string ShowOverdueOverviewLabel,
    string OpenNearestTechnicalLabel,
    string OpenNearestGreenCardLabel,
    string OpenNearestReminderLabel,
    string OpenNearestMaintenanceLabel,
    string OpenNearestRecordLabel,
    string ReviewTechnicalLabel,
    string ReviewGreenCardsLabel,
    string ReviewRemindersLabel,
    string ReviewMaintenanceLabel,
    string ReviewRecordsLabel,
    string OpenPrintableReportLabel,
    string ExportBackupLabel,
    string ImportBackupLabel,
    string CreateAutomaticBackupNowLabel,
    string OpenAutomaticBackupFolderLabel,
    string OpenSettingsLabel,
    string ExportCalendarLabel,
    string ReloadDataLabel,
    string OpenDataFolderLabel,
    string OpenAboutLabel,
    string ThankAuthorLabel,
    string FeedbackIssueLabel,
    string CheckForUpdatesLabel,
    string ExitLabel,
    string CancelLabel)
{
    public bool CanShowMainWindow { get; init; } = true;
    public bool CanOpenBackgroundStatus { get; init; }
    public bool CanShowDashboard { get; init; } = true;
    public bool CanShowUpcomingOverview { get; init; } = true;
    public bool CanShowOverdueOverview { get; init; } = true;
    public bool CanOpenNearestTechnical { get; init; } = true;
    public bool CanOpenNearestGreenCard { get; init; } = true;
    public bool CanOpenNearestReminder { get; init; } = true;
    public bool CanOpenNearestMaintenance { get; init; } = true;
    public bool CanOpenNearestRecord { get; init; } = true;
    public bool CanReviewTechnical { get; init; } = true;
    public bool CanReviewGreenCards { get; init; } = true;
    public bool CanReviewReminders { get; init; } = true;
    public bool CanReviewMaintenance { get; init; } = true;
    public bool CanReviewRecords { get; init; } = true;
    public bool CanOpenPrintableReport { get; init; } = true;
    public bool CanExportBackup { get; init; } = true;
    public bool CanImportBackup { get; init; } = true;
    public bool CanCreateAutomaticBackupNow { get; init; } = true;
    public bool CanOpenAutomaticBackupFolder { get; init; } = true;
    public bool CanOpenSettings { get; init; } = true;
    public bool CanExportCalendar { get; init; } = true;
    public bool CanReloadData { get; init; } = true;
    public bool CanOpenDataFolder { get; init; } = true;
    public bool CanOpenAbout { get; init; } = true;
    public bool CanThankAuthor { get; init; } = true;
    public bool CanReportFeedback { get; init; } = true;
    public bool CanCheckForUpdates { get; init; } = true;
    public bool CanExit { get; init; } = true;

    public static TrayActionsDialogViewModel CreateDefault(IAppLocalizer? localizer = null)
    {
        var effectiveLocalizer = localizer ?? new ResourceAppLocalizer();
        return new(
            effectiveLocalizer.GetString("TrayActions.Title"),
            effectiveLocalizer.GetString("TrayActions.Description"),
            effectiveLocalizer.GetString("TrayActions.BackgroundStatusUnavailable"),
            effectiveLocalizer.GetString("TrayActions.OpenBackgroundStatusLabel"),
            effectiveLocalizer.GetString("TrayActions.ShowMainWindowLabel"),
            effectiveLocalizer.GetString("TrayActions.ShowDashboardLabel"),
            effectiveLocalizer.GetString("TrayActions.ShowUpcomingOverviewLabel"),
            effectiveLocalizer.GetString("TrayActions.ShowOverdueOverviewLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenNearestTechnicalLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenNearestGreenCardLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenNearestReminderLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenNearestMaintenanceLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenNearestRecordLabel"),
            effectiveLocalizer.GetString("TrayActions.ReviewTechnicalLabel"),
            effectiveLocalizer.GetString("TrayActions.ReviewGreenCardsLabel"),
            effectiveLocalizer.GetString("TrayActions.ReviewRemindersLabel"),
            effectiveLocalizer.GetString("TrayActions.ReviewMaintenanceLabel"),
            effectiveLocalizer.GetString("TrayActions.ReviewRecordsLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenPrintableReportLabel"),
            effectiveLocalizer.GetString("TrayActions.ExportBackupLabel"),
            effectiveLocalizer.GetString("TrayActions.ImportBackupLabel"),
            effectiveLocalizer.GetString("TrayActions.CreateAutomaticBackupNowLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenAutomaticBackupFolderLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenSettingsLabel"),
            effectiveLocalizer.GetString("TrayActions.ExportCalendarLabel"),
            effectiveLocalizer.GetString("TrayActions.ReloadDataLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenDataFolderLabel"),
            effectiveLocalizer.GetString("TrayActions.OpenAboutLabel"),
            effectiveLocalizer.GetString("TrayActions.ThankAuthorLabel"),
            effectiveLocalizer.GetString("TrayActions.FeedbackIssueLabel"),
            effectiveLocalizer.GetString("TrayActions.CheckForUpdatesLabel"),
            effectiveLocalizer.GetString("TrayActions.ExitLabel"),
            effectiveLocalizer.GetString("TrayActions.CancelLabel"));
    }
}
