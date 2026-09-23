using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SqlDataAccessHelper _db;

        public NotificationRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }

        public async Task<int> AddAsync(int userId, string notificationType, string title, string message, string linkUrl, string sourceType, int? sourceId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@NotificationType", notificationType),
                new SqlParameter("@Title", title),
                new SqlParameter("@Message", (object)message ?? DBNull.Value),
                new SqlParameter("@LinkUrl", (object)linkUrl ?? DBNull.Value),
                new SqlParameter("@SourceType", sourceType),
                new SqlParameter("@SourceId", (object)sourceId ?? DBNull.Value)
            };
            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.NotificationAdd, parameters);
        }

        public async Task<IEnumerable<NotificationModel>> GetByUserAsync(int userId, bool unreadOnly)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@UnreadOnly", unreadOnly)
            };
            return await _db.ExecuteReaderAsync(SqlDbConstants.NotificationGetByUser, parameters, reader =>
                new NotificationModel
                {
                    NotificationId = reader.GetInt32(reader.GetOrdinal("NotificationId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    NotificationType = reader.GetNullableString("NotificationType") ?? string.Empty,
                    Title = reader.GetNullableString("Title") ?? string.Empty,
                    Message = reader.GetNullableString("Message") ?? string.Empty,
                    LinkUrl = reader.GetNullableString("LinkUrl") ?? string.Empty,
                    SourceType = reader.GetNullableString("SourceType") ?? string.Empty,
                    SourceId = reader.GetNullableInt("SourceId"),
                    IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                    ReadOn = reader.GetNullableDateTime("ReadOn"),
                    CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn"))
                });
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var parameters = new[] { new SqlParameter("@UserId", userId) };
            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.NotificationGetUnreadCount, parameters);
        }

        public async Task MarkReadAsync(int notificationId, int userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@NotificationId", notificationId),
                new SqlParameter("@UserId", userId)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.NotificationMarkRead, parameters);
        }

        public async Task MarkAllReadAsync(int userId)
        {
            var parameters = new[] { new SqlParameter("@UserId", userId) };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.NotificationMarkAllRead, parameters);
        }
    }
}
