using System.Reflection;
using System.Runtime.InteropServices;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Platform;

public sealed class AssemblyAppBuildInfoProvider : IAppBuildInfoProvider
{
    public const string DefaultUpdateManifestBaseUrl = "https://raw.githubusercontent.com/vlcekapps/Vehimap/main/update";
    public const string DefaultReleaseNotesUrl = "https://github.com/vlcekapps/Vehimap/releases";

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
        var runtimeIdentifier = ResolvePreviewRuntimeIdentifier();
        var updateManifestUrl = $"{DefaultUpdateManifestBaseUrl}/latest-dotnet-preview-{runtimeIdentifier}.ini";

        return new AppBuildInfo(
            "Vehimap",
            appVersion,
            fileVersion,
            isPublishedBuild ? "samostatná desktopová aplikace" : "vývojový Avalonia shell",
            applicationPath,
            RuntimeInformation.OSDescription,
            RuntimeInformation.FrameworkDescription,
            updateManifestUrl,
            DefaultReleaseNotesUrl,
            updaterPath,
            isPublishedBuild);
    }

    internal static string ResolvePreviewRuntimeIdentifier()
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
}
