# Suite manifest (`suite-manifest.json`)

Every test project (`TC.*`) under `AutomationTests` gets a **`suite-manifest.json`** in its project folder, created on every build. It describes the test suite and lists all of its test cases, and it's included in the project's release zip (see [ReleasePackage.md](ReleasePackage.md)).

You never edit the manifest by hand. You set the suite values in the `.csproj` and the test case values with attributes on the test methods; the build writes the file.

---

## 1. Quick start

1. **Set the suite values once:** add these four lines to the first `<PropertyGroup>` of the test project's `.csproj`:

   ```xml
   <TestSuiteName>Prior Authorization</TestSuiteName>
   <Version>1.2.0</Version>
   <Description>Professional and Dental Prior Authorization automation scenarios.</Description>
   <Authors>Ohio Developer Workspace</Authors>
   ```

2. **Give each test method its attributes:**

   ```csharp
   [Test]
   [Category("IntegrationTests")]
   [Property("Description", "Submit a Professional PA with two diagnoses, two service lines and an attachment")]
   [Property("TestCaseId", "TC_PA_PROF_01")]
   [CancelAfter(900000)]
   public void ProfessionalPA_Save()
   ```

3. **Build** the project or the solution (Visual Studio **Build**, or `dotnet build`).

4. **Open `suite-manifest.json`** in the project folder, e.g. `AutomationTests\TC.PriorAuthoriztion\suite-manifest.json`.

---

## 2. Example output

```json
{
  "testSuiteName": "Prior Authorization",
  "version": "1.2.0",
  "description": "Professional and Dental Prior Authorization automation scenarios.",
  "entryAssembly": "TC.PriorAuthoriztion.dll",
  "entryType": "TC.PriorAuthoriztion.Tests",
  "author": "Ohio Developer Workspace",
  "testCases": [
    {
      "testCaseId": "TC_PA_PROF_01",
      "testCaseName": "ProfessionalPA_Save",
      "testCaseDescription": "Submit a Professional PA with two diagnoses, two service lines and an attachment",
      "entryClass": "TC.PriorAuthoriztion.Tests.SubmitPAProfessionalTest",
      "entryMethod": "ProfessionalPA_Save",
      "displayOrder": 3,
      "estimatedDurationSeconds": 900,
      "testCaseType": "Integration"
    }
  ]
}
```

---

## 3. Where each value comes from

### Suite values (top of the file)

| Field | Set in | If not set |
|---|---|---|
| `testSuiteName` | `.csproj` `<TestSuiteName>` | The project name, e.g. `TC.MemberEligibilitySearch` |
| `version` | `.csproj` `<Version>` | `1.0.0` |
| `description` | `.csproj` `<Description>` | Empty |
| `author` | `.csproj` `<Authors>` | The project name |
| `entryAssembly` | Automatic: the built test DLL | — |
| `entryType` | Automatic: the namespace of the test classes | — |

### Test case values (`testCases`)

| Field | Set with | If not set |
|---|---|---|
| `testCaseId` | `[Property("TestCaseId", "TC_PA_PROF_01")]` | The method name |
| `testCaseName` | The method name | — |
| `testCaseDescription` | `[Property("Description", "...")]` (or `[Description("...")]`) | Empty |
| `entryClass` | Automatic: the full class name | — |
| `entryMethod` | The method name | — |
| `displayOrder` | Automatic: position in the suite (see section 5) | — |
| `estimatedDurationSeconds` | `[CancelAfter(ms)]` or `[Timeout(ms)]`, in seconds (900000 ms → 900) | `null` |
| `testCaseType` | `[Category("IntegrationTests")]` with "Tests" removed → `Integration` | Empty |

**Which methods are listed:** public methods with `[Test]`, `[TestCase]`, `[TestCaseSource]` or `[Theory]` in non-abstract classes of the project.

---

## 4. Common tasks

| I want to… | Do this, then build |
|---|---|
| Add a test case | Add the test method with its attributes (section 1, step 2); it's listed automatically, including in a new test class |
| Change a description | Edit `[Property("Description", ...)]` on the method |
| Change the suite name, version, description or author | Edit the four lines in the `.csproj` |
| Change the estimated duration | Edit `[CancelAfter(ms)]` on the method |
| Remove a test case | Delete the method, or remove its `[Test]` attribute |
| Fix a test's position in the list | Add `[Order(n)]` to the method |
| Add a new test project | Name it `TC.<Something>` under `AutomationTests`; add the four `.csproj` lines. It gets a manifest automatically |
| Stop a project from getting a manifest and zip | Add `<CreateReleasePackage>false</CreateReleasePackage>` to its `.csproj` |

---

## 5. displayOrder

- **Numbering:** test cases are numbered 1, 2, 3, … across the whole suite.
- **Order:** test classes sorted A–Z by full name, then, within each class, methods with `[Order(n)]` first (by `n`), then the rest in the order they appear in the file.
- **Renumbering:** the numbers are recalculated on every build, so adding a test can shift the numbers of tests after it.

---

## 6. When the file is updated

| Situation | Result |
|---|---|
| A test, its attributes, or the suite values in the `.csproj` changed | Rebuilt on the next build |
| `suite-manifest.json` was deleted | Recreated on the next **Build** |
| Nothing changed | Left untouched (the file is only rewritten when its content changes) |

The build output shows a line like `Suite manifest: ...\suite-manifest.json (4 test cases)` when the Visual Studio build output verbosity is **Normal** or higher.

---

## 7. Troubleshooting

| Problem | Fix |
|---|---|
| `description` is empty | Add `<Description>` to the project's `.csproj` (section 1) |
| `testCaseDescription` is empty | Add `[Property("Description", "...")]` to that test method |
| `author` or `testSuiteName` shows the project name | Add `<Authors>` / `<TestSuiteName>` to the `.csproj` |
| `estimatedDurationSeconds` is `null` | Add `[CancelAfter(ms)]` to the test method |
| A test is missing from the list | It needs `[Test]` (or `[TestCase]`/`[TestCaseSource]`), must be `public`, and its class must not be `abstract` |
| No manifest at all | The project name must start with `TC.` and the project must be under `AutomationTests`; check that the build succeeded |
| Build error from `SuiteManifestGenerator` | Build the solution once so the generator tool is built, then build the test project again |

---

## 8. How it works (for maintainers)

- **Target:** `AutomationTests\Directory.Build.targets` → `GenerateSuiteManifest`, which runs after the build of every `TC.*` project.
  1. Writes the suite values to `obj\...\suite-manifest.settings`.
  2. Runs the console tool `AutomationTests\SuiteManifestGenerator`. It loads the built test DLL and reads its NUnit attributes without running any tests, then writes `suite-manifest.json`.
- **Build order:** the tool is built automatically first, through a `ProjectReference` that isn't copied into the test output.
- **Zip:** the release zip target (`CreateReleasePackage`) runs after the manifest target and includes the manifest.
