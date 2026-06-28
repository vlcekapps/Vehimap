using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface ISmartAdvisorService
{
    SmartAdvisorSummary BuildSmartAdvisor(
        VehimapDataSet dataSet,
        IReadOnlyList<AuditItem> auditItems,
        CostAnalysisSummary? costSummary,
        DateOnly today);
}
