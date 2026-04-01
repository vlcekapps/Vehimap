using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class ManagedAttachmentPathService : IFileAttachmentService
{
    public string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath)
    {
        var normalized = (relativePath ?? string.Empty).Trim().Replace('\\', '/');

        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        if (normalized.StartsWith("data/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[5..];
        }

        while (normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = normalized[1..];
        }

        return string.IsNullOrWhiteSpace(normalized)
            ? string.Empty
            : Path.Combine(dataRoot.DataPath, normalized.Replace('/', Path.DirectorySeparatorChar));
    }
}
