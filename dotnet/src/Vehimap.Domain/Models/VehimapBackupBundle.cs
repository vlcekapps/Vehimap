namespace Vehimap.Domain.Models;

public sealed record VehimapBackupBundle(
    VehimapDataSet Data,
    IReadOnlyList<ManagedAttachment> Attachments);
