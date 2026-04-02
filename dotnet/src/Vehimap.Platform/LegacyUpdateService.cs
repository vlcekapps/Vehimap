using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Platform;

public sealed class LegacyUpdateService : IUpdateService
{
    private readonly IAppBuildInfoProvider _appBuildInfoProvider;
    private readonly HttpClient _httpClient;

    public LegacyUpdateService(IAppBuildInfoProvider appBuildInfoProvider, HttpClient? httpClient = null)
    {
        _appBuildInfoProvider = appBuildInfoProvider;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        var appInfo = _appBuildInfoProvider.GetCurrent();
        try
        {
            var manifest = await LoadManifestAsync(appInfo, cancellationToken).ConfigureAwait(false);
            var comparison = SemVersionService.Compare(currentVersion, manifest.Version);
            var canInstallAutomatically = comparison < 0
                && OperatingSystem.IsWindows()
                && appInfo.IsPublishedBuild
                && File.Exists(appInfo.UpdaterPath)
                && ValidateInstallMetadata(manifest, out _);

            if (comparison < 0)
            {
                var sizeText = manifest.AssetSize is > 0 ? $" Velikost balicku: {FormatSize(manifest.AssetSize.Value)}." : string.Empty;
                return new UpdateCheckResult(
                    currentVersion,
                    manifest.Version,
                    true,
                    manifest.PublishedAt,
                    manifest.NotesUrl,
                    manifest.AssetUrl,
                    manifest.AssetSha256,
                    manifest.AssetSize,
                    canInstallAutomatically,
                    $"Je dostupna novejsi verze Vehimap ({manifest.Version}).{sizeText}");
            }

            if (comparison > 0)
            {
                return new UpdateCheckResult(
                    currentVersion,
                    manifest.Version,
                    false,
                    manifest.PublishedAt,
                    manifest.NotesUrl,
                    manifest.AssetUrl,
                    manifest.AssetSha256,
                    manifest.AssetSize,
                    false,
                    $"Pouzivate novejsi lokalni verzi ({currentVersion}) nez je v manifestu ({manifest.Version}).");
            }

            return new UpdateCheckResult(
                currentVersion,
                manifest.Version,
                false,
                manifest.PublishedAt,
                manifest.NotesUrl,
                manifest.AssetUrl,
                manifest.AssetSha256,
                manifest.AssetSize,
                false,
                $"Pouzivate aktualni verzi Vehimap ({currentVersion}).");
        }
        catch (PreviewManifestUnavailableException ex)
        {
            return new UpdateCheckResult(
                currentVersion,
                currentVersion,
                false,
                null,
                appInfo.ReleaseNotesUrl,
                null,
                null,
                null,
                false,
                ex.Message);
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult(
                currentVersion,
                currentVersion,
                false,
                null,
                appInfo.ReleaseNotesUrl,
                null,
                null,
                null,
                false,
                "Kontrolu aktualizaci se nepodarilo dokoncit.",
                ex.Message);
        }
    }

    public async Task<UpdateInstallResult> PrepareInstallAsync(UpdateCheckResult update, CancellationToken cancellationToken = default)
    {
        var appInfo = _appBuildInfoProvider.GetCurrent();

        if (!OperatingSystem.IsWindows())
        {
            return new UpdateInstallResult(false, "Automaticka instalace je v teto etape dostupna jen na Windows.", null);
        }

        if (!update.IsUpdateAvailable)
        {
            return new UpdateInstallResult(false, "Pro instalaci neni pripravena zadna novejsi verze.", null);
        }

        if (!update.CanInstallAutomatically)
        {
            return new UpdateInstallResult(false, "Aktualizaci lze zatim otevrit jen rucne pres release stranku nebo asset.", null);
        }

        if (!ValidateInstallMetadata(update, out var validationError))
        {
            return new UpdateInstallResult(false, validationError, null);
        }

        if (!File.Exists(appInfo.UpdaterPath))
        {
            return new UpdateInstallResult(false, "Vedle aplikace chybi Vehimap.Updater, automatickou instalaci proto nelze pripravit.", null);
        }

        var tempRoot = Path.Combine(Path.GetTempPath(), $"VehimapDesktopUpdate_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var downloadPath = Path.Combine(tempRoot, "vehimap-update.zip");
            var extractRoot = Path.Combine(tempRoot, "payload");

            await DownloadAsync(update.AssetUrl!, downloadPath, cancellationToken).ConfigureAwait(false);
            ValidateDownloadedAsset(downloadPath, update.AssetSize!.Value, update.Sha256!);

            ZipFile.ExtractToDirectory(downloadPath, extractRoot);
            var sourceDirectory = ResolvePayloadRoot(extractRoot);
            var installPlan = new UpdateInstallPlan(
                appInfo.UpdaterPath,
                sourceDirectory,
                AppContext.BaseDirectory,
                appInfo.ApplicationPath,
                Process.GetCurrentProcess().Id,
                update.LatestVersion);

            return new UpdateInstallResult(true, "Aktualizace je pripravena k instalaci.", installPlan);
        }
        catch (Exception ex)
        {
            TryDeleteDirectory(tempRoot);
            return new UpdateInstallResult(false, $"Aktualizaci se nepodarilo pripravit. {ex.Message}", null);
        }
    }

