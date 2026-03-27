OpenDashboard(*) {
    OpenDashboardDialog(false)
}

OpenStartupDashboard() {
    OpenDashboardDialog(true)
}

RefreshDashboardShortcut() {
    PopulateDashboardList(true)
}

OpenGlobalSearchFromDashboard(*) {
    CloseDashboardDialog()
    OpenGlobalSearchDialog()
}

OpenFleetCostsFromDashboard(*) {
    CloseDashboardDialog()
    OpenFleetCostOverviewDialog()
}

OpenDashboardDialog(showMainOnClose := false) {
    global AppTitle, MainGui, FormGui, SettingsGui, DashboardGui, DashboardSummaryVehiclesLabel, DashboardSummaryTermsLabel, DashboardSummaryCostsLabel, DashboardSummaryDataLabel, DashboardList, DashboardEntries, DashboardOpenButton, DashboardItemButton, DashboardVehicleCostsButton, DashboardEditButton, DashboardShowOnLaunchCtrl, DashboardShowMainOnClose
    global OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui, FleetCostGui

    if IsObject(DashboardGui) {
        WinActivate("ahk_id " DashboardGui.Hwnd)
        return
    }

    for guiRef in [FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui, FleetCostGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    mainWasVisible := DllCall("IsWindowVisible", "ptr", MainGui.Hwnd, "int") != 0
    DashboardShowMainOnClose := showMainOnClose || mainWasVisible

    DashboardEntries := []
    DashboardGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Dashboard")
    DashboardGui.SetFont("s10", "Segoe UI")
    DashboardGui.OnEvent("Close", CloseDashboardDialog)
    DashboardGui.OnEvent("Escape", CloseDashboardDialog)

    MainGui.Opt("+Disabled")

    DashboardGui.AddText("x20 y20 w980", "Dashboard nabízí rychlý přehled vozidel, termínů, nákladů i kvality evidencí, které teď stojí za pozornost nebo doplnění.")

    DashboardGui.AddGroupBox("x20 y50 w980 h70", "Vozidla")
    DashboardSummaryVehiclesLabel := DashboardGui.AddText("x35 y74 w950 h34", "")

    DashboardGui.AddGroupBox("x20 y125 w980 h70", "Termíny")
    DashboardSummaryTermsLabel := DashboardGui.AddText("x35 y149 w950 h34", "")

    DashboardGui.AddGroupBox("x20 y200 w980 h70", "Náklady")
    DashboardSummaryCostsLabel := DashboardGui.AddText("x35 y224 w950 h34", "")

    DashboardGui.AddGroupBox("x20 y275 w980 h70", "Evidence")
    DashboardSummaryDataLabel := DashboardGui.AddText("x35 y299 w950 h34", "")

    DashboardGui.AddGroupBox("x20 y350 w980 h185", "Položky k řešení a datové nedostatky")
    DashboardList := DashboardGui.AddListView("x35 y375 w950 h145 Grid -Multi", ["Druh", "Vozidlo", "Kategorie", "SPZ", "Položka / termín", "Stav"])
    DashboardList.OnEvent("DoubleClick", OpenSelectedDashboardItem)
    DashboardList.ModifyCol(1, "155")
    DashboardList.ModifyCol(2, "190")
    DashboardList.ModifyCol(3, "150")
    DashboardList.ModifyCol(4, "95")
    DashboardList.ModifyCol(5, "210")
    DashboardList.ModifyCol(6, "145")

    DashboardGui.AddGroupBox("x20 y545 w980 h145", "Akce")

    overviewButton := DashboardGui.AddButton("x40 y573 w135 h30", "Přehled termínů")
    overviewButton.OnEvent("Click", OpenOverviewFromDashboard)

    overdueButton := DashboardGui.AddButton("x185 y573 w135 h30", "Propadlé termíny")
    overdueButton.OnEvent("Click", OpenOverdueFromDashboard)

    searchButton := DashboardGui.AddButton("x330 y573 w135 h30", "Globální hledání")
    searchButton.OnEvent("Click", OpenGlobalSearchFromDashboard)

    fleetCostsButton := DashboardGui.AddButton("x475 y573 w135 h30", "Souhrn nákladů")
    fleetCostsButton.OnEvent("Click", OpenFleetCostsFromDashboard)

    DashboardItemButton := DashboardGui.AddButton("x40 y608 w135 h30", "Otevřít položku")
    DashboardItemButton.OnEvent("Click", OpenSelectedDashboardItem)

    DashboardVehicleCostsButton := DashboardGui.AddButton("x185 y608 w135 h30", "Náklady vozidla")
    DashboardVehicleCostsButton.OnEvent("Click", OpenSelectedDashboardVehicleCosts)

    DashboardEditButton := DashboardGui.AddButton("x330 y608 w135 h30", "Upravit vozidlo")
    DashboardEditButton.OnEvent("Click", EditSelectedDashboardVehicle)

    DashboardOpenButton := DashboardGui.AddButton("x475 y608 w135 h30 Default", "Zobrazit vozidlo")
    DashboardOpenButton.OnEvent("Click", OpenSelectedDashboardVehicle)

    closeButton := DashboardGui.AddButton("x620 y608 w110 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseDashboardDialog)

    DashboardShowOnLaunchCtrl := DashboardGui.AddCheckBox("x40 y650 w320", "Zobrazovat dashboard při startu")
    DashboardShowOnLaunchCtrl.Value := GetShowDashboardOnLaunchEnabled()
    DashboardShowOnLaunchCtrl.OnEvent("Click", OnDashboardShowOnLaunchChanged)

    DashboardGui.Show("w1020 h705")
    PopulateDashboardList(true)
    if (DashboardEntries.Length = 0) {
        closeButton.Focus()
    }
}

CloseDashboardDialog(*) {
    global DashboardGui, DashboardSummaryVehiclesLabel, DashboardSummaryTermsLabel, DashboardSummaryCostsLabel, DashboardSummaryDataLabel, DashboardList, DashboardEntries, DashboardOpenButton, DashboardItemButton, DashboardVehicleCostsButton, DashboardEditButton, DashboardShowOnLaunchCtrl, DashboardShowMainOnClose, MainGui

    if IsObject(DashboardGui) {
        DashboardGui.Destroy()
        DashboardGui := 0
    }

    DashboardSummaryVehiclesLabel := 0
    DashboardSummaryTermsLabel := 0
    DashboardSummaryCostsLabel := 0
    DashboardSummaryDataLabel := 0
    DashboardList := 0
    DashboardEntries := []
    DashboardOpenButton := 0
    DashboardItemButton := 0
    DashboardVehicleCostsButton := 0
    DashboardEditButton := 0
    DashboardShowOnLaunchCtrl := 0

    MainGui.Opt("-Disabled")
    if DashboardShowMainOnClose {
        ShowMainWindow()
    }
    DashboardShowMainOnClose := false
}

OnDashboardShowOnLaunchChanged(ctrl, *) {
    global SettingsFile

    IniWrite(ctrl.Value ? 1 : 0, SettingsFile, "app", "show_dashboard_on_launch")
}

PopulateDashboardList(focusList := false) {
    global DashboardEntries, DashboardList, DashboardSummaryVehiclesLabel, DashboardSummaryTermsLabel, DashboardSummaryCostsLabel, DashboardSummaryDataLabel, DashboardOpenButton, DashboardItemButton, DashboardVehicleCostsButton, DashboardEditButton

    if !IsObject(DashboardList) {
        return
    }

    DashboardEntries := BuildDashboardEntries()
    SortDashboardEntries(&DashboardEntries)

    if IsObject(DashboardSummaryVehiclesLabel) {
        DashboardSummaryVehiclesLabel.Text := BuildDashboardVehicleSummaryText(DashboardEntries)
    }

    if IsObject(DashboardSummaryTermsLabel) {
        DashboardSummaryTermsLabel.Text := BuildDashboardTermSummaryText()
    }

    if IsObject(DashboardSummaryCostsLabel) {
        DashboardSummaryCostsLabel.Text := BuildDashboardCostSummaryText()
    }

    if IsObject(DashboardSummaryDataLabel) {
        DashboardSummaryDataLabel.Text := BuildDashboardDataSummaryText()
    }

    DashboardList.Opt("-Redraw")
    DashboardList.Delete()

    for entry in DashboardEntries {
        plateText := Trim(entry.vehicle.plate) = "" ? "-" : entry.vehicle.plate
        DashboardList.Add("", entry.kindLabel, entry.vehicle.name, entry.vehicle.category, plateText, entry.term, entry.status)
    }

    DashboardList.Opt("+Redraw")

    if IsObject(DashboardOpenButton) {
        DashboardOpenButton.Opt(DashboardEntries.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if IsObject(DashboardItemButton) {
        DashboardItemButton.Opt(DashboardEntries.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if IsObject(DashboardVehicleCostsButton) {
        DashboardVehicleCostsButton.Opt(DashboardEntries.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if IsObject(DashboardEditButton) {
        DashboardEditButton.Opt(DashboardEntries.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if (DashboardEntries.Length = 0) {
        return
    }

    DashboardList.Modify(1, focusList ? "Select Focus Vis" : "Select Vis")
}

BuildDashboardVehicleSummaryText(dashboardEntries := "") {
    global Vehicles

    archivedCount := 0
    veteranCount := 0
    parkedCount := 0

    for vehicle in Vehicles {
        meta := GetVehicleMeta(vehicle.id)
        state := NormalizeVehicleState(meta.state)

        switch state {
            case "Archiv":
                archivedCount += 1
            case "Veterán":
                veteranCount += 1
            case "Odstaveno":
                parkedCount += 1
        }
    }

    activeCount := Vehicles.Length - archivedCount
    if (activeCount < 0) {
        activeCount := 0
    }

    attentionCount := GetDashboardProblemVehicleCount(dashboardEntries)
    text := "Celkem vozidel: " Vehicles.Length ". Aktivní: " activeCount ". Archiv: " archivedCount ". Veterán: " veteranCount ". Odstaveno: " parkedCount ". Vyžaduje pozornost: " attentionCount ". Bez zelené karty: " GetMissingGreenCardCount() "."
    highlights := BuildDashboardProblemHighlightsText(dashboardEntries)
    if (highlights != "") {
        text .= " Nejvíc pálí: " highlights "."
    }

    return text
}

BuildDashboardTermSummaryText() {
    counts := GetTrayAttentionCounts()

    if (
        counts.overdueTechnical = 0
        && counts.overdueGreen = 0
        && counts.overdueReminders = 0
        && counts.upcomingTechnical = 0
        && counts.upcomingGreen = 0
        && counts.upcomingReminders = 0
        && GetMissingGreenCardCount() = 0
    ) {
        return "Momentálně není žádná technická kontrola, zelená karta ani vlastní připomínka, která by podle aktuálního nastavení vyžadovala pozornost."
    }

    return "Po termínu: " counts.overdueTechnical " TK, " counts.overdueGreen " ZK, " counts.overdueReminders " připomínek. Brzy vyprší: " counts.upcomingTechnical " TK, " counts.upcomingGreen " ZK, " counts.upcomingReminders " připomínek. Bez vyplněné ZK: " GetMissingGreenCardCount() "."
}

BuildDashboardCostSummaryText() {
    summary := BuildDashboardCurrentYearCostSummary()
    total := summary.totalFuel + summary.totalHistory + summary.totalRecords
    if (summary.parsedCount = 0) {
        text := "Rok " summary.year ": zatím nejsou započítané žádné číselné náklady."
        if (summary.zeroCostVehicleCount > 0) {
            text .= " Bez číselného nákladu letos: " summary.zeroCostVehicleCount " z " summary.activeVehicleCount " aktivních vozidel."
        }
        if (summary.skippedCount > 0) {
            text .= " Položek s nečíselnou částkou: " summary.skippedCount "."
        }
        if (summary.undatedCount > 0) {
            text .= " Položek bez použitelného data: " summary.undatedCount "."
        }
        return text
    }

    text := "Rok " summary.year ": celkem " FormatCostAmount(total) " u " summary.vehicleTotals.Count " vozidel."
    text .= " Tankování: " FormatCostAmount(summary.totalFuel) "."
    text .= " Historie a servis: " FormatCostAmount(summary.totalHistory) "."
    text .= " Doklady a pojištění: " FormatCostAmount(summary.totalRecords) "."

    topVehiclesText := BuildDashboardTopVehicleCostsText(summary)
    if (topVehiclesText != "") {
        text .= " Nejvýš: " topVehiclesText "."
    }

    if (summary.zeroCostVehicleCount > 0) {
        text .= " Bez číselného nákladu letos: " summary.zeroCostVehicleCount " z " summary.activeVehicleCount " aktivních vozidel."
    }

    if (summary.skippedCount > 0 || summary.undatedCount > 0) {
        issueParts := []
        if (summary.skippedCount > 0) {
            issueParts.Push(summary.skippedCount " s nečíselnou částkou")
        }
        if (summary.undatedCount > 0) {
            issueParts.Push(summary.undatedCount " bez použitelného data")
        }
        text .= " Nezapočteno: " JoinInline(issueParts, ", ") "."
    }

    return text
}

BuildDashboardCurrentYearCostSummary() {
    global Vehicles, VehicleFuelLog, VehicleHistory, VehicleRecords

    yearLabel := FormatTime(A_Now, "yyyy")
    yearValue := yearLabel + 0
    summary := {
        year: yearLabel,
        totalFuel: 0.0,
        totalHistory: 0.0,
        totalRecords: 0.0,
        parsedCount: 0,
        skippedCount: 0,
        undatedCount: 0,
        vehicleTotals: Map(),
        topVehicles: [],
        topVehicleId: "",
        topVehicleTotal: 0.0,
        activeVehicleCount: 0,
        zeroCostVehicleCount: 0
    }

    for entry in VehicleFuelLog {
        if (Trim(entry.totalCost) = "") {
            continue
        }
        if !TryGetEventYearMonth(entry.entryDate, &entryYear, &entryMonth) {
            summary.undatedCount += 1
            continue
        }
        if (entryYear != yearValue) {
            continue
        }

        if TryParseMoneyAmount(entry.totalCost, &amount) {
            summary.totalFuel += amount
            summary.parsedCount += 1
            AddDashboardVehicleCostTotal(summary.vehicleTotals, entry.vehicleId, amount)
        } else {
            summary.skippedCount += 1
        }
    }

    for entry in VehicleHistory {
        if (Trim(entry.cost) = "") {
            continue
        }
        if !TryGetEventYearMonth(entry.eventDate, &entryYear, &entryMonth) {
            summary.undatedCount += 1
            continue
        }
        if (entryYear != yearValue) {
            continue
        }

        if TryParseMoneyAmount(entry.cost, &amount) {
            summary.totalHistory += amount
            summary.parsedCount += 1
            AddDashboardVehicleCostTotal(summary.vehicleTotals, entry.vehicleId, amount)
        } else {
            summary.skippedCount += 1
        }
    }

    for entry in VehicleRecords {
        if (Trim(entry.price) = "") {
            continue
        }
        if !TryGetRecordYearMonth(entry, &entryYear, &entryMonth) {
            summary.undatedCount += 1
            continue
        }
        if (entryYear != yearValue) {
            continue
        }

        if TryParseMoneyAmount(entry.price, &amount) {
            summary.totalRecords += amount
            summary.parsedCount += 1
            AddDashboardVehicleCostTotal(summary.vehicleTotals, entry.vehicleId, amount)
        } else {
            summary.skippedCount += 1
        }
    }

    summary.topVehicles := BuildDashboardSortedVehicleCostTotals(summary.vehicleTotals)
    if (summary.topVehicles.Length > 0) {
        summary.topVehicleId := summary.topVehicles[1].vehicleId
        summary.topVehicleTotal := summary.topVehicles[1].total
    }

    for vehicle in Vehicles {
        if IsVehicleInactive(vehicle) {
            continue
        }

        summary.activeVehicleCount += 1
        if !summary.vehicleTotals.Has(vehicle.id) {
            summary.zeroCostVehicleCount += 1
        }
    }

    return summary
}

AddDashboardVehicleCostTotal(vehicleTotals, vehicleId, amount) {
    if !vehicleTotals.Has(vehicleId) {
        vehicleTotals[vehicleId] := 0.0
    }

    vehicleTotals[vehicleId] += amount
}

BuildDashboardSortedVehicleCostTotals(vehicleTotals) {
    items := []
    for vehicleId, total in vehicleTotals {
        items.Push({
            vehicleId: vehicleId,
            total: total
        })
    }

    SortDashboardVehicleCostTotals(&items)
    return items
}

SortDashboardVehicleCostTotals(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareDashboardVehicleCostTotals(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareDashboardVehicleCostTotals(left, right) {
    if (left.total > right.total) {
        return -1
    }
    if (left.total < right.total) {
        return 1
    }

    leftVehicle := FindVehicleById(left.vehicleId)
    rightVehicle := FindVehicleById(right.vehicleId)
    leftName := IsObject(leftVehicle) ? leftVehicle.name : left.vehicleId
    rightName := IsObject(rightVehicle) ? rightVehicle.name : right.vehicleId
    return CompareTextValues(leftName, rightName)
}

BuildDashboardTopVehicleCostsText(summary, limit := 3) {
    if !summary.HasOwnProp("topVehicles") || summary.topVehicles.Length = 0 {
        return ""
    }

    parts := []
    for item in summary.topVehicles {
        vehicle := FindVehicleById(item.vehicleId)
        if !IsObject(vehicle) {
            continue
        }

        parts.Push(ShortenText(vehicle.name, 24) " " FormatCostAmount(item.total))
        if (parts.Length >= limit) {
            break
        }
    }

    return JoinInline(parts, ", ")
}

BuildDashboardProblemHighlightsText(entries := "", limit := 3) {
    items := GetDashboardProblemHighlightItems(entries, limit)
    return JoinInline(items, ", ")
}

GetDashboardProblemVehicleCount(entries := "") {
    if !IsObject(entries) {
        entries := BuildDashboardEntries()
        SortDashboardEntries(&entries)
    }

    seenVehicles := Map()
    for entry in entries {
        if !IsObject(entry) || !entry.HasOwnProp("vehicle") || !IsObject(entry.vehicle) {
            continue
        }

        vehicleId := entry.vehicle.HasOwnProp("id") ? entry.vehicle.id : ""
        if (vehicleId = "" || seenVehicles.Has(vehicleId)) {
            continue
        }

        seenVehicles[vehicleId] := true
    }

    return seenVehicles.Count
}

GetDashboardProblemHighlightItems(entries := "", limit := 3) {
    if !IsObject(entries) {
        entries := BuildDashboardEntries()
        SortDashboardEntries(&entries)
    }

    highlights := []
    seenVehicles := Map()

    for entry in entries {
        if !IsObject(entry) || !entry.HasOwnProp("vehicle") || !IsObject(entry.vehicle) {
            continue
        }

        vehicleId := entry.vehicle.HasOwnProp("id") ? entry.vehicle.id : ""
        if (vehicleId != "" && seenVehicles.Has(vehicleId)) {
            continue
        }

        text := BuildDashboardProblemHighlightText(entry)
        if (text = "") {
            continue
        }

        if (vehicleId != "") {
            seenVehicles[vehicleId] := true
        }
        highlights.Push(text)
        if (highlights.Length >= limit) {
            break
        }
    }

    return highlights
}

BuildDashboardProblemHighlightText(entry) {
    if !entry.HasOwnProp("vehicle") || !IsObject(entry.vehicle) {
        return ""
    }

    vehicleName := ShortenText(entry.vehicle.name, 22)
    switch entry.kind {
        case "technical":
            detail := "TK " entry.status
        case "green":
            detail := (entry.HasOwnProp("isMissingGreen") && entry.isMissingGreen) ? "ZK chybí" : "ZK " entry.status
        case "custom":
            detail := "Připomínka " entry.status
        case "record_path":
            detail := "Doklad " entry.status
        case "vehicle_field":
            detail := entry.term " chybí"
        default:
            detail := entry.kindLabel " " entry.status
    }

    return vehicleName " (" detail ")"
}

BuildDashboardDataSummaryText() {
    global Vehicles, VehicleRecords

    missingPlateCount := 0
    missingTechnicalCount := 0
    missingPathCount := 0
    emptyPathCount := 0

    for vehicle in Vehicles {
        if (Trim(vehicle.plate) = "") {
            missingPlateCount += 1
        }
        if (Trim(vehicle.nextTk) = "") {
            missingTechnicalCount += 1
        }
    }

    for entry in VehicleRecords {
        pathKind := GetVehicleRecordPathInfo(entry).kind
        if (pathKind = "missing_file" || pathKind = "missing_folder") {
            missingPathCount += 1
        } else if (pathKind = "empty") {
            emptyPathCount += 1
        }
    }

    return "Bez SPZ: " missingPlateCount ". Bez příští TK: " missingTechnicalCount ". Bez vyplněné ZK: " GetMissingGreenCardCount() ". Dokladů s nedostupnou přílohou: " missingPathCount ". Dokladů bez cesty: " emptyPathCount "."
}

BuildDashboardEntries() {
    return BuildUpcomingOverviewEntries(true, true)
}

BuildDashboardDataIssueEntries() {
    global Vehicles, VehicleRecords

    entries := []

    for vehicle in Vehicles {
        if (Trim(vehicle.plate) = "") {
            entries.Push(BuildDashboardVehicleFieldIssueEntry(vehicle, "plate"))
        }
        if (Trim(vehicle.nextTk) = "") {
            entries.Push(BuildDashboardVehicleFieldIssueEntry(vehicle, "next_tk"))
        }
    }

    for entry in VehicleRecords {
        pathInfo := GetVehicleRecordPathInfo(entry)
        if (pathInfo.kind = "missing_file" || pathInfo.kind = "missing_folder" || pathInfo.kind = "empty") {
            vehicle := FindVehicleById(entry.vehicleId)
            if IsObject(vehicle) {
                entries.Push(BuildDashboardRecordIssueEntry(vehicle, entry, pathInfo))
            }
        }
    }

    return entries
}

BuildDashboardVehicleFieldIssueEntry(vehicle, fieldKind) {
    if (fieldKind = "plate") {
        return {
            kind: "vehicle_field",
            kindLabel: "Chybějící SPZ",
            vehicle: vehicle,
            dueStamp: "99999999999996",
            term: "SPZ",
            status: "Doplnit v editaci"
        }
    }

    return {
        kind: "vehicle_field",
        kindLabel: "Chybějící příští TK",
        vehicle: vehicle,
        dueStamp: "99999999999997",
        term: "Příští TK",
        status: "Doplnit v editaci"
    }
}

BuildDashboardRecordIssueEntry(vehicle, entry, pathInfo) {
    title := Trim(entry.title)
    if (title = "") {
        title := "(bez názvu)"
    }

    return {
        kind: "record_path",
        kindLabel: "Doklad / příloha",
        vehicle: vehicle,
        dueStamp: "99999999999995",
        term: ShortenText(title, 36),
        status: GetVehicleRecordPathStateLabel(pathInfo.kind),
        entryId: entry.id
    }
}

SortDashboardEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareDashboardEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareDashboardEntries(left, right) {
    leftGroup := GetDashboardEntrySortGroup(left)
    rightGroup := GetDashboardEntrySortGroup(right)

    result := CompareNumberValues(leftGroup, rightGroup)
    if (result != 0) {
        return result
    }

    if (leftGroup = 1) {
        result := CompareOverviewDueStamp(left, right)
        if (result != 0) {
            return result
        }
    }

    if (leftGroup = 4) {
        result := CompareDashboardVehicleFieldKind(left, right)
        if (result != 0) {
            return result
        }
    }

    result := CompareTextValues(left.kindLabel, right.kindLabel)
    if (result != 0) {
        return result
    }

    result := CompareVehicles(left.vehicle, right.vehicle)
    if (result != 0) {
        return result
    }

    result := CompareTextValues(left.term, right.term)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.status, right.status)
}

GetDashboardEntrySortGroup(entry) {
    if (entry.kind = "record_path") {
        return 3
    }

    if (entry.kind = "vehicle_field") {
        return 4
    }

    if (entry.kind = "green" && entry.HasOwnProp("isMissingGreen") && entry.isMissingGreen) {
        return 2
    }

    return 1
}

CompareDashboardVehicleFieldKind(left, right) {
    return CompareNumberValues(GetDashboardVehicleFieldSortValue(left.term), GetDashboardVehicleFieldSortValue(right.term))
}

GetDashboardVehicleFieldSortValue(term) {
    switch term {
        case "Příští TK":
            return 1
        case "SPZ":
            return 2
    }

    return 9
}

GetSelectedDashboardEntry() {
    global DashboardList, DashboardEntries

    if !IsObject(DashboardList) {
        return ""
    }

    row := DashboardList.GetNext(0)
    if !row {
        return ""
    }

    return DashboardEntries[row]
}

OpenOverviewFromDashboard(*) {
    CloseDashboardDialog()
    OpenUpcomingOverviewDialog()
}

OpenOverdueFromDashboard(*) {
    CloseDashboardDialog()
    OpenOverdueDialog()
}

OpenSelectedDashboardItem(*) {
    global AppTitle

    entry := GetSelectedDashboardEntry()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte položku, kterou chcete otevřít.", AppTitle, 0x40)
        return
    }

    CloseDashboardDialog()
    switch entry.kind {
        case "custom":
            if (entry.HasOwnProp("entryId") && entry.entryId != "") {
                OpenVehicleReminderDialog(entry.vehicle, false, entry.entryId)
                return
            }
        case "record_path":
            if (entry.HasOwnProp("entryId") && entry.entryId != "") {
                OpenVehicleRecordsDialog(entry.vehicle, false, entry.entryId)
                return
            }
    }

    OpenVehicleForm("edit", entry.vehicle)
}

OpenSelectedDashboardVehicle(*) {
    global AppTitle

    entry := GetSelectedDashboardEntry()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte položku, kterou chcete zobrazit.", AppTitle, 0x40)
        return
    }

    CloseDashboardDialog()
    OpenVehicleDetailDialog(entry.vehicle)
}

OpenSelectedDashboardVehicleCosts(*) {
    global AppTitle

    entry := GetSelectedDashboardEntry()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte položku, jejíž náklady chcete zobrazit.", AppTitle, 0x40)
        return
    }

    CloseDashboardDialog()
    OpenVehicleCostSummaryDialog(entry.vehicle)
}

EditSelectedDashboardVehicle(*) {
    global AppTitle

    entry := GetSelectedDashboardEntry()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte položku, kterou chcete upravit.", AppTitle, 0x40)
        return
    }

    CloseDashboardDialog()
    OpenVehicleForm("edit", entry.vehicle)
}
