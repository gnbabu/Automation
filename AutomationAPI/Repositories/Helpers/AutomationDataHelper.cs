using System.Text.Json;
using System.Text;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Helpers
{
    public class AutomationDataHelper
    {
        // Parses TestContent's raw JSON array (e.g. `[{"FieldName":"RegID","FieldValue":
        // "0005987"}, ...]`) into the shape GetAutomationDataByFlowNameAsync's real
        // consumers (every TC.* Selenium test project's own DataRepository, via
        // Mapper.BindData<T>(data.automationContents)) actually need on the wire - see
        // AutomationData.AutomationContents for why this exists (was never populated at
        // all before, always null, causing a real "Value cannot be null. (Parameter
        // 'source')" the first time this path was actually exercised end-to-end).
        // Deliberately tolerant of null/empty/malformed content (returns an empty list
        // rather than throwing) - a Section simply having no configured data yet for a
        // given flow/user/environment combination is a normal, expected state, not an
        // error condition.
        public static List<AutomationContentItem> ParseAutomationContents(string? jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return new List<AutomationContentItem>();

            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonString);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    return new List<AutomationContentItem>();

                var result = new List<AutomationContentItem>();
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("FieldName", out var fieldNameProp) &&
                        element.TryGetProperty("FieldValue", out var fieldValueProp))
                    {
                        result.Add(new AutomationContentItem
                        {
                            FieldName = fieldNameProp.GetString(),
                            FieldValue = fieldValueProp.ValueKind == JsonValueKind.String
                                ? fieldValueProp.GetString()
                                : fieldValueProp.GetRawText(),
                        });
                    }
                }
                return result;
            }
            catch (JsonException)
            {
                return new List<AutomationContentItem>();
            }
        }

        // Converts JSON string to: FieldName | FieldValue format
        public static string BuildFieldSummary(string jsonString)
        {
            var sb = new StringBuilder();

            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
                    {
                        sb.AppendLine($"{prop.Name} | {prop.Value}");
                    }
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        if (element.TryGetProperty("FieldName", out var fieldNameProp) &&
                            element.TryGetProperty("FieldValue", out var fieldValueProp))
                        {
                            sb.AppendLine($"{fieldNameProp} | {fieldValueProp}");
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unsupported JSON format.");
                }
            }

            return sb.ToString();
        }


        // Converts FieldName | FieldValue formatted string back to JSON string
        public static string ConvertToJson(string input)
        {
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<Dictionary<string, string>>();

            foreach (var line in lines)
            {
                var parts = line.Split('|', 2);
                if (parts.Length == 2)
                {
                    var item = new Dictionary<string, string>
                    {
                        ["FieldName"] = parts[0].Trim(),
                        ["FieldValue"] = parts[1].Trim()
                    };

                    list.Add(item);
                }
            }

            return JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

    }
}
