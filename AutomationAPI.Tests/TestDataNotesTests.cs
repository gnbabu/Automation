using System.Xml;
using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.TestRunner;
using NUnit.Framework;

namespace AutomationAPI.Tests
{
    [TestFixture]
    public class TestDataNotesTests
    {
        private const string ExploreXml = """
            <test-run>
              <test-suite type="Assembly" name="TC.PriorAuthoriztion.dll">
                <test-suite type="TestFixture" name="SubmitPAProfessionalTest(&quot;TechAdmin&quot;)" classname="TC.PriorAuthoriztion.Tests.SubmitPAProfessionalTest">
                  <properties>
                    <property name="TestDataFlow" value="TestDataModels.SubmitPAProfessional, TestDataModels" />
                    <property name="TestDataNote" value="{&quot;section&quot;:&quot;Attachments&quot;,&quot;field&quot;:null,&quot;message&quot;:&quot;Need one attachment.&quot;}" />
                  </properties>
                  <test-case name="ProfessionalPA_Save" methodname="ProfessionalPA_Save" classname="TC.PriorAuthoriztion.Tests.SubmitPAProfessionalTest">
                    <properties>
                      <property name="TestCaseId" value="TC_PA_PROF_01" />
                    </properties>
                  </test-case>
                  <test-case name="ProfessionalPA_SaveMultiple" methodname="ProfessionalPA_SaveMultiple" classname="TC.PriorAuthoriztion.Tests.SubmitPAProfessionalTest">
                    <properties>
                      <property name="TestCaseId" value="TC_PA_PROF_02" />
                      <property name="TestDataNote" value="{&quot;section&quot;:&quot;Diagnoses&quot;,&quot;field&quot;:null,&quot;message&quot;:&quot;Needs 2 rows.&quot;}" />
                      <property name="TestDataNote" value="{&quot;section&quot;:null,&quot;field&quot;:null,&quot;message&quot;:&quot;General note.&quot;}" />
                    </properties>
                  </test-case>
                </test-suite>
              </test-suite>
            </test-run>
            """;

        private static TestDataFlowSchema Schema() => new()
        {
            FlowName = "SubmitPAProfessional",
            Sections =
            {
                new TestDataSectionSchema { SectionName = "Attachments", IsList = true, Fields = { new TestDataFieldSchema { Name = "FilePath" } } },
                new TestDataSectionSchema { SectionName = "Diagnoses", IsList = true, Fields = { new TestDataFieldSchema { Name = "DiagnosisCode" } } }
            }
        };

        private static List<LibraryMethodInfo> ParseMethods()
        {
            var doc = new XmlDocument();
            doc.LoadXml(ExploreXml);
            return ExploreXmlParser.ParseClasses(doc.DocumentElement!).Single().Methods;
        }

        [Test]
        public void ClassNotes_ApplyToEveryTest_AndMethodNotesOnlyToTheirOwn()
        {
            var methods = ParseMethods();
            var first = methods.Single(m => m.TestCaseId == "TC_PA_PROF_01");
            var second = methods.Single(m => m.TestCaseId == "TC_PA_PROF_02");

            Assert.That(first.TestDataNotes, Has.Count.EqualTo(1));
            Assert.That(second.TestDataNotes, Has.Count.EqualTo(3));
            Assert.That(second.TestDataNotes[0], Does.Contain("Need one attachment."));
        }

        [Test]
        public void ParseNotes_MapsSectionFieldAndMessage()
        {
            var raw = new[] { "{\"section\":\"Attachments\",\"field\":\"FilePath\",\"message\":\"Full path.\"}" };

            var (notes, warnings) = TestDataService.ParseNotes(Schema(), raw);

            Assert.That(warnings, Is.Empty);
            Assert.That(notes.Single().Section, Is.EqualTo("Attachments"));
            Assert.That(notes.Single().Field, Is.EqualTo("FilePath"));
            Assert.That(notes.Single().Message, Is.EqualTo("Full path."));
        }

        [Test]
        public void UnknownSection_BecomesGeneralNoteWithWarning()
        {
            var raw = new[] { "{\"section\":\"Atachments\",\"field\":\"FilePath\",\"message\":\"Typo section.\"}" };

            var (notes, warnings) = TestDataService.ParseNotes(Schema(), raw);

            Assert.That(notes.Single().Section, Is.Null);
            Assert.That(notes.Single().Field, Is.Null);
            Assert.That(warnings.Single(), Does.Contain("\"Atachments\""));
        }

        [Test]
        public void UnknownField_BecomesSectionNoteWithWarning()
        {
            var raw = new[] { "{\"section\":\"Attachments\",\"field\":\"Path\",\"message\":\"Typo field.\"}" };

            var (notes, warnings) = TestDataService.ParseNotes(Schema(), raw);

            Assert.That(notes.Single().Section, Is.EqualTo("Attachments"));
            Assert.That(notes.Single().Field, Is.Null);
            Assert.That(warnings.Single(), Does.Contain("\"Path\""));
        }

        [Test]
        public void PlainText_IsAGeneralNote_AndEmptyMessagesAreSkipped()
        {
            var raw = new[] { "Just some text", "{\"section\":null,\"field\":null,\"message\":\"  \"}" };

            var (notes, warnings) = TestDataService.ParseNotes(Schema(), raw);

            Assert.That(notes.Single().Message, Is.EqualTo("Just some text"));
            Assert.That(notes.Single().Section, Is.Null);
            Assert.That(warnings, Is.Empty);
        }
    }
}
