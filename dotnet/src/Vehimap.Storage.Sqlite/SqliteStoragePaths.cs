// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Storage.Sqlite;

internal static class SqliteStoragePaths
{
    public const string DatabaseFileName = "vehimap.db";
    public const string AttachmentsDirectoryName = "attachments";
    public const string MigrationBackupsDirectoryName = "migration-backups";
    public const string ImportBackupsDirectoryName = "import-backups";

    public static string GetDatabasePath(VehimapDataRoot dataRoot) =>
        Path.Combine(dataRoot.DataPath, DatabaseFileName);

    public static string GetAttachmentsPath(VehimapDataRoot dataRoot) =>
        Path.Combine(dataRoot.DataPath, AttachmentsDirectoryName);

    public static string NormalizeAttachmentRelativePath(string? path)
    {
        return ManagedAttachmentPathGuard.NormalizeAttachmentRelativePath(path);
    }

    public static string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath)
    {
        return ManagedAttachmentPathGuard.ResolveManagedAttachmentPath(dataRoot.DataPath, relativePath);
    }
}
