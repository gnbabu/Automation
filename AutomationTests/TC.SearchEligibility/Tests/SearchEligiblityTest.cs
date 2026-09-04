
using NUnit.Framework;
using TC.MemberEligibilitySearch.Pages;
using OpenQA.Selenium;
using SdetToolbox.Pages;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
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
using TC.MemberEligibilitySearch.Utilities;
using TC.MemberEligibilitySearch.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace TC.MemberEligibilitySearch.Tests
{
    [TestFixture("TechAdmin")]
    public class SearchEligiblityTest : BaseFeatureFixture
    {
        public SearchEligiblityTest(string profile) : base(profile)
        {

        }


        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [CancelAfter(600000)]
        public void SearchMemberEligibility()
        {
            TC.MemberEligibilitySearch.Models.SearchEligibility searchEligibility = (TC.MemberEligibilitySearch.Models.SearchEligibility)DataRepository.GetAutomationData("SearchMemberEligiblity");

            SidebarMenu.Click();
            Thread.Sleep(4000);
            NavigateToSelfService();

            FinancialProviderInformationPage financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(searchEligibility != null && !string.IsNullOrEmpty(searchEligibility.RegID) ? 
                searchEligibility.RegID : "0005987");

            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            TC.MemberEligibilitySearch.Pages.SearchEligibility searchEligibilityPage = new TC.MemberEligibilitySearch.Pages.SearchEligibility(TestWebDriver);
            searchEligibilityPage.lnkBtnSearchEligibility.Click();
            searchEligibilityPage.WaitUntilElementIsVisible();

            if (searchEligibility != null)
            {

                if (!string.IsNullOrEmpty(searchEligibility.MedicaidBillingNumber))
                    searchEligibilityPage.txtMedicaidBillingNumber.Set(searchEligibility.MedicaidBillingNumber);

                if (!string.IsNullOrEmpty(searchEligibility.DateOfBirth))
                    searchEligibilityPage.txtBirthDate.Set(searchEligibility.DateOfBirth);

                if (!string.IsNullOrEmpty(searchEligibility.FromDOS))
                    searchEligibilityPage.txtFromDos.Set(searchEligibility.FromDOS);

                if (!string.IsNullOrEmpty(searchEligibility.ToDOS))
                    searchEligibilityPage.txtToDos.Set(searchEligibility.ToDOS);

                if (!string.IsNullOrEmpty(searchEligibility.SSN))
                    searchEligibilityPage.txtSSN.Set(searchEligibility.SSN);

            }

            Thread.Sleep(3000);

            IJavaScriptExecutor jsExecutor;

            jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", searchEligibilityPage.btnSearch);
            Thread.Sleep(3000);

            if (searchEligibilityPage.lblErrorMsg.Displayed)
            {
                NUnit.Framework.Assert.Fail($"No data found with the search criteria.");
            }

            string medicaidBillingNumber = searchEligibilityPage.txtRecinfoMedicaidbillNumber.GetAttribute("value");
            string lastName = searchEligibilityPage.txtLast.GetAttribute("value");
            string firstname = searchEligibilityPage.txtFirstName.GetAttribute("value");
            string dob = searchEligibilityPage.txtDOB.GetAttribute("value");

            if (!string.IsNullOrEmpty(medicaidBillingNumber) && !string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(firstname)
                && !string.IsNullOrEmpty(dob))
            {
                NUnit.Framework.TestContext.WriteLine($"Member eligibility data found.");
            }
        }

        public IWebElement SidebarMenu
        {
            get
            {
                return TestWebDriver.CreateSmartElement(By.XPath("//button[contains(@class,'hamburger is-closed')]")).Element;
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
                return TestWebDriver.CreateSmartElement(By.XPath($"//a[@title='Self Service']")).Element;

            }
        }
    }
}