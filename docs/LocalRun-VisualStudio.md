# Running test cases from Visual Studio (local run)

Test cases normally run from the Automation Portal (Run Now / Schedule / Recurring). For debugging, the same test cases can be run straight from Visual Studio Test Explorer:

- **Data:** all data comes from JSON files in the test project, not from the Portal or database.
- **No API needed:** the Automation API doesn't need to be running.
- **Nothing written:** nothing is written to the Automation Portal database (no logs, screenshots, queue rows or status).
- **Portal runs:** completely unchanged.

Currently enabled for **TC.MemberEligibilitySearch** (`AutomationTests/TC.SearchEligibility`).

---

## 1. How it works

| | Portal run | Visual Studio run (local) |
|---|---|---|
| Started by | Portal → API queue (`TestQueueWorker`) | Test Explorer / `dotnet test` |
| Detected by | The Portal passes an `AccessToken` test parameter | No `AccessToken` **and** a `localrun.json` next to the test DLLs |
| Environment URL, login user | Automation API (Environment / Login Users) | `localrun.json` |
| Test data (`TestData.Load<T>()`) | Test Data Management (API) | `LocalRunData/<TestCaseId>.json` |
| Legacy data (`DataRepository.GetAutomationData`) | Automation API | `LocalRunData/<TestCaseId>.json` (array format) |
| Step logs | Saved to the Portal (`aut.TestCaseExecutionLogs`) | Written to the Test Explorer output window |
| Screenshots | Saved to the Portal (`aut.TestScreenshots`) | Saved to `bin\Debug\net8.0\LocalRunResults\<TestCaseId>\<date_time>\` |
| Test case status / dashboard | Updated by the API | Not touched |

**Where the switch is:** everything is decided in `Selenium.BaseComponents/Utilities/LocalRun.cs`. `APIGatway`, the only class that talks to the Automation API, checks `LocalRun.IsActive` first in every method.

**Portal runs are never affected:** they always carry an `AccessToken`, so they never take the local path, even if `localrun.json` is present in a release folder.

---

## 2. Files in the test project

```
AutomationTests/TC.SearchEligibility/
├── localrun.example.json          template (committed, no passwords)
├── localrun.json                  your local settings and passwords (NOT committed, see section 3)
└── LocalRunData/                  test data, one file per test case (committed)
    ├── TC_ELIG_01.json            SearchMemberEligibilityTest
    ├── TC_ELIG_20.json            OhidEligibilitySearchTest
    └── TCSearchEligibility.json   legacy SearchEligiblityTest (array of automation-data rows)
