BuildMainGui() {
    global AppTitle, Categories, MainGui, TabsCtrl, VehicleListLabel, MainSearchCtrl, MainStatusFilterCtrl, MainHideInactiveCtrl, MainClearFiltersButton, VehicleList, StatusBar, MainLayout

    MainLayout := {}
    MainGui := Gui("+Resize", AppTitle)
    MainGui.Title := AppTitle
    MainGui.SetFont("s10", "Segoe UI")
    MainGui.Opt("+MinSize955x495")
    MainGui.OnEvent("Close", HideMainWindow)
    MainGui.OnEvent("Escape", HideMainWindow)
    MainGui.OnEvent("Size", OnMainGuiSize)
    MainGui.MenuBar := BuildMainMenuBar()

    TabsCtrl := MainGui.AddTab3("xm ym w930 h30", Categories)
    TabsCtrl.Value := 1
    TabsCtrl.OnEvent("Change", OnCategoryChanged)
    TabsCtrl.UseTab()

    VehicleListLabel := MainGui.AddText("xm y50 w930", "Seznam vozidel v kategorii Osobní vozidla")

    searchLabel := MainGui.AddText("xm y78 w250", "Hledat název, značku, SPZ, poznámku nebo štítek")
    MainSearchCtrl := MainGui.AddEdit("x185 y75 w255")
    MainSearchCtrl.OnEvent("Change", OnMainSearchChanged)

    filterLabel := MainGui.AddText("x455 y78 w85", "Filtr seznamu")
    MainStatusFilterCtrl := MainGui.AddDropDownList("x545 y75 w210 Choose1", ["Všechna vozidla", "Jen s blížícím se termínem", "Jen po termínu", "Jen bez zelené karty"])
    MainStatusFilterCtrl.OnEvent("Change", OnMainVehicleFilterChanged)

    MainClearFiltersButton := MainGui.AddButton("x770 y74 w160 h28", "Vymazat filtry")
    MainClearFiltersButton.OnEvent("Click", ClearMainVehicleFilters)

    MainHideInactiveCtrl := MainGui.AddCheckBox("xm y106 w380", "Skrýt archivovaná a odstavená vozidla")
    MainHideInactiveCtrl.Value := GetHideInactiveVehiclesEnabled()
    MainHideInactiveCtrl.OnEvent("Click", OnMainHideInactiveChanged)

    VehicleList := MainGui.AddListView("xm y134 w930 h219 Grid -Multi", ["Název", "Poznámka", "Značka / model", "SPZ", "Poslední TK", "Příští TK", "Zelená karta do", "Stav"])
    VehicleList.OnEvent("DoubleClick", EditSelectedVehicle)
    VehicleList.OnEvent("ItemSelect", OnMainVehicleSelectionChanged)

    vehicleGroup := MainGui.AddGroupBox("xm y365 w930 h95", "Práce s vozidlem a rychlé akce")
    vehicleGroup.GetPos(&vehicleGroupX, &vehicleGroupY, &vehicleGroupW, &vehicleGroupH)

    addButton := MainGui.AddButton(Format("x{} y{} w95 h30", vehicleGroupX + 15, vehicleGroupY + 23), "Přidat")
    addButton.OnEvent("Click", AddVehicle)

    editButton := MainGui.AddButton(Format("x{} y{} w95 h30", vehicleGroupX + 120, vehicleGroupY + 23), "Upravit")
    editButton.OnEvent("Click", EditSelectedVehicle)

    detailButton := MainGui.AddButton(Format("x{} y{} w150 h30", vehicleGroupX + 225, vehicleGroupY + 23), "Detail vozidla")
    detailButton.OnEvent("Click", OpenSelectedVehicleDetail)

    historyButton := MainGui.AddButton(Format("x{} y{} w160 h30", vehicleGroupX + 385, vehicleGroupY + 23), "Historie událostí")
    historyButton.OnEvent("Click", OpenSelectedVehicleHistory)

    deleteButton := MainGui.AddButton(Format("x{} y{} w130 h30", vehicleGroupX + 555, vehicleGroupY + 23), "Odstranit")
    deleteButton.OnEvent("Click", DeleteSelectedVehicle)

    nextDueButton := MainGui.AddButton(Format("x{} y{} w150 h30", vehicleGroupX + 15, vehicleGroupY + 58), "Nejbližší TK")
    nextDueButton.OnEvent("Click", OpenNearestDueVehicle)

    checkButton := MainGui.AddButton(Format("x{} y{} w150 h30", vehicleGroupX + 175, vehicleGroupY + 58), "Zkontrolovat TK")
    checkButton.OnEvent("Click", ManualDueCheck)

    nextGreenCardButton := MainGui.AddButton(Format("x{} y{} w150 h30", vehicleGroupX + 335, vehicleGroupY + 58), "Nejbližší ZK")
    nextGreenCardButton.OnEvent("Click", OpenNearestGreenCardVehicle)

    checkGreenCardButton := MainGui.AddButton(Format("x{} y{} w160 h30", vehicleGroupX + 495, vehicleGroupY + 58), "Zkontrolovat ZK")
    checkGreenCardButton.OnEvent("Click", ManualGreenCardCheck)

    StatusBar := MainGui.AddStatusBar()
    StatusBar.SetParts(420)
    StatusBar.SetText("Připraveno", 1)
    StatusBar.SetText("Bez vozidel", 2)

    VehicleList.ModifyCol(1, "130")
    VehicleList.ModifyCol(2, "150")
    VehicleList.ModifyCol(3, "160")
    VehicleList.ModifyCol(4, "95")
    VehicleList.ModifyCol(5, "85")
    VehicleList.ModifyCol(6, "85")
    VehicleList.ModifyCol(7, "110")
    VehicleList.ModifyCol(8, "180")

    MainLayout := {
        searchLabel: searchLabel,
        filterLabel: filterLabel,
        vehicleGroup: vehicleGroup,
        addButton: addButton,
        editButton: editButton,
        detailButton: detailButton,
        historyButton: historyButton,
        deleteButton: deleteButton,
        nextDueButton: nextDueButton,
        checkButton: checkButton,
        nextGreenCardButton: nextGreenCardButton,
        checkGreenCardButton: checkGreenCardButton
    }

    shouldHideMainWindow := GetHideOnLaunchEnabled() || GetShowDashboardOnLaunchEnabled()
    showOptions := shouldHideMainWindow ? "w955 h495 Hide" : "w955 h495"
    MainGui.Show(showOptions)
    UpdateMainVehicleActionState()
}

