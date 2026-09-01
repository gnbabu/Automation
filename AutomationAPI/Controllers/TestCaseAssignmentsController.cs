using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCaseAssignmentsController : ControllerBase
    {
        private readonly ITestCaseAssignmentRepository _repository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly ILogger<TestCaseAssignmentsController> _logger;

        private static readonly string[] AssignableReleaseLifecycles = { "Active", "Completed" };

        public TestCaseAssignmentsController(ITestCaseAssignmentRepository repository,
                                            IReleaseRepository releaseRepository,
                                            ILogger<TestCaseAssignmentsController> logger)
        {
            _repository = repository;
            _releaseRepository = releaseRepository;
            _logger = logger;
        }

        [HttpGet("{assignedUserId}/{assignmentName}")]
        public async Task<IActionResult> GetTestCasesByAssignmentAndUserAsync(int assignedUserId, string assignmentName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(assignmentName))
                    return BadRequest("AssignmentName is required.");

                var testCases = await _repository.GetTestCasesByAssignmentNameAndUserAsync(assignmentName, assignedUserId);

                return Ok(testCases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching test cases for assignment and user");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdateAssignmentWithTestCasesAsync([FromBody] AssignmentCreateUpdateRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request: Assignment is missing.");

                if (request.ReleaseId <= 0)
                    return BadRequest("ReleaseId is required.");

                var release = await _releaseRepository.GetByIdAsync(request.ReleaseId);
                if (release == null)
                    return BadRequest($"Release {request.ReleaseId} not found.");

                if (!AssignableReleaseLifecycles.Contains(release.ReleaseLifecycle, StringComparer.OrdinalIgnoreCase))
                    return BadRequest("Test cases can only be assigned against an Active or Completed release.");

                // Test cases can be optional now — no validation needed
                // request.TestCases can be null → backend handles it

                await _repository.CreateOrUpdateAssignmentWithTestCasesAsync(request);

                return Ok(new
                {
                    Message = "Assignment and test cases synced successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while syncing assignment with test cases");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("library-assigned-testcases")]
        public async Task<IActionResult> GetAllAssignedTestCasesInLibraryAsync(string libraryName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(libraryName))
                    return BadRequest("LibraryName is required.");

                var testCases = await _repository.GetAllAssignedTestCasesInLibraryAsync(libraryName);

                return Ok(testCases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching assigned test cases for library");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("library-environment-assigned-testcases")]
        public async Task<IActionResult> GetAssignedTestCasesForLibraryAsync(string libraryName, string environment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(libraryName))
                    return BadRequest("LibraryName is required.");

                if (string.IsNullOrWhiteSpace(environment))
                    return BadRequest("Environment is required.");

                var testCases = await _repository.GetAssignedTestCasesForLibraryAndEnvironmentAsync(libraryName, environment);

                return Ok(testCases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching assigned test cases for library");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("library-release-assigned-testcases")]
        public async Task<IActionResult> GetAssignedTestCasesForLibraryAndReleaseAsync(string libraryName, int releaseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(libraryName))
                    return BadRequest("LibraryName is required.");

                if (releaseId <= 0)
                    return BadRequest("ReleaseId is required.");

                var testCases = await _repository.GetAssignedTestCasesForLibraryAndReleaseAsync(libraryName, releaseId);

                return Ok(testCases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching assigned test cases for library and release");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("assigned-to/{userId}")]
        public async Task<IActionResult> GetAssignmentsByUserIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest("UserId must be greater than zero.");

                var assignments = await _repository.GetAssignmentsByUserIdAsync(userId);

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching assignments for the specified user.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}

