// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileAlertItemViewModel
{
    public MobileAlertItemViewModel(SmartAdvisorItem item, IAppLocalizer localizer)
    {
        Id = item.Id;
        VehicleId = item.VehicleId;
        VehicleName = item.VehicleName;
        EntityKind = item.EntityKind;
        EntityId = item.EntityId;
        Title = item.Title;
        Summary = item.Summary;
        Detail = item.Detail;
        Priority = localizer.GetString($"Mobile.Alerts.Priority.{item.Priority}");
        ItemType = localizer.GetString("Mobile.Alerts.ItemType");
        AccessibleLabel = localizer.Format(
            "Mobile.Alerts.ItemAccessibleLabel",
            Priority,
            VehicleName,
            Title,
            Summary);
        AutomationId = $"MobileAlert_{SanitizeAutomationId(item.Id)}";
    }

    public string Id { get; }

    public string VehicleId { get; }

    public string VehicleName { get; }

    public string EntityKind { get; }

    public string EntityId { get; }

    public string Title { get; }

    public string Summary { get; }

    public string Detail { get; }

    public string Priority { get; }

    public string ItemType { get; }

    public string AccessibleLabel { get; }

    public string AutomationId { get; }

    private static string SanitizeAutomationId(string value)
    {
        var characters = value.Where(character => char.IsLetterOrDigit(character) || character == '_').ToArray();
        return characters.Length == 0 ? "Unknown" : new string(characters);
    }
}
