using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class RuntimeStorageWriteGuardTests
{
    private static readonly string[] LegacyFileNames =
    [
        "vehicles.tsv",
        "history.tsv",
        "fuel.tsv",
        "records.tsv",
        "vehicle_meta.tsv",
        "reminders.tsv",
        "maintenance.tsv",
        "settings.ini"
    ];

    [Fact]
    public void Desktop_runtime_defaults_do_not_fall_back_to_legacy_backup_writes()
    {
        var desktopSource = ReadSourceTree(Path.Combine("dotnet", "src", "Vehimap.Desktop"));
        var applicationSource = ReadSourceTree(Path.Combine("dotnet", "src", "Vehimap.Application"));

        Assert.DoesNotContain("new LegacyBackupService", desktopSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new LegacyBackupService", applicationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new LegacyVehimapDataStore", applicationSource, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(desktopSource, "new LegacyVehimapDataStore"));
    }

    [Fact]
    public void Runtime_layers_do_not_reference_live_legacy_file_names()
    {
        var desktopSource = ReadSourceTree(Path.Combine("dotnet", "src", "Vehimap.Desktop"));
        var applicationSource = ReadSourceTree(Path.Combine("dotnet", "src", "Vehimap.Application"));

        foreach (var fileName in LegacyFileNames)
        {
            Assert.DoesNotContain(fileName, desktopSource, StringComparison.Ordinal);
            Assert.DoesNotContain(fileName, applicationSource, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Sqlite_storage_keeps_legacy_backup_fallback_only_for_import_compatibility()
    {
        var sqliteSource = ReadSourceTree(Path.Combine("dotnet", "src", "Vehimap.Storage.Sqlite"));

        Assert.Equal(1, CountOccurrences(sqliteSource, "new LegacyBackupService"));
        Assert.DoesNotContain("new LegacyVehimapDataStore", sqliteSource, StringComparison.Ordinal);
    }

    private static string ReadSourceTree(string relativePath)
    {
        var root = FindRepositoryRoot();
        var directory = Path.Combine(root, relativePath);
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Zdrojová složka nebyla nalezena: {directory}");
        }

        return string.Join(
            "\n",
            Directory
                .EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)
                .Select(File.ReadAllText));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "dotnet", "src", "Vehimap.Desktop")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Kořen repozitáře Vehimap nebyl nalezen.");
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
