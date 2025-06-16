using System.Reflection;
using NUnit.Framework;

namespace AutomationAPI.Repositories.TestRunner
{
    public class ReflectionTestRunner : ITestRunner
    {
        public string _libsPath = string.Empty;
        public ReflectionTestRunner(IConfiguration configuration)
        {
            _libsPath = configuration["TestSettings:TestLibsPath"];
        }


        public async Task<List<TestExecutionResult>> RunAsyncWorking(string? library, string? className, string? methodName)
        {
            var results = new List<TestExecutionResult>();
            var dllFiles = Directory.GetFiles(_libsPath, "*.dll");

            foreach (var dllPath in dllFiles.Where(d => d.Contains("OnboardingTests")))
            {
                var assembly = Assembly.LoadFrom(dllPath);
                Console.WriteLine($"Loaded assembly: {dllPath}");

                var types = assembly.GetTypes()
                                    .Where(t => t.IsClass && t.IsPublic &&
                                        t.GetCustomAttributes(typeof(NUnit.Framework.TestFixtureAttribute), false).Any());

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                 .Where(m =>
                                 {
                                     // Log method names and attributes for debugging
                                     Console.WriteLine($"Checking method: {m.Name}");

                                     bool hasTestAttribute = m.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), true).Any();
                                     bool hasTestCaseAttribute = m.GetCustomAttributes(typeof(NUnit.Framework.TestCaseAttribute), true).Any();
                                     bool hasTestCaseSourceAttribute = m.GetCustomAttributes(typeof(NUnit.Framework.TestCaseSourceAttribute), true).Any();

                                     Console.WriteLine($"  TestAttribute: {hasTestAttribute}, TestCaseAttribute: {hasTestCaseAttribute}, TestCaseSourceAttribute: {hasTestCaseSourceAttribute}");

                                     return hasTestAttribute || hasTestCaseAttribute || hasTestCaseSourceAttribute;
                                 })
                                 .ToList();


                    var setupMethods = type.GetMethods()
                        .Where(m => m.GetCustomAttribute<SetUpAttribute>() != null)
                        .ToList();

                    var tearDownMethods = type.GetMethods()
                        .Where(m => m.GetCustomAttribute<TearDownAttribute>() != null)
                        .ToList();

