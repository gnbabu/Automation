using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SdetToolbox.Pages;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace TC.SubmitClaims.Tests
{
    [TestFixture("TechAdmin")]
    public class SampleTest : BaseFeatureFixture
    {
        public SampleTest(string profile) : base(profile)
        {

        }

        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [CancelAfter(600000)]
        public void SampleTestCase()
        {

        }
    }
}
