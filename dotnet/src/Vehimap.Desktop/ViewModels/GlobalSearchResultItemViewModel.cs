// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record GlobalSearchResultItemViewModel(
    string VehicleId,
    string EntityKind,
    string EntityId,
    string VehicleName,
    string SectionLabel,
    string Title,
    string Summary,
    string? VehicleLabel = null)
{
    private string EffectiveVehicleLabel => string.IsNullOrWhiteSpace(VehicleLabel)
        ? DesktopLocalization.Localizer.GetString("GlobalSearch.Accessible.VehicleLabel")
        : VehicleLabel;

    public string AccessibleLabel =>
        $"{SectionLabel}, {Title}, {EffectiveVehicleLabel} {VehicleName}, {Summary}".Trim().TrimEnd(',');

    public override string ToString() => AccessibleLabel;
}
