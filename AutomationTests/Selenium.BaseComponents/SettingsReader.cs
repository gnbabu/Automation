using Microsoft.Extensions.Configuration;

namespace Selenium.BaseComponents
{
    public class SettingsReader
    {
        private readonly IConfiguration _configuration;

        public SettingsReader()
        {
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true).Build();
        }

        public string GetSetting(string key)
        {
            return _configuration[key];
        }
    }
}
