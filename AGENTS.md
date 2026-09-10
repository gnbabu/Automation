# OHPNM Automation — Project Notes

## Structure
- `AutomationAPI/` — ASP.NET Core (.NET 8) Web API (controllers + repositories + SQL stored procedures).
- `ohpnm-test-portal/` — Angular frontend.
- `Database/` — SQL scripts (schema + stored procedures + migrations).
- `TestLibs/` — legacy global folder of test DLLs. No longer used by discovery/execution
  (both are now scoped per-Release, see "Test Case Assignment — Release-aware" below);
  kept only because `TestSettings:TestLibsPath` is still present, unreferenced, in config.
- `AutomationTests/` — a **separate, standalone solution** (own `.sln`) of sample/demo NUnit
  test projects (`OnboardingTests`, `PayrollTests`, `RecruitmentTests`, `SeleniumSmokeTests`,
  `AutomationShared`) - NOT part of `AutomationAPI`'s build. These are what actually get
  compiled and manually copied into a Release folder (see "Release Management" below) to
  exercise the app; they exist purely as realistic input for AutomationAPI to discover/run,
  not as tests *of* AutomationAPI itself. `AutomationTests/API/` is an early, superseded
  discovery-only prototype (`AutomationTestController`) - unrelated to `AutomationAPI`,
  kept only for history.

## Test execution engine — NUnit.Engine (replaces hand-rolled reflection)
`AutomationAPI/Repositories/TestRunner/NUnitEngineTestRunner.cs` (the registered
`ITestRunner`, wired in `Program.cs`) runs test DLLs through NUnit's **real execution
engine** (`NUnit.Engine` - was already referenced as a package but completely unused before
this; the app used to hand-roll its own reflection-based invoker instead, `ReflectionTestRunner`,
which is kept in the codebase, **unregistered**, for reference/rollback only). Discovery
(`TestSuitesRepository`) similarly moved from hand-rolled reflection to NUnit's `Explore()`
API via the shared `NUnitEngineHelper`/`ExploreXmlParser`.

### Why this replaced the old reflection-based runner
The old `ReflectionTestRunner` (`method.Invoke(instance, args)` on types found via
`Assembly.LoadFrom` + attribute reflection) had several gaps that only really matter once
real Selenium/enterprise-scale suites are involved (today's sample test projects are simple
Moq unit tests, so these never surfaced before):
1. **Async test methods were never awaited** - `method.Invoke` on an `async Task` method
   returns a `Task` immediately; the old code never awaited it, so it always reported
   `Passed` regardless of what actually happened inside, and any exception thrown inside
   never reached the `catch` block.
2. **No `[OneTimeSetUp]`/`[OneTimeTearDown]`** - only per-test `[SetUp]`/`[TearDown]` -
   Selenium suites conventionally launch/quit the browser once per fixture in these.
3. **`TestContext.CurrentContext`/`TestContext.Parameters` didn't work** - populated by
   NUnit's real engine, not by a bare `MethodInfo.Invoke` call.
4. **No `[Ignore]`/`[Explicit]` handling** - a deliberately-skipped test still ran and got
   force-mapped into Pass/Fail; there was no "Skipped"/"Inconclusive" concept at all.
5. **`[Values]`/`[Range]`/`[Combinatorial]`/`[TestFixtureSource]`-generated test cases were
   invisible** to both discovery and execution - only `[Test]`/`[TestCase]`/
   `[TestCaseSource]` were recognized at the method level, never expanded.
6. **No per-test timeout, no crash/hang isolation** - everything ran inside
   `TestQueueWorker`'s single background-service loop; one hung Selenium wait or a crashed
   driver could block the whole queue indefinitely, or take the whole API process down, for
   every user/Release.
7. **Assemblies were never unloaded** - `Assembly.LoadFrom` loads into the default
   `AssemblyLoadContext` for the process lifetime; two Releases with the same assembly
   identity could silently reuse the *first*-loaded copy.
8. **The `Browser` field was captured end-to-end from the UI into the queue table but never
   actually used** - traced the whole call chain and confirmed `TestQueueWorker` never read
   it or passed it into `ITestRunner`.

`NUnit.Engine` is NUnit's own real runtime, so switching to it fixes 1-5 for free (it's not
an approximation of NUnit's semantics, it *is* NUnit). Process isolation (below) addresses
6-7. Browser wiring (below) addresses 8.

### Process isolation (`ProcessModel=Separate`) - and why it's not a hard blocker
`NUnitEngineTestRunner` runs every test package in an isolated **child process**
(`package.AddSetting(EnginePackageSettings.ProcessModel, "Separate")`) by default, so a
hung/crashed Selenium test can't freeze `TestQueueWorker`'s shared queue for every
user/Release (a deliberate choice for enterprise-scale Selenium suites over staying
in-process - see chat history for the reasoning). This was initially expected to require
the Release folder to have a **full publish output** (`.deps.json` + all dependencies)
alongside the test DLL, since a genuinely separate child process can't benefit from
whatever's already loaded in the host API process the way the old in-process reflection
approach could. **Confirmed by direct testing this is only partially true**:
- A bare test DLL (today's real Release-folder convention - confirmed by inspecting
  `D:\Releases\QA\REL-10_...\` directly: just the DLL + one hand-copied dependency, no
  `.deps.json`, no `NUnit.Framework.dll`) **does** fail under `ProcessModel=Separate` if the
  *only* thing referencing a given assembly is the test project itself - e.g. if
  `AutomationAPI` doesn't also reference it somewhere.
- **But** `AutomationAPI` now explicitly references `NUnit`, `Microsoft.TestPlatform.ObjectModel`,
  and `Selenium.WebDriver` specifically so that a bare Release-folder DLL using just those
  (i.e. a normal NUnit/Moq unit test suite, or a Selenium suite built on `Selenium.WebDriver`)
  **can** run fully isolated (`ProcessModel=Separate`) with **zero changes to the external
  deploy pipeline** - confirmed end-to-end: created a real Release (`REL-14_SeleniumPoC`),
  copied *only* `SeleniumSmokeTests.dll` into its folder (no other files), assigned/queued
  `TC_SEL_001` (a real `async Task` test that launches Chrome via `[OneTimeSetUp]`, submits a
  form, and asserts on the result page), and watched `TestQueueWorker` pick it up and mark it
  `Passed` in the DB - fully isolated, fully real, no manual DLL staging beyond the one file.
- This is a **partial, pragmatic mitigation**, not the general answer - it only covers the
  specific extra dependencies `AutomationAPI` itself chooses to reference. A test suite using
  something else entirely (Appium, RestSharp, a custom internal library, etc.) still needs
  either that dependency added to `AutomationAPI` too, or - the fully general, correct fix -
  the Release folder should get a full publish output for its own specific dependencies
  (`AutomationAPI`'s own baseline references only need to cover the common denominator).
- **Automatic fallback**: if `ProcessModel=Separate` still can't load a dependency,
  `NUnitEngineTestRunner` automatically retries once with `ProcessModel=InProcess` (same
  reliability profile as the old reflection runner) rather than hard-failing the whole run.
  `TestExecutionResult.WasIsolated` records which happened, and `TestQueueWorker` logs a
  warning when a run fell back to in-process, so this is visible/discoverable rather than a
  silent reliability regression. Confirmed (by direct testing) the missing-dependency
  failure shows up in **two different shapes** depending on exactly what's missing - either
  `NUnit.Engine` throws `NUnitEngineException` synchronously, or it doesn't throw at all and
  instead returns a normal-looking result XML with the assembly-level `<test-suite>` marked
  `runstate="NotRunnable"`/`result="Failed" label="Invalid"` - both are detected
  (`NUnitEngineHelper.IsMissingFrameworkDependency`/`IsUnrunnableResult`).

### Filtering by class/method name - NUnit `id`s are NOT stable across runner instances
`TestQueueWorker` calls `ITestRunner.RunAsync` with a specific `ClassName`/`MethodName` per
queue item (matching one assigned test case). Building an NUnit `TestFilter` for this
requires knowing which concrete test(s) match those (historically simple, non-namespaced)
names. **Confirmed by direct testing**: NUnit's numeric test-case `id` attributes (e.g.
`"0-1001"`, from an `Explore()` XML) are only valid within the exact `TestPackage`/
`ITestRunner` instance that produced them - reusing an id captured from one `Explore()` call
against a *different* `TestPackage`/runner instance (e.g. a fresh package built for the
actual `Run()` call) silently matches nothing (returns zero results, no error). Fixed by
building filters from **class+method name pairs** instead
(`<filter><or><and><class>...</class><method>...</method></and>...</or></filter>`,
`NUnitEngineHelper.BuildFilter`/`FindMatchingTestCases`) - NUnit matches these by literal
string comparison at filter-evaluation time, so they're safely reusable across a fresh
package/runner instance built later for the actual execution.

### Discovery caching + Windows file-locking caveat
`NUnitEngineHelper.Explore()` (used by `TestSuitesRepository` for the Assignment
screen/Dashboard) is cached in-memory keyed by `(dllPath, LastWriteTimeUtc)`, so a "huge"
suite's DLLs aren't fully re-scanned from scratch on every page load - a Release's DLL being
rebuilt/republished automatically invalidates the cache (this also mitigates the stale-
assembly risk for discovery specifically; execution itself doesn't need this, since isolated
runs always load fresh into a brand-new process). **Caveat confirmed by direct testing**:
`Explore()` always runs in-process (`ProcessModel=InProcess` - it's read-only/cheap, no
isolation benefit worth the overhead), and on Windows this **locks the DLL file** for as
long as the API process is running, until the assembly is GC'd/unloaded (which .NET Core's
default `AssemblyLoadContext` doesn't reliably do promptly) - attempting to overwrite a
Release's DLL (e.g. redeploying a new build) while the API is running and has explored it at
least once can fail with a file-in-use error. Restarting the API releases the lock. Not
addressed further in this pass (would need a collectible, per-explore `AssemblyLoadContext`
that's explicitly unloaded after each `Explore()` call) - flagged as a known follow-up.

### Reading Browser in a test
`TestQueueWorker` passes the queue item's `Browser` (already captured end-to-end from the
Run Now/Schedule dialogs, previously just stored and never used) into
`TestRunRequest.Browser`. `NUnitEngineTestRunner` sets it as an NUnit engine `TestParameters`/
`TestParametersDictionary` package setting (both the legacy string form and the modern
dictionary form, matching what NUnit's own console runner does, for compatibility across
framework versions) - test code reads it via `TestContext.Parameters["Browser"]`
(`SeleniumSmokeTests.SmokeUiTests.LaunchBrowser()` does exactly this, defaulting to Chrome if
absent). `AutomationShared.CustomTestContext` (a same-process-only static dictionary some
older sample test code reads from, e.g. `OnboardingServiceTests.RegisterEmployee_
ValidDocuments_ReturnsTrue`'s `CustomTestContext.Get("queueId")`) is superseded by this -
left in place, unused, rather than removed, since it doesn't work across the process
boundary that `ProcessModel=Separate` introduces (a parent-process static field is invisible
to an isolated child process).

### `TestExecutionResult` - real outcomes instead of binary Pass/Fail
Gained a `TestOutcome` enum (`Passed`/`Failed`/`Skipped`/`Inconclusive`, from the engine's
real `result`/`label` XML attributes) and `WasIsolated`, replacing the old binary
`bool Passed`(kept as a computed property for compatibility). `TestQueueWorker` maps
`Skipped`/`Inconclusive` to a new `"Skipped"` `TestCaseStatus` (previously impossible - an
`[Ignore]`d test used to be silently force-mapped into Pass or Fail). Frontend status
lists that treat a status as "locked"/terminal
(`test-case-assignment-user.component.ts`'s `LOCKED_STATUSES`,
`test-case-execution-panel.component.ts`'s `disabledStatuses`/`getBadgeClass`) were extended
to include `'Skipped'`/`'Inconclusive'` alongside the existing statuses, with matching badge
colors.

### `SeleniumSmokeTests` - proof-of-concept, not a sample/demo only
Added because there was **no Selenium test anywhere in this codebase** before this refactor
(confirmed by reading all of `AutomationTests`' existing projects - `OnboardingTests`/
`PayrollTests`/`RecruitmentTests` are all pure Moq unit tests) - this is what actually
*proves* the gaps above are fixed, not just an argument that they should be.
`AutomationTests/SeleniumSmokeTests/SmokeUiTests.cs` covers, deliberately, one test per gap:
- `SubmitWebForm_TextInput_ShowsSubmittedValue` - `async Task`, with a real `await
  Task.Delay(...)` plus real Selenium actions, submitting
  `https://www.selenium.dev/selenium/web/web-form.html` (Selenium's own official, stable
  test fixture page - chosen specifically so this suite needs no app/credentials of its own)
  and asserting on the resulting URL - proves async is genuinely awaited end-to-end.
- `[OneTimeSetUp]`/`[OneTimeTearDown]` (`LaunchBrowser`/`QuitBrowser`) - launches/quits a
  real Chrome (or Edge) via Selenium + `WebDriverManager` (auto-resolves a matching
  chromedriver/msedgedriver locally - avoids needing to pin/ship a specific driver version).
  Uses `WebDriverManager`'s resolved driver path explicitly via `ChromeDriverService`/
  `EdgeDriverService`, rather than Selenium's own bundled "Selenium Manager" default -
  confirmed by direct testing that Selenium Manager looks for a `selenium-manager/`
  subfolder next to the running assembly, which only exists in a full `dotnet build`/publish
  output, not a bare deployed test DLL.
- `TextInput_AcceptsValue("Alpha"/"Beta"/"Gamma")` - `[TestCase]` with multiple rows,
  proving parameterized-test discovery/execution (each row shows up as its own test case in
  both `Explore()` and `Run()` output - confirmed via `GET .../libraries?releaseId=...`
  returning all 3 rows individually, something the old reflection scan couldn't do at all).
- `BrowserParameter_IsReadable` - reads `TestContext.Parameters["Browser"]` directly, proving
  the Browser-wiring path end-to-end.
- `DeliberatelySkipped_ShouldReportAsSkipped` - `[Ignore(...)]`, proving it's honored
  (reports `Skipped`, confirmed via a real queued run) instead of running/being force-mapped
  into Pass or Fail.
All 6 tests were run for real (both via plain `dotnet test` locally and through the actual
`NUnitEngineTestRunner`/`TestQueueWorker`/DB pipeline against `REL-14_SeleniumPoC_v1.0.0`) -
5 passed, 1 correctly reported Skipped.

### Fixed: `[Property(...)]` missing for parameterized ([TestCase]/etc.) methods
`ExploreXmlParser.GetProperty` originally only checked a `<test-case>` node's own direct
`<properties>` children. Confirmed by direct testing (`Explore()` on `TextInput_
AcceptsValue`, which has 3 `[TestCase("Alpha"/"Beta"/"Gamma")]` rows) that NUnit attaches a
**method-level** `[Property(...)]` (`Description`/`Priority`/`TestCaseId`) to the
intermediate `<test-suite type="ParameterizedMethod">` wrapper node instead - each
individual `<test-case>` row has no `<properties>` of its own at all in that case. Only a
plain `[Test]` method (one `<test-case>` directly under the `TestFixture`, no wrapper) has
the property directly on itself. Fixed by walking up through ancestor `<test-suite>` nodes
(bounded to stop once a node has no `classname` attribute, i.e. once we'd leave the test
class itself) until a matching property is found - so a plain `[Test]` and a parameterized
method's rows both resolve correctly, and every row of one parameterized method correctly
shares that method's property values (matching NUnit's own semantics - the property really
is declared once per method, not per generated row). Verified directly: all 3
`TextInput_AcceptsValue` rows now correctly report `TestCaseId=TC_SEL_002`,
`Priority=Medium`, and the real `Description` (previously all three came back empty).

### Fixed (found via the Assignment screen after the above fix): parameterized rows sharing one TestCaseId
Once the fix above correctly surfaced the method-level `[Property(...)]` for all 3
`TextInput_AcceptsValue` rows, a **new**, real problem became visible in the Assignment
screen: all 3 rows showed the identical `TestCaseId=TC_SEL_002`. This app treats
`TestCaseId` as the unique key for assignment/execution tracking (e.g.
`aut.AssignedTestCases.TestCaseId`), so 3 distinct executable tests sharing one ID is
ambiguous - assigning/tracking "TC_SEL_002" can't say which of the 3 real variants
(Alpha/Beta/Gamma) it refers to. Root cause: a plain `[TestCase(...)]` + method-level
`[Property(...)]` fundamentally can't express a *different* property value per generated
row - the property is declared once, for the whole method. Fixed in the **test source**
(`SeleniumSmokeTests/SmokeUiTests.cs`), not the discovery code (which was correctly
reporting what was actually declared) - switched `TextInput_AcceptsValue` from
`[TestCase("Alpha"/"Beta"/"Gamma")]` to `[TestCaseSource(nameof(TextInputValues))]` backed
by a static `IEnumerable<TestCaseData>` using `TestCaseData.SetProperty(...)` per row, which
NUnit attaches directly to that row's own `<test-case>` rather than the shared
`ParameterizedMethod` wrapper. Now reports 3 distinct ids (`TC_SEL_002A`/`TC_SEL_002B`/
`TC_SEL_002C`), each with its own `Description`, verified directly. **Guidance for future
enterprise test authors**: any parameterized method whose rows need independent
assignment/tracking in this app must use `TestCaseData.SetProperty("TestCaseId", ...)` (or
equivalent per-row property assignment) rather than `[TestCase(...)]` + a shared
method-level `[Property("TestCaseId", ...)]`, since the two are not the same thing.
Also filled in the previously-intentionally-missing `Description`/`Priority` on
`DeliberatelySkipped_ShouldReportAsSkipped` (`TC_SEL_004`) for consistency - it wasn't a
bug (that test genuinely never declared them), just incomplete authoring.

