// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels.Workspaces;

internal static class TimelineFilterOptions
{
    public const string AllKey = "all";
    public const string FutureKey = "future";
    public const string PastKey = "past";

    public static LocalizedOptionViewModel All => Option(AllKey);

    public static IReadOnlyList<LocalizedOptionViewModel> AllOptions =>
    [
        Option(AllKey),
        Option(FutureKey),
        Option(PastKey)
    ];

    public static LocalizedOptionViewModel Option(string key) =>
        new(key, LabelForKey(key));

    public static string LabelForKey(string key) =>
        key switch
        {
            FutureKey => DesktopLocalization.Localizer.GetString("TimelineWorkspace.Filter.Future"),
            PastKey => DesktopLocalization.Localizer.GetString("TimelineWorkspace.Filter.Past"),
            _ => DesktopLocalization.Localizer.GetString("TimelineWorkspace.Filter.All")
        };
}
