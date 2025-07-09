using System.Text.Json;
using System.Text;

namespace AutomationAPI.Repositories.Helpers
{
    public class AutomationDataHelper
    {
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
