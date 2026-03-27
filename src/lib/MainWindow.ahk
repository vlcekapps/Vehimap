BuildMainGui() {
    global AppTitle, Categories, MainGui, TabsCtrl, VehicleListLabel, MainSearchCtrl, MainStatusFilterCtrl, MainHideInactiveCtrl, MainClearFiltersButton, VehicleList, StatusBar

    MainGui := Gui("", AppTitle)
    MainGui.Title := AppTitle
    MainGui.SetFont("s10", "Segoe UI")
    MainGui.OnEvent("Close", HideMainWindow)
    MainGui.OnEvent("Escape", HideMainWindow)
    MainGui.MenuBar := BuildMainMenuBar()

    TabsCtrl := MainGui.AddTab3("xm ym w930 h30", Categories)
    TabsCtrl.Value := 1
    TabsCtrl.OnEvent("Change", OnCategoryChanged)
    TabsCtrl.UseTab()

    VehicleListLabel := MainGui.AddText("xm y50 w930", "Seznam vozidel v kategorii Osobní vozidla")

    MainGui.AddText("xm y78 w250", "Hledat název, značku, SPZ, poznámku nebo štítek")
    MainSearchCtrl := MainGui.AddEdit("x185 y75 w255")
    MainSearchCtrl.OnEvent("Change", OnMainSearchChanged)

    MainGui.AddText("x455 y78 w85", "Filtr seznamu")
    MainStatusFilterCtrl := MainGui.AddDropDownList("x545 y75 w210 Choose1", ["Všechna vozidla", "Jen s blížícím se termínem", "Jen po termínu", "Jen bez zelené karty"])
    MainStatusFilterCtrl.OnEvent("Change", OnMainVehicleFilterChanged)

    MainClearFiltersButton := MainGui.AddButton("x770 y74 w160 h28", "Vymazat filtry")
    MainClearFiltersButton.OnEvent("Click", ClearMainVehicleFilters)

    MainHideInactiveCtrl := MainGui.AddCheckBox("xm y106 w380", "Skrýt archivovaná a odstavená vozidla")
    MainHideInactiveCtrl.Value := GetHideInactiveVehiclesEnabled()
    MainHideInactiveCtrl.OnEvent("Click", OnMainHideInactiveChanged)

    VehicleList := MainGui.AddListView("xm y134 w930 h219 Grid -Multi", ["Název", "Poznámka", "Značka / model", "SPZ", "Poslední TK", "Příští TK", "Zelená karta do", "Stav"])
    VehicleList.OnEvent("DoubleClick", EditSelectedVehicle)

    vehicleGroup := MainGui.AddGroupBox("xm y365 w930 h95", "Vozidlo")
    vehicleGroup.GetPos(&vehicleGroupX, &vehicleGroupY, &vehicleGroupW, &vehicleGroupH)

    addButton := MainGui.AddButton(Format("x{} y{} w95 h30", vehicleGroupX + 15, vehicleGroupY + 23), "Přidat")
    addButton.OnEvent("Click", AddVehicle)

    editButton := MainGui.AddButton(Format("x{} y{} w95 h30", vehicleGroupX + 120, vehicleGroupY + 23), "Upravit")
    editButton.OnEvent("Click", EditSelectedVehicle)

    deleteButton := MainGui.AddButton(Format("x{} y{} w95 h30", vehicleGroupX + 225, vehicleGroupY + 23), "Odstranit")
    deleteButton.OnEvent("Click", DeleteSelectedVehicle)

    nextDueButton := MainGui.AddButton(Format("x{} y{} w150 h30", vehicleGroupX + 330, vehicleGroupY + 23), "Nejbližší TK")
    nextDueButton.OnEvent("Click", OpenNearestDueVehicle)

    checkButton := MainGui.AddButton(Format("x{} y{} w150 h30", vehicleGroupX + 490, vehicleGroupY + 23), "Zkontrolovat TK")
    checkButton.OnEvent("Click", ManualDueCheck)

    nextGreenCardButton := MainGui.AddButton(Format("x{} y{} w150 h30", vehicleGroupX + 15, vehicleGroupY + 58), "Nejbližší ZK")
    nextGreenCardButton.OnEvent("Click", OpenNearestGreenCardVehicle)

    checkGreenCardButton := MainGui.AddButton(Format("x{} y{} w160 h30", vehicleGroupX + 175, vehicleGroupY + 58), "Zkontrolovat ZK")
    checkGreenCardButton.OnEvent("Click", ManualGreenCardCheck)

    detailButton := MainGui.AddButton(Format("x{} y{} w150 h30", vehicleGroupX + 345, vehicleGroupY + 58), "Detail vozidla")
    detailButton.OnEvent("Click", OpenSelectedVehicleDetail)

    historyButton := MainGui.AddButton(Format("x{} y{} w160 h30", vehicleGroupX + 505, vehicleGroupY + 58), "Historie událostí")
    historyButton.OnEvent("Click", OpenSelectedVehicleHistory)

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

    shouldHideMainWindow := GetHideOnLaunchEnabled() || GetShowDashboardOnLaunchEnabled()
    showOptions := shouldHideMainWindow ? "w955 h495 Hide" : "w955 h495"
    MainGui.Show(showOptions)
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
