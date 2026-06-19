using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopReleasePackagingScriptTests : IDisposable
{
    private readonly string _tempRoot;

    public DesktopReleasePackagingScriptTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-dotnet-release-packaging", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public async Task Package_script_creates_archive_metadata_checksum_and_update_manifest()
    {
        var powerShell = ResolvePowerShell();
        if (powerShell is null)
        {
            return;
        }

        var publishDirectory = Path.Combine(_tempRoot, "publish");
        var outputDirectory = Path.Combine(_tempRoot, "release");
        Directory.CreateDirectory(publishDirectory);
        Directory.CreateDirectory(Path.Combine(publishDirectory, "locales"));
        await File.WriteAllTextAsync(Path.Combine(publishDirectory, "Vehimap.Desktop.exe"), "desktop binary");
        await File.WriteAllTextAsync(Path.Combine(publishDirectory, "Vehimap.Desktop.pdb"), "debug symbols");
        await File.WriteAllTextAsync(Path.Combine(publishDirectory, "locales", "cs.txt"), "cestina");

        const string version = "9.8.7-preview.1";
        var packageScript = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Package-DesktopRelease.ps1");
        var manifestScript = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Write-DotnetPreviewUpdateManifest.ps1");

        var packageResult = await RunPowerShellAsync(
            powerShell,
            packageScript,
            ("PublishDirectory", publishDirectory),
            ("RuntimeIdentifier", "win-x64"),
            ("Version", version),
            ("OutputDirectory", outputDirectory));

        Assert.Equal(0, packageResult.ExitCode);

        var packageName = $"vehimap-desktop-preview-{version}-win-x64.zip";
        var packagePath = Path.Combine(outputDirectory, packageName);
        var checksumPath = packagePath + ".sha256";
        var metadataPath = Path.Combine(outputDirectory, $"vehimap-desktop-preview-{version}-win-x64.json");

        Assert.True(File.Exists(packagePath), packageResult.CombinedOutput);
        Assert.True(File.Exists(checksumPath), packageResult.CombinedOutput);
        Assert.True(File.Exists(metadataPath), packageResult.CombinedOutput);

        using (var archive = ZipFile.OpenRead(packagePath))
        {
            Assert.Contains(archive.Entries, entry => string.Equals(entry.FullName, "Vehimap.Desktop.exe", StringComparison.Ordinal));
            Assert.Contains(archive.Entries, entry => string.Equals(entry.FullName, "locales/cs.txt", StringComparison.Ordinal) || string.Equals(entry.FullName, "locales\\cs.txt", StringComparison.Ordinal));
            Assert.DoesNotContain(archive.Entries, entry => entry.FullName.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase));
        }

        var expectedSha256 = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(packagePath))).ToLowerInvariant();
        var checksumLine = (await File.ReadAllTextAsync(checksumPath)).Trim();
        Assert.StartsWith(expectedSha256, checksumLine, StringComparison.Ordinal);
        Assert.Contains(packageName, checksumLine, StringComparison.Ordinal);

        using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(metadataPath));
        var root = metadata.RootElement;
        Assert.Equal(version, root.GetProperty("version").GetString());
        Assert.Equal("win-x64", root.GetProperty("runtimeIdentifier").GetString());
        Assert.Equal(packageName, root.GetProperty("packageFile").GetString());
        Assert.Equal(Path.GetFileName(checksumPath), root.GetProperty("checksumFile").GetString());
        Assert.Equal(expectedSha256, root.GetProperty("sha256").GetString());
        Assert.Equal(new FileInfo(packagePath).Length, root.GetProperty("packageSize").GetInt64());

        var updateManifestPath = Path.Combine(_tempRoot, "latest-dotnet-preview-win-x64.ini");
        var manifestResult = await RunPowerShellAsync(
            powerShell,
            manifestScript,
            ("PackageMetadataPath", metadataPath),
            ("ArtifactsDirectory", outputDirectory),
            ("ReleaseTag", $"dotnet-preview-v{version}"),
            ("OutputPath", updateManifestPath));

        Assert.Equal(0, manifestResult.ExitCode);

        var updateManifest = await File.ReadAllTextAsync(updateManifestPath);
        Assert.Contains($"version={version}", updateManifest, StringComparison.Ordinal);
        Assert.Contains($"asset_url=https://github.com/vlcekapps/Vehimap/releases/download/dotnet-preview-v{version}/{packageName}", updateManifest, StringComparison.Ordinal);
        Assert.Contains($"asset_sha256={expectedSha256}", updateManifest, StringComparison.Ordinal);
        Assert.Contains($"asset_size={new FileInfo(packagePath).Length}", updateManifest, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Update_manifest_script_rejects_checksum_that_does_not_match_package()
    {
        var powerShell = ResolvePowerShell();
        if (powerShell is null)
        {
            return;
        }

        var artifactsDirectory = Path.Combine(_tempRoot, "artifacts");
        Directory.CreateDirectory(artifactsDirectory);

        var packagePath = Path.Combine(artifactsDirectory, "vehimap-desktop-preview-9.8.7-win-x64.zip");
        await File.WriteAllTextAsync(packagePath, "real package bytes");
        var checksumPath = packagePath + ".sha256";
        await File.WriteAllTextAsync(checksumPath, $"{new string('a', 64)}  {Path.GetFileName(packagePath)}");

        var metadataPath = Path.Combine(artifactsDirectory, "vehimap-desktop-preview-9.8.7-win-x64.json");
        var metadata = new
        {
            version = "9.8.7",
            runtimeIdentifier = "win-x64",
            packageFile = Path.GetFileName(packagePath),
            checksumFile = Path.GetFileName(checksumPath),
            createdUtc = DateTime.UtcNow.ToString("o")
        };
        await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata));

        var manifestScript = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Write-DotnetPreviewUpdateManifest.ps1");
        var result = await RunPowerShellAsync(
            powerShell,
            manifestScript,
            ("PackageMetadataPath", metadataPath),
            ("ArtifactsDirectory", artifactsDirectory),
            ("ReleaseTag", "dotnet-preview-v9.8.7"),
            ("OutputPath", Path.Combine(_tempRoot, "latest.ini")));

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("neodpovida", result.CombinedOutput, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
    }

    private static async Task<ScriptResult> RunPowerShellAsync(
        string powerShellPath,
        string scriptPath,
        params (string Name, string Value)[] arguments)
    {
        var startInfo = new ProcessStartInfo(powerShellPath)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        if (OperatingSystem.IsWindows())
        {
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
        }

        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add("-" + argument.Name);
            startInfo.ArgumentList.Add(argument.Value);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Nepodarilo se spustit PowerShell: {powerShellPath}");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return new ScriptResult(process.ExitCode, stdout, stderr);
    }

    private static string? ResolvePowerShell()
    {
        foreach (var command in new[] { "pwsh", "powershell" })
        {
            var path = FindOnPath(command);
            if (path is not null)
            {
                return path;
            }
        }

        return null;
    }

    private static string? FindOnPath(string command)
    {
        string[] extensions = OperatingSystem.IsWindows()
            ? [".exe", ".cmd", ".bat", string.Empty]
            : [string.Empty];
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var extension in extensions)
            {
                var candidate = Path.Combine(directory, command + extension);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "src", "VERSION"))
                && File.Exists(Path.Combine(current.FullName, "dotnet", "build", "Package-DesktopRelease.ps1")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Nepodarilo se najit koren repozitare Vehimap.");
    }

    private sealed record ScriptResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public string CombinedOutput => StandardOutput + Environment.NewLine + StandardError;
    }
}
