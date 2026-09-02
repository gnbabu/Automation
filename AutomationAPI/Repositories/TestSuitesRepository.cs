using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.TestRunner;

namespace AutomationAPI.Repositories
{
    public class TestSuitesRepository : ITestSuitesRepository
    {
        private readonly ITestCaseAssignmentRepository _testCaseAssignmentRepository;
        public TestSuitesRepository(ITestCaseAssignmentRepository testCaseAssignmentRepository)
        {
            _testCaseAssignmentRepository = testCaseAssignmentRepository;
        }

        // Discovery via NUnit.Engine's Explore() instead of hand-rolled reflection - this
        // correctly expands [TestCase]/[TestCaseSource]/[Values]/[Range]/[Combinatorial]/
        // [TestFixtureSource]-generated test cases into concrete entries (the old reflection
        // scan only ever saw [Test]/[TestCase]/[TestCaseSource] at the method level, and
        // couldn't expand parameterized-by-attribute tests at all), and reads TestCaseId/
        // Priority/Description directly from NUnit's own [Property(...)] attribute via the
        // explore XML's <properties> element instead of the old fragile "find a 2-arg
        // attribute whose first arg string-matches a key name" convention in
        // AttributeInfoManager (confirmed via the real AutomationTests test projects that
        // they already use real [Property("TestCaseId", ...)] etc., so no test-project
        // changes are needed for this).
        public async Task<IEnumerable<LibraryInfo>> GetLibrariesAsync(string releaseFolderPath)
        {
            return await Task.Run(() =>
            {
                var libraries = new List<LibraryInfo>();

                if (!Directory.Exists(releaseFolderPath))
                    return Enumerable.Empty<LibraryInfo>();

                var dllFiles = Directory.GetFiles(releaseFolderPath, "*.dll");

                foreach (var dllPath in dllFiles)
                {
                    try
                    {
                        var exploreXml = NUnitEngineHelper.Explore(dllPath);
                        var classes = ExploreXmlParser.ParseClasses(exploreXml);

                        if (classes.Any())
                        {
                            libraries.Add(new LibraryInfo
                            {
                                LibraryName = Path.GetFileNameWithoutExtension(dllPath),
                                Classes = classes
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // Not a test assembly, or failed to load (e.g. missing dependency) -
                        // skip it and keep scanning the rest of the folder, same as before.
                        continue;
                    }
                }

                return libraries.AsEnumerable();
            });
        }

        public async Task<IEnumerable<TestCaseModel>> GetAllTestCasesByLibrary(string releaseFolderPath, string libraryName)
        {
            var libraries = await GetLibrariesAsync(releaseFolderPath);

            if (!string.IsNullOrEmpty(libraryName))
            {
                libraries = libraries
                    .Where(l => l.LibraryName.Equals(libraryName, StringComparison.OrdinalIgnoreCase));
            }

            var testCases = libraries
                .SelectMany(lib => lib.Classes, (lib, cls) => new { lib, cls })
                .SelectMany(lc => lc.cls.Methods, (lc, method) => new TestCaseModel
                {
                    LibraryName = lc.lib.LibraryName,
                    ClassName = lc.cls.ClassName,
                    MethodName = method.MethodName,
                    Description = method.Description,
                    Priority = method.Priority,
                    TestCaseId = method.TestCaseId,
                    AssignedUsers = new List<string>() // empty, since not required
                })
                .ToList();

            return testCases;
        }
    }
}
