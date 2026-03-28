OpenAboutDialog(*) {
    global AppTitle

    AppMsgBox(BuildAboutProgramText(), AppTitle " - O programu", 0x40)
}

CheckForUpdates(*) {
    global AppTitle

    currentVersion := GetAppVersion()
    try {
        manifest := LoadLatestReleaseManifest()
    } catch as err {
        AppMsgBox("Kontrolu aktualizací se nepodařilo dokončit.`n`n" err.Message, AppTitle, 0x30)
        return
    }

    try {
        comparison := CompareSemVer(currentVersion, manifest.version)
    } catch as err {
        AppMsgBox("Porovnání verzí se nepodařilo dokončit.`n`n" err.Message, AppTitle, 0x30)
        return
    }

    if (comparison < 0) {
        message := "Je dostupná novější verze Vehimap.`n`nAktuálně používáte: " currentVersion "`nNejnovější dostupná verze: " manifest.version
        if (manifest.publishedAt != "") {
            message .= "`nVydáno: " manifest.publishedAt
        }

        assetSize := GetUpdateAssetSize(manifest)
        if (assetSize > 0) {
            message .= "`nVelikost balíčku: " FormatByteSize(assetSize)
        }

        installError := ""
        canInstall := A_IsCompiled
        if canInstall {
            try {
                ValidateUpdateDownloadManifest(manifest)
            } catch as err {
                canInstall := false
                installError := err.Message
            }
        }

        if canInstall {
            result := AppMsgBox(
                message "`n`nAktualizaci můžeme stáhnout a nainstalovat nyní. Vehimap se ukončí a po dokončení znovu spustí.`nPřed pokračováním si prosím uložte případné rozpracované úpravy.`n`nPokračovat?",
                AppTitle,
                0x34
            )
            if (result = "Yes") {
                try {
                    StartUpdateInstallFromManifest(manifest)
                    AppExit()
                } catch as err {
                    AppMsgBox("Aktualizaci se nepodařilo připravit.`n`n" err.Message, AppTitle, 0x30)
                }
            }
            return
        }

        if (!A_IsCompiled) {
            message .= "`n`nAutomatická instalace aktualizace je dostupná jen ve zkompilovaném vehimap.exe."
        } else if (installError != "") {
            message .= "`n`nAutomatickou instalaci teď nelze spustit:`n" installError
        }

        if (manifest.notesUrl != "") {
            result := AppMsgBox(message "`n`nOtevřít stránku vydání?", AppTitle, 0x34)
            if (result = "Yes") {
                AppRun('"' manifest.notesUrl '"')
            }
        } else {
            AppMsgBox(message, AppTitle, 0x40)
        }
        return
    }

    if (comparison > 0) {
        AppMsgBox(
            "Používáte novější lokální verzi (" currentVersion ") než je zatím zapsaná v manifestu (" manifest.version ").",
            AppTitle,
            0x40
        )
        return
    }

    AppMsgBox("Používáte aktuální verzi Vehimap (" currentVersion ").", AppTitle, 0x40)
}

GetAppVersion() {
    global AppVersion

    if IsSet(AppVersion) {
        version := Trim(AppVersion)
        if (version != "") {
            return version
        }
    }

    if A_IsCompiled {
        try {
            version := Trim(FileGetVersion(A_ScriptFullPath))
            if (version != "") {
                return version
            }
        }
    }

    return "Neznámá"
}

GetAppFileVersion() {
    global AppFileVersion

    if IsSet(AppFileVersion) {
        version := Trim(AppFileVersion)
        if (version != "") {
            return version
        }
    }

    if A_IsCompiled {
        try {
            version := Trim(FileGetVersion(A_ScriptFullPath))
            if (version != "") {
                return version
            }
        }
    }

    return ""
}

