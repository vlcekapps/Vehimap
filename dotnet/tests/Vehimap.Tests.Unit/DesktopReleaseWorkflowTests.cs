using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopReleaseWorkflowTests
{
    [Fact]
    public void Dotnet_desktop_workflow_publishes_stable_release_channel()
    {
        var workflow = ReadWorkflow();

        Assert.Contains("\"dotnet-v*\"", workflow, StringComparison.Ordinal);
        Assert.Contains("\"dotnet-beta-v*\"", workflow, StringComparison.Ordinal);
        Assert.Contains("\"dotnet-nightly\"", workflow, StringComparison.Ordinal);
        Assert.Contains("release_tag=\"dotnet-v${version}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("release_name=\"Vehimap Desktop ${version}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("release_name=\"Vehimap Desktop Beta ${version}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("release_name=\"Vehimap Desktop Nightly ${effective_version}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-\" + $rid + \".ini", workflow, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-\" + $channel + \"-\" + $rid + \".ini", workflow, StringComparison.Ordinal);
        Assert.Contains("Write-DotnetUpdateManifest.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedRelease.ps1 -RuntimeIdentifier win-x64 -Channel \"${{ needs.metadata.outputs.channel }}\" -SkipNetwork", workflow, StringComparison.Ordinal);
        Assert.Contains("vehimap-desktop-release-*", workflow, StringComparison.Ordinal);
        Assert.Contains("choco install innosetup", workflow, StringComparison.Ordinal);
        Assert.Contains("-p:VehimapReleaseChannel=${{ needs.metadata.outputs.channel }}", workflow, StringComparison.Ordinal);
        Assert.Contains("--prerelease", workflow, StringComparison.Ordinal);
        Assert.Contains("timeout-minutes: 25", workflow, StringComparison.Ordinal);
        Assert.Contains("timeout-minutes: 15", workflow, StringComparison.Ordinal);

        Assert.DoesNotContain("dotnet-preview-v", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--draft", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Vehimap Desktop Preview", workflow, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dotnet_desktop_workflow_publishes_unique_rolling_nightly_versions()
    {
        var workflow = ReadWorkflow();

        Assert.Contains("effective_version: ${{ steps.version.outputs.effective_version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("effective_version=\"${version}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("effective_version=\"${version}-nightly.${GITHUB_RUN_NUMBER}.${GITHUB_RUN_ATTEMPT}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("if [[ \"${GITHUB_EVENT_NAME}\" == \"workflow_dispatch\" ]]; then", workflow, StringComparison.Ordinal);
        Assert.Contains("if [[ \"${channel}\" == \"nightly\" ]]; then", workflow, StringComparison.Ordinal);
        Assert.Contains("should_release=\"true\"", workflow, StringComparison.Ordinal);
        Assert.Contains("echo \"effective_version=${effective_version}\" >> \"$GITHUB_OUTPUT\"", workflow, StringComparison.Ordinal);
        Assert.Contains("-p:VehimapVersion=${{ needs.metadata.outputs.effective_version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("-Version \"${{ needs.metadata.outputs.effective_version }}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("for ${{ needs.metadata.outputs.effective_version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("release_tag=\"dotnet-nightly\"", workflow, StringComparison.Ordinal);

        Assert.DoesNotContain("release_name=\"Vehimap Desktop Nightly ${version}\"", workflow, StringComparison.Ordinal);
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
    public void Dotnet_desktop_workflow_verifies_generated_windows_manifest_before_commit()
    {
        var workflow = ReadWorkflow();

        Assert.Contains("Verify generated Windows desktop manifest", workflow, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedRelease.ps1 -RuntimeIdentifier win-x64 -Channel \"${{ needs.metadata.outputs.channel }}\" -SkipNetwork", workflow, StringComparison.Ordinal);
        Assert.Contains("Commit desktop manifests", workflow, StringComparison.Ordinal);
        Assert.True(
            workflow.IndexOf("Verify generated Windows desktop manifest", StringComparison.Ordinal) <
            workflow.IndexOf("Commit desktop manifests", StringComparison.Ordinal),
            "Generated manifest verification must run before committing update manifests.");
        Assert.DoesNotContain("Verify AHK retirement readiness after manifest generation", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void Dotnet_desktop_workflow_smokes_windows_installer_before_upload()
    {
        var workflow = ReadWorkflow();

        Assert.Contains("Smoke test Windows installer package", workflow, StringComparison.Ordinal);
        Assert.Contains("if: startsWith(matrix.rid, 'win-')", workflow, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetInstallerSmoke.ps1 -InstallerPath $installerPath -PackageMetadataPath $metadata.FullName", workflow, StringComparison.Ordinal);
        Assert.Contains("Upload packaged artifact", workflow, StringComparison.Ordinal);
        Assert.True(
            workflow.IndexOf("Smoke test Windows installer package", StringComparison.Ordinal) <
            workflow.IndexOf("Upload packaged artifact", StringComparison.Ordinal),
            "Windows installer smoke must run before uploading packaged artifacts.");
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
        Assert.Contains("vehimap-desktop-stable-$version-$RuntimeIdentifier-setup.exe", script, StringComparison.Ordinal);
        Assert.Contains("artifacts\\stable\\$RuntimeIdentifier\\app\\Vehimap.Desktop.exe", script, StringComparison.Ordinal);
        Assert.Contains("Lokalni stable desktop build existuje", script, StringComparison.Ordinal);
        Assert.Contains("asset_kind", script, StringComparison.Ordinal);
        Assert.Contains("channel", script, StringComparison.Ordinal);
        Assert.Contains("Stabilni manifest neobsahuje platny SHA-256 hash.", script, StringComparison.Ordinal);
        Assert.DoesNotContain("artifacts\\desktop-release\\Vehimap.Desktop.exe", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Add-Blocker \"Stabilni manifest nema channel=stable.\"\n    }\n    else", NormalizeLineEndings(script), StringComparison.Ordinal);
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
        Assert.Contains("dotnet-beta-v$version", script, StringComparison.Ordinal);
        Assert.Contains("dotnet-nightly", script, StringComparison.Ordinal);
        Assert.Contains("[ValidateSet(\"stable\", \"beta\", \"nightly\")]", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetReleaseReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetReleasePromotion.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-Channel $Channel", script, StringComparison.Ordinal);
        Assert.Contains("-TargetChannel $Channel", script, StringComparison.Ordinal);
        Assert.Contains("-FailOnBlockers", script, StringComparison.Ordinal);
        Assert.Contains("git @Arguments", script, StringComparison.Ordinal);
        Assert.Contains("status --porcelain", script, StringComparison.Ordinal);
        Assert.Contains("rev-parse origin/main", script, StringComparison.Ordinal);
        Assert.Contains("tag -a $tagName", script, StringComparison.Ordinal);
        Assert.Contains("if ($Push)", script, StringComparison.Ordinal);
        Assert.Contains("push origin $tagName", script, StringComparison.Ordinal);
        Assert.Contains("Dry run OK. Tag nebyl vytvoren.", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Release_readiness_script_is_channel_aware_for_nightly()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetReleaseReadiness.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("[ValidateSet(\"stable\", \"beta\", \"nightly\")]", script, StringComparison.Ordinal);
        Assert.Contains("[string]$EffectiveVersion", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$InstallSmoke", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$AllowLocalInstallSmoke", script, StringComparison.Ordinal);
        Assert.Contains("[int]$InstallerSmokeLaunchSeconds", script, StringComparison.Ordinal);
        Assert.Contains("$version-nightly.local.$timestamp", script, StringComparison.Ordinal);
        Assert.Contains("\"nightly\" { \"dotnet-nightly\" }", script, StringComparison.Ordinal);
        Assert.Contains("\"latest-dotnet-$channelName-$RuntimeIdentifier.ini\"", script, StringComparison.Ordinal);
        Assert.Contains("\"artifacts\\$channelName\\$RuntimeIdentifier\"", script, StringComparison.Ordinal);
        Assert.Contains("Join-Path $readinessRoot \"app\"", script, StringComparison.Ordinal);
        Assert.Contains("Artifact root:", script, StringComparison.Ordinal);
        Assert.Contains("App:", script, StringComparison.Ordinal);
        Assert.Contains("\"-p:VehimapReleaseChannel=$channelName\"", script, StringComparison.Ordinal);
        Assert.Contains("\"-p:VehimapVersion=$effectiveVersion\"", script, StringComparison.Ordinal);
        Assert.Contains("-Version $effectiveVersion", script, StringComparison.Ordinal);
        Assert.Contains("-Channel $channelName", script, StringComparison.Ordinal);
        Assert.Contains("Update manifest neobsahuje ocekavany kanal '$channelName'.", script, StringComparison.Ordinal);
        Assert.Contains("Update manifest neobsahuje platny SHA-256 hash assetu.", script, StringComparison.Ordinal);
        Assert.Contains("if ($channelName -eq \"stable\" -and $manifestContent -match \"preview\")", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetInstallerSmoke.ps1", script, StringComparison.Ordinal);
        Assert.Contains("$installerSmokeArguments", script, StringComparison.Ordinal);
        Assert.Contains("$installerSmokeArguments[\"Install\"] = $true", script, StringComparison.Ordinal);
        Assert.Contains("$installerSmokeArguments[\"AllowLocalInstall\"] = $true", script, StringComparison.Ordinal);
        Assert.Contains("$installerSmokeArguments[\"LaunchSeconds\"] = $InstallerSmokeLaunchSeconds", script, StringComparison.Ordinal);
        Assert.DoesNotContain("artifacts\\release-readiness\\$channelName", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Desktop_ui_test_configuration_prefers_channel_artifacts_for_local_smoke()
    {
        var configurationPath = Path.Combine(FindRepositoryRoot(), "dotnet", "tests", "Vehimap.Tests.UI", "DesktopUiTestConfiguration.cs");
        var configuration = File.ReadAllText(configurationPath);

        Assert.Contains("new[] { \"nightly\", \"beta\", \"stable\" }", configuration, StringComparison.Ordinal);
        Assert.Contains("\"artifacts\", channel, \"win-x64\", \"app\", \"Vehimap.Desktop.exe\"", configuration, StringComparison.Ordinal);
        Assert.Contains("\"desktop-release\", \"Vehimap.Desktop.exe\"", configuration, StringComparison.Ordinal);
    }

    [Fact]
    public void Nightly_readiness_wrapper_uses_nightly_channel()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetNightlyReadiness.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Test-DotnetReleaseReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Channel = \"nightly\"", script, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"EffectiveVersion\"] = $EffectiveVersion", script, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"SkipTests\"] = $true", script, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"InstallSmoke\"] = $true", script, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"AllowLocalInstallSmoke\"] = $true", script, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"InstallerSmokeLaunchSeconds\"] = $InstallerSmokeLaunchSeconds", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Release_promotion_script_guards_beta_and_stable_promotions()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetReleasePromotion.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("[ValidateSet(\"beta\", \"stable\")]", script, StringComparison.Ordinal);
        Assert.Contains("dotnet-nightly", script, StringComparison.Ordinal);
        Assert.Contains("dotnet-beta-v$version", script, StringComparison.Ordinal);
        Assert.Contains("dotnet-v$version", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedRelease.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Read-KeyValueManifest", script, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-nightly-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-beta-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("Zdrojovy nightly manifest ma publikovanou prerelease verzi", script, StringComparison.Ordinal);
        Assert.Contains("Zdrojovy beta manifest ma verzi $version.", script, StringComparison.Ordinal);
        Assert.Contains("Zdrojovy manifest neukazuje na ocekavany release asset $sourceTag.", script, StringComparison.Ordinal);
        Assert.Contains("Zdrojovy manifest neobsahuje platny SHA-256 hash.", script, StringComparison.Ordinal);
        Assert.Contains("status --porcelain", script, StringComparison.Ordinal);
        Assert.Contains("rev-parse origin/main", script, StringComparison.Ordinal);
        Assert.Contains("ls-remote --tags origin", script, StringComparison.Ordinal);
        Assert.Contains("New-DotnetDesktopReleaseTag.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-Channel $TargetChannel -Push", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetBetaReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetStableReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Doporucena lokalni kontrola:", script, StringComparison.Ordinal);
        Assert.Contains("-File .\\build\\$readinessWrapperScriptName -RuntimeIdentifier $RuntimeIdentifier", script, StringComparison.Ordinal);
        Assert.Contains("Pred beta releasem musi existovat rolling nightly tag", script, StringComparison.Ordinal);
        Assert.Contains("Pred stable releasem musi existovat beta tag", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Beta lze stale vytvorit po lokalni readiness gate", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Beta_and_stable_readiness_wrappers_use_dedicated_channels()
    {
        var betaScriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetBetaReadiness.ps1");
        var stableScriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetStableReadiness.ps1");
        var betaScript = File.ReadAllText(betaScriptPath);
        var stableScript = File.ReadAllText(stableScriptPath);

        Assert.Contains("Test-DotnetReleaseReadiness.ps1", betaScript, StringComparison.Ordinal);
        Assert.Contains("Channel = \"beta\"", betaScript, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"SkipTests\"] = $true", betaScript, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"InstallSmoke\"] = $true", betaScript, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"AllowLocalInstallSmoke\"] = $true", betaScript, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"InstallerSmokeLaunchSeconds\"] = $InstallerSmokeLaunchSeconds", betaScript, StringComparison.Ordinal);

        Assert.Contains("Test-DotnetReleaseReadiness.ps1", stableScript, StringComparison.Ordinal);
        Assert.Contains("Channel = \"stable\"", stableScript, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"SkipTests\"] = $true", stableScript, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"InstallSmoke\"] = $true", stableScript, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"AllowLocalInstallSmoke\"] = $true", stableScript, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"InstallerSmokeLaunchSeconds\"] = $InstallerSmokeLaunchSeconds", stableScript, StringComparison.Ordinal);
    }

    [Fact]
    public void Windows_hardening_script_orchestrates_nightly_gate_before_beta()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetWindowsHardening.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Windows hardening gate", script, StringComparison.Ordinal);
        Assert.Contains("Get-DotnetReleaseTrainStatus.ps1", script, StringComparison.Ordinal);
        Assert.Contains("dotnet test $solutionPath", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetNightlyReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("$nightlyReadinessArguments[\"SkipTests\"] = $true", script, StringComparison.Ordinal);
        Assert.Contains("$runFullInstallSmoke", script, StringComparison.Ordinal);
        Assert.Contains("$nightlyReadinessArguments[\"InstallSmoke\"] = $true", script, StringComparison.Ordinal);
        Assert.Contains("$nightlyReadinessArguments[\"AllowLocalInstallSmoke\"] = $true", script, StringComparison.Ordinal);
        Assert.Contains("Plny lokalni install smoke je preskocen kvuli ochrane existujici instalace stejneho kanalu.", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedNightly.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Get-AhkRetirementReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetReleasePromotion.ps1", script, StringComparison.Ordinal);
        Assert.Contains("TargetChannel beta", script, StringComparison.Ordinal);
        Assert.Contains("artifacts\\nightly\\$RuntimeIdentifier\\app\\Vehimap.Desktop.exe", script, StringComparison.Ordinal);
        Assert.DoesNotContain("New-DotnetDesktopReleaseTag.ps1", script, StringComparison.Ordinal);
        Assert.DoesNotContain("TargetChannel stable", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Release_train_status_script_summarizes_channels_without_publishing()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Get-DotnetReleaseTrainStatus.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("[switch]$SkipFetch", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$FailOnBlockers", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetWindowsHardening.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetNightlyReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetBetaReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetStableReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetReleasePromotion.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedNightly.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedBeta.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedStable.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedRelease.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Get-AhkRetirementReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("\"artifacts\\$channel\\$RuntimeIdentifier\"", script, StringComparison.Ordinal);
        Assert.Contains("vehimap-desktop-$channel-$version-$RuntimeIdentifier-setup.exe", script, StringComparison.Ordinal);
        Assert.Contains("vehimap-desktop-nightly-*-$RuntimeIdentifier-setup.exe", script, StringComparison.Ordinal);
        Assert.Contains("dotnet-nightly", script, StringComparison.Ordinal);
        Assert.Contains("dotnet-beta-v$Version", script, StringComparison.Ordinal);
        Assert.Contains("dotnet-v$Version", script, StringComparison.Ordinal);
        Assert.Contains("Dalsi doporuceny krok:", script, StringComparison.Ordinal);
        Assert.Contains("bez -SkipFetch", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedBeta.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetPublishedStable.ps1", script, StringComparison.Ordinal);
        Assert.DoesNotContain("New-DotnetDesktopReleaseTag.ps1 -RuntimeIdentifier $RuntimeIdentifier -Channel stable -Push", script, StringComparison.Ordinal);
        Assert.DoesNotContain("gh release create", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Published_release_verification_script_checks_manifest_release_and_retirement_gate()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetPublishedRelease.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("[ValidateSet(\"stable\", \"beta\", \"nightly\")]", script, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-$channelName-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("latest-dotnet-preview-$RuntimeIdentifier.ini", script, StringComparison.Ordinal);
        Assert.Contains("\"nightly\" { return \"dotnet-nightly\" }", script, StringComparison.Ordinal);
        Assert.Contains("vehimap-desktop-$Channel-$Version-$RuntimeIdentifier-setup.exe", script, StringComparison.Ordinal);
        Assert.Contains("-nightly\\.\\d+\\.\\d+$", script, StringComparison.Ordinal);
        Assert.Contains("asset_sha256", script, StringComparison.Ordinal);
        Assert.Contains("asset_size", script, StringComparison.Ordinal);
        Assert.Contains("asset_kind", script, StringComparison.Ordinal);
        Assert.Contains("channel", script, StringComparison.Ordinal);
        Assert.Contains("$channelDisplayName manifest neobsahuje platny SHA-256 hash.", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Add-Blocker \"Stabilni manifest nema channel=stable.\"\n    }\n    else", NormalizeLineEndings(script), StringComparison.Ordinal);
        Assert.Contains("releases/download/$releaseTag", script, StringComparison.Ordinal);
        Assert.Contains("Invoke-RemoteHeadCheck", script, StringComparison.Ordinal);
        Assert.Contains("$SkipNetwork", script, StringComparison.Ordinal);
        Assert.Contains("Get-AhkRetirementReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-FailOnBlockers", script, StringComparison.Ordinal);
        Assert.Contains("Preview alias a AHK retirement gate se overuji jen pro stable kanal.", script, StringComparison.Ordinal);
        Assert.Contains("AHK retirement gate se spousti jen pro stable kanal.", script, StringComparison.Ordinal);
        Assert.Contains("publikovany .NET desktop release je overeny", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Published_nightly_wrapper_uses_nightly_channel()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetPublishedNightly.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Test-DotnetPublishedRelease.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Channel = \"nightly\"", script, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"SkipNetwork\"] = $true", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Published_beta_wrapper_uses_beta_channel()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetPublishedBeta.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Test-DotnetPublishedRelease.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Channel = \"beta\"", script, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"SkipNetwork\"] = $true", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Published_stable_wrapper_uses_stable_channel()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetPublishedStable.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Test-DotnetPublishedRelease.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Channel = \"stable\"", script, StringComparison.Ordinal);
        Assert.Contains("$arguments[\"SkipNetwork\"] = $true", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Installer_smoke_script_verifies_package_before_optional_install()
    {
        var scriptPath = Path.Combine(FindRepositoryRoot(), "dotnet", "build", "Test-DotnetInstallerSmoke.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("[Parameter(Mandatory = $true)]", script, StringComparison.Ordinal);
        Assert.Contains("[string]$InstallerPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$Install", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$AllowLocalInstall", script, StringComparison.Ordinal);
        Assert.Contains("Test-ChecksumFile", script, StringComparison.Ordinal);
        Assert.Contains("Test-PackageMetadata", script, StringComparison.Ordinal);
        Assert.Contains("assetKind=installer", script, StringComparison.Ordinal);
        Assert.Contains("if (-not $Install)", script, StringComparison.Ordinal);
        Assert.Contains("Pro tichou izolovanou instalaci pridejte -Install.", script, StringComparison.Ordinal);
        Assert.Contains("GITHUB_ACTIONS", script, StringComparison.Ordinal);
        Assert.Contains("muze na lokalnim PC prepsat uninstall registr", script, StringComparison.Ordinal);
        Assert.Contains("/VERYSILENT", script, StringComparison.Ordinal);
        Assert.Contains("/DIR=", script, StringComparison.Ordinal);
        Assert.Contains("/LOG=", script, StringComparison.Ordinal);
        Assert.Contains("$installFullPath", script, StringComparison.Ordinal);
        Assert.Contains("Join-Path $installFullPath \"data\"", script, StringComparison.Ordinal);
        Assert.Contains("Vehimap.Desktop.exe", script, StringComparison.Ordinal);
        Assert.Contains("unins000.exe", script, StringComparison.Ordinal);
        Assert.Contains("Remove-Item -LiteralPath $installFullPath -Recurse -Force", script, StringComparison.Ordinal);
    }

    private static string ReadWorkflow()
    {
        var workflowPath = Path.Combine(FindRepositoryRoot(), ".github", "workflows", "dotnet-desktop.yml");
        return File.ReadAllText(workflowPath);
    }

    private static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal);

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
