# Release Management — Complete Flow Documentation

## 1. Overview

**Release** is the root business entity for the automation lifecycle. One Release belongs to exactly one Environment, has its own dedicated folder on disk, and drives DLL discovery/readiness, activation, test execution association, and sign-off — while preserving all pre-existing automation functionality (test discovery, assignment, execution, logs, screenshots, results).

DLLs are **never uploaded through this application** — they are placed into the release folder by the existing controlled build/deployment process outside the app. Release Management only *detects* whether usable test DLLs are present; it never stores, uploads, or version-validates them.

## 2. Lifecycle States

```
Draft ──(Activate)──► Active ──(Sign Off: Approved)──► Completed
                          │
                          └──(Sign Off: Rejected)──► Rejected (rework/retest, then Sign Off again)
```

| State | Meaning |
|---|---|
| `Draft` | Just created. Folder exists; identity fields (Name/Version/Environment) are still editable. Waiting for DLLs / not yet activated. |
| `Active` | Activated — available for testing. Identity fields now locked; Description remains editable. |
| `Completed` | Sign-off approved. Terminal. |
| `Rejected` | Sign-off rejected. Supports retest → sign-off again. |

Independently, `IsActive` (true/false) is a **soft delete/deactivate flag**, orthogonal to `ReleaseLifecycle` — a release can be deactivated ("Disabled") at any lifecycle stage without losing data.

## 3. End-to-End Flow

### Step 1 — Create Release
UI: `release-form` (Create mode) → `POST /api/Release`
- User provides: **Release Name**, **Version**, **Environment** (dropdown of active environments from Environment Management), **Description**.
- Backend validates: Name/Version/Environment required; Environment must exist and be **active**.
- **Uniqueness**: `ReleaseName + Version + EnvironmentId` (DB filtered unique index `UX_Release_Name_Version_Env`) — 409 Conflict on duplicate.
- Insert happens **first** (folder name embeds the new `ReleaseId`), then the folder is resolved and physically created:
  ```
  <ReleaseSettings:RootPath>\<EnvironmentName>\REL-<ReleaseId>_<ReleaseName>_v<Version>
  ```
  e.g. `D:\Releases\DEV\REL-6_Onboarding-Release_v2.5.0`
- **If folder creation fails**, the just-inserted row is deleted (`usp_DeleteRelease`, compensating transaction) so a failed Create is never reported as successful.
- Result: `ReleaseLifecycle = Draft`, `IsActive = true`, `SignOffStatus = Pending`.

### Step 2 — DLLs placed into the folder (external process)
The existing build/deployment process copies the release's test DLLs into that folder. The app takes no action here — it's a filesystem event outside the application.

### Step 3 — Readiness detection
Two mechanisms, both reusing the same reflection technique as `TestSuitesRepository`/`ReflectionTestRunner` (`Assembly.LoadFrom` + NUnit `TestFixtureAttribute` scan), scoped to the release's own folder:
- **`GET /api/Release/{id}/readiness`** — full check: lists DLL files found, counts how many are *usable* (contain test fixtures), returns `IsReady` + a message. Used by the Details page's Readiness card and as the server-side Activation guard.
- **Cheap list-level check** (`GetDllFileCount`) — non-reflective file count, used for the `dllFileCount`/`folderReady` badges on the card list and Details page — no reflection cost.

**UI auto-refresh**: both the List and Details pages poll every 10s (`setInterval`, paused during in-flight actions via `isUserPerformingAction`, cleared on `ngOnDestroy`) so these badges update live without a manual click. The Details page only re-runs the *reflection-heavy* readiness check while still `Draft` (skips it once Active/Completed/Rejected, since it no longer gates anything).

**Background worker** (`ReleaseDllsReadyNotificationWorker`, every 30s): scans all Draft releases with a folder set; the moment one becomes ready, sends **exactly one** proactive email notification ("DLLs ready — please activate") to active Manager/Admin users, recorded as `aut.ReleaseNotification` (`NotificationType = "DllsReadyForActivation"`), deduplicated so it's never resent. **This worker never activates anything** — it only notifies.

### Step 4 — Activate Release (manual, human action)
UI: Details page "Activate Release" button (enabled only when `readiness.isReady === true` and lifecycle isn't Active/Completed) → `POST /api/Release/{id}/activate`
- Server **re-validates readiness live** (independent of any UI staleness) — 400 if not ready.
- Guards: environment must be active, folder path must be set (`usp_ActivateRelease`).
- On success: `ReleaseLifecycle = Active`, `ActivatedBy`/`ActivatedOn` recorded.
- Sends notification to active Manager/Admin users ("release available for testing"), `NotificationType = "ActivatedForTesting"`, via the shared `IReleaseNotificationService`. Email failures are recorded as `Failed` and **never block activation**.

### Step 5 — Existing automation takes over
Test discovery, assignment, execution, logs, screenshots, and results continue to use the **existing, unmodified** automation infrastructure. (Note: today this still discovers/executes against the global `TestSettings:TestLibsPath`, not per-release folders — wiring `ReleaseId` through assignment/execution end-to-end is listed as future work.)

### Step 6 — Testing completes → Sign-Off
UI: Details page Sign-Off panel, shown only once `canSignOff` is true (`Active` **and** all assigned tests for the release are terminal — no `Running` tests) → `POST /api/Release/{id}/signoff`
- Body: `{ signOffStatus: "Approved"|"Rejected", signOffBy, comments }`
- Server re-validates: blocks if any assigned tests are still non-terminal, or if there are zero assigned tests.
- Writes a row to `aut.ReleaseSignOff` (full history, `GET /api/Release/{id}/signoff-history`) and updates the Release's current snapshot (`SignOffStatus`/`SignedOffBy`/`SignedOffOn`).
- `Approved` → `ReleaseLifecycle = Completed`. `Rejected` → `ReleaseLifecycle = Rejected` (rework/retest, then sign off again).

### Editing / Deactivating / Deleting (any time, not just forward-flow)
- **Edit** (`PUT /api/Release/{id}`): allowed anytime, but **Name/Version/Environment are locked once not Draft** (400 if changed) — they're baked into the immutable folder path and any recorded test history. Description always editable. Folder is **never** renamed, even if Name/Version change while still Draft.
- **Deactivate/Reactivate** ("Disable"/"Enable"): toggles `IsActive` via the same `PUT`, safe at any lifecycle stage, fully reversible, preserves everything.
- **Delete** (`DELETE /api/Release/{id}`): **permanent**, only allowed while `Draft` (400 otherwise — deactivate instead for anything past Draft). Removes the DB row and the physical folder; blocked with 409 if `TestCaseAssignment` rows already reference it.

## 4. Roles
Admin/Manager: create, edit, activate, sign-off, delete (Draft), deactivate. Tester/Viewer: view only (sidebar link currently gated by `isAdmin`; finer per-role gating is listed as future work).

## 5. Key Files
| Layer | File |
|---|---|
| DB | `Database/Release_Management_Migration_Full.sql` (canonical, current-state script) |
| Backend | `ReleaseController.cs`, `ReleaseRepository.cs`, `ReleaseFileService.cs`, `ReleaseReadinessService.cs`, `ReleaseNotificationService.cs`, `Repositories/Workers/ReleaseDllsReadyNotificationWorker.cs` |
| Frontend | `release-management.component.*` (list), `release-form.component.*` (create/edit), `release-details.component.*` (details/activate/sign-off) |
| Docs | `AGENTS.md` (technical reference, kept up to date throughout) |
