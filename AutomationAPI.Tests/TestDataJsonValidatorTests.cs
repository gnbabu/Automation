using AutomationAPI.Repositories;
using NUnit.Framework;

namespace AutomationAPI.Tests
{
    [TestFixture]
    public class TestDataJsonValidatorTests
    {
        private static TestDataJsonResult Doc(string json) =>
            TestDataJsonValidator.ValidateDocument(TestDataSchemas.SubmitPA(), json);

        private static TestDataJsonResult Section(string sectionName, string json)
        {
            var schema = TestDataSchemas.SubmitPA();
            return TestDataJsonValidator.ValidateSection(schema, schema.Sections.Single(s => s.SectionName == sectionName), json);
        }

        private static void AssertError(TestDataJsonResult result, string expectedFragment)
        {
            Assert.That(result.IsValid, Is.False, "expected a validation error");
            Assert.That(result.Errors, Has.Some.Contains(expectedFragment), string.Join(Environment.NewLine, result.Errors));
            Assert.That(result.Document, Is.Null);
            Assert.That(result.Section, Is.Null);
        }

        [Test]
        public void ValidDocument_IsNormalizedAndTyped()
        {
            var result = Doc("""
                {
                  "Recipient": { "MedicaidBillingNumber": "0005987", "DateOfBirth": "1985-06-12" },
                  "Diagnoses": [ { "Sequence": 1, "DiagnosisCode": "Z00.00", "DiagnosisDate": "2026-09-01T00:00:00" } ],
                  "ServiceDetails": [ { "LineNumber": 1, "RequestedUnits": 4, "RequestedDollars": 150.50 } ],
                  "Options": { "IsUrgent": true, "Priority": "High" },
                  "ProviderNotes": null
                }
                """);

            Assert.That(result.IsValid, Is.True, string.Join(Environment.NewLine, result.Errors));
            Assert.That(result.Conversions, Is.Empty);
            var doc = result.Document!;
            Assert.That(doc["Diagnoses"]![0]!["DiagnosisDate"]!.GetValue<string>(), Is.EqualTo("2026-09-01"));
            Assert.That(doc["ServiceDetails"]![0]!["RequestedDollars"]!.GetValue<decimal>(), Is.EqualTo(150.50m));
            Assert.That(doc.ContainsKey("ProviderNotes") && doc["ProviderNotes"] == null, Is.True);
        }

        [Test]
        public void NotAnObject_IsRejected() => AssertError(Doc("[1,2]"), "must be an object");

        [Test]
        public void InvalidJson_IsRejectedWithPosition() => AssertError(Doc("{ \"Recipient\": { "), "Invalid JSON");

        [Test]
        public void UnknownSection_IsRejected() => AssertError(Doc("""{ "OldSection": {} }"""), "\"OldSection\" is not part of the SubmitPAProfessional model");

        [Test]
        public void UnknownField_IsRejectedWithPath() =>
            AssertError(Doc("""{ "Recipient": { "Nickname": "x" } }"""), "Recipient.Nickname: \"Nickname\" is not a field of Recipient");

        [Test]
        public void CaseMismatch_IsRejectedWithHint() =>
            AssertError(Doc("""{ "recipient": {} }"""), "did you mean \"Recipient\"");

        [Test]
        public void FieldCaseMismatch_IsRejectedWithHint() =>
            AssertError(Doc("""{ "Recipient": { "medicaidBillingNumber": "1" } }"""), "did you mean \"MedicaidBillingNumber\"");

        [Test]
        public void ObjectForListSection_IsRejected() =>
            AssertError(Doc("""{ "Diagnoses": { "Sequence": 1 } }"""), "Diagnoses: must be an array of rows");

        [Test]
        public void ArrayForClassSection_IsRejected() =>
            AssertError(Doc("""{ "Recipient": [ {} ] }"""), "Recipient: must be an object");

        [Test]
        public void NullRow_IsRejected() =>
            AssertError(Doc("""{ "Diagnoses": [ { "Sequence": 1 }, null ] }"""), "Diagnoses[1]: each row must be an object");

        [Test]
        public void TooManyRows_IsRejected()
        {
            var rows = string.Join(",", Enumerable.Range(1, TestDataJsonValidator.MaxRows + 1).Select(i => $"{{\"Sequence\":{i}}}"));
            AssertError(Doc($$"""{ "Diagnoses": [ {{rows}} ] }"""), "the limit is 500");
        }

        [Test]
        public void ObjectAsFieldValue_IsRejected() =>
            AssertError(Doc("""{ "Recipient": { "MedicaidBillingNumber": { "x": 1 } } }"""), "must be a single value, not an object");

        [Test]
        public void DuplicateKey_IsRejected() =>
            AssertError(Doc("""{ "Recipient": { "MedicaidBillingNumber": "1", "MedicaidBillingNumber": "2" } }"""), "Recipient.MedicaidBillingNumber: duplicate key");

        [Test]
        public void TooLarge_IsRejected() =>
            AssertError(Doc("{\"ProviderNotes\":\"" + new string('x', TestDataJsonValidator.MaxBytes) + "\"}"), "too large");

