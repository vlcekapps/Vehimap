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
            .PickSaveFileAsync(
                LO("AppShell.FileDialog.BackupExportTitle"),
                suggestedFileName,
                LO("AppShell.FileDialog.BackupFileType"),
                "vehimapbak",
                cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string?> PickBackupImportPathAsync(CancellationToken cancellationToken = default)
    {
        return await _fileDialogService
            .PickOpenFileAsync(
                LO("AppShell.FileDialog.BackupImportTitle"),
                LO("AppShell.FileDialog.BackupFileType"),
                "vehimapbak",
                cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string?> PickVehiclePackageExportPathAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedVehicle is null)
        {
            return null;
        }

        var suggestedFileName = $"{LO("AppShell.FileName.VehiclePackagePrefix")}-{BuildSafeFileName(SelectedVehicle.Name)}.vehimapvehicle";
        return await _fileDialogService
            .PickSaveFileAsync(
                LO("AppShell.FileDialog.VehiclePackageExportTitle"),
                suggestedFileName,
                LO("AppShell.FileDialog.VehiclePackageFileType"),
                "vehimapvehicle",
                cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string?> PickVehiclePackageImportPathAsync(CancellationToken cancellationToken = default)
    {
        return await _fileDialogService
            .PickOpenFileAsync(
                LO("AppShell.FileDialog.VehiclePackageImportTitle"),
                LO("AppShell.FileDialog.VehiclePackageFileType"),
                "vehimapvehicle",
                cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string> ExportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return LO("AppShell.ExportBackup.NotLoaded");
        }

        try
        {
            var result = await _session.ExportBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
            ShellStatus = LFO("AppShell.ExportBackup.Success", backupPath);
            if (result.IncludedManagedAttachmentCount > 0)
            {
                ShellStatus += " " + LFO("AppShell.ExportBackup.IncludedManagedAttachments", result.IncludedManagedAttachmentCount);
            }

            if (result.MissingManagedAttachmentCount > 0)
            {
                ShellStatus += " " + LFO("AppShell.ExportBackup.MissingManagedAttachments", result.MissingManagedAttachmentCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = LFO("AppShell.ExportBackup.Failed", ex.Message);
        }

        return ShellStatus;
    }

    internal async Task<string> ImportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return LO("AppShell.ImportBackup.NotLoaded");
        }

        try
        {
            var restoreResult = await _session.RestoreBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
            RefreshShellFromSessionState(applyLaunchTabPreference: false);
            ShellStatus = LFO("AppShell.ImportBackup.Success", backupPath);
            if (!string.IsNullOrWhiteSpace(restoreResult.PreRestoreBackupPath))
            {
                ShellStatus += " " + LFO("AppShell.ImportBackup.PreRestoreBackup", restoreResult.PreRestoreBackupPath);
            }

            if (restoreResult.RestoredAttachmentCount > 0)
            {
                ShellStatus += " " + LFO("AppShell.ImportBackup.RestoredManagedAttachments", restoreResult.RestoredAttachmentCount);
            }

            RequestBackgroundRefresh();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = LFO("AppShell.ImportBackup.Failed", ex.Message);
        }

        return ShellStatus;
    }

    internal async Task<DataStoreHealthReport> CheckDataStoreHealthAsync(CancellationToken cancellationToken = default)
    {
        if (BlockDataActionIfEditing(LO("AppShell.DataStoreHealth.Action")))
        {
            return new DataStoreHealthReport(
                DataStoreHealthStatus.Warning,
                LO("AppShell.DataStoreHealth.DeferredByPendingEdits"),
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
            ShellStatus = LO("AppShell.VehiclePackage.ExportNoVehicle");
            return ShellStatus;
        }

        if (_dataRoot is null)
        {
            ShellStatus = LO("AppShell.VehiclePackage.ExportNotLoaded");
            return ShellStatus;
        }

        if (BlockDataActionIfEditing(LO("AppShell.VehiclePackage.ExportAction")))
        {
            return ShellStatus;
        }

        try
        {
            var result = await _vehiclePackageService
                .ExportVehicleAsync(packagePath, _dataRoot, _dataSet, SelectedVehicle.Id, cancellationToken)
                .ConfigureAwait(false);
            ShellStatus = LFO("AppShell.VehiclePackage.ExportSuccess", result.VehicleName, result.PackagePath);
            if (result.IncludedAttachmentCount > 0)
            {
                ShellStatus += " " + LFO("AppShell.VehiclePackage.IncludedAttachments", result.IncludedAttachmentCount);
            }

            if (result.MissingAttachmentCount > 0)
            {
                ShellStatus += " " + LFO("AppShell.VehiclePackage.MissingAttachments", result.MissingAttachmentCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = LFO("AppShell.VehiclePackage.ExportFailed", ex.Message);
        }

        return ShellStatus;
    }

    internal async Task<string> ImportVehiclePackageAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        if (_dataRoot is null)
        {
            ShellStatus = LO("AppShell.VehiclePackage.ImportNotLoaded");
            return ShellStatus;
        }

        if (BlockDataActionIfEditing(LO("AppShell.VehiclePackage.ImportAction")))
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
            ShellStatus = LFO("AppShell.VehiclePackage.ImportSuccess", result.ImportedVehicleName);
            if (result.RestoredAttachmentCount > 0)
            {
                ShellStatus += " " + LFO("AppShell.VehiclePackage.RestoredAttachments", result.RestoredAttachmentCount);
            }

            RequestBackgroundRefresh();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = LFO("AppShell.VehiclePackage.ImportFailed", ex.Message);
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
                ? LO("AppShell.Dashboard.ShowOnLaunchEnabled")
                : LO("AppShell.Dashboard.ShowOnLaunchDisabled");
            DashboardWorkspace.SyncShowDashboardOnLaunch(showDashboardOnLaunch);
        }
        catch (Exception ex)
        {
            DashboardWorkspace.SyncShowDashboardOnLaunch(current.ShowDashboardOnLaunch);
            ShellStatus = LFO("AppShell.Dashboard.ShowOnLaunchFailed", ex.Message);
        }
    }

    internal async Task<string> CreateAutomaticBackupNowAsync(CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return LO("AppShell.AutomaticBackup.NotLoaded");
        }

        if (BlockDataActionIfEditing(LO("AppShell.AutomaticBackup.CreateAction")))
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
            return new AutomaticBackupResult(false, true, string.Empty, LO("AppShell.AutomaticBackup.NotLoaded"));
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
            ShellStatus = LO("AppShell.PrintableReport.NotLoaded");
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
                    LO("AppShell.FileDialog.PrintableReportTitle"),
                    fileName,
                    html,
                    LO("AppShell.FileDialog.HtmlFileType"),
                    "html",
                    ["*.html", "*.htm"],
                    cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(reportPath))
            {
                ShellStatus = LO("AppShell.PrintableReport.SaveCancelled");
                return ShellStatus;
            }

            await _fileLauncher.OpenAsync(reportPath, cancellationToken).ConfigureAwait(false);
            ShellStatus = LFO("AppShell.PrintableReport.SavedAndOpened", reportPath);
        }
        catch (Exception ex)
        {
            ShellStatus = LFO("AppShell.PrintableReport.Failed", ex.Message);
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
            LFO("AppShell.Background.TooltipSummary", VehicleCount, AuditCount, DashboardUpcomingTimeline.Count)
        };

        var firstAttention = attentionTimelineItems.FirstOrDefault();
        if (firstAttention is not null)
        {
            toolTipLines.Add(LFO("AppShell.Background.NearestAttention", firstAttention.VehicleName, firstAttention.Title, firstAttention.Date, firstAttention.Status));
        }
        else if (DashboardUpcomingTimeline.FirstOrDefault() is { } firstUpcoming)
        {
            toolTipLines.Add(LFO("AppShell.Background.NearestUpcoming", firstUpcoming.VehicleName, firstUpcoming.Title, firstUpcoming.Date));
        }

        if (firstAttention is not null)
        {
            return new DesktopBackgroundSnapshot(
                string.Join(Environment.NewLine, toolTipLines),
                $"timeline|{attentionTimelineItems.Count}|{firstAttention.VehicleId}|{firstAttention.Kind}|{firstAttention.EntryId}|{firstAttention.Date}",
                LFO("AppShell.Background.NotificationTimelineTitle", attentionTimelineItems.Count),
                LFO("AppShell.Background.NotificationTimelineMessage", firstAttention.VehicleName, firstAttention.Title, firstAttention.Date, firstAttention.Status),
                true);
        }

        if (AuditItems.Count > 0)
        {
            var firstAudit = AuditItems[0];
            return new DesktopBackgroundSnapshot(
                string.Join(Environment.NewLine, toolTipLines),
                $"audit|{AuditItems.Count}|{firstAudit.VehicleId}|{firstAudit.EntityKind}|{firstAudit.EntityId}",
                LFO("AppShell.Background.NotificationAuditTitle", AuditItems.Count),
                LFO("AppShell.Background.NotificationAuditMessage", firstAudit.VehicleName, firstAudit.Title, firstAudit.Message),
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
            .Where(item => IsTimelineStatusAttention(item.Status))
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
        var dataMode = _dataRoot?.IsPortable == true ? LO("AppShell.About.DataModePortable") : LO("AppShell.About.DataModeSystem");
        var dataPath = _dataRoot?.DataPath ?? LO("AppShell.About.DataPathNotLoaded");

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
            ShellStatus = LO("AppShell.External.Empty");
            return ShellStatus;
        }

        try
        {
            await _fileLauncher.OpenAsync(path, cancellationToken).ConfigureAwait(false);
            ShellStatus = LFO("AppShell.External.Opened", path);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShellStatus = LFO("AppShell.External.Failed", ex.Message);
        }

        return ShellStatus;
    }

    internal async Task<string> OpenDataFolderAsync(CancellationToken cancellationToken = default)
    {
        var dataPath = _dataRoot?.DataPath;
        if (string.IsNullOrWhiteSpace(dataPath))
        {
            ShellStatus = LO("AppShell.DataFolder.NotLoaded");
            return ShellStatus;
        }

        try
        {
            if (BlockDataActionIfEditing(LO("AppShell.DataFolder.Action")))
            {
                return ShellStatus;
            }

            Directory.CreateDirectory(dataPath);
            await _fileLauncher.OpenFolderAsync(dataPath, cancellationToken).ConfigureAwait(false);
            ShellStatus = LFO("AppShell.DataFolder.Opened", dataPath);
        }
        catch (Exception ex)
        {
            ShellStatus = LFO("AppShell.DataFolder.Failed", ex.Message);
        }

        return ShellStatus;
    }

    internal async Task<string> OpenAutomaticBackupFolderAsync(CancellationToken cancellationToken = default)
    {
        var backupDirectory = _session.GetAutomaticBackupDirectoryPath();
        if (string.IsNullOrWhiteSpace(backupDirectory))
        {
            ShellStatus = LO("AppShell.AutomaticBackupFolder.NotLoaded");
            return ShellStatus;
        }

        if (BlockDataActionIfEditing(LO("AppShell.AutomaticBackupFolder.Action")))
        {
            return ShellStatus;
        }

        try
        {
            Directory.CreateDirectory(backupDirectory);
            await _fileLauncher.OpenFolderAsync(backupDirectory, cancellationToken).ConfigureAwait(false);
            ShellStatus = LFO("AppShell.AutomaticBackupFolder.Opened", backupDirectory);
        }
        catch (Exception ex)
        {
            ShellStatus = LFO("AppShell.AutomaticBackupFolder.Failed", ex.Message);
        }

        return ShellStatus;
    }

    internal async Task<string> OpenPreMigrationBackupFolderAsync(CancellationToken cancellationToken = default)
    {
        var backupDirectory = _session.GetPreMigrationBackupPath();
        if (string.IsNullOrWhiteSpace(backupDirectory) || !Directory.Exists(backupDirectory))
        {
            ShellStatus = LO("AppShell.PreMigrationBackupFolder.NotAvailable");
            return ShellStatus;
        }

        if (BlockDataActionIfEditing(LO("AppShell.PreMigrationBackupFolder.Action")))
        {
            return ShellStatus;
        }

        try
        {
            await _fileLauncher.OpenFolderAsync(backupDirectory, cancellationToken).ConfigureAwait(false);
            ShellStatus = LFO("AppShell.PreMigrationBackupFolder.Opened", backupDirectory);
        }
        catch (Exception ex)
        {
            ShellStatus = LFO("AppShell.PreMigrationBackupFolder.Failed", ex.Message);
        }

        return ShellStatus;
    }

    private static string BuildDataStoreHealthShellMessage(DataStoreHealthReport report, bool manual)
    {
        return report.Status switch
        {
            DataStoreHealthStatus.Healthy when manual => LO("AppShell.DataStoreHealth.HealthyManual"),
            DataStoreHealthStatus.Healthy => LO("AppShell.DataStoreHealth.Healthy"),
            _ => LFO("AppShell.DataStoreHealth.Summary", report.Summary)
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
            ShellStatus = LFO("AppShell.Update.CheckFailed", ex.Message);
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
            ShellStatus = LFO("AppShell.Update.PrepareInstallFailed", ex.Message);
            return new UpdateInstallResult(false, ShellStatus, null);
        }
    }

    private static string BuildSafeFileName(string value)
    {
        var safeName = string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        return string.IsNullOrWhiteSpace(safeName)
            ? LO("AppShell.FileName.VehicleFallback")
            : safeName;
    }
}