    private async Task<LegacyUpdateManifest> LoadManifestAsync(AppBuildInfo appInfo, CancellationToken cancellationToken)
    {
        var manifestFileName = GetManifestFileName(appInfo.UpdateManifestUrl);
        var localManifestPath = FindLocalManifestPath(AppContext.BaseDirectory, manifestFileName);
        if (!string.IsNullOrWhiteSpace(localManifestPath))
        {
            return LegacyUpdateManifestParser.Parse(await File.ReadAllTextAsync(localManifestPath, cancellationToken).ConfigureAwait(false));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, appInfo.UpdateManifestUrl);
        request.Headers.UserAgent.ParseAdd($"{appInfo.ApplicationName}/{appInfo.AppVersion}");
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound && IsPreviewManifestFileName(manifestFileName))
            {
                throw new PreviewManifestUnavailableException("Preview kanal aktualizaci pro desktopovou .NET vetev zatim neni publikovany.");
            }

            throw new InvalidOperationException($"Server vratil HTTP {(int)response.StatusCode}.");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return LegacyUpdateManifestParser.Parse(content);
    }

    private static string? FindLocalManifestPath(string startPath, string manifestFileName)
    {
        var current = new DirectoryInfo(startPath);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "update", manifestFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }

    private static string GetManifestFileName(string updateManifestUrl)
    {
        if (Uri.TryCreate(updateManifestUrl, UriKind.Absolute, out var manifestUri))
        {
            var manifestName = Path.GetFileName(manifestUri.AbsolutePath);
            if (!string.IsNullOrWhiteSpace(manifestName))
            {
                return manifestName;
            }
        }

        var fallbackName = Path.GetFileName(updateManifestUrl);
        return string.IsNullOrWhiteSpace(fallbackName) ? "latest.ini" : fallbackName;
    }

    private static bool IsPreviewManifestFileName(string manifestFileName) =>
        manifestFileName.Contains("latest-dotnet-preview-", StringComparison.OrdinalIgnoreCase);

    private static bool ValidateInstallMetadata(UpdateCheckResult update, out string error)
    {
        if (string.IsNullOrWhiteSpace(update.AssetUrl))
        {
            error = "Manifest neobsahuje odkaz na release asset.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(update.Sha256) || update.Sha256.Length != 64)
        {
            error = "Manifest neobsahuje platny SHA-256 hash assetu.";
            return false;
        }

        if (update.AssetSize is null or <= 0)
        {
            error = "Manifest neobsahuje platnou velikost assetu.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool ValidateInstallMetadata(LegacyUpdateManifest manifest, out string error)
    {
        return ValidateInstallMetadata(
            new UpdateCheckResult(
                "0.0.0",
                manifest.Version,
                true,
                manifest.PublishedAt,
                manifest.NotesUrl,
                manifest.AssetUrl,
                manifest.AssetSha256,
                manifest.AssetSize,
                false,
                string.Empty),
            out error);
    }

    private async Task DownloadAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Stazeni assetu selhalo (HTTP {(int)response.StatusCode}).");
        }

        await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var destinationStream = File.Create(destinationPath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateDownloadedAsset(string archivePath, long expectedSize, string expectedSha256)
    {
        var fileInfo = new FileInfo(archivePath);
        if (fileInfo.Length != expectedSize)
        {
            throw new InvalidOperationException("Stazeny archiv ma jinou velikost, nez ocekava manifest.");
        }

        using var stream = File.OpenRead(archivePath);
        using var sha = SHA256.Create();
        var actualSha256 = Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
        if (!string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Stazeny archiv neodpovida ocekavanemu SHA-256 hashi.");
        }
    }

    private static string ResolvePayloadRoot(string extractRoot)
    {
        var directories = Directory.GetDirectories(extractRoot);
        var files = Directory.GetFiles(extractRoot);
        if (files.Length == 0 && directories.Length == 1)
        {
            return directories[0];
        }

        return extractRoot;
    }

    private static string FormatSize(long sizeBytes)
    {
        if (sizeBytes < 1024)
        {
            return $"{sizeBytes} B";
        }

        var sizeKb = sizeBytes / 1024d;
        if (sizeKb < 1024)
        {
            return $"{sizeKb:0.0} KB";
        }

        var sizeMb = sizeKb / 1024d;
        if (sizeMb < 1024)
        {
            return $"{sizeMb:0.0} MB";
        }

        var sizeGb = sizeMb / 1024d;
        return $"{sizeGb:0.00} GB";
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch
        {
        }
    }

    private sealed class PreviewManifestUnavailableException : Exception
    {
        public PreviewManifestUnavailableException(string message)
            : base(message)
        {
        }
    }
}
