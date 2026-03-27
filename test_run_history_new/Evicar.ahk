#Requires AutoHotkey v2.0
#SingleInstance Force
Persistent

global AppTitle := "EviCar"
global DataDir := A_ScriptDir "\data"
global VehiclesFile := DataDir "\vehicles.tsv"
global HistoryFile := DataDir "\history.tsv"
global SettingsFile := DataDir "\settings.ini"
global Categories := ["Osobní vozidla", "Motocykly", "Nákladní vozidla", "Autobusy", "Ostatní"]

global Vehicles := []
global VehicleHistory := []
global VisibleVehicleIds := []
global MainGui := 0
global TabsCtrl := 0
global VehicleListLabel := 0
global MainSearchCtrl := 0
global MainStatusFilterCtrl := 0
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
global SettingsGui := 0
global SettingsControls := {}
global OverviewGui := 0
global OverviewList := 0
global OverviewEntries := []
global OverviewAllEntries := []
global OverviewSummaryLabel := 0
global OverviewFilterCtrl := 0
global OverviewSearchCtrl := 0
global OverviewOpenButton := 0
global OverviewEditButton := 0
global OverviewShowMissingGreenCtrl := 0
global OverviewSortColumn := 6
global OverviewSortDescending := false
global OverdueGui := 0
global OverdueList := 0
global OverdueEntries := []
global OverdueAllEntries := []
global OverdueSummaryLabel := 0
global OverdueSearchCtrl := 0
global OverdueOpenButton := 0
global OverdueEditButton := 0
global HistoryGui := 0
global HistoryVehicleId := ""
global HistoryList := 0
global HistorySummaryLabel := 0
global VisibleHistoryEventIds := []
global HistoryFormGui := 0
global HistoryFormControls := {}
global HistoryFormMode := ""
global HistoryFormEventId := ""
global HistoryFormVehicleId := ""
global DueCheckIntervalMs := 900000
global ResumeDueCheckDelayMs := 1500
global LastTrayIconTip := ""

InitApp()

InitApp() {
    ConfigureAppIdentity()
    EnsureDataFiles()
    LoadVehicles()
    LoadVehicleHistory()
    BuildMainGui()
    RefreshVehicleList()
    SetupTrayMenu()
    RefreshTrayIdentityLater()
    StartAutomaticDueMonitoring()
    CheckDueVehicles(true, false)
}

