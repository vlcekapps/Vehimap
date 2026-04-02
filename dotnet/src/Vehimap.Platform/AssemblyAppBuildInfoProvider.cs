using System.Reflection;
using System.Runtime.InteropServices;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Platform;

public sealed class AssemblyAppBuildInfoProvider : IAppBuildInfoProvider
{
    public const string DefaultUpdateManifestUrl = "https://raw.githubusercontent.com/vlcekapps/Vehimap/main/update/latest.ini";
    public const string DefaultReleaseNotesUrl = "https://github.com/vlcekapps/Vehimap/releases/latest";

    public AppBuildInfo GetCurrent()
    {
        var buildAssembly = Assembly.GetExecutingAssembly();
        var applicationPath = Environment.ProcessPath
            ?? (Assembly.GetEntryAssembly()?.Location)
            ?? buildAssembly.Location
            ?? AppContext.BaseDirectory;
        var informationalVersion = buildAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? buildAssembly.GetName().Version?.ToString(3)
            ?? "0.0.0";
        var fileVersion = buildAssembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>()?
            .Version
            ?? buildAssembly.GetName().Version?.ToString()
            ?? "0.0.0.0";
        var appVersion = informationalVersion.Split('+', 2)[0];
        var processName = Path.GetFileNameWithoutExtension(applicationPath);
        var isPublishedBuild = !string.Equals(processName, "dotnet", StringComparison.OrdinalIgnoreCase);
        var updaterExtension = OperatingSystem.IsWindows() ? ".exe" : string.Empty;
        var updaterPath = Path.Combine(AppContext.BaseDirectory, $"Vehimap.Updater{updaterExtension}");

        return new AppBuildInfo(
            "Vehimap",
            appVersion,
            fileVersion,
            isPublishedBuild ? "samostatná desktopová aplikace" : "vývojový Avalonia shell",
            applicationPath,
            RuntimeInformation.OSDescription,
            RuntimeInformation.FrameworkDescription,
            DefaultUpdateManifestUrl,
            DefaultReleaseNotesUrl,
            updaterPath,
            isPublishedBuild);
    }
}
