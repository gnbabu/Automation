using Newtonsoft.Json;

using System.Net.Http.Headers;
using System.Text;

namespace Selenium.BaseComponents.Utilities
{
    public class APIGatway
    {
        private SettingsReader _settingReader;
        public APIGatway()
        {
            _settingReader = new SettingsReader();
        }

        // Short-lived JWT minted by AutomationAPI's ServiceTokenGenerator and threaded in
        // via NUnit TestParameters (the same mechanism Browser already uses) - read here
        // so calls to [Authorize]-protected endpoints (e.g. GetAutomationData below) don't
        // 401. Absent when running outside the queue-driven pipeline (e.g. locally via
        // Test Explorer), in which case those calls simply go out unauthenticated, same as
        // before this existed.
        private static string? AccessToken => NUnit.Framework.TestContext.Parameters["AccessToken"];

        private static void AttachAuthIfAvailable(HttpClient httpClient)
        {
            if (!string.IsNullOrWhiteSpace(AccessToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        }

        public void InvokeServicePost(List<TestResults> testResults)
        {
            try
            {
                string apiUrl = _settingReader.GetSetting("AppSettings:AutomationAPI");
                string message = JsonConvert.SerializeObject(testResults);

                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(message, Encoding.UTF8, "application/json");

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}api/TestResults/bulk-insert");

                    requestMessage.Content = content;

                    HttpResponseMessage httpResponseMessage = httpClient.SendAsync(requestMessage).Result;

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        string result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveTestCaseLog(TestCaseExecutionLog testCaseExecutionLog)
        {
            try
            {
                string apiUrl = _settingReader.GetSetting("AppSettings:AutomationAPI");
                string message = JsonConvert.SerializeObject(testCaseExecutionLog);

                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(message, Encoding.UTF8, "application/json");

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}api/TestCaseExecutionLogs");

                    requestMessage.Content = content;

                    HttpResponseMessage httpResponseMessage = httpClient.SendAsync(requestMessage).Result;

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        string result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    }
                }
                ;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void SaveMethodScreenShots(List<TestScreenshot> screenshots)
        {
            try
            {
                string apiUrl = _settingReader.GetSetting("AppSettings:AutomationAPI");
                string message = JsonConvert.SerializeObject(screenshots);

                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(message, Encoding.UTF8, "application/json");

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}api/TestScreenshots/bulk");

                    requestMessage.Content = content;

                    HttpResponseMessage httpResponseMessage = httpClient.SendAsync(requestMessage).Result;

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        string result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<AutomationData>> GetAutomationData(string flowName)
        {
            List<AutomationData> AutomationData = new List<AutomationData>();
            string apiUrl = _settingReader.GetSetting("AppSettings:AutomationAPI");
            
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(apiUrl);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    AttachAuthIfAvailable(httpClient);

                    HttpResponseMessage response = await httpClient.GetAsync($"api/Automation/data/flow/{flowName}");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        AutomationData = !string.IsNullOrEmpty(data) ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<AutomationData>>(data) : null;
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return AutomationData;
        }

        public async Task<bool> UpdateQueue(string Id, string status)
        {
            try
            {

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://localhost:44390");
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await httpClient.GetAsync($"/api/Queue/{Id}/UpdateQueueStatus/{status}");

                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }
    }
}
