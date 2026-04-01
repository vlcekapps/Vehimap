namespace Vehimap.Application.Abstractions;

public interface IAuditService
{
    IReadOnlyList<AuditItem> BuildAudit(VehimapDataRoot dataRoot, Vehimap.Domain.Models.VehimapDataSet dataSet);
}
