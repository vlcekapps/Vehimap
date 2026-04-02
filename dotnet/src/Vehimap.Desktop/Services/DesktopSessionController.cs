using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopSessionController
{
    private readonly LegacyVehimapBootstrapper _bootstrapper;
    private readonly ILegacyDataStore _legacyDataStore;
    private readonly IAuditService _auditService;
    private readonly ICostAnalysisService _costAnalysisService;
    private readonly IBackupService _backupService;
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
        _supportedSettingsService = supportedSettingsService;
        _appBuildInfoProvider = appBuildInfoProvider;
        _updateService = updateService;
    }

    public VehimapDataRoot? DataRoot { get; private set; }

    public VehimapDataSet DataSet { get; private set; } = new();

    public IReadOnlyList<AuditItem> AuditItems { get; private set; } = [];

    public IReadOnlyDictionary<string, VehicleMeta> MetaByVehicleId => _metaByVehicleId;

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
        var supportedSettings = _supportedSettingsService.Read(result.DataSet.Settings);

        return new DesktopSessionLoadResult(
            result.DataRoot,
            result.DataSet,
            AuditItems,
            MetaByVehicleId,
            costSummary,
            supportedSettings);
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
        _supportedSettingsService.Read(DataSet.Settings);

    public async Task ApplySupportedSettingsAsync(DesktopSupportedSettingsSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _supportedSettingsService.Apply(DataSet.Settings, snapshot);
        await PersistAsync(cancellationToken).ConfigureAwait(false);
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
}

internal sealed record DesktopSessionLoadResult(
    VehimapDataRoot DataRoot,
    VehimapDataSet DataSet,
    IReadOnlyList<AuditItem> AuditItems,
    IReadOnlyDictionary<string, VehicleMeta> MetaByVehicleId,
    CostAnalysisSummary CostSummary,
    DesktopSupportedSettingsSnapshot SupportedSettings);
