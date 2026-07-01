// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.ViewModels;

public sealed record ServiceBookItemViewModel(
    string VehicleId,
    string EntityKind,
    string EntityId,
    string Section,
    string Primary,
    string Secondary,
    string Detail,
    string Status)
{
    public string AccessibleLabel
    {
        get
        {
            var parts = new[]
            {
                Section,
                Primary,
                Secondary,
                Detail,
                Status
            };
            return string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }
    }

    public override string ToString() => AccessibleLabel;
}
