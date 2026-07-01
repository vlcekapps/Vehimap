// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;

namespace Vehimap.Storage.Legacy;

public sealed class LegacyDataRootLocator : IDataRootLocator
{
    private readonly string _applicationDataFolderName;

    public LegacyDataRootLocator(string applicationDataFolderName = "Vehimap")
    {
        _applicationDataFolderName = string.IsNullOrWhiteSpace(applicationDataFolderName)
            ? "Vehimap"
            : applicationDataFolderName.Trim();
    }

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

        var systemDataPath = ResolveSystemDataPath(_applicationDataFolderName);
        return new VehimapDataRoot(appBasePath, systemDataPath, false);
    }

    private static string ResolveSystemDataPath(string applicationDataFolderName)
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                applicationDataFolderName);
        }

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", applicationDataFolderName);
        }

        var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (!string.IsNullOrWhiteSpace(xdgDataHome))
        {
            return Path.Combine(xdgDataHome, applicationDataFolderName);
        }

        var linuxHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(linuxHome, ".local", "share", applicationDataFolderName);
    }
}
