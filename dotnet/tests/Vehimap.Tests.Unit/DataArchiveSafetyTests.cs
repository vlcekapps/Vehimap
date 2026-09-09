// SPDX-License-Identifier: GPL-3.0-or-later
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Compression;
using Vehimap.Application.Services;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DataArchiveSafetyTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"vehimap-archive-safety-{Guid.NewGuid():N}");
    private string Archive => Path.Combine(_root, "test.zip");
    private string Output => Path.Combine(_root, "out");

    public DataArchiveSafetyTests() => Directory.CreateDirectory(_root);
    public void Dispose() => Directory.Delete(_root, true);

    [Theory]
    [InlineData("entry")]
    [InlineData("total")]
    [InlineData("attachments")]
    [InlineData("json")]
    [InlineData("manifest")]
    [InlineData("count")]
    [InlineData("compressed")]
    public async Task Rejects_each_budget_before_extracting_any_file(string budget)
    {
        CreateArchive(("manifest.json", new byte[16]), ("attachments/a.txt", new byte[1024]), ("attachments/b.txt", new byte[1024]));
        var limits = budget switch
        {
            "entry" => DataArchiveLimits.Default with { EntryBytes = 1023 },
            "total" => DataArchiveLimits.Default with { ExpandedBytes = 2048 },
            "attachments" => DataArchiveLimits.Default with { AttachmentBytes = 2047 },
            "json" => DataArchiveLimits.Default with { JsonBytes = 15 },
            "manifest" => DataArchiveLimits.Default with { ManifestBytes = 15 },
            "count" => DataArchiveLimits.Default with { Entries = 2 },
            _ => DataArchiveLimits.Default with { ArchiveBytes = new FileInfo(Archive).Length - 1 }
        };
        await Assert.ThrowsAsync<DataArchiveLimitException>(() => SafeDataArchive.ExtractAsync(Archive, Output, limits: limits));
        Assert.False(Directory.Exists(Output));
    }

    [Fact]
    public async Task Highly_compressible_content_within_budgets_is_allowed()
    {
        var bytes = new byte[100_000];
        CreateArchive(("attachments/a.txt", bytes));
        await SafeDataArchive.ExtractAsync(Archive, Output, limits: DataArchiveLimits.Default with
            { ExpandedBytes = bytes.Length, EntryBytes = bytes.Length, AttachmentBytes = bytes.Length });
        Assert.Equal(bytes, await File.ReadAllBytesAsync(Path.Combine(Output, "attachments", "a.txt")));
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("data/attachments/a.txt")]
    [InlineData("./attachments/a.txt")]
    [InlineData("attachments/../a.txt")]
    [InlineData("/absolute.txt")]
    [InlineData("C:\\absolute.txt")]
    [InlineData("attachments/a.txt.")]
    [InlineData("attachments//a.txt")]
    public async Task Rejects_unsafe_or_alias_paths_before_extracting_valid_entries(string path)
    {
        CreateArchive(("valid.txt", [1]), (path, [2]));
        await Assert.ThrowsAsync<InvalidDataException>(() => SafeDataArchive.ExtractAsync(Archive, Output));
        Assert.False(Directory.Exists(Output));
        Assert.False(File.Exists(Path.Combine(_root, "outside.txt")));
    }

    [Theory]
    [InlineData("a.txt", "A.txt")]
    [InlineData("a", "a/b.txt")]
    [InlineData("a/b.txt", "a")]
    [InlineData("a.txt", "a.txt")]
    public async Task Rejects_case_duplicate_and_file_directory_conflicts(string first, string second)
    {
        CreateArchive((first, [1]), (second, [2]));
        await Assert.ThrowsAsync<InvalidDataException>(() => SafeDataArchive.ExtractAsync(Archive, Output));
        Assert.False(Directory.Exists(Output));
    }

    [Fact]
    public async Task Rejects_archive_symlinks_before_extraction()
    {
        using (var zip = ZipFile.Open(Archive, ZipArchiveMode.Create))
            zip.CreateEntry("link").ExternalAttributes = 0xa1ff << 16;
        await Assert.ThrowsAsync<InvalidDataException>(() => SafeDataArchive.ExtractAsync(Archive, Output));
        Assert.False(Directory.Exists(Output));
    }

    [Fact]
    public async Task Lying_directory_count_does_not_bypass_preflight_entry_limit()
    {
        CreateArchive(("one", []), ("two", []), ("three", []));
        var bytes = File.ReadAllBytes(Archive);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(bytes.Length - 22 + 8), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(bytes.Length - 22 + 10), 1);
        File.WriteAllBytes(Archive, bytes);
        await Assert.ThrowsAsync<DataArchiveLimitException>(() => SafeDataArchive.ExtractAsync(Archive, Output,
            limits: DataArchiveLimits.Default with { Entries = 2 }));
        Assert.False(Directory.Exists(Output));
    }

    [Fact]
    public async Task Actual_stream_bytes_are_bounded_even_when_metadata_is_untrusted()
    {
        using var input = new MemoryStream(new byte[100]);
        using var output = new MemoryStream();
        await Assert.ThrowsAsync<DataArchiveLimitException>(() => SafeDataArchive.CopyBoundedAsync(input, output, 99, default));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public async Task Cancellation_and_nonempty_destination_do_not_overwrite_files()
    {
        CreateArchive(("original.txt", [2]));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SafeDataArchive.ExtractAsync(Archive, Output, new(true)));
        Assert.False(Directory.Exists(Output));
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "original.txt"), "original");
        await Assert.ThrowsAsync<InvalidOperationException>(() => SafeDataArchive.ExtractAsync(Archive, Output));
        Assert.Equal("original", File.ReadAllText(Path.Combine(Output, "original.txt")));
    }

    [Fact]
    public void Failed_export_validation_preserves_previous_archive()
    {
        var source = Path.Combine(_root, "source");
        Directory.CreateDirectory(source);
        File.WriteAllBytes(Path.Combine(source, "manifest.json"), new byte[DataArchiveLimits.Default.ManifestBytes + 1]);
        File.WriteAllText(Archive, "previous backup");
        Assert.Throws<DataArchiveLimitException>(() => AtomicArchiveWriter.CreateFromDirectory(source, Archive));
        Assert.Equal("previous backup", File.ReadAllText(Archive));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
    }

    [Theory]
    [InlineData("en-US", "safe size")]
    [InlineData("cs-CZ", "bezpečný limit")]
    public void Size_error_is_localized_even_when_wrapped(string culture, string expected)
    {
        var error = new IOException("internal path", new DataArchiveLimitException());
        var message = UserFacingExceptionMessageService.Describe(error, new ResourceAppLocalizer(CultureInfo.GetCultureInfo(culture)));
        Assert.Contains(expected, message);
        Assert.DoesNotContain("internal path", message);
    }

    private void CreateArchive(params (string Name, byte[] Bytes)[] files)
    {
        using var archive = ZipFile.Open(Archive, ZipArchiveMode.Create);
        foreach (var (name, bytes) in files)
        {
            using var stream = archive.CreateEntry(name, CompressionLevel.Optimal).Open();
            stream.Write(bytes);
        }
    }
}
