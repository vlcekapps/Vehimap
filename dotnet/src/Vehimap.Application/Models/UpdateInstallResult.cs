// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record UpdateInstallResult(
    bool IsReady,
    string Message,
    UpdateInstallPlan? InstallPlan);
