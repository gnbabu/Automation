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
