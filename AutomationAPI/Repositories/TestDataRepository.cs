using System.Text.Json;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class TestDataRepository : ITestDataRepository
    {
        private readonly SqlDataAccessHelper _db;

        public TestDataRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }

        public async Task<IEnumerable<TestDataTestCase>> GetMyTestCasesAsync(int userId, int environmentId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@EnvironmentId", environmentId)
            };

            return await _db.ExecuteReaderAsync(SqlDbConstants.TestDataGetMyTestCases, parameters, reader => new TestDataTestCase
            {
                AssignmentTestCaseId = reader.GetNullableInt("AssignmentTestCaseId") ?? 0,
                LibraryName = reader.GetNullableString("LibraryName") ?? "",
                ClassName = reader.GetNullableString("ClassName") ?? "",
                MethodName = reader.GetNullableString("MethodName") ?? "",
                TestCaseId = reader.GetNullableString("TestCaseId"),
                TestCaseDescription = reader.GetNullableString("TestCaseDescription"),
                ReleaseId = reader.GetNullableInt("ReleaseId")
            });
        }

        public async Task<TestDataAssignment?> ResolveAssignmentAsync(int assignmentTestCaseId)
        {
            var parameters = new[] { new SqlParameter("@AssignmentTestCaseId", assignmentTestCaseId) };

            var rows = await _db.ExecuteReaderAsync(SqlDbConstants.TestDataResolveAssignment, parameters, reader => new TestDataAssignment
            {
                AssignmentTestCaseId = reader.GetNullableInt("AssignmentTestCaseId") ?? 0,
                LibraryName = reader.GetNullableString("LibraryName") ?? "",
                ClassName = reader.GetNullableString("ClassName") ?? "",
                MethodName = reader.GetNullableString("MethodName") ?? "",
                TestCaseId = reader.GetNullableString("TestCaseId"),
                AssignedUser = reader.GetNullableInt("AssignedUser") ?? 0,
                EnvironmentId = reader.GetNullableInt("EnvironmentId"),
                ReleaseId = reader.GetNullableInt("ReleaseId"),
                ReleaseFolderPath = reader.GetNullableString("ReleaseFolderPath")
            });

            return rows.FirstOrDefault();
        }

        public async Task<IEnumerable<AutomationDataSection>> UpsertModelSectionsAsync(string flowName, IEnumerable<string> sectionNames)
        {
            var parameters = new[]
            {
                new SqlParameter("@FlowName", flowName),
                new SqlParameter("@SectionNames", JsonSerializer.Serialize(sectionNames))
            };

            return await _db.ExecuteReaderAsync(SqlDbConstants.UpsertModelSections, parameters, reader => new AutomationDataSection
            {
                SectionId = reader.GetNullableInt("SectionID"),
                SectionName = reader.GetNullableString("SectionName"),
                FlowName = reader.GetNullableString("FlowName"),
                IsFromModel = reader.GetBoolean(reader.GetOrdinal("IsFromModel"))
            });
        }

        public async Task<TestDataDocumentRecord?> GetDocumentAsync(string libraryName, string className, string methodName, int userId, int environmentId)
        {
            var parameters = KeyParameters(libraryName, className, methodName).Concat(new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@EnvironmentId", environmentId)
            }).ToArray();

            var rows = await _db.ExecuteReaderAsync(SqlDbConstants.TestCaseDataGet, parameters, reader => new TestDataDocumentRecord
            {
                TestCaseDataId = reader.GetNullableInt("TestCaseDataId") ?? 0,
                FlowName = reader.GetNullableString("FlowName") ?? "",
                DataJson = reader.GetNullableString("DataJson") ?? "{}",
                Version = reader.GetInt64(reader.GetOrdinal("Version")),
                ModifiedOn = reader.GetNullableDateTime("ModifiedOn")
            });

            return rows.FirstOrDefault();
        }

        public async Task<TestDataSaveResult> SaveDocumentAsync(string libraryName, string className, string methodName,
            int userId, int environmentId, string flowName, string dataJson, long? expectedVersion)
        {
            var parameters = KeyParameters(libraryName, className, methodName).Concat(new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@EnvironmentId", environmentId),
                new SqlParameter("@FlowName", flowName),
                new SqlParameter("@DataJson", dataJson),
                new SqlParameter("@ExpectedVersion", (object?)expectedVersion ?? DBNull.Value)
            }).ToArray();

            var rows = await _db.ExecuteReaderAsync(SqlDbConstants.TestCaseDataSave, parameters, reader => new TestDataSaveResult
            {
                TestCaseDataId = reader.GetNullableInt("TestCaseDataId"),
                IsNew = reader.GetBoolean(reader.GetOrdinal("IsNew")),
                IsConflict = reader.GetBoolean(reader.GetOrdinal("IsConflict")),
                Version = reader.IsDBNull(reader.GetOrdinal("Version")) ? null : reader.GetInt64(reader.GetOrdinal("Version"))
            });

            return rows.First();
        }

        public async Task<IEnumerable<TestDataCopySource>> GetCopySourcesAsync(string libraryName, string className, string methodName,
            string flowName, int userId, int environmentId)
        {
            var parameters = KeyParameters(libraryName, className, methodName).Concat(new[]
            {
                new SqlParameter("@FlowName", flowName),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@EnvironmentId", environmentId)
            }).ToArray();

            return await _db.ExecuteReaderAsync(SqlDbConstants.TestCaseDataGetCopySources, parameters, reader => new TestDataCopySource
            {
                LibraryName = reader.GetNullableString("LibraryName") ?? "",
                ClassName = reader.GetNullableString("ClassName") ?? "",
                MethodName = reader.GetNullableString("MethodName") ?? "",
                TestCaseId = reader.GetNullableString("TestCaseId")
            });
        }

        private static SqlParameter[] KeyParameters(string libraryName, string className, string methodName) => new[]
        {
            new SqlParameter("@LibraryName", libraryName),
            new SqlParameter("@ClassName", className),
            new SqlParameter("@MethodName", methodName)
        };
    }
}
