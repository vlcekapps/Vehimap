using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    internal string ServiceBookWindowTitle =>
        SelectedVehicle is null
            ? LO("ServiceBook.Window.Title")
            : LFO("ServiceBook.Window.TitleWithVehicle", SelectedVehicle.Name);

    internal ServiceBookWindowViewModel? BuildSelectedVehicleServiceBookModel()
    {
        if (SelectedVehicle is null)
        {
            ShellStatus = LO("ServiceBook.Status.NoVehicle");
            return null;
        }

        var summary = _serviceBookService.BuildVehicleServiceBook(
            _dataSet,
            SelectedVehicle.Id,
            DateOnly.FromDateTime(DateTime.Today));
        var historyItems = summary.HistoryEntries
            .Select(item => new ServiceBookItemViewModel(
                summary.VehicleId,
                "History",
                item.Id,
                LO("ServiceBook.Section.HistoryAndService"),
                item.DateText,
                item.EventType,
                LFO("ServiceBook.Detail.History", item.Odometer, item.Note),
                LFO("ServiceBook.Detail.HistoryCost", item.Cost)))
            .ToList();
        var maintenanceItems = summary.MaintenancePlans
            .Select(item => new ServiceBookItemViewModel(
                summary.VehicleId,
                "Maintenance",
                item.Id,
                LO("ServiceBook.Section.MaintenancePlan"),
                item.Title,
                item.Interval,
                LFO("ServiceBook.Detail.Maintenance", item.LastService, item.Note),
                item.Status))
            .ToList();
        var recordItems = summary.Records
            .Select(item => new ServiceBookItemViewModel(
                summary.VehicleId,
                "Record",
                item.Id,
                LO("ServiceBook.Section.ServiceRecord"),
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
            model => ExportServiceBookHtmlAsync(model),
            DesktopLocalization.Localizer);
    }

    internal bool OpenServiceBookItem(ServiceBookItemViewModel? item)
    {
        if (item is null)
        {
            ShellStatus = LO("ServiceBook.Status.SelectItemFirst");
            return false;
        }

        if (HasPendingEdits)
        {
            ShellStatus = WorkspaceNavigationLockStatus;
            RequestFocus(GetPendingEditFocusTarget());
            return false;
        }

        SelectVehicleAndOpenEntity(item.VehicleId, item.EntityKind, item.EntityId);
        ShellStatus = LFO("ServiceBook.Status.ItemOpened", item.Primary);
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
                    LO("ServiceBook.FileDialog.ExportTitle"),
                    fileName,
                    content,
                    LO("ServiceBook.FileDialog.HtmlFileType"),
                    "html",
                    ["*.html", "*.htm"],
                    cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(savedPath))
            {
                ShellStatus = LO("ServiceBook.Status.ExportCancelled");
                return ShellStatus;
            }

            try
            {
                await _fileLauncher.OpenAsync(savedPath, cancellationToken).ConfigureAwait(false);
                ShellStatus = LFO("ServiceBook.Status.ExportSavedOpened", savedPath);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ShellStatus = LFO("ServiceBook.Status.ExportSavedOpenFailed", savedPath, ex.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = LFO("ServiceBook.Status.ExportFailed", ex.Message);
        }

        return ShellStatus;
    }

    private string BuildServiceBookRecordDetail(ServiceBookRecordEntry item)
    {
        var parts = new List<string>
        {
            LFO("ServiceBook.Detail.Provider", item.Provider),
            LFO("ServiceBook.Detail.Validity", item.Validity),
            LFO("ServiceBook.Detail.Price", item.Price),
            LFO("ServiceBook.Detail.Note", item.Note)
        };

        if (!string.IsNullOrWhiteSpace(item.StoredPath)
            && !string.Equals(item.StoredPath, LO("ServiceBook.Value.NoStoredPath"), StringComparison.CurrentCultureIgnoreCase))
        {
            parts.Add(LFO("ServiceBook.Detail.StoredPath", item.StoredPath));
        }

        return string.Join(". ", parts) + ".";
    }

    private string BuildServiceBookRecordAttachmentStatus(ServiceBookRecordEntry serviceBookRecord)
    {
        var record = _dataSet.Records.FirstOrDefault(item =>
            string.Equals(item.Id, serviceBookRecord.Id, StringComparison.Ordinal));
        if (record is null)
        {
            return LO("ServiceBook.Attachment.RecordMissing");
        }

        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return LO("ServiceBook.Attachment.Empty");
        }

        var resolvedPath = ResolveServiceBookRecordPath(record);
        if (string.IsNullOrWhiteSpace(resolvedPath))
        {
            return record.AttachmentMode == VehicleRecordAttachmentMode.Managed
                ? LO("ServiceBook.Attachment.ManagedResolveFailed")
                : LO("ServiceBook.Attachment.ExternalResolveFailed");
        }

        return File.Exists(resolvedPath)
            ? LFO("ServiceBook.Attachment.Available", resolvedPath)
            : LFO("ServiceBook.Attachment.Missing", resolvedPath);
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
