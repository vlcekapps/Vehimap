// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Abstractions;

public interface ISystemResumeService : IAsyncDisposable
{
    event EventHandler? Resumed;

    Task InitializeAsync(CancellationToken cancellationToken = default);
}
