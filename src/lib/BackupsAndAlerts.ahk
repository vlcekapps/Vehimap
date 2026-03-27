GetSettingsContentForBackup() {
    global SettingsFile

    EnsureSettingsDefaults()
    IniWrite(GetRunAtStartupEnabled() ? "1" : "0", SettingsFile, "app", "run_at_startup")
    return FileExist(SettingsFile) ? NormalizeTextForStorage(FileRead(SettingsFile, "UTF-8")) : ""
}

GetVehiclesContentForBackup() {
    global VehiclesFile

    if FileExist(VehiclesFile) {
        return NormalizeTextForStorage(FileRead(VehiclesFile, "UTF-8"))
    }

    return BuildVehiclesDataContent()
}

GetHistoryContentForBackup() {
    global HistoryFile

    if FileExist(HistoryFile) {
        return NormalizeTextForStorage(FileRead(HistoryFile, "UTF-8"))
    }

    return BuildHistoryDataContent()
}

GetFuelContentForBackup() {
    global FuelLogFile

    if FileExist(FuelLogFile) {
        return NormalizeTextForStorage(FileRead(FuelLogFile, "UTF-8"))
    }

    return BuildFuelDataContent()
}

GetRecordsContentForBackup() {
    global RecordsFile

    if FileExist(RecordsFile) {
        return NormalizeTextForStorage(FileRead(RecordsFile, "UTF-8"))
    }

    return BuildRecordsDataContent()
}

GetVehicleMetaContentForBackup() {
    global VehicleMetaFile

    if FileExist(VehicleMetaFile) {
        return NormalizeTextForStorage(FileRead(VehicleMetaFile, "UTF-8"))
    }

    return BuildVehicleMetaDataContent()
}

GetRemindersContentForBackup() {
    global RemindersFile

    if FileExist(RemindersFile) {
        return NormalizeTextForStorage(FileRead(RemindersFile, "UTF-8"))
    }

    return BuildVehicleRemindersDataContent()
}

GetMaintenancePlansContentForBackup() {
    global MaintenancePlansFile

    if FileExist(MaintenancePlansFile) {
        return NormalizeTextForStorage(FileRead(MaintenancePlansFile, "UTF-8"))
    }

    return BuildVehicleMaintenanceDataContent()
}

BuildCurrentBackupContent() {
    return BuildBackupContent(
        GetSettingsContentForBackup(),
        BuildVehiclesDataContent(),
        BuildHistoryDataContent(),
        BuildFuelDataContent(),
        BuildRecordsDataContent(),
        BuildVehicleMetaDataContent(),
        BuildVehicleRemindersDataContent(),
        BuildVehicleMaintenanceDataContent()
    )
}

GetAutomaticBackupDirectory() {
    global DataDir

    return DataDir "\auto-backups"
}

EnsureAutomaticBackupDirectory() {
    backupDir := GetAutomaticBackupDirectory()
    if !InStr(FileExist(backupDir), "D") {
        DirCreate(backupDir)
    }
    return backupDir
}

GetAutomaticBackupPath() {
    backupDir := EnsureAutomaticBackupDirectory()
    timestamp := FormatTime(A_Now, "yyyy-MM-dd_HH-mm-ss")
    return backupDir "\Vehimap_auto_" timestamp ".vehimapbak"
}

GetAutomaticBackupLastStamp() {
    global SettingsFile

    stamp := Trim(IniRead(SettingsFile, "backups", "last_automatic_backup_stamp", ""))
    return RegExMatch(stamp, "^\d{14}$") ? stamp : ""
}

GetAutomaticBackupLastPath() {
    global SettingsFile

    return Trim(IniRead(SettingsFile, "backups", "last_automatic_backup_path", ""))
}

FormatAutomaticBackupStamp(stamp) {
    return RegExMatch(stamp, "^\d{14}$") ? FormatTime(stamp, "dd.MM.yyyy HH:mm") : "zatím nebyla vytvořena"
}

