// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleTimelineItemViewModel(
    string Kind,
    string KindLabel,
    string Date,
    string Title,
    string Detail,
    string Status,
    string VehicleName,
    string VehicleId,
    string EntryId,
    bool IsFuture,
    string Note)
{
    public string AccessibleLabel =>
        DesktopLocalization.Localizer.Format("TimelineItem.AccessibleLabel", VehicleName, Date, KindLabel, Title, Status, Detail);

    public override string ToString() => AccessibleLabel;
}
