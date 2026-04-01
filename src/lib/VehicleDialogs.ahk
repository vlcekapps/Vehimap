AddVehicle(*) {
    OpenVehicleForm("add")
}

EditSelectedVehicle(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, které chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleForm("edit", vehicle)
}

DeleteSelectedVehicle(*) {
    global AppTitle, Vehicles

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, které chcete odstranit.", AppTitle, 0x40)
        return
    }

    historyCount := GetVehicleHistoryCount(vehicle.id)
    fuelCount := GetVehicleFuelEntryCount(vehicle.id)
    recordCount := GetVehicleRecordCount(vehicle.id)
    reminderCount := GetVehicleReminderCount(vehicle.id)
    maintenanceCount := GetVehicleMaintenancePlanCount(vehicle.id)
    message := "Opravdu chcete odstranit vozidlo: " vehicle.name "?"
    if (historyCount > 0) {
        message .= "`n`nSoučasně bude odstraněno i " historyCount " záznamů z historie událostí."
    }
    if (fuelCount > 0) {
        message .= "`nSoučasně bude odstraněno i " fuelCount " záznamů kilometrů a tankování."
    }
    if (recordCount > 0) {
        message .= "`nSoučasně bude odstraněno i " recordCount " záznamů pojištění a dokladů."
    }
    if (reminderCount > 0) {
        message .= "`nSoučasně bude odstraněno i " reminderCount " vlastních připomínek."
    }
    if (maintenanceCount > 0) {
        message .= "`nSoučasně bude odstraněno i " maintenanceCount " plánů údržby."
    }

    result := MsgBox(message, AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    isNewVehicle := (FormMode != "edit")
    index := FindVehicleIndexById(vehicle.id)
    if !index {
        return
    }

    Vehicles.RemoveAt(index)
    SaveVehicles()
    DeleteVehicleHistory(vehicle.id)
    DeleteVehicleFuelEntries(vehicle.id)
    DeleteVehicleRecords(vehicle.id)
    DeleteVehicleReminders(vehicle.id)
    DeleteVehicleMaintenancePlans(vehicle.id)
    DeleteVehicleMeta(vehicle.id)
    RefreshVehicleList()
    CheckDueVehicles(false, false)
}

OpenSelectedVehicleDetail(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, jehož detail chcete zobrazit.", AppTitle, 0x40)
        return
    }

    OpenVehicleDetailDialog(vehicle)
}

OpenSelectedVehicleHistory(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, jehož historii chcete zobrazit.", AppTitle, 0x40)
        return
    }

    OpenVehicleHistoryDialog(vehicle)
}

OpenSelectedVehicleFuelLog(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, jehož kilometry a tankování chcete zobrazit.", AppTitle, 0x40)
        return
    }

    OpenVehicleFuelDialog(vehicle)
}

OpenSelectedVehicleRecords(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, jehož pojištění a doklady chcete zobrazit.", AppTitle, 0x40)
        return
    }

    OpenVehicleRecordsDialog(vehicle)
}

OpenSelectedVehicleReminders(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, jehož vlastní připomínky chcete zobrazit.", AppTitle, 0x40)
        return
    }

    OpenVehicleReminderDialog(vehicle)
}

OpenSelectedVehicleMaintenancePlans(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, jehož plán údržby chcete zobrazit.", AppTitle, 0x40)
        return
    }

    OpenVehicleMaintenanceDialog(vehicle)
}

OpenSelectedVehicleTimeline(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, jehož časovou osu chcete zobrazit.", AppTitle, 0x40)
        return
    }

    OpenVehicleTimelineDialog(vehicle)
}

OpenSelectedVehicleCosts(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, jehož náklady a souhrny chcete zobrazit.", AppTitle, 0x40)
        return
    }

    OpenVehicleCostSummaryDialog(vehicle)
}

