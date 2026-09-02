using System.Xml;
using NUnit; // EnginePackageSettings
using NUnit.Engine;

namespace AutomationAPI.Repositories.TestRunner
{
    // Replaces ReflectionTestRunner. Runs tests through NUnit's real execution engine
    // (already referenced as a package, previously unused) instead of hand-rolled
    // reflection, so async test methods, [OneTimeSetUp]/[OneTimeTearDown],
    // TestContext.Parameters/CurrentContext, [Ignore]/[Explicit], and attribute-generated
    // parameterized tests ([Values]/[Range]/[Combinatorial]/[TestFixtureSource]) all work
    // correctly - they're handled by NUnit itself, not approximated by us.
    //
    // Runs each package in an isolated child process (ProcessModel=Separate) by default,
    // so one hung/crashed Selenium test can't freeze TestQueueWorker's shared queue for
    // every user/Release. Falls back to in-process execution only if the isolated process
    // can't load NUnit.Framework at all - which happens when the Release folder has just
    // the bare test DLL rather than a full publish output (.deps.json + dependencies)
    // alongside it. See AGENTS.md for why this fallback exists and what's required for
    // isolated execution to actually take effect in production.
    public class NUnitEngineTestRunner : ITestRunner
    {
        private readonly ILogger<NUnitEngineTestRunner> _logger;

        public NUnitEngineTestRunner(ILogger<NUnitEngineTestRunner> logger)
        {
            _logger = logger;
        }

        public Task<List<TestExecutionResult>> RunAsync(TestRunRequest request)
        {
            return Task.Run(() => RunCore(request));
        }

