# Pre-Beta Code And Test Audit

Date: 2026-09-09. Starting revision: `b2bda32f`.

## Scope And Decision

This is a risk-focused, repository-wide audit checkpoint, not a certification or a claim that every execution path has been proved correct. Inspection started with application code, persistence and update boundaries, then moved to tests and release scripts. The solution includes shared application/domain code, SQLite and legacy import, platform adapters, the desktop and mobile view models, updater, and three test projects. Android device execution and macOS/Linux UI execution are outside this Windows run.

Do not use this document alone to approve beta. The fixes below address reproducible defects. Manual assistive-technology acceptance and the remaining stress/recovery work at the end are still required. No real user data was modified by the audit tests.

## Fixed Findings

| Priority | Finding and correction | Regression evidence |
| --- | --- | --- |
| P1 | An interrupted legacy migration could leave a live, empty SQLite database; restart would then skip conversion and archive the legacy input. Convert in a staging root, compare every entity/settings value after reload, then publish a SQLite snapshot before archiving legacy files. | `SqliteStorageFailureTests`: interrupted/incomplete conversion, retry and original-file preservation. |
| P1 | Loading an existing database silently recreated missing schema objects. Reads now use read-only connections and one consistent read transaction; both load and save reject missing/unsupported schema markers instead of repairing them silently. | Missing-table, missing-marker and future-marker tests assert that database bytes remain unchanged. |
| P1 | Backup restore could modify the live database/delete existing attachments before all incoming paths and rows were validated. Stage and validate first, keep the pre-restore copy, then swap attachments with rollback on save failure/cancellation. | Malformed/duplicate paths, invalid rows and injected live-save interruption retain original data and attachments. |
| P1 | Pre-restore `File.Copy(db)` omitted committed WAL pages. Use SQLite's backup API for the safety snapshot. | A live WAL connection commits a changed vehicle; the pre-restore snapshot contains that change. |
| P1 | A missing attachment in an imported vehicle could resolve to an unrelated existing destination file. Reserve collision-free paths even for absent attachments and validate all package owners, metadata and paths before copying anything. | Missing-file collision, mismatched owner/manifest and late malicious path tests. |
| P1 | Lexically contained managed paths could still traverse a junction/symlink or a Windows trailing-dot/space alias. Reject such descendants before resolving/copying. | `ManagedAttachmentPathGuardTests`, including a temporary symbolic link. |
| P1 | Update metadata was not checked against the application's channel; preparation trusted UI eligibility, accepted unsafe asset addresses, and downloads could exceed declared size. Recheck channel/published build, require HTTPS and valid kind/hash, limit received bytes, and propagate user cancellation. Published builds no longer load local manifest overrides. | `UpdateSafetyTests`: wrong channel/metadata, local override, cancellation and oversized chunked transfer. |
| P1 | The archive updater ignored its wait timeout and could overwrite a still-running application, including itself. Abort on timeout, run a copied helper/runtime outside the installation, reject overlapping/linked paths and external entry points, and retain originals for rollback after copy failures. Preserve only the `data` directory, not `database.dll`. | `ArchiveInstallerSafetyTests` and archive install-plan tests. The primary Windows Inno installer flow is unchanged. |
| P1 | Readiness recursively deleted local output even when it contained `app/data`. Reject portable data and linked output roots before building/deleting. Nightly/beta/stable wrappers also now propagate a failed child script exit code. | Isolated PowerShell execution proves portable data survives and a child exit code of 42 reaches the caller. |
| P2 | Localized numeric input accepted malformed grouping and a legacy fallback stripped arbitrary characters. Use strict active-locale parsing in editors; retain tolerant parsing only at existing legacy/read boundaries. Guard conversion range before casting/multiplying. | `NumericInputSafetyTests`, `EditorInputSafetyTests`, existing settings and editing tests. |
| P2 | Reopening/saving miles or gallons could round-trip to a different canonical odometer/fuel volume; canonical money was shown with an incompatible decimal separator. Use one decimal for mile inputs, five for gallon inputs, and localized money on editor load. | Thousands of conversion samples and repeated editor saves preserve canonical values (including 3.12 litres and 1234.56 cost). |
| P2 | Fuel rows missing price or volume biased average price per litre; dated purchases without an odometer were omitted from consumption. Average only paired known values, suppress incomplete segment unit prices, include all dated purchases between usable full-tank endpoints, and reject ambiguous ordering. | Average-price, missing-cost, missing-odometer and ambiguous-date tests in `LegacyFuelAnalysisServiceTests`. |
| P2 | Health diagnostics could report a healthy database with an additional unsupported migration marker, even though runtime loading correctly refused it. Require the same supported schema marker set in diagnostics. | Existing invalid-schema tests now also assert a non-destructive health error. |
| P2 | Very large maintenance intervals could overflow calendar arithmetic or turn the next odometer negative. Bound month arithmetic and use wide addition for service distances; display an invalid-date status instead of crashing. | `CalendarArithmeticSafetyTests`, existing timeline/service-book/projection tests. |
| P2 | Cancel/window-close could reset editor state while an asynchronous save was in progress; a repeated main-window close could bypass an outstanding confirmation. Keep closing/cancellation blocked during these operations. Initialization also retains the UI synchronization context. | Shared lifecycle source contract plus live normal save/cancel cases; artificial slow-save UI fault injection remains to be added. |
| P2 | An idle activation pipe client could block future single-instance requests indefinitely, and different user sessions shared pipe names. Bound the message/time, restrict the pipe to the current user and scope names by user/session. | Single-instance idle-client and name-isolation tests. |
| P2 | UI tests returned successfully when Appium was absent. Use explicit skipped facts/theories without configuration and hard failures when a configured/required session cannot start. | `DesktopUiTestConfigurationTests`, default suite counters and separate live gates. |
| P2 | Several editor tests still targeted removed inline workflows, used unscoped duplicate controls, appended to preset values, or attempted to close a disabled owner below a modal. WinAppDriver also typed `15000` as `+řééé` on the Czech keyboard, which the app correctly rejected. Use real dialog entry points, normal Tab navigation, and Ctrl+V fixture input with clipboard readback; dedicated keyboard/cursor tests still send keys. | `Test-DotnetWindowsUi.ps1 -Profile Editors`; failed attempts and reruns are retained separately. The input mismatch was isolated in `dotnet/artifacts/beta-audit/input-diagnostic/input.trx`. |

