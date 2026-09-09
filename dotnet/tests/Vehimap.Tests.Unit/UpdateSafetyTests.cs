// SPDX-License-Identifier: GPL-3.0-or-later
using System.Net;
using System.Text;
using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed partial class AppShellServicesTests
{
    private AppBuildInfo SafetyBuildInfo(string manifestName = "audit-manifest.ini") => new(
        "Vehimap", "2.0.0", "2.0.0.0", "Published", Path.Combine(_tempRoot, "Vehimap.exe"),
        "Windows", ".NET 10", $"https://example.com/{manifestName}", "https://example.com/releases",
        Path.Combine(_tempRoot, "Vehimap.Updater.exe"), true, "stable");

    private static UpdateCheckResult InstallerUpdate() => new("2.0.0", "2.0.1", true, null, null,
        "https://example.com/setup.exe", new string('a', 64), 16, true, "", AssetKind: "installer");

    [Theory]
    [InlineData("channel")]
    [InlineData("http")]
    [InlineData("hash")]
    [InlineData("kind")]
    [InlineData("unpublished")]
    public async Task Update_install_rejects_unsafe_metadata_without_a_network_request(string invalid)
    {
        var update = InstallerUpdate();
        var info = SafetyBuildInfo();
        switch (invalid)
        {
            case "channel": update = update with { ReleaseChannel = "nightly" }; break;
            case "http": update = update with { AssetUrl = "http://example.com/setup.exe" }; break;
            case "hash": update = update with { Sha256 = new string('z', 64) }; break;
            case "kind": update = update with { AssetKind = "unknown" }; break;
            case "unpublished": info = info with { IsPublishedBuild = false }; break;
        }
        var handler = new CountingUpdateHandler([1, 2, 3]);
        using var http = new HttpClient(handler);
        var result = await new LegacyUpdateService(new StubBuildInfoProvider(info), http, EnglishLocalizer).PrepareInstallAsync(update);
        Assert.False(result.IsReady);
        Assert.Null(result.InstallPlan);
        Assert.Equal(0, handler.Requests);
    }

    [Theory]
    [InlineData("nightly", false)]
    [InlineData("stable", true)]
    public async Task Update_check_accepts_only_the_matching_release_channel(string channel, bool available)
    {
        using var http = new HttpClient(new StubHttpMessageHandler(Encoding.UTF8.GetBytes($"[release]\nversion=2.1.0\nchannel={channel}\n")));
        var result = await new LegacyUpdateService(new StubBuildInfoProvider(SafetyBuildInfo()), http, EnglishLocalizer).CheckForUpdatesAsync("2.0.0");
        Assert.Equal(available, result.IsUpdateAvailable);
        if (available)
        {
            Assert.Equal("2.1.0", result.LatestVersion);
            Assert.Null(result.FailureReason);
        }
        else Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public async Task Published_update_check_ignores_local_manifest_override()
    {
        var name = $"audit-{Guid.NewGuid():N}.ini";
        var path = Path.Combine(AppContext.BaseDirectory, "update", name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        try
        {
            await File.WriteAllTextAsync(path, "[release]\nversion=99.0.0\nchannel=stable\n");
            var handler = new CountingUpdateHandler(Encoding.UTF8.GetBytes("[release]\nversion=2.0.1\nchannel=stable\n"));
            using var http = new HttpClient(handler);
            var result = await new LegacyUpdateService(new StubBuildInfoProvider(SafetyBuildInfo(name)), http, EnglishLocalizer).CheckForUpdatesAsync("2.0.0");
            Assert.Equal("2.0.1", result.LatestVersion);
            Assert.Equal(1, handler.Requests);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task Update_check_propagates_user_cancellation()
    {
        using var http = new HttpClient(new CountingUpdateHandler([]));
        var service = new LegacyUpdateService(new StubBuildInfoProvider(SafetyBuildInfo()), http, EnglishLocalizer);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CheckForUpdatesAsync("2.0.0", new CancellationToken(true)));
    }

    [Fact]
    public async Task Oversized_download_is_stopped_before_reporting_excess_bytes()
    {
        var handler = new CountingUpdateHandler(new byte[200_000], chunked: true);
        using var http = new HttpClient(handler);
        var progress = new ImmediateUpdateProgress();
        var result = await new LegacyUpdateService(new StubBuildInfoProvider(SafetyBuildInfo()), http, EnglishLocalizer)
            .PrepareInstallAsync(InstallerUpdate(), progress);
        Assert.False(result.IsReady);
        Assert.Null(result.InstallPlan);
        Assert.All(progress.Values, item => Assert.InRange(item.BytesReceived, 0, 16));
    }

    private sealed class ImmediateUpdateProgress : IProgress<UpdateInstallProgress>
    {
        public List<UpdateInstallProgress> Values { get; } = [];
        public void Report(UpdateInstallProgress value) => Values.Add(value);
    }

    private sealed class CountingUpdateHandler(byte[] content, bool chunked = false) : HttpMessageHandler
    {
        public int Requests { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests++;
            HttpContent body = chunked ? new UnknownLengthContent(content) : new ByteArrayContent(content);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = body });
        }
    }

    private sealed class UnknownLengthContent(byte[] content) : HttpContent
    {
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(content).AsTask();
    }
}
