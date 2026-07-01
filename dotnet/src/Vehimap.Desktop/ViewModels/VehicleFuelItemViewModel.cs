// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleFuelItemViewModel(
    string Id,
    string Date,
    string FuelType,
    string Liters,
    string TotalCost,
    string Odometer,
    string TankState,
    string FuelDetail,
    string Station,
    string Note)
{
    public string AccessibleLabel =>
        DesktopLocalization.Localizer.Format(
            "FuelItem.AccessibleLabel",
            Date,
            FuelType,
            FuelDetail,
            Station,
            Liters,
            TotalCost,
            Odometer,
            TankState,
            Note);

    public override string ToString() => AccessibleLabel;
}
