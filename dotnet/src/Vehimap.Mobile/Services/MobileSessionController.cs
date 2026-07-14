// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;

namespace Vehimap.Mobile.Services;

public sealed class MobileSessionController
{
    private readonly IVehimapDataStore _dataStore;
    private readonly IMobileDataRootProvider _dataRootProvider;
    private readonly IAppCultureService _cultureService;
    private readonly DesktopSupportedSettingsService _settingsService;
    private readonly IFileAttachmentService _attachmentService;

    public MobileSessionController(
        IVehimapDataStore dataStore,
        IMobileDataRootProvider dataRootProvider,
        IAppCultureService cultureService,
        DesktopSupportedSettingsService settingsService,
        IAppLocalizer localizer,
        IFileAttachmentService? attachmentService = null)
    {
        _dataStore = dataStore;
        _dataRootProvider = dataRootProvider;
        _cultureService = cultureService;
        _settingsService = settingsService;
        _attachmentService = attachmentService ?? new MobileManagedAttachmentPathService();
        Localizer = localizer;
        DataRoot = dataRootProvider.GetDataRoot();
        Settings = settingsService.Read(new VehimapSettings());
    }

    public VehimapDataRoot DataRoot { get; }

    public VehimapDataSet DataSet { get; private set; } = new();

    public DesktopSupportedSettingsSnapshot Settings { get; private set; }

    public IAppLocalizer Localizer { get; private set; }

    public IReadOnlyList<AuditItem> AuditItems { get; private set; } = [];

    public SmartAdvisorSummary AdvisorSummary { get; private set; } =
        new(0, 0, 0, 0, string.Empty, []);

    public string ResolveManagedAttachmentPath(string relativePath) =>
        _attachmentService.ResolveManagedAttachmentPath(DataRoot, relativePath);

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        var dataSet = await _dataStore.LoadAsync(DataRoot, cancellationToken).ConfigureAwait(true);
        var settings = _settingsService.Read(dataSet.Settings);
        ConfigureLocalization(settings);

        var timelineService = new LegacyTimelineService(Localizer);
        timelineService.ApplySupportedSettings(settings);
        var fuelAnalysisService = new LegacyFuelAnalysisService(Localizer);
        fuelAnalysisService.ApplySupportedSettings(settings);
        var auditItems = new LegacyAuditService(_attachmentService, Localizer)
            .BuildAudit(DataRoot, dataSet);
        var advisorSummary = new LegacySmartAdvisorService(
                timelineService,
                fuelAnalysisService,
                Localizer)
            .BuildSmartAdvisor(dataSet, auditItems, costSummary: null, DateOnly.FromDateTime(DateTime.Today));

        DataSet = dataSet;
        Settings = settings;
        AuditItems = auditItems;
        AdvisorSummary = advisorSummary;
    }

    private void ConfigureLocalization(DesktopSupportedSettingsSnapshot settings)
    {
        var preferences = new AppCulturePreferences(
            settings.Language,
            settings.ThousandsSeparator,
            settings.DecimalSeparator);
        _cultureService.ApplyThreadCulture(preferences);
        Localizer = new ResourceAppLocalizer(_cultureService.ResolveCulture(preferences.Language));
    }
}

internal sealed class MobileManagedAttachmentPathService : IFileAttachmentService
{
    public string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath) =>
        ManagedAttachmentPathGuard.ResolveManagedAttachmentPath(dataRoot.DataPath, relativePath);
}
