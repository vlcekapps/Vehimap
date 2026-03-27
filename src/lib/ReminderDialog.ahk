OpenVehicleReminderDialog(vehicle, openAddEntry := false, selectEntryId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderVehicleId, ReminderList, ReminderSummaryLabel, ReminderAllEntries, ReminderSearchCtrl, VisibleReminderIds, ReminderSortColumn, ReminderSortDescending, ReminderFormGui, CostSummaryGui, FleetCostGui

    if IsObject(ReminderGui) {
        WinActivate("ahk_id " ReminderGui.Hwnd)
        return
    }

    for guiRef in [FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderFormGui, CostSummaryGui, FleetCostGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    ShowMainWindow()

    ReminderVehicleId := vehicle.id
    ReminderAllEntries := []
    ReminderSearchCtrl := 0
    VisibleReminderIds := []
    ReminderSortColumn := GetReminderSortColumnSetting()
    ReminderSortDescending := GetReminderSortDescendingSetting()
    ReminderGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Vlastní připomínky")
    ReminderGui.SetFont("s10", "Segoe UI")
    ReminderGui.OnEvent("Close", CloseVehicleReminderDialog)
    ReminderGui.OnEvent("Escape", CloseVehicleReminderDialog)

    MainGui.Opt("+Disabled")

    ReminderGui.AddText("x20 y20 w900", "Zde můžete evidovat vlastní termíny a připomínky pro vozidlo " vehicle.name ". Datum zadejte jako DD.MM.RRRR. U opakovaných připomínek můžete termín po vyřízení posunout tlačítkem na další cyklus.")
    ReminderSummaryLabel := ReminderGui.AddText("x20 y50 w900", "")
    ReminderGui.AddText("x20 y82 w310", "Hledat název, termín, opakování, stav nebo poznámku")
    ReminderSearchCtrl := ReminderGui.AddEdit("x340 y79 w360")
    ReminderSearchCtrl.OnEvent("Change", OnReminderSearchChanged)

    ReminderList := ReminderGui.AddListView("x20 y112 w900 h255 Grid -Multi", ["Název", "Termín", "Upozornit dnů předem", "Opakování", "Stav", "Poznámka"])
    ReminderList.OnEvent("DoubleClick", EditSelectedVehicleReminder)
    ReminderList.OnEvent("ColClick", OnReminderColumnClick)
    ReminderList.ModifyCol(1, "190")
    ReminderList.ModifyCol(2, "95")
    ReminderList.ModifyCol(3, "125")
    ReminderList.ModifyCol(4, "115")
    ReminderList.ModifyCol(5, "90")
    ReminderList.ModifyCol(6, "245")

    addButton := ReminderGui.AddButton("x70 y382 w120 h30", "Přidat záznam")
    addButton.OnEvent("Click", AddVehicleReminder)

    editButton := ReminderGui.AddButton("x200 y382 w120 h30", "Upravit záznam")
    editButton.OnEvent("Click", EditSelectedVehicleReminder)

    advanceButton := ReminderGui.AddButton("x330 y382 w160 h30", "Posunout na další")
    advanceButton.OnEvent("Click", AdvanceSelectedVehicleReminder)

    deleteButton := ReminderGui.AddButton("x500 y382 w130 h30", "Odstranit záznam")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleReminder)

    detailButton := ReminderGui.AddButton("x640 y382 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromReminder)

    closeButton := ReminderGui.AddButton("x770 y382 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleReminderDialog)

    ReminderGui.Show("w940 h432")
    PopulateVehicleReminderList(selectEntryId, true)

    if openAddEntry {
        OpenVehicleReminderForm("add")
    } else if (VisibleReminderIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleReminderDialog(*) {
    global ReminderGui, ReminderVehicleId, ReminderList, ReminderSummaryLabel, ReminderAllEntries, ReminderSearchCtrl, VisibleReminderIds, ReminderSortColumn, ReminderSortDescending, MainGui

    if IsObject(ReminderGui) {
        ReminderGui.Destroy()
        ReminderGui := 0
    }

    ReminderVehicleId := ""
    ReminderList := 0
    ReminderSummaryLabel := 0
    ReminderAllEntries := []
    ReminderSearchCtrl := 0
    VisibleReminderIds := []
    ReminderSortColumn := 2
    ReminderSortDescending := false
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleReminderList(selectEntryId := "", focusList := false) {
    global ReminderGui, ReminderVehicleId, ReminderList, ReminderSummaryLabel, ReminderAllEntries, VisibleReminderIds

    if !IsObject(ReminderGui) || !IsObject(ReminderList) {
        return
    }

    ReminderAllEntries := GetVehicleReminderEntries(ReminderVehicleId)
    entries := FilterVehicleReminderEntriesBySearch(ReminderAllEntries, GetReminderSearchText())
    SortVisibleVehicleReminderEntries(&entries)
    VisibleReminderIds := []
    selectedRow := 0

    if IsObject(ReminderSummaryLabel) {
        ReminderSummaryLabel.Text := BuildVehicleReminderSummaryText(ReminderVehicleId)
    }

    ReminderList.Opt("-Redraw")
    ReminderList.Delete()
    for entry in entries {
        row := ReminderList.Add(
            "",
            entry.title,
            entry.dueDate,
            entry.reminderDays,
            GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : ""),
            GetReminderExpirationStatusText(entry.dueDate, entry.reminderDays + 0),
            ShortenText(entry.note, 80)
        )
        VisibleReminderIds.Push(entry.id)
        if (selectEntryId != "" && entry.id = selectEntryId) {
            selectedRow := row
        }
    }
    ReminderList.Opt("+Redraw")

    if (entries.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    ReminderList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
}

OnReminderSearchChanged(*) {
    selectedEntryId := ""
    entry := GetSelectedVehicleReminder()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleReminderList(selectedEntryId)
}

OnReminderColumnClick(ctrl, column) {
    global ReminderSortColumn, ReminderSortDescending

    if (ReminderSortColumn = column) {
        ReminderSortDescending := !ReminderSortDescending
    } else {
        ReminderSortColumn := column
        ReminderSortDescending := false
    }

    SaveReminderSortSettings(ReminderSortColumn, ReminderSortDescending)

    selectedEntryId := ""
    entry := GetSelectedVehicleReminder()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleReminderList(selectedEntryId, true)
}

GetReminderSearchText() {
    global ReminderSearchCtrl

    if !IsObject(ReminderSearchCtrl) {
        return ""
    }

    return Trim(ReminderSearchCtrl.Text)
}

FilterVehicleReminderEntriesBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        repeatLabel := GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : "")
        statusText := GetReminderExpirationStatusText(entry.dueDate, entry.reminderDays + 0)
        haystack := StrLower(entry.title " " entry.dueDate " " entry.reminderDays " " repeatLabel " " statusText " " entry.note)
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

SortVisibleVehicleReminderEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleReminderEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleReminderEntries(left, right) {
    global ReminderSortColumn, ReminderSortDescending

    result := CompareVisibleVehicleReminderEntriesByColumn(left, right, ReminderSortColumn)
    if (result = 0 && ReminderSortColumn != 2) {
        result := CompareVisibleVehicleReminderEntriesByColumn(left, right, 2)
    }
    if (result = 0) {
        result := CompareTextValues(left.id, right.id)
    }

    return ReminderSortDescending ? -result : result
}

CompareVisibleVehicleReminderEntriesByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareTextValues(left.title, right.title)
        case 2:
            return CompareOptionalStampValues(ParseReminderDueStamp(left.dueDate), ParseReminderDueStamp(right.dueDate))
        case 3:
            return CompareOptionalIntegerTexts(left.reminderDays, right.reminderDays)
        case 4:
            return CompareTextValues(GetReminderRepeatLabel(left.HasOwnProp("repeatMode") ? left.repeatMode : ""), GetReminderRepeatLabel(right.HasOwnProp("repeatMode") ? right.repeatMode : ""))
        case 5:
            return CompareTextValues(GetReminderExpirationStatusText(left.dueDate, left.reminderDays + 0), GetReminderExpirationStatusText(right.dueDate, right.reminderDays + 0))
        case 6:
            return CompareTextValues(left.note, right.note)
    }

    return 0
}

GetSelectedVehicleReminder() {
    global ReminderList, VisibleReminderIds

    if !IsObject(ReminderList) {
        return ""
    }

    row := ReminderList.GetNext(0)
    if !row || row > VisibleReminderIds.Length {
        return ""
    }

    return FindVehicleReminderById(VisibleReminderIds[row])
}

AddVehicleReminder(*) {
    OpenVehicleReminderForm("add")
}

EditSelectedVehicleReminder(*) {
    global AppTitle

    entry := GetSelectedVehicleReminder()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte připomínku, kterou chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleReminderForm("edit", entry)
}

AdvanceSelectedVehicleReminder(*) {
    global AppTitle, VehicleReminders, ReminderVehicleId

    entry := GetSelectedVehicleReminder()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte připomínku, kterou chcete posunout na další termín.", AppTitle, 0x40)
        return
    }

    years := GetReminderRepeatYears(entry.HasOwnProp("repeatMode") ? entry.repeatMode : "")
    if (years < 1) {
        MsgBox("Vybraná připomínka není nastavena jako opakovaná. Opakování můžete nastavit v editaci připomínky.", AppTitle, 0x40)
        return
    }

    nextDueDate := AddYearsToEventDate(entry.dueDate, years)
    if (nextDueDate = "") {
        MsgBox("Nepodařilo se vypočítat další termín připomínky.", AppTitle, 0x30)
        return
    }

    index := FindVehicleReminderIndexById(entry.id)
    if !index {
        return
    }

    VehicleReminders[index].dueDate := nextDueDate
    SaveVehicleReminders()
    PopulateVehicleReminderList(entry.id, true)
    RefreshVehicleList(ReminderVehicleId)
    CheckDueVehicles(false, false)
}

DeleteSelectedVehicleReminder(*) {
    global AppTitle, VehicleReminders

    entry := GetSelectedVehicleReminder()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte připomínku, kterou chcete odstranit.", AppTitle, 0x40)
        return
    }

    result := MsgBox("Opravdu chcete odstranit připomínku " entry.title "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleReminderIndexById(entry.id)
    if !index {
        return
    }

    VehicleReminders.RemoveAt(index)
    SaveVehicleReminders()
    PopulateVehicleReminderList()
    RefreshVehicleList(ReminderVehicleId)
    CheckDueVehicles(false, false)
}

OpenVehicleDetailFromReminder(*) {
    global ReminderVehicleId

    vehicle := FindVehicleById(ReminderVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleReminderDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleReminderForm(mode, entry := "") {
    global AppTitle, ReminderGui, ReminderVehicleId, ReminderFormGui, ReminderFormControls, ReminderFormMode, ReminderFormEntryId, ReminderFormVehicleId, ReminderRepeatOptions

    if IsObject(ReminderFormGui) {
        WinActivate("ahk_id " ReminderFormGui.Hwnd)
        return
    }

    if !IsObject(ReminderGui) {
        return
    }

    ReminderFormMode := mode
    ReminderFormEntryId := IsObject(entry) ? entry.id : ""
    ReminderFormVehicleId := IsObject(entry) ? entry.vehicleId : ReminderVehicleId
    ReminderFormControls := {}

    title := (mode = "edit") ? "Upravit připomínku" : "Přidat připomínku"
    ReminderFormGui := Gui("+Owner" ReminderGui.Hwnd, AppTitle " - " title)
    ReminderFormGui.SetFont("s10", "Segoe UI")
    ReminderFormGui.OnEvent("Close", CloseVehicleReminderForm)
    ReminderFormGui.OnEvent("Escape", CloseVehicleReminderForm)

    ReminderGui.Opt("+Disabled")

    ReminderFormGui.AddText("x20 y20 w520", "Název, termín a počet dnů předem jsou povinné. Datum zadávejte jako DD.MM.RRRR. Opakování je volitelné.")

    ReminderFormGui.AddText("x20 y60 w180", "Název připomínky (povinné)")
    ReminderFormControls.title := ReminderFormGui.AddEdit("x220 y57 w240")

    ReminderFormGui.AddText("x20 y95 w180", "Termín (povinné)")
    ReminderFormControls.dueDate := ReminderFormGui.AddEdit("x220 y92 w140")

    ReminderFormGui.AddText("x20 y130 w180", "Upozornit dnů předem (povinné)")
    ReminderFormControls.reminderDays := ReminderFormGui.AddEdit("x220 y127 w140 Limit3 Number")

    ReminderFormGui.AddText("x20 y165 w180", "Opakování (volitelné)")
    ReminderFormControls.repeatMode := ReminderFormGui.AddDropDownList("x220 y162 w180", ReminderRepeatOptions)

    ReminderFormGui.AddText("x20 y200 w180", "Poznámka (volitelné)")
    ReminderFormControls.note := ReminderFormGui.AddEdit("x20 y225 w440 h95 Multi")

    saveButton := ReminderFormGui.AddButton("x150 y335 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleReminderFromForm)

    cancelButton := ReminderFormGui.AddButton("x280 y335 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleReminderForm)

    if IsObject(entry) {
        ReminderFormControls.title.Text := entry.title
        ReminderFormControls.dueDate.Text := entry.dueDate
        ReminderFormControls.reminderDays.Text := entry.reminderDays
        ReminderFormControls.note.Text := entry.note
        SetDropDownToText(ReminderFormControls.repeatMode, GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : ""), ReminderRepeatOptions)
    } else {
        ReminderFormControls.reminderDays.Text := "30"
        ReminderFormControls.repeatMode.Value := 1
    }

    ReminderFormGui.Show("w480 h385")
    ReminderFormControls.title.Focus()
}

CloseVehicleReminderForm(*) {
    global ReminderFormGui, ReminderFormControls, ReminderFormMode, ReminderFormEntryId, ReminderFormVehicleId, ReminderGui

    if IsObject(ReminderFormGui) {
        ReminderFormGui.Destroy()
        ReminderFormGui := 0
    }

    ReminderFormControls := {}
    ReminderFormMode := ""
    ReminderFormEntryId := ""
    ReminderFormVehicleId := ""

    if IsObject(ReminderGui) {
        ReminderGui.Opt("-Disabled")
        WinActivate("ahk_id " ReminderGui.Hwnd)
    }
}

SaveVehicleReminderFromForm(*) {
    global AppTitle, VehicleReminders, ReminderFormControls, ReminderFormMode, ReminderFormEntryId, ReminderFormVehicleId, ReminderVehicleId

    title := Trim(ReminderFormControls.title.Text)
    dueDate := NormalizeEventDate(ReminderFormControls.dueDate.Text)
    reminderDaysText := Trim(ReminderFormControls.reminderDays.Text)
    repeatMode := NormalizeReminderRepeat(ReminderFormControls.repeatMode.Text)
    note := Trim(ReminderFormControls.note.Text)

    if (title = "") {
        MsgBox("Vyplňte prosím název připomínky.", AppTitle, 0x30)
        ReminderFormControls.title.Focus()
        return
    }

    if (dueDate = "") {
        MsgBox("Pole Termín je povinné a musí být ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        ReminderFormControls.dueDate.Focus()
        return
    }

    if !RegExMatch(reminderDaysText, "^\d{1,3}$") {
        MsgBox("Pole Upozornit dnů předem musí být celé číslo od 0 do 999.", AppTitle, 0x30)
        ReminderFormControls.reminderDays.Focus()
        return
    }

    reminderDays := reminderDaysText + 0
    entry := {
        id: (ReminderFormMode = "edit") ? ReminderFormEntryId : GenerateVehicleReminderId(),
        vehicleId: ReminderFormVehicleId,
        title: title,
        dueDate: dueDate,
        reminderDays: reminderDays,
        repeatMode: repeatMode,
        note: note
    }

    index := FindVehicleReminderIndexById(entry.id)
    if index {
        VehicleReminders[index] := entry
    } else {
        VehicleReminders.Push(entry)
    }

    SaveVehicleReminders()
    CloseVehicleReminderForm()
    PopulateVehicleReminderList(entry.id, true)
    RefreshVehicleList(ReminderVehicleId)
    CheckDueVehicles(false, false)
}
