using System.Data;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class TestCaseAssignmentRepository : ITestCaseAssignmentRepository
    {
        private readonly SqlDataAccessHelper _sqlDataAccessHelper;

        public TestCaseAssignmentRepository(SqlDataAccessHelper sqlDataAccessHelper)
        {
            _sqlDataAccessHelper = sqlDataAccessHelper;
        }

        // Get all assignments by user
        public async Task<IEnumerable<TestCaseAssignment>> GetAssignmentsByUserIdAsync(int userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetTestCaseAssignmentsByUser, parameters, reader => new TestCaseAssignment
            {
                AssignmentId = reader.GetNullableInt("AssignmentId") ?? 0,
                UserId = reader.GetNullableInt("UserId") ?? 0,
                LibraryName = reader.GetNullableString("LibraryName") ?? string.Empty,
                ClassName = reader.GetNullableString("ClassName") ?? string.Empty,
                MethodName = reader.GetNullableString("MethodName") ?? string.Empty,
                AssignedOn = reader.GetNullableDateTime("AssignedOn") ?? DateTime.MinValue,
                AssignedBy = reader.GetNullableInt("AssignedBy")
            }
            );
        }

        // Insert or update single assignment
        public async Task<int> InsertAssignmentAsync(TestCaseAssignment assignment)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = assignment.UserId },
                new SqlParameter("@LibraryName", SqlDbType.NVarChar, 200) { Value = assignment.LibraryName },
                new SqlParameter("@ClassName", SqlDbType.NVarChar, 200) { Value = assignment.ClassName },
                new SqlParameter("@MethodName", SqlDbType.NVarChar, 200) { Value = assignment.MethodName },
                new SqlParameter("@AssignedBy", SqlDbType.Int) { Value = (object?)assignment.AssignedBy ?? DBNull.Value }
            };

            return await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.InsertTestCaseAssignment, parameters.ToArray());
        }

        // Bulk insert or sync assignments
        public async Task BulkInsertAssignmentsAsync(IEnumerable<TestCaseAssignment> assignments)
        {
            var table = new DataTable();
            table.Columns.Add("UserId", typeof(int));
            table.Columns.Add("LibraryName", typeof(string));
            table.Columns.Add("ClassName", typeof(string));
            table.Columns.Add("MethodName", typeof(string));
            table.Columns.Add("AssignedBy", typeof(int));

            foreach (var a in assignments)
            {
                table.Rows.Add(
                    a.UserId,
                    a.LibraryName,
                    a.ClassName,
                    a.MethodName,
                    a.AssignedBy ?? (object)DBNull.Value
                );
            }

            var param = new SqlParameter("@Assignments", SqlDbType.Structured)
            {
                TypeName = SqlDbConstants.TestCaseAssignmentType,
                Value = table
            };

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.BulkSyncTestCaseAssignments, new[] { param });
        }

        public async Task DeleteAssignmentsByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid UserId", nameof(userId));

            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.DeleteTestCaseAssignmentsByUser, parameters);
        }

    }
}
