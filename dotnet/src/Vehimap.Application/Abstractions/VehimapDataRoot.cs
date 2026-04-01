namespace Vehimap.Application.Abstractions;

public sealed record VehimapDataRoot(
    string AppBasePath,
    string DataPath,
    bool IsPortable);
