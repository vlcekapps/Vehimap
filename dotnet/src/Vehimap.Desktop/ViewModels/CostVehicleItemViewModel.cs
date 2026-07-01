// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.ViewModels;

public sealed record CostVehicleItemViewModel(
    string VehicleId,
    string VehicleName,
    string Category,
    string FuelCost,
    string HistoryCost,
    string RecordCost,
    string TotalCost,
    string Distance,
    string CostPerKm,
    string Status,
    string AccessibleLabel)
{
    public override string ToString() => AccessibleLabel;
}
