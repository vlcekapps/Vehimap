// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record BackupRestoreResult(
    string? PreRestoreBackupPath,
    int RestoredAttachmentCount);
