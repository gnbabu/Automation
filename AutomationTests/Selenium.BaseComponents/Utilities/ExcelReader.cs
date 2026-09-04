using System.Data;
using System.Data.OleDb;

namespace Selenium.BaseComponents.Utilities
{
    public static class ExcelReader
    {
        public static OleDbConnection GetConnection(string filename, bool openIt)
        {
            // if your data has no header row, change HDR=NO
            var c = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{filename}';Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\" ");
            if (openIt)
                c.Open();
            return c;
        }

        public static DataSet GetExcelFileAsDataSet(string filename, bool openIt)
        {
            var conn = GetConnection(filename, openIt);
           
            var sheets = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new[] { default, default, default, "TABLE" });
            var ds = new DataSet();
            foreach (DataRow r in sheets.Rows)
                ds.Tables.Add(GetExcelSheetAsDataTable(conn, r["TABLE_NAME"].ToString()));
            return ds;
        }

        public static DataTable GetExcelSheetAsDataTable(OleDbConnection conn, string sheetName)
        {
            using (var da = new OleDbDataAdapter($"select * from [{sheetName}]", conn))
            {
                var dt = new DataTable() { TableName = sheetName.TrimEnd('$') };
                da.Fill(dt);
                return dt;
            }
        }

    }
}
