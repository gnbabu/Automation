using System.Data;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class TestCaseExecutionQueueRepository : ITestCaseExecutionQueueRepository
    {
        private readonly SqlDataAccessHelper _sqlDataAccessHelper;

        public TestCaseExecutionQueueRepository(SqlDataAccessHelper sqlDataAccessHelper)
        {
            _sqlDataAccessHelper = sqlDataAccessHelper;
        }


        public async Task<(int Id, Guid QueueId)> SingleRunNowAsync(int assignmentId, int assignmentTestCaseId)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId),
                new SqlParameter("@AssignmentTestCaseId", assignmentTestCaseId)
            };

            var result = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.SingleRunTestCaseNow, parameters,
                reader => new
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    QueueId = reader.GetGuid(reader.GetOrdinal("QueueId"))
                });

            var row = result.FirstOrDefault();
            if (row == null)
                throw new Exception("RunNow SP returned no data.");

            return (row.Id, row.QueueId);
        }


        public async Task<bool> BulkRunNowAsync(int assignmentId, List<int> assignmentTestCaseIds)
        {
            var table = new DataTable();
            table.Columns.Add("AssignmentTestCaseId", typeof(int));

            assignmentTestCaseIds.ForEach(id => table.Rows.Add(id));

            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId),
                new SqlParameter("@AssignmentTestCaseIds", table)
                {
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "aut.AssignmentTestCaseIdList"
                }
            };

            var result = await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.BulkRunTestCasesNow, parameters);

            return result == 1;
        }

        public async Task<(int Id, Guid QueueId)> SingleScheduleAsync(int assignmentId, int assignmentTestCaseId, DateTime scheduleDate)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId),
                new SqlParameter("@AssignmentTestCaseId", assignmentTestCaseId),
                new SqlParameter("@ScheduleDate", scheduleDate)
            };

            var result = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.ScheduleSingleTestCase, parameters,
                reader => new
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    QueueId = reader.GetGuid(reader.GetOrdinal("QueueId"))
                });

            var row = result.FirstOrDefault();
            if (row == null)
                throw new Exception("ScheduleSingle SP returned no data.");

            return (row.Id, row.QueueId);
        }


        public async Task<bool> BulkScheduleAsync(int assignmentId, List<int> assignmentTestCaseIds, DateTime scheduleDate)
        {
            var table = new DataTable();
            table.Columns.Add("AssignmentTestCaseId", typeof(int));
            assignmentTestCaseIds.ForEach(id => table.Rows.Add(id));

            var parameters = new[]
            {
            new SqlParameter("@AssignmentId", assignmentId),
            new SqlParameter("@AssignmentTestCaseIds", table)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "aut.AssignmentTestCaseIdList"
            },
            new SqlParameter("@ScheduleDate", scheduleDate)
        };

            var result = await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.BulkScheduleTestCases, parameters);

            return result == 1;
        }
    }

}
