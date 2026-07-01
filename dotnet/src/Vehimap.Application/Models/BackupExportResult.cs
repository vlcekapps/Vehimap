// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record BackupExportResult(
    string BackupPath,
    int IncludedManagedAttachmentCount,
    int MissingManagedAttachmentCount);
