// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.Services;

public interface IClipboardService
{
    Task SetTextAsync(string text, CancellationToken cancellationToken = default);
}
