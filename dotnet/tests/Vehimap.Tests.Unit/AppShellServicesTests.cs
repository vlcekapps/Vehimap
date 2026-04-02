using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class AppShellServicesTests : IDisposable
{
    private readonly string _tempRoot;

    public AppShellServicesTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-dotnet-app-shell", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public void Build_info_provider_reads_same_semver_as_root_version_file()
    {
        var provider = new AssemblyAppBuildInfoProvider();
        var appInfo = provider.GetCurrent();
        var expectedVersion = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "VERSION")).Trim();
        var expectedManifestName = $"latest-dotnet-preview-{ResolveExpectedRuntimeIdentifier()}.ini";

        Assert.Equal(expectedVersion, appInfo.AppVersion);
        Assert.Equal(SemVersionService.NormalizeToFileVersion(expectedVersion), appInfo.FileVersion);
        Assert.EndsWith(expectedManifestName, appInfo.UpdateManifestUrl, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(AssemblyAppBuildInfoProvider.DefaultReleaseNotesUrl, appInfo.ReleaseNotesUrl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Supported_settings_service_preserves_unrelated_sections_and_keys()
    {
        var settings = new VehimapSettings();
        settings.SetValue("notifications", "technical_reminder_days", "30");
        settings.SetValue("notifications", "green_card_reminder_days", "15");
        settings.SetValue("app", "show_dashboard_on_launch", "0");
        settings.SetValue("app", "hide_on_launch", "1");
        settings.SetValue("backups", "automatic_backups_enabled", "1");
        settings.SetValue("custom", "untouched_key", "keep-me");

        var service = new DesktopSupportedSettingsService();
        service.Apply(settings, new DesktopSupportedSettingsSnapshot(45, 20, 7, 750, true));

        Assert.Equal("45", settings.GetValue("notifications", "technical_reminder_days"));
        Assert.Equal("20", settings.GetValue("notifications", "green_card_reminder_days"));
        Assert.Equal("7", settings.GetValue("notifications", "maintenance_reminder_days"));
        Assert.Equal("750", settings.GetValue("notifications", "maintenance_reminder_km"));
        Assert.Equal("1", settings.GetValue("app", "show_dashboard_on_launch"));
        Assert.Equal("1", settings.GetValue("app", "hide_on_launch"));
        Assert.Equal("1", settings.GetValue("backups", "automatic_backups_enabled"));
        Assert.Equal("keep-me", settings.GetValue("custom", "untouched_key"));
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0", 0)]
    [InlineData("1.0.1", "1.0.0", 1)]
    [InlineData("1.0.0-beta.1", "1.0.0", -1)]
    [InlineData("1.0.0-beta.2", "1.0.0-beta.1", 1)]
    public void Semver_compare_matches_expected_rules(string left, string right, int expected)
    {
        Assert.Equal(expected, Math.Sign(SemVersionService.Compare(left, right)));
    }

    [Fact]
    public void Manifest_parser_reads_valid_latest_ini()
    {
        const string content = """
            [release]
            version=1.0.9
            published_at=2026-04-02
            notes_url=https://example.com/release
            asset_url=https://example.com/vehimap.zip
            asset_sha256=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
            asset_size=2048
            """;

        var manifest = LegacyUpdateManifestParser.Parse(content);

        Assert.Equal("1.0.9", manifest.Version);
        Assert.Equal("2026-04-02", manifest.PublishedAt);
        Assert.Equal(2048, manifest.AssetSize);
    }

    [Fact]
    public void Manifest_parser_rejects_missing_release_version()
    {
        const string content = """
            [release]
            notes_url=https://example.com/release
            """;

        var ex = Assert.Throws<InvalidOperationException>(() => LegacyUpdateManifestParser.Parse(content));
        Assert.Contains("release/version", ex.Message);
    }

    [Fact]
    public async Task Windows_update_prepare_creates_install_plan_for_updater()
    {
        var updaterPath = Path.Combine(_tempRoot, "Vehimap.Updater.exe");
        await File.WriteAllTextAsync(updaterPath, "stub");

        var zipPath = Path.Combine(_tempRoot, "vehimap-1.0.9.zip");
        await CreateZipAsync(zipPath, ("vehimap.exe", "binary"), ("readme.html", "<html></html>"));
        var zipBytes = await File.ReadAllBytesAsync(zipPath);
        var sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(zipBytes)).ToLowerInvariant();

        var buildInfo = new StubBuildInfoProvider(
            new AppBuildInfo(
                "Vehimap",
                "1.0.2",
                "1.0.2.0",
                "samostatná desktopová aplikace",
                Path.Combine(_tempRoot, "Vehimap.Desktop.exe"),
                "Windows",
                ".NET 10",
                "https://example.com/latest.ini",
                "https://example.com/release",
                updaterPath,
                true));

        using var httpClient = new HttpClient(new StubHttpMessageHandler(zipBytes));
        var service = new LegacyUpdateService(buildInfo, httpClient);
        var result = await service.PrepareInstallAsync(new UpdateCheckResult(
            "1.0.2",
            "1.0.9",
            true,
            "2026-04-02",
            "https://example.com/release",
            "https://example.com/vehimap-1.0.9.zip",
            sha256,
            zipBytes.LongLength,
            true,
            "Je dostupná novější verze."));

        Assert.True(result.IsReady);
        Assert.NotNull(result.InstallPlan);
        Assert.Equal(updaterPath, result.InstallPlan!.UpdaterPath);
        Assert.Equal(AppContext.BaseDirectory, result.InstallPlan.TargetDirectory);
        Assert.Equal("1.0.9", result.InstallPlan.ExpectedVersion);
        Assert.True(File.Exists(Path.Combine(result.InstallPlan.SourceDirectory, "vehimap.exe")));
    }

    [Fact]
    public async Task Missing_preview_manifest_returns_friendly_message()
    {
        var buildInfo = new StubBuildInfoProvider(
            new AppBuildInfo(
                "Vehimap",
                "1.0.2",
                "1.0.2.0",
                "samostatna desktopova aplikace",
                Path.Combine(_tempRoot, "Vehimap.Desktop.exe"),
                "Windows",
                ".NET 10",
                "https://raw.githubusercontent.com/vlcekapps/Vehimap/main/update/latest-dotnet-preview-missing-test-rid.ini",
                "https://github.com/vlcekapps/Vehimap/releases",
                Path.Combine(_tempRoot, "Vehimap.Updater.exe"),
                true));

        using var httpClient = new HttpClient(new StubStatusHttpMessageHandler(HttpStatusCode.NotFound));
        var service = new LegacyUpdateService(buildInfo, httpClient);

        var result = await service.CheckForUpdatesAsync("1.0.2");

        Assert.False(result.IsUpdateAvailable);
        Assert.Null(result.FailureReason);
        Assert.Contains("zatim neni publikovany", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
    }

    private static async Task CreateZipAsync(string zipPath, params (string Name, string Content)[] files)
    {
        await using var stream = File.Create(zipPath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        foreach (var file in files)
        {
            var entry = archive.CreateEntry(file.Name);
            await using var entryStream = entry.Open();
            await entryStream.WriteAsync(Encoding.UTF8.GetBytes(file.Content));
        }
    }

    private sealed class StubBuildInfoProvider : IAppBuildInfoProvider
    {
        private readonly AppBuildInfo _appBuildInfo;

        public StubBuildInfoProvider(AppBuildInfo appBuildInfo)
        {
            _appBuildInfo = appBuildInfo;
        }

        public AppBuildInfo GetCurrent() => _appBuildInfo;
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly byte[] _content;

        public StubHttpMessageHandler(byte[] content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_content)
            };
            return Task.FromResult(response);
        }
    }

    private sealed class StubStatusHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public StubStatusHttpMessageHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(string.Empty)
            });
        }
    }

    private static string ResolveExpectedRuntimeIdentifier()
    {
        if (OperatingSystem.IsWindows())
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "win-x86",
                Architecture.Arm64 => "win-arm64",
                _ => "win-x64"
            };
        }

        if (OperatingSystem.IsMacOS())
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "osx-arm64",
                _ => "osx-x64"
            };
        }

        if (OperatingSystem.IsLinux())
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "linux-arm64",
                Architecture.Arm => "linux-arm",
                _ => "linux-x64"
            };
        }

        return "win-x64";
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var versionFile = Path.Combine(current.FullName, "src", "VERSION");
            var dotnetFolder = Path.Combine(current.FullName, "dotnet");
            if (File.Exists(versionFile) && Directory.Exists(dotnetFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Nepodarilo se najit koren repozitare Vehimap.");
    }
}