        private List<TestExecutionResult> RunCore(TestRunRequest request)
        {
            string? dllPath = ResolveDllPath(request.LibsPath, request.Library);
            if (dllPath == null)
            {
                return new List<TestExecutionResult>
                {
                    Failure("AssemblyResolve", request.Library ?? request.ClassName ?? "",
                        $"Could not find a DLL named '{request.Library}' in '{request.LibsPath}'.")
                };
            }

            XmlNode exploreResult;
            try
            {
                exploreResult = NUnitEngineHelper.Explore(dllPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to explore {Dll}", dllPath);
                return new List<TestExecutionResult>
                {
                    Failure("AssemblyLoad", request.Library ?? "", ex.Message)
                };
            }

            TestFilter filter;
            if (string.IsNullOrEmpty(request.ClassName) && string.IsNullOrEmpty(request.MethodName))
            {
                filter = TestFilter.Empty;
            }
            else
            {
                filter = NUnitEngineHelper.BuildFilter(exploreResult, request.ClassName, request.MethodName);
                if (filter == null)
                {
                    return new List<TestExecutionResult>
                    {
                        Failure(request.MethodName ?? "", request.ClassName ?? "",
                            $"No test matching Class='{request.ClassName}', Method='{request.MethodName}' was found in '{Path.GetFileName(dllPath)}'.")
                    };
                }
            }

            XmlNode runResult;
            bool wasIsolated;
            try
            {
                runResult = RunPackage(dllPath, request, filter, isolated: true);
                wasIsolated = true;

                // Missing dependencies don't always throw - the engine can also just
                // return a normal-looking XmlNode with the assembly marked NotRunnable/
                // Invalid (confirmed by direct testing: which shape happens depends on
                // exactly which dependency is missing first). Treat both the same way.
                if (NUnitEngineHelper.IsUnrunnableResult(runResult))
                {
                    _logger.LogWarning(
                        "Isolated (process-separated) execution of {Dll} came back NotRunnable/Invalid - " +
                        "required dependencies likely aren't deployed alongside it - falling back to " +
                        "in-process execution for this run only. The Release folder needs a full publish " +
                        "output (.deps.json + dependencies), not just the bare test DLL, for isolated " +
                        "execution to actually take effect.", dllPath);
                    runResult = RunPackage(dllPath, request, filter, isolated: false);
                    wasIsolated = false;
                }
            }
            catch (NUnitEngineException ex) when (NUnitEngineHelper.IsMissingFrameworkDependency(ex))
            {
                _logger.LogWarning(ex,
                    "Isolated (process-separated) execution of {Dll} failed because required dependencies " +
                    "aren't deployed alongside it - falling back to in-process execution for this run only. " +
                    "The Release folder needs a full publish output (.deps.json + dependencies), not just the " +
                    "bare test DLL, for isolated execution to actually take effect.", dllPath);
                runResult = RunPackage(dllPath, request, filter, isolated: false);
                wasIsolated = false;
            }

            return ParseRunResults(runResult, wasIsolated);
        }

        private XmlNode RunPackage(string dllPath, TestRunRequest request, TestFilter filter, bool isolated)
        {
            var package = new TestPackage(dllPath);
            package.AddSetting(EnginePackageSettings.ProcessModel, isolated ? "Separate" : "InProcess");

            if (!string.IsNullOrWhiteSpace(request.Browser))
            {
                // Set both the modern dictionary form and the legacy string form, so the
                // parameter is readable via TestContext.Parameters regardless of which
                // NUnit.Framework version the test project was built against (the console
                // runner does the same for exactly this reason).
                package.AddSetting("TestParametersDictionary", new Dictionary<string, string> { ["Browser"] = request.Browser });
                package.AddSetting("TestParameters", $"Browser={request.Browser}");
            }

            // `filter` was built by name (class+method), not by NUnit's numeric test-case
            // `id` - confirmed by direct testing that ids are only valid within the exact
            // TestPackage/runner instance that produced them, so a name-based filter is
            // required here to be safely reusable against this brand-new package/runner.
            using var runner = NUnitEngineHelper.Engine.GetRunner(package);
            return runner.Run(listener: null, filter: filter);
        }

        private static string? ResolveDllPath(string libsPath, string? library)
        {
            if (!Directory.Exists(libsPath))
                return null;

            var dllFiles = Directory.GetFiles(libsPath, "*.dll");

            if (string.IsNullOrEmpty(library))
                return dllFiles.FirstOrDefault();

            return dllFiles.FirstOrDefault(d =>
                Path.GetFileNameWithoutExtension(d).Equals(library, StringComparison.OrdinalIgnoreCase));
        }

        private static List<TestExecutionResult> ParseRunResults(XmlNode runResult, bool wasIsolated)
        {
            var results = new List<TestExecutionResult>();

            foreach (XmlNode testCase in runResult.SelectNodes("//test-case")!)
            {
                var name = testCase.Attributes?["methodname"]?.Value ?? testCase.Attributes?["name"]?.Value ?? "";
                var className = testCase.Attributes?["classname"]?.Value ?? "";
                var resultAttr = testCase.Attributes?["result"]?.Value ?? "Inconclusive";

                var outcome = resultAttr switch
                {
                    "Passed" => TestOutcome.Passed,
                    "Failed" => TestOutcome.Failed,
                    "Skipped" => TestOutcome.Skipped,
                    "Warning" => TestOutcome.Skipped,
                    _ => TestOutcome.Inconclusive
                };

                string message = "";
                var failureNode = testCase.SelectSingleNode("failure/message");
                var reasonNode = testCase.SelectSingleNode("reason/message");
                if (failureNode != null)
                    message = failureNode.InnerText;
                else if (reasonNode != null)
                    message = reasonNode.InnerText;

                DateTime? startTime = ParseXmlDateTime(testCase.Attributes?["start-time"]?.Value);
                DateTime? endTime = ParseXmlDateTime(testCase.Attributes?["end-time"]?.Value);

                results.Add(new TestExecutionResult
                {
                    Name = name,
                    ClassName = className,
                    Outcome = outcome,
                    Message = message,
                    StartTime = startTime,
                    EndTime = endTime,
                    WasIsolated = wasIsolated
                });
            }

            return results;
        }

        private static DateTime? ParseXmlDateTime(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
                ? dt
                : null;
        }

        private static TestExecutionResult Failure(string name, string className, string message) => new()
        {
            Name = name,
            ClassName = className,
            Outcome = TestOutcome.Failed,
            Message = message,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow
        };
    }
}
