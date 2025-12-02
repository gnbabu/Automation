using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework.Internal;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestScreenshotsController : ControllerBase
    {
        private readonly ITestScreenshotRepository _repository;
        private readonly ILogger<TestScreenshotsController> _logger;

        public TestScreenshotsController(ITestScreenshotRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> InsertScreenshot([FromBody] TestScreenshot screenshot)
        {
            try
            {
                if (screenshot == null) return BadRequest("Screenshot cannot be null");

                var result = await _repository.InsertScreenshotAsync(screenshot);
                return Ok(new { InsertedRows = result });

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error while creating the screenshot");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("bulk")]
        public async Task<IActionResult> BulkInsertScreenshots([FromBody] List<TestScreenshot> screenshots)
        {
            try
            {

                if (screenshots == null || !screenshots.Any())
                    return BadRequest("Screenshots cannot be null or empty");

                await _repository.BulkInsertScreenshotsAsync(screenshots);
                return Ok(new { Message = $"{screenshots.Count} screenshots inserted successfully." });

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error while creating the screenshots");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("screenshots")]
        public async Task<IActionResult> GetScreenshotsByAssignmentTestCaseId([FromQuery] int assignmentTestCaseId)
        {
            if (assignmentTestCaseId == 0)
                return BadRequest("Assignment Test Case Id is required.");

            try
            {
                var screenshots = await _repository.GetScreenshotsByAssignmentTestCaseId(assignmentTestCaseId);
                return Ok(screenshots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Screenshots for Assignment Test Case Id : {AssignmentTestCaseId}", assignmentTestCaseId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetScreenshotById(int id)
        {
            try
            {
                var screenshot = await _repository.GetScreenshotByIdAsync(id);
                return Ok(screenshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Screenshot by Id: {id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
