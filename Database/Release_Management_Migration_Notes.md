# Release Management — Database Migrations

> **New deployments / "just give me one script":** use
> `Release_Management_Migration_Full.sql`. It is the single, canonical script
> matching the CURRENT application implementation exactly (not a superset of
> everything ever tried). Notably, it **removes** `aut.ReleaseDLL` and its
> `usp_ReleaseDLL_*` procedures (guarded: only if the table is empty) —
> those were an earlier draft of a DLL-upload/validation feature that was
> **not** built; DLLs are placed into the release folder by the existing
> controlled build/deployment process, and readiness is computed entirely by
> the application via reflection over the folder. The separate Phase 1 /
> Phase 2 files below are kept only as a historical record of how the schema
> evolved, including objects that were later removed; you do not need to run
> them if you use the Full script.

> **Note on DLL handling (supersedes the "Phase 1" description below):** the
> application no longer uploads/validates DLLs through the UI or an API — DLLs are
> placed into the release folder by the existing controlled build/deployment
> process, and readiness is computed by the app layer via reflection over the
> release folder (`IReleaseReadinessService`), not persisted in `aut.ReleaseDLL`.
> The `aut.ReleaseDLL` table and its `usp_ReleaseDLL_*` procedures still exist
> (harmless, unused) but are no longer referenced by the application. See
> **Phase 2** below for the procedures that changed as a result.

This document describes how to run `Release_Management_Migration_Phase1.sql` and
`Release_Management_Migration_Phase2_Updates.sql`, and how to interpret their
output. These scripts cover **database changes only** (schema + data back-fill +
stored procedures).

## Phase 2 — Incremental Update (`Release_Management_Migration_Phase2_Updates.sql`)

Run this on any database that already has Phase 1 applied, to pick up everything
changed since without re-running the full Phase 1 script:

- `usp_GetAllRelease` / `usp_GetReleaseById` — dropped the `ReleaseDLL`-based
  required/uploaded/validated/missing DLL-count columns (DLL presence is
  filesystem state now, computed by the app, not SQL).
