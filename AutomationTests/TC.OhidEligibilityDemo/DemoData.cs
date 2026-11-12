using Newtonsoft.Json.Linq;

namespace TC.OhidEligibilityDemo
{
    public static class DemoData
    {
        public const string PnmLoginUrl = "https://ohpnm-e2e.omes.maximus.com/OH_PNM_E2E/Account/Login.aspx";
        public const string PnmUserId = "123";
        public const string OhidUserName = "34277920";

        public const string ProviderMedicaidId = "2422659";

        public const string MedicaidBillingNumber = "103798930699";
        public const string Ssn = "";
        public static readonly DateTime DateOfBirth = new(1960, 10, 21);
        public static readonly DateTime FromDateOfService = new(2026, 1, 1);
        public static readonly DateTime ToDateOfService = new(2026, 10, 14);

        public const string Browser = "Chrome";

        public static string OhidPassword => Secret("OhidPassword");
        public static string TotpKey => Secret("TotpKey");

        private const string SecretsFile = "demo.secrets.json";

        private static string Secret(string name)
        {
            var path = Path.Combine(AppContext.BaseDirectory, SecretsFile);
            if (!File.Exists(path))
                throw new InvalidOperationException(
                    $"{SecretsFile} is missing. Create it in AutomationTests\\TC.OhidEligibilityDemo with {{ \"OhidPassword\": \"...\", \"TotpKey\": \"...\" }} and rebuild.");

            var value = (string?)JObject.Parse(File.ReadAllText(path))[name];
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"\"{name}\" is empty in {SecretsFile}.");
            return value;
        }
    }
}
