namespace Vehimap.Application.Models;

public sealed record UpdateInstallPlan(
    string UpdaterPath,
    string SourceDirectory,
    string TargetDirectory,
    string EntryPath,
    int ProcessId,
    string ExpectedVersion);
