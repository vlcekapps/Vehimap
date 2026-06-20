namespace Vehimap.Application.Abstractions;

public interface ICostAnalysisService
{
    CostAnalysisSummary BuildYearToDateSummary(Vehimap.Domain.Models.VehimapDataSet dataSet, DateOnly today);

    CostAnalysisSummary BuildPeriodSummary(Vehimap.Domain.Models.VehimapDataSet dataSet, DateOnly periodStart, DateOnly periodEnd);
}