        [Test]
        public void NullForNonNullableField_IsRejected() =>
            AssertError(Doc("""{ "Diagnoses": [ { "Sequence": null } ] }"""), "Diagnoses[0].Sequence: null isn't allowed");

        [TestCase("\"abc\"", "is not a valid whole number")]
        [TestCase("5.5", "is not a valid whole number")]
        [TestCase("\"5.5\"", "is not a valid whole number")]
        [TestCase("true", "is not a valid whole number")]
        public void InvalidInteger_IsRejected(string value, string message) =>
            AssertError(Doc($$"""{ "Diagnoses": [ { "Sequence": {{value}} } ] }"""), $"Diagnoses[0].Sequence: {value} {message}");

        [Test]
        public void IntegerOutOfRange_IsRejected() =>
            AssertError(Doc("""{ "ServiceDetails": [ { "RequestedUnits": 40000 } ] }"""), "is out of range (allowed -32768..32767)");

        [TestCase("\"abc\"")]
        [TestCase("\"1,50\"")]
        [TestCase("false")]
        public void InvalidDecimal_IsRejected(string value) =>
            AssertError(Doc($$"""{ "ServiceDetails": [ { "RequestedDollars": {{value}} } ] }"""), "is not a valid number");

        [TestCase("\"06/12/1985\"")]
        [TestCase("\"1985-13-01\"")]
        [TestCase("19850612")]
        public void InvalidDate_IsRejected(string value) =>
            AssertError(Doc($$"""{ "Recipient": { "DateOfBirth": {{value}} } }"""), "is not a valid date");

        [TestCase("\"yes\"")]
        [TestCase("1")]
        [TestCase("\"1\"")]
        public void InvalidBoolean_IsRejected(string value) =>
            AssertError(Doc($$"""{ "Options": { "IsUrgent": {{value}} } }"""), "use true or false");

        [TestCase("\"Medium\"")]
        [TestCase("1")]
        public void InvalidEnum_IsRejected(string value) =>
            AssertError(Doc($$"""{ "Options": { "Priority": {{value}} } }"""), "is not one of: Low, High");

        [Test]
        public void TextGivenAnObjectOrArray_IsRejected() =>
            AssertError(Doc("""{ "ProviderNotes": [ "a" ] }"""), "ProviderNotes: must be a single value, not an array");

        [Test]
        public void UnambiguousValues_AreConvertedAndReported()
        {
            var result = Doc("""
                {
                  "Recipient": { "MedicaidBillingNumber": 5987 },
                  "ServiceDetails": [ { "RequestedUnits": "4", "RequestedDollars": "150.50" } ],
                  "Options": { "IsUrgent": "TRUE", "Priority": "high" }
                }
                """);

            Assert.That(result.IsValid, Is.True, string.Join(Environment.NewLine, result.Errors));
            var doc = result.Document!;
            Assert.That(doc["Recipient"]!["MedicaidBillingNumber"]!.GetValue<string>(), Is.EqualTo("5987"));
            Assert.That(doc["ServiceDetails"]![0]!["RequestedUnits"]!.GetValue<long>(), Is.EqualTo(4));
            Assert.That(doc["ServiceDetails"]![0]!["RequestedDollars"]!.GetValue<decimal>(), Is.EqualTo(150.50m));
            Assert.That(doc["Options"]!["IsUrgent"]!.GetValue<bool>(), Is.True);
            Assert.That(doc["Options"]!["Priority"]!.GetValue<string>(), Is.EqualTo("High"));
            Assert.That(result.Conversions, Has.Count.EqualTo(5));
            Assert.That(result.Conversions, Has.Some.StartsWith("ServiceDetails[0].RequestedUnits: \"4\""));
        }

        [Test]
        public void MultipleErrors_AreAllCollectedWithPaths()
        {
            var result = Doc("""
                { "Recipient": { "DateOfBirth": "06/12/1985", "Nickname": "x" },
                  "Diagnoses": [ { "Sequence": "abc" } ],
                  "Unknown": 1 }
                """);

            Assert.That(result.Errors, Has.Count.EqualTo(4));
            Assert.That(result.Errors, Has.Some.StartsWith("Recipient.DateOfBirth:"));
            Assert.That(result.Errors, Has.Some.StartsWith("Recipient.Nickname:"));
            Assert.That(result.Errors, Has.Some.StartsWith("Diagnoses[0].Sequence:"));
            Assert.That(result.Errors, Has.Some.StartsWith("Unknown:"));
        }

        [Test]
        public void Section_ListSection_ValidatesArray()
        {
            var ok = Section("Diagnoses", """[ { "Sequence": 1 }, { "Sequence": 2 } ]""");
            Assert.That(ok.IsValid, Is.True);
            Assert.That(ok.Section!.AsArray(), Has.Count.EqualTo(2));

            AssertError(Section("Diagnoses", """{ "Sequence": 1 }"""), "Diagnoses: must be an array of rows");
        }

        [Test]
        public void Section_ModelFields_ValidatesObjectOfRootFields()
        {
            Assert.That(Section(TestDataSchemas.Flow, """{ "ProviderNotes": "x" }""").IsValid, Is.True);
            AssertError(Section(TestDataSchemas.Flow, """{ "Recipient": {} }"""), "\"Recipient\" is not a field of SubmitPAProfessional");
        }
    }
}