                    foreach (var method in methods)
                    {
                        var instance = Activator.CreateInstance(type);
                        if (instance == null)
                        {
                            results.Add(new TestExecutionResult
                            {
                                Name = method.Name,
                                ClassName = type.Name,
                                Passed = false,
                                Message = $"Could not instantiate type {type.FullName}."
                            });
                            continue;
                        }

                        var result = new TestExecutionResult
                        {
                            Name = method.Name,
                            ClassName = type.Name,
                            StartTime = DateTime.UtcNow
                        };

                        try
                        {
                            // Run SetUp methods
                            foreach (var setup in setupMethods)
                            {
                                setup.Invoke(instance, null);
                            }

                            // Run the test method
                            method.Invoke(instance, null);
                            result.Passed = true;
                        }
                        catch (Exception ex)
                        {
                            result.Passed = false;
                            result.Message = ex.ToString();
                        }
                        finally
                        {
                            // Run TearDown methods
                            foreach (var tearDown in tearDownMethods)
                            {
                                try
                                {
                                    tearDown.Invoke(instance, null);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"TearDown failed: {ex.Message}");
                                }
                            }

                            result.EndTime = DateTime.UtcNow;
                            results.Add(result);
                        }
                    }
                }
            }

            return await Task.FromResult(results);
        }



        public async Task<List<TestExecutionResult>> RunAsync(string? library, string? className, string? methodName)
        {
            var results = new List<TestExecutionResult>();
            var dllFiles = Directory.GetFiles(_libsPath, "*.dll");

            foreach (var dllPath in dllFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(dllPath);
                if (!string.IsNullOrEmpty(library) && !fileName.Equals(library, StringComparison.OrdinalIgnoreCase))
                    continue;

                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(dllPath);
                }
                catch (Exception ex)
                {
                    results.Add(new TestExecutionResult
                    {
                        Name = "AssemblyLoad",
                        ClassName = fileName,
                        Passed = false,
                        Message = ex.Message,
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow
                    });
                    continue;
                }

                // Global SetUpFixture support
                //var setupFixtures = assembly.GetTypes()
                //    .Where(t => t.GetCustomAttribute<NUnit.Framework.SetUpFixtureAttribute>() != null);
                //foreach (var fixture in setupFixtures)
                //{
                //    var instance = Activator.CreateInstance(fixture);
                //    fixture.GetMethod("OneTimeSetUp")?.Invoke(instance, null);
                //}

                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && t.IsPublic &&
                                t.GetCustomAttributes(typeof(NUnit.Framework.TestFixtureAttribute), true).Any());

                foreach (var type in types)
                {
                    if (!string.IsNullOrEmpty(className) && !type.Name.Equals(className, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var setupMethods = type.GetMethods().Where(m => m.GetCustomAttribute<NUnit.Framework.SetUpAttribute>() != null).ToList();
                    var tearDownMethods = type.GetMethods().Where(m => m.GetCustomAttribute<NUnit.Framework.TearDownAttribute>() != null).ToList();

                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(m =>
                            m.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), true).Any() ||
                            m.GetCustomAttributes(typeof(NUnit.Framework.TestCaseAttribute), true).Any() ||
                            m.GetCustomAttributes(typeof(NUnit.Framework.TestCaseSourceAttribute), true).Any())
                        .Where(m => string.IsNullOrEmpty(methodName) || m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var method in methods)
                    {
                        var instance = Activator.CreateInstance(type);

                        // Handle TestCase attributes
                        var testCases = method.GetCustomAttributes(typeof(NUnit.Framework.TestCaseAttribute), true)
                                              .Cast<NUnit.Framework.TestCaseAttribute>()
                                              .Select(attr => attr.Arguments)
                                              .DefaultIfEmpty(null);

                        // Handle TestCaseSource
                        var testCaseSources = method.GetCustomAttributes(typeof(NUnit.Framework.TestCaseSourceAttribute), true)
                            .Cast<NUnit.Framework.TestCaseSourceAttribute>()
                            .SelectMany(attr =>
                            {
                                var member = type.GetMember(attr.SourceName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault();
                                return member switch
                                {
                                    FieldInfo fi => (IEnumerable<object[]>)fi.GetValue(null),
                                    PropertyInfo pi => (IEnumerable<object[]>)pi.GetValue(null),
                                    MethodInfo mi => (IEnumerable<object[]>)mi.Invoke(null, null),
                                    _ => new List<object[]>()
                                };
                            });

                        var allTestCases = testCases.Concat(testCaseSources.Any() ? testCaseSources : Enumerable.Empty<object[]>());

                        if (!allTestCases.Any() || allTestCases.First() == null)
                            allTestCases = new List<object[]> { null }; // Regular [Test] method

                        foreach (var args in allTestCases)
                        {
                            var result = new TestExecutionResult
                            {
                                Name = method.Name,
                                ClassName = type.Name,
                                StartTime = DateTime.UtcNow
                            };

                            try
                            {
                                foreach (var setup in setupMethods)
                                    setup.Invoke(instance, null);

                                method.Invoke(instance, args);

                                foreach (var tearDown in tearDownMethods)
                                    tearDown.Invoke(instance, null);

                                result.Passed = true;
                            }
                            catch (Exception ex)
                            {
                                result.Passed = false;
                                result.Message = ex.InnerException?.Message ?? ex.Message;
                            }

                            result.EndTime = DateTime.UtcNow;
                            results.Add(result);
                        }
                    }
                }

                //// Global TearDown for SetUpFixtures
                //foreach (var fixture in setupFixtures)
                //{
                //    var instance = Activator.CreateInstance(fixture);
                //    fixture.GetMethod("OneTimeTearDown")?.Invoke(instance, null);
                //}
            }

            return await Task.FromResult(results);
        }



        //public async Task<List<TestExecutionResult>> RunAsync(string? library, string? className, string? methodName)
        //{
        //    var results = new List<TestExecutionResult>();
        //    var assemblies = new List<Assembly>();

        //    if (_libsPath == null)
        //    {
        //        assemblies = AppDomain.CurrentDomain.GetAssemblies()
        //            .Where(a => a.GetName().Name == library || library == null)
        //            .ToList();
        //    }
        //    else if (Directory.Exists(_libsPath))
        //    {
        //        var dllFiles = Directory.GetFiles(_libsPath, "*.dll");
        //        foreach (var dll in dllFiles)
        //        {
        //            try
        //            {
        //                var assembly = Assembly.LoadFrom(dll);
        //                if (library == null || assembly.GetName().Name == library)
        //                {
        //                    assemblies.Add(assembly);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Failed to load assembly from {dll}: {ex.Message}");
        //            }
        //        }
        //    }

        //    foreach (var assembly in assemblies)
        //    {
        //        var types = assembly.GetTypes()
        //            .Where(t => (className == null || t.Name == className) && t.IsClass && t.GetCustomAttribute<TestFixtureAttribute>() != null);

        //        foreach (var type in types)
        //        {
        //            var methods = type.GetMethods()
        //                .Where(m =>
        //                    (m.GetCustomAttribute<TestAttribute>() != null || m.GetCustomAttribute<TestCaseAttribute>() != null) &&
        //                    (methodName == null || m.Name == methodName));

        //            var setupMethods = type.GetMethods().Where(m => m.GetCustomAttribute<SetUpAttribute>() != null).ToList();
        //            var tearDownMethods = type.GetMethods().Where(m => m.GetCustomAttribute<TearDownAttribute>() != null).ToList();

        //            foreach (var method in methods)
        //            {
        //                var instance = Activator.CreateInstance(type);
        //                var result = new TestExecutionResult
        //                {
        //                    Name = method.Name,
        //                    ClassName = type.Name,
        //                    StartTime = DateTime.UtcNow
        //                };

        //                try
        //                {
        //                    // Run [SetUp] methods
        //                    foreach (var setup in setupMethods)
        //                        setup.Invoke(instance, null);

        //                    // Run the test method
        //                    method.Invoke(instance, null);
        //                    result.Passed = true;

        //                    // Run [TearDown] methods
        //                    foreach (var tearDown in tearDownMethods)
        //                        tearDown.Invoke(instance, null);
        //                }
        //                catch (Exception ex)
        //                {
        //                    result.Passed = false;
        //                    result.Message = ex.InnerException?.Message ?? ex.Message;
        //                }

        //                result.EndTime = DateTime.UtcNow;
        //                results.Add(result);
        //            }
        //        }
        //    }

        //    return await Task.FromResult(results);
        //}
    }

}