```

The project file copies `localrun.json` (when present) and `LocalRunData\*.json` to the build output on every build.

---

## 3. One-time setup on your machine

1. **Create `localrun.json`:** copy `localrun.example.json` to `localrun.json` in the same folder.
2. **Fill in the values:**

```json
{
  "Browser": "Chrome",
  "Environment": {
    "EnvironmentName": "E2EP3",
    "EnvironmentUrl": "https://ohpnm-e2e.omes.maximus.com/OH_PNM_E2E/Account/Login.aspx",
    "RequiresAuthentication": true,
    "EnableSso": false
  },
  "LoginUser": { "UserName": "autotechadmin", "Password": "<password>" },
  "LoginUsersByRole": { },
  "Otp": {
    "TotpKey": "<OH|ID authenticator key, only for OH|ID tests>",
    "PnmUserId": "123",
    "OhidUserName": "<OH|ID username>",
    "OhidPassword": "<OH|ID password>"
  }
}
```

| Setting | Used for |
|---|---|
| `Browser` | `Chrome` or `Edge` |
| `Environment` | The same values as the environment in the Portal's Environment Management. `RequiresAuthentication=false` or `EnableSso=true` skips the PNM login form |
| `LoginUser` | The PNM login used by `BaseFeatureFixture` (same as the Login User picked in Run Now) |
| `LoginUsersByRole` | Only for tests that switch role mid-test with `LoginByProfile("<Role>")`, e.g. `{ "StateAdmin": { "UserName": "...", "Password": "..." } }` |
| `Otp` | Only for OH\|ID tests (`OhidFeatureFixture`, e.g. TC_ELIG_20): authenticator key, PNM user ID for E2E's "Go to IOP" step, OH\|ID username and password |

3. **Keep `localrun.json` out of Git:** it contains passwords. It's listed in `.gitignore` (`**/localrun.json`).
   - **For a shared or release machine:** create the file there by hand, or from your team's secret store.

---

## 4. Test data files (`LocalRunData/<TestCaseId>.json`)

- **Picking the file:** the file name is the test's **TestCaseId**, from `[Property("TestCaseId", "TC_ELIG_01")]`.
- **Model-based tests** (`TestData.Load<T>()`): the file holds the same JSON document as Test Data Management. Only include the sections you want to change; missing sections use the model's default values, exactly like a Portal run.

  ```json
  {
    "Provider": { "ProviderMedicaidId": "2422659" },
    "SearchCriteria": {
      "MedicaidBillingNumber": "103798930699",
      "Ssn": "123456789",
      "DateOfBirth": "1960-10-21",
      "FromDateOfService": "2026-01-01",
      "ToDateOfService": "2026-10-14"
    }
  }
  ```

- **Legacy tests** (`DataRepository.GetAutomationData("<flow>")`): the file is a JSON **array** of automation-data rows, the same shape the API returns.

  ```json
  [ { "SectionName": "SearchMemberEligiblity",
      "automationContents": [ { "FieldName": "MedicaidBillingNumber", "FieldValue": "0005987" } ] } ]
  ```

- **No file:** a model-based test uses the model's default values. The Test Explorer output says which source was used, e.g. `Test data ... from LocalRunData/TC_ELIG_01.json`.
- **Copying Portal data:** open the test case in Test Data Management → JSON view, and paste the document into the file.
- **Rebuild after editing** any JSON file so the copy in the output folder is updated. Running from Test Explorer rebuilds automatically.

---

## 5. Running and debugging

1. Open `AutomationTests\AutomationTests.sln` in Visual Studio.
2. Build the solution.
3. In **Test Explorer**, expand `TC.MemberEligibilitySearch` and right-click the test, e.g. `SearchMemberEligibilityTest → SearchMemberEligibility`, then choose **Run** or **Debug** (breakpoints work).
4. Watch the steps in the test's **Output** pane; screenshots are in `bin\Debug\net8.0\LocalRunResults\`.

**From the command line (same behaviour):**

```powershell
dotnet test AutomationTests\TC.SearchEligibility --filter "FullyQualifiedName~SearchMemberEligibilityTest"
```

**Notes:**
- **Only the failure screenshot is saved locally for existing tests:** their own step-screenshot upload in `[TearDown]` only runs for Portal runs.
- **Avoid sharing a login account at the same time:** a local run isn't covered by the Portal queue's per-account lock, so don't run it while a Portal run uses the same login account.

---

## 6. Enabling another test project

No code changes are needed; the switch lives in Selenium.BaseComponents.

1. Add to the project's `.csproj`:

   ```xml
   <ItemGroup>
     <None Include="localrun.json" Condition="Exists('localrun.json')">
       <CopyToOutputDirectory>Always</CopyToOutputDirectory>
     </None>
     <None Include="LocalRunData\*.json">
       <CopyToOutputDirectory>Always</CopyToOutputDirectory>
     </None>
   </ItemGroup>
   ```

2. Add `localrun.example.json` (copy it from TC.SearchEligibility) and your own `localrun.json`.
3. Add `LocalRunData/<TestCaseId>.json` for each test case that needs data.

---

## 7. Troubleshooting

| Message | Fix |
|---|---|
| "Run this test from the Portal ... or add a localrun.json" | `localrun.json` isn't in the output folder: create it next to the `.csproj` and rebuild |
| "Local run: "Environment" is missing in localrun.json" | Add the `Environment` section |
| "Could not resolve login credentials for profile ..." | Fill in `LoginUser` (UserName and Password) |
| "Local run: "LoginUsersByRole.<Role>" is missing" | Add that role under `LoginUsersByRole` |
| Test data shows "model defaults" | No `LocalRunData/<TestCaseId>.json` for that TestCaseId, or the file wasn't rebuilt into the output |
| OH\|ID "rejected the one-time code" | `Otp.TotpKey` doesn't match the authenticator registered on the OH\|ID account, or the PC clock is wrong. Stop and fix the key before retrying, to avoid locking the account |
| The website shows its own error (e.g. "An error occurred while processing the request") | The application under test returned an error; check the data or the environment, not the local-run setup |

---

## 8. Code reference

| File | Role |
|---|---|
| `Selenium.BaseComponents/Utilities/LocalRun.cs` | Detects local mode; reads `localrun.json` and `LocalRunData`; local log and screenshot output |
| `Selenium.BaseComponents/Utilities/APIGatway.cs` | Each method returns local data first when local mode is on (no HTTP) |
| `Selenium.BaseComponents/Utilities/TestData.cs` | Skips the "assigned Portal test case" check locally |
| `Selenium.BaseComponents/Base/BaseFeatureFixture.cs` | Browser from `localrun.json`; failure screenshot saved locally |
| `Selenium.BaseComponents/Ohid/OhidSettings.cs`, `OhidFeatureFixture.cs` | OH\|ID key and credentials from `localrun.json` |
| `Selenium.BaseComponents.Tests/LocalRunTests.cs` | Unit tests for the local-run behaviour |