BuildAutomaticBackupStatusText() {
    backupDirLabel := "data\auto-backups"
    lastStamp := GetAutomaticBackupLastStamp()
    lastPath := GetAutomaticBackupLastPath()
    if (lastStamp = "") {
        return "Automatické zálohy se ukládají do složky " backupDirLabel ". Poslední záloha v této složce zatím nebyla vytvořena."
    }

    status := "Automatické zálohy se ukládají do složky " backupDirLabel ". Poslední záloha v této složce: " FormatAutomaticBackupStamp(lastStamp) "."
    if (lastPath != "" && FileExist(lastPath)) {
        status .= " Soubor je uložen pod názvem " SubStr(lastPath, InStr(lastPath, "\",, -1) + 1) "."
    }
    return status
}

IsAutomaticBackupDue() {
    if !GetAutomaticBackupsEnabled() {
        return false
    }

    lastStamp := GetAutomaticBackupLastStamp()
    if (lastStamp = "") {
        return true
    }

    intervalDays := GetAutomaticBackupIntervalDays()
    currentDateStamp := SubStr(A_Now, 1, 8) "000000"
    lastDateStamp := SubStr(lastStamp, 1, 8) "000000"
    return DateDiff(currentDateStamp, lastDateStamp, "Days") >= intervalDays
}

RunAutomaticBackupCheck(force := false, showErrorMessage := false) {
    if !force && !GetAutomaticBackupsEnabled() {
        return ""
    }

    if !force && !IsAutomaticBackupDue() {
        if GetAutomaticBackupsEnabled() {
            TrimAutomaticBackupFiles()
        }
        return ""
    }

    return CreateAutomaticBackup(showErrorMessage)
}

CreateAutomaticBackup(showErrorMessage := false) {
    global AppTitle, SettingsFile

    try {
        backupPath := GetAutomaticBackupPath()
        WriteTextFileUtf8(backupPath, BuildCurrentBackupContent())
        IniWrite(A_Now, SettingsFile, "backups", "last_automatic_backup_stamp")
        IniWrite(backupPath, SettingsFile, "backups", "last_automatic_backup_path")
        TrimAutomaticBackupFiles()
        return backupPath
    } catch as err {
        if showErrorMessage {
            MsgBox("Automatická záloha se nepodařila.`n`n" err.Message, AppTitle, 0x30)
        } else {
            TrayTip("Automatická záloha se nepodařila. " ShortenText(err.Message, 110), AppTitle)
        }
        return ""
    }
}

TrimAutomaticBackupFiles() {
    backupDir := GetAutomaticBackupDirectory()
    if !InStr(FileExist(backupDir), "D") {
        return
    }

    keepCount := GetAutomaticBackupKeepCount()
    files := []
    Loop Files backupDir "\*.vehimapbak", "F" {
        files.Push(A_LoopFileFullPath)
    }

    SortTextItemsDescending(&files)
    while (files.Length > keepCount) {
        try FileDelete(files.Pop())
    }
}

GetDefaultBackupPath() {
    timestamp := FormatTime(A_Now, "yyyy-MM-dd_HH-mm")
    return A_ScriptDir "\Vehimap_zaloha_" timestamp ".vehimapbak"
}

EnsureBackupExtension(path) {
    if (StrLower(SubStr(path, -10)) != ".vehimapbak") {
        path .= ".vehimapbak"
    }

    return path
}

BuildBackupContent(settingsContent, vehiclesContent, historyContent := "", fuelContent := "", recordsContent := "", metaContent := "", remindersContent := "", maintenanceContent := "") {
    settingsContent := NormalizeTextForStorage(settingsContent)
    vehiclesContent := NormalizeTextForStorage(vehiclesContent)
    historyContent := NormalizeTextForStorage(historyContent)
    fuelContent := NormalizeTextForStorage(fuelContent)
    recordsContent := NormalizeTextForStorage(recordsContent)
    metaContent := NormalizeTextForStorage(metaContent)
    remindersContent := NormalizeTextForStorage(remindersContent)
    maintenanceContent := NormalizeTextForStorage(maintenanceContent)

    header := JoinLines([
        "# Vehimap backup v5",
        "settings_length=" StrLen(settingsContent),
        "vehicles_length=" StrLen(vehiclesContent),
        "history_length=" StrLen(historyContent),
        "fuel_length=" StrLen(fuelContent),
        "records_length=" StrLen(recordsContent),
        "meta_length=" StrLen(metaContent),
        "reminders_length=" StrLen(remindersContent),
        "maintenance_length=" StrLen(maintenanceContent)
    ])

    return header "`n`n" settingsContent vehiclesContent historyContent fuelContent recordsContent metaContent remindersContent maintenanceContent
}

TryParseBackupContent(content, &settingsContent, &vehiclesContent, &historyContent, &fuelContent, &recordsContent, &metaContent, &remindersContent, &maintenanceContent, &errorMessage) {
    settingsContent := ""
    vehiclesContent := ""
    historyContent := ""
    fuelContent := ""
    recordsContent := ""
    metaContent := ""
    remindersContent := ""
    maintenanceContent := ""
    errorMessage := ""
    content := NormalizeTextForStorage(content)

    delimiterPos := InStr(content, "`n`n")
    if !delimiterPos {
        errorMessage := "Soubor zálohy nemá platnou hlavičku."
        return false
    }

    header := SubStr(content, 1, delimiterPos - 1)
    payload := SubStr(content, delimiterPos + 2)
    headerLines := StrSplit(header, "`n")
    if (headerLines.Length < 3) {
        errorMessage := "Soubor není ve formátu zálohy Vehimap."
        return false
    }

    backupVersion := headerLines[1]
    if (backupVersion != "# Vehimap backup v1" && backupVersion != "# Vehimap backup v2" && backupVersion != "# Vehimap backup v3" && backupVersion != "# Vehimap backup v4" && backupVersion != "# Vehimap backup v5") {
        errorMessage := "Soubor není ve formátu zálohy Vehimap."
        return false
    }

    if !RegExMatch(headerLines[2], "^settings_length=(\d+)$", &settingsMatch) {
        errorMessage := "Soubor zálohy neobsahuje délku nastavení."
        return false
    }

    if !RegExMatch(headerLines[3], "^vehicles_length=(\d+)$", &vehiclesMatch) {
        errorMessage := "Soubor zálohy neobsahuje délku dat vozidel."
        return false
    }

    settingsLength := settingsMatch[1] + 0
    vehiclesLength := vehiclesMatch[1] + 0
    historyLength := 0
    fuelLength := 0
    recordsLength := 0
    metaLength := 0
    remindersLength := 0
    maintenanceLength := 0
    if (
        backupVersion = "# Vehimap backup v2"
        || backupVersion = "# Vehimap backup v3"
        || backupVersion = "# Vehimap backup v4"
        || backupVersion = "# Vehimap backup v5"
    ) {
        if (headerLines.Length < 4 || !RegExMatch(headerLines[4], "^history_length=(\d+)$", &historyMatch)) {
            errorMessage := "Soubor zálohy neobsahuje délku historie."
            return false
        }
        historyLength := historyMatch[1] + 0
    }

    if (backupVersion = "# Vehimap backup v3" || backupVersion = "# Vehimap backup v4" || backupVersion = "# Vehimap backup v5") {
        if (headerLines.Length < 6 || !RegExMatch(headerLines[5], "^fuel_length=(\d+)$", &fuelMatch) || !RegExMatch(headerLines[6], "^records_length=(\d+)$", &recordsMatch)) {
            errorMessage := "Soubor zálohy neobsahuje délky kilometrů a dokladů."
            return false
        }
        fuelLength := fuelMatch[1] + 0
        recordsLength := recordsMatch[1] + 0
    }

    if (backupVersion = "# Vehimap backup v4" || backupVersion = "# Vehimap backup v5") {
        if (headerLines.Length < 8 || !RegExMatch(headerLines[7], "^meta_length=(\d+)$", &metaMatch) || !RegExMatch(headerLines[8], "^reminders_length=(\d+)$", &remindersMatch)) {
            errorMessage := "Soubor zálohy neobsahuje délky stavů a připomínek."
            return false
        }
        metaLength := metaMatch[1] + 0
        remindersLength := remindersMatch[1] + 0
    }

    if (backupVersion = "# Vehimap backup v5") {
        if (headerLines.Length < 9 || !RegExMatch(headerLines[9], "^maintenance_length=(\d+)$", &maintenanceMatch)) {
            errorMessage := "Soubor zálohy neobsahuje délku plánů údržby."
            return false
        }
        maintenanceLength := maintenanceMatch[1] + 0
    }

    if (settingsLength < 0 || vehiclesLength < 0 || historyLength < 0 || fuelLength < 0 || recordsLength < 0 || metaLength < 0 || remindersLength < 0 || maintenanceLength < 0) {
        errorMessage := "Soubor zálohy obsahuje neplatné délky dat."
        return false
    }

    if (StrLen(payload) != settingsLength + vehiclesLength + historyLength + fuelLength + recordsLength + metaLength + remindersLength + maintenanceLength) {
        errorMessage := "Soubor zálohy je neúplný nebo poškozený."
        return false
    }

    settingsContent := SubStr(payload, 1, settingsLength)
    vehiclesContent := SubStr(payload, settingsLength + 1, vehiclesLength)
    payloadOffset := settingsLength + vehiclesLength + 1

    if (
        backupVersion = "# Vehimap backup v2"
        || backupVersion = "# Vehimap backup v3"
        || backupVersion = "# Vehimap backup v4"
        || backupVersion = "# Vehimap backup v5"
    ) {
        historyContent := SubStr(payload, payloadOffset, historyLength)
        payloadOffset += historyLength
    } else {
        historyContent := "# Vehimap history v1`n"
    }

    if (backupVersion = "# Vehimap backup v3" || backupVersion = "# Vehimap backup v4" || backupVersion = "# Vehimap backup v5") {
        fuelContent := SubStr(payload, payloadOffset, fuelLength)
        payloadOffset += fuelLength
        recordsContent := SubStr(payload, payloadOffset, recordsLength)
        payloadOffset += recordsLength
    } else {
        fuelContent := "# Vehimap fuel v1`n"
        recordsContent := "# Vehimap records v1`n"
    }

    if (backupVersion = "# Vehimap backup v4" || backupVersion = "# Vehimap backup v5") {
        metaContent := SubStr(payload, payloadOffset, metaLength)
        payloadOffset += metaLength
        remindersContent := SubStr(payload, payloadOffset, remindersLength)
        payloadOffset += remindersLength
    } else {
        metaContent := "# Vehimap meta v2`n"
        remindersContent := "# Vehimap reminders v1`n"
    }

    if (backupVersion = "# Vehimap backup v5") {
        maintenanceContent := SubStr(payload, payloadOffset, maintenanceLength)
    } else {
        maintenanceContent := "# Vehimap maintenance v1`n"
    }

    return true
}

TryParseVehiclesBackupContent(content, &loadedVehicles, &errorMessage) {
    loadedVehicles := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap data v3" && firstNonEmptyLine != "# Vehimap data v4") {
        errorMessage := "Soubor vozidel není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap data v4'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 12) {
            errorMessage := "Soubor vozidel je poškozený nebo není ve formátu v3. Řádek " index " musí obsahovat přesně 12 polí oddělených tabulátory."
            return false
        }

        loadedVehicles.Push({
            id: UnescapeField(fields[1]),
            name: UnescapeField(fields[2]),
            category: NormalizeCategory(UnescapeField(fields[3])),
            vehicleNote: UnescapeField(fields[4]),
            makeModel: UnescapeField(fields[5]),
            plate: UnescapeField(fields[6]),
            year: UnescapeField(fields[7]),
            power: UnescapeField(fields[8]),
            lastTk: UnescapeField(fields[9]),
            nextTk: UnescapeField(fields[10]),
            greenCardFrom: UnescapeField(fields[11]),
            greenCardTo: UnescapeField(fields[12])
        })
    }

    return true
}

TryParseHistoryBackupContent(content, &loadedHistory, &errorMessage) {
    loadedHistory := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap history v1") {
        errorMessage := "Soubor historie není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap history v1'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor historie obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 6 && fields.Length != 7) {
            errorMessage := "Soubor historie je poškozený. Řádek " index " musí obsahovat 6 nebo 7 polí oddělených tabulátory."
            return false
        }

        loadedHistory.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            eventDate: UnescapeField(fields[3]),
            eventType: UnescapeField(fields[4]),
            odometer: UnescapeField(fields[5]),
            cost: UnescapeField(fields[6]),
            note: (fields.Length = 7) ? UnescapeField(fields[7]) : ""
        })
    }

    return true
}

