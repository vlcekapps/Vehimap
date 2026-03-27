param(
    [string]$AutoHotkeyExe = ""
)

$candidates = @()
if ($AutoHotkeyExe -ne "") {
    $candidates += $AutoHotkeyExe
}
$candidates += @(
    (Join-Path $env:LOCALAPPDATA 'Programs\AutoHotkey\v2\AutoHotkey64.exe'),
    (Join-Path $env:LOCALAPPDATA 'Programs\AutoHotkey\v2\AutoHotkey.exe')
)

$resolved = $candidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $resolved) {
    Write-Error 'AutoHotkey executable was not found. Use -AutoHotkeyExe to pass the path explicitly.'
    exit 1
}

$smokeScript = Join-Path $PSScriptRoot 'Smoke.ahk'
& $resolved $smokeScript
exit $LASTEXITCODE
