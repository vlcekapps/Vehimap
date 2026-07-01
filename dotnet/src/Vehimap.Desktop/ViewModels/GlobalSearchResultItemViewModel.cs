// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.ViewModels;

public sealed record GlobalSearchResultItemViewModel(
    string VehicleId,
    string EntityKind,
    string EntityId,
    string VehicleName,
    string SectionLabel,
    string Title,
    string Summary,
    string VehicleLabel = "vozidlo")
{
    public string AccessibleLabel =>
        $"{SectionLabel}, {Title}, {VehicleLabel} {VehicleName}, {Summary}".Trim().TrimEnd(',');

    public override string ToString() => AccessibleLabel;
}
