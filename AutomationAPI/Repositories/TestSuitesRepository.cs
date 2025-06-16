using System.Reflection;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories
{
    public class TestSuitesRepository : ITestSuitesRepository
    {
        public string _libsPath = string.Empty;
        public TestSuitesRepository(IConfiguration configuration)
        {
            _libsPath = configuration["TestSettings:TestLibsPath"];
        }
        public async Task<IEnumerable<LibraryInfo>> GetLibrariesAsync()
        {
            //string libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestLibs");
            var libraries = new List<LibraryInfo>();

            return await Task.Run(() =>
            {
                if (!Directory.Exists(_libsPath))
                    return Enumerable.Empty<LibraryInfo>();

                var dllFiles = Directory.GetFiles(_libsPath, "*.dll");

                foreach (var dllPath in dllFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(dllPath);

                        var testClasses = assembly.GetTypes()
                            .Where(t => t.IsClass && t.IsPublic &&
                                t.GetCustomAttributes(typeof(NUnit.Framework.TestFixtureAttribute), false).Any())
                            .Select(t => new ClassInfo
                            {
                                ClassName = t.Name,
                                Methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                    .Where(m =>
                                        m.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), false).Any() ||
                                        m.GetCustomAttributes(typeof(NUnit.Framework.TestCaseAttribute), false).Any() ||
                                        m.GetCustomAttributes(typeof(NUnit.Framework.TestCaseSourceAttribute), false).Any())
                                    .Select(m => new LibraryMethodInfo
                                    {
                                        MethodName = m.Name
                                    }).ToList()
                            })
                            .Where(c => c.Methods.Any())
                            .ToList();

                        if (testClasses.Any())
                        {
                            libraries.Add(new LibraryInfo
                            {
                                LibraryName = Path.GetFileNameWithoutExtension(dllPath),
                                Classes = testClasses
                            });
                        }
                    }
                    catch
                    {
                        // Log the error if needed; skip the faulty assembly
                        continue;
                    }
                }

                return libraries.AsEnumerable();
            });
        }

    }
}
