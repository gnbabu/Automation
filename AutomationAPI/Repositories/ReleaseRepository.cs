using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class ReleaseRepository : IReleaseRepository
    {
        private readonly SqlDataAccessHelper _db;

        public ReleaseRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(ReleaseRequestDto releaseRequest)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseName", releaseRequest.ReleaseName),
                new SqlParameter("@Description", releaseRequest.Description ?? (object)DBNull.Value),
                new SqlParameter("@CreatedBy", releaseRequest.CreatedBy)
            };

            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.CreateRelease, parameters);
        }
        public async Task UpdateAsync(ReleaseRequestDto releaseRequest)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseId", releaseRequest.ReleaseId),
                new SqlParameter("@ReleaseName", releaseRequest.ReleaseName),
                new SqlParameter("@Description", releaseRequest.Description ?? (object)DBNull.Value),
                new SqlParameter("@ReleaseLifecycle", releaseRequest.ReleaseLifecycle),
                new SqlParameter("@IsActive", releaseRequest.IsActive)
            };

            await _db.ExecuteNonQueryAsync(SqlDbConstants.UpdateRelease, parameters);
        }

        public async Task<IEnumerable<ReleaseModel>> GetAllAsync()
        {
            return await _db.ExecuteReaderAsync(SqlDbConstants.GetAllReleases, [], MapRelease);
        }

        public async Task<ReleaseModel> GetByIdAsync(int releaseId)
        {
            var parameters = new[] { new SqlParameter("@ReleaseId", releaseId) };

            var result = await _db.ExecuteReaderAsync(SqlDbConstants.GetReleaseById, parameters, MapRelease);

            return result.FirstOrDefault();
        }


        public async Task SignOffAsync(int releaseId, string signedOffBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReleaseId", releaseId),
                new SqlParameter("@SignedOffBy", signedOffBy)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.SignOffRelease, parameters);
        }

        private static ReleaseModel MapRelease(SqlDataReader reader)
        {
            return new ReleaseModel
            {
                ReleaseId = reader.GetInt32(reader.GetOrdinal("ReleaseId")),
                ReleaseName = reader.GetString(reader.GetOrdinal("ReleaseName")),
                Description = reader.GetNullableString("Description") ?? string.Empty,
                ReleaseLifecycle = reader.GetNullableString("ReleaseLifecycle") ?? string.Empty,
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                SignOffStatus = reader.GetNullableString("SignOffStatus") ?? string.Empty,
                SignedOffBy = reader.GetNullableString("SignedOffBy") ?? string.Empty,
                SignedOffOn = reader.IsDBNull(reader.GetOrdinal("SignedOffOn"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("SignedOffOn")),
                CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                CreatedBy = reader.GetNullableString("CreatedBy") ?? string.Empty
            };
        }

    }
}
