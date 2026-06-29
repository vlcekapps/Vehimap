param(
    [switch]$FailOnBlockers
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$legacyLibRoot = Join-Path $repositoryRoot "src\lib"

$passed = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]
$blockers = New-Object System.Collections.Generic.List[string]

function Add-Pass {
    param([string]$Message)
    $passed.Add($Message) | Out-Null
}

function Add-Warning {
    param([string]$Message)
    $warnings.Add($Message) | Out-Null
}

function Add-Blocker {
    param([string]$Message)
    $blockers.Add($Message) | Out-Null
}

$coverage = @(
    [ordered]@{
        Module = "AppRuntime.ahk"
        Area = "start aplikace, nacitani dat a runtime"
        Evidence = @(
            "dotnet\src\Vehimap.Application\Services\LegacyVehimapBootstrapper.cs",
            "dotnet\src\Vehimap.Desktop\Services\DesktopAppRuntimeController.cs",
            "dotnet\src\Vehimap.Desktop\App.axaml.cs"
        )
    },
    [ordered]@{
        Module = "AuditTools.ahk"
        Area = "audit dat a navigace na problemy"
        Evidence = @(
            "dotnet\src\Vehimap.Application\Services\LegacyAuditService.cs",
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\AuditWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\AuditWorkspaceView.axaml"
        )
    },
    [ordered]@{
        Module = "BackupsAndAlerts.ahk"
        Area = "zalohy, oznameni, tray a background kontroly"
        Evidence = @(
            "dotnet\src\Vehimap.Storage.Legacy\LegacyBackupService.cs",
            "dotnet\src\Vehimap.Desktop\Services\DesktopNotificationService.cs",
            "dotnet\src\Vehimap.Desktop\Services\DesktopAppRuntimeController.cs"
        )
    },
    [ordered]@{
        Module = "CoreHelpers.ahk"
        Area = "sdilena normalizace a parsovani hodnot"
        Evidence = @(
            "dotnet\src\Vehimap.Application\Services\VehimapValueParser.cs",
            "dotnet\src\Vehimap.Storage.Legacy\LegacyVehicleValueNormalization.cs"
        )
    },
    [ordered]@{
        Module = "Costs.ahk"
        Area = "naklady, cena za kilometr a exporty"
        Evidence = @(
            "dotnet\src\Vehimap.Application\Services\LegacyCostAnalysisService.cs",
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\CostWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Services\DesktopCostExportService.cs"
        )
    },
    [ordered]@{
        Module = "Dashboard.ahk"
        Area = "dashboard a souhrny napric vozidly"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\DashboardWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\DashboardWorkspaceView.axaml"
        )
    },
    [ordered]@{
        Module = "DataStore.ahk"
        Area = "legacy TSV/INI storage a managed prilohy"
        Evidence = @(
            "dotnet\src\Vehimap.Storage.Legacy\LegacyVehimapDataStore.cs",
            "dotnet\src\Vehimap.Platform\ManagedAttachmentPathService.cs",
            "dotnet\src\Vehimap.Domain\Models\VehimapDataSet.cs"
        )
    },
    [ordered]@{
        Module = "FuelDialog.ahk"
        Area = "tankovani a analyza paliva"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\FuelWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\FuelWorkspaceView.axaml",
            "dotnet\src\Vehimap.Application\Services\LegacyFuelAnalysisService.cs"
        )
    },
    [ordered]@{
        Module = "GlobalSearch.ahk"
        Area = "globalni hledani a otevreni vysledku"
        Evidence = @(
            "dotnet\src\Vehimap.Application\Services\LegacyGlobalSearchService.cs",
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\GlobalSearchWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\GlobalSearchWorkspaceView.axaml"
        )
    },
    [ordered]@{
        Module = "HelpAndUpdates.ahk"
        Area = "O programu, build info a aktualizace"
        Evidence = @(
            "dotnet\src\Vehimap.Platform\AssemblyAppBuildInfoProvider.cs",
            "dotnet\src\Vehimap.Platform\LegacyUpdateService.cs",
            "dotnet\src\Vehimap.Desktop\Views\AboutWindow.axaml",
            "dotnet\src\Vehimap.Desktop\Views\UpdateCheckWindow.axaml"
        )
    },
    [ordered]@{
        Module = "HistoryDialog.ahk"
        Area = "historie vozidla"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\HistoryWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\HistoryWorkspaceView.axaml"
        )
    },
    [ordered]@{
        Module = "ImportExport.ahk"
        Area = "backup import/export a ochranna kopie pred obnovou"
        Evidence = @(
            "dotnet\src\Vehimap.Storage.Legacy\LegacyBackupService.cs",
            "dotnet\src\Vehimap.Storage.Legacy\LegacyBackupSerialization.cs",
            "dotnet\src\Vehimap.Desktop\Services\DesktopSessionController.cs"
        )
    },
    [ordered]@{
        Module = "MaintenancePlans.ahk"
        Area = "plan udrzby, sablony a oznaceni splneno"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\MaintenanceWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\MaintenanceWorkspaceView.axaml",
            "dotnet\src\Vehimap.Desktop\Views\MaintenanceCompletionWindow.axaml"
        )
    },
    [ordered]@{
        Module = "MainWindow.ahk"
        Area = "hlavni shell, menu a klavesovy tok"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\MainWindowViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\MainWindow.axaml",
            "dotnet\src\Vehimap.Desktop\Views\MainWindow.axaml.cs"
        )
    },
    [ordered]@{
        Module = "Overviews.ahk"
        Area = "blizici se a propadle terminy"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\UpcomingOverviewWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\OverdueOverviewWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\UpcomingOverviewWorkspaceView.axaml",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\OverdueOverviewWorkspaceView.axaml"
        )
    },
    [ordered]@{
        Module = "RecordsDialog.ahk"
        Area = "doklady a prilohy"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\RecordWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\RecordWorkspaceView.axaml",
            "dotnet\src\Vehimap.Domain\Enums\VehicleRecordAttachmentMode.cs"
        )
    },
    [ordered]@{
        Module = "ReminderDialog.ahk"
        Area = "pripominky"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\ReminderWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\ReminderWorkspaceView.axaml"
        )
    },
    [ordered]@{
        Module = "SettingsDialog.ahk"
        Area = "nastaveni a podporovane volby"
        Evidence = @(
            "dotnet\src\Vehimap.Application\Services\DesktopSupportedSettingsService.cs",
            "dotnet\src\Vehimap.Desktop\ViewModels\SettingsDialogViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\SettingsWindow.axaml"
        )
    },
    [ordered]@{
        Module = "TimelineAndCalendar.ahk"
        Area = "casova osa a ICS export"
        Evidence = @(
            "dotnet\src\Vehimap.Application\Services\LegacyTimelineService.cs",
            "dotnet\src\Vehimap.Application\Services\LegacyCalendarExportService.cs",
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\TimelineWorkspaceViewModel.cs"
        )
    },
    [ordered]@{
        Module = "VehicleBundles.ahk"
        Area = "balicek pro vozidlo a doporucene sablony"
        Evidence = @(
            "dotnet\src\Vehimap.Application\Services\VehicleStarterBundleService.cs",
            "dotnet\src\Vehimap.Desktop\ViewModels\VehicleStarterBundleDialogViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\VehicleStarterBundleWindow.axaml"
        )
    },
    [ordered]@{
        Module = "VehicleDialogs.ahk"
        Area = "detail a editor vozidla"
        Evidence = @(
            "dotnet\src\Vehimap.Desktop\ViewModels\Workspaces\VehicleDetailWorkspaceViewModel.cs",
            "dotnet\src\Vehimap.Desktop\Views\Workspaces\VehicleDetailWorkspaceView.axaml",
            "dotnet\src\Vehimap.Desktop\ViewModels\MainWindowViewModel.VehicleEditing.cs"
        )
    }
)

