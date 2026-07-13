// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;

namespace Vehimap.Mobile.Services;

public interface IMobileDataRootProvider
{
    VehimapDataRoot GetDataRoot();
}

public sealed class MobileDataRootProvider : IMobileDataRootProvider
{
    public VehimapDataRoot GetDataRoot()
    {
        var localDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localDataPath))
        {
            localDataPath = AppContext.BaseDirectory;
        }

        var dataPath = Path.Combine(localDataPath, "Vehimap");
        return new VehimapDataRoot(AppContext.BaseDirectory, dataPath, IsPortable: false);
    }
}
