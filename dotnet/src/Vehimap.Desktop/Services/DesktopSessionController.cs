using System.Globalization;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopSessionController
{
    private const string NotificationsSection = "notifications";
    private const string DesktopLastAlertDayKey = "last_desktop_alert_day";
    private const string DesktopLastAlertSignatureKey = "last_desktop_alert_signature";

    private static readonly string[] NotificationHistoryKeys =
    [
        "last_alert_day",
        "last_alert_signature",
        "last_green_alert_day",
        "last_green_alert_signature",
        "last_reminder_alert_day",
        "last_reminder_alert_signature",
        "last_maintenance_alert_day",
        "last_maintenance_alert_signature",
        DesktopLastAlertDayKey,
        DesktopLastAlertSignatureKey
    ];

    private readonly LegacyVehimapBootstrapper _bootstrapper;
    private readonly IVehimapDataStore _dataStore;
    private readonly IAuditService _auditService;
    private readonly ICostAnalysisService _costAnalysisService;
    private readonly IBackupService _backupService;
    private readonly IAutostartService _autostartService;
    private readonly DesktopSupportedSettingsService _supportedSettingsService;
    private readonly IAppBuildInfoProvider _appBuildInfoProvider;
    private readonly IUpdateService _updateService;
    private readonly IFileAttachmentService _attachmentService;
    private readonly IDataStoreHealthService _dataStoreHealthService;
    private readonly Dictionary<string, VehicleMeta> _metaByVehicleId = new(StringComparer.Ordinal);

    public DesktopSessionController(
        LegacyVehimapBootstrapper bootstrapper,
        IVehimapDataStore dataStore,
        IFileAttachmentService attachmentService,
        IAuditService auditService,
        ICostAnalysisService costAnalysisService,
        IBackupService backupService,
        IAutostartService autostartService,
        DesktopSupportedSettingsService supportedSettingsService,
        IAppBuildInfoProvider appBuildInfoProvider,
        IUpdateService updateService,
        IDataStoreHealthService? dataStoreHealthService = null)
    {
        _bootstrapper = bootstrapper;
        _dataStore = dataStore;
        _attachmentService = attachmentService;
        _auditService = auditService;
        _costAnalysisService = costAnalysisService;
        _backupService = backupService;
        _autostartService = autostartService;
        _supportedSettingsService = supportedSettingsService;
        _appBuildInfoProvider = appBuildInfoProvider;
        _updateService = updateService;
        _dataStoreHealthService = dataStoreHealthService ?? new NoOpDataStoreHealthService();
    }

    public VehimapDataRoot? DataRoot { get; private set; }

    public VehimapDataSet DataSet { get; private set; } = new();

    public IReadOnlyList<AuditItem> AuditItems { get; private set; } = [];

    public IReadOnlyDictionary<string, VehicleMeta> MetaByVehicleId => _metaByVehicleId;

    public DataMigrationResult? LastMigrationResult { get; private set; }

    public DataStoreHealthReport? LastDataStoreHealthReport { get; private set; }

    public DesktopSupportedSettingsSnapshot CurrentSupportedSettings { get; private set; } =
        new(30, 30, 31, 1000, false, false, false, false, 1, 30);

    public bool IsLoaded => DataRoot is not null;

    public async Task<DesktopSessionLoadResult> LoadAsync(string appBasePath, CancellationToken cancellationToken = default)
    {
        var result = await _bootstrapper.LoadAsync(appBasePath, cancellationToken).ConfigureAwait(false);
        DataRoot = result.DataRoot;
        DataSet = result.DataSet;
        LastMigrationResult = result.MigrationResult;
        AuditItems = _auditService.BuildAudit(result.DataRoot, result.DataSet);

        RebuildMetaLookup();

        var costSummary = _costAnalysisService.BuildYearToDateSummary(result.DataSet, DateOnly.FromDateTime(DateTime.Today));
        bool? autostartEnabled = null;
        try
        {
            autostartEnabled = await _autostartService.IsEnabledAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            autostartEnabled = null;
        }

        CurrentSupportedSettings = _supportedSettingsService.Read(result.DataSet.Settings, autostartEnabled);
        LastDataStoreHealthReport = await _dataStoreHealthService.CheckAsync(result.DataRoot, cancellationToken).ConfigureAwait(false);

        return new DesktopSessionLoadResult(
            result.DataRoot,
            result.DataSet,
            AuditItems,
            MetaByVehicleId,
            costSummary,
            CurrentSupportedSettings);
    }

    public async Task PersistAsync(CancellationToken cancellationToken = default)
    {
        if (DataRoot is null)
        {
            return;
        }

        await _dataStore.SaveAsync(DataRoot, DataSet, cancellationToken).ConfigureAwait(false);
    }

    public void RestoreDataSet(VehimapDataSet dataSet)
    {
        DataSet = dataSet;
        AuditItems = DataRoot is null ? [] : _auditService.BuildAudit(DataRoot, DataSet);
        RebuildMetaLookup();
        CurrentSupportedSettings = _supportedSettingsService.Read(DataSet.Settings, CurrentSupportedSettings.RunAtStartup);
    }

    private void RebuildMetaLookup()
    {
        _metaByVehicleId.Clear();
        foreach (var meta in DataSet.VehicleMetaEntries.GroupBy(item => item.VehicleId, StringComparer.Ordinal))
        {
            _metaByVehicleId[meta.Key] = meta.First();
        }
    }

    public async Task<BackupExportResult> ExportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (DataRoot is null)
        {
            return new BackupExportResult(backupPath, 0, 0);
        }

        return await _backupService.ExportAsync(backupPath, DataRoot, DataSet, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BackupRestoreResult> RestoreBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (DataRoot is null)
        {
            return new BackupRestoreResult(null, 0);
        }

        var bundle = await _backupService.ImportAsync(backupPath, cancellationToken).ConfigureAwait(false);
        ResetNotificationHistory(bundle.Data.Settings);
        var result = await _backupService.RestoreAsync(DataRoot, bundle, cancellationToken).ConfigureAwait(false);
        RestoreDataSet(CloneDataSet(bundle.Data));
        LastDataStoreHealthReport = await _dataStoreHealthService.CheckAsync(DataRoot, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task<DataStoreHealthReport> CheckDataStoreHealthAsync(CancellationToken cancellationToken = default)
    {
        if (DataRoot is null)
        {
            return new DataStoreHealthReport(
                DataStoreHealthStatus.Error,
                "Datová sada není načtená.",
                ["Kontrolu datové sady 2.0 nelze spustit bez načtené datové složky."],
                string.Empty,
                string.Empty);
        }

        LastDataStoreHealthReport = await _dataStoreHealthService.CheckAsync(DataRoot, cancellationToken).ConfigureAwait(false);
        return LastDataStoreHealthReport;
    }

    public DesktopSupportedSettingsSnapshot ReadSupportedSettings() =>
        CurrentSupportedSettings;

    public CostAnalysisSummary BuildCurrentCostSummary() =>
        _costAnalysisService.BuildYearToDateSummary(DataSet, DateOnly.FromDateTime(DateTime.Today));

    public CostAnalysisSummary BuildCostSummary(DateOnly periodStart, DateOnly periodEnd) =>
        _costAnalysisService.BuildPeriodSummary(DataSet, periodStart, periodEnd);

    public async Task ApplySupportedSettingsAsync(DesktopSupportedSettingsSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _autostartService.SetEnabledAsync(snapshot.RunAtStartup, cancellationToken).ConfigureAwait(false);
        await PersistSettingsMutationAsync(
                settings =>
                {
                    _supportedSettingsService.Apply(settings, snapshot);
                    ResetNotificationHistory(settings);
                },
                cancellationToken)
            .ConfigureAwait(false);
        CurrentSupportedSettings = snapshot;
    }

    public async Task<bool> ShouldShowAndRememberDueNotificationAsync(string notificationKey, DateOnly today, CancellationToken cancellationToken = default)
    {
        if (DataRoot is null || string.IsNullOrWhiteSpace(notificationKey) || string.Equals(notificationKey, "none", StringComparison.Ordinal))
        {
            return false;
        }

        var todayKey = today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var lastDay = DataSet.Settings.GetValue(NotificationsSection, DesktopLastAlertDayKey, string.Empty).Trim();
        var lastSignature = DataSet.Settings.GetValue(NotificationsSection, DesktopLastAlertSignatureKey, string.Empty).Trim();
        if (string.Equals(lastDay, todayKey, StringComparison.Ordinal)
            && string.Equals(lastSignature, notificationKey, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            await PersistSettingsMutationAsync(
                    settings =>
                    {
                        settings.SetValue(NotificationsSection, DesktopLastAlertDayKey, todayKey);
                        settings.SetValue(NotificationsSection, DesktopLastAlertSignatureKey, notificationKey);
                    },
                    cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return false;
        }
    }

    public AppBuildInfo GetAppInfo() => _appBuildInfoProvider.GetCurrent();

    public Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default) =>
        _updateService.CheckForUpdatesAsync(_appBuildInfoProvider.GetCurrent().AppVersion, cancellationToken);

    public Task<UpdateInstallResult> PrepareInstallAsync(
        UpdateCheckResult result,
        IProgress<UpdateInstallProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _updateService.PrepareInstallAsync(result, progress, cancellationToken);

    public string ResolveManagedAttachmentPath(string relativePath)
    {
        if (DataRoot is null || string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        return _attachmentService.ResolveManagedAttachmentPath(DataRoot, relativePath);
    }

    public string BuildAutomaticBackupStatusText()
    {
        var backupDirectoryLabel = GetAutomaticBackupDirectoryLabel();
        var lastStamp = GetAutomaticBackupLastStamp();
        var lastPath = GetAutomaticBackupLastPath();

        if (string.IsNullOrWhiteSpace(lastStamp))
        {
            return $"Automatické zálohy se ukládají do složky {backupDirectoryLabel}. Poslední záloha v této složce zatím nebyla vytvořena.";
        }

        var status = $"Automatické zálohy se ukládají do složky {backupDirectoryLabel}. Poslední záloha v této složce: {FormatAutomaticBackupStamp(lastStamp)}.";
        if (!string.IsNullOrWhiteSpace(lastPath) && File.Exists(lastPath))
        {
            status += $" Soubor je uložen pod názvem {Path.GetFileName(lastPath)}.";
        }

        return status;
    }

    public string GetAutomaticBackupDirectoryPath() =>
        GetAutomaticBackupDirectory();

    public string GetPreMigrationBackupPath()
    {
        var lastRunPath = LastMigrationResult?.PreMigrationBackupPath;
        if (!string.IsNullOrWhiteSpace(lastRunPath))
        {
            return lastRunPath;
        }

        return DataSet.Settings.GetValue("migration", "pre_migration_backup_path", string.Empty).Trim();
    }

    public async Task<AutomaticBackupResult> RunAutomaticBackupCheckAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (DataRoot is null)
        {
            return new AutomaticBackupResult(false, true, string.Empty, "Automatickou zálohu nelze vytvořit bez načtených dat.");
        }

        if (!force && !CurrentSupportedSettings.AutomaticBackupsEnabled)
        {
            return new AutomaticBackupResult(false, false, string.Empty, "Automatické zálohy jsou vypnuté.");
        }

        if (!force && !IsAutomaticBackupDue(DateTime.Now))
        {
            TrimAutomaticBackupFiles();
            return new AutomaticBackupResult(false, false, string.Empty, "Automatická záloha ještě není splatná.");
        }

        return await CreateAutomaticBackupAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<AutomaticBackupResult> CreateAutomaticBackupAsync(CancellationToken cancellationToken = default)
    {
        if (DataRoot is null)
        {
            return new AutomaticBackupResult(false, true, string.Empty, "Automatickou zálohu nelze vytvořit bez načtených dat.");
        }

        try
        {
            var now = DateTime.Now;
            var backupPath = GetAutomaticBackupPath(now);
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
            var exportResult = await _backupService.ExportAsync(backupPath, DataRoot, DataSet, cancellationToken).ConfigureAwait(false);
            await PersistSettingsMutationAsync(
                    settings =>
                    {
                        settings.SetValue("backups", "last_automatic_backup_stamp", now.ToString("yyyyMMddHHmmss"));
                        settings.SetValue("backups", "last_automatic_backup_path", backupPath);
                    },
                    cancellationToken)
                .ConfigureAwait(false);
            TrimAutomaticBackupFiles();
            var message = $"Automatická záloha byla vytvořena do {backupPath}.";
            if (exportResult.IncludedManagedAttachmentCount > 0)
            {
                message += $" Spravovaných příloh v záloze: {exportResult.IncludedManagedAttachmentCount}.";
            }

            if (exportResult.MissingManagedAttachmentCount > 0)
            {
                message += $" Přeskočených chybějících spravovaných příloh: {exportResult.MissingManagedAttachmentCount}.";
            }

            return new AutomaticBackupResult(true, false, backupPath, message);
        }
        catch (Exception ex)
        {
            return new AutomaticBackupResult(false, true, string.Empty, $"Automatická záloha se nepodařila: {ex.Message}");
        }
    }

    private string GetAutomaticBackupDirectory()
    {
        return DataRoot is null
            ? string.Empty
            : Path.Combine(DataRoot.DataPath, "auto-backups");
    }

    private string GetAutomaticBackupDirectoryLabel()
    {
        if (DataRoot is null)
        {
            return "data\\auto-backups";
        }

        return DataRoot.IsPortable
            ? "data\\auto-backups"
            : GetAutomaticBackupDirectory();
    }

    private string GetAutomaticBackupPath(DateTime now)
    {
        return Path.Combine(GetAutomaticBackupDirectory(), $"Vehimap_auto_{now:yyyy-MM-dd_HH-mm-ss}.vehimapbak");
    }

    private string GetAutomaticBackupLastStamp()
    {
        var stamp = DataSet.Settings.GetValue("backups", "last_automatic_backup_stamp", string.Empty).Trim();
        return stamp.Length == 14 && stamp.All(char.IsDigit) ? stamp : string.Empty;
    }

    private string GetAutomaticBackupLastPath() =>
        DataSet.Settings.GetValue("backups", "last_automatic_backup_path", string.Empty).Trim();

    private static string FormatAutomaticBackupStamp(string stamp)
    {
        return DateTime.TryParseExact(stamp, "yyyyMMddHHmmss", null, DateTimeStyles.None, out var parsed)
            ? parsed.ToString("dd.MM.yyyy HH:mm")
            : "zatím nebyla vytvořena";
    }

    private bool IsAutomaticBackupDue(DateTime now)
    {
        var lastStamp = GetAutomaticBackupLastStamp();
        if (string.IsNullOrWhiteSpace(lastStamp))
        {
            return true;
        }

        if (!DateTime.TryParseExact(lastStamp, "yyyyMMddHHmmss", null, DateTimeStyles.None, out var lastBackup))
        {
            return true;
        }

        return (now.Date - lastBackup.Date).TotalDays >= CurrentSupportedSettings.AutomaticBackupIntervalDays;
    }

    private void TrimAutomaticBackupFiles()
    {
        var backupDirectory = GetAutomaticBackupDirectory();
        if (string.IsNullOrWhiteSpace(backupDirectory) || !Directory.Exists(backupDirectory))
        {
            return;
        }

        var files = Directory
            .GetFiles(backupDirectory, "*.vehimapbak", SearchOption.TopDirectoryOnly)
            .OrderByDescending(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToList();

        while (files.Count > CurrentSupportedSettings.AutomaticBackupKeepCount)
        {
            try
            {
                File.Delete(files[^1]);
            }
            catch
            {
            }

            files.RemoveAt(files.Count - 1);
        }
    }

    private async Task PersistSettingsMutationAsync(Action<VehimapSettings> updateSettings, CancellationToken cancellationToken)
    {
        var rollbackDataSet = CloneDataSet(DataSet);
        var rollbackSupportedSettings = CurrentSupportedSettings;

        try
        {
            updateSettings(DataSet.Settings);
            await PersistAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            RestoreDataSet(rollbackDataSet);
            CurrentSupportedSettings = rollbackSupportedSettings;
            throw;
        }
    }

    private static VehimapDataSet CloneDataSet(VehimapDataSet source)
    {
        return new VehimapDataSet
        {
            Settings = CloneSettings(source.Settings),
            Vehicles = [.. source.Vehicles],
            HistoryEntries = [.. source.HistoryEntries],
            FuelEntries = [.. source.FuelEntries],
            Records = [.. source.Records],
            VehicleMetaEntries = [.. source.VehicleMetaEntries],
            Reminders = [.. source.Reminders],
            MaintenancePlans = [.. source.MaintenancePlans]
        };
    }

    private static VehimapSettings CloneSettings(VehimapSettings source)
    {
        var clone = new VehimapSettings();
        foreach (var (section, values) in source.Sections)
        {
            foreach (var (key, value) in values)
            {
                clone.SetValue(section, key, value);
            }
        }

        return clone;
    }

    private static void ResetNotificationHistory(VehimapSettings settings)
    {
        foreach (var key in NotificationHistoryKeys)
        {
            settings.SetValue(NotificationsSection, key, string.Empty);
        }
    }

    private sealed class NoOpDataStoreHealthService : IDataStoreHealthService
    {
        public Task<DataStoreHealthReport> CheckAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DataStoreHealthReport(
                DataStoreHealthStatus.Healthy,
                "Kontrola datové sady 2.0 nebyla v testovací session zapojená.",
                ["Testovací session nepoužila konkrétní health službu."],
                Path.Combine(dataRoot.DataPath, "vehimap.db"),
                dataRoot.DataPath));
        }
    }
}

internal sealed record DesktopSessionLoadResult(
    VehimapDataRoot DataRoot,
    VehimapDataSet DataSet,
    IReadOnlyList<AuditItem> AuditItems,
    IReadOnlyDictionary<string, VehicleMeta> MetaByVehicleId,
    CostAnalysisSummary CostSummary,
    DesktopSupportedSettingsSnapshot SupportedSettings);
