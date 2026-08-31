using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class ReleaseRepository : IReleaseRepository
    {
        private readonly SqlDataAccessHelper _db;

        public ReleaseRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(ReleaseRequestDto releaseRequest)
        {
            // Folder path is not known until after the ReleaseId is assigned (folder name
            // includes ReleaseId), so it is stored separately via SetFolderPathAsync.
            var parameters = new[]
            {
                new SqlParameter("@ReleaseName", releaseRequest.ReleaseName),
                new SqlParameter("@Version", (object)releaseRequest.Version ?? DBNull.Value),
                new SqlParameter("@EnvironmentId", (object)releaseRequest.EnvironmentId ?? DBNull.Value),
                new SqlParameter("@Description", (object)releaseRequest.Description ?? DBNull.Value),
                new SqlParameter("@ReleaseFolderPath", DBNull.Value),
                new SqlParameter("@CreatedBy", (object)releaseRequest.CreatedBy ?? DBNull.Value)
            };

            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.CreateRelease, parameters);
        }

        public async Task SetFolderPathAsync(int releaseId, string releaseFolderPath)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseId", releaseId),
                new SqlParameter("@ReleaseFolderPath", releaseFolderPath)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.ReleaseSetFolderPath, parameters);
        }

        public async Task DeleteAsync(int releaseId)
        {
            var parameters = new[] { new SqlParameter("@ReleaseId", releaseId) };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.DeleteRelease, parameters);
        }

        public async Task UpdateAsync(ReleaseRequestDto releaseRequest)
        {
            // ReleaseFolderPath is intentionally NOT modified here (Update never renames the
            // folder); usp_UpdateRelease preserves the existing path via COALESCE when NULL.
            var parameters = new[]
            {
                new SqlParameter("@ReleaseId", releaseRequest.ReleaseId),
                new SqlParameter("@ReleaseName", releaseRequest.ReleaseName),
                new SqlParameter("@Version", (object)releaseRequest.Version ?? DBNull.Value),
                new SqlParameter("@EnvironmentId", (object)releaseRequest.EnvironmentId ?? DBNull.Value),
                new SqlParameter("@Description", (object)releaseRequest.Description ?? DBNull.Value),
                new SqlParameter("@ReleaseFolderPath", DBNull.Value),
                new SqlParameter("@ReleaseLifecycle", (object)releaseRequest.ReleaseLifecycle ?? DBNull.Value),
                new SqlParameter("@IsActive", (object)releaseRequest.IsActive ?? DBNull.Value),
                new SqlParameter("@ModifiedBy", (object)releaseRequest.ModifiedBy ?? DBNull.Value)
            };

            await _db.ExecuteNonQueryAsync(SqlDbConstants.UpdateRelease, parameters);
        }

        public async Task<IEnumerable<ReleaseModel>> GetAllAsync()
        {
            return await _db.ExecuteReaderAsync(SqlDbConstants.GetAllReleases, [], MapRelease);
        }

        public async Task<ReleaseModel> GetByIdAsync(int releaseId)
        {
            var parameters = new[] { new SqlParameter("@ReleaseId", releaseId) };
            var result = await _db.ExecuteReaderAsync(SqlDbConstants.GetReleaseById, parameters, MapRelease);
            return result.FirstOrDefault();
        }

        public async Task ActivateAsync(int releaseId, string activatedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseId", releaseId),
                new SqlParameter("@ActivatedBy", (object)activatedBy ?? DBNull.Value)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.ActivateRelease, parameters);
        }

        public async Task SignOffAsync(int releaseId, ReleaseSignOffRequestDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseId", releaseId),
                new SqlParameter("@SignOffStatus", request.SignOffStatus),
                new SqlParameter("@SignedOffBy", (object)request.SignOffBy ?? DBNull.Value),
                new SqlParameter("@Comments", (object)request.Comments ?? DBNull.Value)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.SignOffRelease, parameters);
        }

        public async Task<IEnumerable<ReleaseSignOffModel>> GetSignOffHistoryAsync(int releaseId)
        {
            var parameters = new[] { new SqlParameter("@ReleaseId", releaseId) };
            return await _db.ExecuteReaderAsync(SqlDbConstants.GetReleaseSignOffHistory, parameters, reader =>
                new ReleaseSignOffModel
                {
                    ReleaseSignOffId = reader.GetInt32(reader.GetOrdinal("ReleaseSignOffId")),
                    ReleaseId = reader.GetInt32(reader.GetOrdinal("ReleaseId")),
                    SignOffStatus = reader.GetNullableString("SignOffStatus") ?? string.Empty,
                    SignOffBy = reader.GetNullableString("SignOffBy") ?? string.Empty,
                    SignOffOn = reader.GetNullableDateTime("SignOffOn"),
                    Comments = reader.GetNullableString("Comments") ?? string.Empty,
                    CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn"))
                });
        }

        public async Task<int> AddNotificationAsync(int releaseId, string notificationType, int? recipientUserId, string recipientEmail, string message)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseId", releaseId),
                new SqlParameter("@NotificationType", notificationType),
                new SqlParameter("@RecipientUserId", (object)recipientUserId ?? DBNull.Value),
                new SqlParameter("@RecipientEmail", (object)recipientEmail ?? DBNull.Value),
                new SqlParameter("@Message", (object)message ?? DBNull.Value)
            };
            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.ReleaseNotificationAdd, parameters);
        }

        public async Task<IEnumerable<ReleaseNotificationModel>> GetNotificationsAsync(int releaseId)
        {
            var parameters = new[] { new SqlParameter("@ReleaseId", releaseId) };
            return await _db.ExecuteReaderAsync(SqlDbConstants.ReleaseNotificationGetByRelease, parameters, reader =>
                new ReleaseNotificationModel
                {
                    ReleaseNotificationId = reader.GetInt32(reader.GetOrdinal("ReleaseNotificationId")),
                    ReleaseId = reader.GetInt32(reader.GetOrdinal("ReleaseId")),
                    NotificationType = reader.GetNullableString("NotificationType") ?? string.Empty,
                    RecipientUserId = reader.GetNullableInt("RecipientUserId"),
                    RecipientEmail = reader.GetNullableString("RecipientEmail") ?? string.Empty,
                    Status = reader.GetNullableString("Status") ?? string.Empty,
                    Message = reader.GetNullableString("Message") ?? string.Empty,
                    CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                    SentOn = reader.GetNullableDateTime("SentOn")
                });
        }

        public async Task MarkNotificationAsync(int releaseNotificationId, string status)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseNotificationId", releaseNotificationId),
                new SqlParameter("@Status", status)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.ReleaseNotificationMarkSent, parameters);
        }

        private static ReleaseModel MapRelease(SqlDataReader reader)
        {
            return new ReleaseModel
            {
                ReleaseId = reader.GetInt32(reader.GetOrdinal("ReleaseId")),
                ReleaseName = reader.GetString(reader.GetOrdinal("ReleaseName")),
                Version = reader.GetNullableString("Version") ?? string.Empty,
                EnvironmentId = reader.GetNullableInt("EnvironmentId"),
                EnvironmentName = reader.GetNullableString("EnvironmentName") ?? string.Empty,
                Description = reader.GetNullableString("Description") ?? string.Empty,
                ReleaseFolderPath = reader.GetNullableString("ReleaseFolderPath") ?? string.Empty,
                ReleaseLifecycle = reader.GetNullableString("ReleaseLifecycle") ?? string.Empty,
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                SignOffStatus = reader.GetNullableString("SignOffStatus") ?? string.Empty,
                SignedOffBy = reader.GetNullableString("SignedOffBy") ?? string.Empty,
                SignedOffOn = reader.GetNullableDateTime("SignedOffOn"),
                CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                CreatedBy = reader.GetNullableString("CreatedBy") ?? string.Empty,
                ModifiedBy = reader.GetNullableString("ModifiedBy") ?? string.Empty,
                ModifiedOn = reader.GetNullableDateTime("ModifiedOn"),
                ActivatedBy = reader.GetNullableString("ActivatedBy") ?? string.Empty,
                ActivatedOn = reader.GetNullableDateTime("ActivatedOn"),
                TotalTests = reader.GetNullableInt("TotalTests") ?? 0,
                PassedTests = reader.GetNullableInt("PassedTests") ?? 0,
                FailedTests = reader.GetNullableInt("FailedTests") ?? 0,
                SkippedTests = reader.GetNullableInt("SkippedTests") ?? 0,
                RunningTests = reader.GetNullableInt("RunningTests") ?? 0
            };
        }
    }
}