LoadLatestReleaseManifest() {
    global AppTitle, UpdateManifestUrl

    hooks := GetVehimapTestHooks()
    if IsObject(hooks) {
        if hooks.HasOwnProp("updateManifestError") && Trim(hooks.updateManifestError) != "" {
            throw Error(hooks.updateManifestError)
        }
        if hooks.HasOwnProp("updateManifest") && IsObject(hooks.updateManifest) {
            return hooks.updateManifest
        }
        if hooks.HasOwnProp("updateManifestPath") && Trim(hooks.updateManifestPath) != "" {
            return ReadLatestReleaseManifestFile(hooks.updateManifestPath)
        }
    }

    if !A_IsCompiled {
        SplitPath(A_LineFile, , &sourceDir)
        localManifestPath := sourceDir "\..\update\latest.ini"
        if FileExist(localManifestPath) {
            return ReadLatestReleaseManifestFile(localManifestPath)
        }
    }

    tempPath := A_Temp "\Vehimap_update_manifest.ini"
    requestUrl := UpdateManifestUrl "?ts=" A_NowUTC
    try {
        request := ComObject("WinHttp.WinHttpRequest.5.1")
        request.Open("GET", requestUrl, false)
        request.SetRequestHeader("User-Agent", AppTitle "/" GetAppVersion())
        request.Send()
        if (request.Status != 200) {
            throw Error("Server vrátil HTTP " request.Status ".")
        }

        WriteTextFileUtf8NoBom(tempPath, request.ResponseText)
        return ReadLatestReleaseManifestFile(tempPath)
    } catch as err {
        throw Error("Nepodařilo se načíst manifest aktualizací. " err.Message)
    } finally {
        if FileExist(tempPath) {
            FileDelete(tempPath)
        }
    }
}

ReadLatestReleaseManifestFile(path) {
    version := Trim(IniRead(path, "release", "version", ""))
    if (version = "") {
        throw Error("Manifest neobsahuje položku release/version.")
    }

    return {
        version: version,
        publishedAt: Trim(IniRead(path, "release", "published_at", "")),
        notesUrl: Trim(IniRead(path, "release", "notes_url", "")),
        assetUrl: Trim(IniRead(path, "release", "asset_url", "")),
        assetSha256: StrLower(Trim(IniRead(path, "release", "asset_sha256", ""))),
        assetSize: Trim(IniRead(path, "release", "asset_size", ""))
    }
}

ValidateUpdateDownloadManifest(manifest) {
    assetUrl := Trim(manifest.assetUrl)
    assetSha256 := StrLower(Trim(manifest.assetSha256))
    assetSize := Trim(manifest.assetSize)

    if (assetUrl = "") {
        throw Error("Manifest neobsahuje odkaz na release asset.")
    }
    if !RegExMatch(assetSha256, "^[0-9a-f]{64}$") {
        throw Error("Manifest neobsahuje platný SHA-256 hash assetu.")
    }
    if !RegExMatch(assetSize, "^\d+$") || (assetSize + 0) <= 0 {
        throw Error("Manifest neobsahuje platnou velikost assetu.")
    }
}

GetUpdateAssetSize(manifest) {
    assetSize := Trim(manifest.assetSize)
    if RegExMatch(assetSize, "^\d+$") {
        return assetSize + 0
    }
    return 0
}

FormatByteSize(sizeBytes) {
    sizeBytes += 0
    if (sizeBytes < 1024) {
        return sizeBytes " B"
    }

    sizeKb := sizeBytes / 1024.0
    if (sizeKb < 1024) {
        return StrReplace(Format("{:.1f}", sizeKb), ".", ",") " KB"
    }

    sizeMb := sizeKb / 1024.0
    if (sizeMb < 1024) {
        return StrReplace(Format("{:.1f}", sizeMb), ".", ",") " MB"
    }

    sizeGb := sizeMb / 1024.0
    return StrReplace(Format("{:.2f}", sizeGb), ".", ",") " GB"
}

StartUpdateInstallFromManifest(manifest) {
    if !A_IsCompiled {
        throw Error("Automatická instalace aktualizace je dostupná jen ve zkompilovaném vehimap.exe.")
    }
    ValidateUpdateDownloadManifest(manifest)

    helperPath := A_Temp "\Vehimap_update_helper_" FormatTime(A_Now, "yyyyMMdd_HHmmss") ".ps1"
    WriteTextFileUtf8(helperPath, BuildUpdateHelperPowerShellScript())

    currentPid := DllCall("kernel32\GetCurrentProcessId", "UInt")
    command := BuildUpdateHelperCommand(helperPath, manifest, currentPid)
    try {
        Run(command, , "Hide")
    } catch as err {
        throw Error("Nepodařilo se spustit pomocný aktualizační proces. " err.Message)
    }
}