BuildMainMenuBar() {
    vehicleMenu := Menu()
    vehicleMenu.Add("Přidat vozidlo`tCtrl+N", AddVehicle)
    vehicleMenu.Add("Upravit vybrané vozidlo`tCtrl+U", EditSelectedVehicle)
    vehicleMenu.Add("Detail vybraného vozidla`tCtrl+O", OpenSelectedVehicleDetail)
    vehicleMenu.Add("Historie vybraného vozidla`tCtrl+H", OpenSelectedVehicleHistory)
    vehicleMenu.Add("Kilometry a tankování`tCtrl+K", OpenSelectedVehicleFuelLog)
    vehicleMenu.Add("Pojištění a doklady`tCtrl+P", OpenSelectedVehicleRecords)
    vehicleMenu.Add("Vlastní připomínky`tCtrl+R", OpenSelectedVehicleReminders)
    vehicleMenu.Add("Plán údržby`tCtrl+M", OpenSelectedVehicleMaintenancePlans)
    vehicleMenu.Add("Časová osa vozidla", OpenSelectedVehicleTimeline)
    vehicleMenu.Add("Balíček pro vozidlo", OpenSelectedVehicleStarterBundle)
    vehicleMenu.Add("Náklady a souhrny", OpenSelectedVehicleCosts)
    vehicleMenu.Add()
    vehicleMenu.Add("Odstranit vybrané vozidlo", DeleteSelectedVehicle)

    fileMenu := Menu()
    fileMenu.Add("Tiskový přehled", OpenPrintableVehicleReport)
    fileMenu.Add()
    fileMenu.Add("Export dat", ExportAppData)
    fileMenu.Add("Import dat", ImportAppData)
    fileMenu.Add()
    fileMenu.Add("Konec", ExitVehimap)

    overviewMenu := Menu()
    overviewMenu.Add("Dashboard`tCtrl+D", OpenDashboard)
    overviewMenu.Add("Náklady napříč vozidly", OpenFleetCostOverviewDialog)
    overviewMenu.Add("Globální hledání`tCtrl+Shift+F", OpenGlobalSearchDialog)
    overviewMenu.Add()
    overviewMenu.Add("Přehled termínů`tCtrl+T", OpenUpcomingOverviewDialog)
    overviewMenu.Add("Propadlé termíny`tCtrl+Shift+T", OpenOverdueDialog)
    overviewMenu.Add("Audit dat", OpenAuditDialog)
    overviewMenu.Add("Export termínů do kalendáře (.ics)", ExportVehimapCalendarIcs)

    toolsMenu := Menu()
    toolsMenu.Add("Nastavení", OpenSettingsDialog)
    toolsMenu.Add("Skrýt do lišty", HideMainWindow)

    helpMenu := Menu()
    helpMenu.Add("O programu", OpenAboutDialog)
    helpMenu.Add()
    helpMenu.Add("Zkontrolovat aktualizace", CheckForUpdates)

    mainMenuBar := MenuBar()
    mainMenuBar.Add("&Soubor", fileMenu)
    mainMenuBar.Add("&Vozidlo", vehicleMenu)
    mainMenuBar.Add("Pře&hled", overviewMenu)
    mainMenuBar.Add("&Nástroje", toolsMenu)
    mainMenuBar.Add("&Nápověda", helpMenu)
    return mainMenuBar
}

