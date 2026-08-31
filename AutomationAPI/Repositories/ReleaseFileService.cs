using System.Text.RegularExpressions;
using AutomationAPI.Repositories.Interfaces;

namespace AutomationAPI.Repositories
{
    public class ReleaseFileService : IReleaseFileService
    {
        private readonly string _rootPath;

        public ReleaseFileService(IConfiguration configuration)
        {
            _rootPath = configuration["ReleaseSettings:RootPath"];
            if (string.IsNullOrWhiteSpace(_rootPath))
                throw new InvalidOperationException("ReleaseSettings:RootPath is not configured.");
        }

        // Trims, collapses whitespace to '-', and strips characters not valid in a folder name.
        private static string Sanitize(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
                throw new ArgumentException("Path segment cannot be empty.");

            var collapsedWhitespace = Regex.Replace(segment.Trim(), @"\s+", "-");

            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(collapsedWhitespace.Where(c => !invalid.Contains(c)).ToArray());

            if (string.IsNullOrWhiteSpace(cleaned))
                throw new ArgumentException($"Path segment '{segment}' is not a valid folder name.");

            return cleaned;
        }

        public string ResolveReleaseFolderPath(string environmentName, int releaseId, string releaseName, string version)
        {
            // <Environment Root>\REL-<ReleaseId>_<ReleaseName>_v<Version>  (no separate version folder)
            // "REL-{id}" mirrors the identifier shown in the UI (release cards/details) for
            // easy cross-reference between the filesystem and the application.
            var folderName = $"REL-{releaseId}_{Sanitize(releaseName)}_v{Sanitize(version)}";
            return Path.Combine(_rootPath, Sanitize(environmentName), folderName);
        }

        public void CreateReleaseFolder(string releaseFolderPath)
        {
            if (string.IsNullOrWhiteSpace(releaseFolderPath))
                throw new ArgumentException("Release folder path cannot be empty.");

            Directory.CreateDirectory(releaseFolderPath);
        }

        public void DeleteReleaseFolder(string releaseFolderPath)
        {
            if (string.IsNullOrWhiteSpace(releaseFolderPath) || !Directory.Exists(releaseFolderPath))
                return;

            Directory.Delete(releaseFolderPath, recursive: true);
        }
    }
}