BuildUpdateHelperCommand(helperPath, manifest, currentPid) {
    powerShellPath := GetPowerShellExePath()
    assetSize := GetUpdateAssetSize(manifest)
    return QuoteCommandArg(powerShellPath)
        . " -NoProfile -ExecutionPolicy Bypass -File " QuoteCommandArg(helperPath)
        . " -ProcessId " currentPid
        . " -AppDir " QuoteCommandArg(A_ScriptDir)
        . " -ExecutablePath " QuoteCommandArg(A_ScriptFullPath)
        . " -DownloadUrl " QuoteCommandArg(manifest.assetUrl)
        . " -ExpectedVersion " QuoteCommandArg(manifest.version)
        . " -ExpectedSha256 " QuoteCommandArg(manifest.assetSha256)
        . " -ExpectedSize " assetSize
}

GetPowerShellExePath() {
    candidates := [
        A_WinDir "\System32\WindowsPowerShell\v1.0\powershell.exe",
        A_WinDir "\Sysnative\WindowsPowerShell\v1.0\powershell.exe",
        "powershell.exe"
    ]

    for _, candidate in candidates {
        if (candidate = "powershell.exe" || FileExist(candidate)) {
            return candidate
        }
    }

    throw Error("PowerShell nebyl nalezen.")
}

QuoteCommandArg(value) {
    return '"' StrReplace(value, '"', '""') '"'
}

