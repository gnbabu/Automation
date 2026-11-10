# OH|ID login with automatic TOTP (separate, opt-in), plus new Member Eligibility test cases

Log in through OH|ID with no human interaction. The test calculates the two-step code from the automation account's authenticator (TOTP) key, read from the git-ignored `appSettings.Local.json` in Selenium.BaseComponents (copied next to every test DLL). The OH|ID login is new, separate code used only by tests that opt in. The existing login, the database, the Portal and the API are unchanged. TC.MemberEligibilitySearch also gains positive, field-validation and OH|ID end-to-end test cases.

## 1. Goal and scope
- **Goal:** run tests against OH|ID (SSO) environments such as PROD with **no human interaction**: "Log in with OH|ID" → username/password → two-step verification (**TOTP Authenticator → Enter code**) → PNM dashboard.
- **OTP source:** the authenticator key of the **dedicated automation OH|ID account**. The test calculates the 6-digit code (RFC 6238), exactly as an authenticator app does.
- **Key storage:** the test project's **`appSettings.Local.json`**, which is git-ignored. The committed `appSettings.json` has only an empty placeholder.
- **Separate and opt-in:** only test classes that inherit the new `OhidFeatureFixture` use the OH|ID login (starting with TC_ELIG_20). `BaseFeatureFixture`, `LoginService`, `APIGatway`, `SettingsReader` and every existing test are unchanged.
- **No changes to:** the database, the Portal or the API. No email, SMS or DUO.

## 2. Current state (verified in code)
| Area | Today |
|---|---|
| PROD environment (Id 11) | `https://ohpnm.omes.maximus.com/OH_PNM_PROD/Account/Login.aspx`; the page only offers "Log in with OH\|ID" |
| `BaseFeatureFixture` | `InitializeTestSuite()` is `public virtual` (`[OneTimeSetUp]`), so a subclass can replace the login. `TestWebDriver`, `AssignmentId`/`AssignmentTestCaseId`, `WebDriverService`, `APIGateway` are accessible to subclasses |
| Credentials | `APIGateway.GetLoginUserCredentials()` returns the username/password of the login user picked in Run Now (TestRunner token only) |
| Environment URL | `APIGateway.GetEnvironmentDetails()` |
| Settings | `SettingsReader` reads only `appSettings.json` from the output folder; `Selenium.BaseComponents/appSettings.json` is tracked in Git |
| Eligibility tests | TC_ELIG_01 only (`SearchMemberEligibilityTest`, model `MemberEligibilitySearch`, `MemberEligibilityPage`) |

## 3. Design
### 3.1 TOTP key
- **Committed placeholder:** `Selenium.BaseComponents/appSettings.json` gets `"Otp": { "TotpKey": "" }`.
- **Real key:** `Selenium.BaseComponents/appSettings.Local.json`, **git-ignored**, with `{ "Otp": { "TotpKey": "<setup key>" } }`. It's copied to the build output when present.
- **Reading it:** the new `OhidSettings` reads `appSettings.Local.json` first, then `appSettings.json`, from the test's output folder. Spaces are removed and the key upper-cased; it's validated as Base32.
- **Releases:** deploy `appSettings.Local.json` to the release folder (e.g. REL-55) alongside the DLLs.
- **Missing key:** fails before login with "OH|ID TOTP key not configured: add Otp:TotpKey to appSettings.Local.json".
- **Never** logged, put in exception messages or committed.
- **Rotation:** the key used for trials was shared in a screenshot, so replace it before regular use. In OH|ID, remove the authenticator, add a new one, put the new key in `appSettings.Local.json`, and add it to the phone backup.

### 3.2 Login flow (`OhidLoginService`)
The way into OH|ID differs by environment (confirmed from screenshots and the TestCase Studio recording `ohpnm-e2e_omes_maximus_2026-11-11_00-07-48.spec.js`):

