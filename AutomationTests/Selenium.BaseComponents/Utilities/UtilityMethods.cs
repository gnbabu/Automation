using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Reflection;

namespace Selenium.BaseComponents.Utilities
{
    public static class UtilityMethods
    {
        public static string GetRESTResponse(string Uri)
        {
            string responseFromServer = null;
            var request = WebRequest.Create(Uri);
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            using (StreamReader reader = new StreamReader(dataStream))
            {
                responseFromServer = reader.ReadToEnd();
            }
            return responseFromServer;
        }

        public static T ConvertToEnum<T>(this string value)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                if (field.CustomAttributes.Count() > 0 && field.CustomAttributes.First().ConstructorArguments.Count() > 0)
                {
                    string attribute = field.CustomAttributes.First().ConstructorArguments.First().Value.ToString();
                    if (attribute != null)
                    {
                        if (attribute == value || field.Name == value)
                            return (T)field.GetValue(null);
                    }
                }
            }
            throw new ArgumentException(string.Format("Cannot convert string {0} to type {1}", value, typeof(T)));
        }

        public static string ConvertToString<T>(this T value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            System.ComponentModel.DescriptionAttribute[] attributes =
                 (System.ComponentModel.DescriptionAttribute[])fi.GetCustomAttributes(
                 typeof(System.ComponentModel.DescriptionAttribute),
                 false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

      

      

        public static int GetColumnIndex(this IWebElement tableElement, string columnName)
        {
            List<string> columnNames = tableElement.GetColumnNames();
            for (int i = 0; i <= columnNames.Count; i++)
            {
                if (columnNames[i].Contains(columnName))
                {
                    return i + 1;
                }
            }
            return -1;
        }

        public static List<string> GetColumnNames(this IWebElement tableElement)
        {
            List<IWebElement> columnHeaders = tableElement.FindElements(By.CssSelector("tr.headerRow>th")).ToList();
            List<string> columnNames = new List<string>();
            foreach (IWebElement element in columnHeaders)
                columnNames.Add(element.Text.Trim());
            return columnNames;
        }

        public static int GetRowCount(this IWebElement tableElement)
        {
            int rowCount = 0;
            try
            {
                rowCount = tableElement.FindElements(By.CssSelector("tbody tr")).Count;

                if (rowCount <= 0)
                    throw new Exception("Error returning the row count for the given table");
            }
            catch { }
            return rowCount;
        }

        public static void SelectOptionFromDDL(IWebDriver webDriver, SelectElement webElement, string value)
        {
            foreach (IWebElement element in webElement.Options)
            {
                if (element.Text == value)
                {
                    element.Click();
                    break;
                }
            }
        }
        public static void SelectOptionByIndexFromDDL(IWebDriver webDriver, SelectElement webElement, int index)
        {
            webElement.SelectByIndex(index);
        }

        public static void SelectOptionFromMultiPicklist(IWebDriver webDriver, SelectElement webElement, string value)
        {
            Actions action = new Actions(webDriver);
            foreach (IWebElement element in webElement.Options)
            {
                if (element.Text == value)
                {
                    action.DoubleClick(element).Build().Perform();
                    break;
                }
            }
        }

        public static void ClickElement(this IWebDriver webDriver, IWebElement webElement)
        {
            ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].click();", webElement);
        }

        public static bool CompareValues<T>(string fieldName, T actualValue, T expectedValue)
        {
            Type valueType = typeof(T);
            bool result;

            if (valueType.ToString() == "System.String")
                result = Convert.ChangeType(actualValue, typeof(T)).ToString().ToUpper().Equals((Convert.ChangeType(expectedValue, typeof(T))).ToString().ToUpper());
            else
                result = Convert.ChangeType(actualValue, typeof(T)).Equals((Convert.ChangeType(expectedValue, typeof(T))));

            if (!result)
                Console.WriteLine(string.Format("FieldName: {0} ---- Actual: {1}, Expected: {2} ---- Match: {3}", fieldName, actualValue, expectedValue, result));

            return result;
        }

        public static void AddAllToDictionary<T, S>(this Dictionary<T, S> target, Dictionary<T, S> source, bool overrideExisting = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source is null");
            }

            foreach (var item in source)
            {
                if (!target.ContainsKey(item.Key) || overrideExisting)
                {
                    target.Add(item.Key, item.Value);
                }
            }
        }
        public static void AddAllKeyToList<T, S>(this Dictionary<T, S> source, List<T> target, bool allowduplicates = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source is null");
            }
            foreach (var item in source)
            {
                if (!target.Contains(item.Key) || allowduplicates)
                {
                    target.Add(item.Key);
                }
            }
        }

        public static void AddAllValueToList<T, S>(this Dictionary<T, S> source, List<S> target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source is null");
            }
            foreach (var item in source)
            {
                target.Add(item.Value);
            }
        }
    }
}
