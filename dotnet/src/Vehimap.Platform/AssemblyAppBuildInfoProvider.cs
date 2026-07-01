using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Platform;

public sealed class AssemblyAppBuildInfoProvider : IAppBuildInfoProvider
{
    private const string ReleaseChannelMetadataName = "VehimapReleaseChannel";
    private readonly Func<IAppLocalizer> _localizerProvider;

    public const string DefaultUpdateManifestBaseUrl = "https://raw.githubusercontent.com/vlcekapps/Vehimap/main/update";
    public const string DefaultReleaseNotesUrl = "https://github.com/vlcekapps/Vehimap/releases";

    public AssemblyAppBuildInfoProvider(Func<IAppLocalizer>? localizerProvider = null)
    {
        _localizerProvider = localizerProvider ?? (() => new ResourceAppLocalizer(CultureInfo.CurrentUICulture));
    }

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
        var runtimeIdentifier = ResolveRuntimeIdentifier();
        var releaseChannel = ResolveCurrentReleaseChannel();
        var applicationName = ReleaseChannelService.GetApplicationName(releaseChannel);
        var updateManifestFileName = ReleaseChannelService.GetUpdateManifestFileName(releaseChannel, runtimeIdentifier);
        var updateManifestUrl = $"{DefaultUpdateManifestBaseUrl}/{updateManifestFileName}";

        return new AppBuildInfo(
            applicationName,
            appVersion,
            fileVersion,
            isPublishedBuild ? L("AppBuildInfo.RuntimeMode.Published") : L("AppBuildInfo.RuntimeMode.Development"),
            applicationPath,
            RuntimeInformation.OSDescription,
            RuntimeInformation.FrameworkDescription,
            updateManifestUrl,
            DefaultReleaseNotesUrl,
            updaterPath,
            isPublishedBuild,
            releaseChannel);
    }

    private string L(string key) => _localizerProvider().GetString(key);

    internal static string ResolveRuntimeIdentifier()
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

    public static string ResolveCurrentReleaseChannel()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var platformAssembly = typeof(AssemblyAppBuildInfoProvider).Assembly;
        var channel = ReadReleaseChannel(entryAssembly)
            ?? ReadReleaseChannel(platformAssembly)
            ?? Environment.GetEnvironmentVariable("VEHIMAP_RELEASE_CHANNEL");
        return ReleaseChannelService.Normalize(channel);
    }

    public static string ResolveCurrentApplicationDataFolderName() =>
        ReleaseChannelService.GetDataFolderName(ResolveCurrentReleaseChannel());

    private static string? ReadReleaseChannel(Assembly? assembly)
    {
        return assembly?
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => string.Equals(attribute.Key, ReleaseChannelMetadataName, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }
}