TryParseFuelBackupContent(content, &loadedFuelLog, &errorMessage) {
    loadedFuelLog := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap fuel v1") {
        errorMessage := "Soubor kilometrů a tankování není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap fuel v1'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor kilometrů a tankování obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 9) {
            errorMessage := "Soubor kilometrů a tankování je poškozený. Řádek " index " musí obsahovat přesně 9 polí oddělených tabulátory."
            return false
        }

        loadedFuelLog.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            entryDate: UnescapeField(fields[3]),
            odometer: UnescapeField(fields[4]),
            liters: UnescapeField(fields[5]),
            totalCost: UnescapeField(fields[6]),
            fullTank: (UnescapeField(fields[7]) = "1") ? 1 : 0,
            fuelType: UnescapeField(fields[8]),
            note: UnescapeField(fields[9])
        })
    }

    return true
}

TryParseRecordsBackupContent(content, &loadedRecords, &errorMessage) {
    loadedRecords := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap records v1") {
        errorMessage := "Soubor pojištění a dokladů není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap records v1'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor pojištění a dokladů obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 10) {
            errorMessage := "Soubor pojištění a dokladů je poškozený. Řádek " index " musí obsahovat přesně 10 polí oddělených tabulátory."
            return false
        }

        loadedRecords.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            recordType: UnescapeField(fields[3]),
            title: UnescapeField(fields[4]),
            provider: UnescapeField(fields[5]),
            validFrom: UnescapeField(fields[6]),
            validTo: UnescapeField(fields[7]),
            price: UnescapeField(fields[8]),
            filePath: UnescapeField(fields[9]),
            note: UnescapeField(fields[10])
        })
    }

    return true
}

TryParseVehicleMetaBackupContent(content, &loadedMeta, &errorMessage) {
    loadedMeta := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap meta v1" && firstNonEmptyLine != "# Vehimap meta v2") {
        errorMessage := "Soubor stavů, štítků a servisního profilu vozidel není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap meta v2'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor stavů, štítků a servisního profilu vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (
            (firstNonEmptyLine = "# Vehimap meta v1" && fields.Length != 3)
            || (firstNonEmptyLine = "# Vehimap meta v2" && fields.Length != 7)
        ) {
            errorMessage := "Soubor stavů, štítků a servisního profilu vozidel je poškozený. Řádek " index " musí odpovídat hlavičce souboru."
            return false
        }

        loadedMeta.Push(NormalizeVehicleMetaEntry({
            vehicleId: UnescapeField(fields[1]),
            state: UnescapeField(fields[2]),
            tags: UnescapeField(fields[3]),
            powertrain: (fields.Length >= 4) ? UnescapeField(fields[4]) : "",
            climateProfile: (fields.Length >= 5) ? UnescapeField(fields[5]) : "",
            timingDrive: (fields.Length >= 6) ? UnescapeField(fields[6]) : "",
            transmission: (fields.Length >= 7) ? UnescapeField(fields[7]) : ""
        }))
    }

    return true
}

TryParseVehicleRemindersBackupContent(content, &loadedReminders, &errorMessage) {
    loadedReminders := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap reminders v1" && firstNonEmptyLine != "# Vehimap reminders v2") {
        errorMessage := "Soubor vlastních připomínek není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap reminders v1' nebo '# Vehimap reminders v2'."
        return false
    }

    isV2 := (firstNonEmptyLine = "# Vehimap reminders v2")

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor vlastních připomínek obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        expectedFieldCount := isV2 ? 7 : 6
        if (fields.Length != expectedFieldCount) {
            errorMessage := "Soubor vlastních připomínek je poškozený. Řádek " index " musí obsahovat přesně " expectedFieldCount " polí oddělených tabulátory."
            return false
        }

        loadedReminders.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            title: UnescapeField(fields[3]),
            dueDate: UnescapeField(fields[4]),
            reminderDays: UnescapeField(fields[5]),
            repeatMode: isV2 ? NormalizeReminderRepeat(UnescapeField(fields[6])) : "Neopakovat",
            note: isV2 ? UnescapeField(fields[7]) : UnescapeField(fields[6])
        })
    }

    return true
}

TryParseVehicleMaintenancePlansBackupContent(content, &loadedPlans, &errorMessage) {
    loadedPlans := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap maintenance v1") {
        errorMessage := "Soubor plánů údržby není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap maintenance v1'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor plánů údržby obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 9) {
            errorMessage := "Soubor plánů údržby je poškozený. Řádek " index " musí obsahovat přesně 9 polí oddělených tabulátory."
            return false
        }

        loadedPlans.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            title: UnescapeField(fields[3]),
            intervalKm: NormalizePositiveIntegerText(UnescapeField(fields[4])),
            intervalMonths: NormalizePositiveIntegerText(UnescapeField(fields[5])),
            lastServiceDate: NormalizeEventDate(UnescapeField(fields[6])),
            lastServiceOdometer: NormalizeOdometerText(UnescapeField(fields[7])),
            isActive: (UnescapeField(fields[8]) = "0") ? 0 : 1,
            note: UnescapeField(fields[9])
        })
    }

    return true
}

BackupCurrentFilesBeforeImport() {
    global DataDir, VehiclesFile, HistoryFile, FuelLogFile, RecordsFile, VehicleMetaFile, RemindersFile, MaintenancePlansFile, SettingsFile

    backupRoot := DataDir "\import-backups"
    if !InStr(FileExist(backupRoot), "D") {
        DirCreate(backupRoot)
    }

    backupDir := backupRoot "\" FormatTime(A_Now, "yyyy-MM-dd_HH-mm-ss")
    DirCreate(backupDir)

    if FileExist(VehiclesFile) {
        FileCopy(VehiclesFile, backupDir "\vehicles.tsv", true)
    }
    if FileExist(HistoryFile) {
        FileCopy(HistoryFile, backupDir "\history.tsv", true)
    }
    if FileExist(FuelLogFile) {
        FileCopy(FuelLogFile, backupDir "\fuel.tsv", true)
    }
    if FileExist(RecordsFile) {
        FileCopy(RecordsFile, backupDir "\records.tsv", true)
    }
    if FileExist(VehicleMetaFile) {
        FileCopy(VehicleMetaFile, backupDir "\vehicle_meta.tsv", true)
    }
    if FileExist(RemindersFile) {
        FileCopy(RemindersFile, backupDir "\reminders.tsv", true)
    }
    if FileExist(MaintenancePlansFile) {
        FileCopy(MaintenancePlansFile, backupDir "\maintenance.tsv", true)
    }
    if FileExist(SettingsFile) {
        FileCopy(SettingsFile, backupDir "\settings.ini", true)
    }

    return backupDir
}

