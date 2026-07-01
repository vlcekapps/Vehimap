// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Abstractions;

public interface IDataRootLocator
{
    VehimapDataRoot Resolve(string appBasePath);
}
