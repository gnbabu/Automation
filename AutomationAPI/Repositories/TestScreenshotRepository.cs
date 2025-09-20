using AutomationAPI.Repositories.Helpers;
using System.Data;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.Data.SqlClient;
using AutomationAPI.Repositories.SQL;

namespace AutomationAPI.Repositories
{
    public class TestScreenshotRepository : ITestScreenshotRepository
    {
        private readonly SqlDataAccessHelper _sqlDataAccessHelper;

        public TestScreenshotRepository(SqlDataAccessHelper sqlDataAccessHelper)
        {
            _sqlDataAccessHelper = sqlDataAccessHelper;
        }


        public async Task<int> InsertScreenshotAsync(TestScreenshot screenshot)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@TestResultId", SqlDbType.Int) { Value = screenshot.TestResultId },
                new SqlParameter("@QueueId", SqlDbType.Int) { Value = screenshot.QueueId },
                new SqlParameter("@Caption", SqlDbType.NVarChar, -1) { Value = (object?)screenshot.Caption ?? DBNull.Value },
                new SqlParameter("@TakenAt", SqlDbType.DateTime) { Value = (object?)screenshot.TakenAt ?? DBNull.Value },
            };

            if (!string.IsNullOrEmpty(screenshot.Screenshot))
            {
                byte[] imageBytes = ConvertBase64ToBytes(screenshot.Screenshot);

                parameters.Add(new SqlParameter
                {
                    ParameterName = "@Screenshot",
                    SqlDbType = SqlDbType.VarBinary,
                    Value = imageBytes ?? (object)DBNull.Value
                });
            }

            return await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.InsertTestScreenshot, parameters.ToArray());
        }

        public async Task BulkInsertScreenshotsAsync(IEnumerable<TestScreenshot> screenshots)
        {
            var table = new DataTable();
            table.Columns.Add("TestResultId", typeof(int));
            table.Columns.Add("QueueId", typeof(int));
            table.Columns.Add("Caption", typeof(string));
            table.Columns.Add("Screenshot", typeof(byte[]));
            table.Columns.Add("TakenAt", typeof(DateTime));

            foreach (var s in screenshots)
            {
                table.Rows.Add(
                    s.TestResultId,
                    s.QueueId,
                    s.Caption ?? (object)DBNull.Value,
                    ConvertBase64ToBytes(s.Screenshot) ?? (object)DBNull.Value,
                    s.TakenAt == default ? (object)DBNull.Value : s.TakenAt
                );
            }

            var param = new SqlParameter("@Screenshots", SqlDbType.Structured)
            {
                TypeName = "[aut].[TestScreenshotType]",
                Value = table
            };

            await _sqlDataAccessHelper.ExecuteNonQueryAsync(SqlDbConstants.BulkInsertTestScreenshots, new[] { param });
        }

        public async Task<IEnumerable<TestScreenshot>> GetScreenshotsByTestResultIdAsync(int testResultId)
        {
            var parameters = new[]
            {
                    new SqlParameter("@TestResultId", SqlDbType.Int) { Value = testResultId }
            };

            var screenshots = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetScreenshotsByTestResultId, parameters, reader => new TestScreenshot
            {
                ID = reader.GetNullableInt("ID") ?? 0,
                TestResultId = reader.GetNullableInt("TestResultId") ?? 0,
                QueueId = reader.GetNullableInt("QueueId") ?? 0,
                Caption = reader.GetNullableString("Caption"),
                Screenshot = reader["Screenshot"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                TakenAt = reader.GetNullableDateTime("TakenAt") ?? DateTime.MinValue
            });

            return screenshots;
        }

        public async Task<TestScreenshot?> GetScreenshotByIdAsync(int id)
        {
            var parameters = new[]
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id }
            };

            var screenshots = await _sqlDataAccessHelper.ExecuteReaderAsync(SqlDbConstants.GetScreenshotById, parameters, reader => new TestScreenshot
            {
                ID = reader.GetNullableInt("ID") ?? 0,
                TestResultId = reader.GetNullableInt("TestResultId") ?? 0,
                QueueId = reader.GetNullableInt("QueueId") ?? 0,
                Caption = reader.GetNullableString("Caption"),
                Screenshot = reader["Screenshot"] is byte[] photoBytes ? GetPhotoBase64(photoBytes) : string.Empty,
                TakenAt = reader.GetNullableDateTime("TakenAt") ?? DateTime.MinValue
            }
            );

            return screenshots.FirstOrDefault();
        }

        public string GetPhotoBase64(byte[] photoBytes)
        {
            return $"data:image/png;base64,{Convert.ToBase64String(photoBytes)}";
        }

        public static byte[] ConvertBase64ToBytes(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                throw new ArgumentException("Screenshot data cannot be null or empty.", nameof(base64String));

            // If the string has a prefix (like data:image/png;base64,), remove it
            var cleanBase64 = base64String.Contains(",")
                ? base64String.Substring(base64String.IndexOf(",") + 1)
                : base64String;

            try
            {
                return Convert.FromBase64String(cleanBase64);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("The provided string is not a valid Base64 encoded string.", ex);
            }
        }

    }
}
