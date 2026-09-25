using System.Data;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class RecurringScheduleRepository : IRecurringScheduleRepository
    {
        private readonly SqlDataAccessHelper _db;

        public RecurringScheduleRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(int assignmentId, string recurrenceType, string daysOfWeek, int? dayOfMonth, TimeSpan timeOfDay, string browser, int? loginUserId, DateTime? endDate, DateTime nextRunDate, string createdBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId),
                new SqlParameter("@RecurrenceType", recurrenceType),
                new SqlParameter("@DaysOfWeek", (object)daysOfWeek ?? DBNull.Value),
                new SqlParameter("@DayOfMonth", (object)dayOfMonth ?? DBNull.Value),
                new SqlParameter("@TimeOfDay", timeOfDay),
                new SqlParameter("@Browser", browser),
                new SqlParameter("@LoginUserId", (object)loginUserId ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)endDate ?? DBNull.Value),
                new SqlParameter("@NextRunDate", nextRunDate),
                new SqlParameter("@CreatedBy", (object)createdBy ?? DBNull.Value)
            };
            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.RecurringScheduleCreate, parameters);
        }

        public async Task<IEnumerable<RecurringScheduleModel>> GetAllAsync()
        {
            return await _db.ExecuteReaderAsync(SqlDbConstants.RecurringScheduleGetAll, [], MapRecurringSchedule);
        }

        public async Task<RecurringScheduleModel> GetByIdAsync(int recurringScheduleId)
        {
            var parameters = new[] { new SqlParameter("@RecurringScheduleId", recurringScheduleId) };
            var result = await _db.ExecuteReaderAsync(SqlDbConstants.RecurringScheduleGetById, parameters, MapRecurringSchedule);
            return result.FirstOrDefault();
        }

        public async Task UpdateAsync(int recurringScheduleId, string recurrenceType, string daysOfWeek, int? dayOfMonth, TimeSpan timeOfDay, string browser, int? loginUserId, DateTime? endDate, DateTime nextRunDate)
        {
            var parameters = new[]
            {
                new SqlParameter("@RecurringScheduleId", recurringScheduleId),
                new SqlParameter("@RecurrenceType", recurrenceType),
                new SqlParameter("@DaysOfWeek", (object)daysOfWeek ?? DBNull.Value),
                new SqlParameter("@DayOfMonth", (object)dayOfMonth ?? DBNull.Value),
                new SqlParameter("@TimeOfDay", timeOfDay),
                new SqlParameter("@Browser", browser),
                new SqlParameter("@LoginUserId", (object)loginUserId ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)endDate ?? DBNull.Value),
                new SqlParameter("@NextRunDate", nextRunDate)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.RecurringScheduleUpdate, parameters);
        }

        private static RecurringScheduleModel MapRecurringSchedule(SqlDataReader reader) =>
            new RecurringScheduleModel
            {
                RecurringScheduleId = reader.GetInt32(reader.GetOrdinal("RecurringScheduleId")),
                AssignmentId = reader.GetInt32(reader.GetOrdinal("AssignmentId")),
                AssignmentName = reader.GetNullableString("AssignmentName") ?? string.Empty,
                Environment = reader.GetNullableString("Environment") ?? string.Empty,
                EnvironmentId = reader.GetNullableInt("EnvironmentId"),
                ReleaseName = reader.GetNullableString("ReleaseName") ?? string.Empty,
                ReleaseLifecycle = reader.GetNullableString("ReleaseLifecycle") ?? string.Empty,
                RecurrenceType = reader.GetNullableString("RecurrenceType") ?? string.Empty,
                DaysOfWeek = reader.GetNullableString("DaysOfWeek"),
                DayOfMonth = reader.GetNullableInt("DayOfMonth"),
                TimeOfDay = reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("TimeOfDay")),
                Browser = reader.GetNullableString("Browser") ?? string.Empty,
                LoginUserId = reader.GetNullableInt("LoginUserId"),
                LoginUserRole = reader.GetNullableString("LoginUserRole"),
                LoginUserName = reader.GetNullableString("LoginUserName"),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                PausedReason = reader.GetNullableString("PausedReason"),
                EndDate = reader.GetNullableDateTime("EndDate"),
                NextRunDate = reader.GetDateTime(reader.GetOrdinal("NextRunDate")),
                LastRunDate = reader.GetNullableDateTime("LastRunDate"),
                RunCount = reader.GetInt32(reader.GetOrdinal("RunCount")),
                CreatedBy = reader.GetNullableString("CreatedBy"),
                CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn"))
            };

        public async Task SetActiveAsync(int recurringScheduleId, bool isActive, string pausedReason = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@RecurringScheduleId", recurringScheduleId),
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@PausedReason", (object)pausedReason ?? DBNull.Value)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.RecurringScheduleSetActive, parameters);
        }

        public async Task DeleteAsync(int recurringScheduleId)
        {
            var parameters = new[] { new SqlParameter("@RecurringScheduleId", recurringScheduleId) };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.RecurringScheduleDelete, parameters);
        }

        public async Task<IEnumerable<RecurringScheduleDueModel>> GetDueAsync(DateTime now)
        {
            var parameters = new[] { new SqlParameter("@Now", now) };
            return await _db.ExecuteReaderAsync(SqlDbConstants.RecurringScheduleGetDue, parameters, reader =>
                new RecurringScheduleDueModel
                {
                    RecurringScheduleId = reader.GetInt32(reader.GetOrdinal("RecurringScheduleId")),
                    AssignmentId = reader.GetInt32(reader.GetOrdinal("AssignmentId")),
                    AssignmentName = reader.GetNullableString("AssignmentName") ?? string.Empty,
                    ReleaseName = reader.GetNullableString("ReleaseName") ?? string.Empty,
                    ReleaseLifecycle = reader.GetNullableString("ReleaseLifecycle"),
                    RecurrenceType = reader.GetNullableString("RecurrenceType") ?? string.Empty,
                    DaysOfWeek = reader.GetNullableString("DaysOfWeek"),
                    DayOfMonth = reader.GetNullableInt("DayOfMonth"),
                    TimeOfDay = reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("TimeOfDay")),
                    Browser = reader.GetNullableString("Browser") ?? string.Empty,
                    LoginUserId = reader.GetNullableInt("LoginUserId"),
                    EndDate = reader.GetNullableDateTime("EndDate")
                });
        }

        public async Task MarkRunAsync(int recurringScheduleId, DateTime? nextRunDate, bool isActive, string pausedReason = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@RecurringScheduleId", recurringScheduleId),
                new SqlParameter("@NextRunDate", (object)nextRunDate ?? DBNull.Value),
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@PausedReason", (object)pausedReason ?? DBNull.Value)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.RecurringScheduleMarkRun, parameters);
        }

        public async Task<List<int>> GetEligibleTestCaseIdsAsync(int assignmentId)
        {
            var parameters = new[] { new SqlParameter("@AssignmentId", assignmentId) };
            var result = await _db.ExecuteReaderAsync(SqlDbConstants.RecurringScheduleGetEligibleTestCaseIds, parameters,
                reader => reader.GetInt32(reader.GetOrdinal("AssignmentTestCaseId")));
            return result.ToList();
        }

        public async Task<IEnumerable<AssignmentOptionModel>> GetAssignmentOptionsAsync()
        {
            return await _db.ExecuteReaderAsync(SqlDbConstants.RecurringScheduleGetAssignmentOptions, [], reader =>
                new AssignmentOptionModel
                {
                    AssignmentId = reader.GetInt32(reader.GetOrdinal("AssignmentId")),
                    AssignmentName = reader.GetNullableString("AssignmentName") ?? string.Empty,
                    Environment = reader.GetNullableString("Environment") ?? string.Empty,
                    EnvironmentId = reader.GetNullableInt("EnvironmentId"),
                    ReleaseId = reader.GetInt32(reader.GetOrdinal("ReleaseId")),
                    ReleaseName = reader.GetNullableString("ReleaseName") ?? string.Empty,
                    ReleaseLifecycle = reader.GetNullableString("ReleaseLifecycle") ?? string.Empty
                });
        }

        // Same aut.AssignmentTestCaseIdList structured-parameter pattern already used by
        // BulkScheduleAsync. An empty list is valid here (NoEligibleTestCases/Paused
        // firings) - the SP treats a zero-row table as "nothing to snapshot".
        public async Task AddRunHistoryAsync(int recurringScheduleId, string result, string detail, List<int> assignmentTestCaseIds)
        {
            var table = new DataTable();
            table.Columns.Add("AssignmentTestCaseId", typeof(int));
            (assignmentTestCaseIds ?? []).ForEach(id => table.Rows.Add(id));

            var parameters = new[]
            {
                new SqlParameter("@RecurringScheduleId", recurringScheduleId),
                new SqlParameter("@Result", result),
                new SqlParameter("@Detail", (object)detail ?? DBNull.Value),
                new SqlParameter("@AssignmentTestCaseIds", table)
                {
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "aut.AssignmentTestCaseIdList"
                }
            };
            await _db.ExecuteScalarAsync<long>(SqlDbConstants.RecurringScheduleAddRunHistory, parameters);
        }

        public async Task ResolvePendingRunHistoryAsync()
        {
            await _db.ExecuteNonQueryAsync(SqlDbConstants.RecurringScheduleResolvePendingRunHistory, []);
        }

        public async Task<IEnumerable<RecurringScheduleRunHistoryModel>> GetRunHistoryAsync(int recurringScheduleId)
        {
            var parameters = new[] { new SqlParameter("@RecurringScheduleId", recurringScheduleId) };
            return await _db.ExecuteReaderAsync(SqlDbConstants.RecurringScheduleGetRunHistory, parameters, reader =>
                new RecurringScheduleRunHistoryModel
                {
                    RunHistoryId = reader.GetInt64(reader.GetOrdinal("RunHistoryId")),
                    RecurringScheduleId = reader.GetInt32(reader.GetOrdinal("RecurringScheduleId")),
                    RunDate = reader.GetDateTime(reader.GetOrdinal("RunDate")),
                    Result = reader.GetNullableString("Result") ?? string.Empty,
                    Detail = reader.GetNullableString("Detail"),
                    TestCasesQueuedCount = reader.GetInt32(reader.GetOrdinal("TestCasesQueuedCount")),
                    ResolutionStatus = reader.GetNullableString("ResolutionStatus") ?? string.Empty,
                    PassedCount = reader.GetNullableInt("PassedCount"),
                    FailedCount = reader.GetNullableInt("FailedCount"),
                    SkippedCount = reader.GetNullableInt("SkippedCount"),
                    ResolvedOn = reader.GetNullableDateTime("ResolvedOn")
                });
        }
    }
}
