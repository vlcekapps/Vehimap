// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record UpdateInstallProgress(
    string Message,
    long BytesReceived = 0,
    long? TotalBytes = null,
    bool IsIndeterminate = false);
