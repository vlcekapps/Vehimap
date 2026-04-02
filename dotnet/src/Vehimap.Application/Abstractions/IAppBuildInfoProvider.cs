using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IAppBuildInfoProvider
{
    AppBuildInfo GetCurrent();
}
