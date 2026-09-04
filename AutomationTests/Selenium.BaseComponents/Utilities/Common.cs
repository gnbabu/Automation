using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Data;

namespace Selenium.BaseComponents.Utilities
{
    public static class Common
    {
        public static void VerifyHeadersOfTable(IWebElement element, String data)
        {
            System.Threading.Thread.Sleep(2000);
            String[] headers = data.Split('~');
            IList<IWebElement> columns = element.FindElements(By.TagName("th"));
            for (int i = 0; i < headers.Length; i++)
            {
                Assert.AreEqual(columns[i].Text, headers[i]);
            }
        }
        

        public static void SwitchToWindowWithURL(IWebDriver webDriver, string url)
        {
            IList<String> windows = webDriver.WindowHandles;
            foreach (string item in windows)
            {
                webDriver.SwitchTo().Window(item);
                if (webDriver.Url.Contains(url))
                    break;
            }
        }
        public static void loginToPartnerPortal(IWebDriver webDriver, DataRow row)
        {
            webDriver.Navigate().GoToUrl(row["SFDCLoginPage"].ToString() + "/" + row["PartnerContact"].ToString());
            //_contactPage = new SFDCContactPage(webDriver);
            //_contactPage.LoginToPortalAsUser();
        }

       

        public static void verifySharingSettings(IWebDriver webDriver, String Object, String internalAccess, String externalAccess, String acessusinghy)
        {
            Assert.AreEqual(internalAccess, webDriver.FindElement(By.XPath("//th[text()='" + Object + "']/following-sibling::td[1]")).Text, "internal Access is not as expeced");
            Assert.AreEqual(externalAccess, webDriver.FindElement(By.XPath("//th[text()='" + Object + "']/following-sibling::td[2]")).Text, "external Access is not as expeced");
            Assert.AreEqual(acessusinghy, webDriver.FindElement(By.XPath("//th[text()='" + Object + "']/following-sibling::td[3]/img")).GetAttribute("title"), "heirarcy is not as expeced");
        }

        public static void Verify_Total_Discount_Price_Field_info_In_setup_page(IWebDriver webDriver)
        {
            Assert.AreEqual("Total Discounted Price", webDriver.FindElement(By.XPath("//td[text()='Field Label']//following-sibling::td[1]")).Text.Trim());
            Assert.AreEqual("Formula", webDriver.FindElement(By.XPath("//td[text()='Data Type']//following-sibling::td[1]")).Text.Trim());
            Assert.AreEqual("", webDriver.FindElement(By.XPath("//label[text()='Help Text']//parent::td//following-sibling::td[1]")).Text.Trim());
        }

        public static void Verify_first_Item_In_DropDownList(this IWebDriver webDriver, string fieldName, string dropDownItem)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            SelectElement select = new SelectElement(webDriver.FindElement(By.XPath("//label[text()='" + fieldName + "']//parent::td//following-sibling::td[1]//select")));
            IList<IWebElement> allOptions = select.Options;
            Assert.AreEqual(allOptions.ElementAt(1).Text, dropDownItem, allOptions.ElementAt(1).Text + " and " + dropDownItem + "are not equal");
        }

        public static void SelectItemFromDropdown(this IWebDriver webDriver, IWebElement dropdown, string selection)
        {
            try
            {

                IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                js.ExecuteScript("arguments[0].click();", dropdown);
                // editPage.SolutionType.Click();

                IWebElement selectElement = dropdown.FindElement(By.XPath(String.Format("(//span[text()='{0}'])[1] | (//a[text()='{0}'])[1]", selection)));

                //selectElement.Click();
                js.ExecuteScript("arguments[0].click();", selectElement);
            }
            catch (NoSuchElementException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        public static byte[] PrintScreenShot(IWebDriver TestWebDriver,string screenshotName="")
        {
            var screenshot = (TestWebDriver as ITakesScreenshot)?.GetScreenshot();
            return screenshot?.AsByteArray ?? Array.Empty<byte>();
        }
    }
}
