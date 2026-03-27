OpenVehicleMaintenanceDialog(vehicle, openAddPlan := false, selectPlanId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui, FleetCostGui, DashboardGui, GlobalSearchGui
    global MaintenanceGui, MaintenanceVehicleId, MaintenanceList, MaintenanceSummaryLabel, MaintenanceAllPlans, MaintenanceSearchCtrl, VisibleMaintenancePlanIds, MaintenanceSortColumn, MaintenanceSortDescending, MaintenanceCompleteButton

    if IsObject(MaintenanceGui) {
        WinActivate("ahk_id " MaintenanceGui.Hwnd)
        return
    }

    for guiRef in [DashboardGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui, FleetCostGui, GlobalSearchGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    ShowMainWindow()

    MaintenanceVehicleId := vehicle.id
    MaintenanceAllPlans := []
    MaintenanceSearchCtrl := 0
    VisibleMaintenancePlanIds := []
    MaintenanceSortColumn := GetMaintenanceSortColumnSetting()
    MaintenanceSortDescending := GetMaintenanceSortDescendingSetting()
    MaintenanceGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Plán údržby")
    MaintenanceGui.SetFont("s10", "Segoe UI")
    MaintenanceGui.OnEvent("Close", CloseVehicleMaintenanceDialog)
    MaintenanceGui.OnEvent("Escape", CloseVehicleMaintenanceDialog)

    MainGui.Opt("+Disabled")

    MaintenanceGui.AddText("x20 y20 w860", "Zde můžete plánovat pravidelnou údržbu vozidla " vehicle.name ". Každý úkon může hlídat interval podle data, tachometru nebo obojího zároveň.")
    MaintenanceSummaryLabel := MaintenanceGui.AddText("x20 y50 w860", "")
    MaintenanceGui.AddText("x20 y82 w290", "Hledat úkon, interval, stav nebo poznámku")
    MaintenanceSearchCtrl := MaintenanceGui.AddEdit("x320 y79 w360")
    MaintenanceSearchCtrl.OnEvent("Change", OnMaintenanceSearchChanged)

    MaintenanceList := MaintenanceGui.AddListView("x20 y112 w860 h255 Grid -Multi", ["Úkon", "Interval", "Poslední servis", "Další servis", "Stav", "Poznámka"])
    MaintenanceList.OnEvent("DoubleClick", EditSelectedVehicleMaintenancePlan)
    MaintenanceList.OnEvent("ColClick", OnMaintenanceColumnClick)
    MaintenanceList.OnEvent("ItemSelect", OnMaintenanceSelectionChanged)
    MaintenanceList.ModifyCol(1, "210")
    MaintenanceList.ModifyCol(2, "120")
    MaintenanceList.ModifyCol(3, "150")
    MaintenanceList.ModifyCol(4, "150")
    MaintenanceList.ModifyCol(5, "150")
    MaintenanceList.ModifyCol(6, "230")

    addButton := MaintenanceGui.AddButton("x50 y382 w120 h30", "Přidat úkon")
    addButton.OnEvent("Click", AddVehicleMaintenancePlan)

    editButton := MaintenanceGui.AddButton("x180 y382 w120 h30", "Upravit úkon")
    editButton.OnEvent("Click", EditSelectedVehicleMaintenancePlan)

    MaintenanceCompleteButton := MaintenanceGui.AddButton("x310 y382 w140 h30", "Označit splněno")
    MaintenanceCompleteButton.OnEvent("Click", CompleteSelectedVehicleMaintenancePlan)

    deleteButton := MaintenanceGui.AddButton("x460 y382 w130 h30", "Odstranit úkon")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleMaintenancePlan)

    detailButton := MaintenanceGui.AddButton("x600 y382 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromMaintenance)

    closeButton := MaintenanceGui.AddButton("x730 y382 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleMaintenanceDialog)

    MaintenanceGui.Show("w900 h432")
    PopulateVehicleMaintenanceList(selectPlanId, true)

    if openAddPlan {
        OpenVehicleMaintenancePlanForm("add")
    } else if (VisibleMaintenancePlanIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleMaintenanceDialog(*) {
    global MaintenanceGui, MaintenanceVehicleId, MaintenanceList, MaintenanceSummaryLabel, MaintenanceAllPlans, MaintenanceSearchCtrl, VisibleMaintenancePlanIds, MaintenanceSortColumn, MaintenanceSortDescending, MaintenanceCompleteButton, MainGui

    if IsObject(MaintenanceGui) {
        MaintenanceGui.Destroy()
        MaintenanceGui := 0
    }

    MaintenanceVehicleId := ""
    MaintenanceList := 0
    MaintenanceSummaryLabel := 0
    MaintenanceAllPlans := []
    MaintenanceSearchCtrl := 0
    VisibleMaintenancePlanIds := []
    MaintenanceSortColumn := 5
    MaintenanceSortDescending := false
    MaintenanceCompleteButton := 0
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleMaintenanceList(selectPlanId := "", focusList := false) {
    global MaintenanceGui, MaintenanceVehicleId, MaintenanceList, MaintenanceSummaryLabel, MaintenanceAllPlans, VisibleMaintenancePlanIds

    if !IsObject(MaintenanceGui) || !IsObject(MaintenanceList) {
        return
    }

    MaintenanceAllPlans := BuildVehicleMaintenanceSnapshots(MaintenanceVehicleId, true)
    snapshots := FilterVehicleMaintenanceSnapshotsBySearch(MaintenanceAllPlans, GetMaintenanceSearchText())
    SortVisibleVehicleMaintenanceSnapshots(&snapshots)
    VisibleMaintenancePlanIds := []
    selectedRow := 0

    if IsObject(MaintenanceSummaryLabel) {
        MaintenanceSummaryLabel.Text := BuildVehicleMaintenanceSummaryText(MaintenanceVehicleId)
    }

    MaintenanceList.Opt("-Redraw")
    MaintenanceList.Delete()
    for snapshot in snapshots {
        row := MaintenanceList.Add("", snapshot.title, snapshot.intervalText, snapshot.lastServiceText, snapshot.nextServiceText, snapshot.statusText, ShortenText(snapshot.plan.note, 80))
        VisibleMaintenancePlanIds.Push(snapshot.plan.id)
        if (selectPlanId != "" && snapshot.plan.id = selectPlanId) {
            selectedRow := row
        }
    }
    MaintenanceList.Opt("+Redraw")

    UpdateVehicleMaintenanceActionState()

    if (snapshots.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    MaintenanceList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
    UpdateVehicleMaintenanceActionState()
}

OnMaintenanceSearchChanged(*) {
    selectedPlanId := ""
    plan := GetSelectedVehicleMaintenancePlan()
    if IsObject(plan) {
        selectedPlanId := plan.id
    }

    PopulateVehicleMaintenanceList(selectedPlanId)
}

OnMaintenanceSelectionChanged(*) {
    UpdateVehicleMaintenanceActionState()
}

OnMaintenanceColumnClick(ctrl, column) {
    global MaintenanceSortColumn, MaintenanceSortDescending

    if (MaintenanceSortColumn = column) {
        MaintenanceSortDescending := !MaintenanceSortDescending
    } else {
        MaintenanceSortColumn := column
        MaintenanceSortDescending := (column = 5)
    }

    SaveMaintenanceSortSettings(MaintenanceSortColumn, MaintenanceSortDescending)

    selectedPlanId := ""
    plan := GetSelectedVehicleMaintenancePlan()
    if IsObject(plan) {
        selectedPlanId := plan.id
    }

    PopulateVehicleMaintenanceList(selectedPlanId, true)
}

GetMaintenanceSearchText() {
    global MaintenanceSearchCtrl

    if !IsObject(MaintenanceSearchCtrl) {
        return ""
    }

    return Trim(MaintenanceSearchCtrl.Text)
}

FilterVehicleMaintenanceSnapshotsBySearch(snapshots, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for snapshot in snapshots {
        haystack := StrLower(
            snapshot.title " "
            snapshot.intervalText " "
            snapshot.lastServiceText " "
            snapshot.nextServiceText " "
            snapshot.statusText " "
            snapshot.plan.note
        )
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(snapshot)
        }
    }

    return filtered
}

SortVisibleVehicleMaintenanceSnapshots(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleMaintenanceSnapshots(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleMaintenanceSnapshots(left, right) {
    global MaintenanceSortColumn, MaintenanceSortDescending

    result := CompareVisibleVehicleMaintenanceSnapshotsByColumn(left, right, MaintenanceSortColumn)
    if (result = 0 && MaintenanceSortColumn != 5) {
        result := CompareVisibleVehicleMaintenanceSnapshotsByColumn(left, right, 5)
    }
    if (result = 0) {
        result := CompareTextValues(left.plan.id, right.plan.id)
    }

    return MaintenanceSortDescending ? -result : result
}

CompareVisibleVehicleMaintenanceSnapshotsByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareTextValues(left.title, right.title)
        case 2:
            return CompareTextValues(left.intervalText, right.intervalText)
        case 3:
            return CompareTextValues(left.lastServiceText, right.lastServiceText)
        case 4:
            return CompareTextValues(left.overviewSortKey, right.overviewSortKey)
        case 5:
            result := CompareNumberValues(GetVehicleMaintenanceStatusSortValue(left), GetVehicleMaintenanceStatusSortValue(right))
            if (result != 0) {
                return result
            }
            return CompareTextValues(left.overviewSortKey, right.overviewSortKey)
        case 6:
            return CompareTextValues(left.plan.note, right.plan.note)
    }

    return 0
}

UpdateVehicleMaintenanceActionState() {
    global MaintenanceList, VisibleMaintenancePlanIds, MaintenanceCompleteButton

    hasSelection := false
    if IsObject(MaintenanceList) {
        row := MaintenanceList.GetNext(0)
        hasSelection := (row > 0 && row <= VisibleMaintenancePlanIds.Length)
    }

    if IsObject(MaintenanceCompleteButton) {
        MaintenanceCompleteButton.Opt(hasSelection ? "-Disabled" : "+Disabled")
    }
}

GetSelectedVehicleMaintenancePlan() {
    global MaintenanceList, VisibleMaintenancePlanIds

    if !IsObject(MaintenanceList) {
        return ""
    }

    row := MaintenanceList.GetNext(0)
    if !row || row > VisibleMaintenancePlanIds.Length {
        return ""
    }

    return FindVehicleMaintenancePlanById(VisibleMaintenancePlanIds[row])
}

GetSelectedVehicleMaintenanceSnapshot() {
    plan := GetSelectedVehicleMaintenancePlan()
    if !IsObject(plan) {
        return ""
    }

    return BuildVehicleMaintenancePlanSnapshot(plan)
}

AddVehicleMaintenancePlan(*) {
    OpenVehicleMaintenancePlanForm("add")
}

EditSelectedVehicleMaintenancePlan(*) {
    global AppTitle

    plan := GetSelectedVehicleMaintenancePlan()
    if !IsObject(plan) {
        MsgBox("Nejprve vyberte úkon, který chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleMaintenancePlanForm("edit", plan)
}

DeleteSelectedVehicleMaintenancePlan(*) {
    global AppTitle, VehicleMaintenancePlans

    plan := GetSelectedVehicleMaintenancePlan()
    if !IsObject(plan) {
        MsgBox("Nejprve vyberte úkon, který chcete odstranit.", AppTitle, 0x40)
        return
    }

    result := MsgBox("Opravdu chcete odstranit plán údržby " plan.title "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleMaintenancePlanIndexById(plan.id)
    if !index {
        return
    }

    VehicleMaintenancePlans.RemoveAt(index)
    SaveVehicleMaintenancePlans()
    RefreshMaintenanceDependentState(plan.vehicleId)
    PopulateVehicleMaintenanceList()
}

CompleteSelectedVehicleMaintenancePlan(*) {
    global AppTitle

    plan := GetSelectedVehicleMaintenancePlan()
    if !IsObject(plan) {
        MsgBox("Nejprve vyberte úkon, který chcete označit jako splněný.", AppTitle, 0x40)
        return
    }

    if !plan.isActive {
        MsgBox("Pozastavený plán nejdříve znovu aktivujte v úpravě úkonu.", AppTitle, 0x40)
        return
    }

    OpenVehicleMaintenanceCompleteForm(plan)
}

OpenVehicleDetailFromMaintenance(*) {
    global MaintenanceVehicleId

    vehicle := FindVehicleById(MaintenanceVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleMaintenanceDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleMaintenancePlanForm(mode, plan := "") {
    global AppTitle, MaintenanceGui, MaintenanceVehicleId, MaintenanceFormGui, MaintenanceFormControls, MaintenanceFormMode, MaintenanceFormPlanId, MaintenanceFormVehicleId

    if IsObject(MaintenanceFormGui) {
        WinActivate("ahk_id " MaintenanceFormGui.Hwnd)
        return
    }

    if !IsObject(MaintenanceGui) {
        return
    }

    MaintenanceFormMode := mode
    MaintenanceFormPlanId := IsObject(plan) ? plan.id : ""
    MaintenanceFormVehicleId := IsObject(plan) ? plan.vehicleId : MaintenanceVehicleId
    MaintenanceFormControls := {}

    title := (mode = "edit") ? "Upravit plán údržby" : "Přidat plán údržby"
    MaintenanceFormGui := Gui("+Owner" MaintenanceGui.Hwnd, AppTitle " - " title)
    MaintenanceFormGui.SetFont("s10", "Segoe UI")
    MaintenanceFormGui.OnEvent("Close", CloseVehicleMaintenancePlanForm)
    MaintenanceFormGui.OnEvent("Escape", CloseVehicleMaintenancePlanForm)

    MaintenanceGui.Opt("+Disabled")

    templateItems := GetVehicleMaintenanceTemplateLabels()

    MaintenanceFormGui.AddText("x20 y20 w500", "Vyberte šablonu nebo zadejte vlastní úkon. Každý plán může hlídat interval podle data, tachometru nebo obojího.")

    MaintenanceFormGui.AddText("x20 y58 w170", "Šablona")
    MaintenanceFormControls.template := MaintenanceFormGui.AddDropDownList("x220 y55 w250", templateItems)
    MaintenanceFormControls.template.Value := 1
    MaintenanceFormControls.template.OnEvent("Change", OnMaintenanceTemplateChanged)

    MaintenanceFormGui.AddText("x20 y93 w170", "Název úkonu (povinné)")
    MaintenanceFormControls.title := MaintenanceFormGui.AddEdit("x220 y90 w250")

    MaintenanceFormGui.AddText("x20 y128 w170", "Interval kilometrů")
    MaintenanceFormControls.intervalKm := MaintenanceFormGui.AddEdit("x220 y125 w250")

    MaintenanceFormGui.AddText("x20 y163 w170", "Interval měsíců")
    MaintenanceFormControls.intervalMonths := MaintenanceFormGui.AddEdit("x220 y160 w250")

    MaintenanceFormGui.AddText("x20 y198 w170", "Poslední servis - datum")
    MaintenanceFormControls.lastServiceDate := MaintenanceFormGui.AddEdit("x220 y195 w250")

    MaintenanceFormGui.AddText("x20 y233 w170", "Poslední servis - tachometr")
    MaintenanceFormControls.lastServiceOdometer := MaintenanceFormGui.AddEdit("x220 y230 w250")

    MaintenanceFormControls.isActive := MaintenanceFormGui.AddCheckBox("x220 y265 w250", "Plán je aktivní")
    MaintenanceFormControls.isActive.Value := 1

    MaintenanceFormGui.AddText("x20 y298 w170", "Poznámka")
    MaintenanceFormControls.note := MaintenanceFormGui.AddEdit("x20 y323 w450 h88 Multi")

    saveButton := MaintenanceFormGui.AddButton("x155 y426 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleMaintenancePlanFromForm)

    cancelButton := MaintenanceFormGui.AddButton("x285 y426 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleMaintenancePlanForm)

    if IsObject(plan) {
        MaintenanceFormControls.title.Text := plan.title
        MaintenanceFormControls.intervalKm.Text := plan.intervalKm
        MaintenanceFormControls.intervalMonths.Text := plan.intervalMonths
        MaintenanceFormControls.lastServiceDate.Text := plan.lastServiceDate
        MaintenanceFormControls.lastServiceOdometer.Text := plan.lastServiceOdometer
        MaintenanceFormControls.isActive.Value := plan.isActive ? 1 : 0
        MaintenanceFormControls.note.Text := plan.note
    }

    MaintenanceFormGui.Show("w490 h472")
    MaintenanceFormControls.title.Focus()
}

OnMaintenanceTemplateChanged(*) {
    global MaintenanceFormControls

    if !IsObject(MaintenanceFormControls) || !MaintenanceFormControls.Has("template") {
        return
    }

    template := GetVehicleMaintenanceTemplateByLabel(MaintenanceFormControls.template.Text)
    if !IsObject(template) {
        return
    }

    MaintenanceFormControls.title.Text := template.title
    MaintenanceFormControls.intervalKm.Text := template.intervalKm
    MaintenanceFormControls.intervalMonths.Text := template.intervalMonths
    MaintenanceFormControls.note.Text := template.note
}

CloseVehicleMaintenancePlanForm(*) {
    global MaintenanceFormGui, MaintenanceFormControls, MaintenanceFormMode, MaintenanceFormPlanId, MaintenanceFormVehicleId, MaintenanceGui

    if IsObject(MaintenanceFormGui) {
        MaintenanceFormGui.Destroy()
        MaintenanceFormGui := 0
    }

    MaintenanceFormControls := {}
    MaintenanceFormMode := ""
    MaintenanceFormPlanId := ""
    MaintenanceFormVehicleId := ""

    if IsObject(MaintenanceGui) {
        MaintenanceGui.Opt("-Disabled")
        WinActivate("ahk_id " MaintenanceGui.Hwnd)
    }
}

SaveVehicleMaintenancePlanFromForm(*) {
    global AppTitle, VehicleMaintenancePlans, MaintenanceFormControls, MaintenanceFormMode, MaintenanceFormPlanId, MaintenanceFormVehicleId

    title := Trim(MaintenanceFormControls.title.Text)
    intervalKm := NormalizePositiveIntegerText(MaintenanceFormControls.intervalKm.Text)
    intervalMonths := NormalizePositiveIntegerText(MaintenanceFormControls.intervalMonths.Text)
    lastServiceDate := NormalizeEventDate(MaintenanceFormControls.lastServiceDate.Text)
    lastServiceOdometer := NormalizeOdometerText(MaintenanceFormControls.lastServiceOdometer.Text)
    isActive := MaintenanceFormControls.isActive.Value ? 1 : 0
    note := Trim(MaintenanceFormControls.note.Text)

    if (title = "") {
        MsgBox("Vyplňte prosím název úkonu.", AppTitle, 0x30)
        MaintenanceFormControls.title.Focus()
        return
    }

    if (intervalKm = "" && intervalMonths = "") {
        MsgBox("Plán údržby musí mít vyplněný interval kilometrů, měsíců nebo obojí.", AppTitle, 0x30)
        MaintenanceFormControls.intervalKm.Focus()
        return
    }

    if (Trim(MaintenanceFormControls.intervalKm.Text) != "" && intervalKm = "") {
        MsgBox("Interval kilometrů zadejte jako kladné celé číslo.", AppTitle, 0x30)
        MaintenanceFormControls.intervalKm.Focus()
        return
    }

    if (Trim(MaintenanceFormControls.intervalMonths.Text) != "" && intervalMonths = "") {
        MsgBox("Interval měsíců zadejte jako kladné celé číslo.", AppTitle, 0x30)
        MaintenanceFormControls.intervalMonths.Focus()
        return
    }

    if (intervalMonths != "" && lastServiceDate = "") {
        MsgBox("Pro interval podle data vyplňte i datum posledního servisu ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        MaintenanceFormControls.lastServiceDate.Focus()
        return
    }

    if (Trim(MaintenanceFormControls.lastServiceDate.Text) != "" && lastServiceDate = "") {
        MsgBox("Datum posledního servisu musí být ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        MaintenanceFormControls.lastServiceDate.Focus()
        return
    }

    if (intervalKm != "" && lastServiceOdometer = "") {
        MsgBox("Pro interval podle tachometru vyplňte i stav tachometru při posledním servisu.", AppTitle, 0x30)
        MaintenanceFormControls.lastServiceOdometer.Focus()
        return
    }

    if (Trim(MaintenanceFormControls.lastServiceOdometer.Text) != "" && lastServiceOdometer = "") {
        MsgBox("Stav tachometru posledního servisu zadejte jako celé číslo.", AppTitle, 0x30)
        MaintenanceFormControls.lastServiceOdometer.Focus()
        return
    }

    plan := {
        id: (MaintenanceFormMode = "edit") ? MaintenanceFormPlanId : GenerateVehicleMaintenancePlanId(),
        vehicleId: MaintenanceFormVehicleId,
        title: title,
        intervalKm: intervalKm,
        intervalMonths: intervalMonths,
        lastServiceDate: lastServiceDate,
        lastServiceOdometer: lastServiceOdometer,
        isActive: isActive,
        note: note
    }

    index := FindVehicleMaintenancePlanIndexById(plan.id)
    if index {
        VehicleMaintenancePlans[index] := plan
    } else {
        VehicleMaintenancePlans.Push(plan)
    }

    SaveVehicleMaintenancePlans()
    CloseVehicleMaintenancePlanForm()
    RefreshMaintenanceDependentState(plan.vehicleId)
    PopulateVehicleMaintenanceList(plan.id, true)
}

OpenVehicleMaintenanceCompleteForm(plan) {
    global AppTitle, MaintenanceGui, MaintenanceCompleteGui, MaintenanceCompleteControls, MaintenanceCompletePlanId, MaintenanceCompleteVehicleId

    if IsObject(MaintenanceCompleteGui) {
        WinActivate("ahk_id " MaintenanceCompleteGui.Hwnd)
        return
    }

    if !IsObject(MaintenanceGui) {
        return
    }

    snapshot := BuildVehicleMaintenancePlanSnapshot(plan)
    MaintenanceCompletePlanId := plan.id
    MaintenanceCompleteVehicleId := plan.vehicleId
    MaintenanceCompleteControls := {}

    MaintenanceCompleteGui := Gui("+Owner" MaintenanceGui.Hwnd, AppTitle " - Označit úkon jako splněný")
    MaintenanceCompleteGui.SetFont("s10", "Segoe UI")
    MaintenanceCompleteGui.OnEvent("Close", CloseVehicleMaintenanceCompleteForm)
    MaintenanceCompleteGui.OnEvent("Escape", CloseVehicleMaintenanceCompleteForm)

    MaintenanceGui.Opt("+Disabled")

    MaintenanceCompleteGui.AddText("x20 y20 w480", "Potvrďte datum a tachometr, od kterých se má plán počítat znovu. Volitelně lze záznam rovnou uložit i do historie událostí.")
    MaintenanceCompleteGui.AddText("x20 y54 w480", "Úkon: " snapshot.title)
    MaintenanceCompleteGui.AddText("x20 y78 w480", "Dosavadní stav: " snapshot.statusText)

    MaintenanceCompleteGui.AddText("x20 y115 w180", "Datum provedení")
    MaintenanceCompleteControls.completedDate := MaintenanceCompleteGui.AddEdit("x220 y112 w220", FormatTime(A_Now, "dd.MM.yyyy"))

    MaintenanceCompleteGui.AddText("x20 y150 w180", "Tachometr při provedení")
    defaultOdometer := GetLatestVehicleOdometerText(plan.vehicleId)
    if (Trim(defaultOdometer) = "") {
        defaultOdometer := plan.lastServiceOdometer
    }
    MaintenanceCompleteControls.completedOdometer := MaintenanceCompleteGui.AddEdit("x220 y147 w220", defaultOdometer)

    MaintenanceCompleteControls.addHistory := MaintenanceCompleteGui.AddCheckBox("x20 y185 w420", "Současně zapsat i do historie událostí")
    MaintenanceCompleteControls.addHistory.Value := 1

    MaintenanceCompleteGui.AddText("x20 y220 w180", "Cena do historie")
    MaintenanceCompleteControls.historyCost := MaintenanceCompleteGui.AddEdit("x220 y217 w220")

    MaintenanceCompleteGui.AddText("x20 y255 w180", "Poznámka do historie")
    MaintenanceCompleteControls.historyNote := MaintenanceCompleteGui.AddEdit("x20 y280 w420 h80 Multi")

    saveButton := MaintenanceCompleteGui.AddButton("x145 y375 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleMaintenanceCompletionFromForm)

    cancelButton := MaintenanceCompleteGui.AddButton("x275 y375 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleMaintenanceCompleteForm)

    MaintenanceCompleteGui.Show("w470 h420")
    MaintenanceCompleteControls.completedDate.Focus()
}

CloseVehicleMaintenanceCompleteForm(*) {
    global MaintenanceCompleteGui, MaintenanceCompleteControls, MaintenanceCompletePlanId, MaintenanceCompleteVehicleId, MaintenanceGui

    if IsObject(MaintenanceCompleteGui) {
        MaintenanceCompleteGui.Destroy()
        MaintenanceCompleteGui := 0
    }

    MaintenanceCompleteControls := {}
    MaintenanceCompletePlanId := ""
    MaintenanceCompleteVehicleId := ""

    if IsObject(MaintenanceGui) {
        MaintenanceGui.Opt("-Disabled")
        WinActivate("ahk_id " MaintenanceGui.Hwnd)
    }
}

SaveVehicleMaintenanceCompletionFromForm(*) {
    global AppTitle, VehicleMaintenancePlans, VehicleHistory, MaintenanceCompleteControls, MaintenanceCompletePlanId

    plan := FindVehicleMaintenancePlanById(MaintenanceCompletePlanId)
    if !IsObject(plan) {
        CloseVehicleMaintenanceCompleteForm()
        return
    }

    completedDate := NormalizeEventDate(MaintenanceCompleteControls.completedDate.Text)
    completedOdometer := NormalizeOdometerText(MaintenanceCompleteControls.completedOdometer.Text)
    historyCost := NormalizeDecimalText(MaintenanceCompleteControls.historyCost.Text)
    historyNote := Trim(MaintenanceCompleteControls.historyNote.Text)
    addHistory := MaintenanceCompleteControls.addHistory.Value ? 1 : 0

    if (completedDate = "") {
        MsgBox("Datum provedení musí být ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        MaintenanceCompleteControls.completedDate.Focus()
        return
    }

    if (plan.intervalKm != "" && completedOdometer = "") {
        MsgBox("Pro kilometrický interval vyplňte i stav tachometru při provedení úkonu.", AppTitle, 0x30)
        MaintenanceCompleteControls.completedOdometer.Focus()
        return
    }

    if (Trim(MaintenanceCompleteControls.completedOdometer.Text) != "" && completedOdometer = "") {
        MsgBox("Tachometr při provedení zadejte jako celé číslo.", AppTitle, 0x30)
        MaintenanceCompleteControls.completedOdometer.Focus()
        return
    }

    if (Trim(MaintenanceCompleteControls.historyCost.Text) != "" && historyCost = "") {
        MsgBox("Cenu do historie zadejte jako číslo, například 2500.", AppTitle, 0x30)
        MaintenanceCompleteControls.historyCost.Focus()
        return
    }

    index := FindVehicleMaintenancePlanIndexById(plan.id)
    if !index {
        CloseVehicleMaintenanceCompleteForm()
        return
    }

    updatedPlan := VehicleMaintenancePlans[index]
    updatedPlan.lastServiceDate := completedDate
    if (completedOdometer != "") {
        updatedPlan.lastServiceOdometer := completedOdometer
    }
    updatedPlan.isActive := 1
    VehicleMaintenancePlans[index] := updatedPlan
    SaveVehicleMaintenancePlans()

    if addHistory {
        note := Trim(historyNote)
        if (note = "") {
            note := "Zapsáno z plánu údržby."
        }

        VehicleHistory.Push({
            id: GenerateHistoryEventId(),
            vehicleId: updatedPlan.vehicleId,
            eventDate: completedDate,
            eventType: updatedPlan.title,
            odometer: completedOdometer,
            cost: historyCost,
            note: note
        })
        SaveVehicleHistory()
    }

    CloseVehicleMaintenanceCompleteForm()
    RefreshMaintenanceDependentState(updatedPlan.vehicleId)
    PopulateVehicleMaintenanceList(updatedPlan.id, true)
}

BuildVehicleMaintenanceSummaryText(vehicleId) {
    plans := BuildVehicleMaintenanceSnapshots(vehicleId, true)
    if (plans.Length = 0) {
        return "K tomuto vozidlu zatím nejsou uložené žádné plány údržby."
    }

    activeCount := 0
    pausedCount := 0
    for snapshot in plans {
        if snapshot.plan.isActive {
            activeCount += 1
        } else {
            pausedCount += 1
        }
    }

    summary := "Plánů údržby: " plans.Length ". Aktivních: " activeCount "."
    if (pausedCount > 0) {
        summary .= " Pozastavených: " pausedCount "."
    }

    if (activeCount = 0) {
        summary .= " Všechny plány jsou momentálně pozastavené."
        return summary
    }

    nextSnapshot := ""
    for snapshot in plans {
        if snapshot.plan.isActive {
            nextSnapshot := snapshot
            break
        }
    }

    if IsObject(nextSnapshot) {
        summary .= " Nejbližší: " nextSnapshot.title " (" nextSnapshot.statusText
        if (nextSnapshot.nextServiceText != "") {
            summary .= "; další " nextSnapshot.nextServiceText
        }
        summary .= ")."
    }

    return summary
}

BuildVehicleMaintenanceSnapshots(vehicleId, includeInactive := true) {
    vehicle := FindVehicleById(vehicleId)
    snapshots := []

    for plan in GetVehicleMaintenancePlans(vehicleId, includeInactive) {
        snapshots.Push(BuildVehicleMaintenancePlanSnapshot(plan, vehicle))
    }

    SortVehicleMaintenanceSnapshotsByPriority(&snapshots)
    return snapshots
}

SortVehicleMaintenanceSnapshotsByPriority(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleMaintenanceSnapshotsByPriority(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleMaintenanceSnapshotsByPriority(left, right) {
    result := CompareNumberValues(GetVehicleMaintenanceStatusSortValue(left), GetVehicleMaintenanceStatusSortValue(right))
    if (result != 0) {
        return result
    }

    result := CompareTextValues(left.overviewSortKey, right.overviewSortKey)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.title, right.title)
}

BuildVehicleMaintenancePlanSnapshot(plan, vehicle := "") {
    if !IsObject(vehicle) {
        vehicle := FindVehicleById(plan.vehicleId)
    }

    currentOdometerText := GetLatestVehicleOdometerText(plan.vehicleId)
    currentOdometer := NormalizeOdometerText(currentOdometerText)
    intervalKm := NormalizePositiveIntegerText(plan.intervalKm)
    intervalMonths := NormalizePositiveIntegerText(plan.intervalMonths)
    dueDate := ""
    dueDateStamp := ""
    nextOdometer := ""
    remainingKm := ""
    kmOverdueBy := 0
    dateSoon := false
    dateOverdue := false
    kmSoon := false
    kmOverdue := false
    dateDaysRemaining := ""
    requiresCurrentOdometer := (intervalKm != "")

    if (intervalMonths != "" && plan.lastServiceDate != "") {
        dueDate := AddMonthsToEventDate(plan.lastServiceDate, intervalMonths + 0)
        dueDateStamp := ParseEventDateStamp(dueDate)
        if (dueDateStamp != "") {
            if (dueDateStamp < A_Now) {
                dateOverdue := true
            } else {
                cutoff := DateAdd(A_Now, GetMaintenanceReminderDays(), "Days")
                if (dueDateStamp <= cutoff) {
                    dateSoon := true
                    dateDaysRemaining := DateDiff(dueDateStamp, A_Now, "Days")
                }
            }
        }
    }

    if (intervalKm != "" && plan.lastServiceOdometer != "") {
        nextOdometer := (plan.lastServiceOdometer + 0) + (intervalKm + 0)
        if (currentOdometer != "") {
            remainingKm := nextOdometer - (currentOdometer + 0)
            if (remainingKm < 0) {
                kmOverdue := true
                kmOverdueBy := Abs(remainingKm)
            } else if (remainingKm <= GetMaintenanceReminderKm()) {
                kmSoon := true
            }
        }
    }

    snapshot := {
        plan: plan,
        vehicle: vehicle,
        title: Trim(plan.title) != "" ? plan.title : "(bez názvu)",
        intervalText: BuildVehicleMaintenanceIntervalText(plan),
        lastServiceText: BuildVehicleMaintenanceLastServiceText(plan),
        nextServiceText: "",
        statusText: "",
        dueDate: dueDate,
        dueDateStamp: dueDateStamp,
        nextOdometer: nextOdometer,
        currentOdometer: currentOdometer,
        currentOdometerText: currentOdometerText,
        remainingKm: remainingKm,
        kmOverdueBy: kmOverdueBy,
        dateSoon: dateSoon,
        dateOverdue: dateOverdue,
        dateDaysRemaining: dateDaysRemaining,
        kmSoon: kmSoon,
        kmOverdue: kmOverdue,
        requiresCurrentOdometer: requiresCurrentOdometer,
        overviewSortKey: ""
    }

    snapshot.nextServiceText := BuildVehicleMaintenanceNextServiceText(snapshot)
    snapshot.statusText := BuildVehicleMaintenanceStatusText(snapshot)
    snapshot.overviewSortKey := BuildVehicleMaintenanceOverviewSortKey(snapshot)
    return snapshot
}

BuildVehicleMaintenanceIntervalText(plan) {
    parts := []
    if (Trim(plan.intervalKm) != "") {
        parts.Push(FormatHistoryOdometer(plan.intervalKm) " km")
    }
    if (Trim(plan.intervalMonths) != "") {
        parts.Push(plan.intervalMonths " měs.")
    }
    return parts.Length > 0 ? JoinInline(parts, " | ") : "Nevyplněno"
}

BuildVehicleMaintenanceLastServiceText(plan) {
    parts := []
    if (Trim(plan.lastServiceDate) != "") {
        parts.Push(plan.lastServiceDate)
    }
    if (Trim(plan.lastServiceOdometer) != "") {
        parts.Push(FormatHistoryOdometer(plan.lastServiceOdometer))
    }
    return parts.Length > 0 ? JoinInline(parts, " | ") : "Nevyplněno"
}

BuildVehicleMaintenanceNextServiceText(snapshot) {
    parts := []
    if (snapshot.dueDate != "") {
        parts.Push(snapshot.dueDate)
    }
    if (snapshot.nextOdometer != "") {
        parts.Push(FormatHistoryOdometer(snapshot.nextOdometer))
    }
    return parts.Length > 0 ? JoinInline(parts, " | ") : "Nevyplněno"
}

BuildVehicleMaintenanceStatusText(snapshot) {
    if !snapshot.plan.isActive {
        return "Pozastaveno"
    }

    parts := []
    if snapshot.dateOverdue {
        parts.Push("Po termínu")
    } else if snapshot.dateSoon {
        parts.Push((snapshot.dateDaysRemaining + 0) < 1 ? "Dnes" : "Do " snapshot.dateDaysRemaining " dnů")
    }

    if snapshot.kmOverdue {
        parts.Push("Po limitu o " FormatHistoryOdometer(snapshot.kmOverdueBy) " km")
    } else if (snapshot.remainingKm != "" && snapshot.kmSoon) {
        parts.Push("Do " FormatHistoryOdometer(snapshot.remainingKm) " km")
    }

    if snapshot.requiresCurrentOdometer && snapshot.currentOdometer = "" {
        parts.Push("Chybí aktuální tachometr")
    }

    if (parts.Length = 0) {
        return "V pořádku"
    }

    return JoinInline(parts, " | ")
}

BuildVehicleMaintenanceOverviewSortKey(snapshot) {
    group := GetVehicleMaintenanceStatusSortValue(snapshot)
    dimensionRank := 3
    dateKey := (snapshot.dueDateStamp != "") ? snapshot.dueDateStamp : "99999999999999"
    kmKey := "999999999"

    if snapshot.dateOverdue || snapshot.dateSoon {
        dimensionRank := 1
    } else if snapshot.kmOverdue || snapshot.kmSoon {
        dimensionRank := 2
    }

    if (snapshot.remainingKm != "") {
        kmDistance := snapshot.remainingKm
        if snapshot.kmOverdue {
            kmDistance := 999999999 - Min(999999999, snapshot.kmOverdueBy)
        } else if (kmDistance < 0) {
            kmDistance := 0
        }
        kmKey := Format("{:09}", kmDistance)
    }

    return group "|" dimensionRank "|" dateKey "|" kmKey "|" StrLower(snapshot.title)
}

GetVehicleMaintenanceStatusSortValue(snapshot) {
    if !snapshot.plan.isActive {
        return 4
    }
    if IsVehicleMaintenanceSnapshotOverdue(snapshot) {
        return 1
    }
    if IsVehicleMaintenanceSnapshotUpcoming(snapshot) {
        return 2
    }
    return 3
}

IsVehicleMaintenanceSnapshotOverdue(snapshot) {
    return snapshot.plan.isActive && (snapshot.dateOverdue || snapshot.kmOverdue)
}

IsVehicleMaintenanceSnapshotUpcoming(snapshot) {
    return snapshot.plan.isActive && !IsVehicleMaintenanceSnapshotOverdue(snapshot) && (snapshot.dateSoon || snapshot.kmSoon)
}

GetLatestVehicleOdometerValue(vehicleId) {
    text := GetLatestVehicleOdometerText(vehicleId)
    return NormalizeOdometerText(text)
}

GetUpcomingVehicleMaintenance(vehicleId := "") {
    global VehicleMaintenancePlans

    items := []
    for plan in VehicleMaintenancePlans {
        if !plan.isActive {
            continue
        }
        if (vehicleId != "" && plan.vehicleId != vehicleId) {
            continue
        }

        vehicle := FindVehicleById(plan.vehicleId)
        if !IsObject(vehicle) {
            continue
        }

        snapshot := BuildVehicleMaintenancePlanSnapshot(plan, vehicle)
        if !IsVehicleMaintenanceSnapshotOverdue(snapshot) && !IsVehicleMaintenanceSnapshotUpcoming(snapshot) {
            continue
        }

        items.Push({
            kind: "maintenance",
            vehicle: vehicle,
            plan: plan,
            snapshot: snapshot,
            dueStamp: (snapshot.dueDateStamp != "") ? snapshot.dueDateStamp : "99999999999999",
            overviewSortKey: snapshot.overviewSortKey,
            term: snapshot.nextServiceText,
            status: snapshot.statusText,
            entryId: plan.id
        })
    }

    SortVehicleMaintenanceUpcomingItems(&items)
    return items
}

SortVehicleMaintenanceUpcomingItems(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleMaintenanceUpcomingItems(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleMaintenanceUpcomingItems(left, right) {
    result := CompareTextValues(left.overviewSortKey, right.overviewSortKey)
    if (result != 0) {
        return result
    }

    return CompareVehicles(left.vehicle, right.vehicle)
}

GetVehicleMaintenanceStateText(vehicleId) {
    items := GetUpcomingVehicleMaintenance(vehicleId)
    if (items.Length = 0) {
        return ""
    }

    return "Servis: " items[1].snapshot.statusText
}

GetVehicleMaintenanceTemplates() {
    static templates := [
        {label: "Vlastní položka", title: "", intervalKm: "", intervalMonths: "", note: ""},
        {label: "Motorový olej a filtr", title: "Motorový olej a filtr", intervalKm: "15000", intervalMonths: "12", note: "Pravidelná výměna oleje a olejového filtru."},
        {label: "Vzduchový filtr", title: "Vzduchový filtr", intervalKm: "30000", intervalMonths: "24", note: "Zkontrolovat nebo vyměnit vzduchový filtr."},
        {label: "Kabinový filtr", title: "Kabinový filtr", intervalKm: "15000", intervalMonths: "12", note: "Pravidelná výměna pylového filtru."},
        {label: "Brzdová kapalina", title: "Brzdová kapalina", intervalKm: "", intervalMonths: "24", note: "Pravidelná výměna brzdové kapaliny."},
        {label: "Chladicí kapalina", title: "Chladicí kapalina", intervalKm: "", intervalMonths: "60", note: "Kontrola a obnova chladicí kapaliny."},
        {label: "Rozvody", title: "Rozvody", intervalKm: "90000", intervalMonths: "60", note: "Rozvodový řemen nebo řetěz podle doporučení výrobce."},
        {label: "Převodový olej", title: "Převodový olej", intervalKm: "60000", intervalMonths: "48", note: "Kontrola nebo výměna převodového oleje."},
        {label: "Klimatizace a dezinfekce", title: "Klimatizace a dezinfekce", intervalKm: "", intervalMonths: "12", note: "Servis klimatizace a dezinfekce okruhu."}
    ]

    return templates
}

GetVehicleMaintenanceTemplateLabels() {
    labels := []
    for template in GetVehicleMaintenanceTemplates() {
        labels.Push(template.label)
    }
    return labels
}

GetVehicleMaintenanceTemplateByLabel(label) {
    for template in GetVehicleMaintenanceTemplates() {
        if (template.label = label) {
            return template
        }
    }
    return ""
}

RefreshMaintenanceDependentState(vehicleId) {
    if IsObject(MaintenanceGui) {
        PopulateVehicleMaintenanceList()
    }

    if IsObject(GlobalSearchGui) {
        PopulateGlobalSearchList()
    }

    RefreshVehicleList(vehicleId)
    CheckDueVehicles(false, false)
    UpdateTrayIconTip(true)
}
