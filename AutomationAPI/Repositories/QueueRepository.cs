using System.Data;
using Microsoft.Data.SqlClient;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using System.Reflection.PortableExecutable;

namespace AutomationAPI.Repositories
{
    public class QueueRepository : IQueueRepository
    {
        private readonly SqlDataAccessHelper _sqlDataAccessHelper;

        public QueueRepository(SqlDataAccessHelper sqlDataAccessHelper)
        {
            _sqlDataAccessHelper = sqlDataAccessHelper;
        }

        public async Task<IEnumerable<QueueInfo>> GetAllQueuesAsync()
        {
            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAllQueues, null, reader => new QueueInfo
            {
                QueueId = reader.GetNullableString("QueueId"),
                TagName = reader.GetNullableString("TagName"),
                EmpId = reader.GetNullableString("EmpId"),
                QueueName = reader.GetNullableString("QueueName"),
                QueueDescription = reader.GetNullableString("QueueDescription"),
                ProductLine = reader.GetNullableString("ProductLine"),
                QueueStatus = reader.GetNullableString("QueueStatus"),
                CreatedDate = reader.GetNullableDateTime("CreatedDate"),
                Id = reader.GetNullableInt("Id") ?? 0, // Or use int? in QueueInfo model
                LibraryName = reader.GetNullableString("LibraryName"),
                ClassName = reader.GetNullableString("ClassName"),
                MethodName = reader.GetNullableString("MethodName")
            });
        }

