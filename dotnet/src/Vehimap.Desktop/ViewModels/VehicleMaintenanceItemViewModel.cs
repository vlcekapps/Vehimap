// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleMaintenanceItemViewModel(
    string Id,
    string Title,
    string Interval,
    string LastService,
    string Status,
    string Note)
{
    public string AccessibleLabel =>
        DesktopLocalization.Localizer.Format("MaintenanceItem.AccessibleLabel", Title, Interval, LastService, Status, Note);

    public override string ToString() => AccessibleLabel;
}
