OpenVehicleMaintenanceDialog(vehicle, openAddPlan := false, selectPlanId := "", openRecommendations := false) {
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

    recommendButton := MaintenanceGui.AddButton("x40 y382 w170 h30", "Doporučené šablony")
    recommendButton.OnEvent("Click", AddRecommendedVehicleMaintenancePlans)

    addButton := MaintenanceGui.AddButton("x220 y382 w120 h30", "Přidat úkon")
    addButton.OnEvent("Click", AddVehicleMaintenancePlan)

    editButton := MaintenanceGui.AddButton("x350 y382 w120 h30", "Upravit úkon")
    editButton.OnEvent("Click", EditSelectedVehicleMaintenancePlan)

    MaintenanceCompleteButton := MaintenanceGui.AddButton("x480 y382 w140 h30", "Označit splněno")
    MaintenanceCompleteButton.OnEvent("Click", CompleteSelectedVehicleMaintenancePlan)

    deleteButton := MaintenanceGui.AddButton("x630 y382 w130 h30", "Odstranit úkon")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleMaintenancePlan)

    detailButton := MaintenanceGui.AddButton("x260 y418 w160 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromMaintenance)

    closeButton := MaintenanceGui.AddButton("x480 y418 w140 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleMaintenanceDialog)

    MaintenanceGui.Show("w900 h468")
    PopulateVehicleMaintenanceList(selectPlanId, true)

    if openRecommendations {
        preview := GetVehicleMaintenanceRecommendationPreview(vehicle.id)
        if (preview.missing.Length > 0) {
            OpenVehicleMaintenanceRecommendationDialog(preview)
            return
        }
    }

    if openAddPlan {
        OpenVehicleMaintenancePlanForm("add")
    } else if (VisibleMaintenancePlanIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleMaintenanceDialog(*) {
    global MaintenanceGui, MaintenanceVehicleId, MaintenanceList, MaintenanceSummaryLabel, MaintenanceAllPlans, MaintenanceSearchCtrl, VisibleMaintenancePlanIds, MaintenanceSortColumn, MaintenanceSortDescending, MaintenanceCompleteButton
    global MaintenanceRecommendGui, MaintenanceRecommendList, MaintenanceRecommendSummaryLabel, MaintenanceRecommendControls, MaintenanceRecommendItems, MaintenanceRecommendVehicleId, MaintenanceRecommendSelectedIndex, MaintenanceRecommendLoading, MainGui

    if IsObject(MaintenanceRecommendGui) {
        MaintenanceRecommendGui.Destroy()
        MaintenanceRecommendGui := 0
    }

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
    MaintenanceRecommendList := 0
    MaintenanceRecommendSummaryLabel := 0
    MaintenanceRecommendControls := {}
    MaintenanceRecommendItems := []
    MaintenanceRecommendVehicleId := ""
    MaintenanceRecommendSelectedIndex := 0
    MaintenanceRecommendLoading := false
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

AddRecommendedVehicleMaintenancePlans(*) {
    global AppTitle, MaintenanceVehicleId

    preview := GetVehicleMaintenanceRecommendationPreview(MaintenanceVehicleId)
    if !preview.HasOwnProp("vehicle") || !IsObject(preview.vehicle) {
        return
    }

    if (preview.recommended.Length = 0) {
        AppMsgBox(
            "Vehimap pro vybrané vozidlo zatím nemá připravené žádné doporučené servisní šablony.",
            AppTitle,
            0x40
        )
        return
    }

    if (preview.missing.Length = 0) {
        AppMsgBox(
            "Vehimap pro vozidlo " preview.vehicle.name " už nenašel žádné další doporučené servisní šablony.`n`n"
            "Profil doporučení: " preview.profileLabel ".",
            AppTitle,
            0x40
        )
        return
    }

    OpenVehicleMaintenanceRecommendationDialog(preview)
}

OfferVehicleMaintenanceRecommendationsAfterCreate(vehicleId) {
    global AppTitle

    preview := GetVehicleMaintenanceRecommendationPreview(vehicleId)
    if !preview.HasOwnProp("vehicle") || !IsObject(preview.vehicle) || preview.missing.Length = 0 {
        return false
    }

    text := "Vehimap pro nové vozidlo " preview.vehicle.name " našel " preview.missing.Length " doporučených servisních šablon.`n`n"
        . "Profil doporučení: " preview.profileLabel ".`n`n"
        . "Chcete je otevřít a případně rovnou přidat?"
    result := AppMsgBox(text, AppTitle, 0x24)
    if (result != "Yes") {
        return false
    }

    hooks := GetVehimapTestHooks()
    if IsObject(hooks) {
        hooks.postCreateMaintenanceRecommendation := {
            vehicleId: vehicleId,
            profileLabel: preview.profileLabel,
            missingCount: preview.missing.Length
        }
        if (hooks.HasOwnProp("skipPostCreateMaintenanceRecommendationOpen") && hooks.skipPostCreateMaintenanceRecommendationOpen) {
            return true
        }
    }

    OpenVehicleMaintenanceDialog(preview.vehicle, false, "", true)
    return true
}

OpenVehicleMaintenanceRecommendationDialog(preview) {
    global AppTitle, MaintenanceGui, MaintenanceRecommendGui, MaintenanceRecommendList, MaintenanceRecommendSummaryLabel, MaintenanceRecommendControls, MaintenanceRecommendItems, MaintenanceRecommendVehicleId, MaintenanceRecommendSelectedIndex, MaintenanceRecommendLoading

    if IsObject(MaintenanceRecommendGui) {
        WinActivate("ahk_id " MaintenanceRecommendGui.Hwnd)
        return
    }

    hooks := GetVehimapTestHooks()
    if IsObject(hooks) && hooks.HasOwnProp("maintenanceRecommendationSelection") {
        RunMaintenanceRecommendationSelectionInTestMode(preview, hooks.maintenanceRecommendationSelection)
        return
    }

    if !IsObject(MaintenanceGui) {
        return
    }

    MaintenanceRecommendItems := BuildVehicleMaintenanceRecommendationDrafts(preview.missing)
    MaintenanceRecommendVehicleId := preview.vehicle.id
    MaintenanceRecommendSelectedIndex := 0
    MaintenanceRecommendLoading := false
    MaintenanceRecommendControls := {}

    MaintenanceRecommendGui := Gui("+Owner" MaintenanceGui.Hwnd, AppTitle " - Doporučené servisní šablony")
    MaintenanceRecommendGui.SetFont("s10", "Segoe UI")
    MaintenanceRecommendGui.OnEvent("Close", CloseVehicleMaintenanceRecommendationDialog)
    MaintenanceRecommendGui.OnEvent("Escape", CloseVehicleMaintenanceRecommendationDialog)

    MaintenanceGui.Opt("+Disabled")

    MaintenanceRecommendGui.AddText("x20 y18 w820", "Vehimap pro vozidlo " preview.vehicle.name " našel chybějící servisní plány. Vyberte, které chcete přidat, a vybranou položku můžete ještě upravit.")
    MaintenanceRecommendGui.AddText("x20 y45 w820", "Profil doporučení: " preview.profileLabel)
    MaintenanceRecommendSummaryLabel := MaintenanceRecommendGui.AddText("x20 y72 w820", "")

    MaintenanceRecommendGui.AddText("x20 y104 w320", "Doporučené servisní plány")
    MaintenanceRecommendList := MaintenanceRecommendGui.AddListView("x20 y128 w360 h250 Checked Grid -Multi", ["Úkon", "Interval", "Poznámka"])
    MaintenanceRecommendList.OnEvent("ItemSelect", OnMaintenanceRecommendationSelectionChanged)
    MaintenanceRecommendList.OnEvent("Click", OnMaintenanceRecommendationListClicked)
    MaintenanceRecommendList.ModifyCol(1, "165")
    MaintenanceRecommendList.ModifyCol(2, "85")
    MaintenanceRecommendList.ModifyCol(3, "90")

    selectAllButton := MaintenanceRecommendGui.AddButton("x20 y388 w110 h28", "Vybrat vše")
    selectAllButton.OnEvent("Click", SelectAllMaintenanceRecommendations)

    clearAllButton := MaintenanceRecommendGui.AddButton("x140 y388 w130 h28", "Vymazat výběr")
    clearAllButton.OnEvent("Click", ClearAllMaintenanceRecommendations)

    MaintenanceRecommendGui.AddText("x410 y104 w380", "Úprava vybrané šablony")
    MaintenanceRecommendGui.AddText("x410 y128 w320", "Změny se uloží jen do právě přidávaných plánů.")

    MaintenanceRecommendGui.AddText("x410 y166 w170", "Název úkonu")
    MaintenanceRecommendControls.title := MaintenanceRecommendGui.AddEdit("x580 y163 w240")
    MaintenanceRecommendControls.title.OnEvent("Change", OnMaintenanceRecommendationDraftFieldChanged)

    MaintenanceRecommendGui.AddText("x410 y201 w170", "Interval kilometrů")
    MaintenanceRecommendControls.intervalKm := MaintenanceRecommendGui.AddEdit("x580 y198 w240")
    MaintenanceRecommendControls.intervalKm.OnEvent("Change", OnMaintenanceRecommendationDraftFieldChanged)

    MaintenanceRecommendGui.AddText("x410 y236 w170", "Interval měsíců")
    MaintenanceRecommendControls.intervalMonths := MaintenanceRecommendGui.AddEdit("x580 y233 w240")
    MaintenanceRecommendControls.intervalMonths.OnEvent("Change", OnMaintenanceRecommendationDraftFieldChanged)

    MaintenanceRecommendGui.AddText("x410 y271 w170", "Poznámka")
    MaintenanceRecommendControls.note := MaintenanceRecommendGui.AddEdit("x410 y296 w410 h120 Multi")
    MaintenanceRecommendControls.note.OnEvent("Change", OnMaintenanceRecommendationDraftFieldChanged)

    saveButton := MaintenanceRecommendGui.AddButton("x465 y432 w150 h30 Default", "Přidat vybrané")
    saveButton.OnEvent("Click", SaveSelectedVehicleMaintenanceRecommendationsFromDialog)

    cancelButton := MaintenanceRecommendGui.AddButton("x625 y432 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleMaintenanceRecommendationDialog)

    MaintenanceRecommendGui.Show("w850 h480")
    PopulateMaintenanceRecommendationList(1)
}

CloseVehicleMaintenanceRecommendationDialog(*) {
    global MaintenanceRecommendGui, MaintenanceRecommendList, MaintenanceRecommendSummaryLabel, MaintenanceRecommendControls, MaintenanceRecommendItems, MaintenanceRecommendVehicleId, MaintenanceRecommendSelectedIndex, MaintenanceRecommendLoading, MaintenanceGui

    if IsObject(MaintenanceRecommendGui) {
        MaintenanceRecommendGui.Destroy()
        MaintenanceRecommendGui := 0
    }

    MaintenanceRecommendList := 0
    MaintenanceRecommendSummaryLabel := 0
    MaintenanceRecommendControls := {}
    MaintenanceRecommendItems := []
    MaintenanceRecommendVehicleId := ""
    MaintenanceRecommendSelectedIndex := 0
    MaintenanceRecommendLoading := false

    if IsObject(MaintenanceGui) {
        MaintenanceGui.Opt("-Disabled")
        WinActivate("ahk_id " MaintenanceGui.Hwnd)
    }
}

PopulateMaintenanceRecommendationList(selectIndex := 1) {
    global MaintenanceRecommendGui, MaintenanceRecommendList, MaintenanceRecommendItems

    if !IsObject(MaintenanceRecommendGui) || !IsObject(MaintenanceRecommendList) {
        return
    }

    MaintenanceRecommendList.Opt("-Redraw")
    MaintenanceRecommendList.Delete()
    for index, item in MaintenanceRecommendItems {
        row := MaintenanceRecommendList.Add("", item.title, BuildVehicleMaintenanceIntervalText(item), ShortenText(item.note, 38))
        MaintenanceRecommendList.Modify(row, item.selected ? "Check" : "-Check")
    }
    MaintenanceRecommendList.Opt("+Redraw")

    UpdateMaintenanceRecommendationSummary()
    SelectMaintenanceRecommendationRow(selectIndex, true)
}

SelectMaintenanceRecommendationRow(index, focusList := false) {
    global MaintenanceRecommendList, MaintenanceRecommendItems, MaintenanceRecommendSelectedIndex

    if !IsObject(MaintenanceRecommendList) || index < 1 || index > MaintenanceRecommendItems.Length {
        MaintenanceRecommendSelectedIndex := 0
        LoadMaintenanceRecommendationDraftIntoControls(0)
        return
    }

    MaintenanceRecommendSelectedIndex := index
    MaintenanceRecommendList.Modify(index, focusList ? "Select Focus Vis" : "Select Vis")
    LoadMaintenanceRecommendationDraftIntoControls(index)
}

OnMaintenanceRecommendationSelectionChanged(*) {
    global MaintenanceRecommendList

    if !IsObject(MaintenanceRecommendList) {
        return
    }

    row := MaintenanceRecommendList.GetNext(0)
    SelectMaintenanceRecommendationRow(row)
}

OnMaintenanceRecommendationListClicked(*) {
    SyncMaintenanceRecommendationSelectionsFromList()
    UpdateMaintenanceRecommendationSummary()
}

LoadMaintenanceRecommendationDraftIntoControls(index) {
    global MaintenanceRecommendControls, MaintenanceRecommendItems, MaintenanceRecommendLoading

    if !IsObject(MaintenanceRecommendControls) || !MaintenanceRecommendControls.Has("title") {
        return
    }

    MaintenanceRecommendLoading := true
    if (index < 1 || index > MaintenanceRecommendItems.Length) {
        MaintenanceRecommendControls.title.Text := ""
        MaintenanceRecommendControls.intervalKm.Text := ""
        MaintenanceRecommendControls.intervalMonths.Text := ""
        MaintenanceRecommendControls.note.Text := ""
        MaintenanceRecommendLoading := false
        return
    }

    item := MaintenanceRecommendItems[index]
    MaintenanceRecommendControls.title.Text := item.title
    MaintenanceRecommendControls.intervalKm.Text := item.intervalKm
    MaintenanceRecommendControls.intervalMonths.Text := item.intervalMonths
    MaintenanceRecommendControls.note.Text := item.note
    MaintenanceRecommendLoading := false
}

OnMaintenanceRecommendationDraftFieldChanged(*) {
    global MaintenanceRecommendControls, MaintenanceRecommendItems, MaintenanceRecommendSelectedIndex, MaintenanceRecommendLoading, MaintenanceRecommendList

    if MaintenanceRecommendLoading || !IsObject(MaintenanceRecommendControls) {
        return
    }

    index := MaintenanceRecommendSelectedIndex
    if (index < 1 || index > MaintenanceRecommendItems.Length) {
        return
    }

    item := MaintenanceRecommendItems[index]
    item.title := MaintenanceRecommendControls.title.Text
    item.intervalKm := MaintenanceRecommendControls.intervalKm.Text
    item.intervalMonths := MaintenanceRecommendControls.intervalMonths.Text
    item.note := MaintenanceRecommendControls.note.Text

    if IsObject(MaintenanceRecommendList) {
        MaintenanceRecommendList.Modify(index, "", item.title, BuildVehicleMaintenanceIntervalText(item), ShortenText(item.note, 38))
    }
}

SyncMaintenanceRecommendationSelectionsFromList() {
    global MaintenanceRecommendList, MaintenanceRecommendItems

    if !IsObject(MaintenanceRecommendList) {
        return
    }

    checkedRows := Map()
    row := 0
    while row := MaintenanceRecommendList.GetNext(row, "Checked") {
        checkedRows[row] := true
    }

    for index, item in MaintenanceRecommendItems {
        item.selected := checkedRows.Has(index)
    }
}

UpdateMaintenanceRecommendationSummary() {
    global MaintenanceRecommendSummaryLabel, MaintenanceRecommendItems

    if !IsObject(MaintenanceRecommendSummaryLabel) {
        return
    }

    selectedCount := 0
    for item in MaintenanceRecommendItems {
        if item.selected {
            selectedCount += 1
        }
    }

    MaintenanceRecommendSummaryLabel.Text := "Doporučeno: " MaintenanceRecommendItems.Length ". Vybráno k přidání: " selectedCount "."
}

SelectAllMaintenanceRecommendations(*) {
    SetAllMaintenanceRecommendationSelections(true)
}

ClearAllMaintenanceRecommendations(*) {
    SetAllMaintenanceRecommendationSelections(false)
}

SetAllMaintenanceRecommendationSelections(selected) {
    global MaintenanceRecommendList, MaintenanceRecommendItems

    if !IsObject(MaintenanceRecommendList) {
        return
    }

    for index, item in MaintenanceRecommendItems {
        item.selected := selected ? 1 : 0
        MaintenanceRecommendList.Modify(index, selected ? "Check" : "-Check")
    }

    UpdateMaintenanceRecommendationSummary()
}

SaveSelectedVehicleMaintenanceRecommendationsFromDialog(*) {
    global AppTitle, MaintenanceRecommendVehicleId, MaintenanceRecommendSelectedIndex

    result := ApplyVehicleMaintenanceRecommendationDrafts(MaintenanceRecommendVehicleId, MaintenanceRecommendItems)
    if result.HasOwnProp("errorMessage") {
        AppMsgBox(result.errorMessage, AppTitle, 0x30)
        if (result.HasOwnProp("invalidIndex")) {
            SelectMaintenanceRecommendationRow(result.invalidIndex, false)
        }
        if (result.HasOwnProp("focusField")) {
            FocusMaintenanceRecommendationField(result.focusField)
        }
        return
    }

    selectPlanId := (result.addedPlans.Length > 0) ? result.addedPlans[1].id : ""
    CloseVehicleMaintenanceRecommendationDialog()
    RefreshMaintenanceDependentState(result.vehicle.id)
    PopulateVehicleMaintenanceList(selectPlanId, true)
    AppMsgBox(BuildVehicleMaintenanceRecommendationResultText(result), AppTitle, 0x40)
}

FocusMaintenanceRecommendationField(fieldName) {
    global MaintenanceRecommendControls

    if IsObject(MaintenanceRecommendControls) && MaintenanceRecommendControls.Has(fieldName) {
        MaintenanceRecommendControls.%fieldName%.Focus()
    }
}

BuildVehicleMaintenanceRecommendationDrafts(templates) {
    drafts := []

    for template in templates {
        drafts.Push({
            title: template.title,
            intervalKm: template.intervalKm,
            intervalMonths: template.intervalMonths,
            note: template.note,
            selected: 1
        })
    }

    return drafts
}

ApplyVehicleMaintenanceRecommendationDrafts(vehicleId, drafts, persist := true) {
    SyncMaintenanceRecommendationSelectionsFromList()

    normalizedDrafts := []
    selectedCount := 0

    for index, draft in drafts {
        if !draft.selected {
            continue
        }

        selectedCount += 1
        normalizedDraft := ""
        errorMessage := ""
        focusField := ""
        if !TryNormalizeVehicleMaintenanceRecommendationDraft(draft, &normalizedDraft, &errorMessage, &focusField) {
            return {
                errorMessage: errorMessage,
                focusField: focusField,
                invalidIndex: index
            }
        }
        normalizedDrafts.Push(normalizedDraft)
    }

    if (selectedCount = 0) {
        return {
            errorMessage: "Vyberte alespoň jednu doporučenou šablonu, kterou chcete přidat.",
            focusField: "title",
            invalidIndex: 1
        }
    }

    applied := AddVehicleMaintenanceRecommendedTemplates(vehicleId, normalizedDrafts, persist)
    applied.selectedCount := selectedCount
    return applied
}

TryNormalizeVehicleMaintenanceRecommendationDraft(draft, &normalizedDraft, &errorMessage, &focusField) {
    title := Trim(draft.title)
    intervalKm := NormalizePositiveIntegerText(draft.intervalKm)
    intervalMonths := NormalizePositiveIntegerText(draft.intervalMonths)
    note := Trim(draft.note)

    if (title = "") {
        errorMessage := "Každá vybraná doporučená šablona musí mít vyplněný název úkonu."
        focusField := "title"
        return false
    }

    if (intervalKm = "" && intervalMonths = "") {
        errorMessage := "Každá vybraná doporučená šablona musí mít vyplněný interval kilometrů, měsíců nebo obojí."
        focusField := "intervalKm"
        return false
    }

    if (Trim(draft.intervalKm) != "" && intervalKm = "") {
        errorMessage := "Interval kilometrů u doporučené šablony zadejte jako kladné celé číslo."
        focusField := "intervalKm"
        return false
    }

    if (Trim(draft.intervalMonths) != "" && intervalMonths = "") {
        errorMessage := "Interval měsíců u doporučené šablony zadejte jako kladné celé číslo."
        focusField := "intervalMonths"
        return false
    }

    normalizedDraft := {
        title: title,
        intervalKm: intervalKm,
        intervalMonths: intervalMonths,
        note: note
    }
    return true
}

RunMaintenanceRecommendationSelectionInTestMode(preview, selection) {
    global AppTitle

    hooks := GetVehimapTestHooks()
    drafts := BuildVehicleMaintenanceRecommendationDrafts(preview.missing)
    RegisterMaintenanceRecommendationPreviewInHooks(preview, drafts)
    ApplyMaintenanceRecommendationSelectionHook(&drafts, selection)

    if IsObject(selection) && selection.HasOwnProp("cancel") && selection.cancel {
        return
    }

    result := ApplyVehicleMaintenanceRecommendationDrafts(preview.vehicle.id, drafts)
    if result.HasOwnProp("errorMessage") {
        AppMsgBox(result.errorMessage, AppTitle, 0x30)
        return
    }

    selectPlanId := (result.addedPlans.Length > 0) ? result.addedPlans[1].id : ""
    RefreshMaintenanceDependentState(preview.vehicle.id)
    PopulateVehicleMaintenanceList(selectPlanId, true)
    AppMsgBox(BuildVehicleMaintenanceRecommendationResultText(result), AppTitle, 0x40)
}

RegisterMaintenanceRecommendationPreviewInHooks(preview, drafts) {
    hooks := GetVehimapTestHooks()
    if !IsObject(hooks) {
        return
    }

    titles := []
    for draft in drafts {
        titles.Push(draft.title)
    }

    hooks.maintenanceRecommendationOpened := {
        vehicleId: preview.vehicle.id,
        profileLabel: preview.profileLabel,
        titles: titles
    }
}

ApplyMaintenanceRecommendationSelectionHook(&drafts, selection) {
    if !IsObject(selection) {
        return
    }

    if selection.HasOwnProp("selectedTitles") && IsObject(selection.selectedTitles) {
        wantedTitles := Map()
        for title in selection.selectedTitles {
            wantedTitles[NormalizeVehicleMaintenancePlanTitleKey(title)] := true
        }

        for draft in drafts {
            draft.selected := wantedTitles.Has(NormalizeVehicleMaintenancePlanTitleKey(draft.title))
        }
    }

    if selection.HasOwnProp("updates") && IsObject(selection.updates) {
        for update in selection.updates {
            matchKey := NormalizeVehicleMaintenancePlanTitleKey(update.HasOwnProp("matchTitle") ? update.matchTitle : "")
            if (matchKey = "") {
                continue
            }

            for draft in drafts {
                if (NormalizeVehicleMaintenancePlanTitleKey(draft.title) != matchKey) {
                    continue
                }

                if update.HasOwnProp("title") {
                    draft.title := update.title
                }
                if update.HasOwnProp("intervalKm") {
                    draft.intervalKm := update.intervalKm
                }
                if update.HasOwnProp("intervalMonths") {
                    draft.intervalMonths := update.intervalMonths
                }
                if update.HasOwnProp("note") {
                    draft.note := update.note
                }
                if update.HasOwnProp("selected") {
                    draft.selected := update.selected ? 1 : 0
                }
                break
            }
        }
    }
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
        {label: "Palivový filtr", title: "Palivový filtr", intervalKm: "30000", intervalMonths: "24", note: "Výměna palivového filtru podle provozu a doporučení výrobce."},
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

PushVehicleMaintenanceTemplateByLabel(&templates, label) {
    template := GetVehicleMaintenanceTemplateByLabel(label)
    if IsObject(template) {
        templates.Push(template)
        return true
    }

    return false
}

GetVehicleMaintenanceRecommendationPreview(vehicleId) {
    vehicle := FindVehicleById(vehicleId)
    if !IsObject(vehicle) {
        return {
            vehicle: "",
            recommended: [],
            missing: [],
            existingCount: 0,
            profileLabel: ""
        }
    }

    recommended := GetRecommendedVehicleMaintenanceTemplates(vehicle)
    existingTitles := BuildVehicleMaintenancePlanTitleMap(vehicleId)
    missing := []
    existingCount := 0

    for template in recommended {
        key := NormalizeVehicleMaintenancePlanTitleKey(template.title)
        if (key != "" && existingTitles.Has(key)) {
            existingCount += 1
            continue
        }
        missing.Push(template)
    }

    return {
        vehicle: vehicle,
        recommended: recommended,
        missing: missing,
        existingCount: existingCount,
        profileLabel: BuildVehicleMaintenanceRecommendationProfileLabel(vehicle)
    }
}

GetRecommendedVehicleMaintenanceTemplates(vehicle) {
    category := NormalizeCategory(vehicle.category)
    labels := []
    powertrain := GetVehicleMaintenancePowertrain(vehicle)
    isElectric := (powertrain = "Elektro")
    isDiesel := (powertrain = "Nafta")
    hasClimate := ShouldRecommendVehicleClimateMaintenance(vehicle)
    shouldTiming := ShouldRecommendVehicleTimingMaintenance(vehicle)
    shouldTransmission := ShouldRecommendVehicleTransmissionMaintenance(vehicle)

    switch category {
        case "Osobní vozidla":
            if isElectric {
                labels := ["Kabinový filtr", "Brzdová kapalina", "Chladicí kapalina"]
            } else {
                labels := ["Motorový olej a filtr", "Vzduchový filtr", "Kabinový filtr", "Brzdová kapalina", "Chladicí kapalina"]
                if shouldTiming {
                    labels.Push("Rozvody")
                }
                if shouldTransmission {
                    labels.Push("Převodový olej")
                }
            }
        case "Motocykly":
            labels := isElectric ? ["Brzdová kapalina"] : ["Motorový olej a filtr", "Vzduchový filtr", "Brzdová kapalina", "Chladicí kapalina"]
        case "Nákladní vozidla":
            if isElectric {
                labels := ["Brzdová kapalina", "Chladicí kapalina"]
            } else {
                labels := ["Motorový olej a filtr", "Vzduchový filtr", "Brzdová kapalina", "Chladicí kapalina"]
                if shouldTransmission {
                    labels.Push("Převodový olej")
                }
            }
        case "Autobusy":
            if isElectric {
                labels := ["Brzdová kapalina", "Chladicí kapalina"]
            } else {
                labels := ["Motorový olej a filtr", "Vzduchový filtr", "Brzdová kapalina", "Chladicí kapalina"]
                if shouldTransmission {
                    labels.Push("Převodový olej")
                }
            }
        default:
            labels := isElectric ? ["Brzdová kapalina"] : ["Brzdová kapalina", "Chladicí kapalina"]
    }

    if hasClimate {
        labels.Push("Klimatizace a dezinfekce")
    }

    if (isDiesel && !isElectric) {
        labels.Push("Palivový filtr")
    }

    templates := []
    for label in labels {
        PushVehicleMaintenanceTemplateByLabel(&templates, label)
    }

    return templates
}
AddVehicleMaintenanceRecommendedTemplates(vehicleId, templates, persist := true) {
    global VehicleMaintenancePlans

    vehicle := FindVehicleById(vehicleId)
    addedPlans := []
    existingTitles := BuildVehicleMaintenancePlanTitleMap(vehicleId)
    skippedCount := 0

    for template in templates {
        key := NormalizeVehicleMaintenancePlanTitleKey(template.title)
        if (key != "" && existingTitles.Has(key)) {
            skippedCount += 1
            continue
        }

        plan := {
            id: GenerateVehicleMaintenancePlanId(),
            vehicleId: vehicleId,
            title: template.title,
            intervalKm: template.intervalKm,
            intervalMonths: template.intervalMonths,
            lastServiceDate: "",
            lastServiceOdometer: "",
            isActive: 1,
            note: template.note
        }
        VehicleMaintenancePlans.Push(plan)
        addedPlans.Push(plan)
        if (key != "") {
            existingTitles[key] := true
        }
    }

    if (persist && addedPlans.Length > 0) {
        SaveVehicleMaintenancePlans()
    }

    return {
        vehicle: vehicle,
        addedPlans: addedPlans,
        skippedCount: skippedCount
    }
}

BuildVehicleMaintenancePlanTitleMap(vehicleId) {
    titles := Map()

    for plan in GetVehicleMaintenancePlans(vehicleId, true) {
        key := NormalizeVehicleMaintenancePlanTitleKey(plan.title)
        if (key != "") {
            titles[key] := true
        }
    }

    return titles
}

NormalizeVehicleMaintenancePlanTitleKey(title) {
    return StrLower(Trim(title))
}

BuildVehicleMaintenanceRecommendationPrompt(preview) {
    text := "Vehimap pro vozidlo " preview.vehicle.name " doporučuje přidat tyto servisní šablony:`n`n"

    for template in preview.missing {
        intervalText := BuildVehicleMaintenanceIntervalText(template)
        text .= "- " template.title
        if (intervalText != "Nevyplněno") {
            text .= " (" intervalText ")"
        }
        text .= "`n"
    }

    text .= "`nProfil doporučení: " preview.profileLabel "."
    if (preview.existingCount > 0) {
        text .= "`nUž založených doporučených plánů: " preview.existingCount "."
    }
    text .= "`n`nPřidat teď chybějící plány?"
    return text
}

BuildVehicleMaintenanceRecommendationResultText(result) {
    addedCount := result.addedPlans.Length
    text := "Přidáno doporučených plánů: " addedCount "."
    if (result.skippedCount > 0) {
        text .= "`nUž existovalo: " result.skippedCount "."
    }
    if IsObject(result.vehicle) {
        text .= "`nVozidlo: " result.vehicle.name "."
    }
    return text
}

BuildVehicleMaintenanceRecommendationProfileLabel(vehicle) {
    meta := GetVehicleMeta(vehicle.id)
    parts := [NormalizeCategory(vehicle.category)]
    powertrain := GetVehicleMaintenancePowertrain(vehicle)
    if (powertrain != "") {
        parts.Push(BuildVehicleMaintenancePowertrainLabel(powertrain))
    }
    if (meta.climateProfile != "") {
        parts.Push(StrLower(meta.climateProfile))
    }
    if (meta.timingDrive != "") {
        parts.Push(BuildVehicleMaintenanceTimingLabel(meta.timingDrive))
    }
    if (meta.transmission != "") {
        parts.Push(BuildVehicleMaintenanceTransmissionLabel(meta.transmission))
    }

    return JoinInline(parts, ", ")
}

BuildVehicleMaintenancePowertrainLabel(powertrain) {
    switch powertrain {
        case "Benzín":
            return "benzínový pohon"
        case "Nafta":
            return "naftový pohon"
        case "Hybrid":
            return "hybridní pohon"
        case "Plug-in hybrid":
            return "plug-in hybrid"
        case "Elektro":
            return "elektrický pohon"
        case "LPG / CNG":
            return "pohon LPG / CNG"
        case "Jiné":
            return "jiný pohon"
    }

    return StrLower(powertrain)
}

BuildVehicleMaintenanceTimingLabel(timingDrive) {
    switch timingDrive {
        case "Řemen":
            return "rozvody řemenem"
        case "Řetěz":
            return "rozvody řetězem"
        case "Není relevantní":
            return "bez pravidelných rozvodů"
    }

    return "rozvody: " StrLower(timingDrive)
}

BuildVehicleMaintenanceTransmissionLabel(transmission) {
    switch transmission {
        case "Manuální":
            return "manuální převodovka"
        case "Automatická":
            return "automatická převodovka"
        case "Není relevantní":
            return "bez klasické převodovky"
    }

    return "převodovka: " StrLower(transmission)
}

GetVehicleMaintenancePowertrain(vehicle) {
    meta := GetVehicleMeta(vehicle.id)
    if (meta.powertrain != "") {
        return meta.powertrain
    }

    if VehicleMaintenanceProfileHasKeyword(vehicle, ["plug-in hybrid", "plug in hybrid", "phev"]) {
        return "Plug-in hybrid"
    }
    if VehicleMaintenanceProfileHasKeyword(vehicle, ["hybrid", "hev"]) {
        return "Hybrid"
    }
    if VehicleMaintenanceProfileHasKeyword(vehicle, ["elektro", "elektř", "electric", "bev", "tesla", "ev"]) {
        return "Elektro"
    }
    if VehicleMaintenanceProfileHasKeyword(vehicle, ["diesel", "nafta", "tdi", "hdi", "dci", "cdi", "crdi", "multijet", "tdci"]) {
        return "Nafta"
    }
    if VehicleMaintenanceProfileHasKeyword(vehicle, ["lpg", "cng", "gpl"]) {
        return "LPG / CNG"
    }
    if VehicleMaintenanceProfileHasKeyword(vehicle, ["benzin", "benzín", "gasoline", "tsi", "tfsi", "mpi", "gdi", "fsi", "ecoboost"]) {
        return "Benzín"
    }

    return ""
}

IsDieselVehicleMaintenanceProfile(vehicle) {
    return GetVehicleMaintenancePowertrain(vehicle) = "Nafta"
}

IsElectricVehicleMaintenanceProfile(vehicle) {
    return GetVehicleMaintenancePowertrain(vehicle) = "Elektro"
}

ShouldRecommendVehicleClimateMaintenance(vehicle) {
    meta := GetVehicleMeta(vehicle.id)
    if (meta.climateProfile = "Má klimatizaci") {
        return true
    }
    if (meta.climateProfile = "Bez klimatizace") {
        return false
    }

    category := NormalizeCategory(vehicle.category)
    return (category = "Osobní vozidla" || category = "Nákladní vozidla" || category = "Autobusy")
}

ShouldRecommendVehicleTimingMaintenance(vehicle) {
    if IsElectricVehicleMaintenanceProfile(vehicle) {
        return false
    }

    meta := GetVehicleMeta(vehicle.id)
    if (meta.timingDrive = "Řemen") {
        return true
    }
    if (meta.timingDrive = "Řetěz" || meta.timingDrive = "Není relevantní") {
        return false
    }

    return true
}

ShouldRecommendVehicleTransmissionMaintenance(vehicle) {
    if IsElectricVehicleMaintenanceProfile(vehicle) {
        return false
    }

    meta := GetVehicleMeta(vehicle.id)
    if (meta.transmission = "Automatická") {
        return true
    }
    if (meta.transmission = "Manuální" || meta.transmission = "Není relevantní") {
        return false
    }

    return true
}

VehicleMaintenanceProfileHasKeyword(vehicle, keywords) {
    haystack := StrLower(
        NormalizeCategory(vehicle.category) " "
        vehicle.makeModel " "
        vehicle.vehicleNote
    )

    for keyword in keywords {
        if InStr(haystack, StrLower(keyword)) {
            return true
        }
    }

    return false
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
