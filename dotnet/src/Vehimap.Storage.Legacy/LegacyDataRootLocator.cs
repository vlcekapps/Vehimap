using Vehimap.Application.Abstractions;

namespace Vehimap.Storage.Legacy;

public sealed class LegacyDataRootLocator : IDataRootLocator
{
    public VehimapDataRoot Resolve(string appBasePath)
    {
        appBasePath = string.IsNullOrWhiteSpace(appBasePath)
            ? AppContext.BaseDirectory
            : Path.GetFullPath(appBasePath);

        var portableDataPath = Path.Combine(appBasePath, "data");
        if (Directory.Exists(portableDataPath))
        {
            return new VehimapDataRoot(appBasePath, portableDataPath, true);
        }

        var systemDataPath = ResolveSystemDataPath();
        return new VehimapDataRoot(appBasePath, systemDataPath, false);
    }

    private static string ResolveSystemDataPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Vehimap");
        }

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", "Vehimap");
        }

        var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (!string.IsNullOrWhiteSpace(xdgDataHome))
        {
            return Path.Combine(xdgDataHome, "Vehimap");
        }

        var linuxHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(linuxHome, ".local", "share", "Vehimap");
    }
}
