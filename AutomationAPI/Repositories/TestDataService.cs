using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.TestRunner;

namespace AutomationAPI.Repositories
{
    public class TestDataValidationException : Exception
    {
        public TestDataValidationException(string message) : base(message) { }
    }

    public class TestDataConflictException : Exception
    {
        public TestDataConflictException(string message) : base(message) { }
    }

    public class TestDataService
    {
        private readonly ITestDataRepository _repository;
        private readonly ITestSuitesRepository _testSuites;
        private readonly IReleaseRepository _releases;

        public TestDataService(ITestDataRepository repository, ITestSuitesRepository testSuites, IReleaseRepository releases)
        {
            _repository = repository;
            _testSuites = testSuites;
            _releases = releases;
        }

        public async Task<List<TestDataTestCase>> GetMyTestCasesAsync(int userId, int environmentId)
        {
            var testCases = (await _repository.GetMyTestCasesAsync(userId, environmentId)).ToList();

            foreach (var group in testCases.Where(t => t.ReleaseId.HasValue).GroupBy(t => t.ReleaseId!.Value))
            {
                var folder = (await _releases.GetByIdAsync(group.Key))?.ReleaseFolderPath;
                if (string.IsNullOrWhiteSpace(folder)) continue;

                var libraries = await _testSuites.GetLibrariesAsync(folder);
                foreach (var testCase in group)
                {
                    var flow = FindTestDataFlow(libraries, testCase.LibraryName, testCase.ClassName, testCase.MethodName);
                    testCase.FlowName = string.IsNullOrEmpty(flow) ? null : TestDataSchemaReader.FlowNameOf(flow);
                }
            }

            return testCases;
        }

        public async Task<TestDataFlowSchema?> GetSchemaAsync(TestDataAssignment assignment)
        {
            if (string.IsNullOrWhiteSpace(assignment.ReleaseFolderPath))
                throw new TestDataValidationException("The test case's release has no folder configured.");

            var libraries = await _testSuites.GetLibrariesAsync(assignment.ReleaseFolderPath);
            var testDataFlow = FindTestDataFlow(libraries, assignment.LibraryName, assignment.ClassName, assignment.MethodName);
            if (string.IsNullOrEmpty(testDataFlow))
                return null;

            var schema = TestDataSchemaReader.Read(assignment.ReleaseFolderPath, testDataFlow);

            var synced = (await _repository.UpsertModelSectionsAsync(schema.FlowName, schema.Sections.Select(s => s.SectionName)))
                .Where(s => s.SectionId.HasValue && s.SectionName != null)
                .ToDictionary(s => s.SectionName!, s => s.SectionId!.Value, StringComparer.Ordinal);

            return new TestDataFlowSchema
            {
                FlowName = schema.FlowName,
                ModelType = schema.ModelType,
                Warnings = schema.Warnings.ToList(),
                Sections = schema.Sections.Select(s => new TestDataSectionSchema
                {
                    SectionId = synced.TryGetValue(s.SectionName, out var id) ? id : 0,
                    SectionName = s.SectionName,
                    IsList = s.IsList,
                    Fields = s.Fields,
                    DefaultRows = s.DefaultRows
                }).ToList()
            };
        }

        public async Task<TestDataDocumentView> GetDataAsync(string libraryName, string className, string methodName,
            TestDataFlowSchema schema, int userId, int environmentId)
        {
            var record = await _repository.GetDocumentAsync(libraryName, className, methodName, userId, environmentId);
            var view = new TestDataDocumentView { Version = record?.Version, ModifiedOn = record?.ModifiedOn };
            if (record == null || record.FlowName != schema.FlowName)
                return view;

            var document = ParseDocument(record.DataJson);
            foreach (var section in schema.Sections)
            {
                var rows = ReadSection(schema, section, document);
                if (rows != null)
                    view.Sections.Add(new TestDataSectionRows { SectionId = section.SectionId, SectionName = section.SectionName, Rows = rows });
            }
            return view;
        }

        public async Task<string> GetRuntimeDocumentAsync(TestDataAssignment assignment, string flowName)
        {
            var record = await _repository.GetDocumentAsync(assignment.LibraryName, assignment.ClassName, assignment.MethodName,
                assignment.AssignedUser, assignment.EnvironmentId!.Value);
            return record != null && record.FlowName == flowName ? record.DataJson : "{}";
        }

