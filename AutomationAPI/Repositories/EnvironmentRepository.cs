using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class EnvironmentRepository : IEnvironmentRepository
    {
        private readonly SqlDataAccessHelper _db;

        public EnvironmentRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(EnvironmentRequestDto request)
        {
            // Authentication Required and Enable SSO are mutually exclusive - a manual
            // username/password login and "no login screen at all" can't both be true for
            // the same environment. Normalized here too as defense in depth (the Portal's
            // own UI + the stored procedure already enforce this), in case a caller
            // bypasses both. SSO wins if somehow both were sent true.
            var enableSso = request.EnableSso ?? false;
            var requiresAuthentication = !enableSso && (request.RequiresAuthentication ?? true);

            var parameters = new[]
            {
                new SqlParameter("@EnvironmentName", request.EnvironmentName),
                new SqlParameter("@Description", request.Description ?? (object)DBNull.Value),
                new SqlParameter("@CreatedBy", request.CreatedBy),
                new SqlParameter("@EnvironmentUrl", (object?)request.EnvironmentUrl ?? DBNull.Value),
                new SqlParameter("@RequiresAuthentication", requiresAuthentication),
                new SqlParameter("@EnableSso", enableSso)
            };

            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.EnvironmentCreate, parameters);
        }

        public async Task UpdateAsync(EnvironmentRequestDto request)
        {
            var enableSso = request.EnableSso ?? false;
            var requiresAuthentication = !enableSso && (request.RequiresAuthentication ?? true);

            var parameters = new[]
            {
                new SqlParameter("@EnvironmentId", request.EnvironmentId!.Value),
                new SqlParameter("@EnvironmentName", request.EnvironmentName),
                new SqlParameter("@Description", request.Description ?? (object)DBNull.Value),
                new SqlParameter("@IsActive", request.IsActive ?? true),
                new SqlParameter("@ModifiedBy", (object?)request.ModifiedBy ?? DBNull.Value),
                new SqlParameter("@EnvironmentUrl", (object?)request.EnvironmentUrl ?? DBNull.Value),
                new SqlParameter("@RequiresAuthentication", requiresAuthentication),
                new SqlParameter("@EnableSso", enableSso)
            };

            await _db.ExecuteNonQueryAsync(SqlDbConstants.EnvironmentUpdate, parameters);
        }

        public async Task<IEnumerable<EnvironmentModel>> GetAllAsync()
        {
            return await _db.ExecuteReaderAsync(SqlDbConstants.EnvironmentGetAll, [], MapEnvironment);
        }

        public async Task<EnvironmentModel> GetByIdAsync(int environmentId)
        {
            var parameters = new[] { new SqlParameter("@EnvironmentId", environmentId) };

            var result = await _db.ExecuteReaderAsync(SqlDbConstants.EnvironmentGetById, parameters, MapEnvironment);

            return result.FirstOrDefault();
        }

        public async Task SoftDeleteAsync(int environmentId, int? modifiedBy = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@EnvironmentId", environmentId),
                new SqlParameter("@ModifiedBy", (object?)modifiedBy ?? DBNull.Value)
            };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.EnvironmentSoftDelete, parameters);
        }

        public async Task HardDeleteAsync(int environmentId)
        {
            var parameters = new[] { new SqlParameter("@EnvironmentId", environmentId) };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.EnvironmentHardDelete, parameters);
        }

        private static EnvironmentModel MapEnvironment(SqlDataReader reader)
        {
            return new EnvironmentModel
            {
                EnvironmentId = reader.GetInt32(reader.GetOrdinal("EnvironmentId")),
                EnvironmentName = reader.GetString(reader.GetOrdinal("EnvironmentName")),
                Description = reader.GetNullableString("Description") ?? string.Empty,
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                EnvironmentUrl = reader.GetNullableString("EnvironmentUrl"),
                RequiresAuthentication = reader.GetBoolean(reader.GetOrdinal("RequiresAuthentication")),
                EnableSso = reader.GetBoolean(reader.GetOrdinal("EnableSso")),
                CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                ModifiedByName = reader.GetNullableString("ModifiedByName"),
                ReleaseCount = reader.GetInt32(reader.GetOrdinal("ReleaseCount"))
            };
        }
    }

}