EnsureSettingsDefaults() {
    global SettingsFile

    EnsureIniKeyExists(SettingsFile, "notifications", "technical_reminder_days", "31")
    EnsureIniKeyExists(SettingsFile, "notifications", "green_card_reminder_days", "31")
    EnsureIniKeyExists(SettingsFile, "notifications", "maintenance_reminder_days", "31")
    EnsureIniKeyExists(SettingsFile, "notifications", "maintenance_reminder_km", "1000")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_alert_day", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_alert_signature", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_green_alert_day", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_green_alert_signature", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_reminder_alert_day", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_reminder_alert_signature", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_maintenance_alert_day", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_maintenance_alert_signature", "")
    EnsureIniKeyExists(SettingsFile, "app", "run_at_startup", "0")
    EnsureIniKeyExists(SettingsFile, "app", "hide_on_launch", "0")
    EnsureIniKeyExists(SettingsFile, "app", "hide_inactive_vehicles", "0")
    EnsureIniKeyExists(SettingsFile, "app", "show_dashboard_on_launch", "0")
    EnsureIniKeyExists(SettingsFile, "backups", "automatic_backups_enabled", "0")
    EnsureIniKeyExists(SettingsFile, "backups", "automatic_backup_interval_days", "1")
    EnsureIniKeyExists(SettingsFile, "backups", "automatic_backup_keep_count", "30")
    EnsureIniKeyExists(SettingsFile, "backups", "last_automatic_backup_stamp", "")
    EnsureIniKeyExists(SettingsFile, "backups", "last_automatic_backup_path", "")
    EnsureIniKeyExists(SettingsFile, "overview", "filter", "all")
    EnsureIniKeyExists(SettingsFile, "overview", "include_missing_green", "0")
    EnsureIniKeyExists(SettingsFile, "overview", "include_data_issues", "0")
    EnsureIniKeyExists(SettingsFile, "overview", "sort_column", "6")
    EnsureIniKeyExists(SettingsFile, "overview", "sort_descending", "0")
    EnsureIniKeyExists(SettingsFile, "history_view", "sort_column", "1")
    EnsureIniKeyExists(SettingsFile, "history_view", "sort_descending", "1")
    EnsureIniKeyExists(SettingsFile, "fuel_view", "sort_column", "1")
    EnsureIniKeyExists(SettingsFile, "fuel_view", "sort_descending", "1")
    EnsureIniKeyExists(SettingsFile, "records_view", "sort_column", "4")
    EnsureIniKeyExists(SettingsFile, "records_view", "sort_descending", "0")
    EnsureIniKeyExists(SettingsFile, "reminder_view", "sort_column", "2")
    EnsureIniKeyExists(SettingsFile, "reminder_view", "sort_descending", "0")
    EnsureIniKeyExists(SettingsFile, "maintenance_view", "sort_column", "5")
    EnsureIniKeyExists(SettingsFile, "maintenance_view", "sort_descending", "0")
}

EnsureIniKeyExists(path, section, key, defaultValue) {
    missingMarker := "__VEHIMAP_MISSING__"
    currentValue := IniRead(path, section, key, missingMarker)
    if (currentValue = missingMarker) {
        IniWrite(defaultValue, path, section, key)
    }
}

RefreshVehicleList(selectVehicleId := "") {
    global Vehicles, VisibleVehicleIds, VehicleList

    if !IsObject(VehicleList) {
        return
    }

    VisibleVehicleIds := []
    category := GetCurrentCategory()
    items := []
    categoryItems := []
    activeCategoryCount := 0
    hiddenInactiveCount := 0
    selectedRow := 0
    searchText := GetMainSearchText()
    filterKind := GetMainVehicleFilterKind()
    hideInactive := GetHideInactiveVehiclesEnabled()

    for vehicle in Vehicles {
        if (vehicle.category = category) {
            categoryItems.Push(vehicle)
            if IsVehicleInactive(vehicle) {
                hiddenInactiveCount += 1
                if hideInactive {
                    continue
                }
            } else {
                activeCategoryCount += 1
            }
            if (VehicleMatchesMainSearch(vehicle, searchText) && VehicleMatchesMainFilter(vehicle, filterKind)) {
                items.Push(vehicle)
            }
        }
    }

    SortVehiclesByDue(&items)

    VehicleList.Opt("-Redraw")
    VehicleList.Delete()
    for vehicle in items {
        row := VehicleList.Add("", vehicle.name, vehicle.vehicleNote, vehicle.makeModel, vehicle.plate, vehicle.lastTk, vehicle.nextTk, vehicle.greenCardTo, GetVehicleStatusText(vehicle))
        VisibleVehicleIds.Push(vehicle.id)
        if (selectVehicleId != "" && vehicle.id = selectVehicleId) {
            VehicleList.Modify(row, "Select Focus Vis")
            selectedRow := row
        }
    }
    VehicleList.Opt("+Redraw")

    if selectedRow {
        FocusVehicleListLater()
    }

    UpdateVehicleListLabel(items.Length, categoryItems.Length, category, hiddenInactiveCount, activeCategoryCount, hideInactive)
    UpdateStatusBar()
    SetupTrayMenu()
}

UpdateVehicleListLabel(itemCount, totalCount, category, hiddenInactiveCount := 0, activeCount := 0, hideInactive := false) {
    global VehicleListLabel

    if !IsObject(VehicleListLabel) {
        return
    }

    if hideInactive {
        if (itemCount = activeCount) {
            suffix := (itemCount = 1) ? "1 aktivní vozidlo" : itemCount " aktivních vozidel"
        } else {
            suffix := "zobrazeno " itemCount " z " activeCount " aktivních vozidel"
        }

        if (hiddenInactiveCount > 0) {
            suffix .= ", skryto " hiddenInactiveCount " archivovaných nebo odstavených"
        }
    } else if (itemCount = totalCount) {
        suffix := (itemCount = 1) ? "1 vozidlo" : itemCount " vozidel"
    } else {
        suffix := "zobrazeno " itemCount " z " totalCount " vozidel"
    }

    VehicleListLabel.Text := "Seznam vozidel v kategorii " category " (" suffix ")"
}

GetCurrentCategory() {
    global Categories, TabsCtrl

    index := TabsCtrl.Value
    if (index < 1 || index > Categories.Length) {
        return Categories[1]
    }
    return Categories[index]
}

GetSelectedVehicle() {
    global VisibleVehicleIds, VehicleList

    row := VehicleList.GetNext(0)
    if !row {
        return ""
    }

    if (row > VisibleVehicleIds.Length) {
        return ""
    }

    return FindVehicleById(VisibleVehicleIds[row])
}

FindVehicleById(vehicleId) {
    global Vehicles

    for vehicle in Vehicles {
        if (vehicle.id = vehicleId) {
            return vehicle
        }
    }

    return ""
}

FindVehicleIndexById(vehicleId) {
    global Vehicles

    for index, vehicle in Vehicles {
        if (vehicle.id = vehicleId) {
            return index
        }
    }

    return 0
}

OpenVehicleById(vehicleId, resetFilters := false) {
    global TabsCtrl

    vehicle := FindVehicleById(vehicleId)
    if !IsObject(vehicle) {
        return
    }

    TabsCtrl.Value := GetCategoryIndex(vehicle.category)
    if resetFilters {
        SetMainVehicleFilters("", "all")
    }
    ShowMainWindow()
    RefreshVehicleList(vehicle.id)
}

OpenNearestDueVehicle(*) {
    global AppTitle

    upcoming := GetUpcomingVehicles()
    if (upcoming.Length = 0) {
        MsgBox("Momentálně není žádné vozidlo s blížící se nebo propadlou technickou kontrolou.", AppTitle, 0x40)
        return
    }

    OpenVehicleById(upcoming[1].vehicle.id, true)
}

ManualDueCheck(*) {
    global AppTitle

    upcoming := GetUpcomingVehicles()
    if (upcoming.Length = 0) {
        MsgBox("Žádná vozidla teď nevyžadují upozornění na technickou kontrolu.", AppTitle, 0x40)
        return
    }

    MsgBox(BuildReminderMessage(upcoming), AppTitle, 0x40)
    OpenVehicleById(upcoming[1].vehicle.id, true)
}

OpenNearestGreenCardVehicle(*) {
    global AppTitle

    if !HasAnyGreenCardConfigured() {
        MsgBox("U žádného vozidla není vyplněná zelená karta. Můžete ji doplnit v editaci vozidla.", AppTitle, 0x40)
        return
    }

    upcoming := GetUpcomingGreenCards()
    if (upcoming.Length = 0) {
        message := "Žádná vyplněná zelená karta teď nevyžaduje upozornění."
        if HasAnyMissingGreenCard() {
            message .= "`nU některých vozidel zelená karta vyplněná není a můžete ji doplnit v editaci vozidla."
        }
        MsgBox(message, AppTitle, 0x40)
        return
    }

    OpenVehicleById(upcoming[1].vehicle.id, true)
}

ManualGreenCardCheck(*) {
    global AppTitle

    if !HasAnyGreenCardConfigured() {
        MsgBox("U žádného vozidla není vyplněná zelená karta. Můžete ji doplnit v editaci vozidla.", AppTitle, 0x40)
        return
    }

    upcoming := GetUpcomingGreenCards()
    if (upcoming.Length = 0) {
        message := "Žádná vyplněná zelená karta teď nevyžaduje upozornění."
        if HasAnyMissingGreenCard() {
            message .= "`nU některých vozidel zelená karta vyplněná není a můžete ji doplnit v editaci vozidla."
        }
        MsgBox(message, AppTitle, 0x40)
        return
    }

    MsgBox(BuildGreenCardReminderMessage(upcoming), AppTitle, 0x40)
    OpenVehicleById(upcoming[1].vehicle.id, true)
}

