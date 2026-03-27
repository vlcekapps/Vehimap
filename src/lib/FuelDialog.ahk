OpenVehicleFuelDialog(vehicle, openAddEntry := false, selectEntryId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelVehicleId, FuelList, FuelSummaryLabel, FuelAllEntries, FuelSearchCtrl, VisibleFuelEntryIds, FuelSortColumn, FuelSortDescending, FuelFormGui, RecordsGui, RecordFormGui

    if IsObject(FuelGui) {
        WinActivate("ahk_id " FuelGui.Hwnd)
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

    if IsObject(FuelFormGui) {
        WinActivate("ahk_id " FuelFormGui.Hwnd)
        return
    }

    if IsObject(RecordsGui) {
        WinActivate("ahk_id " RecordsGui.Hwnd)
        return
    }

    if IsObject(RecordFormGui) {
        WinActivate("ahk_id " RecordFormGui.Hwnd)
        return
    }

    ShowMainWindow()

    FuelVehicleId := vehicle.id
    FuelAllEntries := []
    FuelSearchCtrl := 0
    VisibleFuelEntryIds := []
    FuelSortColumn := GetFuelSortColumnSetting()
    FuelSortDescending := GetFuelSortDescendingSetting()
    FuelGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Kilometry a tankování")
    FuelGui.SetFont("s10", "Segoe UI")
    FuelGui.OnEvent("Close", CloseVehicleFuelDialog)
    FuelGui.OnEvent("Escape", CloseVehicleFuelDialog)

    MainGui.Opt("+Disabled")

    FuelGui.AddText("x20 y20 w820", "Zde můžete evidovat stavy tachometru, tankování a orientační spotřebu vozidla " vehicle.name ". Datum zadejte jako DD.MM.RRRR.")
    FuelSummaryLabel := FuelGui.AddText("x20 y50 w820", "")
    FuelGui.AddText("x20 y82 w320", "Hledat datum, tachometr, litry, cenu, palivo nebo poznámku")
    FuelSearchCtrl := FuelGui.AddEdit("x350 y79 w350")
    FuelSearchCtrl.OnEvent("Change", OnFuelSearchChanged)

    FuelList := FuelGui.AddListView("x20 y112 w820 h255 Grid -Multi", ["Datum", "Tachometr", "Litry", "Cena", "Plná nádrž", "Palivo", "Poznámka"])
    FuelList.OnEvent("DoubleClick", EditSelectedVehicleFuelEntry)
    FuelList.OnEvent("ColClick", OnFuelColumnClick)
    FuelList.ModifyCol(1, "95")
    FuelList.ModifyCol(2, "100")
    FuelList.ModifyCol(3, "70")
    FuelList.ModifyCol(4, "95")
    FuelList.ModifyCol(5, "75")
    FuelList.ModifyCol(6, "95")
    FuelList.ModifyCol(7, "240")

    addButton := FuelGui.AddButton("x80 y382 w120 h30", "Přidat záznam")
    addButton.OnEvent("Click", AddVehicleFuelEntry)

    editButton := FuelGui.AddButton("x210 y382 w120 h30", "Upravit záznam")
    editButton.OnEvent("Click", EditSelectedVehicleFuelEntry)

    deleteButton := FuelGui.AddButton("x340 y382 w130 h30", "Odstranit záznam")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleFuelEntry)

    detailButton := FuelGui.AddButton("x480 y382 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromFuel)

    closeButton := FuelGui.AddButton("x610 y382 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleFuelDialog)

    FuelGui.Show("w860 h432")
    PopulateVehicleFuelList(selectEntryId, true)

    if openAddEntry {
        OpenVehicleFuelEntryForm("add")
    } else if (VisibleFuelEntryIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleFuelDialog(*) {
    global FuelGui, FuelVehicleId, FuelList, FuelSummaryLabel, FuelAllEntries, FuelSearchCtrl, VisibleFuelEntryIds, FuelSortColumn, FuelSortDescending, MainGui

    if IsObject(FuelGui) {
        FuelGui.Destroy()
        FuelGui := 0
    }

    FuelVehicleId := ""
    FuelList := 0
    FuelSummaryLabel := 0
    FuelAllEntries := []
    FuelSearchCtrl := 0
    VisibleFuelEntryIds := []
    FuelSortColumn := 1
    FuelSortDescending := true
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleFuelList(selectEntryId := "", focusList := false) {
    global FuelGui, FuelVehicleId, FuelList, FuelSummaryLabel, FuelAllEntries, VisibleFuelEntryIds

    if !IsObject(FuelGui) || !IsObject(FuelList) {
        return
    }

    FuelAllEntries := GetVehicleFuelEntries(FuelVehicleId)
    entries := FilterVehicleFuelEntriesBySearch(FuelAllEntries, GetFuelSearchText())
    SortVisibleVehicleFuelEntries(&entries)
    VisibleFuelEntryIds := []
    selectedRow := 0

    if IsObject(FuelSummaryLabel) {
        FuelSummaryLabel.Text := BuildVehicleFuelSummaryText(FuelVehicleId)
    }

    FuelList.Opt("-Redraw")
    FuelList.Delete()
    for entry in entries {
        row := FuelList.Add(
            "",
            entry.entryDate,
            FormatHistoryOdometer(entry.odometer),
            FormatFuelLiters(entry.liters),
            FormatFuelMoney(entry.totalCost),
            entry.fullTank ? "Ano" : "Ne",
            entry.fuelType,
            ShortenText(entry.note, 80)
        )
        VisibleFuelEntryIds.Push(entry.id)
        if (selectEntryId != "" && entry.id = selectEntryId) {
            selectedRow := row
        }
    }
    FuelList.Opt("+Redraw")

    if (entries.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    FuelList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
}

OnFuelSearchChanged(*) {
    selectedEntryId := ""
    entry := GetSelectedVehicleFuelEntry()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleFuelList(selectedEntryId)
}

OnFuelColumnClick(ctrl, column) {
    global FuelSortColumn, FuelSortDescending

    if (FuelSortColumn = column) {
        FuelSortDescending := !FuelSortDescending
    } else {
        FuelSortColumn := column
        FuelSortDescending := (column = 1 || column = 2)
    }

    SaveFuelSortSettings(FuelSortColumn, FuelSortDescending)

    selectedEntryId := ""
    entry := GetSelectedVehicleFuelEntry()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleFuelList(selectedEntryId, true)
}

GetFuelSearchText() {
    global FuelSearchCtrl

    if !IsObject(FuelSearchCtrl) {
        return ""
    }

    return Trim(FuelSearchCtrl.Text)
}

FilterVehicleFuelEntriesBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        fullTankText := entry.fullTank ? "ano" : "ne"
        haystack := StrLower(entry.entryDate " " entry.odometer " " entry.liters " " entry.totalCost " " fullTankText " " entry.fuelType " " entry.note)
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

SortVisibleVehicleFuelEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleFuelEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleFuelEntries(left, right) {
    global FuelSortColumn, FuelSortDescending

    result := CompareVisibleVehicleFuelEntriesByColumn(left, right, FuelSortColumn)
    if (result = 0 && FuelSortColumn != 1) {
        result := CompareVisibleVehicleFuelEntriesByColumn(left, right, 1)
    }
    if (result = 0) {
        result := CompareTextValues(left.id, right.id)
    }

    return FuelSortDescending ? -result : result
}

CompareVisibleVehicleFuelEntriesByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareOptionalStampValues(ParseEventDateStamp(left.entryDate), ParseEventDateStamp(right.entryDate))
        case 2:
            return CompareOptionalIntegerTexts(left.odometer, right.odometer)
        case 3:
            return CompareOptionalDecimalTexts(left.liters, right.liters)
        case 4:
            return CompareOptionalMoneyTexts(left.totalCost, right.totalCost)
        case 5:
            return CompareNumberValues(left.fullTank ? 1 : 0, right.fullTank ? 1 : 0)
        case 6:
            return CompareTextValues(left.fuelType, right.fuelType)
        case 7:
            return CompareTextValues(left.note, right.note)
    }

    return 0
}

GetSelectedVehicleFuelEntry() {
    global FuelList, VisibleFuelEntryIds

    if !IsObject(FuelList) {
        return ""
    }

    row := FuelList.GetNext(0)
    if !row || row > VisibleFuelEntryIds.Length {
        return ""
    }

    return FindVehicleFuelEntryById(VisibleFuelEntryIds[row])
}

AddVehicleFuelEntry(*) {
    OpenVehicleFuelEntryForm("add")
}

EditSelectedVehicleFuelEntry(*) {
    global AppTitle

    entry := GetSelectedVehicleFuelEntry()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, který chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleFuelEntryForm("edit", entry)
}

DeleteSelectedVehicleFuelEntry(*) {
    global AppTitle, VehicleFuelLog

    entry := GetSelectedVehicleFuelEntry()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, který chcete odstranit.", AppTitle, 0x40)
        return
    }

    result := MsgBox("Opravdu chcete odstranit záznam z " entry.entryDate "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleFuelEntryIndexById(entry.id)
    if !index {
        return
    }

    VehicleFuelLog.RemoveAt(index)
    SaveVehicleFuelLog()
    PopulateVehicleFuelList()
}

OpenVehicleDetailFromFuel(*) {
    global FuelVehicleId

    vehicle := FindVehicleById(FuelVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleFuelDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleFuelEntryForm(mode, entry := "") {
    global AppTitle, FuelGui, FuelVehicleId, FuelFormGui, FuelFormControls, FuelFormMode, FuelFormEntryId, FuelFormVehicleId, FuelTypeOptions

    if IsObject(FuelFormGui) {
        WinActivate("ahk_id " FuelFormGui.Hwnd)
        return
    }

    if !IsObject(FuelGui) {
        return
    }

    FuelFormMode := mode
    FuelFormEntryId := IsObject(entry) ? entry.id : ""
    FuelFormVehicleId := IsObject(entry) ? entry.vehicleId : FuelVehicleId
    FuelFormControls := {}

    title := (mode = "edit") ? "Upravit záznam" : "Přidat záznam"
    FuelFormGui := Gui("+Owner" FuelGui.Hwnd, AppTitle " - " title)
    FuelFormGui.SetFont("s10", "Segoe UI")
    FuelFormGui.OnEvent("Close", CloseVehicleFuelEntryForm)
    FuelFormGui.OnEvent("Escape", CloseVehicleFuelEntryForm)

    FuelGui.Opt("+Disabled")

    FuelFormGui.AddText("x20 y20 w500", "Datum a stav tachometru jsou povinné. Litry a cena vyplňte, pokud jde o tankování.")

    FuelFormGui.AddText("x20 y60 w180", "Datum záznamu (povinné)")
    FuelFormControls.entryDate := FuelFormGui.AddEdit("x230 y57 w220")

    FuelFormGui.AddText("x20 y95 w180", "Stav tachometru (povinné)")
    FuelFormControls.odometer := FuelFormGui.AddEdit("x230 y92 w220")

    FuelFormGui.AddText("x20 y130 w180", "Natankováno litrů (volitelné)")
    FuelFormControls.liters := FuelFormGui.AddEdit("x230 y127 w220")

    FuelFormGui.AddText("x20 y165 w180", "Cena celkem v Kč (volitelné)")
    FuelFormControls.totalCost := FuelFormGui.AddEdit("x230 y162 w220")

    FuelFormGui.AddText("x20 y200 w180", "Typ paliva (volitelné)")
    FuelFormControls.fuelType := FuelFormGui.AddDropDownList("x230 y197 w220", FuelTypeOptions)

    FuelFormControls.fullTank := FuelFormGui.AddCheckBox("x230 y232 w220", "Plná nádrž")
    FuelFormControls.fullTank.Value := 1

    FuelFormGui.AddText("x20 y265 w180", "Poznámka (volitelné)")
    FuelFormControls.note := FuelFormGui.AddEdit("x20 y290 w430 h80 Multi")

    saveButton := FuelFormGui.AddButton("x150 y385 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleFuelEntryFromForm)

    cancelButton := FuelFormGui.AddButton("x280 y385 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleFuelEntryForm)

    if IsObject(entry) {
        FuelFormControls.entryDate.Text := entry.entryDate
        FuelFormControls.odometer.Text := entry.odometer
        FuelFormControls.liters.Text := entry.liters
        FuelFormControls.totalCost.Text := entry.totalCost
        SetDropDownToText(FuelFormControls.fuelType, entry.fuelType, FuelTypeOptions)
        FuelFormControls.fullTank.Value := entry.fullTank ? 1 : 0
        FuelFormControls.note.Text := entry.note
    }

    FuelFormGui.Show("w470 h430")
    FuelFormControls.entryDate.Focus()
}

CloseVehicleFuelEntryForm(*) {
    global FuelFormGui, FuelFormControls, FuelFormMode, FuelFormEntryId, FuelFormVehicleId, FuelGui

    if IsObject(FuelFormGui) {
        FuelFormGui.Destroy()
        FuelFormGui := 0
    }

    FuelFormControls := {}
    FuelFormMode := ""
    FuelFormEntryId := ""
    FuelFormVehicleId := ""

    if IsObject(FuelGui) {
        FuelGui.Opt("-Disabled")
        WinActivate("ahk_id " FuelGui.Hwnd)
    }
}

SaveVehicleFuelEntryFromForm(*) {
    global AppTitle, VehicleFuelLog, FuelFormControls, FuelFormMode, FuelFormEntryId, FuelFormVehicleId

    entryDate := NormalizeEventDate(FuelFormControls.entryDate.Text)
    odometer := NormalizeOdometerText(FuelFormControls.odometer.Text)
    liters := NormalizeDecimalText(FuelFormControls.liters.Text)
    totalCost := NormalizeDecimalText(FuelFormControls.totalCost.Text)
    fuelType := Trim(FuelFormControls.fuelType.Text)
    fullTank := FuelFormControls.fullTank.Value ? 1 : 0
    note := Trim(FuelFormControls.note.Text)

    if (entryDate = "") {
        MsgBox("Pole Datum záznamu je povinné a musí být ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        FuelFormControls.entryDate.Focus()
        return
    }

    if (odometer = "") {
        MsgBox("Pole Stav tachometru je povinné a musí obsahovat celé číslo.", AppTitle, 0x30)
        FuelFormControls.odometer.Focus()
        return
    }

    if (Trim(FuelFormControls.liters.Text) != "" && liters = "") {
        MsgBox("Natankované litry zadejte jako číslo, například 42,5.", AppTitle, 0x30)
        FuelFormControls.liters.Focus()
        return
    }

    if (Trim(FuelFormControls.totalCost.Text) != "" && totalCost = "") {
        MsgBox("Cenu celkem zadejte jako číslo v Kč, například 1890.", AppTitle, 0x30)
        FuelFormControls.totalCost.Focus()
        return
    }

    if (totalCost != "" && liters = "") {
        MsgBox("Pokud zadáváte cenu tankování, doplňte i počet litrů.", AppTitle, 0x30)
        FuelFormControls.liters.Focus()
        return
    }

    entry := {
        id: (FuelFormMode = "edit") ? FuelFormEntryId : GenerateFuelEntryId(),
        vehicleId: FuelFormVehicleId,
        entryDate: entryDate,
        odometer: odometer,
        liters: liters,
        totalCost: totalCost,
        fullTank: fullTank,
        fuelType: fuelType,
        note: note
    }

    index := FindVehicleFuelEntryIndexById(entry.id)
    if index {
        VehicleFuelLog[index] := entry
    } else {
        VehicleFuelLog.Push(entry)
    }

    SaveVehicleFuelLog()
    CloseVehicleFuelEntryForm()
    PopulateVehicleFuelList(entry.id, true)
}