        public async Task<(TestDataSaveResult Result, List<Dictionary<string, string?>> SavedRows)> SaveSectionAsync(
            TestDataAssignment assignment, TestDataFlowSchema schema, TestDataSectionSchema section,
            List<Dictionary<string, string?>> rows, int userId, int environmentId, long? expectedVersion)
        {
            var record = await _repository.GetDocumentAsync(assignment.LibraryName, assignment.ClassName, assignment.MethodName, userId, environmentId);
            if (record?.Version != expectedVersion)
                throw new TestDataConflictException(ConflictMessage);

            var document = record == null || record.FlowName != schema.FlowName ? new JsonObject() : ParseDocument(record.DataJson);
            var cleanRows = ValidateRows(section, rows);

            if (IsRootSection(schema, section))
            {
                var values = cleanRows.FirstOrDefault() ?? new Dictionary<string, JsonNode?>();
                foreach (var field in section.Fields)
                    document[field.Name] = values.TryGetValue(field.Name, out var v) ? v : null;
            }
            else if (section.IsList)
            {
                var array = new JsonArray();
                foreach (var row in cleanRows)
                    array.Add(ToObject(row));
                document[section.SectionName] = array;
            }
            else
            {
                document[section.SectionName] = ToObject(cleanRows.FirstOrDefault() ?? new Dictionary<string, JsonNode?>());
            }

            var result = await _repository.SaveDocumentAsync(assignment.LibraryName, assignment.ClassName, assignment.MethodName,
                userId, environmentId, schema.FlowName, document.ToJsonString(), record?.Version);
            if (result.IsConflict)
                throw new TestDataConflictException(ConflictMessage);

            var savedRows = cleanRows
                .Select(r => r.ToDictionary(kv => kv.Key, kv => ToText(kv.Value), StringComparer.Ordinal))
                .ToList();
            return (result, savedRows);
        }

        public async Task<(TestDataSaveResult Result, List<string> CopiedSections)> CopyAllAsync(
            TestDataAssignment target, TestDataFlowSchema schema,
            string sourceLibrary, string sourceClass, string sourceMethod, int sourceEnvironmentId,
            int targetEnvironmentId, int userId, long? expectedVersion)
        {
            var source = await _repository.GetDocumentAsync(sourceLibrary, sourceClass, sourceMethod, userId, sourceEnvironmentId);
            if (source == null || source.FlowName != schema.FlowName)
                throw new TestDataValidationException("There's no saved test data to copy from there.");

            var targetRecord = await _repository.GetDocumentAsync(target.LibraryName, target.ClassName, target.MethodName, userId, targetEnvironmentId);
            if (targetRecord?.Version != expectedVersion)
                throw new TestDataConflictException(ConflictMessage);

            var sourceDocument = ParseDocument(source.DataJson);
            var copy = new JsonObject();
            var copied = new List<string>();
            foreach (var section in schema.Sections)
            {
                var keys = IsRootSection(schema, section)
                    ? section.Fields.Select(f => f.Name)
                    : new[] { section.SectionName };
                var any = false;
                foreach (var key in keys)
                {
                    if (!sourceDocument.TryGetPropertyValue(key, out var node)) continue;
                    copy[key] = node?.DeepClone();
                    any = true;
                }
                if (any) copied.Add(section.SectionName);
            }

            if (copied.Count == 0)
                throw new TestDataValidationException("There's no saved test data to copy from there.");

            var result = await _repository.SaveDocumentAsync(target.LibraryName, target.ClassName, target.MethodName,
                userId, targetEnvironmentId, schema.FlowName, copy.ToJsonString(), targetRecord?.Version);
            if (result.IsConflict)
                throw new TestDataConflictException(ConflictMessage);

            return (result, copied);
        }

        private const string ConflictMessage =
            "This test case's data was changed somewhere else (another tab or user) since you opened it. Reload to get the latest, then save again.";

        private static bool IsRootSection(TestDataFlowSchema schema, TestDataSectionSchema section) =>
            !section.IsList && section.SectionName == schema.FlowName;

