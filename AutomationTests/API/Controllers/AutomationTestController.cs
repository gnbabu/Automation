using System.Reflection;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutomationTestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<LibraryInfoDto>> GetTestLibraries()
        {

            string _libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestLibs");

            var libraries = new List<LibraryInfoDto>();

            if (!Directory.Exists(_libsPath))
                return NotFound("Test library folder not found.");

            var dllFiles = Directory.GetFiles(_libsPath, "*.dll");

            foreach (var dllPath in dllFiles)
            {
                try
                {
                    Console.WriteLine($"Loading assembly: {dllPath}");
                    // Load test assembly
                    var assembly = Assembly.LoadFrom(dllPath);

                    var testClasses = assembly.GetTypes()
                        .Where(t => t.IsClass && t.IsPublic &&
                            t.GetCustomAttributes<TestFixtureAttribute>().Any()) // Match by attribute type
                        .Select(t => new ClassInfoDto
                        {
                            ClassName = t.FullName,
                            Methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                       .Where(m => m.GetCustomAttributes<TestAttribute>().Any() ||
                                                   m.GetCustomAttributes<TestCaseAttribute>().Any() ||
                                                   m.GetCustomAttributes<TestCaseSourceAttribute>().Any())
                                       .Select(m => new MethodInfoDto { MethodName = m.Name })
                                       .ToList()
                        })
                        .Where(c => c.Methods.Any())
                        .ToList();

                    if (testClasses.Any())
                    {
                        libraries.Add(new LibraryInfoDto
                        {
                            LibraryName = Path.GetFileNameWithoutExtension(dllPath),
                            Classes = testClasses
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "Failed to load test libraries.",
                        Error = ex.Message
                    });
                }
            }

            return Ok(new { Libraries = libraries, LoadedDlls = dllFiles });
        }
    }
}

