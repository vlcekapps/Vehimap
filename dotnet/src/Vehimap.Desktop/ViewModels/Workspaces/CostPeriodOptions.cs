// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels.Workspaces;

internal static class CostPeriodOptions
{
    public static LocalizedOptionViewModel YearToDate => Option(MainWindowViewModel.CostPeriodYearToDateKey);

    public static IReadOnlyList<LocalizedOptionViewModel> AllOptions =>
    [
        Option(MainWindowViewModel.CostPeriodYearToDateKey),
        Option(MainWindowViewModel.CostPeriodLast30DaysKey),
        Option(MainWindowViewModel.CostPeriodLast90DaysKey),
        Option(MainWindowViewModel.CostPeriodCurrentYearKey),
        Option(MainWindowViewModel.CostPeriodPreviousYearKey),
        Option(MainWindowViewModel.CostPeriodCustomKey)
    ];

    public static LocalizedOptionViewModel Option(string key) =>
        new(key, LabelForKey(key));

    public static string LabelForKey(string key) =>
        key switch
        {
            MainWindowViewModel.CostPeriodLast30DaysKey => DesktopLocalization.Localizer.GetString("CostPeriod.Last30Days"),
            MainWindowViewModel.CostPeriodLast90DaysKey => DesktopLocalization.Localizer.GetString("CostPeriod.Last90Days"),
            MainWindowViewModel.CostPeriodCurrentYearKey => DesktopLocalization.Localizer.GetString("CostPeriod.CurrentYear"),
            MainWindowViewModel.CostPeriodPreviousYearKey => DesktopLocalization.Localizer.GetString("CostPeriod.PreviousYear"),
            MainWindowViewModel.CostPeriodCustomKey => DesktopLocalization.Localizer.GetString("CostPeriod.Custom"),
            _ => DesktopLocalization.Localizer.GetString("CostPeriod.YearToDate")
        };
}
