using Vehimap.Application.Models;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    internal string ServiceBookWindowTitle =>
        SelectedVehicle is null
            ? "Servisní knížka"
            : $"Servisní knížka - {SelectedVehicle.Name}";

    internal ServiceBookWindowViewModel? BuildSelectedVehicleServiceBookModel()
    {
        if (SelectedVehicle is null)
        {
            ShellStatus = "Servisní knížku nelze otevřít bez vybraného vozidla.";
            return null;
        }

        var summary = _serviceBookService.BuildVehicleServiceBook(
            _dataSet,
            SelectedVehicle.Id,
            DateOnly.FromDateTime(DateTime.Today));
        var historyItems = summary.HistoryEntries
            .Select(item => new ServiceBookItemViewModel(
                summary.VehicleId,
                "Historie",
                item.Id,
                "Historie a servis",
                item.DateText,
                item.EventType,
                $"Tachometr {item.Odometer}. Poznámka {item.Note}.",
                $"Cena {item.Cost}"))
            .ToList();
        var maintenanceItems = summary.MaintenancePlans
            .Select(item => new ServiceBookItemViewModel(
                summary.VehicleId,
                "Údržba",
                item.Id,
                "Servisní plán",
                item.Title,
                item.Interval,
                $"Poslední servis {item.LastService}. Poznámka {item.Note}.",
                item.Status))
            .ToList();
        var recordItems = summary.Records
            .Select(item => new ServiceBookItemViewModel(
                summary.VehicleId,
                "Doklad",
                item.Id,
                "Servisní doklad",
                item.Title,
                item.RecordType,
                BuildServiceBookRecordDetail(item),
                BuildServiceBookRecordAttachmentStatus(item)))
            .ToList();

        return new ServiceBookWindowViewModel(
            summary,
            historyItems,
            maintenanceItems,
            recordItems,
            OpenServiceBookItem,
            model => ExportServiceBookHtmlAsync(model));
    }

    internal bool OpenServiceBookItem(ServiceBookItemViewModel? item)
    {
        if (item is null)
        {
            ShellStatus = "Nejprve vyberte položku servisní knížky.";
            return false;
        }

        if (HasPendingEdits)
        {
            ShellStatus = WorkspaceNavigationLockStatus;
            RequestFocus(GetPendingEditFocusTarget());
            return false;
        }

        SelectVehicleAndOpenEntity(item.VehicleId, item.EntityKind, item.EntityId);
        ShellStatus = $"Otevřena položka servisní knížky: {item.Primary}.";
        return true;
    }

    internal async Task<string> ExportServiceBookHtmlAsync(
        ServiceBookWindowViewModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.Now;
            var content = _serviceBookExportService.BuildHtml(
                model.Summary,
                model.HistoryItems,
                model.MaintenanceItems,
                model.RecordItems,
                now);
            var fileName = _serviceBookExportService.BuildFileName(model.Summary, now);
            var savedPath = await _fileSaveService.SaveTextAsync(
                    "Export servisní knížky",
                    fileName,
                    content,
                    "HTML soubor",
                    "html",
                    ["*.html", "*.htm"],
                    cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(savedPath))
            {
                ShellStatus = "Export servisní knížky byl zrušen.";
                return ShellStatus;
            }

            try
            {
                await _fileLauncher.OpenAsync(savedPath, cancellationToken).ConfigureAwait(false);
                ShellStatus = $"Servisní knížka byla uložena do {savedPath} a otevřena.";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ShellStatus = $"Servisní knížka byla uložena do {savedPath}, ale nepodařilo se ji otevřít: {ex.Message}";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = $"Export servisní knížky se nepodařil: {ex.Message}";
        }

        return ShellStatus;
    }

    private string BuildServiceBookRecordDetail(ServiceBookRecordEntry item)
    {
        var parts = new List<string>
        {
            $"Poskytovatel {item.Provider}",
            $"Platnost {item.Validity}",
            $"Cena {item.Price}",
            $"Poznámka {item.Note}"
        };

        if (!string.IsNullOrWhiteSpace(item.StoredPath) && !string.Equals(item.StoredPath, "bez uložené cesty", StringComparison.CurrentCultureIgnoreCase))
        {
            parts.Add($"Uložená cesta {item.StoredPath}");
        }

        return string.Join(". ", parts) + ".";
    }

    private string BuildServiceBookRecordAttachmentStatus(ServiceBookRecordEntry serviceBookRecord)
    {
        var record = _dataSet.Records.FirstOrDefault(item =>
            string.Equals(item.Id, serviceBookRecord.Id, StringComparison.Ordinal));
        if (record is null)
        {
            return "Doklad už není v datech dostupný.";
        }

        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return "Příloha není vyplněná.";
        }

        var resolvedPath = ResolveServiceBookRecordPath(record);
        if (string.IsNullOrWhiteSpace(resolvedPath))
        {
            return record.AttachmentMode == VehicleRecordAttachmentMode.Managed
                ? "Spravovanou přílohu se nepodařilo vyřešit."
                : "Externí cestu se nepodařilo vyřešit.";
        }

        return File.Exists(resolvedPath)
            ? $"Příloha je dostupná: {resolvedPath}"
            : $"Příloha není dostupná: {resolvedPath}";
    }

    private string ResolveServiceBookRecordPath(VehicleRecord record)
    {
        if (_dataRoot is null || string.IsNullOrWhiteSpace(record.FilePath))
        {
            return string.Empty;
        }

        if (record.AttachmentMode == VehicleRecordAttachmentMode.Managed)
        {
            return ResolveManagedAttachmentAbsolutePath(record.FilePath);
        }

        return Path.IsPathRooted(record.FilePath)
            ? record.FilePath
            : Path.GetFullPath(Path.Combine(_dataRoot.AppBasePath, record.FilePath));
    }
}
