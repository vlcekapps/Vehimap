// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.ViewModels;

internal sealed record DesktopBackgroundSnapshot(
    string ToolTipText,
    string NotificationKey,
    string NotificationTitle,
    string NotificationMessage,
    bool HasNotification);