        private static List<Dictionary<string, string?>>? ReadSection(TestDataFlowSchema schema, TestDataSectionSchema section, JsonObject document)
        {
            if (IsRootSection(schema, section))
            {
                var present = section.Fields.Where(f => document.ContainsKey(f.Name)).ToList();
                if (present.Count == 0) return null;
                return new() { present.ToDictionary(f => f.Name, f => ToText(document[f.Name]), StringComparer.Ordinal) };
            }

            if (!document.TryGetPropertyValue(section.SectionName, out var node) || node == null)
                return null;

            if (section.IsList)
                return node is JsonArray array
                    ? array.OfType<JsonObject>().Select(ToRow).ToList()
                    : null;

            return node is JsonObject obj ? new() { ToRow(obj) } : null;
        }

        private static Dictionary<string, string?> ToRow(JsonObject obj) =>
            obj.ToDictionary(kv => kv.Key, kv => ToText(kv.Value), StringComparer.Ordinal);

        private static string? ToText(JsonNode? node)
        {
            if (node == null) return null;
            if (node is JsonValue value && value.TryGetValue<string>(out var s)) return s;
            return node.ToJsonString();
        }

        private static JsonObject ToObject(Dictionary<string, JsonNode?> row)
        {
            var obj = new JsonObject();
            foreach (var (name, value) in row)
                if (value != null) obj[name] = value;
            return obj;
        }

        private static JsonObject ParseDocument(string? json)
        {
            try
            {
                return JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json) as JsonObject ?? new JsonObject();
            }
            catch (JsonException)
            {
                return new JsonObject();
            }
        }

        private static List<Dictionary<string, JsonNode?>> ValidateRows(TestDataSectionSchema section, List<Dictionary<string, string?>> rows)
        {
            var fields = section.Fields.ToDictionary(f => f.Name, StringComparer.Ordinal);
            var cleanRows = (rows ?? new())
                .Select(r => r ?? new Dictionary<string, string?>())
                .Where(r => r.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
                .ToList();

            if (!section.IsList && cleanRows.Count > 1)
                throw new TestDataValidationException($"{section.SectionName} holds a single row, but {cleanRows.Count} were sent.");

            var result = new List<Dictionary<string, JsonNode?>>();
            for (var i = 0; i < cleanRows.Count; i++)
            {
                var row = new Dictionary<string, JsonNode?>(StringComparer.Ordinal);
                foreach (var (name, raw) in cleanRows[i])
                {
                    if (!fields.TryGetValue(name, out var field))
                        throw new TestDataValidationException($"{section.SectionName} has no field named \"{name}\" in the model.");

                    var value = ToTypedValue(field, raw, section.SectionName, section.IsList ? i + 1 : null);
                    if (value != null) row[name] = value;
                }
                result.Add(row);
            }
            return result;
        }

        private static JsonNode? ToTypedValue(TestDataFieldSchema field, string? raw, string sectionName, int? row)
        {
            var value = (raw ?? "").Trim();
            if (value.Length == 0) return null;

            JsonNode Fail(string expected) => throw new TestDataValidationException(
                $"{sectionName}{(row.HasValue ? $" row {row}" : "")}.{field.Name}: \"{value}\" is not a valid {expected}.");

            switch (field.Type)
            {
                case "integer":
                    return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) ? JsonValue.Create(l) : Fail("whole number");
                case "decimal":
                    return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? JsonValue.Create(d) : Fail("number");
                case "date":
                    if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return Fail("date");
                    return JsonValue.Create(dt.TimeOfDay == TimeSpan.Zero
                        ? dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                        : dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
                case "boolean":
                    if (value is "1" || value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase)) return JsonValue.Create(true);
                    if (value is "0" || value.Equals("no", StringComparison.OrdinalIgnoreCase) || value.Equals("false", StringComparison.OrdinalIgnoreCase)) return JsonValue.Create(false);
                    return Fail("yes/no value");
                case "enum":
                    var match = field.Options?.FirstOrDefault(o => o.Equals(value, StringComparison.OrdinalIgnoreCase));
                    return match != null ? JsonValue.Create(match) : Fail($"choice ({string.Join(", ", field.Options ?? new())})");
                default:
                    return JsonValue.Create(value);
            }
        }

        private static string? FindTestDataFlow(IEnumerable<LibraryInfo> libraries, string libraryName, string className, string methodName)
        {
            return libraries
                .Where(l => l.LibraryName.Equals(libraryName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(l => l.Classes)
                .Where(c => c.ClassName == className)
                .SelectMany(c => c.Methods)
                .FirstOrDefault(m => m.MethodName == methodName && !string.IsNullOrEmpty(m.TestDataFlow))
                ?.TestDataFlow;
        }
    }
}
