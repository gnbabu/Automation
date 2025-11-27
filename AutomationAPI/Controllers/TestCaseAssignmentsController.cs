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
        private readonly ILogger<TestCaseAssignmentsController> _logger;

        public TestCaseAssignmentsController(ITestCaseAssignmentRepository repository,
                                            ILogger<TestCaseAssignmentsController> logger)
        {
            _repository = repository;
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

    }
}

