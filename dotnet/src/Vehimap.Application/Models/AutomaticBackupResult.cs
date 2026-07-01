// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record AutomaticBackupResult(
    bool Created,
    bool IsError,
    string BackupPath,
    string Message);
