using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.TestRunner;

namespace AutomationAPI.Repositories
{
    /// <summary>
    /// Determines Release readiness by scanning the Release folder for usable test DLLs.
    /// Uses NUnitEngineHelper.Explore() (the same NUnit.Engine-based discovery
    /// TestSuitesRepository already uses), scoped to a single release folder instead of the
    /// global TestLibs path. This is a read-only readiness check — it does not upload,
    /// store, or validate DLL versions.
    ///
    /// Previously used raw reflection (Assembly.LoadFrom + a direct TestFixtureAttribute
    /// scan) - a stale leftover from before this session's NUnit.Engine refactor that was
    /// never migrated along with TestSuitesRepository/the old ReflectionTestRunner.
    /// Confirmed by direct testing this silently reported real, valid test DLLs as "not
    /// usable" once a full `dotnet publish` output (bundling the test project's own NUnit
    /// version, 4.3.0 in this case) sat alongside AutomationAPI's own already-loaded NUnit
    /// 3.14.0 in the same process - Assembly.LoadFrom hit a version conflict during
    /// GetTypes(), silently swallowed by the bare catch. NUnit.Engine's own isolated
    /// discovery doesn't have this problem (it's exactly why Explore()/Run() replaced raw
    /// reflection for TestSuitesRepository/execution earlier this session - this was simply
    /// the one remaining caller that still needed the same fix).
    /// </summary>
    public class ReleaseReadinessService : IReleaseReadinessService
    {
        public ReleaseReadinessModel CheckReadiness(string releaseFolderPath)
        {
            var result = new ReleaseReadinessModel();

            if (string.IsNullOrWhiteSpace(releaseFolderPath) || !Directory.Exists(releaseFolderPath))
            {
                result.FolderExists = false;
                result.IsReady = false;
                result.Message = "Release folder does not exist.";
                return result;
            }

            result.FolderExists = true;

            var dllFiles = Directory.GetFiles(releaseFolderPath, "*.dll");
            result.DllFiles = dllFiles.Select(Path.GetFileName).ToList();

            if (dllFiles.Length == 0)
            {
                result.IsReady = false;
                result.Message = "No DLLs found in the release folder yet.";
                return result;
            }

            int usableCount = 0;
            // Distinguishes "this DLL just isn't a test assembly" (e.g. Newtonsoft.Json.dll
            // sitting alongside the real test DLL - expected, not a problem) from "this
            // looked like it could be a test assembly but couldn't actually load" (e.g. a
            // bare test DLL deployed under ProcessModel=Separate without its dependencies) -
            // only the latter is worth surfacing a hint about, otherwise every Release with
            // ordinary supporting DLLs in its folder would show a confusing warning.
            var hadMissingDependencyEvidence = false;
            foreach (var dllPath in dllFiles)
            {
                try
                {
                    var exploreXml = NUnitEngineHelper.Explore(dllPath);

                    if (NUnitEngineHelper.IsUnrunnableResult(exploreXml))
                    {
                        // "NotRunnable" also legitimately covers an ordinary non-test DLL
                        // that NUnit could still load fine but found no fixtures in (e.g.
                        // Selenium.BaseComponents.dll itself, sitting in the folder as a
                        // project-reference dependency, not a test assembly) - only count
                        // it as "missing dependency" evidence when the skip reason
                        // actually says so (confirmed by direct testing: a real load
                        // failure's _SKIPREASON mentions "Unable to load"/"Could not load
                        // file or assembly"; a merely-empty assembly's says "No test
                        // fixtures were found" instead).
                        var skipReason = exploreXml.SelectSingleNode("//property[@name='_SKIPREASON']")
                            ?.Attributes?["value"]?.Value ?? "";
                        if (skipReason.Contains("load", StringComparison.OrdinalIgnoreCase))
                            hadMissingDependencyEvidence = true;
                        continue;
                    }

                    var testCaseCount = exploreXml.SelectNodes("//test-case")?.Count ?? 0;
                    if (testCaseCount > 0)
                        usableCount++;
                }
                catch (Exception ex)
                {
                    if (NUnitEngineHelper.IsMissingFrameworkDependency(ex))
                        hadMissingDependencyEvidence = true;

                    // Not a test assembly, or failed to load for some other reason - skip
                    // it and keep scanning the rest of the folder, same as
                    // TestSuitesRepository's own discovery does.
                }
            }

            result.UsableDllCount = usableCount;
            result.IsReady = usableCount > 0;
            result.Message = result.IsReady
                ? $"{usableCount} usable test assembly(ies) found. Ready for activation."
                : hadMissingDependencyEvidence
                    ? "DLLs are present but at least one test assembly couldn't load its dependencies. " +
                      "Use 'dotnet publish' (not just the built DLL) so all required dependency DLLs are " +
                      "included in the Release folder, then re-check readiness."
                    : "DLLs are present but none contain usable test content.";

            return result;
        }

        public int GetDllFileCount(string releaseFolderPath)
        {
            if (string.IsNullOrWhiteSpace(releaseFolderPath) || !Directory.Exists(releaseFolderPath))
                return 0;

            return Directory.GetFiles(releaseFolderPath, "*.dll").Length;
        }
    }
}
