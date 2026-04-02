using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopSessionController
{
    private readonly LegacyVehimapBootstrapper _bootstrapper;
    private readonly ILegacyDataStore _legacyDataStore;
    private readonly IAuditService _auditService;
    private readonly ICostAnalysisService _costAnalysisService;
    private readonly IBackupService _backupService;
    private readonly IAutostartService _autostartService;
    private readonly DesktopSupportedSettingsService _supportedSettingsService;
    private readonly IAppBuildInfoProvider _appBuildInfoProvider;
    private readonly IUpdateService _updateService;
    private readonly IFileAttachmentService _attachmentService;
    private readonly Dictionary<string, VehicleMeta> _metaByVehicleId = new(StringComparer.Ordinal);

    public DesktopSessionController(
        LegacyVehimapBootstrapper bootstrapper,
        ILegacyDataStore legacyDataStore,
        IFileAttachmentService attachmentService,
        IAuditService auditService,
        ICostAnalysisService costAnalysisService,
        IBackupService backupService,
        IAutostartService autostartService,
        DesktopSupportedSettingsService supportedSettingsService,
        IAppBuildInfoProvider appBuildInfoProvider,
        IUpdateService updateService)
    {
        _bootstrapper = bootstrapper;
        _legacyDataStore = legacyDataStore;
        _attachmentService = attachmentService;
        _auditService = auditService;
        _costAnalysisService = costAnalysisService;
        _backupService = backupService;
        _autostartService = autostartService;
        _supportedSettingsService = supportedSettingsService;
        _appBuildInfoProvider = appBuildInfoProvider;
        _updateService = updateService;
    }

    public VehimapDataRoot? DataRoot { get; private set; }

    public VehimapDataSet DataSet { get; private set; } = new();

    public IReadOnlyList<AuditItem> AuditItems { get; private set; } = [];

    public IReadOnlyDictionary<string, VehicleMeta> MetaByVehicleId => _metaByVehicleId;

    public DesktopSupportedSettingsSnapshot CurrentSupportedSettings { get; private set; } =
        new(30, 30, 31, 1000, false, false, false, false, 1, 30);

    public bool IsLoaded => DataRoot is not null;

    public async Task<DesktopSessionLoadResult> LoadAsync(string appBasePath, CancellationToken cancellationToken = default)
    {
        var result = await _bootstrapper.LoadAsync(appBasePath, cancellationToken).ConfigureAwait(false);
        DataRoot = result.DataRoot;
        DataSet = result.DataSet;
        AuditItems = _auditService.BuildAudit(result.DataRoot, result.DataSet);

        _metaByVehicleId.Clear();
        foreach (var meta in result.DataSet.VehicleMetaEntries.GroupBy(item => item.VehicleId, StringComparer.Ordinal))
        {
            _metaByVehicleId[meta.Key] = meta.First();
        }

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

        await _legacyDataStore.SaveAsync(DataRoot, DataSet, cancellationToken).ConfigureAwait(false);
    }

    public async Task ExportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (DataRoot is null)
        {
            return;
        }

        await _backupService.ExportAsync(backupPath, DataRoot, DataSet, cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (DataRoot is null)
        {
            return;
        }

        var bundle = await _backupService.ImportAsync(backupPath, cancellationToken).ConfigureAwait(false);
        await _backupService.RestoreAsync(DataRoot, bundle, cancellationToken).ConfigureAwait(false);
    }

    public DesktopSupportedSettingsSnapshot ReadSupportedSettings() =>
        CurrentSupportedSettings;

    public async Task ApplySupportedSettingsAsync(DesktopSupportedSettingsSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _autostartService.SetEnabledAsync(snapshot.RunAtStartup, cancellationToken).ConfigureAwait(false);
        _supportedSettingsService.Apply(DataSet.Settings, snapshot);
        await PersistAsync(cancellationToken).ConfigureAwait(false);
        CurrentSupportedSettings = snapshot;
    }

    public AppBuildInfo GetAppInfo() => _appBuildInfoProvider.GetCurrent();

    public Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default) =>
        _updateService.CheckForUpdatesAsync(_appBuildInfoProvider.GetCurrent().AppVersion, cancellationToken);

    public Task<UpdateInstallResult> PrepareInstallAsync(UpdateCheckResult result, CancellationToken cancellationToken = default) =>
        _updateService.PrepareInstallAsync(result, cancellationToken);

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
            await _backupService.ExportAsync(backupPath, DataRoot, DataSet, cancellationToken).ConfigureAwait(false);
            DataSet.Settings.SetValue("backups", "last_automatic_backup_stamp", now.ToString("yyyyMMddHHmmss"));
            DataSet.Settings.SetValue("backups", "last_automatic_backup_path", backupPath);
            await PersistAsync(cancellationToken).ConfigureAwait(false);
            TrimAutomaticBackupFiles();
            return new AutomaticBackupResult(true, false, backupPath, $"Automatická záloha byla vytvořena do {backupPath}.");
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
        return DateTime.TryParseExact(stamp, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var parsed)
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

        if (!DateTime.TryParseExact(lastStamp, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var lastBackup))
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
}

internal sealed record DesktopSessionLoadResult(
    VehimapDataRoot DataRoot,
    VehimapDataSet DataSet,
    IReadOnlyList<AuditItem> AuditItems,
    IReadOnlyDictionary<string, VehicleMeta> MetaByVehicleId,
    CostAnalysisSummary CostSummary,
    DesktopSupportedSettingsSnapshot SupportedSettings);
