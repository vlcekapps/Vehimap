// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;

namespace Vehimap.Application.Services;

public static class ManagedAttachmentPathGuard
{
    public const string AttachmentsDirectoryName = "attachments";

    public static string NormalizeAttachmentRelativePath(string? path)
    {
        var normalized = NormalizeRelativePath(path, requireAttachmentsPrefix: true);
        return normalized;
    }

    public static string NormalizeRelativePathInsideRoot(string? path) =>
        NormalizeRelativePath(path, requireAttachmentsPrefix: false);

    public static string ResolveManagedAttachmentPath(string dataPath, string relativePath)
    {
        var normalized = NormalizeAttachmentRelativePath(relativePath);
        return string.IsNullOrWhiteSpace(normalized)
            ? string.Empty
            : ResolveSafeRelativePath(dataPath, normalized);
    }

    public static string ResolveRelativePathInsideRoot(string rootPath, string relativePath)
    {
        var normalized = NormalizeRelativePathInsideRoot(relativePath);
        return string.IsNullOrWhiteSpace(normalized)
            ? string.Empty
            : ResolveSafeRelativePath(rootPath, normalized);
    }

    private static string NormalizeRelativePath(string? path, bool requireAttachmentsPrefix)
    {
        var normalized = (path ?? string.Empty).Trim().Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (IsRootedOrUnc(normalized))
        {
            throw new InvalidDataException("Managed attachment path must be relative.");
        }

        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        if (normalized.StartsWith("data/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[5..];
        }

        if (string.IsNullOrWhiteSpace(normalized) || IsRootedOrUnc(normalized))
        {
            throw new InvalidDataException("Managed attachment path must be relative.");
        }

        var segments = normalized.Split('/');
        if (segments.Any(IsInvalidSegment))
        {
            throw new InvalidDataException("Managed attachment path contains an invalid segment.");
        }

        normalized = string.Join('/', segments);
        if (requireAttachmentsPrefix && !normalized.StartsWith(AttachmentsDirectoryName + "/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Managed attachment path must be stored under attachments/.");
        }

        return normalized;
    }

    private static string ResolveSafeRelativePath(string rootPath, string normalizedRelativePath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new InvalidDataException("Target root path is missing.");
        }

        var rootFullPath = EnsureTrailingSeparator(Path.GetFullPath(rootPath));
        var candidatePath = Path.GetFullPath(Path.Combine(
            rootFullPath,
            normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar)));

        if (!candidatePath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Resolved managed attachment path escapes the target root.");
        }

        return candidatePath;
    }

    private static bool IsRootedOrUnc(string path) =>
        path.StartsWith("/", StringComparison.Ordinal)
        || path.StartsWith("//", StringComparison.Ordinal)
        || (path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':');

    private static bool IsInvalidSegment(string segment) =>
        string.IsNullOrWhiteSpace(segment)
        || string.Equals(segment, ".", StringComparison.Ordinal)
        || string.Equals(segment, "..", StringComparison.Ordinal)
        || segment.Any(IsInvalidFileNameCharacter);

    private static bool IsInvalidFileNameCharacter(char value) =>
        char.GetUnicodeCategory(value) == UnicodeCategory.Control
        || value is '<' or '>' or ':' or '"' or '|' or '?' or '*';

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar)
        || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}
