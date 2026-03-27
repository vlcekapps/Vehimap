#Requires AutoHotkey v2.0
#SingleInstance Force
Persistent
#Include GeneratedBuildInfo.ahk

global AppTitle := "Vehimap"
global DataDir := A_ScriptDir "\data"
global VehiclesFile := DataDir "\vehicles.tsv"
global HistoryFile := DataDir "\history.tsv"
global FuelLogFile := DataDir "\fuel.tsv"
global RecordsFile := DataDir "\records.tsv"
global VehicleMetaFile := DataDir "\vehicle_meta.tsv"
global RemindersFile := DataDir "\reminders.tsv"
global SettingsFile := DataDir "\settings.ini"
global Categories := ["Osobní vozidla", "Motocykly", "Nákladní vozidla", "Autobusy", "Ostatní"]
global FuelTypeOptions := ["", "Benzin", "Nafta", "LPG", "CNG", "Elektřina", "Jiné"]
global RecordTypeOptions := ["Povinné ručení", "Havarijní pojištění", "Asistence", "Doklad", "Servisní dokument", "Jiné"]
global VehicleStateOptions := ["", "Běžný provoz", "Veterán", "Odstaveno", "V renovaci", "Na prodej", "Archiv"]
global ReminderRepeatOptions := ["Neopakovat", "Každý rok", "Každé 2 roky", "Každých 5 let"]
global CostSummaryPresetOptions := ["Vlastní rozsah", "1 měsíc", "2 měsíce", "3 měsíce (čtvrtletí)", "6 měsíců (pololetí)", "9 měsíců", "12 měsíců (celý rok)"]
global MonthOptionLabels := ["01 - leden", "02 - únor", "03 - březen", "04 - duben", "05 - květen", "06 - červen", "07 - červenec", "08 - srpen", "09 - září", "10 - říjen", "11 - listopad", "12 - prosinec"]

global Vehicles := []
global VehicleHistory := []
global VehicleFuelLog := []
global VehicleRecords := []
global VehicleMetaEntries := []
global VehicleReminders := []
global VisibleVehicleIds := []
global MainGui := 0
global TabsCtrl := 0
global VehicleListLabel := 0
global MainSearchCtrl := 0
global MainStatusFilterCtrl := 0
global MainHideInactiveCtrl := 0
global MainClearFiltersButton := 0
global VehicleList := 0
global StatusBar := 0
global FormGui := 0
global FormControls := {}
global FormMode := ""
global FormVehicleId := ""
global DetailGui := 0
global DetailVehicleId := ""
global DetailRecentHistoryList := 0
global DetailHistorySummaryLabel := 0
global DetailReminderSummaryLabel := 0
global DetailFuelSummaryLabel := 0
global DetailRecordsSummaryLabel := 0
global SettingsGui := 0
global SettingsControls := {}
global OverviewGui := 0
global OverviewList := 0
global OverviewEntries := []
global OverviewAllEntries := []
global OverviewSummaryLabel := 0
global OverviewFilterCtrl := 0
global OverviewSearchCtrl := 0
global OverviewItemButton := 0
global OverviewOpenButton := 0
global OverviewEditButton := 0
global OverviewShowMissingGreenCtrl := 0
global OverviewShowDataIssuesCtrl := 0
global OverviewSortColumn := 6
global OverviewSortDescending := false
global OverdueGui := 0
global OverdueList := 0
global OverdueEntries := []
global OverdueAllEntries := []
global OverdueSummaryLabel := 0
global OverdueSearchCtrl := 0
global OverdueItemButton := 0
global OverdueOpenButton := 0
global OverdueEditButton := 0
global GlobalSearchGui := 0
global GlobalSearchList := 0
global GlobalSearchResults := []
global GlobalSearchSummaryLabel := 0
global GlobalSearchSearchCtrl := 0
global GlobalSearchOpenButton := 0
global HistoryGui := 0
global HistoryVehicleId := ""
global HistoryList := 0
global HistorySummaryLabel := 0
global HistoryAllEntries := []
global HistorySearchCtrl := 0
global VisibleHistoryEventIds := []
global HistorySortColumn := 1
global HistorySortDescending := true
global HistoryFormGui := 0
global HistoryFormControls := {}
global HistoryFormMode := ""
global HistoryFormEventId := ""
global HistoryFormVehicleId := ""
global FuelGui := 0
global FuelVehicleId := ""
global FuelList := 0
global FuelSummaryLabel := 0
global FuelAllEntries := []
global FuelSearchCtrl := 0
global VisibleFuelEntryIds := []
global FuelSortColumn := 1
global FuelSortDescending := true
global FuelFormGui := 0
global FuelFormControls := {}
global FuelFormMode := ""
global FuelFormEntryId := ""
global FuelFormVehicleId := ""
global RecordsGui := 0
global RecordsVehicleId := ""
global RecordsList := 0
global RecordsSummaryLabel := 0
global RecordsAllEntries := []
global RecordsSearchCtrl := 0
global RecordsPathStatusLabel := 0
global VisibleRecordIds := []
global RecordsSortColumn := 4
global RecordsSortDescending := false
global RecordsOpenFileButton := 0
global RecordsOpenFolderButton := 0
global RecordsCopyPathButton := 0
global RecordFormGui := 0
global RecordFormControls := {}
global RecordFormMode := ""
global RecordFormEntryId := ""
global RecordFormVehicleId := ""
global ReminderGui := 0
global ReminderVehicleId := ""
global ReminderList := 0
global ReminderSummaryLabel := 0
global ReminderAllEntries := []
global ReminderSearchCtrl := 0
global VisibleReminderIds := []
global ReminderSortColumn := 2
global ReminderSortDescending := false
global ReminderFormGui := 0
global ReminderFormControls := {}
global ReminderFormMode := ""
global ReminderFormEntryId := ""
global ReminderFormVehicleId := ""
global CostSummaryGui := 0
global CostSummaryVehicleId := ""
global CostSummarySummaryLabel := 0
global CostSummaryList := 0
global CostSummaryPeriodYearCtrl := 0
global CostSummaryPresetCtrl := 0
global CostSummaryFromMonthCtrl := 0
global CostSummaryToMonthCtrl := 0
global CostSummaryPeriodSummaryLabel := 0
global CostSummaryPeriodList := 0
global DashboardGui := 0
global DashboardSummaryVehiclesLabel := 0
global DashboardSummaryTermsLabel := 0
global DashboardSummaryCostsLabel := 0
global DashboardSummaryDataLabel := 0
global DashboardList := 0
global DashboardEntries := []
global DashboardOpenButton := 0
global DashboardItemButton := 0
global DashboardEditButton := 0
global DashboardShowOnLaunchCtrl := 0
global DashboardShowMainOnClose := false
global DueCheckIntervalMs := 900000
global AutoBackupCheckIntervalMs := 3600000
global ResumeDueCheckDelayMs := 1500
global LastTrayIconTip := ""
global UpdateManifestUrl := "https://raw.githubusercontent.com/vlcekapps/Vehimap/main/update/latest.ini"

#HotIf IsMainVehimapWindowActive()
^n::AddVehicle()
^u::EditSelectedVehicle()
F2::EditSelectedVehicle()
^f::FocusMainSearchShortcut()
^+f::OpenGlobalSearchDialog()
^d::OpenDashboard()
^t::OpenUpcomingOverviewDialog()
^+t::OpenOverdueDialog()
^o::OpenSelectedVehicleDetail()
^h::OpenSelectedVehicleHistory()
^k::OpenSelectedVehicleFuelLog()
^p::OpenSelectedVehicleRecords()
^r::OpenSelectedVehicleReminders()
#HotIf

#HotIf IsListViewFocusedInGui(MainGui)
Enter::OpenSelectedVehicleDetail()
#HotIf

#HotIf IsGuiWindowActive(GlobalSearchGui)
^f::FocusGlobalSearchShortcut()
^o::OpenSelectedGlobalSearchResult()
#HotIf

#HotIf IsListViewFocusedInGui(GlobalSearchGui)
Enter::OpenSelectedGlobalSearchResult()
#HotIf

#HotIf IsGuiWindowActive(DashboardGui)
^r::RefreshDashboardShortcut()
^f::OpenGlobalSearchFromDashboard()
^u::EditSelectedDashboardVehicle()
F2::EditSelectedDashboardVehicle()
^o::OpenSelectedDashboardVehicle()
^p::OpenSelectedDashboardItem()
^t::OpenOverviewFromDashboard()
^+t::OpenOverdueFromDashboard()
#HotIf

#HotIf IsListViewFocusedInGui(DashboardGui)
Enter::OpenSelectedDashboardItem()
#HotIf

#HotIf IsGuiWindowActive(OverviewGui)
^f::FocusOverviewSearchShortcut()
^r::RefreshUpcomingOverviewDialog()
^p::OpenSelectedOverviewItem()
^u::EditSelectedOverviewVehicle()
F2::EditSelectedOverviewVehicle()
^o::OpenSelectedOverviewVehicle()
^+t::SwitchOverviewToOverdueShortcut()
#HotIf

#HotIf IsListViewFocusedInGui(OverviewGui)
Enter::OpenSelectedOverviewItem()
#HotIf

#HotIf IsGuiWindowActive(OverdueGui)
^f::FocusOverdueSearchShortcut()
^r::RefreshOverdueDialog()
^p::OpenSelectedOverdueItem()
^u::EditSelectedOverdueVehicle()
F2::EditSelectedOverdueVehicle()
^o::OpenSelectedOverdueVehicle()
^t::SwitchOverdueToOverviewShortcut()
#HotIf

#HotIf IsListViewFocusedInGui(OverdueGui)
Enter::OpenSelectedOverdueItem()
#HotIf

#HotIf IsGuiWindowActive(DetailGui)
^u::EditVehicleFromDetail()
F2::EditVehicleFromDetail()
^h::OpenHistoryFromDetail()
^r::OpenRemindersFromDetail()
^k::OpenFuelFromDetail()
^p::OpenRecordsFromDetail()
#HotIf

#HotIf IsGuiWindowActive(HistoryGui)
^f::FocusHistorySearchShortcut()
^n::AddVehicleHistoryEvent()
^u::EditSelectedVehicleHistoryEvent()
F2::EditSelectedVehicleHistoryEvent()
^d::OpenVehicleDetailFromHistory()
#HotIf

#HotIf IsListViewFocusedInGui(HistoryGui)
Enter::EditSelectedVehicleHistoryEvent()
Delete::DeleteSelectedVehicleHistoryEvent()
#HotIf

#HotIf IsGuiWindowActive(FuelGui)
^f::FocusFuelSearchShortcut()
^n::AddVehicleFuelEntry()
^u::EditSelectedVehicleFuelEntry()
F2::EditSelectedVehicleFuelEntry()
^d::OpenVehicleDetailFromFuel()
#HotIf

#HotIf IsListViewFocusedInGui(FuelGui)
Enter::EditSelectedVehicleFuelEntry()
Delete::DeleteSelectedVehicleFuelEntry()
#HotIf

#HotIf IsGuiWindowActive(RecordsGui)
^f::FocusRecordsSearchShortcut()
^n::AddVehicleRecord()
^u::EditSelectedVehicleRecord()
F2::EditSelectedVehicleRecord()
^o::OpenSelectedVehicleRecordFile()
^+o::OpenSelectedVehicleRecordFolder()
^+c::CopySelectedVehicleRecordPath()
^d::OpenVehicleDetailFromRecords()
#HotIf

#HotIf IsListViewFocusedInGui(RecordsGui)
Enter::EditSelectedVehicleRecord()
Delete::DeleteSelectedVehicleRecord()
#HotIf

#HotIf IsGuiWindowActive(ReminderGui)
^f::FocusReminderSearchShortcut()
^n::AddVehicleReminder()
^u::EditSelectedVehicleReminder()
F2::EditSelectedVehicleReminder()
^+n::AdvanceSelectedVehicleReminder()
^d::OpenVehicleDetailFromReminder()
#HotIf

#HotIf IsListViewFocusedInGui(ReminderGui)
Enter::EditSelectedVehicleReminder()
Delete::DeleteSelectedVehicleReminder()
#HotIf

#HotIf IsGuiWindowActive(CostSummaryGui)
^r::RefreshVehicleCostPeriodSummary()
^d::OpenVehicleDetailFromCostSummary()
#HotIf

#HotIf IsGuiWindowActive(SettingsGui)
^s::SaveSettingsFromDialog()
^b::CreateImmediateBackupFromSettings()
#HotIf

#HotIf IsGuiWindowActive(FormGui)
^s::SaveVehicleFromForm()
#HotIf

#HotIf IsGuiWindowActive(HistoryFormGui)
^s::SaveVehicleHistoryEventFromForm()
#HotIf

#HotIf IsGuiWindowActive(FuelFormGui)
^s::SaveVehicleFuelEntryFromForm()
#HotIf

#HotIf IsGuiWindowActive(RecordFormGui)
^s::SaveVehicleRecordFromForm()
#HotIf

#HotIf IsGuiWindowActive(ReminderFormGui)
^s::SaveVehicleReminderFromForm()
#HotIf

if !IsVehimapTestMode() {
    InitApp()
}

IsVehimapTestMode() {
    return (EnvGet("VEHIMAP_TEST_MODE") = "1") || (IsSet(VehimapTestMode) && VehimapTestMode)
}

InitApp() {
    ConfigureAppIdentity()
    EnsureDataFiles()
    LoadVehicles()
    LoadVehicleHistory()
    LoadVehicleFuelLog()
    LoadVehicleRecords()
    LoadVehicleMeta()
    LoadVehicleReminders()
    BuildMainGui()
    RefreshVehicleList()
    SetupTrayMenu()
    RefreshTrayIdentityLater()
    StartAutomaticDueMonitoring()
    CheckDueVehicles(true, false)
    RunAutomaticBackupCheck(false, false)
    if !GetHideOnLaunchEnabled() && GetShowDashboardOnLaunchEnabled() {
        OpenStartupDashboard()
    }
}

ConfigureAppIdentity() {
    global AppTitle, LastTrayIconTip

    A_IconTip := AppTitle
    LastTrayIconTip := AppTitle
    try DllCall("shell32\SetCurrentProcessExplicitAppUserModelID", "wstr", "Vehimap")

    dhw := A_DetectHiddenWindows
    DetectHiddenWindows true
    try WinSetTitle(AppTitle, "ahk_id " A_ScriptHwnd)
    DetectHiddenWindows dhw
}

RefreshTrayIdentityTimer() {
    RefreshTrayIdentity()
}

RefreshTrayIdentityLater() {
    SetTimer(RefreshTrayIdentityTimer, -150)
}

StartAutomaticDueMonitoring() {
    global DueCheckIntervalMs, AutoBackupCheckIntervalMs

    OnMessage(0x218, OnPowerBroadcast)
    SetTimer(CheckDueVehiclesTimer, DueCheckIntervalMs)
    SetTimer(CheckAutomaticBackupsTimer, AutoBackupCheckIntervalMs)
}

RefreshTrayIdentity() {
    global LastTrayIconTip

    tip := BuildTrayIconTip()
    LastTrayIconTip := tip

    A_IconTip := tip
    if !A_IconHidden {
        A_IconHidden := true
        Sleep 100
        A_IconHidden := false
        A_IconTip := tip
    }
}

CheckDueVehiclesTimer() {
    CheckDueVehicles(true, false)
}

CheckDueVehiclesAfterResumeTimer() {
    CheckDueVehicles(true, false)
}

CheckAutomaticBackupsTimer() {
    RunAutomaticBackupCheck(false, false)
}

CheckAutomaticBackupsAfterResumeTimer() {
    RunAutomaticBackupCheck(false, false)
}

OnPowerBroadcast(wParam, lParam, msg, hwnd) {
    global ResumeDueCheckDelayMs

    if (wParam = 7 || wParam = 18) {
        SetTimer(CheckDueVehiclesAfterResumeTimer, -ResumeDueCheckDelayMs)
        SetTimer(CheckAutomaticBackupsAfterResumeTimer, -ResumeDueCheckDelayMs)
    }

    return true
}

FocusVehicleListTimer() {
    global VehicleList

    if IsObject(VehicleList) {
        try VehicleList.Focus()
    }
}

FocusVehicleListLater() {
    SetTimer(FocusVehicleListTimer, -30)
}

EnsureDataFiles() {
    global DataDir, VehiclesFile, HistoryFile, FuelLogFile, RecordsFile, VehicleMetaFile, RemindersFile, SettingsFile

    if !InStr(FileExist(DataDir), "D") {
        DirCreate(DataDir)
    }

    if !FileExist(VehiclesFile) {
        FileAppend("# Vehimap data v3`n", VehiclesFile, "UTF-8")
    }

    if !FileExist(HistoryFile) {
        FileAppend("# Vehimap history v1`n", HistoryFile, "UTF-8")
    }

    if !FileExist(FuelLogFile) {
        FileAppend("# Vehimap fuel v1`n", FuelLogFile, "UTF-8")
    }

    if !FileExist(RecordsFile) {
        FileAppend("# Vehimap records v1`n", RecordsFile, "UTF-8")
    }

    if !FileExist(VehicleMetaFile) {
        FileAppend("# Vehimap meta v1`n", VehicleMetaFile, "UTF-8")
    }

    if !FileExist(RemindersFile) {
        FileAppend("# Vehimap reminders v2`n", RemindersFile, "UTF-8")
    }

    if !FileExist(SettingsFile) {
        IniWrite("31", SettingsFile, "notifications", "technical_reminder_days")
        IniWrite("31", SettingsFile, "notifications", "green_card_reminder_days")
        IniWrite("", SettingsFile, "notifications", "last_alert_day")
        IniWrite("", SettingsFile, "notifications", "last_alert_signature")
        IniWrite("", SettingsFile, "notifications", "last_green_alert_day")
        IniWrite("", SettingsFile, "notifications", "last_green_alert_signature")
        IniWrite("", SettingsFile, "notifications", "last_reminder_alert_day")
        IniWrite("", SettingsFile, "notifications", "last_reminder_alert_signature")
        IniWrite("0", SettingsFile, "app", "run_at_startup")
        IniWrite("0", SettingsFile, "app", "hide_on_launch")
        IniWrite("0", SettingsFile, "app", "hide_inactive_vehicles")
        IniWrite("0", SettingsFile, "app", "show_dashboard_on_launch")
        IniWrite("0", SettingsFile, "backups", "automatic_backups_enabled")
        IniWrite("1", SettingsFile, "backups", "automatic_backup_interval_days")
        IniWrite("30", SettingsFile, "backups", "automatic_backup_keep_count")
        IniWrite("", SettingsFile, "backups", "last_automatic_backup_stamp")
        IniWrite("", SettingsFile, "backups", "last_automatic_backup_path")
        IniWrite("all", SettingsFile, "overview", "filter")
        IniWrite("0", SettingsFile, "overview", "include_missing_green")
        IniWrite("6", SettingsFile, "overview", "sort_column")
        IniWrite("0", SettingsFile, "overview", "sort_descending")
        IniWrite("1", SettingsFile, "history_view", "sort_column")
        IniWrite("1", SettingsFile, "history_view", "sort_descending")
        IniWrite("1", SettingsFile, "fuel_view", "sort_column")
        IniWrite("1", SettingsFile, "fuel_view", "sort_descending")
        IniWrite("4", SettingsFile, "records_view", "sort_column")
        IniWrite("0", SettingsFile, "records_view", "sort_descending")
        IniWrite("2", SettingsFile, "reminder_view", "sort_column")
        IniWrite("0", SettingsFile, "reminder_view", "sort_descending")
    }

    EnsureSettingsDefaults()
}

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

    MainGui.AddText("xm y78 w200", "Hledat název, značku, SPZ nebo štítek")
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

    VehicleList := MainGui.AddListView("xm y134 w930 h219 Grid -Multi", ["Název", "Typ", "Značka / model", "SPZ", "Poslední TK", "Příští TK", "Zelená karta do", "Stav"])
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
    VehicleList.ModifyCol(2, "100")
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
    overviewMenu.Add("Globální hledání`tCtrl+Shift+F", OpenGlobalSearchDialog)
    overviewMenu.Add()
    overviewMenu.Add("Přehled termínů`tCtrl+T", OpenUpcomingOverviewDialog)
    overviewMenu.Add("Propadlé termíny`tCtrl+Shift+T", OpenOverdueDialog)

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
        StrLower(vehicle.vehicleType),
        StrLower(vehicle.makeModel),
        StrLower(vehicle.plate),
        StrLower(meta.state),
        StrLower(meta.tags)
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

OpenDashboard(*) {
    OpenDashboardDialog(false)
}

OpenAboutDialog(*) {
    global AppTitle

    MsgBox(BuildAboutProgramText(), AppTitle " - O programu", 0x40)
}

CheckForUpdates(*) {
    global AppTitle

    currentVersion := GetAppVersion()
    try {
        manifest := LoadLatestReleaseManifest()
    } catch as err {
        MsgBox("Kontrolu aktualizací se nepodařilo dokončit.`n`n" err.Message, AppTitle, 0x30)
        return
    }

    try {
        comparison := CompareSemVer(currentVersion, manifest.version)
    } catch as err {
        MsgBox("Porovnání verzí se nepodařilo dokončit.`n`n" err.Message, AppTitle, 0x30)
        return
    }

    if (comparison < 0) {
        message := "Je dostupná novější verze Vehimap.`n`nAktuálně používáte: " currentVersion "`nNejnovější dostupná verze: " manifest.version
        if (manifest.publishedAt != "") {
            message .= "`nVydáno: " manifest.publishedAt
        }

        assetSize := GetUpdateAssetSize(manifest)
        if (assetSize > 0) {
            message .= "`nVelikost balíčku: " FormatByteSize(assetSize)
        }

        installError := ""
        canInstall := A_IsCompiled
        if canInstall {
            try {
                ValidateUpdateDownloadManifest(manifest)
            } catch as err {
                canInstall := false
                installError := err.Message
            }
        }

        if canInstall {
            result := MsgBox(
                message "`n`nAktualizaci můžeme stáhnout a nainstalovat nyní. Vehimap se ukončí a po dokončení znovu spustí.`nPřed pokračováním si prosím uložte případné rozpracované úpravy.`n`nPokračovat?",
                AppTitle,
                0x34
            )
            if (result = "Yes") {
                try {
                    StartUpdateInstallFromManifest(manifest)
                    ExitApp()
                } catch as err {
                    MsgBox("Aktualizaci se nepodařilo připravit.`n`n" err.Message, AppTitle, 0x30)
                }
            }
            return
        }

        if (!A_IsCompiled) {
            message .= "`n`nAutomatická instalace aktualizace je dostupná jen ve zkompilovaném vehimap.exe."
        } else if (installError != "") {
            message .= "`n`nAutomatickou instalaci teď nelze spustit:`n" installError
        }

        if (manifest.notesUrl != "") {
            result := MsgBox(message "`n`nOtevřít stránku vydání?", AppTitle, 0x34)
            if (result = "Yes") {
                Run('"' manifest.notesUrl '"')
            }
        } else {
            MsgBox(message, AppTitle, 0x40)
        }
        return
    }

    if (comparison > 0) {
        MsgBox(
            "Používáte novější lokální verzi (" currentVersion ") než je zatím zapsaná v manifestu (" manifest.version ").",
            AppTitle,
            0x40
        )
        return
    }

    MsgBox("Používáte aktuální verzi Vehimap (" currentVersion ").", AppTitle, 0x40)
}

GetAppVersion() {
    global AppVersion

    if IsSet(AppVersion) {
        version := Trim(AppVersion)
        if (version != "") {
            return version
        }
    }

    if A_IsCompiled {
        try {
            version := Trim(FileGetVersion(A_ScriptFullPath))
            if (version != "") {
                return version
            }
        }
    }

    return "Neznámá"
}

GetAppFileVersion() {
    global AppFileVersion

    if IsSet(AppFileVersion) {
        version := Trim(AppFileVersion)
        if (version != "") {
            return version
        }
    }

    if A_IsCompiled {
        try {
            version := Trim(FileGetVersion(A_ScriptFullPath))
            if (version != "") {
                return version
            }
        }
    }

    return ""
}

LoadLatestReleaseManifest() {
    global AppTitle, UpdateManifestUrl

    if !A_IsCompiled {
        SplitPath(A_LineFile, , &sourceDir)
        localManifestPath := sourceDir "\..\update\latest.ini"
        if FileExist(localManifestPath) {
            return ReadLatestReleaseManifestFile(localManifestPath)
        }
    }

    tempPath := A_Temp "\Vehimap_update_manifest.ini"
    requestUrl := UpdateManifestUrl "?ts=" A_NowUTC
    try {
        request := ComObject("WinHttp.WinHttpRequest.5.1")
        request.Open("GET", requestUrl, false)
        request.SetRequestHeader("User-Agent", AppTitle "/" GetAppVersion())
        request.Send()
        if (request.Status != 200) {
            throw Error("Server vrátil HTTP " request.Status ".")
        }

        WriteTextFileUtf8(tempPath, request.ResponseText)
        return ReadLatestReleaseManifestFile(tempPath)
    } catch as err {
        throw Error("Nepodařilo se načíst manifest aktualizací. " err.Message)
    } finally {
        if FileExist(tempPath) {
            FileDelete(tempPath)
        }
    }
}

ReadLatestReleaseManifestFile(path) {
    version := Trim(IniRead(path, "release", "version", ""))
    if (version = "") {
        throw Error("Manifest neobsahuje položku release/version.")
    }

    return {
        version: version,
        publishedAt: Trim(IniRead(path, "release", "published_at", "")),
        notesUrl: Trim(IniRead(path, "release", "notes_url", "")),
        assetUrl: Trim(IniRead(path, "release", "asset_url", "")),
        assetSha256: StrLower(Trim(IniRead(path, "release", "asset_sha256", ""))),
        assetSize: Trim(IniRead(path, "release", "asset_size", ""))
    }
}

ValidateUpdateDownloadManifest(manifest) {
    assetUrl := Trim(manifest.assetUrl)
    assetSha256 := StrLower(Trim(manifest.assetSha256))
    assetSize := Trim(manifest.assetSize)

    if (assetUrl = "") {
        throw Error("Manifest neobsahuje odkaz na release asset.")
    }
    if !RegExMatch(assetSha256, "^[0-9a-f]{64}$") {
        throw Error("Manifest neobsahuje platný SHA-256 hash assetu.")
    }
    if !RegExMatch(assetSize, "^\d+$") || (assetSize + 0) <= 0 {
        throw Error("Manifest neobsahuje platnou velikost assetu.")
    }
}

GetUpdateAssetSize(manifest) {
    assetSize := Trim(manifest.assetSize)
    if RegExMatch(assetSize, "^\d+$") {
        return assetSize + 0
    }
    return 0
}

FormatByteSize(sizeBytes) {
    sizeBytes += 0
    if (sizeBytes < 1024) {
        return sizeBytes " B"
    }

    sizeKb := sizeBytes / 1024.0
    if (sizeKb < 1024) {
        return StrReplace(Format("{:.1f}", sizeKb), ".", ",") " KB"
    }

    sizeMb := sizeKb / 1024.0
    if (sizeMb < 1024) {
        return StrReplace(Format("{:.1f}", sizeMb), ".", ",") " MB"
    }

    sizeGb := sizeMb / 1024.0
    return StrReplace(Format("{:.2f}", sizeGb), ".", ",") " GB"
}

StartUpdateInstallFromManifest(manifest) {
    if !A_IsCompiled {
        throw Error("Automatická instalace aktualizace je dostupná jen ve zkompilovaném vehimap.exe.")
    }
    ValidateUpdateDownloadManifest(manifest)

    helperPath := A_Temp "\Vehimap_update_helper_" FormatTime(A_Now, "yyyyMMdd_HHmmss") ".ps1"
    WriteTextFileUtf8(helperPath, BuildUpdateHelperPowerShellScript())

    currentPid := DllCall("kernel32\GetCurrentProcessId", "UInt")
    command := BuildUpdateHelperCommand(helperPath, manifest, currentPid)
    try {
        Run(command, , "Hide")
    } catch as err {
        throw Error("Nepodařilo se spustit pomocný aktualizační proces. " err.Message)
    }
}

BuildUpdateHelperCommand(helperPath, manifest, currentPid) {
    powerShellPath := GetPowerShellExePath()
    assetSize := GetUpdateAssetSize(manifest)
    return QuoteCommandArg(powerShellPath)
        . " -NoProfile -ExecutionPolicy Bypass -File " QuoteCommandArg(helperPath)
        . " -ProcessId " currentPid
        . " -AppDir " QuoteCommandArg(A_ScriptDir)
        . " -ExecutablePath " QuoteCommandArg(A_ScriptFullPath)
        . " -DownloadUrl " QuoteCommandArg(manifest.assetUrl)
        . " -ExpectedVersion " QuoteCommandArg(manifest.version)
        . " -ExpectedSha256 " QuoteCommandArg(manifest.assetSha256)
        . " -ExpectedSize " assetSize
}

GetPowerShellExePath() {
    candidates := [
        A_WinDir "\System32\WindowsPowerShell\v1.0\powershell.exe",
        A_WinDir "\Sysnative\WindowsPowerShell\v1.0\powershell.exe",
        "powershell.exe"
    ]

    for _, candidate in candidates {
        if (candidate = "powershell.exe" || FileExist(candidate)) {
            return candidate
        }
    }

    throw Error("PowerShell nebyl nalezen.")
}

QuoteCommandArg(value) {
    return '"' StrReplace(value, '"', '""') '"'
}