BuildUpdateHelperPowerShellScript() {
    lines := [
        "param(",
        "    [Parameter(Mandatory=$true)][int]$ProcessId,",
        "    [Parameter(Mandatory=$true)][string]$AppDir,",
        "    [Parameter(Mandatory=$true)][string]$ExecutablePath,",
        "    [Parameter(Mandatory=$true)][string]$DownloadUrl,",
        "    [Parameter(Mandatory=$true)][string]$ExpectedVersion,",
        "    [Parameter(Mandatory=$true)][string]$ExpectedSha256,",
        "    [Parameter(Mandatory=$true)][Int64]$ExpectedSize",
        ")",
        "",
        "$ErrorActionPreference = 'Stop'",
        "$popupTitle = 'Vehimap'",
        "",
        "function Show-Popup([string]$message, [int]$icon) {",
        "    try {",
        "        (New-Object -ComObject WScript.Shell).Popup($message, 0, $popupTitle, $icon) | Out-Null",
        "    } catch {",
        "    }",
        "}",
        "",
        "function Get-RelativeFiles([string]$root) {",
        "    $resolvedRoot = (Resolve-Path $root).Path",
        "    $items = Get-ChildItem -Path $root -Recurse -File",
        "    $result = @()",
        "    foreach ($item in $items) {",
        "        $relative = $item.FullName.Substring($resolvedRoot.Length).TrimStart('\')",
        "        $result += $relative.Replace('\', '/')",
        "    }",
        "    return $result",
        "}",
        "",
        "$tempRoot = Join-Path $env:TEMP ('VehimapUpdate_' + [Guid]::NewGuid().ToString('N'))",
        "$downloadPath = Join-Path $tempRoot 'vehimap.zip'",
        "$extractDir = Join-Path $tempRoot 'extract'",
        "$backupDir = Join-Path $tempRoot 'backup'",
        "$currentReadme = Join-Path $AppDir 'readme.html'",
        "$currentChangelog = Join-Path $AppDir 'changelog.html'",
        "$legacyReadmeText = Join-Path $AppDir 'readme.txt'",
        "$backupExe = Join-Path $backupDir 'vehimap.exe'",
        "$backupReadme = Join-Path $backupDir 'readme.html'",
        "$backupChangelog = Join-Path $backupDir 'changelog.html'",
        "$newExePath = Join-Path $extractDir 'vehimap.exe'",
        "$newReadmePath = Join-Path $extractDir 'readme.html'",
        "$newChangelogPath = Join-Path $extractDir 'changelog.html'",
        "$restoreExe = $false",
        "$restoreReadme = $false",
        "$restoreChangelog = $false",
        "$updated = $false",
        "",
        "try {",
        "    New-Item -ItemType Directory -Path $tempRoot | Out-Null",
        "    New-Item -ItemType Directory -Path $extractDir | Out-Null",
        "    New-Item -ItemType Directory -Path $backupDir | Out-Null",
        "",
        "    Invoke-WebRequest -UseBasicParsing -Uri $DownloadUrl -OutFile $downloadPath",
        "    if (!(Test-Path $downloadPath)) {",
        "        throw 'Stažený archiv nebyl nalezen.'",
        "    }",
        "    if ((Get-Item $downloadPath).Length -ne $ExpectedSize) {",
        "        throw 'Stažený archiv má jinou velikost, než očekává manifest.'",
        "    }",
        "",
        "    $actualHash = (Get-FileHash -Path $downloadPath -Algorithm SHA256).Hash.ToLowerInvariant()",
        "    if ($actualHash -ne $ExpectedSha256.ToLowerInvariant()) {",
        "        throw 'Stažený archiv neodpovídá očekávanému SHA-256 hashi.'",
        "    }",
        "",
        "    Expand-Archive -Path $downloadPath -DestinationPath $extractDir -Force",
        "    $relativeFiles = Get-RelativeFiles $extractDir | Sort-Object",
        "    $expectedFiles = @('changelog.html', 'readme.html', 'vehimap.exe')",
        "    if (($relativeFiles -join '|') -ne (($expectedFiles | Sort-Object) -join '|')) {",
        "        throw 'Asset neobsahuje očekávané soubory.'",
        "    }",
        "    if (!(Test-Path $newExePath)) {",
        "        throw 'V rozbaleném archivu chybí vehimap.exe.'",
        "    }",
        "    if (!(Test-Path $newReadmePath)) {",
        "        throw 'V rozbaleném archivu chybí readme.html.'",
        "    }",
        "    if (!(Test-Path $newChangelogPath)) {",
        "        throw 'V rozbaleném archivu chybí changelog.html.'",
        "    }",
        "",
        "    if (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue) {",
        "        Wait-Process -Id $ProcessId -Timeout 120",
        "    }",
        "    if (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue) {",
        "        throw 'Vehimap se nepodařilo ukončit v požadovaném čase.'",
        "    }",
        "",
        "    if (!(Test-Path $ExecutablePath)) {",
        "        throw 'Aktuální vehimap.exe nebyl nalezen.'",
        "    }",
        "",
        "    Move-Item -Path $ExecutablePath -Destination $backupExe -Force",
        "    $restoreExe = $true",
        "    if (Test-Path $currentReadme) {",
        "        Move-Item -Path $currentReadme -Destination $backupReadme -Force",
        "        $restoreReadme = $true",
        "    }",
        "    if (Test-Path $currentChangelog) {",
        "        Move-Item -Path $currentChangelog -Destination $backupChangelog -Force",
        "        $restoreChangelog = $true",
        "    }",
        "",
        "    Copy-Item -Path $newExePath -Destination $ExecutablePath -Force",
        "    Copy-Item -Path $newReadmePath -Destination $currentReadme -Force",
        "    Copy-Item -Path $newChangelogPath -Destination $currentChangelog -Force",
        "    if (Test-Path $legacyReadmeText) {",
        "        Remove-Item $legacyReadmeText -Force -ErrorAction SilentlyContinue",
        "    }",
        "    $restoreExe = $false",
        "    $restoreReadme = $false",
        "    $restoreChangelog = $false",
        "    $updated = $true",
        "",
        "    try {",
        "        Start-Process -FilePath $ExecutablePath -WorkingDirectory $AppDir",
        "    } catch {",
        "        Show-Popup -message ('Aktualizace Vehimap na verzi ' + $ExpectedVersion + ' byla nainstalována, ale aplikaci se nepodařilo znovu spustit. Spusťte ji prosím ručně.') -icon 48",
        "    }",
        "} catch {",
        "    try {",
        "        if ($restoreExe -and (Test-Path $backupExe)) {",
        "            if (Test-Path $ExecutablePath) {",
        "                Remove-Item $ExecutablePath -Force -ErrorAction SilentlyContinue",
        "            }",
        "            Move-Item -Path $backupExe -Destination $ExecutablePath -Force",
        "        }",
        "        if ($restoreReadme -and (Test-Path $backupReadme)) {",
        "            if (Test-Path $currentReadme) {",
        "                Remove-Item $currentReadme -Force -ErrorAction SilentlyContinue",
        "            }",
        "            Move-Item -Path $backupReadme -Destination $currentReadme -Force",
        "        } elseif (Test-Path $currentReadme) {",
        "            Remove-Item $currentReadme -Force -ErrorAction SilentlyContinue",
        "        }",
        "        if ($restoreChangelog -and (Test-Path $backupChangelog)) {",
        "            if (Test-Path $currentChangelog) {",
        "                Remove-Item $currentChangelog -Force -ErrorAction SilentlyContinue",
        "            }",
        "            Move-Item -Path $backupChangelog -Destination $currentChangelog -Force",
        "        } elseif (Test-Path $currentChangelog) {",
        "            Remove-Item $currentChangelog -Force -ErrorAction SilentlyContinue",
        "        }",
        "    } catch {",
        "    }",
        "",
        "    if ($updated) {",
        "        Show-Popup -message ('Aktualizace Vehimap byla nainstalována, ale dokončení hlásí chybu: ' + $_.Exception.Message) -icon 48",
        "        exit 0",
        "    }",
        "",
        "    Show-Popup -message ('Aktualizace Vehimap selhala.' + [Environment]::NewLine + [Environment]::NewLine + $_.Exception.Message) -icon 16",
        "    exit 1",
        "} finally {",
        "    if (Test-Path $tempRoot) {",
        "        Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue",
        "    }",
        "}"
    ]

    return JoinLines(lines)
}

