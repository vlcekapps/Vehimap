# SQLite Restore Recovery

## Contract

This checkpoint addresses interrupted **backup restoration of a readable SQLite dataset**, including a restore into an initially empty root. It does not authorize automatic repair of an arbitrary corrupt database and does not claim power-loss atomicity or beta readiness. No database schema, vehicle package or `.vehimapbak` format changes are required.

1. Validate incoming attachment paths and write the incoming dataset in a separate staging root. Reload and compare every entity and setting before touching live files.
2. Take a SQLite online safety snapshot (including committed WAL pages) and copy existing attachments into `data/import-backups/<operation-id>/`. Reject linked attachment descendants. The operation ID is a generated GUID, not an archive-supplied path.
3. Publish `.restore-journal.json` before replacing live attachments or the database. The versioned envelope checksums its payload, the safety database and the complete attachment tree. Journal writes use a sibling temporary file, flush-to-disk and rename.
4. Install staged attachments and copy the staged database through SQLite's backup API. This avoids renaming a live WAL database underneath existing SQLite connections.
5. Publish the committed journal state only after both operations succeed. Remove the journal last. Keep the safety copies after success.

Cancellation before the commit marker rolls back; after the commit marker the restore is complete. An error during rollback leaves the journal and safety copies in place. Application SQLite loads/saves, backup export and vehicle package copy operations share `.storage.lock` with restore; another cooperating access fails rather than seeing mixed state. The lock file deliberately remains after release to avoid separate lock-file identities on Unix. The active data root must be writable. This is not optimistic concurrency protection against stale datasets, the later caller save after package import, or unrelated programs editing the database directly.

## Restart And Recovery

- An uncommitted journal restores the safety database and a **copy** of the safety attachments. The copies are never consumed. Interrupted incoming attachments are retained in the safety directory rather than deleted.
- Recovery can itself be interrupted and repeated. A committed journal only needs final cleanup; it must not roll back a successful restore.
- Recovery runs before migration and normal SQLite loads, including the missing-database case. A save which encounters a journal performs recovery but rejects the caller's stale dataset and requires reload.
- Missing, changed or malformed recovery evidence fails closed. Health diagnostics return a localized error while a journal is pending. The database is not silently reset.
- If the root was originally empty, uncommitted newly installed SQLite files are quarantined together in the safety area; normal startup can then initialize an empty database. Existing safety copies, active attachments and any legacy input are not automatically deleted.

If recovery cannot finish, close Vehimap and preserve the **entire data folder**, including SQLite WAL/journal files, `.restore-journal.json`, `import-backups`, attachments and any `.restore-*` staging directories. Do not delete the journal to force startup, downgrade to a binary that does not understand this journal, or mix individual database/attachment files from different copies. A confirmed recovery wizard for an already corrupt database is still a separate unresolved task.

The durable journal and flush calls improve recovery ordering, but process termination is not a physical power-cut test. Filesystem directory-entry durability, faulty disks, network filesystems and full-disk conditions need separate fault testing. Free space must accommodate the existing safety copy, incoming staging and, during rollback, an additional copy of attachments. Interrupted staging/safety material is retained rather than aggressively garbage-collected.

## Automated Evidence

`SqliteRestoreRecoveryTests` is part of `SqliteStorageCompatibilityTests`, so both the normal solution test run and the existing storage nightly gate execute it. The compatibility test assembly has a small test-only executable entry point; it is **not** shipped in desktop/mobile artifacts and has no production environment-variable kill switch.

The parent test starts the child on marked synthetic roots under the system temporary directory, waits for an explicit checkpoint, kills that exact child process, then restarts storage and compares the database and attachment bytes. A child that exits early, never reaches its checkpoint or fails to terminate makes the test fail. Normal xUnit test execution is unchanged.

Covered boundaries:

- Journal prepared; old attachments removed; new attachments installed; database installed; commit marker written, for every combination of an existing/missing database and existing/missing attachments.
- A second termination during rollback after database restoration, after removing incoming attachments and after restoring original attachments.
- Changed database/attachment copies, malformed journal, altered commit flag/checksum and an attempted journal path escape.
- Stale save rejection after recovery, access exclusion during a lease, linked attachment rejection and full staged row verification.
- Existing WAL snapshot, ordinary cancellation rollback, SQLite schema guard, old backup import and vehicle package regression tests remain active.

```powershell
dotnet test dotnet/tests/Vehimap.Tests.LegacyCompatibility -c Release
pwsh -NoProfile -File dotnet/build/Test-DotnetStorageNightlyGate.ps1 -RuntimeIdentifier win-x64
```

Implementation references: [Microsoft.Data.Sqlite online backup](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/backup), [SQLite guidance on backup, WAL and unsafe file replacement](https://sqlite.org/howtocorrupt.html).
