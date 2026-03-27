OpenAuditDialog(*) {
    global AppTitle, MainGui, AuditGui, AuditList, AuditSummaryLabel, AuditSearchCtrl, AuditItems, AuditOpenButton, AuditVehicleButton, AuditEditButton
    global DashboardGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui, FleetCostGui, MaintenanceGui, MaintenanceFormGui, MaintenanceCompleteGui

    if IsObject(AuditGui) {
        WinActivate("ahk_id " AuditGui.Hwnd)
        return
    }

    for guiRef in [DashboardGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui, FleetCostGui, MaintenanceGui, MaintenanceFormGui, MaintenanceCompleteGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    ShowMainWindow()

    AuditItems := []
    AuditGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Audit dat")
    AuditGui.SetFont("s10", "Segoe UI")
    AuditGui.OnEvent("Close", CloseAuditDialog)
    AuditGui.OnEvent("Escape", CloseAuditDialog)

    MainGui.Opt("+Disabled")

    AuditGui.AddText("x20 y20 w980", "Audit dat hlĂ„â€šĂ‚Â­dĂ„â€šĂ‹â€ˇ chybÄ‚â€žĂ˘â‚¬ĹźjĂ„â€šĂ‚Â­cĂ„â€šĂ‚Â­ Ă„â€šÄąĹşdaje, neplatnĂ„â€šĂ‚Â© rozsahy, problematickĂ„â€šĂ‚Â© doklady, nekonzistentnĂ„â€šĂ‚Â­ tachometr i poloĂ„Ä…Ă„Äľky, kterĂ„â€šĂ‚Â© se kvĂ„Ä…ÄąÂ»li datĂ„Ä…ÄąÂ»m nedajĂ„â€šĂ‚Â­ spolehlivÄ‚â€žĂ˘â‚¬Ĺź zapoÄ‚â€žÄąÂ¤Ă„â€šĂ‚Â­tat.")
    AuditSummaryLabel := AuditGui.AddText("x20 y50 w980 h32", "")
    AuditGui.AddText("x20 y88 w300", "Hledat evidenci, vozidlo, problĂ„â€šĂ‚Â©m nebo stav")
    AuditSearchCtrl := AuditGui.AddEdit("x330 y85 w320")
    AuditSearchCtrl.OnEvent("Change", OnAuditSearchChanged)

    AuditList := AuditGui.AddListView("x20 y120 w980 h290 Grid -Multi", ["ZĂ„â€šĂ‹â€ˇvaĂ„Ä…Ă„Äľnost", "Evidence", "Vozidlo", "SPZ", "PoloĂ„Ä…Ă„Äľka", "Stav", "Popis"] )
    AuditList.OnEvent("DoubleClick", OpenSelectedAuditItem)
    AuditList.OnEvent("ItemSelect", OnAuditSelectionChanged)
    AuditList.ModifyCol(1, "90")
    AuditList.ModifyCol(2, "135")
    AuditList.ModifyCol(3, "170")
    AuditList.ModifyCol(4, "85")
    AuditList.ModifyCol(5, "150")
    AuditList.ModifyCol(6, "125")
    AuditList.ModifyCol(7, "330")

    AuditGui.AddGroupBox("x20 y425 w980 h100", "Akce")

    AuditOpenButton := AuditGui.AddButton("x40 y455 w140 h30 Default", "OtevĂ„Ä…Ă˘â€žËĂ„â€šĂ‚Â­t poloĂ„Ä…Ă„Äľku")
    AuditOpenButton.OnEvent("Click", OpenSelectedAuditItem)

    AuditVehicleButton := AuditGui.AddButton("x190 y455 w140 h30", "Zobrazit vozidlo")
    AuditVehicleButton.OnEvent("Click", OpenSelectedAuditVehicle)

    AuditEditButton := AuditGui.AddButton("x340 y455 w140 h30", "Upravit")
    AuditEditButton.OnEvent("Click", EditSelectedAuditItem)

    closeButton := AuditGui.AddButton("x860 y455 w100 h30", "ZavĂ„Ä…Ă˘â€žËĂ„â€šĂ‚Â­t")
    closeButton.OnEvent("Click", CloseAuditDialog)

    AuditGui.Show("w1020 h545")
    PopulateAuditList(true)
    if (AuditItems.Length = 0) {
        closeButton.Focus()
    }
}

CloseAuditDialog(*) {
    global AuditGui, AuditList, AuditSummaryLabel, AuditSearchCtrl, AuditItems, AuditOpenButton, AuditVehicleButton, AuditEditButton, MainGui

    if IsObject(AuditGui) {
        AuditGui.Destroy()
        AuditGui := 0
    }

    AuditList := 0
    AuditSummaryLabel := 0
    AuditSearchCtrl := 0
    AuditItems := []
    AuditOpenButton := 0
    AuditVehicleButton := 0
    AuditEditButton := 0
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateAuditList(focusList := false) {
    global AuditGui, AuditList, AuditSummaryLabel, AuditItems

    if !IsObject(AuditGui) || !IsObject(AuditList) {
        return
    }

    AuditItems := BuildVehimapAuditItems()
    AuditItems := FilterVehimapAuditItems(AuditItems, GetAuditSearchText())
    SortVehimapAuditItems(&AuditItems)

    if IsObject(AuditSummaryLabel) {
        AuditSummaryLabel.Text := BuildVehimapAuditSummaryText(BuildVehimapAuditItems(), AuditItems)
    }

    AuditList.Opt("-Redraw")
    AuditList.Delete()
    for item in AuditItems {
        plateText := Trim(item.vehicle.plate) = "" ? "-" : item.vehicle.plate
        AuditList.Add("", item.severityLabel, item.sourceLabel, item.vehicle.name, plateText, item.term, item.status, item.description)
    }
    AuditList.Opt("+Redraw")

    UpdateAuditActionState()
    if (AuditItems.Length = 0) {
        return
    }

    AuditList.Modify(1, focusList ? "Select Focus Vis" : "Select Vis")
    UpdateAuditActionState()
}

OnAuditSearchChanged(*) {
    PopulateAuditList()
}

GetAuditSearchText() {
    global AuditSearchCtrl

    if !IsObject(AuditSearchCtrl) {
        return ""
    }

    return Trim(AuditSearchCtrl.Text)
}

FilterVehimapAuditItems(items, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for item in items {
        haystack := StrLower(
            item.severityLabel " "
            item.sourceLabel " "
            item.vehicle.name " "
            item.vehicle.category " "
            item.vehicle.plate " "
            item.term " "
            item.status " "
            item.description " "
            item.kindLabel
        )
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(item)
        }
    }

    return filtered
}

GetSelectedAuditItem(actionLabel := "otevĂ„Ä…Ă˘â€žËĂ„â€šĂ‚Â­t") {
    global AppTitle, AuditList, AuditItems

    if !IsObject(AuditList) {
        return ""
    }

    row := AuditList.GetNext(0)
    if !row || row > AuditItems.Length {
        MsgBox("Nejprve vyberte poloĂ„Ä…Ă„Äľku, kterou chcete " actionLabel ".", AppTitle, 0x40)
        return ""
    }

    return AuditItems[row]
}

OnAuditSelectionChanged(*) {
    UpdateAuditActionState()
}

UpdateAuditActionState() {
    global AuditOpenButton, AuditVehicleButton, AuditEditButton

    item := GetSelectedAuditItemOrEmpty()
    hasItem := IsObject(item)

    if IsObject(AuditOpenButton) {
        AuditOpenButton.Opt(hasItem ? "-Disabled" : "+Disabled")
    }
    if IsObject(AuditVehicleButton) {
        AuditVehicleButton.Opt(hasItem ? "-Disabled" : "+Disabled")
    }
    if IsObject(AuditEditButton) {
        AuditEditButton.Opt(hasItem ? "-Disabled" : "+Disabled")
    }
}

GetSelectedAuditItemOrEmpty() {
    global AuditList, AuditItems

    if !IsObject(AuditList) {
        return ""
    }

    row := AuditList.GetNext(0)
    if !row || row > AuditItems.Length {
        return ""
    }

    return AuditItems[row]
}

OpenSelectedAuditItem(*) {
    item := GetSelectedAuditItem("otevĂ„Ä…Ă˘â€žËĂ„â€šĂ‚Â­t")
    if !IsObject(item) {
        return
    }

    CloseAuditDialog()
    OpenVehimapAuditItem(item)
}

OpenSelectedAuditVehicle(*) {
    item := GetSelectedAuditItem("zobrazit")
    if !IsObject(item) {
        return
    }

    CloseAuditDialog()
    OpenVehicleById(item.vehicle.id, true)
}

EditSelectedAuditItem(*) {
    item := GetSelectedAuditItem("upravit")
    if !IsObject(item) {
        return
    }

    CloseAuditDialog()
    EditVehimapAuditItem(item)
}

OpenVehimapAuditItem(item) {
    switch item.actionKind {
        case "vehicle":
            OpenVehicleById(item.vehicle.id, true)
        case "record":
            OpenVehicleRecordsDialog(item.vehicle, false, item.entryId)
        case "history":
            OpenVehicleHistoryDialog(item.vehicle, false, item.entryId)
        case "fuel":
            OpenVehicleFuelDialog(item.vehicle, false, item.entryId)
        case "maintenance":
            OpenVehicleMaintenanceDialog(item.vehicle, false, item.entryId)
        case "costs":
            OpenVehicleCostSummaryDialog(item.vehicle)
        default:
            OpenVehicleForm("edit", item.vehicle)
    }
}

EditVehimapAuditItem(item) {
    switch item.actionKind {
        case "record":
            entry := FindVehicleRecordById(item.entryId)
            OpenVehicleRecordsDialog(item.vehicle, false, item.entryId)
            if IsObject(entry) {
                OpenVehicleRecordForm("edit", entry)
                return
            }
        case "history":
            entry := FindVehicleHistoryEventById(item.entryId)
            OpenVehicleHistoryDialog(item.vehicle, false, item.entryId)
            if IsObject(entry) {
                OpenVehicleHistoryEventForm("edit", entry)
                return
            }
        case "fuel":
            entry := FindVehicleFuelEntryById(item.entryId)
            OpenVehicleFuelDialog(item.vehicle, false, item.entryId)
            if IsObject(entry) {
                OpenVehicleFuelEntryForm("edit", entry)
                return
            }
        case "maintenance":
            entry := FindVehicleMaintenancePlanById(item.entryId)
            OpenVehicleMaintenanceDialog(item.vehicle, false, item.entryId)
            if IsObject(entry) {
                OpenVehicleMaintenancePlanForm("edit", entry)
                return
            }
        default:
            OpenVehicleForm("edit", item.vehicle)
    }
}

BuildVehimapAuditItems(vehicleId := "") {
    global Vehicles, VehicleRecords, VehicleHistory, VehicleFuelLog, VehicleMaintenancePlans

    items := []
    for vehicle in Vehicles {
        if (vehicleId != "" && vehicle.id != vehicleId) {
            continue
        }

        if !IsVehicleInactive(vehicle) {
            if (Trim(vehicle.plate) = "") {
                items.Push(BuildVehimapVehicleAuditItem(vehicle, "plate"))
            }
            if (Trim(vehicle.nextTk) = "") {
                items.Push(BuildVehimapVehicleAuditItem(vehicle, "next_tk"))
            }
        }

        if VehicleHasInvalidGreenCardRange(vehicle) {
            items.Push(BuildVehimapVehicleAuditItem(vehicle, "green_range"))
        }

        for item in BuildVehicleOdometerAuditItems(vehicle) {
            items.Push(item)
        }
    }

    for entry in VehicleRecords {
        if (vehicleId != "" && entry.vehicleId != vehicleId) {
            continue
        }

        vehicle := FindVehicleById(entry.vehicleId)
        if !IsObject(vehicle) {
            continue
        }

        pathInfo := GetVehicleRecordPathInfo(entry)
        if (pathInfo.kind = "empty" || pathInfo.kind = "missing_file" || pathInfo.kind = "missing_folder") {
            items.Push(BuildVehimapRecordPathAuditItem(vehicle, entry, pathInfo))
        }

        if RecordHasInvalidValidityRange(entry) {
            items.Push(BuildVehimapRecordRangeAuditItem(vehicle, entry))
        }

        if (Trim(entry.price) != "") {
            if !TryParseMoneyAmount(entry.price, &amount) {
                items.Push(BuildVehimapCostAuditItem(vehicle, "record", entry.id, entry.title, "Doklad", "NeÄ‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnĂ„â€šĂ‹â€ˇ Ä‚â€žÄąÂ¤Ă„â€šĂ‹â€ˇstka", "Cena nebo Ä‚â€žÄąÂ¤Ă„â€šĂ‹â€ˇstka nenĂ„â€šĂ‚Â­ v Ä‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnĂ„â€šĂ‚Â©m formĂ„â€šĂ‹â€ˇtu, proto se nedĂ„â€šĂ‹â€ˇ zapoÄ‚â€žÄąÂ¤Ă„â€šĂ‚Â­tat do nĂ„â€šĂ‹â€ˇkladĂ„Ä…ÄąÂ»."))
            } else if !TryGetRecordYearMonth(entry, &entryYear, &entryMonth) {
                items.Push(BuildVehimapCostAuditItem(vehicle, "record", entry.id, entry.title, "Doklad", "ChybĂ„â€šĂ‚Â­ pouĂ„Ä…Ă„ÄľitelnĂ„â€šĂ‚Â© datum", "PoloĂ„Ä…Ă„Äľka mĂ„â€šĂ‹â€ˇ Ä‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnou Ä‚â€žÄąÂ¤Ă„â€šĂ‹â€ˇstku, ale chybĂ„â€šĂ‚Â­ datum platnosti, podle kterĂ„â€šĂ‚Â©ho by se dala zaĂ„Ä…Ă˘â€žËadit do obdobĂ„â€šĂ‚Â­."))
            }
        }
    }

    for entry in VehicleHistory {
        if (vehicleId != "" && entry.vehicleId != vehicleId) {
            continue
        }
        if (Trim(entry.cost) = "") {
            continue
        }

        vehicle := FindVehicleById(entry.vehicleId)
        if !IsObject(vehicle) {
            continue
        }

        title := Trim(entry.eventType) != "" ? entry.eventType : "(bez nĂ„â€šĂ‹â€ˇzvu)"
        if !TryParseMoneyAmount(entry.cost, &amount) {
            items.Push(BuildVehimapCostAuditItem(vehicle, "history", entry.id, title, "Historie", "NeÄ‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnĂ„â€šĂ‹â€ˇ Ä‚â€žÄąÂ¤Ă„â€šĂ‹â€ˇstka", "Cena udĂ„â€šĂ‹â€ˇlosti nenĂ„â€šĂ‚Â­ v Ä‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnĂ„â€šĂ‚Â©m formĂ„â€šĂ‹â€ˇtu, proto se nedĂ„â€šĂ‹â€ˇ zapoÄ‚â€žÄąÂ¤Ă„â€šĂ‚Â­tat do nĂ„â€šĂ‹â€ˇkladĂ„Ä…ÄąÂ»."))
        } else if !TryGetEventYearMonth(entry.eventDate, &entryYear, &entryMonth) {
            items.Push(BuildVehimapCostAuditItem(vehicle, "history", entry.id, title, "Historie", "ChybĂ„â€šĂ‚Â­ pouĂ„Ä…Ă„ÄľitelnĂ„â€šĂ‚Â© datum", "UdĂ„â€šĂ‹â€ˇlost mĂ„â€šĂ‹â€ˇ Ä‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnou Ä‚â€žÄąÂ¤Ă„â€šĂ‹â€ˇstku, ale chybĂ„â€šĂ‚Â­ validnĂ„â€šĂ‚Â­ datum pro zaĂ„Ä…Ă˘â€žËazenĂ„â€šĂ‚Â­ do obdobĂ„â€šĂ‚Â­."))
        }
    }

    for entry in VehicleFuelLog {
        if (vehicleId != "" && entry.vehicleId != vehicleId) {
            continue
        }
        if (Trim(entry.totalCost) = "") {
            continue
        }

        vehicle := FindVehicleById(entry.vehicleId)
        if !IsObject(vehicle) {
            continue
        }

        title := Trim(entry.fuelType) != "" ? entry.fuelType : "TankovĂ„â€šĂ‹â€ˇnĂ„â€šĂ‚Â­"
        if !TryParseMoneyAmount(entry.totalCost, &amount) {
            items.Push(BuildVehimapCostAuditItem(vehicle, "fuel", entry.id, title, "TankovĂ„â€šĂ‹â€ˇnĂ„â€šĂ‚Â­", "NeÄ‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnĂ„â€šĂ‹â€ˇ Ä‚â€žÄąÂ¤Ă„â€šĂ‹â€ˇstka", "Cena tankovĂ„â€šĂ‹â€ˇnĂ„â€šĂ‚Â­ nenĂ„â€šĂ‚Â­ v Ä‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnĂ„â€šĂ‚Â©m formĂ„â€šĂ‹â€ˇtu, proto se nedĂ„â€šĂ‹â€ˇ zapoÄ‚â€žÄąÂ¤Ă„â€šĂ‚Â­tat do nĂ„â€šĂ‹â€ˇkladĂ„Ä…ÄąÂ»."))
        } else if !TryGetEventYearMonth(entry.entryDate, &entryYear, &entryMonth) {
            items.Push(BuildVehimapCostAuditItem(vehicle, "fuel", entry.id, title, "TankovĂ„â€šĂ‹â€ˇnĂ„â€šĂ‚Â­", "ChybĂ„â€šĂ‚Â­ pouĂ„Ä…Ă„ÄľitelnĂ„â€šĂ‚Â© datum", "ZĂ„â€šĂ‹â€ˇznam tankovĂ„â€šĂ‹â€ˇnĂ„â€šĂ‚Â­ mĂ„â€šĂ‹â€ˇ Ä‚â€žÄąÂ¤Ă„â€šĂ‚Â­selnou Ä‚â€žÄąÂ¤Ă„â€šĂ‹â€ˇstku, ale chybĂ„â€šĂ‚Â­ validnĂ„â€šĂ‚Â­ datum pro zaĂ„Ä…Ă˘â€žËazenĂ„â€šĂ‚Â­ do obdobĂ„â€šĂ‚Â­."))
        }
    }

    for plan in VehicleMaintenancePlans {
        if (vehicleId != "" && plan.vehicleId != vehicleId) {
            continue
        }
        if (!plan.isActive || Trim(plan.intervalKm) = "") {
            continue
        }

        vehicle := FindVehicleById(plan.vehicleId)
        if !IsObject(vehicle) {
            continue
        }

        snapshot := BuildVehicleMaintenancePlanSnapshot(plan, vehicle)
        if (snapshot.requiresCurrentOdometer && snapshot.currentOdometer = "") {
            items.Push(BuildVehimapMaintenanceAuditItem(vehicle, plan, snapshot))
        }
    }

    return items
}

BuildVehimapVehicleAuditItem(vehicle, fieldKind) {
    switch fieldKind {
        case "plate":
            return BuildVehimapAuditEntry(
                "vehicle_field",
                "Upozornění",
                "Vozidlo",
                vehicle,
                "SPZ",
                "Doplnit v editaci",
                "Aktivní vozidlo nemá vyplněnou registrační značku.",
                "vehicle",
                "",
                "Chybějící SPZ",
                "warning",
                "10"
            )
        case "next_tk":
            return BuildVehimapAuditEntry(
                "vehicle_field",
                "Chyba",
                "Vozidlo",
                vehicle,
                "Příští TK",
                "Doplnit v editaci",
                "Aktivní vozidlo nemá vyplněný termín příští technické kontroly.",
                "vehicle",
                "",
                "Chybějící příští TK",
                "error",
                "05"
            )
        default:
            return BuildVehimapAuditEntry(
                "vehicle_green_range",
                "Chyba",
                "Vozidlo",
                vehicle,
                "Zelená karta",
                "Zkontrolovat rozsah",
                "Rozsah zelené karty je neplatný, protože pole Platné od je později než pole Platné do.",
                "vehicle",
                "",
                "Neplatný rozsah zelené karty",
                "error",
                "04"
            )
    }
}

BuildVehimapRecordPathAuditItem(vehicle, entry, pathInfo) {
    title := Trim(entry.title)
    if (title = "") {
        title := "(bez názvu)"
    }

    description := ""
    switch pathInfo.kind {
        case "empty":
            description := "Doklad nemá vyplněnou přílohu ani cestu k souboru."
        case "missing_file":
            description := "Složka existuje, ale soubor dokladu na zadané cestě chybí."
        default:
            description := "Cesta k dokladu míří do neexistující složky nebo na nedostupný soubor."
    }

    return BuildVehimapAuditEntry(
        "record_path",
        (pathInfo.kind = "empty") ? "Upozornění" : "Chyba",
        "Doklad",
        vehicle,
        title,
        GetVehicleRecordPathStateLabel(pathInfo.kind),
        description,
        "record",
        entry.id,
        "Doklad / příloha",
        (pathInfo.kind = "empty") ? "warning" : "error",
        (pathInfo.kind = "empty") ? "30" : "20"
    )
}

BuildVehimapRecordRangeAuditItem(vehicle, entry) {
    title := Trim(entry.title)
    if (title = "") {
        title := "(bez názvu)"
    }

    return BuildVehimapAuditEntry(
        "record_range",
        "Chyba",
        "Doklad",
        vehicle,
        title,
        "Zkontrolovat platnost",
        "Pole Platné od je později než pole Platné do.",
        "record",
        entry.id,
        "Neplatný rozsah platnosti",
        "error",
        "15"
    )
}

BuildVehimapCostAuditItem(vehicle, sourceKind, entryId, title, sourceLabel, status, description) {
    actionKind := (sourceKind = "record") ? "record" : ((sourceKind = "history") ? "history" : "fuel")
    return BuildVehimapAuditEntry(
        sourceKind "_cost",
        "Chyba",
        sourceLabel,
        vehicle,
        Trim(title) != "" ? title : "(bez názvu)",
        status,
        description,
        actionKind,
        entryId,
        "Nákladová položka",
        "error",
        "40"
    )
}

BuildVehimapMaintenanceAuditItem(vehicle, plan, snapshot) {
    title := Trim(plan.title)
    if (title = "") {
        title := "(bez názvu)"
    }

    return BuildVehimapAuditEntry(
        "maintenance_data",
        "Upozornění",
        "Údržba",
        vehicle,
        title,
        "Chybí tachometr",
        "Plán údržby hlídá kilometrový interval, ale u vozidla není k dispozici použitelný aktuální stav tachometru.",
        "maintenance",
        plan.id,
        "Plán údržby",
        "warning",
        "50"
    )
}

BuildVehimapOdometerAuditItem(vehicle, sample, previousSample) {
    sourceLabel := (sample.sourceKind = "history") ? "Historie" : "Tankování"
    term := (sample.sourceKind = "history") ? sample.entry.eventType : ((Trim(sample.entry.fuelType) != "") ? sample.entry.fuelType : "Tankování")
    description := "Tachometr " FormatHistoryOdometer(sample.odometerValue) " km dne " sample.dateText " je nižší než dříve uložená hodnota " FormatHistoryOdometer(previousSample.odometerValue) " km z " previousSample.dateText "."

    return BuildVehimapAuditEntry(
        sample.sourceKind "_odometer",
        "Chyba",
        sourceLabel,
        vehicle,
        term,
        "Tachometr klesá",
        description,
        sample.sourceKind,
        sample.entry.id,
        "Nekonzistentní tachometr",
        "error",
        "25"
    )
}

BuildVehimapAuditEntry(kind, severityLabel, sourceLabel, vehicle, term, status, description, actionKind, entryId := "", kindLabel := "", severity := "warning", sortCode := "90") {
    if (kindLabel = "") {
        kindLabel := sourceLabel
    }

    auditKey := kind "|" vehicle.id "|" entryId "|" term "|" status
    return {
        isAuditIssue: true,
        kind: kind,
        kindLabel: kindLabel,
        severity: severity,
        severityLabel: severityLabel,
        sourceLabel: sourceLabel,
        vehicle: vehicle,
        term: term,
        status: status,
        description: description,
        actionKind: actionKind,
        entryId: entryId,
        dueStamp: "99999999999999",
        overviewSortKey: "9|" sortCode "|" StrLower(sourceLabel) "|" StrLower(vehicle.name) "|" StrLower(term),
        auditKey: auditKey
    }
}

BuildVehimapAuditSummaryText(allItems := "", visibleItems := "") {
    if !IsObject(allItems) {
        allItems := BuildVehimapAuditItems()
    }
    if !IsObject(visibleItems) {
        visibleItems := allItems
    }

    if (allItems.Length = 0) {
        return "Audit dat nenašel žádné problémy, které by teď vyžadovaly doplnění nebo kontrolu."
    }

    counts := GetVehimapAuditCounts(allItems)
    parts := []
    if (counts.missingPlate > 0) {
        parts.Push("Bez SPZ: " counts.missingPlate)
    }
    if (counts.missingNextTk > 0) {
        parts.Push("Bez příští TK: " counts.missingNextTk)
    }
    if (counts.greenRange > 0) {
        parts.Push("Neplatný rozsah ZK: " counts.greenRange)
    }
    if (counts.recordPath > 0) {
        parts.Push("Dokladů s problémovou přílohou: " counts.recordPath)
    }
    if (counts.recordRange > 0) {
        parts.Push("Dokladů s obrácenou platností: " counts.recordRange)
    }
    if (counts.odometer > 0) {
        parts.Push("Klesající tachometr: " counts.odometer)
    }
    if (counts.costIssues > 0) {
        parts.Push("Nákladové položky: " counts.costIssues)
    }
    if (counts.maintenanceData > 0) {
        parts.Push("Servis bez tachometru: " counts.maintenanceData)
    }

    text := "Audit našel " allItems.Length " problémů: " JoinInline(parts, ", ") "."
    if (visibleItems.Length != allItems.Length) {
        text .= " Po hledání je zobrazeno " visibleItems.Length " položek."
    }
    return text
}

GetVehimapAuditCounts(items) {
    counts := {
        missingPlate: 0,
        missingNextTk: 0,
        greenRange: 0,
        recordPath: 0,
        recordRange: 0,
        odometer: 0,
        costIssues: 0,
        maintenanceData: 0
    }

    for item in items {
        switch item.kind {
            case "vehicle_field":
                if (item.term = "SPZ") {
                    counts.missingPlate += 1
                } else if (item.term = "Příští TK") {
                    counts.missingNextTk += 1
                }
            case "vehicle_green_range":
                counts.greenRange += 1
            case "record_path":
                counts.recordPath += 1
            case "record_range":
                counts.recordRange += 1
            case "history_odometer", "fuel_odometer":
                counts.odometer += 1
            case "record_cost", "history_cost", "fuel_cost":
                counts.costIssues += 1
            case "maintenance_data":
                counts.maintenanceData += 1
        }
    }

    return counts
}

SortVehimapAuditItems(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehimapAuditItems(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehimapAuditItems(left, right) {
    result := CompareTextValues(left.overviewSortKey, right.overviewSortKey)
    if (result != 0) {
        return result
    }

    result := CompareVehicles(left.vehicle, right.vehicle)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.auditKey, right.auditKey)
}

VehicleHasInvalidGreenCardRange(vehicle) {
    return (Trim(vehicle.greenCardFrom) != "" && Trim(vehicle.greenCardTo) != "" && ParseDueStamp(vehicle.greenCardFrom) > ParseDueStamp(vehicle.greenCardTo))
}

RecordHasInvalidValidityRange(entry) {
    return (Trim(entry.validFrom) != "" && Trim(entry.validTo) != "" && ParseDueStamp(entry.validFrom) > ParseDueStamp(entry.validTo))
}

GetVehicleOdometerSamples(vehicleId, startMonthIndex := "", endMonthIndex := "") {
    global VehicleHistory, VehicleFuelLog

    samples := []

    for entry in VehicleHistory {
        if (entry.vehicleId != vehicleId) {
            continue
        }

        odometerText := NormalizeOdometerText(entry.odometer)
        stamp := ParseEventDateStamp(entry.eventDate)
        if (odometerText = "" || stamp = "") {
            continue
        }

        monthIndex := GetEventDateMonthIndex(entry.eventDate)
        if (startMonthIndex != "" && (monthIndex < startMonthIndex || monthIndex > endMonthIndex)) {
            continue
        }

        samples.Push({
            sourceKind: "history",
            entry: entry,
            dateText: entry.eventDate,
            stamp: stamp,
            odometerValue: odometerText + 0
        })
    }

    for entry in VehicleFuelLog {
        if (entry.vehicleId != vehicleId) {
            continue
        }

        odometerText := NormalizeOdometerText(entry.odometer)
        stamp := ParseEventDateStamp(entry.entryDate)
        if (odometerText = "" || stamp = "") {
            continue
        }

        monthIndex := GetEventDateMonthIndex(entry.entryDate)
        if (startMonthIndex != "" && (monthIndex < startMonthIndex || monthIndex > endMonthIndex)) {
            continue
        }

        samples.Push({
            sourceKind: "fuel",
            entry: entry,
            dateText: entry.entryDate,
            stamp: stamp,
            odometerValue: odometerText + 0
        })
    }

    SortVehicleOdometerSamples(&samples)
    return samples
}

SortVehicleOdometerSamples(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleOdometerSamples(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleOdometerSamples(left, right) {
    if (left.stamp < right.stamp) {
        return -1
    }
    if (left.stamp > right.stamp) {
        return 1
    }

    result := CompareNumberValues(left.odometerValue, right.odometerValue)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.entry.id, right.entry.id)
}

BuildVehicleOdometerAuditItems(vehicle) {
    samples := GetVehicleOdometerSamples(vehicle.id)
    issues := []
    if (samples.Length < 2) {
        return issues
    }

    highestSample := samples[1]
    for index, sample in samples {
        if (index = 1) {
            continue
        }

        if (sample.odometerValue < highestSample.odometerValue) {
            issues.Push(BuildVehimapOdometerAuditItem(vehicle, sample, highestSample))
            continue
        }

        if (sample.odometerValue > highestSample.odometerValue) {
            highestSample := sample
        }
    }

    return issues
}

HasVehicleOdometerRegressionInRange(vehicleId, startMonthIndex, endMonthIndex) {
    samples := GetVehicleOdometerSamples(vehicleId, startMonthIndex, endMonthIndex)
    if (samples.Length < 2) {
        return false
    }

    highestValue := samples[1].odometerValue
    for index, sample in samples {
        if (index = 1) {
            continue
        }

        if (sample.odometerValue < highestValue) {
            return true
        }
        if (sample.odometerValue > highestValue) {
            highestValue := sample.odometerValue
        }
    }

    return false
}

GetVehicleDistanceSummaryForMonthRange(vehicleId, startMonthIndex, endMonthIndex) {
    samples := GetVehicleOdometerSamples(vehicleId, startMonthIndex, endMonthIndex)
    summary := {
        available: false,
        distanceKm: 0,
        sampleCount: samples.Length,
        issueReason: "",
        minOdometer: "",
        maxOdometer: "",
        hasRegression: false
    }

    if (samples.Length < 2) {
        summary.issueReason := "ChybĂ„â€šĂ‚Â­ aspoĂ„Ä…Ă‚Â dva zĂ„â€šĂ‹â€ˇznamy s tachometrem."
        return summary
    }

    if HasVehicleOdometerRegressionInRange(vehicleId, startMonthIndex, endMonthIndex) {
        summary.hasRegression := true
        summary.issueReason := "V obdobĂ„â€šĂ‚Â­ je nekonzistentnĂ„â€šĂ‚Â­ tachometr."
        return summary
    }

    minValue := samples[1].odometerValue
    maxValue := samples[1].odometerValue
    for sample in samples {
        if (sample.odometerValue < minValue) {
            minValue := sample.odometerValue
        }
        if (sample.odometerValue > maxValue) {
            maxValue := sample.odometerValue
        }
    }

    summary.available := true
    summary.minOdometer := minValue
    summary.maxOdometer := maxValue
    summary.distanceKm := maxValue - minValue
    return summary
}

GetEventDateMonthIndex(eventDate) {
    if !TryGetEventYearMonth(eventDate, &year, &month) {
        return ""
    }

    return year * 12 + month - 1
}

GetMonthIndex(year, month) {
    return year * 12 + month - 1
}

GetMonthYearFromIndex(monthIndex, &year, &month) {
    year := Floor(monthIndex / 12)
    month := Mod(monthIndex, 12) + 1
}

BuildVehicleCostComparisonContext(yearLabel, fromMonth, toMonth) {
    yearValue := yearLabel + 0
    currentStartIndex := GetMonthIndex(yearValue, fromMonth)
    currentEndIndex := GetMonthIndex(yearValue, toMonth)
    lengthMonths := currentEndIndex - currentStartIndex + 1
    previousEndIndex := currentStartIndex - 1
    previousStartIndex := previousEndIndex - lengthMonths + 1

    GetMonthYearFromIndex(previousStartIndex, &previousYear, &previousFromMonth)
    GetMonthYearFromIndex(previousEndIndex, &previousEndYear, &previousToMonth)

    return {
        currentStartIndex: currentStartIndex,
        currentEndIndex: currentEndIndex,
        previousStartIndex: previousStartIndex,
        previousEndIndex: previousEndIndex,
        previousYear: previousYear,
        previousFromMonth: previousFromMonth,
        previousEndYear: previousEndYear,
        previousToMonth: previousToMonth,
        spansMultipleYears: previousYear != previousEndYear
    }
}

BuildCostPerKmText(totalCost, distanceSummary) {
    if !IsObject(distanceSummary) || !distanceSummary.available || distanceSummary.distanceKm <= 0 {
        return "NedostupnĂ„â€šĂ‚Â©"
    }

    return FormatCostAmount(totalCost / distanceSummary.distanceKm) "/km"
}

BuildDistanceText(distanceSummary) {
    if !IsObject(distanceSummary) || !distanceSummary.available {
        return "NedostupnĂ„â€šĂ‚Â©"
    }

    return FormatHistoryOdometer(distanceSummary.distanceKm) " km"
}
