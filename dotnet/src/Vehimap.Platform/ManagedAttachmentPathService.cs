// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Platform;

public sealed class ManagedAttachmentPathService : IFileAttachmentService
{
    public string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath)
    {
        return ManagedAttachmentPathGuard.ResolveManagedAttachmentPath(dataRoot.DataPath, relativePath);
    }
}
