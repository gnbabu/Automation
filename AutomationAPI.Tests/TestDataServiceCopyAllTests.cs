using System.Text.Json.Nodes;
using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Moq;
using NUnit.Framework;

namespace AutomationAPI.Tests
{
    // "Copy from test case" must only ever copy between test cases of the SAME test-data
    // model, and only keys that exist in that model.
    [TestFixture]
    public class TestDataServiceCopyAllTests
    {
        private const int UserId = 1;
        private const int EnvironmentId = 21;
        private const string Flow = "SubmitPAProfessional";

        private Mock<ITestDataRepository> _repository = null!;
        private TestDataService _service = null!;
        private string? _savedJson;

        private static readonly TestDataAssignment Target = new()
        {
            AssignmentTestCaseId = 192,
            LibraryName = "TC.PriorAuthoriztion",
            ClassName = "SubmitPAProfessionalTest",
            MethodName = "ProfessionalPA_SaveMultipleDiagnosesWithoutOrderingProvider",
            AssignedUser = UserId,
            EnvironmentId = EnvironmentId
        };

        // Mirrors what TestDataSchemaReader builds for SubmitPAProfessional (trimmed):
        // a class section, a List<T> section, and the model's own scalar fields.
        private static readonly TestDataFlowSchema Schema = new()
        {
            FlowName = Flow,
            Sections =
            {
                new TestDataSectionSchema { SectionId = 56, SectionName = "Recipient", Fields = { new() { Name = "MedicaidBillingNumber" } } },
                new TestDataSectionSchema { SectionId = 52, SectionName = "Diagnoses", IsList = true, Fields = { new() { Name = "DiagnosisCode" } } },
                new TestDataSectionSchema { SectionId = 62, SectionName = Flow, Fields = { new() { Name = "ProviderNotes" } } }
            }
        };

        [SetUp]
        public void SetUp()
        {
            _savedJson = null;
            _repository = new Mock<ITestDataRepository>();
            _repository
                .Setup(r => r.SaveDocumentAsync(Target.LibraryName, Target.ClassName, Target.MethodName,
                    UserId, EnvironmentId, Flow, It.IsAny<string>(), It.IsAny<long?>()))
                .Callback<string, string, string, int, int, string, string, long?>((_, _, _, _, _, _, json, _) => _savedJson = json)
                .ReturnsAsync(new TestDataSaveResult { TestCaseDataId = 7, Version = 2 });

            _service = new TestDataService(_repository.Object, Mock.Of<ITestSuitesRepository>(), Mock.Of<IReleaseRepository>());
        }

        private void GivenSource(string flowName, string json) =>
            _repository
                .Setup(r => r.GetDocumentAsync("TC.PriorAuthoriztion", "SubmitPAProfessionalTest", "ProfessionalPA_Save", UserId, EnvironmentId))
                .ReturnsAsync(new TestDataDocumentRecord { FlowName = flowName, DataJson = json, Version = 1 });

        private void GivenTarget(TestDataDocumentRecord? record) =>
            _repository
                .Setup(r => r.GetDocumentAsync(Target.LibraryName, Target.ClassName, Target.MethodName, UserId, EnvironmentId))
                .ReturnsAsync(record);

        private Task<(TestDataSaveResult Result, List<string> CopiedSections)> CopyFromProf01(long? expectedVersion = null) =>
            _service.CopyAllAsync(Target, Schema,
                "TC.PriorAuthoriztion", "SubmitPAProfessionalTest", "ProfessionalPA_Save", EnvironmentId,
                EnvironmentId, UserId, expectedVersion);

