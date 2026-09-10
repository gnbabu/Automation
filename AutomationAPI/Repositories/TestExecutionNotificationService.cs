using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class TestExecutionNotificationService : ITestExecutionNotificationService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<TestExecutionNotificationService> _logger;
        private readonly SqlDataAccessHelper _db;

        public TestExecutionNotificationService(
            IUserRepository userRepo,
            IEmailService emailService,
            SqlDataAccessHelper db,
            ILogger<TestExecutionNotificationService> logger)
        {
            _userRepo = userRepo;
            _emailService = emailService;
            _db = db;
            _logger = logger;
        }

        public async Task NotifyScheduledFailureAsync(
            int assignmentTestCaseId,
            string testCaseId,
            string? errorMessage,
            int? assignedUserId)
        {
            try
            {
                // Recipients: the assigned user (regardless of role) + every active
                // Admin - mirrors ReleaseNotificationService's own Manager/Admin
                // resolution, plus the assigned user explicitly (they may not be an
                // Admin/Manager themselves).
                var recipients = new List<(int? UserId, string Email, string UserName)>();

                if (assignedUserId.HasValue)
                {
                    var assignedUser = await _userRepo.GetUserByIdAsync(assignedUserId.Value);
                    if (assignedUser != null && assignedUser.Active && !string.IsNullOrWhiteSpace(assignedUser.Email))
                        recipients.Add((assignedUser.UserId, assignedUser.Email, assignedUser.UserName));
                }

                var allUsers = await _userRepo.GetAllUsersAsync();
                var admins = allUsers.Where(u =>
                    u.Active &&
                    !string.IsNullOrWhiteSpace(u.Email) &&
                    !string.IsNullOrWhiteSpace(u.RoleName) &&
                    u.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                    u.UserId != assignedUserId);

                foreach (var admin in admins)
                    recipients.Add((admin.UserId, admin.Email, admin.UserName));

                var subject = $"Test case {testCaseId} failed (scheduled run)";
                var bodyHtml =
                    $"<p>Scheduled test case <strong>{testCaseId}</strong> (AssignmentTestCaseId {assignmentTestCaseId}) failed.</p>" +
                    $"<p><strong>Error:</strong><br/>{System.Net.WebUtility.HtmlEncode(errorMessage ?? "(no message)")}</p>" +
                    "<p>You can retry it directly from the Test Case Execution Panel.</p>";

                foreach (var (userId, email, userName) in recipients)
                {
                    var notificationId = await AddNotificationAsync(
                        assignmentTestCaseId, "ScheduledRunFailed", userId, email,
                        $"Notify {userName}: {subject}");

                    try
                    {
                        await _emailService.SendAsync(email, subject, bodyHtml);
                        await MarkNotificationAsync(notificationId, "Sent");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to email test failure notification to {Email}", email);
                        await MarkNotificationAsync(notificationId, "Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                // Notification failure must never affect the actual test result being
                // recorded - same defensive reasoning as ReleaseNotificationService.
                _logger.LogError(ex, "Failed to dispatch scheduled-failure notification for AssignmentTestCaseId {AssignmentTestCaseId}", assignmentTestCaseId);
            }
        }

        private async Task<int> AddNotificationAsync(int assignmentTestCaseId, string notificationType, int? recipientUserId, string recipientEmail, string message)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentTestCaseId", assignmentTestCaseId),
                new SqlParameter("@NotificationType", notificationType),
                new SqlParameter("@RecipientUserId", (object?)recipientUserId ?? DBNull.Value),
                new SqlParameter("@RecipientEmail", (object?)recipientEmail ?? DBNull.Value),
                new SqlParameter("@Message", (object?)message ?? DBNull.Value)
            };
            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.TestExecutionNotificationAdd, parameters);
        }

        private async Task MarkNotificationAsync(int testExecutionNotificationId, string status)
        {
            var parameters = new[]
            {
                new SqlParameter("@TestExecutionNotificationId", testExecutionNotificationId),
                new SqlParameter("@Status", status)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.TestExecutionNotificationMarkSent, parameters);
        }
    }
}