OnMainSearchChanged(*) {
    RefreshVehicleList()
}

OnMainVehicleFilterChanged(*) {
    RefreshVehicleList()
}

OnMainHideInactiveChanged(ctrl, *) {
    global SettingsFile

    IniWrite(ctrl.Value ? 1 : 0, SettingsFile, "app", "hide_inactive_vehicles")
    RefreshVehicleList()
}

ClearMainVehicleFilters(*) {
    SetMainVehicleFilters("", "all")
    RefreshVehicleList()
}

SetMainVehicleFilters(searchText := "", filterKind := "all") {
    global MainSearchCtrl, MainStatusFilterCtrl

    if IsObject(MainSearchCtrl) {
        MainSearchCtrl.Text := searchText
    }

    if IsObject(MainStatusFilterCtrl) {
        MainStatusFilterCtrl.Value := GetMainVehicleFilterIndex(filterKind)
    }
}

GetMainSearchText() {
    global MainSearchCtrl

    if !IsObject(MainSearchCtrl) {
        return ""
    }

    return Trim(MainSearchCtrl.Text)
}

GetMainVehicleFilterKind() {
    global MainStatusFilterCtrl

    if !IsObject(MainStatusFilterCtrl) {
        return "all"
    }

    switch MainStatusFilterCtrl.Value {
        case 2:
            return "attention"
        case 3:
            return "overdue"
        case 4:
            return "missing_green"
        default:
            return "all"
    }
}

GetMainVehicleFilterIndex(filterKind) {
    switch filterKind {
        case "attention":
            return 2
        case "overdue":
            return 3
        case "missing_green":
            return 4
        default:
            return 1
    }
}

VehicleMatchesMainSearch(vehicle, searchText := "") {
    needle := StrLower(Trim(searchText))
    if (needle = "") {
        return true
    }

    meta := GetVehicleMeta(vehicle.id)

    haystacks := [
        StrLower(vehicle.name),
        StrLower(vehicle.vehicleNote),
        StrLower(vehicle.makeModel),
        StrLower(vehicle.plate),
        StrLower(meta.state),
        StrLower(meta.tags),
        StrLower(meta.powertrain),
        StrLower(meta.climateProfile),
        StrLower(meta.timingDrive),
        StrLower(meta.transmission)
    ]

    for haystack in haystacks {
        if (InStr(haystack, needle)) {
            return true
        }
    }

    return false
}