        [Test]
        public async Task CopyAll_FromTestCaseOfSameModel_CopiesItsSections()
        {
            GivenSource(Flow, """
                {"Recipient":{"MedicaidBillingNumber":"0005987"},
                 "Diagnoses":[{"DiagnosisCode":"Z00.00"},{"DiagnosisCode":"E11.9"}],
                 "ProviderNotes":"Follow-up"}
                """);
            GivenTarget(null);

            var (_, copied) = await CopyFromProf01();

            Assert.That(copied, Is.EquivalentTo(new[] { "Recipient", "Diagnoses", Flow }));
            var saved = JsonNode.Parse(_savedJson!)!.AsObject();
            Assert.That(saved["Recipient"]!["MedicaidBillingNumber"]!.GetValue<string>(), Is.EqualTo("0005987"));
            Assert.That(saved["Diagnoses"]!.AsArray(), Has.Count.EqualTo(2));
            Assert.That(saved["ProviderNotes"]!.GetValue<string>(), Is.EqualTo("Follow-up"));
        }

        [Test]
        public void CopyAll_FromTestCaseOfDifferentModel_IsRejectedAndNothingIsWritten()
        {
            GivenSource("DentalPA", """{"Recipient":{"MedicaidBillingNumber":"0005987"}}""");
            GivenTarget(null);

            Assert.ThrowsAsync<TestDataValidationException>(() => CopyFromProf01());
            _repository.Verify(r => r.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task CopyAll_DropsKeysThatAreNotInTheModel()
        {
            GivenSource(Flow, """{"Recipient":{"MedicaidBillingNumber":"0005987"},"OldRemovedSection":{"X":"1"},"OldField":"y"}""");
            GivenTarget(null);

            var (_, copied) = await CopyFromProf01();

            Assert.That(copied, Is.EqualTo(new[] { "Recipient" }));
            var saved = JsonNode.Parse(_savedJson!)!.AsObject();
            Assert.That(saved.Select(kv => kv.Key), Is.EquivalentTo(new[] { "Recipient" }));
        }

        [Test]
        public async Task CopyAll_ReplacesTheWholeTargetDocument()
        {
            GivenSource(Flow, """{"Recipient":{"MedicaidBillingNumber":"111"}}""");
            GivenTarget(new TestDataDocumentRecord
            {
                FlowName = Flow,
                Version = 5,
                DataJson = """{"Recipient":{"MedicaidBillingNumber":"999"},"Diagnoses":[{"DiagnosisCode":"A00"}]}"""
            });

            await CopyFromProf01(expectedVersion: 5);

            // Target's own Diagnoses is gone - it goes back to the model's defaults.
            var saved = JsonNode.Parse(_savedJson!)!.AsObject();
            Assert.That(saved.Select(kv => kv.Key), Is.EquivalentTo(new[] { "Recipient" }));
            Assert.That(saved["Recipient"]!["MedicaidBillingNumber"]!.GetValue<string>(), Is.EqualTo("111"));
            _repository.Verify(r => r.SaveDocumentAsync(Target.LibraryName, Target.ClassName, Target.MethodName,
                UserId, EnvironmentId, Flow, It.IsAny<string>(), 5L), Times.Once);
        }

        [Test]
        public void CopyAll_SourceWithoutSavedData_IsRejected()
        {
            _repository
                .Setup(r => r.GetDocumentAsync("TC.PriorAuthoriztion", "SubmitPAProfessionalTest", "ProfessionalPA_Save", UserId, EnvironmentId))
                .ReturnsAsync((TestDataDocumentRecord?)null);
            GivenTarget(null);

            Assert.ThrowsAsync<TestDataValidationException>(() => CopyFromProf01());
        }

        [Test]
        public void CopyAll_WhenTargetChangedSinceLoaded_IsAConflictAndNothingIsWritten()
        {
            GivenSource(Flow, """{"Recipient":{"MedicaidBillingNumber":"111"}}""");
            GivenTarget(new TestDataDocumentRecord { FlowName = Flow, Version = 9, DataJson = "{}" });

            Assert.ThrowsAsync<TestDataConflictException>(() => CopyFromProf01(expectedVersion: 5));
            _repository.Verify(r => r.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }
    }
}
