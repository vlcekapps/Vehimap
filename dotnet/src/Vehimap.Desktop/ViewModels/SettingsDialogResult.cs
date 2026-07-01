// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed record SettingsDialogResult(
    DesktopSupportedSettingsSnapshot Snapshot,
    bool CreateBackupNow);
