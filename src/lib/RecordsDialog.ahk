OpenVehicleRecordsDialog(vehicle, openAddEntry := false, selectEntryId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordsVehicleId, RecordsList, RecordsSummaryLabel, RecordsAllEntries, RecordsSearchCtrl, RecordsPathStatusLabel, VisibleRecordIds, RecordsSortColumn, RecordsSortDescending, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton, RecordsLayout, RecordFormGui

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
    RecordsLayout := {}
    RecordsGui := Gui("+Owner" MainGui.Hwnd " +Resize", AppTitle " - Pojištění a doklady")
    RecordsGui.SetFont("s10", "Segoe UI")
    RecordsGui.Opt("+MinSize1065x474")
    RecordsGui.OnEvent("Close", CloseVehicleRecordsDialog)
    RecordsGui.OnEvent("Escape", CloseVehicleRecordsDialog)
    RecordsGui.OnEvent("Size", OnVehicleRecordsDialogSize)

    MainGui.Opt("+Disabled")

    introLabel := RecordsGui.AddText("x20 y20 w980", "Tady spravujete pojištění, doklady a přílohy k vozidlu " vehicle.name ", včetně spravovaných kopií přímo uvnitř Vehimapu.")
    RecordsSummaryLabel := RecordsGui.AddText("x20 y50 w820", "")
    searchLabel := RecordsGui.AddText("x20 y82 w280", "Hledat druh, název, poskytovatele, platnost nebo přílohu")
    RecordsSearchCtrl := RecordsGui.AddEdit("x310 y79 w360")
    RecordsSearchCtrl.OnEvent("Change", OnRecordsSearchChanged)

    RecordsList := RecordsGui.AddListView("x20 y112 w980 h220 Grid -Multi", ["Druh", "Název", "Poskytovatel", "Platné do", "Cena", "Soubor", "Režim", "Stav cesty"])
    RecordsList.OnEvent("DoubleClick", EditSelectedVehicleRecord)
    RecordsList.OnEvent("ItemSelect", OnRecordsSelectionChanged)
    RecordsList.OnEvent("ColClick", OnRecordsColumnClick)
    RecordsList.ModifyCol(1, "130")
    RecordsList.ModifyCol(2, "170")
    RecordsList.ModifyCol(3, "150")
    RecordsList.ModifyCol(4, "85")
    RecordsList.ModifyCol(5, "95")
    RecordsList.ModifyCol(6, "185")
    RecordsList.ModifyCol(7, "100")
    RecordsList.ModifyCol(8, "110")

    RecordsPathStatusLabel := RecordsGui.AddText("x20 y343 w980 h58", "")

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

    RecordsLayout := {
        introLabel: introLabel,
        searchLabel: searchLabel,
        detailButton: detailButton,
        closeButton: closeButton
    }

    RecordsGui.Show("w1065 h474")
    PopulateVehicleRecordsList(selectEntryId, true)

    if openAddEntry {
        OpenVehicleRecordForm("add")
    } else if (VisibleRecordIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleRecordsDialog(*) {
    global RecordsGui, RecordsVehicleId, RecordsList, RecordsSummaryLabel, RecordsAllEntries, RecordsSearchCtrl, RecordsPathStatusLabel, VisibleRecordIds, RecordsSortColumn, RecordsSortDescending, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton, RecordsLayout, MainGui

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
    RecordsLayout := {}
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
            ShortenText(GetVehicleRecordResolvedFileName(entry), 32),
            GetVehicleRecordAttachmentModeLabel(entry),
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
        haystack := StrLower(
            entry.recordType " "
            entry.title " "
            entry.provider " "
            entry.validFrom " "
            entry.validTo " "
            entry.price " "
            entry.filePath " "
            GetVehicleRecordResolvedFileName(entry) " "
            GetVehicleRecordDisplayPath(entry) " "
            GetVehicleRecordAttachmentModeLabel(entry) " "
            entry.note
        )
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
            return CompareTextValues(GetVehicleRecordResolvedFileName(left), GetVehicleRecordResolvedFileName(right))
        case 7:
            return CompareTextValues(GetVehicleRecordAttachmentModeLabel(left), GetVehicleRecordAttachmentModeLabel(right))
        case 8:
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

OnVehicleRecordsDialogSize(guiObj, minMax, width, height) {
    global RecordsLayout, RecordsSummaryLabel, RecordsSearchCtrl, RecordsList, RecordsPathStatusLabel, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton

    if (minMax = -1) {
        return
    }

    buttonY := height - 62
    statusY := buttonY - 67
    listHeight := statusY - 11 - 112

    if IsObject(RecordsLayout) {
        MoveGuiControl(RecordsLayout.introLabel, 20, 20, width - 40)
        MoveGuiControl(RecordsLayout.searchLabel, 20, 82, 280)
        MoveGuiControl(RecordsLayout.detailButton, width - 230, buttonY, 120, 30)
        MoveGuiControl(RecordsLayout.closeButton, width - 100, buttonY, 80, 30)
    }

    MoveGuiControl(RecordsSummaryLabel, 20, 50, width - 40)
    MoveGuiControl(RecordsSearchCtrl, 310, 79, Max(360, width - 395), 23)
    MoveGuiControl(RecordsList, 20, 112, width - 40, listHeight)
    MoveGuiControl(RecordsPathStatusLabel, 20, statusY, width - 40, 58)
    MoveGuiControl(RecordsOpenFileButton, 425, buttonY, 120, 30)
    MoveGuiControl(RecordsOpenFolderButton, 555, buttonY, 130, 30)
    MoveGuiControl(RecordsCopyPathButton, 695, buttonY, 130, 30)
}

GetVehicleRecordPathInfo(entry) {
    path := Trim(entry.filePath)
    mode := GetVehicleRecordAttachmentMode(entry)
    resolvedPath := ResolveVehicleRecordFilePath(entry)

    if (path = "") {
        return {
            kind: "empty",
            mode: mode,
            modeLabel: GetVehicleRecordAttachmentModeLabel(mode),
            inputPath: "",
            resolvedPath: "",
            folderPath: "",
            exists: false
        }
    }

    if DirExist(resolvedPath) {
        return {
            kind: "folder",
            mode: mode,
            modeLabel: GetVehicleRecordAttachmentModeLabel(mode),
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
            mode: mode,
            modeLabel: GetVehicleRecordAttachmentModeLabel(mode),
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
            mode: mode,
            modeLabel: GetVehicleRecordAttachmentModeLabel(mode),
            inputPath: path,
            resolvedPath: resolvedPath,
            folderPath: parentDirectory,
            exists: false
        }
    }

    return {
        kind: "missing_folder",
        mode: mode,
        modeLabel: GetVehicleRecordAttachmentModeLabel(mode),
        inputPath: path,
        resolvedPath: resolvedPath,
        folderPath: "",
        exists: false
    }
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
    storedPathText := (Trim(pathInfo.inputPath) != "") ? pathInfo.inputPath : "nevyplněno"
    resolvedPathText := (Trim(pathInfo.resolvedPath) != "") ? pathInfo.resolvedPath : "není k dispozici"

    switch pathInfo.kind {
        case "file":
            availabilityText := "soubor je dostupný"
        case "folder":
            availabilityText := "cesta míří na existující složku"
        case "missing_file":
            availabilityText := "složka existuje, ale soubor chybí"
        case "missing_folder":
            availabilityText := "cílová složka ani soubor nejsou dostupné"
        default:
            availabilityText := "u záznamu zatím není vyplněná příloha"
    }

    return "Režim přílohy: " pathInfo.modeLabel
        . "`nUložená cesta: " storedPathText
        . "`nVyřešená cesta: " resolvedPathText
        . "`nDostupnost: " availabilityText
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
    PruneVehicleManagedAttachments(entry.vehicleId)
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

    path := GetVehicleRecordDisplayPath(entry)
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
    global RecordFormAttachmentSourcePath, RecordFormInitialAttachmentMode, RecordFormInitialFilePath, RecordFormStatusLabel, RecordFormMoveManagedButton, RecordFormRelinkButton, RecordFormLayout

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
    RecordFormAttachmentSourcePath := ""
    RecordFormInitialAttachmentMode := IsObject(entry) ? GetVehicleRecordAttachmentMode(entry) : "managed"
    RecordFormInitialFilePath := IsObject(entry) ? Trim(entry.filePath) : ""
    RecordFormStatusLabel := 0
    RecordFormMoveManagedButton := 0
    RecordFormRelinkButton := 0
    RecordFormLayout := {}
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

    RecordFormGui.AddText("x20 y235 w170", "Režim přílohy")
    RecordFormControls.attachmentModeManaged := RecordFormGui.AddRadio("x210 y233 w160 Checked", "Spravovaná kopie")
    RecordFormControls.attachmentModeExternal := RecordFormGui.AddRadio("x380 y233 w140", "Externí cesta")
    RecordFormControls.attachmentModeManaged.OnEvent("Click", OnVehicleRecordAttachmentModeChanged)
    RecordFormControls.attachmentModeExternal.OnEvent("Click", OnVehicleRecordAttachmentModeChanged)

    pathLabel := RecordFormGui.AddText("x20 y268 w220", "Spravovaná cesta")
    RecordFormControls.filePath := RecordFormGui.AddEdit("x20 y293 w435")
    browseButton := RecordFormGui.AddButton("x465 y291 w85 h26", "Importovat")
    browseButton.OnEvent("Click", SelectVehicleRecordFile)
    RecordFormControls.browseButton := browseButton

    RecordFormMoveManagedButton := RecordFormGui.AddButton("x20 y327 w140 h28", "Přesunout do příloh")
    RecordFormMoveManagedButton.OnEvent("Click", MoveVehicleRecordAttachmentToManagedCopy)

    RecordFormRelinkButton := RecordFormGui.AddButton("x170 y327 w110 h28", "Znovu propojit")
    RecordFormRelinkButton.OnEvent("Click", RelinkVehicleRecordAttachment)

    RecordFormStatusLabel := RecordFormGui.AddText("x20 y365 w530 h72", "")

    RecordFormGui.AddText("x20 y448 w170", "Poznámka (volitelné)")
    RecordFormControls.note := RecordFormGui.AddEdit("x20 y473 w530 h80 Multi")

    saveButton := RecordFormGui.AddButton("x180 y568 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleRecordFromForm)

    cancelButton := RecordFormGui.AddButton("x310 y568 w120 h30", "Zrušit")
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
        SetVehicleRecordFormAttachmentMode(GetVehicleRecordAttachmentMode(entry))
    } else {
        RecordFormControls.recordType.Value := 1
        SetVehicleRecordFormAttachmentMode("managed")
    }

    RecordFormLayout := {
        pathLabel: pathLabel
    }
    UpdateVehicleRecordFormAttachmentState()
    RecordFormGui.Show("w580 h620")
    RecordFormControls.recordType.Focus()
}

CloseVehicleRecordForm(*) {
    global RecordFormGui, RecordFormControls, RecordFormMode, RecordFormEntryId, RecordFormVehicleId, RecordsGui
    global RecordFormAttachmentSourcePath, RecordFormInitialAttachmentMode, RecordFormInitialFilePath, RecordFormStatusLabel, RecordFormMoveManagedButton, RecordFormRelinkButton, RecordFormLayout

    if IsObject(RecordFormGui) {
        RecordFormGui.Destroy()
        RecordFormGui := 0
    }

    RecordFormControls := {}
    RecordFormMode := ""
    RecordFormEntryId := ""
    RecordFormVehicleId := ""
    RecordFormAttachmentSourcePath := ""
    RecordFormInitialAttachmentMode := ""
    RecordFormInitialFilePath := ""
    RecordFormStatusLabel := 0
    RecordFormMoveManagedButton := 0
    RecordFormRelinkButton := 0
    RecordFormLayout := {}

    if IsObject(RecordsGui) {
        RecordsGui.Opt("-Disabled")
        WinActivate("ahk_id " RecordsGui.Hwnd)
    }
}

GetVehicleRecordFormAttachmentMode() {
    global RecordFormControls

    if IsObject(RecordFormControls) && RecordFormControls.HasOwnProp("attachmentModeExternal") && RecordFormControls.attachmentModeExternal.Value {
        return "external"
    }

    return "managed"
}

SetVehicleRecordFormAttachmentMode(mode) {
    global RecordFormControls

    mode := NormalizeVehicleRecordAttachmentMode(mode)
    if !IsObject(RecordFormControls) || !RecordFormControls.HasOwnProp("attachmentModeManaged") || !RecordFormControls.HasOwnProp("attachmentModeExternal") {
        return
    }

    RecordFormControls.attachmentModeManaged.Value := (mode = "managed")
    RecordFormControls.attachmentModeExternal.Value := (mode = "external")
}

BuildVehicleRecordFormPathInfo() {
    return GetVehicleRecordPathInfo({
        attachmentMode: GetVehicleRecordFormAttachmentMode(),
        filePath: IsObject(RecordFormControls) && RecordFormControls.HasOwnProp("filePath") ? Trim(RecordFormControls.filePath.Text) : ""
    })
}

UpdateVehicleRecordFormAttachmentState() {
    global RecordFormControls, RecordFormStatusLabel, RecordFormMoveManagedButton, RecordFormRelinkButton, RecordFormAttachmentSourcePath, RecordFormLayout

    if !IsObject(RecordFormControls) {
        return
    }

    mode := GetVehicleRecordFormAttachmentMode()
    pathInfo := BuildVehicleRecordFormPathInfo()
    browseText := (mode = "managed") ? "Importovat" : "Vybrat"
    if RecordFormControls.HasOwnProp("browseButton") {
        RecordFormControls.browseButton.Text := browseText
    }
    if RecordFormControls.HasOwnProp("filePath") {
        RecordFormControls.filePath.Opt(mode = "managed" ? "+ReadOnly" : "-ReadOnly")
    }
    if IsObject(RecordFormLayout) && RecordFormLayout.HasOwnProp("pathLabel") && IsObject(RecordFormLayout.pathLabel) {
        RecordFormLayout.pathLabel.Text := (mode = "managed") ? "Spravovaná cesta" : "Cesta k souboru nebo složce"
    }

    if IsObject(RecordFormMoveManagedButton) {
        canMove := (mode = "external" && pathInfo.kind = "file")
        RecordFormMoveManagedButton.Opt(canMove ? "-Disabled" : "+Disabled")
    }
    if IsObject(RecordFormRelinkButton) {
        canRelink := (Trim(pathInfo.inputPath) != "" || mode = "managed")
        RecordFormRelinkButton.Opt(canRelink ? "-Disabled" : "+Disabled")
    }
    if IsObject(RecordFormStatusLabel) {
        statusText := BuildVehicleRecordPathStatusText(pathInfo)
        if (mode = "managed" && Trim(RecordFormAttachmentSourcePath) != "") {
            statusText .= "`nZdroj pro import: " RecordFormAttachmentSourcePath
        }
        RecordFormStatusLabel.Text := statusText
    }

    hooks := GetVehimapTestHooks()
    if IsObject(hooks) {
        hooks.recordFormModeState := {
            mode: mode,
            filePathReadOnly: (mode = "managed"),
            pathLabel: IsObject(RecordFormLayout) && RecordFormLayout.HasOwnProp("pathLabel") ? RecordFormLayout.pathLabel.Text : "",
            statusText: IsObject(RecordFormStatusLabel) ? RecordFormStatusLabel.Text : ""
        }
    }
}

OnVehicleRecordAttachmentModeChanged(*) {
    global RecordFormAttachmentSourcePath

    if (GetVehicleRecordFormAttachmentMode() = "external") {
        RecordFormAttachmentSourcePath := ""
    }

    UpdateVehicleRecordFormAttachmentState()
}

SelectVehicleRecordFile(*) {
    global AppTitle, A_DefaultDialogTitle, RecordFormControls, RecordFormVehicleId, RecordFormAttachmentSourcePath

    A_DefaultDialogTitle := AppTitle
    selectedPath := FileSelect(1, A_ScriptDir, "Vyberte soubor k záznamu")
    if (selectedPath = "") {
        return
    }

    if (GetVehicleRecordFormAttachmentMode() = "managed") {
        preferredRelativePath := Trim(RecordFormControls.filePath.Text)
        if (preferredRelativePath != "" && RecordFormAttachmentSourcePath = "") {
            preferredRelativePath := NormalizeVehicleAttachmentRelativePath(preferredRelativePath)
        }

        RecordFormAttachmentSourcePath := selectedPath
        RecordFormControls.filePath.Text := BuildManagedVehicleAttachmentRelativePath(RecordFormVehicleId, selectedPath, preferredRelativePath)
    } else {
        RecordFormAttachmentSourcePath := ""
        RecordFormControls.filePath.Text := selectedPath
    }

    UpdateVehicleRecordFormAttachmentState()
}

MoveVehicleRecordAttachmentToManagedCopy(*) {
    global AppTitle, RecordFormControls, RecordFormVehicleId, RecordFormAttachmentSourcePath

    pathInfo := GetVehicleRecordPathInfo({
        attachmentMode: "external",
        filePath: IsObject(RecordFormControls) && RecordFormControls.HasOwnProp("filePath") ? Trim(RecordFormControls.filePath.Text) : ""
    })
    if (pathInfo.kind != "file") {
        MsgBox("Do spravovaných příloh lze přesunout jen existující soubor.", AppTitle, 0x30)
        return
    }

    RecordFormAttachmentSourcePath := pathInfo.resolvedPath
    RecordFormControls.filePath.Text := BuildManagedVehicleAttachmentRelativePath(RecordFormVehicleId, pathInfo.resolvedPath, NormalizeVehicleAttachmentRelativePath(RecordFormInitialFilePath))
    SetVehicleRecordFormAttachmentMode("managed")
    UpdateVehicleRecordFormAttachmentState()
}

RelinkVehicleRecordAttachment(*) {
    SelectVehicleRecordFile()
}

SaveVehicleRecordFromForm(*) {
    global AppTitle, VehicleRecords, RecordFormControls, RecordFormMode, RecordFormEntryId, RecordFormVehicleId, RecordFormAttachmentSourcePath

    recordType := Trim(RecordFormControls.recordType.Text)
    title := Trim(RecordFormControls.title.Text)
    provider := Trim(RecordFormControls.provider.Text)
    validFrom := NormalizeMonthYear(RecordFormControls.validFrom.Text)
    validTo := NormalizeMonthYear(RecordFormControls.validTo.Text)
    price := Trim(RecordFormControls.price.Text)
    attachmentMode := GetVehicleRecordFormAttachmentMode()
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

    if (attachmentMode = "managed" && filePath != "" && (RegExMatch(filePath, "i)^[a-z]:[\\/]") || RegExMatch(filePath, "^\\\\") || RegExMatch(filePath, "^[\\/]"))) {
        if (Trim(RecordFormAttachmentSourcePath) = "") {
            MsgBox("Spravovaná kopie musí být uložená relativně do složky data\\attachments. Použijte tlačítko Importovat nebo Znovu propojit.", AppTitle, 0x30)
            RecordFormControls.browseButton.Focus()
            return
        }
    }

    if (attachmentMode = "managed" && filePath != "" && Trim(RecordFormAttachmentSourcePath) != "") {
        try {
            filePath := CopySourceFileToManagedVehicleAttachment(RecordFormVehicleId, RecordFormAttachmentSourcePath, NormalizeVehicleAttachmentRelativePath(filePath))
        } catch as err {
            MsgBox("Spravovanou kopii se nepodařilo uložit.`n`n" err.Message, AppTitle, 0x30)
            return
        }
    } else if (attachmentMode = "managed") {
        filePath := NormalizeVehicleAttachmentRelativePath(filePath)
    }

    entry := NormalizeVehicleRecordEntry({
        id: (RecordFormMode = "edit") ? RecordFormEntryId : GenerateVehicleRecordId(),
        vehicleId: RecordFormVehicleId,
        recordType: recordType,
        title: title,
        provider: provider,
        validFrom: validFrom,
        validTo: validTo,
        price: price,
        attachmentMode: attachmentMode,
        filePath: filePath,
        note: note
    })

    index := FindVehicleRecordIndexById(entry.id)
    if index {
        VehicleRecords[index] := entry
    } else {
        VehicleRecords.Push(entry)
    }

    SaveVehicleRecords()
    PruneVehicleManagedAttachments(entry.vehicleId)
    CloseVehicleRecordForm()
    PopulateVehicleRecordsList(entry.id, true)
}