if (-not (Test-Path -LiteralPath $legacyLibRoot -PathType Container)) {
    Add-Pass "Legacy slozka src\lib je po finalnim AHK retirement commitu odstranena; parity mapa zustava historickou kontrolou pokryti."
}
else {
    $actualModules = @(Get-ChildItem -LiteralPath $legacyLibRoot -Filter "*.ahk" -File | ForEach-Object { $_.Name } | Sort-Object)
    $mappedModules = @($coverage | ForEach-Object { $_.Module } | Sort-Object)

    foreach ($module in $actualModules) {
        if ($mappedModules -notcontains $module) {
            Add-Blocker "AHK modul src\lib\$module neni v migracni parity mape."
        }
    }

    foreach ($module in $mappedModules) {
        if ($actualModules -contains $module) {
            Add-Pass "AHK modul src\lib\$module je zahrnuty v parity mape."
        }
        else {
            Add-Warning "AHK modul src\lib\$module uz ve zdrojich neni; parity evidence zustava pro historii migrace."
        }
    }
}

foreach ($entry in $coverage) {
    $missingEvidence = New-Object System.Collections.Generic.List[string]
    foreach ($relativePath in $entry.Evidence) {
        $path = Join-Path $repositoryRoot $relativePath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            $missingEvidence.Add($relativePath) | Out-Null
        }
    }

    if ($missingEvidence.Count -eq 0) {
        Add-Pass "$($entry.Module): .NET evidence existuje pro oblast '$($entry.Area)'."
    }
    else {
        Add-Blocker "$($entry.Module): chybi .NET evidence pro oblast '$($entry.Area)': $($missingEvidence -join ', ')"
    }
}

$rootScriptPath = Join-Path $repositoryRoot "src\Vehimap.ahk"
if (Test-Path -LiteralPath $rootScriptPath -PathType Leaf) {
    Add-Warning "Korenovy AHK skript src\Vehimap.ahk stale existuje. Po finalnim AHK retirement commitu ma byt odstranen."
}
else {
    Add-Pass "Korenovy AHK skript src\Vehimap.ahk je po finalnim AHK retirement commitu odstranen."
}

Write-Host "Vehimap AHK -> .NET migration parity"
Write-Host "Mapped modules: $($coverage.Count)"
Write-Host ""

Write-Host "Splneno:"
foreach ($item in $passed) {
    Write-Host "  [OK] $item"
}

if ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "Upozorneni:"
    foreach ($item in $warnings) {
        Write-Host "  [WARN] $item"
    }
}

if ($blockers.Count -gt 0) {
    Write-Host ""
    Write-Host "Blockery:"
    foreach ($item in $blockers) {
        Write-Host "  [BLOCK] $item"
    }
}

Write-Host ""
if ($blockers.Count -eq 0) {
    Write-Host "Vysledek: migracni parity mapa je pruchozi."
}
else {
    Write-Host "Vysledek: migracni parity mapa ma blockery."
}

if ($FailOnBlockers -and $blockers.Count -gt 0) {
    throw "Migracni parity mapa ma $($blockers.Count) blockeru."
}
