using AutomationAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class TestSuitesController : ControllerBase
    {
        private readonly ITestSuitesRepository _testSuitesRepository;

        public TestSuitesController(ITestSuitesRepository testSuitesRepository)
        {
            _testSuitesRepository = testSuitesRepository;
        }

        [HttpGet("libraries")]
        public async Task<IActionResult> GetTestLibraries()
        {
            var libraries = await _testSuitesRepository.GetLibrariesAsync();
            return Ok(libraries);
        }

        /// <summary>
        /// Get all test methods with optional filters
        /// </summary>
        /// <param name="libraryName">Optional: filter by library name</param>
        /// <param name="assigned">Optional: true = assigned, false = unassigned, null = all</param>
        [HttpGet("GetAllTestCasesByLibrary")]
        public async Task<IActionResult> GetAllTestCasesByLibrary([FromQuery] string libraryName)
        {
            var testCases = await _testSuitesRepository.GetAllTestCasesByLibrary(libraryName);
            return Ok(testCases);
        }
    }
}
