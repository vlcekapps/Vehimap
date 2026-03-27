ExportAppData(*) {
    global AppTitle, A_DefaultDialogTitle

    A_DefaultDialogTitle := AppTitle
    backupPath := FileSelect("S16", GetDefaultBackupPath(), "Export dat Vehimap", "Vehimap záloha (*.vehimapbak)")
    if (backupPath = "") {
        return
    }

    backupPath := EnsureBackupExtension(backupPath)
    try {
        WriteTextFileUtf8(backupPath, BuildCurrentBackupContent())
        MsgBox("Export dat byl dokončen.`n`nSoubor:`n" backupPath, AppTitle, 0x40)
    } catch as err {
        MsgBox("Export dat se nepodařil.`n`n" err.Message, AppTitle, 0x30)
    }
}

ImportAppData(*) {
    global AppTitle, A_DefaultDialogTitle, SettingsFile, VehiclesFile, HistoryFile, FuelLogFile, RecordsFile, VehicleMetaFile, RemindersFile

    A_DefaultDialogTitle := AppTitle
    backupPath := FileSelect(1, A_ScriptDir, "Import dat Vehimap", "Vehimap záloha (*.vehimapbak)")
    if (backupPath = "") {
        return
    }

    result := MsgBox(
        "Import přepíše aktuální vozidla, historii událostí, kilometry a tankování, pojištění a doklady, stavy a štítky, vlastní připomínky i nastavení aplikace.`n`nPokračovat v importu?",
        AppTitle,
        0x34
    )
    if (result != "Yes") {
        return
    }

    try {
        backupContent := FileRead(backupPath, "UTF-8")
    } catch as err {
        MsgBox("Zvolený soubor se nepodařilo načíst.`n`n" err.Message, AppTitle, 0x30)
        return
    }

    settingsContent := ""
    vehiclesContent := ""
    historyContent := ""
    fuelContent := ""
    recordsContent := ""
    metaContent := ""
    remindersContent := ""
    errorMessage := ""
    if !TryParseBackupContent(backupContent, &settingsContent, &vehiclesContent, &historyContent, &fuelContent, &recordsContent, &metaContent, &remindersContent, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedVehicles := []
    if !TryParseVehiclesBackupContent(vehiclesContent, &importedVehicles, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedHistory := []
    if !TryParseHistoryBackupContent(historyContent, &importedHistory, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedFuelLog := []
    if !TryParseFuelBackupContent(fuelContent, &importedFuelLog, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedRecords := []
    if !TryParseRecordsBackupContent(recordsContent, &importedRecords, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedMeta := []
    if !TryParseVehicleMetaBackupContent(metaContent, &importedMeta, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedReminders := []
    if !TryParseVehicleRemindersBackupContent(remindersContent, &importedReminders, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    backupDir := BackupCurrentFilesBeforeImport()

    try {
        WriteTextFileUtf8(VehiclesFile, vehiclesContent)
        WriteTextFileUtf8(HistoryFile, historyContent)
        WriteTextFileUtf8(FuelLogFile, fuelContent)
        WriteTextFileUtf8(RecordsFile, recordsContent)
        WriteTextFileUtf8(VehicleMetaFile, metaContent)
        WriteTextFileUtf8(RemindersFile, remindersContent)
        WriteTextFileUtf8(SettingsFile, settingsContent)
        EnsureSettingsDefaults()
        SetRunAtStartupEnabled(IniRead(SettingsFile, "app", "run_at_startup", "0") = "1")
        LoadVehicles()
        LoadVehicleHistory()
        LoadVehicleFuelLog()
        LoadVehicleRecords()
        LoadVehicleMeta()
        LoadVehicleReminders()
        ResetAlertHistory()
        RefreshVehicleList()
        CheckDueVehicles(false, false)
        UpdateTrayIconTip(true)
    } catch as err {
        MsgBox("Import se nepodařilo dokončit.`n`n" err.Message, AppTitle, 0x30)
        return
    }

    message := "Import dat byl dokončen."
    if (backupDir != "") {
        message .= "`n`nPůvodní soubory byly před importem zálohovány do:`n" backupDir
    }
    MsgBox(message, AppTitle, 0x40)
}
