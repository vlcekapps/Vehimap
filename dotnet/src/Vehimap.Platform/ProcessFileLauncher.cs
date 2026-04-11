using System.Diagnostics;
using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class ProcessFileLauncher : IFileLauncher
{
    private readonly Action<ProcessStartInfo> _startProcess;
    private readonly Func<FileLaunchPlatform> _platformResolver;

    public ProcessFileLauncher()
        : this(StartProcess, ResolveCurrentPlatform)
    {
    }

    internal ProcessFileLauncher(Action<ProcessStartInfo> startProcess, Func<FileLaunchPlatform> platformResolver)
    {
        _startProcess = startProcess;
        _platformResolver = platformResolver;
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
        Launch(path);
        return Task.CompletedTask;
    }

    private void Launch(string path)
    {
        _startProcess(BuildStartInfo(path, _platformResolver()));
    }

    internal static ProcessStartInfo BuildStartInfo(string path, FileLaunchPlatform platform)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Cesta k otevření nesmí být prázdná.", nameof(path));
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
            _ => throw new PlatformNotSupportedException("Otevření souboru není pro tuto platformu podporované.")
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

    private static FileLaunchPlatform ResolveCurrentPlatform()
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

        throw new PlatformNotSupportedException("Otevření souboru není pro tuto platformu podporované.");
    }

    private static void StartProcess(ProcessStartInfo startInfo) => Process.Start(startInfo);
}

internal enum FileLaunchPlatform
{
    Windows,
    MacOS,
    Linux
}
