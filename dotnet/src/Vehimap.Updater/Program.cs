// SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using Vehimap.Updater;

try
{
    var options = new Dictionary<string, string>(StringComparer.Ordinal);
    if (args.Length % 2 != 0) throw new ArgumentException("Expected named argument/value pairs.");
    for (var i = 0; i < args.Length; i += 2)
    {
        if (args[i] is not ("--source" or "--target" or "--pid" or "--entry") || !options.TryAdd(args[i], args[i + 1]))
            throw new ArgumentException("Unknown or duplicate argument.");
    }
    var source = options["--source"];
    var target = options["--target"];
    options.TryGetValue("--entry", out var entry);
    ArchiveInstaller.ValidatePaths(source, target, entry);
    if (options.TryGetValue("--pid", out var processId))
    {
        if (!int.TryParse(processId, out var pid) || pid <= 0) throw new ArgumentException("Invalid process ID.");
        if (!ArchiveInstaller.WaitForApplicationExit(pid))
            throw new TimeoutException("The application is still running. No files were replaced.");
    }
    ArchiveInstaller.Install(source, target);
    if (!string.IsNullOrWhiteSpace(entry))
    {
        Process.Start(new ProcessStartInfo(Path.GetFullPath(entry))
        {
            WorkingDirectory = Path.GetFullPath(target),
            UseShellExecute = true
        });
    }
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Update failed: {ex.Message}");
    return 1;
}
