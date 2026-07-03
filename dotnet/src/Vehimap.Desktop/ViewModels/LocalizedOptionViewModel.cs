// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.ViewModels;

public sealed record LocalizedOptionViewModel(string Value, string Label)
{
    public string AccessibleLabel => Label;

    public override string ToString() => Label;
}
