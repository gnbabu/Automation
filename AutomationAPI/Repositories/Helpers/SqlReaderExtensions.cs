using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories.Helpers
{
    public static class SqlReaderExtensions
    {
        public static T? GetNullable<T>(this SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? default : reader.GetFieldValue<T>(ordinal);
        }

        public static string? GetNullableString(this SqlDataReader reader, string columnName)
        {
            return reader.GetNullable<string>(columnName);
        }

        public static int? GetNullableInt(this SqlDataReader reader, string columnName)
        {
            return reader.GetNullable<int>(columnName);
        }

        public static DateTime? GetNullableDateTime(this SqlDataReader reader, string columnName)
        {
            return reader.GetNullable<DateTime>(columnName);
        }
    }

}
