using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Selenium.BaseComponents.Utilities
{
    // Visual Studio / dotnet test runs: no Portal AccessToken, and a localrun.json next to the
    // test DLLs. Every APIGatway call is then answered from that file (with passwords/keys merged
    // in from the git-ignored localrun.secrets.json) and logs/screenshots stay on this machine,
    // so nothing touches the Automation API or database. Portal runs always carry an
    // AccessToken and never take this path.
    public static class LocalRun
    {
        public const string FileName = "localrun.json";
        public const string SecretsFileName = "localrun.secrets.json";

        private static readonly object Gate = new();
        private static readonly string RunStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        private static string? _cachedPath;
        private static DateTime _cachedWriteTime;
        private static DateTime _cachedSecretsWriteTime;
        private static JObject? _cached;

        public static string? DirectoryOverride { get; set; }

        private static string BaseDirectory => DirectoryOverride ?? AppContext.BaseDirectory;

        public static string FilePath => Path.Combine(BaseDirectory, FileName);

        public static string SecretsFilePath => Path.Combine(BaseDirectory, SecretsFileName);

        public static bool IsPortalRun => !string.IsNullOrWhiteSpace(TestContext.Parameters["AccessToken"]);

        public static bool IsActive => !IsPortalRun && File.Exists(FilePath);

        public static string? Browser => (string?)Config["Browser"];

        public static EnvironmentDetails Environment()
        {
            var section = Config["Environment"] as JObject
                ?? throw Missing("Environment");
            return section.ToObject<EnvironmentDetails>()!;
        }

        public static LoginUserCredentials? LoginUser() =>
            (Config["LoginUser"] as JObject)?.ToObject<LoginUserCredentials>();

        public static LoginUserCredentials LoginUserByRole(string role)
        {
            var users = Config["LoginUsersByRole"] as JObject;
            var match = users?.Properties().FirstOrDefault(p => string.Equals(p.Name, role, StringComparison.OrdinalIgnoreCase));
            return (match?.Value as JObject)?.ToObject<LoginUserCredentials>()
                ?? throw Missing($"LoginUsersByRole.{role}");
        }

        public const string DataFolder = "LocalRunData";

        public static string? TestCaseFile(string? testCaseId = null)
        {
            testCaseId ??= CurrentTestCaseId();
            if (string.IsNullOrWhiteSpace(testCaseId)) return null;
            var path = Path.Combine(BaseDirectory, DataFolder, testCaseId + ".json");
            return File.Exists(path) ? path : null;
        }

        public static (string Json, string Source) TestData(string flowName)
        {
            var testCaseId = CurrentTestCaseId();
            var file = TestCaseFile(testCaseId);
            if (file != null && JToken.Parse(File.ReadAllText(file)) is JObject fileDocument)
                return (fileDocument.ToString(), $"{DataFolder}/{Path.GetFileName(file)}");

            var testData = Config["TestData"] as JObject;

            foreach (var key in new[] { testCaseId, flowName })
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                var match = testData?.Properties().FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
                if (match?.Value is JObject document)
                    return (document.ToString(), $"{FileName}: {match.Name}");
            }

            return ("{}", $"model defaults (no {DataFolder}/{testCaseId ?? flowName}.json)");
        }

        public static List<AutomationData> AutomationData(string flowName)
        {
            var file = TestCaseFile();
            if (file != null && JToken.Parse(File.ReadAllText(file)) is JArray fileRows)
                return fileRows.ToObject<List<AutomationData>>() ?? new List<AutomationData>();

            var section = Config["AutomationData"] as JObject;
            var match = section?.Properties().FirstOrDefault(p => string.Equals(p.Name, flowName, StringComparison.OrdinalIgnoreCase));
            return (match?.Value as JArray)?.ToObject<List<AutomationData>>() ?? new List<AutomationData>();
        }

        public static string? Otp(string name) => (string?)(Config["Otp"] as JObject)?[name];

        public static void Log(TestCaseExecutionLog log)
        {
            var level = log.LogLevel == TestCaseLogLevel.Info ? "" : $" [{log.LogLevel}]";
            TestContext.Progress.WriteLine($"[{DateTime.Now:HH:mm:ss}]{level} {log.StepName}: {log.LogMessage}");
        }

        public static string SaveScreenshot(TestScreenshot screenshot)
        {
            var data = screenshot.Screenshot ?? "";
            var comma = data.IndexOf(',');
            var bytes = Convert.FromBase64String(comma >= 0 ? data[(comma + 1)..] : data);

            var folder = ResultsFolder();
            lock (Gate)
            {
                var number = Directory.GetFiles(folder, "*.png").Length + 1;
                var name = string.Concat((screenshot.Caption ?? "Screenshot").Where(char.IsLetterOrDigit));
                var file = Path.Combine(folder, $"{number:D2}_{(name.Length == 0 ? "Screenshot" : name)}.png");
                File.WriteAllBytes(file, bytes);
                return file;
            }
        }

        public static string ResultsFolder()
        {
            var testCase = CurrentTestCaseId() ?? TestContext.CurrentContext.Test.ClassName?.Split('.').Last() ?? "Run";
            var folder = Path.Combine(BaseDirectory, "LocalRunResults", testCase, RunStamp);
            Directory.CreateDirectory(folder);
            return folder;
        }

        private static string? CurrentTestCaseId()
        {
            try
            {
                return TestContext.CurrentContext.Test.Properties.Get("TestCaseId") as string;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static JObject Config
        {
            get
            {
                var path = FilePath;
                if (!File.Exists(path))
                    throw new InvalidOperationException($"Local run: {FileName} not found next to the test DLLs ({BaseDirectory}).");

                var secretsPath = SecretsFilePath;
                lock (Gate)
                {
                    var writeTime = File.GetLastWriteTimeUtc(path);
                    var secretsWriteTime = File.Exists(secretsPath) ? File.GetLastWriteTimeUtc(secretsPath) : DateTime.MinValue;
                    if (_cached == null || _cachedPath != path || _cachedWriteTime != writeTime || _cachedSecretsWriteTime != secretsWriteTime)
                    {
                        var config = JObject.Parse(File.ReadAllText(path));
                        if (File.Exists(secretsPath))
                            config.Merge(JObject.Parse(File.ReadAllText(secretsPath)),
                                new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Ignore });

                        _cached = config;
                        _cachedPath = path;
                        _cachedWriteTime = writeTime;
                        _cachedSecretsWriteTime = secretsWriteTime;
                    }
                    return _cached;
                }
            }
        }

        private static InvalidOperationException Missing(string section) =>
            new($"Local run: \"{section}\" is missing in {FileName} ({FilePath}).");
    }
}
