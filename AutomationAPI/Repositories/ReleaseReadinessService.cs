using System.Reflection;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories
{
    /// <summary>
    /// Determines Release readiness by scanning the Release folder for usable test DLLs.
    /// Reuses the same reflection-based discovery technique as the existing
    /// TestSuitesRepository/ReflectionTestRunner (Assembly.LoadFrom + NUnit TestFixture scan),
    /// scoped to a single release folder instead of the global TestLibs path. This is a
    /// read-only readiness check — it does not upload, store, or validate DLL versions.
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
            foreach (var dllPath in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);

                    var hasTestFixtures = assembly.GetTypes()
                        .Any(t => t.IsClass && t.IsPublic &&
                                  t.GetCustomAttributes(typeof(NUnit.Framework.TestFixtureAttribute), false).Any());

                    if (hasTestFixtures)
                        usableCount++;
                }
                catch
                {
                    // Skip DLLs that fail to load (not usable test content); mirrors existing discovery behavior.
                }
            }

            result.UsableDllCount = usableCount;
            result.IsReady = usableCount > 0;
            result.Message = result.IsReady
                ? $"{usableCount} usable test assembly(ies) found. Ready for activation."
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
