using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories
{
    public class LoginUserRepository : ILoginUserRepository
    {
        private readonly SqlDataAccessHelper _db;

        public LoginUserRepository(SqlDataAccessHelper db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(LoginUserRequestDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@EnvironmentId", request.EnvironmentId),
                new SqlParameter("@PortalUserId", (object?)request.PortalUserId ?? DBNull.Value),
                new SqlParameter("@UserRole", request.UserRole),
                new SqlParameter("@UserName", request.UserName),
                new SqlParameter("@EncryptedPassword", CredentialCipher.Encrypt(request.Password ?? string.Empty)),
                new SqlParameter("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value)
            };

            return await _db.ExecuteScalarAsync<int>(SqlDbConstants.LoginUserCreate, parameters);
        }

        public async Task UpdateAsync(LoginUserRequestDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@LoginUserId", request.LoginUserId!.Value),
                new SqlParameter("@PortalUserId", (object?)request.PortalUserId ?? DBNull.Value),
                new SqlParameter("@UserRole", request.UserRole),
                new SqlParameter("@UserName", request.UserName),
                new SqlParameter("@EncryptedPassword",
                    string.IsNullOrEmpty(request.Password) ? (object)DBNull.Value : CredentialCipher.Encrypt(request.Password)),
                new SqlParameter("@IsActive", request.IsActive ?? true),
                new SqlParameter("@ModifiedBy", (object?)request.ModifiedBy ?? DBNull.Value)
            };

            await _db.ExecuteNonQueryAsync(SqlDbConstants.LoginUserUpdate, parameters);
        }

        public async Task SoftDeleteAsync(int loginUserId, int? modifiedBy = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@LoginUserId", loginUserId),
                new SqlParameter("@ModifiedBy", (object?)modifiedBy ?? DBNull.Value)
            };

            await _db.ExecuteNonQueryAsync(SqlDbConstants.LoginUserSoftDelete, parameters);
        }

        public async Task HardDeleteAsync(int loginUserId)
        {
            var parameters = new[] { new SqlParameter("@LoginUserId", loginUserId) };
            await _db.ExecuteNonQueryAsync(SqlDbConstants.LoginUserHardDelete, parameters);
        }

        public async Task<IEnumerable<LoginUserModel>> GetByEnvironmentAsync(int environmentId)
        {
            var parameters = new[] { new SqlParameter("@EnvironmentId", environmentId) };

            return await _db.ExecuteReaderAsync(SqlDbConstants.LoginUserGetByEnvironment, parameters, reader => new LoginUserModel
            {
                LoginUserId = reader.GetInt32(reader.GetOrdinal("LoginUserId")),
                EnvironmentId = reader.GetInt32(reader.GetOrdinal("EnvironmentId")),
                PortalUserId = reader.GetNullableInt("PortalUserId"),
                PortalUserName = reader.GetNullableString("PortalUserName"),
                UserRole = reader.GetString(reader.GetOrdinal("UserRole")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                ModifiedOn = reader.GetNullableDateTime("ModifiedOn")
            });
        }

        public async Task<LoginUserCredentials?> GetCredentialsAsync(int loginUserId)
        {
            var parameters = new[] { new SqlParameter("@LoginUserId", loginUserId) };

            var results = await _db.ExecuteReaderAsync(SqlDbConstants.LoginUserGetCredentials, parameters, reader =>
            {
                var encrypted = reader.GetString(reader.GetOrdinal("EncryptedPassword"));
                return new LoginUserCredentials
                {
                    LoginUserId = reader.GetInt32(reader.GetOrdinal("LoginUserId")),
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    Password = CredentialCipher.Decrypt(encrypted) ?? string.Empty
                };
            });

            return results.FirstOrDefault();
        }

        public async Task<LoginUserCredentials?> ResolveByRoleAsync(int environmentId, string userRole)
        {
            var parameters = new[]
            {
                new SqlParameter("@EnvironmentId", environmentId),
                new SqlParameter("@UserRole", userRole)
            };

            var results = await _db.ExecuteReaderAsync(SqlDbConstants.LoginUserResolveByRole, parameters, reader =>
            {
                var encrypted = reader.GetString(reader.GetOrdinal("EncryptedPassword"));
                return new LoginUserCredentials
                {
                    LoginUserId = reader.GetInt32(reader.GetOrdinal("LoginUserId")),
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    Password = CredentialCipher.Decrypt(encrypted) ?? string.Empty
                };
            });

            return results.FirstOrDefault();
        }
    }
}
