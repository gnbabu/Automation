using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        private readonly IReleaseRepository _repo;
        private readonly IEnvironmentRepository _envRepo;
        private readonly IReleaseFileService _fileService;
        private readonly IReleaseReadinessService _readinessService;
        private readonly IReleaseNotificationService _notificationService;
        private readonly ILogger<ReleaseController> _logger;

        public ReleaseController(
            IReleaseRepository repo,
            IEnvironmentRepository envRepo,
            IReleaseFileService fileService,
            IReleaseReadinessService readinessService,
            IReleaseNotificationService notificationService,
            ILogger<ReleaseController> logger)
        {
            _repo = repo;
            _envRepo = envRepo;
            _fileService = fileService;
            _readinessService = readinessService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var releases = (await _repo.GetAllAsync()).ToList();
            foreach (var r in releases)
                PopulateFolderInfo(r);
            return Ok(releases);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var release = await _repo.GetByIdAsync(id);
            if (release == null)
                return NotFound();
            PopulateFolderInfo(release);
            return Ok(release);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReleaseRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ReleaseName))
                return BadRequest("Release Name is required.");
            if (string.IsNullOrWhiteSpace(request.Version))
                return BadRequest("Version is required.");
            if (!request.EnvironmentId.HasValue || request.EnvironmentId <= 0)
                return BadRequest("Environment is required.");

            // Environment must exist and be active (source of truth = Environment Management)
            var env = await _envRepo.GetByIdAsync(request.EnvironmentId.Value);
            if (env == null)
                return BadRequest("Selected environment does not exist.");
            if (!env.IsActive)
                return BadRequest("Selected environment is not active.");

            request.CreatedBy = string.IsNullOrWhiteSpace(request.CreatedBy)
                ? (User.Identity?.Name ?? "system")
                : request.CreatedBy;

            // Insert first: the folder name embeds ReleaseId, which only exists after insert.
            int newId;
            try
            {
                newId = await _repo.CreateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create release {ReleaseName}", request.ReleaseName);
                return Conflict("A release with the same Name, Version and Environment already exists.");
            }

            // Create the folder now; if this fails, compensate by removing the row so the
            // caller never sees a falsely-successful Release creation.
            try
            {
                var folderPath = _fileService.ResolveReleaseFolderPath(env.EnvironmentName, newId, request.ReleaseName, request.Version);
                _fileService.CreateReleaseFolder(folderPath);
                await _repo.SetFolderPathAsync(newId, folderPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create release folder for ReleaseId {ReleaseId} ({ReleaseName} {Version})", newId, request.ReleaseName, request.Version);

                try
                {
                    await _repo.DeleteAsync(newId);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to roll back Release {ReleaseId} after folder creation failure", newId);
                }

                return StatusCode(500, "Failed to create the release folder. The release was not created.");
            }

            var created = await _repo.GetByIdAsync(newId);
            PopulateFolderInfo(created);
            return Ok(created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReleaseRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ReleaseName))
                return BadRequest("Release Name is required.");

            request.ReleaseId = id;

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            // Identity fields (Name/Version/Environment) are baked into the release folder
            // path and any test results already recorded against this release, so they can
            // only change while the release is still in Draft (before activation).
            var isDraft = (existing.ReleaseLifecycle ?? string.Empty).Equals("Draft", StringComparison.OrdinalIgnoreCase);
            if (!isDraft)
            {
                var nameChanged = !string.Equals(request.ReleaseName.Trim(), existing.ReleaseName, StringComparison.Ordinal);
                var versionChanged = !string.IsNullOrWhiteSpace(request.Version) &&
                    !string.Equals(request.Version.Trim(), existing.Version, StringComparison.Ordinal);
                var envChanged = request.EnvironmentId.HasValue && request.EnvironmentId.Value != existing.EnvironmentId;

                if (nameChanged || versionChanged || envChanged)
                    return BadRequest("Release Name, Version and Environment can only be changed while the release is in Draft state.");
            }

            if (request.EnvironmentId.HasValue)
            {
                var env = await _envRepo.GetByIdAsync(request.EnvironmentId.Value);
                if (env == null)
                    return BadRequest("Selected environment does not exist.");
                if (isDraft && !env.IsActive)
                    return BadRequest("Selected environment is not active.");
            }

            request.ModifiedBy = string.IsNullOrWhiteSpace(request.ModifiedBy)
                ? (User.Identity?.Name ?? "system")
                : request.ModifiedBy;

            // The release folder is never renamed on update; it stays tied to the ReleaseId
            // assigned at creation time.
            try
            {
                await _repo.UpdateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update release {ReleaseId}", id);
                return Conflict("A release with the same Name, Version and Environment already exists.");
            }

            var updated = await _repo.GetByIdAsync(id);
            PopulateFolderInfo(updated);
            return Ok(updated);
        }

        // Permanent delete: only allowed while still in Draft (nothing of value to lose
        // yet — no activation, no test history). Once a release progresses past Draft,
        // use Deactivate (soft delete, via Update with IsActive=false) instead so its
        // folder/results/sign-off history is preserved.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var release = await _repo.GetByIdAsync(id);
            if (release == null)
                return NotFound();

            if (!(release.ReleaseLifecycle ?? string.Empty).Equals("Draft", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only Draft releases can be permanently deleted. Deactivate this release instead.");

            try
            {
                await _repo.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete release {ReleaseId}", id);
                return Conflict("Cannot delete: this release has associated test assignments or other linked data.");
            }

            if (!string.IsNullOrWhiteSpace(release.ReleaseFolderPath))
            {
                try
                {
                    _fileService.DeleteReleaseFolder(release.ReleaseFolderPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Release {ReleaseId} was deleted, but its folder could not be removed", id);
                }
            }

            return Ok(new { Message = "Release deleted successfully." });
        }

        [HttpGet("{id:int}/readiness")]
        public async Task<IActionResult> GetReadiness(int id)
        {
            var release = await _repo.GetByIdAsync(id);
            if (release == null)
                return NotFound();

            var readiness = _readinessService.CheckReadiness(release.ReleaseFolderPath);
            return Ok(readiness);
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, [FromBody] ReleaseActivateRequestDto request)
        {
            var release = await _repo.GetByIdAsync(id);
            if (release == null)
                return NotFound();

            // Activation prerequisite: the release folder must have usable test DLL content,
            // determined via the existing DLL loading/discovery technique (read-only check).
            var readiness = _readinessService.CheckReadiness(release.ReleaseFolderPath);
            if (!readiness.IsReady)
                return BadRequest(readiness.Message ?? "Release is not ready for activation (DLLs not available).");

            var activatedBy = request?.ActivatedBy;
            if (string.IsNullOrWhiteSpace(activatedBy))
                activatedBy = User.Identity?.Name ?? "system";

            try
            {
                await _repo.ActivateAsync(id, activatedBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Activation blocked for release {ReleaseId}", id);
                return BadRequest(GetUserMessage(ex, "Release could not be activated."));
            }

            // Notify Test Managers/Admins that the release is available for testing.
            var subject = $"Release available for testing: {release.ReleaseName} {release.Version}";
            var body = $"<p>Release <strong>{release.ReleaseName}</strong> (Version {release.Version}, " +
                       $"Environment {release.EnvironmentName}) has been activated and is now available for testing.</p>";
            var notifyResult = await _notificationService.NotifyManagersAndAdminsAsync(
                id, "ActivatedForTesting", subject, body);

            var updated = await _repo.GetByIdAsync(id);
            PopulateFolderInfo(updated);
            return Ok(new { Release = updated, Notification = notifyResult });
        }

        [HttpPost("{id:int}/signoff")]
        public async Task<IActionResult> SignOff(int id, [FromBody] ReleaseSignOffRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SignOffStatus))
                return BadRequest("SignOffStatus is required (Approved or Rejected).");

            var status = request.SignOffStatus.Trim();
            if (!status.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                !status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                return BadRequest("SignOffStatus must be 'Approved' or 'Rejected'.");
            request.SignOffStatus = char.ToUpper(status[0]) + status.Substring(1).ToLower();

            var release = await _repo.GetByIdAsync(id);
            if (release == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(request.SignOffBy))
                request.SignOffBy = User.Identity?.Name ?? "system";

            try
            {
                await _repo.SignOffAsync(id, request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Sign-off blocked for release {ReleaseId}", id);
                return BadRequest(GetUserMessage(ex, "Release could not be signed off."));
            }

            var updated = await _repo.GetByIdAsync(id);
            PopulateFolderInfo(updated);
            return Ok(updated);
        }

        [HttpGet("{id:int}/signoff-history")]
        public async Task<IActionResult> GetSignOffHistory(int id)
        {
            var history = await _repo.GetSignOffHistoryAsync(id);
            return Ok(history);
        }

        [HttpGet("{id:int}/notifications")]
        public async Task<IActionResult> GetNotifications(int id)
        {
            var notifications = await _repo.GetNotificationsAsync(id);
            return Ok(notifications);
        }

        // ---- helpers ----

        // Cheap, non-reflective folder scan for list/detail display badges.
        private void PopulateFolderInfo(ReleaseModel release)
        {
            if (release == null) return;
            var count = _readinessService.GetDllFileCount(release.ReleaseFolderPath);
            release.DllFileCount = count;
            release.FolderReady = count > 0;
        }

        private static string GetUserMessage(Exception ex, string fallback)
        {
            // SqlDataAccessHelper wraps SQL errors; surface the innermost RAISERROR text.
            var inner = ex;
            while (inner.InnerException != null)
                inner = inner.InnerException;
            return string.IsNullOrWhiteSpace(inner.Message) ? fallback : inner.Message;
        }
    }
}