| Environment | PNM page | Way into OH\|ID |
|---|---|---|
| PROD | `.../OH_PNM_PROD/Account/Login.aspx` shows only **"Log in with OH\|ID"** | Click that button |
| E2E | `.../OH_PNM_E2E/Account/Login.aspx` shows the old User ID box | Type the user ID in `#ctl00_MainContent_Login1_UserName` → **Next** `#ctl00_MainContent_Login1_btnNext` → **Go to IOP** `#ctl00_MainContent_Login1_btnGotoIOP` |

1. **Open the environment URL** and detect the page:
   - **"Log in with OH|ID" button present** → click it.
   - **User ID box present** → type the login user's username → **Next** → wait for **Go to IOP** (`btnGotoIOP`) → click it.
   - Switch window if one opens.
2. **OHID login** (`auth.ohid.ohio.gov/login`), **only if shown**: OHID username, Password, **Log in**. The page is skipped when the browser session is already signed in (as in the recording, which went straight to two-step verification). On an OH|ID error, fail with that message (no secrets).
3. **Choose a method** (`ohid.verify.ohio.gov`):
   - Recognised by the **"Two-step verification"** heading (`p.bx--type-delta`).
   - Click the TOTP authenticator option. E2E labels it **"TOTP AuthApp"** (`//div[normalize-space()='TOTP AuthApp']`); PROD labels it **"TOTP Authenticator"** with an **Enter code** link.
   - The locator matches either label under the Authenticator app section and never the DUO TOTP row.
   - If neither is present: "No authenticator registered on this OH|ID account".
4. **Code:** `TotpOtpProvider` calculates the current code, waiting for the next 30-second window if fewer than 5 seconds remain. It types the code and clicks **Verify**.
   - **Rejected:** retry **once** with the next window.
   - **Lockout or "too many attempts":** fail immediately with no retry.
5. **Back in PNM:** the URL is on the PNM host and the Logout link (`ctl00_LoginView2_lnkLogout`) / menu is visible.
6. **Step logs without values:** "Opened OH|ID login", "Entered OH|ID credentials", "Chose TOTP Authenticator", "Entered one-time code", "Signed in to PNM".

### 3.3 Opt-in fixture (`OhidFeatureFixture : BaseFeatureFixture`)
- Overrides `[OneTimeSetUp] InitializeTestSuite()`:
  1. Sets `AssignmentId`/`AssignmentTestCaseId`.
  2. Gets the environment URL and the login user's credentials through `APIGateway`, failing clearly if either is missing.
  3. Reads the TOTP key.
  4. Creates Chrome or Edge from `TestContext.Parameters["Browser"]`.
  5. Runs `OhidLoginService.Login(...)`.
- The base `[TearDown]`/`[OneTimeTearDown]` (failure log, screenshot, browser quit) keep working.
- `EnableSso` and other environment flags aren't used or changed. A test opts in by inheriting this class.

### 3.4 Keeping secrets out of logs and screenshots
- Passwords, codes and the key are never written to logs, exceptions or the console.
- No step screenshots on OH|ID pages.
- On any login failure, the password and code inputs are cleared before the exception reaches the base class's failure screenshot.

### 3.5 OTP interface (as requested)
`IOtpProvider`:
- `GetLatestOtpAsync()`
- `WaitForOtpAsync(TimeSpan timeout)`
- `ExtractOtp(string message)` (6–8 digits; for future providers)

`TotpOtpProvider` implements RFC 6238 (Base32 decode, HMAC-SHA1, 30-second step, 6 digits) in plain .NET, with no new package.

## 4. Implementation steps
### Step 1 — OH|ID code (new files in `AutomationTests/Selenium.BaseComponents/Ohid/`)
- `IOtpProvider.cs`, `TotpOtpProvider.cs`
- `OhidSettings.cs`: reads `Otp:TotpKey` from `appSettings.Local.json` → `appSettings.json`
- `OhidPages.cs`: locators by ID where recorded, otherwise label and visible text:
  - PROD: "Log in with OH|ID"
  - E2E: `#ctl00_MainContent_Login1_UserName`, `#ctl00_MainContent_Login1_btnNext`, `#ctl00_MainContent_Login1_btnGotoIOP`
  - OHID username, Password, Log in
  - Two-step page: `p.bx--type-delta` / "Two-step verification"; TOTP option `//div[normalize-space()='TOTP AuthApp']` (E2E) or the "TOTP Authenticator" row's "Enter code" (PROD), excluding DUO
  - Code input, Verify (**not recorded yet**; confirm on the first supervised run)
  - Error text
  - PNM Logout / menu
