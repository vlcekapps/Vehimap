#Requires AutoHotkey v2.0
#SingleInstance Force
Persistent
#Include GeneratedBuildInfo.ahk
#Include lib\AppRuntime.ahk
#Include lib\CoreHelpers.ahk
#Include lib\Dashboard.ahk
#Include lib\HelpAndUpdates.ahk
#Include lib\GlobalSearch.ahk
#Include lib\HistoryDialog.ahk
#Include lib\ImportExport.ahk
#Include lib\MainWindow.ahk
#Include lib\Overviews.ahk
#Include lib\SettingsDialog.ahk
#Include lib\VehicleDialogs.ahk
#Include lib\MaintenancePlans.ahk
#Include lib\FuelDialog.ahk
#Include lib\RecordsDialog.ahk
#Include lib\ReminderDialog.ahk
#Include lib\Costs.ahk
#Include lib\DataStore.ahk
#Include lib\BackupsAndAlerts.ahk

global AppTitle := "Vehimap"
global DataDir := A_ScriptDir "\data"
global VehiclesFile := DataDir "\vehicles.tsv"
global HistoryFile := DataDir "\history.tsv"
global FuelLogFile := DataDir "\fuel.tsv"
global RecordsFile := DataDir "\records.tsv"
global VehicleMetaFile := DataDir "\vehicle_meta.tsv"
global RemindersFile := DataDir "\reminders.tsv"
global MaintenancePlansFile := DataDir "\maintenance.tsv"
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
global VehicleMaintenancePlans := []
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
global DetailMaintenanceSummaryLabel := 0
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
global MaintenanceGui := 0
global MaintenanceVehicleId := ""
global MaintenanceList := 0
global MaintenanceSummaryLabel := 0
global MaintenanceAllPlans := []
global MaintenanceSearchCtrl := 0
global VisibleMaintenancePlanIds := []
global MaintenanceSortColumn := 5
global MaintenanceSortDescending := false
global MaintenanceCompleteButton := 0
global MaintenanceFormGui := 0
global MaintenanceFormControls := {}
global MaintenanceFormMode := ""
global MaintenanceFormPlanId := ""
global MaintenanceFormVehicleId := ""
global MaintenanceCompleteGui := 0
global MaintenanceCompleteControls := {}
global MaintenanceCompletePlanId := ""
global MaintenanceCompleteVehicleId := ""
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
global FleetCostGui := 0
global FleetCostRows := []
global FleetCostSummaryLabel := 0
global FleetCostList := 0
global FleetCostPeriodYearCtrl := 0
global FleetCostPresetCtrl := 0
global FleetCostFromMonthCtrl := 0
global FleetCostToMonthCtrl := 0
global FleetCostPeriodSummaryLabel := 0
global FleetCostPeriodList := 0
global FleetCostVehicleCostsButton := 0
global FleetCostOpenButton := 0
global FleetCostEditButton := 0
global DashboardGui := 0
global DashboardSummaryVehiclesLabel := 0
global DashboardSummaryTermsLabel := 0
global DashboardSummaryCostsLabel := 0
global DashboardSummaryDataLabel := 0
global DashboardList := 0
global DashboardEntries := []
global DashboardOpenButton := 0
global DashboardItemButton := 0
global DashboardHistoryButton := 0
global DashboardCompleteButton := 0
global DashboardVehicleCostsButton := 0
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
^m::OpenSelectedVehicleMaintenancePlans()
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
^h::OpenSelectedDashboardVehicleHistory()
^l::CompleteSelectedDashboardMaintenance()
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
^m::OpenMaintenanceFromDetail()
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

#HotIf IsGuiWindowActive(MaintenanceGui)
^f::FocusMaintenanceSearchShortcut()
^n::AddVehicleMaintenancePlan()
^+n::AddRecommendedVehicleMaintenancePlans()
^u::EditSelectedVehicleMaintenancePlan()
F2::EditSelectedVehicleMaintenancePlan()
^l::CompleteSelectedVehicleMaintenancePlan()
^d::OpenVehicleDetailFromMaintenance()
#HotIf

#HotIf IsListViewFocusedInGui(MaintenanceGui)
Enter::EditSelectedVehicleMaintenancePlan()
Delete::DeleteSelectedVehicleMaintenancePlan()
#HotIf

#HotIf IsGuiWindowActive(CostSummaryGui)
^r::RefreshVehicleCostPeriodSummary()
^d::OpenVehicleDetailFromCostSummary()
#HotIf

#HotIf IsGuiWindowActive(FleetCostGui)
^r::RefreshFleetCostOverview()
^p::OpenSelectedFleetVehicleCostSummary()
^u::EditSelectedFleetCostVehicle()
F2::EditSelectedFleetCostVehicle()
^o::OpenSelectedFleetCostVehicleDetail()
#HotIf

#HotIf IsListViewFocusedInGui(FleetCostGui)
Enter::OpenSelectedFleetVehicleCostSummary()
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

#HotIf IsGuiWindowActive(MaintenanceFormGui)
^s::SaveVehicleMaintenancePlanFromForm()
#HotIf

#HotIf IsGuiWindowActive(MaintenanceCompleteGui)
^s::SaveVehicleMaintenanceCompletionFromForm()
#HotIf

if !IsVehimapTestMode() {
    InitApp()
}
