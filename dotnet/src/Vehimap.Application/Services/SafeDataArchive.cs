// SPDX-License-Identifier: GPL-3.0-or-later
using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Vehimap.Application.Services;

/// <summary>Bounded extraction into a new, application-owned staging directory, never a live data root.</summary>
public static class SafeDataArchive
{
    public static async Task ExtractAsync(string archivePath, string directory, CancellationToken cancellationToken = default,
        DataArchiveLimits? limits = null)
    {
        limits ??= DataArchiveLimits.Default;
        using var stream = File.OpenRead(archivePath);
        PreflightDirectory(stream, limits, cancellationToken);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var entries = ValidateEntries(archive, directory, limits, cancellationToken);
        if (Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any())
            throw new InvalidOperationException("Archive extraction requires an empty staging directory.");
        Directory.CreateDirectory(directory);
        foreach (var (entry, target, isDirectory) in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (isDirectory) { Directory.CreateDirectory(target); continue; }
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            using var input = entry.Open();
            using var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write);
            await CopyBoundedAsync(input, output, entry.Length, cancellationToken).ConfigureAwait(false);
            if (output.Length != entry.Length) throw new InvalidDataException("Archive entry length is inconsistent.");
        }
    }

    public static void Validate(string archivePath, CancellationToken cancellationToken = default, DataArchiveLimits? limits = null)
    {
        limits ??= DataArchiveLimits.Default;
        using var stream = File.OpenRead(archivePath);
        PreflightDirectory(stream, limits, cancellationToken);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        // No files are created; use a unique root to reuse exactly the import path policy.
        ValidateEntries(archive, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")), limits, cancellationToken);
    }

    public static async Task CopyBoundedAsync(Stream input, Stream output, long maximumBytes, CancellationToken cancellationToken)
    {
        var buffer = new byte[81920];
        long copied = 0;
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
        {
            if (read > maximumBytes - copied) throw new DataArchiveLimitException();
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            copied += read;
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    public static async Task<string> ReadLegacyTextAsync(string path, CancellationToken cancellationToken)
    {
        using var input = File.OpenRead(path);
        if (input.Length > DataArchiveLimits.LegacyTextBytes) throw new DataArchiveLimitException();
        using var output = new MemoryStream();
        await CopyBoundedAsync(input, output, DataArchiveLimits.LegacyTextBytes, cancellationToken).ConfigureAwait(false);
        output.Position = 0;
        using var reader = new StreamReader(output, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    public static bool HasLegacyBackupHeader(string path)
    {
        using var input = File.OpenRead(path);
        Span<byte> prefix = stackalloc byte[64];
        var count = input.ReadAtLeast(prefix, prefix.Length, throwOnEndOfStream: false);
        var text = Encoding.UTF8.GetString(prefix[..count]).TrimStart('\uFEFF');
        var end = text.IndexOfAny(['\r', '\n']);
        if (end < 0) return false;
        var header = text[..end].Trim();
        return header is "# Vehimap backup v1" or "# Vehimap backup v2" or "# Vehimap backup v3"
            or "# Vehimap backup v4" or "# Vehimap backup v5" or "# Vehimap backup v6";
    }

    private static List<(ZipArchiveEntry Entry, string Target, bool Directory)> ValidateEntries(
        ZipArchive archive, string directory, DataArchiveLimits limits, CancellationToken cancellationToken)
    {
        if (archive.Entries.Count > limits.Entries) throw new DataArchiveLimitException();
        var result = new List<(ZipArchiveEntry, string, bool)>();
        var paths = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        long total = 0, attachments = 0;
        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = entry.FullName.Replace('\\', '/');
            var isDirectory = name.EndsWith('/');
            var relative = isDirectory ? name[..^1] : name;
            var segments = relative.Split('/');
            if (name.Length > 1024 || segments.Length > 32) throw new DataArchiveLimitException();
            if (segments.Any(segment => segment.Length == 0 || segment is "." or "..")
                || relative != relative.Trim()
                || ManagedAttachmentPathGuard.NormalizeRelativePathInsideRoot(relative) != relative
                || !paths.TryAdd(relative, isDirectory)
                || ((entry.ExternalAttributes >> 16) & 0xf000) == 0xa000
                || (entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Archive contains an unsafe or duplicate path.");
            var target = ManagedAttachmentPathGuard.ResolveRelativePathInsideRoot(directory, relative);
            var limit = limits.EntryBytes;
            if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) limit = Math.Min(limit, limits.JsonBytes);
            if (name.Equals("manifest.json", StringComparison.OrdinalIgnoreCase)
                || name.Equals("manifest.ini", StringComparison.OrdinalIgnoreCase)) limit = Math.Min(limit, limits.ManifestBytes);
            if (entry.Length > limit || entry.Length > limits.ExpandedBytes - total) throw new DataArchiveLimitException();
            total += entry.Length;
            if (name.StartsWith("attachments/", StringComparison.OrdinalIgnoreCase))
            {
                if (entry.Length > limits.AttachmentBytes - attachments) throw new DataArchiveLimitException();
                attachments += entry.Length;
            }
            if (isDirectory && entry.Length != 0) throw new InvalidDataException("Archive directory contains file data.");
            result.Add((entry, target, isDirectory));
        }
        foreach (var path in paths.Keys)
        {
            for (var index = path.IndexOf('/'); index >= 0; index = path.IndexOf('/', index + 1))
                if (paths.TryGetValue(path[..index], out var parentIsDirectory) && !parentIsDirectory)
                    throw new InvalidDataException("Archive file conflicts with a directory.");
        }
        return result;
    }

    private static void PreflightDirectory(Stream stream, DataArchiveLimits limits, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (stream.Length > limits.ArchiveBytes) throw new DataArchiveLimitException();
        // Bound central-directory parsing before ZipArchive materializes entry objects. Our
        // formats are well below classic ZIP limits; split archives and ZIP64 are not accepted.
        var tail = new byte[(int)Math.Min(stream.Length, 65535 + 22)];
        stream.Position = stream.Length - tail.Length;
        stream.ReadExactly(tail);
        var end = -1;
        for (var i = tail.Length - 22; i >= 0; i--)
            if (BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(i)) == 0x06054b50
                && i + 22 + BinaryPrimitives.ReadUInt16LittleEndian(tail.AsSpan(i + 20)) == tail.Length)
            { end = i; break; }
        if (end < 0) throw new InvalidDataException("Archive has no valid central directory.");
        var eocd = tail.AsSpan(end);
        var count = BinaryPrimitives.ReadUInt16LittleEndian(eocd[10..]);
        var size = BinaryPrimitives.ReadUInt32LittleEndian(eocd[12..]);
        var offset = BinaryPrimitives.ReadUInt32LittleEndian(eocd[16..]);
        var endPosition = stream.Length - tail.Length + end;
        if (count > limits.Entries || size > 8 * 1024 * 1024) throw new DataArchiveLimitException();
        if (BinaryPrimitives.ReadUInt32LittleEndian(eocd[4..]) != 0
            || BinaryPrimitives.ReadUInt16LittleEndian(eocd[8..]) != count
            || (long)offset + size != endPosition)
            throw new InvalidDataException("Split or ZIP64 archives are not supported.");
        stream.Position = offset;
        Span<byte> header = stackalloc byte[46];
        var actualCount = 0;
        while (stream.Position < endPosition)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (++actualCount > limits.Entries) throw new DataArchiveLimitException();
            if (endPosition - stream.Position < header.Length) throw new InvalidDataException("Truncated central directory.");
            stream.ReadExactly(header);
            if (BinaryPrimitives.ReadUInt32LittleEndian(header) != 0x02014b50)
                throw new InvalidDataException("Invalid central directory entry.");
            var nameLength = BinaryPrimitives.ReadUInt16LittleEndian(header[28..]);
            if (nameLength > 4096) throw new DataArchiveLimitException();
            stream.Position += nameLength + BinaryPrimitives.ReadUInt16LittleEndian(header[30..])
                + BinaryPrimitives.ReadUInt16LittleEndian(header[32..]);
            if (stream.Position > endPosition) throw new InvalidDataException("Truncated central directory entry.");
        }
        if (actualCount != count) throw new InvalidDataException("Archive entry count is inconsistent.");
        stream.Position = 0;
    }
}
