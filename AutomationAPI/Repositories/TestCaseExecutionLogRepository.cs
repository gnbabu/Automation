using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class TestCaseExecutionLogRepository : ITestCaseExecutionLogRepository
    {
        private readonly SqlDataAccessHelper _db;

        public TestCaseExecutionLogRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }
        public async Task AddAsync(TestCaseExecutionLog log)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", log.AssignmentId),
                new SqlParameter("@AssignmentTestCaseId", log.AssignmentTestCaseId),
                new SqlParameter("@StepName", log.StepName),
                new SqlParameter("@LogMessage", log.LogMessage),
                new SqlParameter("@LogLevel", log.LogLevel.ToString()),
                new SqlParameter("@ExecutionStatus", log.ExecutionStatus.ToString()),
                new SqlParameter("@ScreenshotId", (object?)log.ScreenshotId ?? DBNull.Value),
                new SqlParameter("@ErrorStackTrace", (object?)log.ErrorStackTrace ?? DBNull.Value)
            };

            await _db.ExecuteNonQueryAsync(SqlDbConstants.AddTestCaseExecutionLog, parameters);
        }

        public async Task<IEnumerable<TestCaseExecutionLog>> GetByTestCaseAsync(int assignmentId, int assignmentTestCaseId)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId),
                new SqlParameter("@AssignmentTestCaseId", assignmentTestCaseId)
            };

            return await _db.ExecuteReaderAsync(SqlDbConstants.GetTestCaseExecutionLogs, parameters, MapLog);
        }

        public async Task<IEnumerable<TestCaseExecutionLog>> GetByAssignmentAsync(int assignmentId)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId)
            };

            return await _db.ExecuteReaderAsync(SqlDbConstants.GetAssignmentExecutionLogs, parameters, MapLog);
        }

        public async Task<IEnumerable<ReleaseExecutionLogDto>> GetByReleaseAsync(string releaseName)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseName", releaseName)
            };

            return await _db.ExecuteReaderAsync(SqlDbConstants.GetReleaseExecutionLogs, parameters,
                reader => new ReleaseExecutionLogDto
                {
                    ReleaseName = reader.GetString(reader.GetOrdinal("ReleaseName")),
                    AssignmentId = reader.GetInt32(reader.GetOrdinal("AssignmentId")),
                    AssignmentTestCaseId = reader.GetInt32(reader.GetOrdinal("AssignmentTestCaseId")),
                    TestCaseId = reader.GetString(reader.GetOrdinal("TestCaseId")),
                    TestCaseDescription = reader.GetString(reader.GetOrdinal("TestCaseDescription")),
                    StepName = reader.GetString(reader.GetOrdinal("StepName")),
                    LogMessage = reader.GetString(reader.GetOrdinal("LogMessage")),
                    LogLevel = Enum.Parse<TestCaseLogLevel>(reader.GetString(reader.GetOrdinal("LogLevel")), true),
                    ExecutionStatus = Enum.Parse<ExecutionStatus>(reader.GetString(reader.GetOrdinal("ExecutionStatus")), true),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
        }

        private static TestCaseExecutionLog MapLog(SqlDataReader reader)
        {
            return new TestCaseExecutionLog
            {
                LogId = reader.GetInt64(reader.GetOrdinal("LogId")),
                AssignmentId = reader.GetInt32(reader.GetOrdinal("AssignmentId")),
                AssignmentTestCaseId = reader.GetInt32(reader.GetOrdinal("AssignmentTestCaseId")),
                TestCaseId = reader.GetString(reader.GetOrdinal("TestCaseId")),
                TestCaseDescription = reader.GetString(reader.GetOrdinal("TestCaseDescription")),
                StepName = reader.GetString(reader.GetOrdinal("StepName")),
                LogMessage = reader.GetString(reader.GetOrdinal("LogMessage")),
                LogLevel = Enum.Parse<TestCaseLogLevel>(reader.GetString(reader.GetOrdinal("LogLevel")), true),
                ExecutionStatus = Enum.Parse<ExecutionStatus>(reader.GetString(reader.GetOrdinal("ExecutionStatus")), true),
                ScreenshotId = reader.IsDBNull(reader.GetOrdinal("ScreenshotId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("ScreenshotId")),
                ErrorStackTrace = reader.IsDBNull(reader.GetOrdinal("ErrorStackTrace"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ErrorStackTrace")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

    }
}
