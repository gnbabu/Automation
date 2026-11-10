# OH|ID login with automatic TOTP, plus new Member Eligibility test cases

Automate the OH|ID single sign-on login end to end with no human interaction. The test generates the two-step code from the automation account's TOTP authenticator key, which is stored encrypted on the Login User next to the password and entered in the Portal's Credential Configuration. TC.MemberEligibilitySearch gains positive, field-validation and OH|ID end-to-end test cases.

## 1. Goal and scope
- **Goal:** run tests against an OH|ID (SSO) environment such as PROD with **no human interaction**: "Log in with OH|ID" → username/password → two-step verification (**TOTP Authenticator**) → PNM dashboard.
- **OTP source:** the TOTP key of a **dedicated, approved automation OH|ID account**. The test computes the 6-digit code (RFC 6238), exactly as an authenticator app would.
- **Key storage:** **encrypted on the Login User**, next to the password, and entered in the Portal (Credential Configuration). This needs one database column.
- **New eligibility test cases:** positive searches, field validations, OH|ID login + search.
- **No email, SMS or DUO.**

## 2. Current state (verified in code)
| Area | Today | Gap |
|---|---|---|
| PROD environment (Id 11) | `.../OH_PNM_PROD/Account/Login.aspx`, `RequiresAuthentication=1`, `EnableSso=0` | The page only offers "Log in with OH\|ID" |
| `BaseFeatureFixture.ResolveCredentialsAndUrlAsync` | `EnableSso=true` means skip login and only navigate | SSO environments end on the PNM login page |
| `LoginService` | Automates the old PNM form | No OH\|ID flow. **Security bug:** the 2-factor exception (lines 66–69) contains the **username and password** |
| `aut.LoginUser` | `UserName`, `EncryptedPassword` (`CredentialCipher`) | No TOTP key |
| `GET api/LoginUser/{id}/credentials`, `resolve` | TestRunner token only; `LoginUserId`, `UserName`, `Password` | No TOTP key |
| Portal Credential Configuration | Username, password ("leave blank to keep"), role | No TOTP field |
| Eligibility tests | TC_ELIG_01 only | No strict-positive, validation or OH\|ID tests |

## 3. Design
### 3.1 TOTP key storage
- **Database:** new column `aut.LoginUser.EncryptedOtpSecret NVARCHAR(500) NULL`, encrypted with the existing `CredentialCipher`, exactly like `EncryptedPassword`.
- **Portal:** Credential Configuration → Login User form gains **"Authenticator (TOTP) key"**:
  - A password-type input, "Leave blank to keep current key".
  - A **Remove key** checkbox.
  - Help text: "Paste the setup key shown when the authenticator was added to this OH|ID account."
- **List view:** shows a **"2-step: TOTP"** badge when a key is saved. The key itself is never shown again.
- **API:** saves it encrypted; returns it **only** to the test runner, through the existing TestRunner-only credentials endpoints, together with the password. Never logged; audited as "TOTP key set/removed" without the value.

### 3.2 Login flow (`OhidLoginService`)
1. Open the environment URL, click **"Log in with OH|ID"**, and switch window if one opens.
2. **OH|ID login** (`auth.ohid.ohio.gov`): username, password, **Log in**. On an OH|ID error message, fail with that message (no secrets).
3. **Choose a method** (`ohid.verify.ohio.gov`): **Authenticator app → TOTP Authenticator → Enter code**, not the DUO TOTP row. If it's missing: "No authenticator registered on this OH|ID account".
4. **Code:** `TotpOtpProvider.WaitForOtpAsync()` returns the current code, waiting for the next 30-second window if fewer than 5 seconds remain. Type it and click **Verify**.
   - **Rejected:** retry **once** with the next window.
   - **Lockout or "too many attempts":** fail immediately with no retry.
5. **Back in PNM:** wait for the environment host, then check the **dashboard** (menu button) and the **signed-in user** (Logout link / user name).
6. **Step logs without values:** "Opened OH|ID login", "Entered OH|ID credentials", "Chose TOTP Authenticator", "Entered one-time code", "Signed in to PNM".

