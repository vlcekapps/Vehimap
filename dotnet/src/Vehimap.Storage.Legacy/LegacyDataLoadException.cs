// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Storage.Legacy;

public sealed class LegacyDataLoadException : InvalidOperationException
{
    public LegacyDataLoadException(string fileName, string filePath, string message, Exception innerException)
        : base(message, innerException)
    {
        FileName = fileName;
        FilePath = filePath;
    }

    public string FileName { get; }

    public string FilePath { get; }
}
