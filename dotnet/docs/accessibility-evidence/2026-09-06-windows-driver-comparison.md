# Windows UI Driver Comparison: 2026-09-06

## Decision

Keep the existing CI driver unchanged. NovaWindows 2 is a promising opt-in backend,
not yet a validated replacement for Vehimap. Successful installation and a passed
startup test are insufficient: editor and keyboard acceptance must also pass.
These are automated UIA observations, not NVDA/Narrator or ACR conformance evidence.

## Environment And Isolation

- Windows 11, .NET SDK 10.0.303 / runtime 10.0.11, Appium 3.7.0.
- Unchanged desktop Avalonia 12.1.2 publish:
  `dotnet/artifacts/nightly/win-x64/app/Vehimap.exe`,
  `2.0.0-nightly.local.20260906070818`.
- User-installed Microsoft WinAppDriver file version `1.2.2009.02003`, used through
  the existing Appium Windows wrapper `5.1.9`.
- NovaWindows `2.0.0-preview.2` installed in the separate Appium home
  `%USERPROFILE%/.appium-novawindows-preview`. Its bundled C# backend started
  without modifying any third-party code or PowerShell encoding settings.
- Appium Windows wrapper `6.1.1` was also installed successfully in that isolated
  home. The execution tool rejected its subsequent server/test launch; **6.1.1 has
  no live result here**. The blocked command was not retried through another route.
- The default Appium home retains Windows `5.1.9` and NovaWindows `1.4.5`.
- Servers were sequential, bound only to `127.0.0.1:4725`, without relaxed security.
- Each test used its own synthetic portable copy under `%TEMP%/vehimap-appium/`,
  with explicit Czech UI preferences. Real installed-channel data was not edited.
- `VEHIMAP_UI_REQUIRE_APPIUM=1` made unavailable sessions fail rather than return
  early. `VEHIMAP_UI_ISOLATED_LAUNCH_ONLY=1` disabled the Windows title-based Root
  fallback, preventing an accidental attach to an unrelated running Vehimap.

## Live Results

All scenarios below are existing `DesktopContinuousIntegrationSmokeTests`, without
weakened assertions, product changes, or special UI focus workarounds.

| Scenario | WinAppDriver / Windows 5.1.9 | NovaWindows 2.0.0-preview.2 |
| --- | --- | --- |
| Startup controls and focus on `VehicleListBox` | Failed during session creation after about 57 seconds | Passed in about 11 seconds |
| F10 menu / return focus / submenu | Not reached | Failed waiting for `FileMenuRoot` focus after first F10 |
| Vehicle edit, save, return focus | Not reached | Failed waiting for `EditVehicleButton` focus after save |
| Vehicle edit, cancel, return focus | Not reached | Failed waiting for `EditVehicleButton` focus after cancel |
| Category ComboBox using ArrowDown | Not reached | Failed finding `VehicleEditorCategoryBox`; ArrowDown assertion was not reached |
| TextBox cursor navigation | Not reached | Failed waiting for expected cursor-position live-region text |

WinAppDriver did launch the isolated Vehimap process, but its create-session response
was HTTP 500 with an object-reference error. This does not establish a product crash,
and the five dependent workflow scenarios were not run against an unusable session.

Nova's ready-server comparison is **one passing and five failing live tests**.
A separate first attempt (`nova-startup.trx`) ran before Appium finished loading the
driver and failed availability preflight. After `/status` reported ready, the actual
startup retry (`nova-startup-ready.trx`) passed. Do not count the preflight attempt as
a failure inside Vehimap or as a passed UI interaction.

## Limits And Follow-up

- The failing focus checks do not yet isolate driver behavior, test-harness
  assumptions, window activation, and application behavior. They remain blockers
  to adoption, not proof that all five are Nova bugs or product regressions.
- Nova logged unsupported `Focused`/`focused` property aliases during fallback
  focus polling; `AutomationId` and `HasKeyboardFocus` were queried successfully.
  Removing log noise alone would not demonstrate a focus fix.
- One synthetic app process remained after driver teardown. It was stopped only
  after checking its exact temporary executable path; all pilot Appium/backend/app
  processes were absent at final cleanup. Teardown isolation needs its own follow-up
  before a broader unattended Nova suite. Temporary failed-session copies may remain.
- Next investigate one menu and one editor case independently, with explicit focus
  diagnostics and the supported WebDriver input APIs. Verify behavior manually with
  NVDA as well. Do not patch Avalonia or relax expected focus just to pass a driver.
- Retry Windows 6.1.1 only when the launch restriction is resolved. It still proxies
  the Microsoft server; its version is not a new WinAppDriver server release.
- Promote a backend only after repeated startup/menu/editor/save/cancel/keyboard
  runs, reliable cleanup, and the required CI smoke profile pass.

No application release, public nightly dispatch, dependency upgrade in Vehimap, or
Windows security change was made. This checkpoint changes only test configuration
and developer/evidence documentation; the existing local nightly remains the target.

Raw evidence is local and ignored by Git under
`dotnet/artifacts/driver-comparison-20260906/`:

- `windows-startup.trx`, `winappdriver.log`, `windows.stdout.log`, `windows.stderr.log`.
- `nova-startup.trx`, `nova-startup-ready.trx`, `nova-workflows.trx`.
- `nova-preview.log`, `nova.stdout.log`, `nova.stderr.log`.

## Repository Verification

The non-interactive suite was explicitly separated from the live comparison:

```powershell
dotnet test dotnet/Vehimap.sln --configuration Release --filter 'FullyQualifiedName!~DesktopContinuousIntegrationSmokeTests&FullyQualifiedName!~DesktopAccessibilitySmokeTests'
```

It passed 669 unit tests, 30 compatibility tests and 11 driver-configuration tests,
including the four isolation-policy combinations. Live cases were excluded from
this command rather than counted as passing after an unavailable-Appium early
return. The failed live acceptance above remains failed.

## Sources

- [Appium Windows 6.1.1: proxy and maintenance warning](https://github.com/appium/appium-windows-driver/blob/v6.1.1/README.md).
- [NovaWindows 2 preview: C# backend and capabilities](https://github.com/AutomateThePlanet/appium-novawindows-driver/blob/v2.0.0-preview.2/README.md).
- [Previous NovaWindows 1.4.5 transport pilot](2026-09-06-novawindows-pilot.md).
