namespace Vehimap.Application.Models;

public sealed record TrayServiceConfiguration(
    string ToolTipText,
    Func<Task> ShowMainWindowAsync,
    Func<Task> ShowDashboardAsync,
    Func<Task> ExitApplicationAsync);
