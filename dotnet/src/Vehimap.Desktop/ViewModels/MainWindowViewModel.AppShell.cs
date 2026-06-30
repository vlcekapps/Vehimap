using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;
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

    internal async Task<string?> PickVehiclePackageExportPathAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedVehicle is null)
        {
            return null;
        }

        var suggestedFileName = $"vehimap-vozidlo-{BuildSafeFileName(SelectedVehicle.Name)}.vehimapvehicle";
        return await _fileDialogService
            .PickSaveFileAsync("Export balíčku vozidla", suggestedFileName, "Balíček vozidla Vehimap", "vehimapvehicle", cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string?> PickVehiclePackageImportPathAsync(CancellationToken cancellationToken = default)
    {
        return await _fileDialogService
            .PickOpenFileAsync("Import balíčku vozidla", "Balíček vozidla Vehimap", "vehimapvehicle", cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string> ExportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Export se nepodařilo připravit, protože nejsou načtená data.";
        }

        try
        {
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
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = $"Export zálohy se nepodařil: {ex.Message}";
        }

        return ShellStatus;
    }

    internal async Task<string> ImportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Obnovu se nepodařilo připravit, protože nejsou načtená data.";
        }

        try
        {
            var restoreResult = await _session.RestoreBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
            RefreshShellFromSessionState(applyLaunchTabPreference: false);
            ShellStatus = $"Data byla obnovena ze zálohy {backupPath}.";
            if (!string.IsNullOrWhiteSpace(restoreResult.PreRestoreBackupPath))
            {
                ShellStatus += $" Původní data byla před obnovou odložena do {restoreResult.PreRestoreBackupPath}.";
            }

            if (restoreResult.RestoredAttachmentCount > 0)
            {
                ShellStatus += $" Obnoveno spravovaných příloh: {restoreResult.RestoredAttachmentCount}.";
            }

            RequestBackgroundRefresh();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = $"Obnova ze zálohy se nepodařila: {ex.Message}";
        }

        return ShellStatus;
    }

    internal async Task<DataStoreHealthReport> CheckDataStoreHealthAsync(CancellationToken cancellationToken = default)
    {
        if (BlockDataActionIfEditing("zkontrolovat datovou sadu 2.0"))
        {
            return new DataStoreHealthReport(
                DataStoreHealthStatus.Warning,
                "Kontrola datové sady byla odložena kvůli rozpracovaným úpravám.",
                [ShellStatus],
                _dataRoot is null ? string.Empty : Path.Combine(_dataRoot.DataPath, "vehimap.db"),
                _dataRoot?.DataPath ?? string.Empty,
                _session.GetPreMigrationBackupPath());
        }

        var report = await _session.CheckDataStoreHealthAsync(cancellationToken).ConfigureAwait(false);
        ShellStatus = BuildDataStoreHealthShellMessage(report, manual: true);
        return report;
    }

    internal async Task<string> ExportSelectedVehiclePackageAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        if (SelectedVehicle is null)
        {
            ShellStatus = "Balíček vozidla nelze exportovat, protože není vybrané žádné vozidlo.";
            return ShellStatus;
        }

        if (_dataRoot is null)
        {
            ShellStatus = "Balíček vozidla nelze exportovat, protože nejsou načtená data.";
            return ShellStatus;
        }

        if (BlockDataActionIfEditing("exportovat balíček vozidla"))
        {
            return ShellStatus;
        }

        try
        {
            var result = await _vehiclePackageService
                .ExportVehicleAsync(packagePath, _dataRoot, _dataSet, SelectedVehicle.Id, cancellationToken)
                .ConfigureAwait(false);
            ShellStatus = $"Balíček vozidla {result.VehicleName} byl uložen do {result.PackagePath}.";
            if (result.IncludedAttachmentCount > 0)
            {
                ShellStatus += $" Přiložených spravovaných příloh: {result.IncludedAttachmentCount}.";
            }

            if (result.MissingAttachmentCount > 0)
            {
                ShellStatus += $" Přeskočených chybějících příloh: {result.MissingAttachmentCount}.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = $"Export balíčku vozidla se nepodařil: {ex.Message}";
        }

        return ShellStatus;
    }

    internal async Task<string> ImportVehiclePackageAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        if (_dataRoot is null)
        {
            ShellStatus = "Balíček vozidla nelze importovat, protože nejsou načtená data.";
            return ShellStatus;
        }

        if (BlockDataActionIfEditing("importovat balíček vozidla"))
        {
            return ShellStatus;
        }

        try
        {
            var result = await _vehiclePackageService
                .ImportVehicleAsync(packagePath, _dataRoot, _dataSet, cancellationToken)
                .ConfigureAwait(false);
            _session.RestoreDataSet(result.DataSet);
            await _session.PersistAsync(cancellationToken).ConfigureAwait(false);
            RefreshShellFromSessionState(result.ImportedVehicleId, DetailTabIndex, applyLaunchTabPreference: false);
            ShellStatus = $"Balíček vozidla {result.ImportedVehicleName} byl importován.";
            if (result.RestoredAttachmentCount > 0)
            {
                ShellStatus += $" Obnoveno spravovaných příloh: {result.RestoredAttachmentCount}.";
            }

            RequestBackgroundRefresh();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = $"Import balíčku vozidla se nepodařil: {ex.Message}";
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

        var previous = _session.ReadSupportedSettings();
        await _session.ApplySupportedSettingsAsync(snapshot).ConfigureAwait(false);
        Load(SelectedVehicle?.Id, SelectedVehicleTabIndex, applyLaunchTabPreference: false);
        NotifyEditorUnitMetadataChanged();
        ShellStatus = string.Equals(previous.Language, snapshot.Language, StringComparison.OrdinalIgnoreCase)
            ? DesktopLocalization.Localizer.GetString("Shell.SettingsSaved")
            : DesktopLocalization.Localizer.GetString("Settings.RestartRequiredStatus");
        RequestBackgroundRefresh();
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
        RequestBackgroundRefresh();
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
        var attentionTimelineItems = BuildBackgroundAttentionItems();
        var appName = _session.GetAppInfo().ApplicationName;

        var toolTipLines = new List<string>
        {
            $"{appName} Desktop",
            $"Vozidla: {VehicleCount} | K řešení: {AuditCount} | Termíny: {DashboardUpcomingTimeline.Count}"
        };

        var firstAttention = attentionTimelineItems.FirstOrDefault();
        if (firstAttention is not null)
        {
            toolTipLines.Add($"Nejbližší k řešení: {firstAttention.VehicleName} - {firstAttention.Title} ({firstAttention.Date}, {firstAttention.Status})");
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
                $"{firstAttention.VehicleName}: {firstAttention.Title} ({firstAttention.Date}). {firstAttention.Status}",
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

    private List<VehicleTimelineItemViewModel> BuildBackgroundAttentionItems()
    {
        return _dataSet.Vehicles
            .SelectMany(vehicle => _timelineService.BuildVehicleTimeline(_dataSet, vehicle.Id, DateOnly.FromDateTime(DateTime.Today)))
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Status)
                && !string.Equals(item.Status, "Bez upozornění", StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(item => item.IsFuture ? 1 : 0)
            .ThenBy(item => item.IsFuture ? item.Date.DayNumber : -item.Date.DayNumber)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.KindLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(CreateTimelineItemViewModel)
            .ToList();
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
            appInfo.ReleaseNotesUrl,
            appInfo.ReleaseChannel,
            DesktopLocalization.Localizer);
    }

    internal string BuildFeedbackIssueUrl()
    {
        var appInfo = _session.GetAppInfo();
        return FeedbackIssueUrlBuilder.Build(appInfo, DataMode, VehicleCount, AuditCount);
    }

    internal async Task<string> OpenExternalAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            ShellStatus = "Externí odkaz nelze otevřít, protože není vyplněná cesta ani URL.";
            return ShellStatus;
        }

        try
        {
            await _fileLauncher.OpenAsync(path, cancellationToken).ConfigureAwait(false);
            ShellStatus = $"Externí odkaz byl otevřen: {path}.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = $"Externí odkaz se nepodařilo otevřít: {ex.Message}";
        }

        return ShellStatus;
    }

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

    internal async Task<string> OpenPreMigrationBackupFolderAsync(CancellationToken cancellationToken = default)
    {
        var backupDirectory = _session.GetPreMigrationBackupPath();
        if (string.IsNullOrWhiteSpace(backupDirectory) || !Directory.Exists(backupDirectory))
        {
            ShellStatus = "Složku předmigrační zálohy zatím nelze otevřít, protože žádná migrace v této datové sadě nebyla zaznamenána.";
            return ShellStatus;
        }

        if (BlockDataActionIfEditing("otevřít složku předmigrační zálohy"))
        {
            return ShellStatus;
        }

        try
        {
            await _fileLauncher.OpenFolderAsync(backupDirectory, cancellationToken).ConfigureAwait(false);
            ShellStatus = $"Složka předmigrační zálohy byla otevřena: {backupDirectory}.";
        }
        catch (Exception ex)
        {
            ShellStatus = $"Složku předmigrační zálohy se nepodařilo otevřít: {ex.Message}";
        }

        return ShellStatus;
    }

    private static string BuildDataStoreHealthShellMessage(DataStoreHealthReport report, bool manual)
    {
        return report.Status switch
        {
            DataStoreHealthStatus.Healthy when manual => "Kontrola datové sady 2.0 proběhla v pořádku.",
            DataStoreHealthStatus.Healthy => "Datová sada 2.0 je v pořádku.",
            _ => $"Kontrola datové sady 2.0: {report.Summary}"
        };
    }

    internal async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var appInfo = _session.GetAppInfo();
        try
        {
            var result = await _session.CheckForUpdatesAsync(cancellationToken).ConfigureAwait(false);
            ShellStatus = result.Message;
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = $"Kontrola aktualizací se nepodařila: {ex.Message}";
            return new UpdateCheckResult(
                appInfo.AppVersion,
                appInfo.AppVersion,
                false,
                null,
                appInfo.ReleaseNotesUrl,
                null,
                null,
                null,
                false,
                ShellStatus,
                ShellStatus);
        }
    }

    internal async Task<UpdateInstallResult> PrepareUpdateInstallAsync(
        UpdateCheckResult result,
        IProgress<UpdateInstallProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var installResult = await _session.PrepareInstallAsync(result, progress, cancellationToken).ConfigureAwait(false);
            ShellStatus = installResult.Message;
            return installResult;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = $"Příprava instalace aktualizace se nepodařila: {ex.Message}";
            return new UpdateInstallResult(false, ShellStatus, null);
        }
    }

    private static string BuildSafeFileName(string value)
    {
        var safeName = string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        return string.IsNullOrWhiteSpace(safeName)
            ? "vozidlo"
            : safeName;
    }
}
