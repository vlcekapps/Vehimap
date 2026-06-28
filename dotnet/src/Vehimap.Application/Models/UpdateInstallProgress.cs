namespace Vehimap.Application.Models;

public sealed record UpdateInstallProgress(
    string Message,
    long BytesReceived = 0,
    long? TotalBytes = null,
    bool IsIndeterminate = false);
