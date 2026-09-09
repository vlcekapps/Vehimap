# SPDX-License-Identifier: GPL-3.0-or-later
param(
    [uri]$ServerUrl = 'http://127.0.0.1:4725/',
    [ValidateSet('Windows', 'NovaWindows')]
    [string]$AutomationName = 'Windows',
    [ValidateSet('Startup', 'Core', 'Editors', 'Repairs', 'All')]
    [string]$Profile = 'Core',
    [string]$AppPath,
    [string]$Configuration = 'Release',
    [ValidateRange(0, 50)]
    [int]$LaunchWaitSeconds = 5
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'This live UI gate requires Windows.' }

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if ([string]::IsNullOrWhiteSpace($AppPath)) {
    $AppPath = Join-Path $repoRoot 'dotnet/artifacts/nightly/win-x64/app/Vehimap.exe'
}
$AppPath = (Resolve-Path -LiteralPath $AppPath).Path
$status = Invoke-RestMethod -Uri ([uri]::new($ServerUrl, 'status')) -TimeoutSec 5
if ($status.value.ready -ne $true) { throw 'The Appium server is not ready.' }

$startup = 'Main_shell_exposes_visible_startup_controls_when_appium_is_available'
$core = @(
    $startup,
    'Main_menu_can_be_invoked_with_f10_without_entering_regular_tab_order_when_appium_is_available',
    'Vehicle_detail_save_returns_focus_to_primary_action_when_appium_is_available',
    'Vehicle_detail_cancel_returns_focus_to_primary_action_when_appium_is_available',
    'Vehicle_detail_editor_keeps_standard_textbox_cursor_navigation_when_appium_is_available',
    'Vehicle_detail_editor_opens_combobox_with_arrow_key_when_appium_is_available'
)
$filter = switch ($Profile) {
    'Startup' { "FullyQualifiedName=Vehimap.Tests.UI.DesktopContinuousIntegrationSmokeTests.$startup" }
    'Core' { ($core | ForEach-Object { "FullyQualifiedName=Vehimap.Tests.UI.DesktopContinuousIntegrationSmokeTests.$_" }) -join '|' }
    'Editors' { 'FullyQualifiedName~DesktopContinuousIntegrationSmokeTests&FullyQualifiedName~editor_runs_in_standalone_window|FullyQualifiedName~DesktopContinuousIntegrationSmokeTests.Editor_dialog_shift_tab|FullyQualifiedName~DesktopContinuousIntegrationSmokeTests.Cancelling_modal_editor' }
    'Repairs' { 'FullyQualifiedName=Vehimap.Tests.UI.DesktopContinuousIntegrationSmokeTests.Unplanned_repair_dialog_saves_once_and_restores_focus_when_appium_is_available' }
    'All' { 'FullyQualifiedName~DesktopContinuousIntegrationSmokeTests|FullyQualifiedName~DesktopAccessibilitySmokeTests' }
}

$runRoot = Join-Path $repoRoot ('dotnet/artifacts/windows-ui/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
$settings = @{
    VEHIMAP_APPIUM_SERVER_URL = $ServerUrl.AbsoluteUri
    VEHIMAP_UI_APP = $AppPath
    VEHIMAP_UI_AUTOMATION_NAME = $AutomationName
    VEHIMAP_UI_REQUIRE_APPIUM = '1'
    VEHIMAP_UI_ISOLATED_LAUNCH_ONLY = '1'
    VEHIMAP_UI_EVIDENCE_PATH = $runRoot
    VEHIMAP_UI_LAUNCH_WAIT_SECONDS = [string]$LaunchWaitSeconds
}
$previous = @{}
try {
    foreach ($key in $settings.Keys) {
        $previous[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
        [Environment]::SetEnvironmentVariable($key, $settings[$key], 'Process')
    }

    Write-Host "Live UI gate: $AutomationName / $Profile / $ServerUrl"
    Write-Host "Application: $AppPath"
    Write-Host "Evidence: $runRoot"
    & dotnet test (Join-Path $repoRoot 'dotnet/tests/Vehimap.Tests.UI/Vehimap.Tests.UI.csproj') `
        --configuration $Configuration --filter $filter `
        --logger 'trx;LogFileName=windows-ui.trx' --results-directory $runRoot
    $testExitCode = $LASTEXITCODE

    $exitDeadline = [DateTime]::UtcNow.AddSeconds(5)
    do {
        $remaining = @(Get-Process -Name ([IO.Path]::GetFileNameWithoutExtension($AppPath)) -ErrorAction SilentlyContinue)
        if ($remaining.Count -eq 0) { break }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $exitDeadline)
    if ($remaining.Count -gt 0) {
        throw "A Vehimap process remains after UI teardown (PID: $($remaining.Id -join ', ')). Close it before another run. Test evidence: $runRoot"
    }
    if ($testExitCode -ne 0) { throw "Live UI tests failed. No Vehimap process remains. See $runRoot" }

    [xml]$results = Get-Content -LiteralPath (Join-Path $runRoot 'windows-ui.trx') -Raw
    $counters = $results.TestRun.ResultSummary.Counters
    $expected = switch ($Profile) { 'Startup' { 1 } 'Core' { $core.Count } 'Editors' { 12 } 'Repairs' { 3 } 'All' { [int]$counters.total } }
    if ($expected -lt 1 -or [int]$counters.total -ne $expected -or [int]$counters.passed -ne $expected -or [int]$counters.executed -ne $expected) {
        throw 'The live UI gate did not execute and pass every expected scenario.'
    }

    Write-Host "Live UI gate OK: $expected scenarios passed; no Vehimap process remains."
}
finally {
    foreach ($key in $previous.Keys) {
        [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process')
    }
}
