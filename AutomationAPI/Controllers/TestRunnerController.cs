using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestRunnerController : ControllerBase
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ILogger<QueueController> _logger;

        public TestRunnerController(IQueueRepository queueRepository, ILogger<QueueController> logger)
        {
            _queueRepository = queueRepository;
            _logger = logger;
        }

        [HttpPost("run-test")]
        public async Task<IActionResult> RunTestAsync([FromBody] QueueInfo queue)
        {
            if (queue == null)
            {
                return BadRequest("Queue information is required.");
            }

            try
            {
                queue.QueueId = Guid.NewGuid().ToString();
                queue.QueueName = await GenerateQueueNameAsync(queue.LibraryName, queue.ClassName, queue.MethodName, queue.QueueId);
                queue.CreatedDate = DateTime.Now;

                var insertedQueueId = await _queueRepository.InsertQueueAsync(queue);

                if (insertedQueueId <= 0)
                {
                    return StatusCode(500, "An error occurred while inserting the queue.");
                }

                return Ok(new { message = "Queue Created Successfully!" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting queue with QueueId: {QueueId}", queue.QueueId);

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        private Task<string> GenerateQueueNameAsync(string? libraryName, string? className, string? methodName, string? queueId)
        {

            var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Fallback if queueId is null or not a valid GUID
            string shortGuid = !string.IsNullOrWhiteSpace(queueId) && queueId.Contains("-")
                ? queueId.Split('-')[0]
                : Guid.NewGuid().ToString().Split('-')[0];

            // Build parts
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(libraryName)) parts.Add(libraryName);
            if (!string.IsNullOrWhiteSpace(className)) parts.Add(className);
            if (!string.IsNullOrWhiteSpace(methodName)) parts.Add(methodName);

            parts.Add(timeStamp);
            parts.Add(shortGuid);

            return Task.FromResult(string.Join("_", parts));

        }

    }
}
