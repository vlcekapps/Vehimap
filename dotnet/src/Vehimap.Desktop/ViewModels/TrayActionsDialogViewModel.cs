namespace Vehimap.Desktop.ViewModels;

public sealed record TrayActionsDialogViewModel(
    string Title,
    string Description,
    string ShowMainWindowLabel,
    string ShowDashboardLabel,
    string ExitLabel,
    string CancelLabel)
{
    public static TrayActionsDialogViewModel CreateDefault() => new(
        "Akce Vehimapu na liště",
        "Vyberte akci pro běžící Vehimap. Toto okno nahrazuje nativní menu lišty pro přístupnější ovládání přes klávesnici a čtečku obrazovky.",
        "Zobrazit Vehimap",
        "Otevřít Dashboard",
        "Ukončit aplikaci",
        "Zavřít");
}