OpenVehicleDetailDialog(vehicle) {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, DetailVehicleId, DetailRecentHistoryList, DetailHistorySummaryLabel, DetailReminderSummaryLabel, DetailFuelSummaryLabel, DetailRecordsSummaryLabel, DetailMaintenanceSummaryLabel, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, MaintenanceGui, MaintenanceFormGui, MaintenanceCompleteGui, DetailLayout

    if IsObject(DetailGui) {
        WinActivate("ahk_id " DetailGui.Hwnd)
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

    if IsObject(MaintenanceGui) {
        WinActivate("ahk_id " MaintenanceGui.Hwnd)
        return
    }

    if IsObject(MaintenanceFormGui) {
        WinActivate("ahk_id " MaintenanceFormGui.Hwnd)
        return
    }

    if IsObject(MaintenanceCompleteGui) {
        WinActivate("ahk_id " MaintenanceCompleteGui.Hwnd)
        return
    }

    ShowMainWindow()

    DetailVehicleId := vehicle.id
    DetailLayout := {}
    meta := GetVehicleMeta(vehicle.id)
    DetailGui := Gui("+Owner" MainGui.Hwnd " +Resize", AppTitle " - Detail vozidla")
    DetailGui.SetFont("s10", "Segoe UI")
    DetailGui.Opt("+MinSize760x860")
    DetailGui.OnEvent("Close", CloseVehicleDetailDialog)
    DetailGui.OnEvent("Escape", CloseVehicleDetailDialog)
    DetailGui.OnEvent("Size", OnVehicleDetailDialogSize)

    MainGui.Opt("+Disabled")

    introLabel := DetailGui.AddText("x20 y20 w720", "Na jednom místě tu rychle zkontrolujete stav vozidla, poslední události, navazující evidence i servisní profil.")

    basicGroup := DetailGui.AddGroupBox("x20 y50 w720 h180", "Základní údaje")
    DetailGui.AddText("x35 y80 w130", "Vlastní pojmenování")
    DetailGui.AddText("x170 y80 w150", FormatDisplayValue(vehicle.name))
    DetailGui.AddText("x355 y80 w110", "Kategorie")
    DetailGui.AddText("x470 y80 w180", FormatDisplayValue(vehicle.category))
    DetailGui.AddText("x35 y110 w130", "Poznámka")
    DetailGui.AddText("x170 y110 w150", FormatDisplayValue(vehicle.vehicleNote))
    DetailGui.AddText("x355 y110 w110", "Značka / model")
    DetailGui.AddText("x470 y110 w180", FormatDisplayValue(vehicle.makeModel))
    DetailGui.AddText("x35 y140 w130", "SPZ")
    DetailGui.AddText("x170 y140 w150", FormatDisplayValue(vehicle.plate))
    DetailGui.AddText("x355 y140 w110", "Rok výroby")
    DetailGui.AddText("x470 y140 w180", FormatDisplayValue(vehicle.year))
    DetailGui.AddText("x35 y170 w130", "Výkon")
    DetailGui.AddText("x170 y170 w150", FormatDisplayValue(vehicle.power))
    DetailGui.AddText("x355 y170 w110", "Celkový stav")
    DetailGui.AddText("x470 y170 w180", BuildVehicleDetailStatusText(vehicle))
    DetailGui.AddText("x35 y195 w130", "Poslední tachometr")
    DetailGui.AddText("x170 y195 w150", FormatDisplayValue(GetLatestVehicleOdometerText(vehicle.id), "Neznámý"))
    DetailGui.AddText("x355 y195 w110", "Stav vozidla")
    DetailGui.AddText("x470 y195 w180", FormatDisplayValue(meta.state, "Nevyplněno"))
    DetailGui.AddText("x35 y220 w130", "Štítky")
    DetailGui.AddText("x170 y220 w520", FormatDisplayValue(meta.tags, "Nevyplněno"))

    termsGroup := DetailGui.AddGroupBox("x20 y240 w720 h110", "Platnost a termíny")
    DetailGui.AddText("x35 y270 w130", "Poslední TK")
    DetailGui.AddText("x170 y270 w150", FormatDisplayValue(vehicle.lastTk))
    DetailGui.AddText("x355 y270 w110", "Příští TK")
    DetailGui.AddText("x470 y270 w180", FormatDisplayValue(vehicle.nextTk))
    DetailGui.AddText("x35 y300 w130", "Stav TK")
    DetailGui.AddText("x170 y300 w150", FormatDisplayValue(GetExpirationStatusText(vehicle.nextTk, GetTechnicalReminderDays()), "V pořádku"))
    DetailGui.AddText("x355 y300 w110", "Zelená karta")
    DetailGui.AddText("x470 y300 w220", BuildGreenCardRangeText(vehicle))
    DetailGui.AddText("x35 y330 w130", "Stav ZK")
    DetailGui.AddText("x170 y330 w520", FormatDisplayValue(GetExpirationStatusText(vehicle.greenCardTo, GetGreenCardReminderDays()), vehicle.greenCardTo = "" ? "Nevyplněno" : "V pořádku"))

    historyGroup := DetailGui.AddGroupBox("x20 y360 w720 h150", "Poslední události")
    DetailHistorySummaryLabel := DetailGui.AddText("x35 y390 w685", BuildVehicleHistorySummaryText(vehicle.id))
    DetailRecentHistoryList := DetailGui.AddListView("x35 y418 w685 h60 Grid -Multi", ["Datum", "Událost", "Km", "Cena"])
    DetailRecentHistoryList.ModifyCol(1, "85")
    DetailRecentHistoryList.ModifyCol(2, "220")
    DetailRecentHistoryList.ModifyCol(3, "110")
    DetailRecentHistoryList.ModifyCol(4, "190")
    PopulateVehicleDetailHistoryList(vehicle.id)

    evidenceGroup := DetailGui.AddGroupBox("x20 y520 w720 h120", "Další evidence")
    DetailReminderSummaryLabel := DetailGui.AddText("x35 y545 w685", BuildVehicleReminderSummaryText(vehicle.id))
    DetailFuelSummaryLabel := DetailGui.AddText("x35 y568 w685", BuildVehicleFuelSummaryText(vehicle.id))
    DetailRecordsSummaryLabel := DetailGui.AddText("x35 y591 w685", BuildVehicleRecordsSummaryText(vehicle.id))
    DetailMaintenanceSummaryLabel := DetailGui.AddText("x35 y614 w685", BuildVehicleMaintenanceSummaryText(vehicle.id))

    serviceProfileGroup := DetailGui.AddGroupBox("x20 y648 w720 h105", "Servisní profil")
    DetailGui.AddText("x35 y678 w130", "Pohon")
    DetailGui.AddText("x170 y678 w150", FormatDisplayValue(meta.powertrain))
    DetailGui.AddText("x355 y678 w110", "Klimatizace")
    DetailGui.AddText("x470 y678 w180", FormatDisplayValue(meta.climateProfile))
    DetailGui.AddText("x35 y708 w130", "Rozvody")
    DetailGui.AddText("x170 y708 w150", FormatDisplayValue(meta.timingDrive))
    DetailGui.AddText("x355 y708 w110", "Převodovka")
    DetailGui.AddText("x470 y708 w180", FormatDisplayValue(meta.transmission))

    editButton := DetailGui.AddButton("x35 y770 w120 h30", "Upravit vozidlo")
    editButton.OnEvent("Click", EditVehicleFromDetail)

    historyButton := DetailGui.AddButton("x165 y770 w110 h30", "Historie")
    historyButton.OnEvent("Click", OpenHistoryFromDetail)

    remindersButton := DetailGui.AddButton("x285 y770 w120 h30", "Připomínky")
    remindersButton.OnEvent("Click", OpenRemindersFromDetail)

    fuelButton := DetailGui.AddButton("x415 y770 w120 h30", "Tankování")
    fuelButton.OnEvent("Click", OpenFuelFromDetail)

    recordsButton := DetailGui.AddButton("x545 y770 w160 h30", "Pojištění a doklady")
    recordsButton.OnEvent("Click", OpenRecordsFromDetail)

    maintenanceButton := DetailGui.AddButton("x110 y805 w150 h30", "Plán údržby")
    maintenanceButton.OnEvent("Click", OpenMaintenanceFromDetail)

    timelineButton := DetailGui.AddButton("x270 y805 w150 h30", "Časová osa")
    timelineButton.OnEvent("Click", OpenTimelineFromDetail)

    costsButton := DetailGui.AddButton("x430 y805 w140 h30", "Náklady a souhrny")
    costsButton.OnEvent("Click", OpenCostsFromDetail)

    closeButton := DetailGui.AddButton("x580 y805 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleDetailDialog)

    DetailLayout := {
        introLabel: introLabel,
        basicGroup: basicGroup,
        termsGroup: termsGroup,
        historyGroup: historyGroup,
        evidenceGroup: evidenceGroup,
        serviceProfileGroup: serviceProfileGroup,
        editButton: editButton,
        historyButton: historyButton,
        remindersButton: remindersButton,
        fuelButton: fuelButton,
        recordsButton: recordsButton,
        maintenanceButton: maintenanceButton,
        timelineButton: timelineButton,
        costsButton: costsButton,
        closeButton: closeButton
    }

    DetailGui.Show("w760 h860")
    editButton.Focus()

    hooks := GetVehimapTestHooks()
    if IsObject(hooks) {
        hooks.detailInitialFocus := "edit"
    }
}

CloseVehicleDetailDialog(*) {
    global DetailGui, DetailVehicleId, DetailRecentHistoryList, DetailHistorySummaryLabel, DetailReminderSummaryLabel, DetailFuelSummaryLabel, DetailRecordsSummaryLabel, DetailMaintenanceSummaryLabel, DetailLayout, MainGui

    if IsObject(DetailGui) {
        DetailGui.Destroy()
        DetailGui := 0
    }

    DetailVehicleId := ""
    DetailRecentHistoryList := 0
    DetailHistorySummaryLabel := 0
    DetailReminderSummaryLabel := 0
    DetailFuelSummaryLabel := 0
    DetailRecordsSummaryLabel := 0
    DetailMaintenanceSummaryLabel := 0
    DetailLayout := {}
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

OnVehicleDetailDialogSize(guiObj, minMax, width, height) {
    global DetailLayout, DetailHistorySummaryLabel, DetailRecentHistoryList, DetailReminderSummaryLabel, DetailFuelSummaryLabel, DetailRecordsSummaryLabel, DetailMaintenanceSummaryLabel

    if (minMax = -1) {
        return
    }

    extraHeight := height - 860
    groupWidth := width - 40
    textWidth := width - 75

    if IsObject(DetailLayout) {
        MoveGuiControl(DetailLayout.introLabel, 20, 20, width - 40)
        MoveGuiControl(DetailLayout.basicGroup, 20, 50, groupWidth, 180)
        MoveGuiControl(DetailLayout.termsGroup, 20, 240, groupWidth, 110)
        MoveGuiControl(DetailLayout.historyGroup, 20, 360, groupWidth, 150 + extraHeight)
        MoveGuiControl(DetailLayout.evidenceGroup, 20, 520 + extraHeight, groupWidth, 120)
        MoveGuiControl(DetailLayout.serviceProfileGroup, 20, 648 + extraHeight, groupWidth, 105)
        MoveGuiControl(DetailLayout.editButton, 35, 770 + extraHeight, 120, 30)
        MoveGuiControl(DetailLayout.historyButton, 165, 770 + extraHeight, 110, 30)
        MoveGuiControl(DetailLayout.remindersButton, 285, 770 + extraHeight, 120, 30)
        MoveGuiControl(DetailLayout.fuelButton, 415, 770 + extraHeight, 120, 30)
        MoveGuiControl(DetailLayout.recordsButton, 545, 770 + extraHeight, 160, 30)
        MoveGuiControl(DetailLayout.maintenanceButton, 110, 805 + extraHeight, 150, 30)
        MoveGuiControl(DetailLayout.timelineButton, 270, 805 + extraHeight, 150, 30)
        MoveGuiControl(DetailLayout.costsButton, 430, 805 + extraHeight, 140, 30)
        MoveGuiControl(DetailLayout.closeButton, width - 180, 805 + extraHeight, 100, 30)
    }

    MoveGuiControl(DetailHistorySummaryLabel, 35, 390, textWidth)
    MoveGuiControl(DetailRecentHistoryList, 35, 418, textWidth, 60 + extraHeight)
    MoveGuiControl(DetailReminderSummaryLabel, 35, 545 + extraHeight, textWidth)
    MoveGuiControl(DetailFuelSummaryLabel, 35, 568 + extraHeight, textWidth)
    MoveGuiControl(DetailRecordsSummaryLabel, 35, 591 + extraHeight, textWidth)
    MoveGuiControl(DetailMaintenanceSummaryLabel, 35, 614 + extraHeight, textWidth)
}

EditVehicleFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleForm("edit", vehicle)
}

OpenHistoryFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleHistoryDialog(vehicle)
}

OpenFuelFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleFuelDialog(vehicle)
}

OpenRemindersFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleReminderDialog(vehicle)
}

OpenRecordsFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleRecordsDialog(vehicle)
}

OpenMaintenanceFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleMaintenanceDialog(vehicle)
}

OpenTimelineFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleTimelineDialog(vehicle)
}

OpenCostsFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleCostSummaryDialog(vehicle)
}

PopulateVehicleDetailHistoryList(vehicleId) {
    global DetailRecentHistoryList

    if !IsObject(DetailRecentHistoryList) {
        return
    }

    DetailRecentHistoryList.Delete()
    recentEvents := GetRecentVehicleHistoryEntries(vehicleId, 5)
    for event in recentEvents {
        DetailRecentHistoryList.Add("", event.eventDate, event.eventType, FormatHistoryOdometer(event.odometer), event.cost)
    }
}

OpenVehicleForm(mode, vehicle := "") {
    global AppTitle, Categories, VehicleStateOptions, VehiclePowertrainOptions, VehicleClimateProfileOptions, VehicleTimingDriveOptions, VehicleTransmissionOptions, FormGui, FormControls, FormMode, FormVehicleId, MainGui, TabsCtrl, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, MaintenanceGui, MaintenanceFormGui, MaintenanceCompleteGui

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

    if IsObject(MaintenanceGui) {
        WinActivate("ahk_id " MaintenanceGui.Hwnd)
        return
    }

    if IsObject(MaintenanceFormGui) {
        WinActivate("ahk_id " MaintenanceFormGui.Hwnd)
        return
    }

    if IsObject(MaintenanceCompleteGui) {
        WinActivate("ahk_id " MaintenanceCompleteGui.Hwnd)
        return
    }

    FormMode := mode
    FormVehicleId := IsObject(vehicle) ? vehicle.id : ""
    FormControls := {}

    title := (mode = "edit") ? "Upravit vozidlo" : "Přidat vozidlo"
    FormGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - " title)
    FormGui.SetFont("s10", "Segoe UI")
    FormGui.OnEvent("Close", CloseVehicleForm)
    FormGui.OnEvent("Escape", CloseVehicleForm)

    MainGui.Opt("+Disabled")

    labelX := 20
    inputX := 245
    inputW := 275
    rowY := 60
    rowStep := 35

    FormGui.AddText("x20 y20 w500", "Pole označená jako povinné musíte vyplnit. Pole označená jako volitelné můžete nechat prázdná.")

    FormGui.AddText("x" labelX " y" rowY " w210", "Vlastní pojmenování (povinné)")
    FormControls.name := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Kategorie (povinné)")
    FormControls.category := FormGui.AddDropDownList(Format("x{} y{} w{}", inputX, rowY - 3, inputW), Categories)
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Poznámka k vozidlu (volitelné)")
    FormControls.vehicleNote := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Značka / model (povinné)")
    FormControls.makeModel := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "SPZ (volitelné)")
    FormControls.plate := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Rok výroby (volitelné)")
    FormControls.year := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Výkon (volitelné)")
    FormControls.power := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Poslední TK (volitelné)")
    FormControls.lastTk := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Příští TK (povinné)")
    FormControls.nextTk := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Zelená karta od (volitelné)")
    FormControls.greenCardFrom := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Zelená karta do (volitelné)")
    FormControls.greenCardTo := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Stav vozidla (volitelné)")
    FormControls.vehicleState := FormGui.AddDropDownList(Format("x{} y{} w{}", inputX, rowY - 3, inputW), VehicleStateOptions)
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Štítky (volitelné)")
    FormControls.vehicleTags := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Pohon (volitelné)")
    FormControls.powertrain := FormGui.AddDropDownList(Format("x{} y{} w{}", inputX, rowY - 3, inputW), VehiclePowertrainOptions)
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Klimatizace (volitelné)")
    FormControls.climateProfile := FormGui.AddDropDownList(Format("x{} y{} w{}", inputX, rowY - 3, inputW), VehicleClimateProfileOptions)
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Rozvody (volitelné)")
    FormControls.timingDrive := FormGui.AddDropDownList(Format("x{} y{} w{}", inputX, rowY - 3, inputW), VehicleTimingDriveOptions)
    rowY += rowStep

    FormGui.AddText("x" labelX " y" rowY " w210", "Převodovka (volitelné)")
    FormControls.transmission := FormGui.AddDropDownList(Format("x{} y{} w{}", inputX, rowY - 3, inputW), VehicleTransmissionOptions)
    rowY += rowStep + 10
    FormGui.AddText("x20 y" rowY " w500", "Datum zadávejte jako MM/RRRR, například 04/2026. Pro upozornění se používají pole Příští TK a Zelená karta do.")

    saveButton := FormGui.AddButton(Format("x185 y{} w140 h30 Default", rowY + 45), "Uložit")
    saveButton.OnEvent("Click", SaveVehicleFromForm)

    cancelButton := FormGui.AddButton(Format("x335 y{} w140 h30", rowY + 45), "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleForm)

    if IsObject(vehicle) {
        FormControls.name.Text := vehicle.name
        SetDropDownToText(FormControls.category, vehicle.category, Categories)
        FormControls.vehicleNote.Text := vehicle.vehicleNote
        FormControls.makeModel.Text := vehicle.makeModel
        FormControls.plate.Text := vehicle.plate
        FormControls.year.Text := vehicle.year
        FormControls.power.Text := vehicle.power
        FormControls.lastTk.Text := vehicle.lastTk
        FormControls.nextTk.Text := vehicle.nextTk
        FormControls.greenCardFrom.Text := vehicle.greenCardFrom
        FormControls.greenCardTo.Text := vehicle.greenCardTo
        meta := GetVehicleMeta(vehicle.id)
        SetDropDownToText(FormControls.vehicleState, meta.state, VehicleStateOptions)
        FormControls.vehicleTags.Text := meta.tags
        SetDropDownToText(FormControls.powertrain, meta.powertrain, VehiclePowertrainOptions)
        SetDropDownToText(FormControls.climateProfile, meta.climateProfile, VehicleClimateProfileOptions)
        SetDropDownToText(FormControls.timingDrive, meta.timingDrive, VehicleTimingDriveOptions)
        SetDropDownToText(FormControls.transmission, meta.transmission, VehicleTransmissionOptions)
    } else {
        FormControls.category.Value := TabsCtrl.Value
    }

    FormGui.Show("w550 h775")
    FormControls.name.Focus()
}

