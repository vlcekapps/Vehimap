using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopReleaseWorkflowTests
{
    [Fact]
    public void Dotnet_desktop_workflow_publishes_stable_release_channel()
    {
        var workflow = ReadWorkflow();

        Assert.Contains("\"dotnet-v*\"", workflow, StringComparison.Ordinal);
        Assert.Contains("release_tag=\"dotnet-v${version}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("release_name=\"Vehimap Desktop ${version}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-\" + $rid + \".ini", workflow, StringComparison.Ordinal);
        Assert.Contains("Write-DotnetUpdateManifest.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains("vehimap-desktop-release-*", workflow, StringComparison.Ordinal);

        Assert.DoesNotContain("dotnet-preview-v", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--draft", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Vehimap Desktop Preview", workflow, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dotnet_desktop_workflow_keeps_preview_manifest_alias_for_existing_preview_builds()
    {
        var workflow = ReadWorkflow();

        Assert.Contains("latest-dotnet-preview-\" + $rid + \".ini", workflow, StringComparison.Ordinal);
        Assert.Contains("Legacy alias for already published desktop preview builds", workflow, StringComparison.Ordinal);
        Assert.Contains("Copy-Item -LiteralPath $outputPath -Destination $legacyPreviewOutputPath -Force", workflow, StringComparison.Ordinal);
    }

    private static string ReadWorkflow()
    {
        var workflowPath = Path.Combine(FindRepositoryRoot(), ".github", "workflows", "dotnet-desktop.yml");
        return File.ReadAllText(workflowPath);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "src", "VERSION"))
                && File.Exists(Path.Combine(current.FullName, ".github", "workflows", "dotnet-desktop.yml")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Nepodarilo se najit koren repozitare Vehimap.");
    }
}
