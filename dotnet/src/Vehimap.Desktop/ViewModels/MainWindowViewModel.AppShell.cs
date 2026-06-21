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

        var result = await _session.ExportBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
        ShellStatus = $"Záloha byla uložena do {backupPath}.";
        if (result.IncludedManagedAttachmentCount > 0)
        {
            ShellStatus += $" Spravovaných příloh v záloze: {result.IncludedManagedAttachmentCount}.";
        }

        if (result.MissingManagedAttachmentCount > 0)
        {
            ShellStatus += $" Přeskočených chybějících spravovaných příloh: {result.MissingManagedAttachmentCount}.";
        }

        return ShellStatus;
    }

    internal async Task<string> ImportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Obnovu se nepodařilo připravit, protože nejsou načtená data.";
        }

        var restoreResult = await _session.RestoreBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
        Load(applyLaunchTabPreference: false);
        ShellStatus = $"Data byla obnovena ze zálohy {backupPath}.";
        if (!string.IsNullOrWhiteSpace(restoreResult.PreRestoreBackupPath))
        {
            ShellStatus += $" Původní data byla před obnovou odložena do {restoreResult.PreRestoreBackupPath}.";
        }

        if (restoreResult.RestoredAttachmentCount > 0)
        {
            ShellStatus += $" Obnoveno spravovaných příloh: {restoreResult.RestoredAttachmentCount}.";
        }

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

    internal async Task SetDashboardShowOnLaunchAsync(bool showDashboardOnLaunch)
    {
        if (!_session.IsLoaded)
        {
            DashboardWorkspace.SyncShowDashboardOnLaunch(showDashboardOnLaunch);
            return;
        }

        var current = _session.ReadSupportedSettings();
        if (current.ShowDashboardOnLaunch == showDashboardOnLaunch)
        {
            DashboardWorkspace.SyncShowDashboardOnLaunch(showDashboardOnLaunch);
            return;
        }

        try
        {
            await SaveSupportedSettingsAsync(current with { ShowDashboardOnLaunch = showDashboardOnLaunch }).ConfigureAwait(false);
            ShellStatus = showDashboardOnLaunch
                ? "Dashboard se bude zobrazovat při startu aplikace."
                : "Dashboard se při startu aplikace nebude otevírat automaticky.";
            DashboardWorkspace.SyncShowDashboardOnLaunch(showDashboardOnLaunch);
        }
        catch (Exception ex)
        {
            DashboardWorkspace.SyncShowDashboardOnLaunch(current.ShowDashboardOnLaunch);
            ShellStatus = $"Volbu dashboardu při startu se nepodařilo uložit: {ex.Message}";
        }
    }

    internal async Task<string> CreateAutomaticBackupNowAsync(CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Automatickou zálohu nelze vytvořit bez načtených dat.";
        }

        if (BlockDataActionIfEditing("vytvořit zálohu"))
        {
            return ShellStatus;
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

    internal Task<bool> ShouldShowAndRememberDueNotificationAsync(string notificationKey, CancellationToken cancellationToken = default)
    {
        return _session.IsLoaded
            ? _session.ShouldShowAndRememberDueNotificationAsync(notificationKey, DateOnly.FromDateTime(DateTime.Today), cancellationToken)
            : Task.FromResult(false);
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
            var fileName = _printableVehicleReportService.BuildFileName(now);
            var reportPath = await _fileSaveService
                .SaveTextAsync(
                    "Uložit tiskový přehled vozidel",
                    fileName,
                    html,
                    "HTML soubor",
                    "html",
                    ["*.html", "*.htm"],
                    cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(reportPath))
            {
                ShellStatus = "Uložení tiskového přehledu bylo zrušeno.";
                return ShellStatus;
            }

            await _fileLauncher.OpenAsync(reportPath, cancellationToken).ConfigureAwait(false);
            ShellStatus = $"Tiskový přehled byl uložen do {reportPath} a otevřen.";
        }
        catch (Exception ex)
        {
            ShellStatus = $"Tiskový přehled se nepodařilo uložit nebo otevřít: {ex.Message}";
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
            .OrderBy(item => item.IsFuture ? 1 : 0)
            .ThenBy(item => item.IsFuture ? item.Date.DayNumber : -item.Date.DayNumber)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.KindLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var toolTipLines = new List<string>
        {
            "Vehimap Desktop",
            $"Vozidla: {VehicleCount} | K řešení: {AuditCount} | Termíny: {DashboardUpcomingTimeline.Count}"
        };

        var firstAttention = attentionTimelineItems.FirstOrDefault();
        if (firstAttention is not null)
        {
            toolTipLines.Add($"Nejbližší k řešení: {firstAttention.VehicleName} - {firstAttention.Title} ({firstAttention.DateText}, {firstAttention.Status})");
        }
        else if (DashboardUpcomingTimeline.FirstOrDefault() is { } firstUpcoming)
        {
            toolTipLines.Add($"Nejbližší: {firstUpcoming.VehicleName} - {firstUpcoming.Title} ({firstUpcoming.Date})");
        }

        if (firstAttention is not null)
        {
            return new DesktopBackgroundSnapshot(
                string.Join(Environment.NewLine, toolTipLines),
                $"timeline|{attentionTimelineItems.Count}|{firstAttention.VehicleId}|{firstAttention.Kind}|{firstAttention.EntryId}|{firstAttention.Date}",
                $"Vehimap: {attentionTimelineItems.Count} termínů k řešení",
                $"{firstAttention.VehicleName}: {firstAttention.Title} ({firstAttention.DateText}). {firstAttention.Status}",
                true);
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

        return new DesktopBackgroundSnapshot(
            string.Join(Environment.NewLine, toolTipLines),
            "none",
            string.Empty,
            string.Empty,
            false);
    }

    internal void ShowDashboardFromTray()
    {
        if (BlockWorkspaceNavigationIfEditing())
        {
            return;
        }

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

    internal async Task<string> OpenDataFolderAsync(CancellationToken cancellationToken = default)
    {
        var dataPath = _dataRoot?.DataPath;
        if (string.IsNullOrWhiteSpace(dataPath))
        {
            ShellStatus = "Datovou složku zatím nelze otevřít, protože data nebyla načtena.";
            return ShellStatus;
        }

        try
        {
            if (BlockDataActionIfEditing("otevřít datovou složku"))
            {
                return ShellStatus;
            }

            Directory.CreateDirectory(dataPath);
            await _fileLauncher.OpenFolderAsync(dataPath, cancellationToken).ConfigureAwait(false);
            ShellStatus = $"Datová složka byla otevřena: {dataPath}.";
        }
        catch (Exception ex)
        {
            ShellStatus = $"Datovou složku se nepodařilo otevřít: {ex.Message}";
        }

        return ShellStatus;
    }

    internal async Task<string> OpenAutomaticBackupFolderAsync(CancellationToken cancellationToken = default)
    {
        var backupDirectory = _session.GetAutomaticBackupDirectoryPath();
        if (string.IsNullOrWhiteSpace(backupDirectory))
        {
            ShellStatus = "Složku automatických záloh zatím nelze otevřít, protože data nebyla načtena.";
            return ShellStatus;
        }

        if (BlockDataActionIfEditing("otevřít složku automatických záloh"))
        {
            return ShellStatus;
        }

        try
        {
            Directory.CreateDirectory(backupDirectory);
            await _fileLauncher.OpenFolderAsync(backupDirectory, cancellationToken).ConfigureAwait(false);
            ShellStatus = $"Složka automatických záloh byla otevřena: {backupDirectory}.";
        }
        catch (Exception ex)
        {
            ShellStatus = $"Složku automatických záloh se nepodařilo otevřít: {ex.Message}";
        }

        return ShellStatus;
    }

    internal Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default) =>
        _session.CheckForUpdatesAsync(cancellationToken);

    internal Task<UpdateInstallResult> PrepareUpdateInstallAsync(UpdateCheckResult result, CancellationToken cancellationToken = default) =>
        _session.PrepareInstallAsync(result, cancellationToken);
}
