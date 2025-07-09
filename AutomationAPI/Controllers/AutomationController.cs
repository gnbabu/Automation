using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Authorization;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AutomationController : ControllerBase
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly ILogger<AutomationController> _logger;

        public AutomationController(IAutomationRepository automationRepository, ILogger<AutomationController> logger)
        {
            _automationRepository = automationRepository;
            _logger = logger;
        }

        // Get Automation Flow Names
        [HttpGet("flows")]
        public async Task<IActionResult> GetAutomationFlowNamesAsync()
        {
            try
            {
                var flows = await _automationRepository.GetAutomationFlowNamesAsync();
                return Ok(flows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving automation flow names.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        //Get Automation Data Sections
        [HttpGet("sections/{flowName}")]
        public async Task<IActionResult> GetAutomationDataSectionsAsync(string flowName)
        {
            try
            {
                if (string.IsNullOrEmpty(flowName))
                {
                    return BadRequest("Flow Name is not provider.");
                }
                var sections = await _automationRepository.GetAutomationDataSectionsAsync(flowName);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving automation data sections for FlowName: {FlowName}", flowName);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        //Get Automation Data by Section ID
        [HttpGet("sections/data")]
        public async Task<IActionResult> GetAutomationDataAsync([FromQuery] int sectionId, [FromQuery] int userId)
        {
            try
            {
                var data = await _automationRepository.GetAutomationDataAsync(sectionId, userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving automation data for SectionID: {SectionID} and UserId: {userId}", sectionId, userId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // 2. Get Automation Data by Flow Name
        [HttpGet("data/flow/{flowName}")]
        public async Task<IActionResult> GetAutomationDataByFlowNameAsync(string flowName)
        {
            try
            {
                _logger.LogInformation("Retrieving automation data for FlowName: {FlowName}", flowName);
                var data = await _automationRepository.GetAutomationDataByFlowNameAsync(flowName);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving automation data for FlowName: {FlowName}", flowName);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // 3. Insert Automation Data
        [HttpPost("data")]
        public async Task<IActionResult> InsertAutomationDataAsync([FromBody] AutomationDataRequest request)
        {
            try
            {
                _logger.LogInformation("Inserting automation data for SectionID: {SectionID}", request.SectionId);
                var newId = await _automationRepository.InsertAutomationDataAsync(request);
                return Ok(newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting automation data for SectionID: {SectionID}", request.SectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // 4. Update Automation Data
        [HttpPut("data")]
        public async Task<IActionResult> UpdateAutomationDataAsync([FromBody] AutomationDataRequest request)
        {
            try
            {
                _logger.LogInformation("Updating automation data for SectionID: {SectionID}", request.SectionId);
                await _automationRepository.UpdateAutomationDataAsync(request);
                return NoContent(); // Successfully updated
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating automation data for SectionID: {SectionID}", request.SectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // 5. Delete Automation Data
        [HttpDelete("data/{sectionId}")]
        public async Task<IActionResult> DeleteAutomationDataAsync(int sectionId)
        {
            try
            {
                _logger.LogInformation("Deleting automation data for SectionID: {SectionID}", sectionId);
                await _automationRepository.DeleteAutomationDataAsync(sectionId);
                return NoContent(); // Successfully deleted
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting automation data for SectionID: {SectionID}", sectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }



        // 7. Insert Automation Data Section
        [HttpPost("sections")]
        public async Task<IActionResult> InsertAutomationDataSectionAsync([FromBody] AutomationDataSectionRequest request)
        {
            try
            {
                _logger.LogInformation("Inserting automation data section with SectionName: {SectionName}", request.SectionName);
                var newId = await _automationRepository.InsertAutomationDataSectionAsync(request);
                return CreatedAtAction(nameof(GetAutomationDataSectionsAsync), new { sectionId = newId }, newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting automation data section with SectionName: {SectionName}", request.SectionName);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // 8. Update Automation Data Section
        [HttpPut("sections")]
        public async Task<IActionResult> UpdateAutomationDataSectionAsync([FromBody] AutomationDataSectionRequest request)
        {
            try
            {
                _logger.LogInformation("Updating automation data section for SectionID: {SectionID}", request.SectionId);
                await _automationRepository.UpdateAutomationDataSectionAsync(request);
                return NoContent(); // Successfully updated
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating automation data section for SectionID: {SectionID}", request.SectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // 9. Delete Automation Data Section
        [HttpDelete("sections/{sectionId}")]
        public async Task<IActionResult> DeleteAutomationDataSectionAsync(int sectionId)
        {
            try
            {
                _logger.LogInformation("Deleting automation data section for SectionID: {SectionID}", sectionId);
                await _automationRepository.DeleteAutomationDataSectionAsync(sectionId);
                return NoContent(); // Successfully deleted
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting automation data section for SectionID: {SectionID}", sectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


    }
}
