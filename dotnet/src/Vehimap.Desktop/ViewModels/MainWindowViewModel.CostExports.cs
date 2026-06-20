using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private bool CanExportFleetCostSummary => _currentCostSummary is not null && CostVehicles.Count > 0;

    private bool CanExportSelectedVehicleCost => _currentCostSummary is not null && CostWorkspace.SelectedDashboardCostVehicle is not null;

    [RelayCommand(CanExecute = nameof(CanExportFleetCostSummary))]
    private async Task ExportFleetCostSummaryAsync()
    {
        if (_currentCostSummary is null)
        {
            CostWorkspace.CostExportStatus = "Souhrn nákladů zatím není připraven k exportu.";
            return;
        }

        var content = _costExportService.BuildFleetSummaryTsv(_currentCostSummary);
        var fileName = _costExportService.BuildFleetSummaryFileName(_currentCostSummary);
        var savedPath = await _fileSaveService.SaveTextAsync(
                "Export souhrnu nákladů",
                fileName,
                content,
                "TSV soubor",
                "tsv",
                ["*.tsv"])
            .ConfigureAwait(false);

        CostWorkspace.CostExportStatus = string.IsNullOrWhiteSpace(savedPath)
            ? "Export souhrnu nákladů byl zrušen."
            : $"Souhrn nákladů byl uložen do {savedPath}.";
    }

    [RelayCommand(CanExecute = nameof(CanExportSelectedVehicleCost))]
    private async Task ExportSelectedVehicleCostDetailAsync()
    {
        var selectedCostVehicle = CostWorkspace.SelectedDashboardCostVehicle;
        if (_currentCostSummary is null || selectedCostVehicle is null)
        {
            CostWorkspace.CostExportStatus = "Nejprve vyberte vozidlo v nákladovém přehledu.";
            return;
        }

        var content = _costExportService.BuildVehicleDetailTsv(
            _dataSet,
            selectedCostVehicle.VehicleId,
            _currentCostSummary.PeriodStart,
            _currentCostSummary.PeriodEnd);
        var fileName = _costExportService.BuildVehicleDetailFileName(
            _dataSet,
            selectedCostVehicle.VehicleId,
            _currentCostSummary.PeriodStart,
            _currentCostSummary.PeriodEnd);
        var savedPath = await _fileSaveService.SaveTextAsync(
                "Export detailu nákladů",
                fileName,
                content,
                "TSV soubor",
                "tsv",
                ["*.tsv"])
            .ConfigureAwait(false);

        CostWorkspace.CostExportStatus = string.IsNullOrWhiteSpace(savedPath)
            ? "Export detailu nákladů byl zrušen."
            : $"Detail nákladů byl uložen do {savedPath}.";
    }

    [RelayCommand(CanExecute = nameof(CanExportSelectedVehicleCost))]
    private async Task ExportSelectedVehicleCostReportAsync()
    {
        var selectedCostVehicle = CostWorkspace.SelectedDashboardCostVehicle;
        if (_currentCostSummary is null || selectedCostVehicle is null)
        {
            CostWorkspace.CostExportStatus = "Nejprve vyberte vozidlo v nákladovém přehledu.";
            return;
        }

        var content = _costExportService.BuildVehicleReportHtml(
            _dataSet,
            _currentCostSummary,
            selectedCostVehicle.VehicleId,
            DateTime.Now);
        var fileName = _costExportService.BuildVehicleReportFileName(
            _dataSet,
            selectedCostVehicle.VehicleId,
            _currentCostSummary.PeriodStart,
            _currentCostSummary.PeriodEnd);
        var savedPath = await _fileSaveService.SaveTextAsync(
                "Export HTML sestavy nákladů",
                fileName,
                content,
                "HTML soubor",
                "html",
                ["*.html", "*.htm"])
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(savedPath))
        {
            CostWorkspace.CostExportStatus = "Export HTML sestavy nákladů byl zrušen.";
            return;
        }

        CostWorkspace.CostExportStatus = $"HTML sestava nákladů byla uložena do {savedPath}.";
        await _fileLauncher.OpenAsync(savedPath).ConfigureAwait(false);
    }
}