CloseVehicleForm(*) {
    global FormGui, MainGui

    if IsObject(FormGui) {
        hwnd := FormGui.Hwnd
        FormGui.Destroy()
        FormGui := 0
    }

    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

SaveVehicleFromForm(*) {
    global AppTitle, FormMode, FormVehicleId, FormControls, Vehicles

    name := Trim(FormControls.name.Text)
    category := Trim(FormControls.category.Text)
    vehicleNote := Trim(FormControls.vehicleNote.Text)
    makeModel := Trim(FormControls.makeModel.Text)
    plate := StrUpper(Trim(FormControls.plate.Text))
    year := Trim(FormControls.year.Text)
    power := Trim(FormControls.power.Text)
    lastTk := NormalizeMonthYear(FormControls.lastTk.Text)
    nextTk := NormalizeMonthYear(FormControls.nextTk.Text)
    greenCardFrom := NormalizeMonthYear(FormControls.greenCardFrom.Text)
    greenCardTo := NormalizeMonthYear(FormControls.greenCardTo.Text)
    vehicleState := Trim(FormControls.vehicleState.Text)
    vehicleTags := Trim(FormControls.vehicleTags.Text)
    powertrain := Trim(FormControls.powertrain.Text)
    climateProfile := Trim(FormControls.climateProfile.Text)
    timingDrive := Trim(FormControls.timingDrive.Text)
    transmission := Trim(FormControls.transmission.Text)
    if (name = "") {
        MsgBox("Vyplňte prosím vlastní pojmenování vozidla.", AppTitle, 0x30)
        FormControls.name.Focus()
        return
    }

    if (category = "") {
        MsgBox("Vyberte prosím kategorii vozidla.", AppTitle, 0x30)
        FormControls.category.Focus()
        return
    }

    if (makeModel = "") {
        MsgBox("Vyplňte prosím značku / model.", AppTitle, 0x30)
        FormControls.makeModel.Focus()
        return
    }

    if (Trim(FormControls.lastTk.Text) != "" && lastTk = "") {
        MsgBox("Pole Poslední TK musí být ve formátu MM/RRRR.", AppTitle, 0x30)
        FormControls.lastTk.Focus()
        return
    }

    if (nextTk = "") {
        MsgBox("Pole Příští TK je povinné a musí být ve formátu MM/RRRR.", AppTitle, 0x30)
        FormControls.nextTk.Focus()
        return
    }

    if (Trim(FormControls.greenCardFrom.Text) != "" && greenCardFrom = "") {
        MsgBox("Pole Zelená karta od musí být ve formátu MM/RRRR.", AppTitle, 0x30)
        FormControls.greenCardFrom.Focus()
        return
    }

    if (Trim(FormControls.greenCardTo.Text) != "" && greenCardTo = "") {
        MsgBox("Pole Zelená karta do musí být ve formátu MM/RRRR.", AppTitle, 0x30)
        FormControls.greenCardTo.Focus()
        return
    }

    if (greenCardFrom != "" && greenCardTo != "" && ParseDueStamp(greenCardFrom) > ParseDueStamp(greenCardTo)) {
        MsgBox("Pole Zelená karta od nesmí být později než pole Zelená karta do.", AppTitle, 0x30)
        FormControls.greenCardFrom.Focus()
        return
    }

    if (year != "" && !RegExMatch(year, "^\d{4}$")) {
        MsgBox("Rok výroby zadejte prosím jako čtyřciferný rok, nebo pole nechte prázdné.", AppTitle, 0x30)
        FormControls.year.Focus()
        return
    }

    isNewVehicle := (FormMode != "edit")

    vehicle := {
        id: (FormMode = "edit") ? FormVehicleId : GenerateVehicleId(),
        name: name,
        category: NormalizeCategory(category),
        vehicleNote: vehicleNote,
        makeModel: makeModel,
        plate: plate,
        year: year,
        power: power,
        lastTk: lastTk,
        nextTk: nextTk,
        greenCardFrom: greenCardFrom,
        greenCardTo: greenCardTo
    }

    index := FindVehicleIndexById(vehicle.id)
    if index {
        Vehicles[index] := vehicle
    } else {
        Vehicles.Push(vehicle)
    }

    SaveVehicles()
    SaveVehicleMetaEntry(vehicle.id, vehicleState, vehicleTags, powertrain, climateProfile, timingDrive, transmission)
    CloseVehicleForm()
    OpenVehicleById(vehicle.id, true)
    if isNewVehicle {
        OfferVehicleStarterBundleAfterCreate(vehicle.id)
    }
    CheckDueVehicles(false, false)
}
