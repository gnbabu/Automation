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

        public void SaveTestCaseLog(TestCaseExecutionLog testCaseExecutionLog)
        {
            try
            {
                string apiUrl = _settingReader.GetSetting("AppSettings:AutomationAPI");
                string message = JsonConvert.SerializeObject(testCaseExecutionLog);

                using (var httpClient = new HttpClient())
                {
                    AttachAuthIfAvailable(httpClient);

                    StringContent content = new StringContent(message, Encoding.UTF8, "application/json");

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}api/TestCaseExecutionLogs");

                    requestMessage.Content = content;

                    HttpResponseMessage httpResponseMessage = httpClient.SendAsync(requestMessage).Result;

                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        // Deliberately not throwing here - a failed step-level log upload
                        // shouldn't fail the actual test - but this used to fail silently
                        // with no trace at all (confirmed by direct testing: this ran
                        // unauthenticated against an [Authorize]-protected endpoint for a
                        // long time with nobody noticing). TestContext.WriteLine surfaces
                        // it in the test's own output instead.
                        NUnit.Framework.TestContext.WriteLine(
                            $"SaveTestCaseLog failed: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                NUnit.Framework.TestContext.WriteLine($"SaveTestCaseLog failed: {ex.Message}");
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
                    AttachAuthIfAvailable(httpClient);

                    StringContent content = new StringContent(message, Encoding.UTF8, "application/json");

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}api/TestScreenshots/bulk");

                    requestMessage.Content = content;

                    HttpResponseMessage httpResponseMessage = httpClient.SendAsync(requestMessage).Result;

                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        NUnit.Framework.TestContext.WriteLine(
                            $"SaveMethodScreenShots failed: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                NUnit.Framework.TestContext.WriteLine($"SaveMethodScreenShots failed: {ex.Message}");
            }
        }


        // Resolves the Environment's URL/RequiresAuthentication flag - read by
        // BaseFeatureFixture in OneTimeSetUp using EnvironmentId from TestContext.
        // Parameters (threaded through by TestQueueWorker/NUnitEngineTestRunner, same
        // mechanism as Browser/AccessToken). Returns null on any failure/absence (API
        // unreachable, no EnvironmentId supplied - e.g. a local Test Explorer run outside
        // the queue pipeline) so the caller can fall back to today's hard-coded
        // UserCredentials/LoginService behavior, matching SaveTestCaseLog's pattern of
        // never throwing out of a best-effort call.
        public async Task<EnvironmentDetails?> GetEnvironmentDetails()
        {
            string? environmentId = NUnit.Framework.TestContext.Parameters["EnvironmentId"];
            if (string.IsNullOrWhiteSpace(environmentId))
                return null;

            try
            {
                string apiUrl = _settingReader.GetSetting("AppSettings:AutomationAPI");

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(apiUrl);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    AttachAuthIfAvailable(httpClient);

                    HttpResponseMessage response = await httpClient.GetAsync($"api/Environment/{environmentId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        NUnit.Framework.TestContext.WriteLine(
                            $"GetEnvironmentDetails failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                        return null;
                    }

                    string data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<EnvironmentDetails>(data);
                }
            }
            catch (Exception ex)
            {
                NUnit.Framework.TestContext.WriteLine($"GetEnvironmentDetails failed: {ex.Message}");
                return null;
            }
        }

        // Resolves the specific login user explicitly picked at Run Now/Schedule time
        // (LoginUserId from TestContext.Parameters) - the only call that ever receives a
        // decrypted password, only reachable with the service JWT this process already
        // carries as AccessToken (see LoginUserController.GetCredentials). Returns null
        // on any failure/absence (no LoginUserId supplied - e.g. the environment doesn't
        // require authentication, or a local Test Explorer run) so the caller falls back
        // to today's hard-coded UserCredentials behavior.
        public async Task<LoginUserCredentials?> GetLoginUserCredentials()
        {
            string? loginUserId = NUnit.Framework.TestContext.Parameters["LoginUserId"];
            if (string.IsNullOrWhiteSpace(loginUserId))
                return null;

            try
            {
                string apiUrl = _settingReader.GetSetting("AppSettings:AutomationAPI");

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(apiUrl);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    AttachAuthIfAvailable(httpClient);

                    HttpResponseMessage response = await httpClient.GetAsync($"api/LoginUser/{loginUserId}/credentials");

                    if (!response.IsSuccessStatusCode)
                    {
                        NUnit.Framework.TestContext.WriteLine(
                            $"GetLoginUserCredentials failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                        return null;
                    }

                    string data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LoginUserCredentials>(data);
                }
            }
            catch (Exception ex)
            {
                NUnit.Framework.TestContext.WriteLine($"GetLoginUserCredentials failed: {ex.Message}");
                return null;
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
    }
}
