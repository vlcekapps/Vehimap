namespace Vehimap.Application;

public sealed record UpdateCheckResult(
    bool IsUpdateAvailable,
    string CurrentVersion,
    string LatestVersion,
    string? ManifestUrl,
    string? AssetUrl,
    string? Sha256);