- `OhidLoginService.cs`: section 3.2, with stale-safe actions, window switching and failures naming the step
- `OhidFeatureFixture.cs`: section 3.3

### Step 2 — Key file wiring
- `Selenium.BaseComponents/appSettings.json`: add the empty `Otp:TotpKey` placeholder.
- `Selenium.BaseComponents/Selenium.BaseComponents.csproj`: copy `appSettings.Local.json` to the output **if it exists**.
- `.gitignore`: add `**/appSettings.Local.json`.

### Step 3 — Unit tests (new project `AutomationTests/Selenium.BaseComponents.Tests`, in `AutomationTests.sln`)
- RFC 6238 test vectors (SHA1 at times 59, 1111111109, 1234567890, 2000000000).
- Base32 parsing (spaces, lower case, invalid characters).
- The window-edge wait.
- `ExtractOtp`.
- `OhidSettings` precedence (Local file wins; missing key gives a clear error).

### Step 4 — Eligibility test cases (`TC.SearchEligibility`, project `TC.MemberEligibilitySearch`)
- **Shared steps:** new `Pages/EligibilitySearchSteps.cs` (Self Service → provider → Search Eligibility; fill; search; result: Found / NoRecords / Error / Validation), used by the **new** test classes. The existing `SearchMemberEligibilityTest` (TC_ELIG_01) is left as it is.
- **Validation message:** `Pages/MemberEligibilityPage.cs` gains a `ValidationMessage` locator (additive).
- **Positive tests:** new `Tests/MemberEligibilityPositiveTest.cs`, model `MemberEligibilitySearch` (gains a `SsnSearch` section; existing sections are unchanged):

  | ID | Passes when |
  |---|---|
  | TC_ELIG_02 Member found | Recipient information shows the MBN and the plans section is shown; "No records" fails |
  | TC_ELIG_03 Search by SSN | Recipient information is shown |

- **Field validations:** new `Tests/MemberEligibilityValidationTest.cs`, new model `TestDataModels/MemberEligibilityValidation.cs`, with one section per scenario holding the inputs plus `ExpectedMessage` (matched with "contains").
  - Each test checks that the message appears and no recipient information is shown.
  - An empty `ExpectedMessage` fails with "Actual message: '…'", so the first run reveals the real texts.

  | ID | Scenario |
  |---|---|
  | TC_ELIG_10 | Missing Medicaid Billing Number and SSN |
  | TC_ELIG_11 | Missing Date of Birth |
  | TC_ELIG_12 | Invalid date format |
  | TC_ELIG_13 | From DOS after To DOS |
  | TC_ELIG_14 | DOS outside the allowed range |

- **OH|ID end to end:** new `Tests/OhidEligibilitySearchTest.cs` inherits **`OhidFeatureFixture`**. **TC_ELIG_20:** OH|ID login with TOTP → dashboard and user check → eligibility search as in TC_ELIG_01.
- **PROD-safe logging:** TC_ELIG_20 logs only the outcome (found / no records / plans section shown) and takes **no** result screenshots.
- **Metadata:** `[TestDataFlow]` and `[TestDataNote]` on each new class.

### Step 5 — Build, deploy and docs
- Build the AutomationTests solution and run the new unit tests.
- Deploy `TC.MemberEligibilitySearch.dll`, `Selenium.BaseComponents.dll`, `TestDataModels.dll` and `appSettings.Local.json` to REL-55. Assign TC_ELIG_02/03/10–14/20 in the Portal.
- `AGENTS.md`: an "OH|ID login with TOTP (opt-in)" section covering the key file, opting in and the security rules.
- Commit (the key file is never committed).

## 5. Prerequisites (outside the code)
1. **Approval** for automated OH|ID login on PROD with the dedicated automation account.
2. The **automation OH|ID account** with PNM eligibility access (not a personal account).
3. **Authenticator registered on that account:**
   - Finish "Set up two-step verification → Next: Test your authenticator" with a code from a phone app.
   - The device then appears under "Manage your registered devices".
