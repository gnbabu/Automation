# OHPNM Automation — Project Notes

## Structure
- `AutomationAPI/` — ASP.NET Core (.NET 8) Web API (controllers + repositories + SQL stored procedures).
- `ohpnm-test-portal/` — Angular frontend.
- `Database/` — SQL scripts (schema + stored procedures + migrations).
- `TestLibs/` — legacy global folder of test DLLs. No longer used by discovery/execution
  (both are now scoped per-Release, see "Test Case Assignment — Release-aware" below);
  kept only because `TestSettings:TestLibsPath` is still present, unreferenced, in config.

## Database
- Server: `DESKTOP-BNTHM9S\SQLEXPRESS`, DB: `MES_AUT_AI`, schema `aut`.
- Connection string lives in `AutomationAPI/appsettings.json` (`ConnectionStrings:DefaultConnection`).
- Data access: stored procedures only, via `SqlDataAccessHelper` (constants in `Repositories/SQL/SqlDbConstants.cs`).

### Running SQL with sqlcmd
`aut.Release` has a **filtered unique index** (`UX_Release_Name_Version_Env`), so any
INSERT/UPDATE/DELETE on it requires `QUOTED_IDENTIFIER ON`. Add `-I` when using sqlcmd:
```
sqlcmd -S "DESKTOP-BNTHM9S\SQLEXPRESS" -d MES_AUT_AI -U automation_user -P "<pw>" -C -I -Q "..."
```
The app's SqlClient sets this automatically, so the API is unaffected.

## Build / run / test the API
```powershell
dotnet build AutomationAPI/AutomationAPI.csproj
dotnet run   --project AutomationAPI/AutomationAPI.csproj --launch-profile http   # http://localhost:5116 (Swagger at /swagger)
```

## Release Management (root business context)

### DLLs are NOT uploaded through this application
DLLs are placed into the Release folder by the **existing controlled build/deployment
process** (outside this app). Release Management only detects readiness — it never stores,
uploads, or version-validates DLLs. There is no `ReleaseDLL` upload UI/API.

### Config
- `ReleaseSettings:RootPath` (appsettings.json) = the base "Environment Root" location
  (`D:\Releases`). Release folders are created at:
  `<RootPath>\<EnvironmentName>\REL-<ReleaseId>_<ReleaseName>_v<Version>`
  (no separate version subfolder — the `REL-{id}` prefix matches the identifier shown in
  the UI, e.g. `REL-6_Onboarding-Release_v2.5.0`). Folder is always ID-based even if the
  release is later renamed, to avoid stale-folder reuse and rename drift. The environment
  name/active-flag come from Environment Management, never
  hard-coded.

### Key backend pieces
- `ReleaseController` / `ReleaseRepository` — CRUD, activate, sign-off, notifications.
  Create flow: insert row (folder path NULL) → resolve folder path using the new
  `ReleaseId` → create the physical folder → `usp_Release_SetFolderPath`. If folder
  creation fails, the just-inserted row is deleted via `usp_DeleteRelease` (compensating
  transaction) so a failed create is never reported as successful.
- `ReleaseFileService` — resolves/creates the release folder only (no DLL file handling).
- `ReleaseReadinessService` (`IReleaseReadinessService`) — **read-only** check reusing the
  same reflection technique as `TestSuitesRepository`/`ReflectionTestRunner`
  (`Assembly.LoadFrom` + NUnit `TestFixtureAttribute` scan), scoped to a single release
  folder instead of the global `TestLibsPath`. `CheckReadiness()` (used by the Details page
  Refresh button and the Activate guard) does the full reflection scan; `GetDllFileCount()`
  (used for list/detail badges) is a cheap file-count-only check with no reflection.
- Notifications on activation go to active **Manager + Admin** users via `IEmailService`
  (SendGrid); failures are recorded (`aut.ReleaseNotification`) and do NOT fail activation.
- `IReleaseNotificationService` (`ReleaseNotificationService`) — extracted, shared
  Manager/Admin recipient-resolution + send/record logic, used by BOTH
  `ReleaseController.Activate` (`notificationType = "ActivatedForTesting"`) and
  `ReleaseDllsReadyNotificationWorker` (`notificationType = "DllsReadyForActivation"`),
  so this logic lives in exactly one place.
