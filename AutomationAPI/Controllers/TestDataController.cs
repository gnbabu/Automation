using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    // Test data for test cases whose data model is defined in code
    // ([TestDataFlow(typeof(Model))]): the flow/sections/fields come from the model,
    // values are saved per test case x section x user x environment. Portal endpoints
    // always work on the logged-in user's own data, for test cases assigned to them.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestDataController : ControllerBase
    {
        private readonly TestDataService _service;
        private readonly ITestDataRepository _repository;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<TestDataController> _logger;

        public TestDataController(TestDataService service, ITestDataRepository repository, IAuditLogService auditLog, ILogger<TestDataController> logger)
        {
            _service = service;
            _repository = repository;
            _auditLog = auditLog;
            _logger = logger;
        }

        private bool IsViewer() => User.IsInRole("Viewer");

        // The isolated test process authenticates with ServiceTokenGenerator's token
        // (Name "TestRunner", user id 0 - so GetAuditUserId() is null).
        private bool IsTestRunner() => this.GetAuditUserId() == null && User.Identity?.Name == "TestRunner";

        [HttpGet("my-testcases")]
        public async Task<IActionResult> GetMyTestCases([FromQuery] int environmentId)
        {
            var userId = this.GetAuditUserId();
            if (userId == null) return Unauthorized();
            if (environmentId <= 0) return BadRequest("EnvironmentId is required.");

            try
            {
                return Ok(await _service.GetMyTestCasesAsync(userId.Value, environmentId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading test cases for UserId {UserId}, EnvironmentId {EnvironmentId}", userId, environmentId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Flow/sections/fields for the test case's model. 204 when it has no model (the
        // Portal then falls back to the manual Flow/Section editor).
        [HttpGet("schema")]
        public async Task<IActionResult> GetSchema([FromQuery] int assignmentTestCaseId)
        {
            var (assignment, error) = await GetOwnAssignmentAsync(assignmentTestCaseId);
            if (error != null) return error;

            try
            {
                var schema = await _service.GetSchemaAsync(assignment!);
                return schema == null ? NoContent() : Ok(schema);
            }
            catch (Exception ex) when (ex is FileNotFoundException or TypeLoadException or TestDataValidationException or ArgumentException)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading test data schema for AssignmentTestCaseId {AssignmentTestCaseId}", assignmentTestCaseId);
                return StatusCode(500, "An error occurred while reading the test data model.");
            }
        }

        // The test case's saved sections (current user) plus the document version to
        // send back on save. The optional from* key reads another test case's data
        // instead - used by "Copy from another test case" (still only the current
        // user's own data).
        [HttpGet("data")]
        public async Task<IActionResult> GetData(
            [FromQuery] int assignmentTestCaseId, [FromQuery] int environmentId,
            [FromQuery] string? fromLibrary = null, [FromQuery] string? fromClass = null, [FromQuery] string? fromMethod = null)
        {
            var (assignment, error) = await GetOwnAssignmentAsync(assignmentTestCaseId);
            if (error != null) return error;
            if (environmentId <= 0) return BadRequest("EnvironmentId is required.");

            try
            {
                var schema = await _service.GetSchemaAsync(assignment!);
                if (schema == null) return NoContent();

                var copying = !string.IsNullOrWhiteSpace(fromLibrary) && !string.IsNullOrWhiteSpace(fromClass) && !string.IsNullOrWhiteSpace(fromMethod);
                return Ok(await _service.GetDataAsync(
                    copying ? fromLibrary! : assignment!.LibraryName,
                    copying ? fromClass! : assignment!.ClassName,
                    copying ? fromMethod! : assignment!.MethodName,
                    schema, this.GetAuditUserId()!.Value, environmentId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading test data for AssignmentTestCaseId {AssignmentTestCaseId}", assignmentTestCaseId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("copy-sources")]
        public async Task<IActionResult> GetCopySources([FromQuery] int assignmentTestCaseId, [FromQuery] int environmentId)
        {
            var (assignment, error) = await GetOwnAssignmentAsync(assignmentTestCaseId);
            if (error != null) return error;
            if (environmentId <= 0) return BadRequest("EnvironmentId is required.");

            try
            {
                var schema = await _service.GetSchemaAsync(assignment!);
                if (schema == null) return Ok(Array.Empty<TestDataCopySource>());

                return Ok(await _repository.GetCopySourcesAsync(assignment!.LibraryName, assignment.ClassName, assignment.MethodName,
                    schema.FlowName, this.GetAuditUserId()!.Value, environmentId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading copy sources for AssignmentTestCaseId {AssignmentTestCaseId}", assignmentTestCaseId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPut("section")]
        public async Task<IActionResult> SaveSection([FromBody] TestDataSectionSaveRequest request)
        {
            if (IsViewer())
                return StatusCode(403, "Viewers have read-only access and cannot save test data.");
            if (request == null || request.EnvironmentId <= 0 || request.SectionId <= 0)
                return BadRequest("Test case, environment and section are required.");

            var (assignment, error) = await GetOwnAssignmentAsync(request.AssignmentTestCaseId);
            if (error != null) return error;

            try
            {
                var schema = await _service.GetSchemaAsync(assignment!);
                var section = schema?.Sections.FirstOrDefault(s => s.SectionId == request.SectionId);
                if (section == null)
                    return BadRequest("That section isn't part of this test case's data model.");

                var userId = this.GetAuditUserId()!.Value;
                var (result, savedRows) = await _service.SaveSectionAsync(assignment!, schema!, section, request.Rows,
                    userId, request.EnvironmentId, request.ExpectedVersion);

                // Field names only, never values - same convention as flow-level Test Data.
                await _auditLog.LogAsync("TestCaseData", result.TestCaseDataId,
                    $"{assignment.TestCaseId ?? $"{assignment.ClassName}.{assignment.MethodName}"} / {section.SectionName}",
                    result.IsNew ? "Created" : "Updated",
                    userId, this.GetAuditUserName(),
                    snapshot: new
                    {
                        libraryName = assignment.LibraryName,
                        className = assignment.ClassName,
                        methodName = assignment.MethodName,
                        flowName = schema!.FlowName,
                        sectionName = section.SectionName,
                        environmentId = request.EnvironmentId,
                        rowCount = savedRows.Count,
                        fieldKeys = savedRows.SelectMany(r => r.Keys).Distinct().ToList()
                    });

                return Ok(result);
            }
            catch (TestDataValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (TestDataConflictException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving test data for AssignmentTestCaseId {AssignmentTestCaseId}, SectionID {SectionID}", request.AssignmentTestCaseId, request.SectionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // "Copy all": replaces the test case's whole document (current user, target
        // environment) with another test case's (same model) or another environment's.
        [HttpPost("copy-all")]
        public async Task<IActionResult> CopyAll([FromBody] TestDataCopyAllRequest request)
        {
            if (IsViewer())
                return StatusCode(403, "Viewers have read-only access and cannot change test data.");
            if (request == null || request.EnvironmentId <= 0)
                return BadRequest("Test case and environment are required.");

            var fromTestCase = !string.IsNullOrWhiteSpace(request.FromLibrary) && !string.IsNullOrWhiteSpace(request.FromClass) && !string.IsNullOrWhiteSpace(request.FromMethod);
            var fromEnvironment = request.FromEnvironmentId > 0;
            if (fromTestCase == fromEnvironment)
                return BadRequest("Choose either another test case or another environment to copy from.");

            var (assignment, error) = await GetOwnAssignmentAsync(request.AssignmentTestCaseId);
            if (error != null) return error;

            try
            {
                var schema = await _service.GetSchemaAsync(assignment!);
                if (schema == null) return BadRequest("This test case has no test data model.");

                var sourceLibrary = fromTestCase ? request.FromLibrary! : assignment!.LibraryName;
                var sourceClass = fromTestCase ? request.FromClass! : assignment!.ClassName;
                var sourceMethod = fromTestCase ? request.FromMethod! : assignment!.MethodName;
                var sourceEnvironmentId = fromEnvironment ? request.FromEnvironmentId!.Value : request.EnvironmentId;
                if (fromEnvironment && sourceEnvironmentId == request.EnvironmentId)
                    return BadRequest("Choose a different environment to copy from.");

                var userId = this.GetAuditUserId()!.Value;
                var (result, copied) = await _service.CopyAllAsync(assignment!, schema,
                    sourceLibrary, sourceClass, sourceMethod, sourceEnvironmentId,
                    request.EnvironmentId, userId, request.ExpectedVersion);

                await _auditLog.LogAsync("TestCaseData", result.TestCaseDataId,
                    $"{assignment!.TestCaseId ?? $"{assignment.ClassName}.{assignment.MethodName}"} / all sections",
                    "Copied", userId, this.GetAuditUserName(),
                    snapshot: new
                    {
                        libraryName = assignment.LibraryName,
                        className = assignment.ClassName,
                        methodName = assignment.MethodName,
                        flowName = schema.FlowName,
                        environmentId = request.EnvironmentId,
                        copiedFrom = new { libraryName = sourceLibrary, className = sourceClass, methodName = sourceMethod, environmentId = sourceEnvironmentId },
                        sections = copied
                    });

                return Ok(new { result.TestCaseDataId, result.Version, CopiedSections = copied });
            }
            catch (TestDataValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (TestDataConflictException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying test data for AssignmentTestCaseId {AssignmentTestCaseId}", request.AssignmentTestCaseId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Called by test runs (Selenium.BaseComponents' TestData.Load<T>()): the test
        // case's own JSON document for its assigned tester and the assignment's
        // environment, shaped like the model ("{}" when nothing is saved).
        [HttpGet("runtime")]
        public async Task<IActionResult> GetRuntimeData([FromQuery] string flowName, [FromQuery] int assignmentTestCaseId)
        {
            if (string.IsNullOrWhiteSpace(flowName) || assignmentTestCaseId <= 0)
                return BadRequest("flowName and assignmentTestCaseId are required.");

            try
            {
                var assignment = await _repository.ResolveAssignmentAsync(assignmentTestCaseId);
                if (assignment == null) return NotFound("Test case assignment not found.");
                if (!IsTestRunner() && this.GetAuditUserId() != assignment.AssignedUser)
                    return StatusCode(403, "This test case isn't assigned to you.");
                if (assignment.EnvironmentId == null)
                    return UnprocessableEntity("The test case's assignment has no environment.");

                var document = await _service.GetRuntimeDocumentAsync(assignment, flowName);
                return Content(document, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading runtime test data for FlowName {FlowName}, AssignmentTestCaseId {AssignmentTestCaseId}", flowName, assignmentTestCaseId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private async Task<(TestDataAssignment? Assignment, IActionResult? Error)> GetOwnAssignmentAsync(int assignmentTestCaseId)
        {
            var userId = this.GetAuditUserId();
            if (userId == null) return (null, Unauthorized());
            if (assignmentTestCaseId <= 0) return (null, BadRequest("assignmentTestCaseId is required."));

            var assignment = await _repository.ResolveAssignmentAsync(assignmentTestCaseId);
            if (assignment == null) return (null, NotFound("Test case assignment not found."));
            if (assignment.AssignedUser != userId) return (null, StatusCode(403, "This test case isn't assigned to you."));
            return (assignment, null);
        }
    }
}
