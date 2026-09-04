using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.SearchRA.Utilities
{
    public static class Helper
    {

        public static void PrintScreenShot(IWebDriver TestWebDriver,string title)
        {
            string screenshotPath = @"C:\Projects\" + title + "_" + DateTime.Now.ToString("MMddyyyyhhmmssfff") + ".png";
           // Screenshot screenshot = (TestWebDriver as ITakesScreenshot).GetScreenshot();
           // screenshot.SaveAsFile(screenshotPath);
        }
    }
}
