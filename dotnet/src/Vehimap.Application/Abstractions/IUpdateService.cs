// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IUpdateService
{
    Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default);
    Task<UpdateInstallResult> PrepareInstallAsync(
        UpdateCheckResult update,
        IProgress<UpdateInstallProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
