using Vehimap.Application.Abstractions;

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
        var normalized = (path ?? string.Empty).Trim().Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        if (normalized.StartsWith("data/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[5..];
        }

        while (normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = normalized[1..];
        }

        return normalized;
    }

    public static string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath)
    {
        var normalized = NormalizeAttachmentRelativePath(relativePath);
        return string.IsNullOrWhiteSpace(normalized)
            ? string.Empty
            : Path.Combine(dataRoot.DataPath, normalized.Replace('/', Path.DirectorySeparatorChar));
    }
}
