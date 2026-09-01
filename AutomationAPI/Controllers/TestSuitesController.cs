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
        private readonly IReleaseRepository _releaseRepository;

        public TestSuitesController(ITestSuitesRepository testSuitesRepository, IReleaseRepository releaseRepository)
        {
            _testSuitesRepository = testSuitesRepository;
            _releaseRepository = releaseRepository;
        }

        /// <summary>
        /// Get all test libraries (DLLs) discovered in the given Release's own folder.
        /// </summary>
        /// <param name="releaseId">Release whose folder should be scanned</param>
        [HttpGet("libraries")]
        public async Task<IActionResult> GetTestLibraries([FromQuery] int releaseId)
        {
            if (releaseId <= 0)
                return BadRequest("releaseId is required.");

            var release = await _releaseRepository.GetByIdAsync(releaseId);
            if (release == null)
                return NotFound($"Release {releaseId} not found.");

            if (string.IsNullOrWhiteSpace(release.ReleaseFolderPath))
                return BadRequest("This release does not have a folder path configured yet.");

            var libraries = await _testSuitesRepository.GetLibrariesAsync(release.ReleaseFolderPath);
            return Ok(libraries);
        }

        /// <summary>
        /// Get all test methods with optional filters, scoped to a Release's own folder
        /// </summary>
        /// <param name="releaseId">Release whose folder should be scanned</param>
        /// <param name="libraryName">Optional: filter by library name</param>
        /// <param name="assigned">Optional: true = assigned, false = unassigned, null = all</param>
        [HttpGet("GetAllTestCasesByLibrary")]
        public async Task<IActionResult> GetAllTestCasesByLibrary([FromQuery] int releaseId, [FromQuery] string libraryName)
        {
            if (releaseId <= 0)
                return BadRequest("releaseId is required.");

            var release = await _releaseRepository.GetByIdAsync(releaseId);
            if (release == null)
                return NotFound($"Release {releaseId} not found.");

            if (string.IsNullOrWhiteSpace(release.ReleaseFolderPath))
                return BadRequest("This release does not have a folder path configured yet.");

            var testCases = await _testSuitesRepository.GetAllTestCasesByLibrary(release.ReleaseFolderPath, libraryName);
            return Ok(testCases);
        }
    }
}
