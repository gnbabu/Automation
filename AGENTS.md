# OHPNM Automation — Project Notes

## Structure
- `AutomationAPI/` — ASP.NET Core (.NET 8) Web API (controllers + repositories + SQL stored procedures).
- `ohpnm-test-portal/` — Angular frontend.
- `Database/` — SQL scripts (schema + stored procedures + migrations).
- `TestLibs/` — global folder of test DLLs used by the reflection-based discovery/execution.

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
  - `release-management.component` — card list (search, environment/status filters, refresh, create); 3/2/1 per row; shows `REL-{id}`, folder name, and a simple "DLLs available / Waiting for DLLs" badge from `dllFileCount`/`folderReady`.
  - `release-form/` — create AND edit (same component, `isEdit` flag driven by an `:id` route param, mirroring `EnvironmentFormComponent`). Release Name, Version, Environment dropdown, Description. No Build/Type/Branch/Tags fields (not proven required). Name/Version/Environment inputs are `[disabled]` once the release is no longer Draft (`canEditIdentity`), with an inline warning banner.
  - `release-details/` — info, **Release Readiness** card (Refresh button → `GET .../readiness`, lists DLL files found, Ready/Waiting badge), test summary, activate button (gated on `readiness.isReady`), sign-off (approve/reject, gated on testing complete), sign-off history, notifications, Edit button.
- Routes: `/release-management`, `/release-management/new`, `/release-management/edit/:id`, `/release-management/:id`.
  Sidebar link is admin-gated (`isAdmin`).

### Still TODO (future)
- Wire the existing assignment/execution UI to pass `ReleaseId` (SPs already accept it;
  legacy assignments keep working via retained text columns), and drive the
  execution-summary release selection by ReleaseId (ReleaseName/Version/Environment shown
  for display, ReleaseId used internally).
- Finer role gating beyond `isAdmin` (Manager/Tester/Viewer) if required.
