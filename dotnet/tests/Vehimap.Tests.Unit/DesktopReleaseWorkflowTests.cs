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
        Assert.Contains("Get-AhkRetirementReadiness.ps1 -RuntimeIdentifier win-x64 -FailOnBlockers", workflow, StringComparison.Ordinal);
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

    [Fact]
    public void Ahk_retirement_readiness_script_guards_final_removal()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Get-AhkRetirementReadiness.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("latest-dotnet-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-preview-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetReleaseReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("dotnet-v$version", script, StringComparison.Ordinal);
        Assert.Contains("src\\Vehimap.ahk", script, StringComparison.Ordinal);
        Assert.Contains("$FailOnBlockers", script, StringComparison.Ordinal);
        Assert.Contains("AHK zatim nemazat", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Release_tag_script_requires_readiness_and_explicit_push()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "New-DotnetDesktopReleaseTag.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("dotnet-v$version", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetReleaseReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("git @Arguments", script, StringComparison.Ordinal);
        Assert.Contains("status --porcelain", script, StringComparison.Ordinal);
        Assert.Contains("rev-parse origin/main", script, StringComparison.Ordinal);
        Assert.Contains("tag -a $tagName", script, StringComparison.Ordinal);
        Assert.Contains("if ($Push)", script, StringComparison.Ordinal);
        Assert.Contains("push origin $tagName", script, StringComparison.Ordinal);
        Assert.Contains("Dry run OK. Tag nebyl vytvoren.", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Published_release_verification_script_checks_manifest_release_and_retirement_gate()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetPublishedRelease.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("latest-dotnet-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-preview-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("asset_sha256", script, StringComparison.Ordinal);
        Assert.Contains("asset_size", script, StringComparison.Ordinal);
        Assert.Contains("releases/download/$releaseTag", script, StringComparison.Ordinal);
        Assert.Contains("Invoke-RemoteHeadCheck", script, StringComparison.Ordinal);
        Assert.Contains("$SkipNetwork", script, StringComparison.Ordinal);
        Assert.Contains("Get-AhkRetirementReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-FailOnBlockers", script, StringComparison.Ordinal);
        Assert.Contains("publikovany .NET desktop release je overeny", script, StringComparison.Ordinal);
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
