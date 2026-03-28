OpenVehicleCostSummaryDialog(vehicle) {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, FleetCostGui
    global CostSummaryGui, CostSummaryVehicleId, CostSummarySummaryLabel, CostSummaryList, CostSummaryPeriodYearCtrl, CostSummaryPresetCtrl, CostSummaryFromMonthCtrl, CostSummaryToMonthCtrl, CostSummaryPeriodSummaryLabel, CostSummaryPeriodList
    global CostSummaryPresetOptions, MonthOptionLabels

    if IsObject(CostSummaryGui) {
        WinActivate("ahk_id " CostSummaryGui.Hwnd)
        return
    }

    for guiRef in [FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, FleetCostGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    ShowMainWindow()

    CostSummaryVehicleId := vehicle.id
    CostSummaryGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Náklady a souhrny")
    CostSummaryGui.SetFont("s10", "Segoe UI")
    CostSummaryGui.OnEvent("Close", CloseVehicleCostSummaryDialog)
    CostSummaryGui.OnEvent("Escape", CloseVehicleCostSummaryDialog)

    MainGui.Opt("+Disabled")

    summary := BuildVehicleCostSummary(vehicle.id)
    yearOptions := GetVehicleCostSummaryYearOptions(vehicle.id)

    CostSummaryGui.AddText("x20 y20 w860", "Zde vidíte náklady vozidla " vehicle.name " za vybrané měsíce ve zvoleném roce i dlouhodobý přehled podle jednotlivých let. Součet vychází z tankování, historie událostí a pojištění či dokladů, kde je vyplněna číselná částka.")

    CostSummaryGui.AddGroupBox("x20 y55 w860 h225", "Vybrané období")
    CostSummaryGui.AddText("x35 y85 w55", "Rok")
    CostSummaryPeriodYearCtrl := CostSummaryGui.AddDropDownList("x95 y82 w100", yearOptions)
    SetDropDownToText(CostSummaryPeriodYearCtrl, FormatTime(A_Now, "yyyy"), yearOptions)

    CostSummaryGui.AddText("x215 y85 w75", "Předvolba")
    CostSummaryPresetCtrl := CostSummaryGui.AddDropDownList("x295 y82 w220", CostSummaryPresetOptions)
    CostSummaryPresetCtrl.Value := 7
    CostSummaryPresetCtrl.OnEvent("Change", OnCostSummaryPresetChanged)

    refreshPeriodButton := CostSummaryGui.AddButton("x535 y80 w125 h28", "Obnovit období")
    refreshPeriodButton.OnEvent("Click", RefreshVehicleCostPeriodSummary)

    CostSummaryGui.AddText("x35 y120 w70", "Od měsíce")
    CostSummaryFromMonthCtrl := CostSummaryGui.AddDropDownList("x110 y117 w170", MonthOptionLabels)
    CostSummaryFromMonthCtrl.Value := 1
    CostSummaryFromMonthCtrl.OnEvent("Change", OnCostSummaryFromMonthChanged)

    CostSummaryGui.AddText("x300 y120 w70", "Do měsíce")
    CostSummaryToMonthCtrl := CostSummaryGui.AddDropDownList("x375 y117 w170", MonthOptionLabels)
    CostSummaryToMonthCtrl.Value := 12
    CostSummaryToMonthCtrl.OnEvent("Change", OnCostSummaryToMonthChanged)

    CostSummaryPeriodYearCtrl.OnEvent("Change", RefreshVehicleCostPeriodSummary)

    CostSummaryPeriodSummaryLabel := CostSummaryGui.AddText("x35 y155 w820 h32", "")

    CostSummaryPeriodList := CostSummaryGui.AddListView("x35 y190 w820 h75 Grid -Multi", ["Skupina", "Počet položek", "Částka"])
    CostSummaryPeriodList.ModifyCol(1, "250")
    CostSummaryPeriodList.ModifyCol(2, "130")
    CostSummaryPeriodList.ModifyCol(3, "180")

    CostSummaryGui.AddGroupBox("x20 y295 w860 h220", "Přehled podle roku")
    CostSummarySummaryLabel := CostSummaryGui.AddText("x35 y325 w820", BuildVehicleCostSummaryText(summary))

    CostSummaryList := CostSummaryGui.AddListView("x35 y355 w820 h120 Grid -Multi", ["Rok", "Tankování", "Historie a servis", "Doklady a pojištění", "Celkem"])
    CostSummaryList.ModifyCol(1, "90")
    CostSummaryList.ModifyCol(2, "150")
    CostSummaryList.ModifyCol(3, "180")
    CostSummaryList.ModifyCol(4, "180")
    CostSummaryList.ModifyCol(5, "150")

    for item in summary.years {
        CostSummaryList.Add("", item.year, FormatCostAmount(item.fuel), FormatCostAmount(item.history), FormatCostAmount(item.records), FormatCostAmount(item.total))
    }

    exportSummaryButton := CostSummaryGui.AddButton("x90 y485 w150 h30", "TSV souhrn")
    exportSummaryButton.OnEvent("Click", ExportVehicleCostSummaryTsv)

    exportDetailButton := CostSummaryGui.AddButton("x250 y485 w150 h30", "TSV detail")
    exportDetailButton.OnEvent("Click", ExportVehicleCostDetailTsv)

    exportHtmlButton := CostSummaryGui.AddButton("x410 y485 w150 h30", "HTML sestava")
    exportHtmlButton.OnEvent("Click", ExportVehicleCostReportHtml)

    detailButton := CostSummaryGui.AddButton("x570 y485 w150 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromCostSummary)

    closeButton := CostSummaryGui.AddButton("x730 y485 w110 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleCostSummaryDialog)

    CostSummaryGui.Show("w900 h555")
    RefreshVehicleCostPeriodSummary()
    closeButton.Focus()
}

CloseVehicleCostSummaryDialog(*) {
    global CostSummaryGui, CostSummaryVehicleId, CostSummarySummaryLabel, CostSummaryList, CostSummaryPeriodYearCtrl, CostSummaryPresetCtrl, CostSummaryFromMonthCtrl, CostSummaryToMonthCtrl, CostSummaryPeriodSummaryLabel, CostSummaryPeriodList, MainGui

    if IsObject(CostSummaryGui) {
        CostSummaryGui.Destroy()
        CostSummaryGui := 0
    }

    CostSummaryVehicleId := ""
    CostSummarySummaryLabel := 0
    CostSummaryList := 0
    CostSummaryPeriodYearCtrl := 0
    CostSummaryPresetCtrl := 0
    CostSummaryFromMonthCtrl := 0
    CostSummaryToMonthCtrl := 0
    CostSummaryPeriodSummaryLabel := 0
    CostSummaryPeriodList := 0
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

OpenFleetCostOverviewDialog(*) {
    global AppTitle, MainGui, FormGui, SettingsGui, DashboardGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui
    global FleetCostGui, FleetCostRows, FleetCostSummaryLabel, FleetCostList, FleetCostPeriodYearCtrl, FleetCostPresetCtrl, FleetCostFromMonthCtrl, FleetCostToMonthCtrl, FleetCostPeriodSummaryLabel, FleetCostPeriodList, FleetCostVehicleCostsButton, FleetCostOpenButton, FleetCostEditButton
    global CostSummaryPresetOptions, MonthOptionLabels

    if IsObject(FleetCostGui) {
        WinActivate("ahk_id " FleetCostGui.Hwnd)
        return
    }

    for guiRef in [DashboardGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    ShowMainWindow()

    FleetCostRows := []
    FleetCostGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Náklady napříč vozidly")
    FleetCostGui.SetFont("s10", "Segoe UI")
    FleetCostGui.OnEvent("Close", CloseFleetCostOverviewDialog)
    FleetCostGui.OnEvent("Escape", CloseFleetCostOverviewDialog)

    MainGui.Opt("+Disabled")

    yearOptions := GetFleetCostSummaryYearOptions()

    FleetCostGui.AddText("x20 y20 w980", "Zde vidíte náklady napříč všemi vozidly za vybrané období. Přehled ukazuje rozložení částek mezi vozidla, aktivní vozidla bez číselného nákladu i rychlé problémové stavy, které stojí za pozornost.")

    FleetCostGui.AddGroupBox("x20 y55 w980 h225", "Vybrané období")
    FleetCostGui.AddText("x35 y85 w55", "Rok")
    FleetCostPeriodYearCtrl := FleetCostGui.AddDropDownList("x95 y82 w100", yearOptions)
    SetDropDownToText(FleetCostPeriodYearCtrl, FormatTime(A_Now, "yyyy"), yearOptions)

    FleetCostGui.AddText("x215 y85 w75", "Předvolba")
    FleetCostPresetCtrl := FleetCostGui.AddDropDownList("x295 y82 w220", CostSummaryPresetOptions)
    FleetCostPresetCtrl.Value := 7
    FleetCostPresetCtrl.OnEvent("Change", OnFleetCostPresetChanged)

    refreshPeriodButton := FleetCostGui.AddButton("x535 y80 w145 h28", "Obnovit období")
    refreshPeriodButton.OnEvent("Click", RefreshFleetCostOverview)

    FleetCostGui.AddText("x35 y120 w70", "Od měsíce")
    FleetCostFromMonthCtrl := FleetCostGui.AddDropDownList("x110 y117 w170", MonthOptionLabels)
    FleetCostFromMonthCtrl.Value := 1
    FleetCostFromMonthCtrl.OnEvent("Change", OnFleetCostFromMonthChanged)

    FleetCostGui.AddText("x300 y120 w70", "Do měsíce")
    FleetCostToMonthCtrl := FleetCostGui.AddDropDownList("x375 y117 w170", MonthOptionLabels)
    FleetCostToMonthCtrl.Value := 12
    FleetCostToMonthCtrl.OnEvent("Change", OnFleetCostToMonthChanged)

    FleetCostPeriodYearCtrl.OnEvent("Change", RefreshFleetCostOverview)

    FleetCostPeriodSummaryLabel := FleetCostGui.AddText("x35 y155 w940 h32", "")

    FleetCostPeriodList := FleetCostGui.AddListView("x35 y190 w940 h75 Grid -Multi", ["Skupina", "Počet položek", "Částka"])
    FleetCostPeriodList.ModifyCol(1, "280")
    FleetCostPeriodList.ModifyCol(2, "140")
    FleetCostPeriodList.ModifyCol(3, "180")

    FleetCostGui.AddGroupBox("x20 y295 w980 h250", "Vozidla a stavy v období")
    FleetCostSummaryLabel := FleetCostGui.AddText("x35 y325 w940 h32", "")

    FleetCostList := FleetCostGui.AddListView("x35 y360 w940 h145 Grid -Multi", ["Vozidlo", "Kategorie", "SPZ", "Tankování", "Historie", "Doklady", "Celkem", "Ujeto km", "Cena / km", "Stav"])
    FleetCostList.OnEvent("DoubleClick", OpenSelectedFleetVehicleCostSummary)
    FleetCostList.ModifyCol(1, "150")
    FleetCostList.ModifyCol(2, "110")
    FleetCostList.ModifyCol(3, "80")
    FleetCostList.ModifyCol(4, "90")
    FleetCostList.ModifyCol(5, "90")
    FleetCostList.ModifyCol(6, "90")
    FleetCostList.ModifyCol(7, "90")
    FleetCostList.ModifyCol(8, "80")
    FleetCostList.ModifyCol(9, "95")
    FleetCostList.ModifyCol(10, "170")

    FleetCostVehicleCostsButton := FleetCostGui.AddButton("x130 y515 w150 h30", "Náklady vozidla")
    FleetCostVehicleCostsButton.OnEvent("Click", OpenSelectedFleetVehicleCostSummary)

    FleetCostOpenButton := FleetCostGui.AddButton("x290 y515 w150 h30", "Detail vozidla")
    FleetCostOpenButton.OnEvent("Click", OpenSelectedFleetCostVehicleDetail)

    FleetCostEditButton := FleetCostGui.AddButton("x450 y515 w150 h30", "Upravit vozidlo")
    FleetCostEditButton.OnEvent("Click", EditSelectedFleetCostVehicle)

    closeButton := FleetCostGui.AddButton("x760 y515 w110 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseFleetCostOverviewDialog)

    FleetCostGui.Show("w1020 h575")
    RefreshFleetCostOverview()
    closeButton.Focus()
}

CloseFleetCostOverviewDialog(*) {
    global FleetCostGui, FleetCostRows, FleetCostSummaryLabel, FleetCostList, FleetCostPeriodYearCtrl, FleetCostPresetCtrl, FleetCostFromMonthCtrl, FleetCostToMonthCtrl, FleetCostPeriodSummaryLabel, FleetCostPeriodList, FleetCostVehicleCostsButton, FleetCostOpenButton, FleetCostEditButton, MainGui

    if IsObject(FleetCostGui) {
        FleetCostGui.Destroy()
        FleetCostGui := 0
    }

    FleetCostRows := []
    FleetCostSummaryLabel := 0
    FleetCostList := 0
    FleetCostPeriodYearCtrl := 0
    FleetCostPresetCtrl := 0
    FleetCostFromMonthCtrl := 0
    FleetCostToMonthCtrl := 0
    FleetCostPeriodSummaryLabel := 0
    FleetCostPeriodList := 0
    FleetCostVehicleCostsButton := 0
    FleetCostOpenButton := 0
    FleetCostEditButton := 0

    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

GetSelectedFleetCostRow() {
    global FleetCostList, FleetCostRows

    if !IsObject(FleetCostList) {
        return ""
    }

    row := FleetCostList.GetNext(0)
    if !row || row > FleetCostRows.Length {
        return ""
    }

    return FleetCostRows[row]
}

OpenSelectedFleetVehicleCostSummary(*) {
    global AppTitle

    row := GetSelectedFleetCostRow()
    if !IsObject(row) {
        MsgBox("Nejprve vyberte vozidlo, jehož náklady chcete zobrazit.", AppTitle, 0x40)
        return
    }

    CloseFleetCostOverviewDialog()
    OpenVehicleCostSummaryDialog(row.vehicle)
}

OpenSelectedFleetCostVehicleDetail(*) {
    global AppTitle

    row := GetSelectedFleetCostRow()
    if !IsObject(row) {
        MsgBox("Nejprve vyberte vozidlo, které chcete zobrazit.", AppTitle, 0x40)
        return
    }

    CloseFleetCostOverviewDialog()
    OpenVehicleDetailDialog(row.vehicle)
}

EditSelectedFleetCostVehicle(*) {
    global AppTitle

    row := GetSelectedFleetCostRow()
    if !IsObject(row) {
        MsgBox("Nejprve vyberte vozidlo, které chcete upravit.", AppTitle, 0x40)
        return
    }

    CloseFleetCostOverviewDialog()
    OpenVehicleForm("edit", row.vehicle)
}

RefreshFleetCostOverview(*) {
    global FleetCostRows, FleetCostSummaryLabel, FleetCostList, FleetCostPeriodYearCtrl, FleetCostFromMonthCtrl, FleetCostToMonthCtrl, FleetCostPeriodSummaryLabel, FleetCostPeriodList, FleetCostVehicleCostsButton, FleetCostOpenButton, FleetCostEditButton

    if !IsObject(FleetCostList) || !IsObject(FleetCostPeriodList) {
        return
    }

    yearLabel := GetSelectedFleetCostYear()
    fromMonth := GetSelectedMonthOptionValue(FleetCostFromMonthCtrl)
    toMonth := GetSelectedMonthOptionValue(FleetCostToMonthCtrl)
    NormalizeFleetCostMonthRange(&fromMonth, &toMonth)
    SetFleetCostMonthControls(fromMonth, toMonth)

    summary := BuildFleetCostPeriodSummary(yearLabel, fromMonth, toMonth)

    FleetCostPeriodSummaryLabel.Text := BuildFleetCostPeriodSummaryText(summary)
    FleetCostSummaryLabel.Text := BuildFleetCostRowsSummaryText(summary)

    FleetCostPeriodList.Opt("-Redraw")
    FleetCostPeriodList.Delete()
    FleetCostPeriodList.Add("", "Tankování", summary.fuelCount, FormatCostAmount(summary.totalFuel))
    FleetCostPeriodList.Add("", "Historie a servis", summary.historyCount, FormatCostAmount(summary.totalHistory))
    FleetCostPeriodList.Add("", "Doklady a pojištění", summary.recordCount, FormatCostAmount(summary.totalRecords))
    FleetCostPeriodList.Add("", "Celkem", summary.parsedCount, FormatCostAmount(summary.totalFuel + summary.totalHistory + summary.totalRecords))
    FleetCostPeriodList.Opt("+Redraw")

    FleetCostRows := summary.rows
    FleetCostList.Opt("-Redraw")
    FleetCostList.Delete()
    for row in FleetCostRows {
        plateText := Trim(row.vehicle.plate) = "" ? "-" : row.vehicle.plate
        FleetCostList.Add("", row.vehicle.name, row.vehicle.category, plateText, FormatCostAmount(row.fuel), FormatCostAmount(row.history), FormatCostAmount(row.records), FormatCostAmount(row.total), row.distanceText, row.costPerKmText, row.status)
    }
    FleetCostList.Opt("+Redraw")

    hasRows := FleetCostRows.Length > 0
    if IsObject(FleetCostVehicleCostsButton) {
        FleetCostVehicleCostsButton.Opt(hasRows ? "-Disabled" : "+Disabled")
    }
    if IsObject(FleetCostOpenButton) {
        FleetCostOpenButton.Opt(hasRows ? "-Disabled" : "+Disabled")
    }
    if IsObject(FleetCostEditButton) {
        FleetCostEditButton.Opt(hasRows ? "-Disabled" : "+Disabled")
    }

    if hasRows {
        FleetCostList.Modify(1, "Select Focus Vis")
    }
}

OnFleetCostPresetChanged(*) {
    ApplyFleetCostPreset()
    RefreshFleetCostOverview()
}

OnFleetCostFromMonthChanged(*) {
    if (GetSelectedFleetCostPresetLength() > 0) {
        ApplyFleetCostPreset()
    }
    NormalizeFleetCostMonthControls()
    RefreshFleetCostOverview()
}

OnFleetCostToMonthChanged(*) {
    global FleetCostPresetCtrl

    if IsObject(FleetCostPresetCtrl) && FleetCostPresetCtrl.Value != 1 {
        FleetCostPresetCtrl.Value := 1
    }
    NormalizeFleetCostMonthControls()
    RefreshFleetCostOverview()
}

OpenVehicleDetailFromCostSummary(*) {
    global CostSummaryVehicleId

    vehicle := FindVehicleById(CostSummaryVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleCostSummaryDialog()
    OpenVehicleDetailDialog(vehicle)
}

ExportVehicleCostSummaryTsv(*) {
    global AppTitle, A_DefaultDialogTitle

    context := GetCurrentVehicleCostExportContext()
    if !IsObject(context) {
        MsgBox("Nepodařilo se připravit souhrn nákladů k exportu.", AppTitle, 0x30)
        return
    }

    A_DefaultDialogTitle := AppTitle
    exportPath := FileSelect("S16", GetDefaultVehicleCostExportPath(context, "souhrn", "tsv"), "Export souhrnu nákladů", "TSV soubor (*.tsv)")
    if (exportPath = "") {
        return
    }

    exportPath := EnsureFileExtension(exportPath, ".tsv")
    try {
        WriteTextFileUtf8(exportPath, BuildVehicleCostSummaryTsvContent(context))
        MsgBox("Souhrn nákladů byl vyexportován.`n`nSoubor:`n" exportPath, AppTitle, 0x40)
    } catch as err {
        MsgBox("Export souhrnu nákladů se nepodařil.`n`n" err.Message, AppTitle, 0x30)
    }
}

ExportVehicleCostDetailTsv(*) {
    global AppTitle, A_DefaultDialogTitle

    context := GetCurrentVehicleCostExportContext()
    if !IsObject(context) {
        MsgBox("Nepodařilo se připravit detail nákladů k exportu.", AppTitle, 0x30)
        return
    }

    A_DefaultDialogTitle := AppTitle
    exportPath := FileSelect("S16", GetDefaultVehicleCostExportPath(context, "detail", "tsv"), "Export detailu nákladů", "TSV soubor (*.tsv)")
    if (exportPath = "") {
        return
    }

    exportPath := EnsureFileExtension(exportPath, ".tsv")
    try {
        WriteTextFileUtf8(exportPath, BuildVehicleCostDetailTsvContent(context))
        MsgBox("Detail nákladů byl vyexportován.`n`nSoubor:`n" exportPath, AppTitle, 0x40)
    } catch as err {
        MsgBox("Export detailu nákladů se nepodařil.`n`n" err.Message, AppTitle, 0x30)
    }
}

ExportVehicleCostReportHtml(*) {
    global AppTitle, A_DefaultDialogTitle

    context := GetCurrentVehicleCostExportContext()
    if !IsObject(context) {
        MsgBox("Nepodařilo se připravit HTML sestavu nákladů.", AppTitle, 0x30)
        return
    }

    A_DefaultDialogTitle := AppTitle
    exportPath := FileSelect("S16", GetDefaultVehicleCostExportPath(context, "sestava", "html"), "Export HTML sestavy nákladů", "HTML soubor (*.html)")
    if (exportPath = "") {
        return
    }

    exportPath := EnsureFileExtension(exportPath, ".html")
    try {
        WriteTextFileUtf8(exportPath, BuildVehicleCostReportHtml(context))
        Run('"' exportPath '"')
    } catch as err {
        MsgBox("Export HTML sestavy nákladů se nepodařil.`n`n" err.Message, AppTitle, 0x30)
    }
}

GetCurrentVehicleCostExportContext() {
    global CostSummaryVehicleId, CostSummaryFromMonthCtrl, CostSummaryToMonthCtrl

    if (CostSummaryVehicleId = "") {
        return ""
    }

    vehicle := FindVehicleById(CostSummaryVehicleId)
    if !IsObject(vehicle) {
        return ""
    }

    yearLabel := GetSelectedCostSummaryYear()
    fromMonth := GetSelectedMonthOptionValue(CostSummaryFromMonthCtrl)
    toMonth := GetSelectedMonthOptionValue(CostSummaryToMonthCtrl)
    NormalizeCostSummaryMonthRange(&fromMonth, &toMonth)

    return {
        vehicle: vehicle,
        year: yearLabel,
        fromMonth: fromMonth,
        toMonth: toMonth,
        periodSummary: BuildVehicleCostPeriodSummary(vehicle.id, yearLabel, fromMonth, toMonth),
        detailEntries: BuildVehicleCostDetailEntries(vehicle.id, yearLabel, fromMonth, toMonth),
        yearlySummary: BuildVehicleCostSummary(vehicle.id)
    }
}

GetDefaultVehicleCostExportPath(context, exportKind, extensionWithoutDot) {
    safeVehicleName := MakeSafeFileNamePart(context.vehicle.name)
    periodLabel := MakeSafeFileNamePart(BuildVehicleCostPeriodLabel(context.year, context.fromMonth, context.toMonth))
    timestamp := FormatTime(A_Now, "yyyy-MM-dd_HH-mm")
    return A_MyDocuments "\" safeVehicleName "_naklady_" exportKind "_" periodLabel "_" timestamp "." extensionWithoutDot
}

BuildVehicleCostSummaryTsvContent(context) {
    periodLabel := BuildVehicleCostPeriodLabel(context.year, context.fromMonth, context.toMonth)
    total := context.periodSummary.totalFuel + context.periodSummary.totalHistory + context.periodSummary.totalRecords
    lines := []
    lines.Push(BuildTsvRow(["Sekce", "Vozidlo", "Období", "Rok", "Tankování položek", "Tankování částka", "Historie položek", "Historie částka", "Doklady položek", "Doklady částka", "Celkem položek", "Celkem částka"]))
    lines.Push(BuildTsvRow([
        "Vybrané období",
        context.vehicle.name,
        periodLabel,
        context.year,
        context.periodSummary.fuelCount,
        FormatCostNumberForTsv(context.periodSummary.totalFuel),
        context.periodSummary.historyCount,
        FormatCostNumberForTsv(context.periodSummary.totalHistory),
        context.periodSummary.recordCount,
        FormatCostNumberForTsv(context.periodSummary.totalRecords),
        context.periodSummary.parsedCount,
        FormatCostNumberForTsv(total)
    ]))

    for item in context.yearlySummary.years {
        lines.Push(BuildTsvRow([
            "Přehled podle roku",
            context.vehicle.name,
            "",
            item.year,
            "",
            FormatCostNumberForTsv(item.fuel),
            "",
            FormatCostNumberForTsv(item.history),
            "",
            FormatCostNumberForTsv(item.records),
            "",
            FormatCostNumberForTsv(item.total)
        ]))
    }

    return JoinLines(lines)
}

BuildVehicleCostDetailTsvContent(context) {
    periodLabel := BuildVehicleCostPeriodLabel(context.year, context.fromMonth, context.toMonth)
    lines := []
    lines.Push(BuildTsvRow(["Vozidlo", "Období", "Datum", "Skupina", "Název", "Částka", "Doplňující údaje", "Poznámka"]))

    if (context.detailEntries.Length = 0) {
        lines.Push(BuildTsvRow([context.vehicle.name, periodLabel, "", "Bez položek", "", "", "", ""]))
        return JoinLines(lines)
    }

    for entry in context.detailEntries {
        lines.Push(BuildTsvRow([
            context.vehicle.name,
            periodLabel,
            entry.dateText,
            entry.group,
            entry.title,
            FormatCostNumberForTsv(entry.amount),
            entry.extraInfo,
            entry.note
        ]))
    }

    return JoinLines(lines)
}

BuildVehicleCostReportHtml(context) {
    generatedAt := FormatTime(A_Now, "dd.MM.yyyy HH:mm")
    periodLabel := BuildVehicleCostPeriodLabel(context.year, context.fromMonth, context.toMonth)
    total := context.periodSummary.totalFuel + context.periodSummary.totalHistory + context.periodSummary.totalRecords

    html := "<!DOCTYPE html><html lang='cs'><head><meta charset='utf-8'>"
    html .= "<title>Vehimap - Náklady vozidla</title>"
    html .= "<style>"
    html .= "body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#1d2329;background:#f7f7f4;}"
    html .= "h1,h2{margin:0 0 12px;}"
    html .= ".meta,.summary{margin:0 0 18px;}"
    html .= ".card{background:#fff;border:1px solid #d8ddd4;border-radius:8px;padding:16px;margin:0 0 18px;}"
    html .= "table{width:100%;border-collapse:collapse;background:#fff;}"
    html .= "th,td{border:1px solid #cfd5cb;padding:8px 10px;text-align:left;vertical-align:top;}"
    html .= "th{background:#edf2e8;}"
    html .= ".empty{font-style:italic;color:#666;}"
    html .= "</style></head><body>"
    html .= "<h1>Náklady vozidla</h1>"
    html .= "<p class='meta'>Vozidlo: " HtmlEscape(context.vehicle.name) " | Období: " HtmlEscape(periodLabel) " | Vytvořeno: " HtmlEscape(generatedAt) "</p>"

    html .= "<div class='card'><h2>Souhrn období</h2>"
    html .= "<p class='summary'>Celkem nákladů: " HtmlEscape(FormatCostAmount(total)) ". Započteno položek: " context.periodSummary.parsedCount "."
    if (context.periodSummary.skippedCount > 0) {
        html .= " Přeskočeno nečíselných částek: " context.periodSummary.skippedCount "."
    }
    if (context.periodSummary.undatedCount > 0) {
        html .= " Položek bez použitelného data: " context.periodSummary.undatedCount "."
    }
    html .= "</p>"
    html .= "<table><thead><tr><th>Skupina</th><th>Počet položek</th><th>Částka</th></tr></thead><tbody>"
    html .= "<tr><td>Tankování</td><td>" context.periodSummary.fuelCount "</td><td>" HtmlEscape(FormatCostAmount(context.periodSummary.totalFuel)) "</td></tr>"
    html .= "<tr><td>Historie a servis</td><td>" context.periodSummary.historyCount "</td><td>" HtmlEscape(FormatCostAmount(context.periodSummary.totalHistory)) "</td></tr>"
    html .= "<tr><td>Doklady a pojištění</td><td>" context.periodSummary.recordCount "</td><td>" HtmlEscape(FormatCostAmount(context.periodSummary.totalRecords)) "</td></tr>"
    html .= "<tr><td><strong>Celkem</strong></td><td><strong>" context.periodSummary.parsedCount "</strong></td><td><strong>" HtmlEscape(FormatCostAmount(total)) "</strong></td></tr>"
    html .= "</tbody></table></div>"

    html .= "<div class='card'><h2>Detail položek období</h2>"
    if (context.detailEntries.Length = 0) {
        html .= "<p class='empty'>Ve zvoleném období nejsou žádné položky s vyplněnou číselnou částkou.</p>"
    } else {
        html .= "<table><thead><tr><th>Datum</th><th>Skupina</th><th>Název</th><th>Částka</th><th>Doplňující údaje</th><th>Poznámka</th></tr></thead><tbody>"
        for entry in context.detailEntries {
            html .= "<tr>"
            html .= "<td>" HtmlEscape(entry.dateText) "</td>"
            html .= "<td>" HtmlEscape(entry.group) "</td>"
            html .= "<td>" HtmlEscape(entry.title) "</td>"
            html .= "<td>" HtmlEscape(FormatCostAmount(entry.amount)) "</td>"
            html .= "<td>" HtmlEscape(entry.extraInfo) "</td>"
            html .= "<td>" HtmlEscape(entry.note) "</td>"
            html .= "</tr>"
        }
        html .= "</tbody></table>"
    }
    html .= "</div>"

    html .= "<div class='card'><h2>Přehled podle roku</h2>"
    if (context.yearlySummary.years.Length = 0) {
        html .= "<p class='empty'>K tomuto vozidlu zatím nejsou žádné nákladové položky s číselnou částkou.</p>"
    } else {
        html .= "<table><thead><tr><th>Rok</th><th>Tankování</th><th>Historie a servis</th><th>Doklady a pojištění</th><th>Celkem</th></tr></thead><tbody>"
        for item in context.yearlySummary.years {
            html .= "<tr>"
            html .= "<td>" HtmlEscape(item.year) "</td>"
            html .= "<td>" HtmlEscape(FormatCostAmount(item.fuel)) "</td>"
            html .= "<td>" HtmlEscape(FormatCostAmount(item.history)) "</td>"
            html .= "<td>" HtmlEscape(FormatCostAmount(item.records)) "</td>"
            html .= "<td>" HtmlEscape(FormatCostAmount(item.total)) "</td>"
            html .= "</tr>"
        }
        html .= "</tbody></table>"
    }
    html .= "</div>"

    html .= "</body></html>"
    return html
}

BuildVehicleCostDetailEntries(vehicleId, yearLabel, fromMonth, toMonth) {
    global VehicleFuelLog, VehicleHistory, VehicleRecords

    entries := []
    yearValue := yearLabel + 0

    for entry in VehicleFuelLog {
        if (entry.vehicleId != vehicleId || Trim(entry.totalCost) = "") {
            continue
        }
        if !TryGetEventYearMonth(entry.entryDate, &entryYear, &entryMonth) {
            continue
        }
        if (entryYear != yearValue || entryMonth < fromMonth || entryMonth > toMonth) {
            continue
        }
        if !TryParseMoneyAmount(entry.totalCost, &amount) {
            continue
        }

        extraParts := []
        if (Trim(entry.fuelType) != "") {
            extraParts.Push("Palivo: " entry.fuelType)
        }
        if (Trim(entry.liters) != "") {
            extraParts.Push("Litry: " entry.liters)
        }
        if (Trim(entry.odometer) != "") {
            extraParts.Push("Tachometr: " entry.odometer)
        }
        if entry.fullTank {
            extraParts.Push("Plná nádrž")
        }

        entries.Push({
            sortStamp: BuildSortableEventDateStamp(entry.entryDate),
            dateText: entry.entryDate,
            group: "Tankování",
            title: (Trim(entry.fuelType) != "") ? entry.fuelType : "Tankování",
            amount: amount,
            extraInfo: JoinInline(extraParts, "; "),
            note: entry.note
        })
    }

    for entry in VehicleHistory {
        if (entry.vehicleId != vehicleId || Trim(entry.cost) = "") {
            continue
        }
        if !TryGetEventYearMonth(entry.eventDate, &entryYear, &entryMonth) {
            continue
        }
        if (entryYear != yearValue || entryMonth < fromMonth || entryMonth > toMonth) {
            continue
        }
        if !TryParseMoneyAmount(entry.cost, &amount) {
            continue
        }

        extraInfo := Trim(entry.odometer) != "" ? "Tachometr: " entry.odometer : ""
        entries.Push({
            sortStamp: BuildSortableEventDateStamp(entry.eventDate),
            dateText: entry.eventDate,
            group: "Historie a servis",
            title: entry.eventType,
            amount: amount,
            extraInfo: extraInfo,
            note: entry.note
        })
    }

    for entry in VehicleRecords {
        if (entry.vehicleId != vehicleId || Trim(entry.price) = "") {
            continue
        }
        if !TryGetRecordYearMonth(entry, &entryYear, &entryMonth) {
            continue
        }
        if (entryYear != yearValue || entryMonth < fromMonth || entryMonth > toMonth) {
            continue
        }
        if !TryParseMoneyAmount(entry.price, &amount) {
            continue
        }

        dateText := Trim(entry.validTo) != "" ? entry.validTo : entry.validFrom
        extraParts := []
        if (Trim(entry.recordType) != "") {
            extraParts.Push("Druh: " entry.recordType)
        }
        if (Trim(entry.provider) != "") {
            extraParts.Push("Poskytovatel: " entry.provider)
        }
        if (Trim(entry.filePath) != "") {
            extraParts.Push("Soubor: " GetVehicleRecordDisplayPath(entry))
        }

        entries.Push({
            sortStamp: ParseDueStamp(dateText),
            dateText: dateText,
            group: "Doklady a pojištění",
            title: entry.title,
            amount: amount,
            extraInfo: JoinInline(extraParts, "; "),
            note: entry.note
        })
    }

    SortVehicleCostDetailEntries(&entries)
    return entries
}

SortVehicleCostDetailEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        swapped := false
        Loop count - A_Index {
            left := items[A_Index]
            right := items[A_Index + 1]
            if (CompareVehicleCostDetailEntries(left, right) > 0) {
                items[A_Index] := right
                items[A_Index + 1] := left
                swapped := true
            }
        }
        if !swapped {
            break
        }
    }
}

CompareVehicleCostDetailEntries(left, right) {
    leftStamp := left.sortStamp
    rightStamp := right.sortStamp
    if (leftStamp = "") {
        leftStamp := "00000000000000"
    }
    if (rightStamp = "") {
        rightStamp := "00000000000000"
    }

    if (leftStamp > rightStamp) {
        return -1
    }
    if (leftStamp < rightStamp) {
        return 1
    }

    return CompareTextValues(left.title, right.title)
}

BuildSortableEventDateStamp(eventDate) {
    normalized := NormalizeEventDate(eventDate)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, ".")
    return parts[3] parts[2] parts[1] "235959"
}

BuildTsvRow(fields) {
    cleanFields := []
    for field in fields {
        cleanFields.Push(SanitizeTsvCell(field))
    }
    return JoinInline(cleanFields, "`t")
}

SanitizeTsvCell(value) {
    value := StrReplace(value, "`t", " ")
    value := StrReplace(value, "`r", " ")
    value := StrReplace(value, "`n", " ")
    return value
}

FormatCostNumberForTsv(value) {
    return StrReplace(Format("{:.2f}", value + 0.0), ".", ",")
}

EnsureFileExtension(path, extension) {
    if (StrLower(SubStr(path, -StrLen(extension) + 1)) != StrLower(extension)) {
        path .= extension
    }

    return path
}

MakeSafeFileNamePart(text) {
    text := Trim(text)
    if (text = "") {
        return "vehimap"
    }

    text := RegExReplace(text, '[\\/:*?"<>|]', "_")
    text := RegExReplace(text, "\s+", "_")
    text := RegExReplace(text, "_{2,}", "_")
    text := Trim(text, " _.")
    return text = "" ? "vehimap" : text
}

OnCostSummaryPresetChanged(*) {
    ApplyCostSummaryPreset()
    RefreshVehicleCostPeriodSummary()
}

OnCostSummaryFromMonthChanged(*) {
    if (GetSelectedCostSummaryPresetLength() > 0) {
        ApplyCostSummaryPreset()
    } else {
        NormalizeCostSummaryMonthControls()
    }
    RefreshVehicleCostPeriodSummary()
}

OnCostSummaryToMonthChanged(*) {
    global CostSummaryPresetCtrl

    if IsObject(CostSummaryPresetCtrl) && CostSummaryPresetCtrl.Value != 1 {
        CostSummaryPresetCtrl.Value := 1
    }
    NormalizeCostSummaryMonthControls()
    RefreshVehicleCostPeriodSummary()
}

RefreshVehicleCostPeriodSummary(*) {
    global CostSummaryVehicleId, CostSummaryPeriodSummaryLabel, CostSummaryPeriodList, CostSummaryFromMonthCtrl, CostSummaryToMonthCtrl

    if !IsObject(CostSummaryPeriodSummaryLabel) || !IsObject(CostSummaryPeriodList) || CostSummaryVehicleId = "" {
        return
    }

    yearLabel := GetSelectedCostSummaryYear()
    fromMonth := GetSelectedMonthOptionValue(CostSummaryFromMonthCtrl)
    toMonth := GetSelectedMonthOptionValue(CostSummaryToMonthCtrl)
    NormalizeCostSummaryMonthRange(&fromMonth, &toMonth)
    SetCostSummaryMonthControls(fromMonth, toMonth)

    summary := BuildVehicleCostPeriodSummary(CostSummaryVehicleId, yearLabel, fromMonth, toMonth)
    CostSummaryPeriodSummaryLabel.Text := BuildVehicleCostPeriodSummaryText(summary)

    CostSummaryPeriodList.Opt("-Redraw")
    CostSummaryPeriodList.Delete()
    CostSummaryPeriodList.Add("", "Tankování", summary.fuelCount, FormatCostAmount(summary.totalFuel))
    CostSummaryPeriodList.Add("", "Historie a servis", summary.historyCount, FormatCostAmount(summary.totalHistory))
    CostSummaryPeriodList.Add("", "Doklady a pojištění", summary.recordCount, FormatCostAmount(summary.totalRecords))
    CostSummaryPeriodList.Add("", "Celkem", summary.parsedCount, FormatCostAmount(summary.totalFuel + summary.totalHistory + summary.totalRecords))
    CostSummaryPeriodList.Opt("+Redraw")
}

ApplyCostSummaryPreset() {
    global CostSummaryFromMonthCtrl

    presetLength := GetSelectedCostSummaryPresetLength()
    if (presetLength < 1) {
        NormalizeCostSummaryMonthControls()
        return
    }

    if (presetLength >= 12) {
        SetCostSummaryMonthControls(1, 12)
        return
    }

    fromMonth := GetSelectedMonthOptionValue(CostSummaryFromMonthCtrl)
    toMonth := fromMonth + presetLength - 1
    if (toMonth > 12) {
        toMonth := 12
    }
    SetCostSummaryMonthControls(fromMonth, toMonth)
}

NormalizeCostSummaryMonthControls() {
    global CostSummaryFromMonthCtrl, CostSummaryToMonthCtrl

    fromMonth := GetSelectedMonthOptionValue(CostSummaryFromMonthCtrl)
    toMonth := GetSelectedMonthOptionValue(CostSummaryToMonthCtrl)
    NormalizeCostSummaryMonthRange(&fromMonth, &toMonth)
    SetCostSummaryMonthControls(fromMonth, toMonth)
}

NormalizeCostSummaryMonthRange(&fromMonth, &toMonth) {
    if (fromMonth < 1) {
        fromMonth := 1
    }
    if (fromMonth > 12) {
        fromMonth := 12
    }
    if (toMonth < 1) {
        toMonth := 1
    }
    if (toMonth > 12) {
        toMonth := 12
    }

    if (fromMonth > toMonth) {
        temp := fromMonth
        fromMonth := toMonth
        toMonth := temp
    }
}

SetCostSummaryMonthControls(fromMonth, toMonth) {
    global CostSummaryFromMonthCtrl, CostSummaryToMonthCtrl

    if IsObject(CostSummaryFromMonthCtrl) {
        CostSummaryFromMonthCtrl.Value := fromMonth
    }
    if IsObject(CostSummaryToMonthCtrl) {
        CostSummaryToMonthCtrl.Value := toMonth
    }
}

GetSelectedCostSummaryPresetLength() {
    global CostSummaryPresetCtrl

    if !IsObject(CostSummaryPresetCtrl) {
        return 12
    }

    switch CostSummaryPresetCtrl.Value {
        case 2:
            return 1
        case 3:
            return 2
        case 4:
            return 3
        case 5:
            return 6
        case 6:
            return 9
        case 7:
            return 12
        default:
            return 0
    }
}

GetSelectedCostSummaryYear() {
    global CostSummaryPeriodYearCtrl

    if !IsObject(CostSummaryPeriodYearCtrl) {
        return FormatTime(A_Now, "yyyy")
    }

    yearLabel := Trim(CostSummaryPeriodYearCtrl.Text)
    if RegExMatch(yearLabel, "^\d{4}$") {
        return yearLabel
    }

    return FormatTime(A_Now, "yyyy")
}

GetSelectedMonthOptionValue(ctrl) {
    if !IsObject(ctrl) {
        return 1
    }

    if (ctrl.Value >= 1 && ctrl.Value <= 12) {
        return ctrl.Value
    }

    if RegExMatch(ctrl.Text, "^(\d{2})", &match) {
        month := match[1] + 0
        if (month >= 1 && month <= 12) {
            return month
        }
    }

    return 1
}

GetSelectedFleetCostPresetLength() {
    global FleetCostPresetCtrl

    if !IsObject(FleetCostPresetCtrl) {
        return 12
    }

    switch FleetCostPresetCtrl.Value {
        case 2:
            return 1
        case 3:
            return 2
        case 4:
            return 3
        case 5:
            return 6
        case 6:
            return 9
        case 7:
            return 12
        default:
            return 0
    }
}

GetSelectedFleetCostYear() {
    global FleetCostPeriodYearCtrl

    if !IsObject(FleetCostPeriodYearCtrl) {
        return FormatTime(A_Now, "yyyy")
    }

    yearLabel := Trim(FleetCostPeriodYearCtrl.Text)
    if RegExMatch(yearLabel, "^\d{4}$") {
        return yearLabel
    }

    return FormatTime(A_Now, "yyyy")
}

ApplyFleetCostPreset() {
    global FleetCostFromMonthCtrl

    presetLength := GetSelectedFleetCostPresetLength()
    if (presetLength < 1) {
        NormalizeFleetCostMonthControls()
        return
    }

    if (presetLength >= 12) {
        SetFleetCostMonthControls(1, 12)
        return
    }

    fromMonth := GetSelectedMonthOptionValue(FleetCostFromMonthCtrl)
    toMonth := fromMonth + presetLength - 1
    if (toMonth > 12) {
        toMonth := 12
        fromMonth := toMonth - presetLength + 1
    }

    SetFleetCostMonthControls(fromMonth, toMonth)
}

NormalizeFleetCostMonthControls() {
    global FleetCostFromMonthCtrl, FleetCostToMonthCtrl

    fromMonth := GetSelectedMonthOptionValue(FleetCostFromMonthCtrl)
    toMonth := GetSelectedMonthOptionValue(FleetCostToMonthCtrl)
    NormalizeFleetCostMonthRange(&fromMonth, &toMonth)
    SetFleetCostMonthControls(fromMonth, toMonth)
}

NormalizeFleetCostMonthRange(&fromMonth, &toMonth) {
    NormalizeCostSummaryMonthRange(&fromMonth, &toMonth)
}

SetFleetCostMonthControls(fromMonth, toMonth) {
    global FleetCostFromMonthCtrl, FleetCostToMonthCtrl

    if IsObject(FleetCostFromMonthCtrl) {
        FleetCostFromMonthCtrl.Value := fromMonth
    }
    if IsObject(FleetCostToMonthCtrl) {
        FleetCostToMonthCtrl.Value := toMonth
    }
}

GetFleetCostSummaryYearOptions() {
    global VehicleFuelLog, VehicleHistory, VehicleRecords

    seen := Map()
    items := []

    AddCostSummaryYearOption(&items, &seen, FormatTime(A_Now, "yyyy"))

    for entry in VehicleFuelLog {
        AddCostSummaryYearOption(&items, &seen, GetYearFromEventDate(entry.entryDate))
    }

    for entry in VehicleHistory {
        AddCostSummaryYearOption(&items, &seen, GetYearFromEventDate(entry.eventDate))
    }

    for entry in VehicleRecords {
        yearLabel := GetYearFromMonthYear(entry.validTo)
        if (yearLabel = "") {
            yearLabel := GetYearFromMonthYear(entry.validFrom)
        }
        AddCostSummaryYearOption(&items, &seen, yearLabel)
    }

    SortYearLabelsDescending(&items)
    if (items.Length = 0) {
        items.Push(FormatTime(A_Now, "yyyy"))
    }

    return items
}

BuildFleetCostPeriodSummary(yearLabel, fromMonth, toMonth) {
    yearValue := yearLabel + 0
    startMonthIndex := GetMonthIndex(yearValue, fromMonth)
    endMonthIndex := GetMonthIndex(yearValue, toMonth)
    return BuildFleetCostPeriodSummaryForRange(yearLabel, fromMonth, toMonth, startMonthIndex, endMonthIndex)
}

BuildFleetCostPeriodSummaryForRange(yearLabel, fromMonth, toMonth, startMonthIndex, endMonthIndex, includeComparison := true) {
    global Vehicles, VehicleFuelLog, VehicleHistory, VehicleRecords

    rowsMap := Map()
    summary := {
        year: yearLabel,
        fromMonth: fromMonth,
        toMonth: toMonth,
        startMonthIndex: startMonthIndex,
        endMonthIndex: endMonthIndex,
        periodLabel: BuildMonthIndexRangeLabel(startMonthIndex, endMonthIndex),
        totalFuel: 0.0,
        totalHistory: 0.0,
        totalRecords: 0.0,
        total: 0.0,
        fuelCount: 0,
        historyCount: 0,
        recordCount: 0,
        parsedCount: 0,
        skippedCount: 0,
        undatedCount: 0,
        activeVehicleCount: 0,
        activeWithoutCostCount: 0,
        vehiclesWithCosts: 0,
        totalDistanceKm: 0,
        distanceText: "Nedostupné",
        costPerKm: "",
        costPerKmText: "Nedostupné",
        costPerKmVehicleCount: 0,
        costPerKmUnavailableCount: 0,
        costPerKmCostTotal: 0.0,
        comparison: "",
        rows: []
    }

    for vehicle in Vehicles {
        if !IsVehicleInactive(vehicle) {
            summary.activeVehicleCount += 1
        }
    }

    for entry in VehicleFuelLog {
        row := EnsureFleetCostRow(rowsMap, entry.vehicleId)
        if !IsObject(row) || Trim(entry.totalCost) = "" {
            continue
        }
        monthIndex := GetEventDateMonthIndex(entry.entryDate)
        if (monthIndex = "") {
            summary.undatedCount += 1
            row.undatedCount += 1
            continue
        }
        if (monthIndex < startMonthIndex || monthIndex > endMonthIndex) {
            continue
        }
        if TryParseMoneyAmount(entry.totalCost, &amount) {
            row.fuel += amount
            row.total += amount
            row.parsedCount += 1
            summary.totalFuel += amount
            summary.fuelCount += 1
            summary.parsedCount += 1
        } else {
            row.skippedCount += 1
            summary.skippedCount += 1
        }
    }

    for entry in VehicleHistory {
        row := EnsureFleetCostRow(rowsMap, entry.vehicleId)
        if !IsObject(row) || Trim(entry.cost) = "" {
            continue
        }
        monthIndex := GetEventDateMonthIndex(entry.eventDate)
        if (monthIndex = "") {
            summary.undatedCount += 1
            row.undatedCount += 1
            continue
        }
        if (monthIndex < startMonthIndex || monthIndex > endMonthIndex) {
            continue
        }
        if TryParseMoneyAmount(entry.cost, &amount) {
            row.history += amount
            row.total += amount
            row.parsedCount += 1
            summary.totalHistory += amount
            summary.historyCount += 1
            summary.parsedCount += 1
        } else {
            row.skippedCount += 1
            summary.skippedCount += 1
        }
    }

    for entry in VehicleRecords {
        row := EnsureFleetCostRow(rowsMap, entry.vehicleId)
        if !IsObject(row) || Trim(entry.price) = "" {
            continue
        }
        monthIndex := GetRecordMonthIndex(entry)
        if (monthIndex = "") {
            summary.undatedCount += 1
            row.undatedCount += 1
            continue
        }
        if (monthIndex < startMonthIndex || monthIndex > endMonthIndex) {
            continue
        }
        if TryParseMoneyAmount(entry.price, &amount) {
            row.records += amount
            row.total += amount
            row.parsedCount += 1
            summary.totalRecords += amount
            summary.recordCount += 1
            summary.parsedCount += 1
        } else {
            row.skippedCount += 1
            summary.skippedCount += 1
        }
    }

    for vehicle in Vehicles {
        if rowsMap.Has(vehicle.id) {
            continue
        }
        if IsVehicleInactive(vehicle) {
            continue
        }

        rowsMap[vehicle.id] := CreateFleetCostRow(vehicle)
    }

    rows := []
    for _, row in rowsMap {
        row.distanceSummary := GetVehicleDistanceSummaryForMonthRange(row.vehicle.id, startMonthIndex, endMonthIndex)
        row.distanceText := BuildDistanceText(row.distanceSummary)
        row.costPerKm := ""
        row.costPerKmText := BuildCostPerKmText(row.total, row.distanceSummary)
        if (row.total > 0) {
            if (row.distanceSummary.available && row.distanceSummary.distanceKm > 0) {
                row.costPerKm := row.total / row.distanceSummary.distanceKm
                row.costPerKmText := FormatCostPerKmValue(row.costPerKm)
                summary.totalDistanceKm += row.distanceSummary.distanceKm
                summary.costPerKmCostTotal += row.total
                summary.costPerKmVehicleCount += 1
            } else {
                summary.costPerKmUnavailableCount += 1
            }
        }
        row.status := BuildFleetCostRowStatusText(row)
        rows.Push(row)
        if (row.total > 0) {
            summary.vehiclesWithCosts += 1
        } else if !IsVehicleInactive(row.vehicle) {
            summary.activeWithoutCostCount += 1
        }
    }

    summary.total := summary.totalFuel + summary.totalHistory + summary.totalRecords
    if (summary.totalDistanceKm > 0) {
        summary.distanceText := Trim(summary.totalDistanceKm) " km"
    }
    if (summary.costPerKmVehicleCount > 0 && summary.totalDistanceKm > 0) {
        summary.costPerKm := summary.costPerKmCostTotal / summary.totalDistanceKm
        summary.costPerKmText := FormatCostPerKmValue(summary.costPerKm)
    }

    SortFleetCostRows(&rows)
    summary.rows := rows
    if includeComparison {
        comparisonContext := BuildVehicleCostComparisonContext(yearLabel, fromMonth, toMonth)
        previousSummary := BuildFleetCostPeriodSummaryForRange(
            comparisonContext.previousYear,
            comparisonContext.previousFromMonth,
            comparisonContext.previousToMonth,
            comparisonContext.previousStartIndex,
            comparisonContext.previousEndIndex,
            false
        )
        summary.comparison := BuildCostPeriodComparisonSummary(summary, previousSummary)
    }
    return summary
}

EnsureFleetCostRow(rowsMap, vehicleId) {
    if (vehicleId = "") {
        return ""
    }

    if rowsMap.Has(vehicleId) {
        return rowsMap[vehicleId]
    }

    rowsMap[vehicleId] := CreateFleetCostRow(GetFleetCostVehicle(vehicleId))
    return rowsMap[vehicleId]
}

GetFleetCostVehicle(vehicleId) {
    vehicle := FindVehicleById(vehicleId)
    if IsObject(vehicle) {
        return vehicle
    }

    return {
        id: vehicleId,
        name: "(neznámé vozidlo)",
        category: "",
        plate: "",
        nextTk: "",
        greenCardTo: ""
    }
}

CreateFleetCostRow(vehicle) {
    return {
        vehicle: vehicle,
        fuel: 0.0,
        history: 0.0,
        records: 0.0,
        total: 0.0,
        parsedCount: 0,
        skippedCount: 0,
        undatedCount: 0,
        distanceSummary: "",
        distanceText: "Nedostupné",
        costPerKm: "",
        costPerKmText: "Nedostupné",
        status: ""
    }
}

BuildFleetCostPeriodSummaryText(summary) {
    total := summary.total
    periodLabel := summary.periodLabel
    if (summary.parsedCount = 0) {
        text := "Období " periodLabel ": zatím nejsou započítané žádné číselné náklady."
    } else {
        text := "Období " periodLabel ": celkem " FormatCostAmount(total) " u " summary.vehiclesWithCosts " vozidel."
        topText := BuildFleetCostTopVehiclesText(summary)
        if (topText != "") {
            text .= " Nejvýš: " topText "."
        }
        text .= " Ujeto km: " summary.distanceText "."
        text .= " Cena / km: " summary.costPerKmText "."
    }

    if (summary.activeWithoutCostCount > 0) {
        text .= " Bez číselného nákladu: " summary.activeWithoutCostCount " z " summary.activeVehicleCount " aktivních vozidel."
    }
    if (summary.costPerKmUnavailableCount > 0) {
        text .= " Cena / km nedostupná u " summary.costPerKmUnavailableCount " vozidel s nákladem."
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

    comparisonText := BuildCostPeriodComparisonText(summary.comparison)
    if (comparisonText != "") {
        text .= " " comparisonText
    }

    return text
}

BuildFleetCostRowsSummaryText(summary) {
    if (summary.rows.Length = 0) {
        return "Momentálně tu nejsou žádná vozidla ani položky, které by šlo do přehledu zahrnout."
    }

    issueVehicleCount := CountFleetCostRowsWithIssues(summary.rows)
    text := "Zobrazeno " summary.rows.Length " vozidel."
    if (summary.vehiclesWithCosts > 0) {
        text .= " Náklad v období má " summary.vehiclesWithCosts " vozidel."
    }
    if (summary.activeWithoutCostCount > 0) {
        text .= " Bez nákladu zůstává " summary.activeWithoutCostCount " aktivních vozidel."
    }
    if (summary.costPerKmUnavailableCount > 0) {
        text .= " Cena / km chybí u " summary.costPerKmUnavailableCount " vozidel s nákladem."
    }
    if (issueVehicleCount > 0) {
        text .= " U " issueVehicleCount " vozidel je navíc stav k řešení."
    }

    highlights := BuildFleetCostRowHighlightsText(summary.rows)
    if (highlights != "") {
        text .= " Nejvíc pálí: " highlights "."
    }

    return text
}

BuildFleetCostTopVehiclesText(summary, limit := 3) {
    parts := []
    for row in summary.rows {
        if (row.total <= 0) {
            continue
        }

        parts.Push(ShortenText(row.vehicle.name, 24) " " FormatCostAmount(row.total))
        if (parts.Length >= limit) {
            break
        }
    }

    return JoinInline(parts, ", ")
}

BuildFleetCostRowHighlightsText(rows, limit := 3) {
    parts := []
    for row in rows {
        if !FleetCostRowHasActionableIssue(row) {
            continue
        }

        detail := GetFirstFleetCostRowIssueText(row)
        if (detail = "") {
            continue
        }

        parts.Push(ShortenText(row.vehicle.name, 22) " (" detail ")")
        if (parts.Length >= limit) {
            break
        }
    }

    return JoinInline(parts, ", ")
}

CountFleetCostRowsWithIssues(rows) {
    count := 0
    for row in rows {
        if FleetCostRowHasActionableIssue(row) {
            count += 1
        }
    }

    return count
}

BuildFleetCostRowStatusText(row) {
    parts := []
    attentionText := GetVehicleStatusText(row.vehicle)
    if (attentionText != "") {
        parts.Push(attentionText)
    }
    if VehicleHasMissingGreenCard(row.vehicle) {
        parts.Push("ZK chybí")
    }

    meta := GetVehicleMeta(row.vehicle.id)
    state := NormalizeVehicleState(meta.state)
    if (state != "" && state != "Běžný provoz") {
        parts.Push(state)
    }

    if (row.total <= 0 && !IsVehicleInactive(row.vehicle)) {
        parts.Push("Bez nákladu v období")
    }
    distanceIssueText := GetFleetCostRowDistanceIssueText(row)
    if (distanceIssueText != "") {
        parts.Push(distanceIssueText)
    }
    if (row.skippedCount > 0) {
        parts.Push("Nečíselné částky: " row.skippedCount)
    }
    if (row.undatedCount > 0) {
        parts.Push("Bez data: " row.undatedCount)
    }
    if (parts.Length = 0) {
        return row.parsedCount > 0 ? "Započteno položek: " row.parsedCount : "V pořádku"
    }

    return JoinInline(parts, "; ")
}

FleetCostRowHasActionableIssue(row) {
    return GetFleetCostRowIssueScore(row) > 0
}

GetFirstFleetCostRowIssueText(row) {
    attentionText := GetVehicleStatusText(row.vehicle)
    if (attentionText != "") {
        return attentionText
    }
    if VehicleHasMissingGreenCard(row.vehicle) {
        return "ZK chybí"
    }
    distanceIssueText := GetFleetCostRowDistanceIssueText(row)
    if (distanceIssueText != "") {
        return distanceIssueText
    }
    if (row.total <= 0 && !IsVehicleInactive(row.vehicle)) {
        return "Bez nákladu v období"
    }
    if (row.skippedCount > 0) {
        return "Nečíselné částky: " row.skippedCount
    }
    if (row.undatedCount > 0) {
        return "Bez data: " row.undatedCount
    }

    return ""
}

GetFleetCostRowDistanceIssueText(row) {
    if (row.total <= 0) {
        return ""
    }

    if !IsObject(row.distanceSummary) {
        return "Cena / km nedostupná"
    }
    if (row.distanceSummary.available && row.distanceSummary.distanceKm > 0) {
        return ""
    }
    if row.distanceSummary.hasRegression {
        return "Cena / km: nekonzistentní tachometr"
    }
    if (row.distanceSummary.available && row.distanceSummary.distanceKm <= 0) {
        return "Cena / km: nulový nájezd"
    }
    if (row.distanceSummary.sampleCount < 2) {
        return "Cena / km: chybí km v období"
    }

    return "Cena / km nedostupná"
}

GetFleetCostRowIssueScore(row) {
    score := row.skippedCount + row.undatedCount
    if (GetVehicleStatusText(row.vehicle) != "") {
        score += 3
    }
    if VehicleHasMissingGreenCard(row.vehicle) {
        score += 2
    }
    if (GetFleetCostRowDistanceIssueText(row) != "") {
        score += 2
    }
    if (row.total <= 0 && !IsVehicleInactive(row.vehicle)) {
        score += 1
    }

    return score
}

SortFleetCostRows(&rows) {
    count := rows.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := rows[i]
        j := i - 1

        while (j >= 1 && CompareFleetCostRows(current, rows[j]) < 0) {
            rows[j + 1] := rows[j]
            j -= 1
        }
        rows[j + 1] := current
    }
}

CompareFleetCostRows(left, right) {
    if (left.total > right.total) {
        return -1
    }
    if (left.total < right.total) {
        return 1
    }

    result := CompareNumberValues(GetFleetCostRowIssueScore(right), GetFleetCostRowIssueScore(left))
    if (result != 0) {
        return result
    }

    leftInactive := IsVehicleInactive(left.vehicle) ? 1 : 0
    rightInactive := IsVehicleInactive(right.vehicle) ? 1 : 0
    result := CompareNumberValues(leftInactive, rightInactive)
    if (result != 0) {
        return result
    }

    return CompareVehicles(left.vehicle, right.vehicle)
}

BuildVehicleCostSummary(vehicleId) {
    global VehicleFuelLog, VehicleHistory, VehicleRecords

    yearsMap := Map()
    summary := {
        totalFuel: 0.0,
        totalHistory: 0.0,
        totalRecords: 0.0,
        parsedCount: 0,
        skippedCount: 0,
        years: []
    }

    for entry in VehicleFuelLog {
        if (entry.vehicleId != vehicleId || Trim(entry.totalCost) = "") {
            continue
        }

        if TryParseMoneyAmount(entry.totalCost, &amount) {
            yearLabel := GetYearFromEventDate(entry.entryDate)
            AddVehicleCostToYear(&yearsMap, yearLabel, "fuel", amount)
            summary.totalFuel += amount
            summary.parsedCount += 1
        } else {
            summary.skippedCount += 1
        }
    }

    for entry in VehicleHistory {
        if (entry.vehicleId != vehicleId || Trim(entry.cost) = "") {
            continue
        }

        if TryParseMoneyAmount(entry.cost, &amount) {
            yearLabel := GetYearFromEventDate(entry.eventDate)
            AddVehicleCostToYear(&yearsMap, yearLabel, "history", amount)
            summary.totalHistory += amount
            summary.parsedCount += 1
        } else {
            summary.skippedCount += 1
        }
    }

    for entry in VehicleRecords {
        if (entry.vehicleId != vehicleId || Trim(entry.price) = "") {
            continue
        }

        if TryParseMoneyAmount(entry.price, &amount) {
            yearLabel := GetYearFromMonthYear(entry.validTo)
            if (yearLabel = "") {
                yearLabel := GetYearFromMonthYear(entry.validFrom)
            }
            AddVehicleCostToYear(&yearsMap, yearLabel, "records", amount)
            summary.totalRecords += amount
            summary.parsedCount += 1
        } else {
            summary.skippedCount += 1
        }
    }

    for _, item in yearsMap {
        item.total := item.fuel + item.history + item.records
        if RegExMatch(item.year, "^\d{4}$") {
            yearValue := item.year + 0
            item.distanceSummary := GetVehicleDistanceSummaryForMonthRange(vehicleId, GetMonthIndex(yearValue, 1), GetMonthIndex(yearValue, 12))
            item.distanceText := BuildDistanceText(item.distanceSummary)
            if (item.distanceSummary.available && item.distanceSummary.distanceKm > 0) {
                item.costPerKm := item.total / item.distanceSummary.distanceKm
                item.costPerKmText := FormatCostPerKmValue(item.costPerKm)
            } else {
                item.costPerKm := ""
                item.costPerKmText := BuildCostPerKmText(item.total, item.distanceSummary)
            }
        } else {
            item.distanceSummary := ""
            item.distanceText := "Nedostupné"
            item.costPerKm := ""
            item.costPerKmText := "Nedostupné"
        }
        summary.years.Push(item)
    }

    years := summary.years
    SortVehicleCostSummaryYears(&years)
    return summary
}

BuildVehicleCostPeriodSummary(vehicleId, yearLabel, fromMonth, toMonth) {
    yearValue := yearLabel + 0
    startMonthIndex := GetMonthIndex(yearValue, fromMonth)
    endMonthIndex := GetMonthIndex(yearValue, toMonth)
    return BuildVehicleCostPeriodSummaryForRange(vehicleId, yearLabel, fromMonth, toMonth, startMonthIndex, endMonthIndex)
}

BuildVehicleCostPeriodSummaryForRange(vehicleId, yearLabel, fromMonth, toMonth, startMonthIndex, endMonthIndex, includeComparison := true) {
    global VehicleFuelLog, VehicleHistory, VehicleRecords

    summary := {
        year: yearLabel,
        fromMonth: fromMonth,
        toMonth: toMonth,
        startMonthIndex: startMonthIndex,
        endMonthIndex: endMonthIndex,
        periodLabel: BuildMonthIndexRangeLabel(startMonthIndex, endMonthIndex),
        totalFuel: 0.0,
        totalHistory: 0.0,
        totalRecords: 0.0,
        total: 0.0,
        fuelCount: 0,
        historyCount: 0,
        recordCount: 0,
        parsedCount: 0,
        skippedCount: 0,
        undatedCount: 0,
        distanceSummary: "",
        distanceText: "Nedostupné",
        costPerKm: "",
        costPerKmText: "Nedostupné",
        comparison: ""
    }

    for entry in VehicleFuelLog {
        if (entry.vehicleId != vehicleId || Trim(entry.totalCost) = "") {
            continue
        }

        monthIndex := GetEventDateMonthIndex(entry.entryDate)
        if (monthIndex = "") {
            summary.undatedCount += 1
            continue
        }
        if (monthIndex < startMonthIndex || monthIndex > endMonthIndex) {
            continue
        }

        if TryParseMoneyAmount(entry.totalCost, &amount) {
            summary.totalFuel += amount
            summary.fuelCount += 1
            summary.parsedCount += 1
        } else {
            summary.skippedCount += 1
        }
    }

    for entry in VehicleHistory {
        if (entry.vehicleId != vehicleId || Trim(entry.cost) = "") {
            continue
        }

        monthIndex := GetEventDateMonthIndex(entry.eventDate)
        if (monthIndex = "") {
            summary.undatedCount += 1
            continue
        }
        if (monthIndex < startMonthIndex || monthIndex > endMonthIndex) {
            continue
        }

        if TryParseMoneyAmount(entry.cost, &amount) {
            summary.totalHistory += amount
            summary.historyCount += 1
            summary.parsedCount += 1
        } else {
            summary.skippedCount += 1
        }
    }

    for entry in VehicleRecords {
        if (entry.vehicleId != vehicleId || Trim(entry.price) = "") {
            continue
        }

        monthIndex := GetRecordMonthIndex(entry)
        if (monthIndex = "") {
            summary.undatedCount += 1
            continue
        }
        if (monthIndex < startMonthIndex || monthIndex > endMonthIndex) {
            continue
        }

        if TryParseMoneyAmount(entry.price, &amount) {
            summary.totalRecords += amount
            summary.recordCount += 1
            summary.parsedCount += 1
        } else {
            summary.skippedCount += 1
        }
    }

    summary.total := summary.totalFuel + summary.totalHistory + summary.totalRecords
    summary.distanceSummary := GetVehicleDistanceSummaryForMonthRange(vehicleId, startMonthIndex, endMonthIndex)
    summary.distanceText := BuildDistanceText(summary.distanceSummary)
    if (summary.distanceSummary.available && summary.distanceSummary.distanceKm > 0) {
        summary.costPerKm := summary.total / summary.distanceSummary.distanceKm
        summary.costPerKmText := FormatCostPerKmValue(summary.costPerKm)
    } else {
        summary.costPerKmText := BuildCostPerKmText(summary.total, summary.distanceSummary)
    }
    if includeComparison {
        comparisonContext := BuildVehicleCostComparisonContext(yearLabel, fromMonth, toMonth)
        previousSummary := BuildVehicleCostPeriodSummaryForRange(
            vehicleId,
            comparisonContext.previousYear,
            comparisonContext.previousFromMonth,
            comparisonContext.previousToMonth,
            comparisonContext.previousStartIndex,
            comparisonContext.previousEndIndex,
            false
        )
        summary.comparison := BuildCostPeriodComparisonSummary(summary, previousSummary)
    }

    return summary
}

BuildVehicleCostPeriodSummaryText(summary) {
    total := summary.total
    text := "Období " summary.periodLabel ". "
    text .= "Celkem nákladů: " FormatCostAmount(total) ". "
    text .= "Započteno položek: " summary.parsedCount "."
    text .= " Ujeto km: " summary.distanceText "."
    text .= " Cena / km: " summary.costPerKmText "."
    if (summary.skippedCount > 0) {
        text .= " Přeskočeno nečíselných částek: " summary.skippedCount "."
    }
    if (summary.undatedCount > 0) {
        text .= " Položek bez použitelného data: " summary.undatedCount "."
    }
    comparisonText := BuildCostPeriodComparisonText(summary.comparison)
    if (comparisonText != "") {
        text .= " " comparisonText
    }
    return text
}

BuildVehicleCostPeriodLabel(yearLabel, fromMonth, toMonth) {
    if (fromMonth = toMonth) {
        return Format("{:02}/{}", fromMonth, yearLabel)
    }

    return Format("{:02}/{} až {:02}/{}", fromMonth, yearLabel, toMonth, yearLabel)
}

BuildMonthIndexRangeLabel(startMonthIndex, endMonthIndex) {
    GetMonthYearFromIndex(startMonthIndex, &startYear, &startMonth)
    GetMonthYearFromIndex(endMonthIndex, &endYear, &endMonth)

    if (startYear = endYear && startMonth = endMonth) {
        return Format("{:02}/{}", startMonth, startYear)
    }
    return Format("{:02}/{} až {:02}/{}", startMonth, startYear, endMonth, endYear)
}

GetRecordMonthIndex(entry) {
    if !TryGetRecordYearMonth(entry, &entryYear, &entryMonth) {
        return ""
    }

    return GetMonthIndex(entryYear, entryMonth)
}

FormatCostPerKmValue(value) {
    return FormatCostAmount(value) "/km"
}

BuildCostPeriodComparisonSummary(currentSummary, previousSummary) {
    comparison := {
        previousLabel: previousSummary.periodLabel,
        totalDifference: currentSummary.total - previousSummary.total,
        totalPercentAvailable: previousSummary.total != 0,
        totalPercent: previousSummary.total != 0 ? ((currentSummary.total - previousSummary.total) / previousSummary.total) * 100.0 : "",
        costPerKmAvailable: currentSummary.costPerKm != "" && previousSummary.costPerKm != "",
        costPerKmDifference: "",
        costPerKmPercentAvailable: false,
        costPerKmPercent: ""
    }

    if comparison.costPerKmAvailable {
        comparison.costPerKmDifference := currentSummary.costPerKm - previousSummary.costPerKm
        if (previousSummary.costPerKm != 0) {
            comparison.costPerKmPercentAvailable := true
            comparison.costPerKmPercent := (comparison.costPerKmDifference / previousSummary.costPerKm) * 100.0
        }
    }

    return comparison
}

BuildCostPeriodComparisonText(comparison) {
    if !IsObject(comparison) || comparison.previousLabel = "" {
        return ""
    }

    text := "Oproti " comparison.previousLabel ": celkem " FormatSignedCostAmount(comparison.totalDifference)
    if comparison.totalPercentAvailable {
        text .= " (" FormatSignedPercentValue(comparison.totalPercent) ")"
    }
    text .= "."

    if comparison.costPerKmAvailable {
        text .= " Cena / km " FormatSignedCostPerKmValue(comparison.costPerKmDifference)
        if comparison.costPerKmPercentAvailable {
            text .= " (" FormatSignedPercentValue(comparison.costPerKmPercent) ")"
        }
        text .= "."
    } else {
        text .= " Cena / km: nedostupné."
    }

    return text
}

FormatSignedCostAmount(value) {
    if (value > 0) {
        return "+" FormatCostAmount(value)
    }
    if (value < 0) {
        return "-" FormatCostAmount(Abs(value))
    }

    return FormatCostAmount(0)
}

FormatSignedCostPerKmValue(value) {
    if (value > 0) {
        return "+" FormatCostPerKmValue(value)
    }
    if (value < 0) {
        return "-" FormatCostPerKmValue(Abs(value))
    }

    return FormatCostPerKmValue(0)
}

FormatSignedPercentValue(value) {
    sign := value > 0 ? "+" : (value < 0 ? "-" : "")
    return sign StrReplace(Format("{:.1f}", Abs(value) + 0.0), ".", ",") " %"
}

GetVehicleCostSummaryYearOptions(vehicleId) {
    global VehicleFuelLog, VehicleHistory, VehicleRecords

    seen := Map()
    items := []

    AddCostSummaryYearOption(&items, &seen, FormatTime(A_Now, "yyyy"))

    for entry in VehicleFuelLog {
        if (entry.vehicleId = vehicleId) {
            AddCostSummaryYearOption(&items, &seen, GetYearFromEventDate(entry.entryDate))
        }
    }

    for entry in VehicleHistory {
        if (entry.vehicleId = vehicleId) {
            AddCostSummaryYearOption(&items, &seen, GetYearFromEventDate(entry.eventDate))
        }
    }

    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId) {
            yearLabel := GetYearFromMonthYear(entry.validTo)
            if (yearLabel = "") {
                yearLabel := GetYearFromMonthYear(entry.validFrom)
            }
            AddCostSummaryYearOption(&items, &seen, yearLabel)
        }
    }

    SortYearLabelsDescending(&items)
    if (items.Length = 0) {
        items.Push(FormatTime(A_Now, "yyyy"))
    }

    return items
}

AddCostSummaryYearOption(&items, &seen, yearLabel) {
    if !RegExMatch(yearLabel, "^\d{4}$") {
        return
    }

    if seen.Has(yearLabel) {
        return
    }

    seen[yearLabel] := true
    items.Push(yearLabel)
}

SortYearLabelsDescending(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        swapped := false
        Loop count - A_Index {
            left := items[A_Index] + 0
            right := items[A_Index + 1] + 0
            if (left < right) {
                temp := items[A_Index]
                items[A_Index] := items[A_Index + 1]
                items[A_Index + 1] := temp
                swapped := true
            }
        }
        if !swapped {
            break
        }
    }
}

BuildVehicleCostSummaryText(summary) {
    total := summary.totalFuel + summary.totalHistory + summary.totalRecords
    text := "Celkem nákladů: " FormatCostAmount(total) "."
    text .= " Tankování: " FormatCostAmount(summary.totalFuel) "."
    text .= " Historie a servis: " FormatCostAmount(summary.totalHistory) "."
    text .= " Doklady a pojištění: " FormatCostAmount(summary.totalRecords) "."
    text .= " Započteno položek: " summary.parsedCount "."
    if (summary.skippedCount > 0) {
        text .= " Přeskočeno nečíselných částek: " summary.skippedCount "."
    }
    return text
}

AddVehicleCostToYear(&yearsMap, yearLabel, bucket, amount) {
    if (yearLabel = "") {
        yearLabel := "Neurčeno"
    }

    if !yearsMap.Has(yearLabel) {
        sortKey := RegExMatch(yearLabel, "^\d{4}$") ? yearLabel + 0 : 0
        yearsMap[yearLabel] := {
            year: yearLabel,
            sortKey: sortKey,
            fuel: 0.0,
            history: 0.0,
            records: 0.0,
            total: 0.0
        }
    }

    yearsMap[yearLabel].%bucket% += amount
}

SortVehicleCostSummaryYears(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        swapped := false
        Loop count - A_Index {
            left := items[A_Index]
            right := items[A_Index + 1]
            if (CompareVehicleCostYearEntries(left, right) > 0) {
                items[A_Index] := right
                items[A_Index + 1] := left
                swapped := true
            }
        }
        if !swapped {
            break
        }
    }
}

CompareVehicleCostYearEntries(left, right) {
    if (left.sortKey > right.sortKey) {
        return -1
    }
    if (left.sortKey < right.sortKey) {
        return 1
    }

    if (left.year = "Neurčeno" && right.year != "Neurčeno") {
        return 1
    }
    if (left.year != "Neurčeno" && right.year = "Neurčeno") {
        return -1
    }

    return CompareTextValues(left.year, right.year)
}

TryParseMoneyAmount(text, &value) {
    value := 0.0
    clean := StrLower(Trim(text))
    if (clean = "") {
        return false
    }

    clean := StrReplace(clean, "kč")
    clean := StrReplace(clean, "czk")
    clean := StrReplace(clean, " ", "")
    clean := StrReplace(clean, ",-", "")
    clean := StrReplace(clean, ".-", "")
    clean := RegExReplace(clean, "[^\d,.\-]", "")
    if (clean = "" || clean = "-" ) {
        return false
    }

    if (InStr(clean, ",") && InStr(clean, ".")) {
        lastComma := InStr(clean, ",", false, -1)
        lastDot := InStr(clean, ".", false, -1)
        if (lastComma > lastDot) {
            clean := StrReplace(clean, ".", "")
            clean := StrReplace(clean, ",", ".")
        } else {
            clean := StrReplace(clean, ",", "")
        }
    } else if (InStr(clean, ",")) {
        clean := StrReplace(clean, ",", ".")
    }

    if !RegExMatch(clean, "^-?\d+(\.\d+)?$") {
        lastDot := InStr(clean, ".", false, -1)
        if !lastDot {
            return false
        }
        integerPart := RegExReplace(SubStr(clean, 1, lastDot - 1), "\.", "")
        decimalPart := SubStr(clean, lastDot + 1)
        clean := integerPart "." decimalPart
        if !RegExMatch(clean, "^-?\d+(\.\d+)?$") {
            return false
        }
    }

    value := clean + 0.0
    return true
}

FormatCostAmount(value) {
    text := Format("{:.2f}", value + 0.0)
    return StrReplace(text, ".", ",") " Kč"
}

GetYearFromEventDate(eventDate) {
    normalized := NormalizeEventDate(eventDate)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, ".")
    return parts[3]
}

GetYearFromMonthYear(monthYear) {
    normalized := NormalizeMonthYear(monthYear)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, "/")
    return parts[2]
}

TryGetEventYearMonth(eventDate, &year, &month) {
    year := 0
    month := 0

    normalized := NormalizeEventDate(eventDate)
    if (normalized = "") {
        return false
    }

    parts := StrSplit(normalized, ".")
    year := parts[3] + 0
    month := parts[2] + 0
    return true
}

TryGetMonthYearYearMonth(monthYear, &year, &month) {
    year := 0
    month := 0

    normalized := NormalizeMonthYear(monthYear)
    if (normalized = "") {
        return false
    }

    parts := StrSplit(normalized, "/")
    month := parts[1] + 0
    year := parts[2] + 0
    return true
}

TryGetRecordYearMonth(recordEntry, &year, &month) {
    if TryGetMonthYearYearMonth(recordEntry.validTo, &year, &month) {
        return true
    }

    return TryGetMonthYearYearMonth(recordEntry.validFrom, &year, &month)
}
