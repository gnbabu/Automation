using Selenium.BaseComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.SubmitClaims.Utilities
{
    public static class Mapper
    {
        public static T BindData<T>(DataTable dt)
        {
            DataRow dr = dt.Rows[0];

            // Get all columns' name
            List<string> columns = new List<string>();

            columns = dt.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("Fields")).ToList();

            columns = columns.Select(s => s.Trim()).ToList();

            var ob = Activator.CreateInstance<T>();

            var fields = typeof(T).GetFields();
            foreach (var fieldInfo in fields)
            {
                if (columns.Contains(fieldInfo.Name))
                {
                    fieldInfo.SetValue(ob, dr[fieldInfo.Name]);
                }
            }

            var properties = typeof(T).GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (columns.Contains(propertyInfo.Name))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string key = columns.Where(c => c == propertyInfo.Name).First();
                        string value = Convert.ToString(row[1]);

                        string datarowKey = !string.IsNullOrEmpty(Convert.ToString(row[0])) ? Convert.ToString(row[0]).TrimEnd() : "";

                        if (datarowKey.Equals(key))
                        {
                            propertyInfo.SetValue(ob, value);
                        }
                    }
                }
            }
            return ob;
        }

        public static T BindData<T>(List<AutomationContent> automationContent)
        {
            var ob = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            foreach (var propertyInfo in properties)
            {
                AutomationContent? automationContentByField = automationContent.Where(f => f.FieldName == propertyInfo.Name).FirstOrDefault();

                if (automationContentByField != null)
                {
                    string value = automationContentByField.FieldValue;
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = value.TrimStart().TrimEnd();
                    }
                    propertyInfo.SetValue(ob, value);
                }
            }

            return ob;
        }
    }
}