ParseSemVer(value) {
    value := Trim(value)
    if !RegExMatch(value, "^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<prerelease>[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$", &match) {
        throw Error("Neplatná semver verze: " value)
    }

    prerelease := ""
    try {
        prerelease := match["prerelease"]
    }

    return {
        major: match["major"] + 0,
        minor: match["minor"] + 0,
        patch: match["patch"] + 0,
        prerelease: prerelease
    }
}

CompareSemVer(left, right) {
    leftParts := ParseSemVer(left)
    rightParts := ParseSemVer(right)

    for _, partName in ["major", "minor", "patch"] {
        if (leftParts.%partName% < rightParts.%partName%) {
            return -1
        }
        if (leftParts.%partName% > rightParts.%partName%) {
            return 1
        }
    }

    leftPrerelease := leftParts.prerelease
    rightPrerelease := rightParts.prerelease
    if (leftPrerelease = "" && rightPrerelease = "") {
        return 0
    }
    if (leftPrerelease = "") {
        return 1
    }
    if (rightPrerelease = "") {
        return -1
    }

    leftIds := StrSplit(leftPrerelease, ".")
    rightIds := StrSplit(rightPrerelease, ".")
    maxCount := leftIds.Length
    if (rightIds.Length > maxCount) {
        maxCount := rightIds.Length
    }

    Loop maxCount {
        index := A_Index
        if (index > leftIds.Length) {
            return -1
        }
        if (index > rightIds.Length) {
            return 1
        }

        leftId := leftIds[index]
        rightId := rightIds[index]
        leftNumeric := RegExMatch(leftId, "^\d+$")
        rightNumeric := RegExMatch(rightId, "^\d+$")

        if (leftNumeric && rightNumeric) {
            leftNumber := leftId + 0
            rightNumber := rightId + 0
            if (leftNumber < rightNumber) {
                return -1
            }
            if (leftNumber > rightNumber) {
                return 1
            }
            continue
        }

        if (leftNumeric && !rightNumeric) {
            return -1
        }
        if (!leftNumeric && rightNumeric) {
            return 1
        }

        textComparison := StrCompare(leftId, rightId)
        if (textComparison < 0) {
            return -1
        }
        if (textComparison > 0) {
            return 1
        }
    }

    return 0
}

IsEquivalentAppAndFileVersion(appVersion, fileVersion) {
    appVersion := Trim(appVersion)
    fileVersion := Trim(fileVersion)

    if (appVersion = "" || fileVersion = "") {
        return false
    }
    if (appVersion = fileVersion) {
        return true
    }
    if RegExMatch(appVersion, "^\d+\.\d+\.\d+$") && RegExMatch(fileVersion, "^\Q" appVersion "\E(?:\.0)+$") {
        return true
    }

    return false
}

BuildAboutProgramText() {
    global AppTitle, DataDir

    currentVersion := GetAppVersion()
    lines := [
        AppTitle,
        "Verze: " currentVersion,
        "Režim spuštění: " (A_IsCompiled ? "samostatná aplikace" : "zdrojový skript"),
        "Soubor aplikace: " A_ScriptFullPath,
        "Datová složka: " DataDir
    ]

    fileVersion := GetAppFileVersion()
    if (fileVersion != "" && !IsEquivalentAppAndFileVersion(currentVersion, fileVersion)) {
        lines.Push("Souborová verze (Windows): " fileVersion)
    }

    return JoinLines(lines)
}
