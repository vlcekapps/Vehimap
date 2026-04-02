using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IUpdateService
{
    Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default);
    Task<UpdateInstallResult> PrepareInstallAsync(UpdateCheckResult update, CancellationToken cancellationToken = default);
}
