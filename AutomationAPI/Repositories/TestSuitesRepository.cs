﻿using System.Collections;
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
            var libraries = new List<LibraryInfo>();
            int testCaseCounter = 1; // Sequential counter for IDs

            return await Task.Run(() =>
            {
                if (!Directory.Exists(_libsPath))
                    return libraries.AsEnumerable();

                var dllFiles = Directory.GetFiles(_libsPath, "*.dll");

                foreach (var dllPath in dllFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(dllPath);

                        var testClasses = assembly.GetTypes()
                            .Where(t => t.IsClass && t.IsPublic &&
                                        t.GetCustomAttributes(typeof(TestFixtureAttribute), false).Any())
                            .Select(t => new ClassInfo
                            {

                                ClassName = t.Name,
                                Methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                    .Where(m => m.GetCustomAttributes().Any(a =>
                                        a is TestAttribute ||
                                        a is TestCaseAttribute ||
                                        a is TestCaseSourceAttribute))
                                    .Select(m =>
                                    {
                                        int currentId = testCaseCounter++; // assign sequential ID

                                        // Description
                                        var descAttr = m.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                                                        .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                                        string description = descAttr?.Description;

                                        string? priority = null;
                                        string? testCaseId = null;

                                        // Extract PropertyAttribute safely
                                        var propertyAttrs = m.GetCustomAttributes(true)
                                                             .OfType<PropertyAttribute>();

                                        foreach (var propAttr in propertyAttrs)
                                        {
                                            // Access non-public Properties dictionary
                                            var propertiesProp = propAttr.GetType()
                                                .GetProperty("Properties", BindingFlags.NonPublic | BindingFlags.Instance);

                                            if (propertiesProp?.GetValue(propAttr) is IDictionary dict)
                                            {
                                                if (dict.Contains("Priority"))
                                                    priority = dict["Priority"]?.ToString();
                                                if (dict.Contains("TestCaseId"))
                                                    testCaseId = dict["TestCaseId"]?.ToString();
                                            }
                                        }

                                        return new LibraryMethodInfo
                                        {
                                            MethodName = m.Name,
                                            Description = description ?? "Description of " + m.Name,
                                            Priority = priority ?? "High",
                                            TestCaseId = testCaseId ?? $"TC-{currentId}"  // fallback uses same sequential ID
                                        };
                                    })
                                    .Where(mi => !string.IsNullOrEmpty(mi.MethodName))
                                    .ToList()
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
                        Console.WriteLine($"Failed to load {dllPath}: {ex.Message}");
                        continue;
                    }
                }

                return libraries.AsEnumerable();
            });
        }

        public async Task<IEnumerable<TestCaseModel>> GetAllTestCasesAsync(string? libraryName, bool? assigned)
        {
            var libraries = await GetLibrariesAsync();

            // Await the async repository call
            var testCaseAssignments = await _testCaseAssignmentRepository.GetAllAssignmentsAsync();

            // Filter by library name if provided
            if (!string.IsNullOrEmpty(libraryName))
            {
                libraries = libraries.Where(l => l.LibraryName.Equals(libraryName, StringComparison.OrdinalIgnoreCase));
            }

            var testCases = new List<TestCaseModel>();

            foreach (var lib in libraries)
            {
                foreach (var cls in lib.Classes)
                {
                    foreach (var method in cls.Methods)
                    {
                        // Get assigned users safely
                        var assignedUsers = testCaseAssignments
                            .Where(a =>
                                a.LibraryName.Equals(lib.LibraryName, StringComparison.OrdinalIgnoreCase) &&
                                a.ClassName.Equals(cls.ClassName, StringComparison.OrdinalIgnoreCase) &&
                                a.MethodName.Equals(method.MethodName, StringComparison.OrdinalIgnoreCase))
                            .Select(a => a.UserId.ToString())
                            .ToList();

                        // Apply assigned/unassigned filter
                        if (assigned.HasValue)
                        {
                            if (assigned.Value && (assignedUsers == null || !assignedUsers.Any())) continue; // Only assigned
                            if (!assigned.Value && assignedUsers.Any()) continue; // Only unassigned
                        }

                        testCases.Add(new TestCaseModel
                        {
                            LibraryName = lib.LibraryName,
                            ClassName = cls.ClassName,
                            MethodName = method.MethodName,
                            Description = method.Description,
                            Priority = method.Priority,
                            TestCaseId = method.TestCaseId,
                            AssignedUsers = assignedUsers ?? new List<string>()
                        });
                    }
                }
            }

            return testCases;
        }



    }
}
