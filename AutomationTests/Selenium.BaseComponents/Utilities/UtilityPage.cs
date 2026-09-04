using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Configuration;


namespace Selenium.BaseComponents.Utilities
{

    public class UtilityPage
    {
        public static int PageTimeOut = GetAppSettingInt("ControlTimeout", 60);
        public static int ControlTimeOut = GetAppSettingInt("PageTimeout", 60);
        public static int SleepTimeOut = GetAppSettingInt("SleepTimeOut", 60);
        public static int DBTimeOut = GetAppSettingInt("DBTimeout", 60);

        private static int GetAppSettingInt(string key, int defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                return value != null ? Convert.ToInt32(value) : defaultValue;
            }
            catch { return defaultValue; }
        }

        public static void WaitFor(IWebDriver Driver, Func<IWebDriver, bool> waitCondition, int timeout = 100)
        {
            var wait = new WebDriverWait(Driver, new TimeSpan(0, 0, UtilityPage.PageTimeOut));
            wait.Until(waitCondition);
        }

        public static void ExecuteJSScript(IWebDriver Driver, string javaScript, params object[] args)
        {
            var js = Driver as IJavaScriptExecutor;
            js.ExecuteScript(javaScript, args);
        }

    }
}
