using System.Data;
using Microsoft.Data.SqlClient;


namespace AutomationAPI.Repositories.Helpers
{
    public class SqlDataAccessHelper
    {
        private readonly string _connectionString;

        public SqlDataAccessHelper(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
            }

            _connectionString = connectionString;
        }

        public async Task<T> ExecuteScalarAsync<T>(string storedProcedure, SqlParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        var result = await command.ExecuteScalarAsync();

                        if (result == null || result == DBNull.Value)
                        {
                            return default;
                        }

                        return (T)Convert.ChangeType(result, typeof(T));
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"SQL error executing stored procedure '{storedProcedure}'.", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidOperationException($"Failed to cast result to type '{typeof(T).Name}' in '{storedProcedure}'.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error executing stored procedure '{storedProcedure}'.", ex);
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string storedProcedure, SqlParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters to the command if they are provided
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        // Execute the non-query command (Insert, Update, Delete, etc.)
                        return await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"SQL error executing stored procedure '{storedProcedure}'.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error executing stored procedure '{storedProcedure}'.", ex);
            }
        }


        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(string storedProcedure, SqlParameter[] parameters, Func<SqlDataReader, T> map)
        {
            var results = new List<T>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(map(reader));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"SQL error executing stored procedure '{storedProcedure}'.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error executing stored procedure '{storedProcedure}'.", ex);
            }

            return results;
        }

    }
}
