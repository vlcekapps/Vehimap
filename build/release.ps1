param(
    [ValidateSet("patch", "minor", "major")]
    [string]$Bump = "patch",
    [string]$Version = "",
    [string]$PrereleaseLabel = "",
    [ValidateRange(0, 9999)]
    [int]$PrereleaseNumber = 0,
    [switch]$SkipPush
)

$ErrorActionPreference = "Stop"

function Require-Command {
    param([string]$Name)
    if ($null -eq (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Prikaz '$Name' neni dostupny v PATH."
    }
}

function Invoke-Git {
    param([string[]]$GitArgs)
    & git @GitArgs
    if ($LASTEXITCODE -ne 0) {
        throw "git selhal: git $($GitArgs -join ' ')"
    }
}

function Read-Utf8NoBom {
    param([string]$Path)
    return [System.IO.File]::ReadAllText($Path, [System.Text.UTF8Encoding]::new($false))
}

function Write-Utf8NoBom {
    param(
        [string]$Path,
        [string]$Content
    )
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
}

function Escape-AhkString {
    param([string]$Value)
    return $Value.Replace('"', '""')
}

function Read-VersionFile {
    param([string]$Path)
    if (!(Test-Path $Path)) {
        throw "Soubor VERSION nenalezen: $Path"
    }

    $raw = (Read-Utf8NoBom -Path $Path).Trim()
    if ($raw -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$') {
        throw "VERSION musi byt ve formatu MAJOR.MINOR.PATCH nebo MAJOR.MINOR.PATCH-prerelease. Nalezeno: '$raw'"
    }
    return $raw
}

function Parse-SemVer {
    param([string]$Value)
    if ($Value -notmatch '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<prerelease>[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$') {
        throw "Neplatna semver verze: '$Value'"
    }

    $prerelease = ""
    if ($Matches["prerelease"]) {
        $prerelease = $Matches["prerelease"]
    }

    return @{
        Major = [int]$Matches["major"]
        Minor = [int]$Matches["minor"]
        Patch = [int]$Matches["patch"]
        Prerelease = $prerelease
    }
}

function Compare-SemVer {
    param([string]$Left, [string]$Right)

    $l = Parse-SemVer $Left
    $r = Parse-SemVer $Right

    foreach ($part in @("Major", "Minor", "Patch")) {
        if ($l[$part] -lt $r[$part]) { return -1 }
        if ($l[$part] -gt $r[$part]) { return 1 }
    }

    if ($l["Prerelease"] -eq "" -and $r["Prerelease"] -eq "") { return 0 }
    if ($l["Prerelease"] -eq "") { return 1 }
    if ($r["Prerelease"] -eq "") { return -1 }

    $lIdentifiers = $l["Prerelease"].Split(".")
    $rIdentifiers = $r["Prerelease"].Split(".")
    $maxCount = [Math]::Max($lIdentifiers.Count, $rIdentifiers.Count)

    for ($i = 0; $i -lt $maxCount; $i++) {
        if ($i -ge $lIdentifiers.Count) { return -1 }
        if ($i -ge $rIdentifiers.Count) { return 1 }

        $leftId = $lIdentifiers[$i]
        $rightId = $rIdentifiers[$i]
        $leftNumeric = $leftId -match '^\d+$'
        $rightNumeric = $rightId -match '^\d+$'

        if ($leftNumeric -and $rightNumeric) {
            $leftNumber = [int64]$leftId
            $rightNumber = [int64]$rightId
            if ($leftNumber -lt $rightNumber) { return -1 }
            if ($leftNumber -gt $rightNumber) { return 1 }
            continue
        }

        if ($leftNumeric -and -not $rightNumeric) { return -1 }
        if (-not $leftNumeric -and $rightNumeric) { return 1 }

        $ordinal = [string]::CompareOrdinal($leftId, $rightId)
        if ($ordinal -lt 0) { return -1 }
        if ($ordinal -gt 0) { return 1 }
    }

    return 0
}

function Get-BumpedVersion {
    param(
        [string]$Current,
        [string]$Kind,
        [string]$PrereleaseLabel = "",
        [int]$PrereleaseNumber = 0
    )

    $parts = Parse-SemVer $Current
    $currentBase = "$($parts["Major"]).$($parts["Minor"]).$($parts["Patch"])"
    $currentPre = $parts["Prerelease"]

    if ($currentPre -ne "") {
        if ($PrereleaseLabel.Trim() -ne "") {
            if ($currentPre -match '^([a-zA-Z]+)\.(\d+)$' -and $Matches[1] -eq $PrereleaseLabel) {
                $num = if ($PrereleaseNumber -gt 0) { $PrereleaseNumber } else { [int]$Matches[2] + 1 }
            } else {
                $num = if ($PrereleaseNumber -gt 0) { $PrereleaseNumber } else { 1 }
            }
            return "$currentBase-$PrereleaseLabel.$num"
        }
        return $currentBase
    }

    $baseVersion = switch ($Kind) {
        "major" { "$([int]$parts["Major"] + 1).0.0" }
        "minor" { "$($parts["Major"]).$([int]$parts["Minor"] + 1).0" }
        default { "$($parts["Major"]).$($parts["Minor"]).$([int]$parts["Patch"] + 1)" }
    }

    if ($PrereleaseLabel.Trim() -ne "") {
        $num = if ($PrereleaseNumber -gt 0) { $PrereleaseNumber } else { 1 }
        return "$baseVersion-$PrereleaseLabel.$num"
    }

    return $baseVersion
}

function Test-PrereleaseVersion {
    param([string]$Value)
    return (Parse-SemVer $Value)["Prerelease"] -ne ""
}

function Convert-SemVerToFileVersion {
    param([string]$Value)

    $parts = Parse-SemVer $Value
    $revision = 0
    if ($parts["Prerelease"] -match '(^|\.)(\d+)($|\.)') {
        $revision = [int]$Matches[2]
    }

    return "$($parts["Major"]).$($parts["Minor"]).$($parts["Patch"]).$revision"
}

function Write-BuildInfoFile {
    param(
        [string]$Path,
        [string]$Version,
        [string]$FileVersion
    )

    $escapedVersion = Escape-AhkString -Value $Version
    $escapedFileVersion = Escape-AhkString -Value $FileVersion
    $content = @(
        "; This file is generated by build/release.ps1. Do not edit manually."
        ";@Ahk2Exe-SetDescription Vehimap"
        ";@Ahk2Exe-SetCompanyName vlcekapps"
        ";@Ahk2Exe-SetFileVersion $FileVersion"
        ";@Ahk2Exe-SetProductVersion $FileVersion"
        ""
        "global AppVersion := ""$escapedVersion"""
        "global AppFileVersion := ""$escapedFileVersion"""
    ) -join "`n"

    Write-Utf8NoBom -Path $Path -Content $content
}

function Ensure-ChangelogFile {
    param([string]$Path)

    if (Test-Path $Path) {
        return
    }

    $initial = @(
        "# Changelog",
        "",
        "Vsechny vyznamne zmeny ve Vehimapu budou zapisovane sem.",
        "",
        "## [Unreleased]",
        "",
        "- Zatim bez zapsanych zmen."
    ) -join "`n"

    Write-Utf8NoBom -Path $Path -Content $initial
}

function Update-Changelog {
    param(
        [string]$Path,
        [string]$NewVersion
    )

    Ensure-ChangelogFile -Path $Path

    $content = Read-Utf8NoBom -Path $Path
    $today = (Get-Date).ToString("yyyy-MM-dd")
    $dash = [char]0x2013

    if ($content -notmatch '(?m)^## \[Unreleased\]') {
        if ($content -match '(?m)^# .*$') {
            $content = [regex]::Replace($content, '(?m)^(# .*)$', "`$1`n`n## [Unreleased]", 1)
        } else {
            $content = "## [Unreleased]`n`n" + $content.TrimStart()
        }
    }

    $releaseHeading = "## [$NewVersion] $dash $today"
    $updated = [regex]::Replace($content, '(?m)^## \[Unreleased\].*$', [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $releaseHeading }, 1)

    if ($updated -notmatch '(?m)^## \[Unreleased\]') {
        if ($updated -match '(?m)^# .*$') {
            $updated = [regex]::Replace($updated, '(?m)^(# .*)$', "`$1`n`n## [Unreleased]", 1)
        } else {
            $updated = "## [Unreleased]`n`n" + $updated.TrimStart()
        }
    }

    Write-Utf8NoBom -Path $Path -Content $updated
    return $true
}

function Assert-CleanWorkingTree {
    $status = (& git status --porcelain)
    if ($LASTEXITCODE -ne 0) {
        throw "Nelze nacist git status."
    }
    if ($status -and $status.Count -gt 0) {
        throw "Pracovni strom neni cisty. Commitujte nebo stashujte zmeny pred spustenim release.ps1."
    }
}

function Compile-Vehimap {
    param(
        [string]$CompilerPath,
        [string]$ScriptPath,
        [string]$OutPath
    )

    if (!(Test-Path $CompilerPath)) {
        throw "Ahk2Exe nebyl nalezen: $CompilerPath"
    }
    if (!(Test-Path $ScriptPath)) {
        throw "Zdrojovy script nebyl nalezen: $ScriptPath"
    }

    $outDir = Split-Path -Parent $OutPath
    if (!(Test-Path $outDir)) {
        New-Item -ItemType Directory -Path $outDir | Out-Null
    }

    if (Test-Path $OutPath) {
        Remove-Item $OutPath -Force
    }

    & $CompilerPath /in $ScriptPath /out $OutPath /silent
    if ($LASTEXITCODE -ne 0) {
        throw "Ahk2Exe selhal pri kompilaci Vehimapu."
    }
    if (!(Test-Path $OutPath)) {
        throw "Kompilator nevyrobil vystupni soubor: $OutPath"
    }
}

function New-ReleaseZip {
    param(
        [string]$ReadmePath,
        [string]$ExePath,
        [string]$OutPath
    )

    if (!(Test-Path $ReadmePath)) {
        throw "Soubor readme.txt nebyl nalezen: $ReadmePath"
    }
    if (!(Test-Path $ExePath)) {
        throw "Vehimap.exe nebyl nalezen: $ExePath"
    }

    $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("vehimap_release_" + [System.Guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    try {
        Copy-Item -Path $ReadmePath -Destination (Join-Path $tempDir "readme.txt") -Force
        Copy-Item -Path $ExePath -Destination (Join-Path $tempDir "vehimap.exe") -Force

        if (Test-Path $OutPath) {
            Remove-Item $OutPath -Force
        }

        Compress-Archive -Path (Join-Path $tempDir '*') -DestinationPath $OutPath
    } finally {
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Require-Command -Name git

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$versionPath = Join-Path $projectRoot "src\VERSION"
$buildInfoPath = Join-Path $projectRoot "src\GeneratedBuildInfo.ahk"
$scriptPath = Join-Path $projectRoot "src\Vehimap.ahk"
$readmePath = Join-Path $projectRoot "src\readme.txt"
$changelogPath = Join-Path $projectRoot "CHANGELOG.md"
$distDir = Join-Path $projectRoot "dist"
$compilerPath = "C:\Users\vlcek\AppData\Local\Programs\AutoHotkey\Compiler\Ahk2Exe.exe"

Assert-CleanWorkingTree

$currentVersion = Read-VersionFile -Path $versionPath
$normalizedPrereleaseLabel = $PrereleaseLabel.Trim().ToLowerInvariant()
if ($normalizedPrereleaseLabel -ne "" -and $normalizedPrereleaseLabel -notin @("alpha", "beta", "rc")) {
    throw "Parametr -PrereleaseLabel musi byt alpha, beta nebo rc."
}
if ($Version -and $Version.Trim() -ne "" -and $normalizedPrereleaseLabel -ne "") {
    throw "Pouzijte bud -Version, nebo kombinaci -PrereleaseLabel/-PrereleaseNumber."
}

$newVersion = if ($Version -and $Version.Trim() -ne "") {
    $Version.Trim()
} else {
    Get-BumpedVersion -Current $currentVersion -Kind $Bump -PrereleaseLabel $normalizedPrereleaseLabel -PrereleaseNumber $PrereleaseNumber
}

if ($newVersion -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$') {
    throw "Verze '$newVersion' neni platna. Pouzijte format MAJOR.MINOR.PATCH nebo MAJOR.MINOR.PATCH-prerelease."
}
if ((Compare-SemVer -Left $newVersion -Right $currentVersion) -le 0) {
    throw "Nova verze '$newVersion' musi byt vyssi nez stavajici '$currentVersion'."
}

$isPrerelease = Test-PrereleaseVersion -Value $newVersion
$fileVersion = Convert-SemVerToFileVersion -Value $newVersion
$tagName = "v$newVersion"
if ((& git tag --list $tagName) -contains $tagName) {
    throw "Tag $tagName jiz existuje lokalne."
}

Write-Host "Verze: $currentVersion -> $newVersion"
if ($isPrerelease) {
    Write-Host "Typ release: prerelease"
}

Write-Utf8NoBom -Path $versionPath -Content ($newVersion + "`n")
Write-BuildInfoFile -Path $buildInfoPath -Version $newVersion -FileVersion $fileVersion
$buildInfoLeaf = Split-Path -Leaf $buildInfoPath
Write-Host "$buildInfoLeaf aktualizovan."
$changelogUpdated = Update-Changelog -Path $changelogPath -NewVersion $newVersion
if ($changelogUpdated) {
    Write-Host "CHANGELOG.md aktualizovan."
}

if (!(Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir | Out-Null
}

$exePath = Join-Path $distDir "vehimap.exe"
$zipPath = Join-Path $distDir "vehimap-$newVersion.zip"

Write-Host "Kompiluji vehimap.exe ..."
Compile-Vehimap -CompilerPath $compilerPath -ScriptPath $scriptPath -OutPath $exePath

Write-Host "Vytvarim asset $zipPath ..."
New-ReleaseZip -ReadmePath $readmePath -ExePath $exePath -OutPath $zipPath

Invoke-Git @("add", "src/VERSION", "src/GeneratedBuildInfo.ahk", "CHANGELOG.md")
Invoke-Git @("commit", "-m", "chore(release): $newVersion")

if (-not $SkipPush) {
    Invoke-Git @("push", "origin", "main")
}

Invoke-Git @("tag", "-a", $tagName, "-m", "Release $newVersion")
if (-not $SkipPush) {
    Invoke-Git @("push", "origin", $tagName)
}

if (-not $SkipPush) {
    Require-Command -Name gh

    $releaseExists = $false
    try {
        & gh release view $tagName *> $null 2>&1
        $releaseExists = ($LASTEXITCODE -eq 0)
    } catch {
        $releaseExists = $false
    }

    if ($releaseExists) {
        $editArgs = @("release", "edit", $tagName, "--title", "Vehimap $newVersion")
        if ($isPrerelease) {
            $editArgs += "--prerelease"
        }
        & gh @editArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Uprava GitHub release selhala."
        }

        & gh release upload $tagName $zipPath --clobber
        if ($LASTEXITCODE -ne 0) {
            throw "Nahrani assetu do GitHub release selhalo."
        }
    } else {
        $createArgs = @("release", "create", $tagName, $zipPath, "--target", "main", "--title", "Vehimap $newVersion", "--generate-notes")
        if ($isPrerelease) {
            $createArgs += @("--prerelease", "--latest=false")
        }
        & gh @createArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Vytvoreni GitHub release selhalo."
        }
    }

    $assets = & gh release view $tagName --json assets --jq ".assets[].name"
    if ($assets -notcontains "vehimap-$newVersion.zip") {
        throw "Asset vehimap-$newVersion.zip nebyl nalezen v release $tagName."
    }
}

Write-Host ""
Write-Host "Release $newVersion dokoncen."
Write-Host "Exe: $exePath"
Write-Host "Zip: $zipPath"
if ($SkipPush) {
    Write-Host "Push i GitHub release byly preskoceny kvuli -SkipPush."
}
