using System.Globalization;
using System.Text;
using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Desktop.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    internal async Task<string?> PickBackupExportPathAsync(CancellationToken cancellationToken = default)
    {
        var suggestedFileName = $"vehimap-{DateTime.Today:yyyy-MM-dd}.vehimapbak";
        return await _fileDialogService
            .PickSaveFileAsync("Export dat Vehimapu", suggestedFileName, "Záloha Vehimap", "vehimapbak", cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string?> PickBackupImportPathAsync(CancellationToken cancellationToken = default)
    {
        return await _fileDialogService
            .PickOpenFileAsync("Import zálohy Vehimapu", "Záloha Vehimap", "vehimapbak", cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string> ExportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Export se nepodařilo připravit, protože nejsou načtená data.";
        }

        await _session.ExportBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
        ShellStatus = $"Záloha byla uložena do {backupPath}.";
        return ShellStatus;
    }

    internal async Task<string> ImportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Obnovu se nepodařilo připravit, protože nejsou načtená data.";
        }

        await _session.RestoreBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
        Load(applyLaunchTabPreference: false);
        ShellStatus = $"Data byla obnovena ze zálohy {backupPath}.";
        return ShellStatus;
    }

    internal DesktopSupportedSettingsSnapshot GetSupportedSettingsSnapshot() =>
        _session.ReadSupportedSettings();

    internal string GetAutomaticBackupStatusText() =>
        _session.BuildAutomaticBackupStatusText();

    internal bool ShouldHideOnLaunch() =>
        _session.ReadSupportedSettings().HideOnLaunch;

    internal async Task SaveSupportedSettingsAsync(DesktopSupportedSettingsSnapshot snapshot)
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        await _session.ApplySupportedSettingsAsync(snapshot).ConfigureAwait(false);
        Load(SelectedVehicle?.Id, SelectedVehicleTabIndex, applyLaunchTabPreference: false);
        ShellStatus = "Nastavení byla uložena a přehledy byly přepočítány.";
    }

    internal async Task<string> CreateAutomaticBackupNowAsync(CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Automatickou zálohu nelze vytvořit bez načtených dat.";
        }

        var result = await _session.CreateAutomaticBackupAsync(cancellationToken).ConfigureAwait(false);
        ShellStatus = result.Message;
        return result.Message;
    }

    internal async Task<AutomaticBackupResult> RunAutomaticBackupCheckAsync(CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return new AutomaticBackupResult(false, true, string.Empty, "Automatickou zálohu nelze vytvořit bez načtených dat.");
        }

        var result = await _session.RunAutomaticBackupCheckAsync(false, cancellationToken).ConfigureAwait(false);
        if (result.Created || result.IsError)
        {
            ShellStatus = result.Message;
        }

        return result;
    }

    internal void ReloadForBackgroundMonitoring() =>
        Load(SelectedVehicle?.Id, SelectedVehicleTabIndex, applyLaunchTabPreference: false);

    internal async Task<string> OpenPrintableVehicleReportAsync(CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            ShellStatus = "Tiskový přehled nelze otevřít bez načtených dat.";
            return ShellStatus;
        }

        try
        {
            var now = DateTime.Now;
            var html = _printableVehicleReportService.BuildHtml(
                _dataSet,
                _metaByVehicleId,
                _timelineService,
                DateOnly.FromDateTime(now),
                now);
            var reportPath = Path.Combine(
                Path.GetTempPath(),
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Vehimap_tiskovy_prehled_{0:yyyyMMdd_HHmmss_fff}.html",
                    now));

            await File.WriteAllTextAsync(reportPath, html, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
            await _fileLauncher.OpenAsync(reportPath, cancellationToken).ConfigureAwait(false);
            ShellStatus = $"Tiskový přehled byl otevřen z dočasného souboru {reportPath}.";
        }
        catch (Exception ex)
        {
            ShellStatus = $"Tiskový přehled se nepodařilo otevřít: {ex.Message}";
        }

        return ShellStatus;
    }

    internal DesktopBackgroundSnapshot BuildBackgroundSnapshot()
    {
        var attentionTimelineItems = _dataSet.Vehicles
            .SelectMany(vehicle => _timelineService.BuildVehicleTimeline(_dataSet, vehicle.Id, DateOnly.FromDateTime(DateTime.Today)))
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Status)
                && !string.Equals(item.Status, "Bez upozornění", StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(item => item.IsFuture)
            .ThenBy(item => item.Date)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.KindLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var toolTipLines = new List<string>
        {
            "Vehimap Desktop",
            $"Vozidla: {VehicleCount} | K řešení: {AuditCount} | Termíny: {DashboardUpcomingTimeline.Count}"
        };

        var firstUpcoming = DashboardUpcomingTimeline.FirstOrDefault();
        if (firstUpcoming is not null)
        {
            toolTipLines.Add($"Nejbližší: {firstUpcoming.VehicleName} - {firstUpcoming.Title} ({firstUpcoming.Date})");
        }

        if (AuditItems.Count > 0)
        {
            var firstAudit = AuditItems[0];
            return new DesktopBackgroundSnapshot(
                string.Join(Environment.NewLine, toolTipLines),
                $"audit|{AuditItems.Count}|{firstAudit.VehicleId}|{firstAudit.EntityKind}|{firstAudit.EntityId}",
                $"Vehimap: {AuditItems.Count} položek k řešení",
                $"{firstAudit.VehicleName}: {firstAudit.Title}. {firstAudit.Message}",
                true);
        }

        if (attentionTimelineItems.Count > 0)
        {
            var firstAttention = attentionTimelineItems[0];
            return new DesktopBackgroundSnapshot(
                string.Join(Environment.NewLine, toolTipLines),
                $"timeline|{attentionTimelineItems.Count}|{firstAttention.VehicleId}|{firstAttention.Kind}|{firstAttention.EntryId}|{firstAttention.Date}",
                $"Vehimap: {attentionTimelineItems.Count} termínů k řešení",
                $"{firstAttention.VehicleName}: {firstAttention.Title} ({firstAttention.Date}). {firstAttention.Status}",
                true);
        }

        return new DesktopBackgroundSnapshot(
            string.Join(Environment.NewLine, toolTipLines),
            "none",
            string.Empty,
            string.Empty,
            false);
    }

    internal void ShowDashboardFromTray()
    {
        SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        RequestFocus(DesktopFocusTarget.SelectedVehicleTabHeader);
    }

    internal AboutDialogViewModel BuildAboutDialogModel()
    {
        var appInfo = _session.GetAppInfo();
        var dataMode = _dataRoot?.IsPortable == true ? "Portable data vedle aplikace" : "Systémová datová složka";
        var dataPath = _dataRoot?.DataPath ?? "Datová složka zatím nebyla načtena";

        return new AboutDialogViewModel(
            appInfo.ApplicationName,
            appInfo.AppVersion,
            appInfo.FileVersion,
            appInfo.RuntimeMode,
            dataPath,
            dataMode,
            appInfo.PlatformDescription,
            appInfo.FrameworkDescription,
            appInfo.ApplicationPath,
            appInfo.ReleaseNotesUrl);
    }

    internal Task OpenExternalAsync(string path, CancellationToken cancellationToken = default) =>
        _fileLauncher.OpenAsync(path, cancellationToken);

    internal Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default) =>
        _session.CheckForUpdatesAsync(cancellationToken);

    internal Task<UpdateInstallResult> PrepareUpdateInstallAsync(UpdateCheckResult result, CancellationToken cancellationToken = default) =>
        _session.PrepareInstallAsync(result, cancellationToken);
}
