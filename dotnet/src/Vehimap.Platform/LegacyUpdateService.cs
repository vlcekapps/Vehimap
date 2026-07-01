using System.Diagnostics;
using System.Globalization;
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
    private readonly Func<IAppLocalizer> _localizerProvider;

    public LegacyUpdateService(
        IAppBuildInfoProvider appBuildInfoProvider,
        HttpClient? httpClient = null,
        Func<IAppLocalizer>? localizerProvider = null)
    {
        _appBuildInfoProvider = appBuildInfoProvider;
        _httpClient = httpClient ?? new HttpClient();
        _localizerProvider = localizerProvider ?? (() => new ResourceAppLocalizer(CultureInfo.CurrentUICulture));
    }

    private IAppLocalizer Localizer => _localizerProvider();

    private string L(string key) => Localizer.GetString(key);

    private string LF(string key, params object?[] args) => Localizer.Format(key, args);

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        var appInfo = _appBuildInfoProvider.GetCurrent();
        try
        {
            var manifest = await LoadManifestAsync(appInfo, cancellationToken).ConfigureAwait(false);
            var comparison = SemVersionService.Compare(currentVersion, manifest.Version);
            var automaticInstallUnavailableReason = comparison < 0
                ? BuildAutomaticInstallUnavailableReason(appInfo, manifest)
                : null;
            var canInstallAutomatically = comparison < 0 && automaticInstallUnavailableReason is null;

            if (comparison < 0)
            {
                var sizeText = manifest.AssetSize is > 0
                    ? LF("UpdateService.Check.UpdateAvailableSizeSuffix", FormatSize(manifest.AssetSize.Value))
                    : string.Empty;
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
                    LF("UpdateService.Check.UpdateAvailable", manifest.Version, sizeText),
                    null,
                    automaticInstallUnavailableReason,
                    manifest.AssetKind,
                    manifest.Channel);
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
                    LF("UpdateService.Check.LocalNewer", currentVersion, manifest.Version),
                    null,
                    null,
                    manifest.AssetKind,
                    manifest.Channel);
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
                LF("UpdateService.Check.Current", currentVersion),
                null,
                null,
                manifest.AssetKind,
                manifest.Channel);
        }
        catch (UpdateManifestUnavailableException ex)
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
                L("UpdateService.Check.Failed"),
                ex.Message);
        }
    }

    public async Task<UpdateInstallResult> PrepareInstallAsync(
        UpdateCheckResult update,
        IProgress<UpdateInstallProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var appInfo = _appBuildInfoProvider.GetCurrent();

        if (!OperatingSystem.IsWindows())
        {
            return new UpdateInstallResult(false, L("UpdateService.Install.WindowsOnly"), null);
        }

        if (!update.IsUpdateAvailable)
        {
            return new UpdateInstallResult(false, L("UpdateService.Install.NoUpdate"), null);
        }

        if (!update.CanInstallAutomatically)
        {
            return new UpdateInstallResult(false, L("UpdateService.Install.ManualOnly"), null);
        }

        if (!ValidateInstallMetadata(update, out var validationError))
        {
            return new UpdateInstallResult(false, validationError, null);
        }

        if (IsArchiveAsset(update.AssetKind) && !File.Exists(appInfo.UpdaterPath))
        {
            return new UpdateInstallResult(false, L("UpdateService.Install.UpdaterMissing"), null);
        }

        var tempRoot = Path.Combine(Path.GetTempPath(), $"VehimapDesktopUpdate_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var downloadPath = IsInstallerAsset(update.AssetKind)
                ? Path.Combine(tempRoot, "vehimap-update-setup.exe")
                : Path.Combine(tempRoot, "vehimap-update.zip");
            var extractRoot = Path.Combine(tempRoot, "payload");

            var totalBytes = update.AssetSize!.Value;
            progress?.Report(new UpdateInstallProgress(L("UpdateService.Install.DownloadProgress"), 0, totalBytes));
            await DownloadAsync(update.AssetUrl!, downloadPath, totalBytes, progress, cancellationToken).ConfigureAwait(false);
            progress?.Report(new UpdateInstallProgress(L("UpdateService.Install.VerifyProgress"), totalBytes, totalBytes, true));
            ValidateDownloadedAsset(downloadPath, update.AssetSize!.Value, update.Sha256!);

            if (IsInstallerAsset(update.AssetKind))
            {
                progress?.Report(new UpdateInstallProgress(L("UpdateService.Install.InstallerReadyProgress"), totalBytes, totalBytes));
                var installerPlan = new UpdateInstallPlan(
                    downloadPath,
                    tempRoot,
                    AppContext.BaseDirectory,
                    appInfo.ApplicationPath,
                    Process.GetCurrentProcess().Id,
                    update.LatestVersion,
                    "installer",
                    downloadPath);

                return new UpdateInstallResult(true, L("UpdateService.Install.InstallerReadyResult"), installerPlan);
            }

            ZipFile.ExtractToDirectory(downloadPath, extractRoot);
            progress?.Report(new UpdateInstallProgress(L("UpdateService.Install.ArchiveReadyProgress"), totalBytes, totalBytes));
            var sourceDirectory = ResolvePayloadRoot(extractRoot);
            var archivePlan = new UpdateInstallPlan(
                appInfo.UpdaterPath,
                sourceDirectory,
                AppContext.BaseDirectory,
                appInfo.ApplicationPath,
                Process.GetCurrentProcess().Id,
                update.LatestVersion,
                "archive");

            return new UpdateInstallResult(true, L("UpdateService.Install.ArchiveReadyResult"), archivePlan);
        }
        catch (Exception ex)
        {
            TryDeleteDirectory(tempRoot);
            return new UpdateInstallResult(false, LF("UpdateService.Install.PrepareFailed", ex.Message), null);
        }
    }

    private async Task<LegacyUpdateManifest> LoadManifestAsync(AppBuildInfo appInfo, CancellationToken cancellationToken)
    {
        var manifestFileName = GetManifestFileName(appInfo.UpdateManifestUrl);
        var localManifestPath = FindLocalManifestPath(AppContext.BaseDirectory, manifestFileName);
        if (!string.IsNullOrWhiteSpace(localManifestPath))
        {
            var localManifest = await TryLoadLocalManifestAsync(localManifestPath, cancellationToken).ConfigureAwait(false);
            if (localManifest is not null)
            {
                return localManifest;
            }
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, appInfo.UpdateManifestUrl);
        request.Headers.UserAgent.ParseAdd($"{appInfo.ApplicationName}/{appInfo.AppVersion}");
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound && IsDotnetReleaseManifestFileName(manifestFileName))
            {
                throw new UpdateManifestUnavailableException(L("UpdateService.Check.DotnetManifestUnavailable"));
            }

            throw new InvalidOperationException(LF("UpdateService.Check.ServerHttp", (int)response.StatusCode));
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return LegacyUpdateManifestParser.Parse(content);
    }

    private static async Task<LegacyUpdateManifest?> TryLoadLocalManifestAsync(string localManifestPath, CancellationToken cancellationToken)
    {
        try
        {
            return LegacyUpdateManifestParser.Parse(await File.ReadAllTextAsync(localManifestPath, cancellationToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
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

    private static bool IsDotnetReleaseManifestFileName(string manifestFileName) =>
        manifestFileName.StartsWith("latest-dotnet-", StringComparison.OrdinalIgnoreCase)
        && manifestFileName.EndsWith(".ini", StringComparison.OrdinalIgnoreCase);

    private string? BuildAutomaticInstallUnavailableReason(AppBuildInfo appInfo, LegacyUpdateManifest manifest)
    {
        if (!OperatingSystem.IsWindows())
        {
            return L("UpdateService.Install.WindowsOnlyDetailed");
        }

        if (!appInfo.IsPublishedBuild)
        {
            return L("UpdateService.Install.PublishedBuildOnly");
        }

        if (IsArchiveAsset(manifest.AssetKind) && !File.Exists(appInfo.UpdaterPath))
        {
            return L("UpdateService.Install.UpdaterMissing");
        }

        if (!ValidateInstallMetadata(manifest, out var validationError))
        {
            return validationError;
        }

        return null;
    }

    private bool ValidateInstallMetadata(UpdateCheckResult update, out string error)
    {
        if (string.IsNullOrWhiteSpace(update.AssetUrl))
        {
            error = L("UpdateService.Install.MissingAssetUrl");
            return false;
        }

        if (string.IsNullOrWhiteSpace(update.Sha256) || update.Sha256.Length != 64)
        {
            error = L("UpdateService.Install.InvalidSha256");
            return false;
        }

        if (update.AssetSize is null or <= 0)
        {
            error = L("UpdateService.Install.InvalidAssetSize");
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool IsInstallerAsset(string? assetKind) =>
        string.Equals(assetKind, "installer", StringComparison.OrdinalIgnoreCase);

    private static bool IsArchiveAsset(string? assetKind) =>
        string.IsNullOrWhiteSpace(assetKind)
        || string.Equals(assetKind, "archive", StringComparison.OrdinalIgnoreCase);

    private bool ValidateInstallMetadata(LegacyUpdateManifest manifest, out string error)
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

    private async Task DownloadAsync(
        string downloadUrl,
        string destinationPath,
        long expectedSize,
        IProgress<UpdateInstallProgress>? progress,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(LF("UpdateService.Download.HttpFailed", (int)response.StatusCode));
        }

        var totalBytes = response.Content.Headers.ContentLength is > 0
            ? response.Content.Headers.ContentLength.Value
            : expectedSize;
        var receivedBytes = 0L;
        var buffer = new byte[81920];
        await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var destinationStream = File.Create(destinationPath);
        while (true)
        {
            var read = await sourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            await destinationStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            receivedBytes += read;
            progress?.Report(new UpdateInstallProgress(L("UpdateService.Install.DownloadProgress"), receivedBytes, totalBytes));
        }
    }

    private void ValidateDownloadedAsset(string archivePath, long expectedSize, string expectedSha256)
    {
        var fileInfo = new FileInfo(archivePath);
        if (fileInfo.Length != expectedSize)
        {
            throw new InvalidOperationException(L("UpdateService.Download.SizeMismatch"));
        }

        using var stream = File.OpenRead(archivePath);
        using var sha = SHA256.Create();
        var actualSha256 = Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
        if (!string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(L("UpdateService.Download.HashMismatch"));
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

    private sealed class UpdateManifestUnavailableException : Exception
    {
        public UpdateManifestUnavailableException(string message)
            : base(message)
        {
        }
    }
}
