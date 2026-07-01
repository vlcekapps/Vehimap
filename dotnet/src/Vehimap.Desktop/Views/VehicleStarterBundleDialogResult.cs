// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Desktop.Views;

public sealed record VehicleStarterBundleDialogResult(IReadOnlyList<VehicleStarterBundleTemplate> SelectedItems);