ConfigureAppIdentity() {
    global AppTitle, LastTrayIconTip

    A_IconTip := AppTitle
    LastTrayIconTip := AppTitle
    try DllCall("shell32\SetCurrentProcessExplicitAppUserModelID", "wstr", "EviCar")

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
    global DueCheckIntervalMs

    OnMessage(0x218, OnPowerBroadcast)
    SetTimer(CheckDueVehiclesTimer, DueCheckIntervalMs)
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

OnPowerBroadcast(wParam, lParam, msg, hwnd) {
    global ResumeDueCheckDelayMs

    if (wParam = 7 || wParam = 18) {
        SetTimer(CheckDueVehiclesAfterResumeTimer, -ResumeDueCheckDelayMs)
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
    global DataDir, VehiclesFile, HistoryFile, SettingsFile

    if !InStr(FileExist(DataDir), "D") {
        DirCreate(DataDir)
    }

    if !FileExist(VehiclesFile) {
        FileAppend("# Evicar data v3`n", VehiclesFile, "UTF-8")
    }

    if !FileExist(HistoryFile) {
        FileAppend("# Evicar history v1`n", HistoryFile, "UTF-8")
    }

    if !FileExist(SettingsFile) {
        IniWrite("31", SettingsFile, "notifications", "technical_reminder_days")
        IniWrite("31", SettingsFile, "notifications", "green_card_reminder_days")
        IniWrite("", SettingsFile, "notifications", "last_alert_day")
        IniWrite("", SettingsFile, "notifications", "last_alert_signature")
        IniWrite("", SettingsFile, "notifications", "last_green_alert_day")
        IniWrite("", SettingsFile, "notifications", "last_green_alert_signature")
        IniWrite("0", SettingsFile, "app", "run_at_startup")
        IniWrite("0", SettingsFile, "app", "hide_on_launch")
        IniWrite("all", SettingsFile, "overview", "filter")
        IniWrite("0", SettingsFile, "overview", "include_missing_green")
        IniWrite("6", SettingsFile, "overview", "sort_column")
        IniWrite("0", SettingsFile, "overview", "sort_descending")
    }

    EnsureSettingsDefaults()
}

BuildMainGui() {
    global AppTitle, Categories, MainGui, TabsCtrl, VehicleListLabel, MainSearchCtrl, MainStatusFilterCtrl, MainClearFiltersButton, VehicleList, StatusBar

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

    MainGui.AddText("xm y78 w160", "Hledat název, značku nebo SPZ")
    MainSearchCtrl := MainGui.AddEdit("x185 y75 w255")
    MainSearchCtrl.OnEvent("Change", OnMainSearchChanged)

    MainGui.AddText("x455 y78 w85", "Filtr seznamu")
    MainStatusFilterCtrl := MainGui.AddDropDownList("x545 y75 w210 Choose1", ["Všechna vozidla", "Jen s blížícím se termínem", "Jen po termínu", "Jen bez zelené karty"])
    MainStatusFilterCtrl.OnEvent("Change", OnMainVehicleFilterChanged)

    MainClearFiltersButton := MainGui.AddButton("x770 y74 w160 h28", "Vymazat filtry")
    MainClearFiltersButton.OnEvent("Click", ClearMainVehicleFilters)

    VehicleList := MainGui.AddListView("xm y108 w930 h245 Grid -Multi", ["Název", "Typ", "Značka / model", "SPZ", "Poslední TK", "Příští TK", "Zelená karta do", "Stav"])
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
    StatusBar.SetParts(300, 630)
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

    showOptions := GetHideOnLaunchEnabled() ? "w955 h495 Hide" : "w955 h495"
    MainGui.Show(showOptions)
}

BuildMainMenuBar() {
    vehicleMenu := Menu()
    vehicleMenu.Add("Přidat vozidlo", AddVehicle)
    vehicleMenu.Add("Upravit vybrané vozidlo", EditSelectedVehicle)
    vehicleMenu.Add("Detail vybraného vozidla", OpenSelectedVehicleDetail)
    vehicleMenu.Add("Historie vybraného vozidla", OpenSelectedVehicleHistory)
    vehicleMenu.Add()
    vehicleMenu.Add("Odstranit vybrané vozidlo", DeleteSelectedVehicle)

    fileMenu := Menu()
    fileMenu.Add("Tiskový přehled", OpenPrintableVehicleReport)
    fileMenu.Add()
    fileMenu.Add("Export dat", ExportAppData)
    fileMenu.Add("Import dat", ImportAppData)
    fileMenu.Add()
    fileMenu.Add("Konec", ExitEvicar)

    overviewMenu := Menu()
    overviewMenu.Add("Přehled termínů", OpenUpcomingOverviewDialog)
    overviewMenu.Add("Propadlé termíny", OpenOverdueDialog)

    toolsMenu := Menu()
    toolsMenu.Add("Nastavení", OpenSettingsDialog)
    toolsMenu.Add("Skrýt do lišty", HideMainWindow)

    mainMenuBar := MenuBar()
    mainMenuBar.Add("&Vozidlo", vehicleMenu)
    mainMenuBar.Add("&Soubor", fileMenu)
    mainMenuBar.Add("Pře&hled", overviewMenu)
    mainMenuBar.Add("&Nástroje", toolsMenu)
    return mainMenuBar
}

OnMainSearchChanged(*) {
    RefreshVehicleList()
}

OnMainVehicleFilterChanged(*) {
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

    haystacks := [
        StrLower(vehicle.name),
        StrLower(vehicle.vehicleType),
        StrLower(vehicle.makeModel),
        StrLower(vehicle.plate)
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

    return false
}

VehicleHasMissingGreenCard(vehicle) {
    return Trim(vehicle.greenCardTo) = ""
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

ExportAppData(*) {
    global AppTitle, A_DefaultDialogTitle

    A_DefaultDialogTitle := AppTitle
    backupPath := FileSelect("S16", GetDefaultBackupPath(), "Export dat EviCar", "EviCar záloha (*.evicarbak)")
    if (backupPath = "") {
        return
    }

    backupPath := EnsureBackupExtension(backupPath)
    try {
        WriteTextFileUtf8(backupPath, BuildBackupContent(GetSettingsContentForBackup(), GetVehiclesContentForBackup(), GetHistoryContentForBackup()))
        MsgBox("Export dat byl dokončen.`n`nSoubor:`n" backupPath, AppTitle, 0x40)
    } catch as err {
        MsgBox("Export dat se nepodařil.`n`n" err.Message, AppTitle, 0x30)
    }
}

ImportAppData(*) {
    global AppTitle, A_DefaultDialogTitle, SettingsFile, VehiclesFile, HistoryFile

    A_DefaultDialogTitle := AppTitle
    backupPath := FileSelect(1, A_ScriptDir, "Import dat EviCar", "EviCar záloha (*.evicarbak)")
    if (backupPath = "") {
        return
    }

    result := MsgBox(
        "Import přepíše aktuální vozidla, historii událostí i nastavení aplikace.`n`nPokračovat v importu?",
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
    errorMessage := ""
    if !TryParseBackupContent(backupContent, &settingsContent, &vehiclesContent, &historyContent, &errorMessage) {
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

    backupDir := BackupCurrentFilesBeforeImport()

    try {
        WriteTextFileUtf8(VehiclesFile, vehiclesContent)
        WriteTextFileUtf8(HistoryFile, historyContent)
        WriteTextFileUtf8(SettingsFile, settingsContent)
        EnsureSettingsDefaults()
        SetRunAtStartupEnabled(IniRead(SettingsFile, "app", "run_at_startup", "0") = "1")
        LoadVehicles()
        LoadVehicleHistory()
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
    global AppTitle, MainGui, FormGui, SettingsGui, SettingsControls, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui

    if IsObject(SettingsGui) {
        WinActivate("ahk_id " SettingsGui.Hwnd)
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

    ShowMainWindow()

    SettingsControls := {}
    SettingsGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Nastavení")
    SettingsGui.SetFont("s10", "Segoe UI")
    SettingsGui.OnEvent("Close", CloseSettingsDialog)
    SettingsGui.OnEvent("Escape", CloseSettingsDialog)

    MainGui.Opt("+Disabled")

    SettingsGui.AddText("x20 y20 w570", "Zde nastavíte samostatně upozornění pro technickou kontrolu i zelenou kartu a také chování aplikace po spuštění.")

    SettingsGui.AddGroupBox("x20 y50 w570 h145", "Upozornění")
    SettingsGui.AddText("x35 y80 w350", "Počet dní pro upozornění na technickou kontrolu (povinné)")
    SettingsControls.technicalReminderDays := SettingsGui.AddEdit("x405 y77 w120 Limit3 Number", GetTechnicalReminderDays())
    SettingsGui.AddText("x35 y115 w350", "Počet dní pro upozornění na platnost zelené karty (povinné)")
    SettingsControls.greenCardReminderDays := SettingsGui.AddEdit("x405 y112 w120 Limit3 Number", GetGreenCardReminderDays())
    SettingsGui.AddText("x35 y150 w520", "Zadejte celé číslo od 1 do 999. Například 31 znamená upozornění přibližně měsíc před koncem.")

    SettingsGui.AddGroupBox("x20 y205 w570 h100", "Aplikace")
    SettingsControls.runAtStartup := SettingsGui.AddCheckBox("x35 y235 w300", "Spustit po startu počítače")
    SettingsControls.runAtStartup.Value := GetRunAtStartupEnabled()
    SettingsControls.hideOnLaunch := SettingsGui.AddCheckBox("x35 y265 w300", "Automaticky skrýt na lištu")
    SettingsControls.hideOnLaunch.Value := GetHideOnLaunchEnabled()

    SettingsGui.AddGroupBox("x20 y315 w570 h80", "Akce")
    saveButton := SettingsGui.AddButton("x320 y343 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveSettingsFromDialog)

    cancelButton := SettingsGui.AddButton("x450 y343 w100 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseSettingsDialog)

    SettingsGui.Show("w610 h415")
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

OpenUpcomingOverviewDialog(*) {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverviewList, OverviewEntries, OverviewAllEntries, OverviewSummaryLabel, OverviewFilterCtrl, OverviewSearchCtrl, OverviewOpenButton, OverviewEditButton, OverviewShowMissingGreenCtrl, OverviewSortColumn, OverviewSortDescending, OverdueGui, DetailGui, HistoryGui, HistoryFormGui

    if IsObject(OverviewGui) {
        WinActivate("ahk_id " OverviewGui.Hwnd)
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

    ShowMainWindow()

    OverviewSortColumn := GetOverviewSortColumnSetting()
    OverviewSortDescending := GetOverviewSortDescendingSetting()
    OverviewAllEntries := BuildUpcomingOverviewEntries(GetOverviewIncludeMissingGreenSetting())
    OverviewEntries := []
    OverviewGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Přehled termínů")
    OverviewGui.SetFont("s10", "Segoe UI")
    OverviewGui.OnEvent("Close", CloseUpcomingOverviewDialog)
    OverviewGui.OnEvent("Escape", CloseUpcomingOverviewDialog)

    MainGui.Opt("+Disabled")

    OverviewGui.AddText("x20 y20 w780", "Zde vidíte všechny blížící se a propadlé termíny technických kontrol a zelených karet podle aktuálního nastavení upozornění.")
    OverviewGui.AddText("x20 y55 w90", "Filtr zobrazení")
    OverviewFilterCtrl := OverviewGui.AddDropDownList("x120 y52 w220 Choose1", ["Vše", "Jen technické kontroly", "Jen zelené karty"])
    OverviewFilterCtrl.Value := GetOverviewFilterIndex()
    OverviewFilterCtrl.OnEvent("Change", OnOverviewFilterChanged)

    OverviewShowMissingGreenCtrl := OverviewGui.AddCheckBox("x350 y54 w280", "Zobrazit i vozidla bez zelené karty")
    OverviewShowMissingGreenCtrl.Value := GetOverviewIncludeMissingGreenSetting()
    OverviewShowMissingGreenCtrl.OnEvent("Click", OnOverviewShowMissingGreenChanged)

    refreshButton := OverviewGui.AddButton("x650 y50 w150 h28", "Obnovit")
    refreshButton.OnEvent("Click", RefreshUpcomingOverviewDialog)

    OverviewGui.AddText("x20 y88 w140", "Hledat název nebo SPZ")
    OverviewSearchCtrl := OverviewGui.AddEdit("x170 y85 w300")
    OverviewSearchCtrl.OnEvent("Change", OnOverviewSearchChanged)

    OverviewSummaryLabel := OverviewGui.AddText("x20 y118 w780", "")

    OverviewList := OverviewGui.AddListView("x20 y148 w780 h187 Grid -Multi", ["Druh", "Vozidlo", "Kategorie", "Značka / model", "SPZ", "Termín", "Stav"])
    OverviewList.OnEvent("DoubleClick", OpenSelectedOverviewVehicle)
    OverviewList.OnEvent("ColClick", OnOverviewColumnClick)

    OverviewList.ModifyCol(1, "120")
    OverviewList.ModifyCol(2, "145")
    OverviewList.ModifyCol(3, "120")
    OverviewList.ModifyCol(4, "145")
    OverviewList.ModifyCol(5, "85")
    OverviewList.ModifyCol(6, "75")
    OverviewList.ModifyCol(7, "90")

    OverviewEditButton := OverviewGui.AddButton("x250 y350 w170 h30", "Upravit vozidlo")
    OverviewEditButton.OnEvent("Click", EditSelectedOverviewVehicle)

    OverviewOpenButton := OverviewGui.AddButton("x430 y350 w170 h30 Default", "Zobrazit vozidlo")
    OverviewOpenButton.OnEvent("Click", OpenSelectedOverviewVehicle)

    closeButton := OverviewGui.AddButton("x610 y350 w110 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseUpcomingOverviewDialog)

    OverviewGui.Show("w820 h405")
    PopulateUpcomingOverviewList("", true)
    if (OverviewEntries.Length > 0) {
        ; selection and focus are handled by PopulateUpcomingOverviewList
    } else {
        closeButton.Focus()
    }
}

CloseUpcomingOverviewDialog(*) {
    global OverviewGui, OverviewList, OverviewEntries, OverviewAllEntries, OverviewSummaryLabel, OverviewFilterCtrl, OverviewSearchCtrl, OverviewOpenButton, OverviewEditButton, OverviewShowMissingGreenCtrl, OverviewSortColumn, OverviewSortDescending, MainGui

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
    OverviewOpenButton := 0
    OverviewEditButton := 0
    OverviewShowMissingGreenCtrl := 0
    OverviewSortColumn := 6
    OverviewSortDescending := false
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

OpenOverdueDialog(*) {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, OverdueList, OverdueEntries, OverdueAllEntries, OverdueSummaryLabel, OverdueSearchCtrl, OverdueOpenButton, OverdueEditButton, DetailGui, HistoryGui, HistoryFormGui

    if IsObject(OverdueGui) {
        WinActivate("ahk_id " OverdueGui.Hwnd)
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

    ShowMainWindow()

    OverdueAllEntries := BuildOverdueEntries()
    OverdueEntries := []
    OverdueGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Propadlé termíny")
    OverdueGui.SetFont("s10", "Segoe UI")
    OverdueGui.OnEvent("Close", CloseOverdueDialog)
    OverdueGui.OnEvent("Escape", CloseOverdueDialog)

    MainGui.Opt("+Disabled")

    OverdueGui.AddText("x20 y20 w780", "Zde vidíte všechny už propadlé technické kontroly a zelené karty.")

    refreshButton := OverdueGui.AddButton("x650 y50 w150 h28", "Obnovit")
    refreshButton.OnEvent("Click", RefreshOverdueDialog)

    OverdueGui.AddText("x20 y55 w140", "Hledat název nebo SPZ")
    OverdueSearchCtrl := OverdueGui.AddEdit("x170 y52 w300")
    OverdueSearchCtrl.OnEvent("Change", OnOverdueSearchChanged)

    OverdueSummaryLabel := OverdueGui.AddText("x20 y90 w780", "")

    OverdueList := OverdueGui.AddListView("x20 y120 w780 h215 Grid -Multi", ["Druh", "Vozidlo", "Kategorie", "Značka / model", "SPZ", "Termín", "Stav"])
    OverdueList.OnEvent("DoubleClick", OpenSelectedOverdueVehicle)
    OverdueList.ModifyCol(1, "120")
    OverdueList.ModifyCol(2, "145")
    OverdueList.ModifyCol(3, "120")
    OverdueList.ModifyCol(4, "145")
    OverdueList.ModifyCol(5, "85")
    OverdueList.ModifyCol(6, "75")
    OverdueList.ModifyCol(7, "90")

    OverdueEditButton := OverdueGui.AddButton("x250 y350 w170 h30", "Upravit vozidlo")
    OverdueEditButton.OnEvent("Click", EditSelectedOverdueVehicle)

    OverdueOpenButton := OverdueGui.AddButton("x430 y350 w170 h30 Default", "Zobrazit vozidlo")
    OverdueOpenButton.OnEvent("Click", OpenSelectedOverdueVehicle)

    closeButton := OverdueGui.AddButton("x610 y350 w110 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseOverdueDialog)

    OverdueGui.Show("w820 h405")
    PopulateOverdueList("", true)
    if (OverdueEntries.Length = 0) {
        closeButton.Focus()
    }
}

CloseOverdueDialog(*) {
    global OverdueGui, OverdueList, OverdueEntries, OverdueAllEntries, OverdueSummaryLabel, OverdueSearchCtrl, OverdueOpenButton, OverdueEditButton, MainGui

    if IsObject(OverdueGui) {
        OverdueGui.Destroy()
        OverdueGui := 0
    }

    OverdueList := 0
    OverdueEntries := []
    OverdueAllEntries := []
    OverdueSummaryLabel := 0
    OverdueSearchCtrl := 0
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
    global OverdueAllEntries, OverdueEntries, OverdueList, OverdueSummaryLabel, OverdueOpenButton, OverdueEditButton

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

GetSelectedOverdueVehicle(actionLabel := "otevřít") {
    global AppTitle, OverdueList, OverdueEntries

    if !IsObject(OverdueList) {
        return ""
    }

    row := OverdueList.GetNext(0)
    if !row || row > OverdueEntries.Length {
        MsgBox("Nejprve vyberte propadlý termín, který chcete " actionLabel ".", AppTitle, 0x40)
        return ""
    }

    return OverdueEntries[row].vehicle
}

OpenSelectedOverdueVehicle(*) {
    vehicle := GetSelectedOverdueVehicle("otevřít")
    if !IsObject(vehicle) {
        return
    }

    CloseOverdueDialog()
    OpenVehicleById(vehicle.id, true)
}

EditSelectedOverdueVehicle(*) {
    vehicle := GetSelectedOverdueVehicle("upravit")
    if !IsObject(vehicle) {
        return
    }

    CloseOverdueDialog()
    OpenVehicleForm("edit", vehicle)
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

    SortUpcomingByDue(&entries)
    return entries
}

BuildOverdueSummary(entries, allEntries) {
    vehicleIds := Map()
    overdueTechnical := 0
    overdueGreen := 0

    for entry in allEntries {
        vehicleIds[entry.vehicle.id] := true
        if (entry.kind = "technical") {
            overdueTechnical += 1
        } else {
            overdueGreen += 1
        }
    }

    if (allEntries.Length = 0) {
        return "Momentálně nejsou žádné propadlé technické kontroly ani zelené karty."
    }

    text := "Propadlých položek: " allEntries.Length " u " vehicleIds.Count " vozidel. "
    text .= "TK po termínu: " overdueTechnical ". ZK po termínu: " overdueGreen "."
    if (entries.Length != allEntries.Length) {
        text .= " Zobrazeno po hledání: " entries.Length "."
    }

    return text
}

OpenPrintableVehicleReport(*) {
    global AppTitle

    reportPath := A_Temp "\EviCar_tiskovy_prehled_" FormatTime(A_Now, "yyyyMMdd_HHmmss") ".html"
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
    html .= "<title>EviCar - Tiskový přehled vozidel</title>"
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
    html .= "<h1>EviCar - Tiskový přehled vozidel</h1>"
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
        rows.Push(
            "<tr>"
            "<td>" HtmlEscape(vehicle.name) "</td>"
            "<td>" HtmlEscape(vehicle.vehicleType) "</td>"
            "<td>" HtmlEscape(vehicle.makeModel) "</td>"
            "<td>" HtmlEscape(vehicle.plate) "</td>"
            "<td>" HtmlEscape(vehicle.year) "</td>"
            "<td>" HtmlEscape(vehicle.power) "</td>"
            "<td>" HtmlEscape(vehicle.lastTk) "</td>"
            "<td>" HtmlEscape(vehicle.nextTk) "</td>"
            "<td>" HtmlEscape(vehicle.greenCardTo) "</td>"
            "<td>" HtmlEscape(GetVehicleStatusText(vehicle)) "</td>"
            "</tr>"
        )
    }

    html := title
    html .= "<table>"
    html .= "<thead><tr><th>Název</th><th>Typ</th><th>Značka / model</th><th>SPZ</th><th>Rok výroby</th><th>Výkon</th><th>Poslední TK</th><th>Příští TK</th><th>Zelená karta do</th><th>Stav</th></tr></thead>"
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

OpenSelectedOverviewVehicle(*) {
    vehicle := GetSelectedOverviewVehicle("otevřít")
    if !IsObject(vehicle) {
        return
    }

    CloseUpcomingOverviewDialog()
    OpenVehicleById(vehicle.id, true)
}

EditSelectedOverviewVehicle(*) {
    vehicle := GetSelectedOverviewVehicle("upravit")
    if !IsObject(vehicle) {
        return
    }

    CloseUpcomingOverviewDialog()
    OpenVehicleForm("edit", vehicle)
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
    OverviewAllEntries := BuildUpcomingOverviewEntries(ShouldShowMissingGreenCardsInOverview())
    PopulateUpcomingOverviewList(selectedKey)
}

PopulateUpcomingOverviewList(selectedKey := "", focusList := false) {
    global OverviewAllEntries, OverviewEntries, OverviewList, OverviewSummaryLabel, OverviewOpenButton, OverviewEditButton

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

GetSelectedOverviewVehicle(actionLabel := "otevřít") {
    global AppTitle, OverviewList, OverviewEntries

    if !IsObject(OverviewList) {
        return ""
    }

    row := OverviewList.GetNext(0)
    if !row || row > OverviewEntries.Length {
        MsgBox("Nejprve vyberte termín, který chcete " actionLabel ".", AppTitle, 0x40)
        return ""
    }

    return OverviewEntries[row].vehicle
}

ShouldShowMissingGreenCardsInOverview() {
    global OverviewShowMissingGreenCtrl

    if IsObject(OverviewShowMissingGreenCtrl) {
        return OverviewShowMissingGreenCtrl.Value = 1
    }

    return GetOverviewIncludeMissingGreenSetting()
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

    return "all"
}

GetOverviewFilterSetting() {
    global SettingsFile

    value := IniRead(SettingsFile, "overview", "filter", "all")
    if (value != "technical" && value != "green") {
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

    return 1
}

GetOverviewIncludeMissingGreenSetting() {
    global SettingsFile

    return IniRead(SettingsFile, "overview", "include_missing_green", "0") = "1" ? 1 : 0
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

    if (filterKind != "technical" && filterKind != "green") {
        filterKind := "all"
    }

    IniWrite(filterKind, SettingsFile, "overview", "filter")
}

SaveOverviewIncludeMissingGreenSetting(enabled) {
    global SettingsFile

    IniWrite(enabled ? "1" : "0", SettingsFile, "overview", "include_missing_green")
}

SaveOverviewSortSettings(column, descending) {
    global SettingsFile

    if (column < 1 || column > 7) {
        column := 6
    }

    IniWrite(column, SettingsFile, "overview", "sort_column")
    IniWrite(descending ? "1" : "0", SettingsFile, "overview", "sort_descending")
}

FilterUpcomingOverviewEntries(entries, filterKind := "all") {
    filtered := []

    for entry in entries {
        if (filterKind = "all" || entry.kind = filterKind) {
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
        if (InStr(StrLower(entry.vehicle.name), needle) || InStr(StrLower(entry.vehicle.plate), needle)) {
            filtered.Push(entry)
        }
    }

    return filtered
}

BuildOverviewEntryKey(entry) {
    return entry.kind "|" entry.vehicle.id "|" entry.term
}

SaveSettingsFromDialog(*) {
    global AppTitle, SettingsControls, SettingsFile

    technicalReminderDays := ValidateReminderDaysSetting(SettingsControls.technicalReminderDays, "Počet dní pro upozornění na technickou kontrolu")
    if (technicalReminderDays = "") {
        return
    }

    greenCardReminderDays := ValidateReminderDaysSetting(SettingsControls.greenCardReminderDays, "Počet dní pro upozornění na platnost zelené karty")
    if (greenCardReminderDays = "") {
        return
    }

    runAtStartup := SettingsControls.runAtStartup.Value ? 1 : 0
    hideOnLaunch := SettingsControls.hideOnLaunch.Value ? 1 : 0

    if !SetRunAtStartupEnabled(runAtStartup) {
        return
    }

    IniWrite(technicalReminderDays, SettingsFile, "notifications", "technical_reminder_days")
    IniWrite(greenCardReminderDays, SettingsFile, "notifications", "green_card_reminder_days")
    IniWrite(runAtStartup, SettingsFile, "app", "run_at_startup")
    IniWrite(hideOnLaunch, SettingsFile, "app", "hide_on_launch")
    ResetAlertHistory()
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
    message := "Opravdu chcete odstranit vozidlo: " vehicle.name "?"
    if (historyCount > 0) {
        message .= "`n`nSoučasně bude odstraněno i " historyCount " záznamů z historie událostí."
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

OpenVehicleDetailDialog(vehicle) {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, DetailVehicleId, DetailRecentHistoryList, DetailHistorySummaryLabel, HistoryGui, HistoryFormGui

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

    ShowMainWindow()

    DetailVehicleId := vehicle.id
    DetailGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Detail vozidla")
    DetailGui.SetFont("s10", "Segoe UI")
    DetailGui.OnEvent("Close", CloseVehicleDetailDialog)
    DetailGui.OnEvent("Escape", CloseVehicleDetailDialog)

    MainGui.Opt("+Disabled")

    DetailGui.AddText("x20 y20 w660", "Zde vidíte souhrn všech údajů o vybraném vozidle a poslední záznamy z jeho historie.")

    DetailGui.AddGroupBox("x20 y50 w660 h150", "Základní údaje")
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

    DetailGui.AddGroupBox("x20 y210 w660 h110", "Platnost a termíny")
    DetailGui.AddText("x35 y240 w130", "Poslední TK")
    DetailGui.AddText("x170 y240 w150", FormatDisplayValue(vehicle.lastTk))
    DetailGui.AddText("x355 y240 w110", "Příští TK")
    DetailGui.AddText("x470 y240 w180", FormatDisplayValue(vehicle.nextTk))
    DetailGui.AddText("x35 y270 w130", "Stav TK")
    DetailGui.AddText("x170 y270 w150", FormatDisplayValue(GetExpirationStatusText(vehicle.nextTk, GetTechnicalReminderDays()), "V pořádku"))
    DetailGui.AddText("x355 y270 w110", "Zelená karta")
    DetailGui.AddText("x470 y270 w180", BuildGreenCardRangeText(vehicle))
    DetailGui.AddText("x35 y300 w130", "Stav ZK")
    DetailGui.AddText("x170 y300 w480", FormatDisplayValue(GetExpirationStatusText(vehicle.greenCardTo, GetGreenCardReminderDays()), vehicle.greenCardTo = "" ? "Nevyplněno" : "V pořádku"))

    DetailGui.AddGroupBox("x20 y330 w660 h155", "Poslední události")
    DetailHistorySummaryLabel := DetailGui.AddText("x35 y360 w625", BuildVehicleHistorySummaryText(vehicle.id))
    DetailRecentHistoryList := DetailGui.AddListView("x35 y388 w625 h70 Grid -Multi", ["Datum", "Událost", "Km", "Cena"])
    DetailRecentHistoryList.ModifyCol(1, "85")
    DetailRecentHistoryList.ModifyCol(2, "220")
    DetailRecentHistoryList.ModifyCol(3, "110")
    DetailRecentHistoryList.ModifyCol(4, "190")
    PopulateVehicleDetailHistoryList(vehicle.id)

    editButton := DetailGui.AddButton("x180 y500 w130 h30", "Upravit vozidlo")
    editButton.OnEvent("Click", EditVehicleFromDetail)

    historyButton := DetailGui.AddButton("x320 y500 w150 h30", "Historie událostí")
    historyButton.OnEvent("Click", OpenHistoryFromDetail)

    addEventButton := DetailGui.AddButton("x480 y500 w140 h30", "Přidat událost")
    addEventButton.OnEvent("Click", AddHistoryEventFromDetail)

    closeButton := DetailGui.AddButton("x625 y500 w70 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleDetailDialog)

    DetailGui.Show("w700 h550")
    closeButton.Focus()
}

CloseVehicleDetailDialog(*) {
    global DetailGui, DetailVehicleId, DetailRecentHistoryList, DetailHistorySummaryLabel, MainGui

    if IsObject(DetailGui) {
        DetailGui.Destroy()
        DetailGui := 0
    }

    DetailVehicleId := ""
    DetailRecentHistoryList := 0
    DetailHistorySummaryLabel := 0
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

AddHistoryEventFromDetail(*) {
    global DetailVehicleId

    vehicle := FindVehicleById(DetailVehicleId)
    if !IsObject(vehicle) {
        return
    }

    CloseVehicleDetailDialog()
    OpenVehicleHistoryDialog(vehicle, true)
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
    global AppTitle, Categories, FormGui, FormControls, FormMode, FormVehicleId, MainGui, TabsCtrl, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryFormGui

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
    rowY += rowStep + 10

    FormGui.AddText("x20 y" rowY " w500", "Datum zadávejte jako MM/RRRR, například 04/2026. Pro upozornění se používají pole Příští TK a Zelená karta do.")

    saveButton := FormGui.AddButton(Format("x185 y{} w140 h30 Default", rowY + 45), "Uložit")
    saveButton.OnEvent("Click", SaveVehicleFromForm)

    cancelButton := FormGui.AddButton(Format("x335 y{} w140 h30", rowY + 45), "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleForm)

    if IsObject(vehicle) {
        FormControls.name.Text := vehicle.name
        SetDropDownToText(FormControls.category, vehicle.category)
        FormControls.vehicleType.Text := vehicle.vehicleType
        FormControls.makeModel.Text := vehicle.makeModel
        FormControls.plate.Text := vehicle.plate
        FormControls.year.Text := vehicle.year
        FormControls.power.Text := vehicle.power
        FormControls.lastTk.Text := vehicle.lastTk
        FormControls.nextTk.Text := vehicle.nextTk
        FormControls.greenCardFrom.Text := vehicle.greenCardFrom
        FormControls.greenCardTo.Text := vehicle.greenCardTo
    } else {
        FormControls.category.Value := TabsCtrl.Value
    }

    FormGui.Show("w550 h565")
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
    CloseVehicleForm()
    OpenVehicleById(vehicle.id, true)
    CheckDueVehicles(false, false)
}

OpenVehicleHistoryDialog(vehicle, openAddEvent := false) {
    global AppTitle, MainGui, FormGui, SettingsGui, OverviewGui, OverdueGui, DetailGui, HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, VisibleHistoryEventIds, HistoryFormGui

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

    ShowMainWindow()

    HistoryVehicleId := vehicle.id
    VisibleHistoryEventIds := []
    HistoryGui := Gui("+Owner" MainGui.Hwnd, AppTitle " - Historie událostí")
    HistoryGui.SetFont("s10", "Segoe UI")
    HistoryGui.OnEvent("Close", CloseVehicleHistoryDialog)
    HistoryGui.OnEvent("Escape", CloseVehicleHistoryDialog)

    MainGui.Opt("+Disabled")

    HistoryGui.AddText("x20 y20 w780", "Zde můžete vést servisní a další události k vozidlu " vehicle.name ". Datum události se zadává jako DD.MM.RRRR.")
    HistorySummaryLabel := HistoryGui.AddText("x20 y50 w780", "")

    HistoryList := HistoryGui.AddListView("x20 y80 w780 h250 Grid -Multi", ["Datum", "Událost", "Km", "Cena", "Poznámka"])
    HistoryList.OnEvent("DoubleClick", EditSelectedVehicleHistoryEvent)
    HistoryList.ModifyCol(1, "95")
    HistoryList.ModifyCol(2, "190")
    HistoryList.ModifyCol(3, "95")
    HistoryList.ModifyCol(4, "100")
    HistoryList.ModifyCol(5, "280")

    addButton := HistoryGui.AddButton("x95 y345 w120 h30", "Přidat událost")
    addButton.OnEvent("Click", AddVehicleHistoryEvent)

    editButton := HistoryGui.AddButton("x225 y345 w120 h30", "Upravit událost")
    editButton.OnEvent("Click", EditSelectedVehicleHistoryEvent)

    deleteButton := HistoryGui.AddButton("x355 y345 w120 h30", "Odstranit událost")
    deleteButton.OnEvent("Click", DeleteSelectedVehicleHistoryEvent)

    detailButton := HistoryGui.AddButton("x485 y345 w120 h30", "Detail vozidla")
    detailButton.OnEvent("Click", OpenVehicleDetailFromHistory)

    closeButton := HistoryGui.AddButton("x615 y345 w100 h30", "Zavřít")
    closeButton.OnEvent("Click", CloseVehicleHistoryDialog)

    HistoryGui.Show("w820 h395")
    PopulateVehicleHistoryList("", true)

    if openAddEvent {
        OpenVehicleHistoryEventForm("add")
    } else if (VisibleHistoryEventIds.Length = 0) {
        addButton.Focus()
    }
}

CloseVehicleHistoryDialog(*) {
    global HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, VisibleHistoryEventIds, MainGui

    if IsObject(HistoryGui) {
        HistoryGui.Destroy()
        HistoryGui := 0
    }

    HistoryVehicleId := ""
    HistoryList := 0
    HistorySummaryLabel := 0
    VisibleHistoryEventIds := []
    MainGui.Opt("-Disabled")
    ShowMainWindow()
}

PopulateVehicleHistoryList(selectEventId := "", focusList := false) {
    global HistoryGui, HistoryVehicleId, HistoryList, HistorySummaryLabel, VisibleHistoryEventIds

    if !IsObject(HistoryGui) || !IsObject(HistoryList) {
        return
    }

    vehicle := FindVehicleById(HistoryVehicleId)
    events := GetVehicleHistoryEntries(HistoryVehicleId)
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
    HistoryFormControls.note := HistoryFormGui.AddEdit("x20 y225 w410 h95 Multi WantTab")

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

    if (firstNonEmptyLine != "# Evicar data v3") {
        ShowVehiclesFileFormatError("Soubor vozidel není v podporovaném formátu. EviCar očekává hlavičku '# Evicar data v3'.")
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
            ShowVehiclesFileFormatError("Soubor vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index ". EviCar očekává jen jednu hlavičku '# Evicar data v3'.")
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

    lines := ["# Evicar data v3"]
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

    if (firstNonEmptyLine != "# Evicar history v1") {
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

    lines := ["# Evicar history v1"]
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

    lines := ["# Evicar data v3"]
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

GetDefaultBackupPath() {
    timestamp := FormatTime(A_Now, "yyyy-MM-dd_HH-mm")
    return A_ScriptDir "\EviCar_zaloha_" timestamp ".evicarbak"
}

EnsureBackupExtension(path) {
    if (StrLower(SubStr(path, -10)) != ".evicarbak") {
        path .= ".evicarbak"
    }

    return path
}

BuildBackupContent(settingsContent, vehiclesContent, historyContent := "") {
    settingsContent := NormalizeTextForStorage(settingsContent)
    vehiclesContent := NormalizeTextForStorage(vehiclesContent)
    historyContent := NormalizeTextForStorage(historyContent)

    header := JoinLines([
        "# EviCar backup v2",
        "settings_length=" StrLen(settingsContent),
        "vehicles_length=" StrLen(vehiclesContent),
        "history_length=" StrLen(historyContent)
    ])

    return header "`n`n" settingsContent vehiclesContent historyContent
}

TryParseBackupContent(content, &settingsContent, &vehiclesContent, &historyContent, &errorMessage) {
    settingsContent := ""
    vehiclesContent := ""
    historyContent := ""
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
        errorMessage := "Soubor není ve formátu zálohy EviCar."
        return false
    }

    backupVersion := headerLines[1]
    if (backupVersion != "# EviCar backup v1" && backupVersion != "# EviCar backup v2") {
        errorMessage := "Soubor není ve formátu zálohy EviCar."
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
    if (backupVersion = "# EviCar backup v2") {
        if (headerLines.Length < 4 || !RegExMatch(headerLines[4], "^history_length=(\d+)$", &historyMatch)) {
            errorMessage := "Soubor zálohy neobsahuje délku historie."
            return false
        }
        historyLength := historyMatch[1] + 0
    }

    if (settingsLength < 0 || vehiclesLength < 0 || historyLength < 0) {
        errorMessage := "Soubor zálohy obsahuje neplatné délky dat."
        return false
    }

    if (StrLen(payload) != settingsLength + vehiclesLength + historyLength) {
        errorMessage := "Soubor zálohy je neúplný nebo poškozený."
        return false
    }

    settingsContent := SubStr(payload, 1, settingsLength)
    vehiclesContent := SubStr(payload, settingsLength + 1, vehiclesLength)
    if (backupVersion = "# EviCar backup v2") {
        historyContent := SubStr(payload, settingsLength + vehiclesLength + 1, historyLength)
    } else {
        historyContent := "# Evicar history v1`n"
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

    if (firstNonEmptyLine != "# Evicar data v3") {
        errorMessage := "Soubor vozidel není v podporovaném formátu. EviCar očekává hlavičku '# Evicar data v3'."
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

    if (firstNonEmptyLine != "# Evicar history v1") {
        errorMessage := "Soubor historie není v podporovaném formátu. EviCar očekává hlavičku '# Evicar history v1'."
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

BackupCurrentFilesBeforeImport() {
    global DataDir, VehiclesFile, HistoryFile, SettingsFile

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
    EnsureIniKeyExists(SettingsFile, "app", "run_at_startup", "0")
    EnsureIniKeyExists(SettingsFile, "app", "hide_on_launch", "0")
    EnsureIniKeyExists(SettingsFile, "overview", "filter", "all")
    EnsureIniKeyExists(SettingsFile, "overview", "include_missing_green", "0")
    EnsureIniKeyExists(SettingsFile, "overview", "sort_column", "6")
    EnsureIniKeyExists(SettingsFile, "overview", "sort_descending", "0")
}

EnsureIniKeyExists(path, section, key, defaultValue) {
    missingMarker := "__EVICAR_MISSING__"
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
    selectedRow := 0
    searchText := GetMainSearchText()
    filterKind := GetMainVehicleFilterKind()

    for vehicle in Vehicles {
        if (vehicle.category = category) {
            categoryItems.Push(vehicle)
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

    UpdateVehicleListLabel(items.Length, categoryItems.Length, category)
    UpdateStatusBar()
    SetupTrayMenu()
}

UpdateVehicleListLabel(itemCount, totalCount, category) {
    global VehicleListLabel

    if !IsObject(VehicleListLabel) {
        return
    }

    if (itemCount = totalCount) {
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

CheckDueVehicles(showTrayNotification := true, forceMessageBox := false) {
    global AppTitle

    upcoming := GetUpcomingVehicles()
    greenCards := GetUpcomingGreenCards()
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

    message := BuildAutomaticReminderMessage(upcoming, greenCards, showTechnicalAlert, showGreenAlert)
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

BuildUpcomingOverviewEntries(includeMissingGreenCards := false) {
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

    return entries
}

BuildUpcomingOverviewSummary(entries, allEntries := "") {
    technicalCount := 0
    greenCount := 0
    totalCount := IsObject(allEntries) ? allEntries.Length : entries.Length

    for entry in entries {
        if (entry.kind = "technical") {
            technicalCount += 1
        } else if (entry.kind = "green") {
            greenCount += 1
        }
    }

    missingGreenCount := GetMissingGreenCardCount()
    if (totalCount = 0) {
        summary := "Momentálně není žádný blížící se ani propadlý termín, který by podle aktuálního nastavení vyžadoval pozornost."
    } else if (entries.Length = totalCount) {
        summary := "Celkem " entries.Length " termínů k pozornosti: " technicalCount " technických kontrol a " greenCount " zelených karet."
    } else {
        summary := "Zobrazeno " entries.Length " z " totalCount " termínů: " technicalCount " technických kontrol a " greenCount " zelených karet."
    }

    if (missingGreenCount > 0) {
        if ShouldShowMissingGreenCardsInOverview() {
            summary .= " Je zapnuto i zobrazení vozidel bez vyplněné zelené karty."
        } else {
            summary .= " U " missingGreenCount " vozidel není zelená karta vyplněná."
        }
    }

    return summary
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

ValidateReminderDaysSetting(ctrl, fieldLabel) {
    global AppTitle

    value := Trim(ctrl.Text)
    if !RegExMatch(value, "^\d{1,3}$") {
        MsgBox(fieldLabel " musí být celé číslo od 1 do 999.", AppTitle, 0x30)
        ctrl.Focus()
        return ""
    }

    value += 0
    if (value < 1 || value > 999) {
        MsgBox(fieldLabel " musí být v rozsahu od 1 do 999.", AppTitle, 0x30)
        ctrl.Focus()
        return ""
    }

    return value
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
        MsgBox("Nepodařilo se " action " spuštění EviCar po startu počítače.`n`n" err.Message, AppTitle, 0x30)
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

BuildAutomaticReminderMessage(upcoming, greenCards, showTechnicalAlert := false, showGreenAlert := false) {
    if (showTechnicalAlert && showGreenAlert) {
        return BuildCombinedReminderMessage(upcoming, greenCards)
    }
    if showTechnicalAlert {
        return BuildReminderMessage(upcoming)
    }
    if showGreenAlert {
        return BuildGreenCardReminderMessage(greenCards)
    }

    return ""
}

BuildCombinedReminderMessage(upcoming, greenCards) {
    lines := []

    if (upcoming.Length > 0) {
        lines.Push(BuildReminderSummaryLine(upcoming))
    }
    if (greenCards.Length > 0) {
        lines.Push(BuildGreenCardReminderSummaryLine(greenCards))
    }
    if (greenCards.Length > 0 && HasAnyMissingGreenCard()) {
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

ShouldShowAlert(signature, kind := "technical") {
    global SettingsFile

    today := SubStr(A_Now, 1, 8)
    if (kind = "green") {
        dayKey := "last_green_alert_day"
        signatureKey := "last_green_alert_signature"
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

    menu.Add("Zkontrolovat technické kontroly", ManualDueCheck)
    menu.Add("Zkontrolovat zelené karty", ManualGreenCardCheck)
    menu.Add("Přehled všech termínů", OpenUpcomingOverviewDialog)
    menu.Add("Propadlé termíny", OpenOverdueDialog)
    menu.Add("Tiskový přehled", OpenPrintableVehicleReport)
    menu.Add("Export dat", ExportAppData)
    menu.Add("Import dat", ImportAppData)
    menu.Add("Nastavení", OpenSettingsDialog)
    menu.Add()
    menu.Add("Konec", ExitEvicar)
    menu.Default := "Otevřít " AppTitle
    menu.ClickCount := 1
    UpdateTrayIconTip()
}

ExitEvicar(*) {
    ExitApp()
}

UpdateStatusBar() {
    global StatusBar, Vehicles

    if !IsObject(StatusBar) {
        return
    }

    category := GetCurrentCategory()
    count := 0
    for vehicle in Vehicles {
        if (vehicle.category = category) {
            count += 1
        }
    }

    StatusBar.SetText(category ": " count " vozidel", 1)

    upcoming := GetUpcomingVehicles()
    greenCards := GetUpcomingGreenCards()

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

    StatusBar.SetText(tkText " | " greenText, 2)
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
        && counts.upcomingTechnical = 0
        && counts.upcomingGreen = 0
    ) {
        return AppTitle
    }

    return AppTitle
        . " - po termínu "
        . counts.overdueTechnical " TK / " counts.overdueGreen " ZK"
        . ", brzy vyprší "
        . counts.upcomingTechnical " TK / " counts.upcomingGreen " ZK"
}

GetTrayAttentionCounts() {
    global Vehicles

    counts := {
        overdueTechnical: 0,
        overdueGreen: 0,
        upcomingTechnical: 0,
        upcomingGreen: 0
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
        return IsLeapYear(year) ? 29 : 28
    }

    return 30
}

IsLeapYear(year) {
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

SetDropDownToText(ctrl, wantedText) {
    global Categories

    wantedText := NormalizeCategory(wantedText)
    for index, item in Categories {
        if (item = wantedText) {
            ctrl.Value := index
            return
        }
    }
    ctrl.Value := Categories.Length
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

ShortenText(text, maxLength) {
    if (StrLen(text) <= maxLength) {
        return text
    }
    return SubStr(text, 1, maxLength - 1) "…"
}
