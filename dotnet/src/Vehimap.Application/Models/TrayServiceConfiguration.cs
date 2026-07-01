// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record TrayServiceConfiguration(
    string ToolTipText,
    Func<Task> OpenTrayActionsAsync,
    Func<Task> ShowMainWindowAsync,
    Func<Task> ShowDashboardAsync,
    Func<Task> ExitApplicationAsync);