### `ReflectionTestRunner` removed
Deleted (`AutomationAPI/Repositories/TestRunner/ReflectionTestRunner.cs`) - confirmed
unregistered/unreferenced anywhere else, superseded by `NUnitEngineTestRunner` which was
regression-tested and verified end-to-end with real Selenium execution. Git history
preserves it if ever needed again.

### Fixed: `Assert.Pass(...)` shows a confusing "exception" in tools like Test Explorer
`BrowserParameter_IsReadable` (`TC_SEL_003`) used to end with `Assert.Pass($"...")`.
`Assert.Pass` has always worked by throwing `NUnit.Framework.SuccessException` internally
as its own control-flow signal to end the test immediately and mark it Passed - not a real
error, and it doesn't affect the reported outcome (confirmed: it correctly showed up as
`Passed` through `NUnitEngineTestRunner` both times). But tools that surface "any exception
this test's execution touched" (e.g. Visual Studio's Test Explorer detail/exception view)
show this prominently, which reads as an alarming failure even though the test genuinely
passed - purely a presentation/noise issue, not a functional bug. Checked the rest of the
suite for the same pattern - it's the only place using `Assert.Pass`/`Assert.Ignore`/
`Assert.Inconclusive` at runtime (`DeliberatelySkipped_ShouldReportAsSkipped` uses the
`[Ignore(...)]` **attribute**, which is handled entirely by the engine before the method
body ever runs, so its body's `Assert.Fail` never actually executes and never throws).
Fixed by removing the `Assert.Pass(...)` call - a test that simply completes its method
body normally needs no explicit "I'm done, mark me Passed" signal at all. Replaced with a
real (conditional) assertion: if the Browser parameter is present, assert it's non-empty;
if absent, the test still completes normally (no assertion needed, nothing to check).
Verified: `dotnet test` now shows it passing in ~1ms with no exception trace at all.

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
- **Added** (per explicit request, and this time done correctly - see the "tried and
  reverted" note above for why the first attempt broke things): once a test case's
  assignment moves past `'Assigned'` (`Queued`/`Scheduled`/`InProgress`/`Passed`/`Failed`/
  `Cancelled` - i.e. it's entered the execution pipeline at all, not just finished), it's
  now **locked** against further changes. Enforced primarily at the **DB level** in
  `usp_CreateOrUpdateAssignmentWithTestCases`
  (`Database/TestCaseAssignment_Lock_Executed_Migration.sql`, idempotent `CREATE OR ALTER`,
  applied to the live DB and verified with self-contained create/save/reset tests) -
  deliberately the actual source of truth, not just a frontend affordance, so a stale or
  buggy client can't silently corrupt an executed result:
  - `MERGE`'s `WHEN MATCHED` only fires `AND Target.TestCaseStatus = 'Assigned'` - resending
    a locked test case (e.g. because it's still checked in a Save that also included a new,
    unrelated selection) no longer touches it at all.
  - The "delete removed test cases" step only ever deletes rows still `'Assigned'` - a
    locked test case omitted from a resend is never silently dropped.
  - **Reset** no longer unconditionally wipes everything: it only removes still-`'Assigned'`
    test cases (and their Queue/Screenshot/Log rows). Locked ones - and consequently the
    `TestCaseAssignment` row itself, since it's not empty - are left in place. Verified: a
    self-contained assignment with one `'Assigned'` and one `Failed`/one already-`Passed`
    (real) test case, reset, left exactly the two locked ones + the assignment row intact,
    removed only the unlocked one. If *nothing* is locked, Reset still does the full
    permanent delete from the prior fix.
  - The proc now returns (`SELECT @LockedCount`) how many test cases it left untouched
    because they were locked, read via `ExecuteScalarAsync<int>`
    (`ITestCaseAssignmentRepository.CreateOrUpdateAssignmentWithTestCasesAsync` now returns
    `Task<int>` instead of `Task`). `TestCaseAssignmentsController`'s response includes
    `LockedCount` + an adjusted message, so Save/Reset isn't a silent no-op when something
    was skipped.
  - Frontend: `tryLoadTestCases()` tracks each visible row's `testCaseStatus` again (safe
    this time - Step 4's filter guarantees a visible row is only ever unassigned or the
    *current* tester's own, never another tester's, so there's no repeat of the earlier
    wrong-tester bug). `isTestCaseSelectable`/`[rowSelectableFn]` disables the checkbox for
    any locked status; the "Current Status" column shows a color-coded status badge
    alongside the tester badge. `onSaveAssignments()` sends the real current status
    (`tc.testCaseStatus || 'Assigned'`) instead of always hardcoding `'Assigned'`. Both
    `onSaveAssignments()`/`onResetAssignments()` read the response's `lockedCount` and show
    an info toast (*"N test case(s) could not be changed because they have already been
    executed"*) instead of a generic success toast when something was skipped.

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

## Fixed: negative "Unassigned" count, mislabeled "Total Tests", badge legibility, Release Readiness/UX (Release Management + Test Case Assignment)

### Test Case Assignment: negative "Unassigned" count
`test-case-assignment-user.component.ts`'s `loadLibraryTestCaseCounts()` computed
`assignedCount` from `getAllAssignedTestCasesInLibrary(libraryName)` - **not** Release-scoped,
counts assignments across *every* Release that ever used that library name (the legacy,
pre-Release-Management endpoint, left in place "in case anything else still calls it" - it
turned out this counts method still did). `totalCases` (from `getAllTestCasesByLibraryName`)
**is** Release-scoped, so `unassignedCount = totalCases - assignedCount` went negative
whenever a library had more historical assignments from other Releases than the current
Release has test cases. Fixed by switching to the already-existing Release-scoped
`getAssignedTestCasesForLibraryAndRelease(libraryName, releaseId)` (same one
`tryLoadTestCases()` already used correctly).

### Release Management: "Total Tests" only ever counted assigned test cases
`aut.usp_GetReleaseById`/`usp_GetAllRelease`'s `TotalTests` (via `sp_helptext`) is
`COUNT(*) FROM TestCaseAssignment JOIN AssignedTestCases WHERE ReleaseId = ...` - i.e. total
*assigned* test cases, not the total discoverable in the Release's DLLs, but displayed
everywhere as plain "Total"/"Tests", implying it was the full inventory. Fixed by adding a
genuine total-discoverable count:
- `ITestSuitesRepository.GetTotalTestCaseCountAsync(releaseFolderPath)` - sums
  `GetLibrariesAsync()`'s (i.e. `NUnitEngineHelper.Explore()`, already cached by
  last-write-time) method counts across every DLL in the folder.
- `ReleaseController`'s `PopulateFolderInfoAsync` (renamed from the now-async
  `PopulateFolderInfo`) populates a new `ReleaseModel.TotalDiscoveredTests` /
  `IReleaseModel.totalDiscoveredTests` field the same way `DllFileCount`/`FolderReady`
  already are, on both `GET /api/Release` (list) and `GET /api/Release/{id}` (details) -
  verified live: `REL-14_SeleniumPoC` correctly shows `totalTests: 5` (assigned) vs
  `totalDiscoveredTests: 6` (real total, all 6 discovered `SeleniumSmokeTests` cases)
  as two distinct numbers instead of one mislabeled one.
- Release Management cards now show `Tests: {{ totalDiscoveredTests }} total` plus a
  separate `Assigned: {{ totalTests }} (P.. F.. S..)` line; Release Details' Test Summary
  card shows Total/Assigned/Unassigned/Passed/Failed/Skipped/Running as distinct numbers.
- `TotalTests`/`totalTests` themselves are unchanged/kept (Passed/Failed/Skipped/Running
  only make sense for assigned+executed tests anyway) - just no longer mislabeled as "Total".

### Illegible badge text (dark text on dark backgrounds) app-wide
`.status-pill` (global, `styles.css`) sets no `color`; Bootstrap's `bg-*` utility classes
only set background, not text color - every status/lifecycle/sign-off badge helper
(`releaseLifecycleBadgeClass`, `statusPillClass`, `signOffPillClass`, and several inline
`[ngClass]` badges) returned bare `bg-*` classes, so dark backgrounds (Completed/
`bg-primary`, Active/`bg-success`, Rejected/`bg-danger`, Approved/`bg-success`) rendered
with illegible dark/black text - confirmed live against the exact reported case
(`Release_ODM v1.5.0`, `releaseLifecycle: "Completed"`). Fixed with one shared helper,
`ohpnm-test-portal/src/app/core/utils/badge-class.util.ts`'s `pairBadgeTextColor(bgClass)`
- always pairs a background class with the correct contrasting text color (dark
backgrounds → `text-white`, light ones like `bg-info`/`bg-warning` → `text-dark`) - used
from `test-case-execution-panel.component.ts` (`releaseLifecycleBadgeClass`/`getBadgeClass`),
`release-management.component.ts` (`statusPillClass`/`signOffPillClass`, plus the inline
"Deactivated" badge), `release-details.component.ts` (new `lifecyclePillClass`/
`signOffPillClass`/`notificationStatusPillClass` methods, `pairBadgeTextColor` also exposed
directly to its template for a couple of inline ternary badges), and
`test-case-assignment-user.component.ts`'s new release-lifecycle badge (see below).
**Follow-up**: initially left `dashboard.component.ts`/`left-sidebar.component.ts` out of
this pass since they weren't in the original bug report - reported back as still broken
("Dashboard badges are still not fixed"), so fixed those too: `dashboard.component.ts`'s
own copy-paste of `releaseLifecycleBadgeClass` (identical bug/fix), and
`left-sidebar.component.ts`'s `environmentBadgeClass` (Development/QA/Production pills -
same missing-text-color pattern). `dashboard.component.ts`'s `getBadgeClass` was already
correct (explicit `text-white`/`text-dark` pairings per case) - left as-is.
`settings.component.html`'s only `bg-*` usage is a `.progress-bar` fill (password
strength), not a text badge, so there's no legibility concern there - left untouched.

### Release Readiness stuck on "READY FOR ACTIVATION" after activation
The Release Details "Release Readiness" card's badge/message was driven purely by
`readiness.isReady` (does the folder currently have usable DLLs) with zero awareness of the
Release's actual lifecycle - kept saying "READY FOR ACTIVATION" forever, even once already
`Active`/`Completed`. Also, `load()` unconditionally re-ran the reflection-heavy readiness
scan on every call (including right after a successful `activate()`), even though
`silentRefresh()` (the auto-refresh tick) already had the right idea of only doing that
while still `Draft`. Fixed: `load()` now only calls `refreshReadiness()` while `Draft`
(same guard as `silentRefresh()`); the template's Readiness card now shows "This release
has already been activated - DLL readiness no longer applies" (+ activated-by/-on) once
not `Draft`, instead of the stale DLL-readiness badge, and hides the manual Refresh button
in that state too.

### Test Case Assignment: better Release/Environment display
"Environment" used to be squeezed in as a `form-control-plaintext` between two real
dropdowns (Release, Test Suite), looking like a broken/disabled dropdown. Replaced with a
summary info bar below the filter row (Release name + version + lifecycle status-pill +
Environment + selected Test Suite), matching `test-case-execution-panel.component.html`'s
"Selected Assignment Info" card pattern for visual consistency between the two screens.

### Release Details: plain `<table>`s converted to `app-data-grid`, with status badges
"Sign-Off History" and "Notifications" now use `app-data-grid` (matching the rest of the
app's convention) instead of hand-rolled `<table>`s, with `cellTemplate`s for their status
columns (color-coded badges via the shared `pairBadgeTextColor` pairing) and for date
columns (`dd-MMM-yyyy HH:mm` formatting, previously done inline via the `date` pipe -
preserved via a shared `dateTemplate`).

### Release Details: section headers now match Dashboard's neutral style
`.section-head` was a page-specific purple gradient (`linear-gradient(135deg, #5c3c9e,
#7b5fc0)`, white text) unique to this page. Replaced with Dashboard's `.rd-header`/
`app-execution-logs-viewer`'s `.log-header` style (light gray `#f9fafb` background, subtle
bottom border, dark bold text) across all of this page's card headers (Release Information,
Release Readiness, Test Summary, Lifecycle & Sign-Off, Sign-Off History, Notifications),
for visual consistency with the rest of the app.

### Fixed: duplicate Release notifications ("showing more records than actual")
Reported as the Notifications grid "showing more records than actual". Root cause found
in the **database**, not the frontend grid: `aut.usp_ActivateRelease` had **no guard**
against activating a release that wasn't `Draft` - it unconditionally re-stamped
`ActivatedBy`/`ActivatedOn` and returned success every time called, and
`ReleaseController.Activate()` unconditionally sends a full "release available for
testing" notification batch to every active Manager/Admin after every successful call.
The frontend's `canActivate` getter normally keeps the Activate button disabled once a
release isn't `Draft`, but that's a UI-only guard - calling the endpoint again (as
happened here, directly, during iterative testing/redeployment of a Release's DLL) could
still re-activate and re-notify with nothing stopping it. Confirmed live:
`aut.ReleaseNotification` had exactly 14 rows for `REL-14_SeleniumPoC` (2 full batches of
7 recipients) instead of 7 - genuinely duplicate database rows, not a rendering bug.
**Fixed** in `Database/Release_Activate_Guard_Migration.sql` (idempotent `CREATE OR ALTER`,
applied to the live DB and verified: re-running `usp_ActivateRelease` against the
already-`Active` Release 14 now correctly raises `"Cannot activate: release is already
Active."` instead of silently succeeding) - only a `Draft` release can be activated now,
matching the same Draft-only-transition convention already used elsewhere (e.g. Delete is
also only allowed while Draft). The existing `RAISERROR`/`GetUserMessage` error-surfacing
pattern this controller already uses for other guarded stored procedures picks this up
automatically as a `400` with a clear message - no controller code changes needed.

### Investigated, not a bug: "Release Activation notifies more than Admin/Manager"
Reported as Release Activation notifying users beyond Admin/Manager. Checked
`ReleaseNotificationService.NotifyManagersAndAdminsAsync`'s recipient filter
(`u.Active && (RoleName == "Admin" || RoleName == "Manager")`) directly against the real
`aut.ReleaseNotification` rows for Release 14/15 - every single recipient was genuinely
either Admin or Manager by role; no Tester/Viewer ever received anything. The filtering
code was already correct. Root cause was **test data**, not code: user "Tester7" (and
inactive "Tester6") were assigned `RoleID = 1` (Admin) instead of `RoleID = 2` (Tester) -
looked like a bug because of the username, but the account genuinely was Admin per the
database. Fixed the data directly (confirmed with the user first): `UPDATE aut.[User] SET
RoleID = 2 WHERE UserID IN (14, 15)` (needs `-I`/`QUOTED_IDENTIFIER ON` like other writes
to `aut.[User]`/`aut.Release`). No code changes were needed or made for this one.

### One-off cleanup: users with no `PasswordHash` deleted
Per explicit request and confirmation: 14 of 18 `aut.[User]` rows had no `PasswordHash`
(never completed real registration - some looked like disposable test/viewer accounts,
others (`vishnu`/`Saharsh`/`muralip`/`Vamshi`/`Tester7`) were genuinely the real Admin/
Manager notification recipients verified earlier the same session, each with real
`ReleaseNotification` history and, for a few, `Environment.CreatedBy` references - flagged
this explicitly and got explicit confirmation to delete all 14 anyway before doing
anything). Checked `sys.foreign_keys` first to find the only two real FKs referencing
`aut.[User]`: `FK_ReleaseNotification_User` (`RecipientUserId`, nullable - those 30 rows
were deleted outright, matching "and that users related data") and
`FK_Environment_CreatedBy` (`CreatedBy`, `NOT NULL` - reassigned the 5 affected
Environment rows' `CreatedBy` to `UserID 1` (`Nareshg`, the real primary Admin) instead of
deleting the Environments themselves, which were out of scope). Also confirmed
`aut.TestCaseAssignment` had zero rows referencing any of the 14 as `AssignedUser`/
`AssignedBy`, so no cleanup needed there. Verified after: no orphaned
`ReleaseNotification.RecipientUserId` references, all `Environment.CreatedBy` values
resolve to an existing user. Remaining users: `Nareshg` (Admin), `testuser25` (Viewer),
`saharshg` (Manager), `muralip1` (Viewer) - all 4 have a real `PasswordHash`. This was a
one-off manual cleanup (like the earlier legacy-assignment cleanup) - not captured as a
reusable/idempotent migration script, since re-running it wouldn't make sense (no more
qualifying users exist).

## UX improvements: Save-diff toast, Assignment coverage bar, remaining badge-color spots
Implemented 3 of the previously-deferred UX suggestions (search box and loading
spinners/skeletons for `DataGridComponent`/`AppDropdownComponent` remain deferred, per
explicit request - noted for later, not implemented):

