using System.Text.Json.Nodes;
using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Moq;
using NUnit.Framework;

namespace AutomationAPI.Tests
{
    [TestFixture]
    public class TestDataServiceDocumentTests
    {
        private const int UserId = 1;
        private const int EnvironmentId = 21;

        private Mock<ITestDataRepository> _repository = null!;
        private TestDataService _service = null!;
        private string? _savedJson;
        private readonly TestDataFlowSchema _schema = TestDataSchemas.SubmitPA();

        private static readonly TestDataAssignment Assignment = new()
        {
            AssignmentTestCaseId = 192,
            LibraryName = "TC.PriorAuthoriztion",
            ClassName = "SubmitPAProfessionalTest",
            MethodName = "ProfessionalPA_SaveMultipleDiagnosesWithoutOrderingProvider",
            AssignedUser = UserId,
            EnvironmentId = EnvironmentId
        };

        [SetUp]
        public void SetUp()
        {
            _savedJson = null;
            _repository = new Mock<ITestDataRepository>();
            _repository
                .Setup(r => r.SaveDocumentAsync(Assignment.LibraryName, Assignment.ClassName, Assignment.MethodName,
                    UserId, EnvironmentId, TestDataSchemas.Flow, It.IsAny<string>(), It.IsAny<long?>()))
                .Callback<string, string, string, int, int, string, string, long?>((_, _, _, _, _, _, json, _) => _savedJson = json)
                .ReturnsAsync(new TestDataSaveResult { TestCaseDataId = 7, Version = 2 });
            _service = new TestDataService(_repository.Object, Mock.Of<ITestSuitesRepository>(), Mock.Of<IReleaseRepository>());
        }

        private void GivenStored(string? json, long version = 1) =>
            _repository
                .Setup(r => r.GetDocumentAsync(Assignment.LibraryName, Assignment.ClassName, Assignment.MethodName, UserId, EnvironmentId))
                .ReturnsAsync(json == null ? null : new TestDataDocumentRecord { FlowName = TestDataSchemas.Flow, DataJson = json, Version = version });

        private JsonObject Saved => JsonNode.Parse(_savedJson!)!.AsObject();

        private void AssertNothingSaved() =>
            _repository.Verify(r => r.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);

        [Test]
        public async Task WholeSave_ReplacesProvidedSections_AndLeavesOthersUntouched()
        {
            GivenStored("""{"Recipient":{"MedicaidBillingNumber":"111"},"Options":{"IsUrgent":true},"ProviderNotes":"old"}""");

            var (_, conversions, sections) = await _service.SaveJsonAsync(Assignment, _schema,
                """{ "Diagnoses": [ { "Sequence": 1 }, { "Sequence": 2 }, { "Sequence": "3" } ] }""", null, UserId, EnvironmentId, 1);

            Assert.That(sections, Is.EqualTo(new[] { "Diagnoses" }));
            Assert.That(conversions, Has.Count.EqualTo(1));
            Assert.That(Saved["Diagnoses"]!.AsArray(), Has.Count.EqualTo(3));
            Assert.That(Saved["Recipient"]!["MedicaidBillingNumber"]!.GetValue<string>(), Is.EqualTo("111"));
            Assert.That(Saved["Options"]!["IsUrgent"]!.GetValue<bool>(), Is.True);
            Assert.That(Saved["ProviderNotes"]!.GetValue<string>(), Is.EqualTo("old"));
        }

        [Test]
        public async Task SectionSave_ReplacesOnlyThatSection()
        {
            GivenStored("""{"Recipient":{"MedicaidBillingNumber":"111","DateOfBirth":"1990-01-01"},"Diagnoses":[{"Sequence":9}]}""");

            await _service.SaveJsonAsync(Assignment, _schema, """{ "MedicaidBillingNumber": "222" }""", "Recipient", UserId, EnvironmentId, 1);

            Assert.That(Saved["Recipient"]!.AsObject().Select(kv => kv.Key), Is.EqualTo(new[] { "MedicaidBillingNumber" }));
            Assert.That(Saved["Recipient"]!["MedicaidBillingNumber"]!.GetValue<string>(), Is.EqualTo("222"));
            Assert.That(Saved["Diagnoses"]![0]!["Sequence"]!.GetValue<int>(), Is.EqualTo(9));
        }

