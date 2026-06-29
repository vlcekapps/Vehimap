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

        if (OperatingSystem.IsWindows() && ResolveInnoCompiler() is null)
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
        var manifestScript = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Write-DotnetUpdateManifest.ps1");

        var packageResult = await RunPowerShellAsync(
            powerShell,
            packageScript,
            ("PublishDirectory", publishDirectory),
            ("RuntimeIdentifier", "win-x64"),
            ("Version", version),
            ("OutputDirectory", outputDirectory));

        Assert.Equal(0, packageResult.ExitCode);

        var packageName = $"vehimap-desktop-stable-{version}-win-x64-setup.exe";
        var packagePath = Path.Combine(outputDirectory, packageName);
        var checksumPath = packagePath + ".sha256";
        var metadataPath = Path.Combine(outputDirectory, $"vehimap-desktop-stable-{version}-win-x64-setup.json");

        Assert.True(File.Exists(packagePath), packageResult.CombinedOutput);
        Assert.True(File.Exists(checksumPath), packageResult.CombinedOutput);
        Assert.True(File.Exists(metadataPath), packageResult.CombinedOutput);

        var expectedSha256 = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(packagePath))).ToLowerInvariant();
        var checksumLine = (await File.ReadAllTextAsync(checksumPath)).Trim();
        Assert.StartsWith(expectedSha256, checksumLine, StringComparison.Ordinal);
        Assert.Contains(packageName, checksumLine, StringComparison.Ordinal);

        using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(metadataPath));
        var root = metadata.RootElement;
        Assert.Equal(version, root.GetProperty("version").GetString());
        Assert.Equal("win-x64", root.GetProperty("runtimeIdentifier").GetString());
        Assert.Equal("stable", root.GetProperty("channel").GetString());
        Assert.Equal("installer", root.GetProperty("assetKind").GetString());
        Assert.Equal(packageName, root.GetProperty("packageFile").GetString());
        Assert.Equal(Path.GetFileName(checksumPath), root.GetProperty("checksumFile").GetString());
        Assert.Equal(expectedSha256, root.GetProperty("sha256").GetString());
        Assert.Equal(new FileInfo(packagePath).Length, root.GetProperty("packageSize").GetInt64());

        var updateManifestPath = Path.Combine(_tempRoot, "latest-dotnet-win-x64.ini");
        var manifestResult = await RunPowerShellAsync(
            powerShell,
            manifestScript,
            ("PackageMetadataPath", metadataPath),
            ("ArtifactsDirectory", outputDirectory),
            ("ReleaseTag", $"dotnet-v{version}"),
            ("OutputPath", updateManifestPath));

        Assert.Equal(0, manifestResult.ExitCode);

        var updateManifest = await File.ReadAllTextAsync(updateManifestPath);
        Assert.Contains($"version={version}", updateManifest, StringComparison.Ordinal);
        Assert.Contains("channel=stable", updateManifest, StringComparison.Ordinal);
        Assert.Contains("asset_kind=installer", updateManifest, StringComparison.Ordinal);
        Assert.Contains($"asset_url=https://github.com/vlcekapps/Vehimap/releases/download/dotnet-v{version}/{packageName}", updateManifest, StringComparison.Ordinal);
        Assert.Contains($"asset_sha256={expectedSha256}", updateManifest, StringComparison.Ordinal);
        Assert.Contains($"asset_size={new FileInfo(packagePath).Length}", updateManifest, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Package_script_creates_nightly_installer_metadata_and_update_manifest()
    {
        var powerShell = ResolvePowerShell();
        if (powerShell is null)
        {
            return;
        }

        if (ResolveInnoCompiler() is null)
        {
            return;
        }

        var publishDirectory = Path.Combine(_tempRoot, "nightly-publish");
        var outputDirectory = Path.Combine(_tempRoot, "nightly-release");
        Directory.CreateDirectory(publishDirectory);
        await File.WriteAllTextAsync(Path.Combine(publishDirectory, "Vehimap.Desktop.exe"), "desktop nightly binary");

        const string version = "9.8.7-nightly.123.1";
        var packageScript = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Package-DesktopRelease.ps1");
        var manifestScript = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Write-DotnetUpdateManifest.ps1");

        var packageResult = await RunPowerShellAsync(
            powerShell,
            packageScript,
            ("PublishDirectory", publishDirectory),
            ("RuntimeIdentifier", "win-x64"),
            ("Version", version),
            ("OutputDirectory", outputDirectory),
            ("Channel", "nightly"));

        Assert.Equal(0, packageResult.ExitCode);

        var packageName = $"vehimap-desktop-nightly-{version}-win-x64-setup.exe";
        var packagePath = Path.Combine(outputDirectory, packageName);
        var checksumPath = packagePath + ".sha256";
        var metadataPath = Path.Combine(outputDirectory, $"vehimap-desktop-nightly-{version}-win-x64-setup.json");

        Assert.True(File.Exists(packagePath), packageResult.CombinedOutput);
        Assert.True(File.Exists(checksumPath), packageResult.CombinedOutput);
        Assert.True(File.Exists(metadataPath), packageResult.CombinedOutput);

        var expectedSha256 = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(packagePath))).ToLowerInvariant();
        using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(metadataPath));
        var root = metadata.RootElement;
        Assert.Equal(version, root.GetProperty("version").GetString());
        Assert.Equal("win-x64", root.GetProperty("runtimeIdentifier").GetString());
        Assert.Equal("nightly", root.GetProperty("channel").GetString());
        Assert.Equal("installer", root.GetProperty("assetKind").GetString());
        Assert.Equal(packageName, root.GetProperty("packageFile").GetString());
        Assert.Equal(expectedSha256, root.GetProperty("sha256").GetString());

        var updateManifestPath = Path.Combine(_tempRoot, "latest-dotnet-nightly-win-x64.ini");
        var manifestResult = await RunPowerShellAsync(
            powerShell,
            manifestScript,
            ("PackageMetadataPath", metadataPath),
            ("ArtifactsDirectory", outputDirectory),
            ("ReleaseTag", "dotnet-nightly"),
            ("OutputPath", updateManifestPath));

        Assert.Equal(0, manifestResult.ExitCode);

        var updateManifest = await File.ReadAllTextAsync(updateManifestPath);
        Assert.Contains($"version={version}", updateManifest, StringComparison.Ordinal);
        Assert.Contains("channel=nightly", updateManifest, StringComparison.Ordinal);
        Assert.Contains("asset_kind=installer", updateManifest, StringComparison.Ordinal);
        Assert.Contains($"asset_url=https://github.com/vlcekapps/Vehimap/releases/download/dotnet-nightly/{packageName}", updateManifest, StringComparison.Ordinal);
        Assert.Contains($"asset_sha256={expectedSha256}", updateManifest, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Inno_template_closes_running_applications_and_offers_postinstall_launch()
    {
        var templatePath = Path.Combine(FindRepositoryRoot(), "dotnet", "installer", "windows", "Vehimap.iss.in");
        var template = await File.ReadAllTextAsync(templatePath);

        Assert.Contains("SetupIconFile={{SOURCE_DIR}}\\Vehimap.ico", template, StringComparison.Ordinal);
        Assert.Contains("UninstallDisplayIcon={app}\\Vehimap.ico", template, StringComparison.Ordinal);
        Assert.Contains("IconFilename: \"{app}\\Vehimap.ico\"", template, StringComparison.Ordinal);
        Assert.Contains("CloseApplications=yes", template, StringComparison.Ordinal);
        Assert.Contains("RestartApplications=no", template, StringComparison.Ordinal);
        Assert.Contains("{{SIGNING_DIRECTIVES}}", template, StringComparison.Ordinal);
        Assert.Contains("[Languages]", template, StringComparison.Ordinal);
        Assert.Contains("Name: \"english\"; MessagesFile: \"compiler:Default.isl\"", template, StringComparison.Ordinal);
        Assert.Contains("Name: \"czech\"; MessagesFile: \"compiler:Languages\\Czech.isl\"", template, StringComparison.Ordinal);
        Assert.Contains("installer-preferences.json", template, StringComparison.Ordinal);
        Assert.Contains("{{DATA_FOLDER}}", template, StringComparison.Ordinal);
        Assert.Contains("{\"language\":\"cs-CZ\"}", template, StringComparison.Ordinal);
        Assert.Contains("{\"language\":\"en-US\"}", template, StringComparison.Ordinal);
        Assert.DoesNotContain("settings.ini", template, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[Run]", template, StringComparison.Ordinal);
        Assert.Contains("Filename: \"{app}\\Vehimap.Desktop.exe\"", template, StringComparison.Ordinal);
        Assert.Contains("Flags: nowait postinstall skipifsilent", template, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Windows_packaging_stages_root_favicon_for_installer_shortcuts()
    {
        var repositoryRoot = FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "dotnet", "build", "Package-DesktopRelease.ps1");
        var projectPath = Path.Combine(repositoryRoot, "dotnet", "src", "Vehimap.Desktop", "Vehimap.Desktop.csproj");
        var gitIgnorePath = Path.Combine(repositoryRoot, ".gitignore");
        var script = await File.ReadAllTextAsync(scriptPath);
        var project = await File.ReadAllTextAsync(projectPath);
        var gitIgnore = await File.ReadAllTextAsync(gitIgnorePath);

        Assert.True(File.Exists(Path.Combine(repositoryRoot, "favicon.ico")), "Korenu repozitare musi existovat favicon.ico pro Windows instalator.");
        Assert.Contains("favicon.ico", script, StringComparison.Ordinal);
        Assert.Contains("Vehimap.ico", script, StringComparison.Ordinal);
        Assert.Contains("Windows instalator vyzaduje ikonu", script, StringComparison.Ordinal);
        Assert.Contains("Copy-Item -LiteralPath $sourceIconPath", script, StringComparison.Ordinal);
        Assert.Contains("$dataFolder = $appName", script, StringComparison.Ordinal);
        Assert.Contains("{{DATA_FOLDER}}", script, StringComparison.Ordinal);
        Assert.Contains("<ApplicationIcon>..\\..\\..\\favicon.ico</ApplicationIcon>", project, StringComparison.Ordinal);
        Assert.Contains("!favicon.ico", gitIgnore, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Windows_packaging_supports_optional_inno_signing_without_ui_configuration()
    {
        var repositoryRoot = FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "dotnet", "build", "Package-DesktopRelease.ps1");
        var templatePath = Path.Combine(repositoryRoot, "dotnet", "installer", "windows", "Vehimap.iss.in");
        var script = await File.ReadAllTextAsync(scriptPath);
        var template = await File.ReadAllTextAsync(templatePath);

        Assert.Contains("VEHIMAP_INNO_SIGNTOOL_COMMAND", script, StringComparison.Ordinal);
        Assert.Contains("musi obsahovat Inno placeholder `$f", script, StringComparison.Ordinal);
        Assert.Contains("SignTool=vehimap", script, StringComparison.Ordinal);
        Assert.Contains("SignedUninstaller=yes", script, StringComparison.Ordinal);
        Assert.Contains("SignToolRetryCount=3", script, StringComparison.Ordinal);
        Assert.Contains("/Svehimap=$signToolCommand", script, StringComparison.Ordinal);
        Assert.Contains("{{SIGNING_DIRECTIVES}}", template, StringComparison.Ordinal);
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

        var packagePath = Path.Combine(artifactsDirectory, "vehimap-desktop-9.8.7-win-x64.zip");
        await File.WriteAllTextAsync(packagePath, "real package bytes");
        var checksumPath = packagePath + ".sha256";
        await File.WriteAllTextAsync(checksumPath, $"{new string('a', 64)}  {Path.GetFileName(packagePath)}");

        var metadataPath = Path.Combine(artifactsDirectory, "vehimap-desktop-9.8.7-win-x64.json");
        var metadata = new
        {
            version = "9.8.7",
            runtimeIdentifier = "win-x64",
            packageFile = Path.GetFileName(packagePath),
            checksumFile = Path.GetFileName(checksumPath),
            createdUtc = DateTime.UtcNow.ToString("o")
        };
        await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata));

        var manifestScript = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Write-DotnetUpdateManifest.ps1");
        var result = await RunPowerShellAsync(
            powerShell,
            manifestScript,
            ("PackageMetadataPath", metadataPath),
            ("ArtifactsDirectory", artifactsDirectory),
            ("ReleaseTag", "dotnet-v9.8.7"),
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

    private static string? ResolveInnoCompiler()
    {
        var envPath = Environment.GetEnvironmentVariable("INNO_SETUP_COMPILER");
        if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
        {
            return envPath;
        }

        var onPath = FindOnPath("ISCC");
        if (onPath is not null)
        {
            return onPath;
        }

        foreach (var candidate in new[]
        {
            @"C:\Program Files\Inno Setup 7\ISCC.exe",
            @"C:\Program Files (x86)\Inno Setup 7\ISCC.exe",
            @"C:\Program Files\Inno Setup 6\ISCC.exe",
            @"C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
        })
        {
            if (File.Exists(candidate))
            {
                return candidate;
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