- `ReleaseDllsReadyNotificationWorker` (`Repositories/Workers/`, hosted `BackgroundService`,
  mirrors `TestQueueWorker`'s shape) — every 30s, scans Draft releases with a folder set,
  runs `IReleaseReadinessService.CheckReadiness()`, and sends **exactly one**
  `DllsReadyForActivation` notification the first time a release becomes ready
  (deduplicated by checking `GetNotificationsAsync` for an existing notification of that
  type before sending). **Never auto-activates** — activation stays a deliberate human
  action; the worker only proactively notifies so an Admin/Manager doesn't have to keep
  checking the page.

### API endpoints
- `GET  /api/Release` — all releases (test summary counts + cheap `dllFileCount`/`folderReady`)
- `GET  /api/Release/{id}`
- `POST /api/Release` — create (validates env active; creates folder as `<RootPath>\<Env>\<ReleaseId>_<Name>_v<Version>`; 409 on duplicate Name+Version+Env)
- `PUT  /api/Release/{id}` — update (never renames/moves the folder)
- `GET  /api/Release/{id}/readiness` — full reflection-based readiness check of the release folder
- `POST /api/Release/{id}/activate` — guards on readiness (usable DLL content must exist) then notifies managers
- `POST /api/Release/{id}/signoff` — body `{ signOffStatus: "Approved"|"Rejected", signOffBy, comments }` (only after all assigned tests are terminal)
- `GET  /api/Release/{id}/signoff-history`
- `GET  /api/Release/{id}/notifications`
- `DELETE /api/Release/{id}` — **permanent** delete; only allowed while `ReleaseLifecycle == Draft`
  (400 otherwise); also removes the physical release folder (best-effort) and reuses
  `usp_DeleteRelease`. Blocked with `409` if the release has associated
  TestCaseAssignment rows (no `ON DELETE CASCADE` on that FK, by design).

### Migration
- `Database/Release_Management_Migration_Phase1.sql` (idempotent, non-destructive) — see
  `Database/Release_Management_Migration_Notes.md`. Includes `usp_Release_SetFolderPath`
  and `usp_DeleteRelease` (compensating delete, used only right after a failed folder
  creation). `usp_GetAllRelease`/`usp_GetReleaseById` no longer return DLL counts (DLL
  presence is filesystem state, computed by the app layer, not persisted in SQL).

### Lifecycle
`Draft → (DLLs placed by build/deploy process) → Active (on activate) → ... tests complete ... → Completed/Rejected (on sign-off)`.
Sign-off happens AFTER testing completes, never before activation.

### Editing a Release
`PUT /api/Release/{id}` supports edits, but identity fields are locked once a release
leaves Draft (they're baked into the immutable release folder name and any recorded test
results): if the release is not `Draft`, changing `ReleaseName`/`Version`/`EnvironmentId`
returns `400`; only `Description` may still change. The folder itself is **never** renamed
on update — it stays tied to the name/version captured at creation time, even if the
release is later renamed while still in Draft.

### Deactivating / deleting a Release
Mirrors Environment Management's soft/hard delete split:
- **Deactivate/Reactivate** ("Disable"/"Enable" in the UI) — toggles `IsActive` via the
  existing `PUT` (no new endpoint), safe at any lifecycle stage since it never touches
  Name/Version/Environment. Reversible; preserves the release, folder, and history.
- **Delete** — `DELETE /api/Release/{id}`, only offered in the UI when
  `releaseLifecycle === 'Draft'` (server re-enforces this). Removes the DB row and its
  physical folder. Not available once a release has progressed past Draft (activated
  releases may have real folder/test/sign-off history that must be preserved) — deactivate
  those instead.

## Frontend (Angular)
- Build: `cd ohpnm-test-portal; npm run build` (prod/AOT). Dev: `ng serve` (uses
  `environment.development.ts` → API at `https://localhost:7147/api`, so run the API on https).
- Standalone components; barrels `@interfaces` (`src/app/core/interfaces/index.ts`) and
  `@services` (`src/app/core/services/index.ts`). HTTP via `HttpService` (JSON).
- Card UI reuses global classes from `styles.css`: `.env-card`, `.env-header`,
  `.env-theme-*`, `.status-pill`.

### Release Management UI
- Service: `ReleaseService` (no DLL upload service — none needed).
- Pages under `src/app/pages/release-management/`:
  - `release-management.component` — card list (search, environment/status filters, refresh, create); 3/2/1 per row; shows `REL-{id}`, folder name, and a simple "DLLs available / Waiting for DLLs" badge from `dllFileCount`/`folderReady`. **Auto-refreshes every 10s** (`startAutoRefresh`/`stopAutoRefresh`, paused while `isUserPerformingAction` during toggle/delete) so badges/test summaries update without a manual click.
  - `release-form/` — create AND edit (same component, `isEdit` flag driven by an `:id` route param, mirroring `EnvironmentFormComponent`). Release Name, Version, Environment dropdown, Description. No Build/Type/Branch/Tags fields (not proven required). Name/Version/Environment inputs are `[disabled]` once the release is no longer Draft (`canEditIdentity`), with an inline warning banner.
  - `release-details/` — info, **Release Readiness** card (Refresh button → `GET .../readiness`, lists DLL files found, Ready/Waiting badge), test summary, activate button (gated on `readiness.isReady`), sign-off (approve/reject, gated on testing complete), sign-off history, notifications, Edit button. **Auto-refreshes every 10s**: release data always refreshes; the reflection-heavy readiness check only re-runs while still `Draft` (stops once Active/Completed/Rejected). Paused during `activate()`/`signOff()` via `isUserPerformingAction`.
- Routes: `/release-management`, `/release-management/new`, `/release-management/edit/:id`, `/release-management/:id`.
  Sidebar link is admin-gated (`isAdmin`).
- Auto-refresh pattern (`refreshInterval`/`refreshSeconds`/`isUserPerformingAction`/`OnDestroy`)
  mirrors the existing convention in `test-case-execution-panel.component.ts` for consistency.

### Still TODO (future)
- Wire the execution-summary release selection by ReleaseId (ReleaseName/Version/Environment
  shown for display, ReleaseId used internally) — `usp_GetReleaseExecutionLogs` is still
  name-based (`@ReleaseName`); Test Case Assignment itself is done (see below).
- Finer role gating beyond `isAdmin` (Manager/Tester/Viewer) if required.
- Dashboard's library filter/summary (`dashboard.component.ts`) is hidden behind
  `libraryDiscoveryAvailable = false` since it has no Release selector — re-enable once a
  Release dropdown is added there too.
- Known pre-existing bug (not fixed, out of scope): `SqlReaderExtensions.GetNullable<T>`
  returns `default(T)` (e.g. `0` for `int`) instead of `null` for DB NULLs on value types.
  Harmless for `ReleaseId` (0 never resolves to a real release, so the TestQueueWorker's
  skip/retry logic still behaves correctly), but worth fixing centrally if it matters
  elsewhere.
- `aut.TestCaseAssignment.ReleaseId` is now `NOT NULL` (see below) — the 3 legacy
  (`ReleaseId = NULL`) assignments and their dependent rows were permanently deleted, so
  there's no longer a "legacy/unlinked assignment" case anywhere in the app.

## Test Case Assignment — Release-aware (Library + Discovery + Execution)

### Release replaces "Library-as-Release" + hardcoded Environment
The Assignment screen (`test-case-assignment-user.component`) now has a **Select Release**
dropdown (Active/Completed releases only) **before** the Test Suite (Library) dropdown.
The old "Select Environment" dropdown is gone — Environment is read-only, derived from the
selected Release (`selectedRelease.environmentName`).

- `AssignmentCreateUpdateRequest`/`ITestCaseAssignmentEntity` carry `ReleaseId`
  (`EnvironmentId` resolved server-side from the text `Environment` if not supplied).
- `TestCaseAssignmentsController.CreateOrUpdateAssignmentWithTestCasesAsync` validates the
  Release exists and `ReleaseLifecycle` is `Active` or `Completed` (400 otherwise) before
  calling the repository.
- **AssignmentName** keeps its exact original formula
  (`{Tester}-{Library}-{Environment}`) and, only when `@ReleaseId` is supplied, appends the
  real Release Name as a 4th segment (e.g. `vishnu-OnboardingTests-QA-Release_Sept`) — fully
  backward compatible; legacy rows with `ReleaseId = NULL` are untouched.
- Duplicate-assignment check ("is this test case already assigned to anyone") is now
  **Library + ReleaseId** scoped (`usp_GetAssignedTestCasesForLibraryAndRelease`,
  `GET /api/TestCaseAssignments/library-release-assigned-testcases`), replacing the old
  Library + Environment-text check for this screen (the old SP/endpoint is left in place,
  unused, in case anything else still calls it).

### Test discovery and execution moved from the global TestLibs folder to per-Release folders
Both DLL discovery and actual test execution are now scoped to **each Release's own**
`ReleaseFolderPath` instead of one global `TestSettings:TestLibsPath` folder (that config
key is left in `appsettings.json`, just unreferenced by these code paths):
- `TestSuitesController` (`GET libraries`, `GET GetAllTestCasesByLibrary`) now requires a
  `releaseId` query param, resolves the Release via `IReleaseRepository`, and scans
  `release.ReleaseFolderPath` (404 if the release doesn't exist, 400 if no folder path set).
- `ITestRunner.RunAsync(string libsPath, ...)` takes the folder to execute from as a
  parameter instead of reading a fixed path at construction.
- `usp_GetPendingExecutionQueues` now also returns `TCA.ReleaseId`; `TestQueueWorker`
  resolves `IReleaseRepository.GetByIdAsync(queue.ReleaseId)` per queue item to get the
  folder to execute from. If it can't be resolved (no ReleaseId, release deleted, or no
  folder path), the item is **left `Queued`** and retried on the next cycle (never marked
  `Failed` for this reason) — logged as `"Skipping queue item {id}: unable to resolve
  release folder for ReleaseId {id}. Will retry."`.
- All of this was smoke-tested end-to-end against the real `REL-10_Release_Sept_v1.0.0`
  folder: discovery returned only that release's own DLL's test cases, an assignment
  created against it produced the expected 4-segment AssignmentName, a queued execution
  correctly loaded/ran `OnboardingTests.dll` from that folder (failing first on a genuinely
  missing dependency DLL, then passing once it was added — proving the folder scoping
  works, not the global one), and a legacy `ReleaseId = NULL` assignment's queued item
  correctly stayed `Queued`/retried instead of failing (this legacy row no longer exists —
  see "Legacy assignment cleanup" below).

## Legacy assignment cleanup + `ReleaseId` now required
The 3 pre-Release-Management assignments (`ReleaseId = NULL`, library-as-release rows) and
all their dependent data were permanently deleted from the live DB (`AssignmentId 22/23/24`;
9 `AssignedTestCases`, 7 `TestCaseExecutionQueue`, 14 `TestCaseExecutionLogs`, 4
`TestScreenshots` rows) since they were no longer needed. `aut.TestCaseAssignment.ReleaseId`
was then altered to `NOT NULL` (dropping/recreating `IX_TestCaseAssignment_ReleaseId` around
the `ALTER COLUMN`, since SQL Server won't alter a column an index depends on). This is
captured in `Release_Management_Migration_Full.sql` as an idempotent, guarded section (only
tightens the column if it's still nullable **and** no `NULL` rows remain — never fails or
loses data if re-run) — the one-time DELETE itself is not part of the idempotent script
(it was a manual, explicitly-confirmed one-off cleanup).

## Test Case Execution Panel — Release-aware alignment
`test-case-execution-panel.component` now mirrors the Assignment screen's Release-awareness:
- A **Release filter** dropdown (scoped to releases the tester actually has assignments in,
  derived from their own `assignments` list) narrows the existing Assignment dropdown;
  selecting a release auto-selects its first matching assignment.
- The "Selected Assignment Name" card was replaced with a clearer info row: **Test Suite**
  (`assignment.releaseName` — historically named, actually the library), **Environment**
  (`assignment.environment`), and **Release** (`{releaseName} v{version}` + a lifecycle
  badge reusing `release-management.component.ts`'s `statusPillClass` color convention,
  reimplemented locally as `releaseLifecycleBadgeClass`). The raw `assignmentName` stays
  visible as a small muted subtitle for traceability.
- **Execution guard**: Run Now / Schedule (single + bulk) are blocked once the assignment's
  linked Release is no longer `Active` (e.g. `Completed`/`Rejected`) — enforced **server-side**
  in `TestCaseExecutionQueueController` (new `usp_GetAssignmentReleaseLifecycle` +
  `ITestCaseAssignmentRepository.GetReleaseLifecycleForAssignmentAsync`, returns `400 "This
  release is {lifecycle} and no longer accepts new test executions."`) and mirrored
  client-side (`isReleaseActive()`) for disabled buttons/rows and immediate toast feedback.
  The guard only applies at **submission** time — items already `Queued`/`Scheduled` before
  a release's lifecycle changes are **not** retroactively cancelled and still execute
  normally via the existing `TestQueueWorker` (verified end-to-end: queued-while-Active item
  completed successfully even after the release was later marked `Completed`).
- `releases` (for the lifecycle badge/guard) refresh on every auto-refresh tick (every 10s,
  alongside the existing test-case refresh), so a lifecycle change is reflected without a
  page reload.
