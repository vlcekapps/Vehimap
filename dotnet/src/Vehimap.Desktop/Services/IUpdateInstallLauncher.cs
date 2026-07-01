// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Desktop.Services;

internal interface IUpdateInstallLauncher
{
    void Launch(UpdateInstallPlan plan);
}
