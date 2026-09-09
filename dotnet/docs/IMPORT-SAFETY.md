# Backup And Vehicle Package Import Safety

This is a Windows nightly hardening checkpoint, not a claim that every malformed
dataset or hardware failure is covered. Storage stays SQLite; valid vehicle data,
user text, units and archive format versions do not change.

## Resource Limits

`SafeDataArchive` applies the same default policy to SQLite `.vehimapbak` and
`.vehimapvehicle` imports. Export validates the completed temporary archive before
replacing an existing destination, so it cannot publish a file rejected by this
size/path policy. It does not automatically split large backups.

| Limit | Maximum |
| --- | --- |
| Compressed archive | 512 MiB |
| Total expanded content | 512 MiB |
| One expanded file | 256 MiB |
| All attachment content | 256 MiB |
| One JSON file | 16 MiB |
| Root manifest (JSON/INI) | 64 KiB |
| ZIP entries, including directories | 10,000 |
| Central directory | 8 MiB |
| Entry name / path depth | 1,024 characters / 32 segments |
| Legacy v1-v6 text backup, including Base64 | 32 MiB |
| Legacy attachment entries | 10,000 |

MiB means 1,048,576 bytes; limits are inclusive. Classic single-volume ZIPs are
supported. Split archives and ZIP64 are rejected, including small third-party
archives explicitly forced into ZIP64. Vehimap exports under these limits do not
need ZIP64. Highly compressible but otherwise valid content is allowed; safety
depends on absolute sizes and counts, not a guessed compression ratio.

The central directory is bounded and counted before `ZipArchive` materializes
entries. All paths, metadata sizes and conflicts are checked before extraction.
Extraction streams content into a fresh application-owned staging folder, checks
cancellation and actual bytes copied, and never writes archive entries directly
to live data. Nested archives are ordinary attachments, not recursively expanded.
Case-only duplicate paths, file/directory conflicts, traversal, alias paths and
symbolic links/reparse entries are refused consistently across platforms.

Only a positively identified legacy v1-v6 header enters the bounded text importer.
A damaged ZIP or a limit rejection never falls back to unbounded text parsing.
User-facing size/count errors have English and Czech resources. If a valid backup
exceeds a limit, keep the original file and request support for a larger/streamed
import; do not remove attachments from the live data folder to work around it.
Smaller per-vehicle packages may be appropriate for sharing individual vehicles.

## Package Commit Boundary

`IVehiclePackageService.ImportVehicleAsync` now commits the merged dataset before
returning. It holds the cooperating storage lease while validating, copying new
attachments and committing SQLite. The caller must not save the returned dataset
a second time. Source/current dataset objects are not edited in place.

On a normal exception or cancellation before commit, SQLite rolls back and the
importer attempts to remove every file it created, including partial copies. It
never deletes a pre-existing destination file. Cleanup failures are surfaced, not
silently treated as success. Empty directories may remain. After success, shell
state/selection are published on the UI context; while importing, editing, a second
import, settings writes and normal shutdown are blocked with a localized status.
The expensive import runs away from the UI thread.

This is **not** a durable package-import journal: forcibly killing the process
between copy and commit may leave unreferenced attachments. Existing restore
journaling applies to full backup restore, not this package operation. Physical
power-loss durability, explicit recovery of an already corrupt live database and
optimistic concurrency with unrelated external writers remain separate work.
Archive limits bound expansion, but do not establish a total memory limit for
loading a SQLite dataset or replace arithmetic/row-count fuzzing.

## Regression Evidence

- `DataArchiveSafetyTests`: all budgets, highly compressed valid data, strict
  paths, duplicates, file/directory conflicts, symlinks, lying directory counts,
  actual stream-byte limits, cancellation, prior-export preservation and EN/CS errors.
- `SqliteImportSafetyTests` (part of `SqliteStorageCompatibilityTests`): a real
  SQLite trigger fails after attachment copy; cancellation at the same boundary;
  lease exclusion; old database/files preserved; retry commits once; oversized ZIP
  and legacy imports fail before touching active data.
- `VehiclePackageImportLifecycleTests`: delayed import blocks competing commands
  and shutdown, failure/cancellation never publish incoming data, success requires
  no second caller save, and all outcomes release the UI lock.
- These tests are included in `Test-DotnetStorageNightlyGate.ps1` and the solution
  test run. Live Core remains a separate keyboard/startup/shutdown regression and
  is not a live file-picker/package-import test.

## Verified Windows Checkpoint (9-10 September 2026)

Published local app: `2.0.0-nightly.local.20260909215058`,
`dotnet/artifacts/nightly/win-x64/app/Vehimap.exe`. The readiness build completed
without warnings/errors; 852 unit, 83 compatibility and 33 UI configuration cases
passed at packaging time (71 opt-in live cases explicitly skipped in that run).
The expanded storage gate passed 71 compatibility and 33 focused unit cases.
License compliance passed. Inno metadata/checksum validation passed; no installer
was executed and no manual public release/tag was created. Installer SHA-256:
`52f00873c4a7857cba17303604a1a739901d70165d52b42bdaf2a2b823def906`.

First live Core run: 4/6 passed, then WinAppDriver's backend connection reset
during the ComboBox arrow-key command. The next test correctly refused the
remaining process. Evidence: `dotnet/artifacts/windows-ui/51b0c2f54be04eb681cfda085f0099ba/windows-ui.trx`.
The app remained responsive through a new attached session and was closed using
Cancel, Close detail and File -> Exit, without Task Manager or forced termination.

The repeated live Core run passed 6/6 in 4 min 26 s with no remaining Vehimap
process: `dotnet/artifacts/windows-ui/9f1bd2dfcf954ea3b74c272c8eabbdae/windows-ui.trx`.
The transport failure is retained as evidence, not relabeled as a passed test or
claimed as a fixed WinAppDriver defect. The separate teardown correction preserves
test copies until process exit is confirmed and adds four pure configuration cases.

Final solution rerun with that test-helper correction: 852 unit, 83 compatibility
and 37 UI configuration cases passed, with 71 opt-in live cases explicitly skipped.
TRX files are under `dotnet/artifacts/beta-audit/import-safety/final/`. No production
code changed after the local app above was published.
The corrected teardown then passed a separate live Startup run (1/1, 25 seconds,
no remaining process): `dotnet/artifacts/windows-ui/3fdb98d145f94580b859214617036be6/windows-ui.trx`.

## References

The bounded staging/extraction policy follows Microsoft's
[ZIP and TAR security guidance](https://learn.microsoft.com/en-us/dotnet/standard/io/zip-tar-best-practices).
Central-directory preflight uses the classic ZIP structure described by
[PKWARE APPNOTE](https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT);
decompression remains the .NET library's responsibility.
