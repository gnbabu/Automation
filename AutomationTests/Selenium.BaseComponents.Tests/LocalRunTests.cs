using NUnit.Framework;
using Selenium.BaseComponents.Utilities;

namespace Selenium.BaseComponents.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class LocalRunTests
    {
        private string _dir = null!;

        [SetUp]
        public void CreateFolder()
        {
            _dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "localrun-" + Guid.NewGuid())).FullName;
            LocalRun.DirectoryOverride = _dir;
        }

        [TearDown]
        public void DeleteFolder()
        {
            LocalRun.DirectoryOverride = null;
            Directory.Delete(_dir, true);
        }

        private void WriteConfig(string json) => File.WriteAllText(Path.Combine(_dir, LocalRun.FileName), json);

        [Test]
        public void InactiveWithoutTheFile()
        {
            Assert.That(LocalRun.IsActive, Is.False);
        }

        [Test]
        public void ActiveWhenFileExistsAndNoAccessToken()
        {
            WriteConfig("{}");
            Assert.That(LocalRun.IsActive, Is.True);
        }

        [Test]
        [Property("TestCaseId", "TC_DEMO_1")]
        public void TestDataPrefersTestCaseId()
        {
            WriteConfig("{ \"TestData\": { \"TC_DEMO_1\": { \"A\": 1 }, \"FlowX\": { \"A\": 2 } } }");

            var (json, source) = LocalRun.TestData("FlowX");

            Assert.That(json, Does.Contain("1"));
            Assert.That(source, Does.Contain("TC_DEMO_1"));
        }

        [Test]
        [Property("TestCaseId", "TC_DEMO_2")]
        public void PerTestCaseFileWins()
        {
            WriteConfig("{ \"TestData\": { \"TC_DEMO_2\": { \"A\": 1 } } }");
            Directory.CreateDirectory(Path.Combine(_dir, LocalRun.DataFolder));
            File.WriteAllText(Path.Combine(_dir, LocalRun.DataFolder, "TC_DEMO_2.json"), "{ \"A\": 9 }");

            var (json, source) = LocalRun.TestData("FlowX");

            Assert.That(json, Does.Contain("9"));
            Assert.That(source, Does.Contain("TC_DEMO_2.json"));
        }

        [Test]
        [Property("TestCaseId", "TC_LEGACY")]
        public void PerTestCaseFileWithArrayIsLegacyAutomationData()
        {
            WriteConfig("{}");
            Directory.CreateDirectory(Path.Combine(_dir, LocalRun.DataFolder));
            File.WriteAllText(Path.Combine(_dir, LocalRun.DataFolder, "TC_LEGACY.json"),
                "[ { \"SectionName\": \"S\", \"automationContents\": [ { \"FieldName\": \"RegID\", \"FieldValue\": \"5\" } ] } ]");

            Assert.That(LocalRun.AutomationData("AnyFlow")[0].automationContents[0].FieldValue, Is.EqualTo("5"));
        }

        [Test]
        public void TestDataFallsBackToFlowThenDefaults()
        {
            WriteConfig("{ \"TestData\": { \"FlowX\": { \"A\": 2 } } }");

            Assert.That(LocalRun.TestData("FlowX").Json, Does.Contain("2"));
            Assert.That(LocalRun.TestData("FlowY").Json, Is.EqualTo("{}"));
        }

        [Test]
        public void ReadsEnvironmentAndLoginUsers()
        {
            WriteConfig("{ \"Environment\": { \"EnvironmentUrl\": \"https://x/Login.aspx\", \"RequiresAuthentication\": true }," +
                        "  \"LoginUser\": { \"UserName\": \"u1\", \"Password\": \"p1\" }," +
                        "  \"LoginUsersByRole\": { \"StateAdmin\": { \"UserName\": \"u2\", \"Password\": \"p2\" } } }");

            Assert.That(LocalRun.Environment().EnvironmentUrl, Is.EqualTo("https://x/Login.aspx"));
            Assert.That(LocalRun.LoginUser()!.UserName, Is.EqualTo("u1"));
            Assert.That(LocalRun.LoginUserByRole("stateadmin").UserName, Is.EqualTo("u2"));
            Assert.Throws<InvalidOperationException>(() => LocalRun.LoginUserByRole("Missing"));
        }

        [Test]
        public void SecretsFileIsMergedOverSharedSettings()
        {
            WriteConfig("{ \"LoginUser\": { \"UserName\": \"u1\" }, \"Otp\": { \"PnmUserId\": \"123\" } }");
            File.WriteAllText(Path.Combine(_dir, LocalRun.SecretsFileName),
                "{ \"LoginUser\": { \"Password\": \"secret\" }, \"Otp\": { \"TotpKey\": \"KEY\" } }");

            Assert.That(LocalRun.LoginUser()!.UserName, Is.EqualTo("u1"));
            Assert.That(LocalRun.LoginUser()!.Password, Is.EqualTo("secret"));
            Assert.That(LocalRun.Otp("PnmUserId"), Is.EqualTo("123"));
            Assert.That(LocalRun.Otp("TotpKey"), Is.EqualTo("KEY"));
        }

        [Test]
        public void ReadsLegacyAutomationData()
        {
            WriteConfig("{ \"AutomationData\": { \"SearchMemberEligiblity\": [ { \"SectionName\": \"S\", \"automationContents\": [ { \"FieldName\": \"SSN\", \"FieldValue\": \"1\" } ] } ] } }");

            var rows = LocalRun.AutomationData("SearchMemberEligiblity");

            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows[0].automationContents[0].FieldName, Is.EqualTo("SSN"));
            Assert.That(LocalRun.AutomationData("Other"), Is.Empty);
        }

        [Test]
        public void GatewaySavesScreenshotsLocallyInsteadOfCallingTheApi()
        {
            WriteConfig("{}");
            var png = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 });

            var id = new APIGatway().SaveMethodScreenShot(new TestScreenshot { Caption = "Step One", Screenshot = $"data:image/png;base64,{png}" });

            Assert.That(id, Is.Null);
            var files = Directory.GetFiles(Path.Combine(_dir, "LocalRunResults"), "*.png", SearchOption.AllDirectories);
            Assert.That(files, Has.Length.EqualTo(1));
            Assert.That(Path.GetFileName(files[0]), Is.EqualTo("01_StepOne.png"));
        }

        [Test]
        public void GatewayReturnsLocalEnvironmentWithoutParameters()
        {
            WriteConfig("{ \"Environment\": { \"EnvironmentUrl\": \"https://local/Login.aspx\" } }");

            var environment = new APIGatway().GetEnvironmentDetails().GetAwaiter().GetResult();

            Assert.That(environment!.EnvironmentUrl, Is.EqualTo("https://local/Login.aspx"));
        }
    }
}
