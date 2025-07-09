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

        public async Task<AutomationData> GetAutomationDataAsync(int sectionId, int userId)
        {
            AutomationData automationData = new AutomationData();
            var parameters = new[]
            {
                new SqlParameter("@SectionID", sectionId),
                new SqlParameter("@UserId", userId)
            };

            var data = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAutomationData, parameters, reader => new AutomationData
            {
                Id = reader.GetNullableInt("Id") ?? 0,
                SectionId = reader.GetNullableInt("SectionID") ?? 0,
                TestContent = reader.GetNullableString("TestContent"),
                UserId = reader.GetNullableInt("UserID") ?? 0,

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
                new SqlParameter("@UserID",automationDataRequest.UserId)
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

            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAutomationDataByFlowName, parameters, reader => new AutomationData
            {
                Id = reader.GetNullableInt("Id") ?? 0,
                SectionId = reader.GetNullableInt("SectionID") ?? 0,
                SectionName = reader.GetNullableString("SectionName"),
                TestContent = reader.GetNullableString("TestContent")
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


        public async Task<int> InsertAutomationDataSectionAsync(AutomationDataSectionRequest request)
        {
            var parameters = new[]
            {
                new SqlParameter("@SectionName", request.SectionName),
                new SqlParameter("@FlowName", request.FlowName)
            };

            return await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.InsertAutomationDataSection, parameters);
        }

        public async Task UpdateAutomationDataSectionAsync(AutomationDataSectionRequest request)
        {
            var parameters = new[]
            {
                new SqlParameter("@SectionID", request.SectionId),
                new SqlParameter("@SectionName", request.SectionName ?? (object)DBNull.Value),
                new SqlParameter("@FlowName", request.FlowName ?? (object)DBNull.Value)
            };

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.UpdateAutomationDataSections, parameters);
        }

        public async Task DeleteAutomationDataSectionAsync(int sectionId)
        {
            var parameters = new SqlParameter[]
            {
            new SqlParameter("@SectionID", sectionId)
            };

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.DeleteAutomationDataSection, parameters);
        }



    }
}
