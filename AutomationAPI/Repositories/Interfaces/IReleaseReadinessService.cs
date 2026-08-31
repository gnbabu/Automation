using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IReleaseReadinessService
    {
        /// <summary>
        /// Determines whether a release folder has usable test DLL content available,
        /// by reusing the existing DLL loading/discovery technique (NUnit TestFixture scan).
        /// DLLs are placed in the folder by the existing controlled build/deployment process;
        /// this is a read-only check, not an upload/validation system.
        /// </summary>
        ReleaseReadinessModel CheckReadiness(string releaseFolderPath);

        /// <summary>
        /// Cheap, non-reflective check (file count only) suitable for list views.
        /// </summary>
        int GetDllFileCount(string releaseFolderPath);
    }
}
