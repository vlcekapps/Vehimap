# Windows Appium Recovery: 2026-09-09

## Scope And Environment

This is live automated UIA evidence, not a manual NVDA/Narrator run or an ACR
conformance statement. The September 6 driver comparison remains historical.

- Windows 11; .NET SDK 10.0.303 / runtime 10.0.11; desktop Avalonia 12.1.2.
- User-started Appium 3.7.0 at `http://127.0.0.1:4725/`, Windows driver 6.1.1,
  Microsoft WinAppDriver file version 1.2.2009.02003.
- Appium home: `%USERPROFILE%/.appium-novawindows-preview`. NovaWindows is installed
  there but is **not selected** in this run. No driver/library upgrade or patch.
- No changes to UAC, antivirus, execution policy or Appium security were made by
  this implementation. Disabling security is not a prerequisite in the documented
  setup. `appium.cmd` avoids the npm PowerShell launcher policy problem.
- Each session uses a new `%TEMP%/vehimap-appium/` published-app copy with synthetic
  Czech data. Installed-channel data is not used. Preflight refuses existing
  Vehimap processes and disables title-based attachment to unrelated windows.
- The final application under test is the local nightly
  `2.0.0-nightly.local.20260909163707`, at
  `dotnet/artifacts/nightly/win-x64/app/Vehimap.exe`.

## Findings And Fixes

1. Creating a session and reading controls was insufficient evidence of keyboard
   input. WinAppDriver rejected the fallback W3C keyboard Actions source. The
   Windows test path now uses session `POST /session/{sessionId}/keys` and releases
   modifiers with NULL after each chord. Nova's separate path is unchanged.
2. Window Close minimizes Vehimap to the tray. Test teardown now dismisses owned
   dialogs and uses File -> Exit, as a user would. A postflight check rejects any
   remaining Vehimap process. Driver force-quit is limited to explicit isolated
   launches and is not treated as proof of successful cleanup.
3. Main, detail and editor windows each expose `TextEditingLiveRegion`. An unscoped
   lookup read the empty background region even when the editor region correctly
   reported its caret. `WithinWindow` scopes UIA selectors; it does not activate
   windows, synthesize focus, or weaken focus assertions.
4. Product defect: editors opened from a standalone workspace were owned by the
   main shell rather than that workspace. The shell now tracks the active
   workspace owner, uses it for editor and post-create bundle modals, and resolves
   return focus there. Hidden/disabled workspace views ignore shared focus events.
5. The menu test clicked a list item but expected return to its ListBox. It now
   starts from the asserted keyboard startup focus. The ComboBox test clicked to
   expand the list before testing ArrowDown; it now uses Tab from the name field,
   asserts Collapsed, sends ArrowDown, and asserts Expanded plus a visible option.
   These corrections test the intended keyboard workflow rather than changing the
   application to match an invalid mouse-focus assumption.
6. WinAppDriver exposes `ExpandCollapseState` in its UI tree but returned null from
   the element attribute endpoint. The ComboBox regression therefore reads the
   actual serialized UIA pattern in the scoped window, retaining both Collapsed
   and Expanded assertions rather than dropping the expansion check.
7. Selenium's `By.Name` emitted a CSS selector, unsupported by Windows sessions.
   Name lookup now uses the driver's native `name` strategy. The failure was only
   reached after both ComboBox pattern assertions had passed.

No new application key handler, native focus activation hack or Avalonia patch was
introduced. The existing temporary TextBox UIA fallback remains a documented limit.

## Reproduction

With the loopback Appium Windows server running, close all Vehimap instances using
their Exit command, then run from the repository root:

```powershell
pwsh ./dotnet/build/Test-DotnetWindowsUi.ps1 -Profile Core
```

The wrapper requires six executed and passing scenarios, a ready server,
`VEHIMAP_UI_REQUIRE_APPIUM=1`, isolated launches and no leftover process. It restores
all environment variables on exit. `All` is an explicit broader-suite option,
not a declaration that every historical UI test has been validated.

## Diagnostic History

Local raw evidence is ignored by Git under `dotnet/artifacts/windows-ui/`:

| Run directory | Result and interpretation |
| --- | --- |
| `dc4c88cca43b44609220caa17f4ea82b` | Startup passed; normal Exit cleanup confirmed. |
| `285d6b6c6047448b980c350b346f3cfc` | Six scenarios executed, two passed; input worked but focus/selector checks failed. |
| `a59a153ba5b24225b5abe9c4e6c38a67` | Three passed after modal selector scoping, including caret editing. |
| `4f9c50e07cb841c28a7df21055ca10e0` | Four passed after product ownership fix, including save/cancel and Tab return. Menu and ComboBox tests exposed the invalid mouse-focus assumptions above. |
| `8a9c001f28ac489299c3e349e5c43ca7` | Five passed; ComboBox failed on the driver's null pattern-attribute response, before the ArrowDown assertion. All processes exited. |
| `0991aafbf6434fafa7a1cb4a5a264be8` | Five passed; ComboBox Collapsed/ArrowDown/Expanded checks passed, but option lookup failed on the unsupported CSS name selector. All processes exited. |

Failure snapshots contain only the synthetic test application's UI tree. Failed
runs are retained as failures; they are not relabeled as passed after a retry.

## Final Live Result

`e7d39a2addbd4fb8badf0ae0adb72b25/windows-ui.trx`: **6 executed, 6 passed, 0
failed, 0 skipped**, test duration 2 minutes 54 seconds. Postflight confirmed no
Vehimap process remained. No manual activation or closing was needed in this run.

| Scenario | Result |
| --- | --- |
| Startup controls and focus on VehicleListBox | Passed |
| F10 open/close, return focus and submenu | Passed |
| Save vehicle, return to Edit, Tab and Shift+Tab | Passed |
| Cancel vehicle, return to Edit and continue with Tab | Passed |
| TextBox caret movement, live-region position and edited value | Passed |
| Tab to category, ArrowDown expands it and exposes an option | Passed |

The Windows backend is usable again for this core workflow. The broader `All`
profile and NovaWindows were not revalidated in this checkpoint. Manual NVDA and
Narrator validation, including the temporary TextBox fallback, remains separate.

## Repository And Package Checks

The non-interactive suite is run separately with both live UI classes excluded,
so absent Appium cannot inflate its passed count. It passed 684 unit tests, 30
compatibility tests and 24 driver-configuration tests. The modal-owner regression
has a source guard in addition to the actual save/cancel UI tests.

Local nightly readiness used `-SkipSolutionBuild -SkipTests` after the independent
solution verification: self-contained win-x64 publish, Inno compilation, package
metadata, size/hash and manifest checks passed. Installer execution was not
requested; the user's installed Vehimap was not replaced. License compliance and
PowerShell syntax checks also passed. No public nightly was dispatched manually.
The SQLite storage nightly gate also passed (18 compatibility and 3 runtime-write
guard cases); no storage implementation or data format changed.

## Sources

- [Windows driver 6.1.1 documentation](https://github.com/appium/appium-windows-driver/blob/v6.1.1/README.md)
- [WinAppDriver supported APIs](https://github.com/microsoft/WinAppDriver/blob/master/Docs/SupportedAPIs.md)
- [WinAppDriver window-context FAQ](https://github.com/microsoft/WinAppDriver/blob/master/Docs/FAQ.md)

The installed Windows driver method map also confirms the session `/keys` route.
An HTTP-ready response alone is not evidence that application input works.