        public async Task<PagedResult<QueueInfo>> SearchQueuesAsync(QueueSearchPayload payload)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId",payload.UserId),
                new SqlParameter("@Page", payload.Page),
                new SqlParameter("@PageSize", payload.PageSize),
                new SqlParameter("@SortColumn", payload.SortColumn),
                new SqlParameter("@SortDirection", payload.SortDirection),
            };


            IEnumerable<QueueInfo> results = new List<QueueInfo>();
            int totalCount = 0;

            results = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.SearchQueues, parameters.ToArray(),
                   reader =>
                   {
                       if (totalCount == 0 && !reader.IsDBNull(reader.GetOrdinal("TotalCount")))
                       {
                           totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                       }

                       return new QueueInfo
                       {
                           QueueId = reader.GetNullableString("QueueId"),
                           TagName = reader.GetNullableString("TagName"),
                           QueueName = reader.GetNullableString("QueueName"),
                           QueueDescription = reader.GetNullableString("QueueDescription"),
                           ProductLine = reader.GetNullableString("ProductLine"),
                           QueueStatus = reader.GetNullableString("QueueStatus"),
                           CreatedDate = reader.GetNullableDateTime("CreatedDate"),
                           Id = reader.GetNullableInt("Id") ?? 0,
                           LibraryName = reader.GetNullableString("LibraryName"),
                           ClassName = reader.GetNullableString("ClassName"),
                           MethodName = reader.GetNullableString("MethodName"),
                           UserName = reader.GetNullableString("UserName"),
                           UserId = reader.GetNullableInt("UserID") ?? 0
                       };
                   });

            return new PagedResult<QueueInfo>
            {
                Data = results,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<QueueInfo>> GetQueuesAsync(string? queueId, string? queueStatus)
        {
            var parameters = new[]
            {
                new SqlParameter("@QueueId", queueId ?? string.Empty),
                new SqlParameter("@QueueStatus", queueStatus ?? string.Empty)
            };

            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetQueues, parameters, reader => new QueueInfo
            {
                QueueId = reader.GetNullableString("QueueId"),
                TagName = reader.GetNullableString("TagName"),
                EmpId = reader.GetNullableString("EmpId"),
                QueueName = reader.GetNullableString("QueueName"),
                QueueDescription = reader.GetNullableString("QueueDescription"),
                ProductLine = reader.GetNullableString("ProductLine"),
                QueueStatus = reader.GetNullableString("QueueStatus"),
                CreatedDate = reader.GetNullableDateTime("CreatedDate"),
                Id = reader.GetNullableInt("Id") ?? 0, // Or use int? in QueueInfo model
                LibraryName = reader.GetNullableString("LibraryName"),
                ClassName = reader.GetNullableString("ClassName"),
                MethodName = reader.GetNullableString("MethodName")
            });

        }

        public async Task<QueueInfo> GetQueueByIdAsync(string queueId)
        {
            // Ensure null or empty values are properly handled
            var parameters = new[]
            {
                new SqlParameter("@QueueId", string.IsNullOrWhiteSpace(queueId) ? (object)DBNull.Value : queueId)
            };

            var queues = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetQueueById, parameters, reader => new QueueInfo
            {
                QueueId = reader.GetNullableString("QueueId"),
                TagName = reader.GetNullableString("TagName"),
                EmpId = reader.GetNullableString("EmpId"),
                QueueName = reader.GetNullableString("QueueName"),
                QueueDescription = reader.GetNullableString("QueueDescription"),
                ProductLine = reader.GetNullableString("ProductLine"),
                QueueStatus = reader.GetNullableString("QueueStatus"),
                CreatedDate = reader.GetNullableDateTime("CreatedDate"),
                Id = reader.GetNullableInt("Id") ?? 0, // Default to 0 if null
                LibraryName = reader.GetNullableString("LibraryName"),
                ClassName = reader.GetNullableString("ClassName"),
                MethodName = reader.GetNullableString("MethodName")
            });

            return queues.FirstOrDefault();
        }

        public async Task<int> InsertQueueAsync(QueueInfo queue)
        {
            var parameters = new[]
            {
                new SqlParameter("@CreatedDate", SqlDbType.DateTime) { Value = (object?)queue.CreatedDate ?? DBNull.Value },
                new SqlParameter("@QueueId", SqlDbType.NVarChar, 50) { Value = (object?)queue.QueueId ?? DBNull.Value },
                new SqlParameter("@TagName", SqlDbType.NVarChar, 150) { Value = (object?)queue.TagName ?? DBNull.Value },
                new SqlParameter("@EmpId", SqlDbType.NVarChar, 50) { Value = (object?)queue.EmpId ?? DBNull.Value },
                new SqlParameter("@QueueName", SqlDbType.NVarChar, 150) { Value = (object?)queue.QueueName ?? DBNull.Value },
                new SqlParameter("@QueueDescription", SqlDbType.NVarChar, 500) { Value = (object?)queue.QueueDescription ?? DBNull.Value },
                new SqlParameter("@ProductLine", SqlDbType.NVarChar, 150) { Value = (object?)queue.ProductLine ?? DBNull.Value },
                new SqlParameter("@QueueStatus", SqlDbType.NVarChar, 150) { Value = (object?)queue.QueueStatus ?? DBNull.Value },
                new SqlParameter("@LibraryName", SqlDbType.NVarChar, -1) { Value = (object?)queue.LibraryName ?? DBNull.Value },
                new SqlParameter("@MethodName", SqlDbType.NVarChar, -1) { Value = (object?)queue.MethodName ?? DBNull.Value },
                new SqlParameter("@ClassName", SqlDbType.NVarChar, -1) { Value = (object?)queue.ClassName ?? DBNull.Value },
                new SqlParameter("@UserID", queue.UserId),

            };

            return await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.InsertQueue, parameters);
        }
        public async Task<int> UpdateQueueStatusAsync(string queueId, string queueStatus)
        {
            var parameters = new[]
            {
                new SqlParameter("@QueueId", SqlDbType.NVarChar, 150) { Value = queueId },
                new SqlParameter("@QueueStatus", SqlDbType.NVarChar, 150) { Value = queueStatus }
            };

            return await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.UpdateQueue, parameters);
        }

        public async Task<bool> DeleteQueueAsync(string queueId)
        {


            var parameters = new[]
            {
                new SqlParameter("@QueueId", SqlDbType.NVarChar, 150) { Value = queueId }
            };

            int rowsAffected = await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.DeleteQueue, parameters);

            return rowsAffected > 0;
        }

        public async Task<PagedResult<QueueInfo>> GetQueueReportsAsync(QueueReportFilterRequest filter)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId",filter.UserId),

                new SqlParameter("@Status", SqlDbType.NVarChar, 100)
                {
                    Value = filter.Status ?? (object)DBNull.Value
                },
                new SqlParameter("@FromDate", SqlDbType.DateTime)
                {
                    Value = filter.FromDate ?? (object)DBNull.Value
                },
                new SqlParameter("@ToDate", SqlDbType.DateTime)
                {
                    Value = filter.ToDate ?? (object)DBNull.Value
                },
                new SqlParameter("@Page", SqlDbType.Int)
                {
                    Value = filter.Page
                },
                new SqlParameter("@PageSize", SqlDbType.Int)
                {
                    Value = filter.PageSize
                }
            };

            IEnumerable<QueueInfo> results = new List<QueueInfo>();
            int totalCount = 0;

            results = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetQueueReports, parameters.ToArray(),
                   reader =>
                   {
                       if (totalCount == 0 && !reader.IsDBNull(reader.GetOrdinal("TotalCount")))
                       {
                           totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                       }

                       return new QueueInfo
                       {
                           QueueId = reader.GetNullableString("QueueId"),
                           TagName = reader.GetNullableString("TagName"),
                           QueueName = reader.GetNullableString("QueueName"),
                           QueueDescription = reader.GetNullableString("QueueDescription"),
                           ProductLine = reader.GetNullableString("ProductLine"),
                           QueueStatus = reader.GetNullableString("QueueStatus"),
                           CreatedDate = reader.GetNullableDateTime("CreatedDate"),
                           Id = reader.GetNullableInt("Id") ?? 0,
                           LibraryName = reader.GetNullableString("LibraryName"),
                           ClassName = reader.GetNullableString("ClassName"),
                           MethodName = reader.GetNullableString("MethodName"),
                           UserName = reader.GetNullableString("UserName"),
                           UserId = reader.GetNullableInt("UserID") ?? 0
                       };
                   });

            return new PagedResult<QueueInfo>
            {
                Data = results,
                TotalCount = totalCount
            };
        }

    }
}