- `usp_Release_SetFolderPath` (new) — stores the resolved folder path after the
  row is inserted and the physical folder created (the folder name embeds
  `ReleaseId`, so it can't be known before insert).
- `usp_DeleteRelease` (new) — compensating delete used when folder creation
  fails right after insert, and reused by the application's explicit
  Draft-only "Delete" action. Cascades to `ReleaseDLL`/`ReleaseNotification`/
  `ReleaseSignOff`; blocked by SQL if `TestCaseAssignment` rows reference the
  release (no cascade there, by design).
- `usp_ReleaseSignOff_GetByRelease` (new) — sign-off history for a release.

No table schema changes, no data changes — procedure bodies only. Idempotent;
safe to run even if Phase 1 already contains these definitions (it does, since
Phase 1 was edited in place during development — this script exists so an
**already-deployed** database that only has an earlier copy of Phase 1 can be
brought current without needing the full Phase 1 file re-run).

```powershell
sqlcmd -S "DESKTOP-BNTHM9S\SQLEXPRESS" -d MES_AUT_AI -U automation_user -P "<password>" -C `
  -i "Database\Release_Management_Migration_Phase2_Updates.sql"
```

Prerequisite: `aut.Release`, `aut.ReleaseSignOff`, etc. must already exist (i.e.
Phase 1 applied) — the script checks this and stops with a clear message
otherwise.

## Phase 1 — Initial Migration

This phase covers **database changes only** (schema + data back-fill + stored
procedures). No backend/UI code is changed as part of Phase 1 itself.

## What this migration does

Makes `aut.Release` the root business context while preserving all existing
automation functionality (discovery, assignment, execution, logs, screenshots,
results). It is **non-destructive** and **idempotent** (safe to re-run).

### Schema changes
- **`aut.Release`** — adds `Version`, `EnvironmentId` (FK → `aut.Environment`),
  `ReleaseFolderPath`, `ModifiedBy`, `ActivatedBy`, `ActivatedOn`. Widens
  `ReleaseLifecycle` to `NVARCHAR(30)`. Adds `IX_Release_EnvironmentId` and a
  filtered unique index `UX_Release_Name_Version_Env` on
  (`ReleaseName`,`Version`,`EnvironmentId`).
- **`aut.ReleaseDLL`** (new) — required DLLs defined manually per release, plus
  upload/validation tracking. Unique on (`ReleaseId`,`DLLName`).
- **`aut.ReleaseNotification`** (new) — post-activation "notify Test Manager".
- **`aut.ReleaseSignOff`** (new) — approve/reject/rework history. The `Release`
  row keeps the latest snapshot (`SignOffStatus`/`SignedOffBy`/`SignedOffOn`).
- **`aut.TestCaseAssignment`** — adds nullable `ReleaseId` (FK → `aut.Release`)
  and `EnvironmentId` (FK → `aut.Environment`). Existing text columns
  `ReleaseName` / `Environment` are **retained** for backward compatibility.

### Data back-fill (non-destructive)
- `TestCaseAssignment.EnvironmentId` is back-filled by matching the existing
  `Environment` text to `aut.Environment.EnvironmentName` (trim/case-insensitive).
  In the current DB, `DEV` → `EnvironmentId = 7`.
- **No fabrication:** existing `TestCaseAssignment.ReleaseName` values are library
  names (not real releases), so `ReleaseId` is left `NULL` and reported. The
  existing `aut.Release` row keeps `Version`/`EnvironmentId` = `NULL` (reported)
  rather than inventing values.

### Stored procedures (all `CREATE OR ALTER`)
- `usp_CreateRelease`, `usp_UpdateRelease` — new Version/EnvironmentId/FolderPath params.
- `usp_GetAllRelease`, `usp_GetReleaseById` — join Environment + computed DLL
  readiness and test summary.
- `usp_ActivateRelease` (new) — guards env active, folder present, required DLLs validated.
- `usp_ReleaseSignOff` (rewritten) — only when all assigned tests are terminal;
  writes history + updates snapshot; Approved/Rejected with comments.
- `usp_ReleaseDLL_Add/_Update/_GetByRelease/_SetUpload/_SetValidation/_Delete`.
- `usp_ReleaseNotification_Add/_GetByRelease/_MarkSent`.
- `usp_CreateOrUpdateAssignmentWithTestCases` — adds optional trailing
  `@ReleaseId`/`@EnvironmentId` params (defaulted NULL → existing callers unaffected).
- `usp_GetTestCaseAssignmentsByUser`, `usp_GetReleaseExecutionLogs` — also return
  `ReleaseId`/`EnvironmentId` (still expose text for display).

## How to run

Using `sqlcmd` (adjust server/credentials as needed):

```powershell
sqlcmd -S "DESKTOP-BNTHM9S\SQLEXPRESS" -d MES_AUT_AI -U automation_user -P "<password>" -C `
  -i "Database\Release_Management_Migration_Phase1.sql"
```

Or open the file in SSMS against `MES_AUT_AI` and Execute.

The script prints progress (`Added ...`, `Created ...`) and, near the end, a
**mapping report** consisting of three result sets.

## Mapping report — how to interpret

1. **`UnmatchedEnvironment`** — assignments whose `Environment` text had no matching
   `aut.Environment` row. Create/rename the environment, then re-run the back-fill
   `UPDATE` (or re-run the whole idempotent script).
2. **`UnlinkedAssignmentRelease`** — assignments with `ReleaseId = NULL`. These are
   the legacy library-name assignments. They will be linked to a real release once
   releases are created through the new flow (later phase); no action required now.
3. **`IncompleteRelease`** — releases with `NULL` `Version` or `EnvironmentId`.
   Complete these via the Release Management UI (later phase) or a manual `UPDATE`.

## Expected post-run state (current DB snapshot)

- Row counts unchanged: `Release = 1`, `Environment = 13`, `TestCaseAssignment = 3`.
- The 3 assignments now have `EnvironmentId = 7` (DEV) and `ReleaseId = NULL`.
- New tables exist: `ReleaseDLL`, `ReleaseNotification`, `ReleaseSignOff`.

## Rollback

The migration only **adds** columns/tables/indexes and replaces procedure bodies.
If a rollback is required, drop the new tables (`ReleaseDLL`, `ReleaseNotification`,
`ReleaseSignOff`), drop the added columns/indexes/FKs on `Release` and
`TestCaseAssignment`, and restore the previous procedure definitions from
`Release_Environment Management Scripts.sql` and
`Execution  Logs DB Script and All Procedures.sql`. No existing data is deleted by
this migration, so a rollback does not lose pre-migration data.