OpenNearestReminder(*) {
    global AppTitle

    upcoming := GetUpcomingCustomReminders()
    if (upcoming.Length = 0) {
        MsgBox("Momentálně není žádná vlastní připomínka, která by vyžadovala pozornost.", AppTitle, 0x40)
        return
    }

    OpenVehicleById(upcoming[1].vehicle.id, true)
}

ManualReminderCheck(*) {
    global AppTitle

    upcoming := GetUpcomingCustomReminders()
    if (upcoming.Length = 0) {
        MsgBox("Žádné vlastní připomínky teď nevyžadují upozornění.", AppTitle, 0x40)
        return
    }

    MsgBox(BuildCustomReminderMessage(upcoming), AppTitle, 0x40)
    OpenVehicleById(upcoming[1].vehicle.id, true)
}

CheckDueVehicles(showTrayNotification := true, forceMessageBox := false) {
    global AppTitle

    upcoming := GetUpcomingVehicles()
    greenCards := GetUpcomingGreenCards()
    reminders := GetUpcomingCustomReminders()
    maintenance := GetUpcomingVehicleMaintenance()
    maintenance := GetUpcomingVehicleMaintenance()
    SetupTrayMenu()
    UpdateStatusBar()

    if forceMessageBox {
        ManualDueCheck()
        return
    }

    if !showTrayNotification {
        return
    }

    showTechnicalAlert := false
    if (upcoming.Length > 0) {
        signature := BuildAlertSignature(upcoming)
        showTechnicalAlert := ShouldShowAlert(signature, "technical")
    }

    showGreenAlert := false
    if (greenCards.Length > 0) {
        signature := BuildGreenCardAlertSignature(greenCards)
        showGreenAlert := ShouldShowAlert(signature, "green")
    }

    showReminderAlert := false
    if (reminders.Length > 0) {
        signature := BuildCustomReminderAlertSignature(reminders)
        showReminderAlert := ShouldShowAlert(signature, "reminder")
    }

    showMaintenanceAlert := false
    if (maintenance.Length > 0) {
        signature := BuildMaintenanceAlertSignature(maintenance)
        showMaintenanceAlert := ShouldShowAlert(signature, "maintenance")
    }

    message := BuildAutomaticReminderMessage(upcoming, greenCards, reminders, maintenance, showTechnicalAlert, showGreenAlert, showReminderAlert, showMaintenanceAlert)
    if (message != "") {
        TrayTip(message, AppTitle)
    }
}

GetUpcomingVehicles() {
    global Vehicles

    cutoff := DateAdd(A_Now, GetTechnicalReminderDays(), "Days")
    upcoming := []

    for vehicle in Vehicles {
        dueStamp := ParseDueStamp(vehicle.nextTk)
        if (dueStamp = "") {
            continue
        }
        if (dueStamp <= cutoff) {
            upcoming.Push({
                vehicle: vehicle,
                dueStamp: dueStamp
            })
        }
    }

    SortUpcomingByDue(&upcoming)
    return upcoming
}

GetUpcomingGreenCards() {
    global Vehicles

    cutoff := DateAdd(A_Now, GetGreenCardReminderDays(), "Days")
    upcoming := []

    for vehicle in Vehicles {
        dueStamp := ParseDueStamp(vehicle.greenCardTo)
        if (dueStamp = "") {
            continue
        }
        if (dueStamp <= cutoff) {
            upcoming.Push({
                vehicle: vehicle,
                dueStamp: dueStamp
            })
        }
    }

    SortUpcomingByDue(&upcoming)
    return upcoming
}

BuildUpcomingOverviewEntries(includeMissingGreenCards := false, includeDataIssues := false) {
    global Vehicles

    technicalReminderDays := GetTechnicalReminderDays()
    greenCardReminderDays := GetGreenCardReminderDays()
    entries := []

    for item in GetUpcomingVehicles() {
        entries.Push({
            kind: "technical",
            kindLabel: "Technická kontrola",
            vehicle: item.vehicle,
            dueStamp: item.dueStamp,
            term: item.vehicle.nextTk,
            status: GetExpirationStatusText(item.vehicle.nextTk, technicalReminderDays),
            isMissingGreen: false
        })
    }

    for item in GetUpcomingGreenCards() {
        entries.Push({
            kind: "green",
            kindLabel: "Zelená karta",
            vehicle: item.vehicle,
            dueStamp: item.dueStamp,
            term: item.vehicle.greenCardTo,
            status: GetExpirationStatusText(item.vehicle.greenCardTo, greenCardReminderDays),
            isMissingGreen: false
        })
    }

    for item in GetUpcomingCustomReminders() {
        entries.Push({
            kind: "custom",
            kindLabel: "Vlastní připomínka",
            vehicle: item.vehicle,
            dueStamp: item.dueStamp,
            term: item.reminder.dueDate,
            status: GetReminderExpirationStatusText(item.reminder.dueDate, item.reminder.reminderDays + 0),
            isMissingGreen: false,
            entryId: item.reminder.id
        })
    }

    for item in GetUpcomingVehicleMaintenance() {
        entries.Push({
            kind: "maintenance",
            kindLabel: "Plán údržby",
            vehicle: item.vehicle,
            dueStamp: item.dueStamp,
            overviewSortKey: item.overviewSortKey,
            term: item.term,
            status: item.status,
            isMissingGreen: false,
            entryId: item.entryId
        })
    }

    if includeMissingGreenCards {
        for vehicle in Vehicles {
            if (vehicle.greenCardTo = "") {
                entries.Push({
                    kind: "green",
                    kindLabel: "Zelená karta",
                    vehicle: vehicle,
                    dueStamp: "99999999999998",
                    term: "Nevyplněno",
                    status: "Chybí",
                    isMissingGreen: true
                })
            }
        }
    }

    if includeDataIssues {
        for entry in BuildDashboardDataIssueEntries() {
            entries.Push(entry)
        }
    }

    return entries
}

BuildUpcomingOverviewSummary(entries, allEntries := "") {
    technicalCount := 0
    greenCount := 0
    customCount := 0
    maintenanceCount := 0
    dataIssueCount := 0
    totalCount := IsObject(allEntries) ? allEntries.Length : entries.Length

    for entry in entries {
        if (entry.kind = "technical") {
            technicalCount += 1
        } else if (entry.kind = "green") {
            greenCount += 1
        } else if (entry.kind = "custom") {
            customCount += 1
        } else if (entry.kind = "maintenance") {
            maintenanceCount += 1
        } else if IsOverviewDataIssueEntry(entry) {
            dataIssueCount += 1
        }
    }

    missingGreenCount := GetMissingGreenCardCount()
    totalDataIssueCount := GetOverviewDataIssueCount()
    if (totalCount = 0) {
        if ShouldShowDataIssuesInOverview() {
            summary := "Momentálně není žádný blížící se ani propadlý termín ani datový nedostatek, který by podle aktuálního nastavení vyžadoval pozornost."
        } else {
            summary := "Momentálně není žádný blížící se ani propadlý termín, který by podle aktuálního nastavení vyžadoval pozornost."
        }
    } else if (entries.Length = totalCount) {
        summary := "Celkem " entries.Length " položek k pozornosti: " technicalCount " technických kontrol, " greenCount " zelených karet, " customCount " vlastních připomínek a " maintenanceCount " plánů údržby"
    } else {
        summary := "Zobrazeno " entries.Length " z " totalCount " položek: " technicalCount " technických kontrol, " greenCount " zelených karet, " customCount " vlastních připomínek a " maintenanceCount " plánů údržby"
    }

    if (totalCount > 0) {
        if (dataIssueCount > 0) {
            summary .= " a " dataIssueCount " datových nedostatků."
        } else {
            summary .= "."
        }
    }

    if (missingGreenCount > 0) {
        if ShouldShowMissingGreenCardsInOverview() {
            summary .= " Je zapnuto i zobrazení vozidel bez vyplněné zelené karty."
        } else {
            summary .= " U " missingGreenCount " vozidel není zelená karta vyplněná."
        }
    }

    if (totalDataIssueCount > 0) {
        if ShouldShowDataIssuesInOverview() {
            summary .= " Je zapnuto i zobrazení datových nedostatků."
        } else {
            summary .= " Další datové nedostatky můžete zobrazit zapnutím volby pod hledáním."
        }
    }

    return summary
}

