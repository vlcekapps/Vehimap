// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Abstractions;

public sealed record VehimapDataRoot(
    string AppBasePath,
    string DataPath,
    bool IsPortable);
