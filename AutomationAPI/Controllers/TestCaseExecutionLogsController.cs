using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCaseExecutionLogsController : ControllerBase
    {
        private readonly ITestCaseExecutionLogRepository _repo;

        public TestCaseExecutionLogsController(ITestCaseExecutionLogRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TestCaseExecutionLog log)
        {

            // Consistency validation
            if (log.LogLevel == TestCaseLogLevel.Fail && log.ExecutionStatus != ExecutionStatus.Failed)
                return BadRequest("Fail log must have ExecutionStatus = Failed");

            if (log.LogLevel == TestCaseLogLevel.Warning && log.ExecutionStatus == ExecutionStatus.Failed)
                return BadRequest("Warning log cannot mark execution as Failed");


            await _repo.AddAsync(log);
            return Ok();
        }

        [HttpGet("assignments/{assignmentId}/testcases/{assignmentTestCaseId}/logs")]
        public async Task<IActionResult> GetTestCaseLogs(int assignmentId, int assignmentTestCaseId)
        {
            var logs = await _repo.GetByTestCaseAsync(assignmentId, assignmentTestCaseId);
            return Ok(logs);
        }

        [HttpGet("assignments/{assignmentId}/logs")]
        public async Task<IActionResult> GetExecutionLogs(int assignmentId)
        {
            var logs = await _repo.GetByAssignmentAsync(assignmentId);
            return Ok(logs);
        }

        [HttpGet("releases/{releaseName}/logs")]
        public async Task<IActionResult> GetReleaseLogs(string releaseName)
        {
            var logs = await _repo.GetByReleaseAsync(releaseName);
            return Ok(logs);
        }
    }
}
