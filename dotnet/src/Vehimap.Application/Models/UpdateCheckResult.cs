namespace Vehimap.Application;

public sealed record UpdateCheckResult(
    string CurrentVersion,
    string LatestVersion,
    bool IsUpdateAvailable,
    string? PublishedAt,
    string? NotesUrl,
    string? AssetUrl,
    string? Sha256,
    long? AssetSize,
    bool CanInstallAutomatically,
    string Message,
    string? FailureReason = null);
