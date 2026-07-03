// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record SmartAdvisorItemViewModel(
    string Id,
    string Priority,
    string Category,
    string VehicleName,
    string VehicleId,
    string EntityKind,
    string EntityId,
    string Title,
    string Summary,
    string Detail,
    string ActionLabel,
    string DueDate,
    int PriorityRank)
{
    public string AccessibleLabel =>
        DesktopLocalization.Localizer.Format("SmartAdvisorItem.AccessibleLabel", Priority, Category, VehicleName, Title, Summary, Detail, ActionLabel);

    public override string ToString() => AccessibleLabel;
}

public sealed record DesktopSmartAdvisorProjection(
    string Summary,
    IReadOnlyList<SmartAdvisorItemViewModel> Items);
