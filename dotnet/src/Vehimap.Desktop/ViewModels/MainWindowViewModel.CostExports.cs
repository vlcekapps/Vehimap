// SPDX-License-Identifier: GPL-3.0-or-later
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
            SetCostExportStatus(LO("CostExport.FleetNotReady"));
            return;
        }

        try
        {
            var content = _costExportService.BuildFleetSummaryTsv(_currentCostSummary);
            var fileName = _costExportService.BuildFleetSummaryFileName(_currentCostSummary);
            var savedPath = await _fileSaveService.SaveTextAsync(
                    LO("CostExport.FleetSummaryTitle"),
                    fileName,
                    content,
                    LO("CostExport.TsvFileType"),
                    "tsv",
                    ["*.tsv"])
                .ConfigureAwait(false);

            SetCostExportStatus(string.IsNullOrWhiteSpace(savedPath)
                ? LO("CostExport.FleetCancelled")
                : LFO("CostExport.FleetSaved", savedPath));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetCostExportStatus(LFO("CostExport.FleetFailed", ex.Message));
        }
    }

    [RelayCommand(CanExecute = nameof(CanExportSelectedVehicleCost))]
    private async Task ExportSelectedVehicleCostDetailAsync()
    {
        var selectedCostVehicle = CostWorkspace.SelectedDashboardCostVehicle;
        if (_currentCostSummary is null || selectedCostVehicle is null)
        {
            SetCostExportStatus(LO("CostExport.SelectVehicleFirst"));
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
                    LO("CostExport.VehicleDetailTitle"),
                    fileName,
                    content,
                    LO("CostExport.TsvFileType"),
                    "tsv",
                    ["*.tsv"])
                .ConfigureAwait(false);

            SetCostExportStatus(string.IsNullOrWhiteSpace(savedPath)
                ? LO("CostExport.DetailCancelled")
                : LFO("CostExport.DetailSaved", savedPath));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetCostExportStatus(LFO("CostExport.DetailFailed", ex.Message));
        }
    }

    [RelayCommand(CanExecute = nameof(CanExportSelectedVehicleCost))]
    private async Task ExportSelectedVehicleCostReportAsync()
    {
        var selectedCostVehicle = CostWorkspace.SelectedDashboardCostVehicle;
        if (_currentCostSummary is null || selectedCostVehicle is null)
        {
            SetCostExportStatus(LO("CostExport.SelectVehicleFirst"));
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
                    LO("CostExport.VehicleReportTitle"),
                    fileName,
                    content,
                    LO("CostExport.HtmlFileType"),
                    "html",
                    ["*.html", "*.htm"])
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(savedPath))
            {
                SetCostExportStatus(LO("CostExport.ReportCancelled"));
                return;
            }

            try
            {
                await _fileLauncher.OpenAsync(savedPath).ConfigureAwait(false);
                SetCostExportStatus(LFO("CostExport.ReportSavedOpened", savedPath));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                SetCostExportStatus(LFO("CostExport.ReportSavedOpenFailed", savedPath, ex.Message));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetCostExportStatus(LFO("CostExport.ReportFailed", ex.Message));
        }
    }

    private void SetCostExportStatus(string status)
    {
        CostWorkspace.CostExportStatus = status;
        ShellStatus = status;
    }
}