GetOverviewDataIssueCount() {
    return BuildDashboardDataIssueEntries().Length
}

GetMissingGreenCardCount() {
    global Vehicles

    count := 0
    for vehicle in Vehicles {
        if (vehicle.greenCardTo = "") {
            count += 1
        }
    }

    return count
}

HasAnyGreenCardConfigured() {
    global Vehicles

    for vehicle in Vehicles {
        if (vehicle.greenCardTo != "") {
            return true
        }
    }

    return false
}

HasAnyMissingGreenCard() {
    global Vehicles

    for vehicle in Vehicles {
        if (vehicle.greenCardTo = "") {
            return true
        }
    }

    return false
}

ValidatePositiveIntegerSetting(ctrl, fieldLabel, minValue := 1, maxValue := 999) {
    global AppTitle

    value := Trim(ctrl.Text)
    maxDigits := StrLen(maxValue "")
    if !RegExMatch(value, "^\d{1," maxDigits "}$") {
        MsgBox(fieldLabel " musí být celé číslo od " minValue " do " maxValue ".", AppTitle, 0x30)
        ctrl.Focus()
        return ""
    }

    value += 0
    if (value < minValue || value > maxValue) {
        MsgBox(fieldLabel " musí být v rozsahu od " minValue " do " maxValue ".", AppTitle, 0x30)
        ctrl.Focus()
        return ""
    }

    return value
}

ValidateReminderDaysSetting(ctrl, fieldLabel) {
    return ValidatePositiveIntegerSetting(ctrl, fieldLabel, 1, 999)
}

GetTechnicalReminderDays() {
    return ReadReminderDaysSetting("technical_reminder_days")
}

GetGreenCardReminderDays() {
    return ReadReminderDaysSetting("green_card_reminder_days")
}

GetMaintenanceReminderDays() {
    return ReadReminderDaysSetting("maintenance_reminder_days")
}

GetMaintenanceReminderKm() {
    global SettingsFile

    value := IniRead(SettingsFile, "notifications", "maintenance_reminder_km", "1000") + 0
    if (value < 1 || value > 999999) {
        return 1000
    }

    return value
}

ReadReminderDaysSetting(keyName) {
    global SettingsFile

    days := IniRead(SettingsFile, "notifications", keyName, "")
    if (days = "") {
        days := IniRead(SettingsFile, "notifications", "reminder_days", "31")
    }

    days += 0
    if (days < 1 || days > 999) {
        return 31
    }

    return days
}

GetRunAtStartupEnabled() {
    return FileExist(GetStartupShortcutPath()) ? 1 : 0
}

GetHideOnLaunchEnabled() {
    global SettingsFile

    return IniRead(SettingsFile, "app", "hide_on_launch", "0") = "1" ? 1 : 0
}

GetHideInactiveVehiclesEnabled() {
    global SettingsFile

    return IniRead(SettingsFile, "app", "hide_inactive_vehicles", "0") = "1" ? 1 : 0
}

GetShowDashboardOnLaunchEnabled() {
    global SettingsFile

    return IniRead(SettingsFile, "app", "show_dashboard_on_launch", "0") = "1" ? 1 : 0
}

GetAutomaticBackupsEnabled() {
    global SettingsFile

    return IniRead(SettingsFile, "backups", "automatic_backups_enabled", "0") = "1" ? 1 : 0
}

GetAutomaticBackupIntervalDays() {
    global SettingsFile

    days := IniRead(SettingsFile, "backups", "automatic_backup_interval_days", "1") + 0
    if (days < 1 || days > 999) {
        return 1
    }
    return days
}

GetAutomaticBackupKeepCount() {
    global SettingsFile

    keepCount := IniRead(SettingsFile, "backups", "automatic_backup_keep_count", "30") + 0
    if (keepCount < 1 || keepCount > 999) {
        return 30
    }
    return keepCount
}

SetRunAtStartupEnabled(enabled) {
    global AppTitle

    shortcutPath := GetStartupShortcutPath()

    try {
        if enabled {
            shell := ComObject("WScript.Shell")
            shortcut := shell.CreateShortcut(shortcutPath)
            shortcut.TargetPath := GetLaunchTargetPath()
            shortcut.Arguments := GetLaunchArguments()
            shortcut.WorkingDirectory := A_ScriptDir
            shortcut.Description := AppTitle
            shortcut.Save()
        } else if FileExist(shortcutPath) {
            FileDelete(shortcutPath)
        }
        return true
    } catch as err {
        action := enabled ? "zapnout" : "vypnout"
        MsgBox("Nepodařilo se " action " spuštění Vehimap po startu počítače.`n`n" err.Message, AppTitle, 0x30)
        return false
    }
}

GetStartupShortcutPath() {
    global AppTitle

    safeTitle := RegExReplace(AppTitle, '[\\/:*?"<>|]', "")
    return A_Startup "\" safeTitle ".lnk"
}

GetLaunchTargetPath() {
    return A_IsCompiled ? A_ScriptFullPath : A_AhkPath
}

GetLaunchArguments() {
    if A_IsCompiled {
        return ""
    }

    return '"' A_ScriptFullPath '"'
}

ResetAlertHistory() {
    global SettingsFile

    IniWrite("", SettingsFile, "notifications", "last_alert_day")
    IniWrite("", SettingsFile, "notifications", "last_alert_signature")
    IniWrite("", SettingsFile, "notifications", "last_green_alert_day")
    IniWrite("", SettingsFile, "notifications", "last_green_alert_signature")
    IniWrite("", SettingsFile, "notifications", "last_reminder_alert_day")
    IniWrite("", SettingsFile, "notifications", "last_reminder_alert_signature")
    IniWrite("", SettingsFile, "notifications", "last_maintenance_alert_day")
    IniWrite("", SettingsFile, "notifications", "last_maintenance_alert_signature")
}

GetExpirationStatusText(monthYear, reminderDays) {
    dueStamp := ParseDueStamp(monthYear)
    if (dueStamp = "") {
        return ""
    }

    if (dueStamp < A_Now) {
        return "Po termínu"
    }

    cutoff := DateAdd(A_Now, reminderDays, "Days")
    if (dueStamp <= cutoff) {
        daysLeft := DateDiff(dueStamp, A_Now, "Days")
        if (daysLeft < 1) {
            return "Tento měsíc"
        }
        return "Do " daysLeft " dní"
    }

    return ""
}

BuildReminderMessage(upcoming) {
    first := upcoming[1]
    lines := []

    if (first.dueStamp < A_Now) {
        lines.Push("Po termínu TK je vozidlo " first.vehicle.name " (" first.vehicle.nextTk ").")
    } else {
        lines.Push("Blíží se TK pro vozidlo " first.vehicle.name " (" first.vehicle.nextTk ").")
    }

    if (upcoming.Length > 1) {
        lines.Push("Pozornost vyžaduje ještě dalších " (upcoming.Length - 1) " vozidel.")
    }

    maxList := upcoming.Length < 4 ? upcoming.Length : 4
    Loop maxList {
        item := upcoming[A_Index]
        prefix := (item.dueStamp < A_Now) ? "Po termínu" : "Termín"
        lines.Push(prefix ": " item.vehicle.name " - " item.vehicle.nextTk)
    }

    return JoinLines(lines)
}

BuildGreenCardReminderMessage(upcoming) {
    first := upcoming[1]
    lines := []

    if (first.dueStamp < A_Now) {
        lines.Push("Po termínu zelené karty je vozidlo " first.vehicle.name " (" first.vehicle.greenCardTo ").")
    } else {
        lines.Push("Blíží se konec zelené karty pro vozidlo " first.vehicle.name " (" first.vehicle.greenCardTo ").")
    }

    if (upcoming.Length > 1) {
        lines.Push("Pozornost vyžaduje ještě dalších " (upcoming.Length - 1) " vozidel.")
    }

    if HasAnyMissingGreenCard() {
        lines.Push("U některých vozidel zelená karta vyplněná není a můžete ji doplnit v editaci vozidla.")
    }

    maxList := upcoming.Length < 4 ? upcoming.Length : 4
    Loop maxList {
        item := upcoming[A_Index]
        prefix := (item.dueStamp < A_Now) ? "Po termínu" : "Termín"
        lines.Push(prefix ": " item.vehicle.name " - " item.vehicle.greenCardTo)
    }

    return JoinLines(lines)
}

