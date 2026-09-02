using System.Collections.Concurrent;
using System.Xml;
using NUnit; // EnginePackageSettings lives directly in the NUnit namespace, not NUnit.Engine
using NUnit.Engine;

namespace AutomationAPI.Repositories.TestRunner
{
    // Shared by NUnitEngineTestRunner (execution) and TestSuitesRepository (discovery) so
    // there's exactly one place that talks to the NUnit engine directly. TestEngineActivator
    // creates a fairly heavyweight object (spins up engine services); the engine itself is
    // stateless per-call (all state lives on the TestPackage/ITestRunner it hands out), so
    // one shared instance for the process lifetime is the documented, supported usage.
    public static class NUnitEngineHelper
    {
        private static readonly Lazy<ITestEngine> _engine = new(() => TestEngineActivator.CreateInstance());

        public static ITestEngine Engine => _engine.Value;

        // Explore() is called on every Assignment-screen/Dashboard load, potentially across
        // many DLLs with many classes/methods for a "huge" enterprise suite - re-scanning
        // from scratch every time doesn't scale. Cache keyed by (path, last-write-time), so
        // a Release's DLL being rebuilt/republished automatically invalidates the cache
        // (this doubles as the Phase-3 stale-assembly mitigation for discovery specifically -
        // execution itself doesn't need this, since isolated runs always load fresh).
        private static readonly ConcurrentDictionary<string, (DateTime LastWriteUtc, XmlNode Xml)> _exploreCache = new();

        // Explore() never executes anything - it's a static discovery scan (same NUnit
        // attribute expansion the real engine would use to run, so [TestCaseSource]/
        // [Values]/[Range]/[Combinatorial]/[TestFixtureSource]-generated cases all show up
        // correctly, unlike the old hand-rolled reflection scan). Always run in-process:
        // it's read-only and cheap, so there's no isolation benefit worth the overhead of
        // spawning a process just to explore.
        public static XmlNode Explore(string dllPath)
        {
            var lastWriteUtc = File.GetLastWriteTimeUtc(dllPath);

            if (_exploreCache.TryGetValue(dllPath, out var cached) && cached.LastWriteUtc == lastWriteUtc)
                return cached.Xml;

            var package = new TestPackage(dllPath);
            package.AddSetting(EnginePackageSettings.ProcessModel, "InProcess");
            using var runner = Engine.GetRunner(package);
            var xml = runner.Explore(TestFilter.Empty);

            _exploreCache[dllPath] = (lastWriteUtc, xml);
            return xml;
        }

        // Finds the (fully-qualified class name, method name) pairs matching the given
        // (optional) simple class/method name filters against an already-Explore()'d XML
        // tree. Matches on both the bare class name and the fully-qualified name, since
        // callers historically only ever dealt in simple names (no namespace) - see
        // TestSuitesRepository/AttributeInfoManager.
        //
        // Deliberately resolved to (class, method) NAME pairs, not NUnit's numeric
        // test-case `id` attributes - confirmed by direct testing that those ids are only
        // valid within the exact TestPackage/ITestRunner instance that produced them via
        // Explore()/Run(); reusing an id captured from one instance's Explore() against a
        // different instance's Run() (e.g. a fresh TestPackage built for the actual
        // execution) silently matches nothing. Class/method name filters don't have that
        // problem - NUnit matches them by literal string comparison at filter-evaluation
        // time, not against a precomputed id.
        public static List<(string ClassName, string MethodName)> FindMatchingTestCases(XmlNode exploreResult, string? className, string? methodName)
        {
            var matches = new List<(string, string)>();

            foreach (XmlNode testCase in exploreResult.SelectNodes("//test-case")!)
            {
                var fullClassName = testCase.Attributes?["classname"]?.Value ?? "";
                var simpleClassName = fullClassName.Contains('.')
                    ? fullClassName.Substring(fullClassName.LastIndexOf('.') + 1)
                    : fullClassName;
                var testMethodName = testCase.Attributes?["methodname"]?.Value ?? "";

                var classMatches = string.IsNullOrEmpty(className)
                    || simpleClassName.Equals(className, StringComparison.OrdinalIgnoreCase)
                    || fullClassName.Equals(className, StringComparison.OrdinalIgnoreCase);
                var methodMatches = string.IsNullOrEmpty(methodName)
                    || testMethodName.Equals(methodName, StringComparison.OrdinalIgnoreCase);

                if (classMatches && methodMatches && !string.IsNullOrEmpty(fullClassName) && !string.IsNullOrEmpty(testMethodName))
                    matches.Add((fullClassName, testMethodName));
            }

            return matches.Distinct().ToList();
        }

        // Returns null when className/methodName were given but nothing matched them -
        // callers should treat that as "no such test" rather than silently running
        // everything (TestFilter.Empty would run the whole package instead).
        public static TestFilter? BuildFilter(XmlNode exploreResult, string? className, string? methodName)
        {
            if (string.IsNullOrEmpty(className) && string.IsNullOrEmpty(methodName))
                return TestFilter.Empty;

            var matches = FindMatchingTestCases(exploreResult, className, methodName);
            if (matches.Count == 0)
                return null;

            var clauses = matches.Select(m =>
                $"<and><class>{System.Security.SecurityElement.Escape(m.ClassName)}</class>" +
                $"<method>{System.Security.SecurityElement.Escape(m.MethodName)}</method></and>");
            var filterXml = "<filter><or>" + string.Join("", clauses) + "</or></filter>";
            return new TestFilter(filterXml);
        }

        // The isolated child process couldn't even load NUnit.Framework/the engine's .NET
        // Core driver - happens when the target DLL doesn't have a full publish output
        // (.deps.json + all dependencies) alongside it. Confirmed by direct testing: a bare
        // DLL with no companion .deps.json always fails this way under ProcessModel=Separate,
        // and there's no equivalent of the old .NET Framework "private bin path" fallback for
        // .NET Core's AssemblyLoadContext-based resolution - only a full publish output works.
        //
        // Confirmed (by direct testing) this failure shows up in TWO different shapes
        // depending on exactly which dependency is missing first: sometimes NUnit.Engine
        // throws NUnitEngineException synchronously from Run()/Explore() (missing
        // nunit.framework itself); other times it doesn't throw at all and instead returns
        // a normal-looking XmlNode result with the assembly-level test-suite marked
        // runstate="NotRunnable"/result="Failed" label="Invalid" and a "_SKIPREASON"
        // property explaining the load failure (e.g. missing
        // Microsoft.VisualStudio.TestPlatform.ObjectModel, a VSTest-adapter dependency) -
        // both need to be checked; relying on only one would silently miss the other.
        public static bool IsMissingFrameworkDependency(Exception ex)
        {
            return ex.Message.Contains("Failed to load the NUnit Framework", StringComparison.OrdinalIgnoreCase)
                || ex.InnerException is System.IO.FileNotFoundException
                || ex.InnerException is System.IO.FileLoadException;
        }

        public static bool IsUnrunnableResult(XmlNode runResult)
        {
            var assemblyNode = runResult.SelectSingleNode("//test-suite[@type='Assembly']");
            var runstate = assemblyNode?.Attributes?["runstate"]?.Value;
            var label = assemblyNode?.Attributes?["label"]?.Value;
            return runstate == "NotRunnable" || label == "Invalid";
        }
    }
}
