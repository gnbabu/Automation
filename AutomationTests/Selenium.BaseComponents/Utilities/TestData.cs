using System.Collections;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Selenium.BaseComponents.Utilities
{
    public static class TestData
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Culture = CultureInfo.InvariantCulture,
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };

        public static T Load<T>() where T : new()
        {
            var flowName = typeof(T).Name;

            if (!int.TryParse(NUnit.Framework.TestContext.Parameters["AssignmentTestCaseId"], out var assignmentTestCaseId)
                || assignmentTestCaseId <= 0)
            {
                throw new InvalidOperationException(
                    $"Test data for '{flowName}' is configured per test case. Run this test from the Portal " +
                    "(Run Now/Schedule) so it knows which test case's data to load.");
            }

            var apiGateway = new APIGatway();
            var json = apiGateway.GetTestCaseData(flowName, assignmentTestCaseId).Result
                ?? throw new InvalidOperationException($"Could not load test data for '{flowName}' from the Automation API.");

            var model = new T();
            try
            {
                JsonConvert.PopulateObject(string.IsNullOrWhiteSpace(json) ? "{}" : json, model, Settings);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Test data for '{flowName}' doesn't match the model: {ex.Message}", ex);
            }

            LogLoaded(apiGateway, model, json, flowName, assignmentTestCaseId);
            return model;
        }

        private static void LogLoaded(APIGatway apiGateway, object model, string json, string flowName, int assignmentTestCaseId)
        {
            var saved = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                if (JToken.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json) is JObject document)
                    foreach (var property in document.Properties())
                        saved.Add(property.Name);
            }
            catch (JsonException)
            {
            }

            var loaded = new List<string>();
            var defaults = new List<string>();
            var hasOwnFields = false;
            var ownFieldSaved = false;

            foreach (var property in model.GetType().GetProperties().Where(p => p.CanWrite))
            {
                if (IsScalar(property.PropertyType))
                {
                    hasOwnFields = true;
                    ownFieldSaved |= saved.Contains(property.Name);
                    continue;
                }

                var label = property.GetValue(model) is ICollection items ? $"{property.Name} ({items.Count} rows)" : property.Name;
                (saved.Contains(property.Name) ? loaded : defaults).Add(label);
            }

            if (hasOwnFields)
                (ownFieldSaved ? loaded : defaults).Add(flowName);

            var message = loaded.Count > 0
                ? $"Test data for flow \"{flowName}\": saved for this test case - {string.Join(", ", loaded)}"
                : $"Test data for flow \"{flowName}\": nothing saved for this test case";
            if (defaults.Count > 0)
                message += $". Using the model's default values: {string.Join(", ", defaults)}";

            int.TryParse(NUnit.Framework.TestContext.Parameters["AssignmentId"], out var assignmentId);
            var test = NUnit.Framework.TestContext.CurrentContext.Test;
            apiGateway.SaveTestCaseLog(new TestCaseExecutionLog
            {
                AssignmentId = assignmentId,
                AssignmentTestCaseId = assignmentTestCaseId,
                TestCaseId = test.Properties.Get("TestCaseId") as string ?? test.MethodName ?? test.Name,
                TestCaseDescription = test.Properties.Get("Description") as string ?? test.Name,
                StepName = "Test Data",
                LogMessage = message,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running
            });
        }

        private static bool IsScalar(Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal)
                || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid);
        }
    }
}
