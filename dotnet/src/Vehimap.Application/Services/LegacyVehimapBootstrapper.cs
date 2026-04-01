using Vehimap.Application.Abstractions;

namespace Vehimap.Application.Services;

public sealed class LegacyVehimapBootstrapper
{
    private readonly IDataRootLocator _dataRootLocator;
    private readonly ILegacyDataStore _legacyDataStore;

    public LegacyVehimapBootstrapper(IDataRootLocator dataRootLocator, ILegacyDataStore legacyDataStore)
    {
        _dataRootLocator = dataRootLocator;
        _legacyDataStore = legacyDataStore;
    }

    public async Task<VehimapBootstrapResult> LoadAsync(string appBasePath, CancellationToken cancellationToken = default)
    {
        var dataRoot = _dataRootLocator.Resolve(appBasePath);
        var dataSet = await _legacyDataStore.LoadAsync(dataRoot, cancellationToken).ConfigureAwait(false);
        return new VehimapBootstrapResult(dataRoot, dataSet);
    }
}
