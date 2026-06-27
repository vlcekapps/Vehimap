using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.ViewModels;
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
        var expectedManifestName = $"latest-dotnet-{ResolveExpectedRuntimeIdentifier()}.ini";

        Assert.Equal(expectedVersion, appInfo.AppVersion);
        Assert.Equal("Vehimap", appInfo.ApplicationName);
        Assert.Equal("stable", appInfo.ReleaseChannel);
        Assert.Equal(SemVersionService.NormalizeToFileVersion(expectedVersion), appInfo.FileVersion);
        Assert.EndsWith(expectedManifestName, appInfo.UpdateManifestUrl, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(AssemblyAppBuildInfoProvider.DefaultReleaseNotesUrl, appInfo.ReleaseNotesUrl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void About_dialog_clipboard_text_contains_support_diagnostics()
    {
        var model = new AboutDialogViewModel(
            "Vehimap",
            "1.2.3",
            "1.2.3.0",
            "portable",
            @"C:\vehimap\data",
            "Portable data vedle aplikace",
            "Windows 11 x64",
            ".NET 10.0",
            @"C:\vehimap\Vehimap.Desktop.exe",
            "https://example.com/release");

        Assert.Contains("Vehimap - O programu", model.ClipboardText, StringComparison.Ordinal);
        Assert.Contains("Verze aplikace: 1.2.3", model.ClipboardText, StringComparison.Ordinal);
        Assert.Contains(@"Datová složka: C:\vehimap\data", model.ClipboardText, StringComparison.Ordinal);
        Assert.Contains(@"Soubor aplikace: C:\vehimap\Vehimap.Desktop.exe", model.ClipboardText, StringComparison.Ordinal);
        Assert.Contains("Release poznámky: https://example.com/release", model.ClipboardText, StringComparison.Ordinal);
        Assert.Contains($"Poděkování autorovi: {AboutDialogViewModel.AuthorSupportUrl}", model.ClipboardText, StringComparison.Ordinal);
    }

    [Fact]
    public void About_dialog_summary_exposes_author_and_channel_version()
    {
        var model = new AboutDialogViewModel(
            "Vehimap Nightly",
            "1.2.3-nightly.45.1",
            "1.2.3.0",
            "published",
            @"C:\vehimap\data",
            "Systémová datová složka",
            "Windows 11 x64",
            ".NET 10.0",
            @"C:\vehimap\Vehimap.Desktop.exe",
            "https://example.com/release",
            "nightly");

        Assert.Equal("by Vlcek apps", model.Author);
        Assert.Equal("Poděkovat autorovi", model.ThankAuthorLabel);
        Assert.Equal("Otevře stránku, kde můžete autorovi poděkovat podporou tvorby.", model.ThankAuthorHelpText);
        Assert.Equal("1.2.3-nightly.45.1 (nightly)", model.DisplayVersion);
        Assert.False(model.IsDiagnosticsVisible);
        Assert.Equal("Zobrazit diagnostická data", model.ToggleDiagnosticsLabel);
        Assert.Contains("Autor: by Vlcek apps", model.DiagnosticText, StringComparison.Ordinal);

        model.ToggleDiagnostics();

        Assert.True(model.IsDiagnosticsVisible);
        Assert.Equal("Skrýt diagnostická data", model.ToggleDiagnosticsLabel);
        Assert.Contains("Kanál: nightly", model.DiagnosticText, StringComparison.Ordinal);
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
        service.Apply(settings, new DesktopSupportedSettingsSnapshot(45, 20, 7, 750, false, true, true, true, 3, 21));

        Assert.Equal("45", settings.GetValue("notifications", "technical_reminder_days"));
        Assert.Equal("20", settings.GetValue("notifications", "green_card_reminder_days"));
        Assert.Equal("7", settings.GetValue("notifications", "maintenance_reminder_days"));
        Assert.Equal("750", settings.GetValue("notifications", "maintenance_reminder_km"));
        Assert.Equal("1", settings.GetValue("app", "show_dashboard_on_launch"));
        Assert.Equal("1", settings.GetValue("app", "hide_on_launch"));
        Assert.Equal("1", settings.GetValue("backups", "automatic_backups_enabled"));
        Assert.Equal("3", settings.GetValue("backups", "automatic_backup_interval_days"));
        Assert.Equal("21", settings.GetValue("backups", "automatic_backup_keep_count"));
        Assert.Equal("keep-me", settings.GetValue("custom", "untouched_key"));
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0", 0)]
    [InlineData("1.0.1", "1.0.0", 1)]
    [InlineData("1.0.0-beta.1", "1.0.0", -1)]
    [InlineData("1.0.0-beta.2", "1.0.0-beta.1", 1)]
    [InlineData("1.0.0-nightly.124.1", "1.0.0-nightly.123.2", 1)]
    [InlineData("1.0.0-nightly.124.2", "1.0.0-nightly.124.1", 1)]
    [InlineData("1.0.0-nightly.124.1", "1.0.0-beta.1", 1)]
    public void Semver_compare_matches_expected_rules(string left, string right, int expected)
    {
        Assert.Equal(expected, Math.Sign(SemVersionService.Compare(left, right)));
    }

    [Theory]
    [InlineData("stable", "Vehimap", "Vehimap", "latest-dotnet-win-x64.ini")]
    [InlineData("beta", "Vehimap Beta", "Vehimap Beta", "latest-dotnet-beta-win-x64.ini")]
    [InlineData("nightly", "Vehimap Nightly", "Vehimap Nightly", "latest-dotnet-nightly-win-x64.ini")]
    public void Release_channel_service_resolves_names_data_folders_and_manifests(
        string channel,
        string expectedAppName,
        string expectedDataFolder,
        string expectedManifest)
    {
        Assert.Equal(expectedAppName, ReleaseChannelService.GetApplicationName(channel));
        Assert.Equal(expectedDataFolder, ReleaseChannelService.GetDataFolderName(channel));
        Assert.Equal(expectedManifest, ReleaseChannelService.GetUpdateManifestFileName(channel, "win-x64"));
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
        Assert.Equal("archive", manifest.AssetKind);
        Assert.Equal("stable", manifest.Channel);
    }

    [Fact]
    public void Manifest_parser_reads_installer_asset_and_channel()
    {
        const string content = """
            [release]
            version=1.0.9
            published_at=2026-04-02
            channel=beta
            asset_kind=installer
            notes_url=https://example.com/release
            asset_url=https://example.com/vehimap-setup.exe
            asset_sha256=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
            asset_size=2048
            """;

        var manifest = LegacyUpdateManifestParser.Parse(content);

        Assert.Equal("installer", manifest.AssetKind);
        Assert.Equal("beta", manifest.Channel);
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
    public async Task Windows_update_prepare_creates_install_plan_for_inno_installer()
    {
        var installerBytes = Encoding.UTF8.GetBytes("setup binary");
        var sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(installerBytes)).ToLowerInvariant();

        var buildInfo = new StubBuildInfoProvider(
            new AppBuildInfo(
                "Vehimap",
                "1.0.2",
                "1.0.2.0",
                "samostatna desktopova aplikace",
                Path.Combine(_tempRoot, "Vehimap.Desktop.exe"),
                "Windows",
                ".NET 10",
                "https://example.com/latest.ini",
                "https://example.com/release",
                Path.Combine(_tempRoot, "missing", "Vehimap.Updater.exe"),
                true));

        using var httpClient = new HttpClient(new StubHttpMessageHandler(installerBytes));
        var service = new LegacyUpdateService(buildInfo, httpClient);
        var result = await service.PrepareInstallAsync(new UpdateCheckResult(
            "1.0.2",
            "1.0.9",
            true,
            "2026-04-02",
            "https://example.com/release",
            "https://example.com/vehimap-setup.exe",
            sha256,
            installerBytes.LongLength,
            true,
            "Je dostupna novejsi verze.",
            null,
            null,
            "installer",
            "stable"));

        Assert.True(result.IsReady);
        Assert.NotNull(result.InstallPlan);
        Assert.Equal("installer", result.InstallPlan!.InstallKind);
        Assert.Equal(result.InstallPlan.UpdaterPath, result.InstallPlan.InstallerPath);
        Assert.True(File.Exists(result.InstallPlan.InstallerPath));
        Assert.Equal("1.0.9", result.InstallPlan.ExpectedVersion);
    }

    [Fact]
    public async Task Missing_desktop_release_manifest_returns_friendly_message()
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
                "https://raw.githubusercontent.com/vlcekapps/Vehimap/main/update/latest-dotnet-missing-test-rid.ini",
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

    [Fact]
    public async Task Invalid_local_update_manifest_falls_back_to_remote_manifest()
    {
        var manifestFileName = $"latest-dotnet-local-fallback-{Guid.NewGuid():N}.ini";
        var localUpdateDirectory = Path.Combine(AppContext.BaseDirectory, "update");
        var localManifestPath = Path.Combine(localUpdateDirectory, manifestFileName);
        Directory.CreateDirectory(localUpdateDirectory);
        await File.WriteAllTextAsync(localManifestPath, "[release]\nnotes_url=https://example.com/broken\n");

        try
        {
            var remoteManifest = """
                [release]
                version=1.0.3
                published_at=2026-04-02
                notes_url=https://example.com/release
                asset_url=https://example.com/vehimap.zip
                asset_sha256=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
                asset_size=2048
                """;
            var buildInfo = new StubBuildInfoProvider(
                new AppBuildInfo(
                    "Vehimap",
                    "1.0.2",
                    "1.0.2.0",
                    "samostatna desktopova aplikace",
                    Path.Combine(_tempRoot, "Vehimap.Desktop.exe"),
                    "Windows",
                    ".NET 10",
                    $"https://example.com/{manifestFileName}",
                    "https://github.com/vlcekapps/Vehimap/releases",
                    Path.Combine(_tempRoot, "Vehimap.Updater.exe"),
                    true));

            using var httpClient = new HttpClient(new StubHttpMessageHandler(Encoding.UTF8.GetBytes(remoteManifest)));
            var service = new LegacyUpdateService(buildInfo, httpClient);

            var result = await service.CheckForUpdatesAsync("1.0.2");

            Assert.True(result.IsUpdateAvailable);
            Assert.Null(result.FailureReason);
            Assert.Equal("1.0.3", result.LatestVersion);
            Assert.Contains("1.0.3", result.Message, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(localManifestPath))
            {
                File.Delete(localManifestPath);
            }
        }
    }

    [Fact]
    public async Task Check_for_updates_reports_why_automatic_install_is_not_available()
    {
        var manifestFileName = $"latest-dotnet-auto-reason-{Guid.NewGuid():N}.ini";
        var manifest = """
            [release]
            version=1.0.3
            published_at=2026-04-02
            notes_url=https://example.com/release
            asset_url=https://example.com/vehimap.zip
            asset_sha256=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
            asset_size=2048
            """;
        var buildInfo = new StubBuildInfoProvider(
            new AppBuildInfo(
                "Vehimap",
                "1.0.2",
                "1.0.2.0",
                "samostatna desktopova aplikace",
                Path.Combine(_tempRoot, "Vehimap.Desktop.exe"),
                "Windows",
                ".NET 10",
                $"https://example.com/{manifestFileName}",
                "https://github.com/vlcekapps/Vehimap/releases",
                Path.Combine(_tempRoot, "missing", "Vehimap.Updater.exe"),
                true));

        using var httpClient = new HttpClient(new StubHttpMessageHandler(Encoding.UTF8.GetBytes(manifest)));
        var service = new LegacyUpdateService(buildInfo, httpClient);

        var result = await service.CheckForUpdatesAsync("1.0.2");

        Assert.True(result.IsUpdateAvailable);
        if (OperatingSystem.IsWindows())
        {
            Assert.False(result.CanInstallAutomatically);
            Assert.Contains("Vehimap.Updater", result.AutomaticInstallUnavailableReason, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.False(result.CanInstallAutomatically);
            Assert.Contains("Windows", result.AutomaticInstallUnavailableReason, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Update_dialog_details_show_automatic_install_status()
    {
        var sha256 = new string('a', 64);
        var model = new UpdateDialogViewModel(new UpdateCheckResult(
            "1.0.2",
            "1.0.3",
            true,
            "2026-04-02",
            "https://example.com/release",
            "https://example.com/vehimap.zip",
            sha256,
            2048,
            false,
            "Je dostupna novejsi verze.",
            null,
            "Vedle aplikace chybi Vehimap.Updater."));

        Assert.Contains("Automatická instalace: nedostupná", model.Details, StringComparison.Ordinal);
        Assert.Contains("Vehimap.Updater", model.Details, StringComparison.Ordinal);
        Assert.Contains("Asset ke stažení: https://example.com/vehimap.zip", model.Details, StringComparison.Ordinal);
        Assert.Contains($"SHA-256: {sha256}", model.Details, StringComparison.Ordinal);
        Assert.Contains("Vehimap - kontrola aktualizací", model.ClipboardText, StringComparison.Ordinal);
        Assert.Contains("Aktuální verze: 1.0.2", model.ClipboardText, StringComparison.Ordinal);
        Assert.Contains("Asset ke stažení: https://example.com/vehimap.zip", model.ClipboardText, StringComparison.Ordinal);
        Assert.Contains($"SHA-256: {sha256}", model.ClipboardText, StringComparison.Ordinal);
        Assert.Equal("Otevřít release stránku", model.PrimaryActionLabel);
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
