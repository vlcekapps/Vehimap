using System.Diagnostics;

if (args.Length < 3)
{
    Console.Error.WriteLine("Usage: Vehimap.Updater --source <folder> --target <folder> [--pid <processId>] [--entry <appPath>]");
    return 1;
}

string? source = null;
string? target = null;
string? entry = null;
int? pid = null;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--source":
            source = i + 1 < args.Length ? args[++i] : null;
            break;
        case "--target":
            target = i + 1 < args.Length ? args[++i] : null;
            break;
        case "--entry":
            entry = i + 1 < args.Length ? args[++i] : null;
            break;
        case "--pid":
            if (i + 1 < args.Length && int.TryParse(args[++i], out var parsedPid))
            {
                pid = parsedPid;
            }
            break;
    }
}

if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
{
    Console.Error.WriteLine("Missing required --source or --target.");
    return 2;
}

source = Path.GetFullPath(source);
target = Path.GetFullPath(target);

if (!Directory.Exists(source))
{
    Console.Error.WriteLine($"Source folder not found: {source}");
    return 3;
}

if (pid is { } processId)
{
    try
    {
        using var process = Process.GetProcessById(processId);
        process.WaitForExit(30_000);
    }
    catch
    {
    }
}

Directory.CreateDirectory(target);
CopyDirectory(source, target, preserveDataDirectory: true);

if (!string.IsNullOrWhiteSpace(entry) && File.Exists(entry))
{
    Process.Start(new ProcessStartInfo
    {
        FileName = entry,
        WorkingDirectory = Path.GetDirectoryName(entry) ?? target,
        UseShellExecute = true
    });
}

return 0;

static void CopyDirectory(string sourceDirectory, string targetDirectory, bool preserveDataDirectory)
{
    foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
    {
        var relative = Path.GetRelativePath(sourceDirectory, directory);
        if (preserveDataDirectory && relative.StartsWith("data", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        Directory.CreateDirectory(Path.Combine(targetDirectory, relative));
    }

    foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
    {
        var relative = Path.GetRelativePath(sourceDirectory, file);
        if (preserveDataDirectory && relative.StartsWith("data", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        var destination = Path.Combine(targetDirectory, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(file, destination, true);
    }
}
