namespace Vehimap.Application.Models;

public sealed record UpdateInstallResult(
    bool IsReady,
    string Message,
    UpdateInstallPlan? InstallPlan);
