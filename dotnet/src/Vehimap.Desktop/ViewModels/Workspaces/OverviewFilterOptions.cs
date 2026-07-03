// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels.Workspaces;

internal static class OverviewFilterOptions
{
    public const string AllKey = "all";
    public const string TechnicalKey = "technical";
    public const string GreenCardsKey = "green_cards";
    public const string RemindersKey = "reminders";
    public const string RecordsKey = "records";
    public const string MaintenanceKey = "maintenance";
    public const string DataIssuesKey = "data_issues";

    public static LocalizedOptionViewModel All => Option(AllKey);

    public static IReadOnlyList<LocalizedOptionViewModel> AllOptions =>
    [
        Option(AllKey),
        Option(TechnicalKey),
        Option(GreenCardsKey),
        Option(RemindersKey),
        Option(RecordsKey),
        Option(MaintenanceKey),
        Option(DataIssuesKey)
    ];

    public static IReadOnlyList<LocalizedOptionViewModel> DueDateOptions =>
    [
        Option(AllKey),
        Option(TechnicalKey),
        Option(GreenCardsKey),
        Option(RemindersKey),
        Option(RecordsKey),
        Option(MaintenanceKey)
    ];

    public static LocalizedOptionViewModel Option(string key) =>
        new(key, LabelForKey(key));

    public static string LabelForKey(string key) =>
        key switch
        {
            TechnicalKey => DesktopLocalization.Localizer.GetString("Overview.Filter.Technical"),
            GreenCardsKey => DesktopLocalization.Localizer.GetString("Overview.Filter.GreenCards"),
            RemindersKey => DesktopLocalization.Localizer.GetString("Overview.Filter.Reminders"),
            RecordsKey => DesktopLocalization.Localizer.GetString("Overview.Filter.Records"),
            MaintenanceKey => DesktopLocalization.Localizer.GetString("Overview.Filter.Maintenance"),
            DataIssuesKey => DesktopLocalization.Localizer.GetString("Overview.Filter.DataIssues"),
            _ => DesktopLocalization.Localizer.GetString("Overview.Filter.All")
        };
}
