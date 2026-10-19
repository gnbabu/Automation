using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Tests
{
    internal static class TestDataSchemas
    {
        public const string Flow = "SubmitPAProfessional";

        private static TestDataFieldSchema Text(string name) => new() { Name = name, Type = "text", Nullable = true };
        private static TestDataFieldSchema Int(string name) => new() { Name = name, Type = "integer", Min = int.MinValue, Max = int.MaxValue };
        private static TestDataFieldSchema Short(string name) => new() { Name = name, Type = "integer", Min = short.MinValue, Max = short.MaxValue };
        private static TestDataFieldSchema Dec(string name) => new() { Name = name, Type = "decimal" };
        private static TestDataFieldSchema Date(string name) => new() { Name = name, Type = "date", Nullable = true };

        public static TestDataFlowSchema SubmitPA() => new()
        {
            FlowName = Flow,
            Sections =
            {
                new TestDataSectionSchema
                {
                    SectionId = 56, SectionName = "Recipient",
                    Fields = { Text("MedicaidBillingNumber"), Date("DateOfBirth") },
                    DefaultRows = { new() { ["MedicaidBillingNumber"] = "0005987", ["DateOfBirth"] = "1985-06-12" } }
                },
                new TestDataSectionSchema
                {
                    SectionId = 52, SectionName = "Diagnoses", IsList = true,
                    Fields = { Int("Sequence"), Text("DiagnosisCode"), Date("DiagnosisDate") },
                    DefaultRows =
                    {
                        new() { ["Sequence"] = "1", ["DiagnosisCode"] = "Z00.00" },
                        new() { ["Sequence"] = "2", ["DiagnosisCode"] = "E11.9" }
                    }
                },
                new TestDataSectionSchema
                {
                    SectionId = 59, SectionName = "ServiceDetails", IsList = true,
                    Fields = { Int("LineNumber"), Short("RequestedUnits"), Dec("RequestedDollars") }
                },
                new TestDataSectionSchema
                {
                    SectionId = 70, SectionName = "Options",
                    Fields =
                    {
                        new TestDataFieldSchema { Name = "IsUrgent", Type = "boolean" },
                        new TestDataFieldSchema { Name = "Priority", Type = "enum", Options = new() { "Low", "High" } }
                    }
                },
                new TestDataSectionSchema
                {
                    SectionId = 62, SectionName = Flow,
                    Fields = { Text("ProviderNotes") },
                    DefaultRows = { new() { ["ProviderNotes"] = "Follow-up" } }
                }
            }
        };
    }
}
