// SPDX-License-Identifier: GPL-3.0-or-later
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopSingleInstanceCoordinator : IDisposable
{
    private const string ActivationMessage = "activate";
    private static readonly TimeSpan DefaultSignalTimeout = TimeSpan.FromSeconds(2);

    private readonly Mutex _mutex;
    private readonly string _pipeName;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly Task? _listenerTask;
    private Func<Task>? _activationRequested;
    private int _pendingActivation;
    private bool _disposed;

    private DesktopSingleInstanceCoordinator(Mutex mutex, string pipeName, bool isPrimary)
    {
        _mutex = mutex;
        _pipeName = pipeName;
        IsPrimary = isPrimary;
        if (IsPrimary)
        {
            _listenerTask = Task.Run(ListenForActivationRequestsAsync);
        }
    }

    public bool IsPrimary { get; }

    public static DesktopSingleInstanceCoordinator Acquire(string releaseChannel)
    {
        var names = BuildNames(releaseChannel);
        var mutex = new Mutex(initiallyOwned: true, names.MutexName, out var createdNew);
        return new DesktopSingleInstanceCoordinator(mutex, names.PipeName, createdNew);
    }

    internal static DesktopSingleInstanceNames BuildNames(string releaseChannel, string? userScope = null)
    {
        var token = BuildSafeChannelToken(releaseChannel);
        userScope ??= $"{Environment.UserDomainName}/{Environment.UserName}/{(OperatingSystem.IsWindows() ? System.Diagnostics.Process.GetCurrentProcess().SessionId.ToString() : Environment.GetEnvironmentVariable("DISPLAY"))}";
        var scope = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userScope)))[..16];
        return new DesktopSingleInstanceNames(
            $"Vehimap.Desktop.{token}.{scope}.SingleInstance",
            $"Vehimap.Desktop.{token}.{scope}.Activation");
    }

    public void SetActivationHandler(Func<Task> activationRequested)
    {
        _activationRequested = activationRequested;
        if (Interlocked.Exchange(ref _pendingActivation, 0) == 1)
        {
            _ = InvokeActivationHandlerAsync();
        }
    }

    public void ClearActivationHandler()
    {
        _activationRequested = null;
    }

    public Task<bool> TrySignalExistingInstanceAsync(CancellationToken cancellationToken = default) =>
        TrySignalExistingInstanceAsync(DefaultSignalTimeout, cancellationToken);

    public async Task<bool> TrySignalExistingInstanceAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (IsPrimary)
        {
            return false;
        }

        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                await client.ConnectAsync(200, cancellationToken).ConfigureAwait(false);
                await using var writer = new StreamWriter(client, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: false)
                {
                    AutoFlush = true
                };
                await writer.WriteLineAsync(ActivationMessage).ConfigureAwait(false);
                return true;
            }
            catch (TimeoutException)
            {
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
            catch (IOException)
            {
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
        }

        return false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _activationRequested = null;
        _cancellation.Cancel();
        try
        {
            _listenerTask?.Wait(TimeSpan.FromMilliseconds(500));
        }
        catch
        {
        }

        _cancellation.Dispose();
        if (IsPrimary)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch
            {
            }
        }

        _mutex.Dispose();
    }

    private async Task ListenForActivationRequestsAsync()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.In,
                    maxNumberOfServerInstances: 1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

                await server.WaitForConnectionAsync(_cancellation.Token).ConfigureAwait(false);
                using var readTimeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token);
                readTimeout.CancelAfter(DefaultSignalTimeout);
                var message = new StringBuilder();
                var buffer = new byte[1];
                while (message.Length < 16 && await server.ReadAsync(buffer, readTimeout.Token).ConfigureAwait(false) != 0)
                {
                    if (buffer[0] == '\n') break;
                    if (buffer[0] != '\r') message.Append((char)buffer[0]);
                }
                if (string.Equals(message.ToString(), ActivationMessage, StringComparison.Ordinal))
                {
                    await InvokeActivationHandlerAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                // An idle client must not prevent later activation requests.
            }
            catch (ObjectDisposedException) when (_cancellation.IsCancellationRequested)
            {
                return;
            }
            catch (IOException) when (!_cancellation.IsCancellationRequested)
            {
            }
        }
    }

    private async Task InvokeActivationHandlerAsync()
    {
        var handler = _activationRequested;
        if (handler is null)
        {
            Interlocked.Exchange(ref _pendingActivation, 1);
            return;
        }

        await handler().ConfigureAwait(false);
    }

    private static string BuildSafeChannelToken(string releaseChannel)
    {
        var value = string.IsNullOrWhiteSpace(releaseChannel) ? "stable" : releaseChannel.Trim().ToLowerInvariant();
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        }

        return builder.Length == 0 ? "stable" : builder.ToString();
    }
}

internal sealed record DesktopSingleInstanceNames(string MutexName, string PipeName);
