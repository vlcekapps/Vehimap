# Desktop Avalonia Upgrade Review

Date: 2026-09-06. Scope: Windows desktop only, Avalonia 12.0.4 -> 12.1.2.
This is an engineering validation record, not an ACR `Supports` claim.

## Environment

- Windows 11 x64, build 26200; .NET SDK 10.0.303, runtime 10.0.11.
- Source baseline: `d2d6a0b1`, plus the desktop framework upgrade in this checkpoint.
- Desktop output: `dotnet/artifacts/nightly/win-x64/app/Vehimap.exe`, version
  `2.0.0-nightly.local.20260906070818`.
- Android and `Vehimap.Mobile` retain Avalonia 12.0.4 and their existing TalkBack evidence.

## Upstream Review

- [12.1.2 release](https://github.com/AvaloniaUI/Avalonia/releases/tag/12.1.2):
  detached-control focus restore (#22113), access-key handling without descendant
  focus (#21920), and hidden tray icon update behavior (#22125).
- [12.1.1 release](https://github.com/AvaloniaUI/Avalonia/releases/tag/12.1.1):
  ComboBox scrolling to the selected popup item (#21764).
- [12.1.0 release](https://github.com/AvaloniaUI/Avalonia/releases/tag/12.1.0):
  Windows UIA ExpandCollapseState ordering (#21693) and decimal COM values (#21634).
- [TextBox issue #9770](https://github.com/AvaloniaUI/Avalonia/issues/9770) remains
  open. The [released TextBox peer](https://github.com/AvaloniaUI/Avalonia/blob/12.1.2/src/Avalonia.Controls/Automation/Peers/TextBoxAutomationPeer.cs)
  implements `IValueProvider`, not a native text provider. Do not retire the temporary
  text-navigation live-region fallback or the associated regression tests yet.

## Automated Checks

- Desktop restore and Release build: passed, zero build warnings/errors.
- Full solution test command: passed (669 unit, 30 legacy compatibility, 74 UI test
  methods). The UI fixture returns early without an Appium server, so those 74 methods
  are **not evidence of executed desktop UI scenarios** on this machine.
- Developer environment check with release tools: passed after Inno Setup installation.
- SQLite storage nightly gate: passed (18 compatibility and 3 runtime-write tests).
- Windows nightly readiness with `-SkipTests` after the full test run: passed,
  including Release build, self-contained publish, Inno compile, installer metadata,
  size/SHA-256 and update manifest checks. Actual installer installation/uninstallation
  smoke was not requested or run; no existing installed Vehimap was replaced.
- License compliance with `-PublishDirectory dotnet/artifacts/nightly/win-x64/app`:
  passed. The published `Vehimap.deps.json` confirms desktop Avalonia 12.1.2,
  MicroCom.Runtime 0.11.6 and Tmds.DBus.Protocol 0.94.1; notices and dependency
  metadata were updated alongside the NuGet snapshot.

## Interactive Validation And Limits

- Appium was installed, but the tool rejected automatic WinAppDriver MSI installation
  with `blocked by policy`, without further detail. No alternate installation path was
  used to bypass that rejection. Live Appium regressions still require WinAppDriver.
- Computer Use smoke used a separate publish copy at
  `dotnet/artifacts/desktop-avalonia-smoke-20260906` with the synthetic
  `legacy-1.0.2-fleet` fixture. No user AppData or normal nightly data was used.
- Observed initial focus on `VehicleListBox`; Down selected the next vehicle and
  updated the detail. Tab moved to `DetailTabButton`, Shift+Tab returned to the list.
  First F10 focused `FileMenuRoot`; second F10 returned to `VehicleListBox`.
- Ctrl+O opened the standalone vehicle detail with focus on `EditVehicleButton`.
  Enter opened `VehicleEditorWindow` with focus on `VehicleEditorNameBox`.
  Shift+Tab reached `CancelVehicleButton`, Tab returned to the name, and the next
  Tab reached `VehicleEditorCategoryBox`. Down exposed the category popup items.
- Popup focus and cancel return were inconclusive: the automation tree reported
  the editor window while the popup was open, and after Escape the editor closed
  but immediate detail focus was not reported. A subsequent observation reported
  main-window filter focus. Later the detail edit button had focus, with concurrent
  user input detected. Do not count this as a passing cancel/focus regression.
- The user stopped Computer Use with physical Escape before the independent
  save/cancel and TextBox cursor tests could finish. No further UI actions were made.
- NVDA/Narrator spoken output was not independently verified by the agent.
  Retest startup announcement, Alt/F10, list navigation, TextBox character/selection
  feedback, ComboBox opening, save/cancel focus return and tray actions before claiming
  assistive-technology conformance. UI Automation tree inspection alone is not enough.

## User Follow-up

After the automated interaction was stopped, the user reported on 2026-09-06 that
the remaining checks were completed manually and passed. This resolves the outstanding
practical smoke check for this desktop upgrade based on the user's report, rather
than an executed Appium run. The exact screen reader/version and individual scenario
results were not supplied, so this is not a complete formal ACR test record.
