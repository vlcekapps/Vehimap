// SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Platform;

public sealed class ProcessFileLauncher : IFileLauncher
{
    private readonly Action<ProcessStartInfo> _startProcess;
    private readonly Func<FileLaunchPlatform> _platformResolver;
    private readonly IAppLocalizer _localizer;

    public ProcessFileLauncher()
        : this(null)
    {
    }

    public ProcessFileLauncher(IAppLocalizer? localizer)
        : this(StartProcess, CreatePlatformResolver(localizer), localizer)
    {
    }

    internal ProcessFileLauncher(
        Action<ProcessStartInfo> startProcess,
        Func<FileLaunchPlatform> platformResolver,
        IAppLocalizer? localizer = null)
    {
        _startProcess = startProcess;
        _platformResolver = platformResolver;
        _localizer = localizer ?? new ResourceAppLocalizer();
    }

    public Task OpenAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Launch(path);
        return Task.CompletedTask;
    }

    public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LaunchFolder(path);
        return Task.CompletedTask;
    }

    private void Launch(string path)
    {
        _startProcess(BuildStartInfo(path, _platformResolver(), _localizer));
    }

    private void LaunchFolder(string path)
    {
        _startProcess(BuildFolderStartInfo(path, _platformResolver(), _localizer));
    }

    internal static ProcessStartInfo BuildStartInfo(string path, FileLaunchPlatform platform) =>
        BuildStartInfo(path, platform, null);

    internal static ProcessStartInfo BuildStartInfo(string path, FileLaunchPlatform platform, IAppLocalizer? localizer)
    {
        localizer ??= new ResourceAppLocalizer();
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(localizer.GetString("Platform.FileLauncher.EmptyPath"), nameof(path));
        }

        return platform switch
        {
            FileLaunchPlatform.Windows => new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            },
            FileLaunchPlatform.MacOS => BuildCommandStartInfo("open", path),
            FileLaunchPlatform.Linux => BuildCommandStartInfo("xdg-open", path),
            _ => throw new PlatformNotSupportedException(localizer.GetString("Platform.FileLauncher.FileUnsupported"))
        };
    }

    internal static ProcessStartInfo BuildFolderStartInfo(string path, FileLaunchPlatform platform) =>
        BuildFolderStartInfo(path, platform, null);

    internal static ProcessStartInfo BuildFolderStartInfo(string path, FileLaunchPlatform platform, IAppLocalizer? localizer)
    {
        localizer ??= new ResourceAppLocalizer();
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(localizer.GetString("Platform.FileLauncher.EmptyPath"), nameof(path));
        }

        return platform switch
        {
            FileLaunchPlatform.Windows => BuildCommandStartInfo("explorer.exe", path),
            FileLaunchPlatform.MacOS => BuildCommandStartInfo("open", path),
            FileLaunchPlatform.Linux => BuildCommandStartInfo("xdg-open", path),
            _ => throw new PlatformNotSupportedException(localizer.GetString("Platform.FileLauncher.FolderUnsupported"))
        };
    }

    private static ProcessStartInfo BuildCommandStartInfo(string command, string path)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(path);
        return startInfo;
    }

    private static Func<FileLaunchPlatform> CreatePlatformResolver(IAppLocalizer? localizer)
    {
        var resolvedLocalizer = localizer ?? new ResourceAppLocalizer();
        return () => ResolveCurrentPlatform(resolvedLocalizer);
    }

    private static FileLaunchPlatform ResolveCurrentPlatform(IAppLocalizer localizer)
    {
        if (OperatingSystem.IsWindows())
        {
            return FileLaunchPlatform.Windows;
        }

        if (OperatingSystem.IsMacOS())
        {
            return FileLaunchPlatform.MacOS;
        }

        if (OperatingSystem.IsLinux())
        {
            return FileLaunchPlatform.Linux;
        }

        throw new PlatformNotSupportedException(localizer.GetString("Platform.FileLauncher.FileUnsupported"));
    }

    private static void StartProcess(ProcessStartInfo startInfo) => Process.Start(startInfo);
}

internal enum FileLaunchPlatform
{
    Windows,
    MacOS,
    Linux
}
