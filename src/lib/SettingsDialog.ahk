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

    SettingsGui.AddGroupBox("x20 y50 w570 h215", "Upozornění")
    SettingsGui.AddText("x35 y80 w350", "Počet dní pro upozornění na technickou kontrolu (povinné)")
    SettingsControls.technicalReminderDays := SettingsGui.AddEdit("x405 y77 w120 Limit3 Number", GetTechnicalReminderDays())
    SettingsGui.AddText("x35 y115 w350", "Počet dní pro upozornění na platnost zelené karty (povinné)")
    SettingsControls.greenCardReminderDays := SettingsGui.AddEdit("x405 y112 w120 Limit3 Number", GetGreenCardReminderDays())
    SettingsGui.AddText("x35 y150 w350", "Počet dní pro upozornění na plán údržby (povinné)")
    SettingsControls.maintenanceReminderDays := SettingsGui.AddEdit("x405 y147 w120 Limit3 Number", GetMaintenanceReminderDays())
    SettingsGui.AddText("x35 y185 w350", "Počet km pro upozornění na plán údržby (povinné)")
    SettingsControls.maintenanceReminderKm := SettingsGui.AddEdit("x405 y182 w120 Limit6 Number", GetMaintenanceReminderKm())
    SettingsGui.AddText("x35 y220 w520", "Zadejte celé číslo. Například 31 znamená upozornění přibližně měsíc před termínem, 1000 znamená upozornění 1000 km před servisním limitem.")

    SettingsGui.AddGroupBox("x20 y275 w570 h145", "Aplikace")
    SettingsControls.runAtStartup := SettingsGui.AddCheckBox("x35 y305 w300", "Spustit po startu počítače")
    SettingsControls.runAtStartup.Value := GetRunAtStartupEnabled()
    SettingsControls.hideOnLaunch := SettingsGui.AddCheckBox("x35 y335 w300", "Automaticky skrýt na lištu")
    SettingsControls.hideOnLaunch.Value := GetHideOnLaunchEnabled()
    SettingsControls.showDashboardOnLaunch := SettingsGui.AddCheckBox("x35 y365 w300", "Zobrazovat dashboard při startu")
    SettingsControls.showDashboardOnLaunch.Value := GetShowDashboardOnLaunchEnabled()
    SettingsGui.AddText("x55 y393 w500", "Pokud je zapnuté automatické skrytí do lišty, dashboard se při startu neotevře.")

    SettingsGui.AddGroupBox("x20 y430 w570 h155", "Zálohy")
    SettingsControls.automaticBackupsEnabled := SettingsGui.AddCheckBox("x35 y460 w350", "Pravidelně vytvářet automatické zálohy")
    SettingsControls.automaticBackupsEnabled.Value := GetAutomaticBackupsEnabled()
    SettingsGui.AddText("x35 y492 w350", "Interval automatické zálohy ve dnech (povinné)")
    SettingsControls.automaticBackupIntervalDays := SettingsGui.AddEdit("x405 y489 w120 Limit3 Number", GetAutomaticBackupIntervalDays())
    SettingsGui.AddText("x35 y524 w350", "Ponechat posledních automatických záloh (povinné)")
    SettingsControls.automaticBackupKeepCount := SettingsGui.AddEdit("x405 y521 w120 Limit3 Number", GetAutomaticBackupKeepCount())
    SettingsControls.backupStatusLabel := SettingsGui.AddText("x35 y553 w520 h24", BuildAutomaticBackupStatusText())

    SettingsGui.AddGroupBox("x20 y595 w570 h80", "Akce")
    backupNowButton := SettingsGui.AddButton("x170 y623 w140 h30", "Zálohovat ihned")
    backupNowButton.OnEvent("Click", CreateImmediateBackupFromSettings)

    saveButton := SettingsGui.AddButton("x320 y623 w120 h30 Default", "Uložit")
    saveButton.OnEvent("Click", SaveSettingsFromDialog)

    cancelButton := SettingsGui.AddButton("x450 y623 w100 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseSettingsDialog)

    SettingsGui.Show("w610 h695")
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

    maintenanceReminderDays := ValidateReminderDaysSetting(SettingsControls.maintenanceReminderDays, "Počet dní pro upozornění na plán údržby")
    if (maintenanceReminderDays = "") {
        return
    }

    maintenanceReminderKm := ValidatePositiveIntegerSetting(SettingsControls.maintenanceReminderKm, "Počet km pro upozornění na plán údržby", 1, 999999)
    if (maintenanceReminderKm = "") {
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
    IniWrite(maintenanceReminderDays, SettingsFile, "notifications", "maintenance_reminder_days")
    IniWrite(maintenanceReminderKm, SettingsFile, "notifications", "maintenance_reminder_km")
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
