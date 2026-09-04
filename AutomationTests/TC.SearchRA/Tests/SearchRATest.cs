
using NUnit.Framework;
using TC.SearchRA.Pages;
 
using OpenQA.Selenium;
using SdetToolbox.Pages;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumExtensions.Extensions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtensions.Configurations;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace TC.SearchRA.Tests
{
    [TestFixture("TechAdmin")]
    public class SearchRATest : BaseFeatureFixture
    {
        public SearchRATest(string profile) : base(profile)
        {

        }


        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [CancelAfter(600000)]
        public void SearchRA()
        {
            TC.SearchRA.Models.SearchRA searchRA = (TC.SearchRA.Models.SearchRA)TC.SearchRA.Utilities.DataRepository.GetAutomationData("SearchRA");

            SidebarMenu.Click();
            Thread.Sleep(4000);
            NavigateToSelfService();

            TC.SearchRA.Pages.FinancialProviderInformationPage financialProviderInformationPage = new TC.SearchRA.Pages.FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(searchRA != null && !string.IsNullOrEmpty(searchRA.RegID) ? searchRA.RegID : "0005987");
            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            TC.SearchRA.Pages.SearchRAPage searchRAPage = new TC.SearchRA.Pages.SearchRAPage(TestWebDriver);
            searchRAPage.lnkBtnSearchRA.Click();
            searchRAPage.WaitUntilElementIsVisible();

            if (searchRA != null)
            {

                if (!string.IsNullOrEmpty(searchRA.DestinationPayerName))
                    searchRAPage.SetPrimaryDestinationPayer(searchRA.DestinationPayerName);

                if (!string.IsNullOrEmpty(searchRA.RANumber))
                    searchRAPage.txtRANumber.Set(searchRA.RANumber);

                if (!string.IsNullOrEmpty(searchRA.ICN))
                    searchRAPage.txtICN.Set(searchRA.ICN);

                if (!string.IsNullOrEmpty(searchRA.ReportRunDateFrom))
                    searchRAPage.txtDateAvailableTo.Set(searchRA.ReportRunDateFrom);

                if (!string.IsNullOrEmpty(searchRA.ToDate))
                    searchRAPage.txtDateAvailableTo.Set(searchRA.ToDate);

            }

            Thread.Sleep(3000);

            IJavaScriptExecutor jsExecutor;

            jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", searchRAPage.btnSearch);
            Thread.Sleep(3000);

            IWebElement searchTable = TestWebDriver.FindElement(By.Id("ctl00_MainContent_ERemittanceAdvice_gvRemittanceAdvicesearch"));

            if (searchTable.Displayed)
            {
                IWebElement tbody = searchTable.FindElement(By.TagName("tbody"));
                if (tbody.Displayed)
                {
                    IWebElement emptydataRow = tbody.FindElement(By.ClassName("gridViewEmptyRow"));

                    if (emptydataRow.Displayed)
                    {
                        IWebElement emptytd = emptydataRow.FindElement(By.TagName("td"));

                        if (emptytd.Displayed)
                        {
                            string searchMessage = emptytd.Text;
                           NUnit.Framework.TestContext.WriteLine($"No data found with the search criteria.");
                        }
                    }
                }
            }
        }

        public IWebElement SidebarMenu
        {
            get
            {
                return TestWebDriver.FindElement(By.XPath("//button[contains(@class,'hamburger is-closed')]"), null);
            }
        }
        public void NavigateToSelfService()
        {
            if (SelfService != null)
            {
                SelfService.Click();
            }
        }
        public IWebElement SelfService
        {
            get
            {
                return TestWebDriver.FindElement(By.XPath($"//a[@title='Self Service']"), null);

            }
        }
    }
}