BuildCustomReminderMessage(upcoming) {
    first := upcoming[1]
    lines := []

    if (first.dueStamp < A_Now) {
        lines.Push("Po termínu připomínky je vozidlo " first.vehicle.name ": " first.reminder.title " (" first.reminder.dueDate ").")
    } else {
        lines.Push("Blíží se připomínka pro vozidlo " first.vehicle.name ": " first.reminder.title " (" first.reminder.dueDate ").")
    }

    if (upcoming.Length > 1) {
        lines.Push("Pozornost vyžaduje ještě dalších " (upcoming.Length - 1) " připomínek.")
    }

    maxList := upcoming.Length < 4 ? upcoming.Length : 4
    Loop maxList {
        item := upcoming[A_Index]
        prefix := (item.dueStamp < A_Now) ? "Po termínu" : "Termín"
        lines.Push(prefix ": " item.vehicle.name " - " item.reminder.title " - " item.reminder.dueDate)
    }

    return JoinLines(lines)
}

BuildAutomaticReminderMessage(upcoming, greenCards, reminders, maintenance, showTechnicalAlert := false, showGreenAlert := false, showReminderAlert := false, showMaintenanceAlert := false) {
    lines := []

    if (showTechnicalAlert && upcoming.Length > 0) {
        lines.Push(BuildReminderSummaryLine(upcoming))
    }
    if (showGreenAlert && greenCards.Length > 0) {
        lines.Push(BuildGreenCardReminderSummaryLine(greenCards))
    }
    if (showReminderAlert && reminders.Length > 0) {
        lines.Push(BuildCustomReminderSummaryLine(reminders))
    }
    if (showMaintenanceAlert && maintenance.Length > 0) {
        lines.Push(BuildMaintenanceReminderSummaryLine(maintenance))
    }
    if ((showGreenAlert || showReminderAlert) && greenCards.Length > 0 && HasAnyMissingGreenCard()) {
        lines.Push("U některých vozidel zelená karta vyplněná není a můžete ji doplnit v editaci vozidla.")
    }

    return JoinLines(lines)
}

BuildReminderSummaryLine(upcoming) {
    first := upcoming[1]
    if (first.dueStamp < A_Now) {
        line := "TK po termínu: " first.vehicle.name " (" first.vehicle.nextTk ")."
    } else {
        line := "Blíží se TK: " first.vehicle.name " (" first.vehicle.nextTk ")."
    }

    if (upcoming.Length > 1) {
        line .= " Další TK: " (upcoming.Length - 1) " vozidel."
    }

    return line
}

BuildGreenCardReminderSummaryLine(upcoming) {
    first := upcoming[1]
    if (first.dueStamp < A_Now) {
        line := "ZK po termínu: " first.vehicle.name " (" first.vehicle.greenCardTo ")."
    } else {
        line := "Blíží se konec ZK: " first.vehicle.name " (" first.vehicle.greenCardTo ")."
    }

    if (upcoming.Length > 1) {
        line .= " Další ZK: " (upcoming.Length - 1) " vozidel."
    }

    return line
}

BuildCustomReminderSummaryLine(upcoming) {
    first := upcoming[1]
    if (first.dueStamp < A_Now) {
        line := "Připomínka po termínu: " first.vehicle.name " - " first.reminder.title " (" first.reminder.dueDate ")."
    } else {
        line := "Blíží se připomínka: " first.vehicle.name " - " first.reminder.title " (" first.reminder.dueDate ")."
    }

    if (upcoming.Length > 1) {
        line .= " Další připomínky: " (upcoming.Length - 1) " položek."
    }

    return line
}

BuildMaintenanceReminderSummaryLine(upcoming) {
    first := upcoming[1]
    if IsVehicleMaintenanceSnapshotOverdue(first.snapshot) {
        line := "Údržba po termínu: " first.vehicle.name " - " first.snapshot.title " (" first.snapshot.statusText ")."
    } else {
        line := "Blíží se údržba: " first.vehicle.name " - " first.snapshot.title " (" first.snapshot.statusText ")."
    }

    if (upcoming.Length > 1) {
        line .= " Další servisní úkony: " (upcoming.Length - 1) " položek."
    }

    return line
}

BuildAlertSignature(upcoming) {
    signature := SubStr(A_Now, 1, 8) "|"
    maxList := upcoming.Length < 5 ? upcoming.Length : 5

    Loop maxList {
        item := upcoming[A_Index]
        signature .= item.vehicle.id ":" item.vehicle.nextTk ";"
    }

    return signature
}

BuildGreenCardAlertSignature(upcoming) {
    signature := SubStr(A_Now, 1, 8) "|"
    maxList := upcoming.Length < 5 ? upcoming.Length : 5

    Loop maxList {
        item := upcoming[A_Index]
        signature .= item.vehicle.id ":" item.vehicle.greenCardTo ";"
    }

    return signature
}

BuildCustomReminderAlertSignature(upcoming) {
    signature := SubStr(A_Now, 1, 8) "|"
    maxList := upcoming.Length < 5 ? upcoming.Length : 5

    Loop maxList {
        item := upcoming[A_Index]
        signature .= item.vehicle.id ":" item.reminder.id ":" item.reminder.dueDate ";"
    }

    return signature
}

BuildMaintenanceAlertSignature(upcoming) {
    signature := SubStr(A_Now, 1, 8) "|"
    maxList := upcoming.Length < 5 ? upcoming.Length : 5

    Loop maxList {
        item := upcoming[A_Index]
        signature .= item.vehicle.id ":" item.plan.id ":" item.snapshot.statusText ";"
    }

    return signature
}

ShouldShowAlert(signature, kind := "technical") {
    global SettingsFile

    today := SubStr(A_Now, 1, 8)
    if (kind = "green") {
        dayKey := "last_green_alert_day"
        signatureKey := "last_green_alert_signature"
    } else if (kind = "reminder") {
        dayKey := "last_reminder_alert_day"
        signatureKey := "last_reminder_alert_signature"
    } else if (kind = "maintenance") {
        dayKey := "last_maintenance_alert_day"
        signatureKey := "last_maintenance_alert_signature"
    } else {
        dayKey := "last_alert_day"
        signatureKey := "last_alert_signature"
    }

    lastDay := IniRead(SettingsFile, "notifications", dayKey, "")
    lastSignature := IniRead(SettingsFile, "notifications", signatureKey, "")

    if (lastDay = today && lastSignature = signature) {
        return false
    }

    IniWrite(today, SettingsFile, "notifications", dayKey)
    IniWrite(signature, SettingsFile, "notifications", signatureKey)
    return true
}

