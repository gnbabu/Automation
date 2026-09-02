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
- **Fixed**: "Finer role gating beyond `isAdmin`" — `aut.UserRole` only has 3 real roles
  (`Admin`, `User`, `Manager`; no separate Tester/Viewer). Managers already received
  Release activation/DLLs-ready email notifications and are the natural approver role, so
  they now get UI parity with Admin on **Release Management** (+ its 4 sub-routes),
  **Dashboard**, and **Test Case Assignment** — `AuthService.isManager()` +
  `canAccessManagerFeatures()` (`isAdmin() || isManager()`), a new `managerGuard`
  (mirrors `adminGuard`, same redirect-to-`/test-case-execution-panel` fallback) swapped in
  for those routes in `app.routes.ts`, and the corresponding sidebar links switched from
  `*ngIf="isAdmin"` to `*ngIf="canAccessManagerFeatures"`. **Users Management** and
  **Environment Management** stay strictly `isAdmin`/`adminGuard`-only (system/account
  administration, not a Manager duty). Enforcement is **UI-only** (route guard + sidebar),
  matching the existing convention — API controllers keep their blanket `[Authorize]`, not
  `[Authorize(Roles=...)]` (see Dashboard's "Access" note below for why). Since no page has
  any internal `isAdmin`-gated button/action (confirmed via search), Managers get full
  functional parity (create/edit/delete/activate/sign-off/assign), not just read-only
  visibility, on these 3 pages.
- **Fixed**: `aut.UserRole` also has a real, actively-used `Viewer` role (RoleID 4; 5 live
  users) that was initially missed (only found by directly querying the live DB — the
  `User`/`Tester` naming differs across seed scripts, and one seed script omits `Viewer`
  entirely). Unlike Manager (more access), Viewer means **less** access: read-only. Two
  write actions were previously open to Viewers exactly like Testers (no gating existed):
  - **Test Case Execution Panel**: Run Now / Schedule (single + bulk) are now blocked for
    Viewers. `AuthService.isViewer()` + `TestCaseExecutionPanelComponent.canExecuteTests()`
    (`isReleaseActive() && !isViewer()`) replace the old bare `isReleaseActive()` checks in
    all 4 action handlers, the bulk-button `[disabled]`s, and `isTestCaseSelectable` (so
    Viewers can't even select rows to bulk-act on); a dedicated warning banner explains why.
    Also enforced **server-side** in `TestCaseExecutionQueueController` (`IsViewer() =>
    User.IsInRole("Viewer")`, checked in all 4 actions — `SingleRunNow`/`BulkRunNow`/
    `SingleSchedule`/`BulkSchedule` — returning `403`), since a Viewer's valid JWT could
    otherwise call the API directly. The JWT's `ClaimTypes.Role` is already `user.RoleName`
    (`AuthService.cs`), so `User.IsInRole("Viewer")` works with no auth changes needed.
  - **Test Data Management**: unlike the Execution Panel (kept visible but read-only for
    Viewers), this page is **hidden entirely** from Viewers — it's about editing test input
    data, so there's no useful "view" mode of it, per explicit decision. Sidebar link is
    `*ngIf="!isViewer"`, and the `/test-data-management` route got a new `notViewerGuard`
    (redirects to `/test-case-execution-panel`, same fallback as the other guards) so direct
    navigation is blocked too. The component-level `onSubmit`/`[readonly]`/`[disabled]`
    guards (added first, before the page was hidden) were left in place as defense-in-depth
    for the brief window before the guard/sidebar change, and are now effectively unreachable
    in normal use. Also enforced server-side in `AutomationController`'s
    `InsertAutomationDataAsync`/`UpdateAutomationDataAsync` (`IsViewer()` pattern, `403`) —
    this stays regardless of UI visibility, since it's the only thing that actually stops a
    direct API call. The Section CRUD endpoints (`sections` POST/PUT/DELETE) were
    deliberately **not** touched — this page never calls them (confirmed via the component),
    so gating them was out of scope for this fix.
  - Unlike the Manager change above, this one **is** enforced server-side (not just UI),
    since it's blocking a genuinely undesired write, not just hiding a page.
- **Fixed**: `SqlReaderExtensions.GetNullable<T>` used to return `default(T)` (e.g. `0` for
  `int`, `DateTime.MinValue`/`"0001-01-01T00:00:00"` for `DateTime`) instead of a true
  `null` for DB NULLs on value types. Long flagged as "harmless" (e.g. for `ReleaseId`,
  since `0` never resolves to a real release) until the Dashboard's Run Details/Timeline
  work actually broke on it: a never-started test case's `StartTime`/`EndTime` came back
  as the sentinel `0001-01-01` instead of `null`, which is truthy in JS and silently
  corrupted the min/max "release execution window" calculation. Fixed centrally —
  `GetNullable<T>` is now `where T : struct` and returns a real `Nullable<T>`;
  `GetNullableString` got its own direct (non-generic) implementation since `string` is a
  reference type and can't satisfy that constraint. Confirmed only the three
  `GetNullableInt`/`GetNullableDateTime`/`GetNullableString` wrappers call the generic
  directly, so this was a safe, fully-contained fix (no other call sites to break). A few
  fields (`TestCaseAssignmentEntity.AssignedDate`/`LastUpdatedDate`,
  `TestScreenshotRepository`'s `TakenAt`) already explicitly coalesced to
  `DateTime.MinValue` themselves in their own mapping code, so their behavior is
  unaffected by this fix — only fields with no such explicit fallback (like `StartTime`/
  `EndTime`) actually changed, correctly, to real nulls.
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
- **Tried and reverted**: briefly experimented with showing every discovered test case
  (instead of hiding ones assigned to a different tester) plus a disable-checkbox-only-if-
  `Passed`/`Failed` rule (`isTestCaseSelectable`/`[rowSelectableFn]`, `ITestCaseModel.
  testCaseStatus`), since the backend has no constraint against the same `TestCaseId` being
  assigned to multiple testers (confirmed via `sp_helptext` on
  `usp_CreateOrUpdateAssignmentWithTestCases` - each Tester+Library+Release combination gets
  its own `AssignmentId`, so this would've been safe DB-wise). Ended up causing several
  follow-on bugs in quick succession (wrong-tester status leaking across testers, a stale
  in-flight request race when switching testers, `onSaveAssignments()` clobbering an
  already-executed test case's real status back to `'Assigned'`). Compared directly against
  the exact pre-Release-Management version (`git show 932c73a:...
  test-case-assignment-user.component.ts`, before `4ae06da` "Make Test Case Assignment
  Release-aware") and confirmed it had **no** disable-checkbox concept either - just the
  same hide-if-assigned-to-someone-else filter, scoped by `Environment` text instead of
  `ReleaseId`. Per explicit request, **reverted to that exact proven filter-based
  behavior** (`tryLoadTestCases()`'s Step 4 hides a test case if it's assigned to anyone
  other than the currently selected tester; `onSaveAssignments()` always sends
  `testCaseStatus: 'Assigned'`), just kept Release-scoped instead of Environment-text-scoped.
  One improvement kept from the experiment: `tryLoadTestCases()` now guards against the
  stale-in-flight-request race with a `loadRequestId` generation counter (switching User/
  Library/Release while a previous load is still in flight now discards that stale
  response instead of risking it overwriting the current selection) - this protection is
  unrelated to the disable-checkbox idea and is safe to keep with the reverted filter logic.
- **Fixed**: "Reset Assignments" (and, more generally, deselecting/removing any previously-
  saved test case via Save) failed outright with a `500` — `SqlException: The DELETE
  statement conflicted with the REFERENCE constraint
  'FK_TestCaseExecutionQueue_AssignedTestCase'` — for any test case that had ever actually
  been queued/executed (a real `aut.TestCaseExecutionQueue` row referencing it) or had a
  screenshot (`aut.TestScreenshots`). Root cause: `usp_CreateOrUpdateAssignmentWithTestCases`
  hard-deletes `aut.AssignedTestCases` rows no longer present in `@TestCases`, but neither
  FK has `ON DELETE CASCADE` - pre-existing (confirmed via `sp_helptext`/direct repro this
  wasn't introduced by any Release/Manager/Viewer work this session), just never exercised
  on a test case with real execution history until now. **Fixed** in
  `Database/TestCaseAssignment_Reset_FK_Fix_Migration.sql` (idempotent `CREATE OR ALTER`,
  applied to the live DB and verified end-to-end via a real Reset call against a live
  assignment): before the existing "delete removed test cases" step, the SP now also
  deletes the matching `TestCaseExecutionQueue` and `TestScreenshots` rows (the actual
  FK-enforced blockers) and `TestCaseExecutionLogs` rows (no FK forces this, but leaving
  them orphaned pointing at a since-deleted `AssignmentTestCaseId` made no sense either) for
  exactly the `AssignmentTestCaseId`s about to be removed. Un-assigning/resetting a test
  case is a deliberate action, so clearing its execution history along with it is the
  correct behavior - re-assigning it later starts fresh. Verified via `sys.foreign_keys`
  that none of the affected tables have a filtered index, so `QUOTED_IDENTIFIER` isn't
  actually load-bearing here, but set it explicitly anyway per this project's established
  convention.
- **Changed** (per explicit request): "Reset Assignments" used to be a soft delete - it
  cleared all `AssignedTestCases` (and, after the fix above, their dependent Queue/
  Screenshots/Logs) but left the `aut.TestCaseAssignment` row itself in place with
  `AssignmentStatus = 'Removed'` (verified via a real Reset call: the row stayed, just with
  that status). Now, resetting an **existing** assignment down to zero test cases
  permanently **deletes the `TestCaseAssignment` row itself** (`Database/
  TestCaseAssignment_Reset_Permanent_Delete_Migration.sql`, idempotent `CREATE OR ALTER`,
  applied to the live DB and verified with a self-contained create-then-reset test: the
  assignment and its test case were both completely gone afterward, not just status-
  flipped) - cleaning up `TestCaseExecutionQueue` (by both its FKs -
  `AssignmentTestCaseId` via `AssignedTestCases` and the direct `AssignmentId` FK),
  `TestScreenshots`, and `TestCaseExecutionLogs` first, same as the fix above. The next
  time that Tester+Library+Release combination gets test cases assigned again, a brand-new
  `AssignmentId` is created - there's no lingering `'Removed'` row. Creating a brand-new
  assignment with zero test cases is unchanged (still just no-ops/rolls back - nothing to
  create or delete either way).

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

## Dashboard ("Test Case Execution Summary") — Release-aware
`dashboard.component` was fully dormant (hidden behind `libraryDiscoveryAvailable = false`)
after discovery moved off the global `TestLibs` folder; it's now revived, Release-scoped:
- A **Select Release** dropdown (Active/Completed only) replaces the old Library dropdown.
  Auto-selects the most recently created release (`GET /api/Release` is already sorted by
  `CreatedOn DESC`) once the list loads, so the page shows data immediately instead of an
  empty state — same "auto-select first item" convention as the Assignment/Execution Panel
  screens. No auto-refresh timer — instead a manual **Refresh** button (backed by the same
  `refreshReleaseData()` used on selection) re-pulls the current Release's data on demand,
  since this page's load (discovery across every library in the release + the full assigned
  list + logs, all merged client-side) is heavier than the other two Release-aware screens.
- The "Total Cases / Unassigned" discovery-vs-assigned comparison (and the 4-card
  Total/Passed/Failed/Running+Skipped summary) is now computed **across every library** in
  the Release's folder, not just one: `TestSuitesService.getLibraries(releaseId)` (already
  Release-scoped) is flattened client-side into a flat test-case list and merged with a new
  `usp_GetAllAssignedTestCasesForRelease(@ReleaseId)` (mirrors the existing per-Library SP,
  minus the library filter) — same discovery-vs-assigned merge pattern used elsewhere, just
  widened from one library to a whole release. Deliberately **not** using `IReleaseModel`'s
  own `TotalTests`/`PassedTests`/etc. aggregates here, since those count only *assigned*
  tests, whereas this page's "Total" has always meant *discoverable* tests (assigned +
  unassigned) — using both on the same page would show two different "Total" numbers.
- `usp_GetReleaseExecutionLogs` switched from name-based (`@ReleaseName`, which actually
  matched the historically-misnamed Library-name text column) to `@ReleaseId` — safe since
  this dashboard was its only caller and was unreachable. Also fixed a latent bug: `LogId`
  was missing from the SELECT, which `execution-logs-viewer.component.ts`'s `trackBy`
  silently depended on (`log.logId`); verified via a real log insert that `logId` now comes
  through correctly.
- Fixed `LibraryMethodInfoMapper.fromApi` (`core/mappers/index.ts`), which only mapped
  `methodName` and silently dropped `testCaseId`/`description`/`priority` even though
  `LibraryMethodInfo` already declared them — this made every "Unassigned" merged row on
  this page show blank details, since the old single-Library flow used a different,
  non-mapped endpoint that never hit this bug. Safe fix: this mapper is otherwise only used
  to populate Library-name-only dropdowns elsewhere, so the extra fields don't affect them.
- **Access**: gated `isAdmin`-only, same convention as Release/Environment/User Management
  and Test Case Assignment (sidebar link hidden via `*ngIf="isAdmin"` in
  `left-sidebar.component.html`) — this page surfaces **every** tester's results for a
  Release, not just the logged-in user's own (unlike the Execution Panel, which stays open
  to everyone since it's scoped to "my assignments"). Also gated at the **route** level via
  a new `adminGuard` (`core/guards/admin.guard.ts`, applied alongside `authGuard` to every
  `isAdmin`-gated route: Dashboard, Users, Test Case Assignment, Release Management +
  sub-routes, Environment Management + sub-routes) — a non-admin navigating directly to
  `/dashboard` is redirected to `/test-case-execution-panel` instead of hitting a broken
  page. At the **API** level, every controller now requires `[Authorize]` (login required) —
  deliberately **not** role-specific (`Roles = "Admin"`), so a logged-in Tester with a valid
  token could still call the Dashboard's backing endpoints directly; only the UI/route hides
  them. This was an explicit choice (see git history) over per-endpoint role checks, partly
  because some backing endpoints (e.g. `GET /api/Release`) are also legitimately used by
  non-admin pages (the Execution Panel's release filter), so blanket role-restricting shared
  endpoints isn't safe without splitting them. Tracked under "Finer role gating" below if
  stronger, endpoint-specific enforcement is wanted later.
- "Run Details & Timeline" card: **Start Time / End Time / Execution Duration are now real**
  (`computeRunTimeline()` in `dashboard.component.ts`), derived from the min `StartTime` /
  max `EndTime` across the currently-loaded, Release-scoped `testCases` — no fabricated
  data. Three honest states: `Not Started` (nothing has a `StartTime` yet → `—`),
  `In Progress` (some cases started but not all finished → shows the real start, "In
  Progress" for end, "Running…" for duration), `Completed` (everything that started has
  finished → real Start/End + formatted `Xm Ys` duration). Verified against real DB data
  (Release 10: 2 `Passed` cases with real timestamps + 1 still `Assigned`/never-started,
  correctly excluded from the window). **"Data Cleanup Status" was removed** (confirmed via
  a full codebase/data-model search that no "cleanup" concept exists anywhere in this app;
  building real tracking for it was explicitly decided against as out of scope) and
  replaced with two more rows, both genuinely new information not already shown elsewhere
  on the page: **Testers Involved** (distinct count of `assignedUserName` among test cases
  that have started) and **Average Test Duration** (mean of individual `Duration` values).
  `formatDuration()` shows decimal-second precision (`"0.29s"`) for sub-second durations
  instead of rounding down to a misleading `"0m 0s"` — real, quick/API-driven test
  executions are commonly well under a second. Verified against real DB data (Release 10:
  testers `{Nareshg, saharshg}` = 2, correctly excluding the still-`Assigned`/never-started test
  case from the tester count).

## Test Data Management — scoped by Environment (not Release)
Unlike Assignment/Execution Panel/Dashboard, `aut.AutomationData` (per-user Flow/Section
test input content, e.g. `NewProviderDTO` field values for the Registration flow) has **no
relationship to Release/Library/TestCase** — its only new scoping dimension is Environment,
per explicit decision (there's no Release/lifecycle concept that applies to raw test input
data):
- `aut.AutomationData` gained `EnvironmentId` (FK to `aut.Environment`, now `NOT NULL`).
  The 27 pre-existing rows were backfilled to **QA** (`EnvironmentId = 8`) in a one-time,
  explicitly-confirmed `UPDATE` — captured as an idempotent guarded migration in
  `Database/AutomationData_Environment_Migration.sql` (only backfills/tightens if the
  column is still nullable and no NULLs remain, so re-running is always a safe no-op).
- `usp_GetAutomationData`/`usp_InsertAutomationData` now take `@EnvironmentId`.
  `usp_UpdateAutomationData` is unchanged — Environment (like Section/User) is fixed at
  creation, never edited afterward, matching the same "identity fields are immutable"
  convention used for `TestCaseAssignment`.
- `TestDataManagementComponent` gained a "Select Environment" dropdown (reusing
  `EnvironmentService`, same pattern as other pages) alongside Flow/Section. Per explicit
  decision, a Flow/Section/Environment combo with no saved data starts **empty** — no
  cross-environment pre-fill, even if the same user already has content for that
  Flow/Section under a different Environment. Saving creates an independent row per
  Environment (verified directly via SQL: inserting a second environment's content for the
  same Section+User left the first environment's row untouched).
- `usp_GetAutomationDataByFlowName` was **not** touched — confirmed it has zero frontend
  callers (dead code, pre-existing), so it wasn't worth updating for a dimension nothing
  reads it through.

## Settings page — self-service Profile editing + Users API privilege-escalation fix
`SettingsComponent` (`/settings`, open to every authenticated role) previously only showed
Username/Email/FullName/Role/Photo as static text plus Change Password. It now also lets a
user edit their own **Photo, Phone Number, Time Zone** (Role/Status/Priority/Active/Teams
stay Admin-only, edited via User Management) — split into a **Profile** card and a
**Security** card (existing Change Password, just regrouped), matching this app's existing
stacked-`.filter-card` convention (no new tab UI introduced).

### Fixed: `PUT/POST/DELETE /api/Users` had no role restriction (privilege escalation)
`UsersController`'s `CreateUser`/`UpdateUser`/`DeleteUser`/`SetUserActiveStatus` (`activate`)
only had blanket `[Authorize]` — no `Roles = "Admin"` check and no ownership check. Any
logged-in user (Tester/Viewer included) could call `PUT /api/Users` directly (their own
valid, legitimately-issued token — no exploit needed, just curl/Postman/devtools) with an
arbitrary `UserId` + `RoleId: 1` in the body and grant themselves Admin, entirely bypassing
the Users page being hidden from non-admins in the sidebar/routes — a UI-side restriction
has zero effect on what the server accepts, since the server can't know or verify how a
request arrived (same class of gap as the Viewer bypass fixed earlier, just privilege
escalation instead of a read-only bypass, so more severe). **Fixed** by adding
`[Authorize(Roles = "Admin")]` to those 4 actions specifically (confirmed via grep: today
only `AddEditUserComponent`/`UserListComponent`, both inside the already Admin-gated `/users`
page, call them — so this is a non-breaking lockdown). `GetAllUsers`/`GetUserById`/
`GetUsers` (Filters)/`roles`/`status`/`timezones`/`priorities` are unchanged (blanket
`[Authorize]`), since non-admin pages legitimately read from these (dropdowns, self-profile
fetch).

**Flagged, deliberately not fixed** (kept out of scope to avoid scope creep on this task):
- `GetUserById` still lets any authenticated user fetch **any** user's record by ID (minor
  info-disclosure of another user's email/phone/photo). Settings only ever calls it with the
  caller's own ID today, but nothing enforces that server-side.
- `ChangePassword` (`UsersController`) has the same "trusts `UserId` from the request body"
  pattern as the old `UpdateUser` did — though less severe, since you'd still need to know
  the target user's *current* password to successfully change it via this route.

### New: JWT carries a real user-id claim
`AuthService.Login`'s claims were `Name`/`Email`/`Jti`/`Role` only — no user-id claim, so the
server had no way to verify "is this really you" for any future self-service endpoint (the
frontend just trusts `getLoggedInUserId()` from `localStorage`). Added
`new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())` at login — purely additive.
**Note**: users already logged in when this shipped are holding an old token without this
claim; `PUT /api/Users/me/profile` returns `401` for them until they log out/in again to get
a fresh token (expected, one-time transitional behavior, not a bug).

### New: `PUT /api/Users/me/profile` — self-service profile update
- New DTO `UpdateOwnProfileRequest { Photo?, PhoneNumber?, TimeZone? }` — deliberately **has
  no `UserId` field at all**. The endpoint always derives the caller's identity from
  `User.FindFirstValue(ClaimTypes.NameIdentifier)` (the JWT claim above), never from the
  request body, so there's no way to target another user's row through it. No role
  restriction — any authenticated user may update their own profile this way.
- Backed by a **new, narrower** stored proc `usp_UpdateUserProfile`
  (`Database/User_Self_Profile_Migration.sql`, idempotent `CREATE OR ALTER`, already applied
  to the live DB) rather than reusing the existing full-row `usp_UpdateUser`/`UpdateUserAsync`
  via a fetch-merge approach. Reason: `UserRepository.GetUserByIdAsync`'s SELECT doesn't map
  `PriorityId`/`Status` into the `User` object at all (pre-existing gap — those columns exist
  and are written by `UpdateUserAsync`/`CreateUserAsync`, just never read back out). A
  fetch-full-user-then-mutate-3-fields-then-call-`UpdateUserAsync` approach would have
  silently NULLed out the user's real Status/Priority on every self-save. The new proc only
  ever touches `Photo`/`PhoneNumber`/`TimeZone` — `RoleID`/`Active`/`Status`/`PriorityId`/
  `Teams`/`UserName`/`Email`/`PasswordHash` are never in its `UPDATE` statement, so there's
  no merge/fetch step and no way for it to clobber an unrelated field. `@Photo` is only
  overwritten when a new photo was actually supplied (`COALESCE(@Photo, Photo)`), since a
  user editing just their phone/time zone shouldn't have to re-upload a photo every time;
  `@PhoneNumber`/`@TimeZone` are always set directly (including clearing them).
- `IUserRepository`/`UserRepository.UpdateOwnProfileAsync` reuses the exact same base64→
  `VARBINARY` photo-conversion pattern already used by `UpdateUserAsync`/`CreateUserAsync`.
- Frontend: `IUpdateOwnProfileRequest` (also no `userId` field, matching the backend DTO) +
  `UsersService.updateOwnProfile()`. `SettingsComponent`'s "Edit Profile" toggle reuses the
  same base64 `FileReader` upload pattern already in `AddEditUserComponent`, and the Time
  Zone `<select>` reuses `UsersService.getTimeZones()` the same way the admin form does.

### Fixed (found while testing the redesigned Profile card): `GET /api/Users/{id}` was silently dropping fields
`usp_GetUserById`/`UserRepository.GetUserByIdAsync` — used **only** by Settings
(`GET Users/{id}` for the logged-in user's own record) — never selected/mapped
`Status`/`StatusName`/`Priority`/`PriorityName`/`LastLogin`, and selected-but-never-mapped
`TimeZoneName`. Pre-existing gap (not introduced by this work): `usp_GetAllUsers` already
joined `UserStatus`/`PriorityStatus`/`TimeZone` and selected all of these correctly — 
`usp_GetUserById` just never got the same treatment. Invisible until now because nothing
previously displayed Status/Last Login on the page that calls this endpoint. **Fixed**:
`usp_GetUserById` rewritten to match `usp_GetAllUsers`'s joins/columns
(`Database/User_GetById_Fix_Migration.sql`, idempotent `CREATE OR ALTER`, already applied
to the live DB and verified via `sqlcmd`); `GetUserByIdAsync`'s mapping updated to read all
of them. Confirmed via direct SP execution: `Status`/`LastLogin`/`Priority`/`TimeZoneName`
now come through correctly for a real user.

Also fixed while investigating: `GetPhotoBase64` (shared by `GetAllUsersAsync`/
`GetUserByIdAsync`/`GetUserByUsernameAsync`/`GetFilteredUsersAsync` — one shared helper, all
4 fixed at once) hardcoded `data:image/png;base64,...` regardless of the photo's actual
format. Verified directly (isolated SP+reader test outside the API) that a real user's
stored photo is actually a JPEG (`FF D8 FF` signature), not a PNG — so every photo was being
mislabeled. Most browsers still render a mislabeled `data:` URI via content-sniffing (so this
wasn't necessarily the cause of any specific "photo won't display" report), but it was
objectively wrong regardless, so a `GetImageMimeType()` byte-signature sniff (JPEG/PNG/GIF/
WebP, defaulting to PNG if unrecognized) was added and is now used for the MIME type instead
of a hardcoded assumption.

### Fixed (found via a live Viewer-account test): `usp_UpdateUserProfile` failed with a QUOTED_IDENTIFIER error
Saving the new "Edit Profile" form threw `SqlException: UPDATE failed because the following
SET options have incorrect settings: 'QUOTED_IDENTIFIER'` for every user, reproduced live
using a real Viewer JWT. Root cause: `aut.[User]` has a filtered index
(`IX_User_ResetPasswordToken`) — same class of constraint already documented for
`aut.Release`'s filtered unique index above — so any UPDATE against it requires
`QUOTED_IDENTIFIER ON`. A stored procedure **bakes in** whatever `QUOTED_IDENTIFIER` setting
was active in the session at `CREATE`/`ALTER` time; running the migration via plain
`sqlcmd ... -i script.sql` (no `-I` flag) created `usp_UpdateUserProfile` with it baked in as
OFF, so every call failed at runtime regardless of the caller. **Fixed** by adding an
explicit `SET QUOTED_IDENTIFIER ON` + `GO` directly in
`Database/User_Self_Profile_Migration.sql` before the `CREATE OR ALTER PROCEDURE`, so the
correct setting is self-contained in the script regardless of how/with-what-flags it's ever
re-run — re-applied to the live DB and verified via `sys.sql_modules.uses_quoted_identifier`
(now `1`) and a real `EXEC ... UPDATE` against a live Viewer account (`UserID 22`, phone
number actually persisted). `usp_GetUserById` (`User_GetById_Fix_Migration.sql`, added
earlier in this same session) got the same fix for consistency, even though it's SELECT-only
so isn't actually affected by this specific error at runtime.

### Fixed: sidebar didn't reflect a profile update until reload
`LeftSidebarComponent` lives outside `<router-outlet>` (rendered once by `LayoutComponent`
for the whole session — see `layout.component.html`), so it only ever read
`AuthService.getLoggedInUser()` once, in its constructor. Saving a profile change on
Settings updated `SettingsComponent`'s own `user` field and `localStorage`, but the
already-alive sidebar instance never re-read either, so its photo/name stayed stale until a
full page reload. **Fixed** by adding a reactive `AuthService.currentUser$`
(`BehaviorSubject<IUser | null>`, seeded from `localStorage` on service creation) and a new
`AuthService.setCurrentUser(user)` (updates both `localStorage` and the subject — used by
`login()` and by `SettingsComponent.loadUserDetails()` after every fetch, including right
after a successful save). `LeftSidebarComponent` now subscribes to `currentUser$` in
`ngOnInit` (unsubscribed in `ngOnDestroy`) instead of only reading once, so it re-renders
immediately when Settings pushes a change — no reload needed. `getLoggedInUser()`/
`isAdmin()`/`isManager()`/`isViewer()`/etc. were left reading `localStorage` directly and
unchanged (still correct, just not reactive) since everywhere else calls them synchronously
and doesn't need push updates.