        [Test]
        public async Task SectionSave_ModelFields_SetsAndClearsRootFields()
        {
            GivenStored("""{"ProviderNotes":"old","Recipient":{"MedicaidBillingNumber":"111"}}""");

            await _service.SaveJsonAsync(Assignment, _schema, "{}", TestDataSchemas.Flow, UserId, EnvironmentId, 1);

            Assert.That(Saved.ContainsKey("ProviderNotes"), Is.False);
            Assert.That(Saved.ContainsKey("Recipient"), Is.True);
        }

        [Test]
        public void InvalidJson_ThrowsWithAllErrors_AndNothingIsSaved()
        {
            GivenStored("{}");

            var ex = Assert.ThrowsAsync<TestDataJsonValidationException>(() => _service.SaveJsonAsync(Assignment, _schema,
                """{ "recipient": {}, "Diagnoses": [ { "Sequence": "abc" } ] }""", null, UserId, EnvironmentId, 1));

            Assert.That(ex!.Errors, Has.Count.EqualTo(2));
            AssertNothingSaved();
        }

        [Test]
        public void StaleVersion_IsAConflict_AndNothingIsSaved()
        {
            GivenStored("{}", version: 9);

            Assert.ThrowsAsync<TestDataConflictException>(() => _service.SaveJsonAsync(Assignment, _schema,
                """{ "ProviderNotes": "x" }""", null, UserId, EnvironmentId, 5));
            AssertNothingSaved();
        }

        [Test]
        public async Task ResetSection_RemovesOnlyThatSection()
        {
            GivenStored("""{"Recipient":{"MedicaidBillingNumber":"111"},"Diagnoses":[{"Sequence":9}]}""");

            var (_, sections) = await _service.ResetAsync(Assignment, _schema, "Diagnoses", UserId, EnvironmentId, 1);

            Assert.That(sections, Is.EqualTo(new[] { "Diagnoses" }));
            Assert.That(Saved.Select(kv => kv.Key), Is.EqualTo(new[] { "Recipient" }));
        }

        [Test]
        public async Task ResetAll_RemovesEverySection()
        {
            GivenStored("""{"Recipient":{"MedicaidBillingNumber":"111"},"ProviderNotes":"x"}""");

            var (_, sections) = await _service.ResetAsync(Assignment, _schema, null, UserId, EnvironmentId, 1);

            Assert.That(sections, Is.EquivalentTo(new[] { "Recipient", TestDataSchemas.Flow }));
            Assert.That(Saved, Is.Empty);
        }

        [Test]
        public async Task EffectiveDocument_MergesSavedOverDefaults_InFullModelShape()
        {
            GivenStored("""{"Diagnoses":[{"Sequence":7,"DiagnosisCode":"A00"}],"ProviderNotes":"saved"}""", version: 4);

            var effective = await _service.GetEffectiveDocumentAsync(Assignment, _schema, UserId, EnvironmentId);
            var doc = effective.Document;

            Assert.That(effective.Version, Is.EqualTo(4));
            Assert.That(effective.SavedSections, Is.EquivalentTo(new[] { "Diagnoses", TestDataSchemas.Flow }));
            Assert.That(doc["Recipient"]!["MedicaidBillingNumber"]!.GetValue<string>(), Is.EqualTo("0005987"));
            Assert.That(doc["Diagnoses"]!.AsArray(), Has.Count.EqualTo(1));
            Assert.That(doc["Diagnoses"]![0]!.AsObject().Select(kv => kv.Key), Is.EqualTo(new[] { "Sequence", "DiagnosisCode", "DiagnosisDate" }));
            Assert.That(doc["ServiceDetails"]!.AsArray(), Is.Empty);
            Assert.That(doc["Options"]!["IsUrgent"]!.GetValue<bool>(), Is.False);
            Assert.That(doc["Options"]!["Priority"]!.GetValue<string>(), Is.EqualTo("Low"));
            Assert.That(doc["ProviderNotes"]!.GetValue<string>(), Is.EqualTo("saved"));

            var roundTrip = TestDataJsonValidator.ValidateDocument(_schema, doc.ToJsonString());
            Assert.That(roundTrip.IsValid, Is.True, string.Join(Environment.NewLine, roundTrip.Errors));
        }
    }
}
