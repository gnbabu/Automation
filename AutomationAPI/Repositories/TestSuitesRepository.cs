using System.Collections;
using System.ComponentModel;
using System.Reflection;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Castle.Core.Internal;
using NUnit.Framework;

namespace AutomationAPI.Repositories
{
    public class TestSuitesRepository : ITestSuitesRepository
    {
        public string _libsPath = string.Empty;
        private readonly ITestCaseAssignmentRepository _testCaseAssignmentRepository;
        public TestSuitesRepository(IConfiguration configuration, ITestCaseAssignmentRepository testCaseAssignmentRepository)
        {
            _libsPath = configuration["TestSettings:TestLibsPath"];
            _testCaseAssignmentRepository = testCaseAssignmentRepository;
        }

        public async Task<IEnumerable<LibraryInfo>> GetLibrariesAsync()
        {
            List<LibraryMethodInfo> libraryMethodInfos = GetMethodAttributes();

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
                    catch (Exception ex)
                    {
                        // Log the error if needed; skip the faulty assembly
                        continue;
                    }
                }

                if (libraryMethodInfos != null && libraryMethodInfos.Count() > 0)
                {
                    foreach (var lib in libraries)
                    {
                        foreach (var cls in lib.Classes)
                        {
                            foreach (var method in cls.Methods)
                            {
                                var matchedMethod = libraryMethodInfos.FirstOrDefault(m => m.MethodName == method.MethodName);
                                if (matchedMethod != null)
                                {
                                    method.TestCaseId = matchedMethod.TestCaseId;
                                    method.Priority = matchedMethod.Priority;
                                    method.Description = matchedMethod.Description;
                                }
                            }
                        }
                    }
                }

                return libraries.AsEnumerable();
            });
        }

        public async Task<IEnumerable<TestCaseModel>> GetAllTestCasesByLibrary(string libraryName)
        {
            var libraries = await GetLibrariesAsync();

            // Filter by library name if provided
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

        public List<LibraryMethodInfo> GetMethodAttributes()
        {
            var results = new List<LibraryMethodInfo>();

            if (!Directory.Exists(_libsPath))
                return null;

            var dllFiles = Directory.GetFiles(_libsPath, "*.dll");

            Type[] types;
            try
            {
                foreach (var dllPath in dllFiles)
                {
                    var assembly = Assembly.LoadFrom(dllPath);

                    var classesWithMethods = assembly.GetTypes()
                            .Where(t => t.IsClass && t.IsPublic &&
                                        t.GetCustomAttributes(typeof(NUnit.Framework.TestFixtureAttribute), inherit: false).Any())
                            .Select(t => new
                            {
                                Type = t,
                                Methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                            .Where(m =>
                                                m.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), false).Any() ||
                                                m.GetCustomAttributes(typeof(NUnit.Framework.TestCaseAttribute), false).Any() ||
                                                m.GetCustomAttributes(typeof(NUnit.Framework.TestCaseSourceAttribute), false).Any())
                                            .ToArray() // <-- MethodInfo[]
                            })
                            .Where(x => x.Methods.Length > 0)
                            .ToList();


                    foreach (var cls in classesWithMethods)
                    {
                        foreach (var methodInfo in cls.Methods)
                        {
                            LibraryMethodInfo libraryMethodInfo = new LibraryMethodInfo();

                            var libMethod = AttributeInfo.BuildLibraryMethodInfo(methodInfo);
                            foreach (var attr in libMethod.Attributes)
                            {
                                libraryMethodInfo.MethodName = libMethod.MethodName;

                                // If the attribute stores key-value pairs in NamedArguments dictionary:
                                foreach (var kvp in attr.NamedArguments)
                                {
                                    string key = kvp.Key;
                                    object? value = kvp.Value;
                                    Console.WriteLine($"Attribute: {attr.AttributeTypeShortName}, Key: {key}, Value: {value}");
                                }
                                if (attr.ConstructorArguments != null && attr.ConstructorArguments.Length == 2)
                                {
                                    var key = attr.ConstructorArguments[0];
                                    var value = attr.ConstructorArguments[1];

                                    if (key != null && value != null)
                                    {
                                        if (key.Equals("TestCaseId"))
                                        {
                                            libraryMethodInfo.TestCaseId = value.ToString() ?? string.Empty;
                                        }
                                        if (key.Equals("Priority"))
                                        {
                                            libraryMethodInfo.Priority = value.ToString() ?? string.Empty;
                                        }
                                        if (key.Equals("Description"))
                                        {
                                            libraryMethodInfo.Description = value.ToString() ?? string.Empty;
                                        }
                                    }
                                }
                            }
                            results.Add(libraryMethodInfo);
                        }
                    }

                }
            }
            catch (ReflectionTypeLoadException rtlex)
            {
                types = rtlex.Types.Where(t => t != null).ToArray()!;
            }


            return results;
        }

    }
}
