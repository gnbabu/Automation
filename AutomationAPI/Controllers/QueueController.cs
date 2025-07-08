using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using AutomationAPI.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QueueController : ControllerBase
    {
        private readonly IQueueRepository _queueRepository;
        private readonly ILogger<QueueController> _logger;

        public QueueController(IQueueRepository queueRepository, ILogger<QueueController> logger)
        {
            _queueRepository = queueRepository;
            _logger = logger;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchQueuesAsync([FromBody] QueueSearchPayload payload)
        {
            _logger.LogInformation("GET request received to fetch all queues.");

            try
            {
                if (payload == null)
                    return BadRequest("Payload cannot be null.");

                if (payload.UserId == 0)
                    return BadRequest("Please provide User Id.");

                var pagedResult = await _queueRepository.SearchQueuesAsync(payload);
                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all queues.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{queueId}")]
        public async Task<IActionResult> GetQueueById(string queueId)
        {
            try
            {
                var queue = await _queueRepository.GetQueueByIdAsync(queueId);
                return Ok(queue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching queues with QueueId: {QueueId}", queueId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/queue
        [HttpGet]
        public async Task<IActionResult> GetQueues([FromQuery] string? queueId, [FromQuery] string? queueStatus)
        {
            try
            {
                var queues = await _queueRepository.GetQueuesAsync(queueId, queueStatus);
                return Ok(queues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching queues with QueueId: {QueueId}, QueueStatus: {QueueStatus}", queueId, queueStatus);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/queue
        [HttpPost]
        public async Task<IActionResult> InsertQueue([FromBody] QueueInfo queue)
        {
            if (queue == null)
            {
                return BadRequest("Queue information is required.");
            }

            try
            {
                var insertedQueueId = await _queueRepository.InsertQueueAsync(queue);

                if (insertedQueueId <= 0)
                {
                    return StatusCode(500, "An error occurred while inserting the queue.");
                }

                return Ok("Queue Created Successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting queue with QueueId: {QueueId}", queue.QueueId);

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/queue/{queueId}
        [HttpPut("{queueId}")]
        public async Task<IActionResult> UpdateQueueStatus(string queueId, [FromBody] string queueStatus)
        {
            if (string.IsNullOrEmpty(queueStatus))
            {
                return BadRequest("Queue status is required.");
            }

            try
            {
                var rowsAffected = await _queueRepository.UpdateQueueStatusAsync(queueId, queueStatus);

                if (rowsAffected > 0)
                {
                    return Ok("Queue status updated successfully.");
                }

                return NotFound($"Queue with id '{queueId}' not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating queue status for QueueId: {QueueId}", queueId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/queue/{queueId}
        [HttpDelete("{queueId}")]
        public async Task<IActionResult> DeleteQueue(string queueId)
        {
            if (string.IsNullOrEmpty(queueId))
            {
                return BadRequest("Queue Id is required.");
            }

            try
            {
                var success = await _queueRepository.DeleteQueueAsync(queueId);

                if (success)
                {
                    return Ok("Queue deleted successfully.");
                }

                return NotFound($"Queue with id '{queueId}' not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting queue with ID: {QueueId}", queueId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("queue-reports")]
        public async Task<IActionResult> GetQueueReports([FromBody] QueueReportFilterRequest payload)
        {
            try
            {
                if (payload == null)
                    return BadRequest("Payload cannot be null.");

                if (payload.UserId == 0)
                    return BadRequest("Please provide User Id.");

                var pagedResult = await _queueRepository.GetQueueReportsAsync(payload);

                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching paged queues.");
                return StatusCode(500, "Internal server error.");
            }

        }
    }
}
