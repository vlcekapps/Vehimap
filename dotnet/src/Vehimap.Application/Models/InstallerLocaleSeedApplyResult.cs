namespace Vehimap.Application.Models;

public sealed record InstallerLocaleSeedApplyResult(
    bool SeedFound,
    bool SettingsChanged,
    bool SeedValid,
    string SeedPath,
    string Message)
{
    public static InstallerLocaleSeedApplyResult NotFound { get; } =
        new(false, false, true, string.Empty, string.Empty);
}