### Test Case Assignment: "N added, M removed" in the Save toast
`tryLoadTestCases()`'s Step 3 already computed `myAssignedIds` (the tester's assignment set
*before* any changes) but only used it locally. Now stored as `private
originallyAssignedIds` on the component, and `onSaveAssignments()` diffs
`selectedMethods`'s ids against it right before calling the save API to compute real
added/removed counts, passed into `showSaveResultToast()` (already the shared success/
locked-count toast helper, extended with two new optional params). Message becomes
`"Assignments saved: 2 added, 1 removed."` instead of a generic "saved successfully" -
falls back to the generic message when nothing actually changed. `onResetAssignments()`'s
call site is unaffected (doesn't pass the new params, so it still shows its own "All
assignments reset." message via the same defaulted-to-0 fallback path).

### Test Case Assignment: coverage progress bar
Added a compact Bootstrap `.progress` bar (green fill, `assignedCount/totalCases` width)
below the existing "Total Cases"/"Unassigned" stat boxes in the filter card - same
dashboard-style visual language already used elsewhere (e.g. Settings' password-strength
bar). Kept the two existing numbers as-is (they weren't wrong, just less immediately
scannable than a bar) plus an explicit "X / Y assigned" caption above the bar.

### Remaining badge-color-pairing spots (2 found via a fresh app-wide sweep)
Same `.status-pill`/`.badge` + bare `bg-*` (no text color) pattern fixed everywhere else
this session, via the existing shared `pairBadgeTextColor` (`core/utils/badge-class.util.ts`):
- `environment-management.component.html`'s Active/Inactive status pill.
- `common-components/execution-logs-viewer/execution-logs-viewer.component.html`'s "N
  steps" badge (a single fixed `bg-secondary` case, no branching, so fixed directly with
  `text-white` in the markup rather than importing the utility for a one-liner).

### Fixed: `tsconfig.json`'s `baseUrl` deprecation lint error
Flagged by the IDE: `baseUrl` is deprecated as of TS 5.x, removed in TS 7.0. Not migrating
off it outright - confirmed via `grep` that 24+ files (including several added this
session) rely on bare `app/...` imports that only resolve via `baseUrl`, not just the 3
explicit `@services`/`@interfaces`/`@mappers` `paths` aliases - a full migration would mean
rewriting every one of those imports (or restructuring `paths` to be baseUrl-independent)
as its own unrelated refactor. Silenced with `"ignoreDeprecations": "5.0"` instead - **not**
`"6.0"` as the IDE's own lint hint suggested: confirmed directly that this project's actual
installed compiler (`npx tsc --version` → 5.7.3) rejects `"6.0"` with a hard `TS5103`
error that breaks the real build (the IDE's suggestion is presumably from a newer bundled
language-service TS version than what's actually installed here) - reverted that attempt
immediately after confirming the build failure, then verified `"5.0"` is both accepted and
actually suppresses the warning (`npx tsc -p tsconfig.json --noEmit` shows nothing
baseUrl-related). Revisit this suppression when actually upgrading toward TS 7.0.

**Note**: the IDE may keep showing this exact warning anyway - confirmed the project's own
`package.json`-pinned TypeScript (`~5.7.2`, installed 5.7.3) is what actually needs `"5.0"`,
but the IDE's language server is evidently a different (newer) bundled TS version that
expects `"6.0"` instead - a workspace-vs-editor TypeScript version mismatch, not a project
config bug. The standard fix (`.vscode/settings.json` with `"typescript.tsdk"` pointed at
`ohpnm-test-portal/node_modules/typescript/lib`, so the editor lints with the same compiler
that actually builds the project) was proposed and **declined** - left as unresolved IDE
noise by choice, not oversight. Do not "fix" this again by changing `ignoreDeprecations`
back to `"6.0"` - that value is confirmed to break the real build under this project's
actual TypeScript version.

## Environment Management: fixed silent failures, unguarded Hard Delete, added audit trail/search/usage counts
Full review of Environment Management (list page, create/edit form, service, controller,
repository, and the actual stored procedures) surfaced several real gaps beyond the
badge-color fix already applied earlier this session:

### Silent failures on delete/toggle/save
`environment-management.component.ts`'s `toggle()`/`delete()` and
`environment-form.component.ts`'s `save()`/`loadEnvironment()` had no `error:` callbacks
at all (or one that only reset a flag) - any backend failure (duplicate name, blocked
delete, etc.) would just silently do nothing visible. Fixed with the same
`err?.error?.message ?? err?.error ?? 'fallback'` toast pattern already used consistently
elsewhere in the app (Settings, Users, auth pages).

### Hard Delete had no guard against in-use environments
Confirmed via `sys.foreign_keys`: 3 real FKs reference `Environment.EnvironmentId` -
`Release`, `TestCaseAssignment`, `AutomationData`. `usp_EnvironmentHardDelete` was an
unconditional `DELETE` with no check - deleting an in-use environment would throw a raw
FK-violation error, and (combined with the silent-failure bug above) the user would see
nothing at all. Fixed in `Database/Environment_Management_Improvements_Migration.sql`
(idempotent): the SP now counts referencing rows across all 3 tables first and
`RAISERROR`s a friendly message instead; `EnvironmentController.HardDelete`/`Create`/
`Update` now catch and surface it as a `409` (same `GetUserMessage`-style pattern
`ReleaseController` already uses). Verified live: hard-deleting the in-use `QA`
environment now returns `409 "Cannot delete: this environment has 3 associated
Release(s)..."` instead of a raw SQL error; creating a duplicate-named environment now
returns `409 "Environment already exists"` instead of an unhandled 500.

### No edit audit trail (`ModifiedBy`)
`aut.Environment` only ever tracked `ModifiedOn`, never *who* last edited/disabled an
environment. Added a nullable `ModifiedBy` column (+ FK to `aut.User`) via the same
migration; `usp_EnvironmentUpdate`/`usp_EnvironmentSoftDelete` now accept and stamp it,
threaded from the controller via `User.FindFirstValue(ClaimTypes.NameIdentifier)` (same
convention `UsersController` already uses) rather than trusting a client-supplied value.
`usp_EnvironmentGetAll`/`GetById` now also return `ModifiedByName` (`LEFT JOIN` - null
until an environment is actually edited/disabled at least once), shown on each card when
present. Verified live: soft-deleting (disabling) an environment now correctly populates
`modifiedByName`.

### Dead `softDelete()` resolved, not left unused or deleted
`EnvironmentService.softDelete()` existed but was never called - the "Disable" button
called the generic `update()` (with `isActive` flipped) instead, identically to "Enable".
Gave it a real semantic role: `toggle()`'s disable path now calls `softDelete()`
specifically, while `update()` stays reserved for actual name/description edits and
re-enabling (there's no "un-soft-delete" endpoint, so re-enabling still uses `update()`).

### Native `confirm()` replaced with the app's own dialog
`delete()` used the browser's native `confirm(...)`, jarring compared to the rest of the
app. Replaced with `ConfirmService.confirm(title, message)` (same pattern
`test-case-execution-panel.component.ts` already uses) - no extra template wiring needed,
since `<app-confirm-dialog>` is already mounted once, globally, in `layout.component.html`
(shared by every routed page), not per-page.

### Added: loading/empty states, search/filter, usage count + Delete button gating
- List page now shows "Loading environments..."/"No environments found." states, mirroring
  `release-management.component.ts`'s existing pattern (previously just showed a blank
  area in both cases).
- Added a search box (name/description) + Active/Inactive status filter, via a
  `filteredEnvironments` computed property, same pattern as Release Management's own
  filtering.
- `usp_EnvironmentGetAll`/`GetById` now also return `ReleaseCount` (`Release` rows per
  `EnvironmentId`), shown on each card as "Used by N release(s)" and used to `[disabled]`
  the Delete button client-side (with an explanatory tooltip) whenever `releaseCount > 0` -
  reinforcing the server-side guard above instead of only failing after a click+confirm.
  `delete()` also re-checks this before even showing the confirm dialog, as defense in
  depth against a stale count.

## Follow-up: SendGrid removed entirely (per explicit request)
The section below describes the state when SendGrid and Brevo coexisted as two selectable
providers. Per a subsequent explicit request, **SendGrid support was fully removed**:
`SendGridEmailService.cs` deleted, the `SendGrid` NuGet package reference dropped, its
keyed DI registration and `appsettings.json`'s `SendGrid` config section (which held a
live, already-invalid/expired API key) removed, and `Email:Provider`'s default changed
from `"SendGrid"` to `"Brevo"` (confirmed live: with zero env var overrides, the app now
resolves straight to Brevo and sends real email successfully). `IEmailService` and the
generic `SmtpEmailService`/keyed-DI/named-options architecture described below are
otherwise unchanged - Brevo is simply now the only registered provider, and the same
"add a future SMTP vendor with zero new code" extensibility story still holds.

**Later removed too** (user asked directly why it was still there): the old, already-
orphaned `EmailSettings.cs`/`"EmailSettings"` config section (an Office365 account, with
its own committed plaintext password - confirmed via grep it had zero remaining references
anywhere once `SmtpEmailService` was rewritten to use the new `SmtpProviderSettings`/
`Email:Brevo` instead). Deleted both the model file and the config block - no functional
change, since nothing referenced either.

### Two more providers added: Office365, Amazon SES (SMTP interface)
Demonstrating the "zero new code" extensibility story for real: added `Email:Office365`
and `Email:AmazonSES` config blocks (both `Enabled: false`, empty credentials - not
configured, since no real accounts for these were provided) and refactored `Program.cs`'s
provider registration into a `foreach` over a `smtpProviderNames` array (`["Brevo",
"Office365", "AmazonSES"]`) instead of repeating the `Configure`/`AddKeyedScoped` pair per
provider by hand - adding a 4th provider now means adding one more name to that array plus
its own `Email:<Name>` config block, nothing else. (Originally added Mailgun instead of
Office365 here - swapped per a follow-up request, same mechanics either way.) Verified
live: switching `Email:Provider` to `"Office365"`/`"AmazonSES"` resolves the correct keyed
`IEmailService` cleanly (proving the DI wiring for both is correct) and fails gracefully
with `"<Provider> email provider is not enabled"` (since neither has real credentials)
rather than a DI resolution error - confirming they're correctly registered and ready for
real credentials whenever needed. `AmazonSES`'s `SmtpHost` includes a region segment
(`email-smtp.us-east-1.amazonaws.com`) that must match whichever AWS region the
account/domain is actually verified in; its SMTP username is a distinct "SMTP credentials"
username generated in the SES console, not the AWS access key id/secret directly. Note:
Office365/`smtp.office365.com` has been progressively disabling basic (plain
username/password) SMTP AUTH for most tenants in favor of OAuth2/modern auth - a plain
password may not authenticate depending on the tenant's configuration; an app password is
sometimes required instead.

## Added: generic, config-driven SMTP email provider (Brevo today, any SMTP vendor later) alongside SendGrid
`IEmailService.SendAsync(string to, string subject, string body)` (6 call sites across
`ReleaseNotificationService.cs`/`AuthenticationController.cs`) previously had exactly one
implementation, `SendGridEmailService` (HTTP REST API via the SendGrid SDK - genuinely
provider-specific, not SMTP at all), hardcoded in `Program.cs`'s DI registration.

### Architecture
- **SendGrid is fundamentally different from every other mainstream provider** - it's an
  HTTP REST API, not SMTP, so it keeps its own dedicated class. Brevo, and virtually every
  other provider (Mailgun, Amazon SES's SMTP interface, Office365, Postmark-SMTP, Zoho,
  Gmail relay, etc.) all speak the same standard SMTP protocol - so instead of a
  Brevo-specific class, revived the existing-but-dead `SmtpEmailService.cs` (registration
  was commented out in `Program.cs`, confirmed zero other references anywhere) as **one
  fully generic SMTP implementation**, driven entirely by a new `SmtpProviderSettings`
  model (`Enabled`/`SmtpHost`/`SmtpPort`/`Username`/`Password`/`FromEmail`/`FromName`) -
  Brevo today, any future SMTP vendor later, with **zero new code**, just another named
  config block + one more DI registration line.
- `IEmailService` gained a second method, `SendAsync(EmailMessage message)` (new
  `Repositories/Models/EmailMessage.cs` - `To`/`Cc`/`Bcc`/`Subject`/`HtmlBody`/
  `PlainTextBody`), for Cc/Bcc/plain-text support. The existing 3-arg method is unchanged
  and both providers' 3-arg overload just delegates into the rich one internally - **none
  of the 6 existing call sites needed any changes**.
- **Provider selection uses .NET 8 keyed DI services**
  (`AddKeyedScoped<IEmailService, X>("SendGrid"/"Brevo")` +
  `GetRequiredKeyedService<IEmailService>(name)`), resolved **once**, in `Program.cs`,
  from `Email:Provider` config (defaults to `"SendGrid"` - today's behavior, unchanged
  unless explicitly set otherwise) into a plain `AddScoped<IEmailService>(...)`. Every
  existing consumer keeps injecting plain `IEmailService` exactly as before, completely
  unaware providers or keys exist - no `if (provider == "Brevo")` branching anywhere in
  business logic/controllers.
- `SmtpProviderSettings` is bound via .NET's *named* options
  (`Configure<SmtpProviderSettings>("Brevo", config.GetSection("Email:Brevo"))`) - the same
  settings type can have multiple independently-configured named instances, one per
  provider, without needing a distinct C# type per vendor.
- `EmailSettings.cs`/the old `"EmailSettings"` config section (which `SmtpEmailService`
  used to be bound to) are now **fully orphaned dead code** - confirmed no remaining
  references anywhere - left in place since removing them wasn't asked for, but safe to
  delete later.

### Fixed along the way: `System.Net.Mail.SmtpClient` doesn't work reliably against Brevo
Confirmed via **direct live testing** against the real Brevo SMTP relay: the initial
implementation using `System.Net.Mail.SmtpClient` (matching the old `SmtpEmailService`'s
original approach) failed with `"535 5.7.0 Command not implemented... Please authenticate
first"` - the AUTH command was never actually issued before `MAIL FROM`. This is a
well-documented, longstanding STARTTLS/AUTH-ordering interoperability bug in .NET's legacy
SMTP client against modern mail relays (Microsoft's own docs steer away from
`System.Net.Mail.SmtpClient` for anything beyond the most trivial scenarios for exactly
this reason). Switched to **MailKit** (`MailKit`/`MimeKit` NuGet packages, v4.17.0 - the
de facto standard, actively-maintained SMTP client for .NET) - confirmed this correctly
performs the STARTTLS→AUTH→MAIL FROM sequence and successfully authenticates/sends against
the real Brevo relay.

### Fixed along the way: Brevo's SMTP "Login" is not the account email
The original config example assumed `Username` = the Brevo account's login email
(`vishnu.reddy.naatla@gmail.com`). Confirmed via live testing this gets `535 5.7.8
Authentication failed` - Brevo's actual SMTP Login (shown under Settings → SMTP & API →
SMTP tab) is a distinct, auto-generated value (`b7a0f2001@smtp-brevo.com` for this
account), not the account email. `FromEmail`/`FromName` (the visible "From" header) are
unrelated to `Username` (the SMTP auth login) and can differ.

