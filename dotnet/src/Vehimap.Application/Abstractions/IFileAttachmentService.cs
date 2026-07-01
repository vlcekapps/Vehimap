// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Abstractions;

public interface IFileAttachmentService
{
    string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath);
}
