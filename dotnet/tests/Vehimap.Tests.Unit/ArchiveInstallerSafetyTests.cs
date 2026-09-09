// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Updater;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class ArchiveInstallerSafetyTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "vehimap-updater-test-" + Guid.NewGuid().ToString("N"));
    private string Source => Path.Combine(_root, "source");
    private string Target => Path.Combine(_root, "target");

    public ArchiveInstallerSafetyTests() { Directory.CreateDirectory(Source); Directory.CreateDirectory(Target); }

    [Fact]
    public void Running_application_must_not_be_treated_as_exited() =>
        Assert.False(ArchiveInstaller.WaitForApplicationExit(Environment.ProcessId, timeoutMilliseconds: 1));

    [Fact]
    public void Copy_preserves_only_data_directory_not_similarly_named_binaries()
    {
        Write(Source, "data/vehimap.db", "incoming database");
        Write(Target, "data/vehimap.db", "user data");
        Write(Source, "database.dll", "new binary");
        Write(Source, "databases/catalog.txt", "new catalog");
        ArchiveInstaller.Install(Source, Target);
        Assert.Equal("user data", File.ReadAllText(Path.Combine(Target, "data/vehimap.db")));
        Assert.Equal("new binary", File.ReadAllText(Path.Combine(Target, "database.dll")));
        Assert.Equal("new catalog", File.ReadAllText(Path.Combine(Target, "databases/catalog.txt")));
    }

    [Fact]
    public void Overlapping_directories_and_external_entry_are_rejected_before_copy()
    {
        Assert.Throws<ArgumentException>(() => ArchiveInstaller.Install(Source, Source));
        Assert.Throws<ArgumentException>(() => ArchiveInstaller.Install(Source, Path.Combine(Source, "nested")));
        Assert.Throws<ArgumentException>(() => ArchiveInstaller.ValidatePaths(Source, Target, Path.Combine(_root, "outside.exe")));
        Assert.Empty(Directory.EnumerateFileSystemEntries(Target));
    }

    [Fact]
    public void Failed_copy_restores_previously_replaced_files()
    {
        // Installation is ordinal: the conflict must occur after both an overwrite and an addition.
        Write(Source, "01-first.dll", "new");
        Write(Target, "01-first.dll", "original");
        Write(Source, "02-added.dll", "added");
        Write(Source, "03-blocked.dll", "new");
        Directory.CreateDirectory(Path.Combine(Target, "03-blocked.dll"));
        Assert.ThrowsAny<IOException>(() => ArchiveInstaller.Install(Source, Target));
        Assert.Equal("original", File.ReadAllText(Path.Combine(Target, "01-first.dll")));
        Assert.False(File.Exists(Path.Combine(Target, "02-added.dll")));
        Assert.True(Directory.Exists(Path.Combine(Target, "03-blocked.dll")));
    }

    private static void Write(string root, string relative, string contents)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);
}