## Test Evidence

An additional live P2 finding: the five non-vehicle editor footers placed Cancel before Save, but their first-field Shift+Tab handler jumped to Cancel. Forward and backward traversal therefore disagreed. Their footer order now matches Vehicle: Save, Cancel. The source guard checks this order and the live theory checks first-field -> Shift+Tab -> Cancel -> Tab -> first-field -> Tab -> second-field -> Shift+Tab -> first-field. No new focus workaround was added.

Automated commands use synthetic data and separate temporary roots. A skipped UI test is not a pass. Run build/readiness and live UI tests sequentially: a live `testhost` locks its own DLL, so rebuilding that project concurrently fails legitimately.

```powershell
dotnet test dotnet/Vehimap.sln -c Release
dotnet list dotnet/Vehimap.sln package --vulnerable --include-transitive
pwsh -NoProfile -File dotnet/build/Test-DotnetStorageNightlyGate.ps1 -RuntimeIdentifier win-x64
pwsh -NoProfile -File dotnet/build/Test-DotnetNightlyReadiness.ps1 -RuntimeIdentifier win-x64
pwsh -NoProfile -File dotnet/build/Test-DotnetWindowsUi.ps1 -Profile Core
pwsh -NoProfile -File dotnet/build/Test-DotnetWindowsUi.ps1 -Profile Editors
```

The NuGet vulnerability query returned no known vulnerable direct/transitive packages for the 11 solution projects using the configured NuGet source on this date. This is not a security guarantee and does not independently audit native libraries.

