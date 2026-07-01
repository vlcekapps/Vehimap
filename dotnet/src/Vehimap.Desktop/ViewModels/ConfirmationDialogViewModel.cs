// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.ViewModels;

public sealed record ConfirmationDialogViewModel(
    string Title,
    string Message,
    string ConfirmLabel,
    string CancelLabel);
