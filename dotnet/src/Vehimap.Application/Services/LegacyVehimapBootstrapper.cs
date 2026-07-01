// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;

namespace Vehimap.Application.Services;

public sealed class LegacyVehimapBootstrapper
{
    private readonly IDataRootLocator _dataRootLocator;
    private readonly IVehimapDataStore _dataStore;
    private readonly IDataMigrationService? _dataMigrationService;

    public LegacyVehimapBootstrapper(IDataRootLocator dataRootLocator, ILegacyDataStore legacyDataStore)
        : this(dataRootLocator, (IVehimapDataStore)legacyDataStore, null)
    {
    }

    public LegacyVehimapBootstrapper(
        IDataRootLocator dataRootLocator,
        IVehimapDataStore dataStore,
        IDataMigrationService? dataMigrationService = null)
    {
        _dataRootLocator = dataRootLocator;
        _dataStore = dataStore;
        _dataMigrationService = dataMigrationService;
    }

    public async Task<VehimapBootstrapResult> LoadAsync(string appBasePath, CancellationToken cancellationToken = default)
    {
        var dataRoot = _dataRootLocator.Resolve(appBasePath);
        var migrationResult = _dataMigrationService is null
            ? null
            : await _dataMigrationService.MigrateIfNeededAsync(dataRoot, cancellationToken).ConfigureAwait(false);
        var dataSet = await _dataStore.LoadAsync(dataRoot, cancellationToken).ConfigureAwait(false);
        return new VehimapBootstrapResult(dataRoot, dataSet, migrationResult);
    }
}
