// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Abstractions;

public interface IFileLauncher
{
    Task OpenAsync(string path, CancellationToken cancellationToken = default);
    Task OpenFolderAsync(string path, CancellationToken cancellationToken = default);
}
