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
        [HttpGet("GetAllTestCases")]
        public async Task<IActionResult> GetAllTestCases([FromQuery] string? libraryName, [FromQuery] bool? assigned)
        {
            var testCases = await _testSuitesRepository.GetAllTestCasesAsync(libraryName, assigned);
            return Ok(testCases);
        }
    }
}