SetupTrayMenu() {
    global AppTitle

    menu := A_TrayMenu
    try menu.Delete()

    menu.Add("Otevřít " AppTitle, ShowMainWindow)
    menu.Add("Dashboard", OpenDashboard)

    upcoming := GetUpcomingVehicles()
    if (upcoming.Length > 0) {
        menu.Add("Zobrazit nejbližší TK: " ShortenText(upcoming[1].vehicle.name, 40) " (" upcoming[1].vehicle.nextTk ")", OpenNearestDueVehicle)
    } else {
        menu.Add("Zobrazit nejbližší TK: nic nečeká", OpenNearestDueVehicle)
    }

    greenCards := GetUpcomingGreenCards()
    if (greenCards.Length > 0) {
        menu.Add("Zobrazit nejbližší ZK: " ShortenText(greenCards[1].vehicle.name, 40) " (" greenCards[1].vehicle.greenCardTo ")", OpenNearestGreenCardVehicle)
    } else if HasAnyGreenCardConfigured() {
        menu.Add("Zobrazit nejbližší ZK: nic nečeká", OpenNearestGreenCardVehicle)
    } else {
        menu.Add("Zobrazit nejbližší ZK: nevyplněno", OpenNearestGreenCardVehicle)
    }

    reminders := GetUpcomingCustomReminders()
    if (reminders.Length > 0) {
        menu.Add("Zobrazit nejbližší připomínku: " ShortenText(reminders[1].vehicle.name, 26) " - " ShortenText(reminders[1].reminder.title, 20), OpenNearestReminder)
    } else {
        menu.Add("Zobrazit nejbližší připomínku: nic nečeká", OpenNearestReminder)
    }

    menu.Add("Zkontrolovat technické kontroly", ManualDueCheck)
    menu.Add("Zkontrolovat zelené karty", ManualGreenCardCheck)
    menu.Add("Zkontrolovat připomínky", ManualReminderCheck)
    menu.Add("Přehled všech termínů", OpenUpcomingOverviewDialog)
    menu.Add("Propadlé termíny", OpenOverdueDialog)
    menu.Add("Tiskový přehled", OpenPrintableVehicleReport)
    menu.Add("Export dat", ExportAppData)
    menu.Add("Import dat", ImportAppData)
    menu.Add("Nastavení", OpenSettingsDialog)
    menu.Add()
    menu.Add("Konec", ExitVehimap)
    menu.Default := "Otevřít " AppTitle
    menu.ClickCount := 1
    UpdateTrayIconTip()
}

ExitVehimap(*) {
    ExitApp()
}

UpdateStatusBar() {
    global StatusBar, Vehicles

    if !IsObject(StatusBar) {
        return
    }

    category := GetCurrentCategory()
    count := 0
    hiddenInactiveCount := 0
    activeCount := 0
    hideInactive := GetHideInactiveVehiclesEnabled()
    for vehicle in Vehicles {
        if (vehicle.category = category) {
            count += 1
            if IsVehicleInactive(vehicle) {
                hiddenInactiveCount += 1
            } else {
                activeCount += 1
            }
        }
    }

    if hideInactive {
        StatusBar.SetText(category ": " activeCount " aktivních / " count " celkem", 1)
    } else if (hiddenInactiveCount > 0) {
        StatusBar.SetText(category ": " count " vozidel, z toho " hiddenInactiveCount " archivovaných nebo odstavených", 1)
    } else {
        StatusBar.SetText(category ": " count " vozidel", 1)
    }

    upcoming := GetUpcomingVehicles()
    greenCards := GetUpcomingGreenCards()
    reminders := GetUpcomingCustomReminders()
    maintenance := GetUpcomingVehicleMaintenance()

    if (upcoming.Length = 0) {
        tkText := "TK: nic nečeká"
    } else {
        vehicle := upcoming[1].vehicle
        prefix := (upcoming[1].dueStamp < A_Now) ? "TK po termínu" : "TK"
        tkText := prefix ": " vehicle.name " (" vehicle.nextTk ")"
    }

    if (greenCards.Length = 0) {
        if HasAnyGreenCardConfigured() {
            greenText := "ZK: nic nečeká"
        } else {
            greenText := "ZK: nevyplněno"
        }
    } else {
        vehicle := greenCards[1].vehicle
        prefix := (greenCards[1].dueStamp < A_Now) ? "ZK po termínu" : "ZK"
        greenText := prefix ": " vehicle.name " (" vehicle.greenCardTo ")"
    }

    if (reminders.Length = 0) {
        reminderText := "Př: nic nečeká"
    } else {
        prefix := (reminders[1].dueStamp < A_Now) ? "Př po termínu" : "Př"
        reminderText := prefix ": " reminders[1].vehicle.name " (" reminders[1].reminder.dueDate ")"
    }

    if (maintenance.Length = 0) {
        maintenanceText := "Servis: nic nečeká"
    } else {
        prefix := IsVehicleMaintenanceSnapshotOverdue(maintenance[1].snapshot) ? "Servis po termínu" : "Servis"
        maintenanceText := prefix ": " maintenance[1].vehicle.name " (" maintenance[1].snapshot.title ")"
    }

    StatusBar.SetText(tkText " | " greenText " | " reminderText " | " maintenanceText, 2)
}

GetVehicleStatusText(vehicle) {
    parts := []
    technicalReminderDays := GetTechnicalReminderDays()
    greenCardReminderDays := GetGreenCardReminderDays()

    tkStatus := GetExpirationStatusText(vehicle.nextTk, technicalReminderDays)
    if (tkStatus != "") {
        parts.Push("TK: " tkStatus)
    }

    greenStatus := GetExpirationStatusText(vehicle.greenCardTo, greenCardReminderDays)
    if (greenStatus != "") {
        parts.Push("ZK: " greenStatus)
    }

    reminderStatus := GetVehicleReminderStateText(vehicle.id)
    if (reminderStatus != "") {
        parts.Push(reminderStatus)
    }

    maintenanceStatus := GetVehicleMaintenanceStateText(vehicle.id)
    if (maintenanceStatus != "") {
        parts.Push(maintenanceStatus)
    }

    if (parts.Length = 0) {
        return ""
    }

    return JoinInline(parts, " | ")
}

UpdateTrayIconTip(forceExplorerRefresh := false) {
    global LastTrayIconTip

    tip := BuildTrayIconTip()
    changed := (tip != LastTrayIconTip)

    A_IconTip := tip
    LastTrayIconTip := tip

    if (changed || forceExplorerRefresh) {
        RefreshTrayIdentityLater()
    }
}

BuildTrayIconTip() {
    global AppTitle

    counts := GetTrayAttentionCounts()
    if (
        counts.overdueTechnical = 0
        && counts.overdueGreen = 0
        && counts.overdueReminders = 0
        && counts.overdueMaintenance = 0
        && counts.upcomingTechnical = 0
        && counts.upcomingGreen = 0
        && counts.upcomingReminders = 0
        && counts.upcomingMaintenance = 0
    ) {
        return AppTitle
    }

    tip := AppTitle
        . " - po termínu "
        . counts.overdueTechnical " TK / " counts.overdueGreen " ZK"
        . ", brzy vyprší "
        . counts.upcomingTechnical " TK / " counts.upcomingGreen " ZK"
    if (counts.overdueReminders > 0 || counts.upcomingReminders > 0) {
        tip .= ", připomínky " counts.overdueReminders " po termínu / " counts.upcomingReminders " brzy"
    }
    if (counts.overdueMaintenance > 0 || counts.upcomingMaintenance > 0) {
        tip .= ", servis " counts.overdueMaintenance " po termínu / " counts.upcomingMaintenance " brzy"
    }
    return tip
}

GetTrayAttentionCounts() {
    global Vehicles

    counts := {
        overdueTechnical: 0,
        overdueGreen: 0,
        upcomingTechnical: 0,
        upcomingGreen: 0,
        overdueReminders: 0,
        upcomingReminders: 0,
        overdueMaintenance: 0,
        upcomingMaintenance: 0
    }

    technicalCutoff := DateAdd(A_Now, GetTechnicalReminderDays(), "Days")
    greenCutoff := DateAdd(A_Now, GetGreenCardReminderDays(), "Days")

    for vehicle in Vehicles {
        technicalDueStamp := ParseDueStamp(vehicle.nextTk)
        if (technicalDueStamp != "") {
            if (technicalDueStamp < A_Now) {
                counts.overdueTechnical += 1
            } else if (technicalDueStamp <= technicalCutoff) {
                counts.upcomingTechnical += 1
            }
        }

        greenDueStamp := ParseDueStamp(vehicle.greenCardTo)
        if (greenDueStamp != "") {
            if (greenDueStamp < A_Now) {
                counts.overdueGreen += 1
            } else if (greenDueStamp <= greenCutoff) {
                counts.upcomingGreen += 1
            }
        }
    }

    for item in GetUpcomingCustomReminders() {
        if (item.dueStamp < A_Now) {
            counts.overdueReminders += 1
        } else {
            counts.upcomingReminders += 1
        }
    }

    for item in GetUpcomingVehicleMaintenance() {
        if IsVehicleMaintenanceSnapshotOverdue(item.snapshot) {
            counts.overdueMaintenance += 1
        } else {
            counts.upcomingMaintenance += 1
        }
    }

    return counts
}