4. **Key:** saved in `Selenium.BaseComponents/appSettings.Local.json` locally, and in the release folder for Portal runs. Replace the trial key before regular use (section 3.1).
5. **Portal:** a PROD login user with the OH|ID username and password in Credential Configuration, picked in Run Now for TC_ELIG_20.
6. **Clock:** the machine running the tests has its clock synchronised (Windows time sync).
7. **Patient data:** decide whether PROD eligibility results may appear in automation logs and screenshots (outcome-only logging recommended).

## 6. Verification
- [ ] `git diff --stat` shows only new files plus the placeholder, csproj copy rule, `.gitignore`, the additive `ValidationMessage` locator, the `SsnSearch` model section, `AGENTS.md` and this plan. `appSettings.Local.json` is **not** listed.
- [ ] Unit tests pass (RFC vectors, Base32, window edge, `ExtractOtp`, settings precedence). AutomationTests solution builds.
- [ ] One supervised OH|ID run **on E2E** (User ID → Next → Go to IOP → OHID login if shown → TOTP AuthApp → code → Verify) and **on PROD** (Log in with OH|ID → …) to confirm the locators. The code-entry page and Verify button weren't in the recording.
- [ ] PROD, supervised: TC_ELIG_20 completes with no human input. No password, code, key or patient details appear in logs or screenshots.
- [ ] E2E: TC_ELIG_02/03 pass. The TC_ELIG_10–14 messages are captured into Test Data Management and re-run until they pass.
- [ ] Regression: TC_ELIG_01 and a non-OH|ID test (e.g. TC_PA_PROF_01) still log in exactly as before.

## 7. Risks
| Risk | Mitigation |
|---|---|
| Account lockout | One retry; none on lockout; infrequent PROD runs; the account lock prevents concurrent logins |
| Clock drift | Windows time sync; window-edge wait; retry with the next window |
| OH\|ID page changes | Label-based locators with fallbacks; failures name the step |
| Key in plain text in release folders | Git-ignored; never logged; restrict release folder access; rotate by re-adding the authenticator |
| Trial key already exposed (screenshot) | Replace it before regular use |
| Patient data on PROD | Outcome-only logging; no result screenshots in TC_ELIG_20 |

## 8. Task list
**Prerequisites**
- P1. Get approval for automated OH|ID login on PROD
- P2. Set up the dedicated automation OH|ID account with PNM eligibility access
- P3. Finish the authenticator registration and confirm it's listed under registered devices
- P4. Save the key in `appSettings.Local.json` (locally and in the release folder); replace the trial key before regular use
- P5. Portal: PROD login user (OH|ID username and password) in Credential Configuration
- P6. Confirm the test machine's time sync
- P7. Decide on patient data in PROD logs and screenshots

**Development**
- D1. `Ohid/`: `IOtpProvider`, `TotpOtpProvider`, `OhidSettings`
- D2. `Ohid/`: `OhidPages`, `OhidLoginService`
- D3. `Ohid/`: `OhidFeatureFixture`
- D4. Key file wiring: placeholder, csproj copy rule, `.gitignore`
- D5. New `Selenium.BaseComponents.Tests` project with the unit tests
- D6. Eligibility: `EligibilitySearchSteps`, `ValidationMessage` locator, `SsnSearch` section
- D7. Eligibility: TC_ELIG_02 and TC_ELIG_03
- D8. Eligibility: `MemberEligibilityValidation` model and TC_ELIG_10–14
- D9. Eligibility: TC_ELIG_20 (opt-in OH|ID) with outcome-only logging
- D10. Build, unit tests, `AGENTS.md`, commit

**Verification and rollout**
- V1. Supervised OH|ID locator check and fixes
- V2. Deploy to REL-55 (including `appSettings.Local.json`) and assign the new test cases
- V3. E2E eligibility runs and capture of the validation messages
- V4. Supervised PROD run of TC_ELIG_20
- V5. Regression checks