BuildUpdateHelperPowerShellScript() {
    lines := [
        "param(",
        "    [Parameter(Mandatory=$true)][int]$ProcessId,",
        "    [Parameter(Mandatory=$true)][string]$AppDir,",
        "    [Parameter(Mandatory=$true)][string]$ExecutablePath,",
        "    [Parameter(Mandatory=$true)][string]$DownloadUrl,",
        "    [Parameter(Mandatory=$true)][string]$ExpectedVersion,",
        "    [Parameter(Mandatory=$true)][string]$ExpectedSha256,",
        "    [Parameter(Mandatory=$true)][Int64]$ExpectedSize",
        ")",
        "",
        "$ErrorActionPreference = 'Stop'",
        "$popupTitle = 'Vehimap'",
        "",
        "function Show-Popup([string]$message, [int]$icon) {",
        "    try {",
        "        (New-Object -ComObject WScript.Shell).Popup($message, 0, $popupTitle, $icon) | Out-Null",
        "    } catch {",
        "    }",
        "}",
        "",
        "function Get-RelativeFiles([string]$root) {",
        "    $resolvedRoot = (Resolve-Path $root).Path",
        "    $items = Get-ChildItem -Path $root -Recurse -File",
        "    $result = @()",
        "    foreach ($item in $items) {",
        "        $relative = $item.FullName.Substring($resolvedRoot.Length).TrimStart('\')",
        "        $result += $relative.Replace('\', '/')",
        "    }",
        "    return $result",
        "}",
        "",
        "$tempRoot = Join-Path $env:TEMP ('VehimapUpdate_' + [Guid]::NewGuid().ToString('N'))",
        "$downloadPath = Join-Path $tempRoot 'vehimap.zip'",
        "$extractDir = Join-Path $tempRoot 'extract'",
        "$backupDir = Join-Path $tempRoot 'backup'",
        "$currentReadme = Join-Path $AppDir 'readme.txt'",
        "$backupExe = Join-Path $backupDir 'vehimap.exe'",
        "$backupReadme = Join-Path $backupDir 'readme.txt'",
        "$newExePath = Join-Path $extractDir 'vehimap.exe'",
        "$newReadmePath = Join-Path $extractDir 'readme.txt'",
        "$restoreExe = $false",
        "$restoreReadme = $false",
        "$updated = $false",
        "",
        "try {",
        "    New-Item -ItemType Directory -Path $tempRoot | Out-Null",
        "    New-Item -ItemType Directory -Path $extractDir | Out-Null",
        "    New-Item -ItemType Directory -Path $backupDir | Out-Null",
        "",
        "    Invoke-WebRequest -UseBasicParsing -Uri $DownloadUrl -OutFile $downloadPath",
        "    if (!(Test-Path $downloadPath)) {",
        "        throw 'Stažený archiv nebyl nalezen.'",
        "    }",
        "    if ((Get-Item $downloadPath).Length -ne $ExpectedSize) {",
        "        throw 'Stažený archiv má jinou velikost, než očekává manifest.'",
        "    }",
        "",
        "    $actualHash = (Get-FileHash -Path $downloadPath -Algorithm SHA256).Hash.ToLowerInvariant()",
        "    if ($actualHash -ne $ExpectedSha256.ToLowerInvariant()) {",
        "        throw 'Stažený archiv neodpovídá očekávanému SHA-256 hashi.'",
        "    }",
        "",
        "    Expand-Archive -Path $downloadPath -DestinationPath $extractDir -Force",
        "    $relativeFiles = Get-RelativeFiles $extractDir | Sort-Object",
        "    $expectedFiles = @('readme.txt', 'vehimap.exe')",
        "    if (($relativeFiles -join '|') -ne (($expectedFiles | Sort-Object) -join '|')) {",
        "        throw 'Asset neobsahuje očekávané soubory.'",
        "    }",
        "    if (!(Test-Path $newExePath)) {",
        "        throw 'V rozbaleném archivu chybí vehimap.exe.'",
        "    }",
        "    if (!(Test-Path $newReadmePath)) {",
        "        throw 'V rozbaleném archivu chybí readme.txt.'",
        "    }",
        "",
        "    if (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue) {",
        "        Wait-Process -Id $ProcessId -Timeout 120",
        "    }",
        "    if (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue) {",
        "        throw 'Vehimap se nepodařilo ukončit v požadovaném čase.'",
        "    }",
        "",
        "    if (!(Test-Path $ExecutablePath)) {",
        "        throw 'Aktuální vehimap.exe nebyl nalezen.'",
        "    }",
        "",
        "    Move-Item -Path $ExecutablePath -Destination $backupExe -Force",
        "    $restoreExe = $true",
        "    if (Test-Path $currentReadme) {",
        "        Move-Item -Path $currentReadme -Destination $backupReadme -Force",
        "        $restoreReadme = $true",
        "    }",
        "",
        "    Copy-Item -Path $newExePath -Destination $ExecutablePath -Force",
        "    Copy-Item -Path $newReadmePath -Destination $currentReadme -Force",
        "    $restoreExe = $false",
        "    $restoreReadme = $false",
        "    $updated = $true",
        "",
        "    try {",
        "        Start-Process -FilePath $ExecutablePath -WorkingDirectory $AppDir",
        "    } catch {",
        "        Show-Popup -message ('Aktualizace Vehimap na verzi ' + $ExpectedVersion + ' byla nainstalována, ale aplikaci se nepodařilo znovu spustit. Spusťte ji prosím ručně.') -icon 48",
        "    }",
        "} catch {",
        "    try {",
        "        if ($restoreExe -and (Test-Path $backupExe)) {",
        "            if (Test-Path $ExecutablePath) {",
        "                Remove-Item $ExecutablePath -Force -ErrorAction SilentlyContinue",
        "            }",
        "            Move-Item -Path $backupExe -Destination $ExecutablePath -Force",
        "        }",
        "        if ($restoreReadme -and (Test-Path $backupReadme)) {",
        "            if (Test-Path $currentReadme) {",
        "                Remove-Item $currentReadme -Force -ErrorAction SilentlyContinue",
        "            }",
        "            Move-Item -Path $backupReadme -Destination $currentReadme -Force",
        "        }",
        "    } catch {",
        "    }",
        "",
        "    if ($updated) {",
        "        Show-Popup -message ('Aktualizace Vehimap byla nainstalována, ale dokončení hlásí chybu: ' + $_.Exception.Message) -icon 48",
        "        exit 0",
        "    }",
        "",
        "    Show-Popup -message ('Aktualizace Vehimap selhala.' + [Environment]::NewLine + [Environment]::NewLine + $_.Exception.Message) -icon 16",
        "    exit 1",
        "} finally {",
        "    if (Test-Path $tempRoot) {",
        "        Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue",
        "    }",
        "}"
    ]

    return JoinLines(lines)
}

ParseSemVer(value) {
    value := Trim(value)
    if !RegExMatch(value, "^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<prerelease>[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$", &match) {
        throw Error("Neplatná semver verze: " value)
    }

    prerelease := ""
    try {
        prerelease := match["prerelease"]
    }

    return {
        major: match["major"] + 0,
        minor: match["minor"] + 0,
        patch: match["patch"] + 0,
        prerelease: prerelease
    }
}

CompareSemVer(left, right) {
    leftParts := ParseSemVer(left)
    rightParts := ParseSemVer(right)

    for _, partName in ["major", "minor", "patch"] {
        if (leftParts.%partName% < rightParts.%partName%) {
            return -1
        }
        if (leftParts.%partName% > rightParts.%partName%) {
            return 1
        }
    }

    leftPrerelease := leftParts.prerelease
    rightPrerelease := rightParts.prerelease
    if (leftPrerelease = "" && rightPrerelease = "") {
        return 0
    }
    if (leftPrerelease = "") {
        return 1
    }
    if (rightPrerelease = "") {
        return -1
    }

    leftIds := StrSplit(leftPrerelease, ".")
    rightIds := StrSplit(rightPrerelease, ".")
    maxCount := leftIds.Length
    if (rightIds.Length > maxCount) {
        maxCount := rightIds.Length
    }

    Loop maxCount {
        index := A_Index
        if (index > leftIds.Length) {
            return -1
        }
        if (index > rightIds.Length) {
            return 1
        }

        leftId := leftIds[index]
        rightId := rightIds[index]
        leftNumeric := RegExMatch(leftId, "^\d+$")
        rightNumeric := RegExMatch(rightId, "^\d+$")

        if (leftNumeric && rightNumeric) {
            leftNumber := leftId + 0
            rightNumber := rightId + 0
            if (leftNumber < rightNumber) {
                return -1
            }
            if (leftNumber > rightNumber) {
                return 1
            }
            continue
        }

        if (leftNumeric && !rightNumeric) {
            return -1
        }
        if (!leftNumeric && rightNumeric) {
            return 1
        }

        textComparison := StrCompare(leftId, rightId)
        if (textComparison < 0) {
            return -1
        }
        if (textComparison > 0) {
            return 1
        }
    }

    return 0
}

IsEquivalentAppAndFileVersion(appVersion, fileVersion) {
    appVersion := Trim(appVersion)
    fileVersion := Trim(fileVersion)

    if (appVersion = "" || fileVersion = "") {
        return false
    }
    if (appVersion = fileVersion) {
        return true
    }
    if RegExMatch(appVersion, "^\d+\.\d+\.\d+$") && RegExMatch(fileVersion, "^\Q" appVersion "\E(?:\.0)+$") {
        return true
    }

    return false
}

BuildAboutProgramText() {
    global AppTitle, DataDir

    appVersion := GetAppVersion()
    lines := [
        AppTitle,
        "Verze: " appVersion,
        "Režim spuštění: " (A_IsCompiled ? "samostatná aplikace" : "zdrojový skript"),
        "Soubor aplikace: " A_ScriptFullPath,
        "Datová složka: " DataDir
    ]

    fileVersion := GetAppFileVersion()
    if (fileVersion != "" && !IsEquivalentAppAndFileVersion(appVersion, fileVersion)) {
        lines.Push("Souborová verze (Windows): " fileVersion)
    }

    return JoinLines(lines)
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

OpenGlobalSearchDialog(*) {
    global AppTitle, MainGui, FormGui, SettingsGui, DashboardGui, OverviewGui, OverdueGui, GlobalSearchGui, GlobalSearchList, GlobalSearchResults, GlobalSearchSummaryLabel, GlobalSearchSearchCtrl, GlobalSearchOpenButton, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui

    if IsObject(GlobalSearchGui) {
        WinActivate("ahk_id " GlobalSearchGui.Hwnd)
        return
    }

    for guiRef in [DashboardGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui] {
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

    GlobalSearchGui.AddText("x20 y20 w1000", "Zde můžete hledat napříč názvy vozidel, historií událostí, kilometry a tankováním, pojištěním a doklady i vlastními připomínkami.")
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
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords, VehicleReminders

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
    }

    return 99
}

BuildGlobalSearchSummaryText(results, searchText := "") {
    needle := Trim(searchText)
    if (needle = "") {
        return "Zadejte hledaný text. Vehimap bude prohledávat vozidla, historii, tankování, doklady i připomínky."
    }

    if (results.Length = 0) {
        return "Pro hledání " needle " nebyl nalezen žádný výsledek."
    }

    vehicleCount := 0
    historyCount := 0
    fuelCount := 0
    recordCount := 0
    reminderCount := 0

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
        }
    }

    return "Nalezeno výsledků: " results.Length ". Vozidla: " vehicleCount ". Historie: " historyCount ". Tankování: " fuelCount ". Doklady: " recordCount ". Připomínky: " reminderCount "."
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

ExportAppData(*) {
    global AppTitle, A_DefaultDialogTitle

    A_DefaultDialogTitle := AppTitle
    backupPath := FileSelect("S16", GetDefaultBackupPath(), "Export dat Vehimap", "Vehimap záloha (*.vehimapbak)")
    if (backupPath = "") {
        return
    }

    backupPath := EnsureBackupExtension(backupPath)
    try {
        WriteTextFileUtf8(backupPath, BuildCurrentBackupContent())
        MsgBox("Export dat byl dokončen.`n`nSoubor:`n" backupPath, AppTitle, 0x40)
    } catch as err {
        MsgBox("Export dat se nepodařil.`n`n" err.Message, AppTitle, 0x30)
    }
}

