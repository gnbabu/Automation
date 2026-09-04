using Microsoft.Extensions.DependencyInjection;
using Selenium.BaseComponents.Services;
using Selenium.BaseComponents.Utilities;

namespace Selenium.BaseComponents.Configuration
{
    /// <summary>
    /// Service configuration for Dependency Injection
    /// Configures all services for the test framework
    /// </summary>
    public static class ServiceConfig
    {
        /// <summary>
        /// Configures and builds the service provider
        /// </summary>
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register core services as transient (created each time they're requested)
            services.AddTransient<WebDriverService>();
            services.AddTransient<LoginService>();

            // Register API Gateway as singleton (shared across tests)
            services.AddSingleton<APIGatway>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Creates a new service provider instance
        /// </summary>
        public static IServiceProvider CreateServiceProvider()
        {
            return ConfigureServices();
        }
    }
}
