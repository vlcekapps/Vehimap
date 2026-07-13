// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileVehicleListItemViewModel
{
    public MobileVehicleListItemViewModel(Vehicle vehicle, VehicleMeta? meta, IAppLocalizer localizer)
    {
        Id = vehicle.Id;
        Name = ValueOrFallback(vehicle.Name, localizer.GetString("Mobile.Value.UnnamedVehicle"));
        MakeModel = ValueOrFallback(vehicle.MakeModel, localizer.GetString("Projection.Value.NoMakeModel"));
        Plate = ValueOrFallback(vehicle.Plate, localizer.GetString("Projection.Value.NoPlate"));
        Category = ValueOrFallback(
            LegacyKnownValueDisplayService.FormatCategory(vehicle.Category, localizer),
            localizer.GetString("Mobile.Value.NoCategory"));
        State = ValueOrFallback(
            LegacyKnownValueDisplayService.FormatVehicleState(meta?.State, localizer),
            localizer.GetString("Projection.Value.NormalOperation"));
        Year = ValueOrFallback(vehicle.Year, localizer.GetString("Mobile.Value.NotEntered"));
        Power = ValueOrFallback(vehicle.Power, localizer.GetString("Mobile.Value.NotEntered"));
        NextTechnicalInspection = ValueOrFallback(vehicle.NextTk, localizer.GetString("Mobile.Value.NotEntered"));
        GreenCardTo = ValueOrFallback(vehicle.GreenCardTo, localizer.GetString("Mobile.Value.NotEntered"));
        Note = ValueOrFallback(vehicle.VehicleNote, localizer.GetString("Mobile.Value.NoNote"));
        ItemType = localizer.GetString("Mobile.VehicleList.ItemType");
        AutomationId = $"MobileVehicle_{SanitizeAutomationId(vehicle.Id)}";
        AccessibleLabel = localizer.Format(
            "Mobile.VehicleList.ItemAccessibleLabel",
            Name,
            MakeModel,
            Category,
            Plate,
            State);
    }

    public string Id { get; }

    public string Name { get; }

    public string MakeModel { get; }

    public string Plate { get; }

    public string Category { get; }

    public string State { get; }

    public string Year { get; }

    public string Power { get; }

    public string NextTechnicalInspection { get; }

    public string GreenCardTo { get; }

    public string Note { get; }

    public string ItemType { get; }

    public string AutomationId { get; }

    public string AccessibleLabel { get; }

    private static string ValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string SanitizeAutomationId(string value)
    {
        var characters = value.Where(character => char.IsLetterOrDigit(character) || character == '_').ToArray();
        return characters.Length == 0 ? "Unknown" : new string(characters);
    }
}