### 3.3 When it's used (`BaseFeatureFixture`)
| Environment | Login user | Behaviour |
|---|---|---|
| `EnableSso=false` | any | Unchanged (old PNM form login) |
| `EnableSso=true` | password + TOTP key saved | **OH\|ID login (new)** |
| `EnableSso=true` | no TOTP key saved | Fails before login: "Add the Authenticator (TOTP) key to this login user in Credential Configuration" |
| `EnableSso=true` | none selected | Skips login as today |

### 3.4 Keeping secrets out of logs and screenshots
- Passwords, codes and TOTP keys are never written to logs, exceptions or the console.
- No step screenshots on OH|ID domains.
- Inputs are cleared before any failure screenshot on an OH|ID page.
- The `LoginService` 2-factor exception is fixed so it no longer exposes the username and password.

### 3.5 OTP interface (as requested)
`IOtpProvider`:
- `GetLatestOtpAsync()`
- `WaitForOtpAsync(TimeSpan timeout)`
- `ExtractOtp(string message)` (6–8 digits; for future providers)

`TotpOtpProvider` implements RFC 6238 (Base32, HMAC-SHA1, 30-second step, 6 digits) in plain .NET, with no new package.

## 4. Implementation steps
### Step 0 — Commit this plan
Copy this plan into the repo as `docs/plans/OHID-TOTP-Login-Plan.md` and commit it ("Add plan for OH|ID login with automatic TOTP").

### Step 1 — Database (`Database/LoginUser_OtpSecret_Migration.sql`, idempotent)
- Add `aut.LoginUser.EncryptedOtpSecret NVARCHAR(500) NULL`.
- **`usp_LoginUserCreate`:** add optional `@EncryptedOtpSecret`.
- **`usp_LoginUserUpdate`:** add optional `@EncryptedOtpSecret` (null keeps the current value) and `@ClearOtpSecret BIT = 0`.
- **`usp_LoginUserGetCredentials`, `usp_LoginUserResolveByRole`:** also return `EncryptedOtpSecret`.
- **`usp_LoginUserGetByEnvironment`, `usp_LoginUserGetById`, `usp_LoginUserGetByEnvironmentAndPortalUser`:** return `HasOtpSecret BIT` only.

### Step 2 — API
- **`Repositories/Models/LoginUserModel.cs`:**
  - `LoginUserRequestDto` gains `OtpSecret`, `ClearOtpSecret`.
  - `LoginUserModel` gains `HasOtpSecret`.
  - `LoginUserCredentials` gains `OtpSecret`.
- **`Repositories/LoginUserRepository.cs`:**
  - Encrypt on create/update (normalised: spaces removed, upper-case; Base32 validated). Pass the clear flag.
  - Decrypt `OtpSecret` in `GetCredentialsAsync`/`ResolveByRoleAsync`.
  - Map `HasOtpSecret` in the list and by-ID reads.
- **`Controllers/LoginUserController.cs`:**
  - Reject an invalid Base32 key with 400 "That isn't a valid authenticator key".
  - Audit "TOTP key set" / "TOTP key removed" without the value. The TestRunner-only checks are unchanged.
- **Unit tests** (`AutomationAPI.Tests/LoginUserOtpTests.cs`): Base32 validation and normalisation; a Portal token can't read credentials.

### Step 3 — Portal (Credential Configuration)
- **`core/interfaces/index.ts`:** login user interfaces gain `hasOtpSecret`; the request gains `otpSecret`, `clearOtpSecret`.
- **`pages/credential-configuration/credential-configuration.component.html/.ts`:**
  - The "Authenticator (TOTP) key" input with help text, and the Remove key checkbox (shown only when editing a user that has a key).
  - The "2-step: TOTP" badge in the list.
  - Clear the field after saving.

### Step 4 — Test framework: OTP provider
- New `Selenium.BaseComponents/Services/Otp/IOtpProvider.cs` and `TotpOtpProvider.cs`.
- Change `Selenium.BaseComponents/Utilities/EnvironmentDetails.cs`: `LoginUserCredentials.OtpSecret`.
- **New project `AutomationTests/Selenium.BaseComponents.Tests`** (NUnit, in `AutomationTests.sln`): RFC 6238 vectors (SHA1 at times 59, 1111111109, 1234567890, 2000000000), Base32 (spaces and lower case), the window-edge wait, `ExtractOtp`.

