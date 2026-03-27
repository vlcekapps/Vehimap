SwitchOverviewToOverdueShortcut() {
    global OverviewGui

    if !IsGuiWindowActive(OverviewGui) {
        return
    }

    CloseUpcomingOverviewDialog()
    OpenOverdueDialog()
}

SwitchOverdueToOverviewShortcut() {
    global OverdueGui

    if !IsGuiWindowActive(OverdueGui) {
        return
    }

    CloseOverdueDialog()
    OpenUpcomingOverviewDialog()
}

OpenUpcomingOverviewDialog(*) {
    global AppTitle, MainGui, FormGui, SettingsGui, DashboardGui, OverviewGui, OverviewList, OverviewEntries, OverviewAllEntries, OverviewSummaryLabel, OverviewFilterCtrl, OverviewSearchCtrl, OverviewItemButton, OverviewOpenButton, OverviewEditButton, OverviewShowMissingGreenCtrl, OverviewShowDataIssuesCtrl, OverviewSortColumn, OverviewSortDescending, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui

    if IsObject(OverviewGui) {
        WinActivate("ahk_id " OverviewGui.Hwnd)
        return
    }

    if IsObject(DashboardGui) {
        WinActivate("ahk_id " DashboardGui.Hwnd)
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

    if IsObject(RecordsGui) {
        WinActivate("ahk_id " RecordsGui.Hwnd)
        return
    }

    if IsObject(RecordFormGui) {
        WinActivate("ahk_id " RecordFormGui.Hwnd)
        return
    }

    ShowMainWindow()

    OverviewSortColumn := GetOverviewSortColumnSetting()
    OverviewSortDescending := GetOverviewSortDescendingSetting()
    OverviewAllEntries := BuildUpcomingOverviewEntries(GetOverviewIncludeMissingGreenSetting(), GetOverviewIncludeDataIssuesSetting())
    OverviewEntries := []
    OverviewGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Přehled termínů")
    OverviewGui.SetFont("s10", "Segoe UI")
    OverviewGui.OnEvent("Close", CloseUpcomingOverviewDialog)
    OverviewGui.OnEvent("Escape", CloseUpcomingOverviewDialog)

    MainGui.Opt("+Disabled")

    OverviewGui.AddText("x20 y20 w860", "Zde vidíte všechny blížící se a propadlé termíny technických kontrol, zelených karet, vlastních připomínek i plánů údržby podle aktuálního nastavení upozornění. Volitelně můžete přidat i datové nedostatky k doplnění.")
    OverviewGui.AddText("x20 y55 w90", "Filtr zobrazení")
    OverviewFilterCtrl := OverviewGui.AddDropDownList("x120 y52 w220 Choose1", ["Vše", "Jen technické kontroly", "Jen zelené karty", "Jen vlastní připomínky", "Jen plány údržby", "Jen datové nedostatky"])
    OverviewFilterCtrl.Value := GetOverviewFilterIndex()
    OverviewFilterCtrl.OnEvent("Change", OnOverviewFilterChanged)

    OverviewShowMissingGreenCtrl := OverviewGui.AddCheckBox("x350 y54 w280", "Zobrazit i vozidla bez zelené karty")
    OverviewShowMissingGreenCtrl.Value := GetOverviewIncludeMissingGreenSetting()
    OverviewShowMissingGreenCtrl.OnEvent("Click", OnOverviewShowMissingGreenChanged)

    refreshButton := OverviewGui.AddButton("x730 y50 w150 h28", "Obnovit")
    refreshButton.OnEvent("Click", RefreshUpcomingOverviewDialog)

    OverviewGui.AddText("x20 y88 w140", "Hledat název, SPZ nebo položku")
    OverviewSearchCtrl := OverviewGui.AddEdit("x170 y85 w250")
    OverviewSearchCtrl.OnEvent("Change", OnOverviewSearchChanged)

    OverviewShowDataIssuesCtrl := OverviewGui.AddCheckBox("x440 y87 w320", "Zobrazit i datové nedostatky")
    OverviewShowDataIssuesCtrl.Value := GetOverviewIncludeDataIssuesSetting()
    OverviewShowDataIssuesCtrl.OnEvent("Click", OnOverviewShowDataIssuesChanged)

    OverviewSummaryLabel := OverviewGui.AddText("x20 y118 w860", "")

    OverviewList := OverviewGui.AddListView("x20 y148 w860 h187 Grid -Multi", ["Druh", "Vozidlo", "Kategorie", "Značka / model", "SPZ", "Položka / termín", "Stav"])
    OverviewList.OnEvent("DoubleClick", OpenSelectedOverviewItem)
    OverviewList.OnEvent("ColClick", OnOverviewColumnClick)

    OverviewList.ModifyCol(1, "135")
    OverviewList.ModifyCol(2, "155")
    OverviewList.ModifyCol(3, "120")
    OverviewList.ModifyCol(4, "150")
    OverviewList.ModifyCol(5, "90")
    OverviewList.ModifyCol(6, "110")
    OverviewList.ModifyCol(7, "100")

    OverviewItemButton := OverviewGui.AddButton("x170 y350 w150 h30", "Otevřít položku")
    OverviewItemButton.OnEvent("Click", OpenSelectedOverviewItem)

    OverviewEditButton := OverviewGui.AddButton("x330 y350 w150 h30", "Upravit vozidlo")
    OverviewEditButton.OnEvent("Click", EditSelectedOverviewVehicle)

    OverviewOpenButton := OverviewGui.AddButton("x490 y350 w150 h30 Default", "Zobrazit vozidlo")
    OverviewOpenButton.OnEvent("Click", OpenSelectedOverviewVehicle)

    closeButton := OverviewGui.AddButton("x650 y350 w150 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseUpcomingOverviewDialog)

    OverviewGui.Show("w900 h405")
    PopulateUpcomingOverviewList("", true)
    if (OverviewEntries.Length > 0) {
        ; selection and focus are handled by PopulateUpcomingOverviewList
    } else {
        closeButton.Focus()
    }
}

CloseUpcomingOverviewDialog(*) {
    global OverviewGui, OverviewList, OverviewEntries, OverviewAllEntries, OverviewSummaryLabel, OverviewFilterCtrl, OverviewSearchCtrl, OverviewItemButton, OverviewOpenButton, OverviewEditButton, OverviewShowMissingGreenCtrl, OverviewShowDataIssuesCtrl, OverviewSortColumn, OverviewSortDescending, MainGui

    if IsObject(OverviewGui) {
        OverviewGui.Destroy()
        OverviewGui := 0
    }

    OverviewList := 0
    OverviewEntries := []
    OverviewAllEntries := []
    OverviewSummaryLabel := 0
    OverviewFilterCtrl := 0
    OverviewSearchCtrl := 0
    OverviewItemButton := 0
    OverviewOpenButton := 0
    OverviewEditButton := 0
    OverviewShowMissingGreenCtrl := 0
    OverviewShowDataIssuesCtrl := 0
    OverviewSortColumn := 6
    OverviewSortDescending := false
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

OpenOverdueDialog(*) {
    global AppTitle, MainGui, FormGui, SettingsGui, DashboardGui, OverviewGui, OverdueGui, OverdueList, OverdueEntries, OverdueAllEntries, OverdueSummaryLabel, OverdueSearchCtrl, OverdueItemButton, OverdueOpenButton, OverdueEditButton, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui

    if IsObject(OverdueGui) {
        WinActivate("ahk_id " OverdueGui.Hwnd)
        return
    }

    if IsObject(DashboardGui) {
        WinActivate("ahk_id " DashboardGui.Hwnd)
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

    if IsObject(RecordsGui) {
        WinActivate("ahk_id " RecordsGui.Hwnd)
        return
    }

    if IsObject(RecordFormGui) {
        WinActivate("ahk_id " RecordFormGui.Hwnd)
        return
    }

    ShowMainWindow()

    OverdueAllEntries := BuildOverdueEntries()
    OverdueEntries := []
    OverdueGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Propadlé termíny")
    OverdueGui.SetFont("s10", "Segoe UI")
    OverdueGui.OnEvent("Close", CloseOverdueDialog)
    OverdueGui.OnEvent("Escape", CloseOverdueDialog)

    MainGui.Opt("+Disabled")

    OverdueGui.AddText("x20 y20 w860", "Zde vidíte všechny už propadlé technické kontroly, zelené karty, vlastní připomínky i plány údržby.")

    refreshButton := OverdueGui.AddButton("x730 y50 w150 h28", "Obnovit")
    refreshButton.OnEvent("Click", RefreshOverdueDialog)

    OverdueGui.AddText("x20 y55 w140", "Hledat název, SPZ nebo položku")
    OverdueSearchCtrl := OverdueGui.AddEdit("x170 y52 w300")
    OverdueSearchCtrl.OnEvent("Change", OnOverdueSearchChanged)

    OverdueSummaryLabel := OverdueGui.AddText("x20 y90 w860", "")

    OverdueList := OverdueGui.AddListView("x20 y120 w860 h215 Grid -Multi", ["Druh", "Vozidlo", "Kategorie", "Značka / model", "SPZ", "Položka / termín", "Stav"])
    OverdueList.OnEvent("DoubleClick", OpenSelectedOverdueItem)
    OverdueList.ModifyCol(1, "135")
    OverdueList.ModifyCol(2, "155")
    OverdueList.ModifyCol(3, "120")
    OverdueList.ModifyCol(4, "150")
    OverdueList.ModifyCol(5, "90")
    OverdueList.ModifyCol(6, "110")
    OverdueList.ModifyCol(7, "100")

    OverdueItemButton := OverdueGui.AddButton("x170 y350 w150 h30", "Otevřít položku")
    OverdueItemButton.OnEvent("Click", OpenSelectedOverdueItem)

    OverdueEditButton := OverdueGui.AddButton("x330 y350 w150 h30", "Upravit vozidlo")
    OverdueEditButton.OnEvent("Click", EditSelectedOverdueVehicle)

    OverdueOpenButton := OverdueGui.AddButton("x490 y350 w150 h30 Default", "Zobrazit vozidlo")
    OverdueOpenButton.OnEvent("Click", OpenSelectedOverdueVehicle)

    closeButton := OverdueGui.AddButton("x650 y350 w150 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseOverdueDialog)

    OverdueGui.Show("w900 h405")
    PopulateOverdueList("", true)
    if (OverdueEntries.Length = 0) {
        closeButton.Focus()
    }
}

CloseOverdueDialog(*) {
    global OverdueGui, OverdueList, OverdueEntries, OverdueAllEntries, OverdueSummaryLabel, OverdueSearchCtrl, OverdueItemButton, OverdueOpenButton, OverdueEditButton, MainGui

    if IsObject(OverdueGui) {
        OverdueGui.Destroy()
        OverdueGui := 0
    }

    OverdueList := 0
    OverdueEntries := []
    OverdueAllEntries := []
    OverdueSummaryLabel := 0
    OverdueSearchCtrl := 0
    OverdueItemButton := 0
    OverdueOpenButton := 0
    OverdueEditButton := 0
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

RefreshOverdueDialog(*) {
    global OverdueGui, OverdueAllEntries

    if !IsObject(OverdueGui) {
        return
    }

    selectedKey := GetSelectedOverdueEntryKey()
    OverdueAllEntries := BuildOverdueEntries()
    PopulateOverdueList(selectedKey)
}

OnOverdueSearchChanged(*) {
    selectedKey := GetSelectedOverdueEntryKey()
    PopulateOverdueList(selectedKey)
}

PopulateOverdueList(selectedKey := "", focusList := false) {
    global OverdueAllEntries, OverdueEntries, OverdueList, OverdueSummaryLabel, OverdueItemButton, OverdueOpenButton, OverdueEditButton

    if !IsObject(OverdueList) {
        return
    }

    OverdueEntries := FilterOverviewEntriesBySearch(OverdueAllEntries, GetOverdueSearchText())
    SortUpcomingByDue(&OverdueEntries)

    if IsObject(OverdueSummaryLabel) {
        OverdueSummaryLabel.Text := BuildOverdueSummary(OverdueEntries, OverdueAllEntries)
    }

    OverdueList.Opt("-Redraw")
    OverdueList.Delete()

    selectedRow := 0
    for entry in OverdueEntries {
        row := OverdueList.Add("", entry.kindLabel, entry.vehicle.name, entry.vehicle.category, entry.vehicle.makeModel, entry.vehicle.plate, entry.term, entry.status)
        if (selectedKey != "" && BuildOverviewEntryKey(entry) = selectedKey) {
            selectedRow := row
        }
    }

    OverdueList.Opt("+Redraw")

    if IsObject(OverdueItemButton) {
        OverdueItemButton.Opt(OverdueEntries.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if IsObject(OverdueOpenButton) {
        OverdueOpenButton.Opt(OverdueEntries.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if IsObject(OverdueEditButton) {
        OverdueEditButton.Opt(OverdueEntries.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if (OverdueEntries.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    mode := focusList ? "Select Focus Vis" : "Select Vis"
    OverdueList.Modify(selectedRow, mode)
}

GetSelectedOverdueEntryKey() {
    global OverdueList, OverdueEntries

    if !IsObject(OverdueList) {
        return ""
    }

    row := OverdueList.GetNext(0)
    if !row || row > OverdueEntries.Length {
        return ""
    }

    return BuildOverviewEntryKey(OverdueEntries[row])
}

GetSelectedOverdueEntry(actionLabel := "otevřít") {
    global AppTitle, OverdueList, OverdueEntries

    if !IsObject(OverdueList) {
        return ""
    }

    row := OverdueList.GetNext(0)
    if !row || row > OverdueEntries.Length {
        MsgBox("Nejprve vyberte položku, kterou chcete " actionLabel ".", AppTitle, 0x40)
        return ""
    }

    return OverdueEntries[row]
}

OpenSelectedOverdueItem(*) {
    entry := GetSelectedOverdueEntry("otevřít")
    if !IsObject(entry) {
        return
    }

    CloseOverdueDialog()
    if (entry.kind = "custom" && entry.HasOwnProp("entryId") && entry.entryId != "") {
        OpenVehicleReminderDialog(entry.vehicle, false, entry.entryId)
        return
    }
    if (entry.kind = "maintenance" && entry.HasOwnProp("entryId") && entry.entryId != "") {
        OpenVehicleMaintenanceDialog(entry.vehicle, false, entry.entryId)
        return
    }

    OpenVehicleForm("edit", entry.vehicle)
}

OpenSelectedOverdueVehicle(*) {
    entry := GetSelectedOverdueEntry("zobrazit")
    if !IsObject(entry) {
        return
    }

    CloseOverdueDialog()
    OpenVehicleById(entry.vehicle.id, true)
}

EditSelectedOverdueVehicle(*) {
    entry := GetSelectedOverdueEntry("upravit")
    if !IsObject(entry) {
        return
    }

    CloseOverdueDialog()
    OpenVehicleForm("edit", entry.vehicle)
}

GetOverdueSearchText() {
    global OverdueSearchCtrl

    if !IsObject(OverdueSearchCtrl) {
        return ""
    }

    return Trim(OverdueSearchCtrl.Text)
}

BuildOverdueEntries() {
    global Vehicles

    entries := []
    technicalReminderDays := GetTechnicalReminderDays()
    greenCardReminderDays := GetGreenCardReminderDays()

    for vehicle in Vehicles {
        dueStamp := ParseDueStamp(vehicle.nextTk)
        if (dueStamp != "" && dueStamp < A_Now) {
            entries.Push({
                kind: "technical",
                kindLabel: "TK",
                vehicle: vehicle,
                term: vehicle.nextTk,
                status: GetExpirationStatusText(vehicle.nextTk, technicalReminderDays),
                dueStamp: dueStamp
            })
        }

        dueStamp := ParseDueStamp(vehicle.greenCardTo)
        if (dueStamp != "" && dueStamp < A_Now) {
            entries.Push({
                kind: "green",
                kindLabel: "ZK",
                vehicle: vehicle,
                term: vehicle.greenCardTo,
                status: GetExpirationStatusText(vehicle.greenCardTo, greenCardReminderDays),
                dueStamp: dueStamp
            })
        }
    }

    for item in GetUpcomingCustomReminders() {
        if (item.dueStamp < A_Now) {
            entries.Push({
                kind: "custom",
                kindLabel: "Připomínka",
                vehicle: item.vehicle,
                term: item.reminder.dueDate,
                status: GetReminderExpirationStatusText(item.reminder.dueDate, item.reminder.reminderDays + 0),
                dueStamp: item.dueStamp,
                entryId: item.reminder.id
            })
        }
    }

    for item in GetUpcomingVehicleMaintenance() {
        if IsVehicleMaintenanceSnapshotOverdue(item.snapshot) {
            entries.Push({
                kind: "maintenance",
                kindLabel: "Plán údržby",
                vehicle: item.vehicle,
                term: item.term,
                status: item.status,
                dueStamp: item.dueStamp,
                overviewSortKey: item.overviewSortKey,
                entryId: item.entryId
            })
        }
    }

    SortUpcomingByDue(&entries)
    return entries
}

BuildOverdueSummary(entries, allEntries) {
    vehicleIds := Map()
    overdueTechnical := 0
    overdueGreen := 0
    overdueCustom := 0
    overdueMaintenance := 0

    for entry in allEntries {
        vehicleIds[entry.vehicle.id] := true
        if (entry.kind = "technical") {
            overdueTechnical += 1
        } else if (entry.kind = "green") {
            overdueGreen += 1
        } else if (entry.kind = "maintenance") {
            overdueMaintenance += 1
        } else {
            overdueCustom += 1
        }
    }

    if (allEntries.Length = 0) {
        return "Momentálně nejsou žádné propadlé technické kontroly, zelené karty, vlastní připomínky ani plány údržby."
    }

    text := "Propadlých položek: " allEntries.Length " u " vehicleIds.Count " vozidel. "
    text .= "TK po termínu: " overdueTechnical ". ZK po termínu: " overdueGreen ". Připomínek po termínu: " overdueCustom ". Úkonů údržby po termínu: " overdueMaintenance "."
    if (entries.Length != allEntries.Length) {
        text .= " Zobrazeno po hledání: " entries.Length "."
    }

    return text
}

OpenPrintableVehicleReport(*) {
    global AppTitle

    reportPath := A_Temp "\Vehimap_tiskovy_prehled_" FormatTime(A_Now, "yyyyMMdd_HHmmss") ".html"
    try {
        WriteTextFileUtf8(reportPath, BuildPrintableVehicleReportHtml())
        Run('"' reportPath '"')
    } catch as err {
        MsgBox("Tiskový přehled se nepodařilo otevřít.`n`n" err.Message, AppTitle, 0x30)
    }
}

BuildPrintableVehicleReportHtml() {
    global Categories, Vehicles

    generatedAt := FormatTime(A_Now, "dd.MM.yyyy HH:mm")
    sections := []
    for category in Categories {
        sections.Push(BuildPrintableVehicleCategorySection(category))
    }

    html := "<!DOCTYPE html>"
    html .= "<html lang=cs>"
    html .= "<head>"
    html .= "<meta charset=utf-8>"
    html .= "<title>Vehimap - Tiskový přehled vozidel</title>"
    html .= "<style>"
    html .= "body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#111;background:#fff;}"
    html .= "h1{margin:0 0 8px 0;font-size:28px;}"
    html .= "h2{margin:28px 0 10px 0;font-size:20px;border-bottom:1px solid #bbb;padding-bottom:4px;}"
    html .= "p.meta{margin:0 0 18px 0;color:#444;}"
    html .= "table{width:100%;border-collapse:collapse;margin-bottom:18px;}"
    html .= "th,td{border:1px solid #b9b9b9;padding:6px 8px;vertical-align:top;text-align:left;font-size:13px;}"
    html .= "th{background:#efefef;font-weight:600;}"
    html .= "p.empty{margin:8px 0 18px 0;color:#555;}"
    html .= "@media print{body{margin:12mm;}a{text-decoration:none;color:inherit;}}"
    html .= "</style>"
    html .= "</head>"
    html .= "<body>"
    html .= "<h1>Vehimap - Tiskový přehled vozidel</h1>"
    html .= "<p class=meta>Vytvořeno: " HtmlEscape(generatedAt) " | Celkem vozidel: " Vehicles.Length "</p>"
    html .= JoinLines(sections, "")
    html .= "</body></html>"
    return html
}

BuildPrintableVehicleCategorySection(category) {
    global Vehicles

    items := []
    for vehicle in Vehicles {
        if (vehicle.category = category) {
            items.Push(vehicle)
        }
    }

    SortVehiclesByDue(&items)

    title := "<h2>" HtmlEscape(category) " (" items.Length ")</h2>"
    if (items.Length = 0) {
        return title "<p class=empty>V této kategorii není žádné vozidlo.</p>"
    }

    rows := []
    for vehicle in items {
        meta := GetVehicleMeta(vehicle.id)
        rows.Push(
            "<tr>"
            "<td>" HtmlEscape(vehicle.name) "</td>"
            "<td>" HtmlEscape(vehicle.vehicleNote) "</td>"
            "<td>" HtmlEscape(vehicle.makeModel) "</td>"
            "<td>" HtmlEscape(vehicle.plate) "</td>"
            "<td>" HtmlEscape(vehicle.year) "</td>"
            "<td>" HtmlEscape(vehicle.power) "</td>"
            "<td>" HtmlEscape(meta.state) "</td>"
            "<td>" HtmlEscape(meta.tags) "</td>"
            "<td>" HtmlEscape(vehicle.lastTk) "</td>"
            "<td>" HtmlEscape(vehicle.nextTk) "</td>"
            "<td>" HtmlEscape(vehicle.greenCardTo) "</td>"
            "<td>" HtmlEscape(GetVehicleStatusText(vehicle)) "</td>"
            "</tr>"
        )
    }

    html := title
    html .= "<table>"
    html .= "<thead><tr><th>Název</th><th>Poznámka</th><th>Značka / model</th><th>SPZ</th><th>Rok výroby</th><th>Výkon</th><th>Stav vozidla</th><th>Štítky</th><th>Poslední TK</th><th>Příští TK</th><th>Zelená karta do</th><th>Stav</th></tr></thead>"
    html .= "<tbody>" JoinLines(rows, "") "</tbody>"
    html .= "</table>"
    return html
}

HtmlEscape(text) {
    text := StrReplace(text, "&", "&amp;")
    text := StrReplace(text, "<", "&lt;")
    text := StrReplace(text, ">", "&gt;")
    text := StrReplace(text, '"', "&quot;")
    return StrReplace(text, "'", "&#39;")
}

OpenSelectedOverviewItem(*) {
    entry := GetSelectedOverviewEntry("otevřít")
    if !IsObject(entry) {
        return
    }

    CloseUpcomingOverviewDialog()
    if (entry.HasOwnProp("isAuditIssue") && entry.isAuditIssue) {
        OpenVehimapAuditItem(entry)
        return
    }

    switch entry.kind {
        case "custom":
            if (entry.HasOwnProp("entryId") && entry.entryId != "") {
                OpenVehicleReminderDialog(entry.vehicle, false, entry.entryId)
                return
            }
        case "maintenance":
            if (entry.HasOwnProp("entryId") && entry.entryId != "") {
                OpenVehicleMaintenanceDialog(entry.vehicle, false, entry.entryId)
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

OpenSelectedOverviewVehicle(*) {
    entry := GetSelectedOverviewEntry("zobrazit")
    if !IsObject(entry) {
        return
    }

    CloseUpcomingOverviewDialog()
    OpenVehicleById(entry.vehicle.id, true)
}

EditSelectedOverviewVehicle(*) {
    entry := GetSelectedOverviewEntry("upravit")
    if !IsObject(entry) {
        return
    }

    CloseUpcomingOverviewDialog()
    OpenVehicleForm("edit", entry.vehicle)
}

OnOverviewFilterChanged(*) {
    selectedKey := GetSelectedOverviewEntryKey()
    SaveOverviewFilterSetting(GetOverviewFilterKind())
    PopulateUpcomingOverviewList(selectedKey)
}

OnOverviewShowMissingGreenChanged(*) {
    SaveOverviewIncludeMissingGreenSetting(ShouldShowMissingGreenCardsInOverview())
    RefreshUpcomingOverviewDialog()
}

OnOverviewShowDataIssuesChanged(*) {
    SaveOverviewIncludeDataIssuesSetting(ShouldShowDataIssuesInOverview())
    RefreshUpcomingOverviewDialog()
}

OnOverviewSearchChanged(*) {
    selectedKey := GetSelectedOverviewEntryKey()
    PopulateUpcomingOverviewList(selectedKey)
}

OnOverviewColumnClick(ctrl, column) {
    global OverviewSortColumn, OverviewSortDescending

    if (OverviewSortColumn = column) {
        OverviewSortDescending := !OverviewSortDescending
    } else {
        OverviewSortColumn := column
        OverviewSortDescending := false
    }

    SaveOverviewSortSettings(OverviewSortColumn, OverviewSortDescending)
    selectedKey := GetSelectedOverviewEntryKey()
    PopulateUpcomingOverviewList(selectedKey)
}

RefreshUpcomingOverviewDialog(*) {
    global OverviewGui, OverviewAllEntries

    if !IsObject(OverviewGui) {
        return
    }

    selectedKey := GetSelectedOverviewEntryKey()
    OverviewAllEntries := BuildUpcomingOverviewEntries(ShouldShowMissingGreenCardsInOverview(), ShouldShowDataIssuesInOverview())
    PopulateUpcomingOverviewList(selectedKey)
}

PopulateUpcomingOverviewList(selectedKey := "", focusList := false) {
    global OverviewAllEntries, OverviewEntries, OverviewList, OverviewSummaryLabel, OverviewItemButton, OverviewOpenButton, OverviewEditButton

    if !IsObject(OverviewList) {
        return
    }

    filterKind := GetOverviewFilterKind()
    OverviewEntries := FilterUpcomingOverviewEntries(OverviewAllEntries, filterKind)
    OverviewEntries := FilterOverviewEntriesBySearch(OverviewEntries, GetOverviewSearchText())
    SortOverviewEntries(&OverviewEntries)

    if IsObject(OverviewSummaryLabel) {
        OverviewSummaryLabel.Text := BuildUpcomingOverviewSummary(OverviewEntries, OverviewAllEntries)
    }

    OverviewList.Opt("-Redraw")
    OverviewList.Delete()

    selectedRow := 0
    for entry in OverviewEntries {
        row := OverviewList.Add("", entry.kindLabel, entry.vehicle.name, entry.vehicle.category, entry.vehicle.makeModel, entry.vehicle.plate, entry.term, entry.status)
        if (selectedKey != "" && BuildOverviewEntryKey(entry) = selectedKey) {
            selectedRow := row
        }
    }

    OverviewList.Opt("+Redraw")

    if IsObject(OverviewItemButton) {
        if (OverviewEntries.Length = 0) {
            OverviewItemButton.Opt("+Disabled")
        } else {
            OverviewItemButton.Opt("-Disabled")
        }
    }

    if IsObject(OverviewOpenButton) {
        if (OverviewEntries.Length = 0) {
            OverviewOpenButton.Opt("+Disabled")
        } else {
            OverviewOpenButton.Opt("-Disabled")
        }
    }

    if IsObject(OverviewEditButton) {
        if (OverviewEntries.Length = 0) {
            OverviewEditButton.Opt("+Disabled")
        } else {
            OverviewEditButton.Opt("-Disabled")
        }
    }

    if (OverviewEntries.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    mode := focusList ? "Select Focus Vis" : "Select Vis"
    OverviewList.Modify(selectedRow, mode)
}

GetSelectedOverviewEntryKey() {
    global OverviewList, OverviewEntries

    if !IsObject(OverviewList) {
        return ""
    }

    row := OverviewList.GetNext(0)
    if !row || row > OverviewEntries.Length {
        return ""
    }

    return BuildOverviewEntryKey(OverviewEntries[row])
}

GetSelectedOverviewEntry(actionLabel := "otevřít") {
    global AppTitle, OverviewList, OverviewEntries

    if !IsObject(OverviewList) {
        return ""
    }

    row := OverviewList.GetNext(0)
    if !row || row > OverviewEntries.Length {
        MsgBox("Nejprve vyberte položku, kterou chcete " actionLabel ".", AppTitle, 0x40)
        return ""
    }

    return OverviewEntries[row]
}

ShouldShowMissingGreenCardsInOverview() {
    global OverviewShowMissingGreenCtrl

    if IsObject(OverviewShowMissingGreenCtrl) {
        return OverviewShowMissingGreenCtrl.Value = 1
    }

    return GetOverviewIncludeMissingGreenSetting()
}

ShouldShowDataIssuesInOverview() {
    global OverviewShowDataIssuesCtrl

    if IsObject(OverviewShowDataIssuesCtrl) {
        return OverviewShowDataIssuesCtrl.Value = 1
    }

    return GetOverviewIncludeDataIssuesSetting()
}

GetOverviewSearchText() {
    global OverviewSearchCtrl

    if !IsObject(OverviewSearchCtrl) {
        return ""
    }

    return Trim(OverviewSearchCtrl.Text)
}

GetOverviewFilterKind() {
    global OverviewFilterCtrl

    if !IsObject(OverviewFilterCtrl) {
        return GetOverviewFilterSetting()
    }

    text := OverviewFilterCtrl.Text
    if (text = "Jen technické kontroly") {
        return "technical"
    }
    if (text = "Jen zelené karty") {
        return "green"
    }
    if (text = "Jen vlastní připomínky") {
        return "custom"
    }
    if (text = "Jen plány údržby") {
        return "maintenance"
    }
    if (text = "Jen datové nedostatky") {
        return "data_issue"
    }

    return "all"
}

GetOverviewFilterSetting() {
    global SettingsFile

    value := IniRead(SettingsFile, "overview", "filter", "all")
    if (value != "technical" && value != "green" && value != "custom" && value != "maintenance" && value != "data_issue") {
        return "all"
    }

    return value
}

GetOverviewFilterIndex() {
    filterKind := GetOverviewFilterSetting()
    if (filterKind = "technical") {
        return 2
    }
    if (filterKind = "green") {
        return 3
    }
    if (filterKind = "custom") {
        return 4
    }
    if (filterKind = "maintenance") {
        return 5
    }
    if (filterKind = "data_issue") {
        return 6
    }

    return 1
}

GetOverviewIncludeMissingGreenSetting() {
    global SettingsFile

    return IniRead(SettingsFile, "overview", "include_missing_green", "0") = "1" ? 1 : 0
}

GetOverviewIncludeDataIssuesSetting() {
    global SettingsFile

    return IniRead(SettingsFile, "overview", "include_data_issues", "0") = "1" ? 1 : 0
}

GetOverviewSortColumnSetting() {
    global SettingsFile

    column := IniRead(SettingsFile, "overview", "sort_column", "6") + 0
    if (column < 1 || column > 7) {
        return 6
    }

    return column
}

GetOverviewSortDescendingSetting() {
    global SettingsFile

    return IniRead(SettingsFile, "overview", "sort_descending", "0") = "1"
}

SaveOverviewFilterSetting(filterKind) {
    global SettingsFile

    if (filterKind != "technical" && filterKind != "green" && filterKind != "custom" && filterKind != "maintenance" && filterKind != "data_issue") {
        filterKind := "all"
    }

    IniWrite(filterKind, SettingsFile, "overview", "filter")
}

SaveOverviewIncludeMissingGreenSetting(enabled) {
    global SettingsFile

    IniWrite(enabled ? "1" : "0", SettingsFile, "overview", "include_missing_green")
}

SaveOverviewIncludeDataIssuesSetting(enabled) {
    global SettingsFile

    IniWrite(enabled ? "1" : "0", SettingsFile, "overview", "include_data_issues")
}

SaveOverviewSortSettings(column, descending) {
    global SettingsFile

    if (column < 1 || column > 7) {
        column := 6
    }

    IniWrite(column, SettingsFile, "overview", "sort_column")
    IniWrite(descending ? "1" : "0", SettingsFile, "overview", "sort_descending")
}

GetListSortColumnSetting(sectionName, defaultColumn, maxColumn) {
    global SettingsFile

    column := IniRead(SettingsFile, sectionName, "sort_column", defaultColumn) + 0
    if (column < 1 || column > maxColumn) {
        return defaultColumn
    }

    return column
}

GetListSortDescendingSetting(sectionName, defaultDescending := false) {
    global SettingsFile

    return IniRead(SettingsFile, sectionName, "sort_descending", defaultDescending ? "1" : "0") = "1"
}

SaveListSortSettings(sectionName, column, descending, defaultColumn, maxColumn) {
    global SettingsFile

    if (column < 1 || column > maxColumn) {
        column := defaultColumn
    }

    IniWrite(column, SettingsFile, sectionName, "sort_column")
    IniWrite(descending ? "1" : "0", SettingsFile, sectionName, "sort_descending")
}

GetHistorySortColumnSetting() {
    return GetListSortColumnSetting("history_view", 1, 5)
}

GetHistorySortDescendingSetting() {
    return GetListSortDescendingSetting("history_view", true)
}

SaveHistorySortSettings(column, descending) {
    SaveListSortSettings("history_view", column, descending, 1, 5)
}

GetFuelSortColumnSetting() {
    return GetListSortColumnSetting("fuel_view", 1, 7)
}

GetFuelSortDescendingSetting() {
    return GetListSortDescendingSetting("fuel_view", true)
}

SaveFuelSortSettings(column, descending) {
    SaveListSortSettings("fuel_view", column, descending, 1, 7)
}

GetRecordsSortColumnSetting() {
    return GetListSortColumnSetting("records_view", 4, 7)
}

GetRecordsSortDescendingSetting() {
    return GetListSortDescendingSetting("records_view", false)
}

SaveRecordsSortSettings(column, descending) {
    SaveListSortSettings("records_view", column, descending, 4, 7)
}

GetReminderSortColumnSetting() {
    return GetListSortColumnSetting("reminder_view", 2, 6)
}

GetReminderSortDescendingSetting() {
    return GetListSortDescendingSetting("reminder_view", false)
}

SaveReminderSortSettings(column, descending) {
    SaveListSortSettings("reminder_view", column, descending, 2, 6)
}

GetMaintenanceSortColumnSetting() {
    return GetListSortColumnSetting("maintenance_view", 5, 6)
}

GetMaintenanceSortDescendingSetting() {
    return GetListSortDescendingSetting("maintenance_view", false)
}

SaveMaintenanceSortSettings(column, descending) {
    SaveListSortSettings("maintenance_view", column, descending, 5, 6)
}

FilterUpcomingOverviewEntries(entries, filterKind := "all") {
    filtered := []

    for entry in entries {
        if (
            filterKind = "all"
            || entry.kind = filterKind
            || (filterKind = "data_issue" && IsOverviewDataIssueEntry(entry))
        ) {
            filtered.Push(entry)
        }
    }

    return filtered
}

FilterOverviewEntriesBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    if (needle = "") {
        for entry in entries {
            filtered.Push(entry)
        }
        return filtered
    }

    for entry in entries {
        haystack := StrLower(
            entry.kindLabel " "
            entry.vehicle.name " "
            entry.vehicle.category " "
            entry.vehicle.makeModel " "
            entry.vehicle.plate " "
            entry.term " "
            entry.status
        )
        if InStr(haystack, needle) {
            filtered.Push(entry)
        }
    }

    return filtered
}

BuildOverviewEntryKey(entry) {
    if (entry.HasOwnProp("auditKey") && entry.auditKey != "") {
        return entry.auditKey
    }

    if (entry.HasOwnProp("entryId")) {
        return entry.kind "|" entry.vehicle.id "|" entry.entryId
    }

    return entry.kind "|" entry.vehicle.id "|" entry.term
}

IsOverviewDataIssueEntry(entry) {
    return entry.HasOwnProp("isAuditIssue") && entry.isAuditIssue
}
