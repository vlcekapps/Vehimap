using Vehimap.Application.Models;

namespace Vehimap.Desktop.Views;

public sealed record VehicleStarterBundleDialogResult(IReadOnlyList<VehicleStarterBundleTemplate> SelectedItems);
