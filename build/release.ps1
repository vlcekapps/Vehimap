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

function Write-UpdateManifestFile {
    param(
        [string]$Path,
        [string]$Version,
        [string]$TagName,
        [string]$AssetSha256,
        [Int64]$AssetSize
    )

    $publishedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    $assetUrl = "https://github.com/vlcekapps/Vehimap/releases/download/$TagName/vehimap-$Version.zip"
    $notesUrl = "https://github.com/vlcekapps/Vehimap/releases/tag/$TagName"
    $content = @(
        "[release]"
        "version=$Version"
        "published_at=$publishedAt"
        "asset_url=$assetUrl"
        "asset_sha256=$AssetSha256"
        "asset_size=$AssetSize"
        "notes_url=$notesUrl"
    ) -join "`n"

    $directory = Split-Path -Parent $Path
    if ($directory -and !(Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    Write-Utf8NoBom -Path $Path -Content $content
}

function Convert-MarkdownInlineToHtml {
    param([string]$Text)

    $encoded = [System.Net.WebUtility]::HtmlEncode($Text)
    $encoded = [regex]::Replace($encoded, '\*\*([^*]+)\*\*', '<strong>$1</strong>')
    $encoded = [regex]::Replace($encoded, '`([^`]+)`', '<code>$1</code>')
    return $encoded
}

function Convert-MarkdownToHtmlDocument {
    param(
        [string]$MarkdownPath,
        [string]$Title,
        [string]$OutPath
    )

    if (!(Test-Path $MarkdownPath)) {
        throw "Markdown soubor nebyl nalezen: $MarkdownPath"
    }

    $markdown = Read-Utf8NoBom -Path $MarkdownPath
    $lines = $markdown -split "\r?\n"
    $htmlLines = @()
    $paragraphLines = @()
    $inList = $false
    $inCode = $false
    $codeLines = @()

    foreach ($rawLine in $lines) {
        $line = $rawLine.TrimEnd()
        $trimmed = $line.Trim()

        if ($inCode) {
            if ($trimmed -match '^```') {
                $encodedCode = [System.Net.WebUtility]::HtmlEncode(($codeLines -join "`n"))
                $htmlLines += "<pre><code>$encodedCode</code></pre>"
                $codeLines = @()
                $inCode = $false
            } else {
                $codeLines += $line
            }
            continue
        }

        if ($trimmed -match '^```') {
            if ($paragraphLines.Count -gt 0) {
                $htmlLines += '<p>' + (Convert-MarkdownInlineToHtml -Text (($paragraphLines -join ' ').Trim())) + '</p>'
                $paragraphLines = @()
            }
            if ($inList) {
                $htmlLines += '</ul>'
                $inList = $false
            }
            $inCode = $true
            continue
        }

        if ($trimmed -eq '') {
            if ($paragraphLines.Count -gt 0) {
                $htmlLines += '<p>' + (Convert-MarkdownInlineToHtml -Text (($paragraphLines -join ' ').Trim())) + '</p>'
                $paragraphLines = @()
            }
            if ($inList) {
                $htmlLines += '</ul>'
                $inList = $false
            }
            continue
        }

        if ($trimmed -match '^(#{1,6})\s+(.*)$') {
            if ($paragraphLines.Count -gt 0) {
                $htmlLines += '<p>' + (Convert-MarkdownInlineToHtml -Text (($paragraphLines -join ' ').Trim())) + '</p>'
                $paragraphLines = @()
            }
            if ($inList) {
                $htmlLines += '</ul>'
                $inList = $false
            }

            $level = $Matches[1].Length
            $headingText = Convert-MarkdownInlineToHtml -Text $Matches[2].Trim()
            $htmlLines += "<h$level>$headingText</h$level>"
            continue
        }

        if ($trimmed -match '^- (.*)$') {
            if ($paragraphLines.Count -gt 0) {
                $htmlLines += '<p>' + (Convert-MarkdownInlineToHtml -Text (($paragraphLines -join ' ').Trim())) + '</p>'
                $paragraphLines = @()
            }
            if (-not $inList) {
                $htmlLines += '<ul>'
                $inList = $true
            }

            $htmlLines += '<li>' + (Convert-MarkdownInlineToHtml -Text $Matches[1].Trim()) + '</li>'
            continue
        }

        if ($inList) {
            $htmlLines += '</ul>'
            $inList = $false
        }

        $paragraphLines += $trimmed
    }

    if ($inCode) {
        $encodedCode = [System.Net.WebUtility]::HtmlEncode(($codeLines -join "`n"))
        $htmlLines += "<pre><code>$encodedCode</code></pre>"
    }
    if ($paragraphLines.Count -gt 0) {
        $htmlLines += '<p>' + (Convert-MarkdownInlineToHtml -Text (($paragraphLines -join ' ').Trim())) + '</p>'
    }
    if ($inList) {
        $htmlLines += '</ul>'
    }

    $titleHtml = [System.Net.WebUtility]::HtmlEncode($Title)
    $bodyHtml = $htmlLines -join "`n"
    $document = @(
        '<!DOCTYPE html>'
        '<html lang="cs">'
        '<head>'
        '  <meta charset="utf-8">'
        '  <meta name="viewport" content="width=device-width, initial-scale=1">'
        "  <title>$titleHtml</title>"
        '  <style>'
        '    body { font-family: "Segoe UI", Arial, sans-serif; margin: 32px auto; max-width: 920px; padding: 0 20px 48px; line-height: 1.6; color: #1b1b1b; background: #ffffff; }'
        '    h1, h2, h3, h4, h5, h6 { line-height: 1.25; margin-top: 1.6em; margin-bottom: 0.5em; }'
        '    h1 { margin-top: 0; font-size: 2rem; }'
        '    h2 { font-size: 1.45rem; border-bottom: 1px solid #d8d8d8; padding-bottom: 0.2em; }'
        '    p { margin: 0 0 1em 0; }'
        '    ul { margin: 0 0 1em 1.4em; padding: 0; }'
        '    li { margin: 0.2em 0; }'
        '    code { font-family: Consolas, "Courier New", monospace; background: #f4f4f4; padding: 0.12em 0.35em; border-radius: 4px; }'
        '    pre { background: #f4f4f4; padding: 14px 16px; overflow: auto; border-radius: 8px; }'
        '    pre code { background: transparent; padding: 0; border-radius: 0; }'
        '  </style>'
        '</head>'
        '<body>'
        $bodyHtml
        '</body>'
        '</html>'
    ) -join "`n"

    Write-Utf8NoBom -Path $OutPath -Content $document
}

function Ensure-ChangelogFile {
    param([string]$Path)

    if (Test-Path $Path) {
        return
    }

    $initial = @(
        "# Changelog",
        "",
        "Všechny významné změny ve Vehimapu budou zapisovány sem.",
        "Formát vychází z [Keep a Changelog](https://keepachangelog.com/cs/1.1.0/)",
        "a projekt používá [Semantic Versioning](https://semver.org/lang/cs/).",
        "",
        "## [Unreleased]",
        ""
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
    $updated = $false

    if ($content -match '(?m)^## \[Unreleased\]') {
        $content = $content -replace '(?m)^## \[Unreleased\].*$', "## [$NewVersion] - $today"
        $updated = $true
    } elseif ($content -match ('(?m)^## \[' + [regex]::Escape($NewVersion) + '\]\s*$')) {
        $content = $content -replace ('(?m)^## \[' + [regex]::Escape($NewVersion) + '\]\s*$'), "## [$NewVersion] - $today"
        $updated = $true
    }

    if ($updated) {
        Write-Utf8NoBom -Path $Path -Content $content
    }

    return $updated
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

    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        if (Test-Path $OutPath) {
            break
        }
        Start-Sleep -Milliseconds 200
    }

    if (!(Test-Path $OutPath)) {
        throw "Kompilator nevyrobil vystupni soubor: $OutPath"
    }
}

function New-ReleaseZip {
    param(
        [string]$ReadmeHtmlPath,
        [string]$ChangelogHtmlPath,
        [string]$ExePath,
        [string]$OutPath
    )

    if (!(Test-Path $ReadmeHtmlPath)) {
        throw "Soubor readme.html nebyl nalezen: $ReadmeHtmlPath"
    }
    if (!(Test-Path $ChangelogHtmlPath)) {
        throw "Soubor changelog.html nebyl nalezen: $ChangelogHtmlPath"
    }
    if (!(Test-Path $ExePath)) {
        throw "Vehimap.exe nebyl nalezen: $ExePath"
    }

    $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("vehimap_release_" + [System.Guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    try {
        Copy-Item -Path $ReadmeHtmlPath -Destination (Join-Path $tempDir "readme.html") -Force
        Copy-Item -Path $ChangelogHtmlPath -Destination (Join-Path $tempDir "changelog.html") -Force
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
$updateManifestPath = Join-Path $projectRoot "update\latest.ini"
$scriptPath = Join-Path $projectRoot "src\Vehimap.ahk"
$readmeMarkdownPath = Join-Path $projectRoot "README.md"
$readmeHtmlPath = Join-Path $projectRoot "src\readme.html"
$changelogHtmlPath = Join-Path $projectRoot "src\changelog.html"
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
Convert-MarkdownToHtmlDocument -MarkdownPath $readmeMarkdownPath -Title "Vehimap - Uživatelská příručka" -OutPath $readmeHtmlPath
Convert-MarkdownToHtmlDocument -MarkdownPath $changelogPath -Title "Vehimap - Changelog" -OutPath $changelogHtmlPath
Write-Host "HTML dokumentace aktualizovana."

if (!(Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir | Out-Null
}

$exePath = Join-Path $distDir "vehimap.exe"
$zipPath = Join-Path $distDir "vehimap-$newVersion.zip"

Write-Host "Kompiluji vehimap.exe ..."
Compile-Vehimap -CompilerPath $compilerPath -ScriptPath $scriptPath -OutPath $exePath

Write-Host "Vytvarim asset $zipPath ..."
New-ReleaseZip -ReadmeHtmlPath $readmeHtmlPath -ChangelogHtmlPath $changelogHtmlPath -ExePath $exePath -OutPath $zipPath

$zipHash = (Get-FileHash -Path $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
$zipSize = (Get-Item $zipPath).Length
Write-UpdateManifestFile -Path $updateManifestPath -Version $newVersion -TagName $tagName -AssetSha256 $zipHash -AssetSize $zipSize
$updateManifestLeaf = Split-Path -Leaf $updateManifestPath
Write-Host "$updateManifestLeaf aktualizovan."

Invoke-Git @("add", "src/VERSION", "src/GeneratedBuildInfo.ahk", "src/readme.html", "src/changelog.html", "update/latest.ini", "CHANGELOG.md")
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
