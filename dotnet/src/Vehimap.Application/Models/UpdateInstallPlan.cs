// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record UpdateInstallPlan(
    string UpdaterPath,
    string SourceDirectory,
    string TargetDirectory,
    string EntryPath,
    int ProcessId,
    string ExpectedVersion,
    string InstallKind = "archive",
    string? InstallerPath = null);
