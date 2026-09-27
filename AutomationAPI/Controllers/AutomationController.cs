using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AutomationController : ControllerBase
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<AutomationController> _logger;

        public AutomationController(IAutomationRepository automationRepository, IAuditLogService auditLog, ILogger<AutomationController> logger)
        {
            _automationRepository = automationRepository;
            _auditLog = auditLog;
            _logger = logger;
        }

        // Viewers have read-only access - mirrors the client-side isViewer() guard in
        // test-data-management.component.ts, enforced server-side too since the UI guard
        // alone can't stop a direct API call from a Viewer's valid token.
        private bool IsViewer() => User.IsInRole("Viewer");

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
        public async Task<IActionResult> GetAutomationDataAsync([FromQuery] int sectionId, [FromQuery] int userId, [FromQuery] int environmentId)
        {
            try
            {
                if (environmentId <= 0)
                    return BadRequest("EnvironmentId is required.");

                var data = await _automationRepository.GetAutomationDataAsync(sectionId, userId, environmentId);
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
                if (IsViewer())
                    return StatusCode(403, "Viewers have read-only access and cannot save test content.");

                if (request.EnvironmentId is null || request.EnvironmentId <= 0)
                    return BadRequest("EnvironmentId is required.");

                _logger.LogInformation("Inserting automation data for SectionID: {SectionID}", request.SectionId);
                var newId = await _automationRepository.InsertAutomationDataAsync(request);

                // Deliberately logs field *keys* only, never values - this data can
                // include credential-like fields (see test-data-management.component.ts's
                // own isSensitiveKey masking), and the Activity Log's audience (any Admin)
                // is broader than this screen's own per-user scoping.
                var sectionName = (await _automationRepository.GetAutomationDataSectionsAsync(null))
                    .FirstOrDefault(s => s.SectionId == request.SectionId)?.SectionName;
                var fieldKeys = AutomationDataHelper.ParseAutomationContents(request.TestContent).Select(f => f.FieldName).ToList();
                await _auditLog.LogAsync("TestData", newId, sectionName ?? $"Section #{request.SectionId}", "Created",
                    this.GetAuditUserId(), this.GetAuditUserName(),
                    snapshot: new { sectionId = request.SectionId, environmentId = request.EnvironmentId, fieldKeys });

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
                if (IsViewer())
                    return StatusCode(403, "Viewers have read-only access and cannot save test content.");

                _logger.LogInformation("Updating automation data for SectionID: {SectionID}", request.SectionId);

                // request only ever carries Id + the new TestContent -
                // fetches the pre-update row for both the section/environment context and
                // the old field keys to diff against.
                var existing = request.Id.HasValue ? await _automationRepository.GetAutomationDataByIdAsync(request.Id.Value) : null;

                await _automationRepository.UpdateAutomationDataAsync(request);

                if (existing != null)
                {
                    var oldKeys = AutomationDataHelper.ParseAutomationContents(existing.TestContent).Select(f => f.FieldName).ToHashSet();
                    var newKeys = AutomationDataHelper.ParseAutomationContents(request.TestContent).Select(f => f.FieldName).ToHashSet();
                    var addedKeys = newKeys.Except(oldKeys).ToList();
                    var removedKeys = oldKeys.Except(newKeys).ToList();

                    var sectionName = (await _automationRepository.GetAutomationDataSectionsAsync(null))
                        .FirstOrDefault(s => s.SectionId == existing.SectionId)?.SectionName;

                    // Field *keys* only, never values - see the same note on Create above.
                    await _auditLog.LogAsync("TestData", existing.Id, sectionName ?? $"Section #{existing.SectionId}", "Updated",
                        this.GetAuditUserId(), this.GetAuditUserName(),
                        snapshot: new { sectionId = existing.SectionId, environmentId = existing.EnvironmentId, fieldCount = newKeys.Count, addedKeys, removedKeys });
                }

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

                var sectionName = (await _automationRepository.GetAutomationDataSectionsAsync(null))
                    .FirstOrDefault(s => s.SectionId == sectionId)?.SectionName;

                await _automationRepository.DeleteAutomationDataAsync(sectionId);

                await _auditLog.LogAsync("TestData", null, sectionName ?? $"Section #{sectionId}", "Deleted",
                    this.GetAuditUserId(), this.GetAuditUserName(),
                    snapshot: new { sectionId });

                return NoContent(); // Successfully deleted
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting automation data for SectionID: {SectionID}", sectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }



        [HttpPost("sections")]
        public async Task<IActionResult> InsertAutomationDataSectionAsync([FromBody] AutomationDataSectionRequest request)
        {
            if (IsViewer())
                return StatusCode(403, "Viewers have read-only access and cannot manage flows or sections.");

            try
            {
                _logger.LogInformation("Inserting automation data section with SectionName: {SectionName}", request.SectionName);
                var newId = await _automationRepository.InsertAutomationDataSectionAsync(request);

                // Distinguishes "brand new Flow" from "new Section in an existing Flow" -
                // there's no separate Flow table, so this is the only signal available;
                // an approximate check (any existing section under this FlowName before
                // the insert) rather than a strict one is fine here, since it only
                // affects which EntityType label the log entry gets, not the section
                // itself.
                var existingInFlow = await _automationRepository.GetAutomationDataSectionsAsync(request.FlowName);
                var isNewFlow = !existingInFlow.Any(s => s.SectionId != newId);
                if (isNewFlow)
                {
                    await _auditLog.LogAsync("Flow", null, request.FlowName, "Created",
                        this.GetAuditUserId(), this.GetAuditUserName(),
                        snapshot: new { flowName = request.FlowName, firstSectionName = request.SectionName });
                }
                await _auditLog.LogAsync("Section", newId, request.SectionName, "Created",
                    this.GetAuditUserId(), this.GetAuditUserName(),
                    snapshot: new { sectionName = request.SectionName, flowName = request.FlowName });

                // Plain Ok(newId) - the previous CreatedAtAction pointed at the
                // sections/{flowName} GET route with a sectionId route value, which
                // throws "No route matches the supplied values" at runtime (a
                // pre-existing bug never hit before because no UI ever called this
                // endpoint) - the insert itself succeeded, so the row WAS created, and
                // the resulting 500 then made every legitimate retry hit the duplicate
                // check and look like a false rejection.
                return Ok(newId);
            }
            catch (DuplicateSectionException ex)
            {
                return Conflict(ex.Message);
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
            if (IsViewer())
                return StatusCode(403, "Viewers have read-only access and cannot manage flows or sections.");

            try
            {
                _logger.LogInformation("Updating automation data section for SectionID: {SectionID}", request.SectionId);

                // Fetch the pre-update name for the audit diff - there is no dedicated
                // GetSectionById, so this reuses the same by-flow lookup the duplicate-
                // name check already relies on.
                var existing = (await _automationRepository.GetAutomationDataSectionsAsync(request.FlowName))
                    .FirstOrDefault(s => s.SectionId == request.SectionId);

                await _automationRepository.UpdateAutomationDataSectionAsync(request);

                if (existing != null && !string.Equals(existing.SectionName, request.SectionName, StringComparison.Ordinal))
                {
                    await _auditLog.LogAsync("Section", request.SectionId, request.SectionName, "Updated",
                        this.GetAuditUserId(), this.GetAuditUserName(),
                        changes: new[] { new AuditFieldChange { Field = "SectionName", Old = existing.SectionName, New = request.SectionName } });
                }

                return NoContent(); // Successfully updated
            }
            catch (DuplicateSectionException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating automation data section for SectionID: {SectionID}", request.SectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // 9. Delete Automation Data Section
        // cascade=true deletes the section's saved test data too - only sent after the
        // user explicitly confirms that in the UI; the default still 409s if data
        // exists.
        [HttpDelete("sections/{sectionId}")]
        public async Task<IActionResult> DeleteAutomationDataSectionAsync(int sectionId, [FromQuery] bool cascade = false)
        {
            if (IsViewer())
                return StatusCode(403, "Viewers have read-only access and cannot manage flows or sections.");

            try
            {
                _logger.LogInformation("Deleting automation data section for SectionID: {SectionID} (cascade: {Cascade})", sectionId, cascade);

                // Fetch the pre-delete name/flow for the audit entry - there is no
                // dedicated GetSectionById; passing a null/empty FlowName to the
                // existing by-flow lookup returns every section across every flow
                // (confirmed in usp_get_AutomationDataSection), which is fine here since
                // this only runs once, right before an already-confirmed delete.
                var existing = (await _automationRepository.GetAutomationDataSectionsAsync(null))
                    .FirstOrDefault(s => s.SectionId == sectionId);

                await _automationRepository.DeleteAutomationDataSectionAsync(sectionId, cascade);

                await _auditLog.LogAsync("Section", sectionId, existing?.SectionName ?? $"Section #{sectionId}", "Deleted",
                    this.GetAuditUserId(), this.GetAuditUserName(),
                    snapshot: new { sectionName = existing?.SectionName, flowName = existing?.FlowName, cascade });

                return NoContent(); // Successfully deleted
            }
            catch (SectionHasDataException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting automation data section for SectionID: {SectionID}", sectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


    }
}
