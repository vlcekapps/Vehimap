// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleListItemViewModel(
    string Id,
    string Name,
    string Category,
    string Plate,
    string MakeModel,
    string VehicleNote,
    string NextTk,
    string GreenCardTo,
    string State,
    string Powertrain,
    string StatusSummary)
{
    public string AccessibleLabel =>
        DesktopLocalization.Localizer.Format("VehicleListItem.AccessibleLabel", Name, MakeModel, Category, Plate, StateOrFallback, StatusSummary);

    private string StateOrFallback => string.IsNullOrWhiteSpace(State)
        ? DesktopLocalization.Localizer.GetString("VehicleListItem.NoState")
        : State;

    public override string ToString() => AccessibleLabel;
}
