namespace Vehimap.Application.Abstractions;

public interface IAuditService
{
    IReadOnlyList<AuditItem> BuildAudit();
}
