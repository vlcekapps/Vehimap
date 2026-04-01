using System.Diagnostics;
using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class ProcessFileLauncher : IFileLauncher
{
    public Task OpenAsync(string path, CancellationToken cancellationToken = default)
    {
        Launch(path);
        return Task.CompletedTask;
    }

    public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default)
    {
        Launch(path);
        return Task.CompletedTask;
    }

    private static void Launch(string path)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        };

        Process.Start(startInfo);
    }
}
