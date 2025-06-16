using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestResultsController : ControllerBase
    {
        private readonly ITestResultRepository _testResultRepository;
        private readonly ILogger<TestResultsController> _logger;

        public TestResultsController(ITestResultRepository testResultRepository, ILogger<TestResultsController> logger)
        {
            _testResultRepository = testResultRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets paged test results with optional filtering and sorting.
        /// </summary>
        /// <param name="payload">Paging, sorting, and filter parameters.</param>
        /// <returns>Paged list of test results.</returns>
        [HttpPost("search")] // Use POST because you’re sending filter params in the body
        public async Task<ActionResult<PagedResult<TestResult>>> SearchTestResultsAsync([FromBody] TestResultPayload payload)
        {
            try
            {
                if (payload == null)
                    return BadRequest("Payload cannot be null.");

                var pagedResult = await _testResultRepository.SearchTestResultsAsync(payload);

                if (pagedResult == null || !pagedResult.Data.Any())
                {
                    return NotFound("No test results found.");
                }

                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching paged test results.");
                return StatusCode(500, "Internal server error.");
            }
        }



        /// <summary>
        /// Gets all test results.
        /// </summary>
        /// <returns>A list of all test results.</returns>
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetAllTestResultsAsync()
        {
            try
            {
                var results = await _testResultRepository.GetAllTestResultsAsync();

                if (results == null || !results.Any())
                {
                    return NotFound("No test results found.");
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all test results.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Gets test results by the specified QueueId.
        /// </summary>
        /// <param name="queueId">The QueueId for which to fetch test results.</param>
        /// <returns>A list of test results.</returns>
        [HttpGet("{queueId}")]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetTestResultsByQueueIdAsync(string queueId)
        {
            try
            {
                var results = await _testResultRepository.GetTestResultsByQueueIdAsync(queueId);

                if (results == null || !results.Any())
                {
                    return NotFound($"No test results found for QueueId: {queueId}");
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching test results for QueueId: {QueueId}", queueId);
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Inserts a new test result.
        /// </summary>
        /// <param name="testResult">The test result to insert.</param>
        /// <returns>The ID of the newly inserted test result.</returns>
        [HttpPost]
        public async Task<ActionResult<int>> InsertTestResultsAsync([FromBody] TestResult testResult)
        {
            if (testResult == null)
            {
                return BadRequest("Test result data is required.");
            }

            try
            {
                int resultId = await _testResultRepository.InsertTestResultsAsync(testResult);

                return Ok($"TestResult created successfully!: {resultId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting a test result.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
