using System.Data;
using Microsoft.Data.SqlClient;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutomationAPI.Repositories
{
    public class AutomationRepository : IAutomationRepository
    {
        private readonly SqlDataAccessHelper _sqlDataAccessHelper;

        public AutomationRepository(SqlDataAccessHelper sqlDataAccessHelper)
        {
            _sqlDataAccessHelper = sqlDataAccessHelper;
        }

        public async Task<IEnumerable<AutomationFlow>> GetAutomationFlowNamesAsync()
        {
            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAutomationFlowNames, null, reader => new AutomationFlow
            {
                FlowName = reader.GetNullableString("FlowName")
            });
        }

        public async Task<IEnumerable<AutomationDataSection>> GetAutomationDataSectionsAsync(string? flowName)
        {
            var parameters = new[]
            {
                new SqlParameter("@FlowName", SqlDbType.NVarChar, 200)
                {
                    Value = string.IsNullOrEmpty(flowName) ? DBNull.Value : flowName
                }
            };

            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAutomationDataSection, parameters, reader => new AutomationDataSection
            {
                SectionId = reader.GetNullableInt("SectionID") ?? 0,
                SectionName = reader.GetNullableString("SectionName"),
                FlowName = reader.GetNullableString("FlowName")
            });

        }

        public async Task<AutomationData> GetAutomationDataAsync(int sectionId, int userId, int environmentId)
        {
            AutomationData automationData = new AutomationData();
            var parameters = new[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@EnvironmentId", environmentId)
            };

            var data = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAutomationData, parameters, reader => new AutomationData
            {
                Id = reader.GetNullableInt("Id") ?? 0,
                SectionId = reader.GetNullableInt("SectionID") ?? 0,
                TestContent = reader.GetNullableString("TestContent"),
                UserId = reader.GetNullableInt("UserID") ?? 0,
                EnvironmentId = reader.GetNullableInt("EnvironmentId"),

            });


            if (data?.Any() == true)
            {
                var first = data.First();

                if (!string.IsNullOrWhiteSpace(first.TestContent))
                {
                    first.TestContent = AutomationDataHelper.BuildFieldSummary(first.TestContent);
                }

                automationData = first;
            }

            return automationData;
        }

        public async Task<int> InsertAutomationDataAsync(AutomationDataRequest automationDataRequest)
        {

            if (!string.IsNullOrEmpty(automationDataRequest.TestContent))
            {
                automationDataRequest.TestContent = AutomationDataHelper.ConvertToJson(automationDataRequest.TestContent);
            }
            var parameters = new[]
            {
                new SqlParameter("@SectionID", automationDataRequest.SectionId),
                new SqlParameter("@TestContent", automationDataRequest.TestContent),
                new SqlParameter("@UserID",automationDataRequest.UserId),
                new SqlParameter("@EnvironmentId", automationDataRequest.EnvironmentId)
            };

            return await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.InsertAutomationData, parameters);
        }

        public async Task UpdateAutomationDataAsync(AutomationDataRequest automationDataRequest)
        {
            if (!string.IsNullOrEmpty(automationDataRequest.TestContent))
            {
                automationDataRequest.TestContent = AutomationDataHelper.ConvertToJson(automationDataRequest.TestContent);
            }

            var parameters = new[]
            {
                new SqlParameter("@ID", automationDataRequest.Id),
                new SqlParameter("@TestContent", automationDataRequest.TestContent?? (object)DBNull.Value)
            };
            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.UpdateAutomationData, parameters);
        }


        public async Task<IEnumerable<AutomationData>> GetAutomationDataByFlowNameAsync(string flowName)
        {
            var parameters = new[]
            {
                new SqlParameter("@FlowName", flowName ?? (object)DBNull.Value)
            };

            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAutomationDataByFlowName, parameters, reader =>
            {
                var testContent = reader.GetNullableString("TestContent");
                return new AutomationData
                {
                    Id = reader.GetNullableInt("Id") ?? 0,
                    SectionId = reader.GetNullableInt("SectionID") ?? 0,
                    SectionName = reader.GetNullableString("SectionName"),
                    TestContent = testContent,
                    // See AutomationData.AutomationContents - this is what every TC.*
                    // Selenium test project's own DataRepository actually reads
                    // (Mapper.BindData<T>(data.automationContents)); never populated
                    // before, always null, causing a real ArgumentNullException the
                    // first time this endpoint was actually exercised by a live queued
                    // test run end-to-end.
                    AutomationContents = AutomationDataHelper.ParseAutomationContents(testContent),
                };
            });
        }



        public async Task DeleteAutomationDataAsync(int sectionId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@SectionID", sectionId)
            };

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.DeleteAutomationData, parameters);
        }


        // Duplicate-name check lives here in C# (reusing the already-existing
        // GetAutomationDataSectionsAsync) rather than in SQL - usp_InsertAutomationDataSection/
        // usp_UpdateAutomationDataSections themselves have no uniqueness constraint at
        // all, so this was previously entirely unchecked.
        private async Task EnsureSectionNameIsUniqueAsync(string flowName, string sectionName, int? excludeSectionId = null)
        {
            var existing = await GetAutomationDataSectionsAsync(flowName);
            var duplicate = existing.Any(s =>
                s.SectionId != excludeSectionId &&
                string.Equals(s.SectionName?.Trim(), sectionName?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                throw new DuplicateSectionException(
                    $"A section named \"{sectionName}\" already exists in flow \"{flowName}\".");
            }
        }

        public async Task<int> InsertAutomationDataSectionAsync(AutomationDataSectionRequest request)
        {
            await EnsureSectionNameIsUniqueAsync(request.FlowName, request.SectionName);

            var parameters = new[]
            {
                new SqlParameter("@SectionName", request.SectionName),
                new SqlParameter("@FlowName", request.FlowName)
            };

            return await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.InsertAutomationDataSection, parameters);
        }

        public async Task UpdateAutomationDataSectionAsync(AutomationDataSectionRequest request)
        {
            await EnsureSectionNameIsUniqueAsync(request.FlowName, request.SectionName, request.SectionId);

            var parameters = new[]
            {
                new SqlParameter("@SectionID", request.SectionId),
                new SqlParameter("@SectionName", request.SectionName ?? (object)DBNull.Value),
                new SqlParameter("@FlowName", request.FlowName ?? (object)DBNull.Value)
            };

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.UpdateAutomationDataSections, parameters);
        }

        public async Task DeleteAutomationDataSectionAsync(int sectionId, bool cascade = false)
        {
            // Default path blocks deletion if the section already has saved test data
            // - there is no FK constraint protecting this relationship at all, so an
            // unguarded delete would silently orphan that data forever. cascade=true
            // (only sent after the user explicitly confirms "delete the section AND its
            // data" in the UI) runs both deletes atomically in one proc instead.
            var countParameters = new[] { new SqlParameter("@SectionID", sectionId) };
            var dataCount = await _sqlDataAccessHelper.ExecuteScalarAsync<int>(
                SqlDbConstants.CountAutomationDataForSection, countParameters);

            if (dataCount > 0 && !cascade)
            {
                throw new SectionHasDataException(
                    $"This section has {dataCount} saved test data entr{(dataCount == 1 ? "y" : "ies")} - remove them first before deleting the section.",
                    dataCount);
            }

            var parameters = new SqlParameter[]
            {
            new SqlParameter("@SectionID", sectionId)
            };

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(
                cascade ? SqlDbConstants.DeleteAutomationDataSectionCascade
                        : SqlDbConstants.DeleteAutomationDataSection,
                parameters);
        }



    }
}
