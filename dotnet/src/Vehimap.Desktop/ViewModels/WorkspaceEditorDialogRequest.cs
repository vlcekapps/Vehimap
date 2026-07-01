// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.ViewModels;

public enum WorkspaceEditorKind
{
    History,
    Fuel,
    Reminder,
    Maintenance,
    Record
}

public sealed record WorkspaceEditorDialogRequest(
    WorkspaceEditorKind Kind,
    DesktopFocusTarget ReturnFocusTarget);