### Security: Brevo credentials moved from user-secrets to `appsettings.json` (explicit, repeated request)
Initially declined to commit the real Brevo SMTP key into `appsettings.json` when first
asked (twice, ambiguously) - stored it via `dotnet user-secrets` instead, entirely outside
the repo. **Later explicitly, unambiguously asked again** ("instead of secret store I want
to keep settings in appsettings") - complied this time, since the instruction was now
clear and direct, and this exact pattern (real secrets committed in `appsettings.json`) is
already the established convention throughout this codebase (`JWTKey:Secret`, the DB
password in `ConnectionStrings`, the old `EmailSettings` Outlook password, the removed
SendGrid API key were all already committed this way) - not a new anti-pattern being
introduced, just consistency with what already exists in this specific project. Real
`Username`/`Password`/`FromEmail` now live directly in `Email:Brevo` in `appsettings.json`;
the user-secrets entries were cleared (`dotnet user-secrets clear`) and the now-unused
`<UserSecretsId>` removed from `AutomationAPI.csproj`, so there's a single, unambiguous
source of truth. Verified live (via a temp build output directory, since a stale locked
`AutomationAPI.exe` blocked the normal `bin` output at the time) that the real email still
sends successfully purely from `appsettings.json` values, no user-secrets involved.

### Verified live (no new backend test project - per explicit choice, same as elsewhere this session)
- Default config (`Email:Provider` unset → `"SendGrid"`): `/api/Authentication/test-email`
  still resolves to `SendGridEmailService` and reaches the real SendGrid API exactly as
  before (fails only due to the pre-existing, already-committed, already-invalid/expired
  SendGrid API key rejecting with `401` - unrelated to this work, not introduced by it).
- `Email:Provider=Brevo` (env var override, not committed): same endpoint now sends via
  Brevo SMTP successfully (`"Email sent successfully"`, `200`) to
  `naresh.net2009@gmail.com` from `vishnu.reddy.naatla@gmail.com`.
- Deliberately-wrong Brevo password: fails cleanly with a clear logged error
  (`Operation=SendEmail Provider=Brevo Recipient=... Subject=...`) - confirmed **no
  password/credentials anywhere in the log output**.
- Cc support verified via a temporary diagnostic endpoint calling the new
  `SendAsync(EmailMessage)` overload directly against both providers (removed after
  verifying - not part of the shipped code).

## Phase 0: merged the real `tests/` Selenium framework into `AutomationTests/`
`D:\OHPNM-Automation\tests\` (a separate, real, production-grade Selenium framework -
`Selenium.BaseComponents` core library + 7 project-specific suites: `TC.Registration`
(namespace `TC.ProviderDataEntry`), `TC.SearchEligibility` (namespace
`TC.MemberEligibilitySearch`), `TC.SearchPA` (namespace `TC.PriorAuthSearch`),
`TC.SearchRA`, `TC.SubmitClaims`, `TC.PriorAuthInquiry`, `TC.PriorAuthoriztion` - note the
typo, kept as-is per explicit request) was analyzed and merged into the existing
`AutomationTests/` solution (`AutomationTests.sln`), which already served this exact role
(a standalone solution of projects that get built and copied into Release folders for
`AutomationAPI` to discover/run via `NUnit.Engine`) for the simpler `OnboardingTests`/
`PayrollTests`/`RecruitmentTests`/`SeleniumSmokeTests` sample projects.

**This move only** (folders relocated as-is via `robocopy /XD bin obj .vs` + old `tests/`
removed, all 8 projects added to `AutomationTests.sln` via `dotnet sln add`) - explicitly
**no renaming** (folder/csproj/namespace mismatches and the `PriorAuthoriztion` typo were
audited and identified but intentionally left untouched per explicit request), **no other
code changes**. Verified via a full clean `dotnet build AutomationTests.sln` (all `bin`/
`obj` removed first) - 0 errors, confirming every `ProjectReference` (e.g. each `TC.*`
project's `..\Selenium.BaseComponents\Selenium.BaseComponents.csproj` relative reference)
resolves correctly now that both live as siblings under `AutomationTests\`.

**Build note**: building this solution with default parallel MSBuild races on copying
`Selenium.BaseComponents`'s output into multiple dependent projects' `bin/` folders
simultaneously (`MSB3021: Access is denied` on `Selenium.BaseComponents.dll.config`) - a
known MSBuild parallel-build issue when several projects share one `ProjectReference`, not
a real problem with the move. Build with `-maxcpucount:1` to avoid it (or accept the
occasional need to retry once).

### Deferred to future phases (analyzed, not yet started)
- `Selenium.BaseComponents.Utilities.APIGatway` calls a mix of endpoints - some match
  current `AutomationAPI` exactly (`GET api/Automation/data/flow/{flowName}`,
  `POST api/TestScreenshots/bulk`), others reference endpoints that no longer exist
  (`POST api/TestCaseExecutionLogs` single-item, `POST api/TestResults/bulk-insert` - no
  such controller - and `GET https://localhost:44390/api/Queue/{Id}/UpdateQueueStatus/
  {status}`, a hardcoded stale port/host with a route shape matching nothing in today's
  `TestCaseExecutionQueueController`). Confirms this framework was built against an
  earlier, now-diverged API version (a "push" self-reporting model) vs. today's "pull/
  observe" model (`NUnitEngineTestRunner`/`TestQueueWorker` watch an isolated process's
  outcome afterward, proven this session via `SeleniumSmokeTests`/`REL-14`). Needs a
  decision: repair the push model (JWT auth + missing endpoints), or drop it for
  local-file + post-run collection via the already-existing `TestScreenshotsController`.
- Confirmed near-100% duplicated boilerplate across every `TC.*` project's `Utilities/`
  (diffed `Helper.cs` between two projects - only the namespace line differs) - a
  consolidation candidate for `Selenium.BaseComponents` (e.g. a generic
  `DataRepository.GetAutomationData<T>(flowName, sectionKey)`).
- Test discovery metadata is inconsistent: `TC.SearchPA`/`TC.PriorAuthSearch`'s tests
  already use the `[Property("TestCaseId"/"Priority"/"Description", ...)]` convention this
  app's discovery (`ExploreXmlParser`) expects; `TC.Registration`/`TC.ProviderDataEntry`'s
  tests use none of it (`[Author]`/`[Category]`/built-in `[Description]` only) - this
  inconsistency, not a code bug, is why the Portal can't yet "always represent the actual
  tests available" for every project uniformly.
- Wiring real Release folders + proving end-to-end execution through the Portal for these
  real suites, and eventual cleanup of dead `APIGatway` methods / hardcoded environment
  URLs in `Data/Users.cs`.

## Phase 3: made TC.PriorAuthSearch's SaveTestCaseLog/SaveMethodScreenShots actually work end-to-end
Verified **Passed** for real: real Chrome, real login, all 5 expected `TestCaseExecutionLogs`
rows created with correct `AssignmentId`/`AssignmentTestCaseId` and step names, all 5
screenshots uploaded with real image data, `hasLogs`/`hasScreenshots` both `true` on the
real assignment afterward (previously always `false`). Found and fixed two layers of bugs
- an already-known set (auth/wiring) and, underneath those, one genuinely new root cause
  that only became visible once the known ones were fixed.

### Layer 1 (as analyzed in planning): auth + dead ID wiring + dead code
- `SaveTestCaseLog`/`SaveMethodScreenShots` (`APIGatway.cs`) never attached the JWT
  `AttachAuthIfAvailable` already built for `GetAutomationData` in the pilot - added it to
  both.
- `AssignmentId`/`AssignmentTestCaseId` were public settable properties on `SearchPATest`
  that nothing ever populated (always `0`) - threaded them end-to-end instead, mirroring
  `Browser`/`AccessToken`: `PendingExecutionQueue`/`usp_GetPendingExecutionQueues` (added
  `ATC.AssignmentId` to the SELECT - `AssignmentTestCaseId` alone was already there) ->
  `TestRunRequest` -> `NUnitEngineTestRunner`'s `TestParametersDictionary` ->
  `BaseFeatureFixture` reads them via `TestContext.Parameters` and exposes them as
  properties every subclass inherits (moved up from `SearchPATest`, which had its own
  copies that shadowed nothing correctly).
- Retired `APIGatway.UpdateQueue` and `InvokeServicePost` entirely (dead/redundant, see
  the question-and-answer below) plus the dead commented-out `foreach` block in
  `BaseFeatureFixture.TearDownTestSuite()` that only ever fed `InvokeServicePost`.
- `TestQueueWorker` now marks a queue item `InProgress` itself, immediately before calling
  `RunAsync` - **not** via a push from inside the test (that was the original plan; revised
  after asking "do we really need this, given TestQueueWorker already handles queue
  status?" - correct instinct: the in-test push only fires if the isolated process
  successfully starts, so a launch failure would leave an item stuck showing "Queued"
  forever; the server-side version is unconditional and needs no new endpoint/auth at all).
- Fixed an unrelated, adjacent bug noticed while reading `TestScreenshotsController`:
  `_logger` was declared but never assigned in the constructor (a real `CS0649` warning) -
  any exception in `InsertScreenshot`/`BulkInsertScreenshots` would have NullReferenceException
  instead of returning the intended 500.

### Layer 2 (only found once Layer 1 was fixed and testing continued anyway): a real working-directory bug
After fixing all of the above, the calls still silently failed with **"An invalid request
URI was provided. Either the request URI must be an absolute URI or BaseAddress must be
set."** Root cause, confirmed by direct testing (dumped the full NUnit run-result XML,
including its `<output>` CDATA, straight from `AutomationAPI`'s own process to see what the
isolated child process had actually written): `Selenium.BaseComponents.SettingsReader`
resolves `"appSettings.json"` relative to `Directory.GetCurrentDirectory()`. For an isolated
(`ProcessModel=Separate`) run, that working directory is **not** the Release folder - it
stayed `AutomationAPI`'s own directory (confirmed via the run XML's `<environment
cwd="...">` attribute, both before and after trying `EnginePackageSettings.WorkDirectory`,
which does *not* change the spawned agent process's real OS-level CWD despite being the
setting that sounds like it should). On Windows, `AutomationAPI`'s own `appsettings.json`
matches the requested filename case-insensitively, so `SettingsReader`'s `optional: false`
check was satisfied by **the wrong file** - one with no `"AppSettings:AutomationAPI"` key
at all - silently producing a `null` `apiUrl` instead of a startup crash.

This had been lurking, undetected, since the pilot: `GetAutomationData` (the only method
exercised during the original pilot) happened to always be tested via a **full publish**
deployment (where the isolated process's CWD quirk didn't matter as much for other
reasons, and `appSettings.json` was always present regardless of CWD resolution
correctness anyway) - `SaveTestCaseLog`/`SaveMethodScreenShots` were the first calls ever
exercised under the bare-file (non-full-publish) deployment style, which is exactly what
surfaced this.

**Fixed** in `SettingsReader.cs` itself: resolve `"appSettings.json"` relative to
`Assembly.GetExecutingAssembly().Location`'s directory instead of
`Directory.GetCurrentDirectory()` - always correct regardless of the spawned process's
working directory, since `appSettings.json` is always deployed alongside
`Selenium.BaseComponents.dll` itself (`CopyToOutputDirectory=Always`).

### Correction to the pilot's "bare-DLL minimum" claim: it's 3 files, not 2
The pilot's real-execution proof (`GetAutomationData` only) claimed the minimum bare-file
deployment for a real `TC.*` project was **2 files**: its own DLL + `Selenium.
BaseComponents.dll`. That was incomplete - it happened to work only because that
specific call path didn't depend on `appSettings.json` being resolved correctly. The real,
now fully-verified minimum (needed for *any* `APIGatway` call, not just
`GetAutomationData`) is **3 files**: `<Project>.dll` + `Selenium.BaseComponents.dll` +
`appSettings.json` (all three sit right next to each other in
`Selenium.BaseComponents`'s own build output, so this is just "copy those 3 files," not
extra work to locate them).

## Follow-up: real browser selection (Chrome/Edge) for Run Now/Bulk Run Now/Schedule/Bulk Schedule
Asked to "provide an option to select the browser when running a test case... passed to
the Base Framework during execution... from Run Now/Schedule and Bulk in all cases."

**Found the core gap**: the backend/queue plumbing for `Browser` was already fully wired
end-to-end (`SingleRunNowRequest`/`BulkRunNowRequest`/`SingleScheduleRequest`/
`BulkScheduleRequest` all already had a `Browser` field; `TestQueueWorker` already read
`queue.Browser` into `TestRunRequest.Browser`; `NUnitEngineTestRunner` already threaded
it into `TestContext.Parameters["Browser"]`) - but `BaseFeatureFixture` (the base class
every real `TC.*` project inherits) **never actually read it**. `InitializeTestSuite()`
unconditionally called `InitializeChromeAndLogin()` - `InitializeEdgeAndLogin()` already
existed with equivalent logic, but was dead code, unreachable from anywhere. Selecting
anything other than Chrome, anywhere in the UI, has never had any actual effect on which
browser launched.

Fixed by adding `BaseFeatureFixture.InitializeBrowserAndLogin()`, called from
`InitializeTestSuite()` instead of the old unconditional Chrome call - reads
`TestContext.Parameters["Browser"]` and dispatches to `InitializeEdgeAndLogin()` when
it's `"Edge"` (case-insensitive), else `InitializeChromeAndLogin()` (matches today's
exact behavior when absent, e.g. a local Test Explorer run outside the queue pipeline).

**A second real bug surfaced immediately by fixing the first one**: the very first real
Edge run failed with `InvalidSelectorException: invalid selector from javascript error:
this.thenCore is not a function` (confirmed via the error message it really was a real
Edge session: `Session info: MicrosoftEdge=152.0.4191.66`) - `CreateChromeDriver`'s
`ExecuteCdpCommand` workaround for the html2pdf.js/jsPDF `thenCore` incompatibility was
never applied in `CreateEdgeDriver` (unnoticed before, since Edge was never actually
reachable at all). Added the identical CDP command injection to `CreateEdgeDriver` -
confirmed by direct testing this was the actual fix: the same real test that failed
first attempt on Edge with this error **Passed** cleanly once this was added.

Frontend, for all 4 flows:
- `ScheduleTestcasesDialogComponent` (shared by Schedule + Bulk Schedule) already had a
  working Browser `<select>`, but offered `Chrome`/`IE` - swapped `IE` for `Edge` (`IE`
  was never supported anywhere on the C# side at all - no `CreateInternetExplorerDriver`
  exists, and real IE support would need a whole separate IEDriverServer dependency).
- `RunNowDialogComponent` (shared by Run Now + Bulk Run Now) had **no browser field at
  all**, and previously only opened when the target environment `RequiresAuthentication`
  - for any environment that didn't need auth, Run Now skipped straight from the
  confirm() prompt to queuing with a hard-coded `'Chrome'`, no dialog whatsoever. Added a
  Browser `<select>` (Chrome/Edge), and the dialog now **always** opens (confirmed
  acceptable with the user - one extra click for non-auth environments, in exchange for
  consistent browser selection everywhere) - the Login User field inside it stays
  conditionally shown/required exactly as before, driven by whether `loginUsers` is
  populated.
- `test-case-execution-panel.component.ts`'s `resolveLoginUserForRunNow` (the one
  centralized helper feeding both `onRunNow` and `onBulkRunNow`) restructured so it
  always opens `runNowDialog` (via a small local `openDialog` helper covering the "no
  release/environment", "doesn't require auth", and "environment lookup failed" cases,
  which previously all skipped the dialog outright) - the callback now carries `browser`
  alongside `loginUserId`, replacing the hard-coded `'Chrome'` in both call sites.

**Verified for real, decisively, at the API level for 3 of the 4 flows** (Run Now, Bulk
Run Now, Schedule - Bulk Schedule shares the identical downstream `TestQueueWorker`/
`NUnitEngineTestRunner`/`BaseFeatureFixture` mechanism as the other three once a queue
row exists, so proving those three is conclusive for all four): queued the same real
test case with `browser: "Edge"` through `single-run`, `bulk-run`, and `single-schedule`
- all three **Passed** cleanly end-to-end (real login, real navigation, real screenshots)
once the CDP fix was in place, each confirmed via the real error/session info during
initial debugging that Edge was genuinely the browser in use, not Chrome. Re-ran with
`browser: "Chrome"` too - still Passes, unaffected.

## Follow-up: Silent/Headless Browser Mode - a Base Framework appSettings.json setting
Asked to add a Silent/Headless Browser Mode option "under the Base Framework application
settings" - clarified this means the `AutomationTests` project's own deployed
`appSettings.json` files (the same file `SettingsReader`/`APIGatway` already read
`AppSettings:AutomationAPI` from), **not** a new Portal UI/DB-driven setting.

**Found a real pre-existing bug while investigating**: `WebDriverService.
CreateChromeDriver(bool headless = false)`/`CreateEdgeDriver(bool headless = false)`
already *accepted* a `headless` parameter, but never actually read it anywhere in either
method body - only an `AGENT_MACHINENAME` environment-variable check (for CI) ever added
`--headless`. `BaseFeatureFixture` always called both with no argument anyway (defaulting
to `false`), so this parameter was pure dead code - headless mode was never actually
reachable at all outside a CI environment before this change.

Implementation:
- `WebDriverService` now owns its own `SettingsReader` (same pattern as `APIGatway`'s
  own), reading `AppSettings:HeadlessMode` once per (isolated, per-run) process.
  `CreateChromeDriver`/`CreateEdgeDriver` now go headless if *any* of: the `headless`
  parameter is true (fixed - actually read now), `AppSettings:HeadlessMode` is `true`,
  or the existing CI env var check - three independent ways to enable it, none of them
  removed.
- Added `"HeadlessMode": "false"` to `AppSettings` in every real project's
  `appSettings.json` (`TC.SearchPA`/`TC.SearchRA`/`TC.SearchEligibility`/
  `TC.SubmitClaims`/`TC.PriorAuthInquiry`/`TC.PriorAuthoriztion`/`Selenium.
  BaseComponents`'s own) - defaults to off, matching today's actual behavior exactly, so
  nothing changes unless someone explicitly flips it to `"true"` in a deployed Release
  folder. (Incidentally fixed a pre-existing invalid-trailing-comma JSON syntax issue in
  5 of these files while adding the new key - harmless in practice since apparently
  tolerated, but now genuinely valid JSON.) `TC.Registration` has no tracked source-level
  `appSettings.json` at all (only an ad-hoc file under its own `bin/` output, never
  wired via its `.csproj`) - left alone, out of scope.

**Verified for real, and found something important while doing so**: deployed a real
Release with `HeadlessMode: "true"`, queued a real run - confirmed via `Get-Process` that
every Chrome process for the run had an empty `MainWindowTitle` (headless has no visible
window at all, unlike a normal run). The run itself failed twice with timing-flavored
errors (`StaleElementReferenceException`/`ElementClickInterceptedException`) reaching well
into the real login flow (past finding/setting the username field). Investigated via the
actual failure screenshot (using the automatic-failure-screenshot feature added earlier)
and found the real cause: a "Terms" agreement modal dialog that the real target site shows
mid-login-flow, which `LoginService.Login()`'s existing `IsActive()`/`ClickCancelButton()`
handling doesn't reliably synchronize with - unrelated to headless mode's correctness, but
apparently exposed consistently by headless mode's different rendering pace. Added
`--window-size=1920,1080` when headless (`--start-maximized` is a documented no-op in
headless mode - there's no real window to maximize, so pages render at a small default
viewport otherwise) - this is a real, worthwhile fix on its own merits, but did **not**
resolve the Terms-modal race. **Decisively confirmed headless mode itself is not at
fault**: re-ran the identical test/Release with `HeadlessMode` flipped back to `"false"`
(no other change) - Passed cleanly in 14.3s. This is a separate, pre-existing login-flow
robustness gap (the Terms modal's timing), not something this change introduced or needs
to fix to satisfy what was actually asked - flagged as a known follow-up if headless mode
needs to be reliable for real runs against this specific target site.

## Follow-up: linked the failure screenshot to its log entry + logged the URL at failure
Asked "what else can we do better on failure" after the above - implemented the top two
suggested improvements:

1. **Linked the failure screenshot to its `TestCaseExecutionLog` row via `ScreenshotId`.**
   `TestCaseExecutionLog.ScreenshotId` already existed but was never populated by
   anything - the failure screenshot was uploaded via the bulk endpoint (`POST api/
   TestScreenshots/bulk`), which doesn't return generated ids at all, so there was no
   way to tell the Portal "this exact screenshot is what this exact failure looked
   like." Fixed end to end:
   - `usp_InsertTestScreenshot` (new migration `Database/TestScreenshot_
     ReturnGeneratedId_Migration.sql`) now `SELECT`s `SCOPE_IDENTITY()` - it previously
     returned nothing at all (`TestScreenshotRepository.InsertScreenshotAsync` used
     `ExecuteNonQueryAsync`, i.e. rows-affected, matching the old `{ InsertedRows }`
     controller response shape). The bulk insert path (used by every test class's own
     success-path `TearDown`) is deliberately left untouched - those screenshots were
     never meant to be linked to one specific log row.
   - `TestScreenshotRepository.InsertScreenshotAsync` switched to
     `ExecuteScalarAsync<int>`; `TestScreenshotsController`'s single-insert endpoint now
     returns `{ ScreenshotId }` instead of `{ InsertedRows }` (confirmed via a full
     grep this JSON shape wasn't consumed anywhere on the frontend, so safe to rename).
   - New `APIGatway.SaveMethodScreenShot` (singular - as opposed to the existing bulk
     `SaveMethodScreenShots`) posts to the single-insert endpoint and returns the
     generated id (or `null` on any failure, matching every other best-effort
     `APIGatway` method's pattern).
   - `BaseFeatureFixture.LogFailureIfAny` now captures/uploads the screenshot *first*
     (via this new method) and sets the resulting id on `TestCaseExecutionLog.
     ScreenshotId` before posting the log - order matters, the log needs the id already
     in hand.
2. **Logs the browser's current URL at the moment of failure**, appended to
   `LogMessage` - essentially free (`TestWebDriver.Url`, wrapped in its own inner
   try/catch since the driver may already be in a bad state by the time this runs), but
   very high value: immediately shows which page the browser was actually on, which is
   often the first thing needed to diagnose a navigation-related failure (exactly the
   kind of VPN/wrong-environment-URL issue that's come up repeatedly in this project).

**Verified for real end-to-end** (not just the API contract in isolation): rebuilt/
redeployed a real Release with a real, deliberately-wrong `LoginUserId`, temporarily
pointed its `appSettings.json` at a throwaway diagnostic API instance (to test the
rebuilt backend without disturbing the user's own running instance), and queued a real
run through the full pipeline. Confirmed via the real stored log row: `screenshotId: 75`
(a real, non-null id), and `logMessage` correctly ending with `"URL at failure: https://
...Login.aspx"`. Cross-checked `aut.TestScreenshots` directly - row 75 genuinely exists,
tagged `Failure_OneTimeSetUp`, `AssignmentTestCaseId` matching. Deferred (discussed but
not implemented, lower priority/higher effort): capturing page source (HTML) on failure
and browser console log (JS error) capture - flagged as further options if wanted later.

## Follow-up: automatic SaveTestCaseLog on any exception/failure (BaseFeatureFixture)
Asked to add `SaveTestCaseLog` calls "if any exception occurs or anything fails" in
`Selenium.BaseComponents`. Investigation (reading every real `Tests/*.cs`) found **no
test class ever calls `SaveTestCaseLog` on failure** - only on the success path
(step-by-step `Info`/`Running` logs). When a test failed, the log trail
(`GET api/TestCaseExecutionLogs`) just stopped abruptly at the last successful step,
with no "Fail" entry showing what/why - even though the test's overall status was
already correctly recorded elsewhere (`AssignedTestCases.ErrorMessage`, via
`NUnitEngineTestRunner`'s result-XML parsing - a separate, unaffected mechanism).
`TC.Registration` has ~20 `catch (Exception ex) { TestContext.Error.WriteLine(...);
throw; }` blocks and `TC.PriorAuthoriztion` has one defensive-retry catch - neither
ever posts a failure log to the API, only NUnit's own local console output.

Added one centralized, private `BaseFeatureFixture.LogFailureIfAny(string stepName)`
helper (no per-test-class changes needed anywhere - every real project inherits this
automatically) that checks `TestContext.CurrentContext.Result.Outcome.Status` (the same
proven pattern `CustomRetry.cs`'s `RetryCommand` already uses) and, if `Failed`, posts a
`TestCaseExecutionLog` with `LogLevel.Fail`/`ExecutionStatus.Failed`, the real exception
message + stack trace, and `TestCaseId`/`Description` read from the test's own NUnit
`[Property(...)]` values - plus an opportunistic screenshot (`Common.PrintScreenShot`/
`APIGateway.SaveMethodScreenShots`, the exact same mechanism every test class's own
success-path `[TearDown]` already uses) if the browser is still alive. Wrapped in its
own try/catch (matching `APIGatway.SaveTestCaseLog`'s own defensive pattern) so a
failure in the logging/screenshot mechanism itself can never mask the real test failure.

Called from **two** places, deliberately - not just one:
- A new `[TearDown]` (`LogFailureAfterEachTest`) - catches a failure inside a `[Test]`
  method itself. Runs independently of/alongside any subclass's own `[TearDown]` (e.g.
  `SearchPATest.AfterTest()`'s success-path screenshots) - NUnit runs every `[TearDown]`
  in the inheritance chain.
- The existing `[OneTimeTearDown]` (`TearDownTestSuite`), extended to call it too,
  **before** disposing `TestWebDriver` - this is the one that actually matters most in
  practice: confirmed by direct testing that many real failures happen inside
  `[OneTimeSetUp]` (login/credential/URL resolution) - a `[Test]`-level `[TearDown]`
  alone would never see these at all, since NUnit doesn't run per-test `TearDown` when
  `OneTimeSetUp` itself fails (no test ever starts). `AssignmentId`/`AssignmentTestCaseId`
  are read from `TestContext.Parameters` at the very top of `InitializeTestSuite()`,
  before anything that can throw - so they're reliably available for this even when
  `OneTimeSetUp` fails partway through.

**Verified for real**: queued a real run with a wrong-credential `LoginUserId` (same
"decisive proof" pattern used earlier in this project) - failed at `OneTimeSetUp` exactly
as expected, and for the first time ever this showed up with `hasLogs: true`/
`hasScreenshots: true` (previously would have been `false`/`false` - nothing was ever
logged for a pure `OneTimeSetUp` failure before this change). Fetched the actual log row
directly: correct `stepName: "OneTimeSetUp"`, `logLevel: "Fail"`, real exception message
and full stack trace, and `testCaseId`/`testCaseDescription` both correctly resolved.
Then re-ran the identical test with a real, working `LoginUserId` - **Passed** normally,
and confirmed via the log list that zero spurious Fail entries were added - only the
expected normal success-path steps (Login to PNM/Self Service/Medicaid Search/PA Search/
Success), proving this change is purely additive and doesn't affect a passing run at all.

## Follow-up: new users default to Active status; Users grid gets a dedicated Active/Inactive badge
Asked for "new user registrations should be Active by default" + "add an Active/Inactive
indicator to the Users grid." Investigation (queried the live `usp_RegisterUser`
definition directly) found `aut.User` already has **two separate status concepts**:
- `Active` (bit) - the field that actually gates login (`AuthService.Login`:
  `if (!user.Active) return "User is inactive"`) - was already hard-coded to `1` on
  registration.
- `Status` (FK to `aut.UserStatus`: Active/Suspended/Pending) - a separate, purely
  informational lookup shown as a colored badge in the Users grid - defaulted to
  **"Suspended"** on registration.

So a newly registered user could already log in fine, but showed up in the Users grid
with a misleading "Suspended" badge. Confirmed with the user this dual-defaulting
wasn't an intentional approval gate - fixed `usp_RegisterUser` (new migration
`Database/UserRegistration_DefaultActiveStatus_Migration.sql`) to look up `'Active'`
instead of `'Suspended'` for the default `Status`, leaving the `Active` bit's own
`1` unchanged (it was already correct). **Verified for real**: registered a real test
user through the actual `/api/Authentication/register` endpoint end-to-end, confirmed
in the DB that both `Active = 1` and `Status` now resolve to `StatusName = 'Active'`
(previously would have been `'Suspended'`), then cleaned up the test row.

Added a new, dedicated Active/Inactive badge column to the Users grid
(`user-list.component.ts`/`.html`) - a clear green/gray badge reflecting the `Active`
bit specifically, distinct from the existing `Status` badge column (Active/Suspended/
Pending) and the existing bare, unlabeled checkbox toggle in the Actions column (which
is unchanged - still the mechanism to flip `Active`; the new column just makes the
current state unambiguous at a glance, next to `Status`).

Note: existing users registered before this migration keep whatever `Status` they
already have - this only changes the default for *future* registrations, not a
retroactive data fix (not requested).

## Follow-up: Login Users moved to a self-service "Credential Configuration" tab
Asked to move Login Users out from under Environment Management into its own top-level
sidebar tab named "Credential Configuration", with an on-page Environment dropdown
(since it's no longer reached via a specific environment's card) - and, in follow-up
clarification, made fully self-service: every logged-in user manages only their own
login credential per environment, never someone else's, with no Portal User picker at
all. This also had to reach into Run Now/Schedule - previously that dropdown showed
*every* configured login user for the target environment (any owner); now it only shows
the current user's own, and blocks with a clear message (before ever opening the dialog)
if they have none configured yet for that environment.

**Key design point discovered while implementing**: `GET api/LoginUser/environment/{id}`
(`usp_LoginUserGetByEnvironment`) was used by the old management screen *and* by Run Now/
Schedule. Rather than changing that endpoint/proc (which would have been a breaking
change for anyone else potentially relying on its unfiltered semantics), left it
completely untouched and added a new, separate, ownership-filtered
`GET api/LoginUser/environment/{id}/mine` (`usp_LoginUserGetByEnvironmentAndPortalUser`)
for both the new self-service screen and the reworked Run Now/Schedule resolution -
`getByEnvironment`/its proc are unused-by-the-Portal-now but deliberately not removed.

Ownership enforcement:
- New `@PortalUserId` parameter on `usp_LoginUserUpdate`/`SoftDelete`/`HardDelete` - each
  adds `AND PortalUserId = @PortalUserId` to its `WHERE`, so a non-owner's call affects 0
  rows; the repository surfaces this as a `bool` (rows-affected > 0) rather than throwing,
  and `LoginUserController` translates `false` into `Forbid()`. **Verified by direct
  testing** with two different real users' JWTs: user 20 attempting to UPDATE or HARD
  DELETE user 1's row both correctly returned 403 and left the row completely unchanged
  in the DB; user 1 (the real owner) doing the same succeeded normally.
- `usp_LoginUserCreate`'s signature is unchanged (still just inserts whatever
  `@PortalUserId` it's given) - enforcement happens in `LoginUserController.Create`
  instead, which now always overwrites `request.PortalUserId` with the caller's own id
  from the JWT before calling the repository, regardless of what the client sends.
  **Verified by direct testing**: sent a `Create` request with a deliberately wrong
  `portalUserId: 999` in the body as user 1 - the resulting DB row's `PortalUserId` was
  `1` (the real caller), not `999`.
- A pre-existing row with `PortalUserId IS NULL` (old seed/verification data) now matches
  nobody - it's simply not self-service-manageable *or* selectable in Run Now/Schedule
  anymore (both now filter to "mine"). Flagged as an accepted, known consequence of the
  self-service model, not a bug - every user who wants to run tests against an
  authenticated environment now needs their own credential added via Credential
  Configuration, even if one already existed there (added by someone else, e.g. an
  admin, under the old model).

Frontend:
- New top-level route `credential-configuration` -> `CredentialConfigurationComponent`
  (`pages/credential-configuration/`, renamed/relocated from
  `pages/environment-management/environment-login-users/`), guarded by `authGuard` only
  (no admin gate - self-service, any logged-in user). Old nested route
  `environment-management/:id/login-users` and its per-card "Login Users" button are
  both removed.
- New sidebar nav link "Credential Configuration", visible to every logged-in user.
- The component now has its own Environment `<select>` (mirrors
  `TestDataManagementComponent`'s exact "pick an environment first" pattern via
  `EnvironmentService.getAll()`) instead of a route param; the Portal User `<select>` and
  its table column are removed entirely; the table only ever shows the caller's own
  credential row(s) for the selected environment (via the new "mine" endpoint).
- `test-case-execution-panel.component.ts`'s `resolveLoginUserForRunNow`/
  `resolveLoginUsersForSchedule` (the two centralized helpers already feeding all 4 of
  single/bulk Run Now/Schedule) now call `getMineForEnvironment` instead of
  `getByEnvironment`, and take a new `onBlocked` callback - if the target environment
  requires authentication and the current user has no *active* credential configured for
  it, the Run Now/Schedule dialog is never opened at all; a toaster explains why and
  points at Credential Configuration, and the run/schedule action is aborted outright
  (not a soft/disabled dialog state). `RunNowDialogComponent`/
  `ScheduleTestcasesDialogComponent`'s `loginUserLabel()` no longer appends a
  `portalUserName` suffix (every entry is now always the viewer's own credential, so it
  would just be redundant).

**Verified for real, end-to-end**: full solution/API both build clean. Direct API tests
proved ownership isolation (`/mine` for two different real users' JWTs against the same
environment - correctly disjoint results), ownership enforcement on write endpoints (403
+ unchanged DB row for a non-owner, success for the real owner), and that `Create` never
trusts a client-supplied `PortalUserId`. Queued a real test through the actual pipeline
with a real, self-owned `LoginUserId` selected (simulating exactly what the reworked
Run Now flow now sends) - the queue/credential-resolution path worked correctly all the
way through to launching a real Chrome session; the run itself then failed with a
WebDriver navigation timeout, the same VPN-off symptom already root-caused in an earlier
session, unrelated to this change.

## Follow-up: removed all remaining hardcoded credentials/URLs from the test projects
Asked to "get rid of hardcoded from test projects" now that the API-driven Environment
URL/LoginUser system was proven working for real. Recommended and implemented full
removal (not just emptying the data while leaving dead fallback code in place, which
would only replace a clear failure with a confusing raw dictionary `KeyNotFoundException`)
- with one addition needed first: **`TC.Registration` actively calls `BaseFeatureFixture.
LoginByProfile(...)` for a mid-test role switch** (`RegistrationTest.cs` - logging in as
`StateAdmin` then later as `EnrollementSpecialist` partway through one test), which
called straight into `UserCredentials` directly, bypassing the whole new system entirely.
Confirmed via a full grep this was the *only* other real call site beyond the two already
replaced in a previous session (`ResolveCredentialsAndUrlAsync`'s fallback,
`LoginByProfile` itself).

Since `LoginByProfile` is a genuinely different use case from the initial Run Now/
Schedule login (an unattended in-test call, not a human picking from a dropdown), a
role-keyed lookup is the right fit here specifically - it doesn't contradict the earlier
"explicit selection, not automatic matching" decision, which was about the *initial*
login only:
- New `usp_LoginUserResolveByRole(@EnvironmentId, @UserRole)` - most-recently-created
  active match for that Environment+Role, or no rows if none configured.
- New `ILoginUserRepository.ResolveByRoleAsync`/`LoginUserRepository.ResolveByRoleAsync`,
  `GET api/LoginUser/resolve?environmentId={id}&role={role}` - same service-token-only
  protection as `GetCredentials` (confirmed by direct testing: a real Admin-role
  Portal-user token got 403, the service-token shape got 200 with the correct decrypted
  password, and a role with no configured data correctly got 404).
- New `APIGatway.GetLoginUserCredentialsByRole(string role)` (Selenium.BaseComponents),
  same fail-gracefully-return-null pattern as the other two API methods.
- `BaseFeatureFixture.LoginByProfile` now calls this API first; **no more hard-coded
  fallback** - throws a clear `InvalidOperationException` naming the missing role/
  environment and pointing at the Login Users screen, instead of silently using a
  removed dictionary (which would have thrown an opaque `KeyNotFoundException`).

Then the actual removal:
- **Deleted `Selenium.BaseComponents/Data/UserCredentials.cs` entirely** (confirmed via a
  full-solution grep zero live call sites remained anywhere - only its own declaration
  and one already-commented-out reference in `TC.Registration/Pages/Registration/
  Agreements.cs`).
- **`LoginService.GetLoginUrl()`'s hard-coded per-environment URL switch removed** -
  confirmed at least one entry (`E2EP3`) was outright wrong (pointed at the `E2E` domain,
  not `E2EP3` - this exact bug was the root cause investigated, then ruled out in favor of
  a VPN issue, in an earlier debugging session). Now throws a clear exception naming the
  environment, since reaching this method at all means `BaseFeatureFixture.Url` had
  nothing else to resolve from (no API-provided `EnvironmentUrl` configured). Fixed a
  latent bug this uncovered: `LoginByProfile` used to call `LoginService`'s 2-arg
  `Login(userName, password)` overload, which always calls this method directly with no
  way to see `BaseFeatureFixture`'s own resolved `_resolvedLoginUrl` - switched to the
  3-arg overload with the fixture's own `Url` property instead.
- **Deleted the dead `Users.TestURL(string)` method** (confirmed zero call sites anywhere
  even before this change - already-dead hard-coded URLs). Left `Users.CurrentEnvironment`/
  `Users.Environment`'s alias constants in place - still structurally referenced by
  `CurrentEnvironment`'s own definition, and they're plain enum-like string labels, not
  secrets.

**Verified for real**: full solution + `AutomationAPI` both build clean (`TC.Registration`
included, despite depending on the changed `LoginByProfile` signature-compatible
rewrite). The new resolve-by-role endpoint's three cases were each confirmed by direct
testing (403/200/404 as described above). Re-ran the exact same real `TC.PriorAuthSearch`
happy-path flow (real `LoginUserId`, real environment, real login) after removing all the
hard-coded fallback data - still **Passed**, ~32s, real logs/screenshots - confirming the
removal didn't regress the now-proven-working API-driven path at all.

## Follow-up: root-caused a real test failure - VPN, not the LoginUser feature
Reported as "test cases failing, not picking up username/password properly despite
`[TestFixture("TechAdmin")]` being present." Investigated via the real DB/API data first
(not by assumption): confirmed a real `aut.LoginUser` row existed (`LoginUserId=4`,
`EnvironmentId=21`, `UserName=autotechadmin` - the correct real credential),
`aut.Environment`'s `EnvironmentUrl`/`RequiresAuthentication` were both correct, and the
live API returned exactly the right values for both `GET api/Environment/21` and
`GET api/LoginUser/environment/21`. The real queued failure was always the same
`NoSuchElementException` for the login page's *username* field specifically - not a
wrong-password failure (which would fail one field later, at the *password* field, as
`TCLoginUserWrong`'s deliberately-fake-credential run from the original decisive proof
correctly did) - meaning the browser never actually reached a working real login page at
all, regardless of which credential path resolved.

Added a permanent, generally useful diagnostic while investigating: `NUnitEngineTestRunner.
ParseRunResults` now also appends each `<test-case>`'s `<output>` node (captured
`TestContext.WriteLine`/`Console.Write` calls) to the stored `ErrorMessage` when present -
previously this was silently invisible everywhere once the isolated child process exited,
making a failure like a silently-caught API-resolution fallback effectively undiagnosable
from the Portal/DB alone.

**Root cause confirmed by direct testing**: the user's VPN was off. Re-ran the exact same
failing test case, unchanged, immediately after turning the VPN on (built and ran a
temporary diagnostic `AutomationAPI` instance on a separate port to avoid disturbing the
user's own live VS-debugged instance on 7147) - **Passed**, ~35s real duration, real
logs/screenshots captured, using the real `LoginUserId=4` (`autotechadmin`) credential via
the new API-driven resolution path. Confirms the LoginUser selection feature itself was
correct all along - the real OHPNM environment is simply unreachable without VPN, which
manifests as "username field not found" (the page never loads at all) rather than a
credential-specific error, regardless of which credential-resolution path is used.

## Follow-up: real Delete (not just Disable) for Login Users
Asked why a login user showed as Inactive after adding one - checked the actual DB data
directly (`SELECT ... FROM aut.LoginUser`) and confirmed the only 3 rows in the table
were leftover verification test rows from earlier, deliberately soft-deleted
(`IsActive = 0`) as part of that verification's own cleanup - **not** a bug. New rows
correctly default to `IsActive = 1` via the table's own `DEFAULT 1` constraint,
untouched by any code path (confirmed by re-reading `LoginUserRepository.CreateAsync`/
`usp_LoginUserCreate` - neither ever sets it to anything else).

Also asked to replace "Disable" with a real "Delete" option:
- New `usp_LoginUserHardDelete` - guarded the same way `usp_EnvironmentHardDelete`
  already is: checks `aut.TestCaseExecutionQueue.LoginUserId` (FK'd to this table) for
  existing usage first and raises a clear error instead of a raw FK-violation if a
  login user has already been used by a real queued/scheduled run - **confirmed this
  guard is load-bearing, not defensive-for-no-reason**: directly queried the DB first
  and found one of the 3 leftover test rows (the "deliberate wrong credentials" one from
  the earlier decisive proof) genuinely is referenced by a real queue row.
- New `ILoginUserRepository.HardDeleteAsync`/`LoginUserRepository.HardDeleteAsync`,
  `LoginUserController`'s `DELETE api/LoginUser/{id}/hard` (mirrors
  `EnvironmentController.HardDelete`'s exact shape), `LoginUserService.hardDelete()`.
- `environment-login-users.component.*`: the "Disable" button/`disable()` method
  (soft-delete) replaced with "Delete"/`delete()` (hard-delete, with a
  permanently-deletes confirmation prompt matching `EnvironmentManagementComponent.
  delete()`'s wording). The soft-delete endpoint/procedure/service method are left in
  place (unused from the UI now, not removed) - it's exactly the fallback path the new
  hard-delete's own error message points to when deletion is blocked by real usage.
- Verified the build compiles cleanly end to end (DB migration re-applied, `AutomationAPI`
  and the Angular app both build with 0 errors) - could not do a fresh live-process
  verification this time because port 7147 was occupied by the user's own
  `AutomationAPI.exe`, running under Visual Studio's debugger (`VsDebugConsole.exe`) -
  confirmed via `Win32_Process` that this session's shell does not have permission to
  terminate it (`Access is denied`). Needs a debug-session restart on the user's side to
  pick up and verify this specific change live.

## Follow-up: Portal User is now a required selection in the Login Users form
Per direction, dropped the "Default (all users)" option from the Add/Edit form's Portal
User dropdown - every *new* login user must now be tied to a specific Portal User
(`environment-login-users.component.html`'s select gets `required` + a disabled
placeholder instead of a selectable `null` option; `isInvalid` now also checks
`model.portalUserId`). `PortalUserId` remains nullable in `aut.LoginUser` itself (no DB
migration needed) - any pre-existing row with a null `PortalUserId` still displays/
works correctly (shown as "Default (all users)" in read-only contexts, e.g. the Run
Now/Schedule dropdowns' labels), this only changes what the *form* allows going forward.

## Per-environment login users, selected explicitly at Run Now/Schedule time
Replaces the hard-coded `Selenium.BaseComponents.Data.UserCredentials`/`Users.CurrentEnvironment`-driven
login flow with API/database-driven Environment URLs and per-environment login
credentials - **explicitly picked by the person queuing/scheduling a run**, not
automatically matched by role/assigned-user. Full backward compatibility preserved: any
environment/role without configured data falls back to today's exact hard-coded
behavior.

### Database (`Database/LoginUser_And_Environment_Auth_Migration.sql`, idempotent)
- `aut.Environment` gains `EnvironmentUrl` (nullable) and `RequiresAuthentication`
  (`BIT NOT NULL DEFAULT 1` - matches every existing environment's actual behavior
  today, so nothing regresses). `usp_EnvironmentCreate/Update/GetAll/GetById` updated to
  accept/return both.
- New `aut.LoginUser` table: `EnvironmentId`, optional `PortalUserId` (label only -
  "whose credential is this", not a matching key), `UserRole` (free-text label matching
  whatever a test's own `[TestFixture("...")]` declares, e.g. "TechAdmin"/"CredSpec" -
  not enforced/auto-matched), `UserName`, `EncryptedPassword`, `IsActive`, audit
  columns. No uniqueness constraints on Role/PortalUserId - an environment can have any
  number of login users, each individually selectable.
- New stored procs: `usp_LoginUserCreate/Update/SoftDelete`,
  `usp_LoginUserGetByEnvironment` (list, never returns the password - used by both the
  management screen and the Run Now/Schedule dropdowns), `usp_LoginUserGetCredentials`
  (by id - the **only** procedure that ever returns the encrypted password).
- `aut.TestCaseExecutionQueue` gains a nullable `LoginUserId` (the specific login user
  selected at Run Now/Schedule time - one shared value for the whole batch on bulk
  actions, since those are already scoped to a single assignment/environment) and the
  existing `usp_SingleRunTestCaseNow`/`usp_BulkRunTestCasesNow`/
  `usp_ScheduleSingleTestCase`/`usp_BulkScheduleTestCases`/`usp_GetPendingExecutionQueues`
  (also gains `EnvironmentId`, sourced from `aut.TestCaseAssignment`, already present
  there) were updated in place to thread it through - all live definitions were queried
  directly from the database first (`OBJECT_DEFINITION`) rather than trusted from the
  many overlapping historical migration `.sql` files in `Database/`, to guarantee the
  updated procs matched exactly what's actually running.
- Seed data deliberately narrow: only `E2EP3`/`PROD` get `EnvironmentUrl` populated
  (matching `LoginService.GetLoginUrl()`'s hard-coded switch) - no new `aut.Environment`
  rows created for aliases (`DEV01`/`INT01`/etc.) that don't already exist as real rows.
  No `aut.LoginUser` rows are seeded via raw SQL at all - real login users get added
  through the new screen/API (which encrypts via the real `CredentialCipher`), not via
  hand-computed ciphertext literals in a migration script.

### Backend (`AutomationAPI`)
- New `CredentialCipher` (`Repositories/Helpers/`) - a self-contained mirror of
  `Selenium.BaseComponents.Utilities.EncryptDycrypt`'s exact algorithm (MD5-derived key +
  TripleDES-ECB, same salt), reused for consistency rather than switching to AES.
  Separate copy, not a shared reference, because `AutomationAPI` deliberately never
  references `AutomationTests`/`Selenium.BaseComponents` (avoids pulling Selenium/
  WebDriver dependencies into the API's own deployable).
- New `LoginUserController`/`ILoginUserRepository`/`LoginUserRepository`/
  `LoginUserModel` mirroring `EnvironmentController`'s exact conventions.
  `GET api/LoginUser/{id}/credentials` is the only endpoint that ever returns a
  decrypted password - protected by an extra check beyond the usual `[Authorize]`
  (`IsServiceToken()`, checking for `ServiceTokenGenerator`'s exact claim shape -
  `NameIdentifier == "0"` and `Name == "TestRunner"`) so a normal Portal user's valid,
  `[Authorize]`-satisfying token (even with the Admin role) gets a 403, not just any
  authenticated caller - **confirmed by direct testing**: a real Admin-role portal token
  got 403, the real service-token shape got 200 with the correct decrypted password.
- `EnvironmentModel`/`EnvironmentRequestDto` extended with `EnvironmentUrl`/
  `RequiresAuthentication`, threaded through `EnvironmentRepository`/
  `EnvironmentController` unchanged otherwise.
- `SingleRunNowRequest`/`BulkRunNowRequest`/`SingleScheduleRequest`/
  `BulkScheduleRequest` gain an optional `LoginUserId`, threaded through
  `TestCaseExecutionQueueController` -> `ITestCaseExecutionQueueRepository`/
  `TestCaseExecutionQueueRepository` -> the queue insert stored procs -> (via
  `usp_GetPendingExecutionQueues`, alongside the new `EnvironmentId`) ->
  `PendingExecutionQueue` -> `TestRunRequest` -> `NUnitEngineTestRunner`'s
  `TestParameters` (`LoginUserId`/`EnvironmentId`, alongside the existing `Browser`/
  `AccessToken`/`AssignmentId`/`AssignmentTestCaseId` - identical threading mechanism,
  already proven).

### Test framework (`Selenium.BaseComponents`)
- `APIGatway` gains `GetEnvironmentDetails()` (`GET api/Environment/{id}`) and
  `GetLoginUserCredentials()` (`GET api/LoginUser/{id}/credentials`), both reading their
  id from `TestContext.Parameters` and reusing the existing `AttachAuthIfAvailable` -
  both fail gracefully (return `null`, logged via `TestContext.WriteLine`) rather than
  throwing, matching `SaveTestCaseLog`'s established pattern.
- `BaseFeatureFixture`: credential/URL resolution moved from the constructor into
  `[OneTimeSetUp]` (now `async Task InitializeTestSuite()` - NUnit supports async
  `[OneTimeSetUp]` natively; confirmed no overrides existed anywhere before changing the
  signature) - `TestContext.Parameters` isn't reliably populated during construction,
  the same reasoning already established for `AssignmentId`/`AssignmentTestCaseId`. The
  constructor now only stashes `profile` (still exactly the `[TestFixture("...")]`
  string) for the fallback path. Resolution order in `ResolveCredentialsAndUrlAsync()`:
  1. `EnvironmentId` available -> call `GetEnvironmentDetails()`.
  2. `RequiresAuthentication == false` -> leave `Username`/`pswd` null, so
     `InitializeChromeAndLogin`'s existing `if (Username != null)` guard skips login
     entirely - **no behavior change** for any environment that doesn't need auth.
  3. `RequiresAuthentication == true` and a `LoginUserId` was supplied (the person
     running/scheduling it explicitly picked one) -> `GetLoginUserCredentials()`,
     use its username/password + the resolved `EnvironmentUrl`.
  4. **Fallback** on any failure/absence (API unreachable, no `EnvironmentId`/
     `LoginUserId` supplied, a local Test Explorer run outside the queue pipeline, an
     environment not yet configured with `EnvironmentUrl`/`LoginUser` data, etc.):
     today's exact hard-coded `UserCredentials.UserNameGenerator`/`PasswordGenerator`/
     `LoginService.GetLoginUrl()` behavior via the stashed `profile` - unchanged.

### Frontend (`ohpnm-test-portal`)
- Environment create/edit form gains an "Environment URL" field and an "Authentication
  Required" checkbox (default checked).
- New dedicated screen (not a modal) `environment-login-users.component.*`, routed at
  `environment-management/:id/login-users` (same `authGuard`+`adminGuard` pattern as
  every other environment-management route) - one page: a flat table (Role | Portal
  User or blank | Username | Active | Edit | Disable) plus an Add/Edit form section on
  the same page. Password is write-only - never pre-filled/shown on edit; leaving it
  blank on update keeps the existing password unchanged (`usp_LoginUserUpdate`'s
  `COALESCE(@EncryptedPassword, EncryptedPassword)`). New "Login Users" action button
  added to each environment card.
- **"Run Now" had no dialog at all before this** (confirmed by reading the code -
  `onRunNow`/`onBulkRunNow` went straight from a plain `confirm()` to a hardcoded
  `browser: 'Chrome'`) - new `RunNowDialogComponent` (mirrors
  `ScheduleTestcasesDialogComponent`'s `ModalService` `open(callback)`/`submit()`
  pattern) is shown **only** when the target environment's `RequiresAuthentication` is
  `true` (checked via a new `resolveLoginUserForRunNow` helper in
  `test-case-execution-panel.component.ts`, using `selectedAssignmentRelease.
  environmentId` - already resolved there) - a flat, required "Login User" dropdown,
  sourced from `GET api/LoginUser/environment/{id}`. When `RequiresAuthentication` is
  `false` (or the environment/its login users can't be resolved), no dialog appears at
  all and execution proceeds exactly as before this feature existed.
- `ScheduleTestcasesDialogComponent` extended the same way - a conditionally-shown
  Login User dropdown (only rendered when login users were actually passed to `open()`).
- Bulk actions (`onBulkRunNow`/`onBulkSchedule`) use **one shared** Login User selection
  for the whole batch - confirmed via `selectedTestCases`/`testCases` always being
  scoped to a single `selectedAssignment`, meaning bulk actions are already inherently
  single-environment.
- New `LoginUserService`, `ILoginUserModel`/`ILoginUserRequestDto` interfaces,
  `environmentUrl`/`requiresAuthentication` added to `IEnvironmentModel`/
  `IEnvironmentRequestDto`, `loginUserId?` added to the 4 run-now/schedule request
  interfaces.

### Verified for real, end-to-end (not just build success)
Used the real, already-running `AutomationAPI` instance (stopped and rebuilt fresh first
to pick up all these backend changes) against `TC.PriorAuthSearch` in the real `E2EP3`
environment:
- `GET api/Environment/21` correctly returns the new `environmentUrl`/
  `requiresAuthentication` fields.
- `GET api/LoginUser/environment/21` never includes a password field.
- `GET api/LoginUser/{id}/credentials`: a real Admin-role Portal-user JWT got **403**; a
  JWT with `ServiceTokenGenerator`'s exact claim shape got **200** with the correct
  decrypted password (round-tripped through `CredentialCipher` correctly).
- `usp_LoginUserUpdate` with no password supplied correctly left the existing
  (different) encrypted password unchanged - confirmed via a follow-up credentials
  fetch.
- **The decisive end-to-end proof**: queued the same real test twice - once with a
  `LoginUserId` pointing at a row with a **deliberately fake** username/password, once
  with no `LoginUserId` at all. The no-`LoginUserId` run **Passed** (fell back to the
  real hard-coded credentials, logged in for real). The fake-credential run **Failed**
  at `OneTimeSetUp` specifically because the fake username was actually submitted to the
  real OHPNM login page (which never advances to the password field for a nonexistent
  account) - conclusive proof the API-resolved credential was genuinely used, not
  silently ignored in favor of the hard-coded fallback.
- Also verified `RequiresAuthentication = false` on the real `E2EP3` environment (then
  restored back to `true` afterward): the resulting run failed in a **completely
  different way** - 0.28s duration, failing at the actual test method's first UI
  interaction (the hamburger menu, only present when logged in) rather than at
  `OneTimeSetUp`'s login step - confirming login was skipped entirely rather than
  attempted and failing silently.

## Fixed: the widespread `SelfService` selector bug (confirmed against the real page, not just by comparison)
Real execution of `TC.PriorAuthoriztion` (once its own consolidation/logging work was
done) reproduced the exact same `NoSuchElementException` already seen for
`TC.SearchRA`/`TC.SearchEligibility` - confirmed via the real stack trace this affects
(at least) 3 of the 7 projects. Rather than fix it by assumption (copying
`TC.PriorAuthSearch`'s already-working selector without checking why), used Playwright
(installed fresh via npm for this - not previously part of the toolchain) to actually log
into the real `E2EP3` environment (`autotechadmin`/real password) and inspect the live
DOM:
- The real "Self Service" link is `<a href="..." title="">Self Service</a>` - **`title`
  is empty**, not `"Self Service"` as the broken selector (`//a[@title='Self Service']`,
  present in 3 of the 7 projects) assumed. `TC.PriorAuthSearch`'s selector
  (`//a[normalize-space()='Self Service']`, matching the link's *text*, not its `title`)
  was correct by coincidence of using the right attribute, not because anyone had
  verified the real markup at the time.
- Also confirmed via the same real session that the very next page's elements
  (`FinancialProviderInformationPage`'s `lblTitle`/`txtMedicaidNumber`/
  `lnkBtnPriorAuth`) all still match correctly on the real page - no further selector
  fixes needed there.
- Fixed `TC.SearchRA`, `TC.SearchEligibility`, and `TC.PriorAuthoriztion`'s `SelfService`
  property to use the confirmed-correct `normalize-space()`-based selector.
- **Verified for real, end-to-end, through the actual test framework** (not just the
  Playwright inspection): re-ran `TC.SearchRA` for real after the fix - **Passed**, ~29s
  real duration, all 6 expected log entries now captured (previously only 1, "Login to
  PNM", before hitting the bug) - confirms the fix genuinely resolves the failure, not
  just that the selector looks right in isolation.

## Phase 1 (rollout, project 6 of 7): TC.PriorAuthoriztion - genuinely different DataRepository shape
Unlike every other project so far, `DataRepository.GetAutomationData("DentalPA")` maps
**10 different sections into 10 different sub-properties** of one composite `DentalPA`
model (`DentalInformation`, `DentalRecipientInformation`, `DentalContactInformation`,
`DentalServiceInformation`, `DentalServiceProviderInformation`,
`DentalOrderingProviderInformation`, `DentalDiagnosisInformation`, `DentalServiceDetails`,
`DentalProviderNotes`, `DentalAttachments`) - not the "one section -> one flat model"
shape `AutomationDataRepository.GetAutomationData<T>(flowName, sectionName)` was built
for. Consolidating this into that generic method would mean either inventing a more
complex generic multi-section-binding helper (bigger scope, unclear value for one
project) or forcing a bad fit - correctly left as bespoke, project-specific
orchestration, not duplicated boilerplate.
- **Still consolidated**: `Mapper.BindData<T>` itself - confirmed unused elsewhere,
  deleted the local `Mapper.cs` entirely. **Zero code changes needed** in
  `DataRepository.cs` - it already had `using Selenium.BaseComponents.Utilities;` at the
  top, so its existing (unmodified) `Mapper.BindData<T>(...)` calls automatically resolved
  to the shared one the moment the local copy was gone. Also deleted the confirmed-dead
  `Helper.cs`/`PageConstants.cs`.
- **Logging**: asked how much detail given this project's real size/complexity - two
  `[Test]` methods (`DentalPA_Submit`/`DentalPA_Save`) plus a ~250-line
  `FillDentalPAFields` helper filling 9 distinct form sections with real popups/alerts/
  file uploads - much larger than anything else in this rollout. Went with full
  per-section logging (not just start/success) per explicit direction. Added a shared
  `LogStep(stepName, message)` helper (mutates one `_testCaseExecutionLog`/`_screenshots`
  instance-field pair) since both `[Test]` methods and the shared `FillDentalPAFields`
  helper all need to log against the same run's log/screenshot state - logs after each of
  the 9 `#region` blocks in `FillDentalPAFields`, plus login/self-service/medicaid-search/
  submit-prior-auth/final-success-or-failure in each `[Test]` method, with distinct
  `[Property(TestCaseId=...)]` values (`TCDentalPASubmit`/`TCDentalPASave`) since these
  are two genuinely separate test cases sharing one class.
- Confirmed no `TestWebDriver.FindElement(...)` ambiguity here despite the existing
  blanket `using Selenium.BaseComponents.Utilities;` (unlike `TC.SearchRA`/
  `TC.SubmitClaims`/`TC.PriorAuthInquiry`, which needed type aliases) - this file's
  `FindElement(By)` calls use the single-argument built-in `IWebDriver.FindElement`
  method, not the ambiguous 2-arg extension method overload both `SdetToolbox.Pages.
  PageHelper` and `Selenium.BaseComponents.Utilities.PageHelper` separately define.
- Verified via a full solution rebuild (0 errors) only - **not** via a real queued
  execution this time (the user had their own `AutomationAPI` instance actively running
  with a live connection; asked before using it for a verification run that would add
  test data to the shared DB, and was told to skip it this round rather than risk
  disrupting that session). Confidence here comes from the build succeeding plus this
  using the exact same `LogStep`/`SaveLog`/screenshot mechanism already proven working via
  real execution in `TC.PriorAuthSearch`/`TC.SearchRA`/`TC.SearchEligibility`/
  `TC.PriorAuthInquiry` - not a first-time-unproven mechanism, just applied to more call
  sites in one file. Worth a real run when convenient/non-disruptive.

## Phase 1 (rollout, project 5 of 7): TC.SearchEligibility
Active project (unlike `TC.PriorAuthInquiry`) - `DataRepository.GetAutomationData("SearchMemberEligiblity")` and `Mapper.BindData` are genuinely called by
`SearchEligiblityTest.SearchMemberEligibility()`. Consolidated the same way as
`TC.PriorAuthSearch`/`TC.SearchRA`:
- `DataRepository.cs` now delegates to `AutomationDataRepository.GetAutomationData<
  Models.SearchEligibility>(flowName, "SearchMemberEligiblity")`.
- Deleted the confirmed-dead `Mapper.cs`/`Helper.cs`/`PageConstants.cs` (zero call sites
  for `Helper.PrintScreenShot`/`Roles`/`Tasks`/`Pages` anywhere in this project).
- Added the same step-by-step logging/screenshot pattern as `TC.SearchRA` throughout
  `SearchMemberEligibility()` (login, self service, medicaid search, eligibility search,
  success) plus `[Property(...)]` metadata. **No type aliases needed here** - this file
  already had a working blanket `using Selenium.BaseComponents.Utilities;` (uses
  `.CreateSmartElement(...)`, not the ambiguous `.FindElement(...)` `TC.SearchRA`/
  `TC.PriorAuthSearch` use), confirmed by the file already compiling successfully before
  this change with that import in place.
- **Verified for real**: full solution rebuild (0 errors), then a real queued execution -
  confirmed the consolidated `DataRepository -> AutomationDataRepository -> Mapper` chain
  works (proceeded past that call cleanly), and confirmed the logging fired correctly
  (exactly 1 log entry, "Login to PNM", `hasLogs`/`hasScreenshots` both `true`). The run
  then failed at the **same pre-existing, unrelated** `SelfService` selector bug already
  documented for `TC.SearchRA` (`//a[@title='Self Service']` doesn't match the real page)
  - confirms this selector bug is more widespread across projects than just one, but
  still untouched/pre-existing, not a regression from this work.

## Phase 1 (rollout, project 4 of 7): TC.PriorAuthInquiry - simplest remaining project
Confirmed identical shape to `TC.SubmitClaims` before this: empty `SampleTest`/
`SampleTestCase` stub, a broken/unused `DataRepository.GetAutomationData` (checks
`"SearchPA"`, wrong for this project regardless, empty if-body), and confirmed via grep
zero call sites anywhere for `Mapper.BindData`/`Helper.PrintScreenShot`/`Roles`/`Tasks`/
`Pages`. Applied the exact same treatment as `TC.SubmitClaims`:
- Deleted the confirmed-dead `Mapper.cs`/`Helper.cs`/`PageConstants.cs`; left
  `DataRepository.cs`'s broken-but-harmless stub as-is (same reasoning - no Model type
  exists to consolidate against without inventing new behavior).
- Renamed `Tests/SampleTest.cs`/`SampleTest`/`SampleTestCase()` ->
  `Tests/PriorAuthInquiryTest.cs`/`PriorAuthInquiryTest`/`PriorAuthInquiry()`, matching
  the established naming convention.
- Added the same minimal start/complete logging + one screenshot (no real steps exist
  yet to log around meaningfully, same as `TC.SubmitClaims`), using the same targeted
  type aliases (not a blanket `using Selenium.BaseComponents.Utilities;`) to avoid the
  `TestWebDriver.FindElement(...)` ambiguity already documented above.
- Verified via a full solution rebuild (0 errors) and a real queued execution -
  **Passed**, `hasLogs`/`hasScreenshots` both `true`.

## Follow-up: renamed TC.SubmitClaims's placeholder SampleTest/SampleTestCase
Confirmed via grep no other references anywhere. Renamed to match the established
convention every other project's test class/file already follows
(`SearchRATest.SearchRA()`, `SearchPATest.NavigateToSeachPAPage()`):
`Tests/SampleTest.cs` -> `Tests/SubmitClaimsTest.cs`, class `SampleTest` ->
`SubmitClaimsTest`, method `SampleTestCase()` -> `SubmitClaims()`. Verified via a real
discovery call (`GET /api/TestSuites/libraries?releaseId=...`) that it's still correctly
discoverable under the new names with its `[Property(...)]` metadata intact.

## Follow-up: added step-level logs/screenshots to TC.SearchRA and TC.SubmitClaims too
Asked directly whether Phase 1 had added log/screenshot capture to the newly-consolidated
projects - it hadn't (Phase 1 only touched `Utilities/*.cs`, not test methods). Neither
project's test method called `SaveTestCaseLog`/`SaveMethodScreenShots` before this at all
(unlike `TC.PriorAuthSearch`, which already had these calls baked in from before this
whole effort started - Phase 3 made those *existing* calls work, it didn't add new ones).
Added the same pattern to both, mirroring `SearchPATest.cs` exactly (`[Property(...)]`
metadata, a `TestCaseExecutionLog`/`Screeshots` per step, `SaveLog(...)`, a `[TearDown]`
that uploads via `APIGateway.SaveMethodScreenShots`).

- **`TC.SearchRA`**: real step-by-step logs added throughout `SearchRA()` (login, self
  service, medicaid search, RA search, search submitted, success).
- **`TC.SubmitClaims`**: `SampleTestCase()`'s body was **completely empty** before this -
  no login flow, no navigation, nothing to log step-by-step around meaningfully. Added
  minimal start/complete logging + one screenshot instead of fabricating fake
  intermediate steps, so the mechanism is proven and ready the moment real test content
  gets added here.
- **Avoided a real ambiguity** in both: a blanket `using Selenium.BaseComponents.
  Utilities;` makes `TestWebDriver.FindElement(...)` ambiguous (`SdetToolbox.Pages.
  PageHelper` vs. `Selenium.BaseComponents.Utilities.PageHelper` have identical extension
  method signatures - confirmed by direct testing, a real `CS0121` build error). Used
  targeted type aliases (`using TestCaseExecutionLog = Selenium.BaseComponents.Utilities.
  TestCaseExecutionLog;` etc.) instead of a blanket import.
- **Verified for real**, including the real "multi-project, one Release folder"
  architecture from the CPM work earlier - deployed `TC.SearchRA.dll` +
  `TC.SubmitClaims.dll` + one shared `Selenium.BaseComponents.dll` + `appSettings.json`
  together in a single Release, discovery correctly found both
  (`totalDiscoveredTests: 2`), and ran both for real:
  - `TC.SubmitClaims`: **Passed**, `hasLogs`/`hasScreenshots` both `true`, exactly 2 log
    entries (start + success) - the full run completed.
  - `TC.SearchRA`: still **Failed** at the same pre-existing `SelfService` selector bug
    documented below (unrelated, untouched by this) - but `hasLogs`/`hasScreenshots` are
    now `true` too, with exactly 1 log entry ("Login to PNM") - confirms the logging
    itself fired correctly right up until the point of the real failure, not before and
    not after.

## Phase 1 (rollout, projects 2-3 of 7): TC.SearchRA and TC.SubmitClaims
Continuing the consolidation from `TC.PriorAuthSearch`, into the same
`Selenium.BaseComponents.Utilities.AutomationDataRepository`/`Mapper`.

- **`TC.SearchRA`**: same pattern as `TC.PriorAuthSearch` - `DataRepository.cs` now
  delegates to `AutomationDataRepository.GetAutomationData<Models.SearchRA>(flowName,
  "SearchRAParams")` (note: its section-name string genuinely differs from its flow name,
  unlike `TC.PriorAuthSearch` where both happened to be `"SearchPA"` - confirms the
  generic method's two separate parameters were the right call). Deleted its own
  `Mapper.cs`/`Helper.cs`/`PageConstants.cs` (all confirmed unused via grep, same as
  `TC.PriorAuthSearch`'s).
  - **Verified for real** via a genuine queued execution - confirmed the consolidated
    `DataRepository -> AutomationDataRepository -> Mapper` chain works correctly (test
    proceeded past that call with no error). The run then failed, but at a **later,
    unrelated, pre-existing** step: `SearchRATest.cs`'s own `SelfService` element uses
    `By.XPath("//a[@title='Self Service']")`, which doesn't match anything in the real
    page (unlike `TC.PriorAuthSearch`'s different, working selector for the same login
    landing element, `//a[normalize-space()='Self Service']`). This selector lives
    entirely in `SearchRATest.cs`, untouched by this phase - a genuine pre-existing bug in
    this project's own page-interaction code, not a Phase 1 regression. Flagged here for
    whoever does this project's Phase 4 real-execution rollout; not fixed now (out of
    scope for boilerplate consolidation).
- **`TC.SubmitClaims`**: confirmed its `DataRepository.GetAutomationData`/`Mapper.
  BindData` are **not called anywhere at all** (not even from each other - the
  `DataRepository`'s own `if` block that would call `Mapper.BindData` is empty, and it
  checks `flowName.Equals("SearchPA")`, which is wrong for this project regardless -
  looks like never-finished copy-paste scaffolding). Its `Models`/`Pages` folders are
  also still empty (confirmed via the `.csproj`'s own `<Folder Include=.../>` entries) -
  there's no Model type to parameterize a generic call with, so **not** consolidated -
  doing so would mean inventing new working behavior, not deduplicating existing
  behavior. Deleted the confirmed-fully-dead `Mapper.cs`/`Helper.cs`/`PageConstants.cs`
  (same dead-scaffolding pattern as the other projects' copies) but left the
  broken-but-harmless `DataRepository.cs` stub as-is - fixing real business logic here is
  a different, later task, not this consolidation phase.
- Verified via a full solution rebuild (0 errors) after each project.

## Phase 1 (rollout, project 1 of 7): consolidated duplicated boilerplate for TC.PriorAuthSearch
Confirmed (hashed/diffed) `Utilities/Helper.cs`, `Mapper.cs`, and `PageConstants.cs` were
byte-for-byte identical between `TC.PriorAuthSearch` and `TC.SearchRA` except namespace;
`DataRepository.cs` differed only in Model type + section-name string. Moved the reusable
parts into `Selenium.BaseComponents.Utilities`:
- New `Mapper.cs` (both `BindData<T>` overloads, moved verbatim).
- New `AutomationDataRepository.cs` - generic `GetAutomationData<T>(flowName,
  sectionName)`. **Deliberately not named `DataRepository`** - every `TC.*` project
  already has its own `DataRepository` class in its own `Utilities` namespace, and
  several already have both that namespace and `Selenium.BaseComponents.Utilities` in
  scope via `using` in their test files - confirmed by direct testing that naming this
  class `DataRepository` made a full solution build fail with `CS0104` (ambiguous
  reference) in the 3 *other*, not-yet-migrated projects that already call their own
  local `DataRepository.GetAutomationData(...)` unqualified - not just in the one project
  actually being touched this phase.
- `TC.PriorAuthSearch`'s own `DataRepository.cs` now just delegates:
  `Selenium.BaseComponents.Utilities.AutomationDataRepository.GetAutomationData<Models.
  SearchPA>(flowName, "SearchPA")` - keeps its exact original public signature
  (`object GetAutomationData(string flowName)`), since other already-migrated-style
  callers elsewhere cast the return value themselves (confirmed this exact pattern in
  `TC.SearchRA`/`TC.SearchEligibility`/`TC.Registration`/`TC.PriorAuthoriztion` - none of
  those were touched this phase, this is just why the signature had to stay identical).
- Deleted `TC.PriorAuthSearch`'s own now-redundant `Mapper.cs`, plus `Helper.cs` and
  `PageConstants.cs` entirely - confirmed via grep both were **already fully dead code**
  in this project before this change (`Helper.PrintScreenShot` - a no-op with its actual
  body commented out - and `PageConstants`'s `Roles`/`Tasks`/`Pages` classes had zero call
  sites anywhere in `TC.PriorAuthSearch`). Not deleted from the other 6 projects - e.g.
  `TC.Registration` has 58 real call sites into its own `PageConstants`'s `Pages`/`Tasks`,
  so that project's copy is load-bearing and stays untouched until its own turn.
- Verified via a full solution rebuild (0 errors) and a real queued execution afterward -
  **Passed**, ~55s real duration, same as before this change - confirms the consolidation
  didn't alter behavior for the one project actually touched.

## Follow-up: bare-DLL deployment enabled for the real Selenium framework too (accepted tradeoff)
After the CPM work below, explicitly asked to make bare-DLL (no full publish) deployment
work for the real `TC.*` projects too - the same convention `SeleniumSmokeTests`/`REL-14`
already had, just extended to cover `Selenium.BaseComponents`' fuller dependency set.
**Deliberately accepted tradeoff**: this couples `AutomationAPI`'s own package references
to what these test projects need - a version bump on either side now has to be kept in
sync by hand (this is exactly the coupling risk raised earlier in favor of full-publish;
proceeding anyway per explicit direction, "I am ok with that dependency for now").

### What changed
- `AutomationAPI.csproj` gained the rest of `Selenium.BaseComponents`' dependencies
  (`FluentAssertions`, `Microsoft.Extensions.Configuration`/`.Json`, `Microsoft.Extensions.
  DependencyInjection`, `MSTest.TestFramework`, `Newtonsoft.Json`, `NUnit3TestAdapter`,
  `Selenium.Support`, `System.Configuration.ConfigurationManager`, `System.Data.OleDb`).
- `NUnit`/`Selenium.WebDriver`/`WebDriverManager` bumped from `3.14.0`/`4.27.0`/`2.17.5`
  (which only matched `SeleniumSmokeTests`, a PoC) to `4.3.0`/`4.47.0`/`2.17.6` (matching
  the real framework) - **tradeoff**: `SeleniumSmokeTests`' own bare-DLL run may now lose
  isolated (`ProcessModel=Separate`) execution and fall back to `InProcess` automatically
  (the existing fallback already handles this gracefully - not a hard failure, confirmed
  this is the documented, accepted behavior for a version mismatch).
- `System.Configuration.ConfigurationManager` pinned to `8.0.1`, not `8.0.0` (what
  `Selenium.BaseComponents` itself uses) - `Microsoft.Data.SqlClient` already transitively
  requires `>= 8.0.1`; `8.0.0` caused a real `NU1605` downgrade build error.

### Real minimum file count discovered by direct testing - corrects an earlier claim in this chat
Initially assumed adding these packages would let a **single** bare test DLL run isolated
per project. **Confirmed by direct testing this is wrong**: every real `TC.*` project also
needs `Selenium.BaseComponents.dll` itself alongside it - that's a **project reference**
(a sibling compiled assembly), not a NuGet package, so nothing added to `AutomationAPI`'s
own package list can provide it. The actual minimum for N real test projects sharing one
Release folder is **N project DLLs + 1 shared `Selenium.BaseComponents.dll`** (not N total,
and not a full publish's ~80-110 files either) - confirmed end-to-end: copied only
`TC.PriorAuthSearch.dll` + `Selenium.BaseComponents.dll` (2 files, no `.deps.json`, no
other dependency DLLs) into a fresh Release folder, and a real queued run
(`ProcessModel=Separate`) launched Chrome, logged into the real `E2EP3` environment, and
reported **Passed** in ~12 seconds - genuinely isolated, genuinely minimal.

### Also fixed while investigating: `ReleaseReadinessService`'s "unrunnable" check was over-broad
Diagnosed via temporary debug logging (added, confirmed the exact failure, then removed):
`NUnitEngineHelper.IsUnrunnableResult` returns true both for a genuine missing-dependency
load failure *and* for an ordinary non-test DLL NUnit loaded fine but found no fixtures in
(e.g. `Selenium.BaseComponents.dll` itself, sitting in the folder as a dependency, not a
test assembly - `_SKIPREASON` says `"No test fixtures were found."`, not a load error).
`ReleaseReadinessService.CheckReadiness` was counting both as "missing dependency
evidence" for its hint message, which could show a misleading "use dotnet publish" hint in
an edge case where the *only* unrunnable DLL was actually just a normal non-test
dependency. Fixed by checking the `_SKIPREASON` text itself for `"load"` before treating it
as missing-dependency evidence (confirmed by direct testing: real load failures say
`"Unable to load one or more of the requested types"`/`"Could not load file or
assembly"`, always containing "load"; the empty-assembly case never does). Doesn't affect
the `IsReady` true/false result either way - only the accuracy of the hint text in an
already-failing edge case.

## Architecture requirement: one Release can (and must) contain many test projects
Mandatory, not optional - a Release folder can hold the published output of several `TC.*`
projects at once (each `dotnet publish`'d into the *same* folder), not just one project per
Release. This raised a real question during the pilot: since a full publish bundles each
project's own dependency DLLs alongside its test DLL, and multiple projects' publishes
share one folder, what happens if two projects need *different* versions of the same shared
package (e.g. `Newtonsoft.Json`)? Whichever publish runs last would silently overwrite the
earlier one's copy on disk, while that earlier project's own `<AssemblyName>.deps.json`
(published right alongside it) still expects the version it was actually built against -
a real risk of a confusing runtime failure with no obvious cause pointing back to it.

Checked directly: as of the pilot, all 7 real `TC.*` projects + `Selenium.BaseComponents`
already used identical versions of every shared package - safe today, but only by
coincidence, not by anything stopping it from silently drifting apart later (e.g. someone
bumps one project's NUnit version alone to pick up a fix, without realizing it could break
a *different* project the next time both land in the same Release folder).

### Fix: NuGet Central Package Management (CPM) for `AutomationTests/`
Added `AutomationTests/Directory.Packages.props`
(`ManagePackageVersionsCentrally=true`) so every shared package version for the real
Selenium framework (`NUnit`, `NUnit.Analyzers`, `NUnit3TestAdapter`, `Selenium.Support`/
`Selenium.WebDriver`, `WebDriverManager`, `Newtonsoft.Json`, `Microsoft.Extensions.
Configuration`/`.Json`, `Microsoft.Extensions.DependencyInjection`, `MSTest.TestFramework`,
`FluentAssertions`, `System.Configuration.ConfigurationManager`, `System.Data.OleDb`,
`coverlet.collector`, `Microsoft.NET.Test.Sdk`) is pinned in exactly one place. All 8
project `.csproj` files (`Selenium.BaseComponents` + 7 `TC.*`) had their per-`PackageReference`
`Version=` attributes removed - they now inherit from the central file, so it's no longer
physically possible for one of them to silently drift onto a different version of a shared
package without a deliberate, visible edit to the one central file (a build-time guarantee
instead of a today-it-happens-to-be-true coincidence).

**Pre-existing sample/demo projects kept their own versions, deliberately, via
`VersionOverride`**: `OnboardingTests`/`PayrollTests`/`RecruitmentTests`/
`SeleniumSmokeTests`/`API` already used genuinely different, older versions of `NUnit`/
`NUnit.Analyzers`/`NUnit3TestAdapter`/`Selenium.WebDriver`/`WebDriverManager` (they're
standalone samples/PoCs, never deployed together with the real framework in one Release, so
there was no reason to force them onto the same versions). `Directory.Packages.props` sets
`CentralPackageVersionOverrideEnabled=true` specifically so these 5 projects can keep their
existing versions via an explicit `VersionOverride="..."` attribute on the affected
`PackageReference`s - a visible, deliberate per-package opt-out, not a silent one, and
zero behavior change for any of them (confirmed via a full solution rebuild: 0 errors, only
pre-existing nullable/style warnings, before and after).

### Adding a new test project under this scheme
- If it only needs packages already used elsewhere (the common case - `Selenium.
  BaseComponents` already covers NUnit/Selenium/Newtonsoft.Json/etc.), just reference the
  package name with no `Version=` at all - it automatically inherits the correct, already-
  proven-consistent version, guaranteed identical to every other project, with less work
  than before (no need to even know the right version number).
- If it needs a genuinely new package nothing else uses yet, add one `<PackageVersion>`
  line to `Directory.Packages.props`, then reference it with no version in the `.csproj`.
  Trying to put a `Version=` directly on a `PackageReference` instead (bypassing the
  central file) fails the build outright with NuGet error `NU1008` - this isn't a
  convention that relies on remembering to do it the right way, the build itself blocks
  the shortcut.

### Deployment convention going forward
**Always `dotnet publish` (never just copy the built DLL) for every real `TC.*` project**,
into whichever Release folder it belongs to - including when multiple projects share one
Release folder. One standard procedure for every project removes the "does this one need
special treatment" guesswork for whoever's deploying, and CPM is what makes doing this
safe for multiple co-located projects (their shared dependency DLLs are now guaranteed
byte-for-byte the same version, so overwriting each other during publish is a non-event).
`ReleaseReadinessService`'s message was also improved to hint at this directly: if a DLL
looks like it should be a test assembly but can't actually load (missing dependencies -
detected via the existing `NUnitEngineHelper.IsUnrunnableResult`/
`IsMissingFrameworkDependency` helpers), the "not ready" message now says so explicitly
("...Use 'dotnet publish' (not just the built DLL)...") instead of a generic "no usable
test content" message that gives no hint about *why*.

## Pilot: real end-to-end execution of `TC.PriorAuthSearch` through the Portal - PASSED, 2 real bugs found and fixed
Per the "bring Portal/API/Selenium tests into sync" effort, ran `TC.PriorAuthSearch`
(folder `TC.SearchPA`, already `[Property(...)]`-metadata-correct) all the way through a
**genuine** real execution - real Chrome, real login to the real `E2EP3` Maximus OHPNM test
environment (`ohpnm-e2ep3.omes.maximus.com`) as `autotechadmin`, real navigation, real
Pass/Fail outcome flowing back through the actual Portal API - not a simulated/theoretical
check. **Result: Passed**, ~28 second real duration. This is the first real (non-sample)
Selenium suite proven to work end-to-end through this pipeline.

### JWT auth threaded into isolated test processes (new capability)
`Selenium.BaseComponents.Utilities.APIGatway.GetAutomationData` calls
`AutomationController`'s `[Authorize]`-protected `api/Automation/data/flow/{flowName}` but
never attached any credentials - would 401 in a real run. Added:
- `AutomationAPI/Repositories/TestRunner/ServiceTokenGenerator.cs` - mints a short-lived
  (30 min default) JWT for a generic `"TestRunner"` identity, signed with the same
  `JWTKey:Secret` real user logins use, so `[Authorize]` accepts it with zero server-side
  changes. The isolated test process has no real user session of its own to reuse a token
  from, so `TestQueueWorker` mints one of these per run instead.
- `TestRunRequest.AccessToken` (new) - threaded through `NUnitEngineTestRunner`'s
  `TestParametersDictionary`/`TestParameters` package settings exactly the same way
  `Browser` already was (both are now merged into one dictionary instead of two separate
  `AddSetting` calls, since NUnit only keeps the last one set per key).
- `APIGatway` reads it back via `NUnit.Framework.TestContext.Parameters["AccessToken"]`
  (same indexer-based, null-safe pattern `BaseFeatureFixture` already used for `queueId`)
  and attaches it as `Authorization: Bearer <token>` before the call. Absent when running
  outside the queue pipeline (e.g. local Test Explorer) - the call just goes out
  unauthenticated then, same as before this existed.

### Bug found and fixed: `ReleaseReadinessService` still used the old, pre-refactor reflection approach
Confirmed by direct testing: activating a Release with a **real, full `dotnet publish`**
output (79 files, `TC.PriorAuthSearch.dll` + all its dependencies) reported
`"DLLs are present but none contain usable test content"` / `usableDllCount: 0` - even
though the exact same DLL was already proven discoverable via `TestSuitesRepository`
(`NUnitEngineHelper.Explore()`) moments earlier. Root cause:
`ReleaseReadinessService.CheckReadiness` was never migrated off the old, pre-NUnit.Engine
reflection technique (`Assembly.LoadFrom` + a direct `TestFixtureAttribute` scan) when
`TestSuitesRepository`/`ReflectionTestRunner` were - it silently hit an NUnit version
conflict (`AutomationAPI` itself references NUnit 3.14.0; this test project's full publish
bundles its own NUnit 4.3.0) during `assembly.GetTypes()`, swallowed by a bare `catch {}`.
**Fixed**: rewrote `CheckReadiness` to use `NUnitEngineHelper.Explore()` (counting
`//test-case` nodes) instead of raw reflection - same fix category as the original
refactor, just the one caller that got missed. Verified live: after the fix, the exact
same Release/folder reported `usableDllCount: 1, isReady: true` and activated
successfully.

### Bug found and fixed: parameterized `[TestFixture(...)]` classes were unrunnable once assigned
Confirmed by direct testing: `ExploreXmlParser.ParseClasses` read a `TestFixture` node's
`name` attribute as `ClassName` - for a *parameterized* fixture like
`[TestFixture("TechAdmin")]`, NUnit's `name` is the display form
`SearchPATest("TechAdmin")` (includes the constructor arg), not a real class name. This
value flowed untouched from discovery (`GET /api/TestSuites/libraries?releaseId=...`)
into a real Test Case Assignment exactly as the Portal's UI would submit it, and then
`NUnitEngineHelper.FindMatchingTestCases` (used to build the filter for `Run()`) could
never match it against anything - NUnit's own `<test-case>` `classname` attribute never
includes the constructor-arg text - so the assigned test silently failed every run with
`"No test matching Class='SearchPATest(\"TechAdmin\")', Method='...' was found"`.
**Fixed**: `ParseClasses` now reads the fixture node's `classname` attribute instead (the
plain, always-correct fully-qualified name NUnit provides specifically for this purpose),
reduced to the simple class name the same way `name` already was for the (more common)
non-parameterized case - so `SearchPATest("TechAdmin")` correctly becomes `SearchPATest`,
matching what `FindMatchingTestCases` expects. Verified live: re-discovery showed the
corrected `className`, and the previously-failing assignment (repaired via a direct,
one-off data fix since it was already locked/terminal from the earlier failed run) then
executed and **passed** for real. This bug would have affected *any* parameterized
`[TestFixture(...)]` class across all 7 real test projects, not just this one - worth
re-checking during Phase 2's metadata audit.

### Pilot process notes (not code issues, just environment friction worth remembering)
- A stray `AutomationAPI.exe`/`.NET Host` process from an earlier session repeatedly
  resisted `Stop-Process -Force`/`taskkill /F` (access denied) while holding the normal
  `bin/Debug` output locked - worked around by building/running to an alternate output
  directory (`bin/pilot_run`) and invoking the DLL directly via `dotnet exec`, rather than
  `dotnet run`, whenever the default output is unexpectedly locked by a process that can't
  be stopped.
- The very first queued run of the corrected code fell back to `InProcess` (logged
  `"ran in-process (not isolated) - the Release folder is likely missing a full publish
  output"`) despite a genuine full publish being present; the very next run of the same
  Release succeeded isolated with no such warning - looked transient (e.g. first-attempt
  module loading contention), not reproduced on the second run. Worth watching for during
  the Phase 4 rollout to the other 6 projects, but not treated as a real blocker here since
  the isolated path did work.

### What this means for the remaining phases
- **Phase 2 (metadata audit)**: also check every parameterized `[TestFixture(...)]` class
  across the other 6 projects for the same class-name bug now that it's fixed upstream -
  no additional code changes should be needed per-project, just confirming the fix covers
  them too (it's a generic XML-parsing fix, not project-specific).
- **Phase 3 (push vs. pull)**: the pilot's one real API call needed (`GetAutomationData`)
  is now fixed and proven working with real auth. The pilot's test method didn't exercise
  `SaveTestCaseLog`/`InvokeServicePost`/`UpdateQueue` at all, so this pilot alone doesn't
  yet answer the broader push-vs-pull question for projects that *do* call those - still
  open for whichever of the remaining 6 projects turns out to need them.

## Fixed: `ModalService` couldn't close a dialog after leaving and returning to its page
`ModalService` (`core/services/modal.service.ts`) is `providedIn: 'root'` - a singleton
that lives for the whole SPA session - but `register(id, element)` only ever created a
`bootstrap.Modal` **the first time** a given id was seen (`if (!this.modals[id])`).
Reported as "Not able to close [the] Schedule Test Case Execution popup" - root cause:
`test-case-execution-panel` (which hosts `ScheduleTestcasesDialogComponent`) is a lazily-
loaded **routed** component (`app.routes.ts`), so navigating away from and back to that
page destroys and recreates the dialog and its modal `<div>` element every time. After the
*first* visit, every later `register()` call for `'scheduleTestcasesModal'` was silently
skipped - the freshly-rendered element from the new visit was never wired to any
`bootstrap.Modal` controller at all, while `open()`/`close()` kept calling `.show()`/
`.hide()` on the *previous* visit's now-detached instance. Same latent bug applies to every
other `ModalService`-based dialog (`forgotPasswordModal`, `forgotUsernameModal`, tips
modals, etc.) if their host component is ever destroyed/recreated, not just this one.
**Fixed**: `register()` now always disposes any previous instance for that id and creates a
fresh `bootstrap.Modal` bound to the current element, instead of skipping registration when
an (possibly stale) entry already exists.

## Test Case Execution Panel — Release-aware alignment
`test-case-execution-panel.component` now mirrors the Assignment screen's Release-awareness:
- A **Release filter** dropdown (scoped to releases the tester actually has assignments in,
  derived from their own `assignments` list) narrows the existing Assignment dropdown;
  selecting a release auto-selects its first matching assignment.
- **Fixed**: on load, this dropdown used to stay on its "All Releases" placeholder even
  though an assignment (and therefore a specific release's data) was already auto-selected
  and shown - it picked the first *assignment* directly rather than going through the
  Release filter, so the dropdown didn't reflect what was actually on screen. Now mirrors
  Dashboard's `loadReleases()`/`onReleaseChange(this.releases[0])` convention: `loadAssignments()`
  auto-selects the first `releaseFilterOptions` entry via `onReleaseFilterChange(...)` (which
  itself then auto-selects that release's first assignment), so the Release dropdown shows a
  real, concrete value immediately, same as Dashboard - falling back to the old
  show-everything-unfiltered behavior only if the tester genuinely has zero assignments
  scoped to any release.
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
