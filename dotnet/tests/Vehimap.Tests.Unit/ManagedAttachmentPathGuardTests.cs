// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Services;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class ManagedAttachmentPathGuardTests
{
    [Theory]
    [InlineData("attachments/veh_1/file.pdf", "attachments/veh_1/file.pdf")]
    [InlineData("./attachments/veh_1/file.pdf", "attachments/veh_1/file.pdf")]
    [InlineData("data/attachments/veh_1/file.pdf", "attachments/veh_1/file.pdf")]
    [InlineData(@"attachments\veh_1\file.pdf", "attachments/veh_1/file.pdf")]
    public void Normalize_attachment_relative_path_accepts_safe_managed_paths(string input, string expected)
    {
        Assert.Equal(expected, ManagedAttachmentPathGuard.NormalizeAttachmentRelativePath(input));
    }

    [Theory]
    [InlineData("../x")]
    [InlineData("attachments/../x")]
    [InlineData(@"C:\x")]
    [InlineData(@"\\server\share\x")]
    [InlineData("/tmp/x")]
    [InlineData("data/../../x")]
    [InlineData("external/file.pdf")]
    public void Normalize_attachment_relative_path_rejects_unsafe_or_non_managed_paths(string input)
    {
        Assert.Throws<InvalidDataException>(() => ManagedAttachmentPathGuard.NormalizeAttachmentRelativePath(input));
    }

    [Fact]
    public void Resolve_managed_attachment_path_stays_under_data_root()
    {
        var root = Path.Combine(Path.GetTempPath(), "vehimap-path-guard-" + Guid.NewGuid());
        var resolved = ManagedAttachmentPathGuard.ResolveManagedAttachmentPath(root, "attachments/veh_1/file.pdf");

        Assert.Equal(Path.Combine(root, "attachments", "veh_1", "file.pdf"), resolved);
    }
}