VehicleMatchesMainFilter(vehicle, filterKind := "all") {
    switch filterKind {
        case "attention":
            return VehicleNeedsAttention(vehicle)
        case "overdue":
            return VehicleHasOverdueTerm(vehicle)
        case "missing_green":
            return VehicleHasMissingGreenCard(vehicle)
        default:
            return true
    }
}

VehicleNeedsAttention(vehicle) {
    return GetVehicleStatusText(vehicle) != ""
}

VehicleHasOverdueTerm(vehicle) {
    nextTkStamp := ParseDueStamp(vehicle.nextTk)
    if (nextTkStamp != "" && nextTkStamp < A_Now) {
        return true
    }

    greenCardStamp := ParseDueStamp(vehicle.greenCardTo)
    if (greenCardStamp != "" && greenCardStamp < A_Now) {
        return true
    }

    reminders := GetUpcomingCustomReminders(vehicle.id)
    if (reminders.Length > 0 && reminders[1].dueStamp < A_Now) {
        return true
    }

    maintenance := GetUpcomingVehicleMaintenance(vehicle.id)
    if (maintenance.Length > 0 && IsVehicleMaintenanceSnapshotOverdue(maintenance[1].snapshot)) {
        return true
    }

    return false
}

VehicleHasMissingGreenCard(vehicle) {
    return Trim(vehicle.greenCardTo) = ""
}

IsVehicleInactive(vehicle) {
    meta := GetVehicleMeta(vehicle.id)
    state := NormalizeVehicleState(meta.state)
    return (state = "Archiv" || state = "Odstaveno")
}

OnCategoryChanged(*) {
    RefreshVehicleList()
}

ShowMainWindow(*) {
    global MainGui

    MainGui.Show()
    WinActivate("ahk_id " MainGui.Hwnd)
}

HideMainWindow(*) {
    global MainGui

    MainGui.Hide()
}

IsMainVehimapWindowActive() {
    global MainGui

    return IsObject(MainGui) && WinActive("ahk_id " MainGui.Hwnd)
}

IsGuiWindowActive(guiRef) {
    return IsObject(guiRef) && WinActive("ahk_id " guiRef.Hwnd)
}

IsListViewFocusedInGui(guiRef) {
    if !IsGuiWindowActive(guiRef) {
        return false
    }

    try return ControlGetFocus("ahk_id " guiRef.Hwnd) = "SysListView321"
    catch {
        return false
    }
}

FocusMainSearchShortcut() {
    global MainSearchCtrl

    ShowMainWindow()
    if IsObject(MainSearchCtrl) {
        MainSearchCtrl.Focus()
    }
}

FocusGlobalSearchShortcut() {
    global GlobalSearchSearchCtrl

    if IsObject(GlobalSearchSearchCtrl) {
        GlobalSearchSearchCtrl.Focus()
    }
}

FocusOverviewSearchShortcut() {
    global OverviewSearchCtrl

    if IsObject(OverviewSearchCtrl) {
        OverviewSearchCtrl.Focus()
    }
}

FocusOverdueSearchShortcut() {
    global OverdueSearchCtrl

    if IsObject(OverdueSearchCtrl) {
        OverdueSearchCtrl.Focus()
    }
}

FocusHistorySearchShortcut() {
    global HistorySearchCtrl

    if IsObject(HistorySearchCtrl) {
        HistorySearchCtrl.Focus()
    }
}

FocusFuelSearchShortcut() {
    global FuelSearchCtrl

    if IsObject(FuelSearchCtrl) {
        FuelSearchCtrl.Focus()
    }
}

FocusRecordsSearchShortcut() {
    global RecordsSearchCtrl

    if IsObject(RecordsSearchCtrl) {
        RecordsSearchCtrl.Focus()
    }
}

