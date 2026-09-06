using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Selenium.BaseComponents
{
    public class SettingsReader
    {
        private readonly IConfiguration _configuration;

        public SettingsReader()
        {
            // Resolved relative to this assembly's own location, not
            // Directory.GetCurrentDirectory() - confirmed by direct testing that an
            // isolated (ProcessModel=Separate) NUnit run's working directory is NOT
            // reliably the deployed Release folder (it stayed AutomationAPI's own
            // directory even with EnginePackageSettings.WorkDirectory set - that setting
            // affects the engine's own file resolution, not the spawned agent process's
            // real OS-level CWD). CWD-relative lookup silently picked up a completely
            // different, wrong appSettings.json (AutomationAPI's own appsettings.json,
            // matched case-insensitively on Windows) with no "AppSettings:AutomationAPI"
            // key, producing a null apiUrl and "must be an absolute URI" HttpClient
            // failures - not an auth/wiring bug. This assembly's own location is always
            // correct regardless of CWD, since appSettings.json is deployed alongside it
            // (CopyToOutputDirectory=Always) in every Release folder.
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? Directory.GetCurrentDirectory();

            _configuration = new ConfigurationBuilder().SetBasePath(assemblyDirectory)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true).Build();
        }

        public string GetSetting(string key)
        {
            return _configuration[key];
        }
    }
}
