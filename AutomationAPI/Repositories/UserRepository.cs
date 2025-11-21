using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.PortableExecutable;

namespace AutomationAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SqlDataAccessHelper _sqlDataAccessHelper;

        public UserRepository(SqlDataAccessHelper sqlDataAccessHelper)
        {
            _sqlDataAccessHelper = sqlDataAccessHelper;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                var parameters = new SqlParameter[] { };

                return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAllUsers, parameters, reader => new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Photo = reader["Photo"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                    RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                    Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                    Priority = reader.IsDBNull(reader.GetOrdinal("Priority"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("Priority")),
                    PriorityName = reader.IsDBNull(reader.GetOrdinal("PriorityName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("PriorityName")),
                    LastLogin = reader.IsDBNull(reader.GetOrdinal("LastLogin"))
                        ? (DateTime?)null
                        : reader.GetDateTime(reader.GetOrdinal("LastLogin")),
                    TimeZone = reader.IsDBNull(reader.GetOrdinal("TimeZone"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("TimeZone")),
                    TimeZoneName = reader.IsDBNull(reader.GetOrdinal("TimeZoneName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("TimeZoneName")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("Status")),
                    StatusName = reader.IsDBNull(reader.GetOrdinal("StatusName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("StatusName")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    TwoFactor = reader.IsDBNull(reader.GetOrdinal("TwoFactor"))
                        ? false
                        : reader.GetBoolean(reader.GetOrdinal("TwoFactor")),
                    Teams = reader.IsDBNull(reader.GetOrdinal("Teams"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("Teams")),

                });
            }
            catch (Exception ex)
            {
                // Log the exception (you can use any logging framework you prefer)
                Console.WriteLine($"Error in GetAllUsersAsync: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }

        }
        public string GetPhotoBase64(byte[] photoBytes)
        {
            return $"data:image/png;base64,{Convert.ToBase64String(photoBytes)}";
        }


        public async Task<User> GetUserByIdAsync(int userId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            var users = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetUserById, parameters, reader => new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.IsDBNull(reader.GetOrdinal("LastName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Photo = reader["Photo"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                TimeZone = reader.IsDBNull(reader.GetOrdinal("TimeZone"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("TimeZone")),
                TwoFactor = reader.IsDBNull(reader.GetOrdinal("TwoFactor"))
                        ? false
                        : reader.GetBoolean(reader.GetOrdinal("TwoFactor")),
                Teams = reader.IsDBNull(reader.GetOrdinal("Teams"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("Teams")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("PhoneNumber"))
            });

            return users.FirstOrDefault();
        }

        public async Task<User> ValidateUserByUsernameAndPasswordAsync(string username, string password)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password)
            };

            var users = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.ValidateUserByUsernameAndPassword, parameters, reader => new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Photo = reader["Photo"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                Active = reader.GetBoolean(reader.GetOrdinal("Active")),
            });

            return users.FirstOrDefault();
        }

        public async Task<int> CreateUserAsync(User user)
        {

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@UserName", user.UserName),
                new SqlParameter("@Password", user.Password),
                new SqlParameter("@FirstName", user.FirstName),
                new SqlParameter("@LastName", user.LastName),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@RoleID", user.RoleId),
                new SqlParameter("@Active", user.Active),
                new SqlParameter("@TwoFactor", user.TwoFactor),
                new SqlParameter("@Teams",user.Teams),
                new SqlParameter("@TimeZone",user.TimeZone),
                new SqlParameter("@PhoneNumber", user.PhoneNumber)

            };


            if (!string.IsNullOrEmpty(user.Photo))
            {
                var base64 = user.Photo.Contains(",")
                    ? user.Photo.Substring(user.Photo.IndexOf(",") + 1)
                    : user.Photo;

                byte[] imageBytes = Convert.FromBase64String(base64);

                parameters.Add(new SqlParameter
                {
                    ParameterName = "@Photo",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = imageBytes ?? (object)DBNull.Value
                });
            }
            else
            {
                parameters.Add(new SqlParameter
                {
                    ParameterName = "@Photo",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = (object)DBNull.Value
                });
            }

            return await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.CreateUser, parameters.ToArray());
        }

        public async Task UpdateUserAsync(User user)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@UserID", user.UserId),
                new SqlParameter("@UserName", user.UserName),
                new SqlParameter("@FirstName", user.FirstName),
                new SqlParameter("@LastName", user.LastName),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@RoleID", user.RoleId),
                new SqlParameter("@TwoFactor", user.TwoFactor),
                new SqlParameter("@Teams",user.Teams),
                new SqlParameter("@TimeZone",user.TimeZone),
                new SqlParameter("@PhoneNumber", user.PhoneNumber)
            };

            byte[] imageBytes = [];
            if (!string.IsNullOrEmpty(user.Photo))
            {
                var base64 = user.Photo.Contains(",")
                    ? user.Photo.Substring(user.Photo.IndexOf(",") + 1)
                    : user.Photo;

                imageBytes = Convert.FromBase64String(base64);
            }
            parameters.Add(new SqlParameter
            {
                ParameterName = "@Photo",
                SqlDbType = SqlDbType.VarBinary,
                Value = (imageBytes != null && imageBytes.Length > 0)
                        ? imageBytes
                        : (object)DBNull.Value

            });

            await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.UpdateUser, parameters.ToArray());
        }

        public async Task DeleteUserAsync(int userId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId)
            };

            await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.DeleteUser, parameters);
        }

        public async Task<IEnumerable<User>> GetFilteredUsersAsync(UserFilter filters)
        {
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Search", filters.Search ?? string.Empty),
                    new SqlParameter("@Status", Convert.ToInt32(filters.Status)),
                    new SqlParameter("@Role", Convert.ToInt32(filters.Role)),
                    new SqlParameter("@Priority", Convert.ToInt32(filters.Priority)),
                };

                return await _sqlDataAccessHelper.ExecuteReaderAsync(
                    SqlDbConstants.FilterAllUsers,
                    parameters.ToArray(),
                    reader => new User
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? string.Empty : reader.GetString(reader.GetOrdinal("FirstName")),
                        LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? string.Empty : reader.GetString(reader.GetOrdinal("LastName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Photo = reader["Photo"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                        RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                        RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                        Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                        Priority = reader.IsDBNull(reader.GetOrdinal("Priority")) ? null : reader.GetInt32(reader.GetOrdinal("Priority")),
                        PriorityName = reader.IsDBNull(reader.GetOrdinal("PriorityName")) ? string.Empty : reader.GetString(reader.GetOrdinal("PriorityName")),
                        LastLogin = reader.IsDBNull(reader.GetOrdinal("LastLogin")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLogin")),
                        TimeZone = reader.IsDBNull(reader.GetOrdinal("TimeZone")) ? null : reader.GetInt32(reader.GetOrdinal("TimeZone")),
                        TimeZoneName = reader.IsDBNull(reader.GetOrdinal("TimeZoneName")) ? string.Empty : reader.GetString(reader.GetOrdinal("TimeZoneName")),
                        Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetInt32(reader.GetOrdinal("Status")),
                        StatusName = reader.IsDBNull(reader.GetOrdinal("StatusName")) ? string.Empty : reader.GetString(reader.GetOrdinal("StatusName")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? string.Empty : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        TwoFactor = reader.IsDBNull(reader.GetOrdinal("TwoFactor")) ? false : reader.GetBoolean(reader.GetOrdinal("TwoFactor")),
                        Teams = reader.IsDBNull(reader.GetOrdinal("Teams")) ? string.Empty : reader.GetString(reader.GetOrdinal("Teams")),
                    }
                );
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An unexpected error occurred while retrieving users.", ex);
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", request.UserId),
                new SqlParameter("@OldPassword", request.OldPassword),
                new SqlParameter("@NewPassword", request.NewPassword)
            };


            var result = await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.ChangePassword, parameters);

            // Check the result to determine if the password change was successful
            if (result == 0)
            {
                // If StatusCode is 0, it means the old password was incorrect or user not found
                throw new InvalidOperationException("Error: Incorrect current password or user not found.");
            }
        }


        public async Task SetUserActiveStatusAsync(int userId, bool active)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@Active", active)
            };

            await _sqlDataAccessHelper.ExecuteScalarAsync<int>(SqlDbConstants.SetUserActiveStatus, parameters);
        }

        public async Task<IEnumerable<UserRole>> GetUserRoles()
        {
            var parameters = new SqlParameter[] { };
            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetUserRoles, parameters, reader => new UserRole
            {
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                RoleName = reader.GetString(reader.GetOrdinal("RoleName"))

            });
        }
    }

}
