// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.Services;

public interface IFilePickerService
{
    Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default);
}
