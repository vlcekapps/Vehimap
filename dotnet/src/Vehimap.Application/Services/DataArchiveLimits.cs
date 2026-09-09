// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Services;

public sealed record DataArchiveLimits
{
    public static DataArchiveLimits Default { get; } = new();
    public long ArchiveBytes { get; init; } = 512L * 1024 * 1024;
    public long ExpandedBytes { get; init; } = 512L * 1024 * 1024;
    public long EntryBytes { get; init; } = 256L * 1024 * 1024;
    public long AttachmentBytes { get; init; } = 256L * 1024 * 1024;
    public long JsonBytes { get; init; } = 16L * 1024 * 1024;
    public long ManifestBytes { get; init; } = 64L * 1024;
    public int Entries { get; init; } = 10_000;
    public const long LegacyTextBytes = 32L * 1024 * 1024;
}

public sealed class DataArchiveLimitException : IOException
{
    public DataArchiveLimitException() : base("The data archive exceeds the supported safety limits.") { }
}
