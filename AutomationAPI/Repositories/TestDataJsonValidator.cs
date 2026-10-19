using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories
{
    public class TestDataJsonResult
    {
        public JsonObject? Document { get; set; }
        public JsonNode? Section { get; set; }
        public List<string> Errors { get; } = new();
        public List<string> Conversions { get; } = new();
        public bool IsValid => Errors.Count == 0;
    }

    public static class TestDataJsonValidator
    {
        public const int MaxErrors = 50;
        public const int MaxRows = 500;
        public const int MaxBytes = 1_048_576;

        public static TestDataJsonResult ValidateDocument(TestDataFlowSchema schema, string json)
        {
            var result = new TestDataJsonResult();
            var root = Parse(json, result);
            if (root == null) return result;

            if (root is not JsonObject obj)
            {
                result.Errors.Add($"The JSON must be an object - {{ \"SectionName\": ... }} - matching the {schema.FlowName} model.");
                return result;
            }

            var rootSection = RootSection(schema);
            var sections = schema.Sections.Where(s => s != rootSection).ToDictionary(s => s.SectionName, StringComparer.Ordinal);
            var rootFields = rootSection?.Fields.ToDictionary(f => f.Name, StringComparer.Ordinal) ?? new();
            var allowed = sections.Keys.Concat(rootFields.Keys).ToList();

            var document = new JsonObject();
            foreach (var (key, value) in obj)
            {
                if (sections.TryGetValue(key, out var section))
                {
                    var node = ValidateSectionNode(section, value, key, result);
                    if (node != null) document[key] = node;
                }
                else if (rootFields.TryGetValue(key, out var field))
                {
                    if (TryValue(field, value, key, result, out var normalized)) document[key] = normalized;
                }
                else
                {
                    AddError(result, Unknown(key, key, allowed, $"is not part of the {schema.FlowName} model"));
                }
            }

            if (result.IsValid) result.Document = document;
            return result;
        }

        public static TestDataJsonResult ValidateSection(TestDataFlowSchema schema, TestDataSectionSchema section, string json)
        {
            var result = new TestDataJsonResult();
            var root = Parse(json, result);
            if (root == null) return result;

            var node = section == RootSection(schema)
                ? ValidateRootFields(section, root, result)
                : ValidateSectionNode(section, root, section.SectionName, result);

            if (result.IsValid) result.Section = node;
            return result;
        }

        public static TestDataSectionSchema? RootSection(TestDataFlowSchema schema) =>
            schema.Sections.FirstOrDefault(s => !s.IsList && s.SectionName == schema.FlowName);

        private static JsonNode? Parse(string json, TestDataJsonResult result)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                result.Errors.Add("The JSON is empty.");
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(json);
            if (bytes.Length > MaxBytes)
            {
                result.Errors.Add($"The JSON is too large ({bytes.Length / 1024} KB). The limit is {MaxBytes / 1024} KB.");
                return null;
            }

            try
            {
                FindDuplicateKeys(bytes, result);
                if (!result.IsValid) return null;
                return JsonNode.Parse(bytes) ?? throw new JsonException("The JSON is null.");
            }
            catch (JsonException ex)
            {
                var where = ex.LineNumber.HasValue ? $" (line {ex.LineNumber + 1}, position {ex.BytePositionInLine + 1})" : "";
                result.Errors.Add($"Invalid JSON{where}: {ex.Message.Split(" Path:")[0]}");
                return null;
            }
        }

        private sealed class Frame
        {
            public HashSet<string>? Keys;
            public string Path = "";
            public string? Pending;
            public int Index;
        }

        private static void FindDuplicateKeys(byte[] utf8, TestDataJsonResult result)
        {
            var reader = new Utf8JsonReader(utf8);
            var stack = new Stack<Frame>();

            string ChildPath()
            {
                if (stack.Count == 0) return "";
                var parent = stack.Peek();
                return parent.Keys != null ? Join(parent.Path, parent.Pending ?? "") : $"{parent.Path}[{parent.Index++}]";
            }

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        stack.Push(new Frame { Keys = new HashSet<string>(StringComparer.Ordinal), Path = ChildPath() });
                        break;
                    case JsonTokenType.StartArray:
                        stack.Push(new Frame { Path = ChildPath() });
                        break;
                    case JsonTokenType.EndObject:
                    case JsonTokenType.EndArray:
                        stack.Pop();
                        break;
                    case JsonTokenType.PropertyName:
                        var top = stack.Peek();
                        var name = reader.GetString() ?? "";
                        if (!top.Keys!.Add(name))
                            AddError(result, $"{Join(top.Path, name)}: duplicate key - each key can appear only once.");
                        top.Pending = name;
                        break;
                    default:
                        ChildPath();
                        break;
                }
            }
        }

        private static JsonNode? ValidateSectionNode(TestDataSectionSchema section, JsonNode? node, string path, TestDataJsonResult result)
        {
            if (section.IsList)
            {
                if (node is not JsonArray array)
                {
                    AddError(result, $"{path}: must be an array of rows - [ {{ ... }}, {{ ... }} ] - because {section.SectionName} holds multiple rows.");
                    return null;
                }
                if (array.Count > MaxRows)
                {
                    AddError(result, $"{path}: has {array.Count} rows; the limit is {MaxRows}.");
                    return null;
                }

                var rows = new JsonArray();
                for (var i = 0; i < array.Count; i++)
                {
                    var rowPath = $"{path}[{i}]";
                    if (array[i] is not JsonObject rowObject)
                    {
                        AddError(result, $"{rowPath}: each row must be an object - {{ \"Field\": value }}.");
                        continue;
                    }
                    var row = ValidateRow(section, rowObject, rowPath, result);
                    if (row != null) rows.Add(row);
                }
                return rows;
            }

            if (node is not JsonObject obj)
            {
                AddError(result, $"{path}: must be an object - {{ \"Field\": value }}" +
                    (node is JsonArray ? $" - not an array, because {section.SectionName} holds a single row." : "."));
                return null;
            }
            return ValidateRow(section, obj, path, result);
        }

        private static JsonObject? ValidateRootFields(TestDataSectionSchema section, JsonNode? node, TestDataJsonResult result)
        {
            if (node is not JsonObject obj)
            {
                AddError(result, $"{section.SectionName}: must be an object of the model's own fields - {{ \"Field\": value }}.");
                return null;
            }
            return ValidateRow(section, obj, "", result);
        }

        private static JsonObject? ValidateRow(TestDataSectionSchema section, JsonObject obj, string path, TestDataJsonResult result)
        {
            var fields = section.Fields.ToDictionary(f => f.Name, StringComparer.Ordinal);
            var row = new JsonObject();
            foreach (var (key, value) in obj)
            {
                var fieldPath = Join(path, key);
                if (!fields.TryGetValue(key, out var field))
                {
                    AddError(result, Unknown(fieldPath, key, fields.Keys, $"is not a field of {section.SectionName}"));
                    continue;
                }
                if (TryValue(field, value, fieldPath, result, out var normalized))
                    row[key] = normalized;
            }
            return row;
        }

        private static bool TryValue(TestDataFieldSchema field, JsonNode? node, string path, TestDataJsonResult result, out JsonNode? normalized)
        {
            normalized = null;

            if (node == null)
            {
                if (field.Nullable) return true;
                AddError(result, $"{path}: null isn't allowed - this {Describe(field)} field can't be empty in the model.");
                return false;
            }
            if (node is JsonObject || node is JsonArray)
            {
                AddError(result, $"{path}: must be a single value, not {(node is JsonObject ? "an object" : "an array")}.");
                return false;
            }

            var element = node.AsValue().GetValue<JsonElement>();
            var raw = element.GetRawText();
            bool Fail(string message) { AddError(result, $"{path}: {raw} {message}"); return false; }
            JsonNode Converted(JsonNode to) { result.Conversions.Add($"{path}: {raw} → {to.ToJsonString()}"); return to; }

            switch (field.Type)
            {
                case "integer":
                    if (element.ValueKind == JsonValueKind.Number)
                    {
                        if (!element.TryGetInt64(out var n)) return Fail($"is not a valid whole number{Range(field)}.");
                        if (!InRange(field, n)) return Fail($"is out of range{Range(field)}.");
                        normalized = JsonValue.Create(n);
                        return true;
                    }
                    if (element.ValueKind == JsonValueKind.String &&
                        long.TryParse(element.GetString(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed))
                    {
                        if (!InRange(field, parsed)) return Fail($"is out of range{Range(field)}.");
                        normalized = Converted(JsonValue.Create(parsed));
                        return true;
                    }
                    return Fail($"is not a valid whole number{Range(field)}.");

                case "decimal":
                    if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var d))
                    {
                        normalized = JsonValue.Create(d);
                        return true;
                    }
                    if (element.ValueKind == JsonValueKind.String &&
                        decimal.TryParse(element.GetString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var pd))
                    {
                        normalized = Converted(JsonValue.Create(pd));
                        return true;
                    }
                    return Fail("is not a valid number (use a dot for decimals, e.g. 150.50).");

                case "date":
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        var s = element.GetString() ?? "";
                        if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        {
                            normalized = JsonValue.Create(s);
                            return true;
                        }
                        if (DateTime.TryParseExact(s, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        {
                            normalized = JsonValue.Create(dt.TimeOfDay == TimeSpan.Zero
                                ? dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                                : dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
                            return true;
                        }
                    }
                    return Fail("is not a valid date - use \"yyyy-MM-dd\" (e.g. \"1985-06-12\").");

                case "boolean":
                    if (element.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    {
                        normalized = JsonValue.Create(element.GetBoolean());
                        return true;
                    }
                    if (element.ValueKind == JsonValueKind.String && bool.TryParse(element.GetString(), out var b))
                    {
                        normalized = Converted(JsonValue.Create(b));
                        return true;
                    }
                    return Fail("is not valid - use true or false.");

                case "enum":
                    var options = field.Options ?? new List<string>();
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        var s = element.GetString() ?? "";
                        if (options.Contains(s, StringComparer.Ordinal))
                        {
                            normalized = JsonValue.Create(s);
                            return true;
                        }
                        var match = options.FirstOrDefault(o => o.Equals(s, StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                        {
                            normalized = Converted(JsonValue.Create(match));
                            return true;
                        }
                    }
                    return Fail($"is not one of: {string.Join(", ", options)}.");

                default:
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.String:
                            normalized = JsonValue.Create(element.GetString());
                            return true;
                        case JsonValueKind.Number:
                            normalized = Converted(JsonValue.Create(raw));
                            return true;
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            normalized = Converted(JsonValue.Create(element.GetBoolean() ? "true" : "false"));
                            return true;
                        default:
                            return Fail("is not valid text.");
                    }
            }
        }

        private static bool InRange(TestDataFieldSchema field, long value) =>
            (!field.Min.HasValue || value >= field.Min.Value) && (!field.Max.HasValue || value <= field.Max.Value);

        private static string Range(TestDataFieldSchema field) =>
            field.Min.HasValue && field.Max.HasValue ? $" (allowed {field.Min}..{field.Max})" : "";

        private static string Describe(TestDataFieldSchema field) => field.Type switch
        {
            "integer" => "whole-number",
            "decimal" => "number",
            "boolean" => "true/false",
            _ => field.Type
        };

        private static string Unknown(string path, string key, IEnumerable<string> allowed, string message)
        {
            var hint = allowed.FirstOrDefault(a => a.Equals(key, StringComparison.OrdinalIgnoreCase));
            return hint != null
                ? $"{path}: \"{key}\" {message} - did you mean \"{hint}\"? (names are case-sensitive)"
                : $"{path}: \"{key}\" {message}.";
        }

        private static string Join(string path, string name) => string.IsNullOrEmpty(path) ? name : $"{path}.{name}";

        private static void AddError(TestDataJsonResult result, string message)
        {
            if (result.Errors.Count < MaxErrors) result.Errors.Add(message);
            else if (result.Errors.Count == MaxErrors) result.Errors.Add($"... more problems not shown (first {MaxErrors} listed).");
        }
    }
}
