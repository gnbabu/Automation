using System.Data;
using Microsoft.Data.SqlClient;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.SQL;
using Microsoft.AspNetCore.Mvc;

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
            var parameters = new SqlParameter[] { };
            return await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetAllUsers, parameters, reader => new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Photo = reader["Photo"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                Active = reader.GetBoolean(reader.GetOrdinal("Active"))
            });
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
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Photo = reader["Photo"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                Active = reader.GetBoolean(reader.GetOrdinal("Active"))
            });

            return users.FirstOrDefault();
        }

        public async Task<User> ValidateUserByEmailAndPasswordAsync(string email, string password)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email),
                new SqlParameter("@Password", password)
            };

            var users = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.ValidateUserByEmailAndPassword, parameters, reader => new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Photo = reader["Photo"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                Active = reader.GetBoolean(reader.GetOrdinal("Active"))
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
                new SqlParameter("@Active", user.Active)
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
                new SqlParameter("@RoleID", user.RoleId)
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
