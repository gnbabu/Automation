namespace AutomationAPI.Repositories.Interfaces
{
    public interface IReleaseFileService
    {
        /// <summary>
        /// Resolves the release folder path as
        /// &lt;Environment Root&gt;\&lt;ReleaseId&gt;_&lt;ReleaseName&gt;_v&lt;Version&gt;.
        /// The Environment Root comes from configuration (ReleaseSettings:RootPath\EnvironmentName),
        /// never hard-coded per environment.
        /// </summary>
        string ResolveReleaseFolderPath(string environmentName, int releaseId, string releaseName, string version);

        /// <summary>
        /// Creates the release folder on disk. Throws if creation fails.
        /// </summary>
        void CreateReleaseFolder(string releaseFolderPath);

        /// <summary>
        /// Deletes the release folder (and its contents) on disk, if it exists.
        /// Used only when a Draft release is permanently deleted. Best-effort: callers
        /// should treat failures as non-fatal (the DB row is the source of truth).
        /// </summary>
        void DeleteReleaseFolder(string releaseFolderPath);
    }
}
