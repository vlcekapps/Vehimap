namespace Vehimap.Desktop.ViewModels;

public sealed record TrayActionsDialogViewModel(
    string Title,
    string Description,
    string ShowMainWindowLabel,
    string ShowDashboardLabel,
    string ShowUpcomingOverviewLabel,
    string ShowOverdueOverviewLabel,
    string OpenNearestTechnicalLabel,
    string OpenNearestGreenCardLabel,
    string OpenNearestReminderLabel,
    string OpenNearestMaintenanceLabel,
    string OpenNearestRecordLabel,
    string ExitLabel,
    string CancelLabel)
{
    public static TrayActionsDialogViewModel CreateDefault() => new(
        "Akce Vehimapu na liště",
        "Vyberte akci pro běžící Vehimap. Toto okno nahrazuje nativní menu lišty pro přístupnější ovládání přes klávesnici a čtečku obrazovky.",
        "Zobrazit Vehimap",
        "Otevřít Dashboard",
        "Blížící se termíny",
        "Propadlé termíny",
        "Nejbližší TK",
        "Nejbližší ZK",
        "Nejbližší připomínka",
        "Nejbližší servis",
        "Nejbližší doklad",
        "Ukončit aplikaci",
        "Zavřít");
}
