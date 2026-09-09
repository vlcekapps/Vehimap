// SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;

namespace Vehimap.Updater;

public static class ArchiveInstaller
{
    private static StringComparison PathComparison => OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public static bool WaitForApplicationExit(int processId, int timeoutMilliseconds = 30_000)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.WaitForExit(timeoutMilliseconds);
        }
        catch (ArgumentException) { return true; } // The process has already exited.
    }

    public static void ValidatePaths(string source, string target, string? entry = null)
    {
        source = Path.GetFullPath(source);
        target = Path.GetFullPath(target);
        if (Contains(source, target) || Contains(target, source))
            throw new ArgumentException("Update source and target must not overlap.");
        RejectLinks(source);
        RejectLinks(target);
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
        if (!string.IsNullOrWhiteSpace(entry))
        {
            var fullEntry = Path.GetFullPath(entry);
            if (!Contains(target, fullEntry) || IsData(Path.GetRelativePath(target, fullEntry))
                || !File.Exists(Path.Combine(source, Path.GetRelativePath(target, fullEntry))))
                throw new ArgumentException("The entry point must be an application file in the update payload.");
        }
    }

    public static void Install(string source, string target)
    {
        ValidatePaths(source, target);
        source = Path.GetFullPath(source);
        target = Path.GetFullPath(target);
        var files = EnumeratePayload(source).Select(path => Path.GetRelativePath(source, path))
            .OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (files.Length == 0) throw new InvalidDataException("The update payload is empty.");
        foreach (var relative in files) RejectLinks(Path.Combine(target, relative));

        // Keep all originals before the first overwrite. A failed rollback retains this directory.
        var backup = Path.Combine(Path.GetDirectoryName(target)!, $".vehimap-update-backup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(backup);
        var originals = new HashSet<string>(files.Where(relative => File.Exists(Path.Combine(target, relative))));
        var touched = new List<string>();
        var safeToDeleteBackup = false;
        try
        {
            foreach (var relative in originals) Copy(Path.Combine(target, relative), Path.Combine(backup, relative));
            try
            {
                foreach (var relative in files)
                {
                    touched.Add(relative);
                    Copy(Path.Combine(source, relative), Path.Combine(target, relative));
                }
            }
            catch
            {
                foreach (var relative in touched.AsEnumerable().Reverse())
                {
                    var destination = Path.Combine(target, relative);
                    if (originals.Contains(relative)) Copy(Path.Combine(backup, relative), destination);
                    else if (File.Exists(destination)) File.Delete(destination);
                }
                safeToDeleteBackup = true;
                throw;
            }
            safeToDeleteBackup = true;
        }
        finally
        {
            if (safeToDeleteBackup)
            {
                try { Directory.Delete(backup, recursive: true); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }

    private static IEnumerable<string> EnumeratePayload(string root, string? directory = null)
    {
        foreach (var path in Directory.EnumerateFileSystemEntries(directory ?? root))
        {
            if (IsData(Path.GetRelativePath(root, path))) continue;
            RejectLinks(path);
            if (Directory.Exists(path))
            {
                foreach (var file in EnumeratePayload(root, path)) yield return file;
            }
            else yield return path;
        }
    }

    private static bool IsData(string relative) =>
        string.Equals(relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0], "data", StringComparison.OrdinalIgnoreCase);

    private static bool Contains(string root, string path) =>
        string.Equals(Path.TrimEndingDirectorySeparator(root), Path.TrimEndingDirectorySeparator(path), PathComparison)
        || path.StartsWith(Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar, PathComparison);

    private static void RejectLinks(string path)
    {
        for (var current = Path.GetFullPath(path); !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            if (Path.Exists(current) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Update paths must not traverse symbolic links or junctions.");
    }

    private static void Copy(string source, string target)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target, overwrite: true);
    }
}