### Step 5 — Test framework: OH|ID page objects
New `Selenium.BaseComponents/Pages/Ohid/`, using label, placeholder and visible-text locators with fallbacks:
- `PnmSsoLoginPage`: "Log in with OH|ID".
- `OhidLoginPage`: "OHID username", "Password", "Log in", error text.
- `OhidTwoStepPage`: "Choose a method"; the "Enter code" link in the **TOTP Authenticator** row under "Authenticator app".
- `OhidOtpPage`: code input, "Verify", error text.
- `PnmShell`: dashboard menu button; signed-in user / Logout (`ctl00_LoginView2_lnkLogout`).

### Step 6 — Test framework: `OhidLoginService` and security fix
- New `Selenium.BaseComponents/Services/OhidLoginService.cs` implementing section 3.2 (stale-safe actions, window switching, failures naming the step).
- `Services/LoginService.cs`: remove the username and password from the 2-factor exception message.

### Step 7 — Test framework: `BaseFeatureFixture`
- `ResolveCredentialsAndUrlAsync`: for `EnableSso`, still fetch the login user's credentials, including `OtpSecret`, and mark the run as SSO.
- `NavigateAndLoginIfNeeded`:
  - SSO with credentials → `OhidLoginService`.
  - SSO without a login user → `NavigateOnly`.
  - Non-SSO → unchanged.
- Failure-screenshot helper: skip or clear inputs on OH|ID domains.
- Update the `EnvironmentDetails.EnableSso` description.

### Step 8 — Eligibility test cases (`TC.SearchEligibility`, project `TC.MemberEligibilitySearch`)
- **Shared steps:** new `Pages/EligibilitySearchSteps.cs` (Self Service → provider → Search Eligibility; fill; search; read the result: Found / NoRecords / Error / Validation). `SearchMemberEligibilityTest` is refactored onto it, with TC_ELIG_01 behaviour unchanged.
- **Validation message:** `Pages/MemberEligibilityPage.cs` gains `ValidationMessage`.
- **Positive tests** (`TestDataModels/MemberEligibilitySearch.cs` gains a `SsnSearch` section):

  | ID | Passes when |
  |---|---|
  | TC_ELIG_01 (existing) | Any completed search, including "No records returned" |
  | TC_ELIG_02 Member found | Recipient information shows the MBN and the plans section is shown; "No records" fails |
  | TC_ELIG_03 Search by SSN | Recipient information is shown |

- **Field validations:** new `Tests/MemberEligibilityValidationTest.cs` and `TestDataModels/MemberEligibilityValidation.cs`, with one section per scenario holding the inputs plus `ExpectedMessage` (matched with "contains").
  - Each test checks that the message appears and that no recipient information is shown.
  - An empty `ExpectedMessage` fails with "Actual message: '…'".

  | ID | Scenario |
  |---|---|
  | TC_ELIG_10 | Missing Medicaid Billing Number and SSN |
  | TC_ELIG_11 | Missing Date of Birth |
  | TC_ELIG_12 | Invalid date format |
  | TC_ELIG_13 | From DOS after To DOS |
  | TC_ELIG_14 | DOS outside the allowed range |

- **OH|ID end to end:** new `Tests/OhidEligibilitySearchTest.cs`. **TC_ELIG_20:** the SSO login reaches the dashboard and the user is confirmed, then the search completes as in TC_ELIG_01. On a non-SSO environment it fails with a clear message.
- **PROD-safe logging:** in SSO environments, eligibility tests log only the outcome (found / no records / plan count) and take no result screenshots.
- **Metadata:** `[TestDataFlow]` and `[TestDataNote]` on each class.

### Step 9 — Build, deploy and docs
- Build the API, Portal, AutomationTests solution and new test project; run all unit tests.
- Apply the migration. Deploy the DLLs to REL-55 and assign TC_ELIG_02/03/10–14/20 in the Portal.
- Update `AGENTS.md`. Commit.

