OpenVehicleTimelineDialog(vehicle) {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, MaintenanceGui, MaintenanceFormGui, MaintenanceCompleteGui, CostSummaryGui, FleetCostGui, AuditGui
    global TimelineGui, TimelineVehicleId, TimelineList, TimelineSummaryLabel, TimelineFilterCtrl, TimelineSearchCtrl, TimelineEntries, TimelineAllEntries, TimelineOpenButton, TimelineVehicleButton

    if IsObject(TimelineGui) {
        WinActivate("ahk_id " TimelineGui.Hwnd)
        return
    }

    for guiRef in [FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, MaintenanceGui, MaintenanceFormGui, MaintenanceCompleteGui, CostSummaryGui, FleetCostGui, AuditGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    ShowMainWindow()

    TimelineVehicleId := vehicle.id
    TimelineAllEntries := BuildVehicleTimelineEntries(vehicle.id)
    TimelineEntries := []
    TimelineGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Časová osa vozidla")
    TimelineGui.SetFont("s10", "Segoe UI")
    TimelineGui.OnEvent("Close", CloseVehicleTimelineDialog)
    TimelineGui.OnEvent("Escape", CloseVehicleTimelineDialog)

    MainGui.Opt("+Disabled")

    TimelineGui.AddText("x20 y20 w940", "Zde vidíte jednu časovou osu vozidla " vehicle.name " napříč historií, tankováním, připomínkami, expirací dokladů, technickou kontrolou, zelenou kartou i servisními úkoly s konkrétním datem.")
    TimelineGui.AddText("x20 y55 w90", "Zobrazit")
    TimelineFilterCtrl := TimelineGui.AddDropDownList("x120 y52 w170 Choose1", ["Vše", "Budoucí", "Minulé"])
    TimelineFilterCtrl.OnEvent("Change", OnVehicleTimelineFilterChanged)

    refreshButton := TimelineGui.AddButton("x740 y50 w140 h28", "Obnovit")
    refreshButton.OnEvent("Click", RefreshVehicleTimelineDialog)

    TimelineGui.AddText("x20 y88 w140", "Hledat položku nebo stav")
    TimelineSearchCtrl := TimelineGui.AddEdit("x170 y85 w360")
    TimelineSearchCtrl.OnEvent("Change", OnVehicleTimelineSearchChanged)

    TimelineSummaryLabel := TimelineGui.AddText("x20 y118 w940", "")

    TimelineList := TimelineGui.AddListView("x20 y148 w940 h300 Grid -Multi", ["Datum", "Druh", "Položka", "Detail", "Stav"])
    TimelineList.OnEvent("DoubleClick", OpenSelectedVehicleTimelineItem)
    TimelineList.ModifyCol(1, "105")
    TimelineList.ModifyCol(2, "135")
    TimelineList.ModifyCol(3, "215")
    TimelineList.ModifyCol(4, "295")
    TimelineList.ModifyCol(5, "170")

    TimelineOpenButton := TimelineGui.AddButton("x210 y463 w170 h30", "Otevřít položku")
    TimelineOpenButton.OnEvent("Click", OpenSelectedVehicleTimelineItem)

    TimelineVehicleButton := TimelineGui.AddButton("x390 y463 w170 h30", "Detail vozidla")
    TimelineVehicleButton.OnEvent("Click", OpenSelectedVehicleTimelineVehicle)

    closeButton := TimelineGui.AddButton("x570 y463 w110 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleTimelineDialog)

    TimelineGui.Show("w980 h515")
    PopulateVehicleTimelineList("", true)
    if (TimelineEntries.Length = 0) {
        closeButton.Focus()
    }
}

CloseVehicleTimelineDialog(*) {
    global TimelineGui, TimelineVehicleId, TimelineList, TimelineSummaryLabel, TimelineFilterCtrl, TimelineSearchCtrl, TimelineEntries, TimelineAllEntries, TimelineOpenButton, TimelineVehicleButton, MainGui

    if IsObject(TimelineGui) {
        TimelineGui.Destroy()
        TimelineGui := 0
    }

    TimelineVehicleId := ""
    TimelineList := 0
    TimelineSummaryLabel := 0
    TimelineFilterCtrl := 0
    TimelineSearchCtrl := 0
    TimelineEntries := []
    TimelineAllEntries := []
    TimelineOpenButton := 0
    TimelineVehicleButton := 0
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

RefreshVehicleTimelineDialog(*) {
    global TimelineVehicleId, TimelineAllEntries

    if (TimelineVehicleId = "") {
        return
    }

    TimelineAllEntries := BuildVehicleTimelineEntries(TimelineVehicleId)
    PopulateVehicleTimelineList(GetSelectedVehicleTimelineEntryKey())
}

OnVehicleTimelineFilterChanged(*) {
    PopulateVehicleTimelineList(GetSelectedVehicleTimelineEntryKey())
}

OnVehicleTimelineSearchChanged(*) {
    PopulateVehicleTimelineList(GetSelectedVehicleTimelineEntryKey())
}

FocusVehicleTimelineSearchShortcut() {
    global TimelineGui, TimelineSearchCtrl

    if !IsGuiWindowActive(TimelineGui) || !IsObject(TimelineSearchCtrl) {
        return
    }

    TimelineSearchCtrl.Focus()
}

PopulateVehicleTimelineList(selectEntryKey := "", focusList := false) {
    global TimelineGui, TimelineList, TimelineSummaryLabel, TimelineEntries, TimelineAllEntries

    if !IsObject(TimelineGui) || !IsObject(TimelineList) {
        return
    }

    TimelineEntries := FilterVehicleTimelineEntries(TimelineAllEntries, GetVehicleTimelineFilterKind(), GetVehicleTimelineSearchText())

    if IsObject(TimelineSummaryLabel) {
        TimelineSummaryLabel.Text := BuildVehicleTimelineSummary(TimelineEntries, TimelineAllEntries)
    }

    TimelineList.Opt("-Redraw")
    TimelineList.Delete()
    selectedRow := 0
    for entry in TimelineEntries {
        row := TimelineList.Add("", entry.dateText, entry.kindLabel, entry.title, entry.detail, entry.status)
        if (selectEntryKey != "" && BuildVehicleTimelineEntryKey(entry) = selectEntryKey) {
            selectedRow := row
        }
    }
    TimelineList.Opt("+Redraw")

    if (TimelineEntries.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    TimelineList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
}

GetVehicleTimelineFilterKind() {
    global TimelineFilterCtrl

    if !IsObject(TimelineFilterCtrl) {
        return "all"
    }

    switch TimelineFilterCtrl.Value {
        case 2:
            return "future"
        case 3:
            return "past"
    }

    return "all"
}

GetVehicleTimelineSearchText() {
    global TimelineSearchCtrl

    if !IsObject(TimelineSearchCtrl) {
        return ""
    }

    return Trim(TimelineSearchCtrl.Text)
}

FilterVehicleTimelineEntries(entries, filterKind := "all", searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        isFuture := IsVehicleTimelineEntryFuture(entry)
        if (filterKind = "future" && !isFuture) {
            continue
        }
        if (filterKind = "past" && isFuture) {
            continue
        }

        haystack := StrLower(entry.dateText " " entry.kindLabel " " entry.title " " entry.detail " " entry.status " " BuildVehicleTimelineEntrySearchText(entry))
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

BuildVehicleTimelineSummary(entries, allEntries) {
    if (allEntries.Length = 0) {
        return "Pro toto vozidlo zatím nejsou žádné časové položky s datem."
    }

    futureCount := 0
    pastCount := 0
    for entry in allEntries {
        if IsVehicleTimelineEntryFuture(entry) {
            futureCount += 1
        } else {
            pastCount += 1
        }
    }

    text := "Celkem položek: " allEntries.Length ". Budoucí: " futureCount ". Minulé: " pastCount "."
    if (entries.Length != allEntries.Length) {
        text .= " Po filtru zobrazeno: " entries.Length "."
    }

    if (futureCount > 0) {
        text .= " Nejbližší budoucí položky jsou nahoře, pod nimi nejnovější minulost."
    }

    return text
}

BuildVehicleTimelineEntries(vehicleId) {
    vehicle := FindVehicleById(vehicleId)
    if !IsObject(vehicle) {
        return []
    }

    entries := []

    for event in GetVehicleHistoryEntries(vehicleId) {
        stamp := ParseEventDateStamp(event.eventDate)
        if (stamp = "") {
            continue
        }

        entries.Push({
            kind: "history",
            kindLabel: "Historie",
            vehicle: vehicle,
            dateText: event.eventDate,
            dateStamp: stamp,
            title: event.eventType,
            detail: BuildVehicleTimelineHistoryDetail(event),
            status: Trim(event.cost) != "" ? FormatFuelMoney(event.cost) : "-",
            entryId: event.id,
            note: event.note
        })
    }

    for entry in GetVehicleFuelEntries(vehicleId) {
        stamp := ParseEventDateStamp(entry.entryDate)
        if (stamp = "") {
            continue
        }

        entries.Push({
            kind: "fuel",
            kindLabel: "Tankování",
            vehicle: vehicle,
            dateText: entry.entryDate,
            dateStamp: stamp,
            title: "Tankování",
            detail: BuildVehicleTimelineFuelDetail(entry),
            status: Trim(entry.totalCost) != "" ? FormatFuelMoney(entry.totalCost) : "-",
            entryId: entry.id,
            note: entry.note
        })
    }

    if (Trim(vehicle.nextTk) != "") {
        stamp := ParseDueStamp(vehicle.nextTk)
        if (stamp != "") {
            entries.Push({
                kind: "technical",
                kindLabel: "Technická kontrola",
                vehicle: vehicle,
                dateText: vehicle.nextTk,
                dateStamp: stamp,
                title: "Příští TK",
                detail: BuildVehicleTimelineVehicleDetail(vehicle),
                status: GetExpirationStatusText(vehicle.nextTk, GetTechnicalReminderDays()),
                entryId: "",
                note: ""
            })
        }
    }

    if (Trim(vehicle.greenCardTo) != "") {
        stamp := ParseDueStamp(vehicle.greenCardTo)
        if (stamp != "") {
            entries.Push({
                kind: "green",
                kindLabel: "Zelená karta",
                vehicle: vehicle,
                dateText: vehicle.greenCardTo,
                dateStamp: stamp,
                title: "Konec zelené karty",
                detail: BuildVehicleTimelineVehicleDetail(vehicle),
                status: GetExpirationStatusText(vehicle.greenCardTo, GetGreenCardReminderDays()),
                entryId: "",
                note: ""
            })
        }
    }

    for reminder in GetVehicleReminderEntries(vehicleId) {
        stamp := ParseReminderDueStamp(reminder.dueDate)
        if (stamp = "") {
            continue
        }

        entries.Push({
            kind: "custom",
            kindLabel: "Připomínka",
            vehicle: vehicle,
            dateText: reminder.dueDate,
            dateStamp: stamp,
            title: reminder.title,
            detail: BuildVehicleTimelineReminderDetail(reminder),
            status: GetReminderExpirationStatusText(reminder.dueDate, reminder.reminderDays + 0),
            entryId: reminder.id,
            note: reminder.note
        })
    }

    for record in GetVehicleRecords(vehicleId) {
        stamp := ParseDueStamp(record.validTo)
        if (stamp = "") {
            continue
        }

        entries.Push({
            kind: "record",
            kindLabel: "Doklad",
            vehicle: vehicle,
            dateText: record.validTo,
            dateStamp: stamp,
            title: record.recordType ": " record.title,
            detail: BuildVehicleTimelineRecordDetail(record),
            status: GetExpirationStatusText(record.validTo, 30),
            entryId: record.id,
            note: record.note
        })
    }

    for snapshot in BuildVehicleMaintenanceSnapshots(vehicleId, true) {
        if !snapshot.plan.isActive || Trim(snapshot.dueDate) = "" || snapshot.dueDateStamp = "" {
            continue
        }

        entries.Push({
            kind: "maintenance",
            kindLabel: "Plán údržby",
            vehicle: vehicle,
            dateText: snapshot.dueDate,
            dateStamp: snapshot.dueDateStamp,
            title: snapshot.title,
            detail: snapshot.nextServiceText,
            status: snapshot.statusText,
            entryId: snapshot.plan.id,
            note: snapshot.plan.note
        })
    }

    SortVehicleTimelineEntries(&entries)
    return entries
}

BuildVehicleTimelineHistoryDetail(event) {
    parts := []
    if (Trim(event.odometer) != "") {
        parts.Push(FormatHistoryOdometer(event.odometer))
    }
    if (Trim(event.note) != "") {
        parts.Push(ShortenText(event.note, 80))
    }

    return parts.Length > 0 ? JoinInline(parts, " | ") : "-"
}

BuildVehicleTimelineFuelDetail(entry) {
    parts := []
    if (Trim(entry.odometer) != "") {
        parts.Push(FormatHistoryOdometer(entry.odometer))
    }
    if (Trim(entry.liters) != "") {
        parts.Push(FormatFuelLiters(entry.liters))
    }
    if (Trim(entry.fuelType) != "") {
        parts.Push(entry.fuelType)
    }
    if (Trim(entry.note) != "") {
        parts.Push(ShortenText(entry.note, 60))
    }

    return parts.Length > 0 ? JoinInline(parts, " | ") : "-"
}

BuildVehicleTimelineReminderDetail(reminder) {
    parts := [GetReminderRepeatLabel(reminder.HasOwnProp("repeatMode") ? reminder.repeatMode : "")]
    if (Trim(reminder.note) != "") {
        parts.Push(ShortenText(reminder.note, 70))
    }
    return JoinNonEmptyTimelineParts(parts)
}

BuildVehicleTimelineRecordDetail(record) {
    parts := []
    if (Trim(record.provider) != "") {
        parts.Push(record.provider)
    }
    if (Trim(record.filePath) != "") {
        parts.Push(GetVehicleRecordPathStateText(record))
        parts.Push(GetVehicleRecordAttachmentModeLabel(record))
    }
    if (Trim(record.note) != "") {
        parts.Push(ShortenText(record.note, 60))
    }

    return parts.Length > 0 ? JoinInline(parts, " | ") : "-"
}

BuildVehicleTimelineVehicleDetail(vehicle) {
    parts := []
    if (Trim(vehicle.plate) != "") {
        parts.Push(vehicle.plate)
    }
    if (Trim(vehicle.makeModel) != "") {
        parts.Push(vehicle.makeModel)
    }

    return parts.Length > 0 ? JoinInline(parts, " | ") : "-"
}

JoinNonEmptyTimelineParts(parts) {
    filtered := []
    for part in parts {
        if (Trim(part) != "") {
            filtered.Push(part)
        }
    }

    return filtered.Length > 0 ? JoinInline(filtered, " | ") : "-"
}

SortVehicleTimelineEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleTimelineEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleTimelineEntries(left, right) {
    leftFuture := IsVehicleTimelineEntryFuture(left)
    rightFuture := IsVehicleTimelineEntryFuture(right)

    if (leftFuture != rightFuture) {
        return leftFuture ? -1 : 1
    }

    if leftFuture {
        if (left.dateStamp < right.dateStamp) {
            return -1
        }
        if (left.dateStamp > right.dateStamp) {
            return 1
        }
    } else {
        if (left.dateStamp > right.dateStamp) {
            return -1
        }
        if (left.dateStamp < right.dateStamp) {
            return 1
        }
    }

    result := CompareTextValues(left.kindLabel, right.kindLabel)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.title, right.title)
}

IsVehicleTimelineEntryFuture(entry) {
    return entry.dateStamp >= GetTimelineTodayStartStamp()
}

GetTimelineTodayStartStamp() {
    return FormatTime(A_Now, "yyyyMMdd") "000000"
}

GetSelectedVehicleTimelineEntry() {
    global TimelineList, TimelineEntries

    if !IsObject(TimelineList) {
        return ""
    }

    row := TimelineList.GetNext(0)
    if !row || row > TimelineEntries.Length {
        return ""
    }

    return TimelineEntries[row]
}

GetSelectedVehicleTimelineEntryKey() {
    entry := GetSelectedVehicleTimelineEntry()
    return IsObject(entry) ? BuildVehicleTimelineEntryKey(entry) : ""
}

BuildVehicleTimelineEntryKey(entry) {
    if entry.HasOwnProp("entryId") && entry.entryId != "" {
        return entry.kind "|" entry.vehicle.id "|" entry.entryId
    }

    return entry.kind "|" entry.vehicle.id "|" entry.dateText "|" entry.title
}

BuildVehicleTimelineEntrySearchText(entry) {
    parts := [entry.vehicle.name, entry.vehicle.category]
    if (Trim(entry.vehicle.plate) != "") {
        parts.Push(entry.vehicle.plate)
    }
    if (Trim(entry.vehicle.makeModel) != "") {
        parts.Push(entry.vehicle.makeModel)
    }
    if entry.HasOwnProp("note") && Trim(entry.note) != "" {
        parts.Push(entry.note)
    }

    return JoinInline(parts, " ")
}

OpenSelectedVehicleTimelineItem(*) {
    entry := GetSelectedVehicleTimelineEntry()
    if !IsObject(entry) {
        return
    }

    OpenVehicleTimelineEntry(entry)
}

OpenSelectedVehicleTimelineVehicle(*) {
    entry := GetSelectedVehicleTimelineEntry()
    if !IsObject(entry) {
        return
    }

    CloseVehicleTimelineDialog()
    OpenVehicleDetailDialog(entry.vehicle)
}

OpenVehicleTimelineEntry(entry, closeDialog := true) {
    hooks := GetVehimapTestHooks()
    if IsObject(hooks) && hooks.HasOwnProp("captureTimelineOpenActions") && hooks.captureTimelineOpenActions {
        if !hooks.HasOwnProp("timelineOpenActions") || !IsObject(hooks.timelineOpenActions) {
            hooks.timelineOpenActions := []
        }
        hooks.timelineOpenActions.Push({
            kind: entry.kind,
            vehicleId: entry.vehicle.id,
            entryId: entry.HasOwnProp("entryId") ? entry.entryId : "",
            title: entry.title
        })
        return
    }

    if closeDialog {
        CloseVehicleTimelineDialog()
    }

    switch entry.kind {
        case "history":
            OpenVehicleHistoryDialog(entry.vehicle, false, entry.entryId)
        case "fuel":
            OpenVehicleFuelDialog(entry.vehicle, false, entry.entryId)
        case "custom":
            OpenVehicleReminderDialog(entry.vehicle, false, entry.entryId)
        case "record":
            OpenVehicleRecordsDialog(entry.vehicle, false, entry.entryId)
        case "maintenance":
            OpenVehicleMaintenanceDialog(entry.vehicle, false, entry.entryId)
        case "technical", "green":
            OpenVehicleForm("edit", entry.vehicle)
        default:
            OpenVehicleDetailDialog(entry.vehicle)
    }
}

ExportVehimapCalendarIcs(*) {
    global AppTitle

    exportData := BuildVehimapCalendarExportItems()
    if (exportData.items.Length = 0) {
        AppMsgBox("V kalendáři teď není žádný budoucí termín s konkrétním datem, který by šel exportovat.", AppTitle, 0x40)
        return
    }

    exportPath := FileSelect("S16", GetDefaultVehimapCalendarExportPath(), "Export termínů do kalendáře", "iCalendar (*.ics)")
    if (exportPath = "") {
        return
    }

    exportPath := EnsureFileExtension(exportPath, ".ics")
    try {
        WriteVehimapCalendarIcs(exportPath, exportData.items)
    } catch as err {
        MsgBox("Kalendář se nepodařilo exportovat.`n`n" err.Message, AppTitle, 0x10)
        return
    }

    message := "Kalendář byl exportován do:`n" exportPath "`n`nPoložek: " exportData.items.Length "."
    if (exportData.skippedMaintenanceCount > 0) {
        message .= "`nServisních úkolů bez konkrétního data bylo vynecháno: " exportData.skippedMaintenanceCount "."
    }
    AppMsgBox(message, AppTitle, 0x40)
}

GetDefaultVehimapCalendarExportPath() {
    return A_ScriptDir "\vehimap-kalendar-" FormatTime(A_Now, "yyyy-MM-dd") ".ics"
}

BuildVehimapCalendarExportItems() {
    global Vehicles

    items := []
    skippedMaintenanceCount := 0
    todayStart := GetTimelineTodayStartStamp()

    for vehicle in Vehicles {
        if (Trim(vehicle.nextTk) != "") {
            stamp := ParseDueStamp(vehicle.nextTk)
            if (stamp != "" && stamp >= todayStart) {
                items.Push(BuildVehimapCalendarItem("technical", "TK", vehicle, vehicle.nextTk, stamp, "", "Příští TK", BuildVehicleTimelineVehicleDetail(vehicle), GetExpirationStatusText(vehicle.nextTk, GetTechnicalReminderDays())))
            }
        }

        if (Trim(vehicle.greenCardTo) != "") {
            stamp := ParseDueStamp(vehicle.greenCardTo)
            if (stamp != "" && stamp >= todayStart) {
                items.Push(BuildVehimapCalendarItem("green", "Zelená karta", vehicle, vehicle.greenCardTo, stamp, "", "Konec zelené karty", BuildVehicleTimelineVehicleDetail(vehicle), GetExpirationStatusText(vehicle.greenCardTo, GetGreenCardReminderDays())))
            }
        }

        for reminder in GetVehicleReminderEntries(vehicle.id) {
            stamp := ParseReminderDueStamp(reminder.dueDate)
            if (stamp = "" || stamp < todayStart) {
                continue
            }

            items.Push(BuildVehimapCalendarItem("custom", "Připomínka", vehicle, reminder.dueDate, stamp, reminder.id, reminder.title, BuildVehicleTimelineReminderDetail(reminder), GetReminderExpirationStatusText(reminder.dueDate, reminder.reminderDays + 0)))
        }

        for record in GetVehicleRecords(vehicle.id) {
            stamp := ParseDueStamp(record.validTo)
            if (stamp = "" || stamp < todayStart) {
                continue
            }

            items.Push(BuildVehimapCalendarItem("record", "Doklad", vehicle, record.validTo, stamp, record.id, record.recordType ": " record.title, BuildVehicleTimelineRecordDetail(record), GetExpirationStatusText(record.validTo, 30)))
        }

        for snapshot in BuildVehicleMaintenanceSnapshots(vehicle.id, true) {
            if !snapshot.plan.isActive {
                continue
            }
            if (Trim(snapshot.dueDate) = "" || snapshot.dueDateStamp = "") {
                skippedMaintenanceCount += 1
                continue
            }
            if (snapshot.dueDateStamp < todayStart) {
                continue
            }

            items.Push(BuildVehimapCalendarItem("maintenance", "Servis", vehicle, snapshot.dueDate, snapshot.dueDateStamp, snapshot.plan.id, snapshot.title, snapshot.nextServiceText, snapshot.statusText))
        }
    }

    SortVehimapCalendarItems(&items)
    return {
        items: items,
        skippedMaintenanceCount: skippedMaintenanceCount
    }
}

BuildVehimapCalendarItem(kind, kindLabel, vehicle, dateText, dateStamp, entryId, title, detail, status) {
    return {
        kind: kind,
        kindLabel: kindLabel,
        vehicle: vehicle,
        dateText: dateText,
        dateStamp: dateStamp,
        entryId: entryId,
        summary: "Vehimap - " kindLabel " - " vehicle.name,
        description: BuildVehimapCalendarDescription(kindLabel, vehicle, title, dateText, detail, status),
        uid: BuildVehimapCalendarUid(kind, vehicle.id, entryId, dateText, title)
    }
}

BuildVehimapCalendarDescription(kindLabel, vehicle, title, dateText, detail, status) {
    lines := [
        "Vozidlo: " vehicle.name,
        "Druh: " kindLabel,
        "Položka: " title,
        "Termín: " dateText
    ]
    if (Trim(vehicle.plate) != "") {
        lines.Push("SPZ: " vehicle.plate)
    }
    if (Trim(detail) != "" && detail != "-") {
        lines.Push("Detail: " detail)
    }
    if (Trim(status) != "" && status != "-") {
        lines.Push("Stav: " status)
    }

    return JoinLines(lines)
}

BuildVehimapCalendarUid(kind, vehicleId, entryId, dateText, title) {
    stableId := entryId != "" ? entryId : SanitizeVehimapCalendarUidPart(dateText "|" title)
    return "vehimap-" kind "-" vehicleId "-" stableId "@vlcekapps"
}

SanitizeVehimapCalendarUidPart(text) {
    value := StrLower(Trim(text))
    value := RegExReplace(value, "[^a-z0-9]+", "-")
    value := Trim(value, "-")
    return value = "" ? "item" : value
}

SortVehimapCalendarItems(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehimapCalendarItems(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehimapCalendarItems(left, right) {
    if (left.dateStamp < right.dateStamp) {
        return -1
    }
    if (left.dateStamp > right.dateStamp) {
        return 1
    }

    result := CompareTextValues(left.summary, right.summary)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.uid, right.uid)
}

WriteVehimapCalendarIcs(path, items) {
    content := BuildVehimapCalendarIcsContent(items)
    WriteTextFileUtf8NoBom(path, content)
}

BuildVehimapCalendarIcsContent(items) {
    lines := [
        "BEGIN:VCALENDAR",
        "VERSION:2.0",
        "PRODID:-//vlcekapps//Vehimap//CS",
        "CALSCALE:GREGORIAN",
        "METHOD:PUBLISH"
    ]

    dtStamp := FormatTime(A_NowUTC, "yyyyMMdd'T'HHmmss'Z'")
    for item in items {
        startDate := SubStr(item.dateStamp, 1, 8)
        endDate := FormatTime(DateAdd(startDate "000000", 1, "Days"), "yyyyMMdd")
        lines.Push("BEGIN:VEVENT")
        lines.Push("UID:" EscapeIcsText(item.uid))
        lines.Push("DTSTAMP:" dtStamp)
        lines.Push("DTSTART;VALUE=DATE:" startDate)
        lines.Push("DTEND;VALUE=DATE:" endDate)
        lines.Push("SUMMARY:" EscapeIcsText(item.summary))
        lines.Push("DESCRIPTION:" EscapeIcsText(item.description))
        lines.Push("END:VEVENT")
    }

    lines.Push("END:VCALENDAR")
    return JoinInline(lines, "`r`n") "`r`n"
}

EscapeIcsText(text) {
    escaped := StrReplace(text, "\", "\\")
    escaped := StrReplace(escaped, ";", "\;")
    escaped := StrReplace(escaped, ",", "\,")
    escaped := StrReplace(escaped, "`r`n", "\n")
    escaped := StrReplace(escaped, "`n", "\n")
    return escaped
}
