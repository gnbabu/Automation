﻿using System.Data;
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


        // Get all assignments 
        public async Task<IEnumerable<TestCaseAssignment>> GetAllAssignmentsAsync()
        {
            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAllTestCaseAssignments, [], reader => new TestCaseAssignment
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

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.BulkSyncTestCaseAssignments_New, new[] { param });
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

        public async Task DeleteAssignmentsAsync(TestCaseAssignmentDeleteRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var parameters = new List<SqlParameter>();

            if (request.UserIds != null && request.UserIds.Any())
            {
                // Convert list of UserIds to CSV string
                string userIdsCsv = string.Join(",", request.UserIds);
                parameters.Add(new SqlParameter("@UserIds", SqlDbType.NVarChar, -1) { Value = userIdsCsv });
            }
            else
            {
                parameters.Add(new SqlParameter("@UserIds", SqlDbType.NVarChar, -1) { Value = DBNull.Value });
            }

            parameters.Add(new SqlParameter("@LibraryName", SqlDbType.NVarChar)
            { Value = (object?)request.LibraryName ?? DBNull.Value });
            parameters.Add(new SqlParameter("@ClassName", SqlDbType.NVarChar)
            { Value = (object?)request.ClassName ?? DBNull.Value });
            parameters.Add(new SqlParameter("@MethodName", SqlDbType.NVarChar)
            { Value = (object?)request.MethodName ?? DBNull.Value });

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.DeleteTestCaseAssignments, parameters.ToArray());
        }


    }
}
