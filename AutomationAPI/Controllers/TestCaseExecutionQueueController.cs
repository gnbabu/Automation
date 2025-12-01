using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]    
    public class TestCaseExecutionQueueController : ControllerBase
    {
        private readonly ITestCaseExecutionQueueRepository _repo;
        private readonly ILogger<TestCaseExecutionQueueController> _logger;

        public TestCaseExecutionQueueController(
            ITestCaseExecutionQueueRepository repo,
            ILogger<TestCaseExecutionQueueController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpPost("single-run")]
        public async Task<IActionResult> SingleRunNow([FromBody] SingleRunNowRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body cannot be null.");

                if (request.AssignmentId <= 0 || request.AssignmentTestCaseId <= 0)
                    return BadRequest("AssignmentId and AssignmentTestCaseId must be valid.");

                var result = await _repo.SingleRunNowAsync(
                    request.AssignmentId,
                    request.AssignmentTestCaseId,
                    request.Browser
                );

                return Ok(new QueueCreateResponse
                {
                    Id = result.Id,
                    QueueId = result.QueueId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in SingleRunNow | AssignmentId: {AssignmentId}, TestCase: {TestCaseId}",
                    request?.AssignmentId, request?.AssignmentTestCaseId);

                return StatusCode(500, "An unexpected error occurred while running test case.");
            }
        }

        [HttpPost("bulk-run")]
        public async Task<IActionResult> BulkRunNow([FromBody] BulkRunNowRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body cannot be null.");

                if (request.AssignmentId <= 0)
                    return BadRequest("AssignmentId is invalid.");

                if (request.AssignmentTestCaseIds == null || !request.AssignmentTestCaseIds.Any())
                    return BadRequest("At least one AssignmentTestCaseId is required.");

                var success = await _repo.BulkRunNowAsync(
                    request.AssignmentId,
                    request.AssignmentTestCaseIds,
                    request.Browser
                );

                return Ok(new { Success = success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in BulkRunNow | AssignmentId: {AssignmentId}, Count: {Count}",
                    request?.AssignmentId, request?.AssignmentTestCaseIds?.Count);

                return StatusCode(500, "An unexpected error occurred while bulk running test cases.");
            }
        }
       
        [HttpPost("single-schedule")]
        public async Task<IActionResult> SingleSchedule([FromBody] SingleScheduleRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body cannot be null.");

                if (request.AssignmentId <= 0 || request.AssignmentTestCaseId <= 0)
                    return BadRequest("AssignmentId and AssignmentTestCaseId must be valid.");

                if (request.ScheduleDate == default)
                    return BadRequest("ScheduleDate is invalid.");

                var result = await _repo.SingleScheduleAsync(
                    request.AssignmentId,
                    request.AssignmentTestCaseId,
                    request.ScheduleDate,
                    request.Browser
                );

                return Ok(new QueueCreateResponse
                {
                    Id = result.Id,
                    QueueId = result.QueueId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in SingleSchedule | AssignmentId: {AssignmentId}, TestCase: {TestCaseId}",
                    request?.AssignmentId, request?.AssignmentTestCaseId);

                return StatusCode(500, "An unexpected error occurred while scheduling test case.");
            }
        }
        
        [HttpPost("bulk-schedule")]
        public async Task<IActionResult> BulkSchedule([FromBody] BulkScheduleRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body cannot be null.");

                if (request.AssignmentId <= 0)
                    return BadRequest("AssignmentId is invalid.");

                if (request.AssignmentTestCaseIds == null || !request.AssignmentTestCaseIds.Any())
                    return BadRequest("At least one AssignmentTestCaseId is required.");

                if (request.ScheduleDate == default)
                    return BadRequest("ScheduleDate is invalid.");

                var success = await _repo.BulkScheduleAsync(
                    request.AssignmentId,
                    request.AssignmentTestCaseIds,
                    request.ScheduleDate,
                    request.Browser
                );

                return Ok(new { Success = success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in BulkSchedule | AssignmentId: {AssignmentId}, Count: {Count}",
                    request?.AssignmentId, request?.AssignmentTestCaseIds?.Count);

                return StatusCode(500, "An unexpected error occurred while scheduling test cases.");
            }
        }
    }
}
