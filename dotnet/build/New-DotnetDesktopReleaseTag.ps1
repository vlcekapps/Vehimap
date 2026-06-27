param(
    [string]$RuntimeIdentifier = "win-x64",
    [ValidateSet("stable", "beta", "nightly")]
    [string]$Channel = "stable",
    [switch]$Push,
    [switch]$DryRun,
    [switch]$SkipReadiness,
    [switch]$SkipFetch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$versionPath = Join-Path $repositoryRoot "src\VERSION"
$readinessScript = Join-Path $PSScriptRoot "Test-DotnetReleaseReadiness.ps1"

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    Push-Location $repositoryRoot
    try {
        $output = & git @Arguments 2>&1
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            $message = ($output | Out-String).Trim()
            if ([string]::IsNullOrWhiteSpace($message)) {
                $message = "git $($Arguments -join ' ') selhal s kodem $exitCode."
            }

            throw $message
        }

        return $output
    }
    finally {
        Pop-Location
    }
}

if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    throw "Soubor verze '$versionPath' neexistuje."
}

$version = (Get-Content -LiteralPath $versionPath | Select-Object -First 1).Trim()
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "Soubor verze '$versionPath' je prazdny."
}

$tagName = switch ($Channel) {
    "beta" { "dotnet-beta-v$version" }
    "nightly" { "dotnet-nightly" }
    default { "dotnet-v$version" }
}

Write-Host "Vehimap .NET desktop release tag"
Write-Host "Version: $version"
Write-Host "Channel: $Channel"
Write-Host "Tag: $tagName"
Write-Host "Runtime readiness: $RuntimeIdentifier"

$branch = (Invoke-Git rev-parse --abbrev-ref HEAD | Select-Object -First 1).Trim()
if ($branch -ne "main") {
    throw "Release tag lze vytvorit jen z vetve main. Aktualni vetev: $branch."
}

$status = (Invoke-Git status --porcelain | Out-String).Trim()
if (-not [string]::IsNullOrWhiteSpace($status)) {
    throw "Pracovni strom neni cisty. Pred release tagem commitnete nebo odlozte zmeny."
}

if (-not $SkipFetch) {
    Invoke-Git fetch origin main --tags | Out-Null
}

$head = (Invoke-Git rev-parse HEAD | Select-Object -First 1).Trim()
$originMain = (Invoke-Git rev-parse origin/main | Select-Object -First 1).Trim()
if ($head -ne $originMain) {
    throw "Lokální main neni shodny s origin/main. Nejdrive provedte pull/push, aby release tag ukazoval na publikovany commit."
}

$existingLocalTag = (Invoke-Git tag --list $tagName | Out-String).Trim()
if (-not [string]::IsNullOrWhiteSpace($existingLocalTag)) {
    throw "Tag '$tagName' uz existuje lokalne."
}

$existingRemoteTag = (Invoke-Git ls-remote --tags origin "refs/tags/$tagName" | Out-String).Trim()
if (-not [string]::IsNullOrWhiteSpace($existingRemoteTag)) {
    throw "Tag '$tagName' uz existuje na origin."
}

if (-not $SkipReadiness) {
    if (-not (Test-Path -LiteralPath $readinessScript -PathType Leaf)) {
        throw "Chybi release readiness skript '$readinessScript'."
    }

    & $readinessScript -RuntimeIdentifier $RuntimeIdentifier -Channel $Channel
}
else {
    Write-Host "Readiness gate preskocena na vyzadani (-SkipReadiness)."
}

if ($DryRun) {
    Write-Host "Dry run OK. Tag nebyl vytvoren."
    Write-Host "Pro skutecne vytvoreni tagu spustte skript bez -DryRun."
    if (-not $Push) {
        Write-Host "Pro odeslani tagu na GitHub pridejte -Push."
    }

    exit 0
}

$message = if ($Channel -eq "nightly") { "Vehimap desktop nightly $version" } else { "Vehimap desktop $Channel $version" }
Invoke-Git tag -a $tagName -m $message | Out-Null
Write-Host "Vytvoren lokalni tag $tagName."

if ($Push) {
    Invoke-Git push origin $tagName | Out-Null
    Write-Host "Tag $tagName byl odeslan na origin. GitHub Actions spusti desktop release workflow."
}
else {
    Write-Host "Tag zatim nebyl odeslan. Pro spusteni release workflow pouzijte:"
    Write-Host "  git push origin $tagName"
}