## 5. Prerequisites (outside the code)
1. **Approval** for automated OH|ID login on PROD with a dedicated automation account (ODM / OH|ID security).
2. A **dedicated automation OH|ID account** with PNM eligibility access (not a personal account).
3. **TOTP key:**
   - If OH|ID says "Maximum TOTP devices registered", **remove the existing authenticator** in the account's two-step settings (or ask the OH|ID help desk to reset it).
   - Then **Add authenticator app → "Can't scan?"** → copy the key. Add it to a phone app as a backup and confirm with a code.
4. **Portal:** PROD **Enable SSO** on. Add or edit the PROD login user with the OH|ID username, password and **Authenticator (TOTP) key**.
5. The **test server's clock** is synchronised.
6. A **decision on patient data (PHI)** in PROD logs (PROD-safe logging recommended).

## 6. Verification
- [ ] Unit tests: TOTP RFC vectors, Base32, window edge, `ExtractOtp`; API key validation and access rules.
- [ ] API, Portal, test solution and new test project build; existing unit tests pass.
- [ ] Migration runs twice cleanly.
- [ ] Saving a key in the Portal stores only ciphertext and shows the badge. The credentials endpoint returns the key only to the TestRunner token.
- [ ] One supervised OH|ID pass to confirm the locators.
- [ ] PROD, supervised: TC_ELIG_20 completes with no human input; no password, code, key or patient details in logs or screenshots.
- [ ] E2E: TC_ELIG_01/02/03 pass; the TC_ELIG_10–14 messages are captured into Test Data Management and re-run until they pass.
- [ ] Regression: a non-SSO test (TC_PA_PROF_01) still uses the old login; an SSO environment without a login user behaves as before.

## 7. Risks
| Risk | Mitigation |
|---|---|
| Account lockout | One retry; none on lockout; infrequent PROD schedules; the account lock prevents concurrent logins |
| Clock drift | Windows time sync; window-edge wait; retry with the next window |
| OH\|ID page changes | Label-based locators with fallbacks; failures name the step |
| Key exposure | Encrypted at rest; never shown, logged or screenshotted; rotate by removing and re-adding the authenticator in OH\|ID and saving the new key in the Portal |
| Patient data on PROD | PROD-safe logging; no result screenshots |

## 8. Task list
**Prerequisites (business and admin)**
- P1. Get approval for automated OH|ID login on PROD (ODM / OH|ID security)
- P2. Create the dedicated automation OH|ID account with PNM eligibility access
- P3. Free the authenticator slot (remove the existing TOTP device or ask the help desk to reset it), add a new authenticator and copy its key
- P4. Portal: turn PROD Enable SSO on; save the PROD login user with username, password and TOTP key
- P5. Confirm the test server's time sync
- P6. Decide on patient data in PROD logs and screenshots

**Development**
- D0. Commit this plan to `docs/plans/OHID-TOTP-Login-Plan.md`
- D1. DB migration: `EncryptedOtpSecret` column and procedure updates
- D2. API: DTOs, repository encryption/decryption, validation, audit, unit tests
- D3. Portal: TOTP key field, Remove option and badge in Credential Configuration
- D4. Test framework: `IOtpProvider` + `TotpOtpProvider` and the `Selenium.BaseComponents.Tests` project
- D5. Test framework: OH|ID page objects
- D6. Test framework: `OhidLoginService`, plus the `LoginService` password-leak fix
- D7. Test framework: SSO login in `BaseFeatureFixture`; no secrets in screenshots
- D8. Eligibility: shared steps and validation-message locator; refactor TC_ELIG_01
- D9. Eligibility: TC_ELIG_02 and TC_ELIG_03
- D10. Eligibility: TC_ELIG_10–14 validations
- D11. Eligibility: TC_ELIG_20 and PROD-safe logging
- D12. Build everything, run unit tests, update `AGENTS.md`, commit

**Verification and rollout**
- V1. Apply the migration and check that saving a key stores only ciphertext
- V2. Supervised OH|ID locator check and fixes
- V3. Deploy to REL-55 and assign the new test cases
- V4. E2E eligibility runs and the validation-message capture
- V5. Supervised PROD run of TC_ELIG_20
- V6. Regression checks
