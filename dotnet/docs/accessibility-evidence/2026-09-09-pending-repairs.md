# Pending Repairs: Automated Dialog Evidence

## Environment

- Date: 2026-09-09.
- Platform: Windows 11 Home, build 26200.
- Application: local self-contained win-x64 nightly
  `2.0.0-nightly.local.20260909211256`, Avalonia 12.1.2.
- Source: the repair-feature changes recorded in the commit containing this
  document, based on `306299be`. The build preceded that commit.
- Driver: Appium 3.7.0, Windows driver 6.1.1 and WinAppDriver on port 4725.
- Data: isolated synthetic fixture copies, not the installed user's dataset.
- Screen reader: no manual NVDA or Narrator speech assessment in this run.

## Final Live Result

Command: `pwsh ./dotnet/build/Test-DotnetWindowsUi.ps1 -Profile PendingRepairs`.

Local evidence: `dotnet/artifacts/windows-ui/52049954d3a54732b4c47741b69f69cb/windows-ui.trx`.
Result: **2 executed, 2 passed, 0 failed, 0 skipped**, duration 4 minutes 39
seconds. Postflight confirmed no remaining Vehimap process. The existing Appium
server was left running.

Both paths (vehicle menu and vehicle detail) cover:

- Empty overview: initial focus on New fault.
- Create dialog: title field first, Shift+Tab to Cancel, Tab back to title,
  Tab to description and Shift+Tab back to title, Escape without a data write.
- Invalid February date: Ctrl+S keeps the dialog open and focuses the date.
- Valid save: the fault is selected; focus returns to New fault.
- Reschedule: initial focus on the date, old/new dates and reason persisted,
  focus returns to Reschedule.
- Completion: history choice first, actual cost stored once in a new history
  entry, recurring plans unchanged.
- Cannot repair: empty reason rejected, entered reason preserved, no second
  cost/history entry created.
- Audit: opening the unresolved fault selects the correct overview item.
- Normal modal closure, process teardown and SQLite-only runtime files.

The detail path uses Tab and Enter to reach/activate its related-action button,
including scrolling into view on the test display. It does not use a forced
focus operation to satisfy an initial-focus assertion.

## Earlier Failed Run

`078923d25acd4cd2b1719b771946fbc0/windows-ui.trx` remains a failed record:
the menu path passed, while a direct coordinate click on the detail button did
not open the overview. Its UIA bounds were below the visible window. The revised
test waits for startup focus and reaches the action by normal Tab navigation.
The production detail button also now uses a shorter EN/CS visible label so it
fits the existing fixed-width layout; its full accessible name is retained.

## Other Checks

- Full solution: 822 unit, 79 compatibility and 33 UI-configuration tests passed;
  71 opt-in live UI test methods were skipped in that separate default run.
- Storage gate: 67 compatibility and 3 SQLite-only-write cases passed.
- Service tests cover EN/CS audit text, exact date boundaries, unit/number input,
  failed-save retry, existing-history linking and repeated-completion rejection.
- SQLite tests cover a pre-upgrade database copy, schema migration, transaction
  rollback after an injected insert failure, backup restore and package ID/link
  remapping. Existing attachment-import regressions remain green.
- Nightly readiness: publish, Inno compilation, metadata and checksum checks
  passed. Installer SHA-256:
  `53770cb7ff276da3ec1336b955d41c8646f2e1a94fddcf92b41f494dcdb89b1b`.
- Repository/publish license compliance and diff whitespace checks passed.

## Remaining Manual Validation

Verify NVDA/Narrator announcement of each dialog and validation error, reading
the selected fault and its schedule history, and focus after closing the overview
back to menu-origin work, detail and standalone audit/dashboard windows. Check
high contrast and enlarged/scaled layouts. Existing-history linking is covered
by service tests, but its ComboBox selection still needs manual AT validation.

Installer execution/uninstall, Android UI parity, other desktop platforms and a
formal ACR determination were not part of this feature check. The existing
temporary TextBox UIA fallback remains documented; no new UIA patch or raw
keyboard handler was introduced for repairs.
