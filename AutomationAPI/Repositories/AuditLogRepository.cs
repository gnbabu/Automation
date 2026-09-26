using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly SqlDataAccessHelper _db;

        public AuditLogRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }

        public async Task<int> InsertAsync(
            string entityType, int? entityId, string entityName, string action,
            int? actorUserId, string actorUserName, string? details)
        {
            var parameters = new[]
            {
                new SqlParameter("@EntityType", entityType),
                new SqlParameter("@EntityId", (object?)entityId ?? DBNull.Value),
                new SqlParameter("@EntityName", entityName),
                new SqlParameter("@Action", action),
                new SqlParameter("@ActorUserId", (object?)actorUserId ?? DBNull.Value),
                new SqlParameter("@ActorUserName", actorUserName),
                new SqlParameter("@Details", (object?)details ?? DBNull.Value)
            };
            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.AuditLogInsert, parameters);
        }

        public async Task<AuditLogPagedResult> GetPagedAsync(AuditLogFilter filter)
        {
            var parameters = new[]
            {
                new SqlParameter("@EntityType", (object?)filter.EntityType ?? DBNull.Value),
                new SqlParameter("@EntityId", (object?)filter.EntityId ?? DBNull.Value),
                new SqlParameter("@ActorUserId", (object?)filter.ActorUserId ?? DBNull.Value),
                new SqlParameter("@Action", (object?)filter.Action ?? DBNull.Value),
                new SqlParameter("@FromDate", (object?)filter.FromDate ?? DBNull.Value),
                new SqlParameter("@ToDate", (object?)filter.ToDate ?? DBNull.Value),
                new SqlParameter("@PageNumber", filter.PageNumber),
                new SqlParameter("@PageSize", filter.PageSize)
            };

            var totalCount = 0;
            var items = await _db.ExecuteReaderAsync(SqlDbConstants.AuditLogGetPaged, parameters, reader =>
            {
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                return new AuditLogModel
                {
                    AuditLogId = reader.GetInt32(reader.GetOrdinal("AuditLogId")),
                    EntityType = reader.GetNullableString("EntityType") ?? string.Empty,
                    EntityId = reader.GetNullableInt("EntityId"),
                    EntityName = reader.GetNullableString("EntityName") ?? string.Empty,
                    Action = reader.GetNullableString("Action") ?? string.Empty,
                    ActorUserId = reader.GetNullableInt("ActorUserId"),
                    ActorUserName = reader.GetNullableString("ActorUserName") ?? string.Empty,
                    Details = reader.GetNullableString("Details"),
                    CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn"))
                };
            });

            return new AuditLogPagedResult { Items = items, TotalCount = totalCount };
        }

        public async Task<IEnumerable<string>> GetDistinctEntityTypesAsync()
        {
            return await _db.ExecuteReaderAsync(SqlDbConstants.AuditLogGetDistinctEntityTypes, null,
                reader => reader.GetNullableString("EntityType") ?? string.Empty);
        }
    }
}
