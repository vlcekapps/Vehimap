// SPDX-License-Identifier: GPL-3.0-or-later
using System.IO.Compression;

namespace Vehimap.Application.Services;

public static class AtomicArchiveWriter
{
    public static void CreateFromDirectory(string sourceDirectory, string archivePath, CancellationToken cancellationToken = default)
    {
        var targetPath = Path.GetFullPath(archivePath);
        var parent = Path.GetDirectoryName(targetPath)!;
        Directory.CreateDirectory(parent);
        var temporaryPath = Path.Combine(parent, $".vehimap-archive-{Guid.NewGuid():N}.tmp");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            ZipFile.CreateFromDirectory(sourceDirectory, temporaryPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            SafeDataArchive.Validate(temporaryPath, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, targetPath, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }
}
