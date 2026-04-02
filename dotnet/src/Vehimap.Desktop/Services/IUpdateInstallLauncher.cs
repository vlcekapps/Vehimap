using Vehimap.Application.Models;

namespace Vehimap.Desktop.Services;

internal interface IUpdateInstallLauncher
{
    void Launch(UpdateInstallPlan plan);
}
