using Microsoft.Data.SqlClient;
using System.Data;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;

namespace AutomationAPI.Repositories
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly SqlDataAccessHelper _sqlDataAccessHelper;

        public TestResultRepository(SqlDataAccessHelper sqlDataAccessHelper)
        {
            _sqlDataAccessHelper = sqlDataAccessHelper;
        }

        public async Task<PagedResult<TestResult>> SearchTestResultsAsync(TestResultPayload payload)
        {
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@UserId", payload.UserId),
                    new SqlParameter("@Page", payload.Page),
                    new SqlParameter("@PageSize", payload.PageSize),
                    new SqlParameter("@SortColumn", payload.SortColumn),
                    new SqlParameter("@SortDirection", payload.SortDirection),
                };

                IEnumerable<TestResult> results = new List<TestResult>();
                int totalCount = 0;

                results = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.SearchTestResults, parameters.ToArray(),
                    reader =>
                    {
                        if (totalCount == 0 && !reader.IsDBNull(reader.GetOrdinal("TotalCount")))
                        {
                            totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                        }

                        return new TestResult
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetNullableString("Name"),
                            ResultStatus = reader.GetNullableString("ResultStatus"),
                            Duration = reader.GetNullableString("Duration"),
                            StartTime = reader.GetNullableDateTime("StartTime"),
                            EndTime = reader.GetNullableDateTime("EndTime"),
                            Message = reader.GetNullableString("Message"),
                            ClassName = reader.GetNullableString("ClassName"),
                            QueueId = reader.GetNullableString("QueueId")
                        };
                    });

                return new PagedResult<TestResult>
                {
                    Data = results,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                // Log if needed
                // Return default or rethrow
                throw;
            }
        }


        public async Task<IEnumerable<TestResult>> GetAllTestResultsAsync()
        {
            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAllTestResults, [], reader => new TestResult
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetNullableString("Name"),
                ResultStatus = reader.GetNullableString("ResultStatus"),
                Duration = reader.GetNullableString("Duration"),
                StartTime = reader.GetNullableDateTime("StartTime"),
                EndTime = reader.GetNullableDateTime("EndTime"),
                Message = reader.GetNullableString("Message"),
                ClassName = reader.GetNullableString("ClassName"),
                QueueId = reader.GetNullableString("QueueId")
            });
        }

        public async Task<IEnumerable<TestResult>> GetTestResultsByQueueIdAsync(string queueId)
        {
            var parameters = new[]
            {
                new SqlParameter("@QueueId", SqlDbType.NVarChar, 150) { Value = queueId }
            };

            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetTestResults, parameters, reader => new TestResult
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetNullableString("Name"),
                ResultStatus = reader.GetNullableString("ResultStatus"),
                Duration = reader.GetNullableString("Duration"),
                StartTime = reader.GetNullableDateTime("StartTime"),
                EndTime = reader.GetNullableDateTime("EndTime"),
                Message = reader.GetNullableString("Message"),
                ClassName = reader.GetNullableString("ClassName"),
                QueueId = reader.GetNullableString("QueueId")
            });
        }

        public async Task<int> InsertTestResultsAsync(TestResult testResult)
        {
            var parameters = new[]
          {
                new SqlParameter("@Name", SqlDbType.NVarChar, 150) { Value = testResult.Name ?? (object)DBNull.Value },
                new SqlParameter("@ResultStatus", SqlDbType.NVarChar, 50) { Value = testResult.ResultStatus ?? (object)DBNull.Value },
                new SqlParameter("@Duration", SqlDbType.NVarChar, 50) { Value = testResult.Duration ?? (object)DBNull.Value },
                new SqlParameter("@StartTime", SqlDbType.DateTime) { Value = testResult.StartTime ?? (object)DBNull.Value },
                new SqlParameter("@EndTime", SqlDbType.DateTime) { Value = testResult.EndTime ?? (object)DBNull.Value },
                new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Value = testResult.Message ?? (object)DBNull.Value },
                new SqlParameter("@ClassName", SqlDbType.NVarChar, 150) { Value = testResult.ClassName ?? (object)DBNull.Value },
                new SqlParameter("@QueueId", SqlDbType.NVarChar, 150) { Value = testResult.QueueId ?? (object)DBNull.Value }
            };

            return await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.InsertTestResults, parameters);
        }
    }
}