Initial successful live Core evidence: `dotnet/artifacts/windows-ui/8cf70ab860714fa0ae61d5d8a7715a83/windows-ui.trx` (6/6, no remaining Vehimap process). The final local nightly results are recorded below. Temporary artifacts are intentionally not committed; the regression tests and repeatable commands are.

Latest verified build: `2.0.0-nightly.local.20260909182554`, Windows x64.

| Check | Result |
| --- | --- |
| Solution build/nightly readiness | Passed with 0 warnings/errors, followed by the final default test run below. |
| Final `dotnet test dotnet/Vehimap.sln -c Release` | 744 unit, 44 compatibility and 28 UI configuration cases passed. The 69 opt-in live cases were explicitly skipped in this default run. TRX files: `dotnet/artifacts/beta-audit/final-verified/`. |
| SQLite storage nightly gate | Passed: 32 selected compatibility cases plus 3 SQLite-only runtime-write cases. |
| License compliance | Passed. |
| Live Editors against the published application | 12/12 passed, no skips, 8 min 13 s; no Vehimap process left after teardown. Evidence: `dotnet/artifacts/windows-ui/956b09868b124690b52da2114783dacd/windows-ui.trx`. Save cases additionally read the committed SQLite values, not just disappearance of the editor. |
| Live Core against the same published application | 6/6 passed, no skips, 3 min 24 s; no Vehimap process left after teardown. Evidence: `dotnet/artifacts/windows-ui/7b2ae7c0815d4012bf7d0c53177ab9d5/windows-ui.trx`. Covers startup, menu, vehicle save/cancel, cursor editing and ComboBox keyboard opening. |
| Inno packaging, metadata and checksum | Passed. Installation/uninstallation was not requested in this run; do not interpret the metadata smoke as a live installer smoke. |

Local application: `dotnet/artifacts/nightly/win-x64/app/Vehimap.exe`.
Installer SHA-256: `147448b7e84ab20862db7b9edf46367921d55647f383410970df3493c04a981f`.
No beta/stable tag or manual public nightly release was created.

## Remaining Validation Before Beta

- Restore has transaction rollback for ordinary exceptions/cancellation, but the database and attachment directory are not one filesystem transaction. Abrupt process/power loss between the directory swap and SQLite commit needs a recovery-journal design and process-kill tests. Preserve `pre-restore-backups`; do not advertise power-loss-atomic restore.
- Explicit recovery into an already corrupt or incompatible live database needs a separate quarantined-replacement flow. Current load/save guards correctly refuse to overwrite it; do not weaken them just to make a restore test pass. Use a new isolated data root to recover a backup until this workflow is implemented.
- Extreme/malicious imported data still needs systematic arithmetic and resource-limit fuzzing (aggregate decimal totals, enormous date ranges, ZIP expansion and attachment counts). The focused conversion/calendar fixes are not a complete fuzzing campaign.
- Full-dataset saves have transaction integrity, but optimistic concurrency across external database writers and package-copy cleanup after a later failed caller save are not yet covered end-to-end.
- The complete extended UI suite, slow-save cancellation, NVDA/Narrator, high contrast/scaling, real installer update/relaunch and macOS/Linux/Android execution remain independent acceptance work. Current TextBox/UIA and native-tray limitations remain in `ACCESSIBILITY.md`; no new ACR conformance claim is made.
- Some shell cards intentionally hide editing actions while their workspace windows expose them. The tests now use the implemented entry points; making all cards consistent is a separate UX decision, not grounds for silently changing an assertion or bringing inline editors back.

## Implementation References

- [SQLite online backup API](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/backup) is used instead of copying a potentially WAL-backed live file.
- [SQLite transactions](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/transactions) inform the consistent read snapshot and rollback boundaries.
- [Accessibility checklist](ACCESSIBILITY.md) remains the technical contract; this audit does not replace manual assistive-technology evidence.
