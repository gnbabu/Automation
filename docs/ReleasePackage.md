# Release package: zip of test DLLs for the release folder

Every time a test project (`TC.*`) under `AutomationTests` is built, a zip is created automatically. It holds exactly the files a Portal release folder needs, ready to share and upload; no manual copying from `bin\` is needed.

---

## 1. Quick start

1. **Build the test project** in Visual Studio (right-click the project → **Build**, or **Build Solution**).
   - For a real release, switch the configuration to **Release** first. Debug builds work too.
2. **Find the zip** in `D:\OHPNM-Automation\AutomationTests\ReleasePackages\`, e.g. `TC.MemberEligibilitySearch.zip`. Visual Studio's **Output** window also shows the path:
   ```
   Release package: D:\OHPNM-Automation\AutomationTests\ReleasePackages\TC.MemberEligibilitySearch.zip
   ```
3. **Share the zip** with whoever deploys the release.
4. **Extract the zip** into the release folder, e.g. `D:\Releases\E2EP3\REL-55_MemberEligibilitySearch_v1.0.0\`.
5. **In the Portal:** Release Management → the release → **Check readiness**, then activate it. Discovery picks up the DLLs from the folder.

---

## 2. What the zip contains

| File | Always | Notes |
|---|---|---|
| `<TestProject>.dll` | Yes | The tests, e.g. `TC.MemberEligibilitySearch.dll` |
| `Selenium.BaseComponents.dll` | Yes | The shared Selenium framework |
| `TestDataModels.dll` | If the project uses test data models | e.g. TC.MemberEligibilitySearch, TC.PriorAuthoriztion |
| `appSettings.json` | Yes | Automation API URL and headless setting |
| `suite-manifest.json` | Yes | List of the suite's test cases (see section 9) |
| `TestFiles\...` | If the project has a `TestFiles` folder | e.g. `TestFiles\SampleAttachment.pdf` for Professional PA |

**Never included:**
- local-run files: `localrun.json`, `localrun.secrets.json`, `LocalRunData\`;
- secrets: `appSettings.Local.json`, `demo.secrets.json`;
- `.pdb` files;
- NUnit, Selenium and Microsoft DLLs, which the Automation API already provides.

**Current zips:**

| Zip | Contents |
|---|---|
| `TC.MemberEligibilitySearch.zip` | DLL, Selenium.BaseComponents, TestDataModels, appSettings.json |
| `TC.PriorAuthoriztion.zip` | DLL, Selenium.BaseComponents, TestDataModels, appSettings.json, `TestFiles\SampleAttachment.pdf` |
| `TC.E2EP3Demo.zip`, `TC.PriorAuthInquiry.zip`, `TC.PriorAuthSearch.zip`, `TC.ProviderDataEntry.zip`, `TC.SearchRA.zip`, `TC.SubmitClaims.zip` | DLL, Selenium.BaseComponents, appSettings.json |

---

## 3. Deploying to a release folder

1. **Create the release** in the Portal (Release Management) if it doesn't exist yet. The Portal shows its folder, e.g. `D:\Releases\E2EP3\REL-56_MyRelease_v1.0.0`.
2. **Extract** the zip's contents directly into that folder: the DLLs go in the folder itself, not in a sub-folder. Overwrite existing files when updating a release.
3. **Check `appSettings.json`:** `AppSettings:AutomationAPI` must point to the Automation API the tests should report to (e.g. the server's API URL rather than `https://localhost:7147/`).
4. **OH|ID tests only** (e.g. TC_ELIG_20): also copy `appSettings.Local.json`, which holds the authenticator key, into the release folder. It's deliberately **not** in the zip because it contains a secret.
5. **Unlock the files if needed:** if the Automation API is running, it may lock the old DLLs in that folder. Stop the API (or recycle its IIS app pool) before overwriting, then start it again.
6. **Activate:** in the Portal, run **Check readiness** on the release; it should report the test DLL as usable. Then activate it and assign the test cases.

---

## 4. When the zip is (re)created

- **On every build** of the project: Build, Rebuild, Build Solution, or `dotnet build`. The previous zip is **overwritten**, so it always matches the last build.
- **If a zip is deleted**, the next Build recreates it. Visual Studio treats the project as out of date while its zip is missing.
- **Not** on Visual Studio's background (design-time) builds, so the zip doesn't change while you type.
- **Configuration:** the zip contains whichever configuration was just built (Debug or Release).

**From the command line:**

```powershell
# one project
dotnet build AutomationTests\TC.SearchEligibility\TC.MemberEligibilitySearch.csproj -c Release

# all test projects
dotnet build AutomationTests\AutomationTests.sln -c Release
```

---

## 5. New test projects

A new project gets its zip **automatically**, with no extra setup, when:
- it's inside the `AutomationTests` folder, and
- its project name starts with **`TC.`**, e.g. `TC.SearchClaims.csproj` produces `TC.SearchClaims.zip`.

If it has a `TestFiles` folder that's copied to the output, the folder is added to the zip automatically.

---

## 6. Turning it on or off

| Want to… | Do this |
|---|---|
| Skip the zip for one project permanently | Add `<CreateReleasePackage>false</CreateReleasePackage>` inside a `<PropertyGroup>` in that `.csproj` |
| Create a zip for a project whose name doesn't start with `TC.` | Add `<CreateReleasePackage>true</CreateReleasePackage>` to its `.csproj` |
| Skip zips for a single build | `dotnet build ... -p:CreateReleasePackage=false` |

`TC.OhidEligibilityDemo` (the standalone Visual Studio demo) is excluded on purpose.

---

## 7. Troubleshooting

| Problem | Fix |
|---|---|
| No zip after building | Check the project name starts with `TC.` and is under `AutomationTests`, and that the build succeeded. Look for the `Release package:` line in the Output window (set build output verbosity to **Normal** or higher) |
| Zip didn't change | The project didn't build (nothing changed). Use **Rebuild** to force it |
| Zip is missing a file the tests need at run time | Make sure the file is copied to the build output (`CopyToOutputDirectory`). Only the files in section 2 are packed; for anything else, ask to extend `AutomationTests\Directory.Build.targets` |
| "The process cannot access the file" when extracting into the release folder | The Automation API has the old DLL loaded: stop the API or recycle its app pool, extract, then start it again |
| Readiness check shows no usable test DLL | Make sure the files were extracted into the release folder itself, not a sub-folder |
| OH\|ID test fails with "TOTP key not configured" | Copy `appSettings.Local.json` into the release folder (section 3, step 4) |

---

## 8. How it works (for maintainers)

- **Where:** `AutomationTests\Directory.Build.targets`. MSBuild imports it automatically into every project under `AutomationTests`.
- **The target:** `CreateReleasePackage` runs `AfterTargets="Build"`.
  1. Copies the files from section 2 out of the build output into `obj\<Configuration>\net8.0\ReleasePackage\`.
  2. Zips that folder with MSBuild's built-in `ZipDirectory` task into `AutomationTests\ReleasePackages\<AssemblyName>.zip`.
- **Git:** `AutomationTests/ReleasePackages/` is in `.gitignore`.

---

## 9. Suite manifest (`suite-manifest.json`)

Every build also writes `suite-manifest.json` into the test project folder and adds it to the zip. See **[SuiteManifest.md](SuiteManifest.md)** for how to use it.
