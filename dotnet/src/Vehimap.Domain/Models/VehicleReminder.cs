// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Domain.Models;

public sealed record VehicleReminder(
    string Id,
    string VehicleId,
    string Title,
    string DueDate,
    string ReminderDays,
    string RepeatMode,
    string Note);
