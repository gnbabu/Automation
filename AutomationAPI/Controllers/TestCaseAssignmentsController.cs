﻿using AutomationAPI.Repositories.Interfaces;
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

        public TestCaseAssignmentsController(ITestCaseAssignmentRepository repository, ILogger<TestCaseAssignmentsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET api/TestCaseAssignments/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAssignmentsByUserIdAsync(int userId)
        {
            try
            {
                var assignments = await _repository.GetAssignmentsByUserIdAsync(userId);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while Get Assignments");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST api/TestCaseAssignments/Sync
        [HttpPost("bulk-insert")]
        public async Task<IActionResult> BulkInsertAssignmentsAsync([FromBody] List<TestCaseAssignment> assignments)
        {
            try
            {
                if (!assignments.Any())
                    return BadRequest("No assignments provided.");

                await _repository.BulkInsertAssignmentsAsync(assignments);
                return Ok(new { Message = $"{assignments.Count} Assignments inserted successfully." });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while Bulk Insert Assignments");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteAssignmentsByUserId(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            try
            {
                await _repository.DeleteAssignmentsByUserIdAsync(userId);
                return Ok(new { Message = $"All assignments for UserId {userId} have been deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while DeleteAssignments By UserId");
                return StatusCode(500, new { Message = "An error occurred while deleting assignments.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes test case assignments based on userIds and/or library/class/method filters
        /// </summary>
        [HttpPost("delete-assignments")]
        public async Task<IActionResult> DeleteAssignments([FromBody] TestCaseAssignmentDeleteRequest request)
        {
            try
            {

                if (request == null)
                    return BadRequest("Request cannot be null.");

                await _repository.DeleteAssignmentsAsync(request);

                return Ok(new { message = "Assignments deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while Delete Assignments");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

