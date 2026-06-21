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
            SetCostExportStatus("Souhrn nákladů zatím není připraven k exportu.");
            return;
        }

        try
        {
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

            SetCostExportStatus(string.IsNullOrWhiteSpace(savedPath)
                ? "Export souhrnu nákladů byl zrušen."
                : $"Souhrn nákladů byl uložen do {savedPath}.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetCostExportStatus($"Export souhrnu nákladů se nepodařil: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanExportSelectedVehicleCost))]
    private async Task ExportSelectedVehicleCostDetailAsync()
    {
        var selectedCostVehicle = CostWorkspace.SelectedDashboardCostVehicle;
        if (_currentCostSummary is null || selectedCostVehicle is null)
        {
            SetCostExportStatus("Nejprve vyberte vozidlo v nákladovém přehledu.");
            return;
        }

        try
        {
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

            SetCostExportStatus(string.IsNullOrWhiteSpace(savedPath)
                ? "Export detailu nákladů byl zrušen."
                : $"Detail nákladů byl uložen do {savedPath}.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetCostExportStatus($"Export detailu nákladů se nepodařil: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanExportSelectedVehicleCost))]
    private async Task ExportSelectedVehicleCostReportAsync()
    {
        var selectedCostVehicle = CostWorkspace.SelectedDashboardCostVehicle;
        if (_currentCostSummary is null || selectedCostVehicle is null)
        {
            SetCostExportStatus("Nejprve vyberte vozidlo v nákladovém přehledu.");
            return;
        }

        try
        {
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
                SetCostExportStatus("Export HTML sestavy nákladů byl zrušen.");
                return;
            }

            try
            {
                await _fileLauncher.OpenAsync(savedPath).ConfigureAwait(false);
                SetCostExportStatus($"HTML sestava nákladů byla uložena do {savedPath}.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                SetCostExportStatus($"HTML sestava nákladů byla uložena do {savedPath}, ale nepodařilo se ji otevřít: {ex.Message}");
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetCostExportStatus($"Export HTML sestavy nákladů se nepodařil: {ex.Message}");
        }
    }

    private void SetCostExportStatus(string status)
    {
        CostWorkspace.CostExportStatus = status;
        ShellStatus = status;
    }
}
