using Microsoft.Data.SqlClient;

namespace AutomationAPI.Repositories.Helpers
{
    public static class SqlReaderExtensions
    {
        // Constrained to value types: returns a true null (Nullable<T>) for DB NULLs
        // instead of default(T) (e.g. DateTime.MinValue for DateTime, 0 for int), which
        // was silently indistinguishable from a real value at every call site.
        public static T? GetNullable<T>(this SqlDataReader reader, string columnName) where T : struct
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (T?)null : reader.GetFieldValue<T>(ordinal);
        }

        public static string? GetNullableString(this SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        public static int? GetNullableInt(this SqlDataReader reader, string columnName)
        {
            return reader.GetNullable<int>(columnName);
        }

        public static DateTime? GetNullableDateTime(this SqlDataReader reader, string columnName)
        {
            return reader.GetNullable<DateTime>(columnName);
        }
        public static double? GetNullableDouble(this SqlDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? (double?)null : reader.GetDouble(ordinal);
        }

    }

}
