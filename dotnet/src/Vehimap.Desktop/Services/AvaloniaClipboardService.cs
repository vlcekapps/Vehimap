// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using AvaloniaApp = Avalonia.Application;

namespace Vehimap.Desktop.Services;

public sealed class AvaloniaClipboardService : IClipboardService
{
    public async Task SetTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (AvaloniaApp.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow }
            || mainWindow.Clipboard is null)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await mainWindow.Clipboard.SetTextAsync(text).ConfigureAwait(true);
    }
}
