namespace Vehimap.Application.Abstractions;

public interface IDataRootLocator
{
    VehimapDataRoot Resolve(string appBasePath);
}
