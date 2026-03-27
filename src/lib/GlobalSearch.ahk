OpenGlobalSearchDialog(*) {
    global AppTitle, MainGui, FormGui, SettingsGui, DashboardGui, OverviewGui, OverdueGui, GlobalSearchGui, GlobalSearchList, GlobalSearchResults, GlobalSearchSummaryLabel, GlobalSearchSearchCtrl, GlobalSearchOpenButton, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui, FleetCostGui

    if IsObject(GlobalSearchGui) {
        WinActivate("ahk_id " GlobalSearchGui.Hwnd)
        return
    }

    for guiRef in [DashboardGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui, FleetCostGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    ShowMainWindow()

    GlobalSearchResults := []
    GlobalSearchGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Globální hledání")
    GlobalSearchGui.SetFont("s10", "Segoe UI")
    GlobalSearchGui.OnEvent("Close", CloseGlobalSearchDialog)
    GlobalSearchGui.OnEvent("Escape", CloseGlobalSearchDialog)

    MainGui.Opt("+Disabled")

    GlobalSearchGui.AddText("x20 y20 w1000", "Zde můžete hledat napříč názvy vozidel, historií událostí, kilometry a tankováním, pojištěním a doklady, vlastními připomínkami i plány údržby.")
    GlobalSearchGui.AddText("x20 y55 w210", "Hledat napříč celým Vehimapem")
    GlobalSearchSearchCtrl := GlobalSearchGui.AddEdit("x240 y52 w430")
    GlobalSearchSearchCtrl.OnEvent("Change", OnGlobalSearchChanged)

    GlobalSearchSummaryLabel := GlobalSearchGui.AddText("x20 y86 w1000", "")

    GlobalSearchList := GlobalSearchGui.AddListView("x20 y112 w1000 h255 Grid -Multi", ["Typ", "Vozidlo", "SPZ", "Položka", "Detail", "Datum / platnost"])
    GlobalSearchList.OnEvent("DoubleClick", OpenSelectedGlobalSearchResult)
    GlobalSearchList.ModifyCol(1, "90")
    GlobalSearchList.ModifyCol(2, "175")
    GlobalSearchList.ModifyCol(3, "95")
    GlobalSearchList.ModifyCol(4, "205")
    GlobalSearchList.ModifyCol(5, "340")
    GlobalSearchList.ModifyCol(6, "95")

    GlobalSearchOpenButton := GlobalSearchGui.AddButton("x650 y382 w160 h30", "Otevřít výsledek")
    GlobalSearchOpenButton.OnEvent("Click", OpenSelectedGlobalSearchResult)
    GlobalSearchOpenButton.Opt("+Disabled")

    closeButton := GlobalSearchGui.AddButton("x820 y382 w110 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseGlobalSearchDialog)

    GlobalSearchGui.Show("w1040 h432")
    PopulateGlobalSearchList()
    GlobalSearchSearchCtrl.Focus()
}

CloseGlobalSearchDialog(*) {
    global GlobalSearchGui, GlobalSearchList, GlobalSearchResults, GlobalSearchSummaryLabel, GlobalSearchSearchCtrl, GlobalSearchOpenButton, MainGui

    if IsObject(GlobalSearchGui) {
        GlobalSearchGui.Destroy()
        GlobalSearchGui := 0
    }

    GlobalSearchList := 0
    GlobalSearchResults := []
    GlobalSearchSummaryLabel := 0
    GlobalSearchSearchCtrl := 0
    GlobalSearchOpenButton := 0
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

OnGlobalSearchChanged(*) {
    selectedKey := GetSelectedGlobalSearchResultKey()
    PopulateGlobalSearchList(selectedKey)
}

PopulateGlobalSearchList(selectedKey := "", focusList := false) {
    global GlobalSearchList, GlobalSearchResults, GlobalSearchSummaryLabel, GlobalSearchOpenButton

    if !IsObject(GlobalSearchList) {
        return
    }

    searchText := GetGlobalSearchText()
    GlobalSearchResults := BuildGlobalSearchResults(searchText)

    if IsObject(GlobalSearchSummaryLabel) {
        GlobalSearchSummaryLabel.Text := BuildGlobalSearchSummaryText(GlobalSearchResults, searchText)
    }

    GlobalSearchList.Opt("-Redraw")
    GlobalSearchList.Delete()

    selectedRow := 0
    for result in GlobalSearchResults {
        row := GlobalSearchList.Add(
            "",
            result.kindLabel,
            result.vehicle.name,
            result.vehicle.plate,
            result.itemText,
            ShortenText(result.detailText, 78),
            result.dateText
        )
        if (selectedKey != "" && BuildGlobalSearchResultKey(result) = selectedKey) {
            selectedRow := row
        }
    }

    GlobalSearchList.Opt("+Redraw")

    if IsObject(GlobalSearchOpenButton) {
        GlobalSearchOpenButton.Opt(GlobalSearchResults.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if (GlobalSearchResults.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    mode := focusList ? "Select Focus Vis" : "Select Vis"
    GlobalSearchList.Modify(selectedRow, mode)
}

GetGlobalSearchText() {
    global GlobalSearchSearchCtrl

    if !IsObject(GlobalSearchSearchCtrl) {
        return ""
    }

    return Trim(GlobalSearchSearchCtrl.Text)
}

BuildGlobalSearchResults(searchText := "") {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords, VehicleReminders, VehicleMaintenancePlans

    results := []
    needle := StrLower(Trim(searchText))
    if (needle = "") {
        return results
    }

    vehicleMap := Map()
    for vehicle in Vehicles {
        vehicleMap[vehicle.id] := vehicle
    }

    for vehicle in Vehicles {
        meta := GetVehicleMeta(vehicle.id)
        AddGlobalSearchResultIfMatch(
            &results,
            needle,
            "vehicle",
            "Vozidlo",
            vehicle,
            vehicle.id,
            BuildGlobalSearchVehicleItemText(vehicle),
            BuildGlobalSearchVehicleDetailText(vehicle, meta),
            BuildGlobalSearchVehicleDateText(vehicle),
            [vehicle.name, vehicle.vehicleType, vehicle.makeModel, vehicle.plate, vehicle.year, vehicle.power, vehicle.category, meta.state, meta.tags, vehicle.lastTk, vehicle.nextTk, vehicle.greenCardFrom, vehicle.greenCardTo, GetVehicleStatusText(vehicle)]
        )
    }

    for event in VehicleHistory {
        if !vehicleMap.Has(event.vehicleId) {
            continue
        }
        vehicle := vehicleMap[event.vehicleId]
        AddGlobalSearchResultIfMatch(
            &results,
            needle,
            "history",
            "Historie",
            vehicle,
            event.id,
            Trim(event.eventType) != "" ? event.eventType : "Událost",
            BuildGlobalSearchHistoryDetailText(event),
            event.eventDate,
            [vehicle.name, vehicle.plate, vehicle.makeModel, event.eventDate, event.eventType, event.odometer, event.cost, event.note]
        )
    }

    for entry in VehicleFuelLog {
        if !vehicleMap.Has(entry.vehicleId) {
            continue
        }
        vehicle := vehicleMap[entry.vehicleId]
        AddGlobalSearchResultIfMatch(
            &results,
            needle,
            "fuel",
            "Tankování",
            vehicle,
            entry.id,
            BuildGlobalSearchFuelItemText(entry),
            BuildGlobalSearchFuelDetailText(entry),
            entry.entryDate,
            [vehicle.name, vehicle.plate, vehicle.makeModel, entry.entryDate, entry.odometer, entry.liters, entry.totalCost, entry.fullTank ? "ano" : "ne", entry.fuelType, entry.note]
        )
    }

    for entry in VehicleRecords {
        if !vehicleMap.Has(entry.vehicleId) {
            continue
        }
        vehicle := vehicleMap[entry.vehicleId]
        AddGlobalSearchResultIfMatch(
            &results,
            needle,
            "record",
            "Doklad",
            vehicle,
            entry.id,
            Trim(entry.title) != "" ? entry.title : entry.recordType,
            BuildGlobalSearchRecordDetailText(entry),
            Trim(entry.validTo) != "" ? entry.validTo : entry.validFrom,
            [vehicle.name, vehicle.plate, vehicle.makeModel, entry.recordType, entry.title, entry.provider, entry.validFrom, entry.validTo, entry.price, entry.filePath, GetFileNameFromPath(entry.filePath), entry.note]
        )
    }

    for entry in VehicleReminders {
        if !vehicleMap.Has(entry.vehicleId) {
            continue
        }
        vehicle := vehicleMap[entry.vehicleId]
        AddGlobalSearchResultIfMatch(
            &results,
            needle,
            "reminder",
            "Připomínka",
            vehicle,
            entry.id,
            Trim(entry.title) != "" ? entry.title : "Připomínka",
            BuildGlobalSearchReminderDetailText(entry),
            entry.dueDate,
            [vehicle.name, vehicle.plate, vehicle.makeModel, entry.title, entry.dueDate, entry.reminderDays, GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : ""), GetReminderExpirationStatusText(entry.dueDate, entry.reminderDays + 0), entry.note]
        )
    }

    for entry in VehicleMaintenancePlans {
        if !vehicleMap.Has(entry.vehicleId) {
            continue
        }
        vehicle := vehicleMap[entry.vehicleId]
        snapshot := BuildVehicleMaintenancePlanSnapshot(entry, vehicle)
        AddGlobalSearchResultIfMatch(
            &results,
            needle,
            "maintenance",
            "Údržba",
            vehicle,
            entry.id,
            Trim(entry.title) != "" ? entry.title : "Plán údržby",
            BuildGlobalSearchMaintenanceDetailText(snapshot),
            snapshot.nextServiceText,
            [vehicle.name, vehicle.plate, vehicle.makeModel, entry.title, snapshot.intervalText, snapshot.lastServiceText, snapshot.nextServiceText, snapshot.statusText, entry.note]
        )
    }

    SortGlobalSearchResults(&results)
    return results
}

AddGlobalSearchResultIfMatch(&results, needle, kind, kindLabel, vehicle, itemId, itemText, detailText, dateText, searchTexts) {
    matchRank := GetSearchTextMatchRank(needle, searchTexts)
    if (matchRank >= 1000000) {
        return
    }

    results.Push({
        kind: kind,
        kindLabel: kindLabel,
        vehicle: vehicle,
        itemId: itemId,
        itemText: itemText,
        detailText: detailText,
        dateText: dateText,
        matchRank: matchRank
    })
}

GetSearchTextMatchRank(needle, texts) {
    if (needle = "") {
        return 1000000
    }

    bestRank := 1000000
    for text in texts {
        haystack := StrLower(Trim(text))
        if (haystack = "") {
            continue
        }

        if (haystack = needle) {
            return 0
        }

        position := InStr(haystack, needle)
        if !position {
            continue
        }

        if (position = 1) {
            rank := 100 + StrLen(haystack)
        } else {
            rank := 1000 + position + StrLen(haystack)
        }

        if (rank < bestRank) {
            bestRank := rank
        }
    }

    return bestRank
}

SortGlobalSearchResults(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareGlobalSearchResults(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareGlobalSearchResults(left, right) {
    result := CompareNumberValues(left.matchRank, right.matchRank)
    if (result != 0) {
        return result
    }

    result := CompareNumberValues(GetGlobalSearchKindPriority(left.kind), GetGlobalSearchKindPriority(right.kind))
    if (result != 0) {
        return result
    }

    result := CompareTextValues(left.vehicle.name, right.vehicle.name)
    if (result != 0) {
        return result
    }

    result := CompareTextValues(left.vehicle.plate, right.vehicle.plate)
    if (result != 0) {
        return result
    }

    result := CompareTextValues(left.itemText, right.itemText)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.itemId, right.itemId)
}

GetGlobalSearchKindPriority(kind) {
    switch kind {
        case "vehicle":
            return 1
        case "reminder":
            return 2
        case "record":
            return 3
        case "history":
            return 4
        case "fuel":
            return 5
        case "maintenance":
            return 6
    }

    return 99
}

BuildGlobalSearchSummaryText(results, searchText := "") {
    needle := Trim(searchText)
    if (needle = "") {
        return "Zadejte hledaný text. Vehimap bude prohledávat vozidla, historii, tankování, doklady, připomínky i údržbu."
    }

    if (results.Length = 0) {
        return "Pro hledání " needle " nebyl nalezen žádný výsledek."
    }

    vehicleCount := 0
    historyCount := 0
    fuelCount := 0
    recordCount := 0
    reminderCount := 0
    maintenanceCount := 0

    for result in results {
        switch result.kind {
            case "vehicle":
                vehicleCount += 1
            case "history":
                historyCount += 1
            case "fuel":
                fuelCount += 1
            case "record":
                recordCount += 1
            case "reminder":
                reminderCount += 1
            case "maintenance":
                maintenanceCount += 1
        }
    }

    return "Nalezeno výsledků: " results.Length ". Vozidla: " vehicleCount ". Historie: " historyCount ". Tankování: " fuelCount ". Doklady: " recordCount ". Připomínky: " reminderCount ". Údržba: " maintenanceCount "."
}

BuildGlobalSearchResultKey(result) {
    return result.kind "|" result.vehicle.id "|" result.itemId
}

GetSelectedGlobalSearchResultKey() {
    global GlobalSearchList, GlobalSearchResults

    if !IsObject(GlobalSearchList) {
        return ""
    }

    row := GlobalSearchList.GetNext(0)
    if !row || row > GlobalSearchResults.Length {
        return ""
    }

    return BuildGlobalSearchResultKey(GlobalSearchResults[row])
}

GetSelectedGlobalSearchResult(actionLabel := "otevřít") {
    global AppTitle, GlobalSearchList, GlobalSearchResults

    if !IsObject(GlobalSearchList) {
        return ""
    }

    row := GlobalSearchList.GetNext(0)
    if !row || row > GlobalSearchResults.Length {
        MsgBox("Nejprve vyberte výsledek, který chcete " actionLabel ".", AppTitle, 0x40)
        return ""
    }

    return GlobalSearchResults[row]
}

OpenSelectedGlobalSearchResult(*) {
    result := GetSelectedGlobalSearchResult("otevřít")
    if !IsObject(result) {
        return
    }

    CloseGlobalSearchDialog()

    switch result.kind {
        case "vehicle":
            OpenVehicleById(result.vehicle.id, true)
        case "history":
            OpenVehicleHistoryDialog(result.vehicle, false, result.itemId)
        case "fuel":
            OpenVehicleFuelDialog(result.vehicle, false, result.itemId)
        case "record":
            OpenVehicleRecordsDialog(result.vehicle, false, result.itemId)
        case "reminder":
            OpenVehicleReminderDialog(result.vehicle, false, result.itemId)
        case "maintenance":
            OpenVehicleMaintenanceDialog(result.vehicle, false, result.itemId)
    }
}

BuildGlobalSearchVehicleItemText(vehicle) {
    if (Trim(vehicle.makeModel) != "") {
        return vehicle.makeModel
    }
    if (Trim(vehicle.vehicleType) != "") {
        return vehicle.vehicleType
    }
    return "Detail vozidla"
}

BuildGlobalSearchVehicleDetailText(vehicle, meta) {
    parts := []
    if (Trim(vehicle.category) != "") {
        parts.Push(vehicle.category)
    }
    if (Trim(meta.state) != "") {
        parts.Push("Stav: " meta.state)
    }
    if (Trim(meta.tags) != "") {
        parts.Push("Štítky: " meta.tags)
    }

    statusText := GetVehicleStatusText(vehicle)
    if (statusText != "") {
        parts.Push(statusText)
    }

    return JoinInline(parts, " | ")
}

BuildGlobalSearchVehicleDateText(vehicle) {
    parts := []
    if (Trim(vehicle.nextTk) != "") {
        parts.Push("TK " vehicle.nextTk)
    }
    if (Trim(vehicle.greenCardTo) != "") {
        parts.Push("ZK " vehicle.greenCardTo)
    }
    return JoinInline(parts, " | ")
}

BuildGlobalSearchHistoryDetailText(event) {
    parts := []
    if (Trim(event.odometer) != "") {
        parts.Push("Km " FormatHistoryOdometer(event.odometer))
    }
    if (Trim(event.cost) != "") {
        parts.Push("Cena " event.cost)
    }
    if (Trim(event.note) != "") {
        parts.Push(ShortenText(event.note, 55))
    }
    return JoinInline(parts, " | ")
}

BuildGlobalSearchFuelItemText(entry) {
    if (Trim(entry.liters) != "" || Trim(entry.totalCost) != "") {
        text := "Tankování"
        if (Trim(entry.fuelType) != "") {
            text .= " - " entry.fuelType
        }
        return text
    }

    return "Stav tachometru"
}

BuildGlobalSearchFuelDetailText(entry) {
    parts := []
    if (Trim(entry.odometer) != "") {
        parts.Push("Km " FormatHistoryOdometer(entry.odometer))
    }
    if (Trim(entry.liters) != "") {
        parts.Push(FormatFuelLiters(entry.liters))
    }
    if (Trim(entry.totalCost) != "") {
        parts.Push(FormatFuelMoney(entry.totalCost))
    }
    if entry.fullTank {
        parts.Push("Plná nádrž")
    }
    if (Trim(entry.note) != "") {
        parts.Push(ShortenText(entry.note, 55))
    }
    return JoinInline(parts, " | ")
}

BuildGlobalSearchRecordDetailText(entry) {
    parts := []
    if (Trim(entry.recordType) != "") {
        parts.Push(entry.recordType)
    }
    if (Trim(entry.provider) != "") {
        parts.Push(entry.provider)
    }

    fileName := GetFileNameFromPath(entry.filePath)
    if (fileName != "") {
        parts.Push(fileName)
    }

    if (Trim(entry.note) != "") {
        parts.Push(ShortenText(entry.note, 50))
    }

    return JoinInline(parts, " | ")
}

BuildGlobalSearchReminderDetailText(entry) {
    parts := []

    repeatLabel := GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : "")
    if (repeatLabel != "" && repeatLabel != "Neopakovat") {
        parts.Push(repeatLabel)
    }

    statusText := GetReminderExpirationStatusText(entry.dueDate, entry.reminderDays + 0)
    if (statusText != "") {
        parts.Push(statusText)
    }

    if (Trim(entry.note) != "") {
        parts.Push(ShortenText(entry.note, 50))
    }

    return JoinInline(parts, " | ")
}

BuildGlobalSearchMaintenanceDetailText(snapshot) {
    parts := []
    if (Trim(snapshot.intervalText) != "") {
        parts.Push(snapshot.intervalText)
    }
    if (Trim(snapshot.statusText) != "") {
        parts.Push(snapshot.statusText)
    }
    if (Trim(snapshot.plan.note) != "") {
        parts.Push(ShortenText(snapshot.plan.note, 50))
    }

    return JoinInline(parts, " | ")
}