FocusReminderSearchShortcut() {
    global ReminderSearchCtrl

    if IsObject(ReminderSearchCtrl) {
        ReminderSearchCtrl.Focus()
    }
}

FocusMaintenanceSearchShortcut() {
    global MaintenanceSearchCtrl

    if IsObject(MaintenanceSearchCtrl) {
        MaintenanceSearchCtrl.Focus()
    }
}

FocusAuditSearchShortcut() {
    global AuditSearchCtrl

    if IsObject(AuditSearchCtrl) {
        AuditSearchCtrl.Focus()
    }

    hooks := GetVehimapTestHooks()
    if IsObject(hooks) {
        hooks.lastFocusTarget := "audit-search"
    }
}

OnMainVehicleSelectionChanged(*) {
    UpdateMainVehicleActionState()
}

UpdateMainVehicleActionState() {
    global VehicleList, VisibleVehicleIds, MainLayout

    hasSelection := false
    if IsObject(VehicleList) {
        row := VehicleList.GetNext(0)
        hasSelection := (row > 0 && row <= VisibleVehicleIds.Length)
    }

    for controlName in ["editButton", "detailButton", "historyButton", "deleteButton"] {
        if IsObject(MainLayout) && MainLayout.HasOwnProp(controlName) && IsObject(MainLayout.%controlName%) {
            MainLayout.%controlName%.Opt(hasSelection ? "-Disabled" : "+Disabled")
        }
    }
}

OnMainGuiSize(guiObj, minMax, width, height) {
    global TabsCtrl, VehicleListLabel, MainSearchCtrl, MainStatusFilterCtrl, MainHideInactiveCtrl, MainClearFiltersButton, VehicleList, MainLayout

    if (minMax = -1) {
        return
    }

    clearButtonX := width - 185
    filterX := width - 410
    filterLabelX := filterX - 90
    searchWidth := filterLabelX - 15 - 185
    groupY := height - 130
    listHeight := groupY - 146

    MoveGuiControl(TabsCtrl, 10, 10, width - 25, 30)
    MoveGuiControl(VehicleListLabel, 10, 50, width - 25)
    if IsObject(MainLayout) && MainLayout.HasOwnProp("searchLabel") {
        MoveGuiControl(MainLayout.searchLabel, 10, 78, 250)
    }
    MoveGuiControl(MainSearchCtrl, 185, 75, searchWidth)
    if IsObject(MainLayout) && MainLayout.HasOwnProp("filterLabel") {
        MoveGuiControl(MainLayout.filterLabel, filterLabelX, 78, 85)
    }
    MoveGuiControl(MainStatusFilterCtrl, filterX, 75, 210)
    MoveGuiControl(MainClearFiltersButton, clearButtonX, 74, 160, 28)
    MoveGuiControl(MainHideInactiveCtrl, 10, 106, width - 25)
    MoveGuiControl(VehicleList, 10, 134, width - 25, listHeight)

    if IsObject(MainLayout) && MainLayout.HasOwnProp("vehicleGroup") {
        MoveGuiControl(MainLayout.vehicleGroup, 10, groupY, width - 25, 95)
        MoveGuiControl(MainLayout.addButton, 25, groupY + 23, 95, 30)
        MoveGuiControl(MainLayout.editButton, 130, groupY + 23, 95, 30)
        MoveGuiControl(MainLayout.detailButton, 235, groupY + 23, 150, 30)
        MoveGuiControl(MainLayout.historyButton, 395, groupY + 23, 160, 30)
        MoveGuiControl(MainLayout.deleteButton, 565, groupY + 23, 130, 30)
        MoveGuiControl(MainLayout.nextDueButton, 25, groupY + 58, 150, 30)
        MoveGuiControl(MainLayout.checkButton, 185, groupY + 58, 150, 30)
        MoveGuiControl(MainLayout.nextGreenCardButton, 345, groupY + 58, 150, 30)
        MoveGuiControl(MainLayout.checkGreenCardButton, 505, groupY + 58, 160, 30)
    }
}
