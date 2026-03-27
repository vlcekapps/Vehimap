OpenVehicleRecordsDialog(vehicle, openAddEntry := false, selectEntryId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordsVehicleId, RecordsList, RecordsSummaryLabel, RecordsAllEntries, RecordsSearchCtrl, RecordsPathStatusLabel, VisibleRecordIds, RecordsSortColumn, RecordsSortDescending, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton, RecordFormGui

    if IsObject(RecordsGui) {
        WinActivate("ahk_id " RecordsGui.Hwnd)
        return
    }

    if IsObject(FormGui) {
        WinActivate("ahk_id " FormGui.Hwnd)
        return
    }

    if IsObject(SettingsGui) {
        WinActivate("ahk_id " SettingsGui.Hwnd)
        return
    }

    if IsObject(OverviewGui) {
        WinActivate("ahk_id " OverviewGui.Hwnd)
        return
    }

    if IsObject(OverdueGui) {
        WinActivate("ahk_id " OverdueGui.Hwnd)
        return
    }

    if IsObject(DetailGui) {
        WinActivate("ahk_id " DetailGui.Hwnd)
        return
    }

    if IsObject(HistoryGui) {
        WinActivate("ahk_id " HistoryGui.Hwnd)
        return
    }

    if IsObject(HistoryFormGui) {
        WinActivate("ahk_id " HistoryFormGui.Hwnd)
        return
    }

    if IsObject(FuelGui) {
        WinActivate("ahk_id " FuelGui.Hwnd)
        return
    }

    if IsObject(FuelFormGui) {
        WinActivate("ahk_id " FuelFormGui.Hwnd)
        return
    }

    if IsObject(RecordFormGui) {
        WinActivate("ahk_id " RecordFormGui.Hwnd)
        return
    }

    ShowMainWindow()

    RecordsVehicleId := vehicle.id
    RecordsAllEntries := []
    RecordsSearchCtrl := 0
    RecordsPathStatusLabel := 0
    VisibleRecordIds := []
    RecordsSortColumn := GetRecordsSortColumnSetting()
    RecordsSortDescending := GetRecordsSortDescendingSetting()
    RecordsOpenFileButton := 0
    RecordsOpenFolderButton := 0
    RecordsCopyPathButton := 0
    RecordsGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Pojištění a doklady")
    RecordsGui.SetFont("s10", "Segoe UI")
    RecordsGui.OnEvent("Close", CloseVehicleRecordsDialog)
    RecordsGui.OnEvent("Escape", CloseVehicleRecordsDialog)

    MainGui.Opt("+Disabled")

    RecordsGui.AddText("x20 y20 w820", "Zde můžete evidovat pojištění, doklady a další soubory k vozidlu " vehicle.name ".")
    RecordsSummaryLabel := RecordsGui.AddText("x20 y50 w820", "")
    RecordsGui.AddText("x20 y82 w280", "Hledat druh, název, poskytovatele, platnost nebo soubor")
    RecordsSearchCtrl := RecordsGui.AddEdit("x310 y79 w360")
    RecordsSearchCtrl.OnEvent("Change", OnRecordsSearchChanged)

    RecordsList := RecordsGui.AddListView("x20 y112 w980 h220 Grid -Multi", ["Druh", "Název", "Poskytovatel", "Platné do", "Cena", "Soubor", "Stav cesty"])
    RecordsList.OnEvent("DoubleClick", EditSelectedVehicleRecord)
    RecordsList.OnEvent("ItemSelect", OnRecordsSelectionChanged)
    RecordsList.OnEvent("ColClick", OnRecordsColumnClick)
    RecordsList.ModifyCol(1, "130")
    RecordsList.ModifyCol(2, "170")
    RecordsList.ModifyCol(3, "150")
    RecordsList.ModifyCol(4, "85")
    RecordsList.ModifyCol(5, "95")
    RecordsList.ModifyCol(6, "210")
    RecordsList.ModifyCol(7, "110")

    RecordsPathStatusLabel := RecordsGui.AddText("x20 y345 w980 h38", "")

    addButton := RecordsGui.AddButton("x25 y392 w120 h30", "Přidat záznam")
    addButton.OnEvent("Click", AddVehicleRecord)

    editButton := RecordsGui.AddButton("x155 y392 w120 h30", "Upravit záznam")
    editButton.OnEvent("Click", EditSelectedVehicleRecord)

    deleteButton := RecordsGui.AddButton("x285 y392 w130 h30", "Odstranit záznam")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleRecord)

    RecordsOpenFileButton := RecordsGui.AddButton("x425 y392 w120 h30", "Otevřít soubor")
    RecordsOpenFileButton.OnEvent("Click", OpenSelectedVehicleRecordFile)

    RecordsOpenFolderButton := RecordsGui.AddButton("x555 y392 w130 h30", "Otevřít složku")
    RecordsOpenFolderButton.OnEvent("Click", OpenSelectedVehicleRecordFolder)

    RecordsCopyPathButton := RecordsGui.AddButton("x695 y392 w130 h30", "Kopírovat cestu")
    RecordsCopyPathButton.OnEvent("Click", CopySelectedVehicleRecordPath)

    detailButton := RecordsGui.AddButton("x835 y392 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromRecords)

    closeButton := RecordsGui.AddButton("x965 y392 w80 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleRecordsDialog)

    RecordsGui.Show("w1065 h442")
    PopulateVehicleRecordsList(selectEntryId, true)

    if openAddEntry {
        OpenVehicleRecordForm("add")
    } else if (VisibleRecordIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleRecordsDialog(*) {
    global RecordsGui, RecordsVehicleId, RecordsList, RecordsSummaryLabel, RecordsAllEntries, RecordsSearchCtrl, RecordsPathStatusLabel, VisibleRecordIds, RecordsSortColumn, RecordsSortDescending, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton, MainGui

    if IsObject(RecordsGui) {
        RecordsGui.Destroy()
        RecordsGui := 0
    }

    RecordsVehicleId := ""
    RecordsList := 0
    RecordsSummaryLabel := 0
    RecordsAllEntries := []
    RecordsSearchCtrl := 0
    RecordsPathStatusLabel := 0
    VisibleRecordIds := []
    RecordsSortColumn := 4
    RecordsSortDescending := false
    RecordsOpenFileButton := 0
    RecordsOpenFolderButton := 0
    RecordsCopyPathButton := 0
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleRecordsList(selectEntryId := "", focusList := false) {
    global RecordsGui, RecordsVehicleId, RecordsList, RecordsSummaryLabel, RecordsAllEntries, VisibleRecordIds

    if !IsObject(RecordsGui) || !IsObject(RecordsList) {
        return
    }

    RecordsAllEntries := GetVehicleRecords(RecordsVehicleId)
    entries := FilterVehicleRecordsBySearch(RecordsAllEntries, GetRecordsSearchText())
    SortVisibleVehicleRecords(&entries)
    VisibleRecordIds := []
    selectedRow := 0

    if IsObject(RecordsSummaryLabel) {
        RecordsSummaryLabel.Text := BuildVehicleRecordsSummaryText(RecordsVehicleId)
    }

    RecordsList.Opt("-Redraw")
    RecordsList.Delete()
    for entry in entries {
        row := RecordsList.Add(
            "",
            entry.recordType,
            entry.title,
            entry.provider,
            entry.validTo,
            entry.price,
            ShortenText(GetFileNameFromPath(entry.filePath), 32),
            GetVehicleRecordPathStateText(entry)
        )
        VisibleRecordIds.Push(entry.id)
        if (selectEntryId != "" && entry.id = selectEntryId) {
            selectedRow := row
        }
    }
    RecordsList.Opt("+Redraw")

    if (entries.Length = 0) {
        UpdateVehicleRecordActionState()
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    RecordsList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
    UpdateVehicleRecordActionState()
}

OnRecordsSearchChanged(*) {
    selectedEntryId := ""
    entry := GetSelectedVehicleRecord()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleRecordsList(selectedEntryId)
}

OnRecordsSelectionChanged(*) {
    UpdateVehicleRecordActionState()
}

OnRecordsColumnClick(ctrl, column) {
    global RecordsSortColumn, RecordsSortDescending

    if (RecordsSortColumn = column) {
        RecordsSortDescending := !RecordsSortDescending
    } else {
        RecordsSortColumn := column
        RecordsSortDescending := false
    }

    SaveRecordsSortSettings(RecordsSortColumn, RecordsSortDescending)

    selectedEntryId := ""
    entry := GetSelectedVehicleRecord()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleRecordsList(selectedEntryId, true)
}

GetRecordsSearchText() {
    global RecordsSearchCtrl

    if !IsObject(RecordsSearchCtrl) {
        return ""
    }

    return Trim(RecordsSearchCtrl.Text)
}

FilterVehicleRecordsBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        haystack := StrLower(entry.recordType " " entry.title " " entry.provider " " entry.validFrom " " entry.validTo " " entry.price " " entry.filePath " " GetFileNameFromPath(entry.filePath) " " entry.note)
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

SortVisibleVehicleRecords(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleRecords(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleRecords(left, right) {
    global RecordsSortColumn, RecordsSortDescending

    result := CompareVisibleVehicleRecordsByColumn(left, right, RecordsSortColumn)
    if (result = 0 && RecordsSortColumn != 4) {
        result := CompareVisibleVehicleRecordsByColumn(left, right, 4)
    }
    if (result = 0) {
        result := CompareTextValues(left.id, right.id)
    }

    return RecordsSortDescending ? -result : result
}

CompareVisibleVehicleRecordsByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareTextValues(left.recordType, right.recordType)
        case 2:
            return CompareTextValues(left.title, right.title)
        case 3:
            return CompareTextValues(left.provider, right.provider)
        case 4:
            return CompareOptionalStampValues(ParseDueStamp(left.validTo), ParseDueStamp(right.validTo))
        case 5:
            return CompareOptionalMoneyTexts(left.price, right.price)
        case 6:
            return CompareTextValues(GetFileNameFromPath(left.filePath), GetFileNameFromPath(right.filePath))
        case 7:
            return CompareNumberValues(GetVehicleRecordPathStateSortValue(left), GetVehicleRecordPathStateSortValue(right))
    }

    return 0
}

UpdateVehicleRecordActionState() {
    global RecordsPathStatusLabel, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        if IsObject(RecordsPathStatusLabel) {
            RecordsPathStatusLabel.Text := "Vyberte záznam, chcete-li zobrazit stav cesty k souboru nebo složce."
        }
        if IsObject(RecordsOpenFileButton) {
            RecordsOpenFileButton.Opt("+Disabled")
        }
        if IsObject(RecordsOpenFolderButton) {
            RecordsOpenFolderButton.Opt("+Disabled")
        }
        if IsObject(RecordsCopyPathButton) {
            RecordsCopyPathButton.Opt("+Disabled")
        }
        return
    }

    pathInfo := GetVehicleRecordPathInfo(entry)
    if IsObject(RecordsPathStatusLabel) {
        RecordsPathStatusLabel.Text := BuildVehicleRecordPathStatusText(pathInfo)
    }

    if IsObject(RecordsOpenFileButton) {
        RecordsOpenFileButton.Opt(pathInfo.kind = "file" ? "-Disabled" : "+Disabled")
    }
    if IsObject(RecordsOpenFolderButton) {
        RecordsOpenFolderButton.Opt(pathInfo.folderPath != "" ? "-Disabled" : "+Disabled")
    }
    if IsObject(RecordsCopyPathButton) {
        RecordsCopyPathButton.Opt(pathInfo.inputPath != "" ? "-Disabled" : "+Disabled")
    }
}

GetVehicleRecordPathInfo(entry) {
    path := Trim(entry.filePath)
    resolvedPath := ResolveVehicleRecordPath(path)

    if (path = "") {
        return {
            kind: "empty",
            inputPath: "",
            resolvedPath: "",
            folderPath: "",
            exists: false
        }
    }

    if DirExist(resolvedPath) {
        return {
            kind: "folder",
            inputPath: path,
            resolvedPath: resolvedPath,
            folderPath: resolvedPath,
            exists: true
        }
    }

    if FileExist(resolvedPath) {
        SplitPath(resolvedPath, , &directoryPath)
        return {
            kind: "file",
            inputPath: path,
            resolvedPath: resolvedPath,
            folderPath: directoryPath,
            exists: true
        }
    }

    SplitPath(resolvedPath, , &parentDirectory)
    if (parentDirectory != "" && DirExist(parentDirectory)) {
        return {
            kind: "missing_file",
            inputPath: path,
            resolvedPath: resolvedPath,
            folderPath: parentDirectory,
            exists: false
        }
    }

    return {
        kind: "missing_folder",
        inputPath: path,
        resolvedPath: resolvedPath,
        folderPath: "",
        exists: false
    }
}

ResolveVehicleRecordPath(path) {
    path := Trim(path)
    if (path = "") {
        return ""
    }

    if RegExMatch(path, "i)^[a-z]:[\\/]" ) || RegExMatch(path, "^\\\\") || RegExMatch(path, "^[\\/]") {
        return path
    }

    return A_ScriptDir "\" path
}

GetVehicleRecordPathStateText(entry) {
    return GetVehicleRecordPathStateLabel(GetVehicleRecordPathInfo(entry).kind)
}

GetVehicleRecordPathStateLabel(kind) {
    switch kind {
        case "file":
            return "Soubor"
        case "folder":
            return "Složka"
        case "missing_file":
            return "Chybí soubor"
        case "missing_folder":
            return "Chybí složka"
        default:
            return "Bez cesty"
    }
}

GetVehicleRecordPathStateSortValue(entry) {
    switch GetVehicleRecordPathInfo(entry).kind {
        case "file":
            return 1
        case "folder":
            return 2
        case "missing_file":
            return 3
        case "missing_folder":
            return 4
        default:
            return 5
    }
}

BuildVehicleRecordPathStatusText(pathInfo) {
    switch pathInfo.kind {
        case "file":
            return "Stav cesty: soubor je dostupný. Cesta: " pathInfo.inputPath
        case "folder":
            return "Stav cesty: záznam míří na existující složku. Cesta: " pathInfo.inputPath
        case "missing_file":
            return "Stav cesty: složka existuje, ale soubor chybí. Cesta: " pathInfo.inputPath
        case "missing_folder":
            return "Stav cesty: cílová složka ani soubor nejsou dostupné. Cesta: " pathInfo.inputPath
        default:
            return "Stav cesty: u vybraného záznamu není vyplněná cesta k souboru ani složce."
    }
}

GetSelectedVehicleRecord() {
    global RecordsList, VisibleRecordIds

    if !IsObject(RecordsList) {
        return ""
    }

    row := RecordsList.GetNext(0)
    if !row || row > VisibleRecordIds.Length {
        return ""
    }

    return FindVehicleRecordById(VisibleRecordIds[row])
}

AddVehicleRecord(*) {
    OpenVehicleRecordForm("add")
}

EditSelectedVehicleRecord(*) {
    global AppTitle

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, který chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleRecordForm("edit", entry)
}

DeleteSelectedVehicleRecord(*) {
    global AppTitle, VehicleRecords

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, který chcete odstranit.", AppTitle, 0x40)
        return
    }

    result := MsgBox("Opravdu chcete odstranit záznam " entry.title "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleRecordIndexById(entry.id)
    if !index {
        return
    }

    VehicleRecords.RemoveAt(index)
    SaveVehicleRecords()
    PopulateVehicleRecordsList()
}

OpenSelectedVehicleRecordFile(*) {
    global AppTitle

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, jehož soubor chcete otevřít.", AppTitle, 0x40)
        return
    }

    pathInfo := GetVehicleRecordPathInfo(entry)
    switch pathInfo.kind {
        case "empty":
            MsgBox("Vybraný záznam nemá vyplněnou cestu k souboru.", AppTitle, 0x40)
            return
        case "folder":
            MsgBox("Vybraná cesta vede na složku. Pro ni použijte tlačítko nebo zkratku pro otevření složky.", AppTitle, 0x40)
            return
        case "missing_file":
            MsgBox("Složka pro vybraný záznam existuje, ale soubor se na zadané cestě nenašel.`n`n" pathInfo.inputPath, AppTitle, 0x30)
            return
        case "missing_folder":
            MsgBox("Zadaná cesta není dostupná, protože chybí cílová složka nebo soubor.`n`n" pathInfo.inputPath, AppTitle, 0x30)
            return
    }

    try {
        Run('"' pathInfo.resolvedPath '"')
    } catch as err {
        MsgBox("Soubor se nepodařilo otevřít.`n`n" err.Message, AppTitle, 0x30)
    }
}

OpenSelectedVehicleRecordFolder(*) {
    global AppTitle

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, jehož složku chcete otevřít.", AppTitle, 0x40)
        return
    }

    pathInfo := GetVehicleRecordPathInfo(entry)
    if (pathInfo.kind = "empty") {
        MsgBox("Vybraný záznam nemá vyplněnou cestu k souboru.", AppTitle, 0x40)
        return
    }

    if (pathInfo.folderPath = "") {
        MsgBox("Nepodařilo se najít existující složku pro vybraný záznam.`n`n" pathInfo.inputPath, AppTitle, 0x30)
        return
    }

    try {
        Run('"' pathInfo.folderPath '"')
    } catch as err {
        MsgBox("Složku se nepodařilo otevřít.`n`n" err.Message, AppTitle, 0x30)
    }
}

CopySelectedVehicleRecordPath(*) {
    global AppTitle

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, jehož cestu chcete zkopírovat.", AppTitle, 0x40)
        return
    }

    path := Trim(entry.filePath)
    if (path = "") {
        MsgBox("Vybraný záznam nemá vyplněnou cestu k souboru.", AppTitle, 0x40)
        return
    }

    A_Clipboard := path
    MsgBox("Cesta byla zkopírována do schránky.`n`n" path, AppTitle, 0x40)
}

OpenVehicleDetailFromRecords(*) {
    global RecordsVehicleId

    vehicle := FindVehicleById(RecordsVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleRecordsDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleRecordForm(mode, entry := "") {
    global AppTitle, RecordFormGui, RecordFormControls, RecordFormMode, RecordFormEntryId, RecordFormVehicleId, RecordsGui, RecordsVehicleId, RecordTypeOptions

    if IsObject(RecordFormGui) {
        WinActivate("ahk_id " RecordFormGui.Hwnd)
        return
    }

    if !IsObject(RecordsGui) {
        return
    }

    RecordFormMode := mode
    RecordFormEntryId := IsObject(entry) ? entry.id : ""
    RecordFormVehicleId := IsObject(entry) ? entry.vehicleId : RecordsVehicleId
    RecordFormControls := {}

    title := (mode = "edit") ? "Upravit záznam" : "Přidat záznam"
    RecordFormGui := Gui("+Owner" RecordsGui.Hwnd, AppTitle " - " title)
    RecordFormGui.SetFont("s10", "Segoe UI")
    RecordFormGui.OnEvent("Close", CloseVehicleRecordForm)
    RecordFormGui.OnEvent("Escape", CloseVehicleRecordForm)

    RecordsGui.Opt("+Disabled")

    RecordFormGui.AddText("x20 y20 w520", "Druh a název záznamu jsou povinné. Platnost pojištění nebo dokladu zadejte jako MM/RRRR.")

    RecordFormGui.AddText("x20 y60 w170", "Druh záznamu (povinné)")
    RecordFormControls.recordType := RecordFormGui.AddDropDownList("x210 y57 w240", RecordTypeOptions)

    RecordFormGui.AddText("x20 y95 w170", "Název záznamu (povinné)")
    RecordFormControls.title := RecordFormGui.AddEdit("x210 y92 w240")

    RecordFormGui.AddText("x20 y130 w170", "Poskytovatel / vydavatel")
    RecordFormControls.provider := RecordFormGui.AddEdit("x210 y127 w240")

    RecordFormGui.AddText("x20 y165 w170", "Platné od (volitelné)")
    RecordFormControls.validFrom := RecordFormGui.AddEdit("x210 y162 w110")

    RecordFormGui.AddText("x335 y165 w115", "Platné do (volitelné)")
    RecordFormControls.validTo := RecordFormGui.AddEdit("x440 y162 w110")

    RecordFormGui.AddText("x20 y200 w170", "Cena / částka (volitelné)")
    RecordFormControls.price := RecordFormGui.AddEdit("x210 y197 w240")

    RecordFormGui.AddText("x20 y235 w170", "Soubor nebo cesta")
    RecordFormControls.filePath := RecordFormGui.AddEdit("x20 y260 w435")
    browseButton := RecordFormGui.AddButton("x465 y258 w85 h26", "Vybrat")
    browseButton.OnEvent("Click", SelectVehicleRecordFile)

    RecordFormGui.AddText("x20 y295 w170", "Poznámka (volitelné)")
    RecordFormControls.note := RecordFormGui.AddEdit("x20 y320 w530 h80 Multi")

    saveButton := RecordFormGui.AddButton("x180 y415 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleRecordFromForm)

    cancelButton := RecordFormGui.AddButton("x310 y415 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleRecordForm)

    if IsObject(entry) {
        SetDropDownToText(RecordFormControls.recordType, entry.recordType, RecordTypeOptions)
        RecordFormControls.title.Text := entry.title
        RecordFormControls.provider.Text := entry.provider
        RecordFormControls.validFrom.Text := entry.validFrom
        RecordFormControls.validTo.Text := entry.validTo
        RecordFormControls.price.Text := entry.price
        RecordFormControls.filePath.Text := entry.filePath
        RecordFormControls.note.Text := entry.note
    } else {
        RecordFormControls.recordType.Value := 1
    }

    RecordFormGui.Show("w580 h460")
    RecordFormControls.recordType.Focus()
}

CloseVehicleRecordForm(*) {
    global RecordFormGui, RecordFormControls, RecordFormMode, RecordFormEntryId, RecordFormVehicleId, RecordsGui

    if IsObject(RecordFormGui) {
        RecordFormGui.Destroy()
        RecordFormGui := 0
    }

    RecordFormControls := {}
    RecordFormMode := ""
    RecordFormEntryId := ""
    RecordFormVehicleId := ""

    if IsObject(RecordsGui) {
        RecordsGui.Opt("-Disabled")
        WinActivate("ahk_id " RecordsGui.Hwnd)
    }
}

SelectVehicleRecordFile(*) {
    global AppTitle, A_DefaultDialogTitle, RecordFormControls

    A_DefaultDialogTitle := AppTitle
    selectedPath := FileSelect(1, A_ScriptDir, "Vyberte soubor k záznamu")
    if (selectedPath = "") {
        return
    }

    RecordFormControls.filePath.Text := selectedPath
}

SaveVehicleRecordFromForm(*) {
    global AppTitle, VehicleRecords, RecordFormControls, RecordFormMode, RecordFormEntryId, RecordFormVehicleId

    recordType := Trim(RecordFormControls.recordType.Text)
    title := Trim(RecordFormControls.title.Text)
    provider := Trim(RecordFormControls.provider.Text)
    validFrom := NormalizeMonthYear(RecordFormControls.validFrom.Text)
    validTo := NormalizeMonthYear(RecordFormControls.validTo.Text)
    price := Trim(RecordFormControls.price.Text)
    filePath := Trim(RecordFormControls.filePath.Text)
    note := Trim(RecordFormControls.note.Text)

    if (recordType = "") {
        MsgBox("Vyberte prosím druh záznamu.", AppTitle, 0x30)
        RecordFormControls.recordType.Focus()
        return
    }

    if (title = "") {
        MsgBox("Vyplňte prosím název záznamu.", AppTitle, 0x30)
        RecordFormControls.title.Focus()
        return
    }

    if (Trim(RecordFormControls.validFrom.Text) != "" && validFrom = "") {
        MsgBox("Pole Platné od musí být ve formátu MM/RRRR.", AppTitle, 0x30)
        RecordFormControls.validFrom.Focus()
        return
    }

    if (Trim(RecordFormControls.validTo.Text) != "" && validTo = "") {
        MsgBox("Pole Platné do musí být ve formátu MM/RRRR.", AppTitle, 0x30)
        RecordFormControls.validTo.Focus()
        return
    }

    if (validFrom != "" && validTo != "" && ParseDueStamp(validFrom) > ParseDueStamp(validTo)) {
        MsgBox("Pole Platné od nesmí být později než pole Platné do.", AppTitle, 0x30)
        RecordFormControls.validFrom.Focus()
        return
    }

    entry := {
        id: (RecordFormMode = "edit") ? RecordFormEntryId : GenerateVehicleRecordId(),
        vehicleId: RecordFormVehicleId,
        recordType: recordType,
        title: title,
        provider: provider,
        validFrom: validFrom,
        validTo: validTo,
        price: price,
        filePath: filePath,
        note: note
    }

    index := FindVehicleRecordIndexById(entry.id)
    if index {
        VehicleRecords[index] := entry
    } else {
        VehicleRecords.Push(entry)
    }

    SaveVehicleRecords()
    CloseVehicleRecordForm()
    PopulateVehicleRecordsList(entry.id, true)
}
