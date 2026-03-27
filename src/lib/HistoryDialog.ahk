OpenVehicleHistoryDialog(vehicle, openAddEvent := false, selectEventId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, HistoryAllEntries, HistorySearchCtrl, VisibleHistoryEventIds, HistorySortColumn, HistorySortDescending, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui

    if IsObject(HistoryGui) {
        WinActivate("ahk_id " HistoryGui.Hwnd)
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

    if IsObject(RecordsGui) {
        WinActivate("ahk_id " RecordsGui.Hwnd)
        return
    }

    if IsObject(RecordFormGui) {
        WinActivate("ahk_id " RecordFormGui.Hwnd)
        return
    }

    ShowMainWindow()

    HistoryVehicleId := vehicle.id
    HistoryAllEntries := []
    HistorySearchCtrl := 0
    VisibleHistoryEventIds := []
    HistorySortColumn := GetHistorySortColumnSetting()
    HistorySortDescending := GetHistorySortDescendingSetting()
    HistoryGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Historie událostí")
    HistoryGui.SetFont("s10", "Segoe UI")
    HistoryGui.OnEvent("Close", CloseVehicleHistoryDialog)
    HistoryGui.OnEvent("Escape", CloseVehicleHistoryDialog)

    MainGui.Opt("+Disabled")

    HistoryGui.AddText("x20 y20 w780", "Zde můžete vést servisní a další události k vozidlu " vehicle.name ". Datum události se zadává jako DD.MM.RRRR.")
    HistorySummaryLabel := HistoryGui.AddText("x20 y50 w780", "")
    HistoryGui.AddText("x20 y82 w290", "Hledat datum, událost, km, cenu nebo poznámku")
    HistorySearchCtrl := HistoryGui.AddEdit("x320 y79 w350")
    HistorySearchCtrl.OnEvent("Change", OnHistorySearchChanged)

    HistoryList := HistoryGui.AddListView("x20 y112 w780 h250 Grid -Multi", ["Datum", "Událost", "Km", "Cena", "Poznámka"])
    HistoryList.OnEvent("DoubleClick", EditSelectedVehicleHistoryEvent)
    HistoryList.OnEvent("ColClick", OnHistoryColumnClick)
    HistoryList.ModifyCol(1, "95")
    HistoryList.ModifyCol(2, "190")
    HistoryList.ModifyCol(3, "95")
    HistoryList.ModifyCol(4, "100")
    HistoryList.ModifyCol(5, "280")

    addButton := HistoryGui.AddButton("x95 y377 w120 h30", "Přidat událost")
    addButton.OnEvent("Click", AddVehicleHistoryEvent)

    editButton := HistoryGui.AddButton("x225 y377 w120 h30", "Upravit událost")
    editButton.OnEvent("Click", EditSelectedVehicleHistoryEvent)

    deleteButton := HistoryGui.AddButton("x355 y377 w120 h30", "Odstranit událost")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleHistoryEvent)

    detailButton := HistoryGui.AddButton("x485 y377 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromHistory)

    closeButton := HistoryGui.AddButton("x615 y377 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleHistoryDialog)

    HistoryGui.Show("w820 h427")
    PopulateVehicleHistoryList(selectEventId, true)

    if openAddEvent {
        OpenVehicleHistoryEventForm("add")
    } else if (VisibleHistoryEventIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleHistoryDialog(*) {
    global HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, HistoryAllEntries, HistorySearchCtrl, VisibleHistoryEventIds, HistorySortColumn, HistorySortDescending, MainGui

    if IsObject(HistoryGui) {
        HistoryGui.Destroy()
        HistoryGui := 0
    }

    HistoryVehicleId := ""
    HistoryList := 0
    HistorySummaryLabel := 0
    HistoryAllEntries := []
    HistorySearchCtrl := 0
    VisibleHistoryEventIds := []
    HistorySortColumn := 1
    HistorySortDescending := true
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleHistoryList(selectEventId := "", focusList := false) {
    global HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, HistoryAllEntries, VisibleHistoryEventIds

    if !IsObject(HistoryGui) || !IsObject(HistoryList) {
        return
    }

    vehicle := FindVehicleById(HistoryVehicleId)
    HistoryAllEntries := GetVehicleHistoryEntries(HistoryVehicleId)
    events := FilterVehicleHistoryEntriesBySearch(HistoryAllEntries, GetHistorySearchText())
    SortVisibleVehicleHistoryEntries(&events)
    VisibleHistoryEventIds := []
    selectedRow := 0

    if IsObject(HistorySummaryLabel) && IsObject(vehicle) {
        HistorySummaryLabel.Text := BuildVehicleHistorySummaryText(vehicle.id)
    }

    HistoryList.Opt("-Redraw")
    HistoryList.Delete()
    for event in events {
        row := HistoryList.Add("", event.eventDate, event.eventType, FormatHistoryOdometer(event.odometer), event.cost, ShortenText(event.note, 80))
        VisibleHistoryEventIds.Push(event.id)
        if (selectEventId != "" && event.id = selectEventId) {
            selectedRow := row
        }
    }
    HistoryList.Opt("+Redraw")

    if (events.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    HistoryList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
}

OnHistorySearchChanged(*) {
    selectedEventId := ""
    event := GetSelectedVehicleHistoryEvent()
    if IsObject(event) {
        selectedEventId := event.id
    }

    PopulateVehicleHistoryList(selectedEventId)
}

OnHistoryColumnClick(ctrl, column) {
    global HistorySortColumn, HistorySortDescending

    if (HistorySortColumn = column) {
        HistorySortDescending := !HistorySortDescending
    } else {
        HistorySortColumn := column
        HistorySortDescending := (column = 1)
    }

    SaveHistorySortSettings(HistorySortColumn, HistorySortDescending)

    selectedEventId := ""
    event := GetSelectedVehicleHistoryEvent()
    if IsObject(event) {
        selectedEventId := event.id
    }

    PopulateVehicleHistoryList(selectedEventId, true)
}

GetHistorySearchText() {
    global HistorySearchCtrl

    if !IsObject(HistorySearchCtrl) {
        return ""
    }

    return Trim(HistorySearchCtrl.Text)
}

FilterVehicleHistoryEntriesBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        haystack := StrLower(entry.eventDate " " entry.eventType " " entry.odometer " " entry.cost " " entry.note)
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

SortVisibleVehicleHistoryEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleHistoryEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleHistoryEntries(left, right) {
    global HistorySortColumn, HistorySortDescending

    result := CompareVisibleVehicleHistoryEntriesByColumn(left, right, HistorySortColumn)
    if (result = 0 && HistorySortColumn != 1) {
        result := CompareVisibleVehicleHistoryEntriesByColumn(left, right, 1)
    }
    if (result = 0) {
        result := CompareTextValues(left.id, right.id)
    }

    return HistorySortDescending ? -result : result
}

CompareVisibleVehicleHistoryEntriesByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareOptionalStampValues(ParseEventDateStamp(left.eventDate), ParseEventDateStamp(right.eventDate))
        case 2:
            return CompareTextValues(left.eventType, right.eventType)
        case 3:
            return CompareOptionalIntegerTexts(left.odometer, right.odometer)
        case 4:
            return CompareOptionalMoneyTexts(left.cost, right.cost)
        case 5:
            return CompareTextValues(left.note, right.note)
    }

    return 0
}

GetSelectedVehicleHistoryEvent() {
    global HistoryList, VisibleHistoryEventIds

    if !IsObject(HistoryList) {
        return ""
    }

    row := HistoryList.GetNext(0)
    if !row || row > VisibleHistoryEventIds.Length {
        return ""
    }

    return FindVehicleHistoryEventById(VisibleHistoryEventIds[row])
}

AddVehicleHistoryEvent(*) {
    OpenVehicleHistoryEventForm("add")
}

EditSelectedVehicleHistoryEvent(*) {
    global AppTitle

    event := GetSelectedVehicleHistoryEvent()
    if !IsObject(event) {
        MsgBox("Nejprve vyberte událost, kterou chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleHistoryEventForm("edit", event)
}

DeleteSelectedVehicleHistoryEvent(*) {
    global AppTitle, VehicleHistory

    event := GetSelectedVehicleHistoryEvent()
    if !IsObject(event) {
        MsgBox("Nejprve vyberte událost, kterou chcete odstranit.", AppTitle, 0x40)
        return
    }

    vehicle := FindVehicleById(event.vehicleId)
    eventLabel := event.eventType " (" event.eventDate ")"
    if IsObject(vehicle) {
        eventLabel .= " u vozidla " vehicle.name
    }

    result := MsgBox("Opravdu chcete odstranit událost " eventLabel "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleHistoryEventIndexById(event.id)
    if !index {
        return
    }

    VehicleHistory.RemoveAt(index)
    SaveVehicleHistory()
    PopulateVehicleHistoryList()
}

OpenVehicleDetailFromHistory(*) {
    global HistoryVehicleId

    vehicle := FindVehicleById(HistoryVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleHistoryDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleHistoryEventForm(mode, event := "") {
    global AppTitle, HistoryGui, HistoryVehicleId, HistoryFormGui, HistoryFormControls, HistoryFormMode, HistoryFormEventId, HistoryFormVehicleId

    if IsObject(HistoryFormGui) {
        WinActivate("ahk_id " HistoryFormGui.Hwnd)
        return
    }

    if !IsObject(HistoryGui) {
        return
    }

    HistoryFormMode := mode
    HistoryFormEventId := IsObject(event) ? event.id : ""
    HistoryFormVehicleId := IsObject(event) ? event.vehicleId : HistoryVehicleId
    HistoryFormControls := {}

    title := (mode = "edit") ? "Upravit událost" : "Přidat událost"
    HistoryFormGui := Gui("+Owner" HistoryGui.Hwnd, AppTitle " - " title)
    HistoryFormGui.SetFont("s10", "Segoe UI")
    HistoryFormGui.OnEvent("Close", CloseVehicleHistoryEventForm)
    HistoryFormGui.OnEvent("Escape", CloseVehicleHistoryEventForm)

    HistoryGui.Opt("+Disabled")

    HistoryFormGui.AddText("x20 y20 w460", "Vyplňte datum a název události. Datum zadávejte jako DD.MM.RRRR, například 26.03.2026.")

    HistoryFormGui.AddText("x20 y60 w170", "Datum události (povinné)")
    HistoryFormControls.eventDate := HistoryFormGui.AddEdit("x210 y57 w220")

    HistoryFormGui.AddText("x20 y95 w170", "Název události (povinné)")
    HistoryFormControls.eventType := HistoryFormGui.AddEdit("x210 y92 w220")

    HistoryFormGui.AddText("x20 y130 w170", "Stav tachometru (volitelné)")
    HistoryFormControls.odometer := HistoryFormGui.AddEdit("x210 y127 w220")

    HistoryFormGui.AddText("x20 y165 w170", "Cena nebo částka (volitelné)")
    HistoryFormControls.cost := HistoryFormGui.AddEdit("x210 y162 w220")

    HistoryFormGui.AddText("x20 y200 w170", "Poznámka (volitelné)")
    HistoryFormControls.note := HistoryFormGui.AddEdit("x20 y225 w410 h95 Multi")

    saveButton := HistoryFormGui.AddButton("x170 y335 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleHistoryEventFromForm)

    cancelButton := HistoryFormGui.AddButton("x300 y335 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleHistoryEventForm)

    if IsObject(event) {
        HistoryFormControls.eventDate.Text := event.eventDate
        HistoryFormControls.eventType.Text := event.eventType
        HistoryFormControls.odometer.Text := event.odometer
        HistoryFormControls.cost.Text := event.cost
        HistoryFormControls.note.Text := event.note
    }

    HistoryFormGui.Show("w450 h380")
    HistoryFormControls.eventDate.Focus()
}

CloseVehicleHistoryEventForm(*) {
    global HistoryFormGui, HistoryFormControls, HistoryFormMode, HistoryFormEventId, HistoryFormVehicleId, HistoryGui

    if IsObject(HistoryFormGui) {
        HistoryFormGui.Destroy()
        HistoryFormGui := 0
    }

    HistoryFormControls := {}
    HistoryFormMode := ""
    HistoryFormEventId := ""
    HistoryFormVehicleId := ""

    if IsObject(HistoryGui) {
        HistoryGui.Opt("-Disabled")
        WinActivate("ahk_id " HistoryGui.Hwnd)
    }
}

SaveVehicleHistoryEventFromForm(*) {
    global AppTitle, VehicleHistory, HistoryFormControls, HistoryFormMode, HistoryFormEventId, HistoryFormVehicleId

    eventDate := NormalizeEventDate(HistoryFormControls.eventDate.Text)
    eventType := Trim(HistoryFormControls.eventType.Text)
    odometer := NormalizeOdometerText(HistoryFormControls.odometer.Text)
    cost := Trim(HistoryFormControls.cost.Text)
    note := Trim(HistoryFormControls.note.Text)

    if (eventDate = "") {
        MsgBox("Pole Datum události je povinné a musí být ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        HistoryFormControls.eventDate.Focus()
        return
    }

    if (eventType = "") {
        MsgBox("Vyplňte prosím název události.", AppTitle, 0x30)
        HistoryFormControls.eventType.Focus()
        return
    }

    if (Trim(HistoryFormControls.odometer.Text) != "" && odometer = "") {
        MsgBox("Stav tachometru zadejte jen jako celé číslo, nebo pole nechte prázdné.", AppTitle, 0x30)
        HistoryFormControls.odometer.Focus()
        return
    }

    event := {
        id: (HistoryFormMode = "edit") ? HistoryFormEventId : GenerateHistoryEventId(),
        vehicleId: HistoryFormVehicleId,
        eventDate: eventDate,
        eventType: eventType,
        odometer: odometer,
        cost: cost,
        note: note
    }

    index := FindVehicleHistoryEventIndexById(event.id)
    if index {
        VehicleHistory[index] := event
    } else {
        VehicleHistory.Push(event)
    }

    SaveVehicleHistory()
    CloseVehicleHistoryEventForm()
    PopulateVehicleHistoryList(event.id, true)
}