ImportAppData(*) {
    global AppTitle, A_DefaultDialogTitle, SettingsFile, VehiclesFile, HistoryFile, FuelLogFile, RecordsFile, VehicleMetaFile, RemindersFile

    A_DefaultDialogTitle := AppTitle
    backupPath := FileSelect(1, A_ScriptDir, "Import dat Vehimap", "Vehimap záloha (*.vehimapbak)")
    if (backupPath = "") {
        return
    }

    result := MsgBox(
        "Import přepíše aktuální vozidla, historii událostí, kilometry a tankování, pojištění a doklady, stavy a štítky, vlastní připomínky i nastavení aplikace.`n`nPokračovat v importu?",
        AppTitle,
        0x34
    )
    if (result != "Yes") {
        return
    }

    try {
        backupContent := FileRead(backupPath, "UTF-8")
    } catch as err {
        MsgBox("Zvolený soubor se nepodařilo načíst.`n`n" err.Message, AppTitle, 0x30)
        return
    }

    settingsContent := ""
    vehiclesContent := ""
    historyContent := ""
    fuelContent := ""
    recordsContent := ""
    metaContent := ""
    remindersContent := ""
    errorMessage := ""
    if !TryParseBackupContent(backupContent, &settingsContent, &vehiclesContent, &historyContent, &fuelContent, &recordsContent, &metaContent, &remindersContent, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedVehicles := []
    if !TryParseVehiclesBackupContent(vehiclesContent, &importedVehicles, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedHistory := []
    if !TryParseHistoryBackupContent(historyContent, &importedHistory, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedFuelLog := []
    if !TryParseFuelBackupContent(fuelContent, &importedFuelLog, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedRecords := []
    if !TryParseRecordsBackupContent(recordsContent, &importedRecords, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedMeta := []
    if !TryParseVehicleMetaBackupContent(metaContent, &importedMeta, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    importedReminders := []
    if !TryParseVehicleRemindersBackupContent(remindersContent, &importedReminders, &errorMessage) {
        MsgBox("Import se nepodařil.`n`n" errorMessage, AppTitle, 0x30)
        return
    }

    backupDir := BackupCurrentFilesBeforeImport()

    try {
        WriteTextFileUtf8(VehiclesFile, vehiclesContent)
        WriteTextFileUtf8(HistoryFile, historyContent)
        WriteTextFileUtf8(FuelLogFile, fuelContent)
        WriteTextFileUtf8(RecordsFile, recordsContent)
        WriteTextFileUtf8(VehicleMetaFile, metaContent)
        WriteTextFileUtf8(RemindersFile, remindersContent)
        WriteTextFileUtf8(SettingsFile, settingsContent)
        EnsureSettingsDefaults()
        SetRunAtStartupEnabled(IniRead(SettingsFile, "app", "run_at_startup", "0") = "1")
        LoadVehicles()
        LoadVehicleHistory()
        LoadVehicleFuelLog()
        LoadVehicleRecords()
        LoadVehicleMeta()
        LoadVehicleReminders()
        ResetAlertHistory()
        RefreshVehicleList()
        CheckDueVehicles(false, false)
        UpdateTrayIconTip(true)
    } catch as err {
        MsgBox("Import se nepodařilo dokončit.`n`n" err.Message, AppTitle, 0x30)
        return
    }

    message := "Import dat byl dokončen."
    if (backupDir != "") {
        message .= "`n`nPůvodní soubory byly před importem zálohovány do:`n" backupDir
    }
    MsgBox(message, AppTitle, 0x40)
}

AddVehicle(*) {
    OpenVehicleForm("add")
}

OpenSettingsDialog(*) {
    global AppTitle, MainGui, FormGui, SettingsGui, SettingsControls, DashboardGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui

    if IsObject(SettingsGui) {
        WinActivate("ahk_id " SettingsGui.Hwnd)
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

    ShowMainWindow()

    SettingsControls := {}
    SettingsGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Nastavení")
    SettingsGui.SetFont("s10", "Segoe UI")
    SettingsGui.OnEvent("Close", CloseSettingsDialog)
    SettingsGui.OnEvent("Escape", CloseSettingsDialog)

    MainGui.Opt("+Disabled")

    SettingsGui.AddText("x20 y20 w570", "Zde nastavíte samostatně upozornění, chování aplikace po spuštění i pravidelné automatické zálohy.")

    SettingsGui.AddGroupBox("x20 y50 w570 h145", "Upozornění")
    SettingsGui.AddText("x35 y80 w350", "Počet dní pro upozornění na technickou kontrolu (povinné)")
    SettingsControls.technicalReminderDays := SettingsGui.AddEdit("x405 y77 w120 Limit3 Number", GetTechnicalReminderDays())
    SettingsGui.AddText("x35 y115 w350", "Počet dní pro upozornění na platnost zelené karty (povinné)")
    SettingsControls.greenCardReminderDays := SettingsGui.AddEdit("x405 y112 w120 Limit3 Number", GetGreenCardReminderDays())
    SettingsGui.AddText("x35 y150 w520", "Zadejte celé číslo od 1 do 999. Například 31 znamená upozornění přibližně měsíc před koncem.")

    SettingsGui.AddGroupBox("x20 y205 w570 h145", "Aplikace")
    SettingsControls.runAtStartup := SettingsGui.AddCheckBox("x35 y235 w300", "Spustit po startu počítače")
    SettingsControls.runAtStartup.Value := GetRunAtStartupEnabled()
    SettingsControls.hideOnLaunch := SettingsGui.AddCheckBox("x35 y265 w300", "Automaticky skrýt na lištu")
    SettingsControls.hideOnLaunch.Value := GetHideOnLaunchEnabled()
    SettingsControls.showDashboardOnLaunch := SettingsGui.AddCheckBox("x35 y295 w300", "Zobrazovat dashboard při startu")
    SettingsControls.showDashboardOnLaunch.Value := GetShowDashboardOnLaunchEnabled()
    SettingsGui.AddText("x55 y323 w500", "Pokud je zapnuté automatické skrytí do lišty, dashboard se při startu neotevře.")

    SettingsGui.AddGroupBox("x20 y360 w570 h155", "Zálohy")
    SettingsControls.automaticBackupsEnabled := SettingsGui.AddCheckBox("x35 y390 w350", "Pravidelně vytvářet automatické zálohy")
    SettingsControls.automaticBackupsEnabled.Value := GetAutomaticBackupsEnabled()
    SettingsGui.AddText("x35 y422 w350", "Interval automatické zálohy ve dnech (povinné)")
    SettingsControls.automaticBackupIntervalDays := SettingsGui.AddEdit("x405 y419 w120 Limit3 Number", GetAutomaticBackupIntervalDays())
    SettingsGui.AddText("x35 y454 w350", "Ponechat posledních automatických záloh (povinné)")
    SettingsControls.automaticBackupKeepCount := SettingsGui.AddEdit("x405 y451 w120 Limit3 Number", GetAutomaticBackupKeepCount())
    SettingsControls.backupStatusLabel := SettingsGui.AddText("x35 y483 w520 h24", BuildAutomaticBackupStatusText())

    SettingsGui.AddGroupBox("x20 y525 w570 h80", "Akce")
    backupNowButton := SettingsGui.AddButton("x170 y553 w140 h30", "Zálohovat ihned")
    backupNowButton.OnEvent("Click", CreateImmediateBackupFromSettings)

    saveButton := SettingsGui.AddButton("x320 y553 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveSettingsFromDialog)

    cancelButton := SettingsGui.AddButton("x450 y553 w100 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseSettingsDialog)

    SettingsGui.Show("w610 h625")
    SettingsControls.technicalReminderDays.Focus()
}

CloseSettingsDialog(*) {
    global SettingsGui, MainGui

    if IsObject(SettingsGui) {
        SettingsGui.Destroy()
        SettingsGui := 0
    }

    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

RefreshSettingsBackupStatusText() {
    global SettingsControls

    if IsObject(SettingsControls) && SettingsControls.Has("backupStatusLabel") && IsObject(SettingsControls.backupStatusLabel) {
        SettingsControls.backupStatusLabel.Text := BuildAutomaticBackupStatusText()
    }
}

CreateImmediateBackupFromSettings(*) {
    global AppTitle

    backupPath := CreateAutomaticBackup(true)
    if (backupPath = "") {
        return
    }

    RefreshSettingsBackupStatusText()
    MsgBox("Záloha byla vytvořena ihned.`n`nSoubor:`n" backupPath, AppTitle, 0x40)
}

OpenDashboardDialog(showMainOnClose := false) {
    global AppTitle, MainGui, FormGui, SettingsGui, DashboardGui, DashboardSummaryVehiclesLabel, DashboardSummaryTermsLabel, DashboardSummaryCostsLabel, DashboardSummaryDataLabel, DashboardList, DashboardEntries, DashboardOpenButton, DashboardItemButton, DashboardEditButton, DashboardShowOnLaunchCtrl, DashboardShowMainOnClose
    global OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui

    if IsObject(DashboardGui) {
        WinActivate("ahk_id " DashboardGui.Hwnd)
        return
    }

    for guiRef in [FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui, CostSummaryGui] {
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

    DashboardGui.AddGroupBox("x20 y50 w980 h60", "Vozidla")
    DashboardSummaryVehiclesLabel := DashboardGui.AddText("x35 y74 w950 h24", "")

    DashboardGui.AddGroupBox("x20 y115 w980 h60", "Termíny")
    DashboardSummaryTermsLabel := DashboardGui.AddText("x35 y139 w950 h24", "")

    DashboardGui.AddGroupBox("x20 y180 w980 h60", "Náklady")
    DashboardSummaryCostsLabel := DashboardGui.AddText("x35 y204 w950 h24", "")

    DashboardGui.AddGroupBox("x20 y245 w980 h60", "Evidence")
    DashboardSummaryDataLabel := DashboardGui.AddText("x35 y269 w950 h24", "")

    DashboardGui.AddGroupBox("x20 y310 w980 h225", "Nejbližší položky a datové nedostatky")
    DashboardList := DashboardGui.AddListView("x35 y335 w950 h185 Grid -Multi", ["Druh", "Vozidlo", "Kategorie", "SPZ", "Položka / termín", "Stav"])
    DashboardList.OnEvent("DoubleClick", OpenSelectedDashboardItem)
    DashboardList.ModifyCol(1, "155")
    DashboardList.ModifyCol(2, "190")
    DashboardList.ModifyCol(3, "150")
    DashboardList.ModifyCol(4, "95")
    DashboardList.ModifyCol(5, "210")
    DashboardList.ModifyCol(6, "145")

    DashboardGui.AddGroupBox("x20 y545 w980 h110", "Akce")

    overviewButton := DashboardGui.AddButton("x40 y573 w135 h30", "Přehled termínů")
    overviewButton.OnEvent("Click", OpenOverviewFromDashboard)

    overdueButton := DashboardGui.AddButton("x185 y573 w135 h30", "Propadlé termíny")
    overdueButton.OnEvent("Click", OpenOverdueFromDashboard)

    searchButton := DashboardGui.AddButton("x330 y573 w135 h30", "Globální hledání")
    searchButton.OnEvent("Click", OpenGlobalSearchFromDashboard)

    DashboardItemButton := DashboardGui.AddButton("x475 y573 w135 h30", "Otevřít položku")
    DashboardItemButton.OnEvent("Click", OpenSelectedDashboardItem)

    DashboardEditButton := DashboardGui.AddButton("x620 y573 w135 h30", "Upravit vozidlo")
    DashboardEditButton.OnEvent("Click", EditSelectedDashboardVehicle)

    DashboardOpenButton := DashboardGui.AddButton("x765 y573 w135 h30 Default", "Zobrazit vozidlo")
    DashboardOpenButton.OnEvent("Click", OpenSelectedDashboardVehicle)

    closeButton := DashboardGui.AddButton("x905 y573 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseDashboardDialog)

    DashboardShowOnLaunchCtrl := DashboardGui.AddCheckBox("x40 y615 w320", "Zobrazovat dashboard při startu")
    DashboardShowOnLaunchCtrl.Value := GetShowDashboardOnLaunchEnabled()
    DashboardShowOnLaunchCtrl.OnEvent("Click", OnDashboardShowOnLaunchChanged)

    DashboardGui.Show("w1020 h670")
    PopulateDashboardList(true)
    if (DashboardEntries.Length = 0) {
        closeButton.Focus()
    }
}

CloseDashboardDialog(*) {
    global DashboardGui, DashboardSummaryVehiclesLabel, DashboardSummaryTermsLabel, DashboardSummaryCostsLabel, DashboardSummaryDataLabel, DashboardList, DashboardEntries, DashboardOpenButton, DashboardItemButton, DashboardEditButton, DashboardShowOnLaunchCtrl, DashboardShowMainOnClose, MainGui

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
    global DashboardEntries, DashboardList, DashboardSummaryVehiclesLabel, DashboardSummaryTermsLabel, DashboardSummaryCostsLabel, DashboardSummaryDataLabel, DashboardOpenButton, DashboardItemButton, DashboardEditButton

    if !IsObject(DashboardList) {
        return
    }

    DashboardEntries := BuildDashboardEntries()
    SortDashboardEntries(&DashboardEntries)

    if IsObject(DashboardSummaryVehiclesLabel) {
        DashboardSummaryVehiclesLabel.Text := BuildDashboardVehicleSummaryText()
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

    if IsObject(DashboardEditButton) {
        DashboardEditButton.Opt(DashboardEntries.Length = 0 ? "+Disabled" : "-Disabled")
    }

    if (DashboardEntries.Length = 0) {
        return
    }

    DashboardList.Modify(1, focusList ? "Select Focus Vis" : "Select Vis")
}

BuildDashboardVehicleSummaryText() {
    global Vehicles

    archivedCount := 0
    veteranCount := 0
    parkedCount := 0
    attentionCount := 0

    for vehicle in Vehicles {
        meta := GetVehicleMeta(vehicle.id)
        state := NormalizeVehicleState(meta.state)

        if VehicleNeedsAttention(vehicle) {
            attentionCount += 1
        }

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

    return "Celkem vozidel: " Vehicles.Length ". Aktivní: " activeCount ". Archiv: " archivedCount ". Veterán: " veteranCount ". Odstaveno: " parkedCount ". Vyžaduje pozornost: " attentionCount ". Bez zelené karty: " GetMissingGreenCardCount() "."
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

    if (summary.topVehicleId != "") {
        vehicle := FindVehicleById(summary.topVehicleId)
        if IsObject(vehicle) {
            text .= " Nejvyšší zatím " vehicle.name " za " FormatCostAmount(summary.topVehicleTotal) "."
        }
    }

    return text
}

BuildDashboardCurrentYearCostSummary() {
    global VehicleFuelLog, VehicleHistory, VehicleRecords

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
        topVehicleId: "",
        topVehicleTotal: 0.0
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

    for vehicleId, total in summary.vehicleTotals {
        if (summary.topVehicleId = "" || total > summary.topVehicleTotal) {
            summary.topVehicleId := vehicleId
            summary.topVehicleTotal := total
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

    OverviewGui.AddText("x20 y20 w860", "Zde vidíte všechny blížící se a propadlé termíny technických kontrol, zelených karet a vlastních připomínek podle aktuálního nastavení upozornění. Volitelně můžete přidat i datové nedostatky k doplnění.")
    OverviewGui.AddText("x20 y55 w90", "Filtr zobrazení")
    OverviewFilterCtrl := OverviewGui.AddDropDownList("x120 y52 w220 Choose1", ["Vše", "Jen technické kontroly", "Jen zelené karty", "Jen vlastní připomínky", "Jen datové nedostatky"])
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

    OverdueGui.AddText("x20 y20 w860", "Zde vidíte všechny už propadlé technické kontroly, zelené karty a vlastní připomínky.")

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

    SortUpcomingByDue(&entries)
    return entries
}

BuildOverdueSummary(entries, allEntries) {
    vehicleIds := Map()
    overdueTechnical := 0
    overdueGreen := 0
    overdueCustom := 0

    for entry in allEntries {
        vehicleIds[entry.vehicle.id] := true
        if (entry.kind = "technical") {
            overdueTechnical += 1
        } else if (entry.kind = "green") {
            overdueGreen += 1
        } else {
            overdueCustom += 1
        }
    }

    if (allEntries.Length = 0) {
        return "Momentálně nejsou žádné propadlé technické kontroly, zelené karty ani vlastní připomínky."
    }

    text := "Propadlých položek: " allEntries.Length " u " vehicleIds.Count " vozidel. "
    text .= "TK po termínu: " overdueTechnical ". ZK po termínu: " overdueGreen ". Připomínek po termínu: " overdueCustom "."
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
            "<td>" HtmlEscape(vehicle.vehicleType) "</td>"
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
    html .= "<thead><tr><th>Název</th><th>Typ</th><th>Značka / model</th><th>SPZ</th><th>Rok výroby</th><th>Výkon</th><th>Stav vozidla</th><th>Štítky</th><th>Poslední TK</th><th>Příští TK</th><th>Zelená karta do</th><th>Stav</th></tr></thead>"
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
    if (text = "Jen datové nedostatky") {
        return "data_issue"
    }

    return "all"
}

GetOverviewFilterSetting() {
    global SettingsFile

    value := IniRead(SettingsFile, "overview", "filter", "all")
    if (value != "technical" && value != "green" && value != "custom" && value != "data_issue") {
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
    if (filterKind = "data_issue") {
        return 5
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

    if (filterKind != "technical" && filterKind != "green" && filterKind != "custom" && filterKind != "data_issue") {
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
    if (entry.HasOwnProp("entryId")) {
        return entry.kind "|" entry.vehicle.id "|" entry.entryId
    }

    return entry.kind "|" entry.vehicle.id "|" entry.term
}

IsOverviewDataIssueEntry(entry) {
    return entry.kind = "record_path" || entry.kind = "vehicle_field"
}

SaveSettingsFromDialog(*) {
    global AppTitle, SettingsControls, SettingsFile

    automaticBackupsWereEnabled := GetAutomaticBackupsEnabled()

    technicalReminderDays := ValidateReminderDaysSetting(SettingsControls.technicalReminderDays, "Počet dní pro upozornění na technickou kontrolu")
    if (technicalReminderDays = "") {
        return
    }

    greenCardReminderDays := ValidateReminderDaysSetting(SettingsControls.greenCardReminderDays, "Počet dní pro upozornění na platnost zelené karty")
    if (greenCardReminderDays = "") {
        return
    }

    automaticBackupIntervalDays := ValidatePositiveIntegerSetting(SettingsControls.automaticBackupIntervalDays, "Interval automatické zálohy ve dnech", 1, 999)
    if (automaticBackupIntervalDays = "") {
        return
    }

    automaticBackupKeepCount := ValidatePositiveIntegerSetting(SettingsControls.automaticBackupKeepCount, "Počet ponechaných automatických záloh", 1, 999)
    if (automaticBackupKeepCount = "") {
        return
    }

    runAtStartup := SettingsControls.runAtStartup.Value ? 1 : 0
    hideOnLaunch := SettingsControls.hideOnLaunch.Value ? 1 : 0
    showDashboardOnLaunch := SettingsControls.showDashboardOnLaunch.Value ? 1 : 0
    automaticBackupsEnabled := SettingsControls.automaticBackupsEnabled.Value ? 1 : 0

    if !SetRunAtStartupEnabled(runAtStartup) {
        return
    }

    IniWrite(technicalReminderDays, SettingsFile, "notifications", "technical_reminder_days")
    IniWrite(greenCardReminderDays, SettingsFile, "notifications", "green_card_reminder_days")
    IniWrite(runAtStartup, SettingsFile, "app", "run_at_startup")
    IniWrite(hideOnLaunch, SettingsFile, "app", "hide_on_launch")
    IniWrite(showDashboardOnLaunch, SettingsFile, "app", "show_dashboard_on_launch")
    IniWrite(automaticBackupsEnabled, SettingsFile, "backups", "automatic_backups_enabled")
    IniWrite(automaticBackupIntervalDays, SettingsFile, "backups", "automatic_backup_interval_days")
    IniWrite(automaticBackupKeepCount, SettingsFile, "backups", "automatic_backup_keep_count")
    ResetAlertHistory()
    if automaticBackupsEnabled {
        if !automaticBackupsWereEnabled {
            if (RunAutomaticBackupCheck(true, true) = "") {
                return
            }
        } else {
            RunAutomaticBackupCheck(false, false)
        }
        TrimAutomaticBackupFiles()
    }
    CloseSettingsDialog()
    RefreshVehicleList()
    CheckDueVehicles(false, false)
    UpdateTrayIconTip(true)
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

    result := MsgBox(message, AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

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
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, DetailVehicleId, DetailRecentHistoryList, DetailHistorySummaryLabel, DetailReminderSummaryLabel, DetailFuelSummaryLabel, DetailRecordsSummaryLabel, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui

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

    ShowMainWindow()

    DetailVehicleId := vehicle.id
    meta := GetVehicleMeta(vehicle.id)
    DetailGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Detail vozidla")
    DetailGui.SetFont("s10", "Segoe UI")
    DetailGui.OnEvent("Close", CloseVehicleDetailDialog)
    DetailGui.OnEvent("Escape", CloseVehicleDetailDialog)

    MainGui.Opt("+Disabled")

    DetailGui.AddText("x20 y20 w720", "Zde vidíte souhrn všech údajů o vybraném vozidle, poslední záznamy z historie, vlastní připomínky, orientační údaje o tankování a přehled pojištění, dokladů i nákladů.")

    DetailGui.AddGroupBox("x20 y50 w720 h180", "Základní údaje")
    DetailGui.AddText("x35 y80 w130", "Vlastní pojmenování")
    DetailGui.AddText("x170 y80 w150", FormatDisplayValue(vehicle.name))
    DetailGui.AddText("x355 y80 w110", "Kategorie")
    DetailGui.AddText("x470 y80 w180", FormatDisplayValue(vehicle.category))
    DetailGui.AddText("x35 y110 w130", "Typ")
    DetailGui.AddText("x170 y110 w150", FormatDisplayValue(vehicle.vehicleType))
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

    DetailGui.AddGroupBox("x20 y240 w720 h110", "Platnost a termíny")
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

    DetailGui.AddGroupBox("x20 y360 w720 h150", "Poslední události")
    DetailHistorySummaryLabel := DetailGui.AddText("x35 y390 w685", BuildVehicleHistorySummaryText(vehicle.id))
    DetailRecentHistoryList := DetailGui.AddListView("x35 y418 w685 h60 Grid -Multi", ["Datum", "Událost", "Km", "Cena"])
    DetailRecentHistoryList.ModifyCol(1, "85")
    DetailRecentHistoryList.ModifyCol(2, "220")
    DetailRecentHistoryList.ModifyCol(3, "110")
    DetailRecentHistoryList.ModifyCol(4, "190")
    PopulateVehicleDetailHistoryList(vehicle.id)

    DetailGui.AddGroupBox("x20 y520 w720 h95", "Další evidence")
    DetailReminderSummaryLabel := DetailGui.AddText("x35 y545 w685", BuildVehicleReminderSummaryText(vehicle.id))
    DetailFuelSummaryLabel := DetailGui.AddText("x35 y568 w685", BuildVehicleFuelSummaryText(vehicle.id))
    DetailRecordsSummaryLabel := DetailGui.AddText("x35 y591 w685", BuildVehicleRecordsSummaryText(vehicle.id))

    editButton := DetailGui.AddButton("x35 y630 w120 h30", "Upravit vozidlo")
    editButton.OnEvent("Click", EditVehicleFromDetail)

    historyButton := DetailGui.AddButton("x165 y630 w110 h30", "Historie")
    historyButton.OnEvent("Click", OpenHistoryFromDetail)

    remindersButton := DetailGui.AddButton("x285 y630 w120 h30", "Připomínky")
    remindersButton.OnEvent("Click", OpenRemindersFromDetail)

    fuelButton := DetailGui.AddButton("x415 y630 w120 h30", "Tankování")
    fuelButton.OnEvent("Click", OpenFuelFromDetail)

    recordsButton := DetailGui.AddButton("x545 y630 w160 h30", "Pojištění a doklady")
    recordsButton.OnEvent("Click", OpenRecordsFromDetail)

    costsButton := DetailGui.AddButton("x240 y665 w150 h30", "Náklady a souhrny")
    costsButton.OnEvent("Click", OpenCostsFromDetail)

    closeButton := DetailGui.AddButton("x400 y665 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleDetailDialog)

    DetailGui.Show("w760 h720")
    closeButton.Focus()
}

CloseVehicleDetailDialog(*) {
    global DetailGui, DetailVehicleId, DetailRecentHistoryList, DetailHistorySummaryLabel, DetailReminderSummaryLabel, DetailFuelSummaryLabel, DetailRecordsSummaryLabel, MainGui

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
    MainGui.Opt("-Disabled")
    ShowMainWindow()
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
    global AppTitle, Categories, VehicleStateOptions, FormGui, FormControls, FormMode, FormVehicleId, MainGui, TabsCtrl, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui

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

    FormGui.AddText("x" labelX " y" rowY " w210", "Typ (volitelné)")
    FormControls.vehicleType := FormGui.AddEdit(Format("x{} y{} w{}", inputX, rowY - 3, inputW))
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
    rowY += rowStep + 10

    FormGui.AddText("x20 y" rowY " w500", "Datum zadávejte jako MM/RRRR, například 04/2026. Pro upozornění se používají pole Příští TK a Zelená karta do.")

    saveButton := FormGui.AddButton(Format("x185 y{} w140 h30 Default", rowY + 45), "Uložit")
    saveButton.OnEvent("Click", SaveVehicleFromForm)

    cancelButton := FormGui.AddButton(Format("x335 y{} w140 h30", rowY + 45), "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleForm)

    if IsObject(vehicle) {
        FormControls.name.Text := vehicle.name
        SetDropDownToText(FormControls.category, vehicle.category, Categories)
        FormControls.vehicleType.Text := vehicle.vehicleType
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
    } else {
        FormControls.category.Value := TabsCtrl.Value
    }

    FormGui.Show("w550 h635")
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
    vehicleType := Trim(FormControls.vehicleType.Text)
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

    vehicle := {
        id: (FormMode = "edit") ? FormVehicleId : GenerateVehicleId(),
        name: name,
        category: NormalizeCategory(category),
        vehicleType: vehicleType,
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
    SaveVehicleMetaEntry(vehicle.id, vehicleState, vehicleTags)
    CloseVehicleForm()
    OpenVehicleById(vehicle.id, true)
    CheckDueVehicles(false, false)
}

OpenVehicleHistoryDialog(vehicle, openAddEvent := false, selectEventId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, HistoryAllEntries, HistorySearchCtrl, VisibleHistoryEventIds, HistorySortColumn, HistorySortDescending, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui

    if IsObject(HistoryGui) {
        WinActivate("ahk_id " HistoryGui.Hwnd)
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

    if IsObject(DetailGui) {
        WinActivate("ahk_id " DetailGui.Hwnd)
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

    HistoryVehicleId := vehicle.id
    HistoryAllEntries := []
    HistorySearchCtrl := 0
    VisibleHistoryEventIds := []
    HistorySortColumn := GetHistorySortColumnSetting()
    HistorySortDescending := GetHistorySortDescendingSetting()
    HistoryGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Historie událostí")
    HistoryGui.SetFont("s10", "Segoe UI")
    HistoryGui.OnEvent("Close", CloseVehicleHistoryDialog)
    HistoryGui.OnEvent("Escape", CloseVehicleHistoryDialog)

    MainGui.Opt("+Disabled")

    HistoryGui.AddText("x20 y20 w780", "Zde můžete vést servisní a další události k vozidlu " vehicle.name ". Datum události se zadává jako DD.MM.RRRR.")
    HistorySummaryLabel := HistoryGui.AddText("x20 y50 w780", "")
    HistoryGui.AddText("x20 y82 w290", "Hledat datum, událost, km, cenu nebo poznámku")
    HistorySearchCtrl := HistoryGui.AddEdit("x320 y79 w350")
    HistorySearchCtrl.OnEvent("Change", OnHistorySearchChanged)

    HistoryList := HistoryGui.AddListView("x20 y112 w780 h250 Grid -Multi", ["Datum", "Událost", "Km", "Cena", "Poznámka"])
    HistoryList.OnEvent("DoubleClick", EditSelectedVehicleHistoryEvent)
    HistoryList.OnEvent("ColClick", OnHistoryColumnClick)
    HistoryList.ModifyCol(1, "95")
    HistoryList.ModifyCol(2, "190")
    HistoryList.ModifyCol(3, "95")
    HistoryList.ModifyCol(4, "100")
    HistoryList.ModifyCol(5, "280")

    addButton := HistoryGui.AddButton("x95 y377 w120 h30", "Přidat událost")
    addButton.OnEvent("Click", AddVehicleHistoryEvent)

    editButton := HistoryGui.AddButton("x225 y377 w120 h30", "Upravit událost")
    editButton.OnEvent("Click", EditSelectedVehicleHistoryEvent)

    deleteButton := HistoryGui.AddButton("x355 y377 w120 h30", "Odstranit událost")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleHistoryEvent)

    detailButton := HistoryGui.AddButton("x485 y377 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromHistory)

    closeButton := HistoryGui.AddButton("x615 y377 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleHistoryDialog)

    HistoryGui.Show("w820 h427")
    PopulateVehicleHistoryList(selectEventId, true)

    if openAddEvent {
        OpenVehicleHistoryEventForm("add")
    } else if (VisibleHistoryEventIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleHistoryDialog(*) {
    global HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, HistoryAllEntries, HistorySearchCtrl, VisibleHistoryEventIds, HistorySortColumn, HistorySortDescending, MainGui

    if IsObject(HistoryGui) {
        HistoryGui.Destroy()
        HistoryGui := 0
    }

    HistoryVehicleId := ""
    HistoryList := 0
    HistorySummaryLabel := 0
    HistoryAllEntries := []
    HistorySearchCtrl := 0
    VisibleHistoryEventIds := []
    HistorySortColumn := 1
    HistorySortDescending := true
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleHistoryList(selectEventId := "", focusList := false) {
    global HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, HistoryAllEntries, VisibleHistoryEventIds

    if !IsObject(HistoryGui) || !IsObject(HistoryList) {
        return
    }

    vehicle := FindVehicleById(HistoryVehicleId)
    HistoryAllEntries := GetVehicleHistoryEntries(HistoryVehicleId)
    events := FilterVehicleHistoryEntriesBySearch(HistoryAllEntries, GetHistorySearchText())
    SortVisibleVehicleHistoryEntries(&events)
    VisibleHistoryEventIds := []
    selectedRow := 0

    if IsObject(HistorySummaryLabel) && IsObject(vehicle) {
        HistorySummaryLabel.Text := BuildVehicleHistorySummaryText(vehicle.id)
    }

    HistoryList.Opt("-Redraw")
    HistoryList.Delete()
    for event in events {
        row := HistoryList.Add("", event.eventDate, event.eventType, FormatHistoryOdometer(event.odometer), event.cost, ShortenText(event.note, 80))
        VisibleHistoryEventIds.Push(event.id)
        if (selectEventId != "" && event.id = selectEventId) {
            selectedRow := row
        }
    }
    HistoryList.Opt("+Redraw")

    if (events.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    HistoryList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
}

OnHistorySearchChanged(*) {
    selectedEventId := ""
    event := GetSelectedVehicleHistoryEvent()
    if IsObject(event) {
        selectedEventId := event.id
    }

    PopulateVehicleHistoryList(selectedEventId)
}

OnHistoryColumnClick(ctrl, column) {
    global HistorySortColumn, HistorySortDescending

    if (HistorySortColumn = column) {
        HistorySortDescending := !HistorySortDescending
    } else {
        HistorySortColumn := column
        HistorySortDescending := (column = 1)
    }

    SaveHistorySortSettings(HistorySortColumn, HistorySortDescending)

    selectedEventId := ""
    event := GetSelectedVehicleHistoryEvent()
    if IsObject(event) {
        selectedEventId := event.id
    }

    PopulateVehicleHistoryList(selectedEventId, true)
}

GetHistorySearchText() {
    global HistorySearchCtrl

    if !IsObject(HistorySearchCtrl) {
        return ""
    }

    return Trim(HistorySearchCtrl.Text)
}

FilterVehicleHistoryEntriesBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        haystack := StrLower(entry.eventDate " " entry.eventType " " entry.odometer " " entry.cost " " entry.note)
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

SortVisibleVehicleHistoryEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleHistoryEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleHistoryEntries(left, right) {
    global HistorySortColumn, HistorySortDescending

    result := CompareVisibleVehicleHistoryEntriesByColumn(left, right, HistorySortColumn)
    if (result = 0 && HistorySortColumn != 1) {
        result := CompareVisibleVehicleHistoryEntriesByColumn(left, right, 1)
    }
    if (result = 0) {
        result := CompareTextValues(left.id, right.id)
    }

    return HistorySortDescending ? -result : result
}

CompareVisibleVehicleHistoryEntriesByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareOptionalStampValues(ParseEventDateStamp(left.eventDate), ParseEventDateStamp(right.eventDate))
        case 2:
            return CompareTextValues(left.eventType, right.eventType)
        case 3:
            return CompareOptionalIntegerTexts(left.odometer, right.odometer)
        case 4:
            return CompareOptionalMoneyTexts(left.cost, right.cost)
        case 5:
            return CompareTextValues(left.note, right.note)
    }

    return 0
}

GetSelectedVehicleHistoryEvent() {
    global HistoryList, VisibleHistoryEventIds

    if !IsObject(HistoryList) {
        return ""
    }

    row := HistoryList.GetNext(0)
    if !row || row > VisibleHistoryEventIds.Length {
        return ""
    }

    return FindVehicleHistoryEventById(VisibleHistoryEventIds[row])
}

AddVehicleHistoryEvent(*) {
    OpenVehicleHistoryEventForm("add")
}

EditSelectedVehicleHistoryEvent(*) {
    global AppTitle

    event := GetSelectedVehicleHistoryEvent()
    if !IsObject(event) {
        MsgBox("Nejprve vyberte událost, kterou chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleHistoryEventForm("edit", event)
}

DeleteSelectedVehicleHistoryEvent(*) {
    global AppTitle, VehicleHistory

    event := GetSelectedVehicleHistoryEvent()
    if !IsObject(event) {
        MsgBox("Nejprve vyberte událost, kterou chcete odstranit.", AppTitle, 0x40)
        return
    }

    vehicle := FindVehicleById(event.vehicleId)
    eventLabel := event.eventType " (" event.eventDate ")"
    if IsObject(vehicle) {
        eventLabel .= " u vozidla " vehicle.name
    }

    result := MsgBox("Opravdu chcete odstranit událost " eventLabel "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleHistoryEventIndexById(event.id)
    if !index {
        return
    }

    VehicleHistory.RemoveAt(index)
    SaveVehicleHistory()
    PopulateVehicleHistoryList()
}

OpenVehicleDetailFromHistory(*) {
    global HistoryVehicleId

    vehicle := FindVehicleById(HistoryVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleHistoryDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleHistoryEventForm(mode, event := "") {
    global AppTitle, HistoryGui, HistoryVehicleId, HistoryFormGui, HistoryFormControls, HistoryFormMode, HistoryFormEventId, HistoryFormVehicleId

    if IsObject(HistoryFormGui) {
        WinActivate("ahk_id " HistoryFormGui.Hwnd)
        return
    }

    if !IsObject(HistoryGui) {
        return
    }

    HistoryFormMode := mode
    HistoryFormEventId := IsObject(event) ? event.id : ""
    HistoryFormVehicleId := IsObject(event) ? event.vehicleId : HistoryVehicleId
    HistoryFormControls := {}

    title := (mode = "edit") ? "Upravit událost" : "Přidat událost"
    HistoryFormGui := Gui("+Owner" HistoryGui.Hwnd, AppTitle " - " title)
    HistoryFormGui.SetFont("s10", "Segoe UI")
    HistoryFormGui.OnEvent("Close", CloseVehicleHistoryEventForm)
    HistoryFormGui.OnEvent("Escape", CloseVehicleHistoryEventForm)

    HistoryGui.Opt("+Disabled")

    HistoryFormGui.AddText("x20 y20 w460", "Vyplňte datum a název události. Datum zadávejte jako DD.MM.RRRR, například 26.03.2026.")

    HistoryFormGui.AddText("x20 y60 w170", "Datum události (povinné)")
    HistoryFormControls.eventDate := HistoryFormGui.AddEdit("x210 y57 w220")

    HistoryFormGui.AddText("x20 y95 w170", "Název události (povinné)")
    HistoryFormControls.eventType := HistoryFormGui.AddEdit("x210 y92 w220")

    HistoryFormGui.AddText("x20 y130 w170", "Stav tachometru (volitelné)")
    HistoryFormControls.odometer := HistoryFormGui.AddEdit("x210 y127 w220")

    HistoryFormGui.AddText("x20 y165 w170", "Cena nebo částka (volitelné)")
    HistoryFormControls.cost := HistoryFormGui.AddEdit("x210 y162 w220")

    HistoryFormGui.AddText("x20 y200 w170", "Poznámka (volitelné)")
    HistoryFormControls.note := HistoryFormGui.AddEdit("x20 y225 w410 h95 Multi")

    saveButton := HistoryFormGui.AddButton("x170 y335 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleHistoryEventFromForm)

    cancelButton := HistoryFormGui.AddButton("x300 y335 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleHistoryEventForm)

    if IsObject(event) {
        HistoryFormControls.eventDate.Text := event.eventDate
        HistoryFormControls.eventType.Text := event.eventType
        HistoryFormControls.odometer.Text := event.odometer
        HistoryFormControls.cost.Text := event.cost
        HistoryFormControls.note.Text := event.note
    }

    HistoryFormGui.Show("w450 h380")
    HistoryFormControls.eventDate.Focus()
}

CloseVehicleHistoryEventForm(*) {
    global HistoryFormGui, HistoryFormControls, HistoryFormMode, HistoryFormEventId, HistoryFormVehicleId, HistoryGui

    if IsObject(HistoryFormGui) {
        HistoryFormGui.Destroy()
        HistoryFormGui := 0
    }

    HistoryFormControls := {}
    HistoryFormMode := ""
    HistoryFormEventId := ""
    HistoryFormVehicleId := ""

    if IsObject(HistoryGui) {
        HistoryGui.Opt("-Disabled")
        WinActivate("ahk_id " HistoryGui.Hwnd)
    }
}

SaveVehicleHistoryEventFromForm(*) {
    global AppTitle, VehicleHistory, HistoryFormControls, HistoryFormMode, HistoryFormEventId, HistoryFormVehicleId

    eventDate := NormalizeEventDate(HistoryFormControls.eventDate.Text)
    eventType := Trim(HistoryFormControls.eventType.Text)
    odometer := NormalizeOdometerText(HistoryFormControls.odometer.Text)
    cost := Trim(HistoryFormControls.cost.Text)
    note := Trim(HistoryFormControls.note.Text)

    if (eventDate = "") {
        MsgBox("Pole Datum události je povinné a musí být ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        HistoryFormControls.eventDate.Focus()
        return
    }

    if (eventType = "") {
        MsgBox("Vyplňte prosím název události.", AppTitle, 0x30)
        HistoryFormControls.eventType.Focus()
        return
    }

    if (Trim(HistoryFormControls.odometer.Text) != "" && odometer = "") {
        MsgBox("Stav tachometru zadejte jen jako celé číslo, nebo pole nechte prázdné.", AppTitle, 0x30)
        HistoryFormControls.odometer.Focus()
        return
    }

    event := {
        id: (HistoryFormMode = "edit") ? HistoryFormEventId : GenerateHistoryEventId(),
        vehicleId: HistoryFormVehicleId,
        eventDate: eventDate,
        eventType: eventType,
        odometer: odometer,
        cost: cost,
        note: note
    }

    index := FindVehicleHistoryEventIndexById(event.id)
    if index {
        VehicleHistory[index] := event
    } else {
        VehicleHistory.Push(event)
    }

    SaveVehicleHistory()
    CloseVehicleHistoryEventForm()
    PopulateVehicleHistoryList(event.id, true)
}

OpenVehicleFuelDialog(vehicle, openAddEntry := false, selectEntryId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelVehicleId, FuelList, FuelSummaryLabel, FuelAllEntries, FuelSearchCtrl, VisibleFuelEntryIds, FuelSortColumn, FuelSortDescending, FuelFormGui, RecordsGui, RecordFormGui

    if IsObject(FuelGui) {
        WinActivate("ahk_id " FuelGui.Hwnd)
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

    FuelVehicleId := vehicle.id
    FuelAllEntries := []
    FuelSearchCtrl := 0
    VisibleFuelEntryIds := []
    FuelSortColumn := GetFuelSortColumnSetting()
    FuelSortDescending := GetFuelSortDescendingSetting()
    FuelGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Kilometry a tankování")
    FuelGui.SetFont("s10", "Segoe UI")
    FuelGui.OnEvent("Close", CloseVehicleFuelDialog)
    FuelGui.OnEvent("Escape", CloseVehicleFuelDialog)

    MainGui.Opt("+Disabled")

    FuelGui.AddText("x20 y20 w820", "Zde můžete evidovat stavy tachometru, tankování a orientační spotřebu vozidla " vehicle.name ". Datum zadejte jako DD.MM.RRRR.")
    FuelSummaryLabel := FuelGui.AddText("x20 y50 w820", "")
    FuelGui.AddText("x20 y82 w320", "Hledat datum, tachometr, litry, cenu, palivo nebo poznámku")
    FuelSearchCtrl := FuelGui.AddEdit("x350 y79 w350")
    FuelSearchCtrl.OnEvent("Change", OnFuelSearchChanged)

    FuelList := FuelGui.AddListView("x20 y112 w820 h255 Grid -Multi", ["Datum", "Tachometr", "Litry", "Cena", "Plná nádrž", "Palivo", "Poznámka"])
    FuelList.OnEvent("DoubleClick", EditSelectedVehicleFuelEntry)
    FuelList.OnEvent("ColClick", OnFuelColumnClick)
    FuelList.ModifyCol(1, "95")
    FuelList.ModifyCol(2, "100")
    FuelList.ModifyCol(3, "70")
    FuelList.ModifyCol(4, "95")
    FuelList.ModifyCol(5, "75")
    FuelList.ModifyCol(6, "95")
    FuelList.ModifyCol(7, "240")

    addButton := FuelGui.AddButton("x80 y382 w120 h30", "Přidat záznam")
    addButton.OnEvent("Click", AddVehicleFuelEntry)

    editButton := FuelGui.AddButton("x210 y382 w120 h30", "Upravit záznam")
    editButton.OnEvent("Click", EditSelectedVehicleFuelEntry)

    deleteButton := FuelGui.AddButton("x340 y382 w130 h30", "Odstranit záznam")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleFuelEntry)

    detailButton := FuelGui.AddButton("x480 y382 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromFuel)

    closeButton := FuelGui.AddButton("x610 y382 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleFuelDialog)

    FuelGui.Show("w860 h432")
    PopulateVehicleFuelList(selectEntryId, true)

    if openAddEntry {
        OpenVehicleFuelEntryForm("add")
    } else if (VisibleFuelEntryIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleFuelDialog(*) {
    global FuelGui, FuelVehicleId, FuelList, FuelSummaryLabel, FuelAllEntries, FuelSearchCtrl, VisibleFuelEntryIds, FuelSortColumn, FuelSortDescending, MainGui

    if IsObject(FuelGui) {
        FuelGui.Destroy()
        FuelGui := 0
    }

    FuelVehicleId := ""
    FuelList := 0
    FuelSummaryLabel := 0
    FuelAllEntries := []
    FuelSearchCtrl := 0
    VisibleFuelEntryIds := []
    FuelSortColumn := 1
    FuelSortDescending := true
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleFuelList(selectEntryId := "", focusList := false) {
    global FuelGui, FuelVehicleId, FuelList, FuelSummaryLabel, FuelAllEntries, VisibleFuelEntryIds

    if !IsObject(FuelGui) || !IsObject(FuelList) {
        return
    }

    FuelAllEntries := GetVehicleFuelEntries(FuelVehicleId)
    entries := FilterVehicleFuelEntriesBySearch(FuelAllEntries, GetFuelSearchText())
    SortVisibleVehicleFuelEntries(&entries)
    VisibleFuelEntryIds := []
    selectedRow := 0

    if IsObject(FuelSummaryLabel) {
        FuelSummaryLabel.Text := BuildVehicleFuelSummaryText(FuelVehicleId)
    }

    FuelList.Opt("-Redraw")
    FuelList.Delete()
    for entry in entries {
        row := FuelList.Add(
            "",
            entry.entryDate,
            FormatHistoryOdometer(entry.odometer),
            FormatFuelLiters(entry.liters),
            FormatFuelMoney(entry.totalCost),
            entry.fullTank ? "Ano" : "Ne",
            entry.fuelType,
            ShortenText(entry.note, 80)
        )
        VisibleFuelEntryIds.Push(entry.id)
        if (selectEntryId != "" && entry.id = selectEntryId) {
            selectedRow := row
        }
    }
    FuelList.Opt("+Redraw")

    if (entries.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    FuelList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
}

OnFuelSearchChanged(*) {
    selectedEntryId := ""
    entry := GetSelectedVehicleFuelEntry()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleFuelList(selectedEntryId)
}

OnFuelColumnClick(ctrl, column) {
    global FuelSortColumn, FuelSortDescending

    if (FuelSortColumn = column) {
        FuelSortDescending := !FuelSortDescending
    } else {
        FuelSortColumn := column
        FuelSortDescending := (column = 1 || column = 2)
    }

    SaveFuelSortSettings(FuelSortColumn, FuelSortDescending)

    selectedEntryId := ""
    entry := GetSelectedVehicleFuelEntry()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleFuelList(selectedEntryId, true)
}

GetFuelSearchText() {
    global FuelSearchCtrl

    if !IsObject(FuelSearchCtrl) {
        return ""
    }

    return Trim(FuelSearchCtrl.Text)
}

FilterVehicleFuelEntriesBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        fullTankText := entry.fullTank ? "ano" : "ne"
        haystack := StrLower(entry.entryDate " " entry.odometer " " entry.liters " " entry.totalCost " " fullTankText " " entry.fuelType " " entry.note)
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

SortVisibleVehicleFuelEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleFuelEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleFuelEntries(left, right) {
    global FuelSortColumn, FuelSortDescending

    result := CompareVisibleVehicleFuelEntriesByColumn(left, right, FuelSortColumn)
    if (result = 0 && FuelSortColumn != 1) {
        result := CompareVisibleVehicleFuelEntriesByColumn(left, right, 1)
    }
    if (result = 0) {
        result := CompareTextValues(left.id, right.id)
    }

    return FuelSortDescending ? -result : result
}

CompareVisibleVehicleFuelEntriesByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareOptionalStampValues(ParseEventDateStamp(left.entryDate), ParseEventDateStamp(right.entryDate))
        case 2:
            return CompareOptionalIntegerTexts(left.odometer, right.odometer)
        case 3:
            return CompareOptionalDecimalTexts(left.liters, right.liters)
        case 4:
            return CompareOptionalMoneyTexts(left.totalCost, right.totalCost)
        case 5:
            return CompareNumberValues(left.fullTank ? 1 : 0, right.fullTank ? 1 : 0)
        case 6:
            return CompareTextValues(left.fuelType, right.fuelType)
        case 7:
            return CompareTextValues(left.note, right.note)
    }

    return 0
}

GetSelectedVehicleFuelEntry() {
    global FuelList, VisibleFuelEntryIds

    if !IsObject(FuelList) {
        return ""
    }

    row := FuelList.GetNext(0)
    if !row || row > VisibleFuelEntryIds.Length {
        return ""
    }

    return FindVehicleFuelEntryById(VisibleFuelEntryIds[row])
}

AddVehicleFuelEntry(*) {
    OpenVehicleFuelEntryForm("add")
}

EditSelectedVehicleFuelEntry(*) {
    global AppTitle

    entry := GetSelectedVehicleFuelEntry()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, který chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleFuelEntryForm("edit", entry)
}

DeleteSelectedVehicleFuelEntry(*) {
    global AppTitle, VehicleFuelLog

    entry := GetSelectedVehicleFuelEntry()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, který chcete odstranit.", AppTitle, 0x40)
        return
    }

    result := MsgBox("Opravdu chcete odstranit záznam z " entry.entryDate "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleFuelEntryIndexById(entry.id)
    if !index {
        return
    }

    VehicleFuelLog.RemoveAt(index)
    SaveVehicleFuelLog()
    PopulateVehicleFuelList()
}

OpenVehicleDetailFromFuel(*) {
    global FuelVehicleId

    vehicle := FindVehicleById(FuelVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleFuelDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleFuelEntryForm(mode, entry := "") {
    global AppTitle, FuelGui, FuelVehicleId, FuelFormGui, FuelFormControls, FuelFormMode, FuelFormEntryId, FuelFormVehicleId, FuelTypeOptions

    if IsObject(FuelFormGui) {
        WinActivate("ahk_id " FuelFormGui.Hwnd)
        return
    }

    if !IsObject(FuelGui) {
        return
    }

    FuelFormMode := mode
    FuelFormEntryId := IsObject(entry) ? entry.id : ""
    FuelFormVehicleId := IsObject(entry) ? entry.vehicleId : FuelVehicleId
    FuelFormControls := {}

    title := (mode = "edit") ? "Upravit záznam" : "Přidat záznam"
    FuelFormGui := Gui("+Owner" FuelGui.Hwnd, AppTitle " - " title)
    FuelFormGui.SetFont("s10", "Segoe UI")
    FuelFormGui.OnEvent("Close", CloseVehicleFuelEntryForm)
    FuelFormGui.OnEvent("Escape", CloseVehicleFuelEntryForm)

    FuelGui.Opt("+Disabled")

    FuelFormGui.AddText("x20 y20 w500", "Datum a stav tachometru jsou povinné. Litry a cena vyplňte, pokud jde o tankování.")

    FuelFormGui.AddText("x20 y60 w180", "Datum záznamu (povinné)")
    FuelFormControls.entryDate := FuelFormGui.AddEdit("x230 y57 w220")

    FuelFormGui.AddText("x20 y95 w180", "Stav tachometru (povinné)")
    FuelFormControls.odometer := FuelFormGui.AddEdit("x230 y92 w220")

    FuelFormGui.AddText("x20 y130 w180", "Natankováno litrů (volitelné)")
    FuelFormControls.liters := FuelFormGui.AddEdit("x230 y127 w220")

    FuelFormGui.AddText("x20 y165 w180", "Cena celkem v Kč (volitelné)")
    FuelFormControls.totalCost := FuelFormGui.AddEdit("x230 y162 w220")

    FuelFormGui.AddText("x20 y200 w180", "Typ paliva (volitelné)")
    FuelFormControls.fuelType := FuelFormGui.AddDropDownList("x230 y197 w220", FuelTypeOptions)

    FuelFormControls.fullTank := FuelFormGui.AddCheckBox("x230 y232 w220", "Plná nádrž")
    FuelFormControls.fullTank.Value := 1

    FuelFormGui.AddText("x20 y265 w180", "Poznámka (volitelné)")
    FuelFormControls.note := FuelFormGui.AddEdit("x20 y290 w430 h80 Multi")

    saveButton := FuelFormGui.AddButton("x150 y385 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleFuelEntryFromForm)

    cancelButton := FuelFormGui.AddButton("x280 y385 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleFuelEntryForm)

    if IsObject(entry) {
        FuelFormControls.entryDate.Text := entry.entryDate
        FuelFormControls.odometer.Text := entry.odometer
        FuelFormControls.liters.Text := entry.liters
        FuelFormControls.totalCost.Text := entry.totalCost
        SetDropDownToText(FuelFormControls.fuelType, entry.fuelType, FuelTypeOptions)
        FuelFormControls.fullTank.Value := entry.fullTank ? 1 : 0
        FuelFormControls.note.Text := entry.note
    }

    FuelFormGui.Show("w470 h430")
    FuelFormControls.entryDate.Focus()
}

CloseVehicleFuelEntryForm(*) {
    global FuelFormGui, FuelFormControls, FuelFormMode, FuelFormEntryId, FuelFormVehicleId, FuelGui

    if IsObject(FuelFormGui) {
        FuelFormGui.Destroy()
        FuelFormGui := 0
    }

    FuelFormControls := {}
    FuelFormMode := ""
    FuelFormEntryId := ""
    FuelFormVehicleId := ""

    if IsObject(FuelGui) {
        FuelGui.Opt("-Disabled")
        WinActivate("ahk_id " FuelGui.Hwnd)
    }
}

SaveVehicleFuelEntryFromForm(*) {
    global AppTitle, VehicleFuelLog, FuelFormControls, FuelFormMode, FuelFormEntryId, FuelFormVehicleId

    entryDate := NormalizeEventDate(FuelFormControls.entryDate.Text)
    odometer := NormalizeOdometerText(FuelFormControls.odometer.Text)
    liters := NormalizeDecimalText(FuelFormControls.liters.Text)
    totalCost := NormalizeDecimalText(FuelFormControls.totalCost.Text)
    fuelType := Trim(FuelFormControls.fuelType.Text)
    fullTank := FuelFormControls.fullTank.Value ? 1 : 0
    note := Trim(FuelFormControls.note.Text)

    if (entryDate = "") {
        MsgBox("Pole Datum záznamu je povinné a musí být ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        FuelFormControls.entryDate.Focus()
        return
    }

    if (odometer = "") {
        MsgBox("Pole Stav tachometru je povinné a musí obsahovat celé číslo.", AppTitle, 0x30)
        FuelFormControls.odometer.Focus()
        return
    }

    if (Trim(FuelFormControls.liters.Text) != "" && liters = "") {
        MsgBox("Natankované litry zadejte jako číslo, například 42,5.", AppTitle, 0x30)
        FuelFormControls.liters.Focus()
        return
    }

    if (Trim(FuelFormControls.totalCost.Text) != "" && totalCost = "") {
        MsgBox("Cenu celkem zadejte jako číslo v Kč, například 1890.", AppTitle, 0x30)
        FuelFormControls.totalCost.Focus()
        return
    }

    if (totalCost != "" && liters = "") {
        MsgBox("Pokud zadáváte cenu tankování, doplňte i počet litrů.", AppTitle, 0x30)
        FuelFormControls.liters.Focus()
        return
    }

    entry := {
        id: (FuelFormMode = "edit") ? FuelFormEntryId : GenerateFuelEntryId(),
        vehicleId: FuelFormVehicleId,
        entryDate: entryDate,
        odometer: odometer,
        liters: liters,
        totalCost: totalCost,
        fullTank: fullTank,
        fuelType: fuelType,
        note: note
    }

    index := FindVehicleFuelEntryIndexById(entry.id)
    if index {
        VehicleFuelLog[index] := entry
    } else {
        VehicleFuelLog.Push(entry)
    }

    SaveVehicleFuelLog()
    CloseVehicleFuelEntryForm()
    PopulateVehicleFuelList(entry.id, true)
}

OpenVehicleRecordsDialog(vehicle, openAddEntry := false, selectEntryId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordsVehicleId, RecordsList, RecordsSummaryLabel, RecordsAllEntries, RecordsSearchCtrl, RecordsPathStatusLabel, VisibleRecordIds, RecordsSortColumn, RecordsSortDescending, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton, RecordFormGui

    if IsObject(RecordsGui) {
        WinActivate("ahk_id " RecordsGui.Hwnd)
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

    if IsObject(RecordFormGui) {
        WinActivate("ahk_id " RecordFormGui.Hwnd)
        return
    }

    ShowMainWindow()

    RecordsVehicleId := vehicle.id
    RecordsAllEntries := []
    RecordsSearchCtrl := 0
    RecordsPathStatusLabel := 0
    VisibleRecordIds := []
    RecordsSortColumn := GetRecordsSortColumnSetting()
    RecordsSortDescending := GetRecordsSortDescendingSetting()
    RecordsOpenFileButton := 0
    RecordsOpenFolderButton := 0
    RecordsCopyPathButton := 0
    RecordsGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Pojištění a doklady")
    RecordsGui.SetFont("s10", "Segoe UI")
    RecordsGui.OnEvent("Close", CloseVehicleRecordsDialog)
    RecordsGui.OnEvent("Escape", CloseVehicleRecordsDialog)

    MainGui.Opt("+Disabled")

    RecordsGui.AddText("x20 y20 w820", "Zde můžete evidovat pojištění, doklady a další soubory k vozidlu " vehicle.name ".")
    RecordsSummaryLabel := RecordsGui.AddText("x20 y50 w820", "")
    RecordsGui.AddText("x20 y82 w280", "Hledat druh, název, poskytovatele, platnost nebo soubor")
    RecordsSearchCtrl := RecordsGui.AddEdit("x310 y79 w360")
    RecordsSearchCtrl.OnEvent("Change", OnRecordsSearchChanged)

    RecordsList := RecordsGui.AddListView("x20 y112 w980 h220 Grid -Multi", ["Druh", "Název", "Poskytovatel", "Platné do", "Cena", "Soubor", "Stav cesty"])
    RecordsList.OnEvent("DoubleClick", EditSelectedVehicleRecord)
    RecordsList.OnEvent("ItemSelect", OnRecordsSelectionChanged)
    RecordsList.OnEvent("ColClick", OnRecordsColumnClick)
    RecordsList.ModifyCol(1, "130")
    RecordsList.ModifyCol(2, "170")
    RecordsList.ModifyCol(3, "150")
    RecordsList.ModifyCol(4, "85")
    RecordsList.ModifyCol(5, "95")
    RecordsList.ModifyCol(6, "210")
    RecordsList.ModifyCol(7, "110")

    RecordsPathStatusLabel := RecordsGui.AddText("x20 y345 w980 h38", "")

    addButton := RecordsGui.AddButton("x25 y392 w120 h30", "Přidat záznam")
    addButton.OnEvent("Click", AddVehicleRecord)

    editButton := RecordsGui.AddButton("x155 y392 w120 h30", "Upravit záznam")
    editButton.OnEvent("Click", EditSelectedVehicleRecord)

    deleteButton := RecordsGui.AddButton("x285 y392 w130 h30", "Odstranit záznam")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleRecord)

    RecordsOpenFileButton := RecordsGui.AddButton("x425 y392 w120 h30", "Otevřít soubor")
    RecordsOpenFileButton.OnEvent("Click", OpenSelectedVehicleRecordFile)

    RecordsOpenFolderButton := RecordsGui.AddButton("x555 y392 w130 h30", "Otevřít složku")
    RecordsOpenFolderButton.OnEvent("Click", OpenSelectedVehicleRecordFolder)

    RecordsCopyPathButton := RecordsGui.AddButton("x695 y392 w130 h30", "Kopírovat cestu")
    RecordsCopyPathButton.OnEvent("Click", CopySelectedVehicleRecordPath)

    detailButton := RecordsGui.AddButton("x835 y392 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromRecords)

    closeButton := RecordsGui.AddButton("x965 y392 w80 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleRecordsDialog)

    RecordsGui.Show("w1065 h442")
    PopulateVehicleRecordsList(selectEntryId, true)

    if openAddEntry {
        OpenVehicleRecordForm("add")
    } else if (VisibleRecordIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleRecordsDialog(*) {
    global RecordsGui, RecordsVehicleId, RecordsList, RecordsSummaryLabel, RecordsAllEntries, RecordsSearchCtrl, RecordsPathStatusLabel, VisibleRecordIds, RecordsSortColumn, RecordsSortDescending, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton, MainGui

    if IsObject(RecordsGui) {
        RecordsGui.Destroy()
        RecordsGui := 0
    }

    RecordsVehicleId := ""
    RecordsList := 0
    RecordsSummaryLabel := 0
    RecordsAllEntries := []
    RecordsSearchCtrl := 0
    RecordsPathStatusLabel := 0
    VisibleRecordIds := []
    RecordsSortColumn := 4
    RecordsSortDescending := false
    RecordsOpenFileButton := 0
    RecordsOpenFolderButton := 0
    RecordsCopyPathButton := 0
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleRecordsList(selectEntryId := "", focusList := false) {
    global RecordsGui, RecordsVehicleId, RecordsList, RecordsSummaryLabel, RecordsAllEntries, VisibleRecordIds

    if !IsObject(RecordsGui) || !IsObject(RecordsList) {
        return
    }

    RecordsAllEntries := GetVehicleRecords(RecordsVehicleId)
    entries := FilterVehicleRecordsBySearch(RecordsAllEntries, GetRecordsSearchText())
    SortVisibleVehicleRecords(&entries)
    VisibleRecordIds := []
    selectedRow := 0

    if IsObject(RecordsSummaryLabel) {
        RecordsSummaryLabel.Text := BuildVehicleRecordsSummaryText(RecordsVehicleId)
    }

    RecordsList.Opt("-Redraw")
    RecordsList.Delete()
    for entry in entries {
        row := RecordsList.Add(
            "",
            entry.recordType,
            entry.title,
            entry.provider,
            entry.validTo,
            entry.price,
            ShortenText(GetFileNameFromPath(entry.filePath), 32),
            GetVehicleRecordPathStateText(entry)
        )
        VisibleRecordIds.Push(entry.id)
        if (selectEntryId != "" && entry.id = selectEntryId) {
            selectedRow := row
        }
    }
    RecordsList.Opt("+Redraw")

    if (entries.Length = 0) {
        UpdateVehicleRecordActionState()
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    RecordsList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
    UpdateVehicleRecordActionState()
}

OnRecordsSearchChanged(*) {
    selectedEntryId := ""
    entry := GetSelectedVehicleRecord()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleRecordsList(selectedEntryId)
}

OnRecordsSelectionChanged(*) {
    UpdateVehicleRecordActionState()
}

OnRecordsColumnClick(ctrl, column) {
    global RecordsSortColumn, RecordsSortDescending

    if (RecordsSortColumn = column) {
        RecordsSortDescending := !RecordsSortDescending
    } else {
        RecordsSortColumn := column
        RecordsSortDescending := false
    }

    SaveRecordsSortSettings(RecordsSortColumn, RecordsSortDescending)

    selectedEntryId := ""
    entry := GetSelectedVehicleRecord()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleRecordsList(selectedEntryId, true)
}

GetRecordsSearchText() {
    global RecordsSearchCtrl

    if !IsObject(RecordsSearchCtrl) {
        return ""
    }

    return Trim(RecordsSearchCtrl.Text)
}

FilterVehicleRecordsBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        haystack := StrLower(entry.recordType " " entry.title " " entry.provider " " entry.validFrom " " entry.validTo " " entry.price " " entry.filePath " " GetFileNameFromPath(entry.filePath) " " entry.note)
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

SortVisibleVehicleRecords(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleRecords(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleRecords(left, right) {
    global RecordsSortColumn, RecordsSortDescending

    result := CompareVisibleVehicleRecordsByColumn(left, right, RecordsSortColumn)
    if (result = 0 && RecordsSortColumn != 4) {
        result := CompareVisibleVehicleRecordsByColumn(left, right, 4)
    }
    if (result = 0) {
        result := CompareTextValues(left.id, right.id)
    }

    return RecordsSortDescending ? -result : result
}

CompareVisibleVehicleRecordsByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareTextValues(left.recordType, right.recordType)
        case 2:
            return CompareTextValues(left.title, right.title)
        case 3:
            return CompareTextValues(left.provider, right.provider)
        case 4:
            return CompareOptionalStampValues(ParseDueStamp(left.validTo), ParseDueStamp(right.validTo))
        case 5:
            return CompareOptionalMoneyTexts(left.price, right.price)
        case 6:
            return CompareTextValues(GetFileNameFromPath(left.filePath), GetFileNameFromPath(right.filePath))
        case 7:
            return CompareNumberValues(GetVehicleRecordPathStateSortValue(left), GetVehicleRecordPathStateSortValue(right))
    }

    return 0
}

UpdateVehicleRecordActionState() {
    global RecordsPathStatusLabel, RecordsOpenFileButton, RecordsOpenFolderButton, RecordsCopyPathButton

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        if IsObject(RecordsPathStatusLabel) {
            RecordsPathStatusLabel.Text := "Vyberte záznam, chcete-li zobrazit stav cesty k souboru nebo složce."
        }
        if IsObject(RecordsOpenFileButton) {
            RecordsOpenFileButton.Opt("+Disabled")
        }
        if IsObject(RecordsOpenFolderButton) {
            RecordsOpenFolderButton.Opt("+Disabled")
        }
        if IsObject(RecordsCopyPathButton) {
            RecordsCopyPathButton.Opt("+Disabled")
        }
        return
    }

    pathInfo := GetVehicleRecordPathInfo(entry)
    if IsObject(RecordsPathStatusLabel) {
        RecordsPathStatusLabel.Text := BuildVehicleRecordPathStatusText(pathInfo)
    }

    if IsObject(RecordsOpenFileButton) {
        RecordsOpenFileButton.Opt(pathInfo.kind = "file" ? "-Disabled" : "+Disabled")
    }
    if IsObject(RecordsOpenFolderButton) {
        RecordsOpenFolderButton.Opt(pathInfo.folderPath != "" ? "-Disabled" : "+Disabled")
    }
    if IsObject(RecordsCopyPathButton) {
        RecordsCopyPathButton.Opt(pathInfo.inputPath != "" ? "-Disabled" : "+Disabled")
    }
}

GetVehicleRecordPathInfo(entry) {
    path := Trim(entry.filePath)
    resolvedPath := ResolveVehicleRecordPath(path)

    if (path = "") {
        return {
            kind: "empty",
            inputPath: "",
            resolvedPath: "",
            folderPath: "",
            exists: false
        }
    }

    if DirExist(resolvedPath) {
        return {
            kind: "folder",
            inputPath: path,
            resolvedPath: resolvedPath,
            folderPath: resolvedPath,
            exists: true
        }
    }

    if FileExist(resolvedPath) {
        SplitPath(resolvedPath, , &directoryPath)
        return {
            kind: "file",
            inputPath: path,
            resolvedPath: resolvedPath,
            folderPath: directoryPath,
            exists: true
        }
    }

    SplitPath(resolvedPath, , &parentDirectory)
    if (parentDirectory != "" && DirExist(parentDirectory)) {
        return {
            kind: "missing_file",
            inputPath: path,
            resolvedPath: resolvedPath,
            folderPath: parentDirectory,
            exists: false
        }
    }

    return {
        kind: "missing_folder",
        inputPath: path,
        resolvedPath: resolvedPath,
        folderPath: "",
        exists: false
    }
}

ResolveVehicleRecordPath(path) {
    path := Trim(path)
    if (path = "") {
        return ""
    }

    if RegExMatch(path, "i)^[a-z]:[\\/]" ) || RegExMatch(path, "^\\\\") || RegExMatch(path, "^[\\/]") {
        return path
    }

    return A_ScriptDir "\" path
}

GetVehicleRecordPathStateText(entry) {
    return GetVehicleRecordPathStateLabel(GetVehicleRecordPathInfo(entry).kind)
}

GetVehicleRecordPathStateLabel(kind) {
    switch kind {
        case "file":
            return "Soubor"
        case "folder":
            return "Složka"
        case "missing_file":
            return "Chybí soubor"
        case "missing_folder":
            return "Chybí složka"
        default:
            return "Bez cesty"
    }
}

GetVehicleRecordPathStateSortValue(entry) {
    switch GetVehicleRecordPathInfo(entry).kind {
        case "file":
            return 1
        case "folder":
            return 2
        case "missing_file":
            return 3
        case "missing_folder":
            return 4
        default:
            return 5
    }
}

BuildVehicleRecordPathStatusText(pathInfo) {
    switch pathInfo.kind {
        case "file":
            return "Stav cesty: soubor je dostupný. Cesta: " pathInfo.inputPath
        case "folder":
            return "Stav cesty: záznam míří na existující složku. Cesta: " pathInfo.inputPath
        case "missing_file":
            return "Stav cesty: složka existuje, ale soubor chybí. Cesta: " pathInfo.inputPath
        case "missing_folder":
            return "Stav cesty: cílová složka ani soubor nejsou dostupné. Cesta: " pathInfo.inputPath
        default:
            return "Stav cesty: u vybraného záznamu není vyplněná cesta k souboru ani složce."
    }
}

GetSelectedVehicleRecord() {
    global RecordsList, VisibleRecordIds

    if !IsObject(RecordsList) {
        return ""
    }

    row := RecordsList.GetNext(0)
    if !row || row > VisibleRecordIds.Length {
        return ""
    }

    return FindVehicleRecordById(VisibleRecordIds[row])
}

AddVehicleRecord(*) {
    OpenVehicleRecordForm("add")
}

EditSelectedVehicleRecord(*) {
    global AppTitle

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, který chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleRecordForm("edit", entry)
}

DeleteSelectedVehicleRecord(*) {
    global AppTitle, VehicleRecords

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, který chcete odstranit.", AppTitle, 0x40)
        return
    }

    result := MsgBox("Opravdu chcete odstranit záznam " entry.title "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleRecordIndexById(entry.id)
    if !index {
        return
    }

    VehicleRecords.RemoveAt(index)
    SaveVehicleRecords()
    PopulateVehicleRecordsList()
}

OpenSelectedVehicleRecordFile(*) {
    global AppTitle

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, jehož soubor chcete otevřít.", AppTitle, 0x40)
        return
    }

    pathInfo := GetVehicleRecordPathInfo(entry)
    switch pathInfo.kind {
        case "empty":
            MsgBox("Vybraný záznam nemá vyplněnou cestu k souboru.", AppTitle, 0x40)
            return
        case "folder":
            MsgBox("Vybraná cesta vede na složku. Pro ni použijte tlačítko nebo zkratku pro otevření složky.", AppTitle, 0x40)
            return
        case "missing_file":
            MsgBox("Složka pro vybraný záznam existuje, ale soubor se na zadané cestě nenašel.`n`n" pathInfo.inputPath, AppTitle, 0x30)
            return
        case "missing_folder":
            MsgBox("Zadaná cesta není dostupná, protože chybí cílová složka nebo soubor.`n`n" pathInfo.inputPath, AppTitle, 0x30)
            return
    }

    try {
        Run('"' pathInfo.resolvedPath '"')
    } catch as err {
        MsgBox("Soubor se nepodařilo otevřít.`n`n" err.Message, AppTitle, 0x30)
    }
}

OpenSelectedVehicleRecordFolder(*) {
    global AppTitle

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, jehož složku chcete otevřít.", AppTitle, 0x40)
        return
    }

    pathInfo := GetVehicleRecordPathInfo(entry)
    if (pathInfo.kind = "empty") {
        MsgBox("Vybraný záznam nemá vyplněnou cestu k souboru.", AppTitle, 0x40)
        return
    }

    if (pathInfo.folderPath = "") {
        MsgBox("Nepodařilo se najít existující složku pro vybraný záznam.`n`n" pathInfo.inputPath, AppTitle, 0x30)
        return
    }

    try {
        Run('"' pathInfo.folderPath '"')
    } catch as err {
        MsgBox("Složku se nepodařilo otevřít.`n`n" err.Message, AppTitle, 0x30)
    }
}

CopySelectedVehicleRecordPath(*) {
    global AppTitle

    entry := GetSelectedVehicleRecord()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte záznam, jehož cestu chcete zkopírovat.", AppTitle, 0x40)
        return
    }

    path := Trim(entry.filePath)
    if (path = "") {
        MsgBox("Vybraný záznam nemá vyplněnou cestu k souboru.", AppTitle, 0x40)
        return
    }

    A_Clipboard := path
    MsgBox("Cesta byla zkopírována do schránky.`n`n" path, AppTitle, 0x40)
}

OpenVehicleDetailFromRecords(*) {
    global RecordsVehicleId

    vehicle := FindVehicleById(RecordsVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleRecordsDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleRecordForm(mode, entry := "") {
    global AppTitle, RecordFormGui, RecordFormControls, RecordFormMode, RecordFormEntryId, RecordFormVehicleId, RecordsGui, RecordsVehicleId, RecordTypeOptions

    if IsObject(RecordFormGui) {
        WinActivate("ahk_id " RecordFormGui.Hwnd)
        return
    }

    if !IsObject(RecordsGui) {
        return
    }

    RecordFormMode := mode
    RecordFormEntryId := IsObject(entry) ? entry.id : ""
    RecordFormVehicleId := IsObject(entry) ? entry.vehicleId : RecordsVehicleId
    RecordFormControls := {}

    title := (mode = "edit") ? "Upravit záznam" : "Přidat záznam"
    RecordFormGui := Gui("+Owner" RecordsGui.Hwnd, AppTitle " - " title)
    RecordFormGui.SetFont("s10", "Segoe UI")
    RecordFormGui.OnEvent("Close", CloseVehicleRecordForm)
    RecordFormGui.OnEvent("Escape", CloseVehicleRecordForm)

    RecordsGui.Opt("+Disabled")

    RecordFormGui.AddText("x20 y20 w520", "Druh a název záznamu jsou povinné. Platnost pojištění nebo dokladu zadejte jako MM/RRRR.")

    RecordFormGui.AddText("x20 y60 w170", "Druh záznamu (povinné)")
    RecordFormControls.recordType := RecordFormGui.AddDropDownList("x210 y57 w240", RecordTypeOptions)

    RecordFormGui.AddText("x20 y95 w170", "Název záznamu (povinné)")
    RecordFormControls.title := RecordFormGui.AddEdit("x210 y92 w240")

    RecordFormGui.AddText("x20 y130 w170", "Poskytovatel / vydavatel")
    RecordFormControls.provider := RecordFormGui.AddEdit("x210 y127 w240")

    RecordFormGui.AddText("x20 y165 w170", "Platné od (volitelné)")
    RecordFormControls.validFrom := RecordFormGui.AddEdit("x210 y162 w110")

    RecordFormGui.AddText("x335 y165 w115", "Platné do (volitelné)")
    RecordFormControls.validTo := RecordFormGui.AddEdit("x440 y162 w110")

    RecordFormGui.AddText("x20 y200 w170", "Cena / částka (volitelné)")
    RecordFormControls.price := RecordFormGui.AddEdit("x210 y197 w240")

    RecordFormGui.AddText("x20 y235 w170", "Soubor nebo cesta")
    RecordFormControls.filePath := RecordFormGui.AddEdit("x20 y260 w435")
    browseButton := RecordFormGui.AddButton("x465 y258 w85 h26", "Vybrat")
    browseButton.OnEvent("Click", SelectVehicleRecordFile)

    RecordFormGui.AddText("x20 y295 w170", "Poznámka (volitelné)")
    RecordFormControls.note := RecordFormGui.AddEdit("x20 y320 w530 h80 Multi")

    saveButton := RecordFormGui.AddButton("x180 y415 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleRecordFromForm)

    cancelButton := RecordFormGui.AddButton("x310 y415 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleRecordForm)

    if IsObject(entry) {
        SetDropDownToText(RecordFormControls.recordType, entry.recordType, RecordTypeOptions)
        RecordFormControls.title.Text := entry.title
        RecordFormControls.provider.Text := entry.provider
        RecordFormControls.validFrom.Text := entry.validFrom
        RecordFormControls.validTo.Text := entry.validTo
        RecordFormControls.price.Text := entry.price
        RecordFormControls.filePath.Text := entry.filePath
        RecordFormControls.note.Text := entry.note
    } else {
        RecordFormControls.recordType.Value := 1
    }

    RecordFormGui.Show("w580 h460")
    RecordFormControls.recordType.Focus()
}

CloseVehicleRecordForm(*) {
    global RecordFormGui, RecordFormControls, RecordFormMode, RecordFormEntryId, RecordFormVehicleId, RecordsGui

    if IsObject(RecordFormGui) {
        RecordFormGui.Destroy()
        RecordFormGui := 0
    }

    RecordFormControls := {}
    RecordFormMode := ""
    RecordFormEntryId := ""
    RecordFormVehicleId := ""

    if IsObject(RecordsGui) {
        RecordsGui.Opt("-Disabled")
        WinActivate("ahk_id " RecordsGui.Hwnd)
    }
}

SelectVehicleRecordFile(*) {
    global AppTitle, A_DefaultDialogTitle, RecordFormControls

    A_DefaultDialogTitle := AppTitle
    selectedPath := FileSelect(1, A_ScriptDir, "Vyberte soubor k záznamu")
    if (selectedPath = "") {
        return
    }

    RecordFormControls.filePath.Text := selectedPath
}

SaveVehicleRecordFromForm(*) {
    global AppTitle, VehicleRecords, RecordFormControls, RecordFormMode, RecordFormEntryId, RecordFormVehicleId

    recordType := Trim(RecordFormControls.recordType.Text)
    title := Trim(RecordFormControls.title.Text)
    provider := Trim(RecordFormControls.provider.Text)
    validFrom := NormalizeMonthYear(RecordFormControls.validFrom.Text)
    validTo := NormalizeMonthYear(RecordFormControls.validTo.Text)
    price := Trim(RecordFormControls.price.Text)
    filePath := Trim(RecordFormControls.filePath.Text)
    note := Trim(RecordFormControls.note.Text)

    if (recordType = "") {
        MsgBox("Vyberte prosím druh záznamu.", AppTitle, 0x30)
        RecordFormControls.recordType.Focus()
        return
    }

    if (title = "") {
        MsgBox("Vyplňte prosím název záznamu.", AppTitle, 0x30)
        RecordFormControls.title.Focus()
        return
    }

    if (Trim(RecordFormControls.validFrom.Text) != "" && validFrom = "") {
        MsgBox("Pole Platné od musí být ve formátu MM/RRRR.", AppTitle, 0x30)
        RecordFormControls.validFrom.Focus()
        return
    }

    if (Trim(RecordFormControls.validTo.Text) != "" && validTo = "") {
        MsgBox("Pole Platné do musí být ve formátu MM/RRRR.", AppTitle, 0x30)
        RecordFormControls.validTo.Focus()
        return
    }

    if (validFrom != "" && validTo != "" && ParseDueStamp(validFrom) > ParseDueStamp(validTo)) {
        MsgBox("Pole Platné od nesmí být později než pole Platné do.", AppTitle, 0x30)
        RecordFormControls.validFrom.Focus()
        return
    }

    entry := {
        id: (RecordFormMode = "edit") ? RecordFormEntryId : GenerateVehicleRecordId(),
        vehicleId: RecordFormVehicleId,
        recordType: recordType,
        title: title,
        provider: provider,
        validFrom: validFrom,
        validTo: validTo,
        price: price,
        filePath: filePath,
        note: note
    }

    index := FindVehicleRecordIndexById(entry.id)
    if index {
        VehicleRecords[index] := entry
    } else {
        VehicleRecords.Push(entry)
    }

    SaveVehicleRecords()
    CloseVehicleRecordForm()
    PopulateVehicleRecordsList(entry.id, true)
}

OpenVehicleReminderDialog(vehicle, openAddEntry := false, selectEntryId := "") {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderVehicleId, ReminderList, ReminderSummaryLabel, ReminderAllEntries, ReminderSearchCtrl, VisibleReminderIds, ReminderSortColumn, ReminderSortDescending, ReminderFormGui, CostSummaryGui

    if IsObject(ReminderGui) {
        WinActivate("ahk_id " ReminderGui.Hwnd)
        return
    }

    for guiRef in [FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderFormGui, CostSummaryGui] {
        if IsObject(guiRef) {
            WinActivate("ahk_id " guiRef.Hwnd)
            return
        }
    }

    ShowMainWindow()

    ReminderVehicleId := vehicle.id
    ReminderAllEntries := []
    ReminderSearchCtrl := 0
    VisibleReminderIds := []
    ReminderSortColumn := GetReminderSortColumnSetting()
    ReminderSortDescending := GetReminderSortDescendingSetting()
    ReminderGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Vlastní připomínky")
    ReminderGui.SetFont("s10", "Segoe UI")
    ReminderGui.OnEvent("Close", CloseVehicleReminderDialog)
    ReminderGui.OnEvent("Escape", CloseVehicleReminderDialog)

    MainGui.Opt("+Disabled")

    ReminderGui.AddText("x20 y20 w900", "Zde můžete evidovat vlastní termíny a připomínky pro vozidlo " vehicle.name ". Datum zadejte jako DD.MM.RRRR. U opakovaných připomínek můžete termín po vyřízení posunout tlačítkem na další cyklus.")
    ReminderSummaryLabel := ReminderGui.AddText("x20 y50 w900", "")
    ReminderGui.AddText("x20 y82 w310", "Hledat název, termín, opakování, stav nebo poznámku")
    ReminderSearchCtrl := ReminderGui.AddEdit("x340 y79 w360")
    ReminderSearchCtrl.OnEvent("Change", OnReminderSearchChanged)

    ReminderList := ReminderGui.AddListView("x20 y112 w900 h255 Grid -Multi", ["Název", "Termín", "Upozornit dnů předem", "Opakování", "Stav", "Poznámka"])
    ReminderList.OnEvent("DoubleClick", EditSelectedVehicleReminder)
    ReminderList.OnEvent("ColClick", OnReminderColumnClick)
    ReminderList.ModifyCol(1, "190")
    ReminderList.ModifyCol(2, "95")
    ReminderList.ModifyCol(3, "125")
    ReminderList.ModifyCol(4, "115")
    ReminderList.ModifyCol(5, "90")
    ReminderList.ModifyCol(6, "245")

    addButton := ReminderGui.AddButton("x70 y382 w120 h30", "Přidat záznam")
    addButton.OnEvent("Click", AddVehicleReminder)

    editButton := ReminderGui.AddButton("x200 y382 w120 h30", "Upravit záznam")
    editButton.OnEvent("Click", EditSelectedVehicleReminder)

    advanceButton := ReminderGui.AddButton("x330 y382 w160 h30", "Posunout na další")
    advanceButton.OnEvent("Click", AdvanceSelectedVehicleReminder)

    deleteButton := ReminderGui.AddButton("x500 y382 w130 h30", "Odstranit záznam")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleReminder)

    detailButton := ReminderGui.AddButton("x640 y382 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromReminder)

    closeButton := ReminderGui.AddButton("x770 y382 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleReminderDialog)

    ReminderGui.Show("w940 h432")
    PopulateVehicleReminderList(selectEntryId, true)

    if openAddEntry {
        OpenVehicleReminderForm("add")
    } else if (VisibleReminderIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleReminderDialog(*) {
    global ReminderGui, ReminderVehicleId, ReminderList, ReminderSummaryLabel, ReminderAllEntries, ReminderSearchCtrl, VisibleReminderIds, ReminderSortColumn, ReminderSortDescending, MainGui

    if IsObject(ReminderGui) {
        ReminderGui.Destroy()
        ReminderGui := 0
    }

    ReminderVehicleId := ""
    ReminderList := 0
    ReminderSummaryLabel := 0
    ReminderAllEntries := []
    ReminderSearchCtrl := 0
    VisibleReminderIds := []
    ReminderSortColumn := 2
    ReminderSortDescending := false
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleReminderList(selectEntryId := "", focusList := false) {
    global ReminderGui, ReminderVehicleId, ReminderList, ReminderSummaryLabel, ReminderAllEntries, VisibleReminderIds

    if !IsObject(ReminderGui) || !IsObject(ReminderList) {
        return
    }

    ReminderAllEntries := GetVehicleReminderEntries(ReminderVehicleId)
    entries := FilterVehicleReminderEntriesBySearch(ReminderAllEntries, GetReminderSearchText())
    SortVisibleVehicleReminderEntries(&entries)
    VisibleReminderIds := []
    selectedRow := 0

    if IsObject(ReminderSummaryLabel) {
        ReminderSummaryLabel.Text := BuildVehicleReminderSummaryText(ReminderVehicleId)
    }

    ReminderList.Opt("-Redraw")
    ReminderList.Delete()
    for entry in entries {
        row := ReminderList.Add(
            "",
            entry.title,
            entry.dueDate,
            entry.reminderDays,
            GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : ""),
            GetReminderExpirationStatusText(entry.dueDate, entry.reminderDays + 0),
            ShortenText(entry.note, 80)
        )
        VisibleReminderIds.Push(entry.id)
        if (selectEntryId != "" && entry.id = selectEntryId) {
            selectedRow := row
        }
    }
    ReminderList.Opt("+Redraw")

    if (entries.Length = 0) {
        return
    }

    if !selectedRow {
        selectedRow := 1
    }

    ReminderList.Modify(selectedRow, focusList ? "Select Focus Vis" : "Select Vis")
}

OnReminderSearchChanged(*) {
    selectedEntryId := ""
    entry := GetSelectedVehicleReminder()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleReminderList(selectedEntryId)
}

OnReminderColumnClick(ctrl, column) {
    global ReminderSortColumn, ReminderSortDescending

    if (ReminderSortColumn = column) {
        ReminderSortDescending := !ReminderSortDescending
    } else {
        ReminderSortColumn := column
        ReminderSortDescending := false
    }

    SaveReminderSortSettings(ReminderSortColumn, ReminderSortDescending)

    selectedEntryId := ""
    entry := GetSelectedVehicleReminder()
    if IsObject(entry) {
        selectedEntryId := entry.id
    }

    PopulateVehicleReminderList(selectedEntryId, true)
}

GetReminderSearchText() {
    global ReminderSearchCtrl

    if !IsObject(ReminderSearchCtrl) {
        return ""
    }

    return Trim(ReminderSearchCtrl.Text)
}

FilterVehicleReminderEntriesBySearch(entries, searchText := "") {
    filtered := []
    needle := StrLower(Trim(searchText))

    for entry in entries {
        repeatLabel := GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : "")
        statusText := GetReminderExpirationStatusText(entry.dueDate, entry.reminderDays + 0)
        haystack := StrLower(entry.title " " entry.dueDate " " entry.reminderDays " " repeatLabel " " statusText " " entry.note)
        if (needle = "" || InStr(haystack, needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

SortVisibleVehicleReminderEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVisibleVehicleReminderEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVisibleVehicleReminderEntries(left, right) {
    global ReminderSortColumn, ReminderSortDescending

    result := CompareVisibleVehicleReminderEntriesByColumn(left, right, ReminderSortColumn)
    if (result = 0 && ReminderSortColumn != 2) {
        result := CompareVisibleVehicleReminderEntriesByColumn(left, right, 2)
    }
    if (result = 0) {
        result := CompareTextValues(left.id, right.id)
    }

    return ReminderSortDescending ? -result : result
}

CompareVisibleVehicleReminderEntriesByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareTextValues(left.title, right.title)
        case 2:
            return CompareOptionalStampValues(ParseReminderDueStamp(left.dueDate), ParseReminderDueStamp(right.dueDate))
        case 3:
            return CompareOptionalIntegerTexts(left.reminderDays, right.reminderDays)
        case 4:
            return CompareTextValues(GetReminderRepeatLabel(left.HasOwnProp("repeatMode") ? left.repeatMode : ""), GetReminderRepeatLabel(right.HasOwnProp("repeatMode") ? right.repeatMode : ""))
        case 5:
            return CompareTextValues(GetReminderExpirationStatusText(left.dueDate, left.reminderDays + 0), GetReminderExpirationStatusText(right.dueDate, right.reminderDays + 0))
        case 6:
            return CompareTextValues(left.note, right.note)
    }

    return 0
}

GetSelectedVehicleReminder() {
    global ReminderList, VisibleReminderIds

    if !IsObject(ReminderList) {
        return ""
    }

    row := ReminderList.GetNext(0)
    if !row || row > VisibleReminderIds.Length {
        return ""
    }

    return FindVehicleReminderById(VisibleReminderIds[row])
}

AddVehicleReminder(*) {
    OpenVehicleReminderForm("add")
}

EditSelectedVehicleReminder(*) {
    global AppTitle

    entry := GetSelectedVehicleReminder()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte připomínku, kterou chcete upravit.", AppTitle, 0x40)
        return
    }

    OpenVehicleReminderForm("edit", entry)
}

AdvanceSelectedVehicleReminder(*) {
    global AppTitle, VehicleReminders, ReminderVehicleId

    entry := GetSelectedVehicleReminder()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte připomínku, kterou chcete posunout na další termín.", AppTitle, 0x40)
        return
    }

    years := GetReminderRepeatYears(entry.HasOwnProp("repeatMode") ? entry.repeatMode : "")
    if (years < 1) {
        MsgBox("Vybraná připomínka není nastavena jako opakovaná. Opakování můžete nastavit v editaci připomínky.", AppTitle, 0x40)
        return
    }

    nextDueDate := AddYearsToEventDate(entry.dueDate, years)
    if (nextDueDate = "") {
        MsgBox("Nepodařilo se vypočítat další termín připomínky.", AppTitle, 0x30)
        return
    }

    index := FindVehicleReminderIndexById(entry.id)
    if !index {
        return
    }

    VehicleReminders[index].dueDate := nextDueDate
    SaveVehicleReminders()
    PopulateVehicleReminderList(entry.id, true)
    RefreshVehicleList(ReminderVehicleId)
    CheckDueVehicles(false, false)
}

DeleteSelectedVehicleReminder(*) {
    global AppTitle, VehicleReminders

    entry := GetSelectedVehicleReminder()
    if !IsObject(entry) {
        MsgBox("Nejprve vyberte připomínku, kterou chcete odstranit.", AppTitle, 0x40)
        return
    }

    result := MsgBox("Opravdu chcete odstranit připomínku " entry.title "?", AppTitle, 0x34)
    if (result != "Yes") {
        return
    }

    index := FindVehicleReminderIndexById(entry.id)
    if !index {
        return
    }

    VehicleReminders.RemoveAt(index)
    SaveVehicleReminders()
    PopulateVehicleReminderList()
    RefreshVehicleList(ReminderVehicleId)
    CheckDueVehicles(false, false)
}

OpenVehicleDetailFromReminder(*) {
    global ReminderVehicleId

    vehicle := FindVehicleById(ReminderVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleReminderDialog()
    OpenVehicleDetailDialog(vehicle)
}

OpenVehicleReminderForm(mode, entry := "") {
    global AppTitle, ReminderGui, ReminderVehicleId, ReminderFormGui, ReminderFormControls, ReminderFormMode, ReminderFormEntryId, ReminderFormVehicleId, ReminderRepeatOptions

    if IsObject(ReminderFormGui) {
        WinActivate("ahk_id " ReminderFormGui.Hwnd)
        return
    }

    if !IsObject(ReminderGui) {
        return
    }

    ReminderFormMode := mode
    ReminderFormEntryId := IsObject(entry) ? entry.id : ""
    ReminderFormVehicleId := IsObject(entry) ? entry.vehicleId : ReminderVehicleId
    ReminderFormControls := {}

    title := (mode = "edit") ? "Upravit připomínku" : "Přidat připomínku"
    ReminderFormGui := Gui("+Owner" ReminderGui.Hwnd, AppTitle " - " title)
    ReminderFormGui.SetFont("s10", "Segoe UI")
    ReminderFormGui.OnEvent("Close", CloseVehicleReminderForm)
    ReminderFormGui.OnEvent("Escape", CloseVehicleReminderForm)

    ReminderGui.Opt("+Disabled")

    ReminderFormGui.AddText("x20 y20 w520", "Název, termín a počet dnů předem jsou povinné. Datum zadávejte jako DD.MM.RRRR. Opakování je volitelné.")

    ReminderFormGui.AddText("x20 y60 w180", "Název připomínky (povinné)")
    ReminderFormControls.title := ReminderFormGui.AddEdit("x220 y57 w240")

    ReminderFormGui.AddText("x20 y95 w180", "Termín (povinné)")
    ReminderFormControls.dueDate := ReminderFormGui.AddEdit("x220 y92 w140")

    ReminderFormGui.AddText("x20 y130 w180", "Upozornit dnů předem (povinné)")
    ReminderFormControls.reminderDays := ReminderFormGui.AddEdit("x220 y127 w140 Limit3 Number")

    ReminderFormGui.AddText("x20 y165 w180", "Opakování (volitelné)")
    ReminderFormControls.repeatMode := ReminderFormGui.AddDropDownList("x220 y162 w180", ReminderRepeatOptions)

    ReminderFormGui.AddText("x20 y200 w180", "Poznámka (volitelné)")
    ReminderFormControls.note := ReminderFormGui.AddEdit("x20 y225 w440 h95 Multi")

    saveButton := ReminderFormGui.AddButton("x150 y335 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveVehicleReminderFromForm)

    cancelButton := ReminderFormGui.AddButton("x280 y335 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleReminderForm)

    if IsObject(entry) {
        ReminderFormControls.title.Text := entry.title
        ReminderFormControls.dueDate.Text := entry.dueDate
        ReminderFormControls.reminderDays.Text := entry.reminderDays
        ReminderFormControls.note.Text := entry.note
        SetDropDownToText(ReminderFormControls.repeatMode, GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : ""), ReminderRepeatOptions)
    } else {
        ReminderFormControls.reminderDays.Text := "30"
        ReminderFormControls.repeatMode.Value := 1
    }

    ReminderFormGui.Show("w480 h385")
    ReminderFormControls.title.Focus()
}

CloseVehicleReminderForm(*) {
    global ReminderFormGui, ReminderFormControls, ReminderFormMode, ReminderFormEntryId, ReminderFormVehicleId, ReminderGui

    if IsObject(ReminderFormGui) {
        ReminderFormGui.Destroy()
        ReminderFormGui := 0
    }

    ReminderFormControls := {}
    ReminderFormMode := ""
    ReminderFormEntryId := ""
    ReminderFormVehicleId := ""

    if IsObject(ReminderGui) {
        ReminderGui.Opt("-Disabled")
        WinActivate("ahk_id " ReminderGui.Hwnd)
    }
}

SaveVehicleReminderFromForm(*) {
    global AppTitle, VehicleReminders, ReminderFormControls, ReminderFormMode, ReminderFormEntryId, ReminderFormVehicleId, ReminderVehicleId

    title := Trim(ReminderFormControls.title.Text)
    dueDate := NormalizeEventDate(ReminderFormControls.dueDate.Text)
    reminderDaysText := Trim(ReminderFormControls.reminderDays.Text)
    repeatMode := NormalizeReminderRepeat(ReminderFormControls.repeatMode.Text)
    note := Trim(ReminderFormControls.note.Text)

    if (title = "") {
        MsgBox("Vyplňte prosím název připomínky.", AppTitle, 0x30)
        ReminderFormControls.title.Focus()
        return
    }

    if (dueDate = "") {
        MsgBox("Pole Termín je povinné a musí být ve formátu DD.MM.RRRR.", AppTitle, 0x30)
        ReminderFormControls.dueDate.Focus()
        return
    }

    if !RegExMatch(reminderDaysText, "^\d{1,3}$") {
        MsgBox("Pole Upozornit dnů předem musí být celé číslo od 0 do 999.", AppTitle, 0x30)
        ReminderFormControls.reminderDays.Focus()
        return
    }

    reminderDays := reminderDaysText + 0
    entry := {
        id: (ReminderFormMode = "edit") ? ReminderFormEntryId : GenerateVehicleReminderId(),
        vehicleId: ReminderFormVehicleId,
        title: title,
        dueDate: dueDate,
        reminderDays: reminderDays,
        repeatMode: repeatMode,
        note: note
    }

    index := FindVehicleReminderIndexById(entry.id)
    if index {
        VehicleReminders[index] := entry
    } else {
        VehicleReminders.Push(entry)
    }

    SaveVehicleReminders()
    CloseVehicleReminderForm()
    PopulateVehicleReminderList(entry.id, true)
    RefreshVehicleList(ReminderVehicleId)
    CheckDueVehicles(false, false)
}

OpenVehicleCostSummaryDialog(vehicle) {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui
    global CostSummaryGui, CostSummaryVehicleId, CostSummarySummaryLabel, CostSummaryList, CostSummaryPeriodYearCtrl, CostSummaryPresetCtrl, CostSummaryFromMonthCtrl, CostSummaryToMonthCtrl, CostSummaryPeriodSummaryLabel, CostSummaryPeriodList
    global CostSummaryPresetOptions, MonthOptionLabels

    if IsObject(CostSummaryGui) {
        WinActivate("ahk_id " CostSummaryGui.Hwnd)
        return
    }

    for guiRef in [FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui, FuelGui, FuelFormGui, RecordsGui, RecordFormGui, ReminderGui, ReminderFormGui] {
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
            extraParts.Push("Soubor: " entry.filePath)
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
        summary.years.Push(item)
    }

    years := summary.years
    SortVehicleCostSummaryYears(&years)
    return summary
}

BuildVehicleCostPeriodSummary(vehicleId, yearLabel, fromMonth, toMonth) {
    global VehicleFuelLog, VehicleHistory, VehicleRecords

    yearValue := yearLabel + 0
    summary := {
        year: yearLabel,
        fromMonth: fromMonth,
        toMonth: toMonth,
        totalFuel: 0.0,
        totalHistory: 0.0,
        totalRecords: 0.0,
        fuelCount: 0,
        historyCount: 0,
        recordCount: 0,
        parsedCount: 0,
        skippedCount: 0,
        undatedCount: 0
    }

    for entry in VehicleFuelLog {
        if (entry.vehicleId != vehicleId || Trim(entry.totalCost) = "") {
            continue
        }

        if !TryGetEventYearMonth(entry.entryDate, &entryYear, &entryMonth) {
            summary.undatedCount += 1
            continue
        }
        if (entryYear != yearValue || entryMonth < fromMonth || entryMonth > toMonth) {
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

        if !TryGetEventYearMonth(entry.eventDate, &entryYear, &entryMonth) {
            summary.undatedCount += 1
            continue
        }
        if (entryYear != yearValue || entryMonth < fromMonth || entryMonth > toMonth) {
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

        if !TryGetRecordYearMonth(entry, &entryYear, &entryMonth) {
            summary.undatedCount += 1
            continue
        }
        if (entryYear != yearValue || entryMonth < fromMonth || entryMonth > toMonth) {
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

    return summary
}

BuildVehicleCostPeriodSummaryText(summary) {
    total := summary.totalFuel + summary.totalHistory + summary.totalRecords
    text := "Období " BuildVehicleCostPeriodLabel(summary.year, summary.fromMonth, summary.toMonth) ". "
    text .= "Celkem nákladů: " FormatCostAmount(total) ". "
    text .= "Započteno položek: " summary.parsedCount "."
    if (summary.skippedCount > 0) {
        text .= " Přeskočeno nečíselných částek: " summary.skippedCount "."
    }
    if (summary.undatedCount > 0) {
        text .= " Položek bez použitelného data: " summary.undatedCount "."
    }
    return text
}

BuildVehicleCostPeriodLabel(yearLabel, fromMonth, toMonth) {
    if (fromMonth = toMonth) {
        return Format("{:02}/{}", fromMonth, yearLabel)
    }

    return Format("{:02}/{} až {:02}/{}", fromMonth, yearLabel, toMonth, yearLabel)
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

LoadVehicles() {
    global Vehicles, VehiclesFile

    Vehicles := []
    if !FileExist(VehiclesFile) {
        return
    }

    content := FileRead(VehiclesFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap data v3") {
        ShowVehiclesFileFormatError("Soubor vozidel není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap data v3'.")
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            ShowVehiclesFileFormatError("Soubor vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index ". Vehimap očekává jen jednu hlavičku '# Vehimap data v3'.")
            Vehicles := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 12) {
            ShowVehiclesFileFormatError("Soubor vozidel je poškozený nebo není ve formátu v3. Řádek " index " musí obsahovat přesně 12 polí oddělených tabulátory.")
            Vehicles := []
            return
        }

        Vehicles.Push({
            id: UnescapeField(fields[1]),
            name: UnescapeField(fields[2]),
            category: NormalizeCategory(UnescapeField(fields[3])),
            vehicleType: UnescapeField(fields[4]),
            makeModel: UnescapeField(fields[5]),
            plate: UnescapeField(fields[6]),
            year: UnescapeField(fields[7]),
            power: UnescapeField(fields[8]),
            lastTk: UnescapeField(fields[9]),
            nextTk: UnescapeField(fields[10]),
            greenCardFrom: UnescapeField(fields[11]),
            greenCardTo: UnescapeField(fields[12])
        })
    }
}

ShowVehiclesFileFormatError(message) {
    global AppTitle, VehiclesFile

    MsgBox(message "`n`nZkontrolujte soubor:`n" VehiclesFile, AppTitle, 0x30)
}

SaveVehicles() {
    global Vehicles, VehiclesFile

    lines := ["# Vehimap data v3"]
    for vehicle in Vehicles {
        lines.Push(
            EscapeField(vehicle.id) "`t"
            EscapeField(vehicle.name) "`t"
            EscapeField(vehicle.category) "`t"
            EscapeField(vehicle.vehicleType) "`t"
            EscapeField(vehicle.makeModel) "`t"
            EscapeField(vehicle.plate) "`t"
            EscapeField(vehicle.year) "`t"
            EscapeField(vehicle.power) "`t"
            EscapeField(vehicle.lastTk) "`t"
            EscapeField(vehicle.nextTk) "`t"
            EscapeField(vehicle.greenCardFrom) "`t"
            EscapeField(vehicle.greenCardTo)
        )
    }

    output := JoinLines(lines)
    if FileExist(VehiclesFile) {
        FileDelete(VehiclesFile)
    }
    FileAppend(output, VehiclesFile, "UTF-8")
}

LoadVehicleHistory() {
    global AppTitle, VehicleHistory, HistoryFile

    VehicleHistory := []
    if !FileExist(HistoryFile) {
        return
    }

    content := FileRead(HistoryFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap history v1") {
        MsgBox("Soubor historie není v podporovaném formátu.`n`nZkontrolujte soubor:`n" HistoryFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor historie obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" HistoryFile, AppTitle, 0x30)
            VehicleHistory := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 6 && fields.Length != 7) {
            MsgBox("Soubor historie je poškozený. Řádek " index " musí obsahovat 6 nebo 7 polí oddělených tabulátory.`n`nZkontrolujte soubor:`n" HistoryFile, AppTitle, 0x30)
            VehicleHistory := []
            return
        }

        VehicleHistory.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            eventDate: UnescapeField(fields[3]),
            eventType: UnescapeField(fields[4]),
            odometer: UnescapeField(fields[5]),
            cost: UnescapeField(fields[6]),
            note: (fields.Length = 7) ? UnescapeField(fields[7]) : ""
        })
    }
}

SaveVehicleHistory() {
    global VehicleHistory, HistoryFile

    output := BuildHistoryDataContent()
    if FileExist(HistoryFile) {
        FileDelete(HistoryFile)
    }
    FileAppend(output, HistoryFile, "UTF-8")
}

BuildHistoryDataContent() {
    global VehicleHistory

    lines := ["# Vehimap history v1"]
    for event in VehicleHistory {
        lines.Push(
            EscapeField(event.id) "`t"
            EscapeField(event.vehicleId) "`t"
            EscapeField(event.eventDate) "`t"
            EscapeField(event.eventType) "`t"
            EscapeField(event.odometer) "`t"
            EscapeField(event.cost) "`t"
            EscapeField(event.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleHistory(vehicleId) {
    global VehicleHistory

    filtered := []
    for event in VehicleHistory {
        if (event.vehicleId != vehicleId) {
            filtered.Push(event)
        }
    }

    VehicleHistory := filtered
    SaveVehicleHistory()
}

GetVehicleHistoryEntries(vehicleId) {
    global VehicleHistory

    entries := []
    for event in VehicleHistory {
        if (event.vehicleId = vehicleId) {
            entries.Push(event)
        }
    }

    SortVehicleHistoryByDateDescending(&entries)
    return entries
}

GetRecentVehicleHistoryEntries(vehicleId, maxCount := 5) {
    entries := GetVehicleHistoryEntries(vehicleId)
    if (entries.Length <= maxCount) {
        return entries
    }

    recent := []
    Loop maxCount {
        recent.Push(entries[A_Index])
    }

    return recent
}

GetVehicleHistoryCount(vehicleId) {
    return GetVehicleHistoryEntries(vehicleId).Length
}

BuildVehicleHistorySummaryText(vehicleId) {
    entries := GetVehicleHistoryEntries(vehicleId)
    if (entries.Length = 0) {
        return "K tomuto vozidlu zatím není uložená žádná historie událostí."
    }

    latest := entries[1]
    summary := "Celkem událostí: " entries.Length ". Poslední událost: " latest.eventType " (" latest.eventDate ")."
    if (latest.odometer != "") {
        summary .= " Tachometr: " FormatHistoryOdometer(latest.odometer) "."
    }
    return summary
}

FindVehicleHistoryEventById(eventId) {
    global VehicleHistory

    for event in VehicleHistory {
        if (event.id = eventId) {
            return event
        }
    }

    return ""
}

FindVehicleHistoryEventIndexById(eventId) {
    global VehicleHistory

    for index, event in VehicleHistory {
        if (event.id = eventId) {
            return index
        }
    }

    return 0
}

GenerateHistoryEventId() {
    return "hist_" A_Now "_" Random(1000, 9999)
}

LoadVehicleFuelLog() {
    global AppTitle, VehicleFuelLog, FuelLogFile

    VehicleFuelLog := []
    if !FileExist(FuelLogFile) {
        return
    }

    content := FileRead(FuelLogFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap fuel v1") {
        MsgBox("Soubor kilometrů a tankování není v podporovaném formátu.`n`nZkontrolujte soubor:`n" FuelLogFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor kilometrů a tankování obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" FuelLogFile, AppTitle, 0x30)
            VehicleFuelLog := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 9) {
            MsgBox("Soubor kilometrů a tankování je poškozený. Řádek " index " musí obsahovat přesně 9 polí oddělených tabulátory.`n`nZkontrolujte soubor:`n" FuelLogFile, AppTitle, 0x30)
            VehicleFuelLog := []
            return
        }

        VehicleFuelLog.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            entryDate: UnescapeField(fields[3]),
            odometer: UnescapeField(fields[4]),
            liters: UnescapeField(fields[5]),
            totalCost: UnescapeField(fields[6]),
            fullTank: (UnescapeField(fields[7]) = "1") ? 1 : 0,
            fuelType: UnescapeField(fields[8]),
            note: UnescapeField(fields[9])
        })
    }
}

SaveVehicleFuelLog() {
    global FuelLogFile

    WriteTextFileUtf8(FuelLogFile, BuildFuelDataContent())
}

BuildFuelDataContent() {
    global VehicleFuelLog

    lines := ["# Vehimap fuel v1"]
    for entry in VehicleFuelLog {
        lines.Push(
            EscapeField(entry.id) "`t"
            EscapeField(entry.vehicleId) "`t"
            EscapeField(entry.entryDate) "`t"
            EscapeField(entry.odometer) "`t"
            EscapeField(entry.liters) "`t"
            EscapeField(entry.totalCost) "`t"
            EscapeField(entry.fullTank ? "1" : "0") "`t"
            EscapeField(entry.fuelType) "`t"
            EscapeField(entry.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleFuelEntries(vehicleId) {
    global VehicleFuelLog

    filtered := []
    changed := false
    for entry in VehicleFuelLog {
        if (entry.vehicleId = vehicleId) {
            changed := true
        } else {
            filtered.Push(entry)
        }
    }

    VehicleFuelLog := filtered
    if changed {
        SaveVehicleFuelLog()
    }
}

GetVehicleFuelEntries(vehicleId) {
    global VehicleFuelLog

    entries := []
    for entry in VehicleFuelLog {
        if (entry.vehicleId = vehicleId) {
            entries.Push(entry)
        }
    }

    SortVehicleFuelEntries(&entries)
    return entries
}

GetVehicleFuelEntryCount(vehicleId) {
    count := 0
    global VehicleFuelLog

    for entry in VehicleFuelLog {
        if (entry.vehicleId = vehicleId) {
            count += 1
        }
    }

    return count
}

BuildVehicleFuelSummaryText(vehicleId) {
    entries := GetVehicleFuelEntries(vehicleId)
    if (entries.Length = 0) {
        return "K tomuto vozidlu zatím nejsou uloženy žádné záznamy kilometrů ani tankování."
    }

    summary := "Záznamů: " entries.Length ". Poslední tachometr: " FormatHistoryOdometer(entries[1].odometer) "."
    latestFuelEntry := ""
    for entry in entries {
        if (entry.liters != "" || entry.totalCost != "") {
            latestFuelEntry := entry
            break
        }
    }

    if IsObject(latestFuelEntry) {
        summary .= " Poslední tankování: "
        if (latestFuelEntry.liters != "") {
            summary .= FormatFuelLiters(latestFuelEntry.liters)
        } else {
            summary .= "bez údajů o litrech"
        }
        if (latestFuelEntry.totalCost != "") {
            summary .= " za " FormatFuelMoney(latestFuelEntry.totalCost)
        }
        summary .= " (" latestFuelEntry.entryDate ")."
    }

    return summary
}

FindVehicleFuelEntryById(entryId) {
    global VehicleFuelLog

    for entry in VehicleFuelLog {
        if (entry.id = entryId) {
            return entry
        }
    }

    return ""
}

FindVehicleFuelEntryIndexById(entryId) {
    global VehicleFuelLog

    for index, entry in VehicleFuelLog {
        if (entry.id = entryId) {
            return index
        }
    }

    return 0
}

GenerateFuelEntryId() {
    return "fuel_" A_Now "_" Random(1000, 9999)
}

SortVehicleFuelEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleFuelEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleFuelEntries(left, right) {
    leftStamp := ParseEventDateStamp(left.entryDate)
    rightStamp := ParseEventDateStamp(right.entryDate)

    if (leftStamp > rightStamp) {
        return -1
    }
    if (leftStamp < rightStamp) {
        return 1
    }

    leftOdometer := (Trim(left.odometer) = "") ? -1 : left.odometer + 0
    rightOdometer := (Trim(right.odometer) = "") ? -1 : right.odometer + 0
    if (leftOdometer > rightOdometer) {
        return -1
    }
    if (leftOdometer < rightOdometer) {
        return 1
    }

    return CompareTextValues(left.id, right.id)
}

NormalizeDecimalText(value) {
    value := Trim(StrReplace(value, " ", ""))
    if (value = "") {
        return ""
    }

    value := StrReplace(value, ".", ",")
    if !RegExMatch(value, "^\d+(,\d+)?$") {
        return ""
    }

    parts := StrSplit(value, ",")
    integerPart := RegExReplace(parts[1], "^0+(?=\d)", "")
    if (integerPart = "") {
        integerPart := "0"
    }

    if (parts.Length = 1) {
        return integerPart
    }

    decimalPart := RegExReplace(parts[2], "0+$")
    if (decimalPart = "") {
        return integerPart
    }

    return integerPart "," decimalPart
}

FormatFuelLiters(value) {
    value := Trim(StrReplace(value, ".", ","))
    if (value = "") {
        return ""
    }

    return value " l"
}

FormatFuelMoney(value) {
    value := Trim(StrReplace(value, ".", ","))
    if (value = "") {
        return ""
    }

    return value " Kč"
}

GetLatestVehicleOdometerText(vehicleId) {
    global VehicleHistory, VehicleFuelLog

    bestStamp := ""
    bestOdometer := ""
    bestOdometerValue := -1

    for entry in VehicleFuelLog {
        if (entry.vehicleId != vehicleId || Trim(entry.odometer) = "") {
            continue
        }

        stamp := ParseEventDateStamp(entry.entryDate)
        odometerValue := entry.odometer + 0
        if (bestStamp = "" || stamp > bestStamp || (stamp = bestStamp && odometerValue > bestOdometerValue)) {
            bestStamp := stamp
            bestOdometer := entry.odometer
            bestOdometerValue := odometerValue
        }
    }

    for event in VehicleHistory {
        if (event.vehicleId != vehicleId || Trim(event.odometer) = "") {
            continue
        }

        stamp := ParseEventDateStamp(event.eventDate)
        odometerValue := event.odometer + 0
        if (bestStamp = "" || stamp > bestStamp || (stamp = bestStamp && odometerValue > bestOdometerValue)) {
            bestStamp := stamp
            bestOdometer := event.odometer
            bestOdometerValue := odometerValue
        }
    }

    return bestOdometer
}

LoadVehicleRecords() {
    global AppTitle, VehicleRecords, RecordsFile

    VehicleRecords := []
    if !FileExist(RecordsFile) {
        return
    }

    content := FileRead(RecordsFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap records v1") {
        MsgBox("Soubor pojištění a dokladů není v podporovaném formátu.`n`nZkontrolujte soubor:`n" RecordsFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor pojištění a dokladů obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" RecordsFile, AppTitle, 0x30)
            VehicleRecords := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 10) {
            MsgBox("Soubor pojištění a dokladů je poškozený. Řádek " index " musí obsahovat přesně 10 polí oddělených tabulátory.`n`nZkontrolujte soubor:`n" RecordsFile, AppTitle, 0x30)
            VehicleRecords := []
            return
        }

        VehicleRecords.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            recordType: UnescapeField(fields[3]),
            title: UnescapeField(fields[4]),
            provider: UnescapeField(fields[5]),
            validFrom: UnescapeField(fields[6]),
            validTo: UnescapeField(fields[7]),
            price: UnescapeField(fields[8]),
            filePath: UnescapeField(fields[9]),
            note: UnescapeField(fields[10])
        })
    }
}

SaveVehicleRecords() {
    global RecordsFile

    WriteTextFileUtf8(RecordsFile, BuildRecordsDataContent())
}

BuildRecordsDataContent() {
    global VehicleRecords

    lines := ["# Vehimap records v1"]
    for entry in VehicleRecords {
        lines.Push(
            EscapeField(entry.id) "`t"
            EscapeField(entry.vehicleId) "`t"
            EscapeField(entry.recordType) "`t"
            EscapeField(entry.title) "`t"
            EscapeField(entry.provider) "`t"
            EscapeField(entry.validFrom) "`t"
            EscapeField(entry.validTo) "`t"
            EscapeField(entry.price) "`t"
            EscapeField(entry.filePath) "`t"
            EscapeField(entry.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleRecords(vehicleId) {
    global VehicleRecords

    filtered := []
    changed := false
    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId) {
            changed := true
        } else {
            filtered.Push(entry)
        }
    }

    VehicleRecords := filtered
    if changed {
        SaveVehicleRecords()
    }
}

GetVehicleRecords(vehicleId) {
    global VehicleRecords

    entries := []
    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId) {
            entries.Push(entry)
        }
    }

    SortVehicleRecords(&entries)
    return entries
}

GetVehicleRecordCount(vehicleId) {
    count := 0
    global VehicleRecords

    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId) {
            count += 1
        }
    }

    return count
}

BuildVehicleRecordsSummaryText(vehicleId) {
    entries := GetVehicleRecords(vehicleId)
    if (entries.Length = 0) {
        return "K tomuto vozidlu zatím není uložen žádný záznam pojištění ani dokladů."
    }

    summary := "Záznamů: " entries.Length "."
    missingPathCount := 0
    emptyPathCount := 0
    for entry in entries {
        pathKind := GetVehicleRecordPathInfo(entry).kind
        if (pathKind = "missing_file" || pathKind = "missing_folder") {
            missingPathCount += 1
        } else if (pathKind = "empty") {
            emptyPathCount += 1
        }
    }

    nearestRecord := ""
    for entry in entries {
        if (Trim(entry.validTo) != "") {
            nearestRecord := entry
            break
        }
    }

    if IsObject(nearestRecord) {
        summary .= " Nejbližší platnost: " nearestRecord.title
        if (nearestRecord.provider != "") {
            summary .= " (" nearestRecord.provider ")"
        }
        summary .= " do " nearestRecord.validTo "."
    } else {
        summary .= " U žádného záznamu není vyplněné datum platnosti."
    }

    if (missingPathCount > 0) {
        summary .= " Nedostupných cest: " missingPathCount "."
    }
    if (emptyPathCount > 0) {
        summary .= " Bez vyplněné cesty: " emptyPathCount "."
    }

    return summary
}

FindVehicleRecordById(entryId) {
    global VehicleRecords

    for entry in VehicleRecords {
        if (entry.id = entryId) {
            return entry
        }
    }

    return ""
}

FindVehicleRecordIndexById(entryId) {
    global VehicleRecords

    for index, entry in VehicleRecords {
        if (entry.id = entryId) {
            return index
        }
    }

    return 0
}

GenerateVehicleRecordId() {
    return "record_" A_Now "_" Random(1000, 9999)
}

SortVehicleRecords(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleRecordEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleRecordEntries(left, right) {
    leftKey := ParseDueStamp(left.validTo)
    rightKey := ParseDueStamp(right.validTo)

    if (leftKey = "") {
        leftKey := "99999999999999"
    }
    if (rightKey = "") {
        rightKey := "99999999999999"
    }

    if (leftKey < rightKey) {
        return -1
    }
    if (leftKey > rightKey) {
        return 1
    }

    result := CompareTextValues(left.recordType, right.recordType)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.title, right.title)
}

GetFileNameFromPath(path) {
    path := Trim(path)
    if (path = "") {
        return ""
    }

    SplitPath(path, &fileName)
    return (fileName = "") ? path : fileName
}

LoadVehicleMeta() {
    global AppTitle, VehicleMetaEntries, VehicleMetaFile

    VehicleMetaEntries := []
    if !FileExist(VehicleMetaFile) {
        return
    }

    content := FileRead(VehicleMetaFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap meta v1") {
        MsgBox("Soubor stavů a štítků vozidel není v podporovaném formátu.`n`nZkontrolujte soubor:`n" VehicleMetaFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor stavů a štítků vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" VehicleMetaFile, AppTitle, 0x30)
            VehicleMetaEntries := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 3) {
            MsgBox("Soubor stavů a štítků vozidel je poškozený. Řádek " index " musí obsahovat přesně 3 pole oddělená tabulátory.`n`nZkontrolujte soubor:`n" VehicleMetaFile, AppTitle, 0x30)
            VehicleMetaEntries := []
            return
        }

        VehicleMetaEntries.Push({
            vehicleId: UnescapeField(fields[1]),
            state: UnescapeField(fields[2]),
            tags: UnescapeField(fields[3])
        })
    }
}

SaveVehicleMeta() {
    global VehicleMetaFile

    WriteTextFileUtf8(VehicleMetaFile, BuildVehicleMetaDataContent())
}

BuildVehicleMetaDataContent() {
    global VehicleMetaEntries

    lines := ["# Vehimap meta v1"]
    for entry in VehicleMetaEntries {
        lines.Push(
            EscapeField(entry.vehicleId) "`t"
            EscapeField(entry.state) "`t"
            EscapeField(entry.tags)
        )
    }

    return JoinLines(lines)
}

GetVehicleMeta(vehicleId) {
    global VehicleMetaEntries

    for entry in VehicleMetaEntries {
        if (entry.vehicleId = vehicleId) {
            return entry
        }
    }

    return {
        vehicleId: vehicleId,
        state: "",
        tags: ""
    }
}

SaveVehicleMetaEntry(vehicleId, state := "", tags := "") {
    global VehicleMetaEntries

    state := NormalizeVehicleState(state)
    tags := NormalizeTagList(tags)
    index := FindVehicleMetaIndex(vehicleId)

    if (state = "" && tags = "") {
        if index {
            VehicleMetaEntries.RemoveAt(index)
            SaveVehicleMeta()
        }
        return
    }

    entry := {
        vehicleId: vehicleId,
        state: state,
        tags: tags
    }

    if index {
        VehicleMetaEntries[index] := entry
    } else {
        VehicleMetaEntries.Push(entry)
    }

    SaveVehicleMeta()
}

FindVehicleMetaIndex(vehicleId) {
    global VehicleMetaEntries

    for index, entry in VehicleMetaEntries {
        if (entry.vehicleId = vehicleId) {
            return index
        }
    }

    return 0
}

DeleteVehicleMeta(vehicleId) {
    global VehicleMetaEntries

    index := FindVehicleMetaIndex(vehicleId)
    if !index {
        return
    }

    VehicleMetaEntries.RemoveAt(index)
    SaveVehicleMeta()
}

NormalizeVehicleState(state) {
    global VehicleStateOptions

    state := Trim(state)
    if (state = "") {
        return ""
    }

    for item in VehicleStateOptions {
        if (item = state) {
            return item
        }
    }

    return state
}

NormalizeTagList(tags) {
    tags := Trim(tags)
    if (tags = "") {
        return ""
    }

    tags := StrReplace(tags, ";", ",")
    rawItems := StrSplit(tags, ",")
    normalized := []
    seen := Map()

    for item in rawItems {
        cleanItem := Trim(item)
        if (cleanItem = "") {
            continue
        }

        key := StrLower(cleanItem)
        if seen.Has(key) {
            continue
        }

        seen[key] := true
        normalized.Push(cleanItem)
    }

    return JoinInline(normalized, ", ")
}

LoadVehicleReminders() {
    global AppTitle, VehicleReminders, RemindersFile

    VehicleReminders := []
    if !FileExist(RemindersFile) {
        return
    }

    content := FileRead(RemindersFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap reminders v1" && firstNonEmptyLine != "# Vehimap reminders v2") {
        MsgBox("Soubor vlastních připomínek není v podporovaném formátu.`n`nZkontrolujte soubor:`n" RemindersFile, AppTitle, 0x30)
        return
    }

    isV2 := (firstNonEmptyLine = "# Vehimap reminders v2")

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor vlastních připomínek obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" RemindersFile, AppTitle, 0x30)
            VehicleReminders := []
            return
        }

        fields := StrSplit(line, "`t")
        expectedFieldCount := isV2 ? 7 : 6
        if (fields.Length != expectedFieldCount) {
            MsgBox("Soubor vlastních připomínek je poškozený. Řádek " index " musí obsahovat přesně " expectedFieldCount " polí oddělených tabulátory.`n`nZkontrolujte soubor:`n" RemindersFile, AppTitle, 0x30)
            VehicleReminders := []
            return
        }

        reminderDays := UnescapeField(fields[5])
        if !RegExMatch(reminderDays, "^\d{1,3}$") {
            reminderDays := "30"
        }

        VehicleReminders.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            title: UnescapeField(fields[3]),
            dueDate: UnescapeField(fields[4]),
            reminderDays: reminderDays,
            repeatMode: isV2 ? NormalizeReminderRepeat(UnescapeField(fields[6])) : "Neopakovat",
            note: isV2 ? UnescapeField(fields[7]) : UnescapeField(fields[6])
        })
    }
}

SaveVehicleReminders() {
    global RemindersFile

    WriteTextFileUtf8(RemindersFile, BuildVehicleRemindersDataContent())
}

BuildVehicleRemindersDataContent() {
    global VehicleReminders

    lines := ["# Vehimap reminders v2"]
    for entry in VehicleReminders {
        lines.Push(
            EscapeField(entry.id) "`t"
            EscapeField(entry.vehicleId) "`t"
            EscapeField(entry.title) "`t"
            EscapeField(entry.dueDate) "`t"
            EscapeField(entry.reminderDays) "`t"
            EscapeField(GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : "")) "`t"
            EscapeField(entry.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleReminders(vehicleId) {
    global VehicleReminders

    filtered := []
    changed := false
    for entry in VehicleReminders {
        if (entry.vehicleId = vehicleId) {
            changed := true
        } else {
            filtered.Push(entry)
        }
    }

    VehicleReminders := filtered
    if changed {
        SaveVehicleReminders()
    }
}

GetVehicleReminderEntries(vehicleId) {
    global VehicleReminders

    entries := []
    for entry in VehicleReminders {
        if (entry.vehicleId = vehicleId) {
            entries.Push(entry)
        }
    }

    SortVehicleReminderEntries(&entries)
    return entries
}

GetVehicleReminderCount(vehicleId) {
    count := 0
    global VehicleReminders

    for entry in VehicleReminders {
        if (entry.vehicleId = vehicleId) {
            count += 1
        }
    }

    return count
}

BuildVehicleReminderSummaryText(vehicleId) {
    entries := GetVehicleReminderEntries(vehicleId)
    if (entries.Length = 0) {
        return "K tomuto vozidlu zatím nejsou uloženy žádné vlastní připomínky."
    }

    nearest := entries[1]
    status := GetReminderExpirationStatusText(nearest.dueDate, nearest.reminderDays + 0)
    summary := "Připomínek: " entries.Length ". Nejbližší: " nearest.title " (" nearest.dueDate
    if (status != "") {
        summary .= ", " status
    }
    repeatLabel := GetReminderRepeatLabel(nearest.HasOwnProp("repeatMode") ? nearest.repeatMode : "")
    if (repeatLabel != "Neopakovat") {
        summary .= ", " repeatLabel
    }
    summary .= ")."
    return summary
}

FindVehicleReminderById(entryId) {
    global VehicleReminders

    for entry in VehicleReminders {
        if (entry.id = entryId) {
            return entry
        }
    }

    return ""
}

FindVehicleReminderIndexById(entryId) {
    global VehicleReminders

    for index, entry in VehicleReminders {
        if (entry.id = entryId) {
            return index
        }
    }

    return 0
}

GenerateVehicleReminderId() {
    return "rem_" A_Now "_" Random(1000, 9999)
}

NormalizeReminderRepeat(repeatMode) {
    global ReminderRepeatOptions

    repeatMode := Trim(repeatMode)
    if (repeatMode = "") {
        return "Neopakovat"
    }

    for option in ReminderRepeatOptions {
        if (option = repeatMode) {
            return option
        }
    }

    return "Neopakovat"
}

GetReminderRepeatLabel(repeatMode) {
    return NormalizeReminderRepeat(repeatMode)
}

GetReminderRepeatYears(repeatMode) {
    repeatMode := NormalizeReminderRepeat(repeatMode)

    switch repeatMode {
        case "Každý rok":
            return 1
        case "Každé 2 roky":
            return 2
        case "Každých 5 let":
            return 5
        default:
            return 0
    }
}

AddYearsToEventDate(eventDate, yearsToAdd) {
    normalized := NormalizeEventDate(eventDate)
    if (normalized = "" || yearsToAdd = 0) {
        return normalized
    }

    parts := StrSplit(normalized, ".")
    day := parts[1] + 0
    month := parts[2] + 0
    year := (parts[3] + 0) + yearsToAdd
    maxDay := DaysInMonth(year, month)
    if (day > maxDay) {
        day := maxDay
    }

    return Format("{:02}.{:02}.{:04}", day, month, year)
}

SortVehicleReminderEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleReminderEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleReminderEntries(left, right) {
    leftStamp := ParseReminderDueStamp(left.dueDate)
    rightStamp := ParseReminderDueStamp(right.dueDate)

    if (leftStamp = "") {
        leftStamp := "99999999999999"
    }
    if (rightStamp = "") {
        rightStamp := "99999999999999"
    }

    if (leftStamp < rightStamp) {
        return -1
    }
    if (leftStamp > rightStamp) {
        return 1
    }

    return CompareTextValues(left.title, right.title)
}

ParseReminderDueStamp(reminderDate) {
    normalized := NormalizeEventDate(reminderDate)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, ".")
    return parts[3] parts[2] parts[1] "235959"
}

GetReminderExpirationStatusText(reminderDate, reminderDays := 30) {
    dueStamp := ParseReminderDueStamp(reminderDate)
    if (dueStamp = "") {
        return ""
    }

    if (dueStamp < A_Now) {
        return "Po termínu"
    }

    cutoff := DateAdd(A_Now, reminderDays, "Days")
    if (dueStamp <= cutoff) {
        daysLeft := DateDiff(dueStamp, A_Now, "Days")
        if (daysLeft < 1) {
            return "Dnes"
        }
        return "Do " daysLeft " dnů"
    }

    return ""
}

GetUpcomingCustomReminders(vehicleId := "") {
    global VehicleReminders

    upcoming := []
    for entry in VehicleReminders {
        if (vehicleId != "" && entry.vehicleId != vehicleId) {
            continue
        }

        vehicle := FindVehicleById(entry.vehicleId)
        if !IsObject(vehicle) {
            continue
        }

        dueStamp := ParseReminderDueStamp(entry.dueDate)
        if (dueStamp = "") {
            continue
        }

        reminderDays := entry.reminderDays + 0
        cutoff := DateAdd(A_Now, reminderDays, "Days")
        if (dueStamp <= cutoff) {
            upcoming.Push({
                kind: "custom",
                vehicle: vehicle,
                reminder: entry,
                dueStamp: dueStamp
            })
        }
    }

    SortUpcomingByDue(&upcoming)
    return upcoming
}

GetVehicleReminderStateText(vehicleId) {
    upcoming := GetUpcomingCustomReminders(vehicleId)
    if (upcoming.Length = 0) {
        return ""
    }

    entry := upcoming[1].reminder
    status := GetReminderExpirationStatusText(entry.dueDate, entry.reminderDays + 0)
    if (status = "") {
        return ""
    }

    return "Př: " status
}

NormalizeEventDate(value) {
    value := Trim(value)
    if (value = "") {
        return ""
    }

    if !RegExMatch(value, "^\s*(\d{1,2})\s*[./-]\s*(\d{1,2})\s*[./-]\s*(\d{4})\s*$", &match) {
        return ""
    }

    day := match[1] + 0
    month := match[2] + 0
    year := match[3] + 0
    if (day < 1 || day > 31 || month < 1 || month > 12 || year < 1900 || year > 2200) {
        return ""
    }

    stamp := Format("{:04}{:02}{:02}", year, month, day)
    if !IsValidDateStamp(stamp) {
        return ""
    }

    return Format("{:02}.{:02}.{:04}", day, month, year)
}

ParseEventDateStamp(eventDate) {
    normalized := NormalizeEventDate(eventDate)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, ".")
    return parts[3] parts[2] parts[1] "000000"
}

IsValidDateStamp(stamp) {
    try {
        DateDiff(stamp, stamp, "Days")
        return true
    } catch {
        return false
    }
}

NormalizeOdometerText(value) {
    value := Trim(StrReplace(value, " ", ""))
    if (value = "") {
        return ""
    }

    return RegExMatch(value, "^\d+$") ? value : ""
}

FormatHistoryOdometer(value) {
    value := Trim(value)
    if (value = "") {
        return ""
    }

    return value " km"
}

SortVehicleHistoryByDateDescending(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        swapped := false
        Loop count - A_Index {
            left := items[A_Index]
            right := items[A_Index + 1]
            leftStamp := ParseEventDateStamp(left.eventDate)
            rightStamp := ParseEventDateStamp(right.eventDate)
            if (leftStamp < rightStamp) {
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

FormatDisplayValue(value, emptyText := "Nevyplněno") {
    value := Trim(value)
    return (value = "") ? emptyText : value
}

BuildGreenCardRangeText(vehicle) {
    if (vehicle.greenCardFrom = "" && vehicle.greenCardTo = "") {
        return "Nevyplněno"
    }

    return FormatDisplayValue(vehicle.greenCardFrom, "od nevyplněno") " až " FormatDisplayValue(vehicle.greenCardTo, "do nevyplněno")
}

BuildVehicleDetailStatusText(vehicle) {
    status := GetVehicleStatusText(vehicle)
    return (status = "") ? "V pořádku" : status
}

BuildVehiclesDataContent() {
    global Vehicles

    lines := ["# Vehimap data v3"]
    for vehicle in Vehicles {
        lines.Push(
            EscapeField(vehicle.id) "`t"
            EscapeField(vehicle.name) "`t"
            EscapeField(vehicle.category) "`t"
            EscapeField(vehicle.vehicleType) "`t"
            EscapeField(vehicle.makeModel) "`t"
            EscapeField(vehicle.plate) "`t"
            EscapeField(vehicle.year) "`t"
            EscapeField(vehicle.power) "`t"
            EscapeField(vehicle.lastTk) "`t"
            EscapeField(vehicle.nextTk) "`t"
            EscapeField(vehicle.greenCardFrom) "`t"
            EscapeField(vehicle.greenCardTo)
        )
    }

    return JoinLines(lines)
}

NormalizeTextForStorage(text) {
    text := StrReplace(text, Chr(0xFEFF))
    text := StrReplace(text, "`r`n", "`n")
    text := StrReplace(text, "`r", "`n")
    return text
}

WriteTextFileUtf8(path, content) {
    if FileExist(path) {
        FileDelete(path)
    }

    FileAppend(content, path, "UTF-8")
}

GetSettingsContentForBackup() {
    global SettingsFile

    EnsureSettingsDefaults()
    IniWrite(GetRunAtStartupEnabled() ? "1" : "0", SettingsFile, "app", "run_at_startup")
    return FileExist(SettingsFile) ? NormalizeTextForStorage(FileRead(SettingsFile, "UTF-8")) : ""
}

GetVehiclesContentForBackup() {
    global VehiclesFile

    if FileExist(VehiclesFile) {
        return NormalizeTextForStorage(FileRead(VehiclesFile, "UTF-8"))
    }

    return BuildVehiclesDataContent()
}

GetHistoryContentForBackup() {
    global HistoryFile

    if FileExist(HistoryFile) {
        return NormalizeTextForStorage(FileRead(HistoryFile, "UTF-8"))
    }

    return BuildHistoryDataContent()
}

GetFuelContentForBackup() {
    global FuelLogFile

    if FileExist(FuelLogFile) {
        return NormalizeTextForStorage(FileRead(FuelLogFile, "UTF-8"))
    }

    return BuildFuelDataContent()
}

GetRecordsContentForBackup() {
    global RecordsFile

    if FileExist(RecordsFile) {
        return NormalizeTextForStorage(FileRead(RecordsFile, "UTF-8"))
    }

    return BuildRecordsDataContent()
}

GetVehicleMetaContentForBackup() {
    global VehicleMetaFile

    if FileExist(VehicleMetaFile) {
        return NormalizeTextForStorage(FileRead(VehicleMetaFile, "UTF-8"))
    }

    return BuildVehicleMetaDataContent()
}

GetRemindersContentForBackup() {
    global RemindersFile

    if FileExist(RemindersFile) {
        return NormalizeTextForStorage(FileRead(RemindersFile, "UTF-8"))
    }

    return BuildVehicleRemindersDataContent()
}

BuildCurrentBackupContent() {
    return BuildBackupContent(
        GetSettingsContentForBackup(),
        BuildVehiclesDataContent(),
        BuildHistoryDataContent(),
        BuildFuelDataContent(),
        BuildRecordsDataContent(),
        BuildVehicleMetaDataContent(),
        BuildVehicleRemindersDataContent()
    )
}

GetAutomaticBackupDirectory() {
    global DataDir

    return DataDir "\auto-backups"
}

EnsureAutomaticBackupDirectory() {
    backupDir := GetAutomaticBackupDirectory()
    if !InStr(FileExist(backupDir), "D") {
        DirCreate(backupDir)
    }
    return backupDir
}

GetAutomaticBackupPath() {
    backupDir := EnsureAutomaticBackupDirectory()
    timestamp := FormatTime(A_Now, "yyyy-MM-dd_HH-mm-ss")
    return backupDir "\Vehimap_auto_" timestamp ".vehimapbak"
}

GetAutomaticBackupLastStamp() {
    global SettingsFile

    stamp := Trim(IniRead(SettingsFile, "backups", "last_automatic_backup_stamp", ""))
    return RegExMatch(stamp, "^\d{14}$") ? stamp : ""
}

GetAutomaticBackupLastPath() {
    global SettingsFile

    return Trim(IniRead(SettingsFile, "backups", "last_automatic_backup_path", ""))
}

FormatAutomaticBackupStamp(stamp) {
    return RegExMatch(stamp, "^\d{14}$") ? FormatTime(stamp, "dd.MM.yyyy HH:mm") : "zatím nebyla vytvořena"
}

BuildAutomaticBackupStatusText() {
    backupDirLabel := "data\auto-backups"
    lastStamp := GetAutomaticBackupLastStamp()
    lastPath := GetAutomaticBackupLastPath()
    if (lastStamp = "") {
        return "Automatické zálohy se ukládají do složky " backupDirLabel ". Poslední záloha v této složce zatím nebyla vytvořena."
    }

    status := "Automatické zálohy se ukládají do složky " backupDirLabel ". Poslední záloha v této složce: " FormatAutomaticBackupStamp(lastStamp) "."
    if (lastPath != "" && FileExist(lastPath)) {
        status .= " Soubor je uložen pod názvem " SubStr(lastPath, InStr(lastPath, "\",, -1) + 1) "."
    }
    return status
}

IsAutomaticBackupDue() {
    if !GetAutomaticBackupsEnabled() {
        return false
    }

    lastStamp := GetAutomaticBackupLastStamp()
    if (lastStamp = "") {
        return true
    }

    intervalDays := GetAutomaticBackupIntervalDays()
    currentDateStamp := SubStr(A_Now, 1, 8) "000000"
    lastDateStamp := SubStr(lastStamp, 1, 8) "000000"
    return DateDiff(currentDateStamp, lastDateStamp, "Days") >= intervalDays
}

RunAutomaticBackupCheck(force := false, showErrorMessage := false) {
    if !force && !GetAutomaticBackupsEnabled() {
        return ""
    }

    if !force && !IsAutomaticBackupDue() {
        if GetAutomaticBackupsEnabled() {
            TrimAutomaticBackupFiles()
        }
        return ""
    }

    return CreateAutomaticBackup(showErrorMessage)
}

CreateAutomaticBackup(showErrorMessage := false) {
    global AppTitle, SettingsFile

    try {
        backupPath := GetAutomaticBackupPath()
        WriteTextFileUtf8(backupPath, BuildCurrentBackupContent())
        IniWrite(A_Now, SettingsFile, "backups", "last_automatic_backup_stamp")
        IniWrite(backupPath, SettingsFile, "backups", "last_automatic_backup_path")
        TrimAutomaticBackupFiles()
        return backupPath
    } catch as err {
        if showErrorMessage {
            MsgBox("Automatická záloha se nepodařila.`n`n" err.Message, AppTitle, 0x30)
        } else {
            TrayTip("Automatická záloha se nepodařila. " ShortenText(err.Message, 110), AppTitle)
        }
        return ""
    }
}

TrimAutomaticBackupFiles() {
    backupDir := GetAutomaticBackupDirectory()
    if !InStr(FileExist(backupDir), "D") {
        return
    }

    keepCount := GetAutomaticBackupKeepCount()
    files := []
    Loop Files backupDir "\*.vehimapbak", "F" {
        files.Push(A_LoopFileFullPath)
    }

    SortTextItemsDescending(&files)
    while (files.Length > keepCount) {
        try FileDelete(files.Pop())
    }
}

GetDefaultBackupPath() {
    timestamp := FormatTime(A_Now, "yyyy-MM-dd_HH-mm")
    return A_ScriptDir "\Vehimap_zaloha_" timestamp ".vehimapbak"
}

EnsureBackupExtension(path) {
    if (StrLower(SubStr(path, -10)) != ".vehimapbak") {
        path .= ".vehimapbak"
    }

    return path
}

BuildBackupContent(settingsContent, vehiclesContent, historyContent := "", fuelContent := "", recordsContent := "", metaContent := "", remindersContent := "") {
    settingsContent := NormalizeTextForStorage(settingsContent)
    vehiclesContent := NormalizeTextForStorage(vehiclesContent)
    historyContent := NormalizeTextForStorage(historyContent)
    fuelContent := NormalizeTextForStorage(fuelContent)
    recordsContent := NormalizeTextForStorage(recordsContent)
    metaContent := NormalizeTextForStorage(metaContent)
    remindersContent := NormalizeTextForStorage(remindersContent)

    header := JoinLines([
        "# Vehimap backup v4",
        "settings_length=" StrLen(settingsContent),
        "vehicles_length=" StrLen(vehiclesContent),
        "history_length=" StrLen(historyContent),
        "fuel_length=" StrLen(fuelContent),
        "records_length=" StrLen(recordsContent),
        "meta_length=" StrLen(metaContent),
        "reminders_length=" StrLen(remindersContent)
    ])

    return header "`n`n" settingsContent vehiclesContent historyContent fuelContent recordsContent metaContent remindersContent
}

TryParseBackupContent(content, &settingsContent, &vehiclesContent, &historyContent, &fuelContent, &recordsContent, &metaContent, &remindersContent, &errorMessage) {
    settingsContent := ""
    vehiclesContent := ""
    historyContent := ""
    fuelContent := ""
    recordsContent := ""
    metaContent := ""
    remindersContent := ""
    errorMessage := ""
    content := NormalizeTextForStorage(content)

    delimiterPos := InStr(content, "`n`n")
    if !delimiterPos {
        errorMessage := "Soubor zálohy nemá platnou hlavičku."
        return false
    }

    header := SubStr(content, 1, delimiterPos - 1)
    payload := SubStr(content, delimiterPos + 2)
    headerLines := StrSplit(header, "`n")
    if (headerLines.Length < 3) {
        errorMessage := "Soubor není ve formátu zálohy Vehimap."
        return false
    }

    backupVersion := headerLines[1]
    if (backupVersion != "# Vehimap backup v1" && backupVersion != "# Vehimap backup v2" && backupVersion != "# Vehimap backup v3" && backupVersion != "# Vehimap backup v4") {
        errorMessage := "Soubor není ve formátu zálohy Vehimap."
        return false
    }

    if !RegExMatch(headerLines[2], "^settings_length=(\d+)$", &settingsMatch) {
        errorMessage := "Soubor zálohy neobsahuje délku nastavení."
        return false
    }

    if !RegExMatch(headerLines[3], "^vehicles_length=(\d+)$", &vehiclesMatch) {
        errorMessage := "Soubor zálohy neobsahuje délku dat vozidel."
        return false
    }

    settingsLength := settingsMatch[1] + 0
    vehiclesLength := vehiclesMatch[1] + 0
    historyLength := 0
    fuelLength := 0
    recordsLength := 0
    metaLength := 0
    remindersLength := 0
    if (backupVersion = "# Vehimap backup v2" || backupVersion = "# Vehimap backup v3") {
        if (headerLines.Length < 4 || !RegExMatch(headerLines[4], "^history_length=(\d+)$", &historyMatch)) {
            errorMessage := "Soubor zálohy neobsahuje délku historie."
            return false
        }
        historyLength := historyMatch[1] + 0
    }

    if (backupVersion = "# Vehimap backup v3" || backupVersion = "# Vehimap backup v4") {
        if (headerLines.Length < 6 || !RegExMatch(headerLines[5], "^fuel_length=(\d+)$", &fuelMatch) || !RegExMatch(headerLines[6], "^records_length=(\d+)$", &recordsMatch)) {
            errorMessage := "Soubor zálohy neobsahuje délky kilometrů a dokladů."
            return false
        }
        fuelLength := fuelMatch[1] + 0
        recordsLength := recordsMatch[1] + 0
    }

    if (backupVersion = "# Vehimap backup v4") {
        if (headerLines.Length < 8 || !RegExMatch(headerLines[7], "^meta_length=(\d+)$", &metaMatch) || !RegExMatch(headerLines[8], "^reminders_length=(\d+)$", &remindersMatch)) {
            errorMessage := "Soubor zálohy neobsahuje délky stavů a připomínek."
            return false
        }
        metaLength := metaMatch[1] + 0
        remindersLength := remindersMatch[1] + 0
    }

    if (settingsLength < 0 || vehiclesLength < 0 || historyLength < 0 || fuelLength < 0 || recordsLength < 0 || metaLength < 0 || remindersLength < 0) {
        errorMessage := "Soubor zálohy obsahuje neplatné délky dat."
        return false
    }

    if (StrLen(payload) != settingsLength + vehiclesLength + historyLength + fuelLength + recordsLength + metaLength + remindersLength) {
        errorMessage := "Soubor zálohy je neúplný nebo poškozený."
        return false
    }

    settingsContent := SubStr(payload, 1, settingsLength)
    vehiclesContent := SubStr(payload, settingsLength + 1, vehiclesLength)
    payloadOffset := settingsLength + vehiclesLength + 1

    if (backupVersion = "# Vehimap backup v2" || backupVersion = "# Vehimap backup v3") {
        historyContent := SubStr(payload, payloadOffset, historyLength)
        payloadOffset += historyLength
    } else {
        historyContent := "# Vehimap history v1`n"
    }

    if (backupVersion = "# Vehimap backup v3" || backupVersion = "# Vehimap backup v4") {
        fuelContent := SubStr(payload, payloadOffset, fuelLength)
        payloadOffset += fuelLength
        recordsContent := SubStr(payload, payloadOffset, recordsLength)
        payloadOffset += recordsLength
    } else {
        fuelContent := "# Vehimap fuel v1`n"
        recordsContent := "# Vehimap records v1`n"
    }

    if (backupVersion = "# Vehimap backup v4") {
        metaContent := SubStr(payload, payloadOffset, metaLength)
        payloadOffset += metaLength
        remindersContent := SubStr(payload, payloadOffset, remindersLength)
    } else {
        metaContent := "# Vehimap meta v1`n"
        remindersContent := "# Vehimap reminders v1`n"
    }

    return true
}

TryParseVehiclesBackupContent(content, &loadedVehicles, &errorMessage) {
    loadedVehicles := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap data v3") {
        errorMessage := "Soubor vozidel není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap data v3'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 12) {
            errorMessage := "Soubor vozidel je poškozený nebo není ve formátu v3. Řádek " index " musí obsahovat přesně 12 polí oddělených tabulátory."
            return false
        }

        loadedVehicles.Push({
            id: UnescapeField(fields[1]),
            name: UnescapeField(fields[2]),
            category: NormalizeCategory(UnescapeField(fields[3])),
            vehicleType: UnescapeField(fields[4]),
            makeModel: UnescapeField(fields[5]),
            plate: UnescapeField(fields[6]),
            year: UnescapeField(fields[7]),
            power: UnescapeField(fields[8]),
            lastTk: UnescapeField(fields[9]),
            nextTk: UnescapeField(fields[10]),
            greenCardFrom: UnescapeField(fields[11]),
            greenCardTo: UnescapeField(fields[12])
        })
    }

    return true
}

TryParseHistoryBackupContent(content, &loadedHistory, &errorMessage) {
    loadedHistory := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap history v1") {
        errorMessage := "Soubor historie není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap history v1'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor historie obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 6 && fields.Length != 7) {
            errorMessage := "Soubor historie je poškozený. Řádek " index " musí obsahovat 6 nebo 7 polí oddělených tabulátory."
            return false
        }

        loadedHistory.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            eventDate: UnescapeField(fields[3]),
            eventType: UnescapeField(fields[4]),
            odometer: UnescapeField(fields[5]),
            cost: UnescapeField(fields[6]),
            note: (fields.Length = 7) ? UnescapeField(fields[7]) : ""
        })
    }

    return true
}

TryParseFuelBackupContent(content, &loadedFuelLog, &errorMessage) {
    loadedFuelLog := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap fuel v1") {
        errorMessage := "Soubor kilometrů a tankování není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap fuel v1'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor kilometrů a tankování obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 9) {
            errorMessage := "Soubor kilometrů a tankování je poškozený. Řádek " index " musí obsahovat přesně 9 polí oddělených tabulátory."
            return false
        }

        loadedFuelLog.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            entryDate: UnescapeField(fields[3]),
            odometer: UnescapeField(fields[4]),
            liters: UnescapeField(fields[5]),
            totalCost: UnescapeField(fields[6]),
            fullTank: (UnescapeField(fields[7]) = "1") ? 1 : 0,
            fuelType: UnescapeField(fields[8]),
            note: UnescapeField(fields[9])
        })
    }

    return true
}

TryParseRecordsBackupContent(content, &loadedRecords, &errorMessage) {
    loadedRecords := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap records v1") {
        errorMessage := "Soubor pojištění a dokladů není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap records v1'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor pojištění a dokladů obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 10) {
            errorMessage := "Soubor pojištění a dokladů je poškozený. Řádek " index " musí obsahovat přesně 10 polí oddělených tabulátory."
            return false
        }

        loadedRecords.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            recordType: UnescapeField(fields[3]),
            title: UnescapeField(fields[4]),
            provider: UnescapeField(fields[5]),
            validFrom: UnescapeField(fields[6]),
            validTo: UnescapeField(fields[7]),
            price: UnescapeField(fields[8]),
            filePath: UnescapeField(fields[9]),
            note: UnescapeField(fields[10])
        })
    }

    return true
}

TryParseVehicleMetaBackupContent(content, &loadedMeta, &errorMessage) {
    loadedMeta := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap meta v1") {
        errorMessage := "Soubor stavů a štítků vozidel není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap meta v1'."
        return false
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor stavů a štítků vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 3) {
            errorMessage := "Soubor stavů a štítků vozidel je poškozený. Řádek " index " musí obsahovat přesně 3 pole oddělená tabulátory."
            return false
        }

        loadedMeta.Push({
            vehicleId: UnescapeField(fields[1]),
            state: UnescapeField(fields[2]),
            tags: UnescapeField(fields[3])
        })
    }

    return true
}

TryParseVehicleRemindersBackupContent(content, &loadedReminders, &errorMessage) {
    loadedReminders := []
    errorMessage := ""
    content := NormalizeTextForStorage(content)
    lines := StrSplit(content, "`n")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return true
    }

    if (firstNonEmptyLine != "# Vehimap reminders v1" && firstNonEmptyLine != "# Vehimap reminders v2") {
        errorMessage := "Soubor vlastních připomínek není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap reminders v1' nebo '# Vehimap reminders v2'."
        return false
    }

    isV2 := (firstNonEmptyLine = "# Vehimap reminders v2")

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            errorMessage := "Soubor vlastních připomínek obsahuje neplatnou hlavičku nebo komentář na řádku " index "."
            return false
        }

        fields := StrSplit(line, "`t")
        expectedFieldCount := isV2 ? 7 : 6
        if (fields.Length != expectedFieldCount) {
            errorMessage := "Soubor vlastních připomínek je poškozený. Řádek " index " musí obsahovat přesně " expectedFieldCount " polí oddělených tabulátory."
            return false
        }

        loadedReminders.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            title: UnescapeField(fields[3]),
            dueDate: UnescapeField(fields[4]),
            reminderDays: UnescapeField(fields[5]),
            repeatMode: isV2 ? NormalizeReminderRepeat(UnescapeField(fields[6])) : "Neopakovat",
            note: isV2 ? UnescapeField(fields[7]) : UnescapeField(fields[6])
        })
    }

    return true
}

BackupCurrentFilesBeforeImport() {
    global DataDir, VehiclesFile, HistoryFile, FuelLogFile, RecordsFile, VehicleMetaFile, RemindersFile, SettingsFile

    backupRoot := DataDir "\import-backups"
    if !InStr(FileExist(backupRoot), "D") {
        DirCreate(backupRoot)
    }

    backupDir := backupRoot "\" FormatTime(A_Now, "yyyy-MM-dd_HH-mm-ss")
    DirCreate(backupDir)

    if FileExist(VehiclesFile) {
        FileCopy(VehiclesFile, backupDir "\vehicles.tsv", true)
    }
    if FileExist(HistoryFile) {
        FileCopy(HistoryFile, backupDir "\history.tsv", true)
    }
    if FileExist(FuelLogFile) {
        FileCopy(FuelLogFile, backupDir "\fuel.tsv", true)
    }
    if FileExist(RecordsFile) {
        FileCopy(RecordsFile, backupDir "\records.tsv", true)
    }
    if FileExist(VehicleMetaFile) {
        FileCopy(VehicleMetaFile, backupDir "\vehicle_meta.tsv", true)
    }
    if FileExist(RemindersFile) {
        FileCopy(RemindersFile, backupDir "\reminders.tsv", true)
    }
    if FileExist(SettingsFile) {
        FileCopy(SettingsFile, backupDir "\settings.ini", true)
    }

    return backupDir
}

EnsureSettingsDefaults() {
    global SettingsFile

    EnsureIniKeyExists(SettingsFile, "notifications", "technical_reminder_days", "31")
    EnsureIniKeyExists(SettingsFile, "notifications", "green_card_reminder_days", "31")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_alert_day", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_alert_signature", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_green_alert_day", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_green_alert_signature", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_reminder_alert_day", "")
    EnsureIniKeyExists(SettingsFile, "notifications", "last_reminder_alert_signature", "")
    EnsureIniKeyExists(SettingsFile, "app", "run_at_startup", "0")
    EnsureIniKeyExists(SettingsFile, "app", "hide_on_launch", "0")
    EnsureIniKeyExists(SettingsFile, "app", "hide_inactive_vehicles", "0")
    EnsureIniKeyExists(SettingsFile, "app", "show_dashboard_on_launch", "0")
    EnsureIniKeyExists(SettingsFile, "backups", "automatic_backups_enabled", "0")
    EnsureIniKeyExists(SettingsFile, "backups", "automatic_backup_interval_days", "1")
    EnsureIniKeyExists(SettingsFile, "backups", "automatic_backup_keep_count", "30")
    EnsureIniKeyExists(SettingsFile, "backups", "last_automatic_backup_stamp", "")
    EnsureIniKeyExists(SettingsFile, "backups", "last_automatic_backup_path", "")
    EnsureIniKeyExists(SettingsFile, "overview", "filter", "all")
    EnsureIniKeyExists(SettingsFile, "overview", "include_missing_green", "0")
    EnsureIniKeyExists(SettingsFile, "overview", "include_data_issues", "0")
    EnsureIniKeyExists(SettingsFile, "overview", "sort_column", "6")
    EnsureIniKeyExists(SettingsFile, "overview", "sort_descending", "0")
    EnsureIniKeyExists(SettingsFile, "history_view", "sort_column", "1")
    EnsureIniKeyExists(SettingsFile, "history_view", "sort_descending", "1")
    EnsureIniKeyExists(SettingsFile, "fuel_view", "sort_column", "1")
    EnsureIniKeyExists(SettingsFile, "fuel_view", "sort_descending", "1")
    EnsureIniKeyExists(SettingsFile, "records_view", "sort_column", "4")
    EnsureIniKeyExists(SettingsFile, "records_view", "sort_descending", "0")
    EnsureIniKeyExists(SettingsFile, "reminder_view", "sort_column", "2")
    EnsureIniKeyExists(SettingsFile, "reminder_view", "sort_descending", "0")
}

EnsureIniKeyExists(path, section, key, defaultValue) {
    missingMarker := "__VEHIMAP_MISSING__"
    currentValue := IniRead(path, section, key, missingMarker)
    if (currentValue = missingMarker) {
        IniWrite(defaultValue, path, section, key)
    }
}

RefreshVehicleList(selectVehicleId := "") {
    global Vehicles, VisibleVehicleIds, VehicleList

    if !IsObject(VehicleList) {
        return
    }

    VisibleVehicleIds := []
    category := GetCurrentCategory()
    items := []
    categoryItems := []
    activeCategoryCount := 0
    hiddenInactiveCount := 0
    selectedRow := 0
    searchText := GetMainSearchText()
    filterKind := GetMainVehicleFilterKind()
    hideInactive := GetHideInactiveVehiclesEnabled()

    for vehicle in Vehicles {
        if (vehicle.category = category) {
            categoryItems.Push(vehicle)
            if IsVehicleInactive(vehicle) {
                hiddenInactiveCount += 1
                if hideInactive {
                    continue
                }
            } else {
                activeCategoryCount += 1
            }
            if (VehicleMatchesMainSearch(vehicle, searchText) && VehicleMatchesMainFilter(vehicle, filterKind)) {
                items.Push(vehicle)
            }
        }
    }

    SortVehiclesByDue(&items)

    VehicleList.Opt("-Redraw")
    VehicleList.Delete()
    for vehicle in items {
        row := VehicleList.Add("", vehicle.name, vehicle.vehicleType, vehicle.makeModel, vehicle.plate, vehicle.lastTk, vehicle.nextTk, vehicle.greenCardTo, GetVehicleStatusText(vehicle))
        VisibleVehicleIds.Push(vehicle.id)
        if (selectVehicleId != "" && vehicle.id = selectVehicleId) {
            VehicleList.Modify(row, "Select Focus Vis")
            selectedRow := row
        }
    }
    VehicleList.Opt("+Redraw")

    if selectedRow {
        FocusVehicleListLater()
    }

    UpdateVehicleListLabel(items.Length, categoryItems.Length, category, hiddenInactiveCount, activeCategoryCount, hideInactive)
    UpdateStatusBar()
    SetupTrayMenu()
}

UpdateVehicleListLabel(itemCount, totalCount, category, hiddenInactiveCount := 0, activeCount := 0, hideInactive := false) {
    global VehicleListLabel

    if !IsObject(VehicleListLabel) {
        return
    }

    if hideInactive {
        if (itemCount = activeCount) {
            suffix := (itemCount = 1) ? "1 aktivní vozidlo" : itemCount " aktivních vozidel"
        } else {
            suffix := "zobrazeno " itemCount " z " activeCount " aktivních vozidel"
        }

        if (hiddenInactiveCount > 0) {
            suffix .= ", skryto " hiddenInactiveCount " archivovaných nebo odstavených"
        }
    } else if (itemCount = totalCount) {
        suffix := (itemCount = 1) ? "1 vozidlo" : itemCount " vozidel"
    } else {
        suffix := "zobrazeno " itemCount " z " totalCount " vozidel"
    }

    VehicleListLabel.Text := "Seznam vozidel v kategorii " category " (" suffix ")"
}

GetCurrentCategory() {
    global Categories, TabsCtrl

    index := TabsCtrl.Value
    if (index < 1 || index > Categories.Length) {
        return Categories[1]
    }
    return Categories[index]
}

GetSelectedVehicle() {
    global VisibleVehicleIds, VehicleList

    row := VehicleList.GetNext(0)
    if !row {
        return ""
    }

    if (row > VisibleVehicleIds.Length) {
        return ""
    }

    return FindVehicleById(VisibleVehicleIds[row])
}

FindVehicleById(vehicleId) {
    global Vehicles

    for vehicle in Vehicles {
        if (vehicle.id = vehicleId) {
            return vehicle
        }
    }

    return ""
}

FindVehicleIndexById(vehicleId) {
    global Vehicles

    for index, vehicle in Vehicles {
        if (vehicle.id = vehicleId) {
            return index
        }
    }

    return 0
}

OpenVehicleById(vehicleId, resetFilters := false) {
    global TabsCtrl

    vehicle := FindVehicleById(vehicleId)
    if !IsObject(vehicle) {
        return
    }

    TabsCtrl.Value := GetCategoryIndex(vehicle.category)
    if resetFilters {
        SetMainVehicleFilters("", "all")
    }
    ShowMainWindow()
    RefreshVehicleList(vehicle.id)
}

OpenNearestDueVehicle(*) {
    global AppTitle

    upcoming := GetUpcomingVehicles()
    if (upcoming.Length = 0) {
        MsgBox("Momentálně není žádné vozidlo s blížící se nebo propadlou technickou kontrolou.", AppTitle, 0x40)
        return
    }

    OpenVehicleById(upcoming[1].vehicle.id, true)
}

ManualDueCheck(*) {
    global AppTitle

    upcoming := GetUpcomingVehicles()
    if (upcoming.Length = 0) {
        MsgBox("Žádná vozidla teď nevyžadují upozornění na technickou kontrolu.", AppTitle, 0x40)
        return
    }

    MsgBox(BuildReminderMessage(upcoming), AppTitle, 0x40)
    OpenVehicleById(upcoming[1].vehicle.id, true)
}

OpenNearestGreenCardVehicle(*) {
    global AppTitle

    if !HasAnyGreenCardConfigured() {
        MsgBox("U žádného vozidla není vyplněná zelená karta. Můžete ji doplnit v editaci vozidla.", AppTitle, 0x40)
        return
    }

    upcoming := GetUpcomingGreenCards()
    if (upcoming.Length = 0) {
        message := "Žádná vyplněná zelená karta teď nevyžaduje upozornění."
        if HasAnyMissingGreenCard() {
            message .= "`nU některých vozidel zelená karta vyplněná není a můžete ji doplnit v editaci vozidla."
        }
        MsgBox(message, AppTitle, 0x40)
        return
    }

    OpenVehicleById(upcoming[1].vehicle.id, true)
}

ManualGreenCardCheck(*) {
    global AppTitle

    if !HasAnyGreenCardConfigured() {
        MsgBox("U žádného vozidla není vyplněná zelená karta. Můžete ji doplnit v editaci vozidla.", AppTitle, 0x40)
        return
    }

    upcoming := GetUpcomingGreenCards()
    if (upcoming.Length = 0) {
        message := "Žádná vyplněná zelená karta teď nevyžaduje upozornění."
        if HasAnyMissingGreenCard() {
            message .= "`nU některých vozidel zelená karta vyplněná není a můžete ji doplnit v editaci vozidla."
        }
        MsgBox(message, AppTitle, 0x40)
        return
    }

    MsgBox(BuildGreenCardReminderMessage(upcoming), AppTitle, 0x40)
    OpenVehicleById(upcoming[1].vehicle.id, true)
}

OpenNearestReminder(*) {
    global AppTitle

    upcoming := GetUpcomingCustomReminders()
    if (upcoming.Length = 0) {
        MsgBox("Momentálně není žádná vlastní připomínka, která by vyžadovala pozornost.", AppTitle, 0x40)
        return
    }

    OpenVehicleById(upcoming[1].vehicle.id, true)
}

ManualReminderCheck(*) {
    global AppTitle

    upcoming := GetUpcomingCustomReminders()
    if (upcoming.Length = 0) {
        MsgBox("Žádné vlastní připomínky teď nevyžadují upozornění.", AppTitle, 0x40)
        return
    }

    MsgBox(BuildCustomReminderMessage(upcoming), AppTitle, 0x40)
    OpenVehicleById(upcoming[1].vehicle.id, true)
}

CheckDueVehicles(showTrayNotification := true, forceMessageBox := false) {
    global AppTitle

    upcoming := GetUpcomingVehicles()
    greenCards := GetUpcomingGreenCards()
    reminders := GetUpcomingCustomReminders()
    SetupTrayMenu()
    UpdateStatusBar()

    if forceMessageBox {
        ManualDueCheck()
        return
    }

    if !showTrayNotification {
        return
    }

    showTechnicalAlert := false
    if (upcoming.Length > 0) {
        signature := BuildAlertSignature(upcoming)
        showTechnicalAlert := ShouldShowAlert(signature, "technical")
    }

    showGreenAlert := false
    if (greenCards.Length > 0) {
        signature := BuildGreenCardAlertSignature(greenCards)
        showGreenAlert := ShouldShowAlert(signature, "green")
    }

    showReminderAlert := false
    if (reminders.Length > 0) {
        signature := BuildCustomReminderAlertSignature(reminders)
        showReminderAlert := ShouldShowAlert(signature, "reminder")
    }

    message := BuildAutomaticReminderMessage(upcoming, greenCards, reminders, showTechnicalAlert, showGreenAlert, showReminderAlert)
    if (message != "") {
        TrayTip(message, AppTitle)
    }
}

GetUpcomingVehicles() {
    global Vehicles

    cutoff := DateAdd(A_Now, GetTechnicalReminderDays(), "Days")
    upcoming := []

    for vehicle in Vehicles {
        dueStamp := ParseDueStamp(vehicle.nextTk)
        if (dueStamp = "") {
            continue
        }
        if (dueStamp <= cutoff) {
            upcoming.Push({
                vehicle: vehicle,
                dueStamp: dueStamp
            })
        }
    }

    SortUpcomingByDue(&upcoming)
    return upcoming
}

GetUpcomingGreenCards() {
    global Vehicles

    cutoff := DateAdd(A_Now, GetGreenCardReminderDays(), "Days")
    upcoming := []

    for vehicle in Vehicles {
        dueStamp := ParseDueStamp(vehicle.greenCardTo)
        if (dueStamp = "") {
            continue
        }
        if (dueStamp <= cutoff) {
            upcoming.Push({
                vehicle: vehicle,
                dueStamp: dueStamp
            })
        }
    }

    SortUpcomingByDue(&upcoming)
    return upcoming
}

BuildUpcomingOverviewEntries(includeMissingGreenCards := false, includeDataIssues := false) {
    global Vehicles

    technicalReminderDays := GetTechnicalReminderDays()
    greenCardReminderDays := GetGreenCardReminderDays()
    entries := []

    for item in GetUpcomingVehicles() {
        entries.Push({
            kind: "technical",
            kindLabel: "Technická kontrola",
            vehicle: item.vehicle,
            dueStamp: item.dueStamp,
            term: item.vehicle.nextTk,
            status: GetExpirationStatusText(item.vehicle.nextTk, technicalReminderDays),
            isMissingGreen: false
        })
    }

    for item in GetUpcomingGreenCards() {
        entries.Push({
            kind: "green",
            kindLabel: "Zelená karta",
            vehicle: item.vehicle,
            dueStamp: item.dueStamp,
            term: item.vehicle.greenCardTo,
            status: GetExpirationStatusText(item.vehicle.greenCardTo, greenCardReminderDays),
            isMissingGreen: false
        })
    }

    for item in GetUpcomingCustomReminders() {
        entries.Push({
            kind: "custom",
            kindLabel: "Vlastní připomínka",
            vehicle: item.vehicle,
            dueStamp: item.dueStamp,
            term: item.reminder.dueDate,
            status: GetReminderExpirationStatusText(item.reminder.dueDate, item.reminder.reminderDays + 0),
            isMissingGreen: false,
            entryId: item.reminder.id
        })
    }

    if includeMissingGreenCards {
        for vehicle in Vehicles {
            if (vehicle.greenCardTo = "") {
                entries.Push({
                    kind: "green",
                    kindLabel: "Zelená karta",
                    vehicle: vehicle,
                    dueStamp: "99999999999998",
                    term: "Nevyplněno",
                    status: "Chybí",
                    isMissingGreen: true
                })
            }
        }
    }

    if includeDataIssues {
        for entry in BuildDashboardDataIssueEntries() {
            entries.Push(entry)
        }
    }

    return entries
}

BuildUpcomingOverviewSummary(entries, allEntries := "") {
    technicalCount := 0
    greenCount := 0
    customCount := 0
    dataIssueCount := 0
    totalCount := IsObject(allEntries) ? allEntries.Length : entries.Length

    for entry in entries {
        if (entry.kind = "technical") {
            technicalCount += 1
        } else if (entry.kind = "green") {
            greenCount += 1
        } else if (entry.kind = "custom") {
            customCount += 1
        } else if IsOverviewDataIssueEntry(entry) {
            dataIssueCount += 1
        }
    }

    missingGreenCount := GetMissingGreenCardCount()
    totalDataIssueCount := GetOverviewDataIssueCount()
    if (totalCount = 0) {
        if ShouldShowDataIssuesInOverview() {
            summary := "Momentálně není žádný blížící se ani propadlý termín ani datový nedostatek, který by podle aktuálního nastavení vyžadoval pozornost."
        } else {
            summary := "Momentálně není žádný blížící se ani propadlý termín, který by podle aktuálního nastavení vyžadoval pozornost."
        }
    } else if (entries.Length = totalCount) {
        summary := "Celkem " entries.Length " položek k pozornosti: " technicalCount " technických kontrol, " greenCount " zelených karet a " customCount " vlastních připomínek"
    } else {
        summary := "Zobrazeno " entries.Length " z " totalCount " položek: " technicalCount " technických kontrol, " greenCount " zelených karet a " customCount " vlastních připomínek"
    }

    if (totalCount > 0) {
        if (dataIssueCount > 0) {
            summary .= " a " dataIssueCount " datových nedostatků."
        } else {
            summary .= "."
        }
    }

    if (missingGreenCount > 0) {
        if ShouldShowMissingGreenCardsInOverview() {
            summary .= " Je zapnuto i zobrazení vozidel bez vyplněné zelené karty."
        } else {
            summary .= " U " missingGreenCount " vozidel není zelená karta vyplněná."
        }
    }

    if (totalDataIssueCount > 0) {
        if ShouldShowDataIssuesInOverview() {
            summary .= " Je zapnuto i zobrazení datových nedostatků."
        } else {
            summary .= " Další datové nedostatky můžete zobrazit zapnutím volby pod hledáním."
        }
    }

    return summary
}

GetOverviewDataIssueCount() {
    return BuildDashboardDataIssueEntries().Length
}

GetMissingGreenCardCount() {
    global Vehicles

    count := 0
    for vehicle in Vehicles {
        if (vehicle.greenCardTo = "") {
            count += 1
        }
    }

    return count
}

HasAnyGreenCardConfigured() {
    global Vehicles

    for vehicle in Vehicles {
        if (vehicle.greenCardTo != "") {
            return true
        }
    }

    return false
}

HasAnyMissingGreenCard() {
    global Vehicles

    for vehicle in Vehicles {
        if (vehicle.greenCardTo = "") {
            return true
        }
    }

    return false
}

ValidatePositiveIntegerSetting(ctrl, fieldLabel, minValue := 1, maxValue := 999) {
    global AppTitle

    value := Trim(ctrl.Text)
    if !RegExMatch(value, "^\d{1,3}$") {
        MsgBox(fieldLabel " musí být celé číslo od " minValue " do " maxValue ".", AppTitle, 0x30)
        ctrl.Focus()
        return ""
    }

    value += 0
    if (value < minValue || value > maxValue) {
        MsgBox(fieldLabel " musí být v rozsahu od " minValue " do " maxValue ".", AppTitle, 0x30)
        ctrl.Focus()
        return ""
    }

    return value
}

ValidateReminderDaysSetting(ctrl, fieldLabel) {
    return ValidatePositiveIntegerSetting(ctrl, fieldLabel, 1, 999)
}

GetTechnicalReminderDays() {
    return ReadReminderDaysSetting("technical_reminder_days")
}

GetGreenCardReminderDays() {
    return ReadReminderDaysSetting("green_card_reminder_days")
}

ReadReminderDaysSetting(keyName) {
    global SettingsFile

    days := IniRead(SettingsFile, "notifications", keyName, "")
    if (days = "") {
        days := IniRead(SettingsFile, "notifications", "reminder_days", "31")
    }

    days += 0
    if (days < 1 || days > 999) {
        return 31
    }

    return days
}

GetRunAtStartupEnabled() {
    return FileExist(GetStartupShortcutPath()) ? 1 : 0
}

GetHideOnLaunchEnabled() {
    global SettingsFile

    return IniRead(SettingsFile, "app", "hide_on_launch", "0") = "1" ? 1 : 0
}

GetHideInactiveVehiclesEnabled() {
    global SettingsFile

    return IniRead(SettingsFile, "app", "hide_inactive_vehicles", "0") = "1" ? 1 : 0
}

GetShowDashboardOnLaunchEnabled() {
    global SettingsFile

    return IniRead(SettingsFile, "app", "show_dashboard_on_launch", "0") = "1" ? 1 : 0
}

GetAutomaticBackupsEnabled() {
    global SettingsFile

    return IniRead(SettingsFile, "backups", "automatic_backups_enabled", "0") = "1" ? 1 : 0
}

GetAutomaticBackupIntervalDays() {
    global SettingsFile

    days := IniRead(SettingsFile, "backups", "automatic_backup_interval_days", "1") + 0
    if (days < 1 || days > 999) {
        return 1
    }
    return days
}

GetAutomaticBackupKeepCount() {
    global SettingsFile

    keepCount := IniRead(SettingsFile, "backups", "automatic_backup_keep_count", "30") + 0
    if (keepCount < 1 || keepCount > 999) {
        return 30
    }
    return keepCount
}

SetRunAtStartupEnabled(enabled) {
    global AppTitle

    shortcutPath := GetStartupShortcutPath()

    try {
        if enabled {
            shell := ComObject("WScript.Shell")
            shortcut := shell.CreateShortcut(shortcutPath)
            shortcut.TargetPath := GetLaunchTargetPath()
            shortcut.Arguments := GetLaunchArguments()
            shortcut.WorkingDirectory := A_ScriptDir
            shortcut.Description := AppTitle
            shortcut.Save()
        } else if FileExist(shortcutPath) {
            FileDelete(shortcutPath)
        }
        return true
    } catch as err {
        action := enabled ? "zapnout" : "vypnout"
        MsgBox("Nepodařilo se " action " spuštění Vehimap po startu počítače.`n`n" err.Message, AppTitle, 0x30)
        return false
    }
}

GetStartupShortcutPath() {
    global AppTitle

    safeTitle := RegExReplace(AppTitle, '[\\/:*?"<>|]', "")
    return A_Startup "\" safeTitle ".lnk"
}

GetLaunchTargetPath() {
    return A_IsCompiled ? A_ScriptFullPath : A_AhkPath
}

GetLaunchArguments() {
    if A_IsCompiled {
        return ""
    }

    return '"' A_ScriptFullPath '"'
}

ResetAlertHistory() {
    global SettingsFile

    IniWrite("", SettingsFile, "notifications", "last_alert_day")
    IniWrite("", SettingsFile, "notifications", "last_alert_signature")
    IniWrite("", SettingsFile, "notifications", "last_green_alert_day")
    IniWrite("", SettingsFile, "notifications", "last_green_alert_signature")
    IniWrite("", SettingsFile, "notifications", "last_reminder_alert_day")
    IniWrite("", SettingsFile, "notifications", "last_reminder_alert_signature")
}

GetExpirationStatusText(monthYear, reminderDays) {
    dueStamp := ParseDueStamp(monthYear)
    if (dueStamp = "") {
        return ""
    }

    if (dueStamp < A_Now) {
        return "Po termínu"
    }

    cutoff := DateAdd(A_Now, reminderDays, "Days")
    if (dueStamp <= cutoff) {
        daysLeft := DateDiff(dueStamp, A_Now, "Days")
        if (daysLeft < 1) {
            return "Tento měsíc"
        }
        return "Do " daysLeft " dní"
    }

    return ""
}

BuildReminderMessage(upcoming) {
    first := upcoming[1]
    lines := []

    if (first.dueStamp < A_Now) {
        lines.Push("Po termínu TK je vozidlo " first.vehicle.name " (" first.vehicle.nextTk ").")
    } else {
        lines.Push("Blíží se TK pro vozidlo " first.vehicle.name " (" first.vehicle.nextTk ").")
    }

    if (upcoming.Length > 1) {
        lines.Push("Pozornost vyžaduje ještě dalších " (upcoming.Length - 1) " vozidel.")
    }

    maxList := upcoming.Length < 4 ? upcoming.Length : 4
    Loop maxList {
        item := upcoming[A_Index]
        prefix := (item.dueStamp < A_Now) ? "Po termínu" : "Termín"
        lines.Push(prefix ": " item.vehicle.name " - " item.vehicle.nextTk)
    }

    return JoinLines(lines)
}

BuildGreenCardReminderMessage(upcoming) {
    first := upcoming[1]
    lines := []

    if (first.dueStamp < A_Now) {
        lines.Push("Po termínu zelené karty je vozidlo " first.vehicle.name " (" first.vehicle.greenCardTo ").")
    } else {
        lines.Push("Blíží se konec zelené karty pro vozidlo " first.vehicle.name " (" first.vehicle.greenCardTo ").")
    }

    if (upcoming.Length > 1) {
        lines.Push("Pozornost vyžaduje ještě dalších " (upcoming.Length - 1) " vozidel.")
    }

    if HasAnyMissingGreenCard() {
        lines.Push("U některých vozidel zelená karta vyplněná není a můžete ji doplnit v editaci vozidla.")
    }

    maxList := upcoming.Length < 4 ? upcoming.Length : 4
    Loop maxList {
        item := upcoming[A_Index]
        prefix := (item.dueStamp < A_Now) ? "Po termínu" : "Termín"
        lines.Push(prefix ": " item.vehicle.name " - " item.vehicle.greenCardTo)
    }

    return JoinLines(lines)
}

BuildCustomReminderMessage(upcoming) {
    first := upcoming[1]
    lines := []

    if (first.dueStamp < A_Now) {
        lines.Push("Po termínu připomínky je vozidlo " first.vehicle.name ": " first.reminder.title " (" first.reminder.dueDate ").")
    } else {
        lines.Push("Blíží se připomínka pro vozidlo " first.vehicle.name ": " first.reminder.title " (" first.reminder.dueDate ").")
    }

    if (upcoming.Length > 1) {
        lines.Push("Pozornost vyžaduje ještě dalších " (upcoming.Length - 1) " připomínek.")
    }

    maxList := upcoming.Length < 4 ? upcoming.Length : 4
    Loop maxList {
        item := upcoming[A_Index]
        prefix := (item.dueStamp < A_Now) ? "Po termínu" : "Termín"
        lines.Push(prefix ": " item.vehicle.name " - " item.reminder.title " - " item.reminder.dueDate)
    }

    return JoinLines(lines)
}

BuildAutomaticReminderMessage(upcoming, greenCards, reminders, showTechnicalAlert := false, showGreenAlert := false, showReminderAlert := false) {
    lines := []

    if (showTechnicalAlert && upcoming.Length > 0) {
        lines.Push(BuildReminderSummaryLine(upcoming))
    }
    if (showGreenAlert && greenCards.Length > 0) {
        lines.Push(BuildGreenCardReminderSummaryLine(greenCards))
    }
    if (showReminderAlert && reminders.Length > 0) {
        lines.Push(BuildCustomReminderSummaryLine(reminders))
    }
    if ((showGreenAlert || showReminderAlert) && greenCards.Length > 0 && HasAnyMissingGreenCard()) {
        lines.Push("U některých vozidel zelená karta vyplněná není a můžete ji doplnit v editaci vozidla.")
    }

    return JoinLines(lines)
}

BuildReminderSummaryLine(upcoming) {
    first := upcoming[1]
    if (first.dueStamp < A_Now) {
        line := "TK po termínu: " first.vehicle.name " (" first.vehicle.nextTk ")."
    } else {
        line := "Blíží se TK: " first.vehicle.name " (" first.vehicle.nextTk ")."
    }

    if (upcoming.Length > 1) {
        line .= " Další TK: " (upcoming.Length - 1) " vozidel."
    }

    return line
}

BuildGreenCardReminderSummaryLine(upcoming) {
    first := upcoming[1]
    if (first.dueStamp < A_Now) {
        line := "ZK po termínu: " first.vehicle.name " (" first.vehicle.greenCardTo ")."
    } else {
        line := "Blíží se konec ZK: " first.vehicle.name " (" first.vehicle.greenCardTo ")."
    }

    if (upcoming.Length > 1) {
        line .= " Další ZK: " (upcoming.Length - 1) " vozidel."
    }

    return line
}

BuildCustomReminderSummaryLine(upcoming) {
    first := upcoming[1]
    if (first.dueStamp < A_Now) {
        line := "Připomínka po termínu: " first.vehicle.name " - " first.reminder.title " (" first.reminder.dueDate ")."
    } else {
        line := "Blíží se připomínka: " first.vehicle.name " - " first.reminder.title " (" first.reminder.dueDate ")."
    }

    if (upcoming.Length > 1) {
        line .= " Další připomínky: " (upcoming.Length - 1) " položek."
    }

    return line
}

BuildAlertSignature(upcoming) {
    signature := SubStr(A_Now, 1, 8) "|"
    maxList := upcoming.Length < 5 ? upcoming.Length : 5

    Loop maxList {
        item := upcoming[A_Index]
        signature .= item.vehicle.id ":" item.vehicle.nextTk ";"
    }

    return signature
}

BuildGreenCardAlertSignature(upcoming) {
    signature := SubStr(A_Now, 1, 8) "|"
    maxList := upcoming.Length < 5 ? upcoming.Length : 5

    Loop maxList {
        item := upcoming[A_Index]
        signature .= item.vehicle.id ":" item.vehicle.greenCardTo ";"
    }

    return signature
}

BuildCustomReminderAlertSignature(upcoming) {
    signature := SubStr(A_Now, 1, 8) "|"
    maxList := upcoming.Length < 5 ? upcoming.Length : 5

    Loop maxList {
        item := upcoming[A_Index]
        signature .= item.vehicle.id ":" item.reminder.id ":" item.reminder.dueDate ";"
    }

    return signature
}

ShouldShowAlert(signature, kind := "technical") {
    global SettingsFile

    today := SubStr(A_Now, 1, 8)
    if (kind = "green") {
        dayKey := "last_green_alert_day"
        signatureKey := "last_green_alert_signature"
    } else if (kind = "reminder") {
        dayKey := "last_reminder_alert_day"
        signatureKey := "last_reminder_alert_signature"
    } else {
        dayKey := "last_alert_day"
        signatureKey := "last_alert_signature"
    }

    lastDay := IniRead(SettingsFile, "notifications", dayKey, "")
    lastSignature := IniRead(SettingsFile, "notifications", signatureKey, "")

    if (lastDay = today && lastSignature = signature) {
        return false
    }

    IniWrite(today, SettingsFile, "notifications", dayKey)
    IniWrite(signature, SettingsFile, "notifications", signatureKey)
    return true
}

SetupTrayMenu() {
    global AppTitle

    menu := A_TrayMenu
    try menu.Delete()

    menu.Add("Otevřít " AppTitle, ShowMainWindow)
    menu.Add("Dashboard", OpenDashboard)

    upcoming := GetUpcomingVehicles()
    if (upcoming.Length > 0) {
        menu.Add("Zobrazit nejbližší TK: " ShortenText(upcoming[1].vehicle.name, 40) " (" upcoming[1].vehicle.nextTk ")", OpenNearestDueVehicle)
    } else {
        menu.Add("Zobrazit nejbližší TK: nic nečeká", OpenNearestDueVehicle)
    }

    greenCards := GetUpcomingGreenCards()
    if (greenCards.Length > 0) {
        menu.Add("Zobrazit nejbližší ZK: " ShortenText(greenCards[1].vehicle.name, 40) " (" greenCards[1].vehicle.greenCardTo ")", OpenNearestGreenCardVehicle)
    } else if HasAnyGreenCardConfigured() {
        menu.Add("Zobrazit nejbližší ZK: nic nečeká", OpenNearestGreenCardVehicle)
    } else {
        menu.Add("Zobrazit nejbližší ZK: nevyplněno", OpenNearestGreenCardVehicle)
    }

    reminders := GetUpcomingCustomReminders()
    if (reminders.Length > 0) {
        menu.Add("Zobrazit nejbližší připomínku: " ShortenText(reminders[1].vehicle.name, 26) " - " ShortenText(reminders[1].reminder.title, 20), OpenNearestReminder)
    } else {
        menu.Add("Zobrazit nejbližší připomínku: nic nečeká", OpenNearestReminder)
    }

    menu.Add("Zkontrolovat technické kontroly", ManualDueCheck)
    menu.Add("Zkontrolovat zelené karty", ManualGreenCardCheck)
    menu.Add("Zkontrolovat připomínky", ManualReminderCheck)
    menu.Add("Přehled všech termínů", OpenUpcomingOverviewDialog)
    menu.Add("Propadlé termíny", OpenOverdueDialog)
    menu.Add("Tiskový přehled", OpenPrintableVehicleReport)
    menu.Add("Export dat", ExportAppData)
    menu.Add("Import dat", ImportAppData)
    menu.Add("Nastavení", OpenSettingsDialog)
    menu.Add()
    menu.Add("Konec", ExitVehimap)
    menu.Default := "Otevřít " AppTitle
    menu.ClickCount := 1
    UpdateTrayIconTip()
}

ExitVehimap(*) {
    ExitApp()
}

UpdateStatusBar() {
    global StatusBar, Vehicles

    if !IsObject(StatusBar) {
        return
    }

    category := GetCurrentCategory()
    count := 0
    hiddenInactiveCount := 0
    activeCount := 0
    hideInactive := GetHideInactiveVehiclesEnabled()
    for vehicle in Vehicles {
        if (vehicle.category = category) {
            count += 1
            if IsVehicleInactive(vehicle) {
                hiddenInactiveCount += 1
            } else {
                activeCount += 1
            }
        }
    }

    if hideInactive {
        StatusBar.SetText(category ": " activeCount " aktivních / " count " celkem", 1)
    } else if (hiddenInactiveCount > 0) {
        StatusBar.SetText(category ": " count " vozidel, z toho " hiddenInactiveCount " archivovaných nebo odstavených", 1)
    } else {
        StatusBar.SetText(category ": " count " vozidel", 1)
    }

    upcoming := GetUpcomingVehicles()
    greenCards := GetUpcomingGreenCards()
    reminders := GetUpcomingCustomReminders()

    if (upcoming.Length = 0) {
        tkText := "TK: nic nečeká"
    } else {
        vehicle := upcoming[1].vehicle
        prefix := (upcoming[1].dueStamp < A_Now) ? "TK po termínu" : "TK"
        tkText := prefix ": " vehicle.name " (" vehicle.nextTk ")"
    }

    if (greenCards.Length = 0) {
        if HasAnyGreenCardConfigured() {
            greenText := "ZK: nic nečeká"
        } else {
            greenText := "ZK: nevyplněno"
        }
    } else {
        vehicle := greenCards[1].vehicle
        prefix := (greenCards[1].dueStamp < A_Now) ? "ZK po termínu" : "ZK"
        greenText := prefix ": " vehicle.name " (" vehicle.greenCardTo ")"
    }

    if (reminders.Length = 0) {
        reminderText := "Př: nic nečeká"
    } else {
        prefix := (reminders[1].dueStamp < A_Now) ? "Př po termínu" : "Př"
        reminderText := prefix ": " reminders[1].vehicle.name " (" reminders[1].reminder.dueDate ")"
    }

    StatusBar.SetText(tkText " | " greenText " | " reminderText, 2)
}

GetVehicleStatusText(vehicle) {
    parts := []
    technicalReminderDays := GetTechnicalReminderDays()
    greenCardReminderDays := GetGreenCardReminderDays()

    tkStatus := GetExpirationStatusText(vehicle.nextTk, technicalReminderDays)
    if (tkStatus != "") {
        parts.Push("TK: " tkStatus)
    }

    greenStatus := GetExpirationStatusText(vehicle.greenCardTo, greenCardReminderDays)
    if (greenStatus != "") {
        parts.Push("ZK: " greenStatus)
    }

    reminderStatus := GetVehicleReminderStateText(vehicle.id)
    if (reminderStatus != "") {
        parts.Push(reminderStatus)
    }

    if (parts.Length = 0) {
        return ""
    }

    return JoinInline(parts, " | ")
}

UpdateTrayIconTip(forceExplorerRefresh := false) {
    global LastTrayIconTip

    tip := BuildTrayIconTip()
    changed := (tip != LastTrayIconTip)

    A_IconTip := tip
    LastTrayIconTip := tip

    if (changed || forceExplorerRefresh) {
        RefreshTrayIdentityLater()
    }
}

BuildTrayIconTip() {
    global AppTitle

    counts := GetTrayAttentionCounts()
    if (
        counts.overdueTechnical = 0
        && counts.overdueGreen = 0
        && counts.overdueReminders = 0
        && counts.upcomingTechnical = 0
        && counts.upcomingGreen = 0
        && counts.upcomingReminders = 0
    ) {
        return AppTitle
    }

    tip := AppTitle
        . " - po termínu "
        . counts.overdueTechnical " TK / " counts.overdueGreen " ZK"
        . ", brzy vyprší "
        . counts.upcomingTechnical " TK / " counts.upcomingGreen " ZK"
    if (counts.overdueReminders > 0 || counts.upcomingReminders > 0) {
        tip .= ", připomínky " counts.overdueReminders " po termínu / " counts.upcomingReminders " brzy"
    }
    return tip
}

GetTrayAttentionCounts() {
    global Vehicles

    counts := {
        overdueTechnical: 0,
        overdueGreen: 0,
        upcomingTechnical: 0,
        upcomingGreen: 0,
        overdueReminders: 0,
        upcomingReminders: 0
    }

    technicalCutoff := DateAdd(A_Now, GetTechnicalReminderDays(), "Days")
    greenCutoff := DateAdd(A_Now, GetGreenCardReminderDays(), "Days")

    for vehicle in Vehicles {
        technicalDueStamp := ParseDueStamp(vehicle.nextTk)
        if (technicalDueStamp != "") {
            if (technicalDueStamp < A_Now) {
                counts.overdueTechnical += 1
            } else if (technicalDueStamp <= technicalCutoff) {
                counts.upcomingTechnical += 1
            }
        }

        greenDueStamp := ParseDueStamp(vehicle.greenCardTo)
        if (greenDueStamp != "") {
            if (greenDueStamp < A_Now) {
                counts.overdueGreen += 1
            } else if (greenDueStamp <= greenCutoff) {
                counts.upcomingGreen += 1
            }
        }
    }

    for item in GetUpcomingCustomReminders() {
        if (item.dueStamp < A_Now) {
            counts.overdueReminders += 1
        } else {
            counts.upcomingReminders += 1
        }
    }

    return counts
}

NormalizeMonthYear(value) {
    value := Trim(value)
    if (value = "") {
        return ""
    }

    if !RegExMatch(value, "^\s*(\d{1,2})\s*[/.-]\s*(\d{4})\s*$", &match) {
        return ""
    }

    month := match[1] + 0
    year := match[2] + 0
    if (month < 1 || month > 12 || year < 1900 || year > 2200) {
        return ""
    }

    return Format("{:02}/{:04}", month, year)
}

ParseDueStamp(monthYear) {
    normalized := NormalizeMonthYear(monthYear)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, "/")
    month := parts[1] + 0
    year := parts[2] + 0
    day := DaysInMonth(year, month)
    return Format("{:04}{:02}{:02}235959", year, month, day)
}

DaysInMonth(year, month) {
    static thirtyOneDayMonths := Map(1, 1, 3, 1, 5, 1, 7, 1, 8, 1, 10, 1, 12, 1)

    if thirtyOneDayMonths.Has(month) {
        return 31
    }

    if (month = 2) {
        return VehimapLeapYearInternal(year) ? 29 : 28
    }

    return 30
}

VehimapLeapYearInternal(year) {
    return (Mod(year, 4) = 0 && Mod(year, 100) != 0) || Mod(year, 400) = 0
}

NormalizeCategory(category) {
    global Categories

    if (category = "Osobní") {
        return "Osobní vozidla"
    }

    if (category = "Nákladní") {
        return "Nákladní vozidla"
    }

    for allowed in Categories {
        if (allowed = category) {
            return allowed
        }
    }
    return "Ostatní"
}

GetCategoryIndex(category) {
    global Categories

    category := NormalizeCategory(category)
    for index, allowed in Categories {
        if (allowed = category) {
            return index
        }
    }

    return Categories.Length
}

SetDropDownToText(ctrl, wantedText, items := "") {
    global Categories

    defaultIndex := 0
    if IsObject(items) {
        if (items.Length = Categories.Length && items[1] = Categories[1]) {
            wantedText := NormalizeCategory(wantedText)
            defaultIndex := Categories.Length
        } else if (items.Length > 0) {
            defaultIndex := 1
        }

        for index, item in items {
            if (item = wantedText) {
                ctrl.Value := index
                return
            }
        }
    }

    if defaultIndex {
        ctrl.Value := defaultIndex
        return
    }

    try ctrl.Text := wantedText
}

GenerateVehicleId() {
    return A_Now "_" Random(1000, 9999)
}

EscapeField(value) {
    value := StrReplace(value, "\", "\\")
    value := StrReplace(value, "`t", "\t")
    value := StrReplace(value, "`n", "\n")
    value := StrReplace(value, "`r")
    return value
}

UnescapeField(value) {
    placeholder := Chr(1)
    value := StrReplace(value, "\\", placeholder)
    value := StrReplace(value, "\t", "`t")
    value := StrReplace(value, "\n", "`n")
    value := StrReplace(value, placeholder, "\")
    return value
}

JoinLines(lines, separator := "`n") {
    output := ""
    for index, line in lines {
        if (index > 1) {
            output .= separator
        }
        output .= line
    }
    return output
}

JoinInline(parts, separator := " | ") {
    output := ""
    for index, part in parts {
        if (index > 1) {
            output .= separator
        }
        output .= part
    }
    return output
}

SortVehiclesByDue(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicles(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicles(left, right) {
    leftKey := ParseDueStamp(left.nextTk)
    rightKey := ParseDueStamp(right.nextTk)

    if (leftKey = "") {
        leftKey := "99999999999999"
    }
    if (rightKey = "") {
        rightKey := "99999999999999"
    }

    if (leftKey < rightKey) {
        return -1
    }
    if (leftKey > rightKey) {
        return 1
    }

    return CompareTextValues(left.name, right.name)
}

SortUpcomingByDue(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareUpcoming(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareUpcoming(left, right) {
    if (left.dueStamp < right.dueStamp) {
        return -1
    }
    if (left.dueStamp > right.dueStamp) {
        return 1
    }
    return CompareVehicles(left.vehicle, right.vehicle)
}

SortOverviewEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareOverviewEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareOverviewEntries(left, right) {
    global OverviewSortColumn, OverviewSortDescending

    result := CompareOverviewEntriesByColumn(left, right, OverviewSortColumn)
    if (result = 0 && OverviewSortColumn != 6) {
        result := CompareOverviewEntriesByColumn(left, right, 6)
    }
    if (result = 0) {
        result := CompareVehicles(left.vehicle, right.vehicle)
    }

    return OverviewSortDescending ? -result : result
}

CompareOverviewEntriesByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareOverviewText(left.kindLabel, right.kindLabel)
        case 2:
            return CompareOverviewText(left.vehicle.name, right.vehicle.name)
        case 3:
            return CompareOverviewText(left.vehicle.category, right.vehicle.category)
        case 4:
            return CompareOverviewText(left.vehicle.makeModel, right.vehicle.makeModel)
        case 5:
            return CompareOverviewText(left.vehicle.plate, right.vehicle.plate)
        case 6:
            return CompareOverviewDueStamp(left, right)
        case 7:
            return CompareOverviewText(left.status, right.status)
    }

    return 0
}

CompareOverviewText(leftText, rightText) {
    return CompareTextValues(leftText, rightText)
}

CompareOverviewDueStamp(left, right) {
    if (left.dueStamp < right.dueStamp) {
        return -1
    }
    if (left.dueStamp > right.dueStamp) {
        return 1
    }

    return CompareTextValues(left.kind, right.kind)
}

CompareTextValues(leftText, rightText) {
    return StrCompare(StrLower(leftText), StrLower(rightText))
}

CompareNumberValues(leftValue, rightValue) {
    if (leftValue < rightValue) {
        return -1
    }
    if (leftValue > rightValue) {
        return 1
    }

    return 0
}

CompareOptionalStampValues(leftStamp, rightStamp) {
    if (leftStamp = "") {
        leftStamp := "99999999999999"
    }
    if (rightStamp = "") {
        rightStamp := "99999999999999"
    }

    if (leftStamp < rightStamp) {
        return -1
    }
    if (leftStamp > rightStamp) {
        return 1
    }

    return 0
}

CompareOptionalIntegerTexts(leftText, rightText) {
    leftValue := (Trim(leftText) = "") ? 2147483647 : leftText + 0
    rightValue := (Trim(rightText) = "") ? 2147483647 : rightText + 0
    return CompareNumberValues(leftValue, rightValue)
}

CompareOptionalDecimalTexts(leftText, rightText) {
    leftValue := 0.0
    rightValue := 0.0
    if !TryParseDecimalValue(leftText, &leftValue) {
        leftValue := 9999999999999999.0
    }
    if !TryParseDecimalValue(rightText, &rightValue) {
        rightValue := 9999999999999999.0
    }

    return CompareNumberValues(leftValue, rightValue)
}

CompareOptionalMoneyTexts(leftText, rightText) {
    leftValue := 0.0
    rightValue := 0.0
    if !TryParseMoneyAmount(leftText, &leftValue) {
        leftValue := 9999999999999999.0
    }
    if !TryParseMoneyAmount(rightText, &rightValue) {
        rightValue := 9999999999999999.0
    }

    return CompareNumberValues(leftValue, rightValue)
}

TryParseDecimalValue(text, &value) {
    value := 0.0
    normalized := NormalizeDecimalText(text)
    if (normalized = "") {
        return false
    }

    value := StrReplace(normalized, ",", ".") + 0.0
    return true
}

SortTextItemsDescending(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        swapped := false
        Loop count - A_Index {
            left := items[A_Index]
            right := items[A_Index + 1]
            if (CompareTextValues(left, right) < 0) {
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

ShortenText(text, maxLength) {
    if (StrLen(text) <= maxLength) {
        return text
    }
    return SubStr(text, 1, maxLength - 1) "…"
}




