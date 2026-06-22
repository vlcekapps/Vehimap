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
    public bool CanCheckForUpdates { get; init; } = true;
    public bool CanExit { get; init; } = true;

    public static TrayActionsDialogViewModel CreateDefault() => new(
        "Akce Vehimapu na liště",
        "Vyberte akci pro běžící Vehimap. Toto okno nahrazuje nativní menu lišty pro přístupnější ovládání přes klávesnici a čtečku obrazovky.",
        "Stav pozadí zatím není dostupný.",
        "Otevřít aktuální upozornění",
        "Zobrazit Vehimap",
        "Otevřít Dashboard",
        "Blížící se termíny",
        "Propadlé termíny",
        "Nejbližší TK",
        "Nejbližší ZK",
        "Nejbližší připomínka",
        "Nejbližší servis",
        "Nejbližší doklad",
        "Zkontrolovat TK",
        "Zkontrolovat ZK",
        "Zkontrolovat připomínky",
        "Zkontrolovat údržbu",
        "Zkontrolovat doklady",
        "Tiskový přehled",
        "Export dat do zálohy",
        "Obnovit data ze zálohy",
        "Zálohovat ihned",
        "Otevřít složku automatických záloh",
        "Nastavení",
        "Export termínů do kalendáře",
        "Načíst data znovu",
        "Otevřít datovou složku",
        "O programu",
        "Zkontrolovat aktualizace",
        "Ukončit aplikaci",
        "Zavřít");
}
