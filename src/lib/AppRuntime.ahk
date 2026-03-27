IsVehimapTestMode() {
    return (EnvGet("VEHIMAP_TEST_MODE") = "1") || (IsSet(VehimapTestMode) && VehimapTestMode)
}

GetVehimapTestHooks() {
    global VehimapTestHooks

    if IsVehimapTestMode() && IsSet(VehimapTestHooks) && IsObject(VehimapTestHooks) {
        return VehimapTestHooks
    }

    return 0
}

AppMsgBox(text, title := "", options := 0) {
    hooks := GetVehimapTestHooks()

    if IsObject(hooks) {
        if !hooks.HasOwnProp("messages") || !IsObject(hooks.messages) {
            hooks.messages := []
        }

        hooks.messages.Push({
            text: text,
            title: title,
            options: options
        })

        if hooks.HasOwnProp("msgBoxResults") && IsObject(hooks.msgBoxResults) && hooks.msgBoxResults.Length > 0 {
            return hooks.msgBoxResults.RemoveAt(1)
        }

        return "OK"
    }

    return MsgBox(text, title, options)
}

AppRun(command, workingDir := "", options := "") {
    hooks := GetVehimapTestHooks()

    if IsObject(hooks) {
        if !hooks.HasOwnProp("runs") || !IsObject(hooks.runs) {
            hooks.runs := []
        }

        hooks.runs.Push({
            command: command,
            workingDir: workingDir,
            options: options
        })
        return 0
    }

    return Run(command, workingDir, options)
}

AppExit() {
    hooks := GetVehimapTestHooks()

    if IsObject(hooks) {
        hooks.exitRequested := true
        return
    }

    ExitApp()
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
