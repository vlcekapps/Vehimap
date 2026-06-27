namespace Vehimap.Application;

public static class ReleaseChannelService
{
    public const string Stable = "stable";
    public const string Beta = "beta";
    public const string Nightly = "nightly";

    public static string Normalize(string? channel)
    {
        if (string.IsNullOrWhiteSpace(channel))
        {
            return Stable;
        }

        return channel.Trim().ToLowerInvariant() switch
        {
            Beta => Beta,
            Nightly => Nightly,
            _ => Stable
        };
    }

    public static string GetApplicationName(string? channel)
    {
        return Normalize(channel) switch
        {
            Beta => "Vehimap Beta",
            Nightly => "Vehimap Nightly",
            _ => "Vehimap"
        };
    }

    public static string GetDataFolderName(string? channel) => GetApplicationName(channel);

    public static string GetInstallerFolderName(string? channel) => GetApplicationName(channel);

    public static string GetUpdateManifestFileName(string? channel, string runtimeIdentifier)
    {
        var normalizedChannel = Normalize(channel);
        return normalizedChannel == Stable
            ? $"latest-dotnet-{runtimeIdentifier}.ini"
            : $"latest-dotnet-{normalizedChannel}-{runtimeIdentifier}.ini";
    }

    public static string GetInstallerAppId(string? channel)
    {
        return Normalize(channel) switch
        {
            Beta => "{{D6BA4F44-3961-4EE0-9645-0C64B00F1D95}",
            Nightly => "{{F62CE01E-1CB2-4E09-A52D-2865B1F02078}",
            _ => "{{C11E3BB4-7B0A-4D4E-91F3-FBC2F3F50D8A}"
        };
    }
}
